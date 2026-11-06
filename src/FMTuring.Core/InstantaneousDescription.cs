namespace FMTuring.Core;

/// <summary>
/// Descripción instantánea (q, α, a, β): estado, cinta a la izquierda del cabezal,
/// símbolo bajo el cabezal y cinta a la derecha (DA-18). Equivale a α q aβ.
/// </summary>
public sealed record InstantaneousDescription(string State, string Left, char Symbol, string Right)
{
    /// <summary>Cadena vacía λ.</summary>
    public const string Lambda = "λ";

    /// <inheritdoc />
    public override string ToString() =>
        $"({State}, {(Left.Length == 0 ? Lambda : Left)}, {Symbol}, {(Right.Length == 0 ? Lambda : Right)})";
}
