using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using StargateAPI.Business.Data;
using StargateAPI.Business.Queries;
using Xunit;

namespace StargateAPI.Tests.Queries
{
    public class GetAstronautDutiesByNameHandlerTests
    {
        private StargateContext CreateInMemoryContext()
        {
            var options = new DbContextOptionsBuilder<StargateContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            return new StargateContext(options);
        }

        [Fact]
        public async Task Handle_WithMultipleDutiesAndRetirement_ShouldReturnCorrectCareerEndDate()
        {
            // Arrange
            var context = CreateInMemoryContext();
            var person = new Person { Name = "John Doe" };
            context.People.Add(person);
            await context.SaveChangesAsync();

            var astronautDetail = new AstronautDetail
            {
                PersonId = person.Id,
                CurrentRank = "General",
                CurrentDutyTitle = "Director",
                CareerStartDate = new DateTime(2025, 1, 15),
                CareerEndDate = new DateTime(2025, 4, 14)
            };
            context.AstronautDetails.Add(astronautDetail);

            var duty1 = new AstronautDuty
            {
                PersonId = person.Id,
                Rank = "Lieutenant",
                DutyTitle = "Pilot",
                DutyStartDate = new DateTime(2025, 1, 15),
                DutyEndDate = new DateTime(2025, 4, 14)
            };
            var duty2 = new AstronautDuty
            {
                PersonId = person.Id,
                Rank = "Lieutenant",
                DutyTitle = "RETIRED",
                DutyStartDate = new DateTime(2025, 4, 15),
                DutyEndDate = new DateTime(2025, 4, 15)
            };
            var duty3 = new AstronautDuty
            {
                PersonId = person.Id,
                Rank = "General",
                DutyTitle = "Director",
                DutyStartDate = new DateTime(2025, 5, 20),
                DutyEndDate = null
            };
            context.AstronautDuties.AddRange(duty1, duty2, duty3);
            await context.SaveChangesAsync();

            var handler = new GetAstronautDutiesByNameHandler(context);
            var request = new GetAstronautDutiesByName { Name = "John Doe" };

            // Act
            var result = await handler.Handle(request, CancellationToken.None);

            // Assert
            result.Success.Should().BeTrue();
            result.Person.CareerEndDate.Should().Be(new DateTime(2025, 4, 14));
            result.AstronautDuties.Should().HaveCount(3);
            result.AstronautDuties[0].DutyTitle.Should().Be("Director"); // Most recent first
            result.AstronautDuties[0].DutyEndDate.Should().BeNull();
        }

        [Fact]
        public async Task Handle_DutiesShouldBeOrderedByStartDateDescending()
        {
            // Arrange
            var context = CreateInMemoryContext();
            var person = new Person { Name = "Jane Doe" };
            context.People.Add(person);
            await context.SaveChangesAsync();

            var astronautDetail = new AstronautDetail
            {
                PersonId = person.Id,
                CurrentRank = "Captain",
                CurrentDutyTitle = "Commander",
                CareerStartDate = new DateTime(2025, 1, 10)
            };
            context.AstronautDetails.Add(astronautDetail);

            var duty1 = new AstronautDuty
            {
                PersonId = person.Id,
                Rank = "Lieutenant",
                DutyTitle = "Pilot",
                DutyStartDate = new DateTime(2025, 1, 10),
                DutyEndDate = new DateTime(2025, 2, 28)
            };
            var duty2 = new AstronautDuty
            {
                PersonId = person.Id,
                Rank = "Captain",
                DutyTitle = "Commander",
                DutyStartDate = new DateTime(2025, 3, 1),
                DutyEndDate = null
            };
            context.AstronautDuties.AddRange(duty1, duty2);
            await context.SaveChangesAsync();

            var handler = new GetAstronautDutiesByNameHandler(context);
            var request = new GetAstronautDutiesByName { Name = "Jane Doe" };

            // Act
            var result = await handler.Handle(request, CancellationToken.None);

            // Assert
            result.Success.Should().BeTrue();
            result.AstronautDuties.Should().HaveCount(2);
            result.AstronautDuties[0].DutyStartDate.Should().Be(new DateTime(2025, 3, 1)); // Most recent
            result.AstronautDuties[1].DutyStartDate.Should().Be(new DateTime(2025, 1, 10));
        }

        [Fact]
        public async Task Handle_ShouldNotIncludePersonNavigationProperty()
        {
            // Arrange
            var context = CreateInMemoryContext();
            var person = new Person { Name = "Bob Smith" };
            context.People.Add(person);
            await context.SaveChangesAsync();

            var astronautDetail = new AstronautDetail
            {
                PersonId = person.Id,
                CurrentRank = "Lieutenant",
                CurrentDutyTitle = "Pilot",
                CareerStartDate = new DateTime(2025, 1, 15)
            };
            context.AstronautDetails.Add(astronautDetail);

            var duty = new AstronautDuty
            {
                PersonId = person.Id,
                Rank = "Lieutenant",
                DutyTitle = "Pilot",
                DutyStartDate = new DateTime(2025, 1, 15),
                DutyEndDate = null
            };
            context.AstronautDuties.Add(duty);
            await context.SaveChangesAsync();

            var handler = new GetAstronautDutiesByNameHandler(context);
            var request = new GetAstronautDutiesByName { Name = "Bob Smith" };

            // Act
            var result = await handler.Handle(request, CancellationToken.None);

            // Assert
            result.Success.Should().BeTrue();
            result.AstronautDuties.Should().HaveCount(1);
            // Verify DTO type is AstronautDutyDto (which doesn't have Person property)
            var dutyType = result.AstronautDuties[0].GetType().Name;
            dutyType.Should().Be("AstronautDutyDto");
        }

        [Fact]
        public async Task Handle_WithPersonButNoAstronautDetails_ShouldReturnNotFound()
        {
            // Arrange
            var context = CreateInMemoryContext();
            var person = new Person { Name = "Charlie Brown" };
            context.People.Add(person);
            await context.SaveChangesAsync();

            var handler = new GetAstronautDutiesByNameHandler(context);
            var request = new GetAstronautDutiesByName { Name = "Charlie Brown" };

            // Act
            var result = await handler.Handle(request, CancellationToken.None);

            // Assert
            result.Success.Should().BeFalse();
            result.ResponseCode.Should().Be(404);
            result.Message.Should().Contain("Astronaut details not found");
        }

        [Fact]
        public async Task Handle_WithNonExistentPerson_ShouldReturnNotFound()
        {
            // Arrange
            var context = CreateInMemoryContext();
            var handler = new GetAstronautDutiesByNameHandler(context);
            var request = new GetAstronautDutiesByName { Name = "NonExistent" };

            // Act
            var result = await handler.Handle(request, CancellationToken.None);

            // Assert
            result.Success.Should().BeFalse();
            result.ResponseCode.Should().Be(404);
            result.Message.Should().Contain("Person not found");
        }
    }
}
