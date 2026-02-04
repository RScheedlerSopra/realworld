# ![RealWorld Example App](logo.png)

ASP.NET Core codebase containing real world examples (CRUD, auth, advanced patterns, etc) that adheres to the [RealWorld](https://github.com/gothinkster/realworld-example-apps) spec and API.

## [RealWorld](https://github.com/gothinkster/realworld)

This codebase was created to demonstrate a fully fledged fullstack application built with ASP.NET Core (with Feature orientation) including CRUD operations, authentication, routing, pagination, and more.

We've gone to great lengths to adhere to the ASP.NET Core community styleguides & best practices.

For more information on how to this works with other frontends/backends, head over to the [RealWorld](https://github.com/gothinkster/realworld) repo.

## How it works

This is using ASP.NET Core with:

- CQRS and [MediatR](https://github.com/jbogard/MediatR)
  - [Simplifying Development and Separating Concerns with MediatR](https://blogs.msdn.microsoft.com/cdndevs/2016/01/26/simplifying-development-and-separating-concerns-with-mediatr/)
  - [CQRS with MediatR and AutoMapper](https://lostechies.com/jimmybogard/2015/05/05/cqrs-with-mediatr-and-automapper/)
  - [Thin Controllers with CQRS and MediatR](https://codeopinion.com/thin-controllers-cqrs-mediatr/)
- [AutoMapper](http://automapper.org)
- [Fluent Validation](https://github.com/JeremySkinner/FluentValidation)
- Feature folders and vertical slices
- [Entity Framework Core](https://docs.microsoft.com/en-us/ef/) on SQLite for demo purposes. Can easily be anything else EF Core supports. Open to porting to other ORMs/DBs.
- Built-in Swagger via [Swashbuckle.AspNetCore](https://github.com/domaindrivendev/Swashbuckle.AspNetCore)
- [Bullseye](https://github.com/adamralph/bullseye) for building!
- JWT authentication using [ASP.NET Core JWT Bearer Authentication](https://github.com/aspnet/Security/tree/master/src/Microsoft.AspNetCore.Authentication.JwtBearer).
- Use [dotnet-format](https://github.com/dotnet/format) for style checking
- `.editorconfig` to enforce some usage patterns

This basic architecture is based on this reference architecture: [https://github.com/jbogard/ContosoUniversityCore](https://github.com/jbogard/ContosoUniversityCore)

## Database Migrations

This project uses Entity Framework Core Migrations to manage database schema changes. Migrations provide version control for your database schema and enable safe, incremental updates across environments.

### Automatic Migration on Startup

The application automatically applies pending migrations when it starts up. This ensures the database schema is always up-to-date with the latest migration.

### Creating New Migrations

When you make changes to the domain entities or `ConduitContext`, create a new migration:

```bash
dotnet ef migrations add <MigrationName> --project src/Conduit --startup-project src/Conduit
```

Example:
```bash
dotnet ef migrations add AddUserProfileIndex --project src/Conduit --startup-project src/Conduit
```

### Viewing Migration Information

To see information about your DbContext and migrations:

```bash
# View DbContext information
dotnet ef dbcontext info --project src/Conduit

# List all migrations
dotnet ef migrations list --project src/Conduit
```

### Rolling Back Migrations

To rollback to a previous migration state:

```bash
# Revert to a specific migration
dotnet ef database update <PreviousMigrationName> --project src/Conduit

# Example: Revert to InitialCreate
dotnet ef database update InitialCreate --project src/Conduit

# To completely remove the last migration (if not yet applied)
dotnet ef migrations remove --project src/Conduit
```

### Environment Configuration

The application reads database configuration from environment variables:

- `ASPNETCORE_Conduit_DatabaseProvider` - Database provider (`sqlite` or `sqlserver`)
- `ASPNETCORE_Conduit_ConnectionString` - Database connection string

If not set, defaults to SQLite with `Filename=realworld.db` for local development.

**Docker Configuration:**

When running via Docker Compose, these variables are set in `docker-compose.yml`.

**Local Development:**

For local development with default SQLite, no environment variables are needed. The database file `realworld.db` will be created in the project directory.

### Migration Files

Migration files are located in `src/Conduit/Migrations/`:
- `<timestamp>_<MigrationName>.cs` - Migration operations (Up/Down)
- `<timestamp>_<MigrationName>.Designer.cs` - Migration metadata
- `ConduitContextModelSnapshot.cs` - Current model snapshot

### Best Practices

1. **Always create a migration** when modifying entities or DbContext configuration
2. **Test migrations locally** before deploying to production
3. **Never modify migration files** after they've been committed and shared
4. **Use descriptive names** for migrations (e.g., `AddArticleTagIndex` not `Update1`)
5. **Review generated migrations** to ensure they match your intent

## Getting started

Install the .NET Core SDK and lots of documentation: [https://www.microsoft.com/net/download/core](https://www.microsoft.com/net/download/core)

Documentation for ASP.NET Core: [https://docs.microsoft.com/en-us/aspnet/core/](https://docs.microsoft.com/en-us/aspnet/core/)

## Docker Build

There is a 'Makefile' for OS X and Linux:

- `make build` executes `docker-compose build`
- `make run` executes `docker-compose up`

The above might work for Docker on Windows

## Local building

- It's just another C# file!   `dotnet run -p build/build.csproj`

## Swagger URL

- `http://localhost:5000/swagger`

## GitHub Actions build

![Build and Test](https://github.com/gothinkster/aspnetcore-realworld-example-app/workflows/Build%20and%20Test/badge.svg)
