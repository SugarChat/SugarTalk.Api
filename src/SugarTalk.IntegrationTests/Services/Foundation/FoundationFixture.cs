using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HR.Message.Contract.Command;
using HR.Message.Contract.Event;
using Microsoft.EntityFrameworkCore;
using Shouldly;
using Smarties.Messages.Enums.Foundation;
using SugarTalk.Core.Data;
using SugarTalk.Core.Domain.Foundation;
using SugarTalk.Core.Services.Foundation;
using SugarTalk.IntegrationTests.TestBaseClasses;
using SugarTalk.IntegrationTests.Utils.Foundation;
using Xunit;

namespace SugarTalk.IntegrationTests.Services.Foundation;

public class FoundationFixture : FoundationFixtureBase
{
    private readonly FoundationUtil _foundationUtil;

    public FoundationFixture()
    {
        _foundationUtil = new FoundationUtil(CurrentScope);
    }

    [Fact]
    public async Task CanHandleFoundationInitializationEvent()
    {
        await Run<IFoundationService, IRepository>(async (f, repository) =>
        {
            var staffs = await repository.Query<RmStaff>().ToListAsync().ConfigureAwait(false);
            var units = await repository.Query<RmUnit>().ToListAsync().ConfigureAwait(false);
            var positions = await repository.Query<RmPosition>().ToListAsync().ConfigureAwait(false);
            var locations = await repository.Query<RmUsLocation>().ToListAsync().ConfigureAwait(false);

            staffs.Count.ShouldBe(0);
            units.Count.ShouldBe(0);
            positions.Count.ShouldBe(0);
            locations.Count.ShouldBe(0);

            await f.HandleFoundationInitializationEventAsync(new FoundationInitializationEvent
            {
                CNStaffs = new List<CNStaff>
                {
                    new()
                    {
                        ID = Guid.NewGuid(),
                        UserID = Guid.NewGuid(),
                        UserName = "张三",
                        PositionStatus = 1,
                        PhoneNumber = "1364789"
                    },
                    new()
                    {
                        ID = Guid.NewGuid(),
                        UserID = Guid.NewGuid(),
                        UserName = "李四",
                        PositionStatus = 1,
                        PhoneNumber = "1364789"
                    }
                },
                USStaffs = new List<USStaff>
                {
                    new()
                    {
                        StaffID = Guid.NewGuid(),
                        UserID = Guid.NewGuid(),
                        UserName = "Mr third",
                        PositionStatus = 1,
                        WorkPhone = "100800"
                    },
                    new()
                    {
                        StaffID = Guid.NewGuid(),
                        UserID = Guid.NewGuid(),
                        UserName = "Miss third",
                        PositionStatus = 1,
                        WorkPhone = "100800"
                    }
                },
                CNUnits = new List<CNUnit>
                {
                    new()
                    {
                        ID = Guid.NewGuid(),
                        ParentID = Guid.NewGuid(),
                        LeaderID = Guid.NewGuid(),
                        TypeID = 2,
                        Name = "王五",
                        LeaderCountryCode = 1,
                        IsActive = true,
                        LocationCode = "20"
                    },
                    new()
                    {
                        ID = Guid.NewGuid(),
                        ParentID = Guid.NewGuid(),
                        LeaderID = Guid.NewGuid(),
                        TypeID = 2,
                        Name = "老六",
                        LeaderCountryCode = 1,
                        IsActive = true,
                    }
                },
                USUnits = new List<USUnit>
                {
                    new()
                    {
                        UnitID = Guid.NewGuid(),
                        ParentID = Guid.NewGuid(),
                        LeaderID = Guid.NewGuid(),
                        Name = "Mr six",
                        LeaderCountryCode = 2,
                        LocationCode = "2",
                        TypeID = 4,
                    },
                    new()
                    {
                        UnitID = Guid.NewGuid(),
                        ParentID = Guid.NewGuid(),
                        LeaderID = Guid.NewGuid(),
                        Name = "Miss six",
                        LeaderCountryCode = 2,
                        LocationCode = "2",
                        TypeID = 4
                    }
                },
                CNPositions = new List<CNPosition>
                {
                    new()
                    {
                        ID = Guid.NewGuid(),
                        UnitID = Guid.NewGuid(),
                        Name = "会计",
                        Description = "这是中国岗位",
                        IsActive = true
                    },
                    new()
                    {
                        ID = Guid.NewGuid(),
                        UnitID = Guid.NewGuid(),
                        Name = "会计",
                        Description = "这是中国岗位",
                        IsActive = true
                    }
                },
                USPositions = new List<USPosition>
                {
                    new()
                    {
                        PositionID = Guid.NewGuid(),
                        UnitID = Guid.NewGuid(),
                        Name = "Accounting",
                        Description = "美国岗位"
                    },
                    new()
                    {
                        PositionID = Guid.NewGuid(),
                        UnitID = Guid.NewGuid(),
                        Name = "Accounting",
                        Description = "美国岗位"
                    }
                },
                USLocations = new List<USLocation>
                {
                    new()
                    {
                        LocationID = Guid.NewGuid(),
                        LocationCode = "20",
                        WarehouseCode = "445566",
                        Name = "洛杉矶"
                    },
                    new()
                    {
                        LocationID = Guid.NewGuid(),
                        LocationCode = "20",
                        WarehouseCode = "445566",
                        Name = "旧金山"
                    }
                },
            }, default).ConfigureAwait(false);
        });

        await Run<IRepository>(async repository =>
            {
                var staffs = await repository.Query<RmStaff>().ToListAsync().ConfigureAwait(false);
                var units = await repository.Query<RmUnit>().ToListAsync().ConfigureAwait(false);
                var positions = await repository.Query<RmPosition>().ToListAsync().ConfigureAwait(false);
                var locations = await repository.Query<RmUsLocation>().ToListAsync().ConfigureAwait(false);

                staffs.Count.ShouldBe(4);
                units.Count.ShouldBe(4);
                positions.Count.ShouldBe(4);
                locations.Count.ShouldBe(2);
            }
        );
    }

