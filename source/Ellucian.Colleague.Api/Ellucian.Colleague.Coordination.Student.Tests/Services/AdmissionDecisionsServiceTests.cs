//Copyright 2017-2021 Ellucian Company L.P. and its affiliates.


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
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Web.Adapters;
using Ellucian.Web.Security;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Web.Http.Exceptions;

namespace Ellucian.Colleague.Coordination.Student.Tests.Services
{    
    [TestClass]
    public class AdmissionDecisionsServiceTests
    {
        private const string admissionDecisionsGuid = "7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc";
        private const string admissionDecisionsCode = "AT";
        private IEnumerable<ApplicationStatus2> _admissionDecisionsCollection;
        private IEnumerable<AdmissionDecisionType> _admissionDecisionTypeCollection;
        private AdmissionDecisionsService _admissionDecisionsService;
        Dtos.AdmissionDecisions admissionDecisionDTOIn;
        Dtos.AdmissionDecisions admissionDecisionDTOOut;

        private Mock<IApplicationStatusRepository> _applicationStatusRepositoryMock;
        private Mock<IAdmissionApplicationsRepository> _admissionApplicationsRepositoryMock;
        private Mock<IStudentReferenceDataRepository> _studentReferenceRepositoryMock;
        private Mock<IReferenceDataRepository> _referenceRepositoryMock;
        private Dictionary<string, string> _admissionApplicationDict = new Dictionary<string, string>();
        Tuple<IEnumerable<Domain.Student.Entities.ApplicationStatus2>, int> _adminDecionsEntityTuple;
        Domain.Entities.Role _roleView = new Domain.Entities.Role(1, "VIEW.ADMISSION.DECISIONS");
        Domain.Entities.Role _roleUpdate = new Domain.Entities.Role(2, "UPDATE.ADMISSION.DECISIONS");

        private Mock<IAdapterRegistry> _adapterRegistryMock;
        //private Mock<ICurrentUserFactory> _currentUserFactoryMock;
        ICurrentUserFactory userFactory;
        private Mock<IRoleRepository> _roleRepositoryMock;
        private Mock<IConfigurationRepository> _configurationRepoMock;
        private Mock<ILogger> _loggerMock;
       

        [TestInitialize]
        public void Initialize()
        {
            _applicationStatusRepositoryMock = new Mock<IApplicationStatusRepository>();
            _admissionApplicationsRepositoryMock = new Mock<IAdmissionApplicationsRepository>();
            _studentReferenceRepositoryMock = new Mock<IStudentReferenceDataRepository>();
            _referenceRepositoryMock = new Mock<IReferenceDataRepository>();
            _adapterRegistryMock = new Mock<IAdapterRegistry>();
            _roleRepositoryMock = new Mock<IRoleRepository>();
            _configurationRepoMock = new Mock<IConfigurationRepository>();
            _loggerMock = new Mock<ILogger>();

            userFactory = new UserFactories.StudentUserFactory.AdmissionDecisionUser();

            InitializeData();
            InitializeMocks();

            _admissionDecisionsService = new AdmissionDecisionsService(_applicationStatusRepositoryMock.Object, _admissionApplicationsRepositoryMock.Object, _studentReferenceRepositoryMock.Object,
                                            _referenceRepositoryMock.Object, _adapterRegistryMock.Object, userFactory, _roleRepositoryMock.Object, _configurationRepoMock.Object, _loggerMock.Object);
        }

        private void InitializeMocks()
        {
            _applicationStatusRepositoryMock.Setup(repo => repo.GetApplicationStatusesAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string[]>(), It.IsAny<DateTimeOffset>(), It.IsAny<Dictionary<string, string>>(), It.IsAny<bool>())).ReturnsAsync(_adminDecionsEntityTuple);
            _applicationStatusRepositoryMock.Setup(repo => repo.GetApplicationStatusByGuidAsync(admissionDecisionsGuid, It.IsAny<bool>())).ReturnsAsync(_adminDecionsEntityTuple.Item1.FirstOrDefault());
            _applicationStatusRepositoryMock.Setup(repo => repo.UpdateAdmissionDecisionAsync(It.IsAny<ApplicationStatus2>())).ReturnsAsync(_admissionDecisionsCollection.First());
            _admissionApplicationsRepositoryMock.Setup(i => i.GetAdmissionApplicationGuidDictionary(It.IsAny<IEnumerable<string>>())).ReturnsAsync(_admissionApplicationDict);
            _studentReferenceRepositoryMock.Setup(i => i.GetAdmissionDecisionTypesAsync(It.IsAny<bool>())).ReturnsAsync(_admissionDecisionTypeCollection);
            _roleRepositoryMock.Setup(i => i.GetRolesAsync()).ReturnsAsync(new List<Domain.Entities.Role>() { _roleView, _roleUpdate });

