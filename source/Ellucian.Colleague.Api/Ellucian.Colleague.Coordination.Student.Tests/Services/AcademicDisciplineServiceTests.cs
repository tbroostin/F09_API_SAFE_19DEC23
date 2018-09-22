// Copyright 2015-2018 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Linq;
using Ellucian.Colleague.Coordination.Student.Services;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.Base.Tests;
using Ellucian.Colleague.Domain.Student.Repositories;
using Ellucian.Web.Adapters;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Web.Security;
using Ellucian.Colleague.Domain.Base;
using Ellucian.Colleague.Domain.Base.Entities;
using System.Threading.Tasks;
using Ellucian.Colleague.Domain.Student.Tests;
using Ellucian.Data.Colleague;
using Ellucian.Colleague.Coordination.Student.Adapters;

namespace Ellucian.Colleague.Coordination.Student.Tests.Services
{
    [TestClass]
    public class AcademicDisciplineServiceTests 
    {
        // sets up a current user
        public abstract class CurrentUserSetup
        {
            protected Domain.Entities.Role PersonRole = new Domain.Entities.Role(105, "Faculty");

            public class PersonUserFactory : ICurrentUserFactory
            {
                public ICurrentUser CurrentUser
                {
                    get
                    {
                        return new CurrentUser(new Claims()
                        {
                            ControlId = "123",
                            Name = "George",
                            PersonId = "0000015",
                            SecurityToken = "321",
                            SessionTimeout = 30,
                            UserName = "Faculty",
                            Roles = new List<string>() { "Faculty" },
                            SessionFixationId = "abc123",
                        });
                    }
                }
            }
        }

        [TestClass]
        public class AcademicDisciplineServiceGet : CurrentUserSetup
        {
            private Mock<IReferenceDataRepository> _refRepoMock;
            private IReferenceDataRepository _refRepo;
            private Mock<IStudentReferenceDataRepository> _stuRefRepoMock;
            private IStudentReferenceDataRepository _stuRefRepo;
            private IStudentRepository _stuRepo;
            private Mock<IAdapterRegistry> _adapterRegistryMock;
            private IAdapterRegistry _adapterRegistry;
            private ILogger _logger;
            private Mock<IRoleRepository> _roleRepoMock;
            private Mock<IStudentRepository> _stuRepoMock;
            private IRoleRepository _roleRepo;
            private ICurrentUserFactory _currentUserFactory;
            private IEnumerable<Domain.Base.Entities.OtherMajor> _allMajors;
            private IEnumerable<Domain.Base.Entities.OtherMinor> _allMinors;
            private IEnumerable<Domain.Base.Entities.OtherSpecial> _allSpecials;

            private IConfigurationRepository baseConfigurationRepository;
            private Mock<IConfigurationRepository> baseConfigurationRepositoryMock;

            private IEnumerable<Domain.Base.Entities.AcademicDiscipline> _allAcadDisciplines;

            private IEnumerable<Domain.Student.Entities.Major> _allActualMajors;
            private IEnumerable<Domain.Student.Entities.Minor> _allActualMinors;

            private AcademicDisciplineService _academicDisciplineService;
            private const string OtherMajorGuid = "9ae3a175-1dfd-4937-b97b-3c9ad596e023";
            private const string OtherMinorGuid = "dd0c42ca-c61d-4ca6-8d21-96ab5be35623";
            private const string OtherSpecialGuid = "72b7737b-27db-4a06-944b-97d00c29b3db";
            private const string AcademicDisciplineGuid = "31d8aa32-dbe6-83j7-a1c4-2cad39e232e4";
            private Domain.Entities.Permission _permissionViewAnyPerson;

