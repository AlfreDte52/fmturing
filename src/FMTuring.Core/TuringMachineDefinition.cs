namespace FMTuring.Core;

/// <summary>
/// Séptupla M = (Q, Σ, Γ, δ, q0, B, F) de una máquina de Turing determinista de una cinta.
/// Al construirse comprueba que la definición sea coherente (Etapa 2 §1).
/// </summary>
public sealed class TuringMachineDefinition
{
    private readonly Dictionary<(string State, char Symbol), Transition> _delta;

    /// <summary>Crea y valida la definición de la máquina.</summary>
    /// <exception cref="ArgumentException">Si la séptupla no es coherente.</exception>
    public TuringMachineDefinition(
        IReadOnlyList<string> states,
        IReadOnlyList<char> inputAlphabet,
        IReadOnlyList<char> tapeAlphabet,
        IReadOnlyList<Transition> transitions,
        string initialState,
        char blank,
        IReadOnlyCollection<string> finalStates)
    {
        States = states;
        InputAlphabet = inputAlphabet;
        TapeAlphabet = tapeAlphabet;
        Transitions = transitions;
        InitialState = initialState;
        Blank = blank;
        FinalStates = new HashSet<string>(finalStates);

        var stateSet = new HashSet<string>(states);
        if (stateSet.Count != states.Count)
        {
            throw new ArgumentException("Q contiene estados repetidos.", nameof(states));
        }

        if (!stateSet.Contains(initialState))
        {
            throw new ArgumentException("q0 debe pertenecer a Q.", nameof(initialState));
        }

        if (!FinalStates.IsSubsetOf(stateSet))
        {
            throw new ArgumentException("F debe ser subconjunto de Q.", nameof(finalStates));
        }

        if (!inputAlphabet.All(tapeAlphabet.Contains))
        {
            throw new ArgumentException("Σ debe ser subconjunto de Γ.", nameof(inputAlphabet));
        }

        if (!tapeAlphabet.Contains(blank) || inputAlphabet.Contains(blank))
        {
            throw new ArgumentException("B debe pertenecer a Γ − Σ.", nameof(blank));
        }

        _delta = new Dictionary<(string, char), Transition>();
        foreach (var t in transitions)
        {
            if (!stateSet.Contains(t.From) || !stateSet.Contains(t.To))
            {
                throw new ArgumentException($"La transición {t} usa un estado fuera de Q.", nameof(transitions));
            }

            if (!tapeAlphabet.Contains(t.Read) || !tapeAlphabet.Contains(t.Write))
            {
                throw new ArgumentException($"La transición {t} usa un símbolo fuera de Γ.", nameof(transitions));
            }

            if (FinalStates.Contains(t.From))
            {
                throw new ArgumentException($"La transición {t} sale de un estado final.", nameof(transitions));
            }

            if (!_delta.TryAdd((t.From, t.Read), t))
            {
                throw new ArgumentException($"δ no es determinista: hay dos transiciones para ({t.From}, {t.Read}).", nameof(transitions));
            }
        }
    }

    /// <summary>Q: conjunto finito de estados, en orden de presentación.</summary>
    public IReadOnlyList<string> States { get; }

    /// <summary>Σ: alfabeto de entrada.</summary>
    public IReadOnlyList<char> InputAlphabet { get; }

    /// <summary>Γ: alfabeto de cinta.</summary>
    public IReadOnlyList<char> TapeAlphabet { get; }

    /// <summary>δ como lista de transiciones.</summary>
    public IReadOnlyList<Transition> Transitions { get; }

    /// <summary>q0: estado inicial.</summary>
    public string InitialState { get; }

    /// <summary>B: símbolo blanco.</summary>
    public char Blank { get; }

    /// <summary>F: estados de aceptación.</summary>
    public IReadOnlySet<string> FinalStates { get; }

    /// <summary>Busca δ(state, symbol). Devuelve false si no está definida (la máquina se detiene).</summary>
    public bool TryGetTransition(string state, char symbol, out Transition transition)
    {
        if (_delta.TryGetValue((state, symbol), out var found))
        {
            transition = found;
            return true;
        }

        transition = null!;
        return false;
    }

    /// <summary>Grado de entrada: transiciones que llegan al estado (los bucles cuentan).</summary>
    public int InDegree(string state) => Transitions.Count(t => t.To == state);

    /// <summary>Grado de salida: transiciones que parten del estado (los bucles cuentan).</summary>
    public int OutDegree(string state) => Transitions.Count(t => t.From == state);
}
