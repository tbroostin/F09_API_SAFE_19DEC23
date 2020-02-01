// Copyright 2019 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Domain.Base.Entities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;

namespace Ellucian.Colleague.Domain.Base.Tests.Entities
{
    [TestClass]
    public class AttachmentCollectionTests
    {
        [TestClass]
        public class AttachmentCollection_ConstructorTests
        {
            [TestMethod]
            public void AttachmentCollectionConstructorSuccess()
            {
                var actual = new AttachmentCollection("id123__TEST--", "name", "owner");
                actual.RetentionDuration = "P5Y";
                Assert.AreEqual("id123__TEST--", actual.Id);
                Assert.AreEqual("name", actual.Name);
                Assert.AreEqual("owner", actual.Owner);
                Assert.AreEqual("P5Y", actual.RetentionDuration);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void AttachmentCollectionConstructorIdNull()
            {
                new AttachmentCollection(null, "name", "owner");
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void AttachmentCollectionConstructorIdEmpty()
            {
                new AttachmentCollection(string.Empty, "name", "owner");
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void AttachmentCollectionConstructorNameNull()
            {
                new AttachmentCollection("id", null, "owner");
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void AttachmentCollectionConstructorNameEmpty()
            {
                new AttachmentCollection("id", string.Empty, "owner");
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void AttachmentCollectionConstructorOwnerNull()
            {
                new AttachmentCollection("id", "name", null);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void AttachmentCollectionConstructorOwnerEmpty()
            {
                new AttachmentCollection("id", "name", string.Empty);
            }

            [TestMethod]
            [ExpectedException(typeof(InvalidOperationException))]
            public void AttachmentCollectionInvalidIdOperation()
            {
                var actual = new AttachmentCollection("id", "name", "owner");
                actual.Id = "123";
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public void AttachmentCollectionInvalidIdStringOperation()
            {
                var actual = new AttachmentCollection("MY_ATTACHMENT_NAME!", "name", "owner");
            }

            [TestMethod]
            [ExpectedException(typeof(FormatException))]
            public void AttachmentCollectionInvalidRetentionDurationOperation()
            {
                var actual = new AttachmentCollection("id", "name", "owner");
                actual.RetentionDuration = "P5Y!";
            }
        }

        [TestClass]
        public class AttachmentCollection_VerifyAttachmentActionTests
        {
            private AttachmentCollection collection;
            private Attachment attachment;

            [TestInitialize]
            public void Initialize()
            {
                collection = new AttachmentCollection("id", "name", "0003333");
                collection.Status = AttachmentCollectionStatus.Active;
                collection.AttachmentOwnerActions = new List<AttachmentOwnerAction>() { AttachmentOwnerAction.Delete, AttachmentOwnerAction.Update };
                attachment = new Attachment("id", "collectionid", "name", "contentType", 100, "0003333");
            }

            // Create action tests
            [TestMethod]
            public void AttachmentCollectionVerifyCreateUserSuccess()
            {
                collection.Users = new List<AttachmentCollectionIdentity>() {
                    new AttachmentCollectionIdentity("99999", AttachmentCollectionIdentityType.User,
                    new List<AttachmentAction>() { AttachmentAction.Create, AttachmentAction.View })};
                bool actual = collection.VerifyAttachmentAction(attachment, "99999", null, AttachmentAction.Create);
                Assert.AreEqual(true, actual);
            }

            [TestMethod]
            public void AttachmentCollectionVerifyCreateOwnerNoPermission()
            {
                bool actual = collection.VerifyAttachmentAction(attachment, "0003333", null, AttachmentAction.Create);
                Assert.AreEqual(false, actual);
            }

            [TestMethod]
            public void AttachmentCollectionVerifyCreateUserNoPermission()
            {
                collection.Users = new List<AttachmentCollectionIdentity>() {
                    new AttachmentCollectionIdentity("99999", AttachmentCollectionIdentityType.User,
                    new List<AttachmentAction>() { AttachmentAction.View })};
                bool actual = collection.VerifyAttachmentAction(attachment, "99999", null, AttachmentAction.Create);
                Assert.AreEqual(false, actual);
            }

            [TestMethod]
            public void AttachmentCollectionVerifyCreateUserNoUserPermission()
            {
                collection.Users = new List<AttachmentCollectionIdentity>() {
                    new AttachmentCollectionIdentity("99999", AttachmentCollectionIdentityType.User, 
                    new List<AttachmentAction>() { AttachmentAction.Update, AttachmentAction.View })};
                collection.Roles = new List<AttachmentCollectionIdentity>() {
                    new AttachmentCollectionIdentity("40", AttachmentCollectionIdentityType.Role,
                    new List<AttachmentAction>() { AttachmentAction.View })};
                bool actual = collection.VerifyAttachmentAction(attachment, "no user", new List<string>() { "41" }, AttachmentAction.Create);
                Assert.AreEqual(false, actual);
            }

            [TestMethod]
            public void AttachmentCollectionVerifyCreateRoleSuccess()
            {
                collection.Roles = new List<AttachmentCollectionIdentity>() {
                    new AttachmentCollectionIdentity("40", AttachmentCollectionIdentityType.Role, 
                    new List<AttachmentAction>() { AttachmentAction.Create, AttachmentAction.View })};
                bool actual = collection.VerifyAttachmentAction(attachment, "99999", new List<string>() { "40" }, AttachmentAction.Create);
                Assert.AreEqual(true, actual);
            }

            [TestMethod]
            public void AttachmentCollectionVerifyCreateRoleNoPermission()
            {
                collection.Roles = new List<AttachmentCollectionIdentity>() {
                    new AttachmentCollectionIdentity("40", AttachmentCollectionIdentityType.Role,
                    new List<AttachmentAction>() { AttachmentAction.View })};
                bool actual = collection.VerifyAttachmentAction(attachment, "99999", new List<string>() { "40" }, AttachmentAction.Create);
                Assert.AreEqual(false, actual);
            }

            [TestMethod]
            public void AttachmentCollectionVerifyCreateRoleNoRolePermission()
            {
                collection.Roles = new List<AttachmentCollectionIdentity>() {
                    new AttachmentCollectionIdentity("40", AttachmentCollectionIdentityType.Role,
                    new List<AttachmentAction>() { AttachmentAction.Update, AttachmentAction.View })};
                bool actual = collection.VerifyAttachmentAction(attachment, "99999", new List<string>() { "99" }, AttachmentAction.Create);
                Assert.AreEqual(false, actual);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void AttachmentCollectionVerifyCreateAttachmentNull()
            {
                collection.VerifyAttachmentAction(null, "99999", null, AttachmentAction.Create);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void AttachmentCollectionVerifyCreateUserNull()
            {
                collection.VerifyAttachmentAction(attachment, null, null, AttachmentAction.Create);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void AttachmentCollectionVerifyCreateUserEmpty()
            {
                collection.VerifyAttachmentAction(attachment, string.Empty, null, AttachmentAction.Create);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public void AttachmentCollectionVerifyCreateInactive()
            {
                collection.Status = AttachmentCollectionStatus.Inactive;
                collection.VerifyAttachmentAction(attachment, "99999", null, AttachmentAction.Create);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public void AttachmentCollectionVerifyCreateMaxSize()
            {
                collection.MaxAttachmentSize = 1;
                collection.VerifyAttachmentAction(attachment, attachment.Owner, null, AttachmentAction.Create);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public void AttachmentCollectionVerifyCreateContentType()
            {
                collection.AllowedContentTypes = new List<string>() { "application/pdf" };
                attachment.ContentType = "test/plain";
                collection.VerifyAttachmentAction(attachment, attachment.Owner, null, AttachmentAction.Create);
            }

            // Update action tests
            [TestMethod]
            public void AttachmentCollectionVerifyUpdateSuccess()
            {
                bool actual = collection.VerifyAttachmentAction(attachment, attachment.Owner, null, AttachmentAction.Update);
                Assert.AreEqual(true, actual);
            }

            [TestMethod]
            public void AttachmentCollectionVerifyUpdateNoPermission()
            {
                collection.AttachmentOwnerActions = new List<AttachmentOwnerAction>() { };
                bool actual = collection.VerifyAttachmentAction(attachment, attachment.Owner, null, AttachmentAction.Update);
                Assert.AreEqual(false, actual);
            }

            [TestMethod]
            public void AttachmentCollectionVerifyUpdateUserSuccess()
            {
                collection.Users = new List<AttachmentCollectionIdentity>() {
                    new AttachmentCollectionIdentity("99999", AttachmentCollectionIdentityType.User,
                    new List<AttachmentAction>() { AttachmentAction.Update, AttachmentAction.View })};
                bool actual = collection.VerifyAttachmentAction(attachment, "99999", null, AttachmentAction.Update);
                Assert.AreEqual(true, actual);
            }

            [TestMethod]
            public void AttachmentCollectionVerifyUpdateUserNoPermission()
            {
                collection.Users = new List<AttachmentCollectionIdentity>() {
                    new AttachmentCollectionIdentity("99999", AttachmentCollectionIdentityType.User,
                    new List<AttachmentAction>() { AttachmentAction.View })};
                bool actual = collection.VerifyAttachmentAction(attachment, "99999", null, AttachmentAction.Update);
                Assert.AreEqual(false, actual);
            }

            [TestMethod]
            public void AttachmentCollectionVerifyUpdateUserNoUserPermission()
            {
                collection.Users = new List<AttachmentCollectionIdentity>() {
                    new AttachmentCollectionIdentity("99999", AttachmentCollectionIdentityType.User,
                    new List<AttachmentAction>() { AttachmentAction.Update, AttachmentAction.View })};
                bool actual = collection.VerifyAttachmentAction(attachment, "no user", null, AttachmentAction.Update);
                Assert.AreEqual(false, actual);
            }

            [TestMethod]
            public void AttachmentCollectionVerifyUpdateRoleSuccess()
            {
                collection.Roles = new List<AttachmentCollectionIdentity>() {
                    new AttachmentCollectionIdentity("40", AttachmentCollectionIdentityType.Role,
                    new List<AttachmentAction>() { AttachmentAction.Update, AttachmentAction.View })};
                bool actual = collection.VerifyAttachmentAction(attachment, "99999", new List<string>() { "40" }, AttachmentAction.Update);
                Assert.AreEqual(true, actual);
            }

            [TestMethod]
            public void AttachmentCollectionVerifyUpdateRoleNoPermission()
            {
                collection.Roles = new List<AttachmentCollectionIdentity>() {
                    new AttachmentCollectionIdentity("40", AttachmentCollectionIdentityType.Role,
                    new List<AttachmentAction>() { AttachmentAction.View })};
                bool actual = collection.VerifyAttachmentAction(attachment, "99999", new List<string>() { "40" }, AttachmentAction.Update);
                Assert.AreEqual(false, actual);
            }

            [TestMethod]
            public void AttachmentCollectionVerifyUpdateRoleNoRolePermission()
            {
                collection.Roles = new List<AttachmentCollectionIdentity>() {
                    new AttachmentCollectionIdentity("40", AttachmentCollectionIdentityType.Role,
                    new List<AttachmentAction>() { AttachmentAction.Update, AttachmentAction.View })};
                bool actual = collection.VerifyAttachmentAction(attachment, "99999", new List<string>() { "99" }, AttachmentAction.Update);
                Assert.AreEqual(false, actual);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void AttachmentCollectionVerifyUpdateAttachmentNull()
            {
                collection.VerifyAttachmentAction(null, "user", null, AttachmentAction.Update);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void AttachmentCollectionVerifyUpdateUserNull()
            {
                collection.VerifyAttachmentAction(attachment, null, null, AttachmentAction.Update);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void AttachmentCollectionVerifyUpdateUserEmpty()
            {
                collection.VerifyAttachmentAction(attachment, string.Empty, null, AttachmentAction.Update);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public void AttachmentCollectionVerifyUpdateInactive()
            {
                collection.Status = AttachmentCollectionStatus.Inactive;
                collection.VerifyAttachmentAction(attachment, attachment.Owner, null, AttachmentAction.Update);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public void AttachmentCollectionVerifyUpdateMaxSize()
            {
                collection.MaxAttachmentSize = 1;
                collection.VerifyAttachmentAction(attachment, attachment.Owner, null, AttachmentAction.Update);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public void AttachmentCollectionVerifyUpdateContentType()
            {
                collection.AllowedContentTypes = new List<string>() { "application/pdf" };
                attachment.ContentType = "test/plain";
                collection.VerifyAttachmentAction(attachment, attachment.Owner, null, AttachmentAction.Update);
            }

            // Delete action tests
            [TestMethod]
            public void AttachmentCollectionVerifyDeleteSuccess()
            {
                bool actual = collection.VerifyAttachmentAction(attachment, attachment.Owner, null, AttachmentAction.Delete);
                Assert.AreEqual(true, actual);
            }

            [TestMethod]
            public void AttachmentCollectionVerifyDeleteNoPermission()
            {
                collection.AttachmentOwnerActions = new List<AttachmentOwnerAction>() { AttachmentOwnerAction.Update };
                bool actual = collection.VerifyAttachmentAction(attachment, attachment.Owner, null, AttachmentAction.Delete);
                Assert.AreEqual(false, actual);
            }

            [TestMethod]
            public void AttachmentCollectionVerifyDeleteUserSuccess()
            {
                collection.Users = new List<AttachmentCollectionIdentity>() {
                    new AttachmentCollectionIdentity("99999", AttachmentCollectionIdentityType.User,
                    new List<AttachmentAction>() { AttachmentAction.Delete, AttachmentAction.View })};
                bool actual = collection.VerifyAttachmentAction(attachment, attachment.Owner, null, AttachmentAction.Delete);
                Assert.AreEqual(true, actual);
            }

            [TestMethod]
            public void AttachmentCollectionVerifyDeleteUserNoPermission()
            {
                collection.Users = new List<AttachmentCollectionIdentity>() {
                    new AttachmentCollectionIdentity("99999", AttachmentCollectionIdentityType.User,
                    new List<AttachmentAction>() { AttachmentAction.View })};
                bool actual = collection.VerifyAttachmentAction(attachment, "99999", null, AttachmentAction.Delete);
                Assert.AreEqual(false, actual);
            }

            [TestMethod]
            public void AttachmentCollectionVerifyDeleteUserNoUserPermission()
            {
                collection.Users = new List<AttachmentCollectionIdentity>() {
                    new AttachmentCollectionIdentity("99999", AttachmentCollectionIdentityType.User,
                    new List<AttachmentAction>() { AttachmentAction.Delete, AttachmentAction.View })};
                bool actual = collection.VerifyAttachmentAction(attachment, "no user", null, AttachmentAction.Delete);
                Assert.AreEqual(false, actual);
            }

            [TestMethod]
            public void AttachmentCollectionVerifyDeleteRoleSuccess()
            {
                collection.Roles = new List<AttachmentCollectionIdentity>() {
                    new AttachmentCollectionIdentity("40", AttachmentCollectionIdentityType.Role,
                    new List<AttachmentAction>() { AttachmentAction.Delete, AttachmentAction.View })};
                bool actual = collection.VerifyAttachmentAction(attachment, "99999", new List<string>() { "40" }, AttachmentAction.Delete);
                Assert.AreEqual(true, actual);
            }

            [TestMethod]
            public void AttachmentCollectionVerifyDeleteRoleNoPermission()
            {
                collection.Roles = new List<AttachmentCollectionIdentity>() {
                    new AttachmentCollectionIdentity("40", AttachmentCollectionIdentityType.Role,
                    new List<AttachmentAction>() { AttachmentAction.View })};
                bool actual = collection.VerifyAttachmentAction(attachment, "99999", new List<string>() { "40" }, AttachmentAction.Delete);
                Assert.AreEqual(false, actual);
            }

            [TestMethod]
            public void AttachmentCollectionVerifyDeleteRoleNoRolePermission()
            {
                collection.Roles = new List<AttachmentCollectionIdentity>() {
                    new AttachmentCollectionIdentity("40", AttachmentCollectionIdentityType.Role,
                    new List<AttachmentAction>() { AttachmentAction.Delete, AttachmentAction.View })};
                bool actual = collection.VerifyAttachmentAction(attachment, "99999", new List<string>() { "99" }, AttachmentAction.Delete);
                Assert.AreEqual(false, actual);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void AttachmentCollectionVerifyDeleteAttachmentNull()
            {
                collection.VerifyAttachmentAction(null, "user", null, AttachmentAction.Delete);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void AttachmentCollectionVerifyDeleteUserNull()
            {
                collection.VerifyAttachmentAction(attachment, null, null, AttachmentAction.Delete);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void AttachmentCollectionVerifyDeleteUserEmpty()
            {
                collection.VerifyAttachmentAction(attachment, string.Empty, null, AttachmentAction.Delete);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public void AttachmentCollectionVerifyDeleteInactive()
            {
                try
                {
                    collection.Status = AttachmentCollectionStatus.Inactive;
                    collection.VerifyAttachmentAction(attachment, attachment.Owner, null, AttachmentAction.Delete);
                }
                catch (ArgumentException ae)
                {
                    Assert.AreEqual("Attachment collection is not active", ae.Message);
                    throw;
                }
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public void AttachmentCollectionVerifyDeleteAlreadyDeleted()
            {
                try
                {
                    attachment.Status = AttachmentStatus.Deleted;
                    collection.VerifyAttachmentAction(attachment, attachment.Owner, null, AttachmentAction.Delete);
                }
                catch (ArgumentException ae)
                {
                    Assert.AreEqual("Attachment already has a status of deleted", ae.Message);
                    throw;
                }
            }

            // View action tests
            [TestMethod]
            public void AttachmentCollectionVerifyViewSuccess()
            {
                collection.AttachmentOwnerActions = new List<AttachmentOwnerAction>() { };
                bool actual = collection.VerifyAttachmentAction(attachment, attachment.Owner, null, AttachmentAction.View);
                Assert.AreEqual(true, actual);
            }

            [TestMethod]
            public void AttachmentCollectionVerifyViewUserSuccess()
            {
                collection.Users = new List<AttachmentCollectionIdentity>() {
                    new AttachmentCollectionIdentity("99999", AttachmentCollectionIdentityType.User,
                    new List<AttachmentAction>() { AttachmentAction.View })};
                bool actual = collection.VerifyAttachmentAction(attachment, attachment.Owner, null, AttachmentAction.View);
                Assert.AreEqual(true, actual);
            }

            [TestMethod]
            public void AttachmentCollectionVerifyViewUserNoPermission()
            {
                collection.Users = new List<AttachmentCollectionIdentity>() {
                    new AttachmentCollectionIdentity("99999", AttachmentCollectionIdentityType.User,
                    new List<AttachmentAction>() { AttachmentAction.Update })};
                bool actual = collection.VerifyAttachmentAction(attachment, "99999", null, AttachmentAction.View);
                Assert.AreEqual(false, actual);
            }

            [TestMethod]
            public void AttachmentCollectionVerifyViewUserNoUserPermission()
            {
                collection.Users = new List<AttachmentCollectionIdentity>() {
                    new AttachmentCollectionIdentity("99999", AttachmentCollectionIdentityType.User,
                    new List<AttachmentAction>() { AttachmentAction.View })};
                bool actual = collection.VerifyAttachmentAction(attachment, "no user", null, AttachmentAction.View);
                Assert.AreEqual(false, actual);
            }

            [TestMethod]
            public void AttachmentCollectionVerifyViewRoleSuccess()
            {
                collection.Roles = new List<AttachmentCollectionIdentity>() {
                    new AttachmentCollectionIdentity("40", AttachmentCollectionIdentityType.Role,
                    new List<AttachmentAction>() { AttachmentAction.View })};
                bool actual = collection.VerifyAttachmentAction(attachment, "99999", new List<string>() { "40" }, AttachmentAction.View);
                Assert.AreEqual(true, actual);
            }

            [TestMethod]
            public void AttachmentCollectionVerifyViewRoleNoPermission()
            {
                collection.Roles = new List<AttachmentCollectionIdentity>() {
                    new AttachmentCollectionIdentity("40", AttachmentCollectionIdentityType.Role,
                    new List<AttachmentAction>() { AttachmentAction.Update })};
                bool actual = collection.VerifyAttachmentAction(attachment, "99999", new List<string>() { "40" }, AttachmentAction.View);
                Assert.AreEqual(false, actual);
            }

            [TestMethod]
            public void AttachmentCollectionVerifyViewRoleNoRolePermission()
            {
                collection.Roles = new List<AttachmentCollectionIdentity>() {
                    new AttachmentCollectionIdentity("40", AttachmentCollectionIdentityType.Role,
                    new List<AttachmentAction>() { AttachmentAction.View })};
                bool actual = collection.VerifyAttachmentAction(attachment, "99999", new List<string>() { "99" }, AttachmentAction.View);
                Assert.AreEqual(false, actual);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void AttachmentCollectionVerifyViewAttachmentNull()
            {
                collection.VerifyAttachmentAction(null, "user", null, AttachmentAction.View);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void AttachmentCollectionVerifyViewUserNull()
            {
                collection.VerifyAttachmentAction(attachment, null, null, AttachmentAction.View);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void AttachmentCollectionVerifyViewUserEmpty()
            {
                collection.VerifyAttachmentAction(attachment, string.Empty, null, AttachmentAction.View);
            }

            // verify action tests, with no attachment
            [TestMethod]
            public void AttachmentCollectionVerifyAttachmentActionUserCreateSuccess()
            {
                collection.Users = new List<AttachmentCollectionIdentity>() {
                    new AttachmentCollectionIdentity("99999", AttachmentCollectionIdentityType.User,
                    new List<AttachmentAction>() { AttachmentAction.Create, AttachmentAction.View })};
                bool actual = collection.VerifyAttachmentAction("99999", null, AttachmentAction.Create);
                Assert.AreEqual(true, actual);
            }

            [TestMethod]
            public void AttachmentCollectionVerifyAttachmentActionRoleCreateSuccess()
            {
                collection.Roles = new List<AttachmentCollectionIdentity>() {
                    new AttachmentCollectionIdentity("40", AttachmentCollectionIdentityType.Role,
                    new List<AttachmentAction>() { AttachmentAction.Create, AttachmentAction.View })};
                bool actual = collection.VerifyAttachmentAction("99999", new List<string>() { "40" }, AttachmentAction.Create);
                Assert.AreEqual(true, actual);
            }
            
            [TestMethod]
            public void AttachmentCollectionVerifyAttachmentActionNoCreate()
            {
                collection.Users = new List<AttachmentCollectionIdentity>() {
                    new AttachmentCollectionIdentity("99999", AttachmentCollectionIdentityType.User,
                    new List<AttachmentAction>() { AttachmentAction.View })};
                bool actual = collection.VerifyAttachmentAction("99999", null, AttachmentAction.Create);
                Assert.AreEqual(false, actual);
            }
            
            [TestMethod]
            public void AttachmentCollectionVerifyAttachmentActionUserCreateNoView()
            {
                collection.Users = new List<AttachmentCollectionIdentity>() {
                    new AttachmentCollectionIdentity("99999", AttachmentCollectionIdentityType.User,
                    new List<AttachmentAction>() { AttachmentAction.Create })};
                bool actual = collection.VerifyAttachmentAction("99999", null, AttachmentAction.Create);
                Assert.AreEqual(true, actual);
            }

            [TestMethod]
            public void AttachmentCollectionVerifyAttachmentActionUserDeleteSuccess()
            {
                collection.Users = new List<AttachmentCollectionIdentity>() {
                    new AttachmentCollectionIdentity("99999", AttachmentCollectionIdentityType.User,
                    new List<AttachmentAction>() { AttachmentAction.Delete, AttachmentAction.View })};
                bool actual = collection.VerifyAttachmentAction("99999", null, AttachmentAction.Delete);
                Assert.AreEqual(true, actual);
            }

            [TestMethod]
            public void AttachmentCollectionVerifyAttachmentActionRoleDeleteSuccess()
            {
                collection.Roles = new List<AttachmentCollectionIdentity>() {
                    new AttachmentCollectionIdentity("40", AttachmentCollectionIdentityType.Role,
                    new List<AttachmentAction>() { AttachmentAction.Delete, AttachmentAction.View })};
                bool actual = collection.VerifyAttachmentAction("99999", new List<string>() { "40" }, AttachmentAction.Delete);
                Assert.AreEqual(true, actual);
            }

            [TestMethod]
            public void AttachmentCollectionVerifyAttachmentActionNoDelete()
            {
                collection.Users = new List<AttachmentCollectionIdentity>() {
                    new AttachmentCollectionIdentity("99999", AttachmentCollectionIdentityType.User,
                    new List<AttachmentAction>() { AttachmentAction.View })};
                bool actual = collection.VerifyAttachmentAction("99999", null, AttachmentAction.Delete);
                Assert.AreEqual(false, actual);
            }

            [TestMethod]
            public void AttachmentCollectionVerifyAttachmentActionUserDeleteNoView()
            {
                collection.Users = new List<AttachmentCollectionIdentity>() {
                    new AttachmentCollectionIdentity("99999", AttachmentCollectionIdentityType.User,
                    new List<AttachmentAction>() { AttachmentAction.Delete })};
                bool actual = collection.VerifyAttachmentAction("99999", null, AttachmentAction.Delete);
                Assert.AreEqual(false, actual);
            }

            [TestMethod]
            public void AttachmentCollectionVerifyAttachmentActionUserUpdateSuccess()
            {
                collection.Users = new List<AttachmentCollectionIdentity>() {
                    new AttachmentCollectionIdentity("99999", AttachmentCollectionIdentityType.User,
                    new List<AttachmentAction>() { AttachmentAction.Update, AttachmentAction.View })};
                bool actual = collection.VerifyAttachmentAction("99999", null, AttachmentAction.Update);
                Assert.AreEqual(true, actual);
            }

            [TestMethod]
            public void AttachmentCollectionVerifyAttachmentActionRoleUpdateSuccess()
            {
                collection.Roles = new List<AttachmentCollectionIdentity>() {
                    new AttachmentCollectionIdentity("40", AttachmentCollectionIdentityType.Role,
                    new List<AttachmentAction>() { AttachmentAction.Update, AttachmentAction.View })};
                bool actual = collection.VerifyAttachmentAction("99999", new List<string>() { "40" }, AttachmentAction.Update);
                Assert.AreEqual(true, actual);
            }

            [TestMethod]
            public void AttachmentCollectionVerifyAttachmentActionNoUpdate()
            {
                collection.Users = new List<AttachmentCollectionIdentity>() {
                    new AttachmentCollectionIdentity("99999", AttachmentCollectionIdentityType.User,
                    new List<AttachmentAction>() { AttachmentAction.View })};
                bool actual = collection.VerifyAttachmentAction("99999", null, AttachmentAction.Update);
                Assert.AreEqual(false, actual);
            }

            [TestMethod]
            public void AttachmentCollectionVerifyAttachmentActionUserUpdateNoView()
            {
                collection.Users = new List<AttachmentCollectionIdentity>() {
                    new AttachmentCollectionIdentity("99999", AttachmentCollectionIdentityType.User,
                    new List<AttachmentAction>() { AttachmentAction.Update })};
                bool actual = collection.VerifyAttachmentAction("99999", null, AttachmentAction.Update);
                Assert.AreEqual(false, actual);
            }

            [TestMethod]
            public void AttachmentCollectionVerifyAttachmentActionUserViewSuccess()
            {
                collection.Users = new List<AttachmentCollectionIdentity>() {
                    new AttachmentCollectionIdentity("99999", AttachmentCollectionIdentityType.User,
                    new List<AttachmentAction>() { AttachmentAction.View })};
                bool actual = collection.VerifyAttachmentAction("99999", null, AttachmentAction.View);
                Assert.AreEqual(true, actual);
            }

            [TestMethod]
            public void AttachmentCollectionVerifyAttachmentActionRoleViewSuccess()
            {
                collection.Roles = new List<AttachmentCollectionIdentity>() {
                    new AttachmentCollectionIdentity("40", AttachmentCollectionIdentityType.Role,
                    new List<AttachmentAction>() { AttachmentAction.View })};
                bool actual = collection.VerifyAttachmentAction("99999", new List<string>() { "40" }, AttachmentAction.View);
                Assert.AreEqual(true, actual);
            }

            [TestMethod]
            public void AttachmentCollectionVerifyAttachmentActionNoView()
            {
                collection.Users = new List<AttachmentCollectionIdentity>() {
                    new AttachmentCollectionIdentity("99999", AttachmentCollectionIdentityType.User,
                    new List<AttachmentAction>() { AttachmentAction.Create })};
                bool actual = collection.VerifyAttachmentAction("99999", null, AttachmentAction.View);
                Assert.AreEqual(false, actual);
            }
        }

        [TestClass]
        public class AttachmentCollection_VerifyUpdateAttachmentCollectionTests
        {
            private AttachmentCollection collection;
            private Attachment attachment;

            [TestInitialize]
            public void Initialize()
            {
                collection = new AttachmentCollection("id", "name", "0003333");
                collection.Status = AttachmentCollectionStatus.Active;
                collection.AttachmentOwnerActions = new List<AttachmentOwnerAction>() { AttachmentOwnerAction.Update, AttachmentOwnerAction.Delete };
                attachment = new Attachment("id", "collectionid", "name", "contentType", 100, "0003333");
            }

            [TestMethod]
            public void AttachmentCollectionVerifyUpdateCollectionSuccess()
            {
                bool actual = collection.VerifyUpdateAttachmentCollection(collection.Owner);
                Assert.AreEqual(true, actual);
            }

            [TestMethod]
            public void AttachmentCollectionVerifyUpdateCollectionNoPermission()
            {
                bool actual = collection.VerifyUpdateAttachmentCollection("not owner");
                Assert.AreEqual(false, actual);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void AttachmentCollectionVerifyUpdateCollectionUserNull()
            {
                collection.VerifyUpdateAttachmentCollection(null);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void AttachmentCollectionVerifyUpdateCollectionUserEmpty()
            {
                collection.VerifyUpdateAttachmentCollection(string.Empty);
            }
        }

        [TestClass]
        public class AttachmentCollection_VerifyViewAttachmentCollectionTests
        {
            private AttachmentCollection collection;
            private Attachment attachment;

            [TestInitialize]
            public void Initialize()
            {
                collection = new AttachmentCollection("id", "name", "0003333");
                collection.Status = AttachmentCollectionStatus.Active;
                collection.AttachmentOwnerActions = new List<AttachmentOwnerAction>() { AttachmentOwnerAction.Update, AttachmentOwnerAction.Delete };
                attachment = new Attachment("id", "collectionid", "name", "contentType", 100, "0003333");
            }

            [TestMethod]
            public void AttachmentCollectionVerifyViewCollectionSuccess()
            {
                bool actual = collection.VerifyViewAttachmentCollection(attachment.Owner, null);
                Assert.AreEqual(true, actual);
            }

            [TestMethod]
            public void AttachmentCollectionVerifyViewCollectionNoPermission()
            {
                collection.AttachmentOwnerActions = new List<AttachmentOwnerAction>() { };
                bool actual = collection.VerifyViewAttachmentCollection("not owner", null);
                Assert.AreEqual(false, actual);
            }

            [TestMethod]
            public void AttachmentCollectionVerifyViewCollectionUserSuccess()
            {
                collection.Users = new List<AttachmentCollectionIdentity>() {
                    new AttachmentCollectionIdentity("99999", AttachmentCollectionIdentityType.User, new List<AttachmentAction>() { AttachmentAction.View })};
                bool actual = collection.VerifyViewAttachmentCollection("99999", null);
                Assert.AreEqual(true, actual);
            }

            [TestMethod]
            public void AttachmentCollectionVerifyViewCollectionUserNoPermission()
            {
                collection.Users = new List<AttachmentCollectionIdentity>() {
                    new AttachmentCollectionIdentity("99999", AttachmentCollectionIdentityType.User, new List<AttachmentAction>() { AttachmentAction.Update })};
                bool actual = collection.VerifyViewAttachmentCollection("99999", null);
                Assert.AreEqual(false, actual);
            }

            [TestMethod]
            public void AttachmentCollectionVerifyViewCollectionUserNoUserPermission()
            {
                collection.Users = new List<AttachmentCollectionIdentity>() {
                    new AttachmentCollectionIdentity("99999", AttachmentCollectionIdentityType.User, new List<AttachmentAction>() { AttachmentAction.View })};
                bool actual = collection.VerifyViewAttachmentCollection("not owner", null);
                Assert.AreEqual(false, actual);
            }

            [TestMethod]
            public void AttachmentCollectionVerifyViewCollectionRoleSuccess()
            {
                collection.Roles = new List<AttachmentCollectionIdentity>() {
                    new AttachmentCollectionIdentity("40", AttachmentCollectionIdentityType.Role, new List<AttachmentAction>() { AttachmentAction.View })};
                bool actual = collection.VerifyViewAttachmentCollection("99999", new List<string>() { "40" });
                Assert.AreEqual(true, actual);
            }

            [TestMethod]
            public void AttachmentCollectionVerifyViewCollectionRoleNoPermission()
            {
                collection.Roles = new List<AttachmentCollectionIdentity>() {
                    new AttachmentCollectionIdentity("40", AttachmentCollectionIdentityType.Role, new List<AttachmentAction>() { AttachmentAction.Update })};
                bool actual = collection.VerifyViewAttachmentCollection("99999", new List<string>() { "40" });
                Assert.AreEqual(false, actual);
            }

            [TestMethod]
            public void AttachmentCollectionVerifyViewCollectionRoleNoRolePermission()
            {
                collection.Roles = new List<AttachmentCollectionIdentity>() {
                    new AttachmentCollectionIdentity("40", AttachmentCollectionIdentityType.Role, new List<AttachmentAction>() { AttachmentAction.View })};
                bool actual = collection.VerifyViewAttachmentCollection("99999", new List<string>() { "99" });
                Assert.AreEqual(false, actual);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void AttachmentCollectionVerifyViewCollectionUserNull()
            {
                collection.VerifyViewAttachmentCollection(null, null);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void AttachmentCollectionVerifyViewCollectionUserEmpty()
            {
                collection.VerifyViewAttachmentCollection(string.Empty, null);
            }
        }
    }
}