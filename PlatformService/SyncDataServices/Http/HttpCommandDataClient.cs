using System;
using System.Net.Http;
using System.Net.Mime;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using PlatformService.Dtos;

namespace PlatformService.SyncDataServices.Http
{
    public class HttpCommandDataClient : ICommandDataClient
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;

        public HttpCommandDataClient(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _configuration = configuration;
        }
        public async Task SendPlatformToCommand(PlatformReadDto plat)
        {
            var httpContent = new StringContent(
                JsonSerializer.Serialize(plat),
                Encoding.UTF8,
                MediaTypeNames.Application.Json
                );
            var response = await _httpClient.PostAsync(new Uri(new Uri(_configuration["CommandService"]), "/api/c/Platforms"), httpContent);

            if (response.IsSuccessStatusCode)
            {
                Console.WriteLine("--->> Sync POST to Command service was successful");
                return;
            }
            Console.WriteLine("--->> Failed to POST to Command service");
        }
    }
}