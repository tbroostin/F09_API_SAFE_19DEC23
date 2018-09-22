/* Copyright 2016 Ellucian Company L.P. and its affiliates. */

using Ellucian.Colleague.Coordination.HumanResources.Services;
using Ellucian.Colleague.Domain.HumanResources.Repositories;
using Ellucian.Colleague.Domain.HumanResources.Tests;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.HumanResources.Tests.Services
{

    [TestClass]
    public class GetEmploymentClassifications : HumanResourcesServiceTestsSetup
    {  
        private IEnumerable<Domain.HumanResources.Entities.EmploymentClassification> _allEmploymentClassifications;
        private Mock<IHumanResourcesReferenceDataRepository> _refRepoMock;
        private EmploymentClassificationService _employmentClassificationService;
        private const string DemographicGuid = "625c69ff-280b-4ed3-9474-662a43616a8a";

        [TestInitialize]
        public void Initialize()
        {
            MockInitialize();
            _allEmploymentClassifications = new TestEmploymentClassRepository().GetEmploymentClassifications();
            _refRepoMock = new Mock<IHumanResourcesReferenceDataRepository>();
            _employmentClassificationService = new EmploymentClassificationService(_refRepoMock.Object, adapterRegistryMock.Object,
                employeeCurrentUserFactory, roleRepositoryMock.Object, loggerMock.Object);
        }

        [TestCleanup]
        public void Cleanup()
        {
            _allEmploymentClassifications = null;
            _refRepoMock = null;
            _employmentClassificationService = null;
            adapterRegistryMock = null;
            roleRepositoryMock = null;
            loggerMock = null;
            employeeCurrentUserFactory = null;
        }

        [TestMethod]
        public async Task GetEmploymentClassifcationByGuid_HEDM_ValidEmploymentClassifcationIdAsync()
        {
            var thisEmploymentClassifcation = _allEmploymentClassifications.FirstOrDefault(m => m.Guid == DemographicGuid);
            _refRepoMock.Setup(repo => repo.GetEmploymentClassificationsAsync(true)).ReturnsAsync(_allEmploymentClassifications.Where(m => m.Guid == DemographicGuid));
            var employmentClassification = await _employmentClassificationService.GetEmploymentClassificationByGuidAsync(DemographicGuid);
            Assert.IsNotNull(thisEmploymentClassifcation);
            Assert.AreEqual(thisEmploymentClassifcation.Guid, employmentClassification.Id);
            Assert.AreEqual(thisEmploymentClassifcation.Code, employmentClassification.Code);
            Assert.AreEqual(null, employmentClassification.Description);
            Assert.AreEqual(thisEmploymentClassifcation.Description, employmentClassification.Title);
        }


        [TestMethod]
        public async Task GetEmploymentClassifications_HEDM_CountEmploymentClassificationsAsync()
        {
            _refRepoMock.Setup(repo => repo.GetEmploymentClassificationsAsync(false)).ReturnsAsync(_allEmploymentClassifications);
            var employmentClassification = await _employmentClassificationService.GetEmploymentClassificationsAsync();
            Assert.AreEqual(4, employmentClassification.Count());
        }

        [TestMethod]
        public async Task GetEmploymentClassifications_HEDM_CompareEmploymentClassificationsAsync()
        {
            _refRepoMock.Setup(repo => repo.GetEmploymentClassificationsAsync(false)).ReturnsAsync(_allEmploymentClassifications);

            var employmentClassifications = (await _employmentClassificationService.GetEmploymentClassificationsAsync()).ToList();
            Assert.IsNotNull(employmentClassifications);
            Assert.AreEqual(_allEmploymentClassifications.ElementAt(0).Guid, employmentClassifications.ElementAt(0).Id);
            Assert.AreEqual(_allEmploymentClassifications.ElementAt(0).Code, employmentClassifications.ElementAt(0).Code);
            Assert.AreEqual(null, employmentClassifications.ElementAt(0).Description);
            Assert.AreEqual(_allEmploymentClassifications.ElementAt(0).Description, employmentClassifications.ElementAt(0).Title);
        }

        [TestMethod]
        [ExpectedException(typeof (InvalidOperationException))]
        public async Task DemographicService_GetEmploymentClassificationByGuid_HEDM_ThrowsInvOpExc()
        {
            _refRepoMock.Setup(repo => repo.GetEmploymentClassificationsAsync(It.IsAny<bool>())).Throws<InvalidOperationException>();
            await _employmentClassificationService.GetEmploymentClassificationByGuidAsync("dshjfkj");
        }
    }
}

