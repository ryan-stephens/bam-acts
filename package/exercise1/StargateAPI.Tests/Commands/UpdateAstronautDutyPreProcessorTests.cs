using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using StargateAPI.Business.Commands;
using StargateAPI.Business.Data;
using Xunit;

namespace StargateAPI.Tests.Commands
{
    public class UpdateAstronautDutyPreProcessorTests
    {
        private StargateContext CreateInMemoryContext()
        {
            var options = new DbContextOptionsBuilder<StargateContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            return new StargateContext(options);
        }

        [Fact]
        public async Task Process_WithValidDuty_ShouldSucceed()
        {
            // Arrange
            var context = CreateInMemoryContext();
            var duty = new AstronautDuty
            {
                Id = 1,
                PersonId = 1,
                Rank = "Lieutenant",
                DutyTitle = "Pilot",
                DutyStartDate = new DateTime(2025, 1, 15),
                DutyEndDate = null
            };
            context.AstronautDuties.Add(duty);
            await context.SaveChangesAsync();

            var preprocessor = new UpdateAstronautDutyPreProcessor(context);
            var request = new UpdateAstronautDuty
            {
                Id = 1,
                Rank = "Captain",
                DutyTitle = "Commander",
                DutyStartDate = new DateTime(2025, 1, 15),
                DutyEndDate = null
            };

            // Act & Assert - Should not throw
            await preprocessor.Process(request, CancellationToken.None);
        }

        [Fact]
        public async Task Process_WithNonExistentDuty_ShouldThrowException()
        {
            // Arrange
            var context = CreateInMemoryContext();
            var preprocessor = new UpdateAstronautDutyPreProcessor(context);
            var request = new UpdateAstronautDuty
            {
                Id = 999,
                Rank = "Captain",
                DutyTitle = "Commander",
                DutyStartDate = new DateTime(2025, 1, 15),
                DutyEndDate = null
            };

            // Act & Assert
            await Assert.ThrowsAsync<BadHttpRequestException>(async () =>
                await preprocessor.Process(request, CancellationToken.None));
        }

        [Fact]
        public async Task Process_WithInvalidDateRange_ShouldThrowException()
        {
            // Arrange
            var context = CreateInMemoryContext();
            var duty = new AstronautDuty
            {
                Id = 1,
                PersonId = 1,
                Rank = "Lieutenant",
                DutyTitle = "Pilot",
                DutyStartDate = new DateTime(2025, 1, 15),
                DutyEndDate = null
            };
            context.AstronautDuties.Add(duty);
            await context.SaveChangesAsync();

            var preprocessor = new UpdateAstronautDutyPreProcessor(context);
            var request = new UpdateAstronautDuty
            {
                Id = 1,
                Rank = "Captain",
                DutyTitle = "Commander",
                DutyStartDate = new DateTime(2025, 3, 1),
                DutyEndDate = new DateTime(2025, 2, 1) // End before start
            };

            // Act & Assert
            await Assert.ThrowsAsync<BadHttpRequestException>(async () =>
                await preprocessor.Process(request, CancellationToken.None));
        }

        [Fact]
        public async Task Process_WithEndDateEqualToStartDate_ShouldThrowException()
        {
            // Arrange
            var context = CreateInMemoryContext();
            var duty = new AstronautDuty
            {
                Id = 1,
                PersonId = 1,
                Rank = "Lieutenant",
                DutyTitle = "Pilot",
                DutyStartDate = new DateTime(2025, 1, 15),
                DutyEndDate = null
            };
            context.AstronautDuties.Add(duty);
            await context.SaveChangesAsync();

            var preprocessor = new UpdateAstronautDutyPreProcessor(context);
            var request = new UpdateAstronautDuty
            {
                Id = 1,
                Rank = "Captain",
                DutyTitle = "Commander",
                DutyStartDate = new DateTime(2025, 3, 1),
                DutyEndDate = new DateTime(2025, 3, 1) // Same as start
            };

            // Act & Assert
            await Assert.ThrowsAsync<BadHttpRequestException>(async () =>
                await preprocessor.Process(request, CancellationToken.None));
        }

        [Fact]
        public async Task Process_WithValidEndDate_ShouldSucceed()
        {
            // Arrange
            var context = CreateInMemoryContext();
            var duty = new AstronautDuty
            {
                Id = 1,
                PersonId = 1,
                Rank = "Lieutenant",
                DutyTitle = "Pilot",
                DutyStartDate = new DateTime(2025, 1, 15),
                DutyEndDate = null
            };
            context.AstronautDuties.Add(duty);
            await context.SaveChangesAsync();

            var preprocessor = new UpdateAstronautDutyPreProcessor(context);
            var request = new UpdateAstronautDuty
            {
                Id = 1,
                Rank = "Captain",
                DutyTitle = "Commander",
                DutyStartDate = new DateTime(2025, 1, 15),
                DutyEndDate = new DateTime(2025, 3, 31)
            };

            // Act & Assert - Should not throw
            await preprocessor.Process(request, CancellationToken.None);
        }
    }
}
