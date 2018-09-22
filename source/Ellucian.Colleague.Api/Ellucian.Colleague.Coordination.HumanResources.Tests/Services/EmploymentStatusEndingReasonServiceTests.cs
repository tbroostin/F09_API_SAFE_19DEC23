/* Copyright 2016 Ellucian Company L.P. and its affiliates. */
using Ellucian.Colleague.Coordination.HumanResources.Services;
using Ellucian.Colleague.Domain.Base.Tests;
using Ellucian.Colleague.Domain.HumanResources.Repositories;
using Ellucian.Colleague.Domain.HumanResources.Tests;
using Ellucian.Colleague.Dtos;
using Ellucian.Web.Adapters;
using Ellucian.Web.Http.TestUtil;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.HumanResources.Tests.Services
{
    [TestClass]
    public class EmploymentStatusEndingReasonServiceTests : HumanResourcesServiceTestsSetup
    {
        public Mock<IHumanResourcesReferenceDataRepository> referenceDataRepository;
        EmploymentStatusEndingReasonService employmentStatusEndingReasonService;
        List<Dtos.EmploymentStatusEndingReason> employmentStatusEndingReasonDtoList = new List<Dtos.EmploymentStatusEndingReason>();
        List<Domain.HumanResources.Entities.EmploymentStatusEndingReason> employmentStatusEndingReasonEntityList = new List<Domain.HumanResources.Entities.EmploymentStatusEndingReason>();
        string id = "625c69ff-280b-4ed3-9474-662a43616a8a";

        [TestInitialize]
        public void Initialize()
        {
            MockInitialize();
            BuildData();
            referenceDataRepository = new Mock<IHumanResourcesReferenceDataRepository>();
            employmentStatusEndingReasonService = new EmploymentStatusEndingReasonService(referenceDataRepository.Object, adapterRegistryMock.Object,
                employeeCurrentUserFactory, roleRepositoryMock.Object, loggerMock.Object);
        }

        [TestCleanup]
        public void Cleanup()
        {
            referenceDataRepository = null;
            employmentStatusEndingReasonService = null;
            employmentStatusEndingReasonDtoList = null;
            employmentStatusEndingReasonEntityList = null;
        }

        [TestMethod]
        public async Task EmploymentStatusEndingReasonService_GetAll()
        {
            referenceDataRepository.Setup(i => i.GetEmploymentStatusEndingReasonsAsync(It.IsAny<bool>())).ReturnsAsync(employmentStatusEndingReasonEntityList);

            var actuals = await employmentStatusEndingReasonService.GetEmploymentStatusEndingReasonsAsync(It.IsAny<bool>());

            Assert.IsNotNull(actuals);

            foreach (var actual in actuals)
            {
                var expected = employmentStatusEndingReasonDtoList.FirstOrDefault(i => i.Id.Equals(actual.Id, StringComparison.OrdinalIgnoreCase));
                Assert.IsNotNull(expected);
                Assert.AreEqual(expected.Id, actual.Id);
                Assert.AreEqual(expected.Id, actual.Id);
                Assert.AreEqual(expected.Id, actual.Id);
                Assert.IsNull(actual.Description);
            }
        }

        [TestMethod]
        public async Task EmploymentStatusEndingReasonService_GetAll_True()
        {
            referenceDataRepository.Setup(i => i.GetEmploymentStatusEndingReasonsAsync(true)).ReturnsAsync(employmentStatusEndingReasonEntityList);

            var actuals = await employmentStatusEndingReasonService.GetEmploymentStatusEndingReasonsAsync(true);

            Assert.IsNotNull(actuals);

            foreach (var actual in actuals)
            {
                var expected = employmentStatusEndingReasonDtoList.FirstOrDefault(i => i.Id.Equals(actual.Id, StringComparison.OrdinalIgnoreCase));
                Assert.IsNotNull(expected);
                Assert.AreEqual(expected.Id, actual.Id);
                Assert.AreEqual(expected.Code, actual.Code);
                Assert.AreEqual(expected.Title, actual.Title);
                Assert.IsNull(actual.Description);
            }
        }

        [TestMethod]
        public async Task EmploymentStatusEndingReasonService_GetById()
        {
            var expected = employmentStatusEndingReasonDtoList.FirstOrDefault(i => i.Id.Equals(id, StringComparison.OrdinalIgnoreCase));
            referenceDataRepository.Setup(i => i.GetEmploymentStatusEndingReasonsAsync(true)).ReturnsAsync(employmentStatusEndingReasonEntityList);

            var actual = await employmentStatusEndingReasonService.GetEmploymentStatusEndingReasonByIdAsync(id);

            Assert.IsNotNull(actual);
            Assert.AreEqual(expected.Id, actual.Id);
            Assert.AreEqual(expected.Code, actual.Code);
            Assert.AreEqual(expected.Title, actual.Title);
            Assert.IsNull(actual.Description);
        }

        [TestMethod]
        [ExpectedException(typeof(KeyNotFoundException))]
        public async Task EmploymentStatusEndingReasonService_GetById_KeyNotFoundException()
        {
            referenceDataRepository.Setup(i => i.GetEmploymentStatusEndingReasonsAsync(true)).ReturnsAsync(employmentStatusEndingReasonEntityList);
            var actual = await employmentStatusEndingReasonService.GetEmploymentStatusEndingReasonByIdAsync(It.IsAny<string>());
        }

        [TestMethod]
        [ExpectedException(typeof(Exception))]
        public async Task EmploymentStatusEndingReasonService_GetById_Exception()
        {
            referenceDataRepository.Setup(i => i.GetEmploymentStatusEndingReasonsAsync(true)).ThrowsAsync(new Exception());
            var actual = await employmentStatusEndingReasonService.GetEmploymentStatusEndingReasonByIdAsync(It.IsAny<string>());
        }

        private void BuildData()
        {
            employmentStatusEndingReasonEntityList = new List<Domain.HumanResources.Entities.EmploymentStatusEndingReason>() 
            {
                new Domain.HumanResources.Entities.EmploymentStatusEndingReason("625c69ff-280b-4ed3-9474-662a43616a8a", "Term", "Termination"),
                new Domain.HumanResources.Entities.EmploymentStatusEndingReason("bfea651b-8e27-4fcd-abe3-04573443c04c", "LOA", "Leave of Absence"),
                new Domain.HumanResources.Entities.EmploymentStatusEndingReason("9ae3a175-1dfd-4937-b97b-3c9ad596e023", "DEM", "Demotion"),
                new Domain.HumanResources.Entities.EmploymentStatusEndingReason("e9e6837f-2c51-431b-9069-4ac4c0da3041", "RET", "Retired"),
                new Domain.HumanResources.Entities.EmploymentStatusEndingReason("80779c4f-b2ac-4ad4-a970-ca5699d9891f", "EOC", "End of Contract"),
                new Domain.HumanResources.Entities.EmploymentStatusEndingReason("ae21110e-991e-405e-9d8b-47eeff210a2d", "EEO", "Change EEO Information"),
            };
            foreach (var entity in employmentStatusEndingReasonEntityList)
            {
                employmentStatusEndingReasonDtoList.Add(new Dtos.EmploymentStatusEndingReason()
                {
                    Id = entity.Guid,
                    Code = entity.Code,
                    Title = entity.Description,
                    Description = null
                });
            }
        }
    }
}
