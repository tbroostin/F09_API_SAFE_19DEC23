// Copyright 2016-2021 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Data.Base.Tests.Repositories;
using Ellucian.Colleague.Data.Student.DataContracts;
using Ellucian.Colleague.Data.Student.Repositories;
using Ellucian.Colleague.Domain.Student.Entities;
using Ellucian.Data.Colleague;
using Ellucian.Web.Cache;
using Ellucian.Web.Http.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CampusOrgEntity = Ellucian.Colleague.Domain.Student.Entities.CampusOrganization;
using Ellucian.Colleague.Domain.Base.Transactions;

namespace Ellucian.Colleague.Data.Student.Tests.Repositories
{
    [TestClass]
    public class CampusOrganizationRepositoryTests : BaseRepositorySetup
    {
        protected void MainInitialize()
        {
            base.MockInitialize();
        }

        [TestClass]
        public class CampusOrgMember_GET : BaseRepositorySetup
        {
            CampusOrganizationRepository campusOrganizationRepository;
            List<CampusOrgMembers> CampusOrgMembers;

            [TestInitialize]
            public void Initialize()
            {
                MockInitialize();
                loggerMock = new Mock<ILogger>();
                CampusOrgMembers = BuildCampusOrgMembers();
                campusOrganizationRepository = BuildCampusOrganisationRepository();
                MockRecords<CampusOrgMembers>("CAMPUS.ORG.MEMBERS", CampusOrgMembers);
                dataReaderMock.Setup(r => r.SelectAsync("CAMPUS.ORG.MEMBERS", "WITH CMPM.PERSON.ST.ID EQ '?'", new string[] { "0014076" }, "?", true, 425)).ReturnsAsync(new string[] { "ART*0014076" });
            }

            [TestCleanup]
            public void Cleanup()
            {
                campusOrganizationRepository = null;
                CampusOrgMembers = null;
                transFactoryMock = null;
                MockCleanup();
            }

            [TestMethod]
            public async Task CampusOrganizationRepository_GetCampusOrgMemberAsync()
            {
                IEnumerable<string> userHrpId = new List<string>() { "0014076" };
                var results = await campusOrganizationRepository.GetCampusOrgMembersAsync(userHrpId);
                Assert.AreEqual(CampusOrgMembers.First().CampusOrgRolesEntityAssociation.First().CmpmRoleStartDatesAssocMember, results.First().StartDate);
            }

            [TestMethod]
            public async Task CampusOrganizationRepository_GetCampusOrgMemberAsyncWithWrongId()
            {
                IEnumerable<string> userHrpId = new List<string>() { "00" };
                var results = await campusOrganizationRepository.GetCampusOrgMembersAsync(userHrpId);
                Assert.AreEqual(0, results.Count());
            }


            private List<CampusOrgMembers> BuildCampusOrgMembers()
            {
                CampusOrgMembersCampusOrgRoles memberAssoc1 = new CampusOrgMembersCampusOrgRoles("AD", new DateTime(2016, 9, 1), new DateTime(2016, 9, 30), 100, "GOOD", "", 94,"D", "307","87");
                List<CampusOrgMembers> campusOrgList = new List<CampusOrgMembers>()
                {
                    new CampusOrgMembers() { CampusOrgRolesEntityAssociation = new List<CampusOrgMembersCampusOrgRoles>() {memberAssoc1}, Recordkey = "ART*0014076", RecordGuid = "1231233321"},
                };
                return campusOrgList;
            }

            private CampusOrganizationRepository BuildCampusOrganisationRepository()
            {
                // Construct repository
                campusOrganizationRepository = new CampusOrganizationRepository(cacheProvider, transFactory, logger, apiSettings);
                return campusOrganizationRepository;
            }
        }

        [TestClass]
        public class CampusOrgAdvisor_GET : BaseRepositorySetup
        {
            CampusOrganizationRepository campusOrganizationRepository;
            List<CampusOrgAdvisors> CampusOrgAdvisors;
            


