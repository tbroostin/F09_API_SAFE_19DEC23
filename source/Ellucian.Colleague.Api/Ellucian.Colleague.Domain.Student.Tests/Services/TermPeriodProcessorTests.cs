// Copyright 2015 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Ellucian.Colleague.Domain.Base.Entities;
using Ellucian.Colleague.Domain.Student.Entities;
using Ellucian.Colleague.Domain.Student.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Ellucian.Colleague.Domain.Student.Tests.Services
{
    [TestClass]
    public class TermPeriodProcessorTests
    {
        private IEnumerable<Term> terms;

        [TestInitialize]
        public void Initialize()
        {
            terms = new TestTermRepository().Get();
        }

        [TestCleanup]
        public void Cleanup()
        {
            terms = null;
        }

        [TestClass]
        public class TermPeriodProcessor_GetTermSortOrder : TermPeriodProcessorTests
        {
            [TestMethod]
            public void TermPeriodProcessor_GetTermSortOrder_NoMatchingTermFound()
            {
                var sortOrder = TermPeriodProcessor.GetTermSortOrder("ABC123", terms);
                Assert.AreEqual("9999-12-31T23:59:599999999-12-31T23:59:59zzzzzzz", sortOrder);
            }

            [TestMethod]
            public void TermPeriodProcessor_GetTermSortOrder_MatchingTermFound()
            {
                var term = terms.Where(t => t.Code == "2013/SP").FirstOrDefault();
                var sortOrder = TermPeriodProcessor.GetTermSortOrder(term.Code, terms);
                Assert.AreEqual(term.StartDate.ToString("s")+term.Sequence.ToString().PadLeft(3, '0')+
                    term.EndDate.ToString("s")+term.Code, sortOrder);
            }
        }

        [TestClass]
        public class TermPeriodProcessor_AreTermIdsEqual : TermPeriodProcessorTests
        {
            [TestMethod]
            public void TermPeriodProcessor_AreTermIdsEqual_True_NonNullIds()
            {
                var areEqual = TermPeriodProcessor.AreTermIdsEqual("2013/SP", "2013/SP");
                Assert.IsTrue(areEqual);
            }

            [TestMethod]
            public void TermPeriodProcessor_AreTermIdsEqual_True_NullIds()
            {
                var areEqual = TermPeriodProcessor.AreTermIdsEqual(null, null);
                Assert.IsTrue(areEqual);
            }

            [TestMethod]
            public void TermPeriodProcessor_AreTermIdsEqual_False_NonNullIds()
            {
                var areEqual = TermPeriodProcessor.AreTermIdsEqual("2013/SP", "2014/SP");
                Assert.IsFalse(areEqual);
            }

            [TestMethod]
            public void TermPeriodProcessor_AreTermIdsEqual_False_OneNullId()
            {
                var areEqual = TermPeriodProcessor.AreTermIdsEqual(null, "2014/SP");
                Assert.IsFalse(areEqual);
            }
        }

        [TestClass]
        public class TermPeriodProcessor_IsInPeriod : TermPeriodProcessorTests
        {
            [TestMethod]
            public void TermPeriodProcessor_IsInPeriod_NullTermId_True()
            {
                var isInPeriod = TermPeriodProcessor.IsInPeriod(null, DateTime.Today, null, DateTime.Today.AddDays(-3), 
                    DateTime.Today.AddDays(3));
                Assert.IsTrue(isInPeriod);
            }

            [TestMethod]
            public void TermPeriodProcessor_IsInPeriod_EmptyTermId_TooEarly()
            {
                var isInPeriod = TermPeriodProcessor.IsInPeriod(string.Empty, DateTime.Today, null, DateTime.Today.AddDays(3),
                    DateTime.Today.AddDays(6));
                Assert.IsFalse(isInPeriod);
            }

            [TestMethod]
            public void TermPeriodProcessor_IsInPeriod_EmptyTermId_TooLate()
            {
                var isInPeriod = TermPeriodProcessor.IsInPeriod(string.Empty, DateTime.Today, null, DateTime.Today.AddDays(-6),
                    DateTime.Today.AddDays(-3));
                Assert.IsFalse(isInPeriod);
            }

            [TestMethod]
            public void TermPeriodProcessor_IsInPeriod_ValidTermId_NullTermIds()
            {
                var isInPeriod = TermPeriodProcessor.IsInPeriod("2013/SP", DateTime.Today, null, null, null);
                Assert.IsFalse(isInPeriod);
            }

            [TestMethod]
            public void TermPeriodProcessor_IsInPeriod_ValidTermId_NoTermIds()
            {
                var isInPeriod = TermPeriodProcessor.IsInPeriod("2013/SP", DateTime.Today, new List<string>(), null, null);
                Assert.IsFalse(isInPeriod);
            }

            [TestMethod]
            public void TermPeriodProcessor_IsInPeriod_ValidTermId_TermIdNotInList()
            {
                var isInPeriod = TermPeriodProcessor.IsInPeriod("2013/SP", DateTime.Today, new List<string>() { "2014/SP", "2015/FA" },
                    null, null);
                Assert.IsFalse(isInPeriod);
            }

            [TestMethod]
            public void TermPeriodProcessor_IsInPeriod_ValidTermId_TermIdInList()
            {
                var isInPeriod = TermPeriodProcessor.IsInPeriod("2013/SP", DateTime.Today, new List<string>() { "2013/SP", "2015/FA" },
                    null, null);
                Assert.IsTrue(isInPeriod);
            }
        }

        [TestClass]
        public class TermPeriodProcessor_IsReportingTerm : TermPeriodProcessorTests
        {
            [TestMethod]
            public void TermPeriodProcessor_IsReportingTerm_NullTermId()
            {
                var isReportingTerm = TermPeriodProcessor.IsReportingTerm(null, terms);
                Assert.IsFalse(isReportingTerm);
            }

            [TestMethod]
            public void TermPeriodProcessor_IsReportingTerm_EmptyTermId()
            {
                var isReportingTerm = TermPeriodProcessor.IsReportingTerm(string.Empty, terms);
                Assert.IsFalse(isReportingTerm);
            }

            [TestMethod]
            public void TermPeriodProcessor_IsReportingTerm_NullTerms()
            {
                var isReportingTerm = TermPeriodProcessor.IsReportingTerm("2013/SP", null);
                Assert.IsFalse(isReportingTerm);
            }

            [TestMethod]
            public void TermPeriodProcessor_IsReportingTerm_NoTerms()
            {
                var isReportingTerm = TermPeriodProcessor.IsReportingTerm("2013/SP", new List<Term>());
                Assert.IsFalse(isReportingTerm);
            }

            [TestMethod]
            public void TermPeriodProcessor_IsReportingTerm_NonReportingTerm()
            {
                var isReportingTerm = TermPeriodProcessor.IsReportingTerm("2013/SP", terms);
                Assert.IsFalse(isReportingTerm);
            }

            [TestMethod]
            public void TermPeriodProcessor_IsReportingTerm_ReportingTerm()
            {
                var isReportingTerm = TermPeriodProcessor.IsReportingTerm("2013RSP", terms);
                Assert.IsTrue(isReportingTerm);
            }
        }

        [TestClass]
        public class TermPeriodProcessor_IsInReportingTerm : TermPeriodProcessorTests
        {
            [TestMethod]
            public void TermPeriodProcessor_IsInReportingTerm_NullTermId()
            {
                var isInReportingTerm = TermPeriodProcessor.IsInReportingTerm(null, "2013RSP", terms);
                Assert.IsFalse(isInReportingTerm);
            }

            [TestMethod]
            public void TermPeriodProcessor_IsInReportingTerm_EmptyTermId()
            {
                var isInReportingTerm = TermPeriodProcessor.IsInReportingTerm(string.Empty, "2013RSP", terms);
                Assert.IsFalse(isInReportingTerm);
            }

            [TestMethod]
            public void TermPeriodProcessor_IsInReportingTerm_NullReportingTermId()
            {
                var isInReportingTerm = TermPeriodProcessor.IsInReportingTerm("2013/SP", null, terms);
                Assert.IsFalse(isInReportingTerm);
            }

            [TestMethod]
            public void TermPeriodProcessor_IsInReportingTerm_EmptyReportingTermId()
            {
                var isInReportingTerm = TermPeriodProcessor.IsInReportingTerm("2013/SP", string.Empty, terms);
                Assert.IsFalse(isInReportingTerm);
            }

            [TestMethod]
            public void TermPeriodProcessor_IsInReportingTerm_NullTerms()
            {
                var isInReportingTerm = TermPeriodProcessor.IsInReportingTerm("2013/SP", "2013RSP", null);
                Assert.IsFalse(isInReportingTerm);
            }

            [TestMethod]
            public void TermPeriodProcessor_IsInReportingTerm_NoTerms()
            {
                var isInReportingTerm = TermPeriodProcessor.IsInReportingTerm("2013/SP", "2013RSP", new List<Term>());
                Assert.IsFalse(isInReportingTerm);
            }

            [TestMethod]
            public void TermPeriodProcessor_IsInReportingTerm_True()
            {
                var isInReportingTerm = TermPeriodProcessor.IsInReportingTerm("2013/SP", "2013RSP", terms);
                Assert.IsTrue(isInReportingTerm);
            }

            [TestMethod]
            public void TermPeriodProcessor_IsInReportingTerm_False()
            {
                var isInReportingTerm = TermPeriodProcessor.IsInReportingTerm("2013/S2", "2013RSP", terms);
                Assert.IsFalse(isInReportingTerm);
            }
        }

        [TestClass]
        public class TermPeriodProcessor_CompareTerms : TermPeriodProcessorTests
        {
            [TestMethod]
            public void TermPeriodProcessor_CompareTerms_ValidTermIds_Equal()
            {
                var equal = TermPeriodProcessor.CompareTerms("2013/SP", "2013/SP");
                Assert.IsTrue(equal);
            }

            [TestMethod]
            public void TermPeriodProcessor_CompareTerms_ValidTermIds_NotEqual()
            {
                var equal = TermPeriodProcessor.CompareTerms("2013/SP", "2014/SP");
                Assert.IsFalse(equal);
            }

            [TestMethod]
            public void TermPeriodProcessor_CompareTerms_NullTermIds_Equal()
            {
                var equal = TermPeriodProcessor.CompareTerms(null, null);
                Assert.IsTrue(equal);
            }

            [TestMethod]
            public void TermPeriodProcessor_CompareTerms_NullAndValidTermIds_NotEqual()
            {
                var equal = TermPeriodProcessor.CompareTerms(null, "2014/SP");
                Assert.IsFalse(equal);
            }
        }

        [TestClass]
        public class TermPeriodProcessor_GetReportingTerm : TermPeriodProcessorTests
        {
            [TestMethod]
            public void TermPeriodProcessor_GetReportingTerm_NoMatchingTerm()
            {
                var reportingTerm = TermPeriodProcessor.GetReportingTerm("ABC123", terms);
                Assert.AreEqual(string.Empty, reportingTerm);
            }

            [TestMethod]
            public void TermPeriodProcessor_GetReportingTerm_MatchingTerm()
            {
                var reportingTerm = TermPeriodProcessor.GetReportingTerm("2013/SP", terms);
                Assert.AreEqual("2013RSP", reportingTerm);
            }
        }

        [TestClass]
        public class TermPeriodProcessor_GetTermPeriod : TermPeriodProcessorTests
        {
            [TestMethod]
            public void TermPeriodProcessor_GetTermPeriod_NoMatchingTerm()
            {
                var termPeriod = TermPeriodProcessor.GetTermPeriod("ABC123", terms);
                Assert.IsNull(termPeriod);
            }

            [TestMethod]
            public void TermPeriodProcessor_GetTermPeriod_MatchingTermWithFinancialPeriod()
            {
                var termPeriod = TermPeriodProcessor.GetTermPeriod("2013/SP", terms);
                Assert.AreEqual(PeriodType.Past, termPeriod);
            }

            [TestMethod]
            public void TermPeriodProcessor_GetTermPeriod_NoMatchingTermNoFinancialPeriod()
            {
                var termPeriod = TermPeriodProcessor.GetTermPeriod("2028/SP", terms);
                Assert.AreEqual(null, termPeriod);
            }
        }

        [TestClass]
        public class TermPeriodProcessor_DateRangeDefaults : TermPeriodProcessorTests
        {
            [TestMethod]
            public void TermPeriodProcessor_DateRangeDefaults_NullRangeStart()
            {
                DateTime? startDate = null;
                DateTime? endDate = DateTime.Today; 
                TermPeriodProcessor.DateRangeDefaults(ref startDate, ref endDate);
                Assert.AreEqual(DateTime.MinValue, startDate);
            }

            [TestMethod]
            public void TermPeriodProcessor_DateRangeDefaults_NullRangeEnd()
            {
                DateTime? startDate = DateTime.Today;
                DateTime? endDate = null;
                TermPeriodProcessor.DateRangeDefaults(ref startDate, ref endDate);
                Assert.AreEqual(DateTime.MaxValue, endDate);
            }

            [TestMethod]
            public void TermPeriodProcessor_DateRangeDefaults_RangeStartLaterThanRangeEnd()
            {
                DateTime? startDate = DateTime.Today;
                DateTime? endDate = DateTime.Today.AddDays(-3);
                TermPeriodProcessor.DateRangeDefaults(ref startDate, ref endDate);
                Assert.AreEqual(DateTime.Today.AddDays(-3), startDate);
                Assert.AreEqual(DateTime.Today, endDate);
            }
        }

        [TestClass]
        public class TermPeriodProcessor_IsRangeOverlap : TermPeriodProcessorTests
        {
            [TestMethod]
            public void TermPeriodProcessor_IsRangeOverlap_StartDate1InRange2()
            {
                var isRangeOverlap = TermPeriodProcessor.IsRangeOverlap(DateTime.Today, DateTime.Today.AddDays(6),
                    DateTime.Today.AddDays(-3), DateTime.Today.AddDays(3));
                Assert.IsTrue(isRangeOverlap);
            }

            [TestMethod]
            public void TermPeriodProcessor_IsRangeOverlap_EndDate1InRange2()
            {
                var isRangeOverlap = TermPeriodProcessor.IsRangeOverlap(DateTime.Today.AddDays(-6), DateTime.Today.AddDays(1),
                    DateTime.Today.AddDays(-3), DateTime.Today.AddDays(3));
                Assert.IsTrue(isRangeOverlap);
            }

            [TestMethod]
            public void TermPeriodProcessor_IsRangeOverlap_StartDate2InRange1()
            {
                var isRangeOverlap = TermPeriodProcessor.IsRangeOverlap(DateTime.Today.AddDays(-3), DateTime.Today.AddDays(3),
                    DateTime.Today, DateTime.Today.AddDays(6)); Assert.IsTrue(isRangeOverlap);
                Assert.IsTrue(isRangeOverlap);
            }

            [TestMethod]
            public void TermPeriodProcessor_IsRangeOverlap_EndDate2InRange1()
            {
                var isRangeOverlap = TermPeriodProcessor.IsRangeOverlap(DateTime.Today.AddDays(-3), DateTime.Today.AddDays(3),
                    DateTime.Today.AddDays(-6), DateTime.Today.AddDays(1));
                Assert.IsTrue(isRangeOverlap);
            }

            [TestMethod]
            public void TermPeriodProcessor_IsRangeOverlap_Range1OutsideRange2()
            {
                var isRangeOverlap = TermPeriodProcessor.IsRangeOverlap(DateTime.Today.AddDays(-6), DateTime.Today.AddDays(-3),
                    DateTime.Today.AddDays(3), DateTime.Today.AddDays(6));
                Assert.IsFalse(isRangeOverlap);
            }

            [TestMethod]
            public void TermPeriodProcessor_IsRangeOverlap_Range2OutsideRange1()
            {
                var isRangeOverlap = TermPeriodProcessor.IsRangeOverlap(DateTime.Today.AddDays(3), DateTime.Today.AddDays(6),
                    DateTime.Today.AddDays(-6), DateTime.Today.AddDays(-3));
                Assert.IsFalse(isRangeOverlap);
            }
        }

        [TestClass]
        public class TermPeriodProcessor_IsDateInRange : TermPeriodProcessorTests
        {
            [TestMethod]
            public void TermPeriodProcessor_IsDateInRange_True()
            {
                var isDateInRange = TermPeriodProcessor.IsDateInRange(DateTime.Today, DateTime.Today.AddDays(-3),
                    DateTime.Today.AddDays(3));
                Assert.IsTrue(isDateInRange);
            }

            [TestMethod]
            public void TermPeriodProcessor_IsDateInRange_False()
            {
                var isDateInRange = TermPeriodProcessor.IsDateInRange(DateTime.Today.AddDays(-3), DateTime.Today, 
                    DateTime.Today.AddDays(3));
                Assert.IsFalse(isDateInRange);
            }
        }

        [TestClass]
        public class TermPeriodProcessor_GetTermIdForTermDescription : TermPeriodProcessorTests
        {
            [TestMethod]
            public void TermPeriodProcessor_GetTermIdForTermDescription_NullTerms()
            {
                var desc = TermPeriodProcessor.GetTermIdForTermDescription("id", null);
                Assert.AreEqual("id", desc);
            }

            [TestMethod]
            public void TermPeriodProcessor_GetTermIdForTermDescription_NoTerms()
            {
                var desc = TermPeriodProcessor.GetTermIdForTermDescription("id", new List<Term>());
                Assert.AreEqual("id", desc);
            }

            [TestMethod]
            public void TermPeriodProcessor_GetTermIdForTermDescription_MatchingTerm()
            {
                var desc = TermPeriodProcessor.GetTermIdForTermDescription("2014 Fall Term", terms);
                Assert.AreEqual("2014/FA", desc);
            }

            [TestMethod]
            public void TermPeriodProcessor_GetTermIdForTermDescription_NoMatchingDescriptionButMatchingId()
            {
                var desc = TermPeriodProcessor.GetTermIdForTermDescription("2014/FA", null);
                Assert.AreEqual("2014/FA", desc);
            }

        }
    }
}
