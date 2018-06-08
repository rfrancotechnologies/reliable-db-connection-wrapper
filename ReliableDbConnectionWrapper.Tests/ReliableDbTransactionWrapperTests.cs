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
    public class ReliableDbTransactionWrapperTests
    {
        [Fact]
        public void ShouldTakeIsolationLevelFromInnerTransaction() 
        {
            IsolationLevel testIsolationLevel = IsolationLevel.ReadCommitted;
            Mock<DbConnection> dbConnectionMock = new Mock<DbConnection>();
            Mock<DbTransaction> dbTransactionMock = new Mock<DbTransaction>();
            Mock<ISyncPolicy> retryPolicyMock = new Mock<ISyncPolicy>();
            ReliableDbConnectionWrapper connectionWrapper = new ReliableDbConnectionWrapper(dbConnectionMock.Object, retryPolicyMock.Object);
            
            dbTransactionMock.Setup(x => x.IsolationLevel).Returns(testIsolationLevel);

            var transactionWrapper = new ReliableDbTransactionWrapper(dbTransactionMock.Object, connectionWrapper, retryPolicyMock.Object);
            Assert.Equal(testIsolationLevel, transactionWrapper.IsolationLevel);
        }

        [Fact]
        public void ShouldReturnConnectionGivenOnConstruction() 
        {
            Mock<DbConnection> dbConnectionMock = new Mock<DbConnection>();
            Mock<DbTransaction> dbTransactionMock = new Mock<DbTransaction>();
            Mock<ISyncPolicy> retryPolicyMock = new Mock<ISyncPolicy>();
            ReliableDbConnectionWrapper connectionWrapper = new ReliableDbConnectionWrapper(dbConnectionMock.Object, retryPolicyMock.Object);
            
            var transactionWrapper = new ReliableDbTransactionWrapper(dbTransactionMock.Object, connectionWrapper, retryPolicyMock.Object);
            Assert.Equal(connectionWrapper, transactionWrapper.Connection);
        }

        [Fact]
        public void ShouldCommitWithRetriesInInnerTransaction() 
        {
            Mock<DbConnection> dbConnectionMock = new Mock<DbConnection>();
            Mock<DbTransaction> dbTransactionMock = new Mock<DbTransaction>();
            Mock<ISyncPolicy> retryPolicyMock = new Mock<ISyncPolicy>();
            ReliableDbConnectionWrapper connectionWrapper = new ReliableDbConnectionWrapper(dbConnectionMock.Object, retryPolicyMock.Object);
            
            var transactionWrapper = new ReliableDbTransactionWrapper(dbTransactionMock.Object, connectionWrapper, retryPolicyMock.Object);
            retryPolicyMock.Setup(x => x.Execute(It.IsAny<Action>())).Callback<Action>(a => a.Invoke());
            
            transactionWrapper.Commit();
            retryPolicyMock.Verify(x => x.Execute(It.IsAny<Action>()));
            dbTransactionMock.Verify(x => x.Commit());
        }

        [Fact]
        public void ShouldRollBackWithRetriesInInnerTransaction() 
        {
            Mock<DbConnection> dbConnectionMock = new Mock<DbConnection>();
            Mock<DbTransaction> dbTransactionMock = new Mock<DbTransaction>();
            Mock<ISyncPolicy> retryPolicyMock = new Mock<ISyncPolicy>();
            ReliableDbConnectionWrapper connectionWrapper = new ReliableDbConnectionWrapper(dbConnectionMock.Object, retryPolicyMock.Object);
            
            var transactionWrapper = new ReliableDbTransactionWrapper(dbTransactionMock.Object, connectionWrapper, retryPolicyMock.Object);
            retryPolicyMock.Setup(x => x.Execute(It.IsAny<Action>())).Callback<Action>(a => a.Invoke());
            
            transactionWrapper.Rollback();
            retryPolicyMock.Verify(x => x.Execute(It.IsAny<Action>()));
            dbTransactionMock.Verify(x => x.Rollback());
        }

        [Fact]
        public void ShouldDisposeInnerTransaction() 
        {
            Mock<DbConnection> dbConnectionMock = new Mock<DbConnection>();
            MockDbTransaction dbTransactionMock = new MockDbTransaction();
            Mock<ISyncPolicy> retryPolicyMock = new Mock<ISyncPolicy>();
            ReliableDbConnectionWrapper connectionWrapper = new ReliableDbConnectionWrapper(dbConnectionMock.Object, retryPolicyMock.Object);
            
            var transactionWrapper = new ReliableDbTransactionWrapper(dbTransactionMock, connectionWrapper, retryPolicyMock.Object);
            
            transactionWrapper.Dispose();
            Assert.Equal(1, dbTransactionMock.DisposalCount);
        }
    }
}