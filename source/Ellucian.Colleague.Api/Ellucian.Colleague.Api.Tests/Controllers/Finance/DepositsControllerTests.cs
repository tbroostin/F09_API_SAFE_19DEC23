//Copyright 2015-2018 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Api.Controllers.Finance;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.Finance;
using Ellucian.Colleague.Dtos.Finance;
using Ellucian.Web.Adapters;
using Ellucian.Web.Security;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;
using System;
using System.Collections.Generic;
using System.IO;
using System.Web;
using System.Web.Http;

namespace Ellucian.Colleague.Api.Tests.Controllers.Finance
{
    [TestClass]
    public class DepositsControllerTests
    {
        [TestClass]
        public class GetDepositsDueTests
        {
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

            private Mock<IAdapterRegistry> adapterRegistryMock;
            private Mock<ILogger> loggerMock;
            private Mock<IAccountsReceivableService> accountsReceivableServiceMock;

            private List<DepositDue> depositsDue;
            private HttpResponse response;

            private DepositsController DepositsController;

            [TestInitialize]
            public void Initialize()
            {
                LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
                EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));

                adapterRegistryMock = new Mock<IAdapterRegistry>();
                loggerMock = new Mock<ILogger>();
                accountsReceivableServiceMock = new Mock<IAccountsReceivableService>();

                depositsDue = new List<DepositDue>()
                {
                    new DepositDue()
                    {
                        Amount = 500m,
                        AmountDue = 400m,
                        AmountPaid = 100m,
                        Balance = 400m,
                        DepositType = "MEALS",
                        DepositTypeDescription = "Meal Plan Deposit",
                        Distribution = "BANK",
                        DueDate = DateTime.Today.AddDays(7),
                        Id = "123",
                        Overdue = false,
                        PersonId = "0001234",
                        SortOrder = "1",
                        TermDescription = "2014 Fall Term",
                        TermId = "2014/FA"
                    },
                    new DepositDue()
                    {
                        Amount = 300m,
                        AmountDue = 300m,
                        AmountPaid = 0m,
                        Balance = 0m,
                        DepositType = "RESHL",
                        DepositTypeDescription = "Residence Hall Deposit",
                        Distribution = "BANK",
                        DueDate = DateTime.Today.AddDays(-7),
                        Id = "124",
                        Overdue = true,
                        PersonId = "0001234",
                        SortOrder = "2",
                        TermDescription = "2014 Fall Term",
                        TermId = "2014/FA"
                    }
                };

                response = new HttpResponse(new StringWriter());
                HttpContext.Current = new HttpContext(new HttpRequest("", "http://doesntMatter.com", ""), response);

                accountsReceivableServiceMock.Setup(aa => aa.GetDepositsDue("0001234")).Returns(depositsDue);
                accountsReceivableServiceMock.Setup(aa => aa.GetDepositsDue("0001235")).Throws(new PermissionsException());

                DepositsController = new DepositsController(accountsReceivableServiceMock.Object, loggerMock.Object);
            }

            [TestCleanup]
            public void Cleanup()
            {
                adapterRegistryMock = null;
                loggerMock = null;
                accountsReceivableServiceMock = null;
                depositsDue = null;
                DepositsController = null;
            }

            [TestMethod]
            public void DepositsController_GetDepositsDue_Valid()
            {
                var dd = DepositsController.GetDepositsDue("0001234");
                Assert.IsNotNull(dd);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public void DepositsController_GetDepositsDue_PermissionsException()
            {
                var dd = DepositsController.GetDepositsDue("0001235");
            }
        }

        [TestClass]
        public class GetDepositTypesTests
        {
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

            private Mock<IAdapterRegistry> adapterRegistryMock;
            private Mock<ILogger> loggerMock;
            private Mock<IAccountsReceivableService> accountsReceivableServiceMock;

            private List<DepositType> depositTypes;
            private HttpResponse response;

            private DepositsController DepositsController;

            [TestInitialize]
            public void Initialize()
            {
                LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
                EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));

                adapterRegistryMock = new Mock<IAdapterRegistry>();
                loggerMock = new Mock<ILogger>();
                accountsReceivableServiceMock = new Mock<IAccountsReceivableService>();

                depositTypes = new List<DepositType>()
                {
                    new DepositType()
                    {
                        Code = "MEALS",
                        Description = "Meal Plan Deposit"
                    },
                    new DepositType()
                    {
                        Code = "RESHL",
                        Description = "Residence Hall Deposit"
                    },
                };

                response = new HttpResponse(new StringWriter());
                HttpContext.Current = new HttpContext(new HttpRequest("", "http://doesntMatter.com", ""), response);

                accountsReceivableServiceMock.Setup(aa => aa.GetDepositTypes()).Returns(depositTypes);

                DepositsController = new DepositsController(accountsReceivableServiceMock.Object, loggerMock.Object);
            }

            [TestCleanup]
            public void Cleanup()
            {
                adapterRegistryMock = null;
                loggerMock = null;
                accountsReceivableServiceMock = null;
                depositTypes = null;
                DepositsController = null;
            }

            [TestMethod]
            public void DepositsController_GetDepositTypes_Valid()
            {
                var dt = DepositsController.GetDepositTypes();
                Assert.IsNotNull(dt);
            }
        }
    }
}
