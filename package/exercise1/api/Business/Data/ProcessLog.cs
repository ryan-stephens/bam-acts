using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.ComponentModel.DataAnnotations.Schema;

namespace StargateAPI.Business.Data
{
    [Table("ProcessLog")]
    public class ProcessLog
    {
        public int Id { get; set; }

        public DateTime Timestamp { get; set; }

        public string LogLevel { get; set; } = string.Empty;

        public string Message { get; set; } = string.Empty;

        public string? ExceptionDetails { get; set; }

        public string? StackTrace { get; set; }

        public string? RequestPath { get; set; }

        public string? RequestMethod { get; set; }
    }

    public class ProcessLogConfiguration : IEntityTypeConfiguration<ProcessLog>
    {
        public void Configure(EntityTypeBuilder<ProcessLog> builder)
        {
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id).ValueGeneratedOnAdd();
            builder.Property(x => x.Timestamp).IsRequired();
            builder.Property(x => x.LogLevel).IsRequired().HasMaxLength(50);
            builder.Property(x => x.Message).IsRequired();
        }
    }
}
