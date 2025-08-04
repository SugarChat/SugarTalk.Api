using System.Threading;
using System.Threading.Tasks;
using Mediator.Net.Context;
using Mediator.Net.Contracts;
using SugarTalk.Core.Services.Http.Clients;
using SugarTalk.Core.Services.Jobs;
using SugarTalk.Core.Services.Tencent;
using SugarTalk.Messages.Commands.Tencent;

namespace SugarTalk.Core.Handlers.CommandHandlers.Tencent;

public class CloudRecordingCallBackCommandHandler : ICommandHandler<CloudRecordingCallBackCommand>
{
    private readonly ISugarTalkClient _sugarTalkClient;
    private readonly ISugarTalkBackgroundJobClient _sugarTalkBackgroundJobClient;

    public CloudRecordingCallBackCommandHandler(ITencentService tencentService, ISugarTalkBackgroundJobClient sugarTalkBackgroundJobClient, ISugarTalkClient sugarTalkClient)
    {
        _sugarTalkBackgroundJobClient = sugarTalkBackgroundJobClient;
        _sugarTalkClient = sugarTalkClient;
    }
    
    public async Task Handle(IReceiveContext<CloudRecordingCallBackCommand> context, CancellationToken cancellationToken)
    {
        _sugarTalkBackgroundJobClient.Enqueue<ITencentService>(x => x.CloudRecordingCallBackAsync(context.Message, cancellationToken));
      
        _sugarTalkBackgroundJobClient.Enqueue<ISugarTalkClient>(client => client.CloudRecordingCallBackAsync(new CloudRecordingCallBackCommand
        {
            EventType = context.Message.EventType,
            EventGroupId = context.Message.EventGroupId,
            CallbackTs = context.Message.CallbackTs,
            EventInfo = context.Message.EventInfo,
        }, cancellationToken));
    }
}