// Copyright 2017-2019 Ellucian Company L.P. and its affiliates.
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ellucian.Colleague.Domain.Planning.Entities;
using Ellucian.Colleague.Domain.Student.Entities;
using Ellucian.Colleague.Domain.Student.Entities.DegreePlans;
using Ellucian.Colleague.Domain.Student.Tests;

namespace Ellucian.Colleague.Domain.Planning.Tests.Entities
{
    [TestClass]
    public class DegreePlanArchiveTests
    {
        [TestClass]
        public class DegreePlanArchiveConstructor
        {
            string studentId = "currentUserId";
            DateTime Now = DateTime.Now;
            DateTime Created = new DateTime(2013, 1, 1, 9, 0, 0);
            int degreePlanId = 1235;
            int archiveId = 4444;
            DegreePlanArchive archive;
            string reviewedBy = "Advisor1";
            string createdBy = "Advisor2";
            int version = 5;

            [TestInitialize]
            public void Initialize()
            {
                // Tests use the following constructor unless otherwise handled. 
                archive = new DegreePlanArchive(archiveId, degreePlanId, studentId, version);
                archive.ReviewedBy = reviewedBy;
                archive.ReviewedDate = Now;
                archive.CreatedBy = createdBy;
                archive.CreatedDate = Created;
            }

            [TestCleanup]
            public void Cleanup()
            {
                archive = null;
            }

