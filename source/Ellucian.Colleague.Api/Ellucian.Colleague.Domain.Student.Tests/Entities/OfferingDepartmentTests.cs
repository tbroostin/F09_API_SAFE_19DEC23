// Copyright 2014 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ellucian.Colleague.Domain.Student.Entities;

namespace Ellucian.Colleague.Domain.Student.Tests.Entities
{
    [TestClass]
    public class OfferingDepartmentTests
    {
        private string deptCode;

        private OfferingDepartment offeringDepartment;

        [TestClass]
        public class OfferingDepartmentConstructor : OfferingDepartmentTests
        {
            [TestInitialize]
            public void Initialize()
            {
                deptCode = "BUSN";
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void OfferingDepartmentConstructorNullDepartmentCode()
            {
                offeringDepartment = new OfferingDepartment(null);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void OfferingDepartmentConstructorEmptyDepartmentCode()
            {
                offeringDepartment = new OfferingDepartment(string.Empty);
            }

            [TestMethod]
            public void OfferingDepartmentConstructorValidDepartmentCode()
            {
                offeringDepartment = new OfferingDepartment(deptCode);
                Assert.AreEqual(deptCode, offeringDepartment.AcademicDepartmentCode);
            }

            [TestMethod]
            public void OfferingDepartmentDefaultResponsibilityPercentage()
            {
                offeringDepartment = new OfferingDepartment(deptCode);
                Assert.AreEqual(100m, offeringDepartment.ResponsibilityPercentage);
            }

            [TestMethod]
            public void OfferingDepartmentValidResponsibilityPercentage()
            {
                offeringDepartment = new OfferingDepartment(deptCode, 50m);
                Assert.AreEqual(50m, offeringDepartment.ResponsibilityPercentage);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentOutOfRangeException))]
            public void OfferingDepartmentNegativeResponsibilityPercentage()
            {
                offeringDepartment = new OfferingDepartment(deptCode, -1m);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentOutOfRangeException))]
            public void OfferingDepartmentResponsibilityPercentageOverMax()
            {
                offeringDepartment = new OfferingDepartment(deptCode, 101m);
            }
        }
    }
}