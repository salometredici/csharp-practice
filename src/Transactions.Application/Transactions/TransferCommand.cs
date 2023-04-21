using MediatR;
using System.Security.Claims;
using Transactions.Domain;
using Transactions.Domain.Transfers;
using Transactions.Infrastructure;

namespace Transactions.Application.Transactions
{
    public class TransferCommand : IRequest<TransferResponse>
    {
        public TransferRequest Request { get; set; }

        public ClaimsPrincipal ClaimsUser { get; set; }

        public TransferCommand(TransferRequest request, ClaimsPrincipal claimsUser)
        {
            Request = request;
            ClaimsUser = claimsUser;
        }
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

            var originAcc = await _transactionsRepository.GetAccountByIdAsync(request.AccountFrom);
            var destAcc = await _transactionsRepository.GetAccountByIdAsync(request.AccountTo);

            VerifyUserIdentity(command.ClaimsUser, originAcc.UserId);

            var amountToAddOnDestAcc = RequiresConversion(originAcc.CurrencyId, destAcc.CurrencyId) ?
                await GetConvertedAmount(request.Amount, originAcc.CurrencyCode, destAcc.CurrencyCode) :
                request.Amount;

            var commissionAmount = IsThirdPartyTransfer(originAcc.UserId, destAcc.UserId) ?
                await GetCommissionAmount(request.Amount) : 0;

            var insertionRequest = new TransferInsertionRequest(request)
            {
                OriginCurrencyCode = originAcc.CurrencyCode,
                DestCurrencyCode = destAcc.CurrencyCode,
                AmountToAddOnDestAcc = amountToAddOnDestAcc,
                CommissionAmount = commissionAmount
            };
            var result = await _transactionsRepository.TransferAmountAsync(insertionRequest);

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
            var response = await _currenciesService.ConvertAmountAsync(amount, originCurrencyCode, destCurrencyCode);
            return response.Result;
        }

        /// <summary>
        /// Checks whether the user that is requesting the transfer is the same as the one logged in
        /// </summary>
        private void VerifyUserIdentity(ClaimsPrincipal claimsUser, int userId)
        {
            var loggedUserId = claimsUser.FindFirst(ClaimTypes.NameIdentifier)!.Value;

            if (int.Parse(loggedUserId) !=  userId)
            {
                throw new HttpException("Logged user differs from the user making the transfer", 401);
            }
        }
    }
}
