using Microsoft.EntityFrameworkCore;
using OpenCode.Domain.Data;
using OpenCode.Domain.Entities;
using OpenCode.Domain.Implementations;
using OpenCode.Domain.Interfaces;
using OpenCode.Domain.Pagination;

namespace OpenCode.Music.Api.Repositories;

public class TrackRepository : Repository<Track>, ITrackRepository
{
    private readonly MusicContext _context;

    public TrackRepository(MusicContext context) : base(context)
    {
        _context = context;
    }

    public async Task<PagedResult<Track>> GetAllAsync(
        string? name, int? albumId,
        int page = 1, int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        if (pageSize > 100) pageSize = 100;
        if (page < 1) page = 1;

        IQueryable<Track> query = _context.Tracks
            .AsNoTracking()
            .Include(t => t.Album);

        if (!string.IsNullOrWhiteSpace(name))
            query = query.Where(t => t.Name.Contains(name));

        if (albumId.HasValue)
            query = query.Where(t => t.AlbumId == albumId.Value);

        var totalCount = await query.CountAsync(cancellationToken);
        var data = await query
            .OrderBy(t => t.TrackNumber)
                .ThenBy(t => t.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<Track>
        {
            Data = data,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<PagedResult<Track>> GetByAlbumIdAsync(
        int albumId, int page = 1, int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        if (pageSize > 100) pageSize = 100;
        if (page < 1) page = 1;

        IQueryable<Track> query = _context.Tracks
            .AsNoTracking()
            .Include(t => t.Album)
            .Where(t => t.AlbumId == albumId);

        var totalCount = await query.CountAsync(cancellationToken);
        var data = await query
            .OrderBy(t => t.TrackNumber)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<Track>
        {
            Data = data,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }
}