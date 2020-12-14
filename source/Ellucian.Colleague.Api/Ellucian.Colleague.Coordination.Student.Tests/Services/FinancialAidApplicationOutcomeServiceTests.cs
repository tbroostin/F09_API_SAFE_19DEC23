//Copyright 2017-2020 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Coordination.Student.Services;
using Ellucian.Colleague.Coordination.Student.Tests.UserFactories;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.Entities;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Colleague.Domain.Student;
using Ellucian.Colleague.Domain.Student.Entities;
using Ellucian.Colleague.Domain.Student.Repositories;
using Ellucian.Colleague.Domain.Student.Tests;
using Ellucian.Colleague.Dtos;
using Ellucian.Web.Adapters;
using Ellucian.Web.Security;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ellucian.Web.Http.Exceptions;

namespace Ellucian.Colleague.Coordination.Student.Tests.Services
{
    [TestClass]
    public class FinancialAidApplicationOutcomeServiceTests : StudentUserFactory
    {
        private const string financialAidApplicationOutcomeGuid = "7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc";
        private const string financialAidApplicationOutcomeStudent = "0003914";
        protected Domain.Entities.Role counselorRole = new Domain.Entities.Role(26, "FINANCIAL AID COUNSELOR");

        private IEnumerable<Fafsa> _financialAidApplicationOutcomeCollection;
        private FinancialAidApplicationOutcomeService _financialAidApplicationOutcomeService;
        private Mock<IFinancialAidApplicationOutcomeRepository> _financialAidApplicationOutcomeRepositoryMock;
        private Mock<IPersonRepository> _personRepositoryMock;
        private Mock<IStudentReferenceDataRepository> _referenceRepositoryMock;
        private Mock<ILogger> _loggerMock;
        private Mock<IAdapterRegistry> _adapterRegistryMock;
        private Mock<IRoleRepository> _roleRepositoryMock;
        private ICurrentUserFactory _currentUserFactory;
        
        public TestFafsaRepository expectedFafsaRepository;
        private IConfigurationRepository baseConfigurationRepository;
        private Mock<IConfigurationRepository> baseConfigurationRepositoryMock;

        private ICollection<Domain.Student.Entities.FinancialAidYear> _financialAidYears = new List<Domain.Student.Entities.FinancialAidYear>();

        [TestInitialize]
        public void Initialize()
        {
            baseConfigurationRepositoryMock = new Mock<IConfigurationRepository>();
            baseConfigurationRepository = baseConfigurationRepositoryMock.Object;
            _roleRepositoryMock = new Mock<IRoleRepository>();
            _currentUserFactory = new StudentUserFactory.CounselorUserFactory();
            counselorRole.AddPermission(new Permission(StudentPermissionCodes.ViewFinancialAidApplicationOutcomes));
            _roleRepositoryMock.Setup(r => r.Roles).Returns(new List<Domain.Entities.Role>() { counselorRole });

            _financialAidApplicationOutcomeRepositoryMock = new Mock<IFinancialAidApplicationOutcomeRepository>();
            _personRepositoryMock = new Mock<IPersonRepository>();
            _referenceRepositoryMock = new Mock<IStudentReferenceDataRepository>();
            _loggerMock = new Mock<ILogger>();
            _adapterRegistryMock = new Mock<IAdapterRegistry>();

            expectedFafsaRepository = new TestFafsaRepository();

            _financialAidApplicationOutcomeCollection = expectedFafsaRepository.GetFafsasAsync(new List<string>() { "0003914" }, new List<string>() { "2013" }).Result;

            _financialAidApplicationOutcomeRepositoryMock.Setup(repo => repo.GetAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>()))
                .ReturnsAsync(new Tuple<IEnumerable<Fafsa>, int>(_financialAidApplicationOutcomeCollection, 3));

