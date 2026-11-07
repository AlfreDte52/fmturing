using FMTuring.Core;

namespace FMTuring.Web;

/// <summary>Datos del sitio que se configuran al final del proyecto.</summary>
public static class SiteConfig
{
    /// <summary>
    /// Enlace de inserción del video de YouTube (no listado), por ejemplo
    /// https://www.youtube-nocookie.com/embed/ID_DEL_VIDEO. Vacío muestra un espacio reservado.
    /// </summary>
    public const string VideoEmbedUrl = "https://www.youtube-nocookie.com/embed/-F4bYEG7ok8";

    /// <summary>Identificador del documento PDF del proyecto en Google Drive.</summary>
    public const string DocumentId = "1u1enyZsZn5_ZtV3q-exPdgDgi4OROQiR";

    /// <summary>Abre el PDF en el visor de Google Drive.</summary>
    public const string DocumentViewUrl = "https://drive.google.com/file/d/" + DocumentId + "/view";

    /// <summary>Vista previa del PDF para incrustar en la página.</summary>
    public const string DocumentPreviewUrl = "https://drive.google.com/file/d/" + DocumentId + "/preview";

    /// <summary>Descarga directa del PDF.</summary>
    public const string DocumentDownloadUrl = "https://drive.google.com/uc?export=download&id=" + DocumentId;

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

/// <summary>Ejemplo precargado del panel de entrada.</summary>
/// <param name="X">Primer operando.</param>
/// <param name="Y">Segundo operando.</param>
/// <param name="Label">Texto del botón.</param>
/// <param name="Title">Descripción al pasar el cursor.</param>
/// <param name="Rejects">Si la máquina rechaza la cadena.</param>
public sealed record InputExample(string X, string Y, string Label, string Title, bool Rejects = false);

/// <summary>Nombres de las fases de la variante por conteo.</summary>
public static class CountingPhaseInfo
{
    public static string Name(Phase? phase) => phase switch
    {
        Phase.A => "Fase A · Validación",
        Phase.B => "Fase B · Ciclo: resta 1 a N2, suma 1 a N1",
        Phase.C => "Fase C · Limpieza",
        _ => "Configuración inicial",
    };
}
