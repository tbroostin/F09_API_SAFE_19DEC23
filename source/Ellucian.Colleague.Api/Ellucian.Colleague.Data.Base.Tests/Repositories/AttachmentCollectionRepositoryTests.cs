// Copyright 2019-2020 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Data.Base.Repositories;
using Ellucian.Colleague.Data.Base.Transactions;
using Ellucian.Colleague.Domain.Base.Entities;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Data.Colleague.Exceptions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Data.Base.Tests.Repositories
{
    public class AttachmentCollectionRepositoryTests : BaseRepositorySetup
    {
        AttachmentCollectionRepository repository;

        [TestInitialize]
        public void Initialize()
        {
            // Initialize Mock framework
            MockInitialize();

            // Build the test repository
            repository = new AttachmentCollectionRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object);
        }

        [TestCleanup]
        public void Cleanup()
        {
            repository = null;
        }

        [TestClass]
        public class AttachmentCollectionRepository_GetAttachmentCollectionByIdAsyncTests : AttachmentCollectionRepositoryTests
        {
            DataContracts.AttachmentCollections userData;

            [TestInitialize]
            public void AttachmentCollectionRepository_GetAttachmentCollectionByIdAsync_Initialize()
            {
                userData = new DataContracts.AttachmentCollections()
                {
                    Recordkey = "COLLECTION1",
                    AtcolAllowedContentTypes = new List<string>() { "PDF", "JPEG" },
                    AtcolDescription = "my collection 1",
                    AtcolMaxAttachmentSize = 10000,
                    AtcolName = "collection #1",
                    AtcolOwnerActions = new List<string>() { "U", "D" },
                    AtcolOwner = "0000001",
                    AtcolRetainDuration = "P5Y",
                    AtcolStatus = "A",
                    AtcolRolesId = new List<string>() { "41", "41", "43", "44" },
                    AtcolRolesActions = new List<string>() { "V", "U", "V", "V" },
                    AtcolUsersId = new List<string>() { "0000001", "0000001", "0000001", "999999" },
                    AtcolUsersActions = new List<string>() { "V", "U", "D", "V" },
                    AtcolEncrKeyId = "7c655b4d-c425-4aff-af98-337004ec8cfe"
                };
                userData.buildAssociations();
            }

            [TestMethod]
            public async Task AttachmentCollectionRepository_GetAttachmentCollectionByIdAsync_Success()
            {
                dataReaderMock.Setup(accessor =>
                    accessor.ReadRecordAsync<DataContracts.AttachmentCollections>
                    (It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(userData);

                var expectedRoles = new List<AttachmentCollectionIdentity>()
                {
                    new AttachmentCollectionIdentity("41", AttachmentCollectionIdentityType.Role, new List<AttachmentAction>() { AttachmentAction.View, AttachmentAction.Update }),
                    new AttachmentCollectionIdentity("43", AttachmentCollectionIdentityType.Role, new List<AttachmentAction>() { AttachmentAction.View }),
                    new AttachmentCollectionIdentity("44", AttachmentCollectionIdentityType.Role, new List<AttachmentAction>() { AttachmentAction.View })
                };

                var expectedUsers = new List<AttachmentCollectionIdentity>()
                {
                    new AttachmentCollectionIdentity("0000001", AttachmentCollectionIdentityType.User, new List<AttachmentAction>() { AttachmentAction.View,
                        AttachmentAction.Update, AttachmentAction.Delete }),
                    new AttachmentCollectionIdentity("999999", AttachmentCollectionIdentityType.User, new List<AttachmentAction>() { AttachmentAction.View })
                };

                var actual = await repository.GetAttachmentCollectionByIdAsync("COLLECTION1");
                Assert.AreEqual(userData.Recordkey, actual.Id);
                Assert.AreEqual(userData.AtcolAllowedContentTypes.Count(), actual.AllowedContentTypes.Count());
                Assert.AreEqual(userData.AtcolDescription, actual.Description);
                Assert.AreEqual(userData.AtcolAllowedContentTypes.Count(), actual.AllowedContentTypes.Count());
                Assert.AreEqual(userData.AtcolMaxAttachmentSize, actual.MaxAttachmentSize);
                Assert.AreEqual(userData.AtcolName, actual.Name);
                Assert.AreEqual(userData.AtcolOwnerActions.Count(), actual.AttachmentOwnerActions.Count());
                Assert.AreEqual(userData.AtcolOwner, actual.Owner);
                Assert.AreEqual(userData.AtcolRetainDuration, actual.RetentionDuration);
                Assert.AreEqual("Active", actual.Status.ToString());
                Assert.AreEqual(expectedRoles.Count(), actual.Roles.Count());
                foreach (var expectedRole in expectedRoles)
                {
                    var actualRole = actual.Roles.Where(r => r.Id == expectedRole.Id).First();
                    Assert.AreEqual(expectedRole.Id, actualRole.Id);
                    Assert.AreEqual(expectedRole.Type, actualRole.Type);
                    CollectionAssert.AreEqual(expectedRole.Actions.ToList(), actualRole.Actions.ToList());
                }
                Assert.AreEqual(expectedUsers.Count(), actual.Users.Count());
                foreach (var expectedUser in expectedUsers)
                {
                    var actualUser = actual.Users.Where(r => r.Id == expectedUser.Id).First();
                    Assert.AreEqual(expectedUser.Id, actualUser.Id);
                    Assert.AreEqual(expectedUser.Type, actualUser.Type);
                    CollectionAssert.AreEqual(expectedUser.Actions.ToList(), actualUser.Actions.ToList());
                }
                Assert.AreEqual(userData.AtcolEncrKeyId, actual.EncryptionKeyId);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task AttachmentCollectionRepository_GetAttachmentCollectionByIdAsync_NullId()
            {
                await repository.GetAttachmentCollectionByIdAsync(null);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task AttachmentCollectionRepository_GetAttachmentCollectionByIdAsync_EmptyId()
            {
                await repository.GetAttachmentCollectionByIdAsync(string.Empty);
            }

            [TestMethod]
            public async Task AttachmentCollectionRepository_GetAttachmentCollectionByIdAsync_Exception()
            {
                dataReaderMock.Setup(accessor =>
                    accessor.ReadRecordAsync<DataContracts.AttachmentCollections>
                    (It.IsAny<string>(), It.IsAny<bool>())).Throws(new Exception());

                string expected = "Error occurred retrieving attachment collection metadata";
                string actual = string.Empty;
                try
                {
                    await repository.GetAttachmentCollectionByIdAsync("COLLECTION1");
                }
                catch (Exception e)
                {
                    actual = e.Message;
                }

                Assert.AreEqual(expected, actual);
            }
        }

        [TestClass]
        public class AttachmentCollectionRepository_GetAttachmentCollectionsByIdAsyncTests : AttachmentCollectionRepositoryTests
        {
            Collection<DataContracts.AttachmentCollections> userData;

            [TestInitialize]
            public void AttachmentCollectionRepository_GetAttachmentCollectionsByIdAsync_Initialize()
            {
                userData = new Collection<DataContracts.AttachmentCollections>();
                var collection1 = new DataContracts.AttachmentCollections()
                {
                    Recordkey = "COLLECTION1",
                    AtcolAllowedContentTypes = new List<string>() { "PDF", "JPEG" },
                    AtcolDescription = "my collection 1",
                    AtcolMaxAttachmentSize = 10000,
                    AtcolName = "collection #1",
                    AtcolOwnerActions = new List<string>() { "U", "D" },
                    AtcolOwner = "0000001",
                    AtcolRetainDuration = "P5Y",
                    AtcolStatus = "A",
                    AtcolRolesId = new List<string>() { "41", "41", "43", "44" },
                    AtcolRolesActions = new List<string>() { "V", "U", "V", "V" },
                    AtcolUsersId = new List<string>() { "0000001", "0000001", "0000001", "999999" },
                    AtcolUsersActions = new List<string>() { "V", "U", "D", "V" }
                };
                collection1.buildAssociations();
                userData.Add(collection1);

                var collection2 = new DataContracts.AttachmentCollections()
                {
                    Recordkey = "COLLECTION2",
                    AtcolAllowedContentTypes = new List<string>() { "PDF", "GIF" },
                    AtcolDescription = "my collection 2",
                    AtcolMaxAttachmentSize = 10000,
                    AtcolName = "collection #1",
                    AtcolOwnerActions = new List<string>() { "U", "D" },
                    AtcolOwner = "0000001",
                    AtcolRetainDuration = "P5Y",
                    AtcolStatus = "A",
                    AtcolRolesId = new List<string>() { "41", "41", "43", "44" },
                    AtcolRolesActions = new List<string>() { "V", "U", "V", "V" },
                    AtcolUsersId = new List<string>() { "0000001", "0000001", "0000001", "999999" },
                    AtcolUsersActions = new List<string>() { "V", "U", "D", "V" }
                };
                collection1.buildAssociations();
                userData.Add(collection2);
            }

            [TestMethod]
            public async Task AttachmentCollectionRepository_GetAttachmentCollectionsByIdAsync_Success()
            {
                dataReaderMock.Setup(a => a.BulkReadRecordAsync<DataContracts.AttachmentCollections>
                    (It.IsAny<string[]>(), It.IsAny<bool>())).ReturnsAsync(userData);

                var actual = await repository.GetAttachmentCollectionsByIdAsync(new List<string>() { "COLLECTION1", "COLLECTION2" });
                Assert.AreEqual(userData.Count(), actual.Count());
                foreach (var collection in userData)
                {
                    Assert.AreEqual(collection.Recordkey, actual.Where(a => a.Id == collection.Recordkey).First().Id);
                }
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task AttachmentCollectionRepository_GetAttachmentCollectionsByIdAsync_NullId()
            {
                await repository.GetAttachmentCollectionsByIdAsync(null);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task AttachmentCollectionRepository_GetAttachmentCollectionsByIdAsync_EmptyId()
            {
                await repository.GetAttachmentCollectionsByIdAsync(new List<string>());
            }

            [TestMethod]
            public async Task AttachmentCollectionRepository_GetAttachmentCollectionsByIdAsync_Exception()
            {
                dataReaderMock.Setup(a => a.BulkReadRecordAsync<DataContracts.AttachmentCollections>
                    (It.IsAny<string[]>(), It.IsAny<bool>())).Throws(new Exception());

                string expected = "Error occurred retrieving bulk attachment collection metadata";
                string actual = string.Empty;
                try
                {
                    await repository.GetAttachmentCollectionsByIdAsync(new List<string>() { "COLLECTION1", "COLLECTION2" });
                }
                catch (Exception e)
                {
                    actual = e.Message;
                }

                Assert.AreEqual(expected, actual);
            }
        }

        [TestClass]
        public class AttachmentCollectionRepository_GetAttachmentCollectionsByUserAsyncTests : AttachmentCollectionRepositoryTests
        {
            Collection<DataContracts.AttachmentCollections> userData;

            [TestInitialize]
            public void AttachmentCollectionRepository_GetAttachmentCollectionsByUserAsync_Initialize()
            {
                userData = new Collection<DataContracts.AttachmentCollections>();
                var collection1 = new DataContracts.AttachmentCollections()
                {
                    Recordkey = "COLLECTION1",
                    AtcolAllowedContentTypes = new List<string>() { "PDF", "JPEG" },
                    AtcolDescription = "my collection 1",
                    AtcolMaxAttachmentSize = 10000,
                    AtcolName = "collection #1",
                    AtcolOwnerActions = new List<string>() { "U", "D" },
                    AtcolOwner = "0000001",
                    AtcolRetainDuration = "P5Y",
                    AtcolStatus = "A",
                    AtcolRolesId = new List<string>() { "41", "41", "43", "44" },
                    AtcolRolesActions = new List<string>() { "V", "U", "V", "V" },
                    AtcolUsersId = new List<string>() { "0000001", "0000001", "0000001", "999999" },
                    AtcolUsersActions = new List<string>() { "V", "U", "D", "V" }
                };
                collection1.buildAssociations();
                userData.Add(collection1);

                var collection2 = new DataContracts.AttachmentCollections()
                {
                    Recordkey = "COLLECTION2",
                    AtcolAllowedContentTypes = new List<string>() { "PDF", "GIF" },
                    AtcolDescription = "my collection 2",
                    AtcolMaxAttachmentSize = 10000,
                    AtcolName = "collection #1",
                    AtcolOwnerActions = new List<string>() { "U", "D" },
                    AtcolOwner = "0000001",
                    AtcolRetainDuration = "P5Y",
                    AtcolStatus = "A",
                    AtcolRolesId = new List<string>() { "41", "41", "43", "44" },
                    AtcolRolesActions = new List<string>() { "V", "U", "V", "V" },
                    AtcolUsersId = new List<string>() { "0000001", "0000001", "0000001", "999999" },
                    AtcolUsersActions = new List<string>() { "V", "U", "D", "V" }
                };
                collection1.buildAssociations();
                userData.Add(collection2);
            }

            [TestMethod]
            public async Task AttachmentCollectionRepository_GetAttachmentCollectionsByUserAsync_Success()
            {
                dataReaderMock.Setup(a => a.BulkReadRecordAsync<DataContracts.AttachmentCollections>
                    (It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(userData);

                var actual = await repository.GetAttachmentCollectionsByUserAsync("0000001", new List<string>() { "41", "33" });
                Assert.AreEqual(userData.Count(), actual.Count());
                foreach (var collection in userData)
                {
                    Assert.AreEqual(collection.Recordkey, actual.Where(a => a.Id == collection.Recordkey).First().Id);
                }
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task AttachmentCollectionRepository_GetAttachmentCollectionsByUserAsync_NullId()
            {
                await repository.GetAttachmentCollectionsByUserAsync(null, null);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task AttachmentCollectionRepository_GetAttachmentCollectionsByUserAsync_EmptyId()
            {
                await repository.GetAttachmentCollectionsByUserAsync(string.Empty, new List<string>());
            }

            [TestMethod]
            public async Task AttachmentCollectionRepository_GetAttachmentCollectionsByUserAsync_Exception()
            {
                dataReaderMock.Setup(a => a.BulkReadRecordAsync<DataContracts.AttachmentCollections>
                    (It.IsAny<string>(), It.IsAny<bool>())).Throws(new Exception());

                string expected = "Error occurred retrieving attachment collection metadata by user";
                string actual = string.Empty;
                try
                {
                    await repository.GetAttachmentCollectionsByUserAsync("0000001", new List<string>() { "41", "33" });
                }
                catch (Exception e)
                {
                    actual = e.Message;
                }

                Assert.AreEqual(expected, actual);
            }
        }

        [TestClass]
        public class AttachmentCollectionRepository_PostAttachmentCollectionAsyncTests : AttachmentCollectionRepositoryTests
        {
            DataContracts.AttachmentCollections userData;
            AttachmentCollection collection;

            [TestInitialize]
            public void AttachmentCollectionRepository_PostAttachmentCollectionAsync_Initialize()
            {
                userData = new DataContracts.AttachmentCollections()
                {
                    Recordkey = "COLLECTION1",
                    AtcolAllowedContentTypes = new List<string>() { "PDF", "BITMAP" },
                    AtcolDescription = "my collection 1",
                    AtcolMaxAttachmentSize = 10000,
                    AtcolName = "collection #1",
                    AtcolOwnerActions = new List<string>() { "U", "D" },
                    AtcolOwner = "0000001",
                    AtcolRetainDuration = "P5Y",
                    AtcolStatus = "A",
                    AtcolRolesId = new List<string>() { "41", "41", "43", "44" },
                    AtcolRolesActions = new List<string>() { "V", "U", "V", "V" },
                    AtcolUsersId = new List<string>() { "0000001", "0000001", "0000001", "999999" },
                    AtcolUsersActions = new List<string>() { "V", "U", "D", "V" }
                };
                userData.buildAssociations();

                collection = new AttachmentCollection(userData.Recordkey, userData.AtcolName, userData.AtcolOwner)
                {
                    AllowedContentTypes = new List<string>() { "application/pdf", "image/bmp", "image/TIFF" },
                    Description = userData.AtcolDescription,
                    MaxAttachmentSize = 10000,
                    AttachmentOwnerActions = new List<AttachmentOwnerAction>() { AttachmentOwnerAction.Update, AttachmentOwnerAction.Delete },
                    RetentionDuration = userData.AtcolRetainDuration,
                    Status = AttachmentCollectionStatus.Active,
                    Roles = new List<AttachmentCollectionIdentity>()
                    {
                        new AttachmentCollectionIdentity("41", AttachmentCollectionIdentityType.Role, new List<AttachmentAction>()
                        {
                            AttachmentAction.View, AttachmentAction.Update
                        }),
                        new AttachmentCollectionIdentity("43", AttachmentCollectionIdentityType.Role, new List<AttachmentAction>()
                        {
                            AttachmentAction.View
                        }),
                        new AttachmentCollectionIdentity("44", AttachmentCollectionIdentityType.Role, new List<AttachmentAction>()
                        {
                            AttachmentAction.View
                        })
                    },
                    Users = new List<AttachmentCollectionIdentity>()
                    {
                        new AttachmentCollectionIdentity("0000001", AttachmentCollectionIdentityType.User, new List<AttachmentAction>()
                            {
                                AttachmentAction.View, AttachmentAction.Update, AttachmentAction.Delete
                            }),
                        new AttachmentCollectionIdentity("999999", AttachmentCollectionIdentityType.User, new List<AttachmentAction>()
                            {
                                AttachmentAction.View
                            })
                    }
                };
            }

            [TestMethod]
            public async Task AttachmentCollectionRepository_PostAttachmentCollectionAsync_Success()
            {
                var expectedResponse = new UpdateAttachmentCollectionsResponse
                {
                    ErrorMsg = string.Empty
                };

                transManagerMock.Setup(accessor =>
                    accessor.ExecuteAsync<UpdateAttachmentCollectionsRequest, UpdateAttachmentCollectionsResponse>
                    (It.IsAny<UpdateAttachmentCollectionsRequest>())).ReturnsAsync(expectedResponse);
                dataReaderMock.Setup(accessor =>
                    accessor.ReadRecordAsync<DataContracts.AttachmentCollections>
                    (It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(userData);

                var response = await repository.PostAttachmentCollectionAsync(collection);
                transManagerMock.Verify();
                dataReaderMock.Verify();
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task AttachmentCollectionRepository_PostAttachmentCollectionAsync_NullCollection()
            {
                await repository.PostAttachmentCollectionAsync(null);
            }

            [TestMethod]
            [ExpectedException(typeof(RepositoryException))]
            public async Task AttachmentCollectionRepository_PostAttachmentCollectionAsync_NullResponse()
            {
                transManagerMock.Setup(accessor =>
                    accessor.ExecuteAsync<UpdateAttachmentCollectionsRequest, UpdateAttachmentCollectionsResponse>
                    (It.IsAny<UpdateAttachmentCollectionsRequest>())).ReturnsAsync(null);

                try
                {
                    await repository.PostAttachmentCollectionAsync(collection);
                }
                catch (RepositoryException e)
                {
                    Assert.IsTrue(e.Message == "No response received creating attachment collection");
                    throw;
                }
            }

            [TestMethod]
            [ExpectedException(typeof(RepositoryException))]
            public async Task AttachmentCollectionRepository_PostAttachmentCollectionAsync_ErrorResponse()
            {
                var expectedResponse = new UpdateAttachmentCollectionsResponse
                {
                    ErrorMsg = "error occurred"
                };

                transManagerMock.Setup(accessor =>
                    accessor.ExecuteAsync<UpdateAttachmentCollectionsRequest, UpdateAttachmentCollectionsResponse>
                    (It.IsAny<UpdateAttachmentCollectionsRequest>())).ReturnsAsync(expectedResponse);

                try
                {
                    await repository.PostAttachmentCollectionAsync(collection);
                }
                catch (RepositoryException e)
                {
                    Assert.IsTrue(e.Message == "Error occurred creating attachment collection");
                    throw;
                }
            }

            [TestMethod]
            [ExpectedException(typeof(RepositoryException))]
            public async Task AttachmentCollectionRepository_PostAttachmentCollectionAsync_CtxException()
            {
                transManagerMock.Setup(accessor =>
                    accessor.ExecuteAsync<UpdateAttachmentCollectionsRequest, UpdateAttachmentCollectionsResponse>
                    (It.IsAny<UpdateAttachmentCollectionsRequest>())).ThrowsAsync(new ColleagueTransactionException("error"));

                try
                {
                    await repository.PostAttachmentCollectionAsync(collection);
                }
                catch (RepositoryException e)
                {
                    Assert.IsTrue(e.Message == "Exception occurred creating attachment collection");
                    throw;
                }
            }
        }

        [TestClass]
        public class AttachmentCollectionRepository_PutAttachmentCollectionAsyncTests : AttachmentCollectionRepositoryTests
        {
            DataContracts.AttachmentCollections userData;

            [TestInitialize]
            public void AttachmentCollectionRepository_PutAttachmentCollectionAsync_Initialize()
            {
                userData = new DataContracts.AttachmentCollections()
                {
                    Recordkey = "COLLECTION1",
                    AtcolAllowedContentTypes = new List<string>() { "PDF", "TIFF" },
                    AtcolDescription = "my collection 1",
                    AtcolMaxAttachmentSize = 10000,
                    AtcolName = "collection #1",
                    AtcolOwnerActions = new List<string>() { "U", "D" },
                    AtcolOwner = "0000001",
                    AtcolRetainDuration = "P5Y",
                    AtcolStatus = "A",
                    AtcolRolesId = new List<string>() { "41", "41", "43", "44" },
                    AtcolRolesActions = new List<string>() { "V", "U", "V", "V" },
                    AtcolUsersId = new List<string>() { "0000001", "0000001", "0000001", "999999" },
                    AtcolUsersActions = new List<string>() { "V", "U", "D", "V" }
                };
                userData.buildAssociations();
            }

            [TestMethod]
            public async Task AttachmentCollectionRepository_PutAttachmentCollectionAsync_Success()
            {
                var expectedResponse = new UpdateAttachmentCollectionsResponse
                {
                    ErrorMsg = string.Empty
                };

                transManagerMock.Setup(accessor =>
                    accessor.ExecuteAsync<UpdateAttachmentCollectionsRequest, UpdateAttachmentCollectionsResponse>
                    (It.IsAny<UpdateAttachmentCollectionsRequest>())).ReturnsAsync(expectedResponse);
                dataReaderMock.Setup(accessor =>
                    accessor.ReadRecordAsync<DataContracts.AttachmentCollections>
                    (It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(userData);

                var response = await repository.PutAttachmentCollectionAsync(
                    new AttachmentCollection(userData.Recordkey, userData.AtcolName, userData.AtcolOwner));
                transManagerMock.Verify();
                dataReaderMock.Verify();
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task AttachmentCollectionRepository_PutAttachmentCollectionAsync_NullCollection()
            {
                await repository.PutAttachmentCollectionAsync(null);
            }

            [TestMethod]
            [ExpectedException(typeof(RepositoryException))]
            public async Task AttachmentCollectionRepository_PutAttachmentCollectionAsync_NullResponse()
            {
                transManagerMock.Setup(accessor =>
                    accessor.ExecuteAsync<UpdateAttachmentCollectionsRequest, UpdateAttachmentCollectionsResponse>
                    (It.IsAny<UpdateAttachmentCollectionsRequest>())).ReturnsAsync(null);

                try
                {
                    await repository.PutAttachmentCollectionAsync(
                        new AttachmentCollection(userData.Recordkey, userData.AtcolName, userData.AtcolOwner));
                }
                catch (RepositoryException e)
                {
                    Assert.IsTrue(e.Message == "No response received updating attachment collection");
                    throw;
                }
            }

            [TestMethod]
            [ExpectedException(typeof(RepositoryException))]
            public async Task AttachmentCollectionRepository_PutAttachmentCollectionAsync_ErrorResponse()
            {
                var expectedResponse = new UpdateAttachmentCollectionsResponse
                {
                    ErrorMsg = "error occurred"
                };

                transManagerMock.Setup(accessor =>
                    accessor.ExecuteAsync<UpdateAttachmentCollectionsRequest, UpdateAttachmentCollectionsResponse>
                    (It.IsAny<UpdateAttachmentCollectionsRequest>())).ReturnsAsync(expectedResponse);

                try
                {
                    await repository.PutAttachmentCollectionAsync(
                        new AttachmentCollection(userData.Recordkey, userData.AtcolName, userData.AtcolOwner));
                }
                catch (RepositoryException e)
                {
                    Assert.IsTrue(e.Message == "Error occurred updating attachment collection");
                    throw;
                }
            }

            [TestMethod]
            [ExpectedException(typeof(RepositoryException))]
            public async Task AttachmentCollectionRepository_PutAttachmentCollectionAsync_CtxException()
            {
                transManagerMock.Setup(accessor =>
                    accessor.ExecuteAsync<UpdateAttachmentCollectionsRequest, UpdateAttachmentCollectionsResponse>
                    (It.IsAny<UpdateAttachmentCollectionsRequest>())).ThrowsAsync(new ColleagueTransactionException("error"));

                try
                {
                    await repository.PutAttachmentCollectionAsync(
                        new AttachmentCollection(userData.Recordkey, userData.AtcolName, userData.AtcolOwner));
                }
                catch (RepositoryException e)
                {
                    Assert.IsTrue(e.Message == "Exception occurred updating attachment collection");
                    throw;
                }
            }
        }
    }
}