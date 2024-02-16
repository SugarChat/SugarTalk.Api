using System.Threading;
using System.Threading.Tasks;
using Mediator.Net.Context;
using Mediator.Net.Contracts;
using SugarTalk.Core.Services.Meetings;
using SugarTalk.Messages.Requests.Meetings;

namespace SugarTalk.Core.Handlers.RequestHandlers.Meetings
{
    public class GetCurrentUserMeetingRecordRequestHandler : IRequestHandler<GetCurrentUserMeetingRecordRequest, GetCurrentUserMeetingRecordResponse>
    {
        private readonly IMeetingService _meetingService;

        public GetCurrentUserMeetingRecordRequestHandler(IMeetingService meetingService)
        {
            _meetingService = meetingService;
        }
        public async Task<GetCurrentUserMeetingRecordResponse> Handle(IReceiveContext<GetCurrentUserMeetingRecordRequest> context, CancellationToken cancellationToken)
        {
            return await _meetingService.GetCurrentUserMeetingRecordsAsync(context.Message, cancellationToken).ConfigureAwait(false);
        }
    }
}