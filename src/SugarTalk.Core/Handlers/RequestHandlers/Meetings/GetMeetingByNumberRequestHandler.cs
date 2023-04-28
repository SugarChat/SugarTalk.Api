using System.Threading;
using System.Threading.Tasks;
using Mediator.Net.Context;
using Mediator.Net.Contracts;
using SugarTalk.Core.Services.Meetings;
using SugarTalk.Messages;
using SugarTalk.Messages.Requests.Meetings;
using SugarTalk.Messages.Responses;

namespace SugarTalk.Core.Handlers.RequestHandlers.Meetings
{
    public class GetMeetingByNumberRequestHandler : IRequestHandler<GetMeetingByNumberRequest, GetMeetingByNumberResponse>
    {
        private readonly IMeetingService _meetingService;

        public GetMeetingByNumberRequestHandler(IMeetingService meetingService)
        {
            _meetingService = meetingService;
        }

        public async Task<GetMeetingByNumberResponse> Handle(IReceiveContext<GetMeetingByNumberRequest> context, CancellationToken cancellationToken)
        {
            return await _meetingService.GetMeetingByNumberAsync(context.Message, cancellationToken).ConfigureAwait(false);
        }
    }
}