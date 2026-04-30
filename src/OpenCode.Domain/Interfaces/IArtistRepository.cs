using OpenCode.Domain.Entities;
using OpenCode.Domain.Pagination;

namespace OpenCode.Domain.Interfaces;

public interface IArtistRepository : IRepository<Artist>
{
    Task<PagedResult<Artist>> GetAllAsync(
        string? name, int? genreId,
        int page = 1, int pageSize = 10,
        CancellationToken cancellationToken = default);

    Task<Artist?> GetByIdWithGenresAsync(int id, CancellationToken cancellationToken = default);
}