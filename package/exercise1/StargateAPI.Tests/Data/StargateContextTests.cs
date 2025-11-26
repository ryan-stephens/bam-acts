using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using StargateAPI.Business.Data;
using Xunit;

namespace StargateAPI.Tests.Data
{
    public class StargateContextTests
    {
        private StargateContext CreateInMemoryContext()
        {
            var options = new DbContextOptionsBuilder<StargateContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            return new StargateContext(options);
        }

        [Fact]
        public void StargateContext_ShouldHavePeopleDbSet()
        {
            // Arrange & Act
            var context = CreateInMemoryContext();

            // Assert
            context.People.Should().NotBeNull();
        }

        [Fact]
        public void StargateContext_ShouldHaveAstronautDutiesDbSet()
        {
            // Arrange & Act
            var context = CreateInMemoryContext();

            // Assert
            context.AstronautDuties.Should().NotBeNull();
        }

        [Fact]
        public void StargateContext_ShouldHaveAstronautDetailsDbSet()
        {
            // Arrange & Act
            var context = CreateInMemoryContext();

            // Assert
            context.AstronautDetails.Should().NotBeNull();
        }

        [Fact]
        public void StargateContext_ShouldHaveProcessLogsDbSet()
        {
            // Arrange & Act
            var context = CreateInMemoryContext();

            // Assert
            context.ProcessLogs.Should().NotBeNull();
        }

        [Fact]
        public async Task StargateContext_ShouldPersistPerson()
        {
            // Arrange
            var context = CreateInMemoryContext();
            var person = new Person { Name = "Test Person" };

            // Act
            context.People.Add(person);
            await context.SaveChangesAsync();

            // Assert
            var savedPerson = await context.People.FirstOrDefaultAsync(p => p.Name == "Test Person");
            savedPerson.Should().NotBeNull();
            savedPerson.Name.Should().Be("Test Person");
        }

        [Fact]
        public async Task StargateContext_ShouldPersistAstronautDuty()
        {
            // Arrange
            var context = CreateInMemoryContext();
            var person = new Person { Name = "Test Person" };
            context.People.Add(person);
            await context.SaveChangesAsync();

            var duty = new AstronautDuty
            {
                PersonId = person.Id,
                Rank = "Lieutenant",
                DutyTitle = "Pilot",
                DutyStartDate = DateTime.Now
            };

            // Act
            context.AstronautDuties.Add(duty);
            await context.SaveChangesAsync();

            // Assert
            var savedDuty = await context.AstronautDuties.FirstOrDefaultAsync(d => d.DutyTitle == "Pilot");
            savedDuty.Should().NotBeNull();
            savedDuty.Rank.Should().Be("Lieutenant");
        }

        [Fact]
        public async Task StargateContext_ShouldPersistAstronautDetail()
        {
            // Arrange
            var context = CreateInMemoryContext();
            var person = new Person { Name = "Test Person" };
            context.People.Add(person);
            await context.SaveChangesAsync();

            var detail = new AstronautDetail
            {
                PersonId = person.Id,
                CurrentRank = "Captain",
                CurrentDutyTitle = "Commander",
                CareerStartDate = DateTime.Now
            };

            // Act
            context.AstronautDetails.Add(detail);
            await context.SaveChangesAsync();

            // Assert
            var savedDetail = await context.AstronautDetails.FirstOrDefaultAsync(d => d.PersonId == person.Id);
            savedDetail.Should().NotBeNull();
            savedDetail.CurrentRank.Should().Be("Captain");
        }

        [Fact]
        public async Task StargateContext_ShouldPersistProcessLog()
        {
            // Arrange
            var context = CreateInMemoryContext();
            var log = new ProcessLog
            {
                Message = "Test log",
                LogLevel = "Info",
                Timestamp = DateTime.UtcNow
            };

            // Act
            context.ProcessLogs.Add(log);
            await context.SaveChangesAsync();

            // Assert
            var savedLog = await context.ProcessLogs.FirstOrDefaultAsync(l => l.Message == "Test log");
            savedLog.Should().NotBeNull();
            savedLog.LogLevel.Should().Be("Info");
        }

        [Fact]
        public async Task StargateContext_ShouldMaintainPersonAstronautDutyRelationship()
        {
            // Arrange
            var context = CreateInMemoryContext();
            var person = new Person { Name = "Test Person" };
            context.People.Add(person);
            await context.SaveChangesAsync();

            var duty = new AstronautDuty
            {
                PersonId = person.Id,
                DutyTitle = "Pilot",
                DutyStartDate = DateTime.Now
            };
            context.AstronautDuties.Add(duty);
            await context.SaveChangesAsync();

            // Act
            var savedPerson = await context.People
                .Include(p => p.AstronautDuties)
                .FirstOrDefaultAsync(p => p.Id == person.Id);

            // Assert
            savedPerson.Should().NotBeNull();
            savedPerson.AstronautDuties.Should().HaveCount(1);
        }

        [Fact]
        public async Task StargateContext_ShouldMaintainPersonAstronautDetailRelationship()
        {
            // Arrange
            var context = CreateInMemoryContext();
            var person = new Person { Name = "Test Person" };
            context.People.Add(person);
            await context.SaveChangesAsync();

            var detail = new AstronautDetail
            {
                PersonId = person.Id,
                CurrentRank = "Captain",
                CurrentDutyTitle = "Commander"
            };
            context.AstronautDetails.Add(detail);
            await context.SaveChangesAsync();

            // Act
            var savedPerson = await context.People
                .Include(p => p.AstronautDetail)
                .FirstOrDefaultAsync(p => p.Id == person.Id);

            // Assert
            savedPerson.Should().NotBeNull();
            savedPerson.AstronautDetail.Should().NotBeNull();
            savedPerson.AstronautDetail.CurrentRank.Should().Be("Captain");
        }

        [Fact]
        public async Task StargateContext_ShouldSupportMultiplePeople()
        {
            // Arrange
            var context = CreateInMemoryContext();
            var person1 = new Person { Name = "Person 1" };
            var person2 = new Person { Name = "Person 2" };
            var person3 = new Person { Name = "Person 3" };

            // Act
            context.People.AddRange(person1, person2, person3);
            await context.SaveChangesAsync();

            // Assert
            var allPeople = await context.People.ToListAsync();
            allPeople.Should().HaveCount(3);
        }

        [Fact]
        public async Task StargateContext_ShouldUpdateExistingEntity()
        {
            // Arrange
            var context = CreateInMemoryContext();
            var person = new Person { Name = "Original Name" };
            context.People.Add(person);
            await context.SaveChangesAsync();

            // Act
            person.Name = "Updated Name";
            await context.SaveChangesAsync();

            // Assert
            var updatedPerson = await context.People.FirstOrDefaultAsync(p => p.Id == person.Id);
            updatedPerson.Name.Should().Be("Updated Name");
        }

        [Fact]
        public async Task StargateContext_ShouldDeleteEntity()
        {
            // Arrange
            var context = CreateInMemoryContext();
            var person = new Person { Name = "To Delete" };
            context.People.Add(person);
            await context.SaveChangesAsync();
            var personId = person.Id;

            // Act
            context.People.Remove(person);
            await context.SaveChangesAsync();

            // Assert
            var deletedPerson = await context.People.FirstOrDefaultAsync(p => p.Id == personId);
            deletedPerson.Should().BeNull();
        }
    }
}
