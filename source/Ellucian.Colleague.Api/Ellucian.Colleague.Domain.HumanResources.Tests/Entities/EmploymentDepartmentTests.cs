//Copyright 2017 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ellucian.Colleague.Domain.HumanResources.Entities;

namespace Ellucian.Colleague.Domain.HumanResources.Tests.Entities
{
     [TestClass]
    public class EmploymentDepartmentTests
    {
        [TestClass]
        public class EmploymentDepartmentConstructor
        {
            private string guid;
            private string code;
            private string desc;
            private EmploymentDepartment employmentDepartments;

            [TestInitialize]
            public void Initialize()
            {
                guid = Guid.NewGuid().ToString();
                code = "AD";
                desc = "Admissions";
                employmentDepartments = new EmploymentDepartment(guid, code, desc);
            }

            [TestMethod]
            public void EmploymentDepartment_Code()
            {
                Assert.AreEqual(code, employmentDepartments.Code);
            }

            [TestMethod]
            public void EmploymentDepartment_Description()
            {
                Assert.AreEqual(desc, employmentDepartments.Description);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void EmploymentDepartment_GuidNullException()
            {
                new EmploymentDepartment(null, code, desc);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void EmploymentDepartment_CodeNullException()
            {
                new EmploymentDepartment(guid, null, desc);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void EmploymentDepartment_DescNullException()
            {
                new EmploymentDepartment(guid, code, null);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void EmploymentDepartmentGuidEmptyException()
            {
                new EmploymentDepartment(string.Empty, code, desc);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void EmploymentDepartmentCodeEmptyException()
            {
                new EmploymentDepartment(guid, string.Empty, desc);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void EmploymentDepartmentDescEmptyException()
            {
                new EmploymentDepartment(guid, code, string.Empty);
            }

        }

        [TestClass]
        public class EmploymentDepartment_Equals
        {
            private string guid;
            private string code;
            private string desc;
            private EmploymentDepartment employmentDepartments1;
            private EmploymentDepartment employmentDepartments2;
            private EmploymentDepartment employmentDepartments3;

            [TestInitialize]
            public void Initialize()
            {
                guid = Guid.NewGuid().ToString();
                code = "AD";
                desc = "Admissions";
                employmentDepartments1 = new EmploymentDepartment(guid, code, desc);
                employmentDepartments2 = new EmploymentDepartment(guid, code, "Second Year");
                employmentDepartments3 = new EmploymentDepartment(Guid.NewGuid().ToString(), "200", desc);
            }

            [TestMethod]
            public void EmploymentDepartmentSameCodesEqual()
            {
                Assert.IsTrue(employmentDepartments1.Equals(employmentDepartments2));
            }

            [TestMethod]
            public void EmploymentDepartmentDifferentCodeNotEqual()
            {
                Assert.IsFalse(employmentDepartments1.Equals(employmentDepartments3));
            }
        }

        [TestClass]
        public class EmploymentDepartment_GetHashCode
        {
            private string guid;
            private string code;
            private string desc;
            private EmploymentDepartment employmentDepartments1;
            private EmploymentDepartment employmentDepartments2;
            private EmploymentDepartment employmentDepartments3;

            [TestInitialize]
            public void Initialize()
            {
                guid = Guid.NewGuid().ToString();
                code = "AD";
                desc = "Admissions";
                employmentDepartments1 = new EmploymentDepartment(guid, code, desc);
                employmentDepartments2 = new EmploymentDepartment(guid, code, "Second Year");
                employmentDepartments3 = new EmploymentDepartment(Guid.NewGuid().ToString(), "200", desc);
            }

            [TestMethod]
            public void EmploymentDepartmentSameCodeHashEqual()
            {
                Assert.AreEqual(employmentDepartments1.GetHashCode(), employmentDepartments2.GetHashCode());
            }

            [TestMethod]
            public void EmploymentDepartmentDifferentCodeHashNotEqual()
            {
                Assert.AreNotEqual(employmentDepartments1.GetHashCode(), employmentDepartments3.GetHashCode());
            }
        }
    }
}
