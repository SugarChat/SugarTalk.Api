using System.Threading.Tasks;
using System.Collections.Concurrent;
using Kurento.NET;

namespace SugarTalk.Core.Services.Kurento
{
    public class MeetingSessionManager
    {
        private readonly KurentoClient _client;
        private readonly IMeetingDataProvider _meetingDataProvider;
        private readonly ConcurrentDictionary<string, MeetingSession> _meetingSessions;

        public MeetingSessionManager(KurentoClient client, IMeetingDataProvider meetingDataProvider)
        {
            _client = client;
            _meetingDataProvider = meetingDataProvider;
            _meetingSessions = new ConcurrentDictionary<string, MeetingSession>();
        }
        
        public async Task<MeetingSession> GetMeetingSessionAsync(string meetingNumber)
        {
            _meetingSessions.TryGetValue(meetingNumber, out var meetingSession);

            if (meetingSession != null) return meetingSession;
            {
                var pipeline = await _client.CreateAsync(new MediaPipeline());

                var meeting = await _meetingDataProvider.GetMeetingByNumber(meetingNumber).ConfigureAwait(false);
                
                meetingSession = new MeetingSession
                {
                    MeetingId = meeting.Id,
                    MeetingNumber = meetingNumber,
                    MeetingType = meeting.MeetingType,
                    Pipeline = pipeline,
                    UserSessions = new ConcurrentDictionary<string, UserSession>()
                };
                
                _meetingSessions.TryAdd(meetingNumber, meetingSession);
            }
            
            return meetingSession;
        }

        /// <summary>
        /// 若没有人员则释放房间资源
        /// </summary>
        /// <param name="meetingNumber"></param>
        /// <returns></returns>
        public async Task TryRemoveRoomAsync(string meetingNumber)
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