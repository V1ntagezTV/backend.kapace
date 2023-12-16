using backend.kapace.BLL.Services;
using backend.kapace.BLL.Services.Interfaces;
using backend.kapace.DAL;
using backend.kapace.DAL.Experimental;
using backend.kapace.DAL.Experimental.StarsRepository;
using backend.kapace.DAL.Repository;
using backend.kapace.DAL.Repository.Interfaces;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;

namespace backend.Tests.Fixtures;

public class MainFixture
{
    public readonly IServiceProvider ServiceProvider;

    public MainFixture() 
    {
        var builder = WebApplication.CreateBuilder();
        var config = builder.Configuration;
        
        builder.Environment.EnvironmentName = "Testing";
        var services = builder.Services;
        services.AddScoped<IContentRepository, ContentRepository>();
        services.AddScoped<IContentService, ContentService>();
        services.AddScoped<IEpisodeRepository, EpisodeRepository>();
        services.AddScoped<IGenreRepository, GenreRepository>();
        services.AddScoped<IContentGenreRepository, ContentGenreRepository>();
        services.AddScoped<ITranslationRepository, TranslationRepository>();
        services.AddScoped<ITranslationService, TranslationService>();
        services.AddScoped<IImageService, ImageService>();
        services.AddScoped<IStaticFilesRepository, StaticFilesRepository>();
        services.AddScoped<IChangesHistoryRepository, ChangesHistoryRepository>();
        services.AddScoped<IChangesHistoryService, ChangesHistoryService>();
        services.AddScoped<BaseRepository<StarsDataColumns>, StarsRepository>();
        
        var connection = config.GetSection("SqlConnection").Value ?? throw new ArgumentException();
        var dataSource = new NpgsqlDataSourceBuilder(connection).MapComposites(connection).Build();
        services.AddSingleton<NpgsqlDataSource>(_ => dataSource);
        
        var app = builder.Build();
        
        ServiceProvider = app.Services;
    }
}
