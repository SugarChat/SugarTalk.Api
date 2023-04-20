using AutoMapper;
using SugarTalk.Core.Domain.Account;
using SugarTalk.Messages.Dtos.Users;

namespace SugarTalk.Core.Mapping
{
    public class UserMapping : Profile
    {
        public UserMapping()
        {
            CreateMap<UserAccount, SignedInUserDto>();
            CreateMap<UserSession, UserSessionDto>().ReverseMap();
            CreateMap<UserAccount, UserAccountDto>().ReverseMap();
        }
    }
}