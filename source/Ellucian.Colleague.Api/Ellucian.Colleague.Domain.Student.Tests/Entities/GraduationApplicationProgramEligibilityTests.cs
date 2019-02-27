// Copyright 2018 Ellucian Company L.P. and its affiliates.
using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ellucian.Colleague.Domain.Student.Entities;
using System.Linq;

namespace Ellucian.Colleague.Domain.Student.Tests.Entities
{
    [TestClass]
    public class GraduationApplicationProgramEligibilityTests
    {
        [TestClass]
        public class GraduationApplicationProgramEligibility_Constructor
        {
            private string studentId;
            private string programCode;
            private bool isEligible;
            private GraduationApplicationProgramEligibility programEligibility;

            [TestInitialize]
            public void Initialize()
            {
                studentId = "StudentId";
                programCode = "ProgramCode";
                isEligible = true;
            }

            [TestMethod]
            public void GraduationApplicationProgramEligibility_PropertiesUpdated()
            {
                programEligibility = new GraduationApplicationProgramEligibility(studentId, programCode, isEligible);
                Assert.AreEqual(studentId, programEligibility.StudentId);
                Assert.AreEqual(programCode, programEligibility.ProgramCode);
                Assert.IsTrue(programEligibility.IsEligible);
                Assert.AreEqual(0, programEligibility.IneligibleMessages.Count());
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void GraduationApplicationProgramEligibility_EmptyStudentId()
            {
                programEligibility = new GraduationApplicationProgramEligibility(string.Empty, programCode, isEligible);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void GraduationApplicationProgramEligibility_NullStudentId()
            {
                programEligibility = new GraduationApplicationProgramEligibility(null, programCode, isEligible);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void GraduationApplicationProgramEligibility_NullProgramCode()
            {
                programEligibility = new GraduationApplicationProgramEligibility(studentId, null, isEligible);
            }


            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void GraduationApplicationProgramEligibility_EmptyProgramCode()
            {
                programEligibility = new GraduationApplicationProgramEligibility(studentId, string.Empty, isEligible);
            }

        }

        [TestClass]
        public class GraduationApplicationProgramEligibility_AddIneligibleMessage
        {
            private string programCode;
            private bool isEligible;
            private GraduationApplicationProgramEligibility programEligibility;

            [TestInitialize]
            public void Initialize()
            {
                programCode = "ProgramCode";
                isEligible = false;
                programEligibility = new GraduationApplicationProgramEligibility("studentId", programCode, isEligible);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void GraduationApplicationProgramEligibility_AddIneligibleMessage_NullMessage()
            {
                programEligibility.AddIneligibleMessage(null);
            }

            [TestMethod]
            public void GraduationApplicationProgramEligibility_AddIneligibleMessage_AddsMessage()
            {
                programEligibility.AddIneligibleMessage("This is message 1");
                programEligibility.AddIneligibleMessage("This is message 2");
                Assert.AreEqual(2, programEligibility.IneligibleMessages.Count());
                Assert.AreEqual("This is message 1", programEligibility.IneligibleMessages.ElementAt(0));
                Assert.AreEqual("This is message 2", programEligibility.IneligibleMessages.ElementAt(1));
            }
        }
    }
}
