using System.Globalization;
using FMTuring.Core;

namespace FMTuring.Web.Diagram;

/// <summary>Un estado dibujado en el diagrama.</summary>
public sealed record DiagramNode(string State, double X, double Y, bool IsFinal);

/// <summary>Una línea del rótulo de una flecha: una transición (lee, escribe, dirección).</summary>
public sealed record DiagramLabelLine(string Text, Transition Transition);

/// <summary>Una flecha entre dos estados, con todas sus transiciones apiladas en el rótulo.</summary>
public sealed record DiagramEdge(
    string From,
    string To,
    string Path,
    IReadOnlyList<DiagramLabelLine> Lines,
    double LabelX,
    double FirstLineY,
    string Anchor);

/// <summary>Banda de fondo de una o varias fases.</summary>
public sealed record DiagramBand(
    IReadOnlyList<Phase> Phases,
    double X,
    double Y,
    double Width,
    double Height,
    string Label,
    double LabelX,
    double LabelY,
    string LabelAnchor);

/// <summary>Flecha con trazo fijo: su camino y la base de su rótulo.</summary>
public sealed record FixedEdge(string Path, double LabelX, double LabelBaseY);

/// <summary>
/// Disposición de un diagrama de transiciones (Etapa 5 §5). Las flechas y sus rótulos se
/// generan desde δ, así que el diagrama no puede diferir de la tabla. Hay una disposición
/// por máquina: <see cref="Full"/> (37 estados) y <see cref="Counting"/> (variante de 11).
/// </summary>
public sealed class DiagramLayout
{
    public const double Radius = 22;
    public const double LineHeight = 15;

    private readonly Dictionary<string, (double X, double Y)> _positions;
    private readonly Dictionary<string, double> _loopDirection;
    private readonly HashSet<(string, string)> _curvedBelow;
    private readonly Dictionary<(string, string), double> _labelAt;
    private readonly Dictionary<(string, string), FixedEdge> _fixed;

    private DiagramLayout(
        TuringMachineDefinition machine,
        double width,
        double height,
        Dictionary<string, (double X, double Y)> positions,
        Dictionary<string, double> loopDirection,
        HashSet<(string, string)> curvedBelow,
        Dictionary<(string, string), double> labelAt,
        Dictionary<(string, string), FixedEdge> fixedEdges,
        IReadOnlyList<DiagramBand> bands)
    {
        Width = width;
        Height = height;
        _positions = positions;
        _loopDirection = loopDirection;
        _curvedBelow = curvedBelow;
        _labelAt = labelAt;
        _fixed = fixedEdges;
        Bands = bands;
        StateCount = machine.States.Count;
        TransitionCount = machine.Transitions.Count;

        Nodes = machine.States
            .Select(s => new DiagramNode(s, positions[s].X, positions[s].Y, machine.FinalStates.Contains(s)))
            .ToList();

        Edges = machine.Transitions
            .GroupBy(t => (t.From, t.To))
            .Select(g => BuildEdge(g.Key.From, g.Key.To, g.ToList()))
            .ToList();

        var (x0, y0) = positions[machine.InitialState];
        StartArrow = $"M{F(x0 - 46)} {F(y0)} L{F(x0 - Radius - 3)} {F(y0)}";
    }

