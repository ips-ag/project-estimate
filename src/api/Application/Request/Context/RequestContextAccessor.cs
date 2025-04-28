namespace ProjectEstimate.Application.Request.Context;

#pragma warning disable CS8766 // Nullability of reference types in return type doesn't match implicitly implemented member (possibly because of nullability attributes).

internal class RequestContextAccessor : IRequestContextAccessor
{
    private static readonly AsyncLocal<RequestContextHolder> CurrentContext = new();

    public RequestContext? Context
    {
        get => CurrentContext.Value?.Context ?? throw new InvalidOperationException("Context was not set.");
        set
        {
            var holder = CurrentContext.Value;
            if (holder != null)
            {
                // Clear current HttpContext trapped in the AsyncLocals, as its done.
                holder.Context = null;
            }
            if (value != null)
            {
                // Use an object indirection to hold the HttpContext in the AsyncLocal,
                // so it can be cleared in all ExecutionContexts when its cleared.
                CurrentContext.Value = new RequestContextHolder { Context = value };
            }
        }
    }

    private class RequestContextHolder
    {
        public RequestContext? Context { get; set; }
    }
}
