// Copyright 2016-2017 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ellucian.Colleague.Coordination.Student.Services;
using Ellucian.Colleague.Domain.Student.Repositories;
using Ellucian.Colleague.Domain.Student.Tests;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;
using Ellucian.Colleague.Domain.Student.Entities;
using Ellucian.Web.Adapters;
using Ellucian.Colleague.Data.Base.Repositories;
using Ellucian.Web.Security;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Colleague.Domain.Base.Repositories;

namespace Ellucian.Colleague.Coordination.Student.Tests.Services
{
    [TestClass]
    public class AcademicPeriodServiceTests
    {
        private const string _termCode = "2000/S1";  
        private ICollection<Term> _termCollection;
        private ICollection<Ellucian.Colleague.Domain.Student.Entities.AcademicPeriod> _academicPeriodCollection;
        private AcademicPeriodService _academicPeriodService;
        private Mock<IAdapterRegistry> _adapterRegistryMock;
        private Mock<ICurrentUserFactory> _currentUserFactoryMock;
        private Mock<IConfigurationRepository> _configurationRepositoryMock;
        private Mock<IRoleRepository> _roleRepositoryMock;
        private Mock<ILogger> _loggerMock;
       private Mock<ITermRepository> _termRepositoryMock;
       
        [TestInitialize]
        public void Initialize()
        {
            _termRepositoryMock = new Mock<ITermRepository>();
            _loggerMock = new Mock<ILogger>();
            _adapterRegistryMock = new Mock<IAdapterRegistry>();
            _configurationRepositoryMock = new Mock<IConfigurationRepository>();
            _currentUserFactoryMock = new Mock<ICurrentUserFactory>();
            _roleRepositoryMock = new Mock<IRoleRepository>();
            _termCollection = new TestTermRepository().Get().ToList();

            _academicPeriodCollection = new TestAcademicPeriodRepository().Get().ToList();
        
            _termRepositoryMock.Setup(repo => repo.GetAsync())
                     .ReturnsAsync(_termCollection);

            _termRepositoryMock.Setup(repo => repo.GetAsync(It.IsAny<bool>()))
                    .ReturnsAsync(_termCollection);

            _academicPeriodService = new AcademicPeriodService(_termRepositoryMock.Object, _adapterRegistryMock.Object, 
                _configurationRepositoryMock.Object, _currentUserFactoryMock.Object, _roleRepositoryMock.Object, _loggerMock.Object);
        }

        [TestCleanup]
        public void Cleanup()
        {

            _loggerMock = null;
            _termRepositoryMock = null;
            _academicPeriodService = null;
            _termCollection = null;
            _academicPeriodCollection = null;
  
        }

        [TestMethod]
        public async Task AcademicPeriodService_GetAcademicPeriodsAsync()
        {
            var results = await _academicPeriodService.GetAcademicPeriods2Async(false);
            Assert.IsTrue(results is IEnumerable<Dtos.AcademicPeriod2>);
            Assert.IsNotNull(results);
        }

        [TestMethod]
        public async Task AcademicPeriodService_GetAcademicPeriods3Async()
        {
            var results = await _academicPeriodService.GetAcademicPeriods3Async(false);
            Assert.IsTrue(results is IEnumerable<Dtos.AcademicPeriod3>);
            Assert.IsNotNull(results);
        }

        [TestMethod]
        public async Task AcademicPeriodService_GetAcademicPeriods4Async()
        {
            var term = _termCollection.FirstOrDefault(x => x.Code == _termCode);

            _termRepositoryMock.Setup(repo => repo.GetAsync(It.IsAny<string>()))
                  .ReturnsAsync(term);
            _termRepositoryMock.Setup(repo => repo.GetAcademicPeriods(_termCollection))
              .Returns(_academicPeriodCollection);

            var results = await _academicPeriodService.GetAcademicPeriods4Async(false);
            Assert.IsTrue(results is IEnumerable<Dtos.AcademicPeriod4>);
            Assert.IsNotNull(results);
        }

