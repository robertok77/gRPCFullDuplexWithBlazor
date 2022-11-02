using FluentValidation;
using gRPCFullDuplex.Blazor.Application.Handlers.Query;

namespace gRPCFullDuplex.Blazor.Application.Handlers.Validation;
/// <summary>
/// Validation rules for GenerateQuery
/// </summary>
public class GenerateQueryValidator : AbstractValidator<GenerateQuery>
{
    public GenerateQueryValidator()
    {
        RuleFor(x => x)
            .NotNull()
            .WithMessage($"{nameof(GenerateQuery)} cannot be null");
        RuleFor(x => x.Setting)
            .NotNull()
            .WithMessage($"{nameof(GenerateQuery.Setting)} cannot be null");
        RuleFor(x => x.Setting.Range)
            .GreaterThan(2)
            .WithMessage($"{nameof(GenerateQuery.Setting.Range)} should be greater than 2");
    }
}