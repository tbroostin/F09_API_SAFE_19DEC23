/* Copyright 2017 Ellucian Company L.P. and its affiliates. */
using Ellucian.Colleague.Coordination.HumanResources.Services;
using Ellucian.Colleague.Domain.Base;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.Base.Tests;
using Ellucian.Colleague.Domain.HumanResources.Repositories;
using Ellucian.Colleague.Domain.HumanResources.Tests;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Colleague.Dtos;
using Ellucian.Colleague.HumanResources.Base.Tests;
using Ellucian.Web.Adapters;
using Ellucian.Web.Http.TestUtil;
using Ellucian.Web.Security;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.HumanResources.Tests.Services
{
    [TestClass]
    public class InstitutionEmployerServiceTests : CurrentUserSetup
    {
        public TestInstitutionEmployersRepository testInstitutionEmployersRepository;
        private Mock<IPositionRepository> positionRepoMock;
        private Mock<IHumanResourcesReferenceDataRepository> refRepoMock;
        private Mock<IReferenceDataRepository> referenceDataRepoMock;
        private Mock<IPersonRepository> personRepoMock;
        private Mock<IInstitutionEmployersRepository> institutionEmployersRepoMock;
        private Mock<IConfigurationRepository> baseConfigurationRepoMock;
        private Mock<IAdapterRegistry> adapterRegistryMock;
        private ICurrentUserFactory currentUserFactory;
        private Mock<IRoleRepository> roleRepoMock;
        private Mock<ILogger> loggerMock;
        private IEnumerable<Domain.HumanResources.Entities.InstitutionEmployers> allInstitutionEmployers;
        private InstitutionEmployersService institutionEmployerService;
        private string guid = "81fda6ce-77aa-4283-a878-75bbea227937";

        private Domain.Entities.Permission permissionViewAnyPerson;

        [TestInitialize]
        public void Initialize()
        {
            positionRepoMock = new Mock<IPositionRepository>();
            refRepoMock = new Mock<IHumanResourcesReferenceDataRepository>();
            personRepoMock = new Mock<IPersonRepository>();
            institutionEmployersRepoMock = new Mock<IInstitutionEmployersRepository>();
            referenceDataRepoMock = new Mock<IReferenceDataRepository>();
            adapterRegistryMock = new Mock<IAdapterRegistry>();
            roleRepoMock = new Mock<IRoleRepository>();
            loggerMock = new Mock<ILogger>();
            baseConfigurationRepoMock = new Mock<IConfigurationRepository>();
           
            allInstitutionEmployers = new TestInstitutionEmployersRepository().GetInstitutionEmployersAsync();
            // Set up current user
            currentUserFactory = new CurrentUserSetup.PersonUserFactory();

            // Mock permissions
            permissionViewAnyPerson = new Ellucian.Colleague.Domain.Entities.Permission(BasePermissionCodes.ViewAnyPerson);
            personRole.AddPermission(permissionViewAnyPerson);
            roleRepoMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { personRole });

            institutionEmployerService = new InstitutionEmployersService(positionRepoMock.Object, refRepoMock.Object, referenceDataRepoMock.Object,
                personRepoMock.Object, institutionEmployersRepoMock.Object, baseConfigurationRepoMock.Object, adapterRegistryMock.Object, 
                currentUserFactory, roleRepoMock.Object, loggerMock.Object);
        }

        [TestCleanup]
        public void Cleanup()
        {
            positionRepoMock = null;
            refRepoMock = null;
            referenceDataRepoMock = null;
            personRepoMock = null;
            institutionEmployersRepoMock = null;
            allInstitutionEmployers = null;
            baseConfigurationRepoMock = null;
            adapterRegistryMock = null;            
            roleRepoMock = null;
            loggerMock = null;
            institutionEmployerService = null;
        }

        [TestMethod]
        public async Task GetInstitutionEmployerByGuid_HEDM_ValidInstitutionEmployerIdAsync()
        {
            Domain.HumanResources.Entities.InstitutionEmployers thisInstitutionEmployer = allInstitutionEmployers.Where(m => m.Guid == guid).FirstOrDefault();        
            var expected = allInstitutionEmployers.FirstOrDefault();
            institutionEmployersRepoMock.Setup(repo => repo.GetInstitutionEmployerByGuidAsync(It.IsAny<string>())).ReturnsAsync(expected);
            Dtos.InstitutionEmployers institutionEmployer = await institutionEmployerService.GetInstitutionEmployersByGuidAsync(guid);
            Assert.AreEqual(thisInstitutionEmployer.Guid, institutionEmployer.Id);
            Assert.AreEqual(thisInstitutionEmployer.Code, institutionEmployer.Code);
            Assert.AreEqual(thisInstitutionEmployer.PreferredName, institutionEmployer.Title);
        }


        [TestMethod]
        public async Task GetInstitutionEmployers_HEDM_CountInstitutionEmployersAsync()
        {
            institutionEmployersRepoMock.Setup(repo => repo.GetInstitutionEmployersAsync()).ReturnsAsync(allInstitutionEmployers);
            IEnumerable<Dtos.InstitutionEmployers> institutionEmployer = await institutionEmployerService.GetInstitutionEmployersAsync();
            Assert.AreEqual(1, institutionEmployer.Count());
        }

        [TestMethod]
        public async Task GetInstitutionEmployers_HEDM_CompareInstitutionEmployersAsync()
        {
            institutionEmployersRepoMock.Setup(repo => repo.GetInstitutionEmployersAsync()).ReturnsAsync(allInstitutionEmployers);

            IEnumerable<Dtos.InstitutionEmployers> institutionEmployers = await institutionEmployerService.GetInstitutionEmployersAsync();
            Assert.AreEqual(allInstitutionEmployers.ElementAt(0).Guid, institutionEmployers.ElementAt(0).Id);
            Assert.AreEqual(allInstitutionEmployers.ElementAt(0).Code, institutionEmployers.ElementAt(0).Code);
            Assert.AreEqual(allInstitutionEmployers.ElementAt(0).PreferredName, institutionEmployers.ElementAt(0).Title);
        }
    }
}
