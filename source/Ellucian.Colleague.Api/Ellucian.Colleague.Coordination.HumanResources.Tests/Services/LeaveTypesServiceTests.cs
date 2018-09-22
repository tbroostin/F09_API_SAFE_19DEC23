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

namespace Ellucian.Colleague.Coordination.HumanResources.Tests.Services
{    
    [TestClass]
    public class LeaveTypesServiceTests
    {
        private const string leaveTypesGuid = "7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc";
        private const string leaveTypesCode = "AT";
        private ICollection<LeaveType> _leaveTypesCollection;
        private LeaveTypesService _leaveTypesService;
        
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
           

            _leaveTypesCollection = new List<LeaveType>()
                {
                    new LeaveType("7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc", "AT", "Athletic"),
                    new LeaveType("849e6a7c-6cd4-4f98-8a73-ab0aa3627f0d", "AC", "Academic"),
                    new LeaveType("d2253ac7-9931-4560-b42f-1fccd43c952e", "CU", "Cultural")
                };

           
            _referenceRepositoryMock.Setup(repo => repo.GetLeaveTypesAsync(It.IsAny<bool>()))
                .ReturnsAsync(_leaveTypesCollection);

            _leaveTypesService = new LeaveTypesService(_referenceRepositoryMock.Object,
                _adapterRegistryMock.Object, _currentUserFactoryMock.Object,
                _roleRepositoryMock.Object, _configurationRepoMock.Object, _loggerMock.Object);
        }

        [TestCleanup]
        public void Cleanup()
        {
            _leaveTypesService = null;
            _leaveTypesCollection = null;
            _referenceRepositoryMock = null;
            _loggerMock = null;
            _currentUserFactoryMock= null;
            _roleRepositoryMock = null;
            _configurationRepoMock = null;
        }

        [TestMethod]
        public async Task LeaveTypesService_GetLeaveTypesAsync()
        {
            var results = await _leaveTypesService.GetLeaveTypesAsync(true);
            Assert.IsTrue(results is IEnumerable<LeaveTypes>);
            Assert.IsNotNull(results);
        }

        [TestMethod]
        public async Task LeaveTypesService_GetLeaveTypesAsync_Count()
        {
            var results = await _leaveTypesService.GetLeaveTypesAsync(true);
            Assert.AreEqual(3, results.Count());
        }

        [TestMethod]
        public async Task LeaveTypesService_GetLeaveTypesAsync_Properties()
        {
            var result =
                (await _leaveTypesService.GetLeaveTypesAsync(true)).FirstOrDefault(x => x.Code == leaveTypesCode);
            Assert.IsNotNull(result.Id);
            Assert.IsNotNull(result.Code);
            Assert.IsNull(result.Description);
           
        }

        [TestMethod]
        public async Task LeaveTypesService_GetLeaveTypesAsync_Expected()
        {
            var expectedResults = _leaveTypesCollection.FirstOrDefault(c => c.Guid == leaveTypesGuid);
            var actualResult =
                (await _leaveTypesService.GetLeaveTypesAsync(true)).FirstOrDefault(x => x.Id == leaveTypesGuid);
            Assert.AreEqual(expectedResults.Guid, actualResult.Id);
            Assert.AreEqual(expectedResults.Description, actualResult.Title);
            Assert.AreEqual(expectedResults.Code, actualResult.Code);
            
        }

        [TestMethod]
        [ExpectedException(typeof (KeyNotFoundException))]
        public async Task LeaveTypesService_GetLeaveTypesByGuidAsync_Empty()
        {
            await _leaveTypesService.GetLeaveTypesByGuidAsync("");
        }

        [TestMethod]
        [ExpectedException(typeof (KeyNotFoundException))]
        public async Task LeaveTypesService_GetLeaveTypesByGuidAsync_Null()
        {
            await _leaveTypesService.GetLeaveTypesByGuidAsync(null);
        }

        [TestMethod]
        [ExpectedException(typeof (KeyNotFoundException))]
        public async Task LeaveTypesService_GetLeaveTypesByGuidAsync_InvalidId()
        {
            _referenceRepositoryMock.Setup(repo => repo.GetLeaveTypesAsync(It.IsAny<bool>()))
                .Throws<KeyNotFoundException>();

            await _leaveTypesService.GetLeaveTypesByGuidAsync("99");
        }

        [TestMethod]
        public async Task LeaveTypesService_GetLeaveTypesByGuidAsync_Expected()
        {
            var expectedResults =
                _leaveTypesCollection.First(c => c.Guid == leaveTypesGuid);
            var actualResult =
                await _leaveTypesService.GetLeaveTypesByGuidAsync(leaveTypesGuid);
            Assert.AreEqual(expectedResults.Guid, actualResult.Id);
            Assert.AreEqual(expectedResults.Description, actualResult.Title);
            Assert.AreEqual(expectedResults.Code, actualResult.Code);
            
        }

        [TestMethod]
        public async Task LeaveTypesService_GetLeaveTypesByGuidAsync_Properties()
        {
            var result =
                await _leaveTypesService.GetLeaveTypesByGuidAsync(leaveTypesGuid);
            Assert.IsNotNull(result.Id);
            Assert.IsNotNull(result.Code);
            Assert.IsNull(result.Description);
            Assert.IsNotNull(result.Title);
            
        }
    }
}