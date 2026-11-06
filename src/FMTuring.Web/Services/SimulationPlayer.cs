using FMTuring.Core;

namespace FMTuring.Web.Services;

/// <summary>Velocidades de reproducción (Etapa 5 §6).</summary>
public enum PlaybackSpeed
{
    Lenta,
    Normal,
    Rapida,
    Inmediata,
}

/// <summary>Los tres tiempos de un paso: lectura, escritura y movimiento.</summary>
public enum StepBeat
{
    Lectura,
    Escritura,
    Movimiento,
}

/// <summary>Lo que la interfaz dibuja en un instante: cinta, estado y regla resaltada.</summary>
/// <param name="Index">Paso confirmado de la traza.</param>
/// <param name="Beat">Tiempo del paso que se está mostrando.</param>
/// <param name="State">Estado que se muestra como activo.</param>
/// <param name="Cells">Celdas no blancas por posición absoluta.</param>
/// <param name="Head">Posición del cabezal.</param>
/// <param name="Transition">Transición resaltada: la que se va a aplicar (lectura, escritura) o la aplicada.</param>
/// <param name="WrittenPosition">Celda recién escrita, si la hay.</param>
/// <param name="Phase">Fase de la transición resaltada.</param>
/// <param name="Explanation">Explicación del paso.</param>
public sealed record TapeFrame(
    int Index,
    StepBeat Beat,
    string State,
    IReadOnlyDictionary<long, char> Cells,
    long Head,
    Transition? Transition,
    long? WrittenPosition,
    Phase? Phase,
    string Explanation);

/// <summary>
/// Reproduce la traza que calcula el motor. Nunca calcula la suma: solo mueve un índice
/// sobre los pasos ya calculados (DA-24), así que retroceder o saltar es inmediato.
/// </summary>
public sealed class SimulationPlayer : IDisposable
{
    private readonly Simulator _simulator;
    private CancellationTokenSource? _playCts;

    public SimulationPlayer(Simulator simulator) => _simulator = simulator;

    /// <summary>Se dispara cada vez que cambia lo que hay que dibujar.</summary>
    public event Action? Changed;

    public string X { get; private set; } = string.Empty;

    public string Y { get; private set; } = string.Empty;

    public IReadOnlyList<string> Errors { get; private set; } = [];

    public ExecutionTrace? Trace { get; private set; }

    public int Index { get; private set; }

    public StepBeat Beat { get; private set; } = StepBeat.Movimiento;

    public bool IsPlaying { get; private set; }

    public bool IsAnimating { get; private set; }

    public PlaybackSpeed Speed { get; set; } = PlaybackSpeed.Normal;

    /// <summary>Primera posición que dibuja la cinta en toda la ejecución.</summary>
    public long MinPosition { get; private set; }

    /// <summary>Última posición que dibuja la cinta en toda la ejecución.</summary>
    public long MaxPosition { get; private set; }

    public bool AtStart => Index == 0 && Beat == StepBeat.Movimiento;

    public bool AtEnd => Trace is null || Index >= Trace.StepCount;

    /// <summary>Duración total de un paso para cada velocidad.</summary>
    public static int DurationMs(PlaybackSpeed speed) => speed switch
    {
        PlaybackSpeed.Lenta => 1200,
        PlaybackSpeed.Normal => 500,
        PlaybackSpeed.Rapida => 90,
        _ => 0,
    };

    /// <summary>Valida los operandos y ejecuta la máquina. Devuelve false si la entrada no es válida.</summary>
    public bool Load(string? x, string? y)
    {
        Stop();
        X = x ?? string.Empty;
        Y = y ?? string.Empty;
        var validation = InputValidator.Validate(X, Y);
        Errors = validation.Errors;
        if (!validation.IsValid)
        {
            Notify();
            return false;
        }

        Trace = _simulator.Run(X, Y);
        Index = 0;
        Beat = StepBeat.Movimiento;
        ComputeBounds(Trace);
        Notify();
        return true;
    }

    /// <summary>Lo que se dibuja ahora.</summary>
    public TapeFrame? Frame
    {
        get
        {
            if (Trace is null)
            {
                return null;
            }

            var step = Trace.Steps[Index];
            if (Beat != StepBeat.Movimiento && Index < Trace.StepCount)
            {
                var next = Trace.Steps[Index + 1];
                var t = next.Transition!;
                var cells = CellsOf(step);
                var state = step.Description.State;
                long? written = null;
                if (Beat == StepBeat.Escritura)
                {
                    SetCell(cells, step.HeadPosition, t.Write);
                    state = t.To;
                    written = step.HeadPosition;
                }

                return new TapeFrame(Index, Beat, state, cells, step.HeadPosition, t, written, next.Phase, next.Explanation);
            }

            long? previousHead = Index > 0 ? Trace.Steps[Index - 1].HeadPosition : null;
            return new TapeFrame(
                Index,
                StepBeat.Movimiento,
                step.Description.State,
                CellsOf(step),
                step.HeadPosition,
                step.Transition,
                previousHead,
                step.Phase,
                step.Explanation);
        }
    }

