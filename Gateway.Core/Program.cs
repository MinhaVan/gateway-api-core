using Ocelot.DependencyInjection;
using Ocelot.Middleware;

var builder = WebApplication.CreateBuilder(args);
var env = Environment.GetEnvironmentVariable("OCEL0T_ENV") ?? "local";

Console.WriteLine($"Environment: {env}");
Console.WriteLine($"Base Path: {Directory.GetCurrentDirectory()}");

builder.Configuration
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", true, true)
    .AddJsonFile($"ocelot.{env}.json")
    .AddEnvironmentVariables();

builder.Services.AddOcelot();

var app = builder.Build();
await app.UseOcelot();

app.Run();
