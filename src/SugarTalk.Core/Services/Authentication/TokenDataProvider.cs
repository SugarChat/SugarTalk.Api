using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;
using SugarTalk.Core.Data;
using SugarTalk.Core.Domain.Account;
using SugarTalk.Core.Ioc;
using SugarTalk.Messages.Enums;

namespace SugarTalk.Core.Services.Authentication
{
    public interface ITokenDataProvider : IScopedDependency
    {
        T GetPayloadFromMemory<T>(string token);
        
        Task<T> GetPayloadFromDataBase<T>(string token, ThirdPartyFrom thirdPartyFrom, CancellationToken cancellationToken);

        void PersistPayloadToMemory<T>(string token, T payload);

        Task PersistPayload<T>(string token, ThirdPartyFrom thirdPartyFrom, T payload, CancellationToken cancellationToken);
    }
    
    public class TokenDataProvider : ITokenDataProvider
    {
        private readonly IMemoryCache _memoryCache;
        private readonly IRepository _repository;

        public TokenDataProvider(IMemoryCache memoryCache, IRepository repository)
        {
            _repository = repository;
            _memoryCache = memoryCache;
        }

        public T GetPayloadFromMemory<T>(string token)
        {
            _memoryCache.TryGetValue(token, out T userInfo);

            return userInfo;
        }

        public async Task<T> GetPayloadFromDataBase<T>(string token, ThirdPartyFrom thirdPartyFrom, CancellationToken cancellationToken)
        {
            var now = DateTimeOffset.Now;

            var authToken = await _repository.Query<UserAuthToken>(x =>
                    x.AccessToken == token && x.ThirdPartyFrom == thirdPartyFrom && x.ExpiredAt > now)
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

        public async Task PersistPayload<T>(string token, ThirdPartyFrom thirdPartyFrom, T payload, 
            CancellationToken cancellationToken)
        {
            if (payload == null) return;
            
            await _repository.InsertAsync(new UserAuthToken
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