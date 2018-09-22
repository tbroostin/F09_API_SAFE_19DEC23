// Copyright 2016-2017 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ellucian.Colleague.Dtos;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.Base.Entities;
using Ellucian.Colleague.Domain.Base.Tests;
using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Data.Colleague;
using Ellucian.Colleague.Dtos.DtoProperties;
using Ellucian.Web.Adapters;
using Ellucian.Web.Security;
using Ellucian.Colleague.Domain.Repositories;

namespace Ellucian.Colleague.Coordination.Base.Tests.Services
{
    [TestClass]
    public class EducationalInstitutionUnitsServiceTests
    {
        private Mock<IReferenceDataRepository> _referenceDataRepositoryMock;
        private IReferenceDataRepository _referenceDataRepository;
        private Mock<IConfigurationRepository> _configurationRepoMock;
        private IConfigurationRepository _configurationRepo;
        private IPersonRepository _personRepository;
        private Mock<IPersonRepository> _personRepositoryMock;
        private ILogger _logger;
        private IAdapterRegistry _adapterRegistry;
        private Mock<IAdapterRegistry> _adapterRegistryMock;
        private ICurrentUserFactory _currentUserFactory;
        private Mock<ICurrentUserFactory> _currentUserFactoryMock;
        private IRoleRepository _roleRepository;
        private Mock<IRoleRepository> _roleRepositoryMock;


        private List<EducationalInstitutionUnits> _educationalInstitutionUnitsCollection;
        private List<EducationalInstitutionUnits2> _educationalInstitutionUnits2Collection;
        private EducationalInstitutionUnitsService _educationalInstitutionUnitsService;
        private List<Department> _allDepartments;
        private List<Division> _allDivisions;
        private List<School> _allSchools;

        private const string DepartmentGuid = "6d6040a5-1a98-4614-943d-ad20101ff057"; //BIOLOGY
        private const string DefaultHostGuid = "7y6040a5-2a98-4614-923d-ad20101ff088";
        private const string DivisionGuid = "50052c84-9f25-4f08-bd13-48e2a2ec4f49";
        private const string SchoolGuid = "62052c84-9f25-4f08-bd13-48e2a2ec4f49";
        
        private Department oneDepartment;
        private Division oneDivision;
        private School oneSchool;

        [TestInitialize]
        public void Initialize()
        {
            _referenceDataRepositoryMock = new Mock<IReferenceDataRepository>();
            _referenceDataRepository = _referenceDataRepositoryMock.Object;
            _logger = new Mock<ILogger>().Object;
            _configurationRepoMock = new Mock<IConfigurationRepository>();
            _configurationRepo = _configurationRepoMock.Object;
            _personRepositoryMock = new Mock<IPersonRepository>();
            _personRepository = _personRepositoryMock.Object;            
            _adapterRegistryMock = new Mock<IAdapterRegistry>();
            _adapterRegistry = _adapterRegistryMock.Object;
            _currentUserFactoryMock = new Mock<ICurrentUserFactory>();
            _currentUserFactory = _currentUserFactoryMock.Object;
            _roleRepositoryMock = new Mock<IRoleRepository>();
            _roleRepository = _roleRepositoryMock.Object;

            _allDepartments = new TestDepartmentRepository().Get().ToList();
            _allDivisions = new TestDivisionRepository().GetDivisions().ToList();
            _allSchools = new TestSchoolRepository().GetSchools().ToList();

            _educationalInstitutionUnitsCollection = new List<Dtos.EducationalInstitutionUnits>();
            _educationalInstitutionUnits2Collection = new List<Dtos.EducationalInstitutionUnits2>();

            foreach (var source in _allDepartments)
            {
                var department = new EducationalInstitutionUnits
                {
                    Id = source.Guid,
                    EducationalInstitutionUnitType = Dtos.EnumProperties.EducationalInstitutionUnitType.Department,
                    Title = source.Description,
                    Description = null
                };

                School school = null;
                var division = _allDivisions.FirstOrDefault(x => x.Code == source.Division);
                if (division != null)
                    school = _allSchools.FirstOrDefault(x => x.Code == division.SchoolCode);

                var parent = new EducationalInstitutionUnitParentDtoProperty
                {
                    Institution = new GuidObject2((school != null) ? school.Guid : string.Empty),
                    Unit = new GuidObject2((division != null) ? division.Guid : string.Empty)
                };
                department.Parents = parent;

                _educationalInstitutionUnitsCollection.Add(department);

                var department2 = new EducationalInstitutionUnits2
                {
                    Id = source.Guid,
                    EducationalInstitutionUnitType = Dtos.EnumProperties.EducationalInstitutionUnitType.Department,
                    Title = source.Description,
                    Description = null
                };

                School school2 = null;
                var division2 = _allDivisions.FirstOrDefault(x => x.Code == source.Division);
                if (division2 != null)
                    school2 = _allSchools.FirstOrDefault(x => x.Code == division.SchoolCode);

                var parent2 = new EducationalInstitutionUnitParentDtoProperty
                {
                    Institution = new GuidObject2((school != null) ? school.Guid : string.Empty),
                    Unit = new GuidObject2((division != null) ? division.Guid : string.Empty)
                };
                department.Parents = parent2;

                _educationalInstitutionUnits2Collection.Add(department2);
            }

            foreach (var source in _allSchools)
            {
                var school = new Ellucian.Colleague.Dtos.EducationalInstitutionUnits
                {
                    Id = source.Guid,
                    EducationalInstitutionUnitType = Dtos.EnumProperties.EducationalInstitutionUnitType.School,
                    Title = source.Description,
                    Description = null
                };

                _educationalInstitutionUnitsCollection.Add(school);

                var school2 = new Ellucian.Colleague.Dtos.EducationalInstitutionUnits2
                {
                    Id = source.Guid,
                    EducationalInstitutionUnitType = Dtos.EnumProperties.EducationalInstitutionUnitType.School,
                    Title = source.Description,
                    Description = null
                };

                _educationalInstitutionUnits2Collection.Add(school2);
            }

            foreach (var source in _allDivisions)
            {
                var division = new Ellucian.Colleague.Dtos.EducationalInstitutionUnits
                {
                    Id = source.Guid,
                    EducationalInstitutionUnitType = Dtos.EnumProperties.EducationalInstitutionUnitType.Division,
                    Title = source.Description,
                    Description = null
                };

               var school = _allSchools.FirstOrDefault(x => x.Code == source.SchoolCode);

                var parent = new EducationalInstitutionUnitParentDtoProperty
                {
                    Unit = new GuidObject2((school != null) ? school.Guid : string.Empty)
                };
                division.Parents = parent;
                _educationalInstitutionUnitsCollection.Add(division);

                var division2 = new Ellucian.Colleague.Dtos.EducationalInstitutionUnits2
                {
                    Id = source.Guid,
                    EducationalInstitutionUnitType = Dtos.EnumProperties.EducationalInstitutionUnitType.Division,
                    Title = source.Description,
                    Description = null
                };

                var school2 = _allSchools.FirstOrDefault(x => x.Code == source.SchoolCode);

                var parent2 = new EducationalInstitutionUnitParentDtoProperty
                {
                    Unit = new GuidObject2((school != null) ? school.Guid : string.Empty)
                };
                division.Parents = parent2;
                _educationalInstitutionUnits2Collection.Add(division2);
            }

            var defaultsConfiguration = new DefaultsConfiguration()
            {
                HostInstitutionCodeId = "0000043"
            };
            _configurationRepoMock.Setup(x => x.GetDefaultsConfiguration()).Returns(defaultsConfiguration);
            _personRepositoryMock.Setup(x => x.GetPersonGuidFromIdAsync(It.IsAny<string>()))
                .ReturnsAsync(DefaultHostGuid);


            oneDepartment = _allDepartments.First(d => d.Guid == DepartmentGuid);
            oneDivision = _allDivisions.First(d => d.Guid == DivisionGuid);
            oneSchool = _allSchools.First(d => d.Guid == SchoolGuid);

            _referenceDataRepositoryMock.Setup(x => x.GetDepartmentByGuidAsync(It.IsAny<string>())).ReturnsAsync(oneDepartment);
            _referenceDataRepositoryMock.Setup(x => x.GetDivisionByGuidAsync(It.IsAny<string>())).ReturnsAsync(oneDivision);
            _referenceDataRepositoryMock.Setup(x => x.GetSchoolByGuidAsync(It.IsAny<string>())).ReturnsAsync(oneSchool);

            _educationalInstitutionUnitsService = new EducationalInstitutionUnitsService(_referenceDataRepository,
                _personRepository, _adapterRegistry, _currentUserFactory, _roleRepository, _configurationRepo, _logger);
        }

