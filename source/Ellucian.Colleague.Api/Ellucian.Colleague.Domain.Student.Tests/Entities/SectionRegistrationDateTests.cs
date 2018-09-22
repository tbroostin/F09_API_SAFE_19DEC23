using System;
using System.Text;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ellucian.Colleague.Domain.Student.Entities;

namespace Ellucian.Colleague.Domain.Student.Tests.Entities
{
    /// <summary>
    /// Summary description for SectionRegistrationDateTests
    /// </summary>
    [TestClass]
    public class SectionRegistrationDateTests
    {
        private string sectionId;
        private string locationCode;
        private DateTime? preregStart;
        private DateTime? preregEnd;
        private DateTime? regStart;
        private DateTime? regEnd;
        private DateTime? addStart;
        private DateTime? addEnd;
        private DateTime? dropStart;
        private DateTime? dropEnd;
        private DateTime? dropOther;
        private SectionRegistrationDate sectionRegDate;
        private List<DateTime?> censusDates;

        [TestInitialize]
        public void Initialize()
        {
            sectionId = "Section1";
            locationCode = "Location1";
            preregStart = DateTime.Today.AddDays(1);
            preregEnd = DateTime.Today.AddDays(2);
            regStart = DateTime.Today.AddDays(3);
            regEnd = DateTime.Today.AddDays(4);
            addStart = DateTime.Today.AddDays(5);
            addEnd = DateTime.Today.AddDays(6);
            dropStart = DateTime.Today.AddDays(7);
            dropEnd = DateTime.Today.AddDays(8);
            dropOther = DateTime.Today.AddDays(9);
            censusDates = null;
            sectionRegDate = new SectionRegistrationDate(sectionId, locationCode, regStart, regEnd, preregStart, preregEnd, addStart, addEnd, dropStart, dropEnd, dropOther, censusDates);
        }

        [TestCleanup]
        public void Cleanup()
        {
            sectionRegDate = null;
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void SectionRegistrationDate_EmptyTerm_Exception()
        {
            sectionRegDate = new SectionRegistrationDate(string.Empty, locationCode, regStart, regEnd, preregStart, preregEnd, addStart, addEnd, dropStart, dropEnd, dropOther, censusDates);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void SectionRegistrationDate_NullTerm_Exception()
        {
            sectionRegDate = new SectionRegistrationDate(null, locationCode, regStart, regEnd, preregStart, preregEnd, addStart, addEnd, dropStart, dropEnd, dropOther, censusDates);
        }

        [TestMethod]
        public void SectionRegistrationDate_Constructor_WithDataSuccess()
        {
            Assert.AreEqual(sectionId, sectionRegDate.SectionId);
            Assert.AreEqual(locationCode, sectionRegDate.Location);
            Assert.AreEqual(regStart, sectionRegDate.RegistrationStartDate);
            Assert.AreEqual(regEnd, sectionRegDate.RegistrationEndDate);
            Assert.AreEqual(preregStart, sectionRegDate.PreRegistrationStartDate);
            Assert.AreEqual(preregEnd, sectionRegDate.PreRegistrationEndDate);
            Assert.AreEqual(addStart, sectionRegDate.AddStartDate);
            Assert.AreEqual(addEnd, sectionRegDate.AddEndDate);
            Assert.AreEqual(dropStart, sectionRegDate.DropStartDate);
            Assert.AreEqual(dropEnd, sectionRegDate.DropEndDate);
            Assert.AreEqual(dropOther, sectionRegDate.DropGradeRequiredDate);
        }

        [TestMethod]
        public void TermRegistrationDate_Constructor_WithNoDataSuccess()
        {
            var termRegDate2 = new TermRegistrationDate(sectionId, null, null, null, null, null, null, null, null, null, null, null);
            Assert.AreEqual(sectionId, sectionRegDate.SectionId);
            Assert.IsNull(termRegDate2.Location);
            Assert.IsNull(termRegDate2.RegistrationStartDate);
            Assert.IsNull(termRegDate2.RegistrationEndDate);
            Assert.IsNull(termRegDate2.PreRegistrationStartDate);
            Assert.IsNull(termRegDate2.PreRegistrationEndDate);
            Assert.IsNull(termRegDate2.AddStartDate);
            Assert.IsNull(termRegDate2.AddEndDate);
            Assert.IsNull(termRegDate2.DropStartDate);
            Assert.IsNull(termRegDate2.DropEndDate);
            Assert.IsNull(termRegDate2.DropGradeRequiredDate);
            Assert.IsNull(termRegDate2.CensusDates);
        }
    }
}
