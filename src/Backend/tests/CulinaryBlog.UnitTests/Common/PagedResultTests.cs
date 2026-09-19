using CulinaryBlog.Application.Common.Models;

namespace CulinaryBlog.UnitTests.Common;

public sealed class PagedResultTests
{
    [Theory]
    [InlineData(0, 12, 0)]
    [InlineData(12, 12, 1)]
    [InlineData(13, 12, 2)]
    [InlineData(50, 50, 1)]
    public void TotalPages_GivenTotalCount_RoundsUp(int totalCount, int pageSize, int expected)
    {
        var result = new PagedResult<int>([], 1, pageSize, totalCount);

        Assert.Equal(expected, result.TotalPages);
    }

    [Fact]
    public void Navigation_OnMiddlePage_HasNextAndPrevious()
    {
        var result = new PagedResult<int>([1], 2, 1, 3);

        Assert.True(result.HasNextPage);
        Assert.True(result.HasPreviousPage);
    }

    [Fact]
    public void Navigation_OnOnlyPage_HasNeitherNextNorPrevious()
    {
        var result = new PagedResult<int>([1, 2], 1, PagedResult<int>.DefaultPageSize, 2);

        Assert.False(result.HasNextPage);
        Assert.False(result.HasPreviousPage);
    }

    [Fact]
    public void TotalPages_WithZeroPageSize_ReturnsZero()
    {
        var result = new PagedResult<int>([], 1, 0, 10);

        Assert.Equal(0, result.TotalPages);
    }
}