        [TestCleanup]
        public void Cleanup()
        {
            _allDepartments = null;
            _allDivisions = null;
            _allSchools = null;

            _referenceDataRepository = null;
            _educationalInstitutionUnitsService = null;

            _logger = null;
            _personRepository = null;
            _personRepositoryMock = null;
            _referenceDataRepository = null;
            _referenceDataRepositoryMock = null;
            _configurationRepoMock = null;
            _configurationRepo = null;
        }

        #region GetEducationalInstitutionUnitsAsync

        [TestMethod]
        public async Task EducationalInstitutionUnitsService_GetEducationalInstitutionUnits()
        {
            var defaultsConfiguration = new DefaultsConfiguration()
            {
                HostInstitutionCodeId = "0000043"
            };
            _configurationRepoMock.Setup(x => x.GetDefaultsConfiguration()).Returns(defaultsConfiguration);
            _personRepositoryMock.Setup(x => x.GetPersonGuidFromIdAsync(It.IsAny<string>()))
                .ReturnsAsync(DefaultHostGuid); 
            
            _referenceDataRepositoryMock.Setup(repo => repo.GetDivisionsAsync(It.IsAny<bool>()))
               .ReturnsAsync(_allDivisions);
            _referenceDataRepositoryMock.Setup(repo => repo.GetDepartmentsAsync(It.IsAny<bool>()))
                .ReturnsAsync(_allDepartments);
            _referenceDataRepositoryMock.Setup(repo => repo.GetSchoolsAsync(It.IsAny<bool>()))
                .ReturnsAsync(_allSchools); 
            

            var actual = (await _educationalInstitutionUnitsService.GetEducationalInstitutionUnitsAsync(false))
                .FirstOrDefault(x => x.Id == DepartmentGuid) ;

            var expected = _educationalInstitutionUnitsCollection.FirstOrDefault(x => x.Id == DepartmentGuid);

            Assert.AreEqual(expected.Id, actual.Id, "Id");
            Assert.AreEqual(expected.Title, actual.Title, "Title");
            Assert.AreEqual(Dtos.EnumProperties.EducationalInstitutionUnitType.Department,
                actual.EducationalInstitutionUnitType, "EducationalInstitutionUnitType");
            Assert.AreEqual(null, actual.Description);
            Assert.AreEqual(DefaultHostGuid, actual.Parents.Institution.Id);
            Assert.AreEqual(expected.Parents.Unit.Id, actual.Parents.Unit.Id);
        }

        #endregion GetEducationalInstitutionUnitsAsync

        #region GetEducationalInstitutionUnitsByGuid

        [TestMethod]
        [ExpectedException(typeof (ArgumentNullException))]
        public async Task EducationalInstitutionUnitsService_GetEducationalInstitutionUnitsByGuid_ArgumentNullException()
        {
            await _educationalInstitutionUnitsService.GetEducationalInstitutionUnitsByGuidAsync("");
        }

        [TestMethod]
        [ExpectedException(typeof (KeyNotFoundException))]
        public async Task EducationalInstitutionUnitsService_GetEducationalInstitutionUnitsByGuid_InvalidID()
        {
            await _educationalInstitutionUnitsService.GetEducationalInstitutionUnitsByGuidAsync("invalid");
        }

        [TestMethod]
        [ExpectedException(typeof (KeyNotFoundException))]
        public async Task EducationalInstitutionUnitsService_GetEducationalInstitutionUnitsById_InvalidOperationException()
        {
            await _educationalInstitutionUnitsService.GetEducationalInstitutionUnitsByGuidAsync(DepartmentGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(KeyNotFoundException))]
        public async Task EducationalInstitutionUnitsService_GetEducationalInstitutionUnitsById_Invalid()
        {
           _referenceDataRepositoryMock.Setup(repo => repo.GetDivisionsAsync(It.IsAny<bool>()))
               .ReturnsAsync(_allDivisions);
            _referenceDataRepositoryMock.Setup(repo => repo.GetDepartmentsAsync(It.IsAny<bool>()))
                .ReturnsAsync(_allDepartments);
            _referenceDataRepositoryMock.Setup(repo => repo.GetSchoolsAsync(It.IsAny<bool>()))
                .ReturnsAsync(_allSchools);

            _referenceDataRepositoryMock.Setup(x => x.GetGuidLookupResultFromGuidAsync(SchoolGuid))
                .ReturnsAsync(new GuidLookupResult() { Entity = "INVALID" });

             await _educationalInstitutionUnitsService.GetEducationalInstitutionUnitsByGuidAsync(SchoolGuid);
           
        }

        [TestMethod]
        public async Task EducationalInstitutionUnitsService_GetEducationalInstitutionUnitsById_School()
        {
            var defaultsConfiguration = new DefaultsConfiguration()
            {
                HostInstitutionCodeId = "0000043"
            };
            _configurationRepoMock.Setup(x => x.GetDefaultsConfiguration()).Returns(defaultsConfiguration);
            _personRepositoryMock.Setup(x => x.GetPersonGuidFromIdAsync(It.IsAny<string>()))
                .ReturnsAsync(DefaultHostGuid);

            _referenceDataRepositoryMock.Setup(repo => repo.GetDivisionsAsync(It.IsAny<bool>()))
               .ReturnsAsync(_allDivisions);
            _referenceDataRepositoryMock.Setup(repo => repo.GetDepartmentsAsync(It.IsAny<bool>()))
                .ReturnsAsync(_allDepartments);
            _referenceDataRepositoryMock.Setup(repo => repo.GetSchoolsAsync(It.IsAny<bool>()))
                .ReturnsAsync(_allSchools);

            _referenceDataRepositoryMock.Setup(x => x.GetGuidLookupResultFromGuidAsync(SchoolGuid))
                .ReturnsAsync(new GuidLookupResult() { Entity = "SCHOOLS" });

            var actual =
                await _educationalInstitutionUnitsService.GetEducationalInstitutionUnitsByGuidAsync(SchoolGuid);
            var expected = _educationalInstitutionUnitsCollection.FirstOrDefault(x => x.Id == SchoolGuid);

            Assert.AreEqual(expected.Id, actual.Id, "Id");
            Assert.AreEqual(expected.Title, actual.Title, "Title");
            Assert.AreEqual(Dtos.EnumProperties.EducationalInstitutionUnitType.School,
                actual.EducationalInstitutionUnitType, "EducationalInstitutionUnitType");
            Assert.AreEqual(null, actual.Description);
            Assert.AreEqual(DefaultHostGuid, actual.Parents.Institution.Id);
            Assert.AreEqual(null, actual.Parents.Unit);
        }

        [TestMethod]
        public async Task EducationalInstitutionUnitsService_GetEducationalInstitutionUnitsById_Division()
        {
            var defaultsConfiguration = new DefaultsConfiguration()
            {
                HostInstitutionCodeId = "0000043"
            };
            _configurationRepoMock.Setup(x => x.GetDefaultsConfiguration()).Returns(defaultsConfiguration);
            _personRepositoryMock.Setup(x => x.GetPersonGuidFromIdAsync(It.IsAny<string>()))
                .ReturnsAsync(DefaultHostGuid); 
            
            _referenceDataRepositoryMock.Setup(repo => repo.GetDivisionsAsync(It.IsAny<bool>()))
               .ReturnsAsync(_allDivisions);
            _referenceDataRepositoryMock.Setup(repo => repo.GetDepartmentsAsync(It.IsAny<bool>()))
                .ReturnsAsync(_allDepartments);
            _referenceDataRepositoryMock.Setup(repo => repo.GetSchoolsAsync(It.IsAny<bool>()))
                .ReturnsAsync(_allSchools);

            _referenceDataRepositoryMock.Setup(x => x.GetGuidLookupResultFromGuidAsync(DivisionGuid))
                .ReturnsAsync(new GuidLookupResult() { Entity = "DIVISIONS" });

            var actual =
                await _educationalInstitutionUnitsService.GetEducationalInstitutionUnitsByGuidAsync(DivisionGuid);
            var expected = _educationalInstitutionUnitsCollection.FirstOrDefault(x => x.Id == DivisionGuid);

            Assert.AreEqual(expected.Id, actual.Id, "Id");
            Assert.AreEqual(expected.Title, actual.Title, "Title");
            Assert.AreEqual(Dtos.EnumProperties.EducationalInstitutionUnitType.Division,
                actual.EducationalInstitutionUnitType, "EducationalInstitutionUnitType");
            Assert.AreEqual(null, actual.Description);
            Assert.AreEqual(DefaultHostGuid, actual.Parents.Institution.Id);
            Assert.AreEqual(expected.Parents.Unit.Id, actual.Parents.Unit.Id);
        }

