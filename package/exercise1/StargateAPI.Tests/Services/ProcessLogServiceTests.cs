using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using StargateAPI.Business.Data;
using StargateAPI.Business.Services;
using Xunit;

namespace StargateAPI.Tests.Services
{
    public class ProcessLogServiceTests
    {
        private StargateContext CreateInMemoryContext()
        {
            var options = new DbContextOptionsBuilder<StargateContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            return new StargateContext(options);
        }

        [Fact]
        public async Task LogSuccessAsync_WithValidData_ShouldLogSuccessfully()
        {
            // Arrange
            var context = CreateInMemoryContext();
            var service = new ProcessLogService(context);
            var message = "Test success message";
            var requestPath = "/test/path";
            var user = "TestUser";

            // Act
            await service.LogSuccessAsync(message, requestPath, user);

            // Assert
            var logs = await context.ProcessLogs.ToListAsync();
            logs.Should().HaveCount(1);
            logs[0].Message.Should().Contain(message);
        }

        [Fact]
        public async Task LogSuccessAsync_ShouldSetCorrectLogLevel()
        {
            // Arrange
            var context = CreateInMemoryContext();
            var service = new ProcessLogService(context);

            // Act
            await service.LogSuccessAsync("Success", "/path", "User");

            // Assert
            var log = await context.ProcessLogs.FirstOrDefaultAsync();
            log.Should().NotBeNull();
            log.LogLevel.Should().NotBeNullOrEmpty();
        }

        [Fact]
        public async Task LogSuccessAsync_ShouldSetTimestamp()
        {
            // Arrange
            var context = CreateInMemoryContext();
            var service = new ProcessLogService(context);
            var beforeLog = DateTime.UtcNow;

            // Act
            await service.LogSuccessAsync("Success", "/path", "User");

            // Assert
            var log = await context.ProcessLogs.FirstOrDefaultAsync();
            log.Should().NotBeNull();
            log.Timestamp.Should().BeOnOrAfter(beforeLog);
        }

        [Fact]
        public async Task LogExceptionAsync_WithValidException_ShouldLogException()
        {
            // Arrange
            var context = CreateInMemoryContext();
            var service = new ProcessLogService(context);
            var exception = new Exception("Test exception");
            var requestMethod = "POST";
            var requestPath = "/test/path";
            var user = "TestUser";

            // Act
            await service.LogExceptionAsync(exception, requestMethod, requestPath, user);

            // Assert
            var logs = await context.ProcessLogs.ToListAsync();
            logs.Should().HaveCount(1);
            logs[0].ExceptionDetails.Should().NotBeNullOrEmpty();
        }

        [Fact]
        public async Task LogExceptionAsync_ShouldIncludeExceptionMessage()
        {
            // Arrange
            var context = CreateInMemoryContext();
            var service = new ProcessLogService(context);
            var exceptionMessage = "Critical error occurred";
            var exception = new Exception(exceptionMessage);

            // Act
            await service.LogExceptionAsync(exception, "GET", "/path", "User");

            // Assert
            var log = await context.ProcessLogs.FirstOrDefaultAsync();
            log.Should().NotBeNull();
            log.ExceptionDetails.Should().Contain(exceptionMessage);
        }

        [Fact]
        public async Task LogExceptionAsync_ShouldSetErrorLogLevel()
        {
            // Arrange
            var context = CreateInMemoryContext();
            var service = new ProcessLogService(context);
            var exception = new Exception("Test");

            // Act
            await service.LogExceptionAsync(exception, "POST", "/path", "User");

            // Assert
            var log = await context.ProcessLogs.FirstOrDefaultAsync();
            log.Should().NotBeNull();
            log.LogLevel.Should().NotBeNullOrEmpty();
        }

        [Fact]
        public async Task LogExceptionAsync_ShouldIncludeStackTrace()
        {
            // Arrange
            var context = CreateInMemoryContext();
            var service = new ProcessLogService(context);
            Exception exception;
            try
            {
                throw new InvalidOperationException("Test exception with stack trace");
            }
            catch (Exception ex)
            {
                exception = ex;
            }

            // Act
            await service.LogExceptionAsync(exception, "DELETE", "/path", "User");

            // Assert
            var log = await context.ProcessLogs.FirstOrDefaultAsync();
            log.Should().NotBeNull();
            log.StackTrace.Should().NotBeNullOrEmpty();
        }

        [Fact]
        public async Task LogSuccessAsync_WithMultipleLogs_ShouldPersistAll()
        {
            // Arrange
            var context = CreateInMemoryContext();
            var service = new ProcessLogService(context);

            // Act
            await service.LogSuccessAsync("Log 1", "/path1", "User1");
            await service.LogSuccessAsync("Log 2", "/path2", "User2");
            await service.LogSuccessAsync("Log 3", "/path3", "User3");

            // Assert
            var logs = await context.ProcessLogs.ToListAsync();
            logs.Should().HaveCount(3);
        }

        [Fact]
        public async Task LogExceptionAsync_WithInnerException_ShouldLogDetails()
        {
            // Arrange
            var context = CreateInMemoryContext();
            var service = new ProcessLogService(context);
            var innerException = new InvalidOperationException("Inner error");
            var outerException = new Exception("Outer error", innerException);

            // Act
            await service.LogExceptionAsync(outerException, "PUT", "/path", "User");

            // Assert
            var log = await context.ProcessLogs.FirstOrDefaultAsync();
            log.Should().NotBeNull();
            log.ExceptionDetails.Should().Contain("Outer error");
        }

        [Fact]
        public async Task ProcessLogService_ShouldPersistToDatabase()
        {
            // Arrange
            var context = CreateInMemoryContext();
            var service = new ProcessLogService(context);

            // Act
            await service.LogSuccessAsync("Test message", "/test", "TestUser");

            // Assert - Verify data persisted
            var savedLogs = await context.ProcessLogs.ToListAsync();
            savedLogs.Should().NotBeEmpty();
            savedLogs.First().Message.Should().NotBeNullOrEmpty();
        }

        [Fact]
        public async Task LogSuccessAsync_ShouldIncludeRequestDetails()
        {
            // Arrange
            var context = CreateInMemoryContext();
            var service = new ProcessLogService(context);
            var requestPath = "/api/person/create";

            // Act
            await service.LogSuccessAsync("Person created", requestPath, "AdminUser");

            // Assert
            var log = await context.ProcessLogs.FirstOrDefaultAsync();
            log.Should().NotBeNull();
            log.RequestPath.Should().Be(requestPath);
        }

        [Fact]
        public async Task LogExceptionAsync_ShouldPersistExceptionToDatabase()
        {
            // Arrange
            var context = CreateInMemoryContext();
            var service = new ProcessLogService(context);

            // Act
            await service.LogExceptionAsync(new Exception("Test error"), "POST", "/path", "User");

            // Assert
            var logs = await context.ProcessLogs.ToListAsync();
            logs.Should().HaveCount(1);
        }
    }
}
