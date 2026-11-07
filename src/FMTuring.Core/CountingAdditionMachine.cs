namespace FMTuring.Core;

/// <summary>
/// Variante introductoria: suma contando. Resta 1 a N2 y suma 1 a N1 hasta que N2 vale 0,
/// y deja en la cinta solo el resultado. Son las 29 transiciones del documento
/// «Máquina de Turing: suma de dos números binarios» (11 estados), presentadas con la
/// notación del resto del sitio: B ∈ Γ y la función extendida como ⊢*.
/// </summary>
/// <remarks>
/// Sirve de contraste con <see cref="BinaryAdditionMachine"/>: tiene pocos estados, pero el
/// ciclo se repite tantas veces como vale N2, así que los pasos crecen con el valor y no
/// con la longitud.
/// </remarks>
public static class CountingAdditionMachine
{
    /// <summary>Estado de aceptación.</summary>
    public const string FinalState = "q10";

    /// <summary>Longitud máxima de cada operando en la interfaz (6 unos + 6 unos = 1.170 pasos).</summary>
    public const int MaxBits = 6;

    /// <summary>Q en el orden del documento: validación, ciclo y limpieza.</summary>
    public static IReadOnlyList<string> States { get; } =
        ["q0", "q1", "q2", "q3", "q4", "q5", "q8", "q9", "q6", "q7", "q10"];

    /// <summary>Γ = {0, 1, #, B}, en el orden de las columnas de la tabla δ.</summary>
    public static IReadOnlyList<char> TapeAlphabet { get; } =
        [Symbols.Zero, Symbols.One, Symbols.Hash, Symbols.Blank];

    /// <summary>δ: las 29 transiciones, en el orden de la notación de lista del documento.</summary>
    public static IReadOnlyList<Transition> Transitions { get; } =
    [
        new("q0", '0', "q1", '0', Move.R),
        new("q0", '1', "q1", '1', Move.R),
        new("q1", '0', "q1", '0', Move.R),
        new("q1", '1', "q1", '1', Move.R),
        new("q1", 'B', "q2", '#', Move.R),
        new("q2", '0', "q3", '0', Move.R),
        new("q2", '1', "q3", '1', Move.R),
        new("q3", '0', "q3", '0', Move.R),
        new("q3", '1', "q3", '1', Move.R),
        new("q3", 'B', "q4", 'B', Move.L),
        new("q4", '0', "q4", '1', Move.L),
        new("q4", '1', "q5", '0', Move.L),
        new("q4", '#', "q6", 'B', Move.R),
        new("q5", '0', "q5", '0', Move.L),
        new("q5", '1', "q5", '1', Move.L),
        new("q5", '#', "q8", '#', Move.L),
        new("q8", '1', "q8", '0', Move.L),
        new("q8", '0', "q9", '1', Move.R),
        new("q8", 'B', "q9", '1', Move.R),
        new("q9", '0', "q9", '0', Move.R),
        new("q9", '1', "q9", '1', Move.R),
        new("q9", '#', "q9", '#', Move.R),
        new("q9", 'B', "q4", 'B', Move.L),
        new("q6", '0', "q6", 'B', Move.R),
        new("q6", '1', "q6", 'B', Move.R),
        new("q6", 'B', "q7", 'B', Move.L),
        new("q7", 'B', "q7", 'B', Move.L),
        new("q7", '0', "q10", '0', Move.S),
        new("q7", '1', "q10", '1', Move.S),
    ];

    /// <summary>La séptupla, validada al construirse.</summary>
    public static TuringMachineDefinition Definition { get; } = new(
        States,
        Symbols.InputAlphabet,
        TapeAlphabet,
        Transitions,
        initialState: "q0",
        blank: Symbols.Blank,
        finalStates: [FinalState]);

    private static readonly Dictionary<string, Phase> Phases = new()
    {
        ["q0"] = Phase.A, ["q1"] = Phase.A, ["q2"] = Phase.A, ["q3"] = Phase.A,
        ["q4"] = Phase.B, ["q5"] = Phase.B, ["q8"] = Phase.B, ["q9"] = Phase.B,
        ["q6"] = Phase.C, ["q7"] = Phase.C,
    };

