// Copyright 2015-2018 Ellucian Company L.P. and its affiliates.
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Hosting;
using AutoMapper;
using Ellucian.Colleague.Api.Controllers.Base;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.Base.Tests;
using Ellucian.Web.Adapters;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;


namespace Ellucian.Colleague.Api.Tests.Controllers.Base
{
    [TestClass]
    public class GradeChangeReasonsControllerTest
    {
        private TestContext testContextInstance;

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }

        private Mock<IAdapterRegistry> AdapterRegistryMock;
        private IAdapterRegistry AdapterRegistry;
        private Mock<IReferenceDataRepository> ReferenceDataRepositoryMock;
        private IReferenceDataRepository ReferenceDataRepository;

        private Mock<IGradeChangeReasonService> GradeChangeReasonServiceMock;
        private IGradeChangeReasonService GradeChangeReasonService;
        private ILogger logger = new Mock<ILogger>().Object;

        private Mock<GradeChangeReasonsController> GradeChangeReasonsControllerMock;
        GradeChangeReasonsController GradeChangeReasonsController;
        private IEnumerable<Ellucian.Colleague.Domain.Base.Entities.GradeChangeReason> allGradeChangeReasonEntities;
        private List<Dtos.GradeChangeReason> allGradeChangeReasonDtos = new List<Dtos.GradeChangeReason>();
        private string gradeChangeReasonGuid = "bf775687-6dfe-42ef-b7c0-aee3d9e681cf";

        [TestInitialize]
        public void Initialize()
        {
            LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
            EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));
            ReferenceDataRepositoryMock = new Mock<IReferenceDataRepository>();
            ReferenceDataRepository = ReferenceDataRepositoryMock.Object;

            AdapterRegistryMock = new Mock<IAdapterRegistry>();
            AdapterRegistry = AdapterRegistryMock.Object;

            GradeChangeReasonServiceMock = new Mock<IGradeChangeReasonService>();
            GradeChangeReasonService = GradeChangeReasonServiceMock.Object;

            var testAdapter = new AutoMapperAdapter<Ellucian.Colleague.Domain.Base.Entities.GradeChangeReason, Ellucian.Colleague.Dtos.GradeChangeReason>(AdapterRegistry, logger);
            AdapterRegistry.AddAdapter(testAdapter);
            AdapterRegistryMock.Setup(x => x.GetAdapter<Domain.Base.Entities.GradeChangeReason, Dtos.GradeChangeReason>()).Returns(testAdapter);

            allGradeChangeReasonEntities = new TestGradeChangeReasonRepository().Get();

            Mapper.CreateMap<Ellucian.Colleague.Domain.Base.Entities.GradeChangeReason, Dtos.GradeChangeReason>();
            foreach(var gradeChangeReason in allGradeChangeReasonEntities)
            {
                Dtos.GradeChangeReason target = Mapper.Map<Ellucian.Colleague.Domain.Base.Entities.GradeChangeReason, Dtos.GradeChangeReason>(gradeChangeReason);
                target.Id = gradeChangeReason.Guid;
                target.Title = gradeChangeReason.Description;

                allGradeChangeReasonDtos.Add(target);
            }

            ReferenceDataRepositoryMock.Setup(x => x.GetGradeChangeReasonAsync(false)).ReturnsAsync(allGradeChangeReasonEntities);

            GradeChangeReasonsControllerMock = new Mock<Api.Controllers.Base.GradeChangeReasonsController>();

            GradeChangeReasonsController = new GradeChangeReasonsController(AdapterRegistry, GradeChangeReasonService, logger);
            GradeChangeReasonsController.Request = new HttpRequestMessage();
            GradeChangeReasonsController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
        }

        [TestCleanup]
        public void Cleanup()
        {
            GradeChangeReasonsController = null;
            ReferenceDataRepository = null;
            allGradeChangeReasonEntities = null;
        }

        [TestMethod]
        public async Task GradeChangeReasonController_GetGradeChangeReasonsAsync()
        {
            GradeChangeReasonServiceMock.Setup(x => x.GetAsync(false)).ReturnsAsync(allGradeChangeReasonDtos);

            var allGradeChangeReasons = (await GradeChangeReasonsController.GetGradeChangeReasonsAsync()).ToList();
            Assert.AreEqual(allGradeChangeReasonDtos.Count, allGradeChangeReasons.Count);

            for(int i = 0; i < allGradeChangeReasons.Count; i++)
            {
                var expected = allGradeChangeReasonDtos[i];
                var actual = allGradeChangeReasons[i];

                Assert.AreEqual(expected.Title, actual.Title);
            }
        }

        [TestMethod]
        public async Task GradeChangeReasonController_GetGradeChangeReasonById()
        {
            var gradeChangeReason = allGradeChangeReasonDtos.Where(i => i.Id == gradeChangeReasonGuid).FirstOrDefault();

            GradeChangeReasonServiceMock.Setup(x => x.GetGradeChangeReasonByIdAsync(gradeChangeReasonGuid)).ReturnsAsync(gradeChangeReason);

            var expected = (await GradeChangeReasonsController.GetGradeChangeReasonByIdAsync(gradeChangeReasonGuid));

            Assert.AreEqual(expected.Code, allGradeChangeReasonDtos[0].Code);
            Assert.AreEqual(expected.Description, allGradeChangeReasonDtos[0].Description);
            Assert.AreEqual(expected.Id, allGradeChangeReasonDtos[0].Id);
            Assert.AreEqual(expected.Title, allGradeChangeReasonDtos[0].Title);

        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task GradeChangeReasonController_GetGradeChangeReasonByNullId()
        {
            await GradeChangeReasonsController.GetGradeChangeReasonByIdAsync(null);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task GradeChangeReasonController_PostThrowsIntAppiExc()
        {
            var result = await GradeChangeReasonsController.PostGradeChangeReasonAsync(allGradeChangeReasonDtos[0]);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task GradeChangeReasonController_PustThrowsIntAppiExc()
        {
            var result = await GradeChangeReasonsController.PutGradeChangeReasonAsync( gradeChangeReasonGuid, allGradeChangeReasonDtos[0]);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task GradeChangeReasonController_DeleteThrowsIntAppiExc()
        {
            var result = await GradeChangeReasonsController.DeleteGradeChangeReasonByIdAsync(gradeChangeReasonGuid);
        }
    }
}
