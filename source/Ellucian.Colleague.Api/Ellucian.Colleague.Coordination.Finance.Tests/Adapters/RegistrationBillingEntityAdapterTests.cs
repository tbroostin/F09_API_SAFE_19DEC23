// Copyright 2014 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using Ellucian.Colleague.Coordination.Finance.Adapters;
using Ellucian.Colleague.Dtos.Finance;
using Ellucian.Colleague.Dtos.Finance.AccountDue;
using Ellucian.Web.Adapters;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;

namespace Ellucian.Colleague.Coordination.Finance.Tests.Adapters
{
    [TestClass]
    public class RegistrationBillingEntityAdapterTests
    {
        RegistrationBilling registrationBillingDto;
        Ellucian.Colleague.Domain.Finance.Entities.RegistrationBilling registrationBillingEntity;
        RegistrationBillingEntityAdapter registrationBillingEntityAdapter;

        [TestInitialize]
        public void Initialize()
        {
            var loggerMock = new Mock<ILogger>();
            var adapterRegistryMock = new Mock<IAdapterRegistry>();

            registrationBillingEntityAdapter = new RegistrationBillingEntityAdapter(adapterRegistryMock.Object, loggerMock.Object);

            var registrationBillingItemEntityAdapter = new RegistrationBillingItemEntityAdapter(adapterRegistryMock.Object, loggerMock.Object);
            adapterRegistryMock.Setup(reg => reg.GetAdapter<Ellucian.Colleague.Domain.Finance.Entities.RegistrationBillingItem, RegistrationBillingItem>()).Returns(registrationBillingItemEntityAdapter);

            registrationBillingEntity = new Domain.Finance.Entities.RegistrationBilling("123", "0001234", "01", DateTime.Today.AddDays(-60),
                DateTime.Today.AddDays(60), "000001235", new List<Domain.Finance.Entities.RegistrationBillingItem>()
                {
                    new Domain.Finance.Entities.RegistrationBillingItem("135", "246"),
                    new Domain.Finance.Entities.RegistrationBillingItem("136", "247"),
                    new Domain.Finance.Entities.RegistrationBillingItem("137", "248"),
                    new Domain.Finance.Entities.RegistrationBillingItem("138", "249"),
                })
                {
                    AdjustmentId = "124",
                    TermId = "2014/FA"
                };

            registrationBillingDto = registrationBillingEntityAdapter.MapToType(registrationBillingEntity);
        }

        [TestCleanup]
        public void Cleanup()
        {

        }

        [TestMethod]
        public void RegistrationBillingEntityAdapterTests_AccountTypeCode()
        {
            Assert.AreEqual(registrationBillingEntity.AccountTypeCode, registrationBillingDto.AccountTypeCode);
        }

        [TestMethod]
        public void RegistrationBillingEntityAdapterTests_AdjustmentId()
        {
            Assert.AreEqual(registrationBillingEntity.AdjustmentId, registrationBillingDto.AdjustmentId);
        }

        [TestMethod]
        public void RegistrationBillingEntityAdapterTests_BillingEnd()
        {
            Assert.AreEqual(registrationBillingEntity.BillingEnd, registrationBillingDto.BillingEnd);
        }

        [TestMethod]
        public void RegistrationBillingEntityAdapterTests_BillingStart()
        {
            Assert.AreEqual(registrationBillingEntity.BillingStart, registrationBillingDto.BillingStart);
        }

        [TestMethod]
        public void RegistrationBillingEntityAdapterTests_Id()
        {
            Assert.AreEqual(registrationBillingEntity.Id, registrationBillingDto.Id);
        }

        [TestMethod]
        public void RegistrationBillingEntityAdapterTests_InvoiceId()
        {
            Assert.AreEqual(registrationBillingEntity.InvoiceId, registrationBillingDto.InvoiceId);
        }

        [TestMethod]
        public void RegistrationBillingEntityAdapterTests_Items()
        {
            Assert.AreEqual(registrationBillingEntity.Items.Count, registrationBillingDto.Items.Count);
            for (int i = 0; i < registrationBillingDto.Items.Count; i++)
            {
                Assert.AreEqual(registrationBillingEntity.Items[i].Id, registrationBillingDto.Items[i].Id);
            }
        }

        [TestMethod]
        public void RegistrationBillingEntityAdapterTests_PersonId()
        {
            Assert.AreEqual(registrationBillingEntity.PersonId, registrationBillingDto.PersonId);
        }

        [TestMethod]
        public void RegistrationBillingEntityAdapterTests_TermId()
        {
            Assert.AreEqual(registrationBillingEntity.TermId, registrationBillingDto.TermId);
        }
    }
}