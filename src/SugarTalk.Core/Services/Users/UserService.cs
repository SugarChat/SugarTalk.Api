using System;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using SugarTalk.Core.Entities;
using SugarTalk.Messages;
using SugarTalk.Messages.Dtos.Users;
using SugarTalk.Messages.Requests.Users;

namespace SugarTalk.Core.Services.Users
{
    public interface IUserService
    {
        ClaimsPrincipal GetCurrentPrincipal();

        Task<SugarTalkResponse<SignedInUserDto>> SignInFromThirdParty(SignInFromThirdPartyRequest request,
            CancellationToken cancellationToken);
    }
    
    public class UserService : IUserService
    {
        private readonly IMapper _mapper;
        private readonly IHttpContextAccessor _httpContextAccessor;
        
        private readonly IUserDataProvider _userDataProvider;

        public UserService(IMapper mapper, IHttpContextAccessor httpContextAccessor, IUserDataProvider userDataProvider)
        {
            _mapper = mapper;
            _userDataProvider = userDataProvider;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<SugarTalkResponse<SignedInUserDto>> SignInFromThirdParty(SignInFromThirdPartyRequest request, 
            CancellationToken cancellationToken)
        {
            CheckIsAuthenticated();

            var user = await GetOrCreateUser(GetCurrentPrincipal(), cancellationToken)
                .ConfigureAwait(false);
            
            return new SugarTalkResponse<SignedInUserDto>
            {
                Data = _mapper.Map<SignedInUserDto>(user)
            };
        }

        public ClaimsPrincipal GetCurrentPrincipal()
        {
            return _httpContextAccessor.HttpContext.User;
        }

        private async Task<User> GetOrCreateUser(ClaimsPrincipal principal, CancellationToken cancellationToken)
        {
            var thirdPartyId = principal.Claims.Single(x => x.Type == SugarTalkClaimType.ThirdPartyId).Value;

            var user = await _userDataProvider.GetUserByThirdPartyId(thirdPartyId, cancellationToken)
                .ConfigureAwait(false);

            if (user == null)
            {
                user = principal.ToUser();

                await _userDataProvider.PersistUser(user, cancellationToken).ConfigureAwait(false);
            }

            return user;
        }
        
        private void CheckIsAuthenticated()
        {
            var currentPrincipal = GetCurrentPrincipal();

            if (currentPrincipal?.Identity == null || currentPrincipal.Identity.IsAuthenticated)
                throw new UnauthorizedAccessException();
        }
    }
}