using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ellucian.Web.Adapters;
using Moq;
using Ellucian.Colleague.Domain.Student.Repositories;
using slf4net;
using System.Collections.Generic;
using Ellucian.Colleague.Coordination.Student.Services;
using System.Linq;
using System.Threading.Tasks;
using Ellucian.Colleague.Domain.Entities;
using Ellucian.Web.Security;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Colleague.Domain.Student.Entities;
using Ellucian.Colleague.Domain.Student;
using Ellucian.Colleague.Coordination.Student.Adapters;
using Ellucian.Colleague.Domain.Base.Repositories;

namespace Ellucian.Colleague.Coordination.Student.Tests.Services
{
    [TestClass]
    public class TestResultServiceTests
    {
        // Sets up a Current user that is a student and one that is an advisor
        public abstract class CurrentUserSetup
        {
            protected Role advisorRole = new Role(105, "Advisor");

            public class StudentUserFactory : ICurrentUserFactory
            {
                public ICurrentUser CurrentUser
                {
                    get
                    {
                        return new CurrentUser(new Claims()
                        {
                            ControlId = "123",
                            Name = "Johnny",
                            PersonId = "0000894",
                            SecurityToken = "321",
                            SessionTimeout = 30,
                            UserName = "Student",
                            Roles = new List<string>() { },
                            SessionFixationId = "abc123"
                        });
                    }
                }
            }

            public class AdvisorUserFactory : ICurrentUserFactory
            {
                public ICurrentUser CurrentUser
                {
                    get
                    {
                        return new CurrentUser(new Claims()
                        {
                            ControlId = "123",
                            Name = "George",
                            PersonId = "0000111",
                            SecurityToken = "321",
                            SessionTimeout = 30,
                            UserName = "Advisor",
                            Roles = new List<string>() { "Advisor" },
                            SessionFixationId = "abc123"
                        });
                    }
                }
            }
        }
        [TestClass]
        public class GetAsync_AsStudentUser : CurrentUserSetup
        {
            private Mock<ITestResultRepository> testResultRepositoryMock;
            private ITestResultRepository testResultRepository;
            private Mock<IStudentRepository> studentRepositoryMock;
            private IStudentRepository studentRepository;
            private Mock<IAdapterRegistry> adapterRegistryMock;
            private IAdapterRegistry adapterRegistry;
            private ILogger logger;
            private List<Ellucian.Colleague.Domain.Student.Entities.TestResult> testResults;
            //private BaseAdapter<Ellucian.Colleague.Domain.Student.Entities.TestResult, Ellucian.Colleague.Dtos.Student.TestResult> testResultDtoAdapter;
            private Mock<IRoleRepository> roleRepoMock;
            private IRoleRepository roleRepo;
            private ICurrentUserFactory currentUserFactory;
            private TestResultService testResultService;
            private Domain.Student.Entities.Student student;
            private Domain.Student.Entities.Student unauthorizedStudent;
            private IConfigurationRepository baseConfigurationRepository;
            private Mock<IConfigurationRepository> baseConfigurationRepositoryMock;

