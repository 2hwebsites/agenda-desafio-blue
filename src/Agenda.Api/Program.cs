using Agenda.Api.Common;
using Agenda.Application;
using Agenda.Domain.Entities;
using Agenda.Infrastructure;
using Agenda.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new()
    {
        Title = "Agenda API", Version = "v1",
        Description = "Contact management API — Tech Lead technical challenge",
    });
});

builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<AgendaDbContext>();
    await db.Database.MigrateAsync();

    if (!await db.Contacts.AnyAsync())
    {
        db.Contacts.AddRange(
            Contact.Create("Ana Silva", "ana.silva@example.com", "(11) 91234-5678"),
            Contact.Create("Bruno Costa", "bruno.costa@example.com", "(21) 98765-4321"),
            Contact.Create("Carla Mendes", "carla.mendes@example.com"));
        await db.SaveChangesAsync();
    }
}

app.UseExceptionHandler();

app.UseSwagger();
app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Agenda API v1"));

app.MapGet("/health", () => Results.Ok(new { status = "healthy", timestamp = DateTime.UtcNow }))
    .WithName("Health").WithTags("Infra").WithSummary("Returns 200 OK when the API is running");

app.MapControllers();

app.Run();

public partial class Program;
