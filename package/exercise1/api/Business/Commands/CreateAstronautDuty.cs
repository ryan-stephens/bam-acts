using MediatR;
using MediatR.Pipeline;
using Microsoft.EntityFrameworkCore;
using StargateAPI.Business.Data;
using StargateAPI.Controllers;
using System.Net;

namespace StargateAPI.Business.Commands
{
    public class CreateAstronautDuty : IRequest<CreateAstronautDutyResult>
    {
        public required string Name { get; set; }

        public required string Rank { get; set; }

        public required string DutyTitle { get; set; }

        public DateTime DutyStartDate { get; set; }
    }

    public class CreateAstronautDutyPreProcessor : IRequestPreProcessor<CreateAstronautDuty>
    {
        private readonly StargateContext _context;

        public CreateAstronautDutyPreProcessor(StargateContext context)
        {
            _context = context;
        }

        public Task Process(CreateAstronautDuty request, CancellationToken cancellationToken)
        {
            // Rule 2: Verify person exists - cannot create astronaut duty for non-existent person
            var person = _context.People.AsNoTracking().FirstOrDefault(z => z.Name == request.Name);

            if (person is null)
            {
                throw new BadHttpRequestException($"Person with name '{request.Name}' does not exist. Cannot create astronaut duty for non-existent person.");
            }

            // Rule 3: An astronaut can only have one current duty at a time (duty with no end date)
            var currentDuty = _context.AstronautDuties
                .FirstOrDefault(z => z.PersonId == person.Id && z.DutyEndDate == null);

            if (currentDuty is not null)
            {
                throw new BadHttpRequestException($"Astronaut '{request.Name}' already has a current duty. Cannot assign multiple current duties.");
            }

            // Rule 5: A new duty cannot start before the previous duty ends
            // Get the most recent duty (by start date)
            var previousDuty = _context.AstronautDuties
                .Where(z => z.PersonId == person.Id)
                .OrderByDescending(z => z.DutyStartDate)
                .FirstOrDefault();

            if (previousDuty != null && previousDuty.DutyEndDate.HasValue)
            {
                // New duty must start AFTER the previous duty end date (next day)
                if (request.DutyStartDate <= previousDuty.DutyEndDate.Value)
                {
                    throw new BadHttpRequestException($"New duty start date ({request.DutyStartDate}) must be after the previous duty end date ({previousDuty.DutyEndDate.Value}).");
                }
            }

            return Task.CompletedTask;
        }
    }

    public class CreateAstronautDutyHandler : IRequestHandler<CreateAstronautDuty, CreateAstronautDutyResult>
    {
        private readonly StargateContext _context;

        public CreateAstronautDutyHandler(StargateContext context)
        {
            _context = context;
        }
        public async Task<CreateAstronautDutyResult> Handle(CreateAstronautDuty request, CancellationToken cancellationToken)
        {
            // Use EF Core LINQ instead of raw SQL to prevent SQL injection
            var person = await _context.People
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Name == request.Name, cancellationToken);

            if (person == null)
            {
                throw new BadHttpRequestException("Person not found");
            }

            var astronautDetail = await _context.AstronautDetails
                .FirstOrDefaultAsync(ad => ad.PersonId == person.Id, cancellationToken);

            // Rule 2: Person who has not had an astronaut assignment will not have Astronaut records
            if (astronautDetail == null)
            {
                // First astronaut duty - create new AstronautDetail record
                astronautDetail = new AstronautDetail();
                astronautDetail.PersonId = person.Id;
                astronautDetail.CurrentDutyTitle = request.DutyTitle;
                astronautDetail.CurrentRank = request.Rank;
                astronautDetail.CareerStartDate = request.DutyStartDate.Date;

                // Rule 6: Person is classified as 'Retired' when Duty Title is 'RETIRED'
                // Rule 7: Person's Career End Date is one day before the Retired Duty Start Date
                if (request.DutyTitle == "RETIRED")
                {
                    astronautDetail.CareerEndDate = request.DutyStartDate.Date;
                }

                await _context.AstronautDetails.AddAsync(astronautDetail);
            }
            else
            {
                // Rule 3: Person will only ever hold one current Astronaut Duty Title, Start Date, and Rank at a time
                // Update the existing AstronautDetail with new current duty
                astronautDetail.CurrentDutyTitle = request.DutyTitle;
                astronautDetail.CurrentRank = request.Rank;

                // Rule 6 & 7: Handle retirement
                if (request.DutyTitle == "RETIRED")
                {
                    astronautDetail.CareerEndDate = request.DutyStartDate.AddDays(-1).Date;
                }
                _context.AstronautDetails.Update(astronautDetail);
            }

            // Get the most recent duty to update its end date
            var astronautDuty = await _context.AstronautDuties
                .Where(ad => ad.PersonId == person.Id)
                .OrderByDescending(ad => ad.DutyStartDate)
                .FirstOrDefaultAsync(cancellationToken);

            // Rule 5: Person's Previous Duty End Date is set to the day before the New Astronaut Duty Start Date
            if (astronautDuty != null)
            {
                astronautDuty.DutyEndDate = request.DutyStartDate.AddDays(-1).Date;
                _context.AstronautDuties.Update(astronautDuty);
            }

            // Rule 4: A Person's Current Duty will not have a Duty End Date
            var newAstronautDuty = new AstronautDuty()
            {
                PersonId = person.Id,
                Rank = request.Rank,
                DutyTitle = request.DutyTitle,
                DutyStartDate = request.DutyStartDate.Date,
                DutyEndDate = null  // Current duty has no end date
            };

            await _context.AstronautDuties.AddAsync(newAstronautDuty);

            await _context.SaveChangesAsync();

            return new CreateAstronautDutyResult()
            {
                Id = newAstronautDuty.Id
            };
        }
    }

    public class CreateAstronautDutyResult : BaseResponse
    {
        public int? Id { get; set; }
    }
}
