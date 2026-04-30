using OpenCode.Domain.Entities;
using OpenCode.Domain.Pagination;

namespace OpenCode.Domain.Interfaces;

public interface IAlbumRepository : IRepository<Album>
{
    Task<PagedResult<Album>> GetAllAsync(
        string? title, int? artistId, int? genreId,
        DateOnly? releaseDateFrom, DateOnly? releaseDateTo,
        int page = 1, int pageSize = 10,
        CancellationToken cancellationToken = default);

    Task<PagedResult<Album>> GetByArtistIdAsync(
        int artistId, int page = 1, int pageSize = 10,
        CancellationToken cancellationToken = default);

    Task<Album?> GetByIdWithArtistAsync(int id, CancellationToken cancellationToken = default);
}