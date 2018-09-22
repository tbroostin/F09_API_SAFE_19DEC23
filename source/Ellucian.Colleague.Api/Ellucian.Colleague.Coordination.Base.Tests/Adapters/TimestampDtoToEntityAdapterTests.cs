/*Copyright 2016 Ellucian Company L.P. and its affiliates.*/
using Ellucian.Colleague.Coordination.Base.Adapters;
using Ellucian.Web.Adapters;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.Base.Tests.Adapters
{
    [TestClass]
    public class TimestampDtoToEntityAdapterTests
    {
        public Dtos.Base.Timestamp inputSource;
        public Domain.Base.Entities.Timestamp actualTimestamp
        {
            get
            {
                return adapterUnderTest.MapToType(inputSource);
            }
        }

        public Mock<IAdapterRegistry> adapterRegistryMock;
        public Mock<ILogger> loggerMock;

        public TimestampDtoToEntityAdapter adapterUnderTest;

        [TestInitialize]
        public void Initialize()
        {
            inputSource = new Dtos.Base.Timestamp()
            {
                AddDateTime = DateTime.Today,
                AddOperator = "MCD",
                ChangeDateTime = DateTime.Today,
                ChangeOperator = "MCD"
            };

            adapterRegistryMock = new Mock<IAdapterRegistry>();
            loggerMock = new Mock<ILogger>();

            adapterUnderTest = new TimestampDtoToEntityAdapter(adapterRegistryMock.Object, loggerMock.Object);
        }

        [TestMethod]
        public void SuccessfulMapTest()
        {
            Assert.AreEqual(inputSource.AddDateTime, actualTimestamp.AddDateTime);
            Assert.AreEqual(inputSource.AddOperator, actualTimestamp.AddOperator);
            Assert.AreEqual(inputSource.ChangeDateTime, actualTimestamp.ChangeDateTime);
            Assert.AreEqual(inputSource.ChangeOperator, actualTimestamp.ChangeOperator);
        }

        [TestMethod]
        public void MapNullTest()
        {
            inputSource = null;
            Assert.IsNull(actualTimestamp);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void DomainExceptionsUnhandledTest()
        {
            inputSource.AddOperator = null;
            var invalid = actualTimestamp;
        }
    }
}
