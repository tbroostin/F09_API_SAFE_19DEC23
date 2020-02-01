// Copyright 2019 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Domain.Student.Entities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;

namespace Ellucian.Colleague.Domain.Student.Tests.Entities
{
    [TestClass]
    public class RegistrationResponseTests
    {
        private List<RegistrationMessage> messages;
        private string ipcRegistrationId;
        private List<string> registeredSectionIds;
        private RegistrationResponse entity;

        [TestInitialize]
        public void RegistrationResponseTests_Initialize()
        {
            messages = new List<RegistrationMessage>()
            {
                null,
                new RegistrationMessage()
                {
                    Message = "Unable to register for section 1",
                    SectionId = "1"
                },
                new RegistrationMessage()
                {
                    Message = "Unable to register for section 2",
                    SectionId = "2"
                },
            };
            ipcRegistrationId = "123";
            registeredSectionIds = new List<string>() { null, string.Empty, "3", "3", "4" };
        }

        [TestMethod]
        public void RegistrationResponse_null_Messages_yields_empty_Messages()
        {
            entity = new RegistrationResponse(null, ipcRegistrationId, registeredSectionIds);
            CollectionAssert.AreEqual(new List<RegistrationMessage>(), entity.Messages);
        }

        [TestMethod]
        public void RegistrationResponse_empty_Messages_yields_empty_Messages()
        {
            entity = new RegistrationResponse(new List<RegistrationMessage>(), ipcRegistrationId, registeredSectionIds);
            CollectionAssert.AreEqual(new List<RegistrationMessage>(), entity.Messages);
        }

        [TestMethod]
        public void RegistrationResponse_with_Messages_yields_correct_Messages()
        {
            entity = new RegistrationResponse(messages, ipcRegistrationId, registeredSectionIds);
            Assert.AreEqual(messages.Count - 1, entity.Messages.Count);
            for(var i = 1; i < messages.Count; i++) // Start at 1 because first message is null
            {
                Assert.AreEqual(messages[i].SectionId, entity.Messages[i-1].SectionId); // Subtract 1 from entity Messages index because there should be less items in the property than were passed in
                Assert.AreEqual(messages[i].Message, entity.Messages[i-1].Message);
            }
        }

        [TestMethod]
        public void RegistrationResponse_null_IpcRegistrationId_yields_null_PaymentControlId()
        {
            entity = new RegistrationResponse(messages, null, registeredSectionIds);
            Assert.AreEqual(null, entity.PaymentControlId);
        }

        [TestMethod]
        public void RegistrationResponse_empty_IpcRegistrationId_yields_empty_PaymentControlId()
        {
            entity = new RegistrationResponse(messages, string.Empty, registeredSectionIds);
            Assert.AreEqual(string.Empty, entity.PaymentControlId);
        }

        [TestMethod]
        public void RegistrationResponse_with_IpcRegistrationId_yields_correct_PaymentControlId()
        {
            entity = new RegistrationResponse(messages, ipcRegistrationId, registeredSectionIds);
            Assert.AreEqual(ipcRegistrationId, entity.PaymentControlId);
        }

        [TestMethod]
        public void RegistrationResponse_null_RegisteredSectionIds_yields_empty_RegisteredSectionIds()
        {
            entity = new RegistrationResponse(messages, ipcRegistrationId, null);
            CollectionAssert.AreEqual(new List<string>(), entity.RegisteredSectionIds);
        }

        [TestMethod]
        public void RegistrationResponse_empty_RegisteredSectionIds_yields_empty_RegisteredSectionIds()
        {
            entity = new RegistrationResponse(messages, ipcRegistrationId, new List<string>());
            CollectionAssert.AreEqual(new List<string>(), entity.RegisteredSectionIds);
        }

        [TestMethod]
        public void RegistrationResponse_with_RegisteredSectionIds_containing_null_empty_and_duplicates_yields_correct_RegisteredSectionIds()
        {
            entity = new RegistrationResponse(messages, ipcRegistrationId, registeredSectionIds);
            CollectionAssert.AreEqual(new List<string>() { "3", "4" }, entity.RegisteredSectionIds);
        }
    }
}
