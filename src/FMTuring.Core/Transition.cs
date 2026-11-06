namespace FMTuring.Core;

/// <summary>Movimiento del cabezal: izquierda, derecha o permanecer.</summary>
public enum Move
{
    /// <summary>Izquierda.</summary>
    L,

    /// <summary>Derecha.</summary>
    R,

    /// <summary>Permanecer (solo en la transición hacia qf, DA-07).</summary>
    S,
}

/// <summary>Fases del algoritmo de suma (Etapa 3 §1).</summary>
public enum Phase
{
    /// <summary>Validación y delimitadores (q0–q3).</summary>
    A,

    /// <summary>Suma por columnas con acarreo (q4–q25).</summary>
    B,

    /// <summary>Normalización del resultado (q26).</summary>
    C,

    /// <summary>Traslado del resultado detrás del = (q27–q34).</summary>
    D,

    /// <summary>Restauración de marcas (q35).</summary>
    E,
}

/// <summary>
/// Una transición δ(q, a) = (p, b, D), escrita en la notación del curso como (q, a = p, b, D).
/// </summary>
/// <param name="From">Estado actual q.</param>
/// <param name="Read">Símbolo leído a.</param>
/// <param name="To">Estado siguiente p.</param>
/// <param name="Write">Símbolo escrito b.</param>
/// <param name="Move">Movimiento D.</param>
public sealed record Transition(string From, char Read, string To, char Write, Move Move)
{
    /// <inheritdoc />
    public override string ToString() => $"({From}, {Read} = {To}, {Write}, {Move})";

    /// <summary>Rótulo de la flecha en el diagrama: lee, escribe, dirección.</summary>
    public string ArrowLabel => $"{Read},{Write},{Move}";
}
