// Copyright 2015-2019 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Coordination.Student.Services;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.Repositories;
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
using GradeScheme = Ellucian.Colleague.Domain.Student.Entities.GradeScheme;
using GradeSubscheme = Ellucian.Colleague.Domain.Student.Entities.GradeSubscheme;
namespace Ellucian.Colleague.Coordination.Student.Tests.Services
{
    [TestClass]
    public class GradeSchemeServiceTests
    {
        private const string GradeSchemeGuid = "bb66b971-3ee0-4477-9bb7-539721f93434";
        private const string GradeSchemeCode = "CE";
        private ICollection<GradeScheme> _gradeSchemeCollection;
        private GradeSchemeService _gradeSchemeService;
        private Mock<ILogger> _loggerMock;
        private Mock<IStudentReferenceDataRepository> _studentReferenceRepositoryMock;
        private Mock<IAdapterRegistry> _adapterRegistryMock;
        private Mock<ICurrentUserFactory> _currentUserFactoryMock;
        private Mock<IRoleRepository> _roleRepositoryMock;
        private Mock<IConfigurationRepository> _configRepositoryMock;

        [TestInitialize]
        public async void Initialize()
        {
            _studentReferenceRepositoryMock = new Mock<IStudentReferenceDataRepository>();
            _loggerMock = new Mock<ILogger>();

            _gradeSchemeCollection = (await new TestStudentReferenceDataRepository().GetGradeSchemesAsync()).ToList();

            _studentReferenceRepositoryMock.Setup(repo => repo.GetGradeSchemesAsync())
                .ReturnsAsync(_gradeSchemeCollection);
            _studentReferenceRepositoryMock.Setup(repo => repo.GetGradeSchemesAsync(It.IsAny<bool>()))
                .ReturnsAsync(_gradeSchemeCollection);
            _adapterRegistryMock = new Mock<IAdapterRegistry>();
            _currentUserFactoryMock = new Mock<ICurrentUserFactory>();
            _roleRepositoryMock = new Mock<IRoleRepository>();
            _configRepositoryMock = new Mock<IConfigurationRepository>();

            _gradeSchemeService = new GradeSchemeService(_studentReferenceRepositoryMock.Object, _loggerMock.Object, _configRepositoryMock.Object, _adapterRegistryMock.Object, _currentUserFactoryMock.Object, _roleRepositoryMock.Object);
        }

        [TestCleanup]
        public void Cleanup()
        {
            _gradeSchemeService = null;
            _gradeSchemeCollection = null;
            _studentReferenceRepositoryMock = null;
            _loggerMock = null;
        }

        [TestMethod]
        public async Task GradeSchemeService_GetGradeSchemesAsync()
        {
            var results = await _gradeSchemeService.GetGradeSchemesAsync();
            Assert.IsTrue(results is IEnumerable<Dtos.GradeScheme>);
            Assert.IsNotNull(results);
        }

        [TestMethod]
        public async Task GradeSchemeService_GetGradeSchemesAsync_Count()
        {
            var results = await _gradeSchemeService.GetGradeSchemesAsync();
            Assert.AreEqual(4, results.Count());
        }

        [TestMethod]
        public async Task GradeSchemeService_GetGradeSchemesAsync_Properties()
        {
            var result =
                (await _gradeSchemeService.GetGradeSchemesAsync()).FirstOrDefault(x => x.Abbreviation == GradeSchemeCode);
            Assert.IsNotNull(result.Guid);
            Assert.IsNotNull(result.Abbreviation);
            Assert.IsNull(result.Description);
            Assert.IsNotNull(result.Title);
            Assert.IsNotNull(result.EffectiveStartDate);
            Assert.IsNotNull(result.EffectiveEndDate);
        }

        [TestMethod]
        public async Task GradeSchemeService_GetGradeSchemesAsync_Expected()
        {
            var expectedResults = _gradeSchemeCollection.FirstOrDefault(c => c.Guid == GradeSchemeGuid);
            var actualResult =
                (await _gradeSchemeService.GetGradeSchemesAsync()).FirstOrDefault(x => x.Guid == GradeSchemeGuid);
            Assert.AreEqual(expectedResults.Guid, actualResult.Guid);
            Assert.AreEqual(expectedResults.Description, actualResult.Title);
            Assert.AreEqual(expectedResults.Code, actualResult.Abbreviation);
            Assert.AreEqual(expectedResults.EffectiveStartDate, actualResult.EffectiveStartDate);
            Assert.AreEqual(expectedResults.EffectiveEndDate, actualResult.EffectiveEndDate);
        }

