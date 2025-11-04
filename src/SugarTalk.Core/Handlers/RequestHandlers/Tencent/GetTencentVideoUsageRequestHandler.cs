using System.Threading;
using Mediator.Net.Context;
using Mediator.Net.Contracts;
using System.Threading.Tasks;
using SugarTalk.Core.Services.Http.Clients;
using SugarTalk.Messages.Requests.Tencent;

namespace SugarTalk.Core.Handlers.RequestHandlers.Tencent;

public class GetTencentVideoUsageRequestHandler : IRequestHandler<GetTencentVideoUsageRequest, GetTencentVideoUsageResponse>
{
    private readonly ITencentClient _tencentClient;

    public GetTencentVideoUsageRequestHandler(ITencentClient tencentClient)
    {
        _tencentClient = tencentClient;
    }

    public async Task<GetTencentVideoUsageResponse> Handle(IReceiveContext<GetTencentVideoUsageRequest> context, CancellationToken cancellationToken)
    {
        return await _tencentClient.GetVideoUsageAsync(context.Message, cancellationToken).ConfigureAwait(false);
    }
}