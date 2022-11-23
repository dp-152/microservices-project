using System;
using System.Text.Json;
using AutoMapper;
using CommandsService.Dtos;
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
            throw new NotImplementedException();
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
    }

    enum EventType
    {
        PlatformPublish,
        Undetermined
    }
}