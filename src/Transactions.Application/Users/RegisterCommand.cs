using MediatR;
using Transactions.Domain;
using Transactions.Domain.Users;
using Transactions.Infrastructure;

namespace Transactions.Application.Users
{
    public class RegisterCommand : IRequest<Unit>
    {
        public RegisterRequest Request { get; set; }

        public RegisterCommand(RegisterRequest request) => Request = request;
    }

    public class RegisterCommandHandler : IRequestHandler<RegisterCommand, Unit>
    {
        private readonly ITransactionsRepository _transactionsRepository;

        public RegisterCommandHandler(ITransactionsRepository transactionsRepository) => _transactionsRepository = transactionsRepository;

        public async Task<Unit> Handle(RegisterCommand command, CancellationToken cancellationToken)
        {
            var user = await _transactionsRepository.GetUserByEmailAsync(command.Request.Email);
            if (user != null)
            {
                throw new HttpException("Email already registered", 409);
            }

            await _transactionsRepository.RegisterAsync(command.Request, GetPwdHash(command.Request.Password));
            return Unit.Value;
        }

        private string GetPwdHash(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password);
        }
    }
}