    /// <summary>La máquina de FMTURING: 37 estados y 141 transiciones.</summary>
    public static DiagramLayout Full { get; } = new(
        BinaryAdditionMachine.Definition,
        1480,
        1800,
        new()
        {
            ["q0"] = (60, 200), ["q1"] = (170, 200), ["q2"] = (280, 200), ["q3"] = (390, 200),
            ["q4"] = (510, 200), ["q5"] = (510, 350),
            ["q6"] = (650, 200), ["q7"] = (650, 350), ["q8"] = (650, 500),
            ["q9"] = (790, 200), ["q10"] = (790, 350), ["q11"] = (790, 500), ["q12"] = (790, 650), ["q13"] = (790, 800),
            ["q14"] = (930, 200), ["q15"] = (930, 350), ["q16"] = (930, 500), ["q17"] = (930, 650),
            ["q18"] = (1070, 200), ["q19"] = (1070, 350), ["q20"] = (1070, 500), ["q21"] = (1070, 650),
            ["q22"] = (1210, 200), ["q23"] = (1210, 350),
            ["q25"] = (650, 1100), ["q26"] = (790, 1100), ["q28"] = (990, 1100), ["q30"] = (1150, 1100), ["q32"] = (1310, 1200),
            ["q24"] = (650, 1300), ["q29"] = (990, 1300), ["q31"] = (1150, 1300),
            ["q27"] = (790, 1450), ["q34"] = (990, 1450), ["q33"] = (1310, 1450), ["q35"] = (1310, 1600), ["qf"] = (1150, 1600),
        },
        new() { ["q22"] = 0, ["q23"] = 0, ["q32"] = 0, ["q35"] = 270, ["q12"] = 180, ["q13"] = 180 },
        [("q9", "q18"), ("q10", "q19"), ("q11", "q20")],
        new()
        {
            [("q4", "q12")] = 0.75, [("q5", "q13")] = 0.75,
            [("q9", "q15")] = 0.25, [("q10", "q16")] = 0.25, [("q11", "q17")] = 0.25,
            [("q12", "q25")] = 0.55, [("q13", "q24")] = 0.45,
            [("q12", "q14")] = 0.15, [("q12", "q15")] = 0.15,
            [("q13", "q15")] = 0.15, [("q13", "q16")] = 0.15,
        },
        new()
        {
            [("q22", "q4")] = new("M1210 178 C1210 60 540 60 525.6 181.4", 860, 82),
            [("q23", "q5")] = new("M1210 372 V860 Q1210 880 1190 880 H530 Q510 880 510 860 V376", 860, 872),
        },
        [
            new([Phase.A], 20, 90, 410, 180, "A · Validación", 32, 258, "start"),
            new([Phase.B], 455, 40, 960, 870, "B · Suma por columnas", 1402, 62, "end"),
            new([Phase.C, Phase.D, Phase.E], 590, 935, 870, 840, "C · Normalización · D · Traslado · E · Restauración", 605, 1760, "start"),
        ]);

    /// <summary>Variante introductoria por conteo: 11 estados y 29 transiciones.</summary>
    public static DiagramLayout Counting { get; } = new(
        CountingAdditionMachine.Definition,
        1000,
        500,
        new()
        {
            ["q0"] = (70, 150), ["q1"] = (220, 150), ["q2"] = (370, 150), ["q3"] = (520, 150),
            ["q4"] = (690, 150), ["q9"] = (900, 150), ["q5"] = (690, 350), ["q8"] = (900, 350),
            ["q6"] = (520, 350), ["q7"] = (370, 350), ["q10"] = (220, 350),
        },
        new() { ["q5"] = 270, ["q8"] = 270, ["q6"] = 270, ["q7"] = 270 },
        [],
        new(),
        new(),
        [
            new([Phase.A], 20, 40, 545, 190, "A · Validación", 32, 218, "start"),
            new([Phase.B], 630, 40, 360, 445, "B · Ciclo: resta 1 a N2, suma 1 a N1", 978, 475, "end"),
            new([Phase.C], 150, 265, 420, 220, "C · Limpieza", 162, 475, "start"),
        ]);

    public double Width { get; }

    public double Height { get; }

    public int StateCount { get; }

    public int TransitionCount { get; }

    public IReadOnlyList<DiagramNode> Nodes { get; }

    public IReadOnlyList<DiagramEdge> Edges { get; }

    public IReadOnlyList<DiagramBand> Bands { get; }

    /// <summary>Flecha sin origen que marca el estado inicial.</summary>
    public string StartArrow { get; }

    public (double X, double Y) PositionOf(string state) => _positions[state];

    /// <summary>Número con punto decimal, para atributos SVG.</summary>
    public static string F(double value) => value.ToString("0.#", CultureInfo.InvariantCulture);

