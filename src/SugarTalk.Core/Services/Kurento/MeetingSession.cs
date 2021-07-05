using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Kurento.NET;

namespace SugarTalk.Core.Services.Kurento
{
    public class MeetingSession
    {
        public Guid MeetingId { set; get; }
        public MediaPipeline Pipeline { set; get; }
        public ConcurrentDictionary<string, UserSession> UserSessions { set; get; }
        
        public async Task RemoveAsync(string id)
        {
            if (UserSessions.TryRemove(id, out UserSession user))
            {
                //释放自身资源
                if (user.SendEndPoint != null)
                {
                    await user.SendEndPoint.ReleaseAsync();
                }
                if (user.ReceviedEndPoints != null)
                {
                    foreach (var endPoint in user.ReceviedEndPoints.Values)
                    {
                        await endPoint.ReleaseAsync();
                    }
                }
                //释放其他人员的资源
                foreach (var u in UserSessions.Values)
                {
                    if (u.ReceviedEndPoints.TryRemove(id, out WebRtcEndpoint endpoint))
                    {
                        await endpoint.ReleaseAsync();
                    }
                }
            }
        }
        public IEnumerable<UserSession> GetOtherUsers(string connectionId) => UserSessions.Values.Where(x => x.Id != connectionId);
    }
}