/*Copyright 2016 Ellucian Company L.P. and its affiliates.*/
using Ellucian.Colleague.Domain.Base.Entities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Domain.Base.Tests.Entities
{
    [TestClass]
    public class TimestampTests
    {
        public string addOperator;
        public DateTimeOffset addDateTime;
        public string changeOperator;
        public DateTimeOffset changeDateTime;

        public Timestamp timestamp;


        public void TimestampTestsInitialize()
        {
            addOperator = "MCD";
            addDateTime = DateTime.Today;
            changeOperator = "MCD";
            changeDateTime = DateTime.Today;
        }

        [TestClass]
        public class ConstructorTests : TimestampTests
        {
            public new Timestamp timestamp
            {
                get
                {
                    return new Timestamp(addOperator, addDateTime, changeOperator, changeDateTime);
                }
            }

            [TestInitialize]
            public void Initialize()
            {
                TimestampTestsInitialize();
            }

            [TestMethod]
            public void AddOperatorTest()
            {
                Assert.AreEqual(addOperator, timestamp.AddOperator);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void AddOperatorRequiredTest()
            {
                addOperator = null;
                var invalid = timestamp;
            }

            [TestMethod]
            public void AddDateTimeTest()
            {
                Assert.AreEqual(addDateTime, timestamp.AddDateTime);
            }

            [TestMethod]
            public void ChangeOperatorTest()
            {
                Assert.AreEqual(changeOperator, timestamp.ChangeOperator);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void ChangeOperatorRequiredTest()
            {
                changeOperator = null;
                var invalid = timestamp;
            }

            [TestMethod]
            public void ChangeDateTimeTest()
            {
                Assert.AreEqual(changeDateTime, timestamp.ChangeDateTime);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentOutOfRangeException))]
            public void ChangeDateTimeCannotBeBeforeAddDateTimeTest()
            {
                changeDateTime = timestamp.AddDateTime.AddSeconds(-1);
                var invalid = timestamp;
            }
        }

        [TestClass]
        public class SetChangeStampTests : TimestampTests
        {
            [TestInitialize]
            public void Initialize()
            {
                TimestampTestsInitialize();
                timestamp = new Timestamp(addOperator, addDateTime, changeOperator, changeDateTime);
            }

            [TestMethod]
            public void SetChangeStampTest()
            {
                changeOperator = "FOOBAR";
                changeDateTime = addDateTime.AddSeconds(1);

                timestamp.SetChangeStamp(changeOperator, changeDateTime);

                Assert.AreEqual(changeOperator, timestamp.ChangeOperator);
                Assert.AreEqual(changeDateTime, timestamp.ChangeDateTime);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void ChangeOperatorRequiredTest()
            {
                timestamp.SetChangeStamp(null, changeDateTime);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentOutOfRangeException))]
            public void ChangeDateTimeCannotBeBeforeAddDateTimeTest()
            {
                changeDateTime = addDateTime.AddSeconds(-1);

                timestamp.SetChangeStamp(changeOperator, changeDateTime);
            }

        }

        [TestClass]
        public class EqualsAndHashCodeTests : TimestampTests
        {
            public Timestamp getTimestamp()
            {
                return new Timestamp(addOperator, addDateTime, changeOperator, changeDateTime);
            }

            [TestInitialize]
            public void Initialize()
            {
                TimestampTestsInitialize();
            }

            [TestMethod]
            public void AreEqualTest()
            {
                Assert.IsTrue(getTimestamp().Equals(getTimestamp()));
                Assert.AreEqual(getTimestamp().GetHashCode(), getTimestamp().GetHashCode());
            }

            [TestMethod]
            public void DifferentAddOperatorTest()
            {
                var t1 = getTimestamp();
                addOperator = "FOOBAR";

                Assert.IsFalse(t1.Equals(getTimestamp()));
                Assert.AreNotEqual(t1.GetHashCode(), getTimestamp().GetHashCode());
            }

            [TestMethod]
            public void DifferentAddDateTimeTest()
            {
                var t1 = getTimestamp();
                addDateTime = addDateTime.AddSeconds(-1);

                Assert.IsFalse(t1.Equals(getTimestamp()));
                Assert.AreNotEqual(t1.GetHashCode(), getTimestamp().GetHashCode());
            }

            [TestMethod]
            public void DifferentChangeOperatorTest()
            {
                var t1 = getTimestamp();
                changeOperator = "FOOBAR";

                Assert.IsFalse(t1.Equals(getTimestamp()));
                Assert.AreNotEqual(t1.GetHashCode(), getTimestamp().GetHashCode());
            }

            [TestMethod]
            public void DifferentChangeDateTimeTest()
            {
                var t1 = getTimestamp();
                changeDateTime = changeDateTime.AddSeconds(1);

                Assert.IsFalse(t1.Equals(getTimestamp()));
                Assert.AreNotEqual(t1.GetHashCode(), getTimestamp().GetHashCode());
            }

            [TestMethod]
            public void NullInputTest()
            {

                Assert.IsFalse(getTimestamp().Equals(null));
            }

            [TestMethod]
            public void DifferentTypeTest()
            {
                Assert.IsFalse(getTimestamp().Equals(new Person("0003914", "DeDiana")));
            }
        }
    }
}
