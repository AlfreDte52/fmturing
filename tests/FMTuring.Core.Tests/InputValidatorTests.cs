using FMTuring.Core;

namespace FMTuring.Core.Tests;

public class InputValidatorTests
{
    [Theory]
    [InlineData("101", "11")]
    [InlineData("0011", "1")]
    [InlineData("", "1")]
    [InlineData("1", "")]
    [InlineData("1111111111111111", "1")]
    public void AceptaEntradasDelModelo(string x, string y)
    {
        Assert.True(InputValidator.Validate(x, y).IsValid);
    }

    [Theory]
    [InlineData("12", "1")]
    [InlineData("1", "1 0")]
    [InlineData("10101010101010101", "1")]
    public void RechazaLoQueNoCabeEnElModelo(string x, string y)
    {
        var result = InputValidator.Validate(x, y);

        Assert.False(result.IsValid);
        Assert.NotEmpty(result.Errors);
    }

    [Fact]
    public void TapeDescribeOmiteLosBlancosDeLosExtremos()
    {
        var tape = new Tape("1B1");

        Assert.Equal("(q0, λ, 1, B1)", tape.Describe("q0").ToString());
    }
}
