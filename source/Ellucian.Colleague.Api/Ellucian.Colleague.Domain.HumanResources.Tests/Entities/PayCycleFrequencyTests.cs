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
    public class PayCycleFrequencyTests
    {
        public string code;
        public string description;
        public int annualPayFrequency;

        [TestInitialize]
        public void Initialize()
        {
            code = "abc";
            description = "xyz";
            annualPayFrequency = 19;
        }

        [TestMethod]
        public void PropertiesAreSetTest()
        {
            var frequency = new PayCycleFrequency(code, description, annualPayFrequency);
            Assert.AreEqual(code, frequency.Code);
            Assert.AreEqual(description, frequency.Description);
            Assert.AreEqual(annualPayFrequency, frequency.AnnualPayFrequency);
        }
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void NoCodeTest()
        {
            new PayCycleFrequency("", description, annualPayFrequency);
        }
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void NoDescriptionTest()
        {
            new PayCycleFrequency(code, "", annualPayFrequency);
        }

    }
}
