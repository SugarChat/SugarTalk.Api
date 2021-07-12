using System;
using SugarTalk.Messages.Enums;

namespace SugarTalk.Messages.Dtos.Users
{
    public class SignedInUserDto
    {
        public Guid Id { get; set; }
        
        public string ThirdPartyId { get; set; }
        
        public ThirdPartyFrom ThirdPartyFrom { get; set; }
        
        public string Email { get; set; }
        
        public string Picture { get; set; }

        public string DisplayName { get; set; }
    }
}