using Mediator.Net.Contracts;

namespace SugarTalk.Messages.Requests.Account;

public class UploadPhotoCommand : ICommand
{
    public string FileName { get; set; }
    
    public byte[] FileContent { get; set; }
}

public class UploadPhotoResponse : IResponse
{
    public string Url { get; set; }
}