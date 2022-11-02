using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Grpc.Core;

namespace gRPCFullDuplex.Tests.Helpers.UnitTests;

public static class CallHelpers
{
    public static AsyncDuplexStreamingCall<TRequest, TResponse> CreateAsyncDuplexStreamingCall<TRequest, TResponse>(
        IClientStreamWriter<TRequest> streamWriter,
        IAsyncStreamReader<TResponse> streamReader)
        => new(streamWriter
            , streamReader
            , Task.FromResult(new Metadata()), () => Status.DefaultSuccess
            , () => new Metadata()
            , () => { });
}