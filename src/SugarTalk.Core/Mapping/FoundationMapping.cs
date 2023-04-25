using AutoMapper;
using HR.Message.Contract.Event;
using SugarTalk.Core.Domain.Foundation;

namespace SugarTalk.Core.Mapping;

public class FoundationMapping : Profile
{
    public FoundationMapping()
    {
        CreateMap<CNStaff, RmStaff>()
            .ForMember(dest => dest.PositionCNStatus, opt => opt.MapFrom(x => x.PositionStatus));
        CreateMap<USStaff, RmStaff>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(x => x.StaffID))
            .ForMember(dest => dest.NameENLong, opt => opt.MapFrom(x => x.PayrollName))
            .ForMember(dest => dest.NameCNLong, opt => opt.MapFrom(x => x.PayrollName))
            .ForMember(dest => dest.PositionUSStatus, opt => opt.MapFrom(x => x.PositionStatus))
            .ForMember(dest => dest.PhoneNumber, opt => opt.MapFrom(x => x.WorkPhone));

        CreateMap<CNUnit, RmUnit>();
        CreateMap<USUnit, RmUnit>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(x => x.UnitID));

        CreateMap<CNPosition, RmPosition>();
        CreateMap<USPosition, RmPosition>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(x => x.PositionID));

        CreateMap<USLocation, RmUsLocation>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(x => x.LocationID))
            .ForMember(dest => dest.Type, opt => opt.MapFrom(x => x.LocationType));

        CreateMap<LocationUSUpdatedEvent, RmUsLocation>()
            .ForMember(dest => dest.Type, opt => opt.MapFrom(x => x.LocationType));
        CreateMap<LocationUSAddedEvent, RmUsLocation>()
            .ForMember(dest => dest.Type, opt => opt.MapFrom(x => x.LocationType))
            .ForMember(dest => dest.Id, opt => opt.MapFrom(x => x.LocationID));

        CreateMap<PositionCNUpdatedEvent, RmPosition>();
        CreateMap<PositionCNAddedEvent, RmPosition>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(x => x.PositionID));
        CreateMap<PositionUSUpdatedEvent, RmPosition>();
        CreateMap<PositionUSAddedEvent, RmPosition>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(x => x.PositionID));

        CreateMap<UserAccountUSAddedEvent, RmStaff>();
        CreateMap<UserAccountCNAddedEvent, RmStaff>();
        CreateMap<StaffUSAddedEvent, RmStaff>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(x => x.StaffID))
            .ForMember(dest => dest.NameENLong, opt => opt.MapFrom(x => x.PayrollName))
            .ForMember(dest => dest.NameCNLong, opt => opt.MapFrom(x => x.PayrollName))
            .ForMember(dest => dest.PositionUSStatus, opt => opt.MapFrom(x => x.PositionStatus))
            .ForMember(dest => dest.PhoneNumber, opt => opt.MapFrom(x => x.WorkPhone));

        CreateMap<StaffCNUpdatedEvent, RmStaff>()
            .ForMember(dest => dest.PositionCNStatus, opt => opt.MapFrom(x => x.PositionStatus));

        CreateMap<StaffCNAddedEvent, RmStaff>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(x => x.StaffID))
            .ForMember(dest => dest.PositionCNStatus, opt => opt.MapFrom(x => x.PositionStatus));

        CreateMap<StaffUSUpdatedEvent, RmStaff>()
            .ForMember(dest => dest.NameCNLong, opt => opt.MapFrom(x => x.PayrollName))
            .ForMember(dest => dest.NameENLong, opt => opt.MapFrom(x => x.PayrollName))
            .ForMember(dest => dest.PhoneNumber, opt => opt.MapFrom(x => x.WorkPhone))
            .ForMember(dest => dest.PositionUSStatus, opt => opt.MapFrom(x => x.PositionStatus));

        CreateMap<UnitUSAddedEvent, RmUnit>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(x => x.UnitID));

        CreateMap<UnitUSUpdatedEvent, RmUnit>();
        CreateMap<UnitUSUpdatedEventEntity, RmUnit>();
        CreateMap<UnitCNUpdatedEvent, RmUnit>();
        CreateMap<UnitCNUpdatedEventEntity, RmUnit>();
        CreateMap<UnitCNMovedEvent, RmUnit>();
        CreateMap<UnitCNMovedEventEntity, RmUnit>();
        CreateMap<UnitCNAddedEvent, RmUnit>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(x => x.UnitID));
    }
}