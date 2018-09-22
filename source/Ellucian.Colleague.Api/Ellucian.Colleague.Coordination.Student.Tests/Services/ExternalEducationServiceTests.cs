// Copyright 2016-2018 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.Base.Entities;
using Ellucian.Colleague.Domain.Base.Tests;
using Ellucian.Web.Security;
using Ellucian.Web.Adapters;
using Ellucian.Colleague.Domain.Repositories;
using ExternalEducation = Ellucian.Colleague.Domain.Base.Entities.ExternalEducation;
using Ellucian.Colleague.Domain.Student.Repositories;
using Ellucian.Colleague.Coordination.Student.Services;
using Ellucian.Colleague.Coordination.Student.Tests.UserFactories;
using Ellucian.Colleague.Domain.Student;

namespace Ellucian.Colleague.Coordination.Student.Tests.Services
{
    [TestClass]
    public class ExternalEducationServiceTests
    {
        // Sets up a Current user that is an advisor
        public abstract class CurrentUserSetup
        {
            protected Domain.Entities.Role facultyRole = new Domain.Entities.Role(105, "Faculty");

            public class FacultyUserFactory : ICurrentUserFactory
            {
                public ICurrentUser CurrentUser
                {
                    get
                    {
                        return new CurrentUser(new Claims()
                        {
                            ControlId = "123",
                            Name = "George",
                            PersonId = "0000011",
                            SecurityToken = "321",
                            SessionTimeout = 30,
                            UserName = "Faculty",
                            Roles = new List<string>() { "Faculty" },
                            SessionFixationId = "abc123"
                        });
                    }
                }
            }
        }

        [TestClass]
        public class GetExternalEducation
        {
            private Mock<IReferenceDataRepository> _referenceDataRepositoryMock;
            private IReferenceDataRepository _referenceDataRepository;
            private Mock<IAdapterRegistry> _adapterRegistryMock;
            private IAdapterRegistry _adapterRegistry;
            private Mock<IRoleRepository> _roleRepoMock;
            private IRoleRepository _roleRepo;
            private ICurrentUserFactory _currentUserFactory;
            private IExternalEducationRepository _externalEducationRepository;
            private Mock<IExternalEducationRepository> _externalEducationRepositoryMock;
            private IInstitutionsAttendRepository _institutionRepository;
            private Mock<IInstitutionsAttendRepository> _institutionRepositoryMock;
            private IPersonRepository _personRepository;
            private Mock<IPersonRepository> _personRepositoryMock;
            private IConfigurationRepository _baseConfigurationRepository;
            private Mock<IConfigurationRepository> _baseConfigurationRepositoryMock;
            private ILogger _logger;

            private ExternalEducationService _externalEducationService;
            private List<Ellucian.Colleague.Domain.Base.Entities.ExternalEducation> _allExternalEducationEntities = new List<ExternalEducation>();
            protected Domain.Entities.Role viewExternalEducation = new Domain.Entities.Role(1, "Faculty");

            private readonly DateTime _currentDate = DateTime.Now;
            private const string ExternalEducation1Guid = "a830e686-7692-4012-8da5-b1b5d44389b4";
            private const string PersonGuid1 = "ED1376E1-DF76-4EE1-AED2-ACBE8AA7EE0A";
            private const string InstitutionGuid1 = "C8B4FB7E-37BE-4F52-9CFC-750F5ADBF9C5";

            private List<OtherMajor> _allOtherMajors;
            private List<OtherMinor> _allOtherMinors;
            private List<OtherSpecial> _allSpecials;
            private List<Ellucian.Colleague.Domain.Base.Entities.OtherHonor> _allOtherHonors;
            

