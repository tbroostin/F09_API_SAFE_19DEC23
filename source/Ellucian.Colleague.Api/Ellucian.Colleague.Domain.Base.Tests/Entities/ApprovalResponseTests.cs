using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ellucian.Colleague.Domain.Base.Entities;

namespace Ellucian.Colleague.Domain.Base.Tests.Entities
{
    [TestClass]
    public class ApprovalResponseTests
    {
        string id = "2468013579";
        string documentId = "1234567890";
        string personId = "0006966";
        string userId = "tcarmen";
        DateTimeOffset received = DateTime.Parse("10/01/2013");
        bool isApproved = false;

        [TestMethod]
        public void ApprovalResponse_Constructor_NullId()
        {
            var result = new ApprovalResponse(null, documentId, personId, userId, received, isApproved);
            Assert.AreEqual(null, result.Id);
        }

        [TestMethod]
        public void ApprovalResponse_Constructor_EmptyId()
        {
            var result = new ApprovalResponse(string.Empty, documentId, personId, userId, received, isApproved);
            Assert.AreEqual(string.Empty, result.Id);
        }

        [TestMethod]
        public void ApprovalResponse_Constructor_ValidId()
        {
            var result = new ApprovalResponse(id, documentId, personId, userId, received, isApproved);
            Assert.AreEqual(id, result.Id);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ApprovalResponse_Constructor_NullDocumentId()
        {
            var result = new ApprovalResponse(id, null, personId, userId, received, isApproved);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ApprovalResponse_Constructor_EmptyDocumentId()
        {
            var result = new ApprovalResponse(id, string.Empty, personId, userId, received, isApproved);
        }

        [TestMethod]
        public void ApprovalResponse_Constructor_ValidDocumentId()
        {
            var result = new ApprovalResponse(id, documentId, personId, userId, received, isApproved);
            Assert.AreEqual(documentId, result.DocumentId);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ApprovalResponse_Constructor_NullPersonId()
        {
            var result = new ApprovalResponse(id, documentId, null, userId, received, isApproved);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ApprovalResponse_Constructor_EmptyPersonId()
        {
            var result = new ApprovalResponse(id, documentId, string.Empty, userId, received, isApproved);
        }

        [TestMethod]
        public void ApprovalResponse_Constructor_ValidPersonId()
        {
            var result = new ApprovalResponse(id, documentId, personId, userId, received, isApproved);
            Assert.AreEqual(personId, result.PersonId);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ApprovalResponse_Constructor_NullUserId()
        {
            var result = new ApprovalResponse(id, documentId, personId, null, received, isApproved);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ApprovalResponse_Constructor_EmptyUserId()
        {
            var result = new ApprovalResponse(id, documentId, personId, string.Empty, received, isApproved);
        }

        [TestMethod]
        public void ApprovalResponse_Constructor_ValidUserId()
        {
            var result = new ApprovalResponse(id, documentId, personId, userId, received, isApproved);
            Assert.AreEqual(userId, result.UserId);
        }

        [TestMethod]
        public void ApprovalResponse_Constructor_ValidReceived()
        {
            var result = new ApprovalResponse(id, documentId, personId, userId, received, isApproved);
            Assert.AreEqual(received, result.Received);
        }

        [TestMethod]
        public void ApprovalResponse_Constructor_ValidIsApproved()
        {
            var result = new ApprovalResponse(id, documentId, personId, userId, received, isApproved);
            Assert.AreEqual(isApproved, result.IsApproved);
        }

        [TestMethod]
        public void ApprovalResponse_SetIdOutsideConstructorNull()
        {
            var result = new ApprovalResponse(null, documentId, personId, userId, received, isApproved);
            result.Id = null;
            Assert.AreEqual(null, result.Id);
        }

        [TestMethod]
        public void ApprovalResponse_SetIdOutsideConstructorEmpty()
        {
            var result = new ApprovalResponse(null, documentId, personId, userId, received, isApproved);
            result.Id = string.Empty;
            Assert.AreEqual(string.Empty, result.Id);
        }

        [TestMethod]
        public void ApprovalResponse_SetIdOutsideConstructorValid()
        {
            var result = new ApprovalResponse(null, documentId, personId, userId, received, isApproved);
            result.Id = id;
            Assert.AreEqual(id, result.Id);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void ApprovalDocument_ChangeIdValueSetInConstructor()
        {
            var result = new ApprovalResponse(id, documentId, personId, userId, received, isApproved);
            result.Id = "1357924680";
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void ApprovalDocument_ChangeIdValueSetOutsideConstructor()
        {
            var result = new ApprovalResponse(null, documentId, personId, userId, received, isApproved);
            result.Id = "1357924680";
            result.Id = "2468013579";
        }
    }
}
