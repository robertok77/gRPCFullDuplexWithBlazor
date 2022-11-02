using Serilog;

namespace gRPCFullDuplex.ServerAPI;

public class Program
{
    public static void Main(string[] args)
    {
        CreateHostBuilder(args).Build().Run();
    }
    /// <summary>
    /// Configure Host with Serilog and Seq
    /// </summary>
    /// <param name="args"></param>
    /// <returns></returns>
    public static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .ConfigureWebHostDefaults(webBuilder => 
                webBuilder.UseStartup<Startup.Startup>())
            .UseSerilog((context, configuration) => 
                configuration.ReadFrom.Configuration(context.Configuration));
}