// Copyright 2017 Ellucian Company L.P. and its affiliates.
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

namespace Ellucian.Colleague.Data.Student.Tests.Repositories
{
    [TestClass]
    public class InstructorRepositoryTests : BaseRepositorySetup
    {
        private Collection<Ellucian.Colleague.Data.Student.DataContracts.Faculty> records;
        private List<Domain.Student.Entities.Instructor> _instructorEntities;
        InstructorRepository _instructorRepository;
        Mock<IColleagueDataReader> dataAccessorMock;
        string[] ids = new[] { "RK1", "RK2", "RK3" };

        [TestInitialize]
        public void Initialize()
        {
            base.MockInitialize();
            BuildData();
            _instructorRepository = BuildAptitudeAssessmentsRepository();
          
        }

        [TestCleanup]
        public void Cleanup()
        {
            MockCleanup();
            dataAccessorMock = null;
            records = null;
            _instructorRepository = null;
            _instructorEntities = null;
           
        }

        [TestMethod]
        public async Task InstructorRepository_GetInstructorsAsync_NoRecords()
        {
            var results = await _instructorRepository.GetInstructorsAsync(It.IsAny<int>(), It.IsAny<int>(), "b87c4f7a-cc3a-4bdf-ab21-eb826da4e3cc", "HL 1", It.IsAny<bool>());
            Assert.IsNotNull(results);
        }

