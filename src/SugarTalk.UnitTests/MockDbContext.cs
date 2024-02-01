using NSubstitute;
using SugarTalk.Core.Data;
using Microsoft.Extensions.Configuration;
using SugarTalk.Core.Settings.System;

namespace SugarTalk.UnitTests;

public static class MockDbContext
{
    public static SugarTalkDbContext GetSugarTalkDbContext()
    {
        var config = Substitute.For<IConfiguration>();
        var connectionString = Substitute.For<SugarTalkConnectionString>(config);

        return Substitute.For<SugarTalkDbContext>(connectionString);
    }

    public static IRepository GetRepository(SugarTalkDbContext dbContext)
    {
        return new EfRepository(dbContext);
    }
}