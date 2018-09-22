// Copyright 2016-2017 Ellucian Company L.P. and its affiliates.

using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ellucian.Colleague.Domain.Student.Entities;

namespace Ellucian.Colleague.Domain.Student.Tests.Entities
{
    [TestClass]
    public class TestSourceTests
    {
        [TestClass]
        public class TestSourceConstructor
        {
            private string guid;
            private string code;
            private string desc;
            private TestSource testSource;

            [TestInitialize]
            public void Initialize()
            {
                guid = Guid.NewGuid().ToString();
                code = "UG";
                desc = "Undergraduate";
                testSource = new TestSource(guid, code, desc);
            }

            [TestMethod]
            public void TestSourceGuid()
            {
                Assert.AreEqual(guid, testSource.Guid);
            }

            [TestMethod]
            public void TestSourceCode()
            {
                Assert.AreEqual(code, testSource.Code);
            }

            [TestMethod]
            public void TestSourceDescription()
            {
                Assert.AreEqual(desc, testSource.Description);
            }

            [TestMethod]
            [ExpectedException(typeof (ArgumentNullException))]
            public void TestSourceGuidNullException()
            {
                new TestSource(null, code, desc);
            }

            [TestMethod]
            [ExpectedException(typeof (ArgumentNullException))]
            public void TestSourceCodeNullException()
            {
                new TestSource(guid, null, desc);
            }

            [TestMethod]
            [ExpectedException(typeof (ArgumentNullException))]
            public void TestSourceDescNullException()
            {
                new TestSource(guid, code, null);
            }

            [TestMethod]
            [ExpectedException(typeof (ArgumentNullException))]
            public void TestSourceGuidEmptyException()
            {
                new TestSource(string.Empty, code, desc);
            }

            [TestMethod]
            [ExpectedException(typeof (ArgumentNullException))]
            public void TestSourceCodeEmptyException()
            {
                new TestSource(guid, string.Empty, desc);
            }

            [TestMethod]
            [ExpectedException(typeof (ArgumentNullException))]
            public void TestSourceDescEmptyException()
            {
                new TestSource(guid, code, string.Empty);
            }
        }
    }
}