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
    public class FloorCharacteristicsTests
    {
        [TestClass]
        public class FloorCharacteristicsConstructor
        {
            private string guid;
            private string code;
            private string desc;
            private FloorCharacteristics floorCharacteristics;

            [TestInitialize]
            public void Initialize()
            {
                guid = Guid.NewGuid().ToString();
                code = "AD";
                desc = "Admissions";
                floorCharacteristics = new FloorCharacteristics(guid, code, desc);
            }

            [TestMethod]
            public void FloorCharacteristics_Code()
            {
                Assert.AreEqual(code, floorCharacteristics.Code);
            }

            [TestMethod]
            public void FloorCharacteristics_Description()
            {
                Assert.AreEqual(desc, floorCharacteristics.Description);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void FloorCharacteristics_GuidNullException()
            {
                new FloorCharacteristics(null, code, desc);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void FloorCharacteristics_CodeNullException()
            {
                new FloorCharacteristics(guid, null, desc);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void FloorCharacteristics_DescNullException()
            {
                new FloorCharacteristics(guid, code, null);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void FloorCharacteristicsGuidEmptyException()
            {
                new FloorCharacteristics(string.Empty, code, desc);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void FloorCharacteristicsCodeEmptyException()
            {
                new FloorCharacteristics(guid, string.Empty, desc);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void FloorCharacteristicsDescEmptyException()
            {
                new FloorCharacteristics(guid, code, string.Empty);
            }

        }

        [TestClass]
        public class FloorCharacteristics_Equals
        {
            private string guid;
            private string code;
            private string desc;
            private FloorCharacteristics floorCharacteristics1;
            private FloorCharacteristics floorCharacteristics2;
            private FloorCharacteristics floorCharacteristics3;

            [TestInitialize]
            public void Initialize()
            {
                guid = Guid.NewGuid().ToString();
                code = "AD";
                desc = "Admissions";
                floorCharacteristics1 = new FloorCharacteristics(guid, code, desc);
                floorCharacteristics2 = new FloorCharacteristics(guid, code, "Second Year");
                floorCharacteristics3 = new FloorCharacteristics(Guid.NewGuid().ToString(), "200", desc);
            }

            [TestMethod]
            public void FloorCharacteristicsSameCodesEqual()
            {
                Assert.IsTrue(floorCharacteristics1.Equals(floorCharacteristics2));
            }

            [TestMethod]
            public void FloorCharacteristicsDifferentCodeNotEqual()
            {
                Assert.IsFalse(floorCharacteristics1.Equals(floorCharacteristics3));
            }
        }

        [TestClass]
        public class FloorCharacteristics_GetHashCode
        {
            private string guid;
            private string code;
            private string desc;
            private FloorCharacteristics floorCharacteristics1;
            private FloorCharacteristics floorCharacteristics2;
            private FloorCharacteristics floorCharacteristics3;

            [TestInitialize]
            public void Initialize()
            {
                guid = Guid.NewGuid().ToString();
                code = "AD";
                desc = "Admissions";
                floorCharacteristics1 = new FloorCharacteristics(guid, code, desc);
                floorCharacteristics2 = new FloorCharacteristics(guid, code, "Second Year");
                floorCharacteristics3 = new FloorCharacteristics(Guid.NewGuid().ToString(), "200", desc);
            }

            [TestMethod]
            public void FloorCharacteristicsSameCodeHashEqual()
            {
                Assert.AreEqual(floorCharacteristics1.GetHashCode(), floorCharacteristics2.GetHashCode());
            }

            [TestMethod]
            public void FloorCharacteristicsDifferentCodeHashNotEqual()
            {
                Assert.AreNotEqual(floorCharacteristics1.GetHashCode(), floorCharacteristics3.GetHashCode());
            }
        }
    }
}
