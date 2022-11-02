using Grpc.Net.Client;
using Grpc.Net.Client.Web;
using gRPCFullDuplex.Blazor.Client.Startup;
using gRPCFullDuplex.Shared;
using gRPCFullDuplex.Shared.Model;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

namespace gRPCFullDuplex.Blazor.Client;

public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebAssemblyHostBuilder.CreateDefault(args);
        builder.RootComponents.Add<App>("#app");
        builder.RootComponents.Add<HeadOutlet>("head::after");

        builder.Services.ConfigureServices();

        //logging settings are defined on server
        var logSettings = await (builder.Configuration, builder.Configuration, builder.Services)
            .AddRemoteSettingsAsync<SerilogSetting>(builder.HostEnvironment.BaseAddress, "/clientsettings");

        //add serilog logger by sending logs to Seq server 
        builder.Logging.AddLogging(builder.HostEnvironment.BaseAddress, logSettings.MinimumLevel.Default);
        
        await builder.Build().RunAsync();
    }
    
}