﻿// Copyright 2022 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using Ellucian.Colleague.Api.Controllers;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Domain.Student.Repositories;
using Ellucian.Colleague.Dtos.Student;
using Ellucian.Data.Colleague.Exceptions;
using Ellucian.Web.Adapters;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;

namespace Ellucian.Colleague.Api.Tests.Controllers.Student
{
    public class MinorsControllerTests
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

        private IStudentReferenceDataRepository referenceDataRepository;
        private Mock<IStudentReferenceDataRepository> referenceDataRepositoryMock;
        private IAdapterRegistry adapterRegistry;
        private Mock<IAdapterRegistry> adapterRegistryMock;
        private ILogger logger;

        private MinorsController minorsController;
        private IEnumerable<Domain.Student.Entities.Minor> minors;

        [TestInitialize]
        public void Initialize()
        {
            LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
            EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));

            referenceDataRepositoryMock = new Mock<IStudentReferenceDataRepository>();
            referenceDataRepository = referenceDataRepositoryMock.Object;
            logger = new Mock<ILogger>().Object;

            adapterRegistryMock = new Mock<IAdapterRegistry>();
            adapterRegistry = adapterRegistryMock.Object;
            var adapter = new AutoMapperAdapter<Domain.Student.Entities.Minor, Minor>(adapterRegistry, logger);
            adapterRegistryMock.Setup(reg => reg.GetAdapter<Domain.Student.Entities.Minor, Minor>()).Returns(adapter);

            minors = BuildMinors();
            minorsController = new MinorsController(adapterRegistry, referenceDataRepository, logger);
        }

        [TestCleanup]
        public void Cleanup()
        {
            minorsController = null;
            referenceDataRepository = null;
        }

        [TestMethod]
        public async Task MinorsController_ReturnsMinorDtos()
        {
            referenceDataRepositoryMock.Setup(x => x.GetMinorsAsync(It.IsAny<bool>())).Returns(Task.FromResult(minors));

            var minorDtos = await minorsController.GetAsync();

            Assert.IsTrue(minorDtos is IEnumerable<Dtos.Student.Minor>);
            Assert.AreEqual(minors.Count(), minorDtos.Count());
        }

        [TestMethod]
        public async Task MinorsController_NullRepositoryResponse_ReturnsEmptyMinorDtos()
        {
            IEnumerable<Domain.Student.Entities.Minor> nullMinorEntities = null;
            referenceDataRepositoryMock.Setup(x => x.GetMinorsAsync(It.IsAny<bool>())).Returns(Task.FromResult(nullMinorEntities));

            var majorDtos = await minorsController.GetAsync();

            Assert.IsTrue(majorDtos is IEnumerable<Dtos.Student.Minor>);
            Assert.AreEqual(0, majorDtos.Count());
        }

        [TestMethod]
        public async Task MinorsController_EmptyRepositoryResponse_ReturnsEmptyMinorDtos()
        {
            IEnumerable<Domain.Student.Entities.Minor> emptyMinorEntities = new List<Domain.Student.Entities.Minor>();
            referenceDataRepositoryMock.Setup(x => x.GetMinorsAsync(It.IsAny<bool>())).Returns(Task.FromResult(emptyMinorEntities));

            var majorDtos = await minorsController.GetAsync();

            Assert.IsTrue(majorDtos is IEnumerable<Dtos.Student.Minor>);
            Assert.AreEqual(0, majorDtos.Count());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task MinorsController_ColleagueSessionExpiredException_ReturnsHttpResponseException_Unauthorized()
        {
            try
            {
                referenceDataRepositoryMock.Setup(x => x.GetMinorsAsync(It.IsAny<bool>())).Throws(new ColleagueSessionExpiredException("session expired"));
                await minorsController.GetAsync();
            }
            catch (HttpResponseException ex)
            {
                Assert.AreEqual(System.Net.HttpStatusCode.Unauthorized, ex.Response.StatusCode);
                throw ex;
            }
            catch (System.Exception e)
            {
                throw e;
            }
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task MinorsController_Exception_ReturnsHttpResponseException_BadRequest()
        {
            try
            {
                referenceDataRepositoryMock.Setup(x => x.GetMinorsAsync(It.IsAny<bool>())).Throws(new ApplicationException());
                await minorsController.GetAsync();
            }
            catch (HttpResponseException ex)
            {
                Assert.AreEqual(System.Net.HttpStatusCode.BadRequest, ex.Response.StatusCode);
                throw ex;
            }
            catch (System.Exception e)
            {
                throw e;
            }
        }

        private IEnumerable<Domain.Student.Entities.Minor> BuildMinors()
        {
            var minors = new List<Domain.Student.Entities.Minor>()
                {
                    new Domain.Student.Entities.Minor("AVIA","Aviation"),
                    new Domain.Student.Entities.Minor("BUSN","Business Administration"),
                    new Domain.Student.Entities.Minor("COMP","Computer Science"),
                    new Domain.Student.Entities.Minor("POLI","Political Science"),
                };
            return minors;
        }
    }
}