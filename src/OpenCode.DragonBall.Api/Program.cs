using FluentValidation;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi;
using OpenCode.Domain.Data;
using OpenCode.Domain.Interfaces;
using OpenCode.DragonBall.Api.Auth;
using OpenCode.DragonBall.Api.Endpoints;
using OpenCode.DragonBall.Api.Repositories;
using OpenCode.DragonBall.Api.Services;
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

builder.Services.AddDbContextPool<DragonBallContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("dragonball")));

builder.Services.AddScoped<ICharacterRepository, CharacterRepository>();
builder.Services.AddScoped<DragonBallSeedService>();
builder.Services.AddHostedService<DragonBallDbInitializer>();

builder.Services.AddValidatorsFromAssemblyContaining<Program>();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority = "http://localhost:8080/realms/OpenCode";
        options.Audience = "dragonball-api";
        options.RequireHttpsMetadata = false;
        options.TokenValidationParameters.ValidateAudience = false;
        options.TokenValidationParameters.ValidateIssuer = false;
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
    options.WithTitle("Dragon Ball API");
    options.WithTheme(ScalarTheme.Purple);
    options.Authentication = new ScalarAuthenticationOptions
    {
        PreferredSecuritySchemes = new List<string> { "BearerAuth" }
    };
    options.WithDefaultHttpClient(ScalarTarget.CSharp, ScalarClient.HttpClient);
    
});

app.UseHttpsRedirection();

app.MapGroup("/api/characters")
   .MapCharacterEndpoints();

app.MapGroup("/api")
   .MapSeedEndpoints();

app.Run();
