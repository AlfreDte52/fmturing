# FMTURING

Simulación web de una máquina de Turing que suma dos números binarios separados por el símbolo blanco B y deja en la cinta la operación completa, por ejemplo `101 + 11 = 1000`.

Proyecto final de Autómatas y Lenguajes Formales. Sitio publicado: https://alfredte52.github.io/fmturing/

## La máquina

- M = (Q, Σ, Γ, δ, q0, B, F), determinista, de una cinta infinita en ambos sentidos.
- Σ = {0, 1} · Γ = {0, 1, B, +, =, X, Y} · F = {qf}.
- 37 estados (q0 a q35 y qf) y 141 transiciones; suma por columnas con acarreo en cinco fases: validación, suma, normalización, traslado del resultado y restauración.
- Cadena mínima `1B1`: 54 pasos. Cadena promedio `101B11`: 179 pasos.

## Introducción: variante por conteo

La página **Introducción** muestra primero una máquina de 11 estados y 29 transiciones que suma contando: resta 1 a N2 y suma 1 a N1 hasta que N2 vale 0, y deja en la cinta solo el resultado. Tiene su propia animación, diagrama, tabla δ, grados y descripciones instantáneas de `1B1` (19 pasos) y `11B10` (36 pasos). Termina con la comparación que justifica la máquina completa: con 16 unos + 16 unos, la variante necesita 2.490.416 pasos y la suma por columnas 3.264. La variante admite hasta 6 bits por operando.

## Estructura

| Proyecto | Contenido |
| --- | --- |
| `src/FMTuring.Core` | Motor: definición formal, cinta, descripciones instantáneas y simulador. `BinaryAdditionMachine` es la máquina del proyecto y `CountingAdditionMachine` la variante de la introducción. |
| `src/FMTuring.Web` | Interfaz Blazor WebAssembly: simulador animado, diagrama del autómata, descripción formal, tabla de transiciones y evaluación. |
| `tests/FMTuring.Core.Tests` | Pruebas xUnit (100): las 3.844 sumas de 1 a 5 bits en ambas máquinas, los pasos de referencia, las descripciones instantáneas de la evaluación formal y de la variante, y los rechazos. |

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
