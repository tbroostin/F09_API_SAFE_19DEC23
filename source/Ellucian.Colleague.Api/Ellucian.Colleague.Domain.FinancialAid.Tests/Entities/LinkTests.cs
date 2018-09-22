using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ellucian.Colleague.Domain.FinancialAid.Entities;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Ellucian.Colleague.Domain.FinancialAid.Tests.Entities
{
    /// <summary>
    /// Test the Domain
    /// </summary>
    [TestClass]
    public class LinkTests
    {
        [TestClass]
        public class LinkConstructor
        {
            private string title;
            private string linkUrl;
            private LinkTypes linkType;

            private Link link;

            [TestInitialize]
            public void Initialize()
            {
                title = "Complete the FAFSA";
                linkUrl = "http://www.fafsa.ed.gov";
                linkType = LinkTypes.FAFSA;

                link = new Link(title, linkType, linkUrl);
            }

            /// <summary>
            /// Test that the constructor set the title correctly
            /// </summary>
            [TestMethod]
            public void LinkTitleCodeEqual()
            {
                Assert.AreEqual(title, link.Title);
            }

            /// <summary>
            /// Test that the constructor set the linkUrl correctly
            /// </summary>
            [TestMethod]
            public void LinkUrlCodeEqual()
            {
                Assert.AreEqual(linkUrl, link.LinkUrl);
            }

            /// <summary>
            /// Test that the constructor set the linkType correctly
            /// </summary>
            [TestMethod]
            public void LinkTypeCodeEqual()
            {
                Assert.AreEqual(linkType, link.LinkType);
            }

            /// <summary>
            /// Test that the constructor throws an exception when a null
            /// value is passed in for the code.
            /// </summary>
            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void LinkTitleNullException()
            {
                new Link(null, link.LinkType, link.LinkUrl);
            }

            /// <summary>
            /// Test that the constructor throws an exception when a null
            /// value is passed in for the description.
            /// </summary>
            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void LinkUrlNullException()
            {
                new Link(link.Title, link.LinkType, null);
            }

            /// <summary>
            /// Test that the Public Accessor Get and Set methods work correctly.
            /// Get -> should return value of private attribute
            /// Set -> should sets value of private attribute to input value
            /// </summary>
            [TestMethod]
            public void LinkTypeGetSet()
            {
                link.LinkType = LinkTypes.FAFSA;
                Assert.AreEqual(link.LinkType, LinkTypes.FAFSA);
            }
        }
    }
}
