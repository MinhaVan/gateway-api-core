using System.Text;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);
var env = Environment.GetEnvironmentVariable("ENV") ?? "local";

Console.WriteLine($"Environment: {env}");
Console.WriteLine($"Base Path: {Directory.GetCurrentDirectory()}");

builder.Configuration
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile($"appsettings.{env}.json", false, true)
    .AddEnvironmentVariables();

builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

var app = builder.Build();

app.UseWebSockets();

// Middleware para logar tudo antes de enviar para o destino
app.Use(async (context, next) =>
{
    var traceId = context.TraceIdentifier;
    Console.WriteLine($"[TRACE] {traceId} {context.Request.Method} {context.Request.Path}");

    // Logando headers
    foreach (var header in context.Request.Headers)
    {
        Console.WriteLine($"[HEADER] {header.Key}: {header.Value}");
    }

    // Logando body (se existir e se for possível ler)
    if (context.Request.ContentLength > 0 &&
        (context.Request.Method == "POST" || context.Request.Method == "PUT" || context.Request.Method == "PATCH"))
    {
        context.Request.EnableBuffering(); // Permite ler o body sem consumi-lo

        using var reader = new StreamReader(context.Request.Body, Encoding.UTF8, leaveOpen: true);
        var body = await reader.ReadToEndAsync();
        context.Request.Body.Position = 0; // Resetar a posição para o ReverseProxy poder ler também

        Console.WriteLine($"[BODY] {body}");
    }

    await next();
});

app.MapReverseProxy();

app.Run();
