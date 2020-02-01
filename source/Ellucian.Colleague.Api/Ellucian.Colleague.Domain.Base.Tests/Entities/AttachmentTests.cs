// Copyright 2019 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Domain.Base.Entities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace Ellucian.Colleague.Domain.Base.Tests.Entities
{
    [TestClass]
    public class AttachmentTests
    {
        [TestClass]
        public class Attachment_Constructor
        {
            [TestMethod]
            public void AttachmentConstructorSuccess()
            {
                var actual = new Attachment("id", "collectionId", "name", "contentType", 100, "owner");
                Assert.AreEqual("id", actual.Id);
                Assert.AreEqual("collectionId", actual.CollectionId);
                Assert.AreEqual("name", actual.Name);
                Assert.AreEqual("contentType", actual.ContentType);
                Assert.AreEqual(100, actual.Size);
                Assert.AreEqual("owner", actual.Owner);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void AttachmentConstructorIdNull()
            {
                new Attachment(null, "collectionId", "name", "contentType", 100, "owner");
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void AttachmentConstructorIdEmpty()
            {
                new Attachment(string.Empty, "collectionId", "name", "contentType", 100, "owner");
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void AttachmentConstructorCollectionIdNull()
            {
                new Attachment("id", null, "name", "contentType", 100, "owner");
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void AttachmentConstructorCollectionIdEmpty()
            {
                new Attachment("id", string.Empty, "name", "contentType", 100, "owner");
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void AttachmentConstructorNameNull()
            {
                new Attachment("id", "collectionId", null, "contentType", 100, "owner");
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void AttachmentConstructorNameEmpty()
            {
                new Attachment("id", "collectionId", string.Empty, "contentType", 100, "owner");
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void AttachmentConstructorContentTypeNull()
            {
                new Attachment("id", "collectionId", "name", null, 100, "owner");
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void AttachmentConstructorContentTypeEmpty()
            {
                new Attachment("id", "collectionId", "name", string.Empty, 100, "owner");
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void AttachmentConstructorOwnerNull()
            {
                new Attachment("id", "collectionId", "name", "contentType", 100, null);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void AttachmentConstructorOwnerEmpty()
            {
                new Attachment("id", "collectionId", "name", "contentType", 100, string.Empty);
            }

            [TestMethod]
            [ExpectedException(typeof(InvalidOperationException))]
            public void AttachmentInvalidIdOperation()
            {
                var attachment = new Attachment("id", "collectionId", "name", "contentType", 100, "owner");
                attachment.Id = "123";
            }

            [TestMethod]
            [ExpectedException(typeof(InvalidOperationException))]
            public void AttachmentInvalidCollectionIdOperation()
            {
                var attachment = new Attachment("id", "collectionId", "name", "contentType", 100, "owner");
                attachment.CollectionId = "123";
            }

            [TestMethod]
            [ExpectedException(typeof(InvalidOperationException))]
            public void AttachmentInvalidOwnerOperation()
            {
                var attachment = new Attachment("id", "collectionId", "name", "contentType", 100, "owner");
                attachment.Owner = "NEWOWNER";
            }
        }
    }
}