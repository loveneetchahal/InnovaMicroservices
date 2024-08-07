namespace ServiceBus
{
    public record OrderCreatedEvent(int OrderId, decimal TotalPrice, int StockCount);
}