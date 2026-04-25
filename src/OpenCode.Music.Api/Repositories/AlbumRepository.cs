using Microsoft.EntityFrameworkCore;
using OpenCode.Domain.Data;
using OpenCode.Domain.Entities;
using OpenCode.Domain.Implementations;
using OpenCode.Domain.Interfaces;
using OpenCode.Domain.Pagination;

namespace OpenCode.Music.Api.Repositories;

public class AlbumRepository : Repository<Album>, IAlbumRepository
{
    private readonly MusicContext _context;

    public AlbumRepository(MusicContext context) : base(context)
    {
        _context = context;
    }

    public async Task<PagedResult<Album>> GetAllAsync(
        string? title, int? artistId, int? genreId,
        DateOnly? releaseDateFrom, DateOnly? releaseDateTo,
        int page = 1, int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        if (pageSize > 100) pageSize = 100;
        if (page < 1) page = 1;

        IQueryable<Album> query = _context.Albums
            .AsNoTracking()
            .Include(a => a.Artist);

        if (!string.IsNullOrWhiteSpace(title))
            query = query.Where(a => a.Title.Contains(title));

        if (artistId.HasValue)
            query = query.Where(a => a.ArtistId == artistId.Value);

        if (genreId.HasValue)
            query = query.Where(a => a.Artist.ArtistGenres.Any(ag => ag.GenreId == genreId.Value));

        if (releaseDateFrom.HasValue)
            query = query.Where(a => a.ReleaseDate >= releaseDateFrom.Value);

        if (releaseDateTo.HasValue)
            query = query.Where(a => a.ReleaseDate <= releaseDateTo.Value);

        var totalCount = await query.CountAsync(cancellationToken);
        var data = await query
            .OrderBy(a => a.Title)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<Album>
        {
            Data = data,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<PagedResult<Album>> GetByArtistIdAsync(
        int artistId, int page = 1, int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        if (pageSize > 100) pageSize = 100;
        if (page < 1) page = 1;

        IQueryable<Album> query = _context.Albums
            .AsNoTracking()
            .Include(a => a.Artist)
            .Where(a => a.ArtistId == artistId);

        var totalCount = await query.CountAsync(cancellationToken);
        var data = await query
            .OrderBy(a => a.ReleaseDate)
                .ThenBy(a => a.Title)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<Album>
        {
            Data = data,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<Album?> GetByIdWithArtistAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _context.Albums
            .AsNoTracking()
            .Include(a => a.Artist)
            .FirstOrDefaultAsync(a => a.Id == id, cancellationToken);
    }
}