        [TestMethod]
        public async Task AcademicPeriodService_GetAcademicPeriods4Async_Start_End_On_Filters()
        {
            var term = _termCollection.FirstOrDefault(x => x.Code == _termCode);

            _termRepositoryMock.Setup(repo => repo.GetAsync(It.IsAny<string>()))
                  .ReturnsAsync(term);
            _termRepositoryMock.Setup(repo => repo.GetAcademicPeriods(_termCollection))
              .Returns(_academicPeriodCollection);

            DateTime? startOn = new DateTime(2018, 12, 27);
            DateTime? endOn = new DateTime(2019, 1, 31);

            var results = await _academicPeriodService.GetAcademicPeriods4Async(false, startOn: startOn, endOn: endOn);
            Assert.IsTrue(results is IEnumerable<Dtos.AcademicPeriod4>);
            Assert.IsNotNull(results);
        }

        //StartOn
        [TestMethod]
        public async Task AcademicPeriodService_GetAcademicPeriods4Async_Start_On_GE_Filters()
        {
            var term = _termCollection.FirstOrDefault(x => x.Code == _termCode);

            _termRepositoryMock.Setup(repo => repo.GetAsync(It.IsAny<string>()))
                  .ReturnsAsync(term);
            _termRepositoryMock.Setup(repo => repo.GetAcademicPeriods(_termCollection))
              .Returns(_academicPeriodCollection);

            DateTime? startOn = new DateTime(2000, 5, 21);
            Dictionary<string, string> filterQualifiers = new Dictionary<string, string>();
            filterQualifiers.Add("StartOn", "GE");

            var results = await _academicPeriodService.GetAcademicPeriods4Async(false, startOn: startOn, filterQualifiers: filterQualifiers);
            Assert.IsTrue(results is IEnumerable<Dtos.AcademicPeriod4>);
            Assert.IsNotNull(results);
        }

        [TestMethod]
        public async Task AcademicPeriodService_GetAcademicPeriods4Async_Start_On_GT_Filters()
        {
            var term = _termCollection.FirstOrDefault(x => x.Code == _termCode);

            _termRepositoryMock.Setup(repo => repo.GetAsync(It.IsAny<string>()))
                  .ReturnsAsync(term);
            _termRepositoryMock.Setup(repo => repo.GetAcademicPeriods(_termCollection))
              .Returns(_academicPeriodCollection);

            DateTime? startOn = new DateTime(2000, 5, 21);
            Dictionary<string, string> filterQualifiers = new Dictionary<string, string>();
            filterQualifiers.Add("StartOn", "GT");

            var results = await _academicPeriodService.GetAcademicPeriods4Async(false, startOn: startOn, filterQualifiers: filterQualifiers);
            Assert.IsTrue(results is IEnumerable<Dtos.AcademicPeriod4>);
            Assert.IsNotNull(results);
        }

        [TestMethod]
        public async Task AcademicPeriodService_GetAcademicPeriods4Async_Start_On_LT_Filters()
        {
            var term = _termCollection.FirstOrDefault(x => x.Code == _termCode);

            _termRepositoryMock.Setup(repo => repo.GetAsync(It.IsAny<string>()))
                  .ReturnsAsync(term);
            _termRepositoryMock.Setup(repo => repo.GetAcademicPeriods(_termCollection))
              .Returns(_academicPeriodCollection);

            DateTime? startOn = new DateTime(2000, 5, 21);
            Dictionary<string, string> filterQualifiers = new Dictionary<string, string>();
            filterQualifiers.Add("StartOn", "LT");

            var results = await _academicPeriodService.GetAcademicPeriods4Async(false, startOn: startOn, filterQualifiers: filterQualifiers);
            Assert.IsTrue(results is IEnumerable<Dtos.AcademicPeriod4>);
            Assert.IsNotNull(results);
        }

        [TestMethod]
        public async Task AcademicPeriodService_GetAcademicPeriods4Async_Start_On_LE_Filters()
        {
            var term = _termCollection.FirstOrDefault(x => x.Code == _termCode);

            _termRepositoryMock.Setup(repo => repo.GetAsync(It.IsAny<string>()))
                  .ReturnsAsync(term);
            _termRepositoryMock.Setup(repo => repo.GetAcademicPeriods(_termCollection))
              .Returns(_academicPeriodCollection);

            DateTime? startOn = new DateTime(2000, 5, 21);
            Dictionary<string, string> filterQualifiers = new Dictionary<string, string>();
            filterQualifiers.Add("StartOn", "LE");

            var results = await _academicPeriodService.GetAcademicPeriods4Async(false, startOn: startOn, filterQualifiers: filterQualifiers);
            Assert.IsTrue(results is IEnumerable<Dtos.AcademicPeriod4>);
            Assert.IsNotNull(results);
        }

