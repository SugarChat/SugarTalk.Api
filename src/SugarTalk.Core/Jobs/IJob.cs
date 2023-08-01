using SugarTalk.Core.Ioc;
using System.Threading.Tasks;

namespace SugarTalk.Core.Jobs;

public interface IJob : IScopedDependency
{
    Task Execute();
    
    string JobId { get; }
}