    private DiagramEdge BuildEdge(string from, string to, List<Transition> transitions)
    {
        var lines = transitions.Select(t => new DiagramLabelLine(t.ArrowLabel, t)).ToList();
        var n = lines.Count;
        var (x1, y1) = _positions[from];
        var (x2, y2) = _positions[to];
        const double r = Radius;

        double Bottom(double y) => y - (n - 1) * LineHeight;
        double Middle(double y) => y - (n - 1) * LineHeight / 2 + 4;

        if (from == to)
        {
            var direction = _loopDirection.TryGetValue(from, out var d) ? d : 90;
            var ang = Deg(direction);
            var a1 = ang + Deg(28);
            var a2 = ang - Deg(28);
            var sx = x1 + r * Math.Cos(a1);
            var sy = y1 - r * Math.Sin(a1);
            var ex = x1 + r * Math.Cos(a2);
            var ey = y1 - r * Math.Sin(a2);
            var c1x = x1 + 62 * Math.Cos(ang + Deg(40));
            var c1y = y1 - 62 * Math.Sin(ang + Deg(40));
            var c2x = x1 + 62 * Math.Cos(ang - Deg(40));
            var c2y = y1 - 62 * Math.Sin(ang - Deg(40));
            var path = $"M{F(sx)} {F(sy)} C{F(c1x)} {F(c1y)} {F(c2x)} {F(c2y)} {F(ex)} {F(ey)}";
            return direction switch
            {
                0 => new DiagramEdge(from, to, path, lines, x1 + 72, Middle(y1), "start"),
                180 => new DiagramEdge(from, to, path, lines, x1 - 72, Middle(y1), "end"),
                270 => new DiagramEdge(from, to, path, lines, x1, y1 + 84, "middle"),
                _ => new DiagramEdge(from, to, path, lines, x1, Bottom(y1 - 72), "middle"),
            };
        }

        if (_fixed.TryGetValue((from, to), out var fixedEdge))
        {
            return new DiagramEdge(from, to, fixedEdge.Path, lines, fixedEdge.LabelX, Bottom(fixedEdge.LabelBaseY), "middle");
        }

        if (_curvedBelow.Contains((from, to)))
        {
            var sx = x1 + r * Math.Cos(Deg(-50));
            var sy = y1 - r * Math.Sin(Deg(-50));
            var ex = x2 - (r + 3) * Math.Cos(Deg(50));
            var ey = y2 + (r + 3) * Math.Sin(Deg(50));
            var path = $"M{F(sx)} {F(sy)} C{F(x1 + 60)} {F(y1 + 70)} {F(x2 - 60)} {F(y2 + 70)} {F(ex)} {F(ey)}";
            return new DiagramEdge(from, to, path, lines, (x2 + x1 + 140) / 2, Bottom(y1 + 76), "middle");
        }

        var dx = x2 - x1;
        var dy = y2 - y1;
        var length = Math.Sqrt(dx * dx + dy * dy);
        var ux = dx / length;
        var uy = dy / length;
        var straight = $"M{F(x1 + ux * r)} {F(y1 + uy * r)} L{F(x2 - ux * (r + 3))} {F(y2 - uy * (r + 3))}";

        if (Math.Abs(dy) < 1)
        {
            return new DiagramEdge(from, to, straight, lines, x1 + dx * 0.5, Bottom(y1 - 8), "middle");
        }

        if (Math.Abs(dx) < 1)
        {
            return new DiagramEdge(from, to, straight, lines, x1 + 10, Middle(y1 + dy * 0.5 + 4), "start");
        }

        var t0 = _labelAt.TryGetValue((from, to), out var at) ? at : 0.38;
        var side = dx > 0 ? -1 : 1;
        var lx = x1 + dx * t0 + -uy * 10 * side;
        var ly = y1 + dy * t0 + ux * 10 * side;
        return new DiagramEdge(from, to, straight, lines, lx, Middle(ly), dx > 0 ? "start" : "end");
    }

    private static double Deg(double degrees) => degrees * Math.PI / 180;
}
