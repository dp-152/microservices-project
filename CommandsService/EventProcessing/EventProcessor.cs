using System;
using System.Text.Json;
using AutoMapper;
using CommandsService.Data;
using CommandsService.Dtos;
using CommandsService.Models;
using Microsoft.Extensions.DependencyInjection;

namespace CommandsService.EventProcessing
{
    public class EventProcessor : IEventProcessor
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IMapper _mapper;

        public EventProcessor(IServiceScopeFactory scopeFactory, IMapper mapper)
        {
            _scopeFactory = scopeFactory;
            _mapper = mapper;
        }

        public void ProcessEvent(string message)
        {
            var eventType = DetermineEvent(message);

            switch (eventType)
            {
                case EventType.PlatformPublish:
                    AddPlatform(message);
                    break;
            }
        }

        private EventType DetermineEvent(string notificationMessage)
        {
            Console.WriteLine("--->> Determining event...");
            var eventType = JsonSerializer.Deserialize<GenericEventDto>(notificationMessage);

            switch (eventType.Event)
            {
                case "Platform_Publish":
                    Console.WriteLine("--->> Platform Publish Event detected");
                    return EventType.PlatformPublish;
                
                default:
                    Console.WriteLine("--->> Could not determine event type");
                    return EventType.Undetermined;
            }
        }

        private void AddPlatform(string platformPublishMessage)
        {
            using var scope = _scopeFactory.CreateScope();
            var repository = scope.ServiceProvider.GetRequiredService<ICommandRepo>();
            var data = JsonSerializer.Deserialize<PlatformPublishedDto>(platformPublishMessage);

            try
            {
                var platform = _mapper.Map<Platform>(data);
                if (repository.PlatformExists(platform.ExternalId))
                    return;

                repository.CreatePlatform(platform);
                repository.SaveChanges();

                Console.WriteLine("--->> Platform saved to database.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"--->> could not add Platform to DB: {ex.Message}");
            }
        }
    }

    enum EventType
    {
        PlatformPublish,
        Undetermined
    }
}