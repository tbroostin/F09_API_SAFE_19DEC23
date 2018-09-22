// Copyright 2012-2014 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ellucian.Colleague.Domain.Student.Entities;

namespace Ellucian.Colleague.Domain.Student.Tests.Entities
{
    [TestClass]
    public class RegistrationEligibility_BaseTests
    {
        private string messageText1;
        private string messageText2;
        private bool hasOverride;
        private bool isEligible;
        private RegistrationEligibility regEligibility;

        [TestInitialize]
        public void Initialize()
        {
            messageText1 = "business office hold";
            messageText2 = "registration not allowed";
            var regMessageList = new List<RegistrationMessage>() {
                new RegistrationMessage() {Message = messageText1},
                new RegistrationMessage() {Message = messageText2}
            };
            isEligible = true;
            hasOverride = true;
            regEligibility = new RegistrationEligibility(regMessageList, isEligible, hasOverride);
        }

        [TestCleanup]
        public void Cleanup()
        {
            regEligibility = null;
        }

        [TestMethod]
        public void Messages()
        {
            Assert.AreEqual(2, regEligibility.Messages.Count());
            Assert.AreEqual(messageText1, regEligibility.Messages.ElementAt(0).Message);
            Assert.AreEqual(messageText2, regEligibility.Messages.ElementAt(1).Message);
        }

        [TestMethod]
        public void HasOverride()
        {
            Assert.AreEqual(hasOverride, regEligibility.HasOverride);
        }

        [TestMethod]
        public void IsEligible()
        {
            Assert.AreEqual(isEligible, regEligibility.IsEligible);
        }

        [TestMethod]
        public void HasOverrideDefaultsToFalse()
        {
            regEligibility = new RegistrationEligibility(new List<RegistrationMessage>(), isEligible);
            Assert.AreEqual(false, regEligibility.HasOverride);
        }

        [TestMethod]
        public void AllowsEmptyRegistrationMessages()
        {
            regEligibility = new RegistrationEligibility(new List<RegistrationMessage>(), isEligible);
            Assert.AreEqual(0, regEligibility.Messages.Count());
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ThrowsExceptionIfMessagesNull()
        {
            List<RegistrationMessage> messages = null;
            regEligibility = new RegistrationEligibility(messages, true);
        }
    }

    [TestClass]
    public class UpdateRegistrationPrioritiesTests
    {
        private static DateTimeOffset currentTime = DateTimeOffset.Now;
        private static DateTime today = DateTime.Today;
        private DateTime termStart = today.AddDays(-4);
        private DateTime termEnd = today.AddDays(7);
        private List<Term> allTerms;
        private string id1 = "student1";
        private List<RegistrationPriority> priorities;
        private RegistrationEligibility regEligibility;
        private DateTimeOffset futureDate;

