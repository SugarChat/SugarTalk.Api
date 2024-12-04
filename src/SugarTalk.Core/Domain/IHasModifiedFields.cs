using System;

namespace SugarTalk.Core.Domain;

public interface IHasModifiedFields
{
    int LastModifiedBy { get; set; }
    
    DateTimeOffset LastModifiedDate { get; set; }
}