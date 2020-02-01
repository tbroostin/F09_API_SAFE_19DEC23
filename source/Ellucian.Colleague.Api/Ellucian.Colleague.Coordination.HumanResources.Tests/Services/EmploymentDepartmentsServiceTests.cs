//Copyright 2017 Ellucian Company L.P. and its affiliates.


using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ellucian.Colleague.Coordination.HumanResources.Services;
using Ellucian.Colleague.Domain.HumanResources.Entities;
using Ellucian.Colleague.Domain.HumanResources.Repositories;
using Ellucian.Colleague.Dtos;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Web.Adapters;
using Ellucian.Web.Security;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Web.Http.Exceptions;

namespace Ellucian.Colleague.Coordination.HumanResources.Tests.Services
{    
    [TestClass]
    public class EmploymentDepartmentsServiceTests
    {
        private const string employmentDepartmentsGuid = "7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc";
        private const string employmentDepartmentsCode = "AT";
        private ICollection<EmploymentDepartment> _employmentDepartmentsCollection;
        private EmploymentDepartmentsService _employmentDepartmentsService;
        
        private Mock<IHumanResourcesReferenceDataRepository> _referenceRepositoryMock;
        private Mock<ILogger> _loggerMock;
        private Mock<IAdapterRegistry> _adapterRegistryMock;
        private Mock<ICurrentUserFactory> _currentUserFactoryMock;
        private Mock<IRoleRepository> _roleRepositoryMock;
        private Mock<IConfigurationRepository> _configurationRepoMock;
       

        [TestInitialize]
        public async void Initialize()
        {
            _referenceRepositoryMock = new Mock<IHumanResourcesReferenceDataRepository>();
           _adapterRegistryMock = new Mock<IAdapterRegistry>();
            _loggerMock = new Mock<ILogger>();
            _currentUserFactoryMock = new Mock<ICurrentUserFactory>();
            _roleRepositoryMock = new Mock<IRoleRepository>();
            _configurationRepoMock = new Mock<IConfigurationRepository>();
           

            _employmentDepartmentsCollection = new List<EmploymentDepartment>()
                {
                    new EmploymentDepartment("7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc", "AT", "Athletic"),
                    new EmploymentDepartment("849e6a7c-6cd4-4f98-8a73-ab0aa3627f0d", "AC", "Academic"),
                    new EmploymentDepartment("d2253ac7-9931-4560-b42f-1fccd43c952e", "CU", "Cultural")
                };

           
            _referenceRepositoryMock.Setup(repo => repo.GetEmploymentDepartmentsAsync(It.IsAny<bool>()))
                .ReturnsAsync(_employmentDepartmentsCollection);

            _employmentDepartmentsService = new EmploymentDepartmentsService(_referenceRepositoryMock.Object,
                _adapterRegistryMock.Object, _currentUserFactoryMock.Object,
                _roleRepositoryMock.Object, _configurationRepoMock.Object, _loggerMock.Object);
        }

        [TestCleanup]
        public void Cleanup()
        {
            _employmentDepartmentsService = null;
            _employmentDepartmentsCollection = null;
            _referenceRepositoryMock = null;
            _loggerMock = null;
            _currentUserFactoryMock= null;
            _roleRepositoryMock = null;
            _configurationRepoMock = null;
        }

        [TestMethod]
        public async Task EmploymentDepartmentsService_GetEmploymentDepartmentsAsync()
        {
            var results = await _employmentDepartmentsService.GetEmploymentDepartmentsAsync(true);
            Assert.IsTrue(results is IEnumerable<EmploymentDepartments>);
            Assert.IsNotNull(results);
        }

        [TestMethod]
        public async Task EmploymentDepartmentsService_GetEmploymentDepartmentsAsync_Count()
        {
            var results = await _employmentDepartmentsService.GetEmploymentDepartmentsAsync(true);
            Assert.AreEqual(3, results.Count());
        }

