// Copyright 2014 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using Ellucian.Colleague.Coordination.Finance.Adapters;
using Ellucian.Colleague.Dtos.Finance;
using Ellucian.Web.Adapters;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;

namespace Ellucian.Colleague.Coordination.Finance.Tests.Adapters
{
    [TestClass]
    public class RegistrationPaymentControlDtoAdapterTests
    {
        RegistrationPaymentControl registrationPaymentControlDto;
        Ellucian.Colleague.Domain.Finance.Entities.RegistrationPaymentControl registrationPaymentControlEntity;
        RegistrationPaymentControlDtoAdapter registrationPaymentControlDtoAdapter;

        [TestInitialize]
        public void Initialize()
        {
            var loggerMock = new Mock<ILogger>();
            var adapterRegistryMock = new Mock<IAdapterRegistry>();
            registrationPaymentControlDtoAdapter = new RegistrationPaymentControlDtoAdapter(adapterRegistryMock.Object, loggerMock.Object);

            registrationPaymentControlDto = new RegistrationPaymentControl()
            {
                AcademicCredits = new List<string>() { "123", "456", "789"},
                Id = "112233",
                InvoiceIds = new List<string>() { "00001234", "00001235", "000001236" },
                LastPlanApprovalId = "123456",
                LastTermsApprovalId = "234567",
                PaymentPlanId = "234",
                Payments = new List<string>() { "147", "470", "703" }, 
                PaymentStatus = RegistrationPaymentStatus.Accepted,
                RegisteredSectionIds = new List<string>() { "135", "468", "791" },
                StudentId = "0001234",
                TermId = "2014/FA"
            };

            registrationPaymentControlEntity = registrationPaymentControlDtoAdapter.MapToType(registrationPaymentControlDto);
        }

        [TestCleanup]
        public void Cleanup()
        {

        }

        [TestMethod]
        public void RegistrationPaymentControlDtoAdapter_AcademicCredits()
        {
            var dtoAcademicCreditList = new List<string>(registrationPaymentControlDto.AcademicCredits);
            CollectionAssert.AreEqual(dtoAcademicCreditList, registrationPaymentControlEntity.AcademicCredits);
        }

        [TestMethod]
        public void RegistrationPaymentControlDtoAdapter_AcademicCredits_Null()
        {
            registrationPaymentControlDto = new RegistrationPaymentControl()
            {
                AcademicCredits = null,
                Id = "112233",
                InvoiceIds = new List<string>() { "00001234", "00001235", "000001236" },
                LastPlanApprovalId = "123456",
                LastTermsApprovalId = "234567",
                PaymentPlanId = "234",
                Payments = new List<string>() { "147", "470", "703" },
                PaymentStatus = RegistrationPaymentStatus.Accepted,
                RegisteredSectionIds = new List<string>() { "135", "468", "791" },
                StudentId = "0001234",
                TermId = "2014/FA"
            };

            registrationPaymentControlEntity = registrationPaymentControlDtoAdapter.MapToType(registrationPaymentControlDto);
            Assert.AreEqual(0, registrationPaymentControlEntity.AcademicCredits.Count);
        }

        [TestMethod]
        public void RegistrationPaymentControlDtoAdapter_AcademicCredits_Empty()
        {
            registrationPaymentControlDto = new RegistrationPaymentControl()
            {
                AcademicCredits = new List<string>(),
                Id = "112233",
                InvoiceIds = new List<string>() { "00001234", "00001235", "000001236" },
                LastPlanApprovalId = "123456",
                LastTermsApprovalId = "234567",
                PaymentPlanId = "234",
                Payments = new List<string>() { "147", "470", "703" },
                PaymentStatus = RegistrationPaymentStatus.Accepted,
                RegisteredSectionIds = new List<string>() { "135", "468", "791" },
                StudentId = "0001234",
                TermId = "2014/FA"
            };

            registrationPaymentControlEntity = registrationPaymentControlDtoAdapter.MapToType(registrationPaymentControlDto);
            Assert.AreEqual(0, registrationPaymentControlEntity.AcademicCredits.Count);
        }

        [TestMethod]
        public void RegistrationPaymentControlDtoAdapter_Id()
        {
            Assert.AreEqual(registrationPaymentControlDto.Id, registrationPaymentControlEntity.Id);
        }

