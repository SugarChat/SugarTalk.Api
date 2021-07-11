using System.Threading;
using System.Threading.Tasks;

namespace SugarTalk.Core.Services.Authentication
{
    public interface ITokenService
    {
        Task<T> GetPayloadFromMemoryOrDb<T>(string token, CancellationToken cancellationToken = default);
    }
    
    public class TokenService : ITokenService
    {

        private readonly ITokenDataProvider _tokenDataProvider;

        public TokenService(ITokenDataProvider tokenDataProvider)
        {
            _tokenDataProvider = tokenDataProvider;
        }

        public async Task<T> GetPayloadFromMemoryOrDb<T>(string token, CancellationToken cancellationToken = default)
        {
            var userInfo = _tokenDataProvider.GetPayloadFromMemory<T>(token);

            if (userInfo != null) return userInfo;
            {
                return await _tokenDataProvider.GetPayloadFromMongoDb<T>(token, cancellationToken)
                    .ConfigureAwait(false);
            }
        }
    }
}