//Copyright 1993-2020 Ellucian Company L.P. and its affiliates.

using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ellucian.Colleague.Domain.Student.Entities;

namespace Ellucian.Colleague.Domain.Student.Tests.Entities
{
    [TestClass]
    public class CareerGoalTests
    {
        [TestClass]
        public class CareerGoalConstructor
        {
            private string guid;
            private string code;
            private string desc;
            private CareerGoal CareerGoal;

            [TestInitialize]
            public void Initialize()
            {
                guid = Guid.NewGuid().ToString();
                code = "AD";
                desc = "Admissions";
                CareerGoal = new CareerGoal(guid, code, desc);
            }

            [TestMethod]
            public void CareerGoal_Code()
            {
                Assert.AreEqual(code, CareerGoal.Code);
            }

            [TestMethod]
            public void CareerGoal_Description()
            {
                Assert.AreEqual(desc, CareerGoal.Description);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void CareerGoal_GuidNullException()
            {
                new CareerGoal(null, code, desc);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void CareerGoal_CodeNullException()
            {
                new CareerGoal(guid, null, desc);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void CareerGoal_DescNullException()
            {
                new CareerGoal(guid, code, null);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void CareerGoalGuidEmptyException()
            {
                new CareerGoal(string.Empty, code, desc);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void CareerGoalCodeEmptyException()
            {
                new CareerGoal(guid, string.Empty, desc);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void CareerGoalDescEmptyException()
            {
                new CareerGoal(guid, code, string.Empty);
            }

        }

        [TestClass]
        public class CareerGoal_Equals
        {
            private string guid;
            private string code;
            private string desc;
            private CareerGoal CareerGoal1;
            private CareerGoal CareerGoal2;
            private CareerGoal CareerGoal3;

            [TestInitialize]
            public void Initialize()
            {
                guid = Guid.NewGuid().ToString();
                code = "AD";
                desc = "Admissions";
                CareerGoal1 = new CareerGoal(guid, code, desc);
                CareerGoal2 = new CareerGoal(guid, code, "Second Year");
                CareerGoal3 = new CareerGoal(Guid.NewGuid().ToString(), "200", desc);
            }

            [TestMethod]
            public void CareerGoalSameCodesEqual()
            {
                Assert.IsTrue(CareerGoal1.Equals(CareerGoal2));
            }

            [TestMethod]
            public void CareerGoalDifferentCodeNotEqual()
            {
                Assert.IsFalse(CareerGoal1.Equals(CareerGoal3));
            }
        }

        [TestClass]
        public class CareerGoal_GetHashCode
        {
            private string guid;
            private string code;
            private string desc;
            private CareerGoal CareerGoal1;
            private CareerGoal CareerGoal2;
            private CareerGoal CareerGoal3;

            [TestInitialize]
            public void Initialize()
            {
                guid = Guid.NewGuid().ToString();
                code = "AD";
                desc = "Admissions";
                CareerGoal1 = new CareerGoal(guid, code, desc);
                CareerGoal2 = new CareerGoal(guid, code, "Second Year");
                CareerGoal3 = new CareerGoal(Guid.NewGuid().ToString(), "200", desc);
            }

            [TestMethod]
            public void CareerGoalSameCodeHashEqual()
            {
                Assert.AreEqual(CareerGoal1.GetHashCode(), CareerGoal2.GetHashCode());
            }

            [TestMethod]
            public void CareerGoalDifferentCodeHashNotEqual()
            {
                Assert.AreNotEqual(CareerGoal1.GetHashCode(), CareerGoal3.GetHashCode());
            }
        }
    }
}
