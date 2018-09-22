//Copyright 2017 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ellucian.Colleague.Coordination.ColleagueFinance.Services;
using Ellucian.Colleague.Domain.ColleagueFinance.Entities;
using Ellucian.Colleague.Domain.ColleagueFinance.Repositories;
using Ellucian.Colleague.Dtos;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Web.Adapters;
using Ellucian.Web.Security;
using Ellucian.Colleague.Domain.Repositories;

namespace Ellucian.Colleague.Coordination.ColleagueFinance.Tests.Services
{
    [TestClass]
    public class FiscalYearsServiceTests
    {
        private const string fiscalYearsGuid = "7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc";
        private const string fiscalYearsCode = "2017";
        private ICollection<FiscalYear> _fiscalYearsCollection;
        private FiscalYearsService _fiscalYearsService;

        private Mock<IColleagueFinanceReferenceDataRepository> _referenceRepositoryMock;
        private Mock<ILogger> _loggerMock;
        private Mock<IAdapterRegistry> _adapterRegistryMock;
        private Mock<ICurrentUserFactory> _currentUserFactoryMock;
        private Mock<IRoleRepository> _roleRepositoryMock;
        private Mock<IConfigurationRepository> _configurationRepoMock;


        [TestInitialize]
        public async void Initialize()
        {
            _referenceRepositoryMock = new Mock<IColleagueFinanceReferenceDataRepository>();
            _adapterRegistryMock = new Mock<IAdapterRegistry>();
            _loggerMock = new Mock<ILogger>();
            _currentUserFactoryMock = new Mock<ICurrentUserFactory>();
            _roleRepositoryMock = new Mock<IRoleRepository>();
            _configurationRepoMock = new Mock<IConfigurationRepository>();


            _fiscalYearsCollection = new List<Domain.ColleagueFinance.Entities.FiscalYear>()
            {
                    new Domain.ColleagueFinance.Entities.FiscalYear("7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc", "2015")
                    { Title = "Fiscal Year: 2015", Status = "O", InstitutionName = "Ellucian University",
                    FiscalStartMonth = 7 },
                    new Domain.ColleagueFinance.Entities.FiscalYear("849e6a7c-6cd4-4f98-8a73-ab0aa3627f0d", "2016")
                    { Title = "Fiscal Year: 2016", Status = "O", InstitutionName = "Ellucian University",
                    FiscalStartMonth = 7},
                    new Domain.ColleagueFinance.Entities.FiscalYear("d2253ac7-9931-4560-b42f-1fccd43c952e", "2017")
                    { Title = "Fiscal Year: 2017", Status = "O", InstitutionName = "Ellucian University",
                    FiscalStartMonth = 7}
            };


            _referenceRepositoryMock.Setup(repo => repo.GetFiscalYearsAsync(It.IsAny<bool>()))
                .ReturnsAsync(_fiscalYearsCollection);

            _fiscalYearsService = new FiscalYearsService(_referenceRepositoryMock.Object,
                _adapterRegistryMock.Object, _currentUserFactoryMock.Object,
                _roleRepositoryMock.Object, _configurationRepoMock.Object, _loggerMock.Object);
        }

        [TestCleanup]
        public void Cleanup()
        {
            _fiscalYearsService = null;
            _fiscalYearsCollection = null;
            _referenceRepositoryMock = null;
            _loggerMock = null;
            _currentUserFactoryMock = null;
            _roleRepositoryMock = null;
            _configurationRepoMock = null;
        }

        [TestMethod]
        public async Task FiscalYearsService_GetFiscalYearsAsync()
        {
            var results = await _fiscalYearsService.GetFiscalYearsAsync(true);
            Assert.IsTrue(results is IEnumerable<FiscalYears>);
            Assert.IsNotNull(results);
        }

        [TestMethod]
        public async Task FiscalYearsService_GetFiscalYearsAsync_Count()
        {
            var results = await _fiscalYearsService.GetFiscalYearsAsync(true);
            Assert.AreEqual(3, results.Count());
        }

