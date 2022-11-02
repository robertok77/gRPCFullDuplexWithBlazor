using gRPCFullDuplex.ServerAPI.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using FluentAssertions.Specialized;
using gRPCFullDuplex.Shared.Contract;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using gRPCFullDuplex.Tests.Helpers.UnitTests;
using gRPCFullDuplex.Shared;
using gRPCFullDuplex.Tests.Helpers;

namespace gRPCFullDuplex.Tests.UnitTests.ServerAPI;

public class ServerTestsEratosthenesSieveService
{
    /// <summary>
    /// gRPC test of server side duplex streaming
    /// </summary>
    /// <returns></returns>
    [Fact]
    public async Task CallEratosthenesSieveServiceExpectedResponseSameAsRequest()
    {
        //Arrange
        var algMock = new Mock<IEratosthenesSieveAlgorithm>();
        algMock.Setup(x => x.SearchAsync(4, 3, It.IsAny<IAsyncEnumerable<int>>(), CancellationToken.None))
            .Returns(() => new ISieveResponse[]
                { new NotPrimeResponse(0, 4), new PrimeResponse(1, 5), new NotPrimeResponse(2, 6) }.AsAsyncEnumerable());

        var service = new EratosthenesSieveService(algMock.Object, new Mock<ILogger<EratosthenesSieveService>>().Object);
        var requestStream = new TestAsyncStreamReader<SieveRequest>(TestServerCallContext.Create());
        var responseStream = new TestServerStreamWriter<SieveReply>(TestServerCallContext.Create());

        //Act
        var task = service.Call(requestStream, responseStream, TestServerCallContext.Create());

        //Assert
        requestStream.AddMessage(new SieveRequest { Sieve = new Sieve() { Start = 4, Range = 3 } });
        (await responseStream.ReadNextAsync()).Should().Be(new SieveReply { Index = 0, IsPrime = false, Value = 4 });
        requestStream.AddMessage(new SieveRequest { Prime = 2 });
        (await responseStream.ReadNextAsync()).Should().Be(new SieveReply { Index = 1, IsPrime = true, Value = 5 });
        requestStream.AddMessage(new SieveRequest { Prime = 3 });
        (await responseStream.ReadNextAsync()).Should().Be(new SieveReply { Index = 2, IsPrime = false, Value = 6 });
        requestStream.Complete();

        await task;
    }
}