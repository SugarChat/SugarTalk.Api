using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using MongoDB.Driver.Linq;
using Newtonsoft.Json;
using SugarTalk.Core.Data.MongoDb;
using SugarTalk.Core.Entities;
using SugarTalk.Messages.Enums;

namespace SugarTalk.Core.Services.Authentication
{
    public interface ITokenDataProvider
    {
        T GetPayloadFromMemory<T>(string token);
        
        Task<T> GetPayloadFromMongoDb<T>(string token, ThirdPartyFrom thirdPartyFrom, CancellationToken cancellationToken);

        void PersistPayloadToMemory<T>(string token, T payload);

        Task PersistPayloadToMongoDb<T>(string token, ThirdPartyFrom thirdPartyFrom, T payload, CancellationToken cancellationToken);
    }
    
    public class TokenDataProvider : ITokenDataProvider
    {
        private readonly IMemoryCache _memoryCache;
        private readonly IMongoDbRepository _mongoDbRepository;

        public TokenDataProvider(IMemoryCache memoryCache, IMongoDbRepository mongoDbRepository)
        {
            _memoryCache = memoryCache;
            _mongoDbRepository = mongoDbRepository;
        }

        public T GetPayloadFromMemory<T>(string token)
        {
            _memoryCache.TryGetValue(token, out T userInfo);

            return userInfo;
        }

        public async Task<T> GetPayloadFromMongoDb<T>(string token, ThirdPartyFrom thirdPartyFrom, CancellationToken cancellationToken)
        {
            var now = DateTimeOffset.Now;

            var authToken = await _mongoDbRepository.Query<UserAuthToken>()
                .Where(x => x.AccessToken == token && x.ThirdPartyFrom == thirdPartyFrom && x.ExpiredAt > now)
                .FirstOrDefaultAsync(cancellationToken).ConfigureAwait(false);

            return authToken != null ? JsonConvert.DeserializeObject<T>(authToken.Payload) : default;
        }

        public void PersistPayloadToMemory<T>(string token, T payload)
        {
            if (payload == null) return;
            
            _memoryCache.Set(token, payload, new MemoryCacheEntryOptions
            {
                SlidingExpiration = TimeSpan.FromDays(2)
            });
        }

        public async Task PersistPayloadToMongoDb<T>(string token, ThirdPartyFrom thirdPartyFrom, T payload, 
            CancellationToken cancellationToken)
        {
            if (payload == null) return;
            
            await _mongoDbRepository.AddAsync(new UserAuthToken
            {
                Id = Guid.NewGuid(),
                AccessToken = token,
                ThirdPartyFrom = thirdPartyFrom,
                ExpiredAt = DateTimeOffset.Now.AddDays(30),
                Payload = JsonConvert.SerializeObject(payload)
            }, cancellationToken).ConfigureAwait(false);
        }
    }
}