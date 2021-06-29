using System;

namespace SugarTalk.Core.Entities
{
    public interface IEntity
    {
        Guid Id { get; set; }
    }
}