        [TestMethod]
        public async Task EducationalInstitutionUnitsService_GetEducationalInstitutionUnitsById_Dept()
        {
            

            _referenceDataRepositoryMock.Setup(repo => repo.GetDivisionsAsync(It.IsAny<bool>()))
               .ReturnsAsync(_allDivisions);
            _referenceDataRepositoryMock.Setup(repo => repo.GetDepartmentsAsync(It.IsAny<bool>()))
                .ReturnsAsync(_allDepartments);
            _referenceDataRepositoryMock.Setup(repo => repo.GetSchoolsAsync(It.IsAny<bool>()))
                .ReturnsAsync(_allSchools); 
            
            _referenceDataRepositoryMock.Setup(x => x.GetGuidLookupResultFromGuidAsync(DepartmentGuid))
                .ReturnsAsync(new GuidLookupResult() {Entity = "DEPTS"});

            var actual =
                await _educationalInstitutionUnitsService.GetEducationalInstitutionUnitsByGuidAsync(DepartmentGuid);
            var expected = _educationalInstitutionUnitsCollection.FirstOrDefault(x => x.Id == DepartmentGuid);

            Assert.AreEqual(expected.Id, actual.Id, "Id");
            Assert.AreEqual(expected.Title, actual.Title, "Title");
            Assert.AreEqual(Dtos.EnumProperties.EducationalInstitutionUnitType.Department,
                actual.EducationalInstitutionUnitType, "EducationalInstitutionUnitType");
            Assert.AreEqual(null, actual.Description);
            Assert.AreEqual(DefaultHostGuid, actual.Parents.Institution.Id);
            Assert.AreEqual(expected.Parents.Unit.Id, actual.Parents.Unit.Id);
        }

        [TestMethod]
        public async Task EducationalInstitutionUnitsService_GetEducationalInstitutionUnitsById_DefaultHost()
        {
            var defaultsConfiguration = new DefaultsConfiguration()
            {
                HostInstitutionCodeId = "0000043"
            };
            _configurationRepoMock.Setup(x => x.GetDefaultsConfiguration()).Returns(defaultsConfiguration);
            _personRepositoryMock.Setup(x => x.GetPersonGuidFromIdAsync(It.IsAny<string>()))
                .ReturnsAsync(DefaultHostGuid);
            
            
            foreach (var dept in _allDepartments)
           {
               dept.School = null;
               dept.Division = null;
           }

           _referenceDataRepositoryMock.Setup(repo => repo.GetDivisionsAsync(It.IsAny<bool>()))
               .ReturnsAsync(_allDivisions);
           _referenceDataRepositoryMock.Setup(repo => repo.GetDepartmentsAsync(It.IsAny<bool>()))
               .ReturnsAsync(_allDepartments);
           _referenceDataRepositoryMock.Setup(repo => repo.GetSchoolsAsync(It.IsAny<bool>()))
               .ReturnsAsync(_allSchools);
                 
            _referenceDataRepositoryMock.Setup(x => x.GetGuidLookupResultFromGuidAsync(DepartmentGuid))
                .ReturnsAsync(new GuidLookupResult() { Entity = "DEPTS" });

            var actual =
                await _educationalInstitutionUnitsService.GetEducationalInstitutionUnitsByGuidAsync(DepartmentGuid);
            var expected = _educationalInstitutionUnitsCollection.FirstOrDefault(x => x.Id == DepartmentGuid);

            Assert.AreEqual(expected.Id, actual.Id, "Id");
            Assert.AreEqual(expected.Title, actual.Title, "Title");
            Assert.AreEqual(Dtos.EnumProperties.EducationalInstitutionUnitType.Department,
                actual.EducationalInstitutionUnitType, "EducationalInstitutionUnitType");
            Assert.AreEqual(null, actual.Description);
            Assert.AreEqual(DefaultHostGuid, actual.Parents.Institution.Id);
            Assert.AreEqual(null, actual.Parents.Unit);
        }

        [TestMethod]
        [ExpectedException(typeof(KeyNotFoundException))]
        public async Task EducationalInstitutionUnitsService_DefaultHost_KeyNotFound()
        {

            var defaultsConfiguration = new DefaultsConfiguration()
            {
                HostInstitutionCodeId = "0000043"
            };
            _configurationRepoMock.Setup(x => x.GetDefaultsConfiguration()).Returns(defaultsConfiguration);
            _personRepositoryMock.Setup(x => x.GetPersonGuidFromIdAsync(It.IsAny<string>()))
                .Throws<KeyNotFoundException>();

            foreach (var dept in _allDepartments)
            {
                dept.School = null;
                dept.Division = null;
            }

            _referenceDataRepositoryMock.Setup(x => x.GetDepartmentByGuidAsync(It.IsAny<string>())).ReturnsAsync(oneDepartment);
            _referenceDataRepositoryMock.Setup(x => x.GetDivisionByGuidAsync(It.IsAny<string>())).ReturnsAsync(oneDivision);
            _referenceDataRepositoryMock.Setup(x => x.GetSchoolByGuidAsync(It.IsAny<string>())).ReturnsAsync(oneSchool);

            _referenceDataRepositoryMock.Setup(x => x.GetGuidLookupResultFromGuidAsync(DepartmentGuid))
                .ReturnsAsync(new GuidLookupResult() {Entity = "DEPTS"});

            await _educationalInstitutionUnitsService.GetEducationalInstitutionUnitsByGuidAsync(DepartmentGuid);
        }

        #endregion GetEducationalInstitutionUnitsById

        #region GetEducationalInstitutionUnitsByType


        [TestMethod]
        public async Task EducationalInstitutionUnitsService_GetEducationalInstitutionUnitsByType()
        {
           
            _referenceDataRepositoryMock.Setup(repo => repo.GetDivisionsAsync(It.IsAny<bool>()))
                .ReturnsAsync(_allDivisions);
            _referenceDataRepositoryMock.Setup(repo => repo.GetDepartmentsAsync(It.IsAny<bool>()))
                .ReturnsAsync(_allDepartments);
            _referenceDataRepositoryMock.Setup(repo => repo.GetSchoolsAsync(It.IsAny<bool>()))
                .ReturnsAsync(_allSchools);
                var educationalInstitutionUnits =
                await _educationalInstitutionUnitsService.GetEducationalInstitutionUnitsByTypeAsync("department");
            var actual = educationalInstitutionUnits.FirstOrDefault(x => x.Id == DepartmentGuid);
            var expected = _educationalInstitutionUnitsCollection.FirstOrDefault(x => x.Id == DepartmentGuid);

            Assert.AreEqual(expected.Id, actual.Id, "Id");
            Assert.AreEqual(expected.Title, actual.Title, "Title");
            Assert.AreEqual(Dtos.EnumProperties.EducationalInstitutionUnitType.Department,
                actual.EducationalInstitutionUnitType, "EducationalInstitutionUnitType");
            Assert.AreEqual(null, actual.Description);
            Assert.AreEqual(DefaultHostGuid, actual.Parents.Institution.Id);
            Assert.AreEqual(expected.Parents.Unit.Id, actual.Parents.Unit.Id);

        }

        #endregion GetEducationalInstitutionUnitsById

        #region GetEducationalInstitutionUnits2Async

        [TestMethod]
        public async Task EducationalInstitutionUnitsService_GetEducationalInstitutionUnits2()
        {
            var defaultsConfiguration = new DefaultsConfiguration()
            {
                HostInstitutionCodeId = "0000043"
            };
            _configurationRepoMock.Setup(x => x.GetDefaultsConfiguration()).Returns(defaultsConfiguration);
            _personRepositoryMock.Setup(x => x.GetPersonGuidFromIdAsync(It.IsAny<string>()))
                .ReturnsAsync(DefaultHostGuid); 
            
            _referenceDataRepositoryMock.Setup(repo => repo.GetDivisionsAsync(It.IsAny<bool>()))
               .ReturnsAsync(_allDivisions);
            _referenceDataRepositoryMock.Setup(repo => repo.GetDepartmentsAsync(It.IsAny<bool>()))
                .ReturnsAsync(_allDepartments);
            _referenceDataRepositoryMock.Setup(repo => repo.GetSchoolsAsync(It.IsAny<bool>()))
                .ReturnsAsync(_allSchools); 
            

            var actual = (await _educationalInstitutionUnitsService.GetEducationalInstitutionUnits2Async(false))
                .FirstOrDefault(x => x.Id == DepartmentGuid) ;

            var expected = _educationalInstitutionUnitsCollection.FirstOrDefault(x => x.Id == DepartmentGuid);

            Assert.AreEqual(expected.Id, actual.Id, "Id");
            Assert.AreEqual(expected.Title, actual.Title, "Title");
            Assert.AreEqual(Dtos.EnumProperties.EducationalInstitutionUnitType.Department,
                actual.EducationalInstitutionUnitType, "EducationalInstitutionUnitType");
            Assert.AreEqual(null, actual.Description);
            Assert.AreEqual(DefaultHostGuid, actual.Parents.Institution.Id);
            Assert.AreEqual(expected.Parents.Unit.Id, actual.Parents.Unit.Id);
        }

