using System;

namespace SugarTalk.Core.Services.Exceptions;

public class StopEgressResponseNotFoundException:Exception
{
    public StopEgressResponseNotFoundException():base("The response returned by liveKitClient could not be obtained")
    {
    }
}