            [TestInitialize]
            public void Initialize()
            {
                MockInitialize();
                loggerMock = new Mock<ILogger>();
                CampusOrgAdvisors = BuildCampusOrgAdvisors();
                campusOrganizationRepository = BuildCampusOrganisationRepository();
                MockRecords<CampusOrgAdvisors>("CAMPUS.ORG.ADVISORS", CampusOrgAdvisors);
                dataReaderMock.Setup(r => r.SelectAsync("CAMPUS.ORG.ADVISORS", "WITH CMPA.PERSON.ST.ID EQ '?'", new string[] { "0014076" }, "?", true, 425)).ReturnsAsync(new string[] { "ART*0014076" });
            }

            [TestCleanup]
            public void Cleanup()
            {
                campusOrganizationRepository = null;
                CampusOrgAdvisors = null;                
                transFactoryMock = null;
                MockCleanup();
            }

            [TestMethod]
            public async Task CampusOrganizationRepository_GetCampusOrgAdvisorAsync()
            {
                IEnumerable<string> userHrpId= new List<string>() { "0014076" };
                var results = await campusOrganizationRepository.GetCampusOrgAdvisorsAsync(userHrpId);
                Assert.AreEqual(CampusOrgAdvisors.First().AdvisorLoadEntityAssociation.First().CmpaStartDatesAssocMember, results.First().StartDate);
            }

            [TestMethod]
            public async Task CampusOrganizationRepository_GetCampusOrgAdvisorAsyncWithWrongId()
            {
                IEnumerable<string> userHrpId = new List<string>() { "00" };
                var results = await campusOrganizationRepository.GetCampusOrgAdvisorsAsync(userHrpId);
                Assert.AreEqual(0, results.Count());
            }


            private List<CampusOrgAdvisors> BuildCampusOrgAdvisors()
            {
                CampusOrgAdvisorsAdvisorLoad advLoadAssoc1 = new CampusOrgAdvisorsAdvisorLoad(new DateTime(2016, 9, 1), new DateTime(2016, 9, 30), 100, "AD", 260, "T", "297", "77");
                List<CampusOrgAdvisors> campusOrgList = new List<CampusOrgAdvisors>()
                {
                    new CampusOrgAdvisors() { AdvisorLoadEntityAssociation = new List<CampusOrgAdvisorsAdvisorLoad>() {advLoadAssoc1}, Recordkey = "ART*0014076"},
                };
                return campusOrgList;
            }

            private CampusOrganizationRepository BuildCampusOrganisationRepository()
            {
                // Construct repository
                campusOrganizationRepository = new CampusOrganizationRepository(cacheProvider, transFactory, logger, apiSettings);
                return campusOrganizationRepository;
            }
        }

        [TestClass]
        public class CampusOrganization_GET
        {
            Mock<IColleagueTransactionFactory> transFactoryMock;
            Mock<ICacheProvider> cacheProviderMock;
            Mock<IColleagueDataReader> dataAccessorMock;
            Mock<ILogger> loggerMock;

            CampusOrganizationRepository campusOrganizationRepository;
            List<CampusOrganization> CampusOrganizations;
            ApiSettings apiSettings;


            [TestInitialize]
            public void Initialize()
            {
                loggerMock = new Mock<ILogger>();

                CampusOrganizations = BuildCampusOrganisations();

                campusOrganizationRepository = BuildCampusOrganisationRepository();
            }

            [TestCleanup]
            public void Cleanup()
            {
                campusOrganizationRepository = null;
                CampusOrganizations = null;
                apiSettings = null;
                transFactoryMock = null;
                cacheProviderMock = null;
                dataAccessorMock = null;
                loggerMock = null;
            }

