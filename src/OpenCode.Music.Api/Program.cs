using Asp.Versioning;
using FluentValidation;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.OpenApi;
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

builder.Services.Configure<OpenApiOptions>(options =>
{
    options.AddDocumentTransformer((document, context, cancellationToken) =>
    {
        if (!document.Components.SecuritySchemes.ContainsKey("Keycloak"))
        {
            document.Components.SecuritySchemes["Keycloak"] = new OpenApiSecurityScheme
            {
                Type = SecuritySchemeType.OAuth2,
                Description = "Keycloak OpenID Connect",
                Flows = new OpenApiOAuthFlows
                {
                    AuthorizationCode = new OpenApiOAuthFlow
                    {
                        AuthorizationUrl = new Uri("http://localhost:8080/realms/OpenCode/protocol/openid-connect/auth"),
                        TokenUrl = new Uri("http://localhost:8080/realms/OpenCode/protocol/openid-connect/token"),
                        Scopes = new Dictionary<string, string>
                        {
                            ["openid"] = "OpenID Connect",
                            ["profile"] = "User profile",
                            ["email"] = "Email address",
                            ["roles"] = "User roles"
                        }
                    }
                }
            };
        }
        return Task.CompletedTask;
    });
});
builder.Services.AddCors(options =>
{
    options.AddPolicy(corsPolicy, policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

builder.Services.AddApiVersioning(options =>
{
    options.ApiVersionReader = new UrlSegmentApiVersionReader();
})
.AddApiExplorer(options =>
{
    options.GroupNameFormat = "'v'VVV";
})
.AddOpenApi();

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
app.MapDefaultEndpoints();
app.UseCors(corsPolicy);
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.MapOpenApi().WithDocumentPerVersion();
app.MapScalarApiReference(options =>
{
    options.WithTitle("Music API");
    options.WithTheme(ScalarTheme.Purple);
    options
        .AddPreferredSecuritySchemes("BearerAuth")
        .AddOAuth2Authentication("Keycloak", scheme =>
        {
            scheme.DefaultScopes = ["openid", "profile", "email", "roles"];
        })
        .AddAuthorizationCodeFlow("Keycloak", flow =>
        {
            flow.AuthorizationUrl = "http://localhost:8080/realms/OpenCode/protocol/openid-connect/auth";
            flow.TokenUrl = "http://localhost:8080/realms/OpenCode/protocol/openid-connect/token";
            flow.ClientId = "music-api";
            flow.Pkce = Pkce.Sha256;
        });
    options.WithDefaultHttpClient(ScalarTarget.CSharp, ScalarClient.HttpClient);

    var descriptions = app.DescribeApiVersions();
    foreach (var description in descriptions)
    {
        options.AddDocument(description.GroupName, description.GroupName);
    }
});

var genresApi = app.NewVersionedApi("Genres");
var genresV1 = genresApi.MapGroup("api/v1/genres").HasApiVersion(1.0);
genresV1.MapGenreEndpoints();

var artistsApi = app.NewVersionedApi("Artists");
var artistsV1 = artistsApi.MapGroup("api/v1/artists").HasApiVersion(1.0);
artistsV1.MapArtistEndpoints();

var albumsApi = app.NewVersionedApi("Albums");
var albumsV1 = albumsApi.MapGroup("api/v1/albums").HasApiVersion(1.0);
albumsV1.MapAlbumEndpoints();

var tracksApi = app.NewVersionedApi("Tracks");
var tracksV1 = tracksApi.MapGroup("api/v1/tracks").HasApiVersion(1.0);
tracksV1.MapTrackEndpoints();

var seedApi = app.NewVersionedApi("Seed");
var seedV1 = seedApi.MapGroup("api/v1").HasApiVersion(1.0);
seedV1.MapSeedEndpoints();

var versionApi = app.NewVersionedApi("Version");
var versionV1 = versionApi.MapGroup("api/v1").HasApiVersion(1.0);
versionV1.MapVersionEndpoints();

app.Run();
