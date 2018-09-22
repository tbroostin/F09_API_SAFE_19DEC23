// Copyright 2014 Ellucian Company L.P. and its affiliates.
using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ellucian.Colleague.Domain.Finance.Entities;

namespace Ellucian.Colleague.Domain.Finance.Tests.Entities
{
    [TestClass]
    public class PaymentPlanTests
    {
        static string id = "123";
        static string templateId = "DEFAULT";
        static decimal originalAmount = 10000m;
        static string personId = "0003315";
        static string receivableTypeCode = "01";
        static string termId = "2013/FA";
        static DateTime firstDueDate = DateTime.Today.AddDays(5);
        static DateTime statusDate = DateTime.Today;
        static List<PlanStatus> planStatuses = new List<PlanStatus>() { new PlanStatus(PlanStatusType.Open, DateTime.Today) };
        static PlanStatus newStatus = new PlanStatus(PlanStatusType.Cancelled, DateTime.Today.AddMonths(5));

        static Charge charge1 = new Charge("234", "345", new List<string>() { "Charge Description" }, "ARCOD", 100m);
        static Charge charge2 = new Charge("235", "345", new List<string>() { "Charge Description" }, "ARCOD", 100m);
        static Charge charge3 = new Charge("236", "345", new List<string>() { "Charge Description" }, "ARCOD", 100m);
        static Charge newCharge1 = new Charge("237", "345", new List<string>() { "New Charge" }, "ARCD2", 120m);

        static PlanCharge planCharge1 = new PlanCharge("123", charge1, 25m, false, true);
        static PlanCharge planCharge2 = new PlanCharge("123", charge2, 1000m, false, true);
        static PlanCharge planCharge3 = new PlanCharge("124", charge3, 1500m, false, true);
        static PlanCharge newPlanCharge1 = new PlanCharge(null, newCharge1, 1500m, false, true);
        static List<PlanCharge> planCharges = new List<PlanCharge>() { planCharge1, planCharge2 };

        static PlanCharge planChargeNoId1 = new PlanCharge(null, charge1, 25m, false, true);
        static PlanCharge planChargeNoId2 = new PlanCharge(string.Empty, charge2, 1000m, false, true);
        static List<PlanCharge> planChargesNoIds = new List<PlanCharge>() { planChargeNoId1, planChargeNoId2 };

        static ScheduledPayment scheduledPayment1 = new ScheduledPayment("236", "123", 500m, DateTime.Today.AddDays(7), 50m, null);
        static ScheduledPayment scheduledPayment2 = new ScheduledPayment("237", "123", 1500m, DateTime.Today.AddDays(14), 0m, null);
        static ScheduledPayment scheduledPayment3 = new ScheduledPayment("238", "124", 500m, DateTime.Today.AddDays(3), 0m, null);
        static List<ScheduledPayment> scheduledPayments = new List<ScheduledPayment>() { scheduledPayment1, scheduledPayment2 };

        static ScheduledPayment scheduledPayment4 = new ScheduledPayment("239", "123", 1500m, DateTime.Today.AddDays(7), 0m, null);
        static ScheduledPayment scheduledPayment5 = new ScheduledPayment("240", "123", 1500m, DateTime.Today.AddDays(14), 0m, null);
        static ScheduledPayment scheduledPayment6 = new ScheduledPayment("241", "123", 1500m, DateTime.Today.AddDays(21), 0m, null);
        static List<ScheduledPayment> replacementSchedule = new List<ScheduledPayment>() { scheduledPayment4, scheduledPayment5, scheduledPayment6 };

        static ScheduledPayment scheduledPayment7 = new ScheduledPayment(null, null, 1500m, DateTime.Today.AddDays(7), 0m, null);
        static ScheduledPayment scheduledPayment8 = new ScheduledPayment(null, null, 1500m, DateTime.Today.AddDays(14), 0m, null);
        static ScheduledPayment scheduledPayment9 = new ScheduledPayment(null, null, 1500m, DateTime.Today.AddDays(21), 0m, null);
        static List<ScheduledPayment> proposedSchedule = new List<ScheduledPayment>() { scheduledPayment7, scheduledPayment8, scheduledPayment9 };

        [TestInitialize]
        public void Initialize()
        {
        }

        [TestMethod]
        public void PaymentPlan_Constructor_ValidId()
        {
            var result = new PaymentPlan(id, templateId, personId, receivableTypeCode, termId, originalAmount, firstDueDate, planStatuses, scheduledPayments, planCharges);

            Assert.AreEqual(id, result.Id);
        }