            [TestMethod]
            public async Task CampusOrganizationRepository_GetCampusOrganizationsAsync_GET_All()
            {
                var results = await campusOrganizationRepository.GetCampusOrganizationsAsync(It.IsAny<bool>());

                Assert.AreEqual(4, results.Count());
                foreach (var result in results)
                {
                    var expected = CampusOrganizations.FirstOrDefault(i => i.Guid.Equals(result.Guid, StringComparison.OrdinalIgnoreCase));
                    Assert.IsNotNull(expected);

                    Assert.AreEqual(expected.CampusOrganizationTypeId, result.CampusOrganizationTypeId);
                    Assert.AreEqual(expected.Code, result.Code);
                    Assert.AreEqual(expected.Description, result.Description);
                    Assert.AreEqual(expected.ParentOrganizationId, result.ParentOrganizationId);
                }
            }

            private List<CampusOrganization> BuildCampusOrganisations()
            {
                List<CampusOrganization> campusOrgList = new List<CampusOrganization>() 
                { 
                    new CampusOrgEntity("1", "d190d4b5-03b5-41aa-99b8-b8286717c956", "Assoc for Computing MacHinery", "1", "ACAD"),
                    new CampusOrgEntity("2", "2d37defe-6c88-4c06-bd37-17242956424e", "Alpha Kappa Lamdba", "2", "GREK"),
                    new CampusOrgEntity("3", "cecdce5a-54a7-45fb-a975-5392a579e5bf", "Art Club", "", "FNAR"),
                    new CampusOrgEntity("4", "038179c8-8d34-4c94-99e8-e2a53bca0305", "Bacon Lovers Of Ellucian Univ", "4", "SOCI"),
                };
                return campusOrgList;
            }

            private CampusOrganizationRepository BuildCampusOrganisationRepository()
            {
                // transaction factory mock
                transFactoryMock = new Mock<IColleagueTransactionFactory>();

                // Cache Provider Mock
                cacheProviderMock = new Mock<ICacheProvider>();

                // Set up data accessor for mocking 
                dataAccessorMock = new Mock<IColleagueDataReader>();
                apiSettings = new ApiSettings("TEST");

                // Set up dataAccessorMock as the object for the DataAccessor
                transFactoryMock.Setup(transFac => transFac.GetDataReader()).Returns(dataAccessorMock.Object);

                var records = new Collection<DataContracts.CampusOrgs>();
                foreach (var item in CampusOrganizations)
                {
                    DataContracts.CampusOrgs record = new DataContracts.CampusOrgs();
                    record.Recordkey = item.Code;
                    record.RecordGuid = item.Guid;
                    record.CmpDesc = item.Description;
                    record.CmpCorpId = item.ParentOrganizationId;
                    record.CmpOrgType = item.CampusOrganizationTypeId;
                    records.Add(record);
                }
                dataAccessorMock.Setup(acc => acc.BulkReadRecordAsync<DataContracts.CampusOrgs>("CAMPUS.ORGS", "", true)).ReturnsAsync(records);

                cacheProviderMock.Setup<Task<Tuple<object, SemaphoreSlim>>>(x =>
                 x.GetAndLockSemaphoreAsync(It.IsAny<string>(), null))
                 .ReturnsAsync(new Tuple<object, SemaphoreSlim>(null, new SemaphoreSlim(1, 1)));

                dataAccessorMock.Setup(acc => acc.SelectAsync(It.IsAny<RecordKeyLookup[]>())).Returns<RecordKeyLookup[]>(recordKeyLookups =>
                {
                    var result = new Dictionary<string, RecordKeyLookupResult>();
                    foreach (var recordKeyLookup in recordKeyLookups)
                    {
                        var record = CampusOrganizations.Where(e => e.Code == recordKeyLookup.PrimaryKey).FirstOrDefault();
                        result.Add(string.Join("+", new string[] { "CAMPUS.ORGS", record.Code }),
                            new RecordKeyLookupResult() { Guid = record.Guid });
                    }
                    return Task.FromResult(result);
                });

                // Construct repository
                campusOrganizationRepository = new CampusOrganizationRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object, apiSettings);

                return campusOrganizationRepository;
            }
        }

