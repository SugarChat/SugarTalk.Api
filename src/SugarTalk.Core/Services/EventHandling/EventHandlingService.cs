using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using SugarTalk.Core.Ioc;
using SugarTalk.Core.Hubs;
using SugarTalk.Core.Services.Meetings;
using SugarTalk.Messages.Events.Meeting;

namespace SugarTalk.Core.Services.EventHandling;

public interface IEventHandlingService : IScopedDependency
{
    Task HandlingEventAsync(AudioChangedEvent @event, CancellationToken cancellationToken);
    
    Task HandlingEventAsync(ScreenSharedEvent @event, CancellationToken cancellationToken);
}

public partial class EventHandlingService : IEventHandlingService
{
    private readonly IHubContext<MeetingHub> _meetingHub;
    private readonly IMeetingDataProvider _meetingDataProvider;

    public EventHandlingService(IHubContext<MeetingHub> meetingHub, IMeetingDataProvider meetingDataProvider)
    {
        _meetingHub = meetingHub;
        _meetingDataProvider = meetingDataProvider;
    }
}