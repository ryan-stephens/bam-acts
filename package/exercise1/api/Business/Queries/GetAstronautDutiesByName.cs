using MediatR;
using Microsoft.EntityFrameworkCore;
using StargateAPI.Business.Data;
using StargateAPI.Business.Dtos;
using StargateAPI.Controllers;

namespace StargateAPI.Business.Queries
{
    public class GetAstronautDutiesByName : IRequest<GetAstronautDutiesByNameResult>
    {
        public string Name { get; set; } = string.Empty;
    }

    public class GetAstronautDutiesByNameHandler : IRequestHandler<GetAstronautDutiesByName, GetAstronautDutiesByNameResult>
    {
        private readonly StargateContext _context;

        public GetAstronautDutiesByNameHandler(StargateContext context)
        {
            _context = context;
        }

        public async Task<GetAstronautDutiesByNameResult> Handle(GetAstronautDutiesByName request, CancellationToken cancellationToken)
        {
            var result = new GetAstronautDutiesByNameResult();

            // Use EF Core LINQ instead of raw SQL to prevent SQL injection
            var personData = await _context.People
                .Where(p => p.Name == request.Name)
                .Select(p => new
                {
                    Person = p,
                    AstronautDetail = p.AstronautDetail
                })
                .FirstOrDefaultAsync(cancellationToken);

            if (personData == null)
            {
                result.Success = false;
                result.Message = "Person not found";
                result.ResponseCode = 404;
                return result;
            }

            if (personData.AstronautDetail == null)
            {
                result.Success = false;
                result.Message = "Astronaut details not found for person";
                result.ResponseCode = 404;
                return result;
            }

            // Map to DTO
            result.Person = new PersonAstronaut
            {
                PersonId = personData.Person.Id,
                Name = personData.Person.Name,
                CurrentRank = personData.AstronautDetail?.CurrentRank ?? string.Empty,
                CurrentDutyTitle = personData.AstronautDetail?.CurrentDutyTitle ?? string.Empty,
                CareerStartDate = personData.AstronautDetail?.CareerStartDate,
                CareerEndDate = personData.AstronautDetail?.CareerEndDate
            };

            // Get astronaut duties ordered by most recent
            // Use AsNoTracking and select only the properties we need to avoid circular reference
            result.AstronautDuties = await _context.AstronautDuties
                .AsNoTracking()
                .Where(ad => ad.PersonId == personData.Person.Id)
                .OrderByDescending(ad => ad.DutyStartDate)
                .Select(ad => new AstronautDuty
                {
                    Id = ad.Id,
                    PersonId = ad.PersonId,
                    Rank = ad.Rank,
                    DutyTitle = ad.DutyTitle,
                    DutyStartDate = ad.DutyStartDate,
                    DutyEndDate = ad.DutyEndDate
                })
                .ToListAsync(cancellationToken);

            return result;
        }
    }

    public class GetAstronautDutiesByNameResult : BaseResponse
    {
        public PersonAstronaut Person { get; set; }
        public List<AstronautDuty> AstronautDuties { get; set; } = new List<AstronautDuty>();
    }
}
