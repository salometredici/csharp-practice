namespace Transactions.Domain.Users
{
    public class UserWithTokenResponse : UserResponse
    {
        public string JwtToken { get; set; }
        public DateTime TokenExpDate { get; set; }

        public UserWithTokenResponse(UserResponse user)
        {
            Id = user.Id;
            FullName = user.FullName;
            Email = user.Email;
            CreationDate = user.CreationDate;
            LastLoginDate = user.LastLoginDate;
        }
    }
}