            [TestInitialize]
            public void Initialize()
            {
                _referenceDataRepositoryMock = new Mock<IReferenceDataRepository>();
                _referenceDataRepository = _referenceDataRepositoryMock.Object;
                _adapterRegistryMock = new Mock<IAdapterRegistry>();
                _adapterRegistry = _adapterRegistryMock.Object;
                _roleRepoMock = new Mock<IRoleRepository>();
                _roleRepo = _roleRepoMock.Object;
                _logger = new Mock<ILogger>().Object;
                _baseConfigurationRepositoryMock = new Mock<IConfigurationRepository>();
                _baseConfigurationRepository = _baseConfigurationRepositoryMock.Object;
                _externalEducationRepositoryMock = new Mock<IExternalEducationRepository>();
                _externalEducationRepository = _externalEducationRepositoryMock.Object;
                _institutionRepositoryMock = new Mock<IInstitutionsAttendRepository>();
                _institutionRepository = _institutionRepositoryMock.Object;
                _personRepositoryMock = new Mock<IPersonRepository>();
                _personRepository = _personRepositoryMock.Object;

                // Set up current user
                _currentUserFactory = new CurrentUserSetup.FacultyUserFactory();
                viewExternalEducation.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(StudentPermissionCodes.ViewExternalEducation));
                _roleRepoMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { viewExternalEducation });

                
                
                _allOtherMajors = new TestAcademicDisciplineRepository().GetOtherMajors().ToList();
                var major = _allOtherMajors.FirstOrDefault(x => x.Code.Equals("ENGL", StringComparison.OrdinalIgnoreCase));

                _allOtherMinors = new TestAcademicDisciplineRepository().GetOtherMinors().ToList();
                var minor = _allOtherMinors.FirstOrDefault(x => x.Code.Equals("HIST", StringComparison.OrdinalIgnoreCase));

                _allSpecials = new TestAcademicDisciplineRepository().GetOtherSpecials().ToList();
                var special = _allSpecials.FirstOrDefault(x => x.Code.Equals("CERT", StringComparison.OrdinalIgnoreCase));

                _allOtherHonors = new TestAcademicHonorsRepository().GetOtherHonors().ToList();
                var honor = _allOtherHonors.FirstOrDefault(x => x.Code.Equals("CL", StringComparison.OrdinalIgnoreCase));

                

                _personRepositoryMock.Setup(repo => repo.GetPersonIdFromGuidAsync(PersonGuid1)).ReturnsAsync("191");
                _personRepositoryMock.Setup(repo => repo.GetPersonGuidFromIdAsync("191")).ReturnsAsync(PersonGuid1);

                _personRepositoryMock.Setup(repo => repo.GetPersonIdFromGuidAsync(InstitutionGuid1)).ReturnsAsync("43");
                _personRepositoryMock.Setup(repo => repo.GetPersonGuidFromIdAsync("43")).ReturnsAsync(InstitutionGuid1);


                var externalEducation1 = new Ellucian.Colleague.Domain.Base.Entities.ExternalEducation(ExternalEducation1Guid)
                {
                    AcadInstitutionsId = "43",
                    AcadPersonId = "191",
                    AcadMajors = new List<string>() { "ENGL" },
                    AcadStartDate = _currentDate,
                    AcadHonors = new List<string>() { "CL" },
                    AcadSpecialization = new List<string>() { "CERT" },
                    AcadThesis = "My Amazing Thesis",
                    AcadRankNumerator = 10,
                    AcadEndDate = _currentDate,
                    AcadMinors = new List<string>() { "HIST" },
                    AcadRankPercent = 25,
                    AcadDegreeDate = _currentDate,
                    AcadGpa = 2,
                    AcadRankDenominator = 40,
                    AcadCommencementDate = _currentDate,
                    AcadComments = "My comments",
                    AcadNoYears = 10
                };

                _allExternalEducationEntities.Add(externalEducation1);


                _referenceDataRepositoryMock.Setup(repo => repo.GetOtherMajorsAsync(It.IsAny<bool>()))
               .ReturnsAsync(_allOtherMajors);

                _referenceDataRepositoryMock.Setup(repo => repo.GetOtherMinorsAsync(It.IsAny<bool>()))
                .ReturnsAsync(_allOtherMinors);

                _referenceDataRepositoryMock.Setup(repo => repo.GetOtherSpecialsAsync(It.IsAny<bool>()))
               .ReturnsAsync(_allSpecials);

