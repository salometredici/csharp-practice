namespace Transactions.Domain.Accounts
{
    public class AccountDto
    {
        public int AccountId { get; set; }
        public int CurrencyId { get; set; }
        public string CurrencyCode { get; set; }
        public string Currency { get; set; }
        public double Balance { get; set; }
        public int UserId { get; set; }
        public string UserFullName { get; set; }

        public AccountResponse ToResponse()
        {
            return new AccountResponse()
            {
                Id = AccountId,
                CurrencyId = CurrencyId,
                CurrencyCode = CurrencyCode,
                Currency = Currency,
                Balance = Balance,
                UserId = UserId,
                FullName = UserFullName
            };
        }
    }
}
