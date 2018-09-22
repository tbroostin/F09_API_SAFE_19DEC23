using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ellucian.Colleague.Domain.Student.Entities;

namespace Ellucian.Colleague.Domain.Student.Tests.Entities
{
    [TestClass]
    public class TermRegistrationDateTests
    {
        private string termCode;
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
        private TermRegistrationDate termRegDate;
        private List<DateTime?> censusDates;

        [TestInitialize]
        public void Initialize()
        {
            termCode = "2014/FA";
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
            termRegDate = new TermRegistrationDate(termCode, locationCode, regStart, regEnd, preregStart, preregEnd, addStart, addEnd, dropStart, dropEnd, dropOther, censusDates);
        }

        [TestCleanup]
        public void Cleanup()
        {
            termRegDate = null;
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void TermRegistrationDate_EmptyTerm_Exception()
        {
            termRegDate = new TermRegistrationDate(string.Empty, locationCode, regStart, regEnd, preregStart, preregEnd, addStart, addEnd, dropStart, dropEnd, dropOther, censusDates);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void TermRegistrationDate_NullTerm_Exception()
        {
            termRegDate = new TermRegistrationDate(null, locationCode, regStart, regEnd, preregStart, preregEnd, addStart, addEnd, dropStart, dropEnd, dropOther, censusDates);
        }

        [TestMethod]
        public void TermRegistrationDate_Constructor_WithDataSuccess()
        {
            Assert.AreEqual(termCode, termRegDate.TermId);
            Assert.AreEqual(locationCode, termRegDate.Location);
            Assert.AreEqual(regStart, termRegDate.RegistrationStartDate);
            Assert.AreEqual(regEnd, termRegDate.RegistrationEndDate);
            Assert.AreEqual(preregStart, termRegDate.PreRegistrationStartDate);
            Assert.AreEqual(preregEnd, termRegDate.PreRegistrationEndDate);
            Assert.AreEqual(addStart, termRegDate.AddStartDate);
            Assert.AreEqual(addEnd, termRegDate.AddEndDate);
            Assert.AreEqual(dropStart, termRegDate.DropStartDate);
            Assert.AreEqual(dropEnd, termRegDate.DropEndDate);
            Assert.AreEqual(dropOther, termRegDate.DropGradeRequiredDate);
        }

        [TestMethod]
        public void TermRegistrationDate_Constructor_WithNoDataSuccess()
        {
            var termRegDate2 = new TermRegistrationDate(termCode, null, null, null, null, null, null, null, null, null, null, null);
            Assert.AreEqual(termCode, termRegDate.TermId);
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
        }
    }
}
