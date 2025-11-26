using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using StargateAPI.Business.Data;
using StargateAPI.Business.Queries;
using Xunit;

namespace StargateAPI.Tests.Queries
{
    public class GetPersonByNameHandlerTests
    {
        private StargateContext CreateInMemoryContext()
        {
            var options = new DbContextOptionsBuilder<StargateContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            return new StargateContext(options);
        }

        [Fact]
        public async Task Handle_WithAstronautAndMultipleDuties_ShouldReturnCompleteDetails()
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
                CareerEndDate = null
            };
            context.AstronautDetails.Add(astronautDetail);

            var duty1 = new AstronautDuty
            {
                PersonId = person.Id,
                Rank = "Lieutenant",
                DutyTitle = "Pilot",
                DutyStartDate = new DateTime(2025, 1, 15),
                DutyEndDate = new DateTime(2025, 3, 31)
            };
            var duty2 = new AstronautDuty
            {
                PersonId = person.Id,
                Rank = "General",
                DutyTitle = "Director",
                DutyStartDate = new DateTime(2025, 4, 1),
                DutyEndDate = null
            };
            context.AstronautDuties.AddRange(duty1, duty2);
            await context.SaveChangesAsync();

            var handler = new GetPersonByNameHandler(context);
            var request = new GetPersonByName { Name = "John Doe" };

            // Act
            var result = await handler.Handle(request, CancellationToken.None);

            // Assert
            result.Success.Should().BeTrue();
            result.Person.Should().NotBeNull();
            result.Person.Name.Should().Be("John Doe");
            result.Person.CurrentRank.Should().Be("General");
            result.Person.CurrentDutyTitle.Should().Be("Director");
            result.Person.CareerStartDate.Should().Be(new DateTime(2025, 1, 15));
        }

        [Fact]
        public async Task Handle_WithRetiredAstronaut_ShouldReturnCareerEndDate()
        {
            // Arrange
            var context = CreateInMemoryContext();
            var person = new Person { Name = "Jane Doe" };
            context.People.Add(person);
            await context.SaveChangesAsync();

            var astronautDetail = new AstronautDetail
            {
                PersonId = person.Id,
                CurrentRank = "Lieutenant",
                CurrentDutyTitle = "RETIRED",
                CareerStartDate = new DateTime(2025, 1, 15),
                CareerEndDate = new DateTime(2025, 4, 14)
            };
            context.AstronautDetails.Add(astronautDetail);
            await context.SaveChangesAsync();

            var handler = new GetPersonByNameHandler(context);
            var request = new GetPersonByName { Name = "Jane Doe" };

            // Act
            var result = await handler.Handle(request, CancellationToken.None);

            // Assert
            result.Success.Should().BeTrue();
            result.Person.CareerEndDate.Should().Be(new DateTime(2025, 4, 14));
            result.Person.CurrentDutyTitle.Should().Be("RETIRED");
        }

        [Fact]
        public async Task Handle_WithExistingPersonNoAstronautDetails_ShouldReturnPersonWithoutDetails()
        {
            // Arrange
            var context = CreateInMemoryContext();
            var person = new Person { Name = "Bob Smith" };
            context.People.Add(person);
            await context.SaveChangesAsync();

            var handler = new GetPersonByNameHandler(context);
            var request = new GetPersonByName { Name = "Bob Smith" };

            // Act
            var result = await handler.Handle(request, CancellationToken.None);

            // Assert
            result.Success.Should().BeTrue();
            result.Person.Should().NotBeNull();
            result.Person.Name.Should().Be("Bob Smith");
            result.Person.CurrentRank.Should().Be(string.Empty);
            result.Person.CurrentDutyTitle.Should().Be(string.Empty);
            result.Person.CareerStartDate.Should().BeNull();
        }

        [Fact]
        public async Task Handle_WithNonExistentPerson_ShouldReturnNotFound()
        {
            // Arrange
            var context = CreateInMemoryContext();
            var handler = new GetPersonByNameHandler(context);
            var request = new GetPersonByName { Name = "NonExistent" };

            // Act
            var result = await handler.Handle(request, CancellationToken.None);

            // Assert
            result.Success.Should().BeFalse();
            result.ResponseCode.Should().Be(404);
            result.Message.Should().Contain("Person not found");
        }

        [Fact]
        public async Task Handle_WithMultiplePeople_ShouldReturnOnlyRequestedPerson()
        {
            // Arrange
            var context = CreateInMemoryContext();
            var person1 = new Person { Name = "Alice Johnson" };
            var person2 = new Person { Name = "Charlie Brown" };
            context.People.AddRange(person1, person2);
            await context.SaveChangesAsync();

            var detail1 = new AstronautDetail
            {
                PersonId = person1.Id,
                CurrentRank = "Captain",
                CurrentDutyTitle = "Commander",
                CareerStartDate = new DateTime(2025, 1, 1)
            };
            context.AstronautDetails.Add(detail1);
            await context.SaveChangesAsync();

            var handler = new GetPersonByNameHandler(context);
            var request = new GetPersonByName { Name = "Alice Johnson" };

            // Act
            var result = await handler.Handle(request, CancellationToken.None);

            // Assert
            result.Success.Should().BeTrue();
            result.Person.Name.Should().Be("Alice Johnson");
            result.Person.CurrentRank.Should().Be("Captain");
        }
    }
}
