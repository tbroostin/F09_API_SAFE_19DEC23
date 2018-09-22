// Copyright 2014-2016 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Data.Base.Tests.Repositories;
using Ellucian.Colleague.Data.Student.DataContracts;
using Ellucian.Colleague.Data.Student.Repositories;
using Ellucian.Colleague.Data.Student.Transactions;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Colleague.Domain.Student.Entities;
using Ellucian.Colleague.Domain.Student.Tests;
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

namespace Ellucian.Colleague.Data.Student.Tests.Repositories
{
    [TestClass]
    public class AdmissionApplicationsRepositoryTests : BaseRepositorySetup
    {
        [TestClass]
        public class GetAdmissionApplicationsTests
        {
            Mock<IColleagueTransactionFactory> transFactoryMock;
            Mock<ICacheProvider> cacheProviderMock;
            Mock<IColleagueDataReader> dataReaderMock;
            Mock<ILogger> loggerMock;
            ApiSettings apiSettingsMock;

            //Data contract objects returned as responses by the mocked datareaders
            private Applications applicationResponseData;
            private Collection<Applications> applicationResponseCollection;

            //TestRepositories
            private TestAdmissionApplicationsRepository expectedRepository;
            private AdmissionApplicationsRepository actualRepository;

            //Test data
            private AdmissionApplication expectedApplication;
            private AdmissionApplication actualApplication;
            private List<AdmissionApplication> expectedApplications;

            //used throughout
            private string applicationId;
            private string[] applicationIds;
            private string applicationGuid;
            int offset = 0;
            int limit = 200;


            [TestInitialize]
            public async void Initialize()
            {
                expectedRepository = new TestAdmissionApplicationsRepository();

                //setup the expected applications
                var pageOfApplications = await expectedRepository.GetAdmissionApplicationsAsync(offset, limit, false);
                expectedApplications = pageOfApplications.Item1.ToList();

                applicationId = expectedApplications.First().ApplicationRecordKey;
                applicationGuid = expectedApplications.First().Guid;
                expectedApplication = await expectedRepository.GetAdmissionApplicationByIdAsync(applicationGuid);

                //set the response data objects
                applicationIds = expectedApplications.Select(ap => ap.ApplicationRecordKey).ToArray();
                applicationResponseCollection = BuildResponseData(expectedApplications);
                applicationResponseData = applicationResponseCollection.FirstOrDefault(ap => ap.RecordGuid == applicationGuid);

                //build the repository
                actualRepository = BuildRepository();
            }

            [TestCleanup]
            public void Cleanup()
            {
                cacheProviderMock = null;
                dataReaderMock = null;
                loggerMock = null;
                transFactoryMock = null;

                applicationResponseData = null;
                expectedRepository = null;
                actualRepository = null;
                expectedApplications = null;
                expectedApplication = null;
                actualApplication = null;
                applicationId = null;
                applicationIds = null;
            }

            private AdmissionApplicationsRepository BuildRepository()
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
                apiSettingsMock = new ApiSettings("TEST");

                // Set up dataAccessorMock as the object for the DataAccessor
                transFactoryMock.Setup(transFac => transFac.GetDataReader()).Returns(dataReaderMock.Object);

                // Single Application
                dataReaderMock.Setup(acc => acc.SelectAsync(It.IsAny<GuidLookup[]>())).Returns<GuidLookup[]>(gla =>
                {
                    var result = new Dictionary<string, GuidLookupResult>();
                    foreach (var gl in gla)
                    {
                        var appl = applicationResponseCollection.FirstOrDefault(x => x.RecordGuid == gl.Guid);
                        result.Add(gl.Guid, appl == null ? null : new GuidLookupResult() { Entity = "APPLICATIONS", PrimaryKey = appl.Recordkey });
                    }
                    return Task.FromResult(result);
                });
                dataReaderMock.Setup(dr => dr.ReadRecordAsync<Applications>("APPLICATIONS", applicationId, true)).ReturnsAsync(applicationResponseData);
                dataReaderMock.Setup<Task<Collection<StudentPrograms>>>(dr => dr.BulkReadRecordAsync<StudentPrograms>(It.IsAny<string[]>(), true)).ReturnsAsync(new Collection<StudentPrograms>());
                