            [TestMethod]
            public void ConstructorSuccess()
            {
                Assert.AreEqual(archiveId, archive.Id);
                Assert.AreEqual(degreePlanId, archive.DegreePlanId);
                Assert.AreEqual(studentId, archive.StudentId);
                Assert.AreEqual(reviewedBy, archive.ReviewedBy);
                Assert.AreEqual(Now, archive.ReviewedDate);
                Assert.AreEqual(createdBy, archive.CreatedBy);
                Assert.AreEqual(Created, archive.CreatedDate);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public void ThrowsExceptionIfDegreePlanArchiveIdIsLessThanZero()
            {
                var archive9 = new DegreePlanArchive(-1, degreePlanId, studentId, version);
            }

            [TestMethod]
            public void NoExceptionIfDegreePlanArchiveIdIsZero()
            {
                var archive = new DegreePlanArchive(0, degreePlanId, studentId, version);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void ThrowsExceptionIfDegreePlanIdIsZero()
            {
                var archive = new DegreePlanArchive(archiveId, 0, null, version);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void ThrowsExceptionIfStudentIdIsNull()
            {
                var archive = new DegreePlanArchive(archiveId, degreePlanId, null, version);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void ThrowsExceptionIfStudentIdIsEmpty()
            {
                var archive = new DegreePlanArchive(archiveId, degreePlanId, "", version);
            }

            [TestMethod]
            public void ConstructorSuccess_EmptyFields()
            {
                var archive2 = new DegreePlanArchive(archiveId, degreePlanId, studentId, version);
                Assert.AreEqual(archiveId, archive2.Id);
                Assert.AreEqual(degreePlanId, archive2.DegreePlanId);
                Assert.AreEqual(studentId, archive2.StudentId);
                Assert.IsNull(archive2.ReviewedBy);
                Assert.IsNull(archive2.ReviewedDate);
                Assert.IsNull(archive2.CreatedBy);
                Assert.IsNull(archive2.CreatedDate);
            }

            [TestMethod]
            public void DegreePlanArchive_ArchiveCourses()
            {
                var archive3 = new DegreePlanArchive(archiveId, degreePlanId, studentId, version);
                var archiveCourses = new List<ArchivedCourse>();
                var ac1 = new ArchivedCourse("Course1");
                ac1.ApprovalDate = DateTime.Now;
                ac1.ApprovalStatus = "Approved";
                ac1.ApprovedBy = "ApprovedBy";
                ac1.Credits = 3.0m;
                ac1.Name = "CourseName";
                ac1.Title = "Title";
                ac1.TermCode = "Term1";
                ac1.SectionId = "Section1";
                archiveCourses.Add(ac1);
                var ac2 = new ArchivedCourse("Course2");
                archiveCourses.Add(ac2);
                archive3.ArchivedCourses = archiveCourses;
                Assert.AreEqual(2, archive3.ArchivedCourses.Count());
            }
        }

        [TestClass]
        public class CreateDegreePlanArchiveTests
        {
            private DegreePlanArchive archive;
            private IEnumerable<StudentProgram> studentPrograms;
            private IEnumerable<Course> courses;
            private IEnumerable<Section> sections;
            private IEnumerable<Term> registrationTerms;
            private DegreePlan degreePlan;
            private IEnumerable<Grade> grades;

            [TestInitialize]
            public async void Initialize()
            {
                // Tests use the following constructor unless otherwise handled.
                studentPrograms = new TestStudentProgramRepository().Get("0000896");
                courses =await new TestCourseRepository().GetAsync();
                registrationTerms = await new TestTermRepository().GetRegistrationTermsAsync();
                sections = await new TestSectionRepository().GetRegistrationSectionsAsync(registrationTerms);
                grades = await new TestGradeRepository().GetAsync();
                degreePlan = await (new TestStudentDegreePlanRepository()).GetAsync(3);
                var note1 = new DegreePlanNote("Note One");
                var note2 = new DegreePlanNote("Note Two");
                var notes = new List<DegreePlanNote>() { note1, note2 };
                var academicCredits = new List<AcademicCredit>();
                degreePlan.Notes = notes;
                archive = Ellucian.Colleague.Domain.Planning.Entities.DegreePlanArchive.CreateDegreePlanArchive(degreePlan, "1111111", studentPrograms, courses, sections, academicCredits, grades);
            }

            [TestCleanup]
            public void Cleanup()
            {
                archive = null;
            }

            [TestMethod]
            public void CreatedDegreePlanArchive_Id()
            {
                Assert.AreEqual(0, archive.Id);
            }

            [TestMethod]
            public void CreatedDegreePlanArchive_DegreePlanId()
            {
                Assert.AreEqual(degreePlan.Id, archive.DegreePlanId);
            }

            [TestMethod]
            public void CreatedDegreePlanArchive_CreatedBy()
            {
                Assert.AreEqual("1111111", archive.CreatedBy);
            }

            [TestMethod]
            public void CreatedDegreePlanArchive_StudentId()
            {
                Assert.AreEqual(degreePlan.PersonId, archive.StudentId);
            }

            [TestMethod]
            public void CreatedDegreePlanArchive_Version()
            {
                Assert.AreEqual(degreePlan.Version, archive.Version);
            }

            [TestMethod]
            public void CreatedDegreePlanArchive_ReviewedBy()
            {
                Assert.AreEqual(degreePlan.LastReviewedAdvisorId, archive.ReviewedBy);
            }

            [TestMethod]
            public void CreatedDegreePlanArchive_ReviewedDate()
            {
                Assert.AreEqual(degreePlan.LastReviewedDate, archive.ReviewedDate);
            }

            [TestMethod]
            public void CreatedDegreePlanArchive_StudentPrograms()
            {
                Assert.AreEqual(studentPrograms.Count(), archive.StudentPrograms.Count());
            }

            [TestMethod]
            public void CreatedDegreePlanArchive_Notes()
            {
                Assert.AreEqual(2, archive.Notes.Count());
            }

            [TestMethod]
            public void CreatedDegreePlanArchive_ArchiveCourses()
            {
                var plannedCoursesCount = degreePlan.NonTermPlannedCourses.Count();
                // For plan 3 there should be 1 nonterm course
                Assert.AreEqual(degreePlan.NonTermPlannedCourses.Count(), archive.ArchivedCourses.Where(a => string.IsNullOrEmpty(a.TermCode)).Count());
                foreach (var termid in degreePlan.TermIds)
                {
                    var plannedCourses = degreePlan.GetPlannedCourses(termid);
                    plannedCoursesCount = plannedCoursesCount + plannedCourses.Count();
                    Assert.AreEqual(plannedCourses.Count(), archive.ArchivedCourses.Where(ac => ac.TermCode == termid).Count());
                }
                Assert.AreEqual(plannedCoursesCount, archive.ArchivedCourses.Count());
            }

        }

        [TestClass]
        public class CreateArchiveCourseTests
        {
            // Although a private method, these tests exist separately because they are formulated to specifically target
            // the results of this method.
            private DegreePlanArchive archive;
            private IEnumerable<StudentProgram> studentPrograms;
            private IEnumerable<Course> courses;
            private IEnumerable<Section> sections;
            private IEnumerable<Term> registrationTerms;
            private DegreePlan degreePlan;
            private IEnumerable<AcademicCredit> academicCredits;
            private IEnumerable<Grade> grades;

            [TestInitialize]
            public async void Initialize()
            {
                // Tests use the following constructor unless otherwise handled.
                studentPrograms = new TestStudentProgramRepository().Get("0000896");
                courses =await new TestCourseRepository().GetAsync();
                registrationTerms = await new TestTermRepository().GetRegistrationTermsAsync();
                sections = await new TestSectionRepository().GetRegistrationSectionsAsync(registrationTerms);
                grades = await new TestGradeRepository().GetAsync();
                degreePlan = await (new TestStudentDegreePlanRepository()).GetAsync(4);
                var academicCredits = await new TestAcademicCreditRepository().GetAsync();
                degreePlan.Notes = new List<DegreePlanNote>();
            }

            [TestCleanup]
            public void Cleanup()
            {
                archive = null;
            }

            [TestMethod]
            public void NullAcademicCredit_Ignored()
            {
                var ac1 = new AcademicCredit("123");
                ac1 = null;
                academicCredits = new List<AcademicCredit>() { ac1 };
                archive = Ellucian.Colleague.Domain.Planning.Entities.DegreePlanArchive.CreateDegreePlanArchive(degreePlan, "1111111", studentPrograms, courses, sections, academicCredits, grades);
                Assert.IsTrue(archive.ArchivedCourses.Count() == 0);
            }

            [TestMethod]
            public void AcademicCreditWithNoCourse_Ignored()
            {
                var ac1 = new AcademicCredit("123");
                academicCredits = new List<AcademicCredit>() { ac1 };
                archive = Ellucian.Colleague.Domain.Planning.Entities.DegreePlanArchive.CreateDegreePlanArchive(degreePlan, "1111111", studentPrograms, courses, sections, academicCredits, grades);
                Assert.IsTrue(archive.ArchivedCourses.Count() == 0);
            }

            [TestMethod]
            public void PlannedCourse_IncludedInArchiveWithCourseData()
            {
                var course = courses.Where(c => c.Id == "47").First();
                var termCode = registrationTerms.Select(t=>t.Code).First();
                var plannedCourse = new PlannedCourse(course.Id, null, GradingType.Graded, Student.Entities.DegreePlans.WaitlistStatus.NotWaitlisted, "0000011", DateTime.Now);
                degreePlan.AddCourse(plannedCourse, termCode);
                academicCredits = new List<AcademicCredit>();
                archive = Ellucian.Colleague.Domain.Planning.Entities.DegreePlanArchive.CreateDegreePlanArchive(degreePlan, "1111111", studentPrograms, courses, sections, academicCredits, grades);
                
                Assert.IsTrue(archive.ArchivedCourses.Count() == 1);
                Assert.AreEqual(course.Id, archive.ArchivedCourses.ElementAt(0).CourseId);
                Assert.AreEqual(course.Name, archive.ArchivedCourses.ElementAt(0).Name);
                Assert.AreEqual(course.Title, archive.ArchivedCourses.ElementAt(0).Title);
                Assert.AreEqual(course.MinimumCredits, archive.ArchivedCourses.ElementAt(0).Credits);
                Assert.AreEqual(termCode, archive.ArchivedCourses.ElementAt(0).TermCode);
                Assert.AreEqual(null, archive.ArchivedCourses.ElementAt(0).SectionId);
                Assert.AreEqual(plannedCourse.AddedBy, archive.ArchivedCourses.ElementAt(0).AddedBy);
                Assert.AreEqual(plannedCourse.AddedOn, archive.ArchivedCourses.ElementAt(0).AddedOn);
            }

            [TestMethod]
            public void PlannedCourse_IncludedInArchiveWithLatestApprovalData()
            {
                var course = courses.Where(c => c.Id == "47").First();
                var termCode = registrationTerms.Select(t => t.Code).First();
                var plannedCourse = new PlannedCourse(course.Id, null, GradingType.Graded, Student.Entities.DegreePlans.WaitlistStatus.NotWaitlisted, "0000011", DateTime.Now);
                degreePlan.AddCourse(plannedCourse, termCode);
                var approvalPerson = "0000123";
                var approvalDate = new DateTime(2012, 02, 15, 01, 15, 00);
                degreePlan.AddApproval(approvalPerson, DegreePlanApprovalStatus.Approved, approvalDate, plannedCourse.CourseId, termCode);
                var denialPerson = "0000234";
                var denialDate = new DateTime(2012, 02, 16, 08, 15, 00);
                degreePlan.AddApproval(denialPerson, DegreePlanApprovalStatus.Denied, denialDate, plannedCourse.CourseId, termCode);
                academicCredits = new List<AcademicCredit>();

                archive = Ellucian.Colleague.Domain.Planning.Entities.DegreePlanArchive.CreateDegreePlanArchive(degreePlan, "1111111", studentPrograms, courses, sections, academicCredits, grades);

                Assert.IsTrue(archive.ArchivedCourses.Count() == 1);
                Assert.AreEqual(course.Id, archive.ArchivedCourses.ElementAt(0).CourseId);
                Assert.AreEqual(DegreePlanApprovalStatus.Denied.ToString(), archive.ArchivedCourses.ElementAt(0).ApprovalStatus);
                Assert.AreEqual(denialPerson, archive.ArchivedCourses.ElementAt(0).ApprovedBy);
                Assert.AreEqual(denialDate, archive.ArchivedCourses.ElementAt(0).ApprovalDate);
            }

            [TestMethod]
            public void PlannedSection_IncludedInArchiveWithSectionData()
            {
                var course = courses.Where(c => c.Id == "47").First();
                var termCode = registrationTerms.Select(t=>t.Code).First();
                var section = sections.Where(s => s.CourseId == course.Id && s.TermId == termCode).First();
                var plannedCourse = new PlannedCourse(course.Id, section.Id, GradingType.Graded, Student.Entities.DegreePlans.WaitlistStatus.NotWaitlisted, "0000011", DateTime.Now);
                degreePlan.AddCourse(plannedCourse, termCode);
                academicCredits = new List<AcademicCredit>();
                
                archive = Ellucian.Colleague.Domain.Planning.Entities.DegreePlanArchive.CreateDegreePlanArchive(degreePlan, "1111111", studentPrograms, courses, sections, academicCredits, grades);

                Assert.IsTrue(archive.ArchivedCourses.Count() == 1);
                Assert.AreEqual(course.Id, archive.ArchivedCourses.ElementAt(0).CourseId);
                Assert.AreEqual(section.Id, archive.ArchivedCourses.ElementAt(0).SectionId);
                Assert.AreNotEqual(course.Title, archive.ArchivedCourses.ElementAt(0).Title);
                Assert.AreEqual(section.Title, archive.ArchivedCourses.ElementAt(0).Title);
                Assert.AreEqual(course.Name + " " + section.Number, archive.ArchivedCourses.ElementAt(0).Name);
            }

            [TestMethod]
            public async Task PlannedCoursesAndAcademicCreditWithoutCourse_BothIncludedInArchive()
            {
                var ac1 = new AcademicCredit("123");
                academicCredits = new List<AcademicCredit>() { ac1 };
                degreePlan.AddCourse(new PlannedCourse("47"), registrationTerms.ElementAt(0).Code);
                academicCredits = new List<AcademicCredit>() { (await new TestAcademicCreditRepository().GetAsync()).First() };
                archive = Ellucian.Colleague.Domain.Planning.Entities.DegreePlanArchive.CreateDegreePlanArchive(degreePlan, "1111111", studentPrograms, courses, sections, academicCredits, grades);
                Assert.IsTrue(archive.ArchivedCourses.Count() == 2);
            }

            [TestMethod]
            public void PlannedCoursesAndAcademicCreditsNotMatchingSectionId_BothItemsIncludedInArchive()
            {
                // Use the first course in the test course repository
                var course = courses.First();
                // use the first term in the registration terms
                var termCode = registrationTerms.ElementAt(0).Code;
                // Add this course to the plan
                var section1 = sections.Where(s => s.CourseId == course.Id && s.TermId == termCode).ElementAt(0);
                var plannedCourse = new PlannedCourse(course.Id, section1.Id, GradingType.Graded, Student.Entities.DegreePlans.WaitlistStatus.NotWaitlisted, "00012345", DateTime.Now);
                degreePlan.AddCourse(plannedCourse, termCode);
                // Get a second section for this course/term
                var section2 = sections.Where(s => s.CourseId == course.Id && s.TermId == termCode).ElementAt(1);
                // Add an academic credit for this second section
                var academicCredit = new AcademicCredit("1111", course, section2.Id);
                academicCredits = new List<AcademicCredit>() { academicCredit };
                // Verifies two different sections have been selected.
                Assert.AreNotEqual(section1.Id, section2.Id);

                archive = Ellucian.Colleague.Domain.Planning.Entities.DegreePlanArchive.CreateDegreePlanArchive(degreePlan, "1111111", studentPrograms, courses, sections, academicCredits, grades);
                
                Assert.IsTrue(archive.ArchivedCourses.Count() == 2);
                Assert.IsNotNull(archive.ArchivedCourses.Where(ac=>ac.SectionId == section1.Id));
                Assert.IsNotNull(archive.ArchivedCourses.Where(ac=>ac.SectionId == section2.Id));
            }

            [TestMethod]
            public void PlannedCoursesAndAcademicCreditsMatchedByCourseAndTerm_AcademicCreditInfoIncludedInArchive()
            {
                // Use the first course in the test course repository
                var course = courses.First();
                // use the first term in the registration terms
                var termCode = registrationTerms.ElementAt(0).Code;
                // Add this course to the plan
                var plannedCourse = new PlannedCourse(course.Id, null, GradingType.Graded, Student.Entities.DegreePlans.WaitlistStatus.NotWaitlisted, "00012345", DateTime.Now);
                degreePlan.AddCourse(new PlannedCourse(course.Id), termCode);
                // Get a section for this course/term
                var section = sections.Where(s=>s.CourseId == course.Id && s.TermId == termCode).First();
                // Add an academic credit for this section with a withdraw grade
                var grade = grades.Where(g=>g.IsWithdraw==true).FirstOrDefault();
                var academicCredit = new AcademicCredit("1111", course, section.Id) { Credit = 6m, ContinuingEducationUnits = 7m, VerifiedGrade = grade, GradeSchemeCode = "UG", TermCode = section.TermId, AdjustedCredit = 3m };
                academicCredits = new List<AcademicCredit>() { academicCredit };
                archive = Ellucian.Colleague.Domain.Planning.Entities.DegreePlanArchive.CreateDegreePlanArchive(degreePlan, "1111111", studentPrograms, courses, sections, academicCredits, grades);
                Assert.IsTrue(archive.ArchivedCourses.Count() == 1);
                Assert.AreEqual(academicCredit.Status.ToString(), archive.ArchivedCourses.ElementAt(0).RegistrationStatus);
                Assert.AreEqual(academicCredit.AdjustedCredit, archive.ArchivedCourses.ElementAt(0).Credits);
                Assert.AreEqual(academicCredit.ContinuingEducationUnits, archive.ArchivedCourses.ElementAt(0).ContinuingEducationUnits);
                Assert.IsTrue(archive.ArchivedCourses.ElementAt(0).HasWithdrawGrade);
            }

            [TestMethod]
            public void PlannedCoursesAndAcademicCreditsMatchedBySection_CombinedInArchive()
            {
                // Use the first course in the test course repository
                var course = courses.First();
                // use the first term in the registration terms
                var termCode = registrationTerms.ElementAt(0).Code;
                // Add this course to the plan
                var section1 = sections.Where(s => s.CourseId == course.Id && s.TermId == termCode).ElementAt(0);
                var plannedCourse = new PlannedCourse(course.Id, section1.Id, GradingType.Graded, Student.Entities.DegreePlans.WaitlistStatus.NotWaitlisted, "00012345", DateTime.Now);
                degreePlan.AddCourse(plannedCourse, termCode);
                // Add an academic credit for the same section
                var academicCredit = new AcademicCredit("1111", course, section1.Id) { TermCode = section1.TermId };
                academicCredits = new List<AcademicCredit>() { academicCredit };

                archive = Ellucian.Colleague.Domain.Planning.Entities.DegreePlanArchive.CreateDegreePlanArchive(degreePlan, "1111111", studentPrograms, courses, sections, academicCredits, grades);
                
                Assert.IsTrue(archive.ArchivedCourses.Count() == 1);
                Assert.IsNotNull(archive.ArchivedCourses.Where(ac=>ac.SectionId == section1.Id));
            }

            [TestMethod]
            public void PlannedCoursesAndMultipleAcademicCreditsMatchedByCourseAndTerm_BothSectionsInArchive()
            {
                // Use the first course in the test course repository
                var course = courses.First();
                // use the first term in the registration terms
                var termCode = registrationTerms.ElementAt(0).Code;
                // Add this course to the plan
                var plannedCourse = new PlannedCourse(course.Id, null, GradingType.Graded, Student.Entities.DegreePlans.WaitlistStatus.NotWaitlisted, "00012345", DateTime.Now);
                degreePlan.AddCourse(plannedCourse, termCode);
                // Add two academic credits for the given course
                var section1 = sections.Where(s => s.CourseId == course.Id && s.TermId == termCode).ElementAt(0);
                var academicCredit1 = new AcademicCredit("1111", course, section1.Id) { TermCode = section1.TermId };
                var section2 = sections.Where(s => s.CourseId == course.Id && s.TermId == termCode).ElementAt(1);
                var academicCredit2 = new AcademicCredit("2222", course, section2.Id) { TermCode = section2.TermId };
                academicCredits = new List<AcademicCredit>() { academicCredit1, academicCredit2 };

                archive = Ellucian.Colleague.Domain.Planning.Entities.DegreePlanArchive.CreateDegreePlanArchive(degreePlan, "1111111", studentPrograms, courses, sections, academicCredits, grades);

                Assert.IsTrue(archive.ArchivedCourses.Count() == 2);
                Assert.IsNotNull(archive.ArchivedCourses.Where(ac => ac.SectionId == section1.Id));
                Assert.IsNotNull(archive.ArchivedCourses.Where(ac => ac.SectionId == section2.Id));
            }

            [TestMethod]
            public void DoesNotArchiveNoncourseAndTransferAcademicCredits()
            {
                // Use the first course in the test course repository
                var course = courses.First();
                // use the first term in the registration terms
                var termCode = registrationTerms.ElementAt(0).Code;
                // Add this course to the plan
                var plannedCourse = new PlannedCourse(course.Id, null, GradingType.Graded, Student.Entities.DegreePlans.WaitlistStatus.NotWaitlisted, "00012345", DateTime.Now);
                degreePlan.AddCourse(new PlannedCourse(course.Id), termCode);
                // Get a random test repository section
                var section = sections.ElementAt(45); 
                // Add an academic credit for nonterm/noncourse item (also verifies linq select against nonterm academic credits is successful)
                var academicCredit = new AcademicCredit("1111") { SectionId = section.Id, Credit = 6m, ContinuingEducationUnits = 7m, Status = CreditStatus.TransferOrNonCourse };
                academicCredits = new List<AcademicCredit>() { academicCredit };
                archive = Ellucian.Colleague.Domain.Planning.Entities.DegreePlanArchive.CreateDegreePlanArchive(degreePlan, "1111111", studentPrograms, courses, sections, academicCredits, grades);
                Assert.IsTrue(archive.ArchivedCourses.Count() == 1);
                Assert.AreEqual(plannedCourse.CourseId, archive.ArchivedCourses.ElementAt(0).CourseId);
            }
        }
    }
}
