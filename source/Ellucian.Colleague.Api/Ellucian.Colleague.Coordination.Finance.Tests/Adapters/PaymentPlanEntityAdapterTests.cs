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
    public class PaymentPlanEntityAdapterTests
    {
        Ellucian.Colleague.Domain.Finance.Entities.PaymentPlan planEntity;
        PaymentPlan planDto;
        PaymentPlanEntityAdapter planEntityAdapter;

        [TestInitialize]
        public void Initialize()
        {
            var loggerMock = new Mock<ILogger>();
            var adapterRegistryMock = new Mock<IAdapterRegistry>();
            var chargeAdapter = adapterRegistryMock.Object.GetAdapter<Ellucian.Colleague.Dtos.Finance.Charge, Ellucian.Colleague.Domain.Finance.Entities.Charge>();
            planEntityAdapter = new PaymentPlanEntityAdapter(adapterRegistryMock.Object, loggerMock.Object);

            var planChargeEntityAdapter = new AutoMapperAdapter<Ellucian.Colleague.Domain.Finance.Entities.PlanCharge, PlanCharge>(adapterRegistryMock.Object, loggerMock.Object);
            adapterRegistryMock.Setup(reg => reg.GetAdapter<Ellucian.Colleague.Domain.Finance.Entities.PlanCharge, PlanCharge>()).Returns(planChargeEntityAdapter);

            var chargeEntityAdapter = new AutoMapperAdapter<Ellucian.Colleague.Domain.Finance.Entities.Charge, Charge>(adapterRegistryMock.Object, loggerMock.Object);
            adapterRegistryMock.Setup(reg => reg.GetAdapter<Ellucian.Colleague.Domain.Finance.Entities.Charge, Charge>()).Returns(chargeEntityAdapter);

            var planStatusEntityAdapter = new AutoMapperAdapter<Ellucian.Colleague.Domain.Finance.Entities.PlanStatus, PlanStatus>(adapterRegistryMock.Object, loggerMock.Object);
            adapterRegistryMock.Setup(reg => reg.GetAdapter<Ellucian.Colleague.Domain.Finance.Entities.PlanStatus, PlanStatus>()).Returns(planStatusEntityAdapter);

            var scheduledPaymentEntityAdapter = new AutoMapperAdapter<Ellucian.Colleague.Domain.Finance.Entities.ScheduledPayment, ScheduledPayment>(adapterRegistryMock.Object, loggerMock.Object);
            adapterRegistryMock.Setup(reg => reg.GetAdapter<Ellucian.Colleague.Domain.Finance.Entities.ScheduledPayment, ScheduledPayment>()).Returns(scheduledPaymentEntityAdapter);

            planEntity = new Domain.Finance.Entities.PaymentPlan(null, "DEFAULT", "0001234", "01", "2014/FA",
                550m, DateTime.Parse("04/01/2024"), new List<Domain.Finance.Entities.PlanStatus>()
                {
                    new Domain.Finance.Entities.PlanStatus(Domain.Finance.Entities.PlanStatusType.Open, DateTime.Today)
                }, new List<Domain.Finance.Entities.ScheduledPayment>()
                {
                    new Domain.Finance.Entities.ScheduledPayment(null, null, 115m, DateTime.Parse("03/15/2024"), 0m, null),
                    new Domain.Finance.Entities.ScheduledPayment(null, null, 165m, DateTime.Parse("04/01/2024"), 0m, null),
                    new Domain.Finance.Entities.ScheduledPayment(null, null, 165m, DateTime.Parse("04/08/2024"), 0m, null),
                    new Domain.Finance.Entities.ScheduledPayment(null, null, 165m, DateTime.Parse("04/15/2024"), 0m, null),

                }, new List<Domain.Finance.Entities.PlanCharge>() 
                {
                    new Domain.Finance.Entities.PlanCharge(null, 
                        new Domain.Finance.Entities.Charge("6", "1", new List<string>() { "Setup Fee" }, "SETUP", 60m)
                        {
                            TaxAmount = 0m,
                        }, 
                    60m, true, true),
                    new Domain.Finance.Entities.PlanCharge(null, 
                        new Domain.Finance.Entities.Charge("7", "2", new List<string>() { "Residence Hall Fee" }, "RESHL", 550m)
                        {
                            TaxAmount = 0m,
                        }, 
                    60m, true, true)
                })
                {
                    CurrentAmount = 550m,
                    DownPaymentPercentage = 10m,
                    Frequency = Domain.Finance.Entities.PlanFrequency.Weekly,
                    GraceDays = 5,
                    LateChargeAmount = 75m,
                    LateChargePercentage = 1m,
                    NumberOfPayments = 3,
                    SetupAmount = 60m,
                    SetupPercentage = 2m
                };

            planDto = planEntityAdapter.MapToType(planEntity);
        }

        [TestCleanup]
        public void Cleanup()
        {

        }

        [TestMethod]
        public void PaymentPlanEntityAdapter_CurrentAmount()
        {
            Assert.AreEqual(planEntity.CurrentAmount, planDto.CurrentAmount);
        }

        [TestMethod]
        public void PaymentPlanEntityAdapter_CurrentStatus()
        {
            Assert.AreEqual(ConvertPlanStatusTypeEntityToPlanStatusTypeDto(planEntity.CurrentStatus), planDto.CurrentStatus);
        }

        [TestMethod]
        public void PaymentPlanEntityAdapter_CurrentStatusDate()
        {
            Assert.AreEqual(planEntity.CurrentStatusDate, planDto.CurrentStatusDate);
        }

        [TestMethod]
        public void PaymentPlanEntityAdapter_DownPaymentAmount()
        {
            Assert.AreEqual(planEntity.ScheduledPayments[0].Amount, planDto.DownPaymentAmount);
        }

        [TestMethod]
        public void PaymentPlanEntityAdapter_DownPaymentAmountPaid()
        {
            Assert.AreEqual(planEntity.DownPaymentAmountPaid, planDto.DownPaymentAmountPaid);
        }

        [TestMethod]
        public void PaymentPlanEntityAdapter_DownPaymentDate()
        {
            Assert.AreEqual(planEntity.DownPaymentDate, planDto.DownPaymentDate);
        }

        [TestMethod]
        public void PaymentPlanEntityAdapter_DownPaymentPercentage()
        {
            Assert.AreEqual(planEntity.DownPaymentPercentage, planDto.DownPaymentPercentage);
        }

        [TestMethod]
        public void PaymentPlanEntityAdapter_FirstDueDate()
        {
            Assert.AreEqual(planEntity.FirstDueDate, planDto.FirstDueDate);
        }

        [TestMethod]
        public void PaymentPlanEntityAdapter_Frequency()
        {
            Assert.AreEqual(PlanFrequency.Weekly, planDto.Frequency);
        }

        [TestMethod]
        public void PaymentPlanEntityAdapter_GraceDays()
        {
            Assert.AreEqual(planEntity.GraceDays, planDto.GraceDays);
        }

        [TestMethod]
        public void PaymentPlanEntityAdapter_Id()
        {
            Assert.AreEqual(planEntity.Id, planDto.Id);
        }

        [TestMethod]
        public void PaymentPlanEntityAdapter_LateChargeAmount()
        {
            Assert.AreEqual(planEntity.LateChargeAmount, planDto.LateChargeAmount);
        }

        [TestMethod]
        public void PaymentPlanEntityAdapter_LateChargePercentage()
        {
            Assert.AreEqual(planEntity.LateChargePercentage, planDto.LateChargePercentage);
        }

        [TestMethod]
        public void PaymentPlanEntityAdapter_NumberOfPayments()
        {
            Assert.AreEqual(planEntity.NumberOfPayments, planDto.NumberOfPayments);
        }

        [TestMethod]
        public void PaymentPlanEntityAdapter_OriginalAmount()
        {
            Assert.AreEqual(planEntity.OriginalAmount, planDto.OriginalAmount);
        }

        [TestMethod]
        public void PaymentPlanEntityAdapter_PersonId()
        {
            Assert.AreEqual(planEntity.PersonId, planDto.PersonId);
        }

        [TestMethod]
        public void PaymentPlanEntityAdapter_PlanCharges()
        {
            var chargeList = new List<PlanCharge>(planDto.PlanCharges);
            for (int i = 0; i < chargeList.Count; i++)
            {
                Assert.AreEqual(planEntity.PlanCharges[i].Amount, chargeList[i].Amount);
                Assert.AreEqual(planEntity.PlanCharges[i].Id, chargeList[i].Id);
                Assert.AreEqual(planEntity.PlanCharges[i].Charge.Amount, chargeList[i].Charge.Amount);
                Assert.AreEqual(planEntity.PlanCharges[i].Charge.BaseAmount, chargeList[i].Charge.BaseAmount);
                Assert.AreEqual(planEntity.PlanCharges[i].Charge.Code, chargeList[i].Charge.Code);
                CollectionAssert.AreEqual(planEntity.PlanCharges[i].Charge.Description, chargeList[i].Charge.Description);
                Assert.AreEqual(planEntity.PlanCharges[i].Charge.Id, chargeList[i].Charge.Id);
                Assert.AreEqual(planEntity.PlanCharges[i].Charge.InvoiceId, chargeList[i].Charge.InvoiceId);
                CollectionAssert.AreEqual(planEntity.PlanCharges[i].Charge.PaymentPlanIds, chargeList[i].Charge.PaymentPlanIds);
                Assert.AreEqual(planEntity.PlanCharges[i].Charge.TaxAmount, chargeList[i].Charge.TaxAmount);
                Assert.AreEqual(planEntity.PlanCharges[i].IsAutomaticallyModifiable, chargeList[i].IsAutomaticallyModifiable);
                Assert.AreEqual(planEntity.PlanCharges[i].IsSetupCharge, chargeList[i].IsSetupCharge);
                Assert.AreEqual(planEntity.PlanCharges[i].PlanId, chargeList[i].PlanId);
            }
        }

        [TestMethod]
        public void PaymentPlanEntityAdapter_ReceivableTypeCode()
        {
            Assert.AreEqual(planEntity.ReceivableTypeCode, planDto.ReceivableTypeCode);
        }

        [TestMethod]
        public void PaymentPlanEntityAdapter_SetupAmount()
        {
            Assert.AreEqual(planEntity.SetupAmount, planDto.SetupAmount);
        }

        [TestMethod]
        public void PaymentPlanEntityAdapter_SetupPercentage()
        {
            Assert.AreEqual(planEntity.SetupPercentage, planDto.SetupPercentage);
        }

        [TestMethod]
        public void PaymentPlanEntityAdapter_TemplateId()
        {
            Assert.AreEqual(planEntity.TemplateId, planDto.TemplateId);
        }

        [TestMethod]
        public void PaymentPlanEntityAdapter_TermId()
        {
            Assert.AreEqual(planEntity.TermId, planDto.TermId);
        }

        [TestMethod]
        public void PaymentPlanEntityAdapter_TotalSetupChargeAmount()
        {
            Assert.AreEqual(planEntity.TotalSetupChargeAmount, planDto.TotalSetupChargeAmount);
        }

        private PlanStatusType ConvertPlanStatusTypeEntityToPlanStatusTypeDto(Domain.Finance.Entities.PlanStatusType? source)
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