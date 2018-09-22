//Copyright 2018 Ellucian Company L.P. and its affiliates.

using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ellucian.Colleague.Domain.Student.Entities;

namespace Ellucian.Colleague.Domain.Student.Tests.Entities
{
    [TestClass]
    public class SectionDescriptionTypeTests
    {
        [TestClass]
        public class SectionDescriptionTypeConstructor
        {
            private string guid;
            private string code;
            private string desc;
            private SectionDescriptionType sectionDescriptionTypes;

            [TestInitialize]
            public void Initialize()
            {
                guid = Guid.NewGuid().ToString();
                code = "SHORT";
                desc = "Short title type";
                sectionDescriptionTypes = new SectionDescriptionType(guid, code, desc);
            }

            [TestMethod]
            public void SectionDescriptionType_Code()
            {
                Assert.AreEqual(code, sectionDescriptionTypes.Code);
            }

            [TestMethod]
            public void SectionDescriptionType_Description()
            {
                Assert.AreEqual(desc, sectionDescriptionTypes.Description);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void SectionDescriptionType_GuidNullException()
            {
                new SectionDescriptionType(null, code, desc);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void SectionDescriptionType_CodeNullException()
            {
                new SectionDescriptionType(guid, null, desc);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void SectionDescriptionType_DescNullException()
            {
                new SectionDescriptionType(guid, code, null);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void SectionDescriptionTypeGuidEmptyException()
            {
                new SectionDescriptionType(string.Empty, code, desc);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void SectionDescriptionTypeCodeEmptyException()
            {
                new SectionDescriptionType(guid, string.Empty, desc);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void SectionDescriptionTypeDescEmptyException()
            {
                new SectionDescriptionType(guid, code, string.Empty);
            }

        }

        [TestClass]
        public class SectionDescriptionType_Equals
        {
            private string guid;
            private string code;
            private string desc;
            private SectionDescriptionType sectionDescriptionTypes1;
            private SectionDescriptionType sectionDescriptionTypes2;
            private SectionDescriptionType sectionDescriptionTypes3;

            [TestInitialize]
            public void Initialize()
            {
                guid = Guid.NewGuid().ToString();
                code = "SHORT";
                desc = "Short title type";
                sectionDescriptionTypes1 = new SectionDescriptionType(guid, code, desc);
                sectionDescriptionTypes2 = new SectionDescriptionType(guid, code, "This is a short type.");
                sectionDescriptionTypes3 = new SectionDescriptionType(Guid.NewGuid().ToString(), "SHORT", desc);
            }

            [TestMethod]
            public void SectionDescriptionTypeSameCodesEqual()
            {
                Assert.IsTrue(sectionDescriptionTypes1.Equals(sectionDescriptionTypes2));
            }

            [TestMethod]
            public void SectionDescriptionTypeDifferentCodeNotEqual()
            {
                Assert.IsFalse(sectionDescriptionTypes1.Equals(sectionDescriptionTypes3));
            }
        }

        [TestClass]
        public class SectionDescriptionType_GetHashCode
        {
            private string guid;
            private string code;
            private string desc;
            private SectionDescriptionType sectionDescriptionTypes1;
            private SectionDescriptionType sectionDescriptionTypes2;
            private SectionDescriptionType sectionDescriptionTypes3;

            [TestInitialize]
            public void Initialize()
            {
                guid = Guid.NewGuid().ToString();
                code = "SHORT";
                desc = "Short title Type";
                sectionDescriptionTypes1 = new SectionDescriptionType(guid, code, desc);
                sectionDescriptionTypes2 = new SectionDescriptionType(guid, code, "This is a short section title type");
                sectionDescriptionTypes3 = new SectionDescriptionType(Guid.NewGuid().ToString(), "SHORT", desc);
            }

            [TestMethod]
            public void SectionDescriptionTypeSameCodeHashEqual()
            {
                Assert.AreEqual(sectionDescriptionTypes1.GetHashCode(), sectionDescriptionTypes2.GetHashCode());
            }

            [TestMethod]
            public void SectionDescriptionTypeDifferentCodeHashNotEqual()
            {
                Assert.AreNotEqual(sectionDescriptionTypes1.GetHashCode(), sectionDescriptionTypes3.GetHashCode());
            }
        }
    }
}
