using gRPCFullDuplex.ServerAPI.Services;
using gRPCFullDuplex.Shared.Contract;
using gRPCFullDuplex.Shared.Domain;

namespace gRPCFullDuplex.ServerAPI.Startup
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddRazorPages();
            services.AddGrpc();
            services.AddTransient<IEratosthenesSieveAlgorithm, EratosthenesSieveAlgorithm>();
            services.AddHttpContextAccessor();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            // Configure the HTTP request pipeline.
            if (env.IsDevelopment())
            {
                app.UseWebAssemblyDebugging();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();

            app.UseBlazorFrameworkFiles();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseGrpcWeb();

            app.UseEndpoints(endpoint =>
            {
                endpoint.MapGrpcService<EratosthenesSieveService>().EnableGrpcWeb();
                endpoint.MapRazorPages();
                endpoint.MapLogEvents(); 
                endpoint.MapClientSettings();
                endpoint.MapFallbackToFile("index.html");
            });
        }
    }
}
