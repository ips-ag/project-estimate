namespace ProjectEstimate.Application.Request.Context;

public interface IRequestContextAccessor
{
    RequestContext? Context { get; set; }
}
