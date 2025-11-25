using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using StargateAPI.Business.Commands;
using StargateAPI.Business.Data;
using Xunit;

namespace StargateAPI.Tests.Commands
{
    public class CreatePersonPreProcessorTests
    {
        private StargateContext CreateInMemoryContext()
        {
            var options = new DbContextOptionsBuilder<StargateContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            return new StargateContext(options);
        }

        [Fact]
        public async Task Process_WithUniqueName_ShouldSucceed()
        {
            // Arrange
            var context = CreateInMemoryContext();
            var preprocessor = new CreatePersonPreProcessor(context);
            var request = new CreatePerson { Name = "New Person" };

            // Act
            Func<Task> act = async () => await preprocessor.Process(request, CancellationToken.None);

            // Assert
            await act.Should().NotThrowAsync();
        }

        [Fact]
        public async Task Process_WithDuplicateName_ShouldThrowBadHttpRequestException()
        {
            // Arrange
            var context = CreateInMemoryContext();
            context.People.Add(new Person { Name = "Existing Person" });
            await context.SaveChangesAsync();

            var preprocessor = new CreatePersonPreProcessor(context);
            var request = new CreatePerson { Name = "Existing Person" };

            // Act
            Func<Task> act = async () => await preprocessor.Process(request, CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<BadHttpRequestException>()
                .WithMessage("Person with name 'Existing Person' already exists. Person names must be unique.");
        }

        [Fact]
        public async Task Process_WithDuplicateNameDifferentCasing_ShouldSucceed()
        {
            // Arrange
            var context = CreateInMemoryContext();
            context.People.Add(new Person { Name = "John Doe" });
            await context.SaveChangesAsync();

            var preprocessor = new CreatePersonPreProcessor(context);
            var request = new CreatePerson { Name = "john doe" };

            // Act
            Func<Task> act = async () => await preprocessor.Process(request, CancellationToken.None);

            // Assert - InMemory database is case-sensitive by default
            // Different casing is treated as a different name
            await act.Should().NotThrowAsync();
        }

        [Fact]
        public async Task Process_WithEmptyDatabase_ShouldSucceed()
        {
            // Arrange
            var context = CreateInMemoryContext();
            var preprocessor = new CreatePersonPreProcessor(context);
            var request = new CreatePerson { Name = "First Person" };

            // Act
            Func<Task> act = async () => await preprocessor.Process(request, CancellationToken.None);

            // Assert
            await act.Should().NotThrowAsync();
        }
    }
}
