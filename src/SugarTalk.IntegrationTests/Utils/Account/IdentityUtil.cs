using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using Microsoft.AspNetCore.Http;
using SugarTalk.Core.Data;
using SugarTalk.Core.Domain.Account;
using SugarTalk.Core.Services.Account;
using SugarTalk.Core.Services.Identity;
using SugarTalk.Messages;

namespace SugarTalk.IntegrationTests.Utils.Account;

public class IdentityUtil : TestUtil
{
    public IdentityUtil(ILifetimeScope scope) : base(scope)
    {
    }

    public async Task CreateUser(TestCurrentUser testUser)
    {
        await RunWithUnitOfWork<IHttpContextAccessor, IRepository>(async (accessor, repository) =>
        {
            await repository.InsertAsync(new UserAccount
            {
                Id = testUser.Id,
                UserName = testUser.UserName,
                Uuid = new Guid("c2af213e-df6e-11ed-b5ea-0242ac120002"),
                Password = "123456",
                ThirdPartyUserId = testUser.ThirdPartyId
            });
            
            if (accessor.HttpContext == null)
                throw new ApplicationException("HttpContext is not available");

            accessor.HttpContext.User = new ClaimsPrincipal(new ClaimsIdentity(new List<Claim>
            {
                new(ClaimTypes.Name, testUser.UserName),
                new(SugarTalkConstants.ThirdPartyId, testUser.ThirdPartyId),
            }, testUser.AuthType.ToString()));
        });
    }
    
    public void SwitchUser(ContainerBuilder builder, TestCurrentUser signUser)
    {
        builder.RegisterInstance(signUser).As<ICurrentUser>();
    }
    
    public async Task InsertRolesAsync(params string[] roleNames)
    {
        await RunWithUnitOfWork<IRepository>(async repository =>
        {
            foreach (var roleName in roleNames)
            {
                await repository.InsertAsync(
                    new Role
                    {
                        Name = roleName, Uuid = Guid.NewGuid()
                    }, CancellationToken.None).ConfigureAwait(false);
            }
        });
    }

    public async Task GrantRolesToUserAsync(int userId, params string[] roleNames)
    {
        await RunWithUnitOfWork<IAccountDataProvider, IRepository>(async (accountDataProvider, repository) =>
        {
            foreach (var roleName in roleNames)
            {
                var role = await repository.SingleOrDefaultAsync<Role>(x => x.Name == roleName);
                if (role != null)
                {
                    await accountDataProvider.AllocateUserToRoleAsync(userId, role.Id, CancellationToken.None).ConfigureAwait(false);
                }
            }
        });
    }
}