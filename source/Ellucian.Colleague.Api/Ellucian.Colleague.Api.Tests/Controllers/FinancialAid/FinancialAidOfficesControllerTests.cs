/*Copyright 2014-2018 Ellucian Company L.P. and its affiliates.*/

using Ellucian.Colleague.Api.Controllers.FinancialAid;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.FinancialAid.Services;
using Ellucian.Colleague.Dtos.FinancialAid;
using Ellucian.Web.Adapters;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;

namespace Ellucian.Colleague.Api.Tests.Controllers.FinancialAid
{
    [TestClass]
    public class FinancialAidOfficesControllerTests
    {
        [TestClass]
        public class GetFinancialAidOfficesTests
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
            private Mock<IFinancialAidOfficeService> financialAidOfficeServiceMock;

            private IEnumerable<FinancialAidOffice> expectedOffices;
            private IEnumerable<FinancialAidOffice> actualOffices;

            private FinancialAidOfficesController financialAidOfficesController;

            [TestInitialize]
            public void Initialize()
            {
                LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
                EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));

                adapterRegistryMock = new Mock<IAdapterRegistry>();
                loggerMock = new Mock<ILogger>();
                financialAidOfficeServiceMock = new Mock<IFinancialAidOfficeService>();

                expectedOffices = new List<FinancialAidOffice>()
                {
                    new FinancialAidOffice()
                    {
                        Id = "MAIN",
                        Name = "Main Office",
                        AddressLabel = new List<string>() {"2375 Fair Lakes Court", "Fairfax, VA 22033"},
                        PhoneNumber = "555-555-5555",
                        EmailAddress = "mainfaoffice@ellucian.edu",
                        DirectorName = "Cindy Lou"                                        
                    },
                    new FinancialAidOffice()
                    {
                        Id = "LAW",
                        Name = "Law Office",
                        AddressLabel = new List<string>() {"444 MadeUp Dr.", "Whatever, ST 54321"},
                        PhoneNumber = "666-666-6666",
                        EmailAddress = "lawfaoffice@ellucian.edu",
                        DirectorName = "JD Director"    
                    }
                };

