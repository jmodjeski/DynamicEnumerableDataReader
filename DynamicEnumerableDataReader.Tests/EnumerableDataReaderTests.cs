using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace ModjeskiNet.Data.Tests
{
    [TestClass]
    public class EnumerableDataReaderTests
    {
        private Mock<IDataReader> _reader = new Mock<IDataReader>();

        [TestMethod]
        public void LinqFiltered()
        {
            const int limit = 5;
            var callcount = 0;
            _reader.Setup(x => x.GetValue(It.IsAny<int>())).Returns(()=>String.Format("Test{0}", callcount));
            _reader.Setup(x => x.Read()).Returns(() => callcount++ < limit);

            _reader.Object.AsEnumerable().Where(x => x.Name == "Test1").Count().Should().Be(1);
        }

        [TestMethod]
        public void StringFiltered()
        {
            const int limit = 5;
            var callcount = 0;
            _reader.Setup(x => x.GetValue(It.IsAny<int>())).Returns(() => String.Format("Test{0}", callcount));
            _reader.Setup(x => x.Read()).Returns(() => callcount++ < limit);

            _reader.Object.AsEnumerable().Where("([Name] == 'Test1') || ([Name] == 'Test2')").Count().Should().Be(2);
        }
    }
}