        [TestClass]
        public class CampusInvolvements_GET
        {
            Mock<IColleagueTransactionFactory> transFactoryMock;
            Mock<IColleagueTransactionInvoker> transManagerMock;
            Mock<ICacheProvider> cacheProviderMock;
            Mock<IColleagueDataReader> dataAccessorMock;
            Mock<ILogger> loggerMock;

            CampusOrganizationRepository campusOrganizationRepository;
            List<CampusInvolvement> campusInvolvementEntities;
            ApiSettings apiSettings;

            int offset = 0;
            int limit = 200;

            [TestInitialize]
            public void Initialize()
            {
                loggerMock = new Mock<ILogger>();

                BuildData();

                campusOrganizationRepository = BuildCampusOrganisationRepository();

                transManagerMock = new Mock<IColleagueTransactionInvoker>();
            }

            [TestCleanup]
            public void Cleanup()
            {
                campusOrganizationRepository = null;
                campusInvolvementEntities = null;
                apiSettings = null;
                transFactoryMock = null;
                cacheProviderMock = null;
                dataAccessorMock = null;
                loggerMock = null;
            }

            [TestMethod]
            public async Task CampusInvolvements_GetCampusInvolvementsAsync()
            {
                var actuals = await campusOrganizationRepository.GetCampusInvolvementsAsync(offset, limit);
                
                Assert.AreEqual(campusInvolvementEntities.Count(), actuals.Item1.Count());

                foreach (var actual in actuals.Item1)
                {
                    var expected = campusInvolvementEntities.FirstOrDefault(i => i.CampusInvolvementId.Equals(actual.CampusInvolvementId, StringComparison.OrdinalIgnoreCase));
                    Assert.IsNotNull(expected);

                    Assert.AreEqual(expected.AcademicPeriodId, actual.AcademicPeriodId);
                    Assert.AreEqual(expected.CampusInvolvementId, actual.CampusInvolvementId);
                    Assert.AreEqual(expected.CampusOrganizationId, actual.CampusOrganizationId);
                    Assert.AreEqual(expected.EndOn, actual.EndOn);
                    Assert.AreEqual(expected.PersonId, actual.PersonId);
                    Assert.AreEqual(expected.RoleId, actual.RoleId);
                    Assert.AreEqual(expected.StartOn, actual.StartOn);
                }
            }

            [TestMethod]
            public async Task CampusInvolvements_GetGetCampusInvolvementByIdAsync()
            {
                var id = "bbd216fb-0fc5-4f44-ae45-42d3cdd1e89a";
                var actual = await campusOrganizationRepository.GetGetCampusInvolvementByIdAsync(id);

                Assert.IsNotNull(actual);

                var expected = campusInvolvementEntities.FirstOrDefault(i => i.CampusInvolvementId.Equals(actual.CampusInvolvementId, StringComparison.OrdinalIgnoreCase));
                Assert.IsNotNull(expected);

                Assert.AreEqual(expected.AcademicPeriodId, actual.AcademicPeriodId);
                Assert.AreEqual(expected.CampusInvolvementId, actual.CampusInvolvementId);
                Assert.AreEqual(expected.CampusOrganizationId, actual.CampusOrganizationId);
                Assert.AreEqual(expected.EndOn, actual.EndOn);
                Assert.AreEqual(expected.PersonId, actual.PersonId);
                Assert.AreEqual(expected.RoleId, actual.RoleId);
                Assert.AreEqual(expected.StartOn, actual.StartOn);
            }

