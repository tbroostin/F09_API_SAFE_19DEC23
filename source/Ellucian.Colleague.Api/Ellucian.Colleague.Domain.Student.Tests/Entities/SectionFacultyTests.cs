// Copyright 2015 Ellucian Company L.P. and its affiliates.
using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ellucian.Colleague.Domain.Student.Entities;

namespace Ellucian.Colleague.Domain.Student.Tests.Entities
{
    [TestClass]
    public class SectionFacultyTests
    {
        string id = "123";
        string sectionId = "45";
        string facultyId = "0123456";
        string instMethod = "LEC";
        DateTime startDate = DateTime.Today;
        DateTime endDate = DateTime.Today.AddDays(120);
        decimal respPercent = 100m;
        SectionFaculty result;

        [TestInitialize]
        public void Initialize()
        {
            result = new SectionFaculty(id, sectionId, facultyId, instMethod, startDate, endDate, respPercent);
        }

        [TestMethod]
        public void SectionFaculty_Constructor_Id()
        {
            Assert.AreEqual(id, result.Id);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void SectionFaculty_Constructor_IdChange()
        {
            result.Id = "5";
        }

        [TestMethod]
        public void SectionFaculty_Constructor_SectionId()
        {
            Assert.AreEqual(sectionId, result.SectionId);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void SectionFaculty_Constructor_SectionIdChange()
        {
            result.SectionId = "5";
        }

        [TestMethod]
        public void SectionFaculty_Constructor_FacultyId()
        {
            Assert.AreEqual(facultyId, result.FacultyId);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void SectionFaculty_Constructor_FacultyIdNull()
        {
            result = new SectionFaculty(id, sectionId, null, instMethod, startDate, endDate, respPercent);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void SectionFaculty_Constructor_FacultyIdEmpty()
        {
            result = new SectionFaculty(id, sectionId, string.Empty, instMethod, startDate, endDate, respPercent);
        }

        [TestMethod]
        public void SectionFaculty_Constructor_InstructionalMethodCode()
        {
            Assert.AreEqual(instMethod, result.InstructionalMethodCode);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void SectionFaculty_Constructor_InstructionalMethodCodeNull()
        {
            result = new SectionFaculty(id, sectionId, facultyId, null, startDate, endDate, respPercent);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void SectionFaculty_Constructor_InstructionalMethodCodeEmpty()
        {
            result = new SectionFaculty(id, sectionId, facultyId, string.Empty, startDate, endDate, respPercent);
        }

        [TestMethod]
        public void SectionFaculty_Constructor_StartDate()
        {
            Assert.AreEqual(startDate, result.StartDate);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void SectionFaculty_Constructor_StartDateDefault()
        {
            result = new SectionFaculty(id, sectionId, facultyId, instMethod, default(DateTime), endDate, respPercent);
        }

        [TestMethod]
        public void SectionFaculty_Constructor_EndDate()
        {
            Assert.AreEqual(endDate, result.EndDate);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void SectionFaculty_Constructor_EndDateDefault()
        {
            result = new SectionFaculty(id, sectionId, facultyId, instMethod, startDate, default(DateTime), respPercent);
        }

        [TestMethod]
        public void SectionFaculty_Constructor_ResponsibilityPercentage()
        {
            Assert.AreEqual(respPercent, result.ResponsibilityPercentage);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void SectionFaculty_Constructor_ResponsibilityPercentageNegative()
        {
            result = new SectionFaculty(id, sectionId, facultyId, instMethod, startDate, endDate, -1m);
        }

        [TestMethod]
        public void SectionFaculty_ResponsibilityPercentage_Set_Positive()
        {
            result = new SectionFaculty(id, sectionId, facultyId, instMethod, startDate, endDate, 1m);

            int newValue = 2;
            result.ResponsibilityPercentage = newValue;

            Assert.AreEqual(newValue, result.ResponsibilityPercentage);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void SectionFaculty_ResponsibilityPercentage_Set_Negative()
        {
            result = new SectionFaculty(id, sectionId, facultyId, instMethod, startDate, endDate, 1m);

            result.ResponsibilityPercentage = -1;
        }

        [TestMethod]
        public void SectionFaculty_Id_Set()
        {
            result = new SectionFaculty(null, null, facultyId, instMethod, startDate, endDate, respPercent);
            result.Id = id;
            Assert.AreEqual(id, result.Id);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void SectionFaculty_Id_SetException()
        {
            result = new SectionFaculty(null, null, facultyId, instMethod, startDate, endDate, respPercent);
            result.Id = id;
            result.Id = "";
        }

        [TestMethod]
        public void SectionFaculty_SectionId_Set()
        {
            result = new SectionFaculty(null, null, facultyId, instMethod, startDate, endDate, respPercent);
            result.SectionId = sectionId;
            Assert.AreEqual(sectionId, result.SectionId);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void SectionFaculty_SectionId_SetException()
        {
            result = new SectionFaculty(null, null, facultyId, instMethod, startDate, endDate, respPercent);
            result.SectionId = sectionId;
            result.SectionId = "";
        }
    }
}
