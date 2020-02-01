// Copyright 2014-2019 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Domain.Student.Entities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace Ellucian.Colleague.Domain.Student.Tests.Entities
{
    [TestClass]
    public class GradeSchemeTests
    {
        [TestClass]
        public class GradeScheme_Guid_Constructor
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
                new GradeScheme(null, code, desc);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void GradeSchemeCodeNullException()
            {
                new GradeScheme(guid, null, desc);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void GradeSchemeDescNullException()
            {
                new GradeScheme(guid, code, null);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void GradeSchemeGuidEmptyException()
            {
                new GradeScheme(string.Empty, code, desc);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void GradeSchemeCodeEmptyException()
            {
                new GradeScheme(guid, string.Empty, desc);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void GradeSchemeDescEmptyException()
            {
                new GradeScheme(guid, code, string.Empty);
            }
        }

        [TestClass]
        public class GradeScheme_Base_Constructor
        {
            private string code;
            private string desc;
            private GradeScheme gradeScheme;

            [TestInitialize]
            public void Initialize()
            {
                code = "UG";
                desc = "Undergraduate";
                gradeScheme = new GradeScheme(code, desc);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void GradeScheme_Code_Null_Exception()
            {
                new GradeScheme(null, desc);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void GradeScheme_Description_Null_Exception()
            {
                new GradeScheme(code, null);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void GradeScheme_Code_Empty_Exception()
            {
                new GradeScheme(string.Empty, desc);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void GradeScheme_Description_Empty_Exception()
            {
                new GradeScheme(code, string.Empty);
            }

            [TestMethod]
            public void GradeScheme_Code()
            {
                Assert.AreEqual(code, gradeScheme.Code);
            }

            [TestMethod]
            public void GradeScheme_Description()
            {
                Assert.AreEqual(desc, gradeScheme.Description);
            }

            [TestMethod]
            public void GradeScheme_GradeCodes()
            {
                Assert.AreEqual(0, gradeScheme.GradeCodes.Count);
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

        [TestClass]
        public class GradeScheme_AddGradeCode
        {
            private string code;
            private string desc;
            private GradeScheme gradeScheme;

            [TestInitialize]
            public void Initialize()
            {
                code = "UG";
                desc = "Undergraduate";
                gradeScheme = new GradeScheme(code, desc);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void GradeScheme_AddGradeCode_Null()
            {
                gradeScheme.AddGradeCode(null);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void GradeScheme_AddGradeCode_Empty()
            {
                gradeScheme.AddGradeCode(string.Empty);
            }

            [TestMethod]
            public void GradeScheme_AddGradeCode_Valid()
            {
                gradeScheme.AddGradeCode("A");
                Assert.AreEqual(1, gradeScheme.GradeCodes.Count);
                Assert.AreEqual("A", gradeScheme.GradeCodes[0]);
            }

            [TestMethod]
            public void GradeScheme_AddGradeCode_Valid_Attempt_Duplicate()
            {
                gradeScheme.AddGradeCode("A");
                gradeScheme.AddGradeCode("A");
                Assert.AreEqual(1, gradeScheme.GradeCodes.Count);
                Assert.AreEqual("A", gradeScheme.GradeCodes[0]);
            }

        }
    }
}