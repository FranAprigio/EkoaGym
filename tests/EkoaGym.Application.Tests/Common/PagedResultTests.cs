using EkoaGym.Application.Common;

namespace EkoaGym.Application.Tests.Common;

public class PagedResultTests
{
    [Theory]
    [InlineData(0, 20, 0)]
    [InlineData(1, 20, 1)]
    [InlineData(20, 20, 1)]
    [InlineData(21, 20, 2)]
    [InlineData(100, 25, 4)]
    public void TotalPages_ComputesCeilingDivision(int totalCount, int pageSize, int expectedTotalPages)
    {
        var result = new PagedResult<int>([], 1, pageSize, totalCount);

        Assert.Equal(expectedTotalPages, result.TotalPages);
    }

    [Fact]
    public void TotalPages_ZeroPageSize_DoesNotThrow()
    {
        var result = new PagedResult<int>([], 1, 0, 10);

        Assert.Equal(0, result.TotalPages);
    }
}
