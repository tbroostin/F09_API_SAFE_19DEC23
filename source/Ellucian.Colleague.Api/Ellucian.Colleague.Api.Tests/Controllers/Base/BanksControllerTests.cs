//Copyright 2015-2018 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Api.Controllers.Base;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Dtos.Base;
using Ellucian.Web.Http.TestUtil;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;
using System;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;
using Ellucian.Web.Adapters;
using System.Collections.Generic;

namespace Ellucian.Colleague.Api.Tests.Controllers.Base
{
    [TestClass]
    public class BanksControllerTests
    {
        public Mock<ILogger> loggerMock;
        public Mock<IBankRepository> bankRepositoryMock;
        public Mock<IAdapterRegistry> adapterRegistryMock;        
        public BanksController controllerUnderTest;
        public Bank bankDto;
        public Domain.Base.Entities.Bank bankEntity;
        public Dictionary<string, Domain.Base.Entities.Bank> bankEntities;
        public FunctionEqualityComparer<Bank> bankDtoComparer;
        public string bankId = "011000015";
        public string bankName = "FEDERAL RESERVE BANK";        

        public void BanksControllerTestsInitialize()
        {
            loggerMock = new Mock<ILogger>();
            bankRepositoryMock = new Mock<IBankRepository>();
            adapterRegistryMock = new Mock<IAdapterRegistry>();
            bankEntity = new Domain.Base.Entities.Bank(bankId, bankName, bankId);

            bankRepositoryMock.Setup(b => b.GetBankAsync(It.IsAny<string>()))
                .Returns<string>((id) => Task.FromResult(bankEntity));

            var bankDtoAdapter = new AutoMapperAdapter<Domain.Base.Entities.Bank, Bank>(adapterRegistryMock.Object, loggerMock.Object);
            adapterRegistryMock.Setup(x => x.GetAdapter<Domain.Base.Entities.Bank, Bank>()).Returns(bankDtoAdapter);          
  
            controllerUnderTest = new BanksController(loggerMock.Object, bankRepositoryMock.Object, adapterRegistryMock.Object);
            
        }

        [TestClass]
        public class GetBankAsyncTests : BanksControllerTests
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

            public string someBankId { get; set;}

            public async Task<Bank> getActualBank()
            {
                someBankId = bankId;
                return (await controllerUnderTest.GetBankAsync(someBankId));
            }

            [TestInitialize]
            public void Initialize()
            {
                LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
                EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));
                base.BanksControllerTestsInitialize();
            }

            [TestMethod]
            public async Task EntityMapsToDtoTest()
            {
                
                BanksControllerTestsInitialize();
                var actualBank = await getActualBank();

                Assert.AreEqual(bankId, actualBank.Id);
                Assert.AreEqual(bankName, actualBank.Name);
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task InputBankIdRequiredTest()
            {
                bankId = null;
                try
                {
                    await getActualBank();
                }
                catch (HttpResponseException hre)
                {
                    Assert.AreEqual(HttpStatusCode.BadRequest, hre.Response.StatusCode);
                    throw;
                }
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task CatchGenericExceptionTest()
            {
                bankRepositoryMock.Setup(s => s.GetBankAsync(It.IsAny<string>()))
                    .Throws(new Exception());

                try
                {
                    await getActualBank();
                }
                catch (HttpResponseException hre)
                {
                    loggerMock.Verify(l => l.Error(It.IsAny<Exception>(), It.IsAny<string>()));
                    Assert.AreEqual(HttpStatusCode.BadRequest, hre.Response.StatusCode);
                    throw;
                }
            }
        }
    }
}
