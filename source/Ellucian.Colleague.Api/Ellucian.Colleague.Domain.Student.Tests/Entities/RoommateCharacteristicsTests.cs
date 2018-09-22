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
    public class RoommateCharacteristicsTests
    {
        [TestClass]
        public class RoommateCharacteristicsConstructor
        {
            private string guid;
            private string code;
            private string desc;
            private RoommateCharacteristics roommateCharacteristics;

            [TestInitialize]
            public void Initialize()
            {
                guid = Guid.NewGuid().ToString();
                code = "AD";
                desc = "Admissions";
                roommateCharacteristics = new RoommateCharacteristics(guid, code, desc);
            }

            [TestMethod]
            public void RoommateCharacteristics_Code()
            {
                Assert.AreEqual(code, roommateCharacteristics.Code);
            }

            [TestMethod]
            public void RoommateCharacteristics_Description()
            {
                Assert.AreEqual(desc, roommateCharacteristics.Description);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void RoommateCharacteristics_GuidNullException()
            {
                new RoommateCharacteristics(null, code, desc);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void RoommateCharacteristics_CodeNullException()
            {
                new RoommateCharacteristics(guid, null, desc);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void RoommateCharacteristics_DescNullException()
            {
                new RoommateCharacteristics(guid, code, null);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void RoommateCharacteristicsGuidEmptyException()
            {
                new RoommateCharacteristics(string.Empty, code, desc);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void RoommateCharacteristicsCodeEmptyException()
            {
                new RoommateCharacteristics(guid, string.Empty, desc);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void RoommateCharacteristicsDescEmptyException()
            {
                new RoommateCharacteristics(guid, code, string.Empty);
            }

        }

        [TestClass]
        public class RoommateCharacteristics_Equals
        {
            private string guid;
            private string code;
            private string desc;
            private RoommateCharacteristics roommateCharacteristics1;
            private RoommateCharacteristics roommateCharacteristics2;
            private RoommateCharacteristics roommateCharacteristics3;

            [TestInitialize]
            public void Initialize()
            {
                guid = Guid.NewGuid().ToString();
                code = "AD";
                desc = "Admissions";
                roommateCharacteristics1 = new RoommateCharacteristics(guid, code, desc);
                roommateCharacteristics2 = new RoommateCharacteristics(guid, code, "Second Year");
                roommateCharacteristics3 = new RoommateCharacteristics(Guid.NewGuid().ToString(), "200", desc);
            }

            [TestMethod]
            public void RoommateCharacteristicsSameCodesEqual()
            {
                Assert.IsTrue(roommateCharacteristics1.Equals(roommateCharacteristics2));
            }

            [TestMethod]
            public void RoommateCharacteristicsDifferentCodeNotEqual()
            {
                Assert.IsFalse(roommateCharacteristics1.Equals(roommateCharacteristics3));
            }
        }

        [TestClass]
        public class RoommateCharacteristics_GetHashCode
        {
            private string guid;
            private string code;
            private string desc;
            private RoommateCharacteristics roommateCharacteristics1;
            private RoommateCharacteristics roommateCharacteristics2;
            private RoommateCharacteristics roommateCharacteristics3;

            [TestInitialize]
            public void Initialize()
            {
                guid = Guid.NewGuid().ToString();
                code = "AD";
                desc = "Admissions";
                roommateCharacteristics1 = new RoommateCharacteristics(guid, code, desc);
                roommateCharacteristics2 = new RoommateCharacteristics(guid, code, "Second Year");
                roommateCharacteristics3 = new RoommateCharacteristics(Guid.NewGuid().ToString(), "200", desc);
            }

            [TestMethod]
            public void RoommateCharacteristicsSameCodeHashEqual()
            {
                Assert.AreEqual(roommateCharacteristics1.GetHashCode(), roommateCharacteristics2.GetHashCode());
            }

            [TestMethod]
            public void RoommateCharacteristicsDifferentCodeHashNotEqual()
            {
                Assert.AreNotEqual(roommateCharacteristics1.GetHashCode(), roommateCharacteristics3.GetHashCode());
            }
        }
    }
}
