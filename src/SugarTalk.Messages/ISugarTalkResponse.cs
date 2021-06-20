using Mediator.Net.Contracts;

namespace SugarTalk.Messages
{
    public interface ISugarTalkResponse : IResponse
    {
        public int Code { get; set;  }

        public string Message { get; set; }
        
    }
    
    public interface ISugarTalkResponse<T>: ISugarTalkResponse
    {
        T Data { get; set; }
    }
}