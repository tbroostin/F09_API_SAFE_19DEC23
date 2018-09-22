using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ellucian.Colleague.Domain.Student.Entities;

namespace Ellucian.Colleague.Domain.Student.Tests.Entities
{
    [TestClass]
    public class CampusOrganizationTypeTests
    {
        [TestClass]
        public class CampusOrganizationTypeConstructor
        {
            private string guid;
            private string code;
            private string desc;
            private CampusOrganizationType campusOrganizationType;

            [TestInitialize]
            public void Initialize()
            {
                guid = Guid.NewGuid().ToString();
                code = "01";
                desc = "Athletics";
                campusOrganizationType = new CampusOrganizationType(guid, code, desc);
            }

            [TestMethod]
            public void CampusOrganizationType_Code()
            {
                Assert.AreEqual(code, campusOrganizationType.Code);
            }

            [TestMethod]
            public void CampusOrganizationType_Description()
            {
                Assert.AreEqual(desc, campusOrganizationType.Description);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void CampusOrganizationType_GuidNullException()
            {
                new CampusOrganizationType(null, code, desc);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void CampusOrganizationType_CodeNullException()
            {
                new CampusOrganizationType(guid, null, desc);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void CampusOrganizationType_DescNullException()
            {
                new CampusOrganizationType(guid, code, null);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void CampusOrganizationTypeGuidEmptyException()
            {
                new CampusOrganizationType(string.Empty, code, desc);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void CampusOrganizationTypeCodeEmptyException()
            {
                new CampusOrganizationType(guid, string.Empty, desc);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void CampusOrganizationTypeDescEmptyException()
            {
                new CampusOrganizationType(guid, code, string.Empty);
            }

        }

        [TestClass]
        public class CampusOrganizationType_Equals
        {
            private string guid;
            private string code;
            private string desc;
            private CampusOrganizationType campusOrganizationType1;
            private CampusOrganizationType campusOrganizationType2;
            private CampusOrganizationType campusOrganizationType3;

            [TestInitialize]
            public void Initialize()
            {
                guid = Guid.NewGuid().ToString();
                code = "01";
                desc = "Athletics";
                campusOrganizationType1 = new CampusOrganizationType(guid, code, desc);
                campusOrganizationType2 = new CampusOrganizationType(guid, code, "Academics");
                campusOrganizationType3 = new CampusOrganizationType(Guid.NewGuid().ToString(), "02", desc);
            }

            [TestMethod]
            public void CampusOrganizationTypeSameCodesEqual()
            {
                Assert.IsTrue(campusOrganizationType1.Equals(campusOrganizationType2));
            }

            [TestMethod]
            public void CampusOrganizationTypeDifferentCodeNotEqual()
            {
                Assert.IsFalse(campusOrganizationType1.Equals(campusOrganizationType3));
            }
        }

        [TestClass]
        public class CampusOrganizationType_GetHashCode
        {
            private string guid;
            private string code;
            private string desc;
            private CampusOrganizationType campusOrganizationType1;
            private CampusOrganizationType campusOrganizationType2;
            private CampusOrganizationType campusOrganizationType3;

            [TestInitialize]
            public void Initialize()
            {
                guid = Guid.NewGuid().ToString();
                code = "01";
                desc = "Athletics";
                campusOrganizationType1 = new CampusOrganizationType(guid, code, desc);
                campusOrganizationType2 = new CampusOrganizationType(guid, code, "Academics");
                campusOrganizationType3 = new CampusOrganizationType(Guid.NewGuid().ToString(), "02", desc);
            }

            [TestMethod]
            public void CampusOrganizationTypeSameCodeHashEqual()
            {
                Assert.AreEqual(campusOrganizationType1.GetHashCode(), campusOrganizationType2.GetHashCode());
            }

            [TestMethod]
            public void CampusOrganizationTypeDifferentCodeHashNotEqual()
            {
                Assert.AreNotEqual(campusOrganizationType1.GetHashCode(), campusOrganizationType3.GetHashCode());
            }
        }
    }
}
