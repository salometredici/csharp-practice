using MediatR;
using System.Net;
using Transactions.Domain.Users;
using Transactions.Infrastructure;

namespace Transactions.Application.Users
{
    public class LoginCommand : IRequest<UserResponse>
    {
        public LoginRequest Request { get; set; }

        public LoginCommand(LoginRequest request) => Request = request;
    }

    public class LoginCommandHandler : IRequestHandler<LoginCommand, UserResponse>
    {
        private readonly ITransactionsRepository _transactionsRepository;

        public LoginCommandHandler(ITransactionsRepository transactionsRepository) => _transactionsRepository = transactionsRepository;

        public async Task<UserResponse> Handle(LoginCommand command, CancellationToken cancellationToken)
        {
            var request = command.Request;

            var user = await _transactionsRepository.GetUserByEmailAsync(request.Email);
            if (user == null)
            {
                throw new HttpRequestException("Email not registered", null, HttpStatusCode.NotFound);
            }
            else if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PwdHash))
            {
                throw new HttpRequestException("Invalid password", null, HttpStatusCode.Unauthorized);
            }

            user.LastLoginDate = DateTime.Now;
            await _transactionsRepository.LoginAsync(request.Email, user.LastLoginDate);
            //<- devolver jwt

            return user.ToResponse();
        }
    }
}
