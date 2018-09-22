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
    public class DenominationTests
    {
        string guid;
        private string code;
        private string description;
        private Denomination denom;

        [TestInitialize]
        public void Initialize()
        {
            guid = Guid.NewGuid().ToString();
            code = "HIS";
            description = "Hispanic/Latino";
            denom = new Denomination(guid, code, description);
        }

        [TestClass]
        public class DenominationConstructor : DenominationTests
        {
            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void DenominationConstructorNullGuid()
            {
                denom = new Denomination(null, code, description);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void DenominationConstructorEmptyGuid()
            {
                denom = new Denomination(string.Empty, code, description);
            }

            [TestMethod]
            public void DenominationConstructorValidGuid()
            {
                denom = new Denomination(guid, code, description);
                Assert.AreEqual(guid, denom.Guid);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void DenominationConstructorNullCode()
            {
                denom = new Denomination(guid, null, description);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void DenominationConstructorEmptyCode()
            {
                denom = new Denomination(guid, string.Empty, description);
            }

            [TestMethod]
            public void DenominationConstructorValidCode()
            {
                denom = new Denomination(guid, code, description);
                Assert.AreEqual(code, denom.Code);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void DenominationConstructorNullDescription()
            {
                denom = new Denomination(guid, code, null);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void DenominationConstructorEmptyDescription()
            {
                denom = new Denomination(guid, code, string.Empty);
            }

            [TestMethod]
            public void DenominationConstructorValidDescription()
            {
                denom = new Denomination(guid, code, description);
                Assert.AreEqual(description, denom.Description);
            }
        }
    }
}
