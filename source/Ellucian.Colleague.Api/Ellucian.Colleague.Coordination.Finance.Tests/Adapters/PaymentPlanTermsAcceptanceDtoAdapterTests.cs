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
    public class PaymentPlanTermsAcceptanceDtoAdapterTests
    {
        PaymentPlanTermsAcceptance planTermsAcceptanceDto;
        Ellucian.Colleague.Domain.Finance.Entities.PaymentPlanTermsAcceptance planTermsAcceptanceEntity;
        PaymentPlanTermsAcceptanceDtoAdapter planTermsAcceptanceDtoAdapter;

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
            planTermsAcceptanceDtoAdapter = new PaymentPlanTermsAcceptanceDtoAdapter(adapterRegistryMock.Object, loggerMock.Object);

            var planDtoAdapter = new AutoMapperAdapter<PaymentPlan, Ellucian.Colleague.Domain.Finance.Entities.PaymentPlan>(adapterRegistryMock.Object, loggerMock.Object);
            adapterRegistryMock.Setup(reg => reg.GetAdapter<PaymentPlan, Ellucian.Colleague.Domain.Finance.Entities.PaymentPlan>()).Returns(planDtoAdapter);

            var chargeDtoAdapter = new AutoMapperAdapter<Charge, Ellucian.Colleague.Domain.Finance.Entities.Charge>(adapterRegistryMock.Object, loggerMock.Object);

            planTermsAcceptanceDto = new PaymentPlanTermsAcceptance()
            {
                AcknowledgementDateTime = DateTimeOffset.UtcNow,
                AcknowledgementText = new List<string>() { "I hereby acknowledge the described payment plan.", "Something else." },
                ApprovalReceived = DateTimeOffset.UtcNow.AddMinutes(-3).AddMinutes(-3),
                ApprovalUserId = "jsmith",
                DownPaymentAmount = (originalAmount * (downPaymentPercent.Value / 100)) + (setupChargeAmount.Value + (originalAmount * (setupChargePercent.Value / 100))),
                DownPaymentDate = scheduledPayments[0].DueDate,
                PaymentControlId = "123",
                ProposedPlan = new PaymentPlan()
                {
                    CurrentAmount = currentAmount,
                    CurrentStatus = PlanStatusType.Open,
                    CurrentStatusDate = DateTime.Today,
                    FirstDueDate = firstDueDate,
                    DownPaymentAmount = scheduledPayments[0].Amount,
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
                },
                RegistrationApprovalId = "234",
                StudentId = "0001234",
                StudentName = "John Smith",
                TermsText = new List<string>() { "These are the terms and conditions.", "Something else." }
            };

            planTermsAcceptanceEntity = planTermsAcceptanceDtoAdapter.MapToType(planTermsAcceptanceDto);
        }

        [TestCleanup]
        public void Cleanup()
        {

        }

        [TestMethod]
        public void PaymentPlanTermsAcceptanceEntityAdapter_AcknowledgementDateTime()
        {
            Assert.AreEqual(planTermsAcceptanceDto.AcknowledgementDateTime, planTermsAcceptanceEntity.AcknowledgementDateTime);
        }

        [TestMethod]
        public void PaymentPlanTermsAcceptanceEntityAdapter_AcknowledgementText()
        {
            CollectionAssert.AreEqual(planTermsAcceptanceDto.AcknowledgementText, planTermsAcceptanceEntity.AcknowledgementText);
        }

        [TestMethod]
        public void PaymentPlanTermsAcceptanceEntityAdapter_ApprovalReceived()
        {
            Assert.AreEqual(planTermsAcceptanceDto.ApprovalReceived, planTermsAcceptanceEntity.ApprovalReceived);
        }

        [TestMethod]
        public void PaymentPlanTermsAcceptanceEntityAdapter_ApprovalUserId()
        {
            Assert.AreEqual(planTermsAcceptanceDto.ApprovalUserId, planTermsAcceptanceEntity.ApprovalUserId);
        }

        [TestMethod]
        public void PaymentPlanTermsAcceptanceEntityAdapter_DownPaymentAmount()
        {
            Assert.AreEqual(planTermsAcceptanceDto.DownPaymentAmount, planTermsAcceptanceEntity.DownPaymentAmount);
        }

        [TestMethod]
        public void PaymentPlanTermsAcceptanceEntityAdapter_DownPaymentDate()
        {
            Assert.AreEqual(planTermsAcceptanceDto.DownPaymentDate, planTermsAcceptanceEntity.DownPaymentDate);
        }

        [TestMethod]
        public void PaymentPlanTermsAcceptanceEntityAdapter_PaymentControlId()
        {
            Assert.AreEqual(planTermsAcceptanceDto.PaymentControlId, planTermsAcceptanceEntity.PaymentControlId);
        }

        [TestMethod]
        public void PaymentPlanTermsAcceptanceEntityAdapter_ProposedPlan()
        {
            var chargeList = new List<PlanCharge>(planTermsAcceptanceDto.ProposedPlan.PlanCharges);
            var scheduledPaymentList = new List<ScheduledPayment>(planTermsAcceptanceDto.ProposedPlan.ScheduledPayments);
            var statusList = new List<PlanStatus>(planTermsAcceptanceDto.ProposedPlan.Statuses);

            Assert.AreEqual(planTermsAcceptanceDto.ProposedPlan.CurrentAmount, planTermsAcceptanceEntity.ProposedPlan.CurrentAmount);
            Assert.AreEqual(ConvertPlanStatusTypeDtoToPlanStatusTypeEntity(planTermsAcceptanceDto.ProposedPlan.CurrentStatus), planTermsAcceptanceEntity.ProposedPlan.CurrentStatus);
            Assert.AreEqual(planTermsAcceptanceDto.ProposedPlan.CurrentStatusDate, planTermsAcceptanceEntity.ProposedPlan.CurrentStatusDate);
            Assert.AreEqual(planTermsAcceptanceDto.ProposedPlan.DownPaymentAmount, planTermsAcceptanceEntity.ProposedPlan.DownPaymentAmount);
            Assert.AreEqual(planTermsAcceptanceDto.ProposedPlan.DownPaymentAmountPaid, planTermsAcceptanceEntity.ProposedPlan.DownPaymentAmountPaid);
            Assert.AreEqual(planTermsAcceptanceDto.ProposedPlan.DownPaymentDate, planTermsAcceptanceEntity.ProposedPlan.DownPaymentDate);
            Assert.AreEqual(planTermsAcceptanceDto.ProposedPlan.DownPaymentPercentage, planTermsAcceptanceEntity.ProposedPlan.DownPaymentPercentage);
            Assert.AreEqual(planTermsAcceptanceDto.ProposedPlan.FirstDueDate, planTermsAcceptanceEntity.ProposedPlan.FirstDueDate);
            Assert.AreEqual(ConvertPlanFrequencyDtoToPlanFrequencyEntity(planTermsAcceptanceDto.ProposedPlan.Frequency), planTermsAcceptanceEntity.ProposedPlan.Frequency);
            Assert.AreEqual(planTermsAcceptanceDto.ProposedPlan.GraceDays, planTermsAcceptanceEntity.ProposedPlan.GraceDays);
            Assert.AreEqual(planTermsAcceptanceDto.ProposedPlan.Id, planTermsAcceptanceEntity.ProposedPlan.Id);
            Assert.AreEqual(planTermsAcceptanceDto.ProposedPlan.LateChargeAmount, planTermsAcceptanceEntity.ProposedPlan.LateChargeAmount);
            Assert.AreEqual(planTermsAcceptanceDto.ProposedPlan.LateChargePercentage, planTermsAcceptanceEntity.ProposedPlan.LateChargePercentage);
            Assert.AreEqual(planTermsAcceptanceDto.ProposedPlan.NumberOfPayments, planTermsAcceptanceEntity.ProposedPlan.NumberOfPayments);
            Assert.AreEqual(planTermsAcceptanceDto.ProposedPlan.OriginalAmount, planTermsAcceptanceEntity.ProposedPlan.OriginalAmount);
            Assert.AreEqual(planTermsAcceptanceDto.ProposedPlan.PersonId, planTermsAcceptanceEntity.ProposedPlan.PersonId);
            Assert.AreEqual(planTermsAcceptanceDto.ProposedPlan.ReceivableTypeCode, planTermsAcceptanceEntity.ProposedPlan.ReceivableTypeCode);
            Assert.AreEqual(planTermsAcceptanceDto.ProposedPlan.SetupAmount, planTermsAcceptanceEntity.ProposedPlan.SetupAmount);
            Assert.AreEqual(planTermsAcceptanceDto.ProposedPlan.SetupPercentage, planTermsAcceptanceEntity.ProposedPlan.SetupPercentage);
            Assert.AreEqual(planTermsAcceptanceDto.ProposedPlan.TemplateId, planTermsAcceptanceEntity.ProposedPlan.TemplateId);
            Assert.AreEqual(planTermsAcceptanceDto.ProposedPlan.TermId, planTermsAcceptanceEntity.ProposedPlan.TermId);
            Assert.AreEqual(planTermsAcceptanceDto.ProposedPlan.TotalSetupChargeAmount, planTermsAcceptanceEntity.ProposedPlan.TotalSetupChargeAmount);

            Assert.AreEqual(chargeList.Count, planTermsAcceptanceEntity.ProposedPlan.PlanCharges.Count);
            for (int i = 0; i < planCharges.Count; i++)
            {
                Assert.AreEqual(chargeList[i].Amount, planTermsAcceptanceEntity.ProposedPlan.PlanCharges[i].Amount);
                Assert.AreEqual(chargeList[i].Id, planTermsAcceptanceEntity.ProposedPlan.PlanCharges[i].Id);
                Assert.AreEqual(chargeList[i].Charge.Amount, planTermsAcceptanceEntity.ProposedPlan.PlanCharges[i].Charge.Amount);
                Assert.AreEqual(chargeList[i].Charge.BaseAmount, planTermsAcceptanceEntity.ProposedPlan.PlanCharges[i].Charge.BaseAmount);
                Assert.AreEqual(chargeList[i].Charge.Code, planTermsAcceptanceEntity.ProposedPlan.PlanCharges[i].Charge.Code);
                CollectionAssert.AreEqual(chargeList[i].Charge.Description, planTermsAcceptanceEntity.ProposedPlan.PlanCharges[i].Charge.Description);
                Assert.AreEqual(chargeList[i].Charge.Id, planTermsAcceptanceEntity.ProposedPlan.PlanCharges[i].Charge.Id);
                Assert.AreEqual(chargeList[i].Charge.InvoiceId, planTermsAcceptanceEntity.ProposedPlan.PlanCharges[i].Charge.InvoiceId);
                Assert.AreEqual(chargeList[i].Charge.TaxAmount, planTermsAcceptanceEntity.ProposedPlan.PlanCharges[i].Charge.TaxAmount);
                Assert.AreEqual(chargeList[i].IsAutomaticallyModifiable, planTermsAcceptanceEntity.ProposedPlan.PlanCharges[i].IsAutomaticallyModifiable);
                Assert.AreEqual(chargeList[i].IsSetupCharge, planTermsAcceptanceEntity.ProposedPlan.PlanCharges[i].IsSetupCharge);
                Assert.AreEqual(chargeList[i].PlanId, planTermsAcceptanceEntity.ProposedPlan.PlanCharges[i].PlanId);
            }

            Assert.AreEqual(scheduledPaymentList.Count, planTermsAcceptanceEntity.ProposedPlan.ScheduledPayments.Count);
            for (int j = 0; j < scheduledPaymentList.Count; j++)
            {
                Assert.AreEqual(scheduledPaymentList[j].Amount, planTermsAcceptanceEntity.ProposedPlan.ScheduledPayments[j].Amount);
                Assert.AreEqual(scheduledPaymentList[j].AmountPaid, planTermsAcceptanceEntity.ProposedPlan.ScheduledPayments[j].AmountPaid);
                Assert.AreEqual(scheduledPaymentList[j].DueDate, planTermsAcceptanceEntity.ProposedPlan.ScheduledPayments[j].DueDate);
                Assert.AreEqual(scheduledPaymentList[j].Id, planTermsAcceptanceEntity.ProposedPlan.ScheduledPayments[j].Id);
                Assert.AreEqual(scheduledPaymentList[j].IsPastDue, planTermsAcceptanceEntity.ProposedPlan.ScheduledPayments[j].IsPastDue);
                Assert.AreEqual(scheduledPaymentList[j].LastPaidDate, planTermsAcceptanceEntity.ProposedPlan.ScheduledPayments[j].LastPaidDate);
                Assert.AreEqual(scheduledPaymentList[j].PlanId, planTermsAcceptanceEntity.ProposedPlan.ScheduledPayments[j].PlanId);
            }

            Assert.AreEqual(statusList.Count, planTermsAcceptanceEntity.ProposedPlan.Statuses.Count);
            for (int k = 0; k < statusList.Count; k++)
            {
                Assert.AreEqual(statusList[k].Date, planTermsAcceptanceEntity.ProposedPlan.Statuses[k].Date);
                Assert.AreEqual(ConvertPlanStatusTypeDtoToPlanStatusTypeEntity(statusList[k].Status), planTermsAcceptanceEntity.ProposedPlan.Statuses[k].Status);
            }
        }

        [TestMethod]
        public void PaymentPlanTermsAcceptanceEntityAdapter_NullCollections()
        {
            planTermsAcceptanceDto.ProposedPlan.PlanCharges = null;
            planTermsAcceptanceDto.ProposedPlan.ScheduledPayments = null;
            planTermsAcceptanceDto.ProposedPlan.Statuses = null;
            planTermsAcceptanceEntity = planTermsAcceptanceDtoAdapter.MapToType(planTermsAcceptanceDto);
            Assert.AreEqual(0, planTermsAcceptanceEntity.ProposedPlan.PlanCharges.Count);
            Assert.AreEqual(0, planTermsAcceptanceEntity.ProposedPlan.ScheduledPayments.Count);

            Assert.AreEqual(1, planTermsAcceptanceEntity.ProposedPlan.Statuses.Count);
            for (int i = 0; i < planTermsAcceptanceEntity.ProposedPlan.Statuses.Count; i++)
            {
                Assert.AreEqual(DateTime.Today, planTermsAcceptanceEntity.ProposedPlan.Statuses[i].Date);
                Assert.AreEqual(Domain.Finance.Entities.PlanStatusType.Open, planTermsAcceptanceEntity.ProposedPlan.Statuses[i].Status);
            }
        }

        [TestMethod]
        public void PaymentPlanTermsAcceptanceEntityAdapter_EmptyCollections()
        {
            planTermsAcceptanceDto.ProposedPlan.PlanCharges = new List<PlanCharge>();
            planTermsAcceptanceDto.ProposedPlan.ScheduledPayments = new List<ScheduledPayment>();
            planTermsAcceptanceDto.ProposedPlan.Statuses = new List<PlanStatus>();
            planTermsAcceptanceEntity = planTermsAcceptanceDtoAdapter.MapToType(planTermsAcceptanceDto);
            Assert.AreEqual(0, planTermsAcceptanceEntity.ProposedPlan.PlanCharges.Count);
            Assert.AreEqual(0, planTermsAcceptanceEntity.ProposedPlan.ScheduledPayments.Count);

            Assert.AreEqual(1, planTermsAcceptanceEntity.ProposedPlan.Statuses.Count);
            for (int i = 0; i < planTermsAcceptanceEntity.ProposedPlan.Statuses.Count; i++)
            {
                Assert.AreEqual(DateTime.Today, planTermsAcceptanceEntity.ProposedPlan.Statuses[i].Date);
                Assert.AreEqual(Domain.Finance.Entities.PlanStatusType.Open, planTermsAcceptanceEntity.ProposedPlan.Statuses[i].Status);
            }
        }


        [TestMethod]
        public void PaymentPlanTermsAcceptanceEntityAdapter_RegistrationApprovalId()
        {
            Assert.AreEqual(planTermsAcceptanceDto.RegistrationApprovalId, planTermsAcceptanceEntity.RegistrationApprovalId);
        }

        [TestMethod]
        public void PaymentPlanTermsAcceptanceEntityAdapter_StudentId()
        {
            Assert.AreEqual(planTermsAcceptanceDto.StudentId, planTermsAcceptanceEntity.StudentId);
        }

        [TestMethod]
        public void PaymentPlanTermsAcceptanceEntityAdapter_StudentName()
        {
            Assert.AreEqual(planTermsAcceptanceDto.StudentName, planTermsAcceptanceEntity.StudentName);
        }

        [TestMethod]
        public void PaymentPlanTermsAcceptanceEntityAdapter_TermsText()
        {
            CollectionAssert.AreEqual(planTermsAcceptanceDto.TermsText, planTermsAcceptanceEntity.TermsText);
        }

        [TestMethod]
        public void PaymentPlanTermsAcceptanceEntityAdapter_NoPaymentControlId()
        {
            planTermsAcceptanceDto.PaymentControlId = null;
            planTermsAcceptanceEntity = planTermsAcceptanceDtoAdapter.MapToType(planTermsAcceptanceDto);
            Assert.IsNull(planTermsAcceptanceDto.PaymentControlId, planTermsAcceptanceEntity.PaymentControlId);
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

        private Domain.Finance.Entities.PlanFrequency ConvertPlanFrequencyDtoToPlanFrequencyEntity(PlanFrequency source)
        {
            switch (source)
            {
                case PlanFrequency.Biweekly:
                    return Domain.Finance.Entities.PlanFrequency.Biweekly;
                case PlanFrequency.Custom:
                    return Domain.Finance.Entities.PlanFrequency.Custom;
                case PlanFrequency.Monthly:
                    return Domain.Finance.Entities.PlanFrequency.Monthly;
                case PlanFrequency.Weekly:
                    return Domain.Finance.Entities.PlanFrequency.Weekly;
                default:
                    return Domain.Finance.Entities.PlanFrequency.Yearly;
            }
        }

    }
}