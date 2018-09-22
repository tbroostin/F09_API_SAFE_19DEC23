//Copyright 2017 Ellucian Company L.P. and its affiliates.

using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ellucian.Colleague.Domain.Base.Entities;

namespace Ellucian.Colleague.Domain.Base.Tests.Entities
{
    [TestClass]
    public class PositionsTests
    {
        [TestClass]
        public class PositionsConstructor
        {
            private string guid;
            private string code;
            private string desc;
            private Positions externalEmploymentPositions;

            [TestInitialize]
            public void Initialize()
            {
                guid = Guid.NewGuid().ToString();
                code = "AD";
                desc = "Admissions";
                externalEmploymentPositions = new Positions(guid, code, desc);
            }

            [TestMethod]
            public void Positions_Code()
            {
                Assert.AreEqual(code, externalEmploymentPositions.Code);
            }

            [TestMethod]
            public void Positions_Description()
            {
                Assert.AreEqual(desc, externalEmploymentPositions.Description);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void Positions_GuidNullException()
            {
                new Positions(null, code, desc);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void Positions_CodeNullException()
            {
                new Positions(guid, null, desc);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void Positions_DescNullException()
            {
                new Positions(guid, code, null);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void PositionsGuidEmptyException()
            {
                new Positions(string.Empty, code, desc);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void PositionsCodeEmptyException()
            {
                new Positions(guid, string.Empty, desc);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void PositionsDescEmptyException()
            {
                new Positions(guid, code, string.Empty);
            }

        }

        [TestClass]
        public class Positions_Equals
        {
            private string guid;
            private string code;
            private string desc;
            private Positions externalEmploymentPositions1;
            private Positions externalEmploymentPositions2;
            private Positions externalEmploymentPositions3;

            [TestInitialize]
            public void Initialize()
            {
                guid = Guid.NewGuid().ToString();
                code = "AD";
                desc = "Admissions";
                externalEmploymentPositions1 = new Positions(guid, code, desc);
                externalEmploymentPositions2 = new Positions(guid, code, "Second Year");
                externalEmploymentPositions3 = new Positions(Guid.NewGuid().ToString(), "200", desc);
            }

            [TestMethod]
            public void PositionsSameCodesEqual()
            {
                Assert.IsTrue(externalEmploymentPositions1.Equals(externalEmploymentPositions2));
            }

            [TestMethod]
            public void PositionsDifferentCodeNotEqual()
            {
                Assert.IsFalse(externalEmploymentPositions1.Equals(externalEmploymentPositions3));
            }
        }

        [TestClass]
        public class Positions_GetHashCode
        {
            private string guid;
            private string code;
            private string desc;
            private Positions externalEmploymentPositions1;
            private Positions externalEmploymentPositions2;
            private Positions externalEmploymentPositions3;

            [TestInitialize]
            public void Initialize()
            {
                guid = Guid.NewGuid().ToString();
                code = "AD";
                desc = "Admissions";
                externalEmploymentPositions1 = new Positions(guid, code, desc);
                externalEmploymentPositions2 = new Positions(guid, code, "Second Year");
                externalEmploymentPositions3 = new Positions(Guid.NewGuid().ToString(), "200", desc);
            }

            [TestMethod]
            public void PositionsSameCodeHashEqual()
            {
                Assert.AreEqual(externalEmploymentPositions1.GetHashCode(), externalEmploymentPositions2.GetHashCode());
            }

            [TestMethod]
            public void PositionsDifferentCodeHashNotEqual()
            {
                Assert.AreNotEqual(externalEmploymentPositions1.GetHashCode(), externalEmploymentPositions3.GetHashCode());
            }
        }
    }
}
