using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using SugarTalk.Messages.Events.Meeting;

namespace SugarTalk.Core.Services.EventHandling;

public partial class EventHandlingService
{
    public async Task HandlingEventAsync(AudioChangedEvent @event, CancellationToken cancellationToken)
    {
        // 现已使用LiveKit的SDK控制
    }

    public async Task HandlingEventAsync(ScreenSharedEvent @event, CancellationToken cancellationToken)
    {
        // 现已使用LiveKit的SDK控制
    }
}