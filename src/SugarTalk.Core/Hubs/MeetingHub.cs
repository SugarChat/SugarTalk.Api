using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace SugarTalk.Core.Hubs;

[Authorize]
public class MeetingHub : DynamicHub
{
    private string MeetingNumber => Context.GetHttpContext().Request.Query["meetingNumber"];
    
    public MeetingHub()
    {
    }
}