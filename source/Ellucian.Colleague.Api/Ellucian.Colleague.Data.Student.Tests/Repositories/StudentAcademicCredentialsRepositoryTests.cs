// Copyright 2019-2021 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Data.Base.DataContracts;
using Ellucian.Colleague.Data.Base.Tests.Repositories;
using Ellucian.Colleague.Data.Student.Repositories;
using Ellucian.Colleague.Domain.Base.Transactions;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Colleague.Domain.Student.Entities;
using Ellucian.Data.Colleague;
using Ellucian.Data.Colleague.DataContracts;
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
    public class StudentAcademicCredentialsRepositoryTests
    {
        [TestClass]
        public class StudentAcademicCredentialsRepository_GETALL_GETBYID : BaseRepositorySetup
        {
            #region DECLARATIONS

            Defaults defaults;
            //ApplValcodes applValCodes;
            Collection<AcadCredentials> acadCredentials;
            Collection<LdmGuid> ldmGuids;
            IEnumerable<StudentAcademicCredential> entities;
            private StudentAcademicCredentialsRepository _studentAcademicCredentialsRepository;
            Mock<IColleagueTransactionInvoker> mockManager = null;
            StudentAcademicCredential criteriaEntity;
            string[] filterPersonIds;
            string acadProgramFilter;
            Dictionary<string, string> filterQualifiers;
            Dictionary<string, string> dict;
            string[] guidList;

            private string guid = "1a49eed8-5fe7-4120-b1cf-f23266b9e874";

            #endregion

            #region TEST SETUP

            [TestInitialize]
            public void Initialize()
            {
                MockInitialize();

                InitializeTestData();

                InitializeTestMock();

                _studentAcademicCredentialsRepository = new StudentAcademicCredentialsRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object, apiSettings);
            }

            [TestCleanup]
            public void Cleanup()
            {
                MockCleanup();
                defaults = null;
                acadCredentials = null;
                entities = null;
                _studentAcademicCredentialsRepository = null;
                criteriaEntity = null;
                filterPersonIds = null;
                acadProgramFilter = null;
                filterQualifiers = null;
            }

            private void InitializeTestData()
            {
                defaults = new Defaults() { DefaultHostCorpId = "4" };
                criteriaEntity = new StudentAcademicCredential()
                {
                    StudentId = "1",
                    Degrees = new List<Tuple<string, DateTime?>>()
                    {
                       new Tuple<string, DateTime?>("BA", DateTime.Today.AddDays(-30))
                    },
                    Ccds = new List<Tuple<string, DateTime?>>()
                    {
                       new Tuple<string, DateTime?>("C1", DateTime.Today.AddDays(-30))
                    },
                    GraduatedOn = DateTime.Today.AddDays(-50),
                    AcademicLevel = "UG",
                    AcadAcadProgramId = "1",
                    AcademicPeriod = "2019/FA"
                };
                acadProgramFilter = "ARTH.AA";
                filterPersonIds = new string[] { "BMA" };

                acadCredentials = new Collection<AcadCredentials>()
                {
                    new AcadCredentials()
                    {
                        AcadAcadLevel = "UG",
                        AcadAcadProgram = "1",
                        AcadCcd = new List<string>()
                        {
                            "C1"
                        },
                        AcadCcdDate = new List<DateTime?>(){DateTime.Today.AddDays(-30) },
                        AcadPersonId = "1",
                        AcadDegree = "BA",
                        AcadMajors = new List<string>()
                        {
                            "ARTH", "HEBR"
                        },
                        AcadMinors = new List<string>()
                        {
                            "HIST"
                        },
                        AcadSpecialization = new List<string>()
                        {
                            "EDLD", "RELG"
                        },
                        AcadHonors = new List<string>(){"CL", "PB"},
                        AcadEndDate = DateTime.Today.AddDays(10),
                        AcadThesis = "Acad Thesis",
                        RecordGuid = guid,
                        Recordkey = "1", 
                        AcadInstitutionsId = "4"
                    }
                };

                entities = new List<StudentAcademicCredential>()
                {
                    new StudentAcademicCredential(guid, "1")
                    {
                        AcadAcadProgramId = "1",
                        AcadDisciplines = new List<string>(){ "ARTH", "HISTORY"},
                        AcademicLevel = "UG",
                        AcademicPeriod = "2019/FA",
                        AcadHonors = new List<string>(){ "CL", "PB" },
                        AcadPersonId = "1",
                        AcadTerm = "2019/FA",
                        AcadThesis = "Acad Thesis",
                        Ccds = new List<Tuple<string, DateTime?>>()
                        {
                             new Tuple<string, DateTime?>("C1", DateTime.Today.AddDays(-30))
                        },
                        Degrees = new List<Tuple<string, DateTime?>>()
                        {
                           new Tuple<string, DateTime?>("BA", DateTime.Today.AddDays(-30))
                        },
                        GraduatedOn = DateTime.Today.AddDays(-50),
                        StudentId = "1",
                        StudentProgramGuid = "1*1"
                    }
                };
                ldmGuids = new Collection<LdmGuid>()
                {
                    new LdmGuid()
                    {
                        Recordkey = guid,
                        LdmGuidPrimaryKey = "1",
                        LdmGuidSecondaryKey = "1"
                    }
                };

                dict = new Dictionary<string, string>();
                dict.Add("1", guid);
                guidList = new string[] { guid };
            }

            private void InitializeTestMock()
            {
                dataReaderMock.Setup(d => d.ReadRecordAsync<Defaults>("CORE.PARMS", "DEFAULTS", true)).ReturnsAsync(defaults);

                dataReaderMock.Setup(d => d.SelectAsync("INSTITUTIONS.ATTEND", It.IsAny<string[]>(), It.IsAny<string>())).ReturnsAsync(new[] { "1", "2" });
                dataReaderMock.Setup(d => d.SelectAsync("ACAD.CREDENTIALS", It.IsAny<string[]>(), It.IsAny<string>())).ReturnsAsync(new[] { "1", "2" });
                dataReaderMock.Setup(repo => repo.SelectAsync("LDM.GUID", It.IsAny<string>(), It.IsAny<string[]>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<int>()))
                .ReturnsAsync(guidList);
                dataReaderMock.Setup(d => d.BulkReadRecordAsync<AcadCredentials>(It.IsAny<string[]>(), It.IsAny<bool>())).ReturnsAsync(acadCredentials);
                dataReaderMock.Setup(d => d.BulkReadRecordAsync<LdmGuid>(It.IsAny<string[]>(), It.IsAny<bool>())).ReturnsAsync(ldmGuids);

                dataReaderMock.Setup(d => d.ReadRecordAsync<AcadCredentials>(It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(acadCredentials.FirstOrDefault());
                
                 mockManager = new Mock<IColleagueTransactionInvoker>();

                var dicResult = new Dictionary<string, GuidLookupResult>();
              
                dataReaderMock.Setup(acc => acc.SelectAsync(It.IsAny<RecordKeyLookup[]>())).Returns<RecordKeyLookup[]>(recordKeyLookups =>
                {
                    var result = new Dictionary<string, RecordKeyLookupResult>();
                    foreach (var recordKeyLookup in recordKeyLookups)
                    {
                        var record = acadCredentials.Where(e => e.Recordkey == recordKeyLookup.PrimaryKey).FirstOrDefault();
                        result.Add(string.Join("+", new string[] { "ACAD.CREDENTIALS", record.Recordkey }),
                            new RecordKeyLookupResult() { Guid = record.RecordGuid });
                    }
                    return Task.FromResult(result);
                });


                transFactoryMock.Setup(transFac => transFac.GetTransactionInvoker()).Returns(mockManager.Object);
                GetCacheApiKeysResponse resp = new GetCacheApiKeysResponse()
                {
                    Offset = 0,
                    Limit = 2,
                    CacheName = "AllStudentAcademicCredentials:",
                    Entity = "ACAD.CREDENTIALS",
                    Sublist = new List<string>() { "1" },
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
                mockManager.Setup(mgr => mgr.ExecuteAsync<GetCacheApiKeysRequest, GetCacheApiKeysResponse>(It.IsAny<GetCacheApiKeysRequest>()))
                    .ReturnsAsync(resp);
            }

            #endregion

            [TestMethod]
            public async Task GetStudentAcademicCredentialsAsync()
            {

                var result = await _studentAcademicCredentialsRepository.GetStudentAcademicCredentialsAsync(0, 100, criteriaEntity, filterPersonIds, acadProgramFilter, filterQualifiers);
                Assert.IsNotNull(result);
            }

            [TestMethod]
            public async Task GetStudentAcademicCredentialsAsync_AcadCredIds_Null()
            {
                dataReaderMock.Setup(d => d.SelectAsync("ACAD.CREDENTIALS", It.IsAny<string[]>(), It.IsAny<string>())).ReturnsAsync(() => null);
                dataReaderMock.Setup(d => d.SelectAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string[]>(), It.IsAny<string>(), true, It.IsAny<int>()))
                    .ReturnsAsync(new string[] { });
                
                
                GetCacheApiKeysResponse resp = new GetCacheApiKeysResponse()
                {
                    Offset = 0,
                    Limit = 2,
                    CacheName = "AllStudentAcademicCredentials:",
                    Entity = "ACAD.CREDENTIALS",
                    Sublist = new List<string>() {  },
                    TotalCount = 0,
                    KeyCacheInfo = new List<KeyCacheInfo>()
                
                };
                mockManager.Setup(mgr => mgr.ExecuteAsync<GetCacheApiKeysRequest, GetCacheApiKeysResponse>(It.IsAny<GetCacheApiKeysRequest>()))
                    .ReturnsAsync(resp);
                var result = await _studentAcademicCredentialsRepository.GetStudentAcademicCredentialsAsync(0, 100, criteriaEntity, filterPersonIds, acadProgramFilter, filterQualifiers);
                Assert.IsNotNull(result);
                Assert.AreEqual(0, result.Item2);
            }

            [TestMethod]
            [ExpectedException(typeof(RepositoryException))]
            public async Task GetStudentAcademicCredentialsAsync_RepositoryException_Collection()
            {
                acadCredentials.FirstOrDefault().AcadPersonId = "";
                var result = await _studentAcademicCredentialsRepository.GetStudentAcademicCredentialsAsync(0, 100, criteriaEntity, filterPersonIds, acadProgramFilter, filterQualifiers);
            }

            [TestMethod]
            [ExpectedException(typeof(RepositoryException))]
            public async Task GetStudentAcademicCredentialsAsync_RepositoryException()
            {
                dataReaderMock.Setup(d => d.BulkReadRecordAsync<LdmGuid>(It.IsAny<string[]>(), It.IsAny<bool>())).ThrowsAsync(new Exception());
                dataReaderMock.Setup(d => d.SelectAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string[]>(), It.IsAny<string>(), true, It.IsAny<int>()))
                    .ReturnsAsync(new string[] { });
                dataReaderMock.Setup(acc => acc.SelectAsync(It.IsAny<RecordKeyLookup[]>())).ThrowsAsync(new RepositoryException());

                var result = await _studentAcademicCredentialsRepository.GetStudentAcademicCredentialsAsync(0, 100, criteriaEntity, filterPersonIds, acadProgramFilter, filterQualifiers);
            }

            [TestMethod]
            [ExpectedException(typeof(RepositoryException))]
            public async Task GetStudentAcademicCredentialsAsync_DictionaryNull_RepositoryException()
            {
                dataReaderMock.Setup(d => d.BulkReadRecordAsync<LdmGuid>(It.IsAny<string[]>(), It.IsAny<bool>())).ReturnsAsync(() => null);
                dataReaderMock.Setup(d => d.SelectAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string[]>(), It.IsAny<string>(), true, It.IsAny<int>()))
                    .ReturnsAsync(new string[] { });
                dataReaderMock.Setup(acc => acc.SelectAsync(It.IsAny<RecordKeyLookup[]>())).ReturnsAsync(() => null);

                var result = await _studentAcademicCredentialsRepository.GetStudentAcademicCredentialsAsync(0, 100, criteriaEntity, filterPersonIds, acadProgramFilter, filterQualifiers);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task GetStudentAcademicCredentialByGuidAsync_ArgumentNullException()
            {
                var result = await _studentAcademicCredentialsRepository.GetStudentAcademicCredentialByGuidAsync("");
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task GetStudentAcademicCredentialByGuidAsync_KeyNotFoundException()
            {
                dataReaderMock.Setup(d => d.ReadRecordAsync<AcadCredentials>(It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(() => null);
                var result = await _studentAcademicCredentialsRepository.GetStudentAcademicCredentialByGuidAsync("1");
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task GetStudentAcademicCredentialByGuidAsync_HostCorpId_DifferentKeyNotFoundException()
            {
                acadCredentials.FirstOrDefault().AcadInstitutionsId = "IncorrectHostId";
                var result = await _studentAcademicCredentialsRepository.GetStudentAcademicCredentialByGuidAsync("1");
            }

            [TestMethod]
            [ExpectedException(typeof(RepositoryException))]
            public async Task GetStudentAcademicCredentialByGuidAsync_GetStAcadCredDictionaryAsync_RepositoryException()
            {
                dataReaderMock.Setup(d => d.BulkReadRecordAsync<LdmGuid>(It.IsAny<string[]>(), It.IsAny<bool>())).ThrowsAsync(new Exception());
                dataReaderMock.Setup(acc => acc.SelectAsync(It.IsAny<RecordKeyLookup[]>())).ThrowsAsync(new RepositoryException());
                var result = await _studentAcademicCredentialsRepository.GetStudentAcademicCredentialByGuidAsync("1");
            }

            [TestMethod]
            [ExpectedException(typeof(RepositoryException))]
            public async Task GetStudentAcademicCredentialByGuidAsync_DictionaryNull_RepositoryException()
            {
                dataReaderMock.Setup(d => d.BulkReadRecordAsync<LdmGuid>(It.IsAny<string[]>(), It.IsAny<bool>())).ReturnsAsync(() => null);
                dataReaderMock.Setup(acc => acc.SelectAsync(It.IsAny<RecordKeyLookup[]>())).ReturnsAsync(() => null);
                var result = await _studentAcademicCredentialsRepository.GetStudentAcademicCredentialByGuidAsync("1");
            }

            [TestMethod]
            [ExpectedException(typeof(RepositoryException))]
            public async Task GetStudentAcademicCredentialByGuidAsync_Collection_RepositoryException()
            {
                acadCredentials.FirstOrDefault().AcadPersonId = "";
                var result = await _studentAcademicCredentialsRepository.GetStudentAcademicCredentialByGuidAsync("1");
            }

            [TestMethod]
            public async Task GetStudentAcademicCredentialByGuidAsync()
            {
                var result = await _studentAcademicCredentialsRepository.GetStudentAcademicCredentialByGuidAsync("1");
                Assert.IsNotNull(result);
            }
        }
    }
}