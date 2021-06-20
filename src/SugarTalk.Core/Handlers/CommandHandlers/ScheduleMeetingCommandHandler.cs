using System.Threading;
using System.Threading.Tasks;
using Mediator.Net.Context;
using Mediator.Net.Contracts;
using SugarTalk.Messages;

namespace SugarTalk.Core.Handlers.CommandHandlers
{
    public class ScheduleMeetingCommandHandler: ICommandHandler<ScheduleMeetingCommand>
    {
        public Task Handle(IReceiveContext<ScheduleMeetingCommand> context, CancellationToken cancellationToken)
        {
            throw new BusinessException(50001,"This method has not yet implemented");
        }
    }
}