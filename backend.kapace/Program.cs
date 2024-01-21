using System.Text.Json.Serialization;
using backend.kapace.BLL.Services;
using backend.kapace.BLL.Services.Interfaces;
using backend.kapace.DAL;
using backend.kapace.DAL.Experimental;
using backend.kapace.DAL.Experimental.StarsRepository;
using backend.kapace.DAL.Repository;
using backend.kapace.DAL.Repository.Interfaces;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using Npgsql;

Dapper.DefaultTypeMap.MatchNamesWithUnderscores = true;

var builder = WebApplication.CreateBuilder(args);
var services = builder.Services;
var config = builder.Configuration;

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
services.AddScoped<ITranslatorsRepository, TranslatorsRepository>();
services.AddScoped<IChangesHistoryService, ChangesHistoryService>();
services.AddScoped<BaseRepository<StarsDataColumns>, StarsRepository>();

var connection = config.GetSection("SqlConnection").Value ?? throw new ArgumentException();
var dataSource = new NpgsqlDataSourceBuilder(connection).MapComposites(connection).Build();

backend.Migrator.Program.Main();

services.AddSingleton<NpgsqlDataSource>(_ => dataSource);
services
    .AddMvc()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = null;
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });
services.AddCors();
services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v0.1", new OpenApiInfo { Title = "My API", Version = "v0.1" });
});
var app = builder.Build();

app.MapControllers();
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v0.1/swagger.json", "My API V1");
    c.RoutePrefix = string.Empty;
});
app.UseRouting();
app.UseCors(policyBuilder =>
{
    policyBuilder.AllowAnyOrigin();
    policyBuilder.AllowAnyHeader();
});
app.Run();
