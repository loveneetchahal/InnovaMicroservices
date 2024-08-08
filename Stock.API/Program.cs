using MassTransit;
using Stock.API.Consumers;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
//builder.Services.AddHostedService<OrderCreatedEventConsumer>();

builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<OrderCreatedEventConsumerWithMasstransit>();

    x.UsingRabbitMq((context, configure) =>
    {
        configure.UseMessageRetry(r => r.Interval(3, TimeSpan.FromSeconds(5)));
        configure.Host(new Uri(builder.Configuration.GetConnectionString("RabbitMQ")!));


        configure.ReceiveEndpoint("stock.api.masstransit.order.created.event.queue",
            e => { e.ConfigureConsumer<OrderCreatedEventConsumerWithMasstransit>(context); });
    });
});
var app = builder.Build();

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