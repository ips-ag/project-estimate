using Microsoft.Agents.AI.Workflows;

namespace ProjectEstimate.Repositories.Agents.Consultant;

internal sealed class UserAnswerExecutor() : Executor<string>("UserAnswer")
{
    public override ValueTask HandleAsync(
        string message,
        IWorkflowContext context,
        CancellationToken cancellationToken = new())
    {
        return ValueTask.CompletedTask;
    }
}
