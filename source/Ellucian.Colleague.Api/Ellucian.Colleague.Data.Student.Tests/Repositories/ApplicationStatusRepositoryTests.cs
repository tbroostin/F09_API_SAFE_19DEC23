// Copyright 2014-2019 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Ellucian.Colleague.Data.Base.Tests.Repositories;
using Ellucian.Colleague.Data.Student.DataContracts;
using Ellucian.Colleague.Data.Student.Repositories;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Threading.Tasks;
using Ellucian.Colleague.Domain.Student.Entities;
using Ellucian.Data.Colleague;
using Ellucian.Web.Cache;
using slf4net;
using Ellucian.Web.Http.Configuration;
using System.Threading;
using Ellucian.Dmi.Runtime;
using Ellucian.Colleague.Data.Student.Transactions;
using Ellucian.Colleague.Domain.Exceptions;

namespace Ellucian.Colleague.Data.Student.Tests.Repositories
{
    [TestClass]
    public class ApplicationStatusRepositoryTests : BaseRepositorySetup
    {
        Collection<Applications> _applicationDataContracts;
        Collection<Applications> _admissionApplicationDataContracts;
        ApplicationStatus2 statusIn;
        public static char _VM = Convert.ToChar(DynamicArray.VM);
        ApplicationStatusRepository _applicationStatusRepository;
        Dictionary<string, RecordKeyLookupResult> recordLookupDict;
        UpdateAdmApplStatusesResponse response;
        Mock<IColleagueTransactionInvoker> _transManagerMock;

        //used throughout
        private string applicationGuid;
        private string[] applStatusesNoSpCodeIds;
        private string[] applIds;
        private string[] applIdxs;

        int offset = 0;
        int limit = 200;


        [TestInitialize]
        public void Initialize()
        {
            MockInitialize();

            InitializeData();

            ////build the repository
            _applicationStatusRepository = BuildRepository();
        }   

        [TestCleanup]
        public void Cleanup()
        {
            applicationGuid = null;
       }

        [TestMethod]
        public async Task ApplicationStatusRepository_GetApplicationStatusesAsync()
        {
            var actualResult = await _applicationStatusRepository.GetApplicationStatusesAsync(offset, limit, applicationGuid, It.IsAny<string[]>(), It.IsAny<DateTimeOffset>(), It.IsAny<Dictionary<string, string>>(), It.IsAny<bool>());
            Assert.IsNotNull(actualResult);
        }

        [TestMethod]
        public async Task ApplicationStatusRepository_GetApplicationStatusesAsync_No_ApplicationId()
        {
            var actualResult = await _applicationStatusRepository.GetApplicationStatusesAsync(offset, limit, "", It.IsAny<string[]>(), It.IsAny<DateTimeOffset>(), It.IsAny<Dictionary<string, string>>(), It.IsAny<bool>());
            Assert.IsNotNull(actualResult);
        }

        [TestMethod]
        public async Task ApplicationStatusRepository_GetApplicationStatusesAsync_AppllIds_Null()
        {
            applIds = new string[] { };
            dataReaderMock.Setup(repo => repo.SelectAsync("APPLICATIONS", It.IsAny<string>(), It.IsAny<string[]>(), "?", It.IsAny<bool>(), It.IsAny<int>())).ReturnsAsync(applIds);

            var actualResult = await _applicationStatusRepository.GetApplicationStatusesAsync(offset, limit, "", It.IsAny<string[]>(), It.IsAny<DateTimeOffset>(), It.IsAny<Dictionary<string, string>>(), It.IsAny<bool>());
            Assert.IsNotNull(actualResult);
        }

