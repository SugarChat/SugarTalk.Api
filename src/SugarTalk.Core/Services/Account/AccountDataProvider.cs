using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using SugarTalk.Core.Constants;
using SugarTalk.Core.Data;
using SugarTalk.Core.Domain.Account;
using SugarTalk.Core.Extensions;
using SugarTalk.Core.Ioc;
using SugarTalk.Core.Services.Identity;
using SugarTalk.Messages.Dto.Users;
using SugarTalk.Messages.Enums.Account;
using Role = SugarTalk.Core.Domain.Account.Role;

namespace SugarTalk.Core.Services.Account
{
    public interface IAccountDataProvider : IScopedDependency
    {
        Task<(bool CanLogin, UserAccountDto Account)> AuthenticateAsync(
            string username, string clearTextPassword, CancellationToken cancellationToken);
        
        Task<UserAccount> GetUserByThirdPartyId(string thirdPartyId, CancellationToken cancellationToken);

        Task PersistUser(UserAccount user, CancellationToken cancellationToken);
        
        Task<UserAccountDto> GetUserAccountAsync(
            int? id = null, string username = null, string thirdPartyUserId = null, bool includeRoles = false,
            UserAccountIssuer? issuer = null, CancellationToken cancellationToken = default);

        Task<UserAccount> CreateUserAccountAsync(string userName, string password, string thirdPartyUserId = null,
            UserAccountIssuer authType = UserAccountIssuer.Wiltechs, CancellationToken cancellationToken = default);
        
        List<Claim> GenerateClaimsFromUserAccount(UserAccountDto account);
        
        Task AllocateUserToRoleAsync(int userId, int roleId, CancellationToken cancellationToken);
        
        Task<List<UserAccount>> GetUserAccountsAsync(List<int> userIds, CancellationToken cancellationToken);
        
        Task<List<UserAccount>> GetUserAccountsAsync(int userId, CancellationToken cancellationToken);
        
        Task<UserAccountDto> CheckCurrentLoggedInUser(CancellationToken cancellationToken);
    }
    
    public partial class AccountDataProvider : IAccountDataProvider
    {
        private readonly IMapper _mapper;
        private readonly IRepository _repository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUser _currentUser;

        public AccountDataProvider(IRepository repository, IMapper mapper, IUnitOfWork unitOfWork, ICurrentUser currentUser)
        {
            _mapper = mapper;
            _repository = repository;
            _unitOfWork = unitOfWork;
            _currentUser = currentUser;
        }

        public async Task<UserAccount> GetUserByThirdPartyId(string thirdPartyId, CancellationToken cancellationToken)
        {
            return await _repository.SingleOrDefaultAsync<UserAccount>(x => x.ThirdPartyUserId == thirdPartyId, cancellationToken)
                .ConfigureAwait(false);
        }

        public async Task PersistUser(UserAccount user, CancellationToken cancellationToken)
        {
            await _repository.InsertAsync(user, cancellationToken).ConfigureAwait(false);
        }
        
        public async Task<UserAccountDto> GetUserAccountAsync(
            int? id = null, string username = null, string thirdPartyUserId = null, bool includeRoles = false,
            UserAccountIssuer? issuer = null, CancellationToken cancellationToken = default)
        {
            var query = _repository.QueryNoTracking<UserAccount>();

            if (id.HasValue)
                query = query.Where(x => x.Id == id);
            
            if (!string.IsNullOrEmpty(username))
                query = query.Where(x => x.UserName == username);
            
            if (thirdPartyUserId != null)
                query = query.Where(x => x.ThirdPartyUserId == thirdPartyUserId);

            if (issuer.HasValue)
                query = query.Where(x => x.Issuer == issuer.Value);
        
            var account = await query
                .ProjectTo<UserAccountDto>(_mapper.ConfigurationProvider)
                .FirstOrDefaultAsync(cancellationToken).ConfigureAwait(false);

            if (account == null || !includeRoles) return account;
            {
                var userRoleIds = await _repository
                    .QueryNoTracking<RoleUser>().Where(x => x.UserId == account.Id)
                    .Select(x => x.RoleId).ToListAsync(cancellationToken).ConfigureAwait(false);

                if (userRoleIds.Any())
                {
                    account.Roles = await _repository
                        .QueryNoTracking<Role>()
                        .Where(x => userRoleIds.Contains(x.Id))
                        .ProjectTo<RoleDto>(_mapper.ConfigurationProvider)
                        .ToListAsync(cancellationToken).ConfigureAwait(false);
                }
            }

            return account;
        }
        
