/*Copyright 2019 Ellucian Company L.P. and its affiliates.*/
using Ellucian.Colleague.Domain.HumanResources.Entities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Ellucian.Colleague.Domain.HumanResources.Tests.Entities
{
    [TestClass]
    public class EmployeeBenefitsEnrollmentPackageTests
    {
        private string employeeId;
        private string packageId;
        private EmployeeBenefitsEnrollmentPackage package;

        [TestInitialize]
        public void Initialize()
        {
            employeeId = "0123456";
            packageId = "19FALLFT";
            package = new EmployeeBenefitsEnrollmentPackage(employeeId, packageId);
        }

        [TestMethod]
        public void ObjectCreatedTest()
        {
            Assert.IsNotNull(package);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void NullEmployeeId_ExceptionThrownTest()
        {
            new EmployeeBenefitsEnrollmentPackage(null, packageId);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void NullPackageId_ExceptionThrownTest()
        {
            new EmployeeBenefitsEnrollmentPackage(employeeId, null);
        }

        [TestMethod]
        public void EmployeeId_IsSetGetTest()
        {
            Assert.AreEqual(employeeId, package.EmployeeId);
        }

        [TestMethod]
        public void PackageId_IsSetGetTest()
        {
            Assert.AreEqual(packageId, package.PackageId);
        }

        [TestMethod]
        public void PackageDescription_GetSetTest()
        {
            package.PackageDescription = "Description";
            Assert.AreEqual("Description", package.PackageDescription);
        }

        [TestMethod]
        public void BenefitEnrollmentPeriodId_GetSetTest()
        {
            package.BenefitsEnrollmentPeriodId = "FALL19";
            Assert.AreEqual("FALL19", package.BenefitsEnrollmentPeriodId);
        }

        [TestMethod]
        public void EmployeeEligibleBenefitTypes_InitializedToEmptyListTest()
        {
            Assert.IsFalse(package.EmployeeEligibleBenefitTypes.Any());
        }

        [TestMethod]
        public void EmployeeEligibleBenefitTypes_GetSetTest()
        {
            var benefitTypes = new List<EmployeeBenefitType>()
            {
                new EmployeeBenefitType("MED", "MEDICAL")
            };
            package.EmployeeEligibleBenefitTypes = benefitTypes;
            Assert.AreEqual(benefitTypes.Count, package.EmployeeEligibleBenefitTypes.Count());
            CollectionAssert.AreEqual(benefitTypes, package.EmployeeEligibleBenefitTypes.ToList());
        }
    }
}
