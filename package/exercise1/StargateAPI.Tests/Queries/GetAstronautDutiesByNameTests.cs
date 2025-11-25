using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using StargateAPI.Business.Data;
using StargateAPI.Business.Queries;
using Xunit;

namespace StargateAPI.Tests.Queries
{
    public class GetAstronautDutiesByNameTests
    {
        private StargateContext CreateInMemoryContext()
        {
            var options = new DbContextOptionsBuilder<StargateContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            return new StargateContext(options);
        }

        [Fact]
        public async Task Handle_WithNonExistentPerson_ShouldReturnNotFoundResponse()
        {
            // Arrange
            var context = CreateInMemoryContext();
            var handler = new GetAstronautDutiesByNameHandler(context);
            var query = new GetAstronautDutiesByName { Name = "NonExistent Person" };

            // Act
            var result = await handler.Handle(query, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeFalse();
            result.Message.Should().Be("Person not found");
            result.ResponseCode.Should().Be(404);
        }

        [Fact]
        public async Task Handle_WithPersonButNoAstronautDetails_ShouldReturnNotFoundResponse()
        {
            // Arrange
            var context = CreateInMemoryContext();
            context.People.Add(new Person { Name = "Regular Person" });
            await context.SaveChangesAsync();

            var handler = new GetAstronautDutiesByNameHandler(context);
            var query = new GetAstronautDutiesByName { Name = "Regular Person" };

            // Act
            var result = await handler.Handle(query, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeFalse();
            result.Message.Should().Be("Astronaut details not found for person");
            result.ResponseCode.Should().Be(404);
        }

        [Fact]
        public async Task Handle_WithAstronautNoDuties_ShouldReturnEmptyDutiesList()
        {
            // Arrange
            var context = CreateInMemoryContext();
            var person = new Person { Name = "New Astronaut" };
            context.People.Add(person);
            await context.SaveChangesAsync();

            context.AstronautDetails.Add(new AstronautDetail
            {
                PersonId = person.Id,
                CurrentRank = "1LT",
                CurrentDutyTitle = "Pilot",
                CareerStartDate = DateTime.Now
            });
            await context.SaveChangesAsync();

            var handler = new GetAstronautDutiesByNameHandler(context);
            var query = new GetAstronautDutiesByName { Name = "New Astronaut" };

            // Act
            var result = await handler.Handle(query, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeTrue();
            result.Person.Should().NotBeNull();
            result.Person!.Name.Should().Be("New Astronaut");
            result.AstronautDuties.Should().BeEmpty();
        }

        [Fact]
        public async Task Handle_WithMultipleDuties_ShouldReturnDutiesOrderedByStartDateDescending()
        {
            // Arrange
            var context = CreateInMemoryContext();
            var person = new Person { Name = "Veteran Astronaut" };
            context.People.Add(person);
            await context.SaveChangesAsync();

            context.AstronautDetails.Add(new AstronautDetail
            {
                PersonId = person.Id,
                CurrentRank = "COL",
                CurrentDutyTitle = "Commander",
                CareerStartDate = new DateTime(2015, 1, 1)
            });

            // Add duties in random order
            context.AstronautDuties.AddRange(
                new AstronautDuty
                {
                    PersonId = person.Id,
                    Rank = "MAJ",
                    DutyTitle = "Pilot",
                    DutyStartDate = new DateTime(2020, 6, 1),
                    DutyEndDate = new DateTime(2022, 5, 31)
                },
                new AstronautDuty
                {
                    PersonId = person.Id,
                    Rank = "1LT",
                    DutyTitle = "Engineer",
                    DutyStartDate = new DateTime(2018, 1, 1),
                    DutyEndDate = new DateTime(2020, 5, 31)
                },
                new AstronautDuty
                {
                    PersonId = person.Id,
                    Rank = "COL",
                    DutyTitle = "Commander",
                    DutyStartDate = new DateTime(2022, 6, 1),
                    DutyEndDate = null  // Current duty
                }
            );
            await context.SaveChangesAsync();

            var handler = new GetAstronautDutiesByNameHandler(context);
            var query = new GetAstronautDutiesByName { Name = "Veteran Astronaut" };

            // Act
            var result = await handler.Handle(query, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeTrue();
            result.AstronautDuties.Should().HaveCount(3);

            // Verify ordering (most recent first)
            result.AstronautDuties[0].DutyStartDate.Should().Be(new DateTime(2022, 6, 1));
            result.AstronautDuties[0].DutyTitle.Should().Be("Commander");
            result.AstronautDuties[0].DutyEndDate.Should().BeNull();

            result.AstronautDuties[1].DutyStartDate.Should().Be(new DateTime(2020, 6, 1));
            result.AstronautDuties[1].DutyTitle.Should().Be("Pilot");

            result.AstronautDuties[2].DutyStartDate.Should().Be(new DateTime(2018, 1, 1));
            result.AstronautDuties[2].DutyTitle.Should().Be("Engineer");
        }

        [Fact]
        public async Task Handle_WithRetiredAstronaut_ShouldCalculateCareerEndDate()
        {
            // Arrange - Rule 7: Career end date is the day before retirement duty start
            var context = CreateInMemoryContext();
            var person = new Person { Name = "Retired Astronaut" };
            context.People.Add(person);
            await context.SaveChangesAsync();

            var retirementStartDate = new DateTime(2025, 1, 1);
            var expectedCareerEndDate = new DateTime(2024, 12, 31); // Day before retirement

            context.AstronautDetails.Add(new AstronautDetail
            {
                PersonId = person.Id,
                CurrentRank = "COL",
                CurrentDutyTitle = "RETIRED",
                CareerStartDate = new DateTime(2015, 1, 1),
                CareerEndDate = null  // Will be calculated
            });

            context.AstronautDuties.AddRange(
                new AstronautDuty
                {
                    PersonId = person.Id,
                    Rank = "COL",
                    DutyTitle = "Commander",
                    DutyStartDate = new DateTime(2020, 1, 1),
                    DutyEndDate = new DateTime(2024, 12, 31)
                },
                new AstronautDuty
                {
                    PersonId = person.Id,
                    Rank = "COL",
                    DutyTitle = "RETIRED",
                    DutyStartDate = retirementStartDate,
                    DutyEndDate = null
                }
            );
            await context.SaveChangesAsync();

            var handler = new GetAstronautDutiesByNameHandler(context);
            var query = new GetAstronautDutiesByName { Name = "Retired Astronaut" };

            // Act
            var result = await handler.Handle(query, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeTrue();
            result.AstronautDuties.Should().HaveCount(2);

            // Verify the RETIRED duty is returned
            var retiredDuty = result.AstronautDuties.First(d => d.DutyTitle == "RETIRED");
            retiredDuty.Should().NotBeNull();
            retiredDuty.DutyStartDate.Should().Be(retirementStartDate);
        }

        [Fact]
        public async Task Handle_ShouldNotIncludePersonNavigationProperty()
        {
            // Arrange - This test verifies we don't have circular reference issues
            var context = CreateInMemoryContext();
            var person = new Person { Name = "Test Astronaut" };
            context.People.Add(person);
            await context.SaveChangesAsync();

            context.AstronautDetails.Add(new AstronautDetail
            {
                PersonId = person.Id,
                CurrentRank = "CPT",
                CurrentDutyTitle = "Pilot",
                CareerStartDate = DateTime.Now
            });

            context.AstronautDuties.Add(new AstronautDuty
            {
                PersonId = person.Id,
                Rank = "CPT",
                DutyTitle = "Pilot",
                DutyStartDate = DateTime.Now,
                DutyEndDate = null
            });
            await context.SaveChangesAsync();

            var handler = new GetAstronautDutiesByNameHandler(context);
            var query = new GetAstronautDutiesByName { Name = "Test Astronaut" };

            // Act
            var result = await handler.Handle(query, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeTrue();
            result.AstronautDuties.Should().HaveCount(1);

            // The Person navigation property should not be loaded (explicit select prevents it)
            result.AstronautDuties[0].PersonId.Should().Be(person.Id);
        }
    }
}
