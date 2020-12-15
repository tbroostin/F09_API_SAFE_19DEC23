// Copyright 2017-2020 Ellucian Company L.P. and its affiliates

using Ellucian.Colleague.Data.Base.Tests.Repositories;
using Ellucian.Colleague.Data.Student.DataContracts;
using Ellucian.Colleague.Data.Student.Repositories;
using Ellucian.Colleague.Domain.Base.Transactions;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Data.Colleague;
using Ellucian.Web.Http.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Data.Student.Tests.Repositories
{
    [TestClass]
    public class StudentFinancialAidApplicationRepositoryTests : BaseRepositorySetup
    {
        public StudentFinancialAidApplicationRepository actualRepository;

        public void ProfileApplicationRepositoyTestsInitialize()
        {
            MockInitialize();
        }

        [TestClass]
        public class GetStudentFinancialAidApplicationRepositoryTests : StudentFinancialAidApplicationRepositoryTests
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
            public async Task StudentFinancialAidApplicationRepository_ArgumentNullException()
            {
                string guid = "db2cb4bf-9531-4d38-8bcf-a96bc8628156";
                GuidLookup[] lookup = new GuidLookup[] { new GuidLookup(guid) };
                var lookUpResults = new Dictionary<string, GuidLookupResult>();
                lookUpResults.Add("AR.INV.ITEMS.INTG", new GuidLookupResult() { Entity = "ISIR.CALC.RESULTS", PrimaryKey = "1", SecondaryKey = "Somekey" });
                dataReaderMock.Setup(i => i.SelectAsync(It.IsAny<GuidLookup[]>())).ReturnsAsync(lookUpResults);
                //throw new KeyNotFoundException(string.Format("No FA application outcome records was found for guid {0}'. ", id));
                await actualRepository.GetByIdAsync("");
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task StudentFinancialAidApplicationRepository_NoFAapplicationOutcomeRecords()
            {
                string guid = "db2cb4bf-9531-4d38-8bcf-a96bc8628156";
                GuidLookup[] lookup = new GuidLookup[] { new GuidLookup(guid) };
                var lookUpResults = new Dictionary<string, GuidLookupResult>();
                lookUpResults.Add("AR.INV.ITEMS.INTG", new GuidLookupResult() { Entity = "ISIR.CALC.RESULTS", PrimaryKey = "1", SecondaryKey = "Somekey" });
                dataReaderMock.Setup(i => i.SelectAsync(It.IsAny<GuidLookup[]>())).ReturnsAsync(lookUpResults);
                //throw new KeyNotFoundException(string.Format("No FA application outcome records was found for guid {0}'. ", id));
                await actualRepository.GetByIdAsync("1");
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task StudentFinancialAidApplicationRepository_InvalidGetRecordInfoFromGuid()
            {
                string guid = "db2cb4bf-9531-4d38-8bcf-a96bc8628156";
                GuidLookup[] lookup = new GuidLookup[] { new GuidLookup(guid) };
                var lookUpResults = new Dictionary<string, GuidLookupResult>();
                lookUpResults.Add("AR.INV.ITEMS.INTG", new GuidLookupResult() { Entity = "INVALID", PrimaryKey = "1", SecondaryKey = "Somekey" });
                dataReaderMock.Setup(i => i.SelectAsync(It.IsAny<GuidLookup[]>())).ReturnsAsync(lookUpResults);
                //throw new KeyNotFoundException(string.Format("No FA application outcome records was found for guid {0}'. ", id));
                await actualRepository.GetByIdAsync("1");
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task StudentFinancialAidApplicationRepository_IsirCalcResults_NotFound()
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
            public async Task StudentFinancialAidApplicationRepository_GetByIdAsync()
            {
                var year = "2017";
                var studentId = "0002020";
                string guid = "db2cb4bf-9531-4d38-8bcf-a96bc8628156";
                GuidLookup[] lookup = new GuidLookup[] { new GuidLookup(guid) };
                var lookUpResults = new Dictionary<string, GuidLookupResult>();
                lookUpResults.Add("AR.INV.ITEMS.INTG", new GuidLookupResult() { Entity = "ISIR.FAFSA", PrimaryKey = "1", SecondaryKey = "Somekey" });
                dataReaderMock.Setup(i => i.SelectAsync(It.IsAny<GuidLookup[]>())).ReturnsAsync(lookUpResults);

                var isirCalcResults = new IsirCalcResults() { RecordGuid = guid, Recordkey = "1", IcresPriEfc = 444, IcresPriFti = 52000, IcresInstEfcOvrAmt = 50 };
                dataReaderMock.Setup(i => i.ReadRecordAsync<IsirCalcResults>("ISIR.CALC.RESULTS", "1", It.IsAny<bool>())).ReturnsAsync(isirCalcResults);

                //isirFafsa = await DataReader.ReadRecordAsync<IsirFafsa>("ISIR.FAFSA", recordInfo.PrimaryKey);
                var isirFafsa = new IsirFafsa()
                {
                    RecordGuid = guid,
                    Recordkey = "1",
                    IfafImportYear = year,
                    IfafStudentId = studentId,
                    IfafIsirType = "PROF"
                };
                dataReaderMock.Setup(i => i.ReadRecordAsync<IsirFafsa>("ISIR.FAFSA", "1", It.IsAny<bool>())).ReturnsAsync(isirFafsa);

                var csAcyrFile = string.Concat("CS.", year);
                var csAcyr = new CsAcyr() { Recordkey = "1", CsFedIsirId = "2", };
                dataReaderMock.Setup(i => i.ReadRecordAsync<CsAcyr>(csAcyrFile, studentId, It.IsAny<bool>())).ReturnsAsync(csAcyr);


                //if (isirFafsa.IfafIsirType == "PROF")
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


                var isirProfile = new IsirProfile();
                dataReaderMock.Setup(i => i.ReadRecordAsync<IsirProfile>("ISIR.PROFILE", "1", It.IsAny<bool>())).ReturnsAsync(isirProfile);


                //var faSysParams = await DataReader.ReadRecordAsync<FaSysParams>("ST.PARMS", "FA.SYS.PARAMS");
                var faSystemParamsResponseData = new FaSysParams()
                {
                    FspInstitutionName = "Datatel Community College FA"
                };
                dataReaderMock.Setup(acc => acc.ReadRecordAsync<FaSysParams>("ST.PARMS", "FA.SYS.PARAMS", true)).ReturnsAsync(faSystemParamsResponseData);


                var actual = await actualRepository.GetByIdAsync("1");

                Assert.IsNotNull(actual);
                Assert.AreEqual(year, actual.AwardYear);
                Assert.AreEqual(guid, actual.Guid);

                Assert.AreEqual("2", actual.CsFederalIsirId);
                Assert.AreEqual("2", actual.FafsaPrimaryId);
                Assert.AreEqual("PROF", actual.FafsaPrimaryType);

                Assert.AreEqual("2", actual.Id);
            }

            [TestMethod]
            public async Task StudentFinancialAidApplicationRepository_GetAsync_VerificationsTrue()
            {
                GetCacheApiKeysResponse resp = new GetCacheApiKeysResponse()
                {
                    Offset = 0,
                    Limit = 2,
                    CacheName = "AllFinancialAidApplicationsKeys:",
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
                transManagerMock.Setup( mgr => mgr.ExecuteAsync<GetCacheApiKeysRequest, GetCacheApiKeysResponse>( It.IsAny<GetCacheApiKeysRequest>() ) )
                    .ReturnsAsync( resp );
                var year = "2017";
                var studentId = "0002020";
                string guid = "db2cb4bf-9531-4d38-8bcf-a96bc8628156";
                GuidLookup[] lookup = new GuidLookup[] { new GuidLookup( guid ) };
                var lookUpResults = new Dictionary<string, GuidLookupResult>();
                lookUpResults.Add( "AR.INV.ITEMS.INTG", new GuidLookupResult() { Entity = "ISIR.FAFSA", PrimaryKey = "1", SecondaryKey = "Somekey" } );
                dataReaderMock.Setup( i => i.SelectAsync( It.IsAny<GuidLookup[]>() ) ).ReturnsAsync( lookUpResults );

                // var fileSuiteYears = await DataReader.SelectAsync("FA.SUITES", null);
                var fileSuiteYears = new string[] { year };
                dataReaderMock.Setup( i => i.SelectAsync( "FA.SUITES", null ) ).ReturnsAsync( fileSuiteYears );

                var csAcyrIds = new string[] { "2" };
                dataReaderMock.Setup( i => i.SelectAsync( "CS." + year, It.IsAny<string>() ) ).ReturnsAsync( csAcyrIds );

                var csAcyr = new CsAcyr() { Recordkey = studentId, CsFedIsirId = "2", CsInstIsirId = "2", CsInstAdj = 25000 };
                var records = new Collection<CsAcyr>() { csAcyr };
                dataReaderMock.Setup( i => i.BulkReadRecordAsync<CsAcyr>( "CS." + year, It.IsAny<string[]>(), true ) ).ReturnsAsync( records );

                var validApplicationIds = new string[] { "2" };
                dataReaderMock.Setup( i => i.SelectAsync( "ISIR.FAFSA", It.IsAny<string[]>(), It.IsAny<string>() ) ).ReturnsAsync( validApplicationIds );

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
                dataReaderMock.Setup( i => i.BulkReadRecordAsync<IsirFafsa>( It.IsAny<string[]>(), true ) ).ReturnsAsync( isirFafsaCollection );

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
                dataReaderMock.Setup( i => i.BulkReadRecordAsync<IsirCalcResults>( It.IsAny<string[]>(), true ) ).ReturnsAsync( isirCalcResultCollection );

                var isirProfile = new IsirProfile() { };
                var isirProfileCollection = new Collection<IsirProfile>() { isirProfile };
                dataReaderMock.Setup( i => i.BulkReadRecordAsync<IsirProfile>( It.IsAny<string[]>(), true ) ).ReturnsAsync( isirProfileCollection );

                var faSystemParamsResponseData = new FaSysParams()
                {
                    FspInstitutionName = "Datatel Community College FA"
                };
                dataReaderMock.Setup<FaSysParams>( acc => acc.ReadRecord<FaSysParams>( "ST.PARMS", "FA.SYS.PARAMS", true ) ).Returns( faSystemParamsResponseData );
                dataReaderMock.Setup( acc => acc.ReadRecordAsync<FaSysParams>( "ST.PARMS", "FA.SYS.PARAMS", true ) ).ReturnsAsync( faSystemParamsResponseData );


                var faSuiteYears = new List<string>() { year };
                var tuple = await actualRepository.GetAsync( 0, 1, false, "1", year, faSuiteYears, "institutional", "ISIR" );

                Assert.IsNotNull( tuple );
                var fafsa = tuple.Item1.ToList();
                var count = tuple.Item2;
                Assert.AreEqual( 1, count );

                var actual = fafsa.FirstOrDefault();

                Assert.AreEqual( year, actual.AwardYear );
                Assert.AreEqual( guid, actual.Guid );

                Assert.AreEqual( "2", actual.CsFederalIsirId );
                Assert.AreEqual( "2", actual.FafsaPrimaryId );
                Assert.AreEqual( "PROF", actual.FafsaPrimaryType );

                Assert.AreEqual( "2", actual.Id );
            }

            [TestMethod]
            public async Task StudentFinancialAidApplicationRepository_GetAsync_VerificationsFalse()
            {
                GetCacheApiKeysResponse resp = new GetCacheApiKeysResponse()
                {
                    Offset = 0,
                    Limit = 2,
                    CacheName = "AllFinancialAidApplicationsKeys:",
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
                transManagerMock.Setup( mgr => mgr.ExecuteAsync<GetCacheApiKeysRequest, GetCacheApiKeysResponse>( It.IsAny<GetCacheApiKeysRequest>() ) )
                    .ReturnsAsync( resp );
                var year = "2017";
                var studentId = "0002020";
                string guid = "db2cb4bf-9531-4d38-8bcf-a96bc8628156";
                GuidLookup[] lookup = new GuidLookup[] { new GuidLookup( guid ) };
                var lookUpResults = new Dictionary<string, GuidLookupResult>();
                lookUpResults.Add( "AR.INV.ITEMS.INTG", new GuidLookupResult() { Entity = "ISIR.FAFSA", PrimaryKey = "1", SecondaryKey = "Somekey" } );
                dataReaderMock.Setup( i => i.SelectAsync( It.IsAny<GuidLookup[]>() ) ).ReturnsAsync( lookUpResults );

                // var fileSuiteYears = await DataReader.SelectAsync("FA.SUITES", null);
                var fileSuiteYears = new string[] { year };
                dataReaderMock.Setup( i => i.SelectAsync( "FA.SUITES", null ) ).ReturnsAsync( fileSuiteYears );


                // var csAcyrIds = await DataReader.SelectAsync("CS." + year, "WITH CS.FED.ISIR.ID NE '' OR WITH CS.INST.ISIR.ID NE ''");
                var csAcyrIds = new string[] { "2" };
                dataReaderMock.Setup( i => i.SelectAsync( "CS." + year, It.IsAny<string>() ) ).ReturnsAsync( csAcyrIds );

                //var records = await DataReader.BulkReadRecordAsync<CsAcyr>("CS." + year, subList.ToArray());
                var csAcyr = new CsAcyr() { Recordkey = studentId, CsFedIsirId = "2", CsInstIsirId = "2", CsInstAdj = 25000 };
                var records = new Collection<CsAcyr>() { csAcyr };
                dataReaderMock.Setup( i => i.BulkReadRecordAsync<CsAcyr>( "CS." + year, It.IsAny<string[]>(), true ) ).ReturnsAsync( records );

                var validApplicationIds = new string[] { "2" };
                dataReaderMock.Setup( i => i.SelectAsync( "ISIR.FAFSA", It.IsAny<string[]>(), It.IsAny<string>() ) ).ReturnsAsync( validApplicationIds );
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
                    IfafFaaAdj = "N"

                };
                var isirFafsaCollection = new Collection<IsirFafsa>() { isirFafsa };
                dataReaderMock.Setup( i => i.BulkReadRecordAsync<IsirFafsa>( It.IsAny<string[]>(), true ) ).ReturnsAsync( isirFafsaCollection );
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
                dataReaderMock.Setup( i => i.BulkReadRecordAsync<IsirCalcResults>( It.IsAny<string[]>(), true ) ).ReturnsAsync( isirCalcResultCollection );

                var isirProfile = new IsirProfile() { };
                var isirProfileCollection = new Collection<IsirProfile>() { isirProfile };
                dataReaderMock.Setup( i => i.BulkReadRecordAsync<IsirProfile>( It.IsAny<string[]>(), true ) ).ReturnsAsync( isirProfileCollection );
                //var IsirProfileRecords = await DataReader.BulkReadRecordAsync<IsirProfile>(effectiveApplicationSubList);

                var faSystemParamsResponseData = new FaSysParams()
                {
                    FspInstitutionName = "Datatel Community College FA"
                };
                dataReaderMock.Setup<FaSysParams>( acc => acc.ReadRecord<FaSysParams>( "ST.PARMS", "FA.SYS.PARAMS", true ) ).Returns( faSystemParamsResponseData );
                dataReaderMock.Setup( acc => acc.ReadRecordAsync<FaSysParams>( "ST.PARMS", "FA.SYS.PARAMS", true ) ).ReturnsAsync( faSystemParamsResponseData );

                var faSuiteYears = new List<string>() { year };
                var tuple = await actualRepository.GetAsync( 0, 1, false, It.IsAny<string>(), "", faSuiteYears );

                Assert.IsNotNull( tuple );
                var fafsa = tuple.Item1.ToList();
                var count = tuple.Item2;
                Assert.AreEqual( 1, count );

                var actual = fafsa.FirstOrDefault();

                Assert.AreEqual( year, actual.AwardYear );
                Assert.AreEqual( guid, actual.Guid );

                Assert.AreEqual( "2", actual.CsFederalIsirId );
                Assert.AreEqual( "2", actual.FafsaPrimaryId );
                Assert.AreEqual( "PROF", actual.FafsaPrimaryType );

                Assert.AreEqual( "2", actual.Id );

                // Assert.AreEqual(false, actual.HasFinancialAidAAministratorAdjustment);
                Assert.AreEqual( false, actual.HasStudentAidReportC );
                Assert.AreEqual( false, actual.IsPellEligible );
                Assert.AreEqual( false, actual.HasVerificationSelection );

            }


            [TestMethod]
            public async Task StudentFinancialAidApplicationRepository_GetAsync_InvalidIsirFafsa()
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


                // var csAcyrIds = await DataReader.SelectAsync("CS." + year, "WITH CS.FED.ISIR.ID NE '' OR WITH CS.INST.ISIR.ID NE ''");
                var csAcyrIds = new string[] { "2" };
                dataReaderMock.Setup(i => i.SelectAsync("CS." + year, "WITH CS.FED.ISIR.ID NE '' OR WITH CS.INST.ISIR.ID NE ''")).ReturnsAsync(csAcyrIds);

                //var records = await DataReader.BulkReadRecordAsync<CsAcyr>("CS." + year, subList.ToArray());
                var csAcyr = new CsAcyr() { Recordkey = studentId, CsFedIsirId = "2", CsInstIsirId = "2", CsInstAdj = 25000 };
                var records = new Collection<CsAcyr>() { csAcyr };
                dataReaderMock.Setup(i => i.BulkReadRecordAsync<CsAcyr>("CS." + year, It.IsAny<string[]>(), true)).ReturnsAsync(records);

                var validApplicationIds = new string[] { "2" };
                dataReaderMock.Setup(i => i.SelectAsync("ISIR.FAFSA", "WITH IFAF.STUDENT.ID NE '' AND WITH IFAF.IMPORT.YEAR NE ''")).ReturnsAsync(validApplicationIds);
                // var validApplicationIds = await DataReader.SelectAsync("ISIR.FAFSA", "WITH IFAF.STUDENT.ID NE '' AND WITH IFAF.IMPORT.YEAR NE ''");

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
                var result = await actualRepository.GetAsync(0, 1, false, It.IsAny<string>(), It.IsAny<string>(), faSuiteYears);
                Assert.IsNotNull( result );
                Assert.AreEqual( 0, result.Item2 );
            }


            [TestMethod]
            [ExpectedException(typeof( RepositoryException ) )]
            public async Task StudentFinancialAidApplicationRepository_GetAsync_InvalidYear()
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
                dataReaderMock.Setup(i => i.SelectAsync("CS." + year, It.IsAny<string>())).ReturnsAsync(null);

                //var records = await DataReader.BulkReadRecordAsync<CsAcyr>("CS." + year, subList.ToArray());
                var csAcyr = new CsAcyr() { Recordkey = studentId, CsFedIsirId = "2", CsInstIsirId = "2", CsInstAdj = 25000 };
                var records = new Collection<CsAcyr>() { csAcyr };
                dataReaderMock.Setup(i => i.BulkReadRecordAsync<CsAcyr>("CS." + year, It.IsAny<string[]>(), true)).ReturnsAsync(records);

                var validApplicationIds = new string[] { "2" };
                dataReaderMock.Setup(i => i.SelectAsync("ISIR.FAFSA", It.IsAny<string[]>(), It.IsAny<string>())).ReturnsAsync(null);
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
                var tuple = await actualRepository.GetAsync(0, 1, false, It.IsAny<string>(), It.IsAny<string>(), faSuiteYears);
            }

            [TestMethod]
            public async Task StudentFinancialAidApplicationRepository_GetAsync_FfaSuiteYearsNull()
            {
                var tuple = await actualRepository.GetAsync(0, 1, false, It.IsAny<string>(), It.IsAny<string>(), null);
                Assert.AreEqual( 0, tuple.Item2 );
            }

            private StudentFinancialAidApplicationRepository BuildRepository()
            {
                
                ApiSettings apiSettings = new ApiSettings("null");
                return new StudentFinancialAidApplicationRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object, apiSettings);
            }
        }
    }
}
