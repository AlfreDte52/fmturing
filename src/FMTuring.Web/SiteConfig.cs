using FMTuring.Core;

namespace FMTuring.Web;

/// <summary>Datos del sitio que se configuran al final del proyecto.</summary>
public static class SiteConfig
{
    /// <summary>
    /// Enlace de inserción del video de YouTube (no listado), por ejemplo
    /// https://www.youtube-nocookie.com/embed/ID_DEL_VIDEO. Vacío muestra un espacio reservado.
    /// </summary>
    public const string VideoEmbedUrl = "";

    /// <summary>Repositorio del proyecto.</summary>
    public const string RepositoryUrl = "https://github.com/AlfreDte52/fmturing";
}

/// <summary>Nombres de las fases para la interfaz.</summary>
public static class PhaseInfo
{
    public static string Name(Phase? phase) => phase switch
    {
        Phase.A => "Fase A · Validación",
        Phase.B => "Fase B · Suma por columnas",
        Phase.C => "Fase C · Normalización",
        Phase.D => "Fase D · Traslado del resultado",
        Phase.E => "Fase E · Restauración",
        _ => "Configuración inicial",
    };

    public static string Short(Phase? phase) => phase switch
    {
        Phase.A => "A · Validación",
        Phase.B => "B · Suma",
        Phase.C => "C · Normalización",
        Phase.D => "D · Traslado",
        Phase.E => "E · Restauración",
        _ => "—",
    };
}
