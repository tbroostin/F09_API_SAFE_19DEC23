//Copyright 2018 Ellucian Company L.P. and its affiliates.

using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ellucian.Colleague.Domain.Student.Entities;

namespace Ellucian.Colleague.Domain.Student.Tests.Entities
{
    [TestClass]
    public class SectionTitleTypeTests
    {
        [TestClass]
        public class SectionTitleTypeConstructor
        {
            private string guid;
            private string code;
            private string desc;
            private SectionTitleType sectionTitleTypes;

            [TestInitialize]
            public void Initialize()
            {
                guid = Guid.NewGuid().ToString();
                code = "SHORT";
                desc = "Short title type";
                sectionTitleTypes = new SectionTitleType(guid, code, desc);
            }

            [TestMethod]
            public void SectionTitleType_Code()
            {
                Assert.AreEqual(code, sectionTitleTypes.Code);
            }

            [TestMethod]
            public void SectionTitleType_Description()
            {
                Assert.AreEqual(desc, sectionTitleTypes.Description);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void SectionTitleType_GuidNullException()
            {
                new SectionTitleType(null, code, desc);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void SectionTitleType_CodeNullException()
            {
                new SectionTitleType(guid, null, desc);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void SectionTitleType_DescNullException()
            {
                new SectionTitleType(guid, code, null);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void SectionTitleTypeGuidEmptyException()
            {
                new SectionTitleType(string.Empty, code, desc);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void SectionTitleTypeCodeEmptyException()
            {
                new SectionTitleType(guid, string.Empty, desc);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void SectionTitleTypeDescEmptyException()
            {
                new SectionTitleType(guid, code, string.Empty);
            }

        }

        [TestClass]
        public class SectionTitleType_Equals
        {
            private string guid;
            private string code;
            private string desc;
            private SectionTitleType sectionTitleTypes1;
            private SectionTitleType sectionTitleTypes2;
            private SectionTitleType sectionTitleTypes3;

            [TestInitialize]
            public void Initialize()
            {
                guid = Guid.NewGuid().ToString();
                code = "SHORT";
                desc = "Short title type";
                sectionTitleTypes1 = new SectionTitleType(guid, code, desc);
                sectionTitleTypes2 = new SectionTitleType(guid, code, "This is a short type.");
                sectionTitleTypes3 = new SectionTitleType(Guid.NewGuid().ToString(), "SHORT", desc);
            }

            [TestMethod]
            public void SectionTitleTypeSameCodesEqual()
            {
                Assert.IsTrue(sectionTitleTypes1.Equals(sectionTitleTypes2));
            }

            [TestMethod]
            public void SectionTitleTypeDifferentCodeNotEqual()
            {
                Assert.IsFalse(sectionTitleTypes1.Equals(sectionTitleTypes3));
            }
        }

        [TestClass]
        public class SectionTitleType_GetHashCode
        {
            private string guid;
            private string code;
            private string desc;
            private SectionTitleType sectionTitleTypes1;
            private SectionTitleType sectionTitleTypes2;
            private SectionTitleType sectionTitleTypes3;

            [TestInitialize]
            public void Initialize()
            {
                guid = Guid.NewGuid().ToString();
                code = "SHORT";
                desc = "Short title Type";
                sectionTitleTypes1 = new SectionTitleType(guid, code, desc);
                sectionTitleTypes2 = new SectionTitleType(guid, code, "This is a short section title type");
                sectionTitleTypes3 = new SectionTitleType(Guid.NewGuid().ToString(), "SHORT", desc);
            }

            [TestMethod]
            public void SectionTitleTypeSameCodeHashEqual()
            {
                Assert.AreEqual(sectionTitleTypes1.GetHashCode(), sectionTitleTypes2.GetHashCode());
            }

            [TestMethod]
            public void SectionTitleTypeDifferentCodeHashNotEqual()
            {
                Assert.AreNotEqual(sectionTitleTypes1.GetHashCode(), sectionTitleTypes3.GetHashCode());
            }
        }
    }
}
