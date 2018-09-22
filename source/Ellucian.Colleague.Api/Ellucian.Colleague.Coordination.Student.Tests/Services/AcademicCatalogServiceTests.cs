// Copyright 2015-2016 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ellucian.Colleague.Coordination.Student.Services;
using Ellucian.Colleague.Domain.Student.Entities.Requirements;
using Ellucian.Colleague.Domain.Student.Repositories;
using Ellucian.Colleague.Dtos;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.Base.Entities;
using Ellucian.Web.Adapters;
using Ellucian.Web.Security;
using Ellucian.Colleague.Domain.Repositories;

namespace Ellucian.Colleague.Coordination.Student.Tests.Services
{
    [TestClass]
    public class AcademicCatalogServiceTests
    {
        [TestClass]
        public class GetAcademicCatalog2
        {
            private Mock<ICatalogRepository> _catalogRepositoryMock;
            private ICatalogRepository _catalogRepository;
            private Mock<IConfigurationRepository> _configurationRepoMock;
            private Mock<IAdapterRegistry> _adopterRegistryMock;
            private Mock<ICurrentUserFactory> _currentUserFactoryMock;
            private Mock<IRoleRepository> _roleRepositoryMock;
            private IConfigurationRepository _configurationRepo;
            private IPersonRepository _personRepository;
            private Mock<IPersonRepository> _personRepositoryMock;
            private Mock<IStudentReferenceDataRepository> _studentReferenceDataRepositoryMock;
            private IStudentReferenceDataRepository _studentReferenceDataRepository;

            private ILogger _logger;
            private AcademicCatalogService _academicCatalogService;
            private ICollection<Catalog> _catalogCollection = new List<Catalog>();
            private ICollection<Domain.Student.Entities.AcademicProgram> _academicProgramCollection = new List<Domain.Student.Entities.AcademicProgram>();

            private const string defaultHostGuid = "7y6040a5-2a98-4614-923d-ad20101ff088";

            [TestInitialize]
            public void Initialize()
            {
                _catalogRepositoryMock = new Mock<ICatalogRepository>();
                _catalogRepository = _catalogRepositoryMock.Object;
                _configurationRepoMock = new Mock<IConfigurationRepository>();
                _configurationRepo = _configurationRepoMock.Object;
                _personRepositoryMock = new Mock<IPersonRepository>();
                _personRepository = _personRepositoryMock.Object;
                _studentReferenceDataRepositoryMock = new Mock<IStudentReferenceDataRepository>();
                _studentReferenceDataRepository = _studentReferenceDataRepositoryMock.Object;
                _adopterRegistryMock = new Mock<IAdapterRegistry>();
                _currentUserFactoryMock = new Mock<ICurrentUserFactory>();
                _roleRepositoryMock = new Mock<IRoleRepository>();

                _logger = new Mock<ILogger>().Object;

                _catalogCollection.Add(new Catalog("9C3B805D-CFE6-483B-86C3-4C20562F8C15", "2010",
                    new DateTime(2010, 1, 1)) { AcadPrograms = new List<string>() { "BA-EDUC", "MA-HIST" } });
                _catalogCollection.Add(new Catalog("73244057-D1EC-4094-A0B7-DE602533E3A6", "2011",
                    new DateTime(2011, 1, 1))
                    {
                        EndDate = new DateTime(2012, 12, 31),
                        AcadPrograms = new List<string>() { "BA-EDUC", "MA-HIST" }
                    });
                _catalogCollection.Add(new Catalog("1df164eb-8178-4321-a9f7-24f12d3991d8", "2015",
                    new DateTime(2015, 1, 1))
                    {
                        EndDate =  DateTime.Now.AddDays(3),
                        AcadPrograms = new List<string>() { "BA-EDUC" }
                    });
              
                _catalogRepositoryMock.Setup(repo => repo.GetAsync(false)).ReturnsAsync(_catalogCollection);

                var defaultsConfiguration = new DefaultsConfiguration()
                {
                    HostInstitutionCodeId = "0000043"
                };
                _configurationRepoMock.Setup(x => x.GetDefaultsConfiguration()).Returns(defaultsConfiguration);
                _personRepositoryMock.Setup(x => x.GetPersonGuidFromIdAsync(It.IsAny<string>())).ReturnsAsync(defaultHostGuid);

                _academicProgramCollection.Add(new Domain.Student.Entities.AcademicProgram("9C3B805D-CFE6-483B-86C3-4C20562F8C15", "BA-EDUC", "BA in Education")
                {
                    StartDate = new DateTime(2011, 12, 31),
                    DeptartmentCodes = new List<string>() { "HIST", "ENGL" },
                    DegreeCode = "BA",
                    AcadLevelCode = "UG",
                    MajorCodes = new List<string>() { "HIST" }
                });
                _academicProgramCollection.Add(new Domain.Student.Entities.AcademicProgram("73244057-D1EC-4094-A0B7-DE602533E3A6", "MA-HIST", "MA History")
                {
                    StartDate = new DateTime(2012, 12, 31),
                    CertificateCodes = new List<string>() { "TEACH" },
                    AcadLevelCode = "CE",
                    SpecializationCodes = new List<string>() { "CHIL" }
                });
                _academicProgramCollection.Add(new Domain.Student.Entities.AcademicProgram("1df164eb-8178-4321-a9f7-24f12d3991d8", "GR-UNDEC", "Graduate Undecided")
                {
                    StartDate = new DateTime(2012, 12, 31),
                    EndDate = new DateTime(2016, 12, 31),
                    HonorCode = "ACA",
                    AcadLevelCode = "GR",
                    MinorCodes = new List<string>() { "ENGL" }
                });
                _studentReferenceDataRepositoryMock.Setup(repo => repo.GetAcademicProgramsAsync(It.IsAny<bool>())).ReturnsAsync(_academicProgramCollection);


                _academicCatalogService = new AcademicCatalogService(_adopterRegistryMock.Object, _catalogRepository, _studentReferenceDataRepository, _personRepository,
                    _currentUserFactoryMock.Object, _configurationRepo, _roleRepositoryMock.Object, _logger);
            }

