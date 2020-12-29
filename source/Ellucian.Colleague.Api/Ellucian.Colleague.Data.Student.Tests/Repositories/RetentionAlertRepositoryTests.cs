// Copyright 2019-2020 Ellucian Company L.P. and its affiliates.

using Moq;
using System;
using slf4net;
using System.Threading;
using Ellucian.Web.Cache;
using System.Threading.Tasks;
using Ellucian.Data.Colleague;
using System.Collections.Generic;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Colleague.Domain.Base.Entities;
using Ellucian.Colleague.Data.Base.Transactions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ellucian.Colleague.Data.Student.Repositories;
using Ellucian.Colleague.Domain.Student.Repositories;
using Ellucian.Colleague.Data.Base.Tests.Repositories;
using Ellucian.Web.Http.Configuration;
using System.Runtime.Caching;
using System.Linq;
using System.Collections.ObjectModel;
using Ellucian.Colleague.Data.Student.DataContracts;

namespace Ellucian.Colleague.Data.Student.Tests.Repositories
{
    [TestClass]
    public class RetentionAlertRepositoryTests : BaseRepositorySetup
    {
        private static Collection<Base.DataContracts.Person> BuildPeople(string[] ids)
        {
            var people = new List<Base.DataContracts.Person>();
            foreach (var id in ids)
            {
                people.Add(new Base.DataContracts.Person() { Recordkey = id, LastName = id });
            }
            return new Collection<Base.DataContracts.Person>(people);
        }

        private static Collection<Students> BuildStudents(string[] ids)
        {
            var students = new List<Students>();
            foreach (var id in ids)
            {
                students.Add(new Students() { Recordkey = id });
            }
            return new Collection<Students>(students);
        }

        private static Collection<Base.DataContracts.PersonSt> BuildPersonStRecords(string[] ids)
        {
            var students = new List<Base.DataContracts.PersonSt>();
            foreach (var id in ids)
            {
                students.Add(new Base.DataContracts.PersonSt() { Recordkey = id });
            }
            return new Collection<Base.DataContracts.PersonSt>(students);
        }

        [TestClass]
        public class RetentionAlertRepository_GetRetentionAlertCases
        {
            Mock<IColleagueTransactionFactory> transFactoryMock;
            Mock<IColleagueDataReader> dataAccessorMock;
            Mock<ICacheProvider> cacheProviderMock;
            Mock<ILogger> loggerMock;
            Mock<ApiSettings> settings;
            Mock<IColleagueTransactionInvoker> mockManager;
            private IRetentionAlertRepository retentionAlertRepository;
            GetRaCaseInformationRequest getRequest;

            [TestInitialize]
            public void Initialize()
            {
                settings = new Mock<ApiSettings>();
                loggerMock = new Mock<ILogger>();
                cacheProviderMock = new Mock<ICacheProvider>();
                transFactoryMock = new Mock<IColleagueTransactionFactory>();
                dataAccessorMock = new Mock<IColleagueDataReader>();
                mockManager = new Mock<IColleagueTransactionInvoker>();
                cacheProviderMock.Setup<Task<Tuple<object, SemaphoreSlim>>>(x =>
                x.GetAndLockSemaphoreAsync(It.IsAny<string>(), null))
                .Returns(Task.FromResult(new Tuple<object, SemaphoreSlim>(
                    null,
                    new SemaphoreSlim(1, 1)
                    )));
                // Set up data accessor for the transaction factory
                transFactoryMock.Setup(transFac => transFac.GetDataReader()).Returns(dataAccessorMock.Object);
                // Set up successful response to a transaction request, capturing the completed request for verification
                transFactoryMock.Setup(transFac => transFac.GetTransactionInvoker()).Returns(mockManager.Object);
                retentionAlertRepository = new RetentionAlertRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object, settings.Object);
            }

            [TestCleanup]
            public void Cleanup()
            {
                transFactoryMock = null;
                dataAccessorMock = null;
                cacheProviderMock = null;
            }

            [TestMethod]
            [ExpectedException(typeof(RepositoryException))]
            public async Task GetRetentionAlertCases_ResponseHasErrorMessage()
            {
                GetRaCaseInformationResponse getResponse = new GetRaCaseInformationResponse()
                {
                    AlErrorMessages = new List<string> { "Error text" }
                };
                mockManager.Setup(mgr => mgr.ExecuteAsync<GetRaCaseInformationRequest, GetRaCaseInformationResponse>(It.IsAny<GetRaCaseInformationRequest>())).Returns(Task.FromResult(getResponse)).Callback<GetRaCaseInformationRequest>(req => getRequest = req);
                var retentionAlertCase = await retentionAlertRepository.GetRetentionAlertCasesAsync("000013", null, null);
            }

            [TestMethod]
            public async Task GetRetentionAlertCases_Success()
            {
                GetRaCaseInformationResponse caseResponse = new GetRaCaseInformationResponse()
                {
                    AError = "0",
                    AlErrorMessages = new List<string>(),
                    CaseInformation = new List<CaseInformation>()
                    {
                        new CaseInformation()
                        {
                            AlCaseIds = "41",
                            AlCaseOwners = "AL,Advisor",
                            AlCategories = "EARLY.ALERT",
                            AlCategoryDescriptions = "Description",
                            AlDatesCreated = DateTime.Now.AddDays(-5),
                            AlPriorities = "High",
                            AlStatuses = "New",
                            AlStudents = "000011",
                            AlReminderDates = DateTime.Now.AddDays(1)
                        }
                    }
                };

                mockManager.Setup(mgr => mgr.ExecuteAsync<GetRaCaseInformationRequest, GetRaCaseInformationResponse>(It.IsAny<GetRaCaseInformationRequest>())).Returns(Task.FromResult(caseResponse)).Callback<GetRaCaseInformationRequest>(req => getRequest = req);
                var retentionAlertCase = await retentionAlertRepository.GetRetentionAlertCasesAsync("000013", null, null);

                // Assert
                Assert.IsTrue(retentionAlertCase is IEnumerable<RetentionAlertWorkCase>);
                Assert.AreEqual(1, retentionAlertCase.Count);
            }
        }

        [TestClass]
        public class RetentionAlertRepository_AddRetentionAlertCase
        {
            Mock<IColleagueTransactionFactory> transFactoryMock;
            Mock<IColleagueDataReader> dataAccessorMock;
            Mock<ICacheProvider> cacheProviderMock;
            Mock<ILogger> loggerMock;
            Mock<ApiSettings> settings;
            Mock<IColleagueTransactionInvoker> mockManager;
            private IRetentionAlertRepository retentionAlertRepository;
            AddOrUpdtRaCaseNoteRequest createRequest;

            [TestInitialize]
            public void Initialize()
            {
                settings = new Mock<ApiSettings>();
                loggerMock = new Mock<ILogger>();
                cacheProviderMock = new Mock<ICacheProvider>();
                transFactoryMock = new Mock<IColleagueTransactionFactory>();
                dataAccessorMock = new Mock<IColleagueDataReader>();
                mockManager = new Mock<IColleagueTransactionInvoker>();
                cacheProviderMock.Setup<Task<Tuple<object, SemaphoreSlim>>>(x =>
                x.GetAndLockSemaphoreAsync(It.IsAny<string>(), null))
                .Returns(Task.FromResult(new Tuple<object, SemaphoreSlim>(
                    null,
                    new SemaphoreSlim(1, 1)
                    )));
                // Set up data accessor for the transaction factory
                transFactoryMock.Setup(transFac => transFac.GetDataReader()).Returns(dataAccessorMock.Object);
                // Set up successful response to a transaction request, capturing the completed request for verification
                transFactoryMock.Setup(transFac => transFac.GetTransactionInvoker()).Returns(mockManager.Object);
                retentionAlertRepository = new RetentionAlertRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object, settings.Object);
            }

            [TestCleanup]
            public void Cleanup()
            {
                transFactoryMock = null;
                dataAccessorMock = null;
                cacheProviderMock = null;
            }

            [TestMethod]
            [ExpectedException(typeof(Exception))]
            public async Task AddRetentionAlertCase_WithAllNull()
            {
                var retentionAlertCaseEntity = AddRetentionAlertCase_WithNullValues();
                AddOrUpdtRaCaseNoteResponse createResponse = new AddOrUpdtRaCaseNoteResponse();
                createResponse.AlErrorMessages = new List<string> { "Student id is required" };
                mockManager.Setup(mgr => mgr.ExecuteAsync<AddOrUpdtRaCaseNoteRequest, AddOrUpdtRaCaseNoteResponse>(It.IsAny<AddOrUpdtRaCaseNoteRequest>())).Returns(Task.FromResult(createResponse)).Callback<AddOrUpdtRaCaseNoteRequest>(req => createRequest = req);
                var retentionAlertCase = await retentionAlertRepository.AddRetentionAlertCaseAsync(retentionAlertCaseEntity);
            }

            [TestMethod]
            [ExpectedException(typeof(Exception))]
            public async Task AddOrUpdateRetentionAlertCase_ResponseHasErrorMessage()
            {
                var retentionAlertCaseEntity = AddRetentionAlertCase_WithNullValues();
                AddOrUpdtRaCaseNoteResponse createResponse = new AddOrUpdtRaCaseNoteResponse();
                createResponse.AlErrorMessages = new List<string> { "Error text" };
                mockManager.Setup(mgr => mgr.ExecuteAsync<AddOrUpdtRaCaseNoteRequest, AddOrUpdtRaCaseNoteResponse>(It.IsAny<AddOrUpdtRaCaseNoteRequest>())).Returns(Task.FromResult(createResponse)).Callback<AddOrUpdtRaCaseNoteRequest>(req => createRequest = req);
                var retentionAlertCase = await retentionAlertRepository.AddRetentionAlertCaseAsync(retentionAlertCaseEntity);
            }

