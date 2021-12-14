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
using Ellucian.Colleague.Domain.HumanResources;
using Ellucian.Web.Http.Exceptions;

namespace Ellucian.Colleague.Coordination.HumanResources.Tests.Services
{
    [TestClass]
    public class JobApplicationsServiceTests : CurrentUserSetup
    {
        private const string jobApplicationsGuid = "7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc";
        private const string jobApplicationsPersonId = "PID";
        private const string personGuid = "1dd56e2d-9b99-4a5b-ab84-55131a31f2e3";
        private ICollection<JobApplication> _jobApplicationsCollection;
        private Tuple<IEnumerable<JobApplication>, int> jobApplicationsTuple;
        private ICollection<JobApplications> _jobApplicationsDtoCollection;
        private Tuple<IEnumerable<JobApplications>, int> jobApplicationsDtoTuple;
        private JobApplicationsService _jobApplicationsService;

        private Mock<IJobApplicationsRepository> _jobApplicationRepositoryMock;
        private Mock<IPositionRepository> _positionRepositoryMock;
        private Mock<IPersonRepository> _personRepositoryMock;
        private Mock<ILogger> _loggerMock;
        private Mock<IAdapterRegistry> _adapterRegistryMock;
        private ICurrentUserFactory _currentUserFactory;
        private Mock<IRoleRepository> _roleRepositoryMock;
        private Mock<IConfigurationRepository> _configurationRepoMock;


        [TestInitialize]
        public async void Initialize()
        {
            _jobApplicationRepositoryMock = new Mock<IJobApplicationsRepository>();
            _positionRepositoryMock = new Mock<IPositionRepository>();
            _personRepositoryMock = new Mock<IPersonRepository>();
            _adapterRegistryMock = new Mock<IAdapterRegistry>();
            _loggerMock = new Mock<ILogger>();
            //_currentUserFactory = new ICurrentUserFactory();
            _roleRepositoryMock = new Mock<IRoleRepository>();
            _configurationRepoMock = new Mock<IConfigurationRepository>();


            _jobApplicationsCollection = new List<JobApplication>()
                {
                    new JobApplication("7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc", "PID"),
                    new JobApplication("849e6a7c-6cd4-4f98-8a73-ab0aa3627f0d", "PID2"),
                    new JobApplication("d2253ac7-9931-4560-b42f-1fccd43c952e", "PID3")
                };
            jobApplicationsTuple = new Tuple<IEnumerable<JobApplication>, int>(_jobApplicationsCollection, _jobApplicationsCollection.Count);

            _jobApplicationsDtoCollection = new List<JobApplications>()
                {
                    new JobApplications() { Id = "7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc", Person = new GuidObject2("849e6a7c-6cd4-4f98-8a73-ab0aa3627f0d") },
                    new JobApplications() { Id = "849e6a7c-6cd4-4f98-8a73-ab0aa3627f0d", Person = new GuidObject2("d2253ac7-9931-4560-b42f-1fccd43c952e") },
                    new JobApplications() { Id = "d2253ac7-9931-4560-b42f-1fccd43c952e", Person = new GuidObject2("7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc") }
                };

            

            var personGuidDictionary = new Dictionary<string, string>() { };
            personGuidDictionary.Add("PID", personGuid);
            personGuidDictionary.Add("PID2", "a7cbdbbe-131e-4b91-9c99-d9b65c41f1c8");
            personGuidDictionary.Add("PID3", "ae91ddf9-0b25-4008-97c5-76ac5fe570a3");
            _personRepositoryMock.Setup(repo => repo.GetPersonGuidsCollectionAsync(It.IsAny<IEnumerable<string>>()))
                .ReturnsAsync(personGuidDictionary);

            jobApplicationsDtoTuple = new Tuple<IEnumerable<JobApplications>, int>(_jobApplicationsDtoCollection, _jobApplicationsDtoCollection.Count);

            // Set up current user
            _currentUserFactory = new CurrentUserSetup.PersonUserFactory();

            // Mock permissions
            var permissionView = new Ellucian.Colleague.Domain.Entities.Permission(HumanResourcesPermissionCodes.ViewJobApplications);
            personRole.AddPermission(permissionView);
            _roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { personRole });