                // Multiple Applications
                dataReaderMock.Setup(dr => dr.SelectAsync("APPLICATIONS", It.IsAny<string>())).ReturnsAsync(applicationIds);
                dataReaderMock.Setup(dr => dr.BulkReadRecordAsync<Applications>("APPLICATIONS", It.IsAny<string[]>(), true)).ReturnsAsync(applicationResponseCollection);
                return new AdmissionApplicationsRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object);
            }

            private Collection<Applications> BuildResponseData(List<AdmissionApplication> applData)
            {
                var collection = new Collection<Applications>();
                foreach (var appl in applData)
                {
                    var applicationRecord = new Applications()
                    {
                        Recordkey = appl.ApplicationRecordKey,
                        RecordGuid = appl.Guid,
                        ApplAcadProgram = appl.ApplicationAcadProgram,
                        ApplAdmissionsRep = appl.ApplicationAdmissionsRep,
                        ApplAdmitStatus = appl.ApplicationAdmitStatus,
                        ApplApplicant = appl.ApplicantPersonId,
                        ApplAttendedInstead = appl.ApplicationAttendedInstead,
                        ApplComments = appl.ApplicationComments,
                        ApplLocations = appl.ApplicationLocations,
                        ApplNo = appl.ApplicationNo,
                        ApplStartTerm = appl.ApplicationStartTerm,
                        ApplStudentLoadIntent = appl.ApplicationStudentLoadIntent,
                        ApplSource = appl.ApplicationSource,
                        ApplWithdrawReason = appl.ApplicationWithdrawReason,
                        ApplResidencyStatus = appl.ApplicationResidencyStatus,

                        ApplStatus = new List<string>(),
                        ApplStatusDate = new List<DateTime?>(),
                        ApplStatusTime = new List<DateTime?>(),
                        ApplStatusesEntityAssociation = new List<ApplicationsApplStatuses>(),
                        ApplDecisionBy = new List<string>(),
                        ApplLocationDates = new List<DateTime?>(),
                        ApplLocationChangeReasons = new List<string>(),
                        ApplLocationInfoEntityAssociation = new List<ApplicationsApplLocationInfo>()
                    };
                    foreach (var status in appl.AdmissionApplicationStatuses)
                    {
                        applicationRecord.ApplStatus.Add(status.ApplicationStatus);
                        applicationRecord.ApplStatusDate.Add(status.ApplicationStatusDate);
                        applicationRecord.ApplStatusTime.Add(status.ApplicationStatusTime);
                        applicationRecord.ApplDecisionBy.Add(string.Empty);
                    }
                    applicationRecord.buildAssociations();

                    collection.Add(applicationRecord);
                }
                return collection;
            }         

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task NullApplicationRecord_ExceptionTest()
            {
                //set the response data object to null and the dataReaderMock
                applicationResponseData = null;
                dataReaderMock.Setup(dr => dr.ReadRecordAsync<Applications>("APPLICATIONS", applicationId, true)).ReturnsAsync(applicationResponseData);

                actualApplication = await actualRepository.GetAdmissionApplicationByIdAsync(applicationGuid);
            }

