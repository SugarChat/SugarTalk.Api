using System.Threading;
using System.Threading.Tasks;
using SugarTalk.Core.Ioc;
using SugarTalk.Messages.Dto.WeChat;

namespace SugarTalk.Core.Services.Http.Clients;

public interface IWeChatClient : IScopedDependency
{
    Task<WorkWeChatResponseDto> SendWorkWechatRobotMessagesAsync(string requestUrl, SendWorkWechatGroupRobotMessageDto messages, CancellationToken cancellationToken);
}

public class WeChatClient : IWeChatClient
{
    private readonly ISugarTalkHttpClientFactory _httpClientFactory;

    public WeChatClient(ISugarTalkHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }
    
    public async Task<WorkWeChatResponseDto> SendWorkWechatRobotMessagesAsync(
        string requestUrl, SendWorkWechatGroupRobotMessageDto messages, CancellationToken cancellationToken)
    {
        return await _httpClientFactory.PostAsJsonAsync<WorkWeChatResponseDto>(requestUrl, messages, cancellationToken).ConfigureAwait(false);
    }
}