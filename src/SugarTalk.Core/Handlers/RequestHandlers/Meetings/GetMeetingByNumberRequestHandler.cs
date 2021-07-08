using System.Threading;
using System.Threading.Tasks;
using Mediator.Net.Context;
using Mediator.Net.Contracts;
using SugarTalk.Core.Services.Meetings;
using SugarTalk.Messages;
using SugarTalk.Messages.Dtos.Meetings;
using SugarTalk.Messages.Requests.Meetings;

namespace SugarTalk.Core.Handlers.RequestHandlers.Meetings
{
    public class GetMeetingByNumberRequestHandler : IRequestHandler<GetMeetingByNumberRequest, SugarTalkResponse<MeetingDto>>
    {
        private readonly IMeetingService _meetingService;

        public GetMeetingByNumberRequestHandler(IMeetingService meetingService)
        {
            _meetingService = meetingService;
        }

        public async Task<SugarTalkResponse<MeetingDto>> Handle(IReceiveContext<GetMeetingByNumberRequest> context, CancellationToken cancellationToken)
        {
            return await _meetingService.GetMeetingByNumber(context.Message, cancellationToken).ConfigureAwait(false);
        }
    }
}