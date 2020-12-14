// Copyright 2019 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Domain.Student.Entities.InstantEnrollment;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Ellucian.Colleague.Domain.Student.Tests.Entities.InstantEnrollment
{
    [TestClass]
    public class InstantEnrollmentStartPaymentGatewayRegistrationResultTests
    {

        [TestInitialize]
        public void InstantEnrollmentStartPaymentGatewayRegistrationResultTests_Initialize()
        {
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void InstantEnrollmentStartPaymentGatewayRegistrationResult_All_Null()
        {
            var entity = new InstantEnrollmentStartPaymentGatewayRegistrationResult(null, null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void InstantEnrollmentStartPaymentGatewayRegistrationResult_All_Blank()
        {
            var entity = new InstantEnrollmentStartPaymentGatewayRegistrationResult(new List<string>() { "" }, "");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void InstantEnrollmentStartPaymentGatewayRegistrationResult_Messages_And_Url()
        {
            var entity = new InstantEnrollmentStartPaymentGatewayRegistrationResult(new List<string>() { "msg" },"url");
        }

        [TestMethod]
        public void InstantEnrollmentStartPaymentGatewayRegistrationResult_Just_Messages()
        {
            var entity = new InstantEnrollmentStartPaymentGatewayRegistrationResult(new List<string>() { "msg1", "msg2" }, null);
            Assert.AreEqual(entity.ErrorMessages.Count, 2);
            Assert.AreEqual(entity.ErrorMessages[0], "msg1");
            Assert.AreEqual(entity.ErrorMessages[1], "msg2");
        }

        [TestMethod]
        public void InstantEnrollmentStartPaymentGatewayRegistrationResult_Just_Url()
        {
            var entity = new InstantEnrollmentStartPaymentGatewayRegistrationResult(null, "url");
            Assert.AreEqual(entity.PaymentProviderRedirectUrl, "url");
        }
    }
}
