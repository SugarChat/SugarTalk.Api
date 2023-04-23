using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using HR.Message.Contract.Command;
using HR.Message.Contract.Event;
using Microsoft.EntityFrameworkCore;
using Serilog;
using SugarTalk.Core.Data;
using SugarTalk.Core.Domain.Foundation;

namespace SugarTalk.Core.Services.Foundation;

public class FoundationService : IFoundationService
{
    private readonly IMapper _mapper;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IRepository _repository;

    public FoundationService(IMapper mapper, IUnitOfWork unitOfWork, IRepository repository)
    {
        _mapper = mapper;
        _unitOfWork = unitOfWork;
        _repository = repository;
    }

    public async Task HandleFoundationInitializationEventAsync(FoundationInitializationEvent @event,
        CancellationToken cancellationToken)
    {
        await FoundationConsumerSafelyRunner.SafelyProcessRequestAsync(async () =>
        {
            var staffs = await _repository.Query<RmStaff>().ToListAsync(cancellationToken: cancellationToken);

            var units = await _repository.Query<RmUnit>().ToListAsync(cancellationToken: cancellationToken);

            var positions = await _repository.Query<RmPosition>()
                .ToListAsync(cancellationToken: cancellationToken);

            var locations = await _repository.Query<RmUsLocation>()
                .ToListAsync(cancellationToken: cancellationToken);

            await _repository.DeleteAllAsync(staffs, cancellationToken);
            await _repository.DeleteAllAsync(units, cancellationToken);
            await _repository.DeleteAllAsync(positions, cancellationToken);
            await _repository.DeleteAllAsync(locations, cancellationToken);

            #region 3.1 生成RmStaff数据。

            if (@event.CNStaffs != null && @event.CNStaffs.Count > 0)
            {
                var cnStaffs = new List<RmStaff>();

                foreach (var cnStaff in @event.CNStaffs)
                {
                    var rmStaff = _mapper.Map<RmStaff>(cnStaff);

                    rmStaff.CountryCode = CountryCode.CN;

                    cnStaffs.Add(rmStaff);
                }

                await _repository.InsertAllAsync(cnStaffs, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);
            }

            if (@event.USStaffs != null && @event.USStaffs.Count > 0)
            {
                var usStaffs = new List<RmStaff>();

                foreach (var usStaff in @event.USStaffs)
                {
                    var rmStaff = _mapper.Map<RmStaff>(usStaff);

                    rmStaff.CountryCode = CountryCode.US;

                    usStaffs.Add(rmStaff);
                }

                await _repository.InsertAllAsync(usStaffs, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);
            }

            #endregion

            #region 3.2 生成RmUnit数据。

            if (@event.CNUnits != null && @event.CNUnits.Count > 0)
            {
                var rmUnits = _mapper.Map<List<RmUnit>>(@event.CNUnits);
                var cnUnits = new List<RmUnit>();

                
                foreach (var cnUnit in @event.CNUnits)
                {
                    var rmUnit = _mapper.Map<RmUnit>(cnUnit);
                    
                    rmUnit.CountryCode = CountryCode.CN;

                    cnUnits.Add(rmUnit);
                }

                await _repository.InsertAllAsync(cnUnits, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);
            }

            if (@event.USUnits != null && @event.USUnits.Count > 0)
            {
                var usUnits = new List<RmUnit>();

                foreach (var usUnit in @event.USUnits)
                {
                    var rmUnit = _mapper.Map<RmUnit>(usUnit);
                    
                    rmUnit.CountryCode = CountryCode.US;

                    usUnits.Add(rmUnit);
                }

                await _repository.InsertAllAsync(usUnits, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);
            }

            #endregion

            #region 3.3 生成RmPosition数据。

            if (@event.CNPositions != null && @event.CNPositions.Count > 0)
            {
                List<RmPosition> cnPositions = new List<RmPosition>();

                foreach (var cnPosition in @event.CNPositions)
                {
                    var rmPosition = _mapper.Map<RmPosition>(cnPosition);

                    rmPosition.CountryCode = CountryCode.CN;

                    cnPositions.Add(rmPosition);
                }

                await _repository.InsertAllAsync(cnPositions, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);
            }

            if (@event.USPositions != null && @event.USPositions.Count > 0)
            {
                List<RmPosition> usPositions = new List<RmPosition>();
                
                foreach (var usPosition in @event.USPositions)
                {
                    var rmUsLocation = _mapper.Map<RmPosition>(usPosition);

                    rmUsLocation.CountryCode = CountryCode.US;

                    usPositions.Add(rmUsLocation);
                }

                await _repository.InsertAllAsync(usPositions, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);
            }

            #endregion

            #region 3.4 生成RmUsLocation数据。

            if (@event.USLocations != null && @event.USLocations.Count > 0)
            {
                var usLocations = new List<RmUsLocation>();

                foreach (var usLocation in @event.USLocations)
                {
                    var rmUsLocation = _mapper.Map<RmUsLocation>(usLocation);

                    usLocations.Add(rmUsLocation);
                }

                await _repository.InsertAllAsync(usLocations, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);
            }

            #endregion
            
        }, cancellationToken).ConfigureAwait(false);
    }

