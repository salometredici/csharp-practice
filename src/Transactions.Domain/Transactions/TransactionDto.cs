namespace Transactions.Domain.Transactions
{
    public class TransactionDto
    {
        public int TransactionId { get; set; }
        public int OriginAccId { get; set; }
        public string OriginCurrCode { get; set; }
        public int DestAccId { get; set; }
        public string DestCurrCode { get; set; }
        public double TransactionAmount { get; set; }
        public DateTime TransactionDate { get; set; }
        public string TransactionDescrip { get; set; }

        public TransactionResponse ToResponse()
        {
            return new TransactionResponse()
            {
                Id = TransactionId,
                AccountFrom = OriginAccId,
                OriginCurrency = OriginCurrCode,
                AccountTo = DestAccId,
                DestCurrency = DestCurrCode,
                Amount = TransactionAmount,
                Date = TransactionDate,
                Description = TransactionDescrip
            };
        }
    }
}
