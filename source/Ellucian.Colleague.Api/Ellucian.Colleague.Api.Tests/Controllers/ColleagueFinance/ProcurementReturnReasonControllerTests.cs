using Ellucian.Colleague.Api.Controllers.ColleagueFinance;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.ColleagueFinance.Services;
using Ellucian.Colleague.Dtos.ColleagueFinance;
using Ellucian.Web.Security;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Newtonsoft.Json.Linq;
using slf4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Hosting;

namespace Ellucian.Colleague.Api.Tests.Controllers.ColleagueFinance
{
    [TestClass]
    public class ProcurementReturnReasonControllerTests
    {
        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        ///

        #region DECLARATION
        public TestContext TestContext { get; set; }
        private Mock<IProcurementReturnReasonService> procurementReturnReasonServiceMock;
        private Mock<ILogger> loggerMock;
        private ProcurementReturnReasonController procurementReturnReasonController;
        private IEnumerable<Domain.Base.Entities.ItemConditions> returnReasonList;
        private List<ProcurementReturnReason> returnReasonCollection;

        #endregion

        #region TEST SETUP
        [TestInitialize]
        public void Initialize()
        {
            LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
            EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.DeploymentDirectory, "App_Data"));

            procurementReturnReasonServiceMock = new Mock<IProcurementReturnReasonService>();
            loggerMock = new Mock<ILogger>();

            returnReasonCollection = new List<Dtos.ColleagueFinance.ProcurementReturnReason>();

            InitializeTestData();
            
            procurementReturnReasonServiceMock.Setup(p => p.GetProcurementReturnReasonsAsync()).ReturnsAsync(returnReasonCollection);

            procurementReturnReasonController = new ProcurementReturnReasonController(procurementReturnReasonServiceMock.Object, loggerMock.Object)
            {
                Request = new HttpRequestMessage()
            };

            procurementReturnReasonController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };
            procurementReturnReasonController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
        }

        [TestCleanup]
        public void Cleanup()
        {
            procurementReturnReasonController = null;
            procurementReturnReasonServiceMock = null;
            loggerMock = null;
            TestContext = null;
            returnReasonList = null;
            returnReasonCollection = null;
        }

        private void InitializeTestData()
        {

            returnReasonCollection = new List<ProcurementReturnReason>();
            returnReasonList = new List<Domain.Base.Entities.ItemConditions>(){
                    new Domain.Base.Entities.ItemConditions("BR","Broken"),
                    new Domain.Base.Entities.ItemConditions("CR","Crushed")
            };

            foreach (var source in returnReasonList)
            {
                var returnReasonCode = new ProcurementReturnReason
                {
                    Code = source.Code,
                    Description = source.Description
                };
                returnReasonCollection.Add(returnReasonCode);
            }
            
            
        }
        #endregion

        #region TEST METHODS

        [TestMethod]
        public async Task ProcurementReturnReasonController_GetProcurementReturnReasonsAsync_ValidTests()
        {
            var sourceContexts = (await procurementReturnReasonController.GetProcurementReturnReasonsAsync()).ToList();
            Assert.AreEqual(returnReasonCollection.Count, sourceContexts.Count);
            for (var i = 0; i < sourceContexts.Count; i++)
            {
                var expected = returnReasonCollection[i];
                var actual = sourceContexts[i];
                Assert.AreEqual(expected.Code, actual.Code, "Code, Index=" + i.ToString());
                Assert.AreEqual(expected.Description, actual.Description, "Description, Index=" + i.ToString());
            }
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task ProcurementReturnReasonController_GetProcurementReturnReasonsAsync_PermissionException()
        {
            procurementReturnReasonServiceMock.Setup(r => r.GetProcurementReturnReasonsAsync()).ThrowsAsync(new PermissionsException());

            await procurementReturnReasonController.GetProcurementReturnReasonsAsync();
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task ProcurementReturnReasonController_GetProcurementReturnReasonsAsync_KeyNotFoundException()
        {
            procurementReturnReasonServiceMock.Setup(r => r.GetProcurementReturnReasonsAsync()).ThrowsAsync(new KeyNotFoundException());

            await procurementReturnReasonController.GetProcurementReturnReasonsAsync();
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task ProcurementReturnReasonController_GetProcurementReturnReasonsAsync_ArgumentNullException()
        {
            procurementReturnReasonServiceMock.Setup(r => r.GetProcurementReturnReasonsAsync()).ThrowsAsync(new ArgumentNullException());

            await procurementReturnReasonController.GetProcurementReturnReasonsAsync();
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task ProcurementReturnReasonController_GetProcurementReturnReasonsAsync_Exception()
        {
            procurementReturnReasonServiceMock.Setup(r => r.GetProcurementReturnReasonsAsync()).ThrowsAsync(new Exception());
            await procurementReturnReasonController.GetProcurementReturnReasonsAsync();
        }
        
        #endregion

    }
}