        #endregion GetEducationalInstitutionUnits2Async

        #region GetEducationalInstitutionUnitsByGuid2

        [TestMethod]
        [ExpectedException(typeof (ArgumentNullException))]
        public async Task EducationalInstitutionUnitsService_GetEducationalInstitutionUnitsByGuid2_ArgumentNullException()
        {
            await _educationalInstitutionUnitsService.GetEducationalInstitutionUnitsByGuid2Async("");
        }

        [TestMethod]
        [ExpectedException(typeof (KeyNotFoundException))]
        public async Task EducationalInstitutionUnitsService_GetEducationalInstitutionUnitsByGuid2_InvalidID()
        {
            await _educationalInstitutionUnitsService.GetEducationalInstitutionUnitsByGuid2Async("invalid");
        }

        [TestMethod]
        [ExpectedException(typeof (KeyNotFoundException))]
        public async Task EducationalInstitutionUnitsService_GetEducationalInstitutionUnitsById2_InvalidOperationException()
        {
            await _educationalInstitutionUnitsService.GetEducationalInstitutionUnitsByGuid2Async(DepartmentGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(KeyNotFoundException))]
        public async Task EducationalInstitutionUnitsService_GetEducationalInstitutionUnitsById2_Invalid()
        {
           _referenceDataRepositoryMock.Setup(repo => repo.GetDivisionsAsync(It.IsAny<bool>()))
               .ReturnsAsync(_allDivisions);
            _referenceDataRepositoryMock.Setup(repo => repo.GetDepartmentsAsync(It.IsAny<bool>()))
                .ReturnsAsync(_allDepartments);
            _referenceDataRepositoryMock.Setup(repo => repo.GetSchoolsAsync(It.IsAny<bool>()))
                .ReturnsAsync(_allSchools);

            _referenceDataRepositoryMock.Setup(x => x.GetGuidLookupResultFromGuidAsync(SchoolGuid))
                .ReturnsAsync(new GuidLookupResult() { Entity = "INVALID" });

             await _educationalInstitutionUnitsService.GetEducationalInstitutionUnitsByGuid2Async(SchoolGuid);
           
        }

        [TestMethod]
        public async Task EducationalInstitutionUnitsService_GetEducationalInstitutionUnitsById2_School()
        {
            var defaultsConfiguration = new DefaultsConfiguration()
            {
                HostInstitutionCodeId = "0000043"
            };
            _configurationRepoMock.Setup(x => x.GetDefaultsConfiguration()).Returns(defaultsConfiguration);
            _personRepositoryMock.Setup(x => x.GetPersonGuidFromIdAsync(It.IsAny<string>()))
                .ReturnsAsync(DefaultHostGuid);

            _referenceDataRepositoryMock.Setup(repo => repo.GetDivisionsAsync(It.IsAny<bool>()))
               .ReturnsAsync(_allDivisions);
            _referenceDataRepositoryMock.Setup(repo => repo.GetDepartmentsAsync(It.IsAny<bool>()))
                .ReturnsAsync(_allDepartments);
            _referenceDataRepositoryMock.Setup(repo => repo.GetSchoolsAsync(It.IsAny<bool>()))
                .ReturnsAsync(_allSchools);

            _referenceDataRepositoryMock.Setup(x => x.GetGuidLookupResultFromGuidAsync(SchoolGuid))
                .ReturnsAsync(new GuidLookupResult() { Entity = "SCHOOLS" });

            var actual =
                await _educationalInstitutionUnitsService.GetEducationalInstitutionUnitsByGuid2Async(SchoolGuid);
            var expected = _educationalInstitutionUnitsCollection.FirstOrDefault(x => x.Id == SchoolGuid);

            Assert.AreEqual(expected.Id, actual.Id, "Id");
            Assert.AreEqual(expected.Title, actual.Title, "Title");
            Assert.AreEqual(Dtos.EnumProperties.EducationalInstitutionUnitType.School,
                actual.EducationalInstitutionUnitType, "EducationalInstitutionUnitType");
            Assert.AreEqual(null, actual.Description);
            Assert.AreEqual(DefaultHostGuid, actual.Parents.Institution.Id);
            Assert.AreEqual(null, actual.Parents.Unit);
        }

        [TestMethod]
        public async Task EducationalInstitutionUnitsService_GetEducationalInstitutionUnitsById2_Division()
        {
            var defaultsConfiguration = new DefaultsConfiguration()
            {
                HostInstitutionCodeId = "0000043"
            };
            _configurationRepoMock.Setup(x => x.GetDefaultsConfiguration()).Returns(defaultsConfiguration);
            _personRepositoryMock.Setup(x => x.GetPersonGuidFromIdAsync(It.IsAny<string>()))
                .ReturnsAsync(DefaultHostGuid); 
            
            _referenceDataRepositoryMock.Setup(repo => repo.GetDivisionsAsync(It.IsAny<bool>()))
               .ReturnsAsync(_allDivisions);
            _referenceDataRepositoryMock.Setup(repo => repo.GetDepartmentsAsync(It.IsAny<bool>()))
                .ReturnsAsync(_allDepartments);
            _referenceDataRepositoryMock.Setup(repo => repo.GetSchoolsAsync(It.IsAny<bool>()))
                .ReturnsAsync(_allSchools);

            _referenceDataRepositoryMock.Setup(x => x.GetGuidLookupResultFromGuidAsync(DivisionGuid))
                .ReturnsAsync(new GuidLookupResult() { Entity = "DIVISIONS" });

            var actual =
                await _educationalInstitutionUnitsService.GetEducationalInstitutionUnitsByGuid2Async(DivisionGuid);
            var expected = _educationalInstitutionUnitsCollection.FirstOrDefault(x => x.Id == DivisionGuid);

            Assert.AreEqual(expected.Id, actual.Id, "Id");
            Assert.AreEqual(expected.Title, actual.Title, "Title");
            Assert.AreEqual(Dtos.EnumProperties.EducationalInstitutionUnitType.Division,
                actual.EducationalInstitutionUnitType, "EducationalInstitutionUnitType");
            Assert.AreEqual(null, actual.Description);
            Assert.AreEqual(DefaultHostGuid, actual.Parents.Institution.Id);
            Assert.AreEqual(expected.Parents.Unit.Id, actual.Parents.Unit.Id);
        }

        [TestMethod]
        public async Task EducationalInstitutionUnitsService_GetEducationalInstitutionUnitsByI2d_Dept()
        {
            

            _referenceDataRepositoryMock.Setup(repo => repo.GetDivisionsAsync(It.IsAny<bool>()))
               .ReturnsAsync(_allDivisions);
            _referenceDataRepositoryMock.Setup(repo => repo.GetDepartmentsAsync(It.IsAny<bool>()))
                .ReturnsAsync(_allDepartments);
            _referenceDataRepositoryMock.Setup(repo => repo.GetSchoolsAsync(It.IsAny<bool>()))
                .ReturnsAsync(_allSchools); 
            
            _referenceDataRepositoryMock.Setup(x => x.GetGuidLookupResultFromGuidAsync(DepartmentGuid))
                .ReturnsAsync(new GuidLookupResult() {Entity = "DEPTS"});

            var actual =
                await _educationalInstitutionUnitsService.GetEducationalInstitutionUnitsByGuid2Async(DepartmentGuid);
            var expected = _educationalInstitutionUnitsCollection.FirstOrDefault(x => x.Id == DepartmentGuid);

            Assert.AreEqual(expected.Id, actual.Id, "Id");
            Assert.AreEqual(expected.Title, actual.Title, "Title");
            Assert.AreEqual(Dtos.EnumProperties.EducationalInstitutionUnitType.Department,
                actual.EducationalInstitutionUnitType, "EducationalInstitutionUnitType");
            Assert.AreEqual(null, actual.Description);
            Assert.AreEqual(DefaultHostGuid, actual.Parents.Institution.Id);
            Assert.AreEqual(expected.Parents.Unit.Id, actual.Parents.Unit.Id);
        }

