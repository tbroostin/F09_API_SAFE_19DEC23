using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ellucian.Colleague.Domain.Student.Entities;

namespace Ellucian.Colleague.Domain.Student.Tests.Entities
{
    [TestClass]
    public class SectionGradeTypeTests
    {
        [TestClass]
        public class SectionGradeTypeConstructor
        {
            private string guid;
            private string code;
            private string desc;
            private SectionGradeType sectionGradeType;

            [TestInitialize]
            public void Initialize()
            {
                guid = Guid.NewGuid().ToString();
                code = "MID1";
                desc = "Midterm Grade 1";
                sectionGradeType = new SectionGradeType(guid, code, desc);
            }

            [TestMethod]
            public void SectionGradeType_Code()
            {
                Assert.AreEqual(code, sectionGradeType.Code);
            }

            [TestMethod]
            public void SectionGradeType_Description()
            {
                Assert.AreEqual(desc, sectionGradeType.Description);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void SectionGradeType_GuidNullException()
            {
                new SectionGradeType(null, code, desc);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void SectionGradeType_CodeNullException()
            {
                new SectionGradeType(guid, null, desc);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void SectionGradeType_DescNullException()
            {
                new SectionGradeType(guid, code, null);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void SectionGradeTypeGuidEmptyException()
            {
                new SectionGradeType(string.Empty, code, desc);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void SectionGradeTypeCodeEmptyException()
            {
                new SectionGradeType(guid, string.Empty, desc);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void SectionGradeTypeDescEmptyException()
            {
                new SectionGradeType(guid, code, string.Empty);
            }

        }

        [TestClass]
        public class SectionGradeType_Equals
        {
            private string guid;
            private string code;
            private string desc;
            private SectionGradeType sectionGradeType1;
            private SectionGradeType sectionGradeType2;
            private SectionGradeType sectionGradeType3;

            [TestInitialize]
            public void Initialize()
            {
                guid = Guid.NewGuid().ToString();
                code = "MID1";
                desc = "Midterm Grade 1";
                sectionGradeType1 = new SectionGradeType(guid, code, desc);
                sectionGradeType2 = new SectionGradeType(guid, code, "Midterm Grade 1");
                sectionGradeType3 = new SectionGradeType(Guid.NewGuid().ToString(), "MID1", desc);
            }

            [TestMethod]
            public void SectionGradeTypeSameCodesEqual()
            {
                Assert.IsTrue(sectionGradeType1.Equals(sectionGradeType2));
            }

            [TestMethod]
            public void SectionGradeTypeDifferentCodeNotEqual()
            {
                Assert.IsFalse(sectionGradeType1.Equals(sectionGradeType3));
            }
        }

        [TestClass]
        public class SectionGradeType_GetHashCode
        {
            private string guid;
            private string code;
            private string desc;
            private SectionGradeType sectionGradeType1;
            private SectionGradeType sectionGradeType2;
            private SectionGradeType sectionGradeType3;

            [TestInitialize]
            public void Initialize()
            {
                guid = Guid.NewGuid().ToString();
                code = "MID1";
                desc = "Midterm Grade 1";
                sectionGradeType1 = new SectionGradeType(guid, code, desc);
                sectionGradeType2 = new SectionGradeType(guid, code, "Midterm Grade 1");
                sectionGradeType3 = new SectionGradeType(Guid.NewGuid().ToString(), "MID1", desc);
            }

            [TestMethod]
            public void SectionGradeTypeSameCodeHashEqual()
            {
                Assert.AreEqual(sectionGradeType1.GetHashCode(), sectionGradeType2.GetHashCode());
            }

            [TestMethod]
            public void SectionGradeTypeDifferentCodeHashNotEqual()
            {
                Assert.AreNotEqual(sectionGradeType1.GetHashCode(), sectionGradeType3.GetHashCode());
            }
        }
    }
}
