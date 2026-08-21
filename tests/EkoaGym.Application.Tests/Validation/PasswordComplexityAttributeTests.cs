using EkoaGym.Application.Common.Validation;

namespace EkoaGym.Application.Tests.Validation;

public class PasswordComplexityAttributeTests
{
    private readonly PasswordComplexityAttribute _attribute = new();

    [Theory]
    [InlineData("Password1")]
    [InlineData("abcdefg1")]
    [InlineData("12345678a")]
    public void IsValid_AcceptsPasswordsMeetingComplexity(string password)
    {
        Assert.True(_attribute.IsValid(password));
    }

    [Theory]
    [InlineData("short1")]      // too short
    [InlineData("alllettersnodigits")] // no digit
    [InlineData("12345678")]    // no letter
    [InlineData("")]
    public void IsValid_RejectsPasswordsFailingComplexity(string password)
    {
        Assert.False(_attribute.IsValid(password));
    }

    [Fact]
    public void IsValid_RejectsNonStringValue()
    {
        Assert.False(_attribute.IsValid(12345678));
    }
}
