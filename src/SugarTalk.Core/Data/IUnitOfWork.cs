using System.Threading;
using System.Threading.Tasks;

namespace SugarTalk.Core.Data;

public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    bool ShouldSaveChanges { get; set; }
}