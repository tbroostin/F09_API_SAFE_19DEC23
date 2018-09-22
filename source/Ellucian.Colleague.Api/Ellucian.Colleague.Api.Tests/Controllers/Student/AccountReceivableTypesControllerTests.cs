// Copyright 2016-2018 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.Student.Services;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Colleague.Domain.Student.Tests;
using Ellucian.Web.Adapters;
using Ellucian.Web.Http.Exceptions;
using Ellucian.Web.Security;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Hosting;
using Ellucian.Colleague.Api.Controllers.Student;

namespace Ellucian.Colleague.Api.Tests.Controllers.Student
{
    [TestClass]
    public class AccountReceivableTypesControllerTests
    {
        [TestClass]
        public class GET
        {
            /// <summary>
            ///     Gets or sets the test context which provides
            ///     information about and functionality for the current test run.
            /// </summary>
            public TestContext TestContext { get; set; }

            Mock<ICurriculumService> curriculumServiceMock;
            Mock<IAdapterRegistry> adapterRegistryMock;
            Mock<ILogger> loggerMock;

            AccountReceivableTypesController accountReceivableTypesController;
            List<Ellucian.Colleague.Domain.Student.Entities.AccountReceivableType> accountReceivableTypesEntities;
            List<Dtos.AccountReceivableType> accountReceivableTypesDto = new List<Dtos.AccountReceivableType>();

            [TestInitialize]
            public void Initialize()
            {
                LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
                EllucianLicenseProvider.RefreshLicense(Path.Combine(TestContext.DeploymentDirectory, "App_Data"));

                curriculumServiceMock = new Mock<ICurriculumService>();
                adapterRegistryMock = new Mock<IAdapterRegistry>();
                loggerMock = new Mock<ILogger>();

                accountReceivableTypesEntities = new TestAccountReceivableTypeRepository().Get();
                foreach (var accountReceivableTypeEntity in accountReceivableTypesEntities)
                {
                    Dtos.AccountReceivableType accountReceivableTypeDto = new Dtos.AccountReceivableType();
                    accountReceivableTypeDto.Id = accountReceivableTypeEntity.Guid;
                    accountReceivableTypeDto.Code = accountReceivableTypeEntity.Code;
                    accountReceivableTypeDto.Title = accountReceivableTypeEntity.Description;
                    accountReceivableTypeDto.Description = string.Empty;
                    accountReceivableTypesDto.Add(accountReceivableTypeDto);
                }

                accountReceivableTypesController = new AccountReceivableTypesController(adapterRegistryMock.Object, curriculumServiceMock.Object, loggerMock.Object) { Request = new HttpRequestMessage() };
                accountReceivableTypesController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey,
                new HttpConfiguration());               
            }

            [TestCleanup]
            public void Cleanup()
            {
                accountReceivableTypesController = null;
                accountReceivableTypesEntities = null;
                accountReceivableTypesDto = null;
            }

            [TestMethod]
            public async Task AccountReceivableTypesController_GetAll_NoCache_True()
            {
                accountReceivableTypesController.Request.Headers.CacheControl = new CacheControlHeaderValue
                {
                    NoCache = true,
                    Public = true
                };

                curriculumServiceMock.Setup(ac => ac.GetAccountReceivableTypesAsync(It.IsAny<bool>())).ReturnsAsync(accountReceivableTypesDto);

                var results = await accountReceivableTypesController.GetAccountReceivableTypesAsync();
                Assert.AreEqual(accountReceivableTypesDto.Count, results.Count());

                foreach (var accountReceivableTypeDto in accountReceivableTypesDto)
                {
                    var result = results.FirstOrDefault(i => i.Id == accountReceivableTypeDto.Id);
                    Assert.AreEqual(result.Id, accountReceivableTypeDto.Id);
                    Assert.AreEqual(result.Code, accountReceivableTypeDto.Code);
                    Assert.AreEqual(result.Title, accountReceivableTypeDto.Title);
                    Assert.AreEqual(result.Description, accountReceivableTypeDto.Description);
                }
            }

            [TestMethod]
            public async Task AccountReceivableTypesController_GetAll_NoCache_False()
            {
                accountReceivableTypesController.Request.Headers.CacheControl = new CacheControlHeaderValue
                {
                    NoCache = false,
                    Public = true
                };

                curriculumServiceMock.Setup(ac => ac.GetAccountReceivableTypesAsync(It.IsAny<bool>())).ReturnsAsync(accountReceivableTypesDto);

                var results = await accountReceivableTypesController.GetAccountReceivableTypesAsync();
                Assert.AreEqual(accountReceivableTypesDto.Count, results.Count());                
                
                foreach (var accountReceivableTypeDto in accountReceivableTypesDto)
                {
                    var result = results.FirstOrDefault(i => i.Id == accountReceivableTypeDto.Id);
                    Assert.AreEqual(result.Id, accountReceivableTypeDto.Id);
                    Assert.AreEqual(result.Code, accountReceivableTypeDto.Code);
                    Assert.AreEqual(result.Title, accountReceivableTypeDto.Title);
                    Assert.AreEqual(result.Description, accountReceivableTypeDto.Description);
                }

            }

