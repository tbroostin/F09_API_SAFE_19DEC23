// Copyright 2016-2018 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ellucian.Web.Adapters;
using Moq;
using slf4net;
using Ellucian.Colleague.Api.Controllers.FinancialAid;
using Ellucian.Colleague.Domain.FinancialAid.Repositories;
using Ellucian.Colleague.Domain.FinancialAid.Tests;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Dtos.FinancialAid;
using Ellucian.Web.Http.TestUtil;
using System.Web.Http;

namespace Ellucian.Colleague.Api.Tests.Controllers.FinancialAid
{
    [TestClass]
    public class AcademicProgressAppealCodesControllerTests
    {
        public Mock<IAdapterRegistry> adapterRegistryMock;
        public Mock<ILogger> loggerMock;
        public Mock<IFinancialAidReferenceDataRepository> referenceDataRepositoryMock;

        public TestFinancialAidReferenceDataRepository dataRepository;

        public AcademicProgressAppealCodesController actualController;

        public AutoMapperAdapter<Colleague.Domain.FinancialAid.Entities.AcademicProgressAppealCode, Colleague.Dtos.FinancialAid.AcademicProgressAppealCode> adapter
        {
            get { return new AutoMapperAdapter<Domain.FinancialAid.Entities.AcademicProgressAppealCode, Dtos.FinancialAid.AcademicProgressAppealCode>(adapterRegistryMock.Object, loggerMock.Object); }
        }
        public List<AcademicProgressAppealCode> expectedAppealCodes
        {
            get { return dataRepository.GetAcademicProgressAppealCodesAsync().Result.Select(s => adapter.MapToType(s)).ToList(); }
        }

        public FunctionEqualityComparer<AcademicProgressAppealCode> comparer;

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
            adapterRegistryMock = new Mock<IAdapterRegistry>();
            loggerMock = new Mock<ILogger>();
            referenceDataRepositoryMock = new Mock<IFinancialAidReferenceDataRepository>();

            dataRepository = new TestFinancialAidReferenceDataRepository();

            referenceDataRepositoryMock.Setup(r => r.GetAcademicProgressAppealCodesAsync()).Returns(dataRepository.GetAcademicProgressAppealCodesAsync());

            adapterRegistryMock.Setup(r => r.GetAdapter<Colleague.Domain.FinancialAid.Entities.AcademicProgressAppealCode, Colleague.Dtos.FinancialAid.AcademicProgressAppealCode>())
                .Returns(() => new AutoMapperAdapter<Colleague.Domain.FinancialAid.Entities.AcademicProgressAppealCode, Colleague.Dtos.FinancialAid.AcademicProgressAppealCode>(adapterRegistryMock.Object, loggerMock.Object));

            comparer = new FunctionEqualityComparer<AcademicProgressAppealCode>((s1, s2) => s1.Code == s2.Code, (s) => s.Code.GetHashCode()); 

            actualController = new AcademicProgressAppealCodesController(adapterRegistryMock.Object, loggerMock.Object, referenceDataRepositoryMock.Object);
        }

        [TestMethod]
        public async Task ActualAppealCodes_EqualExpectedTest()
        {
            CollectionAssert.AreEqual(expectedAppealCodes, (await actualController.GetAcademicProgressAppealCodesAsync()).ToList(), comparer);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task CatchesGenericExceptionAndThrowsHttpResponseExceptionTest()
        {
            referenceDataRepositoryMock.Setup(r => r.GetAcademicProgressAppealCodesAsync()).Throws(new Exception());
            actualController = new AcademicProgressAppealCodesController(adapterRegistryMock.Object, loggerMock.Object, referenceDataRepositoryMock.Object);
            await actualController.GetAcademicProgressAppealCodesAsync();
        }
    }
}
