/* Copyright 2017 Ellucian Company L.P. and its affiliates. */
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
    public class PayStatementConfigurationTests
    {
        public PayStatementConfiguration configuration
        {
            get
            {
                return new PayStatementConfiguration();
            }
        }

        [TestMethod]
        public void DefaultPropertiesAreSetTest()
        {            
            Assert.AreEqual(0, configuration.OffsetDaysCount);
            Assert.IsNull(configuration.PreviousYearsCount);
            Assert.IsFalse(configuration.DisplayZeroAmountBenefitDeductions);
            Assert.AreEqual(SSNDisplay.LastFour, configuration.SocialSecurityNumberDisplay);
            Assert.AreEqual(true, configuration.DisplayWithholdingStatusFlag);
            Assert.AreEqual(string.Empty, configuration.Message);
            Assert.AreEqual(string.Empty, configuration.InstitutionName);
            var emptyAddress = new List<PayStatementAddress>();
            CollectionAssert.AreEqual(emptyAddress, configuration.InstitutionMailingLabel);
        }
    }
}
