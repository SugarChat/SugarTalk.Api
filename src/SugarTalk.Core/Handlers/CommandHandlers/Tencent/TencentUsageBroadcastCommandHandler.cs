using System.Threading;
using System.Threading.Tasks;
using Mediator.Net.Context;
using Mediator.Net.Contracts;
using SugarTalk.Core.Services.Tencent;
using SugarTalk.Messages.Commands.Tencent;

namespace SugarTalk.Core.Handlers.CommandHandlers.Tencent;

public class TencentUsageBroadcastCommandHandler : ICommandHandler<TencentUsageBroadcastCommand>
{
    private readonly ITencentService _tencentService;

    public TencentUsageBroadcastCommandHandler(ITencentService tencentService)
    {
        _tencentService = tencentService;
    }
    
    public async Task Handle(IReceiveContext<TencentUsageBroadcastCommand> context, CancellationToken cancellationToken)
    {
        await _tencentService.TencentUsageBroadcastAsync(context.Message, cancellationToken).ConfigureAwait(false);
    }
}