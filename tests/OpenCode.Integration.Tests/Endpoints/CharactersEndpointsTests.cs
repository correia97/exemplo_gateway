using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OpenCode.Domain.Data;
using OpenCode.Domain.Entities;
using OpenCode.Domain.Interfaces;
using OpenCode.DragonBall.Api.Repositories;
using OpenCode.DragonBall.Api.Endpoints;
using OpenCode.Integration.Tests.Fixtures;

namespace OpenCode.Integration.Tests.Endpoints;

[Collection("PostgresIntegration")]
public class CharactersEndpointsTests : IntegrationTestBase
{
    public CharactersEndpointsTests(PostgresFixture fixture) : base(fixture) { }

    private async Task<IHost> CreateTestHost()
    {
        var builder = WebApplication.CreateBuilder();
        builder.WebHost.UseTestServer();
        builder.Services.AddDbContextPool<DragonBallContext>(options =>
            options.UseNpgsql(ConnectionString));
        builder.Services.AddScoped<ICharacterRepository, CharacterRepository>();
        var app = builder.Build();
        app.MapGroup("/api/characters").MapCharacterEndpoints();
        await app.StartAsync();
        return app;
    }

    [Fact]
    public async Task GetCharacters_ReturnsPagedResults()
    {
        using var host = await CreateTestHost();
        var client = host.GetTestClient();
        var response = await client.GetAsync("/api/characters?Page=1&PageSize=10");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetCharacterById_ReturnsCharacter()
    {
        using var ctx = CreateDragonBallContext();
        var planet = new Planet { Name = "Test" };
        ctx.Planets.Add(planet);
        await ctx.SaveChangesAsync();
        var c = new Character { Name = "TestChar", Race = "Test", Ki = "1000", PlanetId = planet.Id };
        ctx.Characters.Add(c);
        await ctx.SaveChangesAsync();
        using var host = await CreateTestHost();
        var client = host.GetTestClient();
        var response = await client.GetAsync($"/api/characters/{c.Id}");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task CreateCharacter_Returns201()
    {
        using var host = await CreateTestHost();
        var client = host.GetTestClient();
        var payload = new { Name = "NewChar", Race = "Saiyan", Ki = "5000" };
        var response = await client.PostAsJsonAsync("/api/characters", payload);
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public async Task DeleteNonExistent_Returns404()
    {
        using var host = await CreateTestHost();
        var client = host.GetTestClient();
        var response = await client.DeleteAsync("/api/characters/99999");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
