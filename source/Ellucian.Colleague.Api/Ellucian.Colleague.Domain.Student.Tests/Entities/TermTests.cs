using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ellucian.Colleague.Domain.Base.Entities;
using Ellucian.Colleague.Domain.Student.Entities;

namespace Ellucian.Colleague.Domain.Student.Tests.Entities
{
    [TestClass]
    public class TermTests
    {
            private DateTime startDate;
            private DateTime endDate;
            private string code;
            private int reportingYear;
            private int sequence;
            private Term term;
            private string description;
            private string reportingTerm;

            [TestInitialize]
            public void Initialize()
            {
                startDate = new DateTime(2011, 8, 15);
                endDate = new DateTime(2011, 12, 15);
                reportingYear = 2012;
                sequence = 1;
                code = "2011/FA";
                description = "2011 Fall";
                reportingTerm = "2011FAR";
                // Asserts are based off this constructor statement, unless another constructor is used in the test method
                term = new Term(code, description, startDate, endDate, reportingYear, sequence, false, true, reportingTerm, true);
            }

            [TestCleanup]
            public void CleanUp()
            {

            }

        [TestClass]
        public class TermConstructor : TermTests
        {
            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void TermCodeExceptionIfNull()
            {
                new Term(null, description, startDate, endDate, reportingYear, sequence, true, true, reportingTerm, true);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void TermCodeExceptionIfEmpty()
            {
                new Term(string.Empty, description, startDate, endDate, reportingYear, sequence, true, true, reportingTerm, true);
            }

            [TestMethod]
            public void TermCode()
            {
                Assert.AreEqual(code, term.Code);
            }

            [TestMethod]
            public void TermDescription()
            {
                Assert.AreEqual(description, term.Description);
            }

            [TestMethod]
            public void TermNullDescription()
            {
                term = new Term(code, null, startDate, endDate, reportingYear, sequence, true, true, reportingTerm, true);
                Assert.AreEqual(code, term.Description);
            }

            [TestMethod]
            public void TermEmptyDescription()
            {
                term = new Term(code, string.Empty, startDate, endDate, reportingYear, sequence, true, true, reportingTerm, true);
                Assert.AreEqual(code, term.Description);
            }

            [TestMethod]
            public void TermStartDate()
            {
                Assert.AreEqual(startDate, term.StartDate);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void TermStartDateNullException()
            {
                term = new Term(code, description, DateTime.MinValue, endDate, reportingYear, sequence, false, true, reportingTerm, true);
            }

            [TestMethod]
            public void TermEndDate()
            {
                Assert.AreEqual(endDate, term.EndDate);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void TermEndDateNullException()
            {
                term = new Term(code, description, startDate, DateTime.MinValue, reportingYear, sequence, false, true, reportingTerm, true);
            }

            [TestMethod]
            public void TermReportingYear()
            {
                Assert.AreEqual(reportingYear, term.ReportingYear);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public void TermReportingYearNegativeException()
            {
                term = new Term(code, description, startDate, endDate, -2, sequence, false, true, reportingTerm, true);
            }

            [TestMethod]
            public void TermSequence()
            {
                Assert.AreEqual(sequence, term.Sequence);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public void TermSequenceNegativeException()
            {
                term = new Term(code, description, startDate, endDate, reportingYear, -1, false, true, reportingTerm, true);
            }

            [TestMethod]
            public void TermDefaultOnPlan()
            {
                Assert.AreEqual(false, term.DefaultOnPlan);
            }

            [TestMethod]
            public void TermPriorityRegistrationRequired()
            {
                Assert.AreEqual(true, term.RegistrationPriorityRequired);
            }

            [TestMethod]
            public void TermForPlanning()
            {
                Assert.AreEqual(true, term.ForPlanning);
            }

            [TestMethod]
            public void TermAcademicLevels()
            {
                Assert.AreEqual(0, term.AcademicLevels.Count);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void TermReportTermNullException()
            {
                term = new Term(code, description, startDate, endDate, reportingYear, sequence, false, true, null, true);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void TermReportTermEmptyException()
            {
                term = new Term(code, description, startDate, endDate, reportingYear, sequence, false, true, String.Empty, true);
            }

            [TestMethod]
            public void TermReportingTerm()
            {
                Assert.AreEqual(reportingTerm, term.ReportingTerm);
            }
        }

        [TestClass]
        public class TermAddAcademicLevel : TermTests
        {
            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void Term_AddAcademicLevel_NullCode()
            {
                var term = new Term(code, description, startDate, endDate, reportingYear, sequence, true, true, reportingTerm, true);
                term.AddAcademicLevel(null);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void Term_AddAcademicLevel_EmptyCode()
            {
                var term = new Term(code, description, startDate, endDate, reportingYear, sequence, true, true, reportingTerm, true);
                term.AddAcademicLevel(string.Empty);
            }

            [TestMethod]
            public void Term_AddAcademicLevel_ValidCode()
            {
                var term = new Term(code, description, startDate, endDate, reportingYear, sequence, true, true, reportingTerm, true);
                term.AddAcademicLevel("UG");
                Assert.AreEqual(1, term.AcademicLevels.Count);
                Assert.AreEqual("UG", term.AcademicLevels[0]);
            }
        }

        [TestClass]
        public class TermEquals
        {
            // Terms are considered equal if their codes are equal only - regardless of the rest of it...

            private DateTime startDate;
            private DateTime endDate;
            private string code;
            private string description;
            private string reportingTerm;
            private Term t1;
            private Term t2;
            private Term t3;

            [TestInitialize]
            public void Initialize()
            {
                startDate = new DateTime(2011, 8, 15);
                endDate = new DateTime(2011, 12, 15);
                code = "2011/FA";
                description = "2011 Fall";
                reportingTerm = "2011FAR";
                t1 = new Term(code, description, startDate, endDate, 2012, 1, true, true, reportingTerm, true);
                t2 = new Term(code, description, endDate, startDate, 2011, 1, true, true, reportingTerm, true);
                t3 = new Term("2012/FA", description, startDate, endDate, 2012, 1, true, true, "2012FAR", true);
            }

            [TestMethod]
            public void TermSameCodesEqual()
            {
                Assert.IsTrue(t1.Equals(t2));
            }

            [TestMethod]
            public void TermDifferentCodeNotEqual()
            {
                Assert.IsFalse(t1.Equals(t3));
            }

        }

        [TestClass]
        public class TermGetHashCode
        {
            // Only has test is the term code...
            private DateTime startDate;
            private DateTime endDate;
            private string code;
            private string description;
            private string reportingTerm;
            private Term t1;
            private Term t2;
            private Term t3;

            [TestInitialize]
            public void Initialize()
            {
                startDate = new DateTime(2011, 8, 15);
                endDate = new DateTime(2011, 12, 15);
                code = "2011/FA";
                description = "2011 Fall";
                reportingTerm = "2011FAR";
                t1 = new Term(code, description, startDate, endDate, 2012, 1, true, true, reportingTerm, true);
                t2 = new Term(code, description, endDate, startDate, 2011, 1, true, true, reportingTerm, true);
                t3 = new Term("2012FA", description, startDate, endDate, 2012, 1, true, true, reportingTerm, true);
            }

            [TestMethod]
            public void TermSameCodeHashEqual()
            {
                Assert.AreEqual(t1.GetHashCode(), t2.GetHashCode());
            }

            [TestMethod]
            public void TermDifferentCodeHashNotEqual()
            {
                Assert.AreNotEqual(t1.GetHashCode(), t3.GetHashCode());
            }

        }
    }
}
