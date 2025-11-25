using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using StargateAPI.Business.Commands;
using StargateAPI.Business.Data;
using Xunit;

namespace StargateAPI.Tests.Commands
{
    public class CreateAstronautDutyPreProcessorTests
    {
        private StargateContext CreateInMemoryContext()
        {
            var options = new DbContextOptionsBuilder<StargateContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            return new StargateContext(options);
        }

        [Fact]
        public async Task Process_WithNonExistentPerson_ShouldThrowBadHttpRequestException()
        {
            // Arrange
            var context = CreateInMemoryContext();
            var preprocessor = new CreateAstronautDutyPreProcessor(context);
            var request = new CreateAstronautDuty
            {
                Name = "NonExistent Person",
                Rank = "CPT",
                DutyTitle = "Pilot",
                DutyStartDate = DateTime.Now
            };

            // Act
            Func<Task> act = async () => await preprocessor.Process(request, CancellationToken.None);

            // Assert
            // Rule 2: You cannot create an astronaut record for a person that does not exist
            await act.Should().ThrowAsync<BadHttpRequestException>()
                .WithMessage("Person with name 'NonExistent Person' does not exist. Cannot create astronaut duty for non-existent person.");
        }

        [Fact]
        public async Task Process_WithExistingCurrentDuty_ShouldThrowBadHttpRequestException()
        {
            // Arrange
            var context = CreateInMemoryContext();
            var person = new Person { Name = "Test Astronaut" };
            context.People.Add(person);
            await context.SaveChangesAsync();

            // Add astronaut detail
            context.AstronautDetails.Add(new AstronautDetail
            {
                PersonId = person.Id,
                CurrentRank = "CPT",
                CurrentDutyTitle = "Pilot",
                CareerStartDate = DateTime.Now.AddYears(-1)
            });

            // Add existing current duty (no end date)
            context.AstronautDuties.Add(new AstronautDuty
            {
                PersonId = person.Id,
                Rank = "CPT",
                DutyTitle = "Pilot",
                DutyStartDate = DateTime.Now.AddMonths(-6),
                DutyEndDate = null
            });
            await context.SaveChangesAsync();

            var preprocessor = new CreateAstronautDutyPreProcessor(context);
            var request = new CreateAstronautDuty
            {
                Name = "Test Astronaut",
                Rank = "MAJ",
                DutyTitle = "Commander",
                DutyStartDate = DateTime.Now
            };

            // Act
            Func<Task> act = async () => await preprocessor.Process(request, CancellationToken.None);

            // Assert
            // Rule 3: An astronaut can only have one current duty at a time
            await act.Should().ThrowAsync<BadHttpRequestException>()
                .WithMessage("Astronaut 'Test Astronaut' already has a current duty. Cannot assign multiple current duties.");
        }

        [Fact]
        public async Task Process_NewDutyBeforePreviousDutyEnds_ShouldThrowBadHttpRequestException()
        {
            // Arrange
            var context = CreateInMemoryContext();
            var person = new Person { Name = "Test Astronaut" };
            context.People.Add(person);
            await context.SaveChangesAsync();

            context.AstronautDetails.Add(new AstronautDetail
            {
                PersonId = person.Id,
                CurrentRank = "CPT",
                CurrentDutyTitle = "Pilot",
                CareerStartDate = DateTime.Now.AddYears(-1)
            });

            // Add previous duty that ends in the future
            var previousDutyEndDate = new DateTime(2025, 12, 31);
            context.AstronautDuties.Add(new AstronautDuty
            {
                PersonId = person.Id,
                Rank = "CPT",
                DutyTitle = "Pilot",
                DutyStartDate = DateTime.Now.AddMonths(-6),
                DutyEndDate = previousDutyEndDate
            });
            await context.SaveChangesAsync();

            var preprocessor = new CreateAstronautDutyPreProcessor(context);
            var request = new CreateAstronautDuty
            {
                Name = "Test Astronaut",
                Rank = "MAJ",
                DutyTitle = "Commander",
                DutyStartDate = new DateTime(2025, 12, 15) // Before previous duty ends
            };

            // Act
            Func<Task> act = async () => await preprocessor.Process(request, CancellationToken.None);

            // Assert
            // Rule 5: A new duty cannot start before the previous duty ends
            await act.Should().ThrowAsync<BadHttpRequestException>()
                .WithMessage("New duty start date (12/15/2025 12:00:00 AM) must be after the previous duty end date (12/31/2025 12:00:00 AM).");
        }

