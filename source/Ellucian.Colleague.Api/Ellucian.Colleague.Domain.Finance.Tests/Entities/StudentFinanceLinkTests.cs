// Copyright 2016 Ellucian Company L.P. and its affiliates.
using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ellucian.Colleague.Domain.Finance.Entities;

namespace Ellucian.Colleague.Domain.Finance.Tests.Entities
{
    [TestClass]
    public class StudentFinanceLinkTests
    {
        static string title = "Ellucian University";
        static string url = "http://www.ellucian.edu";
        StudentFinanceLink link = new StudentFinanceLink(title, url);

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void StudentFinanceLink_Constructor_NullTitle()
        {
            StudentFinanceLink link = new StudentFinanceLink(null, url);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void StudentFinanceLink_Constructor_EmptyTitle()
        {
            StudentFinanceLink link = new StudentFinanceLink(string.Empty, url);
        }

        [TestMethod]
        public void StudentFinanceLink_Constructor_ValidTitle()
        {
            Assert.AreEqual(title, link.Title);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void StudentFinanceLink_Constructor_NullUrl()
        {
            StudentFinanceLink type = new StudentFinanceLink(title, null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void StudentFinanceLink_Constructor_EmptyUrl()
        {
            StudentFinanceLink type = new StudentFinanceLink(title, string.Empty);
        }

        [TestMethod]
        public void StudentFinanceLink_Constructor_ValidUrl()
        {
            Assert.AreEqual(url, link.Url);
        }
    }
}
