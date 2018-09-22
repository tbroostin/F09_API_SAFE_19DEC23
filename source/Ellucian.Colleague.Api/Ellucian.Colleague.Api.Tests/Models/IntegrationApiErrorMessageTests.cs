using System;
using System.Net;
using Ellucian.Colleague.Api.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Ellucian.Colleague.Api.Tests.Models
{
    [TestClass]
    public class IntegrationApiErrorMessageTests
    {
        string code = "Error.Code";
        string description = "Error description";
        HttpStatusCode httpStatus = HttpStatusCode.SeeOther;
        int intStatus = 401;

        [TestMethod]
        public void IntegrationApiErrorMessage_DefaultConstructor_CodeValid()
        {
            var result = new IntegrationApiErrorMessage(code, description, httpStatus);
            Assert.AreEqual(code, result.Code);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void IntegrationApiErrorMessage_DefaultConstructor_CodeNull()
        {
            var result = new IntegrationApiErrorMessage(null, description, httpStatus);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void IntegrationApiErrorMessage_DefaultConstructor_CodeEmpty()
        {
            var result = new IntegrationApiErrorMessage(string.Empty, description, httpStatus);
        }

        [TestMethod]
        public void IntegrationApiErrorMessage_DefaultConstructor_DescriptionValid()
        {
            var result = new IntegrationApiErrorMessage(code, description, httpStatus);
            Assert.AreEqual(description, result.Description);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void IntegrationApiErrorMessage_DefaultConstructor_DescriptionNull()
        {
            var result = new IntegrationApiErrorMessage(code, null, httpStatus);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void IntegrationApiErrorMessage_DefaultConstructor_DescriptionEmpty()
        {
            var result = new IntegrationApiErrorMessage(code, string.Empty, httpStatus);
        }

        [TestMethod]
        public void IntegrationApiErrorMessage_DefaultConstructor_StatusValid()
        {
            var result = new IntegrationApiErrorMessage(code, description, httpStatus);
            Assert.AreEqual(httpStatus, result.ReturnCode);
        }

        [TestMethod]
        public void IntegrationApiErrorMessage_OverloadConstructor_CodeValid()
        {
            var result = new IntegrationApiErrorMessage(code, description, intStatus);
            Assert.AreEqual(code, result.Code);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void IntegrationApiErrorMessage_OverloadConstructor_CodeNull()
        {
            var result = new IntegrationApiErrorMessage(null, description, intStatus);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void IntegrationApiErrorMessage_OverloadConstructor_CodeEmpty()
        {
            var result = new IntegrationApiErrorMessage(string.Empty, description, intStatus);
        }

        [TestMethod]
        public void IntegrationApiErrorMessage_OverloadConstructor_DescriptionValid()
        {
            var result = new IntegrationApiErrorMessage(code, description, intStatus);
            Assert.AreEqual(description, result.Description);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void IntegrationApiErrorMessage_OverloadConstructor_DescriptionNull()
        {
            var result = new IntegrationApiErrorMessage(code, null, intStatus);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void IntegrationApiErrorMessage_OverloadConstructor_DescriptionEmpty()
        {
            var result = new IntegrationApiErrorMessage(code, string.Empty, intStatus);
        }

        [TestMethod]
        public void IntegrationApiErrorMessage_OverloadConstructor_StatusValid()
        {
            var result = new IntegrationApiErrorMessage(code, description, intStatus);
            Assert.AreEqual(HttpStatusCode.Unauthorized, result.ReturnCode);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void IntegrationApiErrorMessage_OverloadConstructor_StatusNotValid()
        {
            var result = new IntegrationApiErrorMessage(code, description, 999);
        }

    }
}
