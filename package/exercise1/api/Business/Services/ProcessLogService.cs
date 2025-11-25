using StargateAPI.Business.Data;

namespace StargateAPI.Business.Services
{
    public interface IProcessLogService
    {
        Task LogSuccessAsync(string message, string? requestPath = null, string? requestMethod = null);
        Task LogExceptionAsync(Exception exception, string message, string? requestPath = null, string? requestMethod = null);
    }

    public class ProcessLogService : IProcessLogService
    {
        private readonly StargateContext _context;

        public ProcessLogService(StargateContext context)
        {
            _context = context;
        }

        public async Task LogSuccessAsync(string message, string? requestPath = null, string? requestMethod = null)
        {
            var log = new ProcessLog
            {
                Timestamp = DateTime.UtcNow,
                LogLevel = "Success",
                Message = message,
                RequestPath = requestPath,
                RequestMethod = requestMethod
            };

            _context.ProcessLogs.Add(log);
            await _context.SaveChangesAsync();
        }

        public async Task LogExceptionAsync(Exception exception, string message, string? requestPath = null, string? requestMethod = null)
        {
            var log = new ProcessLog
            {
                Timestamp = DateTime.UtcNow,
                LogLevel = "Error",
                Message = message,
                ExceptionDetails = exception.Message,
                StackTrace = exception.StackTrace,
                RequestPath = requestPath,
                RequestMethod = requestMethod
            };

            _context.ProcessLogs.Add(log);
            await _context.SaveChangesAsync();
        }
    }
}
