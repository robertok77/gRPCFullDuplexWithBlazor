using System.Collections.Concurrent;
using Bunit;
using FluentAssertions;
using gRPCFullDuplex.Blazor.Application.Contract;
using gRPCFullDuplex.Blazor.Client.Pages;
using gRPCFullDuplex.Blazor.Client.Startup;
using gRPCFullDuplex.ServerAPI.Startup;
using gRPCFullDuplex.Shared;
using gRPCFullDuplex.Tests.Helpers.IntegrationTests;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Moq;
using Xunit;
using Xunit.Abstractions;

namespace gRPCFullDuplex.Tests.IntegrationTests;

public class PagePrimesAndEratosthenesSieveServiceTest : IntegrationTestBase
{
    public PagePrimesAndEratosthenesSieveServiceTest(GrpcTestFixture<Startup> serverFixture, ITestOutputHelper outputHelper) :
        base(serverFixture, outputHelper)
    {
        ServerFixture.ConfigureWebHost(builder =>
            builder.ConfigureServices(services => services.AddTransient(_ => new Mock<Serilog.ILogger>().Object)));

        ClientContext.Services.ConfigureServices();
        ClientContext.Services.Replace(ServiceDescriptor.Transient(_ => new EratosthenesSieveProto.EratosthenesSieveProtoClient(Channel)));
    }

    [Fact]
    public async Task StartGeneratesPrimesExpectedPrimesSet()
    {
        //Arrange
        var component = ClientContext.RenderComponent<Primes>();
        var viewmodel = component.Instance.ViewModel;
        viewmodel.Range = 10;

        //Act
        await viewmodel.Start();

        //Assert
        viewmodel.Primes.Should().BeEquivalentTo(new ConcurrentDictionary<int, Kind>(new List<KeyValuePair<int, Kind>>()
        {
            new(2, Kind.Client), new(3, Kind.Client), new(5, Kind.Client)
            ,new(7, Kind.Server), new(11, Kind.Server)
        }), "List primes numbers from 2 to 11");
    }

    [Fact]
    public async Task StartAndCancelGeneratesPrimesExpectedUIEnabled()
    {
        //Arrange
        var component = ClientContext.RenderComponent<Primes>();
        var viewmodel = component.Instance.ViewModel;
        viewmodel.Range = 10;

        //Act
        await viewmodel.Start();
        viewmodel.Cancel();

        //Assert
        viewmodel.IsDisabled.Should().BeFalse("UI should be enabled after cancel");
        viewmodel.NotificationService.ErrorMessage.Should().Be("Search has been cancelled.");
    }

    [Fact]
    public async Task StartPrimeGenerationExpectOperationCanceledException()
    {
        //Arrange
        var component = ClientContext.RenderComponent<Primes>();
        var viewmodel = component.Instance.ViewModel;
        viewmodel.Range = 1;

        //Act
        var act = () => viewmodel.Start();

        //Assert
        await act.Should().ThrowExactlyAsync<OperationCanceledException>("Range should be greater than 2");
    }
}