            [TestMethod]
            public async Task AddRetentionAlertCase_Success()
            {
                var retentionAlertCaseEntity = AddRetentionAlertCase_WithSuccess();
                AddOrUpdtRaCaseNoteResponse createResponse = new AddOrUpdtRaCaseNoteResponse()
                {
                    ACaseId = "31",
                    ACaseItemsId = "32",
                    ACaseStatus = "ACTIVE",
                    AlErrorMessages = new List<string>(),
                    AlOwnerIds = new List<string>(),
                    AlOwnerNames = new List<string>(),
                    AlOwnerRoles = new List<string>() { "Advisor", "Faculty" },
                    AlOwnerRoleTitles = new List<string>()
                };

                mockManager.Setup(mgr => mgr.ExecuteAsync<AddOrUpdtRaCaseNoteRequest, AddOrUpdtRaCaseNoteResponse>(It.IsAny<AddOrUpdtRaCaseNoteRequest>())).Returns(Task.FromResult(createResponse)).Callback<AddOrUpdtRaCaseNoteRequest>(req => createRequest = req);
                var retentionAlertCase = await retentionAlertRepository.AddRetentionAlertCaseAsync(retentionAlertCaseEntity);

                Assert.AreEqual(retentionAlertCase.CaseId, "31");
                Assert.AreEqual(retentionAlertCase.CaseItemsId, "32");
                Assert.AreEqual(retentionAlertCase.CaseStatus, "ACTIVE");
                Assert.AreEqual(retentionAlertCase.ErrorMessages.Count, 0);
                Assert.AreEqual(retentionAlertCase.OwnerIds.Count, 0);
                Assert.AreEqual(retentionAlertCase.OwnerNames.Count, 0);
                Assert.AreEqual(retentionAlertCase.OwnerRoles[0], "Advisor");
                Assert.AreEqual(retentionAlertCase.OwnerRoles[1], "Faculty");
                Assert.AreEqual(retentionAlertCase.OwnerRoleTitles.Count, 0);
            }

            private RetentionAlertCase AddRetentionAlertCase_WithSuccess()
            {
                var retentionAlertCaseEntity = new RetentionAlertCase("0000011")
                {
                    CaseType = "EARLY.ALERT",
                    MethodOfContact = new List<string> { "Email" },
                    Notes = new List<string> { "case notes" },
                    Summary = "case summary"
                };
                return retentionAlertCaseEntity;
            }

