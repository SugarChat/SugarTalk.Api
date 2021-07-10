using Mediator.Net.Contracts;
using SugarTalk.Messages.Dtos.Authentication;

namespace SugarTalk.Messages.Requests.Authentication
{
    public class GetFacebookAccessTokenRequest : IRequest
    {
        public string Code { get; set; }
    }
    
    public class GetFacebookAccessTokenResponse : IResponse
    {
        public FacebookAccessTokenDto AccessToken { get; set; }
    }
}