                financialAidOfficeServiceMock.Setup(o => o.GetFinancialAidOffices()).Returns(expectedOffices);
                financialAidOfficesController = new FinancialAidOfficesController(adapterRegistryMock.Object, financialAidOfficeServiceMock.Object, loggerMock.Object);
                actualOffices = financialAidOfficesController.GetFinancialAidOffices();
            }

            [TestCleanup]
            public void Cleanup()
            {
                adapterRegistryMock = null;
                loggerMock = null;
                financialAidOfficeServiceMock = null;
                expectedOffices = null;
                actualOffices = null;
                financialAidOfficesController = null;
            }

            [TestMethod]
            public void PropertiesAreEqualTest()
            {
                foreach (var expectedOffice in expectedOffices)
                {
                    var actualOffice = actualOffices.FirstOrDefault(o => o.Id == expectedOffice.Id);
                    Assert.IsNotNull(actualOffice);

                    Assert.AreEqual(expectedOffice.Name, actualOffice.Name);
                    Assert.AreEqual(expectedOffice.PhoneNumber, actualOffice.PhoneNumber);
                    Assert.AreEqual(expectedOffice.DirectorName, actualOffice.DirectorName);
                    Assert.AreEqual(expectedOffice.EmailAddress, actualOffice.EmailAddress);
                    Assert.AreEqual(expectedOffice.AddressLabel.Count(), actualOffice.AddressLabel.Count());
                    for (int i = 0; i < expectedOffice.AddressLabel.Count(); i++)
                    {
                        Assert.AreEqual(expectedOffice.AddressLabel[i], actualOffice.AddressLabel[i]);
                    }
                }
            }

            [TestMethod]
            public void CatchKeyNotFoundExceptionAndLogMessageTest()
            {
                financialAidOfficeServiceMock.Setup(s => s.GetFinancialAidOffices()).Throws(new KeyNotFoundException("Not FoundException"));

                var exceptionCaught = false;
                try
                {
                    financialAidOfficesController.GetFinancialAidOffices();
                }
                catch (HttpResponseException hre)
                {
                    exceptionCaught = true;
                    Assert.AreEqual(System.Net.HttpStatusCode.NotFound, hre.Response.StatusCode);
                }
                Assert.IsTrue(exceptionCaught);
                loggerMock.Verify(l => l.Error(It.IsAny<KeyNotFoundException>(), It.IsAny<string>()));
            }

            [TestMethod]
            public void CatchUnknownExceptionAndLogMessageTest()
            {
                financialAidOfficeServiceMock.Setup(s => s.GetFinancialAidOffices()).Throws(new Exception("Unknown Exception"));

                var exceptionCaught = false;
                try
                {
                    financialAidOfficesController.GetFinancialAidOffices();
                }
                catch (HttpResponseException hre)
                {
                    exceptionCaught = true;
                    Assert.AreEqual(System.Net.HttpStatusCode.BadRequest, hre.Response.StatusCode);
                }
                Assert.IsTrue(exceptionCaught);
                loggerMock.Verify(l => l.Error(It.IsAny<Exception>(), It.IsAny<string>()));
            }
        }

        [TestClass]
        public class GetFinancialAidOfficesAsyncTests
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
            private Mock<IFinancialAidOfficeService> financialAidOfficeServiceMock;

            private IEnumerable<FinancialAidOffice> expectedOffices;
            private IEnumerable<FinancialAidOffice> actualOffices;

            private FinancialAidOfficesController financialAidOfficesController;

            [TestInitialize]
            public async void Initialize()
            {
                LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
                EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));

                adapterRegistryMock = new Mock<IAdapterRegistry>();
                loggerMock = new Mock<ILogger>();
                financialAidOfficeServiceMock = new Mock<IFinancialAidOfficeService>();

                expectedOffices = new List<FinancialAidOffice>()
                {
                    new FinancialAidOffice()
                    {
                        Id = "MAIN",
                        Name = "Main Office",
                        AddressLabel = new List<string>() {"2375 Fair Lakes Court", "Fairfax, VA 22033"},
                        PhoneNumber = "555-555-5555",
                        EmailAddress = "mainfaoffice@ellucian.edu",
                        DirectorName = "Cindy Lou"                                        
                    },
                    new FinancialAidOffice()
                    {
                        Id = "LAW",
                        Name = "Law Office",
                        AddressLabel = new List<string>() {"444 MadeUp Dr.", "Whatever, ST 54321"},
                        PhoneNumber = "666-666-6666",
                        EmailAddress = "lawfaoffice@ellucian.edu",
                        DirectorName = "JD Director"    
                    }
                };

                financialAidOfficeServiceMock.Setup(o => o.GetFinancialAidOfficesAsync()).ReturnsAsync(expectedOffices);
                financialAidOfficesController = new FinancialAidOfficesController(adapterRegistryMock.Object, financialAidOfficeServiceMock.Object, loggerMock.Object);
                actualOffices = await financialAidOfficesController.GetFinancialAidOfficesAsync();
            }

            [TestCleanup]
            public void Cleanup()
            {
                adapterRegistryMock = null;
                loggerMock = null;
                financialAidOfficeServiceMock = null;
                expectedOffices = null;
                actualOffices = null;
                financialAidOfficesController = null;
            }

            [TestMethod]
            public void PropertiesAreEqualTest()
            {
                foreach (var expectedOffice in expectedOffices)
                {
                    var actualOffice = actualOffices.FirstOrDefault(o => o.Id == expectedOffice.Id);
                    Assert.IsNotNull(actualOffice);

                    Assert.AreEqual(expectedOffice.Name, actualOffice.Name);
                    Assert.AreEqual(expectedOffice.PhoneNumber, actualOffice.PhoneNumber);
                    Assert.AreEqual(expectedOffice.DirectorName, actualOffice.DirectorName);
                    Assert.AreEqual(expectedOffice.EmailAddress, actualOffice.EmailAddress);
                    Assert.AreEqual(expectedOffice.AddressLabel.Count(), actualOffice.AddressLabel.Count());
                    for (int i = 0; i < expectedOffice.AddressLabel.Count(); i++)
                    {
                        Assert.AreEqual(expectedOffice.AddressLabel[i], actualOffice.AddressLabel[i]);
                    }
                }
            }

            [TestMethod]
            public async Task CatchKeyNotFoundExceptionAndLogMessageTest()
            {
                financialAidOfficeServiceMock.Setup(s => s.GetFinancialAidOfficesAsync()).Throws(new KeyNotFoundException("Not FoundException"));

                var exceptionCaught = false;
                try
                {
                    await financialAidOfficesController.GetFinancialAidOfficesAsync();
                }
                catch (HttpResponseException hre)
                {
                    exceptionCaught = true;
                    Assert.AreEqual(System.Net.HttpStatusCode.NotFound, hre.Response.StatusCode);
                }
                Assert.IsTrue(exceptionCaught);
                loggerMock.Verify(l => l.Error(It.IsAny<KeyNotFoundException>(), It.IsAny<string>()));
            }

            [TestMethod]
            public async Task CatchUnknownExceptionAndLogMessageTest()
            {
                financialAidOfficeServiceMock.Setup(s => s.GetFinancialAidOfficesAsync()).Throws(new Exception("Unknown Exception"));

                var exceptionCaught = false;
                try
                {
                    await financialAidOfficesController.GetFinancialAidOfficesAsync();
                }
                catch (HttpResponseException hre)
                {
                    exceptionCaught = true;
                    Assert.AreEqual(System.Net.HttpStatusCode.BadRequest, hre.Response.StatusCode);
                }
                Assert.IsTrue(exceptionCaught);
                loggerMock.Verify(l => l.Error(It.IsAny<Exception>(), It.IsAny<string>()));
            }
        }

        [TestClass]
        public class GetFinancialAidOffices2Tests
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
            private Mock<IFinancialAidOfficeService> financialAidOfficeServiceMock;

            private IEnumerable<FinancialAidOffice2> expectedOffices;
            private IEnumerable<FinancialAidOffice2> actualOffices;

            private FinancialAidOfficesController financialAidOfficesController;

            [TestInitialize]
            public void Initialize()
            {
                LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
                EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));

                adapterRegistryMock = new Mock<IAdapterRegistry>();
                loggerMock = new Mock<ILogger>();
                financialAidOfficeServiceMock = new Mock<IFinancialAidOfficeService>();

                expectedOffices = new List<FinancialAidOffice2>()
                {
                    new FinancialAidOffice2()
                    {
                        Id = "MAIN",
                        Name = "Main Office",
                        AddressLabel = new List<string>() {"2375 Fair Lakes Court", "Fairfax, VA 22033"},
                        PhoneNumber = "555-555-5555",
                        EmailAddress = "mainfaoffice@ellucian.edu",
                        DirectorName = "Cindy Lou"                                        
                    },
                    new FinancialAidOffice2()
                    {
                        Id = "LAW",
                        Name = "Law Office",
                        AddressLabel = new List<string>() {"444 MadeUp Dr.", "Whatever, ST 54321"},
                        PhoneNumber = "666-666-6666",
                        EmailAddress = "lawfaoffice@ellucian.edu",
                        DirectorName = "JD Director"    
                    }
                };

                financialAidOfficeServiceMock.Setup(o => o.GetFinancialAidOffices2()).Returns(expectedOffices);
                financialAidOfficesController = new FinancialAidOfficesController(adapterRegistryMock.Object, financialAidOfficeServiceMock.Object, loggerMock.Object);
                actualOffices = financialAidOfficesController.GetFinancialAidOffices2();
            }

            [TestCleanup]
            public void Cleanup()
            {
                adapterRegistryMock = null;
                loggerMock = null;
                financialAidOfficeServiceMock = null;
                expectedOffices = null;
                actualOffices = null;
                financialAidOfficesController = null;
            }

            [TestMethod]
            public void PropertiesAreEqualTest2()
            {
                foreach (var expectedOffice in expectedOffices)
                {
                    var actualOffice = actualOffices.FirstOrDefault(o => o.Id == expectedOffice.Id);
                    Assert.IsNotNull(actualOffice);

                    Assert.AreEqual(expectedOffice.Name, actualOffice.Name);
                    Assert.AreEqual(expectedOffice.PhoneNumber, actualOffice.PhoneNumber);
                    Assert.AreEqual(expectedOffice.DirectorName, actualOffice.DirectorName);
                    Assert.AreEqual(expectedOffice.EmailAddress, actualOffice.EmailAddress);
                    Assert.AreEqual(expectedOffice.AddressLabel.Count(), actualOffice.AddressLabel.Count());
                    for (int i = 0; i < expectedOffice.AddressLabel.Count(); i++)
                    {
                        Assert.AreEqual(expectedOffice.AddressLabel[i], actualOffice.AddressLabel[i]);
                    }
                }
            }

            [TestMethod]
            public void CatchKeyNotFoundExceptionAndLogMessageTest2()
            {
                financialAidOfficeServiceMock.Setup(s => s.GetFinancialAidOffices2()).Throws(new KeyNotFoundException("Not FoundException"));

                var exceptionCaught = false;
                try
                {
                    financialAidOfficesController.GetFinancialAidOffices2();
                }
                catch (HttpResponseException hre)
                {
                    exceptionCaught = true;
                    Assert.AreEqual(System.Net.HttpStatusCode.NotFound, hre.Response.StatusCode);
                }
                Assert.IsTrue(exceptionCaught);
                loggerMock.Verify(l => l.Error(It.IsAny<KeyNotFoundException>(), It.IsAny<string>()));
            }

            [TestMethod]
            public void CatchUnknownExceptionAndLogMessageTest2()
            {
                financialAidOfficeServiceMock.Setup(s => s.GetFinancialAidOffices2()).Throws(new Exception("Unknown Exception"));

                var exceptionCaught = false;
                try
                {
                    financialAidOfficesController.GetFinancialAidOffices2();
                }
                catch (HttpResponseException hre)
                {
                    exceptionCaught = true;
                    Assert.AreEqual(System.Net.HttpStatusCode.BadRequest, hre.Response.StatusCode);
                }
                Assert.IsTrue(exceptionCaught);
                loggerMock.Verify(l => l.Error(It.IsAny<Exception>(), It.IsAny<string>()));
            }
        }

        [TestClass]
        public class GetFinancialAidOffices2AsyncTests
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
            private Mock<IFinancialAidOfficeService> financialAidOfficeServiceMock;

            private IEnumerable<FinancialAidOffice2> expectedOffices;
            private IEnumerable<FinancialAidOffice2> actualOffices;

            private FinancialAidOfficesController financialAidOfficesController;

            [TestInitialize]
            public async void Initialize()
            {
                LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
                EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));

                adapterRegistryMock = new Mock<IAdapterRegistry>();
                loggerMock = new Mock<ILogger>();
                financialAidOfficeServiceMock = new Mock<IFinancialAidOfficeService>();

                expectedOffices = new List<FinancialAidOffice2>()
                {
                    new FinancialAidOffice2()
                    {
                        Id = "MAIN",
                        Name = "Main Office",
                        AddressLabel = new List<string>() {"2375 Fair Lakes Court", "Fairfax, VA 22033"},
                        PhoneNumber = "555-555-5555",
                        EmailAddress = "mainfaoffice@ellucian.edu",
                        DirectorName = "Cindy Lou"                                        
                    },
                    new FinancialAidOffice2()
                    {
                        Id = "LAW",
                        Name = "Law Office",
                        AddressLabel = new List<string>() {"444 MadeUp Dr.", "Whatever, ST 54321"},
                        PhoneNumber = "666-666-6666",
                        EmailAddress = "lawfaoffice@ellucian.edu",
                        DirectorName = "JD Director"    
                    }
                };

                financialAidOfficeServiceMock.Setup(o => o.GetFinancialAidOffices2Async()).ReturnsAsync(expectedOffices);
                financialAidOfficesController = new FinancialAidOfficesController(adapterRegistryMock.Object, financialAidOfficeServiceMock.Object, loggerMock.Object);
                actualOffices = await financialAidOfficesController.GetFinancialAidOffices2Async();
            }

            [TestCleanup]
            public void Cleanup()
            {
                adapterRegistryMock = null;
                loggerMock = null;
                financialAidOfficeServiceMock = null;
                expectedOffices = null;
                actualOffices = null;
                financialAidOfficesController = null;
            }

            [TestMethod]
            public void PropertiesAreEqualTest2()
            {
                foreach (var expectedOffice in expectedOffices)
                {
                    var actualOffice = actualOffices.FirstOrDefault(o => o.Id == expectedOffice.Id);
                    Assert.IsNotNull(actualOffice);

                    Assert.AreEqual(expectedOffice.Name, actualOffice.Name);
                    Assert.AreEqual(expectedOffice.PhoneNumber, actualOffice.PhoneNumber);
                    Assert.AreEqual(expectedOffice.DirectorName, actualOffice.DirectorName);
                    Assert.AreEqual(expectedOffice.EmailAddress, actualOffice.EmailAddress);
                    Assert.AreEqual(expectedOffice.AddressLabel.Count(), actualOffice.AddressLabel.Count());
                    for (int i = 0; i < expectedOffice.AddressLabel.Count(); i++)
                    {
                        Assert.AreEqual(expectedOffice.AddressLabel[i], actualOffice.AddressLabel[i]);
                    }
                }
            }

            [TestMethod]
            public async Task CatchKeyNotFoundExceptionAndLogMessageTest2()
            {
                financialAidOfficeServiceMock.Setup(s => s.GetFinancialAidOffices2Async()).Throws(new KeyNotFoundException("Not FoundException"));

                var exceptionCaught = false;
                try
                {
                    await financialAidOfficesController.GetFinancialAidOffices2Async();
                }
                catch (HttpResponseException hre)
                {
                    exceptionCaught = true;
                    Assert.AreEqual(System.Net.HttpStatusCode.NotFound, hre.Response.StatusCode);
                }
                Assert.IsTrue(exceptionCaught);
                loggerMock.Verify(l => l.Error(It.IsAny<KeyNotFoundException>(), It.IsAny<string>()));
            }

            [TestMethod]
            public async Task CatchUnknownExceptionAndLogMessageTest2()
            {
                financialAidOfficeServiceMock.Setup(s => s.GetFinancialAidOffices2Async()).Throws(new Exception("Unknown Exception"));

                var exceptionCaught = false;
                try
                {
                    await financialAidOfficesController.GetFinancialAidOffices2Async();
                }
                catch (HttpResponseException hre)
                {
                    exceptionCaught = true;
                    Assert.AreEqual(System.Net.HttpStatusCode.BadRequest, hre.Response.StatusCode);
                }
                Assert.IsTrue(exceptionCaught);
                loggerMock.Verify(l => l.Error(It.IsAny<Exception>(), It.IsAny<string>()));
            }
        }        
    }
}
