using AutoMapper;
using SugarTalk.Core.Domain.Account;
using SugarTalk.Messages.Dtos.Users;

namespace SugarTalk.Core.Mapping
{
    public class UserMapping : Profile
    {
        public UserMapping()
        {
            CreateMap<User, SignedInUserDto>();
            CreateMap<UserSession, UserSessionDto>();
            CreateMap<UserSessionDto, UserSession>();
        }
    }
}