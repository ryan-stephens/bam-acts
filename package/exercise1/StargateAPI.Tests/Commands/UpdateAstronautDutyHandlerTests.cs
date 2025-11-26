using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using StargateAPI.Business.Commands;
using StargateAPI.Business.Data;
using Xunit;

namespace StargateAPI.Tests.Commands
{
    public class UpdateAstronautDutyHandlerTests
    {
        private StargateContext CreateInMemoryContext()
        {
            var options = new DbContextOptionsBuilder<StargateContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            return new StargateContext(options);
        }

        [Fact]
        public async Task Handle_UpdateCurrentDuty_ShouldUpdateAstronautDetail()
        {
            // Arrange
            var context = CreateInMemoryContext();
            var person = new Person { Name = "John Doe" };
            context.People.Add(person);
            await context.SaveChangesAsync();

            var astronautDetail = new AstronautDetail
            {
                PersonId = person.Id,
                CurrentRank = "Lieutenant",
                CurrentDutyTitle = "Pilot",
                CareerStartDate = new DateTime(2025, 1, 15)
            };
            context.AstronautDetails.Add(astronautDetail);

            var duty = new AstronautDuty
            {
                PersonId = person.Id,
                Rank = "Lieutenant",
                DutyTitle = "Pilot",
                DutyStartDate = new DateTime(2025, 1, 15),
                DutyEndDate = null
            };
            context.AstronautDuties.Add(duty);
            await context.SaveChangesAsync();

            var handler = new UpdateAstronautDutyHandler(context);
            var request = new UpdateAstronautDuty
            {
                Id = duty.Id,
                Rank = "Captain",
                DutyTitle = "Commander",
                DutyStartDate = new DateTime(2025, 1, 15),
                DutyEndDate = null
            };

            // Act
            var result = await handler.Handle(request, CancellationToken.None);

            // Assert
            result.Success.Should().BeTrue();

            var updatedDetail = await context.AstronautDetails
                .FirstOrDefaultAsync(ad => ad.PersonId == person.Id);
            updatedDetail.CurrentRank.Should().Be("Captain");
            updatedDetail.CurrentDutyTitle.Should().Be("Commander");
        }

        [Fact]
        public async Task Handle_UpdateToRetired_ShouldSetCareerEndDate()
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
                CurrentDutyTitle = "Pilot",
                CareerStartDate = new DateTime(2025, 1, 15)
            };
            context.AstronautDetails.Add(astronautDetail);

            var duty = new AstronautDuty
            {
                PersonId = person.Id,
                Rank = "Lieutenant",
                DutyTitle = "Pilot",
                DutyStartDate = new DateTime(2025, 1, 15),
                DutyEndDate = null
            };
            context.AstronautDuties.Add(duty);
            await context.SaveChangesAsync();

            var handler = new UpdateAstronautDutyHandler(context);
            var request = new UpdateAstronautDuty
            {
                Id = duty.Id,
                Rank = "Lieutenant",
                DutyTitle = "RETIRED",
                DutyStartDate = new DateTime(2025, 1, 15),
                DutyEndDate = null
            };

            // Act
            var result = await handler.Handle(request, CancellationToken.None);

            // Assert
            result.Success.Should().BeTrue();

            var updatedDetail = await context.AstronautDetails
                .FirstOrDefaultAsync(ad => ad.PersonId == person.Id);
            updatedDetail.CareerEndDate.Should().Be(new DateTime(2025, 1, 14)); // One day before start
            updatedDetail.CurrentDutyTitle.Should().Be("RETIRED");
        }

        [Fact]
        public async Task Handle_UpdateFromRetiredToActive_ShouldClearCareerEndDate()
        {
            // Arrange
            var context = CreateInMemoryContext();
            var person = new Person { Name = "Bob Smith" };
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

            var duty = new AstronautDuty
            {
                PersonId = person.Id,
                Rank = "Lieutenant",
                DutyTitle = "RETIRED",
                DutyStartDate = new DateTime(2025, 4, 15),
                DutyEndDate = null
            };
            context.AstronautDuties.Add(duty);
            await context.SaveChangesAsync();

            var handler = new UpdateAstronautDutyHandler(context);
            var request = new UpdateAstronautDuty
            {
                Id = duty.Id,
                Rank = "General",
                DutyTitle = "Director",
                DutyStartDate = new DateTime(2025, 4, 15),
                DutyEndDate = null
            };

            // Act
            var result = await handler.Handle(request, CancellationToken.None);

            // Assert
            result.Success.Should().BeTrue();

            var updatedDetail = await context.AstronautDetails
                .FirstOrDefaultAsync(ad => ad.PersonId == person.Id);
            updatedDetail.CareerEndDate.Should().BeNull(); // Cleared
            updatedDetail.CurrentDutyTitle.Should().Be("Director");
        }

        [Fact]
        public async Task Handle_CloseDutyWithEndDate_ShouldNotUpdateAstronautDetail()
        {
            // Arrange
            var context = CreateInMemoryContext();
            var person = new Person { Name = "Alice Johnson" };
            context.People.Add(person);
            await context.SaveChangesAsync();

            var astronautDetail = new AstronautDetail
            {
                PersonId = person.Id,
                CurrentRank = "Lieutenant",
                CurrentDutyTitle = "Pilot",
                CareerStartDate = new DateTime(2025, 1, 15)
            };
            context.AstronautDetails.Add(astronautDetail);

            var duty = new AstronautDuty
            {
                PersonId = person.Id,
                Rank = "Lieutenant",
                DutyTitle = "Pilot",
                DutyStartDate = new DateTime(2025, 1, 15),
                DutyEndDate = null
            };
            context.AstronautDuties.Add(duty);
            await context.SaveChangesAsync();

            var handler = new UpdateAstronautDutyHandler(context);
            var request = new UpdateAstronautDuty
            {
                Id = duty.Id,
                Rank = "Lieutenant",
                DutyTitle = "Pilot",
                DutyStartDate = new DateTime(2025, 1, 15),
                DutyEndDate = new DateTime(2025, 3, 31)
            };

            // Act
            var result = await handler.Handle(request, CancellationToken.None);

            // Assert
            result.Success.Should().BeTrue();

            var updatedDetail = await context.AstronautDetails
                .FirstOrDefaultAsync(ad => ad.PersonId == person.Id);
            // Detail should not be updated when closing a past duty
            updatedDetail.CurrentDutyTitle.Should().Be("Pilot");
        }

        [Fact]
        public async Task Handle_NonExistentDuty_ShouldThrowException()
        {
            // Arrange
            var context = CreateInMemoryContext();
            var handler = new UpdateAstronautDutyHandler(context);
            var request = new UpdateAstronautDuty
            {
                Id = 999,
                Rank = "Captain",
                DutyTitle = "Commander",
                DutyStartDate = new DateTime(2025, 1, 15),
                DutyEndDate = null
            };

            // Act & Assert
            var exception = await Assert.ThrowsAsync<BadHttpRequestException>(async () =>
                await handler.Handle(request, CancellationToken.None));
            exception.Message.Should().Contain("Astronaut duty not found");
        }
    }
}
