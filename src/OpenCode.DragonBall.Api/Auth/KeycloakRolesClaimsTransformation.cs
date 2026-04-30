using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using System.Security.Claims;
using System.Text.Json;

namespace OpenCode.DragonBall.Api.Auth;

public class KeycloakRolesClaimsTransformation : IClaimsTransformation
{
    private readonly ILogger<KeycloakRolesClaimsTransformation> _logger;

    public KeycloakRolesClaimsTransformation(ILogger<KeycloakRolesClaimsTransformation> logger)
    {
        _logger = logger;
    }

    public Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal)
    {
        var realmAccessClaim = principal.FindFirst("realm_access");
        if (realmAccessClaim is null)
            return Task.FromResult(principal);

        var clone = principal.Clone();
        var identity = (ClaimsIdentity)clone.Identity!;

        try
        {
            var realmAccess = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(realmAccessClaim.Value);
            if (realmAccess is null || !realmAccess.TryGetValue("roles", out var rolesElement))
                return Task.FromResult(clone);

            var roles = JsonSerializer.Deserialize<List<string>>(rolesElement.GetRawText());
            if (roles is null)
                return Task.FromResult(clone);

            foreach (var role in roles)
            {
                identity.AddClaim(new Claim(ClaimTypes.Role, role));
            }
        }
        catch (JsonException ex)
        {
            _logger.LogWarning(ex, "Failed to parse realm_access claim from JWT token");
        }

        return Task.FromResult(clone);
    }
}
