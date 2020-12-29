// Copyright 2016-2019 Ellucian Company L.P. and its affiliates.

using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Hosting;
using Ellucian.Colleague.Api.Controllers.Base;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Web.Adapters;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;


namespace Ellucian.Colleague.Api.Tests.Controllers.Base
{
    [TestClass]
    public class GradeModesControllerTest
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

        private Mock<GradeModesController> GradeModesControllerMock;
        GradeModesController GradeModesController;
        private string gradeModeGuid = "bf775687-6dfe-42ef-b7c0-aee3d9e681cf";

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

            GradeModesControllerMock = new Mock<Api.Controllers.Base.GradeModesController>();

            GradeModesController = new GradeModesController(logger);
            GradeModesController.Request = new HttpRequestMessage();
            GradeModesController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
        }

        [TestCleanup]
        public void Cleanup()
        {
            GradeModesController = null;
            ReferenceDataRepository = null;
        }


        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task GradeModeController_PostThrowsIntAppiExc()
        {
            var result = await GradeModesController.PostGradeModeAsync(new Dtos.GradeMode { });
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task GradeModeController_PutThrowsIntAppiExc()
        {
            var result = await GradeModesController.PutGradeModeAsync(gradeModeGuid, new Dtos.GradeMode { });
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task GradeModeController_DeleteThrowsIntAppiExc()
        {
            var result = await GradeModesController.DeleteGradeModeByIdAsync(gradeModeGuid);
        }
    }
}
