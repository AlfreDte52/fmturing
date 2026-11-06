namespace FMTuring.Core;

/// <summary>Resultado de una ejecución.</summary>
public enum ExecutionOutcome
{
    /// <summary>La máquina llegó a un estado de F.</summary>
    Accepted,

    /// <summary>La máquina se detuvo fuera de F por falta de transición.</summary>
    Rejected,

    /// <summary>Se alcanzó el tope de seguridad de pasos (RNF-07).</summary>
    StepLimitReached,
}

/// <summary>
/// Un paso de la ejecución: la transición aplicada y la DI resultante.
/// El paso 0 es la configuración inicial y no tiene transición.
/// </summary>
/// <param name="Index">Número de paso.</param>
/// <param name="Transition">Transición aplicada; null en el paso 0.</param>
/// <param name="Description">DI después de aplicar la transición.</param>
/// <param name="HeadPosition">Posición absoluta del cabezal después del paso.</param>
/// <param name="Phase">Fase del estado de origen; null en el paso 0.</param>
/// <param name="Explanation">Explicación del paso en palabras.</param>
public sealed record ExecutionStep(
    int Index,
    Transition? Transition,
    InstantaneousDescription Description,
    long HeadPosition,
    Phase? Phase,
    string Explanation)
{
    /// <summary>Posición absoluta del primer símbolo de α.</summary>
    public long LeftStart => HeadPosition - Description.Left.Length;
}

/// <summary>Traza completa de una ejecución: la función extendida ⊢* desarrollada paso a paso.</summary>
public sealed class ExecutionTrace
{
    /// <summary>Crea la traza.</summary>
    public ExecutionTrace(string input, IReadOnlyList<ExecutionStep> steps, ExecutionOutcome outcome, string finalTape)
    {
        Input = input;
        Steps = steps;
        Outcome = outcome;
        FinalTape = finalTape;
    }

    /// <summary>Cadena de entrada, por ejemplo 101B11.</summary>
    public string Input { get; }

    /// <summary>Pasos, empezando por la configuración inicial (índice 0).</summary>
    public IReadOnlyList<ExecutionStep> Steps { get; }

    /// <summary>Veredicto de la ejecución.</summary>
    public ExecutionOutcome Outcome { get; }

    /// <summary>Contenido no blanco de la cinta al detenerse.</summary>
    public string FinalTape { get; }

    /// <summary>Número de transiciones aplicadas.</summary>
    public int StepCount => Steps.Count - 1;

    /// <summary>Último paso.</summary>
    public ExecutionStep Last => Steps[^1];
}