            [TestInitialize]
            public void Initialize()
            {
                testResultRepositoryMock = new Mock<ITestResultRepository>();
                testResultRepository = testResultRepositoryMock.Object;
                studentRepositoryMock = new Mock<IStudentRepository>();
                studentRepository = studentRepositoryMock.Object;

                adapterRegistryMock = new Mock<IAdapterRegistry>();
                adapterRegistry = adapterRegistryMock.Object;
                roleRepoMock = new Mock<IRoleRepository>();
                roleRepo = roleRepoMock.Object;
                logger = new Mock<ILogger>().Object;
                baseConfigurationRepositoryMock = new Mock<IConfigurationRepository>();
                baseConfigurationRepository = baseConfigurationRepositoryMock.Object;

                // Mock student repo response
                student = new Domain.Student.Entities.Student("0000894", "Smith", 2, new List<string>() { "BA.ENGL" }, new List<string>()) { FirstName = "Bob", MiddleName = "Blakely" };
                studentRepositoryMock.Setup(repo => repo.GetAsync("0000894")).ReturnsAsync(student);
                unauthorizedStudent = new Domain.Student.Entities.Student("00004002", "Jones", 802, new List<string>() { "BA.MATH" }, new List<string>());
                studentRepositoryMock.Setup(repo => repo.GetAsync("00004002")).ReturnsAsync(unauthorizedStudent);

                // Mock the TestResultRepository response

                var test1 = new TestResult("0000894", "Code1", "Test 1 Description", new DateTime(2015, 01, 01), TestType.Admissions);
                test1.Score = 10.5m;
                var subtest1 = new SubTestResult("SUB", "Subtest", DateTime.Now.AddDays(-30));
                subtest1.Score = 11.5m;
                test1.SubTests = new List<SubTestResult>() { subtest1 };
                var comptest1 = new ComponentTest("COMP", 12.5m, 15);
                test1.ComponentTests = new List<ComponentTest>() { comptest1 };
                var test2 = new TestResult("0000894", "Code2", "Test 2 Description", new DateTime(2015, 02, 01), TestType.Admissions);
                test2.Score = null;
                var test3 = new TestResult("0000894", "Code3", "Test 3 Description", new DateTime(2015, 03, 01), TestType.Admissions);
                test3.Score = 1.333m;
                var test4 = new TestResult("0000894", "Code4", "Test 4 Description", new DateTime(2015, 04, 01), TestType.Placement);
                test4.Score = 3m;
                var test5 = new TestResult("0000894", "Code5", "Test 5 Description", new DateTime(2015, 05, 01), TestType.Other);
                test5.Score = 0m;
                var test6 = new TestResult("0000894", "Code6", "Test 6 Description", new DateTime(2015, 06, 01), TestType.Other);
                test6.Score = 4.55m;
                testResults = new List<Ellucian.Colleague.Domain.Student.Entities.TestResult>() { test1, test2, test3, test4, test5, test6};
                
                testResultRepositoryMock.Setup(x => x.GetAsync(It.IsAny<string>())).Returns(Task.FromResult<IEnumerable<Ellucian.Colleague.Domain.Student.Entities.TestResult>>(testResults));

                // Mock Adapters
                var testResultDtoAdapter = new TestResultEntityToTestResultDtoAdapter(adapterRegistry, logger);
                adapterRegistryMock.Setup(areg => areg.GetAdapter<Ellucian.Colleague.Domain.Student.Entities.TestResult, Ellucian.Colleague.Dtos.Student.TestResult>()).Returns(testResultDtoAdapter);
                var subtestResultDtoAdapter = new SubTestResultEntityToSubTestResultDtoAdapter(adapterRegistry, logger);
                adapterRegistryMock.Setup(areg => areg.GetAdapter<Ellucian.Colleague.Domain.Student.Entities.SubTestResult, Ellucian.Colleague.Dtos.Student.SubTestResult>()).Returns(subtestResultDtoAdapter);
                var componentTestDtoAdapter = new ComponentTestEntityToComponentTestDtoAdapter(adapterRegistry, logger);
                adapterRegistryMock.Setup(areg => areg.GetAdapter<Ellucian.Colleague.Domain.Student.Entities.ComponentTest, Ellucian.Colleague.Dtos.Student.ComponentTest>()).Returns(componentTestDtoAdapter);
                // Set up current user
                currentUserFactory = new CurrentUserSetup.StudentUserFactory();

                testResultService = new TestResultService(adapterRegistry, studentRepository, testResultRepository, currentUserFactory, roleRepo, logger, baseConfigurationRepository);
            }

            [TestCleanup]
            public void Cleanup()
            {
                testResultRepository = null;
                testResults = null;
                adapterRegistry = null;
                testResultService = null;
            }

            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public async Task ThrowsErrorIfNotSelf()
            {
                await testResultService.GetAsync("0004002","");
            }

            [TestMethod]
            public async Task ReturnsAllTestsForStudent()
            {
                var result = await testResultService.GetAsync("0000894", "");
                Assert.AreEqual(testResults.Count(), result.Count());
            }

            [TestMethod]
            public async Task ReturnsAllTestsForStudent_TestIntegerScores()
            {
                var result = await testResultService.GetAsync("0000894", "");
                Assert.AreEqual(11, result.ElementAt(0).Score);
                Assert.IsNull(result.ElementAt(1).Score);
                Assert.AreEqual(1, result.ElementAt(2).Score);
                Assert.AreEqual(3, result.ElementAt(3).Score);
                Assert.AreEqual(0, result.ElementAt(4).Score);
                Assert.AreEqual(5, result.ElementAt(5).Score);
                var subtests = result.ElementAt(0).SubTests;
                Assert.AreEqual(12, subtests.ElementAt(0).Score);
                var componentTests = result.ElementAt(0).ComponentTests;
                Assert.AreEqual(13, componentTests.ElementAt(0).Score);
            }

