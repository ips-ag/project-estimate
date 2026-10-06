using System.ComponentModel;
using Microsoft.Extensions.AI;

namespace ProjectEstimate.Repositories.Agents.Tools;

/// <summary>
///     Declaration-only tool which lets an agent ask the user a question.
///     The call is not invoked by the agent, it surfaces as a workflow request which is answered by the user.
/// </summary>
internal static class AskUserTool
{
    public const string Name = "ask_user";
    public const string QuestionParameter = "question";

    public static readonly AIFunctionDeclaration Declaration = AIFunctionFactory.CreateDeclaration(
        name: Name,
        description: "Ask the user a question and wait for the answer.",
        jsonSchema: AIFunctionFactory.Create(
            ([Description("Question for the user, followed by a short explanation in brackets.")] string question) =>
                string.Empty).JsonSchema);
}
