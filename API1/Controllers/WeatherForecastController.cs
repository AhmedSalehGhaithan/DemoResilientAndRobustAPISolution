using Microsoft.AspNetCore.Mvc;
using Polly;
using Polly.Retry;

namespace API1.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController(HttpClient httpClient) : ControllerBase
    {

        [HttpGet(Name = "GetWeatherForecast")]
        public async Task<ActionResult<IEnumerable<WeatherForecast>>> Get()
        {
            try
            {
                // you can call any policy => the private method below
                var policy = waitForEverAndRetry();
                var result = await policy.ExecuteAsync(ConnectToAPI);
                if (result.IsSuccessStatusCode)
                    return Ok(await result.Content.ReadFromJsonAsync<WeatherForecast[]>());
                throw new Exception();
            }
            catch
            {
                return BadRequest("Sorry error occurred");
            }
            

        }

        private AsyncRetryPolicy waitForEverAndRetry()
        {
            TimeSpan waitTime = TimeSpan.FromSeconds(10);
            var waitForEverAndRetry = Policy.Handle<Exception>().WaitAndRetryForeverAsync(sleepDurationProvider: retryAttempt => waitTime,
                                                        onRetry: (exception, retryCount) =>
                                                        {
                                                            Console.WriteLine($"Error : {exception.Message} - Retry : {retryCount}");
                                                        });
            return waitForEverAndRetry;
        }

        private AsyncRetryPolicy waitAndRetry()
        {
            TimeSpan waitTime = TimeSpan.FromSeconds(10);
            var waitAndRetry = Policy.Handle<Exception>().WaitAndRetryAsync(retryCount: 3, sleepDurationProvider: retryAttempt => waitTime,
                                onRetry: (exception, retryCount) =>
                                {
                                    Console.WriteLine($"Error : {exception.Message} - Retry : {retryCount}");
                                });
            return waitAndRetry;
        }
        private AsyncRetryPolicy retryForeverPolicy()
        {
            var retryForeverPolicy = Policy.Handle<Exception>().RetryForeverAsync((exception, retryCount, context) =>
            {
                Console.WriteLine($"Error : {exception.Message} - Retry : {retryCount}");
            });
            return retryForeverPolicy;
        }
        private AsyncRetryPolicy retryPolicy()
        {
            var retryPolicy = Policy.Handle<Exception>().RetryAsync(retryCount: 3, onRetry: (exception, retryCount) =>
            {
                Console.WriteLine($"Error : {exception.Message} - Retry : {retryCount}");
            });
            return retryPolicy;
        }
        private async Task<HttpResponseMessage> ConnectToAPI()
        {
            var response = await httpClient.GetAsync("https://localhost:7119/WeatherForecast");
            return response;
        }
    }
}
