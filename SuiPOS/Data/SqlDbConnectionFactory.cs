using Microsoft.Data.SqlClient;

namespace SuiPOS.Data
{
    public class SqlDbConnectionFactory : IDbConnectionFactory
    {
        private readonly string _connectionString;
        public SqlDbConnectionFactory(IConfiguration config)
            => _connectionString = config.GetConnectionString("DefaultConnection")!;

        public async Task<SqlConnection> CreateConnectionAsync()
        {
            var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();
            return conn;
        }
    }
}
