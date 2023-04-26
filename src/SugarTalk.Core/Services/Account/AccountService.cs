using System;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using SugarTalk.Core.Domain.Account;
using SugarTalk.Core.Ioc;
using SugarTalk.Core.Services.Identity;
using SugarTalk.Messages;
using SugarTalk.Messages.Commands.Account;
using SugarTalk.Messages.Dtos.Users;
using SugarTalk.Messages.Enums.Account;
using SugarTalk.Messages.Events.Account;
using SugarTalk.Messages.Requests.Account;

namespace SugarTalk.Core.Services.Account
{
    public interface IAccountService : IScopedDependency
    {
        ClaimsPrincipal GetCurrentPrincipal();

        Task<LoginResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken);
        
        Task<UserAccount> GetCurrentLoggedInUser(CancellationToken cancellationToken = default);
        
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

        public async Task<UserAccount> GetCurrentLoggedInUser(CancellationToken cancellationToken = default)
        {
            var thirdPartyId = GetCurrentPrincipal().Claims.Single(x => x.Type == SugarTalkConstants.ThirdPartyId).Value;

            return await _accountDataProvider.GetUserByThirdPartyId(thirdPartyId, cancellationToken)
                .ConfigureAwait(false);
        }
        
        public async Task<UserAccountDto> GetCurrentUserAsync(CancellationToken cancellationToken)
        {
            return await _accountDataProvider
                .GetUserAccountAsync(id: _currentUser.Id, includeRoles: true, cancellationToken: cancellationToken).ConfigureAwait(false);
        }
        
        public ClaimsPrincipal GetCurrentPrincipal()
        {
            return _httpContextAccessor.HttpContext.User;
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
        
        private async Task<UserAccount> GetOrCreateUser(ClaimsPrincipal principal, CancellationToken cancellationToken)
        {
            var thirdPartyId = principal.Claims.Single(x => x.Type == SugarTalkConstants.ThirdPartyId).Value;

            var user = await _accountDataProvider.GetUserByThirdPartyId(thirdPartyId, cancellationToken)
                .ConfigureAwait(false);

            if (user == null)
            {
                user = principal.ToUser();

                await _accountDataProvider.PersistUser(user, cancellationToken).ConfigureAwait(false);
            }

            return user;
        }
        
        private void CheckIsAuthenticated()
        {
            var currentPrincipal = GetCurrentPrincipal();

            if (currentPrincipal?.Identity == null || !currentPrincipal.Identity.IsAuthenticated)
                throw new UnauthorizedAccessException();
        }
    }
}