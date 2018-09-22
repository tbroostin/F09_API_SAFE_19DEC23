using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ellucian.Colleague.Data.Base.Tests.Repositories;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ellucian.Colleague.Data.FinancialAid.DataContracts;
using System.Collections.ObjectModel;
using Ellucian.Colleague.Domain.FinancialAid.Entities;
using Ellucian.Colleague.Data.FinancialAid.Repositories;
using Ellucian.Colleague.Domain.FinancialAid.Tests;

namespace Ellucian.Colleague.Data.FinancialAid.Tests.Repositories
{
    /// <summary>
    /// Test Class for StudentAwardPeriodRepository
    /// </summary>
    [TestClass]
    public class StudentAwardPeriodRepositoryTests 
    {

        /// <summary>
        /// Test Class for the Get Method of StudentAwardPeriodRepository
        /// </summary>
        [TestClass]
        public class GetTests : BaseRepositorySetup
        {
            /// <summary>
            /// A sub-class of TaAcyr containing one extra field, AwardYear
            /// </summary>
            public class TaAcyrTest : TaAcyr
            {
                public string AwardYear { get; set; }
            }

            /// <summary>
            /// List of TaAcyr DataContract objects, built from allStudentAwardPeriods, by BuildStudentAwardPeriodsResponse
            /// </summary>
            Collection<TaAcyrTest> taAcyrTestResponseData;

            /// <summary>
            /// FinAid DataContract objects, built from allStudentAwardPeriods, by BuildStudentAwardPeriodResponse
            /// </summary>
            FinAid finAidResponseData;

            /// <summary>
            /// List of StudentAwardPeriod domain objects, built by the TestStudentAwardPeriodRepository
            /// </summary>
            IEnumerable<StudentAwardPeriod> allStudentAwardPeriods;

            /// <summary>
            /// Valid mocked StudentAwardPeriodRepository
            /// </summary>
            StudentAwardPeriodRepository studentAwardPeriodRepo;

            string studentId;

            [TestInitialize]
            public void Initialize()
            {
                MockInitialize();

                studentId = "0003914";

                //Enumerable list of all StudentAwardPeriods
                allStudentAwardPeriods = new TestStudentAwardPeriodRepository().Get(studentId);
                finAidResponseData = BuildFinAidResponse(allStudentAwardPeriods);
                taAcyrTestResponseData = BuildTaAcyrResponse(allStudentAwardPeriods);

                studentAwardPeriodRepo = BuildValidStudentAwardPeriodRepository(studentId);
            }

            [TestCleanup]
            public void Cleanup()
            {
                transFactoryMock = null;
                localCacheMock = null;
                cacheProviderMock = null;
                dataReaderMock = null;
                loggerMock = null;
                transManagerMock = null;
            }

            /// <summary>
            /// Tests that all studentawardperiods are returned by the get for each year
            /// </summary>
            [TestMethod]
            public void GetAllStudentAwardPeriods()
            {
                var studentAwardPeriods = studentAwardPeriodRepo.Get(studentId);
                Assert.IsTrue(studentAwardPeriods.Count() >= 16);
            }

            /// <summary>
            /// Asserts that the records are equal if the amount field matches with the selected 
            /// </summary>
            [TestMethod]
            public void AwardAmount()
            {
                var studentAwardPeriods = studentAwardPeriodRepo.Get(studentId);
                foreach (var sap in studentAwardPeriods)
                {
                    Assert.AreEqual(sap.AwardAmount, allStudentAwardPeriods.Where(
                        a => (a.StudentId == sap.StudentId) &&
                            (a.AwardYear == sap.AwardYear) &&
                            (a.AwardId == sap.AwardId) &&
                            (a.AwardPeriod == sap.AwardPeriod) &&
                            (a.AwardStatus == sap.AwardStatus)
                        ).First().AwardAmount);
                }
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public void StudentNotFoundThrowsException()
            {
                studentAwardPeriodRepo.Get("foobar");
                

            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void NullStudentException()
            {
                studentAwardPeriodRepo.Get(null);
            }

            private StudentAwardPeriodRepository BuildValidStudentAwardPeriodRepository(string studentId)
            {
                //set up the ReadRecord method to return the test FinAid data contract objects
                dataReaderMock.Setup<FinAid>(fa => fa.ReadRecord<FinAid>("FIN.AID", studentId, true)).Returns(finAidResponseData);

                foreach (var year in finAidResponseData.FaSaYears)
                {
                    //extract the TaAcyrTest objects with the current award year
                    var responseData = taAcyrTestResponseData.Where(ta => ta.AwardYear == year);

                    //build a list of TaAcyr Data Contract objects using the extracted TaAcyrTest objects
                    var taAcyrResponseData = new Collection<TaAcyr>();
                    foreach (var ta in responseData)
                    {
                        var TaAcyrData = new TaAcyr();
                        TaAcyrData.Recordkey = ta.Recordkey;
                        TaAcyrData.TaTermAmount = ta.TaTermAmount;
                        TaAcyrData.TaTermAction = ta.TaTermAction;

                        taAcyrResponseData.Add(TaAcyrData);
                    }

                    //Setup the BuildRecordRead method to return the list of TaAcyr Data Contract objects
                    string acyrFile = "TA." + year;
                    string criteria = "WITH TA.STUDENT.ID EQ '" + studentId + "'";
                    dataReaderMock.Setup<Collection<TaAcyr>>(acyr => acyr.BulkReadRecord<TaAcyr>(acyrFile, criteria, true)).Returns(taAcyrResponseData);

                }

                return new StudentAwardPeriodRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object);
            }

            /// <summary>
            /// Return a FinAid DataContract object. Loop through allStudentAwardPeriods to find the list of
            /// years for FaSaYears. 
            /// </summary>
            /// <param name="allStudentAwardPeriods">StudentAwardPeriod Domain objects. Place all of the unique AwardYears in a list.</param>
            /// <returns></returns>
            private FinAid BuildFinAidResponse(IEnumerable<StudentAwardPeriod> allStudentAwardPeriods)
            {
                var faSaYears = allStudentAwardPeriods.Select(a => a.AwardYear).Distinct();

                var finAidData = new FinAid();
                finAidData.Recordkey = allStudentAwardPeriods.First().StudentId;
                finAidData.FaSaYears = faSaYears.ToList();

                return finAidData;
            }

            /// <summary>
            /// Return a collection of TaAcyr DataContract objects
            /// </summary>
            /// <param name="allStudentAwardPeriods"></param>
            /// <returns>TA.ACYR test records</returns>
            private Collection<TaAcyrTest> BuildTaAcyrResponse(IEnumerable<StudentAwardPeriod> allStudentAwardPeriods)
            {
                Collection<TaAcyrTest> repoTaAcyrTest = new Collection<TaAcyrTest>();

                foreach (var sap in allStudentAwardPeriods)
                {
                    var TaAcyrTestData = new TaAcyrTest();
                    TaAcyrTestData.Recordkey = sap.StudentId + "*" + sap.AwardId + "*" + sap.AwardPeriod;
                    TaAcyrTestData.TaTermAmount = sap.AwardAmount;
                    TaAcyrTestData.AwardYear = sap.AwardYear;
                    TaAcyrTestData.TaTermAction = sap.AwardStatus;

                    repoTaAcyrTest.Add(TaAcyrTestData);
                }
                return repoTaAcyrTest;
            }
        }
    }
}
