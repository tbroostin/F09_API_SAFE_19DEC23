// Copyright 2014 Ellucian Company L.P. and its affiliates.
using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ellucian.Colleague.Domain.Base.Entities;

namespace Ellucian.Colleague.Domain.Base.Tests.Entities
{
    [TestClass]
    public class ExternalMappingItemTests
    {
        private string originalCode;
        private ExternalMappingItem externalMappingItem1;
        private ExternalMappingItem externalMappingItem2;

        [TestInitialize]
        public void Initialize()
        {
            originalCode = "CRS.SUBJECT";
        }

        [TestClass]
        public class ExternalMappingItemConstructor : ExternalMappingItemTests
        {
            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void ExternalMappingItemConstructorNullOriginalCode()
            {
                externalMappingItem1 = new ExternalMappingItem(null);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void ExternalMappingItemConstructorEmptyOriginalCode()
            {
                externalMappingItem1 = new ExternalMappingItem(string.Empty);
            }

            [TestMethod]
            public void ExternalMappingItemConstructorValidOriginalCode()
            {
                externalMappingItem1 = new ExternalMappingItem(originalCode);
            }
        }

        [TestClass]
        public class ExternalMappingItemEquals : ExternalMappingItemTests
        {
            [TestMethod]
            public void ExternalMappingItemEqualsNullItem()
            {
                externalMappingItem1 = new ExternalMappingItem(originalCode);
                Assert.IsFalse(externalMappingItem1.Equals(null));
            }

            [TestMethod]
            public void ExternalMappingItemEqualsDifferentType()
            {
                externalMappingItem1 = new ExternalMappingItem(originalCode);
                Assert.IsFalse(externalMappingItem1.Equals(originalCode));
            }

            [TestMethod]
            public void ExternalMappingItemEqualsDifferentItem()
            {
                externalMappingItem1 = new ExternalMappingItem(originalCode);
                externalMappingItem2 = new ExternalMappingItem(originalCode + "A");
                Assert.IsFalse(externalMappingItem1.Equals(externalMappingItem2));
            }

            [TestMethod]
            public void ExternalMappingItemEqualsMatchingItem()
            {
                externalMappingItem1 = new ExternalMappingItem(originalCode);
                externalMappingItem2 = new ExternalMappingItem(originalCode);
                Assert.IsTrue(externalMappingItem1.Equals(externalMappingItem2));
            }
        }

        [TestClass]
        public class ExternalMappingItemGetHashCode : ExternalMappingItemTests
        {
            [TestMethod]
            public void ExternalMappingItemSameCodeHashEqual()
            {
                externalMappingItem1 = new ExternalMappingItem(originalCode);
                externalMappingItem2 = new ExternalMappingItem(originalCode);
                Assert.AreEqual(externalMappingItem1.GetHashCode(), externalMappingItem2.GetHashCode());
            }

            [TestMethod]
            public void ExternalMappingItemDifferentCodeHashNotEqual()
            {
                externalMappingItem1 = new ExternalMappingItem(originalCode);
                externalMappingItem2 = new ExternalMappingItem(originalCode + "A");
                Assert.AreNotEqual(externalMappingItem1.GetHashCode(), externalMappingItem2.GetHashCode());
            }
        }
    }
}
