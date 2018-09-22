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
    public class PlanStatusEntityAdapterTests
    {
        PlanStatus openPlanStatusDto;
        PlanStatus paidPlanStatusDto;
        PlanStatus cancelledPlanStatusDto;
        Ellucian.Colleague.Domain.Finance.Entities.PlanStatus openPlanStatusEntity;
        Ellucian.Colleague.Domain.Finance.Entities.PlanStatus paidPlanStatusEntity;
        Ellucian.Colleague.Domain.Finance.Entities.PlanStatus cancelledPlanStatusEntity;
        PlanStatusEntityAdapter planStatusEntityAdapter;

        [TestInitialize]
        public void Initialize()
        {
            var loggerMock = new Mock<ILogger>();
            var adapterRegistryMock = new Mock<IAdapterRegistry>();

            planStatusEntityAdapter = new PlanStatusEntityAdapter(adapterRegistryMock.Object, loggerMock.Object);

            var planStatusTypeAdapter = new AutoMapperAdapter<Domain.Finance.Entities.PlanStatusType, PlanStatusType>(adapterRegistryMock.Object, loggerMock.Object);
            adapterRegistryMock.Setup(reg => reg.GetAdapter<Ellucian.Colleague.Domain.Finance.Entities.PlanStatusType, PlanStatusType>()).Returns(planStatusTypeAdapter);

            openPlanStatusEntity = new Domain.Finance.Entities.PlanStatus(Domain.Finance.Entities.PlanStatusType.Open, DateTime.Today);
            paidPlanStatusEntity = new Domain.Finance.Entities.PlanStatus(Domain.Finance.Entities.PlanStatusType.Paid, DateTime.Today.AddDays(-3));
            cancelledPlanStatusEntity = new Domain.Finance.Entities.PlanStatus(Domain.Finance.Entities.PlanStatusType.Cancelled, DateTime.Today.AddDays(-7));

            openPlanStatusDto = planStatusEntityAdapter.MapToType(openPlanStatusEntity);
            paidPlanStatusDto = planStatusEntityAdapter.MapToType(paidPlanStatusEntity);
            cancelledPlanStatusDto = planStatusEntityAdapter.MapToType(cancelledPlanStatusEntity);
        }

        [TestCleanup]
        public void Cleanup()
        {

        }

        [TestMethod]
        public void PlanStatusEntityAdapterTests_Date()
        {
            Assert.AreEqual(openPlanStatusEntity.Date, openPlanStatusDto.Date);
            Assert.AreEqual(paidPlanStatusEntity.Date, paidPlanStatusDto.Date);
            Assert.AreEqual(cancelledPlanStatusEntity.Date, cancelledPlanStatusDto.Date);
        }

        [TestMethod]
        public void PlanStatusEntityAdapterTests_Status()
        {
            Assert.AreEqual(ConvertPlanStatusTypeEntityToPlanStatusTypeDto(openPlanStatusEntity.Status), openPlanStatusDto.Status);
            Assert.AreEqual(ConvertPlanStatusTypeEntityToPlanStatusTypeDto(paidPlanStatusEntity.Status), paidPlanStatusDto.Status);
            Assert.AreEqual(ConvertPlanStatusTypeEntityToPlanStatusTypeDto(cancelledPlanStatusEntity.Status), cancelledPlanStatusDto.Status);

        }

        private PlanStatusType ConvertPlanStatusTypeEntityToPlanStatusTypeDto(Domain.Finance.Entities.PlanStatusType source)
        {
            switch (source)
            {
                case Domain.Finance.Entities.PlanStatusType.Open:
                    return PlanStatusType.Open;
                case Domain.Finance.Entities.PlanStatusType.Paid:
                    return PlanStatusType.Paid;
                default:
                    return PlanStatusType.Cancelled;
            }
        }

    }
}