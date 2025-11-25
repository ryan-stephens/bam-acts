using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using StargateAPI.Business.Data;
using StargateAPI.Business.Queries;
using Xunit;

namespace StargateAPI.Tests.Queries
{
    public class GetPeopleTests
    {
        private StargateContext CreateInMemoryContext()
        {
            var options = new DbContextOptionsBuilder<StargateContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            return new StargateContext(options);
        }

        [Fact]
        public async Task Handle_WithEmptyDatabase_ShouldReturnEmptyList()
        {
            // Arrange
            var context = CreateInMemoryContext();
            var handler = new GetPeopleHandler(context);
            var query = new GetPeople();

            // Act
            var result = await handler.Handle(query, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeTrue();
            result.People.Should().BeEmpty();
        }

        [Fact]
        public async Task Handle_WithPeopleButNoAstronauts_ShouldReturnPeopleWithoutDetails()
        {
            // Arrange
            var context = CreateInMemoryContext();
            context.People.Add(new Person { Name = "Regular Person" });
            await context.SaveChangesAsync();

            var handler = new GetPeopleHandler(context);
            var query = new GetPeople();

            // Act
            var result = await handler.Handle(query, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeTrue();
            result.People.Should().HaveCount(1);
            result.People.First().Name.Should().Be("Regular Person");
            result.People.First().CurrentRank.Should().BeEmpty();
            result.People.First().CurrentDutyTitle.Should().BeEmpty();
        }

        [Fact]
        public async Task Handle_WithAstronauts_ShouldReturnPeopleWithDetails()
        {
            // Arrange
            var context = CreateInMemoryContext();
            var person = new Person { Name = "Astronaut Person" };
            context.People.Add(person);
            await context.SaveChangesAsync();

            context.AstronautDetails.Add(new AstronautDetail
            {
                PersonId = person.Id,
                CurrentRank = "CPT",
                CurrentDutyTitle = "Commander",
                CareerStartDate = new DateTime(2020, 1, 1)
            });
            await context.SaveChangesAsync();

            var handler = new GetPeopleHandler(context);
            var query = new GetPeople();

            // Act
            var result = await handler.Handle(query, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeTrue();
            result.People.Should().HaveCount(1);

            var astronaut = result.People.First();
            astronaut.Name.Should().Be("Astronaut Person");
            astronaut.CurrentRank.Should().Be("CPT");
            astronaut.CurrentDutyTitle.Should().Be("Commander");
            astronaut.CareerStartDate.Should().Be(new DateTime(2020, 1, 1));
        }

        [Fact]
        public async Task Handle_WithMultiplePeople_ShouldReturnAll()
        {
            // Arrange
            var context = CreateInMemoryContext();

            var person1 = new Person { Name = "Person 1" };
            var person2 = new Person { Name = "Person 2" };
            var person3 = new Person { Name = "Astronaut 1" };

            context.People.AddRange(person1, person2, person3);
            await context.SaveChangesAsync();

            context.AstronautDetails.Add(new AstronautDetail
            {
                PersonId = person3.Id,
                CurrentRank = "MAJ",
                CurrentDutyTitle = "Pilot",
                CareerStartDate = new DateTime(2019, 6, 1)
            });
            await context.SaveChangesAsync();

            var handler = new GetPeopleHandler(context);
            var query = new GetPeople();

            // Act
            var result = await handler.Handle(query, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeTrue();
            result.People.Should().HaveCount(3);
            result.People.Should().Contain(p => p.Name == "Person 1");
            result.People.Should().Contain(p => p.Name == "Person 2");
            result.People.Should().Contain(p => p.Name == "Astronaut 1" && p.CurrentRank == "MAJ");
        }
    }
}
