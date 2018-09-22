// Copyright 2012-2018 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Data.Base.DataContracts;
using Ellucian.Colleague.Data.Base.Tests.Repositories;
using Ellucian.Colleague.Data.Student.DataContracts;
using Ellucian.Colleague.Data.Student.Repositories;
using Ellucian.Colleague.Domain.Base.Entities;
using Ellucian.Colleague.Domain.Student.Entities;
using Ellucian.Colleague.Domain.Student.Repositories;
using Ellucian.Colleague.Domain.Student.Tests;
using Ellucian.Data.Colleague;
using Ellucian.Data.Colleague.DataContracts;
using Ellucian.Dmi.Runtime;
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

namespace Ellucian.Colleague.Data.Student.Tests.Repositories
{
    [TestClass]
    public class StudentReferenceDataRepositoryTests : BaseRepositorySetup
    {
        StudentReferenceDataRepository referenceDataRepo;

        public void MainInitialize()
        {
            base.MockInitialize();
            referenceDataRepo = new StudentReferenceDataRepository(cacheProvider, transFactory, logger, apiSettings);
        }

        [TestClass]
        public class AcademicDepartmentsTest
        {
            Mock<IColleagueTransactionFactory> transFactoryMock;
            Mock<ICacheProvider> cacheProviderMock;
            Mock<IColleagueDataReader> dataAccessorMock;
            Mock<ILogger> loggerMock;

            IEnumerable<AcademicDepartment> allAcademicDepartments;
            string valcodeName;
            ApiSettings apiSettings;

            StudentReferenceDataRepository referenceDataRepo;

            [TestInitialize]
            public void Initialize()
            {
                loggerMock = new Mock<ILogger>();
                apiSettings = new ApiSettings("TEST");

                allAcademicDepartments = new TestStudentReferenceDataRepository().GetAcademicDepartmentsAsync(false).Result;

                referenceDataRepo = BuildValidReferenceDataRepository();
                valcodeName = referenceDataRepo.BuildFullCacheKey("AllOfferingDepartments");
            }

            [TestCleanup]
            public void Cleanup()
            {
                allAcademicDepartments = null;
                valcodeName = string.Empty;
                apiSettings = null;
            }

            [TestMethod]
            public async Task StudentReferenceDataRepository_GetAcademicDepartmentsAsync_False()
            {
                var results = await referenceDataRepo.GetAcademicDepartmentsAsync(false);
                Assert.AreEqual(allAcademicDepartments.Count(), results.Count());

                foreach (var academicDepartment in allAcademicDepartments)
                {
                    var result = results.FirstOrDefault(i => i.Guid == academicDepartment.Guid);

                    Assert.AreEqual(academicDepartment.Code, result.Code);
                    Assert.AreEqual(academicDepartment.Description, result.Description);
                    Assert.AreEqual(academicDepartment.Guid, result.Guid);
                }

            }

            private StudentReferenceDataRepository BuildValidReferenceDataRepository()
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

                var records = new Collection<Depts>();
                foreach (var item in allAcademicDepartments)
                {
                    Depts record = new Depts();
                    record.RecordGuid = item.Guid;
                    record.DeptsDesc = item.Description;
                    record.Recordkey = item.Code;
                    records.Add(record);
                }
                dataAccessorMock.Setup(acc => acc.BulkReadRecordAsync<Depts>("DEPTS", It.IsAny<GuidLookup[]>(), It.IsAny<bool>())).ReturnsAsync(records);
                dataAccessorMock.Setup(acc => acc.BulkReadRecordAsync<Depts>(It.IsAny<string[]>(), It.IsAny<bool>())).ReturnsAsync(records);

                cacheProviderMock.Setup<Task<Tuple<object, SemaphoreSlim>>>(x =>
                 x.GetAndLockSemaphoreAsync(It.IsAny<string>(), null))
                 .ReturnsAsync(new Tuple<object, SemaphoreSlim>(null, new SemaphoreSlim(1, 1)));

                dataAccessorMock.Setup(acc => acc.SelectAsync("LDM.GUID", It.IsAny<string>())).ReturnsAsync(new string[] { "0500da20-1336-4319-bacf-efe4f4dde018", "62244057-D1EC-4094-A0B7-DE602533E3A6", "fea97622-4445-4666-b673-948227ce7ed2", "6ef164eb-8178-4321-a9f7-24f12d3991d8" });

                dataAccessorMock.Setup(acc => acc.SelectAsync(It.IsAny<RecordKeyLookup[]>())).Returns<RecordKeyLookup[]>(recordKeyLookups =>
                {
                    var result = new Dictionary<string, RecordKeyLookupResult>();
                    int index = 0;
                    foreach (var recordKeyLookup in recordKeyLookups)
                    {
                        var record = allAcademicDepartments.Where(e => e.Code == recordKeyLookup.PrimaryKey).FirstOrDefault();
                        //result.Add(string.Join("+", new string[] { "MEAL.PLANS", record.Code }),
                        result.Add(record.Guid,
                            new RecordKeyLookupResult() { Guid = record.Guid });
                    }
                    return Task.FromResult(result);
                });

                dataAccessorMock.Setup(acc => acc.SelectAsync(It.IsAny<GuidLookup[]>())).Returns<GuidLookup[]>(recordKeyLookups =>
                {
                    var result = new Dictionary<string, GuidLookupResult>();
                    foreach (var recordKeyLookup in recordKeyLookups)
                    {
                        var record = allAcademicDepartments.Where(e => e.Guid == recordKeyLookup.Guid).FirstOrDefault();
                        //result.Add(string.Join("+", new string[] { "MEAL.PLANS", record.Code }),
                        result.Add(record.Guid,
                            new GuidLookupResult() { PrimaryKey = record.Code });
                    }
                    return Task.FromResult(result);
                });

                // Construct repository
                referenceDataRepo = new StudentReferenceDataRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object, apiSettings);

                return referenceDataRepo;
            }
        }

        [TestClass]
        public class AcademicLevels
        {
            Mock<IColleagueTransactionFactory> transFactoryMock;
            Mock<ICacheProvider> cacheProviderMock;
            Mock<IColleagueDataReader> dataAccessorMock;
            Mock<ILogger> loggerMock;
            IEnumerable<AcademicLevel> allAcademicLevel;
            ApplValcodes academicLevelValcodeResponse;
            string valcodeName;
            ApiSettings apiSettings;

            StudentReferenceDataRepository referenceDataRepo;

            [TestInitialize]
            public void Initialize()
            {
                loggerMock = new Mock<ILogger>();
                apiSettings = new ApiSettings("TEST");

                // Build Academic Level responses used for mocking
                allAcademicLevel = new TestAcademicLevelRepository().GetAsync().Result;
                academicLevelValcodeResponse = BuildValcodeResponse(allAcademicLevel);

                // Build Academic Level repository
                referenceDataRepo = BuildValidReferenceDataRepository();
                valcodeName = referenceDataRepo.BuildFullCacheKey("AllAcademicLevels");
            }

            [TestCleanup]
            public void Cleanup()
            {
                transFactoryMock = null;
                dataAccessorMock = null;
                cacheProviderMock = null;
                academicLevelValcodeResponse = null;
                allAcademicLevel = null;
                referenceDataRepo = null;
            }

            [TestMethod]
            public async Task StudentReferenceDataRepo_AcademicLevels_GetAcademicLevelsAsync()
            {
                var repoGetAcademicLevels = await referenceDataRepo.GetAcademicLevelsAsync();
                for (int i = 0; i < allAcademicLevel.Count(); i++)
                {
                    Assert.AreEqual(allAcademicLevel.ElementAt(i).Code, repoGetAcademicLevels.ElementAt(i).Code);
                    Assert.AreEqual(allAcademicLevel.ElementAt(i).Description, repoGetAcademicLevels.ElementAt(i).Description);
                }
            }

            [TestMethod]
            public async Task StudentReferenceDataRepo_AcademicLevels_GetAcademicLevelsAsync_Cache()
            {
                var repoGetAcademicLevels = await referenceDataRepo.GetAcademicLevelsAsync(false);
                for (int i = 0; i < allAcademicLevel.Count(); i++)
                {
                    Assert.AreEqual(allAcademicLevel.ElementAt(i).Code, repoGetAcademicLevels.ElementAt(i).Code);
                    Assert.AreEqual(allAcademicLevel.ElementAt(i).Description, repoGetAcademicLevels.ElementAt(i).Description);
                }
            }

            [TestMethod]
            public async Task StudentReferenceDataRepo_AcademicLevels_GetAcademicLevelsAsync_NoCache()
            {
                var repoGetAcademicLevels = await referenceDataRepo.GetAcademicLevelsAsync(true);
                for (int i = 0; i < allAcademicLevel.Count(); i++)
                {
                    Assert.AreEqual(allAcademicLevel.ElementAt(i).Code, repoGetAcademicLevels.ElementAt(i).Code);
                    Assert.AreEqual(allAcademicLevel.ElementAt(i).Description, repoGetAcademicLevels.ElementAt(i).Description);
                }
            }

            private StudentReferenceDataRepository BuildValidReferenceDataRepository()
            {
                // transaction factory mock
                transFactoryMock = new Mock<IColleagueTransactionFactory>();
                // Cache Mock
                //localCacheMock = new Mock<ObjectCache>();
                // Cache Provider Mock
                cacheProviderMock = new Mock<ICacheProvider>();
                // Set up data accessor for mocking 
                dataAccessorMock = new Mock<IColleagueDataReader>();
                apiSettings = new ApiSettings("TEST");

                // Set up dataAccessorMock as the object for the DataAccessor
                transFactoryMock.Setup(transFac => transFac.GetDataReader()).Returns(dataAccessorMock.Object);

                var records = new Collection<DataContracts.AcadLevels>();
                foreach (var item in allAcademicLevel)
                {
                    DataContracts.AcadLevels record = new DataContracts.AcadLevels();
                    record.RecordGuid = item.Guid;
                    record.AclvDesc = item.Description;
                    record.Recordkey = item.Code;
                    records.Add(record);
                }
                dataAccessorMock.Setup(acc => acc.BulkReadRecordAsync<DataContracts.AcadLevels>("ACAD.LEVELS", "", true)).ReturnsAsync(records);

                cacheProviderMock.Setup<Task<Tuple<object, SemaphoreSlim>>>(x =>
                 x.GetAndLockSemaphoreAsync(It.IsAny<string>(), null))
                 .ReturnsAsync(new Tuple<object, SemaphoreSlim>(null, new SemaphoreSlim(1, 1)));

                dataAccessorMock.Setup(acc => acc.SelectAsync(It.IsAny<RecordKeyLookup[]>())).Returns<RecordKeyLookup[]>(recordKeyLookups =>
                {
                    var result = new Dictionary<string, RecordKeyLookupResult>();
                    foreach (var recordKeyLookup in recordKeyLookups)
                    {
                        var record = allAcademicLevel.Where(e => e.Code == recordKeyLookup.PrimaryKey).FirstOrDefault();
                        result.Add(string.Join("+", new string[] { "ACAD.LEVELS", record.Code }),
                            new RecordKeyLookupResult() { Guid = record.Guid });
                    }
                    return Task.FromResult(result);
                });

                // Construct repository
                referenceDataRepo = new StudentReferenceDataRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object, apiSettings);

                return referenceDataRepo;
            }

            private ApplValcodes BuildValcodeResponse(IEnumerable<AcademicLevel> academicLevel)
            {
                ApplValcodes academicLevelResponse = new ApplValcodes();
                academicLevelResponse.ValsEntityAssociation = new List<ApplValcodesVals>();
                foreach (var item in academicLevel)
                {
                    academicLevelResponse.ValsEntityAssociation.Add(new ApplValcodesVals("", item.Description, "", item.Code, "", "", ""));
                }
                return academicLevelResponse;
            }
        }

        [TestClass]
        public class AcademicPrograms
        {
            Mock<IColleagueTransactionFactory> transFactoryMock;
            Mock<ICacheProvider> cacheProviderMock;
            Mock<IColleagueDataReader> dataAccessorMock;
            Mock<ILogger> loggerMock;
            IEnumerable<AcademicProgram> allAcademicPrograms;
            ApiSettings apiSettings;

            StudentReferenceDataRepository referenceDataRepo;

            [TestInitialize]
            public void Initialize()
            {
                loggerMock = new Mock<ILogger>();
                apiSettings = new ApiSettings("TEST");

                // Build responses used for mocking
                allAcademicPrograms = new TestAcademicProgramRepository().GetAsync().Result;

                // Build repository
                referenceDataRepo = BuildValidReferenceDataRepository();
            }

            [TestCleanup]
            public void Cleanup()
            {
                transFactoryMock = null;
                dataAccessorMock = null;
                cacheProviderMock = null;
                allAcademicPrograms = null;
                referenceDataRepo = null;
            }

            [TestMethod]
            public async Task StudentReferenceDataRepo_AcademicPrograms_GetAcademicProgramsAsync()
            {
                var repoGetAcademicPrograms = await referenceDataRepo.GetAcademicProgramsAsync();
                for (int i = 0; i < allAcademicPrograms.Count(); i++)
                {
                    Assert.AreEqual(allAcademicPrograms.ElementAt(i).Code, repoGetAcademicPrograms.ElementAt(i).Code);
                    Assert.AreEqual(allAcademicPrograms.ElementAt(i).Description, repoGetAcademicPrograms.ElementAt(i).Description);
                    Assert.AreEqual(allAcademicPrograms.ElementAt(i).AcadLevelCode, repoGetAcademicPrograms.ElementAt(i).AcadLevelCode);
                    Assert.AreEqual(allAcademicPrograms.ElementAt(i).CertificateCodes.FirstOrDefault(), repoGetAcademicPrograms.ElementAt(i).CertificateCodes.FirstOrDefault());
                    Assert.AreEqual(allAcademicPrograms.ElementAt(i).DegreeCode, repoGetAcademicPrograms.ElementAt(i).DegreeCode);
                    Assert.AreEqual(allAcademicPrograms.ElementAt(i).DeptartmentCodes.FirstOrDefault(), repoGetAcademicPrograms.ElementAt(i).DeptartmentCodes.FirstOrDefault());
                    Assert.AreEqual(allAcademicPrograms.ElementAt(i).EndDate, repoGetAcademicPrograms.ElementAt(i).EndDate);
                    Assert.AreEqual(allAcademicPrograms.ElementAt(i).FederalCourseClassification, repoGetAcademicPrograms.ElementAt(i).FederalCourseClassification);
                    Assert.AreEqual(allAcademicPrograms.ElementAt(i).Guid, repoGetAcademicPrograms.ElementAt(i).Guid);
                    Assert.AreEqual(allAcademicPrograms.ElementAt(i).HonorCode, repoGetAcademicPrograms.ElementAt(i).HonorCode);
                    Assert.AreEqual(allAcademicPrograms.ElementAt(i).LocalCourseClassifications.FirstOrDefault(), repoGetAcademicPrograms.ElementAt(i).LocalCourseClassifications.FirstOrDefault());
                    Assert.AreEqual(allAcademicPrograms.ElementAt(i).LongDescription, repoGetAcademicPrograms.ElementAt(i).LongDescription);
                    Assert.AreEqual(allAcademicPrograms.ElementAt(i).MajorCodes.FirstOrDefault(), repoGetAcademicPrograms.ElementAt(i).MajorCodes.FirstOrDefault());
                    Assert.AreEqual(allAcademicPrograms.ElementAt(i).MinorCodes.FirstOrDefault(), repoGetAcademicPrograms.ElementAt(i).MinorCodes.FirstOrDefault());
                    Assert.AreEqual(allAcademicPrograms.ElementAt(i).SpecializationCodes.FirstOrDefault(), repoGetAcademicPrograms.ElementAt(i).SpecializationCodes.FirstOrDefault());
                    Assert.AreEqual(allAcademicPrograms.ElementAt(i).StartDate, repoGetAcademicPrograms.ElementAt(i).StartDate);
                }
            }

            [TestMethod]
            public async Task StudentReferenceDataRepo_AcademicPrograms_GetAcademicProgramsAsync_Cache()
            {
                var repoGetAcademicPrograms = await referenceDataRepo.GetAcademicProgramsAsync(false);
                for (int i = 0; i < allAcademicPrograms.Count(); i++)
                {
                    Assert.AreEqual(allAcademicPrograms.ElementAt(i).Code, repoGetAcademicPrograms.ElementAt(i).Code);
                    Assert.AreEqual(allAcademicPrograms.ElementAt(i).Description, repoGetAcademicPrograms.ElementAt(i).Description);
                    Assert.AreEqual(allAcademicPrograms.ElementAt(i).AcadLevelCode, repoGetAcademicPrograms.ElementAt(i).AcadLevelCode);
                    Assert.AreEqual(allAcademicPrograms.ElementAt(i).CertificateCodes.FirstOrDefault(), repoGetAcademicPrograms.ElementAt(i).CertificateCodes.FirstOrDefault());
                    Assert.AreEqual(allAcademicPrograms.ElementAt(i).DegreeCode, repoGetAcademicPrograms.ElementAt(i).DegreeCode);
                    Assert.AreEqual(allAcademicPrograms.ElementAt(i).DeptartmentCodes.FirstOrDefault(), repoGetAcademicPrograms.ElementAt(i).DeptartmentCodes.FirstOrDefault());
                    Assert.AreEqual(allAcademicPrograms.ElementAt(i).EndDate, repoGetAcademicPrograms.ElementAt(i).EndDate);
                    Assert.AreEqual(allAcademicPrograms.ElementAt(i).FederalCourseClassification, repoGetAcademicPrograms.ElementAt(i).FederalCourseClassification);
                    Assert.AreEqual(allAcademicPrograms.ElementAt(i).Guid, repoGetAcademicPrograms.ElementAt(i).Guid);
                    Assert.AreEqual(allAcademicPrograms.ElementAt(i).HonorCode, repoGetAcademicPrograms.ElementAt(i).HonorCode);
                    Assert.AreEqual(allAcademicPrograms.ElementAt(i).LocalCourseClassifications.FirstOrDefault(), repoGetAcademicPrograms.ElementAt(i).LocalCourseClassifications.FirstOrDefault());
                    Assert.AreEqual(allAcademicPrograms.ElementAt(i).LongDescription, repoGetAcademicPrograms.ElementAt(i).LongDescription);
                    Assert.AreEqual(allAcademicPrograms.ElementAt(i).MajorCodes.FirstOrDefault(), repoGetAcademicPrograms.ElementAt(i).MajorCodes.FirstOrDefault());
                    Assert.AreEqual(allAcademicPrograms.ElementAt(i).MinorCodes.FirstOrDefault(), repoGetAcademicPrograms.ElementAt(i).MinorCodes.FirstOrDefault());
                    Assert.AreEqual(allAcademicPrograms.ElementAt(i).SpecializationCodes.FirstOrDefault(), repoGetAcademicPrograms.ElementAt(i).SpecializationCodes.FirstOrDefault());
                    Assert.AreEqual(allAcademicPrograms.ElementAt(i).StartDate, repoGetAcademicPrograms.ElementAt(i).StartDate);
                }
            }

            [TestMethod]
            public async Task StudentReferenceDataRepo_AcademicPrograms_GetAcademicProgramsAsync_NoCache()
            {
                var repoGetAcademicPrograms = await referenceDataRepo.GetAcademicProgramsAsync(true);
                for (int i = 0; i < allAcademicPrograms.Count(); i++)
                {
                    Assert.AreEqual(allAcademicPrograms.ElementAt(i).Code, repoGetAcademicPrograms.ElementAt(i).Code);
                    Assert.AreEqual(allAcademicPrograms.ElementAt(i).Description, repoGetAcademicPrograms.ElementAt(i).Description);
                    Assert.AreEqual(allAcademicPrograms.ElementAt(i).AcadLevelCode, repoGetAcademicPrograms.ElementAt(i).AcadLevelCode);
                    Assert.AreEqual(allAcademicPrograms.ElementAt(i).CertificateCodes.FirstOrDefault(), repoGetAcademicPrograms.ElementAt(i).CertificateCodes.FirstOrDefault());
                    Assert.AreEqual(allAcademicPrograms.ElementAt(i).DegreeCode, repoGetAcademicPrograms.ElementAt(i).DegreeCode);
                    Assert.AreEqual(allAcademicPrograms.ElementAt(i).DeptartmentCodes.FirstOrDefault(), repoGetAcademicPrograms.ElementAt(i).DeptartmentCodes.FirstOrDefault());
                    Assert.AreEqual(allAcademicPrograms.ElementAt(i).EndDate, repoGetAcademicPrograms.ElementAt(i).EndDate);
                    Assert.AreEqual(allAcademicPrograms.ElementAt(i).FederalCourseClassification, repoGetAcademicPrograms.ElementAt(i).FederalCourseClassification);
                    Assert.AreEqual(allAcademicPrograms.ElementAt(i).Guid, repoGetAcademicPrograms.ElementAt(i).Guid);
                    Assert.AreEqual(allAcademicPrograms.ElementAt(i).HonorCode, repoGetAcademicPrograms.ElementAt(i).HonorCode);
                    Assert.AreEqual(allAcademicPrograms.ElementAt(i).LocalCourseClassifications.FirstOrDefault(), repoGetAcademicPrograms.ElementAt(i).LocalCourseClassifications.FirstOrDefault());
                    Assert.AreEqual(allAcademicPrograms.ElementAt(i).LongDescription, repoGetAcademicPrograms.ElementAt(i).LongDescription);
                    Assert.AreEqual(allAcademicPrograms.ElementAt(i).MajorCodes.FirstOrDefault(), repoGetAcademicPrograms.ElementAt(i).MajorCodes.FirstOrDefault());
                    Assert.AreEqual(allAcademicPrograms.ElementAt(i).MinorCodes.FirstOrDefault(), repoGetAcademicPrograms.ElementAt(i).MinorCodes.FirstOrDefault());
                    Assert.AreEqual(allAcademicPrograms.ElementAt(i).SpecializationCodes.FirstOrDefault(), repoGetAcademicPrograms.ElementAt(i).SpecializationCodes.FirstOrDefault());
                    Assert.AreEqual(allAcademicPrograms.ElementAt(i).StartDate, repoGetAcademicPrograms.ElementAt(i).StartDate);
                }
            }

            private StudentReferenceDataRepository BuildValidReferenceDataRepository()
            {
                // transaction factory mock
                transFactoryMock = new Mock<IColleagueTransactionFactory>();
                // Cache Mock
                //localCacheMock = new Mock<ObjectCache>();
                // Cache Provider Mock
                cacheProviderMock = new Mock<ICacheProvider>();
                // Set up data accessor for mocking 
                dataAccessorMock = new Mock<IColleagueDataReader>();
                apiSettings = new ApiSettings("TEST");

                // Set up dataAccessorMock as the object for the DataAccessor
                transFactoryMock.Setup(transFac => transFac.GetDataReader()).Returns(dataAccessorMock.Object);

                var records = new Collection<DataContracts.AcadPrograms>();
                foreach (var item in allAcademicPrograms)
                {
                    DataContracts.AcadPrograms record = new DataContracts.AcadPrograms();
                    record.RecordGuid = item.Guid;
                    record.AcpgTitle = item.Description;
                    record.Recordkey = item.Code;
                    record.AcpgDegree = item.DegreeCode;
                    record.AcpgHonorsCode = item.HonorCode;
                    record.AcpgAcadLevel = item.AcadLevelCode;
                    record.AcpgCcds = item.CertificateCodes;
                    record.AcpgDepts = new List<string>();
                    record.AcpgLocalGovtCodes = new List<string>();
                    record.AcpgMajors = item.MajorCodes;
                    record.AcpgMinors = item.MinorCodes;
                    record.AcpgSpecializations = item.SpecializationCodes;
                    records.Add(record);
                }
                dataAccessorMock.Setup(acc => acc.BulkReadRecordAsync<DataContracts.AcadPrograms>("ACAD.PROGRAMS", "", true)).ReturnsAsync(records);

                cacheProviderMock.Setup<Task<Tuple<object, SemaphoreSlim>>>(x =>
                 x.GetAndLockSemaphoreAsync(It.IsAny<string>(), null))
                 .ReturnsAsync(new Tuple<object, SemaphoreSlim>(null, new SemaphoreSlim(1, 1)));

                dataAccessorMock.Setup(acc => acc.SelectAsync(It.IsAny<RecordKeyLookup[]>())).Returns<RecordKeyLookup[]>(recordKeyLookups =>
                {
                    var result = new Dictionary<string, RecordKeyLookupResult>();
                    foreach (var recordKeyLookup in recordKeyLookups)
                    {
                        var record = allAcademicPrograms.Where(e => e.Code == recordKeyLookup.PrimaryKey).FirstOrDefault();
                        result.Add(string.Join("+", new string[] { "ACAD.PROGRAMS", record.Code }),
                            new RecordKeyLookupResult() { Guid = record.Guid });
                    }
                    return Task.FromResult(result);
                });

                // Construct repository
                referenceDataRepo = new StudentReferenceDataRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object, apiSettings);

                return referenceDataRepo;
            }
        }

        [TestClass]
        public class AcademicStanding
        {
            Mock<IColleagueTransactionFactory> transFactoryMock;
            Mock<ICacheProvider> cacheProviderMock;
            Mock<IColleagueDataReader> dataAccessorMock;
            Mock<ILogger> loggerMock;
            IEnumerable<Ellucian.Colleague.Domain.Student.Entities.AcademicStanding> allAcademicStandings;
            ApplValcodes academicStandingsValcodeResponse;
            string valcodeName;
            ApiSettings apiSettings;
            StudentReferenceDataRepository referenceDataRepo;

            [TestInitialize]
            public void Initialize()
            {
                loggerMock = new Mock<ILogger>();

                // Build responses used for mocking
                allAcademicStandings = new TestStudentReferenceDataRepository().GetAcademicStandingsAsync().Result;
    
                academicStandingsValcodeResponse = BuildValcodeResponse(allAcademicStandings);

                // Build repository
                referenceDataRepo = BuildValidReferenceDataRepository();
                valcodeName = referenceDataRepo.BuildFullCacheKey("ST_ACAD.STANDINGS");

                cacheProviderMock.Setup<Task<Tuple<object, SemaphoreSlim>>>(x =>
                   x.GetAndLockSemaphoreAsync(It.IsAny<string>(), null))
                   .ReturnsAsync(new Tuple<object, SemaphoreSlim>(null, new SemaphoreSlim(1, 1)));

            }

            [TestCleanup]
            public void Cleanup()
            {
                transFactoryMock = null;
                dataAccessorMock = null;
                cacheProviderMock = null;
                academicStandingsValcodeResponse = null;
                allAcademicStandings = null;
                referenceDataRepo = null;
            }

            [TestMethod]
            public async Task StudentReferenceDataRepo_GetsAcademicStandings()
            {
                var repoAcademicStandings = await referenceDataRepo.GetAcademicStandingsAsync();
                
                for (int i = 0; i < allAcademicStandings.Count(); i++)
                {
                    Assert.AreEqual(allAcademicStandings.ElementAt(i).Code, repoAcademicStandings.ElementAt(i).Code);
                    Assert.AreEqual(allAcademicStandings.ElementAt(i).Description, repoAcademicStandings.ElementAt(i).Description);
                }
            }

            [TestMethod]
            public async Task StudentReferenceDataRepo_GetAcademicStandings_WritesToCache()
            {
                // Set up local cache mock to respond to cache request:
                //  -to "Contains" request, return "false" to indicate item is not in cache
                //  -to cache "Get" request, return null so we know it's reading from the "repository"
                cacheProviderMock.Setup(x => x.Contains(valcodeName, null)).Returns(false);
                cacheProviderMock.Setup(x => x.Get(valcodeName, null)).Returns(null);

                // return a valid response to the data accessor request
                dataAccessorMock.Setup(acc => acc.ReadRecordAsync<ApplValcodes>(It.IsAny<string>(), It.IsAny<string>(), true)).ReturnsAsync(academicStandingsValcodeResponse);

                // But after data accessor read, set up mocking so we can verify the list of academic standings was written to the cache
                cacheProviderMock.Setup(x => x.AddAndUnlockSemaphore(valcodeName, It.IsAny<Object>(), It.IsAny<SemaphoreSlim>(), It.IsAny<CacheItemPolicy>(), null)).Verifiable();

                // Verify that academic standings were returned, which means they came from the "repository".
                Assert.IsTrue((await referenceDataRepo.GetAcademicStandingsAsync()).Count() == allAcademicStandings.Count());

                // Verify that the academic standings was added to the cache after it was read from the repository
                cacheProviderMock.Verify(m => m.AddAndUnlockSemaphore(valcodeName, It.IsAny<Object>(), It.IsAny<SemaphoreSlim>(), It.IsAny<CacheItemPolicy>(), null));
            }

            private T GetCache<T>(ICacheProvider cacheProvider, string key)
                where T : class
            {
                object cache = cacheProvider.Get(key, null);
                return cache as T;
            }

            private StudentReferenceDataRepository BuildValidReferenceDataRepository()
            {
                // transaction factory mock
                transFactoryMock = new Mock<IColleagueTransactionFactory>();
                // Cache Mock
                //localCacheMock = new Mock<ObjectCache>();
                // Cache Provider Mock
                cacheProviderMock = new Mock<ICacheProvider>();
                // Set up data accessor for mocking 
                dataAccessorMock = new Mock<IColleagueDataReader>();
                apiSettings = new ApiSettings("TEST");

                // Set up dataAccessorMock as the object for the DataAccessor
                transFactoryMock.Setup(transFac => transFac.GetDataReader()).Returns(dataAccessorMock.Object);

                // Setup response to academic standings valcode read
                dataAccessorMock.Setup(acc => acc.ReadRecordAsync<ApplValcodes>("ST.VALCODES", "ACAD.STANDINGS", true)).ReturnsAsync(academicStandingsValcodeResponse);

                // Construct repository
                referenceDataRepo = new StudentReferenceDataRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object, apiSettings);

                return referenceDataRepo;
            }

            private ApplValcodes BuildValcodeResponse(IEnumerable<Ellucian.Colleague.Domain.Student.Entities.AcademicStanding> academicStanding)
            {
                ApplValcodes applValcodeResponse = new ApplValcodes();
                applValcodeResponse.ValsEntityAssociation = new List<ApplValcodesVals>();
                foreach (var item in academicStanding)
                {
                    applValcodeResponse.ValsEntityAssociation.Add(new ApplValcodesVals("", item.Description, "", item.Code, "", "", ""));
                }
                return applValcodeResponse;
            }
        }

        [TestClass]
        public class AcademicStandings2
        {
            Mock<IColleagueTransactionFactory> transFactoryMock;
            Mock<ICacheProvider> cacheProviderMock;
            Mock<IColleagueDataReader> dataAccessorMock;
            Mock<ILogger> loggerMock;
            IEnumerable<AcademicStanding2> allAcademicStandings;
            ApplValcodes academicStandingValcodeResponse;
            string valcodeName;
            ApiSettings apiSettings;

            Mock<IStudentReferenceDataRepository> referenceDataRepositoryMock;
            IStudentReferenceDataRepository referenceDataRepository;
            StudentReferenceDataRepository referenceDataRepo;

            [TestInitialize]
            public void Initialize()
            {
                loggerMock = new Mock<ILogger>();
                apiSettings = new ApiSettings("TEST");

                // Build academic standings responses used for mocking
                allAcademicStandings = new TestStudentReferenceDataRepository().GetAcademicStandings2Async(false).Result;
                academicStandingValcodeResponse = BuildValcodeResponse(allAcademicStandings);
                var academicStandingValResponse = new List<string>() { "2" };
                academicStandingValcodeResponse.ValActionCode1 = academicStandingValResponse;

                referenceDataRepositoryMock = new Mock<IStudentReferenceDataRepository>();
                referenceDataRepository = referenceDataRepositoryMock.Object;

                // Build academic standing repository
                referenceDataRepo = BuildValidReferenceDataRepository();
                valcodeName = referenceDataRepo.BuildFullCacheKey("ST_ACAD.STANDINGS_GUID");

                cacheProviderMock.Setup<Task<Tuple<object, SemaphoreSlim>>>(x =>
                   x.GetAndLockSemaphoreAsync(It.IsAny<string>(), null))
                   .ReturnsAsync(new Tuple<object, SemaphoreSlim>(null, new SemaphoreSlim(1, 1)));
            }

            [TestCleanup]
            public void Cleanup()
            {
                transFactoryMock = null;
                dataAccessorMock = null;
                cacheProviderMock = null;
                academicStandingValcodeResponse = null;
                allAcademicStandings = null;
                referenceDataRepo = null;
            }

            [TestMethod]
            public async Task StudentReferenceDataRepo_GetsAcademicStandingsNoArgAsync()
            {
                var academicStandings = await referenceDataRepo.GetAcademicStandings2Async();

                for (int i = 0; i < allAcademicStandings.Count(); i++)
                {
                    Assert.AreEqual(allAcademicStandings.ElementAt(i).Code, academicStandings.ElementAt(i).Code);
                    Assert.AreEqual(allAcademicStandings.ElementAt(i).Description, academicStandings.ElementAt(i).Description);
                }
            }

            [TestMethod]
            public async Task StudentReferenceDataRepo_GetsAcademicStandingsCacheAsync()
            {
                var academicStandings = await referenceDataRepo.GetAcademicStandings2Async(false);

                for (int i = 0; i < allAcademicStandings.Count(); i++)
                {
                    Assert.AreEqual(allAcademicStandings.ElementAt(i).Code, academicStandings.ElementAt(i).Code);
                    Assert.AreEqual(allAcademicStandings.ElementAt(i).Description, academicStandings.ElementAt(i).Description);
                }
            }

            [TestMethod]
            public async Task StudentReferenceDataRepo_GetsAcademicStandingsNonCacheAsync()
            {
                var statuses = await referenceDataRepo.GetAcademicStandings2Async(true);

                for (int i = 0; i < allAcademicStandings.Count(); i++)
                {
                    Assert.AreEqual(allAcademicStandings.ElementAt(i).Code, statuses.ElementAt(i).Code);
                    Assert.AreEqual(allAcademicStandings.ElementAt(i).Description, statuses.ElementAt(i).Description);
                }
            }

            [TestMethod]
            public async Task StudentReferenceDataRepo_GetAcademicStandings_WritesToCacheAsync()
            {

                // Set up local cache mock to respond to cache request:
                //  -to "Contains" request, return "false" to indicate item is not in cache
                //  -to cache "Get" request, return null so we know it's reading from the "repository"
                cacheProviderMock.Setup(x => x.Contains(valcodeName, null)).Returns(false);
                cacheProviderMock.Setup(x => x.Get(valcodeName, null)).Returns(null);

                // return a valid response to the data accessor request
                dataAccessorMock.Setup(acc => acc.ReadRecordAsync<ApplValcodes>("ST.VALCODES", "ACAD.STANDINGS", It.IsAny<bool>())).ReturnsAsync(academicStandingValcodeResponse);

                cacheProviderMock.Setup<Task<Tuple<object, SemaphoreSlim>>>(x =>
                 x.GetAndLockSemaphoreAsync(It.IsAny<string>(), null))
                 .ReturnsAsync(new Tuple<object, SemaphoreSlim>(null, new SemaphoreSlim(1, 1)));

                // But after data accessor read, set up mocking so we can verify the list of academicStandings was written to the cache
                cacheProviderMock.Setup(x => x.Add(It.IsAny<string>(), It.IsAny<Task<List<AcademicStanding2>>>(), It.IsAny<CacheItemPolicy>(), null)).Verifiable();

                cacheProviderMock.Setup(x => x.Contains(referenceDataRepo.BuildFullCacheKey("ST_ACAD.STANDINGS"), null)).Returns(true);
                var academicStandings = await referenceDataRepo.GetAcademicStandings2Async(false);
                cacheProviderMock.Setup(x => x.Get(referenceDataRepo.BuildFullCacheKey("ST_ACAD.STANDINGS"), null)).Returns(academicStandings);
                // Verify that academicStandings were returned, which means they came from the "repository".
                Assert.IsTrue(academicStandings.Count() == 3);

                // Verify that the academicStanding item was added to the cache after it was read from the repository
                cacheProviderMock.Verify(m => m.Add(It.IsAny<string>(), It.IsAny<Task<List<AcademicStanding2>>>(), It.IsAny<CacheItemPolicy>(), null), Times.Never);

            }

            [TestMethod]
            public async Task StudentReferenceDataRepo_GetAcademicStandings_GetsCachedAcademicStandingsAsync()
            {
                // Set up local cache mock to respond to cache request:
                //  -to "Contains" request, return "true" to indicate item is in cache
                //  -to "Get" request, return the cache item (in this case the "ACAD.STANDINGS" cache item)
                cacheProviderMock.Setup(x => x.Contains(valcodeName, null)).Returns(true);
                cacheProviderMock.Setup(x => x.Get(valcodeName, null)).Returns(allAcademicStandings).Verifiable();

                // return null for request, so that if we have a result, it wasn't the data accessor that returned it.
                dataAccessorMock.Setup(acc => acc.ReadRecordAsync<ApplValcodes>("ST.VALCODES", "ACAD.STANDINGS", true)).ReturnsAsync(new ApplValcodes());

                // Assert the academicStandings are returned
                Assert.IsTrue((await referenceDataRepo.GetAcademicStandings2Async(false)).Count() == 3);
                // Verify that the sacademicStandings were retrieved from cache
                cacheProviderMock.Verify(m => m.Get(valcodeName, null));
            }

            private StudentReferenceDataRepository BuildValidReferenceDataRepository()
            {
                // transaction factory mock
                transFactoryMock = new Mock<IColleagueTransactionFactory>();
                // Cache Provider Mock
                cacheProviderMock = new Mock<ICacheProvider>();
                // Set up data accessor for mocking 
                dataAccessorMock = new Mock<IColleagueDataReader>();

                // Set up dataAccessorMock as the object for the DataAccessor
                transFactoryMock.Setup(transFac => transFac.GetDataReader()).Returns(dataAccessorMock.Object);

                // Setup response to academicStanding valcode read
                dataAccessorMock.Setup(acc => acc.ReadRecordAsync<ApplValcodes>("ST.VALCODES", "ACAD.STANDINGS", It.IsAny<bool>())).ReturnsAsync(academicStandingValcodeResponse);
                cacheProviderMock.Setup<Task<Tuple<object, SemaphoreSlim>>>(x => x.GetAndLockSemaphoreAsync(It.IsAny<string>(), null))
                .ReturnsAsync(new Tuple<object, SemaphoreSlim>(null, new SemaphoreSlim(1, 1)));

                dataAccessorMock.Setup(acc => acc.SelectAsync(It.IsAny<RecordKeyLookup[]>())).Returns<RecordKeyLookup[]>(recordKeyLookups =>
                {
                    var result = new Dictionary<string, RecordKeyLookupResult>();
                    foreach (var recordKeyLookup in recordKeyLookups)
                    {
                        var academicStanding = allAcademicStandings.Where(e => e.Code == recordKeyLookup.SecondaryKey).FirstOrDefault();
                        result.Add(string.Join("+", new string[] { "ST.VALCODES", "ACAD.STANDINGS", academicStanding.Code }),
                            new RecordKeyLookupResult() { Guid = academicStanding.Guid });
                    }
                    return Task.FromResult(result);
                });

                // Construct repository
                referenceDataRepo = new StudentReferenceDataRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object, apiSettings);

                return referenceDataRepo;
            }

            private ApplValcodes BuildValcodeResponse(IEnumerable<AcademicStanding2> academicStandings)
            {
                ApplValcodes academicStandingsResponse = new ApplValcodes();
                academicStandingsResponse.ValsEntityAssociation = new List<ApplValcodesVals>();
                foreach (var item in academicStandings)
                {
                    academicStandingsResponse.ValsEntityAssociation.Add(new ApplValcodesVals("", item.Description, "2", item.Code, "3", "", ""));
                }
                return academicStandingsResponse;
            }
        }

        [TestClass]
        public class AccountingCodes
        {
            Mock<IColleagueTransactionFactory> transFactoryMock;
            Mock<ICacheProvider> cacheProviderMock;
            Mock<IColleagueDataReader> dataAccessorMock;
            Mock<ILogger> loggerMock;

            IEnumerable<AccountingCode> allAccountingCodes;
            string valcodeName;
            ApiSettings apiSettings;

            StudentReferenceDataRepository referenceDataRepo;

            [TestInitialize]
            public void Initialize()
            {
                loggerMock = new Mock<ILogger>();
                apiSettings = new ApiSettings("TEST");

                allAccountingCodes = new TestAccountingCodesRepository().Get();

                referenceDataRepo = BuildValidReferenceDataRepository();
                valcodeName = referenceDataRepo.BuildFullCacheKey("AllAccountingCodes");
            }

            [TestCleanup]
            public void Cleanup()
            {
                allAccountingCodes = null;
                valcodeName = string.Empty;
                apiSettings = null;
            }

            [TestMethod]
            public async Task StudentReferenceDataRepository_GetAccountingCodesAsync_False()
            {
                var results = await referenceDataRepo.GetAccountingCodesAsync(false);
                Assert.AreEqual(allAccountingCodes.Count(), results.Count());

                foreach (var accountingCode in allAccountingCodes)
                {
                    var result = results.FirstOrDefault(i => i.Guid == accountingCode.Guid);

                    Assert.AreEqual(accountingCode.Code, result.Code);
                    Assert.AreEqual(accountingCode.Description, result.Description);
                    Assert.AreEqual(accountingCode.Guid, result.Guid);
                }

            }

            [TestMethod]
            public async Task StudentReferenceDataRepository_GetAccountingCodesAsync_True()
            {
                var results = await referenceDataRepo.GetAccountingCodesAsync(true);
                Assert.AreEqual(allAccountingCodes.Count(), results.Count());

                foreach (var accountingCode in allAccountingCodes)
                {
                    var result = results.FirstOrDefault(i => i.Guid == accountingCode.Guid);

                    Assert.AreEqual(accountingCode.Code, result.Code);
                    Assert.AreEqual(accountingCode.Description, result.Description);
                    Assert.AreEqual(accountingCode.Guid, result.Guid);
                }

            }

            private StudentReferenceDataRepository BuildValidReferenceDataRepository()
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

                var records = new Collection<DataContracts.ArCodes>();
                foreach (var item in allAccountingCodes)
                {
                    DataContracts.ArCodes record = new DataContracts.ArCodes();
                    record.RecordGuid = item.Guid;
                    record.ArcDesc = item.Description;
                    record.Recordkey = item.Code;
                    records.Add(record);
                }
                dataAccessorMock.Setup(acc => acc.BulkReadRecordAsync<DataContracts.ArCodes>("AR.CODES", "", true)).ReturnsAsync(records);

                cacheProviderMock.Setup<Task<Tuple<object, SemaphoreSlim>>>(x =>
                 x.GetAndLockSemaphoreAsync(It.IsAny<string>(), null))
                 .ReturnsAsync(new Tuple<object, SemaphoreSlim>(null, new SemaphoreSlim(1, 1)));

                dataAccessorMock.Setup(acc => acc.SelectAsync(It.IsAny<RecordKeyLookup[]>())).Returns<RecordKeyLookup[]>(recordKeyLookups =>
                {
                    var result = new Dictionary<string, RecordKeyLookupResult>();
                    foreach (var recordKeyLookup in recordKeyLookups)
                    {
                        var record = allAccountingCodes.Where(e => e.Code == recordKeyLookup.PrimaryKey).FirstOrDefault();
                        result.Add(string.Join("+", new string[] { "AR.CODES", record.Code }),
                            new RecordKeyLookupResult() { Guid = record.Guid });
                    }
                    return Task.FromResult(result);
                });

                // Construct repository
                referenceDataRepo = new StudentReferenceDataRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object, apiSettings);

                return referenceDataRepo;
            }
        }

        [TestClass]
        public class AccountingCodeCategories
        {
            Mock<IColleagueTransactionFactory> transFactoryMock;
            Mock<ICacheProvider> cacheProviderMock;
            Mock<IColleagueDataReader> dataAccessorMock;
            Mock<ILogger> loggerMock;

            IEnumerable<ArCategory> allAccountingCategories;
            string valcodeName;
            ApiSettings apiSettings;

            StudentReferenceDataRepository referenceDataRepo;

            [TestInitialize]
            public void Initialize()
            {
                loggerMock = new Mock<ILogger>();
                apiSettings = new ApiSettings("TEST");

                allAccountingCategories = new TestAccountingCodeCategoriesRepository().Get();

                referenceDataRepo = BuildValidReferenceDataRepository();
                valcodeName = referenceDataRepo.BuildFullCacheKey("AllEEDMArCategories");
            }

            [TestCleanup]
            public void Cleanup()
            {
                allAccountingCategories = null;
                valcodeName = string.Empty;
                apiSettings = null;
            }

            [TestMethod]
            public async Task StudentReferenceDataRepository_GetAccountingCodesAsync_False()
            {
                var results = await referenceDataRepo.GetArCategoriesAsync(false);
                Assert.AreEqual(allAccountingCategories.Count(), results.Count());

                foreach (var accountingCode in allAccountingCategories)
                {
                    var result = results.FirstOrDefault(i => i.Guid == accountingCode.Guid);

                    Assert.AreEqual(accountingCode.Code, result.Code);
                    Assert.AreEqual(accountingCode.Description, result.Description);
                    Assert.AreEqual(accountingCode.Guid, result.Guid);
                }

            }

            [TestMethod]
            public async Task StudentReferenceDataRepository_GetAccountingCodesAsync_True()
            {
                var results = await referenceDataRepo.GetArCategoriesAsync(true);
                Assert.AreEqual(allAccountingCategories.Count(), results.Count());

                foreach (var accountingCode in allAccountingCategories)
                {
                    var result = results.FirstOrDefault(i => i.Guid == accountingCode.Guid);

                    Assert.AreEqual(accountingCode.Code, result.Code);
                    Assert.AreEqual(accountingCode.Description, result.Description);
                    Assert.AreEqual(accountingCode.Guid, result.Guid);
                }

            }

            private StudentReferenceDataRepository BuildValidReferenceDataRepository()
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

                var records = new Collection<DataContracts.ArCategories>();
                foreach (var item in allAccountingCategories)
                {
                    DataContracts.ArCategories record = new DataContracts.ArCategories();
                    record.RecordGuid = item.Guid;
                    record.ArctDesc = item.Description;
                    record.Recordkey = item.Code;
                    records.Add(record);
                }
                dataAccessorMock.Setup(acc => acc.BulkReadRecordAsync<DataContracts.ArCategories>("AR.CATEGORIES", "", true)).ReturnsAsync(records);

                cacheProviderMock.Setup<Task<Tuple<object, SemaphoreSlim>>>(x =>
                 x.GetAndLockSemaphoreAsync(It.IsAny<string>(), null))
                 .ReturnsAsync(new Tuple<object, SemaphoreSlim>(null, new SemaphoreSlim(1, 1)));

                dataAccessorMock.Setup(acc => acc.SelectAsync(It.IsAny<RecordKeyLookup[]>())).Returns<RecordKeyLookup[]>(recordKeyLookups =>
                {
                    var result = new Dictionary<string, RecordKeyLookupResult>();
                    foreach (var recordKeyLookup in recordKeyLookups)
                    {
                        var record = allAccountingCategories.Where(e => e.Code == recordKeyLookup.PrimaryKey).FirstOrDefault();
                        result.Add(string.Join("+", new string[] { "AR.CATEGORIES", record.Code }),
                            new RecordKeyLookupResult() { Guid = record.Guid });
                    }
                    return Task.FromResult(result);
                });

                // Construct repository
                referenceDataRepo = new StudentReferenceDataRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object, apiSettings);

                return referenceDataRepo;
            }
        }

        [TestClass]
        public class AccountReceivableTypes
        {
            Mock<IColleagueTransactionFactory> transFactoryMock;
            Mock<ICacheProvider> cacheProviderMock;
            Mock<IColleagueDataReader> dataAccessorMock;
            Mock<ILogger> loggerMock;

            IEnumerable<AccountReceivableType> allAccountReceivableTypes;
            string valcodeName;
            ApiSettings apiSettings;

            StudentReferenceDataRepository referenceDataRepo;

            [TestInitialize]
            public void Initialize()
            {
                loggerMock = new Mock<ILogger>();
                apiSettings = new ApiSettings("TEST");

                allAccountReceivableTypes = new TestAccountReceivableTypeRepository().Get();

                referenceDataRepo = BuildValidReferenceDataRepository();
                valcodeName = referenceDataRepo.BuildFullCacheKey("AllAccountReceivableTypes");
            }

            [TestCleanup]
            public void Cleanup()
            {
                allAccountReceivableTypes = null;
                valcodeName = string.Empty;
                apiSettings = null;
            }

            [TestMethod]
            public async Task StudentReferenceDataRepository_GetAccountReceivableTypesAsync_False()
            {
                var results = await referenceDataRepo.GetAccountReceivableTypesAsync(false);
                Assert.AreEqual(allAccountReceivableTypes.Count(), results.Count());

                foreach (var accountReceivableType in allAccountReceivableTypes)
                {
                    var result = results.FirstOrDefault(i => i.Guid == accountReceivableType.Guid);

                    Assert.AreEqual(accountReceivableType.Code, result.Code);
                    Assert.AreEqual(accountReceivableType.Description, result.Description);
                    Assert.AreEqual(accountReceivableType.Guid, result.Guid);
                }

            }

            [TestMethod]
            public async Task StudentReferenceDataRepository_GetAccountReceivableTypesAsync_True()
            {
                var results = await referenceDataRepo.GetAccountReceivableTypesAsync(true);
                Assert.AreEqual(allAccountReceivableTypes.Count(), results.Count());

                foreach (var accountReceivableType in allAccountReceivableTypes)
                {
                    var result = results.FirstOrDefault(i => i.Guid == accountReceivableType.Guid);

                    Assert.AreEqual(accountReceivableType.Code, result.Code);
                    Assert.AreEqual(accountReceivableType.Description, result.Description);
                    Assert.AreEqual(accountReceivableType.Guid, result.Guid);
                }

            }

            private StudentReferenceDataRepository BuildValidReferenceDataRepository()
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

                var records = new Collection<DataContracts.ArTypes>();
                foreach (var item in allAccountReceivableTypes)
                {
                    DataContracts.ArTypes record = new DataContracts.ArTypes();
                    record.RecordGuid = item.Guid;
                    record.ArtDesc = item.Description;
                    record.Recordkey = item.Code;
                    records.Add(record);
                }
                dataAccessorMock.Setup(acc => acc.BulkReadRecordAsync<DataContracts.ArTypes>("AR.TYPES", "", true)).ReturnsAsync(records);

                cacheProviderMock.Setup<Task<Tuple<object, SemaphoreSlim>>>(x =>
                 x.GetAndLockSemaphoreAsync(It.IsAny<string>(), null))
                 .ReturnsAsync(new Tuple<object, SemaphoreSlim>(null, new SemaphoreSlim(1, 1)));

                dataAccessorMock.Setup(acc => acc.SelectAsync(It.IsAny<RecordKeyLookup[]>())).Returns<RecordKeyLookup[]>(recordKeyLookups =>
                {
                    var result = new Dictionary<string, RecordKeyLookupResult>();
                    foreach (var recordKeyLookup in recordKeyLookups)
                    {
                        var record = allAccountReceivableTypes.Where(e => e.Code == recordKeyLookup.PrimaryKey).FirstOrDefault();
                        result.Add(string.Join("+", new string[] { "AR.TYPES", record.Code }),
                            new RecordKeyLookupResult() { Guid = record.Guid });
                    }
                    return Task.FromResult(result);
                });

                // Construct repository
                referenceDataRepo = new StudentReferenceDataRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object, apiSettings);

                return referenceDataRepo;
            }
        }

        [TestClass]
        public class AccountReceivableDepositTypes
        {
            Mock<IColleagueTransactionFactory> transFactoryMock;
            Mock<ICacheProvider> cacheProviderMock;
            Mock<IColleagueDataReader> dataAccessorMock;
            Mock<ILogger> loggerMock;

            IEnumerable<AccountReceivableDepositType> allAccountReceivableDepositTypes;
            string valcodeName;
            ApiSettings apiSettings;

            StudentReferenceDataRepository referenceDataRepo;

            [TestInitialize]
            public void Initialize()
            {
                loggerMock = new Mock<ILogger>();
                apiSettings = new ApiSettings("TEST");

                allAccountReceivableDepositTypes = new TestAccountReceivableDepositTypeRepository().Get();

                referenceDataRepo = BuildValidReferenceDataRepository();
                valcodeName = referenceDataRepo.BuildFullCacheKey("AllEEDMAccountReceivableDepositTypes");
            }

            [TestCleanup]
            public void Cleanup()
            {
                allAccountReceivableDepositTypes = null;
                valcodeName = string.Empty;
                apiSettings = null;
            }

            [TestMethod]
            public async Task StudentReferenceDataRepository_GetAccountReceivableDepositTypesAsync_False()
            {
                var results = await referenceDataRepo.GetAccountReceivableDepositTypesAsync(false);
                Assert.AreEqual(allAccountReceivableDepositTypes.Count(), results.Count());

                foreach (var accountReceivableDepositType in allAccountReceivableDepositTypes)
                {
                    var result = results.FirstOrDefault(i => i.Guid == accountReceivableDepositType.Guid);

                    Assert.AreEqual(accountReceivableDepositType.Code, result.Code);
                    Assert.AreEqual(accountReceivableDepositType.Description, result.Description);
                    Assert.AreEqual(accountReceivableDepositType.Guid, result.Guid);
                }

            }

            [TestMethod]
            public async Task StudentReferenceDataRepository_GetAccountReceivableDepositTypesAsync_True()
            {
                var results = await referenceDataRepo.GetAccountReceivableDepositTypesAsync(true);
                Assert.AreEqual(allAccountReceivableDepositTypes.Count(), results.Count());

                foreach (var accountReceivableDepositType in allAccountReceivableDepositTypes)
                {
                    var result = results.FirstOrDefault(i => i.Guid == accountReceivableDepositType.Guid);

                    Assert.AreEqual(accountReceivableDepositType.Code, result.Code);
                    Assert.AreEqual(accountReceivableDepositType.Description, result.Description);
                    Assert.AreEqual(accountReceivableDepositType.Guid, result.Guid);
                }

            }

            private StudentReferenceDataRepository BuildValidReferenceDataRepository()
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

                var records = new Collection<DataContracts.ArDepositTypes>();
                foreach (var item in allAccountReceivableDepositTypes)
                {
                    DataContracts.ArDepositTypes record = new DataContracts.ArDepositTypes();
                    record.RecordGuid = item.Guid;
                    record.ArdtDesc = item.Description;
                    record.Recordkey = item.Code;
                    records.Add(record);
                }
                dataAccessorMock.Setup(acc => acc.BulkReadRecordAsync<DataContracts.ArDepositTypes>("AR.DEPOSIT.TYPES", "", true)).ReturnsAsync(records);

                cacheProviderMock.Setup<Task<Tuple<object, SemaphoreSlim>>>(x =>
                 x.GetAndLockSemaphoreAsync(It.IsAny<string>(), null))
                 .ReturnsAsync(new Tuple<object, SemaphoreSlim>(null, new SemaphoreSlim(1, 1)));

                dataAccessorMock.Setup(acc => acc.SelectAsync(It.IsAny<RecordKeyLookup[]>())).Returns<RecordKeyLookup[]>(recordKeyLookups =>
                {
                    var result = new Dictionary<string, RecordKeyLookupResult>();
                    foreach (var recordKeyLookup in recordKeyLookups)
                    {
                        var record = allAccountReceivableDepositTypes.Where(e => e.Code == recordKeyLookup.PrimaryKey).FirstOrDefault();
                        result.Add(string.Join("+", new string[] { "AR.DEPOSIT.TYPES", record.Code }),
                            new RecordKeyLookupResult() { Guid = record.Guid });
                    }
                    return Task.FromResult(result);
                });

                // Construct repository
                referenceDataRepo = new StudentReferenceDataRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object, apiSettings);

                return referenceDataRepo;
            }
        }

        #region Admission Application Types
        /// <summary>
        /// Test class for AdmissionApplicationTypes
        /// </summary>
        [TestClass]
        public class AdmissionApplicationTypesTests
        {
            Mock<IColleagueTransactionFactory> transFactoryMock;
            Mock<ICacheProvider> cacheProviderMock;
            Mock<IColleagueDataReader> dataAccessorMock;
            Mock<ILogger> loggerMock;
            IEnumerable<AdmissionApplicationType> allAdmissionApplicationTypes;
            ApplValcodes intgAdmissionApplicationTypes;
            string valcodeName;
            ApiSettings apiSettings;

            StudentReferenceDataRepository referenceDataRepo;

            [TestInitialize]
            public void Initialize()
            {
                loggerMock = new Mock<ILogger>();
                apiSettings = new ApiSettings("TEST");

                // Build responses used for mocking
                allAdmissionApplicationTypes = new TestStudentReferenceDataRepository().GetAdmissionApplicationTypesAsync(false).Result;
                intgAdmissionApplicationTypes = BuildValcodeResponse(allAdmissionApplicationTypes);

                // Build repository
                referenceDataRepo = BuildValidReferenceDataRepository();
                valcodeName = referenceDataRepo.BuildFullCacheKey("ST_ADMISSION_APPLICATION_TYPES_GUID");

                cacheProviderMock.Setup<Task<Tuple<object, SemaphoreSlim>>>(x =>
                  x.GetAndLockSemaphoreAsync(It.IsAny<string>(), null))
                  .ReturnsAsync(new Tuple<object, SemaphoreSlim>(null, new SemaphoreSlim(1, 1)));
            }

            [TestCleanup]
            public void Cleanup()
            {
                transFactoryMock = null;
                dataAccessorMock = null;
                cacheProviderMock = null;
                allAdmissionApplicationTypes = null;
                referenceDataRepo = null;
                intgAdmissionApplicationTypes = null;
                apiSettings = null;
            }

            [TestMethod]
            public async Task GetsAdmissionApplicationTypesCacheAsync()
            {
                var admissionApplicationTypes = await referenceDataRepo.GetAdmissionApplicationTypesAsync(false);

                for (int i = 0; i < allAdmissionApplicationTypes.Count(); i++)
                {
                    Assert.AreEqual(allAdmissionApplicationTypes.ElementAt(i).Guid, admissionApplicationTypes.ElementAt(i).Guid);
                    Assert.AreEqual(allAdmissionApplicationTypes.ElementAt(i).Code, admissionApplicationTypes.ElementAt(i).Code);
                    Assert.AreEqual(allAdmissionApplicationTypes.ElementAt(i).Description, admissionApplicationTypes.ElementAt(i).Description);

                }
            }

            [TestMethod]
            public async Task GetsAdmissionApplicationTypesNonCacheAsync()
            {
                var admissionApplicationTypes = await referenceDataRepo.GetAdmissionApplicationTypesAsync(true);

                for (int i = 0; i < allAdmissionApplicationTypes.Count(); i++)
                {
                    Assert.AreEqual(allAdmissionApplicationTypes.ElementAt(i).Guid, admissionApplicationTypes.ElementAt(i).Guid);
                    Assert.AreEqual(allAdmissionApplicationTypes.ElementAt(i).Code, admissionApplicationTypes.ElementAt(i).Code);
                    Assert.AreEqual(allAdmissionApplicationTypes.ElementAt(i).Description, admissionApplicationTypes.ElementAt(i).Description);

                }
            }

            [TestMethod]
            public async Task GetsAdmissionApplicationTypesWritesToCacheAsync()
            {

                // Set up local cache mock to respond to cache request:
                //  -to "Contains" request, return "false" to indicate item is not in cache
                //  -to cache "Get" request, return null so we know it's reading from the "repository"
                cacheProviderMock.Setup(x => x.Contains(valcodeName, null)).Returns(false);
                cacheProviderMock.Setup(x => x.Get(valcodeName, null)).Returns(null);

                // return a valid response to the data accessor request
                dataAccessorMock.Setup(acc => acc.ReadRecordAsync<ApplValcodes>("ST.VALCODES", "INTG.APPLICATION.TYPES", It.IsAny<bool>())).ReturnsAsync(intgAdmissionApplicationTypes);

                cacheProviderMock.Setup<Task<Tuple<object, SemaphoreSlim>>>(x =>
                 x.GetAndLockSemaphoreAsync(It.IsAny<string>(), null))
                 .ReturnsAsync(new Tuple<object, SemaphoreSlim>(null, new SemaphoreSlim(1, 1)));

                // But after data accessor read, set up mocking so we can verify the list of vendor hold reasons was written to the cache
                cacheProviderMock.Setup(x => x.Add(It.IsAny<string>(), It.IsAny<Task<List<AdmissionApplicationType>>>(), It.IsAny<CacheItemPolicy>(), null)).Verifiable();

                cacheProviderMock.Setup(x => x.Contains(referenceDataRepo.BuildFullCacheKey("INTG.APPLICATION.TYPES"), null)).Returns(true);
                var admissionApplicationTypes = await referenceDataRepo.GetAdmissionApplicationTypesAsync(false);
                cacheProviderMock.Setup(x => x.Get(referenceDataRepo.BuildFullCacheKey("INTG.APPLICATION.TYPES"), null)).Returns(admissionApplicationTypes);
                // Verify that admissionApplicationTypes were returned, which means they came from the "repository".
                Assert.IsTrue(admissionApplicationTypes.Count() == 1);

                // Verify that the admissionApplicationTypes item was added to the cache after it was read from the repository
                cacheProviderMock.Verify(m => m.Add(It.IsAny<string>(), It.IsAny<Task<List<AdmissionApplicationType>>>(), It.IsAny<CacheItemPolicy>(), null), Times.Never);

            }

            //[TestMethod]
            //public async Task GetsAdmissionApplicationTypesGetsCachedAsync()
            //{
            //    // Set up local cache mock to respond to cache request:
            //    //  -to "Contains" request, return "true" to indicate item is in cache
            //    //  -to "Get" request, return the cache item (in this case the "INTG.APPLICATION.TYPES" cache item)
            //    cacheProviderMock.Setup(x => x.Contains(valcodeName, null)).Returns(true);
            //    cacheProviderMock.Setup(x => x.Get(valcodeName, null)).Returns(allAdmissionApplicationTypes).Verifiable();

            //    // return null for request, so that if we have a result, it wasn't the data accessor that returned it.
            //    dataAccessorMock.Setup(acc => acc.ReadRecordAsync<ApplValcodes>("ST.VALCODES", "INTG.APPLICATION.TYPES", true)).ReturnsAsync(new ApplValcodes());

            //    // Assert the admissionApplicationTypes are returned
            //    var actual = await referenceDataRepo.GetAdmissionApplicationTypesAsync(false);
            //    Assert.IsTrue(actual.Count() == 1);
            //    // Verify that the admissionApplicationTypes were retrieved from cache
            //    cacheProviderMock.Verify(m => m.Get(valcodeName, null));
            //}
            
            private ApplValcodes BuildValcodeResponse(IEnumerable<AdmissionApplicationType> admissionApplicationTypes)
            {
                ApplValcodes admissionApplicationTypesResponse = new ApplValcodes();
                admissionApplicationTypesResponse.ValsEntityAssociation = new List<ApplValcodesVals>();
                foreach (var item in admissionApplicationTypes)
                {
                    admissionApplicationTypesResponse.ValsEntityAssociation.Add(new ApplValcodesVals("", item.Description, "2", item.Code, "3", "", ""));
                }
                return admissionApplicationTypesResponse;
            }

            private StudentReferenceDataRepository BuildValidReferenceDataRepository()
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

                // Setup response to admissionApplicationTypes valcode read
                dataAccessorMock.Setup(acc => acc.ReadRecordAsync<ApplValcodes>("ST.VALCODES", "INTG.APPLICATION.TYPES", It.IsAny<bool>())).ReturnsAsync(intgAdmissionApplicationTypes);
                cacheProviderMock.Setup<Task<Tuple<object, SemaphoreSlim>>>(x => x.GetAndLockSemaphoreAsync(It.IsAny<string>(), null))
                .ReturnsAsync(new Tuple<object, SemaphoreSlim>(null, new SemaphoreSlim(1, 1)));

                dataAccessorMock.Setup(acc => acc.SelectAsync(It.IsAny<RecordKeyLookup[]>())).Returns<RecordKeyLookup[]>(recordKeyLookups =>
                {
                    var result = new Dictionary<string, RecordKeyLookupResult>();
                    foreach (var recordKeyLookup in recordKeyLookups)
                    {
                        var admissionApplicationType = allAdmissionApplicationTypes.Where(e => e.Code == recordKeyLookup.SecondaryKey).FirstOrDefault();
                        result.Add(string.Join("+", new string[] { "ST.VALCODES", "INTG.APPLICATION.TYPES", admissionApplicationType.Code }),
                            new RecordKeyLookupResult() { Guid = admissionApplicationType.Guid });
                    }
                    return Task.FromResult(result);
                });

                // Construct repository
                //referenceDataRepo = new StudentReferenceDataRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object);
                referenceDataRepo = new StudentReferenceDataRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object, apiSettings);

                return referenceDataRepo;
            }
        }
        #endregion

        #region Admission Application Status Types

        /// <summary>
        /// Test class for ApplicationStatuses codes
        /// </summary>
        [TestClass]
        public class ApplicationStatusesTests
        {
            Mock<IColleagueTransactionFactory> transFactoryMock;
            Mock<ICacheProvider> cacheProviderMock;
            Mock<IColleagueDataReader> dataAccessorMock;
            Mock<ILogger> loggerMock;
            ApiSettings apiSettings;
            //IEnumerable<Ellucian.Colleague.Domain.Student.Entities.AdmissionDecisionType> _applicationStatusesCollection;
            string codeItemName;
            Collection<DataContracts.ApplicationStatuses> _appStatusesCollection;

            StudentReferenceDataRepository referenceDataRepo;

            [TestInitialize]
            public void Initialize()
            {
                loggerMock = new Mock<ILogger>();
                apiSettings = new ApiSettings("TEST");

                // Build responses used for mocking
                //_applicationStatusesCollection = new List<Ellucian.Colleague.Domain.Student.Entities.AdmissionDecisionType>()
                //{
                //    new Ellucian.Colleague.Domain.Student.Entities.AdmissionDecisionType("7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc", "AT", "Athletic"),
                //    new Ellucian.Colleague.Domain.Student.Entities.AdmissionDecisionType("849e6a7c-6cd4-4f98-8a73-ab0aa3627f0d", "AC", "Academic"),
                //    new Ellucian.Colleague.Domain.Student.Entities.AdmissionDecisionType("d2253ac7-9931-4560-b42f-1fccd43c952e", "CU", "Cultural"),
                //    new Ellucian.Colleague.Domain.Student.Entities.AdmissionDecisionType("a49e6a7c-6cd4-4f98-8a73-ab0aa3627f0d", "MS", "Academic"),
                //    new Ellucian.Colleague.Domain.Student.Entities.AdmissionDecisionType("b49e6a7c-6cd4-4f98-8a73-ab0aa3627f0e", "ABC", "Academic"),
                //    new Ellucian.Colleague.Domain.Student.Entities.AdmissionDecisionType("c49e6a7c-6cd4-4f98-8a73-ab0aa3627f0f", "DEF", "Academic")

                //};

                // Build repository
                referenceDataRepo = BuildValidReferenceDataRepository();
                //codeItemName = referenceDataRepo.BuildFullCacheKey("AllApplicationStatuses");
                codeItemName = referenceDataRepo.BuildFullCacheKey("AllAdmissionDecisions");
            }

            [TestCleanup]
            public void Cleanup()
            {
                transFactoryMock = null;
                dataAccessorMock = null;
                cacheProviderMock = null;
               // _applicationStatusesCollection = null;
                referenceDataRepo = null;
            }


            [TestMethod]
            public async Task GetsApplicationStatusesCacheAsync()
            {
                var result = await referenceDataRepo.GetAdmissionDecisionTypesAsync(It.IsAny<bool>());

                for (int i = 0; i < _appStatusesCollection.Count(); i++)
                {
                    Assert.AreEqual(_appStatusesCollection.ElementAt(i).RecordGuid, result.ElementAt(i).Guid);
                    Assert.AreEqual(_appStatusesCollection.ElementAt(i).Recordkey, result.ElementAt(i).Code);
                    Assert.AreEqual(_appStatusesCollection.ElementAt(i).AppsDesc, result.ElementAt(i).Description);
                }
            }

            [TestMethod]
            public async Task GetsApplicationStatusesNonCacheAsync()
            {
                var result = await referenceDataRepo.GetAdmissionDecisionTypesAsync(true);

                for (int i = 0; i < _appStatusesCollection.Count(); i++)
                {
                    Assert.AreEqual(_appStatusesCollection.ElementAt(i).RecordGuid, result.ElementAt(i).Guid);
                    Assert.AreEqual(_appStatusesCollection.ElementAt(i).Recordkey, result.ElementAt(i).Code);
                    Assert.AreEqual(_appStatusesCollection.ElementAt(i).AppsDesc, result.ElementAt(i).Description);
                }
            }

            [TestMethod]
            public async Task GetsApplicationStatusesCache2Async()
            {
                var result = await referenceDataRepo.GetAdmissionDecisionTypesAsync(It.IsAny<bool>());

                for (int i = 0; i < _appStatusesCollection.Count(); i++)
                {
                    Assert.AreEqual(_appStatusesCollection.ElementAt(i).RecordGuid, result.ElementAt(i).Guid);
                    Assert.AreEqual(_appStatusesCollection.ElementAt(i).Recordkey, result.ElementAt(i).Code);
                    Assert.AreEqual(_appStatusesCollection.ElementAt(i).AppsDesc, result.ElementAt(i).Description);
                }
            }

            [TestMethod]
            public async Task GetsApplicationStatusesNonCache2Async()
            {
                var result = await referenceDataRepo.GetAdmissionDecisionTypesAsync(true);

                for (int i = 0; i < _appStatusesCollection.Count(); i++)
                {
                    Assert.AreEqual(_appStatusesCollection.ElementAt(i).RecordGuid, result.ElementAt(i).Guid);
                    Assert.AreEqual(_appStatusesCollection.ElementAt(i).Recordkey, result.ElementAt(i).Code);
                    Assert.AreEqual(_appStatusesCollection.ElementAt(i).AppsDesc, result.ElementAt(i).Description);
                }
            }

            private StudentReferenceDataRepository BuildValidReferenceDataRepository()
            {
                

                // transaction factory mock
                transFactoryMock = new Mock<IColleagueTransactionFactory>();
                // Cache Provider Mock
                cacheProviderMock = new Mock<ICacheProvider>();
                // Set up data accessor for mocking 
                dataAccessorMock = new Mock<IColleagueDataReader>();

                // Set up dataAccessorMock as the object for the DataAccessor
                transFactoryMock.Setup(transFac => transFac.GetDataReader()).Returns(dataAccessorMock.Object);


                string id, guid, id2, guid2, id3, guid3, sid, sid2, sid3;
                GuidLookup guidLookup;
                GuidLookupResult guidLookupResult;
                Dictionary<string, GuidLookupResult> guidLookupDict;
                RecordKeyLookup recordLookup;
                RecordKeyLookupResult recordLookupResult;
                Dictionary<string, RecordKeyLookupResult> recordLookupDict;

                // Set up for GUID lookups
                id = "1";
                id2 = "2";
                id3 = "3";

                // Secondary keys for GUID lookups
                sid = "11";
                sid2 = "22";
                sid3 = "33";

                guid = "F5FC5310-17F1-49FC-926D-CC6E3DA6DAEA".ToLowerInvariant();
                guid2 = "5B35075D-14FB-45F7-858A-83F4174B76EA".ToLowerInvariant();
                guid3 = "246E16D9-8790-4D7E-ACA1-D5B1CB9D4A24".ToLowerInvariant();

                guidLookup = new GuidLookup(guid);
                guidLookupResult = new GuidLookupResult() { Entity = "APPLICATION.STATUSES", PrimaryKey = id, SecondaryKey = sid };
                guidLookupDict = new Dictionary<string, GuidLookupResult>();
                recordLookup = new RecordKeyLookup("APPLICATION.STATUSES", id, "APP.INTG.ADM.DEC.TYP.IDX", sid, false);
                recordLookupResult = new RecordKeyLookupResult() { Guid = guid };
                recordLookupDict = new Dictionary<string, RecordKeyLookupResult>();
                guidLookupDict.Add(guid, new GuidLookupResult() { Entity = "APPLICATION.STATUSES", PrimaryKey = id, SecondaryKey = sid });
                guidLookupDict.Add(guid2, new GuidLookupResult() { Entity = "APPLICATION.STATUSES", PrimaryKey = id2, SecondaryKey = sid2 });
                guidLookupDict.Add(guid3, new GuidLookupResult() { Entity = "APPLICATION.STATUSES", PrimaryKey = id3, SecondaryKey = sid3 });

                recordLookupDict.Add("APPLICATION.STATUSES+" + id + "+" + sid, new RecordKeyLookupResult() { Guid = guid });
                recordLookupDict.Add("APPLICATION.STATUSES+" + id2 + "+" + sid2, new RecordKeyLookupResult() { Guid = guid2 });
                recordLookupDict.Add("APPLICATION.STATUSES+" + id3 + "+" + sid3, new RecordKeyLookupResult() { Guid = guid3 });

                dataAccessorMock.Setup(dr => dr.SelectAsync(It.IsAny<GuidLookup[]>())).ReturnsAsync(guidLookupDict);
                dataAccessorMock.Setup(dr => dr.SelectAsync(It.IsAny<RecordKeyLookup[]>())).ReturnsAsync(recordLookupDict);
                dataAccessorMock.Setup(da => da.SelectAsync(It.IsAny<RecordKeyLookup[]>())).ReturnsAsync(recordLookupDict);

                // Setup response to ApplicationStatuses read
                //var entityCollection = new Collection<ApplicationStatuses>(_applicationStatusesCollection.Select(record =>
                //    new Data.Student.DataContracts.ApplicationStatuses()
                //    {
                //        Recordkey = record.Code,
                //        AppsDesc = record.Description,
                //        RecordGuid = record.Guid
                //    }).ToList());
                //entityCollection.ToList()[0].AppsSpecialProcessingCode = "AP";
                //entityCollection.ToList()[1].AppsSpecialProcessingCode = "CO";
                //entityCollection.ToList()[2].AppsSpecialProcessingCode = "WI";
                //entityCollection.ToList()[3].AppsSpecialProcessingCode = "WI";
                //entityCollection.ToList()[4].AppsSpecialProcessingCode = "MS";
                //entityCollection.ToList()[5].AppsSpecialProcessingCode = "Inalid";
                //entityCollection.ToList()[0].AppIntgAdmDecTypIdx = "11";
                //entityCollection.ToList()[1].AppIntgAdmDecTypIdx = "22";
                //entityCollection.ToList()[2].AppIntgAdmDecTypIdx= "33";
                //entityCollection.ToList()[3].AppIntgAdmDecTypIdx = "WI";
                //entityCollection.ToList()[4].AppIntgAdmDecTypIdx = "MS";
                //entityCollection.ToList()[5].AppIntgAdmDecTypIdx = "Inalid";



                // Build responses used for mocking
                _appStatusesCollection = new Collection<DataContracts.ApplicationStatuses>()
                {
                    new DataContracts.ApplicationStatuses() { RecordGuid = guid, Recordkey = id, AppIntgAdmDecTypIdx = sid, AppsDesc = "123" } ,
                    new DataContracts.ApplicationStatuses() { RecordGuid = guid2, Recordkey = id2, AppIntgAdmDecTypIdx = sid2, AppsDesc = "123" },
                    new DataContracts.ApplicationStatuses() { RecordGuid = guid3, Recordkey = id3, AppIntgAdmDecTypIdx = sid3, AppsDesc = "123" }
                    };

                List<string> ApplicationStatusesIds = new List<string>();
                foreach (var mp in _appStatusesCollection)
                {
                    ApplicationStatusesIds.Add(mp.Recordkey);
                };

                dataAccessorMock.Setup(repo => repo.SelectAsync("APPLICATION.STATUSES", "WITH APP.INTG.ADM.DEC.TYP.IDX NE ''")).ReturnsAsync(ApplicationStatusesIds.ToArray());
                
                dataAccessorMock.Setup(acc => acc.BulkReadRecordAsync<ApplicationStatuses>(It.IsAny<string[]>(), It.IsAny<bool>()))
                    .ReturnsAsync(_appStatusesCollection);

                cacheProviderMock.Setup<Task<Tuple<object, SemaphoreSlim>>>(x => x.GetAndLockSemaphoreAsync(It.IsAny<string>(), null))
                    .ReturnsAsync(new Tuple<object, SemaphoreSlim>(null, new SemaphoreSlim(1, 1)));


                //dataAccessorMock.Setup(acc => acc.SelectAsync(It.IsAny<RecordKeyLookup[]>())).Returns<RecordKeyLookup[]>(recordKeyLookups =>
                //{
                //    var result = new Dictionary<string, RecordKeyLookupResult>();
                //    foreach (var recordKeyLookup in recordKeyLookups)
                //    {
                //        var entity = _applicationStatusesCollection.Where(e => e.Code == recordKeyLookup.PrimaryKey).FirstOrDefault();
                //        result.Add(string.Join("+", new string[] { "APPLICATION.STATUSES", entity.Code }),
                //            new RecordKeyLookupResult() { Guid = entity.Guid });
                //    }
                //    return Task.FromResult(result);
                //});

                // Construct repository
                referenceDataRepo = new StudentReferenceDataRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object, apiSettings);

                return referenceDataRepo;             
            }
        }

        #endregion

        [TestClass]
        public class AdmissionPopulations
        {
            Mock<IColleagueTransactionFactory> transFactoryMock;
            Mock<ICacheProvider> cacheProviderMock;
            Mock<IColleagueDataReader> dataAccessorMock;
            Mock<ILogger> loggerMock;

            IEnumerable<AdmissionPopulation> allAdmissionPopulations;
            string valcodeName;
            ApiSettings apiSettings;

            StudentReferenceDataRepository referenceDataRepo;

            [TestInitialize]
            public void Initialize()
            {
                loggerMock = new Mock<ILogger>();
                apiSettings = new ApiSettings("TEST");

                allAdmissionPopulations = new TestStudentReferenceDataRepository().GetAdmissionPopulationsAsync(false).Result;

                referenceDataRepo = BuildValidReferenceDataRepository();
                valcodeName = referenceDataRepo.BuildFullCacheKey("AllAdmissionPopulations");
            }

            [TestCleanup]
            public void Cleanup()
            {
                allAdmissionPopulations = null;
                valcodeName = string.Empty;
                apiSettings = null;
            }

            [TestMethod]
            public async Task StudentReferenceDataRepository_GetAdmissionPopulationsAsync_False()
            {
                var results = await referenceDataRepo.GetAdmissionPopulationsAsync(false);
                Assert.AreEqual(allAdmissionPopulations.Count(), results.Count());

                foreach (var admissionPopulation in allAdmissionPopulations)
                {
                    var result = results.FirstOrDefault(i => i.Guid == admissionPopulation.Guid);

                    Assert.AreEqual(admissionPopulation.Code, result.Code);
                    Assert.AreEqual(admissionPopulation.Description, result.Description);
                    Assert.AreEqual(admissionPopulation.Guid, result.Guid);
                }

            }

            [TestMethod]
            public async Task StudentReferenceDataRepository_GetAdmissionPopulationsAsync_True()
            {
                var results = await referenceDataRepo.GetAdmissionPopulationsAsync(true);
                Assert.AreEqual(allAdmissionPopulations.Count(), results.Count());

                foreach (var admissionPopulation in allAdmissionPopulations)
                {
                    var result = results.FirstOrDefault(i => i.Guid == admissionPopulation.Guid);

                    Assert.AreEqual(admissionPopulation.Code, result.Code);
                    Assert.AreEqual(admissionPopulation.Description, result.Description);
                    Assert.AreEqual(admissionPopulation.Guid, result.Guid);
                }

            }

            private StudentReferenceDataRepository BuildValidReferenceDataRepository()
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

                var records = new Collection<DataContracts.AdmitStatuses>();
                foreach (var item in allAdmissionPopulations)
                {
                    DataContracts.AdmitStatuses record = new DataContracts.AdmitStatuses();
                    record.RecordGuid = item.Guid;
                    record.AdmsDesc = item.Description;
                    record.Recordkey = item.Code;
                    records.Add(record);
                }

                dataAccessorMock.Setup(ap => ap.BulkReadRecordAsync<DataContracts.AdmitStatuses>("ADMIT.STATUSES", "", true)).ReturnsAsync(records);

                cacheProviderMock.Setup<Task<Tuple<object, SemaphoreSlim>>>(x =>
                 x.GetAndLockSemaphoreAsync(It.IsAny<string>(), null))
                 .ReturnsAsync(new Tuple<object, SemaphoreSlim>(null, new SemaphoreSlim(1, 1)));

                dataAccessorMock.Setup(acc => acc.SelectAsync(It.IsAny<RecordKeyLookup[]>())).Returns<RecordKeyLookup[]>(recordKeyLookups =>
                {
                    var result = new Dictionary<string, RecordKeyLookupResult>();
                    foreach (var recordKeyLookup in recordKeyLookups)
                    {
                        var record = allAdmissionPopulations.Where(e => e.Code == recordKeyLookup.PrimaryKey).FirstOrDefault();
                        result.Add(string.Join("+", new string[] { "ADMIT.STATUSES", record.Code }),
                            new RecordKeyLookupResult() { Guid = record.Guid });
                    }
                    return Task.FromResult(result);
                });

                // Construct repository
                referenceDataRepo = new StudentReferenceDataRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object, apiSettings);

                return referenceDataRepo;
            }
        }

        [TestClass]
        public class AdmissionResidencyTypes
        {
            Mock<IColleagueTransactionFactory> transFactoryMock;
            Mock<ICacheProvider> cacheProviderMock;
            Mock<IColleagueDataReader> dataAccessorMock;
            Mock<ILogger> loggerMock;
            IEnumerable<AdmissionResidencyType> allAdmissionResidencyType;
            ApplValcodes admissionResidencyTypeValcodeResponse;
            string valcodeName;
            ApiSettings apiSettings;

            StudentReferenceDataRepository referenceDataRepo;

            [TestInitialize]
            public void Initialize()
            {
                loggerMock = new Mock<ILogger>();
                apiSettings = new ApiSettings("TEST");

                // Build admission residency responses used for mocking
                allAdmissionResidencyType = new TestStudentReferenceDataRepository().GetAdmissionResidencyTypesAsync(false).Result;
                admissionResidencyTypeValcodeResponse = BuildValcodeResponse(allAdmissionResidencyType);

                // Build student reference data repository
                referenceDataRepo = BuildValidReferenceDataRepository();
                valcodeName = referenceDataRepo.BuildFullCacheKey("AllAdmissionResidencyTypes");
            }

            [TestCleanup]
            public void Cleanup()
            {
                transFactoryMock = null;
                dataAccessorMock = null;
                cacheProviderMock = null;
                admissionResidencyTypeValcodeResponse = null;
                allAdmissionResidencyType = null;
                referenceDataRepo = null;
            }

            [TestMethod]
            public async Task StudentReferenceDataRepo_AdmissionResidencyTypes_GetAdmissionResidencyTypesAsync()
            {
                var repoGetAdmissionResidencyTypes = await referenceDataRepo.GetAdmissionResidencyTypesAsync();
                for (int i = 0; i < allAdmissionResidencyType.Count(); i++)
                {
                    Assert.AreEqual(allAdmissionResidencyType.ElementAt(i).Code, repoGetAdmissionResidencyTypes.ElementAt(i).Code);
                    Assert.AreEqual(allAdmissionResidencyType.ElementAt(i).Description, repoGetAdmissionResidencyTypes.ElementAt(i).Description);
                }
            }

            [TestMethod]
            public async Task StudentReferenceDataRepo_AdmissionResidencyTypes_GetAdmissionResidencyTypesAsync_Cache()
            {
                var repoGetAdmissionResidencyTypes = await referenceDataRepo.GetAdmissionResidencyTypesAsync(false);
                for (int i = 0; i < allAdmissionResidencyType.Count(); i++)
                {
                    Assert.AreEqual(allAdmissionResidencyType.ElementAt(i).Code, repoGetAdmissionResidencyTypes.ElementAt(i).Code);
                    Assert.AreEqual(allAdmissionResidencyType.ElementAt(i).Description, repoGetAdmissionResidencyTypes.ElementAt(i).Description);
                }
            }

            [TestMethod]
            public async Task StudentReferenceDataRepo_AdmissionResidencyTypes_GetAdmissionResidencyTypesAsync_NoCache()
            {
                var repoGetAdmissionResidencyTypes = await referenceDataRepo.GetAdmissionResidencyTypesAsync(true);
                for (int i = 0; i < allAdmissionResidencyType.Count(); i++)
                {
                    Assert.AreEqual(allAdmissionResidencyType.ElementAt(i).Code, repoGetAdmissionResidencyTypes.ElementAt(i).Code);
                    Assert.AreEqual(allAdmissionResidencyType.ElementAt(i).Description, repoGetAdmissionResidencyTypes.ElementAt(i).Description);
                }
            }

            private StudentReferenceDataRepository BuildValidReferenceDataRepository()
            {
                // transaction factory mock
                transFactoryMock = new Mock<IColleagueTransactionFactory>();
                // Cache Mock
                //localCacheMock = new Mock<ObjectCache>();
                // Cache Provider Mock
                cacheProviderMock = new Mock<ICacheProvider>();
                // Set up data accessor for mocking 
                dataAccessorMock = new Mock<IColleagueDataReader>();
                apiSettings = new ApiSettings("TEST");

                // Set up dataAccessorMock as the object for the DataAccessor
                transFactoryMock.Setup(transFac => transFac.GetDataReader()).Returns(dataAccessorMock.Object);

                var records = new Collection<DataContracts.ResidencyStatuses>();
                foreach (var item in allAdmissionResidencyType)
                {
                    DataContracts.ResidencyStatuses record = new DataContracts.ResidencyStatuses();
                    record.RecordGuid = item.Guid;
                    record.ResDesc = item.Description;
                    record.Recordkey = item.Code;
                    records.Add(record);
                }
                dataAccessorMock.Setup(acc => acc.BulkReadRecordAsync<DataContracts.ResidencyStatuses>("RESIDENCY.STATUSES", "", true)).ReturnsAsync(records);

                cacheProviderMock.Setup<Task<Tuple<object, SemaphoreSlim>>>(x =>
                 x.GetAndLockSemaphoreAsync(It.IsAny<string>(), null))
                 .ReturnsAsync(new Tuple<object, SemaphoreSlim>(null, new SemaphoreSlim(1, 1)));

                dataAccessorMock.Setup(acc => acc.SelectAsync(It.IsAny<RecordKeyLookup[]>())).Returns<RecordKeyLookup[]>(recordKeyLookups =>
                {
                    var result = new Dictionary<string, RecordKeyLookupResult>();
                    foreach (var recordKeyLookup in recordKeyLookups)
                    {
                        var record = allAdmissionResidencyType.Where(e => e.Code == recordKeyLookup.PrimaryKey).FirstOrDefault();
                        result.Add(string.Join("+", new string[] { "RESIDENCY.STATUSES", record.Code }),
                            new RecordKeyLookupResult() { Guid = record.Guid });
                    }
                    return Task.FromResult(result);
                });

                // Construct repository
                referenceDataRepo = new StudentReferenceDataRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object, apiSettings);

                return referenceDataRepo;
            }

            private ApplValcodes BuildValcodeResponse(IEnumerable<AdmissionResidencyType> admissionResidencyType)
            {
                ApplValcodes admissionResidencyTypeResponse = new ApplValcodes();
                admissionResidencyTypeResponse.ValsEntityAssociation = new List<ApplValcodesVals>();
                foreach (var item in admissionResidencyType)
                {
                    admissionResidencyTypeResponse.ValsEntityAssociation.Add(new ApplValcodesVals("", item.Description, "", item.Code, "", "", ""));
                }
                return admissionResidencyTypeResponse;
            }
        }
           
        [TestClass]
        public class AdvisorTypes
        {
            Mock<IColleagueTransactionFactory> transFactoryMock;
            Mock<ICacheProvider> cacheProviderMock;
            Mock<IColleagueDataReader> dataAccessorMock;
            Mock<ILogger> loggerMock;
            IEnumerable<AdvisorType> allAdvisorTypes;
            ApplValcodes advisorTypeValcodeResponse;
            string valcodeName;
            ApiSettings apiSettings;

            Mock<IStudentReferenceDataRepository> referenceDataRepositoryMock;
            IStudentReferenceDataRepository referenceDataRepository;
            StudentReferenceDataRepository referenceDataRepo;

            [TestInitialize]
            public void Initialize()
            {
                loggerMock = new Mock<ILogger>();
                apiSettings = new ApiSettings("TEST");

                // Build advisor types responses used for mocking
                allAdvisorTypes = new TestStudentReferenceDataRepository().GetAdvisorTypesAsync(false).Result;
                advisorTypeValcodeResponse = BuildValcodeResponse(allAdvisorTypes);
                var advisorTypeValResponse = new List<string>() { "2" };
                advisorTypeValcodeResponse.ValActionCode1 = advisorTypeValResponse;

                referenceDataRepositoryMock = new Mock<IStudentReferenceDataRepository>();
                referenceDataRepository = referenceDataRepositoryMock.Object;

                // Build course level repository
                referenceDataRepo = BuildValidReferenceDataRepository();
                valcodeName = referenceDataRepo.BuildFullCacheKey("ST_ADVISOR.TYPES_GUID");

                cacheProviderMock.Setup<Task<Tuple<object, SemaphoreSlim>>>(x =>
                   x.GetAndLockSemaphoreAsync(It.IsAny<string>(), null))
                   .ReturnsAsync(new Tuple<object, SemaphoreSlim>(null, new SemaphoreSlim(1, 1)));
            }

            [TestCleanup]
            public void Cleanup()
            {
                transFactoryMock = null;
                dataAccessorMock = null;
                cacheProviderMock = null;
                advisorTypeValcodeResponse = null;
                allAdvisorTypes = null;
                referenceDataRepo = null;
            }

            [TestMethod]
            public async Task StudentReferenceDataRepo_GetsAdvisorTypesNoArgAsync()
            {
                var advisorTypes = await referenceDataRepo.GetAdvisorTypesAsync(false);

                for (int i = 0; i < allAdvisorTypes.Count(); i++)
                {
                    Assert.AreEqual(allAdvisorTypes.ElementAt(i).Code, advisorTypes.ElementAt(i).Code);
                    Assert.AreEqual(allAdvisorTypes.ElementAt(i).Description, advisorTypes.ElementAt(i).Description);
                }
            }

            [TestMethod]
            public async Task StudentReferenceDataRepo_GetsAdvisorTypesCacheAsync()
            {
                var advisorTypes = await referenceDataRepo.GetAdvisorTypesAsync(false);

                for (int i = 0; i < allAdvisorTypes.Count(); i++)
                {
                    Assert.AreEqual(allAdvisorTypes.ElementAt(i).Code, advisorTypes.ElementAt(i).Code);
                    Assert.AreEqual(allAdvisorTypes.ElementAt(i).Description, advisorTypes.ElementAt(i).Description);
                }
            }

            [TestMethod]
            public async Task StudentReferenceDataRepo_GetsAdvisorTypesNonCacheAsync()
            {
                var statuses = await referenceDataRepo.GetAdvisorTypesAsync(true);

                for (int i = 0; i < allAdvisorTypes.Count(); i++)
                {
                    Assert.AreEqual(allAdvisorTypes.ElementAt(i).Code, statuses.ElementAt(i).Code);
                    Assert.AreEqual(allAdvisorTypes.ElementAt(i).Description, statuses.ElementAt(i).Description);
                }
            }

            [TestMethod]
            public async Task StudentReferenceDataRepo_GetAdvisorTypes_WritesToCacheAsync()
            {

                // Set up local cache mock to respond to cache request:
                //  -to "Contains" request, return "false" to indicate item is not in cache
                //  -to cache "Get" request, return null so we know it's reading from the "repository"
                cacheProviderMock.Setup(x => x.Contains(valcodeName, null)).Returns(false);
                cacheProviderMock.Setup(x => x.Get(valcodeName, null)).Returns(null);

                // return a valid response to the data accessor request
                dataAccessorMock.Setup(acc => acc.ReadRecordAsync<ApplValcodes>("ST.VALCODES", "ADVISOR.TYPES", It.IsAny<bool>())).ReturnsAsync(advisorTypeValcodeResponse);

                cacheProviderMock.Setup<Task<Tuple<object, SemaphoreSlim>>>(x =>
                 x.GetAndLockSemaphoreAsync(It.IsAny<string>(), null))
                 .ReturnsAsync(new Tuple<object, SemaphoreSlim>(null, new SemaphoreSlim(1, 1)));

                // But after data accessor read, set up mocking so we can verify the list of AdvisorTypes was written to the cache
                cacheProviderMock.Setup(x => x.Add(It.IsAny<string>(), It.IsAny<Task<List<AdvisorType>>>(), It.IsAny<CacheItemPolicy>(), null)).Verifiable();

                cacheProviderMock.Setup(x => x.Contains(referenceDataRepo.BuildFullCacheKey("ST_ADVISOR.TYPES"), null)).Returns(true);
                var advisorTypes = await referenceDataRepo.GetAdvisorTypesAsync(false);
                cacheProviderMock.Setup(x => x.Get(referenceDataRepo.BuildFullCacheKey("ST_ADVISORS"), null)).Returns(advisorTypes);
                // Verify that advisorTypes were returned, which means they came from the "repository".
                Assert.IsTrue(advisorTypes.Count() == 3);

                // Verify that the advisorType item was added to the cache after it was read from the repository
                cacheProviderMock.Verify(m => m.Add(It.IsAny<string>(), It.IsAny<Task<List<AdvisorType>>>(), It.IsAny<CacheItemPolicy>(), null), Times.Never);

            }

            [TestMethod]
            public async Task StudentReferenceDataRepo_GetAdvisorTypes_GetsCachedAdvisorTypesAsync()
            {
                // Set up local cache mock to respond to cache request:
                //  -to "Contains" request, return "true" to indicate item is in cache
                //  -to "Get" request, return the cache item (in this case the "COURSE.LEVELS" cache item)
                cacheProviderMock.Setup(x => x.Contains(valcodeName, null)).Returns(true);
                cacheProviderMock.Setup(x => x.Get(valcodeName, null)).Returns(allAdvisorTypes).Verifiable();

                // return null for request, so that if we have a result, it wasn't the data accessor that returned it.
                dataAccessorMock.Setup(acc => acc.ReadRecordAsync<ApplValcodes>("ST.VALCODES", "ADVISOR.TYPES", true)).ReturnsAsync(new ApplValcodes());

                // Assert the AdvisorTypes are returned
                Assert.IsTrue((await referenceDataRepo.GetAdvisorTypesAsync(false)).Count() == 3);
                // Verify that the sectionRegistrationStatuses were retrieved from cache
                cacheProviderMock.Verify(m => m.Get(valcodeName, null));
            }

            private StudentReferenceDataRepository BuildValidReferenceDataRepository()
            {
                // transaction factory mock
                transFactoryMock = new Mock<IColleagueTransactionFactory>();
                // Cache Provider Mock
                cacheProviderMock = new Mock<ICacheProvider>();
                // Set up data accessor for mocking 
                dataAccessorMock = new Mock<IColleagueDataReader>();

                // Set up dataAccessorMock as the object for the DataAccessor
                transFactoryMock.Setup(transFac => transFac.GetDataReader()).Returns(dataAccessorMock.Object);

                // Setup response to assessmentSpecialCircumstane valcode read
                dataAccessorMock.Setup(acc => acc.ReadRecordAsync<ApplValcodes>("ST.VALCODES", "ADVISOR.TYPES", It.IsAny<bool>())).ReturnsAsync(advisorTypeValcodeResponse);
                cacheProviderMock.Setup<Task<Tuple<object, SemaphoreSlim>>>(x => x.GetAndLockSemaphoreAsync(It.IsAny<string>(), null))
                .ReturnsAsync(new Tuple<object, SemaphoreSlim>(null, new SemaphoreSlim(1, 1)));

                dataAccessorMock.Setup(acc => acc.SelectAsync(It.IsAny<RecordKeyLookup[]>())).Returns<RecordKeyLookup[]>(recordKeyLookups =>
                {
                    var result = new Dictionary<string, RecordKeyLookupResult>();
                    foreach (var recordKeyLookup in recordKeyLookups)
                    {
                        var advisorType = allAdvisorTypes.Where(e => e.Code == recordKeyLookup.SecondaryKey).FirstOrDefault();
                        result.Add(string.Join("+", new string[] { "ST.VALCODES", "ADVISOR.TYPES", advisorType.Code }),
                            new RecordKeyLookupResult() { Guid = advisorType.Guid });
                    }
                    return Task.FromResult(result);
                });

                // Construct repository
                referenceDataRepo = new StudentReferenceDataRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object, apiSettings);

                return referenceDataRepo;
            }

            private ApplValcodes BuildValcodeResponse(IEnumerable<AdvisorType> advisorTypes)
            {
                ApplValcodes advisorTypesResponse = new ApplValcodes();
                advisorTypesResponse.ValsEntityAssociation = new List<ApplValcodesVals>();
                foreach (var item in advisorTypes)
                {
                    advisorTypesResponse.ValsEntityAssociation.Add(new ApplValcodesVals("", item.Description, "2", item.Code, "3", "", ""));
                }
                return advisorTypesResponse;
            }
        }

        [TestClass]
        public class ApplicationInfluences
        {
            Mock<IColleagueTransactionFactory> transFactoryMock;
            //Mock<ObjectCache> localCacheMock;
            Mock<ICacheProvider> cacheProviderMock;
            Mock<IColleagueDataReader> dataAccessorMock;
            Mock<ILogger> loggerMock;
            IEnumerable<Ellucian.Colleague.Domain.Student.Entities.ApplicationInfluence> allApplicationInfluences;
            ApplValcodes ApplicationInfluencesValcodeResponse;
            string valcodeName;
            ApiSettings apiSettings;
            StudentReferenceDataRepository referenceDataRepo;

            [TestInitialize]
            public void Initialize()
            {
                loggerMock = new Mock<ILogger>();

                // Build responses used for mocking
                allApplicationInfluences = new TestStudentReferenceDataRepository().GetApplicationInfluencesAsync().Result;

                ApplicationInfluencesValcodeResponse = BuildValcodeResponse(allApplicationInfluences);

                // Build application influences repository
                referenceDataRepo = BuildValidReferenceDataRepository();
                valcodeName = referenceDataRepo.BuildFullCacheKey("ST_APPL.INFLUENCES");

                cacheProviderMock.Setup<Task<Tuple<object, SemaphoreSlim>>>(x =>
                   x.GetAndLockSemaphoreAsync(It.IsAny<string>(), null))
                   .ReturnsAsync(new Tuple<object, SemaphoreSlim>(null, new SemaphoreSlim(1, 1)));

            }

            [TestCleanup]
            public void Cleanup()
            {
                transFactoryMock = null;
                dataAccessorMock = null;
                cacheProviderMock = null;
                ApplicationInfluencesValcodeResponse = null;
                allApplicationInfluences = null;
                referenceDataRepo = null;
            }

            [TestMethod]
            public async Task StudentReferenceDataRepo_GetsApplicationInfluences()
            {
                var repo = await referenceDataRepo.GetApplicationInfluencesAsync();

                for (int i = 0; i < allApplicationInfluences.Count(); i++)
                {
                    Assert.AreEqual(allApplicationInfluences.ElementAt(i).Code, repo.ElementAt(i).Code);
                    Assert.AreEqual(allApplicationInfluences.ElementAt(i).Description, repo.ElementAt(i).Description);
                }
            }

            [TestMethod]
            public async Task StudentReferenceDataRepo_GetApplicationInfluences_WritesToCache()
            {
                // Set up local cache mock to respond to cache request:
                //  -to "Contains" request, return "false" to indicate item is not in cache
                //  -to cache "Get" request, return null so we know it's reading from the "repository"
                cacheProviderMock.Setup(x => x.Contains(valcodeName, null)).Returns(false);
                cacheProviderMock.Setup(x => x.Get(valcodeName, null)).Returns(null);

                // return a valid response to the data accessor request
                dataAccessorMock.Setup(acc => acc.ReadRecordAsync<ApplValcodes>(It.IsAny<string>(), It.IsAny<string>(), true)).ReturnsAsync(ApplicationInfluencesValcodeResponse);

                // But after data accessor read, set up mocking so we can verify the list of application influences was written to the cache
                cacheProviderMock.Setup(x => x.AddAndUnlockSemaphore(valcodeName, It.IsAny<Object>(), It.IsAny<SemaphoreSlim>(), It.IsAny<CacheItemPolicy>(), null)).Verifiable();

                // Verify that application influences were returned, which means they came from the "repository".
                Assert.IsTrue((await referenceDataRepo.GetApplicationInfluencesAsync()).Count() == allApplicationInfluences.Count());

                // Verify that the application influences item was added to the cache after it was read from the repository
                cacheProviderMock.Verify(m => m.AddAndUnlockSemaphore(valcodeName, It.IsAny<Object>(), It.IsAny<SemaphoreSlim>(), It.IsAny<CacheItemPolicy>(), null));
            }

            [TestMethod]
            public async Task StudentReferenceDataRepo_GetApplicationInfluences_Cached()
            {
                object lockHandle = null;
                // Set up local cache mock to respond to cache request:
                //  -to "Contains" request, return "false" to indicate item is not in cache
                //  -to cache "Get" request, return null so we know it's getting data from "repository"
                cacheProviderMock.Setup(x => x.Contains(valcodeName, null)).Returns(false);
                cacheProviderMock.Setup(x => x.GetAndLock(valcodeName, out lockHandle, null)).Returns(null);

                // Get these codes to populate the cache
                var applicationInfluences = (await referenceDataRepo.GetApplicationInfluencesAsync());
                // Assert the types are returned
                Assert.IsTrue((await referenceDataRepo.GetApplicationInfluencesAsync()).Count() == allApplicationInfluences.Count());
                // Make sure we can verify that it's in the cache
                cacheProviderMock.Setup(x => x.Contains(valcodeName, null)).Returns(true);
                var applicationInfluencesTuple = new Tuple<object, SemaphoreSlim>(applicationInfluences, new SemaphoreSlim(1, 1));
                cacheProviderMock.Setup(x => x.GetAndLockSemaphoreAsync(valcodeName, null)).ReturnsAsync(applicationInfluencesTuple).Verifiable();

                // Verify that the applicationInfluences are now retrieved from cache
                applicationInfluences = (await referenceDataRepo.GetApplicationInfluencesAsync());
                cacheProviderMock.Verify(m => m.GetAndLockSemaphoreAsync(valcodeName, null));
            }

            private T GetCache<T>(ICacheProvider cacheProvider, string key)
                where T : class
            {
                object cache = cacheProvider.Get(key, null);
                return cache as T;
            }

            private StudentReferenceDataRepository BuildValidReferenceDataRepository()
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

                // Setup response to application influences valcode read
                dataAccessorMock.Setup(acc => acc.ReadRecordAsync<ApplValcodes>("ST.VALCODES", "APPL.INFLUENCES", true)).ReturnsAsync(ApplicationInfluencesValcodeResponse);

                // Construct repository
                referenceDataRepo = new StudentReferenceDataRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object, apiSettings);

                return referenceDataRepo;
            }

            private ApplValcodes BuildValcodeResponse(IEnumerable<Ellucian.Colleague.Domain.Student.Entities.ApplicationInfluence> applicationInfluence)
            {
                ApplValcodes applValcodeResponse = new ApplValcodes();
                applValcodeResponse.ValsEntityAssociation = new List<ApplValcodesVals>();
                foreach (var item in applicationInfluence)
                {
                    applValcodeResponse.ValsEntityAssociation.Add(new ApplValcodesVals("", item.Description, "", item.Code, "", "", ""));
                }
                return applValcodeResponse;
            }
        }

        [TestClass]
        public class AssessmentSpecialCircumstances
        {
            Mock<IColleagueTransactionFactory> transFactoryMock;
            Mock<ICacheProvider> cacheProviderMock;
            Mock<IColleagueDataReader> dataAccessorMock;
            Mock<ILogger> loggerMock;
            IEnumerable<AssessmentSpecialCircumstance> allAssessmentSpecialCircumstances;
            ApplValcodes assessmentSpecialCircumstanceValcodeResponse;
            string valcodeName;
            ApiSettings apiSettings;

            Mock<IStudentReferenceDataRepository> referenceDataRepositoryMock;
            IStudentReferenceDataRepository referenceDataRepository;
            StudentReferenceDataRepository referenceDataRepo;

            [TestInitialize]
            public void Initialize()
            {
                loggerMock = new Mock<ILogger>();
                apiSettings = new ApiSettings("TEST");

                // Build Courses responses used for mocking
                allAssessmentSpecialCircumstances = new TestStudentReferenceDataRepository().GetAssessmentSpecialCircumstancesAsync(false).Result;
                assessmentSpecialCircumstanceValcodeResponse = BuildValcodeResponse(allAssessmentSpecialCircumstances);
                var assessmentSpecialCircumstanceValResponse = new List<string>() { "2" };
                assessmentSpecialCircumstanceValcodeResponse.ValActionCode1 = assessmentSpecialCircumstanceValResponse;

                referenceDataRepositoryMock = new Mock<IStudentReferenceDataRepository>();
                referenceDataRepository = referenceDataRepositoryMock.Object;

                // Build course level repository
                referenceDataRepo = BuildValidReferenceDataRepository();
                valcodeName = referenceDataRepo.BuildFullCacheKey("ST_NON.COURSE.FACTORS_GUID");

                cacheProviderMock.Setup<Task<Tuple<object, SemaphoreSlim>>>(x =>
                   x.GetAndLockSemaphoreAsync(It.IsAny<string>(), null))
                   .ReturnsAsync(new Tuple<object, SemaphoreSlim>(null, new SemaphoreSlim(1, 1)));
            }

            [TestCleanup]
            public void Cleanup()
            {
                transFactoryMock = null;
                dataAccessorMock = null;
                cacheProviderMock = null;
                assessmentSpecialCircumstanceValcodeResponse = null;
                allAssessmentSpecialCircumstances = null;
                referenceDataRepo = null;
            }

            [TestMethod]
            public async Task StudentReferenceDataRepo_GetsAssessmentSpecialCircumstancesNoArgAsync()
            {
                var assessmentSpecialCircumstances = await referenceDataRepo.GetAssessmentSpecialCircumstancesAsync(false);

                for (int i = 0; i < allAssessmentSpecialCircumstances.Count(); i++)
                {
                    Assert.AreEqual(allAssessmentSpecialCircumstances.ElementAt(i).Code, assessmentSpecialCircumstances.ElementAt(i).Code);
                    Assert.AreEqual(allAssessmentSpecialCircumstances.ElementAt(i).Description, assessmentSpecialCircumstances.ElementAt(i).Description);
                }
            }

            [TestMethod]
            public async Task StudentReferenceDataRepo_GetsAssessmentSpecialCircumstancesCacheAsync()
            {
                var assessmentSpecialCircumstances = await referenceDataRepo.GetAssessmentSpecialCircumstancesAsync(false);

                for (int i = 0; i < allAssessmentSpecialCircumstances.Count(); i++)
                {
                    Assert.AreEqual(allAssessmentSpecialCircumstances.ElementAt(i).Code, assessmentSpecialCircumstances.ElementAt(i).Code);
                    Assert.AreEqual(allAssessmentSpecialCircumstances.ElementAt(i).Description, assessmentSpecialCircumstances.ElementAt(i).Description);
                }
            }

            [TestMethod]
            public async Task StudentReferenceDataRepo_GetsAssessmentSpecialCircumstancesNonCacheAsync()
            {
                var statuses = await referenceDataRepo.GetAssessmentSpecialCircumstancesAsync(true);

                for (int i = 0; i < allAssessmentSpecialCircumstances.Count(); i++)
                {
                    Assert.AreEqual(allAssessmentSpecialCircumstances.ElementAt(i).Code, statuses.ElementAt(i).Code);
                    Assert.AreEqual(allAssessmentSpecialCircumstances.ElementAt(i).Description, statuses.ElementAt(i).Description);
                }
            }

            [TestMethod]
            public async Task StudentReferenceDataRepo_GetAssessmentSpecialCircumstances_WritesToCacheAsync()
            {

                // Set up local cache mock to respond to cache request:
                //  -to "Contains" request, return "false" to indicate item is not in cache
                //  -to cache "Get" request, return null so we know it's reading from the "repository"
                cacheProviderMock.Setup(x => x.Contains(valcodeName, null)).Returns(false);
                cacheProviderMock.Setup(x => x.Get(valcodeName, null)).Returns(null);

                // return a valid response to the data accessor request
                dataAccessorMock.Setup(acc => acc.ReadRecordAsync<ApplValcodes>("ST.VALCODES", "NON.COURSE.FACTORS", It.IsAny<bool>())).ReturnsAsync(assessmentSpecialCircumstanceValcodeResponse);

                cacheProviderMock.Setup<Task<Tuple<object, SemaphoreSlim>>>(x =>
                 x.GetAndLockSemaphoreAsync(It.IsAny<string>(), null))
                 .ReturnsAsync(new Tuple<object, SemaphoreSlim>(null, new SemaphoreSlim(1, 1)));

                // But after data accessor read, set up mocking so we can verify the list of assessmentSpecialCircumstances was written to the cache
                cacheProviderMock.Setup(x => x.Add(It.IsAny<string>(), It.IsAny<Task<List<AssessmentSpecialCircumstance>>>(), It.IsAny<CacheItemPolicy>(), null)).Verifiable();

                cacheProviderMock.Setup(x => x.Contains(referenceDataRepo.BuildFullCacheKey("ST_NON.COURSE.FACTORS"), null)).Returns(true);
                var assessmentSpecialCircumstances = await referenceDataRepo.GetAssessmentSpecialCircumstancesAsync(false);
                cacheProviderMock.Setup(x => x.Get(referenceDataRepo.BuildFullCacheKey("ST_NON.COURSE.FACTORS"), null)).Returns(assessmentSpecialCircumstances);
                // Verify that assessmentSpecialCircumstances were returned, which means they came from the "repository".
                Assert.IsTrue(assessmentSpecialCircumstances.Count() == 3);

                // Verify that the assessmentSpecialCircumstance item was added to the cache after it was read from the repository
                cacheProviderMock.Verify(m => m.Add(It.IsAny<string>(), It.IsAny<Task<List<AssessmentSpecialCircumstance>>>(), It.IsAny<CacheItemPolicy>(), null), Times.Never);

            }

            [TestMethod]
            public async Task StudentReferenceDataRepo_GetAssessmentSpecialCircumstances_GetsCachedAssessmentSpecialCircumstancesAsync()
            {
                // Set up local cache mock to respond to cache request:
                //  -to "Contains" request, return "true" to indicate item is in cache
                //  -to "Get" request, return the cache item (in this case the "COURSE.LEVELS" cache item)
                cacheProviderMock.Setup(x => x.Contains(valcodeName, null)).Returns(true);
                cacheProviderMock.Setup(x => x.Get(valcodeName, null)).Returns(allAssessmentSpecialCircumstances).Verifiable();

                // return null for request, so that if we have a result, it wasn't the data accessor that returned it.
                dataAccessorMock.Setup(acc => acc.ReadRecordAsync<ApplValcodes>("ST.VALCODES", "NON.COURSE.FACTORS", true)).ReturnsAsync(new ApplValcodes());

                // Assert the assessmentSpecialCircumstances are returned
                Assert.IsTrue((await referenceDataRepo.GetAssessmentSpecialCircumstancesAsync(false)).Count() == 3);
                // Verify that the sectionRegistrationStatuses were retrieved from cache
                cacheProviderMock.Verify(m => m.Get(valcodeName, null));
            }

            private StudentReferenceDataRepository BuildValidReferenceDataRepository()
            {
                // transaction factory mock
                transFactoryMock = new Mock<IColleagueTransactionFactory>();
                // Cache Provider Mock
                cacheProviderMock = new Mock<ICacheProvider>();
                // Set up data accessor for mocking 
                dataAccessorMock = new Mock<IColleagueDataReader>();

                // Set up dataAccessorMock as the object for the DataAccessor
                transFactoryMock.Setup(transFac => transFac.GetDataReader()).Returns(dataAccessorMock.Object);

                // Setup response to assessmentSpecialCircumstane valcode read
                dataAccessorMock.Setup(acc => acc.ReadRecordAsync<ApplValcodes>("ST.VALCODES", "NON.COURSE.FACTORS", It.IsAny<bool>())).ReturnsAsync(assessmentSpecialCircumstanceValcodeResponse);
                cacheProviderMock.Setup<Task<Tuple<object, SemaphoreSlim>>>(x => x.GetAndLockSemaphoreAsync(It.IsAny<string>(), null))
                .ReturnsAsync(new Tuple<object, SemaphoreSlim>(null, new SemaphoreSlim(1, 1)));

                dataAccessorMock.Setup(acc => acc.SelectAsync(It.IsAny<RecordKeyLookup[]>())).Returns<RecordKeyLookup[]>(recordKeyLookups =>
                {
                    var result = new Dictionary<string, RecordKeyLookupResult>();
                    foreach (var recordKeyLookup in recordKeyLookups)
                    {
                        var assessmentSpecialCircumstance = allAssessmentSpecialCircumstances.Where(e => e.Code == recordKeyLookup.SecondaryKey).FirstOrDefault();
                        result.Add(string.Join("+", new string[] { "ST.VALCODES", "NON.COURSE.FACTORS", assessmentSpecialCircumstance.Code }),
                            new RecordKeyLookupResult() { Guid = assessmentSpecialCircumstance.Guid });
                    }
                    return Task.FromResult(result);
                });

                // Construct repository
                referenceDataRepo = new StudentReferenceDataRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object, apiSettings);

                return referenceDataRepo;
            }

            private ApplValcodes BuildValcodeResponse(IEnumerable<AssessmentSpecialCircumstance> assessmentSpecialCircumstances)
            {
                ApplValcodes assessmentSpecialCircumstancesResponse = new ApplValcodes();
                assessmentSpecialCircumstancesResponse.ValsEntityAssociation = new List<ApplValcodesVals>();
                foreach (var item in assessmentSpecialCircumstances)
                {
                    assessmentSpecialCircumstancesResponse.ValsEntityAssociation.Add(new ApplValcodesVals("", item.Description, "2", item.Code, "3", "", ""));
                }
                return assessmentSpecialCircumstancesResponse;
            }
        }

        [TestClass]
        public class BookOption
        {
            Mock<IColleagueTransactionFactory> transFactoryMock;
            Mock<ICacheProvider> cacheProviderMock;
            Mock<IColleagueDataReader> dataAccessorMock;
            Mock<ILogger> loggerMock;
            IEnumerable<Ellucian.Colleague.Domain.Student.Entities.BookOption> allBookOptions;
            ApplValcodes bookOptionValcodeResponse;
            string valcodeName;
            ApiSettings apiSettings;
            StudentReferenceDataRepository referenceDataRepo;

            [TestInitialize]
            public void Initialize()
            {
                loggerMock = new Mock<ILogger>();

                // Build responses used for mocking
                allBookOptions = new TestStudentReferenceDataRepository().GetBookOptionsAsync().Result;

                bookOptionValcodeResponse = BuildValcodeResponse(allBookOptions);

                // Build repository
                referenceDataRepo = BuildValidReferenceDataRepository();
                valcodeName = referenceDataRepo.BuildFullCacheKey("ST_BOOK.OPTION");

                cacheProviderMock.Setup<Task<Tuple<object, SemaphoreSlim>>>(x =>
                   x.GetAndLockSemaphoreAsync(It.IsAny<string>(), null))
                   .ReturnsAsync(new Tuple<object, SemaphoreSlim>(null, new SemaphoreSlim(1, 1)));

            }

            [TestCleanup]
            public void Cleanup()
            {
                transFactoryMock = null;
                dataAccessorMock = null;
                cacheProviderMock = null;
                bookOptionValcodeResponse = null;
                allBookOptions = null;
                referenceDataRepo = null;
            }

            [TestMethod]
            public async Task StudentReferenceDataRepo_GetBookOptions()
            {
                var repo = await referenceDataRepo.GetBookOptionsAsync();

                for (int i = 0; i < allBookOptions.Count(); i++)
                {
                    Assert.AreEqual(allBookOptions.ElementAt(i).Code, repo.ElementAt(i).Code);
                    Assert.AreEqual(allBookOptions.ElementAt(i).Description, repo.ElementAt(i).Description);
                    Assert.AreEqual(allBookOptions.ElementAt(i).IsRequired, repo.ElementAt(i).IsRequired);
                }
            }

            [TestMethod]
            public async Task StudentReferenceDataRepo_GetBookOptions_WritesToCache()
            {
                // Set up local cache mock to respond to cache request:
                //  -to "Contains" request, return "false" to indicate item is not in cache
                //  -to cache "Get" request, return null so we know it's reading from the "repository"
                cacheProviderMock.Setup(x => x.Contains(valcodeName, null)).Returns(false);
                cacheProviderMock.Setup(x => x.Get(valcodeName, null)).Returns(null);

                // return a valid response to the data accessor request
                dataAccessorMock.Setup(acc => acc.ReadRecordAsync<ApplValcodes>(It.IsAny<string>(), It.IsAny<string>(), true)).ReturnsAsync(bookOptionValcodeResponse);

                // But after data accessor read, set up mocking so we can verify the list of BookOptions was written to the cache
                cacheProviderMock.Setup(x => x.AddAndUnlockSemaphore(valcodeName, It.IsAny<Object>(), It.IsAny<SemaphoreSlim>(), It.IsAny<CacheItemPolicy>(), null)).Verifiable();

                // Verify that BookOptions were returned, which means they came from the "repository".
                Assert.IsTrue((await referenceDataRepo.GetBookOptionsAsync()).Count() == allBookOptions.Count());

                // Verify that the BookOptions was added to the cache after it was read from the repository
                cacheProviderMock.Verify(m => m.AddAndUnlockSemaphore(valcodeName, It.IsAny<Object>(), It.IsAny<SemaphoreSlim>(), It.IsAny<CacheItemPolicy>(), null));
            }

            [TestMethod]
            public async Task StudentReferenceDataRepo_GetBookOptions_Cached()
            {
                object lockHandle = null;
                // Set up local cache mock to respond to cache request:
                //  -to "Contains" request, return "false" to indicate item is not in cache
                //  -to cache "Get" request, return null so we know it's getting data from "repository"
                cacheProviderMock.Setup(x => x.Contains(valcodeName, null)).Returns(false);
                cacheProviderMock.Setup(x => x.GetAndLock(valcodeName, out lockHandle, null)).Returns(null);

                // Get these codes to populate the cache
                var bookOptions = (await referenceDataRepo.GetBookOptionsAsync());
                // Assert the BookOptions are returned
                Assert.IsTrue((await referenceDataRepo.GetBookOptionsAsync()).Count() == allBookOptions.Count());
                // Make sure we can verify that it's in the cache
                cacheProviderMock.Setup(x => x.Contains(valcodeName, null)).Returns(true);
                var bookOptionsTuple = new Tuple<object, SemaphoreSlim>(bookOptions, new SemaphoreSlim(1, 1));
                cacheProviderMock.Setup(x => x.GetAndLockSemaphoreAsync(valcodeName, null)).ReturnsAsync(bookOptionsTuple).Verifiable();

                // Verify that the types are now retrieved from cache
                bookOptions = (await referenceDataRepo.GetBookOptionsAsync());
                cacheProviderMock.Verify(m => m.GetAndLockSemaphoreAsync(valcodeName, null));
            }

            private T GetCache<T>(ICacheProvider cacheProvider, string key)
                where T : class
            {
                object cache = cacheProvider.Get(key, null);
                return cache as T;
            }

            private StudentReferenceDataRepository BuildValidReferenceDataRepository()
            {
                // transaction factory mock
                transFactoryMock = new Mock<IColleagueTransactionFactory>();
                // Cache Mock
                //localCacheMock = new Mock<ObjectCache>();
                // Cache Provider Mock
                cacheProviderMock = new Mock<ICacheProvider>();
                // Set up data accessor for mocking 
                dataAccessorMock = new Mock<IColleagueDataReader>();
                apiSettings = new ApiSettings("TEST");

                // Set up dataAccessorMock as the object for the DataAccessor
                transFactoryMock.Setup(transFac => transFac.GetDataReader()).Returns(dataAccessorMock.Object);

                // Setup response to course type valcode read
                dataAccessorMock.Setup(acc => acc.ReadRecordAsync<ApplValcodes>("ST.VALCODES", "BOOK.OPTION", true)).ReturnsAsync(bookOptionValcodeResponse);

                // Construct repository
                referenceDataRepo = new StudentReferenceDataRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object, apiSettings);

                return referenceDataRepo;
            }

            private ApplValcodes BuildValcodeResponse(IEnumerable<Ellucian.Colleague.Domain.Student.Entities.BookOption> bookOption)
            {
                ApplValcodes applValcodeResponse = new ApplValcodes();
                applValcodeResponse.ValsEntityAssociation = new List<ApplValcodesVals>();
                foreach (var item in bookOption)
                {
                    applValcodeResponse.ValsEntityAssociation.Add(new ApplValcodesVals("", item.Description, item.IsRequired ? "1" : "2", item.Code, "", "", ""));
                }
                return applValcodeResponse;
            }
        }

        [TestClass]
        public class BillingOverrideReasonsTests
        {
            Mock<IColleagueTransactionFactory> transFactoryMock;
            Mock<ICacheProvider> cacheProviderMock;
            Mock<IColleagueDataReader> dataAccessorMock;
            Mock<ILogger> loggerMock;
            IEnumerable<BillingOverrideReasons> allBillingOverrideReasons;
            ApplValcodes billingOverrideReasonsdomainEntityNameResponse;
            string domainEntityNameName;
            ApiSettings apiSettings;

            Mock<IStudentReferenceDataRepository> referenceDataRepositoryMock;
            IStudentReferenceDataRepository referenceDataRepository;
            StudentReferenceDataRepository referenceDataRepo;

            [TestInitialize]
            public void Initialize()
            {
                loggerMock = new Mock<ILogger>();
                apiSettings = new ApiSettings("TEST");

                // Build academic standings responses used for mocking
                allBillingOverrideReasons = new TestStudentReferenceDataRepository().GetBillingOverrideReasonsAsync(false).Result;
                billingOverrideReasonsdomainEntityNameResponse = BuilddomainEntityNameResponse(allBillingOverrideReasons);
                var billingOverrideReasonsValResponse = new List<string>() { "2" };
                billingOverrideReasonsdomainEntityNameResponse.ValActionCode1 = billingOverrideReasonsValResponse;

                referenceDataRepositoryMock = new Mock<IStudentReferenceDataRepository>();
                referenceDataRepository = referenceDataRepositoryMock.Object;

                // Build academic standing repository
                referenceDataRepo = BuildValidReferenceDataRepository();
                domainEntityNameName = referenceDataRepo.BuildFullCacheKey("ST_BILLING.OVERRIDE.REASONS_GUID");

                cacheProviderMock.Setup<Task<Tuple<object, SemaphoreSlim>>>(x =>
                   x.GetAndLockSemaphoreAsync(It.IsAny<string>(), null))
                   .ReturnsAsync(new Tuple<object, SemaphoreSlim>(null, new SemaphoreSlim(1, 1)));
            }

            [TestCleanup]
            public void Cleanup()
            {
                transFactoryMock = null;
                dataAccessorMock = null;
                cacheProviderMock = null;
                billingOverrideReasonsdomainEntityNameResponse = null;
                allBillingOverrideReasons = null;
                referenceDataRepo = null;
            }

            [TestMethod]
            public async Task StudentReferenceDataRepo_GetsBillingOverrideReasonsNoArgAsync()
            {
                var billingOverrideReasons = await referenceDataRepo.GetBillingOverrideReasonsAsync(It.IsAny<bool>());

                for (int i = 0; i < allBillingOverrideReasons.Count(); i++)
                {
                    Assert.AreEqual(allBillingOverrideReasons.ElementAt(i).Code, billingOverrideReasons.ElementAt(i).Code);
                    Assert.AreEqual(allBillingOverrideReasons.ElementAt(i).Description, billingOverrideReasons.ElementAt(i).Description);
                }
            }

            [TestMethod]
            public async Task StudentReferenceDataRepo_GetsBillingOverrideReasonsCacheAsync()
            {
                var billingOverrideReasons = await referenceDataRepo.GetBillingOverrideReasonsAsync(false);

                for (int i = 0; i < allBillingOverrideReasons.Count(); i++)
                {
                    Assert.AreEqual(allBillingOverrideReasons.ElementAt(i).Code, billingOverrideReasons.ElementAt(i).Code);
                    Assert.AreEqual(allBillingOverrideReasons.ElementAt(i).Description, billingOverrideReasons.ElementAt(i).Description);
                }
            }

            [TestMethod]
            public async Task StudentReferenceDataRepo_GetsBillingOverrideReasonsNonCacheAsync()
            {
                var statuses = await referenceDataRepo.GetBillingOverrideReasonsAsync(true);

                for (int i = 0; i < allBillingOverrideReasons.Count(); i++)
                {
                    Assert.AreEqual(allBillingOverrideReasons.ElementAt(i).Code, statuses.ElementAt(i).Code);
                    Assert.AreEqual(allBillingOverrideReasons.ElementAt(i).Description, statuses.ElementAt(i).Description);
                }
            }

            [TestMethod]
            public async Task StudentReferenceDataRepo_GetBillingOverrideReasons_WritesToCacheAsync()
            {

                // Set up local cache mock to respond to cache request:
                //  -to "Contains" request, return "false" to indicate item is not in cache
                //  -to cache "Get" request, return null so we know it's reading from the "repository"
                cacheProviderMock.Setup(x => x.Contains(domainEntityNameName, null)).Returns(false);
                cacheProviderMock.Setup(x => x.Get(domainEntityNameName, null)).Returns(null);

                // return a valid response to the data accessor request
                dataAccessorMock.Setup(acc => acc.ReadRecordAsync<ApplValcodes>("ST.VALCODES", "BILLING.OVERRIDE.REASONS", It.IsAny<bool>())).ReturnsAsync(billingOverrideReasonsdomainEntityNameResponse);

                cacheProviderMock.Setup<Task<Tuple<object, SemaphoreSlim>>>(x =>
                 x.GetAndLockSemaphoreAsync(It.IsAny<string>(), null))
                 .ReturnsAsync(new Tuple<object, SemaphoreSlim>(null, new SemaphoreSlim(1, 1)));

                // But after data accessor read, set up mocking so we can verify the list of billingOverrideReasons was written to the cache
                cacheProviderMock.Setup(x => x.Add(It.IsAny<string>(), It.IsAny<Task<List<BillingOverrideReasons>>>(), It.IsAny<CacheItemPolicy>(), null)).Verifiable();

                cacheProviderMock.Setup(x => x.Contains(referenceDataRepo.BuildFullCacheKey("ST_BILLING.OVERRIDE.REASONS"), null)).Returns(true);
                var billingOverrideReasons = await referenceDataRepo.GetBillingOverrideReasonsAsync(false);
                cacheProviderMock.Setup(x => x.Get(referenceDataRepo.BuildFullCacheKey("ST_BILLING.OVERRIDE.REASONS"), null)).Returns(billingOverrideReasons);
                // Verify that billingOverrideReasons were returned, which means they came from the "repository".
                Assert.IsTrue(billingOverrideReasons.Count() == 3);

                // Verify that the billingOverrideReasons item was added to the cache after it was read from the repository
                cacheProviderMock.Verify(m => m.Add(It.IsAny<string>(), It.IsAny<Task<List<BillingOverrideReasons>>>(), It.IsAny<CacheItemPolicy>(), null), Times.Never);

            }

            [TestMethod]
            public async Task StudentReferenceDataRepo_GetBillingOverrideReasons_GetsCachedBillingOverrideReasonsAsync()
            {
                // Set up local cache mock to respond to cache request:
                //  -to "Contains" request, return "true" to indicate item is in cache
                //  -to "Get" request, return the cache item (in this case the "BILLING.OVERRIDE.REASONS" cache item)
                cacheProviderMock.Setup(x => x.Contains(domainEntityNameName, null)).Returns(true);
                cacheProviderMock.Setup(x => x.Get(domainEntityNameName, null)).Returns(allBillingOverrideReasons).Verifiable();

                // return null for request, so that if we have a result, it wasn't the data accessor that returned it.
                dataAccessorMock.Setup(acc => acc.ReadRecordAsync<ApplValcodes>("ST.VALCODES", "BILLING.OVERRIDE.REASONS", true)).ReturnsAsync(new ApplValcodes());

                // Assert the billingOverrideReasons are returned
                Assert.IsTrue((await referenceDataRepo.GetBillingOverrideReasonsAsync(false)).Count() == 3);
                // Verify that the sbillingOverrideReasons were retrieved from cache
                cacheProviderMock.Verify(m => m.Get(domainEntityNameName, null));
            }

            private StudentReferenceDataRepository BuildValidReferenceDataRepository()
            {
                // transaction factory mock
                transFactoryMock = new Mock<IColleagueTransactionFactory>();
                // Cache Provider Mock
                cacheProviderMock = new Mock<ICacheProvider>();
                // Set up data accessor for mocking 
                dataAccessorMock = new Mock<IColleagueDataReader>();

                // Set up dataAccessorMock as the object for the DataAccessor
                transFactoryMock.Setup(transFac => transFac.GetDataReader()).Returns(dataAccessorMock.Object);

                // Setup response to billingOverrideReasons domainEntityName read
                dataAccessorMock.Setup(acc => acc.ReadRecordAsync<ApplValcodes>("ST.VALCODES", "BILLING.OVERRIDE.REASONS", It.IsAny<bool>())).ReturnsAsync(billingOverrideReasonsdomainEntityNameResponse);
                cacheProviderMock.Setup<Task<Tuple<object, SemaphoreSlim>>>(x => x.GetAndLockSemaphoreAsync(It.IsAny<string>(), null))
                .ReturnsAsync(new Tuple<object, SemaphoreSlim>(null, new SemaphoreSlim(1, 1)));

                dataAccessorMock.Setup(acc => acc.SelectAsync(It.IsAny<RecordKeyLookup[]>())).Returns<RecordKeyLookup[]>(recordKeyLookups =>
                {
                    var result = new Dictionary<string, RecordKeyLookupResult>();
                    foreach (var recordKeyLookup in recordKeyLookups)
                    {
                        var billingOverrideReasons = allBillingOverrideReasons.Where(e => e.Code == recordKeyLookup.SecondaryKey).FirstOrDefault();
                        result.Add(string.Join("+", new string[] { "ST.VALCODES", "BILLING.OVERRIDE.REASONS", billingOverrideReasons.Code }),
                            new RecordKeyLookupResult() { Guid = billingOverrideReasons.Guid });
                    }
                    return Task.FromResult(result);
                });

                // Construct repository
                referenceDataRepo = new StudentReferenceDataRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object, apiSettings);

                return referenceDataRepo;
            }

            private ApplValcodes BuilddomainEntityNameResponse(IEnumerable<BillingOverrideReasons> billingOverrideReasons)
            {
                ApplValcodes billingOverrideReasonsResponse = new ApplValcodes();
                billingOverrideReasonsResponse.ValsEntityAssociation = new List<ApplValcodesVals>();
                foreach (var item in billingOverrideReasons)
                {
                    billingOverrideReasonsResponse.ValsEntityAssociation.Add(new ApplValcodesVals("", item.Description, "2", item.Code, "3", "", ""));
                }
                return billingOverrideReasonsResponse;
            }
        }

        [TestClass]
        public class CampusInvolvementRoles
        {
            Mock<IColleagueTransactionFactory> transFactoryMock;
            Mock<ICacheProvider> cacheProviderMock;
            Mock<IColleagueDataReader> dataAccessorMock;
            Mock<ILogger> loggerMock;

            IEnumerable<CampusInvRole> allCampusInvolvementRoles;
            string valcodeName;
            ApiSettings apiSettings;

            StudentReferenceDataRepository referenceDataRepo;

            [TestInitialize]
            public void Initialize()
            {
                loggerMock = new Mock<ILogger>();
                apiSettings = new ApiSettings("TEST");

                allCampusInvolvementRoles = new TestStudentReferenceDataRepository().GetCampusInvolvementRolesAsync(false).Result;

                referenceDataRepo = BuildValidReferenceDataRepository();
                valcodeName = referenceDataRepo.BuildFullCacheKey("AllRoles");
            }

            [TestCleanup]
            public void Cleanup()
            {
                allCampusInvolvementRoles = null;
                valcodeName = string.Empty;
                apiSettings = null;
            }

            [TestMethod]
            public async Task StudentReferenceDataRepository_GetCampusInvolvementRolesAsync_False()
            {
                var results = await referenceDataRepo.GetCampusInvolvementRolesAsync(false);
                Assert.AreEqual(allCampusInvolvementRoles.Count(), results.Count());

                foreach (var campusInvolvementRole in allCampusInvolvementRoles)
                {
                    var result = results.FirstOrDefault(i => i.Guid == campusInvolvementRole.Guid);

                    Assert.AreEqual(campusInvolvementRole.Code, result.Code);
                    Assert.AreEqual(campusInvolvementRole.Description, result.Description);
                    Assert.AreEqual(campusInvolvementRole.Guid, result.Guid);
                }

            }

            [TestMethod]
            public async Task StudentReferenceDataRepository_GetCampusInvolvementRolesAsync_True()
            {
                var results = await referenceDataRepo.GetCampusInvolvementRolesAsync(true);
                Assert.AreEqual(allCampusInvolvementRoles.Count(), results.Count());

                foreach (var campusInvolvementRole in allCampusInvolvementRoles)
                {
                    var result = results.FirstOrDefault(i => i.Guid == campusInvolvementRole.Guid);

                    Assert.AreEqual(campusInvolvementRole.Code, result.Code);
                    Assert.AreEqual(campusInvolvementRole.Description, result.Description);
                    Assert.AreEqual(campusInvolvementRole.Guid, result.Guid);
                }

            }

            private StudentReferenceDataRepository BuildValidReferenceDataRepository()
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

                var records = new Collection<DataContracts.Roles>();
                foreach (var item in allCampusInvolvementRoles)
                {
                    DataContracts.Roles record = new DataContracts.Roles();
                    record.RecordGuid = item.Guid;
                    record.RolesDesc = item.Description;
                    record.Recordkey = item.Code;
                    records.Add(record);
                }
                dataAccessorMock.Setup(acc => acc.BulkReadRecordAsync<DataContracts.Roles>("ROLES", "", true)).ReturnsAsync(records);

                cacheProviderMock.Setup<Task<Tuple<object, SemaphoreSlim>>>(x =>
                 x.GetAndLockSemaphoreAsync(It.IsAny<string>(), null))
                 .ReturnsAsync(new Tuple<object, SemaphoreSlim>(null, new SemaphoreSlim(1, 1)));

                dataAccessorMock.Setup(acc => acc.SelectAsync(It.IsAny<RecordKeyLookup[]>())).Returns<RecordKeyLookup[]>(recordKeyLookups =>
                {
                    var result = new Dictionary<string, RecordKeyLookupResult>();
                    foreach (var recordKeyLookup in recordKeyLookups)
                    {
                        var record = allCampusInvolvementRoles.Where(e => e.Code == recordKeyLookup.PrimaryKey).FirstOrDefault();
                        result.Add(string.Join("+", new string[] { "ROLES", record.Code }),
                            new RecordKeyLookupResult() { Guid = record.Guid });
                    }
                    return Task.FromResult(result);
                });

                // Construct repository
                referenceDataRepo = new StudentReferenceDataRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object, apiSettings);

                return referenceDataRepo;
            }
        }

        [TestClass]
        public class CampusOrganizationTypes
        {
            Mock<IColleagueTransactionFactory> transFactoryMock;
            Mock<ICacheProvider> cacheProviderMock;
            Mock<IColleagueDataReader> dataAccessorMock;
            Mock<ILogger> loggerMock;

            IEnumerable<CampusOrganizationType> allCampusOrganizationTypes;
            string valcodeName;
            ApiSettings apiSettings;

            StudentReferenceDataRepository referenceDataRepo;

            [TestInitialize]
            public void Initialize()
            {
                loggerMock = new Mock<ILogger>();
                apiSettings = new ApiSettings("TEST");

                allCampusOrganizationTypes = new TestStudentReferenceDataRepository().GetCampusOrganizationTypesAsync(false).Result;

                referenceDataRepo = BuildValidReferenceDataRepository();
                valcodeName = referenceDataRepo.BuildFullCacheKey("AllOrgTypes");
            }

            [TestCleanup]
            public void Cleanup()
            {
                allCampusOrganizationTypes = null;
                valcodeName = string.Empty;
                apiSettings = null;
            }

            [TestMethod]
            public async Task StudentReferenceDataRepository_GetCampusOrganizationTypesAsync_False()
            {
                var results = await referenceDataRepo.GetCampusOrganizationTypesAsync(false);
                Assert.AreEqual(allCampusOrganizationTypes.Count(), results.Count());

                foreach (var campusOrganizationType in allCampusOrganizationTypes)
                {
                    var result = results.FirstOrDefault(i => i.Guid == campusOrganizationType.Guid);

                    Assert.AreEqual(campusOrganizationType.Code, result.Code);
                    Assert.AreEqual(campusOrganizationType.Description, result.Description);
                    Assert.AreEqual(campusOrganizationType.Guid, result.Guid);
                }

            }

            [TestMethod]
            public async Task StudentReferenceDataRepository_GetCampusOrganizationTypesAsync_True()
            {
                var results = await referenceDataRepo.GetCampusOrganizationTypesAsync(true);
                Assert.AreEqual(allCampusOrganizationTypes.Count(), results.Count());

                foreach (var campusOrganizationType in allCampusOrganizationTypes)
                {
                    var result = results.FirstOrDefault(i => i.Guid == campusOrganizationType.Guid);

                    Assert.AreEqual(campusOrganizationType.Code, result.Code);
                    Assert.AreEqual(campusOrganizationType.Description, result.Description);
                    Assert.AreEqual(campusOrganizationType.Guid, result.Guid);
                }

            }

            private StudentReferenceDataRepository BuildValidReferenceDataRepository()
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

                var records = new Collection<DataContracts.OrgTypes>();
                foreach (var item in allCampusOrganizationTypes)
                {
                    DataContracts.OrgTypes record = new DataContracts.OrgTypes();
                    record.RecordGuid = item.Guid;
                    record.OrgtDesc = item.Description;
                    record.Recordkey = item.Code;
                    records.Add(record);
                }
                dataAccessorMock.Setup(acc => acc.BulkReadRecordAsync<DataContracts.OrgTypes>("ORG.TYPES", "", true)).ReturnsAsync(records);

                cacheProviderMock.Setup<Task<Tuple<object, SemaphoreSlim>>>(x =>
                 x.GetAndLockSemaphoreAsync(It.IsAny<string>(), null))
                 .ReturnsAsync(new Tuple<object, SemaphoreSlim>(null, new SemaphoreSlim(1, 1)));

                dataAccessorMock.Setup(acc => acc.SelectAsync(It.IsAny<RecordKeyLookup[]>())).Returns<RecordKeyLookup[]>(recordKeyLookups =>
                {
                    var result = new Dictionary<string, RecordKeyLookupResult>();
                    foreach (var recordKeyLookup in recordKeyLookups)
                    {
                        var record = allCampusOrganizationTypes.Where(e => e.Code == recordKeyLookup.PrimaryKey).FirstOrDefault();
                        result.Add(string.Join("+", new string[] { "ORG.TYPES", record.Code }),
                            new RecordKeyLookupResult() { Guid = record.Guid });
                    }
                    return Task.FromResult(result);
                });

                // Construct repository
                referenceDataRepo = new StudentReferenceDataRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object, apiSettings);

                return referenceDataRepo;
            }
        }

        [TestClass]
        public class CapSize
        {
            Mock<IColleagueTransactionFactory> transFactoryMock;
            Mock<ICacheProvider> cacheProviderMock;
            Mock<IColleagueDataReader> dataAccessorMock;
            Mock<ILogger> loggerMock;
            IEnumerable<Ellucian.Colleague.Domain.Student.Entities.CapSize> allCapSizes;
            ApplValcodes capSizesValcodeResponse;
            string valcodeName;
            ApiSettings apiSettings;
            StudentReferenceDataRepository referenceDataRepo;

            [TestInitialize]
            public void Initialize()
            {
                loggerMock = new Mock<ILogger>();

                // Build responses used for mocking
                allCapSizes = new TestStudentReferenceDataRepository().GetCapSizesAsync().Result;

                capSizesValcodeResponse = BuildValcodeResponse(allCapSizes);

                // Build repository
                referenceDataRepo = BuildValidReferenceDataRepository();
                valcodeName = referenceDataRepo.BuildFullCacheKey("ST_GRADUATION.CAP.SIZES");

                cacheProviderMock.Setup<Task<Tuple<object, SemaphoreSlim>>>(x =>
                   x.GetAndLockSemaphoreAsync(It.IsAny<string>(), null))
                   .ReturnsAsync(new Tuple<object, SemaphoreSlim>(null, new SemaphoreSlim(1, 1)));

            }

            [TestCleanup]
            public void Cleanup()
            {
                transFactoryMock = null;
                dataAccessorMock = null;
                cacheProviderMock = null;
                capSizesValcodeResponse = null;
                allCapSizes = null;
                referenceDataRepo = null;
            }

            [TestMethod]
            public async Task StudentReferenceDataRepo_GetCapSizes()
            {
                var repo = await referenceDataRepo.GetCapSizesAsync();

                for (int i = 0; i < allCapSizes.Count(); i++)
                {
                    Assert.AreEqual(allCapSizes.ElementAt(i).Code, repo.ElementAt(i).Code);
                    Assert.AreEqual(allCapSizes.ElementAt(i).Description, repo.ElementAt(i).Description);
                }
            }

            [TestMethod]
            public async Task StudentReferenceDataRepo_GetCapSizes_WritesToCache()
            {
                // Set up local cache mock to respond to cache request:
                //  -to "Contains" request, return "false" to indicate item is not in cache
                //  -to cache "Get" request, return null so we know it's reading from the "repository"
                cacheProviderMock.Setup(x => x.Contains(valcodeName, null)).Returns(false);
                cacheProviderMock.Setup(x => x.Get(valcodeName, null)).Returns(null);

                // return a valid response to the data accessor request
                dataAccessorMock.Setup(acc => acc.ReadRecordAsync<ApplValcodes>(It.IsAny<string>(), It.IsAny<string>(), true)).ReturnsAsync(capSizesValcodeResponse);

                // But after data accessor read, set up mocking so we can verify the list of CapSizes was written to the cache
                cacheProviderMock.Setup(x => x.AddAndUnlockSemaphore(valcodeName, It.IsAny<Object>(), It.IsAny<SemaphoreSlim>(), It.IsAny<CacheItemPolicy>(), null)).Verifiable();

                // Verify that CapSizes were returned, which means they came from the "repository".
                Assert.IsTrue((await referenceDataRepo.GetCapSizesAsync()).Count() == allCapSizes.Count());

                // Verify that the CapSizes was added to the cache after it was read from the repository
                cacheProviderMock.Verify(m => m.AddAndUnlockSemaphore(valcodeName, It.IsAny<Object>(), It.IsAny<SemaphoreSlim>(), It.IsAny<CacheItemPolicy>(), null));
            }

            [TestMethod]
            public async Task StudentReferenceDataRepo_GetCapSizes_Cached()
            {
                object lockHandle = null;
                // Set up local cache mock to respond to cache request:
                //  -to "Contains" request, return "false" to indicate item is not in cache
                //  -to cache "Get" request, return null so we know it's getting data from "repository"
                cacheProviderMock.Setup(x => x.Contains(valcodeName, null)).Returns(false);
                cacheProviderMock.Setup(x => x.GetAndLock(valcodeName, out lockHandle, null)).Returns(null);

                // Get these codes to populate the cache
                var capSizes = (await referenceDataRepo.GetCapSizesAsync());
                // Assert the cap sizes are returned
                Assert.IsTrue((await referenceDataRepo.GetCapSizesAsync()).Count() == allCapSizes.Count());
                // Make sure we can verify that it's in the cache
                cacheProviderMock.Setup(x => x.Contains(valcodeName, null)).Returns(true);
                var capSizeTuple = new Tuple<object, SemaphoreSlim>(capSizes, new SemaphoreSlim(1, 1));
                cacheProviderMock.Setup(x => x.GetAndLockSemaphoreAsync(valcodeName, null)).ReturnsAsync(capSizeTuple).Verifiable();

                // Verify that the cap sizes are now retrieved from cache
                capSizes = (await referenceDataRepo.GetCapSizesAsync());
                cacheProviderMock.Verify(m => m.GetAndLockSemaphoreAsync(valcodeName, null));
            }

            private T GetCache<T>(ICacheProvider cacheProvider, string key)
                where T : class
            {
                object cache = cacheProvider.Get(key, null);
                return cache as T;
            }

            private StudentReferenceDataRepository BuildValidReferenceDataRepository()
            {
                // transaction factory mock
                transFactoryMock = new Mock<IColleagueTransactionFactory>();
                // Cache Mock
                //localCacheMock = new Mock<ObjectCache>();
                // Cache Provider Mock
                cacheProviderMock = new Mock<ICacheProvider>();
                // Set up data accessor for mocking 
                dataAccessorMock = new Mock<IColleagueDataReader>();
                apiSettings = new ApiSettings("TEST");

                // Set up dataAccessorMock as the object for the DataAccessor
                transFactoryMock.Setup(transFac => transFac.GetDataReader()).Returns(dataAccessorMock.Object);

                // Setup response to cap size valcode read
                dataAccessorMock.Setup(acc => acc.ReadRecordAsync<ApplValcodes>("ST.VALCODES", "GRADUATION.CAP.SIZES", true)).ReturnsAsync(capSizesValcodeResponse);

                // Construct repository
                referenceDataRepo = new StudentReferenceDataRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object, apiSettings);

                return referenceDataRepo;
            }

            private ApplValcodes BuildValcodeResponse(IEnumerable<Ellucian.Colleague.Domain.Student.Entities.CapSize> capSizes)
            {
                ApplValcodes applValcodeResponse = new ApplValcodes();
                applValcodeResponse.ValsEntityAssociation = new List<ApplValcodesVals>();
                foreach (var item in capSizes)
                {
                    applValcodeResponse.ValsEntityAssociation.Add(new ApplValcodesVals("", item.Description, "", item.Code, "", "", ""));
                }
                return applValcodeResponse;
            }
        }

        [TestClass]
        public class CareerGoals
        {
            Mock<IColleagueTransactionFactory> transFactoryMock;
            //Mock<ObjectCache> localCacheMock;
            Mock<ICacheProvider> cacheProviderMock;
            Mock<IColleagueDataReader> dataAccessorMock;
            Mock<ILogger> loggerMock;
            IEnumerable<Ellucian.Colleague.Domain.Student.Entities.CareerGoal> allCareerGoals;
            ApplValcodes careerGoalsValcodeResponse;
            string valcodeName;
            ApiSettings apiSettings;
            StudentReferenceDataRepository referenceDataRepo;

            [TestInitialize]
            public void Initialize()
            {
                loggerMock = new Mock<ILogger>();

                // Build responses used for mocking
                allCareerGoals = new TestStudentReferenceDataRepository().GetCareerGoalsAsync().Result;

                careerGoalsValcodeResponse = BuildValcodeResponse(allCareerGoals);

                // Build repository
                referenceDataRepo = BuildValidReferenceDataRepository();
                valcodeName = referenceDataRepo.BuildFullCacheKey("ST_CAREER.GOALS");

                cacheProviderMock.Setup<Task<Tuple<object, SemaphoreSlim>>>(x =>
                   x.GetAndLockSemaphoreAsync(It.IsAny<string>(), null))
                   .ReturnsAsync(new Tuple<object, SemaphoreSlim>(null, new SemaphoreSlim(1, 1)));

            }

            [TestCleanup]
            public void Cleanup()
            {
                transFactoryMock = null;
                dataAccessorMock = null;
                cacheProviderMock = null;
                careerGoalsValcodeResponse = null;
                allCareerGoals = null;
                referenceDataRepo = null;
            }

            [TestMethod]
            public async Task StudentReferenceDataRepo_GetsCareerGoals()
            {
                var repoCareerGoals = await referenceDataRepo.GetCareerGoalsAsync(); ;

                for (int i = 0; i < allCareerGoals.Count(); i++)
                {
                    Assert.AreEqual(allCareerGoals.ElementAt(i).Code, repoCareerGoals.ElementAt(i).Code);
                    Assert.AreEqual(allCareerGoals.ElementAt(i).Description, repoCareerGoals.ElementAt(i).Description);
                }
            }

            [TestMethod]
            public async Task StudentReferenceDataRepo_GetCareerGoals_WritesToCache()
            {
                // Set up local cache mock to respond to cache request:
                //  -to "Contains" request, return "false" to indicate item is not in cache
                //  -to cache "Get" request, return null so we know it's reading from the "repository"
                cacheProviderMock.Setup(x => x.Contains(valcodeName, null)).Returns(false);
                cacheProviderMock.Setup(x => x.Get(valcodeName, null)).Returns(null);

                // return a valid response to the data accessor request
                dataAccessorMock.Setup(acc => acc.ReadRecordAsync<ApplValcodes>(It.IsAny<string>(), It.IsAny<string>(), true)).ReturnsAsync(careerGoalsValcodeResponse);

                // But after data accessor read, set up mocking so we can verify the list of career goals was written to the cache
                cacheProviderMock.Setup(x => x.AddAndUnlockSemaphore(valcodeName, It.IsAny<Object>(), It.IsAny<SemaphoreSlim>(), It.IsAny<CacheItemPolicy>(), null)).Verifiable();

                // Verify that career goals were returned, which means they came from the "repository".
                Assert.IsTrue((await referenceDataRepo.GetCareerGoalsAsync()).Count() == allCareerGoals.Count());

                // Verify that the career goals was added to the cache after it was read from the repository
                cacheProviderMock.Verify(m => m.AddAndUnlockSemaphore(valcodeName, It.IsAny<Object>(), It.IsAny<SemaphoreSlim>(), It.IsAny<CacheItemPolicy>(), null));
            }

            [TestMethod]
            public async Task StudentReferenceDataRepo_GetCareerGoals_Cached()
            {
                object lockHandle = null;
                // Set up local cache mock to respond to cache request:
                //  -to "Contains" request, return "false" to indicate item is not in cache
                //  -to cache "Get" request, return null so we know it's getting data from "repository"
                cacheProviderMock.Setup(x => x.Contains(valcodeName, null)).Returns(false);
                cacheProviderMock.Setup(x => x.GetAndLock(valcodeName, out lockHandle, null)).Returns(null);

                // Get these codes to populate the cache
                var careerGoals = (await referenceDataRepo.GetCareerGoalsAsync());
                // Assert the types are returned
                Assert.IsTrue((await referenceDataRepo.GetCareerGoalsAsync()).Count() == allCareerGoals.Count());
                // Make sure we can verify that it's in the cache
                cacheProviderMock.Setup(x => x.Contains(valcodeName, null)).Returns(true);
                var careerGoalsTuple = new Tuple<object, SemaphoreSlim>(careerGoals, new SemaphoreSlim(1, 1));
                cacheProviderMock.Setup(x => x.GetAndLockSemaphoreAsync(valcodeName, null)).ReturnsAsync(careerGoalsTuple).Verifiable();

                // Verify that the types are now retrieved from cache
                careerGoals = (await referenceDataRepo.GetCareerGoalsAsync());
                cacheProviderMock.Verify(m => m.GetAndLockSemaphoreAsync(valcodeName, null));
            }

            private T GetCache<T>(ICacheProvider cacheProvider, string key)
                where T : class
            {
                object cache = cacheProvider.Get(key, null);
                return cache as T;
            }

            private StudentReferenceDataRepository BuildValidReferenceDataRepository()
            {
                // transaction factory mock
                transFactoryMock = new Mock<IColleagueTransactionFactory>();
                // Cache Mock
                //localCacheMock = new Mock<ObjectCache>();
                // Cache Provider Mock
                cacheProviderMock = new Mock<ICacheProvider>();
                // Set up data accessor for mocking 
                dataAccessorMock = new Mock<IColleagueDataReader>();
                apiSettings = new ApiSettings("TEST");

                // Set up dataAccessorMock as the object for the DataAccessor
                transFactoryMock.Setup(transFac => transFac.GetDataReader()).Returns(dataAccessorMock.Object);

                // Setup response to career goals valcode read
                dataAccessorMock.Setup(acc => acc.ReadRecordAsync<ApplValcodes>("ST.VALCODES", "CAREER.GOALS", true)).ReturnsAsync(careerGoalsValcodeResponse);

                // Construct repository
                referenceDataRepo = new StudentReferenceDataRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object, apiSettings);

                return referenceDataRepo;
            }

            private ApplValcodes BuildValcodeResponse(IEnumerable<Ellucian.Colleague.Domain.Student.Entities.CareerGoal> careerGoal)
            {
                ApplValcodes applValcodeResponse = new ApplValcodes();
                applValcodeResponse.ValsEntityAssociation = new List<ApplValcodesVals>();
                foreach (var item in careerGoal)
                {
                    applValcodeResponse.ValsEntityAssociation.Add(new ApplValcodesVals("", item.Description, "", item.Code, "", "", ""));
                }
                return applValcodeResponse;
            }
        }

        [TestClass]
        public class ChargeAssessmentMethodsTests
        {
            Mock<IColleagueTransactionFactory> transFactoryMock;
            Mock<ICacheProvider> cacheProviderMock;
            Mock<IColleagueDataReader> dataAccessorMock;
            Mock<ILogger> loggerMock;
            IEnumerable<ChargeAssessmentMethod> allChargeAssessmentMethods;
            ApplValcodes chargeAssessmentMethodsValcodeResponse;
            string domainEntityNameName;
            ApiSettings apiSettings;

            Mock<IStudentReferenceDataRepository> referenceDataRepositoryMock;
            IStudentReferenceDataRepository referenceDataRepository;
            StudentReferenceDataRepository referenceDataRepo;

            [TestInitialize]
            public void Initialize()
            {
                loggerMock = new Mock<ILogger>();
                apiSettings = new ApiSettings("TEST");

                allChargeAssessmentMethods = new TestStudentReferenceDataRepository().GetChargeAssessmentMethodsAsync(false).Result;
                chargeAssessmentMethodsValcodeResponse = BuildValcodeResponse(allChargeAssessmentMethods);
                var chargeAssessmentMethodsValResponse = new List<string>() { "2" };
                chargeAssessmentMethodsValcodeResponse.ValActionCode1 = chargeAssessmentMethodsValResponse;

                referenceDataRepositoryMock = new Mock<IStudentReferenceDataRepository>();
                referenceDataRepository = referenceDataRepositoryMock.Object;

                referenceDataRepo = BuildValidReferenceDataRepository();
                domainEntityNameName = referenceDataRepo.BuildFullCacheKey("ST_BILLING.METHODS_GUID");

                cacheProviderMock.Setup<Task<Tuple<object, SemaphoreSlim>>>(x =>
                   x.GetAndLockSemaphoreAsync(It.IsAny<string>(), null))
                   .ReturnsAsync(new Tuple<object, SemaphoreSlim>(null, new SemaphoreSlim(1, 1)));
            }

            [TestCleanup]
            public void Cleanup()
            {
                transFactoryMock = null;
                dataAccessorMock = null;
                cacheProviderMock = null;
                chargeAssessmentMethodsValcodeResponse = null;
                allChargeAssessmentMethods = null;
                referenceDataRepo = null;
            }


            [TestMethod]
            public async Task StudentReferenceDataRepo_GetsChargeAssessmentMethodsCacheAsync()
            {
                var chargeAssessmentMethods = await referenceDataRepo.GetChargeAssessmentMethodsAsync(false);

                for (int i = 0; i < allChargeAssessmentMethods.Count(); i++)
                {
                    Assert.AreEqual(allChargeAssessmentMethods.ElementAt(i).Code, chargeAssessmentMethods.ElementAt(i).Code);
                    Assert.AreEqual(allChargeAssessmentMethods.ElementAt(i).Description, chargeAssessmentMethods.ElementAt(i).Description);
                }
            }

            [TestMethod]
            public async Task StudentReferenceDataRepo_GetsChargeAssessmentMethodsNonCacheAsync()
            {
                var statuses = await referenceDataRepo.GetChargeAssessmentMethodsAsync(true);

                for (int i = 0; i < allChargeAssessmentMethods.Count(); i++)
                {
                    Assert.AreEqual(allChargeAssessmentMethods.ElementAt(i).Code, statuses.ElementAt(i).Code);
                    Assert.AreEqual(allChargeAssessmentMethods.ElementAt(i).Description, statuses.ElementAt(i).Description);
                }
            }

            [TestMethod]
            public async Task StudentReferenceDataRepo_GetChargeAssessmentMethods_WritesToCacheAsync()
            {

                // Set up local cache mock to respond to cache request:
                //  -to "Contains" request, return "false" to indicate item is not in cache
                //  -to cache "Get" request, return null so we know it's reading from the "repository"
                cacheProviderMock.Setup(x => x.Contains(domainEntityNameName, null)).Returns(false);
                cacheProviderMock.Setup(x => x.Get(domainEntityNameName, null)).Returns(null);

                // return a valid response to the data accessor request
                dataAccessorMock.Setup(acc => acc.ReadRecordAsync<ApplValcodes>("ST.VALCODES", "BILLING.METHODS", It.IsAny<bool>())).ReturnsAsync(chargeAssessmentMethodsValcodeResponse);

                cacheProviderMock.Setup<Task<Tuple<object, SemaphoreSlim>>>(x =>
                 x.GetAndLockSemaphoreAsync(It.IsAny<string>(), null))
                 .ReturnsAsync(new Tuple<object, SemaphoreSlim>(null, new SemaphoreSlim(1, 1)));

                // But after data accessor read, set up mocking so we can verify the list of chargeAssessmentMethods was written to the cache
                cacheProviderMock.Setup(x => x.Add(It.IsAny<string>(), It.IsAny<Task<List<ChargeAssessmentMethod>>>(), It.IsAny<CacheItemPolicy>(), null)).Verifiable();

                cacheProviderMock.Setup(x => x.Contains(referenceDataRepo.BuildFullCacheKey("ST_BILLING.METHODS"), null)).Returns(true);
                var chargeAssessmentMethods = await referenceDataRepo.GetChargeAssessmentMethodsAsync(false);
                cacheProviderMock.Setup(x => x.Get(referenceDataRepo.BuildFullCacheKey("ST_BILLING.METHODS"), null)).Returns(chargeAssessmentMethods);
                // Verify that chargeAssessmentMethods were returned, which means they came from the "repository".
                Assert.IsTrue(chargeAssessmentMethods.Count() == 3);

                // Verify that the chargeAssessmentMethods item was added to the cache after it was read from the repository
                cacheProviderMock.Verify(m => m.Add(It.IsAny<string>(), It.IsAny<Task<List<ChargeAssessmentMethod>>>(), It.IsAny<CacheItemPolicy>(), null), Times.Never);

            }

            [TestMethod]
            public async Task StudentReferenceDataRepo_GetChargeAssessmentMethods_GetsCachedChargeAssessmentMethodsAsync()
            {
                // Set up local cache mock to respond to cache request:
                //  -to "Contains" request, return "true" to indicate item is in cache
                //  -to "Get" request, return the cache item (in this case the "BILLING.METHODS" cache item)
                cacheProviderMock.Setup(x => x.Contains(domainEntityNameName, null)).Returns(true);
                cacheProviderMock.Setup(x => x.Get(domainEntityNameName, null)).Returns(allChargeAssessmentMethods).Verifiable();

                // return null for request, so that if we have a result, it wasn't the data accessor that returned it.
                dataAccessorMock.Setup(acc => acc.ReadRecordAsync<ApplValcodes>("ST.VALCODES", "BILLING.METHODS", true)).ReturnsAsync(new ApplValcodes());

                // Assert the chargeAssessmentMethods are returned
                Assert.IsTrue((await referenceDataRepo.GetChargeAssessmentMethodsAsync(false)).Count() == 3);
                // Verify that the schargeAssessmentMethods were retrieved from cache
                cacheProviderMock.Verify(m => m.Get(domainEntityNameName, null));
            }

            private StudentReferenceDataRepository BuildValidReferenceDataRepository()
            {
                // transaction factory mock
                transFactoryMock = new Mock<IColleagueTransactionFactory>();
                // Cache Provider Mock
                cacheProviderMock = new Mock<ICacheProvider>();
                // Set up data accessor for mocking 
                dataAccessorMock = new Mock<IColleagueDataReader>();

                // Set up dataAccessorMock as the object for the DataAccessor
                transFactoryMock.Setup(transFac => transFac.GetDataReader()).Returns(dataAccessorMock.Object);

                // Setup response to chargeAssessmentMethods domainEntityName read
                dataAccessorMock.Setup(acc => acc.ReadRecordAsync<ApplValcodes>("ST.VALCODES", "BILLING.METHODS", It.IsAny<bool>())).ReturnsAsync(chargeAssessmentMethodsValcodeResponse);
                cacheProviderMock.Setup<Task<Tuple<object, SemaphoreSlim>>>(x => x.GetAndLockSemaphoreAsync(It.IsAny<string>(), null))
                .ReturnsAsync(new Tuple<object, SemaphoreSlim>(null, new SemaphoreSlim(1, 1)));

                dataAccessorMock.Setup(acc => acc.SelectAsync(It.IsAny<RecordKeyLookup[]>())).Returns<RecordKeyLookup[]>(recordKeyLookups =>
                {
                    var result = new Dictionary<string, RecordKeyLookupResult>();
                    foreach (var recordKeyLookup in recordKeyLookups)
                    {
                        var chargeAssessmentMethods = allChargeAssessmentMethods.Where(e => e.Code == recordKeyLookup.SecondaryKey).FirstOrDefault();
                        result.Add(string.Join("+", new string[] { "ST.VALCODES", "BILLING.METHODS", chargeAssessmentMethods.Code }),
                            new RecordKeyLookupResult() { Guid = chargeAssessmentMethods.Guid });
                    }
                    return Task.FromResult(result);
                });

                // Construct repository
                referenceDataRepo = new StudentReferenceDataRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object, apiSettings);

                return referenceDataRepo;
            }

            private ApplValcodes BuildValcodeResponse(IEnumerable<ChargeAssessmentMethod> chargeAssessmentMethods)
            {
                ApplValcodes chargeAssessmentMethodsResponse = new ApplValcodes();
                chargeAssessmentMethodsResponse.ValsEntityAssociation = new List<ApplValcodesVals>();
                foreach (var item in chargeAssessmentMethods)
                {
                    chargeAssessmentMethodsResponse.ValsEntityAssociation.Add(new ApplValcodesVals("", item.Description, "2", item.Code, "3", "", ""));
                }
                return chargeAssessmentMethodsResponse;
            }
        }

        [TestClass]
        public class CourseLevels
        {
            Mock<IColleagueTransactionFactory> transFactoryMock;
            Mock<ICacheProvider> cacheProviderMock;
            Mock<IColleagueDataReader> dataAccessorMock;
            Mock<ILogger> loggerMock;
            IEnumerable<CourseLevel> allCourseLevels;
            ApplValcodes courseLevelValcodeResponse;
            string valcodeName;
            ApiSettings apiSettings;

            Mock<IStudentReferenceDataRepository> referenceDataRepositoryMock;
            IStudentReferenceDataRepository referenceDataRepository;
            StudentReferenceDataRepository referenceDataRepo;

            [TestInitialize]
            public void Initialize()
            {
                loggerMock = new Mock<ILogger>();
                apiSettings = new ApiSettings("TEST");

                // Build Courses responses used for mocking
                allCourseLevels = new TestCourseLevelRepository().Get();
                courseLevelValcodeResponse = BuildValcodeResponse(allCourseLevels);
                var courseLevelValResponse = new List<string>() { "2" };
                courseLevelValcodeResponse.ValActionCode1 = courseLevelValResponse;

                referenceDataRepositoryMock = new Mock<IStudentReferenceDataRepository>();
                referenceDataRepository = referenceDataRepositoryMock.Object;

                // Build course level repository
                referenceDataRepo = BuildValidReferenceDataRepository();
                valcodeName = referenceDataRepo.BuildFullCacheKey("ST_COURSE.LEVELS_GUID");

                cacheProviderMock.Setup<Task<Tuple<object, SemaphoreSlim>>>(x =>
                   x.GetAndLockSemaphoreAsync(It.IsAny<string>(), null))
                   .ReturnsAsync(new Tuple<object, SemaphoreSlim>(null, new SemaphoreSlim(1, 1)));
            }

            [TestCleanup]
            public void Cleanup()
            {
                transFactoryMock = null;
                dataAccessorMock = null;
                cacheProviderMock = null;
                courseLevelValcodeResponse = null;
                allCourseLevels = null;
                referenceDataRepo = null;
            }

            [TestMethod]
            public async Task StudentReferenceDataRepo_GetsCourseLevelsNoArgAsync()
            {
                var courseLevels = await referenceDataRepo.GetCourseLevelsAsync();

                for (int i = 0; i < allCourseLevels.Count(); i++)
                {
                    Assert.AreEqual(allCourseLevels.ElementAt(i).Code, courseLevels.ElementAt(i).Code);
                    Assert.AreEqual(allCourseLevels.ElementAt(i).Description, courseLevels.ElementAt(i).Description);
                }
            }

            [TestMethod]
            public async Task StudentReferenceDataRepo_GetsCourseLevelsCacheAsync()
            {
                var courseLevels = await referenceDataRepo.GetCourseLevelsAsync(false);

                for (int i = 0; i < allCourseLevels.Count(); i++)
                {
                    Assert.AreEqual(allCourseLevels.ElementAt(i).Code, courseLevels.ElementAt(i).Code);
                    Assert.AreEqual(allCourseLevels.ElementAt(i).Description, courseLevels.ElementAt(i).Description);
                }
            }

            [TestMethod]
            public async Task StudentReferenceDataRepo_GetsCourseLevelsNonCacheAsync()
            {
                var statuses = await referenceDataRepo.GetCourseLevelsAsync(true);

                for (int i = 0; i < allCourseLevels.Count(); i++)
                {
                    Assert.AreEqual(allCourseLevels.ElementAt(i).Code, statuses.ElementAt(i).Code);
                    Assert.AreEqual(allCourseLevels.ElementAt(i).Description, statuses.ElementAt(i).Description);
                }
            }

            [TestMethod]
            public async Task StudentReferenceDataRepo_GetCourseLevels_WritesToCacheAsync()
            {

                // Set up local cache mock to respond to cache request:
                //  -to "Contains" request, return "false" to indicate item is not in cache
                //  -to cache "Get" request, return null so we know it's reading from the "repository"
                cacheProviderMock.Setup(x => x.Contains(valcodeName, null)).Returns(false);
                cacheProviderMock.Setup(x => x.Get(valcodeName, null)).Returns(null);

                // return a valid response to the data accessor request
                dataAccessorMock.Setup(acc => acc.ReadRecordAsync<ApplValcodes>("ST.VALCODES", "COURSE.LEVELS", It.IsAny<bool>())).ReturnsAsync(courseLevelValcodeResponse);

                cacheProviderMock.Setup<Task<Tuple<object, SemaphoreSlim>>>(x =>
                 x.GetAndLockSemaphoreAsync(It.IsAny<string>(), null))
                 .ReturnsAsync(new Tuple<object, SemaphoreSlim>(null, new SemaphoreSlim(1, 1)));

                // But after data accessor read, set up mocking so we can verify the list of courseLevels was written to the cache
                cacheProviderMock.Setup(x => x.Add(It.IsAny<string>(), It.IsAny<Task<List<CourseLevel>>>(), It.IsAny<CacheItemPolicy>(), null)).Verifiable();

                cacheProviderMock.Setup(x => x.Contains(referenceDataRepo.BuildFullCacheKey("ST_COURSE.LEVELS"), null)).Returns(true);
                var courseLevels = await referenceDataRepo.GetCourseLevelsAsync(false);
                cacheProviderMock.Setup(x => x.Get(referenceDataRepo.BuildFullCacheKey("ST_COURSE.LEVELS"), null)).Returns(courseLevels);
                // Verify that courseLevels were returned, which means they came from the "repository".
                Assert.IsTrue(courseLevels.Count() == 5);

                // Verify that the courseLevel item was added to the cache after it was read from the repository
                cacheProviderMock.Verify(m => m.Add(It.IsAny<string>(), It.IsAny<Task<List<CourseLevel>>>(), It.IsAny<CacheItemPolicy>(), null), Times.Never);

            }

            [TestMethod]
            public async Task StudentReferenceDataRepo_GetCourseLevels_GetsCachedCourseLevelsAsync()
            {
                // Set up local cache mock to respond to cache request:
                //  -to "Contains" request, return "true" to indicate item is in cache
                //  -to "Get" request, return the cache item (in this case the "COURSE.LEVELS" cache item)
                cacheProviderMock.Setup(x => x.Contains(valcodeName, null)).Returns(true);
                cacheProviderMock.Setup(x => x.Get(valcodeName, null)).Returns(allCourseLevels).Verifiable();

                // return null for request, so that if we have a result, it wasn't the data accessor that returned it.
                dataAccessorMock.Setup(acc => acc.ReadRecordAsync<ApplValcodes>("ST.VALCODES", "COURSE.LEVELS", true)).ReturnsAsync(new ApplValcodes());

                // Assert the courseLevels are returned
                Assert.IsTrue((await referenceDataRepo.GetCourseLevelsAsync(false)).Count() == 5);
                // Verify that the sectionRegistrationStatuses were retrieved from cache
                cacheProviderMock.Verify(m => m.Get(valcodeName, null));
            }

            private StudentReferenceDataRepository BuildValidReferenceDataRepository()
            {
                // transaction factory mock
                transFactoryMock = new Mock<IColleagueTransactionFactory>();
                // Cache Provider Mock
                cacheProviderMock = new Mock<ICacheProvider>();
                // Set up data accessor for mocking 
                dataAccessorMock = new Mock<IColleagueDataReader>();

                // Set up dataAccessorMock as the object for the DataAccessor
                transFactoryMock.Setup(transFac => transFac.GetDataReader()).Returns(dataAccessorMock.Object);

                // Setup response to courseLevel valcode read
                dataAccessorMock.Setup(acc => acc.ReadRecordAsync<ApplValcodes>("ST.VALCODES", "COURSE.LEVELS", It.IsAny<bool>())).ReturnsAsync(courseLevelValcodeResponse);
                cacheProviderMock.Setup<Task<Tuple<object, SemaphoreSlim>>>(x => x.GetAndLockSemaphoreAsync(It.IsAny<string>(), null))
                .ReturnsAsync(new Tuple<object, SemaphoreSlim>(null, new SemaphoreSlim(1, 1)));

                dataAccessorMock.Setup(acc => acc.SelectAsync(It.IsAny<RecordKeyLookup[]>())).Returns<RecordKeyLookup[]>(recordKeyLookups =>
                {
                    var result = new Dictionary<string, RecordKeyLookupResult>();
                    foreach (var recordKeyLookup in recordKeyLookups)
                    {
                        var courseLevel = allCourseLevels.Where(e => e.Code == recordKeyLookup.SecondaryKey).FirstOrDefault();
                        result.Add(string.Join("+", new string[] { "ST.VALCODES", "COURSE.LEVELS", courseLevel.Code }),
                            new RecordKeyLookupResult() { Guid = courseLevel.Guid });
                    }
                    return Task.FromResult(result);
                });

                // Construct repository
                referenceDataRepo = new StudentReferenceDataRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object, apiSettings);

                return referenceDataRepo;
            }

            private ApplValcodes BuildValcodeResponse(IEnumerable<CourseLevel> courseLevels)
            {
                ApplValcodes courseLevelsResponse = new ApplValcodes();
                courseLevelsResponse.ValsEntityAssociation = new List<ApplValcodesVals>();
                foreach (var item in courseLevels)
                {
                    courseLevelsResponse.ValsEntityAssociation.Add(new ApplValcodesVals("", item.Description, "2", item.Code, "3", "", ""));
                }
                return courseLevelsResponse;
            }
        }

        [TestClass]
        public class CourseStatuses
        {
            Mock<IColleagueTransactionFactory> transFactoryMock;
            Mock<ICacheProvider> cacheProviderMock;
            Mock<IColleagueDataReader> dataAccessorMock;
            Mock<ILogger> loggerMock;
            IEnumerable<Domain.Student.Entities.CourseStatuses> allCourseStatuses;
            ApplValcodes courseStatusItemValcodeResponse;
            string valcodeName;
            ApiSettings apiSettings;

            StudentReferenceDataRepository referenceDataRepo;

            [TestInitialize]
            public void Initialize()
            {
                loggerMock = new Mock<ILogger>();
                apiSettings = new ApiSettings("TEST");

                // Build course statuses responses used for mocking
                allCourseStatuses = new TestStudentReferenceDataRepository().GetCourseStatusesAsync(false).Result;
                courseStatusItemValcodeResponse = BuildValcodeResponse(allCourseStatuses);

                // Build repository
                referenceDataRepo = BuildValidReferenceDataRepository();
                valcodeName = referenceDataRepo.BuildFullCacheKey("ST_COURSE.STATUSES_GUID");
            }

            [TestCleanup]
            public void Cleanup()
            {
                transFactoryMock = null;
                dataAccessorMock = null;
                cacheProviderMock = null;
                courseStatusItemValcodeResponse = null;
                allCourseStatuses = null;
                referenceDataRepo = null;
            }

            [TestMethod]
            public async Task StudentReferenceDataRepo_GetCourseStatuses()
            {
                var courseStatuses = await referenceDataRepo.GetCourseStatusesAsync(false);
                for (int i = 0; i < courseStatuses.Count(); i++)
                {
                    Assert.AreEqual(allCourseStatuses.ElementAt(i).Guid, courseStatuses.ElementAt(i).Guid);
                    Assert.AreEqual(allCourseStatuses.ElementAt(i).Code, courseStatuses.ElementAt(i).Code);
                    Assert.AreEqual(allCourseStatuses.ElementAt(i).Description, courseStatuses.ElementAt(i).Description);
                    Assert.AreEqual(allCourseStatuses.ElementAt(i).Status, courseStatuses.ElementAt(i).Status);
                
                }
            }


            [TestMethod]
            public async Task StudentReferenceDataRepo_GetCourseStatuses_Cache()
            {
                var courseStatuses = await referenceDataRepo.GetCourseStatusesAsync(false);
                for (int i = 0; i < courseStatuses.Count(); i++)
                {
                    Assert.AreEqual(allCourseStatuses.ElementAt(i).Code, courseStatuses.ElementAt(i).Code);
                    Assert.AreEqual(allCourseStatuses.ElementAt(i).Description, courseStatuses.ElementAt(i).Description);
                }
            }

            [TestMethod]
            public async Task StudentReferenceDataRepo_GetCourseStatuses_NonCache()
            {
                var courseStatuses = await referenceDataRepo.GetCourseStatusesAsync(true);
                for (int i = 0; i < courseStatuses.Count(); i++)
                {
                    Assert.AreEqual(allCourseStatuses.ElementAt(i).Code, courseStatuses.ElementAt(i).Code);
                    Assert.AreEqual(allCourseStatuses.ElementAt(i).Description, courseStatuses.ElementAt(i).Description);
                }
            }

            [TestMethod]
            public async Task StudentReferenceDataRepo_GetCourseStatuses_WritesToCacheAsync()
            {

                // Set up local cache mock to respond to cache request:
                //  -to "Contains" request, return "false" to indicate item is not in cache
                //  -to cache "Get" request, return null so we know it's reading from the "repository"
                cacheProviderMock.Setup(x => x.Contains(valcodeName, null)).Returns(false);
                cacheProviderMock.Setup(x => x.Get(valcodeName, null)).Returns(null);

                // return a valid response to the data accessor request
                dataAccessorMock.Setup(acc => acc.ReadRecordAsync<ApplValcodes>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(courseStatusItemValcodeResponse);

                cacheProviderMock.Setup<Task<Tuple<object, SemaphoreSlim>>>(x =>
                 x.GetAndLockSemaphoreAsync(It.IsAny<string>(), null))
                 .ReturnsAsync(new Tuple<object, SemaphoreSlim>(null, new SemaphoreSlim(1, 1)));

                // But after data accessor read, set up mocking so we can verify the list of course status was written to the cache
                cacheProviderMock.Setup(x => x.Add(It.IsAny<string>(), It.IsAny<Task<IEnumerable<CourseStatuses>>>(), It.IsAny<CacheItemPolicy>(), null)).Verifiable();

                cacheProviderMock.Setup(x => x.Contains(referenceDataRepo.BuildFullCacheKey("ST_COURSE.STATUSES"), null)).Returns(true);
                var courseStatuses = await referenceDataRepo.GetCourseStatusesAsync(false);
                cacheProviderMock.Setup(x => x.Get(referenceDataRepo.BuildFullCacheKey("ST_COURSE.STATUSES"), null)).Returns(courseStatuses);
                // Verify that course status were returned, which means they came from the "repository".
                Assert.IsTrue(courseStatuses.Count() == allCourseStatuses.Count());

                // Verify that the course status was added to the cache after it was read from the repository
                cacheProviderMock.Verify(m => m.Add(It.IsAny<string>(), It.IsAny<Task<IEnumerable<CourseStatuses>>>(), It.IsAny<CacheItemPolicy>(), null), Times.Never);

            }

            [TestMethod]
            public async Task StudentReferenceDataRepo_GetCourseStatuses_CachedCourseStatuses()
            {
                // Set up local cache mock to respond to cache request:
                //  -to "Contains" request, return "true" to indicate item is in cache
                //  -to "Get" request, return the cache item (in this case the "COURSE.STATUSES" cache item)
                cacheProviderMock.Setup(x => x.Contains(valcodeName, null)).Returns(true);
                cacheProviderMock.Setup(x => x.Get(valcodeName, null)).Returns(allCourseStatuses).Verifiable();

                // return null for request, so that if we have a result, it wasn't the data accessor that returned it.
                dataAccessorMock.Setup(acc => acc.ReadRecordAsync<ApplValcodes>("ST.VALCODES", "COURSE.STATUSES", true)).ReturnsAsync(new ApplValcodes());

                // Assert the CourseStatuses are returned
                Assert.IsTrue((await referenceDataRepo.GetCourseStatusesAsync(false)).Count() == allCourseStatuses.Count());
                // Verify that the CourseStatuses were retrieved from cache
                cacheProviderMock.Verify(m => m.Get(valcodeName, null));
            }

            private StudentReferenceDataRepository BuildValidReferenceDataRepository()
            {
                // transaction factory mock
                transFactoryMock = new Mock<IColleagueTransactionFactory>();
                // Cache Provider Mock
                cacheProviderMock = new Mock<ICacheProvider>();
                // Set up data accessor for mocking 
                dataAccessorMock = new Mock<IColleagueDataReader>();

                // Set up dataAccessorMock as the object for the DataAccessor
                transFactoryMock.Setup(transFac => transFac.GetDataReader()).Returns(dataAccessorMock.Object);

                // Setup response to course status valcode read
                dataAccessorMock.Setup(acc => acc.ReadRecordAsync<ApplValcodes>("ST.VALCODES", "COURSE.STATUSES", It.IsAny<bool>())).ReturnsAsync(courseStatusItemValcodeResponse);
                cacheProviderMock.Setup<Task<Tuple<object, SemaphoreSlim>>>(x => x.GetAndLockSemaphoreAsync(It.IsAny<string>(), null))
                .ReturnsAsync(new Tuple<object, SemaphoreSlim>(null, new SemaphoreSlim(1, 1)));

                dataAccessorMock.Setup(acc => acc.SelectAsync(It.IsAny<RecordKeyLookup[]>())).Returns<RecordKeyLookup[]>(recordKeyLookups =>
                {
                    var result = new Dictionary<string, RecordKeyLookupResult>();
                    foreach (var recordKeyLookup in recordKeyLookups)
                    {
                        var enrollmentStatus = allCourseStatuses.Where(e => e.Code == recordKeyLookup.SecondaryKey).FirstOrDefault();
                        result.Add(string.Join("+", new string[] { "ST.VALCODES", "COURSE.STATUSES", enrollmentStatus.Code }),
                            new RecordKeyLookupResult() { Guid = enrollmentStatus.Guid });
                    }
                    return Task.FromResult(result);
                });

                // Construct repository
                referenceDataRepo = new StudentReferenceDataRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object, apiSettings);

                return referenceDataRepo;
            }

            private ApplValcodes BuildValcodeResponse(IEnumerable<Domain.Student.Entities.CourseStatuses> courseStatusItems)
            {
                ApplValcodes valcodeResponse = new ApplValcodes();
                valcodeResponse.ValsEntityAssociation = new List<ApplValcodesVals>();
                foreach (var item in courseStatusItems)
                {
                    string newType = "";
                    switch (item.Status)
                    {
                        case CourseStatus.Active:
                            newType = "1";
                            break;
                        case CourseStatus.Terminated:
                            newType = "2";
                            break;
                        case CourseStatus.Unknown:
                            newType = "3";
                            break;
                        default:
                            newType = "1";
                            break;
                    }
                    valcodeResponse.ValsEntityAssociation.Add(new ApplValcodesVals("", item.Description, newType, item.Code, "", "", ""));
                }
                return valcodeResponse;
            }
        }

        [TestClass]
        public class CourseTopics
        {
            Mock<IColleagueTransactionFactory> transFactoryMock;
            Mock<ICacheProvider> cacheProviderMock;
            Mock<IColleagueDataReader> dataAccessorMock;
            Mock<ILogger> loggerMock;

            IEnumerable<CourseTopic> allCourseTopics;
            string valcodeName;
            ApiSettings apiSettings;

            StudentReferenceDataRepository referenceDataRepo;

            [TestInitialize]
            public void Initialize()
            {
                loggerMock = new Mock<ILogger>();
                apiSettings = new ApiSettings("TEST");

                allCourseTopics = new TestStudentReferenceDataRepository().GetCourseTopicsAsync(false).Result;

                referenceDataRepo = BuildValidReferenceDataRepository();
                valcodeName = referenceDataRepo.BuildFullCacheKey("AllCourseTopics");
            }

            [TestCleanup]
            public void Cleanup()
            {
                allCourseTopics = null;
                valcodeName = string.Empty;
                apiSettings = null;
            }

            [TestMethod]
            public async Task StudentReferenceDataRepository_GetCourseTopicsAsync_False()
            {
                var results = await referenceDataRepo.GetCourseTopicsAsync(false);
                Assert.AreEqual(allCourseTopics.Count(), results.Count());

                foreach (var courseTopic in allCourseTopics)
                {
                    var result = results.FirstOrDefault(i => i.Guid == courseTopic.Guid);

                    Assert.AreEqual(courseTopic.Code, result.Code);
                    Assert.AreEqual(courseTopic.Description, result.Description);
                    Assert.AreEqual(courseTopic.Guid, result.Guid);
                }

            }

            [TestMethod]
            public async Task StudentReferenceDataRepository_GetCourseTopicsAsync_True()
            {
                var results = await referenceDataRepo.GetCourseTopicsAsync(true);
                Assert.AreEqual(allCourseTopics.Count(), results.Count());

                foreach (var courseTopic in allCourseTopics)
                {
                    var result = results.FirstOrDefault(i => i.Guid == courseTopic.Guid);

                    Assert.AreEqual(courseTopic.Code, result.Code);
                    Assert.AreEqual(courseTopic.Description, result.Description);
                    Assert.AreEqual(courseTopic.Guid, result.Guid);
                }

            }

            private StudentReferenceDataRepository BuildValidReferenceDataRepository()
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

                var records = new Collection<DataContracts.TopicCodes>();
                foreach (var item in allCourseTopics)
                {
                    DataContracts.TopicCodes record = new DataContracts.TopicCodes();
                    record.RecordGuid = item.Guid;
                    record.TopcDesc = item.Description;
                    record.Recordkey = item.Code;
                    records.Add(record);
                }
                dataAccessorMock.Setup(acc => acc.BulkReadRecordAsync<DataContracts.TopicCodes>("TOPIC.CODES", "", true)).ReturnsAsync(records);

                cacheProviderMock.Setup<Task<Tuple<object, SemaphoreSlim>>>(x =>
                 x.GetAndLockSemaphoreAsync(It.IsAny<string>(), null))
                 .ReturnsAsync(new Tuple<object, SemaphoreSlim>(null, new SemaphoreSlim(1, 1)));

                dataAccessorMock.Setup(acc => acc.SelectAsync(It.IsAny<RecordKeyLookup[]>())).Returns<RecordKeyLookup[]>(recordKeyLookups =>
                {
                    var result = new Dictionary<string, RecordKeyLookupResult>();
                    foreach (var recordKeyLookup in recordKeyLookups)
                    {
                        var record = allCourseTopics.Where(e => e.Code == recordKeyLookup.PrimaryKey).FirstOrDefault();
                        result.Add(string.Join("+", new string[] { "TOPIC.CODES", record.Code }),
                            new RecordKeyLookupResult() { Guid = record.Guid });
                    }
                    return Task.FromResult(result);
                });

                // Construct repository
                referenceDataRepo = new StudentReferenceDataRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object, apiSettings);

                return referenceDataRepo;
            }
        }

        [TestClass]
        public class CourseTransferStatuses
        {
            Mock<IColleagueTransactionFactory> transFactoryMock;
            Mock<ICacheProvider> cacheProviderMock;
            Mock<IColleagueDataReader> dataAccessorMock;
            Mock<ILogger> loggerMock;
            IEnumerable<SectionRegistrationStatusItem> allCourseTransferStatuses;
            ApplValcodes courseTransferStatusValcodeResponse;
            string valcodeName;
            ApiSettings apiSettings;

            Mock<IStudentReferenceDataRepository> referenceDataRepositoryMock;
            IStudentReferenceDataRepository referenceDataRepository;
            StudentReferenceDataRepository referenceDataRepo;

            [TestInitialize]
            public void Initialize()
            {
                loggerMock = new Mock<ILogger>();
                apiSettings = new ApiSettings("TEST");

                // Build Courses responses used for mocking
                allCourseTransferStatuses = new List<SectionRegistrationStatusItem>()
                {
                    new SectionRegistrationStatusItem("7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc", "AT", "Athletic"),
                    new SectionRegistrationStatusItem("849e6a7c-6cd4-4f98-8a73-ab0aa3627f0d", "AC", "Academic"),
                    new SectionRegistrationStatusItem("d2253ac7-9931-4560-b42f-1fccd43c952e", "CU", "Cultural")
                };
                courseTransferStatusValcodeResponse = BuildValcodeResponse(allCourseTransferStatuses);
                var courseTransferStatusValResponse = new List<string>() { "2" };
                courseTransferStatusValcodeResponse.ValActionCode1 = courseTransferStatusValResponse;

                referenceDataRepositoryMock = new Mock<IStudentReferenceDataRepository>();
                referenceDataRepository = referenceDataRepositoryMock.Object;

                // Build course transfer status repository
                referenceDataRepo = BuildValidReferenceDataRepository();
                valcodeName = referenceDataRepo.BuildFullCacheKey("ST_STUDENT.ACAD.CRED.STATUSES_GUID");

                cacheProviderMock.Setup<Task<Tuple<object, SemaphoreSlim>>>(x =>
                   x.GetAndLockSemaphoreAsync(It.IsAny<string>(), null))
                   .ReturnsAsync(new Tuple<object, SemaphoreSlim>(null, new SemaphoreSlim(1, 1)));
            }

            [TestCleanup]
            public void Cleanup()
            {
                transFactoryMock = null;
                dataAccessorMock = null;
                cacheProviderMock = null;
                courseTransferStatusValcodeResponse = null;
                allCourseTransferStatuses = null;
                referenceDataRepo = null;
            }

            [TestMethod]
            public async Task StudentReferenceDataRepo_GetsCourseTransferStatusesCacheAsync()
            {
                var courseTransferStatuses = await referenceDataRepo.GetStudentAcademicCreditStatusesAsync(false);

                for (int i = 0; i < allCourseTransferStatuses.Count(); i++)
                {
                    Assert.AreEqual(allCourseTransferStatuses.ElementAt(i).Code, courseTransferStatuses.ElementAt(i).Code);
                    Assert.AreEqual(allCourseTransferStatuses.ElementAt(i).Description, courseTransferStatuses.ElementAt(i).Description);
                }
            }

            [TestMethod]
            public async Task StudentReferenceDataRepo_GetsCourseTransferStatusesNonCacheAsync()
            {
                var statuses = await referenceDataRepo.GetStudentAcademicCreditStatusesAsync(true);

                for (int i = 0; i < allCourseTransferStatuses.Count(); i++)
                {
                    Assert.AreEqual(allCourseTransferStatuses.ElementAt(i).Code, statuses.ElementAt(i).Code);
                    Assert.AreEqual(allCourseTransferStatuses.ElementAt(i).Description, statuses.ElementAt(i).Description);
                }
            }

            [TestMethod]
            public async Task StudentReferenceDataRepo_GetCourseTransferStatuses_WritesToCacheAsync()
            {

                // Set up local cache mock to respond to cache request:
                //  -to "Contains" request, return "false" to indicate item is not in cache
                //  -to cache "Get" request, return null so we know it's reading from the "repository"
                cacheProviderMock.Setup(x => x.Contains(valcodeName, null)).Returns(false);
                cacheProviderMock.Setup(x => x.Get(valcodeName, null)).Returns(null);

                // return a valid response to the data accessor request
                dataAccessorMock.Setup(acc => acc.ReadRecordAsync<ApplValcodes>("ST.VALCODES", "STUDENT.ACAD.CRED.STATUSES", It.IsAny<bool>())).ReturnsAsync(courseTransferStatusValcodeResponse);

                cacheProviderMock.Setup<Task<Tuple<object, SemaphoreSlim>>>(x =>
                 x.GetAndLockSemaphoreAsync(It.IsAny<string>(), null))
                 .ReturnsAsync(new Tuple<object, SemaphoreSlim>(null, new SemaphoreSlim(1, 1)));

                // But after data accessor read, set up mocking so we can verify the list of courseTransferStatuses was written to the cache
                cacheProviderMock.Setup(x => x.Add(It.IsAny<string>(), It.IsAny<Task<List<CourseTransferStatus>>>(), It.IsAny<CacheItemPolicy>(), null)).Verifiable();

                cacheProviderMock.Setup(x => x.Contains(referenceDataRepo.BuildFullCacheKey("ST_STUDENT.ACAD.CRED.STATUSES"), null)).Returns(true);                
                var courseTransferStatuses = await referenceDataRepo.GetStudentAcademicCreditStatusesAsync(false);
                cacheProviderMock.Setup(x => x.Get(referenceDataRepo.BuildFullCacheKey("ST_STUDENT.ACAD.CRED.STATUSES"), null)).Returns(courseTransferStatuses);
                // Verify that courseTransferStatuses were returned, which means they came from the "repository".
                Assert.IsTrue(courseTransferStatuses.Count() == 3);

                // Verify that the courseLevel item was added to the cache after it was read from the repository
                cacheProviderMock.Verify(m => m.Add(It.IsAny<string>(), It.IsAny<Task<List<CourseTransferStatus>>>(), It.IsAny<CacheItemPolicy>(), null), Times.Never);

            }

            [TestMethod]
            public async Task StudentReferenceDataRepo_GetCourseTransferStatuses_GetsCachedCourseTransferStatusesAsync()
            {
                // Set up local cache mock to respond to cache request:
                //  -to "Contains" request, return "true" to indicate item is in cache
                //  -to "Get" request, return the cache item (in this case the "STUDENT.ACAD.CRED.STATUSES" cache item)
                cacheProviderMock.Setup(x => x.Contains(valcodeName, null)).Returns(true);
                cacheProviderMock.Setup(x => x.Get(valcodeName, null)).Returns(allCourseTransferStatuses).Verifiable();

                // return null for request, so that if we have a result, it wasn't the data accessor that returned it.
                dataAccessorMock.Setup(acc => acc.ReadRecordAsync<ApplValcodes>("ST.VALCODES", "STUDENT.ACAD.CRED.STATUSES", true)).ReturnsAsync(new ApplValcodes());

                // Assert the courseTransferStatuses are returned
                Assert.IsTrue((await referenceDataRepo.GetStudentAcademicCreditStatusesAsync(false)).Count() == 3);
                // Verify that the courseTransferStatuses were retrieved from cache
                cacheProviderMock.Verify(m => m.Get(valcodeName, null));
            }

            private StudentReferenceDataRepository BuildValidReferenceDataRepository()
            {
                // transaction factory mock
                transFactoryMock = new Mock<IColleagueTransactionFactory>();
                // Cache Provider Mock
                cacheProviderMock = new Mock<ICacheProvider>();
                // Set up data accessor for mocking 
                dataAccessorMock = new Mock<IColleagueDataReader>();

                // Set up dataAccessorMock as the object for the DataAccessor
                transFactoryMock.Setup(transFac => transFac.GetDataReader()).Returns(dataAccessorMock.Object);

                // Setup response to courseLevel valcode read
                dataAccessorMock.Setup(acc => acc.ReadRecordAsync<ApplValcodes>("ST.VALCODES", "STUDENT.ACAD.CRED.STATUSES", It.IsAny<bool>())).ReturnsAsync(courseTransferStatusValcodeResponse);
                cacheProviderMock.Setup<Task<Tuple<object, SemaphoreSlim>>>(x => x.GetAndLockSemaphoreAsync(It.IsAny<string>(), null))
                .ReturnsAsync(new Tuple<object, SemaphoreSlim>(null, new SemaphoreSlim(1, 1)));

                dataAccessorMock.Setup(acc => acc.SelectAsync(It.IsAny<RecordKeyLookup[]>())).Returns<RecordKeyLookup[]>(recordKeyLookups =>
                {
                    var result = new Dictionary<string, RecordKeyLookupResult>();
                    foreach (var recordKeyLookup in recordKeyLookups)
                    {
                        var courseLevel = allCourseTransferStatuses.Where(e => e.Code == recordKeyLookup.SecondaryKey).FirstOrDefault();
                        result.Add(string.Join("+", new string[] { "ST.VALCODES", "STUDENT.ACAD.CRED.STATUSES", courseLevel.Code }),
                            new RecordKeyLookupResult() { Guid = courseLevel.Guid });
                    }
                    return Task.FromResult(result);
                });

                // Construct repository
                referenceDataRepo = new StudentReferenceDataRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object, apiSettings);

                return referenceDataRepo;
            }
            
            private ApplValcodes BuildValcodeResponse(IEnumerable<SectionRegistrationStatusItem> courseTransferStatuses)
            {
                ApplValcodes courseTransferStatusesResponse = new ApplValcodes();
                courseTransferStatusesResponse.ValsEntityAssociation = new List<ApplValcodesVals>();
                foreach (var item in courseTransferStatuses)
                {
                    courseTransferStatusesResponse.ValsEntityAssociation.Add(new ApplValcodesVals("", item.Description, "2", item.Code, "3", "", ""));
                }
                return courseTransferStatusesResponse;
            }
        }

        [TestClass]
        public class CourseTypes
        {
            Mock<IColleagueTransactionFactory> transFactoryMock;
            Mock<ICacheProvider> cacheProviderMock;
            Mock<IColleagueDataReader> dataAccessorMock;
            Mock<ILogger> loggerMock;
            IEnumerable<CourseType> allCourseTypes;
            ApplValcodes courseTypeValcodeResponse;
            string valcodeName;
            ApiSettings apiSettings;

            Mock<IStudentReferenceDataRepository> referenceDataRepositoryMock;
            IStudentReferenceDataRepository referenceDataRepository;
            StudentReferenceDataRepository referenceDataRepo;

            [TestInitialize]
            public void Initialize()
            {
                loggerMock = new Mock<ILogger>();
                apiSettings = new ApiSettings("TEST");

                // Build advisor types responses used for mocking
                //allCourseTypes = new TestStudentReferenceDataRepository().GetCourseTypesAsync(false).Result;
                allCourseTypes = new TestCourseTypeRepository().Get().ToList();
                courseTypeValcodeResponse = BuildValcodeResponse(allCourseTypes);
                var courseTypeValResponse = new List<string>() { "2" };
                courseTypeValcodeResponse.ValActionCode1 = courseTypeValResponse;

                referenceDataRepositoryMock = new Mock<IStudentReferenceDataRepository>();
                referenceDataRepository = referenceDataRepositoryMock.Object;

                // Build course level repository
                referenceDataRepo = BuildValidReferenceDataRepository();
                valcodeName = referenceDataRepo.BuildFullCacheKey("ST_COURSE.TYPES_GUID");

                cacheProviderMock.Setup<Task<Tuple<object, SemaphoreSlim>>>(x =>
                   x.GetAndLockSemaphoreAsync(It.IsAny<string>(), null))
                   .ReturnsAsync(new Tuple<object, SemaphoreSlim>(null, new SemaphoreSlim(1, 1)));
            }

            [TestCleanup]
            public void Cleanup()
            {
                transFactoryMock = null;
                dataAccessorMock = null;
                cacheProviderMock = null;
                courseTypeValcodeResponse = null;
                allCourseTypes = null;
                referenceDataRepo = null;
            }

            [TestMethod]
            public async Task StudentReferenceDataRepo_GetsCourseTypesNoArgAsync()
            {
                var courseTypes = await referenceDataRepo.GetCourseTypesAsync(false);

                for (int i = 0; i < allCourseTypes.Count(); i++)
                {
                    Assert.AreEqual(allCourseTypes.ElementAt(i).Code, courseTypes.ElementAt(i).Code);
                    Assert.AreEqual(allCourseTypes.ElementAt(i).Description, courseTypes.ElementAt(i).Description);
                }
            }

            [TestMethod]
            public async Task StudentReferenceDataRepo_GetsCourseTypesCacheAsync()
            {
                var courseTypes = await referenceDataRepo.GetCourseTypesAsync(false);

                for (int i = 0; i < allCourseTypes.Count(); i++)
                {
                    Assert.AreEqual(allCourseTypes.ElementAt(i).Code, courseTypes.ElementAt(i).Code);
                    Assert.AreEqual(allCourseTypes.ElementAt(i).Description, courseTypes.ElementAt(i).Description);
                }
            }

            [TestMethod]
            public async Task StudentReferenceDataRepo_GetsCourseTypesNonCacheAsync()
            {
                var statuses = await referenceDataRepo.GetCourseTypesAsync(true);

                for (int i = 0; i < allCourseTypes.Count(); i++)
                {
                    Assert.AreEqual(allCourseTypes.ElementAt(i).Code, statuses.ElementAt(i).Code);
                    Assert.AreEqual(allCourseTypes.ElementAt(i).Description, statuses.ElementAt(i).Description);
                }
            }

            [TestMethod]
            public async Task StudentReferenceDataRepo_GetCourseTypes_WritesToCacheAsync()
            {

                // Set up local cache mock to respond to cache request:
                //  -to "Contains" request, return "false" to indicate item is not in cache
                //  -to cache "Get" request, return null so we know it's reading from the "repository"
                cacheProviderMock.Setup(x => x.Contains(valcodeName, null)).Returns(false);
                cacheProviderMock.Setup(x => x.Get(valcodeName, null)).Returns(null);

                // return a valid response to the data accessor request
                dataAccessorMock.Setup(acc => acc.ReadRecordAsync<ApplValcodes>("ST.VALCODES", "COURSE.TYPES", It.IsAny<bool>())).ReturnsAsync(courseTypeValcodeResponse);

                cacheProviderMock.Setup<Task<Tuple<object, SemaphoreSlim>>>(x =>
                 x.GetAndLockSemaphoreAsync(It.IsAny<string>(), null))
                 .ReturnsAsync(new Tuple<object, SemaphoreSlim>(null, new SemaphoreSlim(1, 1)));

                // But after data accessor read, set up mocking so we can verify the list of CourseTypes was written to the cache
                cacheProviderMock.Setup(x => x.Add(It.IsAny<string>(), It.IsAny<Task<List<CourseType>>>(), It.IsAny<CacheItemPolicy>(), null)).Verifiable();

                cacheProviderMock.Setup(x => x.Contains(referenceDataRepo.BuildFullCacheKey("ST_COURSE.TYPES"), null)).Returns(true);
                var courseTypes = await referenceDataRepo.GetCourseTypesAsync(false);
                cacheProviderMock.Setup(x => x.Get(referenceDataRepo.BuildFullCacheKey("ST_COURSE.TYPES"), null)).Returns(courseTypes);
                // Verify that courseTypes were returned, which means they came from the "repository".
                Assert.IsTrue(courseTypes.Count() == 6);

                // Verify that the courseType item was added to the cache after it was read from the repository
                cacheProviderMock.Verify(m => m.Add(It.IsAny<string>(), It.IsAny<Task<List<CourseType>>>(), It.IsAny<CacheItemPolicy>(), null), Times.Never);

            }

            [TestMethod]
            public async Task StudentReferenceDataRepo_GetCourseTypes_GetsCachedCourseTypesAsync()
            {
                // Set up local cache mock to respond to cache request:
                //  -to "Contains" request, return "true" to indicate item is in cache
                //  -to "Get" request, return the cache item (in this case the "COURSE.LEVELS" cache item)
                cacheProviderMock.Setup(x => x.Contains(valcodeName, null)).Returns(true);
                cacheProviderMock.Setup(x => x.Get(valcodeName, null)).Returns(allCourseTypes).Verifiable();

                // return null for request, so that if we have a result, it wasn't the data accessor that returned it.
                dataAccessorMock.Setup(acc => acc.ReadRecordAsync<ApplValcodes>("ST.VALCODES", "COURSE.TYPES", true)).ReturnsAsync(new ApplValcodes());

                // Assert the CourseTypes are returned
                Assert.IsTrue((await referenceDataRepo.GetCourseTypesAsync(false)).Count() == 6);
                // Verify that the sectionRegistrationStatuses were retrieved from cache
                cacheProviderMock.Verify(m => m.Get(valcodeName, null));
            }

            private StudentReferenceDataRepository BuildValidReferenceDataRepository()
            {
                // transaction factory mock
                transFactoryMock = new Mock<IColleagueTransactionFactory>();
                // Cache Provider Mock
                cacheProviderMock = new Mock<ICacheProvider>();
                // Set up data accessor for mocking 
                dataAccessorMock = new Mock<IColleagueDataReader>();

                // Set up dataAccessorMock as the object for the DataAccessor
                transFactoryMock.Setup(transFac => transFac.GetDataReader()).Returns(dataAccessorMock.Object);

                // Setup response to assessmentSpecialCircumstane valcode read
                dataAccessorMock.Setup(acc => acc.ReadRecordAsync<ApplValcodes>("ST.VALCODES", "COURSE.TYPES", It.IsAny<bool>())).ReturnsAsync(courseTypeValcodeResponse);
                cacheProviderMock.Setup<Task<Tuple<object, SemaphoreSlim>>>(x => x.GetAndLockSemaphoreAsync(It.IsAny<string>(), null))
                .ReturnsAsync(new Tuple<object, SemaphoreSlim>(null, new SemaphoreSlim(1, 1)));

                dataAccessorMock.Setup(acc => acc.SelectAsync(It.IsAny<RecordKeyLookup[]>())).Returns<RecordKeyLookup[]>(recordKeyLookups =>
                {
                    var result = new Dictionary<string, RecordKeyLookupResult>();
                    foreach (var recordKeyLookup in recordKeyLookups)
                    {
                        var courseType = allCourseTypes.Where(e => e.Code == recordKeyLookup.SecondaryKey).FirstOrDefault();
                        result.Add(string.Join("+", new string[] { "ST.VALCODES", "COURSE.TYPES", courseType.Code }),
                            new RecordKeyLookupResult() { Guid = courseType.Guid });
                    }
                    return Task.FromResult(result);
                });

                // Construct repository
                referenceDataRepo = new StudentReferenceDataRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object, apiSettings);

                return referenceDataRepo;
            }

            private ApplValcodes BuildValcodeResponse(IEnumerable<CourseType> courseTypes)
            {
                ApplValcodes courseTypesResponse = new ApplValcodes();
                courseTypesResponse.ValsEntityAssociation = new List<ApplValcodesVals>();
                foreach (var item in courseTypes)
                {
                    courseTypesResponse.ValsEntityAssociation.Add(new ApplValcodesVals("", item.Description, "2", item.Code, "3", "", ""));
                }
                return courseTypesResponse;
            }
        }

        [TestClass]
        public class CreditCategories
        {
            Mock<IColleagueTransactionFactory> transFactoryMock;
            Mock<ICacheProvider> cacheProviderMock;
            Mock<IColleagueDataReader> dataAccessorMock;
            Mock<ILogger> loggerMock;
            List<CreditCategory> allCreditCategories;
            ApplValcodes creditCategoriesValcodeResponse;
            string valcodeName;
            ApiSettings apiSettings;

            StudentReferenceDataRepository referenceDataRepo;

            [TestInitialize]
            public void Initialize()
            {
                loggerMock = new Mock<ILogger>();
                apiSettings = new ApiSettings("TEST");

                allCreditCategories = new TestStudentReferenceDataRepository().GetCreditCategoriesAsync().Result.ToList();
                
                creditCategoriesValcodeResponse = BuildValcodeResponse(allCreditCategories);
                
                referenceDataRepo = BuildValidReferenceDataRepository();
                valcodeName = referenceDataRepo.BuildFullCacheKey("AllCreditCategories");
            }

            [TestCleanup]
            public void Cleanup()
            {
                transFactoryMock = null;
                dataAccessorMock = null;
                cacheProviderMock = null;
                creditCategoriesValcodeResponse = null;
                allCreditCategories = null;
                referenceDataRepo = null;
            }

            [TestMethod]
            public async Task StudentReferenceDataRepo_CreditCategories_GetCreditCategoriesAsync_Cache()
            {
                var repo = await referenceDataRepo.GetCreditCategoriesAsync(false);
                for (int i = 0; i < allCreditCategories.Count(); i++)
                {
                    Assert.AreEqual(allCreditCategories.ElementAt(i).Code, repo.ElementAt(i).Code);
                    Assert.AreEqual(allCreditCategories.ElementAt(i).Description, repo.ElementAt(i).Description);
                }
            }

            [TestMethod]
            public async Task StudentReferenceDataRepo_CreditCategories_GetCreditCategoriesAsync_NoCache()
            {
                var repoGetCreditCategories = await referenceDataRepo.GetCreditCategoriesAsync(true);
                for (int i = 0; i < allCreditCategories.Count(); i++)
                {
                    Assert.AreEqual(allCreditCategories.ElementAt(i).Code, repoGetCreditCategories.ElementAt(i).Code);
                    Assert.AreEqual(allCreditCategories.ElementAt(i).Description, repoGetCreditCategories.ElementAt(i).Description);
                }
            }

            [TestMethod]
            public async Task StudentReferenceDataRepo_CreditCategories_GetCreditCategoriesAsync_NoArg()
            {
                var repoGetCreditCategories = await referenceDataRepo.GetCreditCategoriesAsync();
                for (int i = 0; i < allCreditCategories.Count(); i++)
                {
                    Assert.AreEqual(allCreditCategories.ElementAt(i).Code, repoGetCreditCategories.ElementAt(i).Code);
                    Assert.AreEqual(allCreditCategories.ElementAt(i).Description, repoGetCreditCategories.ElementAt(i).Description);
                }
            }

            private StudentReferenceDataRepository BuildValidReferenceDataRepository()
            {
                // transaction factory mock
                transFactoryMock = new Mock<IColleagueTransactionFactory>();
                // Cache Mock
                //localCacheMock = new Mock<ObjectCache>();
                // Cache Provider Mock
                cacheProviderMock = new Mock<ICacheProvider>();
                // Set up data accessor for mocking 
                dataAccessorMock = new Mock<IColleagueDataReader>();
                apiSettings = new ApiSettings("TEST");

                // Set up dataAccessorMock as the object for the DataAccessor
                transFactoryMock.Setup(transFac => transFac.GetDataReader()).Returns(dataAccessorMock.Object);

                var records = new Collection<DataContracts.CredTypes>();
                foreach (var item in allCreditCategories)
                {
                    DataContracts.CredTypes record = new DataContracts.CredTypes();
                    record.RecordGuid = item.Guid;
                    record.CrtpDesc = item.Description;
                    record.Recordkey = item.Code;
                    records.Add(record);
                }
                dataAccessorMock.Setup(acc => acc.BulkReadRecordAsync<DataContracts.CredTypes>("CRED.TYPES", "", true)).ReturnsAsync(records);

                cacheProviderMock.Setup<Task<Tuple<object, SemaphoreSlim>>>(x =>
                 x.GetAndLockSemaphoreAsync(It.IsAny<string>(), null))
                 .ReturnsAsync(new Tuple<object, SemaphoreSlim>(null, new SemaphoreSlim(1, 1)));

                dataAccessorMock.Setup(acc => acc.SelectAsync(It.IsAny<RecordKeyLookup[]>())).Returns<RecordKeyLookup[]>(recordKeyLookups =>
                {
                    var result = new Dictionary<string, RecordKeyLookupResult>();
                    foreach (var recordKeyLookup in recordKeyLookups)
                    {
                        var record = allCreditCategories.Where(e => e.Code == recordKeyLookup.PrimaryKey).FirstOrDefault();
                        result.Add(string.Join("+", new string[] { "CRED.TYPES", record.Code }),
                            new RecordKeyLookupResult() { Guid = record.Guid });
                    }
                    return Task.FromResult(result);
                });

                // Construct repository
                referenceDataRepo = new StudentReferenceDataRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object, apiSettings);

                return referenceDataRepo;
            }

            private ApplValcodes BuildValcodeResponse(List<CreditCategory> creditCategory)
            {
                ApplValcodes creditCategoryResponse = new ApplValcodes();
                creditCategoryResponse.ValsEntityAssociation = new List<ApplValcodesVals>();
                foreach (var item in creditCategory)
                {
                    creditCategoryResponse.ValsEntityAssociation.Add(new ApplValcodesVals("", item.Description, "", item.Code, "", "", ""));
                }
                return creditCategoryResponse;
            }
        }

        [TestClass]
        public class DistributionsTest
        {
            Mock<IColleagueTransactionFactory> transFactoryMock;
            Mock<ICacheProvider> cacheProviderMock;
            Mock<IColleagueDataReader> dataAccessorMock;
            Mock<ILogger> loggerMock;

            IEnumerable<Distribution2> allDistributions;
            string valcodeName;
            ApiSettings apiSettings;

            StudentReferenceDataRepository referenceDataRepo;

            [TestInitialize]
            public void Initialize()
            {
                loggerMock = new Mock<ILogger>();
                apiSettings = new ApiSettings("TEST");

                allDistributions = new TestDistributionRepository().Get();

                referenceDataRepo = BuildValidReferenceDataRepository();
                valcodeName = referenceDataRepo.BuildFullCacheKey("AllEEDMDistributions2");
            }

            [TestCleanup]
            public void Cleanup()
            {
                allDistributions = null;
                valcodeName = string.Empty;
                apiSettings = null;
            }

            [TestMethod]
            public async Task StudentReferenceDataRepository_GetDistributionsAsync_False()
            {
                var results = await referenceDataRepo.GetDistributionsAsync(false);
                Assert.AreEqual(allDistributions.Count(), results.Count());

                foreach (var distribution in allDistributions)
                {
                    var result = results.FirstOrDefault(i => i.Guid == distribution.Guid);

                    Assert.AreEqual(distribution.Code, result.Code);
                    Assert.AreEqual(distribution.Description, result.Description);
                    Assert.AreEqual(distribution.Guid, result.Guid);
                }

            }

            [TestMethod]
            public async Task StudentReferenceDataRepository_GetDistributionsAsync_True()
            {
                var results = await referenceDataRepo.GetDistributionsAsync(true);
                Assert.AreEqual(allDistributions.Count(), results.Count());

                foreach (var distribution in allDistributions)
                {
                    var result = results.FirstOrDefault(i => i.Guid == distribution.Guid);

                    Assert.AreEqual(distribution.Code, result.Code);
                    Assert.AreEqual(distribution.Description, result.Description);
                    Assert.AreEqual(distribution.Guid, result.Guid);
                }

            }

            private StudentReferenceDataRepository BuildValidReferenceDataRepository()
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

                var records = new Collection<Distributions>();
                foreach (var item in allDistributions)
                {
                    Distributions record = new Distributions();
                    record.RecordGuid = item.Guid;
                    record.DistrDescription = item.Description;
                    record.Recordkey = item.Code;
                    records.Add(record);
                }
                dataAccessorMock.Setup(acc => acc.BulkReadRecordAsync<Distributions>("DISTRIBUTION", "", true)).ReturnsAsync(records);

                cacheProviderMock.Setup<Task<Tuple<object, SemaphoreSlim>>>(x =>
                 x.GetAndLockSemaphoreAsync(It.IsAny<string>(), null))
                 .ReturnsAsync(new Tuple<object, SemaphoreSlim>(null, new SemaphoreSlim(1, 1)));

                dataAccessorMock.Setup(acc => acc.SelectAsync(It.IsAny<RecordKeyLookup[]>())).Returns<RecordKeyLookup[]>(recordKeyLookups =>
                {
                    var result = new Dictionary<string, RecordKeyLookupResult>();
                    foreach (var recordKeyLookup in recordKeyLookups)
                    {
                        var record = allDistributions.Where(e => e.Code == recordKeyLookup.PrimaryKey).FirstOrDefault();
                        result.Add(string.Join("+", new string[] { "DISTRIBUTION", record.Code }),
                            new RecordKeyLookupResult() { Guid = record.Guid });
                    }
                    return Task.FromResult(result);
                });

                // Construct repository
                referenceDataRepo = new StudentReferenceDataRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object, apiSettings);

                return referenceDataRepo;
            }
        }


        [TestClass]
        public class EnrollmentStatuses
        {
            Mock<IColleagueTransactionFactory> transFactoryMock;
            Mock<ICacheProvider> cacheProviderMock;
            Mock<IColleagueDataReader> dataAccessorMock;
            Mock<ILogger> loggerMock;
            IEnumerable<EnrollmentStatus> allEnrollmentStatuses;
            ApplValcodes enrollmentStatusValcodeResponse;
            string valcodeName;
            ApiSettings apiSettings;

            StudentReferenceDataRepository referenceDataRepo;

            [TestInitialize]
            public void Initialize()
            {
                loggerMock = new Mock<ILogger>();
                apiSettings = new ApiSettings("TEST");

                // Build enrollment statuses responses used for mocking
                allEnrollmentStatuses = new TestStudentReferenceDataRepository().GetEnrollmentStatusesAsync(false).Result;
                enrollmentStatusValcodeResponse = BuildValcodeResponse(allEnrollmentStatuses);

                // Build enrollment status repository
                referenceDataRepo = BuildValidReferenceDataRepository();
                valcodeName = referenceDataRepo.BuildFullCacheKey("ST_STUDENT.PROGRAM.STATUSES_GUID");
            }

            [TestCleanup]
            public void Cleanup()
            {
                transFactoryMock = null;
                dataAccessorMock = null;
                cacheProviderMock = null;
                enrollmentStatusValcodeResponse = null;
                allEnrollmentStatuses = null;
                referenceDataRepo = null;
            }

            [TestMethod]
            public async Task StudentReferenceDataRepo_GetEnrollmentStatusesCacheAsync()
            {
                var races = await referenceDataRepo.GetEnrollmentStatusesAsync(false);
                for (int i = 0; i < races.Count(); i++)
                {
                    Assert.AreEqual(allEnrollmentStatuses.ElementAt(i).Code, races.ElementAt(i).Code);
                    Assert.AreEqual(allEnrollmentStatuses.ElementAt(i).Description, races.ElementAt(i).Description);
                }
            }

            [TestMethod]
            public async Task StudentReferenceDataRepo_GetEnrollmentStatusesNonCacheAsync()
            {
                var enrollmentStatuses = await referenceDataRepo.GetEnrollmentStatusesAsync(true);
                for (int i = 0; i < enrollmentStatuses.Count(); i++)
                {
                    Assert.AreEqual(allEnrollmentStatuses.ElementAt(i).Code, enrollmentStatuses.ElementAt(i).Code);
                    Assert.AreEqual(allEnrollmentStatuses.ElementAt(i).Description, enrollmentStatuses.ElementAt(i).Description);
                }
            }

            [TestMethod]
            public async Task StudentReferenceDataRepo_GetEnrollmentStatuses_WritesToCacheAsync()
            {

                // Set up local cache mock to respond to cache request:
                //  -to "Contains" request, return "false" to indicate item is not in cache
                //  -to cache "Get" request, return null so we know it's reading from the "repository"
                cacheProviderMock.Setup(x => x.Contains(valcodeName, null)).Returns(false);
                cacheProviderMock.Setup(x => x.Get(valcodeName, null)).Returns(null);

                // return a valid response to the data accessor request
                dataAccessorMock.Setup(acc => acc.ReadRecordAsync<ApplValcodes>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(enrollmentStatusValcodeResponse);

                cacheProviderMock.Setup<Task<Tuple<object, SemaphoreSlim>>>(x =>
                 x.GetAndLockSemaphoreAsync(It.IsAny<string>(), null))
                 .ReturnsAsync(new Tuple<object, SemaphoreSlim>(null, new SemaphoreSlim(1, 1)));

                // But after data accessor read, set up mocking so we can verify the list of enrollment statuses was written to the cache
                cacheProviderMock.Setup(x => x.Add(It.IsAny<string>(), It.IsAny<Task<IEnumerable<EnrollmentStatus>>>(), It.IsAny<CacheItemPolicy>(), null)).Verifiable();

                cacheProviderMock.Setup(x => x.Contains(referenceDataRepo.BuildFullCacheKey("ST_STUDENT.PROGRAM.STATUSES"), null)).Returns(true);
                var enrollmentStatuses = await referenceDataRepo.GetEnrollmentStatusesAsync(false);
                cacheProviderMock.Setup(x => x.Get(referenceDataRepo.BuildFullCacheKey("ST_STUDENT.PROGRAM.STATUSES"), null)).Returns(enrollmentStatuses);
                // Verify that enrollment statuses were returned, which means they came from the "repository".
                Assert.IsTrue(enrollmentStatuses.Count() == 3);

                // Verify that the enrollment status item was added to the cache after it was read from the repository
                cacheProviderMock.Verify(m => m.Add(It.IsAny<string>(), It.IsAny<Task<IEnumerable<EnrollmentStatus>>>(), It.IsAny<CacheItemPolicy>(), null), Times.Never);

            }

            [TestMethod]
            public async Task StudentReferenceDataRepo_GetEnrollmentStatuses_CachedEnrollmentStatuses()
            {
                // Set up local cache mock to respond to cache request:
                //  -to "Contains" request, return "true" to indicate item is in cache
                //  -to "Get" request, return the cache item (in this case the "STUDENT.PROGRAM.STATUSES" cache item)
                cacheProviderMock.Setup(x => x.Contains(valcodeName, null)).Returns(true);
                cacheProviderMock.Setup(x => x.Get(valcodeName, null)).Returns(allEnrollmentStatuses).Verifiable();

                // return null for request, so that if we have a result, it wasn't the data accessor that returned it.
                dataAccessorMock.Setup(acc => acc.ReadRecordAsync<ApplValcodes>("ST.VALCODES", "STUDENT.PROGRAM.STATUSES", true)).ReturnsAsync(new ApplValcodes());

                // Assert the enrollment statuses are returned
                Assert.IsTrue((await referenceDataRepo.GetEnrollmentStatusesAsync(false)).Count() == 3);
                // Verify that the enrollment statuses were retrieved from cache
                cacheProviderMock.Verify(m => m.Get(valcodeName, null));
            }

            private StudentReferenceDataRepository BuildValidReferenceDataRepository()
            {
                // transaction factory mock
                transFactoryMock = new Mock<IColleagueTransactionFactory>();
                // Cache Provider Mock
                cacheProviderMock = new Mock<ICacheProvider>();
                // Set up data accessor for mocking 
                dataAccessorMock = new Mock<IColleagueDataReader>();

                // Set up dataAccessorMock as the object for the DataAccessor
                transFactoryMock.Setup(transFac => transFac.GetDataReader()).Returns(dataAccessorMock.Object);

                // Setup response to enrollment status valcode read
                dataAccessorMock.Setup(acc => acc.ReadRecordAsync<ApplValcodes>("ST.VALCODES", "STUDENT.PROGRAM.STATUSES", It.IsAny<bool>())).ReturnsAsync(enrollmentStatusValcodeResponse);
                cacheProviderMock.Setup<Task<Tuple<object, SemaphoreSlim>>>(x => x.GetAndLockSemaphoreAsync(It.IsAny<string>(), null))
                .ReturnsAsync(new Tuple<object, SemaphoreSlim>(null, new SemaphoreSlim(1, 1)));

                dataAccessorMock.Setup(acc => acc.SelectAsync(It.IsAny<RecordKeyLookup[]>())).Returns<RecordKeyLookup[]>(recordKeyLookups =>
                {
                    var result = new Dictionary<string, RecordKeyLookupResult>();
                    foreach (var recordKeyLookup in recordKeyLookups)
                    {
                        var enrollmentStatus = allEnrollmentStatuses.Where(e => e.Code == recordKeyLookup.SecondaryKey).FirstOrDefault();
                        result.Add(string.Join("+", new string[] { "ST.VALCODES", "STUDENT.PROGRAM.STATUSES", enrollmentStatus.Code }),
                            new RecordKeyLookupResult() { Guid = enrollmentStatus.Guid });
                    }
                    return Task.FromResult(result);
                });

                // Construct repository
                referenceDataRepo = new StudentReferenceDataRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object, apiSettings);

                return referenceDataRepo;
            }

            private ApplValcodes BuildValcodeResponse(IEnumerable<EnrollmentStatus> enrollmentStatuses)
            {
                ApplValcodes valcodeResponse = new ApplValcodes();
                valcodeResponse.ValsEntityAssociation = new List<ApplValcodesVals>();
                foreach (var item in enrollmentStatuses)
                {
                    string newType = "";
                    switch (item.EnrollmentStatusType)
                    {
                        case EnrollmentStatusType.inactive:
                            newType = "1";
                            break;
                        case EnrollmentStatusType.active:
                            newType = "2";
                            break;
                        case EnrollmentStatusType.complete:
                            newType = "3";
                            break;
                        default:
                            newType = "1";
                            break;
                    }
                    valcodeResponse.ValsEntityAssociation.Add(new ApplValcodesVals("", item.Description, newType, item.Code, "", "", ""));
                }
                return valcodeResponse;
            }
        }

        [TestClass]
        public class ExternalTranscriptStatuses
        {
            Mock<IColleagueTransactionFactory> transFactoryMock;
            Mock<ICacheProvider> cacheProviderMock;
            Mock<IColleagueDataReader> dataAccessorMock;
            Mock<ILogger> loggerMock;
            IEnumerable<Ellucian.Colleague.Domain.Student.Entities.ExternalTranscriptStatus> allExternalTranscriptStatuses;
            ApplValcodes externalTranscriptStatusesValcodeResponse;
            string valcodeName;
            ApiSettings apiSettings;
            StudentReferenceDataRepository referenceDataRepo;

            [TestInitialize]
            public void Initialize()
            {
                loggerMock = new Mock<ILogger>();

                // Build responses used for mocking
                allExternalTranscriptStatuses = new TestStudentReferenceDataRepository().GetExternalTranscriptStatusesAsync().Result;

                externalTranscriptStatusesValcodeResponse = BuildValcodeResponse(allExternalTranscriptStatuses);

                // Build course repository
                referenceDataRepo = BuildValidReferenceDataRepository();
                valcodeName = referenceDataRepo.BuildFullCacheKey("ST_EXTL.TRAN.STATUSES");

                cacheProviderMock.Setup<Task<Tuple<object, SemaphoreSlim>>>(x =>
                   x.GetAndLockSemaphoreAsync(It.IsAny<string>(), null))
                   .ReturnsAsync(new Tuple<object, SemaphoreSlim>(null, new SemaphoreSlim(1, 1)));

            }

            [TestCleanup]
            public void Cleanup()
            {
                transFactoryMock = null;
                dataAccessorMock = null;
                cacheProviderMock = null;
                externalTranscriptStatusesValcodeResponse = null;
                allExternalTranscriptStatuses = null;
                referenceDataRepo = null;
            }

            [TestMethod]
            public async Task StudentReferenceDataRepo_GetExternalTranscriptStatuses()
            {
                var repo = await referenceDataRepo.GetExternalTranscriptStatusesAsync();

                for (int i = 0; i < allExternalTranscriptStatuses.Count(); i++)
                {
                    Assert.AreEqual(allExternalTranscriptStatuses.ElementAt(i).Code, repo.ElementAt(i).Code);
                    Assert.AreEqual(allExternalTranscriptStatuses.ElementAt(i).Description, repo.ElementAt(i).Description);
                }
            }

            [TestMethod]
            public async Task StudentReferenceDataRepo_GetExternalTranscriptStatuses_WritesToCache()
            {
                // Set up local cache mock to respond to cache request:
                //  -to "Contains" request, return "false" to indicate item is not in cache
                //  -to cache "Get" request, return null so we know it's reading from the "repository"
                cacheProviderMock.Setup(x => x.Contains(valcodeName, null)).Returns(false);
                cacheProviderMock.Setup(x => x.Get(valcodeName, null)).Returns(null);

                // return a valid response to the data accessor request
                dataAccessorMock.Setup(acc => acc.ReadRecordAsync<ApplValcodes>(It.IsAny<string>(), It.IsAny<string>(), true)).ReturnsAsync(externalTranscriptStatusesValcodeResponse);

                // But after data accessor read, set up mocking so we can verify the list of ExternalTranscriptStatuses was written to the cache
                cacheProviderMock.Setup(x => x.AddAndUnlockSemaphore(valcodeName, It.IsAny<Object>(), It.IsAny<SemaphoreSlim>(), It.IsAny<CacheItemPolicy>(), null)).Verifiable();

                // Verify that ExternalTranscriptStatuses were returned, which means they came from the "repository".
                Assert.IsTrue((await referenceDataRepo.GetExternalTranscriptStatusesAsync()).Count() == allExternalTranscriptStatuses.Count());

                // Verify that the ExternalTranscriptStatuses was added to the cache after it was read from the repository
                cacheProviderMock.Verify(m => m.AddAndUnlockSemaphore(valcodeName, It.IsAny<Object>(), It.IsAny<SemaphoreSlim>(), It.IsAny<CacheItemPolicy>(), null));
            }

            [TestMethod]
            public async Task StudentReferenceDataRepo_GetExternalTranscriptStatuses_Cached()
            {
                object lockHandle = null;
                // Set up local cache mock to respond to cache request:
                //  -to "Contains" request, return "false" to indicate item is not in cache
                //  -to cache "Get" request, return null so we know it's getting data from "repository"
                cacheProviderMock.Setup(x => x.Contains(valcodeName, null)).Returns(false);
                cacheProviderMock.Setup(x => x.GetAndLock(valcodeName, out lockHandle, null)).Returns(null);

                // Get these codes to populate the cache
                var externalTranscriptStatuses = (await referenceDataRepo.GetExternalTranscriptStatusesAsync());
                // Assert the types are returned
                Assert.IsTrue((await referenceDataRepo.GetExternalTranscriptStatusesAsync()).Count() == allExternalTranscriptStatuses.Count());
                // Make sure we can verify that it's in the cache
                cacheProviderMock.Setup(x => x.Contains(valcodeName, null)).Returns(true);
                var externalTranscriptStatusTuple = new Tuple<object, SemaphoreSlim>(externalTranscriptStatuses, new SemaphoreSlim(1, 1));
                cacheProviderMock.Setup(x => x.GetAndLockSemaphoreAsync(valcodeName, null)).ReturnsAsync(externalTranscriptStatusTuple).Verifiable();

                // Verify that the types are now retrieved from cache
                externalTranscriptStatuses = (await referenceDataRepo.GetExternalTranscriptStatusesAsync());
                cacheProviderMock.Verify(m => m.GetAndLockSemaphoreAsync(valcodeName, null));
            }

            private T GetCache<T>(ICacheProvider cacheProvider, string key)
                where T : class
            {
                object cache = cacheProvider.Get(key, null);
                return cache as T;
            }

            private StudentReferenceDataRepository BuildValidReferenceDataRepository()
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

                // Setup response to course type valcode read
                dataAccessorMock.Setup(acc => acc.ReadRecordAsync<ApplValcodes>("ST.VALCODES", "EXTL.TRAN.STATUSES", true)).ReturnsAsync(externalTranscriptStatusesValcodeResponse);

                // Construct repository
                referenceDataRepo = new StudentReferenceDataRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object, apiSettings);

                return referenceDataRepo;
            }

            private ApplValcodes BuildValcodeResponse(IEnumerable<Ellucian.Colleague.Domain.Student.Entities.ExternalTranscriptStatus> externalTranscriptStatus)
            {
                ApplValcodes applValcodeResponse = new ApplValcodes();
                applValcodeResponse.ValsEntityAssociation = new List<ApplValcodesVals>();
                foreach (var item in externalTranscriptStatus)
                {
                    applValcodeResponse.ValsEntityAssociation.Add(new ApplValcodesVals("", item.Description, "", item.Code, "", "", ""));
                }
                return applValcodeResponse;
            }
        }

        [TestClass]
        public class GownSize
        {
            Mock<IColleagueTransactionFactory> transFactoryMock;
            Mock<ICacheProvider> cacheProviderMock;
            Mock<IColleagueDataReader> dataAccessorMock;
            Mock<ILogger> loggerMock;
            IEnumerable<Ellucian.Colleague.Domain.Student.Entities.GownSize> allGownSizes;
            ApplValcodes gownSizesValcodeResponse;
            string valcodeName;
            ApiSettings apiSettings;
            StudentReferenceDataRepository referenceDataRepo;

            [TestInitialize]
            public void Initialize()
            {
                loggerMock = new Mock<ILogger>();

                // Build responses used for mocking
                allGownSizes = new TestStudentReferenceDataRepository().GetGownSizesAsync().Result;

                gownSizesValcodeResponse = BuildValcodeResponse(allGownSizes);

                // Build repository
                referenceDataRepo = BuildValidReferenceDataRepository();
                valcodeName = referenceDataRepo.BuildFullCacheKey("ST_GRADUATION.GOWN.SIZES");

                cacheProviderMock.Setup<Task<Tuple<object, SemaphoreSlim>>>(x =>
                   x.GetAndLockSemaphoreAsync(It.IsAny<string>(), null))
                   .ReturnsAsync(new Tuple<object, SemaphoreSlim>(null, new SemaphoreSlim(1, 1)));

            }

            [TestCleanup]
            public void Cleanup()
            {
                transFactoryMock = null;
                dataAccessorMock = null;
                cacheProviderMock = null;
                gownSizesValcodeResponse = null;
                allGownSizes = null;
                referenceDataRepo = null;
            }

            [TestMethod]
            public async Task StudentReferenceDataRepo_GetGownSize()
            {
                var repo = await referenceDataRepo.GetGownSizesAsync();

                for (int i = 0; i < allGownSizes.Count(); i++)
                {
                    Assert.AreEqual(allGownSizes.ElementAt(i).Code, repo.ElementAt(i).Code);
                    Assert.AreEqual(allGownSizes.ElementAt(i).Description, repo.ElementAt(i).Description);
                }
            }

            [TestMethod]
            public async Task StudentReferenceDataRepo_GetGownSizes_WritesToCache()
            {
                // Set up local cache mock to respond to cache request:
                //  -to "Contains" request, return "false" to indicate item is not in cache
                //  -to cache "Get" request, return null so we know it's reading from the "repository"
                cacheProviderMock.Setup(x => x.Contains(valcodeName, null)).Returns(false);
                cacheProviderMock.Setup(x => x.Get(valcodeName, null)).Returns(null);

                // return a valid response to the data accessor request
                dataAccessorMock.Setup(acc => acc.ReadRecordAsync<ApplValcodes>(It.IsAny<string>(), It.IsAny<string>(), true)).ReturnsAsync(gownSizesValcodeResponse);

                // But after data accessor read, set up mocking so we can verify the list of gown sizes was written to the cache
                cacheProviderMock.Setup(x => x.AddAndUnlockSemaphore(valcodeName, It.IsAny<Object>(), It.IsAny<SemaphoreSlim>(), It.IsAny<CacheItemPolicy>(), null)).Verifiable();

                // Verify that gown sizes were returned, which means they came from the "repository".
                Assert.IsTrue((await referenceDataRepo.GetGownSizesAsync()).Count() == allGownSizes.Count());

                // Verify that the gown sizes was added to the cache after it was read from the repository
                cacheProviderMock.Verify(m => m.AddAndUnlockSemaphore(valcodeName, It.IsAny<Object>(), It.IsAny<SemaphoreSlim>(), It.IsAny<CacheItemPolicy>(), null));
            }

            [TestMethod]
            public async Task StudentReferenceDataRepo_GetGownSizes_Cached()
            {
                object lockHandle = null;
                // Set up local cache mock to respond to cache request:
                //  -to "Contains" request, return "false" to indicate item is not in cache
                //  -to cache "Get" request, return null so we know it's getting data from "repository"
                cacheProviderMock.Setup(x => x.Contains(valcodeName, null)).Returns(false);
                cacheProviderMock.Setup(x => x.GetAndLock(valcodeName, out lockHandle, null)).Returns(null);

                // Get these codes to populate the cache
                var gownSizes = (await referenceDataRepo.GetGownSizesAsync());
                // Assert the gown sizes are returned
                Assert.IsTrue((await referenceDataRepo.GetGownSizesAsync()).Count() == allGownSizes.Count());
                // Make sure we can verify that it's in the cache
                cacheProviderMock.Setup(x => x.Contains(valcodeName, null)).Returns(true);
                var gownSizesTuple = new Tuple<object, SemaphoreSlim>(gownSizes, new SemaphoreSlim(1, 1));
                cacheProviderMock.Setup(x => x.GetAndLockSemaphoreAsync(valcodeName, null)).ReturnsAsync(gownSizesTuple).Verifiable();

                // Verify that the gown sizes are now retrieved from cache
                gownSizes = (await referenceDataRepo.GetGownSizesAsync());
                cacheProviderMock.Verify(m => m.GetAndLockSemaphoreAsync(valcodeName, null));
            }

            private T GetCache<T>(ICacheProvider cacheProvider, string key)
                where T : class
            {
                object cache = cacheProvider.Get(key, null);
                return cache as T;
            }

            private StudentReferenceDataRepository BuildValidReferenceDataRepository()
            {
                // transaction factory mock
                transFactoryMock = new Mock<IColleagueTransactionFactory>();
                // Cache Mock
                //localCacheMock = new Mock<ObjectCache>();
                // Cache Provider Mock
                cacheProviderMock = new Mock<ICacheProvider>();
                // Set up data accessor for mocking 
                dataAccessorMock = new Mock<IColleagueDataReader>();
                apiSettings = new ApiSettings("TEST");

                // Set up dataAccessorMock as the object for the DataAccessor
                transFactoryMock.Setup(transFac => transFac.GetDataReader()).Returns(dataAccessorMock.Object);

                // Setup response to gown size valcode read
                dataAccessorMock.Setup(acc => acc.ReadRecordAsync<ApplValcodes>("ST.VALCODES", "GRADUATION.GOWN.SIZES", true)).ReturnsAsync(gownSizesValcodeResponse);

                // Construct repository
                referenceDataRepo = new StudentReferenceDataRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object, apiSettings);

                return referenceDataRepo;
            }

            private ApplValcodes BuildValcodeResponse(IEnumerable<Ellucian.Colleague.Domain.Student.Entities.GownSize> gownSizes)
            {
                ApplValcodes applValcodeResponse = new ApplValcodes();
                applValcodeResponse.ValsEntityAssociation = new List<ApplValcodesVals>();
                foreach (var item in gownSizes)
                {
                    applValcodeResponse.ValsEntityAssociation.Add(new ApplValcodesVals("", item.Description, "", item.Code, "", "", ""));
                }
                return applValcodeResponse;
            }
        }

        [TestClass]
        public class GradeSchemes
        {
            Mock<IColleagueTransactionFactory> _transFactoryMock;
            Mock<ICacheProvider> _cacheProviderMock;
            Mock<IColleagueDataReader> _dataAccessorMock;
            Mock<ILogger> _loggerMock;
            IEnumerable<GradeScheme> _allGradeScheme;
   
            ApiSettings _apiSettings;
            StudentReferenceDataRepository _referenceDataRepo;

            [TestInitialize]
            public void Initialize()
            {
                _loggerMock = new Mock<ILogger>();
                _apiSettings = new ApiSettings("TEST");

                // Build Grade Scheme responses used for mocking
                _allGradeScheme = new TestStudentReferenceDataRepository().GetGradeSchemesAsync().Result;

                // Build Grade Scheme repository
                _referenceDataRepo = BuildValidReferenceDataRepository();             
            }

            [TestCleanup]
            public void Cleanup()
            {
                _transFactoryMock = null;
                _dataAccessorMock = null;
                _cacheProviderMock = null;           
                _allGradeScheme = null;
                _referenceDataRepo = null;
            }

            [TestMethod]
            public async Task StudentReferenceDataRepo_GradeSchemes_GetGradeSchemesAsync_Cache()
            {
                var repoGetGradeSchemes = (await _referenceDataRepo.GetGradeSchemesAsync()).ToList();
                for (var i = 0; i < _allGradeScheme.Count(); i++)
                {
                    Assert.AreEqual(_allGradeScheme.ElementAt(i).Code, repoGetGradeSchemes.ElementAt(i).Code);
                    Assert.AreEqual(_allGradeScheme.ElementAt(i).Description, repoGetGradeSchemes.ElementAt(i).Description);
                    Assert.AreEqual(_allGradeScheme.ElementAt(i).Guid, repoGetGradeSchemes.ElementAt(i).Guid);
                    Assert.AreEqual(_allGradeScheme.ElementAt(i).EffectiveStartDate, repoGetGradeSchemes.ElementAt(i).EffectiveStartDate);
                    Assert.AreEqual(_allGradeScheme.ElementAt(i).EffectiveEndDate, repoGetGradeSchemes.ElementAt(i).EffectiveEndDate);
                }
            }

            [TestMethod]
            public async Task StudentReferenceDataRepo_GradeSchemes_GetGradeSchemesAsync_NoCache()
            {
                var repoGetGradeSchemes = (await _referenceDataRepo.GetGradeSchemesAsync(true)).ToList();
                for (var i = 0; i < _allGradeScheme.Count(); i++)
                {
                    Assert.AreEqual(_allGradeScheme.ElementAt(i).Code, repoGetGradeSchemes.ElementAt(i).Code);
                    Assert.AreEqual(_allGradeScheme.ElementAt(i).Description, repoGetGradeSchemes.ElementAt(i).Description);
                    Assert.AreEqual(_allGradeScheme.ElementAt(i).Guid, repoGetGradeSchemes.ElementAt(i).Guid);
                    Assert.AreEqual(_allGradeScheme.ElementAt(i).EffectiveStartDate, repoGetGradeSchemes.ElementAt(i).EffectiveStartDate);
                    Assert.AreEqual(_allGradeScheme.ElementAt(i).EffectiveEndDate, repoGetGradeSchemes.ElementAt(i).EffectiveEndDate);
               
                }
            }

            private StudentReferenceDataRepository BuildValidReferenceDataRepository()
            {
                // transaction factory mock
                _transFactoryMock = new Mock<IColleagueTransactionFactory>();
                  // Cache Provider Mock
                _cacheProviderMock = new Mock<ICacheProvider>();
                // Set up data accessor for mocking 
                _dataAccessorMock = new Mock<IColleagueDataReader>();
                _apiSettings = new ApiSettings("TEST");

                // Set up dataAccessorMock as the object for the DataAccessor
                _transFactoryMock.Setup(transFac => transFac.GetDataReader()).Returns(_dataAccessorMock.Object);

                var records = new Collection<DataContracts.GradeSchemes>();
                foreach (var item in _allGradeScheme)
                {
                    var record = new DataContracts.GradeSchemes
                    {
                        RecordGuid = item.Guid,
                        GrsDesc = item.Description,
                        Recordkey = item.Code,
                        GrsStartDate = item.EffectiveStartDate,
                        GrsEndDate = item.EffectiveEndDate
                    };
                    records.Add(record);
                }
                _dataAccessorMock.Setup(acc => acc.BulkReadRecordAsync<DataContracts.GradeSchemes>("GRADE.SCHEMES", "", true)).ReturnsAsync(records);

                _cacheProviderMock.Setup<Task<Tuple<object, SemaphoreSlim>>>(x =>
                 x.GetAndLockSemaphoreAsync(It.IsAny<string>(), null))
                 .ReturnsAsync(new Tuple<object, SemaphoreSlim>(null, new SemaphoreSlim(1, 1)));

                _dataAccessorMock.Setup(acc => acc.SelectAsync(It.IsAny<RecordKeyLookup[]>())).Returns<RecordKeyLookup[]>(recordKeyLookups =>
                {
                    var result = new Dictionary<string, RecordKeyLookupResult>();
                    foreach (var recordKeyLookup in recordKeyLookups)
                    {
                        var record = _allGradeScheme.FirstOrDefault(e => e.Code == recordKeyLookup.PrimaryKey);
                        result.Add(string.Join("+", new string[] { "GRADE.SCHEMES", record.Code }),
                            new RecordKeyLookupResult() { Guid = record.Guid });
                    }
                    return Task.FromResult(result);
                });

                // Construct repository
                _referenceDataRepo = new StudentReferenceDataRepository(_cacheProviderMock.Object, _transFactoryMock.Object, _loggerMock.Object, _apiSettings);

                return _referenceDataRepo;
            }
        }

        [TestClass]
        public class HousingResidentTypes
        {
            Mock<IColleagueTransactionFactory> transFactoryMock;
            Mock<ICacheProvider> cacheProviderMock;
            Mock<IColleagueDataReader> dataAccessorMock;
            Mock<ILogger> loggerMock;
            IEnumerable<HousingResidentType> allHousingResidentTypes;
            ApplValcodes housingResidentTypeValcodeResponse;
            string valcodeName;
            ApiSettings apiSettings;

            Mock<IStudentReferenceDataRepository> referenceDataRepositoryMock;
            IStudentReferenceDataRepository referenceDataRepository;
            StudentReferenceDataRepository referenceDataRepo;

            [TestInitialize]
            public void Initialize()
            {
                loggerMock = new Mock<ILogger>();
                apiSettings = new ApiSettings("TEST");

                // Build section grade types responses used for mocking
                allHousingResidentTypes = new TestStudentReferenceDataRepository().GetHousingResidentTypesAsync(false).Result;
                housingResidentTypeValcodeResponse = BuildValcodeResponse(allHousingResidentTypes);
                var housingResidentTypeValResponse = new List<string>() { "2" };
                housingResidentTypeValcodeResponse.ValActionCode1 = housingResidentTypeValResponse;

                referenceDataRepositoryMock = new Mock<IStudentReferenceDataRepository>();
                referenceDataRepository = referenceDataRepositoryMock.Object;

                // Build HousingResidentType repository
                referenceDataRepo = BuildValidReferenceDataRepository();
                valcodeName = referenceDataRepo.BuildFullCacheKey("ST_ROOM.ASSIGN.STAFF.CODES_GUID");
            }

            [TestCleanup]
            public void Cleanup()
            {
                transFactoryMock = null;
                dataAccessorMock = null;
                cacheProviderMock = null;
                housingResidentTypeValcodeResponse = null;
                allHousingResidentTypes = null;
                referenceDataRepo = null;
            }

            [TestMethod]
            public async Task StudentReferenceDataRepo_GetHousingResidentTypesCacheAsync()
            {
                var types = await referenceDataRepo.GetHousingResidentTypesAsync(false);

                for (int i = 0; i < allHousingResidentTypes.Count(); i++)
                {
                    Assert.AreEqual(allHousingResidentTypes.ElementAt(i).Code, types.ElementAt(i).Code);
                    Assert.AreEqual(allHousingResidentTypes.ElementAt(i).Description, types.ElementAt(i).Description);
                }
            }

            [TestMethod]
            public async Task StudentReferenceDataRepo_GetHousingResidentTypesNonCacheAsync()
            {
                var types = await referenceDataRepo.GetHousingResidentTypesAsync(true);

                for (int i = 0; i < allHousingResidentTypes.Count(); i++)
                {
                    Assert.AreEqual(allHousingResidentTypes.ElementAt(i).Code, types.ElementAt(i).Code);
                    Assert.AreEqual(allHousingResidentTypes.ElementAt(i).Description, types.ElementAt(i).Description);
                }
            }

            [TestMethod]
            public async Task StudentReferenceDataRepo_GetHousingResidentTypesNoArgAsync()
            {
                var types = await referenceDataRepo.GetHousingResidentTypesAsync(false);

                for (int i = 0; i < allHousingResidentTypes.Count(); i++)
                {
                    Assert.AreEqual(allHousingResidentTypes.ElementAt(i).Code, types.ElementAt(i).Code);
                    Assert.AreEqual(allHousingResidentTypes.ElementAt(i).Description, types.ElementAt(i).Description);
                }
            }

            [TestMethod]
            public async Task StudentReferenceDataRepo_GetHousingResidentTypes_WritesToCacheAsync()
            {

                // Set up local cache mock to respond to cache request:
                //  -to "Contains" request, return "false" to indicate item is not in cache
                //  -to cache "Get" request, return null so we know it's reading from the "repository"
                cacheProviderMock.Setup(x => x.Contains(valcodeName, null)).Returns(false);
                cacheProviderMock.Setup(x => x.Get(valcodeName, null)).Returns(null);

                // return a valid response to the data accessor request
                dataAccessorMock.Setup(acc => acc.ReadRecordAsync<ApplValcodes>("ST.VALCODES", "ROOM.ASSIGN.STAFF.CODES", It.IsAny<bool>())).ReturnsAsync(housingResidentTypeValcodeResponse);

                cacheProviderMock.Setup<Task<Tuple<object, SemaphoreSlim>>>(x =>
                 x.GetAndLockSemaphoreAsync(It.IsAny<string>(), null))
                 .ReturnsAsync(new Tuple<object, SemaphoreSlim>(null, new SemaphoreSlim(1, 1)));

                // But after data accessor read, set up mocking so we can verify the list of HousingResidentTypes was written to the cache
                cacheProviderMock.Setup(x => x.Add(It.IsAny<string>(), It.IsAny<Task<List<HousingResidentType>>>(), It.IsAny<CacheItemPolicy>(), null)).Verifiable();

                cacheProviderMock.Setup(x => x.Contains(referenceDataRepo.BuildFullCacheKey("ST_ROOM.ASSIGN.STAFF.CODES"), null)).Returns(true);
                var housingResidentTypes = await referenceDataRepo.GetHousingResidentTypesAsync(false);
                cacheProviderMock.Setup(x => x.Get(referenceDataRepo.BuildFullCacheKey("ST_ROOM.ASSIGN.STAFF.CODES"), null)).Returns(housingResidentTypes);
                // Verify that HousingResidentTypes were returned, which means they came from the "repository".
                Assert.IsTrue(housingResidentTypes.Count() == 3);

                // Verify that the HousingResidentType item was added to the cache after it was read from the repository
                cacheProviderMock.Verify(m => m.Add(It.IsAny<string>(), It.IsAny<Task<List<HousingResidentType>>>(), It.IsAny<CacheItemPolicy>(), null), Times.Never);
            }

            [TestMethod]
            public async Task StudentReferenceDataRepo_GetHousingResidentTypes_GetsCachedHousingResidentTypes()
            {
                // Set up local cache mock to respond to cache request:
                //  -to "Contains" request, return "true" to indicate item is in cache
                //  -to "Get" request, return the cache item (in this case the "INTG.SECTION.GRADE.TYPES" cache item)
                cacheProviderMock.Setup(x => x.Contains(valcodeName, null)).Returns(true);
                cacheProviderMock.Setup(x => x.Get(valcodeName, null)).Returns(allHousingResidentTypes).Verifiable();

                // return null for request, so that if we have a result, it wasn't the data accessor that returned it.
                dataAccessorMock.Setup(acc => acc.ReadRecordAsync<ApplValcodes>("ST.VALCODES", "ROOM.ASSIGN.STAFF.CODES", true)).ReturnsAsync(new ApplValcodes());

                // Assert the HousingResidentTypes are returned
                Assert.IsTrue((await referenceDataRepo.GetHousingResidentTypesAsync(false)).Count() == allHousingResidentTypes.Count());
                // Verify that the HousingResidentTypes were retrieved from cache
                cacheProviderMock.Verify(m => m.Get(valcodeName, null));
            }

            private StudentReferenceDataRepository BuildValidReferenceDataRepository()
            {
                // transaction factory mock
                transFactoryMock = new Mock<IColleagueTransactionFactory>();
                // Cache Provider Mock
                cacheProviderMock = new Mock<ICacheProvider>();
                // Set up data accessor for mocking 
                dataAccessorMock = new Mock<IColleagueDataReader>();

                // Set up dataAccessorMock as the object for the DataAccessor
                transFactoryMock.Setup(transFac => transFac.GetDataReader()).Returns(dataAccessorMock.Object);

                // Setup response to HousingResidentType valcode read
                dataAccessorMock.Setup(acc => acc.ReadRecordAsync<ApplValcodes>("ST.VALCODES", "ROOM.ASSIGN.STAFF.CODES", It.IsAny<bool>())).ReturnsAsync(housingResidentTypeValcodeResponse);
                cacheProviderMock.Setup<Task<Tuple<object, SemaphoreSlim>>>(x => x.GetAndLockSemaphoreAsync(It.IsAny<string>(), null))
                .ReturnsAsync(new Tuple<object, SemaphoreSlim>(null, new SemaphoreSlim(1, 1)));

                dataAccessorMock.Setup(acc => acc.SelectAsync(It.IsAny<RecordKeyLookup[]>())).Returns<RecordKeyLookup[]>(recordKeyLookups =>
                {
                    var result = new Dictionary<string, RecordKeyLookupResult>();
                    foreach (var recordKeyLookup in recordKeyLookups)
                    {
                        var housingResidentType = allHousingResidentTypes.Where(e => e.Code == recordKeyLookup.SecondaryKey).FirstOrDefault();
                        result.Add(string.Join("+", new string[] { "ST.VALCODES", "ROOM.ASSIGN.STAFF.CODES", housingResidentType.Code }),
                            new RecordKeyLookupResult() { Guid = housingResidentType.Guid });
                    }
                    return Task.FromResult(result);
                });

                // Construct repository
                referenceDataRepo = new StudentReferenceDataRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object, apiSettings);

                return referenceDataRepo;
            }

            private ApplValcodes BuildValcodeResponse(IEnumerable<HousingResidentType> housingResidentTypes)
            {
                ApplValcodes housingResidentTypesResponse = new ApplValcodes();
                housingResidentTypesResponse.ValsEntityAssociation = new List<ApplValcodesVals>();
                foreach (var item in housingResidentTypes)
                {
                    housingResidentTypesResponse.ValsEntityAssociation.Add(new ApplValcodesVals("", item.Description, "2", item.Code, "3", "", ""));
                }
                return housingResidentTypesResponse;
            }
        }
        
        [TestClass]
        public class InstructionalMethods
        {
            Mock<IColleagueTransactionFactory> transFactoryMock;
            Mock<ICacheProvider> cacheProviderMock;
            Mock<IColleagueDataReader> dataAccessorMock;
            Mock<ILogger> loggerMock;
            IEnumerable<InstructionalMethod> allInstructionalMethod;
            ApplValcodes instructionalMethodValcodeResponse;
            string valcodeName;
            ApiSettings apiSettings;

            StudentReferenceDataRepository referenceDataRepo;

            [TestInitialize]
            public void Initialize()
            {
                loggerMock = new Mock<ILogger>();
                apiSettings = new ApiSettings("TEST");

                // Build Instructional Method responses used for mocking
                allInstructionalMethod = new TestStudentReferenceDataRepository().GetInstructionalMethodsAsync().Result;
                instructionalMethodValcodeResponse = BuildValcodeResponse(allInstructionalMethod);

                // Build Instructional Method repository
                referenceDataRepo = BuildValidReferenceDataRepository();
                valcodeName = referenceDataRepo.BuildFullCacheKey("AllInstructionalMethods");
            }

            [TestCleanup]
            public void Cleanup()
            {
                transFactoryMock = null;
                dataAccessorMock = null;
                cacheProviderMock = null;
                instructionalMethodValcodeResponse = null;
                allInstructionalMethod = null;
                referenceDataRepo = null;
            }

            [TestMethod]
            public async Task StudentReferenceDataRepo_GetInstructionalMethods_Cache()
            {
                var repoGetInstructionalMethods = await referenceDataRepo.GetInstructionalMethodsAsync(false);
                for (int i = 0; i < allInstructionalMethod.Count(); i++)
                {
                    Assert.AreEqual(allInstructionalMethod.ElementAt(i).Code, repoGetInstructionalMethods.ElementAt(i).Code);
                    Assert.AreEqual(allInstructionalMethod.ElementAt(i).Description, repoGetInstructionalMethods.ElementAt(i).Description);
                }
            }

            [TestMethod]
            public async Task StudentReferenceDataRepo_GetInstructionalMethods_NoCache()
            {
                var repoGetInstructionalMethods = await referenceDataRepo.GetInstructionalMethodsAsync(true);
                for (int i = 0; i < allInstructionalMethod.Count(); i++)
                {
                    Assert.AreEqual(allInstructionalMethod.ElementAt(i).Code, repoGetInstructionalMethods.ElementAt(i).Code);
                    Assert.AreEqual(allInstructionalMethod.ElementAt(i).Description, repoGetInstructionalMethods.ElementAt(i).Description);
                }
            }

            [TestMethod]
            public async Task StudentReferenceDataRepo_GetInstructionalMethods_NoArgument()
            {
                var repoGetInstructionalMethods = await referenceDataRepo.GetInstructionalMethodsAsync();
                for (int i = 0; i < allInstructionalMethod.Count(); i++)
                {
                    Assert.AreEqual(allInstructionalMethod.ElementAt(i).Code, repoGetInstructionalMethods.ElementAt(i).Code);
                    Assert.AreEqual(allInstructionalMethod.ElementAt(i).Description, repoGetInstructionalMethods.ElementAt(i).Description);
                }
            }

            private StudentReferenceDataRepository BuildValidReferenceDataRepository()
            {
                // transaction factory mock
                transFactoryMock = new Mock<IColleagueTransactionFactory>();
                // Cache Mock
                //localCacheMock = new Mock<ObjectCache>();
                // Cache Provider Mock
                cacheProviderMock = new Mock<ICacheProvider>();
                // Set up data accessor for mocking 
                dataAccessorMock = new Mock<IColleagueDataReader>();
                apiSettings = new ApiSettings("TEST");

                // Set up dataAccessorMock as the object for the DataAccessor
                transFactoryMock.Setup(transFac => transFac.GetDataReader()).Returns(dataAccessorMock.Object);

                var records = new Collection<DataContracts.InstrMethods>();
                foreach (var item in allInstructionalMethod)
                {
                    DataContracts.InstrMethods record = new DataContracts.InstrMethods();
                    record.RecordGuid = item.Guid;
                    record.InmDesc = item.Description;
                    record.Recordkey = item.Code;
                    records.Add(record);
                }
                dataAccessorMock.Setup(acc => acc.BulkReadRecordAsync<DataContracts.InstrMethods>("INSTR.METHODS", "", true)).ReturnsAsync(records);

                cacheProviderMock.Setup<Task<Tuple<object, SemaphoreSlim>>>(x =>
                 x.GetAndLockSemaphoreAsync(It.IsAny<string>(), null))
                 .ReturnsAsync(new Tuple<object, SemaphoreSlim>(null, new SemaphoreSlim(1, 1)));

                dataAccessorMock.Setup(acc => acc.SelectAsync(It.IsAny<RecordKeyLookup[]>())).Returns<RecordKeyLookup[]>(recordKeyLookups =>
                {
                    var result = new Dictionary<string, RecordKeyLookupResult>();
                    foreach (var recordKeyLookup in recordKeyLookups)
                    {
                        var record = allInstructionalMethod.Where(e => e.Code == recordKeyLookup.PrimaryKey).FirstOrDefault();
                        result.Add(string.Join("+", new string[] { "INSTR.METHODS", record.Code }),
                            new RecordKeyLookupResult() { Guid = record.Guid });
                    }
                    return Task.FromResult(result);
                });

                // Construct repository
                referenceDataRepo = new StudentReferenceDataRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object, apiSettings);

                return referenceDataRepo;
            }

            private ApplValcodes BuildValcodeResponse(IEnumerable<InstructionalMethod> instructionalMethod)
            {
                ApplValcodes instructionalMethodResponse = new ApplValcodes();
                instructionalMethodResponse.ValsEntityAssociation = new List<ApplValcodesVals>();
                foreach (var item in instructionalMethod)
                {
                    instructionalMethodResponse.ValsEntityAssociation.Add(new ApplValcodesVals("", item.Description, "", item.Code, "", "", ""));
                }
                return instructionalMethodResponse;
            }
        }

        [TestClass]
        public class IntgTestPercentileTypes
        {
            Mock<IColleagueTransactionFactory> transFactoryMock;
            Mock<ICacheProvider> cacheProviderMock;
            Mock<IColleagueDataReader> dataAccessorMock;
            Mock<ILogger> loggerMock;
            IEnumerable<IntgTestPercentileType> allIntgTestPercentileTypes;
            ApplValcodes intgTestPercentileTypesValcodeResponse;
            string valcodeName;
            ApiSettings apiSettings;

            Mock<IStudentReferenceDataRepository> referenceDataRepositoryMock;
            IStudentReferenceDataRepository referenceDataRepository;
            StudentReferenceDataRepository referenceDataRepo;

            [TestInitialize]
            public void Initialize()
            {
                loggerMock = new Mock<ILogger>();
                apiSettings = new ApiSettings("TEST");

                // Build academic standings responses used for mocking
                allIntgTestPercentileTypes = new TestStudentReferenceDataRepository().GetIntgTestPercentileTypesAsync(false).Result;
                intgTestPercentileTypesValcodeResponse = BuildValcodeResponse(allIntgTestPercentileTypes);
                var intgTestPercentileTypesValResponse = new List<string>() { "2" };
                intgTestPercentileTypesValcodeResponse.ValActionCode1 = intgTestPercentileTypesValResponse;

                referenceDataRepositoryMock = new Mock<IStudentReferenceDataRepository>();
                referenceDataRepository = referenceDataRepositoryMock.Object;

                // Build academic standing repository
                referenceDataRepo = BuildValidReferenceDataRepository();
                valcodeName = referenceDataRepo.BuildFullCacheKey("ST_INTG.TEST.PERCENTILE.TYPES_GUID");

                cacheProviderMock.Setup<Task<Tuple<object, SemaphoreSlim>>>(x =>
                   x.GetAndLockSemaphoreAsync(It.IsAny<string>(), null))
                   .ReturnsAsync(new Tuple<object, SemaphoreSlim>(null, new SemaphoreSlim(1, 1)));
            }

            [TestCleanup]
            public void Cleanup()
            {
                transFactoryMock = null;
                dataAccessorMock = null;
                cacheProviderMock = null;
                intgTestPercentileTypesValcodeResponse = null;
                allIntgTestPercentileTypes = null;
                referenceDataRepo = null;
            }

            [TestMethod]
            public async Task StudentReferenceDataRepo_GetsIntgTestPercentileTypesNoArgAsync()
            {
                var intgTestPercentileTypes = await referenceDataRepo.GetIntgTestPercentileTypesAsync(true);

                for (int i = 0; i < allIntgTestPercentileTypes.Count(); i++)
                {
                    Assert.AreEqual(allIntgTestPercentileTypes.ElementAt(i).Code, intgTestPercentileTypes.ElementAt(i).Code);
                    Assert.AreEqual(allIntgTestPercentileTypes.ElementAt(i).Description, intgTestPercentileTypes.ElementAt(i).Description);
                }
            }

            [TestMethod]
            public async Task StudentReferenceDataRepo_GetsIntgTestPercentileTypesCacheAsync()
            {
                var intgTestPercentileTypes = await referenceDataRepo.GetIntgTestPercentileTypesAsync(false);

                for (int i = 0; i < allIntgTestPercentileTypes.Count(); i++)
                {
                    Assert.AreEqual(allIntgTestPercentileTypes.ElementAt(i).Code, intgTestPercentileTypes.ElementAt(i).Code);
                    Assert.AreEqual(allIntgTestPercentileTypes.ElementAt(i).Description, intgTestPercentileTypes.ElementAt(i).Description);
                }
            }

            [TestMethod]
            public async Task StudentReferenceDataRepo_GetsIntgTestPercentileTypesNonCacheAsync()
            {
                var statuses = await referenceDataRepo.GetIntgTestPercentileTypesAsync(true);

                for (int i = 0; i < allIntgTestPercentileTypes.Count(); i++)
                {
                    Assert.AreEqual(allIntgTestPercentileTypes.ElementAt(i).Code, statuses.ElementAt(i).Code);
                    Assert.AreEqual(allIntgTestPercentileTypes.ElementAt(i).Description, statuses.ElementAt(i).Description);
                }
            }

            [TestMethod]
            public async Task StudentReferenceDataRepo_GetIntgTestPercentileTypes_WritesToCacheAsync()
            {

                // Set up local cache mock to respond to cache request:
                //  -to "Contains" request, return "false" to indicate item is not in cache
                //  -to cache "Get" request, return null so we know it's reading from the "repository"
                cacheProviderMock.Setup(x => x.Contains(valcodeName, null)).Returns(false);
                cacheProviderMock.Setup(x => x.Get(valcodeName, null)).Returns(null);

                // return a valid response to the data accessor request
                dataAccessorMock.Setup(acc => acc.ReadRecordAsync<ApplValcodes>("ST.VALCODES", "INTG.TEST.PERCENTILE.TYPES", It.IsAny<bool>())).ReturnsAsync(intgTestPercentileTypesValcodeResponse);

                cacheProviderMock.Setup<Task<Tuple<object, SemaphoreSlim>>>(x =>
                 x.GetAndLockSemaphoreAsync(It.IsAny<string>(), null))
                 .ReturnsAsync(new Tuple<object, SemaphoreSlim>(null, new SemaphoreSlim(1, 1)));

                // But after data accessor read, set up mocking so we can verify the list of intgTestPercentileTypes was written to the cache
                cacheProviderMock.Setup(x => x.Add(It.IsAny<string>(), It.IsAny<Task<List<IntgTestPercentileTypes>>>(), It.IsAny<CacheItemPolicy>(), null)).Verifiable();

                cacheProviderMock.Setup(x => x.Contains(referenceDataRepo.BuildFullCacheKey("ST_INTG.TEST.PERCENTILE.TYPES"), null)).Returns(true);
                var intgTestPercentileTypes = await referenceDataRepo.GetIntgTestPercentileTypesAsync(false);
                cacheProviderMock.Setup(x => x.Get(referenceDataRepo.BuildFullCacheKey("ST_INTG.TEST.PERCENTILE.TYPES"), null)).Returns(intgTestPercentileTypes);
                // Verify that intgTestPercentileTypes were returned, which means they came from the "repository".
                Assert.IsTrue(intgTestPercentileTypes.Count() == 2);

                // Verify that the intgTestPercentileTypes item was added to the cache after it was read from the repository
                cacheProviderMock.Verify(m => m.Add(It.IsAny<string>(), It.IsAny<Task<List<IntgTestPercentileTypes>>>(), It.IsAny<CacheItemPolicy>(), null), Times.Never);

            }

            [TestMethod]
            public async Task StudentReferenceDataRepo_GetIntgTestPercentileTypes_GetsCachedIntgTestPercentileTypesAsync()
            {
                // Set up local cache mock to respond to cache request:
                //  -to "Contains" request, return "true" to indicate item is in cache
                //  -to "Get" request, return the cache item (in this case the "INTG.TEST.PERCENTILE.TYPES" cache item)
                cacheProviderMock.Setup(x => x.Contains(valcodeName, null)).Returns(true);
                cacheProviderMock.Setup(x => x.Get(valcodeName, null)).Returns(allIntgTestPercentileTypes).Verifiable();

                // return null for request, so that if we have a result, it wasn't the data accessor that returned it.
                dataAccessorMock.Setup(acc => acc.ReadRecordAsync<ApplValcodes>("ST.VALCODES", "INTG.TEST.PERCENTILE.TYPES", true)).ReturnsAsync(new ApplValcodes());

                // Assert the intgTestPercentileTypes are returned
                Assert.IsTrue((await referenceDataRepo.GetIntgTestPercentileTypesAsync(false)).Count() == 2);
                // Verify that the sintgTestPercentileTypes were retrieved from cache
                cacheProviderMock.Verify(m => m.Get(valcodeName, null));
            }

            private StudentReferenceDataRepository BuildValidReferenceDataRepository()
            {
                // transaction factory mock
                transFactoryMock = new Mock<IColleagueTransactionFactory>();
                // Cache Provider Mock
                cacheProviderMock = new Mock<ICacheProvider>();
                // Set up data accessor for mocking 
                dataAccessorMock = new Mock<IColleagueDataReader>();

                // Set up dataAccessorMock as the object for the DataAccessor
                transFactoryMock.Setup(transFac => transFac.GetDataReader()).Returns(dataAccessorMock.Object);

                // Setup response to intgTestPercentileTypes valcode read
                dataAccessorMock.Setup(acc => acc.ReadRecordAsync<ApplValcodes>("ST.VALCODES", "INTG.TEST.PERCENTILE.TYPES", It.IsAny<bool>())).ReturnsAsync(intgTestPercentileTypesValcodeResponse);
                cacheProviderMock.Setup<Task<Tuple<object, SemaphoreSlim>>>(x => x.GetAndLockSemaphoreAsync(It.IsAny<string>(), null))
                .ReturnsAsync(new Tuple<object, SemaphoreSlim>(null, new SemaphoreSlim(1, 1)));

                dataAccessorMock.Setup(acc => acc.SelectAsync(It.IsAny<RecordKeyLookup[]>())).Returns<RecordKeyLookup[]>(recordKeyLookups =>
                {
                    var result = new Dictionary<string, RecordKeyLookupResult>();
                    foreach (var recordKeyLookup in recordKeyLookups)
                    {
                        var intgTestPercentileTypes = allIntgTestPercentileTypes.Where(e => e.Code == recordKeyLookup.SecondaryKey).FirstOrDefault();
                        result.Add(string.Join("+", new string[] { "ST.VALCODES", "INTG.TEST.PERCENTILE.TYPES", intgTestPercentileTypes.Code }),
                            new RecordKeyLookupResult() { Guid = intgTestPercentileTypes.Guid });
                    }
                    return Task.FromResult(result);
                });

                // Construct repository
                referenceDataRepo = new StudentReferenceDataRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object, apiSettings);

                return referenceDataRepo;
            }

            private ApplValcodes BuildValcodeResponse(IEnumerable<IntgTestPercentileType> intgTestPercentileTypes)
            {
                ApplValcodes intgTestPercentileTypesResponse = new ApplValcodes();
                intgTestPercentileTypesResponse.ValsEntityAssociation = new List<ApplValcodesVals>();
                foreach (var item in intgTestPercentileTypes)
                {
                    intgTestPercentileTypesResponse.ValsEntityAssociation.Add(new ApplValcodesVals("", item.Description, "2", item.Code, "3", "", ""));
                }
                return intgTestPercentileTypesResponse;
            }
        }

        [TestClass]
        public class MealClassTests
        {
            Mock<IColleagueTransactionFactory> transFactoryMock;
            Mock<ICacheProvider> cacheProviderMock;
            Mock<IColleagueDataReader> dataAccessorMock;
            Mock<ILogger> loggerMock;
            IEnumerable<Domain.Student.Entities.StudentResidentialCategories> allMealClass;
            ApplValcodes studentResidentialCategoriesValcodeResponse;
            string domainEntityNameName;
            ApiSettings apiSettings;

            Mock<IStudentReferenceDataRepository> referenceDataRepositoryMock;
            IStudentReferenceDataRepository referenceDataRepository;
            StudentReferenceDataRepository referenceDataRepo;

            [TestInitialize]
            public void Initialize()
            {
                loggerMock = new Mock<ILogger>();
                apiSettings = new ApiSettings("TEST");

                // Build academic standings responses used for mocking
                allMealClass = new TestStudentReferenceDataRepository().GetStudentResidentialCategoriesAsync(false).Result;
                studentResidentialCategoriesValcodeResponse = BuildValcodeResponse(allMealClass);
                var studentResidentialCategoriesValResponse = new List<string>() { "2" };
                studentResidentialCategoriesValcodeResponse.ValActionCode1 = studentResidentialCategoriesValResponse;

                referenceDataRepositoryMock = new Mock<IStudentReferenceDataRepository>();
                referenceDataRepository = referenceDataRepositoryMock.Object;

                // Build academic standing repository
                referenceDataRepo = BuildValidReferenceDataRepository();
                domainEntityNameName = referenceDataRepo.BuildFullCacheKey("ST_MEAL.CLASS_GUID");

                cacheProviderMock.Setup<Task<Tuple<object, SemaphoreSlim>>>(x =>
                   x.GetAndLockSemaphoreAsync(It.IsAny<string>(), null))
                   .ReturnsAsync(new Tuple<object, SemaphoreSlim>(null, new SemaphoreSlim(1, 1)));
            }

            [TestCleanup]
            public void Cleanup()
            {
                transFactoryMock = null;
                dataAccessorMock = null;
                cacheProviderMock = null;
                studentResidentialCategoriesValcodeResponse = null;
                allMealClass = null;
                referenceDataRepo = null;
            }


            [TestMethod]
            public async Task StudentReferenceDataRepo_GetsMealClassCacheAsync()
            {
                var studentResidentialCategories = await referenceDataRepo.GetStudentResidentialCategoriesAsync(false);

                for (int i = 0; i < allMealClass.Count(); i++)
                {
                    Assert.AreEqual(allMealClass.ElementAt(i).Code, studentResidentialCategories.ElementAt(i).Code);
                    Assert.AreEqual(allMealClass.ElementAt(i).Description, studentResidentialCategories.ElementAt(i).Description);
                }
            }

            [TestMethod]
            public async Task StudentReferenceDataRepo_GetsMealClassNonCacheAsync()
            {
                var statuses = await referenceDataRepo.GetStudentResidentialCategoriesAsync(true);

                for (int i = 0; i < allMealClass.Count(); i++)
                {
                    Assert.AreEqual(allMealClass.ElementAt(i).Code, statuses.ElementAt(i).Code);
                    Assert.AreEqual(allMealClass.ElementAt(i).Description, statuses.ElementAt(i).Description);
                }
            }

            [TestMethod]
            public async Task StudentReferenceDataRepo_GetMealClass_WritesToCacheAsync()
            {

                // Set up local cache mock to respond to cache request:
                //  -to "Contains" request, return "false" to indicate item is not in cache
                //  -to cache "Get" request, return null so we know it's reading from the "repository"
                cacheProviderMock.Setup(x => x.Contains(domainEntityNameName, null)).Returns(false);
                cacheProviderMock.Setup(x => x.Get(domainEntityNameName, null)).Returns(null);

                // return a valid response to the data accessor request
                dataAccessorMock.Setup(acc => acc.ReadRecordAsync<ApplValcodes>("ST.VALCODES", "MEAL.CLASS", It.IsAny<bool>())).ReturnsAsync(studentResidentialCategoriesValcodeResponse);

                cacheProviderMock.Setup<Task<Tuple<object, SemaphoreSlim>>>(x =>
                 x.GetAndLockSemaphoreAsync(It.IsAny<string>(), null))
                 .ReturnsAsync(new Tuple<object, SemaphoreSlim>(null, new SemaphoreSlim(1, 1)));

                // But after data accessor read, set up mocking so we can verify the list of studentResidentialCategories was written to the cache
                cacheProviderMock.Setup(x => x.Add(It.IsAny<string>(), It.IsAny<Task<List<StudentResidentialCategories>>>(), It.IsAny<CacheItemPolicy>(), null)).Verifiable();

                cacheProviderMock.Setup(x => x.Contains(referenceDataRepo.BuildFullCacheKey("ST_MEAL.CLASS"), null)).Returns(true);
                var studentResidentialCategories = await referenceDataRepo.GetStudentResidentialCategoriesAsync(false);
                cacheProviderMock.Setup(x => x.Get(referenceDataRepo.BuildFullCacheKey("ST_MEAL.CLASS"), null)).Returns(studentResidentialCategories);
                // Verify that studentResidentialCategories were returned, which means they came from the "repository".
                Assert.IsTrue(studentResidentialCategories.Count() == 3);

                // Verify that the studentResidentialCategories item was added to the cache after it was read from the repository
                cacheProviderMock.Verify(m => m.Add(It.IsAny<string>(), It.IsAny<Task<List<StudentResidentialCategories>>>(), It.IsAny<CacheItemPolicy>(), null), Times.Never);

            }

            [TestMethod]
            public async Task StudentReferenceDataRepo_GetMealClass_GetsCachedMealClassAsync()
            {
                // Set up local cache mock to respond to cache request:
                //  -to "Contains" request, return "true" to indicate item is in cache
                //  -to "Get" request, return the cache item (in this case the "MEAL.CLASS" cache item)
                cacheProviderMock.Setup(x => x.Contains(domainEntityNameName, null)).Returns(true);
                cacheProviderMock.Setup(x => x.Get(domainEntityNameName, null)).Returns(allMealClass).Verifiable();

                // return null for request, so that if we have a result, it wasn't the data accessor that returned it.
                dataAccessorMock.Setup(acc => acc.ReadRecordAsync<ApplValcodes>("ST.VALCODES", "MEAL.CLASS", true)).ReturnsAsync(new ApplValcodes());

                // Assert the studentResidentialCategories are returned
                Assert.IsTrue((await referenceDataRepo.GetStudentResidentialCategoriesAsync(false)).Count() == 3);
                // Verify that the sstudentResidentialCategories were retrieved from cache
                cacheProviderMock.Verify(m => m.Get(domainEntityNameName, null));
            }

            private StudentReferenceDataRepository BuildValidReferenceDataRepository()
            {
                // transaction factory mock
                transFactoryMock = new Mock<IColleagueTransactionFactory>();
                // Cache Provider Mock
                cacheProviderMock = new Mock<ICacheProvider>();
                // Set up data accessor for mocking 
                dataAccessorMock = new Mock<IColleagueDataReader>();

                // Set up dataAccessorMock as the object for the DataAccessor
                transFactoryMock.Setup(transFac => transFac.GetDataReader()).Returns(dataAccessorMock.Object);

                // Setup response to studentResidentialCategories domainEntityName read
                dataAccessorMock.Setup(acc => acc.ReadRecordAsync<ApplValcodes>("ST.VALCODES", "MEAL.CLASS", It.IsAny<bool>())).ReturnsAsync(studentResidentialCategoriesValcodeResponse);
                cacheProviderMock.Setup<Task<Tuple<object, SemaphoreSlim>>>(x => x.GetAndLockSemaphoreAsync(It.IsAny<string>(), null))
                .ReturnsAsync(new Tuple<object, SemaphoreSlim>(null, new SemaphoreSlim(1, 1)));

                dataAccessorMock.Setup(acc => acc.SelectAsync(It.IsAny<RecordKeyLookup[]>())).Returns<RecordKeyLookup[]>(recordKeyLookups =>
                {
                    var result = new Dictionary<string, RecordKeyLookupResult>();
                    foreach (var recordKeyLookup in recordKeyLookups)
                    {
                        var studentResidentialCategories = allMealClass.Where(e => e.Code == recordKeyLookup.SecondaryKey).FirstOrDefault();
                        result.Add(string.Join("+", new string[] { "ST.VALCODES", "MEAL.CLASS", studentResidentialCategories.Code }),
                            new RecordKeyLookupResult() { Guid = studentResidentialCategories.Guid });
                    }
                    return Task.FromResult(result);
                });

                // Construct repository
                referenceDataRepo = new StudentReferenceDataRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object, apiSettings);

                return referenceDataRepo;
            }

            private ApplValcodes BuildValcodeResponse(IEnumerable<StudentResidentialCategories> studentResidentialCategories)
            {
                ApplValcodes studentResidentialCategoriesResponse = new ApplValcodes();
                studentResidentialCategoriesResponse.ValsEntityAssociation = new List<ApplValcodesVals>();
                foreach (var item in studentResidentialCategories)
                {
                    studentResidentialCategoriesResponse.ValsEntityAssociation.Add(new ApplValcodesVals("", item.Description, "2", item.Code, "3", "", ""));
                }
                return studentResidentialCategoriesResponse;
            }
        }

        [TestClass]
        public class MealPlanRateTests : BaseRepositorySetup
        {
            Mock<IColleagueTransactionFactory> transFactoryMock;
            Mock<IColleagueDataReader> dataReaderMock;
            Mock<ICacheProvider> cacheProviderMock;
            Mock<IColleagueDataReader> dataAccessorMock;
            Mock<ILogger> loggerMock;
            Collection<DataContracts.MealPlans> _mealPlansCollection;
            string codeItemName;
            ApiSettings apiSettings;

            StudentReferenceDataRepository referenceDataRepo;

            [TestInitialize]
            public void Initialize()
            {
                loggerMock = new Mock<ILogger>();
                dataAccessorMock = new Mock<IColleagueDataReader>();
                cacheProviderMock = new Mock<ICacheProvider>();
                transFactoryMock = new Mock<IColleagueTransactionFactory>();
                dataReaderMock = new Mock<IColleagueDataReader>();
                apiSettings = new ApiSettings("TEST");

                string id, guid, id2, guid2, id3, guid3, sid, sid2, sid3;
                GuidLookup guidLookup;
                GuidLookupResult guidLookupResult;
                Dictionary<string, GuidLookupResult> guidLookupDict;
                RecordKeyLookup recordLookup;
                RecordKeyLookupResult recordLookupResult;
                Dictionary<string, RecordKeyLookupResult> recordLookupDict;

                // Set up for GUID lookups
                id = "1";
                id2 = "2";
                id3 = "3";

                // Secondary keys for GUID lookups
                sid = "11";
                sid2 = "22";
                sid3 = "33";

                guid = "F5FC5310-17F1-49FC-926D-CC6E3DA6DAEA".ToLowerInvariant();
                guid2 = "5B35075D-14FB-45F7-858A-83F4174B76EA".ToLowerInvariant();
                guid3 = "246E16D9-8790-4D7E-ACA1-D5B1CB9D4A24".ToLowerInvariant();

                var offsetDate = DmiString.DateTimeToPickDate(DateTime.Now);

                guidLookup = new GuidLookup(guid);
                guidLookupResult = new GuidLookupResult() { Entity = "MEAL.PLANS", PrimaryKey = id, SecondaryKey = offsetDate.ToString() };
                guidLookupDict = new Dictionary<string, GuidLookupResult>();
                recordLookup = new RecordKeyLookup("MEAL.PLANS", id, "MEAL.RATE.EFFECTIVE.DATES", offsetDate.ToString(), false);
                recordLookupResult = new RecordKeyLookupResult() { Guid = guid };
                recordLookupDict = new Dictionary<string, RecordKeyLookupResult>();
                guidLookupDict.Add(guid, new GuidLookupResult() { Entity = "MEAL.PLANS", PrimaryKey = id, SecondaryKey = offsetDate.ToString() });
                guidLookupDict.Add(guid2, new GuidLookupResult() { Entity = "MEAL.PLANS", PrimaryKey = id2, SecondaryKey = offsetDate.ToString() });
                guidLookupDict.Add(guid3, new GuidLookupResult() { Entity = "MEAL.PLANS", PrimaryKey = id3, SecondaryKey = offsetDate.ToString() });

                recordLookupDict.Add("MEAL.PLANS+" + id + "+" + offsetDate.ToString(), new RecordKeyLookupResult() { Guid = guid });
                recordLookupDict.Add("MEAL.PLANS+" + id2 + "+" + offsetDate.ToString(), new RecordKeyLookupResult() { Guid = guid2 });
                recordLookupDict.Add("MEAL.PLANS+" + id3 + "+" + offsetDate.ToString(), new RecordKeyLookupResult() { Guid = guid3 });

                dataReaderMock.Setup(dr => dr.SelectAsync(It.IsAny<GuidLookup[]>())).ReturnsAsync(guidLookupDict);
                dataReaderMock.Setup(dr => dr.SelectAsync(It.IsAny<RecordKeyLookup[]>())).ReturnsAsync(recordLookupDict);
                dataAccessorMock.Setup(da => da.SelectAsync(It.IsAny<RecordKeyLookup[]>())).ReturnsAsync(recordLookupDict);


                // Build responses used for mocking
                _mealPlansCollection = new Collection<DataContracts.MealPlans>()
                {
                    new DataContracts.MealPlans() { RecordGuid = guid, Recordkey = id, MealDesc = sid ,
                        MealPlanRatesEntityAssociation = new List<DataContracts.MealPlansMealPlanRates>() { new DataContracts.MealPlansMealPlanRates(175, DateTime.Now ) } },
                    new DataContracts.MealPlans() { RecordGuid = guid2, Recordkey = id2, MealDesc = sid2  ,
                         MealPlanRatesEntityAssociation = new List<DataContracts.MealPlansMealPlanRates>() { new DataContracts.MealPlansMealPlanRates(175, DateTime.Now ) } },
                    new DataContracts.MealPlans() { RecordGuid = guid3, Recordkey = id3, MealDesc = sid3 ,
                        MealPlanRatesEntityAssociation = new List<DataContracts.MealPlansMealPlanRates>() { new DataContracts.MealPlansMealPlanRates(175, DateTime.Now ) } },
                    };


                List<string> mealPlanRateGuids = new List<string>();
                foreach (var mp in _mealPlansCollection)
                {
                    mealPlanRateGuids.Add(mp.RecordGuid);
                };
                dataReaderMock.Setup(repo => repo.SelectAsync("MEAL.PLANS", It.IsAny<string>())).ReturnsAsync(mealPlanRateGuids.ToArray());
                dataReaderMock.Setup(repo => repo.BulkReadRecordAsync<DataContracts.MealPlans>(It.IsAny<string[]>(), It.IsAny<bool>())).ReturnsAsync(_mealPlansCollection);

                referenceDataRepo = BuildValidReferenceDataRepository();

            }

            [TestCleanup]
            public void Cleanup()
            {
                MockCleanup();

                transFactoryMock = null;
                dataAccessorMock = null;
                cacheProviderMock = null;
                _mealPlansCollection = null;
                referenceDataRepo = null;
            }

            [TestMethod]
            public async Task StudentReferenceDataRepo_GetGetMealPlanRatesAsync()
            {
                var result = await referenceDataRepo.GetMealPlanRatesAsync(false);

                for (int i = 0; i < _mealPlansCollection.Count(); i++)
                {
                    Assert.AreEqual(_mealPlansCollection.ElementAt(i).RecordGuid, result.ElementAt(i).Guid);
                    Assert.AreEqual(_mealPlansCollection.ElementAt(i).Recordkey, result.ElementAt(i).Code);
                    Assert.AreEqual(_mealPlansCollection.ElementAt(i).MealDesc, result.ElementAt(i).Description);
                }
            }

            [TestMethod]
            [ExpectedException(typeof(Exception))]
            public async Task StudentReferenceDataRepo_GetsGetMealPlanRates_Exception()
            {
                dataReaderMock.Setup(dr => dr.SelectAsync(It.IsAny<GuidLookup[]>())).ReturnsAsync(null);
                dataReaderMock.Setup(dr => dr.SelectAsync(It.IsAny<RecordKeyLookup[]>())).ReturnsAsync(null);
                dataAccessorMock.Setup(da => da.SelectAsync(It.IsAny<RecordKeyLookup[]>())).ReturnsAsync(null);

                await referenceDataRepo.GetMealPlanRatesAsync(true);
            }

            private StudentReferenceDataRepository BuildValidReferenceDataRepository()
            {
                // Initialize the Mock framework
                MockInitialize();

                transFactoryMock.Setup(transFac => transFac.GetDataReader()).Returns(dataReaderMock.Object);

                // Cache mocking
                var cacheProviderMock = new Mock<ICacheProvider>();
                /*cacheProviderMock.Setup<Task<Tuple<object, SemaphoreSlim>>>(x =>
                x.GetAndLockSemaphoreAsync(It.IsAny<string>(), null))
                .Returns(Task.FromResult(new Tuple<object, SemaphoreSlim>(
                null,
                new SemaphoreSlim(1, 1)
                )));*/

                cacheProviderMock.Setup<Task<Tuple<object, SemaphoreSlim>>>(x =>
             x.GetAndLockSemaphoreAsync(It.IsAny<string>(), null))
             .ReturnsAsync(new Tuple<object, SemaphoreSlim>(null, null));

                referenceDataRepo = new StudentReferenceDataRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object, apiSettings);
                return referenceDataRepo;
            }
        }

        [TestClass]
        public class FacultyContractTypesTests
        {
            Mock<IColleagueTransactionFactory> transFactoryMock;
            Mock<ICacheProvider> cacheProviderMock;
            Mock<IColleagueDataReader> dataAccessorMock;
            Mock<ILogger> loggerMock;
            IEnumerable<FacultyContractTypes> allFacultyContractTypes;
            ApplValcodes facultyContractTypesValcodeResponse;
            string valcodeName;
            ApiSettings apiSettings;

            Mock<IStudentReferenceDataRepository> referenceDataRepositoryMock;
            IStudentReferenceDataRepository referenceDataRepository;
            StudentReferenceDataRepository referenceDataRepo;

            [TestInitialize]
            public void Initialize()
            {
                loggerMock = new Mock<ILogger>();
                apiSettings = new ApiSettings("TEST");

                // Build academic standings responses used for mocking
                allFacultyContractTypes = new TestStudentReferenceDataRepository().GetFacultyContractTypesAsync(false).Result;
                facultyContractTypesValcodeResponse = BuildValcodeResponse(allFacultyContractTypes);
                var facultyContractTypesValResponse = new List<string>() { "2" };
                facultyContractTypesValcodeResponse.ValActionCode1 = facultyContractTypesValResponse;

                referenceDataRepositoryMock = new Mock<IStudentReferenceDataRepository>();
                referenceDataRepository = referenceDataRepositoryMock.Object;

                // Build academic standing repository
                referenceDataRepo = BuildValidReferenceDataRepository();
                valcodeName = referenceDataRepo.BuildFullCacheKey("ST_FACULTY_CONTRACT_TYPES_GUID");

                cacheProviderMock.Setup<Task<Tuple<object, SemaphoreSlim>>>(x =>
                   x.GetAndLockSemaphoreAsync(It.IsAny<string>(), null))
                   .ReturnsAsync(new Tuple<object, SemaphoreSlim>(null, new SemaphoreSlim(1, 1)));
            }

            [TestCleanup]
            public void Cleanup()
            {
                transFactoryMock = null;
                dataAccessorMock = null;
                cacheProviderMock = null;
                facultyContractTypesValcodeResponse = null;
                allFacultyContractTypes = null;
                referenceDataRepo = null;
            }

            [TestMethod]
            public async Task StudentReferenceDataRepo_GetsFacultyContractTypesNoArgAsync()
            {
                var facultyContractTypes = await referenceDataRepo.GetFacultyContractTypesAsync(It.IsAny<bool>());

                for (int i = 0; i < allFacultyContractTypes.Count(); i++)
                {
                    Assert.AreEqual(allFacultyContractTypes.ElementAt(i).Code, facultyContractTypes.ElementAt(i).Code);
                    Assert.AreEqual(allFacultyContractTypes.ElementAt(i).Description, facultyContractTypes.ElementAt(i).Description);
                }
            }

            [TestMethod]
            public async Task StudentReferenceDataRepo_GetsFacultyContractTypesCacheAsync()
            {
                var facultyContractTypes = await referenceDataRepo.GetFacultyContractTypesAsync(false);

                for (int i = 0; i < allFacultyContractTypes.Count(); i++)
                {
                    Assert.AreEqual(allFacultyContractTypes.ElementAt(i).Code, facultyContractTypes.ElementAt(i).Code);
                    Assert.AreEqual(allFacultyContractTypes.ElementAt(i).Description, facultyContractTypes.ElementAt(i).Description);
                }
            }

            [TestMethod]
            public async Task StudentReferenceDataRepo_GetsFacultyContractTypesNonCacheAsync()
            {
                var statuses = await referenceDataRepo.GetFacultyContractTypesAsync(true);

                for (int i = 0; i < allFacultyContractTypes.Count(); i++)
                {
                    Assert.AreEqual(allFacultyContractTypes.ElementAt(i).Code, statuses.ElementAt(i).Code);
                    Assert.AreEqual(allFacultyContractTypes.ElementAt(i).Description, statuses.ElementAt(i).Description);
                }
            }

            [TestMethod]
            public async Task StudentReferenceDataRepo_GetFacultyContractTypes_WritesToCacheAsync()
            {

                // Set up local cache mock to respond to cache request:
                //  -to "Contains" request, return "false" to indicate item is not in cache
                //  -to cache "Get" request, return null so we know it's reading from the "repository"
                cacheProviderMock.Setup(x => x.Contains(valcodeName, null)).Returns(false);
                cacheProviderMock.Setup(x => x.Get(valcodeName, null)).Returns(null);

                // return a valid response to the data accessor request
                dataAccessorMock.Setup(acc => acc.ReadRecordAsync<ApplValcodes>("ST.VALCODES", "FACULTY.CONTRACT.TYPES", It.IsAny<bool>())).ReturnsAsync(facultyContractTypesValcodeResponse);

                cacheProviderMock.Setup<Task<Tuple<object, SemaphoreSlim>>>(x =>
                 x.GetAndLockSemaphoreAsync(It.IsAny<string>(), null))
                 .ReturnsAsync(new Tuple<object, SemaphoreSlim>(null, new SemaphoreSlim(1, 1)));

                // But after data accessor read, set up mocking so we can verify the list of facultyContractTypes was written to the cache
                cacheProviderMock.Setup(x => x.Add(It.IsAny<string>(), It.IsAny<Task<List<FacultyContractTypes>>>(), It.IsAny<CacheItemPolicy>(), null)).Verifiable();

                cacheProviderMock.Setup(x => x.Contains(referenceDataRepo.BuildFullCacheKey("ST_FACULTY_CONTRACT_TYPES"), null)).Returns(true);
                var facultyContractTypes = await referenceDataRepo.GetFacultyContractTypesAsync(false);
                cacheProviderMock.Setup(x => x.Get(referenceDataRepo.BuildFullCacheKey("ST_FACULTY_CONTRACT_TYPES"), null)).Returns(facultyContractTypes);
                // Verify that facultyContractTypes were returned, which means they came from the "repository".
                Assert.IsTrue(facultyContractTypes.Count() == 3);

                // Verify that the facultyContractTypes item was added to the cache after it was read from the repository
                cacheProviderMock.Verify(m => m.Add(It.IsAny<string>(), It.IsAny<Task<List<FacultyContractTypes>>>(), It.IsAny<CacheItemPolicy>(), null), Times.Never);

            }

            private StudentReferenceDataRepository BuildValidReferenceDataRepository()
            {
                // transaction factory mock
                transFactoryMock = new Mock<IColleagueTransactionFactory>();
                // Cache Provider Mock
                cacheProviderMock = new Mock<ICacheProvider>();
                // Set up data accessor for mocking 
                dataAccessorMock = new Mock<IColleagueDataReader>();

                // Set up dataAccessorMock as the object for the DataAccessor
                transFactoryMock.Setup(transFac => transFac.GetDataReader()).Returns(dataAccessorMock.Object);

                // Setup response to facultyContractTypes valcode read
                dataAccessorMock.Setup(acc => acc.ReadRecordAsync<ApplValcodes>("ST.VALCODES", "FACULTY.CONTRACT.TYPES", It.IsAny<bool>())).ReturnsAsync(facultyContractTypesValcodeResponse);
                cacheProviderMock.Setup<Task<Tuple<object, SemaphoreSlim>>>(x => x.GetAndLockSemaphoreAsync(It.IsAny<string>(), null))
                .ReturnsAsync(new Tuple<object, SemaphoreSlim>(null, new SemaphoreSlim(1, 1)));

                dataAccessorMock.Setup(acc => acc.SelectAsync(It.IsAny<RecordKeyLookup[]>())).Returns<RecordKeyLookup[]>(recordKeyLookups =>
                {
                    var result = new Dictionary<string, RecordKeyLookupResult>();
                    foreach (var recordKeyLookup in recordKeyLookups)
                    {
                        var facultyContractTypes = allFacultyContractTypes.Where(e => e.Code == recordKeyLookup.SecondaryKey).FirstOrDefault();
                        result.Add(string.Join("+", new string[] { "ST.VALCODES", "FACULTY.CONTRACT.TYPES", facultyContractTypes.Code }),
                            new RecordKeyLookupResult() { Guid = facultyContractTypes.Guid });
                    }
                    return Task.FromResult(result);
                });

                // Construct repository
                referenceDataRepo = new StudentReferenceDataRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object, apiSettings);

                return referenceDataRepo;
            }

            private ApplValcodes BuildValcodeResponse(IEnumerable<FacultyContractTypes> facultyContractTypes)
            {
                ApplValcodes facultyContractTypesResponse = new ApplValcodes();
                facultyContractTypesResponse.ValsEntityAssociation = new List<ApplValcodesVals>();
                foreach (var item in facultyContractTypes)
                {
                    facultyContractTypesResponse.ValsEntityAssociation.Add(new ApplValcodesVals("", item.Description, "2", item.Code, "3", "", ""));
                }
                return facultyContractTypesResponse;
            }
        }

        [TestClass]
        public class FacultySpecialStatusesTests
        {
            Mock<IColleagueTransactionFactory> transFactoryMock;
            Mock<ICacheProvider> cacheProviderMock;
            Mock<IColleagueDataReader> dataAccessorMock;
            Mock<ILogger> loggerMock;
            IEnumerable<FacultySpecialStatuses> allFacultySpecialStatuses;
            ApplValcodes facultySpecialStatusesValcodeResponse;
            string valcodeName;
            ApiSettings apiSettings;

            Mock<IStudentReferenceDataRepository> referenceDataRepositoryMock;
            IStudentReferenceDataRepository referenceDataRepository;
            StudentReferenceDataRepository referenceDataRepo;

            [TestInitialize]
            public void Initialize()
            {
                loggerMock = new Mock<ILogger>();
                apiSettings = new ApiSettings("TEST");

                // Build academic standings responses used for mocking
                allFacultySpecialStatuses = new TestStudentReferenceDataRepository().GetFacultySpecialStatusesAsync(false).Result;
                facultySpecialStatusesValcodeResponse = BuildValcodeResponse(allFacultySpecialStatuses);
                var facultySpecialStatusesValResponse = new List<string>() { "2" };
                facultySpecialStatusesValcodeResponse.ValActionCode1 = facultySpecialStatusesValResponse;

                referenceDataRepositoryMock = new Mock<IStudentReferenceDataRepository>();
                referenceDataRepository = referenceDataRepositoryMock.Object;

                // Build academic standing repository
                referenceDataRepo = BuildValidReferenceDataRepository();
                valcodeName = referenceDataRepo.BuildFullCacheKey("ST_FACULTY_SPECIAL_STATUSES_GUID");

                cacheProviderMock.Setup<Task<Tuple<object, SemaphoreSlim>>>(x =>
                   x.GetAndLockSemaphoreAsync(It.IsAny<string>(), null))
                   .ReturnsAsync(new Tuple<object, SemaphoreSlim>(null, new SemaphoreSlim(1, 1)));
            }

            [TestCleanup]
            public void Cleanup()
            {
                transFactoryMock = null;
                dataAccessorMock = null;
                cacheProviderMock = null;
                facultySpecialStatusesValcodeResponse = null;
                allFacultySpecialStatuses = null;
                referenceDataRepo = null;
            }

            [TestMethod]
            public async Task StudentReferenceDataRepo_GetsFacultySpecialStatusesNoArgAsync()
            {
                var facultySpecialStatuses = await referenceDataRepo.GetFacultySpecialStatusesAsync(It.IsAny<bool>());

                for (int i = 0; i < allFacultySpecialStatuses.Count(); i++)
                {
                    Assert.AreEqual(allFacultySpecialStatuses.ElementAt(i).Code, facultySpecialStatuses.ElementAt(i).Code);
                    Assert.AreEqual(allFacultySpecialStatuses.ElementAt(i).Description, facultySpecialStatuses.ElementAt(i).Description);
                }
            }

            [TestMethod]
            public async Task StudentReferenceDataRepo_GetsFacultySpecialStatusesCacheAsync()
            {
                var facultySpecialStatuses = await referenceDataRepo.GetFacultySpecialStatusesAsync(false);

                for (int i = 0; i < allFacultySpecialStatuses.Count(); i++)
                {
                    Assert.AreEqual(allFacultySpecialStatuses.ElementAt(i).Code, facultySpecialStatuses.ElementAt(i).Code);
                    Assert.AreEqual(allFacultySpecialStatuses.ElementAt(i).Description, facultySpecialStatuses.ElementAt(i).Description);
                }
            }

            [TestMethod]
            public async Task StudentReferenceDataRepo_GetsFacultySpecialStatusesNonCacheAsync()
            {
                var statuses = await referenceDataRepo.GetFacultySpecialStatusesAsync(true);

                for (int i = 0; i < allFacultySpecialStatuses.Count(); i++)
                {
                    Assert.AreEqual(allFacultySpecialStatuses.ElementAt(i).Code, statuses.ElementAt(i).Code);
                    Assert.AreEqual(allFacultySpecialStatuses.ElementAt(i).Description, statuses.ElementAt(i).Description);
                }
            }

            [TestMethod]
            public async Task StudentReferenceDataRepo_GetFacultySpecialStatuses_WritesToCacheAsync()
            {

                // Set up local cache mock to respond to cache request:
                //  -to "Contains" request, return "false" to indicate item is not in cache
                //  -to cache "Get" request, return null so we know it's reading from the "repository"
                cacheProviderMock.Setup(x => x.Contains(valcodeName, null)).Returns(false);
                cacheProviderMock.Setup(x => x.Get(valcodeName, null)).Returns(null);

                // return a valid response to the data accessor request
                dataAccessorMock.Setup(acc => acc.ReadRecordAsync<ApplValcodes>("ST.VALCODES", "FACULTY.SPECIAL.STATUSES", It.IsAny<bool>())).ReturnsAsync(facultySpecialStatusesValcodeResponse);

                cacheProviderMock.Setup<Task<Tuple<object, SemaphoreSlim>>>(x =>
                 x.GetAndLockSemaphoreAsync(It.IsAny<string>(), null))
                 .ReturnsAsync(new Tuple<object, SemaphoreSlim>(null, new SemaphoreSlim(1, 1)));

                // But after data accessor read, set up mocking so we can verify the list of facultySpecialStatuses was written to the cache
                cacheProviderMock.Setup(x => x.Add(It.IsAny<string>(), It.IsAny<Task<List<FacultySpecialStatuses>>>(), It.IsAny<CacheItemPolicy>(), null)).Verifiable();

                cacheProviderMock.Setup(x => x.Contains(referenceDataRepo.BuildFullCacheKey("ST_FACULTY_SPECIAL_STATUSES"), null)).Returns(true);
                var facultySpecialStatuses = await referenceDataRepo.GetFacultySpecialStatusesAsync(false);
                cacheProviderMock.Setup(x => x.Get(referenceDataRepo.BuildFullCacheKey("ST_FACULTY_SPECIAL_STATUSES"), null)).Returns(facultySpecialStatuses);
                // Verify that facultySpecialStatuses were returned, which means they came from the "repository".
                Assert.IsTrue(facultySpecialStatuses.Count() == 3);

                // Verify that the facultySpecialStatuses item was added to the cache after it was read from the repository
                cacheProviderMock.Verify(m => m.Add(It.IsAny<string>(), It.IsAny<Task<List<FacultySpecialStatuses>>>(), It.IsAny<CacheItemPolicy>(), null), Times.Never);

            }

            private StudentReferenceDataRepository BuildValidReferenceDataRepository()
            {
                // transaction factory mock
                transFactoryMock = new Mock<IColleagueTransactionFactory>();
                // Cache Provider Mock
                cacheProviderMock = new Mock<ICacheProvider>();
                // Set up data accessor for mocking 
                dataAccessorMock = new Mock<IColleagueDataReader>();

                // Set up dataAccessorMock as the object for the DataAccessor
                transFactoryMock.Setup(transFac => transFac.GetDataReader()).Returns(dataAccessorMock.Object);

                // Setup response to facultySpecialStatuses valcode read
                dataAccessorMock.Setup(acc => acc.ReadRecordAsync<ApplValcodes>("ST.VALCODES", "FACULTY.SPECIAL.STATUSES", It.IsAny<bool>())).ReturnsAsync(facultySpecialStatusesValcodeResponse);
                cacheProviderMock.Setup<Task<Tuple<object, SemaphoreSlim>>>(x => x.GetAndLockSemaphoreAsync(It.IsAny<string>(), null))
                .ReturnsAsync(new Tuple<object, SemaphoreSlim>(null, new SemaphoreSlim(1, 1)));

                dataAccessorMock.Setup(acc => acc.SelectAsync(It.IsAny<RecordKeyLookup[]>())).Returns<RecordKeyLookup[]>(recordKeyLookups =>
                {
                    var result = new Dictionary<string, RecordKeyLookupResult>();
                    foreach (var recordKeyLookup in recordKeyLookups)
                    {
                        var facultySpecialStatuses = allFacultySpecialStatuses.Where(e => e.Code == recordKeyLookup.SecondaryKey).FirstOrDefault();
                        result.Add(string.Join("+", new string[] { "ST.VALCODES", "FACULTY.SPECIAL.STATUSES", facultySpecialStatuses.Code }),
                            new RecordKeyLookupResult() { Guid = facultySpecialStatuses.Guid });
                    }
                    return Task.FromResult(result);
                });

                // Construct repository
                referenceDataRepo = new StudentReferenceDataRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object, apiSettings);

                return referenceDataRepo;
            }

            private ApplValcodes BuildValcodeResponse(IEnumerable<FacultySpecialStatuses> facultySpecialStatuses)
            {
                ApplValcodes facultySpecialStatusesResponse = new ApplValcodes();
                facultySpecialStatusesResponse.ValsEntityAssociation = new List<ApplValcodesVals>();
                foreach (var item in facultySpecialStatuses)
                {
                    facultySpecialStatusesResponse.ValsEntityAssociation.Add(new ApplValcodesVals("", item.Description, "2", item.Code, "3", "", ""));
                }
                return facultySpecialStatusesResponse;
            }
        }

        [TestClass]
        public class MealTypesTest
        {
            Mock<IColleagueTransactionFactory> transFactoryMock;
            Mock<ICacheProvider> cacheProviderMock;
            Mock<IColleagueDataReader> dataAccessorMock;
            Mock<ILogger> loggerMock;
            IEnumerable<MealType> allMealType;
            ApplValcodes mealTypesdomainEntityNameResponse;
            string domainEntityNameName;
            ApiSettings apiSettings;

            Mock<IStudentReferenceDataRepository> referenceDataRepositoryMock;
            IStudentReferenceDataRepository referenceDataRepository;
            StudentReferenceDataRepository referenceDataRepo;

            [TestInitialize]
            public void Initialize()
            {
                loggerMock = new Mock<ILogger>();
                apiSettings = new ApiSettings("TEST");

                // Build academic standings responses used for mocking
                allMealType = new TestStudentReferenceDataRepository().GetMealTypesAsync(false).Result;
                mealTypesdomainEntityNameResponse = BuilddomainEntityNameResponse(allMealType);
                var mealTypesValResponse = new List<string>() { "2" };
                mealTypesdomainEntityNameResponse.ValActionCode1 = mealTypesValResponse;

                referenceDataRepositoryMock = new Mock<IStudentReferenceDataRepository>();
                referenceDataRepository = referenceDataRepositoryMock.Object;

                // Build academic standing repository
                referenceDataRepo = BuildValidReferenceDataRepository();
                domainEntityNameName = referenceDataRepo.BuildFullCacheKey("ST_MEAL.TYPE_GUID");

                cacheProviderMock.Setup<Task<Tuple<object, SemaphoreSlim>>>(x =>
                   x.GetAndLockSemaphoreAsync(It.IsAny<string>(), null))
                   .ReturnsAsync(new Tuple<object, SemaphoreSlim>(null, new SemaphoreSlim(1, 1)));
            }

            [TestCleanup]
            public void Cleanup()
            {
                transFactoryMock = null;
                dataAccessorMock = null;
                cacheProviderMock = null;
                mealTypesdomainEntityNameResponse = null;
                allMealType = null;
                referenceDataRepo = null;
            }

            [TestMethod]
            public async Task StudentReferenceDataRepo_GetsMealTypeNoArgAsync()
            {
                var mealTypes = await referenceDataRepo.GetMealTypesAsync();

                for (int i = 0; i < allMealType.Count(); i++)
                {
                    Assert.AreEqual(allMealType.ElementAt(i).Code, mealTypes.ElementAt(i).Code);
                    Assert.AreEqual(allMealType.ElementAt(i).Description, mealTypes.ElementAt(i).Description);
                }
            }

            [TestMethod]
            public async Task StudentReferenceDataRepo_GetsMealTypeCacheAsync()
            {
                var mealTypes = await referenceDataRepo.GetMealTypesAsync(false);

                for (int i = 0; i < allMealType.Count(); i++)
                {
                    Assert.AreEqual(allMealType.ElementAt(i).Code, mealTypes.ElementAt(i).Code);
                    Assert.AreEqual(allMealType.ElementAt(i).Description, mealTypes.ElementAt(i).Description);
                }
            }

            [TestMethod]
            public async Task StudentReferenceDataRepo_GetsMealTypeNonCacheAsync()
            {
                var statuses = await referenceDataRepo.GetMealTypesAsync(true);

                for (int i = 0; i < allMealType.Count(); i++)
                {
                    Assert.AreEqual(allMealType.ElementAt(i).Code, statuses.ElementAt(i).Code);
                    Assert.AreEqual(allMealType.ElementAt(i).Description, statuses.ElementAt(i).Description);
                }
            }

            [TestMethod]
            public async Task StudentReferenceDataRepo_GetMealType_WritesToCacheAsync()
            {

                // Set up local cache mock to respond to cache request:
                //  -to "Contains" request, return "false" to indicate item is not in cache
                //  -to cache "Get" request, return null so we know it's reading from the "repository"
                cacheProviderMock.Setup(x => x.Contains(domainEntityNameName, null)).Returns(false);
                cacheProviderMock.Setup(x => x.Get(domainEntityNameName, null)).Returns(null);

                // return a valid response to the data accessor request
                dataAccessorMock.Setup(acc => acc.ReadRecordAsync<ApplValcodes>("ST.VALCODES", "MEAL.TYPE", It.IsAny<bool>())).ReturnsAsync(mealTypesdomainEntityNameResponse);

                cacheProviderMock.Setup<Task<Tuple<object, SemaphoreSlim>>>(x =>
                 x.GetAndLockSemaphoreAsync(It.IsAny<string>(), null))
                 .ReturnsAsync(new Tuple<object, SemaphoreSlim>(null, new SemaphoreSlim(1, 1)));

                // But after data accessor read, set up mocking so we can verify the list of mealTypes was written to the cache
                cacheProviderMock.Setup(x => x.Add(It.IsAny<string>(), It.IsAny<Task<List<MealType>>>(), It.IsAny<CacheItemPolicy>(), null)).Verifiable();

                cacheProviderMock.Setup(x => x.Contains(referenceDataRepo.BuildFullCacheKey("ST_MEAL.TYPE"), null)).Returns(true);
                var mealTypes = await referenceDataRepo.GetMealTypesAsync(false);
                cacheProviderMock.Setup(x => x.Get(referenceDataRepo.BuildFullCacheKey("ST_MEAL.TYPE"), null)).Returns(mealTypes);
                // Verify that mealTypes were returned, which means they came from the "repository".
                Assert.IsTrue(mealTypes.Count() == 3);

                // Verify that the mealTypes item was added to the cache after it was read from the repository
                cacheProviderMock.Verify(m => m.Add(It.IsAny<string>(), It.IsAny<Task<List<MealType>>>(), It.IsAny<CacheItemPolicy>(), null), Times.Never);

            }

            [TestMethod]
            public async Task StudentReferenceDataRepo_GetMealType_GetsCachedMealTypeAsync()
            {
                // Set up local cache mock to respond to cache request:
                //  -to "Contains" request, return "true" to indicate item is in cache
                //  -to "Get" request, return the cache item (in this case the "MEAL.TYPE" cache item)
                cacheProviderMock.Setup(x => x.Contains(domainEntityNameName, null)).Returns(true);
                cacheProviderMock.Setup(x => x.Get(domainEntityNameName, null)).Returns(allMealType).Verifiable();

                // return null for request, so that if we have a result, it wasn't the data accessor that returned it.
                dataAccessorMock.Setup(acc => acc.ReadRecordAsync<ApplValcodes>("ST.VALCODES", "MEAL.TYPE", true)).ReturnsAsync(new ApplValcodes());

                // Assert the mealTypes are returned
                Assert.IsTrue((await referenceDataRepo.GetMealTypesAsync(false)).Count() == 3);
                // Verify that the smealTypes were retrieved from cache
                cacheProviderMock.Verify(m => m.Get(domainEntityNameName, null));
            }

            private StudentReferenceDataRepository BuildValidReferenceDataRepository()
            {
                // transaction factory mock
                transFactoryMock = new Mock<IColleagueTransactionFactory>();
                // Cache Provider Mock
                cacheProviderMock = new Mock<ICacheProvider>();
                // Set up data accessor for mocking 
                dataAccessorMock = new Mock<IColleagueDataReader>();

                // Set up dataAccessorMock as the object for the DataAccessor
                transFactoryMock.Setup(transFac => transFac.GetDataReader()).Returns(dataAccessorMock.Object);

                // Setup response to mealTypes domainEntityName read
                dataAccessorMock.Setup(acc => acc.ReadRecordAsync<ApplValcodes>("ST.VALCODES", "MEAL.TYPE", It.IsAny<bool>())).ReturnsAsync(mealTypesdomainEntityNameResponse);
                cacheProviderMock.Setup<Task<Tuple<object, SemaphoreSlim>>>(x => x.GetAndLockSemaphoreAsync(It.IsAny<string>(), null))
                .ReturnsAsync(new Tuple<object, SemaphoreSlim>(null, new SemaphoreSlim(1, 1)));

                dataAccessorMock.Setup(acc => acc.SelectAsync(It.IsAny<RecordKeyLookup[]>())).Returns<RecordKeyLookup[]>(recordKeyLookups =>
                {
                    var result = new Dictionary<string, RecordKeyLookupResult>();
                    foreach (var recordKeyLookup in recordKeyLookups)
                    {
                        var mealTypes = allMealType.Where(e => e.Code == recordKeyLookup.SecondaryKey).FirstOrDefault();
                        result.Add(string.Join("+", new string[] { "ST.VALCODES", "MEAL.TYPE", mealTypes.Code }),
                            new RecordKeyLookupResult() { Guid = mealTypes.Guid });
                    }
                    return Task.FromResult(result);
                });

                // Construct repository
                referenceDataRepo = new StudentReferenceDataRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object, apiSettings);

                return referenceDataRepo;
            }

            private ApplValcodes BuilddomainEntityNameResponse(IEnumerable<MealType> mealTypes)
            {
                ApplValcodes mealTypesResponse = new ApplValcodes();
                mealTypesResponse.ValsEntityAssociation = new List<ApplValcodesVals>();
                foreach (var item in mealTypes)
                {
                    mealTypesResponse.ValsEntityAssociation.Add(new ApplValcodesVals("", item.Description, "2", item.Code, "3", "", ""));
                }
                return mealTypesResponse;
            }
        }

        [TestClass]
        public class MealPlansTest
        {
            Mock<IColleagueTransactionFactory> transFactoryMock;
            Mock<ICacheProvider> cacheProviderMock;
            Mock<IColleagueDataReader> dataAccessorMock;
            Mock<ILogger> loggerMock;

            IEnumerable<MealPlan> allMealPlans;
            string valcodeName;
            ApiSettings apiSettings;

            StudentReferenceDataRepository referenceDataRepo;

            [TestInitialize]
            public void Initialize()
            {
                loggerMock = new Mock<ILogger>();
                apiSettings = new ApiSettings("TEST");

                allMealPlans = new TestStudentReferenceDataRepository().GetMealPlansAsync(false).Result;

                referenceDataRepo = BuildValidReferenceDataRepository();
                valcodeName = referenceDataRepo.BuildFullCacheKey("AllMealPlans");
            }

            [TestCleanup]
            public void Cleanup()
            {
                allMealPlans = null;
                valcodeName = string.Empty;
                apiSettings = null;
            }

            [TestMethod]
            public async Task StudentReferenceDataRepository_GetMealPlansAsync_False()
            {
                var results = await referenceDataRepo.GetMealPlansAsync(false);
                Assert.AreEqual(allMealPlans.Count(), results.Count());

                foreach (var mealPlan in allMealPlans)
                {
                    var result = results.FirstOrDefault(i => i.Guid == mealPlan.Guid);

                    Assert.AreEqual(mealPlan.Code, result.Code);
                    Assert.AreEqual(mealPlan.Description, result.Description);
                    Assert.AreEqual(mealPlan.Guid, result.Guid);
                }

            }       

            private StudentReferenceDataRepository BuildValidReferenceDataRepository()
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

                var records = new Collection<DataContracts.MealPlans>();
                foreach (var item in allMealPlans)
                {
                    DataContracts.MealPlans record = new DataContracts.MealPlans();
                    record.RecordGuid = item.Guid;
                    record.MealDesc = item.Description;
                    record.Recordkey = item.Code;
                    records.Add(record);
                }
                dataAccessorMock.Setup(acc => acc.BulkReadRecordAsync<DataContracts.MealPlans>("MEAL.PLANS",  It.IsAny<GuidLookup[]>(), It.IsAny<bool>())).ReturnsAsync(records);
                dataAccessorMock.Setup(acc => acc.BulkReadRecordAsync<DataContracts.MealPlans>(It.IsAny<string[]>(), It.IsAny<bool>())).ReturnsAsync(records);

                cacheProviderMock.Setup<Task<Tuple<object, SemaphoreSlim>>>(x =>
                 x.GetAndLockSemaphoreAsync(It.IsAny<string>(), null))
                 .ReturnsAsync(new Tuple<object, SemaphoreSlim>(null, new SemaphoreSlim(1, 1)));

                dataAccessorMock.Setup(acc => acc.SelectAsync("LDM.GUID", It.IsAny<string>())).ReturnsAsync(new string[] { "bb66b971-3ee0-4477-9bb7-539721f93434", "5aeebc5c-c973-4f83-be4b-f64c95002124", "27178aab-a6e8-4d1e-ae27-eca1f7b33363" });

                dataAccessorMock.Setup(acc => acc.SelectAsync(It.IsAny<RecordKeyLookup[]>())).Returns<RecordKeyLookup[]>(recordKeyLookups =>
                {
                    var result = new Dictionary<string, RecordKeyLookupResult>();
                    int index = 0;
                    foreach (var recordKeyLookup in recordKeyLookups)
                    {
                        var record = allMealPlans.Where(e => e.Code == recordKeyLookup.PrimaryKey).FirstOrDefault();
                        //result.Add(string.Join("+", new string[] { "MEAL.PLANS", record.Code }),
                        result.Add(record.Guid,
                            new RecordKeyLookupResult() { Guid = record.Guid });
                    }
                    return Task.FromResult(result);
                });

                dataAccessorMock.Setup(acc => acc.SelectAsync(It.IsAny<GuidLookup[]>())).Returns<GuidLookup[]>(recordKeyLookups =>
                {
                    var result = new Dictionary<string, GuidLookupResult>();
                    foreach (var recordKeyLookup in recordKeyLookups)
                    {
                        var record = allMealPlans.Where(e => e.Guid == recordKeyLookup.Guid).FirstOrDefault();
                        //result.Add(string.Join("+", new string[] { "MEAL.PLANS", record.Code }),
                        result.Add(record.Guid,
                            new GuidLookupResult() { PrimaryKey = record.Code });
                    }
                    return Task.FromResult(result);
                });

                // Construct repository
                referenceDataRepo = new StudentReferenceDataRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object, apiSettings);

                return referenceDataRepo;
            }
        }

        [TestClass]
        public class GetNonAcademicAttendanceEventTypesAsync
        {
            Mock<IColleagueTransactionFactory> transFactoryMock;
            //Mock<ObjectCache> localCacheMock;
            Mock<ICacheProvider> cacheProviderMock;
            Mock<IColleagueDataReader> dataAccessorMock;
            Mock<ILogger> loggerMock;
            IEnumerable<Ellucian.Colleague.Domain.Student.Entities.NonAcademicAttendanceEventType> allNonAcademicAttendanceEventTypes;
            ApplValcodes nonAcademicAttendanceEventTypesResponse;
            string valcodeName;
            ApiSettings apiSettings;
            StudentReferenceDataRepository referenceDataRepo;

            [TestInitialize]
            public void Initialize()
            {
                loggerMock = new Mock<ILogger>();

                // Build responses used for mocking
                allNonAcademicAttendanceEventTypes = new TestStudentReferenceDataRepository().GetNonAcademicAttendanceEventTypesAsync(false).Result;

                nonAcademicAttendanceEventTypesResponse = BuildValcodeResponse(allNonAcademicAttendanceEventTypes);

                // Build repository
                referenceDataRepo = BuildValidReferenceDataRepository();
                valcodeName = referenceDataRepo.BuildFullCacheKey("ST_NAA.EVENT.TYPES");

                cacheProviderMock.Setup<Task<Tuple<object, SemaphoreSlim>>>(x =>
                   x.GetAndLockSemaphoreAsync(It.IsAny<string>(), null))
                   .ReturnsAsync(new Tuple<object, SemaphoreSlim>(null, new SemaphoreSlim(1, 1)));

            }

            [TestCleanup]
            public void Cleanup()
            {
                transFactoryMock = null;
                dataAccessorMock = null;
                cacheProviderMock = null;
                nonAcademicAttendanceEventTypesResponse = null;
                allNonAcademicAttendanceEventTypes = null;
                referenceDataRepo = null;
            }

            [TestMethod]
            public async Task StudentReferenceDataRepo_GetNonAcademicAttendanceEventTypesAsync()
            {
                var repo = await referenceDataRepo.GetNonAcademicAttendanceEventTypesAsync(); ;

                for (int i = 0; i < allNonAcademicAttendanceEventTypes.Count(); i++)
                {
                    Assert.AreEqual(allNonAcademicAttendanceEventTypes.ElementAt(i).Code, repo.ElementAt(i).Code);
                    Assert.AreEqual(allNonAcademicAttendanceEventTypes.ElementAt(i).Description, repo.ElementAt(i).Description);
                }
            }

            [TestMethod]
            public async Task StudentReferenceDataRepo_GetNonAcademicAttendanceEventTypesAsync_Exceptions_excluded()
            {
                // Setup response to student loads valcode read
                nonAcademicAttendanceEventTypesResponse.ValsEntityAssociation.Add(new ApplValcodesVals("", null, "", null, "", "", ""));
                dataAccessorMock.Setup(acc => acc.ReadRecordAsync<ApplValcodes>("ST.VALCODES", "NAA.EVENT.TYPES", true)).ReturnsAsync(nonAcademicAttendanceEventTypesResponse);

                // Construct repository
                referenceDataRepo = new StudentReferenceDataRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object, apiSettings);

                var repo = await referenceDataRepo.GetNonAcademicAttendanceEventTypesAsync(); ;
                Assert.AreEqual(nonAcademicAttendanceEventTypesResponse.ValsEntityAssociation.Count - 1, repo.Count());
                for (int i = 0; i < allNonAcademicAttendanceEventTypes.Count() - 1; i++)
                {
                    Assert.AreEqual(allNonAcademicAttendanceEventTypes.ElementAt(i).Code, repo.ElementAt(i).Code);
                    Assert.AreEqual(allNonAcademicAttendanceEventTypes.ElementAt(i).Description, repo.ElementAt(i).Description);
                }
            }

            [TestMethod]
            public async Task StudentReferenceDataRepo_GetNonAcademicAttendanceEventTypesAsync_WritesToCache()
            {
                // Set up local cache mock to respond to cache request:
                //  -to "Contains" request, return "false" to indicate item is not in cache
                //  -to cache "Get" request, return null so we know it's reading from the "repository"
                cacheProviderMock.Setup(x => x.Contains(valcodeName, null)).Returns(false);
                cacheProviderMock.Setup(x => x.Get(valcodeName, null)).Returns(null);

                // return a valid response to the data accessor request
                dataAccessorMock.Setup(acc => acc.ReadRecordAsync<ApplValcodes>(It.IsAny<string>(), It.IsAny<string>(), true)).ReturnsAsync(nonAcademicAttendanceEventTypesResponse);

                // But after data accessor read, set up mocking so we can verify the list of student loads was written to the cache
                cacheProviderMock.Setup(x => x.AddAndUnlockSemaphore(valcodeName, It.IsAny<Object>(), It.IsAny<SemaphoreSlim>(), It.IsAny<CacheItemPolicy>(), null)).Verifiable();

                // Verify that student loads were returned, which means they came from the "repository".
                Assert.IsTrue((await referenceDataRepo.GetNonAcademicAttendanceEventTypesAsync()).Count() == allNonAcademicAttendanceEventTypes.Count());

                // Verify that the student loads was added to the cache after it was read from the repository
                cacheProviderMock.Verify(m => m.AddAndUnlockSemaphore(valcodeName, It.IsAny<Object>(), It.IsAny<SemaphoreSlim>(), It.IsAny<CacheItemPolicy>(), null));
            }

            private T GetCache<T>(ICacheProvider cacheProvider, string key)
                where T : class
            {
                object cache = cacheProvider.Get(key, null);
                return cache as T;
            }

            private StudentReferenceDataRepository BuildValidReferenceDataRepository()
            {
                // transaction factory mock
                transFactoryMock = new Mock<IColleagueTransactionFactory>();
                // Cache Mock
                //localCacheMock = new Mock<ObjectCache>();
                // Cache Provider Mock
                cacheProviderMock = new Mock<ICacheProvider>();
                // Set up data accessor for mocking 
                dataAccessorMock = new Mock<IColleagueDataReader>();
                apiSettings = new ApiSettings("TEST");

                // Set up dataAccessorMock as the object for the DataAccessor
                transFactoryMock.Setup(transFac => transFac.GetDataReader()).Returns(dataAccessorMock.Object);

                // Setup response to student loads valcode read
                dataAccessorMock.Setup(acc => acc.ReadRecordAsync<ApplValcodes>("ST.VALCODES", "NAA.EVENT.TYPES", true)).ReturnsAsync(nonAcademicAttendanceEventTypesResponse);

                // Construct repository
                referenceDataRepo = new StudentReferenceDataRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object, apiSettings);

                return referenceDataRepo;
            }

            private ApplValcodes BuildValcodeResponse(IEnumerable<Ellucian.Colleague.Domain.Student.Entities.NonAcademicAttendanceEventType> nonAcademicAttendanceTypes)
            {
                ApplValcodes applValcodeResponse = new ApplValcodes();
                applValcodeResponse.ValsEntityAssociation = new List<ApplValcodesVals>();
                foreach (var item in nonAcademicAttendanceTypes)
                {
                    applValcodeResponse.ValsEntityAssociation.Add(new ApplValcodesVals("", item.Description, "", item.Code, "", "", ""));
                }
                return applValcodeResponse;
            }
        }


        /// <summary>
        /// Test class for NonCourseCategories codes
        /// </summary>
        [TestClass]
        public class NonCourseCategoriesTests
        {
            Mock<IColleagueTransactionFactory> transFactoryMock;
            Mock<ICacheProvider> cacheProviderMock;
            Mock<IColleagueDataReader> dataAccessorMock;
            Mock<ILogger> loggerMock;
            IEnumerable<Domain.Student.Entities.NonCourseCategories> allNonCourseCategories;
            ApplValcodes nonCourseCategoriesValcodeResponse;
            string valcodeName;
            ApiSettings apiSettings;

            Mock<IStudentReferenceDataRepository> referenceDataRepositoryMock;
            IStudentReferenceDataRepository referenceDataRepository;
            StudentReferenceDataRepository referenceDataRepo;

            [TestInitialize]
            public void Initialize()
            {
                loggerMock = new Mock<ILogger>();
                apiSettings = new ApiSettings("TEST");

                // Build academic standings responses used for mocking
                allNonCourseCategories = new TestStudentReferenceDataRepository().GetNonCourseCategoriesAsync(false).Result;
                nonCourseCategoriesValcodeResponse = BuildValcodeResponse(allNonCourseCategories);
                var nonCourseCategoriesValResponse = new List<string>() { "2" };
                nonCourseCategoriesValcodeResponse.ValActionCode1 = nonCourseCategoriesValResponse;

                referenceDataRepositoryMock = new Mock<IStudentReferenceDataRepository>();
                referenceDataRepository = referenceDataRepositoryMock.Object;

                // Build academic standing repository
                referenceDataRepo = BuildValidReferenceDataRepository();
                valcodeName = referenceDataRepo.BuildFullCacheKey("ST_NON.COURSE.CATEGORIES_GUID");

                cacheProviderMock.Setup<Task<Tuple<object, SemaphoreSlim>>>(x =>
                   x.GetAndLockSemaphoreAsync(It.IsAny<string>(), null))
                   .ReturnsAsync(new Tuple<object, SemaphoreSlim>(null, new SemaphoreSlim(1, 1)));
            }

            [TestCleanup]
            public void Cleanup()
            {
                transFactoryMock = null;
                dataAccessorMock = null;
                cacheProviderMock = null;
                nonCourseCategoriesValcodeResponse = null;
                allNonCourseCategories = null;
                referenceDataRepo = null;
            }

            [TestMethod]
            public async Task StudentReferenceDataRepo_GetsNonCourseCategoriesNoArgAsync()
            {
                var nonCourseCategories = await referenceDataRepo.GetNonCourseCategoriesAsync(true);

                for (int i = 0; i < allNonCourseCategories.Count(); i++)
                {
                    Assert.AreEqual(allNonCourseCategories.ElementAt(i).Code, nonCourseCategories.ElementAt(i).Code);
                    Assert.AreEqual(allNonCourseCategories.ElementAt(i).Description, nonCourseCategories.ElementAt(i).Description);
                }
            }

            [TestMethod]
            public async Task StudentReferenceDataRepo_GetsNonCourseCategoriesCacheAsync()
            {
                var nonCourseCategories = await referenceDataRepo.GetNonCourseCategoriesAsync(false);

                for (int i = 0; i < allNonCourseCategories.Count(); i++)
                {
                    Assert.AreEqual(allNonCourseCategories.ElementAt(i).Code, nonCourseCategories.ElementAt(i).Code);
                    Assert.AreEqual(allNonCourseCategories.ElementAt(i).Description, nonCourseCategories.ElementAt(i).Description);
                }
            }

            [TestMethod]
            public async Task StudentReferenceDataRepo_GetsNonCourseCategoriesNonCacheAsync()
            {
                var statuses = await referenceDataRepo.GetNonCourseCategoriesAsync(true);

                for (int i = 0; i < allNonCourseCategories.Count(); i++)
                {
                    Assert.AreEqual(allNonCourseCategories.ElementAt(i).Code, statuses.ElementAt(i).Code);
                    Assert.AreEqual(allNonCourseCategories.ElementAt(i).Description, statuses.ElementAt(i).Description);
                }
            }

            [TestMethod]
            public async Task StudentReferenceDataRepo_GetNonCourseCategories_WritesToCacheAsync()
            {

                // Set up local cache mock to respond to cache request:
                //  -to "Contains" request, return "false" to indicate item is not in cache
                //  -to cache "Get" request, return null so we know it's reading from the "repository"
                cacheProviderMock.Setup(x => x.Contains(valcodeName, null)).Returns(false);
                cacheProviderMock.Setup(x => x.Get(valcodeName, null)).Returns(null);

                // return a valid response to the data accessor request
                dataAccessorMock.Setup(acc => acc.ReadRecordAsync<ApplValcodes>("ST.VALCODES", "NON.COURSE.CATEGORIES", It.IsAny<bool>())).ReturnsAsync(nonCourseCategoriesValcodeResponse);

                cacheProviderMock.Setup<Task<Tuple<object, SemaphoreSlim>>>(x =>
                 x.GetAndLockSemaphoreAsync(It.IsAny<string>(), null))
                 .ReturnsAsync(new Tuple<object, SemaphoreSlim>(null, new SemaphoreSlim(1, 1)));

                // But after data accessor read, set up mocking so we can verify the list of nonCourseCategories was written to the cache
                cacheProviderMock.Setup(x => x.Add(It.IsAny<string>(), It.IsAny<Task<List<NonCourseCategories>>>(), It.IsAny<CacheItemPolicy>(), null)).Verifiable();

                cacheProviderMock.Setup(x => x.Contains(referenceDataRepo.BuildFullCacheKey("ST_NON.COURSE.CATEGORIES"), null)).Returns(true);
                var nonCourseCategories = await referenceDataRepo.GetNonCourseCategoriesAsync(false);
                cacheProviderMock.Setup(x => x.Get(referenceDataRepo.BuildFullCacheKey("ST_NON.COURSE.CATEGORIES"), null)).Returns(nonCourseCategories);
                // Verify that nonCourseCategories were returned, which means they came from the "repository".
                Assert.IsTrue(nonCourseCategories.Count() == 3);

                // Verify that the nonCourseCategories item was added to the cache after it was read from the repository
                cacheProviderMock.Verify(m => m.Add(It.IsAny<string>(), It.IsAny<Task<List<NonCourseCategories>>>(), It.IsAny<CacheItemPolicy>(), null), Times.Never);

            }

            [TestMethod]
            public async Task StudentReferenceDataRepo_GetNonCourseCategories_GetsCachedNonCourseCategoriesAsync()
            {
                // Set up local cache mock to respond to cache request:
                //  -to "Contains" request, return "true" to indicate item is in cache
                //  -to "Get" request, return the cache item (in this case the "NON.COURSE.CATEGORIES" cache item)
                cacheProviderMock.Setup(x => x.Contains(valcodeName, null)).Returns(true);
                cacheProviderMock.Setup(x => x.Get(valcodeName, null)).Returns(allNonCourseCategories).Verifiable();

                // return null for request, so that if we have a result, it wasn't the data accessor that returned it.
                dataAccessorMock.Setup(acc => acc.ReadRecordAsync<ApplValcodes>("ST.VALCODES", "NON.COURSE.CATEGORIES", true)).ReturnsAsync(new ApplValcodes());

                // Assert the nonCourseCategories are returned
                Assert.IsTrue((await referenceDataRepo.GetNonCourseCategoriesAsync(false)).Count() == 3);
                // Verify that the snonCourseCategories were retrieved from cache
                cacheProviderMock.Verify(m => m.Get(valcodeName, null));
            }

            private StudentReferenceDataRepository BuildValidReferenceDataRepository()
            {
                // transaction factory mock
                transFactoryMock = new Mock<IColleagueTransactionFactory>();
                // Cache Provider Mock
                cacheProviderMock = new Mock<ICacheProvider>();
                // Set up data accessor for mocking 
                dataAccessorMock = new Mock<IColleagueDataReader>();

                // Set up dataAccessorMock as the object for the DataAccessor
                transFactoryMock.Setup(transFac => transFac.GetDataReader()).Returns(dataAccessorMock.Object);

                // Setup response to nonCourseCategories valcode read
                dataAccessorMock.Setup(acc => acc.ReadRecordAsync<ApplValcodes>("ST.VALCODES", "NON.COURSE.CATEGORIES", It.IsAny<bool>())).ReturnsAsync(nonCourseCategoriesValcodeResponse);
                cacheProviderMock.Setup<Task<Tuple<object, SemaphoreSlim>>>(x => x.GetAndLockSemaphoreAsync(It.IsAny<string>(), null))
                .ReturnsAsync(new Tuple<object, SemaphoreSlim>(null, new SemaphoreSlim(1, 1)));

                dataAccessorMock.Setup(acc => acc.SelectAsync(It.IsAny<RecordKeyLookup[]>())).Returns<RecordKeyLookup[]>(recordKeyLookups =>
                {
                    var result = new Dictionary<string, RecordKeyLookupResult>();
                    foreach (var recordKeyLookup in recordKeyLookups)
                    {
                        var nonCourseCategories = allNonCourseCategories.Where(e => e.Code == recordKeyLookup.SecondaryKey).FirstOrDefault();
                        result.Add(string.Join("+", new string[] { "ST.VALCODES", "NON.COURSE.CATEGORIES", nonCourseCategories.Code }),
                            new RecordKeyLookupResult() { Guid = nonCourseCategories.Guid });
                    }
                    return Task.FromResult(result);
                });

                // Construct repository
                referenceDataRepo = new StudentReferenceDataRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object, apiSettings);

                return referenceDataRepo;
            }

            private ApplValcodes BuildValcodeResponse(IEnumerable<NonCourseCategories> nonCourseCategories)
            {
                ApplValcodes nonCourseCategoriesResponse = new ApplValcodes();
                nonCourseCategoriesResponse.ValsEntityAssociation = new List<ApplValcodesVals>();
                foreach (var item in nonCourseCategories)
                {
                    nonCourseCategoriesResponse.ValsEntityAssociation.Add(new ApplValcodesVals("", item.Description, "2", item.Code, "3", "", ""));
                }
                return nonCourseCategoriesResponse;
            }
        }

        /// <summary>
        /// Test class for NonCourseGradeUses codes
        /// </summary>
        [TestClass]
        public class NonCourseGradeUsesTests
        {
            Mock<IColleagueTransactionFactory> transFactoryMock;
            Mock<ICacheProvider> cacheProviderMock;
            Mock<IColleagueDataReader> dataAccessorMock;
            Mock<ILogger> loggerMock;
            IEnumerable<NonCourseGradeUses> allNonCourseGradeUses;
            ApplValcodes nonCourseGradeUsesValcodeResponse;
            string valcodeName;
            ApiSettings apiSettings;

            Mock<IStudentReferenceDataRepository> referenceDataRepositoryMock;
            IStudentReferenceDataRepository referenceDataRepository;
            StudentReferenceDataRepository referenceDataRepo;

            [TestInitialize]
            public void Initialize()
            {
                loggerMock = new Mock<ILogger>();
                apiSettings = new ApiSettings("TEST");

                // Build academic standings responses used for mocking
                allNonCourseGradeUses = new TestStudentReferenceDataRepository().GetNonCourseGradeUsesAsync(false).Result;
                nonCourseGradeUsesValcodeResponse = BuildValcodeResponse(allNonCourseGradeUses);
                var nonCourseGradeUsesValResponse = new List<string>() { "2" };
                nonCourseGradeUsesValcodeResponse.ValActionCode1 = nonCourseGradeUsesValResponse;

                referenceDataRepositoryMock = new Mock<IStudentReferenceDataRepository>();
                referenceDataRepository = referenceDataRepositoryMock.Object;

                // Build academic standing repository
                referenceDataRepo = BuildValidReferenceDataRepository();
                valcodeName = referenceDataRepo.BuildFullCacheKey("ST_NON.COURSE.GRADE.USES_GUID");

                cacheProviderMock.Setup<Task<Tuple<object, SemaphoreSlim>>>(x =>
                   x.GetAndLockSemaphoreAsync(It.IsAny<string>(), null))
                   .ReturnsAsync(new Tuple<object, SemaphoreSlim>(null, new SemaphoreSlim(1, 1)));
            }

            [TestCleanup]
            public void Cleanup()
            {
                transFactoryMock = null;
                dataAccessorMock = null;
                cacheProviderMock = null;
                nonCourseGradeUsesValcodeResponse = null;
                allNonCourseGradeUses = null;
                referenceDataRepo = null;
            }

            [TestMethod]
            public async Task StudentReferenceDataRepo_GetsNonCourseGradeUsesNoArgAsync()
            {
                var nonCourseGradeUses = await referenceDataRepo.GetNonCourseGradeUsesAsync(true);

                for (int i = 0; i < allNonCourseGradeUses.Count(); i++)
                {
                    Assert.AreEqual(allNonCourseGradeUses.ElementAt(i).Code, nonCourseGradeUses.ElementAt(i).Code);
                    Assert.AreEqual(allNonCourseGradeUses.ElementAt(i).Description, nonCourseGradeUses.ElementAt(i).Description);
                }
            }

            [TestMethod]
            public async Task StudentReferenceDataRepo_GetsNonCourseGradeUsesCacheAsync()
            {
                var nonCourseGradeUses = await referenceDataRepo.GetNonCourseGradeUsesAsync(false);

                for (int i = 0; i < allNonCourseGradeUses.Count(); i++)
                {
                    Assert.AreEqual(allNonCourseGradeUses.ElementAt(i).Code, nonCourseGradeUses.ElementAt(i).Code);
                    Assert.AreEqual(allNonCourseGradeUses.ElementAt(i).Description, nonCourseGradeUses.ElementAt(i).Description);
                }
            }

            [TestMethod]
            public async Task StudentReferenceDataRepo_GetsNonCourseGradeUsesNonCacheAsync()
            {
                var statuses = await referenceDataRepo.GetNonCourseGradeUsesAsync(true);

                for (int i = 0; i < allNonCourseGradeUses.Count(); i++)
                {
                    Assert.AreEqual(allNonCourseGradeUses.ElementAt(i).Code, statuses.ElementAt(i).Code);
                    Assert.AreEqual(allNonCourseGradeUses.ElementAt(i).Description, statuses.ElementAt(i).Description);
                }
            }

            [TestMethod]
            public async Task StudentReferenceDataRepo_GetNonCourseGradeUses_WritesToCacheAsync()
            {

                // Set up local cache mock to respond to cache request:
                //  -to "Contains" request, return "false" to indicate item is not in cache
                //  -to cache "Get" request, return null so we know it's reading from the "repository"
                cacheProviderMock.Setup(x => x.Contains(valcodeName, null)).Returns(false);
                cacheProviderMock.Setup(x => x.Get(valcodeName, null)).Returns(null);

                // return a valid response to the data accessor request
                dataAccessorMock.Setup(acc => acc.ReadRecordAsync<ApplValcodes>("ST.VALCODES", "NON.COURSE.GRADE.USES", It.IsAny<bool>())).ReturnsAsync(nonCourseGradeUsesValcodeResponse);

                cacheProviderMock.Setup<Task<Tuple<object, SemaphoreSlim>>>(x =>
                 x.GetAndLockSemaphoreAsync(It.IsAny<string>(), null))
                 .ReturnsAsync(new Tuple<object, SemaphoreSlim>(null, new SemaphoreSlim(1, 1)));

                // But after data accessor read, set up mocking so we can verify the list of nonCourseGradeUses was written to the cache
                cacheProviderMock.Setup(x => x.Add(It.IsAny<string>(), It.IsAny<Task<List<NonCourseGradeUses>>>(), It.IsAny<CacheItemPolicy>(), null)).Verifiable();

                cacheProviderMock.Setup(x => x.Contains(referenceDataRepo.BuildFullCacheKey("ST_NON.COURSE.GRADE.USES"), null)).Returns(true);
                var nonCourseGradeUses = await referenceDataRepo.GetNonCourseGradeUsesAsync(false);
                cacheProviderMock.Setup(x => x.Get(referenceDataRepo.BuildFullCacheKey("ST_NON.COURSE.GRADE.USES"), null)).Returns(nonCourseGradeUses);
                // Verify that nonCourseGradeUses were returned, which means they came from the "repository".
                Assert.IsTrue(nonCourseGradeUses.Count() == 3);

                // Verify that the nonCourseGradeUses item was added to the cache after it was read from the repository
                cacheProviderMock.Verify(m => m.Add(It.IsAny<string>(), It.IsAny<Task<List<NonCourseGradeUses>>>(), It.IsAny<CacheItemPolicy>(), null), Times.Never);

            }

            [TestMethod]
            public async Task StudentReferenceDataRepo_GetNonCourseGradeUses_GetsCachedNonCourseGradeUsesAsync()
            {
                // Set up local cache mock to respond to cache request:
                //  -to "Contains" request, return "true" to indicate item is in cache
                //  -to "Get" request, return the cache item (in this case the "NON.COURSE.GRADE.USES" cache item)
                cacheProviderMock.Setup(x => x.Contains(valcodeName, null)).Returns(true);
                cacheProviderMock.Setup(x => x.Get(valcodeName, null)).Returns(allNonCourseGradeUses).Verifiable();

                // return null for request, so that if we have a result, it wasn't the data accessor that returned it.
                dataAccessorMock.Setup(acc => acc.ReadRecordAsync<ApplValcodes>("ST.VALCODES", "NON.COURSE.GRADE.USES", true)).ReturnsAsync(new ApplValcodes());

                // Assert the nonCourseGradeUses are returned
                Assert.IsTrue((await referenceDataRepo.GetNonCourseGradeUsesAsync(false)).Count() == 3);
                // Verify that the snonCourseGradeUses were retrieved from cache
                cacheProviderMock.Verify(m => m.Get(valcodeName, null));
            }

            private StudentReferenceDataRepository BuildValidReferenceDataRepository()
            {
                // transaction factory mock
                transFactoryMock = new Mock<IColleagueTransactionFactory>();
                // Cache Provider Mock
                cacheProviderMock = new Mock<ICacheProvider>();
                // Set up data accessor for mocking 
                dataAccessorMock = new Mock<IColleagueDataReader>();

                // Set up dataAccessorMock as the object for the DataAccessor
                transFactoryMock.Setup(transFac => transFac.GetDataReader()).Returns(dataAccessorMock.Object);

                // Setup response to nonCourseGradeUses valcode read
                dataAccessorMock.Setup(acc => acc.ReadRecordAsync<ApplValcodes>("ST.VALCODES", "NON.COURSE.GRADE.USES", It.IsAny<bool>())).ReturnsAsync(nonCourseGradeUsesValcodeResponse);
                cacheProviderMock.Setup<Task<Tuple<object, SemaphoreSlim>>>(x => x.GetAndLockSemaphoreAsync(It.IsAny<string>(), null))
                .ReturnsAsync(new Tuple<object, SemaphoreSlim>(null, new SemaphoreSlim(1, 1)));

                dataAccessorMock.Setup(acc => acc.SelectAsync(It.IsAny<RecordKeyLookup[]>())).Returns<RecordKeyLookup[]>(recordKeyLookups =>
                {
                    var result = new Dictionary<string, RecordKeyLookupResult>();
                    foreach (var recordKeyLookup in recordKeyLookups)
                    {
                        var nonCourseGradeUses = allNonCourseGradeUses.Where(e => e.Code == recordKeyLookup.SecondaryKey).FirstOrDefault();
                        result.Add(string.Join("+", new string[] { "ST.VALCODES", "NON.COURSE.GRADE.USES", nonCourseGradeUses.Code }),
                            new RecordKeyLookupResult() { Guid = nonCourseGradeUses.Guid });
                    }
                    return Task.FromResult(result);
                });

                // Construct repository
                referenceDataRepo = new StudentReferenceDataRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object, apiSettings);

                return referenceDataRepo;
            }

            private ApplValcodes BuildValcodeResponse(IEnumerable<NonCourseGradeUses> nonCourseGradeUses)
            {
                ApplValcodes nonCourseGradeUsesResponse = new ApplValcodes();
                nonCourseGradeUsesResponse.ValsEntityAssociation = new List<ApplValcodesVals>();
                foreach (var item in nonCourseGradeUses)
                {
                    nonCourseGradeUsesResponse.ValsEntityAssociation.Add(new ApplValcodesVals("", item.Description, "2", item.Code, "3", "", ""));
                }
                return nonCourseGradeUsesResponse;
            }
        }

        [TestClass]
        public class PetitionStatuses_Get
        {
            Mock<IColleagueTransactionFactory> transFactoryMock;
            Mock<ICacheProvider> cacheProviderMock;
            Mock<IColleagueDataReader> dataAccessorMock;
            Mock<ILogger> loggerMock;
            ApiSettings apiSettings;
            Collection<PetitionStatuses> petitionStatusResponse;
            StudentReferenceDataRepository referenceDataRepo;

            [TestInitialize]
            public void Initialize()
            {
                loggerMock = new Mock<ILogger>();
                apiSettings = new ApiSettings("TEST");

                petitionStatusResponse = BuildPetitionStatusResponse();
                referenceDataRepo = BuildValidReferenceDataRepository();

                cacheProviderMock.Setup<Task<Tuple<object, SemaphoreSlim>>>(x =>
                   x.GetAndLockSemaphoreAsync(It.IsAny<string>(), null))
                   .ReturnsAsync(new Tuple<object, SemaphoreSlim>(null, new SemaphoreSlim(1, 1)));

            }

            [TestMethod]
            public async Task StudentReferenceDataRepo_GetsPetitionStatuses()
            {
                var petitionStatuses = await referenceDataRepo.GetPetitionStatusesAsync();
                Assert.AreEqual(petitionStatusResponse.Count(), petitionStatuses.Count());
                foreach (var item in petitionStatusResponse)
                {
                    var entry = petitionStatuses.Where(p => p.Code == item.Recordkey).FirstOrDefault();
                    Assert.AreEqual(item.PetDesc, entry.Description);
                    Assert.AreEqual(item.PetGrantedFlag == "Y" ? true : false, entry.IsGranted);
                }
            }

            private StudentReferenceDataRepository BuildValidReferenceDataRepository()
            {
                transFactoryMock = new Mock<IColleagueTransactionFactory>();
                cacheProviderMock = new Mock<ICacheProvider>();
                dataAccessorMock = new Mock<IColleagueDataReader>();
                transFactoryMock.Setup(transFac => transFac.GetDataReader()).Returns(dataAccessorMock.Object);

                // Setup response to petition status valcode read
                dataAccessorMock.Setup<Task<Collection<PetitionStatuses>>>(acc => acc.BulkReadRecordAsync<PetitionStatuses>("PETITION.STATUSES", "", true)).ReturnsAsync(petitionStatusResponse);
                // Construct course repository
                referenceDataRepo = new StudentReferenceDataRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object, apiSettings);

                return referenceDataRepo;
            }

            public Collection<PetitionStatuses> BuildPetitionStatusResponse()
            {
                Collection<PetitionStatuses> results = new Collection<PetitionStatuses>() { new PetitionStatuses() { Recordkey = "A", PetDesc = "Accepted", PetGrantedFlag = "Y" }, new PetitionStatuses() { Recordkey = "D", PetDesc = "Denied", PetGrantedFlag = "N" } };

                return results;
            }
        }

        [TestClass]
        public class RoomRateTests : BaseRepositorySetup
        {
            Mock<IColleagueTransactionFactory> transFactoryMock;
            Mock<IColleagueDataReader> dataReaderMock;
            //Mock<ICacheProvider> cacheProviderMock;
            Mock<IColleagueDataReader> dataAccessorMock;
            Mock<ILogger> loggerMock;
            Collection<DataContracts.RoomRateTables> _roomRatesCollection;
            string codeItemName;
            ApiSettings apiSettings;

            StudentReferenceDataRepository referenceDataRepo;

            [TestInitialize]
            public void Initialize()
            {
                loggerMock = new Mock<ILogger>();
                dataAccessorMock = new Mock<IColleagueDataReader>();
                //cacheProviderMock = new Mock<ICacheProvider>();
                transFactoryMock = new Mock<IColleagueTransactionFactory>();
                dataReaderMock = new Mock<IColleagueDataReader>();
                apiSettings = new ApiSettings("TEST");

                string id, guid, id2, guid2, id3, guid3, sid, sid2, sid3;
                GuidLookup guidLookup;
                GuidLookupResult guidLookupResult;
                Dictionary<string, GuidLookupResult> guidLookupDict;
                RecordKeyLookup recordLookup;
                RecordKeyLookupResult recordLookupResult;
                Dictionary<string, RecordKeyLookupResult> recordLookupDict;

                // Set up for GUID lookups
                id = "1";
                id2 = "2";
                id3 = "3";

                // Secondary keys for GUID lookups
                sid = "11";
                sid2 = "22";
                sid3 = "33";

                guid = "F5FC5310-17F1-49FC-926D-CC6E3DA6DAEA".ToLowerInvariant();
                guid2 = "5B35075D-14FB-45F7-858A-83F4174B76EA".ToLowerInvariant();
                guid3 = "246E16D9-8790-4D7E-ACA1-D5B1CB9D4A24".ToLowerInvariant();

                var offsetDate = DmiString.DateTimeToPickDate(DateTime.Now);

                guidLookup = new GuidLookup(guid);
                guidLookupResult = new GuidLookupResult() { Entity = "ROOM.RATE.TABLES", PrimaryKey = id, SecondaryKey = offsetDate.ToString() };
                guidLookupDict = new Dictionary<string, GuidLookupResult>();
                recordLookup = new RecordKeyLookup("ROOM.RATE.TABLES", id, "RRT.EFFECTIVE.DATES", offsetDate.ToString(), false);
                recordLookupResult = new RecordKeyLookupResult() { Guid = guid };
                recordLookupDict = new Dictionary<string, RecordKeyLookupResult>();
                guidLookupDict.Add(guid, new GuidLookupResult() { Entity = "ROOM.RATE.TABLES", PrimaryKey = id, SecondaryKey = offsetDate.ToString() });
                guidLookupDict.Add(guid2, new GuidLookupResult() { Entity = "ROOM.RATE.TABLES", PrimaryKey = id2, SecondaryKey = offsetDate.ToString() });
                guidLookupDict.Add(guid3, new GuidLookupResult() { Entity = "ROOM.RATE.TABLES", PrimaryKey = id3, SecondaryKey = offsetDate.ToString() });

                recordLookupDict.Add("ROOM.RATE.TABLES+" + id + "+" + offsetDate.ToString(), new RecordKeyLookupResult() { Guid = guid });
                recordLookupDict.Add("ROOM.RATE.TABLES+" + id2 + "+" + offsetDate.ToString(), new RecordKeyLookupResult() { Guid = guid2 });
                recordLookupDict.Add("ROOM.RATE.TABLES+" + id3 + "+" + offsetDate.ToString(), new RecordKeyLookupResult() { Guid = guid3 });

                dataReaderMock.Setup(dr => dr.SelectAsync(It.IsAny<GuidLookup[]>())).ReturnsAsync(guidLookupDict);
                dataReaderMock.Setup(dr => dr.SelectAsync(It.IsAny<RecordKeyLookup[]>())).ReturnsAsync(recordLookupDict);
                dataAccessorMock.Setup(da => da.SelectAsync(It.IsAny<RecordKeyLookup[]>())).ReturnsAsync(recordLookupDict);


                // Build responses used for mocking
                _roomRatesCollection = new Collection<DataContracts.RoomRateTables>()
                {
                    new DataContracts.RoomRateTables() { RecordGuid = guid, Recordkey = id, RrtDesc = sid ,
                        RoomDateRatesEntityAssociation = new List<DataContracts.RoomRateTablesRoomDateRates>() { new DataContracts.RoomRateTablesRoomDateRates(DateTime.Now, 500, 500, 500, 500, 500 ) } },
                    new DataContracts.RoomRateTables() { RecordGuid = guid2, Recordkey = id2, RrtDesc = sid2  ,
                        RoomDateRatesEntityAssociation = new List<DataContracts.RoomRateTablesRoomDateRates>() { new DataContracts.RoomRateTablesRoomDateRates(DateTime.Now, 500, 500, 500, 500, 500 ) } },
                    new DataContracts.RoomRateTables() { RecordGuid = guid3, Recordkey = id3, RrtDesc = sid3 ,
                        RoomDateRatesEntityAssociation = new List<DataContracts.RoomRateTablesRoomDateRates>() { new DataContracts.RoomRateTablesRoomDateRates(DateTime.Now, 500, 500, 500, 500, 500 ) } },
                    };


                List<string> roomRateGuids = new List<string>();
                foreach (var mp in _roomRatesCollection)
                {
                    roomRateGuids.Add(mp.RecordGuid);
                };
                dataReaderMock.Setup(repo => repo.SelectAsync("ROOM.RATE.TABLES", It.IsAny<string>())).ReturnsAsync(roomRateGuids.ToArray());
                dataReaderMock.Setup(repo => repo.BulkReadRecordAsync<DataContracts.RoomRateTables>(It.IsAny<string[]>(), It.IsAny<bool>())).ReturnsAsync(_roomRatesCollection);

                referenceDataRepo = BuildValidReferenceDataRepository();

            }

            [TestCleanup]
            public void Cleanup()
            {
                MockCleanup();

                transFactoryMock = null;
                dataAccessorMock = null;
                cacheProviderMock = null;
                _roomRatesCollection = null;
                referenceDataRepo = null;
            }

            //[TestMethod]
            //public async Task StudentReferenceDataRepo_GetGetRoomRatesAsync()
            //{
            //    var result = await referenceDataRepo.GetRoomRatesAsync(It.IsAny<bool>());

            //    for (int i = 0; i < _roomRatesCollection.Count(); i++)
            //    {
            //        Assert.AreEqual(_roomRatesCollection.ElementAt(i).RecordGuid, result.ElementAt(i).Guid);
            //        Assert.AreEqual(_roomRatesCollection.ElementAt(i).Recordkey, result.ElementAt(i).Code);
            //        Assert.AreEqual(_roomRatesCollection.ElementAt(i).RrtDesc, result.ElementAt(i).Description);
            //    }
            //}

            [TestMethod]
            [ExpectedException(typeof(Exception))]
            public async Task StudentReferenceDataRepo_GetsGetRoomRates_Exception()
            {
                dataReaderMock.Setup(dr => dr.SelectAsync(It.IsAny<GuidLookup[]>())).ReturnsAsync(null);
                dataReaderMock.Setup(dr => dr.SelectAsync(It.IsAny<RecordKeyLookup[]>())).ReturnsAsync(null);
                dataAccessorMock.Setup(da => da.SelectAsync(It.IsAny<RecordKeyLookup[]>())).ReturnsAsync(null);

                await referenceDataRepo.GetRoomRatesAsync(true);
            }

            private StudentReferenceDataRepository BuildValidReferenceDataRepository()
            {
                // Initialize the Mock framework
                MockInitialize();

                transFactoryMock.Setup(transFac => transFac.GetDataReader()).Returns(dataReaderMock.Object);

                // Cache mocking
                cacheProviderMock = new Mock<ICacheProvider>();
                /*cacheProviderMock.Setup<Task<Tuple<object, SemaphoreSlim>>>(x =>
                x.GetAndLockSemaphoreAsync(It.IsAny<string>(), null))
                .Returns(Task.FromResult(new Tuple<object, SemaphoreSlim>(
                null,
                new SemaphoreSlim(1, 1)
                )));*/

                cacheProviderMock.Setup<Task<Tuple<object, SemaphoreSlim>>>(x =>
             x.GetAndLockSemaphoreAsync(It.IsAny<string>(), null))
             .ReturnsAsync(new Tuple<object, SemaphoreSlim>(null, null));

                referenceDataRepo = new StudentReferenceDataRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object, apiSettings);
                return referenceDataRepo;
            }
        }

        [TestClass]
        public class SectionGradeTypes
        {
            Mock<IColleagueTransactionFactory> transFactoryMock;
            Mock<ICacheProvider> cacheProviderMock;
            Mock<IColleagueDataReader> dataAccessorMock;
            Mock<ILogger> loggerMock;
            IEnumerable<SectionGradeType> allSectionGradeTypes;
            ApplValcodes sectionGradeTypeValcodeResponse;
            string valcodeName;
            ApiSettings apiSettings;

            Mock<IStudentReferenceDataRepository> referenceDataRepositoryMock;
            IStudentReferenceDataRepository referenceDataRepository;
            StudentReferenceDataRepository referenceDataRepo;

            [TestInitialize]
            public void Initialize()
            {
                loggerMock = new Mock<ILogger>();
                apiSettings = new ApiSettings("TEST");

                // Build section grade types responses used for mocking
                allSectionGradeTypes = new TestStudentReferenceDataRepository().GetSectionGradeTypesAsync().Result;
                sectionGradeTypeValcodeResponse = BuildValcodeResponse(allSectionGradeTypes);
                var sectionGradeTypeValResponse = new List<string>() { "2" };
                sectionGradeTypeValcodeResponse.ValActionCode1 = sectionGradeTypeValResponse;

                referenceDataRepositoryMock = new Mock<IStudentReferenceDataRepository>();
                referenceDataRepository = referenceDataRepositoryMock.Object;

                // Build sectionGradeType repository
                referenceDataRepo = BuildValidReferenceDataRepository();
                valcodeName = referenceDataRepo.BuildFullCacheKey("ST_INTG.SECTION.GRADE.TYPES_GUID");
            }

            [TestCleanup]
            public void Cleanup()
            {
                transFactoryMock = null;
                dataAccessorMock = null;
                cacheProviderMock = null;
                sectionGradeTypeValcodeResponse = null;
                allSectionGradeTypes = null;
                referenceDataRepo = null;
            }

            [TestMethod]
            public async Task StudentReferenceDataRepo_GetSectionGradeTypesCacheAsync()
            {
                var types = await referenceDataRepo.GetSectionGradeTypesAsync(false);

                for (int i = 0; i < allSectionGradeTypes.Count(); i++)
                {
                    Assert.AreEqual(allSectionGradeTypes.ElementAt(i).Code, types.ElementAt(i).Code);
                    Assert.AreEqual(allSectionGradeTypes.ElementAt(i).Description, types.ElementAt(i).Description);
                }
            }

            [TestMethod]
            public async Task StudentReferenceDataRepo_GetSectionGradeTypesNonCacheAsync()
            {
                var types = await referenceDataRepo.GetSectionGradeTypesAsync(true);

                for (int i = 0; i < allSectionGradeTypes.Count(); i++)
                {
                    Assert.AreEqual(allSectionGradeTypes.ElementAt(i).Code, types.ElementAt(i).Code);
                    Assert.AreEqual(allSectionGradeTypes.ElementAt(i).Description, types.ElementAt(i).Description);
                }
            }

            [TestMethod]
            public async Task StudentReferenceDataRepo_GetSectionGradeTypesNoArgAsync()
            {
                var types = await referenceDataRepo.GetSectionGradeTypesAsync();

                for (int i = 0; i < allSectionGradeTypes.Count(); i++)
                {
                    Assert.AreEqual(allSectionGradeTypes.ElementAt(i).Code, types.ElementAt(i).Code);
                    Assert.AreEqual(allSectionGradeTypes.ElementAt(i).Description, types.ElementAt(i).Description);
                }
            }

            [TestMethod]
            public async Task StudentReferenceDataRepo_GetSectionGradeTypes_WritesToCacheAsync()
            {

                // Set up local cache mock to respond to cache request:
                //  -to "Contains" request, return "false" to indicate item is not in cache
                //  -to cache "Get" request, return null so we know it's reading from the "repository"
                cacheProviderMock.Setup(x => x.Contains(valcodeName, null)).Returns(false);
                cacheProviderMock.Setup(x => x.Get(valcodeName, null)).Returns(null);

                // return a valid response to the data accessor request
                dataAccessorMock.Setup(acc => acc.ReadRecordAsync<ApplValcodes>("ST.VALCODES", "INTG.SECTION.GRADE.TYPES", It.IsAny<bool>())).ReturnsAsync(sectionGradeTypeValcodeResponse);

                cacheProviderMock.Setup<Task<Tuple<object, SemaphoreSlim>>>(x =>
                 x.GetAndLockSemaphoreAsync(It.IsAny<string>(), null))
                 .ReturnsAsync(new Tuple<object, SemaphoreSlim>(null, new SemaphoreSlim(1, 1)));

                // But after data accessor read, set up mocking so we can verify the list of sectionGradeTypes was written to the cache
                cacheProviderMock.Setup(x => x.Add(It.IsAny<string>(), It.IsAny<Task<List<SectionGradeType>>>(), It.IsAny<CacheItemPolicy>(), null)).Verifiable();

                cacheProviderMock.Setup(x => x.Contains(referenceDataRepo.BuildFullCacheKey("ST_INTG.SECTION.GRADE.TYPES"), null)).Returns(true);
                var sectionGradeTypes = await referenceDataRepo.GetSectionGradeTypesAsync(false);
                cacheProviderMock.Setup(x => x.Get(referenceDataRepo.BuildFullCacheKey("ST_INTG.SECTION.GRADE.TYPES"), null)).Returns(sectionGradeTypes);
                // Verify that sectionGradeTypes were returned, which means they came from the "repository".
                Assert.IsTrue(sectionGradeTypes.Count() == 8);

                // Verify that the sectionGradeType item was added to the cache after it was read from the repository
                cacheProviderMock.Verify(m => m.Add(It.IsAny<string>(), It.IsAny<Task<List<SectionGradeType>>>(), It.IsAny<CacheItemPolicy>(), null), Times.Never);
            }

            [TestMethod]
            public async Task StudentReferenceDataRepo_GetSectionGradeTypes_GetsCachedSectionGradeTypes()
            {
                // Set up local cache mock to respond to cache request:
                //  -to "Contains" request, return "true" to indicate item is in cache
                //  -to "Get" request, return the cache item (in this case the "INTG.SECTION.GRADE.TYPES" cache item)
                cacheProviderMock.Setup(x => x.Contains(valcodeName, null)).Returns(true);
                cacheProviderMock.Setup(x => x.Get(valcodeName, null)).Returns(allSectionGradeTypes).Verifiable();

                // return null for request, so that if we have a result, it wasn't the data accessor that returned it.
                dataAccessorMock.Setup(acc => acc.ReadRecordAsync<ApplValcodes>("ST.VALCODES", "INTG.SECTION.GRADE.TYPES", true)).ReturnsAsync(new ApplValcodes());

                // Assert the sectionGradeTypes are returned
                Assert.IsTrue((await referenceDataRepo.GetSectionGradeTypesAsync(false)).Count() == allSectionGradeTypes.Count());
                // Verify that the sectionGradeTypes were retrieved from cache
                cacheProviderMock.Verify(m => m.Get(valcodeName, null));
            }

            private StudentReferenceDataRepository BuildValidReferenceDataRepository()
            {
                // transaction factory mock
                transFactoryMock = new Mock<IColleagueTransactionFactory>();
                // Cache Provider Mock
                cacheProviderMock = new Mock<ICacheProvider>();
                // Set up data accessor for mocking 
                dataAccessorMock = new Mock<IColleagueDataReader>();

                // Set up dataAccessorMock as the object for the DataAccessor
                transFactoryMock.Setup(transFac => transFac.GetDataReader()).Returns(dataAccessorMock.Object);

                // Setup response to sectionGradeType valcode read
                dataAccessorMock.Setup(acc => acc.ReadRecordAsync<ApplValcodes>("ST.VALCODES", "INTG.SECTION.GRADE.TYPES", It.IsAny<bool>())).ReturnsAsync(sectionGradeTypeValcodeResponse);
                cacheProviderMock.Setup<Task<Tuple<object, SemaphoreSlim>>>(x => x.GetAndLockSemaphoreAsync(It.IsAny<string>(), null))
                .ReturnsAsync(new Tuple<object, SemaphoreSlim>(null, new SemaphoreSlim(1, 1)));

                dataAccessorMock.Setup(acc => acc.SelectAsync(It.IsAny<RecordKeyLookup[]>())).Returns<RecordKeyLookup[]>(recordKeyLookups =>
                {
                    var result = new Dictionary<string, RecordKeyLookupResult>();
                    foreach (var recordKeyLookup in recordKeyLookups)
                    {
                        var sectionGradeType = allSectionGradeTypes.Where(e => e.Code == recordKeyLookup.SecondaryKey).FirstOrDefault();
                        result.Add(string.Join("+", new string[] { "ST.VALCODES", "INTG.SECTION.GRADE.TYPES", sectionGradeType.Code }),
                            new RecordKeyLookupResult() { Guid = sectionGradeType.Guid });
                    }
                    return Task.FromResult(result);
                });

                // Construct repository
                referenceDataRepo = new StudentReferenceDataRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object, apiSettings);

                return referenceDataRepo;
            }

            private ApplValcodes BuildValcodeResponse(IEnumerable<SectionGradeType> sectionGradeTypes)
            {
                ApplValcodes sectionGradeTypesResponse = new ApplValcodes();
                sectionGradeTypesResponse.ValsEntityAssociation = new List<ApplValcodesVals>();
                foreach (var item in sectionGradeTypes)
                {
                    sectionGradeTypesResponse.ValsEntityAssociation.Add(new ApplValcodesVals("", item.Description, "2", item.Code, "3", "", ""));
                }
                return sectionGradeTypesResponse;
            }
        }

        [TestClass]
        public class SectionRegistrationStatuses
        {
            Mock<IColleagueTransactionFactory> transFactoryMock;
            Mock<ICacheProvider> cacheProviderMock;
            Mock<IColleagueDataReader> dataAccessorMock;
            Mock<ILogger> loggerMock;
            IEnumerable<SectionRegistrationStatusItem> allSectionRegistrationStatuses;
            ApplValcodes sectionRegistrationStatusValcodeResponse;
            string valcodeName;
            ApiSettings apiSettings;

            Mock<IStudentReferenceDataRepository> referenceDataRepositoryMock;
            IStudentReferenceDataRepository referenceDataRepository;
            StudentReferenceDataRepository referenceDataRepo;

            [TestInitialize]
            public void Initialize()
            {
                loggerMock = new Mock<ILogger>();
                apiSettings = new ApiSettings("TEST");

                // Build section registration statuses responses used for mocking
                allSectionRegistrationStatuses = new TestStudentReferenceDataRepository().GetStudentAcademicCreditStatusesAsync().Result;
                sectionRegistrationStatusValcodeResponse = BuildValcodeResponse(allSectionRegistrationStatuses);
                var sectionRegValResponse = new List<string>() { "2" };
                sectionRegistrationStatusValcodeResponse.ValActionCode1 = sectionRegValResponse;

                referenceDataRepositoryMock = new Mock<IStudentReferenceDataRepository>();
                referenceDataRepository = referenceDataRepositoryMock.Object;

                // Build sectionRegistrationStatus repository
                referenceDataRepo = BuildValidReferenceDataRepository();
                valcodeName = referenceDataRepo.BuildFullCacheKey("ST_STUDENT.ACAD.CRED.STATUSES_GUID");
            }

            [TestCleanup]
            public void Cleanup()
            {
                transFactoryMock = null;
                dataAccessorMock = null;
                cacheProviderMock = null;
                sectionRegistrationStatusValcodeResponse = null;
                allSectionRegistrationStatuses = null;
                referenceDataRepo = null;
            }
            
            [TestMethod]
            public async Task StudentReferenceDataRepo_GetSectionRegistrationStatusesCacheAsync()
            {
                var statuses = await referenceDataRepo.GetStudentAcademicCreditStatusesAsync(false);

                for (int i = 0; i < allSectionRegistrationStatuses.Count(); i++)
                {
                    Assert.AreEqual(allSectionRegistrationStatuses.ElementAt(i).Code, statuses.ElementAt(i).Code);
                    Assert.AreEqual(allSectionRegistrationStatuses.ElementAt(i).Description, statuses.ElementAt(i).Description);
                }
            }
            
            [TestMethod]
            public async Task StudentReferenceDataRepo_GetSectionRegistrationStatusesNonCacheAsync()
            {
                var statuses = await referenceDataRepo.GetStudentAcademicCreditStatusesAsync(true);

                for (int i = 0; i < allSectionRegistrationStatuses.Count(); i++)
                {
                    Assert.AreEqual(allSectionRegistrationStatuses.ElementAt(i).Code, statuses.ElementAt(i).Code);
                    Assert.AreEqual(allSectionRegistrationStatuses.ElementAt(i).Description, statuses.ElementAt(i).Description);
                }
            }

            [TestMethod]
            public async Task StudentReferenceDataRepo_GetSectionRegistrationStatuses_WritesToCacheAsync()
            {

                // Set up local cache mock to respond to cache request:
                //  -to "Contains" request, return "false" to indicate item is not in cache
                //  -to cache "Get" request, return null so we know it's reading from the "repository"
                cacheProviderMock.Setup(x => x.Contains(valcodeName, null)).Returns(false);
                cacheProviderMock.Setup(x => x.Get(valcodeName, null)).Returns(null);

                // return a valid response to the data accessor request
                dataAccessorMock.Setup(acc => acc.ReadRecordAsync<ApplValcodes>("ST.VALCODES", "STUDENT.ACAD.CRED.STATUSES", It.IsAny<bool>())).ReturnsAsync(sectionRegistrationStatusValcodeResponse);

                cacheProviderMock.Setup<Task<Tuple<object, SemaphoreSlim>>>(x =>
                 x.GetAndLockSemaphoreAsync(It.IsAny<string>(), null))
                 .ReturnsAsync(new Tuple<object, SemaphoreSlim>(null, new SemaphoreSlim(1, 1)));

                // But after data accessor read, set up mocking so we can verify the list of sectionRegistrationStatuses was written to the cache
                cacheProviderMock.Setup(x => x.Add(It.IsAny<string>(), It.IsAny<Task<List<SectionRegistrationStatusItem>>>(), It.IsAny<CacheItemPolicy>(), null)).Verifiable();

                cacheProviderMock.Setup(x => x.Contains(referenceDataRepo.BuildFullCacheKey("ST_STUDENT.ACAD.CRED.STATUSES"), null)).Returns(true);
                var sectionRegistrationStatuses = await referenceDataRepo.GetStudentAcademicCreditStatusesAsync(false);
                cacheProviderMock.Setup(x => x.Get(referenceDataRepo.BuildFullCacheKey("ST_STUDENT.ACAD.CRED.STATUSES"), null)).Returns(sectionRegistrationStatuses);
                // Verify that sectionRegistrationStatuses were returned, which means they came from the "repository".
                Assert.IsTrue(sectionRegistrationStatuses.Count() == allSectionRegistrationStatuses.Count());

                // Verify that the sectionRegistrationStatus item was added to the cache after it was read from the repository
                cacheProviderMock.Verify(m => m.Add(It.IsAny<string>(), It.IsAny<Task<List<SectionRegistrationStatusItem>>>(), It.IsAny<CacheItemPolicy>(), null), Times.Never);


            }

            [TestMethod]
            public async Task StudentReferenceDataRepo_GetSectionRegistrationStatuses_GetsCachedSectionRegistrationStatusesAsync()
            {
                // Set up local cache mock to respond to cache request:
                //  -to "Contains" request, return "true" to indicate item is in cache
                //  -to "Get" request, return the cache item (in this case the "STUDENT.ACAD.CRED.STATUSES" cache item)
                cacheProviderMock.Setup(x => x.Contains(valcodeName, null)).Returns(true);
                cacheProviderMock.Setup(x => x.Get(valcodeName, null)).Returns(allSectionRegistrationStatuses).Verifiable();

                // return null for request, so that if we have a result, it wasn't the data accessor that returned it.
                dataAccessorMock.Setup(acc => acc.ReadRecordAsync<ApplValcodes>("ST.VALCODES", "STUDENT.ACAD.CRED.STATUSES", true)).ReturnsAsync(new ApplValcodes());

                // Assert the sectionRegistrationStatuses are returned
                Assert.IsTrue((await referenceDataRepo.GetStudentAcademicCreditStatusesAsync(false)).Count() == allSectionRegistrationStatuses.Count());
                // Verify that the sectionRegistrationStatuses were retrieved from cache
                cacheProviderMock.Verify(m => m.Get(valcodeName, null));
            }
            
            private StudentReferenceDataRepository BuildValidReferenceDataRepository()
            {
                // transaction factory mock
                transFactoryMock = new Mock<IColleagueTransactionFactory>();
                // Cache Mock
                //localCacheMock = new Mock<ObjectCache>();
                // Cache Provider Mock
                cacheProviderMock = new Mock<ICacheProvider>();
                // Set up data accessor for mocking 
                dataAccessorMock = new Mock<IColleagueDataReader>();

                // Set up dataAccessorMock as the object for the DataAccessor
                transFactoryMock.Setup(transFac => transFac.GetDataReader()).Returns(dataAccessorMock.Object);

                // Setup response to valcode read
                dataAccessorMock.Setup(acc => acc.ReadRecordAsync<ApplValcodes>("ST.VALCODES", "STUDENT.ACAD.CRED.STATUSES", It.IsAny<bool>())).ReturnsAsync(sectionRegistrationStatusValcodeResponse);
                cacheProviderMock.Setup<Task<Tuple<object, SemaphoreSlim>>>(x => x.GetAndLockSemaphoreAsync(It.IsAny<string>(), null))
                .ReturnsAsync(new Tuple<object, SemaphoreSlim>(null, new SemaphoreSlim(1, 1)));

                dataAccessorMock.Setup(acc => acc.SelectAsync(It.IsAny<RecordKeyLookup[]>())).Returns<RecordKeyLookup[]>(recordKeyLookups =>
                {
                    var result = new Dictionary<string, RecordKeyLookupResult>();
                    foreach (var recordKeyLookup in recordKeyLookups)
                    {
                        var sectionRegistrationStatus = allSectionRegistrationStatuses.Where(e => e.Code == recordKeyLookup.SecondaryKey).FirstOrDefault();
                        result.Add(string.Join("+", new string[] { "ST.VALCODES", "STUDENT.ACAD.CRED.STATUSES", sectionRegistrationStatus.Code }),
                            new RecordKeyLookupResult() { Guid = sectionRegistrationStatus.Guid });
                    }
                    return Task.FromResult(result);
                });

                // Construct repository
                referenceDataRepo = new StudentReferenceDataRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object, apiSettings);

                return referenceDataRepo;
            }

            private ApplValcodes BuildValcodeResponse(IEnumerable<SectionRegistrationStatusItem> sectionRegistrationStatuses)
            {
                ApplValcodes sectionRegistrationStatusesResponse = new ApplValcodes();
                sectionRegistrationStatusesResponse.ValsEntityAssociation = new List<ApplValcodesVals>();
                foreach (var item in sectionRegistrationStatuses)
                {
                    sectionRegistrationStatusesResponse.ValsEntityAssociation.Add(new ApplValcodesVals("", item.Description, "2", item.Code, "3", "", ""));
                }
                return sectionRegistrationStatusesResponse;
            }
        }

        [TestClass]
        public class SectionStatusesTests
        {
            Mock<IColleagueTransactionFactory> transFactoryMock;
            Mock<ICacheProvider> cacheProviderMock;
            Mock<IColleagueDataReader> dataAccessorMock;
            Mock<ILogger> loggerMock;
            IEnumerable<SectionStatuses> allSectionStatuses;
            ApplValcodes sectionStatusesValcodeResponse;
            string domainEntityNameName;
            ApiSettings apiSettings;

            Mock<IStudentReferenceDataRepository> referenceDataRepositoryMock;
            IStudentReferenceDataRepository referenceDataRepository;
            StudentReferenceDataRepository referenceDataRepo;

            [TestInitialize]
            public void Initialize()
            {
                loggerMock = new Mock<ILogger>();
                apiSettings = new ApiSettings("TEST");

                allSectionStatuses = new TestStudentReferenceDataRepository().GetSectionStatusesAsync(false).Result;
                sectionStatusesValcodeResponse = BuildValcodeResponse(allSectionStatuses);
                var sectionStatusesValResponse = new List<string>() { "2" };
                sectionStatusesValcodeResponse.ValActionCode1 = sectionStatusesValResponse;

                referenceDataRepositoryMock = new Mock<IStudentReferenceDataRepository>();
                referenceDataRepository = referenceDataRepositoryMock.Object;

                referenceDataRepo = BuildValidReferenceDataRepository();
                domainEntityNameName = referenceDataRepo.BuildFullCacheKey("ST_SECTION.STATUSES_GUID");

                cacheProviderMock.Setup<Task<Tuple<object, SemaphoreSlim>>>(x =>
                   x.GetAndLockSemaphoreAsync(It.IsAny<string>(), null))
                   .ReturnsAsync(new Tuple<object, SemaphoreSlim>(null, new SemaphoreSlim(1, 1)));
            }

            [TestCleanup]
            public void Cleanup()
            {
                transFactoryMock = null;
                dataAccessorMock = null;
                cacheProviderMock = null;
                sectionStatusesValcodeResponse = null;
                allSectionStatuses = null;
                referenceDataRepo = null;
            }


            //[TestMethod]
            //public async Task StudentReferenceDataRepo_GetsSectionStatusesCacheAsync()
            //{
            //    var sectionStatuses = await referenceDataRepo.GetSectionStatusesAsync(false);

            //    for (int i = 0; i < allSectionStatuses.Count(); i++)
            //    {
            //        Assert.AreEqual(allSectionStatuses.ElementAt(i).Code, sectionStatuses.ElementAt(i).Code);
            //        Assert.AreEqual(allSectionStatuses.ElementAt(i).Description, sectionStatuses.ElementAt(i).Description);
            //    }
            //}

            //[TestMethod]
            //public async Task StudentReferenceDataRepo_GetsSectionStatusesNonCacheAsync()
            //{
            //    var statuses = await referenceDataRepo.GetSectionStatusesAsync(true);

            //    for (int i = 0; i < allSectionStatuses.Count(); i++)
            //    {
            //        Assert.AreEqual(allSectionStatuses.ElementAt(i).Code, statuses.ElementAt(i).Code);
            //        Assert.AreEqual(allSectionStatuses.ElementAt(i).Description, statuses.ElementAt(i).Description);
            //    }
            //}

            //[TestMethod]
            //public async Task StudentReferenceDataRepo_GetSectionStatuses_WritesToCacheAsync()
            //{

            //    // Set up local cache mock to respond to cache request:
            //    //  -to "Contains" request, return "false" to indicate item is not in cache
            //    //  -to cache "Get" request, return null so we know it's reading from the "repository"
            //    cacheProviderMock.Setup(x => x.Contains(domainEntityNameName, null)).Returns(false);
            //    cacheProviderMock.Setup(x => x.Get(domainEntityNameName, null)).Returns(null);

            //    // return a valid response to the data accessor request
            //    dataAccessorMock.Setup(acc => acc.ReadRecordAsync<ApplValcodes>("ST.VALCODES", "SECTION.STATUSES", It.IsAny<bool>())).ReturnsAsync(sectionStatusesValcodeResponse);

            //    cacheProviderMock.Setup<Task<Tuple<object, SemaphoreSlim>>>(x =>
            //     x.GetAndLockSemaphoreAsync(It.IsAny<string>(), null))
            //     .ReturnsAsync(new Tuple<object, SemaphoreSlim>(null, new SemaphoreSlim(1, 1)));

            //    // But after data accessor read, set up mocking so we can verify the list of sectionStatuses was written to the cache
            //    cacheProviderMock.Setup(x => x.Add(It.IsAny<string>(), It.IsAny<Task<List<SectionStatuses>>>(), It.IsAny<CacheItemPolicy>(), null)).Verifiable();

            //    cacheProviderMock.Setup(x => x.Contains(referenceDataRepo.BuildFullCacheKey("ST_SECTION.STATUSES"), null)).Returns(true);
            //    var sectionStatuses = await referenceDataRepo.GetSectionStatusesAsync(false);
            //    cacheProviderMock.Setup(x => x.Get(referenceDataRepo.BuildFullCacheKey("ST_SECTION.STATUSES"), null)).Returns(sectionStatuses);
            //    // Verify that sectionStatuses were returned, which means they came from the "repository".
            //    Assert.IsTrue(sectionStatuses.Count() == 3);

            //    // Verify that the sectionStatuses item was added to the cache after it was read from the repository
            //    cacheProviderMock.Verify(m => m.Add(It.IsAny<string>(), It.IsAny<Task<List<SectionStatuses>>>(), It.IsAny<CacheItemPolicy>(), null), Times.Never);

            //}

            //[TestMethod]
            //public async Task StudentReferenceDataRepo_GetSectionStatuses_GetsCachedSectionStatusesAsync()
            //{
            //    // Set up local cache mock to respond to cache request:
            //    //  -to "Contains" request, return "true" to indicate item is in cache
            //    //  -to "Get" request, return the cache item (in this case the "SECTION.STATUSES" cache item)
            //    cacheProviderMock.Setup(x => x.Contains(domainEntityNameName, null)).Returns(true);
            //    cacheProviderMock.Setup(x => x.Get(domainEntityNameName, null)).Returns(allSectionStatuses).Verifiable();

            //    // return null for request, so that if we have a result, it wasn't the data accessor that returned it.
            //    dataAccessorMock.Setup(acc => acc.ReadRecordAsync<ApplValcodes>("ST.VALCODES", "SECTION.STATUSES", true)).ReturnsAsync(new ApplValcodes());

            //    // Assert the sectionStatuses are returned
            //    Assert.IsTrue((await referenceDataRepo.GetSectionStatusesAsync(false)).Count() == 3);
            //    // Verify that the ssectionStatuses were retrieved from cache
            //    cacheProviderMock.Verify(m => m.Get(domainEntityNameName, null));
            //}

            private StudentReferenceDataRepository BuildValidReferenceDataRepository()
            {
                // transaction factory mock
                transFactoryMock = new Mock<IColleagueTransactionFactory>();
                // Cache Provider Mock
                cacheProviderMock = new Mock<ICacheProvider>();
                // Set up data accessor for mocking 
                dataAccessorMock = new Mock<IColleagueDataReader>();

                // Set up dataAccessorMock as the object for the DataAccessor
                transFactoryMock.Setup(transFac => transFac.GetDataReader()).Returns(dataAccessorMock.Object);

                // Setup response to sectionStatuses domainEntityName read
                dataAccessorMock.Setup(acc => acc.ReadRecordAsync<ApplValcodes>("ST.VALCODES", "SECTION.STATUSES", It.IsAny<bool>())).ReturnsAsync(sectionStatusesValcodeResponse);
                cacheProviderMock.Setup<Task<Tuple<object, SemaphoreSlim>>>(x => x.GetAndLockSemaphoreAsync(It.IsAny<string>(), null))
                .ReturnsAsync(new Tuple<object, SemaphoreSlim>(null, new SemaphoreSlim(1, 1)));

                dataAccessorMock.Setup(acc => acc.SelectAsync(It.IsAny<RecordKeyLookup[]>())).Returns<RecordKeyLookup[]>(recordKeyLookups =>
                {
                    var result = new Dictionary<string, RecordKeyLookupResult>();
                    foreach (var recordKeyLookup in recordKeyLookups)
                    {
                        var sectionStatuses = allSectionStatuses.Where(e => e.Code == recordKeyLookup.SecondaryKey).FirstOrDefault();
                        result.Add(string.Join("+", new string[] { "ST.SECTION.STATUSES", "SECTION.STATUSES", sectionStatuses.Code }),
                            new RecordKeyLookupResult() { Guid = sectionStatuses.Guid });
                    }
                    return Task.FromResult(result);
                });

                // Construct repository
                referenceDataRepo = new StudentReferenceDataRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object, apiSettings);

                return referenceDataRepo;
            }

            private ApplValcodes BuildValcodeResponse(IEnumerable<SectionStatuses> sectionStatuses)
            {
                ApplValcodes sectionStatusesResponse = new ApplValcodes();
                sectionStatusesResponse.ValsEntityAssociation = new List<ApplValcodesVals>();
                foreach (var item in sectionStatuses)
                {
                    sectionStatusesResponse.ValsEntityAssociation.Add(new ApplValcodesVals("", item.Description, "2", item.Code, "3", "", ""));
                }
                return sectionStatusesResponse;
            }
        }

        [TestClass]
        public class StudentClassification_GET
        {
            Mock<IColleagueTransactionFactory> transFactoryMock;
            Mock<ICacheProvider> cacheProviderMock;
            Mock<IColleagueDataReader> dataAccessorMock;
            Mock<ILogger> loggerMock;
            IEnumerable<Ellucian.Colleague.Domain.Student.Entities.StudentClassification> studentClassiicationEntities;
            string codeItemName;
            ApiSettings apiSettings;
            StudentReferenceDataRepository studentReferenceDataRepo;

            [TestInitialize]
            public void Initialize()
            {
                loggerMock = new Mock<ILogger>();
                apiSettings = new ApiSettings("TEST");
               

                // Build repository
                studentReferenceDataRepo = BuildValidReferenceDataRepository();
                codeItemName = studentReferenceDataRepo.BuildFullCacheKey("AllStudentClassifications");
            }

            [TestCleanup]
            public void Cleanup()
            {
                transFactoryMock = null;
                dataAccessorMock = null;
                cacheProviderMock = null;
                studentClassiicationEntities = null;
                studentReferenceDataRepo = null;
            }

            [TestMethod]
            public async Task StudentReferenceDataRepo_GetAllStudentClassificationAsync()
            {
                var actuals = await studentReferenceDataRepo.GetAllStudentClassificationAsync(It.IsAny<bool>());
                Assert.IsNotNull(actuals);

                foreach (var actual in actuals)
                {
                    var expected = studentClassiicationEntities.FirstOrDefault(i => i.Guid.Equals(actual.Guid, StringComparison.OrdinalIgnoreCase));
                    Assert.IsNotNull(expected);

                    Assert.AreEqual(expected.Code, actual.Code);
                    Assert.AreEqual(expected.Description, actual.Description);
                    Assert.AreEqual(expected.Guid, actual.Guid);
                }                
            }

            [TestMethod]
            public async Task StudentReferenceDataRepo_GetAllStudentClassification_Cache()
            {
                var actuals = await studentReferenceDataRepo.GetAllStudentClassificationAsync(false);
                Assert.IsNotNull(actuals);

                foreach (var actual in actuals)
                {
                    var expected = studentClassiicationEntities.FirstOrDefault(i => i.Guid.Equals(actual.Guid, StringComparison.OrdinalIgnoreCase));
                    Assert.IsNotNull(expected);

                    Assert.AreEqual(expected.Code, actual.Code);
                    Assert.AreEqual(expected.Description, actual.Description);
                    Assert.AreEqual(expected.Guid, actual.Guid);
                }
            }

            [TestMethod]
            public async Task StudentReferenceDataRepo_GetAllStudentClassification_Noncache()
            {
                var actuals = await studentReferenceDataRepo.GetAllStudentClassificationAsync(true);
                Assert.IsNotNull(actuals);

                foreach (var actual in actuals)
                {
                    var expected = studentClassiicationEntities.FirstOrDefault(i => i.Guid.Equals(actual.Guid, StringComparison.OrdinalIgnoreCase));
                    Assert.IsNotNull(expected);

                    Assert.AreEqual(expected.Code, actual.Code);
                    Assert.AreEqual(expected.Description, actual.Description);
                    Assert.AreEqual(expected.Guid, actual.Guid);
                }
            }

            private StudentReferenceDataRepository BuildValidReferenceDataRepository()
            {
                // transaction factory mock
                transFactoryMock = new Mock<IColleagueTransactionFactory>();
                // Cache Mock
                //localCacheMock = new Mock<ObjectCache>();
                // Cache Provider Mock
                cacheProviderMock = new Mock<ICacheProvider>();
                // Set up data accessor for mocking 
                dataAccessorMock = new Mock<IColleagueDataReader>();
                apiSettings = new ApiSettings("TEST");

                // Set up dataAccessorMock as the object for the DataAccessor
                transFactoryMock.Setup(transFac => transFac.GetDataReader()).Returns(dataAccessorMock.Object);

                studentClassiicationEntities = new List<StudentClassification>() 
                {
                    new StudentClassification("3b8f02a3-d349-46b5-a0df-710121fa1f64", "1G", "First Year Graduate"),
                    new StudentClassification("7b8c4ba7-ea28-4604-bca7-da7223f6e2b3", "1L", "First Year Law"),
                    new StudentClassification("bd98c3ed-6adb-4c7c-bc80-7507ea868a23", "2A", "Second Year"),
                    new StudentClassification("6eea82bc-c3f4-45c0-b0ef-a8f25b89ee31", "2G", "Second Year Graduate"),
                    new StudentClassification("7e990bda-9427-4de6-b0ef-bba9b015e399", "2L", "Second Year Law"),
                };


                var records = new Collection<DataContracts.Classes>();
                foreach (var item in studentClassiicationEntities)
                {
                    DataContracts.Classes record = new DataContracts.Classes();
                    record.RecordGuid = item.Guid;
                    record.ClsDesc = item.Description;
                    record.Recordkey = item.Code;
                    records.Add(record);
                }
                dataAccessorMock.Setup(acc => acc.BulkReadRecordAsync<DataContracts.Classes>("CLASSES", "", true)).ReturnsAsync(records);

                cacheProviderMock.Setup<Task<Tuple<object, SemaphoreSlim>>>(x =>
                 x.GetAndLockSemaphoreAsync(It.IsAny<string>(), null))
                 .ReturnsAsync(new Tuple<object, SemaphoreSlim>(null, new SemaphoreSlim(1, 1)));

                dataAccessorMock.Setup(acc => acc.SelectAsync(It.IsAny<RecordKeyLookup[]>())).Returns<RecordKeyLookup[]>(recordKeyLookups =>
                {
                    var result = new Dictionary<string, RecordKeyLookupResult>();
                    foreach (var recordKeyLookup in recordKeyLookups)
                    {
                        var studentClassiications = studentClassiicationEntities.Where(e => e.Code == recordKeyLookup.PrimaryKey).FirstOrDefault();
                        result.Add(string.Join("+", new string[] { "CLASSES", studentClassiications.Code }),
                            new RecordKeyLookupResult() { Guid = studentClassiications.Guid });
                    }
                    return Task.FromResult(result);
                });

                // Construct repository
                studentReferenceDataRepo = new StudentReferenceDataRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object, apiSettings);

                return studentReferenceDataRepo;
            }
        }

        [TestClass]
        public class StudentCohort_GET
        {
            Mock<IColleagueTransactionFactory> transFactoryMock;
            Mock<ICacheProvider> cacheProviderMock;
            Mock<IColleagueDataReader> dataAccessorMock;
            Mock<ILogger> loggerMock;

            StudentReferenceDataRepository studentSeferenceDataRepo;
            List<StudentCohort> studentCohortEntities;
            ApplValcodes studentCohortsValcodeResponse;
            ApiSettings apiSettings;


            [TestInitialize]
            public void Initialize()
            {
                loggerMock = new Mock<ILogger>();
                apiSettings = new ApiSettings("TEST");

                BuildData();
                studentCohortsValcodeResponse = BuildValcodeResponse(studentCohortEntities);
                studentSeferenceDataRepo = BuildValidReferenceDataRepository();
            }

            private void BuildData()
            {
                studentCohortEntities = new List<StudentCohort>() 
                {
                    new StudentCohort("e8dbcea5-ffb8-471e-87b7-ce5d36d5c2e7", "ATHL", "Athletes"),
                    new StudentCohort("c2f57ee5-1c30-44a5-9d18-311f71f7b722", "FRAT", "Fraternity"),
                    new StudentCohort("f05a6c0f-3a56-4a87-b931-bc2901da5ef9", "SORO", "Sorority"),
                    new StudentCohort("05872218-f749-4cdc-b4f0-43200cc21335", "ROTC", "ROTC Participants"),
                    new StudentCohort("827fffc4-3dd2-4492-8f51-4134597ec4bf", "VETS", "Military Veterans"),
                };
            }

            [TestCleanup]
            public void Cleanup()
            {
                studentSeferenceDataRepo = null;
                studentCohortEntities = null;
                studentCohortsValcodeResponse = null;
                apiSettings = null;
            }

            [TestMethod]
            public async Task StudentCohort_GET_AnyCache()
            {
                var actuals = await studentSeferenceDataRepo.GetAllStudentCohortAsync(It.IsAny<bool>());
                Assert.IsNotNull(actuals);

                foreach (var actual in actuals)
                {
                    var expected = studentCohortEntities.FirstOrDefault(i => i.Guid.Equals(actual.Guid, StringComparison.OrdinalIgnoreCase));
                    Assert.IsNotNull(expected);

                    Assert.AreEqual(expected.Guid, actual.Guid);
                    Assert.AreEqual(expected.Code, actual.Code);
                    Assert.AreEqual(expected.Description, actual.Description);
                }
            }

            private StudentReferenceDataRepository BuildValidReferenceDataRepository()
            {
                // transaction factory mock
                transFactoryMock = new Mock<IColleagueTransactionFactory>();
                // Cache Provider Mock
                cacheProviderMock = new Mock<ICacheProvider>();
                // Set up data accessor for mocking 
                dataAccessorMock = new Mock<IColleagueDataReader>();

                // Set up dataAccessorMock as the object for the DataAccessor
                transFactoryMock.Setup(transFac => transFac.GetDataReader()).Returns(dataAccessorMock.Object);

                // Setup response to courseLevel valcode read
                dataAccessorMock.Setup(acc => acc.ReadRecordAsync<ApplValcodes>("ST.VALCODES", "INSTITUTION.COHORTS", It.IsAny<bool>())).ReturnsAsync(studentCohortsValcodeResponse);
                cacheProviderMock.Setup<Task<Tuple<object, SemaphoreSlim>>>(x => x.GetAndLockSemaphoreAsync(It.IsAny<string>(), null))
                .ReturnsAsync(new Tuple<object, SemaphoreSlim>(null, new SemaphoreSlim(1, 1)));

                dataAccessorMock.Setup(acc => acc.SelectAsync(It.IsAny<RecordKeyLookup[]>())).Returns<RecordKeyLookup[]>(recordKeyLookups =>
                {
                    var result = new Dictionary<string, RecordKeyLookupResult>();
                    foreach (var recordKeyLookup in recordKeyLookups)
                    {
                        var courseLevel = studentCohortEntities.Where(e => e.Code == recordKeyLookup.SecondaryKey).FirstOrDefault();
                        result.Add(string.Join("+", new string[] { "ST.VALCODES", "INSTITUTION.COHORTS", courseLevel.Code }),
                            new RecordKeyLookupResult() { Guid = courseLevel.Guid });
                    }
                    return Task.FromResult(result);
                });

                // Construct repository
                studentSeferenceDataRepo = new StudentReferenceDataRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object, apiSettings);

                return studentSeferenceDataRepo;
            }

            private ApplValcodes BuildValcodeResponse(IEnumerable<StudentCohort> studentCohort)
            {
                ApplValcodes studentCohortResponse = new ApplValcodes();
                studentCohortResponse.ValsEntityAssociation = new List<ApplValcodesVals>();
                foreach (var item in studentCohort)
                {
                    studentCohortResponse.ValsEntityAssociation.Add(new ApplValcodesVals("", item.Description, "2", item.Code, "3", "", ""));
                }
                return studentCohortResponse;
            }
        }

        [TestClass]
        public class StudentLoad
        {
            Mock<IColleagueTransactionFactory> transFactoryMock;
            //Mock<ObjectCache> localCacheMock;
            Mock<ICacheProvider> cacheProviderMock;
            Mock<IColleagueDataReader> dataAccessorMock;
            Mock<ILogger> loggerMock;
            IEnumerable<Ellucian.Colleague.Domain.Student.Entities.StudentLoad> allStudentLoads;
            ApplValcodes studentLoadsValcodeResponse;
            string valcodeName;
            ApiSettings apiSettings;
            StudentReferenceDataRepository referenceDataRepo;

            [TestInitialize]
            public void Initialize()
            {
                loggerMock = new Mock<ILogger>();

                // Build responses used for mocking
                allStudentLoads = new TestStudentReferenceDataRepository().GetStudentLoadsAsync().Result;

                studentLoadsValcodeResponse = BuildValcodeResponse(allStudentLoads);

                // Build repository
                referenceDataRepo = BuildValidReferenceDataRepository();
                valcodeName = referenceDataRepo.BuildFullCacheKey("ST_STUDENT.LOADS");

                cacheProviderMock.Setup<Task<Tuple<object, SemaphoreSlim>>>(x =>
                   x.GetAndLockSemaphoreAsync(It.IsAny<string>(), null))
                   .ReturnsAsync(new Tuple<object, SemaphoreSlim>(null, new SemaphoreSlim(1, 1)));

            }

            [TestCleanup]
            public void Cleanup()
            {
                transFactoryMock = null;
                dataAccessorMock = null;
                cacheProviderMock = null;
                studentLoadsValcodeResponse = null;
                allStudentLoads = null;
                referenceDataRepo = null;
            }

            [TestMethod]
            public async Task StudentReferenceDataRepo_GetStudentLoads()
            {
                var repo = await referenceDataRepo.GetStudentLoadsAsync(); ;

                for (int i = 0; i < allStudentLoads.Count(); i++)
                {
                    Assert.AreEqual(allStudentLoads.ElementAt(i).Code, repo.ElementAt(i).Code);
                    Assert.AreEqual(allStudentLoads.ElementAt(i).Description, repo.ElementAt(i).Description);
                }
            }

            [TestMethod]
            public async Task StudentReferenceDataRepo_GetStudentLoads_WritesToCache()
            {
                // Set up local cache mock to respond to cache request:
                //  -to "Contains" request, return "false" to indicate item is not in cache
                //  -to cache "Get" request, return null so we know it's reading from the "repository"
                cacheProviderMock.Setup(x => x.Contains(valcodeName, null)).Returns(false);
                cacheProviderMock.Setup(x => x.Get(valcodeName, null)).Returns(null);

                // return a valid response to the data accessor request
                dataAccessorMock.Setup(acc => acc.ReadRecordAsync<ApplValcodes>(It.IsAny<string>(), It.IsAny<string>(), true)).ReturnsAsync(studentLoadsValcodeResponse);

                // But after data accessor read, set up mocking so we can verify the list of student loads was written to the cache
                cacheProviderMock.Setup(x => x.AddAndUnlockSemaphore(valcodeName, It.IsAny<Object>(), It.IsAny<SemaphoreSlim>(), It.IsAny<CacheItemPolicy>(), null)).Verifiable();

                // Verify that student loads were returned, which means they came from the "repository".
                Assert.IsTrue((await referenceDataRepo.GetStudentLoadsAsync()).Count() == allStudentLoads.Count());

                // Verify that the student loads was added to the cache after it was read from the repository
                cacheProviderMock.Verify(m => m.AddAndUnlockSemaphore(valcodeName, It.IsAny<Object>(), It.IsAny<SemaphoreSlim>(), It.IsAny<CacheItemPolicy>(), null));
            }

            private T GetCache<T>(ICacheProvider cacheProvider, string key)
                where T : class
            {
                object cache = cacheProvider.Get(key, null);
                return cache as T;
            }

            private StudentReferenceDataRepository BuildValidReferenceDataRepository()
            {
                // transaction factory mock
                transFactoryMock = new Mock<IColleagueTransactionFactory>();
                // Cache Mock
                //localCacheMock = new Mock<ObjectCache>();
                // Cache Provider Mock
                cacheProviderMock = new Mock<ICacheProvider>();
                // Set up data accessor for mocking 
                dataAccessorMock = new Mock<IColleagueDataReader>();
                apiSettings = new ApiSettings("TEST");

                // Set up dataAccessorMock as the object for the DataAccessor
                transFactoryMock.Setup(transFac => transFac.GetDataReader()).Returns(dataAccessorMock.Object);

                // Setup response to student loads valcode read
                dataAccessorMock.Setup(acc => acc.ReadRecordAsync<ApplValcodes>("ST.VALCODES", "STUDENT.LOADS", true)).ReturnsAsync(studentLoadsValcodeResponse);

                // Construct repository
                referenceDataRepo = new StudentReferenceDataRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object, apiSettings);

                return referenceDataRepo;
            }

            private ApplValcodes BuildValcodeResponse(IEnumerable<Ellucian.Colleague.Domain.Student.Entities.StudentLoad> studentLoad)
            {
                ApplValcodes applValcodeResponse = new ApplValcodes();
                applValcodeResponse.ValsEntityAssociation = new List<ApplValcodesVals>();
                foreach (var item in studentLoad)
                {
                    applValcodeResponse.ValsEntityAssociation.Add(new ApplValcodesVals("", item.Description, "", item.Code, "", "", ""));
                }
                return applValcodeResponse;
            }
        }

        [TestClass]
        public class StudentPetitionReasons_Get
        {
            Mock<IColleagueTransactionFactory> transFactoryMock;
            Mock<IColleagueDataReader> dataAccessorMock;
            Mock<ICacheProvider> cacheProviderMock;
            Mock<ILogger> loggerMock;
            ApiSettings apiSettings;
            ApplValcodes studentPetitionReasonsValcodeResponse;
            StudentReferenceDataRepository referenceDataRepo;

            [TestInitialize]
            public void Initialize()
            {
                loggerMock = new Mock<ILogger>();
                apiSettings = new ApiSettings("TEST");

                studentPetitionReasonsValcodeResponse = BuildStudentPetitionReasons();
                referenceDataRepo = BuildValidReferenceDataRepository();

                cacheProviderMock.Setup<Task<Tuple<object, SemaphoreSlim>>>(x =>
                   x.GetAndLockSemaphoreAsync(It.IsAny<string>(), null))
                   .ReturnsAsync(new Tuple<object, SemaphoreSlim>(null, new SemaphoreSlim(1, 1)));

            }

            [TestMethod]
            public async Task StudentReferenceDataRepo_GetStudentPetitionReasons()
            {
                var petitionReasons = await referenceDataRepo.GetStudentPetitionReasonsAsync();
                Assert.AreEqual(studentPetitionReasonsValcodeResponse.ValsEntityAssociation.Count(), petitionReasons.Count());
                foreach (var item in studentPetitionReasonsValcodeResponse.ValsEntityAssociation)
                {
                    var entry = petitionReasons.Where(wr => wr.Code == item.ValInternalCodeAssocMember).FirstOrDefault();
                    Assert.AreEqual(item.ValExternalRepresentationAssocMember, entry.Description);
                }
            }

            private StudentReferenceDataRepository BuildValidReferenceDataRepository()
            {
                transFactoryMock = new Mock<IColleagueTransactionFactory>();
                cacheProviderMock = new Mock<ICacheProvider>();
                dataAccessorMock = new Mock<IColleagueDataReader>();
                transFactoryMock.Setup(transFac => transFac.GetDataReader()).Returns(dataAccessorMock.Object);

                // Setup response to valcode read
                dataAccessorMock.Setup(acc => acc.ReadRecordAsync<ApplValcodes>("ST.VALCODES", "STUDENT.PETITIONS.REASON.CODES", It.IsAny<bool>())).ReturnsAsync(studentPetitionReasonsValcodeResponse);

                // Construct repository
                referenceDataRepo = new StudentReferenceDataRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object, apiSettings);

                return referenceDataRepo;
            }

            public ApplValcodes BuildStudentPetitionReasons()
            {
                var reasons = new ApplValcodes() { Recordkey = "STUDENT.PETITIONS.REASON.CODES", ValNoMod = "N" };
                reasons.ValsEntityAssociation = new List<ApplValcodesVals>();
                reasons.ValsEntityAssociation.Add(new ApplValcodesVals("I", "I can handle it", "", "ICJI", "", "", ""));
                reasons.ValsEntityAssociation.Add(new ApplValcodesVals("O", "Over my head", "", "OVMH", "", "", ""));
                return reasons;
            }
        }

        [TestClass]
        public class StudentStatus
        {
            Mock<IColleagueTransactionFactory> transFactoryMock;
            Mock<ICacheProvider> cacheProviderMock;
            Mock<IColleagueDataReader> dataAccessorMock;
            Mock<ILogger> loggerMock;

            StudentReferenceDataRepository studentreferenceDataRepo;
            List<Ellucian.Colleague.Domain.Student.Entities.StudentStatus> studentStatusEntities;
            ApplValcodes studentStatusesValcodeResponse;
            ApiSettings apiSettings;


            [TestInitialize]
            public void Initialize()
            {
                loggerMock = new Mock<ILogger>();
                apiSettings = new ApiSettings("TEST");

                BuildData();
                studentStatusesValcodeResponse = BuildValcodeResponse(studentStatusEntities);
                studentreferenceDataRepo = BuildValidReferenceDataRepository();
            }

            private void BuildData()
            {
                studentStatusEntities = new List<Ellucian.Colleague.Domain.Student.Entities.StudentStatus>() 
                {
                    new Ellucian.Colleague.Domain.Student.Entities.StudentStatus("e8dbcea5-ffb8-471e-87b7-ce5d36d5c2e7", "ATHL", "Athletes"),
                    new Ellucian.Colleague.Domain.Student.Entities.StudentStatus("c2f57ee5-1c30-44a5-9d18-311f71f7b722", "FRAT", "Fraternity"),
                    new Ellucian.Colleague.Domain.Student.Entities.StudentStatus("f05a6c0f-3a56-4a87-b931-bc2901da5ef9", "SORO", "Sorority"),
                    new Ellucian.Colleague.Domain.Student.Entities.StudentStatus("05872218-f749-4cdc-b4f0-43200cc21335", "ROTC", "ROTC Participants"),
                    new Ellucian.Colleague.Domain.Student.Entities.StudentStatus("827fffc4-3dd2-4492-8f51-4134597ec4bf", "VETS", "Military Veterans"),
                };
            }

            [TestCleanup]
            public void Cleanup()
            {
                studentreferenceDataRepo = null;
                studentStatusEntities = null;
                studentStatusesValcodeResponse = null;
                apiSettings = null;
            }

            [TestMethod]
            public async Task StudentStatus_GET_AnyCache()
            {
                var actuals = await studentreferenceDataRepo.GetStudentStatusesAsync(It.IsAny<bool>());
                Assert.IsNotNull(actuals);

                foreach (var actual in actuals)
                {
                    var expected = studentStatusEntities.FirstOrDefault(i => i.Guid.Equals(actual.Guid, StringComparison.OrdinalIgnoreCase));
                    Assert.IsNotNull(expected);

                    Assert.AreEqual(expected.Guid, actual.Guid);
                    Assert.AreEqual(expected.Code, actual.Code);
                    Assert.AreEqual(expected.Description, actual.Description);
                }
            }

            private StudentReferenceDataRepository BuildValidReferenceDataRepository()
            {
                // transaction factory mock
                transFactoryMock = new Mock<IColleagueTransactionFactory>();
                // Cache Provider Mock
                cacheProviderMock = new Mock<ICacheProvider>();
                // Set up data accessor for mocking 
                dataAccessorMock = new Mock<IColleagueDataReader>();

                // Set up dataAccessorMock as the object for the DataAccessor
                transFactoryMock.Setup(transFac => transFac.GetDataReader()).Returns(dataAccessorMock.Object);

                // Setup response to student status valcode read
                dataAccessorMock.Setup(ss => ss.ReadRecordAsync<ApplValcodes>("ST.VALCODES", "STUDENT.TERM.STATUSES", It.IsAny<bool>())).ReturnsAsync(studentStatusesValcodeResponse);
                cacheProviderMock.Setup<Task<Tuple<object, SemaphoreSlim>>>(x => x.GetAndLockSemaphoreAsync(It.IsAny<string>(), null))
                .ReturnsAsync(new Tuple<object, SemaphoreSlim>(null, new SemaphoreSlim(1, 1)));

                dataAccessorMock.Setup(ss => ss.SelectAsync(It.IsAny<RecordKeyLookup[]>())).Returns<RecordKeyLookup[]>(recordKeyLookups =>
                {
                    var result = new Dictionary<string, RecordKeyLookupResult>();
                    foreach (var recordKeyLookup in recordKeyLookups)
                    {
                        var studentStatus = studentStatusEntities.Where(e => e.Code == recordKeyLookup.SecondaryKey).FirstOrDefault();
                        result.Add(string.Join("+", new string[] { "ST.VALCODES", "STUDENT.TERM.STATUSES", studentStatus.Code }),
                            new RecordKeyLookupResult() { Guid = studentStatus.Guid });
                    }
                    return Task.FromResult(result);
                });

                // Construct repository
                studentreferenceDataRepo = new StudentReferenceDataRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object, apiSettings);

                return studentreferenceDataRepo;
            }

            private ApplValcodes BuildValcodeResponse(IEnumerable<Ellucian.Colleague.Domain.Student.Entities.StudentStatus> studentStatus)
            {
                ApplValcodes studentStatusResponse = new ApplValcodes();
                studentStatusResponse.ValsEntityAssociation = new List<ApplValcodesVals>();
                foreach (var item in studentStatus)
                {
                    studentStatusResponse.ValsEntityAssociation.Add(new ApplValcodesVals("", item.Description, "2", item.Code, "3", "", ""));
                }
                return studentStatusResponse;
            }
        }

        [TestClass]
        public class SapTypes
        {
            Mock<IColleagueTransactionFactory> transFactoryMock;
            Mock<ICacheProvider> cacheProviderMock;
            Mock<IColleagueDataReader> dataAccessorMock;
            Mock<ILogger> loggerMock;

            IEnumerable<Domain.Student.Entities.SapType> sapTypes;
            string valcodeName;
            ApiSettings apiSettings;

            StudentReferenceDataRepository referenceDataRepo;

            [TestInitialize]
            public void Initialize()
            {
                loggerMock = new Mock<ILogger>();
                apiSettings = new ApiSettings("TEST");

                sapTypes = new TestStudentReferenceDataRepository().GetSapTypesAsync(false).Result;

                referenceDataRepo = BuildValidReferenceDataRepository();
                valcodeName = referenceDataRepo.BuildFullCacheKey("AllSapTypes");
            }

            [TestCleanup]
            public void Cleanup()
            {
                sapTypes = null;
                valcodeName = string.Empty;
                apiSettings = null;
            }

            [TestMethod]
            public async Task FinancialAidReferenceDataRepository_GetSapTypesAsync_False()
            {
                var results = await referenceDataRepo.GetSapTypesAsync(false);
                Assert.AreEqual(sapTypes.Count(), results.Count());

                foreach (var sapType in sapTypes)
                {
                    var result = results.FirstOrDefault(i => i.Guid == sapType.Guid);

                    Assert.AreEqual(sapType.Code, result.Code);
                    if (string.IsNullOrWhiteSpace(sapType.Description))
                    {
                        Assert.AreEqual(sapType.Code, result.Code);
                    }
                    else
                    {
                        Assert.AreEqual(sapType.Description, result.Description);
                    }
                    Assert.AreEqual(sapType.Guid, result.Guid);
                }
            }

            [TestMethod]
            public async Task FinancialAidReferenceDataRepository_GetSapTypesAsync_True()
            {
                var results = await referenceDataRepo.GetSapTypesAsync(true);
                Assert.AreEqual(sapTypes.Count(), results.Count());

                foreach (var sapType in sapTypes)
                {
                    var result = results.FirstOrDefault(i => i.Guid == sapType.Guid);

                    Assert.AreEqual(sapType.Code, result.Code);
                    if (string.IsNullOrWhiteSpace(sapType.Description))
                    {
                        Assert.AreEqual(sapType.Code, result.Code);
                    }
                    else
                    {
                        Assert.AreEqual(sapType.Description, result.Description);
                    }
                    Assert.AreEqual(sapType.Guid, result.Guid);
                }
            }

            private StudentReferenceDataRepository BuildValidReferenceDataRepository()
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

                var records = new Collection<DataContracts.SapType>();
                foreach (var item in sapTypes)
                {
                    DataContracts.SapType record = new DataContracts.SapType();
                    record.RecordGuid = item.Guid;
                    record.SaptDesc = item.Description;
                    record.Recordkey = item.Code;
                    records.Add(record);
                }
                dataAccessorMock.Setup(acc => acc.BulkReadRecordAsync<DataContracts.SapType>("SAP.TYPE", "", true)).ReturnsAsync(records);

                cacheProviderMock.Setup<Task<Tuple<object, SemaphoreSlim>>>(x =>
                 x.GetAndLockSemaphoreAsync(It.IsAny<string>(), null))
                 .ReturnsAsync(new Tuple<object, SemaphoreSlim>(null, new SemaphoreSlim(1, 1)));

                dataAccessorMock.Setup(acc => acc.SelectAsync(It.IsAny<RecordKeyLookup[]>())).Returns<RecordKeyLookup[]>(recordKeyLookups =>
                {
                    var result = new Dictionary<string, RecordKeyLookupResult>();
                    foreach (var recordKeyLookup in recordKeyLookups)
                    {
                        var record = sapTypes.Where(e => e.Code == recordKeyLookup.PrimaryKey).FirstOrDefault();
                        result.Add(string.Join("+", new string[] { "SAP.TYPE", record.Code }),
                            new RecordKeyLookupResult() { Guid = record.Guid });
                    }
                    return Task.FromResult(result);
                });

                // Construct repository
                referenceDataRepo = new StudentReferenceDataRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object, apiSettings);

                return referenceDataRepo;
            }
        }

        [TestClass]
        public class StudentTypes
        {
            Mock<IColleagueTransactionFactory> transFactoryMock;
            Mock<ICacheProvider> cacheProviderMock;
            Mock<IColleagueDataReader> dataAccessorMock;
            Mock<ILogger> loggerMock;

            IEnumerable<StudentType> allStudentTypes;
            string valcodeName;
            ApiSettings apiSettings;

            StudentReferenceDataRepository referenceDataRepo;

            [TestInitialize]
            public void Initialize()
            {
                loggerMock = new Mock<ILogger>();
                apiSettings = new ApiSettings("TEST");

                allStudentTypes = new TestStudentReferenceDataRepository().GetStudentTypesAsync(false).Result;

                referenceDataRepo = BuildValidReferenceDataRepository();
                valcodeName = referenceDataRepo.BuildFullCacheKey("AllStudentTypes");
            }

            [TestCleanup]
            public void Cleanup()
            {
                allStudentTypes = null;
                valcodeName = string.Empty;
                apiSettings = null;
            }

            [TestMethod]
            public async Task StudentReferenceDataRepository_GetStudentTypesAsync_False()
            {
                var results = await referenceDataRepo.GetStudentTypesAsync(false);
                Assert.AreEqual(allStudentTypes.Count(), results.Count());

                foreach (var studentType in allStudentTypes)
                {
                    var result = results.FirstOrDefault(i => i.Guid == studentType.Guid);

                    Assert.AreEqual(studentType.Code, result.Code);
                    Assert.AreEqual(studentType.Description, result.Description);
                    Assert.AreEqual(studentType.Guid, result.Guid);
                }

            }

            [TestMethod]
            public async Task StudentReferenceDataRepository_GetStudentTypesAsync_True()
            {
                var results = await referenceDataRepo.GetStudentTypesAsync(true);
                Assert.AreEqual(allStudentTypes.Count(), results.Count());

                foreach (var studentType in allStudentTypes)
                {
                    var result = results.FirstOrDefault(i => i.Guid == studentType.Guid);

                    Assert.AreEqual(studentType.Code, result.Code);
                    Assert.AreEqual(studentType.Description, result.Description);
                    Assert.AreEqual(studentType.Guid, result.Guid);
                }

            }

            private StudentReferenceDataRepository BuildValidReferenceDataRepository()
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

                var records = new Collection<DataContracts.StudentTypes>();
                foreach (var item in allStudentTypes)
                {
                    DataContracts.StudentTypes record = new DataContracts.StudentTypes();
                    record.RecordGuid = item.Guid;
                    record.SttDesc = item.Description;
                    record.Recordkey = item.Code;
                    records.Add(record);
                }
                dataAccessorMock.Setup(acc => acc.BulkReadRecordAsync<DataContracts.StudentTypes>("STUDENT.TYPES", "", true)).ReturnsAsync(records);

                cacheProviderMock.Setup<Task<Tuple<object, SemaphoreSlim>>>(x =>
                 x.GetAndLockSemaphoreAsync(It.IsAny<string>(), null))
                 .ReturnsAsync(new Tuple<object, SemaphoreSlim>(null, new SemaphoreSlim(1, 1)));

                dataAccessorMock.Setup(acc => acc.SelectAsync(It.IsAny<RecordKeyLookup[]>())).Returns<RecordKeyLookup[]>(recordKeyLookups =>
                {
                    var result = new Dictionary<string, RecordKeyLookupResult>();
                    foreach (var recordKeyLookup in recordKeyLookups)
                    {
                        var record = allStudentTypes.Where(e => e.Code == recordKeyLookup.PrimaryKey).FirstOrDefault();
                        result.Add(string.Join("+", new string[] { "STUDENT.TYPES", record.Code }),
                            new RecordKeyLookupResult() { Guid = record.Guid });
                    }
                    return Task.FromResult(result);
                });

                // Construct repository
                referenceDataRepo = new StudentReferenceDataRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object, apiSettings);

                return referenceDataRepo;
            }
        }

        [TestClass]
        public class StudentWaiverReasons_Get
        {
            Mock<IColleagueTransactionFactory> transFactoryMock;
            Mock<ICacheProvider> cacheProviderMock;
            Mock<IColleagueDataReader> dataAccessorMock;
            Mock<ILogger> loggerMock;
            ApplValcodes waiverReasonsValcodeResponse;
            ApiSettings apiSettings;

            StudentReferenceDataRepository referenceDataRepo;

            [TestInitialize]
            public void Initialize()
            {
                loggerMock = new Mock<ILogger>();
                apiSettings = new ApiSettings("TEST");

                waiverReasonsValcodeResponse = BuildWaiverReasons();
                referenceDataRepo = BuildValidReferenceDataRepository();

                cacheProviderMock.Setup<Task<Tuple<object, SemaphoreSlim>>>(x =>
                   x.GetAndLockSemaphoreAsync(It.IsAny<string>(), null))
                   .ReturnsAsync(new Tuple<object, SemaphoreSlim>(null, new SemaphoreSlim(1, 1)));

            }

            [TestMethod]
            public async Task StudentReferenceDataRepo_GetsWaiverReasons()
            {
                var waiverReasons = await referenceDataRepo.GetStudentWaiverReasonsAsync();
                Assert.AreEqual(waiverReasonsValcodeResponse.ValsEntityAssociation.Count(), waiverReasons.Count());
                foreach (var item in waiverReasonsValcodeResponse.ValsEntityAssociation)
                {
                    var entry = waiverReasons.Where(wr => wr.Code == item.ValInternalCodeAssocMember).FirstOrDefault();
                    Assert.AreEqual(item.ValExternalRepresentationAssocMember, entry.Description);
                }
            }

            private StudentReferenceDataRepository BuildValidReferenceDataRepository()
            {
                transFactoryMock = new Mock<IColleagueTransactionFactory>();
                cacheProviderMock = new Mock<ICacheProvider>();
                dataAccessorMock = new Mock<IColleagueDataReader>();
                transFactoryMock.Setup(transFac => transFac.GetDataReader()).Returns(dataAccessorMock.Object);

                // Setup response to valcode read
                dataAccessorMock.Setup(acc => acc.ReadRecordAsync<ApplValcodes>("ST.VALCODES", "REQUISITE.WAIVER.REASONS", It.IsAny<bool>())).ReturnsAsync(waiverReasonsValcodeResponse);

                // Construct repository
                referenceDataRepo = new StudentReferenceDataRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object, apiSettings);

                return referenceDataRepo;
            }

            public ApplValcodes BuildWaiverReasons()
            {
                var reasons = new ApplValcodes() { Recordkey = "REQUISITE.WAIVER.REASONS", ValNoMod = "N" };
                reasons.ValsEntityAssociation = new List<ApplValcodesVals>();
                reasons.ValsEntityAssociation.Add(new ApplValcodesVals("L", "Life Learning", "", "LIFE", "", "", ""));
                reasons.ValsEntityAssociation.Add(new ApplValcodesVals("O", "Other", "Y", "OTHER", "", "", ""));
                return reasons;
            }
        }
      
        [TestClass]
        public class SubjectTests
        {
            Mock<IColleagueTransactionFactory> transFactoryMock;
            Mock<ICacheProvider> cacheProviderMock;
            Mock<IColleagueDataReader> dataAccessorMock;
            Mock<ILogger> loggerMock;
            IEnumerable<Subject> allSubjects;
            string codeItemName;
            ApiSettings apiSettings;
            StudentReferenceDataRepository referenceDataRepo;
           
            [TestInitialize]
            public void Initialize()
            {
                loggerMock = new Mock<ILogger>();
                apiSettings = new ApiSettings("TEST");
                // Build responses used for mocking
                allSubjects = new TestSubjectRepository().Get();
                         
                // Build repository
                referenceDataRepo = BuildValidReferenceDataRepository();
                codeItemName = referenceDataRepo.BuildFullCacheKey("AllSubjects");
            }

            [TestCleanup]
            public void Cleanup()
            {
                transFactoryMock = null;
                dataAccessorMock = null;
                cacheProviderMock = null;
                allSubjects = null;
                referenceDataRepo = null;
            }

            [TestMethod]
            public async Task StudentReferenceDataRepo_GetSubject()
            {
                var repoSubjects = await referenceDataRepo.GetSubjectsAsync();
                for (int i = 0; i < allSubjects.Count(); i++)
                {
                    Assert.AreEqual(allSubjects.ElementAt(i).Code, repoSubjects.ElementAt(i).Code);
                    Assert.AreEqual(allSubjects.ElementAt(i).Description, repoSubjects.ElementAt(i).Description);
                }
            }

            [TestMethod]
            public async Task StudentReferenceDataRepo_GetSubject_Cache()
            {
                var repoSubjects = await referenceDataRepo.GetSubjectsAsync(false);
                for (int i = 0; i < allSubjects.Count(); i++)
                {
                    Assert.AreEqual(allSubjects.ElementAt(i).Code, repoSubjects.ElementAt(i).Code);
                    Assert.AreEqual(allSubjects.ElementAt(i).Description, repoSubjects.ElementAt(i).Description);
                }
            }

            [TestMethod]
            public async Task StudentReferenceDataRepo_GetSubject_Noncache()
            {
                var repoSubjects = await referenceDataRepo.GetSubjectsAsync(true);
                for (int i = 0; i < allSubjects.Count(); i++)
                {
                    Assert.AreEqual(allSubjects.ElementAt(i).Code, repoSubjects.ElementAt(i).Code);
                    Assert.AreEqual(allSubjects.ElementAt(i).Description, repoSubjects.ElementAt(i).Description);
                }
            }

            private StudentReferenceDataRepository BuildValidReferenceDataRepository()
            {
                // transaction factory mock
                transFactoryMock = new Mock<IColleagueTransactionFactory>();
                // Cache Mock
                //localCacheMock = new Mock<ObjectCache>();
                // Cache Provider Mock
                cacheProviderMock = new Mock<ICacheProvider>();
                // Set up data accessor for mocking 
                dataAccessorMock = new Mock<IColleagueDataReader>();
                apiSettings = new ApiSettings("TEST");

               // Set up dataAccessorMock as the object for the DataAccessor
                transFactoryMock.Setup(transFac => transFac.GetDataReader()).Returns(dataAccessorMock.Object);
               
                var records = new Collection<DataContracts.Subjects>();
                foreach (var item in allSubjects)
                {
                     DataContracts.Subjects record = new DataContracts.Subjects();
                     record.RecordGuid = item.Guid;
                     record.SubjDesc = item.Description;
                     record.Recordkey = item.Code;
                     records.Add(record);
                }
                dataAccessorMock.Setup(acc => acc.BulkReadRecordAsync<DataContracts.Subjects>("SUBJECTS", "", true)).ReturnsAsync(records);

                cacheProviderMock.Setup<Task<Tuple<object, SemaphoreSlim>>>(x =>
                 x.GetAndLockSemaphoreAsync(It.IsAny<string>(), null))
                 .ReturnsAsync(new Tuple<object, SemaphoreSlim>(null, new SemaphoreSlim(1, 1)));
                         
                dataAccessorMock.Setup(acc => acc.SelectAsync(It.IsAny<RecordKeyLookup[]>())).Returns<RecordKeyLookup[]>(recordKeyLookups =>
                {
                    var result = new Dictionary<string, RecordKeyLookupResult>();
                    foreach (var recordKeyLookup in recordKeyLookups)
                    {
                        var subjects = allSubjects.Where(e => e.Code == recordKeyLookup.PrimaryKey).FirstOrDefault();
                        result.Add(string.Join("+", new string[] { "SUBJECTS", subjects.Code }),
                            new RecordKeyLookupResult() { Guid = subjects.Guid });
                    }
                    return Task.FromResult(result);
                });
           
                // Construct repository
                referenceDataRepo = new StudentReferenceDataRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object, apiSettings);

                return referenceDataRepo;
            }
        }

        [TestClass]
        public class TestSources
        {
            Mock<IColleagueTransactionFactory> transFactoryMock;
            Mock<ICacheProvider> cacheProviderMock;
            Mock<IColleagueDataReader> dataAccessorMock;
            Mock<ILogger> loggerMock;
            IEnumerable<TestSource> allTestSources;
            ApplValcodes testSourceValcodeResponse;
            string valcodeName;
            ApiSettings apiSettings;

            Mock<IStudentReferenceDataRepository> referenceDataRepositoryMock;
            IStudentReferenceDataRepository referenceDataRepository;
            StudentReferenceDataRepository referenceDataRepo;

            [TestInitialize]
            public void Initialize()
            {
                loggerMock = new Mock<ILogger>();
                apiSettings = new ApiSettings("TEST");

                // Build Courses responses used for mocking
                allTestSources = new TestStudentReferenceDataRepository().GetTestSourcesAsync(false).Result;
                testSourceValcodeResponse = BuildValcodeResponse(allTestSources);
                var testSourceValResponse = new List<string>() { "2" };
                testSourceValcodeResponse.ValActionCode1 = testSourceValResponse;

                referenceDataRepositoryMock = new Mock<IStudentReferenceDataRepository>();
                referenceDataRepository = referenceDataRepositoryMock.Object;

                // Build course level repository
                referenceDataRepo = BuildValidReferenceDataRepository();
                valcodeName = referenceDataRepo.BuildFullCacheKey("ST_APPL.TEST.SOURCES_GUID");

                cacheProviderMock.Setup<Task<Tuple<object, SemaphoreSlim>>>(x =>
                   x.GetAndLockSemaphoreAsync(It.IsAny<string>(), null))
                   .ReturnsAsync(new Tuple<object, SemaphoreSlim>(null, new SemaphoreSlim(1, 1)));
            }

            [TestCleanup]
            public void Cleanup()
            {
                transFactoryMock = null;
                dataAccessorMock = null;
                cacheProviderMock = null;
                testSourceValcodeResponse = null;
                allTestSources = null;
                referenceDataRepo = null;
            }

           
            [TestMethod]
            public async Task StudentReferenceDataRepo_GetsTestSourcesCacheAsync()
            {
                var testSources = await referenceDataRepo.GetTestSourcesAsync(false);

                for (int i = 0; i < allTestSources.Count(); i++)
                {
                    Assert.AreEqual(allTestSources.ElementAt(i).Code, testSources.ElementAt(i).Code);
                    Assert.AreEqual(allTestSources.ElementAt(i).Description, testSources.ElementAt(i).Description);
                }
            }

            [TestMethod]
            public async Task StudentReferenceDataRepo_GetsTestSourcesNonCacheAsync()
            {
                var statuses = await referenceDataRepo.GetTestSourcesAsync(true);

                for (int i = 0; i < allTestSources.Count(); i++)
                {
                    Assert.AreEqual(allTestSources.ElementAt(i).Code, statuses.ElementAt(i).Code);
                    Assert.AreEqual(allTestSources.ElementAt(i).Description, statuses.ElementAt(i).Description);
                }
            }

            [TestMethod]
            public async Task StudentReferenceDataRepo_GetTestSources_WritesToCacheAsync()
            {

                // Set up local cache mock to respond to cache request:
                //  -to "Contains" request, return "false" to indicate item is not in cache
                //  -to cache "Get" request, return null so we know it's reading from the "repository"
                cacheProviderMock.Setup(x => x.Contains(valcodeName, null)).Returns(false);
                cacheProviderMock.Setup(x => x.Get(valcodeName, null)).Returns(null);

                // return a valid response to the data accessor request
                dataAccessorMock.Setup(acc => acc.ReadRecordAsync<ApplValcodes>("ST.VALCODES", "APPL.TEST.SOURCES", It.IsAny<bool>())).ReturnsAsync(testSourceValcodeResponse);

                cacheProviderMock.Setup<Task<Tuple<object, SemaphoreSlim>>>(x =>
                 x.GetAndLockSemaphoreAsync(It.IsAny<string>(), null))
                 .ReturnsAsync(new Tuple<object, SemaphoreSlim>(null, new SemaphoreSlim(1, 1)));

                // But after data accessor read, set up mocking so we can verify the list of testSources was written to the cache
                cacheProviderMock.Setup(x => x.Add(It.IsAny<string>(), It.IsAny<Task<List<TestSource>>>(), It.IsAny<CacheItemPolicy>(), null)).Verifiable();

                cacheProviderMock.Setup(x => x.Contains(referenceDataRepo.BuildFullCacheKey("ST_APPL.TEST.SOURCES"), null)).Returns(true);
                var testSources = await referenceDataRepo.GetTestSourcesAsync(false);
                cacheProviderMock.Setup(x => x.Get(referenceDataRepo.BuildFullCacheKey("ST_APPL.TEST.SOURCES"), null)).Returns(testSources);
                // Verify that testSources were returned, which means they came from the "repository".
                Assert.IsTrue(testSources.Count() == allTestSources.Count());

                // Verify that the testSource item was added to the cache after it was read from the repository
                cacheProviderMock.Verify(m => m.Add(It.IsAny<string>(), It.IsAny<Task<List<TestSource>>>(), It.IsAny<CacheItemPolicy>(), null), Times.Never);

            }

            [TestMethod]
            public async Task StudentReferenceDataRepo_GetTestSources_GetsCachedTestSourcesAsync()
            {
                // Set up local cache mock to respond to cache request:
                //  -to "Contains" request, return "true" to indicate item is in cache
                //  -to "Get" request, return the cache item (in this case the "COURSE.LEVELS" cache item)
                cacheProviderMock.Setup(x => x.Contains(valcodeName, null)).Returns(true);
                cacheProviderMock.Setup(x => x.Get(valcodeName, null)).Returns(allTestSources).Verifiable();

                // return null for request, so that if we have a result, it wasn't the data accessor that returned it.
                dataAccessorMock.Setup(acc => acc.ReadRecordAsync<ApplValcodes>("ST.VALCODES", "APPL.TEST.SOURCES", true)).ReturnsAsync(new ApplValcodes());

                // Assert the testSources are returned
                Assert.IsTrue((await referenceDataRepo.GetTestSourcesAsync(false)).Count() == allTestSources.Count());
                // Verify that the sectionRegistrationStatuses were retrieved from cache
                cacheProviderMock.Verify(m => m.Get(valcodeName, null));
            }

            private StudentReferenceDataRepository BuildValidReferenceDataRepository()
            {
                // transaction factory mock
                transFactoryMock = new Mock<IColleagueTransactionFactory>();
                // Cache Provider Mock
                cacheProviderMock = new Mock<ICacheProvider>();
                // Set up data accessor for mocking 
                dataAccessorMock = new Mock<IColleagueDataReader>();

                // Set up dataAccessorMock as the object for the DataAccessor
                transFactoryMock.Setup(transFac => transFac.GetDataReader()).Returns(dataAccessorMock.Object);

                // Setup response to testSource valcode read
                dataAccessorMock.Setup(acc => acc.ReadRecordAsync<ApplValcodes>("ST.VALCODES", "APPL.TEST.SOURCES", It.IsAny<bool>())).ReturnsAsync(testSourceValcodeResponse);
                cacheProviderMock.Setup<Task<Tuple<object, SemaphoreSlim>>>(x => x.GetAndLockSemaphoreAsync(It.IsAny<string>(), null))
                .ReturnsAsync(new Tuple<object, SemaphoreSlim>(null, new SemaphoreSlim(1, 1)));

                dataAccessorMock.Setup(acc => acc.SelectAsync(It.IsAny<RecordKeyLookup[]>())).Returns<RecordKeyLookup[]>(recordKeyLookups =>
                {
                    var result = new Dictionary<string, RecordKeyLookupResult>();
                    foreach (var recordKeyLookup in recordKeyLookups)
                    {
                        var testSource = allTestSources.Where(e => e.Code == recordKeyLookup.SecondaryKey).FirstOrDefault();
                        result.Add(string.Join("+", new string[] { "ST.VALCODES", "APPL.TEST.SOURCES", testSource.Code }),
                            new RecordKeyLookupResult() { Guid = testSource.Guid });
                    }
                    return Task.FromResult(result);
                });

                // Construct repository
                referenceDataRepo = new StudentReferenceDataRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object, apiSettings);

                return referenceDataRepo;
            }

            private ApplValcodes BuildValcodeResponse(IEnumerable<TestSource> testSources)
            {
                ApplValcodes testSourcesResponse = new ApplValcodes();
                testSourcesResponse.ValsEntityAssociation = new List<ApplValcodesVals>();
                foreach (var item in testSources)
                {
                    testSourcesResponse.ValsEntityAssociation.Add(new ApplValcodesVals("", item.Description, "", item.Code, "", "", ""));
                }
                return testSourcesResponse;
            }
        }

        [TestClass]
        public class TranscriptCategory
        {
            Mock<IColleagueTransactionFactory> transFactoryMock;
            Mock<ICacheProvider> cacheProviderMock;
            Mock<IColleagueDataReader> dataAccessorMock;
            Mock<ILogger> loggerMock;
            IEnumerable<Ellucian.Colleague.Domain.Student.Entities.TranscriptCategory> allTranscriptCategories;
            ApplValcodes transcriptCategoryValcodeResponse;
            string valcodeName;
            ApiSettings apiSettings;
            StudentReferenceDataRepository referenceDataRepo;

            [TestInitialize]
            public void Initialize()
            {
                loggerMock = new Mock<ILogger>();

                // Build responses used for mocking
                allTranscriptCategories = new TestStudentReferenceDataRepository().GetTranscriptCategoriesAsync().Result;

                transcriptCategoryValcodeResponse = BuildValcodeResponse(allTranscriptCategories);

                // Build repository
                referenceDataRepo = BuildValidReferenceDataRepository();
                valcodeName = referenceDataRepo.BuildFullCacheKey("ST_TRANSCRIPT.CATEGORIES");

                cacheProviderMock.Setup<Task<Tuple<object, SemaphoreSlim>>>(x =>
                   x.GetAndLockSemaphoreAsync(It.IsAny<string>(), null))
                   .ReturnsAsync(new Tuple<object, SemaphoreSlim>(null, new SemaphoreSlim(1, 1)));

            }

            [TestCleanup]
            public void Cleanup()
            {
                transFactoryMock = null;
                dataAccessorMock = null;
                cacheProviderMock = null;
                transcriptCategoryValcodeResponse = null;
                allTranscriptCategories = null;
                referenceDataRepo = null;
            }

            [TestMethod]
            public async Task StudentReferenceDataRepo_GetTranscriptCategories()
            {
                var repo = await referenceDataRepo.GetTranscriptCategoriesAsync();

                for (int i = 0; i < allTranscriptCategories.Count(); i++)
                {
                    Assert.AreEqual(allTranscriptCategories.ElementAt(i).Code, repo.ElementAt(i).Code);
                    Assert.AreEqual(allTranscriptCategories.ElementAt(i).Description, repo.ElementAt(i).Description);
                }
            }

            [TestMethod]
            public async Task StudentReferenceDataRepo_GetTranscriptCategories_WritesToCache()
            {
                // Set up local cache mock to respond to cache request:
                //  -to "Contains" request, return "false" to indicate item is not in cache
                //  -to cache "Get" request, return null so we know it's reading from the "repository"
                cacheProviderMock.Setup(x => x.Contains(valcodeName, null)).Returns(false);
                cacheProviderMock.Setup(x => x.Get(valcodeName, null)).Returns(null);

                // return a valid response to the data accessor request
                dataAccessorMock.Setup(acc => acc.ReadRecordAsync<ApplValcodes>(It.IsAny<string>(), It.IsAny<string>(), true)).ReturnsAsync(transcriptCategoryValcodeResponse);

                // But after data accessor read, set up mocking so we can verify the list of TranscriptCategories was written to the cache
                cacheProviderMock.Setup(x => x.AddAndUnlockSemaphore(valcodeName, It.IsAny<Object>(), It.IsAny<SemaphoreSlim>(), It.IsAny<CacheItemPolicy>(), null)).Verifiable();

                // Verify that TranscriptCategories were returned, which means they came from the "repository".
                Assert.IsTrue((await referenceDataRepo.GetTranscriptCategoriesAsync()).Count() == allTranscriptCategories.Count());

                // Verify that the TranscriptCategories was added to the cache after it was read from the repository
                cacheProviderMock.Verify(m => m.AddAndUnlockSemaphore(valcodeName, It.IsAny<Object>(), It.IsAny<SemaphoreSlim>(), It.IsAny<CacheItemPolicy>(), null));
            }

            [TestMethod]
            public async Task StudentReferenceDataRepo_GetTranscriptCategories_Cached()
            {
                object lockHandle = null;
                // Set up local cache mock to respond to cache request:
                //  -to "Contains" request, return "false" to indicate item is not in cache
                //  -to cache "Get" request, return null so we know it's getting data from "repository"
                cacheProviderMock.Setup(x => x.Contains(valcodeName, null)).Returns(false);
                cacheProviderMock.Setup(x => x.GetAndLock(valcodeName, out lockHandle, null)).Returns(null);

                // Get these codes to populate the cache
                var transcriptCategories = (await referenceDataRepo.GetTranscriptCategoriesAsync());
                // Assert the TranscriptCategories are returned
                Assert.IsTrue((await referenceDataRepo.GetTranscriptCategoriesAsync()).Count() == allTranscriptCategories.Count());
                // Make sure we can verify that it's in the cache
                cacheProviderMock.Setup(x => x.Contains(valcodeName, null)).Returns(true);
                var transcriptCategoriesTuple = new Tuple<object, SemaphoreSlim>(transcriptCategories, new SemaphoreSlim(1, 1));
                cacheProviderMock.Setup(x => x.GetAndLockSemaphoreAsync(valcodeName, null)).ReturnsAsync(transcriptCategoriesTuple).Verifiable();

                // Verify that the types are now retrieved from cache
                transcriptCategories = (await referenceDataRepo.GetTranscriptCategoriesAsync());
                cacheProviderMock.Verify(m => m.GetAndLockSemaphoreAsync(valcodeName, null));
            }

            private T GetCache<T>(ICacheProvider cacheProvider, string key)
                where T : class
            {
                object cache = cacheProvider.Get(key, null);
                return cache as T;
            }

            private StudentReferenceDataRepository BuildValidReferenceDataRepository()
            {
                // transaction factory mock
                transFactoryMock = new Mock<IColleagueTransactionFactory>();
                // Cache Mock
                //localCacheMock = new Mock<ObjectCache>();
                // Cache Provider Mock
                cacheProviderMock = new Mock<ICacheProvider>();
                // Set up data accessor for mocking 
                dataAccessorMock = new Mock<IColleagueDataReader>();
                apiSettings = new ApiSettings("TEST");

                // Set up dataAccessorMock as the object for the DataAccessor
                transFactoryMock.Setup(transFac => transFac.GetDataReader()).Returns(dataAccessorMock.Object);

                // Setup response to course type valcode read
                dataAccessorMock.Setup(acc => acc.ReadRecordAsync<ApplValcodes>("ST.VALCODES", "TRANSCRIPT.CATEGORIES", true)).ReturnsAsync(transcriptCategoryValcodeResponse);

                // Construct repository
                referenceDataRepo = new StudentReferenceDataRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object, apiSettings);

                return referenceDataRepo;
            }

            private ApplValcodes BuildValcodeResponse(IEnumerable<Ellucian.Colleague.Domain.Student.Entities.TranscriptCategory> transcriptCategory)
            {
                ApplValcodes applValcodeResponse = new ApplValcodes();
                applValcodeResponse.ValsEntityAssociation = new List<ApplValcodesVals>();
                foreach (var item in transcriptCategory)
                {
                    applValcodeResponse.ValsEntityAssociation.Add(new ApplValcodesVals("", item.Description, "", item.Code, "", "", ""));
                }
                return applValcodeResponse;
            }
        }

        [TestClass]
        public class WaitlistStatusCodes : StudentReferenceDataRepositoryTests
        {
            IEnumerable<WaitlistStatusCode> wlStatusCodes;

            [TestInitialize]
            public async void WaitlistStatusCodes_Initialize()
            {
                var statuses = BuildWaitListStatuses();
                dataReaderMock.Setup(r => r.ReadRecord<ApplValcodes>("ST.VALCODES", "WAIT.LIST.STATUSES", true)).Returns(statuses);

                wlStatusCodes = await referenceDataRepo.GetWaitlistStatusCodesAsync();

                cacheProviderMock.Setup<Task<Tuple<object, SemaphoreSlim>>>(x =>
                   x.GetAndLockSemaphoreAsync(It.IsAny<string>(), null))
                   .ReturnsAsync(new Tuple<object, SemaphoreSlim>(null, new SemaphoreSlim(1, 1)));

            }

            public void WaitlistStatusCodes_Active()
            {
                var result = wlStatusCodes.FirstOrDefault(x => x.Code == "A");
                Assert.AreEqual("A", result.Code);
                Assert.AreEqual("Active", result.Description);
                Assert.AreEqual(WaitlistStatus.WaitingToEnroll, result.Status);
            }

            public void WaitlistStatusCodes_Enrolled()
            {
                var result = wlStatusCodes.FirstOrDefault(x => x.Code == "E");
                Assert.AreEqual("E", result.Code);
                Assert.AreEqual("Enrolled", result.Description);
                Assert.AreEqual(WaitlistStatus.Enrolled, result.Status);
            }

            public void WaitlistStatusCodes_Dropped()
            {
                var result = wlStatusCodes.FirstOrDefault(x => x.Code == "D");
                Assert.AreEqual("D", result.Code);
                Assert.AreEqual("Dropped", result.Description);
                Assert.AreEqual(WaitlistStatus.DroppedFromWaitlist, result.Status);
            }

            public void WaitlistStatusCodes_PermittedToEnroll()
            {
                var result = wlStatusCodes.FirstOrDefault(x => x.Code == "P");
                Assert.AreEqual("P", result.Code);
                Assert.AreEqual("Permission to enroll", result.Description);
                Assert.AreEqual(WaitlistStatus.OfferedEnrollment, result.Status);
            }

            public void WaitlistStatusCodes_Expired()
            {
                var result = wlStatusCodes.FirstOrDefault(x => x.Code == "X");
                Assert.AreEqual("X", result.Code);
                Assert.AreEqual("Expired", result.Description);
                Assert.AreEqual(WaitlistStatus.EnrollmentOfferExpired, result.Status);
            }

            public void WaitlistStatusCodes_Cancelled()
            {
                var result = wlStatusCodes.FirstOrDefault(x => x.Code == "C");
                Assert.AreEqual("C", result.Code);
                Assert.AreEqual("Cabcelled", result.Description);
                Assert.AreEqual(WaitlistStatus.SectionCancelled, result.Status);
            }

            public void WaitlistStatusCodes_Closed()
            {
                var result = wlStatusCodes.FirstOrDefault(x => x.Code == "L");
                Assert.AreEqual("L", result.Code);
                Assert.AreEqual("Closed", result.Description);
                Assert.AreEqual(WaitlistStatus.WaitlistClosed, result.Status);
            }

            public void WaitlistStatusCodes_OtherSection()
            {
                var result = wlStatusCodes.FirstOrDefault(x => x.Code == "OS");
                Assert.AreEqual("OS", result.Code);
                Assert.AreEqual("Other Section", result.Description);
                Assert.AreEqual(WaitlistStatus.WaitingToEnroll, result.Status);
            }

            public void WaitlistStatusCodes_Unknown()
            {
                var result = wlStatusCodes.FirstOrDefault(x => x.Code == "OT");
                Assert.AreEqual("OT", result.Code);
                Assert.AreEqual("Other", result.Description);
                Assert.AreEqual(WaitlistStatus.Unknown, result.Status);
            }

            public ApplValcodes BuildWaitListStatuses()
            {
                var statuses = new ApplValcodes() { Recordkey = "WAIT.LIST.STATUSES", ValNoMod = "N" };
                statuses.ValsEntityAssociation.Add(new ApplValcodesVals("A", "Active", "1", "A", "", "", ""));
                statuses.ValsEntityAssociation.Add(new ApplValcodesVals("E", "Enrolled", "2", "E", "", "", ""));
                statuses.ValsEntityAssociation.Add(new ApplValcodesVals("D", "Dropped", "3", "D", "", "", ""));
                statuses.ValsEntityAssociation.Add(new ApplValcodesVals("P", "Permission to Enroll", "4", "P", "", "", ""));
                statuses.ValsEntityAssociation.Add(new ApplValcodesVals("X", "Expired", "5", "X", "", "", ""));
                statuses.ValsEntityAssociation.Add(new ApplValcodesVals("C", "Cancelled", "6", "C", "", "", ""));
                statuses.ValsEntityAssociation.Add(new ApplValcodesVals("L", "Closed", "7", "L", "", "", ""));
                statuses.ValsEntityAssociation.Add(new ApplValcodesVals("OS", "Other Section Enrollment", "8", "OS", "", "", ""));
                statuses.ValsEntityAssociation.Add(new ApplValcodesVals("OT", "Other", "", "OT", "", "", ""));

                return statuses;
            }

        }

        [TestClass]
        public class RoommateCharacteristicsTests
        {
            Mock<IColleagueTransactionFactory> transFactoryMock;
            Mock<ICacheProvider> cacheProviderMock;
            Mock<IColleagueDataReader> dataAccessorMock;
            Mock<ILogger> loggerMock;
            IEnumerable<RoommateCharacteristics> allRoommateCharacteristics;
            ApplValcodes roommateCharacteristicsdomainEntityNameResponse;
            string domainEntityNameName;
            ApiSettings apiSettings;

            Mock<IStudentReferenceDataRepository> referenceDataRepositoryMock;
            IStudentReferenceDataRepository referenceDataRepository;
            StudentReferenceDataRepository referenceDataRepo;

            [TestInitialize]
            public void Initialize()
            {
                loggerMock = new Mock<ILogger>();
                apiSettings = new ApiSettings("TEST");

                // Build academic standings responses used for mocking
                allRoommateCharacteristics = new TestStudentReferenceDataRepository().GetRoommateCharacteristicsAsync(false).Result;
                roommateCharacteristicsdomainEntityNameResponse = BuildValcodeResponse(allRoommateCharacteristics);
                var roommateCharacteristicsValResponse = new List<string>() { "2" };
                roommateCharacteristicsdomainEntityNameResponse.ValActionCode1 = roommateCharacteristicsValResponse;

                referenceDataRepositoryMock = new Mock<IStudentReferenceDataRepository>();
                referenceDataRepository = referenceDataRepositoryMock.Object;

                // Build academic standing repository
                referenceDataRepo = BuildValidReferenceDataRepository();
                domainEntityNameName = referenceDataRepo.BuildFullCacheKey("ST_ROOMMATE.CHARACTERISTICS_GUID");

                cacheProviderMock.Setup<Task<Tuple<object, SemaphoreSlim>>>(x =>
                   x.GetAndLockSemaphoreAsync(It.IsAny<string>(), null))
                   .ReturnsAsync(new Tuple<object, SemaphoreSlim>(null, new SemaphoreSlim(1, 1)));
            }

            [TestCleanup]
            public void Cleanup()
            {
                transFactoryMock = null;
                dataAccessorMock = null;
                cacheProviderMock = null;
                roommateCharacteristicsdomainEntityNameResponse = null;
                allRoommateCharacteristics = null;
                referenceDataRepo = null;
            }

            [TestMethod]
            public async Task StudentReferenceDataRepo_GetsRoommateCharacteristicsCacheAsync()
            {
                var roommateCharacteristics = await referenceDataRepo.GetRoommateCharacteristicsAsync(false);

                for (int i = 0; i < allRoommateCharacteristics.Count(); i++)
                {
                    Assert.AreEqual(allRoommateCharacteristics.ElementAt(i).Code, roommateCharacteristics.ElementAt(i).Code);
                    Assert.AreEqual(allRoommateCharacteristics.ElementAt(i).Description, roommateCharacteristics.ElementAt(i).Description);
                }
            }

            [TestMethod]
            public async Task StudentReferenceDataRepo_GetsRoommateCharacteristicsNonCacheAsync()
            {
                var statuses = await referenceDataRepo.GetRoommateCharacteristicsAsync(true);

                for (int i = 0; i < allRoommateCharacteristics.Count(); i++)
                {
                    Assert.AreEqual(allRoommateCharacteristics.ElementAt(i).Code, statuses.ElementAt(i).Code);
                    Assert.AreEqual(allRoommateCharacteristics.ElementAt(i).Description, statuses.ElementAt(i).Description);
                }
            }

            [TestMethod]
            public async Task StudentReferenceDataRepo_GetRoommateCharacteristics_WritesToCacheAsync()
            {

                // Set up local cache mock to respond to cache request:
                //  -to "Contains" request, return "false" to indicate item is not in cache
                //  -to cache "Get" request, return null so we know it's reading from the "repository"
                cacheProviderMock.Setup(x => x.Contains(domainEntityNameName, null)).Returns(false);
                cacheProviderMock.Setup(x => x.Get(domainEntityNameName, null)).Returns(null);

                // return a valid response to the data accessor request
                dataAccessorMock.Setup(acc => acc.ReadRecordAsync<ApplValcodes>("ST.VALCODES", "ROOMMATE.CHARACTERISTICS", It.IsAny<bool>())).ReturnsAsync(roommateCharacteristicsdomainEntityNameResponse);

                cacheProviderMock.Setup<Task<Tuple<object, SemaphoreSlim>>>(x =>
                 x.GetAndLockSemaphoreAsync(It.IsAny<string>(), null))
                 .ReturnsAsync(new Tuple<object, SemaphoreSlim>(null, new SemaphoreSlim(1, 1)));

                // But after data accessor read, set up mocking so we can verify the list of roommateCharacteristics was written to the cache
                cacheProviderMock.Setup(x => x.Add(It.IsAny<string>(), It.IsAny<Task<List<RoommateCharacteristics>>>(), It.IsAny<CacheItemPolicy>(), null)).Verifiable();

                cacheProviderMock.Setup(x => x.Contains(referenceDataRepo.BuildFullCacheKey("ST_ROOMMATE.CHARACTERISTICS"), null)).Returns(true);
                var roommateCharacteristics = await referenceDataRepo.GetRoommateCharacteristicsAsync(false);
                cacheProviderMock.Setup(x => x.Get(referenceDataRepo.BuildFullCacheKey("ST_ROOMMATE.CHARACTERISTICS"), null)).Returns(roommateCharacteristics);
                // Verify that roommateCharacteristics were returned, which means they came from the "repository".
                Assert.IsTrue(roommateCharacteristics.Count() == 3);

                // Verify that the roommateCharacteristics item was added to the cache after it was read from the repository
                cacheProviderMock.Verify(m => m.Add(It.IsAny<string>(), It.IsAny<Task<List<RoommateCharacteristics>>>(), It.IsAny<CacheItemPolicy>(), null), Times.Never);

            }

            [TestMethod]
            public async Task StudentReferenceDataRepo_GetRoommateCharacteristics_GetsCachedRoommateCharacteristicsAsync()
            {
                // Set up local cache mock to respond to cache request:
                //  -to "Contains" request, return "true" to indicate item is in cache
                //  -to "Get" request, return the cache item (in this case the "ROOMMATE.CHARACTERISTICS" cache item)
                cacheProviderMock.Setup(x => x.Contains(domainEntityNameName, null)).Returns(true);
                cacheProviderMock.Setup(x => x.Get(domainEntityNameName, null)).Returns(allRoommateCharacteristics).Verifiable();

                // return null for request, so that if we have a result, it wasn't the data accessor that returned it.
                dataAccessorMock.Setup(acc => acc.ReadRecordAsync<ApplValcodes>("ST.VALCODES", "ROOMMATE.CHARACTERISTICS", true)).ReturnsAsync(new ApplValcodes());

                // Assert the roommateCharacteristics are returned
                Assert.IsTrue((await referenceDataRepo.GetRoommateCharacteristicsAsync(false)).Count() == 3);
                // Verify that the sroommateCharacteristics were retrieved from cache
                cacheProviderMock.Verify(m => m.Get(domainEntityNameName, null));
            }

            private StudentReferenceDataRepository BuildValidReferenceDataRepository()
            {
                // transaction factory mock
                transFactoryMock = new Mock<IColleagueTransactionFactory>();
                // Cache Provider Mock
                cacheProviderMock = new Mock<ICacheProvider>();
                // Set up data accessor for mocking 
                dataAccessorMock = new Mock<IColleagueDataReader>();

                // Set up dataAccessorMock as the object for the DataAccessor
                transFactoryMock.Setup(transFac => transFac.GetDataReader()).Returns(dataAccessorMock.Object);

                // Setup response to roommateCharacteristics domainEntityName read
                dataAccessorMock.Setup(acc => acc.ReadRecordAsync<ApplValcodes>("ST.VALCODES", "ROOMMATE.CHARACTERISTICS", It.IsAny<bool>())).ReturnsAsync(roommateCharacteristicsdomainEntityNameResponse);
                cacheProviderMock.Setup<Task<Tuple<object, SemaphoreSlim>>>(x => x.GetAndLockSemaphoreAsync(It.IsAny<string>(), null))
                .ReturnsAsync(new Tuple<object, SemaphoreSlim>(null, new SemaphoreSlim(1, 1)));

                dataAccessorMock.Setup(acc => acc.SelectAsync(It.IsAny<RecordKeyLookup[]>())).Returns<RecordKeyLookup[]>(recordKeyLookups =>
                {
                    var result = new Dictionary<string, RecordKeyLookupResult>();
                    foreach (var recordKeyLookup in recordKeyLookups)
                    {
                        var roommateCharacteristics = allRoommateCharacteristics.Where(e => e.Code == recordKeyLookup.SecondaryKey).FirstOrDefault();
                        result.Add(string.Join("+", new string[] { "ST.VALCODES", "ROOMMATE.CHARACTERISTICS", roommateCharacteristics.Code }),
                            new RecordKeyLookupResult() { Guid = roommateCharacteristics.Guid });
                    }
                    return Task.FromResult(result);
                });

                // Construct repository
                referenceDataRepo = new StudentReferenceDataRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object, apiSettings);

                return referenceDataRepo;
            }

            private ApplValcodes BuildValcodeResponse(IEnumerable<RoommateCharacteristics> roommateCharacteristics)
            {
                ApplValcodes roommateCharacteristicsResponse = new ApplValcodes();
                roommateCharacteristicsResponse.ValsEntityAssociation = new List<ApplValcodesVals>();
                foreach (var item in roommateCharacteristics)
                {
                    roommateCharacteristicsResponse.ValsEntityAssociation.Add(new ApplValcodesVals("", item.Description, "2", item.Code, "3", "", ""));
                }
                return roommateCharacteristicsResponse;
            }
        }

        [TestClass]
        public class FloorCharacteristics
        {
            Mock<IColleagueTransactionFactory> transFactoryMock;
            Mock<ICacheProvider> cacheProviderMock;
            Mock<IColleagueDataReader> dataAccessorMock;
            Mock<ILogger> loggerMock;
            IEnumerable<Ellucian.Colleague.Domain.Student.Entities.FloorCharacteristics> allFloorCharacteristics;
            ApplValcodes floorCharacteristicsdomainEntityNameResponse;
            string domainEntityNameName;
            ApiSettings apiSettings;

            Mock<IStudentReferenceDataRepository> referenceDataRepositoryMock;
            IStudentReferenceDataRepository referenceDataRepository;
            StudentReferenceDataRepository referenceDataRepo;

            [TestInitialize]
            public void Initialize()
            {
                loggerMock = new Mock<ILogger>();
                apiSettings = new ApiSettings("TEST");

                // Build academic standings responses used for mocking
                allFloorCharacteristics = new TestStudentReferenceDataRepository().GetFloorCharacteristicsAsync(false).Result;
                floorCharacteristicsdomainEntityNameResponse = BuildValcodeResponse(allFloorCharacteristics);
                var floorCharacteristicsValResponse = new List<string>() { "2" };
                floorCharacteristicsdomainEntityNameResponse.ValActionCode1 = floorCharacteristicsValResponse;

                referenceDataRepositoryMock = new Mock<IStudentReferenceDataRepository>();
                referenceDataRepository = referenceDataRepositoryMock.Object;

                // Build academic standing repository
                referenceDataRepo = BuildValidReferenceDataRepository();
                domainEntityNameName = referenceDataRepo.BuildFullCacheKey("ST_FLOOR.PREFERENCES_GUID");

                cacheProviderMock.Setup<Task<Tuple<object, SemaphoreSlim>>>(x =>
                   x.GetAndLockSemaphoreAsync(It.IsAny<string>(), null))
                   .ReturnsAsync(new Tuple<object, SemaphoreSlim>(null, new SemaphoreSlim(1, 1)));
            }

            [TestCleanup]
            public void Cleanup()
            {
                transFactoryMock = null;
                dataAccessorMock = null;
                cacheProviderMock = null;
                floorCharacteristicsdomainEntityNameResponse = null;
                allFloorCharacteristics = null;
                referenceDataRepo = null;
            }

            [TestMethod]
            public async Task StudentReferenceDataRepo_GetsFloorCharacteristicsNoArgAsync()
            {
                var floorCharacteristics = await referenceDataRepo.GetFloorCharacteristicsAsync(true);

                for (int i = 0; i < allFloorCharacteristics.Count(); i++)
                {
                    Assert.AreEqual(allFloorCharacteristics.ElementAt(i).Code, floorCharacteristics.ElementAt(i).Code);
                    Assert.AreEqual(allFloorCharacteristics.ElementAt(i).Description, floorCharacteristics.ElementAt(i).Description);
                }
            }

            [TestMethod]
            public async Task StudentReferenceDataRepo_GetsFloorCharacteristicsCacheAsync()
            {
                var floorCharacteristics = await referenceDataRepo.GetFloorCharacteristicsAsync(false);

                for (int i = 0; i < allFloorCharacteristics.Count(); i++)
                {
                    Assert.AreEqual(allFloorCharacteristics.ElementAt(i).Code, floorCharacteristics.ElementAt(i).Code);
                    Assert.AreEqual(allFloorCharacteristics.ElementAt(i).Description, floorCharacteristics.ElementAt(i).Description);
                }
            }

            [TestMethod]
            public async Task StudentReferenceDataRepo_GetsFloorCharacteristicsNonCacheAsync()
            {
                var statuses = await referenceDataRepo.GetFloorCharacteristicsAsync(true);

                for (int i = 0; i < allFloorCharacteristics.Count(); i++)
                {
                    Assert.AreEqual(allFloorCharacteristics.ElementAt(i).Code, statuses.ElementAt(i).Code);
                    Assert.AreEqual(allFloorCharacteristics.ElementAt(i).Description, statuses.ElementAt(i).Description);
                }
            }

            [TestMethod]
            public async Task StudentReferenceDataRepo_GetFloorCharacteristics_WritesToCacheAsync()
            {

                // Set up local cache mock to respond to cache request:
                //  -to "Contains" request, return "false" to indicate item is not in cache
                //  -to cache "Get" request, return null so we know it's reading from the "repository"
                cacheProviderMock.Setup(x => x.Contains(domainEntityNameName, null)).Returns(false);
                cacheProviderMock.Setup(x => x.Get(domainEntityNameName, null)).Returns(null);

                // return a valid response to the data accessor request
                dataAccessorMock.Setup(acc => acc.ReadRecordAsync<ApplValcodes>("ST.VALCODES", "FLOOR.PREFERENCES", It.IsAny<bool>())).ReturnsAsync(floorCharacteristicsdomainEntityNameResponse);

                cacheProviderMock.Setup<Task<Tuple<object, SemaphoreSlim>>>(x =>
                 x.GetAndLockSemaphoreAsync(It.IsAny<string>(), null))
                 .ReturnsAsync(new Tuple<object, SemaphoreSlim>(null, new SemaphoreSlim(1, 1)));

                // But after data accessor read, set up mocking so we can verify the list of floorCharacteristics was written to the cache
                cacheProviderMock.Setup(x => x.Add(It.IsAny<string>(), It.IsAny<Task<List<FloorCharacteristics>>>(), It.IsAny<CacheItemPolicy>(), null)).Verifiable();

                cacheProviderMock.Setup(x => x.Contains(referenceDataRepo.BuildFullCacheKey("ST_FLOOR.PREFERENCES"), null)).Returns(true);
                var floorCharacteristics = await referenceDataRepo.GetFloorCharacteristicsAsync(false);
                cacheProviderMock.Setup(x => x.Get(referenceDataRepo.BuildFullCacheKey("ST_FLOOR.PREFERENCES"), null)).Returns(floorCharacteristics);
                // Verify that floorCharacteristics were returned, which means they came from the "repository".
                Assert.IsTrue(floorCharacteristics.Count() == 3);

                // Verify that the floorCharacteristics item was added to the cache after it was read from the repository
                cacheProviderMock.Verify(m => m.Add(It.IsAny<string>(), It.IsAny<Task<List<FloorCharacteristics>>>(), It.IsAny<CacheItemPolicy>(), null), Times.Never);

            }

            [TestMethod]
            public async Task StudentReferenceDataRepo_GetFloorCharacteristics_GetsCachedFloorCharacteristicsAsync()
            {
                // Set up local cache mock to respond to cache request:
                //  -to "Contains" request, return "true" to indicate item is in cache
                //  -to "Get" request, return the cache item (in this case the "FLOOR.PREFERENCES" cache item)
                cacheProviderMock.Setup(x => x.Contains(domainEntityNameName, null)).Returns(true);
                cacheProviderMock.Setup(x => x.Get(domainEntityNameName, null)).Returns(allFloorCharacteristics).Verifiable();

                // return null for request, so that if we have a result, it wasn't the data accessor that returned it.
                dataAccessorMock.Setup(acc => acc.ReadRecordAsync<ApplValcodes>("ST.VALCODES", "FLOOR.PREFERENCES", true)).ReturnsAsync(new ApplValcodes());

                // Assert the floorCharacteristics are returned
                Assert.IsTrue((await referenceDataRepo.GetFloorCharacteristicsAsync(false)).Count() == 3);
                // Verify that the sfloorCharacteristics were retrieved from cache
                cacheProviderMock.Verify(m => m.Get(domainEntityNameName, null));
            }

            private StudentReferenceDataRepository BuildValidReferenceDataRepository()
            {
                // transaction factory mock
                transFactoryMock = new Mock<IColleagueTransactionFactory>();
                // Cache Provider Mock
                cacheProviderMock = new Mock<ICacheProvider>();
                // Set up data accessor for mocking 
                dataAccessorMock = new Mock<IColleagueDataReader>();

                // Set up dataAccessorMock as the object for the DataAccessor
                transFactoryMock.Setup(transFac => transFac.GetDataReader()).Returns(dataAccessorMock.Object);

                // Setup response to floorCharacteristics domainEntityName read
                dataAccessorMock.Setup(acc => acc.ReadRecordAsync<ApplValcodes>("ST.VALCODES", "FLOOR.PREFERENCES", It.IsAny<bool>())).ReturnsAsync(floorCharacteristicsdomainEntityNameResponse);
                cacheProviderMock.Setup<Task<Tuple<object, SemaphoreSlim>>>(x => x.GetAndLockSemaphoreAsync(It.IsAny<string>(), null))
                .ReturnsAsync(new Tuple<object, SemaphoreSlim>(null, new SemaphoreSlim(1, 1)));

                dataAccessorMock.Setup(acc => acc.SelectAsync(It.IsAny<RecordKeyLookup[]>())).Returns<RecordKeyLookup[]>(recordKeyLookups =>
                {
                    var result = new Dictionary<string, RecordKeyLookupResult>();
                    foreach (var recordKeyLookup in recordKeyLookups)
                    {
                        var floorCharacteristics = allFloorCharacteristics.Where(e => e.Code == recordKeyLookup.SecondaryKey).FirstOrDefault();
                        result.Add(string.Join("+", new string[] { "ST.VALCODES", "FLOOR.PREFERENCES", floorCharacteristics.Code }),
                            new RecordKeyLookupResult() { Guid = floorCharacteristics.Guid });
                    }
                    return Task.FromResult(result);
                });

                // Construct repository
                referenceDataRepo = new StudentReferenceDataRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object, apiSettings);

                return referenceDataRepo;
            }

            private ApplValcodes BuildValcodeResponse(IEnumerable<Ellucian.Colleague.Domain.Student.Entities.FloorCharacteristics> floorCharacteristics)
            {
                ApplValcodes floorCharacteristicsResponse = new ApplValcodes();
                floorCharacteristicsResponse.ValsEntityAssociation = new List<ApplValcodesVals>();
                foreach (var item in floorCharacteristics)
                {
                    floorCharacteristicsResponse.ValsEntityAssociation.Add(new ApplValcodesVals("", item.Description, "2", item.Code, "3", "", ""));
                }
                return floorCharacteristicsResponse;
            }
        }

        /// <summary>
        /// Test class for WithdrawReason codes
        /// </summary>
        [TestClass]
        public class WithdrawReasonTests
        {
            Mock<IColleagueTransactionFactory> transFactoryMock;
            Mock<ICacheProvider> cacheProviderMock;
            Mock<IColleagueDataReader> dataAccessorMock;
            Mock<ILogger> loggerMock;
            IEnumerable<WithdrawReason> _withdrawReasonsCollection;
            string codeItemName;
            ApiSettings apiSettings;

            StudentReferenceDataRepository referenceDataRepo;

            [TestInitialize]
            public void Initialize()
            {
                loggerMock = new Mock<ILogger>();
                 // Build responses used for mocking
                _withdrawReasonsCollection = new List<WithdrawReason>()
                {
                    new Domain.Student.Entities.WithdrawReason("761597be-0a12-4aa8-8ffe-afc04b62da41", "AC", "Academic Reasons"),
                    new Domain.Student.Entities.WithdrawReason("8cc60bb6-1e0e-45f1-bf10-b53d6809275e", "FP", "Financial Problems"),
                    new Domain.Student.Entities.WithdrawReason("6196cc8c-6e2c-4bb5-8859-b2553b24c772", "MILIT", "Serve In The Armed Forces"),
             
                };

                // Build repository
                referenceDataRepo = BuildValidReferenceDataRepository();
                codeItemName = referenceDataRepo.BuildFullCacheKey("AllWithdrawReasons");
            }

            [TestCleanup]
            public void Cleanup()
            {
                transFactoryMock = null;
                dataAccessorMock = null;
                cacheProviderMock = null;
                _withdrawReasonsCollection = null;
                referenceDataRepo = null;
            }

            [TestMethod]
            public async Task GetsWithdrawReasonCacheAsync()
            {
                var result = await referenceDataRepo.GetWithdrawReasonsAsync(false);

                for (int i = 0; i < _withdrawReasonsCollection.Count(); i++)
                {
                    Assert.AreEqual(_withdrawReasonsCollection.ElementAt(i).Guid, result.ElementAt(i).Guid);
                    Assert.AreEqual(_withdrawReasonsCollection.ElementAt(i).Code, result.ElementAt(i).Code);
                    Assert.AreEqual(_withdrawReasonsCollection.ElementAt(i).Description, result.ElementAt(i).Description);
                }
            }

            [TestMethod]
            public async Task GetsWithdrawReasonNonCacheAsync()
            {
                var result = await referenceDataRepo.GetWithdrawReasonsAsync(true);

                for (int i = 0; i < _withdrawReasonsCollection.Count(); i++)
                {
                    Assert.AreEqual(_withdrawReasonsCollection.ElementAt(i).Guid, result.ElementAt(i).Guid);
                    Assert.AreEqual(_withdrawReasonsCollection.ElementAt(i).Code, result.ElementAt(i).Code);
                    Assert.AreEqual(_withdrawReasonsCollection.ElementAt(i).Description, result.ElementAt(i).Description);
                }
            }

            private StudentReferenceDataRepository BuildValidReferenceDataRepository()
            {
                // transaction factory mock
                transFactoryMock = new Mock<IColleagueTransactionFactory>();
                // Cache Provider Mock
                cacheProviderMock = new Mock<ICacheProvider>();
                // Set up data accessor for mocking 
                dataAccessorMock = new Mock<IColleagueDataReader>();

                // Set up dataAccessorMock as the object for the DataAccessor
                transFactoryMock.Setup(transFac => transFac.GetDataReader()).Returns(dataAccessorMock.Object);

                // Setup response to WithdrawReason read
                var entityCollection = new Collection<WithdrawReasons>(_withdrawReasonsCollection.Select(record =>
                    new Data.Student.DataContracts.WithdrawReasons()
                    {
                        Recordkey = record.Code,
                        WdrDesc = record.Description,
                        RecordGuid = record.Guid
                    }).ToList());

                dataAccessorMock.Setup(acc => acc.BulkReadRecordAsync<WithdrawReasons>("WITHDRAW.REASONS", "", true))
                    .ReturnsAsync(entityCollection);

                cacheProviderMock.Setup<Task<Tuple<object, SemaphoreSlim>>>(x => x.GetAndLockSemaphoreAsync(It.IsAny<string>(), null))
                    .ReturnsAsync(new Tuple<object, SemaphoreSlim>(null, new SemaphoreSlim(1, 1)));


                dataAccessorMock.Setup(acc => acc.SelectAsync(It.IsAny<RecordKeyLookup[]>())).Returns<RecordKeyLookup[]>(recordKeyLookups =>
                {
                    var result = new Dictionary<string, RecordKeyLookupResult>();
                    foreach (var recordKeyLookup in recordKeyLookups)
                    {
                        var entity = _withdrawReasonsCollection.Where(e => e.Code == recordKeyLookup.PrimaryKey).FirstOrDefault();
                        result.Add(string.Join("+", new string[] { "WITHDRAW.REASONS", entity.Code }),
                            new RecordKeyLookupResult() { Guid = entity.Guid });
                    }
                    return Task.FromResult(result);
                });

                referenceDataRepo = new StudentReferenceDataRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object, apiSettings);

                return referenceDataRepo;
            }
        }

        [TestClass]
        public class DropReasons
        {
            Mock<IColleagueTransactionFactory> transFactoryMock;
            Mock<ICacheProvider> cacheProviderMock;
            Mock<IColleagueDataReader> dataAccessorMock;
            Mock<ILogger> loggerMock;
            IEnumerable<Ellucian.Colleague.Domain.Student.Entities.DropReason> allDropReasons;
            ApplValcodes dropReasonsValcodeResponse;
            string valcodeName;
            ApiSettings apiSettings;
            StudentReferenceDataRepository referenceDataRepo;

            [TestInitialize]
            public void Initialize()
            {
                loggerMock = new Mock<ILogger>();

                // Build responses used for mocking
                allDropReasons = new TestStudentReferenceDataRepository().GetDropReasonsAsync().Result;

                dropReasonsValcodeResponse = BuildValcodeResponse(allDropReasons);

                // Build repository
                referenceDataRepo = BuildValidReferenceDataRepository();
                valcodeName = referenceDataRepo.BuildFullCacheKey("ST_STUDENT.ACAD.CRED.STATUS.REASONS");

                cacheProviderMock.Setup<Task<Tuple<object, SemaphoreSlim>>>(x =>
                   x.GetAndLockSemaphoreAsync(It.IsAny<string>(), null))
                   .ReturnsAsync(new Tuple<object, SemaphoreSlim>(null, new SemaphoreSlim(1, 1)));

            }

            [TestCleanup]
            public void Cleanup()
            {
                transFactoryMock = null;
                dataAccessorMock = null;
                cacheProviderMock = null;
                dropReasonsValcodeResponse = null;
                allDropReasons = null;
                referenceDataRepo = null;
            }

            [TestMethod]
            public async Task StudentReferenceDataRepo_GetDropReasons()
            {
                var repo = await referenceDataRepo.GetDropReasonsAsync();

                for (int i = 0; i < allDropReasons.Count(); i++)
                {
                    Assert.AreEqual(allDropReasons.ElementAt(i).Code, repo.ElementAt(i).Code);
                    Assert.AreEqual(allDropReasons.ElementAt(i).Description, repo.ElementAt(i).Description);
                    Assert.AreEqual(allDropReasons.ElementAt(i).DisplayInSelfService, repo.ElementAt(i).DisplayInSelfService);
                }
            }

            [TestMethod]
            public async Task StudentReferenceDataRepo_GetCapSizes_WritesToCache()
            {
                // Set up local cache mock to respond to cache request:
                //  -to "Contains" request, return "false" to indicate item is not in cache
                //  -to cache "Get" request, return null so we know it's reading from the "repository"
                cacheProviderMock.Setup(x => x.Contains(valcodeName, null)).Returns(false);
                cacheProviderMock.Setup(x => x.Get(valcodeName, null)).Returns(null);

                // return a valid response to the data accessor request
                dataAccessorMock.Setup(acc => acc.ReadRecordAsync<ApplValcodes>(It.IsAny<string>(), It.IsAny<string>(), true)).ReturnsAsync(dropReasonsValcodeResponse);

                // But after data accessor read, set up mocking so we can verify the list of CapSizes was written to the cache
                cacheProviderMock.Setup(x => x.AddAndUnlockSemaphore(valcodeName, It.IsAny<Object>(), It.IsAny<SemaphoreSlim>(), It.IsAny<CacheItemPolicy>(), null)).Verifiable();

                // Verify that CapSizes were returned, which means they came from the "repository".
                Assert.IsTrue((await referenceDataRepo.GetDropReasonsAsync()).Count() == allDropReasons.Count());

                // Verify that the CapSizes was added to the cache after it was read from the repository
                cacheProviderMock.Verify(m => m.AddAndUnlockSemaphore(valcodeName, It.IsAny<Object>(), It.IsAny<SemaphoreSlim>(), It.IsAny<CacheItemPolicy>(), null));
            }

            [TestMethod]
            public async Task StudentReferenceDataRepo_GetCapSizes_Cached()
            {
                object lockHandle = null;
                // Set up local cache mock to respond to cache request:
                //  -to "Contains" request, return "false" to indicate item is not in cache
                //  -to cache "Get" request, return null so we know it's getting data from "repository"
                cacheProviderMock.Setup(x => x.Contains(valcodeName, null)).Returns(false);
                cacheProviderMock.Setup(x => x.GetAndLock(valcodeName, out lockHandle, null)).Returns(null);

                // Get these codes to populate the cache
                var dropReasons = (await referenceDataRepo.GetDropReasonsAsync());
                // Assert the drop reasons are returned
                Assert.IsTrue((await referenceDataRepo.GetDropReasonsAsync()).Count() == allDropReasons.Count());
                // Make sure we can verify that it's in the cache
                cacheProviderMock.Setup(x => x.Contains(valcodeName, null)).Returns(true);
                var dropReasonTuple = new Tuple<object, SemaphoreSlim>(dropReasons, new SemaphoreSlim(1, 1));
                cacheProviderMock.Setup(x => x.GetAndLockSemaphoreAsync(valcodeName, null)).ReturnsAsync(dropReasonTuple).Verifiable();

                // Verify that the drop reasons are now retrieved from cache
                dropReasons = (await referenceDataRepo.GetDropReasonsAsync());
                cacheProviderMock.Verify(m => m.GetAndLockSemaphoreAsync(valcodeName, null));
            }

            private T GetCache<T>(ICacheProvider cacheProvider, string key)
                where T : class
            {
                object cache = cacheProvider.Get(key, null);
                return cache as T;
            }

            private StudentReferenceDataRepository BuildValidReferenceDataRepository()
            {
                // transaction factory mock
                transFactoryMock = new Mock<IColleagueTransactionFactory>();
                // Cache Mock
                //localCacheMock = new Mock<ObjectCache>();
                // Cache Provider Mock
                cacheProviderMock = new Mock<ICacheProvider>();
                // Set up data accessor for mocking 
                dataAccessorMock = new Mock<IColleagueDataReader>();
                apiSettings = new ApiSettings("TEST");

                // Set up dataAccessorMock as the object for the DataAccessor
                transFactoryMock.Setup(transFac => transFac.GetDataReader()).Returns(dataAccessorMock.Object);

                // Setup response to cap size valcode read
                dataAccessorMock.Setup(acc => acc.ReadRecordAsync<ApplValcodes>("ST.VALCODES", "STUDENT.ACAD.CRED.STATUS.REASONS", true)).ReturnsAsync(dropReasonsValcodeResponse);

                // Construct repository
                referenceDataRepo = new StudentReferenceDataRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object, apiSettings);

                return referenceDataRepo;
            }

            private ApplValcodes BuildValcodeResponse(IEnumerable<Ellucian.Colleague.Domain.Student.Entities.DropReason> dropReasons)
            {
                ApplValcodes applValcodeResponse = new ApplValcodes();
                applValcodeResponse.ValsEntityAssociation = new List<ApplValcodesVals>();
                foreach (var item in dropReasons)
                {
                    string specialProcessingCode = "";
                    if(item.Code=="C")
                    {
                        specialProcessingCode = null;

                    }
                    else if(item.Code=="U")
                    {
                        specialProcessingCode = "S";
                    }
                    else if(item.Code=="W")
                    {
                        specialProcessingCode = "aaa";
                    }
                   
                    applValcodeResponse.ValsEntityAssociation.Add(new ApplValcodesVals("", item.Description, specialProcessingCode, item.Code, "", "", ""));
                }
                return applValcodeResponse;
            }
        }

        [TestClass]
        public class AdministrativeInstructionalMethodsTests : BaseRepositorySetup
        {
            public TestAdministrativeInstructionalMethodRepository testDataRepository;

            public StudentReferenceDataRepository repositoryUnderTest;

            public void StudentReferenceDataRepositoryTestsInitialize()
            {
                MockInitialize();
                testDataRepository = new TestAdministrativeInstructionalMethodRepository();

                repositoryUnderTest = BuildRepository();
            }

            [TestInitialize]
            public void Initialize()
            {
                StudentReferenceDataRepositoryTestsInitialize();
            }

            public StudentReferenceDataRepository BuildRepository()
            {
                string id, guid, id2, guid2, id3, guid3, sid, sid2, sid3, guid4, guid5, guid6;
                GuidLookup guidLookup;
                GuidLookupResult guidLookupResult;
                Dictionary<string, GuidLookupResult> guidLookupDict;
                RecordKeyLookup recordLookup;
                RecordKeyLookupResult recordLookupResult;
                Dictionary<string, RecordKeyLookupResult> recordLookupDict;

                var admInstrMethod = testDataRepository.GetAdministrativeInstructionalMethods();

                // Set up for GUID lookups
                id = admInstrMethod.ElementAt(0).Code; // "1";
                id2 = admInstrMethod.ElementAt(1).Code; //"2";
                id3 = admInstrMethod.ElementAt(2).Code; //"3";

                // Secondary keys for GUID lookups
                sid = "11";
                sid2 = "22";
                sid3 = "33";

                guid = admInstrMethod.ElementAt(0).Guid.ToLowerInvariant(); //"F5FC5310-17F1-49FC-926D-CC6E3DA6DAEA".ToLowerInvariant();
                guid2 = admInstrMethod.ElementAt(1).Guid.ToLowerInvariant(); //"5B35075D-14FB-45F7-858A-83F4174B76EA".ToLowerInvariant();
                guid3 = admInstrMethod.ElementAt(2).Guid.ToLowerInvariant(); //"246E16D9-8790-4D7E-ACA1-D5B1CB9D4A24".ToLowerInvariant();

                guid4 = admInstrMethod.ElementAt(0).InstructionalMethodGuid.ToLowerInvariant();
                guid5 = admInstrMethod.ElementAt(1).InstructionalMethodGuid.ToLowerInvariant();
                guid6 = admInstrMethod.ElementAt(2).InstructionalMethodGuid.ToLowerInvariant();

                guidLookup = new GuidLookup(guid);
                guidLookupResult = new GuidLookupResult() { Entity = "INSTR.METHODS", PrimaryKey = id, SecondaryKey = sid };
                guidLookupDict = new Dictionary<string, GuidLookupResult>();
                recordLookup = new RecordKeyLookup("INSTR.METHODS", id, "INM.INTG.KEY.IDX", sid, false);
                recordLookupResult = new RecordKeyLookupResult() { Guid = guid };
                recordLookupDict = new Dictionary<string, RecordKeyLookupResult>();

                dataReaderMock.SetupSequence(acc => acc.SelectAsync(It.IsAny<GuidLookup[]>()))
                    .Returns(Task.FromResult(new Dictionary<string, GuidLookupResult>() { { guid, new GuidLookupResult() { Entity = "INSTR.METHODS", PrimaryKey = id, SecondaryKey = sid } } }))
                    .Returns(Task.FromResult(new Dictionary<string, GuidLookupResult>() { { guid2, new GuidLookupResult() { Entity = "INSTR.METHODS", PrimaryKey = id2, SecondaryKey = sid2 } } }))
                    .Returns(Task.FromResult(new Dictionary<string, GuidLookupResult>() { { guid3, new GuidLookupResult() { Entity = "INSTR.METHODS", PrimaryKey = id3, SecondaryKey = sid3 } } }));
                dataReaderMock.SetupSequence(ep => ep.SelectAsync(It.IsAny<RecordKeyLookup[]>()))
                    .Returns(Task.FromResult(new Dictionary<string, RecordKeyLookupResult>() { { "INSTR.METHODS+" + id + "+" + sid, new RecordKeyLookupResult() { Guid = guid } } }))
                    .Returns(Task.FromResult(new Dictionary<string, RecordKeyLookupResult>() { { "INSTR.METHODS+" + id2 + "+" + sid, new RecordKeyLookupResult() { Guid = guid2 } } }))
                    .Returns(Task.FromResult(new Dictionary<string, RecordKeyLookupResult>() { { "INSTR.METHODS+" + id3 + "+" + sid, new RecordKeyLookupResult() { Guid = guid3 } } }));

                dataReaderMock.Setup(d => d.SelectAsync("INSTR.METHODS", null))
                    .Returns<string, string>((f, c) => Task.FromResult(testDataRepository.GetAdministrativeInstructionalMethods() == null ? null :
                        testDataRepository.GetAdministrativeInstructionalMethods().Select(dtype => dtype.Guid).ToArray()));

                dataReaderMock.Setup(d => d.BulkReadRecordAsync<Ellucian.Colleague.Data.Student.DataContracts.InstrMethods>(It.IsAny<string[]>(), It.IsAny<bool>()))
                    .Returns<string[], bool>((ids, b) =>
                        Task.FromResult(testDataRepository.GetAdministrativeInstructionalMethods() == null ? null :
                            new Collection<Ellucian.Colleague.Data.Student.DataContracts.InstrMethods>(testDataRepository.GetAdministrativeInstructionalMethods()
                                .Where(record => ids.Contains(record.Guid))
                                .Select(record =>
                                    (record == null) ? null : new Ellucian.Colleague.Data.Student.DataContracts.InstrMethods()
                                    {
                                        Recordkey = record.Code,
                                        InmDesc = record.Description,
                                        RecordGuid = record.Guid,
                                        InmIntgKeyIdx = sid
                                    }).ToList())
                        ));

                apiSettings.BulkReadSize = 1;

                loggerMock.Setup(l => l.IsErrorEnabled).Returns(true);

                return new StudentReferenceDataRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object, apiSettings);
            }

            public async Task<IEnumerable<AdministrativeInstructionalMethod>> getExpectedAdministrativeInstructionalMethods()
            {
                return testDataRepository.GetAdministrativeInstructionalMethods();
            }

            public async Task<IEnumerable<AdministrativeInstructionalMethod>> getActualAdministrativeInstructionalMethods(bool ignoreCache = false)
            {
                return await repositoryUnderTest.GetAdministrativeInstructionalMethodsAsync(ignoreCache);
            }

            [TestMethod]
            public async Task ExpectedEqualsActualTest()
            {
                var expected = (await getExpectedAdministrativeInstructionalMethods()).ToList();
                var actual = (await getActualAdministrativeInstructionalMethods()).ToList();
                CollectionAssert.AreEqual(expected, actual);
            }

            [TestMethod]
            public async Task AttributesTest()
            {
                var expected = (await getExpectedAdministrativeInstructionalMethods()).ToArray();
                var actual = (await getActualAdministrativeInstructionalMethods()).ToArray();
                for (int i = 0; i < expected.Count(); i++)
                {
                    Assert.AreEqual(expected[i].Code, actual[i].Code);
                    Assert.AreEqual(expected[i].Guid, actual[i].Guid);
                    Assert.AreEqual(expected[i].Description, actual[i].Description);
                }
            }

            [TestMethod]
            public async Task ExpectedEqualsActualTest_Cached()
            {
                var expected = (await getExpectedAdministrativeInstructionalMethods()).ToList();
                var actual = (await getActualAdministrativeInstructionalMethods(true)).ToList();
                CollectionAssert.AreEqual(expected, actual);
            }

        }

        /// <summary>
        /// Test class for IntgSecTitleTypes codes
        /// </summary>
        [TestClass]
        public class SectionTitleTypesTests
        {
            Mock<IColleagueTransactionFactory> transFactoryMock;
            Mock<ICacheProvider> cacheProviderMock;
            Mock<IColleagueDataReader> dataAccessorMock;
            Mock<ILogger> loggerMock;
            IEnumerable<IntgSecTitleTypes> _sectionTitleTypesCollection;
            string codeItemName;

            StudentReferenceDataRepository referenceDataRepo;

            [TestInitialize]
            public void Initialize()
            {
                loggerMock = new Mock<ILogger>();

                // Build responses used for mocking
                _sectionTitleTypesCollection = new Collection<IntgSecTitleTypes>()
                {
                    new IntgSecTitleTypes() {RecordGuid = "7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc", Recordkey = "SHORT", IsttTitle = "Short title type", IsttDesc = "This is the short title type" }
                };

                // Build repository
                referenceDataRepo = BuildValidReferenceDataRepository();
                codeItemName = referenceDataRepo.BuildFullCacheKey("AllSectionTitleTypes");
            }

            [TestCleanup]
            public void Cleanup()
            {
                transFactoryMock = null;
                dataAccessorMock = null;
                cacheProviderMock = null;
                _sectionTitleTypesCollection = null;
                referenceDataRepo = null;
            }

            [TestMethod]
            public async Task GetsIntgSecTitleTypesCacheAsync()
            {
                var result = await referenceDataRepo.GetSectionTitleTypesAsync(false);

                for (int i = 0; i < _sectionTitleTypesCollection.Count(); i++)
                {
                    Assert.AreEqual(_sectionTitleTypesCollection.ElementAt(i).RecordGuid , result.ElementAt(i).Guid);
                    Assert.AreEqual(_sectionTitleTypesCollection.ElementAt(i).Recordkey, result.ElementAt(i).Code);
                    Assert.AreEqual(_sectionTitleTypesCollection.ElementAt(i).IsttDesc, result.ElementAt(i).Description);
                    Assert.AreEqual(_sectionTitleTypesCollection.ElementAt(i).IsttTitle, result.ElementAt(i).Title);
                }
            }

            [TestMethod]
            public async Task GetsIntgSecTitleTypesNonCacheAsync()
            {
                var result = await referenceDataRepo.GetSectionTitleTypesAsync(true);

                for (int i = 0; i < _sectionTitleTypesCollection.Count(); i++)
                {
                    Assert.AreEqual(_sectionTitleTypesCollection.ElementAt(i).RecordGuid, result.ElementAt(i).Guid);
                    Assert.AreEqual(_sectionTitleTypesCollection.ElementAt(i).Recordkey, result.ElementAt(i).Code);
                    Assert.AreEqual(_sectionTitleTypesCollection.ElementAt(i).IsttDesc, result.ElementAt(i).Description);
                    Assert.AreEqual(_sectionTitleTypesCollection.ElementAt(i).IsttTitle, result.ElementAt(i).Title);
                }
            }

            private StudentReferenceDataRepository BuildValidReferenceDataRepository()
            {

                ApiSettings _apisettings = new ApiSettings();
                // transaction factory mock
                transFactoryMock = new Mock<IColleagueTransactionFactory>();
                // Cache Provider Mock
                cacheProviderMock = new Mock<ICacheProvider>();
                // Set up data accessor for mocking 
                dataAccessorMock = new Mock<IColleagueDataReader>();

                // Set up dataAccessorMock as the object for the DataAccessor
                transFactoryMock.Setup(transFac => transFac.GetDataReader()).Returns(dataAccessorMock.Object);

                dataAccessorMock.Setup(acc => acc.BulkReadRecordAsync<IntgSecTitleTypes>("INTG.SEC.TITLE.TYPES", "", true))
                    .ReturnsAsync((Collection<IntgSecTitleTypes>)_sectionTitleTypesCollection);


                cacheProviderMock.Setup<Task<Tuple<object, SemaphoreSlim>>>(x => x.GetAndLockSemaphoreAsync(It.IsAny<string>(), null))
                    .ReturnsAsync(new Tuple<object, SemaphoreSlim>(null, new SemaphoreSlim(1, 1)));
 
                // Construct repository
                referenceDataRepo = new StudentReferenceDataRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object, _apisettings);

                return referenceDataRepo;
            }
        }

        [TestClass]
        public class SectionDescriptionTypesTests
        {
            Mock<IColleagueTransactionFactory> transFactoryMock;
            Mock<ICacheProvider> cacheProviderMock;
            Mock<IColleagueDataReader> dataAccessorMock;
            Mock<ILogger> loggerMock;
            IEnumerable<SectionDescriptionType> allSectionDescriptionTypes;
            ApplValcodes sectionDescriptionTypesdomainEntityNameResponse;
            string domainEntityNameName;
            ApiSettings apiSettings;

            Mock<IStudentReferenceDataRepository> referenceDataRepositoryMock;
            IStudentReferenceDataRepository referenceDataRepository;
            StudentReferenceDataRepository referenceDataRepo;

            [TestInitialize]
            public void Initialize()
            {
                loggerMock = new Mock<ILogger>();
                apiSettings = new ApiSettings("TEST");

                // Build section description types responses used for mocking
                allSectionDescriptionTypes = new TestStudentReferenceDataRepository().GetSectionDescriptionTypesAsync(false).Result;
                sectionDescriptionTypesdomainEntityNameResponse = BuildValcodeResponse(allSectionDescriptionTypes);
                var sectionDescriptionTypesValResponse = new List<string>() { "2" };
                sectionDescriptionTypesdomainEntityNameResponse.ValActionCode1 = sectionDescriptionTypesValResponse;

                referenceDataRepositoryMock = new Mock<IStudentReferenceDataRepository>();
                referenceDataRepository = referenceDataRepositoryMock.Object;

                // Build section description type repository
                referenceDataRepo = BuildValidReferenceDataRepository();
                domainEntityNameName = referenceDataRepo.BuildFullCacheKey("ST_INTG.SEC.DESC.TYPES_GUID");

                cacheProviderMock.Setup<Task<Tuple<object, SemaphoreSlim>>>(x =>
                   x.GetAndLockSemaphoreAsync(It.IsAny<string>(), null))
                   .ReturnsAsync(new Tuple<object, SemaphoreSlim>(null, new SemaphoreSlim(1, 1)));
            }

            [TestCleanup]
            public void Cleanup()
            {
                transFactoryMock = null;
                dataAccessorMock = null;
                cacheProviderMock = null;
                sectionDescriptionTypesdomainEntityNameResponse = null;
                allSectionDescriptionTypes = null;
                referenceDataRepo = null;
            }

            [TestMethod]
            public async Task StudentReferenceDataRepo_GetsSectionDescriptionTypesCacheAsync()
            {
                var sectionDescriptionTypes = await referenceDataRepo.GetSectionDescriptionTypesAsync(false);

                for (int i = 0; i < allSectionDescriptionTypes.Count(); i++)
                {
                    Assert.AreEqual(allSectionDescriptionTypes.ElementAt(i).Code, sectionDescriptionTypes.ElementAt(i).Code);
                    Assert.AreEqual(allSectionDescriptionTypes.ElementAt(i).Description, sectionDescriptionTypes.ElementAt(i).Description);
                }
            }

            [TestMethod]
            public async Task StudentReferenceDataRepo_GetsSectionDescriptionTypesNonCacheAsync()
            {
                var statuses = await referenceDataRepo.GetSectionDescriptionTypesAsync(true);

                for (int i = 0; i < allSectionDescriptionTypes.Count(); i++)
                {
                    Assert.AreEqual(allSectionDescriptionTypes.ElementAt(i).Code, statuses.ElementAt(i).Code);
                    Assert.AreEqual(allSectionDescriptionTypes.ElementAt(i).Description, statuses.ElementAt(i).Description);
                }
            }

            [TestMethod]
            public async Task StudentReferenceDataRepo_GetSectionDescriptionTypes_WritesToCacheAsync()
            {

                // Set up local cache mock to respond to cache request:
                //  -to "Contains" request, return "false" to indicate item is not in cache
                //  -to cache "Get" request, return null so we know it's reading from the "repository"
                cacheProviderMock.Setup(x => x.Contains(domainEntityNameName, null)).Returns(false);
                cacheProviderMock.Setup(x => x.Get(domainEntityNameName, null)).Returns(null);

                // return a valid response to the data accessor request
                dataAccessorMock.Setup(acc => acc.ReadRecordAsync<ApplValcodes>("ST.VALCODES", "INTG.SEC.DESC.TYPES", It.IsAny<bool>())).ReturnsAsync(sectionDescriptionTypesdomainEntityNameResponse);

                cacheProviderMock.Setup<Task<Tuple<object, SemaphoreSlim>>>(x =>
                 x.GetAndLockSemaphoreAsync(It.IsAny<string>(), null))
                 .ReturnsAsync(new Tuple<object, SemaphoreSlim>(null, new SemaphoreSlim(1, 1)));

                // But after data accessor read, set up mocking so we can verify the list of SectionDescriptionType was written to the cache
                cacheProviderMock.Setup(x => x.Add(It.IsAny<string>(), It.IsAny<Task<List<SectionDescriptionType>>>(), It.IsAny<CacheItemPolicy>(), null)).Verifiable();

                cacheProviderMock.Setup(x => x.Contains(referenceDataRepo.BuildFullCacheKey("ST_INTG.SEC.DESC.TYPES"), null)).Returns(true);
                var sectionDescriptionTypes = await referenceDataRepo.GetSectionDescriptionTypesAsync(false);
                cacheProviderMock.Setup(x => x.Get(referenceDataRepo.BuildFullCacheKey("ST_INTG.SEC.DESC.TYPES"), null)).Returns(sectionDescriptionTypes);
                // Verify that SectionDescriptionType were returned, which means they came from the "repository".
                Assert.IsTrue(sectionDescriptionTypes.Count() == 3);

                // Verify that the SectionDescriptionType item was added to the cache after it was read from the repository
                cacheProviderMock.Verify(m => m.Add(It.IsAny<string>(), It.IsAny<Task<List<SectionDescriptionType>>>(), It.IsAny<CacheItemPolicy>(), null), Times.Never);

            }

            [TestMethod]
            public async Task StudentReferenceDataRepo_GetSectionDescriptionTypes_GetsCachedSectionDescriptionTypesAsync()
            {
                // Set up local cache mock to respond to cache request:
                //  -to "Contains" request, return "true" to indicate item is in cache
                //  -to "Get" request, return the cache item (in this case the "INTG.SEC.DESC.TYPES" cache item)
                cacheProviderMock.Setup(x => x.Contains(domainEntityNameName, null)).Returns(true);
                cacheProviderMock.Setup(x => x.Get(domainEntityNameName, null)).Returns(allSectionDescriptionTypes).Verifiable();

                // return null for request, so that if we have a result, it wasn't the data accessor that returned it.
                dataAccessorMock.Setup(acc => acc.ReadRecordAsync<ApplValcodes>("ST.VALCODES", "INTG.SEC.DESC.TYPES", true)).ReturnsAsync(new ApplValcodes());

                // Assert the SectionDescriptionType are returned
                Assert.IsTrue((await referenceDataRepo.GetSectionDescriptionTypesAsync(false)).Count() == 3);
                // Verify that the SectionDescriptionType were retrieved from cache
                cacheProviderMock.Verify(m => m.Get(domainEntityNameName, null));
            }

            private StudentReferenceDataRepository BuildValidReferenceDataRepository()
            {
                // transaction factory mock
                transFactoryMock = new Mock<IColleagueTransactionFactory>();
                // Cache Provider Mock
                cacheProviderMock = new Mock<ICacheProvider>();
                // Set up data accessor for mocking 
                dataAccessorMock = new Mock<IColleagueDataReader>();

                // Set up dataAccessorMock as the object for the DataAccessor
                transFactoryMock.Setup(transFac => transFac.GetDataReader()).Returns(dataAccessorMock.Object);

                // Setup response to SectionDescriptionType domainEntityName read
                dataAccessorMock.Setup(acc => acc.ReadRecordAsync<ApplValcodes>("ST.VALCODES", "INTG.SEC.DESC.TYPES", It.IsAny<bool>())).ReturnsAsync(sectionDescriptionTypesdomainEntityNameResponse);
                cacheProviderMock.Setup<Task<Tuple<object, SemaphoreSlim>>>(x => x.GetAndLockSemaphoreAsync(It.IsAny<string>(), null))
                .ReturnsAsync(new Tuple<object, SemaphoreSlim>(null, new SemaphoreSlim(1, 1)));

                dataAccessorMock.Setup(acc => acc.SelectAsync(It.IsAny<RecordKeyLookup[]>())).Returns<RecordKeyLookup[]>(recordKeyLookups =>
                {
                    var result = new Dictionary<string, RecordKeyLookupResult>();
                    foreach (var recordKeyLookup in recordKeyLookups)
                    {
                        var sectionDescriptionTypes = allSectionDescriptionTypes.Where(e => e.Code == recordKeyLookup.SecondaryKey).FirstOrDefault();
                        result.Add(string.Join("+", new string[] { "ST.VALCODES", "INTG.SEC.DESC.TYPES", sectionDescriptionTypes.Code }),
                            new RecordKeyLookupResult() { Guid = sectionDescriptionTypes.Guid });
                    }
                    return Task.FromResult(result);
                });

                // Construct repository
                referenceDataRepo = new StudentReferenceDataRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object, apiSettings);

                return referenceDataRepo;
            }

            private ApplValcodes BuildValcodeResponse(IEnumerable<SectionDescriptionType> sectionDescriptionTypes)
            {
                ApplValcodes sectionDescriptionTypesResponse = new ApplValcodes();
                sectionDescriptionTypesResponse.ValsEntityAssociation = new List<ApplValcodesVals>();
                foreach (var item in sectionDescriptionTypes)
                {
                    sectionDescriptionTypesResponse.ValsEntityAssociation.Add(new ApplValcodesVals("", item.Description, "2", item.Code, "3", "", ""));
                }
                return sectionDescriptionTypesResponse;
            }
        }

        [TestClass]
        public class FinancialAidFundClassifications
        {
            Mock<IColleagueTransactionFactory> transFactoryMock;
            Mock<ICacheProvider> cacheProviderMock;
            Mock<IColleagueDataReader> dataAccessorMock;
            Mock<ILogger> loggerMock;

            IEnumerable<FinancialAidFundClassification> allFinancialAidFundClassifications;
            string valcodeName;
            ApiSettings apiSettings;

            StudentReferenceDataRepository referenceDataRepo;

            [TestInitialize]
            public void Initialize()
            {
                loggerMock = new Mock<ILogger>();
                apiSettings = new ApiSettings("TEST");

                allFinancialAidFundClassifications = new TestStudentReferenceDataRepository().GetFinancialAidFundClassificationsAsync(false).Result;

                referenceDataRepo = BuildValidReferenceDataRepository();
                valcodeName = referenceDataRepo.BuildFullCacheKey("AllFinancialAidFundClassifications");
            }

            [TestCleanup]
            public void Cleanup()
            {
                allFinancialAidFundClassifications = null;
                valcodeName = string.Empty;
                apiSettings = null;
            }

            [TestMethod]
            public async Task StudentReferenceDataRepository_GetFinancialAidFundClassificationsAsync_False()
            {
                var results = await referenceDataRepo.GetFinancialAidFundClassificationsAsync(false);
                Assert.AreEqual(allFinancialAidFundClassifications.Count(), results.Count());

                foreach (var financialAidFundClassification in allFinancialAidFundClassifications)
                {
                    var result = results.FirstOrDefault(i => i.Guid == financialAidFundClassification.Guid);

                    Assert.AreEqual(financialAidFundClassification.Code, result.Code);
                    //Assert.AreEqual(financialAidFundClassification.Description, result.Description);
                    Assert.AreEqual(financialAidFundClassification.Guid, result.Guid);
                }

            }

            [TestMethod]
            public async Task StudentReferenceDataRepository_GetFinancialAidFundClassificationsAsync_True()
            {
                var results = await referenceDataRepo.GetFinancialAidFundClassificationsAsync(true);
                Assert.AreEqual(allFinancialAidFundClassifications.Count(), results.Count());

                foreach (var financialAidFundClassification in allFinancialAidFundClassifications)
                {
                    var result = results.FirstOrDefault(i => i.Guid == financialAidFundClassification.Guid);

                    Assert.AreEqual(financialAidFundClassification.Code, result.Code);
                    //Assert.AreEqual(financialAidFundClassification.Description, result.Description);
                    Assert.AreEqual(financialAidFundClassification.Guid, result.Guid);
                }

            }

            private StudentReferenceDataRepository BuildValidReferenceDataRepository()
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

                var records = new Collection<DataContracts.ReportFundTypes>();
                foreach (var item in allFinancialAidFundClassifications)
                {
                    DataContracts.ReportFundTypes record = new DataContracts.ReportFundTypes();
                    record.RecordGuid = item.Guid;
                    record.RftDesc = item.Description;
                    record.RftFundTypeCode = item.Code;
                    record.Recordkey = item.Code;
                    records.Add(record);
                }
                dataAccessorMock.Setup(acc => acc.BulkReadRecordAsync<DataContracts.ReportFundTypes>("REPORT.FUND.TYPES", "", true)).ReturnsAsync(records);

                cacheProviderMock.Setup<Task<Tuple<object, SemaphoreSlim>>>(x =>
                 x.GetAndLockSemaphoreAsync(It.IsAny<string>(), null))
                 .ReturnsAsync(new Tuple<object, SemaphoreSlim>(null, new SemaphoreSlim(1, 1)));

                dataAccessorMock.Setup(acc => acc.SelectAsync(It.IsAny<RecordKeyLookup[]>())).Returns<RecordKeyLookup[]>(recordKeyLookups =>
                {
                    var result = new Dictionary<string, RecordKeyLookupResult>();
                    foreach (var recordKeyLookup in recordKeyLookups)
                    {
                        var record = allFinancialAidFundClassifications.Where(e => e.Code == recordKeyLookup.PrimaryKey).FirstOrDefault();
                        result.Add(string.Join("+", new string[] { "REPORT.FUND.TYPES", record.Code }),
                            new RecordKeyLookupResult() { Guid = record.Guid });
                    }
                    return Task.FromResult(result);
                });

                // Construct repository
                referenceDataRepo = new StudentReferenceDataRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object, apiSettings);

                return referenceDataRepo;
            }
        }

        [TestClass]
        public class FinancialAidYears
        {
            Mock<IColleagueTransactionFactory> transFactoryMock;
            Mock<ICacheProvider> cacheProviderMock;
            Mock<IColleagueDataReader> dataAccessorMock;
            Mock<ILogger> loggerMock;

            IEnumerable<FinancialAidYear> allFinancialAidYears;
            string valcodeName;
            ApiSettings apiSettings;

            StudentReferenceDataRepository referenceDataRepo;

            [TestInitialize]
            public void Initialize()
            {
                loggerMock = new Mock<ILogger>();
                apiSettings = new ApiSettings("TEST");

                allFinancialAidYears = new TestStudentReferenceDataRepository().GetFinancialAidYearsAsync(false).Result;

                referenceDataRepo = BuildValidReferenceDataRepository();
                valcodeName = referenceDataRepo.BuildFullCacheKey("AllFinancialAidYears");
            }

            [TestCleanup]
            public void Cleanup()
            {
                allFinancialAidYears = null;
                valcodeName = string.Empty;
                apiSettings = null;
            }

            [TestMethod]
            public async Task FinancialAidReferenceDataRepository_GetFinancialAidYearsAsync_False()
            {
                var results = await referenceDataRepo.GetFinancialAidYearsAsync(false);
                Assert.AreEqual(allFinancialAidYears.Count(), results.Count());

                foreach (var financialAidYear in allFinancialAidYears)
                {
                    var result = results.FirstOrDefault(i => i.Guid == financialAidYear.Guid);

                    Assert.AreEqual(financialAidYear.Code, result.Code);
                    Assert.AreEqual(financialAidYear.Code, result.Description);
                    Assert.AreEqual(financialAidYear.Guid, result.Guid);
                }

            }

            [TestMethod]
            public async Task FinancialAidReferenceDataRepository_GetFinancialAidYearsAsync_True()
            {
                var results = await referenceDataRepo.GetFinancialAidYearsAsync(true);
                Assert.AreEqual(allFinancialAidYears.Count(), results.Count());

                foreach (var financialAidYear in allFinancialAidYears)
                {
                    var result = results.FirstOrDefault(i => i.Guid == financialAidYear.Guid);

                    Assert.AreEqual(financialAidYear.Code, result.Code);
                    Assert.AreEqual(financialAidYear.Code, result.Description);
                    Assert.AreEqual(financialAidYear.Guid, result.Guid);
                }

            }

            private StudentReferenceDataRepository BuildValidReferenceDataRepository()
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

                var records = new Collection<DataContracts.FaSuites>();
                foreach (var item in allFinancialAidYears)
                {
                    DataContracts.FaSuites record = new DataContracts.FaSuites();
                    record.RecordGuid = item.Guid;
                    //record = item.Description;
                    record.Recordkey = item.Code;
                    record.FaSuitesStatus = item.status;
                    records.Add(record);
                }
                dataAccessorMock.Setup(acc => acc.BulkReadRecordAsync<DataContracts.FaSuites>("FA.SUITES", "", true)).ReturnsAsync(records);

                cacheProviderMock.Setup<Task<Tuple<object, SemaphoreSlim>>>(x =>
                 x.GetAndLockSemaphoreAsync(It.IsAny<string>(), null))
                 .ReturnsAsync(new Tuple<object, SemaphoreSlim>(null, new SemaphoreSlim(1, 1)));

                dataAccessorMock.Setup(acc => acc.SelectAsync(It.IsAny<RecordKeyLookup[]>())).Returns<RecordKeyLookup[]>(recordKeyLookups =>
                {
                    var result = new Dictionary<string, RecordKeyLookupResult>();
                    foreach (var recordKeyLookup in recordKeyLookups)
                    {
                        var record = allFinancialAidYears.Where(e => e.Code == recordKeyLookup.PrimaryKey).FirstOrDefault();
                        result.Add(string.Join("+", new string[] { "FA.SUITES", record.Code }),
                            new RecordKeyLookupResult() { Guid = record.Guid });
                    }
                    return Task.FromResult(result);
                });

                // Construct repository
                referenceDataRepo = new StudentReferenceDataRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object, apiSettings);

                return referenceDataRepo;
            }
        }

        [TestClass]
        public class FinancialAidAwardPeriods
        {
            Mock<IColleagueTransactionFactory> transFactoryMock;
            Mock<ICacheProvider> cacheProviderMock;
            Mock<IColleagueDataReader> dataAccessorMock;
            Mock<ILogger> loggerMock;

            IEnumerable<FinancialAidAwardPeriod> allFinancialAidAwardPeriods;
            string valcodeName;
            ApiSettings apiSettings;

            StudentReferenceDataRepository referenceDataRepo;

            [TestInitialize]
            public void Initialize()
            {
                loggerMock = new Mock<ILogger>();
                apiSettings = new ApiSettings("TEST");

                allFinancialAidAwardPeriods = new TestStudentReferenceDataRepository().GetFinancialAidAwardPeriodsAsync(false).Result;

                referenceDataRepo = BuildValidReferenceDataRepository();
                valcodeName = referenceDataRepo.BuildFullCacheKey("AllFinancialAidAwardPeriods");
            }

            [TestCleanup]
            public void Cleanup()
            {
                allFinancialAidAwardPeriods = null;
                valcodeName = string.Empty;
                apiSettings = null;
            }

            [TestMethod]
            public async Task FinancialAidReferenceDataRepository_GetFinancialAidAwardPeriodsAsync_False()
            {
                var results = await referenceDataRepo.GetFinancialAidAwardPeriodsAsync(false);
                Assert.AreEqual(allFinancialAidAwardPeriods.Count(), results.Count());

                foreach (var financialAidAwardPeriod in allFinancialAidAwardPeriods)
                {
                    var result = results.FirstOrDefault(i => i.Guid == financialAidAwardPeriod.Guid);

                    Assert.AreEqual(financialAidAwardPeriod.Code, result.Code);
                    Assert.AreEqual(financialAidAwardPeriod.Description, result.Description);
                    Assert.AreEqual(financialAidAwardPeriod.Guid, result.Guid);
                }

            }

            [TestMethod]
            public async Task FinancialAidReferenceDataRepository_GetFinancialAidAwardPeriodsAsync_True()
            {
                var results = await referenceDataRepo.GetFinancialAidAwardPeriodsAsync(true);
                Assert.AreEqual(allFinancialAidAwardPeriods.Count(), results.Count());

                foreach (var financialAidAwardPeriod in allFinancialAidAwardPeriods)
                {
                    var result = results.FirstOrDefault(i => i.Guid == financialAidAwardPeriod.Guid);

                    Assert.AreEqual(financialAidAwardPeriod.Code, result.Code);
                    Assert.AreEqual(financialAidAwardPeriod.Description, result.Description);
                    Assert.AreEqual(financialAidAwardPeriod.Guid, result.Guid);
                }

            }

            private StudentReferenceDataRepository BuildValidReferenceDataRepository()
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

                var records = new Collection<DataContracts.AwardPeriods>();
                foreach (var item in allFinancialAidAwardPeriods)
                {
                    DataContracts.AwardPeriods record = new DataContracts.AwardPeriods();
                    record.RecordGuid = item.Guid;
                    record.AwdpDesc = item.Description;
                    record.Recordkey = item.Code;
                    records.Add(record);
                }
                dataAccessorMock.Setup(acc => acc.BulkReadRecordAsync<DataContracts.AwardPeriods>("AWARD.PERIODS", "", true)).ReturnsAsync(records);

                cacheProviderMock.Setup<Task<Tuple<object, SemaphoreSlim>>>(x =>
                 x.GetAndLockSemaphoreAsync(It.IsAny<string>(), null))
                 .ReturnsAsync(new Tuple<object, SemaphoreSlim>(null, new SemaphoreSlim(1, 1)));

                dataAccessorMock.Setup(acc => acc.SelectAsync(It.IsAny<RecordKeyLookup[]>())).Returns<RecordKeyLookup[]>(recordKeyLookups =>
                {
                    var result = new Dictionary<string, RecordKeyLookupResult>();
                    foreach (var recordKeyLookup in recordKeyLookups)
                    {
                        var record = allFinancialAidAwardPeriods.Where(e => e.Code == recordKeyLookup.PrimaryKey).FirstOrDefault();
                        result.Add(string.Join("+", new string[] { "AWARD.PERIODS", record.Code }),
                            new RecordKeyLookupResult() { Guid = record.Guid });
                    }
                    return Task.FromResult(result);
                });

                // Construct repository
                referenceDataRepo = new StudentReferenceDataRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object, apiSettings);

                return referenceDataRepo;
            }
        }

    }
}