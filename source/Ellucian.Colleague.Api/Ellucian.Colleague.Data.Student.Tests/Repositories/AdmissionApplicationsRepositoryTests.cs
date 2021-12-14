// Copyright 2014-2020 Ellucian Company L.P. and its affiliates.
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
using Ellucian.Colleague.Domain.Base.Transactions;

namespace Ellucian.Colleague.Data.Student.Tests.Repositories
{
    [TestClass]
    public class AdmissionApplicationsRepositoryTests : BaseRepositorySetup
    {
        [TestClass]
        public class GetAdmissionApplicationsTests
        {
            Mock<IColleagueTransactionFactory> transFactoryMock;
            Mock<IColleagueTransactionInvoker> transManagerMock;
            Mock<ICacheProvider> cacheProviderMock;
            Mock<IColleagueDataReader> dataReaderMock;
            Mock<ILogger> loggerMock;
            ApiSettings apiSettingsMock;

            //Data contract objects returned as responses by the mocked datareaders
            private Applications applicationResponseData;
            private Collection<Applications> applicationResponseCollection;

            //Data contract objects returned as responses for student programs
            private StudentPrograms studentProgramsResponseData;
            private Collection<StudentPrograms> studentProgramsResponseCollection;

            //Data contract objects returned as responses for applicants
            private Applicants applicantsResponseData;
            private Collection<Applicants> applicantsResponseCollection;

            //TestRepositories
            private TestAdmissionApplicationsRepository expectedRepository;
            private AdmissionApplicationsRepository actualRepository;

            //Test data
            private AdmissionApplication expectedApplication;
            private AdmissionApplication actualApplication;
            private List<AdmissionApplication> actualApplications;
            private List<AdmissionApplication> expectedApplications;

            //used throughout
            private string applicationId;
            private string[] applicationIds;
            private string applicationGuid;
            private string[] applicantIds;
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

                // build student programs
                // Set up response for student programs
                IEnumerable<StudentProgram> studentPrograms = await new TestStudentProgramRepository().GetAsync("0000894");
                studentProgramsResponseCollection = BuildStudentProgramsResponse(studentPrograms);
                studentProgramsResponseData = studentProgramsResponseCollection.First();

                // build applicants
                // Set up response for applicants
                applicantIds = expectedApplications.Where(ap => (!string.IsNullOrWhiteSpace(ap.ApplicantPersonId)))
                          .Select(ap => ap.ApplicantPersonId).Distinct().ToArray();
                applicantsResponseCollection = BuildApplicantsResponse(expectedApplications);
                applicantsResponseData = applicantsResponseCollection.First();

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
                applicationResponseCollection = null;
                studentProgramsResponseData = null;
                studentProgramsResponseCollection = null;
                expectedRepository = null;
                actualRepository = null;
                expectedApplications = null;
                expectedApplication = null;
                actualApplication = null;
                actualApplications = null;
                applicationId = null;
                applicationIds = null;
            }

