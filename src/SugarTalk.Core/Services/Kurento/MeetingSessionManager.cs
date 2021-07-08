using System.Threading.Tasks;
using System.Collections.Concurrent;
using Kurento.NET;
using SugarTalk.Core.Entities;
using SugarTalk.Messages.Dtos;
using SugarTalk.Messages.Dtos.Meetings;

namespace SugarTalk.Core.Services.Kurento
{
    public class MeetingSessionManager
    {
        private readonly KurentoClient _client;
        private readonly ConcurrentDictionary<string, MeetingSession> _meetingSessions;

        public MeetingSessionManager(KurentoClient client)
        {
            _client = client;
            _meetingSessions = new ConcurrentDictionary<string, MeetingSession>();
        }
        
        public async Task<MeetingSession> GetOrCreateMeetingSessionAsync(MeetingDto meeting)
        {
            _meetingSessions.TryGetValue(meeting.MeetingNumber, out var meetingSession);

            if (meetingSession != null) return meetingSession;
            {
                var pipeline = await _client.CreateAsync(new MediaPipeline());

                meetingSession = new MeetingSession
                {
                    MeetingId = meeting.Id,
                    MeetingType = meeting.MeetingType,
                    MeetingNumber = meeting.MeetingNumber,
                    Pipeline = pipeline,
                    UserSessions = new ConcurrentDictionary<string, UserSession>()
                };
                
                _meetingSessions.TryAdd(meeting.MeetingNumber, meetingSession);
            }
            
            return meetingSession;
        }

        /// <summary>
        /// 若没有人员则释放房间资源
        /// </summary>
        /// <param name="meetingNumber"></param>
        /// <returns></returns>
        public async Task TryRemoveMeetingAsync(string meetingNumber)
        {
            if (_meetingSessions.TryGetValue(meetingNumber, out var meetingSession))
            {
                if (meetingSession.UserSessions.IsEmpty)
                {
                    await meetingSession.Pipeline.ReleaseAsync();
                    
                    _meetingSessions.TryRemove(meetingNumber, out _);
                }
            }
        }
    }
}