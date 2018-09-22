using Ellucian.Colleague.Coordination.HumanResources.Adapters;
using Ellucian.Colleague.Coordination.HumanResources.Services;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.Entities;
using Ellucian.Colleague.Domain.HumanResources.Entities;
using Ellucian.Colleague.Domain.HumanResources.Services;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Web.Adapters;
using Ellucian.Web.Security;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.HumanResources.Tests.Services
{
    
    [TestClass]
    public class FacultyContractServiceTests :CurrentUserSetup
    {
        private Mock<IFacultyContractDomainService> facContractDomainServMock;
        private Mock<IAdapterRegistry> adapterRegistryMock;
        private Mock<ICurrentUserFactory> currentUserFactoryMock;
        private Mock<IRoleRepository> roleRepoMock;
        private Mock<IConfigurationRepository> configurationRepoMock;
        private Mock<IStaffRepository> staffRepoMock;

        private ILogger logger;
        private IFacultyContractDomainService facContractDomainServ;
        private IAdapterRegistry adapterRegistry;
        private ICurrentUserFactory currentUserFactory;
        private IRoleRepository roleRepo;
        private IConfigurationRepository configurationRepo;
        private IStaffRepository staffRepo;
        private FacultyContractService facultyContractService;


        private string facultyId = "0000015";
        private FacultyContract contract1;

        [TestInitialize]
        public void Initialize()
        {
            facContractDomainServMock = new Mock<IFacultyContractDomainService>();
            adapterRegistryMock = new Mock<IAdapterRegistry>();
            roleRepoMock = new Mock<IRoleRepository>();
            currentUserFactoryMock = new Mock<ICurrentUserFactory>();
            configurationRepoMock = new Mock<IConfigurationRepository>();
            staffRepoMock = new Mock<IStaffRepository>();

            facContractDomainServ = facContractDomainServMock.Object;
            adapterRegistry = adapterRegistryMock.Object;
            roleRepo = roleRepoMock.Object;
            currentUserFactory = currentUserFactoryMock.Object;
            configurationRepo = configurationRepoMock.Object;
            staffRepo = staffRepoMock.Object;
            logger = new Mock<ILogger>().Object;
            // Set up a current user
            currentUserFactory = new CurrentUserSetup.PersonUserFactory();

            contract1 = new Domain.HumanResources.Entities.FacultyContract(facultyId, "this is a real desc", "this is a number", "faculty", new DateTime(01 / 01 / 1991), new DateTime(02 / 02 / 1991), "load period id", null, null);
            facultyContractService = new FacultyContractService(facContractDomainServ, adapterRegistry, currentUserFactory, roleRepo, logger, staffRepo, configurationRepo);

            var facultyContractToDtoAdapter = new AutoMapperAdapter<Domain.HumanResources.Entities.FacultyContract, Dtos.Base.FacultyContract>(adapterRegistry, logger);
            adapterRegistryMock.Setup(ar => ar.GetAdapter<Domain.HumanResources.Entities.FacultyContract, Dtos.Base.FacultyContract>()).Returns(facultyContractToDtoAdapter);
        }

        [TestCleanup]
        public void Cleanup()   
        {
            facContractDomainServ = null;
            roleRepo = null;
            logger = null;
            adapterRegistry = null;
            staffRepo = null;
            facultyContractService = null;
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task FacultyContractService_GetFacultyContractsByFacultyIds_FacultyIdNullCheck()
        {
            await facultyContractService.GetFacultyContractsByFacultyIdAsync(null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task FacultyContractService_GetFacultyContractsByFacultyId_FacultyIdEmptyStringCheck()
        {
           var facultyId = string.Empty;
            await facultyContractService.GetFacultyContractsByFacultyIdAsync(facultyId);
        }

        [TestMethod]
        [ExpectedException(typeof(PermissionsException))]
        public async Task FacultyContractService_GetFacultyContractsByFacultyId_PermissionCheck()
        {
            var facultyId = "this is a fake ID";
            await facultyContractService.GetFacultyContractsByFacultyIdAsync(facultyId);

        }

        [TestMethod]
        public async Task FacultyContractService_GetFacultyContractsByFacultyId_ContractCheck()
        {
            facContractDomainServMock.Setup(serv => serv.GetFacultyContractsByFacultyIdAsync(facultyId)).ReturnsAsync(new List<Domain.HumanResources.Entities.FacultyContract>() { contract1 });
            var contracts = await facultyContractService.GetFacultyContractsByFacultyIdAsync(facultyId);
            Assert.IsTrue(contracts.Count() == 1);
            var contractResult = contracts.ElementAt(0);
            Assert.AreEqual(facultyId, contractResult.Id);
            Assert.AreEqual(contract1.ContractDescription, contractResult.ContractDescription);
        }
    }
}
