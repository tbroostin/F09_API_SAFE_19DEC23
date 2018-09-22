// Copyright 2017 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ellucian.Colleague.Domain.Student.Entities;
using Ellucian.Web.Http.TestUtil;

namespace Ellucian.Colleague.Domain.Student.Tests.Entities
{
    [TestClass]
    public class CreditFilterTests
    {
        public CreditFilter cf;
        public TestAcademicCreditRepository tacr;
        public TestCourseRepository tcr;
        public AcademicCredit UNDERGRAD_COURSE;
        public AcademicCredit GRAD_COURSE;
        public AcademicCredit NONCOURSE;
        public AcademicCredit CONT_ED_COURSE;
        public AcademicCredit UNGRADED_CREDIT;
        public AcademicCredit WITHDRAWN_GRADED_CREDIT;
        public AcademicCredit WITHDRAWN_UNGRADED_CREDIT;
        public AcademicCredit DROPPED_UNGRADED_CREDIT;
        public AcademicCredit DROPPED_GRADED_CREDIT;


        public Course BIO_PLANNEDCOURSE;
        public Course GR_PLANNEDCOURSE;


        [TestInitialize]
        public void Initialize()
        {
            cf = new CreditFilter();
            tacr = new TestAcademicCreditRepository();
            tcr = new TestCourseRepository();
            UNDERGRAD_COURSE = tacr.GetAsync("1").Result;
            GRAD_COURSE = tacr.GetAsync("57").Result;
            NONCOURSE = tacr.GetAsync("1001").Result;
            CONT_ED_COURSE = tacr.GetAsync("3").Result;
            // In this case the ungraded credit has no grade scheme.
            UNGRADED_CREDIT = tacr.GetAsync("38").Result;

            BIO_PLANNEDCOURSE = tcr.GetAsync("110").Result;
            GR_PLANNEDCOURSE = tcr.GetAsync("9877").Result;
            WITHDRAWN_GRADED_CREDIT = tacr.GetAsync("56").Result;
            WITHDRAWN_UNGRADED_CREDIT = tacr.GetAsync("74").Result;
            DROPPED_UNGRADED_CREDIT = tacr.GetAsync("100").Result;
            DROPPED_GRADED_CREDIT = tacr.GetAsync("101").Result;
        }

        // Acad level, credit

        [TestMethod]
        public void CreditFilterPassesAcadLevel()
        {
            cf.AcademicLevels.Add("UG");
            Assert.AreEqual(true, cf.Passes(UNDERGRAD_COURSE));
        }
        [TestMethod]
        public void CreditFilterBlocksAcadLevel()
        {
            cf.AcademicLevels.Add("UG");
            Assert.AreEqual(false, cf.Passes(GRAD_COURSE));
        }
        [TestMethod]
        public void CreditFilterAcadLevelPassesNoncourse()
        {
            cf.AcademicLevels.Add("UG");
            Assert.AreEqual(true, cf.Passes(NONCOURSE));
        }

        // Acad level, course
        [TestMethod]
        public void CreditFilterPassesAcadLevelCourse()
        {
            cf.AcademicLevels.Add("UG");
            Assert.AreEqual(true, cf.Passes(BIO_PLANNEDCOURSE));
        }
        [TestMethod]
        public void CreditFilterBlocksAcadLevelCourse()
        {
            cf.AcademicLevels.Add("UG");
            Assert.AreEqual(false, cf.Passes(GR_PLANNEDCOURSE));
        }


        // Course level, credit

        [TestMethod]
        public void CreditFilterPassesCourseLevel()
        {
            cf.CourseLevels.Add("100");
            Assert.AreEqual(true, cf.Passes(UNDERGRAD_COURSE));
        }
        [TestMethod]
        public void CreditFilterBlocksCourseLevel()
        {
            cf.CourseLevels.Add("100");
            Assert.AreEqual(false, cf.Passes(GRAD_COURSE));
        }
        [TestMethod]
        public void CreditFilterCourselevelPassesNoncourse()
        {
            cf.CourseLevels.Add("100");
            Assert.AreEqual(true, cf.Passes(NONCOURSE));
        }

