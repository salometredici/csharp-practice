namespace Transactions.Domain.Transfers
{
    public class TransferDto
    {
        public int TransactionId { get; set; }
        public float AmountDebited { get; set; }
        public float CommissionDebited { get; set; }
        public float AmountTransferred { get; set; }

        public TransferResponse ToResponse(string originCurrencyCode, string destCurrencyCode)
        {
            return new TransferResponse()
            {
                TransactionId = TransactionId,
                AmountDebited = $"{AmountDebited} {originCurrencyCode}",
                CommissionDebited = $"{CommissionDebited} {originCurrencyCode}",
                AmountTransferred = $"{AmountTransferred} {destCurrencyCode}"
            };
        }
    }
}
