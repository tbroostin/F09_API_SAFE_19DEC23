// Copyright 2014-2015 Ellucian Company L.P. and its affiliates.
using System;
using Ellucian.Colleague.Domain.Entities;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Ellucian.Colleague.Domain.Base.Tests.Exceptions
{
    [TestClass]
    public class RepositoryErrorTests
    {
        string code = "Test.error.code";
        string message = "This is a test error message.";

        [TestMethod]
        public void RepositoryError_Constructor_NoMessage()
        {
            var result = new RepositoryError(code);
            Assert.AreEqual(code, result.Code);
            Assert.IsNull(result.Message);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void RepositoryError_Constructor_NullCode()
        {
            var result = new RepositoryError(null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void RepositoryError_Constructor_EmptyCode()
        {
            var result = new RepositoryError(string.Empty);
        }

        [TestMethod]
        public void RepositoryError_Constructor_WithMessage()
        {
            var result = new RepositoryError(code, message);
            Assert.AreEqual(code, result.Code);
            Assert.AreEqual(message, result.Message);
        }

        [TestMethod]
        public void RepositoryError_ToString_CodeOnly()
        {
            var result = new RepositoryError(code);
            string expected = "Error code: " + code;
            Assert.AreEqual(expected, result.ToString());
        }

        [TestMethod]
        public void RepositoryError_ToString_CodeWithMessage()
        {
            var result = new RepositoryError(code, message);
            string expected = "Error code: " + code + Environment.NewLine + "Error message: " + message;
            Assert.AreEqual(expected, result.ToString());
        }
    }
}