    public async Task HandleLocationUSAddedEventAsync(LocationUSAddedEvent @event, CancellationToken cancellationToken)
    {
        await FoundationConsumerSafelyRunner.SafelyProcessRequestAsync(async () =>
        {
            var rmUsLocation = _mapper.Map<RmUsLocation>(@event);

            await _repository.InsertAsync(rmUsLocation, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }, cancellationToken).ConfigureAwait(false);
    }

    public async Task HandleLocationUSDeletedEventAsync(LocationUSDeletedEvent @event,
        CancellationToken cancellationToken)
    {
        await FoundationConsumerSafelyRunner.SafelyProcessRequestAsync(async () =>
        {
            var location = await _repository.GetByIdAsync<RmUsLocation>(@event.LocationID, cancellationToken);

            if (location != null)
            {
                await _repository.DeleteAsync(location, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);
            }
        }, cancellationToken).ConfigureAwait(false);
    }

    public async Task HandleLocationUSUpdatedEventAsync(LocationUSUpdatedEvent @event,
        CancellationToken cancellationToken)
    {
        await FoundationConsumerSafelyRunner.SafelyProcessRequestAsync(async () =>
        {
            var location = await _repository.GetByIdAsync<RmUsLocation>(@event.LocationID, cancellationToken);

            if (location != null)
            {
                var rmUsLocation = _mapper.Map(@event, location);

                await _repository.UpdateAsync(rmUsLocation, cancellationToken);

                await _unitOfWork.SaveChangesAsync(cancellationToken);
            }
        }, cancellationToken).ConfigureAwait(false);
    }

    public async Task HandlePositionCNAddedEventAsync(PositionCNAddedEvent @event, CancellationToken cancellationToken)
    {
        await FoundationConsumerSafelyRunner.SafelyProcessRequestAsync(async () =>
        {
            var rmPosition = _mapper.Map<RmPosition>(@event);

            rmPosition.CountryCode = CountryCode.CN;

            await _repository.InsertAsync(rmPosition, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }, cancellationToken).ConfigureAwait(false);
    }

    public async Task HandlePositionCNDeletedEventAsync(PositionCNDeletedEvent @event,
        CancellationToken cancellationToken)
    {
        await FoundationConsumerSafelyRunner.SafelyProcessRequestAsync(async () =>
        {
            var position = await _repository.GetByIdAsync<RmPosition>(@event.PositionID, cancellationToken);
            if (position != null)
            {
                await _repository.DeleteAsync(position, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);
            }
        }, cancellationToken).ConfigureAwait(false);
    }

    public async Task HandlePositionCNUpdatedEventAsync(PositionCNUpdatedEvent @event,
        CancellationToken cancellationToken)
    {
        await FoundationConsumerSafelyRunner.SafelyProcessRequestAsync(async () =>
        {
            var position = await _repository.GetByIdAsync<RmPosition>(@event.PositionID, cancellationToken);

            if (position != null)
            {
                var rmPosition = _mapper.Map(@event, position);

                await _repository.UpdateAsync(rmPosition, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);
            }
        }, cancellationToken).ConfigureAwait(false);
    }

    public async Task HandlePositionUSAddedEventAsync(PositionUSAddedEvent @event, CancellationToken cancellationToken)
    {
        await FoundationConsumerSafelyRunner.SafelyProcessRequestAsync(async () =>
        {
            var rmPosition = _mapper.Map<RmPosition>(@event);

            rmPosition.CountryCode = CountryCode.US;

            await _repository.InsertAsync(rmPosition, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }, cancellationToken).ConfigureAwait(false);
    }

    public async Task HandlePositionUSDeletedEventAsync(PositionUSDeletedEvent @event,
        CancellationToken cancellationToken)
    {
        await FoundationConsumerSafelyRunner.SafelyProcessRequestAsync(async () =>
        {
            var position = await _repository.GetByIdAsync<RmPosition>(@event.PositionID, cancellationToken);

            if (position != null)
            {
                await _repository.DeleteAsync(position, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);
            }
        }, cancellationToken).ConfigureAwait(false);
    }

    public async Task HandlePositionUSUpdatedEventAsync(PositionUSUpdatedEvent @event,
        CancellationToken cancellationToken)
    {
        await FoundationConsumerSafelyRunner.SafelyProcessRequestAsync(async () =>
        {
            var position = await _repository.GetByIdAsync<RmPosition>(@event.PositionID, cancellationToken);

            var rmPosition = _mapper.Map(@event, position);

            if (position != null)
            {
                await _repository.UpdateAsync(rmPosition, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);
            }
        }, cancellationToken).ConfigureAwait(false);
    }

    public async Task HandleStaffCNAddedEventAsync(StaffCNAddedEvent @event, CancellationToken cancellationToken)
    {
        await FoundationConsumerSafelyRunner.SafelyProcessRequestAsync(async () =>
        {
            var rmStaff = _mapper.Map<RmStaff>(@event);

            await _repository.InsertAsync(rmStaff, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }, cancellationToken).ConfigureAwait(false);
    }

    public async Task HandleStaffCNUpdatedEventAsync(StaffCNUpdatedEvent @event, CancellationToken cancellationToken)
    {
        await FoundationConsumerSafelyRunner.SafelyProcessRequestAsync(async () =>
        {
            var staff = await _repository.GetByIdAsync<RmStaff>(@event.StaffID, cancellationToken);

            var rmStaff = _mapper.Map(@event, staff);

            if (staff != null)
            {
                await _repository.UpdateAsync(rmStaff, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);
            }
        }, cancellationToken).ConfigureAwait(false);
    }

    public async Task HandleStaffUSAddedEventAsync(StaffUSAddedEvent @event, CancellationToken cancellationToken)
    {
        await FoundationConsumerSafelyRunner.SafelyProcessRequestAsync(async () =>
        {
            var rmStaff = _mapper.Map<RmStaff>(@event);

            rmStaff.CountryCode = CountryCode.US;

            await _repository.InsertAsync(rmStaff, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }, cancellationToken).ConfigureAwait(false);
    }

    public async Task HandleStaffUSUpdatedEventAsync(StaffUSUpdatedEvent @event, CancellationToken cancellationToken)
    {
        await FoundationConsumerSafelyRunner.SafelyProcessRequestAsync(async () =>
        {
            var staff = await _repository.GetByIdAsync<RmStaff>(@event.StaffID, cancellationToken);

            var rmStaff = _mapper.Map(@event, staff);

            if (staff != null)
            {
                await _repository.UpdateAsync(rmStaff, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);
            }
        }, cancellationToken).ConfigureAwait(false);
    }

    public async Task HandleUnitCNAddedEventAsync(UnitCNAddedEvent @event, CancellationToken cancellationToken)
    {
        await FoundationConsumerSafelyRunner.SafelyProcessRequestAsync(async () =>
        {
            var rmUnit = _mapper.Map<RmUnit>(@event);
            rmUnit.CountryCode = CountryCode.CN;

            await _repository.InsertAsync(rmUnit, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }, cancellationToken).ConfigureAwait(false);
    }

    public async Task HandleUnitCNDeletedEventAsync(UnitCNDeletedEvent @event, CancellationToken cancellationToken)
    {
        await FoundationConsumerSafelyRunner.SafelyProcessRequestAsync(async () =>
        {
            var unit = await _repository.GetByIdAsync<RmUnit>(@event.UnitID, cancellationToken);

            if (unit != null)
            {
                await _repository.DeleteAsync(unit, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);
            }
        }, cancellationToken).ConfigureAwait(false);
    }

    public async Task HandleUnitCNMovedEventAsync(UnitCNMovedEvent @event, CancellationToken cancellationToken)
    {
        await FoundationConsumerSafelyRunner.SafelyProcessRequestAsync(async () =>
        {
            var unit = await _repository.GetByIdAsync<RmUnit>(@event.Entity.UnitID, cancellationToken);

            if (unit != null)
            {
                var updateUnits = new List<RmUnit>();

                var rmUnitEntity = _mapper.Map(@event.Entity, unit);

                if (@event.ChildEntities != null && @event.ChildEntities.Count > 0)
                {
                    foreach (var child in @event.ChildEntities)
                    {
                        var childUnit =
                            await _repository.GetByIdAsync<RmUnit>(child.UnitID, cancellationToken);

                        if (childUnit != null)
                        {
                            childUnit.LocationCode = child.LocationCode;

                            updateUnits.Add(childUnit);
                        }
                    }
                }

                updateUnits.Add(rmUnitEntity);

                await _repository.UpdateAllAsync(updateUnits, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);
            }
        }, cancellationToken).ConfigureAwait(false);
    }

    public async Task HandleUnitCNUpdatedEventAsync(UnitCNUpdatedEvent @event, CancellationToken cancellationToken)
    {
        await FoundationConsumerSafelyRunner.SafelyProcessRequestAsync(async () =>
        {
            var unit = await _repository.GetByIdAsync<RmUnit>(@event.Entity.UnitID, cancellationToken);

            if (unit != null)
            {
                List<RmUnit> updateUnits = new List<RmUnit>();

                var rmUnitEntity = _mapper.Map(@event.Entity, unit);

                if (@event.ChildEntities != null && @event.ChildEntities.Count > 0)
                {
                    foreach (var child in @event.ChildEntities)
                    {
                        var childUnit =
                            await _repository.GetByIdAsync<RmUnit>(child.UnitID, cancellationToken);

                        if (childUnit == null) continue;
                        childUnit.LocationCode = child.LocationCode;

                        updateUnits.Add(childUnit);
                    }

                    updateUnits.Add(rmUnitEntity);

                    await _repository.UpdateAllAsync(updateUnits, cancellationToken);
                    await _unitOfWork.SaveChangesAsync(cancellationToken);
                }
            }
        }, cancellationToken).ConfigureAwait(false);
    }

    public async Task HandleUnitUSAddedEventAsync(UnitUSAddedEvent @event, CancellationToken cancellationToken)
    {
        await FoundationConsumerSafelyRunner.SafelyProcessRequestAsync(async () =>
        {
            var rmUnit = _mapper.Map<RmUnit>(@event);

            rmUnit.CountryCode = CountryCode.US;

            await _repository.InsertAsync(rmUnit, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }, cancellationToken).ConfigureAwait(false);
    }

    public async Task HandleUnitUSUpdatedEventAsync(UnitUSUpdatedEvent @event, CancellationToken cancellationToken)
    {
        await FoundationConsumerSafelyRunner.SafelyProcessRequestAsync(async () =>
        {
            var unit = await _repository.GetByIdAsync<RmUnit>(@event.Entity.UnitID, cancellationToken);

            if (unit != null)
            {
                List<RmUnit> updateUnits = new List<RmUnit>();

                var rmUnits = _mapper.Map(@event.Entity, unit);

                if (@event.ChildEntities != null && @event.ChildEntities.Count > 0)
                {
                    foreach (var child in @event.ChildEntities)
                    {
                        var childUnit =
                            await _repository.GetByIdAsync<RmUnit>(child.UnitID, cancellationToken);

                        if (childUnit == null) continue;
                        childUnit.LocationCode = child.LocationCode;

                        updateUnits.Add(childUnit);
                    }
                }

                updateUnits.Add(rmUnits);

                await _repository.UpdateAllAsync(updateUnits, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);
            }
        }, cancellationToken).ConfigureAwait(false);
    }

    public async Task HandleUserAccountCNAddedEventAsync(UserAccountCNAddedEvent @event,
        CancellationToken cancellationToken)
    {
        await FoundationConsumerSafelyRunner.SafelyProcessRequestAsync(async () =>
        {
            var staff = await _repository.GetByIdAsync<RmStaff>(@event.StaffID, cancellationToken);

            if (staff != null)
            {
                if (staff.CountryCode.HasValue && staff.CountryCode.Value == CountryCode.CN)
                {
                    var rmStaff = _mapper.Map<RmStaff>(@event);

                    await _repository.UpdateAsync(rmStaff, cancellationToken);
                    await _unitOfWork.SaveChangesAsync(cancellationToken);
                }
            }
        }, cancellationToken).ConfigureAwait(false);
    }

    public async Task HandleUserAccountUSAddedEventAsync(UserAccountUSAddedEvent @event,
        CancellationToken cancellationToken)
    {
        await FoundationConsumerSafelyRunner.SafelyProcessRequestAsync(async () =>
        {
            var staff = await _repository.GetByIdAsync<RmStaff>(@event.StaffID, cancellationToken);

            if (staff != null)
            {
                if (staff.CountryCode.HasValue && staff.CountryCode.Value == CountryCode.US)
                {
                    var rmStaff = _mapper.Map<RmStaff>(@event);

                    await _repository.UpdateAsync(rmStaff, cancellationToken);
                    await _unitOfWork.SaveChangesAsync(cancellationToken);
                }
            }
        }, cancellationToken).ConfigureAwait(false);
    }
    
    private static class FoundationConsumerSafelyRunner
    {
        public static async Task SafelyProcessRequestAsync(Func<Task> action, CancellationToken cancellationToken)
        {
            try
            {
                await action().ConfigureAwait(false);

                cancellationToken.ThrowIfCancellationRequested();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Foundation sync error");
            }
        }
    }
}