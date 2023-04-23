using System.Threading;
using System.Threading.Tasks;
using HR.Message.Contract.Event;
using SugarTalk.Core.Ioc;

namespace SugarTalk.Core.Services.Foundation;

public interface IFoundationService : IScopedDependency
{
    Task HandleFoundationInitializationEventAsync(FoundationInitializationEvent @event,
        CancellationToken cancellationToken);

    Task HandleLocationUSAddedEventAsync(LocationUSAddedEvent @event, CancellationToken cancellationToken);

    Task HandleLocationUSDeletedEventAsync(LocationUSDeletedEvent @event, CancellationToken cancellationToken);

    Task HandleLocationUSUpdatedEventAsync(LocationUSUpdatedEvent @event, CancellationToken cancellationToken);

    Task HandlePositionCNAddedEventAsync(PositionCNAddedEvent @event, CancellationToken cancellationToken);

    Task HandlePositionCNDeletedEventAsync(PositionCNDeletedEvent @event, CancellationToken cancellationToken);

    Task HandlePositionCNUpdatedEventAsync(PositionCNUpdatedEvent @event, CancellationToken cancellationToken);

    Task HandlePositionUSAddedEventAsync(PositionUSAddedEvent @event, CancellationToken cancellationToken);

    Task HandlePositionUSDeletedEventAsync(PositionUSDeletedEvent @event, CancellationToken cancellationToken);

    Task HandlePositionUSUpdatedEventAsync(PositionUSUpdatedEvent @event, CancellationToken cancellationToken);

    Task HandleStaffCNAddedEventAsync(StaffCNAddedEvent @event, CancellationToken cancellationToken);

    Task HandleStaffCNUpdatedEventAsync(StaffCNUpdatedEvent @event, CancellationToken cancellationToken);

    Task HandleStaffUSAddedEventAsync(StaffUSAddedEvent @event, CancellationToken cancellationToken);

    Task HandleStaffUSUpdatedEventAsync(StaffUSUpdatedEvent @event, CancellationToken cancellationToken);

    Task HandleUnitCNAddedEventAsync(UnitCNAddedEvent @event, CancellationToken cancellationToken);

    Task HandleUnitCNDeletedEventAsync(UnitCNDeletedEvent @event, CancellationToken cancellationToken);

    Task HandleUnitCNMovedEventAsync(UnitCNMovedEvent @event, CancellationToken cancellationToken);

    Task HandleUnitCNUpdatedEventAsync(UnitCNUpdatedEvent @event, CancellationToken cancellationToken);

    Task HandleUnitUSAddedEventAsync(UnitUSAddedEvent @event, CancellationToken cancellationToken);

    Task HandleUnitUSUpdatedEventAsync(UnitUSUpdatedEvent @event, CancellationToken cancellationToken);

    Task HandleUserAccountCNAddedEventAsync(UserAccountCNAddedEvent @event, CancellationToken cancellationToken);

    Task HandleUserAccountUSAddedEventAsync(UserAccountUSAddedEvent @event, CancellationToken cancellationToken);
}