        [TestMethod]
        [ExpectedException(typeof (InvalidOperationException))]
        public async Task GradeSchemeService_GetGradeSchemeByGuidAsync_Empty()
        {
            await _gradeSchemeService.GetGradeSchemeByGuidAsync("");
        }

        [TestMethod]
        [ExpectedException(typeof (InvalidOperationException))]
        public async Task GradeSchemeService_GetGradeSchemeByGuidAsync_Null()
        {
            await _gradeSchemeService.GetGradeSchemeByGuidAsync(null);
        }

        [TestMethod]
        [ExpectedException(typeof (InvalidOperationException))]
        public async Task GradeSchemeService_GetGradeSchemeByGuidAsync_InvalidId()
        {
            _studentReferenceRepositoryMock.Setup(repo => repo.GetGradeSchemesAsync())
                .Throws<InvalidOperationException>();

            await _gradeSchemeService.GetGradeSchemeByGuidAsync("99");
        }

        [TestMethod]
        public async Task GradeSchemeService_GetGradeSchemeByGuidAsync_Expected()
        {
            var expectedResults =
                _gradeSchemeCollection.First(c => c.Guid == GradeSchemeGuid);
            var actualResult =
                await _gradeSchemeService.GetGradeSchemeByGuidAsync(GradeSchemeGuid);
            Assert.AreEqual(expectedResults.Guid, actualResult.Guid);
            Assert.AreEqual(expectedResults.Description, actualResult.Title);
            Assert.AreEqual(expectedResults.Code, actualResult.Abbreviation);
            Assert.AreEqual(expectedResults.EffectiveStartDate, actualResult.EffectiveStartDate);
            Assert.AreEqual(expectedResults.EffectiveEndDate, actualResult.EffectiveEndDate);
        }

        [TestMethod]
        public async Task GradeSchemeService_GetGradeSchemeByGuidAsync_Properties()
        {
            var result =
                await _gradeSchemeService.GetGradeSchemeByGuidAsync(GradeSchemeGuid);
            Assert.IsNotNull(result.Guid);
            Assert.IsNotNull(result.Abbreviation);
            Assert.IsNull(result.Description);
            Assert.IsNotNull(result.Title);
            Assert.IsNotNull(result.EffectiveStartDate);
            Assert.IsNotNull(result.EffectiveEndDate);
        }
    }

    [TestClass]
    public class GradeSchemeService_GetNonEthosGradeSchemeByIdAsync_Tests
    {
        private ICollection<GradeScheme> _gradeSchemeCollection;
        private GradeSchemeService _gradeSchemeService;
        private Mock<ILogger> _loggerMock;
        private Mock<IStudentReferenceDataRepository> _studentReferenceRepositoryMock;
        private Mock<IAdapterRegistry> _adapterRegistryMock;
        private Mock<ICurrentUserFactory> _currentUserFactoryMock;
        private Mock<IRoleRepository> _roleRepositoryMock;
        private Mock<IConfigurationRepository> _configRepositoryMock;

        [TestInitialize]
        public async void Initialize()
        {
            _studentReferenceRepositoryMock = new Mock<IStudentReferenceDataRepository>();
            _loggerMock = new Mock<ILogger>();

            _gradeSchemeCollection = (await new TestStudentReferenceDataRepository().GetGradeSchemesAsync()).ToList();

            _studentReferenceRepositoryMock.Setup(repo => repo.GetGradeSchemesAsync())
                .ReturnsAsync(_gradeSchemeCollection);

            _adapterRegistryMock = new Mock<IAdapterRegistry>();
            _currentUserFactoryMock = new Mock<ICurrentUserFactory>();
            _roleRepositoryMock = new Mock<IRoleRepository>();
            _configRepositoryMock = new Mock<IConfigurationRepository>();

            var gradeSchemeAdapter = new AutoMapperAdapter<Ellucian.Colleague.Domain.Student.Entities.GradeScheme, Ellucian.Colleague.Dtos.Student.GradeScheme>(_adapterRegistryMock.Object, _loggerMock.Object);
            _adapterRegistryMock.Setup(x => x.GetAdapter<Ellucian.Colleague.Domain.Student.Entities.GradeScheme, Ellucian.Colleague.Dtos.Student.GradeScheme>()).Returns(gradeSchemeAdapter);

            _gradeSchemeService = new GradeSchemeService(_studentReferenceRepositoryMock.Object, _loggerMock.Object, _configRepositoryMock.Object, _adapterRegistryMock.Object, _currentUserFactoryMock.Object, _roleRepositoryMock.Object);
        }

