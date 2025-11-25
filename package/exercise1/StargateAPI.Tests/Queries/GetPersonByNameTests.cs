using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using StargateAPI.Business.Data;
using StargateAPI.Business.Queries;
using Xunit;

namespace StargateAPI.Tests.Queries
{
    public class GetPersonByNameTests
    {
        private StargateContext CreateInMemoryContext()
        {
            var options = new DbContextOptionsBuilder<StargateContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            return new StargateContext(options);
        }

        [Fact]
        public async Task Handle_WithNonExistentPerson_ShouldReturnNotFoundResponse()
        {
            // Arrange
            var context = CreateInMemoryContext();
            var handler = new GetPersonByNameHandler(context);
            var query = new GetPersonByName { Name = "NonExistent Person" };

            // Act
            var result = await handler.Handle(query, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeFalse();
            result.Message.Should().Be("Person not found");
            result.ResponseCode.Should().Be(404);
            result.Person.Should().BeNull();
        }

        [Fact]
        public async Task Handle_WithExistingPersonNoAstronautDetails_ShouldReturnPersonWithoutDetails()
        {
            // Arrange
            var context = CreateInMemoryContext();
            context.People.Add(new Person { Name = "Regular Person" });
            await context.SaveChangesAsync();

            var handler = new GetPersonByNameHandler(context);
            var query = new GetPersonByName { Name = "Regular Person" };

            // Act
            var result = await handler.Handle(query, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeTrue();
            result.Person.Should().NotBeNull();
            result.Person!.Name.Should().Be("Regular Person");
            result.Person.CurrentRank.Should().BeEmpty();
            result.Person.CurrentDutyTitle.Should().BeEmpty();
        }

        [Fact]
        public async Task Handle_WithAstronaut_ShouldReturnPersonWithDetails()
        {
            // Arrange
            var context = CreateInMemoryContext();
            var person = new Person { Name = "John Doe" };
            context.People.Add(person);
            await context.SaveChangesAsync();

            context.AstronautDetails.Add(new AstronautDetail
            {
                PersonId = person.Id,
                CurrentRank = "1LT",
                CurrentDutyTitle = "Commander",
                CareerStartDate = new DateTime(2020, 5, 15),
                CareerEndDate = null
            });
            await context.SaveChangesAsync();

            var handler = new GetPersonByNameHandler(context);
            var query = new GetPersonByName { Name = "John Doe" };

            // Act
            var result = await handler.Handle(query, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeTrue();
            result.Person.Should().NotBeNull();
            result.Person!.Name.Should().Be("John Doe");
            result.Person.CurrentRank.Should().Be("1LT");
            result.Person.CurrentDutyTitle.Should().Be("Commander");
            result.Person.CareerStartDate.Should().Be(new DateTime(2020, 5, 15));
            result.Person.CareerEndDate.Should().BeNull();
        }

        [Fact]
        public async Task Handle_WithRetiredAstronaut_ShouldReturnPersonWithCareerEndDate()
        {
            // Arrange
            var context = CreateInMemoryContext();
            var person = new Person { Name = "Retired Astronaut" };
            context.People.Add(person);
            await context.SaveChangesAsync();

            var careerEndDate = new DateTime(2024, 12, 31);
            context.AstronautDetails.Add(new AstronautDetail
            {
                PersonId = person.Id,
                CurrentRank = "COL",
                CurrentDutyTitle = "RETIRED",
                CareerStartDate = new DateTime(2015, 1, 1),
                CareerEndDate = careerEndDate
            });
            await context.SaveChangesAsync();

            var handler = new GetPersonByNameHandler(context);
            var query = new GetPersonByName { Name = "Retired Astronaut" };

            // Act
            var result = await handler.Handle(query, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeTrue();
            result.Person.Should().NotBeNull();
            result.Person!.CurrentDutyTitle.Should().Be("RETIRED");
            result.Person.CareerEndDate.Should().Be(careerEndDate);
        }
    }
}
