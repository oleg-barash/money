using System.Collections.Generic;
using System.Threading.Tasks;
using Dapper;
using Money.Business.Models;
using Npgsql;

namespace Money.Business
{
    public class DataContext
    {
        private readonly string _connectionString;

        public DataContext(string connectionString)
        {
            _connectionString = connectionString;
        }
        public IEnumerable<Action> GetActions()
        {
            using (var conn = new NpgsqlConnection(_connectionString))
            {
                conn.Open();
                return conn.Query<Action>($"SELECT * FROM public.\"actions\"");
            }
        }

        public Action GetAction(int id)
        {
            using (var conn = new NpgsqlConnection(_connectionString))
            {
                conn.Open();
                return conn.QuerySingle<Action>($@"SELECT * FROM public.""actions"" WHERE ""id"" = {id}");
            }
        }

        public IEnumerable<CreditPayment> GetPayments()
        {
            using (var conn = new NpgsqlConnection(_connectionString))
            {
                conn.Open();
                return conn.Query<CreditPayment>($"SELECT * FROM public.\"payments\"");
            }
        }

        public IEnumerable<Credit> GetCredits()
        {
            using (var conn = new NpgsqlConnection(_connectionString))
            {
                conn.Open();
                return conn.Query<Credit>($"SELECT * FROM public.\"credits\"");
            }
        }
        
        public async Task<int> AddAction(Action action)
        {
            using (var conn = new NpgsqlConnection(_connectionString))
            {
                conn.Open();
                return await conn.ExecuteAsync($@"INSERT INTO ""actions"" (""type"",""value"",""description"",""date"",""category"",""credit"") VALUES(@Type,@Value,@Description,@Date, @Category, @Credit)", action);
            }
        }
        public async Task<int> AddCredit(Credit credit)
        {
            using (var conn = new NpgsqlConnection(_connectionString))
            {
                conn.Open();
                return await conn.ExecuteAsync($@"INSERT INTO ""credits"" (""sum"",""duration"",""description"",""date"",""interest"",""paymentvalue"") VALUES(@Sum,@Duration,@Description,@Date, @Interest, @Paymentvalue)", credit);
            }
        }
    }
}
