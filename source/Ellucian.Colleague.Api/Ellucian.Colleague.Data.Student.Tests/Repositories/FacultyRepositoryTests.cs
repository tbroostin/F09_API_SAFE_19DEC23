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
using Ellucian.Colleague.Domain.Student.Entities;
using Ellucian.Colleague.Domain.Student.Repositories;

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
    }
}