                _referenceDataRepositoryMock.Setup(repo => repo.GetOtherHonorsAsync(It.IsAny<bool>()))
               .ReturnsAsync(_allOtherHonors);



                _externalEducationService = new ExternalEducationService(_adapterRegistry,
                    _referenceDataRepository, _personRepository, _externalEducationRepository,
                    _institutionRepository, _baseConfigurationRepository, _currentUserFactory, _roleRepo, _logger);
            }

            [TestCleanup]
            public void Cleanup()
            {
                _allExternalEducationEntities = null;
                _referenceDataRepository = null;
                _externalEducationService = null;
                _adapterRegistry = null;
                _adapterRegistryMock = null;
                _logger = null;
                _personRepository = null;
                _personRepositoryMock = null;
                _referenceDataRepository = null;
                _referenceDataRepositoryMock = null;
                _externalEducationRepository = null;
                _externalEducationRepositoryMock = null;
                _roleRepo = null;
                _roleRepoMock = null;
            }

            #region GetExternalEducations


            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public async Task ExternalEducationService_GetExternalEducations_InvalidPerson()
            {
                var tuple = new Tuple<IEnumerable<Ellucian.Colleague.Domain.Base.Entities.ExternalEducation>, int>(_allExternalEducationEntities, 1);

                var expected = _allExternalEducationEntities.Where(x => x.Guid == ExternalEducation1Guid);
                _externalEducationRepositoryMock.Setup(x => x.GetExternalEducationAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<string>())).ReturnsAsync(tuple);
                await _externalEducationService.GetExternalEducationsAsync(0, 10, false, "invalid");

            }

            [TestMethod]
            public async Task ExternalEducationService_GetExternalEducations()
            {
                
                var tuple = new Tuple<IEnumerable<Ellucian.Colleague.Domain.Base.Entities.ExternalEducation>, int>(_allExternalEducationEntities, 1);


                _externalEducationRepositoryMock.Setup(x => x.GetExternalEducationAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<string>())).ReturnsAsync(tuple);

                var result = await _externalEducationService.GetExternalEducationsAsync(0, 10, false, "");

                var expected = _allExternalEducationEntities.FirstOrDefault(x => x.Guid.Equals(ExternalEducation1Guid, StringComparison.OrdinalIgnoreCase));
                var actual = result.Item1.FirstOrDefault(x => x.Id.Equals(ExternalEducation1Guid, StringComparison.OrdinalIgnoreCase));

                Assert.IsNotNull(expected);
                Assert.IsNotNull(actual);
                Assert.AreEqual(expected.Guid, actual.Id, "Id");

                Assert.AreEqual(PersonGuid1, actual.Person.Id);
                Assert.AreEqual(InstitutionGuid1, actual.Institution.Id);

                var major = _allOtherMajors.FirstOrDefault(x => x.Code.Equals("ENGL", StringComparison.OrdinalIgnoreCase));
                var minor = _allOtherMinors.FirstOrDefault(x => x.Code.Equals("HIST", StringComparison.OrdinalIgnoreCase));
                var special = _allSpecials.FirstOrDefault(x => x.Code.Equals("CERT", StringComparison.OrdinalIgnoreCase));

                Assert.IsNotNull(actual.Disciplines.FirstOrDefault(x => x.Id.Equals(major.Guid, StringComparison.OrdinalIgnoreCase)));
                Assert.IsNotNull(actual.Disciplines.FirstOrDefault(x => x.Id.Equals(minor.Guid, StringComparison.OrdinalIgnoreCase)));
                Assert.IsNotNull(actual.Disciplines.FirstOrDefault(x => x.Id.Equals(special.Guid, StringComparison.OrdinalIgnoreCase)));


                var honor = _allOtherHonors.FirstOrDefault(x => x.Code.Equals("CL", StringComparison.OrdinalIgnoreCase));

                Assert.AreEqual(expected.AcadThesis, actual.ThesisTitle);
            }

            #endregion GetExternalEducationAsync     
        }
    }

}