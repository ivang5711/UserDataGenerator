using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using UserRegistry.Client.Models;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.Services.AddTransient<DataGenerator>();

await builder.Build().RunAsync();
