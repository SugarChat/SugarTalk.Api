using SugarTalk.Core.Ioc;

namespace SugarTalk.Core.Services.Http.Clients;

public interface ISmartiesClient : IScopedDependency
{
}

public class SmartiesClient : ISmartiesClient
{
    private readonly ISugarTalkHttpClientFactory _httpClientFactory;

    public SmartiesClient(ISugarTalkHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }
}