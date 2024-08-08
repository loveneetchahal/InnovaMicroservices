using Polly;
using Polly.CircuitBreaker;
using Polly.Retry;

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


        public async Task ManuelPollyRetry()
        {
            var httpClient = new HttpClient();


            ResiliencePipeline pipeline = new ResiliencePipelineBuilder().AddRetry(new RetryStrategyOptions()
            {
                MaxRetryAttempts = 3,
                Delay = TimeSpan.FromSeconds(3)
            }).AddCircuitBreaker(new CircuitBreakerStrategyOptions()).AddTimeout(TimeSpan.FromSeconds(10)).Build();


            await pipeline.ExecuteAsync(async ct =>
            {
                var response = await httpClient.GetAsync("https://www.google.com", ct);
            });
        }
    }
}