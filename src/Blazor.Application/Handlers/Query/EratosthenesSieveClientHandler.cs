using Grpc.Core;
using gRPCFullDuplex.Blazor.Application.Contract;
using gRPCFullDuplex.Shared;
using gRPCFullDuplex.Shared.Contract;
using MediatR;

namespace gRPCFullDuplex.Blazor.Application.Handlers.Query;

/// <summary>
/// Handler for GenerateQuery.
/// Generate prime numbers from first half of Eratosthenes sieve.
/// Initializes and executes communication with ServerAPI through gRPC
/// for retrieving prime numbers from second half of Eratosthenes sieve.
/// </summary>
public record EratosthenesSieveClientHandler(
    EratosthenesSieveProto.EratosthenesSieveProtoClient EratosthenesSieveClient
    ,IEratosthenesSieveAlgorithm EratosthenesSieve) 
    : IRequestHandler<GenerateQuery,bool>
{
    private GenerateQuery _request = null!;
    private CancellationToken _token = CancellationToken.None;
    
    public async Task<bool> Handle(GenerateQuery request, CancellationToken token)
    {
        _request = request ?? throw new ArgumentNullException(nameof(request));
        _token = token;

        using var duplexStreaming = EratosthenesSieveClient.Call(null, null, token);
        
        await InitializeSieve(duplexStreaming.RequestStream).ConfigureAwait(false);

        var responseTask = ReadResponseStreamAsync(duplexStreaming.ResponseStream, request.SieveChannel);
        var requestTask = WriteRequestStreamAsync(duplexStreaming.RequestStream, request.SieveChannel);

        await requestTask.ConfigureAwait(false);
        await responseTask.ConfigureAwait(false);
        request.SieveChannel.ChannelWriter.Complete();
        return true;
    }

    private async Task InitializeSieve(IClientStreamWriter<SieveRequest> requestStream) =>
        await requestStream.WriteAsync(new SieveRequest()
        {
            Sieve = new Sieve()
            {
                Start = _request.Setting.ServerStart,
                Range = _request.Setting.ServerRange
            }
        }, _token).ConfigureAwait(false);

    private async Task WriteRequestStreamAsync(IClientStreamWriter<SieveRequest> requestStream, ISieveChannelWriter sieveChannelWriter)
    {
        await Task.Run(async () =>
        {
            foreach (var sieveResponse in EratosthenesSieve.Search(_request.Setting.Start, _request.Setting.WebRange,_token))
            {
                await WriteToChannelAsync(sieveChannelWriter, new SieveValue(sieveResponse, Kind.Client)).ConfigureAwait(false);
                if (sieveResponse is PrimeResponse)
                {
                    await requestStream.WriteAsync(new SieveRequest() { Prime = sieveResponse.Value }, _token).ConfigureAwait(false);
                }
            }
        }, _token).ConfigureAwait(false);
        await requestStream.CompleteAsync().ConfigureAwait(false);
    }

    private async Task ReadResponseStreamAsync(IAsyncStreamReader<SieveReply> responseStream, ISieveChannelWriter sieveChannelWriter)
    {
        await Task.Delay(100).ConfigureAwait(false);
        await foreach (var replay in responseStream.ReadAllAsync(_token).ConfigureAwait(false))
        {
            await WriteToChannelAsync(sieveChannelWriter, new SieveValue(replay.IsPrime
                        ? new PrimeResponse(replay.Index, replay.Value)
                        : new NotPrimeResponse(replay.Index, replay.Value), Kind.Server)).ConfigureAwait(false);
        }
    }
    private ValueTask WriteToChannelAsync(IChannelWriter? channelWriter, SieveValue value)
        => channelWriter != null ? channelWriter.ChannelWriter.WriteAsync(value, _token) : ValueTask.CompletedTask;
}