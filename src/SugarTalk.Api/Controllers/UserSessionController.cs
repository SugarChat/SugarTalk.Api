using System.Threading.Tasks;
using Mediator.Net;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SugarTalk.Messages;
using SugarTalk.Messages.Commands.UserSessions;
using SugarTalk.Messages.Dtos.Users;
using SugarTalk.Messages.Responses;

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
        public async Task<SugarTalkResponse<UserSessionDto>> ChangeAudio(ChangeAudioCommand changeAudioCommand)
        {
            return await _mediator.SendAsync<ChangeAudioCommand, SugarTalkResponse<UserSessionDto>>(changeAudioCommand);
        }
        
        [Route("screen/share"), HttpPost]
        public async Task<SugarTalkResponse<UserSessionDto>> ChangeAudio(ShareScreenCommand shareScreenCommand)
        {
            return await _mediator.SendAsync<ShareScreenCommand, SugarTalkResponse<UserSessionDto>>(shareScreenCommand);
        }
    }
}