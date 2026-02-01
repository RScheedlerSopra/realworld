using System;
using Conduit.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace Conduit.UnitTests.Features.Tags;

/// <summary>
/// Base class for handler tests that need a database context
/// </summary>
public abstract class HandlerTestBase : IDisposable
{
    protected readonly ConduitContext Context;

    protected HandlerTestBase()
    {
        var options = new DbContextOptionsBuilder<ConduitContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        Context = new ConduitContext(options);
    }

    public void Dispose()
    {
        Context?.Dispose();
        GC.SuppressFinalize(this);
    }
}
