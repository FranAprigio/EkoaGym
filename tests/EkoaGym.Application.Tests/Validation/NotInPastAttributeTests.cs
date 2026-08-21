using EkoaGym.Application.Common.Validation;

namespace EkoaGym.Application.Tests.Validation;

public class NotInPastAttributeTests
{
    private readonly NotInPastAttribute _attribute = new();

    [Fact]
    public void IsValid_AcceptsToday()
    {
        Assert.True(_attribute.IsValid(DateOnly.FromDateTime(DateTime.UtcNow)));
    }

    [Fact]
    public void IsValid_AcceptsFutureDate()
    {
        Assert.True(_attribute.IsValid(DateOnly.FromDateTime(DateTime.UtcNow).AddDays(30)));
    }

    [Fact]
    public void IsValid_RejectsPastDate()
    {
        Assert.False(_attribute.IsValid(DateOnly.FromDateTime(DateTime.UtcNow).AddDays(-1)));
    }
}