        [TestMethod]
        public async Task AcademicPeriodService_GetAcademicPeriods4Async_Start_On_NE_Filters()
        {
            var term = _termCollection.FirstOrDefault(x => x.Code == _termCode);

            _termRepositoryMock.Setup(repo => repo.GetAsync(It.IsAny<string>()))
                  .ReturnsAsync(term);
            _termRepositoryMock.Setup(repo => repo.GetAcademicPeriods(_termCollection))
              .Returns(_academicPeriodCollection);

            DateTime? startOn = new DateTime(2000, 5, 21);
            Dictionary<string, string> filterQualifiers = new Dictionary<string, string>();
            filterQualifiers.Add("StartOn", "NE");

            var results = await _academicPeriodService.GetAcademicPeriods4Async(false, startOn: startOn, filterQualifiers: filterQualifiers);
            Assert.IsTrue(results is IEnumerable<Dtos.AcademicPeriod4>);
            Assert.IsNotNull(results);
        }

        [TestMethod]
        public async Task AcademicPeriodService_GetAcademicPeriods4Async_Start_On_EQ_Filters()
        {
            var term = _termCollection.FirstOrDefault(x => x.Code == _termCode);

            _termRepositoryMock.Setup(repo => repo.GetAsync(It.IsAny<string>()))
                  .ReturnsAsync(term);
            _termRepositoryMock.Setup(repo => repo.GetAcademicPeriods(_termCollection))
              .Returns(_academicPeriodCollection);

            DateTime? startOn = new DateTime(2000, 5, 21);
            Dictionary<string, string> filterQualifiers = new Dictionary<string, string>();
            filterQualifiers.Add("StartOn", "EQ");

            var results = await _academicPeriodService.GetAcademicPeriods4Async(false, startOn: startOn, filterQualifiers: filterQualifiers);
            Assert.IsTrue(results is IEnumerable<Dtos.AcademicPeriod4>);
            Assert.IsNotNull(results);
        }

        //EndOn
        [TestMethod]
        public async Task AcademicPeriodService_GetAcademicPeriods4Async_End_On_GE_Filters()
        {
            var term = _termCollection.FirstOrDefault(x => x.Code == _termCode);

            _termRepositoryMock.Setup(repo => repo.GetAsync(It.IsAny<string>()))
                  .ReturnsAsync(term);
            _termRepositoryMock.Setup(repo => repo.GetAcademicPeriods(_termCollection))
              .Returns(_academicPeriodCollection);

            DateTime? endOn = new DateTime(2019, 1, 31);
            Dictionary<string, string> filterQualifiers = new Dictionary<string, string>();
            filterQualifiers.Add("EndOn", "GE");

            var results = await _academicPeriodService.GetAcademicPeriods4Async(false, endOn: endOn, filterQualifiers: filterQualifiers);
            Assert.IsTrue(results is IEnumerable<Dtos.AcademicPeriod4>);
            Assert.IsNotNull(results);
        }

        [TestMethod]
        public async Task AcademicPeriodService_GetAcademicPeriods4Async_End_On_GT_Filters()
        {
            var term = _termCollection.FirstOrDefault(x => x.Code == _termCode);

            _termRepositoryMock.Setup(repo => repo.GetAsync(It.IsAny<string>()))
                  .ReturnsAsync(term);
            _termRepositoryMock.Setup(repo => repo.GetAcademicPeriods(_termCollection))
              .Returns(_academicPeriodCollection);

            DateTime? endOn = new DateTime(2019, 1, 31);
            Dictionary<string, string> filterQualifiers = new Dictionary<string, string>();
            filterQualifiers.Add("EndOn", "GT");

            var results = await _academicPeriodService.GetAcademicPeriods4Async(false, endOn: endOn, filterQualifiers: filterQualifiers);
            Assert.IsTrue(results is IEnumerable<Dtos.AcademicPeriod4>);
            Assert.IsNotNull(results);
        }

        [TestMethod]
        public async Task AcademicPeriodService_GetAcademicPeriods4Async_End_On_LT_Filters()
        {
            var term = _termCollection.FirstOrDefault(x => x.Code == _termCode);

            _termRepositoryMock.Setup(repo => repo.GetAsync(It.IsAny<string>()))
                  .ReturnsAsync(term);
            _termRepositoryMock.Setup(repo => repo.GetAcademicPeriods(_termCollection))
              .Returns(_academicPeriodCollection);

            DateTime? endOn = new DateTime(2019, 1, 31);
            Dictionary<string, string> filterQualifiers = new Dictionary<string, string>();
            filterQualifiers.Add("EndOn", "LT");

            var results = await _academicPeriodService.GetAcademicPeriods4Async(false, endOn: endOn, filterQualifiers: filterQualifiers);
            Assert.IsTrue(results is IEnumerable<Dtos.AcademicPeriod4>);
            Assert.IsNotNull(results);
        }

