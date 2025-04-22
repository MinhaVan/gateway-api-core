using Yarp.ReverseProxy;

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

app.Use(async (context, next) =>
{
    var traceId = context.TraceIdentifier;
    Console.WriteLine($"[TRACE] {traceId} {context.Request.Method} {context.Request.Path}");
    await next();
});

app.MapReverseProxy();

app.Run();