    [Fact]
    public async Task CanHandleLocationUSAddedEvent()
    {
        await Run<IFoundationService, IRepository>(async (f, repository) =>
        {
            await f.HandleLocationUSAddedEventAsync(new LocationUSAddedEvent
            {
                LocationID = Guid.NewGuid(),
                Status = 2,
                LocationType = 2,
            }, default).ConfigureAwait(false);
            
            var response = await repository.Query<RmUsLocation>().ToListAsync().ConfigureAwait(false);
            
            response.ShouldNotBeNull();
            response.Count.ShouldBe(1);
        });
    }

    [Fact]
    public async Task CanHandleLocationUSDeletedEvent()
    {
        await _foundationUtil.AddUsLocationAsync(Guid.Parse("E168D27A-99F1-4D12-972F-0693A664F194"))
            .ConfigureAwait(false);

        await Run<IFoundationService, IRepository>(async (f, repository) =>
        {
            var locationUsDeleted = new LocationUSDeletedEvent
            {
                LocationID = Guid.Parse("E168D27A-99F1-4D12-972F-0693A664F194")
            };
            var before = await repository.QueryNoTracking<RmUsLocation>().ToListAsync().ConfigureAwait(false);
            
            before.Count.ShouldBe(1);
            before.FirstOrDefault()?.Id.ShouldBe(Guid.Parse("E168D27A-99F1-4D12-972F-0693A664F194"));

            await f.HandleLocationUSDeletedEventAsync(locationUsDeleted, default).ConfigureAwait(false);
            
            var after = await repository.QueryNoTracking<RmUsLocation>().ToListAsync().ConfigureAwait(false);
                
            after.ShouldNotBeNull();
            after.Count.ShouldBe(0);
        });
    }

