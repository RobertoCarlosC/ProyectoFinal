using Microsoft.Data.SqlClient;

namespace EnerGym
{
    
    
    public class Database
    {
        private readonly string _connectionString;

        public Database(IConfiguration config)
        {
            _connectionString = config.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("Connection string 'DefaultConnection' no encontrada en appsettings.json");
        }

        
        public SqlConnection GetConnection()
        {
            return new SqlConnection(_connectionString);
        }
    }
}
