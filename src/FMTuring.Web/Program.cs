using FMTuring.Core;
using FMTuring.Web;
using FMTuring.Web.Services;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// El motor es la única fuente de verdad: la interfaz solo reproduce su traza (DA-24).
builder.Services.AddSingleton(new Simulator());
builder.Services.AddScoped<SimulationPlayer>();

await builder.Build().RunAsync();
