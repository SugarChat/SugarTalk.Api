using Mediator.Net.Contracts;
using SugarTalk.Messages.Dtos.Authentication;

namespace SugarTalk.Messages.Requests.Authentication
{
    public class GetGoogleAccessTokenRequest : IRequest
    {
        public string Code { get; set; }
        public string RedirectUri { get; set; }
    }
    
    public class GetGoogleAccessTokenResponse : IResponse
    {
        public GoogleAccessTokenDto AccessToken { get; set; }
    }
}