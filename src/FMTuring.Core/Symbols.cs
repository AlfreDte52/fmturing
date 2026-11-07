namespace FMTuring.Core;

/// <summary>
/// Símbolos del alfabeto de cinta Γ = {0, 1, B, +, =, X, Y} (Etapa 2 §1 y Etapa 3 §2).
/// </summary>
public static class Symbols
{
    /// <summary>Bit 0.</summary>
    public const char Zero = '0';

    /// <summary>Bit 1.</summary>
    public const char One = '1';

    /// <summary>Símbolo blanco B. También separa los operandos en la entrada (DA-01).</summary>
    public const char Blank = 'B';

    /// <summary>Signo de suma; reemplaza al separador al validar la entrada.</summary>
    public const char Plus = '+';

    /// <summary>Signo igual; el resultado z se escribe a su derecha.</summary>
    public const char EqualsSign = '=';

    /// <summary>Marca de un bit 0 ya sumado.</summary>
    public const char MarkedZero = 'X';

    /// <summary>Marca de un bit 1 ya sumado.</summary>
    public const char MarkedOne = 'Y';

    /// <summary>Marca del separador en la variante por conteo (CountingAdditionMachine).</summary>
    public const char Hash = '#';

    /// <summary>Alfabeto de entrada Σ = {0, 1}.</summary>
    public static IReadOnlyList<char> InputAlphabet { get; } = [Zero, One];

    /// <summary>Alfabeto de cinta Γ = {0, 1, B, +, =, X, Y}, en el orden de las columnas de la tabla δ.</summary>
    public static IReadOnlyList<char> TapeAlphabet { get; } = [Zero, One, Blank, Plus, EqualsSign, MarkedZero, MarkedOne];

    /// <summary>Indica si el carácter es un bit binario (0 o 1).</summary>
    public static bool IsBit(char c) => c is Zero or One;
}