    [Fact]
    public async Task CanHandleLocationUSUpdateEvent()
    {
        await _foundationUtil.AddUsLocationAsync(Guid.Parse("E168D27A-99F1-4D12-972F-0693A664F194"), "位置A", "10")
            .ConfigureAwait(false);

        await Run<IFoundationService, IRepository>(async (f, repository) =>
        {
            var locationUsUpdated = new LocationUSUpdatedEvent
            {
                LocationID = Guid.Parse("E168D27A-99F1-4D12-972F-0693A664F194"),
                Name = "位置B",
                WarehouseCode = "20"
            };
            var before =
                await repository.QueryNoTracking<RmUsLocation>().ToListAsync().ConfigureAwait(false);
            before.Count.ShouldBe(1);
            before.FirstOrDefault()?.Id.ShouldBe(Guid.Parse("E168D27A-99F1-4D12-972F-0693A664F194"));
            before.FirstOrDefault()?.Name.ShouldBe("位置A");
            before.FirstOrDefault()?.WarehouseCode.ShouldBe("10");

            await f.HandleLocationUSUpdatedEventAsync(locationUsUpdated, default).ConfigureAwait(false);

            var after =
                await repository.QueryNoTracking<RmUsLocation>().ToListAsync().ConfigureAwait(false);
            after.Count.ShouldBe(1);
            after.FirstOrDefault()?.Id.ShouldBe(Guid.Parse("E168D27A-99F1-4D12-972F-0693A664F194"));
            after.FirstOrDefault()?.Name.ShouldBe("位置B");
            after.FirstOrDefault()?.WarehouseCode.ShouldBe("20");
        });
    }

    [Fact]
    public async Task CanHandlePositionCNAddEvent()
    {
        await Run<IFoundationService, IRepository>(async (f, repository) =>
        {
            await f.HandlePositionCNAddedEventAsync(new PositionCNAddedEvent(), default).ConfigureAwait(false);
        });

        await Run<IRepository>(async repository =>
            {
                var afterDeleteResponse =
                    await repository.QueryNoTracking<RmPosition>().ToListAsync().ConfigureAwait(false);
                afterDeleteResponse.ShouldNotBeNull();
                afterDeleteResponse.Count.ShouldBe(1);
            }
        );
    }

    [Fact]
    public async Task CanHandlePositionCNDeletedEvent()
    {
        await _foundationUtil.AddPositionAsync(Guid.Parse("E168D27A-99F1-4D12-972F-0693A664F194")).ConfigureAwait(false);

        await Run<IFoundationService, IRepository>(async (f, repository) =>
        {
            var before = await repository.QueryNoTracking<RmPosition>().ToListAsync().ConfigureAwait(false);
            
            before.Count.ShouldBe(1);

            var positionCnDeletedEvent = new PositionCNDeletedEvent
            {
                PositionID = Guid.Parse("E168D27A-99F1-4D12-972F-0693A664F194")
            };

            await f.HandlePositionCNDeletedEventAsync(positionCnDeletedEvent, default).ConfigureAwait(false);

            var after = await repository.QueryNoTracking<RmPosition>().ToListAsync().ConfigureAwait(false);
            
            after.ShouldNotBeNull();
            after.Count.ShouldBe(0);
        });
    }

    [Fact]
    public async Task CanHandlePositionCNUpdatedEvent()
    {
        await _foundationUtil.AddPositionAsync(Guid.Parse("E168D27A-99F1-4D12-972F-0693A664F194"),
            Guid.Parse("E168D27A-99F1-4D12-972F-0693A664F194"), "员工A", isActive: false).ConfigureAwait(false);

        await Run<IFoundationService, IRepository>(async (f, repository) =>
        {
            var cnUpdatedEvent = new PositionCNUpdatedEvent
            {
                PositionID = Guid.Parse("E168D27A-99F1-4D12-972F-0693A664F194"),
                Name = "员工B",
                IsActive = true
            };
            await f.HandlePositionCNUpdatedEventAsync(cnUpdatedEvent, default).ConfigureAwait(false);

            var after = await repository.Query<RmPosition>().ToListAsync().ConfigureAwait(false);
            
            after.FirstOrDefault()?.Name.ShouldBe("员工B");
            after.FirstOrDefault()?.IsActive.ShouldBe(true);
        });
    }

    [Fact]
    public async Task CanHandlePositionUSAddedEvent()
    {
        await Run<IFoundationService, IRepository>(async (f, repository) =>
        {
            var before = await repository.Query<RmPosition>().ToListAsync().ConfigureAwait(false);
            before.Count.ShouldBe(0);

            await f.HandlePositionUSAddedEventAsync(new PositionUSAddedEvent
            {
                PositionID = Guid.NewGuid()
            }, default).ConfigureAwait(false);

            var response = await repository.Query<RmPosition>().ToListAsync().ConfigureAwait(false);
            
            response.ShouldNotBeNull();
            response.Count.ShouldBe(1);
        });
    }

