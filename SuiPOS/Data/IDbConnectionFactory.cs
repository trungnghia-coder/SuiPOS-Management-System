using Microsoft.Data.SqlClient;

namespace SuiPOS.Data
{
    public interface IDbConnectionFactory
    {
        Task<SqlConnection> CreateConnectionAsync();
    }
}
