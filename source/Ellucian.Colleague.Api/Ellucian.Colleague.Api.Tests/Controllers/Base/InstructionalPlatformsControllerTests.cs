// Copyright 2012-2014 Ellucian Company L.P. and its affiliates.
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using Ellucian.Colleague.Api.Controllers;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.Base.Tests;
using Ellucian.Colleague.Domain.Base;
using Ellucian.Colleague.Dtos.Base;
using Ellucian.Web.Adapters;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;
using System.Threading.Tasks;
using Ellucian.Colleague.Dtos;
using System.Net.Http;
using System.Web.Http.Hosting;
using System.Web.Http;
using System.Reflection;
using System;
using Ellucian.Colleague.Api.Controllers.Base;
using Ellucian.Web.Security;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Web.Http.Exceptions;

namespace Ellucian.Colleague.Api.Tests.Controllers.Base
{
    [TestClass]
    public class InstructionalPlatformsControllerTests
    {
        [TestClass]
        public class InstructionalPlatformControllerGet
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

            private Mock<IAdapterRegistry> adapterRegistryMock;
            private IAdapterRegistry adapterRegistry;
            private Mock<IReferenceDataRepository> referenceDataRepositoryMock;
            private IReferenceDataRepository referenceDataRepository;
            private Mock<IInstructionalPlatformService> instructionalPlatformServiceMock;
            private IInstructionalPlatformService instructionalPlatformService;
            private ILogger logger = new Mock<ILogger>().Object;

            private InstructionalPlatformsController instructionalPlatformsController;
            List<Colleague.Domain.Base.Entities.InstructionalPlatform> allInstructionalPlatforms = new List<Colleague.Domain.Base.Entities.InstructionalPlatform>();
            private List<Dtos.InstructionalPlatform> allInstructionalPlatformDtos = new List<Dtos.InstructionalPlatform>();

            [TestInitialize]
            public void Initialize()
            {
                EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));
                referenceDataRepositoryMock = new Mock<IReferenceDataRepository>();
                referenceDataRepository = referenceDataRepositoryMock.Object;

                adapterRegistryMock = new Mock<IAdapterRegistry>();
                adapterRegistry = adapterRegistryMock.Object;
                instructionalPlatformServiceMock = new Mock<IInstructionalPlatformService>();
                instructionalPlatformService = instructionalPlatformServiceMock.Object;

                allInstructionalPlatforms.Add(new Colleague.Domain.Base.Entities.InstructionalPlatform("840e72f0-57b9-42a2-ae88-df3c2262fbbc", "CE", "Continuing Education"));
                allInstructionalPlatforms.Add(new Colleague.Domain.Base.Entities.InstructionalPlatform("e986b8a5-25f3-4aa0-bd0e-90982865e749", "D", "Institutional"));
                allInstructionalPlatforms.Add(new Colleague.Domain.Base.Entities.InstructionalPlatform("b5cc288b-8692-474e-91be-bdc55778e2f5", "TR", "Transfer"));

                Mapper.CreateMap<Ellucian.Colleague.Domain.Base.Entities.InstructionalPlatform, Dtos.InstructionalPlatform>();
                foreach (var instructionalPlatform in allInstructionalPlatforms)
                {
                    Dtos.InstructionalPlatform target = Mapper.Map<Ellucian.Colleague.Domain.Base.Entities.InstructionalPlatform, Dtos.InstructionalPlatform>(instructionalPlatform);
                    allInstructionalPlatformDtos.Add(target);
                }

