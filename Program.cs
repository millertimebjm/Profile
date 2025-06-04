// using Companion.Services.Configuration;
// using Serilog;
using System;
using System.Text.Json;

public record Image(string filename, string alt);

class Program
{
    static void Main(string[] args)
    {
        //var profileAppConfigConnectionStringPath = "Values:AppConfigConnectionString";

        var builder = WebApplication.CreateBuilder(args);

        // builder.Configuration.AddJsonFile("local.settings.json", optional: true, reloadOnChange: true);
        // var connectionString = builder.Configuration[profileAppConfigConnectionStringPath];
        // ArgumentException.ThrowIfNullOrWhiteSpace(connectionString);
        // if (!string.IsNullOrEmpty(connectionString))
        // {
        //     builder.Configuration.AddAzureAppConfiguration(connectionString);
        // }

        // Log.Logger = new LoggerConfiguration()
        //     .ReadFrom.Configuration(builder.Configuration)
        //     .Enrich.FromLogContext()
        //     .Enrich.WithProperty("Application", "Profile")
        //     .Enrich.WithMachineName()
        //     .Enrich.WithThreadId()
        //     .WriteTo.Console() // Default to console logging
        //     .CreateLogger();
        // builder.Host.UseSerilog();

        var app = builder.Build();

        app.MapGet("/photos.json", (IConfiguration config) =>
        {
            var files = Directory.GetFiles("wwwroot/photos/");
            var options = new JsonSerializerOptions { WriteIndented = true };
            var images = files.Select(f => new Image(f.Replace("wwwroot/", ""), "Image"));
            string jsonString = JsonSerializer.Serialize(images, options);
            return Results.Content(jsonString, "application/json");
        });

        app.UseDefaultFiles();
        app.UseStaticFiles();

        app.Run();
    }
}
