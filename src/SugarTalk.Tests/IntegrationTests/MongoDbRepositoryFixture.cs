using System;
using System.Threading.Tasks;
using MongoDB.Driver.Linq;
using Shouldly;
using SugarTalk.Core.Data.MongoDb;
using SugarTalk.Core.Entities;
using Xunit;

namespace SugarTalk.Tests.IntegrationTests
{
    public class MongoDbRepositoryFixture : TestBase
    {
        [Fact]
        public async Task ShouldGetMeeting()
        {
            await Run<IMongoDbRepository>(async repository =>
            {
                var meetingId = Guid.NewGuid();

                await repository.AddAsync(new Meeting
                {
                    Id = meetingId
                });
                
                var meeting = await repository
                    .Query<Meeting>()
                    .Where(x => x.Id == meetingId)
                    .SingleOrDefaultAsync();

                meeting.ShouldNotBeNull();
            });
        }
    }
}