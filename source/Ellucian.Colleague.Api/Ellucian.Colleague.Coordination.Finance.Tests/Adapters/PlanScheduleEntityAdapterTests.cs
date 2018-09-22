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
    public class PlanScheduleEntityAdapterTests
    {
        PlanSchedule planScheduleDto;
        Ellucian.Colleague.Domain.Finance.Entities.PlanSchedule planScheduleEntity;
        PlanScheduleEntityAdapter planScheduleEntityAdapter;

        [TestInitialize]
        public void Initialize()
        {
            var loggerMock = new Mock<ILogger>();
            var adapterRegistryMock = new Mock<IAdapterRegistry>();

            planScheduleEntityAdapter = new PlanScheduleEntityAdapter(adapterRegistryMock.Object, loggerMock.Object);

            var planScheduleAdapter = new AutoMapperAdapter<Domain.Finance.Entities.PlanSchedule, PlanSchedule>(adapterRegistryMock.Object, loggerMock.Object);
            adapterRegistryMock.Setup(reg => reg.GetAdapter<Ellucian.Colleague.Domain.Finance.Entities.PlanSchedule, PlanSchedule>()).Returns(planScheduleAdapter);

            planScheduleEntity = new Domain.Finance.Entities.PlanSchedule(DateTime.Today.AddDays(3), 1000m);

            planScheduleDto = planScheduleEntityAdapter.MapToType(planScheduleEntity);
        }

        [TestCleanup]
        public void Cleanup()
        {

        }

        [TestMethod]
        public void PlanScheduleEntityAdapterTests_Amount()
        {
            Assert.AreEqual(planScheduleEntity.Amount, planScheduleDto.Amount);
        }

        [TestMethod]
        public void PlanScheduleEntityAdapterTests_DueDate()
        {
            Assert.AreEqual(planScheduleEntity.DueDate, planScheduleDto.DueDate);
        }
    }
}