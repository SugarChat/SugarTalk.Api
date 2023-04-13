using System;

namespace SugarTalk.Core.Domain
{
    public interface IEntity
    {
        Guid Id { get; set; }
    }
}