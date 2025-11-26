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
    public class AstronautDutyController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly IProcessLogService _logService;

        public AstronautDutyController(IMediator mediator, IProcessLogService logService)
        {
            _mediator = mediator;
            _logService = logService;
        }

        [HttpGet("{name}", Name = "GetAstronautDutiesByName")]
        [ProducesResponseType(typeof(GetAstronautDutiesByNameResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAstronautDutiesByName(string name)
        {
            try
            {
                var result = await _mediator.Send(new GetAstronautDutiesByName()
                {
                    Name = name
                });

                await _logService.LogSuccessAsync(
                    $"Successfully retrieved astronaut duties for: {name}",
                    HttpContext.Request.Path,
                    HttpContext.Request.Method);

                return this.GetResponse(result);
            }
            catch (Exception ex)
            {
                await _logService.LogExceptionAsync(
                    ex,
                    $"Failed to retrieve astronaut duties for: {name}",
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

        [HttpPost("", Name = "CreateAstronautDuty")]
        [ProducesResponseType(typeof(CreateAstronautDutyResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CreateAstronautDuty([FromBody] CreateAstronautDuty request)
        {
            try
            {
                var result = await _mediator.Send(request);

                await _logService.LogSuccessAsync(
                    $"Successfully created astronaut duty for: {request.Name}",
                    HttpContext.Request.Path,
                    HttpContext.Request.Method);

                return this.GetResponse(result);
            }
            catch (Exception ex)
            {
                await _logService.LogExceptionAsync(
                    ex,
                    $"Failed to create astronaut duty for: {request.Name}",
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

        [HttpPut("{id}", Name = "UpdateAstronautDuty")]
        [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdateAstronautDuty(int id, [FromBody] UpdateAstronautDuty request)
        {
            try
            {
                request.Id = id;
                var result = await _mediator.Send(request);

                await _logService.LogSuccessAsync(
                    $"Successfully updated astronaut duty ID: {id}",
                    HttpContext.Request.Path,
                    HttpContext.Request.Method);

                return this.GetResponse(result);
            }
            catch (Exception ex)
            {
                await _logService.LogExceptionAsync(
                    ex,
                    $"Failed to update astronaut duty ID: {id}",
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