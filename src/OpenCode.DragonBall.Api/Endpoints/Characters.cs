using Microsoft.AspNetCore.Http.HttpResults;
using OpenCode.DragonBall.Api.Dtos;
using OpenCode.DragonBall.Api.Repositories;
using OpenCode.Domain.Entities;
using OpenCode.Domain.Interfaces;
using OpenCode.Domain.Pagination;

namespace OpenCode.DragonBall.Api.Endpoints;

public static class Characters
{
    public static RouteGroupBuilder MapCharacterEndpoints(this RouteGroupBuilder group)
    {
        group.MapGet("/", GetAllAsync);
        group.MapGet("/{id:int}", GetByIdAsync);
        group.MapPost("/", CreateAsync);
        group.MapPut("/{id:int}", UpdateAsync);
        group.MapDelete("/{id:int}", DeleteAsync);
        return group;
    }

    private static async Task<Results<Ok<PagedResult<CharacterResponse>>, BadRequest>> GetAllAsync(
        ICharacterRepository repository,
        [AsParameters] GetAllRequest request)
    {
        var result = await repository.GetAllAsync(
            request.Name, request.IntroductionPhase,
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
            IsEarthling = request.IsEarthling,
            IntroductionPhase = request.IntroductionPhase,
            PictureUrl = request.PictureUrl
        };

        var created = await repository.AddAsync(character);
        return TypedResults.Created($"/api/characters/{created.Id}", created.ToResponse());
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
        existing.IsEarthling = request.IsEarthling;
        existing.IntroductionPhase = request.IntroductionPhase;
        existing.PictureUrl = request.PictureUrl;

        await repository.UpdateAsync(existing);
        return TypedResults.Ok(existing.ToResponse());
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
    string? IntroductionPhase,
    int Page = 1,
    int PageSize = 10
);