            private AdmissionApplicationsRepository BuildRepository()
            {
                // transaction factory mock
                transFactoryMock = new Mock<IColleagueTransactionFactory>();
                transManagerMock = new Mock<IColleagueTransactionInvoker>();
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

                transFactoryMock.Setup(transFac => transFac.GetTransactionInvoker()).Returns(transManagerMock.Object);

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
                dataReaderMock.Setup(dr => dr.ReadRecordAsync<Applications>("APPLICATIONS", It.IsAny<GuidLookup>(), true)).ReturnsAsync(applicationResponseData);
                dataReaderMock.Setup(dr => dr.ReadRecordAsync<StudentPrograms>("STUDENT.PROGRAMS", It.IsAny<string>(), true)).ReturnsAsync(studentProgramsResponseData);
                dataReaderMock.Setup<Task<Collection<StudentPrograms>>>(dr => dr.BulkReadRecordAsync<StudentPrograms>(It.IsAny<string[]>(), true)).ReturnsAsync(studentProgramsResponseCollection);
                dataReaderMock.Setup(dr => dr.ReadRecordAsync<Applicants>("APPLICANTS", It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(applicantsResponseData);

                // Multiple Applications
                dataReaderMock.Setup(dr => dr.SelectAsync("APPLICATIONS", It.IsAny<string>())).ReturnsAsync(applicationIds);
                dataReaderMock.Setup(dr => dr.SelectAsync("APPLICATIONS", It.IsAny<string[]>(), It.IsAny<string>())).ReturnsAsync(applicationIds);
                dataReaderMock.Setup(dr => dr.BulkReadRecordAsync<Applications>("APPLICATIONS", It.IsAny<string[]>(), true)).ReturnsAsync(applicationResponseCollection);
                dataReaderMock.Setup(dr => dr.SelectAsync("APPLICANTS", It.IsAny<string[]>(), It.IsAny<string>())).ReturnsAsync(applicantIds);
                dataReaderMock.Setup(dr => dr.BulkReadRecordAsync<Applicants>(It.IsAny<string[]>(), It.IsAny<bool>())).ReturnsAsync(applicantsResponseCollection);

                var response = new Ellucian.Colleague.Domain.Base.Transactions.GetCacheApiKeysResponse()
                {
                    Offset = 0,
                    Limit = 1,
                    CacheName = "allAdmissionApplicationsCacheKey:",
                    Entity = "APPLICATIONS",
                    Sublist = new List<string>() { "1", "2"},
                    TotalCount = 20,
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
                        ApplLocationInfoEntityAssociation = new List<ApplicationsApplLocationInfo>(),
                       
                        ApplIntgCareerGoals = appl.CareerGoals,
                        ApplInfluencedToApply = appl.Influences
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

            private Collection<StudentPrograms> BuildStudentProgramsResponse(IEnumerable<StudentProgram> studentPrograms)
            {
                string[] programStatus = new string[] { "A", "P", "G", "C", "W" };
                int programStatusOffset = 0;
                Collection<StudentPrograms> repoStudentPrograms = new Collection<StudentPrograms>();
                foreach (var studentProgram in studentPrograms)
                {
                    if (programStatusOffset > 4)
                    {
                        programStatusOffset = 0;
                    }
                    var studentProgramData = new StudentPrograms();

                    if (applicationResponseCollection != null && applicationResponseCollection.Count() > programStatusOffset)
                    {
                        studentProgramData.Recordkey = applicationResponseCollection.ElementAt(programStatusOffset).ApplApplicant +
                            "*" + applicationResponseCollection.ElementAt(programStatusOffset).ApplAcadProgram;
                    }
                    else
                    {
                        studentProgramData.Recordkey = studentProgram.StudentId + "*" + studentProgram.ProgramCode;
                    }
                    studentProgramData.RecordGuid = Guid.NewGuid().ToString();
                    studentProgramData.StprAntCmplDate = studentProgram.AnticipatedCompletionDate;
                    studentProgramData.StprCatalog = studentProgram.CatalogCode;

                    studentProgramData.StprMajorListEntityAssociation = new List<StudentProgramsStprMajorList>();
                    studentProgramData.StprMinorListEntityAssociation = new List<StudentProgramsStprMinorList>();
                    studentProgramData.StprCcdListEntityAssociation = new List<StudentProgramsStprCcdList>();
                    studentProgramData.StprSpecialtiesEntityAssociation = new List<StudentProgramsStprSpecialties>();

                    studentProgramData.StprDaOverrides = new List<string>();
                    studentProgramData.StprDaExcpts = new List<string>();

                    studentProgramData.StprStatus = new List<string> { programStatus[programStatusOffset++], "A" };
                    studentProgramData.StprStartDate = new List<DateTime?>() { new DateTime() };
                    studentProgramData.StprSchool = string.Empty;

                    repoStudentPrograms.Add(studentProgramData);
                }

                // Add another repo response item with a previous end date
                var stuProgData = new StudentPrograms();
                stuProgData.Recordkey = "0003748*BA-MATH";
                stuProgData.RecordGuid = Guid.NewGuid().ToString();
                stuProgData.StprCatalog = "2012";
                stuProgData.StprStartDate = new List<DateTime?>() { DateTime.Today.AddDays(-100) };
                stuProgData.StprEndDate = new List<DateTime?>() { DateTime.Today.AddDays(-30) };
                stuProgData.StprDaOverrides = new List<string>();
                stuProgData.StprDaExcpts = new List<string>();
                stuProgData.StprStatus = new List<string>() { "G" };
                stuProgData.StprSchool = string.Empty;
                stuProgData.StprMajorListEntityAssociation = new List<StudentProgramsStprMajorList>();
                stuProgData.StprMinorListEntityAssociation = new List<StudentProgramsStprMinorList>();
                stuProgData.StprCcdListEntityAssociation = new List<StudentProgramsStprCcdList>();
                stuProgData.StprSpecialtiesEntityAssociation = new List<StudentProgramsStprSpecialties>();
                repoStudentPrograms.Add(stuProgData);

                // Add another repo response item with a noncurrent status
                var stuProgData1 = new StudentPrograms();
                stuProgData1.Recordkey = "0003748*MA-LAW";
                stuProgData1.RecordGuid = Guid.NewGuid().ToString();
                stuProgData1.StprCatalog = "2012";
                stuProgData1.StprStartDate = new List<DateTime?>() { DateTime.Today.AddDays(-100) };
                stuProgData1.StprStatus = new List<string>() { "C" };
                stuProgData1.StprSchool = string.Empty;
                stuProgData1.StprDaOverrides = new List<string>();
                stuProgData1.StprDaExcpts = new List<string>();
                stuProgData1.StprMajorListEntityAssociation = new List<StudentProgramsStprMajorList>();
                stuProgData1.StprMinorListEntityAssociation = new List<StudentProgramsStprMinorList>();
                stuProgData1.StprCcdListEntityAssociation = new List<StudentProgramsStprCcdList>();
                stuProgData1.StprSpecialtiesEntityAssociation = new List<StudentProgramsStprSpecialties>();
                repoStudentPrograms.Add(stuProgData1);

                // Add another repo response item with a withdrawn status
                var stuProgData2 = new StudentPrograms();
                stuProgData2.Recordkey = "0037487*MA-LAW";
                stuProgData2.RecordGuid = Guid.NewGuid().ToString();
                stuProgData2.StprCatalog = "2012";
                stuProgData2.StprStartDate = new List<DateTime?>() { DateTime.Today.AddDays(-100) };
                stuProgData2.StprStatus = new List<string>() { "W" };
                stuProgData2.StprSchool = string.Empty;
                stuProgData2.StprDaOverrides = new List<string>();
                stuProgData2.StprDaExcpts = new List<string>();
                stuProgData2.StprMajorListEntityAssociation = new List<StudentProgramsStprMajorList>();
                stuProgData2.StprMinorListEntityAssociation = new List<StudentProgramsStprMinorList>();
                stuProgData2.StprCcdListEntityAssociation = new List<StudentProgramsStprCcdList>();
                stuProgData2.StprSpecialtiesEntityAssociation = new List<StudentProgramsStprSpecialties>();
                repoStudentPrograms.Add(stuProgData2);

                // Add another repo response item with a no status
                var stuProgData3 = new StudentPrograms();
                stuProgData3.Recordkey = "2003894*MS-SCI";
                stuProgData3.RecordGuid = Guid.NewGuid().ToString();
                stuProgData3.StprCatalog = "2012";
                stuProgData3.StprStartDate = new List<DateTime?>() { DateTime.Today.AddDays(-100) };
                stuProgData3.StprStatus = new List<string>();
                stuProgData3.StprSchool = string.Empty;
                stuProgData3.StprDaOverrides = new List<string>();
                stuProgData3.StprDaExcpts = new List<string>();
                stuProgData3.StprMajorListEntityAssociation = new List<StudentProgramsStprMajorList>();
                stuProgData3.StprMinorListEntityAssociation = new List<StudentProgramsStprMinorList>();
                stuProgData3.StprCcdListEntityAssociation = new List<StudentProgramsStprCcdList>();
                stuProgData3.StprSpecialtiesEntityAssociation = new List<StudentProgramsStprSpecialties>();
                repoStudentPrograms.Add(stuProgData3);

                return repoStudentPrograms;
            }

            private Collection<Applicants> BuildApplicantsResponse(List<AdmissionApplication> applData)
            {
                var responseCollection = new Collection<Applicants>();
                foreach (var appl in applData)
                {
                    var applicantRecord = new Applicants()
                    {
                        AppOrigEducGoal = "PHD",
                        Recordkey = appl.ApplicantPersonId
                    };
                    responseCollection.Add(applicantRecord);
                }
                return responseCollection;
            }
            [TestMethod]
            public async Task ApplicationRecord_GetById_Test()
            {
                actualApplication = await actualRepository.GetAdmissionApplicationByIdAsync(applicationGuid);
                Assert.IsNotNull(actualApplication, "actualApplication is not null");
                Assert.AreEqual(expectedApplication.AdmittedOn, actualApplication.AdmittedOn, "AdmittedOn");
                Assert.AreEqual(expectedApplication.ApplicantPersonId, actualApplication.ApplicantPersonId, "ApplicantPersonId");
                Assert.AreEqual(expectedApplication.ApplicationAcadProgram, actualApplication.ApplicationAcadProgram, "ApplicationAcadProgram");
                Assert.AreEqual(expectedApplication.ApplicationAdmissionsRep, actualApplication.ApplicationAdmissionsRep, "ApplicationAdmissionsRep");
                Assert.AreEqual(expectedApplication.ApplicationAdmitStatus, actualApplication.ApplicationAdmitStatus, "ApplicationAdmitStatus");
                Assert.AreEqual(expectedApplication.ApplicationAttendedInstead, actualApplication.ApplicationAttendedInstead, "ApplicationAttendedInstead");
                Assert.AreEqual(expectedApplication.ApplicationComments, actualApplication.ApplicationComments, "ApplicationComments");
                Assert.AreEqual(expectedApplication.ApplicationIntgType, actualApplication.ApplicationIntgType, "ApplicationIntgType");
                Assert.AreEqual(expectedApplication.ApplicationNo, actualApplication.ApplicationNo, "ApplicationNo");
                Assert.AreEqual(expectedApplication.ApplicationOwnerId, actualApplication.ApplicationOwnerId, "ApplicationOwnerId");
                Assert.AreEqual(expectedApplication.ApplicationRecordKey, actualApplication.ApplicationRecordKey, "ApplicationRecordKey");
                Assert.AreEqual(expectedApplication.ApplicationResidencyStatus, actualApplication.ApplicationResidencyStatus, "ApplicationResidencyStatus");
                Assert.AreEqual(expectedApplication.ApplicationSchool, actualApplication.ApplicationSchool, "ApplicationSchool");
                Assert.AreEqual(expectedApplication.ApplicationSource, actualApplication.ApplicationSource, "ApplicationSource");
                Assert.AreEqual(expectedApplication.ApplicationStartTerm, actualApplication.ApplicationStartTerm, "ApplicationStartTerm");
                Assert.AreEqual(expectedApplication.ApplicationStudentLoadIntent, actualApplication.ApplicationStudentLoadIntent, "ApplicationStudentLoadIntent");
                Assert.AreEqual(expectedApplication.ApplicationWithdrawDate, actualApplication.ApplicationWithdrawDate, "ApplicationWithdrawDate");
                Assert.AreEqual(expectedApplication.ApplicationWithdrawReason, actualApplication.ApplicationWithdrawReason, "ApplicationWithdrawReason");
                Assert.AreEqual(expectedApplication.AppliedOn, actualApplication.AppliedOn, "AppliedOn");
                Assert.AreEqual(expectedApplication.MatriculatedOn, actualApplication.MatriculatedOn, "MatriculatedOn");
                Assert.AreEqual(expectedApplication.WithdrawnOn, actualApplication.WithdrawnOn, "WithdrawnOn");
            }

            [TestMethod]
            public async Task ApplicationRecord_GetById2_Test()
            {
                actualApplication = await actualRepository.GetAdmissionApplicationById2Async(applicationGuid);
                Assert.IsNotNull(actualApplication, "actualApplication is not null");
                Assert.IsNotNull(actualApplication.ApplicationAcadProgramGuid, "ApplicationAcadProgramGuid is not null");
                Assert.AreEqual(expectedApplication.AdmittedOn, actualApplication.AdmittedOn, "AdmittedOn");
                Assert.AreEqual(expectedApplication.ApplicantPersonId, actualApplication.ApplicantPersonId, "ApplicantPersonId");
                Assert.AreEqual(expectedApplication.ApplicationAcadProgram, actualApplication.ApplicationAcadProgram, "ApplicationAcadProgram");
                Assert.AreEqual(expectedApplication.ApplicationAdmissionsRep, actualApplication.ApplicationAdmissionsRep, "ApplicationAdmissionsRep");
                Assert.AreEqual(expectedApplication.ApplicationAdmitStatus, actualApplication.ApplicationAdmitStatus, "ApplicationAdmitStatus");
                Assert.AreEqual(expectedApplication.ApplicationAttendedInstead, actualApplication.ApplicationAttendedInstead, "ApplicationAttendedInstead");
                Assert.AreEqual(expectedApplication.ApplicationComments, actualApplication.ApplicationComments, "ApplicationComments");
                Assert.AreEqual(expectedApplication.ApplicationIntgType, actualApplication.ApplicationIntgType, "ApplicationIntgType");
                Assert.AreEqual(expectedApplication.ApplicationNo, actualApplication.ApplicationNo, "ApplicationNo");
                Assert.AreEqual(expectedApplication.ApplicationOwnerId, actualApplication.ApplicationOwnerId, "ApplicationOwnerId");
                Assert.AreEqual(expectedApplication.ApplicationRecordKey, actualApplication.ApplicationRecordKey, "ApplicationRecordKey");
                Assert.AreEqual(expectedApplication.ApplicationResidencyStatus, actualApplication.ApplicationResidencyStatus, "ApplicationResidencyStatus");
                Assert.AreEqual(expectedApplication.ApplicationSource, actualApplication.ApplicationSource, "ApplicationSource");
                Assert.AreEqual(expectedApplication.ApplicationStartTerm, actualApplication.ApplicationStartTerm, "ApplicationStartTerm");
                Assert.AreEqual(expectedApplication.ApplicationStudentLoadIntent, actualApplication.ApplicationStudentLoadIntent, "ApplicationStudentLoadIntent");
                Assert.AreEqual(expectedApplication.ApplicationWithdrawDate, actualApplication.ApplicationWithdrawDate, "ApplicationWithdrawDate");
                Assert.AreEqual(expectedApplication.ApplicationWithdrawReason, actualApplication.ApplicationWithdrawReason, "ApplicationWithdrawReason");
                Assert.AreEqual(expectedApplication.AppliedOn, actualApplication.AppliedOn, "AppliedOn");
                Assert.AreEqual(expectedApplication.MatriculatedOn, actualApplication.MatriculatedOn, "MatriculatedOn");
                Assert.AreEqual(expectedApplication.WithdrawnOn, actualApplication.WithdrawnOn, "WithdrawnOn");
                Assert.AreEqual(expectedApplication.EducationalGoal, actualApplication.EducationalGoal, "EducationalGoal");
                Assert.AreEqual(expectedApplication.CareerGoals, actualApplication.CareerGoals, "CareerGoals");
                Assert.AreEqual(expectedApplication.Influences, actualApplication.Influences, "Influences");
            }

            [TestMethod]
            public async Task ApplicationRecord_GetAll_Test()
            {
                var pageOfApplications = await actualRepository.GetAdmissionApplicationsAsync(offset, limit, false);
                actualApplications = pageOfApplications.Item1 as List<AdmissionApplication>;

                Assert.IsTrue(actualApplications.Any(), "Actual Applications Returned");
                int index = 0;
                foreach (var actualApplication in actualApplications)
                {
                    expectedApplication = expectedApplications.ElementAt(index++);
                    Assert.IsNotNull(actualApplication, "actualApplication is not null");
                    Assert.AreEqual(expectedApplication.AdmittedOn, actualApplication.AdmittedOn, "AdmittedOn");
                    Assert.AreEqual(expectedApplication.ApplicantPersonId, actualApplication.ApplicantPersonId, "ApplicantPersonId");
                    Assert.AreEqual(expectedApplication.ApplicationAcadProgram, actualApplication.ApplicationAcadProgram, "ApplicationAcadProgram");
                    Assert.AreEqual(expectedApplication.ApplicationAdmissionsRep, actualApplication.ApplicationAdmissionsRep, "ApplicationAdmissionsRep");
                    Assert.AreEqual(expectedApplication.ApplicationAdmitStatus, actualApplication.ApplicationAdmitStatus, "ApplicationAdmitStatus");
                    Assert.AreEqual(expectedApplication.ApplicationAttendedInstead, actualApplication.ApplicationAttendedInstead, "ApplicationAttendedInstead");
                    Assert.AreEqual(expectedApplication.ApplicationComments, actualApplication.ApplicationComments, "ApplicationComments");
                    Assert.AreEqual(expectedApplication.ApplicationIntgType, actualApplication.ApplicationIntgType, "ApplicationIntgType");
                    Assert.AreEqual(expectedApplication.ApplicationNo, actualApplication.ApplicationNo, "ApplicationNo");
                    Assert.AreEqual(expectedApplication.ApplicationOwnerId, actualApplication.ApplicationOwnerId, "ApplicationOwnerId");
                    Assert.AreEqual(expectedApplication.ApplicationRecordKey, actualApplication.ApplicationRecordKey, "ApplicationRecordKey");
                    Assert.AreEqual(expectedApplication.ApplicationResidencyStatus, actualApplication.ApplicationResidencyStatus, "ApplicationResidencyStatus");
                    Assert.AreEqual(expectedApplication.ApplicationSchool, actualApplication.ApplicationSchool, "ApplicationSchool");
                    Assert.AreEqual(expectedApplication.ApplicationSource, actualApplication.ApplicationSource, "ApplicationSource");
                    Assert.AreEqual(expectedApplication.ApplicationStartTerm, actualApplication.ApplicationStartTerm, "ApplicationStartTerm");
                    Assert.AreEqual(expectedApplication.ApplicationStudentLoadIntent, actualApplication.ApplicationStudentLoadIntent, "ApplicationStudentLoadIntent");
                    Assert.AreEqual(expectedApplication.ApplicationWithdrawDate, actualApplication.ApplicationWithdrawDate, "ApplicationWithdrawDate");
                    Assert.AreEqual(expectedApplication.ApplicationWithdrawReason, actualApplication.ApplicationWithdrawReason, "ApplicationWithdrawReason");
                    Assert.AreEqual(expectedApplication.AppliedOn, actualApplication.AppliedOn, "AppliedOn");
                    Assert.AreEqual(expectedApplication.MatriculatedOn, actualApplication.MatriculatedOn, "MatriculatedOn");
                    Assert.AreEqual(expectedApplication.WithdrawnOn, actualApplication.WithdrawnOn, "WithdrawnOn");
                }
            }

            [TestMethod]
            public async Task ApplicationRecord_GetAll2_Test()
            {
                var pageOfApplications = await actualRepository.GetAdmissionApplications2Async(offset, limit);
                actualApplications = pageOfApplications.Item1 as List<AdmissionApplication>;

                Assert.IsTrue(actualApplications.Any(), "Actual Applications Returned");
                int index = 0;
                foreach (var actualApplication in actualApplications)
                {
                    expectedApplication = expectedApplications.ElementAt(index++);
                    Assert.IsNotNull(actualApplication, "actualApplication is not null");
                    Assert.AreEqual(expectedApplication.AdmittedOn, actualApplication.AdmittedOn, "AdmittedOn");
                    Assert.AreEqual(expectedApplication.ApplicantPersonId, actualApplication.ApplicantPersonId, "ApplicantPersonId");
                    Assert.AreEqual(expectedApplication.ApplicationAcadProgram, actualApplication.ApplicationAcadProgram, "ApplicationAcadProgram");
                    Assert.AreEqual(expectedApplication.ApplicationAdmissionsRep, actualApplication.ApplicationAdmissionsRep, "ApplicationAdmissionsRep");
                    Assert.AreEqual(expectedApplication.ApplicationAdmitStatus, actualApplication.ApplicationAdmitStatus, "ApplicationAdmitStatus");
                    Assert.AreEqual(expectedApplication.ApplicationAttendedInstead, actualApplication.ApplicationAttendedInstead, "ApplicationAttendedInstead");
                    Assert.AreEqual(expectedApplication.ApplicationComments, actualApplication.ApplicationComments, "ApplicationComments");
                    Assert.AreEqual(expectedApplication.ApplicationIntgType, actualApplication.ApplicationIntgType, "ApplicationIntgType");
                    Assert.AreEqual(expectedApplication.ApplicationNo, actualApplication.ApplicationNo, "ApplicationNo");
                    Assert.AreEqual(expectedApplication.ApplicationOwnerId, actualApplication.ApplicationOwnerId, "ApplicationOwnerId");
                    Assert.AreEqual(expectedApplication.ApplicationRecordKey, actualApplication.ApplicationRecordKey, "ApplicationRecordKey");
                    Assert.AreEqual(expectedApplication.ApplicationResidencyStatus, actualApplication.ApplicationResidencyStatus, "ApplicationResidencyStatus");
                    Assert.AreEqual(expectedApplication.ApplicationSource, actualApplication.ApplicationSource, "ApplicationSource");
                    Assert.AreEqual(expectedApplication.ApplicationStartTerm, actualApplication.ApplicationStartTerm, "ApplicationStartTerm");
                    Assert.AreEqual(expectedApplication.ApplicationStudentLoadIntent, actualApplication.ApplicationStudentLoadIntent, "ApplicationStudentLoadIntent");
                    Assert.AreEqual(expectedApplication.ApplicationWithdrawDate, actualApplication.ApplicationWithdrawDate, "ApplicationWithdrawDate");
                    Assert.AreEqual(expectedApplication.ApplicationWithdrawReason, actualApplication.ApplicationWithdrawReason, "ApplicationWithdrawReason");
                    Assert.AreEqual(expectedApplication.AppliedOn, actualApplication.AppliedOn, "AppliedOn");
                    Assert.AreEqual(expectedApplication.MatriculatedOn, actualApplication.MatriculatedOn, "MatriculatedOn");
                    Assert.AreEqual(expectedApplication.WithdrawnOn, actualApplication.WithdrawnOn, "WithdrawnOn");
                    Assert.AreEqual(expectedApplication.EducationalGoal, actualApplication.EducationalGoal, "EducationalGoal");
                    Assert.AreEqual(expectedApplication.CareerGoals, actualApplication.CareerGoals, "CareerGoals");
                    Assert.AreEqual(expectedApplication.Influences, actualApplication.Influences, "Influences");
                }
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
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task NullApplicationRecord2_ExceptionTest()
            {
                //set the response data object to null and the dataReaderMock
                applicationResponseData = null;
                dataReaderMock.Setup(dr => dr.ReadRecordAsync<Applications>("APPLICATIONS", It.IsAny<GuidLookup>(), true)).ReturnsAsync(applicationResponseData);

                actualApplication = await actualRepository.GetAdmissionApplicationById2Async(applicationGuid);
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

            [TestMethod]
            public async Task MissingProgramRecords_GetById_ExceptionTest()
            {
                //set the response data object to null and the dataReaderMock
                studentProgramsResponseCollection = null;
                dataReaderMock.Setup<Task<Collection<StudentPrograms>>>(dr => dr.BulkReadRecordAsync<StudentPrograms>(It.IsAny<string[]>(), true)).ReturnsAsync(studentProgramsResponseCollection);

                var applications = await actualRepository.GetAdmissionApplicationByIdAsync(applicationGuid);
                Assert.IsTrue(!applications.ApplicationStprAcadPrograms.Any(), "No ApplicationStprAcadPrograms exist");
                Assert.IsTrue(applications.ApplicationSchool == string.Empty, "ApplicationSchool is empty");
            }

            [TestMethod]
            [ExpectedException(typeof(RepositoryException))]
            public async Task MissingProgramRecords_GetById2_ExceptionTest()
            {
                //set the response data object to null and the dataReaderMock
                studentProgramsResponseCollection = null;
                dataReaderMock.Setup<Task<Collection<StudentPrograms>>>(dr => dr.BulkReadRecordAsync<StudentPrograms>(It.IsAny<string[]>(), true)).ReturnsAsync(studentProgramsResponseCollection);

                var applications = await actualRepository.GetAdmissionApplicationById2Async(applicationGuid);
            }

            [TestMethod]
            public async Task NullApplicationRecords_GetAll_ReturnsEmptySet()
            {
                //set the response data object to null and the dataReaderMock
                applicationResponseCollection = null;
                dataReaderMock.Setup(dr => dr.BulkReadRecordAsync<Applications>("APPLICATIONS", It.IsAny<string[]>(), true)).ReturnsAsync(applicationResponseCollection);

                var applicationsTuple = await actualRepository.GetAdmissionApplicationsAsync(offset, limit, false);
                Assert.IsTrue(applicationsTuple.Item2 == 0);
                Assert.IsTrue(applicationsTuple.Item1.Count() == 0);
            }

            [TestMethod]
            public async Task NullApplicationRecords_GetAll2_ReturnsEmptySet()
            {
                //set the response data object to null and the dataReaderMock
                applicationResponseCollection = null;
                dataReaderMock.Setup(dr => dr.BulkReadRecordAsync<Applications>("APPLICATIONS", It.IsAny<string[]>(), true)).ReturnsAsync(applicationResponseCollection);

                var applicationsTuple = await actualRepository.GetAdmissionApplications2Async(offset, limit);
                Assert.IsTrue(applicationsTuple.Item2 == 0);
                Assert.IsTrue(applicationsTuple.Item1.Count() == 0);
            }
        }
    }

    [TestClass]
    public class AdmissionApplicationsRepositoryTests_V11
    {
        [TestClass]
        public class AdmissionApplicationsRepositoryTests_POST_AND_POST : BaseRepositorySetup
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
                    { "KEY", new GuidLookupResult() { Entity = "APPLICATIONS", PrimaryKey = "1" } }
                };

                dataReaderMock.SetupSequence(d => d.SelectAsync(It.IsAny<GuidLookup[]>())).Returns(Task.FromResult(firstResult)).Returns(Task.FromResult(dicResult));

                var result = await admissionApplicationsRepository.UpdateAdmissionApplicationAsync(entity);

                Assert.IsNotNull(result);
                Assert.AreEqual(result.Guid, guid);
            }
        }
    }

    [TestClass]
    public class AdmissionApplicationsSubmissionsRepositoryTests : BaseRepositorySetup
    {
        #region DECLARATIONS

        private AdmissionApplicationsRepository admissionApplicationsRepository;

        private UpdateProspectOrApplicantResponse response;

        private Dictionary<string, GuidLookupResult> dicResult;

        private AdmissionApplication entity;

        private Applications application;

        private Applicants applicant; 
        
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
            admissionApplicationsRepository = null;
            response = null;
            dicResult = null;
            entity = null;
            application = null;
            applicant = null;
            applicationStatuses = null;
            studentPrograms = null;
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
                WithdrawnOn = DateTime.Now.AddDays(-10),
                EducationalGoal = "1",
                CareerGoals = new List<string>() { "1", "2" },
                Influences = new List<string>() { "1", "2" }
            };

            entity.ApplicationDisciplines = new List<ApplicationDiscipline>()
            {
                new ApplicationDiscipline()
                {
                    AdministeringInstitutionUnit = "1",
                    Code = "Disc 1",
                    DisciplineType = Domain.Base.Entities.AcademicDisciplineType.Major,

                },
                new ApplicationDiscipline()
                {
                    AdministeringInstitutionUnit = "1",
                    Code = "Disc 2",
                    DisciplineType = Domain.Base.Entities.AcademicDisciplineType.Minor,

                },
                new ApplicationDiscipline()
                {
                    AdministeringInstitutionUnit = "1",
                    Code = "Disc 3",
                    DisciplineType = Domain.Base.Entities.AcademicDisciplineType.Concentration,

                }
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
                    },
                ApplIntgCareerGoals = new List<string>() { "1", "2" },
                ApplInfluencedToApply = new List<string>() { "1", "2" }
            };