        [TestMethod]
        public async Task AcademicPeriodService_GetAcademicPeriods4Async_End_On_LE_Filters()
        {
            var term = _termCollection.FirstOrDefault(x => x.Code == _termCode);

            _termRepositoryMock.Setup(repo => repo.GetAsync(It.IsAny<string>()))
                  .ReturnsAsync(term);
            _termRepositoryMock.Setup(repo => repo.GetAcademicPeriods(_termCollection))
              .Returns(_academicPeriodCollection);

            DateTime? endOn = new DateTime(2019, 1, 31);
            Dictionary<string, string> filterQualifiers = new Dictionary<string, string>();
            filterQualifiers.Add("EndOn", "LE");

            var results = await _academicPeriodService.GetAcademicPeriods4Async(false, endOn: endOn, filterQualifiers: filterQualifiers);
            Assert.IsTrue(results is IEnumerable<Dtos.AcademicPeriod4>);
            Assert.IsNotNull(results);
        }

        [TestMethod]
        public async Task AcademicPeriodService_GetAcademicPeriods4Async_End_On_NE_Filters()
        {
            var term = _termCollection.FirstOrDefault(x => x.Code == _termCode);

            _termRepositoryMock.Setup(repo => repo.GetAsync(It.IsAny<string>()))
                  .ReturnsAsync(term);
            _termRepositoryMock.Setup(repo => repo.GetAcademicPeriods(_termCollection))
              .Returns(_academicPeriodCollection);

            DateTime? endOn = new DateTime(2019, 1, 31);
            Dictionary<string, string> filterQualifiers = new Dictionary<string, string>();
            filterQualifiers.Add("EndOn", "NE");

            var results = await _academicPeriodService.GetAcademicPeriods4Async(false, endOn: endOn, filterQualifiers: filterQualifiers);
            Assert.IsTrue(results is IEnumerable<Dtos.AcademicPeriod4>);
            Assert.IsNotNull(results);
        }

        [TestMethod]
        public async Task AcademicPeriodService_GetAcademicPeriods4Async_End_On_EQ_Filters()
        {
            var term = _termCollection.FirstOrDefault(x => x.Code == _termCode);

            _termRepositoryMock.Setup(repo => repo.GetAsync(It.IsAny<string>()))
                  .ReturnsAsync(term);
            _termRepositoryMock.Setup(repo => repo.GetAcademicPeriods(_termCollection))
              .Returns(_academicPeriodCollection);

            DateTime? endOn = new DateTime(2019, 1, 31);
            Dictionary<string, string> filterQualifiers = new Dictionary<string, string>();
            filterQualifiers.Add("EndOn", "EQ");

            var results = await _academicPeriodService.GetAcademicPeriods4Async(false, endOn: endOn, filterQualifiers: filterQualifiers);
            Assert.IsTrue(results is IEnumerable<Dtos.AcademicPeriod4>);
            Assert.IsNotNull(results);
        }

        [TestMethod]
        public async Task AcademicPeriodService_GetAcademicPeriodsAsync_Count()
        {
            var term = _termCollection.FirstOrDefault(x => x.Code == _termCode);

            _termRepositoryMock.Setup(repo => repo.GetAsync(It.IsAny<string>()))
                    .ReturnsAsync(term);
            _termRepositoryMock.Setup(repo => repo.GetAcademicPeriods(_termCollection))
              .Returns(_academicPeriodCollection);

            var results = await _academicPeriodService.GetAcademicPeriods2Async(false);
            Assert.AreEqual(_academicPeriodCollection.Count, results.Count());
        }

        [TestMethod]
        public async Task AcademicPeriodService_GetAcademicPeriods3Async_Count()
        {
            var term = _termCollection.FirstOrDefault(x => x.Code == _termCode);

            _termRepositoryMock.Setup(repo => repo.GetAsync(It.IsAny<string>()))
                    .ReturnsAsync(term);
            _termRepositoryMock.Setup(repo => repo.GetAcademicPeriods(_termCollection))
              .Returns(_academicPeriodCollection);

            var results = await _academicPeriodService.GetAcademicPeriods3Async(false);
            Assert.AreEqual(_academicPeriodCollection.Count, results.Count());
        }

