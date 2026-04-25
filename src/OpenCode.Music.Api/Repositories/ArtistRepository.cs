using Microsoft.EntityFrameworkCore;
using OpenCode.Domain.Data;
using OpenCode.Domain.Entities;
using OpenCode.Domain.Implementations;
using OpenCode.Domain.Interfaces;
using OpenCode.Domain.Pagination;

namespace OpenCode.Music.Api.Repositories;

public class ArtistRepository : Repository<Artist>, IArtistRepository
{
    private readonly MusicContext _context;

    public ArtistRepository(MusicContext context) : base(context)
    {
        _context = context;
    }

    public async Task<PagedResult<Artist>> GetAllAsync(
        string? name, int? genreId,
        int page = 1, int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        if (pageSize > 100) pageSize = 100;
        if (page < 1) page = 1;

        IQueryable<Artist> query = _context.Artists
            .AsNoTracking()
            .Include(a => a.ArtistGenres)
                .ThenInclude(ag => ag.Genre);

        if (!string.IsNullOrWhiteSpace(name))
            query = query.Where(a => a.Name.Contains(name));

        if (genreId.HasValue)
            query = query.Where(a => a.ArtistGenres.Any(ag => ag.GenreId == genreId.Value));

        var totalCount = await query.CountAsync(cancellationToken);
        var data = await query
            .OrderBy(a => a.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<Artist>
        {
            Data = data,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<Artist?> GetByIdWithGenresAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _context.Artists
            .AsNoTracking()
            .Include(a => a.ArtistGenres)
                .ThenInclude(ag => ag.Genre)
            .FirstOrDefaultAsync(a => a.Id == id, cancellationToken);
    }
}