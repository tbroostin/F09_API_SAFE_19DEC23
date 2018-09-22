// Copyright 2017 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Domain.Student.Entities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace Ellucian.Colleague.Domain.Student.Tests.Entities
{
    [TestClass]
    public class SectionBookTests
    {
        public string bookId;
        public string requiredStatusCode;
        public string optionalStatusCode;

        [TestInitialize]
        public void Initialize()
        {
            bookId = "123";
            requiredStatusCode = "R";
            optionalStatusCode = "O";
        }

        [ExpectedException(typeof(ArgumentNullException))]
        [TestMethod]
        public void Null_Id()
        {
            var sectionBook = new SectionBook(null, requiredStatusCode, true);
        }

        [ExpectedException(typeof(ArgumentNullException))]
        [TestMethod]
        public void Empty_Id()
        {
            var sectionBook = new SectionBook(string.Empty, requiredStatusCode, true);
        }

        [TestMethod]
        public void BookId()
        {
            var sectionBook = new SectionBook(bookId, requiredStatusCode, true);
            Assert.AreEqual(bookId, sectionBook.BookId);
        }

        [TestMethod]
        public void NotRequired()
        {
            var sectionBook = new SectionBook(bookId, optionalStatusCode, false);
            Assert.AreEqual("O", sectionBook.RequirementStatusCode);
            Assert.IsFalse(sectionBook.IsRequired);
        }

        [TestMethod]
        public void Required()
        {
            var sectionBook = new SectionBook(bookId, requiredStatusCode, true);
            Assert.AreEqual("R", sectionBook.RequirementStatusCode);
            Assert.IsTrue(sectionBook.IsRequired);
        }
    }
}
