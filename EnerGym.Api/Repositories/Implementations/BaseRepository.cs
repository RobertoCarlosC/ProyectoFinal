using Microsoft.Data.SqlClient;

namespace EnerGym.Repositories.Implementations
{
    public abstract class BaseRepository
    {
        private readonly Database _db;

        protected BaseRepository(Database db)
        {
            _db = db;
        }

        protected SqlConnection GetConnection()
        {
            return _db.GetConnection();
        }

        protected async Task<T> WithConnectionAsync<T>(Func<SqlConnection, Task<T>> action)
        {
            using var conn = GetConnection();
            await conn.OpenAsync();
            return await action(conn);
        }

        protected async Task WithConnectionAsync(Func<SqlConnection, Task> action)
        {
            using var conn = GetConnection();
            await conn.OpenAsync();
            await action(conn);
        }
    }
}