            [TestMethod]
            public async Task ReturnsAdmissionTestsForStudent()
            {
                var result = await testResultService.GetAsync("0000894", "admissions");
                Assert.AreEqual(testResults.Count(t => t.Category == TestType.Admissions), result.Count());
            }

            [TestMethod]
            public async Task ReturnsPlacementTestsForStudent()
            {
                var result = await testResultService.GetAsync("0000894", "placement");
                Assert.AreEqual(testResults.Count(t => t.Category == TestType.Placement), result.Count());
            }

            [TestMethod]
            public async Task ReturnsOtherTestsForStudent()
            {
                var result = await testResultService.GetAsync("0000894", "other");
                Assert.AreEqual(testResults.Count(t => t.Category == TestType.Other), result.Count());
            }

            [TestMethod]
            public async Task ReturnsNoTestsIfRepositoryRetrievesNoTests()
            {
                testResultRepositoryMock.Setup(x => x.GetAsync(It.IsAny<string>())).Returns(Task.FromResult<IEnumerable<Ellucian.Colleague.Domain.Student.Entities.TestResult>>(new List<Ellucian.Colleague.Domain.Student.Entities.TestResult>()));
                var result = await testResultService.GetAsync("0000894", "");
                Assert.AreEqual(0, result.Count());
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task ThrowsExceptionIfRepositoryThrowsException()
            {
                testResultRepositoryMock.Setup(x => x.GetAsync(It.IsAny<string>())).Throws(new ArgumentNullException());
                var result = await testResultService.GetAsync("0000894", "");
                Assert.AreEqual(0, result.Count());
            }
        }

        [TestClass]
        public class GetAsync_AsStudentAdvisorUser : CurrentUserSetup
        {
            private Mock<ITestResultRepository> testResultRepositoryMock;
            private ITestResultRepository testResultRepository;
            private Mock<IStudentRepository> studentRepositoryMock;
            private IStudentRepository studentRepository;
            private Mock<IAdapterRegistry> adapterRegistryMock;
            private IAdapterRegistry adapterRegistry;
            private ILogger logger;
            private List<Ellucian.Colleague.Domain.Student.Entities.TestResult> testResults;
            //private AutoMapperAdapter<Ellucian.Colleague.Domain.Student.Entities.TestResult, Ellucian.Colleague.Dtos.Student.TestResult> testResultDtoAdapter;
            private Mock<IRoleRepository> roleRepoMock;
            private IRoleRepository roleRepo;
            private ICurrentUserFactory currentUserFactory;
            private TestResultService testResultService;
            private Domain.Student.Entities.Student student;
            private Domain.Student.Entities.Student unauthorizedStudent;
            private IConfigurationRepository baseConfigurationRepository;
            private Mock<IConfigurationRepository> baseConfigurationRepositoryMock;

