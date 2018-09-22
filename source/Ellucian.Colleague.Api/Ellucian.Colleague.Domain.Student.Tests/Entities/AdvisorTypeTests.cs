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
    public class AdvisorTypeTests
    {
        [TestClass]
        public class AdvisorTypeConstructor
        {
            private string guid;
            private string code;
            private string desc;
            private AdvisorType advisorTypes;

            [TestInitialize]
            public void Initialize()
            {
                guid = Guid.NewGuid().ToString();
                code = "AD";
                desc = "Admissions";
                advisorTypes = new AdvisorType(guid, code, desc, "1");
            }

            [TestMethod]
            public void AdvisorTypes_Code()
            {
                Assert.AreEqual(code, advisorTypes.Code);
            }

            [TestMethod]
            public void AdvisorTypes_Description()
            {
                Assert.AreEqual(desc, advisorTypes.Description);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void AdvisorTypes_GuidNullException()
            {
                new AdvisorType(null, code, desc, "1");
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void AdvisorTypes_CodeNullException()
            {
                new AdvisorType(guid, null, desc, "1");
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void AdvisorTypes_DescNullException()
            {
                new AdvisorType(guid, code, null, "1");
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void AdvisorTypesGuidEmptyException()
            {
                new AdvisorType(string.Empty, code, desc, "1");
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void AdvisorTypesCodeEmptyException()
            {
                new AdvisorType(guid, string.Empty, desc, "1");
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void AdvisorTypesDescEmptyException()
            {
                new AdvisorType(guid, code, string.Empty, "1");
            }

        }

        [TestClass]
        public class AdvisorTypes_Equals
        {
            private string guid;
            private string code;
            private string desc;
            private AdvisorType advisorTypes1;
            private AdvisorType advisorTypes2;
            private AdvisorType advisorTypes3;

            [TestInitialize]
            public void Initialize()
            {
                guid = Guid.NewGuid().ToString();
                code = "AD";
                desc = "Admissions";
                advisorTypes1 = new AdvisorType(guid, code, desc, "1");
                advisorTypes2 = new AdvisorType(guid, code, "Second Year", "1");
                advisorTypes3 = new AdvisorType(Guid.NewGuid().ToString(), "200", desc, "1");
            }

            [TestMethod]
            public void AdvisorTypesSameCodesEqual()
            {
                Assert.IsTrue(advisorTypes1.Equals(advisorTypes2));
            }

            [TestMethod]
            public void AdvisorTypesDifferentCodeNotEqual()
            {
                Assert.IsFalse(advisorTypes1.Equals(advisorTypes3));
            }
        }

        [TestClass]
        public class AdvisorTypes_GetHashCode
        {
            private string guid;
            private string code;
            private string desc;
            private AdvisorType advisorTypes1;
            private AdvisorType advisorTypes2;
            private AdvisorType advisorTypes3;

            [TestInitialize]
            public void Initialize()
            {
                guid = Guid.NewGuid().ToString();
                code = "AD";
                desc = "Admissions";
                advisorTypes1 = new AdvisorType(guid, code, desc, "1");
                advisorTypes2 = new AdvisorType(guid, code, "Second Year", "1");
                advisorTypes3 = new AdvisorType(Guid.NewGuid().ToString(), "200", desc, "1");
            }

            [TestMethod]
            public void AdvisorTypesSameCodeHashEqual()
            {
                Assert.AreEqual(advisorTypes1.GetHashCode(), advisorTypes2.GetHashCode());
            }

            [TestMethod]
            public void AdvisorTypesDifferentCodeHashNotEqual()
            {
                Assert.AreNotEqual(advisorTypes1.GetHashCode(), advisorTypes3.GetHashCode());
            }
        }
    }
}
