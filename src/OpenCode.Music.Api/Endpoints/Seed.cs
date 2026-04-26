using Microsoft.AspNetCore.Http.HttpResults;
using OpenCode.Music.Api.Services;

namespace OpenCode.Music.Api.Endpoints;

public static class Seed
{
    public static RouteGroupBuilder MapSeedEndpoints(this RouteGroupBuilder group)
    {
        group.MapPost("/seed", SeedAsync).AllowAnonymous();
        return group;
    }

    private static async Task<Results<Ok<string>, BadRequest<string>>> SeedAsync(
        MusicSeedService seedService)
    {
        try
        {
            await seedService.SeedAsync();
            return TypedResults.Ok("Database seeded successfully with music data.");
        }
        catch (Exception ex)
        {
            return TypedResults.BadRequest($"Seed failed: {ex.Message}");
        }
    }
}
