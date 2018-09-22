using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ellucian.Colleague.Domain.Student.Entities;

namespace Ellucian.Colleague.Domain.Student.Tests.Entities
{
    [TestClass]
    public class CampusInvolvementRoleTests
    {
        [TestClass]
        public class CampusInvolvementRoleConstructor
        {
            private string guid;
            private string code;
            private string desc;
            private CampusInvRole campusInvolvementRole;

            [TestInitialize]
            public void Initialize()
            {
                guid = Guid.NewGuid().ToString();
                code = "01";
                desc = "Athletics";
                campusInvolvementRole = new CampusInvRole(guid, code, desc);
            }

            [TestMethod]
            public void CampusInvolvementRole_Code()
            {
                Assert.AreEqual(code, campusInvolvementRole.Code);
            }

            [TestMethod]
            public void CampusInvolvementRole_Description()
            {
                Assert.AreEqual(desc, campusInvolvementRole.Description);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void CampusInvolvementRole_GuidNullException()
            {
                new CampusInvRole(null, code, desc);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void CampusInvolvementRole_CodeNullException()
            {
                new CampusInvRole(guid, null, desc);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void CampusInvolvementRole_DescNullException()
            {
                new CampusInvRole(guid, code, null);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void CampusInvolvementRoleGuidEmptyException()
            {
                new CampusInvRole(string.Empty, code, desc);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void CampusInvolvementRoleCodeEmptyException()
            {
                new CampusInvRole(guid, string.Empty, desc);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void CampusInvolvementRoleDescEmptyException()
            {
                new CampusInvRole(guid, code, string.Empty);
            }

        }

        [TestClass]
        public class CampusInvolvementRole_Equals
        {
            private string guid;
            private string code;
            private string desc;
            private CampusInvRole campusInvolvementRole1;
            private CampusInvRole campusInvolvementRole2;
            private CampusInvRole CampusInvolvementRole3;

            [TestInitialize]
            public void Initialize()
            {
                guid = Guid.NewGuid().ToString();
                code = "01";
                desc = "Athletics";
                campusInvolvementRole1 = new CampusInvRole(guid, code, desc);
                campusInvolvementRole2 = new CampusInvRole(guid, code, "Academics");
                CampusInvolvementRole3 = new CampusInvRole(Guid.NewGuid().ToString(), "02", desc);
            }

            [TestMethod]
            public void CampusInvolvementRoleSameCodesEqual()
            {
                Assert.IsTrue(campusInvolvementRole1.Equals(campusInvolvementRole2));
            }

            [TestMethod]
            public void CampusInvolvementRoleDifferentCodeNotEqual()
            {
                Assert.IsFalse(campusInvolvementRole1.Equals(CampusInvolvementRole3));
            }
        }

        [TestClass]
        public class CampusInvolvementRole_GetHashCode
        {
            private string guid;
            private string code;
            private string desc;
            private CampusInvRole campusInvolvementRole1;
            private CampusInvRole campusInvolvementRole2;
            private CampusInvRole campusInvolvementRole3;

            [TestInitialize]
            public void Initialize()
            {
                guid = Guid.NewGuid().ToString();
                code = "01";
                desc = "Athletics";
                campusInvolvementRole1 = new CampusInvRole(guid, code, desc);
                campusInvolvementRole2 = new CampusInvRole(guid, code, "Academics");
                campusInvolvementRole3 = new CampusInvRole(Guid.NewGuid().ToString(), "02", desc);
            }

            [TestMethod]
            public void CampusInvolvementRoleSameCodeHashEqual()
            {
                Assert.AreEqual(campusInvolvementRole1.GetHashCode(), campusInvolvementRole2.GetHashCode());
            }

            [TestMethod]
            public void CampusInvolvementRoleDifferentCodeHashNotEqual()
            {
                Assert.AreNotEqual(campusInvolvementRole1.GetHashCode(), campusInvolvementRole3.GetHashCode());
            }
        }
    }
}
