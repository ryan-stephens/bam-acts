using Microsoft.EntityFrameworkCore;
using StargateAPI.Business.Commands;
using StargateAPI.Business.Data;
using StargateAPI.Business.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddDbContext<StargateContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("StarbaseApiDatabase")));

// Register logging service
builder.Services.AddScoped<IProcessLogService, ProcessLogService>();

builder.Services.AddMediatR(cfg =>
{
    cfg.AddRequestPreProcessor<CreateAstronautDutyPreProcessor>();
    cfg.AddRequestPreProcessor<CreatePersonPreProcessor>();
    cfg.RegisterServicesFromAssemblies(typeof(Program).Assembly);
});

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngularApp", policy =>
    {
        policy.WithOrigins("http://localhost:4200", "https://localhost:4200", "http://localhost:5204")
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// Ensure database is created and migrations are applied
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<StargateContext>();
    db.Database.Migrate();

    // Manually create unique index on Person.Name if it doesn't exist (Rule 1)
    var connection = db.Database.GetDbConnection();
    connection.Open();
    using (var command = connection.CreateCommand())
    {
        command.CommandText = @"
            CREATE UNIQUE INDEX IF NOT EXISTS IX_Person_Name
            ON Person (Name);

            CREATE TABLE IF NOT EXISTS ProcessLog (
                Id INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
                Timestamp TEXT NOT NULL,
                LogLevel TEXT NOT NULL,
                Message TEXT NOT NULL,
                ExceptionDetails TEXT NULL,
                StackTrace TEXT NULL,
                RequestPath TEXT NULL,
                RequestMethod TEXT NULL
            );
        ";
        command.ExecuteNonQuery();
    }
    connection.Close();

    // Seed initial data if database is empty
    if (!db.People.Any())
    {
        var person1 = new Person { Name = "John Doe" };
        var person2 = new Person { Name = "Jane Doe" };
        db.People.AddRange(person1, person2);
        db.SaveChanges();

        var astronautDetail = new AstronautDetail
        {
            PersonId = person1.Id,
            CurrentRank = "1LT",
            CurrentDutyTitle = "Commander",
            CareerStartDate = DateTime.Now
        };
        db.AstronautDetails.Add(astronautDetail);

        var astronautDuty = new AstronautDuty
        {
            PersonId = person1.Id,
            DutyStartDate = DateTime.Now,
            DutyTitle = "Commander",
            Rank = "1LT"
        };
        db.AstronautDuties.Add(astronautDuty);

        db.SaveChanges();
    }
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors("AllowAngularApp");

app.UseAuthorization();

app.MapControllers();

app.Run();


