using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Conduit.Infrastructure;

/// <summary>
/// Design-time factory for creating ConduitContext instances during EF Core migrations.
/// This enables dotnetef migrations commands to create the DbContext without running the full application.
/// </summary>
public class ConduitContextFactory : IDesignTimeDbContextFactory<ConduitContext>
{
    public ConduitContext CreateDbContext(string[] args)
    {
        // Read configuration from environment variables with fallback to SQLite for local development
        var connectionString = Environment.GetEnvironmentVariable("ASPNETCORE_Conduit_ConnectionString") ?? "Filename=realworld.db";
        var databaseProvider = Environment.GetEnvironmentVariable("ASPNETCORE_Conduit_DatabaseProvider") ?? "sqlite";

        var optionsBuilder = new DbContextOptionsBuilder<ConduitContext>();

        if (databaseProvider.Equals("sqlite", StringComparison.OrdinalIgnoreCase))
        {
            optionsBuilder.UseSqlite(connectionString);
        }
        else if (databaseProvider.Equals("sqlserver", StringComparison.OrdinalIgnoreCase))
        {
            optionsBuilder.UseSqlServer(connectionString);
        }
        else
        {
            throw new InvalidOperationException(
                $"Unknown database provider '{databaseProvider}'. Supported providers: sqlite, sqlserver"
            );
        }

        return new ConduitContext(optionsBuilder.Options);
    }
}
