using gRPCFullDuplex.Shared.Model;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using Serilog.Parsing;
using ILogger = Serilog.ILogger;

namespace gRPCFullDuplex.ServerAPI.Startup;

public static class AppExtensions
{
    /// <summary>
    /// Configure log-event endpoint and forward to Seq Server
    /// </summary>
    /// <param name="app"></param>
    public static void MapLogEvents(this IEndpointRouteBuilder app) =>
        app.MapPost("/log-events", ([FromBody] LogEvent[] body, IHttpContextAccessor context, ILogger logger) =>
        {
            var apiKey = context.HttpContext?.Request.Headers["X-Api-Key"].FirstOrDefault();

            foreach (var logEvent in body)
            {
                logger.ForContext("ClientApiKey", apiKey)
                    .Write(logEvent.ToLogEventLevel(), (Exception?)null, "{message}", logEvent.RenderedMessage);
            }
        }).Produces(StatusCodes.Status200OK)
            .WithName("log-events")
            .ExcludeFromDescription();

    /// <summary>
    /// Configure client settings endpoint
    /// </summary>
    /// <param name="app"></param>
    public static void MapClientSettings(this IEndpointRouteBuilder app) =>
        app.MapGet("/clientsettings", (IConfiguration configuration) =>
        {
            var serilogSetting = new SerilogSetting();
            configuration.Bind("Serilog", serilogSetting);
            return serilogSetting;

        }).Produces(StatusCodes.Status200OK)
            .WithName("clientsettings")
            .ExcludeFromDescription();
}