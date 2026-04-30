using System.Net;
using System.Net.Http.Json;
using System.Text;
using FluentValidation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OpenCode.DragonBall.Api.Dtos;
using OpenCode.DragonBall.Api.Validators;

namespace OpenCode.Api.Tests.Services;

public class ValidationPipelineTests
{
    private static async Task<IHost> CreateValidationHost()
    {
        var builder = WebApplication.CreateBuilder();
        builder.WebHost.UseTestServer();
        builder.Services.AddValidatorsFromAssemblyContaining<CreateCharacterValidator>();
        var app = builder.Build();
        app.MapPost("/test-validate", (CreateCharacterRequest request) =>
        {
            return Results.Ok(new { name = request.Name });
        }).AddEndpointFilter<AutoValidationFilter<CreateCharacterRequest>>();
        await app.StartAsync();
        return app;
    }

    [Fact]
    public async Task InvalidRequest_ReturnsBadRequest()
    {
        using var host = await CreateValidationHost();
        var client = host.GetTestClient();
        var request = new CreateCharacterRequest("", "Saiyan", "60.000.000", null, null, null, null);
        var response = await client.PostAsJsonAsync("/test-validate", request);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task InvalidRequest_ReturnsValidationProblemDetails()
    {
        using var host = await CreateValidationHost();
        var client = host.GetTestClient();
        var request = new CreateCharacterRequest("", "Saiyan", "60.000.000", null, null, null, null);
        var response = await client.PostAsJsonAsync("/test-validate", request);
        var problem = await response.Content.ReadFromJsonAsync<HttpValidationProblemDetails>();
        Assert.NotNull(problem);
        Assert.Equal(400, problem.Status);
    }

    [Fact]
    public async Task InvalidRequest_IncludesErrorDetails()
    {
        using var host = await CreateValidationHost();
        var client = host.GetTestClient();
        var request = new CreateCharacterRequest("", "Saiyan", "60.000.000", null, null, null, null);
        var response = await client.PostAsJsonAsync("/test-validate", request);
        var problem = await response.Content.ReadFromJsonAsync<HttpValidationProblemDetails>();
        Assert.NotNull(problem);
        Assert.True(problem.Errors.ContainsKey("Name"));
        Assert.NotEmpty(problem.Errors["Name"]);
    }

    [Fact]
    public async Task ValidRequest_ReturnsOk()
    {
        using var host = await CreateValidationHost();
        var client = host.GetTestClient();
        var request = new CreateCharacterRequest("Goku", "Saiyan", "60.000.000", null, null, null, null);
        var response = await client.PostAsJsonAsync("/test-validate", request);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task MultipleErrors_ReturnsAllErrors()
    {
        using var host = await CreateValidationHost();
        var client = host.GetTestClient();
        var request = new CreateCharacterRequest("", "", "", null, null, null, null);
        var response = await client.PostAsJsonAsync("/test-validate", request);
        var problem = await response.Content.ReadFromJsonAsync<HttpValidationProblemDetails>();
        Assert.NotNull(problem);
        Assert.True(problem.Errors.Count >= 3);
    }

    [Fact]
    public async Task EmptyBody_ReturnsBadRequest()
    {
        using var host = await CreateValidationHost();
        var client = host.GetTestClient();
        var response = await client.PostAsJsonAsync("/test-validate", new { });
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task NullBody_ReturnsBadRequest()
    {
        using var host = await CreateValidationHost();
        var client = host.GetTestClient();
        var content = new StringContent("null", Encoding.UTF8, "application/json");
        var response = await client.PostAsync("/test-validate", content);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task ValidRequest_PassesCorrectData()
    {
        using var host = await CreateValidationHost();
        var client = host.GetTestClient();
        var request = new CreateCharacterRequest("Goku", "Saiyan", "60.000.000", null, null, null, null);
        var response = await client.PostAsJsonAsync("/test-validate", request);
        var body = await response.Content.ReadFromJsonAsync<Dictionary<string, string>>();
        Assert.NotNull(body);
        Assert.Equal("Goku", body["name"]);
    }

    [Fact]
    public async Task InvalidPictureUrl_ReturnsValidationError()
    {
        using var host = await CreateValidationHost();
        var client = host.GetTestClient();
        var request = new CreateCharacterRequest("Goku", "Saiyan", "60.000.000", null, null, "not-a-url", null);
        var response = await client.PostAsJsonAsync("/test-validate", request);
        var problem = await response.Content.ReadFromJsonAsync<HttpValidationProblemDetails>();
        Assert.NotNull(problem);
        Assert.True(problem.Errors.ContainsKey("PictureUrl"));
    }
}

public class AutoValidationFilter<T> : IEndpointFilter where T : class
{
    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        var arg = context.Arguments.OfType<T>().FirstOrDefault();
        if (arg is null) return TypedResults.BadRequest(new { error = "Request body cannot be null" });

        var validator = context.HttpContext.RequestServices.GetService<IValidator<T>>();
        if (validator is null) return await next(context);

        var result = await validator.ValidateAsync(arg, context.HttpContext.RequestAborted);
        if (!result.IsValid)
            return TypedResults.ValidationProblem(result.ToDictionary());

        return await next(context);
    }
}
