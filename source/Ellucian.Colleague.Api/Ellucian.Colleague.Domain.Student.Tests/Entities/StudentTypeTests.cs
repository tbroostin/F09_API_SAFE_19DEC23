using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ellucian.Colleague.Domain.Student.Entities;

namespace Ellucian.Colleague.Domain.Student.Tests.Entities
{
    [TestClass]
    public class StudentTypeTests
    {
        [TestClass]
        public class StudentTypeConstructor
        {
            private string guid;
            private string code;
            private string desc;
            private StudentType studentType;

            [TestInitialize]
            public void Initialize()
            {
                guid = Guid.NewGuid().ToString();
                code = "01";
                desc = "type1";
                studentType = new StudentType(guid, code, desc);
            }

            [TestMethod]
            public void StudentType_Code()
            {
                Assert.AreEqual(code, studentType.Code);
            }

            [TestMethod]
            public void StudentType_Description()
            {
                Assert.AreEqual(desc, studentType.Description);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void StudentType_GuidNullException()
            {
                new StudentType(null, code, desc);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void StudentType_CodeNullException()
            {
                new StudentType(guid, null, desc);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void StudentType_DescNullException()
            {
                new StudentType(guid, code, null);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void StudentTypeGuidEmptyException()
            {
                new StudentType(string.Empty, code, desc);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void StudentTypeCodeEmptyException()
            {
                new StudentType(guid, string.Empty, desc);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void StudentTypeDescEmptyException()
            {
                new StudentType(guid, code, string.Empty);
            }

        }

        [TestClass]
        public class StudentType_Equals
        {
            private string guid;
            private string code;
            private string desc;
            private StudentType studentType1;
            private StudentType studentType2;
            private StudentType studentType3;

            [TestInitialize]
            public void Initialize()
            {
                guid = Guid.NewGuid().ToString();
                code = "01";
                desc = "description type1";
                studentType1 = new StudentType(guid, code, desc);
                studentType2 = new StudentType(guid, code, "decription type2");
                studentType3 = new StudentType(Guid.NewGuid().ToString(), "02", desc);
            }

            [TestMethod]
            public void StudentTypeSameCodesEqual()
            {
                Assert.IsTrue(studentType1.Equals(studentType2));
            }

            [TestMethod]
            public void StudentTypeDifferentCodeNotEqual()
            {
                Assert.IsFalse(studentType1.Equals(studentType3));
            }
        }

        [TestClass]
        public class StudentType_GetHashCode
        {
            private string guid;
            private string code;
            private string desc;
            private StudentType studentType1;
            private StudentType studentType2;
            private StudentType studentType3;

            [TestInitialize]
            public void Initialize()
            {
                guid = Guid.NewGuid().ToString();
                code = "01";
                desc = "description type1";
                studentType1 = new StudentType(guid, code, desc);
                studentType2 = new StudentType(guid, code, "description type2");
                studentType3 = new StudentType(Guid.NewGuid().ToString(), "02", desc);
            }

            [TestMethod]
            public void StudentTypeSameCodeHashEqual()
            {
                Assert.AreEqual(studentType1.GetHashCode(), studentType2.GetHashCode());
            }

            [TestMethod]
            public void StudentTypeDifferentCodeHashNotEqual()
            {
                Assert.AreNotEqual(studentType1.GetHashCode(), studentType3.GetHashCode());
            }
        }
    }
}
