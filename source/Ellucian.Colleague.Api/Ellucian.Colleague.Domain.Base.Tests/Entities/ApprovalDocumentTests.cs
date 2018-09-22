using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ellucian.Colleague.Domain.Base.Entities;

namespace Ellucian.Colleague.Domain.Base.Tests.Entities
{
    [TestClass]
    public class ApprovalDocumentTests
    {
        string id = "1234567890";
        List<String> text = new List<String>() {"Lorem ipsum dolor sit amet, consectetur adipisicing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua.","Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat.","Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur.","Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum."};

        [TestMethod]
        public void ApprovalDocument_Constructor_NullId()
        {
            var result = new ApprovalDocument(null, text);
            Assert.AreEqual(null, result.Id);
        }

        [TestMethod]
        public void ApprovalDocument_Constructor_EmptyId()
        {
            var result = new ApprovalDocument(string.Empty, text);
            Assert.AreEqual(string.Empty, result.Id);
        }

        [TestMethod]
        public void ApprovalDocument_Constructor_ValidId()
        {
            var result = new ApprovalDocument(id, text);
            Assert.AreEqual(id, result.Id);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ApprovalDocument_Constructor_NullText()
        {
            var result = new ApprovalDocument(id, null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ApprovalDocument_Constructor_EmptyText()
        {
            var result = new ApprovalDocument(id, new List<String>());
        }

        [TestMethod]
        public void ApprovalDocument_Constructor_ValidText()
        {
            var result = new ApprovalDocument(id, text);
            CollectionAssert.AreEqual(text, result.Text.ToList());
        }

        [TestMethod]
        public void ApprovalDocument_SetIdOutsideConstructorNull()
        {
            var result = new ApprovalDocument(null, text);
            result.Id = null;
            Assert.AreEqual(null, result.Id);
        }

        [TestMethod]
        public void ApprovalDocument_SetIdOutsideConstructorEmpty()
        {
            var result = new ApprovalDocument(null, text);
            result.Id = string.Empty;
            Assert.AreEqual(string.Empty, result.Id);
        }

        [TestMethod]
        public void ApprovalDocument_SetIdOutsideConstructorValid()
        {
            var result = new ApprovalDocument(null, text);
            result.Id = id;
            Assert.AreEqual(id, result.Id);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void ApprovalDocument_ChangeIdValueSetInConstructor()
        {
            var result = new ApprovalDocument(id, text);
            result.Id = "2345678901";
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void ApprovalDocument_ChangeIdValueSetOutsideConstructor()
        {
            var result = new ApprovalDocument(null, text);
            result.Id = "2345678901";
            result.Id = "3456789012";
        }

        [TestMethod]
        [ExpectedException(typeof(NotSupportedException))]
        public void ApprovalDocument_TryToAddText()
        {
            var result = new ApprovalDocument(id, text);

            (result.Text as IList<string>).Add("something else");
        }

        [TestMethod]
        [ExpectedException(typeof(NotSupportedException))]
        public void ApprovalDocument_TryToInsertText()
        {
            var result = new ApprovalDocument(id, text);

            (result.Text as IList<string>).Insert(1, "something else");
        }

        [TestMethod]
        [ExpectedException(typeof(NotSupportedException))]
        public void ApprovalDocument_TryToDeleteText()
        {
            var result = new ApprovalDocument(id, text);

            (result.Text as IList<string>).Remove("something else");
        }
    }
}
