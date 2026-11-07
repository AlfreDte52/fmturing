using FMTuring.Core;

namespace FMTuring.Core.Tests;

/// <summary>La variante por conteo coincide con el documento de la introducción.</summary>
public class CountingMachineTests
{
    private readonly Simulator _simulator = CountingAdditionMachine.CreateSimulator();

    [Fact]
    public void TieneOnceEstadosYVeintinueveTransiciones()
    {
        var m = CountingAdditionMachine.Definition;

        Assert.Equal(11, m.States.Count);
        Assert.Equal(29, m.Transitions.Count);
        Assert.Equal(new[] { '0', '1', '#', 'B' }, m.TapeAlphabet);
        Assert.Equal(new[] { "q10" }, m.FinalStates);
        Assert.Equal(19, m.Transitions.Select(t => (t.From, t.To)).Distinct().Count());
    }

    [Theory]
    [InlineData("q0", 0, 2)]
    [InlineData("q1", 4, 3)]
    [InlineData("q2", 1, 2)]
    [InlineData("q3", 4, 3)]
    [InlineData("q4", 3, 3)]
    [InlineData("q5", 3, 3)]
    [InlineData("q6", 3, 3)]
    [InlineData("q7", 2, 3)]
    [InlineData("q8", 2, 3)]
    [InlineData("q9", 5, 4)]
    [InlineData("q10", 2, 0)]
    public void GradosDelDocumento(string state, int inDegree, int outDegree)
    {
        Assert.Equal(inDegree, CountingAdditionMachine.Definition.InDegree(state));
        Assert.Equal(outDegree, CountingAdditionMachine.Definition.OutDegree(state));
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(4)]
    [InlineData(5)]
    public void SumaTodosLosParesHastaCincoBits(int lengthX)
    {
        for (var lengthY = 1; lengthY <= 5; lengthY++)
        {
            for (var a = 0; a < 1 << lengthX; a++)
            {
                for (var b = 0; b < 1 << lengthY; b++)
                {
                    var x = Convert.ToString(a, 2).PadLeft(lengthX, '0');
                    var y = Convert.ToString(b, 2).PadLeft(lengthY, '0');

                    var trace = _simulator.Run(x, y);

                    Assert.Equal(ExecutionOutcome.Accepted, trace.Outcome);
                    Assert.Equal(a + b, Convert.ToInt32(trace.FinalTape, 2));
                    Assert.Equal("q10", trace.Last.Description.State);
                }
            }
        }
    }

    [Theory]
    [InlineData("1", "1", 19, "10", 1)]
    [InlineData("11", "10", 36, "101", 2)]
    [InlineData("101", "11", 49, "1000", 3)]
    [InlineData("0011", "1", 24, "0100", 1)]
    [InlineData("111111", "111111", 1170, "1111110", 63)]
    public void PasosResultadoYVueltas(string x, string y, int steps, string tape, int laps)
    {
        var trace = _simulator.Run(x, y);

        Assert.Equal(steps, trace.StepCount);
        Assert.Equal(tape, trace.FinalTape);
        Assert.Equal(laps, CountingAdditionMachine.LapsUpTo(trace, trace.StepCount));
    }

    [Fact]
    public void LosPasosCrecenConElValorDeN2()
    {
        var corto = _simulator.Run("1", "001").StepCount;
        var largo = _simulator.Run("1", "100").StepCount;

        Assert.True(largo > corto);
    }

    /// <summary>Las cifras de la tabla comparativa de la introducción.</summary>
    [Theory]
    [InlineData(1, 19, 54)]
    [InlineData(3, 105, 196)]
    [InlineData(6, 1170, 574)]
    [InlineData(10, 26654, 1386)]
    [InlineData(16, 2490416, 3264)]
    public void ComparacionConLaMaquinaCompleta(int ones, int countingSteps, int fullSteps)
    {
        var operand = new string('1', ones);
        var input = Simulator.BuildInput(operand, operand);

        var counting = _simulator.Count(input, 3_000_000);
        var full = new Simulator().Count(input, 3_000_000);

        Assert.Equal((ExecutionOutcome.Accepted, countingSteps), (counting.Outcome, counting.Steps));
        Assert.Equal((ExecutionOutcome.Accepted, fullSteps), (full.Outcome, full.Steps));
        Assert.Equal(Convert.ToString(2 * ((1 << ones) - 1), 2), counting.FinalTape);
    }

    [Theory]
    [InlineData("11B", "q2")]
    [InlineData("B1", "q0")]
    public void RechazaOperandosVacios(string input, string state)
    {
        var trace = _simulator.Run(input);

        Assert.Equal(ExecutionOutcome.Rejected, trace.Outcome);
        Assert.Equal(state, trace.Last.Description.State);
    }

    [Fact]
    public void CadaTransicionTieneExplicacionYFase()
    {
        foreach (var t in CountingAdditionMachine.Transitions)
        {
            Assert.NotEqual(t.ToString(), CountingAdditionMachine.Explain(t));
            Assert.NotNull(CountingAdditionMachine.PhaseOf(t.From));
            Assert.NotEmpty(CountingAdditionMachine.RoleOf(t.From));
        }
    }

    /// <summary>Las DI coinciden línea por línea con las del documento.</summary>
    [Theory]
    [InlineData("1B1")]
    [InlineData("11B10")]
    public void DescripcionesInstantaneasDelDocumento(string input)
    {
        var golden = File.ReadAllLines(Path.Combine(AppContext.BaseDirectory, "Golden", $"conteo_{input}.txt"))
            .Where(line => line.Length > 0)
            .ToArray();

        var trace = _simulator.Run(input);

        Assert.Equal(golden.Length, trace.Steps.Count);
        for (var i = 0; i < golden.Length; i++)
        {
            var step = trace.Steps[i];
            Assert.Equal(golden[i], $"{i}|{step.Transition?.ToString() ?? "—"}|{step.Description}");
        }
    }

    [Fact]
    public void ElLimiteDeLaVarianteEsDeSeisBits()
    {
        Assert.True(InputValidator.Validate("111111", "111111", CountingAdditionMachine.MaxBits).IsValid);
        Assert.False(InputValidator.Validate("1111111", "1", CountingAdditionMachine.MaxBits).IsValid);
    }
}
