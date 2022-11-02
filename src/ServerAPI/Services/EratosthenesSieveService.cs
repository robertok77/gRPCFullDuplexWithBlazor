using Grpc.Core;
using gRPCFullDuplex.Shared;
using gRPCFullDuplex.Shared.Contract;
using gRPCFullDuplex.Shared.Extensions;

namespace gRPCFullDuplex.ServerAPI.Services;
/// <summary>
/// EratosthenesSieve Service responsible for receiving call from client.
/// Both request and response stream are supported
/// </summary>
public class EratosthenesSieveService : EratosthenesSieveProto.EratosthenesSieveProtoBase
{
    private readonly IEratosthenesSieveAlgorithm _eratosthenesSieve;
    private readonly ILogger<EratosthenesSieveService> _logger;
    public EratosthenesSieveService(IEratosthenesSieveAlgorithm eratosthenesSieve, ILogger<EratosthenesSieveService> logger)
    {
        _eratosthenesSieve = eratosthenesSieve;
        _logger = logger;
    }

    public override async Task Call(IAsyncStreamReader<SieveRequest> requestStream, IServerStreamWriter<SieveReply> responseStream, ServerCallContext context)
    {
        if (!await requestStream.MoveNext(context.CancellationToken).ConfigureAwait(false)) return;
        if (requestStream.Current?.ResultCase != SieveRequest.ResultOneofCase.Sieve) 
            throw new ArgumentException($"Expected sieve initialization data. {requestStream.Current?.ResultCase}");
        
        await foreach (var sieveResponse in _eratosthenesSieve.SearchAsync(
                           requestStream.Current.Sieve.Start,
                           requestStream.Current.Sieve.Range,
                           requestStream.ReadAllPrimeAsync(context.CancellationToken),
                           context.CancellationToken).ConfigureAwait(false))
        {
            _logger.LogDebug("Sieve value sent to stream: {sieveResponse}", sieveResponse);

            await responseStream.WriteAsync(new SieveReply
            {
                Value = sieveResponse.Value,
                Index = sieveResponse.Index,
                IsPrime = sieveResponse is PrimeResponse
            }).ConfigureAwait(false);
        }
        context.Status = Status.DefaultSuccess;
    }
}
