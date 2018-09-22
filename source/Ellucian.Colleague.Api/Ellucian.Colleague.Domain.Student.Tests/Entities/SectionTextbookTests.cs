// Copyright 2017 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Domain.Student.Entities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Domain.Student.Tests.Entities
{
    [TestClass]
    public class SectionTextbookTests
    {
        public string bookId;
        public string sectionId;
        public string requiredCode;
        public Book goodBook;

        [TestInitialize]
        public void Initialize()
        {
            bookId = "123";
            sectionId = "SEC1";
            requiredCode = "R";
            goodBook = new Book("book1", "isbn", "title", "author", "publisher", "copyright", "edition",
                true, 10m, 20m, "comment", "external comments", "altId1", "altId2", "altId3");
        }

        [ExpectedException(typeof(ArgumentNullException))]
        [TestMethod]
        public void Null_Book()
        {
            var sectionTextbook = new SectionTextbook(null, sectionId, requiredCode, SectionBookAction.Add);
        }

        [ExpectedException(typeof(ArgumentNullException))]
        [TestMethod]
        public void Empty_SectionId()
        {
            var sectionTextbook = new SectionTextbook(goodBook, string.Empty, requiredCode, SectionBookAction.Add);
        }

        [ExpectedException(typeof(ArgumentNullException))]
        [TestMethod]
        public void Null_SectionId()
        {
            var sectionTextbook = new SectionTextbook(goodBook, null, requiredCode, SectionBookAction.Add);
        }

        [ExpectedException(typeof(ArgumentNullException))]
        [TestMethod]
        public void Null_Isbn_Null_Title()
        {
            var badBook = new Book("book1", null, null, "author", "publisher", "copyright", "edition",
                true, 10m, 20m, "comment", "external comments", "altId1", "altId2", "altId3");
            var sectionTextbook = new SectionTextbook(badBook, sectionId, requiredCode, SectionBookAction.Add);
        }

        [ExpectedException(typeof(ArgumentNullException))]
        [TestMethod]
        public void Null_Isbn_Empty_Title()
        {
            var badBook = new Book("book1", null, string.Empty, "author", "publisher", "copyright", "edition",
                true, 10m, 20m, "comment", "external comments", "altId1", "altId2", "altId3");
            var sectionTextbook = new SectionTextbook(badBook, sectionId, requiredCode, SectionBookAction.Add);
        }

        [ExpectedException(typeof(ArgumentNullException))]
        [TestMethod]
        public void Null_Isbn_Null_Author()
        {
            var badBook = new Book("book1", null, "title", null, "publisher", "copyright", "edition",
                true, 10m, 20m, "comment", "external comments", "altId1", "altId2", "altId3");
            var sectionTextbook = new SectionTextbook(badBook, sectionId, requiredCode, SectionBookAction.Add);
        }

        [ExpectedException(typeof(ArgumentNullException))]
        [TestMethod]
        public void Null_Isbn_Empty_Author()
        {
            var badBook = new Book("book1", null, "title", string.Empty, "publisher", "copyright", "edition",
                true, 10m, 20m, "comment", "external comments", "altId1", "altId2", "altId3");
            var sectionTextbook = new SectionTextbook(badBook, sectionId, requiredCode, SectionBookAction.Add);
        }

        [ExpectedException(typeof(ArgumentNullException))]
        [TestMethod]
        public void Null_Isbn_Null_Publisher()
        {
            var badBook = new Book("book1", null, "title", "author", null, "copyright", "edition",
                true, 10m, 20m, "comment", "external comments", "altId1", "altId2", "altId3");
            var sectionTextbook = new SectionTextbook(badBook, sectionId, requiredCode, SectionBookAction.Add);
        }

        [ExpectedException(typeof(ArgumentNullException))]
        [TestMethod]
        public void Null_Isbn_Empty_Publisher()
        {
            var badBook = new Book("book1", null, "title", "author", string.Empty, "copyright", "edition",
                true, 10m, 20m, "comment", "external comments", "altId1", "altId2", "altId3");
            var sectionTextbook = new SectionTextbook(badBook, sectionId, requiredCode, SectionBookAction.Add);
        }

        [ExpectedException(typeof(ArgumentNullException))]
        [TestMethod]
        public void Null_Isbn_Null_Copyright()
        {
            var badBook = new Book("book1", null, "title", "author", "publisher", null, "edition",
                true, 10m, 20m, "comment", "external comments", "altId1", "altId2", "altId3");
            var sectionTextbook = new SectionTextbook(badBook, sectionId, requiredCode, SectionBookAction.Add);
        }

        [ExpectedException(typeof(ArgumentNullException))]
        [TestMethod]
        public void Null_Isbn_Empty_Copyright()
        {
            var badBook = new Book("book1", null, "title", "author", "publisher", string.Empty, "edition",
                true, 10m, 20m, "comment", "external comments", "altId1", "altId2", "altId3");
            var sectionTextbook = new SectionTextbook(badBook, sectionId, requiredCode, SectionBookAction.Add);
        }

        [TestMethod]
        public void Null_Isbn_GoodInfo()
        {
            var nullIsbnBook = new Book("book1", null, "title", "author", "publisher", "copyright", "edition",
                true, 10m, 20m, "comment", "external comments", "altId1", "altId2", "altId3");
            var sectionTextbook = new SectionTextbook(nullIsbnBook, sectionId, requiredCode, SectionBookAction.Add);
            Assert.IsNotNull(sectionTextbook);
            Assert.IsNotNull(sectionTextbook.Textbook);
            Assert.IsInstanceOfType(sectionTextbook, typeof(SectionTextbook));
            Assert.IsInstanceOfType(sectionTextbook.Textbook, typeof(Book));
        }

        [TestMethod]
        public void Empty_Isbn_GoodInfo()
        {
            var emptyIsbnBook = new Book("book1", string.Empty, "title", "author", "publisher", "copyright", "edition",
                true, 10m, 20m, "comment", "external comments", "altId1", "altId2", "altId3");
            var sectionTextbook = new SectionTextbook(emptyIsbnBook, sectionId, requiredCode, SectionBookAction.Add);
            Assert.IsNotNull(sectionTextbook);
            Assert.IsNotNull(sectionTextbook.Textbook);
            Assert.IsInstanceOfType(sectionTextbook, typeof(SectionTextbook));
            Assert.IsInstanceOfType(sectionTextbook.Textbook, typeof(Book));
        }

        [TestMethod]
        public void GoodInfo()
        {
            var sectionTextbook = new SectionTextbook(goodBook, sectionId, requiredCode, SectionBookAction.Add);
            Assert.IsNotNull(sectionTextbook);
            Assert.IsNotNull(sectionTextbook.Textbook);
            Assert.IsInstanceOfType(sectionTextbook, typeof(SectionTextbook));
            Assert.IsInstanceOfType(sectionTextbook.Textbook, typeof(Book));
        }

        [TestMethod]
        public void SectionId()
        {
            var sectionTextbook = new SectionTextbook(goodBook, sectionId, requiredCode, SectionBookAction.Add);
            Assert.IsNotNull(sectionTextbook);
            Assert.IsInstanceOfType(sectionTextbook, typeof(SectionTextbook));
            Assert.AreEqual("SEC1", sectionTextbook.SectionId);
        }

        [TestMethod]
        public void RequirementStatus()
        {
            var sectionTextbook = new SectionTextbook(goodBook, sectionId, requiredCode, SectionBookAction.Add);
            Assert.IsNotNull(sectionTextbook);
            Assert.IsInstanceOfType(sectionTextbook, typeof(SectionTextbook));
            Assert.AreEqual("R", sectionTextbook.RequirementStatusCode);
        }

        [TestMethod]
        public void Add_Action()
        {
            var sectionTextbook = new SectionTextbook(goodBook, sectionId, requiredCode, SectionBookAction.Add);
            Assert.IsNotNull(sectionTextbook);
            Assert.IsInstanceOfType(sectionTextbook, typeof(SectionTextbook));
            Assert.AreEqual(SectionBookAction.Add, sectionTextbook.Action);
        }

        [TestMethod]
        public void Update_Action()
        {
            var sectionTextbook = new SectionTextbook(goodBook, sectionId, requiredCode, SectionBookAction.Update);
            Assert.IsNotNull(sectionTextbook);
            Assert.IsInstanceOfType(sectionTextbook, typeof(SectionTextbook));
            Assert.AreEqual(SectionBookAction.Update, sectionTextbook.Action);
        }

        [TestMethod]
        public void Remove_Action()
        {
            var sectionTextbook = new SectionTextbook(goodBook, sectionId, requiredCode, SectionBookAction.Remove);
            Assert.IsNotNull(sectionTextbook);
            Assert.IsInstanceOfType(sectionTextbook, typeof(SectionTextbook));
            Assert.AreEqual(SectionBookAction.Remove, sectionTextbook.Action);
        }
    }
}
