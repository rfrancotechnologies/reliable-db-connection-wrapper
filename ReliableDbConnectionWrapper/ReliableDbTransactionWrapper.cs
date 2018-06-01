using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Polly;

namespace ReliableDbWrapper
{
    public class ReliableDbTransactionWrapper : DbTransaction
    {
        private ReliableDbConnectionWrapper _underlyingConnection;
        private DbTransaction _underlyingTransaction;
        private readonly Policy _retryPolicy;

        public ReliableDbTransactionWrapper(DbTransaction transaction, ReliableDbConnectionWrapper connection,
                Policy retryPolicy)
        {
            _underlyingTransaction = transaction;
            _underlyingConnection = connection;
            _retryPolicy = retryPolicy;
            TransactionId = Guid.NewGuid();
        }

        public Guid TransactionId { get; set; }

        public override IsolationLevel IsolationLevel
        {
            get { return _underlyingTransaction.IsolationLevel; }
        }

        protected override DbConnection DbConnection
        {
            get { return _underlyingConnection; }
        }

        public override void Commit()
        {
            _retryPolicy.Execute(() => _underlyingTransaction.Commit());
        }

        public override void Rollback()
        {
            _retryPolicy.Execute(() => _underlyingTransaction.Rollback());
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _underlyingTransaction.Dispose();
            }

            base.Dispose(disposing);
        }
    }
}
