using FluentValidation;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi;
using OpenCode.Domain.Data;
using OpenCode.Domain.Interfaces;
using OpenCode.Music.Api.Auth;
using OpenCode.Music.Api.Endpoints;
using OpenCode.Music.Api.Repositories;
using OpenCode.Music.Api.Services;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);
var corsPolicy = "frontCorsPolicy";

builder.AddServiceDefaults();
builder.Services.AddCors(options =>
{
    options.AddPolicy(corsPolicy, policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

builder.Services.AddOpenApi();

builder.Services.AddProblemDetails(options =>
{
    options.CustomizeProblemDetails = ctx =>
    {
        ctx.ProblemDetails.Extensions["correlationId"] =
            ctx.HttpContext.Request.Headers["X-Correlation-Id"].FirstOrDefault() ?? "";
        ctx.ProblemDetails.Extensions["traceId"] = ctx.HttpContext.TraceIdentifier;
    };
});

builder.Services.AddDbContextPool<MusicContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("music")));

builder.Services.AddScoped<IGenreRepository, GenreRepository>();
builder.Services.AddScoped<IArtistRepository, ArtistRepository>();
builder.Services.AddScoped<IAlbumRepository, AlbumRepository>();
builder.Services.AddScoped<ITrackRepository, TrackRepository>();
builder.Services.AddScoped<MusicSeedService>();
builder.Services.AddHostedService<MusicDbInitializer>();

builder.Services.AddValidatorsFromAssemblyContaining<Program>();
builder.Services.AddFluentValidationAutoValidation();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority = "http://localhost:8080/realms/OpenCode";
        options.Audience = "music-api";
        options.RequireHttpsMetadata = false;
        options.TokenValidationParameters.ValidateAudience = true;
        options.TokenValidationParameters.ValidateIssuer = true;
        options.TokenValidationParameters.ValidIssuer = "http://localhost:8080/realms/OpenCode";
    });

builder.Services.AddAuthorizationBuilder()
    .AddPolicy("ApiPolicy", policy =>
        {
            policy.RequireAuthenticatedUser();
            policy.RequireRole("editor");
        });

builder.Services.AddTransient<IClaimsTransformation, KeycloakRolesClaimsTransformation>();

var app = builder.Build();

app.UseCorrelationId();
app.UseAuthentication();
app.UseAuthorization();
app.UseCors(corsPolicy);

app.MapOpenApi();
app.MapScalarApiReference(options =>
{
    options.WithTitle("Music API");
    options.WithTheme(ScalarTheme.Purple);
    options.Authentication = new ScalarAuthenticationOptions
    {
        PreferredSecuritySchemes = new List<string> { "BearerAuth" }
    };
    options.WithDefaultHttpClient(ScalarTarget.CSharp, ScalarClient.HttpClient);
});

app.UseHttpsRedirection();

app.MapGroup("/api/genres")
   .MapGenreEndpoints();
app.MapGroup("/api/artists")
   .MapArtistEndpoints();
app.MapGroup("/api/albums")
   .MapAlbumEndpoints();
app.MapGroup("/api/tracks")
   .MapTrackEndpoints();

app.MapGroup("/api")
   .MapSeedEndpoints();

app.Run();
