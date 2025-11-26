using FluentAssertions;
using StargateAPI.Business.Data;
using Xunit;

namespace StargateAPI.Tests.Data
{
    public class PersonDataModelTests
    {
        [Fact]
        public void Person_ShouldHaveIdProperty()
        {
            // Arrange & Act
            var person = new Person { Id = 1, Name = "John Doe" };

            // Assert
            person.Id.Should().Be(1);
        }

        [Fact]
        public void Person_ShouldHaveNameProperty()
        {
            // Arrange & Act
            var person = new Person { Id = 1, Name = "Jane Doe" };

            // Assert
            person.Name.Should().Be("Jane Doe");
        }

        [Fact]
        public void Person_ShouldHaveAstronautDetailNavigation()
        {
            // Arrange & Act
            var person = new Person { Id = 1, Name = "Bob Smith" };

            // Assert
            person.AstronautDetail.Should().BeNull();
        }

        [Fact]
        public void Person_ShouldHaveAstronautDutiesNavigation()
        {
            // Arrange & Act
            var person = new Person { Id = 1, Name = "Alice Johnson" };

            // Assert
            person.AstronautDuties.Should().BeEmpty();
        }

        [Fact]
        public void Person_ShouldAllowSettingAstronautDetail()
        {
            // Arrange
            var person = new Person { Id = 1, Name = "Charlie Brown" };
            var detail = new AstronautDetail { PersonId = 1 };

            // Act
            person.AstronautDetail = detail;

            // Assert
            person.AstronautDetail.Should().Be(detail);
            person.AstronautDetail.PersonId.Should().Be(1);
        }

        [Fact]
        public void Person_ShouldAllowAddingAstronautDuties()
        {
            // Arrange
            var person = new Person { Id = 1, Name = "David Lee" };
            var duty = new AstronautDuty { Id = 1, PersonId = 1, DutyTitle = "Pilot" };

            // Act
            person.AstronautDuties.Add(duty);

            // Assert
            person.AstronautDuties.Should().HaveCount(1);
            person.AstronautDuties.Should().Contain(duty);
        }
    }

    public class AstronautDutyDataModelTests
    {
        [Fact]
        public void AstronautDuty_ShouldHaveIdProperty()
        {
            // Arrange & Act
            var duty = new AstronautDuty { Id = 1, DutyTitle = "Pilot" };

            // Assert
            duty.Id.Should().Be(1);
        }

        [Fact]
        public void AstronautDuty_ShouldHavePersonIdProperty()
        {
            // Arrange & Act
            var duty = new AstronautDuty { Id = 1, PersonId = 1, DutyTitle = "Pilot" };

            // Assert
            duty.PersonId.Should().Be(1);
        }

        [Fact]
        public void AstronautDuty_ShouldHaveRankProperty()
        {
            // Arrange & Act
            var duty = new AstronautDuty { Id = 1, Rank = "Lieutenant", DutyTitle = "Pilot" };

            // Assert
            duty.Rank.Should().Be("Lieutenant");
        }

        [Fact]
        public void AstronautDuty_ShouldHaveDutyTitleProperty()
        {
            // Arrange & Act
            var duty = new AstronautDuty { Id = 1, DutyTitle = "Commander" };

            // Assert
            duty.DutyTitle.Should().Be("Commander");
        }

        [Fact]
        public void AstronautDuty_ShouldHaveDutyStartDateProperty()
        {
            // Arrange
            var startDate = new DateTime(2025, 1, 15);

            // Act
            var duty = new AstronautDuty { Id = 1, DutyStartDate = startDate };

            // Assert
            duty.DutyStartDate.Should().Be(startDate);
        }

        [Fact]
        public void AstronautDuty_ShouldHaveDutyEndDateProperty()
        {
            // Arrange
            var endDate = new DateTime(2025, 3, 31);

            // Act
            var duty = new AstronautDuty { Id = 1, DutyEndDate = endDate };

            // Assert
            duty.DutyEndDate.Should().Be(endDate);
        }

        [Fact]
        public void AstronautDuty_ShouldAllowNullDutyEndDate()
        {
            // Arrange & Act
            var duty = new AstronautDuty { Id = 1, DutyTitle = "Pilot", DutyEndDate = null };

            // Assert
            duty.DutyEndDate.Should().BeNull();
        }

        [Fact]
        public void AstronautDuty_ShouldHavePersonNavigation()
        {
            // Arrange & Act
            var duty = new AstronautDuty { Id = 1, PersonId = 1 };

            // Assert
            duty.Person.Should().BeNull();
        }

        [Fact]
        public void AstronautDuty_ShouldAllowSettingPersonNavigation()
        {
            // Arrange
            var duty = new AstronautDuty { Id = 1, PersonId = 1 };
            var person = new Person { Id = 1, Name = "John Doe" };

            // Act
            duty.Person = person;

            // Assert
            duty.Person.Should().Be(person);
            duty.Person.Name.Should().Be("John Doe");
        }

        [Fact]
        public void AstronautDuty_ShouldHaveAllPropertiesSet()
        {
            // Arrange
            var startDate = new DateTime(2025, 1, 15);
            var endDate = new DateTime(2025, 3, 31);

            // Act
            var duty = new AstronautDuty
            {
                Id = 1,
                PersonId = 1,
                Rank = "Captain",
                DutyTitle = "Commander",
                DutyStartDate = startDate,
                DutyEndDate = endDate
            };

            // Assert
            duty.Id.Should().Be(1);
            duty.PersonId.Should().Be(1);
            duty.Rank.Should().Be("Captain");
            duty.DutyTitle.Should().Be("Commander");
            duty.DutyStartDate.Should().Be(startDate);
            duty.DutyEndDate.Should().Be(endDate);
        }
    }

