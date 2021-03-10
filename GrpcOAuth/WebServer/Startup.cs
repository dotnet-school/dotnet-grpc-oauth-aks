using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Grpc.Net.Client;
using GrpcServer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace WebServer
{
    public class Startup
    {
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapGet("/", async context =>
                {
                  using var channel = GrpcChannel.ForAddress(Environment.GetEnvironmentVariable("GrpcServer"));
      
                  // Greeter service is defined in hello.proto
                  // <service-name>.<service-name>Client is auto-created
                  var client = new Greeter.GreeterClient(channel);
      
                  // HelloRequest is defined in hello.proto
                  var request = new HelloRequest();
                  request.Name = "Nishant";
      
                  // SayHello method is defined in hello.proto
                  var response = client.SayHello(request);

                    await context.Response.WriteAsync(response.Message);
                });
            });
        }
    }
}
