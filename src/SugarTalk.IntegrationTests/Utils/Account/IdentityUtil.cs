using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Autofac;
using Microsoft.AspNetCore.Http;
using SugarTalk.Core.Data;
using SugarTalk.Core.Domain.Account;
using SugarTalk.Core.Services.Users;
using SugarTalk.Messages;
using SugarTalk.Messages.Requests.Users;

namespace SugarTalk.IntegrationTests.Utils.Account;

public class IdentityUtil : TestUtil
{
    public IdentityUtil(ILifetimeScope scope) : base(scope)
    {
    }

    public async Task Signin(TestCurrentUser user)
    {
        await RunWithUnitOfWork<IHttpContextAccessor, IRepository>(async (accessor, repository) =>
        {
            await repository.InsertAsync(new User
            {
                Id = user.Id,
                ThirdPartyId = user.ThirdPartyId,
                ThirdPartyFrom = user.AuthType,
                DisplayName = user.DisplayName,
                Email = user.Email,
                Picture = user.Picture
            });

            if (accessor.HttpContext == null)
                throw new ApplicationException("HttpContext is not available");

            accessor.HttpContext.User = new ClaimsPrincipal(new ClaimsIdentity(new List<Claim>
            {
                new(ClaimTypes.Name, user.DisplayName),
                new(ClaimTypes.Email, user.Email),
                new(SugarTalkConstants.Picture, user.Picture),
                new(SugarTalkConstants.ThirdPartyId, user.ThirdPartyId),
                new(SugarTalkConstants.ThirdPartyFrom, user.AuthType.ToString())
            }, user.AuthType.ToString()));
        });
        
        await Run<IUserService>(async userService =>
        {
            await Task.Run(async () =>
            {
                await userService.SignInFromThirdParty(new SignInFromThirdPartyRequest(), default);
            });
        });
    }
}