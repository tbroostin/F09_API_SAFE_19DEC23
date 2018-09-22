// Copyright 2015 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Coordination.Base.Adapters;
using Ellucian.Colleague.Dtos.Base;
using Ellucian.Web.Adapters;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;
using System;
using System.Collections.Generic;

namespace Ellucian.Colleague.Coordination.Base.Tests.Adapters
{
    [TestClass]
    public class DateTimeOffsetToDateTimeAdapterTests
    {
        DateTimeOffset offset;

        DateTime dateTime;
        DateTimeOffsetToDateTimeAdapter dateTimeOffsetToDateTimeAdapter;

        [TestInitialize]
        public void Initialize()
        {
            offset = new DateTimeOffset(2015, 1, 15, 12, 34, 56, new TimeSpan(4, 0, 0));

            var loggerMock = new Mock<ILogger>();
            var adapterRegistryMock = new Mock<IAdapterRegistry>();

            dateTimeOffsetToDateTimeAdapter = new DateTimeOffsetToDateTimeAdapter(adapterRegistryMock.Object, loggerMock.Object);

            dateTime = dateTimeOffsetToDateTimeAdapter.MapToType(offset);
        }

        [TestCleanup]
        public void Cleanup()
        {

        }

        [TestMethod]
        public void DateTimeOffsetToDateTimeAdapterTests_MapToType()
        {
            Assert.AreEqual(dateTime, offset.DateTime);
        }
    }
}