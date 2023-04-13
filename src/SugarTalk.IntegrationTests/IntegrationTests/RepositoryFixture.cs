using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Shouldly;
using SugarTalk.Core.Data;
using SugarTalk.Core.Domain.Meeting;
using SugarTalk.Tests.IntegrationTests.TestBaseClasses;
using Xunit;

namespace SugarTalk.Tests.IntegrationTests
{
    public class RepositoryFixture : FixtureBase
    {
        [Fact]
        public async Task ShouldGetMeeting()
        {
            await Run<IRepository>(async repository =>
            {
                var meetingId = Guid.NewGuid();

                await repository.InsertAsync(new Meeting
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