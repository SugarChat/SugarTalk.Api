using System;

namespace SugarTalk.Messages.Dtos.Users
{
    public class SignedInUserDto
    {
        public Guid Id { get; set; }
        
        public string ThirdPartyId { get; set; }
        
        public string DisplayName { get; set; }
        
        public string Picture { get; set; }
    }
}