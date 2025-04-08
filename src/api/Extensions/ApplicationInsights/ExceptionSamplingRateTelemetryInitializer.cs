using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;

namespace ProjectEstimate.Extensions.ApplicationInsights;

public class ExceptionSamplingRateTelemetryInitializer: ITelemetryInitializer
{
    public void Initialize(ITelemetry telemetry)
    {
        ISupportSampling? exceptionTelemetry = telemetry as ExceptionTelemetry;
        if (exceptionTelemetry is null) return;
        exceptionTelemetry.SamplingPercentage = 100;
    }
}