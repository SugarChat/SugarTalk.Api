using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Serilog;
using AutoMapper;
using SugarTalk.Core.Ioc;
using SugarTalk.Core.Domain.Account;
using SugarTalk.Core.Services.Aliyun;
using SugarTalk.Core.Services.Identity;
using SugarTalk.Messages.Commands.Account;
using SugarTalk.Messages.Dto.Users;
using SugarTalk.Core.Services.Caching;
using SugarTalk.Messages.Enums.Account;
using SugarTalk.Messages.Events.Account;
using SugarTalk.Messages.Requests.Account;
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

        Task<UploadPhotoResponse> UploadPhotoAsync(UploadPhotoCommand command, CancellationToken cancellationToken);
    }
    
    public class AccountService : IAccountService
    {
        private readonly IMapper _mapper;
        private readonly ITokenProvider _tokenProvider;
        private readonly IIdentityService _identityService;
        private readonly IAliYunOssService _aliYunOssService;
        private readonly IRedisSafeRunner _redisSafeRunner;
        private readonly IAccountDataProvider _accountDataProvider;
        private readonly ICurrentUser _currentUser;
        
        public AccountService(IMapper mapper, IIdentityService identityService, IAccountDataProvider accountDataProvider, ITokenProvider tokenProvider, IAliYunOssService aliYunOssService, ICurrentUser currentUser, IRedisSafeRunner redisSafeRunner)
        {
            _mapper = mapper;
            _tokenProvider = tokenProvider;
            _identityService = identityService;
            _aliYunOssService = aliYunOssService;
            _accountDataProvider = accountDataProvider;
            _currentUser = currentUser;
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

        public async Task<UploadPhotoResponse> UploadPhotoAsync(UploadPhotoCommand command, CancellationToken cancellationToken)
        {
            if (command.FileName == null || command.FileContent == null || _currentUser.Id == null)
                return null;

            try
            {
                _aliYunOssService.UploadFile(command.FileName, command.FileContent);

                var url = _aliYunOssService.GetFileUrl(command.FileName);

                var userAccountProfile = await _accountDataProvider.GetUserAccountProfileAsync(_currentUser.Id.Value, cancellationToken).ConfigureAwait(false);

                if (userAccountProfile != null)
                    await _accountDataProvider.DeleteUserAccountProfileAsync(userAccountProfile, cancellationToken).ConfigureAwait(false);
                        
                await _accountDataProvider.AddUserAccountProfileAsync(new UserAccountProfile
                {
                    Url = url,
                    UserAccountId = _currentUser.Id.Value
                }, cancellationToken).ConfigureAwait(false);
                
                return new UploadPhotoResponse { Url = url };
            }
            catch (Exception e)
            {
                Log.Information("Upload user photo failed: {@e}", e);
                throw;
            }
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