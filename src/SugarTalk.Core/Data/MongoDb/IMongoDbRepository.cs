using MongoDB.Driver.Linq;

namespace SugarTalk.Core.Data.MongoDb
{
    public interface IMongoDbRepository : IRepository
    {
        IMongoQueryable<T> Query<T>();
    }
}