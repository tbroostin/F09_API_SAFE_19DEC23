//Copyright 2018 Ellucian Company L.P. and its affiliates.

using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ellucian.Colleague.Domain.Student.Entities;

namespace Ellucian.Colleague.Domain.Student.Tests.Entities
{
    [TestClass]
    public class CourseTitleTypeTests
    {
        [TestClass]
        public class CourseTitleTypeConstructor
        {
            private string guid;
            private string code;
            private string desc;
            private CourseTitleType courseTitleTypes;

            [TestInitialize]
            public void Initialize()
            {
                guid = Guid.NewGuid().ToString();
                code = "SHORT";
                desc = "Course short title";
                courseTitleTypes = new CourseTitleType(guid, code, desc);
            }

            [TestMethod]
            public void CourseTitleType_Code()
            {
                Assert.AreEqual(code, courseTitleTypes.Code);
            }

            [TestMethod]
            public void CourseTitleType_Description()
            {
                Assert.AreEqual(desc, courseTitleTypes.Description);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void CourseTitleType_GuidNullException()
            {
                new CourseTitleType(null, code, desc);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void CourseTitleType_CodeNullException()
            {
                new CourseTitleType(guid, null, desc);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void CourseTitleType_DescNullException()
            {
                new CourseTitleType(guid, code, null);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void CourseTitleTypeGuidEmptyException()
            {
                new CourseTitleType(string.Empty, code, desc);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void CourseTitleTypeCodeEmptyException()
            {
                new CourseTitleType(guid, string.Empty, desc);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void CourseTitleTypeDescEmptyException()
            {
                new CourseTitleType(guid, code, string.Empty);
            }

        }

        [TestClass]
        public class CourseTitleType_Equals
        {
            private string guid;
            private string code;
            private string desc;
            private CourseTitleType courseTitleTypes1;
            private CourseTitleType courseTitleTypes2;
            private CourseTitleType courseTitleTypes3;

            [TestInitialize]
            public void Initialize()
            {
                guid = Guid.NewGuid().ToString();
                code = "SHORT";
                desc = "Course short title";
                courseTitleTypes1 = new CourseTitleType(guid, code, desc);
                courseTitleTypes2 = new CourseTitleType(guid, code, "Second Year");
                courseTitleTypes3 = new CourseTitleType(Guid.NewGuid().ToString(), "200", desc);
            }

            [TestMethod]
            public void CourseTitleTypeSameCodesEqual()
            {
                Assert.IsTrue(courseTitleTypes1.Equals(courseTitleTypes2));
            }

            [TestMethod]
            public void CourseTitleTypeDifferentCodeNotEqual()
            {
                Assert.IsFalse(courseTitleTypes1.Equals(courseTitleTypes3));
            }
        }

        [TestClass]
        public class CourseTitleType_GetHashCode
        {
            private string guid;
            private string code;
            private string desc;
            private CourseTitleType courseTitleTypes1;
            private CourseTitleType courseTitleTypes2;
            private CourseTitleType courseTitleTypes3;

            [TestInitialize]
            public void Initialize()
            {
                guid = Guid.NewGuid().ToString();
                code = "AD";
                desc = "Admissions";
                courseTitleTypes1 = new CourseTitleType(guid, code, desc);
                courseTitleTypes2 = new CourseTitleType(guid, code, "Second Year");
                courseTitleTypes3 = new CourseTitleType(Guid.NewGuid().ToString(), "200", desc);
            }

            [TestMethod]
            public void CourseTitleTypeSameCodeHashEqual()
            {
                Assert.AreEqual(courseTitleTypes1.GetHashCode(), courseTitleTypes2.GetHashCode());
            }

            [TestMethod]
            public void CourseTitleTypeDifferentCodeHashNotEqual()
            {
                Assert.AreNotEqual(courseTitleTypes1.GetHashCode(), courseTitleTypes3.GetHashCode());
            }
        }
    }
}
