using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.OpenApi;
using OpenCode.Domain.Data;
using OpenCode.Domain.Interfaces;
using OpenCode.Domain.Implementations;
using OpenCode.Music.Api.Repositories;
using OpenCode.Music.Api.Endpoints;
using Scalar.AspNetCore;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddOpenApi(options =>
{
    options.AddDocumentTransformer((document, context, cancellationToken) =>
    {
        document.Servers = [new OpenApiServer { Url = "/api/music" }];
        return Task.CompletedTask;
    });
});

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

builder.Services.AddValidatorsFromAssemblyContaining<Program>();
builder.Services.AddFluentValidationAutoValidation();

var app = builder.Build();

app.UseCorrelationId();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference(options =>
    {
        options.WithTitle("Music API");
        options.WithTheme(ScalarTheme.Purple);
        options.WithDefaultHttpClient(ScalarTarget.CSharp, ScalarClient.HttpClient);
    });
}

app.UseHttpsRedirection();

app.MapGroup("/api/genres")
   .MapGenreEndpoints();
app.MapGroup("/api/artists")
   .MapArtistEndpoints();
app.MapGroup("/api/albums")
   .MapAlbumEndpoints();
app.MapGroup("/api/tracks")
   .MapTrackEndpoints();

app.Run();