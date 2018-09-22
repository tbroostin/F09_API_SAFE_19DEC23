/*Copyright 2014-2015 Ellucian Company L.P. and its affiliates.*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ellucian.Colleague.Domain.FinancialAid.Entities;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Ellucian.Colleague.Domain.FinancialAid.Tests.Entities
{
    [TestClass]
    public class FinancialAidApplicationTests
    {
        public class TestApplication : FinancialAidApplication2
        {
            public TestApplication(string id, string awardYear, string studentId)
                : base(id, awardYear, studentId)
            { }
        }

        public string id;
        public string awardYear;
        public string studentId;

        public TestApplication application;

        [TestClass]
        public class FinancialAidApplicationConstructorTests : FinancialAidApplicationTests
        {
            public int? familyContribution;
            public int? institutionalFamilyContribution;
            public bool isFederallyFlagged;
            public bool isInstitutionallyFlagged;

            [TestInitialize]
            public void Initialize()
            {
                id = "1234";
                awardYear = "2014";
                studentId = "0003914";
                familyContribution = 5431;
                institutionalFamilyContribution = 432;
                isFederallyFlagged = true;
                isInstitutionallyFlagged = true;

                application = new TestApplication(id, awardYear, studentId);
            }

            [TestMethod]
            public void IdEqualTest()
            {
                Assert.AreEqual(id, application.Id);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void IdRequiredTest()
            {
                new TestApplication("", awardYear, studentId);
            }

            [TestMethod]
            public void AwardYearEqualTest()
            {
                Assert.AreEqual(awardYear, application.AwardYear);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void AwardYearRequiredTest()
            {
                new TestApplication(id, null, studentId);
            }

            [TestMethod]
            public void StudentIdEqualTest()
            {
                Assert.AreEqual(studentId, application.StudentId);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void StudentIdRequiredTest()
            {
                new TestApplication(id, awardYear, "");
            }

            [TestMethod]
            public void FamilyContributionGetSetTest()
            {
                //initially null
                Assert.IsNull(application.FamilyContribution);

                application.FamilyContribution = familyContribution;
                Assert.AreEqual(familyContribution, application.FamilyContribution);
            }

            [TestMethod]
            public void InstitutionalFamilyContributionGetSetTest()
            {
                //initially null
                Assert.IsNull(application.InstitutionalFamilyContribution);

                application.InstitutionalFamilyContribution = institutionalFamilyContribution;
                Assert.AreEqual(institutionalFamilyContribution, application.InstitutionalFamilyContribution);
            }

            [TestMethod]
            public void IsFederallyFlaggedGetSetTest()
            {
                //initially False
                Assert.IsFalse(application.IsFederallyFlagged);

                application.IsFederallyFlagged = isFederallyFlagged;
                Assert.AreEqual(isFederallyFlagged, application.IsFederallyFlagged);
            }

            [TestMethod]
            public void IsInstitutionallyFlaggedGetSetTest()
            {
                //initially False
                Assert.IsFalse(application.IsInstitutionallyFlagged);

                application.IsInstitutionallyFlagged = isInstitutionallyFlagged;
                Assert.AreEqual(isInstitutionallyFlagged, application.IsInstitutionallyFlagged);
            }
        }

        [TestClass]
        public class FinancialAidApplicationEqualsTest : FinancialAidApplicationTests
        {
            [TestInitialize]
            public void Initialize()
            {
                id = "54321";
                awardYear = "2014";
                studentId = "0003914";

                application = new TestApplication(id, awardYear, studentId);
            }

            [TestMethod]
            public void IdsEqual_EqualTest()
            {
                var testApplication = new TestApplication(id, awardYear, studentId);
                Assert.AreEqual(testApplication, application);
            }

            [TestMethod]
            public void IdsEqual_HashCodesEqualTest()
            {
                var testApplicaiton = new TestApplication(id, awardYear, studentId);
                Assert.AreEqual(testApplicaiton.GetHashCode(), application.GetHashCode());
            }

            [TestMethod]
            public void IdsNotEqual_NotEqualTest()
            {
                var testApplicaiton = new TestApplication("foobar", awardYear, studentId);
                Assert.AreNotEqual(testApplicaiton, application);
            }

            [TestMethod]
            public void IdsNotEqual_HashCodesNotEqualTest()
            {
                var testApplicaiton = new TestApplication("foobar", awardYear, studentId);
                Assert.AreNotEqual(testApplicaiton.GetHashCode(), application.GetHashCode());
            }
        }

        //public string studentId;
        //public StudentAwardYear studentAwardYear;
        //public bool isProfileComplete;
        //public bool isFafsaComplete;

        //public FinancialAidApplication application;

        //public void BaseInitialize()
        //{
        //    studentId = "0003914";
        //    studentAwardYear = new StudentAwardYear(studentId, "2014", new FinancialAidOffice("office"));

        //    application = new FinancialAidApplication(studentId, studentAwardYear);
        //}

        //[TestClass]
        //public class FinancialAidApplicationConstructorTests : FinancialAidApplicationTests
        //{
        //    [TestInitialize]
        //    public void Initialize()
        //    {
        //        BaseInitialize();
        //    }

        //    [TestMethod]
        //    public void AttributesAreSetTest()
        //    {
        //        Assert.AreEqual(studentId, application.StudentId);
        //        Assert.AreEqual(studentAwardYear, application.AwardYear);
        //        Assert.IsFalse(application.IsProfileComplete);
        //        Assert.IsFalse(application.IsFafsaComplete);
        //        Assert.IsFalse(application.IsApplicationReviewed);
        //    }

        //    [TestMethod]
        //    [ExpectedException(typeof(ArgumentNullException))]
        //    public void NullStudentIdThrowsExceptionTest()
        //    {
        //        new FinancialAidApplication(null, studentAwardYear);
        //    }

        //    [TestMethod]
        //    [ExpectedException(typeof(ArgumentNullException))]
        //    public void NullStudentAwardYearThrowsExceptionTest()
        //    {
        //        new FinancialAidApplication(studentId, null);
        //    }

        //    [TestMethod]
        //    public void NoNeedForProfileApplication_CanOnlySetIsProfileCompleteToFalseTest()
        //    {
        //        studentAwardYear.CurrentOffice.AddConfigurationRange(new List<FinancialAid.Entities.FinancialAidConfiguration>()
        //        {
        //            new FinancialAid.Entities.FinancialAidConfiguration("office",studentAwardYear.Code)
        //            {
        //                IsProfileActive = false
        //            }
        //        });

        //        application.IsProfileComplete = true;
        //        Assert.IsFalse(application.IsProfileComplete);
        //    }

        //    [TestMethod]
        //    public void NeedsProfileApplication_CanSetIsProfileCompleteTest()
        //    {
        //        studentAwardYear.CurrentOffice.AddConfigurationRange(new List<FinancialAid.Entities.FinancialAidConfiguration>()
        //        {
        //            new FinancialAid.Entities.FinancialAidConfiguration("office", studentAwardYear.Code)
        //            {
        //                IsProfileActive = true
        //            }
        //        });

        //        application.IsProfileComplete = true;
        //        Assert.IsTrue(application.IsProfileComplete);

        //        application.IsProfileComplete = false;
        //        Assert.IsFalse(application.IsProfileComplete);
        //    }

        //    [TestMethod]
        //    public void IsApplicationReviewedComesFromStudentAwardYearTest()
        //    {
        //        studentAwardYear.IsApplicationReviewed = true;
        //        Assert.IsTrue(application.IsApplicationReviewed);

        //        studentAwardYear.IsApplicationReviewed = false;
        //        Assert.IsFalse(application.IsApplicationReviewed);
        //    }
        //}

        //[TestClass]
        //public class EqualsTest : FinancialAidApplicationTests
        //{
        //    [TestInitialize]
        //    public void Initialize()
        //    {
        //        BaseInitialize();
        //    }

        //    [TestMethod]
        //    public void SameStudentIdAndAwardYear_EqualTest()
        //    {
        //        var testApplication = new FinancialAidApplication(studentId, studentAwardYear);

        //        Assert.AreEqual(testApplication, application);
        //    }

        //    [TestMethod]
        //    public void DiffStudentId_NotEqualTest()
        //    {
        //        var testApplication = new FinancialAidApplication("foobar", studentAwardYear);

        //        Assert.AreNotEqual(testApplication, application);
        //    }

        //    [TestMethod]
        //    public void DiffAwardYear_NotEqualTest()
        //    {
        //        var testApplication = new FinancialAidApplication(studentId, new StudentAwardYear(studentId, "foobar", new FinancialAidOffice("office")));

        //        Assert.AreNotEqual(testApplication, application);
        //    }
        //}
    }
}
