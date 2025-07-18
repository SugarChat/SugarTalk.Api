using System.Threading;
using System.Threading.Tasks;
using Mediator.Net.Context;
using Mediator.Net.Contracts;
using SugarTalk.Core.Services.Tencent;
using SugarTalk.Messages.Commands.Tencent;

namespace SugarTalk.Core.Handlers.CommandHandlers.Tencent;

public class UpdateCloudRecordingCommandHandler : ICommandHandler<UpdateCloudRecordingCommand, UpdateCloudRecordingResponse>
{
    private readonly ITencentService _tencentService;

    public UpdateCloudRecordingCommandHandler(ITencentService tencentService)
    {
        _tencentService = tencentService;
    }
    
    public async Task<UpdateCloudRecordingResponse> Handle(IReceiveContext<UpdateCloudRecordingCommand> context, CancellationToken cancellationToken)
    {
        return await _tencentService.UpdateCloudRecordingAsync(context.Message, cancellationToken).ConfigureAwait(false);
    }
}