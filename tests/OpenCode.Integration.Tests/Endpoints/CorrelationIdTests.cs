using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Hosting;
using OpenCode.Integration.Tests.Fixtures;

namespace OpenCode.Integration.Tests.Endpoints;

[Collection("PostgresIntegration")]
public class CorrelationIdTests : IntegrationTestBase
{
    public CorrelationIdTests(PostgresFixture fixture) : base(fixture) { }

    private static async Task<IHost> CreateTestHost()
    {
        var builder = WebApplication.CreateBuilder();
        builder.WebHost.UseTestServer();
        var app = builder.Build();
        app.UseCorrelationId();
        app.Run(async context =>
        {
            context.Response.StatusCode = 200;
            await context.Response.WriteAsync("OK");
        });
        await app.StartAsync();
        return app;
    }

    [Fact]
    public async Task ResponseContainsCorrelationId()
    {
        using var host = await CreateTestHost();
        var client = host.GetTestClient();
        var response = await client.GetAsync("/");
        Assert.True(response.Headers.Contains("X-Correlation-Id"));
        Assert.NotNull(response.Headers.GetValues("X-Correlation-Id").FirstOrDefault());
    }

    [Fact]
    public async Task PreservesExistingCorrelationId()
    {
        using var host = await CreateTestHost();
        var client = host.GetTestClient();
        var request = new HttpRequestMessage(HttpMethod.Get, "/");
        request.Headers.Add("X-Correlation-Id", "my-test-id");
        var response = await client.SendAsync(request);
        Assert.Equal("my-test-id", response.Headers.GetValues("X-Correlation-Id").FirstOrDefault());
    }

    [Fact]
    public async Task GeneratesUniqueIds()
    {
        using var host = await CreateTestHost();
        var client = host.GetTestClient();
        var id1 = (await client.GetAsync("/")).Headers.GetValues("X-Correlation-Id").First();
        var id2 = (await client.GetAsync("/")).Headers.GetValues("X-Correlation-Id").First();
        Assert.NotEqual(id1, id2);
    }
}