            private void BuildData()
            {
                campusInvolvementEntities = new List<CampusInvolvement>() 
                {
                    new CampusInvolvement("bbd216fb-0fc5-4f44-ae45-42d3cdd1e89a", "1", "100")
                    {
                        AcademicPeriodId = string.Empty,
                        EndOn = new DateTime(2016, 11, 30),
                        RoleId = "AD",
                        StartOn = new DateTime(2016, 09, 03)
                    },
                    new CampusInvolvement("3f67b180-ce1d-4552-8d81-feb96b9fea5b", "1", "200")
                    {
                        AcademicPeriodId = string.Empty,
                        EndOn = new DateTime(2016, 11, 30),
                        RoleId = "AD",
                        StartOn = new DateTime(2016, 09, 01)
                    },
                    new CampusInvolvement("bf67e156-8f5d-402b-8101-81b0a2796873", "3", "300")
                    {
                        AcademicPeriodId = string.Empty,
                        EndOn = new DateTime(2016, 09, 30),
                        RoleId = null,
                        StartOn = new DateTime(2016, 04, 01)
                    },
                    new CampusInvolvement("0111d6ef-5a86-465f-ac58-4265a997c136", "3", "400")
                    {                        
                        AcademicPeriodId = string.Empty,
                        EndOn = new DateTime(2016, 06, 30),
                        RoleId = "ME",
                        StartOn = new DateTime(2016, 02, 01)

                    },
                };
            }

            private CampusOrganizationRepository BuildCampusOrganisationRepository()
            {
                // transaction factory mock
                transFactoryMock = new Mock<IColleagueTransactionFactory>();
                transManagerMock = new Mock<IColleagueTransactionInvoker>();

                // Cache Provider Mock
                cacheProviderMock = new Mock<ICacheProvider>();

                // Set up data accessor for mocking 
                dataAccessorMock = new Mock<IColleagueDataReader>();
                var campusInlvIds = new string[] { "1*100", "1*200", "3*300", "3*400" };
                dataAccessorMock.Setup(i => i.SelectAsync("CAMPUS.ORG.MEMBERS", It.IsAny<string>())).ReturnsAsync(campusInlvIds);
                apiSettings = new ApiSettings("TEST");

                // Set up dataAccessorMock as the object for the DataAccessor
                transFactoryMock.Setup(transFac => transFac.GetDataReader()).Returns(dataAccessorMock.Object);
                transFactoryMock.Setup(transFac => transFac.GetTransactionInvoker()).Returns(transManagerMock.Object);

                var records = new Collection<DataContracts.CampusOrgMembers>();
                foreach (var item in campusInvolvementEntities)
                {
                    DataContracts.CampusOrgMembers record = new DataContracts.CampusOrgMembers();
                    var key = string.Concat(item.CampusOrganizationId, "*", item.PersonId);
                    record.Recordkey = key;
                    record.RecordGuid = item.CampusInvolvementId;
                    record.CmpmEndDates = new List<DateTime?>() { item.EndOn };
                    record.CmpmStartDates = new List<DateTime?>() { item.StartOn };
                    record.CmpmRoles = new List<string>() { item.RoleId };
                    records.Add(record);
                }
                dataAccessorMock.Setup(acc => acc.BulkReadRecordAsync<DataContracts.CampusOrgMembers>("CAMPUS.ORG.MEMBERS", campusInlvIds, true)).ReturnsAsync(records);

                var invalidRecords = new Dictionary<string, string>();
                var results = new Ellucian.Data.Colleague.BulkReadOutput<DataContracts.CampusOrgMembers>()
                {
                    BulkRecordsRead = new Collection<CampusOrgMembers>() { records[0], records[1], records[2], records[3] },
                    InvalidRecords = invalidRecords,
                    InvalidKeys = new string[] { }
                };
                
                dataAccessorMock.Setup(d => d.BulkReadRecordWithInvalidKeysAndRecordsAsync<DataContracts.CampusOrgMembers>("CAMPUS.ORG.MEMBERS", It.IsAny<string[]>(), It.IsAny<bool>())).ReturnsAsync(results);

                var campInvl = records.FirstOrDefault();
                dataAccessorMock.Setup(acc => acc.ReadRecordAsync<DataContracts.CampusOrgMembers>("CAMPUS.ORG.MEMBERS", campInvl.Recordkey, true)).ReturnsAsync(campInvl);

                cacheProviderMock.Setup<Task<Tuple<object, SemaphoreSlim>>>(x =>
                 x.GetAndLockSemaphoreAsync(It.IsAny<string>(), null))
                 .ReturnsAsync(new Tuple<object, SemaphoreSlim>(null, new SemaphoreSlim(1, 1)));

                dataAccessorMock.Setup(acc => acc.SelectAsync(It.IsAny<GuidLookup[]>())).Returns<GuidLookup[]>(gla =>
                {
                    var result = new Dictionary<string, GuidLookupResult>();
                    foreach (var gl in gla)
                    {
                        var stuprog = records.FirstOrDefault(x => x.RecordGuid == gl.Guid);
                        result.Add(gl.Guid, stuprog == null ? null : new GuidLookupResult() { Entity = "CAMPUS.ORG.MEMBERS", PrimaryKey = stuprog.Recordkey });
                    }
                    return Task.FromResult(result);
                });


                var ids = campusInvolvementEntities.Select(x => x.CampusInvolvementId).ToList();
                GetCacheApiKeysResponse resp = new GetCacheApiKeysResponse()
                {
                    Offset = 0,
                    Limit = 100,
                    CacheName = "AllCampusInvolvements",
                    Entity = "",
                    Sublist = ids,
                    TotalCount = ids.Count,
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
                
                transManagerMock.Setup(mgr => mgr.ExecuteAsync<GetCacheApiKeysRequest, GetCacheApiKeysResponse>(It.IsAny<GetCacheApiKeysRequest>()))
                    .ReturnsAsync(resp);

                // Construct repository
                campusOrganizationRepository = new CampusOrganizationRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object, apiSettings);
                
                return campusOrganizationRepository;
            }
        }

