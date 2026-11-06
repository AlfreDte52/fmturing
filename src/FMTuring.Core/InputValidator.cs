namespace FMTuring.Core;

/// <summary>Resultado de validar los operandos.</summary>
public sealed record ValidationResult(bool IsValid, IReadOnlyList<string> Errors);

/// <summary>
/// Valida los operandos antes de cargarlos en la cinta (RF-12).
/// Solo impide lo que no cabe en el modelo de entrada: símbolos distintos de 0 y 1
/// y más de 16 bits. Un operando vacío se permite, porque lo rechaza la propia máquina.
/// </summary>
public static class InputValidator
{
    /// <summary>Longitud máxima de cada operando (DA-25).</summary>
    public const int MaxBits = 16;

    /// <summary>Valida los dos operandos.</summary>
    public static ValidationResult Validate(string? x, string? y)
    {
        var errors = new List<string>();
        Check(x ?? string.Empty, "primer", errors);
        Check(y ?? string.Empty, "segundo", errors);
        return new ValidationResult(errors.Count == 0, errors);
    }

    private static void Check(string operand, string name, List<string> errors)
    {
        if (operand.Any(c => !Symbols.IsBit(c)))
        {
            errors.Add($"El {name} operando solo puede contener 0 y 1.");
        }

        if (operand.Length > MaxBits)
        {
            errors.Add($"El {name} operando admite como máximo {MaxBits} bits.");
        }
    }
}
