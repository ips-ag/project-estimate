using System.Text;
using Microsoft.SemanticKernel.ChatCompletion;
using ProjectEstimate.Domain;
using ProjectEstimate.Repositories.Agents.Analyst;
using ProjectEstimate.Repositories.Agents.Architect;
using ProjectEstimate.Repositories.Agents.Developer;
using ProjectEstimate.Repositories.Documents;
using ProjectEstimate.Repositories.Hubs;

// using ClosedXML.Excel;
// using Microsoft.Extensions.DependencyInjection;
// using Microsoft.SemanticKernel;
// using Microsoft.SemanticKernel.Connectors.AzureOpenAI;

namespace ProjectEstimate.Repositories.Agents.Consultant;

internal class ConsultantAgent
{
    private const string RoleName = "Consultant";
    private readonly AnalystAgent _analystAgent;
    private readonly ArchitectAgent _architectAgent;
    private readonly DeveloperAgent _developerAgent;
    private readonly IUserInteraction _userInteraction;
    private readonly IDocumentRepository _documentRepository;

    // private readonly Kernel _kernel;
    // private readonly IChatCompletionService _chatCompletion;
    // private readonly PromptExecutionSettings _executionSettings;

    // TODO: get history from repository
    private readonly ChatHistory _history = [];

    public ConsultantAgent(
        // [FromKeyedServices("ConsultantAgent")] Kernel kernel,
        // IChatCompletionService chatCompletion,
        AnalystAgent analystAgent,
        ArchitectAgent architectAgent,
        DeveloperAgent developerAgent,
        IUserInteraction userInteraction,
        IDocumentRepository documentRepository)
    {
        // _kernel = kernel;
        // _chatCompletion = chatCompletion;
        // _executionSettings = new AzureOpenAIPromptExecutionSettings
        // {
        //     FunctionChoiceBehavior = FunctionChoiceBehavior.Auto(),
        //     ChatSystemPrompt =
        //         """
        //         Assistant is a software development project estimator. It helps estimating the time and cost of software development projects.
        //         Input consists of all gathered requirements for a software project. They can be functional or non-functional requirements.
        //         Estimates are provided based on the project requirements and input from the architecture team.
        //         Estimates are provided for each functional requirement in man-days. Can be fractional, e.g. 0.25, 0.5, 1.25, etc.
        //         Provide breakdown and explanation of the estimates for each functional requirement.
        //         Do not answer questions that are not related to software development project estimation.
        //         """
        // };
        _analystAgent = analystAgent;
        _architectAgent = architectAgent;
        _developerAgent = developerAgent;
        _userInteraction = userInteraction;
        _documentRepository = documentRepository;
    }