        [TestMethod]
        public async Task EmploymentDepartmentsService_GetEmploymentDepartmentsAsync_Properties()
        {
            var result =
                (await _employmentDepartmentsService.GetEmploymentDepartmentsAsync(true)).FirstOrDefault(x => x.Code == employmentDepartmentsCode);
            Assert.IsNotNull(result.Id);
            Assert.IsNotNull(result.Code);
            Assert.IsNull(result.Description);
           
        }

        [TestMethod]
        public async Task EmploymentDepartmentsService_GetEmploymentDepartmentsAsync_Expected()
        {
            var expectedResults = _employmentDepartmentsCollection.FirstOrDefault(c => c.Guid == employmentDepartmentsGuid);
            var actualResult =
                (await _employmentDepartmentsService.GetEmploymentDepartmentsAsync(true)).FirstOrDefault(x => x.Id == employmentDepartmentsGuid);
            Assert.AreEqual(expectedResults.Guid, actualResult.Id);
            Assert.AreEqual(expectedResults.Description, actualResult.Title);
            Assert.AreEqual(expectedResults.Code, actualResult.Code);
            
        }

        [TestMethod]
        [ExpectedException(typeof (IntegrationApiException))]
        public async Task EmploymentDepartmentsService_GetEmploymentDepartmentsByGuidAsync_Empty()
        {
            try
            {
                await _employmentDepartmentsService.GetEmploymentDepartmentsByGuidAsync("");
            }
            catch (IntegrationApiException ex)
            {
                Assert.IsNotNull(ex.Errors);
                Assert.AreEqual("keyNotFound", ex.Errors[0].Code);
                throw;
            }
        }

        [TestMethod]
        [ExpectedException(typeof (IntegrationApiException))]
        public async Task EmploymentDepartmentsService_GetEmploymentDepartmentsByGuidAsync_Null()
        {
            try
            {
                await _employmentDepartmentsService.GetEmploymentDepartmentsByGuidAsync(null);
            }
            catch (IntegrationApiException ex)
            {
                Assert.IsNotNull(ex.Errors);
                Assert.AreEqual("keyNotFound", ex.Errors[0].Code);
                throw;
            }
        }

        [TestMethod]
        [ExpectedException(typeof (IntegrationApiException))]
        public async Task EmploymentDepartmentsService_GetEmploymentDepartmentsByGuidAsync_InvalidId()
        {
            _referenceRepositoryMock.Setup(repo => repo.GetEmploymentDepartmentsAsync(It.IsAny<bool>()))
                .Throws<KeyNotFoundException>();

            try
            {
                await _employmentDepartmentsService.GetEmploymentDepartmentsByGuidAsync("99");
            }
            catch (IntegrationApiException ex)
            {
                Assert.IsNotNull(ex.Errors);
                Assert.AreEqual("keyNotFound", ex.Errors[0].Code);
                throw;
            }
        }

        [TestMethod]
        public async Task EmploymentDepartmentsService_GetEmploymentDepartmentsByGuidAsync_Expected()
        {
            var expectedResults =
                _employmentDepartmentsCollection.First(c => c.Guid == employmentDepartmentsGuid);
            var actualResult =
                await _employmentDepartmentsService.GetEmploymentDepartmentsByGuidAsync(employmentDepartmentsGuid);
            Assert.AreEqual(expectedResults.Guid, actualResult.Id);
            Assert.AreEqual(expectedResults.Description, actualResult.Title);
            Assert.AreEqual(expectedResults.Code, actualResult.Code);
            
        }

        [TestMethod]
        public async Task EmploymentDepartmentsService_GetEmploymentDepartmentsByGuidAsync_Properties()
        {
            var result =
                await _employmentDepartmentsService.GetEmploymentDepartmentsByGuidAsync(employmentDepartmentsGuid);
            Assert.IsNotNull(result.Id);
            Assert.IsNotNull(result.Code);
            Assert.IsNull(result.Description);
            Assert.IsNotNull(result.Title);
            
        }
    }
}