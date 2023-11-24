using SugarTalk.Core.Ioc;
using SugarTalk.Core.Services.Http.Clients;

namespace SugarTalk.Core.Services.Smarties;

public interface ISmartiesService : IScopedDependency
{
}

public class SmartiesService : ISmartiesService
{
    private readonly ISmartiesClient _smartiesClient;

    public SmartiesService(ISmartiesClient smartiesClient)
    {
        _smartiesClient = smartiesClient;
    }
}
