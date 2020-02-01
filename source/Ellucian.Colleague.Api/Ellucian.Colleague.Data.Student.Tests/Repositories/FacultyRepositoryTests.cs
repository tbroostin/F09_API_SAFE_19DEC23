// Copyright 2012-2018 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Data.Base.DataContracts;
using Ellucian.Colleague.Data.Base.Tests;
using Ellucian.Colleague.Data.Base.Tests.Repositories;
using Ellucian.Colleague.Data.Student.DataContracts;
using Ellucian.Colleague.Data.Student.Repositories;
using Ellucian.Web.Http.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Data.Student.Tests.Repositories
{
    [TestClass]
    public class FacultyRepositoryTests : BaseRepositorySetup
    {
        private FacultyRepository repository;
        private ApiSettings apiSettingsMock;

        [TestInitialize]
        public void Initialize()
        {
            // Initialize Mock framework
            MockInitialize();

            // Set up data reads
            FacultyRepository_DataReader_Setup();

            // Initialize API settings
            apiSettingsMock = new ApiSettings("TEST");

            // Build the test repository
            repository = new FacultyRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object, apiSettingsMock);
        }

        [TestClass]
        public class FacultyRepository_SearchFacultyIdsAsync_Tests : FacultyRepositoryTests
        {
            [TestInitialize]
            public void FacultyRepository_SearchFacultyIdsAsync_Initialize()
            {
                base.Initialize();
                dataReaderMock.Setup(r => r.SelectAsync("FACULTY", "WITH FAC.ADVISE.FLAG NE 'Y'")).Returns(Task.FromResult(TestFacultyRepository.GetAllFacultyRecords().Take(10).Select(fac => fac.Recordkey).ToArray()));
            }

            [TestMethod]
            public async Task FacultyRepository_SearchFacultyIdsAsync_Defaults()
            {
                var facultyIds = await repository.SearchFacultyIdsAsync();
                CollectionAssert.AreEqual(TestFacultyRepository.GetAllFacultyRecords().Select(fac => fac.Recordkey).ToList(), facultyIds.ToList());
            }

            [TestMethod]
            public async Task FacultyRepository_SearchFacultyIdsAsync_faculty_who_are_advisors()
            {
                var facultyIds = await repository.SearchFacultyIdsAsync(false, true);
                CollectionAssert.AreEqual(TestFacultyRepository.GetAllFacultyRecords().Select(fac => fac.Recordkey).ToList(), facultyIds.ToList());
            }

            [TestMethod]
            public async Task FacultyRepository_SearchFacultyIdsAsync_faculty_who_are_not_advisors()
            {
                var facultyIds = await repository.SearchFacultyIdsAsync(true, false);
                Assert.AreEqual(10, facultyIds.Count());
            }

            [TestMethod]
            public async Task FacultyRepository_SearchFacultyIdsAsync_advisors_only_faculty_only()
            {
                var facultyIds = await repository.SearchFacultyIdsAsync(true, true);
                CollectionAssert.AreEqual(TestFacultyRepository.GetAllFacultyRecords().Select(fac => fac.Recordkey).ToList(), facultyIds.ToList());
            }

            [TestMethod]
            public async Task FacultyRepository_SearchFacultyIdsAsync_no_exclusions()
            {
                var facultyIds = await repository.SearchFacultyIdsAsync(false, false);
                CollectionAssert.AreEqual(TestFacultyRepository.GetAllFacultyRecords().Select(fac => fac.Recordkey).ToList(), facultyIds.ToList());
            }
        }

        [TestClass]
        public class FacultyRepository_GetAsync_Tests : FacultyRepositoryTests
        {
            [ExpectedException(typeof(ArgumentNullException))]
            [TestMethod]
            public async Task FacultyRepository_GetAsync_null_ID()
            {
                string id = null;
                var faculty = await repository.GetAsync(id);
            }

            [ExpectedException(typeof(KeyNotFoundException))]
            [TestMethod]
            public async Task FacultyRepository_GetAsync_nonexistent_ID()
            {
                string id = "INVALID_KEY";
                var faculty = await repository.GetAsync(id);
            }

            [TestMethod]
            public async Task FacultyRepository_GetAsync_valid_ID()
            {
                string id = TestFacultyRepository.GetAllFacultyRecords().First().Recordkey;
                var faculty = await repository.GetAsync(id);
                Assert.IsNotNull(faculty);
            }

            [ExpectedException(typeof(KeyNotFoundException))]
            [TestMethod]
            public async Task FacultyRepository_GetAsync_invalid_ID()
            {
                dataReaderMock.Setup(rdr => rdr.BulkReadRecordWithInvalidKeysAndRecordsAsync<Ellucian.Colleague.Data.Base.DataContracts.Person>("PERSON", It.IsAny<string[]>(), It.IsAny<bool>()))
                    .ReturnsAsync(new Ellucian.Data.Colleague.BulkReadOutput<Person>() { BulkRecordsRead = null, InvalidKeys = new string[] { "1234567" }, InvalidRecords = new Dictionary<string, string>() { { "1234567", "" } } });
                string id = "1234567";
                var faculty = await repository.GetAsync(id);
                loggerMock.Verify(l => l.Info("Unable to retrieve faculty information for ID 1234567."));
            }
        }

        [TestClass]
        public class FacultyRepository_GetFacultyByIdsAsync_Tests : FacultyRepositoryTests
        {
            [TestMethod]
            public async Task FacultyRepository_GetFacultyByIdsAsync_null_IDs()
            {
                List<string> ids = null;
                var faculty = await repository.GetFacultyByIdsAsync(ids);
                CollectionAssert.AreEqual(new List<Domain.Student.Entities.Faculty>(), faculty.ToList());
            }

            [TestMethod]
            public async Task FacultyRepository_GetFacultyByIdsAsync_no_IDs()
            {
                List<string> ids = new List<string>();
                var faculty = await repository.GetFacultyByIdsAsync(ids);
                CollectionAssert.AreEqual(new List<Domain.Student.Entities.Faculty>(), faculty.ToList());
            }


            [TestMethod]
            public async Task FacultyRepository_GetFacultyByIdsAsync_invalid_IDs()
            {
                List<string> ids = new List<string>() { "INVALID_KEY" };
                var faculty = await repository.GetFacultyByIdsAsync(ids);
                CollectionAssert.AreEqual(new List<Domain.Student.Entities.Faculty>(), faculty.ToList());
                loggerMock.Verify(l => l.Error("Unable to retrieve faculty information for IDs  INVALID_KEY."));
            }

            [TestMethod]
            public async Task FacultyRepository_GetFacultyByIdsAsync_valid_IDs()
            {
                List<string> ids = new List<string>() { TestFacultyRepository.GetAllFacultyRecords().First().Recordkey };
                var faculty = await repository.GetFacultyByIdsAsync(ids);
                Assert.IsNotNull(faculty.ElementAt(0));
            }

            [TestMethod]
            public async Task FacultyRepository_GetFacultyByIdsAsync_valid_and_invalid_ID()
            {
                dataReaderMock.Setup(rdr => rdr.BulkReadRecordWithInvalidKeysAndRecordsAsync<Ellucian.Colleague.Data.Base.DataContracts.Person>("PERSON", It.IsAny<string[]>(), It.IsAny<bool>()))
                    .ReturnsAsync(new Ellucian.Data.Colleague.BulkReadOutput<Person>() { BulkRecordsRead = new System.Collections.ObjectModel.Collection<Person>() { TestFacultyRepository.GetAllFacultyPersonRecords().First() }, InvalidKeys = new string[] { "1234567" }, InvalidRecords = new Dictionary<string, string>() { { "1234567", "" } } });
                List<string> ids = new List<string>() { TestFacultyRepository.GetAllFacultyRecords().First().Recordkey, "1234567" };
                var faculty = await repository.GetFacultyByIdsAsync(ids);
                Assert.IsNotNull(faculty.ElementAt(0));
                loggerMock.Verify(l => l.Error("Unable to retrieve faculty information for IDs  1234567."));
            }
        }

        [TestClass]
        public class FacultyRepository_GetAsync_Multiple_Tests : FacultyRepositoryTests
        {
            [TestMethod]
            public async Task FacultyRepository_GetAsync_null_IDs()
            {
                List<string> ids = null;
                var faculty = await repository.GetAsync(ids);
                CollectionAssert.AreEqual(new List<Domain.Student.Entities.Faculty>(), faculty.ToList());
            }

            [TestMethod]
            public async Task FacultyRepository_GetAsync_no_IDs()
            {
                List<string> ids = new List<string>();
                var faculty = await repository.GetAsync(ids);
                CollectionAssert.AreEqual(new List<Domain.Student.Entities.Faculty>(), faculty.ToList());
            }


            [TestMethod]
            public async Task FacultyRepository_GetAsync_invalid_IDs()
            {
                List<string> ids = new List<string>() { "INVALID_KEY" };
                var faculty = await repository.GetAsync(ids);
                CollectionAssert.AreEqual(new List<Domain.Student.Entities.Faculty>(), faculty.ToList());
                loggerMock.Verify(l => l.Error("The following requested faculty ids were not found:  INVALID_KEY"));
            }

            [TestMethod]
            public async Task FacultyRepository_GetAsync_valid_IDs()
            {
                List<string> ids = new List<string>() { TestFacultyRepository.GetAllFacultyRecords().First().Recordkey };
                var faculty = await repository.GetAsync(ids);
                Assert.IsNotNull(faculty.ElementAt(0));
            }

            [TestMethod]
            public async Task FacultyRepository_GetAsync_valid_and_invalid_IDs()
            {
                List<string> ids = new List<string>() { TestFacultyRepository.GetAllFacultyRecords().First().Recordkey, "INVALID_KEY" };
                var faculty = await repository.GetAsync(ids);
                Assert.IsNotNull(faculty.ElementAt(0));
                loggerMock.Verify(l => l.Error("The following requested faculty ids were not found:  INVALID_KEY"));
            }
        }

        /// <summary>
        /// Set up data reads
        /// </summary>
        private void FacultyRepository_DataReader_Setup()
        {
            // COUNTRIES
            MockRecordsAsync<Countries>("COUNTRIES", TestCountriesRepository.GetAllCountriesRecords());

            // FACULTY
            MockRecordsAsync<Faculty>("FACULTY", TestFacultyRepository.GetAllFacultyRecords());

            // PERSON
            MockRecordsAsync<Base.DataContracts.Person>("PERSON", TestFacultyRepository.GetAllFacultyPersonRecords());

            // ADDRESS
            MockRecordsAsync<Base.DataContracts.Address>("ADDRESS", TestFacultyRepository.GetAllFacultyAddressRecords());
        }
    }
}