    [Fact]
    public async Task CanHandlePositionUSDeletedEvent()
    {
        await Run<IFoundationService, IRepository>(async (f, repository) =>
        {
            var deletedEvent = new PositionUSDeletedEvent
            {
                PositionID = Guid.Parse("E168D27A-99F1-4D12-972F-0693A664F194")
            };

            await f.HandlePositionUSDeletedEventAsync(deletedEvent, default).ConfigureAwait(false);

            var response = await repository.Query<RmPosition>().ToListAsync().ConfigureAwait(false);
            
            response.ShouldNotBeNull();
            response.Count.ShouldBe(0);
        });
    }

    [Fact]
    public async Task CanHandlePosionUSUpdatedEvent()
    {
        await _foundationUtil.AddPositionAsync(Guid.Parse("E168D27A-99F1-4D12-972F-0693A664F194"),
            Guid.Parse("E168D27A-99F1-4D12-972F-0693A664F194"), "员工A", "这是一个简单的描述").ConfigureAwait(false);

        await Run<IFoundationService, IRepository>(async (f, repository) =>
        {
            var positionUsUpdated = new PositionUSUpdatedEvent
            {
                PositionID = Guid.Parse("E168D27A-99F1-4D12-972F-0693A664F194"),
                Name = "员工B",
                Description = "更改为复杂一点的描述"
            };

            await f.HandlePositionUSUpdatedEventAsync(positionUsUpdated, default).ConfigureAwait(false);

            var after =
                await repository.Query<RmPosition>().ToListAsync().ConfigureAwait(false);
            
            after.FirstOrDefault()?.Name.ShouldBe("员工B");
            after.FirstOrDefault()?.Description.ShouldBe("更改为复杂一点的描述");
        });
    }

    [Fact]
    public async Task CanHandleStaffCNAddedEvent()
    {
        await Run<IFoundationService, IRepository>(async (f, repository) =>
        {
            var before = await repository.Query<RmStaff>().ToListAsync().ConfigureAwait(false);
            before.Count.ShouldBe(0);

            await f.HandleStaffCNAddedEventAsync(new StaffCNAddedEvent()
            {
                StaffID = Guid.Parse("E168D27A-99F1-4D12-972F-0693A664F194")
            }, default).ConfigureAwait(false);

            var after = await repository.Query<RmStaff>().ToListAsync().ConfigureAwait(false);
            
            after.Count.ShouldBe(1);
        });
    }

    [Fact]
    public async Task CanHandleStaffCNUpdatedEvent()
    {
        await Run<IFoundationService, IRepository>(async (f, repository) =>
        {
            var staffId = Guid.Parse("E168D27A-99F1-4D12-972F-0693A664F194");

            await _foundationUtil.AddStaffAsync(staffId, workPlace: "第三办公室", nameEnLong: "1000")
                .ConfigureAwait(false);

            var before = await repository.QueryNoTracking<RmStaff>().ToListAsync().ConfigureAwait(false);

            before.Count.ShouldBe(1);
            before.FirstOrDefault()?.WorkPlace.ShouldBe("第三办公室");

            var staffCnUpdatedEvent = new StaffCNUpdatedEvent
            {
                StaffID = staffId,
                WorkPlace = "第一办公室",
                NameCNLong = "50"
            };
            await f.HandleStaffCNUpdatedEventAsync(staffCnUpdatedEvent, default).ConfigureAwait(false);

            var after = await repository.QueryNoTracking<RmStaff>().ToListAsync().ConfigureAwait(false);

            after.FirstOrDefault()?.WorkPlace.ShouldBe("第一办公室");
            after.FirstOrDefault()?.NameCNLong.ShouldBe("50");
        });
    }

