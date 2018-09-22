using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ellucian.Colleague.Domain.Student.Entities.Requirements;
using Ellucian.Colleague.Domain.Student.Entities;

namespace Ellucian.Colleague.Domain.Student.Tests.Entities
{
    [TestClass]
    public class StudentProgramTests
    {
        [TestClass]
        public class StudentProgramConstructor
        {
            ProgramRequirements programRequirements;
            StudentProgram studentProgram;
            string program;
            string catalog;

            [TestInitialize]
            public void Initialize()
            {
                catalog = "2012";
                program = "Any Program";
                programRequirements = new ProgramRequirements(program, catalog);
                studentProgram = new StudentProgram("15", catalog, program);
            }

            [TestMethod]
            public void StudentProgram_PersonId()
            {
                Assert.AreEqual("15", studentProgram.StudentId);
            }

            //[TestMethod]
            //public void StudentProgram_ProgramRequirements()
            //{
            //    Assert.AreEqual(programRequirements, studentProgram.ProgramRequirements);
            //}


            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void ExceptionIfPersonIdNull()
            {
                new StudentProgram(null, program, catalog);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void ExceptionIfProgramCodeNull()
            {
                new StudentProgram("15", null, catalog);
            }
        }

        [TestClass]
        public class AddAddlRequirement
        {
            ProgramRequirements programRequirements;
            string program;
            string catalog;
            StudentProgram studentProgram;
            Requirement requirement;
            string gradeScheme;
            AdditionalRequirement addlReq;

            List<RequirementType> requirementTypes = new TestRequirementRepository().RequirementTypes;


            [TestInitialize]
            public void Initialize()
            {
                catalog = "2012";
                program = "Any Program";
                programRequirements = new ProgramRequirements(program, catalog);
                studentProgram = new StudentProgram("15", program, catalog);

                gradeScheme = "UG";
                requirement = new Requirement("13", "MY.REQ", "My Requirement", gradeScheme, null);
                addlReq = new AdditionalRequirement("Award", requirement.Id, AwardType.Ccd, "Certificate for being awesome");

                studentProgram.AddAddlRequirement(addlReq);
            }

            [TestMethod]
            public void AdditionalRequirements()
            {
                Assert.AreEqual(addlReq, studentProgram.AdditionalRequirements.ElementAt(0));
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void ExceptionIfNull()
            {
                addlReq = null;
                studentProgram.AddAddlRequirement(addlReq);
            }
        }

        [TestClass]
        public class AddMajors
        {
            StudentProgram studentProgram;
            StudentMajors studentMajor;
            string code;
            string name;
            string catalog;
            string program;
            DateTime? startDate;
            DateTime? endDate;

            [TestInitialize]
            public void Initialize()
            {
                catalog = "2012";
                program = "MATH.BA";
                studentProgram = new StudentProgram("15", program, catalog);
                code = "MATH";
                name = "Mathematics";
                startDate = DateTime.Today.AddDays(-30);
                endDate = DateTime.Today;
                studentMajor = new StudentMajors(code, name, startDate, endDate);
                studentProgram.AddMajors(studentMajor);
            }

            [TestMethod]
            public void StudentMajors()
            {
                Assert.AreEqual(studentMajor, studentProgram.StudentProgramMajors.ElementAt(0));
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void ExceptionIfNull()
            {
                studentMajor = null;
                studentProgram.AddMajors(studentMajor);
            }
        }

        [TestClass]
        public class AddMinors
        {
            StudentProgram studentProgram;
            StudentMinors studentMinor;
            string code;
            string name;
            string catalog;
            string program;
            DateTime? startDate;
            DateTime? endDate;

            [TestInitialize]
            public void Initialize()
            {
                catalog = "2012";
                program = "MATH.BA";
                studentProgram = new StudentProgram("15", program, catalog);
                code = "MATH";
                name = "Mathematics";
                startDate = DateTime.Today.AddDays(-30);
                endDate = DateTime.Today;
                studentMinor = new StudentMinors(code, name, startDate, endDate);
                studentProgram.AddMinors(studentMinor);
            }

            [TestMethod]
            public void StudentMinors()
            {
                Assert.AreEqual(studentMinor, studentProgram.StudentProgramMinors.ElementAt(0));
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void ExceptionIfNull()
            {
                studentMinor = null;
                studentProgram.AddMinors(studentMinor);
            }
        }
    }
}
