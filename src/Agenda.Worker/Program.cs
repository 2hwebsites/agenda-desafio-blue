using Agenda.Worker;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.Configure<RabbitMqOptions>(builder.Configuration.GetSection("RabbitMq"));
builder.Services.AddHostedService<ContactCreatedConsumer>();

var host = builder.Build();
await host.RunAsync();