    [Fact]
    public async Task CanHandleStaffUSAddedEvent()
    {
        await Run<IFoundationService, IRepository>(async (f, repository) =>
        {
            var before = await repository.QueryNoTracking<RmStaff>().ToListAsync().ConfigureAwait(false);
            before.Count.ShouldBe(0);

            await f.HandleStaffUSAddedEventAsync(new StaffUSAddedEvent()
            {
                StaffID = Guid.Parse("E168D27A-99F1-4D12-972F-0693A664F194"),
                UserName = "张三",
            }, default).ConfigureAwait(false);

            var after = await repository.QueryNoTracking<RmStaff>().ToListAsync().ConfigureAwait(false);

            after.Count.ShouldBe(1);
            after.FirstOrDefault()?.UserName.ShouldBe("张三");
        });
    }

    [Fact]
    public async Task CanHandleStaffUSUpdatedEvent()
    {
        var staffId = Guid.Parse("E168D27A-99F1-4D12-972F-0693A664F194");

        await _foundationUtil.AddStaffAsync(staffId, nameCnLong: "张三").ConfigureAwait(false);

        await Run<IFoundationService, IRepository>(async (f, repository) =>
        {
            var before = await repository.QueryNoTracking<RmStaff>().ToListAsync().ConfigureAwait(false);

            before.Count.ShouldBe(1);

            await f.HandleStaffUSUpdatedEventAsync(new StaffUSUpdatedEvent
            {
                StaffID = staffId,
                PayrollName = "李四喜",
                PositionStatus = 2,
            }, default).ConfigureAwait(false);

            var after = await repository.QueryNoTracking<RmStaff>().ToListAsync().ConfigureAwait(false);

            after.Count.ShouldBe(1);
            after.FirstOrDefault()?.NameENLong.ShouldBe("李四喜");
        });
    }

    [Fact]
    public async Task CanHandleUnitCNAddedEvent()
    {
        await Run<IFoundationService, IRepository>(async (f, repository) =>
        {
            var before = await repository.QueryNoTracking<RmUnit>().ToListAsync().ConfigureAwait(false);
            before.Count.ShouldBe(0);

            await f.HandleUnitCNAddedEventAsync(new UnitCNAddedEvent
            {
                UnitID = Guid.Parse("E168D27A-99F1-4D12-972F-0693A664F194"),
                LeaderCountryCode = 1,
                TypeID = 2
            }, default).ConfigureAwait(false);

            var after = await repository.QueryNoTracking<RmUnit>().ToListAsync().ConfigureAwait(false);

            after.Count.ShouldBe(1);
        });
    }

    [Fact]
    public async Task CanHandleUnitCNDeletedEvent()
    {
        var staffId = Guid.Parse("E168D27A-99F1-4D12-972F-0693A664F194");

        await _foundationUtil
            .AddUnitAsync(staffId, countryCode: (CountryCode?)1, leaderCountryCode: (CountryCode?)1, typeId: (UnitType)2)
            .ConfigureAwait(false);

        await Run<IFoundationService, IRepository>(async (f, repository) =>
        {
            var before = await repository.QueryNoTracking<RmUnit>().ToListAsync().ConfigureAwait(false);
            before.Count.ShouldBe(1);

            await f.HandleUnitCNDeletedEventAsync(new UnitCNDeletedEvent
            {
                UnitID = Guid.Parse("E168D27A-99F1-4D12-972F-0693A664F194")
            }, default).ConfigureAwait(false);

            var after = await repository.QueryNoTracking<RmUnit>().ToListAsync().ConfigureAwait(false);

            after.Count.ShouldBe(0);
        });
    }