            private RetentionAlertCase AddRetentionAlertCase_WithNullValues()
            {
                var retentionAlertCaseEntity = new RetentionAlertCase("0000011")
                {
                    CaseType = "",
                    MethodOfContact = null,
                    Notes = null,
                    Summary = null
                };
                return retentionAlertCaseEntity;
            }

        }

        [TestClass]
        public class RetentionAlertRepository_UpdateRetentionAlertCase
        {
            Mock<IColleagueTransactionFactory> transFactoryMock;
            Mock<IColleagueDataReader> dataAccessorMock;
            Mock<ICacheProvider> cacheProviderMock;
            Mock<ILogger> loggerMock;
            Mock<ApiSettings> settings;
            Mock<IColleagueTransactionInvoker> mockManager;
            private IRetentionAlertRepository retentionAlertRepository;
            AddOrUpdtRaCaseNoteRequest createRequest;

            [TestInitialize]
            public void Initialize()
            {
                settings = new Mock<ApiSettings>();
                loggerMock = new Mock<ILogger>();
                cacheProviderMock = new Mock<ICacheProvider>();
                transFactoryMock = new Mock<IColleagueTransactionFactory>();
                dataAccessorMock = new Mock<IColleagueDataReader>();
                mockManager = new Mock<IColleagueTransactionInvoker>();
                cacheProviderMock.Setup<Task<Tuple<object, SemaphoreSlim>>>(x =>
                x.GetAndLockSemaphoreAsync(It.IsAny<string>(), null))
                .Returns(Task.FromResult(new Tuple<object, SemaphoreSlim>(
                    null,
                    new SemaphoreSlim(1, 1)
                    )));
                // Set up data accessor for the transaction factory
                transFactoryMock.Setup(transFac => transFac.GetDataReader()).Returns(dataAccessorMock.Object);
                // Set up successful response to a transaction request, capturing the completed request for verification
                transFactoryMock.Setup(transFac => transFac.GetTransactionInvoker()).Returns(mockManager.Object);
                retentionAlertRepository = new RetentionAlertRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object, settings.Object);
            }

            [TestCleanup]
            public void Cleanup()
            {
                transFactoryMock = null;
                dataAccessorMock = null;
                cacheProviderMock = null;
            }

            [TestMethod]
            [ExpectedException(typeof(Exception))]
            public async Task UpdateRetentionAlertCase_WithAllNull()
            {
                var retentionAlertCaseEntity = UpdateRetentionAlertCase_WithNullValues();
                AddOrUpdtRaCaseNoteResponse createResponse = new AddOrUpdtRaCaseNoteResponse();
                createResponse.AlErrorMessages = new List<string> { "Student id is required" };
                mockManager.Setup(mgr => mgr.ExecuteAsync<AddOrUpdtRaCaseNoteRequest, AddOrUpdtRaCaseNoteResponse>(It.IsAny<AddOrUpdtRaCaseNoteRequest>())).Returns(Task.FromResult(createResponse)).Callback<AddOrUpdtRaCaseNoteRequest>(req => createRequest = req);
                var retentionAlertCase = await retentionAlertRepository.UpdateRetentionAlertCaseAsync("31", retentionAlertCaseEntity);
            }

            [TestMethod]
            [ExpectedException(typeof(Exception))]
            public async Task UpdateRetentionAlertCase_ResponseHasErrorMessage()
            {
                var retentionAlertCaseEntity = UpdateRetentionAlertCase_WithNullValues();
                AddOrUpdtRaCaseNoteResponse createResponse = new AddOrUpdtRaCaseNoteResponse();
                createResponse.AlErrorMessages = new List<string> { "Error text" };
                mockManager.Setup(mgr => mgr.ExecuteAsync<AddOrUpdtRaCaseNoteRequest, AddOrUpdtRaCaseNoteResponse>(It.IsAny<AddOrUpdtRaCaseNoteRequest>())).Returns(Task.FromResult(createResponse)).Callback<AddOrUpdtRaCaseNoteRequest>(req => createRequest = req);
                var retentionAlertCase = await retentionAlertRepository.UpdateRetentionAlertCaseAsync("31", retentionAlertCaseEntity);
            }

            [TestMethod]
            public async Task UpdateRetentionAlertCase_Success()
            {
                var retentionAlertCaseEntity = UpdateRetentionAlertCase_WithSuccess();
                AddOrUpdtRaCaseNoteResponse createResponse = new AddOrUpdtRaCaseNoteResponse()
                {
                    ACaseId = "31",
                    ACaseItemsId = "32",
                    ACaseStatus = "ACTIVE",
                    AlErrorMessages = new List<string>(),
                    AlOwnerIds = new List<string>(),
                    AlOwnerNames = new List<string>(),
                    AlOwnerRoles = new List<string>() { "Advisor", "Faculty" },
                    AlOwnerRoleTitles = new List<string>()
                };

                mockManager.Setup(mgr => mgr.ExecuteAsync<AddOrUpdtRaCaseNoteRequest, AddOrUpdtRaCaseNoteResponse>(It.IsAny<AddOrUpdtRaCaseNoteRequest>())).Returns(Task.FromResult(createResponse)).Callback<AddOrUpdtRaCaseNoteRequest>(req => createRequest = req);
                var retentionAlertCase = await retentionAlertRepository.UpdateRetentionAlertCaseAsync("31", retentionAlertCaseEntity);

                Assert.AreEqual(retentionAlertCase.CaseId, "31");
                Assert.AreEqual(retentionAlertCase.CaseItemsId, "32");
                Assert.AreEqual(retentionAlertCase.CaseStatus, "ACTIVE");
                Assert.AreEqual(retentionAlertCase.ErrorMessages.Count, 0);
                Assert.AreEqual(retentionAlertCase.OwnerIds.Count, 0);
                Assert.AreEqual(retentionAlertCase.OwnerNames.Count, 0);
                Assert.AreEqual(retentionAlertCase.OwnerRoles[0], "Advisor");
                Assert.AreEqual(retentionAlertCase.OwnerRoles[1], "Faculty");
                Assert.AreEqual(retentionAlertCase.OwnerRoleTitles.Count, 0);
            }

            private RetentionAlertCase UpdateRetentionAlertCase_WithSuccess()
            {
                var retentionAlertCaseEntity = new RetentionAlertCase("0000011")
                {
                    CaseType = "EARLY.ALERT",
                    MethodOfContact = new List<string> { "Email" },
                    Notes = new List<string> { "case notes" },
                    Summary = "case summary"
                };
                return retentionAlertCaseEntity;
            }

            private RetentionAlertCase UpdateRetentionAlertCase_WithNullValues()
            {
                var retentionAlertCaseEntity = new RetentionAlertCase("0000011")
                {
                    CaseType = "",
                    MethodOfContact = null,
                    Notes = null,
                    Summary = null
                };
                return retentionAlertCaseEntity;
            }

        }

        [TestClass]
        public class RetentionAlertRepository_GetRetentionAlertContributionsAsync
        {
            Mock<IColleagueTransactionFactory> transFactoryMock;
            Mock<IColleagueDataReader> dataAccessorMock;
            Mock<ICacheProvider> cacheProviderMock;
            Mock<ILogger> loggerMock;
            Mock<ApiSettings> settings;
            Mock<IColleagueTransactionInvoker> mockManager;
            private IRetentionAlertRepository retentionAlertRepository;
            GetMyCaseContributionsRequest getRequest;
            private ContributionsQueryCriteria contributionsQueryCriteria;

            [TestInitialize]
            public void Initialize()
            {
                settings = new Mock<ApiSettings>();
                loggerMock = new Mock<ILogger>();
                cacheProviderMock = new Mock<ICacheProvider>();
                transFactoryMock = new Mock<IColleagueTransactionFactory>();
                dataAccessorMock = new Mock<IColleagueDataReader>();
                mockManager = new Mock<IColleagueTransactionInvoker>();
                cacheProviderMock.Setup<Task<Tuple<object, SemaphoreSlim>>>(x =>
                x.GetAndLockSemaphoreAsync(It.IsAny<string>(), null))
                .Returns(Task.FromResult(new Tuple<object, SemaphoreSlim>(
                    null,
                    new SemaphoreSlim(1, 1)
                    )));

                contributionsQueryCriteria = new ContributionsQueryCriteria() { IncludeCasesOverOneYear = false, IncludeClosedCases = false, IncludeOwnedCases = false };

                // Set up data accessor for the transaction factory
                transFactoryMock.Setup(transFac => transFac.GetDataReader()).Returns(dataAccessorMock.Object);
                // Set up successful response to a transaction request, capturing the completed request for verification
                transFactoryMock.Setup(transFac => transFac.GetTransactionInvoker()).Returns(mockManager.Object);
                retentionAlertRepository = new RetentionAlertRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object, settings.Object);
            }

            [TestCleanup]
            public void Cleanup()
            {
                transFactoryMock = null;
                dataAccessorMock = null;
                cacheProviderMock = null;
            }

            [TestMethod]
            [ExpectedException(typeof(RepositoryException))]
            public async Task GetRetentionAlertCases_ResponseHasErrorMessage()
            {
                GetMyCaseContributionsResponse getResponse = new GetMyCaseContributionsResponse()
                {
                    AlErrorMessages = new List<string> { "Error text" }
                };
                mockManager.Setup(mgr => mgr.ExecuteAsync<GetMyCaseContributionsRequest, GetMyCaseContributionsResponse>(It.IsAny<GetMyCaseContributionsRequest>())).Returns(Task.FromResult(getResponse)).Callback<GetMyCaseContributionsRequest>(req => getRequest = req);
                var contributions = await retentionAlertRepository.GetRetentionAlertContributionsAsync("000013", contributionsQueryCriteria);
            }

            [TestMethod]
            public async Task GetRetentionAlertCases_Success()
            {
                GetMyCaseContributionsResponse caseResponse = new GetMyCaseContributionsResponse()
                {
                    AlErrorMessages = new List<string>(),
                    MyCaseContributions = new List<MyCaseContributions>()
                    {
                        new MyCaseContributions()
                        {
                            AlCaseIds = "41",
                            AlCaseItemIds = "260",
                            AlCaseOwners = "AL,Advisor",
                            AlCaseItemDates = DateTime.Now.AddDays(-5),
                            AlCaseStatuses = "New",
                            AlCaseSummaries = "Summary",
                            AlCategories = "EARLY.ALERT",
                            AlCategoryDescriptions = "Description",
                            AlDateCreated = DateTime.Now.AddDays(-5),
                            AlStudents = "000013"
                        }
                    }
                };

                mockManager.Setup(mgr => mgr.ExecuteAsync<GetMyCaseContributionsRequest, GetMyCaseContributionsResponse>(It.IsAny<GetMyCaseContributionsRequest>())).Returns(Task.FromResult(caseResponse)).Callback<GetMyCaseContributionsRequest>(req => getRequest = req);
                var contributions = await retentionAlertRepository.GetRetentionAlertContributionsAsync("000013", contributionsQueryCriteria);

                // Assert
                Assert.IsTrue(contributions is IEnumerable<RetentionAlertWorkCase>);
                Assert.AreEqual(1, contributions.Count);
            }
        }

        [TestClass]
        public class RetentionAlertRepository_GetRetentionAlertOpenCasesAsync
        {
            Mock<IColleagueTransactionFactory> transFactoryMock;
            Mock<IColleagueDataReader> dataAccessorMock;
            Mock<ICacheProvider> cacheProviderMock;
            Mock<ILogger> loggerMock;
            Mock<ApiSettings> settings;
            Mock<IColleagueTransactionInvoker> mockManager;
            private IRetentionAlertRepository retentionAlertRepository;
            GetRaOpenCasesForReportingRequest getRequest;

            [TestInitialize]
            public void Initialize()
            {
                settings = new Mock<ApiSettings>();
                loggerMock = new Mock<ILogger>();
                cacheProviderMock = new Mock<ICacheProvider>();
                transFactoryMock = new Mock<IColleagueTransactionFactory>();
                dataAccessorMock = new Mock<IColleagueDataReader>();
                mockManager = new Mock<IColleagueTransactionInvoker>();
                cacheProviderMock.Setup<Task<Tuple<object, SemaphoreSlim>>>(x =>
                x.GetAndLockSemaphoreAsync(It.IsAny<string>(), null))
                .Returns(Task.FromResult(new Tuple<object, SemaphoreSlim>(
                    null,
                    new SemaphoreSlim(1, 1)
                    )));
                // Set up data accessor for the transaction factory
                transFactoryMock.Setup(transFac => transFac.GetDataReader()).Returns(dataAccessorMock.Object);
                // Set up successful response to a transaction request, capturing the completed request for verification
                transFactoryMock.Setup(transFac => transFac.GetTransactionInvoker()).Returns(mockManager.Object);
                retentionAlertRepository = new RetentionAlertRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object, settings.Object);
            }

            [TestCleanup]
            public void Cleanup()
            {
                transFactoryMock = null;
                dataAccessorMock = null;
                cacheProviderMock = null;
            }

            [TestMethod]
            [ExpectedException(typeof(RepositoryException))]
            public async Task GetRetentionAlertOpenCases_ResponseHasErrorMessage()
            {
                GetRaOpenCasesForReportingResponse getResponse = new GetRaOpenCasesForReportingResponse()
                {
                    AlErrorMessages = new List<string> { "Error text" }
                };
                mockManager.Setup(mgr => mgr.ExecuteAsync<GetRaOpenCasesForReportingRequest, GetRaOpenCasesForReportingResponse>(It.IsAny<GetRaOpenCasesForReportingRequest>())).Returns(Task.FromResult(getResponse)).Callback<GetRaOpenCasesForReportingRequest>(req => getRequest = req);
                var openCases = await retentionAlertRepository.GetRetentionAlertOpenCasesAsync("000013");
            }

            [TestMethod]
            public async Task GetRetentionAlertOpenCases_Success()
            {
                GetRaOpenCasesForReportingResponse getResponse = new GetRaOpenCasesForReportingResponse()
                {
                    AlErrorMessages = new List<string>(),
                    OpenCases = new List<OpenCases>()
                    {
                        new OpenCases()
                        {
                            AlCategoryId = "3",
                            AlCategory = "Early Alert",
                            AlThirtyDaysOld = "30,40,50",
                            AlSixtyDaysOld = "45",
                            AlNinetyDaysOld = "",
                            AlOverNinetyDaysOld = "12,23",
                            AlTotalOpenCases = "6"
                        }
                    }
                };

                mockManager.Setup(mgr => mgr.ExecuteAsync<GetRaOpenCasesForReportingRequest, GetRaOpenCasesForReportingResponse>(It.IsAny<GetRaOpenCasesForReportingRequest>())).Returns(Task.FromResult(getResponse)).Callback<GetRaOpenCasesForReportingRequest>(req => getRequest = req);
                var openCases = await retentionAlertRepository.GetRetentionAlertOpenCasesAsync("000013");

                // Assert
                Assert.IsTrue(openCases is IEnumerable<RetentionAlertOpenCase>);
                Assert.AreEqual(1, openCases.Count);
            }
        }

        [TestClass]
        public class RetentionAlertRepository_GetRetentionAlertCaseDetailAsync
        {
            Mock<IColleagueTransactionFactory> transFactoryMock;
            Mock<IColleagueDataReader> dataAccessorMock;
            Mock<ICacheProvider> cacheProviderMock;
            Mock<ILogger> loggerMock;
            Mock<ApiSettings> settings;
            Mock<IColleagueTransactionInvoker> mockManager;
            private IRetentionAlertRepository retentionAlertRepository;
            GetRaCaseDetailsRequest getRequest;

            [TestInitialize]
            public void Initialize()
            {
                settings = new Mock<ApiSettings>();
                loggerMock = new Mock<ILogger>();
                cacheProviderMock = new Mock<ICacheProvider>();
                transFactoryMock = new Mock<IColleagueTransactionFactory>();
                dataAccessorMock = new Mock<IColleagueDataReader>();
                mockManager = new Mock<IColleagueTransactionInvoker>();
                cacheProviderMock.Setup<Task<Tuple<object, SemaphoreSlim>>>(x =>
                x.GetAndLockSemaphoreAsync(It.IsAny<string>(), null))
                .Returns(Task.FromResult(new Tuple<object, SemaphoreSlim>(
                    null,
                    new SemaphoreSlim(1, 1)
                    )));
                // Set up data accessor for the transaction factory
                transFactoryMock.Setup(transFac => transFac.GetDataReader()).Returns(dataAccessorMock.Object);
                // Set up successful response to a transaction request, capturing the completed request for verification
                transFactoryMock.Setup(transFac => transFac.GetTransactionInvoker()).Returns(mockManager.Object);
                retentionAlertRepository = new RetentionAlertRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object, settings.Object);
            }

            [TestCleanup]
            public void Cleanup()
            {
                transFactoryMock = null;
                dataAccessorMock = null;
                cacheProviderMock = null;
            }

            [TestMethod]
            [ExpectedException(typeof(Exception))]
            public async Task GetRetentionAlertCaseDetail_ResponseHasErrorMessage()
            {
                GetRaCaseDetailsResponse getResponse = new GetRaCaseDetailsResponse()
                {
                    AlErrorMessages = new List<string> { "Error text" }
                };
                mockManager.Setup(mgr => mgr.ExecuteAsync<GetRaCaseDetailsRequest, GetRaCaseDetailsResponse>(It.IsAny<GetRaCaseDetailsRequest>())).Returns(Task.FromResult(getResponse)).Callback<GetRaCaseDetailsRequest>(req => getRequest = req);
                var caseDetails = await retentionAlertRepository.GetRetentionAlertCaseDetailAsync("31");
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task GetRetentionAlertCaseDetail_NullCaseId()
            {
                var caseDetails = await retentionAlertRepository.GetRetentionAlertCaseDetailAsync("");
            }

            [TestMethod]
            public async Task GetRetentionAlertCases_Success()
            {
                List<CaseHistory> caseHistory = new List<CaseHistory>()
                {
                    new CaseHistory()
                    {
                        AlCaseItemTypes = "Case Item Type 1",
                        AlContactMethods = "Phone",
                        AlDetailedNotes = "Detailed Notes",
                        AlUpdatedBy = "0000011"
                    },
                    new CaseHistory()
                    {
                        AlCaseItemTypes = "Case Item Type 2",
                        AlContactMethods = "Email",
                        AlDetailedNotes = "Detailed Notes 2",
                        AlUpdatedBy = "0000012"
                    }
                };

                GetRaCaseDetailsResponse caseResponse = new GetRaCaseDetailsResponse()
                {
                    AlErrorMessages = new List<string>(),
                    CaseHistory = caseHistory,
                    AStatus = "New",
                    ACaseOwner = "case owner",
                    ACaseType = "Case Type",
                    ACategoryName = "EARLY.ALERT",
                    ACategory = "Early.Alert",
                    ACategoryId = "1",
                    ACreatedBy = "0000010",
                    ACasePriority = "Medium",
                    AStudentId = "0000015"
                };

                mockManager.Setup(mgr => mgr.ExecuteAsync<GetRaCaseDetailsRequest, GetRaCaseDetailsResponse>(It.IsAny<GetRaCaseDetailsRequest>())).Returns(Task.FromResult(caseResponse)).Callback<GetRaCaseDetailsRequest>(req => getRequest = req);
                var caseDetail = await retentionAlertRepository.GetRetentionAlertCaseDetailAsync("31");

                // Assert
                Assert.IsTrue(caseDetail is RetentionAlertCaseDetail);
                Assert.IsNotNull(caseDetail);
                Assert.AreEqual(caseResponse.AStatus, caseDetail.Status);
                Assert.AreEqual(caseResponse.ACaseOwner, caseDetail.CaseOwner);
                Assert.AreEqual(caseResponse.ACaseType, caseDetail.CaseType);
                Assert.AreEqual(caseResponse.ACategoryName, caseDetail.CategoryName);
                Assert.AreEqual(caseResponse.ACategoryId, caseDetail.CategoryId);
                Assert.AreEqual(caseResponse.ACreatedBy, caseDetail.CreatedBy);
                Assert.AreEqual(caseResponse.ACasePriority, caseDetail.Priority);
                Assert.AreEqual(caseResponse.AStudentId, caseDetail.StudentId);
            }
        }

        [TestClass]
        public class RetentionAlertRepository_WorkCaseActions
        {
            Mock<IColleagueTransactionFactory> transFactoryMock;
            Mock<IColleagueDataReader> dataAccessorMock;
            Mock<ICacheProvider> cacheProviderMock;
            Mock<ILogger> loggerMock;
            Mock<ApiSettings> settings;
            Mock<IColleagueTransactionInvoker> mockManager;
            private IRetentionAlertRepository retentionAlertRepository;
            UpdtRaCaseRequest createRequest;

            [TestInitialize]
            public void Initialize()
            {
                settings = new Mock<ApiSettings>();
                loggerMock = new Mock<ILogger>();
                cacheProviderMock = new Mock<ICacheProvider>();
                transFactoryMock = new Mock<IColleagueTransactionFactory>();
                dataAccessorMock = new Mock<IColleagueDataReader>();
                mockManager = new Mock<IColleagueTransactionInvoker>();
                cacheProviderMock.Setup<Task<Tuple<object, SemaphoreSlim>>>(x =>
                x.GetAndLockSemaphoreAsync(It.IsAny<string>(), null))
                .Returns(Task.FromResult(new Tuple<object, SemaphoreSlim>(
                    null,
                    new SemaphoreSlim(1, 1)
                    )));
                // Set up data accessor for the transaction factory
                transFactoryMock.Setup(transFac => transFac.GetDataReader()).Returns(dataAccessorMock.Object);
                // Set up successful response to a transaction request, capturing the completed request for verification
                transFactoryMock.Setup(transFac => transFac.GetTransactionInvoker()).Returns(mockManager.Object);
                retentionAlertRepository = new RetentionAlertRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object, settings.Object);
            }

            [TestCleanup]
            public void Cleanup()
            {
                transFactoryMock = null;
                dataAccessorMock = null;
                cacheProviderMock = null;
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task AddRetentionAlertCaseNote_ArgumentCheck_0()
            {
                var retentionAlertCaseEntity = new RetentionAlertWorkCaseNote(null, null, null);
                UpdtRaCaseResponse response = new UpdtRaCaseResponse();
                mockManager.Setup(mgr => mgr.ExecuteAsync<UpdtRaCaseRequest, UpdtRaCaseResponse>(It.IsAny<UpdtRaCaseRequest>())).Returns(Task.FromResult(response)).Callback<UpdtRaCaseRequest>(req => createRequest = req);
                var retentionAlertCase = await retentionAlertRepository.AddRetentionAlertCaseNoteAsync(null, retentionAlertCaseEntity);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task AddRetentionAlertCaseNote_ArgumentCheck_1()
            {
                var retentionAlertCaseEntity = new RetentionAlertWorkCaseNote("1", null, null);
                UpdtRaCaseResponse response = new UpdtRaCaseResponse();
                mockManager.Setup(mgr => mgr.ExecuteAsync<UpdtRaCaseRequest, UpdtRaCaseResponse>(It.IsAny<UpdtRaCaseRequest>())).Returns(Task.FromResult(response)).Callback<UpdtRaCaseRequest>(req => createRequest = req);
                var retentionAlertCase = await retentionAlertRepository.AddRetentionAlertCaseNoteAsync(null, retentionAlertCaseEntity);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task AddRetentionAlertCaseNote_ArgumentCheck_2()
            {
                var retentionAlertCaseEntity = new RetentionAlertWorkCaseNote("1", "2", null);
                UpdtRaCaseResponse response = new UpdtRaCaseResponse();
                mockManager.Setup(mgr => mgr.ExecuteAsync<UpdtRaCaseRequest, UpdtRaCaseResponse>(It.IsAny<UpdtRaCaseRequest>())).Returns(Task.FromResult(response)).Callback<UpdtRaCaseRequest>(req => createRequest = req);
                var retentionAlertCase = await retentionAlertRepository.AddRetentionAlertCaseNoteAsync(null, retentionAlertCaseEntity);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task AddRetentionAlertCaseNote_ArgumentCheck_3()
            {
                var retentionAlertCaseEntity = new RetentionAlertWorkCaseNote("1", "2", new List<string>() { "3" });
                UpdtRaCaseResponse response = new UpdtRaCaseResponse();
                mockManager.Setup(mgr => mgr.ExecuteAsync<UpdtRaCaseRequest, UpdtRaCaseResponse>(It.IsAny<UpdtRaCaseRequest>())).Returns(Task.FromResult(response)).Callback<UpdtRaCaseRequest>(req => createRequest = req);
                var retentionAlertCase = await retentionAlertRepository.AddRetentionAlertCaseNoteAsync(null, retentionAlertCaseEntity);

                Assert.IsNotNull(retentionAlertCase);
                Assert.AreEqual("1", retentionAlertCase.CaseId);
                Assert.AreEqual(false, retentionAlertCase.HasError);
                Assert.AreEqual(0, retentionAlertCase.ErrorMessages.Count);
            }


            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task AddRetentionAlertCaseNote_ArgumentCheck_4()
            {
                var retentionAlertCaseEntity = new RetentionAlertWorkCaseNote(null, null, null);
                UpdtRaCaseResponse response = new UpdtRaCaseResponse();
                mockManager.Setup(mgr => mgr.ExecuteAsync<UpdtRaCaseRequest, UpdtRaCaseResponse>(It.IsAny<UpdtRaCaseRequest>())).Returns(Task.FromResult(response)).Callback<UpdtRaCaseRequest>(req => createRequest = req);
                var retentionAlertCase = await retentionAlertRepository.AddRetentionAlertCaseNoteAsync("1", retentionAlertCaseEntity);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task AddRetentionAlertCaseNote_ArgumentCheck_5()
            {
                var retentionAlertCaseEntity = new RetentionAlertWorkCaseNote("1", null, null);
                UpdtRaCaseResponse response = new UpdtRaCaseResponse();
                mockManager.Setup(mgr => mgr.ExecuteAsync<UpdtRaCaseRequest, UpdtRaCaseResponse>(It.IsAny<UpdtRaCaseRequest>())).Returns(Task.FromResult(response)).Callback<UpdtRaCaseRequest>(req => createRequest = req);
                var retentionAlertCase = await retentionAlertRepository.AddRetentionAlertCaseNoteAsync("1", retentionAlertCaseEntity);
            }

            [TestMethod]
            public async Task AddRetentionAlertCaseNote_ArgumentCheck_6()
            {
                var retentionAlertCaseEntity = new RetentionAlertWorkCaseNote("1", "2", null);
                UpdtRaCaseResponse response = new UpdtRaCaseResponse();
                mockManager.Setup(mgr => mgr.ExecuteAsync<UpdtRaCaseRequest, UpdtRaCaseResponse>(It.IsAny<UpdtRaCaseRequest>())).Returns(Task.FromResult(response)).Callback<UpdtRaCaseRequest>(req => createRequest = req);
                var retentionAlertCase = await retentionAlertRepository.AddRetentionAlertCaseNoteAsync("1", retentionAlertCaseEntity);
            }

            [TestMethod]
            public async Task AddRetentionAlertCaseNote_ArgumentCheck_7()
            {
                var retentionAlertCaseEntity = new RetentionAlertWorkCaseNote("1", "2", new List<string>() { "3" });
                UpdtRaCaseResponse response = new UpdtRaCaseResponse();
                mockManager.Setup(mgr => mgr.ExecuteAsync<UpdtRaCaseRequest, UpdtRaCaseResponse>(It.IsAny<UpdtRaCaseRequest>())).Returns(Task.FromResult(response)).Callback<UpdtRaCaseRequest>(req => createRequest = req);
                var retentionAlertCase = await retentionAlertRepository.AddRetentionAlertCaseNoteAsync("1", retentionAlertCaseEntity);

                Assert.IsNotNull(retentionAlertCase);
                Assert.AreEqual("1", retentionAlertCase.CaseId);
                Assert.AreEqual(false, retentionAlertCase.HasError);
                Assert.AreEqual(0, retentionAlertCase.ErrorMessages.Count);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task AddRetentionAlertCaseCommCode_ArgumentCheck_0()
            {
                var retentionAlertCaseEntity = new RetentionAlertWorkCaseCommCode(null, null);
                UpdtRaCaseResponse response = new UpdtRaCaseResponse();
                mockManager.Setup(mgr => mgr.ExecuteAsync<UpdtRaCaseRequest, UpdtRaCaseResponse>(It.IsAny<UpdtRaCaseRequest>())).Returns(Task.FromResult(response)).Callback<UpdtRaCaseRequest>(req => createRequest = req);
                var retentionAlertCase = await retentionAlertRepository.AddRetentionAlertCaseCommCodeAsync(null, retentionAlertCaseEntity);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task AddRetentionAlertCaseCommCode_ArgumentCheck_1()
            {
                var retentionAlertCaseEntity = new RetentionAlertWorkCaseCommCode("1", null);
                UpdtRaCaseResponse response = new UpdtRaCaseResponse();
                mockManager.Setup(mgr => mgr.ExecuteAsync<UpdtRaCaseRequest, UpdtRaCaseResponse>(It.IsAny<UpdtRaCaseRequest>())).Returns(Task.FromResult(response)).Callback<UpdtRaCaseRequest>(req => createRequest = req);
                var retentionAlertCase = await retentionAlertRepository.AddRetentionAlertCaseCommCodeAsync(null, retentionAlertCaseEntity);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task AddRetentionAlertCaseCommCode_ArgumentCheck_2()
            {
                var retentionAlertCaseEntity = new RetentionAlertWorkCaseCommCode("1", "2");
                UpdtRaCaseResponse response = new UpdtRaCaseResponse();
                mockManager.Setup(mgr => mgr.ExecuteAsync<UpdtRaCaseRequest, UpdtRaCaseResponse>(It.IsAny<UpdtRaCaseRequest>())).Returns(Task.FromResult(response)).Callback<UpdtRaCaseRequest>(req => createRequest = req);
                var retentionAlertCase = await retentionAlertRepository.AddRetentionAlertCaseCommCodeAsync(null, retentionAlertCaseEntity);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task AddRetentionAlertCaseCommCode_ArgumentCheck_3()
            {
                var retentionAlertCaseEntity = new RetentionAlertWorkCaseCommCode(null, null);
                UpdtRaCaseResponse response = new UpdtRaCaseResponse();
                mockManager.Setup(mgr => mgr.ExecuteAsync<UpdtRaCaseRequest, UpdtRaCaseResponse>(It.IsAny<UpdtRaCaseRequest>())).Returns(Task.FromResult(response)).Callback<UpdtRaCaseRequest>(req => createRequest = req);
                var retentionAlertCase = await retentionAlertRepository.AddRetentionAlertCaseCommCodeAsync("1", retentionAlertCaseEntity);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task AddRetentionAlertCaseCommCode_ArgumentCheck_4()
            {
                var retentionAlertCaseEntity = new RetentionAlertWorkCaseCommCode("1", null);
                UpdtRaCaseResponse response = new UpdtRaCaseResponse();
                mockManager.Setup(mgr => mgr.ExecuteAsync<UpdtRaCaseRequest, UpdtRaCaseResponse>(It.IsAny<UpdtRaCaseRequest>())).Returns(Task.FromResult(response)).Callback<UpdtRaCaseRequest>(req => createRequest = req);
                var retentionAlertCase = await retentionAlertRepository.AddRetentionAlertCaseCommCodeAsync("1", retentionAlertCaseEntity);
            }

            [TestMethod]
            public async Task AddRetentionAlertCaseCommCode_ArgumentCheck_5()
            {
                var retentionAlertCaseEntity = new RetentionAlertWorkCaseCommCode("1", "2");
                UpdtRaCaseResponse response = new UpdtRaCaseResponse();
                mockManager.Setup(mgr => mgr.ExecuteAsync<UpdtRaCaseRequest, UpdtRaCaseResponse>(It.IsAny<UpdtRaCaseRequest>())).Returns(Task.FromResult(response)).Callback<UpdtRaCaseRequest>(req => createRequest = req);
                var retentionAlertCase = await retentionAlertRepository.AddRetentionAlertCaseCommCodeAsync("1", retentionAlertCaseEntity);

                Assert.IsNotNull(retentionAlertCase);
                Assert.AreEqual("1", retentionAlertCase.CaseId);
                Assert.AreEqual(false, retentionAlertCase.HasError);
                Assert.AreEqual(0, retentionAlertCase.ErrorMessages.Count);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task AddRetentionAlertCaseType_ArgumentCheck_0()
            {
                var retentionAlertCaseEntity = new RetentionAlertWorkCaseType(null, null, null);
                UpdtRaCaseResponse response = new UpdtRaCaseResponse();
                mockManager.Setup(mgr => mgr.ExecuteAsync<UpdtRaCaseRequest, UpdtRaCaseResponse>(It.IsAny<UpdtRaCaseRequest>())).Returns(Task.FromResult(response)).Callback<UpdtRaCaseRequest>(req => createRequest = req);
                var retentionAlertCase = await retentionAlertRepository.AddRetentionAlertCaseTypeAsync(null, retentionAlertCaseEntity);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task AddRetentionAlertCaseType_ArgumentCheck_1()
            {
                var retentionAlertCaseEntity = new RetentionAlertWorkCaseType("1", null, null);
                UpdtRaCaseResponse response = new UpdtRaCaseResponse();
                mockManager.Setup(mgr => mgr.ExecuteAsync<UpdtRaCaseRequest, UpdtRaCaseResponse>(It.IsAny<UpdtRaCaseRequest>())).Returns(Task.FromResult(response)).Callback<UpdtRaCaseRequest>(req => createRequest = req);
                var retentionAlertCase = await retentionAlertRepository.AddRetentionAlertCaseTypeAsync(null, retentionAlertCaseEntity);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task AddRetentionAlertCaseType_ArgumentCheck_2()
            {
                var retentionAlertCaseEntity = new RetentionAlertWorkCaseType("1", "2", null);
                UpdtRaCaseResponse response = new UpdtRaCaseResponse();
                mockManager.Setup(mgr => mgr.ExecuteAsync<UpdtRaCaseRequest, UpdtRaCaseResponse>(It.IsAny<UpdtRaCaseRequest>())).Returns(Task.FromResult(response)).Callback<UpdtRaCaseRequest>(req => createRequest = req);
                var retentionAlertCase = await retentionAlertRepository.AddRetentionAlertCaseTypeAsync(null, retentionAlertCaseEntity);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task AddRetentionAlertCaseType_ArgumentCheck_3()
            {
                var retentionAlertCaseEntity = new RetentionAlertWorkCaseType("1", "2", new List<string>() { "3" });
                UpdtRaCaseResponse response = new UpdtRaCaseResponse();
                mockManager.Setup(mgr => mgr.ExecuteAsync<UpdtRaCaseRequest, UpdtRaCaseResponse>(It.IsAny<UpdtRaCaseRequest>())).Returns(Task.FromResult(response)).Callback<UpdtRaCaseRequest>(req => createRequest = req);
                var retentionAlertCase = await retentionAlertRepository.AddRetentionAlertCaseTypeAsync(null, retentionAlertCaseEntity);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task AddRetentionAlertCaseType_ArgumentCheck_4()
            {
                var retentionAlertCaseEntity = new RetentionAlertWorkCaseType(null, null, null);
                UpdtRaCaseResponse response = new UpdtRaCaseResponse();
                mockManager.Setup(mgr => mgr.ExecuteAsync<UpdtRaCaseRequest, UpdtRaCaseResponse>(It.IsAny<UpdtRaCaseRequest>())).Returns(Task.FromResult(response)).Callback<UpdtRaCaseRequest>(req => createRequest = req);
                var retentionAlertCase = await retentionAlertRepository.AddRetentionAlertCaseTypeAsync("1", retentionAlertCaseEntity);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task AddRetentionAlertCaseType_ArgumentCheck_5()
            {
                var retentionAlertCaseEntity = new RetentionAlertWorkCaseType("1", null, null);
                UpdtRaCaseResponse response = new UpdtRaCaseResponse();
                mockManager.Setup(mgr => mgr.ExecuteAsync<UpdtRaCaseRequest, UpdtRaCaseResponse>(It.IsAny<UpdtRaCaseRequest>())).Returns(Task.FromResult(response)).Callback<UpdtRaCaseRequest>(req => createRequest = req);
                var retentionAlertCase = await retentionAlertRepository.AddRetentionAlertCaseTypeAsync("1", retentionAlertCaseEntity);
            }

            [TestMethod]
            public async Task AddRetentionAlertCaseType_ArgumentCheck_6()
            {
                var retentionAlertCaseEntity = new RetentionAlertWorkCaseType("1", "2", null);
                UpdtRaCaseResponse response = new UpdtRaCaseResponse();
                mockManager.Setup(mgr => mgr.ExecuteAsync<UpdtRaCaseRequest, UpdtRaCaseResponse>(It.IsAny<UpdtRaCaseRequest>())).Returns(Task.FromResult(response)).Callback<UpdtRaCaseRequest>(req => createRequest = req);
                var retentionAlertCase = await retentionAlertRepository.AddRetentionAlertCaseTypeAsync("1", retentionAlertCaseEntity);

                Assert.IsNotNull(retentionAlertCase);
                Assert.AreEqual("1", retentionAlertCase.CaseId);
                Assert.AreEqual(false, retentionAlertCase.HasError);
                Assert.AreEqual(0, retentionAlertCase.ErrorMessages.Count);
            }

            [TestMethod]
            public async Task AddRetentionAlertCaseType_ArgumentCheck_7()
            {
                var retentionAlertCaseEntity = new RetentionAlertWorkCaseType("1", "2", new List<string>() { "3" });
                UpdtRaCaseResponse response = new UpdtRaCaseResponse();
                mockManager.Setup(mgr => mgr.ExecuteAsync<UpdtRaCaseRequest, UpdtRaCaseResponse>(It.IsAny<UpdtRaCaseRequest>())).Returns(Task.FromResult(response)).Callback<UpdtRaCaseRequest>(req => createRequest = req);
                var retentionAlertCase = await retentionAlertRepository.AddRetentionAlertCaseTypeAsync("1", retentionAlertCaseEntity);

                Assert.IsNotNull(retentionAlertCase);
                Assert.AreEqual("1", retentionAlertCase.CaseId);
                Assert.AreEqual(false, retentionAlertCase.HasError);
                Assert.AreEqual(0, retentionAlertCase.ErrorMessages.Count);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task ChangeRetentionAlertCasePriority_ArgumentCheck_0()
            {
                var retentionAlertCaseEntity = new RetentionAlertWorkCasePriority(null, null);
                UpdtRaCaseResponse response = new UpdtRaCaseResponse();
                mockManager.Setup(mgr => mgr.ExecuteAsync<UpdtRaCaseRequest, UpdtRaCaseResponse>(It.IsAny<UpdtRaCaseRequest>())).Returns(Task.FromResult(response)).Callback<UpdtRaCaseRequest>(req => createRequest = req);
                var retentionAlertCase = await retentionAlertRepository.ChangeRetentionAlertCasePriorityAsync(null, retentionAlertCaseEntity);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task ChangeRetentionAlertCasePriority_ArgumentCheck_1()
            {
                var retentionAlertCaseEntity = new RetentionAlertWorkCasePriority("1", null);
                UpdtRaCaseResponse response = new UpdtRaCaseResponse();
                mockManager.Setup(mgr => mgr.ExecuteAsync<UpdtRaCaseRequest, UpdtRaCaseResponse>(It.IsAny<UpdtRaCaseRequest>())).Returns(Task.FromResult(response)).Callback<UpdtRaCaseRequest>(req => createRequest = req);
                var retentionAlertCase = await retentionAlertRepository.ChangeRetentionAlertCasePriorityAsync(null, retentionAlertCaseEntity);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task ChangeRetentionAlertCasePriority_ArgumentCheck_2()
            {
                var retentionAlertCaseEntity = new RetentionAlertWorkCasePriority("1", "2");
                UpdtRaCaseResponse response = new UpdtRaCaseResponse();
                mockManager.Setup(mgr => mgr.ExecuteAsync<UpdtRaCaseRequest, UpdtRaCaseResponse>(It.IsAny<UpdtRaCaseRequest>())).Returns(Task.FromResult(response)).Callback<UpdtRaCaseRequest>(req => createRequest = req);
                var retentionAlertCase = await retentionAlertRepository.ChangeRetentionAlertCasePriorityAsync(null, retentionAlertCaseEntity);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task ChangeRetentionAlertCasePriority_ArgumentCheck_3()
            {
                var retentionAlertCaseEntity = new RetentionAlertWorkCasePriority(null, null);
                UpdtRaCaseResponse response = new UpdtRaCaseResponse();
                mockManager.Setup(mgr => mgr.ExecuteAsync<UpdtRaCaseRequest, UpdtRaCaseResponse>(It.IsAny<UpdtRaCaseRequest>())).Returns(Task.FromResult(response)).Callback<UpdtRaCaseRequest>(req => createRequest = req);
                var retentionAlertCase = await retentionAlertRepository.ChangeRetentionAlertCasePriorityAsync("1", retentionAlertCaseEntity);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task ChangeRetentionAlertCasePriority_ArgumentCheck_4()
            {
                var retentionAlertCaseEntity = new RetentionAlertWorkCasePriority("1", null);
                UpdtRaCaseResponse response = new UpdtRaCaseResponse();
                mockManager.Setup(mgr => mgr.ExecuteAsync<UpdtRaCaseRequest, UpdtRaCaseResponse>(It.IsAny<UpdtRaCaseRequest>())).Returns(Task.FromResult(response)).Callback<UpdtRaCaseRequest>(req => createRequest = req);
                var retentionAlertCase = await retentionAlertRepository.ChangeRetentionAlertCasePriorityAsync("1", retentionAlertCaseEntity);
            }

            [TestMethod]
            public async Task ChangeRetentionAlertCasePriority_ArgumentCheck_5()
            {
                var retentionAlertCaseEntity = new RetentionAlertWorkCasePriority("1", "2");
                UpdtRaCaseResponse response = new UpdtRaCaseResponse();
                mockManager.Setup(mgr => mgr.ExecuteAsync<UpdtRaCaseRequest, UpdtRaCaseResponse>(It.IsAny<UpdtRaCaseRequest>())).Returns(Task.FromResult(response)).Callback<UpdtRaCaseRequest>(req => createRequest = req);
                var retentionAlertCase = await retentionAlertRepository.ChangeRetentionAlertCasePriorityAsync("1", retentionAlertCaseEntity);

                Assert.IsNotNull(retentionAlertCase);
                Assert.AreEqual("1", retentionAlertCase.CaseId);
                Assert.AreEqual(false, retentionAlertCase.HasError);
                Assert.AreEqual(0, retentionAlertCase.ErrorMessages.Count);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task CloseRetentionAlertCase_ArgumentCheck_0()
            {
                var retentionAlertCaseEntity = new RetentionAlertWorkCaseClose(null, null, null, null);
                UpdtRaCaseResponse response = new UpdtRaCaseResponse();
                mockManager.Setup(mgr => mgr.ExecuteAsync<UpdtRaCaseRequest, UpdtRaCaseResponse>(It.IsAny<UpdtRaCaseRequest>())).Returns(Task.FromResult(response)).Callback<UpdtRaCaseRequest>(req => createRequest = req);
                var retentionAlertCase = await retentionAlertRepository.CloseRetentionAlertCaseAsync(null, retentionAlertCaseEntity);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task CloseRetentionAlertCase_ArgumentCheck_1()
            {
                var retentionAlertCaseEntity = new RetentionAlertWorkCaseClose("1", null, null, null);
                UpdtRaCaseResponse response = new UpdtRaCaseResponse();
                mockManager.Setup(mgr => mgr.ExecuteAsync<UpdtRaCaseRequest, UpdtRaCaseResponse>(It.IsAny<UpdtRaCaseRequest>())).Returns(Task.FromResult(response)).Callback<UpdtRaCaseRequest>(req => createRequest = req);
                var retentionAlertCase = await retentionAlertRepository.CloseRetentionAlertCaseAsync(null, retentionAlertCaseEntity);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task CloseRetentionAlertCase_ArgumentCheck_2()
            {
                var retentionAlertCaseEntity = new RetentionAlertWorkCaseClose("1", "2", null, null);
                UpdtRaCaseResponse response = new UpdtRaCaseResponse();
                mockManager.Setup(mgr => mgr.ExecuteAsync<UpdtRaCaseRequest, UpdtRaCaseResponse>(It.IsAny<UpdtRaCaseRequest>())).Returns(Task.FromResult(response)).Callback<UpdtRaCaseRequest>(req => createRequest = req);
                var retentionAlertCase = await retentionAlertRepository.CloseRetentionAlertCaseAsync(null, retentionAlertCaseEntity);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task CloseRetentionAlertCase_ArgumentCheck_3()
            {
                var retentionAlertCaseEntity = new RetentionAlertWorkCaseClose("1", "2", "3", null);
                UpdtRaCaseResponse response = new UpdtRaCaseResponse();
                mockManager.Setup(mgr => mgr.ExecuteAsync<UpdtRaCaseRequest, UpdtRaCaseResponse>(It.IsAny<UpdtRaCaseRequest>())).Returns(Task.FromResult(response)).Callback<UpdtRaCaseRequest>(req => createRequest = req);
                var retentionAlertCase = await retentionAlertRepository.CloseRetentionAlertCaseAsync(null, retentionAlertCaseEntity);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task CloseRetentionAlertCase_ArgumentCheck_4()
            {
                var retentionAlertCaseEntity = new RetentionAlertWorkCaseClose("1", "2", "3", new List<string>() { "4" });
                UpdtRaCaseResponse response = new UpdtRaCaseResponse();
                mockManager.Setup(mgr => mgr.ExecuteAsync<UpdtRaCaseRequest, UpdtRaCaseResponse>(It.IsAny<UpdtRaCaseRequest>())).Returns(Task.FromResult(response)).Callback<UpdtRaCaseRequest>(req => createRequest = req);
                var retentionAlertCase = await retentionAlertRepository.CloseRetentionAlertCaseAsync(null, retentionAlertCaseEntity);

                Assert.IsNotNull(retentionAlertCase);
                Assert.AreEqual("1", retentionAlertCase.CaseId);
                Assert.AreEqual(false, retentionAlertCase.HasError);
                Assert.AreEqual(0, retentionAlertCase.ErrorMessages.Count);
            }
            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task CloseRetentionAlertCase_ArgumentCheck_5()
            {
                var retentionAlertCaseEntity = new RetentionAlertWorkCaseClose(null, null, null, null);
                UpdtRaCaseResponse response = new UpdtRaCaseResponse();
                mockManager.Setup(mgr => mgr.ExecuteAsync<UpdtRaCaseRequest, UpdtRaCaseResponse>(It.IsAny<UpdtRaCaseRequest>())).Returns(Task.FromResult(response)).Callback<UpdtRaCaseRequest>(req => createRequest = req);
                var retentionAlertCase = await retentionAlertRepository.CloseRetentionAlertCaseAsync("1", retentionAlertCaseEntity);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task CloseRetentionAlertCase_ArgumentCheck_6()
            {
                var retentionAlertCaseEntity = new RetentionAlertWorkCaseClose("1", null, null, null);
                UpdtRaCaseResponse response = new UpdtRaCaseResponse();
                mockManager.Setup(mgr => mgr.ExecuteAsync<UpdtRaCaseRequest, UpdtRaCaseResponse>(It.IsAny<UpdtRaCaseRequest>())).Returns(Task.FromResult(response)).Callback<UpdtRaCaseRequest>(req => createRequest = req);
                var retentionAlertCase = await retentionAlertRepository.CloseRetentionAlertCaseAsync("1", retentionAlertCaseEntity);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task CloseRetentionAlertCase_ArgumentCheck_7()
            {
                var retentionAlertCaseEntity = new RetentionAlertWorkCaseClose("1", "2", null, null);
                UpdtRaCaseResponse response = new UpdtRaCaseResponse();
                mockManager.Setup(mgr => mgr.ExecuteAsync<UpdtRaCaseRequest, UpdtRaCaseResponse>(It.IsAny<UpdtRaCaseRequest>())).Returns(Task.FromResult(response)).Callback<UpdtRaCaseRequest>(req => createRequest = req);
                var retentionAlertCase = await retentionAlertRepository.CloseRetentionAlertCaseAsync("1", retentionAlertCaseEntity);
            }

            [TestMethod]
            public async Task CloseRetentionAlertCase_ArgumentCheck_8()
            {
                var retentionAlertCaseEntity = new RetentionAlertWorkCaseClose("1", "2", "3", null);
                UpdtRaCaseResponse response = new UpdtRaCaseResponse();
                mockManager.Setup(mgr => mgr.ExecuteAsync<UpdtRaCaseRequest, UpdtRaCaseResponse>(It.IsAny<UpdtRaCaseRequest>())).Returns(Task.FromResult(response)).Callback<UpdtRaCaseRequest>(req => createRequest = req);
                var retentionAlertCase = await retentionAlertRepository.CloseRetentionAlertCaseAsync("1", retentionAlertCaseEntity);

                Assert.IsNotNull(retentionAlertCase);
                Assert.AreEqual("1", retentionAlertCase.CaseId);
                Assert.AreEqual(false, retentionAlertCase.HasError);
                Assert.AreEqual(0, retentionAlertCase.ErrorMessages.Count);
            }

            [TestMethod]
            public async Task CloseRetentionAlertCase_ArgumentCheck_9()
            {
                var retentionAlertCaseEntity = new RetentionAlertWorkCaseClose("1", "2", "3", new List<string>() { "notes" });
                UpdtRaCaseResponse response = new UpdtRaCaseResponse();
                mockManager.Setup(mgr => mgr.ExecuteAsync<UpdtRaCaseRequest, UpdtRaCaseResponse>(It.IsAny<UpdtRaCaseRequest>())).Returns(Task.FromResult(response)).Callback<UpdtRaCaseRequest>(req => createRequest = req);
                var retentionAlertCase = await retentionAlertRepository.CloseRetentionAlertCaseAsync("1", retentionAlertCaseEntity);

                Assert.IsNotNull(retentionAlertCase);
                Assert.AreEqual("1", retentionAlertCase.CaseId);
                Assert.AreEqual(false, retentionAlertCase.HasError);
                Assert.AreEqual(0, retentionAlertCase.ErrorMessages.Count);
            }

            [TestMethod]
            public async Task SendRetentionAlertWorkCaseMail_ArgumentCheck_1()
            {
                var retentionAlertCaseEntity = new RetentionAlertWorkCaseSendMail("1", "subject", "body", new List<string>() { "name" }, new List<string>() { "email" }, new List<string>() { "TO" });
                UpdtRaCaseResponse response = new UpdtRaCaseResponse();
                mockManager.Setup(mgr => mgr.ExecuteAsync<UpdtRaCaseRequest, UpdtRaCaseResponse>(It.IsAny<UpdtRaCaseRequest>())).Returns(Task.FromResult(response)).Callback<UpdtRaCaseRequest>(req => createRequest = req);
                var retentionAlertCase = await retentionAlertRepository.SendRetentionAlertWorkCaseMailAsync("1", retentionAlertCaseEntity);

                Assert.IsNotNull(retentionAlertCase);
                Assert.AreEqual("1", retentionAlertCase.CaseId);
                Assert.AreEqual(false, retentionAlertCase.HasError);
                Assert.AreEqual(0, retentionAlertCase.ErrorMessages.Count);
            }

            [TestMethod]
            public async Task SendRetentionAlertWorkCaseMail_ArgumentCheck_2()
            {
                var retentionAlertSendMailEntity = new RetentionAlertWorkCaseSendMail(null, null, null, null, null, null);
                UpdtRaCaseResponse response = new UpdtRaCaseResponse();
                mockManager.Setup(mgr => mgr.ExecuteAsync<UpdtRaCaseRequest, UpdtRaCaseResponse>(It.IsAny<UpdtRaCaseRequest>())).Returns(Task.FromResult(response)).Callback<UpdtRaCaseRequest>(req => createRequest = req);
                var retentionAlertCase = await retentionAlertRepository.SendRetentionAlertWorkCaseMailAsync("1", retentionAlertSendMailEntity);

                Assert.IsNotNull(retentionAlertCase);
                Assert.AreEqual("1", retentionAlertCase.CaseId);
                Assert.AreEqual(false, retentionAlertCase.HasError);
                Assert.AreEqual(0, retentionAlertCase.ErrorMessages.Count);
            }
        }

        [TestClass]
        public class RetentionAlertRepository_GetRetentionAlertClosedCasesByReasonAsync
        {
            Mock<IColleagueTransactionFactory> transFactoryMock;
            Mock<IColleagueDataReader> dataAccessorMock;
            Mock<ICacheProvider> cacheProviderMock;
            Mock<ILogger> loggerMock;
            Mock<ApiSettings> settings;
            Mock<IColleagueTransactionInvoker> mockManager;
            private IRetentionAlertRepository retentionAlertRepository;
            GetRaCaseDetailsRequest getRequest;

            public static char _SM = Convert.ToChar(Dmi.Runtime.DynamicArray.SM);

            [TestInitialize]
            public void Initialize()
            {
                settings = new Mock<ApiSettings>();
                loggerMock = new Mock<ILogger>();
                cacheProviderMock = new Mock<ICacheProvider>();
                transFactoryMock = new Mock<IColleagueTransactionFactory>();
                dataAccessorMock = new Mock<IColleagueDataReader>();
                mockManager = new Mock<IColleagueTransactionInvoker>();
                cacheProviderMock.Setup<Task<Tuple<object, SemaphoreSlim>>>(x =>
                x.GetAndLockSemaphoreAsync(It.IsAny<string>(), null))
                .Returns(Task.FromResult(new Tuple<object, SemaphoreSlim>(
                    null,
                    new SemaphoreSlim(1, 1)
                    )));
                // Set up data accessor for the transaction factory
                transFactoryMock.Setup(transFac => transFac.GetDataReader()).Returns(dataAccessorMock.Object);
                // Set up successful response to a transaction request, capturing the completed request for verification
                transFactoryMock.Setup(transFac => transFac.GetTransactionInvoker()).Returns(mockManager.Object);
                retentionAlertRepository = new RetentionAlertRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object, settings.Object);
            }

            [TestCleanup]
            public void Cleanup()
            {
                transFactoryMock = null;
                dataAccessorMock = null;
                cacheProviderMock = null;
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task GetRetentionAlertClosedCasesByReasonAsync_NullCaseCategoryId()
            {
                var caseDetails = await retentionAlertRepository.GetRetentionAlertClosedCasesByReasonAsync(null);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task GetRetentionAlertClosedCasesByReasonAsync_EmptyCaseCategoryId()
            {
                var caseDetails = await retentionAlertRepository.GetRetentionAlertClosedCasesByReasonAsync("");
            }

            [TestMethod]
            public async Task GetRetentionAlertClosedCasesByReasonAsync_Success()
            {
                var request = new GetRaClosedCasesRequest();

                var response = new GetRaClosedCasesResponse()
                {
                    RAClosureReasons = new List<RAClosureReasons>()
                    {
                        new RAClosureReasons()
                        {
                            AlClosureReasons = "1",
                            AlCases = string.Join(_SM.ToString(), new string[] {"10", "20", "30" } ),
                            AlLastActionDates = string.Join(_SM.ToString(), new string[] {"12345", "12346", "12347"})
                        },
                        new RAClosureReasons()
                        {
                            AlClosureReasons = "2",
                            AlCases = string.Join(_SM.ToString(), new string [] {"11", "22", "33"} ),
                            AlLastActionDates = string.Join(_SM.ToString(), new string[] {"12345", "12346", "12347"})
                        }
                    }
                };

                mockManager.Setup(mgr => mgr.ExecuteAsync<GetRaClosedCasesRequest, GetRaClosedCasesResponse>(It.IsAny<GetRaClosedCasesRequest>())).Returns(Task.FromResult(response)).Callback<GetRaClosedCasesRequest>(req => request = req);
                var closedCasesByReason = await retentionAlertRepository.GetRetentionAlertClosedCasesByReasonAsync("1");

                // Assert
                Assert.IsNotNull(closedCasesByReason);
                Assert.IsTrue(closedCasesByReason is List<RetentionAlertClosedCasesByReason>);
                Assert.AreEqual(response.RAClosureReasons.Count, closedCasesByReason.Count);

                for (var i = 0; i < response.RAClosureReasons.Count; i++)
                {
                    Assert.AreEqual(response.RAClosureReasons[i].AlClosureReasons, closedCasesByReason[i].ClosureReasonId);
                }
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task SetRetentionAlertEmailPreferenceAsync_NullArguments_1()
            {
                var response = await retentionAlertRepository.SetRetentionAlertEmailPreferenceAsync(null, null);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task SetRetentionAlertEmailPreferenceAsync_NullArguments_2()
            {
                var response = await retentionAlertRepository.SetRetentionAlertEmailPreferenceAsync("", null);
            }
            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task SetRetentionAlertEmailPreferenceAsync_NullArguments_3()
            {
                var response = await retentionAlertRepository.SetRetentionAlertEmailPreferenceAsync("1", null);
            }

            [TestMethod]
            public async Task SetRetentionAlertEmailPreferenceAsync_Success()
            {
                RetentionAlertSendEmailPreference prefereneEntity = new RetentionAlertSendEmailPreference()
                {
                    HasSendEmailFlag = true,
                    Message = "Your current setting is to receive e-mail reminders when cases are assigned to you."
                };

                ManageRaEmailPreferenceRequest request = new ManageRaEmailPreferenceRequest();
                ManageRaEmailPreferenceResponse response = new ManageRaEmailPreferenceResponse()
                {
                    ASendPref = true,
                    AMessage = "Your current setting is to receive e-mail reminders when cases are assigned to you."
                };

                mockManager.Setup(m => m.ExecuteAsync<ManageRaEmailPreferenceRequest, ManageRaEmailPreferenceResponse>(It.IsAny<ManageRaEmailPreferenceRequest>())).Returns(Task.FromResult(response)).Callback<ManageRaEmailPreferenceRequest>(req => request = req);
                var sendEmailPreference = await retentionAlertRepository.SetRetentionAlertEmailPreferenceAsync("1", prefereneEntity);

                Assert.AreEqual(prefereneEntity.HasSendEmailFlag, sendEmailPreference.HasSendEmailFlag);
                Assert.AreEqual(prefereneEntity.Message, sendEmailPreference.Message);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task GetRetentionAlertEmailPreferenceAsync_NullArguments_1()
            {
                var response = await retentionAlertRepository.GetRetentionAlertEmailPreferenceAsync(null);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task GetRetentionAlertEmailPreferenceAsync_NullArguments_2()
            {
                var response = await retentionAlertRepository.GetRetentionAlertEmailPreferenceAsync("");
            }

            [TestMethod]
            public async Task GetRetentionAlertEmailPreferenceAsync_Success()
            {
                RetentionAlertSendEmailPreference preferenceEntity = new RetentionAlertSendEmailPreference()
                {
                    HasSendEmailFlag = true,
                    Message = "Your current setting is to receive e-mail reminders when cases are assigned to you."
                };

                ManageRaEmailPreferenceRequest request = new ManageRaEmailPreferenceRequest();
                ManageRaEmailPreferenceResponse response = new ManageRaEmailPreferenceResponse()
                {
                    ASendPref = true,
                    AMessage = "Your current setting is to receive e-mail reminders when cases are assigned to you."
                };

                mockManager.Setup(m => m.ExecuteAsync<ManageRaEmailPreferenceRequest, ManageRaEmailPreferenceResponse>(It.IsAny<ManageRaEmailPreferenceRequest>())).Returns(Task.FromResult(response)).Callback<ManageRaEmailPreferenceRequest>(req => request = req);
                var sendEmailPreference = await retentionAlertRepository.GetRetentionAlertEmailPreferenceAsync("1");

                Assert.AreEqual(preferenceEntity.HasSendEmailFlag, sendEmailPreference.HasSendEmailFlag);
                Assert.AreEqual(preferenceEntity.Message, sendEmailPreference.Message);
            }

        }

        [TestClass]
        public class RetentionAlertRepository_SearchByNameForExactMatchAsync
        {

            protected Mock<IColleagueTransactionFactory> transFactoryMock;
            protected Mock<ObjectCache> localCacheMock;
            protected Mock<ICacheProvider> cacheProviderMock;
            protected Mock<IColleagueDataReader> dataReaderMock;
            protected Mock<IColleagueDataReader> anonymousDataReaderMock;
            protected Mock<ILogger> loggerMock;
            protected Mock<IColleagueTransactionInvoker> transManagerMock;
            ApiSettings apiSettingsMock;
            protected Mock<StudentRepository> studentRepoMock;

            RetentionAlertRepository retentionAlertRepository;

            string knownStudentId1 = "111";
            string knownStudentId2 = "222";

            [TestInitialize]
            public void Initialize()
            {
                // Initialize person setup and Mock framework
                // transaction factory mock
                transFactoryMock = new Mock<IColleagueTransactionFactory>();
                // Cache Mock
                localCacheMock = new Mock<ObjectCache>();
                // Cache Provider Mock
                cacheProviderMock = new Mock<ICacheProvider>();
                // Set up data accessor for mocking 
                dataReaderMock = new Mock<IColleagueDataReader>();
                // Logger mock
                loggerMock = new Mock<ILogger>();
                // Set up transaction manager for mocking 
                transManagerMock = new Mock<IColleagueTransactionInvoker>();
                apiSettingsMock = new ApiSettings("null");
                transFactoryMock.Setup(transFac => transFac.GetDataReader()).Returns(dataReaderMock.Object);
                // Set up transManagerMock as the object for the transaction manager
                transFactoryMock.Setup(transFac => transFac.GetTransactionInvoker()).Returns(transManagerMock.Object);


                cacheProviderMock.Setup<Task<Tuple<object, SemaphoreSlim>>>(x =>
                    x.GetAndLockSemaphoreAsync(It.IsAny<string>(), null))
                    .ReturnsAsync(new Tuple<object, SemaphoreSlim>(null, new SemaphoreSlim(1, 1)));

                // Build the test repository
                retentionAlertRepository = BuildMockAdviseeRepository();//retentionAlertRepository
            }

            private RetentionAlertRepository BuildMockAdviseeRepository()
            {

                RetentionAlertRepository repository = new RetentionAlertRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object, apiSettingsMock);

                return repository;
            }

            private void MockStudentData(string studentId)
            {
                dataReaderMock.Setup(a => a.BulkReadRecordAsync<Base.DataContracts.Person>(It.IsAny<string[]>(), true)).Returns((string[] s, bool b) => Task.FromResult(BuildPeople(s)));
                dataReaderMock.Setup(a => a.BulkReadRecordAsync<Student.DataContracts.Students>(It.IsAny<string[]>(), true)).Returns((string[] s, bool b) => Task.FromResult(BuildStudents(s)));
                dataReaderMock.Setup(a => a.BulkReadRecordAsync<Base.DataContracts.PersonSt>(It.IsAny<string[]>(), true)).Returns((string[] s, bool b) => Task.FromResult(BuildPersonStRecords(s)));
                dataReaderMock.Setup(d => d.BulkReadRecordAsync<Student.DataContracts.StudentAdvisement>(It.IsAny<string[]>(), true)).ReturnsAsync((Collection<Student.DataContracts.StudentAdvisement>)null);
                dataReaderMock.Setup(d => d.BulkReadRecordAsync<Base.DataContracts.ForeignPerson>(It.IsAny<string[]>(), true)).ReturnsAsync((Collection<Base.DataContracts.ForeignPerson>)null);
                dataReaderMock.Setup(d => d.BulkReadRecordAsync<Student.DataContracts.Applicants>(It.IsAny<string[]>(), true)).ReturnsAsync((Collection<Student.DataContracts.Applicants>)null);

                // Mock the read of the Preferred Name Address Hierarchy for the Preferred Name
                dataReaderMock.Setup<Ellucian.Colleague.Data.Base.DataContracts.NameAddrHierarchy>(a =>
                    a.ReadRecord<Ellucian.Colleague.Data.Base.DataContracts.NameAddrHierarchy>("NAME.ADDR.HIERARCHY", "PREFERRED", true))
                    .Returns(new Ellucian.Colleague.Data.Base.DataContracts.NameAddrHierarchy()
                    {
                        Recordkey = "PREFERRED",
                        NahNameHierarchy = new List<string>() { "MA", "XYZ", "PF" }
                    });

                //// Mock the call for getting the preferred address
                transManagerMock.Setup<TxGetHierarchyAddressResponse>(
                    manager => manager.Execute<TxGetHierarchyAddressRequest, TxGetHierarchyAddressResponse>(
                        It.IsAny<TxGetHierarchyAddressRequest>())
                    ).Returns<TxGetHierarchyAddressRequest>(request =>
                    {
                        return new TxGetHierarchyAddressResponse() { OutAddressId = studentId, OutAddressLabel = new List<string>() { "AdressLabel" } };
                    });

            }

            private void MockStudentData(string studentId1, string studentId2)
            {
                // Mock individual reads for each student
                MockStudentData(studentId1);
                MockStudentData(studentId2);
                // Mock bulk reads that must return a collection
                dataReaderMock.Setup(a => a.BulkReadRecordAsync<Base.DataContracts.Person>(It.IsAny<string[]>(), true))
                    .ReturnsAsync(new Collection<Base.DataContracts.Person>()
                    {
                        new Base.DataContracts.Person() { Recordkey = studentId1, LastName = "last" + studentId1 },
                        new Base.DataContracts.Person() { Recordkey = studentId2, LastName = "last" + studentId2 }
                    });
                dataReaderMock.Setup(a => a.BulkReadRecordAsync<Student.DataContracts.Students>(It.IsAny<string[]>(), true))
                    .ReturnsAsync(new Collection<Student.DataContracts.Students>()
                    {
                        new Student.DataContracts.Students() { Recordkey = studentId1, StuAcadPrograms = null },
                        new Student.DataContracts.Students() { Recordkey = studentId2, StuAcadPrograms = null }
                    });
            }

            [TestCleanup]
            public void TestCleanup()
            {
                retentionAlertRepository = null;
            }

            [TestMethod]
            public async Task SearchByNameForExactMatchAsync_ReturnsEmptyListIfCalledWithEmptyNames()
            {
                var result = await retentionAlertRepository.SearchStudentsByNameForExactMatchAsync(string.Empty, string.Empty, string.Empty);
                Assert.AreEqual(0, result.Count());
            }

            [TestMethod]
            public async Task SearchByNameForExactMatchAsync_ReturnsEmptyListIfCalledWithNullNames()
            {
                var result = await retentionAlertRepository.SearchStudentsByNameForExactMatchAsync(null);
                Assert.AreEqual(0, result.Count());
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task SearchByNameForExactMatchAsync_ThrowArgumentNullExceptionIdLastNameIsNull()
            {
                var result = await retentionAlertRepository.SearchStudentsByNameForExactMatchAsync(null, "firstname", "middlename");
            }

            [TestMethod]
            public async Task SearchByNameForExactMatchAsync_ReturnsEmptyListIfPersonSearchByNameReturnsEmptyList()
            {
                var lookupStringResponse = new GetPersonSearchKeyListResponse() { ErrorMessage = "", KeyList = new List<string>() };
                transManagerMock.Setup(manager => manager
                        .ExecuteAsync<GetPersonSearchKeyListRequest, GetPersonSearchKeyListResponse>(It.IsAny<GetPersonSearchKeyListRequest>()))
                        .ReturnsAsync(lookupStringResponse);
                var lastName = "Gerbil";
                var result = await retentionAlertRepository.SearchStudentsByNameForExactMatchAsync(lastName);
                Assert.AreEqual(0, result.Count());
            }

            [TestMethod]
            public async Task SearchByNameForExactMatchAsync_ReturnsEmptyListIfPersonSearchByNameReturnsKeyListNull()
            {
                var lookupStringResponse = new GetPersonSearchKeyListResponse() { ErrorMessage = "", KeyList = null };
                transManagerMock.Setup(manager => manager
                        .ExecuteAsync<GetPersonSearchKeyListRequest, GetPersonSearchKeyListResponse>(It.IsAny<GetPersonSearchKeyListRequest>()))
                        .ReturnsAsync(lookupStringResponse);
                var lastName = "Gerbil";
                var result = await retentionAlertRepository.SearchStudentsByNameForExactMatchAsync(lastName);
                Assert.AreEqual(0, result.Count());
            }
            [TestMethod]
            public async Task SearchByNameForExactMatchAsync_ReturnsEmptyListIfPersonSearchByNameReturnsNull()
            {
                transManagerMock.Setup(manager => manager
                        .ExecuteAsync<GetPersonSearchKeyListRequest, GetPersonSearchKeyListResponse>(It.IsAny<GetPersonSearchKeyListRequest>()))
                        .ReturnsAsync(null);
                var lastName = "Gerbil";
                var result = await retentionAlertRepository.SearchStudentsByNameForExactMatchAsync(lastName);
                Assert.AreEqual(0, result.Count());
            }

            [TestMethod]
            public async Task SearchByNameForExactMatchAsync_ReturnsEmptyListIfStudentSelectReturnsNull()
            {
                var personIdsList = new string[] { "001", "002", "003" };
                string[] nullStudentIdsList = null;
                var lookupStringResponse = new GetPersonSearchKeyListResponse() { ErrorMessage = "", KeyList = personIdsList.ToList() };
                transManagerMock.Setup(manager => manager
                        .ExecuteAsync<GetPersonSearchKeyListRequest, GetPersonSearchKeyListResponse>(It.IsAny<GetPersonSearchKeyListRequest>()))
                        .ReturnsAsync(lookupStringResponse);
                dataReaderMock.Setup(acc => acc.Select("STUDENTS", It.IsAny<string[]>(), It.IsAny<string>())).Returns(nullStudentIdsList);
                var lastName = "Gerbil";
                var result = await retentionAlertRepository.SearchStudentsByNameForExactMatchAsync(lastName);
                Assert.AreEqual(0, result.Count());
            }

            [TestMethod]
            public async Task SearchByNameForExactMatchAsync_ReturnsEmptyListIfStudentSelectReturnsEmptyList()
            {
                var personIdsList = new string[] { "001", "002", "003" };
                var blankStudentIdsList = new string[] { };
                var lastName = "Gerbil";
                var lookupStringResponse = new GetPersonSearchKeyListResponse() { ErrorMessage = "", KeyList = personIdsList.ToList() };
                transManagerMock.Setup(manager => manager
                        .ExecuteAsync<GetPersonSearchKeyListRequest, GetPersonSearchKeyListResponse>(It.IsAny<GetPersonSearchKeyListRequest>()))
                        .ReturnsAsync(lookupStringResponse);
                dataReaderMock.Setup(acc => acc.Select("STUDENTS", It.IsAny<string[]>(), It.IsAny<string>())).Returns(blankStudentIdsList);
                var result = await retentionAlertRepository.SearchStudentsByNameForExactMatchAsync(lastName);
                Assert.AreEqual(0, result.Count());
            }
            
        }

    }
}
