using Dapper;
using Microsoft.Extensions.Configuration;
using Npgsql;
using NpgsqlTypes;
using System.Data;
using Transactions.Domain.Accounts;
using Transactions.Domain.Transactions;
using Transactions.Domain.Transfers;
using Transactions.Domain.Users;

namespace Transactions.Infrastructure
{
    public interface ITransactionsRepository
    {
        // Users - Estos métodos podrían ir en su propio UsersRepository, pero para no seguir expandiendo el código/creando schemas, etc lo dejo acá esta vez
        Task RegisterAsync(RegisterRequest request, string encrPwd);
        Task<UserDto> GetUserByEmailAsync(string email);
        Task LoginAsync(string email, DateTime? loginDate);
        // Transactions
        Task<float> GetCommissionRate();
        Task<AccountResponse> GetAccountByIdAsync(int accountId);
        Task<TransferDto> TransferAmountAsync(TransferInsertionRequest request);
        Task<IEnumerable<TransactionResponse>> SearchTransactionsAsync(int userId, DateTime? from, DateTime? to, int? srcAccId);
    }

    public class TransactionsRepository : ITransactionsRepository
    {
        private readonly IConfiguration _configuration;
        private const string DbName = "TransactionsDb";

        public TransactionsRepository(IConfiguration configuration) => _configuration = configuration;

        #region [Users]

        public async Task RegisterAsync(RegisterRequest request, string encrPwd)
        {
            using (var conn = CreateConnection())
            {
                conn.Open();

                var sql = "CALL transactions.proc_register(:p_name,:p_surname,:p_email,:p_pwd_hash);";
                
                var command = new NpgsqlCommand(sql, conn);
                command.Parameters.AddWithValue(":p_name", NpgsqlDbType.Varchar, request.Name);
                command.Parameters.AddWithValue(":p_surname", NpgsqlDbType.Varchar, request.Surname);
                command.Parameters.AddWithValue(":p_email", NpgsqlDbType.Varchar, request.Email);
                command.Parameters.AddWithValue(":p_pwd_hash", NpgsqlDbType.Varchar, encrPwd);

                await command.ExecuteNonQueryAsync();
            }
        }

        public async Task<UserDto> GetUserByEmailAsync(string email)
        {
            using (var conn = CreateConnection())
            {
                conn.Open();

                var sql = "SELECT * FROM transactions.func_get_user_by_email(:p_email);";
                return await conn.QueryFirstOrDefaultAsync<UserDto>(sql, new { p_email = email });
            }
        }

        public async Task LoginAsync(string email, DateTime? loginDate)
        {
            using (var conn = CreateConnection())
            {
                conn.Open();

                var sql = "CALL transactions.proc_login(:p_email,:p_login_date);";

                var command = new NpgsqlCommand(sql, conn);
                command.Parameters.AddWithValue(":p_email", NpgsqlDbType.Varchar, email);
                command.Parameters.AddWithValue(":p_login_date", NpgsqlDbType.Timestamp, loginDate!);

                await command.ExecuteNonQueryAsync();
            }
        }

        #endregion [Users]

        #region [Transactions]

        public async Task<float> GetCommissionRate()
        {
            using (var conn = CreateConnection())
            {
                conn.Open();
                var sql = $"SELECT * FROM transactions.func_get_commission_rate();";
                return await conn.ExecuteScalarAsync<float>(sql);
            }
        }

        public async Task<AccountResponse> GetAccountByIdAsync(int accountId)
        {
            using (var conn = CreateConnection())
            {
                conn.Open();
                var sql = $"SELECT * FROM transactions.func_get_account_by_id({accountId}::integer);";
                var accountDto = await conn.QueryFirstAsync<AccountDto>(sql);
                return accountDto.ToResponse();
            }
        }

        public async Task<TransferDto> TransferAmountAsync(TransferInsertionRequest request)
        {
            using (var conn = CreateConnection())
            {
                conn.Open();
                var sql = $"SELECT * FROM transactions.func_transfer_amount(" +
                    $"@p_acc_from::integer,@p_origin_curr_code::varchar,@p_acc_to::integer,@p_dest_curr_code::varchar," +
                    $"@p_amount_to_debit_on_origin::real,@p_amount_to_add_on_dest::real,@p_date::timestamp,@p_descrip::varchar,@p_commission_amount::real);";

                return await conn.QueryFirstAsync<TransferDto>(sql, new
                {
                    p_acc_from = request.AccountFrom,
                    p_origin_curr_code = request.OriginCurrencyCode,
                    p_acc_to = request.AccountTo,
                    p_dest_curr_code = request.DestCurrencyCode,
                    p_amount_to_debit_on_origin = request.Amount,
                    p_amount_to_add_on_dest = request.AmountToAddOnDestAcc,
                    p_date = request.Date,
                    p_descrip = request.Description,
                    p_commission_amount = request.CommissionAmount
                });
            }
        }

        public async Task<IEnumerable<TransactionResponse>> SearchTransactionsAsync(int userId, DateTime? from, DateTime? to, int? srcAccId)
        {
            using (var conn = CreateConnection())
            {
                conn.Open();
                var sql = "SELECT * FROM transactions.func_search_transactions(:p_user_id,:p_from,:p_to,:p_srcAccId);";
                var transactionsDto = await conn.QueryAsync<TransactionDto>(sql, 
                    new
                    {
                        p_user_id = userId,
                        p_from = from,
                        p_to = to,
                        p_srcAccId = srcAccId
                    });
                return transactionsDto.Select(t => t.ToResponse());
            }
        }

        #endregion [Transactions]

        private NpgsqlConnection CreateConnection()
        {
            var connStr = _configuration.GetConnectionString(DbName);
            return new NpgsqlConnection(connStr);
        }
    }
}
