
using Bunit;
using Grpc.Net.Client;
using gRPCFullDuplex.ServerAPI.Startup;
using gRPCFullDuplex.Tests.Helpers.IntegrationTests;
using Microsoft.Extensions.Logging;
using Xunit;
using Xunit.Abstractions;

namespace gRPCFullDuplex.Tests.IntegrationTests;

public class IntegrationTestBase : IClassFixture<GrpcTestFixture<Startup>>, IDisposable
{
    private GrpcChannel? _channel;
    private readonly IDisposable? _testContext;

    protected GrpcTestFixture<Startup> ServerFixture { get; }
    protected TestContext ClientContext { get; }
    protected ILoggerFactory LoggerFactory => ServerFixture.LoggerFactory;
    protected GrpcChannel Channel => _channel ??= CreateChannel();

    public IntegrationTestBase(GrpcTestFixture<Startup> serverFixture, ITestOutputHelper outputHelper)
    {
        ServerFixture = serverFixture;
        _testContext = ServerFixture.GetTestContext(outputHelper);
        ClientContext = new TestContext();
    }
    protected GrpcChannel CreateChannel() => GrpcChannel.ForAddress("http://localhost"
        , new GrpcChannelOptions
        {
            LoggerFactory = LoggerFactory,
            HttpHandler = ServerFixture.Handler
        });
    public void Dispose()
    {
        _testContext?.Dispose();
        _channel = null;
        ClientContext?.Dispose();
    }
}