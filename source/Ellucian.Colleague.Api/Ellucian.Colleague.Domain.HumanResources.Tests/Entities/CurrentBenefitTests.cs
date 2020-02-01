/* Copyright 2019 Ellucian Company L.P. and its affiliates. */
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
    public class CurrentBenefitTests
    {
        public string BenefitDescription { get; set; }
        public string Coverage { get; set; }
        public string EmployeeCost { get; set; }
        public List<string> Dependents { get; set; }        
        public List<string> HealthCareProviders { get; set; }        
        public List<string> Beneficiaries { get; set; }

        public CurrentBenefit CurrentBenefit { get; set; }

        public void CurrentBenefitTestsInitialize()
        {
            BenefitDescription = "Dental Employee Plus One";
            Coverage = "Employee plus One";
            EmployeeCost = "$8.65";
            Dependents = new List<string>()
            {
                "Jason Richerdson",
                "Mark Tester"
            };
            HealthCareProviders = new List<string>()
            {
                "Self - Rick Dalton #1729",
                "Jason Richerdson - Subanna and Co #98765"
            };
            Beneficiaries = new List<string>()
            {
                "Spike Stubin 100% (Beneficiary)"
            };
        }
    }

    [TestClass]
    public class CurrentBenefitGenericTests : CurrentBenefitTests
    {
        public new CurrentBenefit CurrentBenefit
        {
            get
            {
                return new CurrentBenefit(BenefitDescription, Coverage, EmployeeCost, Dependents, HealthCareProviders, Beneficiaries);
            }
        }

        [TestInitialize]
        public void Initialize()
        {
            CurrentBenefitTestsInitialize();
        }

        #region Constructor Tests
        [TestMethod]
        public void CurrentBenefit_BenefitDescriptionTest()
        {
            Assert.AreEqual(BenefitDescription, CurrentBenefit.BenefitDescription);
            Assert.AreEqual(Coverage, CurrentBenefit.Coverage);
            Assert.AreEqual(EmployeeCost, CurrentBenefit.EmployeeCost);
            Assert.IsNotNull(CurrentBenefit.Dependents);
            Assert.IsNotNull(CurrentBenefit.HealthCareProviders);
            Assert.IsNotNull(CurrentBenefit.Beneficiaries);
        }
        #endregion

        #region Attribute Tests
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CurrentBenefit_BenefitDescriptionRequiredTest()
        {
            BenefitDescription = null;
            CurrentBenefit expectedObject = CurrentBenefit;
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CurrentBenefit_BenefitDescriptionEmptyTest()
        {
            BenefitDescription = string.Empty;
            CurrentBenefit expectedObject = CurrentBenefit;
        }

        [TestMethod]
        public void CurrentBenefit_Dependents_ContainsGivenItemTest()
        {
            string dependentName = "Jason Richerdson";
            var result = CurrentBenefit.Dependents.Select(x => x).Contains(dependentName);
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void CurrentBenefit_HealthCareProviders_ContainsGivenItemTest()
        {
            string healthCareProvidersName = "Self - Rick Dalton #1729";
            var result = CurrentBenefit.HealthCareProviders.Select(x => x).Contains(healthCareProvidersName);
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void CurrentBenefit_Beneficiaries_ContainsGivenItemTest()
        {
            string BeneficiariesName = "Spike Stubin 100% (Beneficiary)";
            var result = CurrentBenefit.Beneficiaries.Select(x => x).Contains(BeneficiariesName);
            Assert.IsTrue(result);
        }
        #endregion

        #region Equals Tests
        [TestMethod]
        public void CurrentBenefit_ObjectsNotEqualWhenInputIsNullTest()
        {
            Assert.IsFalse(CurrentBenefit.Equals(null));
        }

        [TestMethod]
        public void CurrentBenefit_ObjectsNotEqualWhenInputIsDifferentTypeTest()
        {
            Assert.IsFalse(CurrentBenefit.Equals(new EmployeeStipend("Test", 1155.05m)));
        }
        #endregion
    }
}
