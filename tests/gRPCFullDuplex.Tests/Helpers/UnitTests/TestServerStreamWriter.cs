
using System.Threading.Channels;
using Grpc.Core;

namespace gRPCFullDuplex.Tests.Helpers.UnitTests;

public class TestServerStreamWriter<T> : IServerStreamWriter<T>, IClientStreamWriter<T>, IAsyncStreamReader<T> where T : class
{
    private readonly ServerCallContext _serverCallContext;
    private readonly Channel<T> _channel;

    public TestServerStreamWriter(ServerCallContext serverCallContext)
    {
        _channel = Channel.CreateUnbounded<T>();

        _serverCallContext = serverCallContext;
    }

    public void Complete() => _channel.Writer.Complete();

    public IAsyncEnumerable<T> ReadAllAsync() => _channel.Reader.ReadAllAsync();

    public async Task<T?> ReadNextAsync()
    {
        if (await _channel.Reader.WaitToReadAsync())
        {
            _channel.Reader.TryRead(out var message);
            return message;
        }

        return null;
    }
    #region IServerStreamWriter

    public WriteOptions? WriteOptions { get; set; }
    public Task WriteAsync(T message)
    {
        if (_serverCallContext.CancellationToken.IsCancellationRequested)
        {
            return Task.FromCanceled(_serverCallContext.CancellationToken);
        }

        if (!_channel.Writer.TryWrite(message))
        {
            throw new InvalidOperationException("Unable to write message.");
        }

        return Task.CompletedTask;
    }
    #endregion

    #region IClientStreamWriter
    public Task CompleteAsync() => Task.Run(() => _channel.Writer.Complete());
    #endregion

    #region IAsyncStreamReader
    public async Task<bool> MoveNext(CancellationToken cancellationToken)
    {
        _serverCallContext.CancellationToken.ThrowIfCancellationRequested();

        if (await _channel.Reader.WaitToReadAsync(cancellationToken) &&
            _channel.Reader.TryRead(out var message))
        {
            Current = message;
            return true;
        }

        Current = null!;
        return false;
    }
    public T Current { get; private set; } = null!;
    #endregion
}