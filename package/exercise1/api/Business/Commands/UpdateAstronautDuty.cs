using MediatR;
using MediatR.Pipeline;
using Microsoft.EntityFrameworkCore;
using StargateAPI.Business.Data;
using StargateAPI.Controllers;
using System.Net;

namespace StargateAPI.Business.Commands
{
    public class UpdateAstronautDuty : IRequest<BaseResponse>
    {
        public int Id { get; set; }

        public required string Rank { get; set; }

        public required string DutyTitle { get; set; }

        public DateTime DutyStartDate { get; set; }

        public DateTime? DutyEndDate { get; set; }
    }

    public class UpdateAstronautDutyPreProcessor : IRequestPreProcessor<UpdateAstronautDuty>
    {
        private readonly StargateContext _context;

        public UpdateAstronautDutyPreProcessor(StargateContext context)
        {
            _context = context;
        }

        public Task Process(UpdateAstronautDuty request, CancellationToken cancellationToken)
        {
            // Verify duty exists
            var duty = _context.AstronautDuties.AsNoTracking().FirstOrDefault(z => z.Id == request.Id);

            if (duty is null)
            {
                throw new BadHttpRequestException($"Astronaut duty with ID '{request.Id}' does not exist.");
            }

            // Validate dates
            if (request.DutyEndDate.HasValue && request.DutyStartDate >= request.DutyEndDate.Value)
            {
                throw new BadHttpRequestException("Duty start date must be before duty end date.");
            }

            return Task.CompletedTask;
        }
    }

    public class UpdateAstronautDutyHandler : IRequestHandler<UpdateAstronautDuty, BaseResponse>
    {
        private readonly StargateContext _context;

        public UpdateAstronautDutyHandler(StargateContext context)
        {
            _context = context;
        }

        public async Task<BaseResponse> Handle(UpdateAstronautDuty request, CancellationToken cancellationToken)
        {
            var duty = await _context.AstronautDuties
                .Include(d => d.Person)
                .FirstOrDefaultAsync(d => d.Id == request.Id, cancellationToken);

            if (duty == null)
            {
                throw new BadHttpRequestException("Astronaut duty not found");
            }

            // Update duty fields
            duty.Rank = request.Rank;
            duty.DutyTitle = request.DutyTitle;
            duty.DutyStartDate = request.DutyStartDate.Date;
            duty.DutyEndDate = request.DutyEndDate?.Date;

            // If this is the current duty (no end date), update AstronautDetail
            if (!request.DutyEndDate.HasValue)
            {
                var astronautDetail = await _context.AstronautDetails
                    .FirstOrDefaultAsync(ad => ad.PersonId == duty.PersonId, cancellationToken);

                if (astronautDetail != null)
                {
                    astronautDetail.CurrentDutyTitle = request.DutyTitle;
                    astronautDetail.CurrentRank = request.Rank;

                    // Handle retirement status
                    if (request.DutyTitle == "RETIRED")
                    {
                        astronautDetail.CareerEndDate = request.DutyStartDate.AddDays(-1).Date;
                    }
                    else if (astronautDetail.CareerEndDate.HasValue)
                    {
                        // If changing from RETIRED to something else, clear end date
                        astronautDetail.CareerEndDate = null;
                    }

                    _context.AstronautDetails.Update(astronautDetail);
                }
            }

            _context.AstronautDuties.Update(duty);
            await _context.SaveChangesAsync(cancellationToken);

            return new BaseResponse()
            {
                Success = true,
                Message = "Astronaut duty updated successfully",
                ResponseCode = (int)HttpStatusCode.OK
            };
        }
    }
}
