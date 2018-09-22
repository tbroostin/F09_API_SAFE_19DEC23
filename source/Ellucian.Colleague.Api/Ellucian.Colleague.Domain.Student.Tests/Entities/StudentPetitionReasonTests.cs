// Copyright 2015 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using Ellucian.Colleague.Domain.Student.Entities;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Ellucian.Colleague.Domain.Student.Tests.Entities
{
    [TestClass]
    public class StudentPetitionReasonTests
    {
        [TestMethod]
        public void StudentPetitionReason_Constructor()
        {
            string code = "ICJI";
            string description = "I can handle it";
            var sudentPetitionReason = new StudentPetitionReason(code, description);
            Assert.AreEqual(code, sudentPetitionReason.Code);
            Assert.AreEqual(description, sudentPetitionReason.Description);
        }
    }
}
