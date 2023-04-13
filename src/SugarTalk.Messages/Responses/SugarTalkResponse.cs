using System.Net;
using Mediator.Net.Contracts;

namespace SugarTalk.Messages.Responses;

public class SugarTalkResponse<T> : SugarTalkResponse
{
    public T Data { get; set; }
}

public class SugarTalkResponse : IResponse
{
    public HttpStatusCode Code { get; set; }
    
    public string Msg { get; set; }
}