            _financialAidApplicationOutcomeService = new FinancialAidApplicationOutcomeService(_financialAidApplicationOutcomeRepositoryMock.Object,
                _personRepositoryMock.Object, _referenceRepositoryMock.Object, baseConfigurationRepository,
                _adapterRegistryMock.Object, _currentUserFactory, _roleRepositoryMock.Object, _loggerMock.Object);

            Dictionary<string, string> studentDictionary = new Dictionary<string, string>();
            studentDictionary.Add("Student1", "9ae3a175-1dfd-4937-b97b-3c9ad596e023");
            studentDictionary.Add("Student2", "31d8aa32-dbe6-4a49-a1c4-2cad39e232e4");
            studentDictionary.Add("Student3", "b769e6a9-da86-47a9-ab21-b17198880439");
            studentDictionary.Add("Student4", "e297656e-8d50-4c63-a2dd-0fcfc46647c4");
            studentDictionary.Add("Student5", "8d0e291e-7246-4067-aff1-47ff6adc0392");
            studentDictionary.Add("Student6", "b91bbee8-88d1-4063-86e2-e7cb1865b45a");
            studentDictionary.Add("Student7", "4eaca2e7-fb59-44b6-be64-ce9e2ad73e81");
            studentDictionary.Add("Student8", "c76a6755-7594-4a24-a821-be2c8293ff78");
            studentDictionary.Add("Student9", "95860685-7a99-476b-99f0-34066a5c20f6");
            studentDictionary.Add("Student10", "119cdf92-18b4-44f0-9fcb-6b3dd9702f67");
            studentDictionary.Add("Student11", "b772f098-77f3-48ef-b691-ea5b8aff5646");
            studentDictionary.Add("Student12", "e692812d-a23f-4601-a112-dc2d58389045");
            studentDictionary.Add("Student13", "9ae3a175-1dfd-4937-b97b-3c9ad596e023");
            studentDictionary.Add("Student14", "13660156-d481-4b3d-b617-92136979314c");
            studentDictionary.Add("Student15", "bcea6b4e-01ff-4d52-b4d5-7f6a5aa10820");
            studentDictionary.Add("Student16", "2198dcfa-cd4b-4df3-ab17-73b63ad595ee");
            studentDictionary.Add("Student17", "c37a2fde-4bac-4c84-b530-6b6f7d1f490a");
            studentDictionary.Add("Student18", "400dce82-2cdc-4990-a864-fc9943084d1a");
            _personRepositoryMock.Setup(x => x.GetPersonGuidsCollectionAsync(It.IsAny<List<string>>())).ReturnsAsync(studentDictionary);
            _personRepositoryMock.Setup(x => x.GetPersonIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync(studentDictionary.FirstOrDefault().Key);

            _financialAidYears.Add(new Domain.Student.Entities.FinancialAidYear("9C3B805D-CFE6-483B-86C3-4C20562F8C15", "2013", "CODE1", "STATUS1") { HostCountry = "USA" });
            _financialAidYears.Add(new Domain.Student.Entities.FinancialAidYear("73244057-D1EC-4094-A0B7-DE602533E3A6", "2014", "CODE2", "STATUS2") { HostCountry = "CAN", status = "D" });
            _financialAidYears.Add(new Domain.Student.Entities.FinancialAidYear("1df164eb-8178-4321-a9f7-24f12d3991d8", "2015", "CODE3", "STATUS3") { HostCountry = "USA" });
            _financialAidYears.Add(new Domain.Student.Entities.FinancialAidYear("1df164eb-8178-4321-a9f7-24f12d3991d9", "2016", "CODE3", "STATUS3") { HostCountry = "USA" });

            _referenceRepositoryMock.Setup(repo => repo.GetFinancialAidYearsAsync(It.IsAny<bool>())).ReturnsAsync(_financialAidYears);
        }

        [TestCleanup]
        public void Cleanup()
        {
            _financialAidApplicationOutcomeService = null;
            _financialAidApplicationOutcomeCollection = null;
            _personRepositoryMock = null;
            _referenceRepositoryMock = null;
            _adapterRegistryMock = null;
            _currentUserFactory = null;
            _roleRepositoryMock = null;
            _loggerMock = null;
        }