    /// <summary>Avanza un paso, animando sus tres tiempos según la velocidad.</summary>
    public async Task StepForwardAsync(CancellationToken cancellationToken = default)
    {
        if (Trace is null || AtEnd)
        {
            return;
        }

        var duration = DurationMs(Speed == PlaybackSpeed.Inmediata ? PlaybackSpeed.Normal : Speed);
        try
        {
            if (duration >= 300)
            {
                IsAnimating = true;
                Beat = StepBeat.Lectura;
                Notify();
                await Task.Delay((int)(duration * 0.35), cancellationToken);

                Beat = StepBeat.Escritura;
                Notify();
                await Task.Delay((int)(duration * 0.30), cancellationToken);

                Index++;
                Beat = StepBeat.Movimiento;
                Notify();
                await Task.Delay((int)(duration * 0.35), cancellationToken);
            }
            else
            {
                Index++;
                Beat = StepBeat.Movimiento;
                Notify();
                await Task.Delay(duration, cancellationToken);
            }
        }
        catch (OperationCanceledException)
        {
            Beat = StepBeat.Movimiento;
            Notify();
        }
        finally
        {
            IsAnimating = false;
        }
    }

    /// <summary>Botón Paso: avanza uno, si no hay otra animación en curso.</summary>
    public async Task StepAsync()
    {
        if (IsAnimating)
        {
            return;
        }

        Stop();
        await StepForwardAsync();
    }

    public void StepBack()
    {
        Stop();
        if (Index > 0)
        {
            Index--;
        }

        Beat = StepBeat.Movimiento;
        Notify();
    }

    public void GoTo(int index)
    {
        if (Trace is null)
        {
            return;
        }

        Stop();
        Index = Math.Clamp(index, 0, Trace.StepCount);
        Beat = StepBeat.Movimiento;
        Notify();
    }

    public void Reset() => GoTo(0);

    public void TogglePlay()
    {
        if (IsPlaying)
        {
            Stop();
            Notify();
        }
        else
        {
            _ = PlayAsync();
        }
    }

    public async Task PlayAsync()
    {
        if (Trace is null)
        {
            return;
        }

        if (Speed == PlaybackSpeed.Inmediata)
        {
            GoTo(Trace.StepCount);
            return;
        }

        Stop();
        if (AtEnd)
        {
            Index = 0;
            Beat = StepBeat.Movimiento;
        }

        var cts = new CancellationTokenSource();
        _playCts = cts;
        IsPlaying = true;
        Notify();
        try
        {
            while (!AtEnd && !cts.IsCancellationRequested)
            {
                await StepForwardAsync(cts.Token);
            }
        }
        finally
        {
            if (ReferenceEquals(_playCts, cts))
            {
                _playCts = null;
                IsPlaying = false;
                Notify();
            }

            cts.Dispose();
        }
    }

    public void Stop()
    {
        _playCts?.Cancel();
        _playCts = null;
        IsPlaying = false;
    }

    public void Dispose() => Stop();

    private static Dictionary<long, char> CellsOf(ExecutionStep step)
    {
        var cells = new Dictionary<long, char>();
        var position = step.LeftStart;
        foreach (var c in step.Description.Left)
        {
            SetCell(cells, position++, c);
        }

        SetCell(cells, step.HeadPosition, step.Description.Symbol);
        position = step.HeadPosition + 1;
        foreach (var c in step.Description.Right)
        {
            SetCell(cells, position++, c);
        }

        return cells;
    }

    private static void SetCell(Dictionary<long, char> cells, long position, char symbol)
    {
        if (symbol == Symbols.Blank)
        {
            cells.Remove(position);
        }
        else
        {
            cells[position] = symbol;
        }
    }

    private void ComputeBounds(ExecutionTrace trace)
    {
        long min = 0;
        long max = 0;
        foreach (var step in trace.Steps)
        {
            min = Math.Min(min, Math.Min(step.LeftStart, step.HeadPosition));
            max = Math.Max(max, step.HeadPosition + step.Description.Right.Length);
        }

        MinPosition = min - 10;
        MaxPosition = max + 10;
    }

    private void Notify() => Changed?.Invoke();
}