    /// <summary>
    ///     Reads user input and writes agent output.
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    public async ValueTask<string?> ExecuteAsync(ChatCompletionRequest request, CancellationToken cancellationToken)
    {
        string? userInput = request.Prompt;
        string? fileInput = await _documentRepository.ReadDocumentAsync(request.FileLocation, cancellationToken);

        // using var workbook = new XLWorkbook();
        // var inputWorksheet = workbook.Worksheets.Add("Input");
        // inputWorksheet.FirstRow().Style.Font.Bold = true;
        // inputWorksheet.Cell("A1").Value = "User Input";
        // inputWorksheet.Cell("A2").Value = userInput;
        // inputWorksheet.Columns().AdjustToContents();

        // Add user input
        var userMessage =
            $"""
             User prompt:
             \"\"\"{userInput}\"\"\"
             Additional context:
             \"\"\"{fileInput}\"\"\"
             """;
        _history.AddUserMessage(userMessage);

        // consult analyst
        foreach (var message in _history)
        {
            if (message.Content is null || message.Role == AuthorRole.System) continue;
            await _userInteraction.WriteAssistantMessageAsync(
                assistant: RoleName,
                message: $"➜ **Analyst** Sending message *{message.Content}*",
                logLevel: LogLevel.Debug,
                cancel: cancellationToken);
        }
        var verifications = await _analystAgent.VerifyRequirementsAsync(_history, cancellationToken);
        // if (verifications.Count > 0)
        // {
        //     var verificationWorksheet = workbook.Worksheets.Add("Requirement verification");
        //     verificationWorksheet.FirstRow().Style.Font.Bold = true;
        //     verificationWorksheet.Cell("A1").Value = "Question";
        //     verificationWorksheet.Cell("B1").Value = "Answer";
        //     var row = 2;
        //     foreach (var verification in verifications)
        //     {
        //         verificationWorksheet.Cell($"A{row}").Value = verification.Question;
        //         verificationWorksheet.Cell($"B{row}").Value = verification.Answer;
        //         row++;
        //     }
        //     verificationWorksheet.Columns().AdjustToContents();
        // }
        foreach (var message in _history)
        {
            if (message.Content is null || message.Role == AuthorRole.System) continue;
            await _userInteraction.WriteAssistantMessageAsync(
                assistant: RoleName,
                message: $"➜ **Architect** Sending message *{message.Content}*",
                logLevel: LogLevel.Debug,
                cancel: cancellationToken);
        }

        var initialEstimates = await _architectAgent.EstimateAsync(_history, cancellationToken);
        // if (initialEstimates is not null && initialEstimates.UserStories.Count > 0)
        // {
        //     var initialEstimatesWorksheet = workbook.Worksheets.Add("Initial estimates");
        //     initialEstimatesWorksheet.FirstRow().Style.Font.Bold = true;
        //     initialEstimatesWorksheet.Cell("A1").Value = "User Story";
        //     initialEstimatesWorksheet.Cell("B1").Value = "Task";
        //     initialEstimatesWorksheet.Cell("C1").Value = "Optimistic";
        //     initialEstimatesWorksheet.Cell("D1").Value = "Realistic";
        //     initialEstimatesWorksheet.Cell("E1").Value = "Pessimistic";
        //     initialEstimatesWorksheet.Cell("F1").Value = "Effort";
        //     var row = 2;
        //     foreach (var userStory in initialEstimates.UserStories)
        //     {
        //         if (userStory.Tasks.Count == 0) continue; // what to do with user story without tasks?
        //         foreach (var task in userStory.Tasks)
        //         {
        //             initialEstimatesWorksheet.Cell($"A{row}").Value = userStory.Name;
        //             initialEstimatesWorksheet.Cell($"B{row}").Value = task.Name;
        //             initialEstimatesWorksheet.Cell($"C{row}").Value = task.Optimistic;
        //             initialEstimatesWorksheet.Cell($"D{row}").Value = task.Realistic;
        //             initialEstimatesWorksheet.Cell($"E{row}").Value = task.Pessimistic;
        //             initialEstimatesWorksheet.Cell($"F{row}").FormulaA1 = $"=(C{row}+4*D{row}+E{row})/6";
        //             row++;
        //         }
        //     }
        //     row++;
        //     initialEstimatesWorksheet.Cell($"A{row}").Value = "Total effort";
        //     initialEstimatesWorksheet.Cell($"F{row}").FormulaA1 = $"=SUM(F2:F{row - 1})";
        //     initialEstimatesWorksheet.Row(row).Style.Font.Bold = true;
        //     initialEstimatesWorksheet.Columns().AdjustToContents();
        // }

        foreach (var message in _history)
        {
            if (message.Content is null || message.Role == AuthorRole.System) continue;
            await _userInteraction.WriteAssistantMessageAsync(
                assistant: RoleName,
                message: $"➜ **Developer** Sending message *{message.Content}*",
                logLevel: LogLevel.Debug,
                cancel: cancellationToken);
        }
        var estimates = await _developerAgent.ValidateEstimatesAsync(_history, cancellationToken);
        StringBuilder response = new();
        if (estimates is not null && estimates.UserStories.Count > 0)
        {
            // var estimatesWorksheet = workbook.Worksheets.Add("Estimates");
            // estimatesWorksheet.FirstRow().Style.Font.Bold = true;
            // estimatesWorksheet.Cell("A1").Value = "User Story";
            // estimatesWorksheet.Cell("B1").Value = "Task";
            // estimatesWorksheet.Cell("C1").Value = "Optimistic";
            // estimatesWorksheet.Cell("D1").Value = "Realistic";
            // estimatesWorksheet.Cell("E1").Value = "Pessimistic";
            // estimatesWorksheet.Cell("F1").Value = "Effort";
            // estimatesWorksheet.Cell("G1").Value = "Correction Reason";
            response.Append(
                "|" +
                string.Join(
                    "|",
                    "User Story",
                    "Task",
                    "Optimistic",
                    "Realistic",
                    "Pessimistic",
                    "Effort",
                    "Correction Reason") +
                "|  \n");
            response.Append("|:---|:---|:---|:---|:---|:---|:---|  \n");
            // var row = 2;
            foreach (var userStory in estimates.UserStories)
            {
                if (userStory.Tasks.Count == 0) continue; // what to do with user story without tasks?
                foreach (var task in userStory.Tasks)
                {
                    // estimatesWorksheet.Cell($"A{row}").Value = userStory.Name;
                    // estimatesWorksheet.Cell($"B{row}").Value = task.Name;
                    // estimatesWorksheet.Cell($"C{row}").Value = task.Optimistic;
                    // estimatesWorksheet.Cell($"D{row}").Value = task.Realistic;
                    // estimatesWorksheet.Cell($"E{row}").Value = task.Pessimistic;
                    // estimatesWorksheet.Cell($"F{row}").FormulaA1 = $"=(C{row}+4*D{row}+E{row})/6";
                    // estimatesWorksheet.Cell($"G{row}").Value = task.CorrectionReason;
                    response.AppendLine(
                        "|" +
                        string.Join(
                            "|",
                            userStory.Name,
                            task.Name,
                            task.Optimistic,
                            task.Realistic,
                            task.Pessimistic,
                            $"{(task.Optimistic + 4 * task.Realistic + task.Pessimistic) / 6:0.####}",
                            task.CorrectionReason) +
                        "|");
                    // row++;
                }
            }
            // row++;
            // estimatesWorksheet.Cell($"A{row}").Value = "Total effort";
            // estimatesWorksheet.Cell($"F{row}").FormulaA1 = $"=SUM(F2:F{row - 1})";
            // estimatesWorksheet.Row(row).Style.Font.Bold = true;
            // estimatesWorksheet.Columns().AdjustToContents();
            response.Append(
                "|" +
                string.Join(
                    "|",
                    "__Total effort__",
                    "",
                    "",
                    "",
                    "",
                    "",
                    "__" +
                    $"{estimates.UserStories.Sum(
                        userStory => userStory.Tasks.Sum(
                            task => (task.Optimistic + 4 * task.Realistic + task.Pessimistic) / 6)):0.####}" +
                    "__|  \n"));
        }

        // await _userInteraction.WriteAssistantMessageAsync(RoleName, "Estimation complete", cancellationToken);

        // workbook.SaveAs("estimates.xlsx", new SaveOptions { EvaluateFormulasBeforeSaving = true });
        // using MemoryStream stream = new();
        // workbook.SaveAs(stream, new SaveOptions { EvaluateFormulasBeforeSaving = true });
        return response.ToString();
    }

    public async ValueTask<string?> UploadFileAsync(UserFile file, CancellationToken cancel)
    {
        return await _documentRepository.CreateDocumentAsync(file, cancel);
    }
}
