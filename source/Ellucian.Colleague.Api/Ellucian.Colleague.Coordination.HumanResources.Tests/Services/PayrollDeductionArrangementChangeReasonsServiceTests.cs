/* Copyright 2016-2020 Ellucian Company L.P. and its affiliates. */
using Ellucian.Colleague.Coordination.HumanResources.Services;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.HumanResources.Repositories;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ellucian.Web.Http.Exceptions;

namespace Ellucian.Colleague.Coordination.HumanResources.Tests.Services
{
    [TestClass]
    public class PayrollDeductionArrangementChangeReasonsServiceTests : HumanResourcesServiceTestsSetup
    {
        private IConfigurationRepository _configurationRepository;
        private Mock<IConfigurationRepository> _configurationRepositoryMock;
        public Mock<IHumanResourcesReferenceDataRepository> referenceDataRepository;
        PayrollDeductionArrangementChangeReasonsService payrollDeductionArrangementChangeReasonsService;
        List<Dtos.PayrollDeductionArrangementChangeReason> payrollDeductionArrangementChangeReasonDtoList = new List<Dtos.PayrollDeductionArrangementChangeReason>();
        List<Domain.HumanResources.Entities.PayrollDeductionArrangementChangeReason> payrollDeductionArrangementChangeReasonEntityList = new List<Domain.HumanResources.Entities.PayrollDeductionArrangementChangeReason>();
        string id = "625c69ff-280b-4ed3-9474-662a43616a8a";

        [TestInitialize]
        public void Initialize()
        {
            _configurationRepositoryMock = new Mock<IConfigurationRepository>();
            _configurationRepository = _configurationRepositoryMock.Object;
            MockInitialize();
            BuildData();
            referenceDataRepository = new Mock<IHumanResourcesReferenceDataRepository>();
            payrollDeductionArrangementChangeReasonsService = new PayrollDeductionArrangementChangeReasonsService(referenceDataRepository.Object, adapterRegistryMock.Object,
                employeeCurrentUserFactory, _configurationRepository, roleRepositoryMock.Object, loggerMock.Object);
        }

        [TestCleanup]
        public void Cleanup()
        {
            _configurationRepository = null;
            _configurationRepositoryMock = null;
            referenceDataRepository = null;
            payrollDeductionArrangementChangeReasonsService = null;
            payrollDeductionArrangementChangeReasonDtoList = null;
            payrollDeductionArrangementChangeReasonEntityList = null;
        }

        [TestMethod]
        public async Task PayrollDeductionArrangementChangeReason_GetAll()
        {
            referenceDataRepository.Setup(i => i.GetPayrollDeductionArrangementChangeReasonsAsync(It.IsAny<bool>())).ReturnsAsync(payrollDeductionArrangementChangeReasonEntityList);

            var actuals = await payrollDeductionArrangementChangeReasonsService.GetPayrollDeductionArrangementChangeReasonsAsync(It.IsAny<bool>());

            Assert.IsNotNull(actuals);

            foreach (var actual in actuals)
            {
                var expected = payrollDeductionArrangementChangeReasonDtoList.FirstOrDefault(i => i.Id.Equals(actual.Id, StringComparison.OrdinalIgnoreCase));
                Assert.IsNotNull(expected);
                Assert.AreEqual(expected.Id, actual.Id);
                Assert.AreEqual(expected.Id, actual.Id);
                Assert.AreEqual(expected.Id, actual.Id);
                Assert.IsNull(actual.Description);
            }
        }

        [TestMethod]
        public async Task PayrollDeductionArrangementChangeReason_GetAll_True()
        {
            referenceDataRepository.Setup(i => i.GetPayrollDeductionArrangementChangeReasonsAsync(true)).ReturnsAsync(payrollDeductionArrangementChangeReasonEntityList);

            var actuals = await payrollDeductionArrangementChangeReasonsService.GetPayrollDeductionArrangementChangeReasonsAsync(true);

            Assert.IsNotNull(actuals);

            foreach (var actual in actuals)
            {
                var expected = payrollDeductionArrangementChangeReasonDtoList.FirstOrDefault(i => i.Id.Equals(actual.Id, StringComparison.OrdinalIgnoreCase));
                Assert.IsNotNull(expected);
                Assert.AreEqual(expected.Id, actual.Id);
                Assert.AreEqual(expected.Code, actual.Code);
                Assert.AreEqual(expected.Title, actual.Title);
                Assert.IsNull(actual.Description);
            }
        }

