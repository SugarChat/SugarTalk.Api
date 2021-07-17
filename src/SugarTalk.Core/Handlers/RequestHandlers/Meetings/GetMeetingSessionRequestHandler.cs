using System.Threading;
using System.Threading.Tasks;
using Mediator.Net.Context;
using Mediator.Net.Contracts;
using SugarTalk.Core.Services.Kurento;
using SugarTalk.Core.Services.Meetings;
using SugarTalk.Messages;
using SugarTalk.Messages.Requests.Meetings;

namespace SugarTalk.Core.Handlers.RequestHandlers.Meetings
{
    public class GetMeetingSessionRequestHandler : IRequestHandler<GetMeetingSessionRequest, SugarTalkResponse<MeetingSession>>
    {
        private readonly IMeetingService _meetingService;

        public GetMeetingSessionRequestHandler(IMeetingService meetingService)
        {
            _meetingService = meetingService;
        }

        public async Task<SugarTalkResponse<MeetingSession>> Handle(IReceiveContext<GetMeetingSessionRequest> context, CancellationToken cancellationToken)
        {
            return await _meetingService.GetMeetingSession(context.Message, cancellationToken).ConfigureAwait(false);
        }
    }
}