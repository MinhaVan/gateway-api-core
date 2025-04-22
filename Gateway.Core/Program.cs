using Ocelot.DependencyInjection;
using Ocelot.Middleware;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", true, true)
    .AddJsonFile($"ocelot.{Environment.GetEnvironmentVariable("OCEL0T_ENV") ?? "local"}.json")
    .AddEnvironmentVariables();

builder.Services.AddOcelot();

var app = builder.Build();
await app.UseOcelot();

app.Run();
