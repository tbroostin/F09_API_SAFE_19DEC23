using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ellucian.Colleague.Domain.Student.Entities;

namespace Ellucian.Colleague.Domain.Student.Tests.Entities
{
    [TestClass]
    public class GradeSchemeTests
    {
        [TestClass]
        public class GradeSchemeConstructor
        {
            private string guid;
            private string code;
            private string desc;
            private GradeScheme gradeScheme;

            [TestInitialize]
            public void Initialize()
            {
                guid = Guid.NewGuid().ToString();
                code = "UG";
                desc = "Undergraduate";
                gradeScheme = new GradeScheme(guid, code, desc);
            }

            [TestMethod]
            public void GradeSchemeGuid()
            {
                Assert.AreEqual(guid, gradeScheme.Guid);
            }

            [TestMethod]
            public void GradeSchemeCode()
            {
                Assert.AreEqual(code, gradeScheme.Code);
            }

            [TestMethod]
            public void GradeSchemeDescription()
            {
                Assert.AreEqual(desc, gradeScheme.Description);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void GradeSchemeGuidNullException()
            {
                new AcademicLevel(null, code, desc);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void GradeSchemeCodeNullException()
            {
                new AcademicLevel(guid, null, desc);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void GradeSchemeDescNullException()
            {
                new AcademicLevel(guid, code, null);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void GradeSchemeGuidEmptyException()
            {
                new AcademicLevel(string.Empty, code, desc);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void GradeSchemeCodeEmptyException()
            {
                new AcademicLevel(guid, string.Empty, desc);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void GradeSchemeDescEmptyException()
            {
                new AcademicLevel(guid, code, string.Empty);
            }

        }

        [TestClass]
        public class GradeSchemeEquals
        {
            private string guid;
            private string code;
            private string desc;
            private GradeScheme gradeScheme1;
            private GradeScheme gradeScheme2;
            private GradeScheme gradeScheme3;

            [TestInitialize]
            public void Initialize()
            {
                guid = Guid.NewGuid().ToString();
                code = "UG";
                desc = "Undergraduate";
                gradeScheme1 = new GradeScheme(guid, code, desc);
                gradeScheme2 = new GradeScheme(guid, code, "Graduate");
                gradeScheme3 = new GradeScheme(Guid.NewGuid().ToString(), "GR", desc);
            }

            [TestMethod]
            public void GradeSchemeSameCodesEqual()
            {
                Assert.IsTrue(gradeScheme1.Equals(gradeScheme2));
            }

            [TestMethod]
            public void GradeSchemeDifferentCodeNotEqual()
            {
                Assert.IsFalse(gradeScheme1.Equals(gradeScheme3));
            }
        }

        [TestClass]
        public class GradeSchemeGetHashCode
        {
            private string guid;
            private string code;
            private string desc;
            private GradeScheme gradeScheme1;
            private GradeScheme gradeScheme2;
            private GradeScheme gradeScheme3;

            [TestInitialize]
            public void Initialize()
            {
                guid = Guid.NewGuid().ToString();
                code = "UG";
                desc = "Undergraduate";
                gradeScheme1 = new GradeScheme(guid, code, desc);
                gradeScheme2 = new GradeScheme(guid, code, "Graduate");
                gradeScheme3 = new GradeScheme(Guid.NewGuid().ToString(), "GR", desc);
            }

            [TestMethod]
            public void GradeSchemeSameCodeHashEqual()
            {
                Assert.AreEqual(gradeScheme1.GetHashCode(), gradeScheme2.GetHashCode());
            }

            [TestMethod]
            public void GradeSchemeDifferentCodeHashNotEqual()
            {
                Assert.AreNotEqual(gradeScheme1.GetHashCode(), gradeScheme3.GetHashCode());
            }
        }
    }
}