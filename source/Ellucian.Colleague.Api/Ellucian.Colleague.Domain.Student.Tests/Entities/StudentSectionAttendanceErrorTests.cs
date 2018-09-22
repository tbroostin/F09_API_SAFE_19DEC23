// Copyright 2018 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using Ellucian.Colleague.Domain.Student.Entities;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Ellucian.Colleague.Domain.Student.Tests.Entities
{
    [TestClass]
    public class StudentSectionAttendanceErrorTests
    {
        [TestClass]
        public class StudentSectionAttendanceError_Constructor
        {
            private string studentCourseSecId;
            private string errorMessage;
            private StudentSectionAttendanceError studentSectionAttendanceError;

            [TestInitialize]
            public void Initialize()
            {
                studentCourseSecId = "1234";
                errorMessage = "Item locked.";

            }

            [TestCleanup]
            public void CleanUp()
            {
                studentSectionAttendanceError = null;
            }


            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void StudentAttendance_ThrowArgumentNullException_StudentCourseSecIdNull()
            {
                studentSectionAttendanceError = new StudentSectionAttendanceError(null);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void StudentAttendance_ThrowArgumentNullException_StudentCourseSecIdEmpty()
            {
                studentSectionAttendanceError = new StudentSectionAttendanceError(string.Empty);
            }

            [TestMethod]
            public void StudentAttendance_ValidProperties()
            {
                studentSectionAttendanceError = new StudentSectionAttendanceError(studentCourseSecId);
                studentSectionAttendanceError.ErrorMessage = errorMessage;
                Assert.AreEqual(studentCourseSecId, studentSectionAttendanceError.StudentCourseSectionId);
                Assert.AreEqual(errorMessage, studentSectionAttendanceError.ErrorMessage);
            }
        }

        
    }
}