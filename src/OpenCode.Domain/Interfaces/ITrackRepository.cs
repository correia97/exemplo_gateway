using OpenCode.Domain.Entities;
using OpenCode.Domain.Pagination;

namespace OpenCode.Domain.Interfaces;

public interface ITrackRepository : IRepository<Track>
{
    Task<PagedResult<Track>> GetAllAsync(
        string? name, int? albumId,
        int page = 1, int pageSize = 10,
        CancellationToken cancellationToken = default);

    Task<PagedResult<Track>> GetByAlbumIdAsync(
        int albumId, int page = 1, int pageSize = 10,
        CancellationToken cancellationToken = default);
}