using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);
var env = Environment.GetEnvironmentVariable("ENV") ?? "local";

Console.WriteLine($"Environment: {env}");
Console.WriteLine($"Base Path: {Directory.GetCurrentDirectory()}");

builder.Configuration
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile($"appsettings.{env}.json", false, true)
    .AddEnvironmentVariables();


Console.WriteLine($"Configuration: {JsonSerializer.Serialize(builder.Configuration)}");

builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

var app = builder.Build();

app.UseWebSockets();

app.Use(async (context, next) =>
{
    var traceId = context.TraceIdentifier;
    Console.WriteLine($"[TRACE] {traceId} {context.Request.Method} {context.Request.Path}");
    await next();
});

app.MapReverseProxy();

app.Run();
