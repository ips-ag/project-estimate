namespace ProjectEstimate.Application.Request.Context;

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
                holder.Context = null;
            }
            if (value != null)
            {
                CurrentContext.Value = new RequestContextHolder { Context = value };
            }
        }
    }

    private class RequestContextHolder
    {
        public RequestContext? Context { get; set; }
    }
}
