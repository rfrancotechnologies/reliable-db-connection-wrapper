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
    public class ReliableDbCommandWrapperTests
    {
        private Fixture fixture;
        public ReliableDbCommandWrapperTests() 
        {
            fixture = new Fixture();
        }

        [Fact]
        public void ShouldReadCommandTextFromInnerCommand() 
        {
            string testCommandText = fixture.Create<string>();
            Mock<DbCommand> dbCommandMock = new Mock<DbCommand>();
            Mock<ISyncPolicy> retryPolicyMock = new Mock<ISyncPolicy>();
            dbCommandMock.Setup(x => x.CommandText).Returns(testCommandText);
            var commandWrapper = new ReliableDbCommandWrapper(dbCommandMock.Object, retryPolicyMock.Object);
            Assert.Equal(testCommandText, commandWrapper.CommandText);
        }

        [Fact]
        public void ShouldSetCommandTextToInnerCommand()
        {
            string testCommandText = fixture.Create<string>();
            Mock<DbCommand> dbCommandMock = new Mock<DbCommand>();
            Mock<ISyncPolicy> retryPolicyMock = new Mock<ISyncPolicy>();
            var commandWrapper = new ReliableDbCommandWrapper(dbCommandMock.Object, retryPolicyMock.Object);

            commandWrapper.CommandText = testCommandText;
            dbCommandMock.VerifySet(x => x.CommandText = testCommandText);
        }

        [Fact]
        public void ShouldReadCommandTimeoutFromInnerCommand() 
        {
            int testTimeOut = fixture.Create<int>();
            Mock<DbCommand> dbCommandMock = new Mock<DbCommand>();
            Mock<ISyncPolicy> retryPolicyMock = new Mock<ISyncPolicy>();
            dbCommandMock.Setup(x => x.CommandTimeout).Returns(testTimeOut);
            var commandWrapper = new ReliableDbCommandWrapper(dbCommandMock.Object, retryPolicyMock.Object);
            Assert.Equal(testTimeOut, commandWrapper.CommandTimeout);
        }

        [Fact]
        public void ShouldSetCommandTimeoutToInnerCommand()
        {
            int testTimeOut = fixture.Create<int>();
            Mock<DbCommand> dbCommandMock = new Mock<DbCommand>();
            Mock<ISyncPolicy> retryPolicyMock = new Mock<ISyncPolicy>();
            var commandWrapper = new ReliableDbCommandWrapper(dbCommandMock.Object, retryPolicyMock.Object);

            commandWrapper.CommandTimeout = testTimeOut;
            dbCommandMock.VerifySet(x => x.CommandTimeout = testTimeOut);
        }

        [Fact]
        public void ShouldReadCommandTypeFromInnerCommand() 
        {
            CommandType testCommandType = fixture.Create<CommandType>();
            Mock<DbCommand> dbCommandMock = new Mock<DbCommand>();
            Mock<ISyncPolicy> retryPolicyMock = new Mock<ISyncPolicy>();
            dbCommandMock.Setup(x => x.CommandType).Returns(testCommandType);
            var commandWrapper = new ReliableDbCommandWrapper(dbCommandMock.Object, retryPolicyMock.Object);
            Assert.Equal(testCommandType, commandWrapper.CommandType);
        }

        [Fact]
        public void ShouldSetCommandTypeToInnerCommand()
        {
            CommandType testCommandType = fixture.Create<CommandType>();
            Mock<DbCommand> dbCommandMock = new Mock<DbCommand>();
            Mock<ISyncPolicy> retryPolicyMock = new Mock<ISyncPolicy>();
            var commandWrapper = new ReliableDbCommandWrapper(dbCommandMock.Object, retryPolicyMock.Object);

            commandWrapper.CommandType = testCommandType;
            dbCommandMock.VerifySet(x => x.CommandType = testCommandType);
        }

        [Fact]
        public void ShouldReadDesignTimeVisibleFromInnerCommand() 
        {
            bool testDesignTimeVisible = fixture.Create<bool>();
            Mock<DbCommand> dbCommandMock = new Mock<DbCommand>();
            Mock<ISyncPolicy> retryPolicyMock = new Mock<ISyncPolicy>();
            dbCommandMock.Setup(x => x.DesignTimeVisible).Returns(testDesignTimeVisible);
            var commandWrapper = new ReliableDbCommandWrapper(dbCommandMock.Object, retryPolicyMock.Object);
            Assert.Equal(testDesignTimeVisible, commandWrapper.DesignTimeVisible);
        }

        [Fact]
        public void ShouldSetDesignTimeVisibleToInnerCommand()
        {
            bool testDesignTimeVisible = fixture.Create<bool>();
            Mock<DbCommand> dbCommandMock = new Mock<DbCommand>();
            Mock<ISyncPolicy> retryPolicyMock = new Mock<ISyncPolicy>();
            var commandWrapper = new ReliableDbCommandWrapper(dbCommandMock.Object, retryPolicyMock.Object);

            commandWrapper.DesignTimeVisible = testDesignTimeVisible;
            dbCommandMock.VerifySet(x => x.DesignTimeVisible = testDesignTimeVisible);
        }        

        [Fact]
        public void ShouldReadUpdatedRowSourceFromInnerCommand() 
        {
            UpdateRowSource testUpdatedRowSource = fixture.Create<UpdateRowSource>();
            Mock<DbCommand> dbCommandMock = new Mock<DbCommand>();
            Mock<ISyncPolicy> retryPolicyMock = new Mock<ISyncPolicy>();
            dbCommandMock.Setup(x => x.UpdatedRowSource).Returns(testUpdatedRowSource);
            var commandWrapper = new ReliableDbCommandWrapper(dbCommandMock.Object, retryPolicyMock.Object);
            Assert.Equal(testUpdatedRowSource, commandWrapper.UpdatedRowSource);
        }

        [Fact]
        public void ShouldSetUpdatedRowSourceToInnerCommand()
        {
            UpdateRowSource testUpdatedRowSource = fixture.Create<UpdateRowSource>();
            Mock<DbCommand> dbCommandMock = new Mock<DbCommand>();
            Mock<ISyncPolicy> retryPolicyMock = new Mock<ISyncPolicy>();
            var commandWrapper = new ReliableDbCommandWrapper(dbCommandMock.Object, retryPolicyMock.Object);

            commandWrapper.UpdatedRowSource = testUpdatedRowSource;
            dbCommandMock.VerifySet(x => x.UpdatedRowSource = testUpdatedRowSource);
        }        

        [Fact]
        public void ShouldSetDbConnectionToInnerCommand()
        {
            Mock<DbConnection> testDbConnection = new Mock<DbConnection>();
            MockDbCommand dbCommandMock = new MockDbCommand();
            Mock<ISyncPolicy> retryPolicyMock = new Mock<ISyncPolicy>();
            var commandWrapper = new ReliableDbCommandWrapper(dbCommandMock, retryPolicyMock.Object);

            commandWrapper.Connection = testDbConnection.Object;
            Assert.Equal(testDbConnection.Object, dbCommandMock.Connection);
        }

        [Fact]
        public void ShouldReadDbParameterCollectionFromInnerCommand() 
        {
            MockDbCommand dbCommandMock = new MockDbCommand();
            Mock<ISyncPolicy> retryPolicyMock = new Mock<ISyncPolicy>();
            var commandWrapper = new ReliableDbCommandWrapper(dbCommandMock, retryPolicyMock.Object);

            Mock<DbParameterCollection> mockParameterCollection = new Mock<DbParameterCollection>();
            mockParameterCollection.Setup(x => x.ToString()).Returns("TestParameterCollection");
            dbCommandMock.NextParameterCollectionToReturn = mockParameterCollection.Object;

            Assert.Equal("TestParameterCollection", commandWrapper.Parameters.ToString());
        }

        [Fact]
        public void ShouldReturnNullTransactionWhenInnerCommandHasNoTransaction() 
        {
            MockDbCommand dbCommandMock = new MockDbCommand();
            Mock<ISyncPolicy> retryPolicyMock = new Mock<ISyncPolicy>();
            var commandWrapper = new ReliableDbCommandWrapper(dbCommandMock, retryPolicyMock.Object);

            dbCommandMock.Transaction = null;
            Assert.Null(commandWrapper.Transaction);
        }

        [Fact]
        public void ShouldReadTransactionFromInnerCommandAndWrap() 
        {
            MockDbCommand dbCommandMock = new MockDbCommand();
            Mock<ISyncPolicy> retryPolicyMock = new Mock<ISyncPolicy>();
            var commandWrapper = new ReliableDbCommandWrapper(dbCommandMock, retryPolicyMock.Object);
            MockDbTransaction mockDbTransaction = new MockDbTransaction();

            dbCommandMock.Transaction = mockDbTransaction;
            Assert.True(commandWrapper.Transaction is ReliableDbTransactionWrapper);
            Assert.Equal(mockDbTransaction, ((ReliableDbTransactionWrapper) commandWrapper.Transaction).InnerTransaction);
        }

        [Fact]
        public void ShouldSetTransactionToInnerCommand() 
        {
            MockDbCommand dbCommandMock = new MockDbCommand();
            Mock<ISyncPolicy> retryPolicyMock = new Mock<ISyncPolicy>();
            var commandWrapper = new ReliableDbCommandWrapper(dbCommandMock, retryPolicyMock.Object);
            MockDbTransaction mockDbTransaction = new MockDbTransaction();

            commandWrapper.Transaction = mockDbTransaction;
            Assert.Equal(mockDbTransaction, dbCommandMock.Transaction);
        }

        [Fact]
        public void ShouldSetWrappedTransactionToInnerCommand() 
        {
            MockDbCommand dbCommandMock = new MockDbCommand();
            Mock<ISyncPolicy> retryPolicyMock = new Mock<ISyncPolicy>();
            var commandWrapper = new ReliableDbCommandWrapper(dbCommandMock, retryPolicyMock.Object);
            
            MockDbTransaction mockDbTransaction = new MockDbTransaction();
            ReliableDbTransactionWrapper transactionWrapper = new ReliableDbTransactionWrapper(mockDbTransaction, null, null);

            commandWrapper.Transaction = transactionWrapper;
            Assert.Equal(mockDbTransaction, dbCommandMock.Transaction);
        }

        [Fact]
        public void ShouldCancelInnerCommand() 
        {
            Mock<DbCommand> dbCommandMock = new Mock<DbCommand>();
            Mock<ISyncPolicy> retryPolicyMock = new Mock<ISyncPolicy>();
            var commandWrapper = new ReliableDbCommandWrapper(dbCommandMock.Object, retryPolicyMock.Object);
            
            commandWrapper.Cancel();
            dbCommandMock.Verify(x => x.Cancel());
        }

        [Fact]
        public void ShouldExecuteNonQueryWithRetriesInInnerCommandAndReturnResult() 
        {
            int testAffectedRows = fixture.Create<int>();
            Mock<DbCommand> dbCommandMock = new Mock<DbCommand>();
            Mock<ISyncPolicy> retryPolicyMock = new Mock<ISyncPolicy>();
            var commandWrapper = new ReliableDbCommandWrapper(dbCommandMock.Object, retryPolicyMock.Object);
            
            retryPolicyMock.Setup(x => x.Execute<int>(It.IsAny<Func<int>>()))
                .Returns(testAffectedRows)
                .Callback<Func<int>>(a => a.Invoke());

            int obtainedAffectedRows = commandWrapper.ExecuteNonQuery();
            retryPolicyMock.Verify(x => x.Execute<int>(It.IsAny<Func<int>>()));
            dbCommandMock.Verify(x => x.ExecuteNonQuery());
            Assert.Equal(testAffectedRows, obtainedAffectedRows);
        }

        [Fact]
        public void ShouldExecuteScalarWithRetriesInInnerCommandAndReturnResult() 
        {
            int testScalarResult = fixture.Create<int>();
            Mock<DbCommand> dbCommandMock = new Mock<DbCommand>();
            Mock<ISyncPolicy> retryPolicyMock = new Mock<ISyncPolicy>();
            var commandWrapper = new ReliableDbCommandWrapper(dbCommandMock.Object, retryPolicyMock.Object);
            
            retryPolicyMock.Setup(x => x.Execute<object>(It.IsAny<Func<object>>()))
                .Returns(testScalarResult)
                .Callback<Func<object>>(a => a.Invoke());

            int obtainedScalarResult = (int) commandWrapper.ExecuteScalar();
            retryPolicyMock.Verify(x => x.Execute<object>(It.IsAny<Func<object>>()));
            dbCommandMock.Verify(x => x.ExecuteScalar());
            Assert.Equal(testScalarResult, obtainedScalarResult);
        }

        [Fact]
        public void ShouldPrepareWithRetriesInnerCommand() 
        {
            Mock<DbCommand> dbCommandMock = new Mock<DbCommand>();
            Mock<ISyncPolicy> retryPolicyMock = new Mock<ISyncPolicy>();
            var commandWrapper = new ReliableDbCommandWrapper(dbCommandMock.Object, retryPolicyMock.Object);
            retryPolicyMock.Setup(x => x.Execute(It.IsAny<Action>()))
                .Callback<Action>(a => a.Invoke());
            
            commandWrapper.Prepare();
            retryPolicyMock.Verify(x => x.Execute(It.IsAny<Action>()));
            dbCommandMock.Verify(x => x.Prepare());
        }
        
        [Fact]
        public void ShouldCreateNewParametersInInnerCommand() 
        {
            MockDbCommand dbCommandMock = new MockDbCommand();
            Mock<ISyncPolicy> retryPolicyMock = new Mock<ISyncPolicy>();
            var commandWrapper = new ReliableDbCommandWrapper(dbCommandMock, retryPolicyMock.Object);
            Mock<DbParameter> mockDbParameter = new Mock<DbParameter>();
            
            dbCommandMock.NextParameterToReturn = mockDbParameter.Object;
            var obtainedParameter = commandWrapper.CreateParameter();
            Assert.Equal(mockDbParameter.Object, obtainedParameter);
        }

        [Fact]
        public void ShouldExecuteDbDataReaderWithRetriesInInnerCommand() 
        {
            MockDbCommand dbCommandMock = new MockDbCommand();
            Mock<ISyncPolicy> retryPolicyMock = new Mock<ISyncPolicy>();
            var commandWrapper = new ReliableDbCommandWrapper(dbCommandMock, retryPolicyMock.Object);
            Mock<DbDataReader> mockDbDataReader = new Mock<DbDataReader>();
            mockDbDataReader.Setup(x => x.GetName(1)).Returns("TestColumnName");
            retryPolicyMock.Setup(x => x.Execute<DbDataReader>(It.IsAny<Func<DbDataReader>>()))
                .Returns(mockDbDataReader.Object)
                .Callback<Func<DbDataReader>>(a => a.Invoke());

            var obtainedReader = commandWrapper.ExecuteReader();
            retryPolicyMock.Verify(x => x.Execute<DbDataReader>(It.IsAny<Func<DbDataReader>>()));
            Assert.Equal("TestColumnName", obtainedReader.GetName(1));
            Assert.Equal(1, dbCommandMock.ExecuteDbDataReaderCount);
        }
        
        [Fact]
        public void ShoulDisposeInnerCommand() 
        {
            MockDbCommand dbCommandMock = new MockDbCommand();
            Mock<ISyncPolicy> retryPolicyMock = new Mock<ISyncPolicy>();
            var commandWrapper = new ReliableDbCommandWrapper(dbCommandMock, retryPolicyMock.Object);
            commandWrapper.Dispose();
            Assert.Equal(1, dbCommandMock.DisposeCount);
        }
    }
}