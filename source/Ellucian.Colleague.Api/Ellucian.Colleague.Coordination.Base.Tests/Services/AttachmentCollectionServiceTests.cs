// Copyright 2019-2020 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Coordination.Base.Adapters;
using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Colleague.Coordination.Base.Tests.UserFactories;
using Ellucian.Colleague.Domain.Base;
using Ellucian.Colleague.Domain.Base.Entities;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.Base.Tests;
using Ellucian.Colleague.Domain.Entities;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Web.Adapters;
using Ellucian.Web.Security;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.Base.Tests.Services
{
    [TestClass]
    public class AttachmentCollectionServiceTests : GenericUserFactory
    {
        private class AttachmentCollectionUserFactory : ICurrentUserFactory
        {
            public ICurrentUser CurrentUser
            {
                get
                {
                    return new CurrentUser(new Claims()
                    {
                        PersonId = "0000001",
                        ControlId = "123",
                        Name = "Johnny",
                        SecurityToken = "321",
                        SessionTimeout = 30,
                        UserName = "Faculty",
                        Roles = new List<string>() { "Faculty", "Collection Admin" },
                        SessionFixationId = "abc123"
                    });
                }
            }
        }

        public Mock<IAdapterRegistry> adapterRegistryMock;
        public Mock<ILogger> loggerMock;
        public Mock<IRoleRepository> roleRepositoryMock;
        public ICurrentUserFactory currentUserFactory;
        public Mock<IAttachmentCollectionRepository> attachmentCollectionRepositoryMock;
        public TestAttachmentCollectionRepository testCollectionRepository;
        public Mock<IEncryptionKeyRepository> encrRepositoryMock;
        public AttachmentCollectionService collectionService;
        public IEnumerable<Domain.Entities.Role> roles;

        private EncrKey fakeEncrKey;

        public void BaseInitialize()
        {
            adapterRegistryMock = new Mock<IAdapterRegistry>();
            loggerMock = new Mock<ILogger>();
            // creating an attachment collection requires a permission
            var attachmentCollectionRole = new Domain.Entities.Role(21, "Collection Admin");
            attachmentCollectionRole.AddPermission(new Permission(BasePermissionCodes.CreateAttachmentCollection));
            roleRepositoryMock = new Mock<IRoleRepository>();
            roleRepositoryMock.Setup(role => role.Roles).Returns(new List<Domain.Entities.Role>() { attachmentCollectionRole });
            currentUserFactory = new AttachmentCollectionUserFactory();
            attachmentCollectionRepositoryMock = new Mock<IAttachmentCollectionRepository>();
            testCollectionRepository = new TestAttachmentCollectionRepository();
            encrRepositoryMock = new Mock<IEncryptionKeyRepository>();            
            roles = new List<Domain.Entities.Role>()
            {
                new Domain.Entities.Role(15, "Student"),
                new Domain.Entities.Role(19, "Professor"),
                new Domain.Entities.Role(30, "Advisor"),
                new Domain.Entities.Role(31, "Tester"),
                attachmentCollectionRole
            };
            fakeEncrKey = new EncrKey("7c655b4d-c425-4aff-af98-337004ec8cfe", "test-key",
                "testing", 1, EncrKeyStatus.Active);
        }

        public void BaseCleanup()
        {
            adapterRegistryMock = null;
            loggerMock = null;
            roleRepositoryMock = null;
            currentUserFactory = null;
            attachmentCollectionRepositoryMock = null;
            testCollectionRepository = null;
            collectionService = null;
            encrRepositoryMock = null;
        }

        public void BuildAttachmentCollectionService()
        {
            collectionService = new AttachmentCollectionService(adapterRegistryMock.Object, attachmentCollectionRepositoryMock.Object,
                encrRepositoryMock.Object, currentUserFactory, roleRepositoryMock.Object, loggerMock.Object);
        }

        [TestClass]
        public class GetAttachmentCollectionByIdAsyncTests : AttachmentCollectionServiceTests
        {
            private AttachmentCollectionEntityAdapter attachmentCollectionEntityToDtoAdapter;

            [TestInitialize]
            public void Initialize()
            {
                BaseInitialize();
                attachmentCollectionRepositoryMock.Setup(r => r.GetAttachmentCollectionByIdAsync(It.IsAny<string>()))
                    .Returns<string>((id) => testCollectionRepository.GetAttachmentCollectionByIdAsync(id));
                attachmentCollectionEntityToDtoAdapter = new AttachmentCollectionEntityAdapter(adapterRegistryMock.Object, loggerMock.Object);
                adapterRegistryMock.Setup(a => a.GetAdapter<AttachmentCollection, Dtos.Base.AttachmentCollection>()).Returns(attachmentCollectionEntityToDtoAdapter);
                roleRepositoryMock.Setup(r => r.GetRolesAsync()).ReturnsAsync(roles);
                BuildAttachmentCollectionService();
            }

            [TestCleanup]
            public void Cleanup()
            {
                BaseCleanup();
            }

            [TestMethod]
            public async Task AttachmentCollectionService_GetAttachmentCollectionByIdAsyncSuccess()
            {
                string expectedId = "COLLECTION1";
                var actual = await collectionService.GetAttachmentCollectionByIdAsync(expectedId);
                Assert.AreEqual(expectedId, actual.Id);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task AttachmentCollectionService_GetAttachmentCollectionByIdAsyncNullId()
            {
                await collectionService.GetAttachmentCollectionByIdAsync(null);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task AttachmentCollectionService_GetAttachmentCollectionByIdAsyncEmptyId()
            {
                await collectionService.GetAttachmentCollectionByIdAsync(string.Empty);
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task AttachmentCollectionService_GetAttachmentCollectionByIdAsyncNotFound()
            {
                await collectionService.GetAttachmentCollectionByIdAsync("does not exist");
            }

            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public async Task AttachmentCollectionService_GetAttachmentCollectionByIdAsyncNoPermissions()
            {
                await collectionService.GetAttachmentCollectionByIdAsync("COLLECTION3");
            }

            [TestMethod]
            [ExpectedException(typeof(Exception))]
            public async Task AttachmentCollectionService_GetAttachmentCollectionByIdAsyncNoRoleMatch()
            {
                var collection = await testCollectionRepository.GetAttachmentCollectionByIdAsync("COLLECTION1");
                // create fake roles
                foreach (var role in collection.Roles)
                {
                    role.Id = "fake role";
                }

                attachmentCollectionRepositoryMock.Setup(r => r.GetAttachmentCollectionByIdAsync(It.IsAny<string>()))
                    .ReturnsAsync(collection);
                BuildAttachmentCollectionService();

                try
                {
                    await collectionService.GetAttachmentCollectionByIdAsync(collection.Id);
                }
                catch (Exception e)
                {
                    Assert.AreEqual("Cannot convert role ID fake role to its title", e.Message);
                    throw;
                }
            }
        }

        [TestClass]
        public class GetAttachmentCollectionsByUserAsyncTests : AttachmentCollectionServiceTests
        {
            private AttachmentCollectionEntityAdapter attachmentCollectionEntityToDtoAdapter;

            [TestInitialize]
            public void Initialize()
            {
                BaseInitialize();
                attachmentCollectionRepositoryMock.Setup(r => r.GetAttachmentCollectionsByUserAsync(It.IsAny<string>(), It.IsAny<IEnumerable<string>>()))
                    .Returns<string, IEnumerable<string>>((id, r) => testCollectionRepository.GetAttachmentCollectionsByUserAsync(id, r));
                attachmentCollectionEntityToDtoAdapter = new AttachmentCollectionEntityAdapter(adapterRegistryMock.Object, loggerMock.Object);
                adapterRegistryMock.Setup(a => a.GetAdapter<AttachmentCollection, Dtos.Base.AttachmentCollection>()).Returns(attachmentCollectionEntityToDtoAdapter);
                roleRepositoryMock.Setup(r => r.GetRolesAsync()).ReturnsAsync(roles);
                BuildAttachmentCollectionService();
            }

            [TestCleanup]
            public void Cleanup()
            {
                BaseCleanup();
            }

            [TestMethod]
            public async Task AttachmentCollectionService_GetAttachmentCollectionsByUserAsyncSuccess()
            {
                var expected = new List<string>
                {
                    "COLLECTION1",
                    "COLLECTION2",
                    "COLLECTION4"
                };
                var actual = await collectionService.GetAttachmentCollectionsByUserAsync();
                Assert.AreEqual(expected.Count(), actual.Count());
                foreach (var expectedId in expected)
                {
                    Assert.AreEqual(expectedId, actual.ToList().Where(a => a.Id == expectedId).First().Id);
                }
            }
        }

        [TestClass]
        public class PostAttachmentCollectionAsyncTests : AttachmentCollectionServiceTests
        {
            private AttachmentCollectionEntityAdapter attachmentCollectionEntityToDtoAdapter;
            private AttachmentCollectionDtoAdapter attachmentCollectionDtoToEntityAdapter;

            [TestInitialize]
            public void Initialize()
            {
                BaseInitialize();
                attachmentCollectionRepositoryMock.Setup(r => r.PostAttachmentCollectionAsync(It.IsAny<AttachmentCollection>()))
                    .Returns<AttachmentCollection>((a) => testCollectionRepository.PostAttachmentCollectionAsync(a));
                attachmentCollectionRepositoryMock.Setup(r => r.GetAttachmentCollectionByIdAsync(It.IsAny<string>()))
                    .Returns<string>((id) => testCollectionRepository.GetAttachmentCollectionByIdAsync(id));
                attachmentCollectionEntityToDtoAdapter = new AttachmentCollectionEntityAdapter(adapterRegistryMock.Object, loggerMock.Object);
                adapterRegistryMock.Setup(a => a.GetAdapter<AttachmentCollection, Dtos.Base.AttachmentCollection>()).Returns(attachmentCollectionEntityToDtoAdapter);
                attachmentCollectionDtoToEntityAdapter = new AttachmentCollectionDtoAdapter(adapterRegistryMock.Object, loggerMock.Object);
                adapterRegistryMock.Setup(a => a.GetAdapter<Dtos.Base.AttachmentCollection, AttachmentCollection>()).Returns(attachmentCollectionDtoToEntityAdapter);
                roleRepositoryMock.Setup(r => r.GetRolesAsync()).ReturnsAsync(roles);
                encrRepositoryMock.Setup(e => e.GetKeyAsync(fakeEncrKey.Id)).ReturnsAsync(fakeEncrKey);
                BuildAttachmentCollectionService();
            }

            [TestCleanup]
            public void Cleanup()
            {
                BaseCleanup();
            }

            [TestMethod]
            public async Task AttachmentCollectionService_PostAttachmentCollectionAsyncSuccess()
            {
                var expected = new Dtos.Base.AttachmentCollection();
                expected.Id = "NEW_COLLECTION";
                expected.Name = "Testing";
                expected.EncryptionKeyId = fakeEncrKey.Id;

                var actual = await collectionService.PostAttachmentCollectionAsync(expected);
                Assert.AreEqual(expected.Id, actual.Id);
            }

            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public async Task AttachmentCollectionService_PostAttachmentCollectionAsyncNoPermission()
            {
                // use a user factory without the create collection permission
                collectionService = new AttachmentCollectionService(adapterRegistryMock.Object, attachmentCollectionRepositoryMock.Object,
                    encrRepositoryMock.Object, new UserFactory(), roleRepositoryMock.Object, loggerMock.Object);

                var expected = new Dtos.Base.AttachmentCollection();
                expected.Id = "NEW_COLLECTION";
                expected.Name = "Testing";
                expected.EncryptionKeyId = fakeEncrKey.Id;

                await collectionService.PostAttachmentCollectionAsync(expected);
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task AttachmentCollectionService_PostAttachmentCollectionAsyncInvalidEncryptionKey()
            {
                var expected = new Dtos.Base.AttachmentCollection();
                expected.Id = "NEW_COLLECTION";
                expected.Name = "Testing";
                expected.EncryptionKeyId = "fake key";

                await collectionService.PostAttachmentCollectionAsync(expected);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task AttachmentCollectionService_PostAttachmentCollectionAsyncNullCollection()
            {
                await collectionService.PostAttachmentCollectionAsync(null);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public async Task AttachmentCollectionService_PostAttachmentCollectionAsyncCollectionExists()
            {
                var collection = await testCollectionRepository.GetAttachmentCollectionByIdAsync("COLLECTION1");
                var expected = attachmentCollectionEntityToDtoAdapter.MapToType(collection);
                // need the role titles
                foreach(var role in expected.Roles)
                {
                    role.Id = roles.Where(r => r.Id.ToString() == role.Id).FirstOrDefault().Title;
                }

                await collectionService.PostAttachmentCollectionAsync(expected);
            }

            [TestMethod]
            [ExpectedException(typeof(Exception))]
            public async Task AttachmentCollectionService_PostAttachmentCollectionAsyncNoRoles()
            {
                var collection = await testCollectionRepository.GetAttachmentCollectionByIdAsync("COLLECTION1");
                var expected = attachmentCollectionEntityToDtoAdapter.MapToType(collection);
                // need the role titles
                foreach (var role in expected.Roles)
                {
                    role.Id = roles.Where(r => r.Id.ToString() == role.Id).FirstOrDefault().Title;
                }

                // force no roles
                roleRepositoryMock.Setup(r => r.GetRolesAsync()).ReturnsAsync(() => null);
                BuildAttachmentCollectionService();

                try
                {
                    await collectionService.PostAttachmentCollectionAsync(expected);
                }
                catch (Exception e)
                {
                    Assert.AreEqual("No roles found for conversion of titles to IDs", e.Message);
                    throw;
                }
            }

            [TestMethod]
            [ExpectedException(typeof(Exception))]
            public async Task AttachmentCollectionService_PostAttachmentCollectionAsyncNoRoleMatch()
            {
                var collection = await testCollectionRepository.GetAttachmentCollectionByIdAsync("COLLECTION1");
                var expected = attachmentCollectionEntityToDtoAdapter.MapToType(collection);
                // create fake roles
                foreach (var role in expected.Roles)
                {
                    role.Id = "fake role";
                }

                try
                {
                    await collectionService.PostAttachmentCollectionAsync(expected);
                }
                catch (Exception e)
                {
                    Assert.AreEqual("Cannot convert role title fake role to its ID", e.Message);
                    throw;
                }
            }
        }

        [TestClass]
        public class PutAttachmentCollectionAsyncTests : AttachmentCollectionServiceTests
        {
            private AttachmentCollectionEntityAdapter attachmentCollectionEntityToDtoAdapter;
            private AttachmentCollectionDtoAdapter attachmentCollectionDtoToEntityAdapter;

            [TestInitialize]
            public void Initialize()
            {
                BaseInitialize();
                attachmentCollectionRepositoryMock.Setup(r => r.PutAttachmentCollectionAsync(It.IsAny<AttachmentCollection>()))
                    .Returns<AttachmentCollection>((a) => testCollectionRepository.PutAttachmentCollectionAsync(a));
                attachmentCollectionRepositoryMock.Setup(r => r.GetAttachmentCollectionByIdAsync(It.IsAny<string>()))
                    .Returns<string>((id) => testCollectionRepository.GetAttachmentCollectionByIdAsync(id));
                attachmentCollectionEntityToDtoAdapter = new AttachmentCollectionEntityAdapter(adapterRegistryMock.Object, loggerMock.Object);
                adapterRegistryMock.Setup(a => a.GetAdapter<AttachmentCollection, Dtos.Base.AttachmentCollection>()).Returns(attachmentCollectionEntityToDtoAdapter);
                attachmentCollectionDtoToEntityAdapter = new AttachmentCollectionDtoAdapter(adapterRegistryMock.Object, loggerMock.Object);
                adapterRegistryMock.Setup(a => a.GetAdapter<Dtos.Base.AttachmentCollection, AttachmentCollection>()).Returns(attachmentCollectionDtoToEntityAdapter);
                roleRepositoryMock.Setup(r => r.GetRolesAsync()).ReturnsAsync(roles);
                encrRepositoryMock.Setup(e => e.GetKeyAsync(fakeEncrKey.Id)).ReturnsAsync(fakeEncrKey);
                BuildAttachmentCollectionService();
            }

            [TestCleanup]
            public void Cleanup()
            {
                BaseCleanup();
            }

            [TestMethod]
            public async Task AttachmentCollectionService_PutAttachmentCollectionAsyncSuccess()
            {
                var collection = await testCollectionRepository.GetAttachmentCollectionByIdAsync("COLLECTION4");
                var expected = attachmentCollectionEntityToDtoAdapter.MapToType(collection);
                // need the role titles
                foreach (var role in expected.Roles)
                {
                    role.Id = roles.Where(r => r.Id.ToString() == role.Id).FirstOrDefault().Title;
                }
                expected.EncryptionKeyId = fakeEncrKey.Id;

                var actual = await collectionService.PutAttachmentCollectionAsync(expected.Id, expected);
                Assert.AreEqual(expected.Id, actual.Id);
                foreach (var role in actual.Roles)
                {
                    Assert.AreEqual(role.Id, expected.Roles.Where(r => r.Id.ToString() == role.Id).FirstOrDefault().Id);
                }
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task AttachmentCollectionService_PutAttachmentCollectionAsyncInvalidEncryptionKey()
            {
                var collection = await testCollectionRepository.GetAttachmentCollectionByIdAsync("COLLECTION4");
                var expected = attachmentCollectionEntityToDtoAdapter.MapToType(collection);
                // need the role titles
                foreach (var role in expected.Roles)
                {
                    role.Id = roles.Where(r => r.Id.ToString() == role.Id).FirstOrDefault().Title;
                }
                expected.EncryptionKeyId = "fake key";

                await collectionService.PutAttachmentCollectionAsync(expected.Id, expected);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task AttachmentCollectionService_PutAttachmentCollectionAsyncNullIdCollection()
            {
                var collection = await testCollectionRepository.GetAttachmentCollectionByIdAsync("COLLECTION1");
                var expected = attachmentCollectionEntityToDtoAdapter.MapToType(collection);

                await collectionService.PutAttachmentCollectionAsync(null, expected);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task AttachmentCollectionService_PutAttachmentCollectionAsyncEmptyIdCollection()
            {
                var collection = await testCollectionRepository.GetAttachmentCollectionByIdAsync("COLLECTION1");
                var expected = attachmentCollectionEntityToDtoAdapter.MapToType(collection);

                await collectionService.PutAttachmentCollectionAsync(string.Empty, expected);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task AttachmentCollectionService_PutAttachmentCollectionAsyncNullCollection()
            {
                await collectionService.PutAttachmentCollectionAsync("test", null);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public async Task AttachmentCollectionService_PutAttachmentCollectionAsyncIdMismatchCollection()
            {
                var collection = await testCollectionRepository.GetAttachmentCollectionByIdAsync("COLLECTION1");
                var expected = attachmentCollectionEntityToDtoAdapter.MapToType(collection);

                await collectionService.PutAttachmentCollectionAsync("invalid ID", expected);
            }

            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public async Task AttachmentCollectionService_PutAttachmentCollectionAsyncNoPermission()
            {
                var collection = await testCollectionRepository.GetAttachmentCollectionByIdAsync("COLLECTION3");
                var expected = attachmentCollectionEntityToDtoAdapter.MapToType(collection);
                // need the role titles
                foreach (var role in expected.Roles)
                {
                    role.Id = roles.Where(r => r.Id.ToString() == role.Id).FirstOrDefault().Title;
                }

                await collectionService.PutAttachmentCollectionAsync(expected.Id, expected);
            }

            [TestMethod]
            [ExpectedException(typeof(Exception))]
            public async Task AttachmentCollectionService_PutAttachmentCollectionAsyncNoRoles()
            {
                var collection = await testCollectionRepository.GetAttachmentCollectionByIdAsync("COLLECTION1");
                var expected = attachmentCollectionEntityToDtoAdapter.MapToType(collection);
                // need the role titles
                foreach (var role in expected.Roles)
                {
                    role.Id = roles.Where(r => r.Id.ToString() == role.Id).FirstOrDefault().Title;
                }

                // force no roles
                roleRepositoryMock.Setup(r => r.GetRolesAsync()).ReturnsAsync(() => null);
                BuildAttachmentCollectionService();

                try
                {
                    await collectionService.PutAttachmentCollectionAsync(expected.Id, expected);
                }
                catch (Exception e)
                {
                    Assert.AreEqual("No roles found for conversion of titles to IDs", e.Message);
                    throw;
                }
            }

            [TestMethod]
            [ExpectedException(typeof(Exception))]
            public async Task AttachmentCollectionService_PutAttachmentCollectionAsyncNoRoleMatch()
            {
                var collection = await testCollectionRepository.GetAttachmentCollectionByIdAsync("COLLECTION1");
                var expected = attachmentCollectionEntityToDtoAdapter.MapToType(collection);
                // create fake roles
                foreach (var role in expected.Roles)
                {
                    role.Id = "fake role";
                }

                try
                {
                    await collectionService.PutAttachmentCollectionAsync(expected.Id, expected);
                }
                catch (Exception e)
                {
                    Assert.AreEqual("Cannot convert role title fake role to its ID", e.Message);
                    throw;
                }
            }
        }

        [TestClass]
        public class GetEffectivePermissionsAsyncTests : AttachmentCollectionServiceTests
        {
            private AttachmentCollectionEntityAdapter attachmentCollectionEntityToDtoAdapter;

            [TestInitialize]
            public void Initialize()
            {
                BaseInitialize();
                attachmentCollectionRepositoryMock.Setup(r => r.GetAttachmentCollectionByIdAsync(It.IsAny<string>()))
                    .Returns<string>((id) => testCollectionRepository.GetAttachmentCollectionByIdAsync(id));
                attachmentCollectionEntityToDtoAdapter = new AttachmentCollectionEntityAdapter(adapterRegistryMock.Object, loggerMock.Object);
                adapterRegistryMock.Setup(a => a.GetAdapter<AttachmentCollection, Dtos.Base.AttachmentCollection>()).Returns(attachmentCollectionEntityToDtoAdapter);
                roleRepositoryMock.Setup(r => r.GetRolesAsync()).ReturnsAsync(roles);
                BuildAttachmentCollectionService();
            }

            [TestCleanup]
            public void Cleanup()
            {
                BaseCleanup();
            }

            [TestMethod]
            public async Task AttachmentCollectionService_GetEffectivePermissionsAsyncSuccess()
            {
                var actual = await collectionService.GetEffectivePermissionsAsync("COLLECTION1");
                Assert.AreEqual(true, actual.CanCreateAttachments);
                Assert.AreEqual(true, actual.CanDeleteAttachments);
                Assert.AreEqual(true, actual.CanUpdateAttachments);
                Assert.AreEqual(true, actual.CanViewAttachments);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task AttachmentCollectionService_GetEffectivePermissionsAsyncNullId()
            {
                await collectionService.GetEffectivePermissionsAsync(null);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task AttachmentCollectionService_GetEffectivePermissionsAsyncEmptyId()
            {
                await collectionService.GetEffectivePermissionsAsync(string.Empty);
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task AttachmentCollectionService_GetEffectivePermissionsAsyncNoCollection()
            {
                await collectionService.GetEffectivePermissionsAsync("FAKE_COLLECTION");
            }

            [TestMethod]
            public async Task AttachmentCollectionService_GetEffectivePermissionsAsyncInactiveCollection()
            {
                var actual = await collectionService.GetEffectivePermissionsAsync("INACTIVE_COLLECTION");
                Assert.AreEqual(false, actual.CanCreateAttachments);
                Assert.AreEqual(false, actual.CanDeleteAttachments);
                Assert.AreEqual(false, actual.CanUpdateAttachments);
                Assert.AreEqual(false, actual.CanViewAttachments);
            }
        }
    }
}