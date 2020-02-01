// Copyright 2014-2015 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using Ellucian.Colleague.Domain.Entities;
using Ellucian.Colleague.Domain.Exceptions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Ellucian.Colleague.Domain.Base.Tests.Exceptions
{
    [TestClass]
    public class RepositoryExceptionTests
    {
        string message = "This is a repository exception test; this is only a test.";
        string code1 = "Error.code1";
        string code2 = "Error.code2";
        string message1 = "This is a repository error.";
        string message2 = "This is another repository error.";
        RepositoryException result;

        [TestInitialize]
        public void Initialize()
        {
            result = new RepositoryException(message);
        }

        [TestMethod]
        public void RepositoryException_Constructor_Default()
        {
            var result = new RepositoryException();
            Assert.AreEqual("Repository exception", result.Message);
        }

        [TestMethod]
        public void RepositoryException_Constructor_OverloadMessage()
        {
            var result = new RepositoryException(message);
            Assert.AreEqual(message, result.Message);
        }

        [TestMethod]
        public void RepositoryException_Constructor_OverloadMessageinnerException()
        {
            var ex = new Exception();
            var result = new RepositoryException(message, ex);
            Assert.AreEqual(message, result.Message);
            Assert.AreEqual(ex, result.InnerException);
        }

        [TestMethod]
        public void RepositoryException_AddError()
        {
            result.AddError(new RepositoryError(code1, message1));
            Assert.AreEqual(code1, result.Errors[0].Code);
            Assert.AreEqual(message1, result.Errors[0].Message);
        }

        [TestMethod]
        public void RepositoryException_AddError_DuplicateError()
        {
            var error = new RepositoryError(code1, message1);
            result.AddError(error);
            result.AddError(error);

            Assert.AreEqual(code1, result.Errors[0].Code);
            Assert.AreEqual(code1, result.Errors[1].Code);
            Assert.AreEqual(message1, result.Errors[0].Message);
            Assert.AreEqual(message1, result.Errors[1].Message);
        }

        [TestMethod]
        public void RepositoryException_AddErrors()
        {
            var errors = new List<RepositoryError>();
            errors.Add(new RepositoryError(code1, message1));
            errors.Add(new RepositoryError(code2, message2));
            result.AddErrors(errors);

            Assert.AreEqual(code1, result.Errors[0].Code);
            Assert.AreEqual(message1, result.Errors[0].Message);

            Assert.AreEqual(code2, result.Errors[1].Code);
            Assert.AreEqual(message2, result.Errors[1].Message);
        }

        // When platform removes the throws from the AddError() method these tests will fail.
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void RepositoryException_AddError_Null()
        {
            result.AddError(null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void RepositoryException_AddErrors_Null()
        {
            result.AddErrors(null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void RepositoryException_AddErrors_Empty()
        {
            result.AddErrors(new List<RepositoryError>());
        }

        [TestMethod]
        public void RepositoryException_ToString()
        {
            var errors = new List<RepositoryError>();
            errors.Add(new RepositoryError(code1, message1));
            errors.Add(new RepositoryError(code2, message2));
            result.AddErrors(errors);
            Assert.AreEqual("Exception: This is a repository exception test; this is only a test.Error code: Error.code1"
                +Environment.NewLine
                +"Error message: This is a repository error."
                +Environment.NewLine
                +"Error code: Error.code2"
                +Environment.NewLine
                +"Error message: This is another repository error.", result.ToString());
        }
    }
}