            [TestMethod]
            public async Task AccountReceivableTypesController_GetById()
            {
                string id = "a142d78a-b472-45de-8a4b-953258976a0b";
                var accountReceivableTypeDto = accountReceivableTypesDto.FirstOrDefault(i => i.Id == id);
                accountReceivableTypesController.Request.Headers.CacheControl = new CacheControlHeaderValue
                {
                    NoCache = true,
                    Public = true
                };

                curriculumServiceMock.Setup(ac => ac.GetAccountReceivableTypeByIdAsync(id)).ReturnsAsync(accountReceivableTypeDto);

                var result = await accountReceivableTypesController.GetAccountReceivableTypeByIdAsync(id);
                Assert.AreEqual(result.Id, accountReceivableTypeDto.Id);
                Assert.AreEqual(result.Code, accountReceivableTypeDto.Code);
                Assert.AreEqual(result.Title, accountReceivableTypeDto.Title);
                Assert.AreEqual(result.Description, accountReceivableTypeDto.Description);
            }

            //[TestMethod]
            //[ExpectedException(typeof(HttpResponseException))]
            //public async Task AccountReceivableTypesController_GetAll_Exception()
            //{
            //    curriculumServiceMock.Setup(ac => ac.GetAccountReceivableTypesAsync(It.IsAny<bool>())).ThrowsAsync(new Exception());
            //    var result = await accountReceivableTypesController.GetAccountReceivableTypesAsync();
            //}

            //[TestMethod]
            //[ExpectedException(typeof(HttpResponseException))]
            //public async Task AccountReceivableTypesController_GetById_KeyNotFoundException()
            //{
            //    curriculumServiceMock.Setup(ac => ac.GetAccountReceivableTypeByIdAsync(It.IsAny<string>())).ThrowsAsync(new KeyNotFoundException());
            //    var result = await accountReceivableTypesController.GetAccountReceivableTypeByIdAsync(It.IsAny<string>());
            //}

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task AccountReceivableTypesController_GetById_Exception()
            {
                curriculumServiceMock.Setup(ac => ac.GetAccountReceivableTypeByIdAsync(It.IsAny<string>())).ThrowsAsync(new Exception());
                var result = await accountReceivableTypesController.GetAccountReceivableTypeByIdAsync(It.IsAny<string>());
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task AccountReceivableTypeController_GetAccountReceivableTypesAsync_Exception()
            {
                curriculumServiceMock.Setup(s => s.GetAccountReceivableTypesAsync(It.IsAny<bool>())).Throws<Exception>();
                await accountReceivableTypesController.GetAccountReceivableTypesAsync();
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task AccountReceivableTypeController_GetAccountReceivableTypesAsync_IntegrationApiException()
            {
                curriculumServiceMock.Setup(s => s.GetAccountReceivableTypesAsync(It.IsAny<bool>())).Throws<IntegrationApiException>();
                await accountReceivableTypesController.GetAccountReceivableTypesAsync();
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task AccountReceivableTypeController_GetAccountReceivableTypeByIdAsync_PermissionsException()
            {
                var expected = accountReceivableTypesDto.FirstOrDefault();
                curriculumServiceMock.Setup(x => x.GetAccountReceivableTypeByIdAsync(expected.Id)).Throws<PermissionsException>();
                Debug.Assert(expected != null, "expected != null");
                await accountReceivableTypesController.GetAccountReceivableTypeByIdAsync(expected.Id);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task AccountReceivableTypeController_GetAccountReceivableTypesAsync_PermissionsException()
            {
                curriculumServiceMock.Setup(s => s.GetAccountReceivableTypesAsync(It.IsAny<bool>())).Throws<PermissionsException>();
                await accountReceivableTypesController.GetAccountReceivableTypesAsync();
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task AccountReceivableTypeController_GetAccountReceivableTypeByIdAsync_KeyNotFoundException()
            {
                var expected = accountReceivableTypesDto.FirstOrDefault();
                curriculumServiceMock.Setup(x => x.GetAccountReceivableTypeByIdAsync(expected.Id)).Throws<KeyNotFoundException>();
                Debug.Assert(expected != null, "expected != null");
                await accountReceivableTypesController.GetAccountReceivableTypeByIdAsync(expected.Id);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task AccountReceivableTypeController_GetAccountReceivableTypesAsync_KeyNotFoundException()
            {
                curriculumServiceMock.Setup(s => s.GetAccountReceivableTypesAsync(It.IsAny<bool>())).Throws<KeyNotFoundException>();
                await accountReceivableTypesController.GetAccountReceivableTypesAsync();
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task AccountReceivableTypeController_GetAccountReceivableTypesAsync_ArgumentNullException()
            {
                curriculumServiceMock.Setup(s => s.GetAccountReceivableTypesAsync(It.IsAny<bool>())).Throws<ArgumentNullException>();
                await accountReceivableTypesController.GetAccountReceivableTypesAsync();
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task AccountReceivableTypeController_GetAccountReceivableTypesAsync_RepositoryException()
            {
                curriculumServiceMock.Setup(s => s.GetAccountReceivableTypesAsync(It.IsAny<bool>())).Throws<RepositoryException>();
                await accountReceivableTypesController.GetAccountReceivableTypesAsync();
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task AccountReceivableTypesController_PUT_Exception()
            {
                var result = await accountReceivableTypesController.PutAccountReceivableTypeAsync(It.IsAny<string>(), new Dtos.AccountReceivableType());
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task AccountReceivableTypesController_POST_Exception()
            {
                var result = await accountReceivableTypesController.PostAccountReceivableTypeAsync(new Dtos.AccountReceivableType());
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task AccountReceivableTypesController_DELETE_Exception()
            {
                await accountReceivableTypesController.DeleteAccountReceivableTypeAsync(It.IsAny<string>());
            }
        }
    }
}