        [TestMethod]
        public async Task AcademicPeriodService_GetAcademicPeriodsAsync_Properties()
        {
           
            _termRepositoryMock.Setup(repo => repo.GetAsync())
                   .ReturnsAsync(_termCollection);

            _termRepositoryMock.Setup(repo => repo.GetAcademicPeriods(_termCollection))
                .Returns(_academicPeriodCollection);

            var result =
                (await _academicPeriodService.GetAcademicPeriods2Async(false)).FirstOrDefault(x => x.Code == _termCode);
            Assert.IsNotNull(result.Id);
            Assert.IsNotNull(result.Code);
            Assert.IsNull(result.Description);
            Assert.IsNotNull(result.Title);
            Assert.IsNotNull(result.Category);
        }

        [TestMethod]
        public async Task AcademicPeriodService_GetAcademicPeriods3Async_Properties()
        {

            _termRepositoryMock.Setup(repo => repo.GetAsync())
                   .ReturnsAsync(_termCollection);

            _termRepositoryMock.Setup(repo => repo.GetAcademicPeriods(_termCollection))
                .Returns(_academicPeriodCollection);

            var result =
                (await _academicPeriodService.GetAcademicPeriods3Async(false)).FirstOrDefault(x => x.Code == _termCode);
            Assert.IsNotNull(result.Id);
            Assert.IsNotNull(result.Code);
            Assert.IsNull(result.Description);
            Assert.IsNotNull(result.Title);
            Assert.IsNotNull(result.Category);
        }   

        [TestMethod]
        [ExpectedException(typeof(KeyNotFoundException))]
        public async Task AcademicPeriodService_GetAcademicPeriodByIdAsync_Empty()
        {
            var term = _termCollection.FirstOrDefault(x => x.Code == _termCode);

            _termRepositoryMock.Setup(repo => repo.GetAsync(It.IsAny<string>()))
                    .ReturnsAsync(term);

            await _academicPeriodService.GetAcademicPeriodByGuid2Async("");
        }

        [TestMethod]
        [ExpectedException(typeof(KeyNotFoundException))]
        public async Task AcademicPeriodService_GetAcademicPeriodById3Async_Empty()
        {
            var term = _termCollection.FirstOrDefault(x => x.Code == _termCode);

            _termRepositoryMock.Setup(repo => repo.GetAsync(It.IsAny<string>()))
                    .ReturnsAsync(term);

            await _academicPeriodService.GetAcademicPeriodByGuid3Async("");
        }

        [TestMethod]
        [ExpectedException(typeof(KeyNotFoundException))]
        public async Task AcademicPeriodService_GetAcademicPeriodByGuidAsync_Null()
        {
            await _academicPeriodService.GetAcademicPeriodByGuid2Async(null);
        }

        [TestMethod]
        [ExpectedException(typeof(KeyNotFoundException))]
        public async Task AcademicPeriodService_GetAcademicPeriodByGuid3Async_Null()
        {
            await _academicPeriodService.GetAcademicPeriodByGuid3Async(null);
        }

        [TestMethod]
        [ExpectedException(typeof(KeyNotFoundException))]
        public async Task AcademicPeriodService_GetAcademicPeriodByGuidAsync_InvalidId()
        {
            _termRepositoryMock.Setup(repo => repo.GetAsync(It.IsAny<string>()))
                .Throws<InvalidOperationException>();

            await _academicPeriodService.GetAcademicPeriodByGuid2Async("99");
        }

        [TestMethod]
        [ExpectedException(typeof(KeyNotFoundException))]
        public async Task AcademicPeriodService_GetAcademicPeriodByGuid3Async_InvalidId()
        {
            _termRepositoryMock.Setup(repo => repo.GetAsync(It.IsAny<string>()))
                .Throws<InvalidOperationException>();

            await _academicPeriodService.GetAcademicPeriodByGuid3Async("99");
        }

