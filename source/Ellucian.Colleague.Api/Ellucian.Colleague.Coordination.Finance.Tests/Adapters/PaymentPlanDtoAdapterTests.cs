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
    public class PaymentPlanDtoAdapterTests
    {
        PaymentPlan planDto;
        Ellucian.Colleague.Domain.Finance.Entities.PaymentPlan planEntity;
        PaymentPlanDtoAdapter planDtoAdapter;

        string id = null;
        string templateId = "DEFAULT";
        string personId = "0010456";
        string receivableTypeCode = "01";
        string termId = "2024/SP";
        decimal originalAmount = 550m;
        DateTime firstDueDate = DateTime.Parse("04/01/2024");
        decimal currentAmount = 550m;
        PlanFrequency frequency = PlanFrequency.Weekly;
        int? numberOfPayments = 3;
        decimal? setupChargeAmount = 60m;
        decimal? setupChargePercent = 2m;
        decimal? downPaymentPercent = 10m;
        int? graceDays = 5;
        decimal? lateChargeAmount = 75m;
        decimal? lateChargePercent = 1m;
        List<PlanCharge> planCharges = new List<PlanCharge>()
            {
                new PlanCharge()
                {
                    Amount = 60m,
                    Id = null,
                    IsAutomaticallyModifiable = true,
                    IsSetupCharge = true,
                    PlanId = null,
                    Charge = new Charge()
                    {
                        Amount = 60m,
                        BaseAmount = 60m,
                        Code = "SETUP",
                        Description = new List<string>() { "Setup Fee" },
                        Id = "6",
                        InvoiceId = "1",
                        TaxAmount = 0m,
                        PaymentPlanIds = new List<string>() { "1006" }
                    }
                },
                new PlanCharge()
                {
                    Amount = 5500m,
                    Id = null,
                    IsAutomaticallyModifiable = true,
                    IsSetupCharge = false,
                    PlanId = null,
                    Charge = new Charge()
                    {
                        Amount = 550m,
                        BaseAmount = 550m,
                        Code = "RESHL",
                        Description = new List<string>() { "Residence Hall Fee" },
                        Id = "7",
                        InvoiceId = "2",
                        TaxAmount = 0m,
                        PaymentPlanIds = new List<string>() { "1006" }
                    }
                }
            };
        List<ScheduledPayment> scheduledPayments = new List<ScheduledPayment>()
            {
                new ScheduledPayment()
                {
                    Amount = 115m,
                    AmountPaid = 0m,
                    DueDate = DateTime.Parse("03/15/2024"),
                    Id = null,
                    IsPastDue = DateTime.Parse("03/15/2024") < DateTime.Today,
                    LastPaidDate = null,
                    PlanId = null
                },
                new ScheduledPayment()
                {
                    Amount = 165m,
                    AmountPaid = 0m,
                    DueDate = DateTime.Parse("04/01/2024"),
                    Id = null,
                    IsPastDue = DateTime.Parse("04/01/2024") < DateTime.Today,
                    LastPaidDate = null,
                    PlanId = null
                },
                new ScheduledPayment()
                {
                    Amount = 165m,
                    AmountPaid = 0m,
                    DueDate = DateTime.Parse("04/08/2024"),
                    Id = null,
                    IsPastDue = DateTime.Parse("04/08/2024") < DateTime.Today,
                    LastPaidDate = null,
                    PlanId = null
                },
                new ScheduledPayment()
                {
                    Amount = 115m,
                    AmountPaid = 0m,
                    DueDate = DateTime.Parse("04/15/2024"),
                    Id = null,
                    IsPastDue = DateTime.Parse("04/15/2024") < DateTime.Today,
                    LastPaidDate = null,
                    PlanId = null
                }
            };
        List<PlanStatus> planStatuses = new List<PlanStatus>()
            {
                new PlanStatus()
                {
                    Status = PlanStatusType.Open,
                    Date = DateTime.Today
                }
            };

        [TestInitialize]
        public void Initialize()
        {
            var loggerMock = new Mock<ILogger>();
            var adapterRegistryMock = new Mock<IAdapterRegistry>();
            var chargeAdapter = adapterRegistryMock.Object.GetAdapter<Ellucian.Colleague.Dtos.Finance.Charge, Ellucian.Colleague.Domain.Finance.Entities.Charge>();
            planDtoAdapter = new PaymentPlanDtoAdapter(adapterRegistryMock.Object, loggerMock.Object);

            var planChargeDtoAdapter = new AutoMapperAdapter<PlanCharge, Ellucian.Colleague.Domain.Finance.Entities.PlanCharge>(adapterRegistryMock.Object, loggerMock.Object);
            adapterRegistryMock.Setup(reg => reg.GetAdapter<PlanCharge, Ellucian.Colleague.Domain.Finance.Entities.PlanCharge>()).Returns(planChargeDtoAdapter);

            var chargeDtoAdapter = new AutoMapperAdapter<Charge, Ellucian.Colleague.Domain.Finance.Entities.Charge>(adapterRegistryMock.Object, loggerMock.Object);
            adapterRegistryMock.Setup(reg => reg.GetAdapter<Charge, Ellucian.Colleague.Domain.Finance.Entities.Charge>()).Returns(chargeDtoAdapter);

            planDto = new PaymentPlan()
            {
                CurrentAmount = currentAmount,
                CurrentStatus = PlanStatusType.Open,
                CurrentStatusDate = DateTime.Today,
                FirstDueDate = firstDueDate,
                DownPaymentAmount = (originalAmount * (downPaymentPercent.Value / 100)) + (setupChargeAmount.Value + (originalAmount * (setupChargePercent.Value / 100))),
                DownPaymentAmountPaid = 0m,
                DownPaymentDate = scheduledPayments[0].DueDate,
                DownPaymentPercentage = downPaymentPercent.Value,
                Frequency = frequency,
                GraceDays = graceDays.Value,
                Id = id,
                LateChargeAmount = lateChargeAmount.Value,
                LateChargePercentage = lateChargePercent.Value,
                NumberOfPayments = numberOfPayments.Value,
                OriginalAmount = originalAmount,
                PersonId = personId,
                PlanCharges = planCharges,
                ReceivableTypeCode = receivableTypeCode,
                ScheduledPayments = scheduledPayments,
                SetupAmount = setupChargeAmount.Value,
                SetupPercentage = setupChargePercent.Value,
                Statuses = planStatuses,
                TemplateId = templateId,
                TermId = termId,
                TotalSetupChargeAmount = setupChargeAmount.Value + (originalAmount * (setupChargePercent.Value / 100))
            };

            planEntity = planDtoAdapter.MapToType(planDto);
        }

        [TestCleanup]
        public void Cleanup()
        {

        }

        [TestMethod]
        public void PaymentPlanDto2PaymentPlanEntityAdapter_CurrentAmount()
        {
            Assert.AreEqual(planDto.CurrentAmount, planEntity.CurrentAmount);
        }

        [TestMethod]
        public void PaymentPlanDto2PaymentPlanEntityAdapter_CurrentStatus()
        {
            Assert.AreEqual(ConvertPlanStatusTypeDtoToPlanStatusTypeEntity(planDto.CurrentStatus), planEntity.CurrentStatus);
        }

        [TestMethod]
        public void PaymentPlanDto2PaymentPlanEntityAdapter_CurrentStatusDate()
        {
            Assert.AreEqual(planDto.CurrentStatusDate, planEntity.CurrentStatusDate);
        }

        [TestMethod]
        public void PaymentPlanDto2PaymentPlanEntityAdapter_DownPaymentAmount()
        {
            Assert.AreEqual(scheduledPayments[0].Amount, planEntity.DownPaymentAmount);
        }

        [TestMethod]
        public void PaymentPlanDto2PaymentPlanEntityAdapter_DownPaymentAmountPaid()
        {
            Assert.AreEqual(planDto.DownPaymentAmountPaid, planEntity.DownPaymentAmountPaid);
        }

        [TestMethod]
        public void PaymentPlanDto2PaymentPlanEntityAdapter_DownPaymentDate()
        {
            Assert.AreEqual(planDto.DownPaymentDate, planEntity.DownPaymentDate);
        }

        [TestMethod]
        public void PaymentPlanDto2PaymentPlanEntityAdapter_DownPaymentPercentage()
        {
            Assert.AreEqual(planDto.DownPaymentPercentage, planEntity.DownPaymentPercentage);
        }

        [TestMethod]
        public void PaymentPlanDto2PaymentPlanEntityAdapter_FirstDueDate()
        {
            Assert.AreEqual(planDto.FirstDueDate, planEntity.FirstDueDate);
        }

        [TestMethod]
        public void PaymentPlanDto2PaymentPlanEntityAdapter_Frequency()
        {
            Assert.AreEqual(Domain.Finance.Entities.PlanFrequency.Weekly, planEntity.Frequency);
        }

        [TestMethod]
        public void PaymentPlanDto2PaymentPlanEntityAdapter_GraceDays()
        {
            Assert.AreEqual(planDto.GraceDays, planEntity.GraceDays);
        }

        [TestMethod]
        public void PaymentPlanDto2PaymentPlanEntityAdapter_Id()
        {
            Assert.AreEqual(planDto.Id, planEntity.Id);
        }

        [TestMethod]
        public void PaymentPlanDto2PaymentPlanEntityAdapter_LateChargeAmount()
        {
            Assert.AreEqual(planDto.LateChargeAmount, planEntity.LateChargeAmount);
        }

        [TestMethod]
        public void PaymentPlanDto2PaymentPlanEntityAdapter_LateChargePercentage()
        {
            Assert.AreEqual(planDto.LateChargePercentage, planEntity.LateChargePercentage);
        }

        [TestMethod]
        public void PaymentPlanDto2PaymentPlanEntityAdapter_NumberOfPayments()
        {
            Assert.AreEqual(planDto.NumberOfPayments, planEntity.NumberOfPayments);
        }

        [TestMethod]
        public void PaymentPlanDto2PaymentPlanEntityAdapter_OriginalAmount()
        {
            Assert.AreEqual(planDto.OriginalAmount, planEntity.OriginalAmount);
        }

        [TestMethod]
        public void PaymentPlanDto2PaymentPlanEntityAdapter_PersonId()
        {
            Assert.AreEqual(planDto.PersonId, planEntity.PersonId);
        }

        [TestMethod]
        public void PaymentPlanDto2PaymentPlanEntityAdapter_PlanCharges()
        {
            var chargeList = new List<PlanCharge>(planDto.PlanCharges);
            for (int i = 0; i < planCharges.Count; i++)
            {
                Assert.AreEqual(chargeList[i].Amount, planEntity.PlanCharges[i].Amount);
                Assert.AreEqual(chargeList[i].Id, planEntity.PlanCharges[i].Id);
                Assert.AreEqual(chargeList[i].Charge.Amount, planEntity.PlanCharges[i].Charge.Amount);
                Assert.AreEqual(chargeList[i].Charge.BaseAmount, planEntity.PlanCharges[i].Charge.BaseAmount);
                Assert.AreEqual(chargeList[i].Charge.Code, planEntity.PlanCharges[i].Charge.Code);
                CollectionAssert.AreEqual(chargeList[i].Charge.Description, planEntity.PlanCharges[i].Charge.Description);
                Assert.AreEqual(chargeList[i].Charge.Id, planEntity.PlanCharges[i].Charge.Id);
                Assert.AreEqual(chargeList[i].Charge.InvoiceId, planEntity.PlanCharges[i].Charge.InvoiceId);
                CollectionAssert.AreEqual(chargeList[i].Charge.PaymentPlanIds, planEntity.PlanCharges[i].Charge.PaymentPlanIds);
                Assert.AreEqual(chargeList[i].Charge.TaxAmount, planEntity.PlanCharges[i].Charge.TaxAmount);
                Assert.AreEqual(chargeList[i].IsAutomaticallyModifiable, planEntity.PlanCharges[i].IsAutomaticallyModifiable);
                Assert.AreEqual(chargeList[i].IsSetupCharge, planEntity.PlanCharges[i].IsSetupCharge);
                Assert.AreEqual(chargeList[i].PlanId, planEntity.PlanCharges[i].PlanId);
            }
        }

        [TestMethod]
        public void PaymentPlanDto2PaymentPlanEntityAdapter_ReceivableTypeCode()
        {
            Assert.AreEqual(planDto.ReceivableTypeCode, planEntity.ReceivableTypeCode);
        }

        [TestMethod]
        public void PaymentPlanDto2PaymentPlanEntityAdapter_SetupAmount()
        {
            Assert.AreEqual(planDto.SetupAmount, planEntity.SetupAmount);
        }

        [TestMethod]
        public void PaymentPlanDto2PaymentPlanEntityAdapter_SetupPercentage()
        {
            Assert.AreEqual(planDto.SetupPercentage, planEntity.SetupPercentage);
        }

        [TestMethod]
        public void PaymentPlanDto2PaymentPlanEntityAdapter_TemplateId()
        {
            Assert.AreEqual(planDto.TemplateId, planEntity.TemplateId);
        }

        [TestMethod]
        public void PaymentPlanDto2PaymentPlanEntityAdapter_TermId()
        {
            Assert.AreEqual(planDto.TermId, planEntity.TermId);
        }

        [TestMethod]
        public void PaymentPlanDto2PaymentPlanEntityAdapter_TotalSetupChargeAmount()
        {
            Assert.AreEqual(planDto.TotalSetupChargeAmount, planEntity.TotalSetupChargeAmount);
        }

        [TestMethod]
        public void PaymentPlanDto2PaymentPlanEntityAdapter_NullPlanCharges()
        {
            planDto = new PaymentPlan()
            {
                CurrentAmount = currentAmount,
                CurrentStatus = PlanStatusType.Open,
                CurrentStatusDate = DateTime.Today,
                FirstDueDate = firstDueDate,
                DownPaymentAmount = (originalAmount * (downPaymentPercent.Value / 100)) + (setupChargeAmount.Value + (originalAmount * (setupChargePercent.Value / 100))),
                DownPaymentAmountPaid = 0m,
                DownPaymentDate = scheduledPayments[0].DueDate,
                DownPaymentPercentage = downPaymentPercent.Value,
                Frequency = frequency,
                GraceDays = graceDays.Value,
                Id = id,
                LateChargeAmount = lateChargeAmount.Value,
                LateChargePercentage = lateChargePercent.Value,
                NumberOfPayments = numberOfPayments.Value,
                OriginalAmount = originalAmount,
                PersonId = personId,
                PlanCharges = null,
                ReceivableTypeCode = receivableTypeCode,
                ScheduledPayments = scheduledPayments,
                SetupAmount = setupChargeAmount.Value,
                SetupPercentage = setupChargePercent.Value,
                Statuses = planStatuses,
                TemplateId = templateId,
                TermId = termId,
                TotalSetupChargeAmount = setupChargeAmount.Value + (originalAmount * (setupChargePercent.Value / 100))
            };

            planEntity = planDtoAdapter.MapToType(planDto);
            Assert.AreEqual(0, planEntity.PlanCharges.Count);
        }

        [TestMethod]
        public void PaymentPlanDto2PaymentPlanEntityAdapter_ZeroPlanCharges()
        {
            planDto = new PaymentPlan()
            {
                CurrentAmount = currentAmount,
                CurrentStatus = PlanStatusType.Open,
                CurrentStatusDate = DateTime.Today,
                FirstDueDate = firstDueDate,
                DownPaymentAmount = (originalAmount * (downPaymentPercent.Value / 100)) + (setupChargeAmount.Value + (originalAmount * (setupChargePercent.Value / 100))),
                DownPaymentAmountPaid = 0m,
                DownPaymentDate = scheduledPayments[0].DueDate,
                DownPaymentPercentage = downPaymentPercent.Value,
                Frequency = frequency,
                GraceDays = graceDays.Value,
                Id = id,
                LateChargeAmount = lateChargeAmount.Value,
                LateChargePercentage = lateChargePercent.Value,
                NumberOfPayments = numberOfPayments.Value,
                OriginalAmount = originalAmount,
                PersonId = personId,
                PlanCharges = new List<PlanCharge>(),
                ReceivableTypeCode = receivableTypeCode,
                ScheduledPayments = scheduledPayments,
                SetupAmount = setupChargeAmount.Value,
                SetupPercentage = setupChargePercent.Value,
                Statuses = planStatuses,
                TemplateId = templateId,
                TermId = termId,
                TotalSetupChargeAmount = setupChargeAmount.Value + (originalAmount * (setupChargePercent.Value / 100))
            };

            planEntity = planDtoAdapter.MapToType(planDto);
            Assert.AreEqual(0, planEntity.PlanCharges.Count);
        }

        [TestMethod]
        public void PaymentPlanDto2PaymentPlanEntityAdapter_NullScheduledPayments()
        {
            planDto = new PaymentPlan()
            {
                CurrentAmount = currentAmount,
                CurrentStatus = PlanStatusType.Open,
                CurrentStatusDate = DateTime.Today,
                FirstDueDate = firstDueDate,
                DownPaymentAmount = (originalAmount * (downPaymentPercent.Value / 100)) + (setupChargeAmount.Value + (originalAmount * (setupChargePercent.Value / 100))),
                DownPaymentAmountPaid = 0m,
                DownPaymentDate = scheduledPayments[0].DueDate,
                DownPaymentPercentage = downPaymentPercent.Value,
                Frequency = frequency,
                GraceDays = graceDays.Value,
                Id = id,
                LateChargeAmount = lateChargeAmount.Value,
                LateChargePercentage = lateChargePercent.Value,
                NumberOfPayments = numberOfPayments.Value,
                OriginalAmount = originalAmount,
                PersonId = personId,
                PlanCharges = planCharges,
                ReceivableTypeCode = receivableTypeCode,
                ScheduledPayments = null,
                SetupAmount = setupChargeAmount.Value,
                SetupPercentage = setupChargePercent.Value,
                Statuses = planStatuses,
                TemplateId = templateId,
                TermId = termId,
                TotalSetupChargeAmount = setupChargeAmount.Value + (originalAmount * (setupChargePercent.Value / 100))
            };

            planEntity = planDtoAdapter.MapToType(planDto);
            Assert.AreEqual(0, planEntity.ScheduledPayments.Count);
        }

        [TestMethod]
        public void PaymentPlanDto2PaymentPlanEntityAdapter_ZeroScheduledPayments()
        {
            planDto = new PaymentPlan()
            {
                CurrentAmount = currentAmount,
                CurrentStatus = PlanStatusType.Open,
                CurrentStatusDate = DateTime.Today,
                FirstDueDate = firstDueDate,
                DownPaymentAmount = (originalAmount * (downPaymentPercent.Value / 100)) + (setupChargeAmount.Value + (originalAmount * (setupChargePercent.Value / 100))),
                DownPaymentAmountPaid = 0m,
                DownPaymentDate = scheduledPayments[0].DueDate,
                DownPaymentPercentage = downPaymentPercent.Value,
                Frequency = frequency,
                GraceDays = graceDays.Value,
                Id = id,
                LateChargeAmount = lateChargeAmount.Value,
                LateChargePercentage = lateChargePercent.Value,
                NumberOfPayments = numberOfPayments.Value,
                OriginalAmount = originalAmount,
                PersonId = personId,
                PlanCharges = planCharges,
                ReceivableTypeCode = receivableTypeCode,
                ScheduledPayments = new List<ScheduledPayment>(),
                SetupAmount = setupChargeAmount.Value,
                SetupPercentage = setupChargePercent.Value,
                Statuses = planStatuses,
                TemplateId = templateId,
                TermId = termId,
                TotalSetupChargeAmount = setupChargeAmount.Value + (originalAmount * (setupChargePercent.Value / 100))
            };

            planEntity = planDtoAdapter.MapToType(planDto);
            Assert.AreEqual(0, planEntity.ScheduledPayments.Count);
        }

        [TestMethod]
        public void PaymentPlanDto2PaymentPlanEntityAdapter_NullStatuses()
        {
            planDto = new PaymentPlan()
            {
                CurrentAmount = currentAmount,
                CurrentStatus = PlanStatusType.Open,
                CurrentStatusDate = DateTime.Today,
                FirstDueDate = firstDueDate,
                DownPaymentAmount = (originalAmount * (downPaymentPercent.Value / 100)) + (setupChargeAmount.Value + (originalAmount * (setupChargePercent.Value / 100))),
                DownPaymentAmountPaid = 0m,
                DownPaymentDate = scheduledPayments[0].DueDate,
                DownPaymentPercentage = downPaymentPercent.Value,
                Frequency = frequency,
                GraceDays = graceDays.Value,
                Id = id,
                LateChargeAmount = lateChargeAmount.Value,
                LateChargePercentage = lateChargePercent.Value,
                NumberOfPayments = numberOfPayments.Value,
                OriginalAmount = originalAmount,
                PersonId = personId,
                PlanCharges = planCharges,
                ReceivableTypeCode = receivableTypeCode,
                ScheduledPayments = null,
                SetupAmount = setupChargeAmount.Value,
                SetupPercentage = setupChargePercent.Value,
                Statuses = null,
                TemplateId = templateId,
                TermId = termId,
                TotalSetupChargeAmount = setupChargeAmount.Value + (originalAmount * (setupChargePercent.Value / 100))
            };

            planEntity = planDtoAdapter.MapToType(planDto);
            Assert.AreEqual(1, planEntity.Statuses.Count);
            Assert.AreEqual(Domain.Finance.Entities.PlanStatusType.Open, planEntity.Statuses[0].Status);
            Assert.AreEqual(DateTime.Today, planEntity.Statuses[0].Date);
        }

        [TestMethod]
        public void PaymentPlanDto2PaymentPlanEntityAdapter_ZeroStatuses()
        {
            planDto = new PaymentPlan()
            {
                CurrentAmount = currentAmount,
                CurrentStatus = PlanStatusType.Open,
                CurrentStatusDate = DateTime.Today,
                FirstDueDate = firstDueDate,
                DownPaymentAmount = (originalAmount * (downPaymentPercent.Value / 100)) + (setupChargeAmount.Value + (originalAmount * (setupChargePercent.Value / 100))),
                DownPaymentAmountPaid = 0m,
                DownPaymentDate = scheduledPayments[0].DueDate,
                DownPaymentPercentage = downPaymentPercent.Value,
                Frequency = frequency,
                GraceDays = graceDays.Value,
                Id = id,
                LateChargeAmount = lateChargeAmount.Value,
                LateChargePercentage = lateChargePercent.Value,
                NumberOfPayments = numberOfPayments.Value,
                OriginalAmount = originalAmount,
                PersonId = personId,
                PlanCharges = planCharges,
                ReceivableTypeCode = receivableTypeCode,
                ScheduledPayments = scheduledPayments,
                SetupAmount = setupChargeAmount.Value,
                SetupPercentage = setupChargePercent.Value,
                Statuses = new List<PlanStatus>(),
                TemplateId = templateId,
                TermId = termId,
                TotalSetupChargeAmount = setupChargeAmount.Value + (originalAmount * (setupChargePercent.Value / 100))
            };

            planEntity = planDtoAdapter.MapToType(planDto);
            Assert.AreEqual(1, planEntity.Statuses.Count);
            Assert.AreEqual(Domain.Finance.Entities.PlanStatusType.Open, planEntity.Statuses[0].Status);
            Assert.AreEqual(DateTime.Today, planEntity.Statuses[0].Date);
        }

        private Domain.Finance.Entities.PlanStatusType ConvertPlanStatusTypeDtoToPlanStatusTypeEntity(PlanStatusType source)
        {
            switch (source)
            {
                case PlanStatusType.Open:
                    return Domain.Finance.Entities.PlanStatusType.Open;
                case PlanStatusType.Paid:
                    return Domain.Finance.Entities.PlanStatusType.Paid;
                default:
                    return Domain.Finance.Entities.PlanStatusType.Cancelled;
            }
        }
    }
}