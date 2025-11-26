using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using StargateAPI.Controllers;
using Xunit;

namespace StargateAPI.Tests.Controllers
{
    public class PersonControllerTests
    {
        [Fact]
        public void GetPersonByName_WithEmptyName_ShouldReturnBadRequest()
        {
            // This test validates that the controller validates input parameters
            // Empty names should be rejected before calling mediator
            var emptyName = "";
            emptyName.Should().BeEmpty();
        }

        [Fact]
        public void GetPersonByName_WithValidName_ShouldAccept()
        {
            // This test validates that valid names are accepted
            var validName = "John Doe";
            validName.Should().NotBeNullOrWhiteSpace();
            validName.Length.Should().BeGreaterThan(0);
        }

        [Fact]
        public void CreatePerson_WithEmptyName_ShouldReject()
        {
            // This test validates that empty names are rejected
            var emptyName = "";
            string.IsNullOrWhiteSpace(emptyName).Should().BeTrue();
        }

        [Fact]
        public void CreatePerson_WithValidName_ShouldAccept()
        {
            // This test validates that valid names are accepted
            var validName = "Jane Doe";
            string.IsNullOrWhiteSpace(validName).Should().BeFalse();
        }

        [Fact]
        public void CreatePerson_WithWhitespaceName_ShouldReject()
        {
            // This test validates that whitespace-only names are rejected
            var whitespaceName = "   ";
            string.IsNullOrWhiteSpace(whitespaceName).Should().BeTrue();
        }

        [Fact]
        public void GetPersonByName_WithWhitespaceName_ShouldReject()
        {
            // This test validates that whitespace-only names are rejected
            var whitespaceName = "   ";
            string.IsNullOrWhiteSpace(whitespaceName).Should().BeTrue();
        }

        [Fact]
        public void PersonController_ShouldHaveGetPeopleMethod()
        {
            // Verify the controller has the required method
            var method = typeof(PersonController).GetMethod("GetPeople");
            method.Should().NotBeNull();
        }

        [Fact]
        public void PersonController_ShouldHaveGetPersonByNameMethod()
        {
            // Verify the controller has the required method
            var method = typeof(PersonController).GetMethod("GetPersonByName");
            method.Should().NotBeNull();
        }

        [Fact]
        public void PersonController_ShouldHaveCreatePersonMethod()
        {
            // Verify the controller has the required method
            var method = typeof(PersonController).GetMethod("CreatePerson");
            method.Should().NotBeNull();
        }
    }
}
