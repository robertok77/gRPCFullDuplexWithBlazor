using FluentAssertions;
using Google.Protobuf.WellKnownTypes;
using gRPCFullDuplex.Shared;
using gRPCFullDuplex.Shared.Contract;
using gRPCFullDuplex.Shared.Domain;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using System.Collections.Generic;
using Xunit;
using gRPCFullDuplex.Tests.Helpers;

namespace gRPCFullDuplex.Tests.UnitTests.Domain;

public class TestsEratosthenesSieveAlgorithm
{
    /// <summary>
    /// EratosthenesSieveAlgorithm 'Search' test
    /// </summary>
    [Fact]
    public void SearchSieveExpectedResponse()
    {
        //Arrange
        var mockLogger = new Mock<ILogger<EratosthenesSieveAlgorithm>>();
        var alg = new EratosthenesSieveAlgorithm(mockLogger.Object);
        
        //Act
        var result = alg.Search(2, 10, CancellationToken.None);
        //Assert
        result.Should().BeEquivalentTo(new ISieveResponse[]
        {
            new PrimeResponse(0, 2), new NotPrimeResponse(2, 4), new NotPrimeResponse(4, 6), new NotPrimeResponse(6, 8),
            new NotPrimeResponse(8, 10), new PrimeResponse(1, 3), new NotPrimeResponse(7, 9), new PrimeResponse(3, 5),
            new PrimeResponse(5, 7), new PrimeResponse(9, 11)
        });
    }
    /// <summary>
    /// EratosthenesSieveAlgorithm 'SearchAsync' test
    /// </summary>
    [Fact]
    public void SearchAsyncSieveUsingPrimesExpectedResponse()
    {
        //Arrange
        var mockLogger = new Mock<ILogger<EratosthenesSieveAlgorithm>>();
        var alg = new EratosthenesSieveAlgorithm(mockLogger.Object);
        var mockPrimesEnum = new Mock<IAsyncEnumerable<int>>();

        mockPrimesEnum.Setup(x => x.GetAsyncEnumerator(CancellationToken.None))
            .Returns(() => new[] { 2, 3, 5, 7 }.AsAsyncEnumerable().GetAsyncEnumerator(CancellationToken.None));

        //Act, Assert
        alg.SearchAsync(2 + 10, 10, mockPrimesEnum.Object, CancellationToken.None)
            .ToBlockingEnumerable()
            .Should()
            .BeEquivalentTo(new ISieveResponse[]
            {
                new NotPrimeResponse(0,12),new PrimeResponse(1,13),new NotPrimeResponse(2,14),new NotPrimeResponse(3,15)
                ,new NotPrimeResponse(4,16),new PrimeResponse(5,17),new NotPrimeResponse(6,18),new PrimeResponse(7,19)
                ,new NotPrimeResponse(8,20),new NotPrimeResponse(9,21)
            });
    }
}