                instructionalPlatformsController = new InstructionalPlatformsController(instructionalPlatformService, logger);
                instructionalPlatformsController.Request = new HttpRequestMessage();
                instructionalPlatformsController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
            }

            [TestCleanup]
            public void Cleanup()
            {
                instructionalPlatformsController = null;
                referenceDataRepository = null;
            }

            [TestMethod]
            public void InstructionalPlatformsController_GetInstructionalPlatforms_ValidateFields()
            {
                instructionalPlatformServiceMock.Setup(x => x.GetInstructionalPlatforms(It.IsAny<bool>())).Returns(allInstructionalPlatformDtos);

                var instructionalPlatforms = (instructionalPlatformsController.GetInstructionalPlatforms()).ToList();
                Assert.AreEqual(allInstructionalPlatformDtos.Count, instructionalPlatforms.Count);
                for (int i = 0; i < instructionalPlatforms.Count; i++)
                {
                    var expected = allInstructionalPlatformDtos[i];
                    var actual = instructionalPlatforms[i];
                    Assert.AreEqual(expected.Id, actual.Id, "Id, Index=" + i.ToString());
                    Assert.AreEqual(expected.Title, actual.Title, "Title, Index=" + i.ToString());
                }
            }

            [TestMethod]
            public void InstructionalPlatformsController_GetInstructionalPlatformByGuid_ValidateFields()
            {
                var expected = allInstructionalPlatformDtos.FirstOrDefault();
                instructionalPlatformServiceMock.Setup(x => x.GetInstructionalPlatformByGuid(expected.Id, false)).Returns(expected);

                var instructionalPlatforms = (instructionalPlatformsController.GetInstructionalPlatforms()).ToList();
                var actual = instructionalPlatformsController.GetInstructionalPlatformsById(expected.Id);

                Assert.AreEqual(expected.Id, actual.Id, "Id");
                Assert.AreEqual(expected.Title, actual.Title, "Title");
            }

            [TestMethod]
            public void InstructionalPlatformsController_GetHedm_CacheControlNotNull()
            {
                instructionalPlatformsController.Request.Headers.CacheControl = new System.Net.Http.Headers.CacheControlHeaderValue();
                instructionalPlatformServiceMock.Setup(x => x.GetInstructionalPlatforms(It.IsAny<bool>())).Returns(allInstructionalPlatformDtos);

                List<Dtos.InstructionalPlatform> InstructionalPlatforms = instructionalPlatformsController.GetInstructionalPlatforms() as List<Dtos.InstructionalPlatform>;
                Dtos.InstructionalPlatform ip = InstructionalPlatforms.Where(a => a.Code == "CE").FirstOrDefault();
                Ellucian.Colleague.Domain.Base.Entities.InstructionalPlatform ipt = allInstructionalPlatforms.Where(a => a.Code == "CE").FirstOrDefault();
                Assert.AreEqual(ipt.Code, ip.Code);
                Assert.AreEqual(ipt.Description, ip.Description);
            }

            [TestMethod]
            public void InstructionalPlatformsController_GetHedm_NoCache()
            {
                instructionalPlatformsController.Request.Headers.CacheControl = new System.Net.Http.Headers.CacheControlHeaderValue();
                instructionalPlatformsController.Request.Headers.CacheControl.NoCache = true;
                instructionalPlatformServiceMock.Setup(x => x.GetInstructionalPlatforms(It.IsAny<bool>())).Returns(allInstructionalPlatformDtos);

                List<Dtos.InstructionalPlatform> InstructionalPlatforms = instructionalPlatformsController.GetInstructionalPlatforms() as List<Dtos.InstructionalPlatform>;
                Dtos.InstructionalPlatform ip = InstructionalPlatforms.Where(a => a.Code == "CE").FirstOrDefault();
                Ellucian.Colleague.Domain.Base.Entities.InstructionalPlatform ipt = allInstructionalPlatforms.Where(a => a.Code == "CE").FirstOrDefault();
                Assert.AreEqual(ipt.Code, ip.Code);
                Assert.AreEqual(ipt.Description, ip.Description);
            }

            [TestMethod]
            public void InstructionalPlatformsController_GetHedm_Cache()
            {
                instructionalPlatformsController.Request.Headers.CacheControl = new System.Net.Http.Headers.CacheControlHeaderValue();
                instructionalPlatformsController.Request.Headers.CacheControl.NoCache = false;
                instructionalPlatformServiceMock.Setup(x => x.GetInstructionalPlatforms(It.IsAny<bool>())).Returns(allInstructionalPlatformDtos);

                List<Dtos.InstructionalPlatform> InstructionalPlatforms = instructionalPlatformsController.GetInstructionalPlatforms() as List<Dtos.InstructionalPlatform>;
                Dtos.InstructionalPlatform ip = InstructionalPlatforms.Where(a => a.Code == "CE").FirstOrDefault();
                Ellucian.Colleague.Domain.Base.Entities.InstructionalPlatform ipt = allInstructionalPlatforms.Where(a => a.Code == "CE").FirstOrDefault();
                Assert.AreEqual(ipt.Code, ip.Code);
                Assert.AreEqual(ipt.Description, ip.Description);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public void InstructionalPlatformsController_GetThrowsIntAppiExc()
            {
                instructionalPlatformServiceMock.Setup(gc => gc.GetInstructionalPlatforms(It.IsAny<bool>())).Throws<Exception>();

                instructionalPlatformsController.GetInstructionalPlatforms();
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public void InstructionalPlatformsController_GetByIdThrowsIntAppiExc()
            {
                instructionalPlatformServiceMock.Setup(gc => gc.GetInstructionalPlatformByGuid(It.IsAny<string>(), It.IsAny<bool>())).Throws<Exception>();

                instructionalPlatformsController.GetInstructionalPlatformsById("sdjfh");
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public void InstructionalPlatformsController_GetThrowsIntAppiExc2()
            {
                instructionalPlatformServiceMock.Setup(gc => gc.GetInstructionalPlatforms(It.IsAny<bool>())).Throws<PermissionsException>();

                instructionalPlatformsController.GetInstructionalPlatforms();
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public void InstructionalPlatformsController_GetThrowsIntAppiExc3()
            {
                instructionalPlatformServiceMock.Setup(gc => gc.GetInstructionalPlatforms(It.IsAny<bool>())).Throws<ArgumentException>();

                instructionalPlatformsController.GetInstructionalPlatforms();
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public void InstructionalPlatformsController_GetThrowsIntAppiExc4()
            {
                instructionalPlatformServiceMock.Setup(gc => gc.GetInstructionalPlatforms(It.IsAny<bool>())).Throws<RepositoryException>();

                instructionalPlatformsController.GetInstructionalPlatforms();
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public void InstructionalPlatformsController_GetThrowsIntAppiExc5()
            {
                instructionalPlatformServiceMock.Setup(gc => gc.GetInstructionalPlatforms(It.IsAny<bool>())).Throws<IntegrationApiException>();

                instructionalPlatformsController.GetInstructionalPlatforms();
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public void InstructionalPlatformsController_GetByIdThrowsIntAppiExc2()
            {
                instructionalPlatformServiceMock.Setup(gc => gc.GetInstructionalPlatformByGuid(It.IsAny<string>(), It.IsAny<bool>())).Throws<PermissionsException>();

                instructionalPlatformsController.GetInstructionalPlatformsById("sdjfh");
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public void InstructionalPlatformsController_GetByIdThrowsIntAppiExc3()
            {
                instructionalPlatformServiceMock.Setup(gc => gc.GetInstructionalPlatformByGuid(It.IsAny<string>(), It.IsAny<bool>())).Throws<ArgumentException>();

                instructionalPlatformsController.GetInstructionalPlatformsById("sdjfh");
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public void InstructionalPlatformsController_GetByIdThrowsIntAppiExc4()
            {
                instructionalPlatformServiceMock.Setup(gc => gc.GetInstructionalPlatformByGuid(It.IsAny<string>(), It.IsAny<bool>())).Throws<RepositoryException>();

                instructionalPlatformsController.GetInstructionalPlatformsById("sdjfh");
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public void InstructionalPlatformsController_GetByIdThrowsIntAppiExc5()
            {
                instructionalPlatformServiceMock.Setup(gc => gc.GetInstructionalPlatformByGuid(It.IsAny<string>(), It.IsAny<bool>())).Throws<IntegrationApiException>();

                instructionalPlatformsController.GetInstructionalPlatformsById("sdjfh");
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public void InstructionalPlatformsController_PostInstructionalPlatformAsync()
            {
                var response = instructionalPlatformsController.PostInstructionalPlatforms(allInstructionalPlatformDtos.FirstOrDefault());
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public void InstructionalPlatformsController_PutInstructionalPlatformAsync()
            {
                var instructionalPlatform = allInstructionalPlatformDtos.FirstOrDefault();
                var response = instructionalPlatformsController.PutInstructionalPlatforms(instructionalPlatform.Id, instructionalPlatform);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public void InstructionalPlatformsController_DeleteInstructionalPlatformAsync()
            {
                instructionalPlatformsController.DeleteInstructionalPlatforms(allInstructionalPlatformDtos.FirstOrDefault().Id);
            }
        }
    }
}
