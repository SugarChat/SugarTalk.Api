using System;

namespace SugarTalk.Core.Services.Utils
{
    public class Clock : IClock
    {
        public DateTimeOffset Now => DateTimeOffset.Now;
    }
}