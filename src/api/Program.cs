using Azure.Monitor.OpenTelemetry.AspNetCore;
using ProjectEstimate.Application.Extensions;
using ProjectEstimate.Extensions.Cors;
using ProjectEstimate.Extensions.Security;
using ProjectEstimate.Repositories.Extensions;
using ProjectEstimate.Repositories.Hubs;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();
try
{
    var builder = WebApplication.CreateBuilder(args);
    builder.Configuration.AddJsonFile("secrets.json", optional: true, reloadOnChange: true);
    // Serilog forwards events to registered providers (Azure Monitor); drop the defaults to avoid duplicate console output
    builder.Logging.ClearProviders();
    builder.Host.UseSerilog(
        (context, configuration) => configuration.ReadFrom.Configuration(context.Configuration),
        writeToProviders: true);
    builder.Services.AddControllers();
    builder.Services.AddProblemDetails();
    builder.Services.AddOpenApi();
    if (!string.IsNullOrEmpty(builder.Configuration["APPLICATIONINSIGHTS_CONNECTION_STRING"]))
    {
        builder.Services.AddOpenTelemetry().UseAzureMonitor();
    }
    builder.Services.AddSignalR();
    builder.Services.ConfigureAuthentication();
    builder.Services.ConfigureCors();
    builder.Services.AddApplication();
    builder.Services.AddRepositories();

    var app = builder.Build();
    if (app.Environment.IsDevelopment())
    {
        app.MapOpenApi();
    }
    app.UseCorsMiddleware();
    app.UseAuthentication();
    app.UseAuthorization();
    app.MapControllers();
    app.MapHub<ChatHub>("/api/hub");
    await app.RunAsync();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}
