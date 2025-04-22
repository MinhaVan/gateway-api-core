using Ocelot.DependencyInjection;
using Ocelot.Middleware;

internal class Program
{
    private static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Configuration.AddJsonFile("ocelot.json", optional: false, reloadOnChange: true);

        builder.Logging.ClearProviders();
        builder.Logging.AddConsole();
        builder.Logging.SetMinimumLevel(LogLevel.Debug);
        builder.Services.AddOcelot();

        var app = builder.Build();

        await app.UseOcelot();

        app.Run();
    }
}