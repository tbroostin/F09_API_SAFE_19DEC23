// Copyright 2012-2022 Ellucian Company L.P. and its affiliates.
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
using Ellucian.Colleague.Domain.Student.Entities;
using Ellucian.Colleague.Domain.Student.Repositories;
using Ellucian.Web.Cache;
using System.Collections.ObjectModel;
using Ellucian.Data.Colleague;
using slf4net;
using Ellucian.Colleague.Data.Base.Transactions;
using System.Threading;
using Ellucian.Data.Colleague.Exceptions;

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
            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task FacultyRepository_GetAsync_null_ID()
            {
                string id = null;
                var faculty = await repository.GetAsync(id);
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
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

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task FacultyRepository_GetAsync_invalid_ID()
            {
                dataReaderMock.Setup(rdr => rdr.BulkReadRecordWithInvalidKeysAndRecordsAsync<Ellucian.Colleague.Data.Base.DataContracts.Person>("PERSON", It.IsAny<string[]>(), It.IsAny<bool>()))
                    .ReturnsAsync(new Ellucian.Data.Colleague.BulkReadOutput<Person>() { BulkRecordsRead = null, InvalidKeys = new string[] { "1234567" }, InvalidRecords = new Dictionary<string, string>() { { "1234567", "" } } });
                string id = "1234567";
                var faculty = await repository.GetAsync(id);
                loggerMock.Verify(l => l.Info("Unable to retrieve faculty information for ID 1234567."));
            }

            [TestMethod]
            [ExpectedException(typeof(ColleagueSessionExpiredException))]
            public async Task FacultyRepository_GetAsync_RepositoryThrowsColleagueExpiredException()
            {
                dataReaderMock.Setup(rdr => rdr.BulkReadRecordWithInvalidKeysAndRecordsAsync<Ellucian.Colleague.Data.Base.DataContracts.Person>("PERSON", It.IsAny<string[]>(), It.IsAny<bool>()))
                    .Returns(() =>
                    {
                        throw new ColleagueSessionExpiredException("session timeout");
                    });

                string id = "1234567";
                var faculty = await repository.GetAsync(id);
            }

            [TestMethod]
            public async Task FacultyRepository_GetAsync_PersonDisplayName_Null()
            {
                var emptyStwebDefault = new StwebDefaults();
                dataReaderMock.Setup(r => r.ReadRecordAsync<StwebDefaults>("ST.PARMS", "STWEB.DEFAULTS", true)).ReturnsAsync(emptyStwebDefault);
                Domain.Student.Entities.Faculty faculty = await repository.GetAsync("1234567");
                Assert.IsNull(faculty.PersonDisplayName);
            }

            [TestMethod]
            public async Task FacultyRepository_GetAsync_PersonDisplayName()
            {
                Domain.Student.Entities.Faculty faculty = await repository.GetAsync("0000036");
                Assert.IsNotNull(faculty.PersonDisplayName);
                Assert.AreEqual("M. K. Smith", faculty.PersonDisplayName.FullName);
                Assert.AreEqual("M.", faculty.PersonDisplayName.FirstName);
                Assert.AreEqual("FACULTY", faculty.PersonDisplayName.HierarchyCode);
                Assert.AreEqual("Smith", faculty.PersonDisplayName.LastName);
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

        [TestClass]
        public class FacultyRepository_GetFacultyOfficeHoursByIdsAsync_Tests : FacultyRepositoryTests
        {
            private Mock<IFacultyRepository> facultyRepoMock;
            private List<Domain.Student.Entities.FacultyOfficeHours> facultyOfficeHours;
            private List<string> facultyIds = null;

            [TestInitialize]
            public void FacultyRepository_GetFacultyOfficeHoursByIdsAsync_Initialize()
            {
                base.Initialize();
                facultyRepoMock = new Mock<IFacultyRepository>();
                facultyIds = new List<string> { "4682", "4688", "4871" };

                facultyOfficeHours = new List<Domain.Student.Entities.FacultyOfficeHours>()
                {
                        new Domain.Student.Entities.FacultyOfficeHours()
                        {
                            FacultyId = "4682",
                            OfficeHours = new List<OfficeHours>()
                            {
                                new OfficeHours()
                                {
                                    OfficeStartDate = DateTime.Now.AddDays(-10),
                                    OfficeEndDate = DateTime.Now.AddDays(10),
                                    OfficeBuilding = "CHW Changewood Hall",
                                    OfficeRoom = "CHW",
                                    OfficeFrequency = "W"
                                },
                                new OfficeHours()
                                {
                                    OfficeStartDate = DateTime.Now.AddDays(-30),
                                    OfficeEndDate = DateTime.Now.AddDays(10),
                                    OfficeBuilding = "MS Upper Study Room",
                                    OfficeRoom = "MS",
                                    OfficeFrequency = "M"
                                }

                            }
                        },
                        new Domain.Student.Entities.FacultyOfficeHours()
                        {
                            FacultyId = "4688",
                            OfficeHours = new List<OfficeHours>()
                            {
                                new OfficeHours()
                                {
                                    OfficeStartDate = DateTime.Now.AddDays(-10),
                                    OfficeEndDate = DateTime.Now.AddDays(10),
                                    OfficeBuilding = "MSX Master Hall",
                                    OfficeRoom = "MSX",
                                    OfficeFrequency = "D"
                                },
                                new OfficeHours()
                                {
                                    OfficeStartDate = DateTime.Now.AddDays(-30),
                                    OfficeEndDate = DateTime.Now.AddDays(10),
                                    OfficeBuilding = "TGH Tiger Botany Hall",
                                    OfficeRoom = "TGH",
                                    OfficeFrequency = "W"
                                }

                            }
                        },

                        new Domain.Student.Entities.FacultyOfficeHours()
                        {
                            FacultyId = "4871",
                            OfficeHours = new List<OfficeHours>()
                            {
                                new OfficeHours()
                                {
                                    OfficeStartDate = DateTime.Now.AddDays(-10),
                                    OfficeEndDate = DateTime.Now.AddDays(10),
                                    OfficeBuilding = "FRT Fraternity Hall",
                                    OfficeRoom = "FRT",
                                    OfficeFrequency = "D"
                                },
                                new OfficeHours()
                                {
                                    OfficeStartDate = DateTime.Now.AddDays(-20),
                                    OfficeEndDate = DateTime.Now.AddDays(10),
                                    OfficeBuilding = "MSR Mountains Study Room",
                                    OfficeRoom = "MSR",
                                    OfficeFrequency = "W"
                                },
                                new OfficeHours()
                                {
                                    OfficeStartDate = DateTime.Now.AddDays(20),
                                    OfficeEndDate = DateTime.Now.AddDays(10),
                                    OfficeBuilding = "ESR Electric Study Room",
                                    OfficeRoom = "ESR",
                                    OfficeFrequency = "M"
                                }
                            }
                        }
                    };
            }

            [TestMethod]
            public async Task FacultyRepository_GetFacultyOfficeHoursByIdsAsync_Defaults()
            {
                facultyRepoMock.Setup(repo => repo.GetFacultyOfficeHoursByIdsAsync(It.IsAny<IEnumerable<string>>())).Returns(Task.FromResult<IEnumerable<Domain.Student.Entities.FacultyOfficeHours>>(facultyOfficeHours));
                var facultyOfficeHoursList = await repository.GetFacultyOfficeHoursByIdsAsync(facultyIds);
                Assert.AreEqual(facultyOfficeHours.Count(), facultyIds.Count());
            }

            [TestMethod]
            public async Task FacultyRepository_GetFacultyOfficeHoursByIdsAsync_Office_Hours_In_Range()
            {
                facultyRepoMock.Setup(repo => repo.GetFacultyOfficeHoursByIdsAsync(It.IsAny<IEnumerable<string>>())).Returns(Task.FromResult<IEnumerable<Domain.Student.Entities.FacultyOfficeHours>>(facultyOfficeHours));
                var facultyOfficeHoursList = await repository.GetFacultyOfficeHoursByIdsAsync(facultyIds);
                Assert.IsTrue(facultyOfficeHours[0].OfficeHours[0].OfficeStartDate <= DateTimeOffset.UtcNow.Date);
                Assert.IsTrue(facultyOfficeHours[0].OfficeHours[0].OfficeEndDate >= DateTimeOffset.UtcNow.Date);
                Assert.IsTrue(facultyOfficeHours[1].OfficeHours[0].OfficeStartDate <= DateTimeOffset.UtcNow.Date);
                Assert.IsTrue(facultyOfficeHours[1].OfficeHours[0].OfficeEndDate >= DateTimeOffset.UtcNow.Date);
                Assert.IsTrue(facultyOfficeHours[2].OfficeHours[0].OfficeStartDate <= DateTimeOffset.UtcNow.Date);
                Assert.IsTrue(facultyOfficeHours[2].OfficeHours[0].OfficeEndDate >= DateTimeOffset.UtcNow.Date);
                Assert.IsTrue(facultyOfficeHours[2].OfficeHours[1].OfficeStartDate <= DateTimeOffset.UtcNow.Date);
                Assert.IsTrue(facultyOfficeHours[2].OfficeHours[1].OfficeEndDate >= DateTimeOffset.UtcNow.Date);
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
            MockRecordsAsync<DataContracts.Faculty>("FACULTY", TestFacultyRepository.GetAllFacultyRecords());

            // PERSON
            MockRecordsAsync<Base.DataContracts.Person>("PERSON", TestFacultyRepository.GetAllFacultyPersonRecords());

            // ADDRESS
            MockRecordsAsync<Base.DataContracts.Address>("ADDRESS", TestFacultyRepository.GetAllFacultyAddressRecords());

            var stWebDflt = BuildStwebDefaults();
            dataReaderMock.Setup(r => r.ReadRecordAsync<StwebDefaults>("ST.PARMS", "STWEB.DEFAULTS", true)).ReturnsAsync(stWebDflt);

            // mock data reader for getting the STUDENT Name Addr Hierarchy
            dataReaderMock.Setup<Task<Ellucian.Colleague.Data.Base.DataContracts.NameAddrHierarchy>>(a =>
                a.ReadRecordAsync<Ellucian.Colleague.Data.Base.DataContracts.NameAddrHierarchy>("NAME.ADDR.HIERARCHY", "FACULTY", true))
                .ReturnsAsync(new Ellucian.Colleague.Data.Base.DataContracts.NameAddrHierarchy()
                {
                    Recordkey = "FACULTY",
                    NahNameHierarchy = new List<string>() { "Mr", "FAC", "PF" }
                });
        }

        private static StwebDefaults BuildStwebDefaults()
        {
            StwebDefaults stwebDefaults = new StwebDefaults();
            stwebDefaults.Recordkey = "STWEB.DEFAULTS";
            stwebDefaults.StwebFacAdvDispNameHier = "FACULTY";
            return stwebDefaults;
        }

        [TestClass]
        public class DepartmentalOversightRepository_SearchFacultyByName
        {
            protected Mock<ICacheProvider> cacheProviderMock;
            protected Mock<IColleagueTransactionFactory> transFactoryMock;
            protected Mock<IColleagueDataReader> dataReaderMock;
            protected Mock<IColleagueDataReader> anonymousDataReaderMock;
            protected Mock<ILogger> loggerMock;
            protected Mock<IColleagueTransactionInvoker> transManagerMock;
            ApiSettings apiSettingsMock;

            FacultyRepository facultyRepository;

            string knownStudentId1 = "111";
            string knownStudentId2 = "222";

            [TestInitialize]
            public void Initialize()
            {
                // Initialize person setup and Mock framework
                // transaction factory mock
                transFactoryMock = new Mock<IColleagueTransactionFactory>();
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

                // Mock the call to get a lookup string
                var lookupStringResponse = new GetPersonLookupStringResponse() { IndexString = ";PartialNameIndex BROWN_SL", ErrorMessage = "" };
                transManagerMock.Setup(manager => manager
                        .ExecuteAsync<GetPersonLookupStringRequest, GetPersonLookupStringResponse>(It.IsAny<GetPersonLookupStringRequest>()))
                        .ReturnsAsync(lookupStringResponse);

                cacheProviderMock.Setup<Task<Tuple<object, SemaphoreSlim>>>(x =>
                    x.GetAndLockSemaphoreAsync(It.IsAny<string>(), null))
                    .ReturnsAsync(new Tuple<object, SemaphoreSlim>(null, new SemaphoreSlim(1, 1)));

                // Build the test repository
                facultyRepository = new FacultyRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object, apiSettingsMock);
            }

            [TestCleanup]
            public void TestCleanup()
            {
                facultyRepository = null;
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task SearchFacultyByNameAsyncAsync_ThrowArgumentNullExceptionIdLastNameIsNull()
            {
                var result = await facultyRepository.SearchByNameAsync(string.Empty, string.Empty, string.Empty);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task SearchFacultyByNameAsyncAsync_ThrowArgumentNullExceptionNull()
            {
                var result = await facultyRepository.SearchByNameAsync(null);
            }

            [TestMethod]
            public async Task SearchFacultyByNameAsyncAsync_ReturnsEmptyListIfPersonSearchByNameReturnsEmptyList()
            {
                var emptyPersonIdsList = new string[] { };
                dataReaderMock.Setup(acc => acc.Select("PERSON", It.IsAny<string>())).Returns(emptyPersonIdsList);
                var lastName = "Gerbil";
                var result = await facultyRepository.SearchByNameAsync(lastName);
                Assert.AreEqual(0, result.Count());
            }

            [TestMethod]
            public async Task SearchFacultyByNameAsyncAsync_ReturnsEmptyListIfPersonSearchByNameReturnsNull()
            {
                string[] nullPersonIdsList = null;
                dataReaderMock.Setup(acc => acc.Select("PERSON", It.IsAny<string[]>(), It.IsAny<string>())).Returns(nullPersonIdsList);
                var lastName = "Gerbil";
                var result = await facultyRepository.SearchByNameAsync(lastName);
                Assert.AreEqual(0, result.Count());
            }

            [TestMethod]
            public async Task SearchFacultyByNameAsyncAsync_ReturnsEmptyListIfFacultySelectReturnsNull()
            {
                var personIdsList = new string[] { "001", "002", "003" };
                string[] nullStudentIdsList = null;
                dataReaderMock.Setup(acc => acc.Select("PERSON", It.IsAny<string>())).Returns(personIdsList);
                dataReaderMock.Setup(acc => acc.Select("STUDENTS", It.IsAny<string[]>(), It.IsAny<string>())).Returns(nullStudentIdsList);
                var lastName = "Gerbil";
                var result = await facultyRepository.SearchByNameAsync(lastName);
                Assert.AreEqual(0, result.Count());
            }

            [TestMethod]
            public async Task SearchFacultyByNameAsyncAsync_ReturnsEmptyListIfFacultySelectReturnsEmptyList()
            {
                var personIdsList = new string[] { "001", "002", "003" };
                var blankStudentIdsList = new string[] { };
                var lastName = "Gerbil";
                dataReaderMock.Setup(acc => acc.Select("PERSON", It.IsAny<string>())).Returns(personIdsList);
                dataReaderMock.Setup(acc => acc.Select("STUDENTS", It.IsAny<string[]>(), It.IsAny<string>())).Returns(blankStudentIdsList);
                var result = await facultyRepository.SearchByNameAsync(lastName);
                Assert.AreEqual(0, result.Count());
            }

            [TestMethod]
            public async Task SearchFacultyByNameAsyncAsync_ReturnsSuccess()
            {
                var personIdsList = new string[] { knownStudentId1, knownStudentId2 };
                var assignedAdviseesList = new string[] { knownStudentId1 };
                var blankStudentIdsList = new string[] { };
                var lastName = "Gerbil";
                // mock for SearchbyName
                dataReaderMock.Setup(acc => acc.SelectAsync("PERSON", It.IsAny<string>())).ReturnsAsync(personIdsList);
                // mock data for retrieving all person/student information
                MockStudentData(knownStudentId1);

                var result = await facultyRepository.SearchByNameAsync(lastName, null, null);
                Assert.AreEqual(2, result.Count());
            }

            private void MockStudentData(string studentId)
            {
                dataReaderMock.Setup(a => a.BulkReadRecordAsync<Person>(It.IsAny<string[]>(), true)).Returns((string[] s, bool b) => Task.FromResult(BuildPerson(s)));
            }

            private static Collection<Person> BuildPerson(string[] ids)
            {
                var person = new List<Person>();
                foreach (var id in ids)
                {
                    person.Add(new Person() { Recordkey = id, LastName = id });
                }
                return new Collection<Person>(person);
            }
        }
    }
}