using Microsoft.Extensions.Options;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.AzureOpenAI;
using ProjectEstimate.Repositories.Agents.Analyst;
using ProjectEstimate.Repositories.Agents.Architect;
using ProjectEstimate.Repositories.Agents.Consultant;
using ProjectEstimate.Repositories.Agents.Developer;
using ProjectEstimate.Repositories.Configuration;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();
try
{
    var builder = WebApplication.CreateBuilder(args);
    builder.Configuration.AddJsonFile("secrets.json", optional: true, reloadOnChange: true);
    builder.Host.UseSerilog();
    builder.Services.AddControllers();
    builder.Services.AddCors();
    builder.Services.AddOpenApi();
    builder.Services.AddApplicationInsightsTelemetry();
    builder.Services.AddOptions<AzureOpenAiSettings>()
        .BindConfiguration(AzureOpenAiSettings.SectionName)
        .ValidateDataAnnotations()
        .ValidateOnStart();
    builder.Services.AddSingleton<IChatCompletionService>(
        sp =>
        {
            var options = sp.GetRequiredService<IOptions<AzureOpenAiSettings>>().Value;
            return new AzureOpenAIChatCompletionService(
                options.DeploymentName,
                options.Endpoint,
                options.ApiKey);
        });
    builder.Services.AddKeyedTransient<Kernel>("ConsultantAgent", (sp, _) => new Kernel(sp));
    builder.Services.AddScoped<ConsultantAgent>();
    builder.Services.AddKeyedTransient<Kernel>("AnalystAgent", (sp, _) => new Kernel(sp));
    builder.Services.AddScoped<AnalystAgent>();
    builder.Services.AddKeyedTransient<Kernel>("ArchitectAgent", (sp, _) => new Kernel(sp));
    builder.Services.AddScoped<ArchitectAgent>();
    builder.Services.AddKeyedTransient<Kernel>("DeveloperAgent", (sp, _) => new Kernel(sp));
    builder.Services.AddScoped<DeveloperAgent>();

    var app = builder.Build();
    if (app.Environment.IsDevelopment())
    {
        app.MapOpenApi();
    }
    app.UseCors(cors => cors.AllowAnyMethod().AllowAnyHeader().AllowAnyOrigin());
    app.MapControllers();
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
