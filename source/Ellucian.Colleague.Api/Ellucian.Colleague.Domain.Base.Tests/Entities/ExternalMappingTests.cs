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
    public class ExternalMappingTests
    {
        private string code;
        private string description;
        private ExternalMapping externalMapping;
        private ExternalMappingItem item1;
        private ExternalMappingItem item2;
        private ExternalMappingItem item3;
        private ExternalMappingItem item4;

        [TestInitialize]
        public void Initialize()
        {
            code = "LDM.SUBJECTS";
            description = "Mapping of Subjects to Departments";
            externalMapping = new ExternalMapping(code, description);
            item1 = new ExternalMappingItem("ACCT") { NewCode = "BUSN" };
            item2 = new ExternalMappingItem("AVIA") { NewCode = "AUTO" };
            item3 = new ExternalMappingItem("COMP") { NewCode = "MATH" };
            item4 = new ExternalMappingItem("ECON") { NewCode = "BUSN" };
        }

        [TestClass]
        public class ExternalMappingAddItemTests : ExternalMappingTests
        {
            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void ExternalMappingAddItemNullItem()
            {
                externalMapping.AddItem(null);
            }

            [TestMethod]
            public void ExternalMappingAddItemValidItem()
            {
                externalMapping.AddItem(item1);
                Assert.AreEqual(item1, externalMapping.Items.ToList()[0]);
            }

            [TestMethod]
            public void ExternalMappingAddItemDuplicateItem()
            {
                externalMapping.AddItem(item1);
                externalMapping.AddItem(item1);
                Assert.AreEqual(1, externalMapping.Items.Count);
                Assert.AreEqual(item1, externalMapping.Items.ToList()[0]);
            }
        }

        [TestClass]
        public class ExternalMappingGetInternalCodeMappingTests : ExternalMappingTests
        {
            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void ExternalMappingGetInternalCodeMappingNullExternalCode()
            {
                externalMapping.GetInternalCodeMapping(null);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void ExternalMappingGetInternalCodeMappingEmptyExternalCode()
            {
                externalMapping.GetInternalCodeMapping(string.Empty);
            }

            [TestMethod]
            public void ExternalMappingGetInternalCodeMappingValidExternalCode()
            {
                externalMapping.AddItem(item1);
                externalMapping.AddItem(item2);
                externalMapping.AddItem(item3);
                var internalCode = externalMapping.GetInternalCodeMapping("ACCT");
                Assert.AreEqual(item1, internalCode);
            }

            [TestMethod]
            [ExpectedException(typeof(InvalidOperationException))]
            public void ExternalMappingGetInternalCodeMappingInvalidExternalCode()
            {
                externalMapping.AddItem(item1);
                externalMapping.AddItem(item2);
                externalMapping.AddItem(item3);
                var internalCode = externalMapping.GetInternalCodeMapping("BADCODE");
            }
        }

        [TestClass]
        public class ExternalMappingGetExternalCodeMappingsTests : ExternalMappingTests
        {
            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void ExternalMappingGetExternalCodeMappingsNullInternalCode()
            {
                externalMapping.GetExternalCodeMappings(null);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void ExternalMappingGetExternalCodeMappingsEmptyInternalCode()
            {
                externalMapping.GetExternalCodeMappings(string.Empty);
            }

            [TestMethod]
            public void ExternalMappingGetExternalCodeMappingsValidInternalCode()
            {
                externalMapping.AddItem(item1);
                externalMapping.AddItem(item2);
                externalMapping.AddItem(item3);
                externalMapping.AddItem(item4);
                var externalCodes = externalMapping.GetExternalCodeMappings("BUSN");
                Assert.AreEqual(item1, externalCodes.ToList()[0]);
                Assert.AreEqual(item4, externalCodes.ToList()[1]);
            }

            [TestMethod]
            [ExpectedException(typeof(InvalidOperationException))]
            public void ExternalMappingGetExternalCodeMappingsInvalidInternalCode()
            {
                externalMapping.AddItem(item1);
                externalMapping.AddItem(item2);
                externalMapping.AddItem(item3);
                externalMapping.AddItem(item4);
                var externalCodes = externalMapping.GetExternalCodeMappings("BADCODE");
            }
        }
    }
}