            _studentReferenceRepositoryMock.Setup(i => i.GetAdmissionDecisionTypesGuidAsync(It.IsAny<string>()))
                .ReturnsAsync(_admissionDecisionTypeCollection.FirstOrDefault(x=> x.Code == "WITHP").Guid);

        }

        private void InitializeData()
        {
            _admissionDecisionsCollection = new List<Domain.Student.Entities.ApplicationStatus2>()
                {
                    new Domain.Student.Entities.ApplicationStatus2("7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc", "1", "WITHP",  Convert.ToDateTime("2017-07-31 00:00:00.000"), Convert.ToDateTime("1900-01-01 21:38:44.000")),
                    new Domain.Student.Entities.ApplicationStatus2("8a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc", "2", "PR",  Convert.ToDateTime("2017-07-31 00:00:00.000"), Convert.ToDateTime("1900-01-01 14:53:56.000")),
                    new Domain.Student.Entities.ApplicationStatus2("9a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc", "3", "AD",  Convert.ToDateTime("2017-07-31 00:00:00.000"), Convert.ToDateTime("1900-01-01 11:05:45.000"))
                };
            _admissionDecisionTypeCollection = new List<AdmissionDecisionType>()
            {
                new AdmissionDecisionType("3a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc", "WITHP", "Decision Type 1"),
                new AdmissionDecisionType("2a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc", "PR", "Decision Type 1"),
                new AdmissionDecisionType("1a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc", "AD", "Decision Type 1")
            };
            admissionDecisionDTOIn = new Dtos.AdmissionDecisions()
            {
                Id = Guid.Empty.ToString(),
                Application = new Dtos.GuidObject2("7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc"),
                DecidedOn = new DateTime(2017, 10, 11),
                DecisionType = new Dtos.GuidObject2("3a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc")
            };
            admissionDecisionDTOOut = new Dtos.AdmissionDecisions()
            {
                Id = "7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc",
                Application = new Dtos.GuidObject2("7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc"),
                DecidedOn = new DateTime(2017, 10, 11),
                DecisionType = new Dtos.GuidObject2("3a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc")
            };

            _roleView.AddPermission(new Domain.Entities.Permission("VIEW.ADMISSION.DECISIONS"));
            _roleUpdate.AddPermission(new Domain.Entities.Permission("UPDATE.ADMISSION.DECISIONS"));
            _admissionApplicationDict.Add("1", "6a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc");
            _admissionApplicationDict.Add("2", "5a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc");
            _admissionApplicationDict.Add("3", "4a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc");
            _adminDecionsEntityTuple = new Tuple<IEnumerable<Domain.Student.Entities.ApplicationStatus2>, int>(_admissionDecisionsCollection, _admissionDecisionsCollection.Count());
        }

        [TestCleanup]
        public void Cleanup()
        {
            _applicationStatusRepositoryMock = null;
            _admissionApplicationsRepositoryMock = null;
            _adminDecionsEntityTuple = null;
            _admissionApplicationDict = null;
            _admissionDecisionsService = null;
            _admissionDecisionsCollection = null;
            _studentReferenceRepositoryMock = null;
            _loggerMock = null;
            _roleRepositoryMock = null;
            _configurationRepoMock = null;
        }

