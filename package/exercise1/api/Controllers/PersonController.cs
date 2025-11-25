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

        [HttpGet("")]
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

        [HttpGet("{name}")]
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

        [HttpPost("")]
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
    }
}