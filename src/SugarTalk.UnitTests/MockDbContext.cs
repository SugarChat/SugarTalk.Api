using NSubstitute;
using SugarTalk.Core.Data;
using SugarTalk.Core.Services.Utils;
using SugarTalk.Core.Settings.System;
using SugarTalk.Core.Services.Identity;
using Microsoft.Extensions.Configuration;

namespace SugarTalk.UnitTests;

public static class MockDbContext
{
    public static SugarTalkDbContext GetSugarTalkDbContext()
    {
        var config = Substitute.For<IConfiguration>();
        var clock = Substitute.For<IClock>();
        var currentUser = Substitute.For<ICurrentUser>();
        var connectionString = Substitute.For<SugarTalkConnectionString>(config);

        return Substitute.For<SugarTalkDbContext>(connectionString, currentUser, clock);
    }

    public static IRepository GetRepository(SugarTalkDbContext dbContext)
    {
        return new EfRepository(dbContext);
    }
}