            [TestCleanup]
            public void Cleanup()
            {
                _catalogCollection = null;
                _catalogRepository = null;
                _academicCatalogService = null;
                _studentReferenceDataRepository = null;
                _configurationRepoMock = null;
                _personRepositoryMock = null;

            }

            [TestMethod]
            public async Task AcademicCatalogService__AcademicCatalogs2()
            {
                var results = await _academicCatalogService.GetAcademicCatalogs2Async();
                Assert.IsTrue(results is IEnumerable<AcademicCatalog2>);
                Assert.IsNotNull(results);
            }

            [TestMethod]
            public async Task AcademicCatalogService_AcademicCatalogs2_Count()
            {
                var results = await _academicCatalogService.GetAcademicCatalogs2Async();
                Assert.AreEqual(3, results.Count());
            }

            [TestMethod]
            public async Task AcademicCatalogService_AcademicCatalogs2_Properties()
            {
                var results = await _academicCatalogService.GetAcademicCatalogs2Async();
                var acadedmicCatalog = results.First(x => x.Code == "2015");
                Assert.IsNotNull(acadedmicCatalog.Id);
                Assert.IsNotNull(acadedmicCatalog.Code);
                Assert.IsNotNull(acadedmicCatalog.StartDate);
                Assert.IsNotNull(acadedmicCatalog.EndDate);
                Assert.IsNotNull(acadedmicCatalog.status);
            }

            [TestMethod]
            public async Task AcademicCatalogService_AcademicCatalogs2_Expected()
            {
                var expectedResults = _catalogCollection.First(c => c.Code == "2015");

                var expectedAcademicProgram = _academicProgramCollection.FirstOrDefault(x => x.Code == "BA-EDUC");
                var results = await _academicCatalogService.GetAcademicCatalogs2Async();
                var acadedmicCatalog = results.First(s => s.Code == "2015");
                Assert.AreEqual(expectedResults.Guid, acadedmicCatalog.Id);
                Assert.AreEqual(expectedResults.Code, acadedmicCatalog.Code);
                Assert.AreEqual(expectedResults.StartDate, acadedmicCatalog.StartDate);
                Assert.AreEqual(expectedResults.EndDate, acadedmicCatalog.EndDate);
                Assert.AreEqual(LifeCycleStatus.Active, acadedmicCatalog.status);
                Assert.AreEqual(defaultHostGuid, acadedmicCatalog.Institution.Id);
                Assert.AreEqual(expectedAcademicProgram.Guid, acadedmicCatalog.AcademicPrograms.ElementAtOrDefault(0).Id);
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task AcademicCatalogService_GetAcademicCatalogByGuid2_Empty()
            {
                await _academicCatalogService.GetAcademicCatalogByGuid2Async("");
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task AcademicCatalogService_GetAcademicCatalogByGuid2_Null()
            {
                await _academicCatalogService.GetAcademicCatalogByGuid2Async(null);
            }

            [TestMethod]
            public async Task AcademicCatalogService_GetAcademicCatalogByGuid2_Expected()
            {
                var expectedResults =
                    _catalogCollection.First(c => c.Guid == "1df164eb-8178-4321-a9f7-24f12d3991d8");
                var acadedmicCatalog =
                    await _academicCatalogService.GetAcademicCatalogByGuid2Async("1df164eb-8178-4321-a9f7-24f12d3991d8");
                Assert.AreEqual(expectedResults.Guid, acadedmicCatalog.Id);
                Assert.AreEqual(expectedResults.Code, acadedmicCatalog.Code);
                Assert.AreEqual(expectedResults.StartDate, acadedmicCatalog.StartDate);
                Assert.AreEqual(expectedResults.EndDate, acadedmicCatalog.EndDate);
                Assert.AreEqual(LifeCycleStatus.Active, acadedmicCatalog.status);
                Assert.AreEqual(defaultHostGuid, acadedmicCatalog.Institution.Id);
            }

            [TestMethod]
            public async Task AcademicCatalogService_GetAcademicCatalogByGuid2_Properties()
            {
                var acadedmicCatalog =
                   await _academicCatalogService.GetAcademicCatalogByGuid2Async("1df164eb-8178-4321-a9f7-24f12d3991d8");
                Assert.IsNotNull(acadedmicCatalog.Id);
                Assert.IsNotNull(acadedmicCatalog.Code);
                Assert.IsNotNull(acadedmicCatalog.StartDate);
                Assert.IsNotNull(acadedmicCatalog.EndDate);
                Assert.IsNotNull(acadedmicCatalog.status);
                Assert.IsNotNull(acadedmicCatalog.Institution);
            }
        }
    }
}