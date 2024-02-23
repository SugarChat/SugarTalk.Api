using System;

namespace SugarTalk.Core.Services.Exceptions;

public class GetEgressInfoListResponseNotFoundException:Exception
{
    public GetEgressInfoListResponseNotFoundException() : base("The response returned by liveKitClient could not be obtained")
    {
    }
}