            _jobApplicationRepositoryMock.Setup(repo => repo.GetJobApplicationsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
                .ReturnsAsync(jobApplicationsTuple);

            _jobApplicationRepositoryMock.Setup(repo => repo.GetJobApplicationByIdAsync(It.IsAny<string>()))
                .ReturnsAsync(_jobApplicationsCollection.ElementAt(0));

            _jobApplicationsService = new JobApplicationsService(_jobApplicationRepositoryMock.Object,
                _positionRepositoryMock.Object, _personRepositoryMock.Object, _adapterRegistryMock.Object, _currentUserFactory,
                _roleRepositoryMock.Object, _configurationRepoMock.Object, _loggerMock.Object);
        }

        [TestCleanup]
        public void Cleanup()
        {
            _jobApplicationsService = null;
            _jobApplicationsCollection = null;
            _jobApplicationRepositoryMock = null;
            _loggerMock = null;
            _currentUserFactory = null;
            _roleRepositoryMock = null;
            _configurationRepoMock = null;
        }

        [TestMethod]
        public async Task JobApplicationsService_GetJobApplicationsAsync()
        {
            var results = await _jobApplicationsService.GetJobApplicationsAsync(0, 2, true);
            Assert.IsNotNull(results);
            Assert.IsTrue(results.Item1 is IEnumerable<JobApplications>);
        }

        [TestMethod]
        public async Task JobApplicationsService_GetJobApplicationsAsync_Count()
        {
            var results = await _jobApplicationsService.GetJobApplicationsAsync(0, 2, true);
            Assert.AreEqual(3, results.Item2);
        }

        [TestMethod]
        public async Task JobApplicationsService_GetJobApplicationsAsync_Expected()
        {
            var expectedResults = _jobApplicationsCollection.FirstOrDefault(c => c.Guid == jobApplicationsGuid);

            var actualResult =
                (await _jobApplicationsService.GetJobApplicationsAsync(0, 2, true)).Item1.FirstOrDefault(x => x.Id == jobApplicationsGuid);
            
            Assert.AreEqual(expectedResults.Guid, actualResult.Id);
            Assert.AreEqual(personGuid, actualResult.Person.Id);

        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task JobApplicationsService_GetJobApplicationsByGuidAsync_Empty()
        {
            _jobApplicationRepositoryMock.Setup(repo => repo.GetJobApplicationByIdAsync(It.IsAny<string>()))
                .Throws<KeyNotFoundException>();
            await _jobApplicationsService.GetJobApplicationsByGuidAsync("");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task JobApplicationsService_GetJobApplicationsByGuidAsync_Null()
        {
            _jobApplicationRepositoryMock.Setup(repo => repo.GetJobApplicationByIdAsync(It.IsAny<string>()))
                .Throws<KeyNotFoundException>();
            await _jobApplicationsService.GetJobApplicationsByGuidAsync(null);
        }

        [TestMethod]
        [ExpectedException(typeof(KeyNotFoundException))]
        public async Task JobApplicationsService_GetJobApplicationsByGuidAsync_InvalidId()
        {
            _jobApplicationRepositoryMock.Setup(repo => repo.GetJobApplicationByIdAsync(It.IsAny<string>()))
                .Throws<KeyNotFoundException>();

            await _jobApplicationsService.GetJobApplicationsByGuidAsync("99");
        }

        [TestMethod]
        public async Task JobApplicationsService_GetJobApplicationsByGuidAsync_Expected()
        {
            var expectedResults =
                _jobApplicationsCollection.First(c => c.Guid == jobApplicationsGuid);

            var actualResult =
                await _jobApplicationsService.GetJobApplicationsByGuidAsync(jobApplicationsGuid);
            
            
            Assert.AreEqual(expectedResults.Guid, actualResult.Id);
            Assert.AreEqual(personGuid, actualResult.Person.Id);

        }

        [TestMethod]
        public async Task JobApplicationsService_GetJobApplicationsByGuidAsync_Properties()
        {
            var result =
                await _jobApplicationsService.GetJobApplicationsByGuidAsync(jobApplicationsGuid);
            Assert.IsNotNull(result.Id);
            Assert.IsNotNull(result.Person);

        }
    }
}