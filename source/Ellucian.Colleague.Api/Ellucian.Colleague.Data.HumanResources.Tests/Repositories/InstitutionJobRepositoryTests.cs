/* Copyright 2016-2017 Ellucian Company L.P. and its affiliates. */

using Ellucian.Colleague.Data.Base.Tests.Repositories;
using Ellucian.Colleague.Data.HumanResources.Repositories;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Ellucian.Colleague.Data.HumanResources.DataContracts;
using Ellucian.Data.Colleague;
using Ellucian.Colleague.Domain.HumanResources.Entities;
using Ellucian.Colleague.Data.HumanResources.Transactions;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Colleague.Data.Base.DataContracts;

namespace Ellucian.Colleague.Data.HumanResources.Tests.Repositories
{
    [TestClass]
    public class InstitutionJobRepositoryTests : BaseRepositorySetup
    {
        private InstitutionJobsRepository repositoryUnderTest;

        //test data objects
        private List<string> institutionJobIdList;
        private List<string> personIdList;
        private List<string> hrperIdList;
        private Collection<DataContracts.Position> positionsCollDataList;
        private Collection<Perpos> perposCollDataList;
        private Collection<Perposwg> perposwgCollDataList;
        private Collection<Perstat> perstatCollDataList;
        private Collection<Perben> perbenCollDataList;
        private Collection<Hrper> hrperCollDataList;

        string guid = "4f937f08-f6a0-4a1c-8d55-9f2a6dd6be46";

        [TestInitialize]
        public void Initialize()
        {
            MockInitialize();
            repositoryUnderTest = new InstitutionJobsRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object, apiSettings);

            TestDataSetup();

            GuidLookupResult result = new GuidLookupResult() { Entity = "PERPOS", PrimaryKey = "1" };
            Dictionary<string, GuidLookupResult> resultDict = new Dictionary<string, GuidLookupResult>();
            resultDict.Add(guid, result);

            dataReaderMock.Setup(repo => repo.SelectAsync(It.IsAny<GuidLookup[]>())).ReturnsAsync(resultDict);

            dataReaderMock.Setup(repo => repo.SelectAsync("POSITION", It.IsAny<string>(), It.IsAny<string[]>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<int>())).ReturnsAsync(institutionJobIdList.ToArray());

            dataReaderMock.Setup(repo => repo.SelectAsync("POSITION", It.IsAny<string>())).ReturnsAsync(institutionJobIdList.ToArray());

            dataReaderMock.Setup(repo => repo.SelectAsync("HRPER", It.IsAny<string>())).ReturnsAsync(hrperIdList.ToArray());

            dataReaderMock.Setup(repo => repo.BulkReadRecordAsync<DataContracts.Position>(It.IsAny<string[]>(), It.IsAny<bool>()))
                .ReturnsAsync(positionsCollDataList);

            dataReaderMock.Setup(repo => repo.SelectAsync("PERPOS", It.IsAny<string>())).ReturnsAsync(personIdList.ToArray());

            dataReaderMock.Setup(repo => repo.BulkReadRecordAsync<Perpos>("PERPOS", It.IsAny<string[]>(), It.IsAny<bool>()))
                .ReturnsAsync(perposCollDataList);

            dataReaderMock.Setup(repo => repo.ReadRecordAsync<Perpos>("PERPOS", It.IsAny<string>(), It.IsAny<bool>()))
                .ReturnsAsync(perposCollDataList[0]);

            dataReaderMock.Setup(repo => repo.SelectAsync("PERPOSWG", It.IsAny<string>(),
                        It.IsAny<string[]>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<int>())).ReturnsAsync(personIdList.ToArray());

            dataReaderMock.Setup(repo => repo.SelectAsync("PERPOSWG", It.IsAny<string>())).ReturnsAsync(personIdList.ToArray());

            dataReaderMock.Setup(repo => repo.BulkReadRecordAsync<Perposwg>(It.IsAny<string[]>(), It.IsAny<bool>()))
                .ReturnsAsync(perposwgCollDataList);

            dataReaderMock.Setup(repo => repo.SelectAsync("PERSTAT", It.IsAny<string>(),
                        It.IsAny<string[]>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<int>())).ReturnsAsync(personIdList.ToArray());

