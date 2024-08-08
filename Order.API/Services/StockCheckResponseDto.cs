namespace Order.API.Services
{
    public record StockCheckResponseDto(int ProductId, bool StockStatus);
}