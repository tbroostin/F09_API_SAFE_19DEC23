using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ellucian.Colleague.Domain.HumanResources.Entities;

namespace Ellucian.Colleague.Domain.HumanResources.Tests.Entities
{
    [TestClass]
    public class DeductionTypeTests
    {
        [TestClass]
        public class DeductionTypeConstructor
        {
            private string guid;
            private string code;
            private string desc;
            private DeductionType deductType;

            [TestInitialize]
            public void Initialize()
            {
                guid = Guid.NewGuid().ToString();
                code = "CODE";
                desc = "Description";
                deductType = new DeductionType(guid, code, desc);
            }

            [TestMethod]
            public void TypeCode()
            {
                Assert.AreEqual(deductType.Code, code);
            }

            [TestMethod]
            public void TypeDescription()
            {
                Assert.AreEqual(deductType.Description, desc);
            }
        }
    }
}
