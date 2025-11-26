using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using StargateAPI.Business.Commands;
using StargateAPI.Business.Data;
using Xunit;

namespace StargateAPI.Tests.Commands
{
    public class CreateAstronautDutyHandlerTests
    {
        private StargateContext CreateInMemoryContext()
        {
            var options = new DbContextOptionsBuilder<StargateContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            return new StargateContext(options);
        }

        [Fact]
        public async Task Handle_FirstDutyForPerson_ShouldCreateAstronautDetailAndDuty()
        {
            // Arrange
            var context = CreateInMemoryContext();
            var person = new Person { Name = "John Doe" };
            context.People.Add(person);
            await context.SaveChangesAsync();

            var handler = new CreateAstronautDutyHandler(context);
            var request = new CreateAstronautDuty
            {
                Name = "John Doe",
                Rank = "Lieutenant",
                DutyTitle = "Pilot",
                DutyStartDate = new DateTime(2025, 1, 15)
            };

            // Act
            var result = await handler.Handle(request, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeTrue();
            result.Id.Should().BeGreaterThan(0);

            var astronautDetail = await context.AstronautDetails
                .FirstOrDefaultAsync(ad => ad.PersonId == person.Id);
            astronautDetail.Should().NotBeNull();
            astronautDetail.CurrentRank.Should().Be("Lieutenant");
            astronautDetail.CurrentDutyTitle.Should().Be("Pilot");
            astronautDetail.CareerStartDate.Should().Be(new DateTime(2025, 1, 15));
        }

        [Fact]
        public async Task Handle_SecondDutyForPerson_ShouldUpdateAstronautDetailAndClosePreviousDuty()
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

            var firstDuty = new AstronautDuty
            {
                PersonId = person.Id,
                Rank = "Lieutenant",
                DutyTitle = "Pilot",
                DutyStartDate = new DateTime(2025, 1, 15),
                DutyEndDate = null
            };
            context.AstronautDuties.Add(firstDuty);
            await context.SaveChangesAsync();

            var handler = new CreateAstronautDutyHandler(context);
            var request = new CreateAstronautDuty
            {
                Name = "Jane Doe",
                Rank = "Captain",
                DutyTitle = "Commander",
                DutyStartDate = new DateTime(2025, 3, 1)
            };

            // Act
            var result = await handler.Handle(request, CancellationToken.None);

            // Assert
            result.Success.Should().BeTrue();

            var updatedDetail = await context.AstronautDetails
                .FirstOrDefaultAsync(ad => ad.PersonId == person.Id);
            updatedDetail.CurrentRank.Should().Be("Captain");
            updatedDetail.CurrentDutyTitle.Should().Be("Commander");

            var closedDuty = await context.AstronautDuties
                .FirstOrDefaultAsync(ad => ad.Id == firstDuty.Id);
            closedDuty.DutyEndDate.Should().Be(new DateTime(2025, 2, 28)); // One day before new duty
        }

        [Fact]
        public async Task Handle_RetiredDuty_ShouldSetCareerEndDate()
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
                CurrentDutyTitle = "Pilot",
                CareerStartDate = new DateTime(2025, 1, 15)
            };
            context.AstronautDetails.Add(astronautDetail);

            var firstDuty = new AstronautDuty
            {
                PersonId = person.Id,
                Rank = "Lieutenant",
                DutyTitle = "Pilot",
                DutyStartDate = new DateTime(2025, 1, 15),
                DutyEndDate = null
            };
            context.AstronautDuties.Add(firstDuty);
            await context.SaveChangesAsync();

            var handler = new CreateAstronautDutyHandler(context);
            var request = new CreateAstronautDuty
            {
                Name = "Bob Smith",
                Rank = "Lieutenant",
                DutyTitle = "RETIRED",
                DutyStartDate = new DateTime(2025, 4, 15)
            };

            // Act
            var result = await handler.Handle(request, CancellationToken.None);

            // Assert
            result.Success.Should().BeTrue();

