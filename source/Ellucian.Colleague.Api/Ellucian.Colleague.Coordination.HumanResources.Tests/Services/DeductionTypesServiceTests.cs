/* Copyright 2016 Ellucian Company L.P. and its affiliates. */
using Ellucian.Colleague.Coordination.HumanResources.Services;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.Base.Tests;
using Ellucian.Colleague.Domain.HumanResources.Entities;
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
    public class DeductionTypesServiceTests : HumanResourcesServiceTestsSetup
    {
        public Mock<IHumanResourcesReferenceDataRepository> referenceDataRepository;
        public Mock<IConfigurationRepository> configurationRepository;
        DeductionTypesService deductionTypesService;
        List<Dtos.DeductionType> deductionTypeDtoList = new List<Dtos.DeductionType>();
        List<Domain.HumanResources.Entities.DeductionType> deductionTypeEntityList = new List<Domain.HumanResources.Entities.DeductionType>();
        string id = "625c69ff-280b-4ed3-9474-662a43616a8a";

        [TestInitialize]
        public void Initialize()
        {
            MockInitialize();
            BuildData();
            referenceDataRepository = new Mock<IHumanResourcesReferenceDataRepository>();
            configurationRepository = new Mock<IConfigurationRepository>();
            deductionTypesService = new DeductionTypesService(referenceDataRepository.Object, adapterRegistryMock.Object,
                employeeCurrentUserFactory, roleRepositoryMock.Object, loggerMock.Object, configurationRepository.Object);
        }

        [TestCleanup]
        public void Cleanup()
        {
            referenceDataRepository = null;
            deductionTypesService = null;
            deductionTypeDtoList = null;
            deductionTypeEntityList = null;
        }

        [TestMethod]
        public async Task DeductionType_GetAll()
        {
            referenceDataRepository.Setup(i => i.GetDeductionTypesAsync(It.IsAny<bool>())).ReturnsAsync(deductionTypeEntityList);

            var actuals = await deductionTypesService.GetDeductionTypesAsync(It.IsAny<bool>());

            Assert.IsNotNull(actuals);

            foreach (var actual in actuals)
            {
                var expected = deductionTypeDtoList.FirstOrDefault(i => i.Id.Equals(actual.Id, StringComparison.OrdinalIgnoreCase));
                Assert.IsNotNull(expected);
                Assert.AreEqual(expected.Id, actual.Id);
                Assert.AreEqual(expected.Id, actual.Id);
                Assert.AreEqual(expected.Id, actual.Id);
                Assert.IsNull(actual.Description);
            }
        }

        [TestMethod]
        public async Task DeductionType_GetAll_True()
        {
            referenceDataRepository.Setup(i => i.GetDeductionTypesAsync(true)).ReturnsAsync(deductionTypeEntityList);

            var actuals = await deductionTypesService.GetDeductionTypesAsync(true);

            Assert.IsNotNull(actuals);

            foreach (var actual in actuals)
            {
                var expected = deductionTypeDtoList.FirstOrDefault(i => i.Id.Equals(actual.Id, StringComparison.OrdinalIgnoreCase));
                Assert.IsNotNull(expected);
                Assert.AreEqual(expected.Id, actual.Id);
                Assert.AreEqual(expected.Code, actual.Code);
                Assert.AreEqual(expected.Title, actual.Title);
                Assert.IsNull(actual.Description);
            }
        }

        [TestMethod]
        public async Task DeductionType_GetById()
        {
            var expected = deductionTypeDtoList.FirstOrDefault(i => i.Id.Equals(id, StringComparison.OrdinalIgnoreCase));
            referenceDataRepository.Setup(i => i.GetDeductionTypesAsync(It.IsAny<bool>())).ReturnsAsync(deductionTypeEntityList);

            var actual = await deductionTypesService.GetDeductionTypeByIdAsync(id);

            Assert.IsNotNull(actual);
            Assert.AreEqual(expected.Id, actual.Id);
            Assert.AreEqual(expected.Code, actual.Code);
            Assert.AreEqual(expected.Title, actual.Title);
            Assert.IsNull(actual.Description);
        }

        [TestMethod]
        [ExpectedException(typeof(KeyNotFoundException))]
        public async Task DeductionType_GetById_KeyNotFoundException()
        {
            referenceDataRepository.Setup(i => i.GetDeductionTypesAsync(It.IsAny<bool>())).ReturnsAsync(deductionTypeEntityList);
            var actual = await deductionTypesService.GetDeductionTypeByIdAsync(It.IsAny<string>());
        }

        [TestMethod]
        [ExpectedException(typeof(Exception))]
        public async Task DeductionType_GetById_Exception()
        {
            referenceDataRepository.Setup(i => i.GetDeductionTypesAsync(true)).ThrowsAsync(new Exception());
            var actual = await deductionTypesService.GetDeductionTypeByIdAsync(It.IsAny<string>());
        }

        private void BuildData()
        {
            deductionTypeEntityList = new List<Domain.HumanResources.Entities.DeductionType>() 
            {
                new Domain.HumanResources.Entities.DeductionType("625c69ff-280b-4ed3-9474-662a43616a8a", "MAR", "Marriage"),
                new Domain.HumanResources.Entities.DeductionType("bfea651b-8e27-4fcd-abe3-04573443c04c", "BOC", "Birth of Child"),
                new Domain.HumanResources.Entities.DeductionType("9ae3a175-1dfd-4937-b97b-3c9ad596e023", "SJC", "Spouse Job Change"),
                new Domain.HumanResources.Entities.DeductionType("e9e6837f-2c51-431b-9069-4ac4c0da3041", "DIV", "Divorce"),
                new Domain.HumanResources.Entities.DeductionType("80779c4f-b2ac-4ad4-a970-ca5699d9891f", "ADP", "Adoption"),
                new Domain.HumanResources.Entities.DeductionType("ae21110e-991e-405e-9d8b-47eeff210a2d", "DEA", "Death"),
            };
            foreach (var entity in deductionTypeEntityList)
            {
                deductionTypeDtoList.Add(new Dtos.DeductionType()
                {
                    Id = entity.Guid,
                    Code = entity.Code,
                    Title = entity.Description,
                    Description = null
                });
            }
        }
    }

    [TestClass]
    public class DeductionTypes2ServiceTests : HumanResourcesServiceTestsSetup
    {
        public Mock<IHumanResourcesReferenceDataRepository> referenceDataRepository;
        public Mock<IConfigurationRepository> configurationRepository;
        DeductionTypesService deductionTypesService;
        List<Dtos.DeductionType2> deductionTypeDtoList = new List<Dtos.DeductionType2>();
        List<Domain.HumanResources.Entities.DeductionType> deductionTypeEntityList = new List<Domain.HumanResources.Entities.DeductionType>();
        string id = "625c69ff-280b-4ed3-9474-662a43616a8a";

        [TestInitialize]
        public void Initialize()
        {
            MockInitialize();
            BuildData();
            referenceDataRepository = new Mock<IHumanResourcesReferenceDataRepository>();
            configurationRepository = new Mock<IConfigurationRepository>();
            deductionTypesService = new DeductionTypesService(referenceDataRepository.Object, adapterRegistryMock.Object,
                employeeCurrentUserFactory, roleRepositoryMock.Object, loggerMock.Object, configurationRepository.Object);
        }

        [TestCleanup]
        public void Cleanup()
        {
            referenceDataRepository = null;
            deductionTypesService = null;
            deductionTypeDtoList = null;
            deductionTypeEntityList = null;
        }

        [TestMethod]
        public async Task DeductionType_GetAll()
        {
            referenceDataRepository.Setup(i => i.GetDeductionTypes2Async(It.IsAny<bool>())).ReturnsAsync(deductionTypeEntityList);
            referenceDataRepository.Setup(i => i.GetCostCalculationMethodsAsync(It.IsAny<bool>())).ReturnsAsync(new List<CostCalculationMethod>() { new CostCalculationMethod("guid", "ccm", "desc") });

            var actuals = await deductionTypesService.GetDeductionTypes2Async(It.IsAny<bool>());

            Assert.IsNotNull(actuals);

            foreach (var actual in actuals)
            {
                var expected = deductionTypeDtoList.FirstOrDefault(i => i.Id.Equals(actual.Id, StringComparison.OrdinalIgnoreCase));
                Assert.IsNotNull(expected);
                Assert.AreEqual(expected.Id, actual.Id);
                Assert.AreEqual(expected.Id, actual.Id);
                Assert.AreEqual(expected.Id, actual.Id);
                Assert.IsNull(actual.Description);
            }
        }

        [TestMethod]
        public async Task DeductionType_GetAll_True()
        {
            referenceDataRepository.Setup(i => i.GetDeductionTypes2Async(true)).ReturnsAsync(deductionTypeEntityList);
            referenceDataRepository.Setup(i => i.GetCostCalculationMethodsAsync(It.IsAny<bool>())).ReturnsAsync(new List<CostCalculationMethod>() { new CostCalculationMethod("guid", "ccm", "desc") });

            var actuals = await deductionTypesService.GetDeductionTypes2Async(true);

            Assert.IsNotNull(actuals);

            foreach (var actual in actuals)
            {
                var expected = deductionTypeDtoList.FirstOrDefault(i => i.Id.Equals(actual.Id, StringComparison.OrdinalIgnoreCase));
                Assert.IsNotNull(expected);
                Assert.AreEqual(expected.Id, actual.Id);
                Assert.AreEqual(expected.Code, actual.Code);
                Assert.AreEqual(expected.Title, actual.Title);
                Assert.IsNull(actual.Description);
            }
        }

        [TestMethod]
        public async Task DeductionType_GetById()
        {
            var expected = deductionTypeDtoList.FirstOrDefault(i => i.Id.Equals(id, StringComparison.OrdinalIgnoreCase));
            referenceDataRepository.Setup(i => i.GetDeductionTypes2Async(It.IsAny<bool>())).ReturnsAsync(deductionTypeEntityList);
            referenceDataRepository.Setup(i => i.GetCostCalculationMethodsAsync(It.IsAny<bool>())).ReturnsAsync(new List<CostCalculationMethod>() { new CostCalculationMethod("guid", "ccm", "desc") });

            var actual = await deductionTypesService.GetDeductionTypeById2Async(id);

            Assert.IsNotNull(actual);
            Assert.AreEqual(expected.Id, actual.Id);
            Assert.AreEqual(expected.Code, actual.Code);
            Assert.AreEqual(expected.Title, actual.Title);
            Assert.IsNull(actual.Description);
        }

        [TestMethod]
        [ExpectedException(typeof(KeyNotFoundException))]
        public async Task DeductionType_GetById_KeyNotFoundException()
        {
            referenceDataRepository.Setup(i => i.GetDeductionTypes2Async(It.IsAny<bool>())).ReturnsAsync(deductionTypeEntityList);
            var actual = await deductionTypesService.GetDeductionTypeById2Async(It.IsAny<string>());
        }

        [TestMethod]
        [ExpectedException(typeof(Exception))]
        public async Task DeductionType_GetById_Exception()
        {
            referenceDataRepository.Setup(i => i.GetDeductionTypes2Async(true)).ThrowsAsync(new Exception());
            var actual = await deductionTypesService.GetDeductionTypeById2Async(It.IsAny<string>());
        }

        private void BuildData()
        {
            deductionTypeEntityList = new List<Domain.HumanResources.Entities.DeductionType>() 
            {
                new Domain.HumanResources.Entities.DeductionType("625c69ff-280b-4ed3-9474-662a43616a8a", "MAR", "Marriage", "cat", "ccm", 4, new List<string>(), new List<string>()),
                new Domain.HumanResources.Entities.DeductionType("bfea651b-8e27-4fcd-abe3-04573443c04c", "BOC", "Birth of Child", "cat", "ccm", 4, new List<string>(), new List<string>()),
                new Domain.HumanResources.Entities.DeductionType("9ae3a175-1dfd-4937-b97b-3c9ad596e023", "SJC", "Spouse Job Change", "cat", "ccm", 4, new List<string>(), new List<string>()),
                new Domain.HumanResources.Entities.DeductionType("e9e6837f-2c51-431b-9069-4ac4c0da3041", "DIV", "Divorce", "cat", "ccm", 4, new List<string>(), new List<string>()),
                new Domain.HumanResources.Entities.DeductionType("80779c4f-b2ac-4ad4-a970-ca5699d9891f", "ADP", "Adoption", "cat", "ccm", 4, new List<string>(), new List<string>()),
                new Domain.HumanResources.Entities.DeductionType("ae21110e-991e-405e-9d8b-47eeff210a2d", "DEA", "Death", "cat", "ccm", 4, new List<string>(), new List<string>()),
            };
            foreach (var entity in deductionTypeEntityList)
            {
                deductionTypeDtoList.Add(new Dtos.DeductionType2()
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
