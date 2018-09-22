// Copyright 2017-2018 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;
using Ellucian.Colleague.Api.Controllers.Base;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Web.Adapters;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;

namespace Ellucian.Colleague.Api.Tests.Controllers.Base
{
    [TestClass]
    public class OrganizationalPersonPositionsControllerTests
    {
        private OrganizationalPersonPositionsController organizationalPersonPositionsController;
        private IAdapterRegistry adapterRegistry;
        private Mock<IOrganizationalPersonPositionService> organizationalPersonPositionServiceMock;
        private IOrganizationalPersonPositionService organizationalPersonPositionService;
        private Ellucian.Colleague.Dtos.Base.OrganizationalPersonPosition organizationalPersonPositionDto;
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

            organizationalPersonPositionServiceMock = new Mock<IOrganizationalPersonPositionService>();
            organizationalPersonPositionService = organizationalPersonPositionServiceMock.Object;

            organizationalPersonPositionDto = new Dtos.Base.OrganizationalPersonPosition();

            organizationalPersonPositionServiceMock.Setup(s => s.GetOrganizationalPersonPositionByIdAsync(It.IsAny<string>())).ReturnsAsync(organizationalPersonPositionDto);

            organizationalPersonPositionsController = new OrganizationalPersonPositionsController(organizationalPersonPositionService, logger);
        }

        [TestMethod]
        public async Task GetOrganizationalPersonPositionAsync_Succeeds()
        {
            organizationalPersonPositionDto.Id = "OPP1";
            organizationalPersonPositionDto.PersonId = "P1";
            organizationalPersonPositionDto.PersonName = "Brick Jones";
            organizationalPersonPositionDto.PositionId = "POS1";
            organizationalPersonPositionDto.PositionTitle = "DEAN OF BUSINESS";
            organizationalPersonPositionDto.Relationships = new List<Dtos.Base.OrganizationalRelationship>();

            var personPositions = await organizationalPersonPositionsController.GetOrganizationalPersonPositionAsync("OPP1");
            Assert.IsTrue(personPositions is Dtos.Base.OrganizationalPersonPosition);
        }

        [TestMethod]
        public async Task GetOrganizationalPersonPositionAsync_Fails()
        {
            HttpStatusCode statusCode = HttpStatusCode.Unused;
            organizationalPersonPositionServiceMock.Setup(s => s.GetOrganizationalPersonPositionByIdAsync("OPP0")).Throws(new InvalidOperationException());
            try
            {
                await organizationalPersonPositionsController.GetOrganizationalPersonPositionAsync("OPP0");
            }
            catch (HttpResponseException e)
            {
                statusCode = e.Response.StatusCode;
            }
            Assert.AreEqual(HttpStatusCode.BadRequest, statusCode);
        }

    }
}
