// Copyright 2019 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Coordination.Base.Adapters;
using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Colleague.Coordination.Base.Tests.UserFactories;
using Ellucian.Colleague.Domain.Base.Entities;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.Base.Tests;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Web.Adapters;
using Ellucian.Web.Security;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.Base.Tests.Services
{
    [TestClass]
    public class AttachmentServiceTests : GenericUserFactory
    {
        public Mock<IAdapterRegistry> adapterRegistryMock;
        public Mock<IRoleRepository> roleRepositoryMock;
        public Mock<ILogger> loggerMock;
        public ICurrentUserFactory currentUserFactory;
        public Mock<IAttachmentRepository> attachmentRepositoryMock;
        public TestAttachmentRepository testRepository;
        public Mock<IAttachmentCollectionRepository> attachmentCollectionRepositoryMock;
        public TestAttachmentCollectionRepository testCollectionRepository;
        public Mock<IEncryptionKeyRepository> encrRepositoryMock;
        public AttachmentService attachmentService;

        private EncrKey fakeEncrKey;

        public void BaseInitialize()
        {
            adapterRegistryMock = new Mock<IAdapterRegistry>();
            loggerMock = new Mock<ILogger>();
            roleRepositoryMock = new Mock<IRoleRepository>();
            currentUserFactory = new UserFactory();
            attachmentRepositoryMock = new Mock<IAttachmentRepository>();
            attachmentCollectionRepositoryMock = new Mock<IAttachmentCollectionRepository>();
            testRepository = new TestAttachmentRepository();
            testCollectionRepository = new TestAttachmentCollectionRepository();
            encrRepositoryMock = new Mock<IEncryptionKeyRepository>();
            fakeEncrKey = new EncrKey("7c655b4d-c425-4aff-af98-337004ec8cfe", "test-key",
                "testing", 1, EncrKeyStatus.Active);
        }

        public void BaseCleanup()
        {
            adapterRegistryMock = null;
            loggerMock = null;
            roleRepositoryMock = null;
            currentUserFactory = null;
            attachmentRepositoryMock = null;
            attachmentCollectionRepositoryMock = null;
            testRepository = null;
            testCollectionRepository = null;
            attachmentService = null;
            encrRepositoryMock = null;
        }

        public void BuildAttachmentService()
        {
            attachmentService = new AttachmentService(adapterRegistryMock.Object, attachmentRepositoryMock.Object,
                attachmentCollectionRepositoryMock.Object, currentUserFactory, encrRepositoryMock.Object, roleRepositoryMock.Object,
                loggerMock.Object);
        }

        [TestClass]
        public class GetAttachmentsAsyncTests : AttachmentServiceTests
        {
            private AttachmentEntityAdapter attachmentEntityToDtoAdapter;

            [TestInitialize]
            public void Initialize()
            {
                BaseInitialize();
                attachmentRepositoryMock.Setup(r => r.GetAttachmentsAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                    .Returns<string, string, string>((o, cId, tO) => testRepository.GetAttachmentsAsync(o, cId, tO));
                attachmentCollectionRepositoryMock.Setup(r => r.GetAttachmentCollectionsByIdAsync(It.IsAny<IEnumerable<string>>()))
                    .Returns<IEnumerable<string>>((ids) => testCollectionRepository.GetAttachmentCollectionsByIdAsync(ids));
                attachmentEntityToDtoAdapter = new AttachmentEntityAdapter(adapterRegistryMock.Object, loggerMock.Object);
                adapterRegistryMock.Setup(a => a.GetAdapter<Domain.Base.Entities.Attachment, Dtos.Base.Attachment>()).Returns(attachmentEntityToDtoAdapter);
                BuildAttachmentService();
            }

            [TestCleanup]
            public void Cleanup()
            {
                BaseCleanup();
            }

            [TestMethod]
            public async Task AttachmentService_GetAttachmentsAsyncSuccess()
            {
                var expected = new List<string>()
                {
                    "f7bbd166-aa4b-4bc6-b343-de0cace2cfa2",
                    "ccf84c4a-351f-481f-8646-28154d0867c8",
                    "9bc1e856-c9c9-44c7-920c-87bfc16896ab"
                };
                var actual = await attachmentService.GetAttachmentsAsync("0000001", string.Empty, string.Empty);
                Assert.AreEqual(expected.Count(), actual.Count());
                foreach (var expectedId in expected)
                {
                    Assert.AreEqual(expectedId, actual.ToList().Where(a => a.Id == expectedId).First().Id);
                }
            }

            [TestMethod]
            public async Task AttachmentService_GetAttachmentsAsyncCollectionSuccess()
            {
                var expected = new List<string>()
                {
                    "25d87724-6906-4867-9463-e6f699e17f0e",
                    "97bec6ac-3729-46bb-a3ef-952c71d857ea",
                    "9bc1e856-c9c9-44c7-920c-87bfc16896ab",
                    "000b9366-f9d1-4c44-a2b3-2cc6a6496e52",
                    "3c23db9d-d58f-4e17-af4b-050afcaa4f00"
                };
                var actual = await attachmentService.GetAttachmentsAsync(string.Empty, "COLLECTION2", string.Empty);
                Assert.AreEqual(expected.Count(), actual.Count());
                foreach (var expectedId in expected)
                {
                    Assert.AreEqual(expectedId, actual.ToList().Where(a => a.Id == expectedId).First().Id);
                }
            }

            [TestMethod]
            public async Task AttachmentService_GetAttachmentsAsyncOwnerAndCollectionSuccess()
            {
                var expected = new List<string>()
                {
                    "25d87724-6906-4867-9463-e6f699e17f0e"
                };
                var actual = await attachmentService.GetAttachmentsAsync("0004111", "COLLECTION2", string.Empty);
                Assert.AreEqual(expected.Count(), actual.Count());
                foreach (var expectedId in expected)
                {
                    Assert.AreEqual(expectedId, actual.ToList().Where(a => a.Id == expectedId).First().Id);
                }
            }

            [TestMethod]
            public async Task AttachmentService_GetAttachmentsAsyncCollectionAndTagOneSuccess()
            {
                var expected = new List<string>()
                {
                    "25d87724-6906-4867-9463-e6f699e17f0e",
                    "97bec6ac-3729-46bb-a3ef-952c71d857ea",
                    "9bc1e856-c9c9-44c7-920c-87bfc16896ab",
                    "000b9366-f9d1-4c44-a2b3-2cc6a6496e52",
                    "3c23db9d-d58f-4e17-af4b-050afcaa4f00"
                };
                var actual = await attachmentService.GetAttachmentsAsync(string.Empty, "COLLECTION2", "tag1");
                Assert.AreEqual(expected.Count(), actual.Count());
                foreach (var expectedId in expected)
                {
                    Assert.AreEqual(expectedId, actual.ToList().Where(a => a.Id == expectedId).First().Id);
                }
            }

            [TestMethod]
            public async Task AttachmentService_GetAttachmentsAsyncOwnerAndCollectionNoPermission()
            {
                var actual = await attachmentService.GetAttachmentsAsync(string.Empty, "COLLECTION3", string.Empty);
                Assert.AreEqual(0, actual.Count());
            }
        }

        [TestClass]
        public class GetAttachmentContentAsyncTests : AttachmentServiceTests
        {
            private AttachmentEntityAdapter attachmentEntityToDtoAdapter;

            [TestInitialize]
            public void Initialize()
            {
                BaseInitialize();
                attachmentRepositoryMock.Setup(r => r.GetAttachmentContentAsync(It.IsAny<Attachment>()))
                    .Returns<Attachment>((a) => testRepository.GetAttachmentContentAsync(a));
                attachmentRepositoryMock.Setup(r => r.GetAttachmentByIdWithEncrMetadataAsync(It.IsAny<string>()))
                    .Returns<string>((id) => testRepository.GetAttachmentByIdWithEncrMetadataAsync(id));
                attachmentCollectionRepositoryMock.Setup(r => r.GetAttachmentCollectionsByIdAsync(It.IsAny<IEnumerable<string>>()))
                    .Returns<IEnumerable<string>>((ids) => testCollectionRepository.GetAttachmentCollectionsByIdAsync(ids));
                attachmentEntityToDtoAdapter = new AttachmentEntityAdapter(adapterRegistryMock.Object, loggerMock.Object);
                encrRepositoryMock.Setup(e => e.GetKeyAsync(fakeEncrKey.Id)).ReturnsAsync(fakeEncrKey);
                adapterRegistryMock.Setup(a => a.GetAdapter<Attachment, Dtos.Base.Attachment>()).Returns(attachmentEntityToDtoAdapter);
                BuildAttachmentService();
            }

            [TestCleanup]
            public void Cleanup()
            {
                BaseCleanup();
            }

            [TestMethod]
            public async Task AttachmentService_GetAttachmentContentAsyncSuccess()
            {
                string expectedId = "f7bbd166-aa4b-4bc6-b343-de0cace2cfa2";
                var actual = await attachmentService.GetAttachmentContentAsync(expectedId);
                Assert.AreEqual(expectedId, actual.Item1.Id);
                Assert.AreEqual("C:\testpath", actual.Item2);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task AttachmentService_GetAttachmentContentAsyncIdNull()
            {
                await attachmentService.GetAttachmentContentAsync(null);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task AttachmentService_GetAttachmentContentAsyncIdEmpty()
            {
                await attachmentService.GetAttachmentContentAsync(string.Empty);
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task AttachmentService_GetAttachmentContentAsyncNoAttachmentFound()
            {
                try
                {
                    await attachmentService.GetAttachmentContentAsync("does not exist");
                }
                catch (KeyNotFoundException knfe)
                {
                    Assert.AreEqual("Attachment not found", knfe.Message);
                    throw;
                }       
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task AttachmentService_GetAttachmentContentAsyncNotActive()
            {
                try
                {
                    await attachmentService.GetAttachmentContentAsync("c239b1f1-8883-4a74-bf8c-3b853a46c782");
                }
                catch (KeyNotFoundException knfe)
                {
                    Assert.AreEqual("The attachment is not active", knfe.Message);
                    throw;
                }
            }

            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public async Task AttachmentService_GetAttachmentContentAsyncNoPermissions()
            {
                await attachmentService.GetAttachmentContentAsync("2977b1d4-6d37-46a6-bf2e-bcabdc56745d");
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task AttachmentService_GetAttachmentContentAsyncEmptyFilePath()
            {
                attachmentRepositoryMock.Setup(r => r.GetAttachmentContentAsync(It.IsAny<Attachment>()))
                    .Returns(Task.FromResult(string.Empty));
                BuildAttachmentService();
                await attachmentService.GetAttachmentContentAsync("297c4460-5955-4be5-a6f0-c28f786c4894");
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public async Task AttachmentService_GetAttachmentContentAsyncInactiveKey()
            {
                try
                {
                    // make the key inactive
                    fakeEncrKey.Status = EncrKeyStatus.Inactive;
                    // assign the attachment the key
                    var attachment = await testRepository.GetAttachmentByIdNoEncrMetadataAsync("f7bbd166-aa4b-4bc6-b343-de0cace2cfa2");
                    attachment.EncrKeyId = fakeEncrKey.Id;
                    attachmentRepositoryMock.Setup(r => r.GetAttachmentByIdWithEncrMetadataAsync(It.IsAny<string>()))
                        .ReturnsAsync(attachment);
                    BuildAttachmentService();

                    string expectedId = "f7bbd166-aa4b-4bc6-b343-de0cace2cfa2";
                    await attachmentService.GetAttachmentContentAsync(expectedId);
                }
                catch (ArgumentException e)
                {
                    Assert.AreEqual("The encryption key is not active", e.Message);
                    throw;
                }
            }
        }

        [TestClass]
        public class PostAttachmentAsyncTests : AttachmentServiceTests
        {
            private AttachmentEntityAdapter attachmentEntityToDtoAdapter;
            private AttachmentDtoAdapter attachmentDtoToEntityAdapter;

            [TestInitialize]
            public void Initialize()
            {
                BaseInitialize();
                attachmentRepositoryMock.Setup(r => r.PostAttachmentAsync(It.IsAny<Attachment>()))
                    .Returns<Attachment>((a) => testRepository.PostAttachmentAsync(a));
                attachmentCollectionRepositoryMock.Setup(r => r.GetAttachmentCollectionsByIdAsync(It.IsAny<IEnumerable<string>>()))
                    .Returns<IEnumerable<string>>((ids) => testCollectionRepository.GetAttachmentCollectionsByIdAsync(ids));
                attachmentEntityToDtoAdapter = new AttachmentEntityAdapter(adapterRegistryMock.Object, loggerMock.Object);
                adapterRegistryMock.Setup(a => a.GetAdapter<Attachment, Dtos.Base.Attachment>()).Returns(attachmentEntityToDtoAdapter);
                attachmentDtoToEntityAdapter = new AttachmentDtoAdapter(adapterRegistryMock.Object, loggerMock.Object);
                adapterRegistryMock.Setup(a => a.GetAdapter<Dtos.Base.Attachment, Attachment>()).Returns(attachmentDtoToEntityAdapter);
                encrRepositoryMock.Setup(e => e.GetKeyAsync(fakeEncrKey.Id)).ReturnsAsync(fakeEncrKey);
                BuildAttachmentService();
            }

            [TestCleanup]
            public void Cleanup()
            {
                BaseCleanup();
            }

            [TestMethod]
            public async Task AttachmentService_PostAttachmentAsyncSuccess()
            {
                var attachment = await testRepository.GetAttachmentByIdNoEncrMetadataAsync("297c4460-5955-4be5-a6f0-c28f786c4894");
                var attachmentEncryption = new Dtos.Base.AttachmentEncryption(fakeEncrKey.Id, "AES256",
                    Convert.FromBase64String("zqc0PJyMqfgHS2txd56RDi7dfMoI76elZZyyntMFGs2jcogIVXSU01Y9hc"
                    + "LezN0KLf8v3crobdiPkHWSCtV+Xl+XM5vhJpfiVVqEdMgG7xXwvdY5rl8gMh298wVSfmCQaFu5z9oMkqn39NJFpr9amf"
                    + "MOANLrzMUwFdX/jxi2lJJmiirCt/1uNXA6/cQdRfHBtvT0tur+4En5TpSY84DkevOcnDTaKiPa3ZIHM1SZTR9T0mKY59"
                    + "0Vyo97YJFOuR6W7Q+hVuA62lTqAxUOc19nwKJRCwJ5K6dxtZqaGhbPQu9ubFrvGHPYNrhApjYBlqBdk2yYGKYRy7TIDI"
                    + "2ZxLIvqA=="),
                    Convert.FromBase64String("d7ztE4TNClbBVp4tbRb94w==")
                    );
                attachment.Name = "AttachmentService_PostAttachmentAsyncSuccess";
                var expected = attachmentEntityToDtoAdapter.MapToType(attachment);
                var actual = await attachmentService.PostAttachmentAsync(expected, attachmentEncryption);
                Assert.AreEqual(expected.Name, actual.Name);
                Assert.AreEqual(expected.Owner, actual.Owner);
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task AttachmentService_PostAttachmentAsyncInvalidEncryptionKey()
            {
                var attachment = await testRepository.GetAttachmentByIdNoEncrMetadataAsync("297c4460-5955-4be5-a6f0-c28f786c4894");
                var attachmentEncryption = new Dtos.Base.AttachmentEncryption("invalid key", "AES256",
                    Convert.FromBase64String("zqc0PJyMqfgHS2txd56RDi7dfMoI76elZZyyntMFGs2jcogIVXSU01Y9hc"
                    + "LezN0KLf8v3crobdiPkHWSCtV+Xl+XM5vhJpfiVVqEdMgG7xXwvdY5rl8gMh298wVSfmCQaFu5z9oMkqn39NJFpr9amf"
                    + "MOANLrzMUwFdX/jxi2lJJmiirCt/1uNXA6/cQdRfHBtvT0tur+4En5TpSY84DkevOcnDTaKiPa3ZIHM1SZTR9T0mKY59"
                    + "0Vyo97YJFOuR6W7Q+hVuA62lTqAxUOc19nwKJRCwJ5K6dxtZqaGhbPQu9ubFrvGHPYNrhApjYBlqBdk2yYGKYRy7TIDI"
                    + "2ZxLIvqA=="),
                    Convert.FromBase64String("d7ztE4TNClbBVp4tbRb94w==")
                    );
                var expected = attachmentEntityToDtoAdapter.MapToType(attachment);

                await attachmentService.PostAttachmentAsync(expected, attachmentEncryption);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task AttachmentService_PostAttachmentAsyncNullAttachment()
            {
                await attachmentService.PostAttachmentAsync(null, null);
            }

            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public async Task AttachmentService_PostAttachmentAsyncNoPermissions()
            {
                var attachment = await testRepository.GetAttachmentByIdNoEncrMetadataAsync("2977b1d4-6d37-46a6-bf2e-bcabdc56745d");
                var attachmentDto = attachmentEntityToDtoAdapter.MapToType(attachment);

                await attachmentService.PostAttachmentAsync(attachmentDto, null);
            }
        }

        [TestClass]
        public class PostAttachmentAndContentAsyncTests : AttachmentServiceTests
        {
            private AttachmentEntityAdapter attachmentEntityToDtoAdapter;
            private AttachmentDtoAdapter attachmentDtoToEntityAdapter;

            [TestInitialize]
            public void Initialize()
            {
                BaseInitialize();
                attachmentRepositoryMock.Setup(r => r.PostAttachmentAndContentAsync(It.IsAny<Attachment>(), It.IsAny<Stream>()))
                    .Returns<Attachment, Stream>((a, s) => testRepository.PostAttachmentAndContentAsync(a, s));
                attachmentCollectionRepositoryMock.Setup(r => r.GetAttachmentCollectionsByIdAsync(It.IsAny<IEnumerable<string>>()))
                    .Returns<IEnumerable<string>>((ids) => testCollectionRepository.GetAttachmentCollectionsByIdAsync(ids));
                attachmentEntityToDtoAdapter = new AttachmentEntityAdapter(adapterRegistryMock.Object, loggerMock.Object);
                adapterRegistryMock.Setup(a => a.GetAdapter<Domain.Base.Entities.Attachment, Dtos.Base.Attachment>()).Returns(attachmentEntityToDtoAdapter);
                attachmentDtoToEntityAdapter = new AttachmentDtoAdapter(adapterRegistryMock.Object, loggerMock.Object);
                adapterRegistryMock.Setup(a => a.GetAdapter<Dtos.Base.Attachment, Domain.Base.Entities.Attachment>()).Returns(attachmentDtoToEntityAdapter);
                encrRepositoryMock.Setup(e => e.GetKeyAsync(fakeEncrKey.Id)).ReturnsAsync(fakeEncrKey);
                BuildAttachmentService();
            }

            [TestCleanup]
            public void Cleanup()
            {
                BaseCleanup();
            }

            [TestMethod]
            public async Task AttachmentService_PostAttachmentAndContentAsyncSuccess()
            {
                var attachment = await testRepository.GetAttachmentByIdNoEncrMetadataAsync("297c4460-5955-4be5-a6f0-c28f786c4894");
                var attachmentEncryption = new Dtos.Base.AttachmentEncryption(fakeEncrKey.Id, "AES256",
                    Convert.FromBase64String("zqc0PJyMqfgHS2txd56RDi7dfMoI76elZZyyntMFGs2jcogIVXSU01Y9hc"
                    + "LezN0KLf8v3crobdiPkHWSCtV+Xl+XM5vhJpfiVVqEdMgG7xXwvdY5rl8gMh298wVSfmCQaFu5z9oMkqn39NJFpr9amf"
                    + "MOANLrzMUwFdX/jxi2lJJmiirCt/1uNXA6/cQdRfHBtvT0tur+4En5TpSY84DkevOcnDTaKiPa3ZIHM1SZTR9T0mKY59"
                    + "0Vyo97YJFOuR6W7Q+hVuA62lTqAxUOc19nwKJRCwJ5K6dxtZqaGhbPQu9ubFrvGHPYNrhApjYBlqBdk2yYGKYRy7TIDI"
                    + "2ZxLIvqA=="),
                    Convert.FromBase64String("d7ztE4TNClbBVp4tbRb94w=="));
                var expected = attachmentEntityToDtoAdapter.MapToType(attachment);

                string attachmentContent = "This is a test string";
                attachment.Size = attachmentContent.Length;
                var actual = await attachmentService.PostAttachmentAndContentAsync(expected, attachmentEncryption, 
                    new MemoryStream(Encoding.ASCII.GetBytes(attachmentContent)));
                Assert.AreEqual(expected.Name, actual.Name);
                Assert.AreEqual(expected.Owner, actual.Owner);
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task AttachmentService_PostAttachmentAndContentAsyncInvalidEncryptionKey()
            {
                var attachment = await testRepository.GetAttachmentByIdNoEncrMetadataAsync("297c4460-5955-4be5-a6f0-c28f786c4894");
                var attachmentEncryption = new Dtos.Base.AttachmentEncryption("invalid key", "AES256",
                    Convert.FromBase64String("zqc0PJyMqfgHS2txd56RDi7dfMoI76elZZyyntMFGs2jcogIVXSU01Y9hc"
                    + "LezN0KLf8v3crobdiPkHWSCtV+Xl+XM5vhJpfiVVqEdMgG7xXwvdY5rl8gMh298wVSfmCQaFu5z9oMkqn39NJFpr9amf"
                    + "MOANLrzMUwFdX/jxi2lJJmiirCt/1uNXA6/cQdRfHBtvT0tur+4En5TpSY84DkevOcnDTaKiPa3ZIHM1SZTR9T0mKY59"
                    + "0Vyo97YJFOuR6W7Q+hVuA62lTqAxUOc19nwKJRCwJ5K6dxtZqaGhbPQu9ubFrvGHPYNrhApjYBlqBdk2yYGKYRy7TIDI"
                    + "2ZxLIvqA=="),
                    Convert.FromBase64String("d7ztE4TNClbBVp4tbRb94w=="));
                var expected = attachmentEntityToDtoAdapter.MapToType(attachment);

                string attachmentContent = "This is a test string";
                attachment.Size = attachmentContent.Length;
                await attachmentService.PostAttachmentAndContentAsync(expected, attachmentEncryption,
                    new MemoryStream(Encoding.ASCII.GetBytes(attachmentContent)));
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task AttachmentService_PostAttachmentAndContentAsyncNullAttachment()
            {
                await attachmentService.PostAttachmentAndContentAsync(null, null, new MemoryStream());
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task AttachmentService_PostAttachmentAndContentAsyncNullStream()
            {
                var attachment = await testRepository.GetAttachmentByIdNoEncrMetadataAsync("297c4460-5955-4be5-a6f0-c28f786c4894");
                var attachmentDto = attachmentEntityToDtoAdapter.MapToType(attachment);

                await attachmentService.PostAttachmentAndContentAsync(attachmentDto, null, null);
            }

            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public async Task AttachmentService_PostAttachmentAndContentAsyncNoPermissions()
            {
                var attachment = await testRepository.GetAttachmentByIdNoEncrMetadataAsync("2977b1d4-6d37-46a6-bf2e-bcabdc56745d");
                var attachmentDto = attachmentEntityToDtoAdapter.MapToType(attachment);

                string attachmentContent = "This is a test string";
                attachment.Size = attachmentContent.Length;
                await attachmentService.PostAttachmentAndContentAsync(attachmentDto, null,
                    new MemoryStream(Encoding.ASCII.GetBytes(attachmentContent)));
            }
        }

        [TestClass]
        public class PutAttachmentAsyncTests : AttachmentServiceTests
        {
            private AttachmentEntityAdapter attachmentEntityToDtoAdapter;
            private AttachmentDtoAdapter attachmentDtoToEntityAdapter;

            [TestInitialize]
            public void Initialize()
            {
                BaseInitialize();
                attachmentRepositoryMock.Setup(r => r.PutAttachmentAsync(It.IsAny<Attachment>()))
                    .Returns<Attachment>((a) => testRepository.PutAttachmentAsync(a));
                attachmentRepositoryMock.Setup(r => r.GetAttachmentByIdWithEncrMetadataAsync(It.IsAny<string>()))
                    .Returns<string>((id) => testRepository.GetAttachmentByIdWithEncrMetadataAsync(id));
                attachmentCollectionRepositoryMock.Setup(r => r.GetAttachmentCollectionsByIdAsync(It.IsAny<IEnumerable<string>>()))
                    .Returns<IEnumerable<string>>((ids) => testCollectionRepository.GetAttachmentCollectionsByIdAsync(ids));
                attachmentEntityToDtoAdapter = new AttachmentEntityAdapter(adapterRegistryMock.Object, loggerMock.Object);
                adapterRegistryMock.Setup(a => a.GetAdapter<Domain.Base.Entities.Attachment, Dtos.Base.Attachment>()).Returns(attachmentEntityToDtoAdapter);
                attachmentDtoToEntityAdapter = new AttachmentDtoAdapter(adapterRegistryMock.Object, loggerMock.Object);
                adapterRegistryMock.Setup(a => a.GetAdapter<Dtos.Base.Attachment, Domain.Base.Entities.Attachment>()).Returns(attachmentDtoToEntityAdapter);
                encrRepositoryMock.Setup(e => e.GetKeyAsync(fakeEncrKey.Id)).ReturnsAsync(fakeEncrKey);
                BuildAttachmentService();
            }

            [TestCleanup]
            public void Cleanup()
            {
                BaseCleanup();
            }

            [TestMethod]
            public async Task AttachmentService_PutAttachmentAsyncSuccess()
            {
                var attachment = await testRepository.GetAttachmentByIdWithEncrMetadataAsync("297c4460-5955-4be5-a6f0-c28f786c4894");
                attachment.EncrKeyId = fakeEncrKey.Id;
                attachment.EncrContentKey = Convert.FromBase64String("zqc0PJyMqfgHS2txd56RDi7dfMoI76elZZyyntMFGs2jcogIVXSU01Y9hc"
                    + "LezN0KLf8v3crobdiPkHWSCtV+Xl+XM5vhJpfiVVqEdMgG7xXwvdY5rl8gMh298wVSfmCQaFu5z9oMkqn39NJFpr9amf"
                    + "MOANLrzMUwFdX/jxi2lJJmiirCt/1uNXA6/cQdRfHBtvT0tur+4En5TpSY84DkevOcnDTaKiPa3ZIHM1SZTR9T0mKY59"
                    + "0Vyo97YJFOuR6W7Q+hVuA62lTqAxUOc19nwKJRCwJ5K6dxtZqaGhbPQu9ubFrvGHPYNrhApjYBlqBdk2yYGKYRy7TIDI"
                    + "2ZxLIvqA==");
                attachment.EncrType = "AES256";
                attachment.EncrIV = Convert.FromBase64String("d7ztE4TNClbBVp4tbRb94w==");
                // rename
                attachment.Name = "new_name.pdf";
                var expected = attachmentEntityToDtoAdapter.MapToType(attachment);

                var actual = await attachmentService.PutAttachmentAsync(expected.Id, expected);
                Assert.AreEqual(expected.Id, actual.Id);
                Assert.AreEqual(expected.Name, actual.Name);
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task AttachmentService_PutAttachmentAsyncInvalidEncryptionKey()
            {
                var attachment = await testRepository.GetAttachmentByIdWithEncrMetadataAsync("297c4460-5955-4be5-a6f0-c28f786c4894");
                attachment.EncrKeyId = "invalid key";
                attachment.EncrContentKey = Convert.FromBase64String("zqc0PJyMqfgHS2txd56RDi7dfMoI76elZZyyntMFGs2jcogIVXSU01Y9hc"
                    + "LezN0KLf8v3crobdiPkHWSCtV+Xl+XM5vhJpfiVVqEdMgG7xXwvdY5rl8gMh298wVSfmCQaFu5z9oMkqn39NJFpr9amf"
                    + "MOANLrzMUwFdX/jxi2lJJmiirCt/1uNXA6/cQdRfHBtvT0tur+4En5TpSY84DkevOcnDTaKiPa3ZIHM1SZTR9T0mKY59"
                    + "0Vyo97YJFOuR6W7Q+hVuA62lTqAxUOc19nwKJRCwJ5K6dxtZqaGhbPQu9ubFrvGHPYNrhApjYBlqBdk2yYGKYRy7TIDI"
                    + "2ZxLIvqA==");
                attachment.EncrType = "AES256";
                attachment.EncrIV = Convert.FromBase64String("d7ztE4TNClbBVp4tbRb94w==");
                var expected = attachmentEntityToDtoAdapter.MapToType(attachment);

                await attachmentService.PutAttachmentAsync(expected.Id, expected);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task AttachmentService_PutAttachmentAsyncNullId()
            {
                var attachment = await testRepository.GetAttachmentByIdWithEncrMetadataAsync("297c4460-5955-4be5-a6f0-c28f786c4894");
                var expected = attachmentEntityToDtoAdapter.MapToType(attachment);

                await attachmentService.PutAttachmentAsync(null, expected);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task AttachmentService_PutAttachmentAsyncEmptyId()
            {
                var attachment = await testRepository.GetAttachmentByIdWithEncrMetadataAsync("297c4460-5955-4be5-a6f0-c28f786c4894");
                var expected = attachmentEntityToDtoAdapter.MapToType(attachment);

                await attachmentService.PutAttachmentAsync(string.Empty, expected);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task AttachmentService_PutAttachmentAsyncNullAttachment()
            {
                await attachmentService.PutAttachmentAsync("test", null);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public async Task AttachmentService_PutAttachmentAsyncMismatchId()
            {
                var attachment = await testRepository.GetAttachmentByIdWithEncrMetadataAsync("297c4460-5955-4be5-a6f0-c28f786c4894");
                var expected = attachmentEntityToDtoAdapter.MapToType(attachment);

                await attachmentService.PutAttachmentAsync("mismatch id", expected);
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task AttachmentService_PutAttachmentAsyncAttachmentDoesNotExist()
            {
                var attachment = await testRepository.GetAttachmentByIdWithEncrMetadataAsync("297c4460-5955-4be5-a6f0-c28f786c4894");
                var expected = attachmentEntityToDtoAdapter.MapToType(attachment);
                expected.Id = "does not exist";

                await attachmentService.PutAttachmentAsync(expected.Id, expected);
            }

            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public async Task AttachmentService_PutAttachmentAsyncNoPermissions()
            {
                var attachment = await testRepository.GetAttachmentByIdWithEncrMetadataAsync("2977b1d4-6d37-46a6-bf2e-bcabdc56745d");
                var attachmentDto = attachmentEntityToDtoAdapter.MapToType(attachment);

                await attachmentService.PutAttachmentAsync(attachmentDto.Id, attachmentDto);
            }
        }

        [TestClass]
        public class DeleteAttachmentAsyncTests : AttachmentServiceTests
        {
            [TestInitialize]
            public void Initialize()
            {
                BaseInitialize();
                attachmentRepositoryMock.Setup(r => r.DeleteAttachmentAsync(It.IsAny<Attachment>()))
                    .Returns<Attachment>((a) => testRepository.DeleteAttachmentAsync(a));
                attachmentRepositoryMock.Setup(r => r.GetAttachmentByIdNoEncrMetadataAsync(It.IsAny<string>()))
                    .Returns<string>((id) => testRepository.GetAttachmentByIdNoEncrMetadataAsync(id));
                attachmentCollectionRepositoryMock.Setup(r => r.GetAttachmentCollectionsByIdAsync(It.IsAny<IEnumerable<string>>()))
                    .Returns<IEnumerable<string>>((ids) => testCollectionRepository.GetAttachmentCollectionsByIdAsync(ids));                
                BuildAttachmentService();
            }

            [TestCleanup]
            public void Cleanup()
            {
                BaseCleanup();
            }

            [TestMethod]
            public async Task AttachmentService_DeleteAttachmentAsyncSuccess()
            {
                await attachmentService.DeleteAttachmentAsync("297c4460-5955-4be5-a6f0-c28f786c4894");
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task AttachmentService_DeleteAttachmentAsyncNullId()
            {
                await attachmentService.DeleteAttachmentAsync(null);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task AttachmentService_DeleteAttachmentAsyncEmptyId()
            {
                await attachmentService.DeleteAttachmentAsync(string.Empty);
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task AttachmentService_DeleteAttachmentAsyncNoAttachment()
            {
                await attachmentService.DeleteAttachmentAsync("does not exist");
            }

            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public async Task AttachmentService_DeleteAttachmentAsyncNoPermissions()
            {
                await attachmentService.DeleteAttachmentAsync("2977b1d4-6d37-46a6-bf2e-bcabdc56745d");
            }
        }
    }
}