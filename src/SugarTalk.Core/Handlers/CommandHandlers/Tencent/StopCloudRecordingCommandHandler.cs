using System.Threading;
using System.Threading.Tasks;
using Mediator.Net.Context;
using Mediator.Net.Contracts;
using SugarTalk.Core.Services.Tencent;
using SugarTalk.Messages.Commands.Tencent;

namespace SugarTalk.Core.Handlers.CommandHandlers.Tencent;

public class StopCloudRecordingCommandHandler : ICommandHandler<StopCloudRecordingCommand, StopCloudRecordingResponse>
{
    private readonly ITencentService _tencentService;

    public StopCloudRecordingCommandHandler(ITencentService tencentService)
    {
        _tencentService = tencentService;
    }
    
    public async Task<StopCloudRecordingResponse> Handle(IReceiveContext<StopCloudRecordingCommand> context, CancellationToken cancellationToken)
    {
        return await _tencentService.StopCloudRecordingAsync(context.Message, cancellationToken).ConfigureAwait(false);
    }
}