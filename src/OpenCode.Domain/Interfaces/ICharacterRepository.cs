using OpenCode.Domain.Entities;
using OpenCode.Domain.Pagination;

namespace OpenCode.Domain.Interfaces;

public interface ICharacterRepository : IRepository<Character>
{
    Task<PagedResult<Character>> GetAllAsync(
        string? name, string? introductionPhase,
        int page = 1, int pageSize = 10,
        CancellationToken cancellationToken = default);
}