        [TestClass]
        public class CampusOrganization2_GET
        {
            Mock<IColleagueTransactionFactory> transFactoryMock;
            Mock<ICacheProvider> cacheProviderMock;
            Mock<IColleagueDataReader> dataAccessorMock;
            Mock<ILogger> loggerMock;

            CampusOrganizationRepository campusOrganizationRepository;
            List<CampusOrganization2> CampusOrganization2Records;
            ApiSettings apiSettings;


            [TestInitialize]
            public void Initialize()
            {
                loggerMock = new Mock<ILogger>();
                CampusOrganization2Records = BuildCampusOrganization2Records();
                campusOrganizationRepository = BuildCampusOrganizationRepository();
            }

            [TestCleanup]
            public void Cleanup()
            {
                campusOrganizationRepository = null;
                CampusOrganization2Records = null;
                apiSettings = null;
                transFactoryMock = null;
                cacheProviderMock = null;
                dataAccessorMock = null;
                loggerMock = null;
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task CampusOrganizationRepository_GetCampusOrganizations2AsyncNullInputTest()
            {
                List<string> campusOrgIds = null;
                var results = await campusOrganizationRepository.GetCampusOrganizations2Async(campusOrgIds);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public async Task CampusOrganizationRepository_GetCampusOrganizations2AsyncNoCampusOrgIdInputTest()
            {
                List<string> campusOrgIds = new List<string>();
                var results = await campusOrganizationRepository.GetCampusOrganizations2Async(campusOrgIds);
            }

            [TestMethod]
            public async Task CampusOrganizationRepository_GetCampusOrganizations2AsyncTest()
            {             
                List<string> campusOrgIds = new List<string>() { "CYC", "BIOS" };
                var results = await campusOrganizationRepository.GetCampusOrganizations2Async(campusOrgIds);
                Assert.AreEqual(CampusOrganization2Records.First().CampusOrganizationId, results.First().CampusOrganizationId);
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task CampusOrganizationRepository_GetCampusOrganizations2AsyncWithWrongIdTest()
            {
                List<string> campusOrgIds = new List<string>() { "WRONG"};               
                var results = await campusOrganizationRepository.GetCampusOrganizations2Async(campusOrgIds);                         
            }

            [TestMethod]
            [ExpectedException(typeof(ApplicationException))]
            public async Task CampusOrganizationRepository_GetCampusOrganizations2AsyncReturnsNullTest()
            {
                List<string> campusOrgIds = new List<string>() { "WRONG" };

                CampusOrganization2Records = null;

                dataAccessorMock.Setup(d => d.SelectAsync("CAMPUS.ORGS", "WITH CAMPUS.ORGS.ID EQ ?", It.IsAny<string[]>(), "?", true, 425))
                .Returns<string, string, string[], string, bool, int>((f, c, cmp, p, b, s) =>
                Task.FromResult(CampusOrganization2Records == null ? null :
                   CampusOrganization2Records
                       .Where(r => cmp.Select(v => v.Replace("\"", "").Replace("\\", "")).Contains(r.CampusOrganizationId))
                       .Select(r => r.CampusOrganizationId)
                       .ToArray()));
                
                var results = await campusOrganizationRepository.GetCampusOrganizations2Async(campusOrgIds);
            }

            [TestMethod]
            [ExpectedException(typeof(ApplicationException))]
            public async Task CampusOrganizationRepository_GetCampusOrganizations2AsyncBulkReaderReturnsNoRecordsTest()
            {
                List<string> campusOrgIds = new List<string>() { "CYC", "BIOS" };
                var records = new List<CampusOrganization2>();
                dataAccessorMock.Setup(d => d.BulkReadRecordAsync<CampusOrgs>(It.IsAny<string[]>(), true))
                .Returns<string[], bool>((ids, b) =>
                    Task.FromResult(records == null ? null :
                        new Collection<CampusOrgs>(
                            records
                            .Where(r => ids.Contains(r.CampusOrganizationId))
                            .Select(r => new CampusOrgs()
                            {
                                Recordkey = r.CampusOrganizationId,
                                CmpDesc = r.CampusOrganizationDescription
                            }).ToList())));
                var results = await campusOrganizationRepository.GetCampusOrganizations2Async(campusOrgIds);
            }

            private List<CampusOrganization2> BuildCampusOrganization2Records()
            {
                List<CampusOrganization2> campusOrganization2Records = new List<CampusOrganization2>()
                {
                    new CampusOrganization2("CYC", "Cycling Club"),
                    new CampusOrganization2("BIOS", "Amateur Bioinformatics Group")
                };

                return campusOrganization2Records;
            }

            private CampusOrganizationRepository BuildCampusOrganizationRepository()
            {                
                transFactoryMock = new Mock<IColleagueTransactionFactory>();                
                cacheProviderMock = new Mock<ICacheProvider>();             
                dataAccessorMock = new Mock<IColleagueDataReader>();
                apiSettings = new ApiSettings("TEST");

                // Set up dataAccessorMock as the object for the DataAccessor
                transFactoryMock.Setup(transFac => transFac.GetDataReader()).Returns(dataAccessorMock.Object);

                var records = new Collection<DataContracts.CampusOrgs>();
                foreach (var item in CampusOrganization2Records)
                {
                    DataContracts.CampusOrgs record = new DataContracts.CampusOrgs();
                    record.Recordkey = item.CampusOrganizationId;
                    record.CmpDesc = item.CampusOrganizationDescription;
                    records.Add(record);
                }

                dataAccessorMock.Setup(acc => acc.BulkReadRecordAsync<DataContracts.CampusOrgs>(It.IsAny<string[]>(), true)).ReturnsAsync(records);

                dataAccessorMock.Setup(d => d.SelectAsync("CAMPUS.ORGS", "WITH CAMPUS.ORGS.ID EQ ?", It.IsAny<string[]>(), "?", true, 425))
                .Returns<string, string, string[], string, bool, int>((f, c, campusOrgIds, p, b, s) =>
                Task.FromResult(CampusOrganization2Records == null ? null :
                   CampusOrganization2Records
                       .Where(r => campusOrgIds.Select(v => v.Replace("\"", "").Replace("\\", "")).Contains(r.CampusOrganizationId))
                       .Select(r => r.CampusOrganizationId)
                       .ToArray()));
                                
                // Construct repository
                campusOrganizationRepository = new CampusOrganizationRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object, apiSettings);

                return campusOrganizationRepository;
            }
        }

    }
}
