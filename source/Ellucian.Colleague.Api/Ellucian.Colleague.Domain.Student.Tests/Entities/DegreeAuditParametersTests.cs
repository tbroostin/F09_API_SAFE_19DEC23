// Copyright 2017 Ellucian Company L.P. and its affiliates.
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ellucian.Colleague.Domain.Student.Entities;
using Ellucian.Colleague.Domain.Student.Entities.Requirements;

namespace Ellucian.Colleague.Domain.Student.Tests.Entities
{
    [TestClass]
    public class DegreeAuditParametersTests
    {
        DegreeAuditParameters daParams;
        
        [TestInitialize]
        public void Initialize()
        {
            daParams = new DegreeAuditParameters(ExtraCourses.Display, true, true);
        }

        [TestMethod]
        public void DegreeAuditParameters_ExtraCourseHandling()
        {
            Assert.AreEqual(ExtraCourses.Display, daParams.ExtraCourseHandling);
        }

        [TestMethod]
        public void DegreeAuditParameters_ModifiedSort()
        {
            Assert.IsTrue(daParams.ModifiedDefaultSort);
        }

        [TestMethod]
        public void DegreeAuditParameters_UseLowGrade()
        {
            Assert.IsTrue(daParams.UseLowGrade);
        }

        [TestMethod]
        public void DegreeAuditParameters_DefaultParameters()
        {
            var daParams2 = new DegreeAuditParameters(ExtraCourses.Display);
            Assert.IsFalse(daParams2.UseLowGrade);
            Assert.IsFalse(daParams2.ModifiedDefaultSort);
        }
    }
}
