using System;
using System.Collections.Generic;
using System.Data.Common;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using Xunit;
using ReliableDbWrapper;
using Polly;
using System.Data;

namespace ReliableDbWrapper.Tests
{
    /// <sumary>
    /// Test mock class for those DbConnection methods that are not virtual or protected and cannot
    /// be mocked from a standar Moq Mock<>.
    /// </summary>

    public class MockDbConnection : DbConnection
    {
        public  int BeginDbTransactionCount { get; set; }
        public  int CreateCommandCount { get; set; }
        public  int DisposeCount { get; set; }
        public  int CloseCount { get; set; }
        public IsolationLevel LastUsedTransactionIsolationLevel { get; set; }

        public override string ConnectionString { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public override string Database => throw new NotImplementedException();

        public override string DataSource => throw new NotImplementedException();

        public override string ServerVersion => throw new NotImplementedException();

        public override ConnectionState State { get => ConnectionState.Open; }

        public override void ChangeDatabase(string databaseName)
        {
            throw new NotImplementedException();
        }

        public override void Close()
        {
            lock(this) 
            {
                CloseCount++;
            }
        }

        public override void Open()
        {
            throw new NotImplementedException();
        }

        protected override DbTransaction BeginDbTransaction(IsolationLevel isolationLevel)
        {
            lock(this) 
            {
                BeginDbTransactionCount++;
            }
            LastUsedTransactionIsolationLevel = isolationLevel;
            return new MockDbTransaction();
        }

        protected override DbCommand CreateDbCommand()
        {
            lock(this) 
            {
                CreateCommandCount++;
            }
            return new MockDbCommand();
        }

        protected override void Dispose(bool disposing) 
        {
            lock(this) 
            {
                DisposeCount++;
            }
        }
    }
}