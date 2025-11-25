using Microsoft.EntityFrameworkCore;
using StargateAPI.Business.Commands;
using StargateAPI.Business.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddDbContext<StargateContext>(options => 
    options.UseSqlite(builder.Configuration.GetConnectionString("StarbaseApiDatabase")));

builder.Services.AddMediatR(cfg =>
{
    cfg.AddRequestPreProcessor<CreateAstronautDutyPreProcessor>();
    cfg.RegisterServicesFromAssemblies(typeof(Program).Assembly);
});

var app = builder.Build();

// Ensure database is created and migrations are applied
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<StargateContext>();
    db.Database.Migrate();

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

app.UseAuthorization();

app.MapControllers();

app.Run();


