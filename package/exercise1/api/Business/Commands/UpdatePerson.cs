using MediatR;
using MediatR.Pipeline;
using Microsoft.EntityFrameworkCore;
using StargateAPI.Business.Data;
using StargateAPI.Controllers;

namespace StargateAPI.Business.Commands
{
    public class UpdatePerson : IRequest<UpdatePersonResult>
    {
        public required string CurrentName { get; set; } = string.Empty;
        public required string NewName { get; set; } = string.Empty;
    }

    public class UpdatePersonPreProcessor : IRequestPreProcessor<UpdatePerson>
    {
        private readonly StargateContext _context;
        public UpdatePersonPreProcessor(StargateContext context)
        {
            _context = context;
        }
        public async Task Process(UpdatePerson request, CancellationToken cancellationToken)
        {
            // Validate that new name is not empty or whitespace
            if (string.IsNullOrWhiteSpace(request.NewName))
            {
                throw new BadHttpRequestException("New person name cannot be empty or whitespace.");
            }

            // Check that person with current name exists
            var person = await _context.People.AsNoTracking()
                .FirstOrDefaultAsync(z => z.Name == request.CurrentName, cancellationToken);

            if (person is null)
            {
                throw new BadHttpRequestException($"Person with name '{request.CurrentName}' not found.");
            }

            // Rule 1: A Person is uniquely identified by their Name
            // Check that new name doesn't already exist (unless it's the same as current name)
            if (request.CurrentName != request.NewName)
            {
                var existingPerson = await _context.People.AsNoTracking()
                    .FirstOrDefaultAsync(z => z.Name == request.NewName, cancellationToken);

                if (existingPerson is not null)
                {
                    throw new BadHttpRequestException($"Person with name '{request.NewName}' already exists. Person names must be unique.");
                }
            }
        }
    }

    public class UpdatePersonHandler : IRequestHandler<UpdatePerson, UpdatePersonResult>
    {
        private readonly StargateContext _context;

        public UpdatePersonHandler(StargateContext context)
        {
            _context = context;
        }

        public async Task<UpdatePersonResult> Handle(UpdatePerson request, CancellationToken cancellationToken)
        {
            var person = await _context.People
                .FirstOrDefaultAsync(z => z.Name == request.CurrentName, cancellationToken);

            if (person is null)
            {
                throw new BadHttpRequestException($"Person with name '{request.CurrentName}' not found.");
            }

            person.Name = request.NewName;

            await _context.SaveChangesAsync(cancellationToken);

            return new UpdatePersonResult()
            {
                Id = person.Id,
                NewName = person.Name
            };
        }
    }

    public class UpdatePersonResult : BaseResponse
    {
        public int Id { get; set; }
        public string NewName { get; set; } = string.Empty;
    }
}
