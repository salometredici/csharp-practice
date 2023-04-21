using MediatR;
using Transactions.Domain.Transfers;
using Transactions.Infrastructure;

namespace Transactions.Application.Transactions
{
    public class TransferCommand : IRequest<TransferResponse>
    {
        public TransferRequest Request { get; set; }

        public TransferCommand(TransferRequest request) => Request = request;
    }

    public class TransferCommandHandler : IRequestHandler<TransferCommand, TransferResponse>
    {
        private readonly ICurrenciesService _currenciesService;
        private readonly ITransactionsRepository _transactionsRepository;

        public TransferCommandHandler(ICurrenciesService currenciesService, ITransactionsRepository transactionsRepository)
        {
            _currenciesService = currenciesService;
            _transactionsRepository = transactionsRepository;
        }

        public async Task<TransferResponse> Handle(TransferCommand command, CancellationToken cancellationToken)
        {
            var request = command.Request;

            var originAcc = await _transactionsRepository.GetAccountAsync(request.AccountFrom);
            var destAcc = await _transactionsRepository.GetAccountAsync(request.AccountTo);

            var amountToAddOnDestAcc = RequiresConversion(originAcc.CurrencyId, destAcc.CurrencyId) ?
                await GetConvertedAmount(request.Amount, originAcc.CurrencyCode, destAcc.CurrencyCode) :
                request.Amount;

            var commissionAmount = IsThirdPartyTransfer(originAcc.UserId, destAcc.UserId) ?
                await GetCommissionAmount(request.Amount) : 0;

            var result = await _transactionsRepository.TransferAmountAsync(request, amountToAddOnDestAcc, commissionAmount);

            return result.ToResponse(originAcc.CurrencyCode, destAcc.CurrencyCode);
        }

        private bool IsThirdPartyTransfer(int originUserId, int destUserId) => originUserId != destUserId;
        private bool RequiresConversion(int originCurrId, int destCurrId) => originCurrId != destCurrId;


        /// <summary>
        /// Returns the value of the commision to be applied on the account of origin
        /// </summary>
        /// <param name="transactionAmount">Initial transfer amount in the currency of the account of origin</param>
        private async Task<float> GetCommissionAmount(float transactionAmount)
        {
            var commissionRate = await _transactionsRepository.GetCommissionRate();
            return transactionAmount * commissionRate;
        }

        /// <summary>
        /// Returns the converted value of the transfer amount to be added to the destination account
        /// </summary>
        private async Task<float> GetConvertedAmount(float amount, string originCurrencyCode, string destCurrencyCode)
        {
            //return 0.02572f;
            // chequear lo del coste por llamado y volumen de transacciones por orden del millon
            var response = await _currenciesService.ConvertAmountAsync(amount, originCurrencyCode, destCurrencyCode);
            return response.Result;
        }
    }
}
