using System.Net;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using SugarTalk.Core.Ioc;
using SugarTalk.Core.Services.Identity;
using SugarTalk.Messages.Commands.Account;
using SugarTalk.Messages.Dto.Users;
using SugarTalk.Messages.Enums.Account;
using SugarTalk.Messages.Events.Account;
using SugarTalk.Messages.Requests.Account;

namespace SugarTalk.Core.Services.Account
{
    public interface IAccountService : IScopedDependency
    {
        Task<LoginResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken);
        
        Task<UserAccountDto> GetCurrentUserAsync(CancellationToken cancellationToken);
        
        Task<UserAccountRegisteredEvent> RegisterAsync(RegisterCommand command, CancellationToken cancellationToken);
        
        Task<UserAccountDto> GetOrCreateUserAccountFromThirdPartyAsync(string userId, string userName, CancellationToken cancellationToken);
    }
    
    public class AccountService : IAccountService
    {
        private readonly IMapper _mapper;
        private readonly ICurrentUser _currentUser;
        private readonly ITokenProvider _tokenProvider;
        private readonly IHttpContextAccessor _httpContextAccessor;
        
        private readonly IAccountDataProvider _accountDataProvider;

        public AccountService(IMapper mapper, ICurrentUser currentUser, IHttpContextAccessor httpContextAccessor, IAccountDataProvider accountDataProvider, ITokenProvider tokenProvider)
        {
            _mapper = mapper;
            _currentUser = currentUser;
            _tokenProvider = tokenProvider;
            _accountDataProvider = accountDataProvider;
            _httpContextAccessor = httpContextAccessor;
        }
        
        public async Task<LoginResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken)
        {
            var (canLogin, account) = await _accountDataProvider
                .AuthenticateAsync(request.UserName, request.Password, cancellationToken).ConfigureAwait(false);

            if (!canLogin)
                return new LoginResponse { Code = HttpStatusCode.Unauthorized };
        
            return new LoginResponse
            {
                Data = _tokenProvider.Generate(_accountDataProvider.GenerateClaimsFromUserAccount(account))
            };
        }
        
        public async Task<UserAccountDto> GetCurrentUserAsync(CancellationToken cancellationToken)
        {
            return await _accountDataProvider
                .GetUserAccountAsync(id: _currentUser.Id, includeRoles: true, cancellationToken: cancellationToken).ConfigureAwait(false);
        }

        public async Task<UserAccountDto> GetOrCreateUserAccountFromThirdPartyAsync(string userId, string userName, CancellationToken cancellationToken)
        {
            var userAccount = await _accountDataProvider.GetUserAccountAsync(thirdPartyUserId: userId, includeRoles: true, cancellationToken: cancellationToken).ConfigureAwait(false);

            if (userAccount != null) return userAccount;

            var account = await _accountDataProvider
                .CreateUserAccountAsync(userName, null, userId, UserAccountIssuer.Wiltechs, cancellationToken).ConfigureAwait(false);

            return _mapper.Map<UserAccountDto>(account);
        }

        public async Task<UserAccountRegisteredEvent> RegisterAsync(RegisterCommand command, CancellationToken cancellationToken)
        {
            await _accountDataProvider.CreateUserAccountAsync(command.UserName, command.Password, cancellationToken: cancellationToken);

            return new UserAccountRegisteredEvent();
        }
    }
}