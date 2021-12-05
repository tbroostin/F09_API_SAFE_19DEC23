// Copyright 2019-2020 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Data.Base.Repositories;
using Ellucian.Colleague.Data.Base.Transactions;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Data.Colleague.Exceptions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Data.Base.Tests.Repositories
{
    [TestClass]
    public class AttachmentRepositoryTests : BaseRepositorySetup
    {
        AttachmentRepository repository;

        [TestInitialize]
        public void Initialize()
        {
            // Initialize Mock framework
            MockInitialize();

            // Build the test repository
            repository = new AttachmentRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object, apiSettings);
        }

        [TestCleanup]
        public void Cleanup()
        {
            repository = null;
        }

        [TestClass]
        public class AttachmentRepository_GetAttachmentByIdWithEncrMetadataAsyncTests : AttachmentRepositoryTests
        {
            DataContracts.Attachments userData;

            [TestInitialize]
            public void AttachmentRepository_GetAttachmentByIdWithEncrMetadataAsync_Initialize()
            {
                Initialize();

                userData = new DataContracts.Attachments()
                {
                    Recordkey = "f070516e-09f4-4232-af06-78391100e213",
                    AttCollectionId = "COLLECTION1",
                    AttContentType = "application/pdf",
                    AttName = "myattachment.pdf",
                    AttOwner = "0000001",
                    AttSize = 10000,
                    AttStatus = "Active",
                    AttTagOne = "tag",
                    AttachmentsAddopr = "USER",
                    AttachmentsChgopr = "USER2",
                    AttachmentsDelopr = "USER3",
                    AttEncrContentKey = "zqc0PJyMqfgHS2txd56RDi7dfMoI76elZZyyntMFGs2jcogIVXSU01Y9hc"
                    + "LezN0KLf8v3crobdiPkHWSCtV+Xl+XM5vhJpfiVVqEdMgG7xXwvdY5rl8gMh298wVSfmCQaFu5z9oMkqn39NJFpr9amf"
                    + "MOANLrzMUwFdX/jxi2lJJmiirCt/1uNXA6/cQdRfHBtvT0tur+4En5TpSY84DkevOcnDTaKiPa3ZIHM1SZTR9T0mKY59"
                    + "0Vyo97YJFOuR6W7Q+hVuA62lTqAxUOc19nwKJRCwJ5K6dxtZqaGhbPQu9ubFrvGHPYNrhApjYBlqBdk2yYGKYRy7TIDI"
                    + "2ZxLIvqA==",
                    AttEncrIv = "d7ztE4TNClbBVp4tbRb94w==",
                    AttEncrKeyId = "7c655b4d-c425-4aff-af98-337004ec8cfe",
                    AttEncrType = "AES256"
                };
                userData.buildAssociations();
            }

            [TestMethod]
            public async Task AttachmentRepository_GetAttachmentByIdWithEncrMetadataAsync_Success()
            {
                dataReaderMock.Setup(accessor =>
                    accessor.ReadRecordAsync<DataContracts.Attachments>
                    (It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(userData);

                var actual = await repository.GetAttachmentByIdWithEncrMetadataAsync(userData.Recordkey);
                Assert.AreEqual(userData.Recordkey, actual.Id);
                Assert.AreEqual(userData.AttCollectionId, actual.CollectionId);
                Assert.AreEqual(userData.AttContentType, actual.ContentType);
                Assert.AreEqual(userData.AttName, actual.Name);
                Assert.AreEqual(userData.AttOwner, actual.Owner);
                Assert.AreEqual(userData.AttSize, actual.Size);
                Assert.AreEqual(userData.AttStatus, actual.Status.ToString());
                Assert.AreEqual(userData.AttTagOne, actual.TagOne);
                Assert.AreEqual(userData.AttachmentsAddopr, actual.CreatedBy);
                Assert.AreEqual(userData.AttachmentsChgopr, actual.ModifiedBy);
                Assert.AreEqual(userData.AttachmentsDelopr, actual.DeletedBy);
                Assert.AreEqual(userData.AttEncrContentKey, Convert.ToBase64String(actual.EncrContentKey));
                Assert.AreEqual(userData.AttEncrIv, Convert.ToBase64String(actual.EncrIV));
                Assert.AreEqual(userData.AttEncrKeyId, actual.EncrKeyId);
                Assert.AreEqual(userData.AttEncrType, actual.EncrType);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task AttachmentRepository_GetAttachmentByIdWithEncrMetadataAsync_NullId()
            {
                await repository.GetAttachmentByIdWithEncrMetadataAsync(null);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task AttachmentRepository_GetAttachmentByIdWithEncrMetadataAsync_EmptyId()
            {
                await repository.GetAttachmentByIdWithEncrMetadataAsync(string.Empty);
            }

            [TestMethod]
            public async Task AttachmentRepository_GetAttachmentByIdWithEncrMetadataAsync_Exception()
            {
                dataReaderMock.Setup(accessor =>
                    accessor.ReadRecordAsync<DataContracts.Attachments>
                    (It.IsAny<string>(), It.IsAny<bool>())).Throws(new Exception());

                string expected = "Error occurred retrieving attachment metadata";
                string actual = string.Empty;
                try
                {
                    await repository.GetAttachmentByIdWithEncrMetadataAsync(userData.Recordkey);
                }
                catch (Exception e)
                {
                    actual = e.Message;
                }

                Assert.AreEqual(expected, actual);
            }
        }

        [TestClass]
        public class AttachmentRepository_GetAttachmentByIdNoEncrMetadataAsyncTests : AttachmentRepositoryTests
        {
            DataContracts.AttachmentsNoEncr userData;

            [TestInitialize]
            public void AttachmentRepository_GetAttachmentByIdNoEncrMetadataAsync_Initialize()
            {
                Initialize();

                userData = new DataContracts.AttachmentsNoEncr()
                {
                    Recordkey = "f070516e-09f4-4232-af06-78391100e213",
                    AttCollectionId = "COLLECTION1",
                    AttContentType = "application/pdf",
                    AttName = "myattachment.pdf",
                    AttOwner = "0000001",
                    AttSize = 10000,
                    AttStatus = "Active",
                    AttTagOne = "tag",
                    AttachmentsAddopr = "USER",
                    AttachmentsChgopr = "USER2",
                    AttachmentsDelopr = "USER3"
                };
                userData.buildAssociations();
            }

            [TestMethod]
            public async Task AttachmentRepository_GetAttachmentByIdNoEncrMetadataAsync_Success()
            {
                dataReaderMock.Setup(accessor =>
                    accessor.ReadRecordAsync<DataContracts.AttachmentsNoEncr>
                    (It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(userData);

                var actual = await repository.GetAttachmentByIdNoEncrMetadataAsync(userData.Recordkey);
                Assert.AreEqual(userData.Recordkey, actual.Id);
                Assert.AreEqual(userData.AttCollectionId, actual.CollectionId);
                Assert.AreEqual(userData.AttContentType, actual.ContentType);
                Assert.AreEqual(userData.AttName, actual.Name);
                Assert.AreEqual(userData.AttOwner, actual.Owner);
                Assert.AreEqual(userData.AttSize, actual.Size);
                Assert.AreEqual(userData.AttStatus, actual.Status.ToString());
                Assert.AreEqual(userData.AttTagOne, actual.TagOne);
                Assert.AreEqual(userData.AttachmentsAddopr, actual.CreatedBy);
                Assert.AreEqual(userData.AttachmentsChgopr, actual.ModifiedBy);
                Assert.AreEqual(userData.AttachmentsDelopr, actual.DeletedBy);
                Assert.AreEqual(null, actual.EncrContentKey);
                Assert.AreEqual(null, actual.EncrIV);
                Assert.AreEqual(null, actual.EncrKeyId);
                Assert.AreEqual(null, actual.EncrType);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task AttachmentRepository_GetAttachmentByIdNoEncrMetadataAsync_NullId()
            {
                await repository.GetAttachmentByIdNoEncrMetadataAsync(null);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task AttachmentRepository_GetAttachmentByIdNoEncrMetadataAsync_EmptyId()
            {
                await repository.GetAttachmentByIdNoEncrMetadataAsync(string.Empty);
            }

            [TestMethod]
            public async Task AttachmentRepository_GetAttachmentByIdNoEncrMetadataAsync_Exception()
            {
                dataReaderMock.Setup(accessor =>
                    accessor.ReadRecordAsync<DataContracts.AttachmentsNoEncr>
                    (It.IsAny<string>(), It.IsAny<bool>())).Throws(new Exception());

                string expected = "Error occurred retrieving attachment metadata";
                string actual = string.Empty;
                try
                {
                    await repository.GetAttachmentByIdNoEncrMetadataAsync(userData.Recordkey);
                }
                catch (Exception e)
                {
                    actual = e.Message;
                }

                Assert.AreEqual(expected, actual);
            }
        }

        [TestClass]
        public class AttachmentRepository_GetAttachmentsAsyncTests : AttachmentRepositoryTests
        {
            Collection<DataContracts.AttachmentsNoEncr> userData;

            [TestInitialize]
            public void AttachmentRepository_GetAttachmentsAsync_Initialize()
            {
                Initialize();

                // user data w/ encryption properties
                userData = new Collection<DataContracts.AttachmentsNoEncr>();
                var attachment1 = new DataContracts.AttachmentsNoEncr()
                {
                    Recordkey = "f070516e-09f4-4232-af06-78391100e213",
                    AttCollectionId = "COLLECTION1",
                    AttContentType = "application/pdf",
                    AttName = "myattachment.pdf",
                    AttOwner = "0000001",
                    AttSize = 10000,
                    AttStatus = "Active",
                    AttTagOne = "tag",
                    AttachmentsAddopr = "USER",
                    AttachmentsChgopr = "USER2",
                    AttachmentsDelopr = "USER3"
                };
                userData.Add(attachment1);
                var attachment2 = new DataContracts.AttachmentsNoEncr()
                {
                    Recordkey = "90c96aff-4d7b-4f65-a35a-b87ba6c2fc68",
                    AttCollectionId = "COLLECTION1",
                    AttContentType = "application/pdf",
                    AttName = "myattachment2.pdf",
                    AttOwner = "0000001",
                    AttSize = 10000,
                    AttStatus = "Active",
                    AttTagOne = "tag",
                    AttachmentsAddopr = "USER",
                    AttachmentsChgopr = "USER2",
                    AttachmentsDelopr = "USER3"
                };
                userData.Add(attachment2);
            }

            [TestMethod]
            public async Task AttachmentRepository_GetAttachmentsAsync_OwnerAndCollectionSuccess()
            {
                dataReaderMock.Setup(a => a.BulkReadRecordAsync<DataContracts.AttachmentsNoEncr>
                    (It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(userData);

                var actual = await repository.GetAttachmentsAsync("0000001", "COLLECTION1", null);
                Assert.AreEqual(userData.Count(), actual.Count());
                foreach (var collection in userData)
                {
                    Assert.AreEqual(collection.Recordkey, actual.Where(a => a.Id == collection.Recordkey).First().Id);
                }
            }

            [TestMethod]
            public async Task AttachmentRepository_GetAttachmentsAsync_OwnerSuccess()
            {
                dataReaderMock.Setup(a => a.BulkReadRecordAsync<DataContracts.AttachmentsNoEncr>
                    (It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(userData);

                var actual = await repository.GetAttachmentsAsync("0000001", null, null);
                Assert.AreEqual(userData.Count(), actual.Count());
                foreach (var collection in userData)
                {
                    Assert.AreEqual(collection.Recordkey, actual.Where(a => a.Id == collection.Recordkey).First().Id);
                }
            }

            [TestMethod]
            public async Task AttachmentRepository_GetAttachmentsAsync_CollectionSuccess()
            {
                dataReaderMock.Setup(a => a.BulkReadRecordAsync<DataContracts.AttachmentsNoEncr>
                    (It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(userData);

                var actual = await repository.GetAttachmentsAsync(null, "COLLECTION1", null);
                Assert.AreEqual(userData.Count(), actual.Count());
                foreach (var collection in userData)
                {
                    Assert.AreEqual(collection.Recordkey, actual.Where(a => a.Id == collection.Recordkey).First().Id);
                }
            }

            [TestMethod]
            public async Task AttachmentRepository_GetAttachmentsAsync_TagOneSuccess()
            {
                dataReaderMock.Setup(a => a.BulkReadRecordAsync<DataContracts.AttachmentsNoEncr>
                    (It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(userData);

                var actual = await repository.GetAttachmentsAsync(null, "COLLECTION1", "tag1");
                Assert.AreEqual(userData.Count(), actual.Count());
                foreach (var collection in userData)
                {
                    Assert.AreEqual(collection.Recordkey, actual.Where(a => a.Id == collection.Recordkey).First().Id);
                }
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public async Task AttachmentRepository_GetAttachmentsAsync_NullOwnerAndCollection()
            {
                await repository.GetAttachmentsAsync(null, null, "tag1");
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public async Task AttachmentRepository_GetAttachmentsAsync_EmptyOwnerAndCollection()
            {
                await repository.GetAttachmentsAsync(string.Empty, string.Empty, string.Empty);
            }

            [TestMethod]
            public async Task AttachmentRepository_GetAttachmentsAsync_OwnerAndCollectionException()
            {
                dataReaderMock.Setup(a => a.BulkReadRecordAsync<DataContracts.AttachmentsNoEncr>
                    (It.IsAny<string>(), It.IsAny<bool>())).Throws(new Exception());

                string expected = "Error occurred retrieving bulk attachment metadata";
                string actual = string.Empty;
                try
                {
                    await repository.GetAttachmentsAsync("0000001", "COLLECTION1", null);
                }
                catch (Exception e)
                {
                    actual = e.Message;
                }

                Assert.AreEqual(expected, actual);
            }
        }

        [TestClass]
        public class AttachmentRepository_GetAttachmentContentAsyncTests : AttachmentRepositoryTests
        {
            [TestInitialize]
            public void AttachmentRepository_GetAttachmentContentAsync_Initialize()
            {
                Initialize();
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task AttachmentRepository_GetAttachmentContentAsync_NullAttachment()
            {
                await repository.GetAttachmentContentAsync(null);
            }
        }

        [TestClass]
        public class AttachmentRepository_QueryAttachmentsAsyncTests : AttachmentRepositoryTests
        {
            Collection<DataContracts.AttachmentsNoEncr> userData;

            [TestInitialize]
            public void AttachmentRepository_QueryAttachmentsAsync_Initialize()
            {
                Initialize();

                userData = new Collection<DataContracts.AttachmentsNoEncr>();
                var attachment1 = new DataContracts.AttachmentsNoEncr()
                {
                    Recordkey = "f070516e-09f4-4232-af06-78391100e213",
                    AttCollectionId = "COLLECTION1",
                    AttContentType = "application/pdf",
                    AttName = "myattachment.pdf",
                    AttOwner = "0000001",
                    AttSize = 10000,
                    AttStatus = "Active",
                    AttTagOne = "tag",
                    AttachmentsAddopr = "USER",
                    AttachmentsChgopr = "USER2",
                    AttachmentsDelopr = "USER3"
                };
                userData.Add(attachment1);
                var attachment2 = new DataContracts.AttachmentsNoEncr()
                {
                    Recordkey = "90c96aff-4d7b-4f65-a35a-b87ba6c2fc68",
                    AttCollectionId = "COLLECTION1",
                    AttContentType = "application/pdf",
                    AttName = "myattachment2.pdf",
                    AttOwner = "0000001",
                    AttSize = 10000,
                    AttStatus = "Active",
                    AttTagOne = "tag",
                    AttachmentsAddopr = "USER",
                    AttachmentsChgopr = "USER2",
                    AttachmentsDelopr = "USER3"
                };
                userData.Add(attachment2);
            }

            [TestMethod]
            public async Task AttachmentRepository_QueryAttachmentsAsync_Success()
            {
                dataReaderMock.Setup(a => a.BulkReadRecordAsync<DataContracts.AttachmentsNoEncr>
                    (It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(userData);

                var actual = await repository.QueryAttachmentsAsync(true, "0000001", new DateTime(2020, 3, 26), new DateTime(2020, 3, 27),
                    new List<string>() { "COLLECTION1", "COLLECTION2" });
                Assert.AreEqual(userData.Count(), actual.Count());
                foreach (var collection in userData)
                {
                    Assert.AreEqual(collection.Recordkey, actual.Where(a => a.Id == collection.Recordkey).First().Id);
                }
                
                dataReaderMock.Verify();
            }

            [TestMethod]
            public async Task AttachmentRepository_QueryAttachmentsAsync_NoCollections()
            {
                string expected = "Collection ID(s) are required to query attachments";
                string actual = string.Empty;
                
                // null collection
                try
                {
                    await repository.QueryAttachmentsAsync(false, null, new DateTime(2020, 1, 2), new DateTime(2020, 2, 3), null);
                }
                catch (ArgumentException e)
                {
                    actual = e.Message;
                }

                Assert.AreEqual(expected, actual);

                // empty collection
                actual = string.Empty;
                try
                {
                    await repository.QueryAttachmentsAsync(false, null, new DateTime(2020, 1, 2), new DateTime(2020, 2, 3), new List<string>());
                }
                catch (ArgumentException e)
                {
                    actual = e.Message;
                }

                Assert.AreEqual(expected, actual);

                // empty collection string
                expected = "No valid Collection ID(s) provided to query attachments";
                actual = string.Empty;
                try
                {
                    await repository.QueryAttachmentsAsync(false, null, new DateTime(2020, 1, 2), new DateTime(2020, 2, 3), new List<string>() { string.Empty, string.Empty });
                }
                catch (ArgumentException e)
                {
                    actual = e.Message;
                }

                Assert.AreEqual(expected, actual);
            }

            [TestMethod]
            public async Task AttachmentRepository_QueryAttachmentsAsync_StartDateAndEndDateRequired()
            {
                string expected = "Start and end date are required to query attachments";
                string actual = string.Empty;

                // start date without end date
                try
                {
                    await repository.QueryAttachmentsAsync(false, null, DateTime.Now, null, new List<string>() { "COLLECTION1" });
                }
                catch (ArgumentException e)
                {
                    actual = e.Message;
                }

                Assert.AreEqual(expected, actual);

                // end date without start date
                actual = string.Empty;
                try
                {
                    await repository.QueryAttachmentsAsync(false, null, null, DateTime.Now, new List<string>() { "COLLECTION1" });
                }
                catch (ArgumentException e)
                {
                    actual = e.Message;
                }

                Assert.AreEqual(expected, actual);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentOutOfRangeException))]
            public async Task AttachmentRepository_QueryAttachmentsAsync_InvalidStartDate()
            {
                await repository.QueryAttachmentsAsync(false, null, new DateTime(2020, 3, 2), new DateTime(2020, 3, 1),
                    new List<string>() { "COLLECTION1" });               
            }

            [TestMethod]
            public async Task AttachmentRepository_QueryAttachmentsAsync_Exception()
            {
                dataReaderMock.Setup(a => a.BulkReadRecordAsync<DataContracts.AttachmentsNoEncr>
                    (It.IsAny<string>(), It.IsAny<bool>())).Throws(new Exception());

                string expected = "Error occurred retrieving attachment metadata by query";
                string actual = string.Empty;
                try
                {
                    await repository.QueryAttachmentsAsync(true, "0000001", new DateTime(2020, 3, 26), new DateTime(2020, 3, 27),
                    new List<string>() { "COLLECTION1", "COLLECTION2" });
                }
                catch (Exception e)
                {
                    actual = e.Message;
                }

                Assert.AreEqual(expected, actual);
            }
        }

        [TestClass]
        public class AttachmentRepository_PostAttachmentAsyncTests : AttachmentRepositoryTests
        {
            DataContracts.Attachments userData;
            Domain.Base.Entities.Attachment attachment;

            [TestInitialize]
            public void AttachmentRepository_PostAttachmentAsync_Initialize()
            {
                Initialize();

                userData = new DataContracts.Attachments()
                {
                    Recordkey = "f070516e-09f4-4232-af06-78391100e213",
                    AttCollectionId = "COLLECTION1",
                    AttContentType = "application/pdf",
                    AttName = "myattachment.pdf",
                    AttOwner = "0000001",
                    AttSize = 10000,
                    AttStatus = "Active",
                    AttTagOne = "tag",
                    AttachmentsAddopr = "USER",
                    AttachmentsChgopr = "USER2",
                    AttachmentsDelopr = "USER3"
                };

                attachment = new Domain.Base.Entities.Attachment("f070516e-09f4-4232-af06-78391100e213", "COLLECTION1", "myattachment.pdf",
                    "application/pdf", 100, "0000001")
                {
                    Size = 10000,
                    Status = Domain.Base.Entities.AttachmentStatus.Active,
                    TagOne = "tag"
                };
            }

            [TestMethod]
            public async Task AttachmentRepository_PostAttachmentAsync_Success()
            {
                var expectedResponse = new UpdateAttachmentsResponse
                {
                    ErrorMsg = string.Empty
                };

                transManagerMock.Setup(accessor =>
                    accessor.ExecuteAsync<UpdateAttachmentsRequest, UpdateAttachmentsResponse>
                    (It.IsAny<UpdateAttachmentsRequest>())).ReturnsAsync(expectedResponse);
                dataReaderMock.Setup(accessor =>
                    accessor.ReadRecordAsync<DataContracts.Attachments>
                    (It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(userData);

                var actual = await repository.PostAttachmentAsync(attachment);
                transManagerMock.Verify();
                dataReaderMock.Verify();
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task AttachmentRepository_PostAttachmentAsync_NullAttachment()
            {
                await repository.PostAttachmentAsync(null);
            }

            [TestMethod]
            [ExpectedException(typeof(RepositoryException))]
            public async Task AttachmentRepository_PostAttachmentAsync_NullResponse()
            {

                transManagerMock.Setup(accessor =>
                    accessor.ExecuteAsync<UpdateAttachmentsRequest, UpdateAttachmentsResponse>
                    (It.IsAny<UpdateAttachmentsRequest>())).ReturnsAsync(() => null);

                try
                {
                    await repository.PostAttachmentAsync(attachment);
                }
                catch (RepositoryException e)
                {
                    Assert.IsTrue(e.Message == "No response received creating attachment");
                    throw;
                }
            }

            [TestMethod]
            [ExpectedException(typeof(RepositoryException))]
            public async Task AttachmentRepository_PostAttachmentAsync_ErrorResponse()
            {
                var expectedResponse = new UpdateAttachmentsResponse
                {
                    ErrorMsg = "error occurred"
                };

                transManagerMock.Setup(accessor =>
                    accessor.ExecuteAsync<UpdateAttachmentsRequest, UpdateAttachmentsResponse>
                    (It.IsAny<UpdateAttachmentsRequest>())).ReturnsAsync(expectedResponse);

                try
                {
                    await repository.PostAttachmentAsync(attachment);
                }
                catch (RepositoryException e)
                {
                    Assert.IsTrue(e.Message == "Error occurred creating attachment");
                    throw;
                }
            }

            [TestMethod]
            [ExpectedException(typeof(RepositoryException))]
            public async Task AttachmentRepository_PostAttachmentAsync_CtxException()
            {
                transManagerMock.Setup(accessor =>
                    accessor.ExecuteAsync<UpdateAttachmentsRequest, UpdateAttachmentsResponse>
                    (It.IsAny<UpdateAttachmentsRequest>())).ThrowsAsync(new ColleagueTransactionException("error"));

                try
                {
                    await repository.PostAttachmentAsync(attachment);
                }
                catch (RepositoryException e)
                {
                    Assert.IsTrue(e.Message == "Exception occurred creating attachment");
                    throw;
                }
            }
        }

        [TestClass]
        public class AttachmentRepository_PostAttachmentAndContentAsyncTests : AttachmentRepositoryTests
        {
            Domain.Base.Entities.Attachment attachment;

            [TestInitialize]
            public void AttachmentRepository_PostAttachmentAndContentAsync_Initialize()
            {
                Initialize();

                attachment = new Domain.Base.Entities.Attachment("f070516e-09f4-4232-af06-78391100e213", "COLLECTION1", "myattachment.pdf",
                    "application/pdf", 100, "0000001")
                {
                    Size = 10000,
                    Status = Domain.Base.Entities.AttachmentStatus.Active,
                    TagOne = "tag"
                };
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task AttachmentRepository_PostAttachmentAndContentAsync_NullAttachment()
            {
                await repository.PostAttachmentAndContentAsync(null, new MemoryStream());
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task AttachmentRepository_PostAttachmentAndContentAsync_NullStream()
            {
                await repository.PostAttachmentAndContentAsync(attachment, null);
            }
        }

        [TestClass]
        public class AttachmentRepository_PutAttachmentAsyncTests : AttachmentRepositoryTests
        {
            DataContracts.Attachments userData;
            Domain.Base.Entities.Attachment attachment;

            [TestInitialize]
            public void AttachmentRepository_PutAttachmentAsync_Initialize()
            {
                Initialize();

                userData = new DataContracts.Attachments()
                {
                    Recordkey = "f070516e-09f4-4232-af06-78391100e213",
                    AttCollectionId = "COLLECTION1",
                    AttContentType = "application/pdf",
                    AttName = "myattachment.pdf",
                    AttOwner = "0000001",
                    AttSize = 10000,
                    AttStatus = "Active",
                    AttTagOne = "tag",
                    AttachmentsAddopr = "USER",
                    AttachmentsChgopr = "USER2",
                    AttachmentsDelopr = "USER3"
                };

                attachment = new Domain.Base.Entities.Attachment("f070516e-09f4-4232-af06-78391100e213", "COLLECTION1", "myattachment.pdf",
                    "application/pdf", 100, "0000001")
                {
                    Size = 10000,
                    Status = Domain.Base.Entities.AttachmentStatus.Active,
                    TagOne = "tag"
                };
            }

            [TestMethod]
            public async Task AttachmentRepository_PutAttachmentAsync_Success()
            {
                var expectedResponse = new UpdateAttachmentsResponse
                {
                    ErrorMsg = string.Empty
                };

                transManagerMock.Setup(accessor =>
                    accessor.ExecuteAsync<UpdateAttachmentsRequest, UpdateAttachmentsResponse>
                    (It.IsAny<UpdateAttachmentsRequest>())).ReturnsAsync(expectedResponse);
                dataReaderMock.Setup(accessor =>
                    accessor.ReadRecordAsync<DataContracts.Attachments>
                    (It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(userData);

                var actual = await repository.PutAttachmentAsync(attachment);
                transManagerMock.Verify();
                dataReaderMock.Verify();
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task AttachmentRepository_PutAttachmentAsync_NullAttachment()
            {
                await repository.PutAttachmentAsync(null);
            }

            [TestMethod]
            [ExpectedException(typeof(RepositoryException))]
            public async Task AttachmentRepository_PutAttachmentAsync_NullResponse()
            {

                transManagerMock.Setup(accessor =>
                    accessor.ExecuteAsync<UpdateAttachmentsRequest, UpdateAttachmentsResponse>
                    (It.IsAny<UpdateAttachmentsRequest>())).ReturnsAsync(() => null);

                try
                {
                    await repository.PutAttachmentAsync(attachment);
                }
                catch (RepositoryException e)
                {
                    Assert.IsTrue(e.Message == "No response received updating attachment");
                    throw;
                }
            }

            [TestMethod]
            [ExpectedException(typeof(RepositoryException))]
            public async Task AttachmentRepository_PutAttachmentAsync_ErrorResponse()
            {
                var expectedResponse = new UpdateAttachmentsResponse
                {
                    ErrorMsg = "error occurred"
                };

                transManagerMock.Setup(accessor =>
                    accessor.ExecuteAsync<UpdateAttachmentsRequest, UpdateAttachmentsResponse>
                    (It.IsAny<UpdateAttachmentsRequest>())).ReturnsAsync(expectedResponse);

                try
                {
                    await repository.PutAttachmentAsync(attachment);
                }
                catch (RepositoryException e)
                {
                    Assert.IsTrue(e.Message == "Error occurred updating attachment");
                    throw;
                }
            }

            [TestMethod]
            [ExpectedException(typeof(RepositoryException))]
            public async Task AttachmentRepository_PutAttachmentAsync_CtxException()
            {
                transManagerMock.Setup(accessor =>
                    accessor.ExecuteAsync<UpdateAttachmentsRequest, UpdateAttachmentsResponse>
                    (It.IsAny<UpdateAttachmentsRequest>())).ThrowsAsync(new ColleagueTransactionException("error"));

                try
                {
                    await repository.PutAttachmentAsync(attachment);
                }
                catch (RepositoryException e)
                {
                    Assert.IsTrue(e.Message == "Exception occurred updating attachment");
                    throw;
                }
            }
        }

        [TestClass]
        public class AttachmentRepository_DeleteAttachmentAsyncTests : AttachmentRepositoryTests
        {
            Domain.Base.Entities.Attachment attachment;

            [TestInitialize]
            public void AttachmentRepository_DeleteAttachmentAsync_Initialize()
            {
                Initialize();

                attachment = new Domain.Base.Entities.Attachment("f070516e-09f4-4232-af06-78391100e213", "COLLECTION1", "myattachment.pdf",
                    "application/pdf", 100, "0000001")
                {
                    Size = 10000,
                    Status = Domain.Base.Entities.AttachmentStatus.Active,
                    TagOne = "tag"
                };
            }

            [TestMethod]
            public async Task AttachmentRepository_DeleteAttachmentAsync_Success()
            {
                var expectedResponse = new CrudAttachmentContentResponse
                {
                    ErrorMsg = string.Empty
                };

                transManagerMock.Setup(accessor =>
                    accessor.ExecuteAsync<CrudAttachmentContentRequest, CrudAttachmentContentResponse>
                    (It.IsAny<CrudAttachmentContentRequest>())).ReturnsAsync(expectedResponse);

                await repository.DeleteAttachmentAsync(attachment);
                transManagerMock.Verify();
                dataReaderMock.Verify();
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task AttachmentRepository_DeleteAttachmentAsync_NullAttachment()
            {
                await repository.DeleteAttachmentAsync(null);
            }

            [TestMethod]
            [ExpectedException(typeof(RepositoryException))]
            public async Task AttachmentRepository_DeleteAttachmentAsync_NullResponse()
            {

                transManagerMock.Setup(accessor =>
                    accessor.ExecuteAsync<CrudAttachmentContentRequest, CrudAttachmentContentResponse>
                    (It.IsAny<CrudAttachmentContentRequest>())).ReturnsAsync(() => null);

                try
                {
                    await repository.DeleteAttachmentAsync(attachment);
                }
                catch (RepositoryException e)
                {
                    Assert.IsTrue(e.Message == "No response received deleting attachment");
                    throw;
                }
            }

            [TestMethod]
            [ExpectedException(typeof(RepositoryException))]
            public async Task AttachmentRepository_DeleteAttachmentAsync_ErrorResponse()
            {
                var expectedResponse = new CrudAttachmentContentResponse
                {
                    ErrorMsg = "error occurred"
                };

                transManagerMock.Setup(accessor =>
                    accessor.ExecuteAsync<CrudAttachmentContentRequest, CrudAttachmentContentResponse>
                    (It.IsAny<CrudAttachmentContentRequest>())).ReturnsAsync(expectedResponse);

                try
                {
                    await repository.DeleteAttachmentAsync(attachment);
                }
                catch (RepositoryException e)
                {
                    Assert.IsTrue(e.Message == "Error occurred deleting attachment");
                    throw;
                }
            }

            [TestMethod]
            [ExpectedException(typeof(RepositoryException))]
            public async Task AttachmentRepository_DeleteAttachmentAsync_CtxException()
            {
                transManagerMock.Setup(accessor =>
                    accessor.ExecuteAsync<CrudAttachmentContentRequest, CrudAttachmentContentResponse>
                    (It.IsAny<CrudAttachmentContentRequest>())).ThrowsAsync(new ColleagueTransactionException("error"));

                try
                {
                    await repository.DeleteAttachmentAsync(attachment);
                }
                catch (RepositoryException e)
                {
                    Assert.IsTrue(e.Message == "Exception occurred deleting attachment");
                    throw;
                }
            }
        }
    }
}