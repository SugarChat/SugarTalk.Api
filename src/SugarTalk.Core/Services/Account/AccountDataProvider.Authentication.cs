using System.Threading;
using System.Threading.Tasks;
using SugarTalk.Core.Domain.Account;
using SugarTalk.Core.Extensions;
using SugarTalk.Messages.Dto.Users;

namespace SugarTalk.Core.Services.Account;

public partial class AccountDataProvider
{
    public async Task<(bool CanLogin, UserAccountDto Account)> AuthenticateAsync(
        string username, string clearTextPassword, CancellationToken cancellationToken)
    {
        var hashPassword = clearTextPassword.ToSha256();

        var canLogin = await _repository
            .AnyAsync<UserAccount>(x => x.UserName == username && x.Password == hashPassword && x.IsActive,
                cancellationToken).ConfigureAwait(false);

        if (!canLogin) 
            return (false, null);
        
        var account = await GetUserAccountAsync(username: username, includeRoles: true,
            cancellationToken: cancellationToken).ConfigureAwait(false);
        
        return (true, account);
    }
}