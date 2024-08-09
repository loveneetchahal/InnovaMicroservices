using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Order.API.Models;
using Order.API.Services;
using Polly;
using Polly.Extensions.Http;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("SqlServer"));
});
builder.Services.AddHttpClient<StockService>(x =>
{
    x.BaseAddress = new Uri(builder.Configuration.GetSection("MicroservicesAddress")["StockBaseUrl"]!);
}).AddPolicyHandler(timeoutPolicy(3)).AddPolicyHandler(RetryPolicy()).AddPolicyHandler(AdvancedCircuitPolicy());

static IAsyncPolicy<HttpResponseMessage> RetryPolicy()
{
    return HttpPolicyExtensions.HandleTransientHttpError().WaitAndRetryAsync(10,
        (_) => TimeSpan.FromSeconds(5));
}

static IAsyncPolicy<HttpResponseMessage> CircuitPolicy()
{
    return HttpPolicyExtensions.HandleTransientHttpError().CircuitBreakerAsync(2, TimeSpan.FromSeconds(30));
}

static IAsyncPolicy<HttpResponseMessage> AdvancedCircuitPolicy()
{
    return HttpPolicyExtensions.HandleTransientHttpError()
        .AdvancedCircuitBreakerAsync(0.5, TimeSpan.FromSeconds(20), 2, TimeSpan.FromSeconds(30));
}


builder.Services.AddMassTransit(x =>
{
    x.UsingRabbitMq((context, configure) =>
    {
        configure.Host(new Uri(builder.Configuration.GetConnectionString("RabbitMQ")!));
    });
});

builder.Services.AddSingleton<ServiceBus.IBus, ServiceBus.RabbitMqBus>();
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
}

app.UseSwagger();
app.UseSwaggerUI();
//app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
return;

static IAsyncPolicy<HttpResponseMessage> timeoutPolicy(int seconds)
{
    return Policy.TimeoutAsync<HttpResponseMessage>(seconds);
}