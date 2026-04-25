using OpenCode.Domain.Pagination;

namespace OpenCode.Domain.Tests.Pagination;

public class PagedResultTests
{
    [Fact]
    public void TotalPages_WhenTotalCountIsZero_ReturnsZero()
    {
        var result = new PagedResult<string>
        {
            Data = Enumerable.Empty<string>(),
            TotalCount = 0,
            Page = 1,
            PageSize = 10
        };

        Assert.Equal(0, result.TotalPages);
    }

    [Fact]
    public void TotalPages_WhenTotalCountEqualsPageSize_ReturnsOne()
    {
        var result = new PagedResult<string>
        {
            Data = Enumerable.Range(0, 10).Select(i => $"item{i}"),
            TotalCount = 10,
            Page = 1,
            PageSize = 10
        };

        Assert.Equal(1, result.TotalPages);
    }

    [Fact]
    public void TotalPages_WhenTotalCountExceedsPageSize_RoundsUp()
    {
        var result = new PagedResult<string>
        {
            Data = Enumerable.Range(0, 10).Select(i => $"item{i}"),
            TotalCount = 25,
            Page = 1,
            PageSize = 10
        };

        Assert.Equal(3, result.TotalPages);
    }

    [Fact]
    public void TotalPages_WhenTotalCountIsExactlyMultipleOfPageSize_ReturnsExactDivision()
    {
        var result = new PagedResult<string>
        {
            Data = Enumerable.Range(0, 20).Select(i => $"item{i}"),
            TotalCount = 30,
            Page = 1,
            PageSize = 10
        };

        Assert.Equal(3, result.TotalPages);
    }

    [Fact]
    public void Constructor_InitializesDataAsEmptyEnumerable()
    {
        var result = new PagedResult<string>();
        Assert.NotNull(result.Data);
        Assert.Empty(result.Data);
    }

    [Fact]
    public void TotalPages_WhenSingleItem_ReturnsOne()
    {
        var result = new PagedResult<string>
        {
            Data = new[] { "only" },
            TotalCount = 1,
            Page = 1,
            PageSize = 10
        };

        Assert.Equal(1, result.TotalPages);
    }
}
