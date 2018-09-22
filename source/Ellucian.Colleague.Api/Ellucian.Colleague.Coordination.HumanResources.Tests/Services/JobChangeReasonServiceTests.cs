/* Copyright 2016 Ellucian Company L.P. and its affiliates. */
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
    public class JobChangeReasonServiceTests : CurrentUserSetup
    {
        private Mock<IPersonRepository> personRepoMock;
        private IPersonRepository personRepo;
        private Mock<IHumanResourcesReferenceDataRepository> refRepoMock;
        private IHumanResourcesReferenceDataRepository refRepo;
        private Mock<IAdapterRegistry> adapterRegistryMock;
        private IAdapterRegistry adapterRegistry;
        private ILogger logger;
        private Mock<IRoleRepository> roleRepoMock;
        private IRoleRepository roleRepo;
        private ICurrentUserFactory currentUserFactory;
        private IEnumerable<Domain.HumanResources.Entities.JobChangeReason> allJobChangeReasons;
        private JobChangeReasonService jobChangeReasonService;
        private string guid = "625c69ff-280b-4ed3-9474-662a43616a8a";

        private Domain.Entities.Permission permissionViewAnyPerson;

        [TestInitialize]
        public void Initialize()
        {
            personRepoMock = new Mock<IPersonRepository>();
            personRepo = personRepoMock.Object;
            refRepoMock = new Mock<IHumanResourcesReferenceDataRepository>();
            refRepo = refRepoMock.Object;
            adapterRegistryMock = new Mock<IAdapterRegistry>();
            adapterRegistry = adapterRegistryMock.Object;
            roleRepoMock = new Mock<IRoleRepository>();
            roleRepo = roleRepoMock.Object;
            logger = new Mock<ILogger>().Object;

            allJobChangeReasons = new TestJobChangeReasonRepository().GetJobChangeReasons();

            // Set up current user
            currentUserFactory = new CurrentUserSetup.PersonUserFactory();

            // Mock permissions
            permissionViewAnyPerson = new Ellucian.Colleague.Domain.Entities.Permission(BasePermissionCodes.ViewAnyPerson);
            personRole.AddPermission(permissionViewAnyPerson);
            roleRepoMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { personRole });

            jobChangeReasonService = new JobChangeReasonService(refRepo, adapterRegistry, currentUserFactory, roleRepo, logger);
        }

        [TestCleanup]
        public void Cleanup()
        {
            refRepo = null;
            personRepo = null;
            allJobChangeReasons = null;
            adapterRegistry = null;
            roleRepo = null;
            logger = null;
            jobChangeReasonService = null;
        }

        [TestMethod]
        public async Task GetJobChangeReasonByGuid_HEDM_ValidJobChangeReasonIdAsync()
        {
            Domain.HumanResources.Entities.JobChangeReason thisJobChangeReason = allJobChangeReasons.Where(m => m.Guid == guid).FirstOrDefault();
            refRepoMock.Setup(repo => repo.GetJobChangeReasonsAsync(true)).ReturnsAsync(allJobChangeReasons.Where(m => m.Guid == guid));
            Dtos.JobChangeReason jobChangeReason = await jobChangeReasonService.GetJobChangeReasonByGuidAsync(guid);
            Assert.AreEqual(thisJobChangeReason.Guid, jobChangeReason.Id);
            Assert.AreEqual(thisJobChangeReason.Code, jobChangeReason.Code);
            Assert.AreEqual(null, jobChangeReason.Description);
            Assert.AreEqual(thisJobChangeReason.Description, jobChangeReason.Title);
        }


        [TestMethod]
        public async Task GetJobChangeReasons_HEDM_CountJobChangeReasonsAsync()
        {
            refRepoMock.Setup(repo => repo.GetJobChangeReasonsAsync(false)).ReturnsAsync(allJobChangeReasons);
            IEnumerable<Ellucian.Colleague.Dtos.JobChangeReason> jobChangeReason = await jobChangeReasonService.GetJobChangeReasonsAsync();
            Assert.AreEqual(4, jobChangeReason.Count());
        }

        [TestMethod]
        public async Task GetJobChangeReasons_HEDM_CompareJobChangeReasonsAsync()
        {
            refRepoMock.Setup(repo => repo.GetJobChangeReasonsAsync(false)).ReturnsAsync(allJobChangeReasons);

            IEnumerable<Dtos.JobChangeReason> jobChangeReasons = await jobChangeReasonService.GetJobChangeReasonsAsync();
            Assert.AreEqual(allJobChangeReasons.ElementAt(0).Guid, jobChangeReasons.ElementAt(0).Id);
            Assert.AreEqual(allJobChangeReasons.ElementAt(0).Code, jobChangeReasons.ElementAt(0).Code);
            Assert.AreEqual(null, jobChangeReasons.ElementAt(0).Description);
            Assert.AreEqual(allJobChangeReasons.ElementAt(0).Description, jobChangeReasons.ElementAt(0).Title);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public async Task DemographicService_GetJobChangeReasonByGuid_HEDM_ThrowsInvOpExc()
        {
            refRepoMock.Setup(repo => repo.GetJobChangeReasonsAsync(It.IsAny<bool>())).Throws<InvalidOperationException>();
            await jobChangeReasonService.GetJobChangeReasonByGuidAsync("dshjfkj");
        }
    }
}
