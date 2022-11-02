using System.Threading.Channels;
using gRPCFullDuplex.Blazor.Application.Contract;

namespace gRPCFullDuplex.Blazor.Application.Services;

public class SieveChannel : ISieveChannel
{
    private readonly Channel<SieveValue> _channel = Channel.CreateUnbounded<SieveValue>();
    public ChannelWriter<SieveValue> ChannelWriter => _channel.Writer;
    public ChannelReader<SieveValue> ChannelReader => _channel.Reader;
}