        [TestMethod]
        public async Task PayrollDeductionArrangementChangeReason_GetById()
        {
            var expected = payrollDeductionArrangementChangeReasonDtoList.FirstOrDefault(i => i.Id.Equals(id, StringComparison.OrdinalIgnoreCase));
            referenceDataRepository.Setup(i => i.GetPayrollDeductionArrangementChangeReasonsAsync(It.IsAny<bool>())).ReturnsAsync(payrollDeductionArrangementChangeReasonEntityList);

            var actual = await payrollDeductionArrangementChangeReasonsService.GetPayrollDeductionArrangementChangeReasonByIdAsync(id);

            Assert.IsNotNull(actual);
            Assert.AreEqual(expected.Id, actual.Id);
            Assert.AreEqual(expected.Code, actual.Code);
            Assert.AreEqual(expected.Title, actual.Title);
            Assert.IsNull(actual.Description);
        }

        [TestMethod]
        [ExpectedException(typeof(KeyNotFoundException))]
        public async Task PayrollDeductionArrangementChangeReason_GetById_KeyNotFoundException()
        {
            referenceDataRepository.Setup(i => i.GetPayrollDeductionArrangementChangeReasonsAsync(It.IsAny<bool>())).ReturnsAsync(payrollDeductionArrangementChangeReasonEntityList);
            var actual = await payrollDeductionArrangementChangeReasonsService.GetPayrollDeductionArrangementChangeReasonByIdAsync(It.IsAny<string>());
        }

        [TestMethod]
        [ExpectedException(typeof(ColleagueWebApiException))]
        public async Task PayrollDeductionArrangementChangeReason_GetById_Exception()
        {
            referenceDataRepository.Setup(i => i.GetPayrollDeductionArrangementChangeReasonsAsync(true)).ThrowsAsync(new Exception());
            var actual = await payrollDeductionArrangementChangeReasonsService.GetPayrollDeductionArrangementChangeReasonByIdAsync(It.IsAny<string>());
        }

        private void BuildData()
        {
            payrollDeductionArrangementChangeReasonEntityList = new List<Domain.HumanResources.Entities.PayrollDeductionArrangementChangeReason>() 
            {
                new Domain.HumanResources.Entities.PayrollDeductionArrangementChangeReason("625c69ff-280b-4ed3-9474-662a43616a8a", "MAR", "Marriage"),
                new Domain.HumanResources.Entities.PayrollDeductionArrangementChangeReason("bfea651b-8e27-4fcd-abe3-04573443c04c", "BOC", "Birth of Child"),
                new Domain.HumanResources.Entities.PayrollDeductionArrangementChangeReason("9ae3a175-1dfd-4937-b97b-3c9ad596e023", "SJC", "Spouse Job Change"),
                new Domain.HumanResources.Entities.PayrollDeductionArrangementChangeReason("e9e6837f-2c51-431b-9069-4ac4c0da3041", "DIV", "Divorce"),
                new Domain.HumanResources.Entities.PayrollDeductionArrangementChangeReason("80779c4f-b2ac-4ad4-a970-ca5699d9891f", "ADP", "Adoption"),
                new Domain.HumanResources.Entities.PayrollDeductionArrangementChangeReason("ae21110e-991e-405e-9d8b-47eeff210a2d", "DEA", "Death"),
            };
            foreach (var entity in payrollDeductionArrangementChangeReasonEntityList)
            {
                payrollDeductionArrangementChangeReasonDtoList.Add(new Dtos.PayrollDeductionArrangementChangeReason()
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
