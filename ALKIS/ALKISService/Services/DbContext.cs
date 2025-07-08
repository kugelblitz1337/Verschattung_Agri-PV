
using System;
using System.Data;
using System.Threading.Tasks;
using DuckDB.NET.Data;
using static DuckDB.NET.Native.NativeMethods;

namespace ALKISService.Services
{

    public interface IDbContext : IDisposable
    {
        DuckDBConnection Connection { get; }
        int Execute(string sql, object param = null);
        T ExecuteScalar<T>(string sql, object param = null);
        IEnumerable<T> Query<T>(string sql, object param = null) where T : new();
        T QuerySingleOrDefault<T>(string sql, object param = null);
        Task<int> ExecuteAsync(string sql, object param = null);
        Task<IEnumerable<T>> QueryAsync<T>(string sql, object param = null);
        Task<T> QuerySingleOrDefaultAsync<T>(string sql, object param = null);
        int Execute(string sql, List<DuckDBParameter> param = null);
    }
    public class DbContext : IDbContext
    {

        public DuckDBConnection Connection { get; }
        private bool _disposed;

        private const string ConnectionString = "DataSource=flurstuecke.db";
        public DbContext()
        {
            Connection = new DuckDBConnection(ConnectionString);
            Connection.Open();
        }
        public int Execute(string sql, object param = null)
        {
            try
            {
                using var command = Connection.CreateCommand();
                command.CommandText = sql;
                AddParameters(command, param);
                return command.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                return 0;
            }
        }

        public int Execute(string sql, List<DuckDBParameter> param)
        {
            try
            {
                using var command = Connection.CreateCommand();
                command.CommandText = sql;
                AddParameters(command, param);
                return command.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                return 0;
            }
        }

        public T ExecuteScalar<T>(string sql, object param = null)
        {
            using var command = Connection.CreateCommand();
            command.CommandText = sql;
            AddParameters(command, param);
            return (T)command.ExecuteScalar();
        }
        public IEnumerable<T> Query<T>(string sql, object param = null) where T : new()
        {
            using var command = Connection.CreateCommand();
            command.CommandText = sql;
            AddParameters(command, param);
            using var reader = command.ExecuteReader();
            var results = new List<T>();

            var props = typeof(T).GetProperties();

            while (reader.Read())
            {
                var entity = new T();
                foreach (var prop in props)
                {
                    // DuckDB/SQLite: Spaltennamen sind case-insensitive!
                    var colIndex = reader.GetOrdinal(prop.Name);
                    if (!reader.IsDBNull(colIndex))
                    {
                        var value = reader.GetValue(colIndex);
                        prop.SetValue(entity, value is DBNull ? null : value);
                    }
                }
                results.Add(entity);
            }
            return results;
        }

        public T QuerySingleOrDefault<T>(string sql, object param = null)
        {
            using var command = Connection.CreateCommand();
            command.CommandText = sql;
            AddParameters(command, param);
            using var reader = command.ExecuteReader();
            if (reader.Read())
            {
                return (T)reader.GetValue(0);
            }
            return default;
        }

        public async Task<int> ExecuteAsync(string sql, object param = null)
        {
            using var command = Connection.CreateCommand();
            command.CommandText = sql;
            AddParameters(command, param);
            return await Task.FromResult(command.ExecuteNonQuery());
        }

        public async Task<IEnumerable<T>> QueryAsync<T>(string sql, object param = null)
        {
            using var command = Connection.CreateCommand();
            command.CommandText = sql;
            AddParameters(command, param);
            using var reader = command.ExecuteReader();
            var results = new List<T>();
            while (await Task.FromResult(reader.Read()))
            {
                results.Add((T)reader.GetValue(0));
            }
            return results;
        }

        public async Task<T> QuerySingleOrDefaultAsync<T>(string sql, object param = null)
        {
            using var command = Connection.CreateCommand();
            command.CommandText = sql;
            AddParameters(command, param);
            using var reader = command.ExecuteReader();
            if (await Task.FromResult(reader.Read()))
            {
                return (T)reader.GetValue(0);
            }
            return default;
        }

        private void AddParameters(DuckDBCommand command, object param)
        {
            if (param == null) return;

            foreach (var property in param.GetType().GetProperties())
            {
                var parameter = command.CreateParameter();
                var value = property.GetValue(param);
                if (value is double || value is float)
                {
                    parameter.DbType = System.Data.DbType.Double;
                    value = (double)property.GetValue(param);
                }
                else if (value is int || value is long)
                    parameter.DbType = System.Data.DbType.Int64; // oder Int32 je nach Bedarf
                else if (value is DateTime)
                    parameter.DbType = System.Data.DbType.DateTime;
                else if (value is bool)
                    parameter.DbType = System.Data.DbType.Boolean;
                else
                    parameter.DbType = System.Data.DbType.String;
                parameter.ParameterName = null;
                parameter.Value = value ?? DBNull.Value; 
                command.Parameters.Add(parameter);
            }
        }

        private void AddParameters(DuckDBCommand command, List<DuckDBParameter> parameters)
        {
            if (parameters == null) return;

            foreach (var param in parameters)
            {
           
                command.Parameters.Add(param);
            }
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                Connection.Dispose();
                _disposed = true;
            }
        }
    }
}