        [TestInitialize]
        public void Initialize()
        {
            futureDate = DateTimeOffset.Now.AddDays(2);
            Term t1 = new Term("T1", "Test 1", termStart, termEnd, 2014, 2, true, true, "RT", false);
            Term t2 = new Term("T2", "Test 2", termStart, termEnd, 2014, 3, true, true, "RT", true);
            Term t3 = new Term("T3", "Test 3", termStart, termEnd, 2014, 4, true, true, "RT", true);
            Term t4 = new Term("T4", "Test 4", termStart, termEnd, 2014, 5, true, true, "RT", true);
            Term t5 = new Term("T5", "Test 5", termStart, termEnd, 2014, 6, true, true, "RT", true);
            Term t6 = new Term("T6", "Test 6", termStart, termEnd, 2014, 7, true, true, "RT", true);
            Term t7 = new Term("T7", "Test 7", termStart, termEnd, 2014, 8, true, true, "RT", true);
            Term t8 = new Term("T8", "Test 8", termStart, termEnd, 2014, 9, true, true, "RT", true);
            Term t9 = new Term("T9", "Test 9", termStart, termEnd, 2014, 10, true, true, "RT", true);
            Term t10 = new Term("T10", "Test 10", termStart, termEnd, 2014, 11, true, true, "RT", true);
            Term t11 = new Term("T11", "Test 11", termStart, termEnd, 2014, 12, true, true, "RT", true);
            Term rt = new Term("RT", "Report", termStart, termEnd, 2014, 1, false, false, "RT", false);
            allTerms = new List<Term>() { t1, t2, t3, t4, t5, t6, t7, t8, t9, t10, t11, rt };
            RegistrationEligibilityTerm ret1 = new RegistrationEligibilityTerm("T1", false, false);
            ret1.Status = RegistrationEligibilityTermStatus.Open;
            RegistrationEligibilityTerm ret2 = new RegistrationEligibilityTerm("T2", true, false);
            ret2.Status = RegistrationEligibilityTermStatus.Open;
            RegistrationEligibilityTerm ret3 = new RegistrationEligibilityTerm("T3", true, false);
            ret3.Status = RegistrationEligibilityTermStatus.Open;
            RegistrationEligibilityTerm ret4 = new RegistrationEligibilityTerm("T4", true, true);
            ret4.Status = RegistrationEligibilityTermStatus.Open;
            RegistrationEligibilityTerm ret5 = new RegistrationEligibilityTerm("T5", true, false);
            ret5.Status = RegistrationEligibilityTermStatus.Open;
            RegistrationEligibilityTerm ret6 = new RegistrationEligibilityTerm("T6", true, true);
            ret6.Status = RegistrationEligibilityTermStatus.Open;
            RegistrationEligibilityTerm ret7 = new RegistrationEligibilityTerm("T7", true, true);
            ret7.Status = RegistrationEligibilityTermStatus.Open;
            RegistrationEligibilityTerm ret8 = new RegistrationEligibilityTerm("T8", true, false);
            ret8.Status = RegistrationEligibilityTermStatus.Open;
            RegistrationEligibilityTerm ret9 = new RegistrationEligibilityTerm("T9", true, false);
            ret9.Status = RegistrationEligibilityTermStatus.Open;
            RegistrationEligibilityTerm ret10 = new RegistrationEligibilityTerm("T10", true, false);
            ret10.Status = RegistrationEligibilityTermStatus.Open;
            RegistrationEligibilityTerm ret11 = new RegistrationEligibilityTerm("T11", true, false);
            ret11.Status = RegistrationEligibilityTermStatus.Open;
            RegistrationEligibilityTerm ret12 = new RegistrationEligibilityTerm("T12", true, false);
            ret12.Status = RegistrationEligibilityTermStatus.Future;
            ret12.AnticipatedTimeForAdds = futureDate;
            ret12.Message = "Future Message";
            ret12.FailedRegistrationTermRules = true;
            RegistrationEligibilityTerm ret13 = new RegistrationEligibilityTerm("T13", true, false);
            ret13.Status = RegistrationEligibilityTermStatus.HasOverride;

            // Mock some registration priorities for the Registration Eligibility Terms
            // No Reg Priority for T1
            // No Rep Priority for T2
            // Priority for T3 is same as sterm start and end. Student is eligible to register.
            RegistrationPriority pri_t3 = new RegistrationPriority("1", id1, "T3", termStart, termEnd);
            // No Reg Priority for T4
            DateTimeOffset? temp = DateTimeOffset.Now;
            DateTimeOffset? futureStart = temp.Value.AddDays(2);
            // Future Registration Priority for T5
            RegistrationPriority pri_t5 = new RegistrationPriority("2", id1, "T5", futureStart, termEnd);
            // Priority exists and is open for T6
            RegistrationPriority pri_t6 = new RegistrationPriority("3", id1, "T6", termStart, termEnd);
            // Future priority exists for T7
            RegistrationPriority pri_t7 = new RegistrationPriority("4", id1, "T7", futureStart, termEnd);
            // T8 has multiple REG.PRIORITIES
            DateTimeOffset? pri_t8aStart = temp.Value.AddDays(-4);
            DateTimeOffset? pri_t8aEnd = temp.Value.AddDays(-2);
            RegistrationPriority pri_t8a = new RegistrationPriority("5", id1, "T8", pri_t8aStart, pri_t8aEnd);
            DateTimeOffset? pri_t8bStart = temp.Value.AddDays(-1);
            DateTimeOffset? pri_t8bEnd = temp.Value.AddDays(1);
            RegistrationPriority pri_t8b = new RegistrationPriority("6", id1, "T8", pri_t8bStart, pri_t8bEnd);
            // T9, 2 REG.PRIORITIES, both prior
            DateTimeOffset? pri_t9aStart = temp.Value.AddDays(-4);
            DateTimeOffset? pri_t9aEnd = temp.Value.AddDays(-3);
            RegistrationPriority pri_t9a = new RegistrationPriority("7", id1, "T9", pri_t9aStart, pri_t9aEnd);
            DateTimeOffset? pri_t9bStart = temp.Value.AddDays(-2);
            DateTimeOffset? pri_t9bEnd = temp.Value.AddDays(-1);
            RegistrationPriority pri_t9b = new RegistrationPriority("8", id1, "T9", pri_t9bStart, pri_t9bEnd);
            // T10, 2 REG.PRIORITIES, both future
            DateTimeOffset? pri_t10aStart = temp.Value.AddDays(1);
            DateTimeOffset? pri_t10aEnd = temp.Value.AddDays(2);
            RegistrationPriority pri_t10a = new RegistrationPriority("9", id1, "T10", pri_t10aStart, pri_t10aEnd);
            DateTimeOffset? pri_t10bStart = temp.Value.AddDays(3);
            DateTimeOffset? pri_t10bEnd = temp.Value.AddDays(4);
            RegistrationPriority pri_t10b = new RegistrationPriority("10", id1, "T10", pri_t10bStart, pri_t10bEnd);
            // T11, 2 REG.PRIORITIES, one prior, one future
            DateTimeOffset? pri_t11aStart = temp.Value.AddDays(-2);
            DateTimeOffset? pri_t11aEnd = temp.Value.AddDays(-1);
            RegistrationPriority pri_t11a = new RegistrationPriority("11", id1, "T11", pri_t11aStart, pri_t11aEnd);
            DateTimeOffset? pri_t11bStart = temp.Value.AddDays(1);
            DateTimeOffset? pri_t11bEnd = temp.Value.AddDays(2);
            RegistrationPriority pri_t11b = new RegistrationPriority("12", id1, "T11", pri_t11bStart, pri_t11bEnd);
            // T12, 1 REG.PRIORITY, past
            DateTimeOffset? pri_t12Start = temp.Value.AddDays(-1);
            DateTimeOffset? pri_t12End = temp.Value.AddDays(-2);
            RegistrationPriority pri_t12 = new RegistrationPriority("13", id1, "T12", pri_t12Start, pri_t12End);
            // T13, 1 REG.PRIORITY, future
            DateTimeOffset? pri_t13Start = temp.Value.AddDays(1);
            DateTimeOffset? pri_t13End = temp.Value.AddDays(2);
            RegistrationPriority pri_t13 = new RegistrationPriority("14", id1, "T13", pri_t13Start, pri_t13End);

            priorities = new List<Ellucian.Colleague.Domain.Student.Entities.RegistrationPriority>() { pri_t3, pri_t5, pri_t6, pri_t7, pri_t8b, pri_t8a, pri_t9b, pri_t9a, pri_t10a, pri_t10b, pri_t11b, pri_t11a, pri_t12, pri_t13 };

            var messages = new List<RegistrationMessage>();
            regEligibility = new RegistrationEligibility(messages, true, false);
            regEligibility.AddRegistrationEligibilityTerm(ret1);
            regEligibility.AddRegistrationEligibilityTerm(ret2);
            regEligibility.AddRegistrationEligibilityTerm(ret3);
            regEligibility.AddRegistrationEligibilityTerm(ret4);
            regEligibility.AddRegistrationEligibilityTerm(ret5);
            regEligibility.AddRegistrationEligibilityTerm(ret6);
            regEligibility.AddRegistrationEligibilityTerm(ret7);
            regEligibility.AddRegistrationEligibilityTerm(ret8);
            regEligibility.AddRegistrationEligibilityTerm(ret9);
            regEligibility.AddRegistrationEligibilityTerm(ret10);
            regEligibility.AddRegistrationEligibilityTerm(ret11);
            regEligibility.AddRegistrationEligibilityTerm(ret12);
            regEligibility.AddRegistrationEligibilityTerm(ret13);
        }

