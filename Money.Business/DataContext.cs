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
                return conn.Query<Action>("SELECT * FROM public.\"Actions\"");
            }
        }

        public Action GetAction(int id)
        {
            using (var conn = new NpgsqlConnection(_connectionString))
            {
                conn.Open();
                return conn.QuerySingle<Action>($@"SELECT * FROM public.""Actions"" WHERE ""Id"" = {id}");
            }
        }

        public IEnumerable<CreditPayment> GetPayments()
        {
            using (var conn = new NpgsqlConnection(_connectionString))
            {
                conn.Open();
                return conn.Query<CreditPayment>($"SELECT * FROM public.\"Payments\"");
            }
        }

        public IEnumerable<Credit> GetCredits()
        {
            using (var conn = new NpgsqlConnection(_connectionString))
            {
                conn.Open();
                return conn.Query<Credit>($"SELECT * FROM public.\"Credits\"");
            }
        }
        


        public async Task<int> AddAction(Action action)
        {
            using (var conn = new NpgsqlConnection(_connectionString))
            {
                conn.Open();
                return await conn.ExecuteAsync($@"INSERT INTO ""Actions"" (""Type"",""Value"",""Description"",""Date"",""Category"",""Credit"") VALUES(@Type,@Value,@Description,@Date, @Category, @Credit)", action);
            }
        }

    }
}
