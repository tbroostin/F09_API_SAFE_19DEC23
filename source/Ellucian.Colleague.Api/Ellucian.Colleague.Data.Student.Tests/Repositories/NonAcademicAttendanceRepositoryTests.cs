// Copyright 2017 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Data.Base.Tests.Repositories;
using Ellucian.Colleague.Data.Student.DataContracts;
using Ellucian.Colleague.Data.Student.Repositories;
using Ellucian.Colleague.Domain.Student.Entities;
using Ellucian.Web.Http.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Data.Student.Tests.Repositories
{
    [TestClass]
    public class NonAcademicAttendanceRepositoryTests : BaseRepositorySetup
    {
        NonAcademicAttendanceRepository repository;
        ApiSettings apiSettingsMock;
        private string personId;
        private Collection<PersonNaaReqmts> personNaaReqmts;
        private Collection<PersonNaaAttend> PersonNaaAttends;

        [TestInitialize]
        public async void Initialize()
        {
            // Initialize Mock framework
            MockInitialize();
            apiSettingsMock = new ApiSettings("null");
            personId = "0001234";
            var reqmtCriteriaString = "NAAPERRQ.PERSON EQ '" + personId + "'";
            personNaaReqmts = new Collection<PersonNaaReqmts>()
            {
                new PersonNaaReqmts()
                {
                    Recordkey = "1",
                    NaaperrqPerson = personId,
                    NaaperrqTerm = "TERM",
                    NaaperrqRequiredUnits = 30m,
                    NaaperrqOverrideUnits = 24m,
                    NaaperrqAttendances = new List<string>() { "111", "112", "113", "114" }
                },
                new PersonNaaReqmts()
                {
                    Recordkey = "2",
                    NaaperrqPerson = personId,
                    NaaperrqTerm = "TERM2",
                    NaaperrqRequiredUnits = 30m,
                    NaaperrqAttendances = new List<string>() { "211", "212", "213", "214" }
                }
            };
            var attendedCriteriaString = "PERNAA.PERSON EQ '" + personId + "'";
            PersonNaaAttends = new Collection<PersonNaaAttend>()
            {
                new PersonNaaAttend()
                {
                    Recordkey = "1",
                    PernaaPerson = personId,
                    PernaaEvent = "11",
                    PernaaEarnedUnits = 30m,
                },
                new PersonNaaAttend()
                {
                    Recordkey = "2",
                    PernaaPerson = personId,
                    PernaaEvent = "22",
                }
            };

            // Mock the call for getting multiple PERSON.NAA.REQMTS records
            dataReaderMock.Setup(acc => acc.BulkReadRecordAsync<PersonNaaReqmts>(reqmtCriteriaString, It.IsAny<bool>())).ReturnsAsync(personNaaReqmts);
            dataReaderMock.Setup(acc => acc.BulkReadRecordAsync<PersonNaaAttend>(attendedCriteriaString, It.IsAny<bool>())).ReturnsAsync(PersonNaaAttends);

            // Build the test repository
            repository = new NonAcademicAttendanceRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object, apiSettingsMock);
        }

        [TestCleanup]
        public void Cleanup()
        {
            repository = null;
            apiSettingsMock = null;
            personId = null;
            personNaaReqmts = null;
        }

        #region GetNonAcademicAttendanceRequirementsAsync

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task NonAcademicAttendanceRepository_GetNonAcademicAttendanceRequirementsAsync_Null_PersonId()
        {
            var requirements = await repository.GetNonacademicAttendanceRequirementsAsync(null);
        }

        [TestMethod]
        public async Task NonAcademicAttendanceRepository_GetNonAcademicAttendanceRequirementsAsync_DataReader_returns_null()
        {
            personNaaReqmts = null;
            dataReaderMock.Setup(acc => acc.BulkReadRecordAsync<PersonNaaReqmts>(It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(personNaaReqmts);
            repository = new NonAcademicAttendanceRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object, apiSettingsMock);

            var requirements = await repository.GetNonacademicAttendanceRequirementsAsync(personId);
            CollectionAssert.AreEqual(new List<NonAcademicAttendanceRequirement>(), requirements.ToList());
        }

        [TestMethod]
        public async Task NonAcademicAttendanceRepository_GetNonAcademicAttendanceRequirementsAsync_skips_null_records()
        {
            personNaaReqmts.Add(null);
            var requirements = await repository.GetNonacademicAttendanceRequirementsAsync(personId);
            Assert.AreEqual(personNaaReqmts.Count - 1, requirements.Count());
            for(int i = 0; i < personNaaReqmts.Count - 1; i++)
            {
                Assert.AreEqual(personNaaReqmts[i].Recordkey, requirements.ElementAt(i).Id);
                Assert.AreEqual(personNaaReqmts[i].NaaperrqPerson, requirements.ElementAt(i).PersonId);
                Assert.AreEqual(personNaaReqmts[i].NaaperrqTerm, requirements.ElementAt(i).TermCode);
                Assert.AreEqual(personNaaReqmts[i].NaaperrqRequiredUnits, requirements.ElementAt(i).DefaultRequiredUnits);
                Assert.AreEqual(personNaaReqmts[i].NaaperrqOverrideUnits, requirements.ElementAt(i).RequiredUnitsOverride);
            }
        }

        [TestMethod]
        public async Task NonAcademicAttendanceRepository_GetNonAcademicAttendanceRequirementsAsync_skips_corrupted_records()
        {
            personNaaReqmts.Add(new PersonNaaReqmts()
            {
                Recordkey = "3",
                NaaperrqPerson = personId,
                NaaperrqTerm = null,
            });
            var requirements = await repository.GetNonacademicAttendanceRequirementsAsync(personId);
            Assert.AreEqual(personNaaReqmts.Count - 1, requirements.Count());
            for (int i = 0; i < personNaaReqmts.Count - 1; i++)
            {
                Assert.AreEqual(personNaaReqmts[i].Recordkey, requirements.ElementAt(i).Id);
                Assert.AreEqual(personNaaReqmts[i].NaaperrqPerson, requirements.ElementAt(i).PersonId);
                Assert.AreEqual(personNaaReqmts[i].NaaperrqTerm, requirements.ElementAt(i).TermCode);
                Assert.AreEqual(personNaaReqmts[i].NaaperrqRequiredUnits, requirements.ElementAt(i).DefaultRequiredUnits);
                Assert.AreEqual(personNaaReqmts[i].NaaperrqOverrideUnits, requirements.ElementAt(i).RequiredUnitsOverride);
                CollectionAssert.AreEqual(personNaaReqmts[i].NaaperrqAttendances, requirements.ElementAt(i).NonAcademicAttendanceIds);
            }
        }

        #endregion

        #region GetNonAcademicAttendancesAsync

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task NonAcademicAttendanceRepository_GetNonAcademicAttendancesAsync_Null_PersonId()
        {
            var requirements = await repository.GetNonacademicAttendancesAsync(null);
        }

        [TestMethod]
        public async Task NonAcademicAttendanceRepository_GetNonAcademicAttendancesAsync_DataReader_returns_null()
        {
            PersonNaaAttends = null;
            dataReaderMock.Setup(acc => acc.BulkReadRecordAsync<PersonNaaAttend>(It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(PersonNaaAttends);
            repository = new NonAcademicAttendanceRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object, apiSettingsMock);

            var attendances = await repository.GetNonacademicAttendancesAsync(personId);
            CollectionAssert.AreEqual(new List<NonAcademicAttendance>(), attendances.ToList());
        }

        [TestMethod]
        public async Task NonAcademicAttendanceRepository_GetNonAcademicAttendancesAsync_skips_null_records()
        {
            PersonNaaAttends.Add(null);
            var attendances = await repository.GetNonacademicAttendancesAsync(personId);
            Assert.AreEqual(PersonNaaAttends.Count - 1, attendances.Count());
            for (int i = 0; i < PersonNaaAttends.Count - 1; i++)
            {
                Assert.AreEqual(PersonNaaAttends[i].Recordkey, attendances.ElementAt(i).Id);
                Assert.AreEqual(PersonNaaAttends[i].PernaaPerson, attendances.ElementAt(i).PersonId);
                Assert.AreEqual(PersonNaaAttends[i].PernaaEvent, attendances.ElementAt(i).EventId);
                Assert.AreEqual(PersonNaaAttends[i].PernaaEarnedUnits, attendances.ElementAt(i).UnitsEarned);
            }
        }

        [TestMethod]
        public async Task NonAcademicAttendanceRepository_GetNonAcademicAttendancesAsync_skips_corrupted_records()
        {
            PersonNaaAttends.Add(new PersonNaaAttend()
            {
                Recordkey = "3",
                PernaaPerson = personId,
                PernaaEvent = null,
            });
            var attendances = await repository.GetNonacademicAttendancesAsync(personId);
            Assert.AreEqual(PersonNaaAttends.Count - 1, attendances.Count());
            for (int i = 0; i < PersonNaaAttends.Count - 1; i++)
            {
                Assert.AreEqual(PersonNaaAttends[i].Recordkey, attendances.ElementAt(i).Id);
                Assert.AreEqual(PersonNaaAttends[i].PernaaPerson, attendances.ElementAt(i).PersonId);
                Assert.AreEqual(PersonNaaAttends[i].PernaaEvent, attendances.ElementAt(i).EventId);
                Assert.AreEqual(PersonNaaAttends[i].PernaaEarnedUnits, attendances.ElementAt(i).UnitsEarned);
            }
        }

        #endregion
    }
}
