using System.Threading;
using System.Threading.Tasks;
using Mediator.Net.Context;
using Mediator.Net.Contracts;
using SugarTalk.Messages.Requests.Meetings;

namespace SugarTalk.Core.Handlers.RequestHandlers.Meetings;

public class GetMeetingSpeakTranslationRequestHandler : IRequestHandler<GetMeetingSpeakTranslationRequest, GetMeetingSpeakTranslationResponse>
{
    public Task<GetMeetingSpeakTranslationResponse> Handle(IReceiveContext<GetMeetingSpeakTranslationRequest> context, CancellationToken cancellationToken)
    {
        throw new System.NotImplementedException();
    }
}