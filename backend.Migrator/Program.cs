using System.Reflection;
using backend.Infrastructure.Database;
using FluentMigrator.Runner;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace backend.Migrator;

public static class Program
{
    public static void  Main()
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
        
        host.Build().MigrateDatabase().Run();
    }
}