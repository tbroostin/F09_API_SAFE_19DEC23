// Copyright 2015-2016 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Colleague.Domain.Base.Entities;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.Base.Tests;
using Ellucian.Colleague.Domain.Entities;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Web.Adapters;
using Ellucian.Web.Security;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;

namespace Ellucian.Colleague.Coordination.Base.Tests.Services
{
    [TestClass]
    public class AcademicCredentialServiceTests
    {
        // sets up a current user
        public abstract class CurrentUserSetup
        {
            protected Role PersonRole = new Role(105, "Faculty");

            public class PersonUserFactory : ICurrentUserFactory
            {
                public ICurrentUser CurrentUser
                {
                    get
                    {
                        return new CurrentUser(new Claims
                        {
                            ControlId = "123",
                            Name = "George",
                            PersonId = "0000015",
                            SecurityToken = "321",
                            SessionTimeout = 30,
                            UserName = "Faculty",
                            Roles = new List<string> {"Faculty"},
                            SessionFixationId = "abc123"
                        });
                    }
                }
            }
        }

        [TestClass]
        public class AcademicCredentialServiceGet : CurrentUserSetup
        {
            private readonly string _otherCcdGuid = "72b7737b-27db-4a06-944b-97d00c29b3db";
            private readonly string _otherDegreeGuid = "dd0c42ca-c61d-4ca6-8d21-96ab5be35623";
            
            private AcademicCredentialService _academicCredentialService;
            private IAdapterRegistry _adapterRegistry;
            private Mock<IAdapterRegistry> _adapterRegistryMock;
            private ICurrentUserFactory _currentUserFactory;
            private ILogger _logger;
            private IReferenceDataRepository _refRepo;
            private Mock<IReferenceDataRepository> _refRepoMock;
            private IRoleRepository _roleRepo;
            private Mock<IRoleRepository> _roleRepoMock;
 
            private IEnumerable<OtherCcd> _allCcds;
            private IEnumerable<OtherDegree> _allDegrees;
            private IEnumerable<AcadCredential> _allAcadCredentials;
            private IConfigurationRepository baseConfigurationRepository;
            private Mock<IConfigurationRepository> baseConfigurationRepositoryMock;

          
            [TestInitialize]
            public void Initialize()
            {
                _adapterRegistryMock = new Mock<IAdapterRegistry>();
                _adapterRegistry = _adapterRegistryMock.Object;
                _refRepoMock = new Mock<IReferenceDataRepository>();
                _refRepo = _refRepoMock.Object;
                _adapterRegistryMock = new Mock<IAdapterRegistry>();
                _adapterRegistry = _adapterRegistryMock.Object;
                _roleRepoMock = new Mock<IRoleRepository>();
                _roleRepo = _roleRepoMock.Object;
                _logger = new Mock<ILogger>().Object;
                baseConfigurationRepositoryMock = new Mock<IConfigurationRepository>();
                baseConfigurationRepository = baseConfigurationRepositoryMock.Object;

                _allDegrees = new TestAcademicCredentialsRepository().GetOtherDegrees();
                _allCcds = new TestAcademicCredentialsRepository().GetOtherCcds();
                _allAcadCredentials = new TestAcademicCredentialsRepository().GetAcadCredentials();

              
                // Set up current user
                _currentUserFactory = new PersonUserFactory();
             
                _refRepoMock.Setup(repo => repo.GetOtherDegreesAsync(It.IsAny<bool>())).ReturnsAsync(_allDegrees);
                _refRepoMock.Setup(repo => repo.GetOtherCcdsAsync(It.IsAny<bool>())).ReturnsAsync(_allCcds);

                _refRepoMock.Setup(repo => repo.GetAcadCredentialsAsync(It.IsAny<bool>())).ReturnsAsync(_allAcadCredentials);

                _academicCredentialService = new AcademicCredentialService(_adapterRegistry, _refRepo, baseConfigurationRepository, _currentUserFactory, _roleRepo, _logger);
            }

