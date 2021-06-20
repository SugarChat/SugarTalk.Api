using System;

namespace SugarTalk.Messages
{
    public class SugarTalkResponse<T> : ISugarTalkResponse<T>
    {
        public int Code { get; set; } = 20000;
        public string Message { get; set; } = "Success";
        public T Data { get; set; }
    }
}