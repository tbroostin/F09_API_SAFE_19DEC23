using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ellucian.Colleague.Domain.Student.Entities;

namespace Ellucian.Colleague.Domain.Student.Tests.Entities
{
    [TestClass]
    public class StudentStatusTests
    {
        [TestClass]
        public class StudentStatusConstructor
        {
            private string guid;
            private string code;
            private string desc;
            private StudentStatus studentStatus;

            [TestInitialize]
            public void Initialize()
            {
                guid = Guid.NewGuid().ToString();
                code = "01";
                desc = "type1";
                studentStatus = new StudentStatus(guid, code, desc);
            }

            [TestMethod]
            public void StudentStatus_Code()
            {
                Assert.AreEqual(code, studentStatus.Code);
            }

            [TestMethod]
            public void StudentStatus_Description()
            {
                Assert.AreEqual(desc, studentStatus.Description);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void StudentStatus_GuidNullException()
            {
                new StudentStatus(null, code, desc);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void StudentStatus_CodeNullException()
            {
                new StudentStatus(guid, null, desc);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void StudentStatus_DescNullException()
            {
                new StudentStatus(guid, code, null);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void StudentStatusGuidEmptyException()
            {
                new StudentStatus(string.Empty, code, desc);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void StudentStatusCodeEmptyException()
            {
                new StudentStatus(guid, string.Empty, desc);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void StudentStatusDescEmptyException()
            {
                new StudentStatus(guid, code, string.Empty);
            }

        }

        [TestClass]
        public class StudentStatus_Equals
        {
            private string guid;
            private string code;
            private string desc;
            private StudentStatus studentStatus1;
            private StudentStatus studentStatus2;
            private StudentStatus studentStatus3;

            [TestInitialize]
            public void Initialize()
            {
                guid = Guid.NewGuid().ToString();
                code = "01";
                desc = "description type1";
                studentStatus1 = new StudentStatus(guid, code, desc);
                studentStatus2 = new StudentStatus(guid, code, "decription type2");
                studentStatus3 = new StudentStatus(Guid.NewGuid().ToString(), "02", desc);
            }

            [TestMethod]
            public void StudentStatusSameCodesEqual()
            {
                Assert.IsTrue(studentStatus1.Equals(studentStatus2));
            }

            [TestMethod]
            public void StudentStatusDifferentCodeNotEqual()
            {
                Assert.IsFalse(studentStatus1.Equals(studentStatus3));
            }
        }

        [TestClass]
        public class StudentStatus_GetHashCode
        {
            private string guid;
            private string code;
            private string desc;
            private StudentStatus studentStatus1;
            private StudentStatus studentStatus2;
            private StudentStatus studentStatus3;

            [TestInitialize]
            public void Initialize()
            {
                guid = Guid.NewGuid().ToString();
                code = "01";
                desc = "description type1";
                studentStatus1 = new StudentStatus(guid, code, desc);
                studentStatus2 = new StudentStatus(guid, code, "description type2");
                studentStatus3 = new StudentStatus(Guid.NewGuid().ToString(), "02", desc);
            }

            [TestMethod]
            public void StudentStatusSameCodeHashEqual()
            {
                Assert.AreEqual(studentStatus1.GetHashCode(), studentStatus2.GetHashCode());
            }

            [TestMethod]
            public void StudentStatusDifferentCodeHashNotEqual()
            {
                Assert.AreNotEqual(studentStatus1.GetHashCode(), studentStatus3.GetHashCode());
            }
        }
    }
}
