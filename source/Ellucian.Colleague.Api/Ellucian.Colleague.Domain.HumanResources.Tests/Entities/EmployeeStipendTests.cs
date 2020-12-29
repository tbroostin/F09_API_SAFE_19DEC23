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
    public class EmployeeStipendTests
    {
        public string StipendDescription { get; set; }
        public decimal? StipendAmount { get; set; }

        EmployeeStipend EmployeeStipend;

        [TestClass]
        public class EmployeeStipendConstructorTests : EmployeeStipendTests
        {
            [TestInitialize]
            public void Initialize()
            {
                StipendDescription = "Stipend for Print Shop Assistant";
                StipendAmount = 1080.72m;

            }

            public EmployeeStipend CreateEmployeeStipend()
            {
                return new EmployeeStipend(StipendDescription, StipendAmount);
            }

            [TestMethod]
            public void ConstructorSetPropertiesTests()
            {
                EmployeeStipend = CreateEmployeeStipend();

                Assert.AreEqual(StipendDescription, EmployeeStipend.StipendDescription);
                Assert.AreEqual(StipendAmount, EmployeeStipend.StipendAmount);

            }

            [TestMethod]
            public void StipendAmountIsNullTest()
            {
                EmployeeStipend = CreateEmployeeStipend();
                EmployeeStipend.StipendAmount = null;
                Assert.IsNull(EmployeeStipend.StipendAmount);
            }

            [TestMethod]
            public void ObjectsNotEqualWhenInputIsNullTest()
            {
                Assert.IsFalse(CreateEmployeeStipend().Equals(null));
            }
        }
    }
}
