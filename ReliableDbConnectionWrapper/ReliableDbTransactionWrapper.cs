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
        public ReliableDbConnectionWrapper InnerConnection { get; set; }
        public DbTransaction InnerTransaction { get; set; }
        private readonly Policy _retryPolicy;

        public ReliableDbTransactionWrapper(DbTransaction transaction, ReliableDbConnectionWrapper connection,
                Policy retryPolicy)
        {
            InnerTransaction = transaction;
            InnerConnection = connection;
            _retryPolicy = retryPolicy;
            TransactionId = Guid.NewGuid();
        }

        public Guid TransactionId { get; set; }

        public override IsolationLevel IsolationLevel
        {
            get { return InnerTransaction.IsolationLevel; }
        }

        protected override DbConnection DbConnection
        {
            get { return InnerConnection; }
        }

        public override void Commit()
        {
            _retryPolicy.Execute(() => InnerTransaction.Commit());
        }

        public override void Rollback()
        {
            _retryPolicy.Execute(() => InnerTransaction.Rollback());
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                InnerTransaction.Dispose();
            }

            base.Dispose(disposing);
        }
    }
}
