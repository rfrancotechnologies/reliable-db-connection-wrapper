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
    public class ReliableDbConnectionWrapperTests
    {
        [Fact]
        public void ShouldReadConnectionStringFromInnerConnection() 
        {
            string testConnectionString = "TestConnectionString";
            Mock<DbConnection> dbConnectionMock = new Mock<DbConnection>();
            dbConnectionMock.Setup(x => x.ConnectionString).Returns(testConnectionString);
            var connectionWrapper = new ReliableDbConnectionWrapper(dbConnectionMock.Object, 
                Policy.Handle<Exception>().WaitAndRetry(3, (retry) => TimeSpan.FromSeconds(1)));
            Assert.Equal(testConnectionString, connectionWrapper.ConnectionString);
        }

        [Fact]
        public void ShouldSetConnectionStringToInnerConnection() 
        {
            string testConnectionString = "TestConnectionString";
            Mock<DbConnection> dbConnectionMock = new Mock<DbConnection>();
            var connectionWrapper = new ReliableDbConnectionWrapper(dbConnectionMock.Object, 
                Policy.Handle<Exception>().WaitAndRetry(3, (retry) => TimeSpan.FromSeconds(1)));

            connectionWrapper.ConnectionString = testConnectionString;
            dbConnectionMock.VerifySet(x => x.ConnectionString = testConnectionString);
        }

        [Fact]
        public void ShouldReadDatabaseNameFromInnerConnection() 
        {
            string testDatabaseName = "TestDatabaseName";
            Mock<DbConnection> dbConnectionMock = new Mock<DbConnection>();
            dbConnectionMock.Setup(x => x.Database).Returns(testDatabaseName);
            var connectionWrapper = new ReliableDbConnectionWrapper(dbConnectionMock.Object, 
                Policy.Handle<Exception>().WaitAndRetry(3, (retry) => TimeSpan.FromSeconds(1)));
            Assert.Equal(testDatabaseName, connectionWrapper.Database);
        }

        [Fact]
        public void ShouldReadDataSourceFromInnerConnection() 
        {
            string testDataSource = "TestDataSource";
            Mock<DbConnection> dbConnectionMock = new Mock<DbConnection>();
            dbConnectionMock.Setup(x => x.DataSource).Returns(testDataSource);
            var connectionWrapper = new ReliableDbConnectionWrapper(dbConnectionMock.Object, 
                Policy.Handle<Exception>().WaitAndRetry(3, (retry) => TimeSpan.FromSeconds(1)));
            Assert.Equal(testDataSource, connectionWrapper.DataSource);
        }

        [Fact]
        public void ShouldReadServerVersionFromInnerConnection() 
        {
            string testServerVersion = "10.00.15";
            Mock<DbConnection> dbConnectionMock = new Mock<DbConnection>();
            dbConnectionMock.Setup(x => x.ServerVersion).Returns(testServerVersion);
            var connectionWrapper = new ReliableDbConnectionWrapper(dbConnectionMock.Object, 
                Policy.Handle<Exception>().WaitAndRetry(3, (retry) => TimeSpan.FromSeconds(1)));
            Assert.Equal(testServerVersion, connectionWrapper.ServerVersion);
        }

        [Fact]
        public void ShouldReadConnectionStateFromInnerConnection() 
        {
            ConnectionState testConnectionState = ConnectionState.Open;
            Mock<DbConnection> dbConnectionMock = new Mock<DbConnection>();
            dbConnectionMock.Setup(x => x.State).Returns(testConnectionState);
            var connectionWrapper = new ReliableDbConnectionWrapper(dbConnectionMock.Object, 
                Policy.Handle<Exception>().WaitAndRetry(3, (retry) => TimeSpan.FromSeconds(1)));
            Assert.Equal(testConnectionState, connectionWrapper.State);
        }
 
        [Fact]
        public void ShouldSetNewDatabaseToInnerConnection() 
        {
            string testDatabasename = "TestDatabaseName";
            Mock<DbConnection> dbConnectionMock = new Mock<DbConnection>();
            var connectionWrapper = new ReliableDbConnectionWrapper(dbConnectionMock.Object, 
                Policy.Handle<Exception>().WaitAndRetry(3, (retry) => TimeSpan.FromSeconds(1)));

            connectionWrapper.ChangeDatabase(testDatabasename);
            dbConnectionMock.Verify(x => x.ChangeDatabase(testDatabasename));
        }

        [Fact]
        public void ShouldCloseInnerConnection() 
        {
            Mock<DbConnection> dbConnectionMock = new Mock<DbConnection>();
            var connectionWrapper = new ReliableDbConnectionWrapper(dbConnectionMock.Object, 
                Policy.Handle<Exception>().WaitAndRetry(3, (retry) => TimeSpan.FromSeconds(1)));

            connectionWrapper.Close();
            dbConnectionMock.Verify(x => x.Close());
        }

        [Fact]
        public void ShouldOpenInnerConnectionWhenInStatusOtherThanOpen() 
        {
            Mock<DbConnection> dbConnectionMock = new Mock<DbConnection>();
            var connectionWrapper = new ReliableDbConnectionWrapper(dbConnectionMock.Object, 
                Policy.Handle<Exception>().WaitAndRetry(3, (retry) => TimeSpan.FromSeconds(1)));
            dbConnectionMock.Setup(x => x.State).Returns(ConnectionState.Closed);

            connectionWrapper.Open();
            dbConnectionMock.Verify(x => x.Open());
        }

        [Fact]
        public void ShouldNotOpenInnerConnectionWhenAlreadyOpen() 
        {
            Mock<DbConnection> dbConnectionMock = new Mock<DbConnection>();
            var connectionWrapper = new ReliableDbConnectionWrapper(dbConnectionMock.Object, 
                Policy.Handle<Exception>().WaitAndRetry(3, (retry) => TimeSpan.FromSeconds(1)));
            dbConnectionMock.Setup(x => x.State).Returns(ConnectionState.Open);

            connectionWrapper.Open();
            dbConnectionMock.Verify(x => x.Open(), Times.Never);
        }

        [Fact]
        public void ShouldBeginTransactionInInnerConnectionAnReturnTransactionWrapper() 
        {
            var testIsolationLevel = IsolationLevel.ReadCommitted;
            var dbConnectionMock = new MockDbConnection();
            var connectionWrapper = new ReliableDbConnectionWrapper(dbConnectionMock, 
                Policy.Handle<Exception>().WaitAndRetry(3, (retry) => TimeSpan.FromSeconds(1)));

            var obtainedTransaction = connectionWrapper.BeginTransaction(testIsolationLevel);
            Assert.True(((MockDbConnection) dbConnectionMock).TransactionBegun);
            Assert.Equal(testIsolationLevel, ((MockDbConnection) dbConnectionMock).UsedTransactionIsolationLevel);
        }
    }

    public class MockDbConnection : DbConnection
    {
        public  bool TransactionBegun { get; set; }
        public IsolationLevel UsedTransactionIsolationLevel { get; set; }

        public override string ConnectionString { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public override string Database => throw new NotImplementedException();

        public override string DataSource => throw new NotImplementedException();

        public override string ServerVersion => throw new NotImplementedException();

        public override ConnectionState State => throw new NotImplementedException();

        public override void ChangeDatabase(string databaseName)
        {
            throw new NotImplementedException();
        }

        public override void Close()
        {
            throw new NotImplementedException();
        }

        public override void Open()
        {
            throw new NotImplementedException();
        }

        protected override DbTransaction BeginDbTransaction(IsolationLevel isolationLevel)
        {
            TransactionBegun = true;
            UsedTransactionIsolationLevel = isolationLevel;
            return null;
        }

        protected override DbCommand CreateDbCommand()
        {
            throw new NotImplementedException();
        }
    }
}