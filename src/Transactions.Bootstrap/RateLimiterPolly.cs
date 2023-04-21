using Polly;
using Polly.RateLimit;
using Transactions.Domain;

namespace Transactions.Bootstrap
{
    public static class RateLimiterPolly
    {
        /// <summary>
        /// Limitamos las llamadas a la API externa a 2 por minuto
        /// </summary>
        public static AsyncRateLimitPolicy _throttlingPolicy = Policy.RateLimitAsync(1, TimeSpan.FromMinutes(1));

        public static async Task<T> Throttle<T>(Func<Task<T>> func)
        {
            var result = await _throttlingPolicy.ExecuteAndCaptureAsync(func);
            if (result.Outcome == OutcomeType.Failure)
            {
                throw new HttpException(result.FinalException.Message, 429);
            }
            return result.Result;
        }
    }
}
