using Microsoft.AspNetCore.Http.HttpResults;
using OpenCode.Domain.Entities;
using OpenCode.Domain.Interfaces;
using OpenCode.Domain.Pagination;
using OpenCode.Music.Api.Dtos;
using OpenCode.Music.Api.Repositories;

namespace OpenCode.Music.Api.Endpoints;

public static class Artists
{
    public static RouteGroupBuilder MapArtistEndpoints(this RouteGroupBuilder group)
    {
        group.MapGet("/", GetAllAsync).AllowAnonymous();
        group.MapGet("/{id:int}", GetByIdAsync).AllowAnonymous();
        group.MapPost("/", CreateAsync).RequireAuthorization("ApiPolicy");
        group.MapPut("/{id:int}", UpdateAsync).RequireAuthorization("ApiPolicy");
        group.MapDelete("/{id:int}", DeleteAsync).RequireAuthorization("ApiPolicy");
        group.MapGet("/{artistId:int}/albums", GetAlbumsByArtistAsync).AllowAnonymous();
        return group;
    }

    private static async Task<Results<Ok<PagedResult<ArtistResponse>>, BadRequest>> GetAllAsync(
        IArtistRepository repository,
        [AsParameters] ArtistQuery query)
    {
        var result = await repository.GetAllAsync(query.Name, query.GenreId, query.Page, query.PageSize);
        var response = new PagedResult<ArtistResponse>
        {
            Data = result.Data.Select(a => a.ToResponse()),
            TotalCount = result.TotalCount,
            Page = result.Page,
            PageSize = result.PageSize
        };
        return TypedResults.Ok(response);
    }

    private static async Task<Results<Ok<ArtistResponse>, NotFound>> GetByIdAsync(
        IArtistRepository repository,
        int id)
    {
        var artist = await repository.GetByIdAsync(id);
        if (artist is null)
            return TypedResults.NotFound();

        if (artist.ArtistGenres is null || artist.ArtistGenres.Count == 0)
        {
            var repo = (ArtistRepository)repository;
            var fullArtist = await repo.GetByIdWithGenresAsync(id);
            if (fullArtist is null)
                return TypedResults.NotFound();
            return TypedResults.Ok(fullArtist.ToResponse());
        }

        return TypedResults.Ok(artist.ToResponse());
    }

    private static async Task<Results<Created<ArtistResponse>, BadRequest>> CreateAsync(
        IArtistRepository repository,
        CreateArtistRequest request)
    {
        var artist = new Artist
        {
            Name = request.Name,
            Biography = request.Biography
        };

        if (request.GenreIds?.Count > 0)
        {
            foreach (var genreId in request.GenreIds)
            {
                artist.ArtistGenres.Add(new ArtistGenre
                {
                    GenreId = genreId
                });
            }
        }

        var created = await repository.AddAsync(artist);
        return TypedResults.Created($"/api/artists/{created.Id}", created.ToResponse());
    }

    private static async Task<Results<Ok<ArtistResponse>, NotFound, BadRequest>> UpdateAsync(
        IArtistRepository repository,
        int id,
        UpdateArtistRequest request)
    {
        var existing = await repository.GetByIdAsync(id);
        if (existing is null)
            return TypedResults.NotFound();

        existing.Name = request.Name;
        existing.Biography = request.Biography;

        existing.ArtistGenres.Clear();
        if (request.GenreIds?.Count > 0)
        {
            foreach (var genreId in request.GenreIds)
            {
                existing.ArtistGenres.Add(new ArtistGenre
                {
                    ArtistId = id,
                    GenreId = genreId
                });
            }
        }

        await repository.UpdateAsync(existing);
        return TypedResults.Ok(existing.ToResponse());
    }

    private static async Task<Results<NoContent, NotFound>> DeleteAsync(
        IArtistRepository repository,
        int id)
    {
        var existing = await repository.GetByIdAsync(id);
        if (existing is null)
            return TypedResults.NotFound();

        await repository.DeleteAsync(existing);
        return TypedResults.NoContent();
    }

    private static async Task<Results<Ok<PagedResult<AlbumResponse>>, NotFound>> GetAlbumsByArtistAsync(
        IAlbumRepository albumRepository,
        int artistId,
        int page = 1,
        int pageSize = 10)
    {
        var result = await albumRepository.GetByArtistIdAsync(artistId, page, pageSize);
        var response = new PagedResult<AlbumResponse>
        {
            Data = result.Data.Select(a => a.ToResponse()),
            TotalCount = result.TotalCount,
            Page = result.Page,
            PageSize = result.PageSize
        };
        return TypedResults.Ok(response);
    }
}

internal record ArtistQuery(
    string? Name,
    int? GenreId,
    int Page = 1,
    int PageSize = 10
);