// Copyright 2012-2018 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.Caching;
using Ellucian.Colleague.Data.Base.DataContracts;
using Ellucian.Colleague.Data.Base.Transactions;
using Ellucian.Colleague.Data.Planning.Repositories;
using Ellucian.Colleague.Data.Student.DataContracts;
using Ellucian.Colleague.Domain.Planning.Entities;
using Ellucian.Data.Colleague;
using Ellucian.Data.Colleague.DataContracts;
using Ellucian.Web.Cache;
using Ellucian.Web.Http.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;
using System.Threading.Tasks;
using System.Threading;

namespace Ellucian.Colleague.Data.Planning.Tests.Repositories
{
    [TestClass]
    public class AdvisorRepositoryTests 
    {
        private static Collection<T> ToCollection<T>(IEnumerable<T> data)
        {
            return new Collection<T>(data.ToList());
        }

        public class AdvisorRepositoryTestsSetup
        {
            protected Mock<IColleagueTransactionFactory> transFactoryMock;
            protected Mock<ObjectCache> localCacheMock;
            protected Mock<ICacheProvider> cacheProviderMock;
            protected Mock<IColleagueDataReader> dataReaderMock;
            protected Mock<ILogger> loggerMock;
            protected Mock<IColleagueTransactionInvoker> transManagerMock;

            protected Mock<AdvisorRepository> advisorRepoMock;

            protected AdvisorRepository advisorRepository;

            protected string staffAdvisor = "0000001";
            protected string notCurrentFacultyAdvisorWithAdvisees = "0000002";
            protected string advisorWithAdvisees = "0000003";
            protected string notCurrentStaffAdvisor = "0000004";
            protected string nonStaff = "0000005";
            protected string invalidAdvisor = "9999999";
            protected string facultyAdvisorWithNoAdvisees = "0000006";
            protected string facultyWithCurrentFormerAdvisees = "0000007";
            protected string facultyWithFormerFutureAdvisees = "0000008";

            protected Collection<Student.DataContracts.StudentAdvisement> studentAdvisementResponse;
            protected Collection<Base.DataContracts.Person> personResponse;
            protected Collection<Student.DataContracts.Faculty> facultyResponse;
            protected Collection<Base.DataContracts.Staff> staffResponse;

            protected AdvisorRepository BuildMockAdvisorRepository()
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

                transFactoryMock.Setup(transFac => transFac.GetDataReader()).Returns(dataReaderMock.Object);
                // Set up transManagerMock as the object for the transaction manager
                transFactoryMock.Setup(transFac => transFac.GetTransactionInvoker()).Returns(transManagerMock.Object);

                cacheProviderMock.Setup<Task<Tuple<object, SemaphoreSlim>>>(x =>
                    x.GetAndLockSemaphoreAsync(It.IsAny<string>(), null))
                    .ReturnsAsync(new Tuple<object, SemaphoreSlim>(null, new SemaphoreSlim(1, 1)));

                // Getup for Get, GetAdvisors
                // bulk read PERSON response. 
                personResponse = new Collection<Base.DataContracts.Person>()
                {
                    new Base.DataContracts.Person()
                    {
                        // "0000001"
                        Recordkey = notCurrentFacultyAdvisorWithAdvisees, LastName = "Person1", FirstName = "John", 
                        PeopleEmailEntityAssociation = new List<PersonPeopleEmail>()
                        {
                             new PersonPeopleEmail() {PersonEmailAddressesAssocMember = "sperson1@domain.com", PersonEmailTypesAssocMember = "WWW", PersonPreferredEmailAssocMember = ""}
                        }
                    },
                    new Base.DataContracts.Person()
                    {
                        // "0000002"
                        Recordkey = staffAdvisor, LastName = "Person2", FirstName = "J", MiddleName = "Andrew", 
                        PeopleEmailEntityAssociation = new List<PersonPeopleEmail>()
                        {
                             new PersonPeopleEmail() {PersonEmailAddressesAssocMember = "sperson2@domain.com", PersonEmailTypesAssocMember = "WWW", PersonPreferredEmailAssocMember = ""}
                        }
                    },
                    new Base.DataContracts.Person()
                    {
                        // "0000003"
                        Recordkey = advisorWithAdvisees, LastName = "Person3", FirstName = "John", MiddleName = "A",
                        PeopleEmailEntityAssociation = new List<PersonPeopleEmail>()
                        {
                             new PersonPeopleEmail() {PersonEmailAddressesAssocMember = "sperson3@domain.com", PersonEmailTypesAssocMember = "WWW", PersonPreferredEmailAssocMember = ""},
                             new PersonPeopleEmail() {PersonEmailAddressesAssocMember = "personalPerson3@domain.com", PersonEmailTypesAssocMember = "LOC", PersonPreferredEmailAssocMember = "y"}
                        }
                    },
                    new Base.DataContracts.Person()
                    {
                        // "0000004"
                        Recordkey = notCurrentStaffAdvisor, LastName = "Person4",
                    },
                    new Base.DataContracts.Person()
                    {
                        // "0000005"
                        Recordkey = nonStaff, LastName = "Person5",
                    },
                    new Base.DataContracts.Person()
                    {
                        // "0000006"
                        Recordkey = facultyAdvisorWithNoAdvisees, LastName = "Person6"
                    },
                    new Base.DataContracts.Person()
                    {
                        // "0000007"
                        Recordkey = facultyWithCurrentFormerAdvisees, LastName = "Person7"
                    },
                    new Base.DataContracts.Person()
                    {
                        // "0000008"
                        Recordkey = facultyWithFormerFutureAdvisees , LastName = "Person8"
                    }
                };
                dataReaderMock.Setup(acc => acc.BulkReadRecordAsync<Base.DataContracts.Person>(It.IsAny<string[]>(), true)).ReturnsAsync(personResponse);

