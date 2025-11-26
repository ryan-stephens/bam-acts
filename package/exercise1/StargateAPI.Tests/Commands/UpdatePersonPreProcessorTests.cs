using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using StargateAPI.Business.Commands;
using StargateAPI.Business.Data;
using Xunit;

namespace StargateAPI.Tests.Commands
{
    public class UpdatePersonPreProcessorTests
    {
        private StargateContext CreateInMemoryContext()
        {
            var options = new DbContextOptionsBuilder<StargateContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            return new StargateContext(options);
        }

        [Fact]
        public async Task Process_WithValidUpdate_ShouldSucceed()
        {
            // Arrange
            var context = CreateInMemoryContext();
            context.People.Add(new Person { Name = "John Doe" });
            await context.SaveChangesAsync();

            var preprocessor = new UpdatePersonPreProcessor(context);
            var request = new UpdatePerson
            {
                CurrentName = "John Doe",
                NewName = "Jane Doe"
            };

            // Act
            Func<Task> act = async () => await preprocessor.Process(request, CancellationToken.None);

            // Assert
            await act.Should().NotThrowAsync();
        }

        [Fact]
        public async Task Process_WithNonExistentPerson_ShouldThrowBadHttpRequestException()
        {
            // Arrange
            var context = CreateInMemoryContext();
            var preprocessor = new UpdatePersonPreProcessor(context);
            var request = new UpdatePerson
            {
                CurrentName = "NonExistent Person",
                NewName = "New Name"
            };

            // Act
            Func<Task> act = async () => await preprocessor.Process(request, CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<BadHttpRequestException>()
                .WithMessage("Person with name 'NonExistent Person' not found.");
        }

        [Fact]
        public async Task Process_WithDuplicateNewName_ShouldThrowBadHttpRequestException()
        {
            // Arrange
            var context = CreateInMemoryContext();
            context.People.Add(new Person { Name = "John Doe" });
            context.People.Add(new Person { Name = "Jane Doe" });
            await context.SaveChangesAsync();

            var preprocessor = new UpdatePersonPreProcessor(context);
            var request = new UpdatePerson
            {
                CurrentName = "John Doe",
                NewName = "Jane Doe"
            };

            // Act
            Func<Task> act = async () => await preprocessor.Process(request, CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<BadHttpRequestException>()
                .WithMessage("Person with name 'Jane Doe' already exists. Person names must be unique.");
        }

        [Fact]
        public async Task Process_WithEmptyNewName_ShouldThrowBadHttpRequestException()
        {
            // Arrange
            var context = CreateInMemoryContext();
            context.People.Add(new Person { Name = "John Doe" });
            await context.SaveChangesAsync();

            var preprocessor = new UpdatePersonPreProcessor(context);
            var request = new UpdatePerson
            {
                CurrentName = "John Doe",
                NewName = ""
            };

            // Act
            Func<Task> act = async () => await preprocessor.Process(request, CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<BadHttpRequestException>()
                .WithMessage("New person name cannot be empty or whitespace.");
        }

        [Fact]
        public async Task Process_WithWhitespaceNewName_ShouldThrowBadHttpRequestException()
        {
            // Arrange
            var context = CreateInMemoryContext();
            context.People.Add(new Person { Name = "John Doe" });
            await context.SaveChangesAsync();

            var preprocessor = new UpdatePersonPreProcessor(context);
            var request = new UpdatePerson
            {
                CurrentName = "John Doe",
                NewName = "   "
            };

            // Act
            Func<Task> act = async () => await preprocessor.Process(request, CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<BadHttpRequestException>()
                .WithMessage("New person name cannot be empty or whitespace.");
        }

        [Fact]
        public async Task Process_WithSameCurrentAndNewName_ShouldSucceed()
        {
            // Arrange
            var context = CreateInMemoryContext();
            context.People.Add(new Person { Name = "John Doe" });
            await context.SaveChangesAsync();

            var preprocessor = new UpdatePersonPreProcessor(context);
            var request = new UpdatePerson
            {
                CurrentName = "John Doe",
                NewName = "John Doe"
            };

            // Act
            Func<Task> act = async () => await preprocessor.Process(request, CancellationToken.None);

            // Assert
            await act.Should().NotThrowAsync();
        }

        [Fact]
        public async Task Process_WithDifferentCasingNewName_ShouldSucceed()
        {
            // Arrange
            var context = CreateInMemoryContext();
            context.People.Add(new Person { Name = "John Doe" });
            await context.SaveChangesAsync();

            var preprocessor = new UpdatePersonPreProcessor(context);
            var request = new UpdatePerson
            {
                CurrentName = "John Doe",
                NewName = "john doe"
            };

            // Act
            Func<Task> act = async () => await preprocessor.Process(request, CancellationToken.None);

            // Assert
            // InMemory database is case-sensitive by default
            await act.Should().NotThrowAsync();
        }
    }
}
