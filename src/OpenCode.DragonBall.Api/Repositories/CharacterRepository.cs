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
        string? name, string? introductionPhase,
        int page = 1, int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        if (pageSize > 100) pageSize = 100;
        if (page < 1) page = 1;

        var query = _context.Characters.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(name))
            query = query.Where(c => c.Name.Contains(name));

        if (!string.IsNullOrWhiteSpace(introductionPhase))
            query = query.Where(c => c.IntroductionPhase == introductionPhase);

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
}