using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using PlatformService.AsyncDataServices;
using PlatformService.Data;
using PlatformService.Dtos;
using PlatformService.Models;
using PlatformService.SyncDataServices.Http;

namespace PlatformService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PlatformsController : ControllerBase
    {
        private readonly IPlatformRepo _repository;
        private readonly IMapper _mapper;
        private readonly ICommandDataClient _commandDataClient;
        private readonly IMessageBusClient _messageBusClient;

        public PlatformsController(IPlatformRepo repository, IMapper mapper, ICommandDataClient commandDataClient,
            IMessageBusClient messageBusClient)
        {
            _repository = repository;
            _mapper = mapper;
            _commandDataClient = commandDataClient;
            _messageBusClient = messageBusClient;
        }

        [HttpGet]
        public ActionResult<IEnumerable<PlatformReadDto>> GetPlatforms()
        {
            Console.WriteLine("--->> Getting platforms...");

            var platformItems = _repository.GetAllPlatforms();

            return Ok(_mapper.Map<IEnumerable<PlatformReadDto>>(platformItems));
        }

        [HttpGet("{id}", Name = "GetPlatformById")]
        public ActionResult<PlatformReadDto> GetPlatformById(int id)
        {
            Console.WriteLine($"--->> Getting platform with id {id.ToString()}...");

            var platformItem = _repository.GetPlatformById(id);

            if (platformItem is null)
            {
                Console.WriteLine($"--->> Platform with id {id.ToString()} does not exist");
                return NotFound();
            }

            return Ok(_mapper.Map<PlatformReadDto>(platformItem));
        }

        [HttpPost]
        public async Task<ActionResult<PlatformReadDto>> CreatePlatform(PlatformCreateDto platformData)
        {
            var platformModel = _mapper.Map<Platform>(platformData);
            _repository.CreatePlatform(platformModel);
            _repository.SaveChanges();

            var result = _mapper.Map<PlatformReadDto>(platformModel);

            // Send sync message
            try
            {
                await _commandDataClient.SendPlatformToCommand(result);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"--->> Could not send synchronously: {ex.Message}");
            }
            
            // Send async message
            try
            {
                var data = _mapper.Map<PlatformPublishDto>(platformModel);
                data.Event = "Platform_Publish";
                _messageBusClient.PublishNewPlatform(data);                
            }
            catch (Exception ex)
            {
                Console.WriteLine($"--->> Could not send async message: {ex.Message}");
            }

            return CreatedAtRoute(nameof(GetPlatformById), new { id = result.Id }, result);
        }
    }
}