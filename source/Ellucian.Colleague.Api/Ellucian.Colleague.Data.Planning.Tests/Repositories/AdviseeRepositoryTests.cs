// Copyright 2012-2019 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Data.Base.DataContracts;
using Ellucian.Colleague.Data.Base.Transactions;
using Ellucian.Colleague.Data.Planning.Repositories;
using Ellucian.Colleague.Data.Planning.Transactions;
using Ellucian.Colleague.Data.Student.DataContracts;
using Ellucian.Colleague.Data.Student.Repositories;
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
using System.Runtime.Caching;
using System.Threading;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Data.Planning.Tests.Repositories
{
    [TestClass]
    public class AdviseeRepositoryTests
    {
        private static Collection<Base.DataContracts.Person> BuildPeople(string[] ids)
        {
            var people = new List<Base.DataContracts.Person>();
            foreach (var id in ids)
            {
                people.Add(new Base.DataContracts.Person() { Recordkey = id, LastName = id });
            }
            return new Collection<Base.DataContracts.Person>(people);
        }

        private static Collection<Students> BuildStudents(string[] ids)
        {
            var students = new List<Students>();
            foreach (var id in ids)
            {
                students.Add(new Students() { Recordkey = id });
            }
            return new Collection<Students>(students);
        }

        private static Collection<Base.DataContracts.PersonSt> BuildPersonStRecords(string[] ids)
        {
            var students = new List<Base.DataContracts.PersonSt>();
            foreach (var id in ids)
            {
                students.Add(new Base.DataContracts.PersonSt() { Recordkey = id });
            }
            return new Collection<Base.DataContracts.PersonSt>(students);
        }

        [TestClass]
        public class AdviseeRepository_Get
        {
            protected Mock<IColleagueTransactionFactory> transFactoryMock;
            protected Mock<ObjectCache> localCacheMock;
            protected Mock<ICacheProvider> cacheProviderMock;
            protected Mock<IColleagueDataReader> dataReaderMock;
            protected Mock<IColleagueDataReader> anonymousDataReaderMock;
            protected Mock<ILogger> loggerMock;
            protected Mock<IColleagueTransactionInvoker> transManagerMock;

            protected Mock<StudentRepository> studentRepoMock;
            ApiSettings apiSettingsMock;
            AdviseeRepository adviseeRepository;

            string unknownStudentId = "999";
            string knownStudentId1 = "111";
            string knownStudentId2 = "222";
            string[] onlyStudents = new string[] {"222" };

            [TestInitialize]
            public void Initialize()
            {
                // Initialize person setup and Mock framework
                // transaction factory mock
                transFactoryMock = new Mock<IColleagueTransactionFactory>();
                // Cache Mock
                localCacheMock = new Mock<ObjectCache>();
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

                var dflts = new Dflts()
                {
                    DfltsFixedLenPerson = "7"
                };
                dataReaderMock.Setup(r => r.ReadRecordAsync<Dflts>("CORE.PARMS", "DEFAULTS", It.IsAny<bool>())).ReturnsAsync(dflts);

                // set up the mock data accessor 
                dataReaderMock.Setup(a => a.ReadRecordAsync<Student.DataContracts.Students>(knownStudentId1, true)).ReturnsAsync(new Student.DataContracts.Students() { Recordkey = knownStudentId1, StuAcadPrograms = null });
                dataReaderMock.Setup(a => a.ReadRecordAsync<Student.DataContracts.Students>(knownStudentId2, true)).ReturnsAsync(new Student.DataContracts.Students() { Recordkey = knownStudentId2, StuAcadPrograms = null });
                dataReaderMock.Setup(a => a.ReadRecordAsync<Student.DataContracts.Students>(unknownStudentId, true)).ReturnsAsync((Student.DataContracts.Students)null);

                dataReaderMock.Setup(a => a.ReadRecordAsync<Base.DataContracts.PersonSt>(knownStudentId1, true)).ReturnsAsync(new Base.DataContracts.PersonSt() { Recordkey = knownStudentId1, PstAdvisement = new List<string>() });
                dataReaderMock.Setup(a => a.ReadRecordAsync<Base.DataContracts.PersonSt>(knownStudentId2, true)).ReturnsAsync(new Base.DataContracts.PersonSt() { Recordkey = knownStudentId1, PstAdvisement = new List<string>() });
                dataReaderMock.Setup(a => a.ReadRecordAsync<Base.DataContracts.PersonSt>(unknownStudentId, true)).ReturnsAsync((Base.DataContracts.PersonSt)null);

                dataReaderMock.Setup(a => a.ReadRecordAsync<Base.DataContracts.Person>(knownStudentId1, true)).ReturnsAsync(new Base.DataContracts.Person() { Recordkey = knownStudentId1, LastName = "Last1" });
                dataReaderMock.Setup(a => a.ReadRecordAsync<Base.DataContracts.Person>(knownStudentId2, true)).ReturnsAsync(new Base.DataContracts.Person() { Recordkey = knownStudentId2, LastName = "Last2" });
                dataReaderMock.Setup(a => a.ReadRecordAsync<Base.DataContracts.Person>(unknownStudentId, true)).ReturnsAsync((Base.DataContracts.Person)null);

                dataReaderMock.Setup(a => a.BulkReadRecordAsync<Base.DataContracts.Person>(new string[] { "111", "222" }, true)).ReturnsAsync(new Collection<Base.DataContracts.Person>() { new Base.DataContracts.Person() { Recordkey = "111", LastName = "brown" }, new Base.DataContracts.Person() { Recordkey = "222", LastName = "smith" } });
                dataReaderMock.Setup(a => a.BulkReadRecordAsync<Base.DataContracts.Person>(new string[] { "111" }, true)).ReturnsAsync(new Collection<Base.DataContracts.Person>() { new Base.DataContracts.Person() { Recordkey = "111", LastName = "brown" } });
                dataReaderMock.Setup(a => a.BulkReadRecordAsync<Base.DataContracts.Person>(new string[] { "222" }, true)).ReturnsAsync(new Collection<Base.DataContracts.Person>() { new Base.DataContracts.Person() { Recordkey = "222", LastName = "smith" } });
                dataReaderMock.Setup(a => a.BulkReadRecordAsync<Base.DataContracts.Person>(new string[] { "999" }, true)).ReturnsAsync(new Collection<Base.DataContracts.Person>());

                dataReaderMock.Setup(a => a.BulkReadRecordAsync<Student.DataContracts.Students>(new string[] { "999" }, true)).ReturnsAsync(new Collection<Student.DataContracts.Students>());
                dataReaderMock.Setup(a => a.BulkReadRecordAsync<Student.DataContracts.Students>(new string[] { "111", "222" }, true)).ReturnsAsync(new Collection<Student.DataContracts.Students>() { new Student.DataContracts.Students() { Recordkey = "111", StuAcadPrograms = null }, new Student.DataContracts.Students() { Recordkey = "222", StuAcadPrograms = null } });
                dataReaderMock.Setup(a => a.BulkReadRecordAsync<Student.DataContracts.Students>(new string[] { "111" }, true)).ReturnsAsync(new Collection<Student.DataContracts.Students>() { new Student.DataContracts.Students() { Recordkey = "111", StuAcadPrograms = null } });
                dataReaderMock.Setup(a => a.BulkReadRecordAsync<Student.DataContracts.Students>(new string[] { "222" }, true)).ReturnsAsync(new Collection<Student.DataContracts.Students>() { new Student.DataContracts.Students() { Recordkey = "222", StuAcadPrograms = new List<string>() {"MATH.BA"}}});
                dataReaderMock.Setup(a => a.BulkReadRecordAsync<Base.DataContracts.PersonSt>(It.IsAny<string[]>(), true)).ReturnsAsync(new Collection<Base.DataContracts.PersonSt>());

                dataReaderMock.Setup(d => d.BulkReadRecordAsync<Student.DataContracts.StudentAdvisement>(It.IsAny<string[]>(), true)).ReturnsAsync((Collection<Student.DataContracts.StudentAdvisement>)null);
                dataReaderMock.Setup(d => d.BulkReadRecordAsync<Base.DataContracts.ForeignPerson>(It.IsAny<string[]>(), true)).ReturnsAsync((Collection<Base.DataContracts.ForeignPerson>)null);
                dataReaderMock.Setup(d => d.BulkReadRecordAsync<Student.DataContracts.Applicants>(It.IsAny<string[]>(), true)).ReturnsAsync((Collection<Student.DataContracts.Applicants>)null);

                dataReaderMock.Setup<string[]>(a => a.Select("DEGREE_PLAN", "DP.STUDENT.ID EQ '" + knownStudentId1 + "'")).Returns(new string[] { "1" });
                dataReaderMock.Setup<string[]>(a => a.Select("DEGREE_PLAN", "DP.STUDENT.ID EQ '" + knownStudentId1 + "' '" + knownStudentId2 + "'")).Returns(new string[] { "1", "2" });

                // Mock the read of the Preferred Name Address Hierarchy for the Preferred Name
                dataReaderMock.Setup<Ellucian.Colleague.Data.Base.DataContracts.NameAddrHierarchy>(a =>
                    a.ReadRecord<Ellucian.Colleague.Data.Base.DataContracts.NameAddrHierarchy>("NAME.ADDR.HIERARCHY", "PREFERRED", true))
                    .Returns(new Ellucian.Colleague.Data.Base.DataContracts.NameAddrHierarchy()
                    {
                        Recordkey = "PREFERRED",
                        NahNameHierarchy = new List<string>() { "MA", "XYZ", "PF" }
                    });

                // Mock the call for getting the preferred address
                transManagerMock.Setup<TxGetHierarchyAddressResponse>(
                    manager => manager.Execute<TxGetHierarchyAddressRequest, TxGetHierarchyAddressResponse>(
                        It.IsAny<TxGetHierarchyAddressRequest>())
                    ).Returns<TxGetHierarchyAddressRequest>(request =>
                    {
                        return new TxGetHierarchyAddressResponse() { OutAddressId = "1111", OutAddressLabel = new List<string>() { "AdressLabel" } };
                    });

                // Mock the call to get a lookup string
                var lookupStringResponse = new GetPersonLookupStringResponse() { IndexString = ";PartialNameIndex BROWN_SL", ErrorMessage = "" };
                transManagerMock.Setup(manager => manager
                        .ExecuteAsync<GetPersonLookupStringRequest, GetPersonLookupStringResponse>(It.IsAny<GetPersonLookupStringRequest>()))
                        .ReturnsAsync(lookupStringResponse);

                cacheProviderMock.Setup<Task<Tuple<object, SemaphoreSlim>>>(x =>
                    x.GetAndLockSemaphoreAsync(It.IsAny<string>(), null))
                    .ReturnsAsync(new Tuple<object, SemaphoreSlim>(null, new SemaphoreSlim(1, 1)));

                // Build the test repository
                adviseeRepository = BuildMockAdviseeRepository();
            }

            private AdviseeRepository BuildMockAdviseeRepository()
            {

                AdviseeRepository repository = new AdviseeRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object, apiSettingsMock);

                return repository;
            }

            [TestCleanup]
            public void TestCleanup()
            {
                adviseeRepository = null;
            }

            [Ignore]
            [TestMethod]
            // generic test for a known ID, verify that the same ID is passed out.
            public async Task Get_StudentByID()
            {
                dataReaderMock.Setup(a => a.SelectAsync("PERSON", knownStudentId1)).ReturnsAsync(new string[] { knownStudentId1 });
                Domain.Student.Entities.PlanningStudent student = await adviseeRepository.GetAsync(knownStudentId1);
                Assert.AreEqual(knownStudentId1, student.Id);
            }

            [Ignore]
            [TestMethod]
            // generic test for an unknown ID, verify not found
            public async Task Get_StudentByID_UnknownID()
            {
                dataReaderMock.Setup(a => a.Select("PERSON", unknownStudentId)).Returns(new string[] { });
                Domain.Student.Entities.PlanningStudent student = await adviseeRepository.GetAsync(unknownStudentId);
                Assert.IsNull(student);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task Get_StudentByID_ThrowsExceptionIfAdviseeIdEmpty()
            {
                await adviseeRepository.GetAsync(string.Empty);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task Get_StudentByID_ThrowsExceptionIfAdviseeIdNull()
            {
                await adviseeRepository.GetAsync(null);
            }

            [TestMethod]
            public async Task Get_StudentIds_Multiple()
            {
                dataReaderMock.Setup(a => a.SelectAsync("DEGREE_PLAN", It.IsAny<string[]>(), It.IsAny<string>())).ReturnsAsync(new string[] { knownStudentId1, knownStudentId2 });
                dataReaderMock.Setup(a => a.SelectAsync("PERSON", It.IsAny<string[]>(), It.IsAny<string>())).ReturnsAsync(new string[] { knownStudentId1, knownStudentId2 });
                dataReaderMock.Setup(a => a.SelectAsync("STUDENTS", It.IsAny<string[]>(), It.IsAny<string>())).ReturnsAsync(new string[] { knownStudentId1, knownStudentId2 });
                IEnumerable<Domain.Student.Entities.PlanningStudent> students = await adviseeRepository.GetAsync(new List<string> { knownStudentId1, knownStudentId2 });
                Assert.AreEqual(2, students.Count());
            }

            [TestMethod]
            public async Task Get_StudentIds_Multiple_LimitToStudents()
            {
                dataReaderMock.Setup(a => a.SelectAsync("DEGREE_PLAN", It.IsAny<string[]>(), It.IsAny<string>())).ReturnsAsync(new string[] { knownStudentId1, knownStudentId2 });
                dataReaderMock.Setup(a => a.SelectAsync("PERSON", onlyStudents, "BY LAST.NAME BY FIRST.NAME BY MIDDLE.NAME")).ReturnsAsync(new string[] { knownStudentId2 });
                dataReaderMock.Setup(a => a.SelectAsync("STUDENTS", It.IsAny<string[]>(), string.Empty)).ReturnsAsync(new string[] { knownStudentId2 });
                IEnumerable<Domain.Student.Entities.PlanningStudent> students = await adviseeRepository.GetAsync(new List<string> { knownStudentId1, knownStudentId2 });
                Assert.AreEqual(1, students.Count());
            }

            [TestMethod]
            public async Task Get_Students_UnknownID_ReturnsNone()
            {
                dataReaderMock.Setup(a => a.SelectAsync("DEGREE_PLAN", It.IsAny<string[]>(), It.IsAny<string>())).ReturnsAsync(new string[] { unknownStudentId });
                dataReaderMock.Setup(a => a.SelectAsync("PERSON", It.IsAny<string[]>(), It.IsAny<string>())).ReturnsAsync(new string[] { });
                dataReaderMock.Setup(a => a.SelectAsync("STUDENTS", It.IsAny<string[]>(), It.IsAny<string>())).ReturnsAsync(new string[] { });
                IEnumerable<Domain.Student.Entities.PlanningStudent> students = await adviseeRepository.GetAsync(new List<string>() { unknownStudentId });
                Assert.AreEqual(0, students.Count());
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public async Task Get_Students_ThrowsExceptionIfAdviseeIdsEmpty()
            {
                await adviseeRepository.GetAsync(new List<string> { });
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public async Task Get_Students_ThrowsExceptionIfAdviseeIdsNull()
            {
                var adviseeIds = new List<string>() { };
                adviseeIds = null;
                await adviseeRepository.GetAsync(adviseeIds);
            }

        }

        [TestClass]
        public class AdviseeRepository_SearchByName
        {

            protected Mock<IColleagueTransactionFactory> transFactoryMock;
            protected Mock<ObjectCache> localCacheMock;
            protected Mock<ICacheProvider> cacheProviderMock;
            protected Mock<IColleagueDataReader> dataReaderMock;
            protected Mock<IColleagueDataReader> anonymousDataReaderMock;
            protected Mock<ILogger> loggerMock;
            protected Mock<IColleagueTransactionInvoker> transManagerMock;
            ApiSettings apiSettingsMock;
            protected Mock<StudentRepository> studentRepoMock;

            AdviseeRepository adviseeRepository;

            string knownStudentId1 = "111";
            string knownStudentId2 = "222";

            [TestInitialize]
            public void Initialize()
            {
                // Initialize person setup and Mock framework
                // transaction factory mock
                transFactoryMock = new Mock<IColleagueTransactionFactory>();
                // Cache Mock
                localCacheMock = new Mock<ObjectCache>();
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
                adviseeRepository = BuildMockAdviseeRepository();
            }

            private AdviseeRepository BuildMockAdviseeRepository()
            {

                AdviseeRepository repository = new AdviseeRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object, apiSettingsMock);

                return repository;
            }

            private void MockStudentData(string studentId)
            {
                dataReaderMock.Setup(a => a.BulkReadRecordAsync<Base.DataContracts.Person>(It.IsAny<string[]>(), true)).Returns((string[] s, bool b) => Task.FromResult(BuildPeople(s)));
                dataReaderMock.Setup(a => a.BulkReadRecordAsync<Student.DataContracts.Students>(It.IsAny<string[]>(), true)).Returns((string[] s, bool b) => Task.FromResult(BuildStudents(s)));
                dataReaderMock.Setup(a => a.BulkReadRecordAsync<Base.DataContracts.PersonSt>(It.IsAny<string[]>(), true)).Returns((string[] s, bool b) => Task.FromResult(BuildPersonStRecords(s)));
                dataReaderMock.Setup(d => d.BulkReadRecordAsync<Student.DataContracts.StudentAdvisement>(It.IsAny<string[]>(), true)).ReturnsAsync((Collection<Student.DataContracts.StudentAdvisement>)null);
                dataReaderMock.Setup(d => d.BulkReadRecordAsync<Base.DataContracts.ForeignPerson>(It.IsAny<string[]>(), true)).ReturnsAsync((Collection<Base.DataContracts.ForeignPerson>)null);
                dataReaderMock.Setup(d => d.BulkReadRecordAsync<Student.DataContracts.Applicants>(It.IsAny<string[]>(), true)).ReturnsAsync((Collection<Student.DataContracts.Applicants>)null);

                // Mock the read of the Preferred Name Address Hierarchy for the Preferred Name
                dataReaderMock.Setup<Ellucian.Colleague.Data.Base.DataContracts.NameAddrHierarchy>(a =>
                    a.ReadRecord<Ellucian.Colleague.Data.Base.DataContracts.NameAddrHierarchy>("NAME.ADDR.HIERARCHY", "PREFERRED", true))
                    .Returns(new Ellucian.Colleague.Data.Base.DataContracts.NameAddrHierarchy()
                    {
                        Recordkey = "PREFERRED",
                        NahNameHierarchy = new List<string>() { "MA", "XYZ", "PF" }
                    });

                //// Mock the call for getting the preferred address
                transManagerMock.Setup<TxGetHierarchyAddressResponse>(
                    manager => manager.Execute<TxGetHierarchyAddressRequest, TxGetHierarchyAddressResponse>(
                        It.IsAny<TxGetHierarchyAddressRequest>())
                    ).Returns<TxGetHierarchyAddressRequest>(request =>
                    {
                        return new TxGetHierarchyAddressResponse() { OutAddressId = studentId, OutAddressLabel = new List<string>() { "AdressLabel" } };
                    });

            }

            private void MockStudentData(string studentId1, string studentId2)
            {
                // Mock individual reads for each student
                MockStudentData(studentId1);
                MockStudentData(studentId2);
                // Mock bulk reads that must return a collection
                dataReaderMock.Setup(a => a.BulkReadRecordAsync<Base.DataContracts.Person>(It.IsAny<string[]>(), true))
                    .ReturnsAsync(new Collection<Base.DataContracts.Person>() 
                    { 
                        new Base.DataContracts.Person() { Recordkey = studentId1, LastName = "last" + studentId1 }, 
                        new Base.DataContracts.Person() { Recordkey = studentId2, LastName = "last" + studentId2 } 
                    });
                dataReaderMock.Setup(a => a.BulkReadRecordAsync<Student.DataContracts.Students>(It.IsAny<string[]>(), true))
                    .ReturnsAsync(new Collection<Student.DataContracts.Students>() 
                    { 
                        new Student.DataContracts.Students() { Recordkey = studentId1, StuAcadPrograms = null }, 
                        new Student.DataContracts.Students() { Recordkey = studentId2, StuAcadPrograms = null } 
                    });
            }

            [TestCleanup]
            public void TestCleanup()
            {
                adviseeRepository = null;
            }

            [TestMethod]
            public async Task SearchByName_ReturnsEmptyListIfCalledWithEmptyNames()
            {
                var result = await adviseeRepository.SearchByNameAsync(string.Empty, string.Empty, string.Empty);
                Assert.AreEqual(0, result.Count());
            }

            [TestMethod]
            public async Task SearchByName_ReturnsEmptyListIfCalledWithNullNames()
            {
                var result = await adviseeRepository.SearchByNameAsync(null);
                Assert.AreEqual(0, result.Count());
            }

            [TestMethod]
            public async Task SearchByName_ReturnsEmptyListIfPersonSearchByNameReturnsEmptyList()
            {
                var emptyPersonIdsList = new string[] { };
                dataReaderMock.Setup(acc => acc.Select("PERSON", It.IsAny<string>())).Returns(emptyPersonIdsList);
                var lastName = "Gerbil";
                var result = await adviseeRepository.SearchByNameAsync(lastName);
                Assert.AreEqual(0, result.Count());
            }

            [TestMethod]
            public async Task SearchByName_ReturnsEmptyListIfPersonSearchByNameReturnsNull()
            {
                string[] nullPersonIdsList = null;
                dataReaderMock.Setup(acc => acc.Select("PERSON", It.IsAny<string[]>(), It.IsAny<string>())).Returns(nullPersonIdsList);
                var lastName = "Gerbil";
                var result = await adviseeRepository.SearchByNameAsync(lastName);
                Assert.AreEqual(0, result.Count());
            }

            [TestMethod]
            public async Task SearchByName_ReturnsEmptyListIfStudentSelectReturnsNull()
            {
                var personIdsList = new string[] { "001", "002", "003" };
                string[] nullStudentIdsList = null;
                dataReaderMock.Setup(acc => acc.Select("PERSON", It.IsAny<string>())).Returns(personIdsList);
                dataReaderMock.Setup(acc => acc.Select("STUDENTS", It.IsAny<string[]>(), It.IsAny<string>())).Returns(nullStudentIdsList);
                var lastName = "Gerbil";
                var result = await adviseeRepository.SearchByNameAsync(lastName);
                Assert.AreEqual(0, result.Count());
            }

            [TestMethod]
            public async Task SearchByName_ReturnsEmptyListIfStudentSelectReturnsEmptyList()
            {
                var personIdsList = new string[] { "001", "002", "003" };
                var blankStudentIdsList = new string[] { };
                var lastName = "Gerbil";
                dataReaderMock.Setup(acc => acc.Select("PERSON", It.IsAny<string>())).Returns(personIdsList);
                dataReaderMock.Setup(acc => acc.Select("STUDENTS", It.IsAny<string[]>(), It.IsAny<string>())).Returns(blankStudentIdsList);
                var result = await adviseeRepository.SearchByNameAsync(lastName);
                Assert.AreEqual(0, result.Count());
            }

            [TestMethod]
            public async Task SearchByName_LimitsReturnedItemsToAssignedAdvisees()
            {
                var personIdsList = new string[] { knownStudentId1, knownStudentId2 };
                var assignedAdviseesList = new string[] { knownStudentId1 };
                var blankStudentIdsList = new string[] { };
                var lastName = "Gerbil";
                // mock for SearchbyName
                dataReaderMock.Setup(acc => acc.SelectAsync("PERSON", It.IsAny<string>())).ReturnsAsync(personIdsList);
                // mock for FilterByEntity
                dataReaderMock.Setup(acc => acc.SelectAsync("STUDENTS", It.IsAny<string[]>(), It.IsAny<string>())).ReturnsAsync(personIdsList);
                // mock for GetCurrentPage, students who have requested review
                dataReaderMock.Setup(a => a.SelectAsync("DEGREE_PLAN", It.IsAny<string>(), It.IsAny<string[]>(), "?", true, It.IsAny<int>())).ReturnsAsync(blankStudentIdsList);
                // mock for GetCurrentPage, all provided students (limited by advisees in this case)
                dataReaderMock.Setup(acc => acc.SelectAsync("STUDENTS", It.IsAny<string[]>(), It.IsAny<string>())).ReturnsAsync(assignedAdviseesList);
                // mock data for retrieving all person information (sorted by name)
                dataReaderMock.Setup(acc => acc.SelectAsync("PERSON", It.IsAny<string[]>(), It.IsAny<string>())).ReturnsAsync(assignedAdviseesList);
                // mock data for retrieving all person/student information
                MockStudentData(knownStudentId1);

                var result = await adviseeRepository.SearchByNameAsync(lastName, null, null, int.MaxValue, 1, assignedAdviseesList);

                Assert.AreEqual(knownStudentId1, result.ElementAt(0).Id);
                Assert.AreEqual(1, result.Count());
            }

            [TestMethod]
            public async Task SearchByName_ReturnsAdviseesRequestedReviewFirst()
            {
                var personIdsList = new string[] { knownStudentId1, knownStudentId2 };
                var requestedReviewList = new string[] { knownStudentId2 };
                var blankStudentIdsList = new string[] { };
                var lastName = "Gerbil";
                // mock for SearchbyName
                dataReaderMock.Setup(acc => acc.SelectAsync("PERSON", It.IsAny<string>())).ReturnsAsync(personIdsList);
                // mock for FilterByEntity
                dataReaderMock.Setup(acc => acc.SelectAsync("STUDENTS", It.IsAny<string[]>(), It.IsAny<string>())).ReturnsAsync(personIdsList);
                // mock for GetCurrentPage, students who have requested review
                dataReaderMock.Setup(a => a.SelectAsync("DEGREE_PLAN", It.IsAny<string>(), It.IsAny<string[]>(), "?", true, It.IsAny<int>())).ReturnsAsync(requestedReviewList);
                dataReaderMock.Setup(a => a.SelectAsync("PERSON", requestedReviewList, It.IsAny<string>())).ReturnsAsync(requestedReviewList);
                // mock for GetCurrentPage, all students
                dataReaderMock.Setup(acc => acc.SelectAsync("PERSON", personIdsList, It.IsAny<string>())).ReturnsAsync(personIdsList);
                // mock data for retrieving all person/student information
                MockStudentData(knownStudentId1, knownStudentId2);

                var result = await adviseeRepository.SearchByNameAsync(lastName, null, null, int.MaxValue, 1);

                Assert.AreEqual(knownStudentId2, result.ElementAt(0).Id);
                Assert.AreEqual(knownStudentId1, result.ElementAt(1).Id);
                Assert.AreEqual(2, result.Count());
            }
        }

        [TestClass]
        public class AdviseeRepository_SearchByNameForExactMatchAsync
        {

            protected Mock<IColleagueTransactionFactory> transFactoryMock;
            protected Mock<ObjectCache> localCacheMock;
            protected Mock<ICacheProvider> cacheProviderMock;
            protected Mock<IColleagueDataReader> dataReaderMock;
            protected Mock<IColleagueDataReader> anonymousDataReaderMock;
            protected Mock<ILogger> loggerMock;
            protected Mock<IColleagueTransactionInvoker> transManagerMock;
            ApiSettings apiSettingsMock;
            protected Mock<StudentRepository> studentRepoMock;

            AdviseeRepository adviseeRepository;

            string knownStudentId1 = "111";
            string knownStudentId2 = "222";

            [TestInitialize]
            public void Initialize()
            {
                // Initialize person setup and Mock framework
                // transaction factory mock
                transFactoryMock = new Mock<IColleagueTransactionFactory>();
                // Cache Mock
                localCacheMock = new Mock<ObjectCache>();
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
             

                cacheProviderMock.Setup<Task<Tuple<object, SemaphoreSlim>>>(x =>
                    x.GetAndLockSemaphoreAsync(It.IsAny<string>(), null))
                    .ReturnsAsync(new Tuple<object, SemaphoreSlim>(null, new SemaphoreSlim(1, 1)));

                // Build the test repository
                adviseeRepository = BuildMockAdviseeRepository();
            }

            private AdviseeRepository BuildMockAdviseeRepository()
            {

                AdviseeRepository repository = new AdviseeRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object, apiSettingsMock);

                return repository;
            }

            private void MockStudentData(string studentId)
            {
                dataReaderMock.Setup(a => a.BulkReadRecordAsync<Base.DataContracts.Person>(It.IsAny<string[]>(), true)).Returns((string[] s, bool b) => Task.FromResult(BuildPeople(s)));
                dataReaderMock.Setup(a => a.BulkReadRecordAsync<Student.DataContracts.Students>(It.IsAny<string[]>(), true)).Returns((string[] s, bool b) => Task.FromResult(BuildStudents(s)));
                dataReaderMock.Setup(a => a.BulkReadRecordAsync<Base.DataContracts.PersonSt>(It.IsAny<string[]>(), true)).Returns((string[] s, bool b) => Task.FromResult(BuildPersonStRecords(s)));
                dataReaderMock.Setup(d => d.BulkReadRecordAsync<Student.DataContracts.StudentAdvisement>(It.IsAny<string[]>(), true)).ReturnsAsync((Collection<Student.DataContracts.StudentAdvisement>)null);
                dataReaderMock.Setup(d => d.BulkReadRecordAsync<Base.DataContracts.ForeignPerson>(It.IsAny<string[]>(), true)).ReturnsAsync((Collection<Base.DataContracts.ForeignPerson>)null);
                dataReaderMock.Setup(d => d.BulkReadRecordAsync<Student.DataContracts.Applicants>(It.IsAny<string[]>(), true)).ReturnsAsync((Collection<Student.DataContracts.Applicants>)null);

                // Mock the read of the Preferred Name Address Hierarchy for the Preferred Name
                dataReaderMock.Setup<Ellucian.Colleague.Data.Base.DataContracts.NameAddrHierarchy>(a =>
                    a.ReadRecord<Ellucian.Colleague.Data.Base.DataContracts.NameAddrHierarchy>("NAME.ADDR.HIERARCHY", "PREFERRED", true))
                    .Returns(new Ellucian.Colleague.Data.Base.DataContracts.NameAddrHierarchy()
                    {
                        Recordkey = "PREFERRED",
                        NahNameHierarchy = new List<string>() { "MA", "XYZ", "PF" }
                    });

                //// Mock the call for getting the preferred address
                transManagerMock.Setup<TxGetHierarchyAddressResponse>(
                    manager => manager.Execute<TxGetHierarchyAddressRequest, TxGetHierarchyAddressResponse>(
                        It.IsAny<TxGetHierarchyAddressRequest>())
                    ).Returns<TxGetHierarchyAddressRequest>(request =>
                    {
                        return new TxGetHierarchyAddressResponse() { OutAddressId = studentId, OutAddressLabel = new List<string>() { "AdressLabel" } };
                    });

            }

            private void MockStudentData(string studentId1, string studentId2)
            {
                // Mock individual reads for each student
                MockStudentData(studentId1);
                MockStudentData(studentId2);
                // Mock bulk reads that must return a collection
                dataReaderMock.Setup(a => a.BulkReadRecordAsync<Base.DataContracts.Person>(It.IsAny<string[]>(), true))
                    .ReturnsAsync(new Collection<Base.DataContracts.Person>()
                    {
                        new Base.DataContracts.Person() { Recordkey = studentId1, LastName = "last" + studentId1 },
                        new Base.DataContracts.Person() { Recordkey = studentId2, LastName = "last" + studentId2 }
                    });
                dataReaderMock.Setup(a => a.BulkReadRecordAsync<Student.DataContracts.Students>(It.IsAny<string[]>(), true))
                    .ReturnsAsync(new Collection<Student.DataContracts.Students>()
                    {
                        new Student.DataContracts.Students() { Recordkey = studentId1, StuAcadPrograms = null },
                        new Student.DataContracts.Students() { Recordkey = studentId2, StuAcadPrograms = null }
                    });
            }

            [TestCleanup]
            public void TestCleanup()
            {
                adviseeRepository = null;
            }

            [TestMethod]
            public async Task SearchByNameForExactMatchAsync_ReturnsEmptyListIfCalledWithEmptyNames()
            {
                var result = await adviseeRepository.SearchByNameForExactMatchAsync(string.Empty, string.Empty, string.Empty);
                Assert.AreEqual(0, result.Count());
            }

            [TestMethod]
            public async Task SearchByNameForExactMatchAsync_ReturnsEmptyListIfCalledWithNullNames()
            {
                var result = await adviseeRepository.SearchByNameForExactMatchAsync(null);
                Assert.AreEqual(0, result.Count());
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task SearchByNameForExactMatchAsync_ThrowArgumentNullExceptionIdLastNameIsNull()
            {
                var result = await adviseeRepository.SearchByNameForExactMatchAsync(null, "firstname", "middlename");
            }

            [TestMethod]
            public async Task SearchByNameForExactMatchAsync_ReturnsEmptyListIfPersonSearchByNameReturnsEmptyList()
            {
                var lookupStringResponse = new GetPersonSearchKeyListResponse() { ErrorMessage = "", KeyList = new List<string>()  };
                transManagerMock.Setup(manager => manager
                        .ExecuteAsync<GetPersonSearchKeyListRequest, GetPersonSearchKeyListResponse>(It.IsAny<GetPersonSearchKeyListRequest>()))
                        .ReturnsAsync(lookupStringResponse);
                var lastName = "Gerbil";
                var result = await adviseeRepository.SearchByNameForExactMatchAsync(lastName);
                Assert.AreEqual(0, result.Count());
            }

            [TestMethod]
            public async Task SearchByNameForExactMatchAsync_ReturnsEmptyListIfPersonSearchByNameReturnsKeyListNull()
            {
                var lookupStringResponse = new GetPersonSearchKeyListResponse() { ErrorMessage = "", KeyList = null };
                transManagerMock.Setup(manager => manager
                        .ExecuteAsync<GetPersonSearchKeyListRequest, GetPersonSearchKeyListResponse>(It.IsAny<GetPersonSearchKeyListRequest>()))
                        .ReturnsAsync(lookupStringResponse);
                var lastName = "Gerbil";
                var result = await adviseeRepository.SearchByNameForExactMatchAsync(lastName);
                Assert.AreEqual(0, result.Count());
            }
            [TestMethod]
            public async Task SearchByNameForExactMatchAsync_ReturnsEmptyListIfPersonSearchByNameReturnsNull()
            {
                transManagerMock.Setup(manager => manager
                        .ExecuteAsync<GetPersonSearchKeyListRequest, GetPersonSearchKeyListResponse>(It.IsAny<GetPersonSearchKeyListRequest>()))
                        .ReturnsAsync(null);
                var lastName = "Gerbil";
                var result = await adviseeRepository.SearchByNameForExactMatchAsync(lastName);
                Assert.AreEqual(0, result.Count());
            }

            [TestMethod]
            public async Task SearchByNameForExactMatchAsync_ReturnsEmptyListIfStudentSelectReturnsNull()
            {
                var personIdsList = new string[] { "001", "002", "003" };
                string[] nullStudentIdsList = null;
                var lookupStringResponse = new GetPersonSearchKeyListResponse() { ErrorMessage = "", KeyList = personIdsList.ToList() };
                transManagerMock.Setup(manager => manager
                        .ExecuteAsync<GetPersonSearchKeyListRequest, GetPersonSearchKeyListResponse>(It.IsAny<GetPersonSearchKeyListRequest>()))
                        .ReturnsAsync(lookupStringResponse);
                dataReaderMock.Setup(acc => acc.Select("STUDENTS", It.IsAny<string[]>(), It.IsAny<string>())).Returns(nullStudentIdsList);
                var lastName = "Gerbil";
                var result = await adviseeRepository.SearchByNameForExactMatchAsync(lastName);
                Assert.AreEqual(0, result.Count());
            }

            [TestMethod]
            public async Task SearchByNameForExactMatchAsync_ReturnsEmptyListIfStudentSelectReturnsEmptyList()
            {
                var personIdsList = new string[] { "001", "002", "003" };
                var blankStudentIdsList = new string[] { };
                var lastName = "Gerbil";
                var lookupStringResponse = new GetPersonSearchKeyListResponse() { ErrorMessage = "", KeyList = personIdsList.ToList() };
                transManagerMock.Setup(manager => manager
                        .ExecuteAsync<GetPersonSearchKeyListRequest, GetPersonSearchKeyListResponse>(It.IsAny<GetPersonSearchKeyListRequest>()))
                        .ReturnsAsync(lookupStringResponse);
                dataReaderMock.Setup(acc => acc.Select("STUDENTS", It.IsAny<string[]>(), It.IsAny<string>())).Returns(blankStudentIdsList);
                var result = await adviseeRepository.SearchByNameForExactMatchAsync(lastName);
                Assert.AreEqual(0, result.Count());
            }

            [TestMethod]
            public async Task SearchByNameForExactMatchAsync_LimitsReturnedItemsToAssignedAdvisees()
            {
                var personIdsList = new string[] { knownStudentId1, knownStudentId2 };
                var assignedAdviseesList = new string[] { knownStudentId1 };
                var blankStudentIdsList = new string[] { };
                var lastName = "Gerbil";
                var lookupStringResponse = new GetPersonSearchKeyListResponse() { ErrorMessage = "", KeyList = personIdsList.ToList() };
                transManagerMock.Setup(manager => manager
                        .ExecuteAsync<GetPersonSearchKeyListRequest, GetPersonSearchKeyListResponse>(It.IsAny<GetPersonSearchKeyListRequest>()))
                        .ReturnsAsync(lookupStringResponse);
                // mock for FilterByEntity
                dataReaderMock.Setup(acc => acc.SelectAsync("STUDENTS", It.IsAny<string[]>(), It.IsAny<string>())).ReturnsAsync(personIdsList);
                // mock for GetCurrentPage, students who have requested review
                dataReaderMock.Setup(a => a.SelectAsync("DEGREE_PLAN", It.IsAny<string>(), It.IsAny<string[]>(), "?", true, It.IsAny<int>())).ReturnsAsync(blankStudentIdsList);
                // mock for GetCurrentPage, all provided students (limited by advisees in this case)
                dataReaderMock.Setup(acc => acc.SelectAsync("STUDENTS", It.IsAny<string[]>(), It.IsAny<string>())).ReturnsAsync(assignedAdviseesList);
                // mock data for retrieving all person information (sorted by name)
                dataReaderMock.Setup(acc => acc.SelectAsync("PERSON", It.IsAny<string[]>(), It.IsAny<string>())).ReturnsAsync(assignedAdviseesList);
                // mock data for retrieving all person/student information
                MockStudentData(knownStudentId1);

                var result = await adviseeRepository.SearchByNameForExactMatchAsync(lastName, null, null, int.MaxValue, 1, assignedAdviseesList);

                Assert.AreEqual(knownStudentId1, result.ElementAt(0).Id);
                Assert.AreEqual(1, result.Count());
            }

            [TestMethod]
            public async Task SearchByNameForExactMatchAsync_ReturnsAdviseesRequestedReviewFirst()
            {
                var personIdsList = new string[] { knownStudentId1, knownStudentId2 };
                var requestedReviewList = new string[] { knownStudentId2 };
                var blankStudentIdsList = new string[] { };
                var lastName = "Gerbil";
                var lookupStringResponse = new GetPersonSearchKeyListResponse() { ErrorMessage = "", KeyList = personIdsList.ToList() };
                transManagerMock.Setup(manager => manager
                        .ExecuteAsync<GetPersonSearchKeyListRequest, GetPersonSearchKeyListResponse>(It.IsAny<GetPersonSearchKeyListRequest>()))
                        .ReturnsAsync(lookupStringResponse);
                
                // mock for FilterByEntity
                dataReaderMock.Setup(acc => acc.SelectAsync("STUDENTS", It.IsAny<string[]>(), It.IsAny<string>())).ReturnsAsync(personIdsList);
                // mock for GetCurrentPage, students who have requested review
                dataReaderMock.Setup(a => a.SelectAsync("DEGREE_PLAN", It.IsAny<string>(), It.IsAny<string[]>(), "?", true, It.IsAny<int>())).ReturnsAsync(requestedReviewList);
                dataReaderMock.Setup(a => a.SelectAsync("PERSON", requestedReviewList, It.IsAny<string>())).ReturnsAsync(requestedReviewList);
                // mock for GetCurrentPage, all students
                dataReaderMock.Setup(acc => acc.SelectAsync("PERSON", personIdsList, It.IsAny<string>())).ReturnsAsync(personIdsList);

                // mock data for retrieving all person/student information
                MockStudentData(knownStudentId1, knownStudentId2);

                var result = await adviseeRepository.SearchByNameForExactMatchAsync(lastName, null, null, int.MaxValue, 1);

                Assert.AreEqual(knownStudentId2, result.ElementAt(0).Id);
                Assert.AreEqual(knownStudentId1, result.ElementAt(1).Id);
                Assert.AreEqual(2, result.Count());
            }
        }

        [TestClass]
        public class AdviseeRepository_SearchByAdvisorIds
        {

            protected Mock<IColleagueTransactionFactory> transFactoryMock;
            protected Mock<ObjectCache> localCacheMock;
            protected Mock<ICacheProvider> cacheProviderMock;
            protected Mock<IColleagueDataReader> dataReaderMock;
            protected Mock<IColleagueDataReader> anonymousDataReaderMock;
            protected Mock<ILogger> loggerMock;
            protected Mock<IColleagueTransactionInvoker> transManagerMock;

            protected Mock<StudentRepository> studentRepoMock;
            ApiSettings apiSettingsMock;
            AdviseeRepository adviseeRepository;

            //string unknownStudentId = "999";
            string knownStudentId1 = "111";
            string knownStudentId2 = "222";

            [TestInitialize]
            public void Initialize()
            {
                // Initialize person setup and Mock framework
                // transaction factory mock
                transFactoryMock = new Mock<IColleagueTransactionFactory>();
                // Cache Mock
                localCacheMock = new Mock<ObjectCache>();
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
                    x.GetAndLockSemaphoreAsync(It.IsAny<string>(), null)).Returns(Task.FromResult(new Tuple<object, SemaphoreSlim>(null, new SemaphoreSlim(1, 1))));
                
                // Build the test repository
                adviseeRepository = BuildMockAdviseeRepository();
            }

            private AdviseeRepository BuildMockAdviseeRepository()
            {

                AdviseeRepository repository = new AdviseeRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object, apiSettingsMock);

                return repository;
            }

            private void MockStudentData(string studentId)
            {

                dataReaderMock.Setup(a => a.BulkReadRecordAsync<Base.DataContracts.Person>(It.IsAny<string[]>(), true)).Returns((string[] s, bool b) => Task.FromResult(BuildPeople(s)));
                dataReaderMock.Setup(a => a.BulkReadRecordAsync<Student.DataContracts.Students>(It.IsAny<string[]>(), true)).Returns((string[] s, bool b) => Task.FromResult(BuildStudents(s)));
                dataReaderMock.Setup(a => a.BulkReadRecordAsync<Base.DataContracts.PersonSt>(It.IsAny<string[]>(), true)).Returns((string[] s, bool b) => Task.FromResult(BuildPersonStRecords(s)));
                dataReaderMock.Setup(d => d.BulkReadRecordAsync<Student.DataContracts.StudentAdvisement>("STUDENT.ADVISEMENT", It.IsAny<string[]>(), true)).ReturnsAsync((Collection<Student.DataContracts.StudentAdvisement>)null);

                // Mock the read of the Preferred Name Address Hierarchy for the Preferred Name
                dataReaderMock.Setup<Ellucian.Colleague.Data.Base.DataContracts.NameAddrHierarchy>(a =>
                    a.ReadRecord<Ellucian.Colleague.Data.Base.DataContracts.NameAddrHierarchy>("NAME.ADDR.HIERARCHY", "PREFERRED", true))
                    .Returns(new Ellucian.Colleague.Data.Base.DataContracts.NameAddrHierarchy()
                    {
                        Recordkey = "PREFERRED",
                        NahNameHierarchy = new List<string>() { "MA", "XYZ", "PF" }
                    });

                //// Mock the call for getting the preferred address
                transManagerMock.Setup<TxGetHierarchyAddressResponse>(
                    manager => manager.Execute<TxGetHierarchyAddressRequest, TxGetHierarchyAddressResponse>(
                        It.IsAny<TxGetHierarchyAddressRequest>())
                    ).Returns<TxGetHierarchyAddressRequest>(request =>
                    {
                        return new TxGetHierarchyAddressResponse() { OutAddressId = studentId, OutAddressLabel = new List<string>() { "AdressLabel" } };
                    });

            }

            private void MockStudentData(string studentId1, string studentId2)
            {
                // Mock individual reads for each student
                MockStudentData(studentId1);
                MockStudentData(studentId2);
            }

            [TestCleanup]
            public void TestCleanup()
            {
                adviseeRepository = null;
            }

            [TestMethod]
            public async Task SearchByAdvisorIds_ReturnsEmptyListIfCalledWithNullAdvisorIds()
            {
                var result = await adviseeRepository.SearchByAdvisorIdsAsync(null);
                Assert.AreEqual(0, result.Count());
            }

            [TestMethod]
            public async Task SearchByAdvisorIds_ReturnsEmptyListIfCalledWithZeroAdvisorIds()
            {
                var result = await adviseeRepository.SearchByAdvisorIdsAsync(new List<string>());
                Assert.AreEqual(0, result.Count());
            }

            [TestMethod]
            public async Task SearchByAdvisorIds_ReturnsEmptyListIfStudentAdvisementSelectReturnsEmptyList()
            {
                var emptyIdsList = new string[] { };
                dataReaderMock.Setup(acc => acc.Select("STUDENT.ADVISEMENT", It.IsAny<string>())).Returns(emptyIdsList);
                IEnumerable<string> advisorIds = new List<string>() { "111" };
                var result = await adviseeRepository.SearchByAdvisorIdsAsync(advisorIds);
                Assert.AreEqual(0, result.Count());
            }

            [TestMethod]
            public async Task SearchByAdvisorIds_ReturnsEmptyListIfStudentAdvisementSelectReturnsNull()
            {
                string[] nullIdsList = null;
                dataReaderMock.Setup(acc => acc.Select("PERSON", It.IsAny<string[]>(), It.IsAny<string>())).Returns(nullIdsList);
                IEnumerable<string> advisorIds = new List<string>() { "111", "222" };
                var result = await adviseeRepository.SearchByAdvisorIdsAsync(advisorIds);
                Assert.AreEqual(0, result.Count());
            }

            [TestMethod]
            public async Task SearchByAdvisorIds_LimitsReturnedItemsToAssignedAdvisees()
            {
                var personIdsList = new string[] { knownStudentId1, knownStudentId2 };
                var assignedAdviseesList = new string[] { knownStudentId1 };
                var blankStudentIdsList = new string[] { };
                IEnumerable<string> advisorIds = new List<string>() { "111", "222" };
                // mock for SearchbyAdvisorIds
                dataReaderMock.Setup(acc => acc.SelectAsync("STUDENT.ADVISEMENT", It.IsAny<string>(), It.IsAny<string[]>(), "?", true, It.IsAny<int>())).ReturnsAsync(personIdsList);
                // mock for GetCurrentPage, students who have requested review
                dataReaderMock.Setup(a => a.SelectAsync("DEGREE_PLAN", It.IsAny<string>(), It.IsAny<string[]>(), "?", true, It.IsAny<int>())).ReturnsAsync(blankStudentIdsList);
                // mock for GetCurrentPage, all provided students (limited by advisees in this case)
                dataReaderMock.Setup(acc => acc.SelectAsync("STUDENTS", It.IsAny<string[]>(), It.IsAny<string>())).ReturnsAsync(assignedAdviseesList);
                // mock for GetCurrentPage, all provided PERSONs - sorted by name (limited by advisees in this case)
                dataReaderMock.Setup(acc => acc.SelectAsync("PERSON", It.IsAny<string[]>(), It.IsAny<string>())).ReturnsAsync(assignedAdviseesList);
                // mock data for retrieving all person/student information
                MockStudentData(knownStudentId1);

                var result = await adviseeRepository.SearchByAdvisorIdsAsync(advisorIds, int.MaxValue, 1, assignedAdviseesList);

                Assert.AreEqual(knownStudentId1, result.ElementAt(0).Id);
                Assert.AreEqual(1, result.Count());
            }

            [TestMethod]
            public async Task SearchByAdvisorIds_ReturnsAdviseesRequestedReviewFirst()
            {
                var personIdsList = new string[] { knownStudentId1, knownStudentId2 };
                var requestedReviewList = new string[] { knownStudentId2 };
                var blankStudentIdsList = new string[] { };
                IEnumerable<string> advisorIds = new List<string>() { "111", "222" };
                // mock for SearchbyAdvisorIds
                dataReaderMock.Setup(acc => acc.SelectAsync("STUDENT.ADVISEMENT", It.IsAny<string>(), It.IsAny<string[]>(), "?", true, It.IsAny<int>())).ReturnsAsync(personIdsList);
                // mock for GetCurrentPage, students who have requested review
                dataReaderMock.Setup(a => a.SelectAsync("DEGREE_PLAN", It.IsAny<string>(), It.IsAny<string[]>(), "?", true, It.IsAny<int>())).ReturnsAsync(requestedReviewList);
                dataReaderMock.Setup(a => a.SelectAsync("PERSON", requestedReviewList, It.IsAny<string>())).ReturnsAsync(requestedReviewList);
                // mock for GetCurrentPage, all students
                dataReaderMock.Setup(acc => acc.SelectAsync("STUDENTS", personIdsList, It.IsAny<string>())).ReturnsAsync(personIdsList);
                // mock for GetCurrentPage, all PERSONS sorted by name
                dataReaderMock.Setup(acc => acc.SelectAsync("PERSON", personIdsList, It.IsAny<string>())).ReturnsAsync(personIdsList);
                // mock data for retrieving all person/student information
                MockStudentData(knownStudentId1, knownStudentId2);

                var result = await adviseeRepository.SearchByAdvisorIdsAsync(advisorIds, int.MaxValue, 1);

                Assert.AreEqual(knownStudentId2, result.ElementAt(0).Id);
                Assert.AreEqual(knownStudentId1, result.ElementAt(1).Id);
                Assert.AreEqual(2, result.Count());
            }
        }

        [TestClass]
        public class AdviseeRepository_PostCompletedAdvisementAsync
        {
            protected Mock<IColleagueTransactionFactory> transFactoryMock;
            protected Mock<ObjectCache> localCacheMock;
            protected Mock<ICacheProvider> cacheProviderMock;
            protected Mock<IColleagueDataReader> dataReaderMock;
            protected Mock<IColleagueDataReader> anonymousDataReaderMock;
            protected Mock<ILogger> loggerMock;
            protected Mock<IColleagueTransactionInvoker> transManagerMock;

            protected Mock<StudentRepository> studentRepoMock;
            ApiSettings apiSettingsMock;
            AdviseeRepository adviseeRepository;

            string unknownStudentId = "999";
            string knownStudentId1 = "111";
            string knownStudentId2 = "222";
            string[] onlyStudents = new string[] { "222" };

            private CreateStudentAdvisementResponse response;

            [TestInitialize]
            public void Initialize()
            {
                // Initialize person setup and Mock framework
                // transaction factory mock
                transFactoryMock = new Mock<IColleagueTransactionFactory>();
                // Cache Mock
                localCacheMock = new Mock<ObjectCache>();
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

                // set up the mock data accessor 
                dataReaderMock.Setup(a => a.ReadRecordAsync<Student.DataContracts.Students>(knownStudentId1, true)).ReturnsAsync(new Student.DataContracts.Students() { Recordkey = knownStudentId1, StuAcadPrograms = null });
                dataReaderMock.Setup(a => a.ReadRecordAsync<Student.DataContracts.Students>(knownStudentId2, true)).ReturnsAsync(new Student.DataContracts.Students() { Recordkey = knownStudentId2, StuAcadPrograms = null });
                dataReaderMock.Setup(a => a.ReadRecordAsync<Student.DataContracts.Students>(unknownStudentId, true)).ReturnsAsync((Student.DataContracts.Students)null);

                dataReaderMock.Setup(a => a.ReadRecordAsync<Base.DataContracts.PersonSt>(knownStudentId1, true)).ReturnsAsync(new Base.DataContracts.PersonSt() { Recordkey = knownStudentId1, PstAdvisement = new List<string>() });
                dataReaderMock.Setup(a => a.ReadRecordAsync<Base.DataContracts.PersonSt>(knownStudentId2, true)).ReturnsAsync(new Base.DataContracts.PersonSt() { Recordkey = knownStudentId1, PstAdvisement = new List<string>() });
                dataReaderMock.Setup(a => a.ReadRecordAsync<Base.DataContracts.PersonSt>(unknownStudentId, true)).ReturnsAsync((Base.DataContracts.PersonSt)null);

                dataReaderMock.Setup(a => a.ReadRecordAsync<Base.DataContracts.Person>(knownStudentId1, true)).ReturnsAsync(new Base.DataContracts.Person() { Recordkey = knownStudentId1, LastName = "Last1" });
                dataReaderMock.Setup(a => a.ReadRecordAsync<Base.DataContracts.Person>(knownStudentId2, true)).ReturnsAsync(new Base.DataContracts.Person() { Recordkey = knownStudentId2, LastName = "Last2" });
                dataReaderMock.Setup(a => a.ReadRecordAsync<Base.DataContracts.Person>(unknownStudentId, true)).ReturnsAsync((Base.DataContracts.Person)null);

                dataReaderMock.Setup(a => a.BulkReadRecordAsync<Base.DataContracts.Person>(new string[] { "111", "222" }, true)).ReturnsAsync(new Collection<Base.DataContracts.Person>() { new Base.DataContracts.Person() { Recordkey = "111", LastName = "brown" }, new Base.DataContracts.Person() { Recordkey = "222", LastName = "smith" } });
                dataReaderMock.Setup(a => a.BulkReadRecordAsync<Base.DataContracts.Person>(new string[] { "111" }, true)).ReturnsAsync(new Collection<Base.DataContracts.Person>() { new Base.DataContracts.Person() { Recordkey = "111", LastName = "brown" } });
                dataReaderMock.Setup(a => a.BulkReadRecordAsync<Base.DataContracts.Person>(new string[] { "222" }, true)).ReturnsAsync(new Collection<Base.DataContracts.Person>() { new Base.DataContracts.Person() { Recordkey = "222", LastName = "smith" } });
                dataReaderMock.Setup(a => a.BulkReadRecordAsync<Base.DataContracts.Person>(new string[] { "999" }, true)).ReturnsAsync(new Collection<Base.DataContracts.Person>());

                dataReaderMock.Setup(a => a.BulkReadRecordAsync<Student.DataContracts.Students>(new string[] { "999" }, true)).ReturnsAsync(new Collection<Student.DataContracts.Students>());
                dataReaderMock.Setup(a => a.BulkReadRecordAsync<Student.DataContracts.Students>(new string[] { "111", "222" }, true)).ReturnsAsync(new Collection<Student.DataContracts.Students>() { new Student.DataContracts.Students() { Recordkey = "111", StuAcadPrograms = null }, new Student.DataContracts.Students() { Recordkey = "222", StuAcadPrograms = null } });
                dataReaderMock.Setup(a => a.BulkReadRecordAsync<Student.DataContracts.Students>(new string[] { "111" }, true)).ReturnsAsync(new Collection<Student.DataContracts.Students>() { new Student.DataContracts.Students() { Recordkey = "111", StuAcadPrograms = null } });
                dataReaderMock.Setup(a => a.BulkReadRecordAsync<Student.DataContracts.Students>(new string[] { "222" }, true)).ReturnsAsync(new Collection<Student.DataContracts.Students>() { new Student.DataContracts.Students() { Recordkey = "222", StuAcadPrograms = new List<string>() { "MATH.BA" } } });
                dataReaderMock.Setup(a => a.BulkReadRecordAsync<Base.DataContracts.PersonSt>(It.IsAny<string[]>(), true)).ReturnsAsync(new Collection<Base.DataContracts.PersonSt>());

                dataReaderMock.Setup(d => d.BulkReadRecordAsync<Student.DataContracts.StudentAdvisement>(It.IsAny<string[]>(), true)).ReturnsAsync((Collection<Student.DataContracts.StudentAdvisement>)null);
                dataReaderMock.Setup(d => d.BulkReadRecordAsync<Base.DataContracts.ForeignPerson>(It.IsAny<string[]>(), true)).ReturnsAsync((Collection<Base.DataContracts.ForeignPerson>)null);
                dataReaderMock.Setup(d => d.BulkReadRecordAsync<Student.DataContracts.Applicants>(It.IsAny<string[]>(), true)).ReturnsAsync((Collection<Student.DataContracts.Applicants>)null);

                dataReaderMock.Setup<string[]>(a => a.Select("DEGREE_PLAN", "DP.STUDENT.ID EQ '" + knownStudentId1 + "'")).Returns(new string[] { "1" });
                dataReaderMock.Setup<string[]>(a => a.Select("DEGREE_PLAN", "DP.STUDENT.ID EQ '" + knownStudentId1 + "' '" + knownStudentId2 + "'")).Returns(new string[] { "1", "2" });

                // Mock the read of the Preferred Name Address Hierarchy for the Preferred Name
                dataReaderMock.Setup<Ellucian.Colleague.Data.Base.DataContracts.NameAddrHierarchy>(a =>
                    a.ReadRecord<Ellucian.Colleague.Data.Base.DataContracts.NameAddrHierarchy>("NAME.ADDR.HIERARCHY", "PREFERRED", true))
                    .Returns(new Ellucian.Colleague.Data.Base.DataContracts.NameAddrHierarchy()
                    {
                        Recordkey = "PREFERRED",
                        NahNameHierarchy = new List<string>() { "MA", "XYZ", "PF" }
                    });

                // Mock the call for getting the preferred address
                transManagerMock.Setup<TxGetHierarchyAddressResponse>(
                    manager => manager.Execute<TxGetHierarchyAddressRequest, TxGetHierarchyAddressResponse>(
                        It.IsAny<TxGetHierarchyAddressRequest>())
                    ).Returns<TxGetHierarchyAddressRequest>(request =>
                    {
                        return new TxGetHierarchyAddressResponse() { OutAddressId = "1111", OutAddressLabel = new List<string>() { "AdressLabel" } };
                    });

                // Mock the call to get a lookup string
                var lookupStringResponse = new GetPersonLookupStringResponse() { IndexString = ";PartialNameIndex BROWN_SL", ErrorMessage = "" };
                transManagerMock.Setup(manager => manager
                        .ExecuteAsync<GetPersonLookupStringRequest, GetPersonLookupStringResponse>(It.IsAny<GetPersonLookupStringRequest>()))
                        .ReturnsAsync(lookupStringResponse);

                response = new CreateStudentAdvisementResponse()
                {
                    AStudentsId = knownStudentId1
                };
                transManagerMock.Setup(manager => manager
                        .ExecuteAsync<CreateStudentAdvisementRequest, CreateStudentAdvisementResponse>(It.IsAny<CreateStudentAdvisementRequest>()))
                        .ReturnsAsync(response);

                cacheProviderMock.Setup<Task<Tuple<object, SemaphoreSlim>>>(x =>
                    x.GetAndLockSemaphoreAsync(It.IsAny<string>(), null))
                    .ReturnsAsync(new Tuple<object, SemaphoreSlim>(null, new SemaphoreSlim(1, 1)));

                // Build the test repository
                adviseeRepository = BuildMockAdviseeRepository();
            }

            private AdviseeRepository BuildMockAdviseeRepository()
            {
                AdviseeRepository repository = new AdviseeRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object, apiSettingsMock);
                return repository;
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task AdviseeRepository_PostCompletedAdvisementAsync_Null_StudentId()
            {
                var planningStudent = await adviseeRepository.PostCompletedAdvisementAsync(null, DateTime.Today, DateTime.Now, knownStudentId2);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task AdviseeRepository_PostCompletedAdvisementAsync_Null_AdvisorId()
            {
                var planningStudent = await adviseeRepository.PostCompletedAdvisementAsync(knownStudentId1, DateTime.Today, DateTime.Now, null);
            }

            [TestMethod]
            [ExpectedException(typeof(ApplicationException))]
            public async Task AdviseeRepository_PostCompletedAdvisementAsync_Null_CTX_Response()
            {
                transManagerMock.Setup(manager => manager
                    .ExecuteAsync<CreateStudentAdvisementRequest, CreateStudentAdvisementResponse>(It.IsAny<CreateStudentAdvisementRequest>()))
                    .ReturnsAsync(null);
                adviseeRepository = BuildMockAdviseeRepository();

                var planningStudent = await adviseeRepository.PostCompletedAdvisementAsync(knownStudentId1, DateTime.Today, DateTime.Now, knownStudentId2);
            }

            [TestMethod]
            [ExpectedException(typeof(ApplicationException))]
            public async Task AdviseeRepository_PostCompletedAdvisementAsync_Error_Occurred_no_Error_Messages()
            {
                response.AErrorOccurred = true;
                transManagerMock.Setup(manager => manager
                    .ExecuteAsync<CreateStudentAdvisementRequest, CreateStudentAdvisementResponse>(It.IsAny<CreateStudentAdvisementRequest>()))
                    .ReturnsAsync(response);
                adviseeRepository = BuildMockAdviseeRepository();

                var planningStudent = await adviseeRepository.PostCompletedAdvisementAsync(knownStudentId1, DateTime.Today, DateTime.Now, knownStudentId2);
            }

            [TestMethod]
            [ExpectedException(typeof(ApplicationException))]
            public async Task AdviseeRepository_PostCompletedAdvisementAsync_Error_Occurred_with_Error_Messages()
            {
                response.AErrorOccurred = true;
                response.AlErrorMessages = new List<string>() { "An error occurred." };
                transManagerMock.Setup(manager => manager
                    .ExecuteAsync<CreateStudentAdvisementRequest, CreateStudentAdvisementResponse>(It.IsAny<CreateStudentAdvisementRequest>()))
                    .ReturnsAsync(response);
                adviseeRepository = BuildMockAdviseeRepository();

                var planningStudent = await adviseeRepository.PostCompletedAdvisementAsync(knownStudentId1, DateTime.Today, DateTime.Now, knownStudentId2);
            }

            [TestMethod]
            public async Task AdviseeRepository_PostCompletedAdvisementAsync_Success()
            {
                var planningStudent = await adviseeRepository.PostCompletedAdvisementAsync(knownStudentId1, DateTime.Today, DateTime.Now, knownStudentId2);
                Assert.IsNotNull(planningStudent);
            }

        }
    }
}
