using MediatR;
using Transactions.Domain.Transactions;
using Transactions.Infrastructure;

namespace Transactions.Application.Transactions
{
    public class TransactionsSearchQuery : IRequest<IEnumerable<TransactionResponse>>
    {
        public DateTime? From { get; set; }
        public DateTime? To { get; set; }
        public int? SourceAccountId { get; set; }

        public TransactionsSearchQuery(DateTime? from, DateTime? to, int? srcAccId)
        {
            From = from;
            To = to;
            SourceAccountId = srcAccId;
        }
    }

    public class TransactionsSearchQueryHandler : IRequestHandler<TransactionsSearchQuery, IEnumerable<TransactionResponse>>
    {
        private readonly ITransactionsRepository _transactionsRepository;

        public TransactionsSearchQueryHandler(ITransactionsRepository transactionsRepository) => _transactionsRepository = transactionsRepository;

        public async Task<IEnumerable<TransactionResponse>> Handle(TransactionsSearchQuery request, CancellationToken cancellationToken)
        {
            var userId = 1; // tngo q obtener a partir del mail este id <- del bearer
            return await _transactionsRepository.SearchTransactionsAsync(userId, request.From, request.To, request.SourceAccountId);
        }
    }
}
