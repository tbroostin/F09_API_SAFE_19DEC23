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
    public class EmployeeBendedTests
    {
        private string benededCode;
        private string benededDescription;
        private decimal? benededEmployerAmount;
        private decimal? benededEmployeeAmount;

        public EmployeeBended EmployeeBended;

        public string BenededCode
        {
            get
            {
                return benededCode;
            }

            set
            {
                benededCode = value;
            }
        }

        public string BenededDescription
        {
            get
            {
                return benededDescription;
            }

            set
            {
                benededDescription = value;
            }
        }

        public decimal? BenededEmployerAmount
        {
            get
            {
                return benededEmployerAmount;
            }

            set
            {
                benededEmployerAmount = value;
            }
        }

        public decimal? BenededEmployeeAmount
        {
            get
            {
                return benededEmployeeAmount;
            }

            set
            {
                benededEmployeeAmount = value;
            }
        }

        [TestClass]
        public class EmployeeBendedGenericTests : EmployeeBendedTests
        {
            [TestInitialize]
            public void Initialize()
            {
                BenededCode = "DEP1";
                BenededDescription = "Dental Employee Plus One";
                BenededEmployerAmount = 780.72m;
                BenededEmployeeAmount = 207.48m;

                EmployeeBended = new EmployeeBended(BenededCode, BenededDescription, BenededEmployerAmount, BenededEmployeeAmount);
            }

            public EmployeeBended CreateEmployeeBended()
            {
                return new EmployeeBended(BenededCode, BenededDescription, BenededEmployerAmount, BenededEmployeeAmount);
            }

            #region ConstructorTests
            [TestMethod]
            public void ConstructorSetPropertiesTests()
            {
                Assert.AreEqual(BenededCode, EmployeeBended.BenededCode);
                Assert.AreEqual(BenededDescription, EmployeeBended.BenededDescription);
                Assert.AreEqual(BenededEmployerAmount, EmployeeBended.BenededEmployerAmount);
                Assert.AreEqual(BenededEmployeeAmount, EmployeeBended.BenededEmployeeAmount);
            }
            #endregion 

            #region AttributeTests
            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void BenededCodeNullTest()
            {
                BenededCode = null;
                var error = CreateEmployeeBended();
                
            }
            [TestMethod]
            public void BenededEmployerAmountIsNullTest()
            {
                EmployeeBended.BenededEmployerAmount = null;
                Assert.IsNull(EmployeeBended.BenededEmployerAmount);

            }

            [TestMethod]
            public void BenededEmployeeAmountIsNullTest()
            {
                EmployeeBended.BenededEmployeeAmount = null;
                Assert.IsNull(EmployeeBended.BenededEmployeeAmount);

            }
            #endregion

            #region EqualsTests
            [TestMethod]
            public void ObjectsEqualWhenIdsEqualTest()
            {
                var empBended1 = CreateEmployeeBended();
                var empBended2 = CreateEmployeeBended();
                Assert.AreEqual(empBended1.BenededCode, empBended2.BenededCode);
            }

            [TestMethod]
            public void ObjectsNotEqualWhenIdsNotEqualTest()
            {
                var empBended1 = CreateEmployeeBended();
                BenededCode = "DSP1";
                var empBended2 = CreateEmployeeBended();
                Assert.AreNotEqual(empBended1.BenededCode, empBended2.BenededCode);
            }

            [TestMethod]
            public void ObjectsNotEqualWhenInputIsNullTest()
            {
                Assert.IsFalse(CreateEmployeeBended().Equals(null));
            }
            #endregion
        }
    }
}
