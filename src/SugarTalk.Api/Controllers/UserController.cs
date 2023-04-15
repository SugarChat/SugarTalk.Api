using System.Threading.Tasks;
using Mediator.Net;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SugarTalk.Messages.Requests.Users;

namespace SugarTalk.Api.Controllers
{
    [Authorize]
    [ApiController]
    [Route("[controller]")]
    public class UserController: ControllerBase
    {
        private readonly IMediator _mediator;
        
        public UserController(IMediator mediator)
        {
            _mediator = mediator;
        }
        
        [Route("signin"), HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(SignInFromThirdPartyResponse))]
        public async Task<IActionResult> SignInFromThirdParty()
        {
            var response = await _mediator.RequestAsync<SignInFromThirdPartyRequest, SignInFromThirdPartyResponse>(
                new SignInFromThirdPartyRequest());

            return Ok(response);
        }
    }
}