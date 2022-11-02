using FluentAssertions;
using Grpc.Core;
using gRPCFullDuplex.Blazor.Application.Contract;
using gRPCFullDuplex.Blazor.Application.Handlers.Query;
using gRPCFullDuplex.Blazor.Application.Services;
using gRPCFullDuplex.Shared;
using gRPCFullDuplex.Shared.Contract;
using gRPCFullDuplex.Tests.Helpers;
using gRPCFullDuplex.Tests.Helpers.UnitTests;
using Moq;
using Xunit;
using static gRPCFullDuplex.Shared.EratosthenesSieveProto;

namespace gRPCFullDuplex.Tests.UnitTests.Blazor.Client;

public class ClientTestsEratosthenesSieveProtoClient
{
    /// <summary>
    /// gRPC test of client side duplex streaming
    /// </summary>
    /// <returns></returns>
    [Fact]
    public async Task GenerateQueryHandlerTestExpectResponse()
    {
        //Arrange
        var range = 10;
        var algMock = new Mock<IEratosthenesSieveAlgorithm>();
        algMock.Setup(x => x.Search(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .Returns(() => new ISieveResponse[]
            {
                new PrimeResponse(0, 2), new PrimeResponse(1, 3),new NotPrimeResponse(2, 4) , new PrimeResponse(3, 5)
                ,new NotPrimeResponse(4, 6)
            }.AsEnumerable());

        var clientMock = new Mock<EratosthenesSieveProtoClient>();
        var streamWriter = new TestServerStreamWriter<SieveRequest>(TestServerCallContext.Create());
        var streamReader = new TestAsyncStreamReader<SieveReply>(TestServerCallContext.Create());

        clientMock.Setup(x => x.Call(It.IsAny<Metadata>(), It.IsAny<DateTime?>(), It.IsAny<CancellationToken>()))
            .Returns(CallHelpers.CreateAsyncDuplexStreamingCall(streamWriter, streamReader));

        var serverTask = Task.Run(async () =>
        {
            await foreach (var r in streamWriter.ReadAllAsync()) { }
            streamReader.AddMessage(new SieveReply { Index = 5, Value = 7, IsPrime = true });
            streamReader.AddMessage(new SieveReply { Index = 6, Value = 8, IsPrime = false });
            streamReader.AddMessage(new SieveReply { Index = 7, Value = 9, IsPrime = false });
            streamReader.AddMessage(new SieveReply { Index = 8, Value = 10, IsPrime = false });
            streamReader.AddMessage(new SieveReply { Index = 9, Value = 11, IsPrime = true });
            streamReader.Complete();
        });

        var handler = new EratosthenesSieveClientHandler(clientMock.Object, algMock.Object);
        var query = new GenerateQuery(new GenerateSetting(range), new SieveChannel());

        //Act
        await Task.WhenAll(serverTask, handler.Handle(query, CancellationToken.None));

        var result = new List<SieveValue>();
        await foreach (var value in query.SieveChannel.ChannelReader.ReadAllAsync())
            result.Add(value);
        
        //Assert
        result.Should().BeEquivalentTo(new SieveValue[]
        {
            new(new PrimeResponse(0, 2), Kind.Client), new(new PrimeResponse(1, 3), Kind.Client), new(new NotPrimeResponse(2, 4), Kind.Client)
            , new(new PrimeResponse(3, 5), Kind.Client), new(new NotPrimeResponse(4, 6), Kind.Client)
            , new(new PrimeResponse(5, 7), Kind.Server), new(new NotPrimeResponse(6, 8), Kind.Server),new(new NotPrimeResponse(7, 9), Kind.Server)
            , new(new NotPrimeResponse(8, 10), Kind.Server), new(new PrimeResponse(9, 11), Kind.Server)
        });
    }
}