        [TestCleanup]
        public void Cleanup()
        {
            _gradeSchemeService = null;
            _gradeSchemeCollection = null;
            _studentReferenceRepositoryMock = null;
            _loggerMock = null;
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task GradeSchemeService_GetNonEthosGradeSchemeByIdAsync_Null_Id()
        {
            var scheme = await _gradeSchemeService.GetNonEthosGradeSchemeByIdAsync(null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task GradeSchemeService_GetNonEthosGradeSchemeByIdAsync_Empty_Id()
        {
            var scheme = await _gradeSchemeService.GetNonEthosGradeSchemeByIdAsync(string.Empty);
        }

        [TestMethod]
        public async Task GradeSchemeService_GetNonEthosGradeSchemeByIdAsync_valid()
        {
            var scheme = await _gradeSchemeService.GetNonEthosGradeSchemeByIdAsync(_gradeSchemeCollection.First().Code);
            Assert.AreEqual(_gradeSchemeCollection.First().Code, scheme.Code);
            Assert.AreEqual(_gradeSchemeCollection.First().Description, scheme.Description);
            CollectionAssert.AreEqual(_gradeSchemeCollection.First().GradeCodes.ToList(), scheme.GradeCodes.ToList());
        }

        [TestMethod]
        [ExpectedException(typeof(KeyNotFoundException))]
        public async Task GradeSchemeService_GetNonEthosGradeSchemeByIdAsync_key_not_found()
        {
            _studentReferenceRepositoryMock.Setup(repo => repo.GetGradeSchemesAsync())
                .ReturnsAsync(_gradeSchemeCollection);

            var scheme = await _gradeSchemeService.GetNonEthosGradeSchemeByIdAsync("GARBAGE");
        }

        [TestMethod]
        [ExpectedException(typeof(KeyNotFoundException))]
        public async Task GradeSchemeService_GetNonEthosGradeSchemeByIdAsync_Repo_returns_null()
        {
            _studentReferenceRepositoryMock.Setup(repo => repo.GetGradeSchemesAsync())
                .ReturnsAsync(null);

            var scheme = await _gradeSchemeService.GetNonEthosGradeSchemeByIdAsync("UG");
        }

        [TestMethod]
        [ExpectedException(typeof(KeyNotFoundException))]
        public async Task GradeSchemeService_GetNonEthosGradeSchemeByIdAsync_Repo_returns_empty_list()
        {
            _studentReferenceRepositoryMock.Setup(repo => repo.GetGradeSchemesAsync())
                .ReturnsAsync(new List<GradeScheme>());

            var scheme = await _gradeSchemeService.GetNonEthosGradeSchemeByIdAsync("UG");
        }
    }

    [TestClass]
    public class GradeSchemeService_GetGradeSubschemeByIdAsync_Tests
    {
        private ICollection<GradeSubscheme> _gradeSubschemeCollection;
        private GradeSchemeService _gradeSchemeService;
        private Mock<ILogger> _loggerMock;
        private Mock<IStudentReferenceDataRepository> _studentReferenceRepositoryMock;
        private Mock<IAdapterRegistry> _adapterRegistryMock;
        private Mock<ICurrentUserFactory> _currentUserFactoryMock;
        private Mock<IRoleRepository> _roleRepositoryMock;
        private Mock<IConfigurationRepository> _configRepositoryMock;

        [TestInitialize]
        public async void Initialize()
        {
            _studentReferenceRepositoryMock = new Mock<IStudentReferenceDataRepository>();
            _loggerMock = new Mock<ILogger>();

            _gradeSubschemeCollection = (await new TestStudentReferenceDataRepository().GetGradeSubschemesAsync()).ToList();

            _studentReferenceRepositoryMock.Setup(repo => repo.GetGradeSubschemesAsync())
                .ReturnsAsync(_gradeSubschemeCollection);

            _adapterRegistryMock = new Mock<IAdapterRegistry>();
            _currentUserFactoryMock = new Mock<ICurrentUserFactory>();
            _roleRepositoryMock = new Mock<IRoleRepository>();
            _configRepositoryMock = new Mock<IConfigurationRepository>();

            var gradeSubschemeAdapter = new AutoMapperAdapter<Ellucian.Colleague.Domain.Student.Entities.GradeSubscheme, Ellucian.Colleague.Dtos.Student.GradeSubscheme>(_adapterRegistryMock.Object, _loggerMock.Object);
            _adapterRegistryMock.Setup(x => x.GetAdapter<Ellucian.Colleague.Domain.Student.Entities.GradeSubscheme, Ellucian.Colleague.Dtos.Student.GradeSubscheme>()).Returns(gradeSubschemeAdapter);

            _gradeSchemeService = new GradeSchemeService(_studentReferenceRepositoryMock.Object, _loggerMock.Object, _configRepositoryMock.Object, _adapterRegistryMock.Object, _currentUserFactoryMock.Object, _roleRepositoryMock.Object);
        }

        [TestCleanup]
        public void Cleanup()
        {
            _gradeSchemeService = null;
            _gradeSubschemeCollection = null;
            _studentReferenceRepositoryMock = null;
            _loggerMock = null;
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task GradeSchemeService_GetGradeSubschemeByIdAsync_Null_Id()
        {
            var scheme = await _gradeSchemeService.GetGradeSubschemeByIdAsync(null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task GradeSchemeService_GetGradeSubschemeByIdAsync_Empty_Id()
        {
            var scheme = await _gradeSchemeService.GetGradeSubschemeByIdAsync(string.Empty);
        }

        [TestMethod]
        public async Task GradeSchemeService_GetGradeSubschemeByIdAsync_valid()
        {
            var scheme = await _gradeSchemeService.GetGradeSubschemeByIdAsync(_gradeSubschemeCollection.First().Code);
            Assert.AreEqual(_gradeSubschemeCollection.First().Code, scheme.Code);
            Assert.AreEqual(_gradeSubschemeCollection.First().Description, scheme.Description);
            CollectionAssert.AreEqual(_gradeSubschemeCollection.First().GradeCodes.ToList(), scheme.GradeCodes.ToList());
        }

        [TestMethod]
        [ExpectedException(typeof(KeyNotFoundException))]
        public async Task GradeSchemeService_GetGradeSubschemeByIdAsync_key_not_found()
        {
            _studentReferenceRepositoryMock.Setup(repo => repo.GetGradeSubschemesAsync())
                .ReturnsAsync(_gradeSubschemeCollection);

            var scheme = await _gradeSchemeService.GetGradeSubschemeByIdAsync("GARBAGE");
        }

        [TestMethod]
        [ExpectedException(typeof(KeyNotFoundException))]
        public async Task GradeSchemeService_GetGradeSubschemeByIdAsync_Repo_returns_null()
        {
            _studentReferenceRepositoryMock.Setup(repo => repo.GetGradeSubschemesAsync())
                .ReturnsAsync(null);

            var scheme = await _gradeSchemeService.GetGradeSubschemeByIdAsync("UG");
        }

        [TestMethod]
        [ExpectedException(typeof(KeyNotFoundException))]
        public async Task GradeSchemeService_GetGradeSubschemeByIdAsync_Repo_returns_empty_list()
        {
            _studentReferenceRepositoryMock.Setup(repo => repo.GetGradeSubschemesAsync())
                .ReturnsAsync(new List<GradeSubscheme>());

            var scheme = await _gradeSchemeService.GetGradeSubschemeByIdAsync("UG");
        }
    }

    [TestClass]
    public class GradeSchemeService2Tests
    {
        private const string GradeSchemeGuid = "bb66b971-3ee0-4477-9bb7-539721f93434";
        private const string GradeSchemeCode = "CE";
        private ICollection<GradeScheme> _gradeSchemeCollection;
        private GradeSchemeService _gradeSchemeService;
        private Mock<ILogger> _loggerMock;
        private Mock<IStudentReferenceDataRepository> _studentReferenceRepositoryMock;
        private Mock<IAdapterRegistry> _adapterRegistryMock;
        private Mock<ICurrentUserFactory> _currentUserFactoryMock;
        private Mock<IRoleRepository> _roleRepositoryMock;
        private Mock<IConfigurationRepository> _configRepositoryMock;

        [TestInitialize]
        public async void Initialize()
        {
            _studentReferenceRepositoryMock = new Mock<IStudentReferenceDataRepository>();
            _loggerMock = new Mock<ILogger>();

            _gradeSchemeCollection = (await new TestStudentReferenceDataRepository().GetGradeSchemesAsync()).ToList();

            _studentReferenceRepositoryMock.Setup(repo => repo.GetGradeSchemesAsync())
                .ReturnsAsync(_gradeSchemeCollection);
            _studentReferenceRepositoryMock.Setup(repo => repo.GetGradeSchemesAsync(It.IsAny<bool>()))
                .ReturnsAsync(_gradeSchemeCollection);

            _adapterRegistryMock = new Mock<IAdapterRegistry>();
            _currentUserFactoryMock = new Mock<ICurrentUserFactory>();
            _roleRepositoryMock = new Mock<IRoleRepository>();
            _configRepositoryMock = new Mock<IConfigurationRepository>();

            _gradeSchemeService = new GradeSchemeService(_studentReferenceRepositoryMock.Object, _loggerMock.Object, _configRepositoryMock.Object, _adapterRegistryMock.Object, _currentUserFactoryMock.Object, _roleRepositoryMock.Object);
        }

        [TestCleanup]
        public void Cleanup()
        {
            _gradeSchemeService = null;
            _gradeSchemeCollection = null;
            _studentReferenceRepositoryMock = null;
            _loggerMock = null;
        }

        [TestMethod]
        public async Task GradeSchemeService_GetGradeSchemes2Async()
        {
            var results = await _gradeSchemeService.GetGradeSchemes2Async(true);
            Assert.IsTrue(results is IEnumerable<GradeScheme2>);
            Assert.IsNotNull(results);
        }

        [TestMethod]
        public async Task GradeSchemeService_GetGradeSchemes2Async_Count()
        {
            var results = await _gradeSchemeService.GetGradeSchemes2Async(true);
            Assert.AreEqual(4, results.Count());
        }

        [TestMethod]
        public async Task GradeSchemeService_GetGradeSchemes2Async_Properties()
        {
            var result =
                (await _gradeSchemeService.GetGradeSchemes2Async(true)).FirstOrDefault(x => x.Code == GradeSchemeCode);
            Assert.IsNotNull(result.Id);
            Assert.IsNotNull(result.Code);
            Assert.IsNull(result.Description);
            Assert.IsNotNull(result.Title);
            Assert.IsNotNull(result.StartOn);
            Assert.IsNotNull(result.EndOn);
        }

        [TestMethod]
        public async Task GradeSchemeService_GetGradeSchemes2Async_Expected()
        {
            var expectedResults = _gradeSchemeCollection.FirstOrDefault(c => c.Guid == GradeSchemeGuid);
            var actualResult =
                (await _gradeSchemeService.GetGradeSchemes2Async(true)).FirstOrDefault(x => x.Id == GradeSchemeGuid);
            Assert.AreEqual(expectedResults.Guid, actualResult.Id);
            Assert.AreEqual(expectedResults.Description, actualResult.Title);
            Assert.AreEqual(expectedResults.Code, actualResult.Code);
            Assert.AreEqual(expectedResults.EffectiveStartDate, actualResult.StartOn);
            Assert.AreEqual(expectedResults.EffectiveEndDate, actualResult.EndOn);
        }

        [TestMethod]
        [ExpectedException(typeof (InvalidOperationException))]
        public async Task GradeSchemeService_GetGradeSchemeByIdAsync_Empty()
        {
            await _gradeSchemeService.GetGradeSchemeByIdAsync("");
        }

        [TestMethod]
        [ExpectedException(typeof (InvalidOperationException))]
        public async Task GradeSchemeService_GetGradeSchemeByIdAsync_Null()
        {
            await _gradeSchemeService.GetGradeSchemeByIdAsync(null);
        }

        [TestMethod]
        [ExpectedException(typeof (InvalidOperationException))]
        public async Task GradeSchemeService_GetGradeSchemeByIdAsync_InvalidId()
        {
            _studentReferenceRepositoryMock.Setup(repo => repo.GetGradeSchemesAsync())
                .Throws<InvalidOperationException>();

            await _gradeSchemeService.GetGradeSchemeByIdAsync("99");
        }

        [TestMethod]
        public async Task GradeSchemeService_GetGradeSchemeByIdAsync_Expected()
        {
            var expectedResults =
                _gradeSchemeCollection.First(c => c.Guid == GradeSchemeGuid);
            var actualResult =
                await _gradeSchemeService.GetGradeSchemeByIdAsync(GradeSchemeGuid);
            Assert.AreEqual(expectedResults.Guid, actualResult.Id);
            Assert.AreEqual(expectedResults.Description, actualResult.Title);
            Assert.AreEqual(expectedResults.Code, actualResult.Code);
            Assert.AreEqual(expectedResults.EffectiveStartDate, actualResult.StartOn);
            Assert.AreEqual(expectedResults.EffectiveEndDate, actualResult.EndOn);
        }

        [TestMethod]
        public async Task GradeSchemeService_GetGradeSchemeByIdAsync_Properties()
        {
            var result =
                await _gradeSchemeService.GetGradeSchemeByIdAsync(GradeSchemeGuid);
            Assert.IsNotNull(result.Id);
            Assert.IsNotNull(result.Code);
            Assert.IsNull(result.Description);
            Assert.IsNotNull(result.Title);
            Assert.IsNotNull(result.StartOn);
            Assert.IsNotNull(result.EndOn);
        }
    }
}