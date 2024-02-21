using System.Threading;
using Mediator.Net.Context;
using Mediator.Net.Contracts;
using System.Threading.Tasks;
using SugarTalk.Messages.Events.Meeting.Summary;

namespace SugarTalk.Core.Handlers.EventHandlers.Meeting.Summary;

public class MeetingRecordSummarizedEventHandler : IEventHandler<MeetingRecordSummarizedEvent>
{
    public Task Handle(IReceiveContext<MeetingRecordSummarizedEvent> context, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}