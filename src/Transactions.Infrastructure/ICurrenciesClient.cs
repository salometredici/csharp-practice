using Refit;
using Transactions.Domain;

namespace Transactions.Infrastructure
{
    public interface ICurrenciesClient
	{
		[Get("/fixer/convert")]
        Task<CurrencyConversionResponse> ConvertAmountAsync([Query] float amount, [Query] string from, [Query] string to);
	}
}
