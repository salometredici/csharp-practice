namespace Transactions.Domain.Transactions
{
    public class TransactionResponse
    {
        public int Id { get; set; }
        public int AccountFrom { get; set; }
        public string OriginCurrency { get; set; }
        public int AccountTo { get; set; }
        public string DestCurrency { get; set; }
        public double Amount { get; set; }
        public DateTime Date { get; set; }
        public string Description { get; set; }
    }
}
