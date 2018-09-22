using Ellucian.Colleague.Api.Controllers.ColleagueFinance;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.ColleagueFinance.Services;
using Ellucian.Colleague.Domain.Base.Exceptions;
using Ellucian.Web.Adapters;
using Ellucian.Web.Security;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Http.Hosting;

namespace Ellucian.Colleague.Api.Tests.Controllers.ColleagueFinance
{
    [TestClass]
    public class BudgetAdjustmentsControllerTests
    {
    
        #region Test Context

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext { get; set; }

        #endregion

        private Mock<IAdapterRegistry> _adapterRegistryMock;
        private Mock<ILogger> _loggerMock;
        private Mock<IBudgetAdjustmentService> _budgetAdjustmentServiceMock;

        private HttpResponse _response;

        private BudgetAdjustmentsController _budgetAdjustmentsController;

        [TestInitialize]
        public void Initialize()
        {
            LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
            EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));

            _adapterRegistryMock = new Mock<IAdapterRegistry>();
            _loggerMock = new Mock<ILogger>();
            _budgetAdjustmentServiceMock = new Mock<IBudgetAdjustmentService>();

            
            _response = new HttpResponse(new StringWriter());

            _budgetAdjustmentsController = new BudgetAdjustmentsController(_budgetAdjustmentServiceMock.Object, _loggerMock.Object)
            {
                Request = new HttpRequestMessage() { RequestUri = new Uri("http://localhost") }
            };
            _budgetAdjustmentsController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
        }

        [TestCleanup]
        public void Cleanup()
        {
            _adapterRegistryMock = null;
            _loggerMock = null;
            _budgetAdjustmentServiceMock = null;
            _budgetAdjustmentsController = null;
        }

        [TestMethod]
        public async Task BudgetAdjustmentsController_PutAsync_Valid()
        {
            _budgetAdjustmentServiceMock.Setup(x => x.UpdateBudgetAdjustmentAsync(It.IsAny<string>(), It.IsAny<Dtos.ColleagueFinance.BudgetAdjustment>())).ReturnsAsync(new Dtos.ColleagueFinance.BudgetAdjustment());
            var budgetAdjustmentDto = await _budgetAdjustmentsController.PutAsync("1", new Dtos.ColleagueFinance.BudgetAdjustment());
            Assert.IsNotNull(budgetAdjustmentDto);
        }
        
        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task BudgetAdjustmentsController_PutAsync_PermissionsException()
        {
            _budgetAdjustmentServiceMock.Setup(x => x.UpdateBudgetAdjustmentAsync(It.IsAny<string>(), It.IsAny<Dtos.ColleagueFinance.BudgetAdjustment>())).ThrowsAsync(new PermissionsException());
            await _budgetAdjustmentsController.PutAsync("1", new Dtos.ColleagueFinance.BudgetAdjustment());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task BudgetAdjustmentsController_PutAsync_ConfigurationException()
        {
            _budgetAdjustmentServiceMock.Setup(x => x.UpdateBudgetAdjustmentAsync(It.IsAny<string>(), It.IsAny<Dtos.ColleagueFinance.BudgetAdjustment>())).ThrowsAsync(new ConfigurationException());
            await _budgetAdjustmentsController.PutAsync("1", new Dtos.ColleagueFinance.BudgetAdjustment());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task BudgetAdjustmentsController_PutAsync_Exception()
        {
            _budgetAdjustmentServiceMock.Setup(x => x.UpdateBudgetAdjustmentAsync(It.IsAny<string>(), It.IsAny<Dtos.ColleagueFinance.BudgetAdjustment>())).ThrowsAsync(new Exception());
            await _budgetAdjustmentsController.PutAsync("1", new Dtos.ColleagueFinance.BudgetAdjustment());
        }
    }
}
