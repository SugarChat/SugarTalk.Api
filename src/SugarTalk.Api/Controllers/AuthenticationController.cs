using System.Threading.Tasks;
using Mediator.Net;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SugarTalk.Messages.Requests.Authentication;

namespace SugarTalk.Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AuthenticationController : ControllerBase
    {
        private readonly IMediator _mediator;

        public AuthenticationController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [Route("google/accessToken"), HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(GetGoogleAccessTokenResponse))]
        public async  Task<IActionResult> GetGoogleAccessToken([FromQuery] GetGoogleAccessTokenRequest request)
        {
            var response = await _mediator.RequestAsync<GetGoogleAccessTokenRequest, GetGoogleAccessTokenResponse>(request);

            return Ok(response);
        }
        
        [Route("facebook/accessToken"), HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(GetFacebookAccessTokenResponse))]
        public async  Task<IActionResult> GetFacebookAccessToken([FromQuery] GetFacebookAccessTokenRequest request)
        {
            var response = await _mediator.RequestAsync<GetFacebookAccessTokenRequest, GetFacebookAccessTokenResponse>(request);

            return Ok(response);
        }
    }
}