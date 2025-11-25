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

        [HttpGet("{name}")]
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

        [HttpPost("")]
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
    }
}