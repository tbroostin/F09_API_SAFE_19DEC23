// Copyright 2015-2018 Ellucian Company L.P. and its affiliates.

using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ellucian.Colleague.Domain.Student.Entities;

namespace Ellucian.Colleague.Domain.Student.Tests.Entities
{
    [TestClass]
    public class AcademicPeriodTests
    {
        private string guid;
        private DateTime startDate;
        private DateTime endDate;
        private string code;
        private int reportingYear;
        private int sequence;
        private AcademicPeriod academicPeriod;
        private string description;
        private string reportingTerm;
        private string parentId;
        private string preceedingId;

        [TestInitialize]
        public void Initialize()
        {
            guid = "060d0dd1-75aa-4c43-989c-0015b6f01d6e";
            startDate = new DateTime(2011, 8, 15);
            endDate = new DateTime(2011, 12, 15);
            reportingYear = 2012;
            sequence = 1;
            code = "2011/FA";
            description = "2011 Fall";
            reportingTerm = "2011FAR";
            parentId = "dsada1-75aa-4c43-989c-0015b6f01d6e";
            preceedingId = "ffsd-75aa-4c43-989c-0015b6f0dsfds";
            academicPeriod = new AcademicPeriod(guid, code, description, startDate, endDate, reportingYear, sequence, reportingTerm, preceedingId, parentId, null);
        }

        [TestCleanup]
        public void CleanUp()
        {

        }

        [TestClass]
        public class AcademicPeriodConstructor : AcademicPeriodTests
        {
            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void AcademicPeriodCodeExceptionIfNull()
            {
                academicPeriod = new AcademicPeriod(guid, null, description, startDate, endDate, reportingYear, sequence, reportingTerm, preceedingId, parentId, null);

            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void TermCodeExceptionIfEmpty()
            {
                academicPeriod = new AcademicPeriod(guid, string.Empty, description, startDate, endDate, reportingYear, sequence, reportingTerm, preceedingId, parentId, null);

            }

            [TestMethod]
            public void AcademicPeriodCode()
            {
                Assert.AreEqual(code, academicPeriod.Code);
            }

            [TestMethod]
            public void AcademicPeriodDescription()
            {
                Assert.AreEqual(description, academicPeriod.Description);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void AcademicPeriodNullDescription()
            {
                academicPeriod = new AcademicPeriod(guid, code, null, startDate, endDate, reportingYear, sequence, reportingTerm, preceedingId, parentId, null);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void AcademicPeriodEmptyDescription()
            {
                academicPeriod = new AcademicPeriod(guid, code, string.Empty, startDate, endDate, reportingYear, sequence, reportingTerm, preceedingId, parentId, null);
            }

            [TestMethod]
            public void AcademicPeriodStartDate()
            {
                Assert.AreEqual(startDate, academicPeriod.StartDate);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void AcademicPeriodStartDateNullException()
            {
                academicPeriod = new AcademicPeriod(guid, code, description, DateTime.MinValue, endDate, reportingYear, sequence, reportingTerm, preceedingId, parentId, null);

            }

            [TestMethod]
            public void AcademicPeriodEndDate()
            {
                Assert.AreEqual(endDate, academicPeriod.EndDate);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void AcademicPeriodEndDateNullException()
            {
                academicPeriod = new AcademicPeriod(guid, code, description, startDate, DateTime.MinValue, reportingYear, sequence, reportingTerm, preceedingId, parentId, null);

            }

            [TestMethod]
            public void AcademicPeriodReportingYear()
            {
                Assert.AreEqual(reportingYear, academicPeriod.ReportingYear);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public void AcademicPeriodReportingYearNegativeException()
            {
                academicPeriod = new AcademicPeriod(guid, code, description, startDate, endDate, -2, sequence, reportingTerm, preceedingId, parentId, null);

            }

            [TestMethod]
            public void AcademicPeriodSequence()
            {
                Assert.AreEqual(sequence, academicPeriod.Sequence);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public void AcademicPeriodSequenceNegativeException()
            {
                academicPeriod = new AcademicPeriod(guid, code, description, startDate, endDate, reportingYear, -1, reportingTerm, preceedingId, parentId, null);

            }


            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void AcademicPeriodReportTermNullException()
            {
                academicPeriod = new AcademicPeriod(guid, code, description, startDate, endDate, reportingYear, sequence, null, preceedingId, parentId, null);

            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void AcademicPeriodReportTermEmptyException()
            {
                academicPeriod = new AcademicPeriod(guid, code, description, startDate, endDate, reportingYear, sequence, string.Empty, preceedingId, parentId, null);

            }

            [TestMethod]
            public void AcademicPeriodReportingTerm()
            {
                Assert.AreEqual(reportingTerm, academicPeriod.ReportingTerm);
            }

            [TestMethod]
            public void AcademicPeriodPreceedingId()
            {
                Assert.AreEqual(preceedingId, academicPeriod.PrecedingId);
            }

            [TestMethod]
            public void AcademicPeriodParentId()
            {
                Assert.AreEqual(parentId, academicPeriod.ParentId);
            }
        }
    }
}