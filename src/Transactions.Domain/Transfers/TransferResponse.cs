namespace Transactions.Domain.Transfers
{
    public class TransferResponse
    {
        public int TransactionId { get; set; }
        public string AmountDebited { get; set; }
        public string CommissionDebited { get; set; }
        public string AmountTransferred { get; set; }
    }
}
