//Copyright 2014-2015 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
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
    /// This class provides the setups for the specific test classes of AwardLetterRepository.
    /// This class contains no test methods.
    /// <see cref="AwardLetterRepository_GetAwardLetterTests"/>
    /// <see cref="AwardLetterRepository_GetSingleAwardLetterTests"/>
    /// <see cref="AwardLetterRepository_UpdateAwardLetterTests"/>
    /// </summary>
    [TestClass]
    public class AwardLetterRepositoryTests : BaseRepositorySetup
    {
        /// <summary>
        /// A sub class of CsAcyr data contract containing one extra field, AwardYear.
        /// The award year is need to setup Mock to return particular data for a year.
        /// </summary>
        public class CsAcyrTest : CsAcyr
        {
            public string AwardYear { get; set; }
        }

        /// <summary>
        /// A sub class of YsAcyr data contract containing one extra field, AwardYear.
        /// The award year is need to setup Mock to return particular data for a year.
        /// </summary>
        public class YsAcyrTest : YsAcyr
        {
            public string AwardYear { get; set; }
        }

        /// <summary>
        /// A sub class of EvalAwardLetterParamsRuleTableResponse containing one extra field, AwardYear.
        /// The award year is needed to setup Mock to return particular data for a year
        /// </summary>
        public class EvalAwardLetterParamsResponseTest : Transactions.EvalAwardLetterParamsRuleTableResponse
        {
            public string AwardYear { get; set; }
        }

        //Declare the data contract object collections which will be returned as responses by the mocked datareaders
        public Collection<CsAcyrTest> csAcyrTestResponseData;
        public Collection<YsAcyrTest> ysAcyrTestResponseData;
        public Collection<EvalAwardLetterParamsResponseTest> evalTransactionTestResponseData;
        public FaSysParams faSysParamsResponseData;
        public Collection<AltrParameters> awardLetterParametersResponseData;
        public UpdateAwardLetterResponse updateAwardLetterResponseData;

        //Student Id used throughout
        public string studentId;
        public TestFinancialAidOfficeRepository officeRepository;
        public TestStudentAwardYearRepository studentAwardYearRepository;
        public TestFafsaRepository fafsaRepository;
        public CurrentOfficeService currentOfficeService;

        //Test repositories
        public TestAwardLetterRepository expectedRepository;
        public AwardLetterRepository actualRepository;

        public Transactions.UpdateAwardLetterRequest actualUpdateRequest;



        #region Build DataContracts and Repositories

        public AwardLetterRepository BuildAwardLetterRepository()
        {
            dataReaderMock.Setup<FaSysParams>(fsp => fsp.ReadRecord<FaSysParams>("ST.PARMS", "FA.SYS.PARAMS", true)).Returns(faSysParamsResponseData);
            dataReaderMock.Setup<Collection<AltrParameters>>(p => p.BulkReadRecord<AltrParameters>("", false)).Returns(awardLetterParametersResponseData);

            //returns an empty response since the repository doesn't do anything with it.
            //also capturing the submitted request for testing
            transManagerMock.Setup(t => t.Execute<UpdateAwardLetterRequest, UpdateAwardLetterResponse>(It.IsAny<UpdateAwardLetterRequest>())
                ).Returns(updateAwardLetterResponseData
                ).Callback<UpdateAwardLetterRequest>(
                    req =>
                    {
                        actualUpdateRequest = req;
                    });


            foreach (var csAcyrTest in csAcyrTestResponseData)
            {
                //Convert the CsAcyrTest object into CsAcyr objects
                if (csAcyrTest != null)
                {
                    var csAcyrResponse = new CsAcyr()
                    {
                        Recordkey = csAcyrTest.Recordkey,
                        CsBudgetAdj = csAcyrTest.CsBudgetAdj,
                        CsFc = csAcyrTest.CsFc,
                        CsInstAdj = csAcyrTest.CsInstAdj,
                        CsLocation = csAcyrTest.CsLocation,
                        CsNeed = csAcyrTest.CsNeed,
                        CsStdTotalExpenses = csAcyrTest.CsStdTotalExpenses,
                        CsFedIsirId = csAcyrTest.CsFedIsirId
                    };

                    //Mock the record read
                    string acyrFile = "CS." + csAcyrTest.AwardYear;
                    dataReaderMock.Setup<CsAcyr>(cs => cs.ReadRecord<CsAcyr>(acyrFile, studentId, true)).Returns(csAcyrResponse);
                }
            }

            foreach (var ysAcyrTest in ysAcyrTestResponseData)
            {
                //Convert the YsAcyrTest object into YsAcyr object
                if (ysAcyrTest != null)
                {
                    var ysAcyrResponse = new YsAcyr()
                    {
                        Recordkey = ysAcyrTest.Recordkey
                    };

                    //Mock the record read
                    string acyrFile = "YS." + ysAcyrTest.AwardYear;
                    dataReaderMock.Setup(dr => dr.ReadRecord<YsAcyr>(acyrFile, studentId, true)).Returns(ysAcyrResponse);
                }
            }

            transManagerMock.Setup<EvalAwardLetterParamsRuleTableResponse>(m =>
                m.Execute<EvalAwardLetterParamsRuleTableRequest, EvalAwardLetterParamsRuleTableResponse>(
                    It.IsAny<EvalAwardLetterParamsRuleTableRequest>())
                ).Returns<EvalAwardLetterParamsRuleTableRequest>(
                    (req) =>
                    {
                        var testResponse = evalTransactionTestResponseData.FirstOrDefault(tt => tt != null && tt.AwardYear == req.Year);
                        if (testResponse == null) return null;
                        return new EvalAwardLetterParamsRuleTableResponse()
                        {
                            LogMessages = testResponse.LogMessages,
                            Result = testResponse.Result
                        };
                    });

            //Return the mocked repository
            return new AwardLetterRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object);
        }

        public Collection<AltrParameters> BuildAwardLetterParametersResponseData(List<TestAwardLetterRepository.AwardLetterParameter> parameterDataList)
        {
            var parametersCollection = new Collection<AltrParameters>();
            foreach (var parameterData in parameterDataList)
            {
                var parameter = new AltrParameters()
                {
                    Recordkey = parameterData.Id,
                    AltrIntroText = parameterData.OpeningParagraph,
                    AltrClosingText = parameterData.ClosingParagraph,
                    AltrNeedBlock = (parameterData.IsNeedBlockActive) ? "Y" : "N",
                    AltrOfficeBlock = (parameterData.IsOfficeBlockActive) ? "Y" : "N",
                    AltrHousingCode = (parameterData.IsHousingCodeActive) ? "Y" : "N",
                    AltrAwdPerColumn1 = parameterData.AwardPeriodsGroup1,
                    AltrAwdPerColumn2 = parameterData.AwardPeriodsGroup2,
                    AltrAwdPerColumn3 = parameterData.AwardPeriodsGroup3,
                    AltrAwdPerColumn4 = parameterData.AwardPeriodsGroup4,
                    AltrAwdPerColumn5 = parameterData.AwardPeriodsGroup5,
                    AltrAwdPerColumn6 = parameterData.AwardPeriodsGroup6,
                    AltrTitleColumn1 = parameterData.AwardPeriodGroup1Title,
                    AltrTitleColumn2 = parameterData.AwardPeriodGroup2Title,
                    AltrTitleColumn3 = parameterData.AwardPeriodGroup3Title,
                    AltrTitleColumn4 = parameterData.AwardPeriodGroup4Title,
                    AltrTitleColumn5 = parameterData.AwardPeriodGroup5Title,
                    AltrTitleColumn6 = parameterData.AwardPeriodGroup6Title,
                    AltrCategoryGroup1 = parameterData.AwardCategoriesGroup1,
                    AltrCategoryGroup2 = parameterData.AwardCategoriesGroup2,
                    AltrTitleGroup1 = parameterData.AwardCategoryGroup1Title,
                    AltrTitleGroup2 = parameterData.AwardCategoryGroup2Title,
                    AltrTitleGroup3 = parameterData.AwardCategoryGroup3Title,
                    AltrTitleAwdName = parameterData.AwardColumnTitle,
                    AltrTitleAwdTotal = parameterData.TotalColumnTitle
                };
                parametersCollection.Add(parameter);
            }
            return parametersCollection;
        }

        public FaSysParams BuildSystemParametersResponseData(TestAwardLetterRepository.SystemParameters systemParameters)
        {
            return new FaSysParams()
            {
                Recordkey = "FA.SYS.PARAMS",
                FspInstitutionAddress = systemParameters.Address,
                FspInstitutionCsz = systemParameters.CityStateZip,
                FspInstitutionName = systemParameters.InstitutionName,
                FspPellPhoneNumber = systemParameters.PhoneNumber,
                FspTitleIvCode = systemParameters.TitleIVCode
            };
        }

        public Collection<EvalAwardLetterParamsResponseTest> BuildTransactionTestResponseData(List<TestAwardLetterRepository.AwardLetterParamsTransaction> transactionData)
        {
            var transactionTestCollection = new Collection<EvalAwardLetterParamsResponseTest>();

            foreach (var transaction in transactionData)
            {
                var transactionTest = new EvalAwardLetterParamsResponseTest()
                {
                    AwardYear = transaction.Year,
                    LogMessages = transaction.LogMessages,
                    Result = transaction.Result
                };
                transactionTestCollection.Add(transactionTest);
            }
            return transactionTestCollection;
        }

        public Collection<YsAcyrTest> BuildYsAcyrTestResponseData(List<TestAwardLetterRepository.StudentYsYear> ysAcyrTestData)
        {
            var ysAcyrCollection = new Collection<YsAcyrTest>();

            foreach (var testDataObj in ysAcyrTestData)
            {
                var ysAcyrTest = new YsAcyrTest()
                {
                    Recordkey = studentId,
                    AwardYear = testDataObj.awardYear
                };
                ysAcyrCollection.Add(ysAcyrTest);
            }

            return ysAcyrCollection;
        }

        public Collection<CsAcyrTest> BuildCsAcyrTestResponseData(List<TestAwardLetterRepository.StudentCsYear> csAcyrTestData)
        {
            var csAcyrTestCollection = new Collection<CsAcyrTest>();

            foreach (var testDataObj in csAcyrTestData)
            {
                var csAcyrTest = new CsAcyrTest()
                {
                    Recordkey = studentId,
                    AwardYear = testDataObj.AwardYear,
                    CsBudgetAdj = testDataObj.BudgetAdjustment,
                    CsFc = testDataObj.Efc.ToString(),
                    CsInstAdj = testDataObj.InstitutionAdjustment,
                    CsNeed = testDataObj.Need,
                    CsStdTotalExpenses = testDataObj.TotalExpenses,
                    CsFedIsirId = testDataObj.IsirFedId.ToString()
                };
                csAcyrTestCollection.Add(csAcyrTest);
            }
            return csAcyrTestCollection;
        }

        #endregion


    }
}