    public class AstronautDetailDataModelTests
    {
        [Fact]
        public void AstronautDetail_ShouldHavePersonIdProperty()
        {
            // Arrange & Act
            var detail = new AstronautDetail { PersonId = 1 };

            // Assert
            detail.PersonId.Should().Be(1);
        }

        [Fact]
        public void AstronautDetail_ShouldHaveCurrentRankProperty()
        {
            // Arrange & Act
            var detail = new AstronautDetail { PersonId = 1, CurrentRank = "Lieutenant" };

            // Assert
            detail.CurrentRank.Should().Be("Lieutenant");
        }

        [Fact]
        public void AstronautDetail_ShouldHaveCurrentDutyTitleProperty()
        {
            // Arrange & Act
            var detail = new AstronautDetail { PersonId = 1, CurrentDutyTitle = "Pilot" };

            // Assert
            detail.CurrentDutyTitle.Should().Be("Pilot");
        }

        [Fact]
        public void AstronautDetail_ShouldHaveCareerStartDateProperty()
        {
            // Arrange
            var startDate = new DateTime(2025, 1, 15);

            // Act
            var detail = new AstronautDetail { PersonId = 1, CareerStartDate = startDate };

            // Assert
            detail.CareerStartDate.Should().Be(startDate);
        }

        [Fact]
        public void AstronautDetail_ShouldHaveCareerEndDateProperty()
        {
            // Arrange
            var endDate = new DateTime(2025, 4, 14);

            // Act
            var detail = new AstronautDetail { PersonId = 1, CareerEndDate = endDate };

            // Assert
            detail.CareerEndDate.Should().Be(endDate);
        }

        [Fact]
        public void AstronautDetail_ShouldAllowNullCareerEndDate()
        {
            // Arrange & Act
            var detail = new AstronautDetail { PersonId = 1, CareerEndDate = null };

            // Assert
            detail.CareerEndDate.Should().BeNull();
        }

        [Fact]
        public void AstronautDetail_ShouldHavePersonNavigation()
        {
            // Arrange & Act
            var detail = new AstronautDetail { PersonId = 1 };

            // Assert
            detail.Person.Should().BeNull();
        }

        [Fact]
        public void AstronautDetail_ShouldAllowSettingPersonNavigation()
        {
            // Arrange
            var detail = new AstronautDetail { PersonId = 1 };
            var person = new Person { Id = 1, Name = "Jane Doe" };

            // Act
            detail.Person = person;

            // Assert
            detail.Person.Should().Be(person);
            detail.Person.Name.Should().Be("Jane Doe");
        }

        [Fact]
        public void AstronautDetail_ShouldHaveAllPropertiesSet()
        {
            // Arrange
            var startDate = new DateTime(2025, 1, 15);
            var endDate = new DateTime(2025, 4, 14);

            // Act
            var detail = new AstronautDetail
            {
                PersonId = 1,
                CurrentRank = "General",
                CurrentDutyTitle = "Director",
                CareerStartDate = startDate,
                CareerEndDate = endDate
            };

            // Assert
            detail.PersonId.Should().Be(1);
            detail.CurrentRank.Should().Be("General");
            detail.CurrentDutyTitle.Should().Be("Director");
            detail.CareerStartDate.Should().Be(startDate);
            detail.CareerEndDate.Should().Be(endDate);
        }

        [Fact]
        public void AstronautDetail_ShouldAllowEmptyStringProperties()
        {
            // Arrange & Act
            var detail = new AstronautDetail
            {
                PersonId = 1,
                CurrentRank = "",
                CurrentDutyTitle = ""
            };

            // Assert
            detail.CurrentRank.Should().Be("");
            detail.CurrentDutyTitle.Should().Be("");
        }
    }

    public class DataModelRelationshipsTests
    {
        [Fact]
        public void Person_AndAstronautDuty_ShouldMaintainRelationship()
        {
            // Arrange
            var person = new Person { Id = 1, Name = "Test Person" };
            var duty = new AstronautDuty { Id = 1, PersonId = 1, DutyTitle = "Pilot" };

            // Act
            person.AstronautDuties.Add(duty);
            duty.Person = person;

            // Assert
            person.AstronautDuties.Should().Contain(duty);
            duty.Person.Should().Be(person);
            duty.PersonId.Should().Be(person.Id);
        }

        [Fact]
        public void Person_AndAstronautDetail_ShouldMaintainOneToOneRelationship()
        {
            // Arrange
            var person = new Person { Id = 1, Name = "Test Person" };
            var detail = new AstronautDetail { PersonId = 1 };

            // Act
            person.AstronautDetail = detail;
            detail.Person = person;

            // Assert
            person.AstronautDetail.Should().Be(detail);
            detail.Person.Should().Be(person);
            detail.PersonId.Should().Be(person.Id);
        }

        [Fact]
        public void MultipleAstronautDuties_ShouldBelongToSamePerson()
        {
            // Arrange
            var person = new Person { Id = 1, Name = "Test Person" };
            var duty1 = new AstronautDuty { Id = 1, PersonId = 1, DutyTitle = "Pilot" };
            var duty2 = new AstronautDuty { Id = 2, PersonId = 1, DutyTitle = "Commander" };

            // Act
            person.AstronautDuties.Add(duty1);
            person.AstronautDuties.Add(duty2);

            // Assert
            person.AstronautDuties.Should().HaveCount(2);
            person.AstronautDuties.Should().AllSatisfy(d => d.PersonId.Should().Be(1));
        }
    }
}
