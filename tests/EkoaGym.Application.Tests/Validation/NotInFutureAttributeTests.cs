using EkoaGym.Application.Common.Validation;

namespace EkoaGym.Application.Tests.Validation;

public class NotInFutureAttributeTests
{
    private readonly NotInFutureAttribute _attribute = new();

    [Fact]
    public void IsValid_AcceptsToday()
    {
        Assert.True(_attribute.IsValid(DateOnly.FromDateTime(DateTime.UtcNow)));
    }

    [Fact]
    public void IsValid_AcceptsPastDate()
    {
        Assert.True(_attribute.IsValid(DateOnly.FromDateTime(DateTime.UtcNow).AddYears(-30)));
    }

    [Fact]
    public void IsValid_RejectsFutureDate()
    {
        Assert.False(_attribute.IsValid(DateOnly.FromDateTime(DateTime.UtcNow).AddDays(1)));
    }

    [Fact]
    public void IsValid_RejectsNonDateValue()
    {
        Assert.False(_attribute.IsValid("not a date"));
    }
}
