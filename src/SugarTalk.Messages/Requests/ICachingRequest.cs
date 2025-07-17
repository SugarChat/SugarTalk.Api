using System;
using Mediator.Net.Contracts;

namespace SugarTalk.Messages.Requests;

public interface ICachingRequest : IRequest
{
    string GetCacheKey();
    
    TimeSpan? GetCacheExpiration();
}