// Copyright 2016 Ellucian Company L.P. and its affiliates.
using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ellucian.Colleague.Domain.Base.Entities;

namespace Ellucian.Colleague.Domain.Base.Tests.Entities
{
    [TestClass]
    public class SourceContextTests
    {
        private string guid;
        private string code;
        private string description;
        SourceContext sourceContext;


        [TestInitialize]
        public void Initialize()
        {
            guid = Guid.NewGuid().ToString();
            code = "REMARKS";
            description = "Remarks Source Codes";
        }

        [TestClass]
        public class SourceContextConstructor : SourceContextTests
        {
            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void SourceContextConstructorNullGuid()
            {
                sourceContext = new SourceContext(null, code, description);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void SourceContextConstructorEmptyGuid()
            {
                sourceContext = new SourceContext(string.Empty, code, description);
            }

            [TestMethod]
            public void SourceContextConstructorValidGuid()
            {
                sourceContext = new SourceContext(guid, code, description);
                Assert.AreEqual(guid, sourceContext.Guid);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void SourceContextConstructorNullCode()
            {
                sourceContext = new SourceContext(guid, null, description);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void SourceContextConstructorEmptyCode()
            {
                sourceContext = new SourceContext(guid, string.Empty, description);
            }

            [TestMethod]
            public void SourceContextConstructorValidCode()
            {
                sourceContext = new SourceContext(guid, code, description);
                Assert.AreEqual(code, sourceContext.Code);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void SourceContextConstructorNullDescription()
            {
                sourceContext = new SourceContext(guid, code, null);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void SourceContextConstructorEmptyDescription()
            {
                sourceContext = new SourceContext(guid, code, string.Empty);
            }

            [TestMethod]
            public void SourceContextConstructorValidDescription()
            {
                sourceContext = new SourceContext(guid, code, description);
                Assert.AreEqual(description, sourceContext.Description);
            }
        }
    }
}