// Copyright 2019 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Api.Controllers.Base;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Web.Adapters;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;

namespace Ellucian.Colleague.Api.Tests.Controllers.Base
{
    [TestClass]
    public class AuthenticationSchemeControllerTests
    {
        private AuthenticationSchemeController authenticationSchemeController;
        private IAdapterRegistry adapterRegistry;
        private Mock<IAuthenticationSchemeService> authenticationSchemeServiceMock;
        private IAuthenticationSchemeService authenticationSchemeService;
        private Ellucian.Colleague.Dtos.Base.AuthenticationScheme authenticationSchemeColleagueDto;
        ILogger logger = new Mock<ILogger>().Object;

        #region Test Context

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

        #endregion

        [TestInitialize]
        public void Initialize()
        {
            LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
            EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));

            HashSet<ITypeAdapter> adapters = new HashSet<ITypeAdapter>();
            adapterRegistry = new AdapterRegistry(adapters, logger);

            authenticationSchemeServiceMock = new Mock<IAuthenticationSchemeService>();
            authenticationSchemeService = authenticationSchemeServiceMock.Object;

            authenticationSchemeColleagueDto = new Dtos.Base.AuthenticationScheme()
            {
                Code = "COLLEAGUE"
            };

            authenticationSchemeServiceMock.Setup(s => s.GetAuthenticationSchemeAsync("colleagueauthuser")).ReturnsAsync(authenticationSchemeColleagueDto);
            authenticationSchemeServiceMock.Setup(s => s.GetAuthenticationSchemeAsync("nullauthuser")).ReturnsAsync(null);
            authenticationSchemeServiceMock.Setup(s => s.GetAuthenticationSchemeAsync("notfounduser")).ThrowsAsync(new ApplicationException());

            authenticationSchemeController = new AuthenticationSchemeController(authenticationSchemeService, logger);
        }

        [TestMethod]
        public async Task GetAuthenticationSchemeAsync_UserWithColleagueAuth_Succeeds()
        {
            var actualAuthScheme = await authenticationSchemeController.GetAuthenticationSchemeAsync("colleagueauthuser");
            Assert.AreEqual(authenticationSchemeColleagueDto.Code, actualAuthScheme.Code);
        }

        [TestMethod]
        public async Task GetAuthenticationSchemeAsync_UserWithNullAuth_Succeeds()
        {
            var actualAuthScheme = await authenticationSchemeController.GetAuthenticationSchemeAsync("nullauthuser");
            Assert.IsNull(actualAuthScheme);
        }

        [TestMethod]
        public async Task GetAuthenticationSchemeAsync_ServiceThrowsException_Fails()
        {
            HttpStatusCode statusCode = HttpStatusCode.OK;
            try
            {
                await authenticationSchemeController.GetAuthenticationSchemeAsync("notfounduser");
            }
            catch (HttpResponseException e)
            {
                statusCode = e.Response.StatusCode;
            }
            Assert.AreEqual(HttpStatusCode.BadRequest, statusCode);
        }


    }
}
