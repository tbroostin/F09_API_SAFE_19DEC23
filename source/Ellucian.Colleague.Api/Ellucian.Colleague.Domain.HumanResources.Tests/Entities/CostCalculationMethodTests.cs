//Copyright 2017 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ellucian.Colleague.Domain.HumanResources.Entities;

namespace Ellucian.Colleague.Domain.HumanResources.Tests.Entities
{
     [TestClass]
    public class CostCalculationMethodTests
    {
        [TestClass]
        public class CostCalculationMethodConstructor
        {
            private string guid;
            private string code;
            private string desc;
            private CostCalculationMethod costCalculationMethods;

            [TestInitialize]
            public void Initialize()
            {
                guid = Guid.NewGuid().ToString();
                code = "AD";
                desc = "Admissions";
                costCalculationMethods = new CostCalculationMethod(guid, code, desc);
            }

            [TestMethod]
            public void CostCalculationMethod_Code()
            {
                Assert.AreEqual(code, costCalculationMethods.Code);
            }

            [TestMethod]
            public void CostCalculationMethod_Description()
            {
                Assert.AreEqual(desc, costCalculationMethods.Description);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void CostCalculationMethod_GuidNullException()
            {
                new CostCalculationMethod(null, code, desc);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void CostCalculationMethod_CodeNullException()
            {
                new CostCalculationMethod(guid, null, desc);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void CostCalculationMethod_DescNullException()
            {
                new CostCalculationMethod(guid, code, null);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void CostCalculationMethodGuidEmptyException()
            {
                new CostCalculationMethod(string.Empty, code, desc);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void CostCalculationMethodCodeEmptyException()
            {
                new CostCalculationMethod(guid, string.Empty, desc);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void CostCalculationMethodDescEmptyException()
            {
                new CostCalculationMethod(guid, code, string.Empty);
            }

        }

        [TestClass]
        public class CostCalculationMethod_Equals
        {
            private string guid;
            private string code;
            private string desc;
            private CostCalculationMethod costCalculationMethods1;
            private CostCalculationMethod costCalculationMethods2;
            private CostCalculationMethod costCalculationMethods3;

            [TestInitialize]
            public void Initialize()
            {
                guid = Guid.NewGuid().ToString();
                code = "AD";
                desc = "Admissions";
                costCalculationMethods1 = new CostCalculationMethod(guid, code, desc);
                costCalculationMethods2 = new CostCalculationMethod(guid, code, "Second Year");
                costCalculationMethods3 = new CostCalculationMethod(Guid.NewGuid().ToString(), "200", desc);
            }

            [TestMethod]
            public void CostCalculationMethodSameCodesEqual()
            {
                Assert.IsTrue(costCalculationMethods1.Equals(costCalculationMethods2));
            }

            [TestMethod]
            public void CostCalculationMethodDifferentCodeNotEqual()
            {
                Assert.IsFalse(costCalculationMethods1.Equals(costCalculationMethods3));
            }
        }

        [TestClass]
        public class CostCalculationMethod_GetHashCode
        {
            private string guid;
            private string code;
            private string desc;
            private CostCalculationMethod costCalculationMethods1;
            private CostCalculationMethod costCalculationMethods2;
            private CostCalculationMethod costCalculationMethods3;

            [TestInitialize]
            public void Initialize()
            {
                guid = Guid.NewGuid().ToString();
                code = "AD";
                desc = "Admissions";
                costCalculationMethods1 = new CostCalculationMethod(guid, code, desc);
                costCalculationMethods2 = new CostCalculationMethod(guid, code, "Second Year");
                costCalculationMethods3 = new CostCalculationMethod(Guid.NewGuid().ToString(), "200", desc);
            }

            [TestMethod]
            public void CostCalculationMethodSameCodeHashEqual()
            {
                Assert.AreEqual(costCalculationMethods1.GetHashCode(), costCalculationMethods2.GetHashCode());
            }

            [TestMethod]
            public void CostCalculationMethodDifferentCodeHashNotEqual()
            {
                Assert.AreNotEqual(costCalculationMethods1.GetHashCode(), costCalculationMethods3.GetHashCode());
            }
        }
    }
}