            [TestMethod]
            public async Task NullApplicationRecord_LogsErrorTest()
            {
                var exceptionCaught = false;
                try
                {
                    //set the response data object to null and the dataReaderMock
                    applicationResponseData = null;
                    dataReaderMock.Setup(dr => dr.ReadRecordAsync<Applications>("APPLICATIONS", applicationId, true)).ReturnsAsync(applicationResponseData);

                    actualApplication = await actualRepository.GetAdmissionApplicationByIdAsync(applicationGuid);
                }
                catch
                {
                    exceptionCaught = true;
                }

                Assert.IsTrue(exceptionCaught);
                // loggerMock.Verify(l => l.Error(It.IsAny<string>()));
            }
        }
    }

    [TestClass]
    public class AdmissionApplicationsRepositoryTests_V11
    {
        [TestClass]
        public class AdmissionApplicationsRepositoryTests_POST_AND_POST: BaseRepositorySetup
        {
            #region DECLARATIONS

            private AdmissionApplicationsRepository admissionApplicationsRepository;

            private UpdateAdmApplicationResponse response;

            private Dictionary<string, GuidLookupResult> dicResult;

            private AdmissionApplication entity;

            private Applications application;

            private Collection<ApplicationStatuses> applicationStatuses;

            private Collection<StudentPrograms> studentPrograms;

            private string guid = "1a49eed8-5fe7-4120-b1cf-f23266b9e874";

            #endregion

            #region TEST SETUP

            [TestInitialize]
            public void Initialize()
            {
                MockInitialize();

                InitializeTestData();

                InitializeTestMock();

                admissionApplicationsRepository = new AdmissionApplicationsRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object);
            }

            [TestCleanup]
            public void Cleanup()
            {
                MockCleanup();
            }

            private void InitializeTestData()
            {
                entity = new AdmissionApplication("1a49eed8-5fe7-4120-b1cf-f23266b9e874", "1")
                {
                    ApplicationStudentLoadIntent = "1",
                    ApplicationStartTerm = "1",
                    ApplicationAdmitStatus = "active",
                    ApplicationOwnerId = "1",
                    ApplicationSchool = "1",
                    ApplicationIntgType = "1",
                    ApplicationAcadProgram = "1",
                    AdmittedOn = DateTime.Now,
                    ApplicantPersonId = "1",
                    AppliedOn = DateTime.Now.AddDays(-20),
                    ApplicationComments = "comments",
                    MatriculatedOn = DateTime.Now.AddDays(-100),
                    ApplicationNo = "1",
                    ApplicationResidencyStatus = "1",
                    ApplicationLocations = new List<string>() { "1" },
                    ApplicationSource = "1",
                    ApplicationAttendedInstead = "1",
                    ApplicationWithdrawReason = "1",
                    WithdrawnOn = DateTime.Now.AddDays(-10)
                };

                dicResult = new Dictionary<string, GuidLookupResult>()
                {
                    {
                        "1a49eed8-5fe7-4120-b1cf-f23266b9e874", new GuidLookupResult(){Entity = "APPLICATIONS", PrimaryKey = "1" }
                    }
                };

                applicationStatuses = new Collection<ApplicationStatuses>()
                {
                    new ApplicationStatuses() {RecordGuid = "1a49eed8-5fe7-4120-b1cf-f23266b9e874", Recordkey = "2"}
                };

                application = new Applications()
                {
                    RecordGuid = "1a49eed8-5fe7-4120-b1cf-f23266b9e874",
                    Recordkey = "1",
                    ApplApplicant = "1",
                    ApplAcadProgram = "1",
                    ApplStatusesEntityAssociation = new List<ApplicationsApplStatuses>()
                    {
                        new ApplicationsApplStatuses(){ ApplStatusAssocMember = "1" }
                    }
                };

                studentPrograms = new Collection<StudentPrograms>()
                {
                    new StudentPrograms(){RecordGuid = "1a49eed8-5fe7-4120-b1cf-f23266b9e874", Recordkey = "1*1"}
                };

                response = new UpdateAdmApplicationResponse() { Guid = "1a49eed8-5fe7-4120-b1cf-f23266b9e874" };
            }

            private void InitializeTestMock()
            {
                transManagerMock.Setup(t => t.ExecuteAsync<UpdateAdmApplicationRequest, UpdateAdmApplicationResponse>(It.IsAny<UpdateAdmApplicationRequest>())).ReturnsAsync(response);
                dataReaderMock.Setup(d => d.SelectAsync(It.IsAny<GuidLookup[]>())).ReturnsAsync(dicResult);
                dataReaderMock.Setup(d => d.SelectAsync(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(new string[] { "1a49eed8-5fe7-4120-b1cf-f23266b9e874" });
                dataReaderMock.Setup(d => d.BulkReadRecordAsync<ApplicationStatuses>(It.IsAny<string>(), true)).ReturnsAsync(applicationStatuses);
                dataReaderMock.Setup(d => d.BulkReadRecordAsync<StudentPrograms>(It.IsAny<string[]>(), true)).ReturnsAsync(studentPrograms);
                dataReaderMock.Setup(d => d.ReadRecordAsync<Applications>(It.IsAny<string>(), It.IsAny<string>(), true)).ReturnsAsync(application);
            }

            #endregion

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task AdmissionApplicationRepo_CreateAdmissionApplicationAsync_Entity_As_Null()
            {
                await admissionApplicationsRepository.CreateAdmissionApplicationAsync(null);
            }

            [TestMethod]
            [ExpectedException(typeof(RepositoryException))]
            public async Task AdmissionApplicationRepo_CreateAdmissionApplicationAsync_Response_With_UpdateAdmApplicationErrors()
            {
                response = new UpdateAdmApplicationResponse()
                {
                    UpdateAdmApplicationErrors = new List<UpdateAdmApplicationErrors>()
                    {
                        new UpdateAdmApplicationErrors() { ErrorCodes = "101", ErrorMessages = "ERROR MESSAGE" }
                    }
                };

                transManagerMock.Setup(mgr => mgr.ExecuteAsync<UpdateAdmApplicationRequest, UpdateAdmApplicationResponse>(It.IsAny<UpdateAdmApplicationRequest>())).ReturnsAsync(response);

                await admissionApplicationsRepository.CreateAdmissionApplicationAsync(entity);
            }

            [TestMethod]
            public async Task AdmissionApplicationRepository_CreateAdmissionApplicationAsync()
            {
                var result = await admissionApplicationsRepository.CreateAdmissionApplicationAsync(entity);

                Assert.IsNotNull(result);
                Assert.AreEqual(result.Guid, guid);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task AdmissionApplicationRepo_UpdateAdmissionApplicationAsync_Entity_As_Null()
            {
                await admissionApplicationsRepository.UpdateAdmissionApplicationAsync(null);
            }

            [TestMethod]
            [ExpectedException(typeof(RepositoryException))]
            public async Task AdmissionApplicationRepo_UpdateAdmissionApplicationAsync_Response_With_UpdateAdmApplicationErrors()
            {
                response = new UpdateAdmApplicationResponse()
                {
                    UpdateAdmApplicationErrors = new List<UpdateAdmApplicationErrors>()
                    {
                        new UpdateAdmApplicationErrors() { ErrorCodes = "101", ErrorMessages = "ERROR MESSAGE" }
                    }
                };

                transManagerMock.Setup(mgr => mgr.ExecuteAsync<UpdateAdmApplicationRequest, UpdateAdmApplicationResponse>(It.IsAny<UpdateAdmApplicationRequest>())).ReturnsAsync(response);

                await admissionApplicationsRepository.UpdateAdmissionApplicationAsync(entity);
            }


            [TestMethod]
            public async Task AdmissionApplicationRepository_UpdateAdmissionApplicationAsync()
            {
                entity = new AdmissionApplication("1a49eed8-5fe7-4120-b1cf-f23266b9e874", "1") { };

                var result = await admissionApplicationsRepository.UpdateAdmissionApplicationAsync(entity);

                Assert.IsNotNull(result);
                Assert.AreEqual(result.Guid, guid);
            }

            [TestMethod]
            public async Task AdmissionApplicationRepository_UpdateAdmissionApplicationAsync_Create_Record()
            {
                Dictionary<string, GuidLookupResult> firstResult = new Dictionary<string, GuidLookupResult>()
                {
                    { "KEY", new GuidLookupResult() { Entity = "APPLICATIONS", PrimaryKey = null } }
                };

                dataReaderMock.SetupSequence(d => d.SelectAsync(It.IsAny<GuidLookup[]>())).Returns(Task.FromResult(firstResult)).Returns(Task.FromResult(dicResult));

                var result = await admissionApplicationsRepository.UpdateAdmissionApplicationAsync(entity);

                Assert.IsNotNull(result);
                Assert.AreEqual(result.Guid, guid);
            }
        }
    }
}