    [Fact]
    public async Task CanHandleUnitCNMovedEvent()
    {
        var parentId = Guid.Parse("E168D27A-99F1-4D12-972F-0693A664F194");
        var childId = Guid.Parse("E168D27A-99F1-4D12-972F-0693A664F195");
        var childIdSecond = Guid.Parse("E168D27A-99F1-4D12-972F-0693A664F196");

        await Run<IFoundationService, IRepository>(async (f, repository) =>
        {
            await _foundationUtil.AddUnitAsync(parentId, parentId: parentId, locationCode: "20").ConfigureAwait(false);
            await _foundationUtil.AddUnitAsync(childId, parentId: parentId, locationCode: "30").ConfigureAwait(false);
            await _foundationUtil.AddUnitAsync(childIdSecond, parentId: parentId, locationCode: "40")
                .ConfigureAwait(false);

            var before = await repository.QueryNoTracking<RmUnit>().ToListAsync().ConfigureAwait(false);

            before.First(x => x.ParentId == parentId).LocationCode.ShouldBe("20");
            before.First(x => x.Id == childId).LocationCode.ShouldBe("30");
            before.First(x => x.Id == childIdSecond).LocationCode.ShouldBe("40");

            await f.HandleUnitCNMovedEventAsync(new UnitCNMovedEvent
            {
                Entity = new UnitCNMovedEventEntity
                {
                    ParentID = parentId,
                    UnitID = parentId,
                    LocationCode = "200"
                },
                ChildEntities = new List<UnitCNMovedEventEntity>
                {
                    new UnitCNMovedEventEntity
                    {
                        ParentID = parentId,
                        UnitID = childId,
                        LocationCode = "300"
                    },
                    new UnitCNMovedEventEntity
                    {
                        ParentID = parentId,
                        UnitID = childIdSecond,
                        LocationCode = "400"
                    }
                }
            }, default);

            var after = await repository.QueryNoTracking<RmUnit>().ToListAsync().ConfigureAwait(false);

            after.First(x => x.Id == parentId).LocationCode.ShouldBe("200");
            after.First(x => x.Id == childId).LocationCode.ShouldBe("300");
            after.First(x => x.Id == childIdSecond).LocationCode.ShouldBe("400");
        });
    }

    [Fact]
    public async Task CanHandleUnitCNUpdatedEvent()
    {
        var parentId = Guid.Parse("E168D27A-99F1-4D12-972F-0693A664F194");
        var childId = Guid.Parse("E168D27A-99F1-4D12-972F-0693A664F195");
        var childIdSecond = Guid.Parse("E168D27A-99F1-4D12-972F-0693A664F196");

        await _foundationUtil.AddUnitAsync(parentId, parentId: parentId, locationCode: "20").ConfigureAwait(false);
        await _foundationUtil.AddUnitAsync(childId, parentId: parentId, locationCode: "30").ConfigureAwait(false);
        await _foundationUtil.AddUnitAsync(childIdSecond, parentId: parentId, locationCode: "40").ConfigureAwait(false);

        await Run<IFoundationService, IRepository>(async (f, repository) =>
        {
            var before = await repository.QueryNoTracking<RmUnit>().ToListAsync().ConfigureAwait(false);

            before.First(x => x.ParentId == parentId).LocationCode.ShouldBe("20");
            before.First(x => x.Id == childId).LocationCode.ShouldBe("30");
            before.First(x => x.Id == childIdSecond).LocationCode.ShouldBe("40");

            await f.HandleUnitCNUpdatedEventAsync(new UnitCNUpdatedEvent
            {
                Entity = new UnitCNUpdatedEventEntity
                {
                    ParentID = parentId,
                    UnitID = parentId,
                    Name = "张经理",
                    LeaderID = Guid.NewGuid(),
                    LeaderCountryCode = 2,
                    LocationCode = "200",
                    Description = "积极员工",
                    IsActive = true
                },
                ChildEntities = new List<UnitCNUpdatedEventEntity>
                {
                    new UnitCNUpdatedEventEntity
                    {
                        ParentID = parentId,
                        UnitID = childId,
                        LocationCode = "300"
                    },
                    new UnitCNUpdatedEventEntity
                    {
                        ParentID = parentId,
                        UnitID = childIdSecond,
                        LocationCode = "400"
                    }
                }
            }, default);

            var after = await repository.QueryNoTracking<RmUnit>().ToListAsync().ConfigureAwait(false);

            after.FirstOrDefault(x => x.ParentId == parentId)?.Name.ShouldBe("张经理");
            after.FirstOrDefault(x => x.ParentId == parentId)?.Description.ShouldBe("积极员工");

            after.FirstOrDefault(x => x.Id == childId)?.LocationCode.ShouldBe("300");
            after.FirstOrDefault(x => x.Id == childIdSecond)?.LocationCode.ShouldBe("400");
        });
    }

