//Copyright 2017 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ellucian.Colleague.Domain.Student.Entities;

namespace Ellucian.Colleague.Domain.Student.Tests.Entities
{
     [TestClass]
    public class HousingResidentTypeTests
    {
        [TestClass]
        public class HousingResidentTypeConstructor
        {
            private string guid;
            private string code;
            private string desc;
            private HousingResidentType housingResidentTypes;

            [TestInitialize]
            public void Initialize()
            {
                guid = Guid.NewGuid().ToString();
                code = "AD";
                desc = "Admissions";
                housingResidentTypes = new HousingResidentType(guid, code, desc);
            }

            [TestMethod]
            public void HousingResidentType_Code()
            {
                Assert.AreEqual(code, housingResidentTypes.Code);
            }

            [TestMethod]
            public void HousingResidentType_Description()
            {
                Assert.AreEqual(desc, housingResidentTypes.Description);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void HousingResidentType_GuidNullException()
            {
                new HousingResidentType(null, code, desc);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void HousingResidentType_CodeNullException()
            {
                new HousingResidentType(guid, null, desc);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void HousingResidentType_DescNullException()
            {
                new HousingResidentType(guid, code, null);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void HousingResidentTypeGuidEmptyException()
            {
                new HousingResidentType(string.Empty, code, desc);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void HousingResidentTypeCodeEmptyException()
            {
                new HousingResidentType(guid, string.Empty, desc);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void HousingResidentTypeDescEmptyException()
            {
                new HousingResidentType(guid, code, string.Empty);
            }

        }

        [TestClass]
        public class HousingResidentType_Equals
        {
            private string guid;
            private string code;
            private string desc;
            private HousingResidentType housingResidentTypes1;
            private HousingResidentType housingResidentTypes2;
            private HousingResidentType housingResidentTypes3;

            [TestInitialize]
            public void Initialize()
            {
                guid = Guid.NewGuid().ToString();
                code = "AD";
                desc = "Admissions";
                housingResidentTypes1 = new HousingResidentType(guid, code, desc);
                housingResidentTypes2 = new HousingResidentType(guid, code, "Second Year");
                housingResidentTypes3 = new HousingResidentType(Guid.NewGuid().ToString(), "200", desc);
            }

            [TestMethod]
            public void HousingResidentTypeSameCodesEqual()
            {
                Assert.IsTrue(housingResidentTypes1.Equals(housingResidentTypes2));
            }

            [TestMethod]
            public void HousingResidentTypeDifferentCodeNotEqual()
            {
                Assert.IsFalse(housingResidentTypes1.Equals(housingResidentTypes3));
            }
        }

        [TestClass]
        public class HousingResidentType_GetHashCode
        {
            private string guid;
            private string code;
            private string desc;
            private HousingResidentType housingResidentTypes1;
            private HousingResidentType housingResidentTypes2;
            private HousingResidentType housingResidentTypes3;

            [TestInitialize]
            public void Initialize()
            {
                guid = Guid.NewGuid().ToString();
                code = "AD";
                desc = "Admissions";
                housingResidentTypes1 = new HousingResidentType(guid, code, desc);
                housingResidentTypes2 = new HousingResidentType(guid, code, "Second Year");
                housingResidentTypes3 = new HousingResidentType(Guid.NewGuid().ToString(), "200", desc);
            }

            [TestMethod]
            public void HousingResidentTypeSameCodeHashEqual()
            {
                Assert.AreEqual(housingResidentTypes1.GetHashCode(), housingResidentTypes2.GetHashCode());
            }

            [TestMethod]
            public void HousingResidentTypeDifferentCodeHashNotEqual()
            {
                Assert.AreNotEqual(housingResidentTypes1.GetHashCode(), housingResidentTypes3.GetHashCode());
            }
        }
    }
}
