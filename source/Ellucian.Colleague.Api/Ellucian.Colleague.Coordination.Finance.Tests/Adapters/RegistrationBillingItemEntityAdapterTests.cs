// Copyright 2014 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using Ellucian.Colleague.Coordination.Finance.Adapters;
using Ellucian.Colleague.Dtos.Finance;
using Ellucian.Colleague.Dtos.Finance.AccountDue;
using Ellucian.Colleague.Dtos.Student;
using Ellucian.Web.Adapters;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;

namespace Ellucian.Colleague.Coordination.Finance.Tests.Adapters
{
    [TestClass]
    public class RegistrationBillingItemEntityAdapterTests
    {
        RegistrationBillingItem registrationBillingItemDto;
        Ellucian.Colleague.Domain.Finance.Entities.RegistrationBillingItem registrationBillingItemEntity;
        RegistrationBillingItemEntityAdapter registrationBillingItemEntityAdapter;

        [TestInitialize]
        public void Initialize()
        {
            var loggerMock = new Mock<ILogger>();
            var adapterRegistryMock = new Mock<IAdapterRegistry>();

            registrationBillingItemEntityAdapter = new RegistrationBillingItemEntityAdapter(adapterRegistryMock.Object, loggerMock.Object);

            var sectionEntityAdapter = new AutoMapperAdapter<Domain.Student.Entities.Section, Section>(adapterRegistryMock.Object, loggerMock.Object);
            adapterRegistryMock.Setup(reg => reg.GetAdapter<Domain.Student.Entities.Section, Section>()).Returns(sectionEntityAdapter);

            registrationBillingItemEntity = new Domain.Finance.Entities.RegistrationBillingItem("135", "246");

            registrationBillingItemDto = registrationBillingItemEntityAdapter.MapToType(registrationBillingItemEntity);
        }

        [TestCleanup]
        public void Cleanup()
        {

        }

        [TestMethod]
        public void RegistrationBillingItemEntityAdapterTests_Id()
        {
            Assert.AreEqual(registrationBillingItemEntity.Id, registrationBillingItemDto.Id);
        }
    }
}