        [TestMethod]
        public async Task EducationalInstitutionUnitsService_GetEducationalInstitutionUnitsById2_DefaultHost()
        {
            var defaultsConfiguration = new DefaultsConfiguration()
            {
                HostInstitutionCodeId = "0000043"
            };
            _configurationRepoMock.Setup(x => x.GetDefaultsConfiguration()).Returns(defaultsConfiguration);
            _personRepositoryMock.Setup(x => x.GetPersonGuidFromIdAsync(It.IsAny<string>()))
                 .ReturnsAsync(DefaultHostGuid);


            foreach (var dept in _allDepartments)
           {
               dept.School = null;
               dept.Division = null;
           }

           _referenceDataRepositoryMock.Setup(repo => repo.GetDivisionsAsync(It.IsAny<bool>()))
               .ReturnsAsync(_allDivisions);
           _referenceDataRepositoryMock.Setup(repo => repo.GetDepartmentsAsync(It.IsAny<bool>()))
               .ReturnsAsync(_allDepartments);
           _referenceDataRepositoryMock.Setup(repo => repo.GetSchoolsAsync(It.IsAny<bool>()))
               .ReturnsAsync(_allSchools);
                 
            _referenceDataRepositoryMock.Setup(x => x.GetGuidLookupResultFromGuidAsync(DepartmentGuid))
                .ReturnsAsync(new GuidLookupResult() { Entity = "DEPTS" });

            var actual =
                await _educationalInstitutionUnitsService.GetEducationalInstitutionUnitsByGuid2Async(DepartmentGuid);
            var expected = _educationalInstitutionUnitsCollection.FirstOrDefault(x => x.Id == DepartmentGuid);

            Assert.AreEqual(expected.Id, actual.Id, "Id");
            Assert.AreEqual(expected.Title, actual.Title, "Title");
            Assert.AreEqual(Dtos.EnumProperties.EducationalInstitutionUnitType.Department,
                actual.EducationalInstitutionUnitType, "EducationalInstitutionUnitType");
            Assert.AreEqual(null, actual.Description);
            Assert.AreEqual(DefaultHostGuid, actual.Parents.Institution.Id);
            Assert.AreEqual(null, actual.Parents.Unit);
        }

        [TestMethod]
        [ExpectedException(typeof(KeyNotFoundException))]
        public async Task EducationalInstitutionUnitsService2_DefaultHost_KeyNotFound()
        {
            var defaultsConfiguration = new DefaultsConfiguration()
            {
                HostInstitutionCodeId = "0000043"
            };
            _configurationRepoMock.Setup(x => x.GetDefaultsConfiguration()).Returns(defaultsConfiguration);
            _personRepositoryMock.Setup(x => x.GetPersonGuidFromIdAsync(It.IsAny<string>()))
                .Throws<KeyNotFoundException>();


            foreach (var dept in _allDepartments)
            {
                dept.School = null;
                dept.Division = null;
            }
            
            _referenceDataRepositoryMock.Setup(repo => repo.GetDivisionsAsync(It.IsAny<bool>()))
                .ReturnsAsync(_allDivisions);
            _referenceDataRepositoryMock.Setup(repo => repo.GetDepartmentsAsync(It.IsAny<bool>()))
                .ReturnsAsync(_allDepartments);
            _referenceDataRepositoryMock.Setup(repo => repo.GetSchoolsAsync(It.IsAny<bool>()))
                .ReturnsAsync(_allSchools);

            _referenceDataRepositoryMock.Setup(x => x.GetGuidLookupResultFromGuidAsync(DepartmentGuid))
                .ReturnsAsync(new GuidLookupResult() {Entity = "DEPTS"});

            await _educationalInstitutionUnitsService.GetEducationalInstitutionUnitsByGuid2Async(DepartmentGuid);
        }

        #endregion GetEducationalInstitutionUnitsByGuid2

        #region GetEducationalInstitutionUnitsByType2

        [TestMethod]
        public async Task EducationalInstitutionUnitsService_GetEducationalInstitutionUnitsByType2()
        {
            _referenceDataRepositoryMock.Setup(repo => repo.GetDivisionsAsync(It.IsAny<bool>()))
                .ReturnsAsync(_allDivisions);
            _referenceDataRepositoryMock.Setup(repo => repo.GetDepartmentsAsync(It.IsAny<bool>()))
                .ReturnsAsync(_allDepartments);
            _referenceDataRepositoryMock.Setup(repo => repo.GetSchoolsAsync(It.IsAny<bool>()))
                .ReturnsAsync(_allSchools);
                var educationalInstitutionUnits =
                await _educationalInstitutionUnitsService.GetEducationalInstitutionUnitsByType2Async("department");
            var actual = educationalInstitutionUnits.FirstOrDefault(x => x.Id == DepartmentGuid);
            var expected = _educationalInstitutionUnitsCollection.FirstOrDefault(x => x.Id == DepartmentGuid);

            Assert.AreEqual(expected.Id, actual.Id, "Id");
            Assert.AreEqual(expected.Title, actual.Title, "Title");
            Assert.AreEqual(Dtos.EnumProperties.EducationalInstitutionUnitType.Department,
                actual.EducationalInstitutionUnitType, "EducationalInstitutionUnitType");
            Assert.AreEqual(null, actual.Description);
            Assert.AreEqual(DefaultHostGuid, actual.Parents.Institution.Id);
            Assert.AreEqual(expected.Parents.Unit.Id, actual.Parents.Unit.Id);

        }

        #endregion GetEducationalInstitutionUnitsByType2
    }


    [TestClass]
    public class EducationalInstitutionUnitsServiceTestsV12
    {
        private Mock<IReferenceDataRepository> _referenceDataRepositoryMock;
        private IReferenceDataRepository _referenceDataRepository;
        private Mock<IConfigurationRepository> _configurationRepoMock;
        private IConfigurationRepository _configurationRepo;
        private IPersonRepository _personRepository;
        private Mock<IPersonRepository> _personRepositoryMock;
        private ILogger _logger;
        private IAdapterRegistry _adapterRegistry;
        private Mock<IAdapterRegistry> _adapterRegistryMock;
        private ICurrentUserFactory _currentUserFactory;
        private Mock<ICurrentUserFactory> _currentUserFactoryMock;
        private IRoleRepository _roleRepository;
        private Mock<IRoleRepository> _roleRepositoryMock;
        
        private List<EducationalInstitutionUnits3> _educationalInstitutionUnitsCollection;
        private EducationalInstitutionUnitsService _educationalInstitutionUnitsService;
        private List<Department> _allDepartments;
        private List<Division> _allDivisions;
        private List<School> _allSchools;

        private const string DepartmentGuid = "6d6040a5-1a98-4614-943d-ad20101ff057"; 
        private const string DefaultHostGuid = "7y6040a5-2a98-4614-923d-ad20101ff088";
        private const string DivisionGuid = "50052c84-9f25-4f08-bd13-48e2a2ec4f49";
        private const string SchoolGuid = "62052c84-9f25-4f08-bd13-48e2a2ec4f49";

        private Department oneDepartment;
        private Division oneDivision;
        private School oneSchool;

