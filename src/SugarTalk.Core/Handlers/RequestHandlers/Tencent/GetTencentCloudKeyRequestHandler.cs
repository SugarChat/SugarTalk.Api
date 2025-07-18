using System.Threading;
using Mediator.Net.Context;
using Mediator.Net.Contracts;
using System.Threading.Tasks;
using SugarTalk.Core.Services.Tencent;
using SugarTalk.Messages.Requests.Tencent;

namespace SugarTalk.Core.Handlers.RequestHandlers.Tencent;

public class GetTencentCloudKeyRequestHandler : IRequestHandler<GetTencentCloudKeyRequest, GetTencentCloudKeyResponse>
{
    private readonly ITencentService _tencentService;

    public GetTencentCloudKeyRequestHandler(ITencentService tencentService)
    {
        _tencentService = tencentService;
    }

    public Task<GetTencentCloudKeyResponse> Handle(IReceiveContext<GetTencentCloudKeyRequest> context, CancellationToken cancellationToken)
    {
        return Task.FromResult(_tencentService.GetTencentCloudKey(context.Message));
    }
}