// Copyright 2020 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Coordination.Student.Adapters;
using Ellucian.Colleague.Dtos.Student.InstantEnrollment;
using Ellucian.Web.Adapters;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;
using System;

namespace Ellucian.Colleague.Coordination.Student.Tests.Adapters
{
    [TestClass]
    public class InstantEnrollmentPaymentAcknowledgementParagraphRequestDtoToEntityAdapterTests
    {
        private Mock<IAdapterRegistry> adapterRegistryMock;
        private Mock<ILogger> loggerMock;
        private InstantEnrollmentPaymentAcknowledgementParagraphRequestDtoToEntityAdapter adapter;
        private InstantEnrollmentPaymentAcknowledgementParagraphRequest dto;
        private Domain.Student.Entities.InstantEnrollment.InstantEnrollmentPaymentAcknowledgementParagraphRequest entity;

        [TestInitialize]
        public void InstantEnrollmentPaymentAcknowledgementParagraphRequestDtoToEntityAdapterTests_Initialize()
        {
            adapterRegistryMock = new Mock<IAdapterRegistry>();
            loggerMock = new Mock<ILogger>();
            adapter = new InstantEnrollmentPaymentAcknowledgementParagraphRequestDtoToEntityAdapter(adapterRegistryMock.Object, loggerMock.Object);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void InstantEnrollmentPaymentAcknowledgementParagraphRequestDtoToEntityAdapter_null_Dto()
        {
            entity = adapter.MapToType(null);
        }

        [TestMethod]
        public void InstantEnrollmentPaymentAcknowledgementParagraphRequestDtoToEntityAdapter_valid()
        {
            entity = adapter.MapToType(new InstantEnrollmentPaymentAcknowledgementParagraphRequest()
            {
                PersonId = "0001234",
                CashReceiptId = "0004567"
            });
            Assert.AreEqual("0001234", entity.PersonId);
            Assert.AreEqual("0004567", entity.CashReceiptId);
        }
    }
}
