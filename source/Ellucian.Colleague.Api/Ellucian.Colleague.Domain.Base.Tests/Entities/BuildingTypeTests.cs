using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ellucian.Colleague.Domain.Base.Entities;

namespace Ellucian.Colleague.Domain.Base.Tests.Entities
{
    [TestClass]
    public class BuildingTypeTests
    {
        [TestClass]
        public class BuildingTypeConstructor
        {
            private string code;
            private string desc;
            private BuildingType bldgType;

            [TestInitialize]
            public void Initialize()
            {
                code = "ADMIN";
                desc = "Administrative";
                bldgType = new BuildingType(code, desc);
            }

            [TestMethod]
            public void TypeCode()
            {
                Assert.AreEqual(bldgType.Code, code);
            }

            [TestMethod]
            public void TypeDescription()
            {
                Assert.AreEqual(bldgType.Description, desc);
            }
        }
    }
}
