// Copyright 2017-2018 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;
using Ellucian.Colleague.Api.Controllers;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Web.Adapters;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;

namespace Ellucian.Colleague.Api.Tests.Controllers.Base
{
    [TestClass]
    public class OrganizationalRelationshipsControllerTests
    {
        private OrganizationalRelationshipsController organizationalRelationshipsController;
        private IAdapterRegistry adapterRegistry;
        private Mock<IOrganizationalPersonPositionService> organizationalPersonPositionServiceMock;
        private IOrganizationalPersonPositionService organizationalPersonPositionService;
        private Mock<IOrganizationalRelationshipService> organizationalRelationshipServiceMock;
        private IOrganizationalRelationshipService organizationalRelationshipService;
        private Ellucian.Colleague.Dtos.Base.OrganizationalRelationship organizationalRelationshipDto;
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

            organizationalRelationshipServiceMock = new Mock<IOrganizationalRelationshipService>();
            organizationalRelationshipService = organizationalRelationshipServiceMock.Object;

            // setup organizationalRelationshipDto object                
            organizationalRelationshipDto = new Dtos.Base.OrganizationalRelationship();
            organizationalPersonPositionDto = new Dtos.Base.OrganizationalPersonPosition();

            organizationalRelationshipServiceMock.Setup(s => s.AddAsync(organizationalRelationshipDto)).ReturnsAsync(organizationalRelationshipDto);
            organizationalRelationshipServiceMock.Setup(s => s.UpdateAsync(organizationalRelationshipDto)).ReturnsAsync(organizationalRelationshipDto);
            organizationalPersonPositionServiceMock.Setup(s => s.GetOrganizationalPersonPositionByIdAsync(It.IsAny<string>())).ReturnsAsync(organizationalPersonPositionDto);

            organizationalRelationshipsController = new OrganizationalRelationshipsController(organizationalPersonPositionService, organizationalRelationshipService, logger);
        }

        [TestMethod]
        public async Task CreateOrganizationalRelationship_Succeeds()
        {
            organizationalRelationshipDto.Id = "1";
            organizationalRelationshipDto.OrganizationalPersonPositionId = "123";
            organizationalRelationshipDto.RelatedOrganizationalPersonPositionId = "1234";
            organizationalRelationshipDto.RelatedPersonId = "2";
            organizationalRelationshipDto.RelatedPersonName = "Walter White";
            organizationalRelationshipDto.RelatedPositionTitle = "Chemistry Teacher";
            organizationalRelationshipDto.Category = "MGR";

            var relationship = await organizationalRelationshipsController.CreateOrganizationalRelationshipAsync(organizationalRelationshipDto);
            Assert.IsTrue(relationship is Dtos.Base.OrganizationalRelationship);
        }

        [TestMethod]
        public async Task CreateOrganizationalRelationship_Fails()
        {
            HttpStatusCode statusCode = HttpStatusCode.Unused;
            organizationalRelationshipServiceMock.Setup(s => s.AddAsync(organizationalRelationshipDto)).Throws(new InvalidOperationException());
            try
            {
                await organizationalRelationshipsController.CreateOrganizationalRelationshipAsync(organizationalRelationshipDto);
            }
            catch (HttpResponseException e)
            {
                statusCode = e.Response.StatusCode;
            }
            Assert.AreEqual(HttpStatusCode.BadRequest, statusCode);
        }

        [TestMethod]
        public async Task UpdateOrganizationalRelationship_Succeeds()
        {
            organizationalRelationshipDto.Id = "1";
            organizationalRelationshipDto.OrganizationalPersonPositionId = "123";
            organizationalRelationshipDto.RelatedOrganizationalPersonPositionId = "1234";
            organizationalRelationshipDto.RelatedPersonId = "2";
            organizationalRelationshipDto.RelatedPersonName = "Walter White";
            organizationalRelationshipDto.RelatedPositionTitle = "Chemistry Teacher";
            organizationalRelationshipDto.Category = "MGR";

            var relationship = await organizationalRelationshipsController.UpdateOrganizationalRelationshipAsync(organizationalRelationshipDto);
            Assert.IsTrue(relationship is Dtos.Base.OrganizationalRelationship);
        }

        [TestMethod]
        public async Task UpdateOrganizationalRelationship_Fails()
        {
            HttpStatusCode statusCode = HttpStatusCode.Unused;
            organizationalRelationshipServiceMock.Setup(s => s.UpdateAsync(organizationalRelationshipDto)).Throws(new InvalidOperationException());
            try
            {
                await organizationalRelationshipsController.UpdateOrganizationalRelationshipAsync(organizationalRelationshipDto);
            }
            catch (HttpResponseException e)
            {
                statusCode = e.Response.StatusCode;
            }
            Assert.AreEqual(HttpStatusCode.BadRequest, statusCode);
        }

        [TestMethod]
        public async Task DeleteOrganizationalRelationship_Succeeds()
        {
            organizationalRelationshipServiceMock.Setup(s => s.DeleteAsync(It.IsAny<string>())).Returns(Task.FromResult(false)).Verifiable();
            var deletedId = "1";
            await organizationalRelationshipsController.DeleteOrganizationalRelationshipAsync(deletedId);
            organizationalRelationshipServiceMock.Verify(m => m.DeleteAsync(deletedId), Times.Once());
        }

        [TestMethod]
        public async Task DeleteOrganizationalRelationship_Fails()
        {
            HttpStatusCode statusCode = HttpStatusCode.Unused;
            organizationalRelationshipServiceMock.Setup(s => s.DeleteAsync(It.IsAny<string>())).Throws(new InvalidOperationException());
            var deletedId = "1";
            try
            {
                await organizationalRelationshipsController.DeleteOrganizationalRelationshipAsync(deletedId);
            }
            catch (HttpResponseException e)
            {
                statusCode = e.Response.StatusCode;
            }
            Assert.AreEqual(HttpStatusCode.BadRequest, statusCode);
        }

    }
}
