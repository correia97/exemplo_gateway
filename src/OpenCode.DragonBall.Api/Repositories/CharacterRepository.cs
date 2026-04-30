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

        if (planetId.HasValue)
            query = query.Where(c => c.PlanetId == planetId);

        // Parse Ki filter values to numeric for correct comparison
        var minKiNumeric = ParseKiToNumeric(minKi);
        var maxKiNumeric = ParseKiToNumeric(maxKi);

        var allData = await query.ToListAsync(cancellationToken);

        // Apply Ki filter in-memory with numeric comparison
        if (minKiNumeric.HasValue)
            allData = allData.Where(c =>
            {
                var val = ParseKiToNumeric(c.Ki);
                return val.HasValue && val.Value >= minKiNumeric.Value;
            }).ToList();

        if (maxKiNumeric.HasValue)
            allData = allData.Where(c =>
            {
                var val = ParseKiToNumeric(c.Ki);
                return val.HasValue && val.Value <= maxKiNumeric.Value;
            }).ToList();

        var totalCount = allData.Count;
        var data = allData
            .OrderBy(c => c.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        return new PagedResult<Character>
        {
            Data = data,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    private static decimal? ParseKiToNumeric(string? kiValue)
    {
        if (string.IsNullOrWhiteSpace(kiValue)) return null;
        var cleaned = kiValue.TrimEnd('+').Trim();
        var parts = cleaned.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (!decimal.TryParse(parts[0], System.Globalization.NumberStyles.Any,
                System.Globalization.CultureInfo.InvariantCulture, out var value))
            return null;
        if (parts.Length > 1)
        {
            var magnitude = parts[1].ToLowerInvariant() switch
            {
                "thousand" => 1_000m,
                "million" => 1_000_000m,
                "billion" => 1_000_000_000m,
                "trillion" => 1_000_000_000_000m,
                "quadrillion" => 1_000_000_000_000_000m,
                "quintillion" => 1_000_000_000_000_000_000m,
                "sextillion" => 1_000_000_000_000_000_000_000m,
                "septillion" => 1_000_000_000_000_000_000_000_000m,
                _ => 1m
            };
            value *= magnitude;
        }
        return value;
    }

    public override async Task<Character?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _context.Characters
            .Include(c => c.Planet)
            .Include(c => c.Transformations)
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
    }
}
