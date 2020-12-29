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
    public class EmployeeBenefitsTests
    {
        public string PersonId { get; set; }
        public string AdditionalInformation { get; set; }        
        public List<CurrentBenefit> CurrentBenefits { get; set; }

        public EmployeeBenefits EmployeeBenefits;
        public void EmployeeBenefitsTestInitialize()
        {
            PersonId = "0014697";
            AdditionalInformation = "Ellucian University also provides the following non-contributory benefits to all employees: Short Term Disability, Long Term Disability, Employee Assistance Program, Tuition Reimbursement, Credit Union, Car Buying Service, Fitness Benefit up to $130/year, Fitness Facility, Entertainment Benefit up to $75/year, Free Tickets Program.";
            CurrentBenefits = new List<CurrentBenefit>()
            {
                new CurrentBenefit(
                    "Dental Employee Plus One",
                    "Employee plus One",
                    "$8.65",
                    new List<string>()
                    {
                        "Jason Richerdson",
                        "Mark Tester"
                    },
                    new List<string>()
                    {
                        "Self - Rick Dalton #1729",
                         "Jason Richerdson - Subanna and Co #98765"
                    },
                    new List<string>()
                    {
                        "Spike Stubin 100% (Beneficiary)"
                    }
                )
            };
        }
    }

    [TestClass]
    public class EmployeeBenefitsGenericTestes : EmployeeBenefitsTests
    {
        public new EmployeeBenefits EmployeeBenefits
        {
            get
            {
                return new EmployeeBenefits(PersonId, AdditionalInformation, CurrentBenefits);
            }
        }
        [TestInitialize]
        public void Initialize()
        {
            EmployeeBenefitsTestInitialize();
        }

        #region Constructor Tests
        [TestMethod]
        public void EmployeeBenefits_ConstructorTests()
        {
            Assert.AreEqual(PersonId, EmployeeBenefits.PersonId);
            Assert.AreEqual(AdditionalInformation, EmployeeBenefits.AdditionalInformation);
            Assert.IsNotNull(EmployeeBenefits.CurrentBenefits);
        }
        #endregion

        #region Attribute Tests
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void EmployeeBenefits_PersonIdIsRequiredTests()
        {
            PersonId = null;
            var expectedException = EmployeeBenefits;
        }

        [TestMethod]
        public void EmployeeBenefits_CurrentBenefit_ContainsGivenItemTest()
        {
            string benefitDescription = "Dental Employee Plus One";
            var result = EmployeeBenefits.CurrentBenefits.Select(t => t.BenefitDescription).Contains(benefitDescription);
            Assert.IsTrue(result);
        }
        #endregion

        #region Equals Tests
        [TestMethod]
        public void EmployeeBenefits_ObjectsNotEqualWhenInputIsNullTest()
        {
            Assert.IsFalse(EmployeeBenefits.Equals(null));
        }

        [TestMethod]
        public void EmployeeBenefits_ObjectsNotEqualWhenInputIsDifferentTypeTest()
        {
            Assert.IsFalse(EmployeeBenefits.Equals(new EmployeeStipend("Test", 1155.05m)));
        }
        #endregion
    }
}
