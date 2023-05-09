using FluentMigrator.Runner;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace backend.Infrastructure.Database;

public static class MigrationManager
{
    public static IHost MigrateDatabase(this IHost host)
    {
        using var scope = host.Services.CreateScope();
        var migrationRunner = scope.ServiceProvider.GetService<IMigrationRunner>() 
            ?? throw new ArgumentNullException(nameof(IMigrationRunner));
        try
        {
            migrationRunner.ListMigrations();
            migrationRunner.MigrateUp();
        } 
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }

        return host;
    }
}