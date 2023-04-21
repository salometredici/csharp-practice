namespace Transactions.Domain
{
    public class CurrencyConversionResponse
    {
        public CurrencyQuery Query { get; set; }
        public CurrencyConversionInfo Info { get; set; }
        public DateTime Date { get; set; }
        public float Result { get; set; }
    }

    public class CurrencyQuery
    {
        public string From { get; set; }
        public string To { get; set; }
        public float Amount { get; set; }
    }

    public class CurrencyConversionInfo
    {
        public int Timestamp { get; set; }
        public float Rate { get; set; }
    }
}