        [Fact]
        public async Task Process_NewDutyOnSameDayAsPreviousDutyEnds_ShouldThrowBadHttpRequestException()
        {
            // Arrange
            var context = CreateInMemoryContext();
            var person = new Person { Name = "Test Astronaut" };
            context.People.Add(person);
            await context.SaveChangesAsync();

            context.AstronautDetails.Add(new AstronautDetail
            {
                PersonId = person.Id,
                CurrentRank = "CPT",
                CurrentDutyTitle = "Pilot",
                CareerStartDate = DateTime.Now.AddYears(-1)
            });

            var previousDutyEndDate = new DateTime(2025, 12, 31);
            context.AstronautDuties.Add(new AstronautDuty
            {
                PersonId = person.Id,
                Rank = "CPT",
                DutyTitle = "Pilot",
                DutyStartDate = DateTime.Now.AddMonths(-6),
                DutyEndDate = previousDutyEndDate
            });
            await context.SaveChangesAsync();

            var preprocessor = new CreateAstronautDutyPreProcessor(context);
            var request = new CreateAstronautDuty
            {
                Name = "Test Astronaut",
                Rank = "MAJ",
                DutyTitle = "Commander",
                DutyStartDate = previousDutyEndDate // Same day
            };

            // Act
            Func<Task> act = async () => await preprocessor.Process(request, CancellationToken.None);

            // Assert
            // Rule 5: New duty must start the day after previous duty ends
            await act.Should().ThrowAsync<BadHttpRequestException>();
        }

        [Fact]
        public async Task Process_FirstDutyForPerson_ShouldSucceed()
        {
            // Arrange
            var context = CreateInMemoryContext();
            var person = new Person { Name = "New Astronaut" };
            context.People.Add(person);
            await context.SaveChangesAsync();

            var preprocessor = new CreateAstronautDutyPreProcessor(context);
            var request = new CreateAstronautDuty
            {
                Name = "New Astronaut",
                Rank = "1LT",
                DutyTitle = "Pilot",
                DutyStartDate = DateTime.Now
            };

            // Act
            Func<Task> act = async () => await preprocessor.Process(request, CancellationToken.None);

            // Assert
            await act.Should().NotThrowAsync();
        }

        [Fact]
        public async Task Process_NewDutyAfterPreviousDutyEnds_ShouldSucceed()
        {
            // Arrange
            var context = CreateInMemoryContext();
            var person = new Person { Name = "Test Astronaut" };
            context.People.Add(person);
            await context.SaveChangesAsync();

            context.AstronautDetails.Add(new AstronautDetail
            {
                PersonId = person.Id,
                CurrentRank = "CPT",
                CurrentDutyTitle = "Pilot",
                CareerStartDate = DateTime.Now.AddYears(-1)
            });

            var previousDutyEndDate = new DateTime(2025, 5, 31);
            context.AstronautDuties.Add(new AstronautDuty
            {
                PersonId = person.Id,
                Rank = "CPT",
                DutyTitle = "Pilot",
                DutyStartDate = DateTime.Now.AddMonths(-6),
                DutyEndDate = previousDutyEndDate
            });
            await context.SaveChangesAsync();

            var preprocessor = new CreateAstronautDutyPreProcessor(context);
            var request = new CreateAstronautDuty
            {
                Name = "Test Astronaut",
                Rank = "MAJ",
                DutyTitle = "Commander",
                DutyStartDate = new DateTime(2025, 6, 1) // Day after previous duty ends
            };

            // Act
            Func<Task> act = async () => await preprocessor.Process(request, CancellationToken.None);

            // Assert
            await act.Should().NotThrowAsync();
        }
    }
}
