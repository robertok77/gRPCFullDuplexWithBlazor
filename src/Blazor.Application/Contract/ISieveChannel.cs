using System.Threading.Channels;
using gRPCFullDuplex.Shared.Contract;

namespace gRPCFullDuplex.Blazor.Application.Contract;
public interface IChannelWriter
{
    ChannelWriter<SieveValue> ChannelWriter { get; }
}

public interface IChannelReader
{
    ChannelReader<SieveValue> ChannelReader { get; }
}

public interface ISieveChannel : ISieveChannelReader, ISieveChannelWriter { }

public interface ISieveChannelWriter : IChannelWriter { }

public interface ISieveChannelReader : IChannelReader { }

public record SieveValue(ISieveResponse Response, Kind Kind);
public enum Kind { Client, Server }

