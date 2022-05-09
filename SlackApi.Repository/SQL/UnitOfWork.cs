using SlackApi.Core.Enums;
using SlackApi.Core.Extensions;
using StackExchange.Profiling;
using System.Data;
using System.Data.SqlClient;

namespace SlackApi.Repository.SQL
{
    public class UnitOfWork : IDisposable
    {
        private IDbConnection? _dbConnection;
        public IDbConnection Connection;
        public IDbTransaction? Transaction { get; set; }
        private bool _disposed;

        public UnitOfWork(string connectionString)
        {
            if (ConfigExtensions.GetCurrentEnvironment() == EnvironmentType.Development)
            {
                _dbConnection = new StackExchange.Profiling.Data.ProfiledDbConnection(
                    new SqlConnection(connectionString), MiniProfiler.Current);
            }
            else
            {
                _dbConnection = new SqlConnection(connectionString);
            }

            _dbConnection.Open();
            Transaction = _dbConnection.BeginTransaction();

            if (Transaction?.Connection == null)
                throw new NullReferenceException("Unable to setup db connection!");

            Connection = Transaction.Connection;
        }

        public void Dispose()
        {
            Cleanup(true);
            GC.SuppressFinalize(this);
        }

        public void Commit()
        {
            if (Transaction == null || _dbConnection == null)
                throw new NullReferenceException("Unable to setup db connection!");

            try
            {
                Transaction.Commit();
            }
            catch
            {
                Transaction.Rollback();
                throw;
            }
            finally
            {
                Transaction.Dispose();
                Transaction = _dbConnection.BeginTransaction();
            }
        }

        private void Cleanup(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    if (Transaction != null)
                    {
                        Transaction.Dispose();
                        Transaction = null;
                    }
                    if (_dbConnection != null)
                    {
                        _dbConnection.Dispose();
                        _dbConnection = null;
                    }
                }
                _disposed = true;
            }
        }
    }
}