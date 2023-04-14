using System.Threading;
using System.Threading.Tasks;
using Mediator.Net.Context;
using Mediator.Net.Contracts;
using SugarTalk.Core.Services.Meetings;
using SugarTalk.Messages;
using SugarTalk.Messages.Dtos.Meetings;
using SugarTalk.Messages.Requests.Meetings;
using SugarTalk.Messages.Responses;

namespace SugarTalk.Core.Handlers.RequestHandlers.Meetings
{
    public class GetMeetingSessionRequestHandler : IRequestHandler<GetMeetingSessionRequest, GetMeetingSessionResponse>
    {
        private readonly IMeetingSessionService _meetingSessionService;

        public GetMeetingSessionRequestHandler(IMeetingSessionService meetingSessionService)
        {
            _meetingSessionService = meetingSessionService;
        }

        public async Task<GetMeetingSessionResponse> Handle(IReceiveContext<GetMeetingSessionRequest> context, CancellationToken cancellationToken)
        {
            return await _meetingSessionService.GetMeetingSession(context.Message, cancellationToken).ConfigureAwait(false);
        }
    }
}