        [TestCleanup]
        public void Cleanup()
        {
            regEligibility = null;
        }

        [TestMethod]
        public void RegEliTerms_NoRegPriorityRequirement()
        {
            // Student has no priority for term1 but term 1 does not require it.
            regEligibility.UpdateRegistrationPriorities(priorities, allTerms);
            var regEliTerm = regEligibility.Terms.Where(t => t.TermCode == "T1").FirstOrDefault();
            Assert.AreEqual(RegistrationEligibilityTermStatus.Open, regEliTerm.Status);
            Assert.IsNull(regEliTerm.AnticipatedTimeForAdds);
        }

        [TestMethod]
        public void RegEliTerms_NoRegPriorityNoOverride()
        {
            // Term T2 requires a priorioty but the student does not have one for that term
            regEligibility.UpdateRegistrationPriorities(priorities, allTerms);;
            var regEliTerm = regEligibility.Terms.Where(t => t.TermCode == "T2").First();
            Assert.AreEqual(RegistrationEligibilityTermStatus.NotEligible, regEliTerm.Status);
            Assert.IsNull(regEliTerm.AnticipatedTimeForAdds);
        }

        [TestMethod]
        public void RegEliTerms_CurrentRegPriorityNoOverride()
        {
            // term set to check reg.priorities, no override, valid reg.priority in place for student
            regEligibility.UpdateRegistrationPriorities(priorities, allTerms);;
            var regEliTerm = regEligibility.Terms.Where(t => t.TermCode == "T3").First();
            Assert.AreEqual(RegistrationEligibilityTermStatus.Open, regEliTerm.Status);
            Assert.IsNull(regEliTerm.AnticipatedTimeForAdds);
        }

