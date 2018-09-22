// Copyright 2016 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Data.Base.DataContracts;
using Ellucian.Colleague.Data.Base.Repositories;
using Ellucian.Data.Colleague;
using Ellucian.Data.Colleague.DataContracts;
using Ellucian.Web.Cache;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.Caching;
using System.Threading;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Data.Base.Tests.Repositories
{
    [TestClass]
    public class WorkTaskRepositoryTests
    {
        public class WorkTaskRepositoryTestsBase
        {
            protected Mock<IColleagueTransactionFactory> transFactoryMock;
            protected Mock<ObjectCache> localCacheMock;
            protected Mock<ICacheProvider> cacheProviderMock;
            protected Mock<IColleagueDataReader> dataAccessorMock;
            protected Mock<IColleagueTransactionInvoker> transactionMock;
            protected Mock<ILogger> loggerMock;
            protected WorkTaskRepository workTaskRepo;

            public void InitializeBase()
            {
                loggerMock = new Mock<ILogger>();

                workTaskRepo = BuildWorkTaskRepository();

            }

            public void CleanupBase()
            {
                workTaskRepo = null;
                loggerMock = null;
            }

            private WorkTaskRepository BuildWorkTaskRepository()
            {
                transFactoryMock = new Mock<IColleagueTransactionFactory>();
                localCacheMock = new Mock<ObjectCache>();
                cacheProviderMock = new Mock<ICacheProvider>();
                dataAccessorMock = new Mock<IColleagueDataReader>();
                transactionMock = new Mock<IColleagueTransactionInvoker>();
                transFactoryMock.Setup(transFac => transFac.GetDataReader()).Returns(dataAccessorMock.Object);
                transFactoryMock.Setup(transFac => transFac.GetTransactionInvoker()).Returns(transactionMock.Object);

                cacheProviderMock.Setup<Task<Tuple<object, SemaphoreSlim>>>(x =>
                    x.GetAndLockSemaphoreAsync(It.IsAny<string>(), null))
                    .ReturnsAsync(new Tuple<object, SemaphoreSlim>(null, new SemaphoreSlim(1, 1)));

                // Set up onboarding user responses
                return new WorkTaskRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object);
            }
        }

        [TestClass]
        public class WorkTaskRepository_GetAsync : WorkTaskRepositoryTestsBase
        {
            private string personId;
            private List<string> roleIds;
            private string[] worklistAddrforEntityIds;
            private string[] worklistAddrforRoleIds;
            private List<WorklistAddr> worklistAddr;
            private string[] worklistAddrIds;
            private List<Worklist> worklist;
            private ApplValcodes wfCategories;

            [TestInitialize]
            public void Initialize()
            {
                InitializeBase();
                SetupRepositoryMock();
            }

            [TestCleanup]
            public void Cleanup()
            {
                CleanupBase();
            }

            private void SetupRepositoryMock()
            {
                // Set up Mocking
                personId = "0000001";
                roleIds = new List<string>() { "ROLE1", "ROLE2" };

                worklistAddr = new List<WorklistAddr>()
                    {
                        new WorklistAddr() {Recordkey = "1", WkladOrgEntity = "0000001", WkladOrgRole = "", WkladWorklist = "11"},
                        new WorklistAddr() {Recordkey = "2", WkladOrgEntity = "0000002", WkladOrgRole = "", WkladWorklist = "12"},
                        new WorklistAddr() {Recordkey = "3", WkladOrgEntity = "", WkladOrgRole = "ROLE1", WkladWorklist = "13"},
                        new WorklistAddr() {Recordkey = "4", WkladOrgEntity = "", WkladOrgRole = "ROLE2", WkladWorklist = "14"}
                    };
                worklistAddrIds = worklistAddr.Select(wa => wa.Recordkey).ToArray();

                worklistAddrforEntityIds = worklistAddr.Where(wa => wa.WkladOrgEntity == personId).Select(w => w.WkladWorklist).ToArray();

                worklistAddrforRoleIds = worklistAddr.Where(wa => roleIds.Contains(wa.WkladOrgRole)).Select(w => w.WkladWorklist).ToArray();

                worklist = new List<Worklist>()
                    { 
                        new Worklist() {Recordkey = "11", WklCategory = "CAT1", WklExecState = "NS", WklDescription = "Worklist Open Item"},
                        new Worklist() {Recordkey = "12", WklCategory = "NOTFOUND", WklExecState = "C", WklDescription = "Worklist Closed Item"},
                        new Worklist() {Recordkey = "13", WklCategory = "", WklExecState = "S", WklDescription = "Worklist Suspended Item"}
                    };


                // mock data accessor WF.CATEGORIES
                wfCategories = new ApplValcodes()
                    {
                        ValInternalCode = new List<string>() { "CAT1", "CAT2" },
                        ValExternalRepresentation = new List<string>() { "Category 1", "Category 2" },
                        ValsEntityAssociation = new List<ApplValcodesVals>()
                        {
                            new ApplValcodesVals() { ValInternalCodeAssocMember = "CAT1", ValExternalRepresentationAssocMember = "Category 1" },
                            new ApplValcodesVals() { ValInternalCodeAssocMember = "CAT2", ValExternalRepresentationAssocMember = "Category 2" }
                        }
                    };
                dataAccessorMock.Setup<Task<ApplValcodes>>(acc => acc.ReadRecordAsync<ApplValcodes>("UT.VALCODES", "WF.CATEGORIES", true))
                    .ReturnsAsync(wfCategories);
            }

            [TestMethod]
            public async Task GetsOpenTasksForPerson()
            {
                // list of WorklistAddr records that contain an entity Id
                var worklistAddr1 = worklistAddr.Where(wa => (!string.IsNullOrEmpty(wa.WkladOrgEntity))).ToList();
                // array of worklistaddr ids 
                var worklistAddr1Ids = worklistAddr1.Select(wa => wa.Recordkey).ToArray();
                // array of worklist Ids
                var worklist1Ids = worklistAddr1.Select(wa => wa.WkladWorklist).ToArray();
                // array of worklist records
                var worklist1 = worklist.Where(w => worklist1Ids.Contains(w.Recordkey)).ToList();

                // Mock response to worklistAddr Select
                dataAccessorMock.Setup<Task<string[]>>(acc => acc.SelectAsync("WORKLIST.ADDR", "WITH WKLAD.ORG.ENTITY EQ '0000001'"))
                    .ReturnsAsync(worklistAddr1Ids);
                // Mock response to worklistAddr bulkread
                dataAccessorMock.Setup<Task<Collection<WorklistAddr>>>(acc => acc.BulkReadRecordAsync<WorklistAddr>(It.IsAny<string[]>(), true))
                    .ReturnsAsync(new Collection<WorklistAddr>(worklistAddr1));
                // Mock response to worklist read
                dataAccessorMock.Setup<Task<Collection<Worklist>>>(acc => acc.BulkReadRecordAsync<Worklist>(It.IsAny<string[]>(), true))
                    .ReturnsAsync(new Collection<Worklist>(worklist1));

                var results = await workTaskRepo.GetAsync(personId, null);

                Assert.AreEqual(1, results.Count());
                Assert.AreEqual("11", results.ElementAt(0).Id);
            }

            [TestMethod]
            public async Task GetsTasksForRole()
            {
                // List ofworklistaddr records that contain a role
                var worklistAddr1 = worklistAddr.Where(wa => (!string.IsNullOrEmpty(wa.WkladOrgRole))).ToList();
                // Array of worklistaddr ids
                var worklistAddr1Ids = worklistAddr1.Select(wa => wa.Recordkey).ToArray();
                // array of worklist ids
                var worklist1Ids = worklistAddr1.Select(wa => wa.WkladWorklist).ToArray();
                // list of worklist records
                var worklist1 = worklist.Where(w => worklist1Ids.Contains(w.Recordkey)).ToList();

                // Mock response to worklist addr role select
                dataAccessorMock.Setup<Task<string[]>>(acc => acc.SelectAsync("WORKLIST.ADDR", "WITH WKLAD.ORG.ROLE EQ '?'", It.IsAny<string[]>(), "?", true, 425))
                    .ReturnsAsync(worklistAddr1Ids);
                // Mock response for worklistAddr bulk read
                dataAccessorMock.Setup<Task<Collection<WorklistAddr>>>(acc => acc.BulkReadRecordAsync<WorklistAddr>(It.IsAny<string[]>(), true))
                    .ReturnsAsync(new Collection<WorklistAddr>(worklistAddr1));
                // Mock response to worklist read
                dataAccessorMock.Setup<Task<Collection<Worklist>>>(acc => acc.BulkReadRecordAsync<Worklist>(It.IsAny<string[]>(), true))
                    .ReturnsAsync(new Collection<Worklist>(worklist1));


                var results = await workTaskRepo.GetAsync(null, roleIds);

                Assert.AreEqual(1, results.Count());
                Assert.AreEqual("13", results.ElementAt(0).Id);
            }

            [TestMethod]
            public async Task GetsAndInitializesAllWorkTasks()
            {
                // list of WorklistAddr records that contain an entity Id
                var worklistAddr1Ids = worklistAddr.Where(wa => (!string.IsNullOrEmpty(wa.WkladOrgEntity))).Select(wa => wa.Recordkey).ToArray();
                // List ofworklistaddr records that contain a role
                var worklistAddr2Ids = worklistAddr.Where(wa => (!string.IsNullOrEmpty(wa.WkladOrgRole))).Select(wa => wa.Recordkey).ToArray();

                // Mock response to worklistAddr person Select
                dataAccessorMock.Setup<Task<string[]>>(acc => acc.SelectAsync("WORKLIST.ADDR", "WITH WKLAD.ORG.ENTITY EQ '0000001' OR WITH WKLAD.ORG.ROLE EQ '?'", It.IsAny<string[]>(), "?", true, 425))
                    .ReturnsAsync(worklistAddrIds);

                dataAccessorMock.Setup<Task<Collection<WorklistAddr>>>(acc => acc.BulkReadRecordAsync<WorklistAddr>(It.IsAny<string[]>(), true))
                    .ReturnsAsync(new Collection<WorklistAddr>(worklistAddr));
                // Mock response to worklist read
                dataAccessorMock.Setup<Task<Collection<Worklist>>>(acc => acc.BulkReadRecordAsync<Worklist>(It.IsAny<string[]>(), true))
                    .ReturnsAsync(new Collection<Worklist>(worklist));

                var results = await workTaskRepo.GetAsync(personId, roleIds);

                Assert.AreEqual(2, results.Count());
                Assert.IsNotNull(results.Where(r => r.Id == "11").FirstOrDefault());
                Assert.IsNotNull(results.Where(r => r.Id == "13").FirstOrDefault());

                foreach (var item in results)
                {
                    var worklistItem = worklist.Where(w => w.Recordkey == item.Id).First();
                    Assert.AreEqual(worklistItem.WklDescription, item.Description);
                    // Category should have the translated description if not found, and the original code if not found.
                    var valEntry = wfCategories.ValsEntityAssociation.Where(v => v.ValInternalCodeAssocMember == worklistItem.WklCategory).FirstOrDefault();
                    if (valEntry != null)
                    {
                        Assert.AreEqual(valEntry.ValExternalRepresentationAssocMember, item.Category);
                    }
                    else
                    {
                        Assert.AreEqual(worklistItem.WklCategory, item.Category);
                    }
                }
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public async Task NullPersonsAndRoles_ThrowsException()
            {
                var results = await workTaskRepo.GetAsync(null, null);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public async Task EmptyPersonsAndRoles_ThrowsException()
            {
                var results = await workTaskRepo.GetAsync(string.Empty, new List<string>());
            }

            [TestMethod]
            public async Task NoWorklistAddrSelected_ReturnsEmptyList()
            {
                // Mock response to worklist addr role select only
                dataAccessorMock.Setup<Task<string[]>>(acc => acc.SelectAsync("WORKLIST.ADDR", "WITH WKLAD.ORG.ENTITY EQ ?", It.IsAny<string[]>(), "?", true, 425))
                    .ReturnsAsync(null);

                var results = await workTaskRepo.GetAsync(personId, roleIds);
                Assert.AreEqual(0, results.Count());
            }

            [TestMethod]
            public async Task NoWorklistAddrRead_ReturnsEmptyList()
            {
                // list of WorklistAddr records that contain an entity Id
                var worklistAddr1 = worklistAddr.Where(wa => (!string.IsNullOrEmpty(wa.WkladOrgEntity))).ToList();
                // array of worklistaddr ids 
                var worklistAddr1Ids = worklistAddr1.Select(wa => wa.Recordkey).ToArray();

                // Mock response to worklistAddr Select
                dataAccessorMock.Setup<Task<string[]>>(acc => acc.SelectAsync("WORKLIST.ADDR", "WITH WKLAD.ORG.ENTITY EQ ?", It.IsAny<string[]>(), "?", true, 425))
                    .ReturnsAsync(worklistAddr1Ids);

                var results = await workTaskRepo.GetAsync(personId, new List<string>());

                Assert.AreEqual(0, results.Count());
            }

            [TestMethod]
            public async Task NoWorklistRead_ReaturnsEmptyList()
            {
                // list of WorklistAddr records that contain an entity Id
                var worklistAddr1 = worklistAddr.Where(wa => (!string.IsNullOrEmpty(wa.WkladOrgEntity))).ToList();
                // array of worklistaddr ids 
                var worklistAddr1Ids = worklistAddr1.Select(wa => wa.Recordkey).ToArray();
                // array of worklist Ids
                var worklist1Ids = worklistAddr1.Select(wa => wa.WkladWorklist).ToArray();
                // array of worklist records
                var worklist1 = worklist.Where(w => worklist1Ids.Contains(w.Recordkey)).ToList();

                // Mock response to worklistAddr Select
                dataAccessorMock.Setup<Task<string[]>>(acc => acc.SelectAsync("WORKLIST.ADDR", "WITH WKLAD.ORG.ENTITY EQ ?", It.IsAny<string[]>(), "?", true, 425))
                    .ReturnsAsync(worklistAddr1Ids);
                // Mock response to worklistAddr bulkread
                dataAccessorMock.Setup<Task<Collection<WorklistAddr>>>(acc => acc.BulkReadRecordAsync<WorklistAddr>(It.IsAny<string[]>(), true))
                    .ReturnsAsync(new Collection<WorklistAddr>(worklistAddr1));

                var results = await workTaskRepo.GetAsync(personId, new List<string>());

                Assert.AreEqual(0, results.Count());
            }

            [TestMethod]
            public async Task NullCategoriesValcode_ReturnsCategoryCode()
            {
                // Mock null wfCategories response
                ApplValcodes wfCategories1 = null;
                dataAccessorMock.Setup<Task<ApplValcodes>>(acc => acc.ReadRecordAsync<ApplValcodes>("UT.VALCODES", "WF.CATEGORIES", true))
                    .ReturnsAsync(wfCategories1);

                // list of WorklistAddr records that contain an entity Id
                var worklistAddr1Ids = worklistAddr.Where(wa => (!string.IsNullOrEmpty(wa.WkladOrgEntity))).Select(wa => wa.Recordkey).ToArray();
                // List ofworklistaddr records that contain a role
                var worklistAddr2Ids = worklistAddr.Where(wa => (!string.IsNullOrEmpty(wa.WkladOrgRole))).Select(wa => wa.Recordkey).ToArray();

                // Mock response to worklistAddr person Select
                dataAccessorMock.Setup<Task<string[]>>(acc => acc.SelectAsync("WORKLIST.ADDR", "WITH WKLAD.ORG.ENTITY EQ '0000001' OR WITH WKLAD.ORG.ROLE EQ '?'", It.IsAny<string[]>(), "?", true, 425))
                    .ReturnsAsync(worklistAddrIds);

                dataAccessorMock.Setup<Task<Collection<WorklistAddr>>>(acc => acc.BulkReadRecordAsync<WorklistAddr>(It.IsAny<string[]>(), true))
                    .ReturnsAsync(new Collection<WorklistAddr>(worklistAddr));
                // Mock response to worklist read
                dataAccessorMock.Setup<Task<Collection<Worklist>>>(acc => acc.BulkReadRecordAsync<Worklist>(It.IsAny<string[]>(), true))
                    .ReturnsAsync(new Collection<Worklist>(worklist));

                var results = await workTaskRepo.GetAsync(personId, roleIds);

                Assert.AreEqual(2, results.Count());
                Assert.IsNotNull(results.Where(r => r.Id == "11").FirstOrDefault());
                Assert.IsNotNull(results.Where(r => r.Id == "13").FirstOrDefault());

                foreach (var item in results)
                {
                    var worklistItem = worklist.Where(w => w.Recordkey == item.Id).First();
                    // Category valcode is null so returned category always matches original code value
                    Assert.AreEqual(worklistItem.WklCategory, item.Category);
                }
            }
        }
    }
}
