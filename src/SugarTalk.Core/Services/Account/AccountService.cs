using System;
using Serilog;
using System.Net;
using AutoMapper;
using System.Threading;
using SugarTalk.Core.Ioc;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using SugarTalk.Messages.Dto.Users;
using SugarTalk.Core.Services.Caching;
using SugarTalk.Core.Services.Identity;
using SugarTalk.Messages.Enums.Account;
using SugarTalk.Messages.Events.Account;
using SugarTalk.Messages.Requests.Account;
using SugarTalk.Messages.Commands.Account;
using SugarTalk.Core.Domain.Account.Exceptions;

namespace SugarTalk.Core.Services.Account
{
    public interface IAccountService : IScopedDependency
    {
        Task<LoginResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken);
        
        Task<GetCurrentUserResponse> GetCurrentUserAsync(GetCurrentUserRequest request, CancellationToken cancellationToken);
        
        Task<UserAccountRegisteredEvent> RegisterAsync(RegisterCommand command, CancellationToken cancellationToken);
        
        Task<UserAccountDto> GetOrCreateUserAccountFromThirdPartyAsync(string userId, string userName, UserAccountIssuer issuer, CancellationToken cancellationToken);

        Task<UserAccountDto> GetOrCreateGuestUserAccountAsync(string userName, CancellationToken cancellationToken);
    }
    
    public class AccountService : IAccountService
    {
        private readonly IMapper _mapper;
        private readonly ITokenProvider _tokenProvider;
        private readonly IIdentityService _identityService;
        private readonly IRedisSafeRunner _redisSafeRunner;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IAccountDataProvider _accountDataProvider;

        public AccountService(IMapper mapper, IIdentityService identityService, IRedisSafeRunner redisSafeRunner, IHttpContextAccessor httpContextAccessor, IAccountDataProvider accountDataProvider, ITokenProvider tokenProvider)
        {
            _mapper = mapper;
            _tokenProvider = tokenProvider;
            _identityService = identityService;
            _accountDataProvider = accountDataProvider;
            _httpContextAccessor = httpContextAccessor;
            _redisSafeRunner = redisSafeRunner;
        }
        
        public async Task<LoginResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken)
        {
            var (canLogin, account) = await _accountDataProvider
                .AuthenticateAsync(request.UserName, request.Password, cancellationToken).ConfigureAwait(false);
            
            Log.Information("canLogin:{canLogin}, account:{account}", canLogin, account);

            if (!canLogin)
                return new LoginResponse { Code = HttpStatusCode.Unauthorized };
        
            return new LoginResponse
            {
                Data = _tokenProvider.Generate(_accountDataProvider.GenerateClaimsFromUserAccount(account))
            };
        }
        
        public async Task<GetCurrentUserResponse> GetCurrentUserAsync(GetCurrentUserRequest request, CancellationToken cancellationToken)
        {
            return new GetCurrentUserResponse
            {
                Data = await _identityService.GetCurrentUserAsync(cancellationToken: cancellationToken).ConfigureAwait(false)
            };
        }

        public async Task<UserAccountDto> GetOrCreateUserAccountFromThirdPartyAsync(string userId, string userName, UserAccountIssuer issuer, CancellationToken cancellationToken)
        {
            return await _redisSafeRunner.ExecuteWithLockAsync($"{userName}-{userId}", async () =>
            {
                var userAccount = await _accountDataProvider.GetUserAccountAsync(thirdPartyUserId: userId, includeRoles: true, cancellationToken: cancellationToken).ConfigureAwait(false);

                if (userAccount != null) return userAccount;
                
                var account = await _accountDataProvider
                    .CreateUserAccountAsync(userName, "123abc", userId, issuer, cancellationToken).ConfigureAwait(false);
                
                return _mapper.Map<UserAccountDto>(account);
            }, wait: TimeSpan.FromSeconds(10), retry: TimeSpan.FromSeconds(1)).ConfigureAwait(false);
        }

        public async Task<UserAccountDto> GetOrCreateGuestUserAccountAsync(string userName, CancellationToken cancellationToken)
        {
            var userAccount = await _accountDataProvider.GetUserAccountAsync(
                username: userName, includeRoles: true, issuer: UserAccountIssuer.Guest, cancellationToken: cancellationToken).ConfigureAwait(false);
            
            if (userAccount != null) return userAccount;

          
            var account = await _accountDataProvider
                .CreateUserAccountAsync(userName, string.Empty, authType: UserAccountIssuer.Guest, cancellationToken: cancellationToken).ConfigureAwait(false);
             
            return _mapper.Map<UserAccountDto>(account);
        }

        public async Task<UserAccountRegisteredEvent> RegisterAsync(RegisterCommand command, CancellationToken cancellationToken)
        {
            await CheckCanRegisterAsync(command, cancellationToken).ConfigureAwait(false);
            
            await _accountDataProvider.CreateUserAccountAsync(command.UserName, command.Password, cancellationToken: cancellationToken);

            return new UserAccountRegisteredEvent();
        }
        
        private async Task CheckCanRegisterAsync(RegisterCommand command, CancellationToken cancellationToken)
        {
            var userAccount = await _accountDataProvider
                .GetUserAccountAsync(username: command.UserName, includeRoles: false, cancellationToken: cancellationToken).ConfigureAwait(false);

            if (userAccount != null)
                throw new CannotRegisterWhenExistTheSameUserAccountException();
        }
    }
}