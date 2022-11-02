using Serilog.Events;

namespace gRPCFullDuplex.ServerAPI.Startup;

public static class LogEventExtensions
{
    public static LogEventLevel ToLogEventLevel(this LogEvent logEvent) =>
        Enum.TryParse<LogEventLevel>(logEvent.Level, true, out var result) ? result : LogEventLevel.Fatal;
}

public record LogEvent(DateTime Timestamp, string Level, string RenderedMessage);