using Transactions.Domain;

namespace Transactions.Infrastructure
{
    public interface ICurrenciesService
    {
        Task<CurrencyConversionResponse> ConvertAmountAsync(float amount, string currFrom, string currTo);
    }

    public class CurrenciesService : ICurrenciesService
    {
        private readonly ICurrenciesClient _currenciesClient;

        public CurrenciesService(ICurrenciesClient httpCurrenciesService)
        {
            _currenciesClient = httpCurrenciesService;
        }

        public async Task<CurrencyConversionResponse> ConvertAmountAsync(float amount, string currFrom, string currTo)
        {
            var result = await RateLimiterPolly.Throttle(async () =>
                await _currenciesClient.ConvertAmountAsync(amount, currFrom, currTo)
            );
            return result;
        }
    }
}
