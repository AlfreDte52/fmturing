using FMTuring.Core;

namespace FMTuring.Core.Tests;

/// <summary>La séptupla coincide con la Etapa 3 (§3, §4 y §7).</summary>
public class MachineDefinitionTests
{
    private static readonly TuringMachineDefinition M = BinaryAdditionMachine.Definition;

    [Fact]
    public void Tiene37EstadosY141Transiciones()
    {
        Assert.Equal(37, M.States.Count);
        Assert.Equal(141, M.Transitions.Count);
    }

    [Fact]
    public void AlfabetosCoincidenConLaEtapa3()
    {
        Assert.Equal(new[] { '0', '1' }, M.InputAlphabet);
        Assert.Equal(new[] { '0', '1', 'B', '+', '=', 'X', 'Y' }, M.TapeAlphabet);
        Assert.Equal('B', M.Blank);
        Assert.Equal("q0", M.InitialState);
        Assert.Equal(new[] { "qf" }, M.FinalStates);
    }

    [Fact]
    public void SoloLaTransicionHaciaQfUsaS()
    {
        var withS = Assert.Single(M.Transitions, t => t.Move == Move.S);
        Assert.Equal("(q35, = = qf, =, S)", withS.ToString());
    }

    [Fact]
    public void QfNoTieneTransicionesDeSalida()
    {
        Assert.DoesNotContain(M.Transitions, t => t.From == "qf");
    }

    [Theory]
    [InlineData("q0", 0, 2)]
    [InlineData("q1", 4, 3)]
    [InlineData("q15", 6, 3)]
    [InlineData("q22", 8, 7)]
    [InlineData("q29", 9, 7)]
    [InlineData("q33", 1, 3)]
    [InlineData("q35", 5, 5)]
    [InlineData("qf", 1, 0)]
    public void GradosDeEntradaYSalida(string state, int inDegree, int outDegree)
    {
        Assert.Equal(inDegree, M.InDegree(state));
        Assert.Equal(outDegree, M.OutDegree(state));
    }

    [Fact]
    public void LaSumaDeLosGradosEsElTotalDeTransiciones()
    {
        Assert.Equal(141, M.States.Sum(M.InDegree));
        Assert.Equal(141, M.States.Sum(M.OutDegree));
    }

    [Fact]
    public void UnaDefinicionNoDeterministaSeRechaza()
    {
        Transition[] duplicated =
        [
            new("q0", '0', "qf", '0', Move.S),
            new("q0", '0', "q0", '1', Move.R),
        ];

        Assert.Throws<ArgumentException>(() => new TuringMachineDefinition(
            ["q0", "qf"], ['0', '1'], ['0', '1', 'B'], duplicated, "q0", 'B', ["qf"]));
    }

    [Fact]
    public void CadaTransicionTieneExplicacion()
    {
        Assert.All(M.Transitions, t => Assert.NotEqual(t.ToString(), StepExplainer.Explain(t)));
    }
}
