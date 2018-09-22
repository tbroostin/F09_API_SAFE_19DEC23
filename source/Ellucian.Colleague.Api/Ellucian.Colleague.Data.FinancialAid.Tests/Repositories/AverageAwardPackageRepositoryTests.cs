/*Copyright 2014-2017 Ellucian Company L.P. and its affiliates.*/
using Ellucian.Colleague.Data.Base.Tests.Repositories;
using Ellucian.Colleague.Data.FinancialAid.DataContracts;
using Ellucian.Colleague.Data.FinancialAid.Repositories;
using Ellucian.Colleague.Data.FinancialAid.Transactions;
using Ellucian.Colleague.Domain.FinancialAid.Entities;
using Ellucian.Colleague.Domain.FinancialAid.Services;
using Ellucian.Colleague.Domain.FinancialAid.Tests;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Data.FinancialAid.Tests.Repositories
{
    /// <summary>
    /// Test class for AverageAwardPackageRepository
    /// </summary>
    [TestClass]
    public class AverageAwardPackageRepositoryTests : BaseRepositorySetup
    {
        /// <summary>
        /// Test CsAcyr object with needed properties
        /// </summary>
        private class CsAcyrTest : CsAcyr
        {
            public string AwardYear;
        }

        private List<CsAcyrTest> csAcyrTestResponseData;
        private List<IsirFafsa> isirFafsaTestResponseData;
        private IEnumerable<AverageAwardPackage> expectedAverageAwardPackages;
        private IEnumerable<AverageAwardPackage> actualAverageAwardPackages;

        private List<StudentAwardYear> activeStudentAwardYears;

        private TestAverageAwardPackageRepository expectedRepository;
        private AverageAwardPackageRepository actualRepository;

        private TestStudentAwardYearRepository studentAwardYearRepository;
        private TestFinancialAidOfficeRepository officeRepository;
        private CurrentOfficeService currentOfficeService;

        private GetGraduateLevelResponse getGraduateLevelResponseTransaction;
        private GetGraduateLevelRequest actualGraduateLevelRequestTransaction;

        private string studentId;

        #region Initialize and Cleanup

        [TestInitialize]
        public async void Initialize()
        {
            MockInitialize();

            studentAwardYearRepository = new TestStudentAwardYearRepository();
            officeRepository = new TestFinancialAidOfficeRepository();
            currentOfficeService = new CurrentOfficeService(officeRepository.GetFinancialAidOffices());

            studentId = "0004791";

            activeStudentAwardYears = studentAwardYearRepository.GetStudentAwardYears(studentId, currentOfficeService).ToList();
            csAcyrTestResponseData = BuildCsAcyrTestResponseData(studentAwardYearRepository.CsStudentData);

            expectedRepository = new TestAverageAwardPackageRepository();
            isirFafsaTestResponseData = BuildIsirFafsaTestResponseData(expectedRepository.isirFafsaData);
            expectedAverageAwardPackages = expectedRepository.GetAverageAwardPackagesAsync(studentId, activeStudentAwardYears).Result;
            var graduateLevelResponseData = expectedRepository.GraduateLevelTransactionResponseData;

            getGraduateLevelResponseTransaction = new GetGraduateLevelResponse()
            {
                GraduateLevel = graduateLevelResponseData.GraduateLevel,
                ErrorMessage = graduateLevelResponseData.ErrorMessage
            };


            actualRepository = BuildRepositoryAsync();
            actualAverageAwardPackages = await actualRepository.GetAverageAwardPackagesAsync(studentId, activeStudentAwardYears);
        }


        [TestCleanup]
        public void Cleanup()
        {
            transFactoryMock = null;
            dataReaderMock = null;
            cacheProviderMock = null;
            csAcyrTestResponseData = null;
            expectedAverageAwardPackages = null;
            actualAverageAwardPackages = null;
            activeStudentAwardYears = null;
            expectedRepository = null;
            actualRepository = null;
            studentAwardYearRepository = null;
            officeRepository = null;
            currentOfficeService = null;
            studentId = null;
        }
        #endregion

        #region Build Repository and Responses

        private AverageAwardPackageRepository BuildRepositoryAsync()
        {
            foreach (var csAcyrTest in csAcyrTestResponseData)
            {
                if (csAcyrTest != null)
                {
                    var csAcyrResponse = new CsAcyr()
                    {
                        CsFedIsirId = csAcyrTest.CsFedIsirId
                    };
                    var acyrFile = "CS." + csAcyrTest.AwardYear;
                    dataReaderMock.Setup(dr => dr.ReadRecordAsync<CsAcyr>(acyrFile, studentId, true)).ReturnsAsync(csAcyrResponse);
                }
            }

            foreach (var isirFafsaRecord in isirFafsaTestResponseData)
            {
                if (isirFafsaRecord != null)
                {
                    var isirKey = isirFafsaRecord.Recordkey;
                    dataReaderMock.Setup(dr => dr.ReadRecordAsync<IsirFafsa>(isirKey, true)).ReturnsAsync(isirFafsaRecord);
                }
            }

            transManagerMock.Setup(t =>
                t.ExecuteAsync<GetGraduateLevelRequest, GetGraduateLevelResponse>(It.IsAny<GetGraduateLevelRequest>()))
                .ReturnsAsync(getGraduateLevelResponseTransaction)
                .Callback<GetGraduateLevelRequest>(r =>
                    actualGraduateLevelRequestTransaction = r);

            return new AverageAwardPackageRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object);
        }


        private List<CsAcyrTest> BuildCsAcyrTestResponseData(List<TestStudentAwardYearRepository.CsStudent> csRecords)
        {
            var csAcyrTestList = new List<CsAcyrTest>();
            foreach (var csRecord in csRecords)
            {
                var csAcyrTest = new CsAcyrTest()
                {
                    CsFedIsirId = csRecord.FedIsirId,
                    AwardYear = csRecord.AwardYear
                };

                csAcyrTestList.Add(csAcyrTest);
            }
            return csAcyrTestList;
        }

        private List<IsirFafsa> BuildIsirFafsaTestResponseData(List<TestAverageAwardPackageRepository.IsirFafsaRecord> isirFafsaRecords)
        {
            var isirFafsaList = new List<IsirFafsa>();
            foreach (var isirFafsa in isirFafsaRecords)
            {
                var isirFafsaRecord = new IsirFafsa()
                {
                    Recordkey = isirFafsa.Id,
                    IfafIsirType = isirFafsa.Type,
                    IfafGradProf = isirFafsa.AttendingGradSchool
                };

                isirFafsaList.Add(isirFafsaRecord);
            }
            return isirFafsaList;
        }

        #endregion

        #region GetAverageAwardPackages Tests

        /// <summary>
        /// Tests if the number of the actual average award package objects equals the
        /// expected number of average award packages
        /// </summary>
        [TestMethod]
        public void NumberActualAverageAwardPackages_EqualsExpectedNumberTest()
        {
            Assert.AreEqual(expectedAverageAwardPackages.Count(), actualAverageAwardPackages.Count());
        }

        /// <summary>
        /// Tests if all the properties of all the actual average award package objects
        /// equal the corresponding properties of the expected objects
        /// </summary>
        [TestMethod]
        public void ActualAwardPackageProperties_EqualExpectedPropertiesTest()
        {
            foreach (var expectedPackage in expectedAverageAwardPackages)
            {
                var actualPackage = actualAverageAwardPackages.FirstOrDefault(aap => aap.AwardYearCode == expectedPackage.AwardYearCode);
                Assert.IsNotNull(actualPackage);
                Assert.AreEqual(expectedPackage.AverageGrantAmount, actualPackage.AverageGrantAmount);
                Assert.AreEqual(expectedPackage.AverageLoanAmount, actualPackage.AverageLoanAmount);
                Assert.AreEqual(expectedPackage.AverageScholarshipAmount, actualPackage.AverageScholarshipAmount);
            }
        }

        /// <summary>
        /// Tests if an ArgumentNullException is thrown when
        /// a studentId argument is passed as null
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task NullStudentIdThrowsExceptionTest()
        {
            await actualRepository.GetAverageAwardPackagesAsync(null, activeStudentAwardYears);
        }

        /// <summary>
        /// Tests if an ArgumentNullException is thrown when
        /// a studentId argument is passed as an empty string
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task EmptyStringStudentIdThrowsExceptionTest()
        {
            await actualRepository.GetAverageAwardPackagesAsync("", activeStudentAwardYears);
        }

        /// <summary>
        /// Test if an ArgumentNullException is thrown when a studentAwardYearList argument
        /// is null
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task NullStudentAwardYearsListThrowsExceptionTest()
        {
            await actualRepository.GetAverageAwardPackagesAsync(studentId, null);
        }

        /// <summary>
        /// Test if an ArgumentNullException is thrown when s studentAwardYearList argument
        /// is empty
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task EmptyStudentAwardYearsListThrowsExceptionTest()
        {
            await actualRepository.GetAverageAwardPackagesAsync(studentId, new List<StudentAwardYear>());
        }

        /// <summary>
        /// Tests if undegraduate packages are returned if the student's level is undegrad
        /// </summary>
        [TestMethod]
        public async Task UndegraduatePackagesReturnedTest()
        {
            getGraduateLevelResponseTransaction = new GetGraduateLevelResponse() { GraduateLevel = "N" };
            actualRepository = BuildRepositoryAsync();
            actualAverageAwardPackages = await actualRepository.GetAverageAwardPackagesAsync(studentId, activeStudentAwardYears);
            foreach (var expectedYear in activeStudentAwardYears)
            {
                var averageUndegraduateAwardPackage = expectedYear.CurrentConfiguration.UndergraduatePackage;

                var actualAverageAwardPackage = actualAverageAwardPackages.FirstOrDefault(aap => aap.AwardYearCode == expectedYear.Code);
                Assert.IsNotNull(actualAverageAwardPackage);
                Assert.AreEqual(averageUndegraduateAwardPackage.AverageGrantAmount, actualAverageAwardPackage.AverageGrantAmount);
                Assert.AreEqual(averageUndegraduateAwardPackage.AverageLoanAmount, actualAverageAwardPackage.AverageLoanAmount);
                Assert.AreEqual(averageUndegraduateAwardPackage.AverageScholarshipAmount, actualAverageAwardPackage.AverageScholarshipAmount);

            }
        }

        /// <summary>
        /// Tests if we receive average award packages even if the response.GraduateLevel is empty
        /// </summary>
        [TestMethod]
        public async Task EmptyResponseGraduateLevel_ReturnsAverageAwardPackagesTest()
        {
            getGraduateLevelResponseTransaction = new GetGraduateLevelResponse() { GraduateLevel = "" };
            actualRepository = BuildRepositoryAsync();
            actualAverageAwardPackages = await actualRepository.GetAverageAwardPackagesAsync(studentId, activeStudentAwardYears);
            Assert.IsTrue(actualAverageAwardPackages.Count() > 0);
            foreach (var expectedYear in activeStudentAwardYears)
            {
                Assert.IsNotNull(actualAverageAwardPackages.FirstOrDefault(aap => aap.AwardYearCode == expectedYear.Code));
            }
        }

        /// <summary>
        /// Tests if an appropriate message is logged when a null csDataRecord
        /// is returned from the repository
        /// </summary>
        [TestMethod]
        public async Task NullCsDataRecord_MessageLoggedTest()
        {
            actualRepository = BuildRepositoryAsync();

            var awardYearNoAveragePackage = activeStudentAwardYears.First().Code;
            var csAcyrFile = "CS." + awardYearNoAveragePackage;
            CsAcyr acyrFile = null;
            dataReaderMock.Setup(dr => dr.ReadRecordAsync<CsAcyr>(csAcyrFile, studentId, true)).ReturnsAsync(acyrFile);
            actualAverageAwardPackages = await actualRepository.GetAverageAwardPackagesAsync(studentId, activeStudentAwardYears);

            loggerMock.Verify(l => l.Info(string.Format("Student {0} has no {1} record", studentId, csAcyrFile)));
        }

        /// <summary>
        /// Tests if appropriate message is logged when a year/ years
        /// have null currentConfiguration(s)
        /// </summary>
        [TestMethod]
        public async Task NullConfiguration_MessageLoggedTest()
        {
            var office = new FinancialAidOffice("office") { IsDefault = true };
            activeStudentAwardYears = studentAwardYearRepository.GetStudentAwardYears(studentId, new CurrentOfficeService(new List<FinancialAidOffice>() { office })).ToList();

            await actualRepository.GetAverageAwardPackagesAsync(studentId, activeStudentAwardYears);

            foreach (var studentAwardYear in activeStudentAwardYears)
            {
                loggerMock.Verify(l => l.Info(string.Format("StudentAwardYear has no configuration for student id {0}, awardYear {1}. Cannot retrieve average award package.", studentId, studentAwardYear.Code)));
            }
        }

        /// <summary>
        /// Tests if the student id in the graduateLevelRequest equals the expected
        /// student id
        /// </summary>
        [TestMethod]
        public void ActualGradLevelRequestStudentId_EqualsExpectedStudentIdTest()
        {
            Assert.AreEqual(studentId, actualGraduateLevelRequestTransaction.StudentId);
        }

        #endregion
    }
}
