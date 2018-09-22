using Ellucian.Colleague.Domain.HumanResources.Entities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Domain.HumanResources.Tests.Entities
{
    [TestClass]
    public class PayStatementAddressTests
    {
        public string addressLine;
        public PayStatementAddress payStatementAddress
        {
            get
            {
                return new PayStatementAddress(addressLine);
            }
        }

        [TestInitialize]
        public void Initialize()
        {
            addressLine = "2003 Edmund Halley Dr.";
        }

        [TestMethod]
        public void ConstructorSetsAddressLineTest()
        {
            Assert.AreEqual(addressLine, payStatementAddress.AddressLine);
        }

        [TestMethod]
        public void SetAndGetTest()
        {
            var line = payStatementAddress;
            line.AddressLine = "foobar";

            Assert.AreEqual("foobar", line.AddressLine);
        }

        [TestMethod]
        public void NullValuesAllowedTest()
        {
            addressLine = null;
            Assert.IsNull(payStatementAddress.AddressLine);
        }
    }
}
