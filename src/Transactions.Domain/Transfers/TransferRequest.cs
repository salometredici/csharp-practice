namespace Transactions.Domain.Transfers
{
    public class TransferRequest
    {
        public int AccountFrom { get; set; }
        public int AccountTo { get; set; }
        public float Amount { get; set; }
        public DateTime Date { get; set; }
        public string Description { get; set; }
    }
}