            [TestInitialize]
            public void Initialize()
            {
                testResultRepositoryMock = new Mock<ITestResultRepository>();
                testResultRepository = testResultRepositoryMock.Object;
                studentRepositoryMock = new Mock<IStudentRepository>();
                studentRepository = studentRepositoryMock.Object;

                adapterRegistryMock = new Mock<IAdapterRegistry>();
                adapterRegistry = adapterRegistryMock.Object;
                roleRepoMock = new Mock<IRoleRepository>();
                roleRepo = roleRepoMock.Object;
                logger = new Mock<ILogger>().Object;
                baseConfigurationRepositoryMock = new Mock<IConfigurationRepository>();
                baseConfigurationRepository = baseConfigurationRepositoryMock.Object;

                // Mock student repo response
                student = new Domain.Student.Entities.Student("0000894", "Smith", 2, new List<string>() { "BA.ENGL" }, new List<string>()) { FirstName = "Bob", MiddleName = "Blakely" };
                studentRepositoryMock.Setup(repo => repo.GetAsync("0000894")).ReturnsAsync(student);
                unauthorizedStudent = new Domain.Student.Entities.Student("00004002", "Jones", 802, new List<string>() { "BA.MATH" }, new List<string>());
                studentRepositoryMock.Setup(repo => repo.GetAsync("00004002")).ReturnsAsync(unauthorizedStudent);

                // Mock the TestResultRepository response

                var test1 = new TestResult("0000894", "Code1", "Test 1 Description", new DateTime(2015, 01, 01), TestType.Admissions);
                test1.Score = 10.25m;
                var test2 = new TestResult("0000894", "Code2", "Test 2 Description", new DateTime(2015, 02, 01), TestType.Admissions);
                test2.Score = null;
                var test3 = new TestResult("0000894", "Code3", "Test 3 Description", new DateTime(2015, 03, 01), TestType.Admissions);
                test3.Score = 1.333m;
                var test4 = new TestResult("0000894", "Code4", "Test 4 Description", new DateTime(2015, 04, 01), TestType.Placement);
                test4.Score = 3m;
                var test5 = new TestResult("0000894", "Code5", "Test 5 Description", new DateTime(2015, 05, 01), TestType.Other);
                test5.Score = decimal.MaxValue;
                var test6 = new TestResult("0000894", "Code6", "Test 6 Description", new DateTime(2015, 06, 01), TestType.Other);
                test5.Score = 4.55m;
                testResults = new List<Ellucian.Colleague.Domain.Student.Entities.TestResult>() { test1, test2, test3, test4, test5, test6 };

                testResultRepositoryMock.Setup(x => x.GetAsync(It.IsAny<string>())).Returns(Task.FromResult<IEnumerable<Ellucian.Colleague.Domain.Student.Entities.TestResult>>(testResults));

                // Mock Adapters
                var testResultDtoAdapter = new TestResultEntityToTestResultDtoAdapter(adapterRegistry, logger);
                adapterRegistryMock.Setup(areg => areg.GetAdapter<Ellucian.Colleague.Domain.Student.Entities.TestResult, Ellucian.Colleague.Dtos.Student.TestResult>()).Returns(testResultDtoAdapter);

                // Set up current user
                currentUserFactory = new CurrentUserSetup.AdvisorUserFactory();

                testResultService = new TestResultService(adapterRegistry, studentRepository, testResultRepository, currentUserFactory, roleRepo, logger, baseConfigurationRepository);
            }

            [TestCleanup]
            public void Cleanup()
            {
                testResultRepository = null;
                testResults = null;
                adapterRegistry = null;
                testResultService = null;
            }

            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public async Task ThrowsErrorWhenAdvisorDoesNotHaveCorrectRole()
            {
                
                await testResultService.Get2Async("0000894", "");
            }

            [TestMethod]
            public async Task ReturnsAll_AdvisorHasViewStudentInformationPermission()
            {
                // Set up needed permission
                advisorRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(StudentPermissionCodes.ViewStudentInformation));
                roleRepoMock.Setup(rpm => rpm.GetRolesAsync()).ReturnsAsync(new List<Role>() { advisorRole });
                roleRepoMock.Setup(repo => repo.Roles).Returns(new List<Role>() { advisorRole });
                var result = await testResultService.GetAsync("0000894", "");
                Assert.AreEqual(testResults.Count(), result.Count());
            }

            [TestMethod]
            public async Task ReturnsAll_AdvisorHasViewAnyAdviseePermission()
            {
                // Set up needed permission
                advisorRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(StudentPermissionCodes.ViewStudentInformation));
                roleRepoMock.Setup(rpm => rpm.GetRolesAsync()).ReturnsAsync(new List<Role>() { advisorRole });
                roleRepoMock.Setup(repo => repo.Roles).Returns(new List<Role>() { advisorRole });
                var result = await testResultService.GetAsync("0000894", "");
                Assert.AreEqual(testResults.Count(), result.Count());
            }


