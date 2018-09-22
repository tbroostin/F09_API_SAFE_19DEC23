// Copyright 2012-2017 Ellucian Company L.P. and its affiliates.
using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ellucian.Colleague.Domain.Student.Entities;

namespace Ellucian.Colleague.Domain.Student.Tests.Entities
{
    [TestClass]
    public class BookTests
    {
        Book goodBook;

        [TestInitialize]
        public void Initialize()
        {
            
            goodBook = new Book("111", "123-456-7890", "Title", "Author", "Publisher", "2017", "5th edition", 
                true, 30.00m, 60.00m, "This is a comment", "External Comments", "this is alt1", "this is alt2", "this is alt3");
        }

        [TestMethod]
        public void allInfoGiven()
        {
            Assert.AreEqual("111", goodBook.Id);
            Assert.AreEqual("123-456-7890", goodBook.Isbn);
            Assert.AreEqual("Title", goodBook.Title);
            Assert.AreEqual("Author", goodBook.Author);
            Assert.AreEqual("Publisher", goodBook.Publisher);
            Assert.AreEqual("2017", goodBook.Copyright);
            Assert.AreEqual("5th edition", goodBook.Edition);
            Assert.AreEqual(true, goodBook.IsActive);
            Assert.AreEqual(30.00m, goodBook.PriceUsed);
            Assert.AreEqual(60.00m, goodBook.Price);
            Assert.AreEqual("This is a comment", goodBook.Comment);
            Assert.AreEqual("External Comments", goodBook.ExternalComments);
            Assert.AreEqual("this is alt1", goodBook.AlternateID1);
            Assert.AreEqual("this is alt2", goodBook.AlternateID2);
            Assert.AreEqual("this is alt3", goodBook.AlternateID3);
        }

        [TestMethod]
        public void BookInfoButNoIsbn()
        {
            Book newBook = new Book("111", null, "Title", "Author", "Publisher", "2017", "5th edition",
                true, 30.00m, 60.00m, "This is a comment", "External Comments", "this is alt1", "this is alt2", "this is alt3");
        }

        [TestMethod]
        public void NoBookInfoButIsbn()
        {
            Book newBook = new Book("111", "123-456-7890", null, null, null, null, "5th edition",
                true, 30.00m, 60.00m, "This is a comment", "External Comments", "this is alt1", "this is alt2", "this is alt3");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void NoTitleNoIsbn()
        {
            Book newBook = new Book("111", null, null, "Author", "Publisher", "2017", "5th edition",
                true, 30.00m, 60.00m, "This is a comment", "External Comments", "this is alt1", "this is alt2", "this is alt3");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void NoAuthorNoIsbn()
        {
            Book newBook = new Book("111", null, "Title", null, "Publisher", "2017", "5th edition",
                true, 30.00m, 60.00m, "This is a comment", "External Comments", "this is alt1", "this is alt2", "this is alt3");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void NoPublisherNoIsbn()
        {
            Book newBook = new Book("111", null, "Title", "Author", null, "2017", "5th edition",
                true, 30.00m, 60.00m, "This is a comment", "External Comments", "this is alt1", "this is alt2", "this is alt3");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void NoCopyrightNoIsbn()
        {
            Book newBook = new Book("111", null, "Title", "Author", "Publisher", null, "5th edition",
                true, 30.00m, 60.00m, "This is a comment", "External Comments", "this is alt1", "this is alt2", "this is alt3");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void InvalidHighPriceUsed()
        {
            Book newBook = new Book("111", "123-456-7890", "Title", "Author", "Publisher", "2017", "5th edition",
                true, 10000.00m, 60.00m, "This is a comment", "External Comments", "this is alt1", "this is alt2", "this is alt3");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void InvalidLowPriceUsed()
        {
            Book newBook = new Book("111", "123-456-7890", "Title", "Author", "Publisher", "2017", "5th edition",
                true, -0.50m, 60.00m, "This is a comment", "External Comments", "this is alt1", "this is alt2", "this is alt3");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void InvalidHighPriceNew()
        {
            Book newBook = new Book("111", "123-456-7890", "Title", "Author", "Publisher", "2017", "5th edition",
                true, 30.00m, 10000.00m, "This is a comment", "External Comments", "this is alt1", "this is alt2", "this is alt3");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void InvalidLowPriceNew()
        {
            Book newBook = new Book("111", "123-456-7890", "Title", "Author", "Publisher", "2017", "5th edition",
                true, 30.00m, -0.50m, "This is a comment", "External Comments", "this is alt1", "this is alt2", "this is alt3");
        }
        
        [TestMethod]
        public void NullPriceUsed()
        {
            Book newBook = new Book("111", "123-456-7890", "Title", "Author", "Publisher", "2017", "5th edition",
                true, null, 60.00m, "This is a comment", "External Comments", "this is alt1", "this is alt2", "this is alt3");
            Assert.AreEqual(null, newBook.PriceUsed);
        }

        [TestMethod]
        public void NullPriceNew()
        {
            Book newBook = new Book("111", "123-456-7890", "Title", "Author", "Publisher", "2017", "5th edition",
                true, 30.00m, null, "This is a comment", "External Comments", "this is alt1", "this is alt2", "this is alt3");
            Assert.AreEqual(null, newBook.Price);
        }
    }
}