        [TestInitialize]
        public void Initialize()
        {
            _referenceDataRepositoryMock = new Mock<IReferenceDataRepository>();
            _referenceDataRepository = _referenceDataRepositoryMock.Object;
            _logger = new Mock<ILogger>().Object;
            _configurationRepoMock = new Mock<IConfigurationRepository>();
            _configurationRepo = _configurationRepoMock.Object;
            _personRepositoryMock = new Mock<IPersonRepository>();
            _personRepository = _personRepositoryMock.Object;
            _adapterRegistryMock = new Mock<IAdapterRegistry>();
            _adapterRegistry = _adapterRegistryMock.Object;
            _currentUserFactoryMock = new Mock<ICurrentUserFactory>();
            _currentUserFactory = _currentUserFactoryMock.Object;
            _roleRepositoryMock = new Mock<IRoleRepository>();
            _roleRepository = _roleRepositoryMock.Object;

            _allDepartments = new TestDepartmentRepository().Get().ToList();
            _allDivisions = new TestDivisionRepository().GetDivisions().ToList();
            _allSchools = new TestSchoolRepository().GetSchools().ToList();
          
            _educationalInstitutionUnitsCollection = new List<Dtos.EducationalInstitutionUnits3>();

            foreach (var source in _allDepartments)
            {
                var department = new EducationalInstitutionUnits
                {
                    Id = source.Guid,
                    EducationalInstitutionUnitType = Dtos.EnumProperties.EducationalInstitutionUnitType.Department,
                    Title = source.Description,
                    Description = null
                };

                School school = null;
                var division = _allDivisions.FirstOrDefault(x => x.Code == source.Division);
                if (division != null)
                    school = _allSchools.FirstOrDefault(x => x.Code == division.SchoolCode);

                var parent = new EducationalInstitutionUnitParentDtoProperty
                {
                    Institution = new GuidObject2((school != null) ? school.Guid : string.Empty),
                    Unit = new GuidObject2((division != null) ? division.Guid : string.Empty)
                };
                department.Parents = parent;

               

                var departmentDto = new EducationalInstitutionUnits3
                {
                    Id = source.Guid,
                    Type = Dtos.EnumProperties.EducationalInstitutionUnitType.Department,
                    Title = source.Description,
                    Description = null
                };

                School schoolDto = null;
                var divisionDto = _allDivisions.FirstOrDefault(x => x.Code == source.Division);
                if (divisionDto != null)
                    schoolDto = _allSchools.FirstOrDefault(x => x.Code == division.SchoolCode);

                var parent2 = new EducationalInstitutionUnitParentDtoProperty
                {
                    Institution = new GuidObject2((school != null) ? school.Guid : string.Empty),
                    Unit = new GuidObject2((division != null) ? division.Guid : string.Empty)
                };
                department.Parents = parent2;

                _educationalInstitutionUnitsCollection.Add(departmentDto);
            }

            foreach (var source in _allSchools)
            {
               
                var school = new Ellucian.Colleague.Dtos.EducationalInstitutionUnits3
                {
                    Id = source.Guid,
                    Type = Dtos.EnumProperties.EducationalInstitutionUnitType.School,
                    Title = source.Description,
                    Description = null
                };

                _educationalInstitutionUnitsCollection.Add(school);
            }

            foreach (var source in _allDivisions)
            {
                

                var division = new Ellucian.Colleague.Dtos.EducationalInstitutionUnits3
                {
                    Id = source.Guid,
                    Type = Dtos.EnumProperties.EducationalInstitutionUnitType.Division,
                    Title = source.Description,
                    Description = null
                };

                var school = _allSchools.FirstOrDefault(x => x.Code == source.SchoolCode);

                var parent2 = new EducationalInstitutionUnitParentDtoProperty
                {
                    Unit = new GuidObject2((school != null) ? school.Guid : string.Empty)
                };
                division.Parents = parent2;
                _educationalInstitutionUnitsCollection.Add(division);
            }


            oneDepartment = _allDepartments.First(d => d.Guid == DepartmentGuid);
            oneDivision = _allDivisions.First(d => d.Guid == DivisionGuid);
            oneSchool = _allSchools.First(d => d.Guid == SchoolGuid);

            var defaultsConfiguration = new DefaultsConfiguration()
            {
                HostInstitutionCodeId = "0000043"
            };
            _configurationRepoMock.Setup(x => x.GetDefaultsConfiguration()).Returns(defaultsConfiguration);
            _personRepositoryMock.Setup(x => x.GetPersonGuidFromIdAsync(It.IsAny<string>()))
                .ReturnsAsync(DefaultHostGuid);
            _referenceDataRepositoryMock.Setup(x => x.GetDepartmentByGuidAsync(It.IsAny<string>())).ReturnsAsync(oneDepartment);
            _referenceDataRepositoryMock.Setup(x => x.GetDivisionByGuidAsync(It.IsAny<string>())).ReturnsAsync(oneDivision);
            _referenceDataRepositoryMock.Setup(x => x.GetSchoolByGuidAsync(It.IsAny<string>())).ReturnsAsync(oneSchool);

            _educationalInstitutionUnitsService = new EducationalInstitutionUnitsService(_referenceDataRepository,
                _personRepository, _adapterRegistry, _currentUserFactory, _roleRepository, _configurationRepo, _logger);
        }

        [TestCleanup]
        public void Cleanup()
        {
            _allDepartments = null;
            _allDivisions = null;
            _allSchools = null;

            _referenceDataRepository = null;
            _educationalInstitutionUnitsService = null;

            _logger = null;
            _personRepository = null;
            _personRepositoryMock = null;
            _referenceDataRepository = null;
            _referenceDataRepositoryMock = null;
            _configurationRepoMock = null;
            _configurationRepo = null;
        }      

        #region GetEducationalInstitutionUnits3Async

        [TestMethod]
        public async Task EducationalInstitutionUnitsService_GetEducationalInstitutionUnits3()
        {
            var defaultsConfiguration = new DefaultsConfiguration()
            {
                HostInstitutionCodeId = "0000043"
            };
            _configurationRepoMock.Setup(x => x.GetDefaultsConfiguration()).Returns(defaultsConfiguration);
            _personRepositoryMock.Setup(x => x.GetPersonGuidFromIdAsync(It.IsAny<string>()))
                .ReturnsAsync(DefaultHostGuid);

            _referenceDataRepositoryMock.Setup(repo => repo.GetDivisionsAsync(It.IsAny<bool>()))
               .ReturnsAsync(_allDivisions);
            _referenceDataRepositoryMock.Setup(repo => repo.GetDepartmentsAsync(It.IsAny<bool>()))
                .ReturnsAsync(_allDepartments);
            _referenceDataRepositoryMock.Setup(repo => repo.GetSchoolsAsync(It.IsAny<bool>()))
                .ReturnsAsync(_allSchools);


            var actual = (await _educationalInstitutionUnitsService.GetEducationalInstitutionUnits3Async(false))
                .FirstOrDefault(x => x.Id == DepartmentGuid);

            var expected = _educationalInstitutionUnitsCollection.FirstOrDefault(x => x.Id == DepartmentGuid);

            Assert.AreEqual(expected.Id, actual.Id, "Id");
            Assert.AreEqual(expected.Title, actual.Title, "Title");
            Assert.AreEqual(Dtos.EnumProperties.EducationalInstitutionUnitType.Department,
                actual.Type, "Type");
            Assert.AreEqual(null, actual.Description);
            Assert.AreEqual(DefaultHostGuid, actual.Parents.Institution.Id);
            Assert.AreEqual("ca7d1b7d-ab81-4f9f-8f3b-83f4c9031f89", actual.Parents.Unit.Id);
        }



        [TestMethod]
        public async Task EducationalInstitutionUnitsService_GetEducationalInstitutionUnits3_TypeDepartmentFilter()
        {
            var defaultsConfiguration = new DefaultsConfiguration()
            {
                HostInstitutionCodeId = "0000043"
            };
            _configurationRepoMock.Setup(x => x.GetDefaultsConfiguration()).Returns(defaultsConfiguration);
            _personRepositoryMock.Setup(x => x.GetPersonGuidFromIdAsync(It.IsAny<string>()))
                .ReturnsAsync(DefaultHostGuid);

            _referenceDataRepositoryMock.Setup(repo => repo.GetDivisionsAsync(It.IsAny<bool>()))
               .ReturnsAsync(_allDivisions);
            _referenceDataRepositoryMock.Setup(repo => repo.GetDepartmentsAsync(It.IsAny<bool>()))
                .ReturnsAsync(_allDepartments);
            _referenceDataRepositoryMock.Setup(repo => repo.GetSchoolsAsync(It.IsAny<bool>()))
                .ReturnsAsync(_allSchools);


            var actual = (await _educationalInstitutionUnitsService.GetEducationalInstitutionUnits3Async(false, Dtos.EnumProperties.EducationalInstitutionUnitType.Department))
                .FirstOrDefault(x => x.Id == DepartmentGuid);

            var expected = _educationalInstitutionUnitsCollection.FirstOrDefault(x => x.Id == DepartmentGuid);

            Assert.AreEqual(expected.Id, actual.Id, "Id");
            Assert.AreEqual(expected.Title, actual.Title, "Title");
            Assert.AreEqual(Dtos.EnumProperties.EducationalInstitutionUnitType.Department,
                actual.Type, "Type");
            Assert.AreEqual(null, actual.Description);
            Assert.AreEqual(DefaultHostGuid, actual.Parents.Institution.Id);
            Assert.AreEqual("ca7d1b7d-ab81-4f9f-8f3b-83f4c9031f89", actual.Parents.Unit.Id);
        }

        [TestMethod]
        public async Task EducationalInstitutionUnitsService_GetEducationalInstitutionUnits3_TypeSchoolFilter()
        {
            var defaultsConfiguration = new DefaultsConfiguration()
            {
                HostInstitutionCodeId = "0000043"
            };
            _configurationRepoMock.Setup(x => x.GetDefaultsConfiguration()).Returns(defaultsConfiguration);
            _personRepositoryMock.Setup(x => x.GetPersonGuidFromIdAsync(It.IsAny<string>()))
                .ReturnsAsync(DefaultHostGuid);

            _referenceDataRepositoryMock.Setup(repo => repo.GetDivisionsAsync(It.IsAny<bool>()))
               .ReturnsAsync(_allDivisions);
            _referenceDataRepositoryMock.Setup(repo => repo.GetDepartmentsAsync(It.IsAny<bool>()))
                .ReturnsAsync(_allDepartments);
            _referenceDataRepositoryMock.Setup(repo => repo.GetSchoolsAsync(It.IsAny<bool>()))
                .ReturnsAsync(_allSchools);


            var actual = (await _educationalInstitutionUnitsService.GetEducationalInstitutionUnits3Async(false, Dtos.EnumProperties.EducationalInstitutionUnitType.School))
                .FirstOrDefault(x => x.Id == SchoolGuid);

            var expected = _educationalInstitutionUnitsCollection.FirstOrDefault(x => x.Id == SchoolGuid);

            Assert.AreEqual(expected.Id, actual.Id, "Id");
            Assert.AreEqual(expected.Title, actual.Title, "Title");
            Assert.AreEqual(Dtos.EnumProperties.EducationalInstitutionUnitType.School,
                actual.Type, "Type");
            Assert.AreEqual(null, actual.Description);
            Assert.AreEqual(DefaultHostGuid, actual.Parents.Institution.Id);
 
        }