            [TestInitialize]
            public void Initialize() {
               _refRepoMock = new Mock<IReferenceDataRepository>();
                _refRepo = _refRepoMock.Object;
                _adapterRegistryMock = new Mock<IAdapterRegistry>();
                _adapterRegistry = _adapterRegistryMock.Object;
                _roleRepoMock = new Mock<IRoleRepository>();
                _roleRepo = _roleRepoMock.Object;
                _stuRefRepoMock = new Mock<IStudentReferenceDataRepository>();
                _stuRefRepo = _stuRefRepoMock.Object;
                _stuRepoMock = new Mock<IStudentRepository>();
                _stuRepo = _stuRepoMock.Object;
                _logger = new Mock<ILogger>().Object;

                baseConfigurationRepositoryMock = new Mock<IConfigurationRepository>();
                baseConfigurationRepository = baseConfigurationRepositoryMock.Object;

                _allMajors = new TestAcademicDisciplineRepository().GetOtherMajors();
                _allMinors = new TestAcademicDisciplineRepository().GetOtherMinors();
                _allSpecials = new TestAcademicDisciplineRepository().GetOtherSpecials();
                _allAcadDisciplines = new TestAcademicDisciplineRepository().GetAcademicDisciplines();
                
                
                TestStudentReferenceDataRepository tsrdr = new TestStudentReferenceDataRepository();
                _allActualMajors = tsrdr.GetMajorsAsync().Result as List<Domain.Student.Entities.Major>;
                _allActualMinors = tsrdr.GetMinorsAsync().Result as List<Domain.Student.Entities.Minor>;
                



                _adapterRegistryMock = new Mock<IAdapterRegistry>();
                _adapterRegistry = _adapterRegistryMock.Object;


                // Set up AcademicDiscipline2 to AcademicDiscipline3 adapter
                _adapterRegistryMock.Setup(registry => registry.GetAdapter<Dtos.AcademicDiscipline2, Dtos.AcademicDiscipline3>()).Returns(new AcademicDiscipline2DtoToAcademicDiscipline3DtoAdapter(_adapterRegistry,_logger));


                // Set up current user
                _currentUserFactory = new CurrentUserSetup.PersonUserFactory();

                // Mock permissions
                _permissionViewAnyPerson = new Ellucian.Colleague.Domain.Entities.Permission(BasePermissionCodes.ViewAnyPerson);
                PersonRole.AddPermission(_permissionViewAnyPerson);
                _roleRepoMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { PersonRole });

                _refRepoMock.Setup(repo => repo.GetOtherMajorsAsync(It.IsAny<bool>())).ReturnsAsync(_allMajors);
                _refRepoMock.Setup(repo => repo.GetOtherMinorsAsync(It.IsAny<bool>())).ReturnsAsync(_allMinors);
                _refRepoMock.Setup(repo => repo.GetOtherSpecialsAsync(It.IsAny<bool>())).ReturnsAsync(_allSpecials);

                _refRepoMock.Setup(repo => repo.GetAcademicDisciplinesAsync(It.IsAny<bool>())).ReturnsAsync(_allAcadDisciplines);

