// Copyright 2020 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Domain.Student.Entities.Portal;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace Ellucian.Colleague.Domain.Student.Tests.Entities.Portal
{
    [TestClass]
    public class PortalStudentPreferredCourseSectionUpdateResultTests
    {
        PortalStudentPreferredCourseSectionUpdateResult entity;

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void PortalStudentPreferredCourseSectionUpdateResult_Constructor_null_CourseSectionId()
        {
            entity = new PortalStudentPreferredCourseSectionUpdateResult(null, PortalStudentPreferredCourseSectionUpdateStatus.Ok);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void PortalStudentPreferredCourseSectionUpdateResult_Constructor_null_Message_status_Error()
        {
            entity = new PortalStudentPreferredCourseSectionUpdateResult("12345", PortalStudentPreferredCourseSectionUpdateStatus.Error);
        }

        [TestMethod]
        public void PortalStudentPreferredCourseSectionUpdateResult_Constructor_valid_Ok()
        {
            entity = new PortalStudentPreferredCourseSectionUpdateResult("12345", PortalStudentPreferredCourseSectionUpdateStatus.Ok);
            Assert.AreEqual("12345", entity.CourseSectionId);
            Assert.AreEqual(PortalStudentPreferredCourseSectionUpdateStatus.Ok, entity.Status);
            Assert.IsNull(entity.Message);
        }

        [TestMethod]
        public void PortalStudentPreferredCourseSectionUpdateResult_Constructor_valid_Error()
        {
            entity = new PortalStudentPreferredCourseSectionUpdateResult("12345", PortalStudentPreferredCourseSectionUpdateStatus.Error, "Could not add section 12345 at this time.");
            Assert.AreEqual("12345", entity.CourseSectionId);
            Assert.AreEqual(PortalStudentPreferredCourseSectionUpdateStatus.Error, entity.Status);
            Assert.AreEqual("Could not add section 12345 at this time.", entity.Message);
        }
    }
}
