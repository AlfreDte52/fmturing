# FMTURING

Simulación web de una máquina de Turing que suma dos números binarios separados por el símbolo blanco B y deja en la cinta la operación completa, por ejemplo `101 + 11 = 1000`.

Proyecto final de Autómatas y Lenguajes Formales. Sitio publicado: https://alfredte52.github.io/fmturing/

## La máquina

- M = (Q, Σ, Γ, δ, q0, B, F), determinista, de una cinta infinita en ambos sentidos.
- Σ = {0, 1} · Γ = {0, 1, B, +, =, X, Y} · F = {qf}.
- 37 estados (q0 a q35 y qf) y 141 transiciones; suma por columnas con acarreo en cinco fases: validación, suma, normalización, traslado del resultado y restauración.
- Cadena mínima `1B1`: 54 pasos. Cadena promedio `101B11`: 179 pasos.

## Estructura

| Proyecto | Contenido |
| --- | --- |
| `src/FMTuring.Core` | Motor: definición formal, cinta, descripciones instantáneas y simulador. Es la única fuente de verdad de δ. |
| `src/FMTuring.Web` | Interfaz Blazor WebAssembly: simulador animado, diagrama del autómata, descripción formal, tabla de transiciones y evaluación. |
| `tests/FMTuring.Core.Tests` | Pruebas xUnit: las 3.844 sumas de 1 a 5 bits, los pasos de referencia, las 233 descripciones instantáneas de la evaluación formal y los rechazos. |

La interfaz no calcula la suma: reproduce la traza que genera el motor, así que la animación no puede diferir de la máquina formal.

## Requisitos

- SDK de .NET 10.

## Uso local

```bash
dotnet test
dotnet run --project src/FMTuring.Web
```

Luego abre la dirección que indica la consola (por ejemplo `http://localhost:5013`).

## Publicación

Cada push a `main` ejecuta `.github/workflows/deploy.yml`: corre las pruebas, publica el sitio y lo despliega en GitHub Pages. Si una prueba falla, no se publica.

Configuración única en GitHub: **Settings → Pages → Build and deployment → Source: GitHub Actions**.

## Video

El enlace del video se configura en `src/FMTuring.Web/SiteConfig.cs` (`VideoEmbedUrl`), con el formato de inserción de YouTube: `https://www.youtube-nocookie.com/embed/ID_DEL_VIDEO`.
