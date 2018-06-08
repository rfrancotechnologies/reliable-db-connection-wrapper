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
    public class ReliableDbConnectionWrapperTests
    {
        private Fixture fixture;
        public ReliableDbConnectionWrapperTests() 
        {
            fixture = new Fixture();
        }

        [Fact]
        public void ShouldReadConnectionStringFromInnerConnection() 
        {
            string testConnectionString = fixture.Create<string>();
            Mock<DbConnection> dbConnectionMock = new Mock<DbConnection>();
            Mock<ISyncPolicy> retryPolicyMock = new Mock<ISyncPolicy>();
            dbConnectionMock.Setup(x => x.ConnectionString).Returns(testConnectionString);
            var connectionWrapper = new ReliableDbConnectionWrapper(dbConnectionMock.Object, retryPolicyMock.Object);
            Assert.Equal(testConnectionString, connectionWrapper.ConnectionString);
        }

        [Fact]
        public void ShouldSetConnectionStringToInnerConnection() 
        {
            string testConnectionString = fixture.Create<string>();
            Mock<DbConnection> dbConnectionMock = new Mock<DbConnection>();
            Mock<ISyncPolicy> retryPolicyMock = new Mock<ISyncPolicy>();
            var connectionWrapper = new ReliableDbConnectionWrapper(dbConnectionMock.Object, retryPolicyMock.Object);

            connectionWrapper.ConnectionString = testConnectionString;
            dbConnectionMock.VerifySet(x => x.ConnectionString = testConnectionString);
        }

        [Fact]
        public void ShouldReadDatabaseNameFromInnerConnection() 
        {
            string testDatabaseName = fixture.Create<string>();
            Mock<DbConnection> dbConnectionMock = new Mock<DbConnection>();
            Mock<ISyncPolicy> retryPolicyMock = new Mock<ISyncPolicy>();
            dbConnectionMock.Setup(x => x.Database).Returns(testDatabaseName);
            var connectionWrapper = new ReliableDbConnectionWrapper(dbConnectionMock.Object, retryPolicyMock.Object);
            Assert.Equal(testDatabaseName, connectionWrapper.Database);
        }

        [Fact]
        public void ShouldReadDataSourceFromInnerConnection() 
        {
            string testDataSource = fixture.Create<string>();
            Mock<DbConnection> dbConnectionMock = new Mock<DbConnection>();
            Mock<ISyncPolicy> retryPolicyMock = new Mock<ISyncPolicy>();
            dbConnectionMock.Setup(x => x.DataSource).Returns(testDataSource);
            var connectionWrapper = new ReliableDbConnectionWrapper(dbConnectionMock.Object, retryPolicyMock.Object);
            Assert.Equal(testDataSource, connectionWrapper.DataSource);
        }

        [Fact]
        public void ShouldReadServerVersionFromInnerConnection() 
        {
            string testServerVersion = fixture.Create<string>();
            Mock<DbConnection> dbConnectionMock = new Mock<DbConnection>();
            Mock<ISyncPolicy> retryPolicyMock = new Mock<ISyncPolicy>();
            dbConnectionMock.Setup(x => x.ServerVersion).Returns(testServerVersion);
            var connectionWrapper = new ReliableDbConnectionWrapper(dbConnectionMock.Object, retryPolicyMock.Object);
            Assert.Equal(testServerVersion, connectionWrapper.ServerVersion);
        }

        [Fact]
        public void ShouldReadConnectionStateFromInnerConnection() 
        {
            ConnectionState testConnectionState = fixture.Create<ConnectionState>();
            Mock<DbConnection> dbConnectionMock = new Mock<DbConnection>();
            Mock<ISyncPolicy> retryPolicyMock = new Mock<ISyncPolicy>();
            dbConnectionMock.Setup(x => x.State).Returns(testConnectionState);
            var connectionWrapper = new ReliableDbConnectionWrapper(dbConnectionMock.Object, retryPolicyMock.Object);
            Assert.Equal(testConnectionState, connectionWrapper.State);
        }
 
        [Fact]
        public void ShouldSetNewDatabaseToInnerConnection() 
        {
            string testDatabasename = fixture.Create<string>();
            Mock<DbConnection> dbConnectionMock = new Mock<DbConnection>();
            Mock<ISyncPolicy> retryPolicyMock = new Mock<ISyncPolicy>();
            var connectionWrapper = new ReliableDbConnectionWrapper(dbConnectionMock.Object, retryPolicyMock.Object);

            connectionWrapper.ChangeDatabase(testDatabasename);
            dbConnectionMock.Verify(x => x.ChangeDatabase(testDatabasename));
        }

        [Fact]
        public void ShouldCloseInnerConnection() 
        {
            Mock<DbConnection> dbConnectionMock = new Mock<DbConnection>();
            Mock<ISyncPolicy> retryPolicyMock = new Mock<ISyncPolicy>();
            var connectionWrapper = new ReliableDbConnectionWrapper(dbConnectionMock.Object, retryPolicyMock.Object);

            connectionWrapper.Close();
            dbConnectionMock.Verify(x => x.Close());
        }

        [Fact]
        public void ShouldOpenInnerConnectionWithRetriesWhenInStatusOtherThanOpen() 
        {
            Mock<DbConnection> dbConnectionMock = new Mock<DbConnection>();
            Mock<ISyncPolicy> retryPolicyMock = new Mock<ISyncPolicy>();
            var connectionWrapper = new ReliableDbConnectionWrapper(dbConnectionMock.Object, retryPolicyMock.Object);

            dbConnectionMock.Setup(x => x.State).Returns(ConnectionState.Closed);
            retryPolicyMock.Setup(x => x.Execute(It.IsAny<Action>())).Callback<Action>(a => a.Invoke());
            
            connectionWrapper.Open();
            retryPolicyMock.Verify(x => x.Execute(It.IsAny<Action>()));
            dbConnectionMock.Verify(x => x.Open());
        }

        [Fact]
        public void ShouldNotOpenInnerConnectionWhenAlreadyOpen() 
        {
            Mock<DbConnection> dbConnectionMock = new Mock<DbConnection>();
            Mock<ISyncPolicy> retryPolicyMock = new Mock<ISyncPolicy>();
            var connectionWrapper = new ReliableDbConnectionWrapper(dbConnectionMock.Object, retryPolicyMock.Object);
            dbConnectionMock.Setup(x => x.State).Returns(ConnectionState.Open);

            connectionWrapper.Open();
            dbConnectionMock.Verify(x => x.Open(), Times.Never);
        }

        [Fact]
        public void ShouldBeginTransactionInInnerConnectionAnReturnWrapper() 
        {
            var testIsolationLevel = IsolationLevel.ReadCommitted;
            var dbConnectionMock = new MockDbConnection();
            Mock<ISyncPolicy> retryPolicyMock = new Mock<ISyncPolicy>();
            var connectionWrapper = new ReliableDbConnectionWrapper(dbConnectionMock, retryPolicyMock.Object);

            var obtainedTransaction = connectionWrapper.BeginTransaction(testIsolationLevel);
            Assert.Equal(1, ((MockDbConnection) connectionWrapper.InnerConnection).BeginDbTransactionCount);
            Assert.Equal(testIsolationLevel, ((MockDbConnection) dbConnectionMock).LastUsedTransactionIsolationLevel);
            Assert.True(obtainedTransaction is ReliableDbTransactionWrapper);
            Assert.True(((ReliableDbTransactionWrapper) obtainedTransaction).InnerTransaction is MockDbTransaction);
        }

        [Fact]
        public void ShouldCreateCommandsInInnerConnectionAnReturnWrapper() 
        {
            var dbConnectionMock = new MockDbConnection();
            Mock<ISyncPolicy> retryPolicyMock = new Mock<ISyncPolicy>();
            var connectionWrapper = new ReliableDbConnectionWrapper(dbConnectionMock, retryPolicyMock.Object);

            var obtainedCommand = connectionWrapper.CreateCommand();
            Assert.Equal(1, ((MockDbConnection) connectionWrapper.InnerConnection).CreateCommandCount);
            Assert.True(obtainedCommand is ReliableDbCommandWrapper);
        }

        [Fact]
        public void ShouldCloseAndDisposeInnerConnection() 
        {
            var dbConnectionMock = new MockDbConnection();
            Mock<ISyncPolicy> retryPolicyMock = new Mock<ISyncPolicy>();
            var connectionWrapper = new ReliableDbConnectionWrapper(dbConnectionMock, retryPolicyMock.Object);

            connectionWrapper.Dispose();
            Assert.Equal(1, ((MockDbConnection) connectionWrapper.InnerConnection).CloseCount);
            Assert.Equal(1, ((MockDbConnection) connectionWrapper.InnerConnection).DisposeCount);
        }
    }
}