// Copyright 2019 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Coordination.Base.Adapters;
using Ellucian.Colleague.Dtos.Base;
using Ellucian.Web.Adapters;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Ellucian.Colleague.Coordination.Base.Tests.Adapters
{
    [TestClass]
    public class AttachmentCollectionDtoAdapterTests
    {
        AttachmentCollectionDtoAdapter mapper;
        Mock<IAdapterRegistry> adapterRegistryMock;
        Mock<ILogger> loggerMock;

        [TestInitialize]
        public void Initialize()
        {
            loggerMock = new Mock<ILogger>();
            adapterRegistryMock = new Mock<IAdapterRegistry>();

            mapper = new AttachmentCollectionDtoAdapter(adapterRegistryMock.Object, loggerMock.Object);
        }

        [TestClass]
        public class AttachmentCollectionDtoAdapter_MapToTypeTests : AttachmentCollectionDtoAdapterTests
        {
            AttachmentCollection collection;

            [TestInitialize]
            public void AttachmentCollectionDtoAdapter_MapToType_Initialize()
            {
                collection = new AttachmentCollection()
                {
                    Id = "COLLECTION1",
                    AllowedContentTypes = new List<string>() { "application/pdf", "text/plain" },
                    AttachmentOwnerActions = new List<AttachmentOwnerAction>() { AttachmentOwnerAction.Update, AttachmentOwnerAction.Delete },
                    Description = "description",
                    MaxAttachmentSize = 1000,
                    Name = "my collection 1",
                    Owner = "0000001",
                    RetentionDuration = "P5Y",
                    Status = AttachmentCollectionStatus.Active,
                    Roles = new List<AttachmentCollectionIdentity>()
                    {
                        new AttachmentCollectionIdentity()
                        {
                            Id = "31", Type = AttachmentCollectionIdentityType.Role, Actions = new List<AttachmentAction>()
                            {
                                AttachmentAction.Update, AttachmentAction.Delete, AttachmentAction.View
                            }
                        },
                        new AttachmentCollectionIdentity()
                        {
                            Id = "99", Type = AttachmentCollectionIdentityType.Role, Actions = new List<AttachmentAction>()
                            {
                                AttachmentAction.View
                            }
                        }
                    },
                    Users = new List<AttachmentCollectionIdentity>()
                    {
                        new AttachmentCollectionIdentity()
                        {
                            Id = "0000001", Type = AttachmentCollectionIdentityType.User, Actions = new List<AttachmentAction>()
                            {
                                AttachmentAction.Update, AttachmentAction.Delete, AttachmentAction.View
                            }
                        },
                        new AttachmentCollectionIdentity()
                        {
                            Id = "9999999", Type = AttachmentCollectionIdentityType.User, Actions = new List<AttachmentAction>()
                            {
                                AttachmentAction.View
                            }
                        }
                    }
                };
            }

            [TestMethod]
            public void AttachmentCollectionDtoAdapter_MapToType_Success()
            {
                var actual = mapper.MapToType(collection);
                Assert.AreEqual(collection.Id, actual.Id);
                Assert.AreEqual(collection.AllowedContentTypes.Count(), actual.AllowedContentTypes.Count());
                foreach (var contentType in collection.AllowedContentTypes)
                {
                    Assert.AreEqual(contentType, actual.AllowedContentTypes.Where(c => c == contentType).First());
                }
                Assert.AreEqual(collection.AttachmentOwnerActions.Count(), actual.AttachmentOwnerActions.Count());
                foreach (var ownerAction in collection.AttachmentOwnerActions)
                {
                    Assert.AreEqual(ownerAction.ToString(), actual.AttachmentOwnerActions.Where(
                        a => a.ToString() == ownerAction.ToString()).First().ToString());
                }
                Assert.AreEqual(collection.Description, actual.Description);
                Assert.AreEqual(collection.MaxAttachmentSize, actual.MaxAttachmentSize);
                Assert.AreEqual(collection.Name, actual.Name);
                Assert.AreEqual(collection.Owner, actual.Owner);
                Assert.AreEqual(collection.RetentionDuration, actual.RetentionDuration);
                Assert.AreEqual(collection.Status.ToString(), actual.Status.ToString());
                Assert.AreEqual(collection.Roles.Count(), actual.Roles.Count());
                foreach (var expectedRole in collection.Roles)
                {
                    var actualRole = actual.Roles.Where(r => r.Id == expectedRole.Id).First();
                    Assert.AreEqual(expectedRole.Id, actualRole.Id);
                    Assert.AreEqual(expectedRole.Type.ToString(), actualRole.Type.ToString());
                    foreach (var expectedAction in expectedRole.Actions)
                    {
                        Assert.AreEqual(expectedAction.ToString(),
                            expectedRole.Actions.Where(a => a.ToString() == expectedAction.ToString()).First().ToString());
                    }
                }
                Assert.AreEqual(collection.Users.Count(), actual.Users.Count());
                foreach (var expectedUser in collection.Users)
                {
                    var actualUser = actual.Users.Where(r => r.Id == expectedUser.Id).First();
                    Assert.AreEqual(expectedUser.Id, actualUser.Id);
                    Assert.AreEqual(expectedUser.Type.ToString(), actualUser.Type.ToString());
                    foreach (var expectedAction in expectedUser.Actions)
                    {
                        Assert.AreEqual(expectedAction.ToString(),
                            expectedUser.Actions.Where(a => a.ToString() == expectedAction.ToString()).First().ToString());
                    }
                }
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void AttachmentCollectionDtoAdapter_MapToType_IdNull()
            {
                mapper.MapToType(new AttachmentCollection() { Id = null, Name = "name", Owner = "0000001"  });
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void AttachmentCollectionDtoAdapter_MapToType_IdEmpty()
            {
                mapper.MapToType(new AttachmentCollection() { Id = string.Empty, Name = "name", Owner = "0000001" });
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void AttachmentCollectionDtoAdapter_MapToType_NameNull()
            {
                mapper.MapToType(new AttachmentCollection() { Id = "COLLECTION1", Name = null, Owner = "0000001" });
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void AttachmentCollectionDtoAdapter_MapToType_NameEmpty()
            {
                mapper.MapToType(new AttachmentCollection() { Id = "COLLECTION1", Name = string.Empty, Owner = "0000001" });
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void AttachmentCollectionDtoAdapter_MapToType_OwnerNull()
            {
                mapper.MapToType(new AttachmentCollection() { Id = "COLLECTION1", Name = "name", Owner = null });
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void AttachmentCollectionDtoAdapter_MapToType_OwnerEmpty()
            {
                mapper.MapToType(new AttachmentCollection() { Id = "COLLECTION1", Name = "name", Owner = string.Empty });
            }
        }
    }
}