using System.Threading.Tasks;
using Autofac;
using Mediator.Net;
using SugarTalk.Core.Data;
using SugarTalk.Core.Domain.Account;
using SugarTalk.Core.Extensions;
using SugarTalk.Messages.Commands.Account;
using SugarTalk.Messages.Enums.Account;

namespace SugarTalk.IntegrationTests.Utils.Account;

public class AccountUtil : TestUtil
{
    public AccountUtil(ILifetimeScope scope) : base(scope)
    {
    }
    
    public async Task<UserAccount> AddUserAccount(string userName, string password, bool isActive = true, UserAccountIssuer issuer = UserAccountIssuer.Wiltechs)
    {
        return await RunWithUnitOfWork<IRepository, UserAccount>(async repository =>
        {
            var account = new UserAccount
            {
                UserName = userName,
                Password = password.ToSha256(),
                IsActive = isActive,
                Issuer = issuer
            };
            await repository.InsertAsync(account);
            return account;
        });
    }

    public async Task RegisterUserAccount(string username, string password)
    {
        await Run<IMediator>(async mediator =>
        {
            await mediator.SendAsync<RegisterCommand, RegisterResponse>(new RegisterCommand
            {
                UserName = username,
                Password = password
            });
        });
    }
}