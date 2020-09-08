/*Copyright 2020 Ellucian Company L.P. and its affiliates.*/
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
    public class EmployeeBenefitsEnrollmentEligibilityTests
    {
        private string employeeId;
        private string eligibilityPeriod;
        private DateTime? startDate;
        private DateTime? endDate;
        private string description;
        private string ineligibleReason;
        private string eligibilityPackage;
        private bool isEnrollmentInitiated;
        private bool isPackageSubmitted;
        private List<string> benefitsPageCustomText;
        private List<string> benefitsEnrollmentPageCustomText;
        private List<string> manageDepBenPageCustomText;
        private EmployeeBenefitsEnrollmentEligibility eligibility;

        [TestInitialize]
        public void Initialize()
        {
            employeeId = "1234567";
            eligibilityPeriod = "fall";
            startDate = new DateTime(2020, 01, 01);
            endDate = new DateTime(2020, 08, 01);
            description = "2020 fall";
            ineligibleReason = "no package";
            eligibilityPeriod = "2020 fall";
            eligibility = new EmployeeBenefitsEnrollmentEligibility(employeeId, eligibilityPeriod, ineligibleReason);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void NullEmployeeId_ExceptionThrownTest()
        {
            new EmployeeBenefitsEnrollmentEligibility(null, eligibilityPeriod, ineligibleReason);
        }

        [TestMethod]
        
        public void ObjectCreatedTest()
        {
            Assert.IsNotNull(eligibility);
        }

        [TestMethod]

        public void EmployeeId_GetSetTest()
        {
            Assert.AreEqual(employeeId, eligibility.EmployeeId);
        }

        [TestMethod]

        public void EligibilityPeriod_GetSetTest()
        {
            Assert.AreEqual(eligibilityPeriod, eligibility.EligibilityPeriod);
        }

        [TestMethod]

        public void IneligibleReason_GetSetTest()
        {
            Assert.AreEqual(ineligibleReason, eligibility.IneligibleReason);
        }

        [TestMethod]

        public void StartDate_GetSetTest()
        {
            eligibility.StartDate = startDate;
            Assert.AreEqual(startDate, eligibility.StartDate);
        }

        [TestMethod]

        public void EndDate_GetSetTest()
        {
            eligibility.EndDate = endDate;
            Assert.AreEqual(endDate, eligibility.EndDate);
        }

        [TestMethod]

        public void Description_GetSetTest()
        {
            eligibility.Description = description;
            Assert.AreEqual(description, eligibility.Description);
        }

        [TestMethod]

        public void EligibilityPackage_GetSetTest()
        {
            eligibility.EligibilityPackage = eligibilityPackage;
            Assert.AreEqual(eligibilityPackage, eligibility.EligibilityPackage);
        }

        [TestMethod]

        public void IsEnrollmentInitiated_GetSetTest()
        {
            Assert.IsFalse(eligibility.IsEnrollmentInitiated);
            eligibility.IsEnrollmentInitiated = true;
            Assert.IsTrue(eligibility.IsEnrollmentInitiated);
        }

        [TestMethod]

        public void IsPackageSubmitted_GetSetTest()
        {
            Assert.IsFalse(eligibility.IsPackageSubmitted);
            eligibility.IsPackageSubmitted = true;
            Assert.IsTrue(eligibility.IsPackageSubmitted);
        }

        [TestMethod]

        public void BenefitsPageCustomText_GetSetTest()
        {
            eligibility.BenefitsPageCustomText = benefitsPageCustomText;
            Assert.AreEqual(eligibilityPackage, eligibility.BenefitsPageCustomText);
        }

        [TestMethod]

        public void BenefitsEnrollmentPageCustomText_GetSetTest()
        {
            eligibility.BenefitsPageCustomText = benefitsEnrollmentPageCustomText;
            Assert.AreEqual(eligibilityPackage, eligibility.BenefitsEnrollmentPageCustomText);
        }

        [TestMethod]

        public void ManageDepBenPageCustomText_GetSetTest()
        {
            eligibility.BenefitsPageCustomText = manageDepBenPageCustomText;
            Assert.AreEqual(eligibilityPackage, eligibility.ManageDepBenPageCustomText);
        }
    }
}
