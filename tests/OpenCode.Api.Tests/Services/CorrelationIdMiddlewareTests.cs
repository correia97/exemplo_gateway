using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Hosting;

namespace OpenCode.Api.Tests.Services;

public class CorrelationIdMiddlewareTests
{
    private static async Task<IHost> CreateHost(Action<WebApplication> configureApp)
    {
        var builder = WebApplication.CreateBuilder();
        builder.WebHost.UseTestServer();
        var app = builder.Build();
        configureApp(app);
        await app.StartAsync();
        return app;
    }

    [Fact]
    public async Task Response_Contains_CorrelationId_Header()
    {
        using var host = await CreateHost(app =>
        {
            app.UseCorrelationId();
            app.Run(async context =>
            {
                context.Response.StatusCode = 200;
                await context.Response.WriteAsync("OK");
            });
        });

        var client = host.GetTestClient();
        var response = await client.GetAsync("/");

        Assert.True(response.Headers.Contains("X-Correlation-Id"));
        Assert.NotNull(response.Headers.GetValues("X-Correlation-Id").FirstOrDefault());
    }

    [Fact]
    public async Task Uses_Existing_CorrelationId_From_Request()
    {
        using var host = await CreateHost(app =>
        {
            app.UseCorrelationId();
            app.Run(async context =>
            {
                context.Response.StatusCode = 200;
                await context.Response.WriteAsync("OK");
            });
        });

        var client = host.GetTestClient();
        var request = new HttpRequestMessage(HttpMethod.Get, "/");
        request.Headers.Add("X-Correlation-Id", "my-custom-id");
        var response = await client.SendAsync(request);

        Assert.Equal("my-custom-id", response.Headers.GetValues("X-Correlation-Id").FirstOrDefault());
    }

    [Fact]
    public async Task Generates_Unique_CorrelationIds()
    {
        string id1, id2;
        using (var host = await CreateHost(app =>
        {
            app.UseCorrelationId();
            app.Run(async context =>
            {
                context.Response.StatusCode = 200;
                await context.Response.WriteAsync("OK");
            });
        }))
        {
            var client = host.GetTestClient();
            id1 = (await client.GetAsync("/")).Headers.GetValues("X-Correlation-Id").FirstOrDefault()!;
            id2 = (await client.GetAsync("/")).Headers.GetValues("X-Correlation-Id").FirstOrDefault()!;
        }

        Assert.NotNull(id1);
        Assert.NotNull(id2);
        Assert.NotEqual(id1, id2);
    }

    [Fact]
    public async Task Middleware_Does_Not_Block_Response()
    {
        using var host = await CreateHost(app =>
        {
            app.UseCorrelationId();
            app.Run(async context =>
            {
                await context.Response.WriteAsync("Hello");
            });
        });

        var client = host.GetTestClient();
        var response = await client.GetAsync("/");
        var body = await response.Content.ReadAsStringAsync();

        Assert.Equal("Hello", body);
        Assert.Equal(200, (int)response.StatusCode);
    }
}
