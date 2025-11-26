using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using StargateAPI.Business.Commands;
using StargateAPI.Business.Data;
using Xunit;

namespace StargateAPI.Tests.Commands
{
    public class CreatePersonHandlerTests
    {
        private StargateContext CreateInMemoryContext()
        {
            var options = new DbContextOptionsBuilder<StargateContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            return new StargateContext(options);
        }

        [Fact]
        public async Task Handle_WithValidName_ShouldCreatePerson()
        {
            // Arrange
            var context = CreateInMemoryContext();
            var handler = new CreatePersonHandler(context);
            var request = new CreatePerson { Name = "John Doe" };

            // Act
            var result = await handler.Handle(request, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeTrue();
            result.Id.Should().BeGreaterThan(0);

            var person = await context.People.FirstOrDefaultAsync(p => p.Name == "John Doe");
            person.Should().NotBeNull();
            person.Name.Should().Be("John Doe");
        }

        [Fact]
        public async Task Handle_WithValidName_ShouldReturnCorrectPersonId()
        {
            // Arrange
            var context = CreateInMemoryContext();
            var handler = new CreatePersonHandler(context);
            var request = new CreatePerson { Name = "Jane Doe" };

            // Act
            var result = await handler.Handle(request, CancellationToken.None);

            // Assert
            result.Id.Should().BeGreaterThan(0);
            var person = await context.People.FirstOrDefaultAsync(p => p.Id == result.Id);
            person.Should().NotBeNull();
            person.Name.Should().Be("Jane Doe");
        }

        [Fact]
        public async Task Handle_MultiplePersons_ShouldCreateAll()
        {
            // Arrange
            var context = CreateInMemoryContext();
            var handler = new CreatePersonHandler(context);

            var request1 = new CreatePerson { Name = "Alice Johnson" };
            var request2 = new CreatePerson { Name = "Bob Smith" };
            var request3 = new CreatePerson { Name = "Charlie Brown" };

            // Act
            var result1 = await handler.Handle(request1, CancellationToken.None);
            var result2 = await handler.Handle(request2, CancellationToken.None);
            var result3 = await handler.Handle(request3, CancellationToken.None);

            // Assert
            result1.Success.Should().BeTrue();
            result2.Success.Should().BeTrue();
            result3.Success.Should().BeTrue();

            var people = await context.People.ToListAsync();
            people.Should().HaveCount(3);
            people.Select(p => p.Name).Should().Contain(new[] { "Alice Johnson", "Bob Smith", "Charlie Brown" });
        }

        [Fact]
        public async Task Handle_ShouldPersistToDatabase()
        {
            // Arrange
            var context = CreateInMemoryContext();
            var handler = new CreatePersonHandler(context);
            var request = new CreatePerson { Name = "David Lee" };

            // Act
            var result = await handler.Handle(request, CancellationToken.None);

            // Assert
            result.Success.Should().BeTrue();

            // Verify persistence by querying the database
            var person = await context.People.FirstOrDefaultAsync(p => p.Id == result.Id);
            person.Should().NotBeNull();
            person.Name.Should().Be("David Lee");
        }

        [Fact]
        public async Task Handle_WithDifferentNames_ShouldCreateDistinctPeople()
        {
            // Arrange
            var context = CreateInMemoryContext();
            var handler = new CreatePersonHandler(context);

            var request1 = new CreatePerson { Name = "Eva Martinez" };
            var request2 = new CreatePerson { Name = "Frank Wilson" };

            // Act
            var result1 = await handler.Handle(request1, CancellationToken.None);
            var result2 = await handler.Handle(request2, CancellationToken.None);

            // Assert
            result1.Id.Should().NotBe(result2.Id);

            var person1 = await context.People.FirstOrDefaultAsync(p => p.Id == result1.Id);
            var person2 = await context.People.FirstOrDefaultAsync(p => p.Id == result2.Id);

            person1.Name.Should().Be("Eva Martinez");
            person2.Name.Should().Be("Frank Wilson");
        }
    }
}
