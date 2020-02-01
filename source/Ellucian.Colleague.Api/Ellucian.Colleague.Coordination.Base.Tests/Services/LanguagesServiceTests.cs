//Copyright 2019 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Colleague.Domain.Base.Entities;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Dtos;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Web.Adapters;
using Ellucian.Web.Security;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Colleague.Domain.Entities;
using Ellucian.Colleague.Coordination.Base.Tests.UserFactories;

namespace Ellucian.Colleague.Coordination.Base.Tests.Services
{
    [TestClass]
    public class LanguagesServiceTests : GenericUserFactory
    {
        private const string languagesGuid = "7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc";
        private const string languagesCode = "ENG";
        private ICollection<Language2> _languagesCollection;
        private LanguagesService _languagesService;

        private Mock<ILanguageRepository> _languageRepositoryMock;
        private Mock<IReferenceDataRepository> _referenceRepositoryMock;
        private Mock<ILogger> _loggerMock;
        private Mock<IAdapterRegistry> _adapterRegistryMock;
        private Mock<ICurrentUserFactory> _currentUserFactoryMock;
        private Mock<IRoleRepository> _roleRepositoryMock;
        private Mock<IConfigurationRepository> _configurationRepoMock;
        private ICurrentUserFactory currentUserFactory;


        [TestInitialize]
        public async void Initialize()
        {
            _languageRepositoryMock = new Mock<ILanguageRepository>();
            _referenceRepositoryMock = new Mock<IReferenceDataRepository>();
            _adapterRegistryMock = new Mock<IAdapterRegistry>();
            _loggerMock = new Mock<ILogger>();
            _currentUserFactoryMock = new Mock<ICurrentUserFactory>();
            _roleRepositoryMock = new Mock<IRoleRepository>();
            _configurationRepoMock = new Mock<IConfigurationRepository>();

            currentUserFactory = new LanguageUser();

            _languagesCollection = new List<Language2>()
                {
                    new Language2("7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc", "ENG", "English"),
                    new Language2("849e6a7c-6cd4-4f98-8a73-ab0aa3627f0d", "AC", "Academic"),
                    new Language2("d2253ac7-9931-4560-b42f-1fccd43c952e", "CU", "Cultural")
                };

            _languageRepositoryMock.Setup(repo => repo.GetLanguagesAsync(It.IsAny<bool>()))
                .ReturnsAsync(_languagesCollection);

            _languageRepositoryMock.Setup(repo => repo.GetLanguageByGuidAsync(It.IsAny<string>(), It.IsAny<bool>()))
                .ReturnsAsync(new Language2("7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc", "ENG", "English") { IsoCode = "ABC" });

            _languageRepositoryMock.Setup(repo => repo.UpdateLanguageAsync(It.IsAny<Language2>()))
                .ReturnsAsync(new Language2("7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc", "ENG", "English") { IsoCode = "ABC" });

            //var languageRole = new Domain.Entities.Role(105, "Student");
            var updateLanguageRole = new Ellucian.Colleague.Domain.Entities.Role(1, "UPDATE.LANGUAGE");
            updateLanguageRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Base.BasePermissionCodes.UpdateLanguage));
            _roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { updateLanguageRole });

            // Set up current user
            currentUserFactory = new  GenericUserFactory.LanguageUser();

            _languagesService = new LanguagesService(_languageRepositoryMock.Object, _referenceRepositoryMock.Object,
                _adapterRegistryMock.Object, currentUserFactory,
                _roleRepositoryMock.Object, _configurationRepoMock.Object, _loggerMock.Object);
        }

        [TestCleanup]
        public void Cleanup()
        {
            _languagesService = null;
            _languagesCollection = null;
            _referenceRepositoryMock = null;
            _loggerMock = null;
            _currentUserFactoryMock = null;
            _roleRepositoryMock = null;
            _configurationRepoMock = null;
        }

        [TestMethod]
        public async Task LanguagesService_GetLanguagesAsync()
        {
            var results = await _languagesService.GetLanguagesAsync(true);
            Assert.IsTrue(results is IEnumerable<Languages>);
            Assert.IsNotNull(results);
        }

        [TestMethod]
        public async Task LanguagesService_GetLanguagesAsync_Count()
        {
            var results = await _languagesService.GetLanguagesAsync(true);
            Assert.AreEqual(3, results.Count());
        }

        [TestMethod]
        public async Task LanguagesService_GetLanguagesAsync_Properties()
        {
            var result =
                (await _languagesService.GetLanguagesAsync(true)).FirstOrDefault(x => x.Code == languagesCode);
            Assert.IsNotNull(result.Id);
            Assert.IsNotNull(result.Code);
            Assert.IsNotNull(result.Title);

        }

        [TestMethod]
        public async Task LanguagesService_GetLanguagesAsync_Expected()
        {
            var expectedResults = _languagesCollection.FirstOrDefault(c => c.Guid == languagesGuid);
            var actualResult =
                (await _languagesService.GetLanguagesAsync(true)).FirstOrDefault(x => x.Id == languagesGuid);
            Assert.AreEqual(expectedResults.Guid, actualResult.Id);
            Assert.AreEqual(expectedResults.Description, actualResult.Title);
            Assert.AreEqual(expectedResults.Code, actualResult.Code);

        }

        [TestMethod]
        [ExpectedException(typeof(KeyNotFoundException))]
        public async Task LanguagesService_GetLanguagesByGuidAsync_Empty()
        {
            await _languagesService.GetLanguagesByGuidAsync("");
        }

        [TestMethod]
        [ExpectedException(typeof(KeyNotFoundException))]
        public async Task LanguagesService_GetLanguagesByGuidAsync_Null()
        {
            await _languagesService.GetLanguagesByGuidAsync(null);
        }

        [TestMethod]
        [ExpectedException(typeof(KeyNotFoundException))]
        public async Task LanguagesService_GetLanguagesByGuidAsync_InvalidId()
        {
            _languageRepositoryMock.Setup(repo => repo.GetLanguageByGuidAsync(It.IsAny<string>(), It.IsAny<bool>()))
                .Throws<KeyNotFoundException>();

            await _languagesService.GetLanguagesByGuidAsync("99");
        }

        [TestMethod]
        public async Task LanguagesService_GetLanguagesByGuidAsync_Expected()
        {
            var expectedResults =
                _languagesCollection.First(c => c.Guid == languagesGuid);
            var actualResult =
                await _languagesService.GetLanguagesByGuidAsync(languagesGuid);
            Assert.AreEqual(expectedResults.Guid, actualResult.Id);
            Assert.AreEqual(expectedResults.Description, actualResult.Title);
            Assert.AreEqual(expectedResults.Code, actualResult.Code);

        }

        [TestMethod]
        public async Task LanguagesService_GetLanguagesByGuidAsync_Properties()
        {
            var result =
                await _languagesService.GetLanguagesByGuidAsync(languagesGuid);
            Assert.IsNotNull(result.Id);
            Assert.IsNotNull(result.Code);
            Assert.IsNotNull(result.Title);

        }

        //[TestMethod]
        //public async Task LanguagesService_UpdateLanguageAsync()
        //{            
        //    var language = new Languages();
        //    language.Id = "7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc";
        //    language.Code = "ENG";
        //    language.ISOCode = "ABC";
        //    var result = await _languagesService.UpdateLanguageAsync(language, false);
        //    Assert.IsNotNull(result.Id);
        //    Assert.IsNotNull(result.Code);
        //    Assert.IsNotNull(result.Title);
        //    Assert.AreEqual(language.ISOCode, result.ISOCode);
        //}

    }

    [TestClass]
    public class LanguageIsoCodesServiceTests
    {
        private const string languageIsoCodesGuid = "7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc";
        private const string languageIsoCodesCode = "AT";
        private ICollection<Domain.Base.Entities.LanguageIsoCodes> _languageIsoCodesCollection;
        private LanguagesService _languageIsoCodesService;

        private Mock<IReferenceDataRepository> _referenceRepositoryMock;
        private Mock<ILogger> _loggerMock;
        private Mock<IAdapterRegistry> _adapterRegistryMock;
        private Mock<ICurrentUserFactory> _currentUserFactoryMock;
        private Mock<IRoleRepository> _roleRepositoryMock;
        private Mock<IConfigurationRepository> _configurationRepoMock;


        [TestInitialize]
        public async void Initialize()
        {
            _referenceRepositoryMock = new Mock<IReferenceDataRepository>();
            _adapterRegistryMock = new Mock<IAdapterRegistry>();
            _loggerMock = new Mock<ILogger>();
            _currentUserFactoryMock = new Mock<ICurrentUserFactory>();
            _roleRepositoryMock = new Mock<IRoleRepository>();
            _configurationRepoMock = new Mock<IConfigurationRepository>();


            _languageIsoCodesCollection = new List<Domain.Base.Entities.LanguageIsoCodes>()
                {
                    new Domain.Base.Entities.LanguageIsoCodes("7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc", "AT", "Athletic") {InactiveFlag = "Y" },
                    new Domain.Base.Entities.LanguageIsoCodes("849e6a7c-6cd4-4f98-8a73-ab0aa3627f0d", "AC", "Academic"),
                    new Domain.Base.Entities.LanguageIsoCodes("d2253ac7-9931-4560-b42f-1fccd43c952e", "CU", "Cultural")
                };

            _referenceRepositoryMock.Setup(repo => repo.GetLanguageIsoCodesAsync(It.IsAny<bool>()))
                .ReturnsAsync(_languageIsoCodesCollection);

            _referenceRepositoryMock.Setup(repo => repo.GetLanguageIsoCodeByGuidAsync(languageIsoCodesGuid))
                .ReturnsAsync(new Domain.Base.Entities.LanguageIsoCodes("7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc", "AT", "Athletic") { InactiveFlag = "Y" });

            _languageIsoCodesService = new LanguagesService(null, _referenceRepositoryMock.Object,
                _adapterRegistryMock.Object, _currentUserFactoryMock.Object,
                _roleRepositoryMock.Object, _configurationRepoMock.Object, _loggerMock.Object);            
        }

        [TestCleanup]
        public void Cleanup()
        {
            _languageIsoCodesService = null;
            _languageIsoCodesCollection = null;
            _referenceRepositoryMock = null;
            _loggerMock = null;
            _currentUserFactoryMock = null;
            _roleRepositoryMock = null;
            _configurationRepoMock = null;
        }

        [TestMethod]
        public async Task LanguageIsoCodesService_GetLanguageIsoCodesAsync()
        {
            var results = await _languageIsoCodesService.GetLanguageIsoCodesAsync(true);
            Assert.IsTrue(results is IEnumerable<Dtos.LanguageIsoCodes>);
            Assert.IsNotNull(results);
        }

        [TestMethod]
        public async Task LanguageIsoCodesService_GetLanguageIsoCodesAsync_Count()
        {
            var results = await _languageIsoCodesService.GetLanguageIsoCodesAsync(true);
            Assert.AreEqual(3, results.Count());
        }

        [TestMethod]
        public async Task LanguageIsoCodesService_GetLanguageIsoCodesAsync_Properties()
        {
            var result =
                (await _languageIsoCodesService.GetLanguageIsoCodesAsync(true)).FirstOrDefault(x => x.IsoCode == languageIsoCodesCode);
            Assert.IsNotNull(result.Id);
            Assert.IsNotNull(result.IsoCode);

        }

        [TestMethod]
        public async Task LanguageIsoCodesService_GetLanguageIsoCodesAsync_Expected()
        {
            var expectedResults = _languageIsoCodesCollection.FirstOrDefault(c => c.Guid == languageIsoCodesGuid);
            var actualResult =
                (await _languageIsoCodesService.GetLanguageIsoCodesAsync(true)).FirstOrDefault(x => x.Id == languageIsoCodesGuid);
            Assert.AreEqual(expectedResults.Guid, actualResult.Id);
            Assert.AreEqual(expectedResults.Description, actualResult.Title);
            Assert.AreEqual(expectedResults.Code, actualResult.IsoCode);

        }

        [TestMethod]
        [ExpectedException(typeof(KeyNotFoundException))]
        public async Task LanguageIsoCodesService_GetLanguageIsoCodesByGuidAsync_Empty()
        {
            await _languageIsoCodesService.GetLanguageIsoCodeByGuidAsync("");
        }

        [TestMethod]
        [ExpectedException(typeof(KeyNotFoundException))]
        public async Task LanguageIsoCodesService_GetLanguageIsoCodesByGuidAsync_Null()
        {
            await _languageIsoCodesService.GetLanguageIsoCodeByGuidAsync(null);
        }

        [TestMethod]
        [ExpectedException(typeof(KeyNotFoundException))]
        public async Task LanguageIsoCodesService_GetLanguageIsoCodesByGuidAsync_InvalidId()
        {
            _referenceRepositoryMock.Setup(repo => repo.GetLanguageIsoCodesAsync(It.IsAny<bool>()))
                .Throws<KeyNotFoundException>();

            await _languageIsoCodesService.GetLanguageIsoCodeByGuidAsync("99");
        }

        [TestMethod]
        public async Task LanguageIsoCodesService_GetLanguageIsoCodesByGuidAsync_Expected()
        {
            var expectedResults =
                _languageIsoCodesCollection.First(c => c.Guid == languageIsoCodesGuid);
            var actualResult =
                await _languageIsoCodesService.GetLanguageIsoCodeByGuidAsync(languageIsoCodesGuid);
            Assert.AreEqual(expectedResults.Guid, actualResult.Id);
            Assert.AreEqual(expectedResults.Description, actualResult.Title);
            Assert.AreEqual(expectedResults.Code, actualResult.IsoCode);

        }

        [TestMethod]
        public async Task LanguageIsoCodesService_GetLanguageIsoCodesByGuidAsync_Properties()
        {
            var result =
                await _languageIsoCodesService.GetLanguageIsoCodeByGuidAsync(languageIsoCodesGuid);
            Assert.IsNotNull(result.Id);
            Assert.IsNotNull(result.IsoCode);
            Assert.IsNotNull(result.Title);

        }
    }
}