    [Fact]
    public async Task CanHandleUnitUSAddedEvent()
    {
        await Run<IFoundationService, IRepository>(async (f, repository) =>
        {
            var before = await repository.QueryNoTracking<RmUnit>().ToListAsync().ConfigureAwait(false);

            before.ShouldNotBeNull();
            before.Count.ShouldBe(0);

            await f.HandleUnitUSAddedEventAsync(new UnitUSAddedEvent()
            {
                UnitID = Guid.NewGuid(),
                TypeID = 2,
                LeaderCountryCode = 2
            }, default).ConfigureAwait(false);

            var after = await repository.QueryNoTracking<RmUnit>().ToListAsync().ConfigureAwait(false);

            after.ShouldNotBeNull();
            after.Count.ShouldBe(1);
        });
    }

    [Fact]
    public async Task CanHandleUnitUSUpdatedEvent()
    {
        var parentId = Guid.Parse("E168D27A-99F1-4D12-972F-0693A664F194");
        var childId = Guid.Parse("E168D27A-99F1-4D12-972F-0693A664F195");
        var childIdSecond = Guid.Parse("E168D27A-99F1-4D12-972F-0693A664F196");

        await _foundationUtil.AddUnitAsync(parentId, parentId: parentId, name: "张三组", leaderId: parentId,
                typeId: UnitType.Group, leaderCountryCode: CountryCode.CN, locationCode: "20")
            .ConfigureAwait(false);

        await _foundationUtil.AddUnitAsync(childId, parentId: parentId, leaderId: childId, typeId: UnitType.Dept,
                leaderCountryCode: CountryCode.CN, locationCode: "20")
            .ConfigureAwait(false);

        await _foundationUtil.AddUnitAsync(childIdSecond, parentId: parentId, leaderId: childIdSecond,
                typeId: UnitType.Group, leaderCountryCode: CountryCode.CN, locationCode: "20")
            .ConfigureAwait(false);

        await Run<IFoundationService, IRepository>(async (f, repository) =>
        {
            var before = await repository.QueryNoTracking<RmUnit>().ToListAsync().ConfigureAwait(false);

            before.FirstOrDefault(x => x.Id.Equals(parentId))?.TypeId.ShouldBe(UnitType.Group);
            before.FirstOrDefault(x => x.Id.Equals(childId))?.TypeId.ShouldBe(UnitType.Dept);
            before.FirstOrDefault(x => x.Id.Equals(childIdSecond))?.TypeId.ShouldBe(UnitType.Group);

            await f.HandleUnitUSUpdatedEventAsync(new UnitUSUpdatedEvent()
            {
                Entity = new UnitUSUpdatedEventEntity
                {
                    UnitID = parentId,
                    Name = "张三丰组",
                    TypeID = 4,
                    ParentID = parentId,
                    LeaderID = Guid.NewGuid(),
                    LeaderCountryCode = 1,
                    LocationCode = "200"
                },
                ChildEntities = new List<UnitUSUpdatedEventEntity>
                {
                    new()
                    {
                        UnitID = childId,
                        ParentID = parentId,
                        LocationCode = "200"
                    },
                    new()
                    {
                        UnitID = childIdSecond,
                        ParentID = parentId,
                        LocationCode = "300"
                    },
                }
            }, default).ConfigureAwait(false);

            var after = await repository.QueryNoTracking<RmUnit>().ToListAsync().ConfigureAwait(false);

            after.FirstOrDefault(x => x.Id == parentId)?.Name.ShouldBe("张三丰组");
            after.FirstOrDefault(x => x.Id == parentId)?.LocationCode.ShouldBe("200");

            after.FirstOrDefault(x => x.Id == childId)?.LocationCode.ShouldBe("200");
            after.FirstOrDefault(x => x.Id == childIdSecond)?.LocationCode.ShouldBe("300");
        });
    }

    [Fact]
    public async Task CanHandleUserAccountCNAddedEvent()
    {
        var staffId = Guid.Parse("E168D27A-99F1-4D12-972F-0693A664F194");
        
        await _foundationUtil.AddStaffAsync(staffId, countryCode: CountryCode.CN).ConfigureAwait(false);

        await Run<IFoundationService, IRepository>(async (f, repository) =>
        {
            var before = await repository.QueryNoTracking<RmStaff>().ToListAsync().ConfigureAwait(false);

            before.Count.ShouldBe(1);
            before.FirstOrDefault()?.UserName.ShouldBeNull();
            before.FirstOrDefault()?.UserId.ShouldBeNull();

            await f.HandleUserAccountCNAddedEventAsync(new UserAccountCNAddedEvent
            {
                StaffID = staffId,
                UserID = staffId,
                UserName = "张三"
            }, default).ConfigureAwait(false);

            var after = await repository.QueryNoTracking<RmStaff>().ToListAsync().ConfigureAwait(false);

            after.FirstOrDefault()?.UserName.ShouldBe("张三");
            after.FirstOrDefault()?.UserId.ShouldBe(staffId);
        });
    }

