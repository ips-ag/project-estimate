using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ProjectEstimate;
using ProjectEstimate.Configuration;
using Serilog;

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .WriteTo.Console()
    .CreateLogger();

// create .NET host using defaults
var host = Host.CreateDefaultBuilder(args)
    .ConfigureAppConfiguration(builder => builder.AddJsonFile("secrets.json", optional: false, reloadOnChange: true))
    .ConfigureLogging(
        builder =>
        {
            builder.ClearProviders();
            builder.AddSerilog();
        })
    .ConfigureServices(
        services =>
        {
            services.AddHostedService<ChatService>();
            services.AddOptions<AzureOpenAiSettings>().BindConfiguration("Azure:OpenAi");
        });

await host.RunConsoleAsync();
