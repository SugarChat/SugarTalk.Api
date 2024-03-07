using System.Threading;
using System.Threading.Tasks;
using Mediator.Net.Context;
using Mediator.Net.Contracts;
using SugarTalk.Core.Services.Meetings;
using SugarTalk.Messages.Commands.Meetings;

namespace SugarTalk.Core.Handlers.CommandHandlers.Meetings;

public class StorageMeetingRecordVideoCommandHandler : ICommandHandler<StorageMeetingRecordVideoCommand, StorageMeetingRecordVideoResponse>
{
    private readonly IMeetingService _meetingService;

    public StorageMeetingRecordVideoCommandHandler(IMeetingService meetingService)
    {
        _meetingService = meetingService;
    }

    public async Task<StorageMeetingRecordVideoResponse> Handle(IReceiveContext<StorageMeetingRecordVideoCommand> context, CancellationToken cancellationToken)
    {
        return await _meetingService.StorageMeetingRecordVideoAsync(context.Message, cancellationToken).ConfigureAwait(false);
    }
}