        [TestMethod]
        public async Task FiscalYearsService_GetFiscalYearsAsync_Properties()
        {
            var result =
                (await _fiscalYearsService.GetFiscalYearsAsync(true)).FirstOrDefault(x => x.Id == fiscalYearsGuid);
            Assert.IsNotNull(result.Id);
            Assert.IsNotNull(result.Title);
            Assert.IsNotNull(result.Status);
            Assert.IsNotNull(result.NumberOfPeriods);
            Assert.IsNotNull(result.StartOn);
            Assert.IsNotNull(result.EndOn);
            Assert.IsNotNull(result.YearEndAdjustment);
            Assert.IsNotNull(result.ReportingSegment);
        }

        [TestMethod]
        public async Task FiscalYearsService_GetFiscalYearsAsync_Expected()
        {
            var expectedResults = _fiscalYearsCollection.FirstOrDefault(c => c.Guid == fiscalYearsGuid);
            var actualResult =
                (await _fiscalYearsService.GetFiscalYearsAsync(true)).FirstOrDefault(x => x.Id == fiscalYearsGuid);
            Assert.AreEqual(expectedResults.Guid, actualResult.Id);
            Assert.AreEqual(expectedResults.Title, actualResult.Title);
            Assert.AreEqual(Dtos.EnumProperties.FiscalPeriodsStatus.Open, actualResult.Status);
            Assert.AreEqual(Dtos.EnumProperties.FiscalYearsYearEndAdjustment.Inactive, actualResult.YearEndAdjustment);
            Assert.AreEqual(12, actualResult.NumberOfPeriods);
            Assert.AreEqual(new DateTime(2014,7,1), actualResult.StartOn);
            Assert.AreEqual(new DateTime(2015, 6, 30), actualResult.EndOn);
            Assert.AreEqual(expectedResults.InstitutionName, actualResult.ReportingSegment);
        }


        [TestMethod]
        public async Task FiscalYearsService_GetFiscalYearsAsync_Filter_Expected()
        {
            var expectedResults = _fiscalYearsCollection.FirstOrDefault(c => c.Guid == fiscalYearsGuid);
            var actualResult =
                (await _fiscalYearsService.GetFiscalYearsAsync(true, "Ellucian University")).FirstOrDefault(x => x.Id == fiscalYearsGuid);
            Assert.AreEqual(expectedResults.Guid, actualResult.Id);
            Assert.AreEqual(expectedResults.Title, actualResult.Title);
            Assert.AreEqual(Dtos.EnumProperties.FiscalPeriodsStatus.Open, actualResult.Status);
            Assert.AreEqual(Dtos.EnumProperties.FiscalYearsYearEndAdjustment.Inactive, actualResult.YearEndAdjustment);
            Assert.AreEqual(12, actualResult.NumberOfPeriods);
            Assert.AreEqual(new DateTime(2014, 7, 1), actualResult.StartOn);
            Assert.AreEqual(new DateTime(2015, 6, 30), actualResult.EndOn);
            Assert.AreEqual(expectedResults.InstitutionName, actualResult.ReportingSegment);
        }


        [TestMethod]
        public async Task FiscalYearsService_GetFiscalYearsAsync_StatusClosed_Expected()
        {
            _fiscalYearsCollection = new List<Domain.ColleagueFinance.Entities.FiscalYear>()
            {
                    new Domain.ColleagueFinance.Entities.FiscalYear("7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc", "2015")
                    { Title = "Fiscal Year: 2015", Status = "C", InstitutionName = "Ellucian University",
                    FiscalStartMonth = 7 },
            };

            _referenceRepositoryMock.Setup(repo => repo.GetFiscalYearsAsync(It.IsAny<bool>()))
                .ReturnsAsync(_fiscalYearsCollection);

            var expectedResults = _fiscalYearsCollection.FirstOrDefault(c => c.Guid == fiscalYearsGuid);
            var actualResult =
                (await _fiscalYearsService.GetFiscalYearsAsync(true)).FirstOrDefault(x => x.Id == fiscalYearsGuid);
            Assert.AreEqual(expectedResults.Guid, actualResult.Id);
            Assert.AreEqual(expectedResults.Title, actualResult.Title);
            Assert.AreEqual(Dtos.EnumProperties.FiscalPeriodsStatus.Closed, actualResult.Status);
            Assert.AreEqual(Dtos.EnumProperties.FiscalYearsYearEndAdjustment.Inactive, actualResult.YearEndAdjustment);
            Assert.AreEqual(12, actualResult.NumberOfPeriods);
            Assert.AreEqual(new DateTime(2014, 7, 1), actualResult.StartOn);
            Assert.AreEqual(new DateTime(2015, 6, 30), actualResult.EndOn);
            Assert.AreEqual(expectedResults.InstitutionName, actualResult.ReportingSegment);
        }


