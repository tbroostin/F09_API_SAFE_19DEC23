// Copyright 2020 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Domain.Base.Entities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace Ellucian.Colleague.Domain.Base.Tests.Entities
{
    [TestClass]
    public class RequiredDocumentCollectionMappingTests
    {
        [TestClass]
        public class RequiredDocumentCollectionMappingConstructor
        {
            private string code1;
            private string collection1;
            private string code2;
            private string collection2;
            private OfficeCodeAttachmentCollection officeCollection1;
            private OfficeCodeAttachmentCollection officeCollection2;
            private RequiredDocumentCollectionMapping codeCollection;

            [TestInitialize]
            public void Initialize()
            {
                code1 = "OFFICEA";
                collection1 = "ADMIN_COLLECTION";
                code2 = "OFFICEB";
                collection2 = "OTHER_COLLECTION";
                officeCollection1 = new OfficeCodeAttachmentCollection(code1, collection1);
                officeCollection2 = new OfficeCodeAttachmentCollection(code2, collection2);
                codeCollection = new RequiredDocumentCollectionMapping();
                codeCollection.RequestsWithoutOfficeCodeCollection = "NO_OFFICE_CODE";
                codeCollection.UnmappedOfficeCodeCollection = "UNMAPPED_OFFICE_CODE";
                codeCollection.AddOfficeCodeAttachment(officeCollection1);
                codeCollection.AddOfficeCodeAttachment(officeCollection2);
            }

            [TestCleanup]
            public void Cleanup()
            {
                codeCollection = null;
            }

            [TestMethod]
            public void RequiredDocumentCollectionMapping_Default()
            {
                var newCodeCollection = new RequiredDocumentCollectionMapping();
                Assert.AreEqual(0, newCodeCollection.OfficeCodeMapping.Count);
            }

            [TestMethod]
            public void RequiredDocumentCollectionMapping_TestAllFields()
            {
                Assert.AreEqual("NO_OFFICE_CODE", codeCollection.RequestsWithoutOfficeCodeCollection);
                Assert.AreEqual("UNMAPPED_OFFICE_CODE", codeCollection.UnmappedOfficeCodeCollection);
                Assert.AreEqual(2, codeCollection.OfficeCodeMapping.Count);
            }

            [TestMethod]
            public void RequiredDocumentCollectionMapping_AddOfficeMapping_AddingDuplicate()
            {
                codeCollection.AddOfficeCodeAttachment(officeCollection1);
                // Should still only have 2
                Assert.AreEqual(2, codeCollection.OfficeCodeMapping.Count);
            } 


        }
    }
}
