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
    /// Test mock class for those DbTransaction methods that are not virtual or protected and cannot
    /// be mocked from a standar Moq Mock<>.
    /// </summary>
    public class MockDbTransaction : DbTransaction
    {
        public int DisposalCount { get; set; }
        public override IsolationLevel IsolationLevel => throw new NotImplementedException();

        protected override DbConnection DbConnection => throw new NotImplementedException();

        public override void Commit()
        {
            throw new NotImplementedException();
        }

        public override void Rollback()
        {
            throw new NotImplementedException();
        }

        protected override void Dispose(bool disposing)
        {
            lock(this) {
                DisposalCount++;
            }
        }
    }
}