using System;

namespace SugarTalk.Core.Entities
{
    public class User : IEntity
    {
        public Guid Id { get; set; }
        public string ThirdPartyId { get; set; }
        public string Email { get; set; }
        public string Picture { get; set; }
        public string DisplayName { get; set; }
    }
}