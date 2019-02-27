// Copyright 2018 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Data.Base.DataContracts;
using Ellucian.Colleague.Data.Base.Repositories;
using Ellucian.Colleague.Data.Base.Transactions;
using Ellucian.Colleague.Domain.Base.Entities;
using Ellucian.Data.Colleague;
using Ellucian.Data.Colleague.DataContracts;
using Ellucian.Data.Colleague.Exceptions;
using Ellucian.Web.Cache;
using Ellucian.Web.Http.Configuration;
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
    public class MessageRepositoryTests
    {
        public class MessageRepositoryTestsBase
        {
            protected Mock<IColleagueTransactionFactory> transFactoryMock;
            protected Mock<ObjectCache> localCacheMock;
            protected Mock<ICacheProvider> cacheProviderMock;
            protected Mock<IColleagueDataReader> dataAccessorMock;
            protected Mock<IColleagueTransactionInvoker> transactionInvoker;
            protected Mock<ILogger> loggerMock;
            protected MessageRepository messageRepo;
            UpdateMsgWorklistResponse response;
            protected ApiSettings apiSettingsMock;
            //private Mock<IColleagueTransactionInvoker> transactionInvoker = null;

            public void InitializeBase()
            {
                loggerMock = new Mock<ILogger>();
                apiSettingsMock = new ApiSettings("null");
                messageRepo = BuildMessageRepository();
                //transactionInvoker = new Mock<IColleagueTransactionInvoker>();
                //transFactoryMock.Setup(transFac => transFac.GetTransactionInvoker()).Returns(transactionInvoker.Object);

            }

            public void CleanupBase()
            {
                messageRepo = null;
                loggerMock = null;
                transactionInvoker = null;


            }

            private MessageRepository BuildMessageRepository()
            {
                transFactoryMock = new Mock<IColleagueTransactionFactory>();
                localCacheMock = new Mock<ObjectCache>();
                cacheProviderMock = new Mock<ICacheProvider>();
                dataAccessorMock = new Mock<IColleagueDataReader>();
                transactionInvoker = new Mock<IColleagueTransactionInvoker>();
                transFactoryMock.Setup(transFac => transFac.GetDataReader()).Returns(dataAccessorMock.Object);
                transFactoryMock.Setup(transFac => transFac.GetTransactionInvoker()).Returns(transactionInvoker.Object);

                cacheProviderMock.Setup<Task<Tuple<object, SemaphoreSlim>>>(x =>
                    x.GetAndLockSemaphoreAsync(It.IsAny<string>(), null))
                    .ReturnsAsync(new Tuple<object, SemaphoreSlim>(null, new SemaphoreSlim(1, 1)));



                // Set up onboarding user responses
                return new MessageRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object, apiSettingsMock);
            }
        }

        [TestClass]
        public class MessageRepository_UpdateAsync : MessageRepositoryTestsBase
        {
            private string personId;
            private string messageId;
            private ExecutionState newState;
            private List<string> roleIds;
            private string[] worklistAddrforEntityIds;
            private string[] worklistAddrforRoleIds;
            private List<WorklistAddr> worklistAddr;
            private string[] worklistAddrIds;
            private List<string> worklistIds;
            private List<Worklist> worklist;
            private ApplValcodes wfCategories;
            private UpdateMsgWorklistResponse response;


            [TestInitialize]
            public void Initialize()
            {
                InitializeBase();
                SetupRepositoryMock();
            }

            private void SetupRepositoryMock()
            {
                // Set up Mocking
                personId = "0016511";
                roleIds = new List<string>() { "ROLE1", "ROLE2" };
                messageId = "3757";
                newState = ExecutionState.ON;

                worklistAddrIds = new string[] { "1", "2", "3", "4" };

                worklistAddr = new List<WorklistAddr>()
                    {
                        new WorklistAddr() {Recordkey = "1", WkladOrgEntity = "0000001", WkladOrgRole = "", WkladWorklist = "11"},
                        new WorklistAddr() {Recordkey = "2", WkladOrgEntity = "0000002", WkladOrgRole = "", WkladWorklist = "12"},
                        new WorklistAddr() {Recordkey = "3", WkladOrgEntity = "", WkladOrgRole = "ROLE1", WkladWorklist = "13"},
                        new WorklistAddr() {Recordkey = "4", WkladOrgEntity = "", WkladOrgRole = "ROLE2", WkladWorklist = "14"}
                    };

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

                // Mock the call for worklistAddrIds
                dataAccessorMock.Setup<Task<string[]>>(acc => acc.SelectAsync("WORKLIST.ADDR", It.IsAny<string>()))
                    .ReturnsAsync(worklistAddrIds);

                // Mock response to worklist records read
                dataAccessorMock.Setup<Task<Collection<Worklist>>>(acc => acc.BulkReadRecordAsync<Worklist>(It.IsAny<string[]>(), true))
                    .ReturnsAsync(new Collection<Worklist>(worklist));

                // Mock response to worklistAddr bulkread
                var worklistAddr1 = worklistAddr.Where(wa => (!string.IsNullOrEmpty(wa.WkladOrgEntity))).ToList();
                dataAccessorMock.Setup<Task<Collection<WorklistAddr>>>(acc => acc.BulkReadRecordAsync<WorklistAddr>(It.IsAny<string[]>(), true))
                    .ReturnsAsync(new Collection<WorklistAddr>(worklistAddr1));


            }


            [TestMethod]
            [Ignore]
            public async Task MessageUpdatesWorktaskForPerson()
            {

                UpdateMsgWorklistRequest messageWorklistUpdateRequest = new UpdateMsgWorklistRequest()
                {
                    AWorklistId = worklist[0].Recordkey,
                    ANewState = newState.ToString()

                };

                UpdateMsgWorklistRequest updateMsgWorklistRequest = null;
                UpdateMsgWorklistResponse updateMsgWorklistResponse = new UpdateMsgWorklistResponse() { AErrorOccurred = "0" };

                transactionInvoker.Setup(mgr => mgr.ExecuteAsync<UpdateMsgWorklistRequest, UpdateMsgWorklistResponse>(It.IsAny<UpdateMsgWorklistRequest>())).ReturnsAsync(updateMsgWorklistResponse).Callback<UpdateMsgWorklistRequest>(req => updateMsgWorklistRequest = req);

                var result = await messageRepo.UpdateMessageWorklistAsync(worklist[0].Recordkey, personId, newState);
                Assert.IsNotNull(result);

            }

            [TestMethod]
            [Ignore]
            [ExpectedException(typeof(ArgumentException))]
            public async Task MessageUpdate_NullMessageId()
            {

                UpdateMsgWorklistRequest messageWorklistUpdateRequest = new UpdateMsgWorklistRequest()
                {
                    AWorklistId = worklist[0].Recordkey,
                    ANewState = newState.ToString()

                };

                UpdateMsgWorklistRequest updateMsgWorklistRequest = null;
                UpdateMsgWorklistResponse updateMsgWorklistResponse = new UpdateMsgWorklistResponse() { AErrorOccurred = "0" };

                transactionInvoker.Setup(mgr => mgr.ExecuteAsync<UpdateMsgWorklistRequest, UpdateMsgWorklistResponse>(It.IsAny<UpdateMsgWorklistRequest>())).ReturnsAsync(updateMsgWorklistResponse).Callback<UpdateMsgWorklistRequest>(req => updateMsgWorklistRequest = req);
                await messageRepo.UpdateMessageWorklistAsync(null, personId, newState);
            }

            [TestMethod]
            [Ignore]
            [ExpectedException(typeof(ArgumentException))]
            public async Task MessageUpdate_NullPersonId()
            {

                UpdateMsgWorklistRequest messageWorklistUpdateRequest = new UpdateMsgWorklistRequest()
                {
                    AWorklistId = worklist[0].Recordkey,
                    ANewState = newState.ToString()

                };

                UpdateMsgWorklistRequest updateMsgWorklistRequest = null;
                UpdateMsgWorklistResponse updateMsgWorklistResponse = new UpdateMsgWorklistResponse() { AErrorOccurred = "0" };

                transactionInvoker.Setup(mgr => mgr.ExecuteAsync<UpdateMsgWorklistRequest, UpdateMsgWorklistResponse>(It.IsAny<UpdateMsgWorklistRequest>())).ReturnsAsync(updateMsgWorklistResponse).Callback<UpdateMsgWorklistRequest>(req => updateMsgWorklistRequest = req);
                await messageRepo.UpdateMessageWorklistAsync(messageId, null, newState);
            }

 
            [TestMethod]
            [Ignore]
            public async Task CreateWorktaskForPerson()
            {

                CreateMsgWorklistRequest messageWorklistUpdateRequest = new CreateMsgWorklistRequest()
                {
                    AWorkflowDefId = "a"
                };

                CreateMsgWorklistRequest createMsgWorklistRequest = null;
                CreateMsgWorklistResponse createMsgWorklistResponse = new CreateMsgWorklistResponse() {
                    AlWorklistIds = {"11"},
                    AErrorOccurred = "0"
                };

                transactionInvoker.Setup(mgr => mgr.ExecuteAsync<CreateMsgWorklistRequest, CreateMsgWorklistResponse>(It.IsAny<CreateMsgWorklistRequest>())).ReturnsAsync(createMsgWorklistResponse).Callback<CreateMsgWorklistRequest>(req => createMsgWorklistRequest = req);
                var workflowDefId = "a";
                var processCode = "a";
                var subjectLine = "a";

                var result = await messageRepo.CreateMessageWorklistAsync("p", workflowDefId, processCode, subjectLine);
                Assert.IsNotNull(result);
            }

            [TestMethod]
            [Ignore]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task MessageCreate_NullPersonId()
            {

                CreateMsgWorklistRequest messageWorklistUpdateRequest = new CreateMsgWorklistRequest()
                {
                    AWorkflowDefId = "a"
                };

                CreateMsgWorklistRequest createMsgWorklistRequest = null;
                CreateMsgWorklistResponse createMsgWorklistResponse = new CreateMsgWorklistResponse()
                {
                    AlWorklistIds = { "11" },
                    AErrorOccurred = "0"
                };

                transactionInvoker.Setup(mgr => mgr.ExecuteAsync<CreateMsgWorklistRequest, CreateMsgWorklistResponse>(It.IsAny<CreateMsgWorklistRequest>())).ReturnsAsync(createMsgWorklistResponse).Callback<CreateMsgWorklistRequest>(req => createMsgWorklistRequest = req);
                var workflowDefId = "a";
                var processCode = "a";
                var subjectLine = "a";

                await messageRepo.CreateMessageWorklistAsync(null, workflowDefId, processCode, subjectLine);
            }

            [TestMethod]
            [Ignore]
            [ExpectedException(typeof(ArgumentException))]
            public async Task MessageCreate_NullWorkflowId()
            {

                CreateMsgWorklistRequest messageWorklistUpdateRequest = new CreateMsgWorklistRequest()
                {
                    AWorkflowDefId = "a"
                };

                CreateMsgWorklistRequest createMsgWorklistRequest = null;
                CreateMsgWorklistResponse createMsgWorklistResponse = new CreateMsgWorklistResponse()
                {
                    AlWorklistIds = { "11" },
                    AErrorOccurred = "0"
                };

                transactionInvoker.Setup(mgr => mgr.ExecuteAsync<CreateMsgWorklistRequest, CreateMsgWorklistResponse>(It.IsAny<CreateMsgWorklistRequest>())).ReturnsAsync(createMsgWorklistResponse).Callback<CreateMsgWorklistRequest>(req => createMsgWorklistRequest = req);

                var processCode = "a";
                var subjectLine = "a";

                await messageRepo.CreateMessageWorklistAsync("p", null, processCode, subjectLine);
            }

            [TestMethod]
            [Ignore]
            [ExpectedException(typeof(ArgumentException))]
            public async Task MessageCreate_NullProcessCode()
            {

                CreateMsgWorklistRequest messageWorklistUpdateRequest = new CreateMsgWorklistRequest()
                {
                    AWorkflowDefId = "a"
                };

                CreateMsgWorklistRequest createMsgWorklistRequest = null;
                CreateMsgWorklistResponse createMsgWorklistResponse = new CreateMsgWorklistResponse()
                {
                    AlWorklistIds = { "11" },
                    AErrorOccurred = "0"
                };

                transactionInvoker.Setup(mgr => mgr.ExecuteAsync<CreateMsgWorklistRequest, CreateMsgWorklistResponse>(It.IsAny<CreateMsgWorklistRequest>())).ReturnsAsync(createMsgWorklistResponse).Callback<CreateMsgWorklistRequest>(req => createMsgWorklistRequest = req);
                var workflowDefId = "a";
                var subjectLine = "a";

                await messageRepo.CreateMessageWorklistAsync("p", workflowDefId, null, subjectLine);
            }

            [TestMethod]
            [Ignore]
            [ExpectedException(typeof(ArgumentException))]
            public async Task MessageCreate_NullSubjectLine()
            {

                CreateMsgWorklistRequest messageWorklistUpdateRequest = new CreateMsgWorklistRequest()
                {
                    AWorkflowDefId = "a"
                };

                CreateMsgWorklistRequest createMsgWorklistRequest = null;
                CreateMsgWorklistResponse createMsgWorklistResponse = new CreateMsgWorklistResponse()
                {
                    AlWorklistIds = { "11" },
                    AErrorOccurred = "0"
                };

                transactionInvoker.Setup(mgr => mgr.ExecuteAsync<CreateMsgWorklistRequest, CreateMsgWorklistResponse>(It.IsAny<CreateMsgWorklistRequest>())).ReturnsAsync(createMsgWorklistResponse).Callback<CreateMsgWorklistRequest>(req => createMsgWorklistRequest = req);
                var workflowDefId = "a";
                var processCode = "a";

                await messageRepo.CreateMessageWorklistAsync("p", workflowDefId, processCode, null);
            }


        }
    }


}