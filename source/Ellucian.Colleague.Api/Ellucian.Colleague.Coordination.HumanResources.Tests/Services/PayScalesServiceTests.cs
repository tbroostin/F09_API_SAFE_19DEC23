using Ellucian.Colleague.Coordination.HumanResources.Services;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.HumanResources;
using Ellucian.Colleague.Domain.HumanResources.Repositories;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Web.Adapters;
using Ellucian.Web.Security;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;

namespace Ellucian.Colleague.Coordination.HumanResources.Tests.Services
{
    [TestClass]
    public class PayScalesServiceTests_V11
    {
        [TestClass]
        public class PayScalesServiceTests_GET_V11 : CurrentUserSetup
        {
            #region DECLARATION

            protected Domain.Entities.Role viewPayScales = new Domain.Entities.Role(1, "VIEW.PAY.SCALES");

            private Mock<IPayScalesRepository> payScalesRepositoryMock;
            private Mock<IHumanResourcesReferenceDataRepository> hrReferenceDataRepositoryMock;
            private Mock<IAdapterRegistry> adapterRegistryMock;
            private Mock<IRoleRepository> roleRepositoryMock;
            private Mock<ILogger> loggerMock;
            private Mock<IConfigurationRepository> configurationRepositoryMock;

            private ICurrentUserFactory currentUserFactory;

            private PayScalesService payScalesService;

            private List<Domain.HumanResources.Entities.PayScale> payScales;

            private string guid = "1a59eed8-5fe7-4120-b1cf-f23266b9e874";

            #endregion

            #region TEST SETUP

            [TestInitialize]
            public void Initialize()
            {
                payScalesRepositoryMock = new Mock<IPayScalesRepository>();
                adapterRegistryMock = new Mock<IAdapterRegistry>();
                hrReferenceDataRepositoryMock = new Mock<IHumanResourcesReferenceDataRepository>();
                configurationRepositoryMock = new Mock<IConfigurationRepository>();
                roleRepositoryMock = new Mock<IRoleRepository>();
                loggerMock = new Mock<ILogger>();

                currentUserFactory = new CurrentUserSetup.PayScalesUserFactory();

                InitializeTestData();

                InitializeMock();

                payScalesService = new PayScalesService(payScalesRepositoryMock.Object, hrReferenceDataRepositoryMock.Object,
                    adapterRegistryMock.Object, currentUserFactory, roleRepositoryMock.Object, configurationRepositoryMock.Object, loggerMock.Object);
            }

            [TestCleanup]
            public void Cleanup()
            {
                payScalesRepositoryMock = null;
                adapterRegistryMock = null;
                hrReferenceDataRepositoryMock = null;
                roleRepositoryMock = null;
                loggerMock = null;
                currentUserFactory = null;
                configurationRepositoryMock = null;
            }

            private void InitializeTestData()
            {
                payScales = new List<Domain.HumanResources.Entities.PayScale>()
                {
                    new Domain.HumanResources.Entities.PayScale(guid, "1", "desc", DateTime.Today, DateTime.Today.AddDays(100))
                    {
                        WageTableGuid = guid,
                        Scales = new List<Domain.HumanResources.Entities.PayScalesScales>()
                        {
                            new Domain.HumanResources.Entities.PayScalesScales()
                            {
                                Amount = 100,
                                Grade = "A",
                                Step = "1"
                            }
                        }
                    }
                };
            }

            private void InitializeMock()
            {
                viewPayScales.AddPermission(new Domain.Entities.Permission(HumanResourcesPermissionCodes.ViewPayScales));
                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { viewPayScales });

                payScalesRepositoryMock.Setup(p => p.GetPayScalesAsync(false)).ReturnsAsync(payScales);
                payScalesRepositoryMock.Setup(p => p.GetPayScalesByIdAsync(It.IsAny<string>())).ReturnsAsync(payScales.FirstOrDefault());
                payScalesRepositoryMock.Setup(p => p.GetHostCountryAsync()).ReturnsAsync("CANADA");
            }

            #endregion

            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public async Task PayScalesService_GetPayScalesAsync_PermissionException()
            {
                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { });
                await payScalesService.GetPayScalesAsync();
            }

            [TestMethod]
            public async Task PayScalesService_GetPayScalesAsync_Repository_Returns_Null()
            {
                payScalesRepositoryMock.Setup(p => p.GetPayScalesAsync(false)).ReturnsAsync(null);

                var result = await payScalesService.GetPayScalesAsync();

                Assert.IsNotNull(result);
                Assert.AreEqual(0, result.Count());
            }

            [TestMethod]
            public async Task PayScalesService_GetPayScalesAsync_Repository_Returns_Empty()
            {
                payScalesRepositoryMock.Setup(p => p.GetPayScalesAsync(false)).ReturnsAsync(new List<Domain.HumanResources.Entities.PayScale>() { });

                var result = await payScalesService.GetPayScalesAsync();

                Assert.IsNotNull(result);
                Assert.AreEqual(0, result.Count());
            }

            [TestMethod]
            public async Task PayScalesService_GetPayScalesAsync()
            {
                var result = await payScalesService.GetPayScalesAsync();

                Assert.IsNotNull(result);
                Assert.AreEqual(1, result.Count());
                Assert.AreEqual(guid, result.FirstOrDefault().Id);
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task PayScalesService_GetPayScalesByGuidAsync_KeyNotFoundException()
            {
                payScalesRepositoryMock.Setup(p => p.GetPayScalesByIdAsync(It.IsAny<string>())).ThrowsAsync(new KeyNotFoundException());
                await payScalesService.GetPayScalesByGuidAsync(guid);
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task PayScalesService_GetPayScalesByGuidAsync_InvalidOperationException()
            {
                payScalesRepositoryMock.Setup(p => p.GetPayScalesByIdAsync(It.IsAny<string>())).ThrowsAsync(new InvalidOperationException());
                await payScalesService.GetPayScalesByGuidAsync(guid);
            }

            [TestMethod]
            public async Task PayScalesService_GetPayScalesByGuidAsync()
            {
                var result = await payScalesService.GetPayScalesByGuidAsync(guid);

                Assert.IsNotNull(result);
                Assert.AreEqual(guid, result.Id);
            }
        }
    }
}