        // Course level, course

        [TestMethod]
        public void CreditFilterPassesCourseLevelCourse()
        {
            cf.CourseLevels.Add("100");
            Assert.AreEqual(true, cf.Passes(BIO_PLANNEDCOURSE));
        }
        [TestMethod]
        public void CreditFilterBlocksCourseLevelCourse()
        {
            cf.CourseLevels.Add("100");
            Assert.AreEqual(false, cf.Passes(GR_PLANNEDCOURSE));
        }


        // Credit Type
        [TestMethod]
        public void EmptyCreditTypeFilterPassesAll()
        {
            Assert.IsTrue(cf.CreditTypes.Count() == 0);
            Assert.AreEqual(true, cf.Passes(UNDERGRAD_COURSE));
            Assert.AreEqual(true, cf.Passes(CONT_ED_COURSE));
            Assert.AreEqual(true, cf.Passes(NONCOURSE));
        }
        [TestMethod]
        public void CreditTypeFilterPasses()
        {
            cf.CreditTypes.Add("IN");
            Assert.AreEqual(true, cf.Passes(UNDERGRAD_COURSE));
        }
        [TestMethod]
        public void CreditTypeFilterBlocks()
        {
            cf.CreditTypes.Add("IN");
            Assert.AreEqual(false, cf.Passes(CONT_ED_COURSE));
        }
        [TestMethod]
        public void CreditTypeFilterBlocksNoncourse()
        {
            cf.CreditTypes.Add("IN");
            Assert.AreEqual(false, cf.Passes(NONCOURSE));
        }
        [TestMethod]
        public void CreditTypeFilter_Empty_PassesPlannedCourse()
        {
            // Planned course passes when no credit types specified
            Assert.AreEqual(true, cf.Passes(BIO_PLANNEDCOURSE));
        }
        [TestMethod]
        public void CreditTypeFilter_NonMatching_BlocksPlannedCourse()
        {
            // Planned course does not pass when nonmatching credit types specified.
            cf.CreditTypes.Add("X");
            Assert.AreEqual(false, cf.Passes(BIO_PLANNEDCOURSE));
        }

        [TestMethod]
        public void CreditTypeFilter_Matching_PassesPlannedCourse()
        {
            // Planned course passes when it's credit type is specified.
            cf.CreditTypes.Add("X");
            cf.CreditTypes.Add("IN");
            Assert.AreEqual(true, cf.Passes(BIO_PLANNEDCOURSE));
        }

        // department, credit

        [TestMethod]
        public void DepartmentFilterPasses()
        {
            cf.Departments.Add("HIST");
            Assert.AreEqual(true, cf.Passes(UNDERGRAD_COURSE));
        }
        [TestMethod]
        public void DepartmentFilterBlocks()
        {
            cf.Departments.Add("HIST");
            Assert.AreEqual(false, cf.Passes(CONT_ED_COURSE));
        }
        [TestMethod]
        public void DepartmentFilterPassesNoncourse()
        {
            cf.Departments.Add("HIST");
            Assert.AreEqual(true, cf.Passes(NONCOURSE));
        }
        // department, course
        [TestMethod]
        public void DepartmentFilterPassesCourse()
        {
            cf.Departments.Add("BIOL");
            Assert.AreEqual(true, cf.Passes(BIO_PLANNEDCOURSE));
        }
        [TestMethod]
        public void DepartmentFilterBlocksCourse()
        {
            cf.Departments.Add("BIOL");
            Assert.AreEqual(false, cf.Passes(GR_PLANNEDCOURSE));
        }

        // subject, credit

