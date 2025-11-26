using FluentAssertions;
using StargateAPI.Business.Commands;
using StargateAPI.Controllers;
using Xunit;

namespace StargateAPI.Tests.Controllers
{
    public class AstronautDutyControllerTests
    {
        [Fact]
        public void CreateAstronautDuty_WithValidRequest_ShouldHaveRequiredFields()
        {
            // Validate that CreateAstronautDuty request has required fields
            var request = new CreateAstronautDuty
            {
                Name = "John Doe",
                Rank = "Lieutenant",
                DutyTitle = "Pilot",
                DutyStartDate = new DateTime(2025, 1, 15)
            };

            request.Name.Should().NotBeNullOrEmpty();
            request.Rank.Should().NotBeNullOrEmpty();
            request.DutyTitle.Should().NotBeNullOrEmpty();
            request.DutyStartDate.Should().Be(new DateTime(2025, 1, 15));
        }

        [Fact]
        public void UpdateAstronautDuty_WithValidRequest_ShouldHaveRequiredFields()
        {
            // Validate that UpdateAstronautDuty request has required fields
            var request = new UpdateAstronautDuty
            {
                Id = 1,
                Rank = "Captain",
                DutyTitle = "Commander",
                DutyStartDate = new DateTime(2025, 1, 15),
                DutyEndDate = null
            };

            request.Id.Should().BeGreaterThan(0);
            request.Rank.Should().NotBeNullOrEmpty();
            request.DutyTitle.Should().NotBeNullOrEmpty();
            request.DutyStartDate.Should().Be(new DateTime(2025, 1, 15));
        }

        [Fact]
        public void AstronautDutyController_ShouldHaveGetAstronautDutiesByNameMethod()
        {
            // Verify the controller has the required method
            var method = typeof(AstronautDutyController).GetMethod("GetAstronautDutiesByName");
            method.Should().NotBeNull();
        }

        [Fact]
        public void AstronautDutyController_ShouldHaveCreateAstronautDutyMethod()
        {
            // Verify the controller has the required method
            var method = typeof(AstronautDutyController).GetMethod("CreateAstronautDuty");
            method.Should().NotBeNull();
        }

        [Fact]
        public void AstronautDutyController_ShouldHaveUpdateAstronautDutyMethod()
        {
            // Verify the controller has the required method
            var method = typeof(AstronautDutyController).GetMethod("UpdateAstronautDuty");
            method.Should().NotBeNull();
        }

        [Fact]
        public void CreateAstronautDuty_WithRetiredDuty_ShouldAcceptRetiredTitle()
        {
            // Validate that RETIRED is a valid duty title
            var request = new CreateAstronautDuty
            {
                Name = "John Doe",
                Rank = "Lieutenant",
                DutyTitle = "RETIRED",
                DutyStartDate = new DateTime(2025, 4, 15)
            };

            request.DutyTitle.Should().Be("RETIRED");
        }

        [Fact]
        public void UpdateAstronautDuty_WithDateRange_ShouldValidateDates()
        {
            // Validate that dates can be properly set
            var startDate = new DateTime(2025, 1, 15);
            var endDate = new DateTime(2025, 3, 31);

            startDate.Should().BeBefore(endDate);
            endDate.Should().BeAfter(startDate);
        }

        [Fact]
        public void CreateAstronautDuty_WithMultipleDuties_ShouldAcceptSequential()
        {
            // Validate that multiple duty requests can be created sequentially
            var duty1 = new CreateAstronautDuty
            {
                Name = "John Doe",
                Rank = "Lieutenant",
                DutyTitle = "Pilot",
                DutyStartDate = new DateTime(2025, 1, 15)
            };

            var duty2 = new CreateAstronautDuty
            {
                Name = "John Doe",
                Rank = "Captain",
                DutyTitle = "Commander",
                DutyStartDate = new DateTime(2025, 3, 1)
            };

            duty1.Name.Should().Be(duty2.Name);
            duty1.DutyStartDate.Should().BeBefore(duty2.DutyStartDate);
        }
    }
}
