namespace FMTuring.Core;

/// <summary>
/// Cinta infinita en ambos sentidos. Solo guarda las celdas no blancas.
/// </summary>
public sealed class Tape
{
    private readonly Dictionary<long, char> _cells = new();

    /// <summary>Carga la cadena de entrada a partir de la posición 0, con el cabezal sobre su primer símbolo.</summary>
    public Tape(string input)
    {
        ArgumentNullException.ThrowIfNull(input);
        for (var i = 0; i < input.Length; i++)
        {
            if (input[i] != Symbols.Blank)
            {
                _cells[i] = input[i];
            }
        }

        Head = 0;
    }

    /// <summary>Posición absoluta del cabezal.</summary>
    public long Head { get; private set; }

    /// <summary>Lee el símbolo bajo el cabezal.</summary>
    public char Read() => ReadAt(Head);

    /// <summary>Lee el símbolo de una posición cualquiera.</summary>
    public char ReadAt(long position) => _cells.TryGetValue(position, out var c) ? c : Symbols.Blank;

    /// <summary>Escribe un símbolo bajo el cabezal.</summary>
    public void Write(char symbol)
    {
        if (symbol == Symbols.Blank)
        {
            _cells.Remove(Head);
        }
        else
        {
            _cells[Head] = symbol;
        }
    }

    /// <summary>Mueve el cabezal una celda (o lo deja en su sitio con S).</summary>
    public void MoveHead(Move move)
    {
        Head += move switch
        {
            Move.L => -1,
            Move.R => 1,
            _ => 0,
        };
    }

    /// <summary>
    /// Descripción instantánea (q, α, a, β): sin blancos en los extremos de α y β (Etapa 2 §5).
    /// </summary>
    public InstantaneousDescription Describe(string state)
    {
        var left = string.Empty;
        var right = string.Empty;
        if (_cells.Count > 0)
        {
            var min = Math.Min(_cells.Keys.Min(), Head);
            var max = Math.Max(_cells.Keys.Max(), Head);
            left = Span(min, Head - 1).TrimStart(Symbols.Blank);
            right = Span(Head + 1, max).TrimEnd(Symbols.Blank);
        }

        return new InstantaneousDescription(state, left, Read(), right);
    }

    /// <summary>Contenido no blanco de la cinta, del primer al último símbolo distinto de B.</summary>
    public string Content()
    {
        if (_cells.Count == 0)
        {
            return string.Empty;
        }

        return Span(_cells.Keys.Min(), _cells.Keys.Max());
    }

    private string Span(long from, long to)
    {
        if (to < from)
        {
            return string.Empty;
        }

        var chars = new char[to - from + 1];
        for (var p = from; p <= to; p++)
        {
            chars[p - from] = ReadAt(p);
        }

        return new string(chars);
    }
}
