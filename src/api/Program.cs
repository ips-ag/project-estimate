using Microsoft.ApplicationInsights.Extensibility;
using ProjectEstimate.Application.Extensions;
using ProjectEstimate.Extensions.ApplicationInsights;
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
    builder.Host.UseSerilog((context, configuration) =>
        configuration.ReadFrom.Configuration(context.Configuration));
    builder.Services.AddControllers();
    builder.Services.AddOpenApi();
    builder.Services.AddApplicationInsightsTelemetry();
    builder.Services.AddSignalR();
    builder.Services.AddSingleton<ITelemetryInitializer, ExceptionSamplingRateTelemetryInitializer>();
    builder.Services.AddApplication();
    builder.Services.AddRepositories();

    var app = builder.Build();
    if (app.Environment.IsDevelopment())
    {
        app.MapOpenApi();
    }
    app.UseCors(cors => cors.AllowAnyMethod().AllowAnyHeader().SetIsOriginAllowed(_ => true).AllowCredentials());
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
