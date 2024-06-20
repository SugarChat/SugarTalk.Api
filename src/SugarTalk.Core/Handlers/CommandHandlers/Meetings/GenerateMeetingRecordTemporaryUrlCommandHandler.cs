using System.Threading;
using System.Threading.Tasks;
using Mediator.Net.Context;
using Mediator.Net.Contracts;
using SugarTalk.Core.Services.Meetings;
using SugarTalk.Messages.Requests.Meetings;

namespace SugarTalk.Core.Handlers.RequestHandlers.Meetings;

public class GenerateMeetingRecordTemporaryUrlCommandHandler : ICommandHandler<GenerateMeetingRecordTemporaryUrlCommand, GenerateMeetingRecordTemporaryUrlResponse>
{
    private readonly IMeetingService _meetingService;

    public GenerateMeetingRecordTemporaryUrlCommandHandler(IMeetingService meetingService)
    {
        _meetingService = meetingService;
    }

    public Task<GenerateMeetingRecordTemporaryUrlResponse> Handle(IReceiveContext<GenerateMeetingRecordTemporaryUrlCommand> context, CancellationToken cancellationToken)
    {
        return  _meetingService.GenerateMeetingRecordTemporaryUrlAsync(context.Message);
    }
}