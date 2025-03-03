using System.Threading;
using System.Threading.Tasks;
using Mediator.Net.Context;
using Mediator.Net.Contracts;
using SugarTalk.Core.Services.Meetings;
using SugarTalk.Messages.Commands.Meetings;

namespace SugarTalk.Core.Handlers.CommandHandlers.Meetings;

public class MeetingUserSpeakRecordCommandHandler : ICommandHandler<MeetingUserSpeakRecordCommand>
{
    private readonly IMeetingService _meetingService;
    
    public MeetingUserSpeakRecordCommandHandler(IMeetingService meetingService)
    {
        _meetingService = meetingService;
    }
    
    public async Task Handle(IReceiveContext<MeetingUserSpeakRecordCommand> context, CancellationToken cancellationToken)
    {
        await _meetingService.MeetingUserSpeakRecordAsync(context.Message, cancellationToken).ConfigureAwait(false);
    }
}