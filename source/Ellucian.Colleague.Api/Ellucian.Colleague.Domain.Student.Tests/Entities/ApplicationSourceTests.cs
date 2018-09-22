// Copyright 2017 Ellucian Company L.P. and its affiliates.

using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ellucian.Colleague.Domain.Student.Entities;

namespace Ellucian.Colleague.Domain.Student.Tests.Entities
{
    [TestClass]
    public class ApplicationSourceTests
    {
        [TestClass]
        public class ApplicationSourceConstructor
        {
            private string guid;
            private string code;
            private string desc;
            private ApplicationSource applicationSource;

            [TestInitialize]
            public void Initialize()
            {
                guid = Guid.NewGuid().ToString();
                code = "UG";
                desc = "Undergraduate";
                applicationSource = new ApplicationSource(guid, code, desc);
            }

            [TestMethod]
            public void ApplicationSourceGuid()
            {
                Assert.AreEqual(guid, applicationSource.Guid);
            }

            [TestMethod]
            public void ApplicationSourceCode()
            {
                Assert.AreEqual(code, applicationSource.Code);
            }

            [TestMethod]
            public void ApplicationSourceDescription()
            {
                Assert.AreEqual(desc, applicationSource.Description);
            }

            [TestMethod]
            [ExpectedException(typeof (ArgumentNullException))]
            public void ApplicationSourceGuidNullException()
            {
                new ApplicationSource(null, code, desc);
            }

            [TestMethod]
            [ExpectedException(typeof (ArgumentNullException))]
            public void ApplicationSourceCodeNullException()
            {
                new ApplicationSource(guid, null, desc);
            }

            [TestMethod]
            [ExpectedException(typeof (ArgumentNullException))]
            public void ApplicationSourceDescNullException()
            {
                new ApplicationSource(guid, code, null);
            }

            [TestMethod]
            [ExpectedException(typeof (ArgumentNullException))]
            public void ApplicationSourceGuidEmptyException()
            {
                new ApplicationSource(string.Empty, code, desc);
            }

            [TestMethod]
            [ExpectedException(typeof (ArgumentNullException))]
            public void ApplicationSourceCodeEmptyException()
            {
                new ApplicationSource(guid, string.Empty, desc);
            }

            [TestMethod]
            [ExpectedException(typeof (ArgumentNullException))]
            public void ApplicationSourceDescEmptyException()
            {
                new ApplicationSource(guid, code, string.Empty);
            }
        }
    }
}