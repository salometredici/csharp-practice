namespace Transactions.Domain.Authentication
{
    public class JwtTokenResponse
    {
        public string BearerToken { get; set; }
        public DateTime ExpirationDate { get; set; }
    }
}
