﻿using MassTransit;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Order.API.Models;
using Order.API.Services;
using RabbitMQ.Client;
using ServiceBus;


namespace Order.API.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class OrderController(
        ServiceBus.IBus bus,
        IPublishEndpoint publishEndpoint,
        StockService stockService,
        AppDbContext context)
        : ControllerBase
    {
        [HttpGet]
        public async Task<IActionResult> GetProductList()
        {
            return Ok(context.Products.ToList());
        }


        [HttpGet]
        public async Task<IActionResult> Create4FromSyncCommunication()
        {
            var stockCheck = await stockService.StockCheck(10);

            return Ok(new { hasStock = stockCheck });
        }


        [HttpGet]
        public IActionResult Create3FromAsyncCommunication()
        {
            var orderCreatedEvent = new OrderCreatedEvent(1, 500, 10);

            CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
            cancellationTokenSource.CancelAfter(TimeSpan.FromSeconds(50));
            publishEndpoint.Publish(orderCreatedEvent, cancellationTokenSource.Token);

            return Ok();
        }


        [HttpGet]
        public IActionResult Create2FromAsyncCommunication()
        {
            var orderCreatedEvent = new OrderCreatedEvent(1, 500, 10);


            bus.PublishAsDirect(orderCreatedEvent, BusConst.OrderCreatedEventExchangeName, ExchangeType.Direct,
                BusConst.OrderCreatedEventRouteKey);


            return Ok();
        }


        [HttpGet]
        public IActionResult CreateFromAsyncCommunication()
        {
            // db operations (create)

            // publish event


            Enumerable.Range(1, 50).ToList().ForEach(x =>
            {
                var orderCreatedEvent = new OrderCreatedEvent(x, 500, 10);
                bus.Publish(orderCreatedEvent, BusConst.OrderCreatedEventExchangeName, ExchangeType.Fanout);
            });


            //bus.Publish(orderCreatedEvent, "stock.api.order.created.event.queue");
            //bus.Publish(orderCreatedEvent, "notification.api.order.created.event.queue");
            //bus.Publish(orderCreatedEvent, "sms.api.order.created.event.queue");
            return Ok();
        }
    }
}