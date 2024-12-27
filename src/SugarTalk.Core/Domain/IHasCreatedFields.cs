using System;

namespace SugarTalk.Core.Domain;

public interface IHasCreatedFields
{
    int CreatedBy { get; set; }
    
    DateTimeOffset CreatedDate { get; set; }
}