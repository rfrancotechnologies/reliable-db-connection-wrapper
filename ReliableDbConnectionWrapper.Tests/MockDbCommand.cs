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
using AutoFixture;

namespace ReliableDbWrapper.Tests
{
    /// <sumary>
    /// Test mock class for those DbCommand methods that are not virtual or protected and cannot
    /// be mocked from a standar Moq Mock<>.
    /// </summary>
    public class MockDbCommand : DbCommand
    {
        private Fixture fixture;
        public DbParameterCollection NextParameterCollectionToReturn { get; set; }

        public DbParameter NextParameterToReturn { get; set; }
        public int ExecuteDbDataReaderCount { get; set; }

        public DbDataReader NextDbDataReaderToReturn { get; set; }
        public int DisposeCount { get; set; }

        public MockDbCommand() 
        {
            fixture = new Fixture();
        }
        
        public override string CommandText { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public override int CommandTimeout { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public override CommandType CommandType { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public override bool DesignTimeVisible { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public override UpdateRowSource UpdatedRowSource { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        protected override DbConnection DbConnection { get; set; }

        protected override DbParameterCollection DbParameterCollection => NextParameterCollectionToReturn;

        protected override DbTransaction DbTransaction { get; set; }

        public override void Cancel()
        {
            throw new NotImplementedException();
        }

        public override int ExecuteNonQuery()
        {
            throw new NotImplementedException();
        }

        public override object ExecuteScalar()
        {
            throw new NotImplementedException();
        }

        public override void Prepare()
        {
            throw new NotImplementedException();
        }

        protected override DbParameter CreateDbParameter()
        {
            return NextParameterToReturn;
        }

        protected override DbDataReader ExecuteDbDataReader(CommandBehavior behavior)
        {
            lock(this)
            {
                ExecuteDbDataReaderCount++;
            }
            return NextDbDataReaderToReturn;
        }

        protected override void Dispose(bool disposing) {
            lock(this) {
                DisposeCount++;
            }
        }
    }
}