        [TestMethod]
        public async Task AcademicPeriodService_GetAcademicPeriodByIdAsync_Tests_Expected()
        {
            var term = _termCollection.FirstOrDefault(x => x.Code == _termCode);
            var acadPeriod = _academicPeriodCollection.FirstOrDefault(x => x.Code == _termCode);

            _termRepositoryMock.Setup(repo => repo.GetAsync())
                   .ReturnsAsync(_termCollection);

            _termRepositoryMock.Setup(repo => repo.GetAsync(It.IsAny<string>()))
                    .ReturnsAsync(term);

            _termRepositoryMock.Setup(repo => repo.GetAcademicPeriods(_termCollection))
                .Returns(_academicPeriodCollection);

           
            var actualResult =
                await _academicPeriodService.GetAcademicPeriodByGuid2Async(acadPeriod.Guid);
            Assert.AreEqual(acadPeriod.Guid, actualResult.Id);
            Assert.AreEqual(acadPeriod.Description, actualResult.Title);
            Assert.AreEqual(acadPeriod.Code, actualResult.Code);
            Assert.AreEqual(acadPeriod.StartDate, actualResult.Start);
            Assert.AreEqual(acadPeriod.EndDate, actualResult.End);
            
        }

        [TestMethod]
        public async Task AcademicPeriodService_GetAcademicPeriodById3Async_Tests_Expected()
        {
            var term = _termCollection.FirstOrDefault(x => x.Code == _termCode);
            var acadPeriod = _academicPeriodCollection.FirstOrDefault(x => x.Code == _termCode);

            _termRepositoryMock.Setup(repo => repo.GetAsync())
                   .ReturnsAsync(_termCollection);

            _termRepositoryMock.Setup(repo => repo.GetAsync(It.IsAny<string>()))
                    .ReturnsAsync(term);

            _termRepositoryMock.Setup(repo => repo.GetAcademicPeriods(_termCollection))
                .Returns(_academicPeriodCollection);


            var actualResult =
                await _academicPeriodService.GetAcademicPeriodByGuid3Async(acadPeriod.Guid);
            Assert.AreEqual(acadPeriod.Guid, actualResult.Id);
            Assert.AreEqual(acadPeriod.Description, actualResult.Title);
            Assert.AreEqual(acadPeriod.Code, actualResult.Code);
            Assert.AreEqual(acadPeriod.StartDate, actualResult.Start);
            Assert.AreEqual(acadPeriod.EndDate, actualResult.End);

        }

        [TestMethod]
        public async Task AcademicPeriodService_GetAcademicPeriodByIdAsync_Properties()
        {
            var term = _termCollection.FirstOrDefault(x => x.Code == _termCode);

            _termRepositoryMock.Setup(repo => repo.GetAsync())
                   .ReturnsAsync(_termCollection);

            _termRepositoryMock.Setup(repo => repo.GetAsync(It.IsAny<string>()))
                    .ReturnsAsync(term);

            _termRepositoryMock.Setup(repo => repo.GetAcademicPeriods(_termCollection))
                .Returns(_academicPeriodCollection);

            var acadPeriodGuid = _academicPeriodCollection.FirstOrDefault(x => x.Code == _termCode).Guid;       

            var result =
                await _academicPeriodService.GetAcademicPeriodByGuid2Async(acadPeriodGuid);
            Assert.IsNotNull(result.Id);
            Assert.IsNotNull(result.Code);
            Assert.IsNull(result.Description);
            Assert.IsNotNull(result.Title);
            Assert.IsNotNull(result.Category);
            Assert.IsNotNull(result.Start);
            Assert.IsNotNull(result.End);
        }

        [TestMethod]
        public async Task AcademicPeriodService_GetAcademicPeriodById3Async_Properties()
        {
            var term = _termCollection.FirstOrDefault(x => x.Code == _termCode);

            _termRepositoryMock.Setup(repo => repo.GetAsync())
                   .ReturnsAsync(_termCollection);

            _termRepositoryMock.Setup(repo => repo.GetAsync(It.IsAny<string>()))
                    .ReturnsAsync(term);

            _termRepositoryMock.Setup(repo => repo.GetAcademicPeriods(_termCollection))
                .Returns(_academicPeriodCollection);

            var acadPeriodGuid = _academicPeriodCollection.FirstOrDefault(x => x.Code == _termCode).Guid;

            var result =
                await _academicPeriodService.GetAcademicPeriodByGuid3Async(acadPeriodGuid);
            Assert.IsNotNull(result.Id);
            Assert.IsNotNull(result.Code);
            Assert.IsNull(result.Description);
            Assert.IsNotNull(result.Title);
            Assert.IsNotNull(result.Category);
            Assert.IsNotNull(result.Start);
            Assert.IsNotNull(result.End);
        }
    }
}