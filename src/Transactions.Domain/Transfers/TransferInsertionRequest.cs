namespace Transactions.Domain.Transfers
{
    public class TransferInsertionRequest : TransferRequest
    {
        public string OriginCurrencyCode { get; set; }
        public string DestCurrencyCode { get; set; }

        public float AmountToAddOnDestAcc { get; set; }
        public float CommissionAmount { get; set; }

        public TransferInsertionRequest(TransferRequest request)
        {
            AccountFrom = request.AccountFrom;
            AccountTo = request.AccountTo;
            Amount = request.Amount;
            Date = request.Date;
            Description = request.Description;
        }
    }
}