            dataReaderMock.Setup(repo => repo.SelectAsync("PERSTAT", It.IsAny<string>())).ReturnsAsync(personIdList.ToArray());

            dataReaderMock.Setup(repo => repo.BulkReadRecordAsync<Perstat>(It.IsAny<string[]>(), It.IsAny<bool>()))
                .ReturnsAsync(perstatCollDataList);

            dataReaderMock.Setup(repo => repo.SelectAsync("PERBEN", It.IsAny<string>(),
                        It.IsAny<string[]>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<int>())).ReturnsAsync(personIdList.ToArray());

            dataReaderMock.Setup(repo => repo.SelectAsync("PERBEN", It.IsAny<string>())).ReturnsAsync(personIdList.ToArray());

            dataReaderMock.Setup(repo => repo.BulkReadRecordAsync<Perben>(It.IsAny<string[]>(), It.IsAny<bool>()))
                .ReturnsAsync(perbenCollDataList);

            dataReaderMock.Setup(repo => repo.BulkReadRecordAsync<Hrper>(It.IsAny<string[]>(), It.IsAny<bool>()))
                .ReturnsAsync(hrperCollDataList);

            string[] recordGuids = { perposCollDataList[0].RecordGuid };
            dataReaderMock.Setup(repo => repo.SelectAsync("LDM.GUID", It.IsAny<string>())).ReturnsAsync(recordGuids);
            
        }

        [TestMethod]
        public async Task GetAllInstitutionJobs_V8()
        {
            var testData = repositoryUnderTest.GetInstitutionJobsAsync(0, 100, It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), false).Result;