            [TestCleanup]
            public void Cleanup()
            {
                _refRepo = null;
                _allDegrees = null;
                _allCcds = null;
                _adapterRegistry = null;
                _roleRepo = null;
                _logger = null;
                _academicCredentialService = null;
            }

            [TestMethod]
            [ExpectedException(typeof (KeyNotFoundException))]
            public async Task GetAcademicCredentialByGuid_InvalidHonorGuid()
            {
                await _academicCredentialService.GetAcademicCredentialByGuidAsync("");
            }

            [TestMethod]
            [ExpectedException(typeof (KeyNotFoundException))]
            public async Task GetAcademicCredentialByGuid_InvalidOtherHonor()
            {

                await _academicCredentialService.GetAcademicCredentialByGuidAsync("999999999");
            }


            [TestMethod]
            public async Task GetAcademicCredentialByGuid_ValidDegreeGuid()
            {
                var expected = _allAcadCredentials.FirstOrDefault(m => m.Guid == _otherDegreeGuid);
                var actual = await _academicCredentialService.GetAcademicCredentialByGuidAsync(_otherDegreeGuid);
                
                Assert.IsNotNull(expected);
                Assert.IsNotNull(actual);

                Assert.IsNotNull((expected));
                Assert.AreEqual(expected.Guid, actual.Id);
                Assert.AreEqual(expected.Code, actual.Abbreviation);
                Assert.AreEqual(expected.Description, actual.Title);
            }

            [TestMethod]
            public async Task GetAcademicCredentialByGuid_ValidCcdGuid()
            {
                var expected = _allAcadCredentials.FirstOrDefault(m => m.Guid == _otherCcdGuid);
                var actual = await _academicCredentialService.GetAcademicCredentialByGuidAsync(_otherCcdGuid);

                Assert.IsNotNull(expected);
                Assert.IsNotNull(actual);

                Assert.AreEqual(expected.Guid, actual.Id);
                Assert.AreEqual(expected.Code, actual.Abbreviation);
                Assert.AreEqual(expected.Description, actual.Title);
            }

            [TestMethod]
            public async Task GetAcademicCredentials_CountAcademicCredentials()
            {
                var count = _allAcadCredentials.Count();
                var academicCredential = await _academicCredentialService.GetAcademicCredentialsAsync();
                Assert.AreEqual(count, academicCredential.Count());
            }

            [TestMethod]
            public async Task GetAcademicCredentials_CompareAcademicCredentialsDegrees()
            {
                var academicCredential = await _academicCredentialService.GetAcademicCredentialsAsync();
                var expected = _allAcadCredentials.FirstOrDefault(x => x.Guid.Equals(_otherDegreeGuid, StringComparison.OrdinalIgnoreCase));
                var actual = academicCredential.FirstOrDefault(x => x.Id.Equals(_otherDegreeGuid, StringComparison.OrdinalIgnoreCase));

                Assert.IsNotNull(expected);
                Assert.IsNotNull(actual);

                Assert.AreEqual(expected.Guid, actual.Id);
                Assert.AreEqual(expected.Code, actual.Abbreviation);
                Assert.AreEqual(expected.Description, actual.Title);

            }

            [TestMethod]
            public async Task GetAcademicCredentials_CompareAcademicCredentialsCcds()
            {
                 var academicCredential = await _academicCredentialService.GetAcademicCredentialsAsync();

                var expected = _allAcadCredentials.FirstOrDefault(x => x.Guid.Equals(_otherCcdGuid, StringComparison.OrdinalIgnoreCase));
                var actual = academicCredential.FirstOrDefault(x => x.Id.Equals(_otherCcdGuid, StringComparison.OrdinalIgnoreCase));

                Assert.IsNotNull(expected);
                Assert.IsNotNull(actual);

                Assert.AreEqual(expected.Guid, actual.Id);
                Assert.AreEqual(expected.Code, actual.Abbreviation);
                Assert.AreEqual(expected.Description, actual.Title);
            }
        }
    }
}