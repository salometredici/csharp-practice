namespace Transactions.Domain.Transactions
{
    public class TransactionDto
    {
        public int TransactionId { get; set; }
        public int OriginAccId { get; set; }
        public string OriginCurrDescrip { get; set; }
        public int DestAccId { get; set; }
        public string DestCurrDescrip { get; set; }
        public double TransactionAmount { get; set; }
        public DateTime TransactionDate { get; set; }
        public string TransactionDescrip { get; set; }

        public TransactionResponse ToResponse()
        {
            return new TransactionResponse()
            {
                Id = TransactionId,
                AccountFrom = OriginAccId,
                OriginCurrency = OriginCurrDescrip,
                AccountTo = DestAccId,
                DestCurrency = DestCurrDescrip,
                Amount = TransactionAmount,
                Date = TransactionDate,
                Description = TransactionDescrip
            };
        }
    }
}
