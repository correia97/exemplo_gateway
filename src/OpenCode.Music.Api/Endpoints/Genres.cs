using Microsoft.AspNetCore.Http.HttpResults;
using OpenCode.Domain.Entities;
using OpenCode.Domain.Interfaces;
using OpenCode.Domain.Pagination;
using OpenCode.Music.Api.Dtos;

namespace OpenCode.Music.Api.Endpoints;

public static class Genres
{
    public static RouteGroupBuilder MapGenreEndpoints(this RouteGroupBuilder group)
    {
        group.MapGet("/", GetAllAsync);
        group.MapGet("/{id:int}", GetByIdAsync);
        group.MapPost("/", CreateAsync);
        group.MapPut("/{id:int}", UpdateAsync);
        group.MapDelete("/{id:int}", DeleteAsync);
        return group;
    }

    private static async Task<Results<Ok<PagedResult<GenreResponse>>, BadRequest>> GetAllAsync(
        IGenreRepository repository,
        [AsParameters] GenreQuery query)
    {
        var result = await repository.GetAllAsync(query.Name, query.Page, query.PageSize);
        var response = new PagedResult<GenreResponse>
        {
            Data = result.Data.Select(g => g.ToResponse()),
            TotalCount = result.TotalCount,
            Page = result.Page,
            PageSize = result.PageSize
        };
        return TypedResults.Ok(response);
    }

    private static async Task<Results<Ok<GenreResponse>, NotFound>> GetByIdAsync(
        IGenreRepository repository,
        int id)
    {
        var genre = await repository.GetByIdAsync(id);
        return genre is null
            ? TypedResults.NotFound()
            : TypedResults.Ok(genre.ToResponse());
    }

    private static async Task<Results<Created<GenreResponse>, BadRequest>> CreateAsync(
        IGenreRepository repository,
        CreateGenreRequest request)
    {
        var genre = new Genre
        {
            Name = request.Name,
            Description = request.Description
        };
        var created = await repository.AddAsync(genre);
        return TypedResults.Created($"/api/genres/{created.Id}", created.ToResponse());
    }

    private static async Task<Results<Ok<GenreResponse>, NotFound, BadRequest>> UpdateAsync(
        IGenreRepository repository,
        int id,
        UpdateGenreRequest request)
    {
        var existing = await repository.GetByIdAsync(id);
        if (existing is null)
            return TypedResults.NotFound();

        existing.Name = request.Name;
        existing.Description = request.Description;

        await repository.UpdateAsync(existing);
        return TypedResults.Ok(existing.ToResponse());
    }

    private static async Task<Results<NoContent, NotFound>> DeleteAsync(
        IGenreRepository repository,
        int id)
    {
        var existing = await repository.GetByIdAsync(id);
        if (existing is null)
            return TypedResults.NotFound();

        await repository.DeleteAsync(existing);
        return TypedResults.NoContent();
    }
}

internal record GenreQuery(
    string? Name,
    int Page = 1,
    int PageSize = 10
);