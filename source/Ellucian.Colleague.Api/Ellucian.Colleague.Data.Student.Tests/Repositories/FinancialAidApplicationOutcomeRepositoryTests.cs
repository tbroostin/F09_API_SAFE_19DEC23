//Copyright 2017-2020 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Data.Base.Tests.Repositories;
using Ellucian.Colleague.Data.Student.DataContracts;
using Ellucian.Colleague.Data.Student.Repositories;
using Ellucian.Data.Colleague;
using Ellucian.Web.Http.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Ellucian.Colleague.Domain.Base.Transactions;
using Ellucian.Colleague.Domain.Exceptions;

namespace Ellucian.Colleague.Data.Student.Tests.Repositories
{
    [TestClass]
    public class FinancialAidApplicationOutcomeRepositoryTests : BaseRepositorySetup
    {
        public FinancialAidApplicationOutcomeRepository actualRepository;

        public void ProfileApplicationRepositoyTestsInitialize()
        {
            MockInitialize();            
        }

        [TestClass]
        public class GetFinancialAidApplicationOutcomeRepositoryTests : FinancialAidApplicationOutcomeRepositoryTests
        {
            public string studentId;

            [TestInitialize]
            public void Initialize()
            {
                ProfileApplicationRepositoyTestsInitialize();
                actualRepository = BuildRepository();
            }


            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task FinancialAidApplicationOutcomeRepository_ArgumentNullException()
            {
                string guid = "db2cb4bf-9531-4d38-8bcf-a96bc8628156";
                GuidLookup[] lookup = new GuidLookup[] { new GuidLookup(guid) };
                var lookUpResults = new Dictionary<string, GuidLookupResult>();
                lookUpResults.Add("AR.INV.ITEMS.INTG", new GuidLookupResult() { Entity = "ISIR.CALC.RESULTS", PrimaryKey = "1", SecondaryKey = "Somekey" });
                dataReaderMock.Setup(i => i.SelectAsync(It.IsAny<GuidLookup[]>())).ReturnsAsync(lookUpResults);
                await actualRepository.GetByIdAsync("");
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task FinancialAidApplicationOutcomeRepository_NoFAapplicationOutcomeRecords()
            {
                string guid = "db2cb4bf-9531-4d38-8bcf-a96bc8628156";
                GuidLookup[] lookup = new GuidLookup[] { new GuidLookup(guid) };
                var lookUpResults = new Dictionary<string, GuidLookupResult>();
                lookUpResults.Add("AR.INV.ITEMS.INTG", new GuidLookupResult() { Entity = "ISIR.CALC.RESULTS", PrimaryKey = "1", SecondaryKey = "Somekey" });
                dataReaderMock.Setup(i => i.SelectAsync(It.IsAny<GuidLookup[]>())).ReturnsAsync(lookUpResults);
                await actualRepository.GetByIdAsync("1");
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task FinancialAidApplicationOutcomeRepository_InvalidGetRecordInfoFromGuid()
            {
                string guid = "db2cb4bf-9531-4d38-8bcf-a96bc8628156";
                GuidLookup[] lookup = new GuidLookup[] { new GuidLookup(guid) };
                var lookUpResults = new Dictionary<string, GuidLookupResult>();
                lookUpResults.Add("AR.INV.ITEMS.INTG", new GuidLookupResult() { Entity = "INVALID", PrimaryKey = "1", SecondaryKey = "Somekey" });
                dataReaderMock.Setup(i => i.SelectAsync(It.IsAny<GuidLookup[]>())).ReturnsAsync(lookUpResults);
                await actualRepository.GetByIdAsync("1");
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task FinancialAidApplicationOutcomeRepository_IsirCalcResults_NotFound()
            {
                string guid = "db2cb4bf-9531-4d38-8bcf-a96bc8628156";
                GuidLookup[] lookup = new GuidLookup[] { new GuidLookup(guid) };
                var lookUpResults = new Dictionary<string, GuidLookupResult>();
                lookUpResults.Add("AR.INV.ITEMS.INTG", new GuidLookupResult() { Entity = "ISIR.CALC.RESULTS", PrimaryKey = "1", SecondaryKey = "Somekey" });
                dataReaderMock.Setup(i => i.SelectAsync(It.IsAny<GuidLookup[]>())).ReturnsAsync(lookUpResults);

                var isirCalcResults = new IsirCalcResults() { RecordGuid = guid, Recordkey = "1" };
                dataReaderMock.Setup(i => i.ReadRecordAsync<IsirCalcResults>("ISIR.CALC.RESULTS", "INVALID", It.IsAny<bool>())).ReturnsAsync(isirCalcResults);
                await actualRepository.GetByIdAsync("1");
            }


            [TestMethod]
            public async Task FinancialAidApplicationOutcomeRepository_GetByIdAsync()
            {
                var year = "2017";
                var studentId = "0002020";
                string guid = "db2cb4bf-9531-4d38-8bcf-a96bc8628156";
                GuidLookup[] lookup = new GuidLookup[] { new GuidLookup(guid) };
                var lookUpResults = new Dictionary<string, GuidLookupResult>();
                lookUpResults.Add("AR.INV.ITEMS.INTG", new GuidLookupResult() { Entity = "ISIR.CALC.RESULTS", PrimaryKey = "1", SecondaryKey = "Somekey" });
                dataReaderMock.Setup(i => i.SelectAsync(It.IsAny<GuidLookup[]>())).ReturnsAsync(lookUpResults);

                var isirCalcResults = new IsirCalcResults() { RecordGuid = guid, Recordkey = "1", IcresPriEfc = 444, IcresPriFti = 52000, IcresInstEfcOvrAmt = 50 };
                dataReaderMock.Setup(i => i.ReadRecordAsync<IsirCalcResults>("ISIR.CALC.RESULTS", "1", It.IsAny<bool>())).ReturnsAsync(isirCalcResults);

                var isirFafsa = new IsirFafsa()
                {
                    RecordGuid = guid,
                    Recordkey = "1",
                    IfafImportYear = year,
                    IfafStudentId = studentId,
                    IfafIsirType = "PROF"
                };
                dataReaderMock.Setup(i => i.ReadRecordAsync<IsirFafsa>("ISIR.FAFSA", "1", It.IsAny<bool>())).ReturnsAsync(isirFafsa);

                var isirResults = new IsirResults() { Recordkey = "1" };
                dataReaderMock.Setup(i => i.ReadRecordAsync<IsirResults>("ISIR.RESULTS", "1", It.IsAny<bool>())).ReturnsAsync(isirResults);

                var csAcyrFile = string.Concat("CS.", year);
                var csAcyr = new CsAcyr() { Recordkey = "1", CsFedIsirId = "2", };
                dataReaderMock.Setup(i => i.ReadRecordAsync<CsAcyr>(csAcyrFile, studentId, It.IsAny<bool>())).ReturnsAsync(csAcyr);


                var pncpIds = new string[] { "2" };
                dataReaderMock.Setup(i => i.SelectAsync("ISIR.FAFSA", It.IsAny<string>())).ReturnsAsync(pncpIds);

                var isirFafsa2 = new IsirFafsa()
                {
                    RecordGuid = guid,
                    Recordkey = "2",
                    IfafImportYear = year,
                    IfafStudentId = studentId,
                    IfafIsirType = "PROF",
                    IfafSLegalRes = "NY",
                    IfafFatherGradeLvl = "3",
                    IfafMotherGradeLvl = "3",
                    IfafMarried = "Y",
                    IfafDependChildren = "N",
                    IfafOtherDepend = "N",
                    IfafHousing1 = "1",
                    IfafHousing2 = "2",
                    IfafHousing3 = "3",
                    IfafHousing4 = "4",
                    IfafHousing5 = "5",
                    IfafHousing6 = "6",
                    IfafHousing7 = "7",
                    IfafHousing8 = "8",
                    IfafHousing9 = "9",
                    IfafHousing10 = "10",
                    IfafActiveDuty = "N",
                    IfafHomelessAtRisk = "N"

                };
                dataReaderMock.Setup(i => i.ReadRecordAsync<IsirFafsa>("ISIR.FAFSA", It.IsAny<string>(), It.IsAny<bool>()))
                    .ReturnsAsync(isirFafsa2);



                var isirCalcResults2 = new IsirCalcResults()
                {
                    RecordGuid = new Guid().ToString(),
                    Recordkey = "2",
                    IcresDependency = "I",
                    IcresAzeInd = "Y",
                    IcresSimpleNeedInd = "N",
                    IcresPriEfc = 555,
                    IcresPriFti = 50000,
                    IcresInstEfcOvrAmt = 40
                };
                dataReaderMock.Setup(i => i.ReadRecordAsync<IsirCalcResults>("ISIR.CALC.RESULTS", "2", It.IsAny<bool>()))
                    .ReturnsAsync(isirCalcResults2);

                var isirResults2 = new IsirResults() { Recordkey = "2" };
                dataReaderMock.Setup(i => i.ReadRecordAsync<IsirResults>("ISIR.RESULTS", "2", It.IsAny<bool>()))
                    .ReturnsAsync(isirResults2);

                var isirProfile = new IsirProfile();
                dataReaderMock.Setup(i => i.ReadRecordAsync<IsirProfile>("ISIR.PROFILE", "1", It.IsAny<bool>())).ReturnsAsync(isirProfile);

                var actual = await actualRepository.GetByIdAsync("1");

                Assert.IsNotNull(actual);
                Assert.AreEqual(year, actual.AwardYear);
                Assert.AreEqual(guid, actual.Guid);
                Assert.AreEqual(52000, actual.FisapTotalIncome);
                Assert.AreEqual("2", actual.CsFederalIsirId);
                Assert.AreEqual("2", actual.FafsaPrimaryId);
                Assert.AreEqual("PROF", actual.FafsaPrimaryType);
                Assert.AreEqual(444, actual.FamilyContribution);
                Assert.AreEqual("2", actual.Id);
                Assert.AreEqual(50, actual.InstitutionFamilyContributionOverrideAmount);

            }


            [TestMethod]
            public async Task FinancialAidApplicationOutcomeRepository_GetAsync_VerificationsTrue()
            {
                var year = "2017";
                var studentId = "0002020";
                string guid = "db2cb4bf-9531-4d38-8bcf-a96bc8628156";
                GuidLookup[] lookup = new GuidLookup[] { new GuidLookup(guid) };
                var lookUpResults = new Dictionary<string, GuidLookupResult>();
                lookUpResults.Add("AR.INV.ITEMS.INTG", new GuidLookupResult() { Entity = "ISIR.CALC.RESULTS", PrimaryKey = "1", SecondaryKey = "Somekey" });
                dataReaderMock.Setup(i => i.SelectAsync(It.IsAny<GuidLookup[]>())).ReturnsAsync(lookUpResults);

                var fileSuiteYears = new string[] { year };
                dataReaderMock.Setup(i => i.SelectAsync("FA.SUITES", null)).ReturnsAsync(fileSuiteYears);

                var csAcyrIds = new string[] { "2" };
                dataReaderMock.Setup(i => i.SelectAsync("CS." + year, It.IsAny<string>())).ReturnsAsync(csAcyrIds);

                var csAcyr = new CsAcyr() { Recordkey = studentId, CsFedIsirId = "2", CsInstIsirId = "2", CsInstAdj = 25000 };
                var records = new Collection<CsAcyr>() { csAcyr };
                dataReaderMock.Setup(i => i.BulkReadRecordAsync<CsAcyr>("CS." + year, It.IsAny<string[]>(), true)).ReturnsAsync(records);

                var validApplicationIds = new string[] { "2" };
                dataReaderMock.Setup(i => i.SelectAsync("ISIR.FAFSA", It.IsAny<string[]>(), It.IsAny<string>())).ReturnsAsync(validApplicationIds);

                var isirFafsa = new IsirFafsa()
                {
                    RecordGuid = guid,
                    Recordkey = "2",
                    IfafImportYear = year,
                    IfafStudentId = studentId,
                    IfafIsirType = "PROF",
                    IfafSLegalRes = "NY",
                    IfafFatherGradeLvl = "3",
                    IfafMotherGradeLvl = "3",
                    IfafMarried = "Y",
                    IfafDependChildren = "N",
                    IfafOtherDepend = "N",
                    IfafHousing1 = "1",
                    IfafHousing2 = "2",
                    IfafHousing3 = "3",
                    IfafHousing4 = "4",
                    IfafHousing5 = "5",
                    IfafHousing6 = "6",
                    IfafHousing7 = "7",
                    IfafHousing8 = "8",
                    IfafHousing9 = "9",
                    IfafHousing10 = "10",
                    IfafActiveDuty = "N",
                    IfafHomelessAtRisk = "N",
                    IfafFaaAdj = "Y"

                };
                var isirFafsaCollection = new Collection<IsirFafsa>() { isirFafsa };
                dataReaderMock.Setup(i => i.BulkReadRecordAsync<IsirFafsa>(It.IsAny<string[]>(), true)).ReturnsAsync(isirFafsaCollection);

                var isirCalcResults = new IsirCalcResults()
                {
                    RecordGuid = new Guid().ToString(),
                    Recordkey = "2",
                    IcresDependency = "I",
                    IcresAzeInd = "Y",
                    IcresSimpleNeedInd = "N",
                    IcresPriEfc = 444,
                    IcresPriFti = 52000,
                    IcresInstEfcOvrAmt = 50
                };
                var isirCalcResultCollection = new Collection<IsirCalcResults>() { isirCalcResults };
                dataReaderMock.Setup(i => i.BulkReadRecordAsync<IsirCalcResults>(It.IsAny<string[]>(), true)).ReturnsAsync(isirCalcResultCollection);

                var isirResults = new IsirResults() { Recordkey = "2", IresSarCFlag = "Y", IresCpsPellElig = "Y", IresVerifFlag = "Y" };
                var isirResultsCollection = new Collection<IsirResults>() { isirResults };
                dataReaderMock.Setup(i => i.BulkReadRecordAsync<IsirResults>(It.IsAny<string[]>(), true)).ReturnsAsync(isirResultsCollection);

                var isirProfile = new IsirProfile() { };
                var isirProfileCollection = new Collection<IsirProfile>() { isirProfile };
                dataReaderMock.Setup(i => i.BulkReadRecordAsync<IsirProfile>(It.IsAny<string[]>(), true)).ReturnsAsync(isirProfileCollection);

                var faSystemParamsResponseData = new FaSysParams()
                {
                    FspInstitutionName = "Datatel Community College FA"
                };
                dataReaderMock.Setup<FaSysParams>(acc => acc.ReadRecord<FaSysParams>("ST.PARMS", "FA.SYS.PARAMS", true)).Returns(faSystemParamsResponseData);
                
                var faSuiteYears = new List<string>() { year };
                var tuple = await actualRepository.GetAsync(0, 1, false, null, null, null, null, faSuiteYears);

                Assert.IsNotNull(tuple);
                var fafsa = tuple.Item1.ToList();
                var count = tuple.Item2;
                Assert.AreEqual(1, count);

                var actual = fafsa.FirstOrDefault();

                Assert.AreEqual(year, actual.AwardYear);
                Assert.AreEqual(guid, actual.Guid);
                Assert.AreEqual(52000, actual.FisapTotalIncome);
                Assert.AreEqual("2", actual.CsFederalIsirId);
                Assert.AreEqual("2", actual.FafsaPrimaryId);
                Assert.AreEqual("PROF", actual.FafsaPrimaryType);
                Assert.AreEqual(444, actual.FamilyContribution);
                Assert.AreEqual("2", actual.Id);
                Assert.AreEqual(50, actual.InstitutionFamilyContributionOverrideAmount);
                Assert.AreEqual("Y", actual.FinancialAidAAministratorAdjustment);
                Assert.AreEqual(true, actual.HasStudentAidReportC);
                Assert.AreEqual(true, actual.IsPellEligible);
                Assert.AreEqual(true, actual.HasVerificationSelection);

            }

            [TestMethod]
            public async Task FinancialAidApplicationOutcomeRepository_GetAsync_CorrectionVerificationsTrue()
            {
                var year = "2017";
                var studentId = "0002020";
                string guidIsirFafsaOrig = "db2cb4bf-9531-4d38-8bcf-a96bc8628156";
                string guidIsirFafsaCorr = "db2cb4bf-9531-4d38-8bcf-a96bc8628157";
                string guidIsirCalcResultOrig = "db2cb4bf-9531-4d38-8bcf-a96bc8628158";
                string guidIsirCalcResultCorr = "db2cb4bf-9531-4d38-8bcf-a96bc8628159";
                GuidLookup[] lookup = new GuidLookup[] { new GuidLookup(guidIsirFafsaOrig) };
                var lookUpResults = new Dictionary<string, GuidLookupResult>();
                lookUpResults.Add("AR.INV.ITEMS.INTG", new GuidLookupResult() { Entity = "ISIR.CALC.RESULTS", PrimaryKey = "1", SecondaryKey = "Somekey" });
                dataReaderMock.Setup(i => i.SelectAsync(It.IsAny<GuidLookup[]>())).ReturnsAsync(lookUpResults);

                var fileSuiteYears = new string[] { year };
                dataReaderMock.Setup(i => i.SelectAsync("FA.SUITES", null)).ReturnsAsync(fileSuiteYears);

                var csAcyrIds = new string[] { "2" };
                dataReaderMock.Setup(i => i.SelectAsync("CS." + year, It.IsAny<string>())).ReturnsAsync(csAcyrIds);

                var csAcyr = new CsAcyr() { Recordkey = studentId, CsFedIsirId = "2", CsInstIsirId = "2", CsInstAdj = 25000 };
                var records = new Collection<CsAcyr>() { csAcyr };
                dataReaderMock.Setup(i => i.BulkReadRecordAsync<CsAcyr>("CS." + year, It.IsAny<string[]>(), true)).ReturnsAsync(records);

                var validApplicationIds = new string[] { "2" };
                dataReaderMock.Setup(i => i.SelectAsync("ISIR.FAFSA", It.IsAny<string[]>(), It.IsAny<string>())).ReturnsAsync(validApplicationIds);

                var isirFafsaOrig = new IsirFafsa()
                {
                    RecordGuid = guidIsirFafsaOrig,
                    Recordkey = "2",
                    IfafImportYear = year,
                    IfafStudentId = studentId,
                    IfafIsirType = "PROF",
                    IfafCorrectionId = "3"

                };
                var isirFafsaCollection = new Collection<IsirFafsa>() { isirFafsaOrig };

                var isirFafsaCorr = new IsirFafsa()
                {
                    RecordGuid = guidIsirFafsaCorr,
                    Recordkey = "3",
                    IfafImportYear = year,
                    IfafStudentId = studentId,
                    IfafIsirType = "PROF",
                    IfafSLegalRes = "NY",
                    IfafFatherGradeLvl = "3",
                    IfafMotherGradeLvl = "3",
                    IfafMarried = "Y",
                    IfafDependChildren = "N",
                    IfafOtherDepend = "N",
                    IfafHousing1 = "1",
                    IfafHousing2 = "2",
                    IfafHousing3 = "3",
                    IfafHousing4 = "4",
                    IfafHousing5 = "5",
                    IfafHousing6 = "6",
                    IfafHousing7 = "7",
                    IfafHousing8 = "8",
                    IfafHousing9 = "9",
                    IfafHousing10 = "10",
                    IfafActiveDuty = "N",
                    IfafHomelessAtRisk = "N",
                    IfafFaaAdj = "Y"
                };
                isirFafsaCollection.Add(isirFafsaCorr);

                dataReaderMock.Setup(i => i.BulkReadRecordAsync<IsirFafsa>(It.IsAny<string[]>(), true)).ReturnsAsync(isirFafsaCollection);

                var isirCalcResultsOrig = new IsirCalcResults()
                {
                    RecordGuid = guidIsirCalcResultOrig,
                    Recordkey = "2"
                };
                var isirCalcResultCollection = new Collection<IsirCalcResults>() { isirCalcResultsOrig };

                var isirCalcResultsCorr = new IsirCalcResults()
                {
                    RecordGuid = guidIsirCalcResultCorr,
                    Recordkey = "3",
                    IcresDependency = "I",
                    IcresAzeInd = "Y",
                    IcresSimpleNeedInd = "N",
                    IcresPriEfc = 444,
                    IcresPriFti = 52000,
                    IcresInstEfcOvrAmt = 50
                };

                isirCalcResultCollection.Add(isirCalcResultsCorr);

                dataReaderMock.Setup(i => i.BulkReadRecordAsync<IsirCalcResults>(It.IsAny<string[]>(), true)).ReturnsAsync(isirCalcResultCollection);

                var isirResultsCorr = new IsirResults() { Recordkey = "3", IresSarCFlag = "Y", IresCpsPellElig = "Y", IresVerifFlag = "Y" };
                var isirResultsCollection = new Collection<IsirResults>() { isirResultsCorr };
                dataReaderMock.Setup(i => i.BulkReadRecordAsync<IsirResults>(It.IsAny<string[]>(), true)).ReturnsAsync(isirResultsCollection);

                var isirProfile = new IsirProfile() { };
                var isirProfileCollection = new Collection<IsirProfile>() { isirProfile };
                dataReaderMock.Setup(i => i.BulkReadRecordAsync<IsirProfile>(It.IsAny<string[]>(), true)).ReturnsAsync(isirProfileCollection);

                var faSystemParamsResponseData = new FaSysParams()
                {
                    FspInstitutionName = "Datatel Community College FA"
                };
                dataReaderMock.Setup<FaSysParams>(acc => acc.ReadRecord<FaSysParams>("ST.PARMS", "FA.SYS.PARAMS", true)).Returns(faSystemParamsResponseData);


                var faSuiteYears = new List<string>() { year };
                var tuple = await actualRepository.GetAsync(0, 1, false, null, null, null, null, faSuiteYears);

                Assert.IsNotNull(tuple);
                var fafsa = tuple.Item1.ToList();
                var count = tuple.Item2;
                Assert.AreEqual(1, count);

                var actual = fafsa.FirstOrDefault();

                Assert.AreEqual(year, actual.AwardYear);
                Assert.AreEqual(guidIsirFafsaCorr, actual.Guid);
                Assert.AreEqual(52000, actual.FisapTotalIncome);
                Assert.AreEqual("2", actual.CsFederalIsirId);
                Assert.AreEqual("3", actual.FafsaPrimaryId);
                Assert.AreEqual("PROF", actual.FafsaPrimaryType);
                Assert.AreEqual(444, actual.FamilyContribution);
                Assert.AreEqual("3", actual.Id);
                Assert.AreEqual(50, actual.InstitutionFamilyContributionOverrideAmount);
                Assert.AreEqual("Y", actual.FinancialAidAAministratorAdjustment);
                Assert.AreEqual(true, actual.HasStudentAidReportC);
                Assert.AreEqual(true, actual.IsPellEligible);
                Assert.AreEqual(true, actual.HasVerificationSelection);
                Assert.AreEqual(guidIsirCalcResultCorr, actual.CalcResultsGuid);

            }

            [TestMethod]
            public async Task FinancialAidApplicationOutcomeRepository_GetAsync_VerificationsFalse()
            {
                //CollectionAssert.AreEqual(expectedProfileApplications, actualProfileApplications);
                var year = "2017";
                var studentId = "0002020";
                string guid = "db2cb4bf-9531-4d38-8bcf-a96bc8628156";
                GuidLookup[] lookup = new GuidLookup[] { new GuidLookup(guid) };
                var lookUpResults = new Dictionary<string, GuidLookupResult>();
                lookUpResults.Add("AR.INV.ITEMS.INTG", new GuidLookupResult() { Entity = "ISIR.CALC.RESULTS", PrimaryKey = "1", SecondaryKey = "Somekey" });
                dataReaderMock.Setup(i => i.SelectAsync(It.IsAny<GuidLookup[]>())).ReturnsAsync(lookUpResults);

                // var fileSuiteYears = await DataReader.SelectAsync("FA.SUITES", null);
                var fileSuiteYears = new string[] { year };
                dataReaderMock.Setup(i => i.SelectAsync("FA.SUITES", null)).ReturnsAsync(fileSuiteYears);
                
                var csAcyrIds = new string[] { "2" };
                dataReaderMock.Setup(i => i.SelectAsync("CS." + year, It.IsAny<string>())).ReturnsAsync(csAcyrIds);

                var csAcyr = new CsAcyr() { Recordkey = studentId, CsFedIsirId = "2", CsInstIsirId = "2", CsInstAdj = 25000 };
                var records = new Collection<CsAcyr>() { csAcyr };
                dataReaderMock.Setup(i => i.BulkReadRecordAsync<CsAcyr>("CS." + year, It.IsAny<string[]>(), true)).ReturnsAsync(records);

                var validApplicationIds = new string[] { "2" };
                dataReaderMock.Setup(i => i.SelectAsync("ISIR.FAFSA", It.IsAny<string[]>(), It.IsAny<string>())).ReturnsAsync(validApplicationIds);

                var isirFafsa = new IsirFafsa()
                {
                    RecordGuid = guid,
                    Recordkey = "2",
                    IfafImportYear = year,
                    IfafStudentId = studentId,
                    IfafIsirType = "PROF",
                    IfafSLegalRes = "NY",
                    IfafFatherGradeLvl = "3",
                    IfafMotherGradeLvl = "3",
                    IfafMarried = "Y",
                    IfafDependChildren = "N",
                    IfafOtherDepend = "N",
                    IfafHousing1 = "1",
                    IfafHousing2 = "2",
                    IfafHousing3 = "3",
                    IfafHousing4 = "4",
                    IfafHousing5 = "5",
                    IfafHousing6 = "6",
                    IfafHousing7 = "7",
                    IfafHousing8 = "8",
                    IfafHousing9 = "9",
                    IfafHousing10 = "10",
                    IfafActiveDuty = "N",
                    IfafHomelessAtRisk = "N",
                    IfafFaaAdj = "N"

                };
                var isirFafsaCollection = new Collection<IsirFafsa>() { isirFafsa };
                dataReaderMock.Setup(i => i.BulkReadRecordAsync<IsirFafsa>(It.IsAny<string[]>(), true)).ReturnsAsync(isirFafsaCollection);
                
                var isirCalcResults = new IsirCalcResults()
                {
                    RecordGuid = new Guid().ToString(),
                    Recordkey = "2",
                    IcresDependency = "I",
                    IcresAzeInd = "Y",
                    IcresSimpleNeedInd = "N",
                    IcresPriEfc = 444,
                    IcresPriFti = 52000,
                    IcresInstEfcOvrAmt = 50
                };
                var isirCalcResultCollection = new Collection<IsirCalcResults>() { isirCalcResults };
                dataReaderMock.Setup(i => i.BulkReadRecordAsync<IsirCalcResults>(It.IsAny<string[]>(), true)).ReturnsAsync(isirCalcResultCollection);

                var isirResults = new IsirResults() { Recordkey = "2", IresSarCFlag = "N", IresCpsPellElig = "N", IresVerifFlag = "N" };
                var isirResultsCollection = new Collection<IsirResults>() { isirResults };
                dataReaderMock.Setup(i => i.BulkReadRecordAsync<IsirResults>(It.IsAny<string[]>(), true)).ReturnsAsync(isirResultsCollection);

                var isirProfile = new IsirProfile() { };
                var isirProfileCollection = new Collection<IsirProfile>() { isirProfile };
                dataReaderMock.Setup(i => i.BulkReadRecordAsync<IsirProfile>(It.IsAny<string[]>(), true)).ReturnsAsync(isirProfileCollection);
                
                var faSystemParamsResponseData = new FaSysParams()
                {
                    FspInstitutionName = "Datatel Community College FA"
                };
                dataReaderMock.Setup<FaSysParams>(acc => acc.ReadRecord<FaSysParams>("ST.PARMS", "FA.SYS.PARAMS", true)).Returns(faSystemParamsResponseData);


                var faSuiteYears = new List<string>() { year };
                var tuple = await actualRepository.GetAsync(0, 1, false, null, null, null, null, faSuiteYears);

                Assert.IsNotNull(tuple);
                var fafsa = tuple.Item1.ToList();
                var count = tuple.Item2;
                Assert.AreEqual(1, count);

                var actual = fafsa.FirstOrDefault();

                Assert.AreEqual(year, actual.AwardYear);
                Assert.AreEqual(guid, actual.Guid);
                Assert.AreEqual(52000, actual.FisapTotalIncome);
                Assert.AreEqual("2", actual.CsFederalIsirId);
                Assert.AreEqual("2", actual.FafsaPrimaryId);
                Assert.AreEqual("PROF", actual.FafsaPrimaryType);
                Assert.AreEqual(444, actual.FamilyContribution);
                Assert.AreEqual("2", actual.Id);
                Assert.AreEqual(50, actual.InstitutionFamilyContributionOverrideAmount);
                // Assert.AreEqual(false, actual.HasFinancialAidAAministratorAdjustment);
                Assert.AreEqual(false, actual.HasStudentAidReportC);
                Assert.AreEqual(false, actual.IsPellEligible);
                Assert.AreEqual(false, actual.HasVerificationSelection);

            }


            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task FinancialAidApplicationOutcomeRepository_GetAsync_InvalidIsirFafsa()
            {
                var year = "2017";
                var studentId = "0002020";
                string guid = "db2cb4bf-9531-4d38-8bcf-a96bc8628156";
                GuidLookup[] lookup = new GuidLookup[] { new GuidLookup(guid) };
                var lookUpResults = new Dictionary<string, GuidLookupResult>();
                lookUpResults.Add("AR.INV.ITEMS.INTG", new GuidLookupResult() { Entity = "ISIR.CALC.RESULTS", PrimaryKey = "1", SecondaryKey = "Somekey" });
                dataReaderMock.Setup(i => i.SelectAsync(It.IsAny<GuidLookup[]>())).ReturnsAsync(lookUpResults);

                var fileSuiteYears = new string[] { year };
                dataReaderMock.Setup(i => i.SelectAsync("FA.SUITES", null)).ReturnsAsync(fileSuiteYears);

                                var csAcyrIds = new string[] { "2" };
                dataReaderMock.Setup(i => i.SelectAsync("CS." + year, It.IsAny<string>())).ReturnsAsync(csAcyrIds);

                //var records = await DataReader.BulkReadRecordAsync<CsAcyr>("CS." + year, subList.ToArray());
                var csAcyr = new CsAcyr() { Recordkey = studentId, CsFedIsirId = "2", CsInstIsirId = "2", CsInstAdj = 25000 };
                var records = new Collection<CsAcyr>() { csAcyr };
                dataReaderMock.Setup(i => i.BulkReadRecordAsync<CsAcyr>("CS." + year, It.IsAny<string[]>(), true)).ReturnsAsync(records);

                var validApplicationIds = new string[] { "2" };
                dataReaderMock.Setup(i => i.SelectAsync("ISIR.FAFSA", It.IsAny<string[]>(), It.IsAny<string>())).ReturnsAsync(validApplicationIds);

                dataReaderMock.Setup(i => i.BulkReadRecordAsync<IsirFafsa>(It.IsAny<string[]>(), true)).Throws(new KeyNotFoundException());


                var isirCalcResults = new IsirCalcResults()
                {
                    RecordGuid = new Guid().ToString(),
                    Recordkey = "2",
                    IcresDependency = "I",
                    IcresAzeInd = "Y",
                    IcresSimpleNeedInd = "N",
                    IcresPriEfc = 444,
                    IcresPriFti = 52000,
                    IcresInstEfcOvrAmt = 50
                };
                var isirCalcResultCollection = new Collection<IsirCalcResults>() { isirCalcResults };
                dataReaderMock.Setup(i => i.BulkReadRecordAsync<IsirCalcResults>(It.IsAny<string[]>(), true)).ReturnsAsync(isirCalcResultCollection);

                // var bulkIsirCalcResultsRecords = await DataReader.BulkReadRecordAsync<IsirCalcResults>(effectiveApplicationSubList);

                var isirResults = new IsirResults() { Recordkey = "2", IresSarCFlag = "N", IresCpsPellElig = "N", IresVerifFlag = "N" };
                var isirResultsCollection = new Collection<IsirResults>() { isirResults };
                dataReaderMock.Setup(i => i.BulkReadRecordAsync<IsirResults>(It.IsAny<string[]>(), true)).ReturnsAsync(isirResultsCollection);

                // var bulkIsirResultsRecords = await DataReader.BulkReadRecordAsync<IsirResults>(effectiveApplicationSubList);

                var isirProfile = new IsirProfile() { };
                var isirProfileCollection = new Collection<IsirProfile>() { isirProfile };
                dataReaderMock.Setup(i => i.BulkReadRecordAsync<IsirProfile>(It.IsAny<string[]>(), true)).ReturnsAsync(isirProfileCollection);
                //var IsirProfileRecords = await DataReader.BulkReadRecordAsync<IsirProfile>(effectiveApplicationSubList);

                var faSystemParamsResponseData = new FaSysParams()
                {
                    FspInstitutionName = "Datatel Community College FA"
                };
                dataReaderMock.Setup<FaSysParams>(acc => acc.ReadRecord<FaSysParams>("ST.PARMS", "FA.SYS.PARAMS", true)).Returns(faSystemParamsResponseData);


                var faSuiteYears = new List<string>() { year };
                await actualRepository.GetAsync(0, 1, false, null, null, null, null, faSuiteYears);

            }


            [TestMethod]
            public async Task FinancialAidApplicationOutcomeRepository_GetAsync_InvalidYear()
            {
                //CollectionAssert.AreEqual(expectedProfileApplications, actualProfileApplications);
                var year = "2014";
                var studentId = "0002020";
                string guid = "db2cb4bf-9531-4d38-8bcf-a96bc8628156";
                GuidLookup[] lookup = new GuidLookup[] { new GuidLookup(guid) };
                var lookUpResults = new Dictionary<string, GuidLookupResult>();
                lookUpResults.Add("AR.INV.ITEMS.INTG", new GuidLookupResult() { Entity = "ISIR.CALC.RESULTS", PrimaryKey = "1", SecondaryKey = "Somekey" });
                dataReaderMock.Setup(i => i.SelectAsync(It.IsAny<GuidLookup[]>())).ReturnsAsync(lookUpResults);

                // var fileSuiteYears = await DataReader.SelectAsync("FA.SUITES", null);
                var fileSuiteYears = new string[] { year };
                dataReaderMock.Setup(i => i.SelectAsync("FA.SUITES", null)).ReturnsAsync(fileSuiteYears);


                // var csAcyrIds = await DataReader.SelectAsync("CS." + year, "WITH CS.FED.ISIR.ID NE '' OR WITH CS.INST.ISIR.ID NE ''");
                var csAcyrIds = new string[] { "2" };
                dataReaderMock.Setup(i => i.SelectAsync("CS." + year, "WITH CS.FED.ISIR.ID NE '' OR WITH CS.INST.ISIR.ID NE ''")).ReturnsAsync(() => null);

                //var records = await DataReader.BulkReadRecordAsync<CsAcyr>("CS." + year, subList.ToArray());
                var csAcyr = new CsAcyr() { Recordkey = studentId, CsFedIsirId = "2", CsInstIsirId = "2", CsInstAdj = 25000 };
                var records = new Collection<CsAcyr>() { csAcyr };
                dataReaderMock.Setup(i => i.BulkReadRecordAsync<CsAcyr>("CS." + year, It.IsAny<string[]>(), true)).ReturnsAsync(records);

                var validApplicationIds = new string[] { "2" };
                dataReaderMock.Setup(i => i.SelectAsync("ISIR.FAFSA", "WITH IFAF.STUDENT.ID NE '' AND WITH IFAF.IMPORT.YEAR NE ''")).ReturnsAsync(() => null);
                // var validApplicationIds = await DataReader.SelectAsync("ISIR.FAFSA", "WITH IFAF.STUDENT.ID NE '' AND WITH IFAF.IMPORT.YEAR NE ''");

                var isirFafsa = new IsirFafsa()
                {
                    RecordGuid = guid,
                    Recordkey = "2",
                    IfafImportYear = year,
                    IfafStudentId = studentId,
                    IfafIsirType = "PROF",
                    IfafSLegalRes = "NY",
                    IfafFatherGradeLvl = "3",
                    IfafMotherGradeLvl = "3",
                    IfafMarried = "Y",
                    IfafDependChildren = "N",
                    IfafOtherDepend = "N",
                    IfafHousing1 = "1",
                    IfafHousing2 = "2",
                    IfafHousing3 = "3",
                    IfafHousing4 = "4",
                    IfafHousing5 = "5",
                    IfafHousing6 = "6",
                    IfafHousing7 = "7",
                    IfafHousing8 = "8",
                    IfafHousing9 = "9",
                    IfafHousing10 = "10",
                    IfafActiveDuty = "N",
                    IfafHomelessAtRisk = "N",
                    IfafFaaAdj = "Y"

                };
                var isirFafsaCollection = new Collection<IsirFafsa>() { isirFafsa };
                dataReaderMock.Setup(i => i.BulkReadRecordAsync<IsirFafsa>(It.IsAny<string[]>(), true)).ReturnsAsync(isirFafsaCollection);
                // var bulkRecords = await DataReader.BulkReadRecordAsync<IsirFafsa>(applicationSubList);

                var isirCalcResults = new IsirCalcResults()
                {
                    RecordGuid = new Guid().ToString(),
                    Recordkey = "2",
                    IcresDependency = "I",
                    IcresAzeInd = "Y",
                    IcresSimpleNeedInd = "N",
                    IcresPriEfc = 444,
                    IcresPriFti = 52000,
                    IcresInstEfcOvrAmt = 50
                };
                var isirCalcResultCollection = new Collection<IsirCalcResults>() { isirCalcResults };
                dataReaderMock.Setup(i => i.BulkReadRecordAsync<IsirCalcResults>(It.IsAny<string[]>(), true)).ReturnsAsync(isirCalcResultCollection);

                // var bulkIsirCalcResultsRecords = await DataReader.BulkReadRecordAsync<IsirCalcResults>(effectiveApplicationSubList);

                var isirResults = new IsirResults() { Recordkey = "2", IresSarCFlag = "Y", IresCpsPellElig = "Y", IresVerifFlag = "Y" };
                var isirResultsCollection = new Collection<IsirResults>() { isirResults };
                dataReaderMock.Setup(i => i.BulkReadRecordAsync<IsirResults>(It.IsAny<string[]>(), true)).ReturnsAsync(isirResultsCollection);

                // var bulkIsirResultsRecords = await DataReader.BulkReadRecordAsync<IsirResults>(effectiveApplicationSubList);

                var isirProfile = new IsirProfile() { };
                var isirProfileCollection = new Collection<IsirProfile>() { isirProfile };
                dataReaderMock.Setup(i => i.BulkReadRecordAsync<IsirProfile>(It.IsAny<string[]>(), true)).ReturnsAsync(isirProfileCollection);
                //var IsirProfileRecords = await DataReader.BulkReadRecordAsync<IsirProfile>(effectiveApplicationSubList);

                var faSystemParamsResponseData = new FaSysParams()
                {
                    FspInstitutionName = "Datatel Community College FA"
                };
                dataReaderMock.Setup<FaSysParams>(acc => acc.ReadRecord<FaSysParams>("ST.PARMS", "FA.SYS.PARAMS", true)).Returns(faSystemParamsResponseData);
                               
                transManagerMock.Setup(acc => acc.ExecuteAsync<GetCacheApiKeysRequest,
                    GetCacheApiKeysResponse>(It.IsAny<GetCacheApiKeysRequest>())).ReturnsAsync(() => null);

                var faSuiteYears = new List<string>() { year };
                var tuple = await actualRepository.GetAsync(0, 1, false, null, null, null, null, faSuiteYears);

                Assert.IsNotNull(tuple);
                var fafsa = tuple.Item1.ToList();
                var count = tuple.Item2;
                Assert.AreEqual(0, count);

            }
            private FinancialAidApplicationOutcomeRepository BuildRepository()
            {
                var response = new Ellucian.Colleague.Domain.Base.Transactions.GetCacheApiKeysResponse()
                {
                    Offset = 0,
                    Limit = 1,
                    CacheName = "AllFinancialAidApplicationOutcomesCacheKey:",
                    Entity = "",
                    Sublist = new List<string>() { "2" },
                    TotalCount = 1,
                    KeyCacheInfo = new List<KeyCacheInfo>()
                {
                    new KeyCacheInfo()
                    {
                        KeyCacheMax = 5905,
                        KeyCacheMin = 1,
                        KeyCachePart = "000",
                        KeyCacheSize = 5905
                    },
                    new KeyCacheInfo()
                    {
                        KeyCacheMax = 7625,
                        KeyCacheMin = 5906,
                        KeyCachePart = "001",
                        KeyCacheSize = 1720
                    }
                }
                };
                transManagerMock.Setup(acc => acc.ExecuteAsync<GetCacheApiKeysRequest,
                    GetCacheApiKeysResponse>(It.IsAny<GetCacheApiKeysRequest>())).ReturnsAsync(response);

                ApiSettings apiSettings = new ApiSettings("null");
                return new FinancialAidApplicationOutcomeRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object, apiSettings);
            }

            [TestMethod]
            public async Task FinancialAidApplicationOutcomeRepository_GetAsync_ValidFilterYear()
            {
                var year = "2017";
                var studentId = "0002020";
                string guid = "db2cb4bf-9531-4d38-8bcf-a96bc8628156";
                GuidLookup[] lookup = new GuidLookup[] { new GuidLookup(guid) };
                var lookUpResults = new Dictionary<string, GuidLookupResult>();
                lookUpResults.Add("AR.INV.ITEMS.INTG", new GuidLookupResult() { Entity = "ISIR.CALC.RESULTS", PrimaryKey = "1", SecondaryKey = "Somekey" });
                dataReaderMock.Setup(i => i.SelectAsync(It.IsAny<GuidLookup[]>())).ReturnsAsync(lookUpResults);

                var fileSuiteYears = new string[] { year };
                dataReaderMock.Setup(i => i.SelectAsync("FA.SUITES", null)).ReturnsAsync(fileSuiteYears);

                var csAcyrIds = new string[] { "2" };
                dataReaderMock.Setup(i => i.SelectAsync("CS." + year, It.IsAny<string>())).ReturnsAsync(csAcyrIds);

                var csAcyr = new CsAcyr() { Recordkey = studentId, CsFedIsirId = "2", CsInstIsirId = "2", CsInstAdj = 25000 };
                var records = new Collection<CsAcyr>() { csAcyr };
                dataReaderMock.Setup(i => i.BulkReadRecordAsync<CsAcyr>("CS." + year, It.IsAny<string[]>(), true)).ReturnsAsync(records);

                var validApplicationIds = new string[] { "2" };
                dataReaderMock.Setup(i => i.SelectAsync("ISIR.FAFSA", It.IsAny<string[]>(), It.IsAny<string>())).ReturnsAsync(validApplicationIds);

                var isirFafsa = new IsirFafsa()
                {
                    RecordGuid = guid,
                    Recordkey = "2",
                    IfafImportYear = year,
                    IfafStudentId = studentId,
                    IfafIsirType = "PROF",
                    IfafSLegalRes = "NY",
                    IfafFatherGradeLvl = "3",
                    IfafMotherGradeLvl = "3",
                    IfafMarried = "Y",
                    IfafDependChildren = "N",
                    IfafOtherDepend = "N",
                    IfafHousing1 = "1",
                    IfafHousing2 = "2",
                    IfafHousing3 = "3",
                    IfafHousing4 = "4",
                    IfafHousing5 = "5",
                    IfafHousing6 = "6",
                    IfafHousing7 = "7",
                    IfafHousing8 = "8",
                    IfafHousing9 = "9",
                    IfafHousing10 = "10",
                    IfafActiveDuty = "N",
                    IfafHomelessAtRisk = "N",
                    IfafFaaAdj = "Y"

                };
                var isirFafsaCollection = new Collection<IsirFafsa>() { isirFafsa };
                dataReaderMock.Setup(i => i.BulkReadRecordAsync<IsirFafsa>(It.IsAny<string[]>(), true)).ReturnsAsync(isirFafsaCollection);

                var isirCalcResults = new IsirCalcResults()
                {
                    RecordGuid = new Guid().ToString(),
                    Recordkey = "2",
                    IcresDependency = "I",
                    IcresAzeInd = "Y",
                    IcresSimpleNeedInd = "N",
                    IcresPriEfc = 444,
                    IcresPriFti = 52000,
                    IcresInstEfcOvrAmt = 50
                };
                var isirCalcResultCollection = new Collection<IsirCalcResults>() { isirCalcResults };
                dataReaderMock.Setup(i => i.BulkReadRecordAsync<IsirCalcResults>(It.IsAny<string[]>(), true)).ReturnsAsync(isirCalcResultCollection);

                var isirResults = new IsirResults() { Recordkey = "2", IresSarCFlag = "Y", IresCpsPellElig = "Y", IresVerifFlag = "Y" };
                var isirResultsCollection = new Collection<IsirResults>() { isirResults };
                dataReaderMock.Setup(i => i.BulkReadRecordAsync<IsirResults>(It.IsAny<string[]>(), true)).ReturnsAsync(isirResultsCollection);

                var isirProfile = new IsirProfile() { };
                var isirProfileCollection = new Collection<IsirProfile>() { isirProfile };
                dataReaderMock.Setup(i => i.BulkReadRecordAsync<IsirProfile>(It.IsAny<string[]>(), true)).ReturnsAsync(isirProfileCollection);

                var faSystemParamsResponseData = new FaSysParams()
                {
                    FspInstitutionName = "Datatel Community College FA"
                };
                dataReaderMock.Setup<FaSysParams>(acc => acc.ReadRecord<FaSysParams>("ST.PARMS", "FA.SYS.PARAMS", true)).Returns(faSystemParamsResponseData);

                var faSuiteYears = new List<string>() { year };
                var tuple = await actualRepository.GetAsync(0, 1, false, null, "2017", null, null, faSuiteYears);

                Assert.IsNotNull(tuple);
                var fafsa = tuple.Item1.ToList();
                var count = tuple.Item2;
                Assert.AreEqual(1, count);
                
            }

            [TestMethod]
            public async Task FinancialAidApplicationOutcomeRepository_GetAsync_InvalidFilterYear()
            {
                var year = "2017";
                var studentId = "0002020";
                string guid = "db2cb4bf-9531-4d38-8bcf-a96bc8628156";
                GuidLookup[] lookup = new GuidLookup[] { new GuidLookup(guid) };
                var lookUpResults = new Dictionary<string, GuidLookupResult>();
                lookUpResults.Add("AR.INV.ITEMS.INTG", new GuidLookupResult() { Entity = "ISIR.CALC.RESULTS", PrimaryKey = "1", SecondaryKey = "Somekey" });
                dataReaderMock.Setup(i => i.SelectAsync(It.IsAny<GuidLookup[]>())).ReturnsAsync(lookUpResults);

                var fileSuiteYears = new string[] { year };
                dataReaderMock.Setup(i => i.SelectAsync("FA.SUITES", null)).ReturnsAsync(fileSuiteYears);

                var csAcyrIds = new string[] { "2" };
                dataReaderMock.Setup(i => i.SelectAsync("CS." + year, It.IsAny<string>())).ReturnsAsync(csAcyrIds);

                var csAcyr = new CsAcyr() { Recordkey = studentId, CsFedIsirId = "2", CsInstIsirId = "2", CsInstAdj = 25000 };
                var records = new Collection<CsAcyr>() { csAcyr };
                dataReaderMock.Setup(i => i.BulkReadRecordAsync<CsAcyr>("CS." + year, It.IsAny<string[]>(), true)).ReturnsAsync(records);

                var validApplicationIds = new string[] { "2" };
                dataReaderMock.Setup(i => i.SelectAsync("ISIR.FAFSA", It.IsAny<string[]>(), It.IsAny<string>())).ReturnsAsync(validApplicationIds);

                var isirFafsa = new IsirFafsa()
                {
                    RecordGuid = guid,
                    Recordkey = "2",
                    IfafImportYear = year,
                    IfafStudentId = studentId,
                    IfafIsirType = "PROF",
                    IfafSLegalRes = "NY",
                    IfafFatherGradeLvl = "3",
                    IfafMotherGradeLvl = "3",
                    IfafMarried = "Y",
                    IfafDependChildren = "N",
                    IfafOtherDepend = "N",
                    IfafHousing1 = "1",
                    IfafHousing2 = "2",
                    IfafHousing3 = "3",
                    IfafHousing4 = "4",
                    IfafHousing5 = "5",
                    IfafHousing6 = "6",
                    IfafHousing7 = "7",
                    IfafHousing8 = "8",
                    IfafHousing9 = "9",
                    IfafHousing10 = "10",
                    IfafActiveDuty = "N",
                    IfafHomelessAtRisk = "N",
                    IfafFaaAdj = "Y"

                };
                var isirFafsaCollection = new Collection<IsirFafsa>() { isirFafsa };
                dataReaderMock.Setup(i => i.BulkReadRecordAsync<IsirFafsa>(It.IsAny<string[]>(), true)).ReturnsAsync(isirFafsaCollection);

                var isirCalcResults = new IsirCalcResults()
                {
                    RecordGuid = new Guid().ToString(),
                    Recordkey = "2",
                    IcresDependency = "I",
                    IcresAzeInd = "Y",
                    IcresSimpleNeedInd = "N",
                    IcresPriEfc = 444,
                    IcresPriFti = 52000,
                    IcresInstEfcOvrAmt = 50
                };
                var isirCalcResultCollection = new Collection<IsirCalcResults>() { isirCalcResults };
                dataReaderMock.Setup(i => i.BulkReadRecordAsync<IsirCalcResults>(It.IsAny<string[]>(), true)).ReturnsAsync(isirCalcResultCollection);

                var isirResults = new IsirResults() { Recordkey = "2", IresSarCFlag = "Y", IresCpsPellElig = "Y", IresVerifFlag = "Y" };
                var isirResultsCollection = new Collection<IsirResults>() { isirResults };
                dataReaderMock.Setup(i => i.BulkReadRecordAsync<IsirResults>(It.IsAny<string[]>(), true)).ReturnsAsync(isirResultsCollection);

                var isirProfile = new IsirProfile() { };
                var isirProfileCollection = new Collection<IsirProfile>() { isirProfile };
                dataReaderMock.Setup(i => i.BulkReadRecordAsync<IsirProfile>(It.IsAny<string[]>(), true)).ReturnsAsync(isirProfileCollection);

                var faSystemParamsResponseData = new FaSysParams()
                {
                    FspInstitutionName = "Datatel Community College FA"
                };
                dataReaderMock.Setup<FaSysParams>(acc => acc.ReadRecord<FaSysParams>("ST.PARMS", "FA.SYS.PARAMS", true)).Returns(faSystemParamsResponseData);

                var faSuiteYears = new List<string>() { year };
                var tuple = await actualRepository.GetAsync(0, 1, false, null, "2014", null, null, faSuiteYears);
                
                var count = tuple.Item2;                
                Assert.AreEqual(0, count);
                
            }

            [TestMethod]
            public async Task FinancialAidApplicationOutcomeRepository_GetAsync_ValidFilterStudent()
            {
                var year = "2017";
                var studentId = "0002020";
                string guid = "db2cb4bf-9531-4d38-8bcf-a96bc8628156";
                GuidLookup[] lookup = new GuidLookup[] { new GuidLookup(guid) };
                var lookUpResults = new Dictionary<string, GuidLookupResult>();
                lookUpResults.Add("AR.INV.ITEMS.INTG", new GuidLookupResult() { Entity = "ISIR.CALC.RESULTS", PrimaryKey = "1", SecondaryKey = "Somekey" });
                dataReaderMock.Setup(i => i.SelectAsync(It.IsAny<GuidLookup[]>())).ReturnsAsync(lookUpResults);

                var fileSuiteYears = new string[] { year };
                dataReaderMock.Setup(i => i.SelectAsync("FA.SUITES", null)).ReturnsAsync(fileSuiteYears);

                var csAcyrIds = new string[] { "2" };
                dataReaderMock.Setup(i => i.SelectAsync("CS." + year, It.IsAny<string[]>(), It.IsAny<string>())).ReturnsAsync(csAcyrIds);
                dataReaderMock.Setup(i => i.SelectAsync("CS." + year, It.IsAny<string>())).ReturnsAsync(csAcyrIds);

                var csAcyr = new CsAcyr() { Recordkey = studentId, CsFedIsirId = "2", CsInstIsirId = "2", CsInstAdj = 25000 };
                var records = new Collection<CsAcyr>() { csAcyr };
                dataReaderMock.Setup(i => i.BulkReadRecordAsync<CsAcyr>("CS." + year, It.IsAny<string[]>(), true)).ReturnsAsync(records);

                var validApplicationIds = new string[] { "2" };
                dataReaderMock.Setup(i => i.SelectAsync("ISIR.FAFSA", It.IsAny<string[]>(), It.IsAny<string>())).ReturnsAsync(validApplicationIds);

                var isirFafsa = new IsirFafsa()
                {
                    RecordGuid = guid,
                    Recordkey = "2",
                    IfafImportYear = year,
                    IfafStudentId = studentId,
                    IfafIsirType = "PROF",
                    IfafSLegalRes = "NY",
                    IfafFatherGradeLvl = "3",
                    IfafMotherGradeLvl = "3",
                    IfafMarried = "Y",
                    IfafDependChildren = "N",
                    IfafOtherDepend = "N",
                    IfafHousing1 = "1",
                    IfafHousing2 = "2",
                    IfafHousing3 = "3",
                    IfafHousing4 = "4",
                    IfafHousing5 = "5",
                    IfafHousing6 = "6",
                    IfafHousing7 = "7",
                    IfafHousing8 = "8",
                    IfafHousing9 = "9",
                    IfafHousing10 = "10",
                    IfafActiveDuty = "N",
                    IfafHomelessAtRisk = "N",
                    IfafFaaAdj = "Y"

                };
                var isirFafsaCollection = new Collection<IsirFafsa>() { isirFafsa };
                dataReaderMock.Setup(i => i.BulkReadRecordAsync<IsirFafsa>(It.IsAny<string[]>(), true)).ReturnsAsync(isirFafsaCollection);

                var isirCalcResults = new IsirCalcResults()
                {
                    RecordGuid = new Guid().ToString(),
                    Recordkey = "2",
                    IcresDependency = "I",
                    IcresAzeInd = "Y",
                    IcresSimpleNeedInd = "N",
                    IcresPriEfc = 444,
                    IcresPriFti = 52000,
                    IcresInstEfcOvrAmt = 50
                };
                var isirCalcResultCollection = new Collection<IsirCalcResults>() { isirCalcResults };
                dataReaderMock.Setup(i => i.BulkReadRecordAsync<IsirCalcResults>(It.IsAny<string[]>(), true)).ReturnsAsync(isirCalcResultCollection);

                var isirResults = new IsirResults() { Recordkey = "2", IresSarCFlag = "Y", IresCpsPellElig = "Y", IresVerifFlag = "Y" };
                var isirResultsCollection = new Collection<IsirResults>() { isirResults };
                dataReaderMock.Setup(i => i.BulkReadRecordAsync<IsirResults>(It.IsAny<string[]>(), true)).ReturnsAsync(isirResultsCollection);

                var isirProfile = new IsirProfile() { };
                var isirProfileCollection = new Collection<IsirProfile>() { isirProfile };
                dataReaderMock.Setup(i => i.BulkReadRecordAsync<IsirProfile>(It.IsAny<string[]>(), true)).ReturnsAsync(isirProfileCollection);

                var faSystemParamsResponseData = new FaSysParams()
                {
                    FspInstitutionName = "Datatel Community College FA"
                };
                dataReaderMock.Setup<FaSysParams>(acc => acc.ReadRecord<FaSysParams>("ST.PARMS", "FA.SYS.PARAMS", true)).Returns(faSystemParamsResponseData);

                var faSuiteYears = new List<string>() { year };
                var tuple = await actualRepository.GetAsync(0, 1, false, "0002020", null, null, null, faSuiteYears);
                
                Assert.IsNotNull(tuple);
                var fafsa = tuple.Item1.ToList();
                var count = tuple.Item2;
                Assert.AreEqual(1, count);

            }

            [TestMethod]
            public async Task FinancialAidApplicationOutcomeRepository_GetAsync_InvalidFilterStudent()
            {
                var year = "2017";
                var studentId = "0002020";
                string guid = "db2cb4bf-9531-4d38-8bcf-a96bc8628156";
                GuidLookup[] lookup = new GuidLookup[] { new GuidLookup(guid) };
                var lookUpResults = new Dictionary<string, GuidLookupResult>();
                lookUpResults.Add("AR.INV.ITEMS.INTG", new GuidLookupResult() { Entity = "ISIR.CALC.RESULTS", PrimaryKey = "1", SecondaryKey = "Somekey" });
                dataReaderMock.Setup(i => i.SelectAsync(It.IsAny<GuidLookup[]>())).ReturnsAsync(lookUpResults);

                var fileSuiteYears = new string[] { year };
                dataReaderMock.Setup(i => i.SelectAsync("FA.SUITES", null)).ReturnsAsync(fileSuiteYears);

                var csAcyrIds = new string[] { "2" };
                dataReaderMock.Setup(i => i.SelectAsync("CS." + year, It.IsAny<string>())).ReturnsAsync(csAcyrIds);

                var csAcyr = new CsAcyr() { Recordkey = studentId, CsFedIsirId = "2", CsInstIsirId = "2", CsInstAdj = 25000 };
                var records = new Collection<CsAcyr>() { csAcyr };
                dataReaderMock.Setup(i => i.BulkReadRecordAsync<CsAcyr>("CS." + year, It.IsAny<string[]>(), true)).ReturnsAsync(records);

                var validApplicationIds = new string[] { "2" };
                dataReaderMock.Setup(i => i.SelectAsync("ISIR.FAFSA", It.IsAny<string[]>(), It.IsAny<string>())).ReturnsAsync(validApplicationIds);

                var isirFafsa = new IsirFafsa()
                {
                    RecordGuid = guid,
                    Recordkey = "2",
                    IfafImportYear = year,
                    IfafStudentId = studentId,
                    IfafIsirType = "PROF",
                    IfafSLegalRes = "NY",
                    IfafFatherGradeLvl = "3",
                    IfafMotherGradeLvl = "3",
                    IfafMarried = "Y",
                    IfafDependChildren = "N",
                    IfafOtherDepend = "N",
                    IfafHousing1 = "1",
                    IfafHousing2 = "2",
                    IfafHousing3 = "3",
                    IfafHousing4 = "4",
                    IfafHousing5 = "5",
                    IfafHousing6 = "6",
                    IfafHousing7 = "7",
                    IfafHousing8 = "8",
                    IfafHousing9 = "9",
                    IfafHousing10 = "10",
                    IfafActiveDuty = "N",
                    IfafHomelessAtRisk = "N",
                    IfafFaaAdj = "Y"

                };
                var isirFafsaCollection = new Collection<IsirFafsa>() { isirFafsa };
                dataReaderMock.Setup(i => i.BulkReadRecordAsync<IsirFafsa>(It.IsAny<string[]>(), true)).ReturnsAsync(isirFafsaCollection);

                var isirCalcResults = new IsirCalcResults()
                {
                    RecordGuid = new Guid().ToString(),
                    Recordkey = "2",
                    IcresDependency = "I",
                    IcresAzeInd = "Y",
                    IcresSimpleNeedInd = "N",
                    IcresPriEfc = 444,
                    IcresPriFti = 52000,
                    IcresInstEfcOvrAmt = 50
                };
                var isirCalcResultCollection = new Collection<IsirCalcResults>() { isirCalcResults };
                dataReaderMock.Setup(i => i.BulkReadRecordAsync<IsirCalcResults>(It.IsAny<string[]>(), true)).ReturnsAsync(isirCalcResultCollection);

                var isirResults = new IsirResults() { Recordkey = "2", IresSarCFlag = "Y", IresCpsPellElig = "Y", IresVerifFlag = "Y" };
                var isirResultsCollection = new Collection<IsirResults>() { isirResults };
                dataReaderMock.Setup(i => i.BulkReadRecordAsync<IsirResults>(It.IsAny<string[]>(), true)).ReturnsAsync(isirResultsCollection);

                var isirProfile = new IsirProfile() { };
                var isirProfileCollection = new Collection<IsirProfile>() { isirProfile };
                dataReaderMock.Setup(i => i.BulkReadRecordAsync<IsirProfile>(It.IsAny<string[]>(), true)).ReturnsAsync(isirProfileCollection);

                var faSystemParamsResponseData = new FaSysParams()
                {
                    FspInstitutionName = "Datatel Community College FA"
                };
                dataReaderMock.Setup<FaSysParams>(acc => acc.ReadRecord<FaSysParams>("ST.PARMS", "FA.SYS.PARAMS", true)).Returns(faSystemParamsResponseData);

                var response = new Ellucian.Colleague.Domain.Base.Transactions.GetCacheApiKeysResponse()
                {
                    Offset = 0,
                    Limit = 1,
                    CacheName = "AllFinancialAidApplicationOutcomesCacheKey:",
                    Entity = "",
                    Sublist = new List<string>(),
                    TotalCount = 0,
                    KeyCacheInfo = new List<KeyCacheInfo>()
                {
                    new KeyCacheInfo()
                    {
                        KeyCacheMax = 5905,
                        KeyCacheMin = 1,
                        KeyCachePart = "000",
                        KeyCacheSize = 5905
                    },
                    new KeyCacheInfo()
                    {
                        KeyCacheMax = 7625,
                        KeyCacheMin = 5906,
                        KeyCachePart = "001",
                        KeyCacheSize = 1720
                    }
                }
                };
                transManagerMock.Setup(acc => acc.ExecuteAsync<GetCacheApiKeysRequest,
                    GetCacheApiKeysResponse>(It.IsAny<GetCacheApiKeysRequest>())).ReturnsAsync(response);


                var faSuiteYears = new List<string>() { year };
                var tuple = await actualRepository.GetAsync(0, 1, false, "0000001", null, null, null, faSuiteYears);

                var count = tuple.Item2;
                Assert.AreEqual(0, count);
            }

            [TestMethod]
            [ExpectedException(typeof(RepositoryException))]
            public async Task FinancialAidApplicationOutcomeRepository_GetAsync_MissingFafsaGuid()
            {
                var year = "2017";
                var studentId = "0002020";
                string guid = "db2cb4bf-9531-4d38-8bcf-a96bc8628156";
                GuidLookup[] lookup = new GuidLookup[] { new GuidLookup(guid) };
                var lookUpResults = new Dictionary<string, GuidLookupResult>();
                lookUpResults.Add("AR.INV.ITEMS.INTG", new GuidLookupResult() { Entity = "ISIR.CALC.RESULTS", PrimaryKey = "1", SecondaryKey = "Somekey" });
                dataReaderMock.Setup(i => i.SelectAsync(It.IsAny<GuidLookup[]>())).ReturnsAsync(lookUpResults);

                var fileSuiteYears = new string[] { year };
                dataReaderMock.Setup(i => i.SelectAsync("FA.SUITES", null)).ReturnsAsync(fileSuiteYears);

                var csAcyrIds = new string[] { "2" };
                dataReaderMock.Setup(i => i.SelectAsync("CS." + year, It.IsAny<string>())).ReturnsAsync(csAcyrIds);

                var csAcyr = new CsAcyr() { Recordkey = studentId, CsFedIsirId = "2", CsInstIsirId = "2", CsInstAdj = 25000 };
                var records = new Collection<CsAcyr>() { csAcyr };
                dataReaderMock.Setup(i => i.BulkReadRecordAsync<CsAcyr>("CS." + year, It.IsAny<string[]>(), true)).ReturnsAsync(records);

                var validApplicationIds = new string[] { "2" };
                dataReaderMock.Setup(i => i.SelectAsync("ISIR.FAFSA", It.IsAny<string[]>(), It.IsAny<string>())).ReturnsAsync(validApplicationIds);

                var isirFafsa = new IsirFafsa()
                {
                    RecordGuid = "",
                    Recordkey = "2",
                    IfafImportYear = year,
                    IfafStudentId = studentId,
                    IfafIsirType = "PROF",
                    IfafSLegalRes = "NY",
                    IfafFatherGradeLvl = "3",
                    IfafMotherGradeLvl = "3",
                    IfafMarried = "Y",
                    IfafDependChildren = "N",
                    IfafOtherDepend = "N",
                    IfafHousing1 = "1",
                    IfafHousing2 = "2",
                    IfafHousing3 = "3",
                    IfafHousing4 = "4",
                    IfafHousing5 = "5",
                    IfafHousing6 = "6",
                    IfafHousing7 = "7",
                    IfafHousing8 = "8",
                    IfafHousing9 = "9",
                    IfafHousing10 = "10",
                    IfafActiveDuty = "N",
                    IfafHomelessAtRisk = "N",
                    IfafFaaAdj = "Y"

                };
                var isirFafsaCollection = new Collection<IsirFafsa>() { isirFafsa };
                dataReaderMock.Setup(i => i.BulkReadRecordAsync<IsirFafsa>(It.IsAny<string[]>(), true)).ReturnsAsync(isirFafsaCollection);

                var isirCalcResults = new IsirCalcResults()
                {
                    RecordGuid = new Guid().ToString(),
                    Recordkey = "2",
                    IcresDependency = "I",
                    IcresAzeInd = "Y",
                    IcresSimpleNeedInd = "N",
                    IcresPriEfc = 444,
                    IcresPriFti = 52000,
                    IcresInstEfcOvrAmt = 50
                };
                var isirCalcResultCollection = new Collection<IsirCalcResults>() { isirCalcResults };
                dataReaderMock.Setup(i => i.BulkReadRecordAsync<IsirCalcResults>(It.IsAny<string[]>(), true)).ReturnsAsync(isirCalcResultCollection);

                var isirResults = new IsirResults() { Recordkey = "2", IresSarCFlag = "Y", IresCpsPellElig = "Y", IresVerifFlag = "Y" };
                var isirResultsCollection = new Collection<IsirResults>() { isirResults };
                dataReaderMock.Setup(i => i.BulkReadRecordAsync<IsirResults>(It.IsAny<string[]>(), true)).ReturnsAsync(isirResultsCollection);

                var isirProfile = new IsirProfile() { };
                var isirProfileCollection = new Collection<IsirProfile>() { isirProfile };
                dataReaderMock.Setup(i => i.BulkReadRecordAsync<IsirProfile>(It.IsAny<string[]>(), true)).ReturnsAsync(isirProfileCollection);

                var faSystemParamsResponseData = new FaSysParams()
                {
                    FspInstitutionName = "Datatel Community College FA"
                };
                dataReaderMock.Setup<FaSysParams>(acc => acc.ReadRecord<FaSysParams>("ST.PARMS", "FA.SYS.PARAMS", true)).Returns(faSystemParamsResponseData);

                var faSuiteYears = new List<string>() { year };
                var tuple = await actualRepository.GetAsync(0, 1, false, null, null, null, null, faSuiteYears);
            }
        }
    }
}
