using System;
using System.Collections.Generic;
using System.Net;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ellucian.Web.Http.Exceptions;

namespace Ellucian.Web.Http.Tests.Exceptions
{
    [TestClass]
    public class IntegrationApiExceptionTests
    {
        [TestClass]
        public class IntegrationApiException_DefaultConstructor
        {
            [TestMethod]
            public void IntegrationApiException_DefaultConstructor_DefaultMessage()
            {
                var result = new IntegrationApiException();
                Assert.AreEqual("Integration API exception", result.Message);
            }

            [TestMethod]
            public void IntegrationApiException_DefaultConstructor_ErrorCount()
            {
                var result = new IntegrationApiException();
                Assert.AreEqual(0, result.Errors.Count);
            }

            [TestMethod]
            public void IntegrationApiException_DefaultConstructor_HttpStatusCode()
            {
                var result = new IntegrationApiException();
                Assert.AreEqual(HttpStatusCode.BadRequest, result.HttpStatusCode);
            }
        }

        [TestClass]
        public class IntegrationApiException_OverloadConstructorWithMessage
        {
            [TestMethod]
            public void IntegrationApiException_DefaultConstructor_Message()
            {
                string msg = "Exception message";
                var result = new IntegrationApiException(msg);
                Assert.AreEqual(msg, result.Message);
            }

            [TestMethod]
            public void IntegrationApiException_DefaultConstructor_ErrorCount()
            {
                var result = new IntegrationApiException();
                Assert.AreEqual(0, result.Errors.Count);
            }

            [TestMethod]
            public void IntegrationApiException_DefaultConstructor_HttpStatusCode()
            {
                var result = new IntegrationApiException();
                Assert.AreEqual(HttpStatusCode.BadRequest, result.HttpStatusCode);
            }
        }

        [TestClass]
        public class IntegrationApiException_OverloadConstructorWithException
        {
            [TestMethod]
            public void IntegrationApiException_DefaultConstructor_Message()
            {
                string msg = "Exception message";
                var ex = new ArgumentException("Argument exception");
                var result = new IntegrationApiException(msg, ex);
                Assert.AreEqual(msg, result.Message);
            }

            [TestMethod]
            public void IntegrationApiException_DefaultConstructor_Exception()
            {
                string msg = "Exception message";
                var ex = new ArgumentException("Argument exception");
                var result = new IntegrationApiException(msg, ex);
                Assert.AreEqual(ex, result.InnerException);
            }

            [TestMethod]
            public void IntegrationApiException_DefaultConstructor_ErrorCount()
            {
                var result = new IntegrationApiException();
                Assert.AreEqual(0, result.Errors.Count);
            }

            [TestMethod]
            public void IntegrationApiException_DefaultConstructor_HttpStatusCode()
            {
                var result = new IntegrationApiException();
                Assert.AreEqual(HttpStatusCode.BadRequest, result.HttpStatusCode);
            }
        }

        [TestClass]
        public class IntegrationApiException_OverloadConstructorWithApiError
        {
            private string msg;
            private string code;
            private string desc;
            private string message;
            private HttpStatusCode status;
            private IntegrationApiError apiError;
            private IntegrationApiException result;

            [TestInitialize]
            public void Initialize()
            {
                msg = "Exception message";
                code = "Error.Code";
                desc = "Error description";
                message = "Error message";
                status = HttpStatusCode.NotFound;
                apiError = new IntegrationApiError(code, desc, message, status);
                result = new IntegrationApiException(msg, apiError);
            }

            [TestMethod]
            public void IntegrationApiException_DefaultConstructor_Message()
            {
                Assert.AreEqual(msg, result.Message);
            }

            [TestMethod]
            public void IntegrationApiException_DefaultConstructor_ErrorCount()
            {
                Assert.AreEqual(1, result.Errors.Count);
            }

            [TestMethod]
            public void IntegrationApiException_DefaultConstructor_HttpStatusCode()
            {
                Assert.AreEqual(status, result.HttpStatusCode);
            }
        }

        [TestClass]
        public class IntegrationApiException_OverloadConstructorWithApiErrors
        {
            private string msg;
            private string code, code2;
            private string desc, desc2;
            private string message, message2;
            private HttpStatusCode status, status2;
            private IntegrationApiError apiError, apiError2;
            private List<IntegrationApiError> errors; 
            private IntegrationApiException result;

            [TestInitialize]
            public void Initialize()
            {
                msg = "Exception message";
                code = "Error.Code";
                desc = "Error description";
                message = "Error message";
                status = HttpStatusCode.NotFound;
                apiError = new IntegrationApiError(code, desc, message, status);

                code2 = "Error.Code2";
                desc2 = "Error description2";
                message2 = "Error message 2";
                status2 = HttpStatusCode.NotImplemented;
                apiError2 = new IntegrationApiError(code2, desc2, message2, status2);

                errors = new List<IntegrationApiError>() {apiError, apiError2};
                result = new IntegrationApiException(msg, errors);
            }

            [TestMethod]
            public void IntegrationApiException_DefaultConstructor_Message()
            {
                Assert.AreEqual(msg, result.Message);
            }

            [TestMethod]
            public void IntegrationApiException_DefaultConstructor_ErrorCount()
            {
                Assert.AreEqual(errors.Count, result.Errors.Count);
            }

            [TestMethod]
            public void IntegrationApiException_DefaultConstructor_Errors()
            {
                CollectionAssert.AreEqual(errors, result.Errors);
            }

            [TestMethod]
            public void IntegrationApiException_DefaultConstructor_HttpStatusCode()
            {
                var statusCode = status > status2 ? status : status2;
                Assert.AreEqual(statusCode, result.HttpStatusCode);
            }
        }

        //[TestClass]
        //public class IntegrationApiException_AddError
        //{

        //}

        //[TestClass]
        //public class IntegrationApiException_AddErrors
        //{

        //}

        //[TestClass]
        //public class IntegrationApiException_ToString
        //{

        //}
    }
}
