// Copyright 2019 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Coordination.Base.Adapters;
using Ellucian.Colleague.Dtos.Base;
using Ellucian.Web.Adapters;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;
using System;

namespace Ellucian.Colleague.Coordination.Base.Tests.Adapters
{
    [TestClass]
    public class AttachmentDtoAdapterTests
    {
        AttachmentDtoAdapter mapper;
        Mock<IAdapterRegistry> adapterRegistryMock;
        Mock<ILogger> loggerMock;

        [TestInitialize]
        public void Initialize()
        {
            loggerMock = new Mock<ILogger>();
            adapterRegistryMock = new Mock<IAdapterRegistry>();

            mapper = new AttachmentDtoAdapter(adapterRegistryMock.Object, loggerMock.Object);
        }

        [TestClass]
        public class AttachmentDtoAdapter_MapToTypeTests : AttachmentDtoAdapterTests
        {
            Attachment attachment;

            [TestInitialize]
            public void AttachmentDtoAdapter_MapToType_Initialize()
            {
                attachment = new Attachment()
                {
                    Id = "28a1adee-f28a-4a77-b8e7-318f96812aa4",
                    CollectionId = "COLLECTION1",
                    ContentType = "application/pdf",
                    CreatedBy = "0000001",
                    DeletedBy = "9999999",
                    ModifiedBy = "2222222",
                    Name = "test.pdf",
                    Owner = "0000001",
                    Size = 122332,
                    Status = AttachmentStatus.Active,
                    TagOne = "tag"
                };
            }

            [TestMethod]
            public void AttachmentDtoAdapter_MapToType_Success()
            {
                var actual = mapper.MapToType(attachment);
                Assert.AreEqual(attachment.Id, actual.Id);
                Assert.AreEqual(attachment.CollectionId, actual.CollectionId);
                Assert.AreEqual(attachment.ContentType, actual.ContentType);
                Assert.AreEqual(attachment.CreatedBy, actual.CreatedBy);
                Assert.AreEqual(attachment.DeletedBy, actual.DeletedBy);
                Assert.AreEqual(attachment.ModifiedBy, actual.ModifiedBy);
                Assert.AreEqual(attachment.Name, actual.Name);
                Assert.AreEqual(attachment.Owner, actual.Owner);
                Assert.AreEqual(attachment.Size, actual.Size);
                Assert.AreEqual(attachment.Status.ToString(), actual.Status.ToString());
                Assert.AreEqual(attachment.TagOne, actual.TagOne);
                attachment.Status = AttachmentStatus.Active;
                actual = mapper.MapToType(attachment);
                Assert.AreEqual(attachment.Status.ToString(), actual.Status.ToString());
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void AttachmentDtoAdapter_MapToType_IdNull()
            {
                mapper.MapToType(new Attachment()
                {
                    Id = null,
                    CollectionId = attachment.CollectionId,
                    ContentType = attachment.ContentType,
                    Name = attachment.Name,
                    Size = attachment.Size,
                    Owner = attachment.Owner
                });
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void AttachmentDtoAdapter_MapToType_IdEmpty()
            {
                mapper.MapToType(new Attachment()
                {
                    Id = string.Empty,
                    CollectionId = attachment.CollectionId,
                    ContentType = attachment.ContentType,
                    Name = attachment.Name,
                    Size = attachment.Size,
                    Owner = attachment.Owner
                });
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void AttachmentDtoAdapter_MapToType_CollectionIdNull()
            {
                mapper.MapToType(new Attachment()
                {
                    Id = attachment.Id,
                    CollectionId = null,
                    ContentType = attachment.ContentType,
                    Name = attachment.Name,
                    Size = attachment.Size,
                    Owner = attachment.Owner
                });
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void AttachmentDtoAdapter_MapToType_CollectionIdEmpty()
            {
                mapper.MapToType(new Attachment()
                {
                    Id = attachment.Id,
                    CollectionId = string.Empty,
                    ContentType = attachment.ContentType,
                    Name = attachment.Name,
                    Size = attachment.Size,
                    Owner = attachment.Owner
                });
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void AttachmentDtoAdapter_MapToType_ContentTypeNull()
            {
                mapper.MapToType(new Attachment()
                {
                    Id = attachment.Id,
                    CollectionId = attachment.CollectionId,
                    ContentType = null,
                    Name = attachment.Name,
                    Size = attachment.Size,
                    Owner = attachment.Owner
                });
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void AttachmentDtoAdapter_MapToType_ContentTypeEmpty()
            {
                mapper.MapToType(new Attachment()
                {
                    Id = attachment.Id,
                    CollectionId = attachment.CollectionId,
                    ContentType = string.Empty,
                    Name = attachment.Name,
                    Size = attachment.Size,
                    Owner = attachment.Owner
                });
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void AttachmentDtoAdapter_MapToType_NameNull()
            {
                mapper.MapToType(new Attachment()
                {
                    Id = attachment.Id,
                    CollectionId = attachment.CollectionId,
                    ContentType = attachment.ContentType,
                    Name = null,
                    Size = attachment.Size,
                    Owner = attachment.Owner
                });
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void AttachmentDtoAdapter_MapToType_NameEmpty()
            {
                mapper.MapToType(new Attachment()
                {
                    Id = attachment.Id,
                    CollectionId = attachment.CollectionId,
                    ContentType = attachment.ContentType,
                    Name = string.Empty,
                    Size = attachment.Size,
                    Owner = attachment.Owner
                });
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void AttachmentDtoAdapter_MapToType_SizeNull()
            {
                mapper.MapToType(new Attachment()
                {
                    Id = attachment.Id,
                    CollectionId = attachment.CollectionId,
                    ContentType = attachment.ContentType,
                    Name = attachment.Name,
                    Size = null,
                    Owner = attachment.Owner
                });
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void AttachmentDtoAdapter_MapToType_OwnerNull()
            {
                mapper.MapToType(new Attachment()
                {
                    Id = attachment.Id,
                    CollectionId = attachment.CollectionId,
                    ContentType = attachment.ContentType,
                    Name = attachment.Name,
                    Size = attachment.Size,
                    Owner = null
                });
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void AttachmentDtoAdapter_MapToType_OwnerEmpty()
            {
                mapper.MapToType(new Attachment()
                {
                    Id = attachment.Id,
                    CollectionId = attachment.CollectionId,
                    ContentType = attachment.ContentType,
                    Name = attachment.Name,
                    Size = attachment.Size,
                    Owner = string.Empty
                });
            }
        }
    }
}