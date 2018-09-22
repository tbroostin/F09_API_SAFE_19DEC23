using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ellucian.Colleague.Domain.Base.Entities;

namespace Ellucian.Colleague.Domain.Base.Entities
{
    [TestClass]
    public class ImportantNumberCategoryTests
    {
        [TestClass]
        public class ImportantNumberCategoryConstructor
        {
            private string code;
            private string desc;
            private ImportantNumberCategory cat;

            [TestInitialize]
            public void Initialize()
            {
                code = "FOOD";
                desc = "Diners & Dives";
                cat = new ImportantNumberCategory(code, desc);
            }

            [TestMethod]
            public void CategoryCode()
            {
                Assert.AreEqual(cat.Code, code);
            }

            [TestMethod]
            public void CategoryDescription()
            {
                Assert.AreEqual(cat.Description, desc);
            }
        }
    }
}
