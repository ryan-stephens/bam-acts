using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using StargateAPI.Business.Commands;
using StargateAPI.Business.Data;
using Xunit;

namespace StargateAPI.Tests.Commands
{
    public class CreatePersonTests
    {
        private StargateContext CreateInMemoryContext()
        {
            var options = new DbContextOptionsBuilder<StargateContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            return new StargateContext(options);
        }

        [Fact]
        public async Task Handle_WithValidName_ShouldCreatePerson()
        {
            // Arrange
            var context = CreateInMemoryContext();
            var handler = new CreatePersonHandler(context);
            var command = new CreatePerson { Name = "Test Person" };

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeTrue();
            result.Id.Should().BeGreaterThan(0);
            result.Message.Should().Be("Successful");
            result.ResponseCode.Should().Be(200);

            var person = await context.People.FirstOrDefaultAsync(p => p.Name == "Test Person");
            person.Should().NotBeNull();
            person!.Id.Should().Be(result.Id);
        }

        [Fact]
        public async Task Handle_WithValidName_ShouldReturnCorrectPersonId()
        {
            // Arrange
            var context = CreateInMemoryContext();
            var handler = new CreatePersonHandler(context);
            var command = new CreatePerson { Name = "Jane Smith" };

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            var savedPerson = await context.People.FindAsync(result.Id);
            savedPerson.Should().NotBeNull();
            savedPerson!.Name.Should().Be("Jane Smith");
        }

        [Fact]
        public async Task Handle_MultiplePersons_ShouldCreateAll()
        {
            // Arrange
            var context = CreateInMemoryContext();
            var handler = new CreatePersonHandler(context);

            // Act
            var result1 = await handler.Handle(new CreatePerson { Name = "Person 1" }, CancellationToken.None);
            var result2 = await handler.Handle(new CreatePerson { Name = "Person 2" }, CancellationToken.None);
            var result3 = await handler.Handle(new CreatePerson { Name = "Person 3" }, CancellationToken.None);

            // Assert
            result1.Success.Should().BeTrue();
            result2.Success.Should().BeTrue();
            result3.Success.Should().BeTrue();

            var allPeople = await context.People.ToListAsync();
            allPeople.Should().HaveCount(3);
            allPeople.Should().Contain(p => p.Name == "Person 1");
            allPeople.Should().Contain(p => p.Name == "Person 2");
            allPeople.Should().Contain(p => p.Name == "Person 3");
        }
    }
}
