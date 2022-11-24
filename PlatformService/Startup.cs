using System;
using System.IO;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using PlatformService.AsyncDataServices;
using PlatformService.Data;
using PlatformService.SyncDataServices.Grpc;
using PlatformService.SyncDataServices.Http;

namespace PlatformService
{
    public class Startup
    {
        private readonly IWebHostEnvironment _environment;

        public Startup(IConfiguration configuration, IWebHostEnvironment environment)
        {
            Configuration = configuration;
            _environment = environment;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContext<AppDbContext>(opts =>
            {
                if (_environment.IsDevelopment())
                {
                    Console.WriteLine("--->> Using InMem Database");
                    opts.UseInMemoryDatabase("mem");
                }
                else
                {
                    Console.WriteLine("--->> Using SQL Server Database");
                    opts.UseSqlServer(Configuration.GetConnectionString("PlatformsConn"));
                }
            });
            services.AddScoped<IPlatformRepo, PlatformRepo>();
            services.AddSingleton<IMessageBusClient, MessageBusClient>();
            services.AddGrpc();

            services.AddHttpClient<ICommandDataClient, HttpCommandDataClient>();
            services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
            services.AddControllers();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "PlatformService", Version = "v1" });
            });
        }

        public void Configure(IApplicationBuilder app)
        {
            if (_environment.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "PlatformService v1"));
            }

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapGrpcService<GrpcPlatformService>();
                endpoints.MapGet("/protos/platforms.proto", async context =>
                {
                    await context.Response.WriteAsync(await File.ReadAllTextAsync("Protos/platforms.proto"));
                });
            });

            PrepDb.PrepPopulation(app, _environment.IsProduction());
        }
    }
}