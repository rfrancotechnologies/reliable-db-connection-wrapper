using System;
using System.Data;
using System.Data.Common;
using Polly;

namespace ReliableDbWrapper
{
    public class ReliableDbConnectionWrapper : DbConnection
    {
        private readonly Policy _retryPolicy;
        private DbConnection _underlyingConnection;

        public ReliableDbConnectionWrapper(DbConnection underlyingConnection, Policy retryPolicy)
        {
            _retryPolicy = retryPolicy;
            _underlyingConnection = underlyingConnection;
        }

        public override string ConnectionString
        {
            get
            {
                return _underlyingConnection.ConnectionString;
            }

            set
            {
                _underlyingConnection.ConnectionString = value;
            }
        }

        public override string Database => _underlyingConnection.Database;

        public override string DataSource => _underlyingConnection.DataSource;

        public override string ServerVersion => _underlyingConnection.ServerVersion;

        public override ConnectionState State => _underlyingConnection.State;

        public override void ChangeDatabase(string databaseName)
        {
            _underlyingConnection.ChangeDatabase(databaseName);
        }

        public override void Close()
        {
            _underlyingConnection.Close();
        }

        public override void Open()
        {
            _retryPolicy.Execute(() =>
            {
                if (_underlyingConnection.State != ConnectionState.Open)
                {
                    _underlyingConnection.Open();
                }
            });
        }

        protected override DbTransaction BeginDbTransaction(IsolationLevel isolationLevel)
        {
            return _underlyingConnection.BeginTransaction(isolationLevel);
        }

        protected override DbCommand CreateDbCommand()
        {
            return new ReliableDbCommandWrapper(_underlyingConnection.CreateCommand(), _retryPolicy);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_underlyingConnection.State == ConnectionState.Open)
                {
                    _underlyingConnection.Close();
                }

                _underlyingConnection.Dispose();
            }

            GC.SuppressFinalize(this);
        }
    }
}
