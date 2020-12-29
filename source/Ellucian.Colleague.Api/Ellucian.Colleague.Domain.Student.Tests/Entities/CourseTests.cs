// Copyright 2012-2015 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using Ellucian.Colleague.Domain.Student.Entities;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Ellucian.Colleague.Domain.Student.Tests.Entities
{
    [TestClass]
    public class CourseTests
    {
        [TestClass]
        public class Course_Constructor
        {
            private string deptCode1;
            private string deptCode2;
            private ICollection<OfferingDepartment> deptCodes;
            private string subjCode;
            private string acadLevelCode;
            private string courseLevelCode1;
            private string courseLevelCode2;
            private ICollection<string> courseLevelCodes;
            private string number;
            private string title;
            private string longTitle;
            private decimal credits;
            private decimal ceus;
            private Course course;
            private List<string> courseTypes;
            private ICollection<CourseApproval> approvals;
            

            [TestInitialize]
            public void Initialize()
            {
                deptCode1 = "HIST";
                deptCode2 = "HUMN";
                deptCodes = new List<OfferingDepartment>() { new OfferingDepartment(deptCode1, 50m), new OfferingDepartment(deptCode2, 50m) };
                subjCode = "HIS";
                acadLevelCode = "UG";
                courseLevelCode1 = "100";
                courseLevelCode2 = "200";
                courseLevelCodes = new List<string>() { courseLevelCode1, courseLevelCode2 };
                title = "American History";
                longTitle = "Longer American History";
                number = "105";
                credits = 3.0m;
                ceus = 1.0m;
                approvals = new List<CourseApproval>() { new CourseApproval("A", DateTime.Today, "0000043", "0003315", DateTime.Today) };
                course = new Course("2", title, longTitle, deptCodes, subjCode, number, acadLevelCode, courseLevelCodes, credits, ceus, approvals);
                courseTypes = new List<string>() { "A", "B", "C" };
               
            }

            [TestMethod]
            public void Course_Constructor_Title()
            {
                // Title updated
                Assert.AreEqual(title, course.Title);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void Course_Constructor_Title_NullException()
            {
                // Null title causes exception
                course = new Course("2", null, longTitle, deptCodes, subjCode, number, acadLevelCode, courseLevelCodes, credits, ceus, approvals);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void Course_Constructor_Title_EmptyException()
            {
                // Null title causes exception
                course = new Course("2", string.Empty, longTitle, deptCodes, subjCode, number, acadLevelCode, courseLevelCodes, credits, ceus, approvals);
            }

            [TestMethod]
            public void Course_Constructor_LongTitle_Null_ShortTitleUsed()
            {
                // Null title causes exception
                course = new Course("2", title, null, deptCodes, subjCode, number, acadLevelCode, courseLevelCodes, credits, ceus, approvals);
                Assert.AreEqual(title, course.LongTitle);
            }

            [TestMethod]
            public void Course_Constructor_LongTitle_Empty_ShortTitleUsed()
            {
                // Null title causes exception
                course = new Course("2", title, string.Empty, deptCodes, subjCode, number, acadLevelCode, courseLevelCodes, credits, ceus, approvals);
                Assert.AreEqual(title, course.LongTitle);
            }

            [TestMethod]
            public void Course_Constructor_LongTitle_Verify()
            {
                // Null title causes exception
                course = new Course("2", title, longTitle, deptCodes, subjCode, number, acadLevelCode, courseLevelCodes, credits, ceus, approvals);
                Assert.AreEqual(longTitle, course.LongTitle);
            }

            [TestMethod]
            public void Course_Constructor_Departments()
            {
                // Departments updated
                Assert.IsTrue(deptCode1.Equals(course.Departments.Where(d => d.AcademicDepartmentCode == deptCode1).First().AcademicDepartmentCode));
                Assert.IsTrue(deptCode2.Equals(course.Departments.Where(d => d.AcademicDepartmentCode == deptCode2).First().AcademicDepartmentCode));
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void Course_Constructor_Departments_NullException()
            {
                // null department list causes exception
                course = new Course("2", title, longTitle, null, subjCode, number, acadLevelCode, courseLevelCodes, credits, ceus, approvals);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void Course_Constructor_Departments_EmptyException()
            {
                // null department list causes exception
                course = new Course("2", title, longTitle, new List<OfferingDepartment>(), subjCode, number, acadLevelCode, courseLevelCodes, credits, ceus, approvals);
            }

            [TestMethod]
            public void Course_Constructor_SubjectCode()
            {
                // Subject updated
                Assert.IsTrue(subjCode.Equals(course.SubjectCode));
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void Course_Constructor_SubjectCode_NullException()
            {
                // null subject causes exception
                course = new Course("2", title, longTitle, deptCodes, null, number, acadLevelCode, courseLevelCodes, credits, ceus, approvals);
            }
            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void Course_Constructor_SubjectCode__empty_string_NullException()
            {
                // null subject causes exception
                course = new Course("2", title, longTitle, deptCodes, "", number, acadLevelCode, courseLevelCodes, credits, ceus, approvals);
            }

            [TestMethod]
            public void Course_Constructor_Number()
            {
                // number updated
                Assert.AreEqual(number, course.Number);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void Course_Constructor_Number_NullException()
            {
                // null number causes exception
                course = new Course("2", title, longTitle, deptCodes, subjCode, null, acadLevelCode, courseLevelCodes, credits, ceus, approvals);
            }

            [TestMethod]
            public void Course_Constructor_AcademicLevelCode()
            {
                // Academic level updated
                Assert.AreEqual(acadLevelCode, course.AcademicLevelCode);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void Course_Constructor_AcademicLevelCode_NullException()
            {
                // null academic level causes exception
                course = new Course("2", title, longTitle, deptCodes, subjCode, number, null, courseLevelCodes, credits, ceus, approvals);
            }

            [TestMethod]
            public void Course_Constructor_CourseLevelCodes()
            {
                // Course level updated
                Assert.AreEqual(courseLevelCode1, course.CourseLevelCodes.Where(cl => cl == courseLevelCode1).First());
                Assert.AreEqual(courseLevelCode2, course.CourseLevelCodes.Where(cl => cl == courseLevelCode2).First());
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void Course_Constructor_CourseLevelCodes_NullException()
            {
                // null course level list causes exception
                course = new Course("2", title, longTitle, deptCodes, subjCode, number, acadLevelCode, null, credits, ceus, approvals);
            }

            [TestMethod]
            public void Course_Constructor_MinimumCredits()
            {
                //MinCredits updated
                Assert.AreEqual(credits, course.MinimumCredits);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentOutOfRangeException))]
            public void Course_Constructor_MinimumCredits_ThrowExceptionIfNegative()
            {
                course = new Course("2", title, longTitle, deptCodes, subjCode, number, acadLevelCode, courseLevelCodes, -2.0m, ceus, approvals);
            }

            [TestMethod]
            public void Course_Constructor_Ceus()
            {
                // CEUs updated
                Assert.AreEqual(ceus, course.Ceus);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentOutOfRangeException))]
            public void Course_Constructor_Ceus_ThrowExceptionIfNegative()
            {
                course = new Course("2", title, longTitle, deptCodes, subjCode, number, acadLevelCode, courseLevelCodes, 2.0m, -2.0m, approvals);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public void Course_Constructor_MinCreditsAndCEUs_NullException()
            {
                // exception if both MinCredits and CEUs null. 
                // Must have at least one or the other, even if 0.
                course = new Course("2", title, longTitle, deptCodes, subjCode, number, acadLevelCode, courseLevelCodes, null, null, approvals);
            }

            [TestMethod]
            public void Course_Constructor_MinCreditsZeroCEUsNull()
            {
                decimal? credits = 0m;
                decimal? ceus = null;
                course = new Course("2", title, longTitle, deptCodes, subjCode, number, acadLevelCode, courseLevelCodes, credits, ceus, approvals);
                Assert.AreEqual(credits, course.MinimumCredits);
                Assert.AreEqual(ceus, course.Ceus);
            }

            [TestMethod]
            public void Course_Constructor_MinCreditsNullCEUsZero()
            {
                decimal? credits = null;
                decimal? ceus = 0m;
                course = new Course("2", title, longTitle, deptCodes, subjCode, number, acadLevelCode, courseLevelCodes, credits, ceus, approvals);
                Assert.AreEqual(credits, course.MinimumCredits);
                Assert.AreEqual(ceus, course.Ceus);
            }

            [TestMethod]
            public void Course_Constructor_MinCreditsZeroCEUsZero()
            {
                decimal? credits = 0m;
                decimal? ceus = 0m;
                course = new Course("2", title, longTitle, deptCodes, subjCode, number, acadLevelCode, courseLevelCodes, credits, ceus, approvals);
                Assert.AreEqual(credits, course.MinimumCredits);
                Assert.AreEqual(ceus, course.Ceus);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void Course_Constructor_CourseApprovals_NullException()
            {
                // null approval list causes exception
                course = new Course("2", title, longTitle, deptCodes, subjCode, number, acadLevelCode, courseLevelCodes, credits, ceus, null);
            }

            [TestMethod]
            public void Course_Constructor_CourseApprovals_Empty()
            {
                // empty approval list defaults to Active course status
                course = new Course("2", title, longTitle, deptCodes, subjCode, number, acadLevelCode, courseLevelCodes, credits, ceus, new List<CourseApproval>());

                Assert.AreEqual(CourseStatus.Active, course.Status);
            }

            [TestMethod]
            public void Course_Constructor_CourseApprovals()
            {
                Assert.AreEqual(approvals.Count, course.CourseApprovals.Count);
                CollectionAssert.AreEqual(approvals.ToList(), course.CourseApprovals.ToList());
            }
        }

        [TestClass]
        public class Course_NonRequiredProperties
        {
            private string deptCode1;
            private string deptCode2;
            private ICollection<OfferingDepartment> deptCodes;
            private string subjCode;
            private string acadLevelCode;
            private string courseLevelCode1;
            private string courseLevelCode2;
            private ICollection<string> courseLevelCodes;
            private string number;
            private string title;
            private string longTitle;
            private decimal credits;
            private decimal ceus;
            private Course course;
            private ICollection<CourseApproval> approvals;
            private string externalSource;

            [TestInitialize]
            public void Initialize()
            {
                deptCode1 = "HIST";
                deptCode2 = "HUMN";
                deptCodes = new List<OfferingDepartment>() { new OfferingDepartment(deptCode1, 75m), new OfferingDepartment(deptCode2, 25m) };
                subjCode = "HIS";
                acadLevelCode = "UG";
                courseLevelCode1 = "100";
                courseLevelCode2 = "200";
                courseLevelCodes = new List<string>() { courseLevelCode1, courseLevelCode2 };
                title = "American History";
                longTitle = "Longer American History";
                number = "105";
                credits = 3.0m;
                ceus = 1.0m;
                approvals = new List<CourseApproval>() { new CourseApproval("A", DateTime.Today, "0000043", "0003315", DateTime.Today) };
                externalSource = "Colleague";
                course = new Course("2", title, longTitle, deptCodes, subjCode, number, acadLevelCode, courseLevelCodes, credits, ceus, approvals);
            }

            [TestMethod]
            public void Course_NonRequiredProperties_Description_EmptyAllowed()
            {
                course.Description = "";
                Assert.IsTrue(string.IsNullOrEmpty(course.Description));
            }

            [TestMethod]
            public void Course_NonRequiredProperties_Description_NullAllowed()
            {
                course.Description = null;
                Assert.IsTrue(string.IsNullOrEmpty(course.Description));
            }

            [TestMethod]
            public void Course_NonRequiredProperties_Description()
            {
                course.Description = "a description";
                Assert.AreEqual("a description", course.Description);
            }

            [TestMethod]
            public void Course_NonRequiredProperties_ExternalSource()
            {
                course.ExternalSource = externalSource;
                Assert.AreEqual(externalSource, course.ExternalSource);
            }


            [TestMethod]
            public void Course_NonRequiredProperties_TopicCode()
            {
                var topicCode = "Topic Code";
                course.TopicCode = topicCode;
                Assert.AreEqual(topicCode, course.TopicCode);
            }

            [TestMethod]
            public void Course_NonRequiredProperties_Types_IsEmptyString()
            {
                Assert.IsTrue(course.Types.Count() == 0);
            }

            [TestMethod]
            public void Course_NonRequiredProperties_MaximumCredits()
            {
                decimal? credits = 0m;
                decimal? ceus = 0m;
                course = new Course("2", title, longTitle, deptCodes, subjCode, number, acadLevelCode, courseLevelCodes, credits, ceus, approvals);
                course.MaximumCredits = 1m;
                Assert.AreEqual(credits, course.MinimumCredits);
                Assert.AreEqual(credits + 1, course.MaximumCredits);
            }

            [TestMethod]
            public void Course_NonRequiredProperties_VariableCreditIncrement()
            {
                decimal? minCredits = 0m;
                decimal? maxCredits = 1m;
                decimal? credIncr = 1m;
                decimal? ceus = 0m;
                course = new Course("2", title, longTitle, deptCodes, subjCode, number, acadLevelCode, courseLevelCodes, minCredits, ceus, approvals);
                course.MaximumCredits = maxCredits;
                course.VariableCreditIncrement = credIncr;
                Assert.AreEqual(minCredits, course.MinimumCredits);
                Assert.AreEqual(maxCredits, course.MaximumCredits);
                Assert.AreEqual(credIncr, course.VariableCreditIncrement);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentOutOfRangeException))]
            public void Course_NonRequiredProperties_MaxCredits_ThrowErrorIfNegative()
            {
                course.MaximumCredits = -2.0m;
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentOutOfRangeException))]
            public void Course_NonRequiredProperties_MinCreditsNullAndMaxCreditsSuppliedException()
            {
                // exception if MaxCredits supplied and MinCredits null. 
                course = new Course("2", title, longTitle, deptCodes, subjCode, number, acadLevelCode, courseLevelCodes, null, ceus, approvals);
                course.MaximumCredits = 3m;
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentOutOfRangeException))]
            public void Course_NonRequiredProperties_MaxCredits_ThrowErrorIfLessThanMinCredits()
            {
                course.MaximumCredits = 1.0m;
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentOutOfRangeException))]
            public void Course_NonRequiredProperties_VariableCreditIncrement_ThrowsExceptionIfNegative()
            {
                course.VariableCreditIncrement = -1.0m;
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentOutOfRangeException))]
            public void Course_NonRequiredProperties_MaxCreditsNullAndVarIncrCreditsSuppliedException()
            {
                // exception if VarIncrCredits supplied and MaxCredits null. 
                course = new Course("2", title, longTitle, deptCodes, subjCode, number, acadLevelCode, courseLevelCodes, 3.0m, null, approvals);
                course.VariableCreditIncrement = 1m;
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentOutOfRangeException))]
            public void Course_NonRequiredProperties_VariableCreditIncrement_ThrowsExceptionIfLargerThanMaximum()
            {
                course.MaximumCredits = 3.0m;
                course.VariableCreditIncrement = 4.0m;
            }

            [TestMethod]
            public void Course_NonRequiredProperties_LocationCodes()
            {
                var locCodes = new List<string>() { "MAIN", "WEST" };
                course.LocationCodes = locCodes;
                Assert.AreEqual("MAIN", course.LocationCodes.ElementAt(0));
                Assert.AreEqual("WEST", course.LocationCodes.ElementAt(1));
                Assert.AreEqual(2, course.LocationCodes.Count());
            }

            [TestMethod]
            public void Course_NonRequiredProperties_LocationCodes_IsEmptyListIfSetToNull()
            {
                course.LocationCodes = null;
                Assert.AreEqual(0, course.LocationCodes.Count());
            }

            [TestMethod]
            public void Course_NonRequiredProperties_LocationCodes_EmptyListCreated()
            {
                Assert.AreEqual(0, course.LocationCodes.Count());
            }

            [TestMethod]
            public void Course_NonRequiredProperties_Requisites()
            {
                course = new Course("2", title, longTitle, deptCodes, subjCode, number, acadLevelCode, courseLevelCodes, 0, 0, approvals);
                var coreq1 = new Requisite("01", true);
                var coreq2 = new Requisite("02", false);
                course.Requisites = new List<Requisite>() { coreq1, coreq2 };
                Assert.AreEqual(coreq1, course.Requisites.ElementAt(0));
                Assert.AreEqual(coreq2, course.Requisites.ElementAt(1));
            }

            [TestMethod]
            public void Course_NonRequiredProperties_Requisites_NewFormat()
            {
                Course course2 = new Course("2", title, longTitle, deptCodes, subjCode, number, acadLevelCode, courseLevelCodes, 0, 0, approvals);

                var requisite1 = new Requisite("12345", true, RequisiteCompletionOrder.Previous, true);
                var requisite2 = new Requisite("6789", false, RequisiteCompletionOrder.Concurrent, false);
                var requisites = new List<Requisite>() { requisite1, requisite2 };
                course.Requisites = requisites;
                Assert.AreEqual(2, course.Requisites.Count());
                var course2Requisite1 = course.Requisites.ElementAt(0);
                Assert.AreEqual("12345", course2Requisite1.RequirementCode);
                Assert.AreEqual(true, course2Requisite1.IsRequired);
                Assert.AreEqual(RequisiteCompletionOrder.Previous, course2Requisite1.CompletionOrder);
                Assert.AreEqual(true, course2Requisite1.IsProtected);
                Assert.IsNull(course2Requisite1.CorequisiteCourseId);
                var course2Requisite2 = course.Requisites.ElementAt(1);
                Assert.AreEqual("6789", course2Requisite2.RequirementCode);
                Assert.AreEqual(false, course2Requisite2.IsRequired);
                Assert.AreEqual(RequisiteCompletionOrder.Concurrent, course2Requisite2.CompletionOrder);
                Assert.AreEqual(false, course2Requisite2.IsProtected);
                Assert.IsNull(course2Requisite2.CorequisiteCourseId);
            }

            [TestMethod]
            public void Course_NonRequiredProperties_Requisites_OldCoreqFormat()
            {
                Course course2 = new Course("2", title, longTitle, deptCodes, subjCode, number, acadLevelCode, courseLevelCodes, 0, 0, approvals);
                var requisite1 = new Requisite("1111", true);
                var requisite2 = new Requisite("2222", false);
                var requisites = new List<Requisite>() { requisite1, requisite2 };
                course.Requisites = requisites;
                Assert.AreEqual(2, course.Requisites.Count());
                var course2Requisite1 = course.Requisites.ElementAt(0);
                Assert.AreEqual("1111", course2Requisite1.CorequisiteCourseId);
                Assert.AreEqual(true, course2Requisite1.IsRequired);
                Assert.AreEqual(RequisiteCompletionOrder.PreviousOrConcurrent, course2Requisite1.CompletionOrder);
                Assert.IsNull(course2Requisite1.RequirementCode);
                var course2Requisite2 = course.Requisites.ElementAt(1);
                Assert.AreEqual("2222", course2Requisite2.CorequisiteCourseId);
                Assert.AreEqual(false, course2Requisite2.IsRequired);
                Assert.AreEqual(RequisiteCompletionOrder.PreviousOrConcurrent, course2Requisite2.CompletionOrder);
                Assert.IsNull(course2Requisite2.RequirementCode);
            }

            [TestMethod]
            public void Course_NonRequiredProperties_Requisites_IsEmptyListIfSetToNull()
            {
                course = new Course("2", title, longTitle, deptCodes, subjCode, number, acadLevelCode, courseLevelCodes, 0, 0, approvals);
                course.Requisites = null;
                Assert.AreEqual(0, course.Requisites.Count());
            }

            [TestMethod]
            public void Course_NonRequiredProperties_EquatedCourseIds()
            {
                var equates = new List<string>() { "1", "2" };
                course.AddEquatedCourseId(equates.ElementAt(0));
                Assert.AreEqual(equates.ElementAt(0), course.EquatedCourseIds.ElementAt(0));
            }
        }

        [TestClass]
        public class Course_IsCurrent
        {
            private string deptCode1;
            private string deptCode2;
            private ICollection<OfferingDepartment> deptCodes;
            private string subjCode;
            private string acadLevelCode;
            private string courseLevelCode1;
            private string courseLevelCode2;
            private ICollection<string> courseLevelCodes;
            private string number;
            private string title;
            private string longTitle;
            private decimal credits;
            private decimal ceus;
            private Course course;
            private ICollection<CourseApproval> approvals;

            [TestInitialize]
            public void Initialize()
            {
                deptCode1 = "HIST";
                deptCode2 = "HUMN";
                deptCodes = new List<OfferingDepartment>() { new OfferingDepartment(deptCode1, 50m), new OfferingDepartment(deptCode2, 50m) };
                subjCode = "HIS";
                acadLevelCode = "UG";
                courseLevelCode1 = "100";
                courseLevelCode2 = "200";
                courseLevelCodes = new List<string>() { courseLevelCode1, courseLevelCode2 };
                title = "American History";
                longTitle = "Longer American History";
                number = "105";
                credits = 3.0m;
                ceus = 1.0m;
                approvals = new List<CourseApproval>() { new CourseApproval("A", DateTime.Today, "0000043", "0003315", DateTime.Today) };
                course = new Course("2", title, longTitle, deptCodes, subjCode, number, acadLevelCode, courseLevelCodes, credits, ceus, approvals);
            }

            [TestMethod]
            public void ActiveStatusAndNullStartDateAndNullEndDateIsCurrent()
            {
                course = new Course("2", title, longTitle, deptCodes, subjCode, number, acadLevelCode, courseLevelCodes, credits, ceus, approvals);
                Assert.IsFalse(course.IsCurrent);
            }


            [TestMethod]
            public void ActiveStatusAndPastStartDateAndNullEndDateIsCurrent()
            {
                course = new Course("2", title, longTitle, deptCodes, subjCode, number, acadLevelCode, courseLevelCodes, credits, ceus, approvals);
                course.StartDate = DateTime.Today.AddDays(-30);
                Assert.IsTrue(course.IsCurrent);
            }

            [TestMethod]
            public void ActiveStatusAndTodayStartDateAndNullEndDateIsCurrent()
            {
                course = new Course("2", title, longTitle, deptCodes, subjCode, number, acadLevelCode, courseLevelCodes, credits, ceus, approvals);
                course.StartDate = DateTime.Today;
                Assert.IsTrue(course.IsCurrent); ;
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentOutOfRangeException))]
            public void EndDateBeforeStartDate_ThrowsException()
            {
                course = new Course("2", title, longTitle, deptCodes, subjCode, number, acadLevelCode, courseLevelCodes, credits, ceus, approvals);
                course.StartDate = DateTime.Today;
                var endDate = DateTime.Today.AddDays(-10);
                course.EndDate = endDate;
            }

            [TestMethod]
            public void EndDateEqualToStartDate_NoException()
            {
                course = new Course("2", title, longTitle, deptCodes, subjCode, number, acadLevelCode, courseLevelCodes, credits, ceus, approvals);
                course.StartDate = DateTime.Today;
                var endDate = DateTime.Today;
                course.EndDate = endDate;
                Assert.AreEqual(endDate, course.EndDate);
            }

            [TestMethod]
            public void NullStartDateAndPastEndDate_NoException()
            {
                course = new Course("2", title, longTitle, deptCodes, subjCode, number, acadLevelCode, courseLevelCodes, credits, ceus, approvals);
                var endDate = DateTime.Today.AddDays(-10);
                course.EndDate = endDate;
                Assert.AreEqual(endDate, course.EndDate);
            }

            [TestMethod]
            public void NullStartDateAndTodayEndDate_NoException()
            {
                course = new Course("2", title, longTitle, deptCodes, subjCode, number, acadLevelCode, courseLevelCodes, credits, ceus, approvals);
                var endDate = DateTime.Today;
                course.EndDate = endDate;
                Assert.AreEqual(endDate, course.EndDate);
            }

            [TestMethod]
            public void NullStartDateAndFutureEndDate_NoException()
            {
                course = new Course("2", title, longTitle, deptCodes, subjCode, number, acadLevelCode, courseLevelCodes, credits, ceus, approvals);
                var endDate = DateTime.Today.AddDays(10);
                course.EndDate = endDate;
                Assert.AreEqual(endDate, course.EndDate);
            }


            [TestMethod]
            public void ActiveStatusAndPastStartDateAndPastEndDateIsNotCurrent()
            {
                course = new Course("2", title, longTitle, deptCodes, subjCode, number, acadLevelCode, courseLevelCodes, credits, ceus, approvals);
                course.StartDate = DateTime.Today.AddDays(-30);
                course.EndDate = DateTime.Today.AddDays(-10);
                Assert.IsFalse(course.IsCurrent);
            }

            [TestMethod]
            public void ActiveStatusAndPastStartDateAndTodayEndDateIsCurrent()
            {
                course = new Course("2", title, longTitle, deptCodes, subjCode, number, acadLevelCode, courseLevelCodes, credits, ceus, approvals);
                course.StartDate = DateTime.Today.AddDays(-30);
                course.EndDate = DateTime.Today;
                Assert.IsTrue(course.IsCurrent);
            }


            [TestMethod]
            public void ActiveStatusAndPastStartDateAndFutureEndDateIsCurrent()
            {
                course = new Course("2", title, longTitle, deptCodes, subjCode, number, acadLevelCode, courseLevelCodes, credits, ceus, approvals);
                course.StartDate = DateTime.Today.AddDays(-30);
                course.EndDate = DateTime.Today.AddDays(10);
                Assert.IsTrue(course.IsCurrent);
            }

            [TestMethod]
            public void ActiveStatusAndTodayStartDateAndTodayEndDateIsCurrent()
            {
                course = new Course("2", title, longTitle, deptCodes, subjCode, number, acadLevelCode, courseLevelCodes, credits, ceus, approvals);
                course.StartDate = DateTime.Today;
                course.EndDate = DateTime.Today;
                Assert.IsTrue(course.IsCurrent);
            }

            [TestMethod]
            public void ActiveStatusAndTodayStartDateAndFutureEndDateIsCurrent()
            {
                course = new Course("2", title, longTitle, deptCodes, subjCode, number, acadLevelCode, courseLevelCodes, credits, ceus, approvals);
                course.StartDate = DateTime.Today;
                course.EndDate = DateTime.Today.AddDays(10);
                Assert.IsTrue(course.IsCurrent);
            }


            [TestMethod]
            public void NonActiveStatusAndCurrentDatesIsNotCurrent()
            {
                course = new Course("2", title, longTitle, deptCodes, subjCode, number, acadLevelCode, courseLevelCodes, credits, ceus, new List<CourseApproval>() { new CourseApproval("T", DateTime.Today, "0000043", "0003315", DateTime.Today) { Status = CourseStatus.Terminated } });
                course.StartDate = DateTime.Today.AddDays(-5);
                Assert.IsFalse(course.IsCurrent); ;
            }

            [TestMethod]
            public void NonActiveStatusAndNullDatesIsNotCurrent()
            {
                course = new Course("2", title, longTitle, deptCodes, subjCode, number, acadLevelCode, courseLevelCodes, credits, ceus, new List<CourseApproval>() { new CourseApproval("T", DateTime.Today, "0000043", "0003315", DateTime.Today) { Status = CourseStatus.Terminated } });
                Assert.IsFalse(course.IsCurrent);
            }

            public void NonActiveStatusAndFutureDatesIsNotCurrent()
            {
                course = new Course("2", title, longTitle, deptCodes, subjCode, number, acadLevelCode, courseLevelCodes, credits, ceus, approvals);
                course.StartDate = DateTime.Today.AddDays(1);
                course.EndDate = DateTime.Today.AddDays(30);
                Assert.IsFalse(course.IsCurrent);
            }

        }


        [TestClass]
        public class CourseGetHashCode
        {
            private string deptCode1;
            private string deptCode2;
            private ICollection<OfferingDepartment> deptCodes;
            private string subjectCode;
            private string acadLevelCode;
            string courseLevelCode1;
            string courseLevelCode2;
            private ICollection<string> courseLevelCodes;
            private string number;
            private string title;
            private string longTitle;
            private decimal credits;
            private decimal ceus;
            private Course course1;
            private Course course2;
            private Course course3;
            private ICollection<CourseApproval> approvals;

            [TestInitialize]
            public void Initialize()
            {
                deptCode1 = "HIST";
                deptCode2 = "HUMN";
                deptCodes = new List<OfferingDepartment>() { new OfferingDepartment(deptCode1, 50m), new OfferingDepartment(deptCode2, 50m) };
                subjectCode = "HIS";
                acadLevelCode = "UG";
                courseLevelCode1 = "100";
                courseLevelCode2 = "100";
                courseLevelCodes = new List<string>() { courseLevelCode1, courseLevelCode2 };
                title = "American History";
                longTitle = "Longer American History";
                number = "105";
                credits = 3.0m;
                ceus = 1.0m;
                approvals = new List<CourseApproval>() { new CourseApproval("A", DateTime.Today, "0000043", "0003315", DateTime.Today) };
                course1 = new Course("2", title, longTitle, deptCodes, subjectCode, number, acadLevelCode, courseLevelCodes, credits, ceus, approvals);
                course2 = new Course("3", title, longTitle, deptCodes, subjectCode, number, acadLevelCode, courseLevelCodes, credits, ceus, approvals);
                course3 = new Course("2", "European History", "Longer European History", deptCodes, subjectCode, number, acadLevelCode, courseLevelCodes, credits, ceus, approvals);
            }

            [TestMethod]
            public void CourseGetHashCodeTest()
            {
                // objects with equal ID have equal hash.
                Assert.AreEqual(course1.GetHashCode(), course3.GetHashCode());
                Assert.AreNotEqual(course2.GetHashCode(), course3.GetHashCode());
            }
        }

        [TestClass]
        public class CourseEquals
        {
            private string deptCode1;
            private string deptCode2;
            private ICollection<OfferingDepartment> deptCodes;
            private string subjectCode;
            private string acadLevelCode;
            private string courseLevelCode1;
            private string courseLevelCode2;
            private ICollection<string> courseLevelCodes;
            private string number;
            private string title;
            private string longTitle;
            private decimal credits;
            private decimal ceus;
            private Course course1;
            private Course course2;
            private Course course3;
            private ICollection<CourseApproval> approvals;

            [TestInitialize]
            public void Initialize()
            {
                deptCode1 = "HIST";
                deptCode2 = "HUMN";
                deptCodes = new List<OfferingDepartment>() { new OfferingDepartment(deptCode1, 50m), new OfferingDepartment(deptCode2, 50m) };
                subjectCode = "HIS";
                acadLevelCode = "UG";
                courseLevelCode1 = "100";
                courseLevelCode2 = "200";
                courseLevelCodes = new List<string>() { courseLevelCode1, courseLevelCode2 };
                title = "American History";
                longTitle = "Longer American History";
                number = "105";
                credits = 3.0m;
                ceus = 1.0m;
                approvals = new List<CourseApproval>() { new CourseApproval("A", DateTime.Today, "0000043", "0003315", DateTime.Today) };
                course1 = new Course("2", title, longTitle, deptCodes, subjectCode, number, acadLevelCode, courseLevelCodes, credits, ceus, approvals);
                course2 = new Course("3", title, longTitle, deptCodes, subjectCode, number, acadLevelCode, courseLevelCodes, credits, ceus, approvals);
                course3 = new Course("2", "European History", "Longer European History", deptCodes, subjectCode, number, acadLevelCode, courseLevelCodes, credits, ceus, approvals);
            }

            [TestMethod]
            public void CourseEqualsTest()
            {
                // ID determines equality
                Assert.IsTrue(course1.Equals(course3));
                Assert.IsFalse(course2.Equals(course3));
            }
        }

        [TestClass]
        public class Course_AddType
        {
            private string deptCode1;
            private string deptCode2;
            private ICollection<OfferingDepartment> deptCodes;
            private string subjectCode;
            private string acadLevelCode;
            private string courseLevelCode1;
            private string courseLevelCode2;
            private ICollection<string> courseLevelCodes;
            private string number;
            private string title;
            private string longTitle;
            private decimal credits;
            private decimal ceus;
            private Course course1;
            private ICollection<CourseApproval> approvals;

            [TestInitialize]
            public void Initialize()
            {
                deptCode1 = "HIST";
                deptCode2 = "HUMN";
                deptCodes = new List<OfferingDepartment>() { new OfferingDepartment(deptCode1, 50m), new OfferingDepartment(deptCode2, 50m) };
                subjectCode = "HIS";
                acadLevelCode = "UG";
                courseLevelCode1 = "100";
                courseLevelCode2 = "200";
                courseLevelCodes = new List<string>() { courseLevelCode1, courseLevelCode2 };
                title = "American History";
                longTitle = "Longer American History";
                number = "105";
                credits = 3.0m;
                ceus = 1.0m;
                approvals = new List<CourseApproval>() { new CourseApproval("A", DateTime.Today, "0000043", "0003315", DateTime.Today) };
                course1 = new Course("2", title, longTitle, deptCodes, subjectCode, number, acadLevelCode, courseLevelCodes, credits, ceus, approvals);
            }

            [TestMethod]
            public void AddType_AddsTypes()
            {
                Assert.IsTrue(course1.Types.Count() == 0);
                var types = new List<string>() { "TYPE1", "TYPE2" };
                foreach (var type in types)
                {
                    course1.AddType(type);
                }
                // Verify count is correct and that items within the collection are correct.
                Assert.IsTrue(course1.Types.Count() == types.Count());
                foreach (var type in types)
                {
                    Assert.IsTrue(course1.Types.Contains(type));
                }
            }

            [TestMethod]
            public void AddType_IgnoresDuplicateTypes()
            {
                Assert.IsTrue(course1.Types.Count() == 0);
                var types = new List<string>() { "TYPE1", "TYPE2", "TYPE1", "TYPE2" };
                foreach (var type in types)
                {
                    course1.AddType(type);
                }
                // Verify that the number and values in the collection match the Distinct values in the original list.
                Assert.IsTrue(course1.Types.Count() == types.Distinct().Count());
                foreach (var type in types)
                {
                    Assert.IsTrue(course1.Types.Contains(type));
                }
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void AddType_ThrowsErrorForNullType()
            {
                var type1 = "TYPEA";
                string type2 = null;
                course1.AddType(type1);
                course1.AddType(type2);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void AddType_ThrowsErrorForEmptyType()
            {
                var type1 = "TYPEA";
                string type2 = string.Empty;
                course1.AddType(type1);
                course1.AddType(type2);
            }

        }

        [TestClass]
        public class Course_AddEquatedCourseIds
        {
            private string deptCode1;
            private string deptCode2;
            private ICollection<OfferingDepartment> deptCodes;
            private string subjectCode;
            private string acadLevelCode;
            private string courseLevelCode1;
            private string courseLevelCode2;
            private ICollection<string> courseLevelCodes;
            private string number;
            private string title;
            private string longTitle;
            private decimal credits;
            private decimal ceus;
            private Course course1;
            private ICollection<CourseApproval> approvals;

            [TestInitialize]
            public void Initialize()
            {
                deptCode1 = "HIST";
                deptCode2 = "HUMN";
                deptCodes = new List<OfferingDepartment>() { new OfferingDepartment(deptCode1, 50m), new OfferingDepartment(deptCode2, 50m) };
                subjectCode = "HIS";
                acadLevelCode = "UG";
                courseLevelCode1 = "100";
                courseLevelCode2 = "200";
                courseLevelCodes = new List<string>() { courseLevelCode1, courseLevelCode2 };
                title = "American History";
                longTitle = "Longer American History";
                number = "105";
                credits = 3.0m;
                ceus = 1.0m;
                approvals = new List<CourseApproval>() { new CourseApproval("A", DateTime.Today, "0000043", "0003315", DateTime.Today) };
                course1 = new Course("2", title, longTitle, deptCodes, subjectCode, number, acadLevelCode, courseLevelCodes, credits, ceus, approvals);
            }

            [TestMethod]
            public void AddEquatedCourseIds_AddsCourseIds()
            {
                Assert.IsTrue(course1.EquatedCourseIds.Count() == 0);
                var courseIds = new List<string>() { "1", "2" };
                foreach (var courseId in courseIds)
                {
                    course1.AddEquatedCourseId(courseId);
                }
                // Verify count is correct and that items within the collection are correct.
                Assert.IsTrue(course1.EquatedCourseIds.Count() == courseIds.Count());
                foreach (var courseId in courseIds)
                {
                    Assert.IsTrue(course1.EquatedCourseIds.Contains(courseId));
                }
            }

            [TestMethod]
            public void AddType_IgnoresDuplicateTypes()
            {
                Assert.IsTrue(course1.EquatedCourseIds.Count() == 0);
                var courseIds = new List<string>() { "1", "2", "2", "1" };
                foreach (var courseId in courseIds)
                {
                    course1.AddEquatedCourseId(courseId);
                }
                // Verify count is correct and that items within the collection are correct.
                Assert.IsTrue(course1.EquatedCourseIds.Count() == courseIds.Distinct().Count());
                foreach (var courseId in courseIds)
                {
                    Assert.IsTrue(course1.EquatedCourseIds.Contains(courseId));
                }
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void AddEquatedCourseId_ThrowsErrorForNullCourseId()
            {
                string courseId = null;
                course1.AddType(courseId);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void AddEquatedCourseId_ThrowsErrorForEmptyCourseId()
            {
                var courseId = string.Empty;
                course1.AddType(courseId);
            }

        }

        [TestClass]
        public class Course_AddLocationCycleRestrictions
        {
            private string deptCode1;
            private string deptCode2;
            private ICollection<OfferingDepartment> deptCodes;
            private string subjectCode;
            private string acadLevelCode;
            private string courseLevelCode1;
            private string courseLevelCode2;
            private ICollection<string> courseLevelCodes;
            private string number;
            private string title;
            private string longTitle;
            private decimal credits;
            private decimal ceus;
            private Course course1;
            private ICollection<CourseApproval> approvals;

            [TestInitialize]
            public void Initialize()
            {
                deptCode1 = "HIST";
                deptCode2 = "HUMN";
                deptCodes = new List<OfferingDepartment>() { new OfferingDepartment(deptCode1, 50m), new OfferingDepartment(deptCode2, 50m) };
                subjectCode = "HIS";
                acadLevelCode = "UG";
                courseLevelCode1 = "100";
                courseLevelCode2 = "200";
                courseLevelCodes = new List<string>() { courseLevelCode1, courseLevelCode2 };
                title = "American History";
                longTitle = "Longer American History";
                number = "105";
                credits = 3.0m;
                ceus = 1.0m;
                approvals = new List<CourseApproval>() { new CourseApproval("A", DateTime.Today, "0000043", "0003315", DateTime.Today) };
                course1 = new Course("2", title, longTitle, deptCodes, subjectCode, number, acadLevelCode, courseLevelCodes, credits, ceus, approvals);
            }

            [TestMethod]
            public void AddLocationCycleRestrictions_ValidAdditionOfRestrictions()
            {
                var restriction1 = new LocationCycleRestriction("LOC1", "SESS1", "YEAR1");
                var restriction2 = new LocationCycleRestriction("LOC2", "SESS2", "YEAR2");
                var restriction3 = new LocationCycleRestriction("LOC3", string.Empty, null);
                course1.AddLocationCycleRestriction(restriction1);
                course1.AddLocationCycleRestriction(restriction2);
                course1.AddLocationCycleRestriction(restriction3);
                Assert.AreEqual(3, course1.LocationCycleRestrictions.Count());

                var result1 = course1.LocationCycleRestrictions.ElementAt(0);
                Assert.AreEqual(restriction1.Location, result1.Location);
                Assert.AreEqual(restriction1.SessionCycle, result1.SessionCycle);
                Assert.AreEqual(restriction1.YearlyCycle, result1.YearlyCycle);

                var result2 = course1.LocationCycleRestrictions.ElementAt(1);
                Assert.AreEqual(restriction2.Location, result2.Location);
                Assert.AreEqual(restriction2.SessionCycle, result2.SessionCycle);
                Assert.AreEqual(restriction2.YearlyCycle, result2.YearlyCycle);

                var result3 = course1.LocationCycleRestrictions.ElementAt(2);
                Assert.AreEqual(restriction3.Location, result3.Location);
                Assert.AreEqual(restriction3.SessionCycle, result3.SessionCycle);
                Assert.AreEqual(restriction3.YearlyCycle, result3.YearlyCycle);
            }


            [TestMethod]
            public void AddLocationCycleRestrictions_AllowsSameLocation()
            {
                // Does not throw error if duplicate locations.
                var restriction1 = new LocationCycleRestriction("LOC1", "SESS1", "YEAR1");
                var restriction2 = new LocationCycleRestriction("LOC1", "SESS2", "YEAR2");

                course1.AddLocationCycleRestriction(restriction1);
                course1.AddLocationCycleRestriction(restriction2);
                Assert.AreEqual(2, course1.LocationCycleRestrictions.Count());
                Assert.AreEqual(2, course1.LocationCycleRestrictions.Where(loc => loc.Location == "LOC1").Count());
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void AddLocationCycleRestrictions_ThrowsErrorIfNoLocationCycleRestrictionProvided()
            {
                course1.AddLocationCycleRestriction(null);
            }

            public void AddLocationCycleRestrictions_DuplicateRestrictionNotAdded()
            {
                // Does not throw error if duplicate locations.
                var restriction1 = new LocationCycleRestriction("LOC1", "SESS1", "YEAR1");
                var restriction2 = new LocationCycleRestriction("LOC1", "SESS1", "YEAR1");

                course1.AddLocationCycleRestriction(restriction1);
                course1.AddLocationCycleRestriction(restriction2);
                Assert.AreEqual(1, course1.LocationCycleRestrictions.Count());
            }

            public void AddLocationCycleRestrictions_DuplicateRestrictionWithNullsNotAdded()
            {
                // Does not throw error if duplicate locations.
                var restriction1 = new LocationCycleRestriction("LOC1", null, null);
                var restriction2 = new LocationCycleRestriction("LOC1", string.Empty, string.Empty);

                course1.AddLocationCycleRestriction(restriction1);
                course1.AddLocationCycleRestriction(restriction2);
                Assert.AreEqual(1, course1.LocationCycleRestrictions.Count());
            }
        }
    }
}