            Assert.AreEqual(testData.Item2, 40);
        }

        [TestMethod]
        public async Task GetAllInstitutionJobsFilterAll_V8()
        {
            var testData = repositoryUnderTest.GetInstitutionJobsAsync(0, 100, "e43e7195-6eca-451d-b6c3-1e52fe540083",
                "27e30a4c-1071-48c9-8f7d-3e0209349a4c", "fadbb5f0-e39d-4b1e-82c9-77617ee2164c", "Math", "2000-01-01 00:00:00.000",
                "2020-12-31 00:00:00.000", "active", "4950f23d-4927-49d9-aa42-be4d97b4aed0", "primary", It.IsAny<bool>()).Result;

            Assert.AreEqual(testData.Item2, 0);
        }

        [TestMethod]
        public async Task GetByGuidInstitutionJob_V8()
        {
            var testData = repositoryUnderTest.GetInstitutionJobsByGuidAsync(perposCollDataList[0].RecordGuid).Result;

            Assert.AreEqual(testData.Guid, perposCollDataList[0].RecordGuid);
        }

        public void TestDataSetup()
        {
            institutionJobIdList = new List<string>()
            {
                "1", "2", "3", "4", "5", "6", "7", "8", "9", "10", "11", "12", "13", "14", "15", "16", "17", "18", "19", "20",
                "21", "22", "23", "24", "25", "26", "27", "28", "29", "30", "31", "32", "33", "34", "35", "36", "37", "38", "39", "40",
                "41", "42", "43", "44", "45", "46", "47", "48", "49", "50", "51", "52", "53", "54", "55", "56", "57", "58", "59", "60",
                "61", "62", "63", "64", "65", "66", "67", "68", "69", "70", "71", "72", "73", "74", "75", "76", "77", "78", "79", "80"
            };

            hrperIdList = new List<string>()
            {
                "41", "42", "43", "44", "45", "46", "47", "48", "49", "50", "51", "52", "53", "54", "55", "56", "57", "58", "59", "60",
                "61", "62", "63", "64", "65", "66", "67", "68", "69", "70", "71", "72", "73", "74", "75", "76", "77", "78", "79", "80",
                "91", "92", "93", "94", "95", "96", "97", "98", "99", "100", "101", "102", "103", "104", "105", "106", "107", "108", "109", "110",
                "111", "112", "113", "114", "115", "116", "117", "118", "119", "120", "121", "122", "123", "124", "125", "126", "127", "128", "129", "130",
                "131", "132", "133", "134", "135", "136", "137", "138", "139", "140", "141", "142", "143", "144", "145", "146", "147", "148", "149", "150"
            };

            personIdList = new List<string>()
            {
                "41", "42", "43", "44", "45", "46", "47", "48", "49", "50", "51", "52", "53", "54", "55", "56", "57", "58", "59", "60",
                "61", "62", "63", "64", "65", "66", "67", "68", "69", "70", "71", "72", "73", "74", "75", "76", "77", "78", "79", "80"
            };

            positionsCollDataList = new Collection<DataContracts.Position>();

            for (int i = 41; i < 81; i++)
            {
                positionsCollDataList.Add(new DataContracts.Position()
                {
                    RecordGuid = Guid.NewGuid().ToString(),
                    RecordModelName = "positions",
                    PosHrlyOrSlry = "S",
                    Recordkey = i.ToString()
                });
            }

            perposCollDataList = new Collection<Perpos>();

            for (int i = 41; i < 81; i++)
            {
                perposCollDataList.Add(new Perpos()
                {
                    RecordGuid = Guid.NewGuid().ToString(),
                    PerposAltSupervisorId = i.ToString(),
                    PerposHrpId = i.ToString(),
                    PerposSupervisorHrpId = i.ToString(),
                    PerposPositionId = i.ToString(),
                    PerposStartDate = DateTime.Now,
                    PerposEndDate = DateTime.Now.AddYears(1),
                    Recordkey = i.ToString()
                });
            }

            perposwgCollDataList = new Collection<Perposwg>();

            for (int i = 41; i < 81; i++)
            {
                perposwgCollDataList.Add(new Perposwg()
                {
                    PpwgBaseEt = "et",
                    PpwgEndDate = DateTime.Now.AddYears(1),
                    PpwgStartDate = DateTime.Now,
                    PpwgHrpId = i.ToString(),
                    PpwitemsEntityAssociation = new List<PerposwgPpwitems>()
                    {
                        new PerposwgPpwitems( "value0", "value1", "value2", "value3", "value4", new decimal(4.0), new decimal(5.0), new decimal(6.0), new decimal(7.0), new decimal(8.0), "value10", "value11", new decimal(9.0))
                    },
                    Recordkey = i.ToString()
                });
            }

            perstatCollDataList = new Collection<Perstat>();

            for (int i = 41; i < 81; i++)
            {
                perstatCollDataList.Add(new Perstat()
                {
                    Recordkey = i.ToString(),
                    PerstatHrpId = i.ToString(),
                    PerstatEndDate = DateTime.Now.AddYears(1),
                    PerstatEndReason = "End Time",
                    PerstatPrimaryPerposId = i.ToString(),
                    PerstatPrimaryPosId = i.ToString(),
                    PerstatStartDate = DateTime.Now
                });
            }

            perbenCollDataList = new Collection<Perben>();

            for (int i = 41; i < 81; i++)
            {
                perbenCollDataList.Add(new Perben()
                {
                    Recordkey = i.ToString(),
                    PerbenBdId = i.ToString(),
                    PerbenCancelDate = DateTime.Now,
                    PerbenHrpId = i.ToString()
                });
            }

            hrperCollDataList = new Collection<Hrper>();

            for (int i = 41; i < 81; i++)
            {
                hrperCollDataList.Add(new Hrper()
                {
                    Recordkey = i.ToString()
                });
            }

        }

    }

    [TestClass]
    public class InstitutionJobRepositoryTests_V11_POST_PUT : BaseRepositorySetup
    {
        #region DECLARATIONS

        private InstitutionJobsRepository institutionJobsRepository;

        private Dictionary<string, GuidLookupResult> dicResult;

        private InstitutionJobs institutionJobs;

        private CreateUpdateInstJobsResponse response;

        private Perpos perpos;

        private Collection<Perposwg> perposwg;

        private Collection<DataContracts.Position> positions;

        private Collection<Perben> perbens;

        private Collection<Perstat> perstats;

        private Collection<Hrper> hrpers;

        private string guid = "1a49eed8-5fe7-4120-b1cf-f23266b9e874";

        #endregion

        #region TEST SETUP

        [TestInitialize]
        public void Initialize()
        {
            MockInitialize();

            InitializeTestData();

            InitializeTestMock();

            institutionJobsRepository = new InstitutionJobsRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object, apiSettings);
        }

        [TestCleanup]
        public void Cleanup()
        {
            MockCleanup();
        }

        private void InitializeTestData()
        {
            perpos = new Perpos()
            {
                RecordGuid = guid,
                Recordkey = "1",
                PerposStartDate = DateTime.Today,
                PerposHrpId = "1",
                PerposPositionId = "1",
                PerposEndDate = DateTime.Today.AddDays(100),
                PerposAltSupervisorId = "1",
                PerposSupervisorHrpId = "1",
                PerposEndReason = "1",
            };

            perposwg = new Collection<Perposwg>()
            {
                new Perposwg()
                {
                    PpwgHrpId = "1",
                    PpwgEndDate = DateTime.Today.AddDays(100),
                    PpwgStartDate = DateTime.Today,
                    PpwgPiGlNo = new List<string>() { "1" },
                    PpwitemsEntityAssociation = new List<PerposwgPpwitems>()
                    {
                        new PerposwgPpwitems()
                        {
                            PpwgProjectsIdsAssocMember = "1",
                            PpwgPiGlNoAssocMember = "1",
                            
                        }
                    }
                }
            };

            positions = new Collection<DataContracts.Position>()
            {
                new DataContracts.Position()
                {
                    RecordGuid = guid,
                    Recordkey = "1",
                    PosDept = "1",
                    PosClass = "1",
                    PosHrlyOrSlry = "S"
                }
            };

            perstats = new Collection<Perstat>()
            {
                new Perstat() { Recordkey = "1" }
            };

            hrpers = new Collection<Hrper>()
            {
                new Hrper() { Recordkey = "1", RecordGuid = guid }
            };

            perbens = new Collection<Perben>()
            {
                new Perben() { RecordGuid = guid, Recordkey = "1" }
            };

            dicResult = new Dictionary<string, GuidLookupResult>() { { guid, new GuidLookupResult() { Entity = "PERPOS", PrimaryKey = "1" } } };

            institutionJobs = new InstitutionJobs(guid, "1", "1", "1", DateTime.Today)
            {
                Classification = "1",
                Department = "1",
                Employer = "1",
                EndDate = DateTime.Today.AddDays(100),
                FullTimeEquivalent = 100,
                EndReason = "1",
                CurrencyCode = "USD",
                CycleWorkTimeAmount = 100,
                YearWorkTimeAmount = 100,
                SupervisorId = "1",
                AlternateSupervisorId = "1",
                PayClass = "SOM",
                PayCycle = "SM",
                PerposwgItems = new List<PersonPositionWageItem>()
                {
                    new PersonPositionWageItem()
                    {
                        PayRate = "2",
                        StartDate = DateTime.Today,
                        EndDate = DateTime.Today.AddDays(100),
                        Grade = "1",
                        Step = "2",
                        HourlyOrSalary = "h",
                        AccountingStringAllocation = new List<PpwgAccountingStringAllocation> { new PpwgAccountingStringAllocation()
                        {
                            GlNumber = "1",
                            GlPercentDistribution = 2
                        } }
                        
                    },
                    new PersonPositionWageItem()
                    {
                        PayRate = "3",
                        StartDate = DateTime.Today.AddDays(-100),
                        EndDate = DateTime.Today.AddDays(-10),
                        Grade = "3",
                        Step = "1",
                        HourlyOrSalary = "y",
                        AccountingStringAllocation = new List<PpwgAccountingStringAllocation> { new PpwgAccountingStringAllocation()
                        {
                            GlNumber = "2",
                            GlPercentDistribution = 1
                        } }
                    }
                },
            };

            response = new CreateUpdateInstJobsResponse() { Guid = guid, InstJobId = "1" };
        }

        private void InitializeTestMock()
        {
            dataReaderMock.Setup(r => r.SelectAsync(It.IsAny<GuidLookup[]>())).ReturnsAsync(dicResult);
            dataReaderMock.Setup(r => r.SelectAsync(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(new string[] { "1" });

            dataReaderMock.Setup(d => d.ReadRecord<Defaults>("CORE.PARMS", "DEFAULTS", true)).Returns(new Defaults() { DefaultHostCorpId = "1" });

            dataReaderMock.Setup(r => r.ReadRecordAsync<Perpos>(It.IsAny<string>(), It.IsAny<string>(), true)).ReturnsAsync(perpos);
            dataReaderMock.Setup(d => d.ReadRecordAsync<IntlParams>(It.IsAny<string>(), It.IsAny<string>(), true)).ReturnsAsync(new IntlParams() { HostCountry = "USA" });

            dataReaderMock.Setup(d => d.BulkReadRecordAsync<Perposwg>(It.IsAny<string[]>(), true)).ReturnsAsync(perposwg);
            dataReaderMock.Setup(d => d.BulkReadRecordAsync<DataContracts.Position>(It.IsAny<string[]>(), true)).ReturnsAsync(positions);
            dataReaderMock.Setup(d => d.BulkReadRecordAsync<Perben>(It.IsAny<string[]>(), true)).ReturnsAsync(perbens);
            dataReaderMock.Setup(d => d.BulkReadRecordAsync<Perstat>(It.IsAny<string[]>(), true)).ReturnsAsync(perstats);
            dataReaderMock.Setup(d => d.BulkReadRecordAsync<Perstat>(It.IsAny<string[]>(), true)).ReturnsAsync(perstats);
            dataReaderMock.Setup(d => d.BulkReadRecordAsync<Hrper>(It.IsAny<string[]>(), true)).ReturnsAsync(hrpers);

            dataReaderMock.Setup(d => d.SelectAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string[]>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<int>())).ReturnsAsync(new string[] { "1" });

            transManagerMock.Setup(t => t.ExecuteAsync<CreateUpdateInstJobsRequest, CreateUpdateInstJobsResponse>(It.IsAny<CreateUpdateInstJobsRequest>())).ReturnsAsync(response);
        }

        #endregion

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task InstitutionJobsRepository_CreateInstitutionJobsAsync_Entity_Null()
        {
            await institutionJobsRepository.CreateInstitutionJobsAsync(null);
        }

        [TestMethod]
        [ExpectedException(typeof(RepositoryException))]
        public async Task InstitutionJobsRepository_CreateInstitutionJobsAsync_RepositoryException()
        {
            response.ErrorMessages = new List<string>() { "Error" };

            await institutionJobsRepository.CreateInstitutionJobsAsync(institutionJobs);
        }

        [TestMethod]
        public async Task InstitutionJobsRepository_CreateInstitutionJobsAsync()
        {
            var result = await institutionJobsRepository.CreateInstitutionJobsAsync(institutionJobs);

            Assert.IsNotNull(result);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task InstitutionJobsRepository_UpdateInstitutionJobsAsync_Entity_Null()
        {
            await institutionJobsRepository.UpdateInstitutionJobsAsync(null);
        }

        [TestMethod]
        public async Task InstitutionJobsRepository_UpdateInstitutionJobsAsync_Create_New_Record()
        {
            var firstResult = new Dictionary<string, GuidLookupResult>() { { guid, new GuidLookupResult() { Entity = "PERPOS", PrimaryKey = null } } };
            dataReaderMock.SetupSequence(r => r.SelectAsync(It.IsAny<GuidLookup[]>())).Returns(Task.FromResult(firstResult)).Returns(Task.FromResult(dicResult));

            institutionJobs.StartDate = DateTime.Today.AddDays(-100);
            institutionJobs.EndDate = DateTime.Today.AddDays(-1);

            var result = await institutionJobsRepository.UpdateInstitutionJobsAsync(institutionJobs);

            Assert.IsNotNull(result);
        }

        [TestMethod]
        [ExpectedException(typeof(RepositoryException))]
        public async Task InstitutionJobsRepository_UpdateInstitutionJobsAsync_RepositoryException()
        {
            response.ErrorMessages = new List<string>() { "Error" };

            await institutionJobsRepository.UpdateInstitutionJobsAsync(institutionJobs);
        }

        [TestMethod]
        public async Task InstitutionJobsRepository_UpdateInstitutionJobsAsync()
        {
            var result = await institutionJobsRepository.UpdateInstitutionJobsAsync(institutionJobs);

            Assert.IsNotNull(result);
        }
    }
}