        [TestMethod]
        public async Task FinancialAidApplicationOutcomeService_GetFinancialAidApplicationOutcomeAsync()
        {
            var resultsTuple = await _financialAidApplicationOutcomeService.GetAsync(0, 3, new FinancialAidApplicationOutcome(), true);
            var results = resultsTuple.Item1;
            Assert.IsTrue(results is IEnumerable<FinancialAidApplicationOutcome>);
            Assert.IsNotNull(results);
        }

        [TestMethod]
        public async Task FinancialAidApplicationOutcomeService_GetFinancialAidApplicationOutcomeAsync_Count()
        {
            var resultsTuple = await _financialAidApplicationOutcomeService.GetAsync(0, 3, new FinancialAidApplicationOutcome(), true);
            var resultCount = resultsTuple.Item2;
            Assert.AreEqual(resultCount, 3);
        }

        [TestMethod]
        public async Task FinancialAidApplicationOutcomeService_GetFinancialAidApplicationOutcomeAsync_Properties()
        {
            var result =
                (await _financialAidApplicationOutcomeService.GetAsync(0, 3, new FinancialAidApplicationOutcome() , true)).Item1.FirstOrDefault(x => x.Id == financialAidApplicationOutcomeGuid);
            Assert.IsNotNull(result.Id);
           
        }

        [TestMethod]
        public async Task FinancialAidApplicationOutcomeService_GetFinancialAidApplicationOutcomeAsync_Expected()
        {
            var expectedResults = _financialAidApplicationOutcomeCollection.FirstOrDefault(c => c.Guid == financialAidApplicationOutcomeGuid);
            var actualResult =
                (await _financialAidApplicationOutcomeService.GetAsync(0, 3, new FinancialAidApplicationOutcome(), true)).Item1.FirstOrDefault(x => x.Id == financialAidApplicationOutcomeGuid);
            Assert.AreEqual(expectedResults.Guid, actualResult.Id);
            
        }
        
        [TestMethod]
        public async Task StudentFinancialAidApplicationOutcomeService_GetFinancialAidApplicationOutcomesAsync_StudentId_Cache()
        {
            _financialAidApplicationOutcomeRepositoryMock.Setup(repo => repo.GetAsync(It.IsAny<int>(), It.IsAny<int>(), false, It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>())).ReturnsAsync(new Tuple<IEnumerable<Fafsa>, int>(_financialAidApplicationOutcomeCollection, 3));
            
            var faFilter = new Dtos.FinancialAidApplicationOutcome();
            faFilter.Applicant = new Dtos.DtoProperties.FinancialAidApplicationApplicant() { Person = new GuidObject2() { Id = "9ae3a175-1dfd-4937-b97b-3c9ad596e023" } };
            Tuple<IEnumerable<Dtos.FinancialAidApplicationOutcome>, int> financialAidApplicationOutcomes = await _financialAidApplicationOutcomeService.GetAsync(0, 100, faFilter, false);
            Assert.AreEqual(_financialAidApplicationOutcomeCollection.ElementAt(0).Guid, financialAidApplicationOutcomes.Item1.ElementAt(0).Id);
        }

