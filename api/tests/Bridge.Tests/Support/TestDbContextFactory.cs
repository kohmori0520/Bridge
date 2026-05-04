using Bridge.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Bridge.Tests.Support;

internal static class TestDbContextFactory
{
    public static BridgeDbContext Create(string databaseNamePrefix)
    {
        var options = new DbContextOptionsBuilder<BridgeDbContext>()
            .UseInMemoryDatabase($"{databaseNamePrefix}-{Guid.NewGuid()}")
            .ConfigureWarnings(warnings => warnings.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;

        return new BridgeDbContext(options);
    }
}
