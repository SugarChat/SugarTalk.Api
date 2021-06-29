using System.Collections.Concurrent;
using System.Threading.Tasks;
using Kurento.NET;

namespace SugarTalk.Core.Services.Kurento
{
    public class RoomSessionManager
    {
        private readonly ConcurrentDictionary<string, RoomSession> _roomSessions;
        private readonly KurentoClient _client;
 

        public RoomSessionManager(KurentoClient client)
        {
            _client = client;
            _roomSessions = new ConcurrentDictionary<string, RoomSession>();
        }
        
        public async Task<RoomSession> GetRoomSessionAsync(string roomID)
        {
            if (!_roomSessions.TryGetValue(roomID, out RoomSession roomSession))
            {
                var pipeline = await _client.CreateAsync(new MediaPipeline());
                roomSession = new RoomSession()
                {
                    RoomID = roomID,
                    Pipeline = pipeline,
                    UserSessions = new ConcurrentDictionary<string, UserSession>()
                };
                _roomSessions.TryAdd(roomID, roomSession);
            }
            return roomSession;
        }

        /// <summary>
        /// 若没有人员则释放房间资源
        /// </summary>
        /// <param name="roomID"></param>
        /// <returns></returns>
        public async Task TryRemoveRoomAsync(string roomID)
        {
            if (_roomSessions.TryGetValue(roomID, out RoomSession roomSession))
            {
                if (roomSession.UserSessions.IsEmpty)
                {
                    await roomSession.Pipeline.ReleaseAsync();
                    _roomSessions.TryRemove(roomID, out _);
                }
            }
        }
    }
}