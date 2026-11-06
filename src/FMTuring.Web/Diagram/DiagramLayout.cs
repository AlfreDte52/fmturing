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

/// <summary>
/// Disposición del diagrama de transiciones (Etapa 5 §5). Las flechas y sus rótulos se
/// generan desde δ, así que el diagrama no puede diferir de la tabla.
/// </summary>
public static class DiagramLayout
{
    public const double Width = 1480;
    public const double Height = 1800;
    public const double Radius = 22;
    public const double LineHeight = 15;

    private static readonly Dictionary<string, (double X, double Y)> Positions = new()
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
    };

    /// <summary>Dirección de cada bucle en grados (90 = arriba, 0 = derecha, 180 = izquierda, 270 = abajo).</summary>
    private static readonly Dictionary<string, double> LoopDirection = new()
    {
        ["q22"] = 0, ["q23"] = 0, ["q32"] = 0, ["q35"] = 270, ["q12"] = 180, ["q13"] = 180,
    };

    /// <summary>Flechas que saltan una columna y se curvan por debajo para no cruzar estados.</summary>
    private static readonly HashSet<(string, string)> CurvedBelow = [("q9", "q18"), ("q10", "q19"), ("q11", "q20")];

    /// <summary>Posición del rótulo a lo largo de flechas largas, para no encimarlo con otros.</summary>
    private static readonly Dictionary<(string, string), double> LabelAt = new()
    {
        [("q4", "q12")] = 0.75, [("q5", "q13")] = 0.75,
        [("q9", "q15")] = 0.25, [("q10", "q16")] = 0.25, [("q11", "q17")] = 0.25,
        [("q12", "q25")] = 0.55, [("q13", "q24")] = 0.45,
        [("q12", "q14")] = 0.15, [("q12", "q15")] = 0.15,
        [("q13", "q15")] = 0.15, [("q13", "q16")] = 0.15,
    };

    static DiagramLayout()
    {
        Nodes = BinaryAdditionMachine.States
            .Select(s => new DiagramNode(s, Positions[s].X, Positions[s].Y, s == BinaryAdditionMachine.FinalState))
            .ToList();

        Edges = BinaryAdditionMachine.Transitions
            .GroupBy(t => (t.From, t.To))
            .Select(g => BuildEdge(g.Key.From, g.Key.To, g.ToList()))
            .ToList();

        Bands =
        [
            new([Phase.A], 20, 90, 410, 180, "A · Validación", 32, 258, "start"),
            new([Phase.B], 455, 40, 960, 870, "B · Suma por columnas", 1402, 62, "end"),
            new([Phase.C, Phase.D, Phase.E], 590, 935, 870, 840, "C · Normalización · D · Traslado · E · Restauración", 605, 1760, "start"),
        ];
    }

    public static IReadOnlyList<DiagramNode> Nodes { get; }

    public static IReadOnlyList<DiagramEdge> Edges { get; }

    public static IReadOnlyList<DiagramBand> Bands { get; }

    public static (double X, double Y) PositionOf(string state) => Positions[state];

    /// <summary>Número con punto decimal, para atributos SVG.</summary>
    public static string F(double value) => value.ToString("0.#", CultureInfo.InvariantCulture);

    private static DiagramEdge BuildEdge(string from, string to, List<Transition> transitions)
    {
        var lines = transitions.Select(t => new DiagramLabelLine(t.ArrowLabel, t)).ToList();
        var n = lines.Count;
        var (x1, y1) = Positions[from];
        var (x2, y2) = Positions[to];
        const double r = Radius;

        double Bottom(double y) => y - (n - 1) * LineHeight;
        double Middle(double y) => y - (n - 1) * LineHeight / 2 + 4;

        if (from == to)
        {
            var direction = LoopDirection.TryGetValue(from, out var d) ? d : 90;
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

        if ((from, to) == ("q22", "q4"))
        {
            return new DiagramEdge(from, to, "M1210 178 C1210 60 540 60 525.6 181.4", lines, 860, Bottom(82), "middle");
        }

        if ((from, to) == ("q23", "q5"))
        {
            return new DiagramEdge(from, to, "M1210 372 V860 Q1210 880 1190 880 H530 Q510 880 510 860 V376", lines, 860, Bottom(872), "middle");
        }

        if (CurvedBelow.Contains((from, to)))
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

        var t0 = LabelAt.TryGetValue((from, to), out var at) ? at : 0.38;
        var side = dx > 0 ? -1 : 1;
        var lx = x1 + dx * t0 + -uy * 10 * side;
        var ly = y1 + dy * t0 + ux * 10 * side;
        return new DiagramEdge(from, to, straight, lines, lx, Middle(ly), dx > 0 ? "start" : "end");
    }

    private static double Deg(double degrees) => degrees * Math.PI / 180;
}
