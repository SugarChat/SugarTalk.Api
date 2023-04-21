using System.Threading.Tasks;
using Mediator.Net;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
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
        
        [Route("audio/change"), HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ChangeAudioResponse))]
        public async Task<IActionResult> ChangeAudio(ChangeAudioCommand changeAudioCommand)
        {
            var response = await _mediator.SendAsync<ChangeAudioCommand, ChangeAudioResponse>(changeAudioCommand);

            return Ok(response);
        }
        
        [Route("screen/share"), HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ShareScreenResponse))]
        public async Task<IActionResult> ChangeAudio(ShareScreenCommand shareScreenCommand)
        {
            var response = await _mediator.SendAsync<ShareScreenCommand, ShareScreenResponse>(shareScreenCommand);

            return Ok(response);
        }
    }
}