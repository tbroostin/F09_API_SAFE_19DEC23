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
    public class SectionStatusesTests
    {
        [TestClass]
        public class SectionStatusesConstructor
        {
            private string guid;
            private string code;
            private string desc;
            private SectionStatuses sectionStatuses;

            [TestInitialize]
            public void Initialize()
            {
                guid = Guid.NewGuid().ToString();
                code = "AD";
                desc = "Admissions";
                sectionStatuses = new SectionStatuses(guid, code, desc);
            }

            [TestMethod]
            public void SectionStatuses_Code()
            {
                Assert.AreEqual(code, sectionStatuses.Code);
            }

            [TestMethod]
            public void SectionStatuses_Description()
            {
                Assert.AreEqual(desc, sectionStatuses.Description);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void SectionStatuses_GuidNullException()
            {
                new SectionStatuses(null, code, desc);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void SectionStatuses_CodeNullException()
            {
                new SectionStatuses(guid, null, desc);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void SectionStatuses_DescNullException()
            {
                new SectionStatuses(guid, code, null);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void SectionStatusesGuidEmptyException()
            {
                new SectionStatuses(string.Empty, code, desc);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void SectionStatusesCodeEmptyException()
            {
                new SectionStatuses(guid, string.Empty, desc);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void SectionStatusesDescEmptyException()
            {
                new SectionStatuses(guid, code, string.Empty);
            }

        }

        [TestClass]
        public class SectionStatuses_Equals
        {
            private string guid;
            private string code;
            private string desc;
            private SectionStatuses sectionStatuses1;
            private SectionStatuses sectionStatuses2;
            private SectionStatuses sectionStatuses3;

            [TestInitialize]
            public void Initialize()
            {
                guid = Guid.NewGuid().ToString();
                code = "AD";
                desc = "Admissions";
                sectionStatuses1 = new SectionStatuses(guid, code, desc);
                sectionStatuses2 = new SectionStatuses(guid, code, "Second Year");
                sectionStatuses3 = new SectionStatuses(Guid.NewGuid().ToString(), "200", desc);
            }

            [TestMethod]
            public void SectionStatusesSameCodesEqual()
            {
                Assert.IsTrue(sectionStatuses1.Equals(sectionStatuses2));
            }

            [TestMethod]
            public void SectionStatusesDifferentCodeNotEqual()
            {
                Assert.IsFalse(sectionStatuses1.Equals(sectionStatuses3));
            }
        }

        [TestClass]
        public class SectionStatuses_GetHashCode
        {
            private string guid;
            private string code;
            private string desc;
            private SectionStatuses sectionStatuses1;
            private SectionStatuses sectionStatuses2;
            private SectionStatuses sectionStatuses3;

            [TestInitialize]
            public void Initialize()
            {
                guid = Guid.NewGuid().ToString();
                code = "AD";
                desc = "Admissions";
                sectionStatuses1 = new SectionStatuses(guid, code, desc);
                sectionStatuses2 = new SectionStatuses(guid, code, "Second Year");
                sectionStatuses3 = new SectionStatuses(Guid.NewGuid().ToString(), "200", desc);
            }

            [TestMethod]
            public void SectionStatusesSameCodeHashEqual()
            {
                Assert.AreEqual(sectionStatuses1.GetHashCode(), sectionStatuses2.GetHashCode());
            }

            [TestMethod]
            public void SectionStatusesDifferentCodeHashNotEqual()
            {
                Assert.AreNotEqual(sectionStatuses1.GetHashCode(), sectionStatuses3.GetHashCode());
            }
        }
    }
}
