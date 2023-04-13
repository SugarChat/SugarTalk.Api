using System.Threading.Tasks;
using Mediator.Net;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SugarTalk.Messages;
using SugarTalk.Messages.Dtos.Users;
using SugarTalk.Messages.Requests.Users;
using SugarTalk.Messages.Responses;

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
        public async Task<SugarTalkResponse<SignedInUserDto>> SignInFromThirdParty()
        {
            return await _mediator.RequestAsync<SignInFromThirdPartyRequest, SugarTalkResponse<SignedInUserDto>>(
                new SignInFromThirdPartyRequest());
        }
    }
}