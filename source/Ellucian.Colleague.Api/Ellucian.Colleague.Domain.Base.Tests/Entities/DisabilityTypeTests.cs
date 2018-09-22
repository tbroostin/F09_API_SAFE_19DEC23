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
    public class DisabilityTypeTests
    {
        private string code;
        private string description;
        private DisabilityType disType;

        [TestInitialize]
        public void Initialize()
        {
            code = "HIS";
            description = "Hispanic/Latino";
            disType = new DisabilityType(code, description);
        }

        [TestClass]
        public class DisabilityTypeConstructor : DisabilityTypeTests
        {
            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void DisabilityTypeConstructorNullCode()
            {
                disType = new DisabilityType(null, description);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void DisabilityTypeConstructorEmptyCode()
            {
                disType = new DisabilityType(string.Empty, description);
            }

            [TestMethod]
            public void DisabilityTypeConstructorValidCode()
            {
                disType = new DisabilityType(code, description);
                Assert.AreEqual(code, disType.Code);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void DisabilityTypeConstructorNullDescription()
            {
                disType = new DisabilityType(code, null);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void DisabilityTypeConstructorEmptyDescription()
            {
                disType = new DisabilityType(code, string.Empty);
            }

            [TestMethod]
            public void DisabilityTypeConstructorValidDescription()
            {
                disType = new DisabilityType(code, description);
                Assert.AreEqual(description, disType.Description);
            }
        }
    }
}
