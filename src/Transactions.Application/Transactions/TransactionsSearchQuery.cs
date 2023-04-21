using MediatR;
using System.Security.Claims;
using Transactions.Domain.Transactions;
using Transactions.Infrastructure;

namespace Transactions.Application.Transactions
{
    public class TransactionsSearchQuery : IRequest<IEnumerable<TransactionResponse>>
    {
        public DateTime? From { get; set; }
        public DateTime? To { get; set; }
        public int? SourceAccountId { get; set; }

        public ClaimsPrincipal ClaimsUser { get; set; }

        public TransactionsSearchQuery(DateTime? from, DateTime? to, int? srcAccId, ClaimsPrincipal claimsUser)
        {
            From = from;
            To = to;
            SourceAccountId = srcAccId;
            ClaimsUser = claimsUser;
        }
    }

    public class TransactionsSearchQueryHandler : IRequestHandler<TransactionsSearchQuery, IEnumerable<TransactionResponse>>
    {
        private readonly ITransactionsRepository _transactionsRepository;

        public TransactionsSearchQueryHandler(ITransactionsRepository transactionsRepository) => _transactionsRepository = transactionsRepository;

        public async Task<IEnumerable<TransactionResponse>> Handle(TransactionsSearchQuery request, CancellationToken cancellationToken)
        {
            // Obtenemos el id del usuario logueado a partir de su token
            var userId = request.ClaimsUser.FindFirst(ClaimTypes.NameIdentifier)!.Value;

            return await _transactionsRepository.SearchTransactionsAsync(int.Parse(userId), request.From, request.To, request.SourceAccountId);
        }
    }
}