                // mock bulk read FACULTY response
                facultyResponse = new Collection<Student.DataContracts.Faculty>()
                {
                    new Student.DataContracts.Faculty()
                    {
                        // "0000003"
                        Recordkey = advisorWithAdvisees, FacAdviseFlag = "Y", FacAdvisees = new List<string>() { "4", "5", "6", "10" }
                    },
                    new Student.DataContracts.Faculty()
                    {
                        // "0000004"
                        Recordkey = notCurrentStaffAdvisor, 
                    },
                    new Student.DataContracts.Faculty()
                    {
                        // "0000001"
                        Recordkey = notCurrentFacultyAdvisorWithAdvisees, FacAdviseFlag = "", FacAdvisees = new List<string>() { "7", "8", "9" }
                    },
                    new Student.DataContracts.Faculty()
                    {
                        // "0000006"
                        Recordkey = facultyAdvisorWithNoAdvisees, FacAdviseFlag = "Y"
                    },
                    new Student.DataContracts.Faculty()
                    {
                        // "0000007"
                        Recordkey = facultyWithCurrentFormerAdvisees, FacAdviseFlag = "Y", FacAdvisees = new List<string>() { "5", "7", "8", "9" }
                    },
                    new Student.DataContracts.Faculty()
                    {
                        // "0000008"
                        Recordkey = facultyWithFormerFutureAdvisees, FacAdviseFlag = "Y", FacAdvisees = new List<string>() { "5", "7", "8", "9" }
                    }
                };
                dataReaderMock.Setup(acc => acc.BulkReadRecordAsync<Student.DataContracts.Faculty>("FACULTY", It.IsAny<string[]>(), true)).ReturnsAsync(facultyResponse);

                // Current, Former and Future Advisements
                studentAdvisementResponse = new Collection<Student.DataContracts.StudentAdvisement>() 
                {
                    new StudentAdvisement() { Recordkey = "1", StadStudent = "0000001", StadEndDate = new DateTime(2010, 1, 1) },
                    new StudentAdvisement() { Recordkey = "2", StadStudent = "0000002", StadStartDate = new DateTime(2009, 1, 1), StadEndDate = new DateTime(2009, 1, 2) },
                    new StudentAdvisement() { Recordkey = "3", StadStudent = "0000003", StadStartDate = new DateTime(2030, 1, 1) },
                    new StudentAdvisement() { Recordkey = "4", StadStudent = "0000004" },
                    new StudentAdvisement() { Recordkey = "5", StadStudent = "0000005", StadStartDate = new DateTime(2010, 1, 1) },
                    new StudentAdvisement() { Recordkey = "6", StadStudent = "0000006", StadStartDate = new DateTime(2010, 1, 1), StadEndDate = new DateTime(2030, 1, 1) },
                    // and throwing in a dup too
                    new StudentAdvisement() { Recordkey = "10", StadStudent = "0000006", StadStartDate = new DateTime(2010, 1, 1) },
                    // Former Advisements
                    new StudentAdvisement() { Recordkey = "7", StadStudent = "0000111", StadEndDate = new DateTime(2010, 1, 1) },
                    new StudentAdvisement() { Recordkey = "8", StadStudent = "0000222", StadStartDate = new DateTime(2009, 1, 1), StadEndDate = new DateTime(2009, 1, 2) },
                    // Future Advisement
                    new StudentAdvisement() { Recordkey = "9", StadStudent = "0000333", StadStartDate = new DateTime(2030, 1, 1) }
                };
                dataReaderMock.Setup(acc => acc.BulkReadRecordAsync<Student.DataContracts.StudentAdvisement>("STUDENT.ADVISEMENT", It.IsAny<string[]>(), true)).Returns((string s1, string[] s2, bool b) => Task.FromResult(ToCollection(studentAdvisementResponse.Where(x => s2.Contains(x.Recordkey)))));

                // Mock bulk read STAFF response
                staffResponse = new Collection<Base.DataContracts.Staff>()
                {
                    new Base.DataContracts.Staff()
                    {
                        // "0000002"
                        Recordkey = staffAdvisor, StaffType = "S", StaffStatus = "C"
                    },
                    new Base.DataContracts.Staff()
                    {
                        // "0000005"
                        Recordkey = nonStaff, StaffType = "X"
                    },
                    new Base.DataContracts.Staff()
                    {
                        // "0000004"
                        Recordkey = notCurrentStaffAdvisor, StaffType = "S"
                    }
                };
                dataReaderMock.Setup(acc => acc.BulkReadRecordAsync<Base.DataContracts.Staff>("STAFF", It.IsAny<string[]>(), true)).ReturnsAsync(staffResponse);