        [TestMethod]
        public void PaymentPlan_Constructor_SetIdAfterInitialization()
        {
            var result = new PaymentPlan(null, templateId, personId, receivableTypeCode, termId, originalAmount, firstDueDate, planStatuses, null, null);
            result.Id = id;

            Assert.AreEqual(id, result.Id);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void PaymentPlan_Constructor_TryToChangeId()
        {
            var result = new PaymentPlan(id, templateId, personId, receivableTypeCode, termId, originalAmount, firstDueDate, planStatuses, scheduledPayments, planCharges);
            result.Id = "234";
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void PaymentPlan_Constructor_NullTemplateId()
        {
            var result = new PaymentPlan(id, null, personId, receivableTypeCode, termId, originalAmount, firstDueDate, planStatuses, scheduledPayments, planCharges);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void PaymentPlan_Constructor_EmptyTemplateId()
        {
            var result = new PaymentPlan(id, string.Empty, personId, receivableTypeCode, termId, originalAmount, firstDueDate, planStatuses, scheduledPayments, planCharges);
        }

        [TestMethod]
        public void PaymentPlan_Constructor_ValidTemplateId()
        {
            var result = new PaymentPlan(id, templateId, personId, receivableTypeCode, termId, originalAmount, firstDueDate, planStatuses, scheduledPayments, planCharges);

            Assert.AreEqual(templateId, result.TemplateId);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void PaymentPlan_Constructor_NullPersonId()
        {
            var result = new PaymentPlan(id, templateId, null, receivableTypeCode, termId, originalAmount, firstDueDate, planStatuses, scheduledPayments, planCharges);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void PaymentPlan_Constructor_EmptyPersonId()
        {
            var result = new PaymentPlan(id, templateId, string.Empty, receivableTypeCode, termId, originalAmount, firstDueDate, planStatuses, scheduledPayments, planCharges);
        }

        [TestMethod]
        public void PaymentPlan_Constructor_ValidPersonId()
        {
            var result = new PaymentPlan(id, templateId, personId, receivableTypeCode, termId, originalAmount, firstDueDate, planStatuses, scheduledPayments, planCharges);

            Assert.AreEqual(personId, result.PersonId);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void PaymentPlan_Constructor_NullReceivableTypeCode()
        {
            var result = new PaymentPlan(id, templateId, personId, null, termId, originalAmount, firstDueDate, planStatuses, scheduledPayments, planCharges);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void PaymentPlan_Constructor_EmptyReceivableTypeCode()
        {
            var result = new PaymentPlan(id, templateId, personId, string.Empty, termId, originalAmount, firstDueDate, planStatuses, scheduledPayments, planCharges);
        }

        [TestMethod]
        public void PaymentPlan_Constructor_ValidReceivableTypeCode()
        {
            var result = new PaymentPlan(id, templateId, personId, receivableTypeCode, termId, originalAmount, firstDueDate, planStatuses, scheduledPayments, planCharges);

            Assert.AreEqual(receivableTypeCode, result.ReceivableTypeCode);
        }

        [TestMethod]
        public void PaymentPlan_Constructor_ValidNullTerm()
        {
            var result = new PaymentPlan(id, templateId, personId, receivableTypeCode, null, originalAmount, firstDueDate, planStatuses, scheduledPayments, planCharges);

            Assert.IsNull(result.TermId);
        }

        [TestMethod]
        public void PaymentPlan_Constructor_ValidEmptyTerm()
        {
            var result = new PaymentPlan(id, templateId, personId, receivableTypeCode, string.Empty, originalAmount, firstDueDate, planStatuses, scheduledPayments, planCharges);

            Assert.AreEqual(String.Empty, result.TermId);
        }

        [TestMethod]
        public void PaymentPlan_Constructor_ValidNonNullTerm()
        {
            var result = new PaymentPlan(id, templateId, personId, receivableTypeCode, termId, originalAmount, firstDueDate, planStatuses, scheduledPayments, planCharges);

            Assert.AreEqual(termId, result.TermId);
        }

        [TestMethod]
        public void PaymentPlan_Constructor_ValidOriginalAmount()
        {
            var result = new PaymentPlan(id, templateId, personId, receivableTypeCode, termId, originalAmount, firstDueDate, planStatuses, scheduledPayments, planCharges);

            Assert.AreEqual(originalAmount, result.OriginalAmount);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void PaymentPlan_Constructor_InvalidOriginalAmount()
        {
            var result = new PaymentPlan(id, templateId, personId, receivableTypeCode, termId, -1, firstDueDate, planStatuses, scheduledPayments, planCharges);
        }

        [TestMethod]
        public void PaymentPlan_Constructor_ValidFirstDueDate()
        {
            var result = new PaymentPlan(id, templateId, personId, receivableTypeCode, termId, originalAmount, firstDueDate, planStatuses, scheduledPayments, planCharges);
            Assert.AreEqual(firstDueDate, result.FirstDueDate);
        }

        [TestMethod]
        public void PaymentPlan_Constructor_NullStatuses()
        {
            var result = new PaymentPlan(id, templateId, personId, receivableTypeCode, termId, originalAmount, firstDueDate, null, scheduledPayments, planCharges);

            Assert.IsNotNull(result.Statuses);
            Assert.AreEqual(1, result.Statuses.Count);
            Assert.IsInstanceOfType(result.Statuses[0], typeof(PlanStatus));
            Assert.AreEqual(PlanStatusType.Open, result.Statuses[0].Status);
            Assert.AreEqual(DateTime.Today.Date, result.Statuses[0].Date.Date);
        }

        [TestMethod]
        public void PaymentPlan_Constructor_EmptyStatuses()
        {
            var result = new PaymentPlan(id, templateId, personId, receivableTypeCode, termId, originalAmount, firstDueDate, new List<PlanStatus>(), scheduledPayments, planCharges);

            Assert.IsNotNull(result.Statuses);
            Assert.AreEqual(1, result.Statuses.Count);
            Assert.IsInstanceOfType(result.Statuses[0], typeof(PlanStatus));
            Assert.AreEqual(PlanStatusType.Open, result.Statuses[0].Status);
            Assert.AreEqual(DateTime.Today.Date, result.Statuses[0].Date.Date);
        }

        [TestMethod]
        public void PaymentPlan_Constructor_ValidStatusesOneStatus()
        {
            var result = new PaymentPlan(id, templateId, personId, receivableTypeCode, termId, originalAmount, firstDueDate, planStatuses, scheduledPayments, planCharges);

            Assert.IsNotNull(result.Statuses);
            Assert.AreEqual(planStatuses.Count, result.Statuses.Count);
            CollectionAssert.AllItemsAreNotNull(result.Statuses);
            CollectionAssert.AllItemsAreInstancesOfType(result.Statuses, typeof(PlanStatus));
            CollectionAssert.AreEqual(planStatuses, result.Statuses);
        }

        [TestMethod]
        public void PaymentPlan_Constructor_ValidStatusesMultipleStatuses()
        {
            planStatuses.Add(new PlanStatus(PlanStatusType.Paid, DateTime.Today.AddDays(100)));
            planStatuses.Reverse();

            var result = new PaymentPlan(id, templateId, personId, receivableTypeCode, termId, originalAmount, firstDueDate, planStatuses, scheduledPayments, planCharges);

            Assert.IsNotNull(result.Statuses);
            Assert.AreEqual(planStatuses.Count, result.Statuses.Count);
            CollectionAssert.AllItemsAreNotNull(result.Statuses);
            CollectionAssert.AllItemsAreInstancesOfType(result.Statuses, typeof(PlanStatus));
            CollectionAssert.AreEqual(planStatuses, result.Statuses);
        }

        [TestMethod]
        public void PaymentPlan_Constructor_CurrentStatus()
        {
            var result = new PaymentPlan(id, templateId, personId, receivableTypeCode, termId, originalAmount, firstDueDate, planStatuses, scheduledPayments, planCharges);

            Assert.AreEqual(planStatuses[0].Status, result.CurrentStatus);
        }

        [TestMethod]
        public void PaymentPlan_Constructor_CurrentStatusDate()
        {
            var result = new PaymentPlan(id, templateId, personId, receivableTypeCode, termId, originalAmount, firstDueDate, planStatuses, scheduledPayments, planCharges);

            Assert.AreEqual(planStatuses[0].Date, result.CurrentStatusDate);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void PaymentPlan_Constructor_NullChargeNonNullId()
        {
            var result = new PaymentPlan(id, templateId, personId, receivableTypeCode, termId, originalAmount, firstDueDate, planStatuses, scheduledPayments, null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void PaymentPlan_Constructor_NonNullChargePlanIdNullId()
        {
            var result = new PaymentPlan(null, templateId, personId, receivableTypeCode, termId, originalAmount, firstDueDate, planStatuses, null, planCharges);
        }

        [TestMethod]
        public void PaymentPlan_Constructor_NullChargeNullId()
        {
            var result = new PaymentPlan(null, templateId, personId, receivableTypeCode, termId, originalAmount, firstDueDate, planStatuses, null, null);

            Assert.IsNotNull(result.PlanCharges);
            Assert.AreEqual(0, result.PlanCharges.Count);
        }

        [TestMethod]
        public void PaymentPlan_Constructor_NullChargePlanIdNullId()
        {
            var result = new PaymentPlan(null, templateId, personId, receivableTypeCode, termId, originalAmount, firstDueDate, planStatuses, null, planChargesNoIds);

            Assert.IsNotNull(result.PlanCharges);
            Assert.AreEqual(planChargesNoIds.Count, result.PlanCharges.Count);
            CollectionAssert.AllItemsAreNotNull(result.PlanCharges);
            CollectionAssert.AllItemsAreInstancesOfType(result.PlanCharges, typeof(PlanCharge));
            CollectionAssert.AllItemsAreUnique(result.PlanCharges);
            CollectionAssert.AreEqual(planChargesNoIds, result.PlanCharges);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void PaymentPlan_Constructor_NonNullScheduledPaymentNullId()
        {
            var result = new PaymentPlan(null, templateId, personId, receivableTypeCode, termId, originalAmount, firstDueDate, planStatuses, scheduledPayments, planCharges);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void PaymentPlan_Constructor_NullScheduledPaymentNonNullId()
        {
            var result = new PaymentPlan(id, templateId, personId, receivableTypeCode, termId, originalAmount, firstDueDate, planStatuses, null, planCharges);
        }

        [TestMethod]
        public void PaymentPlan_Constructor_NullScheduledPaymentNullId()
        {
            var result = new PaymentPlan(null, templateId, personId, receivableTypeCode, termId, originalAmount, firstDueDate, planStatuses, null, null);

            Assert.IsNotNull(result.ScheduledPayments);
            Assert.AreEqual(0, result.ScheduledPayments.Count);
        }

        [TestMethod]
        public void PaymentPlan_Constructor_NonNullScheduledPaymentNonNullId()
        {
            var result = new PaymentPlan(id, templateId, personId, receivableTypeCode, termId, originalAmount, firstDueDate, planStatuses, scheduledPayments, planCharges);

            Assert.IsNotNull(result.ScheduledPayments);
            Assert.AreEqual(scheduledPayments.Count, result.ScheduledPayments.Count);
            CollectionAssert.AllItemsAreNotNull(result.ScheduledPayments);
            CollectionAssert.AllItemsAreInstancesOfType(result.ScheduledPayments, typeof(ScheduledPayment));
            CollectionAssert.AllItemsAreUnique(result.ScheduledPayments);
            CollectionAssert.AreEqual(scheduledPayments, result.ScheduledPayments);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void PaymentPlan_AddStatus_NullStatus()
        {
            var result = new PaymentPlan(id, templateId, personId, receivableTypeCode, termId, originalAmount, firstDueDate, planStatuses, scheduledPayments, planCharges);
            result.AddStatus(null);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void PaymentPlan_AddStatus_CurrentStatusIsCancelled()
        {
            var result = new PaymentPlan(id, templateId, personId, receivableTypeCode, termId, originalAmount, firstDueDate, planStatuses, scheduledPayments, planCharges);
            result.AddStatus(new PlanStatus(PlanStatusType.Cancelled, DateTime.Today.AddMonths(-1)));

            result.AddStatus(new PlanStatus(PlanStatusType.Open, DateTime.Today));
        }

        [TestMethod]
        public void PaymentPlan_AddStatus_CurrentStatus()
        {
            var result = new PaymentPlan(id, templateId, personId, receivableTypeCode, termId, originalAmount, firstDueDate, planStatuses, scheduledPayments, planCharges);
            result.AddStatus(newStatus);

            Assert.AreEqual(newStatus.Status, result.CurrentStatus);
        }

        [TestMethod]
        public void PaymentPlan_AddStatus_CurrentStatusDate()
        {
            var result = new PaymentPlan(id, templateId, personId, receivableTypeCode, termId, originalAmount, firstDueDate, planStatuses, scheduledPayments, planCharges);
            result.AddStatus(newStatus);

            Assert.AreEqual(newStatus.Date, result.CurrentStatusDate);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void PaymentPlan_AddPlanCharge_NullPlanCharge()
        {
            var result = new PaymentPlan(id, templateId, personId, receivableTypeCode, termId, originalAmount, firstDueDate, planStatuses, scheduledPayments, planCharges);
            result.AddPlanCharge(null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void PaymentPlan_AddPlanCharge_NullPlanChargePlanId_ValidPlanId()
        {
            var result = new PaymentPlan(id, templateId, personId, receivableTypeCode, termId, originalAmount, firstDueDate, planStatuses, scheduledPayments, planCharges);
            var planCharge = new PlanCharge(null, charge3, 1500m, false, true);
            result.AddPlanCharge(planCharge);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void PaymentPlan_AddPlanCharge_EmptyPlanChargePlanId_ValidPlanId()
        {
            var result = new PaymentPlan(id, templateId, personId, receivableTypeCode, termId, originalAmount, firstDueDate, planStatuses, scheduledPayments, planCharges);
            var planCharge = new PlanCharge(string.Empty, charge3, 1500m, false, true);
            result.AddPlanCharge(planCharge);
        }

        [TestMethod]
        public void PaymentPlan_AddPlanCharge_NullPlanChargePlanId_NullPlanId()
        {
            var result = new PaymentPlan(null, templateId, personId, receivableTypeCode, termId, originalAmount, firstDueDate, planStatuses, null, null);
            var planCharge = new PlanCharge(null, charge3, 1500m, false, true);
            result.AddPlanCharge(planCharge);
            Assert.AreEqual(1, result.PlanCharges.Count);
            Assert.AreEqual(planCharge, result.PlanCharges[0]);
        }

        [TestMethod]
        public void PaymentPlan_AddPlanCharge_NullPlanChargePlanId_EmptyPlanId()
        {
            var result = new PaymentPlan(string.Empty, templateId, personId, receivableTypeCode, termId, originalAmount, firstDueDate, planStatuses, null, null);
            var planCharge = new PlanCharge(null, charge3, 1500m, false, true);
            result.AddPlanCharge(planCharge);
            Assert.AreEqual(1, result.PlanCharges.Count);
            Assert.AreEqual(planCharge, result.PlanCharges[0]);
        }

        [TestMethod]
        public void PaymentPlan_AddPlanCharge_EmptyPlanChargePlanId_NullPlanId()
        {
            var result = new PaymentPlan(null, templateId, personId, receivableTypeCode, termId, originalAmount, firstDueDate, planStatuses, null, null);
            var planCharge = new PlanCharge(string.Empty, charge3, 1500m, false, true);
            result.AddPlanCharge(planCharge);
            Assert.AreEqual(1, result.PlanCharges.Count);
            Assert.AreEqual(planCharge, result.PlanCharges[0]);
        }

        [TestMethod]
        public void PaymentPlan_AddPlanCharge_EmptyPlanChargePlanId_EmptyPlanId()
        {
            var result = new PaymentPlan(string.Empty, templateId, personId, receivableTypeCode, termId, originalAmount, firstDueDate, planStatuses, null, null);
            var planCharge = new PlanCharge(string.Empty, charge3, 1500m, false, true);
            result.AddPlanCharge(planCharge);
            Assert.AreEqual(1, result.PlanCharges.Count);
            Assert.AreEqual(planCharge, result.PlanCharges[0]);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void PaymentPlan_AddPlanCharge_ValidPlanChargePlanId_NullPlanId()
        {
            var result = new PaymentPlan(null, templateId, personId, receivableTypeCode, termId, originalAmount, firstDueDate, planStatuses, null, null);
            var planCharge = new PlanCharge(id, charge3, 1500m, false, true);
            result.AddPlanCharge(planCharge);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void PaymentPlan_AddPlanCharge_ValidPlanChargePlanId_EmptyPlanId()
        {
            var result = new PaymentPlan(string.Empty, templateId, personId, receivableTypeCode, termId, originalAmount, firstDueDate, planStatuses, null, null);
            var planCharge = new PlanCharge(id, charge3, 1500m, false, true);
            result.AddPlanCharge(planCharge);
        }

        [TestMethod]
        public void PaymentPlan_AddPlanCharge_ValidPlanChargePlanId_MatchingPlanId()
        {
            var result = new PaymentPlan(id, templateId, personId, receivableTypeCode, termId, originalAmount, firstDueDate, planStatuses, scheduledPayments, planCharges);
            var planCharge = new PlanCharge(id, charge3, 1500m, false, true);
            result.AddPlanCharge(planCharge);
            Assert.AreEqual(3, result.PlanCharges.Count);
            Assert.AreEqual(planCharge, result.PlanCharges[2]);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void PaymentPlan_AddPlanCharge_ValidPlanChargePlanId_NonmatchingPlanId()
        {
            var result = new PaymentPlan(id, templateId, personId, receivableTypeCode, termId, originalAmount, firstDueDate, planStatuses, scheduledPayments, planCharges);
            var planCharge = new PlanCharge(id+"A", charge3, 1500m, false, true);
            result.AddPlanCharge(planCharge);
        }

        [TestMethod]
        public void PaymentPlan_AddPlanCharge_DuplicatePlanCharge()
        {
            var result = new PaymentPlan(id, templateId, personId, receivableTypeCode, termId, originalAmount, firstDueDate, planStatuses, scheduledPayments, planCharges);
            var planCharge = new PlanCharge(id, charge3, 1500m, false, true);
            result.AddPlanCharge(planCharge);
            result.AddPlanCharge(planCharge);
            Assert.AreEqual(3, result.PlanCharges.Count);
            Assert.AreEqual(planCharge, result.PlanCharges[2]);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void PaymentPlan_AddScheduledPayment_NullScheduledPayment()
        {
            var result = new PaymentPlan(id, templateId, personId, receivableTypeCode, termId, originalAmount, firstDueDate, planStatuses, scheduledPayments, planCharges);
            result.AddScheduledPayment(null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void PaymentPlan_AddScheduledPayment_NullScheduledPaymentPlanId_ValidPlanId()
        {
            var result = new PaymentPlan(id, templateId, personId, receivableTypeCode, termId, originalAmount, firstDueDate, planStatuses, scheduledPayments, planCharges);
            var scheduledPayment = new ScheduledPayment(null, null, 500m, DateTime.Today.AddDays(7), 50m, null);
            result.AddScheduledPayment(scheduledPayment);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void PaymentPlan_AddScheduledPayment_EmptyScheduledPaymentPlanId_ValidPlanId()
        {
            var result = new PaymentPlan(id, templateId, personId, receivableTypeCode, termId, originalAmount, firstDueDate, planStatuses, scheduledPayments, planCharges);
            var scheduledPayment = new ScheduledPayment(null, string.Empty, 500m, DateTime.Today.AddDays(7), 50m, null);
            result.AddScheduledPayment(scheduledPayment);
        }

        [TestMethod]
        public void PaymentPlan_AddScheduledPayment_NullScheduledPaymentPlanId_NullPlanId()
        {
            var result = new PaymentPlan(null, templateId, personId, receivableTypeCode, termId, originalAmount, firstDueDate, planStatuses, null, null);
            var scheduledPayment = new ScheduledPayment(null, null, 500m, DateTime.Today.AddDays(7), 50m, null);
            result.AddScheduledPayment(scheduledPayment);
            Assert.AreEqual(1, result.ScheduledPayments.Count);
            Assert.AreEqual(scheduledPayment, result.ScheduledPayments[0]);
        }

        [TestMethod]
        public void PaymentPlan_AddScheduledPayment_NullScheduledPaymentPlanId_EmptyPlanId()
        {
            var result = new PaymentPlan(string.Empty, templateId, personId, receivableTypeCode, termId, originalAmount, firstDueDate, planStatuses, null, null);
            var scheduledPayment = new ScheduledPayment(null, null, 500m, DateTime.Today.AddDays(7), 50m, null);
            result.AddScheduledPayment(scheduledPayment);
            Assert.AreEqual(1, result.ScheduledPayments.Count);
            Assert.AreEqual(scheduledPayment, result.ScheduledPayments[0]);
        }

        [TestMethod]
        public void PaymentPlan_AddScheduledPayment_EmptyScheduledPaymentPlanId_NullPlanId()
        {
            var result = new PaymentPlan(null, templateId, personId, receivableTypeCode, termId, originalAmount, firstDueDate, planStatuses, null, null);
            var scheduledPayment = new ScheduledPayment(null, string.Empty, 500m, DateTime.Today.AddDays(7), 50m, null);
            result.AddScheduledPayment(scheduledPayment);
            Assert.AreEqual(1, result.ScheduledPayments.Count);
            Assert.AreEqual(scheduledPayment, result.ScheduledPayments[0]);
        }

        [TestMethod]
        public void PaymentPlan_AddScheduledPayment_EmptyScheduledPaymentPlanId_EmptyPlanId()
        {
            var result = new PaymentPlan(string.Empty, templateId, personId, receivableTypeCode, termId, originalAmount, firstDueDate, planStatuses, null, null);
            var scheduledPayment = new ScheduledPayment(null, string.Empty, 500m, DateTime.Today.AddDays(7), 50m, null);
            result.AddScheduledPayment(scheduledPayment);
            Assert.AreEqual(1, result.ScheduledPayments.Count);
            Assert.AreEqual(scheduledPayment, result.ScheduledPayments[0]);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void PaymentPlan_AddScheduledPayment_ValidScheduledPaymentPlanId_NullPlanId()
        {
            var result = new PaymentPlan(null, templateId, personId, receivableTypeCode, termId, originalAmount, firstDueDate, planStatuses, null, null);
            var scheduledPayment = new ScheduledPayment("236", id, 500m, DateTime.Today.AddDays(7), 50m, null);
            result.AddScheduledPayment(scheduledPayment);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void PaymentPlan_AddScheduledPayment_ValidScheduledPaymentPlanId_EmptyPlanId()
        {
            var result = new PaymentPlan(string.Empty, templateId, personId, receivableTypeCode, termId, originalAmount, firstDueDate, planStatuses, null, null);
            var scheduledPayment = new ScheduledPayment("236", id, 500m, DateTime.Today.AddDays(7), 50m, null);
            result.AddScheduledPayment(scheduledPayment);
        }

        [TestMethod]
        public void PaymentPlan_AddScheduledPayment_ValidScheduledPaymentPlanId_MatchingPlanId()
        {
            var result = new PaymentPlan(id, templateId, personId, receivableTypeCode, termId, originalAmount, firstDueDate, planStatuses, scheduledPayments, planCharges);
            var scheduledPayment = new ScheduledPayment("236", id, 500m, DateTime.Today.AddDays(7), 50m, null);
            result.AddScheduledPayment(scheduledPayment);
            Assert.AreEqual(2, result.ScheduledPayments.Count);
            Assert.AreEqual(scheduledPayment, result.ScheduledPayments[0]);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void PaymentPlan_AddScheduledPayment_ValidScheduledPaymentPlanId_NonmatchingPlanId()
        {
            var result = new PaymentPlan(id, templateId, personId, receivableTypeCode, termId, originalAmount, firstDueDate, planStatuses, scheduledPayments, planCharges);
            var scheduledPayment = new ScheduledPayment("236", id+"A", 500m, DateTime.Today.AddDays(7), 50m, null);
            result.AddScheduledPayment(scheduledPayment);
        }

        [TestMethod]
        public void PaymentPlan_AddScheduledPayment_DuplicateScheduledPayment()
        {
            var result = new PaymentPlan(id, templateId, personId, receivableTypeCode, termId, originalAmount, firstDueDate, planStatuses, scheduledPayments, planCharges);
            var scheduledPayment = new ScheduledPayment("236", id, 500m, DateTime.Today.AddDays(7), 50m, null);
            result.AddScheduledPayment(scheduledPayment);
            result.AddScheduledPayment(scheduledPayment);
            Assert.AreEqual(2, result.ScheduledPayments.Count);
            Assert.AreEqual(scheduledPayment, result.ScheduledPayments[0]);
        }

        [TestMethod]
        public void PaymentPlan_TotalSetupChargeAmount_UnassignedAttributeValues()
        {
            var result = new PaymentPlan(id, templateId, personId, receivableTypeCode, termId, originalAmount, firstDueDate, planStatuses, scheduledPayments, planCharges);
            Assert.AreEqual(0m, result.TotalSetupChargeAmount);
        }

        [TestMethod]
        public void PaymentPlan_TotalSetupChargeAmount_FixedSetupCharge()
        {
            var result = new PaymentPlan(id, templateId, personId, receivableTypeCode, termId, originalAmount, firstDueDate, planStatuses, scheduledPayments, planCharges);
            result.SetupAmount = 100m;
            Assert.AreEqual(100m, result.TotalSetupChargeAmount);
        }

        [TestMethod]
        public void PaymentPlan_TotalSetupChargeAmount_PercentageSetupCharge()
        {
            var result = new PaymentPlan(id, templateId, personId, receivableTypeCode, termId, originalAmount, firstDueDate, planStatuses, scheduledPayments, planCharges);
            result.SetupPercentage = 10m;
            Assert.AreEqual( 1000m, result.TotalSetupChargeAmount);
        }

        [TestMethod]
        public void PaymentPlan_TotalSetupChargeAmount_FixedAndPercentageSetupCharge()
        {
            var result = new PaymentPlan(id, templateId, personId, receivableTypeCode, termId, originalAmount, firstDueDate, planStatuses, scheduledPayments, planCharges);
            result.SetupAmount = 100m;
            result.SetupPercentage = 10m;
            Assert.AreEqual(1100m, result.TotalSetupChargeAmount);
        }

        [TestMethod]
        public void PaymentPlan_DownPaymentAmount_ZeroDownPaymentPercentage()
        {
            var result = new PaymentPlan(id, templateId, personId, receivableTypeCode, termId, originalAmount, firstDueDate, planStatuses, scheduledPayments, planCharges);
            result.DownPaymentPercentage = 0m;
            Assert.AreEqual(0m, result.DownPaymentAmount);
        }

        [TestMethod]
        public void PaymentPlan_DownPaymentAmount_NonzeroDownPaymentPercentage()
        {
            var result = new PaymentPlan(id, templateId, personId, receivableTypeCode, termId, originalAmount, firstDueDate, planStatuses, scheduledPayments, planCharges);
            result.DownPaymentPercentage = 10m;
            Assert.AreEqual(scheduledPayments[0].Amount, result.DownPaymentAmount);
        }

        [TestMethod]
        public void PaymentPlan_DownPaymentAmountPaid_ZeroDownPaymentPercentage()
        {
            var result = new PaymentPlan(id, templateId, personId, receivableTypeCode, termId, originalAmount, firstDueDate, planStatuses, scheduledPayments, planCharges);
            result.DownPaymentPercentage = 0m;
            Assert.AreEqual(0m, result.DownPaymentAmountPaid);
        }

        [TestMethod]
        public void PaymentPlan_DownPaymentAmountPaid_NonzeroDownPaymentPercentage()
        {
            var result = new PaymentPlan(id, templateId, personId, receivableTypeCode, termId, originalAmount, firstDueDate, planStatuses, scheduledPayments, planCharges);
            result.DownPaymentPercentage = 10m;
            Assert.AreEqual(scheduledPayments[0].AmountPaid, result.DownPaymentAmountPaid);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void PaymentPlan_ReplaceSchedule_NullSchedule()
        {
            var result = new PaymentPlan(id, templateId, personId, receivableTypeCode, termId, originalAmount, firstDueDate, planStatuses, scheduledPayments, planCharges);
            result.ReplaceSchedule(null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void PaymentPlan_ReplaceSchedule_EmptySchedule()
        {
            var result = new PaymentPlan(id, templateId, personId, receivableTypeCode, termId, originalAmount, firstDueDate, planStatuses, scheduledPayments, planCharges);
            result.ReplaceSchedule(new List<ScheduledPayment>());
        }

        [TestMethod]
        public void PaymentPlan_ReplaceSchedule_ValidSchedule()
        {
            var result = new PaymentPlan(id, templateId, personId, receivableTypeCode, termId, originalAmount, firstDueDate, planStatuses, scheduledPayments, planCharges);
            result.ReplaceSchedule(replacementSchedule);
            Assert.AreEqual(replacementSchedule.Count, result.ScheduledPayments.Count);
            for (int i = 0; i < result.ScheduledPayments.Count; i++)
            {
                Assert.AreEqual(replacementSchedule[i], result.ScheduledPayments[i]);
            }
        }

        [TestMethod]
        public void PaymentPlan_DownPaymentDate_ZeroDownPayment()
        {
            var result = new PaymentPlan(id, templateId, personId, receivableTypeCode, termId, originalAmount, firstDueDate, planStatuses, scheduledPayments, planCharges) { DownPaymentPercentage = 0 };
            Assert.AreEqual(null, result.DownPaymentDate);
        }

        [TestMethod]
        public void PaymentPlan_DownPaymentDate_NonzeroDownPayment()
        {
            var result = new PaymentPlan(id, templateId, personId, receivableTypeCode, termId, originalAmount, firstDueDate, planStatuses, scheduledPayments, planCharges) { DownPaymentPercentage = 10 };
            Assert.AreEqual(scheduledPayments[0].DueDate, result.DownPaymentDate);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void PaymentPlan_Id_ValidIdAlreadySetByConstructor()
        {
            var result = new PaymentPlan(id, templateId, personId, receivableTypeCode, termId, originalAmount, firstDueDate, planStatuses, scheduledPayments, planCharges) { DownPaymentPercentage = 10 };
            result.Id = id + "A";
        }

        [TestMethod]
        public void PaymentPlan_Id_NullIdExplicitySetOutsideConstructor()
        {
            var result = new PaymentPlan(null, templateId, personId, receivableTypeCode, termId, originalAmount, firstDueDate, planStatuses, null, null);
            result.Id = id;
            Assert.AreEqual(id, result.Id);
        }

        [TestMethod]
        public void PaymentPlan_Id_EmptyIdExplicitySetOutsideConstructorAndPlanIdsUpdatedOnExistingCollections()
        {
            var result = new PaymentPlan(string.Empty, templateId, personId, receivableTypeCode, termId, originalAmount, firstDueDate, planStatuses, null, null);
            result.AddScheduledPayment(new ScheduledPayment(null, null, 1000m, DateTime.Today.AddDays(14), 0m, null));
            result.AddPlanCharge(new PlanCharge(null, charge1, 25m, false, true));
            result.Id = id;
            Assert.AreEqual(id, result.Id);
            Assert.AreEqual(id, result.ScheduledPayments[0].PlanId);
            Assert.AreEqual(id, result.PlanCharges[0].PlanId);
        }

        [TestMethod]
        public void PaymentPlan_Id_EmptyIdExplicitySetOutsideConstructorAndExistingCollectionsUntouched()
        {
            var result = new PaymentPlan(string.Empty, templateId, personId, receivableTypeCode, termId, originalAmount, firstDueDate, planStatuses, null, null);
            result.Id = id;
            Assert.AreEqual(0, result.ScheduledPayments.Count);
            Assert.AreEqual(0, result.PlanCharges.Count);
        }
    }
}
