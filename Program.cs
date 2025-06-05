// using Companion.Services.Configuration;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
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

        app.MapGet("/photos.json", async (IConfiguration config, ILogger<Program> logger) =>
        {
            // Replace with your container or folder SAS URL
            string sasUrl = config["Profile:PhotoStorage"] ?? "";
            ArgumentException.ThrowIfNullOrWhiteSpace(sasUrl);

            // Create a BlobContainerClient using the SAS URL
            BlobContainerClient containerClient = new BlobContainerClient(new Uri(sasUrl));

            List<string> files = new();
            // List blobs with optional prefix (i.e., virtual directory)
            await foreach (BlobItem blobItem in containerClient.GetBlobsAsync(prefix: ""))
            {
                string blobName = blobItem.Name;
                string publicUrl = $"{containerClient.Uri.AbsoluteUri}{Uri.EscapeDataString(blobName)}";
                files.Add(publicUrl);
            }

            var options = new JsonSerializerOptions { WriteIndented = true };
            string jsonString = JsonSerializer.Serialize(files, options);
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
