using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using StargateAPI.Business.Commands;
using StargateAPI.Business.Data;
using Xunit;

namespace StargateAPI.Tests.Commands
{
    public class UpdatePersonHandlerTests
    {
        private StargateContext CreateInMemoryContext()
        {
            var options = new DbContextOptionsBuilder<StargateContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            return new StargateContext(options);
        }

        [Fact]
        public async Task Handle_WithValidUpdate_ShouldUpdatePersonName()
        {
            // Arrange
            var context = CreateInMemoryContext();
            var person = new Person { Name = "John Doe" };
            context.People.Add(person);
            await context.SaveChangesAsync();

            var handler = new UpdatePersonHandler(context);
            var request = new UpdatePerson
            {
                CurrentName = "John Doe",
                NewName = "Jane Smith"
            };

            // Act
            var result = await handler.Handle(request, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeTrue();
            result.Id.Should().Be(person.Id);
            result.NewName.Should().Be("Jane Smith");

            var updatedPerson = await context.People.FirstOrDefaultAsync(p => p.Id == person.Id);
            updatedPerson.Should().NotBeNull();
            updatedPerson.Name.Should().Be("Jane Smith");
        }

        [Fact]
        public async Task Handle_WithNonExistentPerson_ShouldThrowBadHttpRequestException()
        {
            // Arrange
            var context = CreateInMemoryContext();
            var handler = new UpdatePersonHandler(context);
            var request = new UpdatePerson
            {
                CurrentName = "NonExistent Person",
                NewName = "New Name"
            };

            // Act
            Func<Task> act = async () => await handler.Handle(request, CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<BadHttpRequestException>()
                .WithMessage("Person with name 'NonExistent Person' not found.");
        }

        [Fact]
        public async Task Handle_ShouldPersistChangesToDatabase()
        {
            // Arrange
            var context = CreateInMemoryContext();
            var person = new Person { Name = "Alice Johnson" };
            context.People.Add(person);
            await context.SaveChangesAsync();
            var originalId = person.Id;

            var handler = new UpdatePersonHandler(context);
            var request = new UpdatePerson
            {
                CurrentName = "Alice Johnson",
                NewName = "Alice Williams"
            };

            // Act
            var result = await handler.Handle(request, CancellationToken.None);

            // Assert
            result.Success.Should().BeTrue();
            result.Id.Should().Be(originalId);

            // Verify persistence by querying the database
            var updatedPerson = await context.People.FirstOrDefaultAsync(p => p.Id == originalId);
            updatedPerson.Should().NotBeNull();
            updatedPerson.Name.Should().Be("Alice Williams");

            // Verify old name doesn't exist
            var oldPerson = await context.People.FirstOrDefaultAsync(p => p.Name == "Alice Johnson");
            oldPerson.Should().BeNull();
        }

        [Fact]
        public async Task Handle_ShouldMaintainPersonId()
        {
            // Arrange
            var context = CreateInMemoryContext();
            var person = new Person { Name = "Bob Smith" };
            context.People.Add(person);
            await context.SaveChangesAsync();
            var originalId = person.Id;

            var handler = new UpdatePersonHandler(context);
            var request = new UpdatePerson
            {
                CurrentName = "Bob Smith",
                NewName = "Robert Smith"
            };

            // Act
            var result = await handler.Handle(request, CancellationToken.None);

            // Assert
            result.Id.Should().Be(originalId);
            var updatedPerson = await context.People.FirstOrDefaultAsync(p => p.Name == "Robert Smith");
            updatedPerson.Should().NotBeNull();
            updatedPerson.Id.Should().Be(originalId);
        }

        [Fact]
        public async Task Handle_WithMultipleUpdates_ShouldUpdateCorrectly()
        {
            // Arrange
            var context = CreateInMemoryContext();
            var person = new Person { Name = "Charlie Brown" };
            context.People.Add(person);
            await context.SaveChangesAsync();
            var personId = person.Id;

            var handler = new UpdatePersonHandler(context);

            var request1 = new UpdatePerson
            {
                CurrentName = "Charlie Brown",
                NewName = "Charles Brown"
            };

            var request2 = new UpdatePerson
            {
                CurrentName = "Charles Brown",
                NewName = "Chuck Brown"
            };

            // Act
            var result1 = await handler.Handle(request1, CancellationToken.None);
            var result2 = await handler.Handle(request2, CancellationToken.None);

            // Assert
            result1.Success.Should().BeTrue();
            result1.Id.Should().Be(personId);
            result1.NewName.Should().Be("Charles Brown");

            result2.Success.Should().BeTrue();
            result2.Id.Should().Be(personId);
            result2.NewName.Should().Be("Chuck Brown");

            var finalPerson = await context.People.FirstOrDefaultAsync(p => p.Id == personId);
            finalPerson.Should().NotBeNull();
            finalPerson.Name.Should().Be("Chuck Brown");

            var people = await context.People.ToListAsync();
            people.Should().HaveCount(1);
        }

        [Fact]
        public async Task Handle_WithRelatedAstronautData_ShouldOnlyUpdatePersonName()
        {
            // Arrange
            var context = CreateInMemoryContext();
            var person = new Person { Name = "David Lee" };
            context.People.Add(person);
            await context.SaveChangesAsync();

            var astronautDetail = new AstronautDetail
            {
                PersonId = person.Id,
                CurrentRank = "Captain",
                CurrentDutyTitle = "Pilot",
                CareerStartDate = DateTime.Now.AddYears(-5)
            };
            context.AstronautDetails.Add(astronautDetail);

            var astronautDuty = new AstronautDuty
            {
                PersonId = person.Id,
                Rank = "Lieutenant",
                DutyTitle = "Co-Pilot",
                DutyStartDate = DateTime.Now.AddYears(-5)
            };
            context.AstronautDuties.Add(astronautDuty);
            await context.SaveChangesAsync();

            var handler = new UpdatePersonHandler(context);
            var request = new UpdatePerson
            {
                CurrentName = "David Lee",
                NewName = "David Chen"
            };

            // Act
            var result = await handler.Handle(request, CancellationToken.None);

            // Assert
            result.Success.Should().BeTrue();
            result.NewName.Should().Be("David Chen");

            // Verify person name updated
            var updatedPerson = await context.People
                .Include(p => p.AstronautDetail)
                .Include(p => p.AstronautDuties)
                .FirstOrDefaultAsync(p => p.Id == person.Id);

            updatedPerson.Should().NotBeNull();
            updatedPerson.Name.Should().Be("David Chen");

            // Verify astronaut data relationships maintained
            updatedPerson.AstronautDetail.Should().NotBeNull();
            updatedPerson.AstronautDetail.PersonId.Should().Be(person.Id);
            updatedPerson.AstronautDetail.CurrentRank.Should().Be("Captain");

            updatedPerson.AstronautDuties.Should().HaveCount(1);
            updatedPerson.AstronautDuties.First().PersonId.Should().Be(person.Id);
            updatedPerson.AstronautDuties.First().Rank.Should().Be("Lieutenant");
        }
    }
}
