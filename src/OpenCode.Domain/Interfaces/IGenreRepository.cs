using OpenCode.Domain.Entities;
using OpenCode.Domain.Pagination;

namespace OpenCode.Domain.Interfaces;

public interface IGenreRepository : IRepository<Genre>
{
    Task<PagedResult<Genre>> GetAllAsync(
        string? name,
        int page = 1, int pageSize = 10,
        CancellationToken cancellationToken = default);
}