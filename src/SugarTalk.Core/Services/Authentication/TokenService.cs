using System.Threading;
using System.Threading.Tasks;
using SugarTalk.Messages.Enums;

namespace SugarTalk.Core.Services.Authentication
{
    public interface ITokenService
    {
        Task<T> GetPayloadFromMemoryOrDb<T>(string token, ThirdPartyFrom thirdPartyFrom,
            CancellationToken cancellationToken = default);

        Task PersistPayloadToMemoryAndDb<T>(string token, ThirdPartyFrom thirdPartyFrom, T payload,
            CancellationToken cancellationToken = default);
    }
    
    public class TokenService : ITokenService
    {

        private readonly ITokenDataProvider _tokenDataProvider;

        public TokenService(ITokenDataProvider tokenDataProvider)
        {
            _tokenDataProvider = tokenDataProvider;
        }

        public async Task<T> GetPayloadFromMemoryOrDb<T>(string token, ThirdPartyFrom thirdPartyFrom, 
            CancellationToken cancellationToken = default)
        {
            var userInfo = _tokenDataProvider.GetPayloadFromMemory<T>(token);

            if (userInfo != null) return userInfo;
            {
                userInfo = await _tokenDataProvider.GetPayloadFromMongoDb<T>(token, thirdPartyFrom, cancellationToken)
                    .ConfigureAwait(false);
                
                if (userInfo != null)
                    _tokenDataProvider.PersistPayloadToMemory(token, userInfo);

                return userInfo;
            }
        }

        public async Task PersistPayloadToMemoryAndDb<T>(string token, ThirdPartyFrom thirdPartyFrom, T payload, 
            CancellationToken cancellationToken = default)
        {
            if (payload == null) return;

            _tokenDataProvider.PersistPayloadToMemory(token, payload);

            await _tokenDataProvider.PersistPayloadToMongoDb(token, thirdPartyFrom, payload, cancellationToken)
                .ConfigureAwait(false);
        }
    }
}