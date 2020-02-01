// Copyright 2012-2019 Ellucian Company L.P. and its affiliates.
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ellucian.Colleague.Domain.Student.Entities;
using Ellucian.Colleague.Domain.Student.Entities.DegreePlans;

namespace Ellucian.Colleague.Domain.Student.Tests.Entities.DegreePlans
{
    class PlannedCourseWarningTests
    {
        [TestClass]
        public class PlannedCourseWarningConstructor
        {
            [TestMethod]
            public void ReturnsValidPlannedCourseWarning()
            {
                Requisite req = new Requisite("PREREQ1", true, RequisiteCompletionOrder.Previous, false);
                PlannedCourseWarning warning = new PlannedCourseWarning(PlannedCourseWarningType.UnmetRequisite);
                warning.Requisite = req;
                Assert.AreEqual(PlannedCourseWarningType.UnmetRequisite, warning.Type);
                var warningReq = warning.Requisite;
                Assert.IsTrue(warningReq.IsRequired);
                Assert.AreEqual("PREREQ1", warningReq.RequirementCode);
                Assert.AreEqual(RequisiteCompletionOrder.Previous, warningReq.CompletionOrder);
            }

            [TestMethod]
            public void SetsWarningType()
            {
                PlannedCourseWarning warning = new PlannedCourseWarning(PlannedCourseWarningType.TimeConflict);
                warning.SectionId = "111";
                Assert.AreEqual(PlannedCourseWarningType.TimeConflict, warning.Type);
                Assert.AreEqual("111", warning.SectionId);
            }

            [TestMethod]
            public void ReturnsValidCoreqWarning()
            {
                Requisite req = new Requisite("1232", false);
                PlannedCourseWarning warning = new PlannedCourseWarning(PlannedCourseWarningType.UnmetRequisite);
                warning.Requisite = req;
                Assert.AreEqual(PlannedCourseWarningType.UnmetRequisite, warning.Type);
                var warningReq = warning.Requisite;
                Assert.IsFalse(warningReq.IsRequired);
                Assert.AreEqual("1232", warningReq.CorequisiteCourseId);
                Assert.AreEqual(RequisiteCompletionOrder.PreviousOrConcurrent, warningReq.CompletionOrder);
            }
        }
    }
}
