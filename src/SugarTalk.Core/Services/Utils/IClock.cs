using System;
using SugarTalk.Core.Ioc;

namespace SugarTalk.Core.Services.Utils
{
    public interface IClock : IScopedDependency
    {
        DateTimeOffset Now { get; }
    }
}