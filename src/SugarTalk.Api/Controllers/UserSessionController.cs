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
        
        [Route("audio/change"), HttpPost]
        public async Task<ChangeAudioResponse> ChangeAudio(ChangeAudioCommand changeAudioCommand)
        {
            return await _mediator.SendAsync<ChangeAudioCommand, ChangeAudioResponse>(changeAudioCommand);
        }
        
        [Route("screen/share"), HttpPost]
        public async Task<ShareScreenResponse> ChangeAudio(ShareScreenCommand shareScreenCommand)
        {
            return await _mediator.SendAsync<ShareScreenCommand, ShareScreenResponse>(shareScreenCommand);
        }
    }
}