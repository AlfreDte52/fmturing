namespace FMTuring.Core;

/// <summary>
/// Ejecuta la función de transición δ sobre una cadena y produce la traza completa
/// de descripciones instantáneas. La interfaz solo reproduce esta traza (DA-24).
/// </summary>
public sealed class Simulator
{
    /// <summary>Tope de seguridad de pasos (RNF-07).</summary>
    public const int DefaultMaxSteps = 10_000;

    private readonly TuringMachineDefinition _machine;
    private readonly Func<Transition, string> _explain;
    private readonly Func<string, Phase?> _phaseOf;

    /// <summary>Crea un simulador para la máquina de suma binaria.</summary>
    public Simulator()
        : this(BinaryAdditionMachine.Definition, StepExplainer.Explain, BinaryAdditionMachine.PhaseOf)
    {
    }

    /// <summary>Crea un simulador para cualquier definición.</summary>
    public Simulator(TuringMachineDefinition machine, Func<Transition, string> explain, Func<string, Phase?> phaseOf)
    {
        _machine = machine;
        _explain = explain;
        _phaseOf = phaseOf;
    }

    /// <summary>Construye la cadena de entrada x B y.</summary>
    public static string BuildInput(string x, string y) => (x ?? string.Empty) + Symbols.Blank + (y ?? string.Empty);

    /// <summary>Ejecuta la máquina con los operandos x y y separados por el blanco.</summary>
    public ExecutionTrace Run(string x, string y, int maxSteps = DefaultMaxSteps) => Run(BuildInput(x, y), maxSteps);

    /// <summary>Ejecuta la máquina sobre una cadena de entrada escrita sobre Σ ∪ {B}.</summary>
    public ExecutionTrace Run(string input, int maxSteps = DefaultMaxSteps)
    {
        ArgumentNullException.ThrowIfNull(input);
        var tape = new Tape(input);
        var state = _machine.InitialState;
        var steps = new List<ExecutionStep>
        {
            new(0, null, tape.Describe(state), tape.Head, null, "Configuración inicial"),
        };

        ExecutionOutcome outcome;
        while (true)
        {
            if (_machine.FinalStates.Contains(state))
            {
                outcome = ExecutionOutcome.Accepted;
                break;
            }

            if (steps.Count - 1 >= maxSteps)
            {
                outcome = ExecutionOutcome.StepLimitReached;
                break;
            }

            if (!_machine.TryGetTransition(state, tape.Read(), out var t))
            {
                outcome = ExecutionOutcome.Rejected;
                break;
            }

            var phase = _phaseOf(t.From);
            tape.Write(t.Write);
            tape.MoveHead(t.Move);
            state = t.To;
            steps.Add(new ExecutionStep(steps.Count, t, tape.Describe(state), tape.Head, phase, _explain(t)));
        }

        return new ExecutionTrace(input, steps, outcome, tape.Content());
    }

    /// <summary>
    /// Ejecuta la máquina sin guardar la traza: solo cuenta los pasos. Sirve para cadenas
    /// cuya traza completa no conviene guardar (la variante por conteo con 16 bits da
    /// millones de pasos).
    /// </summary>
    public (ExecutionOutcome Outcome, int Steps, string FinalTape) Count(string input, int maxSteps)
    {
        ArgumentNullException.ThrowIfNull(input);
        var tape = new Tape(input);
        var state = _machine.InitialState;
        var steps = 0;
        while (true)
        {
            if (_machine.FinalStates.Contains(state))
            {
                return (ExecutionOutcome.Accepted, steps, tape.Content());
            }

            if (steps >= maxSteps)
            {
                return (ExecutionOutcome.StepLimitReached, steps, tape.Content());
            }

            if (!_machine.TryGetTransition(state, tape.Read(), out var t))
            {
                return (ExecutionOutcome.Rejected, steps, tape.Content());
            }

            tape.Write(t.Write);
            tape.MoveHead(t.Move);
            state = t.To;
            steps++;
        }
    }
}
