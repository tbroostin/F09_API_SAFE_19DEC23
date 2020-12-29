// Copyright 2019 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using Ellucian.Colleague.Coordination.Planning.Reports;
using Ellucian.Colleague.Domain.Planning.Entities;
using Ellucian.Colleague.Domain.Planning.Tests;
using Ellucian.Colleague.Domain.Student.Entities;
using Ellucian.Colleague.Domain.Student.Entities.Requirements;
using Ellucian.Colleague.Domain.Student.Tests;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Ellucian.Colleague.Coordination.Planning.Tests.Reports
{
    [TestClass]
    public class DegreePlanArchiveReportTests
    {
        DegreePlanArchive degreePlanArchive;
        Domain.Student.Entities.Student student;
        Domain.Student.Entities.Student student2;
        IEnumerable<Program> programs;
        IEnumerable<Advisor> advisors;
        IEnumerable<Term> terms;
        DegreePlanArchiveReport degreePlanArchiveReport;
        IEnumerable<StudentProgram> studentPrograms;

        [TestInitialize]
        public async void Initialize()
        {
            degreePlanArchive = await new TestDegreePlanArchiveRepository().GetDegreePlanArchiveAsync(2);

            student = new Domain.Student.Entities.Student("0000698", "LastName", 2, new List<string>(), new List<string>());
            student.PersonDisplayName = new Domain.Base.Entities.PersonHierarchyName("HCODE") { FullName = "Chosen Fullname", FirstName = "Chosenfirst", LastName = "Chosenlast" };
            student.FirstName = "FirstName";
            student.PreferredName = "PreferredName";

            programs = await new TestProgramRepository().GetAsync();

            studentPrograms = (await new TestStudentProgramRepository().GetAsync(student.Id)).ToList();
            studentPrograms.FirstOrDefault().ProgramName = "Student Custom Program Title";

            //TODO: needs to be async
            terms = new TestTermRepository().Get();

            advisors = new List<Advisor>() { new Advisor("0000111", "Johnson") { FirstName = "Edward", Name = "Edward Johnson" }, new Advisor("1212121", "Smithson") };
            degreePlanArchiveReport = new DegreePlanArchiveReport(degreePlanArchive, student, programs, advisors, terms, studentPrograms);
        }

        [TestCleanup]
        public void Cleanup()
        {
            student = null;
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void NoDegreePlanArchive_ThowsException()
        {
            var degreePlanArchiveReport = new DegreePlanArchiveReport(null, student, programs, advisors, terms, studentPrograms);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void NoStudent_ThowsException()
        {
            var degreePlanArchiveReport = new DegreePlanArchiveReport(degreePlanArchive, null, programs, advisors, terms, studentPrograms);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void NoPrograms_ThowsException()
        {
            var degreePlanArchiveReport = new DegreePlanArchiveReport(degreePlanArchive, student, null, advisors, terms, studentPrograms);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void NoAdvisors_ThowsException()
        {
            var degreePlanArchiveReport = new DegreePlanArchiveReport(degreePlanArchive, student, programs, null, terms, studentPrograms);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void NoTerms_ThowsException()
        {
            var degreePlanArchiveReport = new DegreePlanArchiveReport(degreePlanArchive, student, programs, advisors, null, studentPrograms);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ZeroTerms_ThowsException()
        {
            var degreePlanArchiveReport = new DegreePlanArchiveReport(degreePlanArchive, student, programs, advisors, new List<Term>(), studentPrograms);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void NoStudentPrograms_ThowsException()
        {
            var degreePlanArchiveReport = new DegreePlanArchiveReport(degreePlanArchive, student, programs, advisors, terms, null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ZeroStudentPrograms_ThowsException()
        {
            var degreePlanArchiveReport = new DegreePlanArchiveReport(degreePlanArchive, student, programs, advisors, terms, new List<StudentProgram>());
        }

        [TestMethod]
        public void DegreePlanArchiveReport_ReportTitle()
        {
            var date = new DateTime(2013, 5, 1);
            Assert.AreEqual("Course Plan as of " + date.ToShortDateString(), degreePlanArchiveReport.ReportTitle);
        }

        [TestMethod]
        public void DegreePlanArchiveReport_StudentId()
        {
            Assert.AreEqual(student.Id, degreePlanArchiveReport.StudentId);
        }

        [TestMethod]
        public void DegreePlanArchiveReport_StudentName_FromPersonDisplayName()
        {
            Assert.AreEqual(student.PersonDisplayName.FullName, degreePlanArchiveReport.StudentName);
        }

        [TestMethod]
        public void DegreePlanArchiveReport_StudentLastName_FromPersonDisplayName()
        {
            Assert.AreEqual(student.PersonDisplayName.LastName, degreePlanArchiveReport.StudentLastName);
        }

        [TestMethod]
        public void DegreePlanArchiveReport_StudentFirstName_FromPersonDisplayname()
        {
            Assert.AreEqual(student.PersonDisplayName.FirstName, degreePlanArchiveReport.StudentFirstName);
        }

        [TestMethod]
        public void DegreePlanArchiveReport_StudentName_NoDisplayName()
        {
            student2 = new Domain.Student.Entities.Student("0000698", "LastName", 2, new List<string>(), new List<string>());
            student2.FirstName = "FirstName";
            student2.PreferredName = "PreferredName";
            degreePlanArchiveReport = new DegreePlanArchiveReport(degreePlanArchive, student2, programs, advisors, terms, studentPrograms);
            Assert.AreEqual(student2.PreferredName, degreePlanArchiveReport.StudentName);
        }

        [TestMethod]
        public void DegreePlanArchiveReport_StudentLastName_NoDisplayName()
        {
            student2 = new Domain.Student.Entities.Student("0000698", "LastName", 2, new List<string>(), new List<string>());
            student2.FirstName = "FirstName";
            student2.PreferredName = "PreferredName";
            degreePlanArchiveReport = new DegreePlanArchiveReport(degreePlanArchive, student2, programs, advisors, terms, studentPrograms);
            Assert.AreEqual(student2.LastName, degreePlanArchiveReport.StudentLastName);
        }

        [TestMethod]
        public void DegreePlanArchiveReport_StudentFirstName()
        {
            student2 = new Domain.Student.Entities.Student("0000698", "LastName", 2, new List<string>(), new List<string>());
            student2.FirstName = "FirstName";
            student2.PreferredName = "PreferredName";
            degreePlanArchiveReport = new DegreePlanArchiveReport(degreePlanArchive, student2, programs, advisors, terms, studentPrograms);
            Assert.AreEqual(student2.FirstName, degreePlanArchiveReport.StudentFirstName);
        }

        [TestMethod]
        public void DegreePlanArchiveReport_ArchivedBy()
        {
            Assert.AreEqual("Edward Johnson", degreePlanArchiveReport.ArchivedBy);
        }

        [TestMethod]
        public void DegreePlanArchiveReport_ArchivedOn()
        {
            Assert.AreEqual(degreePlanArchive.CreatedDate, degreePlanArchiveReport.ArchivedOn);
        }

        [TestMethod]
        public void DegreePlanArchiveReport_StudentPrograms()
        {
            Assert.AreEqual(degreePlanArchive.StudentPrograms.Count(), degreePlanArchiveReport.StudentPrograms.Count());
        }

        [TestMethod]
        public void DegreePlanArchiveReport_ArchivedCourses()
        {
            Assert.AreEqual(degreePlanArchive.ArchivedCourses.Count(), degreePlanArchiveReport.ArchivedCourses.Count());
        }

        [TestMethod]
        public void DegreePlanArchiveReport_ArchivedCourse_Properties()
        {
            var reportArchivedCourse = degreePlanArchiveReport.ArchivedCourses.Where(a => a.TermCode == "2008/FA").FirstOrDefault();
            Assert.AreEqual("Johnson, E.", reportArchivedCourse.ApprovedBy);
            Assert.AreEqual(1m, reportArchivedCourse.Credits);
            Assert.AreEqual("Approved", reportArchivedCourse.ApprovalStatus);
            Assert.AreEqual("2008/FA", reportArchivedCourse.TermCode);
        }

        [TestMethod]
        public void DegreePlanArchiveReport_ArchivedCourse_FormattedValues()
        {
            var reportArchivedCourse = degreePlanArchiveReport.ArchivedCourses.Where(a => a.TermCode == "2008/FA").FirstOrDefault();
            Assert.AreEqual("1", reportArchivedCourse.FormattedCredits);
            Assert.AreEqual("", reportArchivedCourse.FormattedCeus);
            var reportArchivedCeuCourse = degreePlanArchiveReport.ArchivedCourses.Where(a => a.TermCode == "2007/SP").FirstOrDefault();
            Assert.AreEqual("", reportArchivedCeuCourse.FormattedCredits);
            Assert.AreEqual("2", reportArchivedCeuCourse.FormattedCeus);
        }

        [TestMethod]
        public void DegreePlanArchiveReport_ArchivedNotes()
        {
            Assert.AreEqual(degreePlanArchive.Notes.Count(), degreePlanArchiveReport.ArchivedNotes.Count());
        }

        [TestMethod]
        public void DegreePlanArchiveReport_StudentPrograms_StudentProgramCustomName()
        {
            var studentProgram = degreePlanArchiveReport.StudentPrograms.FirstOrDefault();
            Assert.AreEqual(studentProgram.Key, studentPrograms.FirstOrDefault().ProgramName);
        }
    }
}