        [TestMethod]
        public async Task EducationalInstitutionUnitsService_GetEducationalInstitutionUnits3_TypeDivisionFilter()
        {
            var defaultsConfiguration = new DefaultsConfiguration()
            {
                HostInstitutionCodeId = "0000043"
            };
            _configurationRepoMock.Setup(x => x.GetDefaultsConfiguration()).Returns(defaultsConfiguration);
            _personRepositoryMock.Setup(x => x.GetPersonGuidFromIdAsync(It.IsAny<string>()))
                .ReturnsAsync(DefaultHostGuid);

            _referenceDataRepositoryMock.Setup(repo => repo.GetDivisionsAsync(It.IsAny<bool>()))
               .ReturnsAsync(_allDivisions);
            _referenceDataRepositoryMock.Setup(repo => repo.GetDepartmentsAsync(It.IsAny<bool>()))
                .ReturnsAsync(_allDepartments);
            _referenceDataRepositoryMock.Setup(repo => repo.GetSchoolsAsync(It.IsAny<bool>()))
                .ReturnsAsync(_allSchools);


            var actual = (await _educationalInstitutionUnitsService.GetEducationalInstitutionUnits3Async(false, Dtos.EnumProperties.EducationalInstitutionUnitType.Division))
                .FirstOrDefault(x => x.Id == DivisionGuid);

            var expected = _educationalInstitutionUnitsCollection.FirstOrDefault(x => x.Id == DivisionGuid);

            Assert.AreEqual(expected.Id, actual.Id, "Id");
            Assert.AreEqual(expected.Title, actual.Title, "Title");
            Assert.AreEqual(Dtos.EnumProperties.EducationalInstitutionUnitType.Division,
                actual.Type, "Type");
            Assert.AreEqual(null, actual.Description);
            Assert.AreEqual(DefaultHostGuid, actual.Parents.Institution.Id);
            Assert.AreEqual("62052c84-9f25-4f08-bd13-48e2a2ec4f49", actual.Parents.Unit.Id);

        }

        [TestMethod]
        public async Task EducationalInstitutionUnitsService_GetEducationalInstitutionUnits3_ActiveFilter()
        {
            var defaultsConfiguration = new DefaultsConfiguration()
            {
                HostInstitutionCodeId = "0000043"
            };
            _configurationRepoMock.Setup(x => x.GetDefaultsConfiguration()).Returns(defaultsConfiguration);
            _personRepositoryMock.Setup(x => x.GetPersonGuidFromIdAsync(It.IsAny<string>()))
                .ReturnsAsync(DefaultHostGuid);

            _referenceDataRepositoryMock.Setup(repo => repo.GetDivisionsAsync(It.IsAny<bool>()))
               .ReturnsAsync(_allDivisions);
            _referenceDataRepositoryMock.Setup(repo => repo.GetDepartmentsAsync(It.IsAny<bool>()))
                .ReturnsAsync(_allDepartments);
            _referenceDataRepositoryMock.Setup(repo => repo.GetSchoolsAsync(It.IsAny<bool>()))
                .ReturnsAsync(_allSchools);


            var actual = (await _educationalInstitutionUnitsService.GetEducationalInstitutionUnits3Async(false, 
                Dtos.EnumProperties.EducationalInstitutionUnitType.NotSet, Dtos.EnumProperties.Status.Active))
                .FirstOrDefault(x => x.Id == DepartmentGuid);

            var expected = _educationalInstitutionUnitsCollection.FirstOrDefault(x => x.Id == DepartmentGuid);

            Assert.AreEqual(expected.Id, actual.Id, "Id");
            Assert.AreEqual(expected.Title, actual.Title, "Title");
            Assert.AreEqual(Dtos.EnumProperties.EducationalInstitutionUnitType.Department,
                actual.Type, "Type");
            Assert.AreEqual(null, actual.Description);
            Assert.AreEqual(DefaultHostGuid, actual.Parents.Institution.Id);
            Assert.AreEqual("ca7d1b7d-ab81-4f9f-8f3b-83f4c9031f89", actual.Parents.Unit.Id);
        }

        [TestMethod]
        public async Task EducationalInstitutionUnitsService_GetEducationalInstitutionUnits3_InactiveFilter()
        {
            var defaultsConfiguration = new DefaultsConfiguration()
            {
                HostInstitutionCodeId = "0000043"
            };
            _configurationRepoMock.Setup(x => x.GetDefaultsConfiguration()).Returns(defaultsConfiguration);
            _personRepositoryMock.Setup(x => x.GetPersonGuidFromIdAsync(It.IsAny<string>()))
                .ReturnsAsync(DefaultHostGuid);

            
            _referenceDataRepositoryMock.Setup(repo => repo.GetDivisionsAsync(It.IsAny<bool>()))
               .ReturnsAsync(_allDivisions);
            _referenceDataRepositoryMock.Setup(repo => repo.GetDepartmentsAsync(It.IsAny<bool>()))
                .ReturnsAsync(_allDepartments);
            _referenceDataRepositoryMock.Setup(repo => repo.GetSchoolsAsync(It.IsAny<bool>()))
                .ReturnsAsync(_allSchools);


            var actual = await _educationalInstitutionUnitsService.GetEducationalInstitutionUnits3Async(false,
                Dtos.EnumProperties.EducationalInstitutionUnitType.NotSet, Dtos.EnumProperties.Status.Inactive);
          
            Assert.AreEqual(actual.Count(), 0);
            
        }
        #endregion GetEducationalInstitutionUnits3Async

        #region GetEducationalInstitutionUnitsByGuid3

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task EducationalInstitutionUnitsService_GetEducationalInstitutionUnitsByGuid3_ArgumentNullException()
        {
            await _educationalInstitutionUnitsService.GetEducationalInstitutionUnitsByGuid3Async("");
        }

        [TestMethod]
        [ExpectedException(typeof(KeyNotFoundException))]
        public async Task EducationalInstitutionUnitsService_GetEducationalInstitutionUnitsByGuid3_InvalidID()
        {
            await _educationalInstitutionUnitsService.GetEducationalInstitutionUnitsByGuid3Async("invalid");
        }

        [TestMethod]
        [ExpectedException(typeof(KeyNotFoundException))]
        public async Task EducationalInstitutionUnitsService_GetEducationalInstitutionUnitsById3_InvalidOperationException()
        {
            await _educationalInstitutionUnitsService.GetEducationalInstitutionUnitsByGuid3Async(DepartmentGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(KeyNotFoundException))]
        public async Task EducationalInstitutionUnitsService_GetEducationalInstitutionUnitsById3_Invalid()
        {
            _referenceDataRepositoryMock.Setup(repo => repo.GetDivisionsAsync(It.IsAny<bool>()))
                .ReturnsAsync(_allDivisions);
            _referenceDataRepositoryMock.Setup(repo => repo.GetDepartmentsAsync(It.IsAny<bool>()))
                .ReturnsAsync(_allDepartments);
            _referenceDataRepositoryMock.Setup(repo => repo.GetSchoolsAsync(It.IsAny<bool>()))
                .ReturnsAsync(_allSchools);

            _referenceDataRepositoryMock.Setup(x => x.GetGuidLookupResultFromGuidAsync(SchoolGuid))
                .ReturnsAsync(new GuidLookupResult() { Entity = "INVALID" });

            await _educationalInstitutionUnitsService.GetEducationalInstitutionUnitsByGuid3Async(SchoolGuid);

        }

