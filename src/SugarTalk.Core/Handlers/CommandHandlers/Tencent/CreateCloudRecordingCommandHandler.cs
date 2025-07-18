using System.Threading;
using System.Threading.Tasks;
using Mediator.Net.Context;
using Mediator.Net.Contracts;
using SugarTalk.Core.Services.Meetings;
using SugarTalk.Core.Services.Tencent;
using SugarTalk.Messages.Commands.Speech;
using SugarTalk.Messages.Commands.Tencent;

namespace SugarTalk.Core.Handlers.CommandHandlers.Tencent;

public class CreateCloudRecordingCommandHandler : ICommandHandler<CreateCloudRecordingCommand, StartCloudRecordingResponse>
{
    private readonly ITencentService _tencentService;

    public CreateCloudRecordingCommandHandler(ITencentService tencentService)
    {
        _tencentService = tencentService;
    }
    
    public async Task<StartCloudRecordingResponse> Handle(IReceiveContext<CreateCloudRecordingCommand> context, CancellationToken cancellationToken)
    {
        return await _tencentService.CreateCloudRecordingAsync(context.Message, cancellationToken).ConfigureAwait(false);
    }
}