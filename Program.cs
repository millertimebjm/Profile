// using Companion.Services.Configuration;
using Serilog;
using Serilog.Sinks.Grafana.Loki;
using System;
using System.Text.Json;

public record Image(string filename, string alt);

class Program
{
    static void Main(string[] args)
    {

        var builder = WebApplication.CreateBuilder(args);

        const string profileAppConfigConnectionStringPath = "Values:Profile:AppConfigConnection";

        builder.Configuration.AddJsonFile("local.settings.json", optional: true, reloadOnChange: true);
        var connectionString = builder.Configuration[profileAppConfigConnectionStringPath];
        ArgumentException.ThrowIfNullOrWhiteSpace(connectionString);
        if (!string.IsNullOrEmpty(connectionString))
        {
            builder.Configuration.AddAzureAppConfiguration(connectionString);
        }

        

        builder.Host.UseSerilog();
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .WriteTo.Console()
            .WriteTo.GrafanaLoki("http://media.bltmiller.com:3100", 
                labels: new[]
                {
                    new LokiLabel() {Key = "app", Value = "profile"},
                    new LokiLabel() {Key = "env", Value = "prod"},
                })
            .CreateLogger();

        var app = builder.Build();

        app.MapGet("/photos.json", (IConfiguration config, ILogger<Program> logger) =>
        {
            var files = Directory.GetFiles("wwwroot/photos/");
            var options = new JsonSerializerOptions { WriteIndented = true };
            var images = files.Select(f => new Image(f.Replace("wwwroot/", ""), "Image"));
            string jsonString = JsonSerializer.Serialize(images, options);
            return Results.Content(jsonString, "application/json");
        });

        app.UseDefaultFiles();
        app.UseStaticFiles();

        app.UseSerilogRequestLogging();

        try
        {
            app.Run();
        }
        finally
        {
            Log.CloseAndFlush();
        }
    }
}
