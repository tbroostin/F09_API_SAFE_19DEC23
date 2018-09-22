/*Copyright 2017-2018 Ellucian Company L.P. and its affiliates.*/
using Ellucian.Colleague.Api.Controllers.FinancialAid;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.Base;
using Ellucian.Colleague.Coordination.FinancialAid.Services;
using Ellucian.Colleague.Dtos.Base;
using Ellucian.Colleague.Dtos.FinancialAid;
using Ellucian.Web.Adapters;
using Ellucian.Web.Security;
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

namespace Ellucian.Colleague.Api.Tests.Controllers.FinancialAid
{
    [TestClass]
    public class FinancialAidPersonControllerTests
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
        private Mock<IFinancialAidPersonService> financialAidPersonServiceMock;

        private FinancialAidPersonQueryCriteria searchCriteria;

        private List<Person> expectedPersons;
        private List<Person> actualPersons;
        private PrivacyWrapper<IEnumerable<Person>> privacyWrapper;

        private FinancialAidPersonController financialAidPersonController;

        [TestInitialize]
        public void Initialize()
        {
            LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
            EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));

            adapterRegistryMock = new Mock<IAdapterRegistry>();
            loggerMock = new Mock<ILogger>();
            financialAidPersonServiceMock = new Mock<IFinancialAidPersonService>();

            searchCriteria = new FinancialAidPersonQueryCriteria()
            {
                FinancialAidPersonIds = new List<string>(),
                FinancialAidPersonQueryKeyword = "John Smith"
            };

            expectedPersons = new List<Person>()
            {
                new Person()
                {
                    Id = "0004567",
                    PreferredName = "John Smith"
                }
            };

            privacyWrapper = new PrivacyWrapper<IEnumerable<Person>>(expectedPersons.AsEnumerable(), false) { HasPrivacyRestrictions = false };
            financialAidPersonServiceMock.Setup(s => s.SearchFinancialAidPersonsAsync(searchCriteria)).ReturnsAsync(privacyWrapper);
            financialAidPersonController = new FinancialAidPersonController(financialAidPersonServiceMock.Object, adapterRegistryMock.Object, loggerMock.Object);
        }

        [TestMethod]
        public async Task QueryFinancialAidPersonsByPostAsync_ReturnsExpectedResultTest()
        {
            actualPersons = (await financialAidPersonController.QueryFinancialAidPersonsByPostAsync(searchCriteria)).ToList();
            CollectionAssert.AreEqual(expectedPersons, actualPersons);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task NullSearchCriteria_HttpResponseExceptionThrownTest()
        {
            await financialAidPersonController.QueryFinancialAidPersonsByPostAsync(null);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task NoSearchCriteria_HttpResponseExceptionThrownTest()
        {
            await financialAidPersonController.QueryFinancialAidPersonsByPostAsync(new FinancialAidPersonQueryCriteria());
        }

        [TestMethod]
        public async Task PermissionsExceptionIsCaught_HttpResponseExceptionThrownTest()
        {
            financialAidPersonServiceMock.Setup(s => s.SearchFinancialAidPersonsAsync(searchCriteria)).Throws(new PermissionsException("pex"));

            var exceptionCaught = false;
            try
            {
                await financialAidPersonController.QueryFinancialAidPersonsByPostAsync(searchCriteria);
            }
            catch (HttpResponseException hre)
            {
                exceptionCaught = true;
                Assert.AreEqual(HttpStatusCode.Forbidden, hre.Response.StatusCode);
            }

            Assert.IsTrue(exceptionCaught);
            loggerMock.Verify(l => l.Error(It.IsAny<PermissionsException>(), It.IsAny<string>()));
        }

        [TestMethod]
        public async Task ApplicationExceptionIsCaught_HttpResponseExceptionThrownTest()
        {
            financialAidPersonServiceMock.Setup(s => s.SearchFinancialAidPersonsAsync(searchCriteria)).Throws(new ApplicationException("ae"));

            var exceptionCaught = false;
            try
            {
                await financialAidPersonController.QueryFinancialAidPersonsByPostAsync(searchCriteria);
            }
            catch (HttpResponseException hre)
            {
                exceptionCaught = true;
                Assert.AreEqual(HttpStatusCode.BadRequest, hre.Response.StatusCode);
            }

            Assert.IsTrue(exceptionCaught);
            loggerMock.Verify(l => l.Error(It.IsAny<ApplicationException>(), It.IsAny<string>()));
        }

        [TestMethod]
        public async Task GenericExceptionIsCaught_HttpResponseExceptionThrownTest()
        {
            financialAidPersonServiceMock.Setup(s => s.SearchFinancialAidPersonsAsync(searchCriteria)).Throws(new Exception("e"));

            var exceptionCaught = false;
            try
            {
                await financialAidPersonController.QueryFinancialAidPersonsByPostAsync(searchCriteria);
            }
            catch
            {
                exceptionCaught = true;
            }

            Assert.IsTrue(exceptionCaught);
            loggerMock.Verify(l => l.Error(It.IsAny<Exception>(), It.IsAny<string>()));
        }
    }
}
