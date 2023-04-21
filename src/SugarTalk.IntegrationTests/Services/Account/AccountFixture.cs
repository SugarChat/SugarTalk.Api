using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Mediator.Net;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Shouldly;
using SugarTalk.Core.Data;
using SugarTalk.Core.Domain.Account;
using SugarTalk.Core.Domain.Account.Exceptions;
using SugarTalk.Core.Extensions;
using SugarTalk.Core.Services.Account;
using SugarTalk.Core.Settings.Authentication;
using SugarTalk.IntegrationTests.TestBaseClasses;
using SugarTalk.IntegrationTests.Utils.Account;
using SugarTalk.Messages.Enums.Account;
using SugarTalk.Messages.Requests.Account;
using Xunit;

namespace SugarTalk.IntegrationTests.Services.Account;

public class AccountFixture : AccountFixtureBase
{
    private readonly AccountUtil _accountUtil;

    public AccountFixture()
    {
        _accountUtil = new AccountUtil(CurrentScope);
    }

    [Fact]
    public void ShouldSha256Correct()
    {
        var clearText = "ece18047-239b-4309-b52d-472d9d2dfc15";
        
        clearText.ToSha256().ToUpper().ShouldBe("a3b1282868e2797613d4b647febd6b5c8b4f28db1d0d7c195765a31f0dd5f765".ToUpper());
    }

    [Theory]
    [InlineData("admin", "123456", true, true)]
    [InlineData("admin", "123456", false, false)]
    [InlineData("admin", "1234567", true, false)]
    [InlineData("admin1", "123456", true, false)]
    public async Task CanLogin(string username, string password, bool isActive, bool canLogin)
    {
        await _accountUtil.AddUserAccount("admin", "123456", isActive: isActive);

        await Run<IMediator>(async mediator =>
        {
            var response = await mediator.RequestAsync<LoginRequest, LoginResponse>(new LoginRequest
            {
                UserName = username,
                Password = password
            });

            if (canLogin)
                response.Data.ShouldNotBeEmpty();
            else
                response.Data.ShouldBeNull();
        });
    }

    [Fact]
    public async Task TokenProvideAfterLoginShouldBeValidated()
    {
        await _accountUtil.AddUserAccount("admin", "123456", isActive: true);
        
        var token = await Run<IMediator, string>(async mediator =>
        {
            var response = await mediator.RequestAsync<LoginRequest, LoginResponse>(new LoginRequest
            {
                UserName = "admin",
                Password = "123456"
            });

            return response.Data;
        });

        token.ShouldNotBeEmpty();
        
        var validationParameters = new TokenValidationParameters
        {
            ValidateLifetime = false,
            ValidateAudience = false,
            ValidateIssuer = false,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey =
                new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(new JwtSymmetricKeySetting(CurrentConfiguration).Value
                        .PadRight(256 / 8, '\0')))
        };
        
        new JwtSecurityTokenHandler().ValidateToken(token, validationParameters, out var validToken);

        validToken.ShouldNotBeNull();
    }

    [Fact]
    public async Task CanRegisterUserAccount()
    {
        var userName = "bans";
        var passWord = "123456";
        
        await _accountUtil.RegisterUserAccount(userName, passWord);
        
        var userAccounts = await Run<IRepository, List<UserAccount>>(async repository =>
            await repository.Query<UserAccount>().ToListAsync().ConfigureAwait(false));

        var userAccount = userAccounts.FirstOrDefault(x => x.UserName == userName);
        
        userAccount.ShouldNotBeNull();
        userAccount.UserName.ShouldBe(userName);
        userAccount.Password.ShouldBe(passWord.ToSha256());
        userAccount.IsActive.ShouldBe(true);
        
        await Run<IMediator>(async mediator =>
        {
            var response = await mediator.RequestAsync<LoginRequest, LoginResponse>(new LoginRequest
            {
                UserName = userName,
                Password = passWord
            });

            response.Data.ShouldNotBeEmpty();
        });

        await MarkUserAccountAsUnActivate(userAccount);
        
        await Run<IMediator>(async mediator =>
        {
            var response = await mediator.RequestAsync<LoginRequest, LoginResponse>(new LoginRequest
            {
                UserName = userName,
                Password = passWord
            });
        
            response.Data.ShouldBeNull();
        });
    }
    
    [Fact]
    public async Task CannotRegisterUserAccountWhenExistTheSameUsername()
    {
        var userName = "bans";
        var passWord = "123456";
        
        await _accountUtil.AddUserAccount("bans", "123456", isActive: true);
        
        await Assert.ThrowsAsync<CannotRegisterWhenExistTheSameUserAccountException>(async () =>
        {
            await _accountUtil.RegisterUserAccount(userName, passWord);
        });

        var userAccounts = await Run<IRepository, List<UserAccount>>(
            async repository =>
                await repository.Query<UserAccount>().Where(x => 
                    x.UserName == userName).ToListAsync().ConfigureAwait(false));

        userAccounts.ShouldNotBeNull();
        userAccounts.Count.ShouldBe(1);
        
        var userAccount = userAccounts.Single();
        
        userAccount.UserName = "mars";

        await RunWithUnitOfWork<IRepository>(async repository => 
            await repository.UpdateAsync(userAccount, CancellationToken.None));

        await _accountUtil.RegisterUserAccount(userName, passWord);
        
        userAccounts = await Run<IRepository, List<UserAccount>>(async repository => 
            await repository.Query<UserAccount>().ToListAsync().ConfigureAwait(false));

        var userAccountNames = userAccounts.Select(x => x.UserName).ToList();
        
        userAccounts.ShouldNotBeNull();
        userAccounts.Count.ShouldBe(3);
        
        userAccountNames.ShouldContain("bans");
        userAccountNames.ShouldContain("mars");
    }

    [Fact]
    public async Task CanGetOrCreateUserAccountFromThirdPartyAsync()
    {
        var userId = Guid.NewGuid().ToString();

        await Run<IAccountService>(async service =>
        {
            var beforeUserAccount = await service.GetOrCreateUserAccountFromThirdPartyAsync(userId, "test name",
                CancellationToken.None).ConfigureAwait(false);

            beforeUserAccount.ShouldNotBeNull();
            beforeUserAccount.UserName.ShouldBe("test name");
            beforeUserAccount.ThirdPartyUserId.ShouldBe(userId);
            beforeUserAccount.Issuer.ShouldBe(UserAccountIssuer.Wiltechs);
            
            var afterUserAccount = await service.GetOrCreateUserAccountFromThirdPartyAsync(userId, "test name",
                CancellationToken.None).ConfigureAwait(false);

            afterUserAccount.ShouldNotBeNull();
            afterUserAccount.Id.ShouldBe(beforeUserAccount.Id);
            afterUserAccount.Issuer.ShouldBe(beforeUserAccount.Issuer);
            afterUserAccount.UserName.ShouldBe(beforeUserAccount.UserName);
            afterUserAccount.ThirdPartyUserId.ShouldBe(beforeUserAccount.ThirdPartyUserId);
        });
    }

    private async Task MarkUserAccountAsUnActivate(UserAccount userAccount)
    {
        userAccount.IsActive = false;
        
        await RunWithUnitOfWork<IRepository>(async repository => 
            await repository.UpdateAsync(userAccount).ConfigureAwait(false));
    }
}