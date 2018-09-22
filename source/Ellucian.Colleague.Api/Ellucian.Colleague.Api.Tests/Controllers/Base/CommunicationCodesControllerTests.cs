/* Copyright 2015-2018 Ellucian Company L.P. and its affiliates.*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web.Http;
using Ellucian.Colleague.Api.Controllers;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.Base.Adapters;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.Base.Tests.Entities;
using Ellucian.Colleague.Dtos.Base;
using Ellucian.Web.Adapters;
using Ellucian.Web.Http.TestUtil;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;

namespace Ellucian.Colleague.Api.Tests.Controllers.Base
{
    [TestClass]
    public class CommunicationCodesControllerTests
    {
        public Mock<IAdapterRegistry> adapterRegistryMock;
        public Mock<ILogger> loggerMock;
        public Mock<IReferenceDataRepository> referenceDataRepositoryMock;

        public TestCommunicationCodesRepository communicationCodesRepository;
        public CommunicationCodesController actualController;

        public FunctionEqualityComparer<CommunicationCode> communicationCodeDtoComparer;
        public FunctionEqualityComparer<CommunicationCode2> communicationCode2DtoComparer;

        public void CommunicationCodesControllerTestsInitialize()
        {
            adapterRegistryMock = new Mock<IAdapterRegistry>();
            loggerMock = new Mock<ILogger>();
            referenceDataRepositoryMock = new Mock<IReferenceDataRepository>();

            communicationCodesRepository = new TestCommunicationCodesRepository();

            communicationCodeDtoComparer = new FunctionEqualityComparer<CommunicationCode>(
                (c1, c2) => c1.Code == c2.Code && c1.AwardYear == c2.AwardYear,
                (c) => c.Code.GetHashCode() ^ c.AwardYear.GetHashCode());

            communicationCode2DtoComparer = new FunctionEqualityComparer<CommunicationCode2>(
                (c1, c2) => c1.Code == c2.Code && c1.AwardYear == c2.AwardYear && c1.Hyperlinks.Count() == c2.Hyperlinks.Count(),
                (c) => c.Code.GetHashCode() ^ c.AwardYear.GetHashCode());
        }

        [TestClass]
        public class GetCommuncationCodesTests : CommunicationCodesControllerTests
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
            public List<CommunicationCode> expectedCommunicationCodeDtos;
            public List<CommunicationCode> actualCommunicationCodeDtos
            { get { return actualController.GetCommunicationCodes().ToList(); } }
            public AutoMapperAdapter<Domain.Base.Entities.CommunicationCode, Dtos.Base.CommunicationCode> communicationCodeEntityToDtoAdapter;

            [TestInitialize]
            public void Initialize()
            {
                CommunicationCodesControllerTestsInitialize();
                LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
                EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));

                referenceDataRepositoryMock.Setup(r => r.CommunicationCodes)
                    .Returns(() => communicationCodesRepository.GetCommunicationCodeEntities());

                communicationCodeEntityToDtoAdapter = new AutoMapperAdapter<Domain.Base.Entities.CommunicationCode, Dtos.Base.CommunicationCode>(adapterRegistryMock.Object, loggerMock.Object);

                adapterRegistryMock.Setup(r => r.GetAdapter<Domain.Base.Entities.CommunicationCode, Dtos.Base.CommunicationCode>())
                    .Returns(() => communicationCodeEntityToDtoAdapter);

                expectedCommunicationCodeDtos = communicationCodesRepository.GetCommunicationCodeEntities()
                    .Select(c => communicationCodeEntityToDtoAdapter.MapToType(c)).ToList();

                actualController = new CommunicationCodesController(adapterRegistryMock.Object, referenceDataRepositoryMock.Object, loggerMock.Object);
            }

            [TestMethod]
            public void ExpectedEqualsActualTest()
            {
                CollectionAssert.AreEqual(expectedCommunicationCodeDtos, actualCommunicationCodeDtos, communicationCodeDtoComparer);
            }

            [TestMethod]
            public void FirstUrlTest()
            {
                foreach (var communicationDto in actualCommunicationCodeDtos)
                {
                    var communicationDomain = communicationCodesRepository.GetCommunicationCodeEntities().First(c => c.Code == communicationDto.Code && c.AwardYear == communicationDto.AwardYear);
                    var firstLink = communicationDomain.Hyperlinks.FirstOrDefault();
                    if (firstLink == null)
                    {
                        Assert.AreEqual(string.Empty, communicationDto.Url);
                    }
                    else
                    {
                        Assert.AreEqual(firstLink.Url, communicationDto.Url);
                    }
                }
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public void CatchGenericExceptionTest()
            {
                referenceDataRepositoryMock.Setup(r => r.CommunicationCodes)
                    .Throws(new Exception("ex"));

                try
                {
                    var test = actualCommunicationCodeDtos;
                }
                catch (HttpResponseException hre)
                {
                    loggerMock.Verify(l => l.Error(It.IsAny<Exception>(), It.IsAny<string>()));
                    Assert.AreEqual(HttpStatusCode.BadRequest, hre.Response.StatusCode);
                    throw;
                }
            }
        }

        [TestClass]
        public class GetCommunicationCodes2Tests : CommunicationCodesControllerTests
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

            public List<CommunicationCode2> expectedCommunicationCodeDtos;
            public List<CommunicationCode2> actualCommunicationCodeDtos
            { get { return actualController.GetCommunicationCodes2().ToList(); } }
            public AutoMapperAdapter<Domain.Base.Entities.CommunicationCode, Dtos.Base.CommunicationCode2> communicationCodeEntityToDtoAdapter;

            [TestInitialize]
            public void Initialize()
            {
                CommunicationCodesControllerTestsInitialize();
                LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
                EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));

                referenceDataRepositoryMock.Setup(r => r.CommunicationCodes)
                    .Returns(() => communicationCodesRepository.GetCommunicationCodeEntities());

                communicationCodeEntityToDtoAdapter = new CommunicationCodeEntityToDto2Adapter(adapterRegistryMock.Object, loggerMock.Object);

                adapterRegistryMock.Setup(r => r.GetAdapter<Domain.Base.Entities.CommunicationCode, Dtos.Base.CommunicationCode2>())
                    .Returns(() => communicationCodeEntityToDtoAdapter);

                expectedCommunicationCodeDtos = communicationCodesRepository.GetCommunicationCodeEntities()
                    .Select(c => communicationCodeEntityToDtoAdapter.MapToType(c)).ToList();

                actualController = new CommunicationCodesController(adapterRegistryMock.Object, referenceDataRepositoryMock.Object, loggerMock.Object);
           
            }

            [TestMethod]
            public void ExpectedEqualsActualTest()
            {
                CollectionAssert.AreEqual(expectedCommunicationCodeDtos, actualCommunicationCodeDtos, communicationCode2DtoComparer);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public void CatchGenericExceptionTest()
            {
                referenceDataRepositoryMock.Setup(r => r.CommunicationCodes)
                    .Throws(new Exception("ex"));

                try
                {
                    var test = actualCommunicationCodeDtos;
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