        [TestMethod]
        public async Task StudentFinancialAidApplicationService_GetFinancialAidApplicationsAsync_Invalid_StudentId_Cache()
        {
            _personRepositoryMock.Setup(x => x.GetPersonIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync(string.Empty);
            var emptyTuple = new Tuple<IEnumerable<Domain.Student.Entities.Fafsa>, int>(new List<Domain.Student.Entities.Fafsa>(), 0);
            _financialAidApplicationOutcomeRepositoryMock.Setup(repo => repo.GetAsync(It.IsAny<int>(), It.IsAny<int>(), false, It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>())).ReturnsAsync(new Tuple<IEnumerable<Fafsa>, int>(_financialAidApplicationOutcomeCollection, 3));
            var faFilter = new Dtos.FinancialAidApplicationOutcome();
            faFilter.Applicant = new Dtos.DtoProperties.FinancialAidApplicationApplicant() { Person = new GuidObject2() { Id = "abc" } };
            Tuple<IEnumerable<Dtos.FinancialAidApplicationOutcome>, int> financialAidApplicationOutcomes = await _financialAidApplicationOutcomeService.GetAsync(0, 100, faFilter, false);
            Assert.AreEqual(emptyTuple.Item1.Count(), 0);
        }

        [TestMethod]
        public async Task StudentFinancialAidApplicationService_GetFinancialAidApplicationsAsync_Invalid_StudentId_Exp_Cache()
        {
            _personRepositoryMock.Setup(x => x.GetPersonIdFromGuidAsync(It.IsAny<string>())).Throws<Exception>();
            var emptyTuple = new Tuple<IEnumerable<Domain.Student.Entities.Fafsa>, int>(new List<Domain.Student.Entities.Fafsa>(), 0);
            _financialAidApplicationOutcomeRepositoryMock.Setup(repo => repo.GetAsync(It.IsAny<int>(), It.IsAny<int>(), false, It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>())).ReturnsAsync(new Tuple<IEnumerable<Fafsa>, int>(_financialAidApplicationOutcomeCollection, 3));
            var faFilter = new Dtos.FinancialAidApplicationOutcome();
            faFilter.Applicant = new Dtos.DtoProperties.FinancialAidApplicationApplicant() { Person = new GuidObject2() { Id = "abc" } };
            Tuple<IEnumerable<Dtos.FinancialAidApplicationOutcome>, int> financialAidApplicationOutcomes = await _financialAidApplicationOutcomeService.GetAsync(0, 100, faFilter, false);
            Assert.AreEqual(emptyTuple.Item1.Count(), 0);
        }

        [TestMethod]
        public async Task StudentFinancialAidApplicationService_GetFinancialAidApplicationsAsync_AidYear_Cache()
        {
            _financialAidApplicationOutcomeRepositoryMock.Setup(repo => repo.GetAsync(It.IsAny<int>(), It.IsAny<int>(), false, It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>())).ReturnsAsync(new Tuple<IEnumerable<Fafsa>, int>(_financialAidApplicationOutcomeCollection, 3));
            var faFilter = new Dtos.FinancialAidApplicationOutcome();
            faFilter.AidYear = new GuidObject2() { Id = "9C3B805D-CFE6-483B-86C3-4C20562F8C15".ToLower() };
            Tuple<IEnumerable<Dtos.FinancialAidApplicationOutcome>, int> financialAidApplicationOutcomes = await _financialAidApplicationOutcomeService.GetAsync(0, 100, faFilter, false);
            Assert.AreEqual(_financialAidApplicationOutcomeCollection.ElementAt(0).Guid, financialAidApplicationOutcomes.Item1.ElementAt(0).Id);
        }

        [TestMethod]
        public async Task StudentFinancialAidApplicationService_GetFinancialAidApplicationsAsync_Invalid_AidYear_Cache()
        {
            var emptyTuple = new Tuple<IEnumerable<Domain.Student.Entities.Fafsa>, int>(new List<Domain.Student.Entities.Fafsa>(), 0);
            _financialAidApplicationOutcomeRepositoryMock.Setup(repo => repo.GetAsync(It.IsAny<int>(), It.IsAny<int>(), false, It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>())).ReturnsAsync(new Tuple<IEnumerable<Fafsa>, int>(_financialAidApplicationOutcomeCollection, 3));
            var faFilter = new Dtos.FinancialAidApplicationOutcome();
            faFilter.AidYear = new GuidObject2() { Id = "abc".ToLower() };
            Tuple<IEnumerable<Dtos.FinancialAidApplicationOutcome>, int> financialAidApplicationOutcomes = await _financialAidApplicationOutcomeService.GetAsync(0, 100, faFilter, false);
            Assert.AreEqual(emptyTuple.Item1.Count(), 0);
        }

        [TestMethod]
        [ExpectedException(typeof(IntegrationApiException))]
        public async Task StudentFinancialAidApplicationService_GetFinancialAidApplicationsAsync_MissingApplicantGuid()
        {
            var collection = new List<Fafsa>();
            foreach (var entity in _financialAidApplicationOutcomeCollection)
            {
                entity.CalcResultsGuid = string.Empty;
                collection.Add(entity);
            }

            _financialAidApplicationOutcomeRepositoryMock.Setup(repo => repo.GetAsync(It.IsAny<int>(), It.IsAny<int>(), false, It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>()))
                .ReturnsAsync(new Tuple<IEnumerable<Fafsa>, int>(collection, 3));

            var faFilter = new Dtos.FinancialAidApplicationOutcome();
            faFilter.Applicant = new Dtos.DtoProperties.FinancialAidApplicationApplicant() { Person = new GuidObject2() { Id = "9ae3a175-1dfd-4937-b97b-3c9ad596e023" } };
            Tuple<IEnumerable<Dtos.FinancialAidApplicationOutcome>, int> financialAidApplicationOutcomes = await _financialAidApplicationOutcomeService.GetAsync(0, 100, faFilter, false);
        }
               
        [TestMethod]
        [ExpectedException(typeof (ArgumentNullException))]
        public async Task FinancialAidApplicationOutcomeService_GetFinancialAidApplicationOutcomeByGuidAsync_Empty()
        {
            await _financialAidApplicationOutcomeService.GetByIdAsync("");
        }

        [TestMethod]
        [ExpectedException(typeof (ArgumentNullException))]
        public async Task FinancialAidApplicationOutcomeService_GetFinancialAidApplicationOutcomeByGuidAsync_Null()
        {
            await _financialAidApplicationOutcomeService.GetByIdAsync(null);
        }

        [TestMethod]
        [ExpectedException(typeof (KeyNotFoundException))]
        public async Task FinancialAidApplicationOutcomeService_GetFinancialAidApplicationOutcomeByGuidAsync_InvalidId()
        {
            _financialAidApplicationOutcomeRepositoryMock.Setup(repo => repo.GetByIdAsync(It.IsAny<string>()))
                .Throws<KeyNotFoundException>();

            await _financialAidApplicationOutcomeService.GetByIdAsync("99");
        }

        [TestMethod]
        public async Task FinancialAidApplicationOutcomeService_GetFinancialAidApplicationOutcomeByGuidAsync_Expected()
        {
            var expectedResults =
                _financialAidApplicationOutcomeCollection.First(c => c.Guid == financialAidApplicationOutcomeGuid);
            _financialAidApplicationOutcomeRepositoryMock.Setup(repo => repo.GetByIdAsync(It.IsAny<string>()))
                .ReturnsAsync(expectedResults);
            var actualResult =
                await _financialAidApplicationOutcomeService.GetByIdAsync(financialAidApplicationOutcomeGuid);
            Assert.AreEqual(expectedResults.Guid, actualResult.Id);
            
        }

        [TestMethod]
        public async Task FinancialAidApplicationOutcomeService_GetFinancialAidApplicationOutcomeByGuidAsync_Properties()
        {
            var expectedResults =
                _financialAidApplicationOutcomeCollection.First(c => c.Guid == financialAidApplicationOutcomeGuid);
            _financialAidApplicationOutcomeRepositoryMock.Setup(repo => repo.GetByIdAsync(It.IsAny<string>()))
                .ReturnsAsync(expectedResults);
            var result =
                await _financialAidApplicationOutcomeService.GetByIdAsync(financialAidApplicationOutcomeGuid);
            Assert.IsNotNull(result.Id);
            
        }
    }
}