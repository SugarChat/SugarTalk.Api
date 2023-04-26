using AutoMapper;
using SugarTalk.Core.Domain.Account;
using SugarTalk.Messages.Dto.Users;

namespace SugarTalk.Core.Mapping
{
    public class UserMapping : Profile
    {
        public UserMapping()
        {
            CreateMap<UserAccount, UserAccountDto>().ReverseMap();
        }
    }
}