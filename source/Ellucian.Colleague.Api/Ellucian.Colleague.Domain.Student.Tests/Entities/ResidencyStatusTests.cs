using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ellucian.Colleague.Domain.Student.Entities;

namespace Ellucian.Colleague.Domain.Student.Tests.Entities
{
    [TestClass]
    public class ResidencyStatusTests
    {
        [TestClass]
        public class ResidencyStatusConstructor
        {
            private string guid;
            private string code;
            private string desc;
            private ResidencyStatus residencyStatus;

            [TestInitialize]
            public void Initialize()
            {
                guid = Guid.NewGuid().ToString();
                code = "01";
                desc = "type1";
                residencyStatus = new ResidencyStatus(guid, code, desc);
            }

            [TestMethod]
            public void ResidencyStatus_Code()
            {
                Assert.AreEqual(code, residencyStatus.Code);
            }

            [TestMethod]
            public void ResidencyStatus_Description()
            {
                Assert.AreEqual(desc, residencyStatus.Description);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void ResidencyStatus_GuidNullException()
            {
                new ResidencyStatus(null, code, desc);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void ResidencyStatus_CodeNullException()
            {
                new ResidencyStatus(guid, null, desc);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void ResidencyStatus_DescNullException()
            {
                new ResidencyStatus(guid, code, null);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void ResidencyStatusGuidEmptyException()
            {
                new ResidencyStatus(string.Empty, code, desc);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void ResidencyStatusCodeEmptyException()
            {
                new ResidencyStatus(guid, string.Empty, desc);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void ResidencyStatusDescEmptyException()
            {
                new ResidencyStatus(guid, code, string.Empty);
            }

        }

        [TestClass]
        public class ResidencyStatus_Equals
        {
            private string guid;
            private string code;
            private string desc;
            private ResidencyStatus residencyStatus1;
            private ResidencyStatus residencyStatus2;
            private ResidencyStatus residencyStatus3;

            [TestInitialize]
            public void Initialize()
            {
                guid = Guid.NewGuid().ToString();
                code = "01";
                desc = "description type1";
                residencyStatus1 = new ResidencyStatus(guid, code, desc);
                residencyStatus2 = new ResidencyStatus(guid, code, "decription type2");
                residencyStatus3 = new ResidencyStatus(Guid.NewGuid().ToString(), "02", desc);
            }

            [TestMethod]
            public void ResidencyStatusSameCodesEqual()
            {
                Assert.IsTrue(residencyStatus1.Equals(residencyStatus2));
            }

            [TestMethod]
            public void ResidencyStatusDifferentCodeNotEqual()
            {
                Assert.IsFalse(residencyStatus1.Equals(residencyStatus3));
            }
        }

        [TestClass]
        public class ResidencyStatus_GetHashCode
        {
            private string guid;
            private string code;
            private string desc;
            private ResidencyStatus residencyStatus1;
            private ResidencyStatus residencyStatus2;
            private ResidencyStatus residencyStatus3;

            [TestInitialize]
            public void Initialize()
            {
                guid = Guid.NewGuid().ToString();
                code = "01";
                desc = "description type1";
                residencyStatus1 = new ResidencyStatus(guid, code, desc);
                residencyStatus2 = new ResidencyStatus(guid, code, "description type2");
                residencyStatus3 = new ResidencyStatus(Guid.NewGuid().ToString(), "02", desc);
            }

            [TestMethod]
            public void ResidencyStatusSameCodeHashEqual()
            {
                Assert.AreEqual(residencyStatus1.GetHashCode(), residencyStatus2.GetHashCode());
            }

            [TestMethod]
            public void ResidencyStatusDifferentCodeHashNotEqual()
            {
                Assert.AreNotEqual(residencyStatus1.GetHashCode(), residencyStatus3.GetHashCode());
            }
        }
    }
}
