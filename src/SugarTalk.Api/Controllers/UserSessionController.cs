using System.Threading.Tasks;
using Mediator.Net;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SugarTalk.Messages.Commands.UserSessions;

namespace SugarTalk.Api.Controllers
{
    [Authorize]
    [ApiController]
    [Route("[controller]")]
    public class UserSessionController: ControllerBase
    {
        private readonly IMediator _mediator;
        
        public UserSessionController(IMediator mediator)
        {
            _mediator = mediator;
        }
        
        [Route("changeAudio"), HttpPost]
        public async Task<IActionResult> ChangeAudio(ChangeAudioCommand changeAudioCommand)
        {
            await _mediator.SendAsync(changeAudioCommand);

            return Ok();
        }
    }
}