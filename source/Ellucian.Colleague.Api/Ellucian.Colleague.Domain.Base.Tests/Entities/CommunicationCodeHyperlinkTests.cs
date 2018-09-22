/*Copyright 2014-2015 Ellucian Company L.P. and its affiliates.*/
using Ellucian.Colleague.Domain.Base.Entities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Domain.Base.Tests.Entities
{
    [TestClass]
    public class CommunicationCodeHyperlinkTests
    {
        public string url;
        public string title;

        public CommunicationCodeHyperlink hyperlink;

        public void CommunicationCodeHyperlinkTestsInitialize()
        {
            url = "http://www.ellucian.com";
            title = "Ellucian, Inc.";
        }

        [TestClass]
        public class CommunicationCodeHyperlinkConstructorTests : CommunicationCodeHyperlinkTests
        {
            [TestInitialize]
            public void Initialize()
            {
                CommunicationCodeHyperlinkTestsInitialize();
            }

            [TestMethod]
            public void UrlTest()
            {
                hyperlink = new CommunicationCodeHyperlink(url, title);
                Assert.AreEqual(url, hyperlink.Url);
            }

            [TestMethod]
            public void TitleTest()
            {
                hyperlink = new CommunicationCodeHyperlink(url, title);
                Assert.AreEqual(title, hyperlink.Title);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void UrlRequiredTest()
            {
                new CommunicationCodeHyperlink(null, title);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void TitleRequiredTest()
            {
                new CommunicationCodeHyperlink(url, "");
            }
        }

        [TestClass]
        public class UrlAndTitleAttributesTests : CommunicationCodeHyperlinkTests
        {
            [TestInitialize]
            public void Initialize()
            {
                CommunicationCodeHyperlinkTestsInitialize();
                hyperlink = new CommunicationCodeHyperlink(url, title);
            }

            [TestMethod]
            public void SetAndGetUrlTest()
            {
                var newUrl = "www.foobar.com";
                hyperlink.Url = newUrl;
                Assert.AreEqual(newUrl, hyperlink.Url);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public void SetEmptyUrlTest()
            {
                hyperlink.Url = "";
            }

            [TestMethod]
            public void SetAndGetTitleTest()
            {
                var newTitle = "foobar";
                hyperlink.Title = newTitle;
                Assert.AreEqual(newTitle, hyperlink.Title);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public void SetEmptyTitleTest()
            {
                hyperlink.Title = null;
            }
        }

        [TestClass]
        public class EqualsAndHashCodeTests : CommunicationCodeHyperlinkTests
        {
            [TestInitialize]
            public void Initialize()
            {
                CommunicationCodeHyperlinkTestsInitialize();
                hyperlink = new CommunicationCodeHyperlink(url, title);
            }

            [TestMethod]
            public void EqualIfUrlAndTitleAreEqualTest()
            {
                var test = new CommunicationCodeHyperlink(url, title);
                Assert.IsTrue(test.Equals(hyperlink));
                Assert.IsTrue(hyperlink.Equals(test));
            }

            [TestMethod]
            public void SameHashCodeIfUrlAndTitleAreEqualTest()
            {
                var test = new CommunicationCodeHyperlink(url, title);
                Assert.AreEqual(test.GetHashCode(), hyperlink.GetHashCode());
            }

            [TestMethod]
            public void NotEqualIfUrlsAreDifferentTest()
            {
                var test = new CommunicationCodeHyperlink("foobar", title);
                Assert.IsFalse(hyperlink.Equals(test));
            }

            [TestMethod]
            public void DifferentHashCodeIfUrlsAreDifferentTest()
            {
                var test = new CommunicationCodeHyperlink("foobar", title);
                Assert.AreNotEqual(test.GetHashCode(), hyperlink.GetHashCode());
            }

            [TestMethod]
            public void NotEqualIfTitlesAreDifferentTest()
            {
                var test = new CommunicationCodeHyperlink(url, "foobar");
                Assert.IsFalse(hyperlink.Equals(test));
            }

            [TestMethod]
            public void DifferentHashCodeIfTitlesAreDifferentTest()
            {
                var test = new CommunicationCodeHyperlink("foobar", title);
                Assert.AreNotEqual(test.GetHashCode(), hyperlink.GetHashCode());
            }

            [TestMethod]
            public void NotEqualIfNullTest()
            {
                Assert.IsFalse(hyperlink.Equals(null));
            }

            [TestMethod]
            public void NotEqualIfDifferentTypeTest()
            {
                Assert.IsFalse(hyperlink.Equals(new CommunicationCode("guid","code", "desc")));
            }
        }
    }
}
