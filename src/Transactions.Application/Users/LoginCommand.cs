using MediatR;
using Transactions.Domain;
using Transactions.Domain.Users;
using Transactions.Infrastructure;

namespace Transactions.Application.Users
{
    public class LoginCommand : IRequest<UserWithTokenResponse>
    {
        public LoginRequest Request { get; set; }

        public LoginCommand(LoginRequest request) => Request = request;
    }

    public class LoginCommandHandler : IRequestHandler<LoginCommand, UserWithTokenResponse>
    {
        private readonly IJwtProvider _jwtProvider;
        private readonly ITransactionsRepository _transactionsRepository;

        public LoginCommandHandler(IJwtProvider jwtProvider, ITransactionsRepository transactionsRepository)
        {
            _jwtProvider = jwtProvider;
            _transactionsRepository = transactionsRepository;
        }

        public async Task<UserWithTokenResponse> Handle(LoginCommand command, CancellationToken cancellationToken)
        {
            var request = command.Request;

            var user = await _transactionsRepository.GetUserByEmailAsync(request.Email);
            if (user == null)
            {
                throw new HttpException("Email not registered", 404);
            }
            else if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PwdHash))
            {
                throw new HttpException("Invalid password", 401);
            }

            user.LastLoginDate = DateTime.Now;
            await _transactionsRepository.LoginAsync(request.Email, user.LastLoginDate);

            var tokenResponse = _jwtProvider.GetJwtTokenResponse(user);

            return new UserWithTokenResponse(user.ToResponse())
            {
                JwtToken = tokenResponse.BearerToken,
                TokenExpDate = tokenResponse.ExpirationDate
            };
        }
    }
}
