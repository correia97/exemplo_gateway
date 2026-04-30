using Microsoft.AspNetCore.Http.HttpResults;
using OpenCode.DragonBall.Api.Dtos;
using OpenCode.Domain.Entities;
using OpenCode.Domain.Interfaces;
using OpenCode.Domain.Pagination;

namespace OpenCode.DragonBall.Api.Endpoints;

public static class Characters
{
    public static RouteGroupBuilder MapCharacterEndpoints(this RouteGroupBuilder group)
    {
        group.MapGet("/", GetAllAsync).AllowAnonymous();
        group.MapGet("/{id:int}", GetByIdAsync).AllowAnonymous();
        group.MapPost("/", CreateAsync).RequireAuthorization("ApiPolicy");
        group.MapPut("/{id:int}", UpdateAsync).RequireAuthorization("ApiPolicy");
        group.MapDelete("/{id:int}", DeleteAsync).RequireAuthorization("ApiPolicy");
        return group;
    }

    private static async Task<Results<Ok<PagedResult<CharacterResponse>>, BadRequest>> GetAllAsync(
        ICharacterRepository repository,
        [AsParameters] GetAllRequest request)
    {
        var result = await repository.GetAllAsync(
            request.Name, request.Race, request.MinKi, request.MaxKi, request.PlanetId,
            request.Page, request.PageSize);

        var response = new PagedResult<CharacterResponse>
        {
            Data = result.Data.Select(c => c.ToResponse()),
            TotalCount = result.TotalCount,
            Page = result.Page,
            PageSize = result.PageSize
        };

        return TypedResults.Ok(response);
    }

    private static async Task<Results<Ok<CharacterResponse>, NotFound>> GetByIdAsync(
        ICharacterRepository repository,
        int id)
    {
        var character = await repository.GetByIdAsync(id);
        return character is null
            ? TypedResults.NotFound()
            : TypedResults.Ok(character.ToResponse());
    }

    private static async Task<Results<Created<CharacterResponse>, BadRequest>> CreateAsync(
        ICharacterRepository repository,
        CreateCharacterRequest request)
    {
        var character = new Character
        {
            Name = request.Name,
            Race = request.Race,
            Ki = request.Ki,
            MaxKi = request.MaxKi,
            Description = request.Description,
            PictureUrl = request.PictureUrl,
            PlanetId = request.PlanetId
        };

        var created = await repository.AddAsync(character);
        var full = await repository.GetByIdAsync(created.Id);
        return TypedResults.Created($"/api/characters/{created.Id}", full!.ToResponse());
    }

    private static async Task<Results<Ok<CharacterResponse>, NotFound, BadRequest>> UpdateAsync(
        ICharacterRepository repository,
        int id,
        UpdateCharacterRequest request)
    {
        var existing = await repository.GetByIdAsync(id);
        if (existing is null)
            return TypedResults.NotFound();

        existing.Name = request.Name;
        existing.Race = request.Race;
        existing.Ki = request.Ki;
        existing.MaxKi = request.MaxKi;
        existing.Description = request.Description;
        existing.PictureUrl = request.PictureUrl;
        existing.PlanetId = request.PlanetId;

        await repository.UpdateAsync(existing);
        var full = await repository.GetByIdAsync(id);
        return TypedResults.Ok(full!.ToResponse());
    }

    private static async Task<Results<NoContent, NotFound>> DeleteAsync(
        ICharacterRepository repository,
        int id)
    {
        var existing = await repository.GetByIdAsync(id);
        if (existing is null)
            return TypedResults.NotFound();

        await repository.DeleteAsync(existing);
        return TypedResults.NoContent();
    }
}

internal record GetAllRequest(
    string? Name,
    string? Race,
    string? MinKi,
    string? MaxKi,
    int? PlanetId,
    int Page = 1,
    int PageSize = 10
);
