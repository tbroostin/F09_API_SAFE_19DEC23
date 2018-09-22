//Copyright 2017 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ellucian.Colleague.Coordination.Student.Services;
using Ellucian.Colleague.Domain.Student.Entities;
using Ellucian.Colleague.Domain.Student.Repositories;
using Ellucian.Colleague.Dtos;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;
using Ellucian.Web.Security;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Web.Adapters;
using Ellucian.Colleague.Domain.Base.Repositories;

namespace Ellucian.Colleague.Coordination.Student.Tests.Services
{    
    [TestClass]
    public class AdmissionResidencyTypesServiceTests
    {
        private const string admissionResidencyTypesGuid = "7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc";
        private const string admissionResidencyTypesCode = "AT";
        private ICollection<AdmissionResidencyType> _admissionResidencyTypesCollection;
        private AdmissionResidencyTypesService _admissionResidencyTypesService;
        private ILogger logger;
        private Mock<IAdapterRegistry> adapterRegistryMock;
        private IAdapterRegistry adapterRegistry;
        private Mock<ICurrentUserFactory> userFactoryMock;
        private ICurrentUserFactory userFactory;
        private Mock<IRoleRepository> roleRepoMock;
        private IRoleRepository roleRepo;
        private Mock<IStudentReferenceDataRepository> _referenceRepositoryMock;
        private Mock<IStaffRepository> _staffRepositoryMock;
        private IConfigurationRepository baseConfigurationRepository;
        private Mock<IConfigurationRepository> baseConfigurationRepositoryMock;

        [TestInitialize]
        public async void Initialize()
        {
            _referenceRepositoryMock = new Mock<IStudentReferenceDataRepository>();
            _staffRepositoryMock = new Mock<IStaffRepository>();
            adapterRegistryMock = new Mock<IAdapterRegistry>();
            adapterRegistry = adapterRegistryMock.Object;
            userFactoryMock = new Mock<ICurrentUserFactory>();
            userFactory = userFactoryMock.Object;
            roleRepoMock = new Mock<IRoleRepository>();
            roleRepo = roleRepoMock.Object;
            logger = new Mock<ILogger>().Object;
            baseConfigurationRepositoryMock = new Mock<IConfigurationRepository>();
            baseConfigurationRepository = baseConfigurationRepositoryMock.Object;

            _admissionResidencyTypesCollection = new List<AdmissionResidencyType>()
                {
                    new AdmissionResidencyType("7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc", "AT", "Athletic"),
                    new AdmissionResidencyType("849e6a7c-6cd4-4f98-8a73-ab0aa3627f0d", "AC", "Academic"),
                    new AdmissionResidencyType("d2253ac7-9931-4560-b42f-1fccd43c952e", "CU", "Cultural")
                };


            _referenceRepositoryMock.Setup(repo => repo.GetAdmissionResidencyTypesAsync(It.IsAny<bool>()))
                .ReturnsAsync(_admissionResidencyTypesCollection);

            _admissionResidencyTypesService = new AdmissionResidencyTypesService(_referenceRepositoryMock.Object,  _staffRepositoryMock.Object, adapterRegistry, userFactory, roleRepo, logger, baseConfigurationRepository);
        }

        [TestCleanup]
        public void Cleanup()
        {
            _admissionResidencyTypesService = null;
            _admissionResidencyTypesCollection = null;
            _referenceRepositoryMock = null;
            logger = null;
        }

        [TestMethod]
        public async Task AdmissionResidencyTypesService_GetAdmissionResidencyTypesAsync()
        {
            var results = await _admissionResidencyTypesService.GetAdmissionResidencyTypesAsync(true);
            Assert.IsTrue(results is IEnumerable<AdmissionResidencyTypes>);
            Assert.IsNotNull(results);
        }

        [TestMethod]
        public async Task AdmissionResidencyTypesService_GetAdmissionResidencyTypesAsync_Count()
        {
            var results = await _admissionResidencyTypesService.GetAdmissionResidencyTypesAsync(true);
            Assert.AreEqual(3, results.Count());
        }

        //[TestMethod]
        //public async Task AdmissionResidencyTypesService_GetAdmissionResidencyTypesAsync_Properties()
        //{
        //    var result =
        //        (await _admissionResidencyTypesService.GetAdmissionResidencyTypesAsync(true)).FirstOrDefault(x => x.Code == admissionResidencyTypesCode);
        //    Assert.IsNotNull(result.Id);
        //    //Assert.IsNotNull(result.Code);
        //    Assert.IsNull(result.Description);
           
        //}

        [TestMethod]
        public async Task AdmissionResidencyTypesService_GetAdmissionResidencyTypesAsync_Expected()
        {
            var expectedResults = _admissionResidencyTypesCollection.FirstOrDefault(c => c.Guid == admissionResidencyTypesGuid);
            var actualResult =
                (await _admissionResidencyTypesService.GetAdmissionResidencyTypesAsync(true)).FirstOrDefault(x => x.Id == admissionResidencyTypesGuid);
            Assert.AreEqual(expectedResults.Guid, actualResult.Id);
            Assert.AreEqual(expectedResults.Description, actualResult.Title);
            //Assert.AreEqual(expectedResults.Code, actualResult.Code);
            
        }

        [TestMethod]
        [ExpectedException(typeof (KeyNotFoundException))]
        public async Task AdmissionResidencyTypesService_GetAdmissionResidencyTypesByGuidAsync_Empty()
        {
            await _admissionResidencyTypesService.GetAdmissionResidencyTypesByGuidAsync("");
        }

        [TestMethod]
        [ExpectedException(typeof (KeyNotFoundException))]
        public async Task AdmissionResidencyTypesService_GetAdmissionResidencyTypesByGuidAsync_Null()
        {
            await _admissionResidencyTypesService.GetAdmissionResidencyTypesByGuidAsync(null);
        }

        [TestMethod]
        [ExpectedException(typeof (KeyNotFoundException))]
        public async Task AdmissionResidencyTypesService_GetAdmissionResidencyTypesByGuidAsync_InvalidId()
        {
            _referenceRepositoryMock.Setup(repo => repo.GetAdmissionResidencyTypesAsync(It.IsAny<bool>()))
                .Throws<KeyNotFoundException>();

            await _admissionResidencyTypesService.GetAdmissionResidencyTypesByGuidAsync("99");
        }

        [TestMethod]
        public async Task AdmissionResidencyTypesService_GetAdmissionResidencyTypesByGuidAsync_Expected()
        {
            var expectedResults =
                _admissionResidencyTypesCollection.First(c => c.Guid == admissionResidencyTypesGuid);
            var actualResult =
                await _admissionResidencyTypesService.GetAdmissionResidencyTypesByGuidAsync(admissionResidencyTypesGuid);
            Assert.AreEqual(expectedResults.Guid, actualResult.Id);
            Assert.AreEqual(expectedResults.Description, actualResult.Title);
            //Assert.AreEqual(expectedResults.Code, actualResult.Code);
            
        }

        [TestMethod]
        public async Task AdmissionResidencyTypesService_GetAdmissionResidencyTypesByGuidAsync_Properties()
        {
            var result =
                await _admissionResidencyTypesService.GetAdmissionResidencyTypesByGuidAsync(admissionResidencyTypesGuid);
            Assert.IsNotNull(result.Id);
            //Assert.IsNotNull(result.Code);
            Assert.IsNull(result.Description);
            Assert.IsNotNull(result.Title);
            
        }
    }
}