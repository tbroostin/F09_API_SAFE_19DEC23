// Copyright 2020 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Domain.Base.Entities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace Ellucian.Colleague.Domain.Base.Tests.Entities
{
    [TestClass]
    public class OfficeCodeAttachmentCollectionTests
    {
        [TestClass]
        public class OfficeCodeAttachmentCollectionConstructor
        {
            private string code;
            private string collection;
            private OfficeCodeAttachmentCollection codeCollection;

            [TestInitialize]
            public void Initialize()
            {
                code = "OFFICEA";
                collection = "Administrative";
                codeCollection = new OfficeCodeAttachmentCollection(code, collection);
            }

            [TestCleanup]
            public void Cleanup()
            {
                codeCollection = null;
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void NullOfficeCode()
            {
                codeCollection = new OfficeCodeAttachmentCollection(null, collection);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void EmptyOfficeCode()
            {
                codeCollection = new OfficeCodeAttachmentCollection(string.Empty, collection);
            } 

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void NullAttachmentCollection()
            {
                codeCollection = new OfficeCodeAttachmentCollection(code, null);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void EmptyAttachmentCollection()
            {
                codeCollection = new OfficeCodeAttachmentCollection(code, string.Empty);
            }

            [TestMethod]
            public void OfficeCode()
            {
                Assert.AreEqual(codeCollection.OfficeCode, code);
            }

            [TestMethod]
            public void AttachmentCollection()
            {
                Assert.AreEqual(codeCollection.AttachmentCollection, collection);
            }
        }
    }
}
