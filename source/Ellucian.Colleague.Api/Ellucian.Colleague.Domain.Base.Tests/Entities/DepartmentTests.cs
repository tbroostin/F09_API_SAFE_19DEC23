// Copyright 2014-2016 Ellucian Company L.P. and its affiliates.
using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ellucian.Colleague.Domain.Base.Entities;

namespace Ellucian.Colleague.Domain.Base.Tests.Entities
{
    [TestClass]
    public class DepartmentTests
    {
        [TestClass]
        public class DepartmentConstructor
        {
            private string guid;
            private string code;
            private string desc;
            private Department dept;
            private bool isActive;
            private string institutionId;

            [TestInitialize]
            public void Initialize()
            {
                guid = Guid.NewGuid().ToString();
                code = "ENGL";
                desc = "English Department";
                isActive = true;
                institutionId = "00001";
                dept = new Department(guid, code, desc, isActive)
                { InstitutionId = institutionId };
            }

            [TestMethod]
            public void DepartmentGuid()
            {
                Assert.AreEqual(guid, dept.Guid);
            }

            [TestMethod]
            public void DepartmentCode()
            {
                Assert.AreEqual(code, dept.Code);
            }

            [TestMethod]
            public void DepartmentDescription()
            {
                Assert.AreEqual(desc, dept.Description);
            }

            [TestMethod]
            public void DepartmentInstitutionId()
            {
                Assert.AreEqual(institutionId, dept.InstitutionId);
            }

            [TestMethod]
            public void DepartmentIsActive()
            {
                Assert.AreEqual(isActive, dept.IsActive);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void DepartmentGuidNullException()
            {
                new Department(null, code, desc, isActive);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void DepartmentCodeNullException()
            {
                new Department(guid, null, desc, isActive);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void DepartmentDescNullException()
            {
                new Department(guid, code, null, isActive);
            }
        }

        [TestClass]
        public class DepartmentEquals
        {
            private string guid;
            private string code;
            private string desc;
            private bool isActive;
            private Department dept1;
            private Department dept2;
            private Department dept3;

            [TestInitialize]
            public void Initialize()
            {
                guid = Guid.NewGuid().ToString();
                code = "ENGL";
                desc = "English Department";
                isActive = true;
                dept1 = new Department(guid, code, desc, isActive);
                dept2 = new Department(guid, code, "English", isActive);
                dept3 = new Department(guid, "HIST", desc, isActive);
            }

            [TestMethod]
            public void DepartmentSameCodesEqual()
            {
                Assert.IsTrue(dept1.Equals(dept2));
            }

            [TestMethod]
            public void DepartmentDifferentCodeNotEqual()
            {
                Assert.IsFalse(dept1.Equals(dept3));
            }
        }

        [TestClass]
        public class DepartmentGetHashCode
        {
            private string guid;
            private string code;
            private string desc;
            private bool isActive;
            private Department dept1;
            private Department dept2;
            private Department dept3;

            [TestInitialize]
            public void Initialize()
            {
                guid = Guid.NewGuid().ToString();
                code = "ENGL";
                desc = "English Department";
                isActive = true;
                dept1 = new Department(guid, code, desc, isActive);
                dept2 = new Department(guid, code, "English", isActive);
                dept3 = new Department(guid, "HIST", desc, isActive);
            }

            [TestMethod]
            public void DepartmentSameCodeHashEqual()
            {
                Assert.AreEqual(dept1.GetHashCode(), dept2.GetHashCode());
            }

            [TestMethod]
            public void DepartmentDifferentCodeHashNotEqual()
            {
                Assert.AreNotEqual(dept1.GetHashCode(), dept3.GetHashCode());
            }
        }

        [TestClass]
        public class DepartmentAddLocation
        {
            private string guid;
            private string code;
            private string desc;
            private Department dept;
            private bool isActive;

            [TestInitialize]
            public void Initialize()
            {
                guid = Guid.NewGuid().ToString();
                code = "ENGL";
                desc = "English Department";
                isActive = true;
                dept = new Department(guid, code, desc, isActive);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void DepartmentAddLocationNullId()
            {
                dept.AddLocation(null);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void DepartmentAddLocationEmptyId()
            {
                dept.AddLocation(string.Empty);
            }

            [TestMethod]
            public void DepartmentAddLocationValidId()
            {
                dept.AddLocation("MC");
                Assert.AreEqual("MC", dept.LocationIds.ToList()[0]);
            }
        }
    }
}