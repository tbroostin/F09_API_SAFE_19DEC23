// Copyright 2014 Ellucian Company L.P. and its affiliates.
using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ellucian.Colleague.Domain.Student.Entities;

namespace Ellucian.Colleague.Domain.Student.Tests.Entities
{
    [TestClass]
    public class AcademicDepartmentTests
    {
        [TestClass]
        public class AcademicDepartmentConstructor
        {
            private string guid;
            private string code;
            private string desc;
            private AcademicDepartment acadDept;
            private bool isActive;

            [TestInitialize]
            public void Initialize()
            {
                guid = Guid.NewGuid().ToString();
                code = "ENGL";
                desc = "English Department";
                isActive = true;
                acadDept = new AcademicDepartment(guid, code, desc, isActive);
            }

            [TestMethod]
            public void AcademicDepartmentGuid()
            {
                Assert.AreEqual(guid, acadDept.Guid);
            }

            [TestMethod]
            public void AcademicDepartmentCode()
            {
                Assert.AreEqual(code, acadDept.Code);
            }

            [TestMethod]
            public void AcademicDepartmentDescription()
            {
                Assert.AreEqual(desc, acadDept.Description);
            }

            [TestMethod]
            public void AcademicDepartmentIsActive()
            {
                Assert.AreEqual(isActive, acadDept.IsActive);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void AcademicDepartmentGuidNullException()
            {
                new AcademicDepartment(null, code, desc, isActive);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void AcademicDepartmentGuidEmptyException()
            {
                new AcademicDepartment(string.Empty, code, desc, isActive);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void AcademicDepartmentCodeNullException()
            {
                new AcademicDepartment(guid, null, desc, isActive);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void AcademicDepartmentCodeEmptyException()
            {
                new AcademicDepartment(guid, string.Empty, desc, isActive);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void AcademicDepartmentDescEmptyException()
            {
                new AcademicDepartment(guid, code, string.Empty, isActive);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void AcademicDepartmentDescNullException()
            {
                new AcademicDepartment(guid, code, null, isActive);
            }
        }
    }
}