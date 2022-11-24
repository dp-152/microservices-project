using System;
using System.Collections.Generic;
using CommandsService.Models;
using CommandsService.SyncDataServices.Grpc;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace CommandsService.Data
{
    public static class PrepDb
    {
        public static void PrepPopulation(IApplicationBuilder applicationBuilder)
        {
            using var scope = applicationBuilder.ApplicationServices.CreateScope();
            var grpcClient = scope.ServiceProvider.GetRequiredService<IPlatformDataClient>();
            var platformsCollection = grpcClient.ReturnAllPlatforms();
            SeedData(scope.ServiceProvider.GetRequiredService<ICommandRepo>(), platformsCollection);
        }

        private static void SeedData(ICommandRepo repository, IEnumerable<Platform> platforms)
        {
            Console.WriteLine("--->> Seeding new platforms...");

            foreach (var platform in platforms)
            {
                if (!repository.ExternalPlatformExists(platform.ExternalId))
                {
                    repository.CreatePlatform(platform);
                }

                repository.SaveChanges();
            }
        }
    }
}