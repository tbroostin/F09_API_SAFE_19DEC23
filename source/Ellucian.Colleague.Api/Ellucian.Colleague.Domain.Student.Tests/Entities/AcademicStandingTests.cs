using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ellucian.Colleague.Domain.Student.Entities;

namespace Ellucian.Colleague.Domain.Student.Tests.Entities
{
    [TestClass]
    public class AcademicStandingTests
    {
        [TestClass]
        public class AcademicStandingConstructor
        {
            private string guid;
            private string code;
            private string desc;
            private AcademicStanding2 academicStanding;

            [TestInitialize]
            public void Initialize()
            {
                guid = Guid.NewGuid().ToString();
                code = "100";
                desc = "First Year";
                academicStanding = new AcademicStanding2(guid, code, desc);
            }

            [TestMethod]
            public void AcademicStanding_Code()
            {
                Assert.AreEqual(code, academicStanding.Code);
            }

            [TestMethod]
            public void AcademicStanding_Description()
            {
                Assert.AreEqual(desc, academicStanding.Description);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void AcademicStanding_GuidNullException()
            {
                new AcademicStanding2(null, code, desc);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void AcademicStanding_CodeNullException()
            {
                new AcademicStanding2(guid, null, desc);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void AcademicStanding_DescNullException()
            {
                new AcademicStanding2(guid, code, null);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void AcademicStandingGuidEmptyException()
            {
                new AcademicStanding2(string.Empty, code, desc);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void AcademicStandingCodeEmptyException()
            {
                new AcademicStanding2(guid, string.Empty, desc);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void AcademicStandingDescEmptyException()
            {
                new AcademicStanding2(guid, code, string.Empty);
            }

        }

        [TestClass]
        public class AcademicStanding_Equals
        {
            private string guid;
            private string code;
            private string desc;
            private AcademicStanding2 academicStanding1;
            private AcademicStanding2 academicStanding2;
            private AcademicStanding2 academicStanding3;

            [TestInitialize]
            public void Initialize()
            {
                guid = Guid.NewGuid().ToString();
                code = "100";
                desc = "First Year";
                academicStanding1 = new AcademicStanding2(guid, code, desc);
                academicStanding2 = new AcademicStanding2(guid, code, "Second Year");
                academicStanding3 = new AcademicStanding2(Guid.NewGuid().ToString(), "200", desc);
            }

            [TestMethod]
            public void AcademicStandingSameCodesEqual()
            {
                Assert.IsTrue(academicStanding1.Equals(academicStanding2));
            }

            [TestMethod]
            public void AcademicStandingDifferentCodeNotEqual()
            {
                Assert.IsFalse(academicStanding1.Equals(academicStanding3));
            }
        }

        [TestClass]
        public class AcademicStanding_GetHashCode
        {
            private string guid;
            private string code;
            private string desc;
            private AcademicStanding2 academicStanding1;
            private AcademicStanding2 academicStanding2;
            private AcademicStanding2 academicStanding3;

            [TestInitialize]
            public void Initialize()
            {
                guid = Guid.NewGuid().ToString();
                code = "100";
                desc = "First Year";
                academicStanding1 = new AcademicStanding2(guid, code, desc);
                academicStanding2 = new AcademicStanding2(guid, code, "Second Year");
                academicStanding3 = new AcademicStanding2(Guid.NewGuid().ToString(), "200", desc);
            }

            [TestMethod]
            public void AcademicStandingSameCodeHashEqual()
            {
                Assert.AreEqual(academicStanding1.GetHashCode(), academicStanding2.GetHashCode());
            }

            [TestMethod]
            public void AcademicStandingDifferentCodeHashNotEqual()
            {
                Assert.AreNotEqual(academicStanding1.GetHashCode(), academicStanding3.GetHashCode());
            }
        }
    }
}
