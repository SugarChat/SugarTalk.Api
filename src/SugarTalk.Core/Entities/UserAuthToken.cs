using System;
using SugarTalk.Messages.Enums;

namespace SugarTalk.Core.Entities
{
    public class UserAuthToken : IEntity
    {
        public Guid Id { get; set; }
        
        public string AccessToken { get; set; }
        
        public DateTimeOffset ExpiredAt { get; set; }
        
        public ThirdPartyFrom ThirdPartyFrom { get; set; }
        
        public string Payload { get; set; }
    }
}