using System.Data;
using Microsoft.Data.SqlClient;

namespace EnerGym.Infrastructure
{
    public static class SqlParameterHelper
    {
        public static SqlParameter Create(string name, SqlDbType type, object? value)
        {
            return new SqlParameter(name, type) { Value = value ?? DBNull.Value };
        }

        public static SqlParameter Create(string name, SqlDbType type, int size, object? value)
        {
            return new SqlParameter(name, type, size) { Value = value ?? DBNull.Value };
        }

        public static SqlParameter String(string name, int size, string? value)
        {
            return Create(name, SqlDbType.NVarChar, size, string.IsNullOrWhiteSpace(value) ? (object?)DBNull.Value : value.Trim());
        }

        public static SqlParameter Int(string name, int? value)
        {
            return Create(name, SqlDbType.Int, value ?? (object?)DBNull.Value);
        }

        public static SqlParameter Decimal(string name, decimal? value)
        {
            return Create(name, SqlDbType.Decimal, value ?? (object?)DBNull.Value);
        }

        public static SqlParameter DateTime(string name, DateTime? value)
        {
            return Create(name, SqlDbType.DateTime, value ?? (object?)DBNull.Value);
        }

        public static SqlParameter Bool(string name, bool? value)
        {
            return Create(name, SqlDbType.Bit, value ?? (object?)DBNull.Value);
        }
    }
}
