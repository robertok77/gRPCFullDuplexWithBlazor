using FluentValidation;
using MediatR;

namespace gRPCFullDuplex.Blazor.Application.Handlers.Validation;
/// <summary>
/// Pipeline validation for Query / Commands
/// </summary>
/// <typeparam name="TRequest"></typeparam>
/// <typeparam name="TResponse"></typeparam>
public record ValidationBehaviour<TRequest, TResponse>
    (IValidator<TRequest> Validator) : IPipelineBehavior<TRequest, TResponse> where TRequest : IRequest<TResponse>
{
    public Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        Validator.ValidateAndThrow(request);
        return next();
    }
}
