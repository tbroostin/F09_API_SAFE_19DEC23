// Copyright 2019 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Domain.Base.Entities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace Ellucian.Colleague.Domain.Base.Tests.Entities
{
    [TestClass]
    public class EncrKeyTests
    {
        [TestClass]
        public class EncrKey_ConstructorTests
        {
            [TestMethod]
            public void EncrKey_Constructor_Success()
            {
                var actual = new EncrKey("f070516e-09f4-4232-af06-78391100e213", "test key", "key",
                    1, EncrKeyStatus.Active);
                Assert.AreEqual("f070516e-09f4-4232-af06-78391100e213", actual.Id);
                Assert.AreEqual("test key", actual.Name);
                Assert.AreEqual("key", actual.Key);
                Assert.AreEqual(1, actual.Version);
                Assert.AreEqual("Active", actual.Status.ToString());
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void EncrKey_Constructor_NullId()
            {
                new EncrKey(null, "test key", "key", 1, EncrKeyStatus.Active);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void EncrKey_Constructor_NullName()
            {
                new EncrKey("f070516e-09f4-4232-af06-78391100e213", null, "key", 1, EncrKeyStatus.Active);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void EncrKey_Constructor_NullKey()
            {
                new EncrKey("f070516e-09f4-4232-af06-78391100e213", "test key", null, 1, EncrKeyStatus.Active);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public void EncrKey_Constructor_InvalidVersion()
            {
                new EncrKey("f070516e-09f4-4232-af06-78391100e213", "test key", "key", 0, EncrKeyStatus.Active);
            }
        }

        [TestClass]
        public class EncrKey_PropertyTests
        {
            [TestMethod]
            [ExpectedException(typeof(InvalidOperationException))]
            public void EncrKey_Property_ChangeId()
            {
                var actual = new EncrKey("f070516e-09f4-4232-af06-78391100e213", "test key", "key",
                    1, EncrKeyStatus.Active);
                actual.Id = "cannot change";
            }

            [TestMethod]
            [ExpectedException(typeof(InvalidOperationException))]
            public void EncrKey_Property_ChangeKey()
            {
                var actual = new EncrKey("f070516e-09f4-4232-af06-78391100e213", "test key", "key",
                    1, EncrKeyStatus.Active);
                actual.Key = "cannot change";
            }

            [TestMethod]
            [ExpectedException(typeof(InvalidOperationException))]
            public void EncrKey_Property_ChangeVersion()
            {
                var actual = new EncrKey("f070516e-09f4-4232-af06-78391100e213", "test key", "key",
                    1, EncrKeyStatus.Active);
                actual.Version = 2;
            }
        }
    }
}