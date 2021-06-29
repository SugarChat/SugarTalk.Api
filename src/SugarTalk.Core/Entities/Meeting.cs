using System;

namespace SugarTalk.Core.Entities
{
    public class Meeting: IEntity
    {
        public Guid Id { get; set; }
        
        public Guid SourceId { get; set; }

        public string SourceDescription { get; set; }

        public Guid TargetId { get; set; }

        public DateTimeOffset CreateAt { get; set; }
        
        public Guid CreatedBy { get; set; }
    }
}