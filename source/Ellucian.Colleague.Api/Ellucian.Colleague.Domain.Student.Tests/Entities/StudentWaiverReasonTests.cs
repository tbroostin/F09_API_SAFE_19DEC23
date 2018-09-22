// Copyright 2015 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Domain.Student.Entities;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Ellucian.Colleague.Domain.Student.Tests.Entities
{
    [TestClass]
    public class StudentWaiverReasonTests
    {
        [TestMethod]
        public void WaiverReason_Constructor()
        {
            string code = "OTHER";
            string description = "Other Reason";
            var waiverReason = new StudentWaiverReason(code, description);
            Assert.AreEqual(code, waiverReason.Code);
            Assert.AreEqual(description, waiverReason.Description);
        }
    }
}