        [TestMethod]
        public void SubjectFilterPasses()
        {
            cf.Subjects.Add("HIST");
            Assert.AreEqual(true, cf.Passes(UNDERGRAD_COURSE));
        }
        [TestMethod]
        public void SubjectFilterBlocks()
        {
            cf.Subjects.Add("HIST");
            Assert.AreEqual(false, cf.Passes(CONT_ED_COURSE));
        }
        [TestMethod]
        public void SubjectFilterPassesNoncourse()
        {
            cf.Subjects.Add("HIST");
            Assert.AreEqual(true, cf.Passes(NONCOURSE));
        }

        // subject, course

        [TestMethod]
        public void SubjectFilterPassesCourse()
        {
            cf.Subjects.Add("BIOL");
            Assert.AreEqual(true, cf.Passes(BIO_PLANNEDCOURSE));
        }
        [TestMethod]
        public void SubjectFilterBlocksCourse()
        {
            cf.Subjects.Add("BIOL");
            Assert.AreEqual(false, cf.Passes(GR_PLANNEDCOURSE));
        }

        // mark, credit

        [TestMethod]
        public void MarkFilterPasses()
        {
            cf.Marks.Add("G");
            UNDERGRAD_COURSE.Mark = "G";
            Assert.AreEqual(true, cf.Passes(UNDERGRAD_COURSE));
        }
        [TestMethod]
        public void MarkFilterBlocks()
        {
            CONT_ED_COURSE.Mark = "G";
            Assert.AreEqual(false, cf.Passes(CONT_ED_COURSE));
        }
        [TestMethod]
        public void MarkFilterPassesNoncourse()
        {
            cf.Marks.Add("G");
            Assert.AreEqual(true, cf.Passes(NONCOURSE));
        }

        // mark, course

        [TestMethod]
        public void MarkFilterPassesCourse()
        {
            cf.Marks.Add("G");
            Assert.AreEqual(true, cf.Passes(BIO_PLANNEDCOURSE));
        }

        [TestMethod]
        public void IncludeUngraded_PassesUnGradedCredit()
        {
            cf.IncludeNeverGradedCredits = true;
            Assert.AreEqual(true, cf.Passes(UNGRADED_CREDIT));
        }

        [TestMethod]
        public void IncludeUngraded_PassesGradedCredit()
        {
            cf.IncludeNeverGradedCredits = true;
            Assert.AreEqual(true, cf.Passes(UNDERGRAD_COURSE));
        }


        [TestMethod]
        public void ExcludeUngraded_FailsUngradedCredit()
        {
            cf.IncludeNeverGradedCredits = false;
            Assert.AreEqual(false, cf.Passes(UNGRADED_CREDIT));
        }

        [TestMethod]
        public void ExcludeUngraded_PassesGradedCredit()
        {
            cf.IncludeNeverGradedCredits = false;
            // Test an academic credit that does indeed have a grade scheme and make sure it still passes.
            Assert.AreEqual(true, cf.Passes(UNDERGRAD_COURSE));
        }

        [TestMethod]
        public void IsUngradedWithdrawn_True()
        {
            cf.IncludeNeverGradedCredits = true;
            // Test an academic credit that does indeed have a grade scheme and make sure it still passes.
            Assert.IsFalse(cf.Passes(WITHDRAWN_UNGRADED_CREDIT));
        }

        [TestMethod]
        public void IsUngradedWithdrawn_False()
        {
            cf.IncludeNeverGradedCredits = true;
            // Test an academic credit that does indeed have a grade scheme and make sure it still passes.
            Assert.IsTrue(cf.Passes(WITHDRAWN_GRADED_CREDIT));
        }

        //Test for dropped courses
        [TestMethod]
        public void IsUngradedDroppedCourseSelected()
        {
            cf.IncludeNeverGradedCredits = true;
            // Test an academic credit that has ungraded dropped course . It should not pass.
            Assert.IsFalse(cf.Passes(DROPPED_UNGRADED_CREDIT));

        }
        [TestMethod]
        public void IsGradedDroppedCourseSelected()
        {
            cf.IncludeNeverGradedCredits = true;
            // Test an academic credit that has graded dropped course . It should  pass.
            Assert.IsTrue(cf.Passes(DROPPED_GRADED_CREDIT));

        }
    }

}
