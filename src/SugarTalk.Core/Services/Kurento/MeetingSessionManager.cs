using System;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using Kurento.NET;

namespace SugarTalk.Core.Services.Kurento
{
    public class MeetingSessionManager
    {
        private readonly KurentoClient _client;
        private readonly IMeetingDataProvider _meetingDataProvider;
        private readonly ConcurrentDictionary<Guid, MeetingSession> _meetingSessions;

        public MeetingSessionManager(KurentoClient client, IMeetingDataProvider meetingDataProvider)
        {
            _client = client;
            _meetingDataProvider = meetingDataProvider;
            _meetingSessions = new ConcurrentDictionary<Guid, MeetingSession>();
        }
        
        public async Task<MeetingSession> GetMeetingSessionAsync(Guid meetingId)
        {
            _meetingSessions.TryGetValue(meetingId, out var meetingSession);

            if (meetingSession != null) return meetingSession;
            {
                var pipeline = await _client.CreateAsync(new MediaPipeline());

                var meeting = await _meetingDataProvider.GetMeetingById(meetingId).ConfigureAwait(false);
                
                meetingSession = new MeetingSession
                {
                    MeetingId = meeting.Id,
                    Pipeline = pipeline,
                    UserSessions = new ConcurrentDictionary<string, UserSession>()
                };
                
                _meetingSessions.TryAdd(meetingId, meetingSession);
            }
            
            return meetingSession;
        }

        /// <summary>
        /// 若没有人员则释放房间资源
        /// </summary>
        /// <param name="meetingId"></param>
        /// <returns></returns>
        public async Task TryRemoveRoomAsync(Guid meetingId)
        {
            if (_meetingSessions.TryGetValue(meetingId, out var meetingSession))
            {
                if (meetingSession.UserSessions.IsEmpty)
                {
                    await meetingSession.Pipeline.ReleaseAsync();
                    
                    _meetingSessions.TryRemove(meetingId, out _);
                }
            }
        }
    }
}