        [TestMethod]
        public async Task FiscalYearsService_GetFiscalYearsAsync_StatusYearEnd_Expected()
        {
            _fiscalYearsCollection = new List<Domain.ColleagueFinance.Entities.FiscalYear>()
            {
                    new Domain.ColleagueFinance.Entities.FiscalYear("7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc", "2015")
                    { Title = "Fiscal Year: 2015", Status = "Y", InstitutionName = "Ellucian University",
                    FiscalStartMonth = 7 },
            };

            _referenceRepositoryMock.Setup(repo => repo.GetFiscalYearsAsync(It.IsAny<bool>()))
                .ReturnsAsync(_fiscalYearsCollection);

            var expectedResults = _fiscalYearsCollection.FirstOrDefault(c => c.Guid == fiscalYearsGuid);
            var actualResult =
                (await _fiscalYearsService.GetFiscalYearsAsync(true)).FirstOrDefault(x => x.Id == fiscalYearsGuid);
            Assert.AreEqual(expectedResults.Guid, actualResult.Id);
            Assert.AreEqual(expectedResults.Title, actualResult.Title);
            Assert.AreEqual(Dtos.EnumProperties.FiscalPeriodsStatus.Closed, actualResult.Status);
            Assert.AreEqual(Dtos.EnumProperties.FiscalYearsYearEndAdjustment.Active, actualResult.YearEndAdjustment);
            Assert.AreEqual(12, actualResult.NumberOfPeriods);
            Assert.AreEqual(new DateTime(2014, 7, 1), actualResult.StartOn);
            Assert.AreEqual(new DateTime(2015, 6, 30), actualResult.EndOn);
            Assert.AreEqual(expectedResults.InstitutionName, actualResult.ReportingSegment);
        }
        [TestMethod]
        public async Task FiscalYearsService_GetFiscalYearsAsync_InvalidFilter_Expected()
        {
            var results = await _fiscalYearsService.GetFiscalYearsAsync(true, "INVALID");
            Assert.AreEqual(0, results.Count());
        }

            [TestMethod]
        [ExpectedException(typeof(KeyNotFoundException))]
        public async Task FiscalYearsService_GetFiscalYearsByGuidAsync_Empty()
        {
            await _fiscalYearsService.GetFiscalYearsByGuidAsync("");
        }

        [TestMethod]
        [ExpectedException(typeof(KeyNotFoundException))]
        public async Task FiscalYearsService_GetFiscalYearsByGuidAsync_Null()
        {
            await _fiscalYearsService.GetFiscalYearsByGuidAsync(null);
        }

        [TestMethod]
        [ExpectedException(typeof(KeyNotFoundException))]
        public async Task FiscalYearsService_GetFiscalYearsByGuidAsync_InvalidId()
        {
            _referenceRepositoryMock.Setup(repo => repo.GetFiscalYearsAsync(It.IsAny<bool>()))
                .Throws<KeyNotFoundException>();

            await _fiscalYearsService.GetFiscalYearsByGuidAsync("99");
        }

        [TestMethod]
        public async Task FiscalYearsService_GetFiscalYearsByGuidAsync_Expected()
        {
            var expectedResults =
                _fiscalYearsCollection.First(c => c.Guid == fiscalYearsGuid);
            var actualResult =
                await _fiscalYearsService.GetFiscalYearsByGuidAsync(fiscalYearsGuid);
            Assert.AreEqual(expectedResults.Guid, actualResult.Id);
            Assert.AreEqual(expectedResults.Title, actualResult.Title);
            
        }

        [TestMethod]
        public async Task FiscalYearsService_GetFiscalYearsByGuidAsync_Properties()
        {
            var result =
                await _fiscalYearsService.GetFiscalYearsByGuidAsync(fiscalYearsGuid);
            Assert.IsNotNull(result.Id);
              Assert.IsNotNull(result.Title);
        }
    }
}