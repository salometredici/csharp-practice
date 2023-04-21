namespace Transactions.Domain.Users
{
    public class UserDto
    {
        public int UserId { get; set; }
        public string UserName { get; set; }
        public string UserSurname { get; set; }
        public string UserEmail { get; set; }
        public DateTime CreationDate { get; set; }
        public DateTime? LastLoginDate { get; set; }
        public string PwdHash { get; set; }

        public UserResponse ToResponse()
        {
            return new UserResponse()
            {
                Id = UserId,
                FullName = $"{UserName} {UserSurname}",
                Email = UserEmail,
                CreationDate = CreationDate,
                LastLoginDate = LastLoginDate
            };
        }
    }
}
