using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using StargateAPI.Business.Data;
using StargateAPI.Business.Queries;
using Xunit;

namespace StargateAPI.Tests.Queries
{
    public class GetPeopleHandlerTests
    {
        private StargateContext CreateInMemoryContext()
        {
            var options = new DbContextOptionsBuilder<StargateContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            return new StargateContext(options);
        }

        [Fact]
        public async Task Handle_WithMultiplePeopleAndAstronauts_ShouldReturnAllWithDetails()
        {
            // Arrange
            var context = CreateInMemoryContext();
            var person1 = new Person { Name = "John Doe" };
            var person2 = new Person { Name = "Jane Doe" };
            var person3 = new Person { Name = "Bob Smith" };
            context.People.AddRange(person1, person2, person3);
            await context.SaveChangesAsync();

            var detail1 = new AstronautDetail
            {
                PersonId = person1.Id,
                CurrentRank = "Lieutenant",
                CurrentDutyTitle = "Pilot",
                CareerStartDate = new DateTime(2025, 1, 15)
            };
            var detail2 = new AstronautDetail
            {
                PersonId = person2.Id,
                CurrentRank = "Captain",
                CurrentDutyTitle = "Commander",
                CareerStartDate = new DateTime(2025, 2, 1)
            };
            context.AstronautDetails.AddRange(detail1, detail2);
            await context.SaveChangesAsync();

            var handler = new GetPeopleHandler(context);
            var request = new GetPeople();

            // Act
            var result = await handler.Handle(request, CancellationToken.None);

            // Assert
            result.Success.Should().BeTrue();
            result.People.Should().HaveCount(3);
            result.People[0].Name.Should().Be("John Doe");
            result.People[0].CurrentRank.Should().Be("Lieutenant");
            result.People[1].Name.Should().Be("Jane Doe");
            result.People[1].CurrentRank.Should().Be("Captain");
            result.People[2].Name.Should().Be("Bob Smith");
            result.People[2].CurrentRank.Should().Be(string.Empty);
        }

        [Fact]
        public async Task Handle_WithEmptyDatabase_ShouldReturnEmptyList()
        {
            // Arrange
            var context = CreateInMemoryContext();
            var handler = new GetPeopleHandler(context);
            var request = new GetPeople();

            // Act
            var result = await handler.Handle(request, CancellationToken.None);

            // Assert
            result.Success.Should().BeTrue();
            result.People.Should().BeEmpty();
        }

        [Fact]
        public async Task Handle_WithPeopleButNoAstronauts_ShouldReturnPeopleWithoutDetails()
        {
            // Arrange
            var context = CreateInMemoryContext();
            var person1 = new Person { Name = "Alice Johnson" };
            var person2 = new Person { Name = "Charlie Brown" };
            context.People.AddRange(person1, person2);
            await context.SaveChangesAsync();

            var handler = new GetPeopleHandler(context);
            var request = new GetPeople();

            // Act
            var result = await handler.Handle(request, CancellationToken.None);

            // Assert
            result.Success.Should().BeTrue();
            result.People.Should().HaveCount(2);
            result.People[0].CurrentRank.Should().Be(string.Empty);
            result.People[0].CurrentDutyTitle.Should().Be(string.Empty);
            result.People[0].CareerStartDate.Should().BeNull();
        }

        [Fact]
        public async Task Handle_WithAstronauts_ShouldReturnPeopleWithDetails()
        {
            // Arrange
            var context = CreateInMemoryContext();
            var person = new Person { Name = "David Lee" };
            context.People.Add(person);
            await context.SaveChangesAsync();

            var detail = new AstronautDetail
            {
                PersonId = person.Id,
                CurrentRank = "General",
                CurrentDutyTitle = "Director",
                CareerStartDate = new DateTime(2025, 1, 10),
                CareerEndDate = null
            };
            context.AstronautDetails.Add(detail);
            await context.SaveChangesAsync();

            var handler = new GetPeopleHandler(context);
            var request = new GetPeople();

            // Act
            var result = await handler.Handle(request, CancellationToken.None);

            // Assert
            result.Success.Should().BeTrue();
            result.People.Should().HaveCount(1);
            result.People[0].CurrentRank.Should().Be("General");
            result.People[0].CurrentDutyTitle.Should().Be("Director");
            result.People[0].CareerStartDate.Should().Be(new DateTime(2025, 1, 10));
        }

        [Fact]
        public async Task Handle_WithRetiredAstronaut_ShouldIncludeCareerEndDate()
        {
            // Arrange
            var context = CreateInMemoryContext();
            var person = new Person { Name = "Eva Martinez" };
            context.People.Add(person);
            await context.SaveChangesAsync();

            var detail = new AstronautDetail
            {
                PersonId = person.Id,
                CurrentRank = "Lieutenant",
                CurrentDutyTitle = "RETIRED",
                CareerStartDate = new DateTime(2025, 1, 15),
                CareerEndDate = new DateTime(2025, 4, 14)
            };
            context.AstronautDetails.Add(detail);
            await context.SaveChangesAsync();

            var handler = new GetPeopleHandler(context);
            var request = new GetPeople();

            // Act
            var result = await handler.Handle(request, CancellationToken.None);

            // Assert
            result.Success.Should().BeTrue();
            result.People.Should().HaveCount(1);
            result.People[0].CareerEndDate.Should().Be(new DateTime(2025, 4, 14));
        }
    }
}
