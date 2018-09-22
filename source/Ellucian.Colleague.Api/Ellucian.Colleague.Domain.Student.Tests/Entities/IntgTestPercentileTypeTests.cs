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
    public class IntgTestPercentileTypeTests
    {
        [TestClass]
        public class IntgTestPercentileTypeConstructor
        {
            private string guid;
            private string code;
            private string desc;
            private IntgTestPercentileType intgTestPercentileTypes;

            [TestInitialize]
            public void Initialize()
            {
                guid = Guid.NewGuid().ToString();
                code = "AD";
                desc = "Admissions";
                intgTestPercentileTypes = new IntgTestPercentileType(guid, code, desc);
            }

            [TestMethod]
            public void IntgTestPercentileType_Code()
            {
                Assert.AreEqual(code, intgTestPercentileTypes.Code);
            }

            [TestMethod]
            public void IntgTestPercentileType_Description()
            {
                Assert.AreEqual(desc, intgTestPercentileTypes.Description);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void IntgTestPercentileType_GuidNullException()
            {
                new IntgTestPercentileType(null, code, desc);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void IntgTestPercentileType_CodeNullException()
            {
                new IntgTestPercentileType(guid, null, desc);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void IntgTestPercentileType_DescNullException()
            {
                new IntgTestPercentileType(guid, code, null);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void IntgTestPercentileTypeGuidEmptyException()
            {
                new IntgTestPercentileType(string.Empty, code, desc);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void IntgTestPercentileTypeCodeEmptyException()
            {
                new IntgTestPercentileType(guid, string.Empty, desc);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void IntgTestPercentileTypeDescEmptyException()
            {
                new IntgTestPercentileType(guid, code, string.Empty);
            }

        }

        [TestClass]
        public class IntgTestPercentileType_Equals
        {
            private string guid;
            private string code;
            private string desc;
            private IntgTestPercentileType intgTestPercentileTypes1;
            private IntgTestPercentileType intgTestPercentileTypes2;
            private IntgTestPercentileType intgTestPercentileTypes3;

            [TestInitialize]
            public void Initialize()
            {
                guid = Guid.NewGuid().ToString();
                code = "AD";
                desc = "Admissions";
                intgTestPercentileTypes1 = new IntgTestPercentileType(guid, code, desc);
                intgTestPercentileTypes2 = new IntgTestPercentileType(guid, code, "Second Year");
                intgTestPercentileTypes3 = new IntgTestPercentileType(Guid.NewGuid().ToString(), "200", desc);
            }

            [TestMethod]
            public void IntgTestPercentileTypeSameCodesEqual()
            {
                Assert.IsTrue(intgTestPercentileTypes1.Equals(intgTestPercentileTypes2));
            }

            [TestMethod]
            public void IntgTestPercentileTypeDifferentCodeNotEqual()
            {
                Assert.IsFalse(intgTestPercentileTypes1.Equals(intgTestPercentileTypes3));
            }
        }

        [TestClass]
        public class IntgTestPercentileType_GetHashCode
        {
            private string guid;
            private string code;
            private string desc;
            private IntgTestPercentileType intgTestPercentileTypes1;
            private IntgTestPercentileType intgTestPercentileTypes2;
            private IntgTestPercentileType intgTestPercentileTypes3;

            [TestInitialize]
            public void Initialize()
            {
                guid = Guid.NewGuid().ToString();
                code = "AD";
                desc = "Admissions";
                intgTestPercentileTypes1 = new IntgTestPercentileType(guid, code, desc);
                intgTestPercentileTypes2 = new IntgTestPercentileType(guid, code, "Second Year");
                intgTestPercentileTypes3 = new IntgTestPercentileType(Guid.NewGuid().ToString(), "200", desc);
            }

            [TestMethod]
            public void IntgTestPercentileTypeSameCodeHashEqual()
            {
                Assert.AreEqual(intgTestPercentileTypes1.GetHashCode(), intgTestPercentileTypes2.GetHashCode());
            }

            [TestMethod]
            public void IntgTestPercentileTypeDifferentCodeHashNotEqual()
            {
                Assert.AreNotEqual(intgTestPercentileTypes1.GetHashCode(), intgTestPercentileTypes3.GetHashCode());
            }
        }
    }
}
