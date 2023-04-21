namespace Transactions.Domain
{
    public class HttpException : Exception
    {
        public int StatusCode { get; set; }
        public HttpException(string msg, int statusCode) : base(msg) => StatusCode = statusCode;
    }
}