    private static readonly Dictionary<string, string> Roles = new()
    {
        ["q0"] = "Verifica que N1 tenga al menos un dígito",
        ["q1"] = "Recorre N1 y escribe # sobre el blanco separador",
        ["q2"] = "Verifica que N2 tenga al menos un dígito",
        ["q3"] = "Recorre N2 hasta el blanco final",
        ["q4"] = "Resta 1 a N2, o detecta que N2 = 0",
        ["q5"] = "Regresa a la izquierda hasta el #",
        ["q8"] = "Suma 1 a N1",
        ["q9"] = "Vuelve al final de N2 para repetir el ciclo",
        ["q6"] = "Borra el # y N2",
        ["q7"] = "Regresa al último dígito del resultado",
        ["q10"] = "Estado final",
    };

    private static readonly Dictionary<(string State, char Symbol), string> Texts = new()
    {
        [("q0", '0')] = "Valida el primer dígito de N1",
        [("q0", '1')] = "Valida el primer dígito de N1",
        [("q1", '0')] = "Recorre N1",
        [("q1", '1')] = "Recorre N1",
        [("q1", 'B')] = "Cambia el separador B por #",
        [("q2", '0')] = "Valida el primer dígito de N2",
        [("q2", '1')] = "Valida el primer dígito de N2",
        [("q3", '0')] = "Recorre N2",
        [("q3", '1')] = "Recorre N2",
        [("q3", 'B')] = "Llega al final de N2 y regresa al último dígito",
        [("q4", '0')] = "Resta 1 a N2: el 0 pasa a 1 y sigue el préstamo",
        [("q4", '1')] = "Resta 1 a N2: el 1 pasa a 0, termina la resta",
        [("q4", '#')] = "N2 ya vale 0: borra el # y empieza la limpieza",
        [("q5", '0')] = "Regresa hacia el #",
        [("q5", '1')] = "Regresa hacia el #",
        [("q5", '#')] = "Cruza el # y pasa a N1",
        [("q8", '1')] = "Suma 1 a N1: el 1 pasa a 0 y sigue el acarreo",
        [("q8", '0')] = "Suma 1 a N1: el 0 pasa a 1, termina la suma",
        [("q8", 'B')] = "Suma 1 a N1: el acarreo agrega un dígito nuevo",
        [("q9", '0')] = "Vuelve hacia el final de N2",
        [("q9", '1')] = "Vuelve hacia el final de N2",
        [("q9", '#')] = "Cruza el #",
        [("q9", 'B')] = "Llega al final de N2: empieza otra vuelta",
        [("q6", '0')] = "Borra un dígito de N2",
        [("q6", '1')] = "Borra un dígito de N2",
        [("q6", 'B')] = "N2 borrado: regresa hacia el resultado",
        [("q7", 'B')] = "Regresa sobre las celdas borradas",
        [("q7", '0')] = "Llega al último dígito del resultado: acepta",
        [("q7", '1')] = "Llega al último dígito del resultado: acepta",
    };

    /// <summary>Simulador listo para esta máquina.</summary>
    public static Simulator CreateSimulator() => new(Definition, Explain, PhaseOf);

    /// <summary>Fase del estado: A validación, B ciclo de resta y suma, C limpieza; null para q10.</summary>
    public static Phase? PhaseOf(string state) => Phases.TryGetValue(state, out var phase) ? phase : null;

    /// <summary>Función del estado, tal como la describe el documento.</summary>
    public static string RoleOf(string state) => Roles.TryGetValue(state, out var role) ? role : string.Empty;

    /// <summary>Explicación en palabras del paso que aplica la transición.</summary>
    public static string Explain(Transition transition) =>
        Texts.TryGetValue((transition.From, transition.Read), out var text) ? text : transition.ToString();

    /// <summary>
    /// Vueltas completas del ciclo (resta y suma) hasta el paso indicado: se cuentan las
    /// veces que se aplicó (q9, B = q4, B, L).
    /// </summary>
    public static int LapsUpTo(ExecutionTrace trace, int index)
    {
        ArgumentNullException.ThrowIfNull(trace);
        var laps = 0;
        for (var i = 1; i <= index && i < trace.Steps.Count; i++)
        {
            var t = trace.Steps[i].Transition;
            if (t is { From: "q9", Read: Symbols.Blank })
            {
                laps++;
            }
        }

        return laps;
    }
}
