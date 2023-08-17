using System.Reflection;
using backend.Infrastructure.Database;
using FluentMigrator.Runner;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace backend.Migrator;

public static class Program
{
    public static void Main()
    {
        var host = Host.CreateDefaultBuilder();
        host.ConfigureServices((hostBuilder, services) =>
        {
            var connectionString = hostBuilder.Configuration.GetSection("SqlConnection")?.Value
                                   ?? throw new ArgumentException("SqlConnection not found");
            
            services.AddLogging(c => c.AddFluentMigratorConsole())
                .AddFluentMigratorCore()
                .ConfigureRunner(c => c.AddPostgres()
                    .WithGlobalConnectionString(connectionString)
                    .ScanIn(Assembly.GetExecutingAssembly()).For.Migrations());
        });

        host.Build().MigrateDatabase();
    }

    private static void MigrateDatabase(this IHost host)
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
    }
}