using Ellucian.Colleague.Domain.ColleagueFinance.Entities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Domain.ColleagueFinance.Tests.Entities
{
    [TestClass]
    public class ShipToCodeTests
    {
        [TestMethod]
        public void Constructor()
        {
            var code = "code";
            var description = "description";
            var shipToCode = new ShipToCode(code, description);
            Assert.AreEqual(code, shipToCode.Code, "Code should be the same.");
            Assert.AreEqual(description, shipToCode.Description, "Description should be the same.");
        }
    }
}
