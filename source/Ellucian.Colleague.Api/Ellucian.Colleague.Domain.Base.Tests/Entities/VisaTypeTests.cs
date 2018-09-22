// Copyright 2014 Ellucian Company L.P. and its affiliates.
using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ellucian.Colleague.Domain.Base.Entities;

namespace Ellucian.Colleague.Domain.Base.Tests.Entities
{
    [TestClass]
    public class VisaTypeTests
    {
        private string code;
        private string description;
        private VisaType visaType;

        [TestInitialize]
        public void Initialize()
        {
            code = "HIS";
            description = "Hispanic/Latino";
            visaType = new VisaType(code, description);
        }

        [TestClass]
        public class VisaTypeConstructor : VisaTypeTests
        {
            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void VisaTypeConstructorNullCode()
            {
                visaType = new VisaType(null, description);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void VisaTypeConstructorEmptyCode()
            {
                visaType = new VisaType(string.Empty, description);
            }

            [TestMethod]
            public void VisaTypeConstructorValidCode()
            {
                visaType = new VisaType(code, description);
                Assert.AreEqual(code, visaType.Code);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void VisaTypeConstructorNullDescription()
            {
                visaType = new VisaType(code, null);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void VisaTypeConstructorEmptyDescription()
            {
                visaType = new VisaType(code, string.Empty);
            }

            [TestMethod]
            public void VisaTypeConstructorValidDescription()
            {
                visaType = new VisaType(code, description);
                Assert.AreEqual(description, visaType.Description);
            }
        }
    }
}
