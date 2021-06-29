using System;
using System.Threading;
using System.Threading.Tasks;
using Mediator.Net.Context;
using Mediator.Net.Contracts;
using SugarTalk.Core.Services;
using SugarTalk.Core.Services.Kurento;
using SugarTalk.Messages;

namespace SugarTalk.Core.Handlers.CommandHandlers
{
    public class ScheduleMeetingCommandHandler: ICommandHandler<ScheduleMeetingCommand, SugarTalkResponse<MeetingDto>>
    {
        private readonly RoomSessionManager _roomSessionManager;

        public ScheduleMeetingCommandHandler(RoomSessionManager roomSessionManager)
        {
            _roomSessionManager = roomSessionManager;
        }
        
        public async Task<SugarTalkResponse<MeetingDto>> Handle(IReceiveContext<ScheduleMeetingCommand> context, CancellationToken cancellationToken)
        {
            var random = new Random();
            var meetingNumber = random.Next(1000, 2000).ToString();
            await _roomSessionManager.GetRoomSessionAsync(meetingNumber);
            return new SugarTalkResponse<MeetingDto>()
            {
                Data = new MeetingDto()
                {
                    MeetingId = meetingNumber
                }
            };
        }
    }
}