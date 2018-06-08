using System;
using System.Data;
using System.Data.Common;
using Polly;

namespace ReliableDbWrapper
{
    public class ReliableDbCommandWrapper : DbCommand
    {
        private DbCommand _underlyingDbCommand;
        private readonly ISyncPolicy _retryPolicy;

        public ReliableDbConnectionWrapper InnerConnection { get; set; } 

        public ReliableDbCommandWrapper(DbCommand underlyingCommand, ISyncPolicy retryPolicy)
        {
            _underlyingDbCommand = underlyingCommand;
            _retryPolicy = retryPolicy;
        }

        public override string CommandText
        {
            get => _underlyingDbCommand.CommandText;
            set => _underlyingDbCommand.CommandText = value;
        }

        public override int CommandTimeout
        {
            get => _underlyingDbCommand.CommandTimeout;
            set => _underlyingDbCommand.CommandTimeout = value;
        }

        public override CommandType CommandType
        {
            get => _underlyingDbCommand.CommandType;
            set => _underlyingDbCommand.CommandType = value;
        }

        public override bool DesignTimeVisible
        {
            get => _underlyingDbCommand.DesignTimeVisible;
            set => _underlyingDbCommand.DesignTimeVisible = value;
        }

        public override UpdateRowSource UpdatedRowSource
        {
            get => _underlyingDbCommand.UpdatedRowSource;
            set => _underlyingDbCommand.UpdatedRowSource = value;
        }

        protected override DbConnection DbConnection
        {
            get
            {
                return InnerConnection;
            }

            set
            {
                InnerConnection = value as ReliableDbConnectionWrapper;
                if (InnerConnection != null)
                {
                    _underlyingDbCommand.Connection = InnerConnection.InnerConnection;
                }
                else
                { 
                    InnerConnection = new ReliableDbConnectionWrapper(value, _retryPolicy);
                    _underlyingDbCommand.Connection = InnerConnection.InnerConnection; 
                }
            }
        }

        protected override DbParameterCollection DbParameterCollection => _underlyingDbCommand.Parameters;

        protected override DbTransaction DbTransaction
        {
            get
            {
                return _underlyingDbCommand.Transaction == null ? null 
                    : new ReliableDbTransactionWrapper(_underlyingDbCommand.Transaction, InnerConnection, _retryPolicy);
            }

            set
            {
                var transaction = value as ReliableDbTransactionWrapper;
                _underlyingDbCommand.Transaction = (transaction != null) ? transaction.InnerTransaction : value;
            }
        }

        public override void Cancel()
        {
            _underlyingDbCommand.Cancel();
        }

        public override int ExecuteNonQuery()
        {
            return _retryPolicy.Execute(() => _underlyingDbCommand.ExecuteNonQuery());
        }

        public override object ExecuteScalar()
        {
            return _retryPolicy.Execute(() => _underlyingDbCommand.ExecuteScalar());
        }

        public override void Prepare()
        {
            _retryPolicy.Execute(() => _underlyingDbCommand.Prepare());
        }

        protected override DbParameter CreateDbParameter()
        {
            return _underlyingDbCommand.CreateParameter();
        }

        protected override DbDataReader ExecuteDbDataReader(CommandBehavior behavior)
        {
            return _retryPolicy.Execute(() => _underlyingDbCommand.ExecuteReader(behavior));
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _underlyingDbCommand.Dispose();
            }

            GC.SuppressFinalize(this);
        }
    }
}