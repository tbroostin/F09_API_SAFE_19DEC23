//Copyright 2017-2018 Ellucian Company L.P. and its affiliates.

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

namespace Ellucian.Colleague.Coordination.Student.Tests.Services
{
    [TestClass]
    public class FinancialAidApplicationOutcomeerviceTests : StudentUserFactory
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

            _financialAidApplicationOutcomeRepositoryMock.Setup(repo => repo.GetAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<List<string>>()))
                .ReturnsAsync(new Tuple<IEnumerable<Fafsa>, int>(_financialAidApplicationOutcomeCollection, 3));

            _financialAidApplicationOutcomeService = new FinancialAidApplicationOutcomeService(_financialAidApplicationOutcomeRepositoryMock.Object,
                _personRepositoryMock.Object, _referenceRepositoryMock.Object, baseConfigurationRepository,
                _adapterRegistryMock.Object, _currentUserFactory, _roleRepositoryMock.Object, _loggerMock.Object);
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
            var resultsTuple = await _financialAidApplicationOutcomeService.GetAsync(0, 3, true);
            var results = resultsTuple.Item1;
            Assert.IsTrue(results is IEnumerable<FinancialAidApplicationOutcome>);
            Assert.IsNotNull(results);
        }

        [TestMethod]
        public async Task FinancialAidApplicationOutcomeService_GetFinancialAidApplicationOutcomeAsync_Count()
        {
            var resultsTuple = await _financialAidApplicationOutcomeService.GetAsync(0, 3, true);
            var resultCount = resultsTuple.Item2;
            Assert.AreEqual(resultCount, 3);
        }

        [TestMethod]
        public async Task FinancialAidApplicationOutcomeService_GetFinancialAidApplicationOutcomeAsync_Properties()
        {
            var result =
                (await _financialAidApplicationOutcomeService.GetAsync(0, 3, true)).Item1.FirstOrDefault(x => x.Id == financialAidApplicationOutcomeGuid);
            Assert.IsNotNull(result.Id);
           
        }

        [TestMethod]
        public async Task FinancialAidApplicationOutcomeService_GetFinancialAidApplicationOutcomeAsync_Expected()
        {
            var expectedResults = _financialAidApplicationOutcomeCollection.FirstOrDefault(c => c.Guid == financialAidApplicationOutcomeGuid);
            var actualResult =
                (await _financialAidApplicationOutcomeService.GetAsync(0, 3, true)).Item1.FirstOrDefault(x => x.Id == financialAidApplicationOutcomeGuid);
            Assert.AreEqual(expectedResults.Guid, actualResult.Id);
            
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