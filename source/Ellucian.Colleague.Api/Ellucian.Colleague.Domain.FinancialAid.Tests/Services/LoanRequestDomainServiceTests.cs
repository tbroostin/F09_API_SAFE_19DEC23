//Copyright 2014 Ellucian Company L.P. and its affiliates
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ellucian.Colleague.Domain.Base.Entities;
using Ellucian.Colleague.Domain.Base.Exceptions;
using Ellucian.Colleague.Domain.FinancialAid.Entities;
using Ellucian.Colleague.Domain.FinancialAid.Services;
using Ellucian.Colleague.Domain.Student.Tests;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Ellucian.Colleague.Domain.FinancialAid.Tests.Services
{
    [TestClass]
    public class LoanRequestDomainServiceTests
    {
        [TestClass]
        public class GetLoanRequestAssignmentTests
        {
            private Domain.Student.Entities.Student studentEntity;
            private Domain.Student.Entities.Applicant applicantEntity;

            private TestFinancialAidOfficeRepository testFinancialAidOfficeRepository;
            private CurrentOfficeService currentOfficeService;
            private TestStudentAwardYearRepository testStudentAwardYearRepository;

            private StudentAwardYear activeStudentAwardYear;

            [TestInitialize]
            public void Initialize()
            {
                studentEntity = new Student.Entities.Student("1234567", "foo", null, new List<string>(), new List<string>())
                {
                    FinancialAidCounselorId = "1111111"
                };
                applicantEntity = new Student.Entities.Applicant("7654321", "bar")
                {
                    FinancialAidCounselorId = "2222222"
                };

                testFinancialAidOfficeRepository = new TestFinancialAidOfficeRepository();
                currentOfficeService = new CurrentOfficeService(testFinancialAidOfficeRepository.GetFinancialAidOffices());
                testStudentAwardYearRepository = new TestStudentAwardYearRepository();

                activeStudentAwardYear = testStudentAwardYearRepository.GetStudentAwardYears(studentEntity.Id, currentOfficeService).Where(y => y.IsActive).First();
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void NullPersonBasedObjectThrowsExceptionTest()
            {
                LoanRequestDomainService.GetLoanRequestAssignment(null, activeStudentAwardYear);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public void AwardYearsAssignedToDifferentPerson_ThrowsExceptionTest()
            {
                activeStudentAwardYear = testStudentAwardYearRepository.GetStudentAwardYears("foobar", currentOfficeService).Where(y => y.IsActive).First();
                LoanRequestDomainService.GetLoanRequestAssignment(studentEntity, activeStudentAwardYear);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void NullAwardYearThrowsExceptionTest()
            {
                LoanRequestDomainService.GetLoanRequestAssignment(studentEntity, null);
            }

            [TestMethod]
            [ExpectedException(typeof(InvalidOperationException))]
            public void CanRequestLoanIsInactiveExceptionTest()
            {
                activeStudentAwardYear.CurrentConfiguration.AreLoanRequestsAllowed = false;
                LoanRequestDomainService.GetLoanRequestAssignment(studentEntity, activeStudentAwardYear);
            }

            [TestMethod]
            [ExpectedException(typeof(ExistingResourceException))]
            public void ExistingPendingLoanRequestThrowsExceptionTest()
            {
                activeStudentAwardYear.PendingLoanRequestId = "foo";

                LoanRequestDomainService.GetLoanRequestAssignment(studentEntity, activeStudentAwardYear);
            }

            [TestMethod]
            public void ReturnStudentCounselorIdTest()
            {
                //var activeAwardYears = testStudentAwardYearRepository.GetStudentAwardYears(studentEntity.Id, currentOfficeService).Where(y => y.IsActive);
                activeStudentAwardYear.PendingLoanRequestId = string.Empty;
                //var awardYear = activeAwardYears.First().Code;
                var assignedToId = LoanRequestDomainService.GetLoanRequestAssignment(studentEntity, activeStudentAwardYear);

                Assert.AreEqual(studentEntity.FinancialAidCounselorId, assignedToId);
            }

            [TestMethod]
            public void ReturnApplicantCounselorIdTest()
            {
                activeStudentAwardYear = testStudentAwardYearRepository.GetStudentAwardYears(applicantEntity.Id, currentOfficeService).Where(y => y.IsActive).First();
                activeStudentAwardYear.PendingLoanRequestId = string.Empty;
                var assignedToId = LoanRequestDomainService.GetLoanRequestAssignment(applicantEntity, activeStudentAwardYear);

                Assert.AreEqual(applicantEntity.FinancialAidCounselorId, assignedToId);
            }

            [TestMethod]
            [ExpectedException(typeof(InvalidOperationException))]
            public void NonStudentOrApplicantObjectThrowsExceptionTest()
            {
                var staffEntity = new Staff("foo", "bar");

                activeStudentAwardYear = testStudentAwardYearRepository.GetStudentAwardYears(staffEntity.Id, currentOfficeService).Where(y => y.IsActive).First();
                activeStudentAwardYear.PendingLoanRequestId = string.Empty;

                LoanRequestDomainService.GetLoanRequestAssignment(staffEntity, activeStudentAwardYear);
            }

            [TestMethod]
            public void NoCounselorForStudentUseEmptyCounselorIdTest()
            {
                studentEntity.FinancialAidCounselorId = string.Empty;

                activeStudentAwardYear.PendingLoanRequestId = string.Empty;
                var assignedToId = LoanRequestDomainService.GetLoanRequestAssignment(studentEntity, activeStudentAwardYear);

                Assert.AreEqual(string.Empty, assignedToId);
            }

            [TestMethod]
            public void NoCounselorForApplicantUseEmptyCounselorIdTest()
            {
                applicantEntity.FinancialAidCounselorId = string.Empty;

                activeStudentAwardYear = testStudentAwardYearRepository.GetStudentAwardYears(applicantEntity.Id, currentOfficeService).Where(y => y.IsActive).First();
                activeStudentAwardYear.PendingLoanRequestId = string.Empty;
                var assignedToId = LoanRequestDomainService.GetLoanRequestAssignment(applicantEntity, activeStudentAwardYear);

                Assert.AreEqual(string.Empty, assignedToId);
            }
        }
    }
}

