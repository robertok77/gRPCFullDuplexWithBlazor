using Grpc.Core;
using gRPCFullDuplex.Blazor.Application.Contract;
using gRPCFullDuplex.Blazor.Application.Handlers.Query;
using MediatR.Pipeline;
using Microsoft.Extensions.Logging;

namespace gRPCFullDuplex.Blazor.Application.Handlers.Exception;

/// <summary>
/// Handling exceptions generated during execution Query/Command cycle
/// through MediatR pipeline behaviour
/// </summary>
public record GenerateQueryExceptionHandler
    (INotificationService NotificationService, ILogger<GenerateQueryExceptionHandler> Logger) 
    : IRequestExceptionHandler<GenerateQuery, bool>
{
    public Task Handle(GenerateQuery request, System.Exception exception, RequestExceptionHandlerState<bool> state, CancellationToken token)
    {
        switch (exception)
        {
            case RpcException rpcException:
                Logger.LogError("{rpcException}", rpcException);
                NotificationService.ErrorMessage = "Communication problem. Please, try again.";
                break;
            default:
                Logger.LogError("{exception}", exception);
                NotificationService.ErrorMessage = "Cannot generate prime. Please, contact with Administrator.";
                break;
        }
        state.SetHandled(false);
        return Task.CompletedTask;
    }
}