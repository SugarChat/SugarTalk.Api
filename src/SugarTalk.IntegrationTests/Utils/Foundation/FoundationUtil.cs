using System;
using System.Threading.Tasks;
using Autofac;
using HR.Message.Contract.Command;
using Smarties.Messages.Enums.Foundation;
using SugarTalk.Core.Data;
using SugarTalk.Core.Domain.Foundation;

namespace SugarTalk.IntegrationTests.Utils.Foundation;

public class FoundationUtil : TestUtil
{
    public FoundationUtil(ILifetimeScope scope) : base(scope)
    {
    }

    public async Task AddPositionAsync(Guid id, Guid? unitId = null, string? name = null, string? description = null,
        CountryCode? countryCode = null, bool? isActive = null)
    {
        await RunWithUnitOfWork<IRepository>(async repository =>
        {
            await repository.InsertAsync(new RmPosition
            {
                Id = id,
                UnitId = unitId,
                Name = name,
                Description = description,
                CountryCode = countryCode,
                IsActive = isActive
            });
        });
    }

    public async Task AddStaffAsync(Guid id, Guid? userId = null, string? userName = null, string? nameCnLong = null,
        string? nameEnLong = null, Guid? companyId = null, string? companyName = null, Guid? departmentId = null, string departmentName = null,
        Guid? groupId = null, string? groupName = null, Guid? positionId = null, string? positionName = null,
        PositionCnStatus? positionCnStatus = null, PositionUsStatus? positionUsStatus = null,
        Guid? locationId = null, string? locationName = null, Guid? superiorId = null, string? phoneNumber = null,
        string? email = null, string? workPlace = null,
        CountryCode? countryCode = null)
    {
        await RunWithUnitOfWork<IRepository>(async repository =>
        {
            await repository.InsertAsync(new RmStaff
            {
                Id = id,
                UserId = userId,
                UserName = userName,
                NameCNLong = nameCnLong,
                NameENLong = nameEnLong,
                CompanyId = companyId,
                CompanyName = companyName,
                DepartmentId = departmentId,
                DepartmentName = departmentName,
                GroupID = groupId,
                PositionId = positionId,
                PositionName = positionName,
                PositionCNStatus = positionCnStatus,
                PositionUSStatus = positionUsStatus,
                SuperiorId = superiorId,
                GroupName = groupName,
                LocationId = locationId,
                LocationName = locationName,
                PhoneNumber = phoneNumber,
                Email = email,
                WorkPlace = workPlace,
                CountryCode = countryCode,
            });
        });
    }
    
    public async Task AddUnitAsync(Guid id, Guid? userId = null, string? name = null, UnitType? typeId = null,
        Guid? parentId = null, Guid? leaderId = null, CountryCode? leaderCountryCode = null, string? locationCode = null, string? description = null,
            CountryCode? countryCode = null, bool? isActive = null)
    {
        await RunWithUnitOfWork<IRepository>(async repository =>
        {
            await repository.InsertAsync(new RmUnit
            {
                Id = id,
                Name = name,
                TypeId = typeId,
                ParentId = parentId,
                LeaderId = leaderId,
                LeaderCountryCode = leaderCountryCode,
                LocationCode = locationCode,
                Description = description,
                CountryCode = countryCode,
                IsActive = isActive
            });
        });
    }
    
    public async Task AddUsLocationAsync(Guid id, string? name = null, string? warehouseCode = null, string locationCode = null,
        UsLocationType? type = null, UsLocationStatus? status = null)
    {
        await RunWithUnitOfWork<IRepository>(async repository =>
        {
            await repository.InsertAsync(new RmUsLocation()
            {
                Id = id,
                Name = name,
                LocationCode = locationCode,
                Status = status,
                Type = type,
                WarehouseCode = warehouseCode,
            });
        });
    }
}