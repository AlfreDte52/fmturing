using FMTuring.Core;

namespace FMTuring.Core.Tests;

/// <summary>La ejecución coincide con la verificación de las Etapas 3 y 4.</summary>
public class SimulatorTests
{
    private readonly Simulator _simulator = new();

    public static TheoryData<int, int> PairLengths()
    {
        var data = new TheoryData<int, int>();
        for (var lx = 1; lx <= 5; lx++)
        {
            for (var ly = 1; ly <= 5; ly++)
            {
                data.Add(lx, ly);
            }
        }

        return data;
    }

    /// <summary>Los 3.844 pares de operandos de 1 a 5 bits (Etapa 3 §8).</summary>
    [Theory]
    [MemberData(nameof(PairLengths))]
    public void SumaTodosLosParesDeEstaLongitud(int lengthX, int lengthY)
    {
        for (var a = 0; a < 1 << lengthX; a++)
        {
            for (var b = 0; b < 1 << lengthY; b++)
            {
                var x = Convert.ToString(a, 2).PadLeft(lengthX, '0');
                var y = Convert.ToString(b, 2).PadLeft(lengthY, '0');
                var z = Convert.ToString(a + b, 2);

                var trace = _simulator.Run(x, y);

                Assert.Equal(ExecutionOutcome.Accepted, trace.Outcome);
                Assert.Equal($"{x}+{y}={z}", trace.FinalTape);
                Assert.Equal("qf", trace.Last.Description.State);
                Assert.Equal('=', trace.Last.Description.Symbol);
            }
        }
    }

    [Theory]
    [InlineData("1", "1", 54)]
    [InlineData("10", "1", 79)]
    [InlineData("11", "1", 101)]
    [InlineData("10", "11", 114)]
    [InlineData("101", "11", 179)]
    [InlineData("1011", "110", 279)]
    public void PasosDeReferencia(string x, string y, int steps)
    {
        Assert.Equal(steps, _simulator.Run(x, y).StepCount);
    }

    [Fact]
    public void LosPasosDependenDeLaLongitudYNoDelValor()
    {
        Assert.Equal(_simulator.Run("10", "11").StepCount, _simulator.Run("11", "11").StepCount);
        Assert.Equal(_simulator.Run("101", "11").StepCount, _simulator.Run("110", "11").StepCount);
    }

    [Fact]
    public void ConservaLosOperandosYNormalizaElResultado()
    {
        Assert.Equal("0011+1=100", _simulator.Run("0011", "1").FinalTape);
        Assert.Equal("0+0=0", _simulator.Run("0", "0").FinalTape);
        Assert.Equal("000+00=0", _simulator.Run("000", "00").FinalTape);
    }

    [Fact]
    public void DieciseisBitsCabenHolgadamenteEnElTope()
    {
        var ones = new string('1', 16);
        var trace = _simulator.Run(ones, ones);

        Assert.Equal(ExecutionOutcome.Accepted, trace.Outcome);
        Assert.Equal(3264, trace.StepCount);
    }

    [Theory]
    [InlineData("", "1", "q0")]
    [InlineData("", "", "q0")]
    [InlineData("101", "", "q2")]
    [InlineData("1", "", "q2")]
    public void RechazaOperandosVacios(string x, string y, string haltState)
    {
        var trace = _simulator.Run(x, y);

        Assert.Equal(ExecutionOutcome.Rejected, trace.Outcome);
        Assert.Equal(haltState, trace.Last.Description.State);
    }

    [Fact]
    public void RechazaUnaEntradaSinSeparador()
    {
        var trace = _simulator.Run("101");

        Assert.Equal(ExecutionOutcome.Rejected, trace.Outcome);
        Assert.Equal("q2", trace.Last.Description.State);
    }

    [Fact]
    public void RespetaElTopeDePasos()
    {
        Assert.Equal(ExecutionOutcome.StepLimitReached, _simulator.Run("1", "1", maxSteps: 10).Outcome);
    }

    /// <summary>Las 54 y 179 DI coinciden texto por texto con la Etapa 4.</summary>
    [Theory]
    [InlineData("1B1")]
    [InlineData("101B11")]
    public void DescripcionesInstantaneasDeLaEtapa4(string input)
    {
        var golden = File.ReadAllLines(Path.Combine(AppContext.BaseDirectory, "Golden", $"di_{input}.txt"))
            .Where(line => line.Length > 0)
            .ToArray();

        var trace = _simulator.Run(input);

        Assert.Equal(golden.Length, trace.Steps.Count);
        for (var i = 0; i < golden.Length; i++)
        {
            var step = trace.Steps[i];
            var actual = $"{i}|{step.Transition?.ToString() ?? "—"}|{step.Description}";
            Assert.Equal(golden[i], actual);
        }
    }
}