            [TestMethod]
            public async Task ReturnAll_ViewAssignedAdviseesPermission()
            {
                // Arrange
                // Set up needed permission
                advisorRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(PlanningPermissionCodes.ViewAssignedAdvisees));
                roleRepoMock.Setup(rpm => rpm.GetRolesAsync()).ReturnsAsync(new List<Role>() { advisorRole });
                // Mock student repo responses so that the student has this assigned advisor
                student = new Domain.Student.Entities.Student("0000894", "Smith", 2, new List<string>() { "BA.ENGL" }, new List<string>()) { FirstName = "Bob", MiddleName = "Blakely" };
                student.AddAdvisement("0000111", null, null, "major");
                student.AddAdvisor("0000111");
                studentRepositoryMock.Setup(repo => repo.GetAsync("0000894")).ReturnsAsync(student);
                StudentAccess studentAccess = new StudentAccess("0000894");
                studentAccess.AddAdvisement("0000111", null, null, "major");
                List<string> ids = new List<string>() { "0000894" };
                List<StudentAccess> listStudentAccess = new List<StudentAccess>() { studentAccess };
                studentRepositoryMock.Setup(repo => repo.GetStudentAccessAsync(ids)).ReturnsAsync(listStudentAccess);

                // Act
                var result = await testResultService.GetAsync("0000894", "");
                // Assert
                Assert.AreEqual(testResults.Count(), result.Count());
            }
        }

        [TestClass]
        public class Get2Async_AsStudentUser : CurrentUserSetup
        {
            private Mock<ITestResultRepository> testResultRepositoryMock;
            private ITestResultRepository testResultRepository;
            private Mock<IStudentRepository> studentRepositoryMock;
            private IStudentRepository studentRepository;
            private Mock<IAdapterRegistry> adapterRegistryMock;
            private IAdapterRegistry adapterRegistry;
            private ILogger logger;
            private List<Ellucian.Colleague.Domain.Student.Entities.TestResult> testResults;
            private AutoMapperAdapter<Ellucian.Colleague.Domain.Student.Entities.TestResult, Ellucian.Colleague.Dtos.Student.TestResult2> testResultDtoAdapter;
            private AutoMapperAdapter<Ellucian.Colleague.Domain.Student.Entities.SubTestResult, Ellucian.Colleague.Dtos.Student.SubTestResult2> subTestResultDtoAdapter;
            private AutoMapperAdapter<Ellucian.Colleague.Domain.Student.Entities.ComponentTest, Ellucian.Colleague.Dtos.Student.ComponentTest2> componentTestDtoAdapter;
            private Mock<IRoleRepository> roleRepoMock;
            private IRoleRepository roleRepo;
            private ICurrentUserFactory currentUserFactory;
            private TestResultService testResultService;
            private Domain.Student.Entities.Student student;
            private Domain.Student.Entities.Student unauthorizedStudent;
            private IConfigurationRepository baseConfigurationRepository;
            private Mock<IConfigurationRepository> baseConfigurationRepositoryMock;

            [TestInitialize]
            public void Initialize()
            {
                testResultRepositoryMock = new Mock<ITestResultRepository>();
                testResultRepository = testResultRepositoryMock.Object;
                studentRepositoryMock = new Mock<IStudentRepository>();
                studentRepository = studentRepositoryMock.Object;

                adapterRegistryMock = new Mock<IAdapterRegistry>();
                adapterRegistry = adapterRegistryMock.Object;
                roleRepoMock = new Mock<IRoleRepository>();
                roleRepo = roleRepoMock.Object;
                logger = new Mock<ILogger>().Object;
                baseConfigurationRepositoryMock = new Mock<IConfigurationRepository>();
                baseConfigurationRepository = baseConfigurationRepositoryMock.Object;

                // Mock student repo response
                student = new Domain.Student.Entities.Student("0000894", "Smith", 2, new List<string>() { "BA.ENGL" }, new List<string>()) { FirstName = "Bob", MiddleName = "Blakely" };
                studentRepositoryMock.Setup(repo => repo.GetAsync("0000894")).ReturnsAsync(student);
                unauthorizedStudent = new Domain.Student.Entities.Student("00004002", "Jones", 802, new List<string>() { "BA.MATH" }, new List<string>());
                studentRepositoryMock.Setup(repo => repo.GetAsync("00004002")).ReturnsAsync(unauthorizedStudent);

                // Mock the TestResultRepository response

                var test1 = new TestResult("0000894", "Code1", "Test 1 Description", new DateTime(2015, 01, 01), TestType.Admissions);
                test1.Score = 10.25m;
                var subtest1 = new SubTestResult("SUB1", "Subtest1", DateTime.Now.AddDays(-10));
                subtest1.Score = 11.111m;
                test1.SubTests = new List<SubTestResult>() { subtest1 };
                var componentTest1 = new ComponentTest("COMP1", 2.222m, 15);
                test1.ComponentTests = new List<ComponentTest>() { componentTest1 };
                var test2 = new TestResult("0000894", "Code2", "Test 2 Description", new DateTime(2015, 02, 01), TestType.Admissions);
                test2.Score = null;
                var test3 = new TestResult("0000894", "Code3", "Test 3 Description", new DateTime(2015, 03, 01), TestType.Admissions);
                test3.Score = 1.333m;
                var test4 = new TestResult("0000894", "Code4", "Test 4 Description", new DateTime(2015, 04, 01), TestType.Placement);
                test4.Score = 3m;
                var test5 = new TestResult("0000894", "Code5", "Test 5 Description", new DateTime(2015, 05, 01), TestType.Other);
                test5.Score = decimal.MaxValue;
                var test6 = new TestResult("0000894", "Code6", "Test 6 Description", new DateTime(2015, 06, 01), TestType.Other);
                test5.Score = 4.55m;
                testResults = new List<Ellucian.Colleague.Domain.Student.Entities.TestResult>() { test1, test2, test3, test4, test5, test6 };

                testResultRepositoryMock.Setup(x => x.GetAsync(It.IsAny<string>())).Returns(Task.FromResult<IEnumerable<Ellucian.Colleague.Domain.Student.Entities.TestResult>>(testResults));

                // Mock Adapters
                testResultDtoAdapter = new AutoMapperAdapter<Ellucian.Colleague.Domain.Student.Entities.TestResult, Ellucian.Colleague.Dtos.Student.TestResult2>(adapterRegistry, logger);
                adapterRegistryMock.Setup(areg => areg.GetAdapter<Ellucian.Colleague.Domain.Student.Entities.TestResult, Ellucian.Colleague.Dtos.Student.TestResult2>()).Returns(testResultDtoAdapter);
                subTestResultDtoAdapter = new AutoMapperAdapter<Ellucian.Colleague.Domain.Student.Entities.SubTestResult, Ellucian.Colleague.Dtos.Student.SubTestResult2>(adapterRegistry, logger);
                adapterRegistryMock.Setup(areg => areg.GetAdapter<Ellucian.Colleague.Domain.Student.Entities.SubTestResult, Ellucian.Colleague.Dtos.Student.SubTestResult2>()).Returns(subTestResultDtoAdapter);
                componentTestDtoAdapter = new AutoMapperAdapter<Ellucian.Colleague.Domain.Student.Entities.ComponentTest, Ellucian.Colleague.Dtos.Student.ComponentTest2>(adapterRegistry, logger);
                adapterRegistryMock.Setup(areg => areg.GetAdapter<Ellucian.Colleague.Domain.Student.Entities.ComponentTest, Ellucian.Colleague.Dtos.Student.ComponentTest2>()).Returns(componentTestDtoAdapter);
                // Set up current user
                currentUserFactory = new CurrentUserSetup.StudentUserFactory();

                testResultService = new TestResultService(adapterRegistry, studentRepository, testResultRepository, currentUserFactory, roleRepo, logger, baseConfigurationRepository);
            }

            [TestCleanup]
            public void Cleanup()
            {
                testResultRepository = null;
                testResults = null;
                adapterRegistry = null;
                testResultService = null;
            }

            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public async Task ThrowsErrorIfNotSelf()
            {
                await testResultService.Get2Async("0004002", "");
            }

            [TestMethod]
            public async Task ReturnsAllTestsForStudent()
            {
                var result = await testResultService.Get2Async("0000894", "");
                Assert.AreEqual(testResults.Count(), result.Count());
            }

            [TestMethod]
            public async Task ReturnsAllTestsForStudent_Scores()
            {
                var result = await testResultService.Get2Async("0000894", "");
                foreach (var testResult in testResults)
                {
                    var newResult = result.Where(r => r.Code == testResult.Code).FirstOrDefault();
                    if (newResult != null)
                    {
                        Assert.AreEqual(testResult.Score, newResult.Score);
                    } 
                }
            }

            [TestMethod]
            public async Task ReturnsAdmissionTestsForStudent()
            {
                var result = await testResultService.Get2Async("0000894", "admissions");
                Assert.AreEqual(testResults.Count(t => t.Category == TestType.Admissions), result.Count());
            }

            [TestMethod]
            public async Task ReturnsPlacementTestsForStudent()
            {
                var result = await testResultService.Get2Async("0000894", "placement");
                Assert.AreEqual(testResults.Count(t => t.Category == TestType.Placement), result.Count());
            }

            [TestMethod]
            public async Task ReturnsOtherTestsForStudent()
            {
                var result = await testResultService.Get2Async("0000894", "other");
                Assert.AreEqual(testResults.Count(t => t.Category == TestType.Other), result.Count());
            }

            [TestMethod]
            public async Task ReturnsNoTestsIfRepositoryRetrievesNoTests()
            {
                testResultRepositoryMock.Setup(x => x.GetAsync(It.IsAny<string>())).Returns(Task.FromResult<IEnumerable<Ellucian.Colleague.Domain.Student.Entities.TestResult>>(new List<Ellucian.Colleague.Domain.Student.Entities.TestResult>()));
                var result = await testResultService.Get2Async("0000894", "");
                Assert.AreEqual(0, result.Count());
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task ThrowsExceptionIfRepositoryThrowsException()
            {
                testResultRepositoryMock.Setup(x => x.GetAsync(It.IsAny<string>())).Throws(new ArgumentNullException());
                var result = await testResultService.Get2Async("0000894", "");
                Assert.AreEqual(0, result.Count());
            }
        }

        [TestClass]
        public class Get2Async_AsStudentAdvisorUser : CurrentUserSetup
        {
            private Mock<ITestResultRepository> testResultRepositoryMock;
            private ITestResultRepository testResultRepository;
            private Mock<IStudentRepository> studentRepositoryMock;
            private IStudentRepository studentRepository;
            private Mock<IAdapterRegistry> adapterRegistryMock;
            private IAdapterRegistry adapterRegistry;
            private ILogger logger;
            private List<Ellucian.Colleague.Domain.Student.Entities.TestResult> testResults;
            private AutoMapperAdapter<Ellucian.Colleague.Domain.Student.Entities.TestResult, Ellucian.Colleague.Dtos.Student.TestResult2> testResultDtoAdapter;
            private Mock<IRoleRepository> roleRepoMock;
            private IRoleRepository roleRepo;
            private ICurrentUserFactory currentUserFactory;
            private TestResultService testResultService;
            private Domain.Student.Entities.Student student;
            private Domain.Student.Entities.Student unauthorizedStudent;
            private IConfigurationRepository baseConfigurationRepository;
            private Mock<IConfigurationRepository> baseConfigurationRepositoryMock;

            [TestInitialize]
            public void Initialize()
            {
                testResultRepositoryMock = new Mock<ITestResultRepository>();
                testResultRepository = testResultRepositoryMock.Object;
                studentRepositoryMock = new Mock<IStudentRepository>();
                studentRepository = studentRepositoryMock.Object;

                adapterRegistryMock = new Mock<IAdapterRegistry>();
                adapterRegistry = adapterRegistryMock.Object;
                roleRepoMock = new Mock<IRoleRepository>();
                roleRepo = roleRepoMock.Object;
                logger = new Mock<ILogger>().Object;
                baseConfigurationRepositoryMock = new Mock<IConfigurationRepository>();
                baseConfigurationRepository = baseConfigurationRepositoryMock.Object;

                // Mock student repo response
                student = new Domain.Student.Entities.Student("0000894", "Smith", 2, new List<string>() { "BA.ENGL" }, new List<string>()) { FirstName = "Bob", MiddleName = "Blakely" };
                studentRepositoryMock.Setup(repo => repo.GetAsync("0000894")).ReturnsAsync(student);
                unauthorizedStudent = new Domain.Student.Entities.Student("00004002", "Jones", 802, new List<string>() { "BA.MATH" }, new List<string>());
                studentRepositoryMock.Setup(repo => repo.GetAsync("00004002")).ReturnsAsync(unauthorizedStudent);

                // Mock the TestResultRepository response
                testResults = new List<Ellucian.Colleague.Domain.Student.Entities.TestResult>() { 
                    new TestResult("0000894", "Code1", "Test 1 Description", new DateTime(2015, 01, 01), TestType.Admissions), 
                    new TestResult("0000894", "Code2", "Test 2 Description", new DateTime(2015, 02, 01), TestType.Admissions),
                    new TestResult("0000894", "Code3", "Test 3 Description", new DateTime(2015, 03, 01), TestType.Admissions),
                    new TestResult("0000894", "Code4", "Test 4 Description", new DateTime(2015, 04, 01), TestType.Placement),
                    new TestResult("0000894", "Code5", "Test 5 Description", new DateTime(2015, 05, 01), TestType.Other),
                    new TestResult("0000894", "Code6", "Test 6 Description", new DateTime(2015, 06, 01), TestType.Other)};

                testResultRepositoryMock.Setup(x => x.GetAsync(It.IsAny<string>())).Returns(Task.FromResult<IEnumerable<Ellucian.Colleague.Domain.Student.Entities.TestResult>>(testResults));

                // Mock Adapters
                testResultDtoAdapter = new AutoMapperAdapter<Ellucian.Colleague.Domain.Student.Entities.TestResult, Ellucian.Colleague.Dtos.Student.TestResult2>(adapterRegistry, logger);
                adapterRegistryMock.Setup(areg => areg.GetAdapter<Ellucian.Colleague.Domain.Student.Entities.TestResult, Ellucian.Colleague.Dtos.Student.TestResult2>()).Returns(testResultDtoAdapter);

                // Set up current user
                currentUserFactory = new CurrentUserSetup.AdvisorUserFactory();

                testResultService = new TestResultService(adapterRegistry, studentRepository, testResultRepository, currentUserFactory, roleRepo, logger, baseConfigurationRepository);
            }

            [TestCleanup]
            public void Cleanup()
            {
                testResultRepository = null;
                testResults = null;
                adapterRegistry = null;
                testResultService = null;
            }

            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public async Task ThrowsErrorWhenAdvisorDoesNotHaveCorrectRole()
            {

                await testResultService.Get2Async("0000894", "");
            }

            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public async Task ReturnsAll_AdvisorHasViewStudentInformationPermission()
            {
                // Set up needed permission
                advisorRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(StudentPermissionCodes.ViewStudentInformation));
                roleRepoMock.Setup(rpm => rpm.GetRolesAsync()).ReturnsAsync(new List<Role>() { advisorRole });
                roleRepoMock.Setup(repo => repo.Roles).Returns(new List<Role>() { advisorRole });
                var result = await testResultService.Get2Async("0000894", "");
                Assert.AreEqual(testResults.Count(), result.Count());
            }

            [TestMethod]
            public async Task ReturnsAll_AdvisorHasViewAnyAdviseePermission()
            {
                // Set up needed permission
                advisorRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(PlanningPermissionCodes.ViewAnyAdvisee));
                roleRepoMock.Setup(rpm => rpm.GetRolesAsync()).ReturnsAsync(new List<Role>() { advisorRole });
                roleRepoMock.Setup(repo => repo.Roles).Returns(new List<Role>() { advisorRole });
                var result = await testResultService.Get2Async("0000894", "");
                Assert.AreEqual(testResults.Count(), result.Count());
            }


            [TestMethod]
            public async Task ReturnAll_ViewAssignedAdviseesPermission()
            {
                // Arrange
                // Set up needed permission
                advisorRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(PlanningPermissionCodes.ViewAssignedAdvisees));
                roleRepoMock.Setup(rpm => rpm.GetRolesAsync()).ReturnsAsync(new List<Role>() { advisorRole });
                // Mock student repo responses so that the student has this assigned advisor
                student = new Domain.Student.Entities.Student("0000894", "Smith", 2, new List<string>() { "BA.ENGL" }, new List<string>()) { FirstName = "Bob", MiddleName = "Blakely" };
                student.AddAdvisement("0000111", null, null, "major");
                student.AddAdvisor("0000111");
                studentRepositoryMock.Setup(repo => repo.GetAsync("0000894")).ReturnsAsync(student);
                StudentAccess studentAccess = new StudentAccess("0000894");
                studentAccess.AddAdvisement("0000111", null, null, "major");
                List<string> ids = new List<string>() { "0000894" };
                List<StudentAccess> listStudentAccess = new List<StudentAccess>() { studentAccess };
                studentRepositoryMock.Setup(repo => repo.GetStudentAccessAsync(ids)).ReturnsAsync(listStudentAccess);

                // Act
                var result = await testResultService.Get2Async("0000894", "");
                // Assert
                Assert.AreEqual(testResults.Count(), result.Count());
            }
        }
    }
}
