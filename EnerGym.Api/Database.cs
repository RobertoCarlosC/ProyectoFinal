using Microsoft.Data.SqlClient;

namespace EnerGym
{
    
    
    public class Database
    {
        private readonly string _connectionString;

        public Database(IConfiguration config)
        {
            var cs = config.GetConnectionString("DefaultConnection");
            if (string.IsNullOrWhiteSpace(cs))
                throw new InvalidOperationException("Connection string 'DefaultConnection' no encontrada. Configure appsettings.json o la variable de entorno DOTNET_ConnectionStrings__DefaultConnection.");
            _connectionString = cs;
        }

        
        public SqlConnection GetConnection()
        {
            return new SqlConnection(_connectionString);
        }
    }
}