            var updatedDetail = await context.AstronautDetails
                .FirstOrDefaultAsync(ad => ad.PersonId == person.Id);
            updatedDetail.CareerEndDate.Should().Be(new DateTime(2025, 4, 14)); // One day before retirement
            updatedDetail.CurrentDutyTitle.Should().Be("RETIRED");
        }

        [Fact]
        public async Task Handle_DutyAfterRetirement_ShouldClearCareerEndDate()
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
                CurrentDutyTitle = "RETIRED",
                CareerStartDate = new DateTime(2025, 1, 15),
                CareerEndDate = new DateTime(2025, 4, 14)
            };
            context.AstronautDetails.Add(astronautDetail);

            var retiredDuty = new AstronautDuty
            {
                PersonId = person.Id,
                Rank = "Lieutenant",
                DutyTitle = "RETIRED",
                DutyStartDate = new DateTime(2025, 4, 15),
                DutyEndDate = new DateTime(2025, 4, 15)
            };
            context.AstronautDuties.Add(retiredDuty);
            await context.SaveChangesAsync();

            var handler = new CreateAstronautDutyHandler(context);
            var request = new CreateAstronautDuty
            {
                Name = "Alice Johnson",
                Rank = "General",
                DutyTitle = "Director",
                DutyStartDate = new DateTime(2025, 5, 20)
            };

            // Act
            var result = await handler.Handle(request, CancellationToken.None);

            // Assert
            result.Success.Should().BeTrue();

            var updatedDetail = await context.AstronautDetails
                .FirstOrDefaultAsync(ad => ad.PersonId == person.Id);
            updatedDetail.CareerEndDate.Should().BeNull(); // Cleared after retirement
            updatedDetail.CurrentDutyTitle.Should().Be("Director");
        }

        [Fact]
        public async Task Handle_RetiredDutyShouldHaveEndDate()
        {
            // Arrange
            var context = CreateInMemoryContext();
            var person = new Person { Name = "Charlie Brown" };
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

            var firstDuty = new AstronautDuty
            {
                PersonId = person.Id,
                Rank = "Lieutenant",
                DutyTitle = "Pilot",
                DutyStartDate = new DateTime(2025, 1, 15),
                DutyEndDate = null
            };
            context.AstronautDuties.Add(firstDuty);
            await context.SaveChangesAsync();

            var handler = new CreateAstronautDutyHandler(context);
            var request = new CreateAstronautDuty
            {
                Name = "Charlie Brown",
                Rank = "Lieutenant",
                DutyTitle = "RETIRED",
                DutyStartDate = new DateTime(2025, 4, 15)
            };

            // Act
            var result = await handler.Handle(request, CancellationToken.None);

            // Assert
            result.Success.Should().BeTrue();

            var retiredDuty = await context.AstronautDuties
                .FirstOrDefaultAsync(ad => ad.DutyTitle == "RETIRED" && ad.PersonId == person.Id);
            retiredDuty.Should().NotBeNull();
            retiredDuty.DutyEndDate.Should().Be(new DateTime(2025, 4, 15)); // Closed immediately
        }

        [Fact]
        public async Task Handle_NonRetiredDutyShouldNotHaveEndDate()
        {
            // Arrange
            var context = CreateInMemoryContext();
            var person = new Person { Name = "David Lee" };
            context.People.Add(person);
            await context.SaveChangesAsync();

            var handler = new CreateAstronautDutyHandler(context);
            var request = new CreateAstronautDuty
            {
                Name = "David Lee",
                Rank = "Lieutenant",
                DutyTitle = "Pilot",
                DutyStartDate = new DateTime(2025, 1, 15)
            };

            // Act
            var result = await handler.Handle(request, CancellationToken.None);

            // Assert
            result.Success.Should().BeTrue();

            var duty = await context.AstronautDuties
                .FirstOrDefaultAsync(ad => ad.Id == result.Id);
            duty.DutyEndDate.Should().BeNull(); // Current duty has no end date
        }
    }
}