        [TestMethod]
        public void RegEliTErms_NoRegPriorityCanOverride()
        {
            // term set to check reg.priorities, none present, but override allowed
            regEligibility.UpdateRegistrationPriorities(priorities, allTerms);;
            var regEliTerm = regEligibility.Terms.Where(t => t.TermCode == "T4").First();
            Assert.AreEqual(RegistrationEligibilityTermStatus.Open, regEliTerm.Status);
            Assert.IsNull(regEliTerm.AnticipatedTimeForAdds);
        }

        [TestMethod]
        public void RegEliTErms_FutureRegPriorityNoOverride()
        {
            // term set to check reg.priorities, future priority exists, override allowed
            regEligibility.UpdateRegistrationPriorities(priorities, allTerms);;
            var regEliTerm = regEligibility.Terms.Where(t => t.TermCode == "T5").First();
            Assert.AreEqual(RegistrationEligibilityTermStatus.Future, regEliTerm.Status);
            Assert.IsNotNull(regEliTerm.AnticipatedTimeForAdds.Value);
        }

        [TestMethod]
        public void RegEliTErms_CurentRegPriorityCanOverride()
        {
            // term set to check reg.priorities, current priority in effect, override allowed
            regEligibility.UpdateRegistrationPriorities(priorities, allTerms);;
            var regEliTerm = regEligibility.Terms.Where(t => t.TermCode == "T6").First();
            Assert.AreEqual(RegistrationEligibilityTermStatus.Open, regEliTerm.Status);
            Assert.IsNull(regEliTerm.AnticipatedTimeForAdds);
        }

        [TestMethod]
        public void RegEliTErms_FutureRegPriorityCanOverride()
        {
            // term set to check reg.priorities, override allowed
            regEligibility.UpdateRegistrationPriorities(priorities, allTerms);;
            var regEliTerm = regEligibility.Terms.Where(t => t.TermCode == "T7").First();
            Assert.AreEqual(RegistrationEligibilityTermStatus.Open, regEliTerm.Status);
            Assert.IsNull(regEliTerm.AnticipatedTimeForAdds);
        }

        [TestMethod]
        public void RegEliTErms_MultiplePrioritiesOnePriorOneCurrent()
        {
            // term set to check reg.priorities, no override
            regEligibility.UpdateRegistrationPriorities(priorities, allTerms);;
            var regEliTerm = regEligibility.Terms.Where(t => t.TermCode == "T8").First();
            Assert.AreEqual(RegistrationEligibilityTermStatus.Open, regEliTerm.Status);
            Assert.IsNull(regEliTerm.AnticipatedTimeForAdds);
            Assert.IsFalse(regEliTerm.FailedRegistrationPriorities);
            Assert.IsFalse(regEliTerm.FailedRegistrationTermRules);
        }

        [TestMethod]
        public void RegEliTErms_MultiplePrioritiesTwoPrior()
        {
            // term set to check reg.priorities, no override
            regEligibility.UpdateRegistrationPriorities(priorities, allTerms);;
            var regEliTerm = regEligibility.Terms.Where(t => t.TermCode == "T9").First();
            Assert.AreEqual(RegistrationEligibilityTermStatus.Past, regEliTerm.Status);
            Assert.IsNull(regEliTerm.AnticipatedTimeForAdds);
        }