                // mock STAFF.TYPES valcode response
                ApplValcodes staffTypesResponse = new ApplValcodes()
                {
                    ValsEntityAssociation = new List<ApplValcodesVals>() {
                    new ApplValcodesVals() { ValInternalCodeAssocMember = "S", ValActionCode1AssocMember = "S" },
                    new ApplValcodesVals() { ValInternalCodeAssocMember = "V", ValActionCode1AssocMember = ""},
                    new ApplValcodesVals() { ValInternalCodeAssocMember = "X", ValActionCode1AssocMember = "N"},
                    }
                };
                dataReaderMock.Setup(accessor => accessor.ReadRecordAsync<ApplValcodes>("CORE.VALCODES", "STAFF.TYPES", true)).ReturnsAsync(staffTypesResponse);

                // mock STAFF.STATUSES valcode response
                ApplValcodes staffStatusesResponse = new ApplValcodes()
                {
                    ValsEntityAssociation = new List<ApplValcodesVals>() {
                    new ApplValcodesVals() { ValInternalCodeAssocMember = "C", ValActionCode1AssocMember = "A" },
                    new ApplValcodesVals() { ValInternalCodeAssocMember = "F", ValActionCode1AssocMember = ""},
                    new ApplValcodesVals() { ValInternalCodeAssocMember = "X", ValActionCode1AssocMember = "F"}
                }
                };
                dataReaderMock.Setup(accessor => accessor.ReadRecordAsync<ApplValcodes>("CORE.VALCODES", "STAFF.STATUSES", true)).ReturnsAsync(staffStatusesResponse);

                var apiSettingsMock = new ApiSettings("settings") { BulkReadSize = 300 };

                AdvisorRepository repository = new AdvisorRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object, apiSettingsMock);