        [TestMethod]
        public async Task AdmissionDecisionsService_GetAdmissionDecisionsAsync()
        {
            var results = await _admissionDecisionsService.GetAdmissionDecisionsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<Dictionary<string, string>>(), It.IsAny<string>(), It.IsAny<bool>());
            Assert.IsTrue(results is Tuple<IEnumerable<AdmissionDecisions>, int>);
            Assert.IsNotNull(results);
        }

        [TestMethod]
        public async Task AdmissionDecisionsService_GetAdmissionDecisionsAsync_Count()
        {
            var results = await _admissionDecisionsService.GetAdmissionDecisionsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<Dictionary<string, string>>(), It.IsAny<string>(), It.IsAny<bool>());
            Assert.AreEqual(3, results.Item1.Count());
        }

        //[TestMethod]
        //[ExpectedException(typeof(PermissionsException))]
        //public async Task AdmissionDecisionsService_GetAdmissionDecisionsAsync_PermissionsException()
        //{
        //    _roleRepositoryMock.Setup(i => i.GetRolesAsync()).ReturnsAsync(new List<Domain.Entities.Role>() { });
        //    await _admissionDecisionsService.GetAdmissionDecisionsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<Dictionary<string, string>>(), It.IsAny<string>(), It.IsAny<bool>());
        //}

        [TestMethod]
        [ExpectedException(typeof(KeyNotFoundException))]
        public async Task AdmissionDecisionsService_GetAdmissionDecisionsByGuidAsync_KeyNotFoundException()
        {
            _admissionApplicationsRepositoryMock.Setup(i => i.GetAdmissionApplicationGuidDictionary(It.IsAny<IEnumerable<string>>())).ThrowsAsync(new KeyNotFoundException());
            await _admissionDecisionsService.GetAdmissionDecisionsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<Dictionary<string, string>>(), It.IsAny<string>(), It.IsAny<bool>());
        }

        [TestMethod]
        [ExpectedException(typeof(IntegrationApiException))]
        public async Task AdmissionDecisionsService_GetAdmissionDecisionsByGuidAsync_Application_KeyNotFoundException()
        {
            Domain.Student.Entities.ApplicationStatus2 entity = new Domain.Student.Entities.ApplicationStatus2("7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc", "100", "ABC", Convert.ToDateTime("2017-07-31 00:00:00.000"), Convert.ToDateTime("1900-01-01 21:38:44.000"));
            _applicationStatusRepositoryMock.Setup(repo => repo.GetApplicationStatusByGuidAsync(admissionDecisionsGuid, It.IsAny<bool>())).ReturnsAsync(entity);

            var actualResult =
                await _admissionDecisionsService.GetAdmissionDecisionsByGuidAsync(admissionDecisionsGuid, true);
        }

        [TestMethod]
        [ExpectedException(typeof(KeyNotFoundException))]
        public async Task AdmissionDecisionsService_GetAdmissionDecisionsByGuidAsync_AdmissionDecisionId_KeyNotFoundException()
        {
            _studentReferenceRepositoryMock.Setup(i => i.GetAdmissionDecisionTypesGuidAsync(It.IsAny<string>()))
               .Throws<KeyNotFoundException>();

            var entity = new Domain.Student.Entities.ApplicationStatus2("7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc", "1", "ABC", Convert.ToDateTime("2017-07-31 00:00:00.000"), Convert.ToDateTime("1900-01-01 21:38:44.000"));
            _applicationStatusRepositoryMock.Setup(repo => repo.GetApplicationStatusByGuidAsync(admissionDecisionsGuid, It.IsAny<bool>())).ReturnsAsync(entity);

            var actualResult =
                await _admissionDecisionsService.GetAdmissionDecisionsByGuidAsync(admissionDecisionsGuid, true);
        }

        //[TestMethod]
        //[ExpectedException(typeof(PermissionsException))]
        //public async Task AdmissionDecisionsService_GetAdmissionDecisionsByGuidAsync_PermissionsException()
        //{
        //    _roleRepositoryMock.Setup(i => i.GetRolesAsync()).ReturnsAsync(new List<Domain.Entities.Role>() { });

        //    await _admissionDecisionsService.GetAdmissionDecisionsByGuidAsync(null);
        //}

        [TestMethod]
        [ExpectedException(typeof(KeyNotFoundException))]
        public async Task AdmissionDecisionsService_GetAdmissionDecisionsByGuidAsync_InvalidId()
        {
            await _admissionDecisionsService.GetAdmissionDecisionsByGuidAsync("99");
        }

        [TestMethod]
        public async Task AdmissionDecisionsService_GetAdmissionDecisionsByGuidAsync_Expected()
        {
            var expectedResults =
                _admissionDecisionsCollection.First(c => c.Guid == admissionDecisionsGuid);
            var actualResult =
                await _admissionDecisionsService.GetAdmissionDecisionsByGuidAsync(admissionDecisionsGuid, true);
            Assert.IsNotNull(actualResult);
            Assert.AreEqual(expectedResults.Guid, actualResult.Id);
        }

        //[TestMethod]
        //[ExpectedException(typeof(PermissionsException))]
        //public async Task AdmissionDecisionsService_CreateAdmissionDecisionAsync_PermissionsException()
        //{
        //    _roleRepositoryMock.Setup(i => i.GetRolesAsync()).ReturnsAsync(new List<Domain.Entities.Role>() { });

        //    await _admissionDecisionsService.CreateAdmissionDecisionAsync(admissionDecisionDTOIn);
        //}

        [TestMethod]
        [ExpectedException(typeof(IntegrationApiException))]
        public async Task AdmissionDecisionsService_CreateAdmissionDecisionAsync_KeyNotFoundException()
        {
            var result = await _admissionDecisionsService.CreateAdmissionDecisionAsync(admissionDecisionDTOIn);
        }

        [TestMethod]
        [ExpectedException(typeof(IntegrationApiException))]
        public async Task AdmissionDecisionsService_CreateAdmissionDecisionAsync_ID_With_Guid_ArgumentException()
        {
            admissionDecisionDTOIn.Id = Guid.NewGuid().ToString();
            _applicationStatusRepositoryMock.Setup(i => i.GetApplicationStatusKey(It.IsAny<string>()))
                    .Returns(Task.FromResult(new Tuple<string, string, string>("", "", "")));
            var result = await _admissionDecisionsService.CreateAdmissionDecisionAsync(admissionDecisionDTOIn);
        }

        [TestMethod]
        [ExpectedException(typeof(IntegrationApiException))]
        public async Task AdmissionDecisionsService_CreateAdmissionDecisionAsync_Item1_APPLICATION_KeyNotFoundException()
        {            
            _applicationStatusRepositoryMock.SetupSequence(i => i.GetApplicationStatusKey(It.IsAny<string>()))
                    .Returns(Task.FromResult(new Tuple<string, string, string>("APPLICATION", "", "")));
            var result = await _admissionDecisionsService.CreateAdmissionDecisionAsync(admissionDecisionDTOIn);
        }

        [TestMethod]
        [ExpectedException(typeof(IntegrationApiException))]
        public async Task AdmissionDecisionsService_CreateAdmissionDecisionAsync_Item3_Has_Index_KeyNotFoundException()
        {
            _applicationStatusRepositoryMock.SetupSequence(i => i.GetApplicationStatusKey(It.IsAny<string>()))
                    .Returns(Task.FromResult(new Tuple<string, string, string>("APPLICATIONS", "", "1*1")));
            var result = await _admissionDecisionsService.CreateAdmissionDecisionAsync(admissionDecisionDTOIn);
        }

        [TestMethod]
        [ExpectedException(typeof(IntegrationApiException))]
        public async Task AdmissionDecisionsService_CreateAdmissionDecisionAsync_DecisionType_ID_Invalid_KeyNotFoundException()
        {
            _applicationStatusRepositoryMock.SetupSequence(i => i.GetApplicationStatusKey(It.IsAny<string>()))
                    .Returns(Task.FromResult(new Tuple<string, string, string>("APPLICATIONS", "1", "")));
            admissionDecisionDTOIn.DecisionType.Id = Guid.NewGuid().ToString();
            var result = await _admissionDecisionsService.CreateAdmissionDecisionAsync(admissionDecisionDTOIn);
        }

        [TestMethod]
        [ExpectedException(typeof(IntegrationApiException))]
        public async Task AdmissionDecisionsService_CreateAdmissionDecisionAsync_SpecialProcessingCode_Null_KeyNotFoundException()
        {
            _applicationStatusRepositoryMock.SetupSequence(i => i.GetApplicationStatusKey(It.IsAny<string>()))
                    .Returns(Task.FromResult(new Tuple<string, string, string>("APPLICATIONS", "1", "")));
            admissionDecisionDTOIn.DecisionType.Id = Guid.NewGuid().ToString();
            var result = await _admissionDecisionsService.CreateAdmissionDecisionAsync(admissionDecisionDTOIn);
        }

        [TestMethod]
        [ExpectedException(typeof(IntegrationApiException))]
        public async Task AdmissionDecisionsService_CreateAdmissionDecisionAsync_SpecialProcessingCode_Empty_String_InvalidOperationException()
        {
            _applicationStatusRepositoryMock.SetupSequence(i => i.GetApplicationStatusKey(It.IsAny<string>()))
                    .Returns(Task.FromResult(new Tuple<string, string, string>("APPLICATIONS", "1", "")));
            _admissionDecisionTypeCollection.First().SpecialProcessingCode = "";
            var result = await _admissionDecisionsService.CreateAdmissionDecisionAsync(admissionDecisionDTOIn);
        }

        [TestMethod]
        public async Task AdmissionDecisionsService_CreateAdmissionDecisionAsync()
        {
            _admissionDecisionTypeCollection.First().SpecialProcessingCode = "AB";
            _applicationStatusRepositoryMock.SetupSequence(i => i.GetApplicationStatusKey(It.IsAny<string>()))
                    .Returns(Task.FromResult(new Tuple<string, string, string>("APPLICATIONS", "1", "")));

            var result = await _admissionDecisionsService.CreateAdmissionDecisionAsync(admissionDecisionDTOIn);
            Assert.IsNotNull(result);
        }
    }
}