// Copyright 2017-2018 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Coordination.Student.Services;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Colleague.Domain.Student;
using Ellucian.Colleague.Domain.Student.Entities;
using Ellucian.Colleague.Domain.Student.Repositories;
using Ellucian.Colleague.Domain.Student.Tests;
using Ellucian.Colleague.Dtos;
using Ellucian.Colleague.Dtos.DtoProperties;
using Ellucian.Web.Adapters;
using Ellucian.Web.Http.Exceptions;
using Ellucian.Web.Security;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.Student.Tests.Services
{
    [TestClass]
    public class StudentFinancialAidApplicationServiceTests
    {
        public abstract class CurrentUserSetup
        {
            protected Domain.Entities.Role personRole = new Domain.Entities.Role(105, "Faculty");

            public class PersonUserFactory : ICurrentUserFactory
            {
                public ICurrentUser CurrentUser
                {
                    get
                    {
                        return new CurrentUser(new Claims()
                        {
                            ControlId = "123",
                            Name = "George",
                            PersonId = "0000015",
                            SecurityToken = "321",
                            SessionTimeout = 30,
                            UserName = "Faculty",
                            Roles = new List<string>() { "Faculty" },
                            SessionFixationId = "abc123",
                        });
                    }
                }
            }
        }

        /// <summary>
        /// This class tests the FinancialAidApplicationService2 class.
        /// </summary>
        [TestClass]
        public class StudentFinancialAidApplicationServiceUnitTests : CurrentUserSetup
        {

            private Domain.Entities.Permission permissionViewAnyApplication;
            private IAdapterRegistry adapterRegistry;
            private ICollection<Domain.Student.Entities.FinancialAidYear> _financialAidYears = new List<Domain.Student.Entities.FinancialAidYear>();
            private IConfigurationRepository baseConfigurationRepository;
            private ICurrentUserFactory currentUserFactory;
            private IEnumerable<Domain.Student.Entities.Fafsa> allFinancialAidApplications;
            private ILogger logger;
            private IPersonRepository personRepo;
            private IRoleRepository roleRepo;
            private IStudentFinancialAidApplicationRepository faAppRepo;
            private IStudentReferenceDataRepository refRepo;
            private Mock<IAdapterRegistry> adapterRegistryMock;
            private Mock<IConfigurationRepository> baseConfigurationRepositoryMock;
            private Mock<IPersonRepository> personRepoMock;
            private Mock<IRoleRepository> roleRepoMock;
            private Mock<IStudentFinancialAidApplicationRepository> faAppRepoMock;
            private Mock<IStudentReferenceDataRepository> refRepoMock;
            private string financialAidApplicationGuid = "31d8aa32-dbe6-4a49-a1c4-2cad39e232e4";
            private StudentFinancialAidApplicationService financialAidApplicationService;
            private Tuple<IEnumerable<Domain.Student.Entities.Fafsa>, int> _financialAidApplicationTuple;
            private List<FinancialAidMaritalStatus> finAidMarStatus = new List<FinancialAidMaritalStatus>();

            [TestInitialize]
            public void Initialize()
            {
                personRepoMock = new Mock<IPersonRepository>();
                personRepo = personRepoMock.Object;
                refRepoMock = new Mock<IStudentReferenceDataRepository>();
                refRepo = refRepoMock.Object;
                faAppRepoMock = new Mock<IStudentFinancialAidApplicationRepository>();
                faAppRepo = faAppRepoMock.Object;
                adapterRegistryMock = new Mock<IAdapterRegistry>();
                adapterRegistry = adapterRegistryMock.Object;
                roleRepoMock = new Mock<IRoleRepository>();
                roleRepo = roleRepoMock.Object;
                logger = new Mock<ILogger>().Object;

                baseConfigurationRepositoryMock = new Mock<IConfigurationRepository>();
                baseConfigurationRepository = baseConfigurationRepositoryMock.Object;

                allFinancialAidApplications = new TestFinancialAidApplicationRepository().GetFinancialAidApplications();

                _financialAidApplicationTuple = new Tuple<IEnumerable<Domain.Student.Entities.Fafsa>, int>(allFinancialAidApplications, allFinancialAidApplications.Count());

                _financialAidYears.Add(new Domain.Student.Entities.FinancialAidYear("9C3B805D-CFE6-483B-86C3-4C20562F8C15", "2013", "CODE1", "STATUS1") { HostCountry = "USA" });
                _financialAidYears.Add(new Domain.Student.Entities.FinancialAidYear("73244057-D1EC-4094-A0B7-DE602533E3A6", "2014", "CODE2", "STATUS2") { HostCountry = "CAN", status = "D" });
                _financialAidYears.Add(new Domain.Student.Entities.FinancialAidYear("1df164eb-8178-4321-a9f7-24f12d3991d8", "2015", "CODE3", "STATUS3") { HostCountry = "USA" });
                _financialAidYears.Add(new Domain.Student.Entities.FinancialAidYear("1df164eb-8178-4321-a9f7-24f12d3991d9", "2016", "CODE3", "STATUS3") { HostCountry = "USA" });

                refRepoMock.Setup(repo => repo.GetFinancialAidYearsAsync(It.IsAny<bool>())).ReturnsAsync(_financialAidYears);

                finAidMarStatus = new List<FinancialAidMaritalStatus>()
                {
                    new FinancialAidMaritalStatus("1", "Married"),
                    new FinancialAidMaritalStatus("2", "Single"),
                    new FinancialAidMaritalStatus("3", "Divorced"),
                    new FinancialAidMaritalStatus("4", "Separated"),
                    new FinancialAidMaritalStatus("5", "Married/Remarried"),
                    new FinancialAidMaritalStatus("6", "Divorced/Widowed")

                };
                refRepoMock.Setup( repo => repo.GetFinancialAidMaritalStatusesAsync( It.IsAny<bool>(), It.IsAny<string>() ) ).ReturnsAsync( finAidMarStatus );

                refRepoMock.Setup( repo => repo.GetFinancialAidMaritalStatusAsync( It.IsAny<string>(), "1" ) ).ReturnsAsync( finAidMarStatus[0] );
                refRepoMock.Setup( repo => repo.GetFinancialAidMaritalStatusAsync( It.IsAny<string>(), "2" ) ).ReturnsAsync( finAidMarStatus[1] );
                refRepoMock.Setup( repo => repo.GetFinancialAidMaritalStatusAsync( It.IsAny<string>(), "3" ) ).ReturnsAsync( finAidMarStatus[2] );
                refRepoMock.Setup( repo => repo.GetFinancialAidMaritalStatusAsync( It.IsAny<string>(), "4" ) ).ReturnsAsync( finAidMarStatus[3] );
                refRepoMock.Setup( repo => repo.GetFinancialAidMaritalStatusAsync( It.IsAny<string>(), "5" ) ).ReturnsAsync( finAidMarStatus[4] ); 
                refRepoMock.Setup( repo => repo.GetFinancialAidMaritalStatusAsync( It.IsAny<string>(), "6" ) ).ReturnsAsync( finAidMarStatus[5] );




                // Set up current user
                //currentUserFactory = new CurrentUserSetup.PersonUserFactory();
                //currentUserFactory = new CurrentUserSetup.StudentUserFactory();

                // Set up current user
                //currentUserFactory = userFactoryMock.Object;
                currentUserFactory = new CurrentUserSetup.PersonUserFactory();

                // Mock permissions
                permissionViewAnyApplication = new Ellucian.Colleague.Domain.Entities.Permission(StudentPermissionCodes.ViewFinancialAidApplications);
                personRole.AddPermission(permissionViewAnyApplication);

                roleRepoMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { personRole });

                financialAidApplicationService = new StudentFinancialAidApplicationService(faAppRepo, personRepo, refRepo, baseConfigurationRepository, adapterRegistry, currentUserFactory, roleRepo, logger);

                Dictionary<string, string> studentDictionary = new Dictionary<string, string>();
                studentDictionary.Add("Student1", "9ae3a175-1dfd-4937-b97b-3c9ad596e023");
                studentDictionary.Add("Student2", "31d8aa32-dbe6-4a49-a1c4-2cad39e232e4");
                studentDictionary.Add("Student3", "b769e6a9-da86-47a9-ab21-b17198880439");
                studentDictionary.Add( "Student4", "e297656e-8d50-4c63-a2dd-0fcfc46647c4");
                studentDictionary.Add( "Student5",  "8d0e291e-7246-4067-aff1-47ff6adc0392");
                studentDictionary.Add( "Student6",  "b91bbee8-88d1-4063-86e2-e7cb1865b45a");
                studentDictionary.Add( "Student7", "4eaca2e7-fb59-44b6-be64-ce9e2ad73e81"); 
                studentDictionary.Add(  "Student8",  "c76a6755-7594-4a24-a821-be2c8293ff78"); 
                studentDictionary.Add( "Student9", "95860685-7a99-476b-99f0-34066a5c20f6"); 
                studentDictionary.Add(  "Student10",  "119cdf92-18b4-44f0-9fcb-6b3dd9702f67"); 
                studentDictionary.Add(  "Student11",  "b772f098-77f3-48ef-b691-ea5b8aff5646"); 
                studentDictionary.Add(  "Student12", "e692812d-a23f-4601-a112-dc2d58389045");
                studentDictionary.Add( "Student13",  "9ae3a175-1dfd-4937-b97b-3c9ad596e023"); 
                studentDictionary.Add(  "Student14",  "13660156-d481-4b3d-b617-92136979314c"); 
                studentDictionary.Add( "Student15",  "bcea6b4e-01ff-4d52-b4d5-7f6a5aa10820");
                studentDictionary.Add(  "Student16",  "2198dcfa-cd4b-4df3-ab17-73b63ad595ee");
                studentDictionary.Add( "Student17",  "c37a2fde-4bac-4c84-b530-6b6f7d1f490a"); 
                studentDictionary.Add( "Student18",  "400dce82-2cdc-4990-a864-fc9943084d1a");

                personRepoMock.Setup(x => x.GetPersonGuidsCollectionAsync(It.IsAny<List<string>>())).ReturnsAsync(studentDictionary);
                personRepoMock.Setup(x => x.GetPersonIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync(studentDictionary.FirstOrDefault().Key);
            }

            [TestCleanup]
            public void Cleanup()
            {
                refRepo = null;
                faAppRepo = null;
                personRepo = null;
                allFinancialAidApplications = null;
                adapterRegistry = null;
                roleRepo = null;
                logger = null;
                financialAidApplicationService = null;
            }

            [TestMethod]
            public async Task StudentFinancialAidApplicationService_GetFinancialAidApplicationByIdAsync()
            {
                Domain.Student.Entities.Fafsa thisFinancialAidApplication = allFinancialAidApplications.Where(m => m.Guid == financialAidApplicationGuid).FirstOrDefault();
                faAppRepoMock.Setup(repo => repo.GetByIdAsync(It.IsAny<string>())).ReturnsAsync(_financialAidApplicationTuple.Item1.Where(m => m.Guid == financialAidApplicationGuid).FirstOrDefault());
                Dtos.FinancialAidApplication financialAidApplication = await financialAidApplicationService.GetByIdAsync(financialAidApplicationGuid);
                Assert.AreEqual(thisFinancialAidApplication.Guid, financialAidApplication.Id);
            }


            [TestMethod]
            public async Task StudentFinancialAidApplicationService_GetFinancialAidApplicationsAsync_Count_Cache()
            {
                faAppRepoMock.Setup(repo => repo.GetAsync(It.IsAny<int>(), It.IsAny<int>(), false, It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<string>(), It.IsAny<string>() ) )
                    .ReturnsAsync(_financialAidApplicationTuple);



                Tuple<IEnumerable<Ellucian.Colleague.Dtos.FinancialAidApplication>, int> financialAidApplication = await financialAidApplicationService.GetAsync(0, 100, It.IsAny<Dtos.FinancialAidApplication>(), false);
                Assert.AreEqual(allFinancialAidApplications.Count(), financialAidApplication.Item2);
            }


            [TestMethod]
            public async Task StudentFinancialAidApplicationService_GetFinancialAidApplicationsAsync_NoRecords()
            {
                var emptyTuple = new Tuple<IEnumerable<Domain.Student.Entities.Fafsa>, int>(new List<Domain.Student.Entities.Fafsa>(), 0);
                faAppRepoMock.Setup(repo => repo.GetAsync(It.IsAny<int>(), It.IsAny<int>(), false, It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<string>(), It.IsAny<string>() ) ).ReturnsAsync(emptyTuple);

                Tuple<IEnumerable<Ellucian.Colleague.Dtos.FinancialAidApplication>, int> financialAidApplication = await financialAidApplicationService.GetAsync(0, 100, It.IsAny<Dtos.FinancialAidApplication>(), false);
                Assert.AreEqual(emptyTuple.Item1.Count(), 0);
            }

            [TestMethod]
            public async Task StudentFinancialAidApplicationService_GetFinancialAidApplicationsAsync_Null_Tuple()
            {
                var emptyTuple = new Tuple<IEnumerable<Domain.Student.Entities.Fafsa>, int>(new List<Domain.Student.Entities.Fafsa>(), 0);
                faAppRepoMock.Setup(repo => repo.GetAsync(It.IsAny<int>(), It.IsAny<int>(), false, It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<string>(), It.IsAny<string>() ) ).ReturnsAsync(() => null);

                Tuple<IEnumerable<Ellucian.Colleague.Dtos.FinancialAidApplication>, int> financialAidApplication = await financialAidApplicationService.GetAsync(0, 100, It.IsAny<Dtos.FinancialAidApplication>(), false);
                Assert.AreEqual(emptyTuple.Item1.Count(), 0);
            }

            [TestMethod]
            public async Task StudentFinancialAidApplicationService_GetFinancialAidApplicationsAsync_Cache()
            {
                faAppRepoMock.Setup(repo => repo.GetAsync(It.IsAny<int>(), It.IsAny<int>(), false, It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<string>(), It.IsAny<string>() ) ).ReturnsAsync(_financialAidApplicationTuple);

                Tuple<IEnumerable<Dtos.FinancialAidApplication>, int> financialAidApplications = await financialAidApplicationService.GetAsync(0, 100, It.IsAny<Dtos.FinancialAidApplication>(), false);
                Assert.AreEqual(allFinancialAidApplications.ElementAt(0).Guid, financialAidApplications.Item1.ElementAt(0).Id);
            }

            [TestMethod]
            public async Task StudentFinancialAidApplicationService_GetFinancialAidApplicationsAsync_StudentId_Cache()
            {
                faAppRepoMock.Setup(repo => repo.GetAsync(It.IsAny<int>(), It.IsAny<int>(), false, It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<string>(), It.IsAny<string>() ) ).ReturnsAsync(_financialAidApplicationTuple);
                var faFilter = new Dtos.FinancialAidApplication();
                faFilter.Applicant =  new Dtos.DtoProperties.FinancialAidApplicationApplicant() { Person = new GuidObject2() { Id = "9ae3a175-1dfd-4937-b97b-3c9ad596e023" } };
                Tuple<IEnumerable<Dtos.FinancialAidApplication>, int> financialAidApplications = await financialAidApplicationService.GetAsync(0, 100, faFilter, false);
                Assert.AreEqual(allFinancialAidApplications.ElementAt(0).Guid, financialAidApplications.Item1.ElementAt(0).Id);
            }

            [TestMethod]
            public async Task StudentFinancialAidApplicationService_GetFinancialAidApplicationsAsync_Invalid_StudentId_Cache()
            {
                personRepoMock.Setup(x => x.GetPersonIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync(string.Empty);
                var emptyTuple = new Tuple<IEnumerable<Domain.Student.Entities.Fafsa>, int>(new List<Domain.Student.Entities.Fafsa>(), 0);
                faAppRepoMock.Setup(repo => repo.GetAsync(It.IsAny<int>(), It.IsAny<int>(), false, It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<string>(), It.IsAny<string>() ) ).ReturnsAsync(_financialAidApplicationTuple);
                var faFilter = new Dtos.FinancialAidApplication();
                faFilter.Applicant = new Dtos.DtoProperties.FinancialAidApplicationApplicant() { Person = new GuidObject2() { Id = "abc" } };
                Tuple<IEnumerable<Dtos.FinancialAidApplication>, int> financialAidApplications = await financialAidApplicationService.GetAsync(0, 100, faFilter, false);
                Assert.AreEqual(emptyTuple.Item1.Count(), 0);
            }

            [TestMethod]
            public async Task StudentFinancialAidApplicationService_GetFinancialAidApplicationsAsync_Invalid_StudentId_Exp_Cache()
            {
                personRepoMock.Setup(x => x.GetPersonIdFromGuidAsync(It.IsAny<string>())).Throws<Exception>();
                var emptyTuple = new Tuple<IEnumerable<Domain.Student.Entities.Fafsa>, int>(new List<Domain.Student.Entities.Fafsa>(), 0);
                faAppRepoMock.Setup(repo => repo.GetAsync(It.IsAny<int>(), It.IsAny<int>(), false, It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<string>(), It.IsAny<string>() ) ).ReturnsAsync(_financialAidApplicationTuple);
                var faFilter = new Dtos.FinancialAidApplication();
                faFilter.Applicant = new Dtos.DtoProperties.FinancialAidApplicationApplicant() { Person = new GuidObject2() { Id = "abc" } };
                Tuple<IEnumerable<Dtos.FinancialAidApplication>, int> financialAidApplications = await financialAidApplicationService.GetAsync(0, 100, faFilter, false);
                Assert.AreEqual(emptyTuple.Item1.Count(), 0);
            }

            [TestMethod]
            public async Task StudentFinancialAidApplicationService_GetFinancialAidApplicationsAsync_AidYear_Cache()
            {
                faAppRepoMock.Setup(repo => repo.GetAsync(It.IsAny<int>(), It.IsAny<int>(), false, It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<string>(), It.IsAny<string>() ) ).ReturnsAsync(_financialAidApplicationTuple);
                var faFilter = new Dtos.FinancialAidApplication();
                faFilter.AidYear = new GuidObject2() { Id = "9C3B805D-CFE6-483B-86C3-4C20562F8C15".ToLower() } ;
                Tuple<IEnumerable<Dtos.FinancialAidApplication>, int> financialAidApplications = await financialAidApplicationService.GetAsync(0, 100, faFilter, false);
                Assert.AreEqual(allFinancialAidApplications.ElementAt(0).Guid, financialAidApplications.Item1.ElementAt(0).Id);
            }

            [TestMethod]
            public async Task StudentFinancialAidApplicationService_GetFinancialAidApplicationsAsync_Invalid_AidYear_Cache()
            {
                var emptyTuple = new Tuple<IEnumerable<Domain.Student.Entities.Fafsa>, int>(new List<Domain.Student.Entities.Fafsa>(), 0);
                faAppRepoMock.Setup(repo => repo.GetAsync(It.IsAny<int>(), It.IsAny<int>(), false, It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<string>(), It.IsAny<string>() ) ).ReturnsAsync(_financialAidApplicationTuple);
                var faFilter = new Dtos.FinancialAidApplication();
                faFilter.AidYear = new GuidObject2() { Id = "abc".ToLower() };
                Tuple<IEnumerable<Dtos.FinancialAidApplication>, int> financialAidApplications = await financialAidApplicationService.GetAsync(0, 100, faFilter, false);
                Assert.AreEqual(emptyTuple.Item1.Count(), 0);
            }

            [TestMethod]
            public async Task StudentFinancialAidApplicationService_GetFinancialAidApplicationsAsync_Count_NonCache()
            {
                faAppRepoMock.Setup(repo => repo.GetAsync(It.IsAny<int>(), It.IsAny<int>(), true, It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<string>(), It.IsAny<string>() ) ).ReturnsAsync(_financialAidApplicationTuple);
                Tuple<IEnumerable<Ellucian.Colleague.Dtos.FinancialAidApplication>, int> financialAidApplication = await financialAidApplicationService.GetAsync(0, 100, It.IsAny<Dtos.FinancialAidApplication>(), true);
                Assert.AreEqual(allFinancialAidApplications.Count(), financialAidApplication.Item2);
            }

            [TestMethod]
            public async Task StudentFinancialAidApplicationService_GetFinancialAidApplicationsAsync_NonCache()
            {
                faAppRepoMock.Setup(repo => repo.GetAsync(It.IsAny<int>(), It.IsAny<int>(), true, It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<string>(), It.IsAny<string>() ) ).ReturnsAsync(_financialAidApplicationTuple);

                Tuple<IEnumerable<Ellucian.Colleague.Dtos.FinancialAidApplication>, int> financialAidApplications = await financialAidApplicationService.GetAsync(0, 100, It.IsAny<Dtos.FinancialAidApplication>(), true);
                Assert.AreEqual(allFinancialAidApplications.ElementAt(0).Guid, financialAidApplications.Item1.ElementAt(0).Id);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task StudentFinancialAidApplicationService_GetFinancialAidApplicationsAsync_NoId_ArgumentNullException()
            {
                var fafsa = new Domain.Student.Entities.Fafsa("123", "2004", string.Empty);
                _financialAidApplicationTuple = new Tuple<IEnumerable<Domain.Student.Entities.Fafsa>, int>(new List<Domain.Student.Entities.Fafsa> {fafsa}, 1);
                faAppRepoMock.Setup(repo => repo.GetAsync(It.IsAny<int>(), It.IsAny<int>(), true, It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<string>(), It.IsAny<string>() ) ).ReturnsAsync(_financialAidApplicationTuple);
                Tuple<IEnumerable<Ellucian.Colleague.Dtos.FinancialAidApplication>, int> financialAidApplications = await financialAidApplicationService.GetAsync(0, 100, It.IsAny<Dtos.FinancialAidApplication>(), true);
            }

            [TestMethod]
            [ExpectedException(typeof( IntegrationApiException ) )]
            public async Task StudentFinancialAidApplicationService_GetFinancialAidApplicationsAsync_BadGuid_ArgumentNullException()
            {
                var fafsa = new Domain.Student.Entities.Fafsa("123", "2004", "123");
                _financialAidApplicationTuple = new Tuple<IEnumerable<Domain.Student.Entities.Fafsa>, int>(new List<Domain.Student.Entities.Fafsa> { fafsa }, 1);
                faAppRepoMock.Setup(repo => repo.GetAsync(It.IsAny<int>(), It.IsAny<int>(), true, It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<string>(), It.IsAny<string>() ) ).ReturnsAsync(_financialAidApplicationTuple);
                Tuple<IEnumerable<Ellucian.Colleague.Dtos.FinancialAidApplication>, int> financialAidApplications = await financialAidApplicationService.GetAsync(0, 100, It.IsAny<Dtos.FinancialAidApplication>(), true);
            }

            [TestMethod]
            [ExpectedException(typeof( IntegrationApiException ) )]
            public async Task StudentFinancialAidApplicationService_GetFinancialAidApplicationsAsync_NoPersonGuid_ArgumentNullException()
            {
                personRepoMock.Setup(x => x.GetPersonGuidsCollectionAsync(It.IsAny<List<string>>())).ReturnsAsync(() => null);
                faAppRepoMock.Setup(repo => repo.GetAsync(It.IsAny<int>(), It.IsAny<int>(), true, It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<string>(), It.IsAny<string>() ) ).ReturnsAsync(_financialAidApplicationTuple);
                Tuple<IEnumerable<Ellucian.Colleague.Dtos.FinancialAidApplication>, int> financialAidApplications = await financialAidApplicationService.GetAsync(0, 100, It.IsAny<Dtos.FinancialAidApplication>(), true);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task StudentFinancialAidApplicationService_GetFinancialAidApplicationsAsync_NoAidYear_ArgumentNullException()
            {
                var fafsa = new Domain.Student.Entities.Fafsa("123",string.Empty, "1234");
                _financialAidApplicationTuple = new Tuple<IEnumerable<Domain.Student.Entities.Fafsa>, int>(new List<Domain.Student.Entities.Fafsa> { fafsa }, 1);
                faAppRepoMock.Setup(repo => repo.GetAsync(It.IsAny<int>(), It.IsAny<int>(), true, It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<string>(), It.IsAny<string>() ) ).ReturnsAsync(_financialAidApplicationTuple);
                Tuple<IEnumerable<Ellucian.Colleague.Dtos.FinancialAidApplication>, int> financialAidApplications = await financialAidApplicationService.GetAsync(0, 100, It.IsAny<Dtos.FinancialAidApplication>(), true);
            }

            [TestMethod]
            [ExpectedException(typeof( IntegrationApiException ) )]
            public async Task StudentFinancialAidApplicationService_GetFinancialAidApplicationsAsync_BadAidYear_ArgumentNullException()
            {
                var fafsa = new Domain.Student.Entities.Fafsa("123", "123", "1234");
                _financialAidApplicationTuple = new Tuple<IEnumerable<Domain.Student.Entities.Fafsa>, int>(new List<Domain.Student.Entities.Fafsa> { fafsa }, 1);
                faAppRepoMock.Setup(repo => repo.GetAsync(It.IsAny<int>(), It.IsAny<int>(), true, It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<string>(), It.IsAny<string>() ) ).ReturnsAsync(_financialAidApplicationTuple);
                Tuple<IEnumerable<Ellucian.Colleague.Dtos.FinancialAidApplication>, int> financialAidApplications = await financialAidApplicationService.GetAsync(0, 100, It.IsAny<Dtos.FinancialAidApplication>(), true);
            }

            [TestMethod]
            [ExpectedException( typeof(ColleagueWebApiException) )]
            public async Task StudentFinancialAidApplicationService_GetFinancialAidApplicationsAsync_ArgumentException()
            {
                faAppRepoMock.Setup( repo => repo.GetAsync( It.IsAny<int>(), It.IsAny<int>(), true, It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<string>(), It.IsAny<string>() ) ).Throws<ArgumentException>();
                Tuple<IEnumerable<Ellucian.Colleague.Dtos.FinancialAidApplication>, int> financialAidApplications = await financialAidApplicationService.GetAsync( 0, 100, It.IsAny<Dtos.FinancialAidApplication>(), true );

            }

            [TestMethod]
            [ExpectedException( typeof(ColleagueWebApiException) )]
            public async Task StudentFinancialAidApplicationService_GetFinancialAidApplicationsAsync_KeyNotFoundException()
            {
                faAppRepoMock.Setup( repo => repo.GetAsync( It.IsAny<int>(), It.IsAny<int>(), true, It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<string>(), It.IsAny<string>() ) ).Throws<KeyNotFoundException>();
                Tuple<IEnumerable<Ellucian.Colleague.Dtos.FinancialAidApplication>, int> financialAidApplications = await financialAidApplicationService.GetAsync( 0, 100, It.IsAny<Dtos.FinancialAidApplication>(), true );
            }

            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public async Task StudentFinancialAidApplicationService_GetFinancialAidApplicationsAsync_PermissionsException()
            {
                faAppRepoMock.Setup(repo => repo.GetAsync(It.IsAny<int>(), It.IsAny<int>(), true, It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<string>(), It.IsAny<string>() ) ).Throws<PermissionsException>();
                Tuple<IEnumerable<Ellucian.Colleague.Dtos.FinancialAidApplication>, int> financialAidApplications = await financialAidApplicationService.GetAsync(0, 100, It.IsAny<Dtos.FinancialAidApplication>(), true);
            }

            [TestMethod]
            [ExpectedException(typeof(ColleagueWebApiException) )]
            public async Task StudentFinancialAidApplicationService_GetFinancialAidApplicationsAsync_InvalidOperationException()
            {
                faAppRepoMock.Setup(repo => repo.GetAsync(It.IsAny<int>(), It.IsAny<int>(), true, It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<string>(), It.IsAny<string>() ) ).Throws<InvalidOperationException>();
                Tuple<IEnumerable<Ellucian.Colleague.Dtos.FinancialAidApplication>, int> financialAidApplications = await financialAidApplicationService.GetAsync(0, 100, It.IsAny<Dtos.FinancialAidApplication>(), true);
            }

            [TestMethod]
            [ExpectedException(typeof(ColleagueWebApiException))]
            public async Task StudentFinancialAidApplicationService_GetFinancialAidApplicationsAsync_Exception()
            {
                faAppRepoMock.Setup(repo => repo.GetAsync(It.IsAny<int>(), It.IsAny<int>(), true, It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<string>(), It.IsAny<string>() ) ).Throws<Exception>();
                Tuple<IEnumerable<Ellucian.Colleague.Dtos.FinancialAidApplication>, int> financialAidApplications = await financialAidApplicationService.GetAsync(0, 100, It.IsAny<Dtos.FinancialAidApplication>(), true);
            }

            [TestMethod]
            [ExpectedException(typeof( IntegrationApiException ) )]
            public async Task StudentFinancialAidApplicationService_GetFinancialAidApplicationByIdAsync_ThrowsInvOpExc()
            {
                faAppRepoMock.Setup(repo => repo.GetAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<string>(), It.IsAny<string>() ) ).Throws<KeyNotFoundException>();
                await financialAidApplicationService.GetByIdAsync("dshjfkj");
            }
        }
    }
}