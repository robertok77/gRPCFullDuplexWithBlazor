using System.Reflection;
using FluentValidation;
using Grpc.Net.Client;
using Grpc.Net.Client.Web;
using gRPCFullDuplex.Blazor.Application.Contract;
using gRPCFullDuplex.Blazor.Application.Handlers.Exception;
using gRPCFullDuplex.Blazor.Application.Handlers.Logging;
using gRPCFullDuplex.Blazor.Application.Handlers.Query;
using gRPCFullDuplex.Blazor.Application.Handlers.Validation;
using gRPCFullDuplex.Blazor.Application.Services;
using gRPCFullDuplex.Blazor.Client.ViewModel;
using gRPCFullDuplex.Shared;
using gRPCFullDuplex.Shared.Contract;
using gRPCFullDuplex.Shared.Domain;
using MediatR;
using MediatR.Pipeline;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Serilog;
using Serilog.Core;
using Serilog.Events;
using Serilog.Sinks.Http.HttpClients;

namespace gRPCFullDuplex.Blazor.Client.Startup;

public delegate T Factory<A, T>(A a);
public static class ServiceCollectionExtension
{
    /// <summary>
    /// Configure all services
    /// </summary>
    /// <param name="services"></param>
    public static void ConfigureServices(this IServiceCollection services)
    {
        services.AddgRpc();
        services.AddServices();
        services.AddMediator();
    }
    /// <summary>
    /// gRPC client configuration
    /// </summary>
    /// <param name="services"></param>
    /// <param name="baseAddress"></param>
    public static void AddgRpc(this IServiceCollection services)
    {
        services.AddTransient(provider =>
        {
            var httpClient = new HttpClient(new GrpcWebHandler(GrpcWebMode.GrpcWeb, new HttpClientHandler()));
            var baseUri = new Uri(provider.GetRequiredService<IWebAssemblyHostEnvironment>().BaseAddress);
            var channel = GrpcChannel.ForAddress(baseUri, new GrpcChannelOptions { HttpClient = httpClient });
            return new EratosthenesSieveProto.EratosthenesSieveProtoClient(channel);
        });
    }
    public static void AddServices(this IServiceCollection services)
    {
        services.AddTransient<IEratosthenesSieveAlgorithm, EratosthenesSieveAlgorithm>();

        services.AddSingleton<Factory<GenerateSetting, GenerateQuery>>(provider =>
            settings => ActivatorUtilities.CreateInstance<GenerateQuery>(provider, settings));
        //prime numbers are sent from service to ui through channels
        services.AddTransient<ISieveChannel, SieveChannel>();
        services.AddSingleton<INotificationService, NotificationService>();
        services.AddScoped<PrimesVM>();
    }
    public static void AddMediator(this IServiceCollection services)
    {
        //validation, exception,logging handling 
        services.AddMediatR(Assembly.GetExecutingAssembly(), typeof(EratosthenesSieveClientHandler).Assembly);
        services.AddValidatorsFromAssemblyContaining<GenerateQuery>(ServiceLifetime.Transient);
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehaviour<,>));
        services.AddTransient<IRequestExceptionHandler<GenerateQuery, bool>, GenerateQueryExceptionHandler>();
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(LoggingBehaviour<,>));
    }
    /// <summary>
    /// Serilog configuration
    /// sends logs through server api to Seq, since WASM does not support Seq
    /// </summary>
    /// <param name="loggingBuilder"></param>
    /// <param name="baseAddress"></param>
    /// <param name="minimumLevel"></param>
    public static void AddLogging(this ILoggingBuilder loggingBuilder, string baseAddress, string minimumLevel)
    {
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.ControlledBy(new LoggingLevelSwitch((LogEventLevel)Enum.Parse(typeof(LogEventLevel), minimumLevel)))
            .WriteTo.Console()
            .Enrich.FromLogContext()
            .WriteTo.Http(requestUri: $@"{baseAddress}log-events", queueLimitBytes: null,
                httpClient: new JsonHttpClient(new HttpClient()
                {
                    BaseAddress = new Uri(baseAddress),
                    DefaultRequestHeaders = { { "X-Api-Key", Guid.NewGuid().ToString() } }
                }))
            .CreateLogger();

        loggingBuilder.AddSerilog();
    }

    /// <summary>
    /// Retrieve configuration settings from Server
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="builder"></param>
    /// <param name="baseAddress"></param>
    /// <param name="requestUri"></param>
    /// <returns></returns>
    public static async Task<T> AddRemoteSettingsAsync<T>(
        this (IConfigurationBuilder configBuilder, IConfiguration configuration, IServiceCollection services) builder
        , string baseAddress
        , string requestUri) where T : class, new()
    {
        using var http = new HttpClient() { BaseAddress = new Uri(baseAddress) };
        using var response = await http.GetAsync(requestUri);
        await using var stream = await response.Content.ReadAsStreamAsync();
        builder.configBuilder.AddJsonStream(stream);
        var t = new T();
        builder.configuration.Bind(t);
        builder.services.Configure<T>(opt => builder.configuration.Bind(opt));
        return t;
    }
}