        [TestMethod]
        public async Task EducationalInstitutionUnitsService_GetEducationalInstitutionUnitsById3_School()
        {
            var defaultsConfiguration = new DefaultsConfiguration()
            {
                HostInstitutionCodeId = "0000043"
            };
            _configurationRepoMock.Setup(x => x.GetDefaultsConfiguration()).Returns(defaultsConfiguration);
            _personRepositoryMock.Setup(x => x.GetPersonGuidFromIdAsync(It.IsAny<string>()))
                .ReturnsAsync(DefaultHostGuid);

            _referenceDataRepositoryMock.Setup(repo => repo.GetDivisionsAsync(It.IsAny<bool>()))
               .ReturnsAsync(_allDivisions);
            _referenceDataRepositoryMock.Setup(repo => repo.GetDepartmentsAsync(It.IsAny<bool>()))
                .ReturnsAsync(_allDepartments);
            _referenceDataRepositoryMock.Setup(repo => repo.GetSchoolsAsync(It.IsAny<bool>()))
                .ReturnsAsync(_allSchools);

            _referenceDataRepositoryMock.Setup(x => x.GetGuidLookupResultFromGuidAsync(SchoolGuid))
                .ReturnsAsync(new GuidLookupResult() { Entity = "SCHOOLS" });

            var actual =
                await _educationalInstitutionUnitsService.GetEducationalInstitutionUnitsByGuid3Async(SchoolGuid);
            var expected = _educationalInstitutionUnitsCollection.FirstOrDefault(x => x.Id == SchoolGuid);

            Assert.AreEqual(expected.Id, actual.Id, "Id");
            Assert.AreEqual(expected.Title, actual.Title, "Title");
            Assert.AreEqual(Dtos.EnumProperties.EducationalInstitutionUnitType.School,
                actual.Type, "Type");
            Assert.AreEqual(null, actual.Description);
            Assert.AreEqual(DefaultHostGuid, actual.Parents.Institution.Id);
            Assert.AreEqual(null, actual.Parents.Unit);
        }

        [TestMethod]
        public async Task EducationalInstitutionUnitsService_GetEducationalInstitutionUnitsById3_Division()
        {
            var defaultsConfiguration = new DefaultsConfiguration()
            {
                HostInstitutionCodeId = "0000043"
            };
            _configurationRepoMock.Setup(x => x.GetDefaultsConfiguration()).Returns(defaultsConfiguration);
            _personRepositoryMock.Setup(x => x.GetPersonGuidFromIdAsync(It.IsAny<string>()))
                .ReturnsAsync(DefaultHostGuid);

            _referenceDataRepositoryMock.Setup(repo => repo.GetDivisionsAsync(It.IsAny<bool>()))
               .ReturnsAsync(_allDivisions);
            _referenceDataRepositoryMock.Setup(repo => repo.GetDepartmentsAsync(It.IsAny<bool>()))
                .ReturnsAsync(_allDepartments);
            _referenceDataRepositoryMock.Setup(repo => repo.GetSchoolsAsync(It.IsAny<bool>()))
                .ReturnsAsync(_allSchools);

            _referenceDataRepositoryMock.Setup(x => x.GetGuidLookupResultFromGuidAsync(DivisionGuid))
                .ReturnsAsync(new GuidLookupResult() { Entity = "DIVISIONS" });

            var actual =
                await _educationalInstitutionUnitsService.GetEducationalInstitutionUnitsByGuid3Async(DivisionGuid);
            var expected = _educationalInstitutionUnitsCollection.FirstOrDefault(x => x.Id == DivisionGuid);

            Assert.AreEqual(expected.Id, actual.Id, "Id");
            Assert.AreEqual(expected.Title, actual.Title, "Title");
            Assert.AreEqual(Dtos.EnumProperties.EducationalInstitutionUnitType.Division,
                actual.Type, "Type");
            Assert.AreEqual(null, actual.Description);
            Assert.AreEqual(DefaultHostGuid, actual.Parents.Institution.Id);
            Assert.AreEqual(expected.Parents.Unit.Id, actual.Parents.Unit.Id);
        }

        [TestMethod]
        public async Task EducationalInstitutionUnitsService_GetEducationalInstitutionUnitsById3_Dept()
        {


            _referenceDataRepositoryMock.Setup(repo => repo.GetDivisionsAsync(It.IsAny<bool>()))
               .ReturnsAsync(_allDivisions);
            _referenceDataRepositoryMock.Setup(repo => repo.GetDepartmentsAsync(It.IsAny<bool>()))
                .ReturnsAsync(_allDepartments);
            _referenceDataRepositoryMock.Setup(repo => repo.GetSchoolsAsync(It.IsAny<bool>()))
                .ReturnsAsync(_allSchools);

            _referenceDataRepositoryMock.Setup(x => x.GetGuidLookupResultFromGuidAsync(DepartmentGuid))
                .ReturnsAsync(new GuidLookupResult() { Entity = "DEPTS" });

            var actual =
                await _educationalInstitutionUnitsService.GetEducationalInstitutionUnitsByGuid3Async(DepartmentGuid);
            var expected = _educationalInstitutionUnitsCollection.FirstOrDefault(x => x.Id == DepartmentGuid);

            Assert.AreEqual(expected.Id, actual.Id, "Id");
            Assert.AreEqual(expected.Title, actual.Title, "Title");
            Assert.AreEqual(Dtos.EnumProperties.EducationalInstitutionUnitType.Department,
                actual.Type, "Type");
            Assert.AreEqual(null, actual.Description);
            Assert.AreEqual(DefaultHostGuid, actual.Parents.Institution.Id);
            Assert.AreEqual("ca7d1b7d-ab81-4f9f-8f3b-83f4c9031f89", actual.Parents.Unit.Id);
        }

        [TestMethod]
        public async Task EducationalInstitutionUnitsService_GetEducationalInstitutionUnitsById3_DefaultHost()
        {
            var defaultsConfiguration = new DefaultsConfiguration()
            {
                HostInstitutionCodeId = "0000043"
            };
            _configurationRepoMock.Setup(x => x.GetDefaultsConfiguration()).Returns(defaultsConfiguration);
            _personRepositoryMock.Setup(x => x.GetPersonGuidFromIdAsync(It.IsAny<string>()))
                 .ReturnsAsync(DefaultHostGuid);


            foreach (var dept in _allDepartments)
            {
                dept.School = null;
                dept.Division = null;
            }

            _referenceDataRepositoryMock.Setup(repo => repo.GetDivisionsAsync(It.IsAny<bool>()))
                .ReturnsAsync(_allDivisions);
            _referenceDataRepositoryMock.Setup(repo => repo.GetDepartmentsAsync(It.IsAny<bool>()))
                .ReturnsAsync(_allDepartments);
            _referenceDataRepositoryMock.Setup(repo => repo.GetSchoolsAsync(It.IsAny<bool>()))
                .ReturnsAsync(_allSchools);

            _referenceDataRepositoryMock.Setup(x => x.GetGuidLookupResultFromGuidAsync(DepartmentGuid))
                .ReturnsAsync(new GuidLookupResult() { Entity = "DEPTS" });

            var actual =
                await _educationalInstitutionUnitsService.GetEducationalInstitutionUnitsByGuid3Async(DepartmentGuid);
            var expected = _educationalInstitutionUnitsCollection.FirstOrDefault(x => x.Id == DepartmentGuid);

            Assert.AreEqual(expected.Id, actual.Id, "Id");
            Assert.AreEqual(expected.Title, actual.Title, "Title");
            Assert.AreEqual(Dtos.EnumProperties.EducationalInstitutionUnitType.Department,
                actual.Type, "Type");
            Assert.AreEqual(null, actual.Description);
            Assert.AreEqual(DefaultHostGuid, actual.Parents.Institution.Id);
            Assert.AreEqual(null, actual.Parents.Unit);
        }

        [TestMethod]
        [ExpectedException(typeof(KeyNotFoundException))]
        public async Task EducationalInstitutionUnitsService3_DefaultHost_KeyNotFound()
        {
            var defaultsConfiguration = new DefaultsConfiguration()
            {
                HostInstitutionCodeId = "0000043"
            };
            _configurationRepoMock.Setup(x => x.GetDefaultsConfiguration()).Returns(defaultsConfiguration);
            _personRepositoryMock.Setup(x => x.GetPersonGuidFromIdAsync(It.IsAny<string>()))
                .Throws<KeyNotFoundException>();


            foreach (var dept in _allDepartments)
            {
                dept.School = null;
                dept.Division = null;
            }

            _referenceDataRepositoryMock.Setup(repo => repo.GetDivisionsAsync(It.IsAny<bool>()))
                .ReturnsAsync(_allDivisions);
            _referenceDataRepositoryMock.Setup(repo => repo.GetDepartmentsAsync(It.IsAny<bool>()))
                .ReturnsAsync(_allDepartments);
            _referenceDataRepositoryMock.Setup(repo => repo.GetSchoolsAsync(It.IsAny<bool>()))
                .ReturnsAsync(_allSchools);

            _referenceDataRepositoryMock.Setup(x => x.GetGuidLookupResultFromGuidAsync(DepartmentGuid))
                .ReturnsAsync(new GuidLookupResult() { Entity = "DEPTS" });

            await _educationalInstitutionUnitsService.GetEducationalInstitutionUnitsByGuid3Async(DepartmentGuid);
        }

        #endregion GetEducationalInstitutionUnitsByGuid3
        
    }
}
 