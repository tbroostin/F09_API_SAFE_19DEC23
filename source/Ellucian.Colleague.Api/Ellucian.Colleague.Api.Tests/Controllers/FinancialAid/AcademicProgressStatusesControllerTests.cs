// Copyright 2015-2018 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Api.Controllers.FinancialAid;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Domain.FinancialAid.Repositories;
using Ellucian.Colleague.Domain.FinancialAid.Tests;
using Ellucian.Colleague.Dtos.FinancialAid;
using Ellucian.Web.Adapters;
using Ellucian.Web.Http.TestUtil;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;

namespace Ellucian.Colleague.Api.Tests.Controllers.FinancialAid
{
    [TestClass]
    public class AcademicProgressStatusesControllerTests
    {
        public Mock<IAdapterRegistry> adapterRegistryMock;
        public Mock<ILogger> loggerMock;
        public Mock<IFinancialAidReferenceDataRepository> referenceDataRepositoryMock;

        public TestFinancialAidReferenceDataRepository dataRepository;
        public AcademicProgressStatusesController actualController;

        public FunctionEqualityComparer<AcademicProgressStatus> comparer;

        public void AcademicProgressStatusesControllerTestsInitialize()
        {
            adapterRegistryMock = new Mock<IAdapterRegistry>();
            loggerMock = new Mock<ILogger>();
            referenceDataRepositoryMock = new Mock<IFinancialAidReferenceDataRepository>();

            dataRepository = new TestFinancialAidReferenceDataRepository();
            comparer = new FunctionEqualityComparer<AcademicProgressStatus>((s1, s2) => s1.Code == s2.Code, (s) => s.Code.GetHashCode()); 
        }

        [TestClass]
        public class GetAcademicProgressStatusesTests : AcademicProgressStatusesControllerTests
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

            public AutoMapperAdapter<Colleague.Domain.FinancialAid.Entities.AcademicProgressStatus, Colleague.Dtos.FinancialAid.AcademicProgressStatus> adapter
            {
                get { return new AutoMapperAdapter<Domain.FinancialAid.Entities.AcademicProgressStatus, Dtos.FinancialAid.AcademicProgressStatus>(adapterRegistryMock.Object, loggerMock.Object); }
            }
            public List<AcademicProgressStatus> expectedStatuses
            {
                get { return dataRepository.GetAcademicProgressStatusesAsync().Result.Select(s => adapter.MapToType(s)).ToList(); }
            }
            
            [TestInitialize]
            public void Initialize()
            {
                AcademicProgressStatusesControllerTestsInitialize();


                LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
                EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));

                //referenceDataRepositoryMock.Setup(r => r.BudgetComponents)
                //    .Returns(() => dataRepository.BudgetComponents);

                referenceDataRepositoryMock.Setup(r => r.GetAcademicProgressStatusesAsync()).Returns(dataRepository.GetAcademicProgressStatusesAsync());

                adapterRegistryMock.Setup(r => r.GetAdapter<Colleague.Domain.FinancialAid.Entities.AcademicProgressStatus, Colleague.Dtos.FinancialAid.AcademicProgressStatus>())
                    .Returns(() => new AutoMapperAdapter<Colleague.Domain.FinancialAid.Entities.AcademicProgressStatus, Colleague.Dtos.FinancialAid.AcademicProgressStatus>(adapterRegistryMock.Object, loggerMock.Object));

                actualController = new AcademicProgressStatusesController(adapterRegistryMock.Object, loggerMock.Object, referenceDataRepositoryMock.Object);
            }
            
            [TestMethod]
            public async Task ExpectedEqualsActualTest()
            {
                var actual = await actualController.GetAcademicProgressStatusesAsync();

                CollectionAssert.AreEqual(expectedStatuses, actual.ToList(), comparer);
            }
            
            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task CatchGenericExceptionTest()
            {
                referenceDataRepositoryMock.Setup(r => r.GetAcademicProgressStatusesAsync()).Throws(new Exception());

                try
                {
                    var actual = await actualController.GetAcademicProgressStatusesAsync();
                }
                catch (HttpResponseException)
                {
                    loggerMock.Verify(l => l.Error(It.IsAny<Exception>(), It.IsAny<string>()));
                    throw;
                }
            }
        }

    }
}
