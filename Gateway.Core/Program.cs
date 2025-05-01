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
#if !DEBUG
app.UsePathBase("/gateway");
#endif

app.Use(async (context, next) =>
{
    var traceId = context.TraceIdentifier;
    Console.WriteLine($"[TRACE] {traceId} {context.Request.Method} {context.Request.Path}");

    foreach (var header in context.Request.Headers)
        Console.WriteLine($"[HEADER] {header.Key}: {header.Value}");
    
    if (context.Request.ContentLength > 0 &&
        (context.Request.Method == "POST" || context.Request.Method == "PUT" || context.Request.Method == "PATCH"))
    {
        context.Request.EnableBuffering();

        using var reader = new StreamReader(context.Request.Body, Encoding.UTF8, leaveOpen: true);
        var body = await reader.ReadToEndAsync();
        context.Request.Body.Position = 0;
        Console.WriteLine($"[BODY] {body}");
    }

    await next();
});

app.MapReverseProxy();

app.Run();
