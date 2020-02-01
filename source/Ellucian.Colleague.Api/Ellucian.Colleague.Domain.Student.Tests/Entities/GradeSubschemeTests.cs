// Copyright 2019 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Domain.Student.Entities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace Ellucian.Colleague.Domain.Student.Tests.Entities
{
    [TestClass]
    public class GradeSubschemeTests
    {
        [TestClass]
        public class GradeSubscheme_Base_Constructor
        {
            private string code;
            private string desc;
            private GradeSubscheme GradeSubscheme;

            [TestInitialize]
            public void Initialize()
            {
                code = "UG";
                desc = "Undergraduate";
                GradeSubscheme = new GradeSubscheme(code, desc);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void GradeSubscheme_Code_Null_Exception()
            {
                new GradeSubscheme(null, desc);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void GradeSubscheme_Description_Null_Exception()
            {
                new GradeSubscheme(code, null);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void GradeSubscheme_Code_Empty_Exception()
            {
                new GradeSubscheme(string.Empty, desc);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void GradeSubscheme_Description_Empty_Exception()
            {
                new GradeSubscheme(code, string.Empty);
            }

            [TestMethod]
            public void GradeSubscheme_Code()
            {
                Assert.AreEqual(code, GradeSubscheme.Code);
            }

            [TestMethod]
            public void GradeSubscheme_Description()
            {
                Assert.AreEqual(desc, GradeSubscheme.Description);
            }

            [TestMethod]
            public void GradeSubscheme_GradeCodes()
            {
                Assert.AreEqual(0, GradeSubscheme.GradeCodes.Count);
            }
        }

        [TestClass]
        public class GradeSubschemeEquals
        {
            private string guid;
            private string code;
            private string desc;
            private GradeSubscheme GradeSubscheme1;
            private GradeSubscheme GradeSubscheme2;
            private GradeSubscheme GradeSubscheme3;

            [TestInitialize]
            public void Initialize()
            {
                guid = Guid.NewGuid().ToString();
                code = "UG";
                desc = "Undergraduate";
                GradeSubscheme1 = new GradeSubscheme(code, desc);
                GradeSubscheme2 = new GradeSubscheme(code, "Graduate");
                GradeSubscheme3 = new GradeSubscheme("GR", desc);
            }

            [TestMethod]
            public void GradeSubschemeSameCodesEqual()
            {
                Assert.IsTrue(GradeSubscheme1.Equals(GradeSubscheme2));
            }

            [TestMethod]
            public void GradeSubschemeDifferentCodeNotEqual()
            {
                Assert.IsFalse(GradeSubscheme1.Equals(GradeSubscheme3));
            }
        }

        [TestClass]
        public class GradeSubschemeGetHashCode
        {
            private string guid;
            private string code;
            private string desc;
            private GradeSubscheme GradeSubscheme1;
            private GradeSubscheme GradeSubscheme2;
            private GradeSubscheme GradeSubscheme3;

            [TestInitialize]
            public void Initialize()
            {
                guid = Guid.NewGuid().ToString();
                code = "UG";
                desc = "Undergraduate";
                GradeSubscheme1 = new GradeSubscheme(code, desc);
                GradeSubscheme2 = new GradeSubscheme(code, "Graduate");
                GradeSubscheme3 = new GradeSubscheme("GR", desc);
            }

            [TestMethod]
            public void GradeSubschemeSameCodeHashEqual()
            {
                Assert.AreEqual(GradeSubscheme1.GetHashCode(), GradeSubscheme2.GetHashCode());
            }

            [TestMethod]
            public void GradeSubschemeDifferentCodeHashNotEqual()
            {
                Assert.AreNotEqual(GradeSubscheme1.GetHashCode(), GradeSubscheme3.GetHashCode());
            }
        }

        [TestClass]
        public class GradeSubscheme_AddGradeCode
        {
            private string code;
            private string desc;
            private GradeSubscheme GradeSubscheme;

            [TestInitialize]
            public void Initialize()
            {
                code = "UG";
                desc = "Undergraduate";
                GradeSubscheme = new GradeSubscheme(code, desc);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void GradeSubscheme_AddGradeCode_Null()
            {
                GradeSubscheme.AddGradeCode(null);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void GradeSubscheme_AddGradeCode_Empty()
            {
                GradeSubscheme.AddGradeCode(string.Empty);
            }

            [TestMethod]
            public void GradeSubscheme_AddGradeCode_Valid()
            {
                GradeSubscheme.AddGradeCode("A");
                Assert.AreEqual(1, GradeSubscheme.GradeCodes.Count);
                Assert.AreEqual("A", GradeSubscheme.GradeCodes[0]);
            }

            [TestMethod]
            public void GradeSubscheme_AddGradeCode_Valid_Attempt_Duplicate()
            {
                GradeSubscheme.AddGradeCode("A");
                GradeSubscheme.AddGradeCode("A");
                Assert.AreEqual(1, GradeSubscheme.GradeCodes.Count);
                Assert.AreEqual("A", GradeSubscheme.GradeCodes[0]);
            }

        }
    }
}