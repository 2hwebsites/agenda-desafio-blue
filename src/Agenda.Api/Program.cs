using Agenda.Api.Common;
using Agenda.Api.Extensions;
using Agenda.Api.Infrastructure;
using Agenda.Application;
using Agenda.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddApiAuthentication(builder.Configuration);
builder.Services.AddApiDocumentation();

builder.Services.AddControllers();
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

var app = builder.Build();

if (app.Configuration.GetValue<bool>("RUN_MIGRATIONS", app.Environment.IsDevelopment()))
    await DbInitializer.InitializeAsync(app.Services);

app.UseExceptionHandler();

app.UseSwagger();
app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Agenda API v1"));

app.MapGet("/health", () => Results.Ok(new { status = "healthy", timestamp = DateTime.UtcNow }))
    .WithName("Health").WithTags("Infra").WithSummary("Returns 200 OK when the API is running");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

public partial class Program;
