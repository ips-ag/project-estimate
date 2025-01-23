using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ProjectEstimate;
using ProjectEstimate.Configuration;
using Serilog;
using Serilog.Exceptions;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .WriteTo.Console()
    .Enrich.FromLogContext()
    .Enrich.WithExceptionDetails()
    .CreateBootstrapLogger();
try
{
    var host = Host.CreateDefaultBuilder(args)
        .ConfigureAppConfiguration(builder => builder.AddJsonFile("secrets.json", optional: true, reloadOnChange: true))
        .ConfigureLogging(
            (ctx, builder) =>
            {
                builder.ClearProviders();
                var logger = new LoggerConfiguration()
                    .ReadFrom.Configuration(ctx.Configuration)
                    .WriteTo.Console()
                    .Enrich.FromLogContext()
                    .Enrich.WithExceptionDetails()
                    .CreateLogger();
                builder.AddSerilog(logger);
            })
        .ConfigureServices(
            services =>
            {
                services.AddHostedService<ChatService>();
                services.AddOptions<AzureOpenAiSettings>().BindConfiguration("Azure:OpenAi");
            });
    await host.RunConsoleAsync();
}
catch (Exception e)
{
    Log.Fatal(e, "Fatal error occurred");
}
