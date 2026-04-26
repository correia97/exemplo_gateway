using Microsoft.AspNetCore.Http.HttpResults;
using OpenCode.Domain.Entities;
using OpenCode.Domain.Interfaces;
using OpenCode.Domain.Pagination;
using OpenCode.Music.Api.Dtos;

namespace OpenCode.Music.Api.Endpoints;

public static class Tracks
{
    public static RouteGroupBuilder MapTrackEndpoints(this RouteGroupBuilder group)
    {
        group.MapGet("/", GetAllAsync).AllowAnonymous();
        group.MapGet("/{id:int}", GetByIdAsync).AllowAnonymous();
        group.MapPost("/", CreateAsync);
        group.MapPut("/{id:int}", UpdateAsync);
        group.MapDelete("/{id:int}", DeleteAsync);
        return group;
    }

    private static async Task<Results<Ok<PagedResult<TrackResponse>>, BadRequest>> GetAllAsync(
        ITrackRepository repository,
        [AsParameters] TrackQuery query)
    {
        var result = await repository.GetAllAsync(query.Name, query.AlbumId, query.Page, query.PageSize);
        var response = new PagedResult<TrackResponse>
        {
            Data = result.Data.Select(t => t.ToResponse()),
            TotalCount = result.TotalCount,
            Page = result.Page,
            PageSize = result.PageSize
        };
        return TypedResults.Ok(response);
    }

    private static async Task<Results<Ok<TrackResponse>, NotFound>> GetByIdAsync(
        ITrackRepository repository,
        int id)
    {
        var track = await repository.GetByIdAsync(id);
        return track is null
            ? TypedResults.NotFound()
            : TypedResults.Ok(track.ToResponse());
    }

    private static async Task<Results<Created<TrackResponse>, BadRequest>> CreateAsync(
        ITrackRepository repository,
        CreateTrackRequest request)
    {
        var track = new Track
        {
            Name = request.Name,
            TrackNumber = request.TrackNumber,
            Duration = request.Duration,
            Lyrics = request.Lyrics,
            AlbumId = request.AlbumId,
            IsStandalone = request.IsStandalone
        };
        var created = await repository.AddAsync(track);
        return TypedResults.Created($"/api/tracks/{created.Id}", created.ToResponse());
    }

    private static async Task<Results<Ok<TrackResponse>, NotFound, BadRequest>> UpdateAsync(
        ITrackRepository repository,
        int id,
        UpdateTrackRequest request)
    {
        var existing = await repository.GetByIdAsync(id);
        if (existing is null)
            return TypedResults.NotFound();

        existing.Name = request.Name;
        existing.TrackNumber = request.TrackNumber;
        existing.Duration = request.Duration;
        existing.Lyrics = request.Lyrics;
        existing.AlbumId = request.AlbumId;
        existing.IsStandalone = request.IsStandalone;

        await repository.UpdateAsync(existing);
        return TypedResults.Ok(existing.ToResponse());
    }

    private static async Task<Results<NoContent, NotFound>> DeleteAsync(
        ITrackRepository repository,
        int id)
    {
        var existing = await repository.GetByIdAsync(id);
        if (existing is null)
            return TypedResults.NotFound();

        await repository.DeleteAsync(existing);
        return TypedResults.NoContent();
    }
}

internal record TrackQuery(
    string? Name,
    int? AlbumId,
    int Page = 1,
    int PageSize = 10
);