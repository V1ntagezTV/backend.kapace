using System.Text.Json.Serialization;
using backend.kapace;
using backend.kapace.BLL;
using backend.kapace.BLL.Services;
using backend.kapace.BLL.Services.Interfaces;
using backend.kapace.DAL;
using backend.kapace.DAL.Experimental;
using backend.kapace.DAL.Experimental.StarsRepository;
using backend.kapace.DAL.Repository;
using backend.kapace.DAL.Repository.Interfaces;
using backend.kapace.Middlewares;
using backend.kapace.Options;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Npgsql;

AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
Dapper.DefaultTypeMap.MatchNamesWithUnderscores = true;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddExceptionHandler<ServiceExceptionHandler>();
builder.Services.AddProblemDetails();
var services = builder.Services;
var config = builder.Configuration;

services.Configure<SmtpMailOptions>(builder.Configuration.GetSection(nameof(SmtpMailOptions)));
services.AddScoped<IContentRepository, ContentRepository>();
services.AddScoped<IContentService, ContentService>();
services.AddScoped<IEpisodeRepository, EpisodeRepository>();
services.AddScoped<IGenreRepository, GenreRepository>();
services.AddScoped<IGenreService, GenreService>();
services.AddScoped<IContentGenreRepository, ContentGenreRepository>();
services.AddScoped<ITranslationRepository, TranslationRepository>();
services.AddScoped<ITranslationService, TranslationService>();
services.AddScoped<IImageService, ImageService>();
services.AddScoped<IStaticFilesRepository, StaticFilesRepository>();
services.AddScoped<IChangesHistoryRepository, ChangesHistoryRepository>();
services.AddScoped<ITranslatorsRepository, TranslatorsRepository>();
services.AddScoped<IChangesHistoryService, ChangesHistoryService>();
services.AddScoped<IEpisodeService, EpisodeService>();
services.AddScoped<BaseRepository<StarsDataColumns>, StarsRepository>();
services.AddScoped<ITranslatorService, TranslatorService>();
services.AddScoped<IUserService, UserService>();
services.AddScoped<IUserRepository, UserRepository>();
services.AddScoped<IUserPermissionRepository, UserPermissionRepository>();
services.AddScoped<IPermissionRepository, PermissionRepository>();
services.AddScoped<IPermissionService, PermissionService>();
services.AddScoped<IVerificationCodeRepository, VerificationCodeRepository>();
services.AddScoped<IFavoriteRepository, FavoriteRepository>();
services.AddScoped<IFavoriteService, FavoriteService>();
services.AddScoped<ILoggingRepository, LoggingRepository>();
services.AddMemoryCache();
var connection = config.GetSection("SqlConnection").Value ?? throw new ArgumentException();
var npgsqlBuilder = new NpgsqlDataSourceBuilder(connection);
npgsqlBuilder.MapComposites();
backend.Migrator.Program.Main();

services.AddSingleton(_ => npgsqlBuilder.Build());
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
    c.CustomSchemaIds(x => x.FullName);
    c.SwaggerDoc("v0.1", new OpenApiInfo { Title = "My API", Version = "v0.1" });
});
AddAuthorization();

var app = builder.Build();
app.UseExceptionHandler();
app.MapControllers();
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v0.1/swagger.json", "My API V1");
    c.RoutePrefix = string.Empty;
});
app.UseRouting();
app.UseCors(options => options
    .AllowAnyMethod()
    .AllowAnyHeader()
    .AllowCredentials()
    .SetIsOriginAllowed(_ => true)); // allow any origin
app.UseAuthentication(); // Включаем аутентификацию
app.UseAuthorization();  // Включаем авторизацию
app.UseCookiePolicy();
app.UseCors();
app.Run();


void AddAuthorization()
{
    services
        .AddAuthorization(options =>
        {
            var defaultPolicies = new AuthorizationPolicyBuilder(
                CookieAuthenticationDefaults.AuthenticationScheme,
                JwtBearerDefaults.AuthenticationScheme);
            
            defaultPolicies = defaultPolicies.RequireAuthenticatedUser();
            options.DefaultPolicy = defaultPolicies.Build();
        })
        .AddAuthentication(options =>
        {
            options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
        })
        .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options =>
        {
            options.LoginPath = "";
            // флаг, чтобы кука передавалась только по HTTPS. Это защищает куку от перехвата через атаки типа "man-in-the-middle" на незашифрованных соединениях. Настройка в ASP.NET Core:
            options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
            // Указывает, что кука недоступна через JavaScript (например, через document.cookie). Это минимизирует риск XSS-атак
            options.Cookie.HttpOnly = true;
            // Используйте атрибут SameSite для защиты от CSRF-атак (межсайтовых подделок запросов). Этот атрибут ограничивает передачу куки только в пределах одного сайта. Настройка в ASP.NET Core:
            options.Cookie.SameSite = SameSiteMode.None;
            // Время жизни куки
            options.ExpireTimeSpan = TimeSpan.FromHours(24);
            // Включает "скользящее" время жизни куки, которое автоматически обновляет время жизни куки при каждом запросе
            options.SlidingExpiration = true;
            // Включает проверку валидности куки при каждом запросе
            options.Cookie.IsEssential = true;
        })
        .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                // указывает, будет ли валидироваться издатель при валидации токена
                ValidateIssuer = true,
                // строка, представляющая издателя
                ValidIssuer = AuthOptions.Issuer,
                // будет ли валидироваться потребитель токена
                ValidateAudience = true,
                // установка потребителя токена
                ValidAudience = AuthOptions.Audience,
                // будет ли валидироваться время существования
                ValidateLifetime = true,
                // установка ключа безопасности
                IssuerSigningKey = AuthOptions.GetSymmetricSecurityKey(),
                // валидация ключа безопасности
                ValidateIssuerSigningKey = true,
            };
        });
}