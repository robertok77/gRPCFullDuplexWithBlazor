using gRPCFullDuplex.Blazor.Application.Contract;
using MediatR;

namespace gRPCFullDuplex.Blazor.Application.Handlers.Query;

public record GenerateQuery(GenerateSetting Setting, ISieveChannel SieveChannel) : IRequest<bool>;