        public async Task<UserAccount> CreateUserAccountAsync(string requestUserName, string requestPassword, 
            string thirdPartyUserId = null, UserAccountIssuer authType = UserAccountIssuer.Wiltechs, CancellationToken cancellationToken = default)
        {
            var userAccount = new UserAccount
            {
                CreatedOn = DateTime.Now,
                ModifiedOn = DateTime.Now,
                Uuid = Guid.NewGuid(),
                UserName = requestUserName,
                Password = requestPassword?.ToSha256(),
                ThirdPartyUserId = thirdPartyUserId,
                Issuer = authType,
                IsActive = true
            };

            await _repository.InsertAsync(userAccount, cancellationToken).ConfigureAwait(false);
            
            await _unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            
            return userAccount;
        }

        public List<Claim> GenerateClaimsFromUserAccount(UserAccountDto account)
        {
            var claims = new List<Claim>
            {
                new(ClaimTypes.Name, account.UserName),
                new(ClaimTypes.NameIdentifier, account.Id.ToString()),
                new(ClaimTypes.Authentication, AuthenticationSchemeConstants.SelfAuthenticationScheme)
            };
            claims.AddRange(account.Roles.Select(r => new Claim(ClaimTypes.Role, r.Name)));
            return claims;
        }

        public async Task AllocateUserToRoleAsync(int userId, int roleId, CancellationToken cancellationToken)
        {
            var user = await GetUserAccountAsync(userId, includeRoles: true, cancellationToken: cancellationToken).ConfigureAwait(false);
        
            if (user == null) return;
        
            var userRole = 
                await _repository.SingleOrDefaultAsync<RoleUser>(x => 
                    x.UserId == user.Id && x.RoleId == roleId, cancellationToken).ConfigureAwait(false);

            if (userRole != null) return;
            {
                await _repository.InsertAsync(new RoleUser
                {
                    RoleId = roleId,
                    UserId = user.Id,
                    Uuid = Guid.NewGuid()
                }, cancellationToken).ConfigureAwait(false);
            }
        }

        public async Task<List<UserAccount>> GetUserAccountsAsync(List<int> userIds, CancellationToken cancellationToken)
        {
            if (userIds is not { Count: > 0 }) return new List<UserAccount>();

            return await _repository.QueryNoTracking<UserAccount>()
                .Where(x => userIds.Contains(x.Id)).ToListAsync(cancellationToken).ConfigureAwait(false);
        }

        public async Task<List<UserAccount>> GetUserAccountsAsync(int userId, CancellationToken cancellationToken)
        {
            var user = await _repository.QueryNoTracking<UserAccount>()
                .Where(x => userId == x.Id).FirstOrDefaultAsync(cancellationToken).ConfigureAwait(false);
            
            if (user is null) return new List<UserAccount>();

            return await _repository.QueryNoTracking<UserAccount>()
                .Where(x => x.UserName.ToUpper().Contains(user.UserName.ToUpper()))
                .ToListAsync(cancellationToken).ConfigureAwait(false);
        }

        public async Task<UserAccountDto> CheckCurrentLoggedInUser(CancellationToken cancellationToken)
        {
            var currentUser = await GetUserAccountAsync(_currentUser.Id.Value, cancellationToken: cancellationToken).ConfigureAwait(false);

            if (currentUser is null) throw new UnauthorizedAccessException();

            return currentUser;
        }
    }
}