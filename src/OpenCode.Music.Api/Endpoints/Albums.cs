using Microsoft.AspNetCore.Http.HttpResults;
using OpenCode.Domain.Entities;
using OpenCode.Domain.Interfaces;
using OpenCode.Domain.Pagination;
using OpenCode.Music.Api.Dtos;
using OpenCode.Music.Api.Repositories;

namespace OpenCode.Music.Api.Endpoints;

public static class Albums
{
    public static RouteGroupBuilder MapAlbumEndpoints(this RouteGroupBuilder group)
    {
        group.MapGet("/", GetAllAsync).AllowAnonymous();
        group.MapGet("/{id:int}", GetByIdAsync).AllowAnonymous();
        group.MapPost("/", CreateAsync);
        group.MapPut("/{id:int}", UpdateAsync);
        group.MapDelete("/{id:int}", DeleteAsync);
        group.MapGet("/{albumId:int}/tracks", GetTracksByAlbumAsync).AllowAnonymous();
        return group;
    }

    private static async Task<Results<Ok<PagedResult<AlbumResponse>>, BadRequest>> GetAllAsync(
        IAlbumRepository repository,
        [AsParameters] AlbumQuery query)
    {
        var result = await repository.GetAllAsync(
            query.Title, query.ArtistId, query.GenreId,
            query.ReleaseDateFrom, query.ReleaseDateTo,
            query.Page, query.PageSize);

        var response = new PagedResult<AlbumResponse>
        {
            Data = result.Data.Select(a => a.ToResponse()),
            TotalCount = result.TotalCount,
            Page = result.Page,
            PageSize = result.PageSize
        };
        return TypedResults.Ok(response);
    }

    private static async Task<Results<Ok<AlbumResponse>, NotFound>> GetByIdAsync(
        IAlbumRepository repository,
        int id)
    {
        var repo = (AlbumRepository)repository;
        var album = await repo.GetByIdWithArtistAsync(id);
        return album is null
            ? TypedResults.NotFound()
            : TypedResults.Ok(album.ToResponse());
    }

    private static async Task<Results<Created<AlbumResponse>, BadRequest>> CreateAsync(
        IAlbumRepository repository,
        CreateAlbumRequest request)
    {
        var album = new Album
        {
            Title = request.Title,
            ReleaseDate = request.ReleaseDate,
            CoverUrl = request.CoverUrl,
            ArtistId = request.ArtistId
        };
        var created = await repository.AddAsync(album);

        var albumRepo = (AlbumRepository)repository;
        var fullAlbum = await albumRepo.GetByIdWithArtistAsync(created.Id);
        return TypedResults.Created($"/api/albums/{created.Id}", fullAlbum?.ToResponse() ?? created.ToResponse());
    }

    private static async Task<Results<Ok<AlbumResponse>, NotFound, BadRequest>> UpdateAsync(
        IAlbumRepository repository,
        int id,
        UpdateAlbumRequest request)
    {
        var existing = await repository.GetByIdAsync(id);
        if (existing is null)
            return TypedResults.NotFound();

        existing.Title = request.Title;
        existing.ReleaseDate = request.ReleaseDate;
        existing.CoverUrl = request.CoverUrl;

        await repository.UpdateAsync(existing);

        var repo = (AlbumRepository)repository;
        var updated = await repo.GetByIdWithArtistAsync(id);
        return TypedResults.Ok(updated?.ToResponse() ?? existing.ToResponse());
    }

    private static async Task<Results<NoContent, NotFound>> DeleteAsync(
        IAlbumRepository repository,
        int id)
    {
        var existing = await repository.GetByIdAsync(id);
        if (existing is null)
            return TypedResults.NotFound();

        await repository.DeleteAsync(existing);
        return TypedResults.NoContent();
    }

    private static async Task<Results<Ok<PagedResult<TrackResponse>>, NotFound>> GetTracksByAlbumAsync(
        ITrackRepository trackRepository,
        int albumId,
        int page = 1,
        int pageSize = 10)
    {
        var result = await trackRepository.GetByAlbumIdAsync(albumId, page, pageSize);
        var response = new PagedResult<TrackResponse>
        {
            Data = result.Data.Select(t => t.ToResponse()),
            TotalCount = result.TotalCount,
            Page = result.Page,
            PageSize = result.PageSize
        };
        return TypedResults.Ok(response);
    }
}

internal record AlbumQuery(
    string? Title,
    int? ArtistId,
    int? GenreId,
    DateOnly? ReleaseDateFrom,
    DateOnly? ReleaseDateTo,
    int Page = 1,
    int PageSize = 10
);