using Ellucian.Colleague.Data.Base.Tests.Repositories;
using Ellucian.Colleague.Data.Student.DataContracts;
using Ellucian.Colleague.Data.Student.Repositories;
using Ellucian.Colleague.Domain.Student.Entities;
using Ellucian.Data.Colleague;
using Ellucian.Web.Cache;
using Ellucian.Web.Http.Configuration;
// Copyright 2017 Ellucian Company L.P. and its affiliates.
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
    public class AptitudeAssessmentsRepositoryTests : BaseRepositorySetup
    {
        private Collection<NonCourses> records;
        private List<NonCourse> _aptitudeAssessmentEntities;
        AptitudeAssessmentsRepository _aptitudeAssessmentsRepository;
        Mock<IColleagueDataReader> dataAccessorMock;
        ApiSettings apiSettings;

        [TestInitialize]
        public void Initialize() 
        {
            base.MockInitialize();
             BuildData();
           _aptitudeAssessmentsRepository = BuildAptitudeAssessmentsRepository();
        }

        [TestCleanup]
        public void Cleanup()
        {
            MockCleanup();
        }


        [TestMethod]
        public async Task AptitudeAssessmentsRepository_GetAptitudeAssessmentsAsync_True()
        {
            var result = await _aptitudeAssessmentsRepository.GetAptitudeAssessmentsAsync(true);
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public async Task AptitudeAssessmentsRepository_GetAptitudeAssessmentsAsync_False()
        {
            var result = await _aptitudeAssessmentsRepository.GetAptitudeAssessmentsAsync(false);
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public async Task AptitudeAssessmentsRepository_GetAptitudeAssessmentByIdAsync()
        {
            var id = "3d390690-7b66-4b66-820e-7610c96c5973";
            dataAccessorMock.Setup(acc => acc.ReadRecordAsync<NonCourses>("NON.COURSES", It.IsAny<string>(), true)).ReturnsAsync(records[0]);
            var result = await _aptitudeAssessmentsRepository.GetAptitudeAssessmentByIdAsync(id);
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public async Task AptitudeAssessmentsRepository_GetAptitudeAssessmentsIdFromGuidAsync()
        {
            var id = "3d390690-7b66-4b66-820e-7610c96c5973";
            dataAccessorMock.Setup(acc => acc.ReadRecordAsync<NonCourses>("NON.COURSES", It.IsAny<string>(), true)).ReturnsAsync(records[0]);
            var result = await _aptitudeAssessmentsRepository.GetAptitudeAssessmentsIdFromGuidAsync(id);
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public async Task AptitudeAssessmentsRepository_GetAptitudeAssessmentGuidsAsync_Null_Argument()
        {
            var results = await _aptitudeAssessmentsRepository.GetAptitudeAssessmentGuidsAsync(new List<string>());
            Assert.IsNull(results);
        }

        [TestMethod]
        public async Task AptitudeAssessmentsRepository_GetAptitudeAssessmentGuidsAsync_()
        {
            Dictionary<string, RecordKeyLookupResult> recordKeyLookupResult = new Dictionary<string,RecordKeyLookupResult>();
            recordKeyLookupResult.Add("A+1", new RecordKeyLookupResult() { Guid = "590663dd-cf58-4082-8b3b-3d9a055052ea", ModelName = "A+1" });
            recordKeyLookupResult.Add("B+2", new RecordKeyLookupResult() { Guid = "1c691e20-67d0-46df-b6bb-dfc04d7521bf", ModelName = "B+2" });
            
            dataAccessorMock.Setup(repo => repo.SelectAsync(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(new[] { "1", "2", "3" });
            dataAccessorMock.Setup(repo => repo.SelectAsync(It.IsAny<RecordKeyLookup[]>())).ReturnsAsync(recordKeyLookupResult);
            var results = await _aptitudeAssessmentsRepository.GetAptitudeAssessmentGuidsAsync(new List<string>() { "1", "2" });
            Assert.IsNotNull(results);
        }

        [TestMethod]
        [ExpectedException(typeof(KeyNotFoundException))]
        public async Task AptitudeAssessmentsRepository_GetAptitudeAssessmentByIdAsync_KeyNotFoundException()
        {
            var id = "3d390690-7b66-4b66-820e-7610c96c5974";
            var result = await _aptitudeAssessmentsRepository.GetAptitudeAssessmentByIdAsync(id);
        }

        [TestMethod]
        [ExpectedException(typeof(KeyNotFoundException))]
        public async Task AptitudeAssessmentsRepository_GetAptitudeAssessmentByIdAsync_DataContract_NotFound_KeyNotFoundException()
        {
            var id = "3d390690-7b66-4b66-820e-7610c96c5973";
            dataAccessorMock.Setup(acc => acc.ReadRecordAsync<NonCourses>("NON.COURSES", "DC", true)).ReturnsAsync(null);
            var result = await _aptitudeAssessmentsRepository.GetAptitudeAssessmentByIdAsync(id);
        }

        private void BuildData()
        {
            _aptitudeAssessmentEntities = new List<NonCourse>() 
            {
                new NonCourse("3d390690-7b66-4b66-820e-7610c96c5973", "Short Title 1")
                {
                    AssessmentTypeId = "Cat1",
                    CalculationMethod = "A",
                    Code = "AB",
                    Description = "Desc 1",
                    ParentAssessmentId = "GH",
                    ScoreMax = 75,
                    ScoreMin = 45
                },
                new NonCourse("3d390690-7b66-4b66-820e-7610c96c5973", "Short Title 2")
                {
                    AssessmentTypeId = "Cat1",
                    CalculationMethod = "B",
                    Code = "CD",
                    Description = "Desc 2",
                    ParentAssessmentId = "GH",
                    ScoreMax = 100,
                    ScoreMin = 50
                },
                new NonCourse("3d390690-7b66-4b66-820e-7610c96c5973", "Short Title 3")
                {
                    AssessmentTypeId = "Cat1",
                    CalculationMethod = "C",
                    Code = "EF",
                    Description = "Desc 3",
                    ParentAssessmentId = "GH",
                    ScoreMax = 90,
                    ScoreMin = 30
                }
            };
            //dataReaderMock.Setup(acc => acc.BulkReadRecordAsync<NonCourse>("", true)).ReturnsAsync(_aptitudeAssessmentEntities);
        }

        private AptitudeAssessmentsRepository BuildAptitudeAssessmentsRepository()
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

            records = new Collection<DataContracts.NonCourses>();
            foreach (var item in _aptitudeAssessmentEntities)
            {
                DataContracts.NonCourses record = new DataContracts.NonCourses();
                record.Recordkey = item.Code;
                record.RecordGuid = item.Guid;
                record.NcrsShortTitle = item.Title;
                record.NcrsDesc = item.Description;
                record.NcrsPrimaryNcrsId = item.ParentAssessmentId;
                record.NcrsMinScore = item.ScoreMin;
                record.NcrsMaxScore = item.ScoreMax;
                record.NcrsGradeUse = item.CalculationMethod;
                record.NcrsCategory = item.AssessmentTypeId;
                record.NcrsCategoryIdx = "A";
                records.Add(record);
            }
            dataAccessorMock.Setup(acc => acc.BulkReadRecordAsync<DataContracts.NonCourses>(It.IsAny<string>(), true)).ReturnsAsync(records);

            cacheProviderMock.Setup<Task<Tuple<object, SemaphoreSlim>>>(x =>
             x.GetAndLockSemaphoreAsync(It.IsAny<string>(), null))
             .ReturnsAsync(new Tuple<object, SemaphoreSlim>(null, new SemaphoreSlim(1, 1)));

            dataAccessorMock.Setup(acc => acc.SelectAsync(It.IsAny<GuidLookup[]>())).Returns<GuidLookup[]>(gla =>
            {
                var result = new Dictionary<string, GuidLookupResult>();
                foreach (var gl in gla)
                {
                    var stuprog = records.FirstOrDefault(x => x.RecordGuid == gl.Guid);
                    result.Add(gl.Guid, stuprog == null ? null : new GuidLookupResult() { Entity = "NON.COURSES", PrimaryKey = stuprog.Recordkey });
                }
                return Task.FromResult(result);
            });

            // Construct repository
            _aptitudeAssessmentsRepository = new AptitudeAssessmentsRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object);

            return _aptitudeAssessmentsRepository;
        }
    }
}
