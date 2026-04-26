using Microsoft.EntityFrameworkCore;
using OpenCode.Domain.Data;
using OpenCode.Domain.Entities;
using OpenCode.Domain.Implementations;
using OpenCode.Domain.Interfaces;
using OpenCode.Domain.Pagination;

namespace OpenCode.DragonBall.Api.Repositories;

public class CharacterRepository : Repository<Character>, ICharacterRepository
{
    private readonly DragonBallContext _context;

    public CharacterRepository(DragonBallContext context) : base(context)
    {
        _context = context;
    }

    public async Task<PagedResult<Character>> GetAllAsync(
        string? name, string? race, string? minKi, string? maxKi, int? planetId,
        int page = 1, int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        if (pageSize > 100) pageSize = 100;
        if (page < 1) page = 1;

        IQueryable<Character> query = _context.Characters
            .AsNoTracking()
            .Include(c => c.Planet)
            .Include(c => c.Transformations);

        if (!string.IsNullOrWhiteSpace(name))
            query = query.Where(c => c.Name.Contains(name));

        if (!string.IsNullOrWhiteSpace(race))
            query = query.Where(c => c.Race == race);

        if (!string.IsNullOrWhiteSpace(minKi))
            query = query.Where(c => c.Ki.CompareTo(minKi) >= 0);

        if (!string.IsNullOrWhiteSpace(maxKi))
            query = query.Where(c => c.Ki.CompareTo(maxKi) <= 0);

        if (planetId.HasValue)
            query = query.Where(c => c.PlanetId == planetId);

        var totalCount = await query.CountAsync(cancellationToken);
        var data = await query
            .OrderBy(c => c.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<Character>
        {
            Data = data,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    public override async Task<Character?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _context.Characters
            .Include(c => c.Planet)
            .Include(c => c.Transformations)
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
    }
}
