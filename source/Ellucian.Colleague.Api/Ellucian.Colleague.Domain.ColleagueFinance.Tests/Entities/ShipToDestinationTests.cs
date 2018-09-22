//Copyright 2017 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ellucian.Colleague.Domain.ColleagueFinance.Entities;

namespace Ellucian.Colleague.Domain.ColleagueFinance.Tests.Entities
{
     [TestClass]
    public class ShipToDestinationTests
    {
        [TestClass]
        public class ShipToDestinationConstructor
        {
            private string guid;
            private string code;
            private string desc;
            private ShipToDestination shipToDestinations;

            [TestInitialize]
            public void Initialize()
            {
                guid = Guid.NewGuid().ToString();
                code = "AD";
                desc = "Admissions";
                shipToDestinations = new ShipToDestination(guid, code, desc);
            }

            [TestMethod]
            public void ShipToDestination_Code()
            {
                Assert.AreEqual(code, shipToDestinations.Code);
            }

            [TestMethod]
            public void ShipToDestination_Description()
            {
                Assert.AreEqual(desc, shipToDestinations.Description);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void ShipToDestination_GuidNullException()
            {
                new ShipToDestination(null, code, desc);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void ShipToDestination_CodeNullException()
            {
                new ShipToDestination(guid, null, desc);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void ShipToDestination_DescNullException()
            {
                new ShipToDestination(guid, code, null);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void ShipToDestinationGuidEmptyException()
            {
                new ShipToDestination(string.Empty, code, desc);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void ShipToDestinationCodeEmptyException()
            {
                new ShipToDestination(guid, string.Empty, desc);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void ShipToDestinationDescEmptyException()
            {
                new ShipToDestination(guid, code, string.Empty);
            }

        }

        [TestClass]
        public class ShipToDestination_Equals
        {
            private string guid;
            private string code;
            private string desc;
            private ShipToDestination shipToDestinations1;
            private ShipToDestination shipToDestinations2;
            private ShipToDestination shipToDestinations3;

            [TestInitialize]
            public void Initialize()
            {
                guid = Guid.NewGuid().ToString();
                code = "AD";
                desc = "Admissions";
                shipToDestinations1 = new ShipToDestination(guid, code, desc);
                shipToDestinations2 = new ShipToDestination(guid, code, "Second Year");
                shipToDestinations3 = new ShipToDestination(Guid.NewGuid().ToString(), "200", desc);
            }

            [TestMethod]
            public void ShipToDestinationSameCodesEqual()
            {
                Assert.IsTrue(shipToDestinations1.Equals(shipToDestinations2));
            }

            [TestMethod]
            public void ShipToDestinationDifferentCodeNotEqual()
            {
                Assert.IsFalse(shipToDestinations1.Equals(shipToDestinations3));
            }
        }

        [TestClass]
        public class ShipToDestination_GetHashCode
        {
            private string guid;
            private string code;
            private string desc;
            private ShipToDestination shipToDestinations1;
            private ShipToDestination shipToDestinations2;
            private ShipToDestination shipToDestinations3;

            [TestInitialize]
            public void Initialize()
            {
                guid = Guid.NewGuid().ToString();
                code = "AD";
                desc = "Admissions";
                shipToDestinations1 = new ShipToDestination(guid, code, desc);
                shipToDestinations2 = new ShipToDestination(guid, code, "Second Year");
                shipToDestinations3 = new ShipToDestination(Guid.NewGuid().ToString(), "200", desc);
            }

            [TestMethod]
            public void ShipToDestinationSameCodeHashEqual()
            {
                Assert.AreEqual(shipToDestinations1.GetHashCode(), shipToDestinations2.GetHashCode());
            }

            [TestMethod]
            public void ShipToDestinationDifferentCodeHashNotEqual()
            {
                Assert.AreNotEqual(shipToDestinations1.GetHashCode(), shipToDestinations3.GetHashCode());
            }
        }
    }
}
