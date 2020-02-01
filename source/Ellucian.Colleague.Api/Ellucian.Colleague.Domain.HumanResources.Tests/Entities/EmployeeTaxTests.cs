/*Copyright 2019 Ellucian Company L.P. and its affiliates.*/
using Ellucian.Colleague.Domain.HumanResources.Entities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Domain.HumanResources.Tests.Entities
{
    [TestClass]
    public class EmployeeTaxTests
    {
        private string taxCode;
        private string taxDescription;
        private decimal? taxEmployerAmount;
        private decimal? taxEmployeeAmount;

        public EmployeeTax EmployeeTax;

        public string TaxCode
        {
            get
            {
                return taxCode;
            }

            set
            {
                taxCode = value;
            }
        }

        public string TaxDescription
        {
            get
            {
                return taxDescription;
            }

            set
            {
                taxDescription = value;
            }
        }

        public decimal? TaxEmployerAmount
        {
            get
            {
                return taxEmployerAmount;
            }

            set
            {
                taxEmployerAmount = value;
            }
        }

        public decimal? TaxEmployeeAmount
        {
            get
            {
                return taxEmployeeAmount;
            }

            set
            {
                taxEmployeeAmount = value;
            }
        }

        [TestClass]
        public class EmployeeTaxGenericTests : EmployeeTaxTests
        {
            [TestInitialize]
            public void Initialize()
            {
                TaxCode = "MEDI";
                TaxDescription = "Medicare Portion - FICA";
                TaxEmployerAmount = 1797.65m;
                TaxEmployeeAmount = 1797.65m;
                EmployeeTax = new EmployeeTax(TaxCode, TaxDescription, TaxEmployerAmount, TaxEmployeeAmount);
            }

            public EmployeeTax CreateEmployeeTax()
            {
                return new EmployeeTax(TaxCode, TaxDescription, TaxEmployerAmount, TaxEmployeeAmount);
            }

            #region ConstructorTests
            [TestMethod]
            public void ConstructorSetPropertiesTests()
            {
                Assert.AreEqual(TaxCode, EmployeeTax.TaxCode);
                Assert.AreEqual(TaxDescription, EmployeeTax.TaxDescription);
                Assert.AreEqual(TaxEmployerAmount, EmployeeTax.TaxEmployerAmount);
                Assert.AreEqual(TaxEmployeeAmount, EmployeeTax.TaxEmployeeAmount);
            }
            #endregion

            #region AttributeTests
            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void TaxCodeNullTest()
            {
                TaxCode = null;
                var error = CreateEmployeeTax();

            }

            [TestMethod]
            public void TaxEmployerAmountIsNullTest()
            {
                EmployeeTax.TaxEmployerAmount = null;
                Assert.IsNull(EmployeeTax.TaxEmployerAmount);

            }

            [TestMethod]
            public void TaxEmployeeAmountIsNullTest()
            {
                EmployeeTax.TaxEmployeeAmount = null;
                Assert.IsNull(EmployeeTax.TaxEmployeeAmount);

            }
            #endregion

            #region EqualsTests
            [TestMethod]
            public void ObjectsEqualWhenIdsEqualTest()
            {
                var empTax1 = CreateEmployeeTax();
                var empTax2 = CreateEmployeeTax();
                Assert.AreEqual(empTax1.TaxCode, empTax2.TaxCode);
            }

            [TestMethod]
            public void ObjectsNotEqualWhenIdsNotEqualTest()
            {
                var empBended1 = CreateEmployeeTax();
                TaxCode = "MED1";
                var empBended2 = CreateEmployeeTax();
                Assert.AreNotEqual(empBended1.TaxCode, empBended2.TaxCode);
            }

            [TestMethod]
            public void ObjectsNotEqualWhenInputIsNullTest()
            {
                Assert.IsFalse(CreateEmployeeTax().Equals(null));
            }
            #endregion


        }
    }
}