            applicant = new Applicants()
            {
                Recordkey = "1",
                AppOrigEducGoal = "1"
            };

            studentPrograms = new Collection<StudentPrograms>()
                {
                    new StudentPrograms(){RecordGuid = "1a49eed8-5fe7-4120-b1cf-f23266b9e874", Recordkey = "1*1"}
                };

            response = new UpdateProspectOrApplicantResponse() { Guid = "1a49eed8-5fe7-4120-b1cf-f23266b9e874" };
        }

        private void InitializeTestMock()
        {
            transManagerMock.Setup(t => t.ExecuteAsync<UpdateProspectOrApplicantRequest, UpdateProspectOrApplicantResponse>(It.IsAny<UpdateProspectOrApplicantRequest>()))
                .ReturnsAsync(response);
            dataReaderMock.Setup(d => d.SelectAsync(It.IsAny<GuidLookup[]>())).ReturnsAsync(dicResult);
            dataReaderMock.Setup(d => d.SelectAsync(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(new string[] { "1a49eed8-5fe7-4120-b1cf-f23266b9e874" });
            dataReaderMock.Setup(d => d.BulkReadRecordAsync<ApplicationStatuses>(It.IsAny<string>(), true)).ReturnsAsync(applicationStatuses);
            dataReaderMock.Setup(d => d.BulkReadRecordAsync<StudentPrograms>(It.IsAny<string[]>(), true)).ReturnsAsync(studentPrograms);
            dataReaderMock.Setup(d => d.ReadRecordAsync<Applications>(It.IsAny<string>(), It.IsAny<string>(), true)).ReturnsAsync(application);
            dataReaderMock.Setup(d => d.ReadRecordAsync<Applications>(It.IsAny<string>(), It.IsAny<GuidLookup>(), true)).ReturnsAsync(application);
            dataReaderMock.Setup(d => d.ReadRecordAsync<Applicants>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(applicant);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task UpdateAdmissionApplicationSubmissionAsync_ArgumentNullException_Null_Entity()
        {
            await admissionApplicationsRepository.UpdateAdmissionApplicationSubmissionAsync(null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task UpdateAdmissionApplicationSubmissionAsync_ArgumentNullException_Null_Id()
        {
            AdmissionApplication entity = new AdmissionApplication(" ");
            await admissionApplicationsRepository.UpdateAdmissionApplicationSubmissionAsync(entity);
        }

        [TestMethod]
        [ExpectedException(typeof(RepositoryException))]
        public async Task UpdateAdmissionApplicationSubmissionAsync_RepositoryException()
        {
            response.UpdateProspectApplicantErrors = new List<UpdateProspectApplicantErrors>()
            {
                new UpdateProspectApplicantErrors(){ ErrorCodes = "Error" }
            };
            await admissionApplicationsRepository.UpdateAdmissionApplicationSubmissionAsync(entity);
        }

        [TestMethod]
        public async Task UpdateAdmissionApplicationSubmissionAsync()
        {
            admissionApplicationsRepository.EthosExtendedDataDictionary = new Dictionary<string, string>();
            admissionApplicationsRepository.EthosExtendedDataDictionary.Add("key", "value");
            var actual = await admissionApplicationsRepository.UpdateAdmissionApplicationSubmissionAsync(entity);
            Assert.IsNotNull(actual);
            Assert.AreEqual(actual.Guid, guid);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task CreateAdmissionApplicationSubmissionAsync_ArgumentNullException()
        {
            var actual = await admissionApplicationsRepository.CreateAdmissionApplicationSubmissionAsync(null);
        }

        [TestMethod]
        [ExpectedException(typeof(RepositoryException))]
        public async Task CreateAdmissionApplicationSubmissionAsync_RepositoryException()
        {
            response.UpdateProspectApplicantErrors = new List<UpdateProspectApplicantErrors>()
            {
                new UpdateProspectApplicantErrors(){ ErrorCodes = "Error" }
            };
            var actual = await admissionApplicationsRepository.CreateAdmissionApplicationSubmissionAsync(entity);
        }

        [TestMethod]
        public async Task CreateAdmissionApplicationSubmissionAsync()
        {
            admissionApplicationsRepository.EthosExtendedDataDictionary = new Dictionary<string, string>();
            admissionApplicationsRepository.EthosExtendedDataDictionary.Add("key", "value");

            var actual = await admissionApplicationsRepository.CreateAdmissionApplicationSubmissionAsync(entity);
            Assert.IsNotNull(actual);
            Assert.AreEqual(actual.Guid, guid);
        }

        [TestMethod]
        public async Task GetAdmissionApplicationSubmissionByIdAsync()
        {
            var actual = await admissionApplicationsRepository.GetAdmissionApplicationSubmissionByIdAsync("1234", It.IsAny<bool>());
            Assert.IsNotNull(actual);
            Assert.AreEqual(actual.Guid, guid);
        }

        [TestMethod]
        [ExpectedException(typeof(KeyNotFoundException))]
        public async Task GetAdmissionApplicationSubmissionByIdAsync_KeyNotFoundException()
        {
            dataReaderMock.Setup(repo => repo.ReadRecordAsync<Applications>("APPLICATIONS", It.IsAny<GuidLookup>(), It.IsAny<bool>())).ReturnsAsync(() => null);
            var actual = await admissionApplicationsRepository.GetAdmissionApplicationSubmissionByIdAsync("1234", It.IsAny<bool>());
        }

        [TestMethod]
        [ExpectedException(typeof(KeyNotFoundException))]
        public async Task GetAdmissionApplicationSubmissionByIdAsync_StatusNUll_KeyNotFoundException()
        {
            applicationStatuses = new Collection<ApplicationStatuses>()
                {
                    new ApplicationStatuses() {RecordGuid = "3a49eed8-5fe7-4120-b1cf-f23266b9e874", Recordkey = "1"}
                };
            dataReaderMock.Setup(repo => repo.BulkReadRecordAsync<ApplicationStatuses>("WITH APPS.SPECIAL.PROCESSING.CODE EQ ''", It.IsAny<bool>())).ReturnsAsync(applicationStatuses);
            var actual = await admissionApplicationsRepository.GetAdmissionApplicationSubmissionByIdAsync("1234", It.IsAny<bool>());
        }
        #endregion
    }
}