                return repository;
            }
       
        }

        [TestClass]
        public class AdvisorRepositoryTests_Get : AdvisorRepositoryTestsSetup
        {

            [TestInitialize]
            public void Initialize()
            {
                // Build the test repository
                advisorRepository = BuildMockAdvisorRepository();
            }

            [TestCleanup]
            public void TestCleanup()
            {
                advisorRepository = null;
            }

            [TestMethod]
            public async Task Get_AdvisorWithAdvisees_AllAttributesReturned()
            {
                Advisor advisor = await advisorRepository.GetAsync(advisorWithAdvisees);
                
                var personData = personResponse.Where(p => p.Recordkey == advisorWithAdvisees).FirstOrDefault();
                var facultyData = facultyResponse.Where(f => f.Recordkey == advisorWithAdvisees).FirstOrDefault();
                var studentAdvisementData = studentAdvisementResponse.Where(sa => facultyData.FacAdvisees.Contains(sa.Recordkey));
                var staffData = staffResponse.Where(s => s.Recordkey == advisorWithAdvisees).FirstOrDefault();

                Assert.AreEqual(personData.LastName, advisor.LastName);
                Assert.AreEqual(personData.FirstName, advisor.FirstName);
                Assert.AreEqual(personData.MiddleName, advisor.MiddleName);
                Assert.AreEqual("John A. Person3", advisor.Name);
                foreach (var email in personData.PeopleEmailEntityAssociation)
                {
                    var advisorEmail = advisor.EmailAddresses.Where(e => e.Value == email.PersonEmailAddressesAssocMember).FirstOrDefault();
                    Assert.AreEqual(email.PersonEmailTypesAssocMember, advisorEmail.TypeCode);
                    Assert.AreEqual(email.PersonPreferredEmailAssocMember.ToUpper() == "Y", advisorEmail.IsPreferred);
                }
                Assert.IsTrue(advisor.IsActive);
                Assert.AreEqual(studentAdvisementData.Select(sa=>sa.StadStudent).Distinct().Count(), advisor.Advisees.Count());
                foreach (var stuAdv in studentAdvisementData)
                {
                    Assert.IsTrue(advisor.Advisees.Contains(stuAdv.StadStudent));
                }
            }

            [TestMethod]
            public async Task Get_NotCurrentFacultyAdvisorWithAdvisees_InactiveAndNoAdviseesReturned()
            {
                Advisor advisor = await advisorRepository.GetAsync(notCurrentFacultyAdvisorWithAdvisees);
                Assert.AreEqual(false, advisor.IsActive);
                Assert.AreEqual(0, advisor.Advisees.Count());
            }

            [TestMethod]
            public async Task Get_StaffAdvisor_ActiveAndNoAdviseesReturned()
            {
                Advisor advisor = await advisorRepository.GetAsync(staffAdvisor);
                Assert.AreEqual(true, advisor.IsActive);
                Assert.AreEqual(0, advisor.Advisees.Count());
            }

            [TestMethod]
            public async Task Get_NonCurrentStaffAdvisor_Exception()
            {
                Advisor advisor = await advisorRepository.GetAsync(notCurrentStaffAdvisor);
                Assert.AreEqual(false, advisor.IsActive);
                Assert.AreEqual(0, advisor.Advisees.Count());
            }

            [TestMethod]
            public async Task Get_ExcludeFormerAdvisees()
            {
                // Although there are 4 student advisement records - two have an end date and should be excluded.  Only the one that starts in the future should be included.



                Advisor advisor = await advisorRepository.GetAsync(facultyWithCurrentFormerAdvisees, AdviseeInclusionType.ExcludeFormerAdvisees);
                Assert.AreEqual(true, advisor.IsActive);
                Assert.AreEqual(2, advisor.Advisees.Count());
                var adviseeId1 = advisor.Advisees.First();
                Assert.AreEqual("0000005", adviseeId1);
                var adviseeId2 = advisor.Advisees.Last();
                Assert.AreEqual("0000333", adviseeId2);

            }

            [TestMethod]
            public async Task Get_CurrentAdviseesOnly()
            {
                // Although there are 4 student advisement records - two have an end date and one is in future so only 1 should be returned.

                Advisor advisor = await advisorRepository.GetAsync(facultyWithFormerFutureAdvisees, AdviseeInclusionType.CurrentAdviseesOnly);
                Assert.AreEqual(true, advisor.IsActive);
                Assert.AreEqual(1, advisor.Advisees.Count());
                var adviseeId = advisor.Advisees.First();
                Assert.AreEqual("0000005", adviseeId);
            }

            [TestMethod]
            public async Task Get_NoAdvisees()
            {
                // Although there are 4 student advisement records, none should be returned. 

                Advisor advisor = await advisorRepository.GetAsync(facultyWithFormerFutureAdvisees, AdviseeInclusionType.NoAdvisees);
                Assert.AreEqual(0, advisor.Advisees.Count());

            }

            [TestMethod]
            public async Task Get_AllAdvisees()
            {
                // There are 4 student advisement records some former and some future. All should be returned.

                Advisor advisor = await advisorRepository.GetAsync(facultyWithFormerFutureAdvisees, AdviseeInclusionType.AllAdvisees);
                Assert.AreEqual(4, advisor.Advisees.Count());

            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task Get_NonStaff_Exception()
            {
                Advisor advisor = await advisorRepository.GetAsync(nonStaff);
            }


            [TestMethod]
            public async Task Get_Advisees()
            {
                Advisor advisor = await advisorRepository.GetAsync(advisorWithAdvisees);
                Assert.AreEqual(3, advisor.Advisees.Count());
                Assert.AreEqual("0000004", advisor.Advisees.ElementAt(0));
            }


            [TestMethod]
            public async Task Get_FacultyAdvisorWithNoAdvisees()
            {
                Advisor advisor = await advisorRepository.GetAsync(facultyAdvisorWithNoAdvisees);
                Assert.AreEqual(0, advisor.Advisees.Count());
            }

            [TestMethod]
            public async Task Get_FacultyAdvisorWithNoAdvisees_ActiveAndNoAdviseesReturned()
            {
                Advisor advisor = await advisorRepository.GetAsync(facultyAdvisorWithNoAdvisees);
                Assert.AreEqual(true, advisor.IsActive);
                Assert.AreEqual(0, advisor.Advisees.Count());
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task Get_InvalidAdvisor_Exception()
            {
                Advisor advisor = await advisorRepository.GetAsync(invalidAdvisor);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task Get_NullId_ThrowsException()
            {
                Advisor advisor = await advisorRepository.GetAsync(null);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task Get_EmptyId_ThrowsException()
            {
                Advisor advisor = await advisorRepository.GetAsync(string.Empty);
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task Get_EmptyPersonResponse_ThrowsException()
            {
                var noPersonResponse = new Collection<Base.DataContracts.Person>();
                dataReaderMock.Setup<Collection<Base.DataContracts.Person>>(acc => acc.BulkReadRecord<Base.DataContracts.Person>(It.IsAny<string[]>(), true)).Returns(noPersonResponse);
                Advisor advisor = await advisorRepository.GetAsync("0000099");
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task Get_NullPersonResponse_ThrowsException()
            {
                var nullPersonResponse = new Collection<Base.DataContracts.Person>();
                nullPersonResponse = null;
                dataReaderMock.Setup<Collection<Base.DataContracts.Person>>(acc => acc.BulkReadRecord<Base.DataContracts.Person>(It.IsAny<string[]>(), true)).Returns(nullPersonResponse);
                Advisor advisor = await advisorRepository.GetAsync("0000099");
            }
        }

        [TestClass]
        public class AdvisorRepositoryTests_GetAdvisors : AdvisorRepositoryTestsSetup
        {

            [TestInitialize]
            public void Initialize()
            {
                // Build the test repository
                advisorRepository = BuildMockAdvisorRepository();
            }

            [TestCleanup]
            public void TestCleanup()
            {
                advisorRepository = null;
            }

            [TestMethod]
            public async Task GetAdvisors_MultipleInputIds_MultipleAdvisorsReturned()
            {
                var advisorIds = new List<string>() { advisorWithAdvisees, facultyAdvisorWithNoAdvisees }.AsEnumerable();
                var advisors = await advisorRepository.GetAdvisorsAsync(advisorIds);

                // verify that only one advisor returned because the notCurrentStaffAdvisor will not return an advisor.
                Assert.AreEqual(2, advisors.Count());

                // Get the advisor with advisees and check data
                var advisor = advisors.Where(a => a.Id == advisorWithAdvisees).First();
                // check data for advisor with advisees object returned
                var personData = personResponse.Where(p => p.Recordkey == advisorWithAdvisees).FirstOrDefault();
                var facultyData = facultyResponse.Where(f => f.Recordkey == advisorWithAdvisees).FirstOrDefault();
                var studentAdvisementData = studentAdvisementResponse.Where(sa => facultyData.FacAdvisees.Contains(sa.Recordkey));
                var staffData = staffResponse.Where(s => s.Recordkey == advisorWithAdvisees).FirstOrDefault();
                
                Assert.AreEqual(personData.LastName, advisor.LastName);
                Assert.AreEqual(personData.FirstName, advisor.FirstName);
                Assert.AreEqual(personData.MiddleName, advisor.MiddleName);
                Assert.AreEqual("John A. Person3", advisor.Name);
                foreach (var email in personData.PeopleEmailEntityAssociation)
                {
                    var advisorEmail = advisor.EmailAddresses.Where(e => e.Value == email.PersonEmailAddressesAssocMember).FirstOrDefault();
                    Assert.AreEqual(email.PersonEmailTypesAssocMember, advisorEmail.TypeCode);
                    Assert.AreEqual(email.PersonPreferredEmailAssocMember.ToUpper() == "Y", advisorEmail.IsPreferred);
                }
                Assert.IsTrue(advisor.IsActive);
                Assert.AreEqual(studentAdvisementData.Select(sa => sa.StadStudent).Distinct().Count(), advisor.Advisees.Count());
                foreach (var stuAdv in studentAdvisementData)
                {
                    Assert.IsTrue(advisor.Advisees.Contains(stuAdv.StadStudent));
                }
                // Get the Advisor with no advisees and check data
                advisor = advisors.Where(a => a.Id == facultyAdvisorWithNoAdvisees).First();
                // check data for advisor with advisees object returned
                personData = personResponse.Where(p => p.Recordkey == facultyAdvisorWithNoAdvisees).FirstOrDefault();
                facultyData = facultyResponse.Where(f => f.Recordkey == facultyAdvisorWithNoAdvisees).FirstOrDefault();
                staffData = staffResponse.Where(s => s.Recordkey == facultyAdvisorWithNoAdvisees).FirstOrDefault();

                Assert.AreEqual(personData.LastName, advisor.LastName);
                Assert.AreEqual(personData.FirstName, advisor.FirstName);
                Assert.AreEqual(personData.MiddleName, advisor.MiddleName);
                Assert.AreEqual("Person6", advisor.Name);
                Assert.IsTrue(advisor.IsActive);
                Assert.AreEqual(0, advisor.Advisees.Count());
            }

            [TestMethod]
            public async Task GetAdvisors_OneNotFoundOneFound_ReturnsOneFoundAdvisor()
            {
                var advisorIds = new List<string>() { invalidAdvisor, notCurrentStaffAdvisor }.AsEnumerable();
                var advisors = await advisorRepository.GetAdvisorsAsync(advisorIds);
                Assert.AreEqual(1, advisors.Count());
                Assert.AreEqual(notCurrentStaffAdvisor, advisors.ElementAt(0).Id);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task GetAdvisors_NullList_ThrowsException()
            {
                var advisors = await advisorRepository.GetAdvisorsAsync(null);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task GetAdvisors_EmptyList_ThrowsException()
            {
                var advisors = await advisorRepository.GetAdvisorsAsync(new List<string>());
            }

            [TestMethod]
            public async Task GetAdvisors_NotFoundIds_ReturnsEmptyList()
            {
                // Except when invalid advisor--then return a null person
                var advisors = await advisorRepository.GetAdvisorsAsync(new List<string>() { invalidAdvisor, nonStaff });
                Assert.AreEqual(0, advisors.Count());
            }

            [TestMethod]
            public async Task GetAdvisors_EmptyPersonResponse_ThrowsException()
            {
                var noPersonResponse = new Collection<Base.DataContracts.Person>();
                dataReaderMock.Setup<Collection<Base.DataContracts.Person>>(acc => acc.BulkReadRecord<Base.DataContracts.Person>(It.IsAny<string[]>(), true)).Returns(noPersonResponse);
                var advisors = await advisorRepository.GetAdvisorsAsync(new List<string>(){"0000099"});
                Assert.IsTrue(advisors.Count() == 0);
            }

            [TestMethod]
            public async Task GetAdvisors_NullPersonResponse_ThrowsException()
            {
                var nullPersonResponse = new Collection<Base.DataContracts.Person>();
                nullPersonResponse = null;
                dataReaderMock.Setup(acc => acc.BulkReadRecord<Base.DataContracts.Person>(It.IsAny<string[]>(), true)).Returns(nullPersonResponse);
                var advisors = await advisorRepository.GetAdvisorsAsync(new List<string>() { "0000099" });
                Assert.IsTrue(advisors.Count() == 0);
            }

            [TestMethod]
            public async Task GetAdvisors_OnlyActiveAdvisees_NowExcludeFormer()
            {

                var advisors = await advisorRepository.GetAdvisorsAsync(new List<string>() { facultyWithCurrentFormerAdvisees }, AdviseeInclusionType.ExcludeFormerAdvisees);

                dataReaderMock.Verify(reader => reader.BulkReadRecordAsync<StudentAdvisement>("STUDENT.ADVISEMENT", It.IsAny<string[]>(), It.IsAny<bool>()), Times.Once);
                Assert.IsNotNull(advisors);
                Assert.AreEqual(1, advisors.Count(), "Only a single advisor should be returned.");
                Assert.AreEqual(2, advisors.ElementAt(0).Advisees.Count(), "Count of active advisees does not match count of active advisements");
            }

            [TestMethod]
            public async Task GetAdvisors_OnlyActiveAdvisees_CurrentAdviseesOnly()
            {

                var advisors = await advisorRepository.GetAdvisorsAsync(new List<string>() { facultyWithFormerFutureAdvisees }, AdviseeInclusionType.CurrentAdviseesOnly);

                dataReaderMock.Verify(reader => reader.BulkReadRecordAsync<StudentAdvisement>("STUDENT.ADVISEMENT", It.IsAny<string[]>(), It.IsAny<bool>()), Times.Once);
                Assert.IsNotNull(advisors);
                Assert.AreEqual(1, advisors.Count(), "Only a single advisor should be returned.");
                Assert.AreEqual(1, advisors.ElementAt(0).Advisees.Count(), "Count of active advisees does not match count of active advisements");
            }


            [TestMethod]
            public async Task GetAdvisors_advisee_data_excluded_when_NoAdvisees_chosen()
            {
                var activeAdvisements = studentAdvisementResponse.Where(x => x.StadEndDate >= DateTime.Today || x.StadEndDate == null && x.Recordkey == advisorWithAdvisees);
                dataReaderMock.Setup(reader => reader.SelectAsync("STUDENT.ADVISEMENT", It.IsAny<string[]>(), It.IsAny<string>())).Returns((string s1, string[] s2, string s3) => Task.FromResult(activeAdvisements.Where(x => s2.Contains(x.Recordkey)).Select(x => x.Recordkey).ToArray()));
                dataReaderMock.Setup(reader => reader.BulkReadRecordAsync<StudentAdvisement>("STUDENT.ADVISEMENT", It.IsAny<string[]>(), It.IsAny<bool>())).ReturnsAsync(new Collection<StudentAdvisement>());
                var advisors = await advisorRepository.GetAdvisorsAsync(new List<string>() { advisorWithAdvisees }, AdviseeInclusionType.NoAdvisees);

                Assert.IsNotNull(advisors);
                Assert.AreEqual(1, advisors.Count(), "Only a single advisor should be returned.");
                // Verify data reader select/bulk read calls for STUDENT.ADVISEMENT did not occur
                dataReaderMock.Verify(reader => reader.SelectAsync("STUDENT.ADVISEMENT", It.IsAny<string[]>(), It.IsAny<string>()), Times.Never());
                dataReaderMock.Verify(reader => reader.BulkReadRecordAsync<StudentAdvisement>("STUDENT.ADVISEMENT", It.IsAny<string[]>(), It.IsAny<bool>()), Times.Never());
                Assert.AreEqual(0, advisors.ElementAt(0).Advisees.Count(), "Count of active advisees does not match count of active advisements");
            }

        }

        [TestClass]
        public class AdvisorRepository_SearchAdvisorByName
        {

            protected Mock<IColleagueTransactionFactory> transFactoryMock;
            protected Mock<ObjectCache> localCacheMock;
            protected Mock<ICacheProvider> cacheProviderMock;
            protected Mock<IColleagueDataReader> dataReaderMock;
            protected Mock<ILogger> loggerMock;
            protected Mock<IColleagueTransactionInvoker> transManagerMock;

            protected Mock<AdvisorRepository> advisorRepoMock;

            AdvisorRepository advisorRepository;

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
                advisorRepository = BuildMockAdvisorRepository();
            }

            private AdvisorRepository BuildMockAdvisorRepository()
            {

                var apiSettingsMock = new ApiSettings("null");

                AdvisorRepository repository = new AdvisorRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object, apiSettingsMock);

                return repository;
            }


            [TestCleanup]
            public void TestCleanup()
            {
                advisorRepository = null;
            }

            [TestMethod]
            public async Task SearchAdvisorByName_ReturnsEmptyListIfCalledWithEmptyNames()
            {
                var result = await advisorRepository.SearchAdvisorByNameAsync(string.Empty, string.Empty, string.Empty);
                Assert.AreEqual(0, result.Count());
            }

            [TestMethod]
            public async Task SearchAdvisorByName_ReturnsEmptyListIfCalledWithNullNames()
            {
                var result = await advisorRepository.SearchAdvisorByNameAsync(null);
                Assert.AreEqual(0, result.Count());
            }

            [TestMethod]
            public async Task SearchAdvisorByName_ReturnsEmptyListIfPersonSearchByNameReturnsEmptyList()
            {
                var emptyPersonIdsList = new string[] { };
                dataReaderMock.Setup(acc => acc.Select("PERSON", It.IsAny<string>())).Returns(emptyPersonIdsList);
                var lastName = "Gerbil";
                var result = await advisorRepository.SearchAdvisorByNameAsync(lastName);
                Assert.AreEqual(0, result.Count());
            }

            [TestMethod]
            public async Task SearchAdvisorByName_ReturnsEmptyListIfPersonSearchByNameReturnsNull()
            {
                string[] nullPersonIdsList = null;
                dataReaderMock.Setup(acc => acc.Select("PERSON", It.IsAny<string[]>(), It.IsAny<string>())).Returns(nullPersonIdsList);
                var lastName = "Gerbil";
                var result = await advisorRepository.SearchAdvisorByNameAsync(lastName);
                Assert.AreEqual(0, result.Count());
            }

            [TestMethod]
            public async Task SearchAdvisorByName_ReturnsEmptyListIfFacultySelectReturnsNull()
            {
                var personIdsList = new string[] { "001", "002", "003" };
                string[] nullFacultyIdsList = null;
                dataReaderMock.Setup(acc => acc.Select("PERSON", It.IsAny<string>())).Returns(personIdsList);
                dataReaderMock.Setup(acc => acc.Select("FACULTY", It.IsAny<string[]>(), It.IsAny<string>())).Returns(nullFacultyIdsList);
                var lastName = "Gerbil";
                var result = await advisorRepository.SearchAdvisorByNameAsync(lastName);
                Assert.AreEqual(0, result.Count());
            }

            [TestMethod]
            public async Task AdvisorSearchByName_ReturnsEmptyListIfFacultySelectReturnsEmptyList()
            {
                var personIdsList = new string[] { "001", "002", "003" };
                var blankStudentIdsList = new string[] { };
                var lastName = "Gerbil";
                dataReaderMock.Setup(acc => acc.SelectAsync("PERSON", It.IsAny<string>())).ReturnsAsync(personIdsList);
                dataReaderMock.Setup(acc => acc.SelectAsync("FACULTY", It.IsAny<string[]>(), It.IsAny<string>())).ReturnsAsync(blankStudentIdsList);
                var result = await advisorRepository.SearchAdvisorByNameAsync(lastName);
                Assert.AreEqual(0, result.Count());
            }
        }

        [TestClass]
        public class AdvisorRepository_SearchAdvisorByNameForExactMatchAsync
        {

            protected Mock<IColleagueTransactionFactory> transFactoryMock;
            protected Mock<ObjectCache> localCacheMock;
            protected Mock<ICacheProvider> cacheProviderMock;
            protected Mock<IColleagueDataReader> dataReaderMock;
            protected Mock<ILogger> loggerMock;
            protected Mock<IColleagueTransactionInvoker> transManagerMock;

            protected Mock<AdvisorRepository> advisorRepoMock;

            AdvisorRepository advisorRepository;

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

                transFactoryMock.Setup(transFac => transFac.GetDataReader()).Returns(dataReaderMock.Object);
                // Set up transManagerMock as the object for the transaction manager
                transFactoryMock.Setup(transFac => transFac.GetTransactionInvoker()).Returns(transManagerMock.Object);

                // Mock the call to get a person key list
                var lookupStringResponse = new GetPersonSearchKeyListResponse() { ErrorMessage = "", KeyList = new List<string>() { "12345", "67890" } };
                transManagerMock.Setup(manager => manager
                        .ExecuteAsync<GetPersonSearchKeyListRequest, GetPersonSearchKeyListResponse>(It.IsAny<GetPersonSearchKeyListRequest>()))
                        .ReturnsAsync(lookupStringResponse);

                cacheProviderMock.Setup<Task<Tuple<object, SemaphoreSlim>>>(x =>
                    x.GetAndLockSemaphoreAsync(It.IsAny<string>(), null))
                    .ReturnsAsync(new Tuple<object, SemaphoreSlim>(null, new SemaphoreSlim(1, 1)));

                // Build the test repository
                advisorRepository = BuildMockAdvisorRepository();
            }

            private AdvisorRepository BuildMockAdvisorRepository()
            {

                var apiSettingsMock = new ApiSettings("null");

                AdvisorRepository repository = new AdvisorRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object, apiSettingsMock);

                return repository;
            }


            [TestCleanup]
            public void TestCleanup()
            {
                advisorRepository = null;
            }

            [TestMethod]
            public async Task SearchAdvisorByNameForExactMatchAsync_ReturnsEmptyListIfCalledWithEmptyNames()
            {
                var result = await advisorRepository.SearchAdvisorByNameForExactMatchAsync(string.Empty, string.Empty, string.Empty);
                Assert.AreEqual(0, result.Count());
            }

            [TestMethod]
            public async Task SearchAdvisorByNameForExactMatchAsync_ReturnsEmptyListIfCalledWithNullNames()
            {
                var result = await advisorRepository.SearchAdvisorByNameForExactMatchAsync(null);
                Assert.AreEqual(0, result.Count());
            }

            [TestMethod]
            public async Task SearchAdvisorByNameForExactMatchAsync_ReturnsEmptyListIfPersonSearchByNameReturnsEmptyList()
            {
                // Mock the call to get a person key list
                var lookupStringResponse = new GetPersonSearchKeyListResponse() { ErrorMessage = "", KeyList = new List<string>()  };
                transManagerMock.Setup(manager => manager
                        .ExecuteAsync<GetPersonSearchKeyListRequest, GetPersonSearchKeyListResponse>(It.IsAny<GetPersonSearchKeyListRequest>()))
                        .ReturnsAsync(lookupStringResponse);
                var lastName = "Gerbil";
                var result = await advisorRepository.SearchAdvisorByNameForExactMatchAsync(lastName);
                Assert.AreEqual(0, result.Count());
            }

            [TestMethod]
            public async Task SearchAdvisorByNameForExactMatchAsync_ReturnsEmptyListIfPersonSearchByNameReturnsNull()
            {
                // Mock the call to get a person key list
                transManagerMock.Setup(manager => manager
                        .ExecuteAsync<GetPersonSearchKeyListRequest, GetPersonSearchKeyListResponse>(It.IsAny<GetPersonSearchKeyListRequest>()))
                        .ReturnsAsync(() => null);
                var lastName = "Gerbil";
                var result = await advisorRepository.SearchAdvisorByNameForExactMatchAsync(lastName);
                Assert.AreEqual(0, result.Count());
            }

            [TestMethod]
            public async Task SearchAdvisorByNameForExactMatchAsync_ReturnsEmptyListIfFacultySelectReturnsNull()
            {
                var personIdsList = new string[] { "001", "002", "003" };
                string[] nullFacultyIdsList = null;
                // Mock the call to get a person key list
                var lookupStringResponse = new GetPersonSearchKeyListResponse() { ErrorMessage = "", KeyList = personIdsList.ToList() };
                transManagerMock.Setup(manager => manager
                        .ExecuteAsync<GetPersonSearchKeyListRequest, GetPersonSearchKeyListResponse>(It.IsAny<GetPersonSearchKeyListRequest>()))
                        .ReturnsAsync(lookupStringResponse);
                dataReaderMock.Setup(acc => acc.Select("FACULTY", It.IsAny<string[]>(), It.IsAny<string>())).Returns(nullFacultyIdsList);
                var lastName = "Gerbil";
                var result = await advisorRepository.SearchAdvisorByNameForExactMatchAsync(lastName);
                Assert.AreEqual(0, result.Count());
            }

            [TestMethod]
            public async Task SearchAdvisorByNameForExactMatchAsync_ReturnsEmptyListIfFacultySelectReturnsEmptyList()
            {
                var personIdsList = new string[] { "001", "002", "003" };
                var blankStudentIdsList = new string[] { };
                var lastName = "Gerbil";
                // Mock the call to get a person key list
                var lookupStringResponse = new GetPersonSearchKeyListResponse() { ErrorMessage = "", KeyList = personIdsList.ToList() };
                transManagerMock.Setup(manager => manager
                        .ExecuteAsync<GetPersonSearchKeyListRequest, GetPersonSearchKeyListResponse>(It.IsAny<GetPersonSearchKeyListRequest>()))
                        .ReturnsAsync(lookupStringResponse);
                dataReaderMock.Setup(acc => acc.SelectAsync("FACULTY", It.IsAny<string[]>(), It.IsAny<string>())).ReturnsAsync(blankStudentIdsList);
                var result = await advisorRepository.SearchAdvisorByNameForExactMatchAsync(lastName);
                Assert.AreEqual(0, result.Count());
            }
            [TestMethod]
            public async Task SearchAdvisorByNameForExactMatchAsync_ReturnsWhenAllPersonsAreFaculties()
            {
                var personIdsList = new string[] { "001", "002", "003" };
                var blankStudentIdsList = new string[] { };
                var lastName = "Gerbil";
                // Mock the call to get a person key list
                var lookupStringResponse = new GetPersonSearchKeyListResponse() { ErrorMessage = "", KeyList = personIdsList.ToList() };
                transManagerMock.Setup(manager => manager
                        .ExecuteAsync<GetPersonSearchKeyListRequest, GetPersonSearchKeyListResponse>(It.IsAny<GetPersonSearchKeyListRequest>()))
                        .ReturnsAsync(lookupStringResponse);
                dataReaderMock.Setup(acc => acc.SelectAsync("FACULTY", It.IsAny<string[]>(), It.IsAny<string>())).ReturnsAsync(personIdsList);
                var result = await advisorRepository.SearchAdvisorByNameForExactMatchAsync(lastName);
                Assert.AreEqual(3, result.Count());
                Assert.AreEqual("001", result.ToList()[0]);
                Assert.AreEqual("002", result.ToList()[1]);
                Assert.AreEqual("003", result.ToList()[2]);
            }
        }


    }
}
