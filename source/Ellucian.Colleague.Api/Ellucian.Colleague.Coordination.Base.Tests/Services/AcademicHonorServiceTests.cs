// Copyright 2017 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Colleague.Domain.Base;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.Base.Tests;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Web.Adapters;
using Ellucian.Web.Security;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.Base.Tests.Services
{
    [TestClass]
    public class AcademicHonorServiceTests 
    {
        // sets up a current user
        public abstract class CurrentUserSetup
        {
            protected Domain.Entities.Role personRole = new Domain.Entities.Role(105, "Faculty");

            public class PersonUserFactory : ICurrentUserFactory
            {
                public ICurrentUser CurrentUser
                {
                    get
                    {
                        return new CurrentUser(new Claims()
                        {
                            ControlId = "123",
                            Name = "George",
                            PersonId = "0000015",
                            SecurityToken = "321",
                            SessionTimeout = 30,
                            UserName = "Faculty",
                            Roles = new List<string>() { "Faculty" },
                            SessionFixationId = "abc123",
                        });
                    }
                }
            }
        }

        [TestClass]
        public class AcademicHonorService_Get : CurrentUserSetup
        {
            private Mock<IPersonRepository> personRepoMock;
            private Mock<IConfigurationRepository> configurationRepositoryMock;
            private IPersonRepository personRepo;
            private Mock<IReferenceDataRepository> refRepoMock;
            private IReferenceDataRepository refRepo;
            private Mock<IAdapterRegistry> adapterRegistryMock;
            private IAdapterRegistry adapterRegistry;
            private ILogger logger;
            private Mock<IRoleRepository> roleRepoMock;
            private IRoleRepository roleRepo;
            private ICurrentUserFactory currentUserFactory;
            private IEnumerable<Domain.Base.Entities.OtherHonor> allOtherHonors;
            private OtherHonorService otherHonorService;
            private string otherHonorGuid = "9ae3a175-1dfd-4937-b97b-3c9ad596e023";
           
            private Domain.Entities.Permission permissionViewAnyPerson;

            [TestInitialize]
            public void Initialize() {
                personRepoMock = new Mock<IPersonRepository>();
                configurationRepositoryMock = new Mock<IConfigurationRepository>();
                personRepo = personRepoMock.Object;
                refRepoMock = new Mock<IReferenceDataRepository>();
                refRepo = refRepoMock.Object;
                adapterRegistryMock = new Mock<IAdapterRegistry>();
                adapterRegistry = adapterRegistryMock.Object;
                roleRepoMock = new Mock<IRoleRepository>();
                roleRepo = roleRepoMock.Object;
                logger = new Mock<ILogger>().Object;

                allOtherHonors = new TestAcademicHonorsRepository().GetOtherHonors();
               

                adapterRegistryMock = new Mock<IAdapterRegistry>();
                adapterRegistry = adapterRegistryMock.Object;

                // Set up current user
                currentUserFactory = new CurrentUserSetup.PersonUserFactory();

                // Mock permissions
                permissionViewAnyPerson = new Ellucian.Colleague.Domain.Entities.Permission(BasePermissionCodes.ViewAnyPerson);
                personRole.AddPermission(permissionViewAnyPerson);
                roleRepoMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { personRole });

                otherHonorService = new OtherHonorService(adapterRegistry, refRepo, personRepo, configurationRepositoryMock.Object, currentUserFactory, roleRepo, logger);
            }

            [TestCleanup]
            public void Cleanup() {
                refRepo = null;
                personRepo = null;
                allOtherHonors = null;
                adapterRegistry = null;
                roleRepo = null;
                logger = null;
                otherHonorService = null;
            }

            [TestMethod]
            public async Task GetAcademicHonorByGuid_ValidMajorGuidAsync()
            {
               Domain.Base.Entities.OtherHonor thisOtherHonor = allOtherHonors.Where(m => m.Guid == otherHonorGuid).FirstOrDefault();
               refRepoMock.Setup(repo => repo.GetOtherHonorsAsync(true)).ReturnsAsync(allOtherHonors.Where(m => m.Guid == otherHonorGuid));
                Dtos.OtherHonor otherHonor = await otherHonorService.GetOtherHonorByGuidAsync(otherHonorGuid);
                Assert.AreEqual(thisOtherHonor.Guid, otherHonor.Id);
                Assert.AreEqual(thisOtherHonor.Code, otherHonor.Code);
                Assert.AreEqual(null, otherHonor.Description);
               // Assert.AreEqual("Major", otherHonor.GetType.ToString());
            }

            
            [TestMethod]
            public async Task GetAcademicHonors_CountOtherHonorsAsync()
            {
                refRepoMock.Setup(repo => repo.GetOtherHonorsAsync(false)).ReturnsAsync(allOtherHonors);
                IEnumerable<Ellucian.Colleague.Dtos.OtherHonor> otherHonor = await otherHonorService.GetOtherHonorsAsync(false);
                Assert.AreEqual(2, otherHonor.Count());
            }

            [TestMethod]
            public async Task GetAcademicHonors_CompareAcademicDisciplinesMajorsAsync()
            {
                refRepoMock.Setup(repo => repo.GetOtherHonorsAsync(false)).ReturnsAsync(allOtherHonors);

                IEnumerable<Dtos.OtherHonor> otherHonors = await otherHonorService.GetOtherHonorsAsync(false);
                Assert.AreEqual(allOtherHonors.ElementAt(0).Guid, otherHonors.ElementAt(0).Id);
                Assert.AreEqual(allOtherHonors.ElementAt(0).Code, otherHonors.ElementAt(0).Code);
                Assert.AreEqual(null, otherHonors.ElementAt(0).Description);
                //Assert.AreEqual("Major", academicDiscipline.ElementAt(0).Type.ToString());
            }
        }
    }
}
