using ProjectEstimate.Repositories.Hubs.Models;

namespace ProjectEstimate.Repositories.Hubs.Converters;

public class LogLevelConverter
{
    public LogLevelModel ToModel(LogLevel logLevel)
    {
        return logLevel switch
        {
            LogLevel.Trace => LogLevelModel.Trace,
            LogLevel.Debug => LogLevelModel.Debug,
            LogLevel.Information => LogLevelModel.Info,
            LogLevel.Warning => LogLevelModel.Warning,
            LogLevel.Error => LogLevelModel.Error,
            LogLevel.Critical => LogLevelModel.Critical,
            LogLevel.None => LogLevelModel.Info, // Default fallback
            _ => LogLevelModel.Info
        };
    }
}
