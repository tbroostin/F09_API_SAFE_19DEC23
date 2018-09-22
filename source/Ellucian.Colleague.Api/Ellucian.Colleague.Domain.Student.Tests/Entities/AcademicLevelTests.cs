// Copyright 2013-2014 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ellucian.Colleague.Domain.Student.Entities;

namespace Ellucian.Colleague.Domain.Student.Tests.Entities
{
    [TestClass]
    public class AcademicLevelTests
    {
        [TestClass]
        public class AcademicLevel_Constructor
        {
            private string guid;
            private string code;
            private string desc;
            private string gradeScheme;
            private AcademicLevel acadLevel;

            [TestInitialize]
            public void Initialize()
            {
                guid = GetGuid();
                code = "UG";
                desc = "Undergraduate";
                gradeScheme = "UG";
                acadLevel = new AcademicLevel(guid, code, desc) { GradeScheme = "UG" };
            }

            [TestMethod]
            public void AcademicLevel_Guid()
            {
                Assert.AreEqual(guid, acadLevel.Guid);
            }

            [TestMethod]
            public void AcademicLevel_Code()
            {
                Assert.AreEqual(code, acadLevel.Code);
            }

            [TestMethod]
            public void AcademicLevel_Description()
            {
                Assert.AreEqual(desc, acadLevel.Description);
            }

            [TestMethod]
            public void AcademicLevel_GradeScheme()
            {
                Assert.AreEqual(gradeScheme, acadLevel.GradeScheme);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void AcademicLevel_GuidNullException()
            {
                new AcademicLevel(null, code, desc);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void AcademicLevel_GuidEmptyException()
            {
                new AcademicLevel(string.Empty, code, desc);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void AcademicLevel_CodeNullException()
            {
                new AcademicLevel(guid, null, desc);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void AcademicLevelCodeEmptyException()
            {
                new AcademicLevel(guid, string.Empty, desc);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void AcademicLevelDescEmptyException()
            {
                new AcademicLevel(guid, code, string.Empty);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void AcademicLevel_DescNullException()
            {
                new AcademicLevel(guid, code, null);
            }

        }

        [TestClass]
        public class AcademicLevel_Equals
        {
            private string guid;
            private string code;
            private string desc;
            private AcademicLevel acadLevel1;
            private AcademicLevel acadLevel2;
            private AcademicLevel acadLevel3;

            [TestInitialize]
            public void Initialize()
            {
                code = "UG";
                desc = "Undergraduate";
                // Same code value, same GUID
                guid = GetGuid();
                acadLevel1 = new AcademicLevel(guid, code, desc);
                acadLevel2 = new AcademicLevel(guid, code, "Graduate");
                acadLevel3 = new AcademicLevel(GetGuid(), "GR", desc);
            }

            [TestMethod]
            public void AcademicLevel_Equals_SameCodesEqual()
            {
                Assert.IsTrue(acadLevel1.Equals(acadLevel2));
            }

            [TestMethod]
            public void AcademicLevel_Equals_DifferentCodeNotEqual()
            {
                Assert.IsFalse(acadLevel1.Equals(acadLevel3));
            }
        }

        [TestClass]
        public class AcademicLevel_GetHashCode
        {
            private string code;
            private string desc;
            private AcademicLevel acadLevel1;
            private AcademicLevel acadLevel2;
            private AcademicLevel acadLevel3;

            [TestInitialize]
            public void Initialize()
            {
                code = "UG";
                desc = "Undergraduate";
                // Same code value, same GUID
                var guid = GetGuid();
                acadLevel1 = new AcademicLevel(guid, code, desc);
                acadLevel2 = new AcademicLevel(guid, code, "Graduate");
                acadLevel3 = new AcademicLevel(GetGuid(), "GR", desc);
            }

            [TestMethod]
            public void AcademicLevel_GetHashCode_SameCodeHashEqual()
            {
                Assert.AreEqual(acadLevel1.GetHashCode(), acadLevel2.GetHashCode());
            }

            [TestMethod]
            public void AcademicLevel_GetHashCode_DifferentCodeHashNotEqual()
            {
                Assert.AreNotEqual(acadLevel1.GetHashCode(), acadLevel3.GetHashCode());
            }
        }

        private static string GetGuid()
        {
            return Guid.NewGuid().ToString();
        }
    }
}