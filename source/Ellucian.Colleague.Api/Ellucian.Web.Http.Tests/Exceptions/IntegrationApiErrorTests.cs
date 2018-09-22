// Copyright 2014 Ellucian Company L.P. and its affiliates.
using System;
using System.Net;
using Ellucian.Web.Http.Exceptions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Ellucian.Web.Http.Tests.Exceptions
{
    [TestClass]
    public class IntegrationApiErrorTests
    {
        HttpStatusCode status;
        string code, desc, message;

        [TestInitialize]
        public void Initialize()
        {
            code = "Error.Code";
            desc = "This is an error.";
            message = "This is a message.";
            status = HttpStatusCode.OK;
        }

        [TestMethod]
        public void IntegrationApiError_Constructor_ValidCode()
        {
            var error = new IntegrationApiError(code, desc, message, status);
            Assert.AreEqual(code, error.Code);
        }

        [TestMethod]
        public void IntegrationApiError_Constructor_NullCode()
        {
            var error = new IntegrationApiError(null, desc, message, status);
            Assert.AreEqual(null, error.Code);
        }

        [TestMethod]
        public void IntegrationApiError_Constructor_EmptyCode()
        {
            var error = new IntegrationApiError(string.Empty, desc, message, status);
            Assert.AreEqual(null, error.Code);
            Assert.AreEqual(null, error.Description);
            Assert.AreEqual(null, error.Message);
            Assert.AreEqual((HttpStatusCode)0, error.StatusCode);
        }

        [TestMethod]
        public void IntegrationApiError_Constructor_ValidDescription()
        {
            var error = new IntegrationApiError(code, desc, message, status);
            Assert.AreEqual(message, error.Message);
        }

        [TestMethod]
        public void IntegrationApiError_Constructor_NullDescription()
        {
            var error = new IntegrationApiError(code, desc, null, status);
            Assert.AreEqual(null, error.Message);
        }

        [TestMethod]
        public void IntegrationApiError_Constructor_EmptyDescription()
        {
            var error = new IntegrationApiError(code, desc, string.Empty, status);
            Assert.AreEqual(string.Empty, error.Message);
        }

        [TestMethod]
        public void IntegrationApiError_Constructor_ValidMessage()
        {
            var error = new IntegrationApiError(code, desc, message, status);
            Assert.AreEqual(message, error.Message);
        }

        [TestMethod]
        public void IntegrationApiError_Constructor_NullMessage()
        {
            var error = new IntegrationApiError(code, desc, null, status);
            Assert.AreEqual(null, error.Message);
        }

        [TestMethod]
        public void IntegrationApiError_Constructor_EmptyMessage()
        {
            var error = new IntegrationApiError(code, desc, string.Empty, status);
            Assert.AreEqual(string.Empty, error.Message);
        }

        [TestMethod]
        public void IntegrationApiError_Constructor_StatusCode()
        {
            var error = new IntegrationApiError(code, desc, message, status);
            Assert.AreEqual(status, error.StatusCode);
        }

        [TestMethod]
        public void IntegrationApiError_Constructor_DefaultStatusCode()
        {
            var error = new IntegrationApiError(code, desc, message);
            Assert.AreEqual(HttpStatusCode.BadRequest, error.StatusCode);
        }

        [TestMethod]
        public void IntegrationApiError_Constructor_DefaultMessage()
        {
            var error = new IntegrationApiError(code, desc);
            Assert.AreEqual(null, error.Message);
        }

        [TestMethod]
        public void IntegrationApiError_Constructor_DefaultDescription()
        {
            var error = new IntegrationApiError(code);
            Assert.AreEqual(null, error.Description);
        }
    }
}