        [TestMethod]
        public void RegEliTErms_MultiplePrioritiesTwoFuture()
        {
            // term set to check reg.priorities, no override. Anticipated is the earliest of the 2.
            regEligibility.UpdateRegistrationPriorities(priorities, allTerms);;
            var regEliTerm = regEligibility.Terms.Where(t => t.TermCode == "T10").First();
            Assert.AreEqual(RegistrationEligibilityTermStatus.Future, regEliTerm.Status);
            Assert.IsNotNull(regEliTerm.AnticipatedTimeForAdds.Value);
            var prit10a = priorities.Where(p => p.Id == "9").FirstOrDefault();
            Assert.AreEqual(prit10a.Start, regEliTerm.AnticipatedTimeForAdds);
        }

        [TestMethod]
        public void RegEliTErms_MultiplePrioritiesOnePriorOneFuture()
        {
            // term set to check reg.priorities, no override - Anticipated date is the future one.
            regEligibility.UpdateRegistrationPriorities(priorities, allTerms);;
            var regEliTerm = regEligibility.Terms.Where(t => t.TermCode == "T11").First();
            Assert.AreEqual(RegistrationEligibilityTermStatus.Future, regEliTerm.Status);
            Assert.IsNotNull(regEliTerm.AnticipatedTimeForAdds.Value);
            var prit11b = priorities.Where(p => p.Id == "12").FirstOrDefault();
            Assert.AreEqual(prit11b.Start, regEliTerm.AnticipatedTimeForAdds);
        }

        [TestMethod]
        public void NoRegTerms_NoRegPriorities()
        {
            // RegistrationEligibility has no registration terms and there are no priorities. Nothing open for registration.
            var noPriorities = new List<RegistrationPriority>();
            var regEligibilityNew = new RegistrationEligibility(new List<RegistrationMessage>(), true, true);
            regEligibilityNew.UpdateRegistrationPriorities(noPriorities, allTerms);
            Assert.IsTrue(regEligibilityNew.IsEligible);
            Assert.AreEqual(0, regEligibilityNew.Terms.Count());
        }

        [TestMethod]
        public void TermRuleFuture_RegPriorityPast_TermRulesWin()
        {
            // term set to check reg.priorities, and there are term rules where the student meets one that has a future registration dates
            // then even if there are also registration priorities set up for the past we want the status to be Future. 
            regEligibility.UpdateRegistrationPriorities(priorities, allTerms); ;
            var regEliTerm = regEligibility.Terms.Where(t => t.TermCode == "T12").First();
            Assert.AreEqual(RegistrationEligibilityTermStatus.Future, regEliTerm.Status);
            Assert.AreEqual(futureDate, regEliTerm.AnticipatedTimeForAdds);
            Assert.AreEqual("Future Message", regEliTerm.Message);
        }

        [TestMethod]
        public void TermHasOverride_RegPriorityFuture_ShowMessageAndDate()
        {
            // term set to check reg.priorities, override - Anticipated date is the future one.
            regEligibility.UpdateRegistrationPriorities(priorities, allTerms); ;
            var regEliTerm = regEligibility.Terms.Where(t => t.TermCode == "T13").First();
            Assert.AreEqual(RegistrationEligibilityTermStatus.HasOverride, regEliTerm.Status);
            Assert.IsNotNull(regEliTerm.AnticipatedTimeForAdds.Value);
            var prit14 = priorities.Where(p => p.Id == "14").FirstOrDefault();
            Assert.AreEqual(prit14.Start, regEliTerm.AnticipatedTimeForAdds);
            Assert.IsNotNull(regEliTerm.Message);
        }

        [TestMethod]
        public void FailedPriorities()
        {
            // term set to check reg.priorities, no override - Anticipated date is the future one.
            regEligibility.UpdateRegistrationPriorities(priorities, allTerms); ;
            var regEliTerm = regEligibility.Terms.Where(t => t.TermCode == "T11").First();
            Assert.AreEqual(RegistrationEligibilityTermStatus.Future, regEliTerm.Status);
            Assert.IsTrue(regEliTerm.FailedRegistrationPriorities);

        }
    }
}
