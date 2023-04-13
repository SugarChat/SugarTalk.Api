using System;
using System.Threading;
using System.Threading.Tasks;
using SugarTalk.Core.Data;
using SugarTalk.Core.Domain.Account;

namespace SugarTalk.Core.Services.Users
{
    public interface IUserDataProvider
    {
        Task<User> GetUserByThirdPartyId(string thirdPartyId, CancellationToken cancellationToken);

        Task PersistUser(User user, CancellationToken cancellationToken);
    }
    
    public class UserDataProvider : IUserDataProvider
    {
        private readonly IRepository _repository;

        public UserDataProvider(IRepository repository)
        {
            _repository = repository;
        }

        public async Task<User> GetUserByThirdPartyId(string thirdPartyId, CancellationToken cancellationToken)
        {
            return await _repository.SingleOrDefaultAsync<User>(x => x.ThirdPartyId == thirdPartyId, cancellationToken)
                .ConfigureAwait(false);
        }

        public async Task PersistUser(User user, CancellationToken cancellationToken)
        {
            if (user.Id == Guid.Empty)
                user.Id = Guid.NewGuid();

            await _repository.InsertAsync(user, cancellationToken).ConfigureAwait(false);
        }
    }
}