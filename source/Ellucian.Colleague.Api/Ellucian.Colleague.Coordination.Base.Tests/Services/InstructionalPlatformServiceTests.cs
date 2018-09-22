// Copyright 2015 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Web.Adapters;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Web.Security;

namespace Ellucian.Colleague.Coordination.Base.Tests.Services
{
    [TestClass]
    public class InstructionalPlatformServiceTests
    {
        // Sets up a Current user that is an advisor
        public abstract class CurrentUserSetup
        {
            protected Domain.Entities.Role facultyRole = new Domain.Entities.Role(105, "Faculty");

            public class FacultyUserFactory : ICurrentUserFactory
            {
                public ICurrentUser CurrentUser
                {
                    get
                    {
                        return new CurrentUser(new Claims()
                        {
                            ControlId = "123",
                            Name = "George",
                            PersonId = "0000011",
                            SecurityToken = "321",
                            SessionTimeout = 30,
                            UserName = "Faculty",
                            Roles = new List<string>() { "Faculty" },
                            SessionFixationId = "abc123"
                        });
                    }
                }
            }
        }

        [TestClass]
        public class GetInstructionalPlatform
        {         
            private Mock<IReferenceDataRepository> _referenceDataRepositoryMock;
            private IReferenceDataRepository _referenceDataRepository;
            private Mock<IAdapterRegistry> _adapterRegistryMock;
            private IAdapterRegistry _adapterRegistry;
            private Mock<IRoleRepository> _roleRepoMock;
            private IRoleRepository _roleRepo;
            private ICurrentUserFactory _currentUserFactory;
            private ILogger _logger;
            private InstructionalPlatformService _instructionalPlatformService;
            private ICollection<Colleague.Domain.Base.Entities.InstructionalPlatform> _instructionalPlatformCollection = new List<Colleague.Domain.Base.Entities.InstructionalPlatform>();

            [TestInitialize]
            public void Initialize()
            {
                _referenceDataRepositoryMock = new Mock<IReferenceDataRepository>();
                _referenceDataRepository = _referenceDataRepositoryMock.Object;
                _adapterRegistryMock = new Mock<IAdapterRegistry>();
                _adapterRegistry = _adapterRegistryMock.Object;
                _roleRepoMock = new Mock<IRoleRepository>();
                _roleRepo = _roleRepoMock.Object;
                _logger = new Mock<ILogger>().Object;

                // Set up current user
                _currentUserFactory = new CurrentUserSetup.FacultyUserFactory();

                _instructionalPlatformCollection.Add(new Colleague.Domain.Base.Entities.InstructionalPlatform("840e72f0-57b9-42a2-ae88-df3c2262fbbc", "CE", "Continuing Education"));
                _instructionalPlatformCollection.Add(new Colleague.Domain.Base.Entities.InstructionalPlatform("e986b8a5-25f3-4aa0-bd0e-90982865e749", "D", "Institutional"));
                _instructionalPlatformCollection.Add(new Colleague.Domain.Base.Entities.InstructionalPlatform("b5cc288b-8692-474e-91be-bdc55778e2f5", "TR", "Transfer")); 
                _referenceDataRepositoryMock.Setup(repo => repo.GetInstructionalPlatformsAsync(false)).ReturnsAsync(_instructionalPlatformCollection);

                _instructionalPlatformService = new InstructionalPlatformService(_adapterRegistry, _referenceDataRepository, _currentUserFactory, _roleRepo, _logger);         
            }

            [TestCleanup]
            public void Cleanup()
            {
                _instructionalPlatformCollection = null;
                _referenceDataRepository = null;
                _instructionalPlatformService = null;               
            }

            [TestMethod]
            public async Task InstructionalPlatformService__InstructionalPlatforms()
            {
                var results = await _instructionalPlatformService.GetInstructionalPlatformsAsync();
                Assert.IsTrue(results is IEnumerable<Dtos.InstructionalPlatform>); 
                Assert.IsNotNull(results);
            }

            public async Task InstructionalPlatformService_InstructionalPlatforms_Count()
            {
                var results = await _instructionalPlatformService.GetInstructionalPlatformsAsync();
                Assert.AreEqual(3, results.Count());
            }

            [TestMethod]
            public async Task InstructionalPlatformService_InstructionalPlatforms_Properties()
            {
                var results = await _instructionalPlatformService.GetInstructionalPlatformsAsync();
                var instructionalPlatform = results.First(x => x.Code == "CE");
                Assert.IsNotNull(instructionalPlatform.Id);
                Assert.IsNotNull(instructionalPlatform.Code);
            }

            [TestMethod]
            public async Task InstructionalPlatformService_InstructionalPlatforms_Expected()
            {
                var expectedResults = _instructionalPlatformCollection.First(c => c.Code == "CE");
                var results = await _instructionalPlatformService.GetInstructionalPlatformsAsync();
                var instructionalPlatform = results.First(s => s.Code == "CE");
                Assert.AreEqual(expectedResults.Guid, instructionalPlatform.Id);
                Assert.AreEqual(expectedResults.Code, instructionalPlatform.Code);
            }


            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task InstructionalPlatformService_GetInstructionalPlatformByGuid_Empty()
            {
                await _instructionalPlatformService.GetInstructionalPlatformByGuidAsync("");
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task InstructionalPlatformService_GetInstructionalPlatformByGuid_Null()
            {
                await _instructionalPlatformService.GetInstructionalPlatformByGuidAsync(null);
            }

            [TestMethod]
            public async Task InstructionalPlatformService_GetInstructionalPlatformByGuid_Expected()
            {
                var expectedResults = _instructionalPlatformCollection.First(c => c.Guid == "840e72f0-57b9-42a2-ae88-df3c2262fbbc");
                var instructionalPlatform = await _instructionalPlatformService.GetInstructionalPlatformByGuidAsync("840e72f0-57b9-42a2-ae88-df3c2262fbbc");
                Assert.AreEqual(expectedResults.Guid, instructionalPlatform.Id);
                Assert.AreEqual(expectedResults.Code, instructionalPlatform.Code);
            }

            [TestMethod]
            public async Task InstructionalPlatformService_GetInstructionalPlatformByGuid_Properties()
            {
                var instructionalPlatform = await _instructionalPlatformService.GetInstructionalPlatformByGuidAsync("840e72f0-57b9-42a2-ae88-df3c2262fbbc");
                Assert.IsNotNull(instructionalPlatform.Id);
                Assert.IsNotNull(instructionalPlatform.Code);
            }

        }
    }
}