        [TestMethod]
        public async Task InstructorRepository_GetInstructorsAsync_WithRecords()
        {
            dataAccessorMock.Setup(i => i.SelectAsync(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(ids);
            dataAccessorMock.Setup(i => i.BulkReadRecordAsync<DataContracts.Faculty>(It.IsAny<string>(), ids, true)).ReturnsAsync(records);
            var results = await _instructorRepository.GetInstructorsAsync(0, 3, "b87c4f7a-cc3a-4bdf-ab21-eb826da4e3cc", "HL 1", It.IsAny<bool>());
            Assert.IsNotNull(results);
        }

        [TestMethod]
        public async Task InstructorRepository_GetInstructorsAsync_WithRecords_No_InstructorId()
        {
            dataAccessorMock.Setup(i => i.SelectAsync(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(ids);
            dataAccessorMock.Setup(i => i.BulkReadRecordAsync<DataContracts.Faculty>(It.IsAny<string>(), ids, true)).ReturnsAsync(records);
            var results = await _instructorRepository.GetInstructorsAsync(0, 3, "", "HL 1", It.IsAny<bool>());
            Assert.IsNotNull(results);
        }

        [TestMethod]
        public async Task InstructorRepository_GetInstructorByIdAsync()
        {
            dataAccessorMock.Setup(i => i.ReadRecordAsync<DataContracts.Faculty>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(records.First());
            var results = await _instructorRepository.GetInstructorByIdAsync("b87c4f7a-cc3a-4bdf-ab21-eb826da4e3cc");
            Assert.IsNotNull(results);
        }

        [TestMethod]
        public async Task AptitudeAssessmentsRepository_GetAptitudeAssessmentGuidsAsync()
        {
            Dictionary<string, RecordKeyLookupResult> recordKeyLookupResult = new Dictionary<string, RecordKeyLookupResult>();
            recordKeyLookupResult.Add("A+1", new RecordKeyLookupResult() { Guid = "590663dd-cf58-4082-8b3b-3d9a055052ea", ModelName = "A+1" });
            recordKeyLookupResult.Add("B+2", new RecordKeyLookupResult() { Guid = "1c691e20-67d0-46df-b6bb-dfc04d7521bf", ModelName = "B+2" });

            dataAccessorMock.Setup(repo => repo.SelectAsync(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(new[] { "1", "2", "3" });
            dataAccessorMock.Setup(repo => repo.SelectAsync(It.IsAny<RecordKeyLookup[]>())).ReturnsAsync(recordKeyLookupResult);
            var results = await _instructorRepository.GetPersonGuidsAsync(new List<string>() { "1", "2" });
            Assert.IsNotNull(results);
        }

        [TestMethod]
        public async Task AptitudeAssessmentsRepository_GetAptitudeAssessmentGuidsAsync_NullList()
        {
            var results = await _instructorRepository.GetPersonGuidsAsync(null);
            Assert.IsNull(results);
        }

        [TestMethod]
        public async Task AptitudeAssessmentsRepository_GetAptitudeAssessmentGuidsAsync_EmptyList()
        {
            var results = await _instructorRepository.GetPersonGuidsAsync(new List<string>());
            Assert.IsNull(results);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task InstructorRepository_GetInstructorByIdAsync_EmptyGuid_KeyNotFoundException()
        {
            var results = await _instructorRepository.GetInstructorByIdAsync("");
        }

        [TestMethod]
        [ExpectedException(typeof(KeyNotFoundException))]
        public async Task InstructorRepository_GetInstructorByIdAsync_WrongGuid_KeyNotFoundException()
        {
            var results = await _instructorRepository.GetInstructorByIdAsync("ABC");
        }

        [TestMethod]
        [ExpectedException(typeof(KeyNotFoundException))]
        public async Task InstructorRepository_GetInstructorByIdAsync_InvalidEntity_KeyNotFoundException()
        {
            var guid = "c87c4f7a-cc3a-4bdf-ab21-eb826da4e3cc";
            dataAccessorMock.Setup(acc => acc.SelectAsync(It.IsAny<GuidLookup[]>())).Returns<GuidLookup[]>(gla =>
            {
                var result = new Dictionary<string, GuidLookupResult>();
                foreach (var gl in gla)
                {
                    var stuprog = records.FirstOrDefault(x => x.RecordGuid == gl.Guid);
                    result.Add(gl.Guid, stuprog == null ? null : new GuidLookupResult() { Entity = "PERSON", PrimaryKey = stuprog.Recordkey });
                }
                return Task.FromResult(result);
            });

            var results = await _instructorRepository.GetInstructorByIdAsync(guid);
        }


        [TestMethod]
        public async Task InstructorRepository_GetInstructorByIdAsync_ValidEntity_KeyNotFoundException()
        {
            var guid = "b87c4f7a-cc3a-4bdf-ab21-eb826da4e3cc";
            var expected = records.FirstOrDefault(i => i.RecordGuid == guid);
            Assert.IsNotNull(expected);
            
            dataAccessorMock.Setup(i => i.ReadRecordAsync<DataContracts.Faculty>(It.IsAny<string>(), expected.Recordkey, true)).ReturnsAsync(expected);

            dataAccessorMock.Setup(acc => acc.SelectAsync(It.IsAny<GuidLookup[]>())).Returns<GuidLookup[]>(gla =>
            {
                var result = new Dictionary<string, GuidLookupResult>();
                foreach (var gl in gla)
                {
                    var stuprog = records.FirstOrDefault(x => x.RecordGuid == gl.Guid);
                    result.Add(gl.Guid, stuprog == null ? null : new GuidLookupResult() { Entity = "FACULTY", PrimaryKey = expected.Recordkey });
                }
                return Task.FromResult(result);
            });

            var actual = await _instructorRepository.GetInstructorByIdAsync(guid);
            Assert.IsNotNull(actual);
            
            Assert.AreEqual(expected.RecordGuid, actual.RecordGuid);
            Assert.AreEqual(expected.Recordkey, actual.RecordKey);
            Assert.AreEqual(expected.FacHomeLocation, actual.HomeLocation);
        }


        [TestMethod]
        [ExpectedException(typeof(KeyNotFoundException))]
        public async Task InstructorRepository_GetInstructorByIdAsync_DataContract_Null_KeyNotFoundException()
        {
            dataAccessorMock.Setup(i => i.ReadRecordAsync<DataContracts.Faculty>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(null);
            var results = await _instructorRepository.GetInstructorByIdAsync("b87c4f7a-cc3a-4bdf-ab21-eb826da4e3cc");
        }

        private void BuildData()
        {
            _instructorEntities = new List<Domain.Student.Entities.Instructor>() 
            {
                new Domain.Student.Entities.Instructor("b87c4f7a-cc3a-4bdf-ab21-eb826da4e3cc", "RK1")
                {
                    ContractType = "CT 1",
                    HomeLocation = "HL 1",
                    SpecialStatus = "SS 1",
                    Departments = new List<Domain.Student.Entities.FacultyDeptLoad>()
                    {
                        new Domain.Student.Entities.FacultyDeptLoad()
                        {
                            DeptPcts = 50,
                            FacultyDepartment = "Dept 1"
                        } 
                    }
                },
                new Domain.Student.Entities.Instructor("29ee36ef-08f3-435d-b518-cc6c7133e17b", "RK2")
                {
                    ContractType = "CT 2",
                    HomeLocation = "HL 2",
                    SpecialStatus = "SS 2",
                    Departments = new List<Domain.Student.Entities.FacultyDeptLoad>()
                    {
                        new Domain.Student.Entities.FacultyDeptLoad()
                        {
                            DeptPcts = 50,
                            FacultyDepartment = "Dept 2"
                        } 
                    }
                },
                new Domain.Student.Entities.Instructor("130db3a2-2301-4cb2-aec4-eafae19fd083", "RK3")
                {
                    ContractType = "CT 3",
                    HomeLocation = "HL 3",
                    SpecialStatus = "SS 3",
                    Departments = null
                }
            };
        }

        private InstructorRepository BuildAptitudeAssessmentsRepository()
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

            records = new Collection<Ellucian.Colleague.Data.Student.DataContracts.Faculty>();
            foreach (var item in _instructorEntities)
            {
                DataContracts.Faculty record = new DataContracts.Faculty();
                record.RecordGuid = item.RecordGuid;
                record.Recordkey = item.RecordKey;
                if (item.Departments != null)
                {
                    record.DeptLoadEntityAssociation = new List<DataContracts.FacultyDeptLoad>() 
                    {
                        new DataContracts.FacultyDeptLoad()
                        {
                            FacDeptPctsAssocMember = item.Departments.First().DeptPcts,
                            FacDeptsAssocMember = item.Departments.First().FacultyDepartment
                        }
                    };
                };
                record.FacHomeLocation = item.HomeLocation;
                record.FacSpecialStatus = item.SpecialStatus;
                record.FacContractType = item.ContractType;
                records.Add(record);
            }
            dataAccessorMock.Setup(acc => acc.BulkReadRecordAsync<Ellucian.Colleague.Data.Student.DataContracts.Faculty>(It.IsAny<string>(), true)).ReturnsAsync(records);

            cacheProviderMock.Setup<Task<Tuple<object, SemaphoreSlim>>>(x =>
             x.GetAndLockSemaphoreAsync(It.IsAny<string>(), null))
             .ReturnsAsync(new Tuple<object, SemaphoreSlim>(null, new SemaphoreSlim(1, 1)));

            dataAccessorMock.Setup(acc => acc.SelectAsync(It.IsAny<GuidLookup[]>())).Returns<GuidLookup[]>(gla =>
            {
                var result = new Dictionary<string, GuidLookupResult>();
                foreach (var gl in gla)
                {
                    var stuprog = records.FirstOrDefault(x => x.RecordGuid == gl.Guid);
                    result.Add(gl.Guid, stuprog == null ? null : new GuidLookupResult() { Entity = "FACULTY", PrimaryKey = stuprog.Recordkey });
                }
                return Task.FromResult(result);
            });

            // Construct repository
            _instructorRepository = new InstructorRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object, apiSettings);

            return _instructorRepository;
        }
    }
}
