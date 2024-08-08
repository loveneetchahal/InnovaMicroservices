namespace Order.API.Services
{
    public class StockService(HttpClient client)
    {
        public async Task<bool> StockCheck(int productId)
        {
            //https://localhost:7210/api/Stock/StockCheck/2

            var response = await client.GetAsync($"/api/Stock/StockCheck/{productId}");


            var stockCheckResponse =
                await client.GetFromJsonAsync<StockCheckResponseDto>($"/api/Stock/StockCheck/{productId}");

            return stockCheckResponse!.StockStatus;
        }
    }
}