        [TestMethod]
        public async Task ApplicationStatusRepository_GetApplicationStatusByGuidAsync()
        {
            var actualResult = await _applicationStatusRepository.GetApplicationStatusByGuidAsync("1c0be7c8-d88a-4396-a47a-d9c75ff65367", It.IsAny<bool>());
            Assert.IsNotNull(actualResult);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task ApplicationStatusRepository_GetApplicationStatusByGuidAsync_ArgumentNullException()
        {
            var actualResult = await _applicationStatusRepository.GetApplicationStatusByGuidAsync("", It.IsAny<bool>());
        }

        [TestMethod]
        [ExpectedException(typeof(KeyNotFoundException))]
        public async Task ApplicationStatusRepository_GetApplicationStatusByGuidAsync_KeyNotFoundException()
        {
            var actualResult = await _applicationStatusRepository.GetApplicationStatusByGuidAsync("1234", It.IsAny<bool>());
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task ApplicationStatusRepository_UpdateAdmissionDecisionAsync_ArgumentNullException()
        {
            await _applicationStatusRepository.UpdateAdmissionDecisionAsync(null);
        }

        [TestMethod]
        [ExpectedException(typeof(RepositoryException))]
        public async Task ApplicationStatusRepository_UpdateAdmissionDecisionAsync_RepositoryException()
        {
            var response = new UpdateAdmApplStatusesResponse();
            response.ApplicationStatusErrors = new List<ApplicationStatusErrors>()
            {
                new ApplicationStatusErrors()
                {
                   ErrorCode = "ApplicationStatus.Error.Code",
                   ErrorMsg = "Error occured!!!"
                }
            };
            transFactoryMock.Setup(transFac => transFac.GetTransactionInvoker()).Returns(transManagerMock.Object);
            transManagerMock.Setup(repo => repo.ExecuteAsync<UpdateAdmApplStatusesRequest, UpdateAdmApplStatusesResponse>(It.IsAny<UpdateAdmApplStatusesRequest>())).ReturnsAsync(response);

            var result = await _applicationStatusRepository.UpdateAdmissionDecisionAsync(statusIn);
        }

        [TestMethod]
        public async Task ApplicationStatusRepository_UpdateAdmissionDecisionAsync_ErrorCode_NotNull_And_NoErrorMessage()
        {
            var result = await _applicationStatusRepository.UpdateAdmissionDecisionAsync(statusIn);
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public async Task ApplicationStatusRepository_UpdateAdmissionDecisionAsync()
        {
            var result = await _applicationStatusRepository.UpdateAdmissionDecisionAsync(statusIn);
            Assert.IsNotNull(result);
        }

        private ApplicationStatusRepository BuildRepository()
        {
            // transaction factory mock
            transFactoryMock = new Mock<IColleagueTransactionFactory>();
            // Cache Provider Mock
            cacheProviderMock = new Mock<ICacheProvider>();
            // Logger Mock
            loggerMock = new Mock<ILogger>();

            cacheProviderMock.Setup<Task<Tuple<object, SemaphoreSlim>>>(x =>
                x.GetAndLockSemaphoreAsync(It.IsAny<string>(), null))
                .ReturnsAsync(new Tuple<object, SemaphoreSlim>(null, new SemaphoreSlim(1, 1)));

            // Set up data accessor for mocking 
            dataReaderMock = new Mock<IColleagueDataReader>();
            apiSettings = new ApiSettings("TEST");

            // Set up dataAccessorMock as the object for the DataAccessor
            transFactoryMock.Setup(transFac => transFac.GetDataReader()).Returns(dataReaderMock.Object);

            dataReaderMock.Setup(acc => acc.SelectAsync(It.IsAny<GuidLookup[]>())).Returns<GuidLookup[]>(gla =>
            {
                var result = new Dictionary<string, GuidLookupResult>();
                foreach (var gl in gla)
                {
                    var application = _admissionApplicationDataContracts.FirstOrDefault(x => x.RecordGuid == gl.Guid);
                    result.Add(gl.Guid, application == null ? null : new GuidLookupResult() { Entity = "APPLICATIONS", PrimaryKey = application.Recordkey });
                }
                return Task.FromResult(result);
            });

            recordLookupDict = new Dictionary<string, RecordKeyLookupResult>();
           
            dataReaderMock.Setup(repo => repo.SelectAsync(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(applStatusesNoSpCodeIds);
            dataReaderMock.SetupSequence(repo => repo.SelectAsync("APPLICATIONS", It.IsAny<string>(), It.IsAny<string[]>(), "?", It.IsAny<bool>(), It.IsAny<int>()))
                .Returns(Task.FromResult(applIds))
                .Returns(Task.FromResult(applIdxs));

            var key = "WITHP*18110*77924";
            var key2 = "AD*17335*39945";
            recordLookupDict.Add("APPLICATIONS+" + "1" + "+" + key, new RecordKeyLookupResult() { Guid = "1c0be7c8-d88a-4396-a47a-d9c75ff65367", ModelName = "APPLICATIONS" });
            recordLookupDict.Add("APPLICATIONS+" + "1" + "+" + key2, new RecordKeyLookupResult() { Guid = "2c0be7c8-d88a-4396-a47a-d9c75ff65367", ModelName = "APPLICATIONS" });
            dataReaderMock.Setup(dr => dr.SelectAsync(It.IsAny<RecordKeyLookup[]>())).ReturnsAsync(recordLookupDict);

            dataReaderMock.Setup(repo => repo.BulkReadRecordAsync<Applications>(It.IsAny<string>(), It.IsAny<string[]>(), It.IsAny<bool>())).ReturnsAsync(_applicationDataContracts);
            dataReaderMock.Setup(repo => repo.ReadRecordAsync<Applications>("APPLICATIONS", It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(_applicationDataContracts[0]);

            transFactoryMock.Setup(transFac => transFac.GetTransactionInvoker()).Returns(transManagerMock.Object);
            transManagerMock.Setup(repo => repo.ExecuteAsync<UpdateAdmApplStatusesRequest, UpdateAdmApplStatusesResponse>(It.IsAny<UpdateAdmApplStatusesRequest>())).ReturnsAsync(response);

            return new ApplicationStatusRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object, apiSettings);
        }

        private void InitializeData()
        {
            applStatusesNoSpCodeIds = new[] { "1", "1", "2", "3" };
            applIds = new[] { "1", "1" };
            applIdxs = new string[] { "WITHP*18110*77924", "AD*17335*39945" };
            applicationGuid = "5c0be7c8-d88a-4396-a47a-d9c75ff65364";

            _applicationDataContracts = new Collection<Applications>()
            {
                new Applications()
                {
                    Recordkey = "1",
                    RecordGuid = "6c0be7c8-d88a-4396-a47a-d9c75ff65367",
                    ApplStatusesEntityAssociation = new List<ApplicationsApplStatuses>()
                    {
                        new ApplicationsApplStatuses()
                        {
                            ApplDecisionByAssocMember = "0012297",
                            ApplStatusAssocMember = "WITHP",
                            ApplStatusDateAssocMember = Convert.ToDateTime("2017-07-31 00:00:00.000"),
                            ApplStatusTimeAssocMember = Convert.ToDateTime("1900-01-01 21:38:44.000")
                        },
                        new ApplicationsApplStatuses()
                        {
                            ApplDecisionByAssocMember = "0012297",
                            ApplStatusAssocMember = "AD",
                            ApplStatusDateAssocMember = Convert.ToDateTime("2015-06-17 00:00:00.000"),
                            ApplStatusTimeAssocMember = Convert.ToDateTime("1900-01-01 11:05:45.000")
                        }
                    }
                }
            };
            statusIn = new ApplicationStatus2("1c0be7c8-d88a-4396-a47a-d9c75ff65367", "0012297", "AB", Convert.ToDateTime("2017-07-31 00:00:00.000"), Convert.ToDateTime("1900-01-01 21:38:44.000"));
            response = new UpdateAdmApplStatusesResponse() { AdmdecGuid = "1c0be7c8-d88a-4396-a47a-d9c75ff65367" };
            _admissionApplicationDataContracts = new Collection<Applications>()
            {
                new Applications()
                {
                    Recordkey = "1",
                    RecordGuid = "1c0be7c8-d88a-4396-a47a-d9c75ff65367",
                    ApplStatusesEntityAssociation = new List<ApplicationsApplStatuses>()
                    {
                        new ApplicationsApplStatuses()
                        {
                            ApplDecisionByAssocMember = "0012297",
                            ApplStatusAssocMember = "WITHP",
                            ApplStatusDateAssocMember = Convert.ToDateTime("2017-07-31 00:00:00.000"),
                            ApplStatusTimeAssocMember = Convert.ToDateTime("1900-01-01 21:38:44.000")
                        },
                        new ApplicationsApplStatuses()
                        {
                            ApplDecisionByAssocMember = "0012297",
                            ApplStatusAssocMember = "AD",
                            ApplStatusDateAssocMember = Convert.ToDateTime("2015-06-17 00:00:00.000"),
                            ApplStatusTimeAssocMember = Convert.ToDateTime("1900-01-01 11:05:45.000")
                        }
                    }
                }
            };
        }
    }
}
