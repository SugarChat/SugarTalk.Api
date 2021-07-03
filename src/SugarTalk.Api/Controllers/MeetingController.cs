using System.Threading.Tasks;
using Mediator.Net;
using Microsoft.AspNetCore.Mvc;
using SugarTalk.Core.Data;
using SugarTalk.Messages;

namespace SugarTalk.Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class MeetingController: ControllerBase
    {
        private readonly IMediator _mediator;
        
        public MeetingController(IMediator mediator, IRepository repository)
        {
            _mediator = mediator;
        }
        
        [Route("schedule"), HttpPost]
        public async Task<SugarTalkResponse<MeetingDto>> ScheduleMeeting(ScheduleMeetingCommand scheduleMeetingCommand)
        {
            return await _mediator.SendAsync<ScheduleMeetingCommand, SugarTalkResponse<MeetingDto>>(scheduleMeetingCommand);
        }
    }
}