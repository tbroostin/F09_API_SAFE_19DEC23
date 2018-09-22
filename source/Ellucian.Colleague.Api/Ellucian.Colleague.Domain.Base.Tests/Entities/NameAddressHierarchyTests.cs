// Copyright 2017 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Domain.Base.Entities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Ellucian.Colleague.Domain.Base.Tests.Entities
{
    [TestClass]
    public class NameAddressHierarchyTests
    {
        [TestClass]
        public class NameAddressHierarchyConstructor
        {
            private string code;
            private List<string> nameTypes = new List<string>();
            private NameAddressHierarchy nah;

            [TestInitialize]
            public void Initialize()
            {
                code = "PREFERRED";

                nah = new NameAddressHierarchy(code);
            }

            [TestMethod]
            public void NameAddressHierarchyCode()
            {
                Assert.AreEqual(code, nah.Code);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void NameAddressHierarchyCodeEmptyException()
            {
                new NameAddressHierarchy(string.Empty);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void NameAddressHierarchyCodeNullException()
            {
                new NameAddressHierarchy(null);
            }

        }

        [TestClass]
        public class NameAddressHierarchyTests_AddNameTypes
        {
            private string code;
            private NameAddressHierarchy nah;

            [TestInitialize]
            public void Initialize()
            {

                code = "ALTERNATE";
                nah = new NameAddressHierarchy(code);
                nah.AddNameTypeHierarchy("AAA");
                nah.AddNameTypeHierarchy("BBB");
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void NameAddressHierarchyTests_AddNameTypeHierarchy_NullNameType()
            {
                nah.AddNameTypeHierarchy(null);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void NameAddressHierarchyTests_AddNameTypeHierarchy_EmptyBuilding()
            {
                nah.AddNameTypeHierarchy(string.Empty);
            }

            [TestMethod]
            public void NameAddressHierarchyTests_AddNameTypeHierarchy_Count()
            {
                Assert.AreEqual(2, nah.NameTypeHierarchy.Count());
            }

            [TestMethod]
            public void NameAddressHierarchyTests_AddNameTypeHierarchy_Values()
            {
                Assert.AreEqual("AAA", nah.NameTypeHierarchy[0]);
                Assert.AreEqual("BBB", nah.NameTypeHierarchy[1]);
            }

            [TestMethod]
            public void NameAddressHierarchyTests_AddNameTypeHierarchy_AddDuplicate()
            {
                nah.AddNameTypeHierarchy("AAA");
                Assert.AreEqual(2, nah.NameTypeHierarchy.Count());
            }
        }

        
    }
}