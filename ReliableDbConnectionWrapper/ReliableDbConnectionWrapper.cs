using System;
using System.Data;
using System.Data.Common;
using Polly;

namespace ReliableDbWrapper
{
    public class ReliableDbConnectionWrapper : DbConnection
    {
        private readonly ISyncPolicy _retryPolicy;
        public DbConnection InnerConnection { get; set; }

        public ReliableDbConnectionWrapper(DbConnection underlyingConnection, ISyncPolicy retryPolicy)
        {
            _retryPolicy = retryPolicy;
            InnerConnection = underlyingConnection;
        }

        public override string ConnectionString
        {
            get
            {
                return InnerConnection.ConnectionString;
            }

            set
            {
                InnerConnection.ConnectionString = value;
            }
        }

        public override string Database => InnerConnection.Database;

        public override string DataSource => InnerConnection.DataSource;

        public override string ServerVersion => InnerConnection.ServerVersion;

        public override ConnectionState State => InnerConnection.State;

        public override void ChangeDatabase(string databaseName)
        {
            InnerConnection.ChangeDatabase(databaseName);
        }

        public override void Close()
        {
            InnerConnection.Close();
        }

        public override void Open()
        {
            _retryPolicy.Execute(() =>
            {
                if (InnerConnection.State != ConnectionState.Open)
                {
                    InnerConnection.Open();
                }
            });
        }

        protected override DbTransaction BeginDbTransaction(IsolationLevel isolationLevel)
        {
            return new ReliableDbTransactionWrapper(InnerConnection.BeginTransaction(isolationLevel), this, _retryPolicy);
        }

        protected override DbCommand CreateDbCommand()
        {
            return new ReliableDbCommandWrapper(InnerConnection.CreateCommand(), _retryPolicy);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (InnerConnection.State == ConnectionState.Open)
                {
                    InnerConnection.Close();
                }

                InnerConnection.Dispose();
            }

            GC.SuppressFinalize(this);
        }
    }
}
