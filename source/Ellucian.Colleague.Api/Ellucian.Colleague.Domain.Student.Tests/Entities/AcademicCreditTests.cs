// Copyright 2012-2018 Ellucian Company L.P. and its affiliates.
using System;
using System.Linq;
using Ellucian.Colleague.Domain.Student.Entities;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Ellucian.Colleague.Domain.Student.Tests.Entities
{
    [TestClass]
    public class AcademicCreditTests
    {
        private string acadCredId;
        private AcademicCredit ac;
        private Course c;

        [TestInitialize]
        public void Initialize()
        {
            acadCredId = "999";
            c = new TestCourseRepository().GetAsync("21").Result;
        }

        [TestClass]
        public class Constructor : AcademicCreditTests
        {
            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void CourseNullException()
            {
                ac = new AcademicCredit("123", null, "1");
            }
            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void IdNullException()
            {
                ac = new AcademicCredit(null);
            }
            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void IdNullExceptionFullConstructor()
            {
                ac = new AcademicCredit(null, c, "1");
            }
            
        }
        [TestClass]
        public class IsInstitutional : AcademicCreditTests
        {

            [TestMethod]
            public void IsInstitutionalTrue()
            {
                ac = new AcademicCredit(acadCredId);
                ac.Type = CreditType.Institutional;
                Assert.IsTrue(ac.IsInstitutional());
            }
            [TestMethod]
            public void IsInstitutionalFalse()
            {
                ac = new AcademicCredit(acadCredId);
                ac.Type = CreditType.Other;
                Assert.IsFalse(ac.IsInstitutional());
            }
        }
        [TestClass]
        public class GradingTypeTests : AcademicCreditTests
        {
            [TestMethod]
            public void GradedTypeByDefault()
            {
                ac = new AcademicCredit(acadCredId, c, "1111");
                Assert.AreEqual(GradingType.Graded, ac.GradingType);
            }
        }

        // added for mobile
        [TestClass]
        public class AddMidTermGrade : AcademicCreditTests {
            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void AddMidTermGradeThrowsForNullMidTermGrade() {
                ac = new AcademicCredit(acadCredId);
                ac.AddMidTermGrade(null);
            }
        }
        // end added for mobile

        [TestClass]
        public class AddDepartment : AcademicCreditTests
        {
            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void AddDeparmentThrowsForNullDepartment()
            {
                ac = new AcademicCredit(acadCredId);
                ac.AddDepartment(null);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void AddDeparmentThrowsForEmptyDepartment()
            {
                ac = new AcademicCredit(acadCredId);
                ac.AddDepartment("");
            }

            [TestMethod]
            public void AddDepartmentDoesNotAddDuplicateDepartment()
            {
                ac = new AcademicCredit(acadCredId);
                ac.AddDepartment("dept");
                ac.AddDepartment("dept");
                Assert.AreEqual(1, ac.DepartmentCodes.Where(d=>d == "dept").Count());
            }

            [TestMethod]
            public void AddDeparmentSingle()
            {
                ac = new AcademicCredit(acadCredId);
                ac.AddDepartment("HIST");
                Assert.IsTrue(ac.DepartmentCodes.Contains("HIST"));
            }
        }

        [TestClass]
        public class IsTransfer
        {
            [TestMethod]
            public void FalseIfStatusIsTransferAndIsNonCourseIsTrue()
            {
                var ac = new AcademicCredit("123");
                ac.Status = CreditStatus.TransferOrNonCourse;
                ac.IsNonCourse = true;
                Assert.IsFalse(ac.IsTransfer);
            }

            [TestMethod]
            public void TrueIfStatusIsTransferAndIsNonCourseIsFalse()
            {
                var ac = new AcademicCredit("123");
                ac.Status = CreditStatus.TransferOrNonCourse;
                ac.IsNonCourse = false;
                Assert.IsTrue(ac.IsTransfer);                
            }

            [TestMethod]
            public void FalseIfStatusIsNotTransfer()
            {
                var ac = new AcademicCredit("123");
                ac.Status = CreditStatus.New;
                ac.IsNonCourse = false;
                Assert.IsFalse(ac.IsTransfer);        
            }
        }

        [TestClass]
        public class IsCompletedCredit
        {
            [TestMethod]
            public void TrueIfHasVerifiedGrade()
            {
                var ac = new AcademicCredit("123");
                ac.VerifiedGrade = new Grade("A", "A", "UG");
                Assert.IsTrue(ac.IsCompletedCredit);
            }
            
            [TestMethod]
            public void FalseIfIncompleteGrade()
            {
                var ac = new AcademicCredit("123");
                ac.VerifiedGrade = new Grade("I", "I", "UG") { IncompleteGrade = "F" };
                Assert.IsFalse(ac.IsCompletedCredit);
            }

            [TestMethod]
            public void TrueIfStatusIsNonCourseTransferWithPassedEndDate()
            {
                var ac = new AcademicCredit("123");
                ac.Status = CreditStatus.TransferOrNonCourse;
                ac.EndDate = DateTime.Today.Subtract(new TimeSpan(76, 0, 0)); // today minus 3 days
                Assert.IsTrue(ac.IsCompletedCredit);
            }

            [TestMethod]
            public void TrueIfStatusIsNonCourseTransferWithCurrentEndDate()
            {
                var ac = new AcademicCredit("123");
                ac.Status = CreditStatus.TransferOrNonCourse;
                ac.EndDate = DateTime.Today;
                Assert.IsTrue(ac.IsCompletedCredit);
            }

            [TestMethod]
            public void FalseIfStatusIsNonCourseTransferWithFutureEndDate()
            {
                var ac = new AcademicCredit("123");
                ac.Status = CreditStatus.TransferOrNonCourse;
                ac.EndDate = DateTime.Today.AddDays(3);
                ac.VerifiedGrade = null;
                Assert.IsFalse(ac.IsCompletedCredit);
            }

            [TestMethod]
            public void FalseIfStatusIsNonCourseTransferWithNullEndDate()
            {
                var ac = new AcademicCredit("123");
                ac.Status = CreditStatus.TransferOrNonCourse;
                ac.EndDate = null;
                Assert.IsFalse(ac.IsCompletedCredit);
            }

            [TestMethod]
            public void FalseIfNeitherHasVerifiedGradeNorNonCourseTransferOtherwise()
            {
                var ac = new AcademicCredit("123");
                Assert.IsFalse(ac.IsCompletedCredit);
            }

            [TestMethod]
            public void TrueIfNoGradeSchemeAndEndDatePassed()
            {
                var ac = new AcademicCredit("123");
                ac.VerifiedGrade = null;
                ac.GradeSchemeCode = null;
                ac.EndDate = DateTime.Today.Subtract(new TimeSpan(78, 0, 0));
                Assert.IsTrue(ac.IsCompletedCredit);
            }

            [TestMethod]
            public void FalseIfNoGradeSchemeAndEndDateFuture()
            {
                var ac = new AcademicCredit("123");
                ac.VerifiedGrade = null;
                ac.GradeSchemeCode = null;
                ac.EndDate = DateTime.Today.AddDays(3);
                Assert.IsFalse(ac.IsCompletedCredit);
            }

            [TestMethod]
            public void FalseIfNoGradeSchemeAndNullEndDate()
            {
                var ac = new AcademicCredit("123");
                ac.GradeSchemeCode = null;
                ac.EndDate = null;
                Assert.IsFalse(ac.IsCompletedCredit);
            }
        }

        [TestClass]
        public class AcademicCredit_ToString
        {
            [TestMethod]
            public void AcademicCredit_ToString_NullCourse()
            {
                var acString = new AcademicCredit("999").ToString();
                Assert.AreEqual("ACred (noncourse, id=999)", acString);
            }

            [TestMethod]
            public void AcademicCredit_ToString_ValidCourse()
            {
                var course = new TestCourseRepository().GetAsync("21").Result; 
                var acString = new AcademicCredit("999", course, "123").ToString();
                Assert.AreEqual("ACred BIOL*200", acString);
            }
        }

        [TestClass]
        public class StartDateForRules
        {
            [TestMethod]
            public void SameAsStartDate()
            {
                var ac = new AcademicCredit("123") { StartDate = DateTime.Today };
                Assert.AreEqual(ac.StartDate, ac.StartDateForRules);
            }

            [TestMethod]
            public void MinValueIfStartDateNull()
            {
                var ac = new AcademicCredit("123") { StartDate = null };
                Assert.AreEqual(DateTime.MinValue, ac.StartDateForRules);
            }
        }

        [TestMethod]
        public void StudentCourseSectionId_Get_Set()
        {
            var ac = new AcademicCredit("123");
            ac.StudentCourseSectionId = "456";
            Assert.AreEqual("456", ac.StudentCourseSectionId);
        }
    }
}