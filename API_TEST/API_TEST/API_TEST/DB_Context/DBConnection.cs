using Microsoft.Data.SqlClient;
using System.Data;

namespace API_TEST.DB_Context
{
    public class DB_Connection
    {
        private readonly string _connectionString;

        public DB_Connection(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DbConnection");
        }

    }
}