                _refRepoMock.Setup(repo => repo.GetAcademicDisciplinesMajorAsync(It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(_allAcadDisciplines.FirstOrDefault());
                _refRepoMock.Setup(repo => repo.GetAcademicDisciplinesMinorAsync(It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(_allAcadDisciplines.FirstOrDefault());
                _refRepoMock.Setup(repo => repo.GetAcademicDisciplinesSpecialAsync(It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(_allAcadDisciplines.FirstOrDefault());

                _stuRefRepoMock.Setup(stu => stu.GetMajorsAsync(It.IsAny<bool>())).ReturnsAsync(_allActualMajors);
                _stuRefRepoMock.Setup(stu => stu.GetMinorsAsync(It.IsAny<bool>())).ReturnsAsync(_allActualMinors);
                _stuRefRepoMock.Setup(stu => stu.GetHostCountryAsync()).ReturnsAsync("USA");

                _academicDisciplineService = new AcademicDisciplineService(_adapterRegistry, _refRepo, _stuRefRepo, _currentUserFactory, _roleRepo, _logger, _stuRepo, baseConfigurationRepository);
            }

            [TestCleanup]
            public void Cleanup() {
                _refRepo = null;       
                _allMajors = null;
                _adapterRegistry = null;
                _roleRepo = null;
                _logger = null;
                _academicDisciplineService = null;
            }


            [TestMethod]
            public async Task GetAcademicDisciplineByGuid_ValidMajorGuidAsync()
            {
                AcademicDiscipline thisAcadDiscipline = _allAcadDisciplines.FirstOrDefault(m => m.Guid == OtherMajorGuid);
                _refRepoMock.Setup(repo => repo.GetAcademicDisciplinesMajorAsync(It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(_allAcadDisciplines.FirstOrDefault(m => m.Guid == OtherMajorGuid));
                _refRepoMock.Setup(repo => repo.GetRecordInfoFromGuidReferenceDataRepoAsync(It.IsAny<string>())).ReturnsAsync(new GuidLookupResult() { PrimaryKey = thisAcadDiscipline.Code, Entity = "OTHER.MAJORS" });
                Dtos.AcademicDiscipline academicDiscipline = await _academicDisciplineService.GetAcademicDisciplineByGuidAsync(OtherMajorGuid);

                Assert.IsNotNull(thisAcadDiscipline);
                Assert.AreEqual(thisAcadDiscipline.Guid, academicDiscipline.Id);
                Assert.AreEqual(thisAcadDiscipline.Code, academicDiscipline.Abbreviation);
                Assert.AreEqual(thisAcadDiscipline.Description, academicDiscipline.Title);
                Assert.AreEqual("Major", academicDiscipline.Type.ToString());
            }
            [TestMethod]
            public async Task GetAcademicDiscipline2ByGuid_ValidMajorGuidAsync()
            {
                AcademicDiscipline thisAcadDiscipline = _allAcadDisciplines.FirstOrDefault(m => m.Guid == OtherMajorGuid);
                _refRepoMock.Setup(repo => repo.GetAcademicDisciplinesMajorAsync(It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(_allAcadDisciplines.FirstOrDefault(m => m.Guid == OtherMajorGuid));
                _refRepoMock.Setup(repo => repo.GetRecordInfoFromGuidReferenceDataRepoAsync(It.IsAny<string>())).ReturnsAsync(new GuidLookupResult() { PrimaryKey = thisAcadDiscipline.Code, Entity = "OTHER.MAJORS" });
                Dtos.AcademicDiscipline2 academicDiscipline = await _academicDisciplineService.GetAcademicDiscipline2ByGuidAsync(OtherMajorGuid);

                Assert.IsNotNull(thisAcadDiscipline);
                Assert.AreEqual(thisAcadDiscipline.Guid, academicDiscipline.Id);
                Assert.AreEqual(thisAcadDiscipline.Code, academicDiscipline.Abbreviation);
                Assert.AreEqual(thisAcadDiscipline.Description, academicDiscipline.Title);
                Assert.AreEqual("Major", academicDiscipline.Type.ToString());
            }

            [TestMethod]
            public async Task GetAcademicDiscipline2ByGuid_Canada()
            {
                _stuRefRepoMock.Setup(stu => stu.GetHostCountryAsync()).ReturnsAsync("CANADA");
                AcademicDiscipline thisAcadDiscipline = _allAcadDisciplines.FirstOrDefault(m => m.Guid == OtherMajorGuid);
                _refRepoMock.Setup(repo => repo.GetAcademicDisciplinesMajorAsync(It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(_allAcadDisciplines.FirstOrDefault(m => m.Guid == OtherMajorGuid));
                _refRepoMock.Setup(repo => repo.GetRecordInfoFromGuidReferenceDataRepoAsync(It.IsAny<string>())).ReturnsAsync(new GuidLookupResult() { PrimaryKey = thisAcadDiscipline.Code, Entity = "OTHER.MAJORS" });
                var actualMajor = _allActualMajors.FirstOrDefault(am => am.Code == thisAcadDiscipline.Code);
                Dtos.AcademicDiscipline2 academicDiscipline = await _academicDisciplineService.GetAcademicDiscipline2ByGuidAsync(OtherMajorGuid);

                Assert.IsNotNull(thisAcadDiscipline);
                Assert.AreEqual(thisAcadDiscipline.Guid, academicDiscipline.Id);
                Assert.AreEqual(thisAcadDiscipline.Code, academicDiscipline.Abbreviation);
                Assert.AreEqual(thisAcadDiscipline.Description, academicDiscipline.Title);
                Assert.AreEqual("Major", academicDiscipline.Type.ToString());
                Assert.AreEqual(actualMajor.FederalCourseClassification, academicDiscipline.Reporting.First().Value.Value);
                Assert.AreEqual(Dtos.EnumProperties.IsoCode.CAN, academicDiscipline.Reporting.First().Value.Code);
            }


            [TestMethod]
            public async Task GetAcademicDisciplineByGuid_ValidMinorGuidAsync()
            {
                AcademicDiscipline thisAcadDiscipline = _allAcadDisciplines.FirstOrDefault(m => m.Guid == OtherMinorGuid);
                _refRepoMock.Setup(repo => repo.GetAcademicDisciplinesMinorAsync(It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(_allAcadDisciplines.FirstOrDefault(m => m.Guid == OtherMinorGuid));
                _refRepoMock.Setup(repo => repo.GetRecordInfoFromGuidReferenceDataRepoAsync(It.IsAny<string>())).ReturnsAsync(new GuidLookupResult() { PrimaryKey = thisAcadDiscipline.Code, Entity = "OTHER.MINORS" });
                var academicDiscipline = await _academicDisciplineService.GetAcademicDisciplineByGuidAsync(OtherMinorGuid);

                Assert.IsNotNull(thisAcadDiscipline);
                Assert.AreEqual(thisAcadDiscipline.Guid, academicDiscipline.Id);
                Assert.AreEqual(thisAcadDiscipline.Code, academicDiscipline.Abbreviation);
                Assert.AreEqual(thisAcadDiscipline.Description, academicDiscipline.Title);
                Assert.AreEqual("Minor", academicDiscipline.Type.ToString());
            }

            [TestMethod]
            public async Task GetAcademicDiscipline2ByGuid_ValidMinorGuidAsync()
            {
                AcademicDiscipline thisAcadDiscipline = _allAcadDisciplines.FirstOrDefault(m => m.Guid == OtherMinorGuid);
                _refRepoMock.Setup(repo => repo.GetAcademicDisciplinesMinorAsync(It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(_allAcadDisciplines.FirstOrDefault(m => m.Guid == OtherMinorGuid));
                _refRepoMock.Setup(repo => repo.GetRecordInfoFromGuidReferenceDataRepoAsync(It.IsAny<string>())).ReturnsAsync(new GuidLookupResult() { PrimaryKey = thisAcadDiscipline.Code, Entity = "OTHER.MINORS" });
                var actualMinor = _allActualMinors.FirstOrDefault(am => am.Code == thisAcadDiscipline.Code);
                _refRepoMock.Setup(repo => repo.GetOtherMinorsAsync(true)).ReturnsAsync(_allMinors.Where(m => m.Guid == OtherMinorGuid));
                var academicDiscipline = await _academicDisciplineService.GetAcademicDiscipline2ByGuidAsync(OtherMinorGuid);

                Assert.IsNotNull(thisAcadDiscipline);
                Assert.AreEqual(thisAcadDiscipline.Guid, academicDiscipline.Id);
                Assert.AreEqual(thisAcadDiscipline.Code, academicDiscipline.Abbreviation);
                Assert.AreEqual(thisAcadDiscipline.Description, academicDiscipline.Title);
                Assert.AreEqual("Minor", academicDiscipline.Type.ToString());
                Assert.AreEqual(actualMinor.FederalCourseClassification, academicDiscipline.Reporting.First().Value.Value);
            }
            [TestMethod]
            public async Task GetAcademicDisciplineByGuid_ValidConcentrationGuidAsync()
            {
                AcademicDiscipline thisAcadDiscipline = _allAcadDisciplines.FirstOrDefault(m => m.Guid == OtherSpecialGuid);
                _refRepoMock.Setup(repo => repo.GetAcademicDisciplinesSpecialAsync(It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(_allAcadDisciplines.FirstOrDefault(m => m.Guid == OtherSpecialGuid));
                _refRepoMock.Setup(repo => repo.GetRecordInfoFromGuidReferenceDataRepoAsync(It.IsAny<string>())).ReturnsAsync(new GuidLookupResult() { PrimaryKey = thisAcadDiscipline.Code, Entity = "OTHER.SPECIALS" });
                var academicDiscipline = await _academicDisciplineService.GetAcademicDisciplineByGuidAsync(OtherSpecialGuid);
                Assert.IsNotNull(thisAcadDiscipline);
                Assert.AreEqual(thisAcadDiscipline.Guid, academicDiscipline.Id);
                Assert.AreEqual(thisAcadDiscipline.Code, academicDiscipline.Abbreviation);
                Assert.AreEqual(thisAcadDiscipline.Description, academicDiscipline.Title);
                Assert.AreEqual("Concentration", academicDiscipline.Type.ToString());
            }

            [TestMethod]
            public async Task GetAcademicDisciplines_CountAcademicDisciplinesAsync()
            {
                _refRepoMock.Setup(repo => repo.GetOtherMajorsAsync(false)).ReturnsAsync(_allMajors);
                _refRepoMock.Setup(repo => repo.GetOtherMinorsAsync(false)).ReturnsAsync(_allMinors);
                _refRepoMock.Setup(repo => repo.GetOtherSpecialsAsync(false)).ReturnsAsync(_allSpecials);
                IEnumerable<Dtos.AcademicDiscipline2> academicDiscipline = await _academicDisciplineService.GetAcademicDisciplines2Async();
                Assert.AreEqual(7, academicDiscipline.Count());
            }

            [TestMethod]
            public async Task GetAcademicDisciplines_CompareAcademicDisciplinesMajorsAsync()
            {
                var academicDiscipline = await _academicDisciplineService.GetAcademicDisciplines2Async();

                var expected = _allAcadDisciplines.FirstOrDefault(x => x.Guid.Equals(OtherMajorGuid, StringComparison.OrdinalIgnoreCase));
                var actual = academicDiscipline.FirstOrDefault(x => x.Id.Equals(OtherMajorGuid, StringComparison.OrdinalIgnoreCase));

                Assert.IsNotNull(expected);
                Assert.IsNotNull(actual);

                Assert.AreEqual(expected.Guid, actual.Id);
                Assert.AreEqual(expected.Code, actual.Abbreviation);
                Assert.AreEqual(expected.Description, actual.Title);
                Assert.AreEqual("Major", actual.Type.ToString());
            }

            [TestMethod]
            public async Task GetAcademicDisciplines_CompareAcademicDisciplinesMinorsAsync()
            {
                var academicDiscipline = await _academicDisciplineService.GetAcademicDisciplines2Async();

                var expected = _allAcadDisciplines.FirstOrDefault(x => x.Guid.Equals(OtherMinorGuid, StringComparison.OrdinalIgnoreCase));
                var actual = academicDiscipline.FirstOrDefault(x => x.Id.Equals(OtherMinorGuid, StringComparison.OrdinalIgnoreCase));

                Assert.IsNotNull(expected);
                Assert.IsNotNull(actual);

                Assert.AreEqual(expected.Guid, actual.Id);
                Assert.AreEqual(expected.Code, actual.Abbreviation);
                Assert.AreEqual(expected.Description, actual.Title);
                Assert.AreEqual("Minor", actual.Type.ToString());
            }

            [TestMethod]
            public async Task GetAcademicDisciplines_CompareAcademicDisciplinesSpecialsAsync()
            {
                var academicDiscipline = await _academicDisciplineService.GetAcademicDisciplines2Async();

                var expected = _allAcadDisciplines.FirstOrDefault(x => x.Guid.Equals(OtherSpecialGuid, StringComparison.OrdinalIgnoreCase));
                var actual = academicDiscipline.FirstOrDefault(x => x.Id.Equals(OtherSpecialGuid, StringComparison.OrdinalIgnoreCase));

                Assert.IsNotNull(expected);
                Assert.IsNotNull(actual);

                Assert.AreEqual(expected.Guid, actual.Id);
                Assert.AreEqual(expected.Code, actual.Abbreviation);
                Assert.AreEqual(expected.Description, actual.Title);
                Assert.AreEqual("Concentration", actual.Type.ToString());

            }
            
            [TestMethod]
            public async Task GetAcademicDisciplines_GetAllByTypeMajorAsync()
            {
                var actual = await _academicDisciplineService.GetAcademicDisciplines3Async(Dtos.EnumProperties.MajorStatus.NotSet, "major", false);
                var expected = _allMajors;
                
                Assert.IsNotNull(expected);
                Assert.IsNotNull(actual);

                Assert.AreEqual(expected.Count(), actual.Count(), "Expected: '" + string.Join(",", expected.Select(am => am.Code)) + "' - Actual: '" + String.Join(",", actual.Select(ac => ac.Abbreviation)) + "'");
            }

            [TestMethod]
            public async Task GetAcademicDisciplines_GetAllByTypeMinorAsync()
            {
                var actual = await _academicDisciplineService.GetAcademicDisciplines3Async(Dtos.EnumProperties.MajorStatus.NotSet, "minor", false);
                var expected = _allMinors;

                Assert.IsNotNull(expected);
                Assert.IsNotNull(actual);
                Assert.AreEqual(expected.Count(), actual.Count(), "Expected: '" + string.Join(",", expected.Select(am => am.Code)) + "' - Actual: '" + String.Join(",", actual.Select(ac => ac.Abbreviation)) + "'");
            }

            [TestMethod]
            public async Task GetAcademicDisciplines_GetAllByTypeConcentrationAsync()
            {
                var actual = await _academicDisciplineService.GetAcademicDisciplines3Async(Dtos.EnumProperties.MajorStatus.NotSet, "concentration", false);
                var expected = _allSpecials;

                Assert.IsNotNull(expected);
                Assert.IsNotNull(actual);
                Assert.AreEqual(expected.Count(), actual.Count(), "Expected: '" + string.Join(",", expected.Select(am => am.Code)) + "' - Actual: '" + String.Join(",", actual.Select(ac => ac.Abbreviation)) + "'");
            }
            [TestMethod]
            public async Task GetAcademicDisciplines_GetAllByMajorStatusActiveAsync()
            {
                var actual = await _academicDisciplineService.GetAcademicDisciplines3Async(Dtos.EnumProperties.MajorStatus.Active, "", false);
                var expected = _allActualMajors.Where(acm => acm.ActiveFlag = true);

                Assert.IsNotNull(expected);
                Assert.IsNotNull(actual);
                Assert.AreEqual(expected.Count(), actual.Count(), "Expected: '" + string.Join(",", expected.Select(am => am.Code)) + "' - Actual: '" + String.Join(",", actual.Select(ac => ac.Abbreviation)) + "'");
            }
            [TestMethod]
            public async Task GetAcademicDisciplines_GetAllByMajorStatusInactiveAsync()
            {
                var actual = await _academicDisciplineService.GetAcademicDisciplines3Async(Dtos.EnumProperties.MajorStatus.Inactive, "", false);
                var expected = _allActualMajors.Where(acm => acm.ActiveFlag = false);

                Assert.IsNotNull(expected);
                Assert.IsNotNull(actual);
                Assert.AreEqual(expected.Count(), actual.Count(), "Expected: '" + string.Join(",", expected.Select(am => am.Code)) + "' - Actual: '" + String.Join(",", actual.Select(ac => ac.Abbreviation)) + "'");
            }
        }

        

        
    }
}