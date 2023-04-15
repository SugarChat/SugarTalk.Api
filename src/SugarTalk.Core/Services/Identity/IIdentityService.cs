using System.Threading;
using System.Threading.Tasks;
using SugarTalk.Core.Ioc;

namespace SugarTalk.Core.Services.Identity;

public interface IIdentityService : IScopedDependency
{
    Task<bool> IsInRolesAsync(int userId, string[] rolesArray, CancellationToken cancellationToken);
}
