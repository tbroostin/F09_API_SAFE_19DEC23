// Copyright 2019 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Domain.Base.Entities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Ellucian.Colleague.Domain.Base.Tests.Entities
{
    [TestClass]
    public class AttachmentCollectionIdentityTests
    {
        [TestClass]
        public class AttachmentCollectionIdentity_Constructor
        {
            [TestMethod]
            public void AttachmentCollectionIdentityConstructorSuccess()
            {
                var actual = new AttachmentCollectionIdentity("id", AttachmentCollectionIdentityType.Role, 
                    new List<AttachmentAction>() { AttachmentAction.View });
                Assert.AreEqual("id", actual.Id);
                Assert.AreEqual(AttachmentCollectionIdentityType.Role, actual.Type);
                Assert.AreEqual(AttachmentAction.View, actual.Actions.First());
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void AttachmentCollectionIdentityConstructorIdNull()
            {
                new AttachmentCollectionIdentity(null, AttachmentCollectionIdentityType.User, new List<AttachmentAction>() { AttachmentAction.View });
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void AttachmentCollectionIdentityConstructorIdEmpty()
            {
                new AttachmentCollectionIdentity(string.Empty, AttachmentCollectionIdentityType.User, new List<AttachmentAction>() { AttachmentAction.View });
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void AttachmentCollectionIdentityConstructorActionsNull()
            {
                new AttachmentCollectionIdentity("id", AttachmentCollectionIdentityType.User, null);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void AttachmentCollectionIdentityConstructorActionsEmpty()
            {
                new AttachmentCollectionIdentity("id", AttachmentCollectionIdentityType.User, new List<AttachmentAction>());
            }
        }
    }
}