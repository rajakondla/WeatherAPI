using System;
using System.Data;
using System.Data.Common;
using System.Diagnostics;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;

namespace Collette.Data
{
    public class DbConnectionWrapper : IDbConnectionWrapper
    {
        private bool _disposedValue;

        private readonly Lazy<IDbConnection> _connection;
        private readonly string _connectionString;

        public DbConnectionWrapper(IOptions<DbConnectionOption> dbOptionsAccessor)
        {
            _connectionString = dbOptionsAccessor.Value.ConnectionString;

            _connection = new Lazy<IDbConnection>(() => {
                var connection = new SqlConnection(_connectionString);

                return connection;
            });
        }

        public IDbConnection GetConnection(IDbTransaction? transaction = null,
            IDbConnection? connection = null)
        {
            if (transaction != null)
                return transaction.Connection;

            if (connection != null)
                return connection;

            var connectionValue = _connection.Value;
            if (connectionValue.State == ConnectionState.Closed)
            {
                Debug.WriteLine(connectionValue);

                connectionValue.Open();
            }
            return connectionValue;
        }

        public IDbConnection CreateConnection(bool autoOpen = false)
        {
            DbConnection connection = new SqlConnection(_connectionString);

            if (autoOpen)
                connection.Open();

            return connection;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing && (_connection.IsValueCreated))
                {
                    var connection = _connection.Value;
                    connection.Close();
                    connection.Dispose();
                }

                _disposedValue = true;
            }
        }
    }

    public class DbConnectionOption
    {
        public string ConnectionString { get; set; } = null!;
    }
}
