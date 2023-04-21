namespace Transactions.Domain.Accounts
{
    public class AccountResponse
    {
        public int Id { get; set; }
        public int CurrencyId { get; set; }
        public string CurrencyCode { get; set; }
        public string Currency { get; set; }
        public double Balance { get; set; }
        public int UserId { get; set; }
        public string FullName { get; set; }
    }
}
