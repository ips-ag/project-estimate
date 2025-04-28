using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.Extensions.Options;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.AzureOpenAI;
using ProjectEstimate.Application.Extensions;
using ProjectEstimate.Extensions.ApplicationInsights;
using ProjectEstimate.Repositories.Agents.Analyst;
using ProjectEstimate.Repositories.Agents.Architect;
using ProjectEstimate.Repositories.Agents.Consultant;
using ProjectEstimate.Repositories.Agents.Developer;
using ProjectEstimate.Repositories.Configuration;
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
    builder.Services.AddOptions<AzureOpenAiSettings>()
        .BindConfiguration(AzureOpenAiSettings.SectionName)
        .ValidateDataAnnotations()
        .ValidateOnStart();
    builder.Services.AddSingleton<IChatCompletionService>(sp =>
    {
        var options = sp.GetRequiredService<IOptions<AzureOpenAiSettings>>().Value;
        var loggerFactory = sp.GetRequiredService<ILoggerFactory>();
        return new AzureOpenAIChatCompletionService(
            options.DeploymentName,
            options.Endpoint,
            options.ApiKey,
            loggerFactory: loggerFactory);
    });
    builder.Services.AddKeyedTransient<Kernel>("ConsultantAgent", (sp, _) => new Kernel(sp));
    builder.Services.AddScoped<ConsultantAgent>();
    builder.Services.AddKeyedTransient<Kernel>("AnalystAgent", (sp, _) => new Kernel(sp));
    builder.Services.AddScoped<AnalystAgent>();
    builder.Services.AddKeyedTransient<Kernel>("ArchitectAgent", (sp, _) => new Kernel(sp));
    builder.Services.AddScoped<ArchitectAgent>();
    builder.Services.AddKeyedTransient<Kernel>("DeveloperAgent", (sp, _) => new Kernel(sp));
    builder.Services.AddScoped<DeveloperAgent>();
    builder.Services.AddSingleton<IUserInteraction, SignalrUserInteraction>();

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
