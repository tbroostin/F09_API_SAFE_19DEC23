/*Copyright 2015 Ellucian Company L.P. and its affiliates.*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ellucian.Colleague.Data.Base.Tests.Repositories;
using Ellucian.Colleague.Data.FinancialAid.DataContracts;
using Ellucian.Colleague.Data.FinancialAid.Repositories;
using Ellucian.Colleague.Data.FinancialAid.Transactions;
using Ellucian.Colleague.Domain.FinancialAid.Entities;
using Ellucian.Colleague.Domain.FinancialAid.Services;
using Ellucian.Colleague.Domain.FinancialAid.Tests;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Ellucian.Colleague.Data.FinancialAid.Tests.Repositories
{
    /// <summary>
    /// Tests for the repository
    /// </summary>
    [TestClass]
    public class StudentDefaultAwardPeriodRepositoryTests : BaseRepositorySetup
    {
        /// <summary>
        /// Test SaAcyr object with needed properties
        /// </summary>
        private class SaAcyrTest : SaAcyr
        {
            public string AwardYear;
            public List<string> SaTerms;
        }

        private List<SaAcyrTest> saAcyrTestResponseData;
        private IEnumerable<StudentDefaultAwardPeriod> expectedDefaultAwardPeriods;
        private IEnumerable<StudentDefaultAwardPeriod> actualDefaultAwardPeriods;

        private List<StudentAwardYear> activeStudentAwardYears;

        private TestStudentDefaultAwardPeriodRepository expectedRepository;
        private StudentDefaultAwardPeriodRepository actualRepository;

        private TestStudentAwardYearRepository studentAwardYearRepository;
        private TestFinancialAidOfficeRepository officeRepository;
        private CurrentOfficeService currentOfficeService;

        public GetDefaultAwardPeriodsResponse getDefaultAwardPeriodsResponseData;
        public Transactions.GetDefaultAwardPeriodsRequest actualGetRequest;

        private string studentId;

        [TestInitialize]
        public async void Initialize()
        {
            MockInitialize();

            studentAwardYearRepository = new TestStudentAwardYearRepository();
            officeRepository = new TestFinancialAidOfficeRepository();
            currentOfficeService = new CurrentOfficeService(officeRepository.GetFinancialAidOffices());

            studentId = "0004791";

            activeStudentAwardYears = studentAwardYearRepository.GetStudentAwardYears(studentId, currentOfficeService).ToList();
            saAcyrTestResponseData = BuildSaAcyrTestResponseData(studentAwardYearRepository.SaStudentData);

            expectedRepository = new TestStudentDefaultAwardPeriodRepository();
            
            expectedDefaultAwardPeriods = await expectedRepository.GetStudentDefaultAwardPeriodsAsync(studentId, activeStudentAwardYears);
 

            actualRepository =  BuildRepository();
            actualDefaultAwardPeriods = await actualRepository.GetStudentDefaultAwardPeriodsAsync(studentId, activeStudentAwardYears);
        }

        [TestCleanup]
        public void Cleanup()
        {
            transFactoryMock = null;
            dataReaderMock = null;
            cacheProviderMock = null;
            saAcyrTestResponseData = null;
            expectedDefaultAwardPeriods = null;
            actualDefaultAwardPeriods = null;
            activeStudentAwardYears = null;
            expectedRepository = null;
            actualRepository = null;
            studentAwardYearRepository = null;
            officeRepository = null;
            currentOfficeService = null;
            studentId = null;
        }

        private StudentDefaultAwardPeriodRepository BuildRepository()
        {
            getDefaultAwardPeriodsResponseData = new GetDefaultAwardPeriodsResponse() {AwardPeriodsList = new List<string>() {"15/FA","16/WI" }};

            transManagerMock.Setup(t => t.ExecuteAsync<GetDefaultAwardPeriodsRequest, GetDefaultAwardPeriodsResponse>(It.IsAny<GetDefaultAwardPeriodsRequest>())
                ).ReturnsAsync(getDefaultAwardPeriodsResponseData)
                 .Callback<GetDefaultAwardPeriodsRequest>(
                 req =>
                 {
                     actualGetRequest = req;
                 }
                 );
            
            
            
            foreach (var saAcyrTest in saAcyrTestResponseData)
            {
                if (saAcyrTest != null)
                {
                    var saAcyrResponse = new SaAcyr()
                    {
                        SaTerms = new List<string>() {"15/FA", "16/WI"},
                        
                    };
                    var acyrFile = "SA." + saAcyrTest.AwardYear;
                    dataReaderMock.Setup(dr => dr.ReadRecord<SaAcyr>(acyrFile, studentId, true)).Returns(saAcyrResponse);
                }
            }

            return new StudentDefaultAwardPeriodRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object);
        }

        private List<SaAcyrTest> BuildSaAcyrTestResponseData(List<TestStudentAwardYearRepository.SaStudent> saRecords)
        {
            var saAcyrTestList = new List<SaAcyrTest>();
            foreach (var saRecord in saRecords)
            {
                var saAcyrTest = new SaAcyrTest()
                {
                    SaTerms = new List<string>() {"15/FA","16/WI"},
                    AwardYear = saRecord.AwardYear
                };

                saAcyrTestList.Add(saAcyrTest);
            }
            return saAcyrTestList;
        }
   
        [TestMethod]
        public void NumberActualDefaultAwardPeriods_EqualsExpectedNumberTest()
        {
            Assert.AreEqual(expectedDefaultAwardPeriods.Count(), actualDefaultAwardPeriods.Count());
        }

        /// <summary>
        /// Tests if all the properties of all the actual average award package objects
        /// equal the corresponding properties of the expected objects
        /// </summary>
        [TestMethod]
        public void ActualDefaultAwardPeriodProperties_EqualExpectedPropertiesTest()
        {
            foreach (var expectedAwardPeriod in expectedDefaultAwardPeriods)
            {
                var actualAwardPeriod = actualDefaultAwardPeriods.FirstOrDefault(adap => adap.AwardYear == expectedAwardPeriod.AwardYear);
                Assert.IsNotNull(actualAwardPeriod);
                Assert.AreEqual(expectedAwardPeriod.StudentId, actualAwardPeriod.StudentId);
                Assert.AreEqual(expectedAwardPeriod.AwardYear, actualAwardPeriod.AwardYear);
                CollectionAssert.AreEqual(expectedAwardPeriod.DefaultAwardPeriods, actualAwardPeriod.DefaultAwardPeriods);
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
            actualDefaultAwardPeriods = await actualRepository.GetStudentDefaultAwardPeriodsAsync(null, activeStudentAwardYears);
        }

        /// <summary>
        /// Tests if an ArgumentNullException is thrown when
        /// a studentawardYear argument is passed as null
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task NullStudentAwardYearThrowsExceptionTest()
        {
            actualDefaultAwardPeriods = await actualRepository.GetStudentDefaultAwardPeriodsAsync(studentId, null);
        }

    }
}
