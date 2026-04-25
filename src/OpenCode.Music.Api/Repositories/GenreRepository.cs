using Microsoft.EntityFrameworkCore;
using OpenCode.Domain.Data;
using OpenCode.Domain.Entities;
using OpenCode.Domain.Implementations;
using OpenCode.Domain.Interfaces;
using OpenCode.Domain.Pagination;

namespace OpenCode.Music.Api.Repositories;

public class GenreRepository : Repository<Genre>, IGenreRepository
{
    private readonly MusicContext _context;

    public GenreRepository(MusicContext context) : base(context)
    {
        _context = context;
    }

    public async Task<PagedResult<Genre>> GetAllAsync(
        string? name,
        int page = 1, int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        if (pageSize > 100) pageSize = 100;
        if (page < 1) page = 1;

        var query = _context.Genres.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(name))
            query = query.Where(g => g.Name.Contains(name));

        var totalCount = await query.CountAsync(cancellationToken);
        var data = await query
            .OrderBy(g => g.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<Genre>
        {
            Data = data,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }
}