    [Fact]
    public async Task CanHandleUserAccountUSAddedEvent()
    {
        var staffId = Guid.Parse("E168D27A-99F1-4D12-972F-0693A664F194");
        await _foundationUtil.AddStaffAsync(staffId, countryCode: CountryCode.US).ConfigureAwait(false);

        await Run<IFoundationService, IRepository>(async (f, repository) =>
        {
            var before = await repository.QueryNoTracking<RmStaff>().ToListAsync().ConfigureAwait(false);

            before.Count.ShouldBe(1);
            before.FirstOrDefault()?.UserName.ShouldBeNull();
            before.FirstOrDefault()?.UserId.ShouldBeNull();

            await f.HandleUserAccountUSAddedEventAsync(new UserAccountUSAddedEvent
            {
                StaffID = staffId,
                UserID = staffId,
                UserName = "张三"
            }, default).ConfigureAwait(false);

            var after = await repository.QueryNoTracking<RmStaff>().ToListAsync().ConfigureAwait(false);

            after.FirstOrDefault()?.UserName.ShouldBe("张三");
            after.FirstOrDefault()?.UserId.ShouldBe(staffId);
        });
    }

    [Theory]
    [InlineData("E168D27A-99F1-4D12-972F-0693A664F194", "E168D27A-99F1-4D12-972F-0693A664F194", 1)]
    [InlineData("E168D27A-99F1-4D12-972F-0693A664F195", "E168D27A-99F1-4D12-972F-0693A664F195", 1)]
    [InlineData("E168D27A-99F1-4D12-972F-0693A664F196", "E168D27A-99F1-4D12-972F-0693A664F196", 1)]
    [InlineData("E168D27A-99F1-4D12-972F-0693A664F198", "E168D27A-99F1-4D12-972F-0693A664F198", 1)]
    public async Task CanInsertFoundationEntityAsync(Guid id, Guid uId, int expectValue)
    {
        await RunWithUnitOfWork<IRepository>(async repository =>
            {
                await _foundationUtil.AddPositionAsync(id, uId).ConfigureAwait(false);
                await _foundationUtil.AddStaffAsync(id, uId).ConfigureAwait(false);
                await _foundationUtil.AddUnitAsync(id, uId).ConfigureAwait(false);
                await _foundationUtil.AddUsLocationAsync(id).ConfigureAwait(false);
            }
        );
        await RunWithUnitOfWork<IRepository>(async repository =>
            {
                var positions = await repository.Query<RmPosition>().ToListAsync().ConfigureAwait(false);
                var staffs = await repository.Query<RmStaff>().ToListAsync().ConfigureAwait(false);
                var units = await repository.Query<RmUnit>().ToListAsync().ConfigureAwait(false);
                var usLocations = await repository.Query<RmUsLocation>().ToListAsync().ConfigureAwait(false);

                positions.Count.ShouldBe(expectValue);
                positions.FirstOrDefault()?.Id.ShouldBe(id);
                positions.FirstOrDefault()?.UnitId.ShouldBe(uId);

                staffs.Count.ShouldBe(expectValue);
                staffs.FirstOrDefault()?.Id.ShouldBe(id);
                staffs.FirstOrDefault()?.UserId.ShouldBe(uId);

                units.Count.ShouldBe(expectValue);
                positions.FirstOrDefault()?.Id.ShouldBe(id);
                positions.FirstOrDefault()?.UnitId.ShouldBe(uId);

                usLocations.Count.ShouldBe(expectValue);
                positions.FirstOrDefault()?.Id.ShouldBe(id);
                positions.FirstOrDefault()?.UnitId.ShouldBe(uId);
            }
        );
    }
}