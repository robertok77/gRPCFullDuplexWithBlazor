using MediatR;
using Microsoft.Extensions.Logging;

namespace gRPCFullDuplex.Blazor.Application.Handlers.Logging;

/// <summary>
/// Logging execution of Query/Command cycle
/// through MediatR pipeline behaviour
/// </summary>
/// <typeparam name="TRequest"></typeparam>
/// <typeparam name="TResponse"></typeparam>
public record LoggingBehaviour<TRequest, TResponse>
    (ILogger<LoggingBehaviour<TRequest, TResponse>> Logger) 
    : IPipelineBehavior<TRequest, TResponse> where TRequest : IRequest<TResponse>
{
    public Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken token)
    {
        using var scope = Logger.BeginScope($"Handling <{typeof(TRequest).Name}, {typeof(TResponse).Name}>");
        {
            Logger.LogInformation("{Request}", request);
            return next();
        }
    }
}
