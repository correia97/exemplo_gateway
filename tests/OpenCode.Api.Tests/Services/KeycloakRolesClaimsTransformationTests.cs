using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using OpenCode.DragonBall.Api.Auth;

namespace OpenCode.Api.Tests.Services;

public class KeycloakRolesClaimsTransformationTests
{
    private static readonly ILogger<KeycloakRolesClaimsTransformation> Logger =
        new LoggerFactory().CreateLogger<KeycloakRolesClaimsTransformation>();

    private readonly KeycloakRolesClaimsTransformation _transformation = new(Logger);

    private static ClaimsPrincipal CreatePrincipalWithRealmAccess(string json)
    {
        var claims = new List<Claim>
        {
            new("realm_access", json),
            new("sub", "user-123")
        };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        return new ClaimsPrincipal(identity);
    }

    [Fact]
    public async Task TransformAsync_Adds_Role_Claims_From_RealmAccess()
    {
        var principal = CreatePrincipalWithRealmAccess(
            JsonSerializer.Serialize(new { roles = new[] { "viewer", "editor" } }));

        var result = await _transformation.TransformAsync(principal);

        Assert.True(result.IsInRole("viewer"));
        Assert.True(result.IsInRole("editor"));
    }

    [Fact]
    public async Task TransformAsync_When_No_RealmAccess_Returns_Original()
    {
        var identity = new ClaimsIdentity(new[] { new Claim("sub", "user-123") }, "TestAuth");
        var principal = new ClaimsPrincipal(identity);

        var result = await _transformation.TransformAsync(principal);

        Assert.False(result.IsInRole("viewer"));
    }

    [Fact]
    public async Task TransformAsync_When_RealmAccess_Invalid_Json_Returns_Original()
    {
        var principal = CreatePrincipalWithRealmAccess("not-valid-json");

        var result = await _transformation.TransformAsync(principal);

        Assert.False(result.IsInRole("viewer"));
    }

    [Fact]
    public async Task TransformAsync_When_RealmAccess_Missing_Roles_Returns_Original()
    {
        var principal = CreatePrincipalWithRealmAccess(
            JsonSerializer.Serialize(new { other = new[] { "value" } }));

        var result = await _transformation.TransformAsync(principal);

        Assert.False(result.IsInRole("viewer"));
    }

    [Fact]
    public async Task TransformAsync_When_Single_Role_Adds_Claim()
    {
        var principal = CreatePrincipalWithRealmAccess(
            JsonSerializer.Serialize(new { roles = new[] { "editor" } }));

        var result = await _transformation.TransformAsync(principal);

        Assert.True(result.IsInRole("editor"));
        Assert.False(result.IsInRole("viewer"));
    }

    [Fact]
    public async Task TransformAsync_When_Roles_Empty_Returns_Original()
    {
        var principal = CreatePrincipalWithRealmAccess(
            JsonSerializer.Serialize(new { roles = new string[0] }));

        var result = await _transformation.TransformAsync(principal);

        Assert.False(result.IsInRole("viewer"));
        Assert.False(result.IsInRole("editor"));
    }
}
