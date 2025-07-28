using System.Threading;
using System.Threading.Tasks;
using Mediator.Net.Context;
using Mediator.Net.Contracts;
using SugarTalk.Core.Services.Jobs;
using SugarTalk.Core.Services.Tencent;
using SugarTalk.Messages.Commands.Tencent;

namespace SugarTalk.Core.Handlers.CommandHandlers.Tencent;

public class CloudRecordingCallBackCommandHandler : ICommandHandler<CloudRecordingCallBackCommand>
{
    private readonly ISugarTalkBackgroundJobClient _sugarTalkBackgroundJobClient;

    public CloudRecordingCallBackCommandHandler(ITencentService tencentService, ISugarTalkBackgroundJobClient sugarTalkBackgroundJobClient)
    {
        _sugarTalkBackgroundJobClient = sugarTalkBackgroundJobClient;
    }
    
    public async Task Handle(IReceiveContext<CloudRecordingCallBackCommand> context, CancellationToken cancellationToken)
    {
        _sugarTalkBackgroundJobClient.Enqueue<ITencentService>(x => x.CloudRecordingCallBackAsync(context.Message, cancellationToken));
    }
}