using OpenCode.Domain.Pagination;

namespace OpenCode.Domain.Tests.Pagination;

public class PagedResultEdgeCaseTests
{
    [Fact]
    public void TotalPages_When_TotalCount_Is_MaxInt()
    {
        var result = new PagedResult<string>
        {
            Data = Enumerable.Empty<string>(),
            TotalCount = int.MaxValue,
            Page = 1,
            PageSize = 10
        };

        Assert.True(result.TotalPages > 0);
    }

    [Fact]
    public void Data_Default_Is_Empty_Not_Null()
    {
        var result = new PagedResult<int>();

        Assert.NotNull(result.Data);
    }

    [Fact]
    public void Can_Set_Data_To_Empty_List()
    {
        var result = new PagedResult<int>
        {
            Data = new List<int>(),
            TotalCount = 0,
            Page = 1,
            PageSize = 10
        };

        Assert.Empty(result.Data);
        Assert.Equal(0, result.TotalPages);
    }

    [Fact]
    public void Large_DataSet_Computes_Correct_Pages()
    {
        var items = Enumerable.Range(1, 1000).ToList();
        var result = new PagedResult<int>
        {
            Data = items,
            TotalCount = 1000,
            Page = 50,
            PageSize = 20
        };

        Assert.Equal(50, result.TotalPages);
        Assert.Equal(50, result.Page);
        Assert.Equal(20, result.PageSize);
        Assert.Equal(1000, result.TotalCount);
    }

    [Fact]
    public void Page_One_With_Small_PageSize()
    {
        var result = new PagedResult<string>
        {
            Data = new[] { "only" },
            TotalCount = 1,
            Page = 1,
            PageSize = 1
        };

        Assert.Equal(1, result.TotalPages);
    }

    [Fact]
    public void Large_PageSize_Single_Page()
    {
        var result = new PagedResult<string>
        {
            Data = new[] { "a", "b", "c" },
            TotalCount = 3,
            Page = 1,
            PageSize = 100
        };

        Assert.Equal(1, result.TotalPages);
    }

    [Fact]
    public void Exactly_One_Item_Per_Page()
    {
        var result = new PagedResult<string>
        {
            Data = new[] { "first" },
            TotalCount = 5,
            Page = 3,
            PageSize = 1
        };

        Assert.Equal(5, result.TotalPages);
        Assert.Equal(3, result.Page);
    }
}