        [TestMethod]
        public void RegistrationPaymentControlDtoAdapter_InvoiceIds()
        {
            var dtoInvoiceIdList = new List<string>(registrationPaymentControlDto.InvoiceIds);
            CollectionAssert.AreEqual(dtoInvoiceIdList, registrationPaymentControlEntity.InvoiceIds);
        }

        [TestMethod]
        public void RegistrationPaymentControlDtoAdapter_InvoiceIds_Null()
        {
            registrationPaymentControlDto = new RegistrationPaymentControl()
            {
                AcademicCredits = new List<string>() { "00001234", "00001235", "000001236" },
                Id = "112233",
                InvoiceIds = null,
                LastPlanApprovalId = "123456",
                LastTermsApprovalId = "234567",
                PaymentPlanId = "234",
                Payments = new List<string>() { "147", "470", "703" },
                PaymentStatus = RegistrationPaymentStatus.Accepted,
                RegisteredSectionIds = new List<string>() { "135", "468", "791" },
                StudentId = "0001234",
                TermId = "2014/FA"
            };

            registrationPaymentControlEntity = registrationPaymentControlDtoAdapter.MapToType(registrationPaymentControlDto);
            Assert.AreEqual(0, registrationPaymentControlEntity.InvoiceIds.Count);
        }

        [TestMethod]
        public void RegistrationPaymentControlDtoAdapter_InvoiceIds_Empty()
        {
            registrationPaymentControlDto = new RegistrationPaymentControl()
            {
                AcademicCredits = new List<string>() { "00001234", "00001235", "000001236" },
                Id = "112233",
                InvoiceIds = new List<string>(),
                LastPlanApprovalId = "123456",
                LastTermsApprovalId = "234567",
                PaymentPlanId = "234",
                Payments = new List<string>() { "147", "470", "703" },
                PaymentStatus = RegistrationPaymentStatus.Accepted,
                RegisteredSectionIds = new List<string>() { "135", "468", "791" },
                StudentId = "0001234",
                TermId = "2014/FA"
            };

            registrationPaymentControlEntity = registrationPaymentControlDtoAdapter.MapToType(registrationPaymentControlDto);
            Assert.AreEqual(0, registrationPaymentControlEntity.InvoiceIds.Count);
        }

        [TestMethod]
        public void RegistrationPaymentControlDtoAdapter_LastPlanApprovalId()
        {
            Assert.AreEqual(registrationPaymentControlDto.LastPlanApprovalId, registrationPaymentControlEntity.LastPlanApprovalId);
        }

        [TestMethod]
        public void RegistrationPaymentControlDtoAdapter_LastTermsApprovalId()
        {
            Assert.AreEqual(registrationPaymentControlDto.LastTermsApprovalId, registrationPaymentControlEntity.LastTermsApprovalId);
        }

        [TestMethod]
        public void RegistrationPaymentControlDtoAdapter_PaymentPlanId()
        {
            Assert.AreEqual(registrationPaymentControlDto.PaymentPlanId, registrationPaymentControlEntity.PaymentPlanId);
        }

        [TestMethod]
        public void RegistrationPaymentControlDtoAdapter_Payments()
        {
            var dtoPaymentsList = new List<string>(registrationPaymentControlDto.Payments);
            CollectionAssert.AreEqual(dtoPaymentsList, registrationPaymentControlEntity.Payments);
        }

        [TestMethod]
        public void RegistrationPaymentControlDtoAdapter_Payments_Null()
        {
            registrationPaymentControlDto = new RegistrationPaymentControl()
            {
                AcademicCredits = new List<string>() { "00001234", "00001235", "000001236" },
                Id = "112233",
                InvoiceIds = new List<string>() { "147", "470", "703" },
                LastPlanApprovalId = "123456",
                LastTermsApprovalId = "234567",
                PaymentPlanId = "234",
                Payments = null,
                PaymentStatus = RegistrationPaymentStatus.Accepted,
                RegisteredSectionIds = new List<string>() { "135", "468", "791" },
                StudentId = "0001234",
                TermId = "2014/FA"
            };

            registrationPaymentControlEntity = registrationPaymentControlDtoAdapter.MapToType(registrationPaymentControlDto);
            Assert.AreEqual(0, registrationPaymentControlEntity.Payments.Count);
        }

