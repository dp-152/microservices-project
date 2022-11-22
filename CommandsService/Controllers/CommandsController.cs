using System;
using System.Collections.Generic;
using AutoMapper;
using CommandsService.Data;
using CommandsService.Dtos;
using CommandsService.Models;
using Microsoft.AspNetCore.Mvc;

namespace CommandsService.Controllers
{
    [ApiController]
    [Route("api/c/Platforms/{platformId}/[controller]")]
    public class CommandsController : ControllerBase
    {
        private readonly ICommandRepo _repository;
        private readonly IMapper _mapper;

        public CommandsController(ICommandRepo repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        [HttpGet]
        public ActionResult<IEnumerable<CommandReadDto>> GetCommandsForPlatform(int platformId)
        {
            Console.WriteLine($"--->> Hit {nameof(GetCommandsForPlatform)}, platformId: {platformId.ToString()}");

            if (!_repository.PlatformExists(platformId))
                return NotFound();

            var commands = _repository.GetCommandsForPlatform(platformId);

            return Ok(_mapper.Map<IEnumerable<CommandReadDto>>(commands));
        }

        [HttpGet("{commandId}", Name = nameof(GetCommandForPlatform))]
        public ActionResult<CommandReadDto> GetCommandForPlatform(int platformId, int commandId)
        {
            Console.WriteLine(
                $"--->> Hit {nameof(GetCommandForPlatform)}, platformId: {platformId.ToString()}, commandId: {commandId.ToString()}");

            if (!_repository.PlatformExists(platformId))
                return NotFound();

            var command = _repository.GetCommand(platformId, commandId);

            if (command is null)
                return NotFound();

            return Ok(_mapper.Map<CommandReadDto>(command));
        }

        [HttpPost]
        public ActionResult CreateCommandForPlatform(int platformId, CommandCreateDto commandData)
        {
            Console.WriteLine($"--->> Hit {nameof(GetCommandsForPlatform)}, platformId: {platformId.ToString()}");

            if (!_repository.PlatformExists(platformId))
                return NotFound();

            var command = _mapper.Map<Command>(commandData);

            _repository.CreateCommand(platformId, command);
            _repository.SaveChanges();

            var result = _mapper.Map<CommandReadDto>(command);
            return CreatedAtRoute(
                nameof(GetCommandForPlatform),
                new { platformId = result.PlatformId, commandId = result.Id },
                result
            );
        }
    }
}