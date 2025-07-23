using System.Threading;
using System.Threading.Tasks;
using Mediator.Net.Context;
using Mediator.Net.Contracts;
using SugarTalk.Core.Services.Tencent;
using SugarTalk.Messages.Commands.Tencent;

namespace SugarTalk.Core.Handlers.CommandHandlers.Tencent;

public class CloudRecordingCallBackCommandHandler : ICommandHandler<CloudRecordingCallBackCommand>
{
    private readonly ITencentService _tencentService;

    public CloudRecordingCallBackCommandHandler(ITencentService tencentService)
    {
        _tencentService = tencentService;
    }
    
    public async Task Handle(IReceiveContext<CloudRecordingCallBackCommand> context, CancellationToken cancellationToken)
    {
        await _tencentService.CloudRecordingCallBackAsync(context.Message, cancellationToken).ConfigureAwait(false);
    }
}