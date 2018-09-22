// Copyright 2017-2018 Ellucian Company L.P. and its affiliates.
using AutoMapper;
using Ellucian.Colleague.Api.Controllers.Student;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Domain.Student.Entities;
using Ellucian.Colleague.Domain.Student.Repositories;
using Ellucian.Colleague.Domain.Student.Tests;
using Ellucian.Data.Colleague.Exceptions;
using Ellucian.Web.Adapters;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Hosting;

namespace Ellucian.Colleague.Api.Tests.Controllers.Student
{
    [TestClass]
    public class NonAcademicAttendanceEventTypesControllerTests
    {
        public TestContext TestContext { get; set; }

        private List<NonAcademicAttendanceEventType> allNonAcademicAttendanceEventTypeEntities;
        private List<Dtos.Student.NonAcademicAttendanceEventType> allNonAcademicAttendanceEventTypeDtos;

        private Mock<IStudentReferenceDataRepository> studentReferenceDataRepositoryMock;
        private IStudentReferenceDataRepository studentReferenceDataRepository;
        private IAdapterRegistry _adapterRegistry;
        private readonly ILogger _logger = new Mock<ILogger>().Object;
        private NonAcademicAttendanceEventTypesController controller;

        [TestInitialize]
        public async void Initialize()
        {
            LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
            EllucianLicenseProvider.RefreshLicense(Path.Combine(TestContext.TestDeploymentDir, "App_Data"));
            studentReferenceDataRepositoryMock = new Mock<IStudentReferenceDataRepository>();
            studentReferenceDataRepository = studentReferenceDataRepositoryMock.Object;

            allNonAcademicAttendanceEventTypeDtos = new List<Dtos.Student.NonAcademicAttendanceEventType>();
            var adapters = new HashSet<ITypeAdapter>();
            _adapterRegistry = new AdapterRegistry(adapters, _logger);
            var testAdapter = new AutoMapperAdapter<NonAcademicAttendanceEventType, Dtos.Student.NonAcademicAttendanceEventType>(_adapterRegistry, _logger);
            _adapterRegistry.AddAdapter(testAdapter);

            allNonAcademicAttendanceEventTypeEntities = (await new TestStudentReferenceDataRepository().GetNonAcademicAttendanceEventTypesAsync(false)) as List<NonAcademicAttendanceEventType>;

            Mapper.CreateMap<NonAcademicAttendanceEventType, Dtos.Student.NonAcademicAttendanceEventType>();
            Debug.Assert(allNonAcademicAttendanceEventTypeEntities != null, "allNonAcademicAttendanceEventTypeEntities != null");
            foreach (var entity in allNonAcademicAttendanceEventTypeEntities)
            {
                var target = Mapper.Map<NonAcademicAttendanceEventType, Dtos.Student.NonAcademicAttendanceEventType>(entity);
                allNonAcademicAttendanceEventTypeDtos.Add(target);
            }
            studentReferenceDataRepositoryMock.Setup(s => s.GetNonAcademicAttendanceEventTypesAsync(It.IsAny<bool>()))
                .ReturnsAsync(allNonAcademicAttendanceEventTypeEntities);

            controller = new NonAcademicAttendanceEventTypesController(_adapterRegistry, studentReferenceDataRepository, _logger)
            {
                Request = new HttpRequestMessage()
            };
            controller.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey,
                new HttpConfiguration());
            controller.Request.Headers.CacheControl = new CacheControlHeaderValue
            {
                NoCache = true,
                Public = true
            };
        }

        [TestClass]
        public class NonAcademicAttendanceEventTypesController_GetAsync_Tests : NonAcademicAttendanceEventTypesControllerTests
        {
            [TestInitialize]
            public void NonAcademicAttendanceEventTypesControllerTests_Initialize()
            {
                base.Initialize();
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task NonAcademicAttendanceEventTypesController_GetAsync_Repository_returns_null()
            {
                allNonAcademicAttendanceEventTypeEntities = null;
                studentReferenceDataRepositoryMock.Setup(s => s.GetNonAcademicAttendanceEventTypesAsync(It.IsAny<bool>()))
                    .ReturnsAsync(allNonAcademicAttendanceEventTypeEntities);
                controller = new NonAcademicAttendanceEventTypesController(_adapterRegistry, studentReferenceDataRepository, _logger)
                {
                    Request = new HttpRequestMessage()
                };
                controller.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey,
                    new HttpConfiguration());
                controller.Request.Headers.CacheControl = new CacheControlHeaderValue
                {
                    NoCache = true,
                    Public = true
                };

                var dtos = await controller.GetAsync();

                CollectionAssert.AreEqual(new List<Dtos.Student.NonAcademicAttendanceEventType>(), dtos.ToList());
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task NonAcademicAttendanceEventTypesController_GetAsync_data_read_error()
            {
                studentReferenceDataRepositoryMock.Setup(s => s.GetNonAcademicAttendanceEventTypesAsync(It.IsAny<bool>()))
                    .ThrowsAsync(new ColleagueDataReaderException("Could not read file."));
                controller = new NonAcademicAttendanceEventTypesController(_adapterRegistry, studentReferenceDataRepository, _logger)
                {
                    Request = new HttpRequestMessage()
                };
                controller.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey,
                    new HttpConfiguration());
                controller.Request.Headers.CacheControl = new CacheControlHeaderValue
                {
                    NoCache = true,
                    Public = true
                };

                var dtos = await controller.GetAsync();

                CollectionAssert.AreEqual(new List<Dtos.Student.NonAcademicAttendanceEventType>(), dtos.ToList());
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task NonAcademicAttendanceEventTypesController_GetAsync_AdapterRegistry_returns_null()
            {
                AutoMapperAdapter<NonAcademicAttendanceEventType, Dtos.Student.NonAcademicAttendanceEventType> testAdapter = null;
                var adapters = new HashSet<ITypeAdapter>();
                _adapterRegistry = new AdapterRegistry(adapters, _logger);
                _adapterRegistry.AddAdapter(testAdapter);

                controller = new NonAcademicAttendanceEventTypesController(_adapterRegistry, studentReferenceDataRepository, _logger)
                {
                    Request = new HttpRequestMessage()
                };
                controller.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey,
                    new HttpConfiguration());
                controller.Request.Headers.CacheControl = new CacheControlHeaderValue
                {
                    NoCache = true,
                    Public = true
                };

                var dtos = await controller.GetAsync();

                CollectionAssert.AreEqual(new List<Dtos.Student.NonAcademicAttendanceEventType>(), dtos.ToList());
            }

            [TestMethod]
            public async Task NonAcademicAttendanceEventTypesController_GetAsync_Success()
            {
                var dtos = await controller.GetAsync();
                Assert.AreEqual(allNonAcademicAttendanceEventTypeDtos.Count, dtos.Count());
                for(int i = 0; i < allNonAcademicAttendanceEventTypeDtos.Count; i++)
                {
                    Assert.AreEqual(allNonAcademicAttendanceEventTypeDtos[i].Code, dtos.ElementAt(i).Code);
                    Assert.AreEqual(allNonAcademicAttendanceEventTypeDtos[i].Description, dtos.ElementAt(i).Description);
                }
            }
        }
    }
}
