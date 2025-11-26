using MediatR;
using Microsoft.AspNetCore.Mvc;
using StargateAPI.Business.Commands;
using StargateAPI.Business.Queries;
using StargateAPI.Business.Services;
using System.Net;

namespace StargateAPI.Controllers
{

    [ApiController]
    [Route("[controller]")]
    public class PersonController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly IProcessLogService _logService;

        public PersonController(IMediator mediator, IProcessLogService logService)
        {
            _mediator = mediator;
            _logService = logService;
        }

        [HttpGet("all", Name = "GetAllPeople")]
        [ProducesResponseType(typeof(GetPeopleResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetPeople()
        {
            try
            {
                var result = await _mediator.Send(new GetPeople()
                {

                });

                await _logService.LogSuccessAsync(
                    "Successfully retrieved all people",
                    HttpContext.Request.Path,
                    HttpContext.Request.Method);

                return this.GetResponse(result);
            }
            catch (Exception ex)
            {
                await _logService.LogExceptionAsync(
                    ex,
                    "Failed to retrieve people",
                    HttpContext.Request.Path,
                    HttpContext.Request.Method);

                return this.GetResponse(new BaseResponse()
                {
                    Message = ex.Message,
                    Success = false,
                    ResponseCode = (int)HttpStatusCode.InternalServerError
                });
            }
        }

        [HttpGet("{name}", Name = "GetPersonByName")]
        [ProducesResponseType(typeof(GetPersonByNameResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetPersonByName(string name)
        {
            try
            {
                var result = await _mediator.Send(new GetPersonByName()
                {
                    Name = name
                });

                await _logService.LogSuccessAsync(
                    $"Successfully retrieved person: {name}",
                    HttpContext.Request.Path,
                    HttpContext.Request.Method);

                return this.GetResponse(result);
            }
            catch (Exception ex)
            {
                await _logService.LogExceptionAsync(
                    ex,
                    $"Failed to retrieve person: {name}",
                    HttpContext.Request.Path,
                    HttpContext.Request.Method);

                return this.GetResponse(new BaseResponse()
                {
                    Message = ex.Message,
                    Success = false,
                    ResponseCode = (int)HttpStatusCode.InternalServerError
                });
            }
        }

        [HttpPost("", Name = "CreatePerson")]
        [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CreatePerson([FromBody] string name)
        {
            try
            {
                var result = await _mediator.Send(new CreatePerson()
                {
                    Name = name
                });

                await _logService.LogSuccessAsync(
                    $"Successfully created person: {name}",
                    HttpContext.Request.Path,
                    HttpContext.Request.Method);

                return this.GetResponse(result);
            }
            catch (Exception ex)
            {
                await _logService.LogExceptionAsync(
                    ex,
                    $"Failed to create person: {name}",
                    HttpContext.Request.Path,
                    HttpContext.Request.Method);

                return this.GetResponse(new BaseResponse()
                {
                    Message = ex.Message,
                    Success = false,
                    ResponseCode = (int)HttpStatusCode.InternalServerError
                });
            }

        }

        [HttpPut("{currentName}", Name = "UpdatePerson")]
        [ProducesResponseType(typeof(UpdatePersonResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdatePerson(string currentName, [FromBody] string newName)
        {
            try
            {
                var result = await _mediator.Send(new UpdatePerson()
                {
                    CurrentName = currentName,
                    NewName = newName
                });

                await _logService.LogSuccessAsync(
                    $"Successfully updated person from '{currentName}' to '{newName}'",
                    HttpContext.Request.Path,
                    HttpContext.Request.Method);

                return this.GetResponse(result);
            }
            catch (Exception ex)
            {
                await _logService.LogExceptionAsync(
                    ex,
                    $"Failed to update person: {currentName}",
                    HttpContext.Request.Path,
                    HttpContext.Request.Method);

                return this.GetResponse(new BaseResponse()
                {
                    Message = ex.Message,
                    Success = false,
                    ResponseCode = (int)HttpStatusCode.InternalServerError
                });
            }
        }
    }
}