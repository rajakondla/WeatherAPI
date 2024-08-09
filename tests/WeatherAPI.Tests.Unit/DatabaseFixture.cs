using System;
using System.Data;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;
using Collette.Data;
using Collette.Neo.Database.Utility;
using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.SqlServer.Dac;
using Xunit;

namespace WeatherAPI.Tests.Unit
{
    public class DatabaseFixture : IAsyncLifetime
    {
        private Lazy<IDbConnection>? _dbConnection;
        private static string sqlServerName => "localhost, 1433";
        private string _testDatabaseName;

        private string _testDbConnectionString;
        public string _masterConnectionString;

        private string _dacPacPath = @"D:\code\WeatherAPI\WeatherAPI\WeatherAPI.Database\bin\Release\WeatherAPI.Database.dacpac";

        private static SemaphoreSlim _semaphoreSlim = new SemaphoreSlim(1, 1);

        public IConfiguration Configuration { get; private set; }

        public DatabaseFixture()
        {
            var configurationBuilder = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", false);

            configurationBuilder.AddEnvironmentVariables();

            Configuration = configurationBuilder.Build();

            var dateTimeStamp = DateTime.Now.ToString("ddmmyyhhffff");
            _testDatabaseName = Configuration.GetSection("TestDbName").Value + "_" + dateTimeStamp;
            var connStringTemplate = Configuration.GetSection("ConnectionStrings:db").Value;

            var currentDirectory = Environment.CurrentDirectory;
            currentDirectory = currentDirectory.Substring(0, currentDirectory.IndexOf(@"\tests\"));

            _dacPacPath = Path.Combine(currentDirectory, _dacPacPath);

            if (string.Equals(connStringTemplate, "@replaceDb@", StringComparison.InvariantCultureIgnoreCase))
            {
                connStringTemplate = "Server={0};Database={1};user id=sa;password=jKRropm5dwp4S9yuSzk3;TrustServerCertificate=true";

                _dacPacPath = _dacPacPath.Replace("\\Release\\", "\\Debug\\");

                _testDbConnectionString = string.Format(connStringTemplate, sqlServerName, _testDatabaseName);
                _masterConnectionString = string.Format(connStringTemplate, sqlServerName, "master");
            }
            else
            {
                _testDbConnectionString = string.Format(connStringTemplate, _testDatabaseName);
                _masterConnectionString = string.Format(connStringTemplate, "master");
            }

            Configuration["ConnectionStrings:db"] = _testDbConnectionString;
            Configuration["ConnectionStrings:master"] = _masterConnectionString;
        }

        public string TestNeoDbConnectionString => _testDbConnectionString;

        public IDbConnectionWrapper DbConnection => new InternalDbConnectionWrapper(this);

        public async Task InitializeAsync()
        {
            var dbDeployOptions = new DacDeployOptions();
            dbDeployOptions.SqlCommandVariableValues.Add("SQLDatabaseName", _testDatabaseName);

            await _semaphoreSlim.WaitAsync();

            try
            {
                DacPacUtility.DeployDacPac(_testDbConnectionString,
                    FilePathHelper.NormalizePath(_dacPacPath),
                    _testDatabaseName, dbDeployOptions);
            }
            catch
            {
                await DropTestDatabase(_testDatabaseName);
                throw;
            }
            finally
            {
                _semaphoreSlim.Release();
            }

            _dbConnection = new Lazy<IDbConnection>(() =>
                new SqlConnection(_testDbConnectionString)
            );
        }

        public static X509Certificate2? FindClientCertificate(string subjectName)
        {
            return
                FindCertificate(StoreLocation.LocalMachine);

            X509Certificate2? FindCertificate(StoreLocation location)
            {
                using (var store = new X509Store(location))
                {
                    store.Open(OpenFlags.OpenExistingOnly);
                    var certs = store.Certificates.Find(X509FindType.FindBySubjectName, subjectName, true);
                    return certs.OfType<X509Certificate2>().FirstOrDefault();
                }
            };
        }

        public async Task DisposeAsync()
        {
            await DropTestDatabase(_testDatabaseName);

            if (_dbConnection != null &&
                _dbConnection.IsValueCreated)
            {
                var connection = _dbConnection.Value;

                if (connection.State != ConnectionState.Closed)
                    connection.Close();

                connection.Dispose();
            }
        }

        public IDbConnection GetConnection()
        {
            if (_dbConnection == null)
            {
                throw new Exception("Initialize neo connection first");
            }

            IDbConnection? connectionValue = _dbConnection.Value;

            if (connectionValue!.State == ConnectionState.Closed)
            {
                connectionValue.Open();
            }

            return connectionValue;
        }

        private async Task DropTestDatabase(string dbName)
        {
            using (var con = new SqlConnection(_masterConnectionString))
            {
                await con.OpenAsync();
                await con.ExecuteAsync(@"
                    declare @Command nvarchar(max)
                    IF (DB_ID(@databaseName) IS NOT NULL)
                    BEGIN
                        select @Command = concat('
                        ALTER DATABASE ',@databaseName,' SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
                        DROP DATABASE ',@databaseName)
                        exec sys.sp_executesql @Command
                    END", param: new { databaseName = dbName });
            }
        }
    }

    internal class InternalDbConnectionWrapper : IDbConnectionWrapper
    {
        private readonly DatabaseFixture? _databaseFixture;

        public InternalDbConnectionWrapper(DatabaseFixture? databaseFixture = null)
        {
            _databaseFixture = databaseFixture;
        }

        public void Dispose()
        {
            if (_databaseFixture != null)
            {
                GetConnection().Dispose();
            }
        }

        public IDbConnection CreateConnection(bool autoOpen = false)
        {
            return GetConnection();
        }

        public IDbConnection GetConnection(IDbTransaction? dbTransaction = null, IDbConnection? connection = null)
        {
            return _databaseFixture != null ? _databaseFixture.GetConnection() : new FakeDbConnection();
        }
    }

    public static class FilePathHelper
    {
        public static string NormalizePath(string path) => Path.DirectorySeparatorChar == '\\' ?
           path.Replace("/", "\\") : path.Replace("\\", "/");
    }

    public class FakeDbConnection : IDbConnection
    {
        public string ConnectionString { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public int ConnectionTimeout => throw new NotImplementedException();

        public string Database => throw new NotImplementedException();

        public ConnectionState State => throw new NotImplementedException();

        public IDbTransaction BeginTransaction()
        {
            return new FakeDbTransaction();
        }

        public IDbTransaction BeginTransaction(IsolationLevel il)
        {
            return new FakeDbTransaction();
        }

        public void ChangeDatabase(string databaseName)
        {
        }

        public void Close()
        {
        }

        public IDbCommand CreateCommand()
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
        }

        public void Open()
        {
        }
    }

    public class FakeDbTransaction : IDbTransaction
    {
        public IDbConnection Connection => throw new NotImplementedException();

        public IsolationLevel IsolationLevel => throw new NotImplementedException();

        public void Commit()
        {
        }

        public void Dispose()
        {
        }

        public void Rollback()
        {
        }
    }
}