        [TestMethod]
        public void RegistrationPaymentControlDtoAdapter_Payments_Empty()
        {
            registrationPaymentControlDto = new RegistrationPaymentControl()
            {
                AcademicCredits = new List<string>() { "00001234", "00001235", "000001236" },
                Id = "112233",
                InvoiceIds = new List<string>() { "147", "470", "703" },
                LastPlanApprovalId = "123456",
                LastTermsApprovalId = "234567",
                PaymentPlanId = "234",
                Payments = new List<string>(),
                PaymentStatus = RegistrationPaymentStatus.Accepted,
                RegisteredSectionIds = new List<string>() { "135", "468", "791" },
                StudentId = "0001234",
                TermId = "2014/FA"
            };

            registrationPaymentControlEntity = registrationPaymentControlDtoAdapter.MapToType(registrationPaymentControlDto);
            Assert.AreEqual(0, registrationPaymentControlEntity.Payments.Count);
        }

        [TestMethod]
        public void RegistrationPaymentControlDtoAdapter_PaymentStatus()
        {
            Assert.AreEqual(ConvertPaymentStatusDtoToPaymentStatusEntity(registrationPaymentControlDto.PaymentStatus), registrationPaymentControlEntity.PaymentStatus);
        }

        [TestMethod]
        public void RegistrationPaymentControlDtoAdapter_RegisteredSectionIds()
        {
            var dtoRegisteredSectionIdsList = new List<string>(registrationPaymentControlDto.RegisteredSectionIds);
            CollectionAssert.AreEqual(dtoRegisteredSectionIdsList, registrationPaymentControlEntity.RegisteredSectionIds);
        }

        [TestMethod]
        public void RegistrationPaymentControlDtoAdapter_RegisteredSectionIdsNull()
        {
            registrationPaymentControlDto = new RegistrationPaymentControl()
            {
                AcademicCredits = new List<string>() { "00001234", "00001235", "000001236" },
                Id = "112233",
                InvoiceIds = new List<string>() { "147", "470", "703" },
                LastPlanApprovalId = "123456",
                LastTermsApprovalId = "234567",
                PaymentPlanId = "234",
                Payments = new List<string>() { "135", "468", "791" },
                PaymentStatus = RegistrationPaymentStatus.Accepted,
                RegisteredSectionIds = null,
                StudentId = "0001234",
                TermId = "2014/FA"
            };

            registrationPaymentControlEntity = registrationPaymentControlDtoAdapter.MapToType(registrationPaymentControlDto);
            Assert.AreEqual(0, registrationPaymentControlEntity.RegisteredSectionIds.Count);
        }

        [TestMethod]
        public void RegistrationPaymentControlDtoAdapter_RegisteredSectionIds_Empty()
        {
            registrationPaymentControlDto = new RegistrationPaymentControl()
            {
                AcademicCredits = new List<string>() { "00001234", "00001235", "000001236" },
                Id = "112233",
                InvoiceIds = new List<string>() { "147", "470", "703" },
                LastPlanApprovalId = "123456",
                LastTermsApprovalId = "234567",
                PaymentPlanId = "234",
                Payments = new List<string>() { "135", "468", "791" },
                PaymentStatus = RegistrationPaymentStatus.Accepted,
                RegisteredSectionIds = new List<string>(),
                StudentId = "0001234",
                TermId = "2014/FA"
            };

            registrationPaymentControlEntity = registrationPaymentControlDtoAdapter.MapToType(registrationPaymentControlDto);
            Assert.AreEqual(0, registrationPaymentControlEntity.RegisteredSectionIds.Count);
        }

        [TestMethod]
        public void RegistrationPaymentControlDtoAdapter_StudentId()
        {
            Assert.AreEqual(registrationPaymentControlDto.StudentId, registrationPaymentControlEntity.StudentId);
        }

        [TestMethod]
        public void RegistrationPaymentControlDtoAdapter_TermId()
        {
            Assert.AreEqual(registrationPaymentControlDto.TermId, registrationPaymentControlEntity.TermId);
        }

        private Domain.Finance.Entities.RegistrationPaymentStatus ConvertPaymentStatusDtoToPaymentStatusEntity (RegistrationPaymentStatus source)
        {
            switch (source)
            {
                case RegistrationPaymentStatus.Accepted:
                    return Domain.Finance.Entities.RegistrationPaymentStatus.Accepted;
                case RegistrationPaymentStatus.Complete:
                    return Domain.Finance.Entities.RegistrationPaymentStatus.Complete;
                case RegistrationPaymentStatus.Error:
                    return Domain.Finance.Entities.RegistrationPaymentStatus.Error;
                default:
                    return Domain.Finance.Entities.RegistrationPaymentStatus.New;
            }
        }
    }
}