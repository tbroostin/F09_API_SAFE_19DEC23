//Copyright 2019 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Colleague.Domain.Base;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Colleague.Dtos;
using Ellucian.Colleague.Dtos.DtoProperties;
using Ellucian.Web.Adapters;
using Ellucian.Web.Security;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ellucian.Web.Http.Exceptions;

namespace Ellucian.Colleague.Coordination.Base.Tests.Services
{
    [TestClass]
    public class PersonExternalEducationCredentialsServiceTests
    {
        public abstract class CurrentUserSetup
        {
            protected Ellucian.Colleague.Domain.Entities.Role viewExternalCredentialsRole = new Ellucian.Colleague.Domain.Entities.Role(1, BasePermissionCodes.ViewPersonExternalEducationCredentials);
            protected Ellucian.Colleague.Domain.Entities.Role updateExternalCredentialsRole = new Ellucian.Colleague.Domain.Entities.Role(1, BasePermissionCodes.UpdatePersonExternalEducationCredentials);

            public class StudentUserFactory : ICurrentUserFactory
            {
                public ICurrentUser CurrentUser
                {
                    get
                    {
                        return new CurrentUser(new Claims()
                        {
                            ControlId = "123",
                            Name = "Samwise",
                            PersonId = "STU1",
                            SecurityToken = "321",
                            SessionTimeout = 30,
                            UserName = "Samwise",
                            Roles = new List<string>() { },
                            SessionFixationId = "abc123"
                        });
                    }
                }
            }

            // Represents a third party system like ILP
            public class ThirdPartyUserFactory : ICurrentUserFactory
            {
                public ICurrentUser CurrentUser
                {
                    get
                    {
                        return new CurrentUser(new Claims()
                        {
                            ControlId = "123",
                            Name = "ILP",
                            PersonId = "ILP",
                            SecurityToken = "321",
                            SessionTimeout = 30,
                            UserName = "ILP",
                            Roles = new List<string>() { BasePermissionCodes.ViewPersonExternalEducationCredentials, BasePermissionCodes.UpdatePersonExternalEducationCredentials },
                            SessionFixationId = "abc123"
                        });
                    }
                }
            }
        }

        [TestClass]
        public class PersonExternalEducationCredentials_Tests : CurrentUserSetup
        {
            private const string personExternalEducationCredentialsGuid = "7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc";
            private const string personExternalEducationCredentialsCode = "1";
            private ICollection<Domain.Base.Entities.ExternalEducation> _personExternalEducationCredentialsCollection;
            private PersonExternalEducationCredentialsService _personExternalEducationCredentialsService;
            private IEnumerable<Domain.Base.Entities.OtherHonor> _academicHonors;
            private IEnumerable<Domain.Base.Entities.OtherCcd> _otherCertifications;
            private IEnumerable<Domain.Base.Entities.CcdType> _ccdTypes;
            private IEnumerable<Domain.Base.Entities.AcademicDiscipline> _academicDisciplines;
            private IEnumerable<Domain.Base.Entities.AcadCredential> _academicCredentials;
            private readonly int offset = 0;
            private readonly int limit = 100;

            private Mock<IReferenceDataRepository> _referenceRepositoryMock;
            private Mock<ILogger> _loggerMock;
            private Mock<IAdapterRegistry> _adapterRegistryMock;
            private ICurrentUserFactory currentUserFactory;
            private Mock<IRoleRepository> _roleRepositoryMock;
            private Mock<IConfigurationRepository> _configurationRepoMock;
            private Mock<IPersonExternalEducationCredentialsRepository> _personExternalEducationCredentialsRepoMock;
            private Mock<IInstitutionRepository> _institutionsRepoMock;


            [TestInitialize]
            public void Initialize()
            {
                _personExternalEducationCredentialsRepoMock = new Mock<IPersonExternalEducationCredentialsRepository>();
                _institutionsRepoMock = new Mock<IInstitutionRepository>();
                _referenceRepositoryMock = new Mock<IReferenceDataRepository>();
                _adapterRegistryMock = new Mock<IAdapterRegistry>();
                _loggerMock = new Mock<ILogger>();
                currentUserFactory = new CurrentUserSetup.ThirdPartyUserFactory();
                _roleRepositoryMock = new Mock<IRoleRepository>();
                _configurationRepoMock = new Mock<IConfigurationRepository>();


                viewExternalCredentialsRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(BasePermissionCodes.ViewPersonExternalEducationCredentials));
                updateExternalCredentialsRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(BasePermissionCodes.UpdatePersonExternalEducationCredentials));

                _roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { viewExternalCredentialsRole, updateExternalCredentialsRole });

                _personExternalEducationCredentialsCollection = new List<Domain.Base.Entities.ExternalEducation>()
                {
                     new Domain.Base.Entities.ExternalEducation("7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc")
                    {
                        AcadPersonId = "1",
                        AcadInstitutionsId = "1",
                        Id = "1",
                        AcadAcadProgram = "MATH.BA",
                        AcadDegree = "BA",
                        AcadCcd = new List<string>() { "CERT", "EMR"},
                        AcadCcdDate = new List<DateTime?>() { new DateTime(2019, 11, 20), new DateTime(2018, 11, 15) },
                        AcadDegreeDate = new DateTime(2019, 11, 15),
                        AcadStartDate = new DateTime(2017, 01, 15),
                        AcadHonors = new List<string>() { "MC" },
                        InstAttendGuid = "7a2bf6b5-cdcd-4c8f-b5d8-4664bf5b3fbc"
                    },
                    new Domain.Base.Entities.ExternalEducation("849e6a7c-6cd4-4f98-8a73-ab0aa3627f0d")
                    {
                        AcadPersonId = "1",
                        AcadInstitutionsId = "2",
                        Id = "2",
                        AcadAcadProgram = "ENGL.BA",
                        AcadCcd = new List<string>() { "GED" },
                        AcadCcdDate = new List<DateTime?>() { new DateTime(2016, 11, 20) },
                        AcadDegreeDate = new DateTime(2016, 11, 15),
                        AcadStartDate = new DateTime(2014, 01, 15),
                        InstAttendGuid = "849e6a7c-6cd4-4f98-8a73-ab0aa8875f0d"
                    },
                    new Domain.Base.Entities.ExternalEducation("d2253ac7-9931-4560-b42f-1fccd43c952e")
                    {
                        AcadPersonId = "1",
                        AcadInstitutionsId = "3",
                        Id = "3",
                        AcadAcadProgram = "HIST.BA",
                        AcadDegree = "MA",
                        AcadThesis = "Making Doughnuts",
                        AcadDegreeDate = new DateTime(2014, 11, 15),
                        AcadStartDate = new DateTime(2012, 01, 15),
                        InstAttendGuid = "d2253ac7-9931-2289-b42f-1fccd43c952e",
                        InstExtCredits = 3000,
                        AcadRankDenominator = 1500,
                        AcadRankNumerator = 12,
                        AcadRankPercent = 10
                    }
                };

                _personExternalEducationCredentialsRepoMock.Setup(repo => repo.GetExternalEducationCredentialsAsync(offset, limit, It.IsAny<string[]>(), It.IsAny<string>(), It.IsAny<bool>()))
                    .ReturnsAsync(new Tuple<IEnumerable<Domain.Base.Entities.ExternalEducation>, int>(_personExternalEducationCredentialsCollection, 3));

                _personExternalEducationCredentialsRepoMock.Setup(repo => repo.GetExternalEducationCredentialsByGuidAsync(It.IsAny<string>()))
                    .ReturnsAsync(_personExternalEducationCredentialsCollection.FirstOrDefault(cs => cs.Id == personExternalEducationCredentialsCode));

                // Mock all Reference Data
                #region Reference Methods
                _academicHonors = new List<Domain.Base.Entities.OtherHonor>()
                {
                    new Domain.Base.Entities.OtherHonor("59e8c15f-c310-48b5-9776-88f5ef722bfa","MC","Magna Cum Laude"),
                    new Domain.Base.Entities.OtherHonor("f138e083-00eb-49e9-a329-4e9be6789354","HD","With High Distinction")
                };
                _referenceRepositoryMock.Setup(repo => repo.GetOtherHonorsAsync(It.IsAny<bool>())).ReturnsAsync(_academicHonors);

                _otherCertifications = new List<Domain.Base.Entities.OtherCcd>()
                {
                    new Domain.Base.Entities.OtherCcd("f5ab51c4-eef7-4213-9a40-38b53bf73ff6","CERT","Certificate") { CredentialTypeID = "1" },
                    new Domain.Base.Entities.OtherCcd("b9cc39e8-e3a0-4774-93b8-986c04b15d07","EMR","Emergency Medical Tech") { CredentialTypeID = "1" },
                    new Domain.Base.Entities.OtherCcd("3ba3fe5c-6410-4d08-bee4-2c2609646230","GED","High School Equivalency") { CredentialTypeID = "1" }
                };
                _referenceRepositoryMock.Setup(repo => repo.GetOtherCcdsAsync(It.IsAny<bool>())).ReturnsAsync(_otherCertifications);

                _ccdTypes = new List<Domain.Base.Entities.CcdType>()
                {
                    new Domain.Base.Entities.CcdType("1","Level 1","1")
                };
                _referenceRepositoryMock.Setup(repo => repo.GetCcdTypeAsync(It.IsAny<bool>())).ReturnsAsync(_ccdTypes);

                _academicDisciplines = new List<Domain.Base.Entities.AcademicDiscipline>()
                {
                    new Domain.Base.Entities.AcademicDiscipline("6a3ec59c-940c-492d-beea-c8431b026871","ARTH","Art History", Domain.Base.Entities.AcademicDisciplineType.Major),
                    new Domain.Base.Entities.AcademicDiscipline("069ee03d-b085-4a7f-8d04-a859af172260","BIOL","Biology", Domain.Base.Entities.AcademicDisciplineType.Minor),
                    new Domain.Base.Entities.AcademicDiscipline("1bf32fe7-a9a8-469b-a4a4-28905a1b1a29","FINA","Finance", Domain.Base.Entities.AcademicDisciplineType.Concentration)
                };
                _referenceRepositoryMock.Setup(repo => repo.GetAcademicDisciplinesAsync(It.IsAny<bool>())).ReturnsAsync(_academicDisciplines);

                _academicCredentials = new List<Domain.Base.Entities.AcadCredential>()
                {
                    new Domain.Base.Entities.AcadCredential("4d5945fb-7dca-4a3a-9335-2ab5213d9eaf","MA","Master of Arts", Domain.Base.Entities.AcademicCredentialType.Degree),
                    new Domain.Base.Entities.AcadCredential("3e4ac23d-79dc-4d43-a5d2-fb254cb13c62","BA","Bachelor of Arts", Domain.Base.Entities.AcademicCredentialType.Degree),
                    new Domain.Base.Entities.AcadCredential("f5ab51c4-eef7-4213-9a40-38b53bf73ff6","CERT","Certificate", Domain.Base.Entities.AcademicCredentialType.Certificate),
                    new Domain.Base.Entities.AcadCredential("b9cc39e8-e3a0-4774-93b8-986c04b15d07","EMR","Emergency Medical Tech", Domain.Base.Entities.AcademicCredentialType.Certificate),
                    new Domain.Base.Entities.AcadCredential("3ba3fe5c-6410-4d08-bee4-2c2609646230","GED","High School Equivalency", Domain.Base.Entities.AcademicCredentialType.Certificate)
                };
                _referenceRepositoryMock.Setup(repo => repo.GetAcadCredentialsAsync(It.IsAny<bool>())).ReturnsAsync(_academicCredentials);
                #endregion

                _personExternalEducationCredentialsService = new PersonExternalEducationCredentialsService(_personExternalEducationCredentialsRepoMock.Object,
                    _institutionsRepoMock.Object, _referenceRepositoryMock.Object, _adapterRegistryMock.Object, currentUserFactory,
                    _roleRepositoryMock.Object, _configurationRepoMock.Object, _loggerMock.Object);
            }

            [TestCleanup]
            public void Cleanup()
            {
                _personExternalEducationCredentialsService = null;
                _personExternalEducationCredentialsCollection = null;
                _referenceRepositoryMock = null;
                _loggerMock = null;
                currentUserFactory = null;
                _roleRepositoryMock = null;
                _configurationRepoMock = null;
            }

            [TestMethod]
            public async Task PersonExternalEducationCredentialsService_GetPersonExternalEducationCredentialsAsync()
            {
                var results = await _personExternalEducationCredentialsService.GetPersonExternalEducationCredentialsAsync(offset, limit, string.Empty, true);
                Assert.IsTrue(results.Item1 is IEnumerable<Dtos.PersonExternalEducationCredentials>);
                Assert.AreEqual(results.Item2, 3);
                Assert.IsNotNull(results);
            }

            [TestMethod]
            public async Task PersonExternalEducationCredentialsSaervice_GetPersonExternalEducationCredentialsAsync_Count()
            {
                var results = await _personExternalEducationCredentialsService.GetPersonExternalEducationCredentialsAsync(offset, limit, string.Empty, true);
                Assert.AreEqual(results.Item2, 3);
            }

            [TestMethod]
            public async Task PersonExternalEducationCredentialsService_GetPersonExternalEducationCredentialsAsync_Properties()
            {
                var result =
                    (await _personExternalEducationCredentialsService.GetPersonExternalEducationCredentialsAsync(offset, limit, string.Empty, true)).Item1.FirstOrDefault(x => x.Id == personExternalEducationCredentialsGuid);

                Assert.IsNotNull(result.Id);
                Assert.IsNotNull(result.Credential);
            }

            [TestMethod]
            public async Task PersonExternalEducationCredentialsService_GetPersonExternalEducationCredentialsAsync_Expected()
            {
                var expectedResults = _personExternalEducationCredentialsCollection.FirstOrDefault(c => c.Guid == personExternalEducationCredentialsGuid);
                var actualResult =
                    (await _personExternalEducationCredentialsService.GetPersonExternalEducationCredentialsAsync(offset, limit, string.Empty, true)).Item1.FirstOrDefault(x => x.Id == personExternalEducationCredentialsGuid);

                Assert.AreEqual(expectedResults.Guid, actualResult.Id);
                Assert.AreEqual(expectedResults.AcadRankPercent, actualResult.ClassPercentile);
                Assert.AreEqual(expectedResults.AcadRankDenominator, actualResult.ClassSize);
                Assert.AreEqual(expectedResults.AcadRankNumerator, actualResult.ClassRank);
                Assert.AreEqual(expectedResults.InstAttendGuid, actualResult.ExternalEducation.Id);
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task PersonExternalEducationCredentialsService_GetPersonExternalEducationCredentialsByGuidAsync_Empty()
            {
                _personExternalEducationCredentialsRepoMock.Setup(repo => repo.GetExternalEducationCredentialsByGuidAsync(It.IsAny<string>()))
                    .Throws<KeyNotFoundException>();

                await _personExternalEducationCredentialsService.GetPersonExternalEducationCredentialsByGuidAsync("");
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task PersonExternalEducationCredentialsService_GetPersonExternalEducationCredentialsByGuidAsync_Null()
            {
                _personExternalEducationCredentialsRepoMock.Setup(repo => repo.GetExternalEducationCredentialsByGuidAsync(It.IsAny<string>()))
                    .Throws<KeyNotFoundException>();

                await _personExternalEducationCredentialsService.GetPersonExternalEducationCredentialsByGuidAsync(null);
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task PersonExternalEducationCredentialsService_GetPersonExternalEducationCredentialsByGuidAsync_InvalidId()
            {
                _personExternalEducationCredentialsRepoMock.Setup(repo => repo.GetExternalEducationCredentialsByGuidAsync(It.IsAny<string>()))
                    .Throws<KeyNotFoundException>();

                await _personExternalEducationCredentialsService.GetPersonExternalEducationCredentialsByGuidAsync("99");
            }

            [TestMethod]
            public async Task PersonExternalEducationCredentialsService_GetPersonExternalEducationCredentialsByGuidAsync_Expected()
            {
                var expectedResults =
                    _personExternalEducationCredentialsCollection.First(c => c.Guid == personExternalEducationCredentialsGuid);
                var actualResult =
                    await _personExternalEducationCredentialsService.GetPersonExternalEducationCredentialsByGuidAsync(personExternalEducationCredentialsGuid);
                Assert.AreEqual(expectedResults.Guid, actualResult.Id);

            }

            [TestMethod]
            public async Task PersonExternalEducationCredentialsService_GetPersonExternalEducationCredentialsByGuidAsync_Properties()
            {
                var result =
                    await _personExternalEducationCredentialsService.GetPersonExternalEducationCredentialsByGuidAsync(personExternalEducationCredentialsGuid);
                Assert.IsNotNull(result.Id);

            }
        }

        [TestClass]
        public class PersonExternalEducationCredentialsServiceTests_PutPost : CurrentUserSetup
        {

            private const string personExternalEducationCredentialsGuid = "7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc";
            private const string personExternalEducationCredentialsCode = "1";
            private ICollection<Domain.Base.Entities.ExternalEducation> _personExternalEducationCredentialsCollection;
            private PersonExternalEducationCredentialsService _personExternalEducationCredentialsService;
            private IEnumerable<Domain.Base.Entities.OtherHonor> _academicHonors;
            private IEnumerable<Domain.Base.Entities.OtherCcd> _otherCertifications;
            private IEnumerable<Domain.Base.Entities.CcdType> _ccdTypes;
            private IEnumerable<Domain.Base.Entities.AcademicDiscipline> _academicDisciplines;
            private IEnumerable<Domain.Base.Entities.AcadCredential> _academicCredentials;
            private Domain.Base.Entities.Institution _institution;

            private readonly int offset = 0;
            private readonly int limit = 100;

            private Mock<IReferenceDataRepository> _referenceRepositoryMock;
            private Mock<ILogger> _loggerMock;
            private Mock<IAdapterRegistry> _adapterRegistryMock;
            private ICurrentUserFactory currentUserFactory;
            private Mock<IRoleRepository> _roleRepositoryMock;
            private Mock<IConfigurationRepository> _configurationRepoMock;
            private Mock<IPersonExternalEducationCredentialsRepository> _personExternalEducationCredentialsRepoMock;
            private Mock<IInstitutionRepository> _institutionsRepoMock;

            private List<Ellucian.Colleague.Dtos.PersonExternalEducationCredentials> _personExternalEducationCredentialsDtos;

            [TestInitialize]
            public void Initialize()
            {
                _personExternalEducationCredentialsRepoMock = new Mock<IPersonExternalEducationCredentialsRepository>();
                _institutionsRepoMock = new Mock<IInstitutionRepository>();
                _referenceRepositoryMock = new Mock<IReferenceDataRepository>();
                _adapterRegistryMock = new Mock<IAdapterRegistry>();
                _loggerMock = new Mock<ILogger>();
                currentUserFactory = new CurrentUserSetup.ThirdPartyUserFactory();
                _roleRepositoryMock = new Mock<IRoleRepository>();
                _configurationRepoMock = new Mock<IConfigurationRepository>();


                viewExternalCredentialsRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(BasePermissionCodes.ViewPersonExternalEducationCredentials));
                updateExternalCredentialsRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(BasePermissionCodes.UpdatePersonExternalEducationCredentials));

                BuildData();

                _roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { viewExternalCredentialsRole, updateExternalCredentialsRole });

                _personExternalEducationCredentialsCollection = new List<Domain.Base.Entities.ExternalEducation>()
                {
                     new Domain.Base.Entities.ExternalEducation("7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc")
                    {
                        AcadPersonId = "1",
                        AcadInstitutionsId = "1",
                        Id = "1",
                        AcadAcadProgram = "MATH.BA",
                        AcadDegree = "BA",
                        AcadCcd = new List<string>() { "CERT", "EMR"},
                        AcadCcdDate = new List<DateTime?>() { new DateTime(2019, 11, 20), new DateTime(2018, 11, 15) },
                        AcadDegreeDate = new DateTime(2019, 11, 15),
                        AcadStartDate = new DateTime(2017, 01, 15),
                        AcadHonors = new List<string>() { "MC" },
                        InstAttendGuid = "7a2bf6b5-cdcd-4c8f-b5d8-4664bf5b3fbc"
                    },
                    new Domain.Base.Entities.ExternalEducation("849e6a7c-6cd4-4f98-8a73-ab0aa3627f0d")
                    {
                        AcadPersonId = "1",
                        AcadInstitutionsId = "2",
                        Id = "2",
                        AcadAcadProgram = "ENGL.BA",
                        AcadCcd = new List<string>() { "GED" },
                        AcadCcdDate = new List<DateTime?>() { new DateTime(2016, 11, 20) },
                        AcadDegreeDate = new DateTime(2016, 11, 15),
                        AcadStartDate = new DateTime(2014, 01, 15),
                        InstAttendGuid = "849e6a7c-6cd4-4f98-8a73-ab0aa8875f0d"
                    },
                    new Domain.Base.Entities.ExternalEducation("d2253ac7-9931-4560-b42f-1fccd43c952e")
                    {
                        AcadPersonId = "1",
                        AcadInstitutionsId = "3",
                        Id = "3",
                        AcadAcadProgram = "HIST.BA",
                        AcadDegree = "MA",
                        AcadThesis = "Making Doughnuts",
                        AcadDegreeDate = new DateTime(2014, 11, 15),
                        AcadStartDate = new DateTime(2012, 01, 15),
                        InstAttendGuid = "d2253ac7-9931-2289-b42f-1fccd43c952e",
                        InstExtCredits = 3000,
                        AcadRankDenominator = 1500,
                        AcadRankNumerator = 12,
                        AcadRankPercent = 10
                    }
                };
                
                _personExternalEducationCredentialsRepoMock.Setup(repo => repo.GetExternalEducationIdFromGuidAsync(It.IsAny<string>()))
                    .ReturnsAsync("0003397*0000043");

                _personExternalEducationCredentialsRepoMock.Setup(repo => repo.GetExternalEducationCredentialsIdFromGuidAsync(It.IsAny<string>()))
                    .ReturnsAsync("1");

                _personExternalEducationCredentialsRepoMock.Setup(repo => repo.GetExternalEducationCredentialsAsync(offset, limit, It.IsAny<string[]>(), It.IsAny<string>(), It.IsAny<bool>()))
                    .ReturnsAsync(new Tuple<IEnumerable<Domain.Base.Entities.ExternalEducation>, int>(_personExternalEducationCredentialsCollection, 3));

                _personExternalEducationCredentialsRepoMock.Setup(repo => repo.GetExternalEducationCredentialsByGuidAsync(It.IsAny<string>()))
                    .ReturnsAsync(_personExternalEducationCredentialsCollection.FirstOrDefault(cs => cs.Id == personExternalEducationCredentialsCode));

                _personExternalEducationCredentialsRepoMock.Setup(repo => repo.CreateExternalEducationCredentialsAsync(It.IsAny<Domain.Base.Entities.ExternalEducation>()))
                    .ReturnsAsync(_personExternalEducationCredentialsCollection.FirstOrDefault(cs => cs.Id == personExternalEducationCredentialsCode));

                _personExternalEducationCredentialsRepoMock.Setup(repo => repo.UpdateExternalEducationCredentialsAsync(It.IsAny<Domain.Base.Entities.ExternalEducation>()))
                    .ReturnsAsync(_personExternalEducationCredentialsCollection.FirstOrDefault(cs => cs.Id == personExternalEducationCredentialsCode));

                // Mock all Reference Data
                #region Reference Methods
                _academicHonors = new List<Domain.Base.Entities.OtherHonor>()
                {
                    new Domain.Base.Entities.OtherHonor("59e8c15f-c310-48b5-9776-88f5ef722bfa","MC","Magna Cum Laude"),
                    new Domain.Base.Entities.OtherHonor("f138e083-00eb-49e9-a329-4e9be6789354","HD","With High Distinction")
                };
                _referenceRepositoryMock.Setup(repo => repo.GetOtherHonorsAsync(It.IsAny<bool>())).ReturnsAsync(_academicHonors);

                _otherCertifications = new List<Domain.Base.Entities.OtherCcd>()
                {
                    new Domain.Base.Entities.OtherCcd("f5ab51c4-eef7-4213-9a40-38b53bf73ff6","CERT","Certificate") { CredentialTypeID = "1" },
                    new Domain.Base.Entities.OtherCcd("b9cc39e8-e3a0-4774-93b8-986c04b15d07","EMR","Emergency Medical Tech") { CredentialTypeID = "1" },
                    new Domain.Base.Entities.OtherCcd("3ba3fe5c-6410-4d08-bee4-2c2609646230","GED","High School Equivalency") { CredentialTypeID = "1" }
                };
                _referenceRepositoryMock.Setup(repo => repo.GetOtherCcdsAsync(It.IsAny<bool>())).ReturnsAsync(_otherCertifications);

                _ccdTypes = new List<Domain.Base.Entities.CcdType>()
                {
                    new Domain.Base.Entities.CcdType("1","Level 1","1")
                };
                _referenceRepositoryMock.Setup(repo => repo.GetCcdTypeAsync(It.IsAny<bool>())).ReturnsAsync(_ccdTypes);

                _academicDisciplines = new List<Domain.Base.Entities.AcademicDiscipline>()
                {
                    new Domain.Base.Entities.AcademicDiscipline("6a3ec59c-940c-492d-beea-c8431b026871","ARTH","Art History", Domain.Base.Entities.AcademicDisciplineType.Major),
                    new Domain.Base.Entities.AcademicDiscipline("069ee03d-b085-4a7f-8d04-a859af172260","BIOL","Biology", Domain.Base.Entities.AcademicDisciplineType.Minor),
                    new Domain.Base.Entities.AcademicDiscipline("1bf32fe7-a9a8-469b-a4a4-28905a1b1a29","FINA","Finance", Domain.Base.Entities.AcademicDisciplineType.Concentration)
                };
                _referenceRepositoryMock.Setup(repo => repo.GetAcademicDisciplinesAsync(It.IsAny<bool>())).ReturnsAsync(_academicDisciplines);

                _academicCredentials = new List<Domain.Base.Entities.AcadCredential>()
                {
                    new Domain.Base.Entities.AcadCredential("4d5945fb-7dca-4a3a-9335-2ab5213d9eaf","MA","Master of Arts", Domain.Base.Entities.AcademicCredentialType.Degree),
                    new Domain.Base.Entities.AcadCredential("3e4ac23d-79dc-4d43-a5d2-fb254cb13c62","BA","Bachelor of Arts", Domain.Base.Entities.AcademicCredentialType.Degree),
                    new Domain.Base.Entities.AcadCredential("f5ab51c4-eef7-4213-9a40-38b53bf73ff6","CERT","Certificate", Domain.Base.Entities.AcademicCredentialType.Certificate),
                    new Domain.Base.Entities.AcadCredential("b9cc39e8-e3a0-4774-93b8-986c04b15d07","EMR","Emergency Medical Tech", Domain.Base.Entities.AcademicCredentialType.Certificate),
                    new Domain.Base.Entities.AcadCredential("3ba3fe5c-6410-4d08-bee4-2c2609646230","GED","High School Equivalency", Domain.Base.Entities.AcademicCredentialType.Certificate)
                };
                _referenceRepositoryMock.Setup(repo => repo.GetAcadCredentialsAsync(It.IsAny<bool>())).ReturnsAsync(_academicCredentials);

                _institution = new Domain.Base.Entities.Institution("0000043", Domain.Base.Entities.InstType.College);
                _institutionsRepoMock.Setup(repo => repo.GetInstitutionAsync(It.IsAny<string>())).ReturnsAsync(_institution);

                #endregion

                _personExternalEducationCredentialsService = new PersonExternalEducationCredentialsService(_personExternalEducationCredentialsRepoMock.Object,
                    _institutionsRepoMock.Object, _referenceRepositoryMock.Object, _adapterRegistryMock.Object, currentUserFactory,
                    _roleRepositoryMock.Object, _configurationRepoMock.Object, _loggerMock.Object);
            }

            private void BuildData()
            {
                _personExternalEducationCredentialsDtos = new List<Dtos.PersonExternalEducationCredentials>()
                {
                    new Dtos.PersonExternalEducationCredentials()
                    {
                        ExternalEducation = new GuidObject2("1df164eb-8178-4321-a9f7-24f27f3991d8"),
                        Credential = new GuidObject2("3e4ac23d-79dc-4d43-a5d2-fb254cb13c62"),
                        EarnedOn = new DateTime(2018, 12, 17),
                        Disciplines = new List<GuidObject2>()
                        {
                            new GuidObject2("6a3ec59c-940c-492d-beea-c8431b026871"),
                            new GuidObject2("069ee03d-b085-4a7f-8d04-a859af172260")
                        },
                        Recognitions = new List<GuidObject2>()
                        {
                            new GuidObject2("59e8c15f-c310-48b5-9776-88f5ef722bfa"),
                            new GuidObject2("f138e083-00eb-49e9-a329-4e9be6789354")
                        },
                        ThesisTitle = "Making donuts",
                        ClassSize = 300,
                        ClassPercentile = 75,
                        ClassRank = 25,
                        AttendancePeriods = new List<PersonExternalEducationCredentialsAttendanceperiodsDtoProperty>()
                        {
                            new PersonExternalEducationCredentialsAttendanceperiodsDtoProperty()
                            {
                                StartOn = new DateTime(2016, 09, 15),
                                EndOn = new DateTime(2018, 12, 17)
                            }
                        },
                        PerformanceMeasure = "99",
                        SupplementalCredentials = new List<PersonExternalEducationCredentialsSupplementalcredentialsDtoProperty>()
                        {
                            new PersonExternalEducationCredentialsSupplementalcredentialsDtoProperty()
                            {
                                Credential = new GuidObject2("f5ab51c4-eef7-4213-9a40-38b53bf73ff6"),
                                EarnedOn = new DateTime(2018, 12, 17)
                            }
                        },
                        Id = "7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc"
                    }
                };
            }


            [TestCleanup]
            public void Cleanup()
            {
                _personExternalEducationCredentialsService = null;
                _personExternalEducationCredentialsCollection = null;
                _referenceRepositoryMock = null;
                _loggerMock = null;
                currentUserFactory = null;
                _roleRepositoryMock = null;
                _configurationRepoMock = null;
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task PersonExternalEducationCredentialsService_CreatePersonExternalEducationCredentialsAsync_ArgumentNullException()
            {
                await _personExternalEducationCredentialsService.CreatePersonExternalEducationCredentialsAsync(null);

            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task PersonExternalEducationCredentialsService_CreatePersonExternalEducationCredentialsAsync_NullId()
            {
                var personExternalEducationCredentialsDto = new Dtos.PersonExternalEducationCredentials();
                personExternalEducationCredentialsDto.Id = null;
                await _personExternalEducationCredentialsService.CreatePersonExternalEducationCredentialsAsync(null);
            }

            [TestMethod]
            public async Task PersonExternalEducationCredentialsService_CreatePersonExternalEducationCredentialsAsync()
            {
                var personExternalEducationCredential = _personExternalEducationCredentialsDtos.FirstOrDefault(x => x.Id == personExternalEducationCredentialsGuid);
                var result = await _personExternalEducationCredentialsService.CreatePersonExternalEducationCredentialsAsync(personExternalEducationCredential);
                Assert.IsNotNull(result.Id);
            }


            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task PersonExternalEducationCredentialsService_UpdatePersonExternalEducationCredentialsAsync_ArgumentNullException()
            {
                await _personExternalEducationCredentialsService.UpdatePersonExternalEducationCredentialsAsync(null);

            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task PersonExternalEducationCredentialsService_UpdatePersonExternalEducationCredentialsAsync_NullId()
            {
                var personExternalEducationCredentialsDto = new Dtos.PersonExternalEducationCredentials();
                personExternalEducationCredentialsDto.Id = null;
                await _personExternalEducationCredentialsService.UpdatePersonExternalEducationCredentialsAsync(null);
            }

            [TestMethod]
            public async Task PersonExternalEducationCredentialsService_UpdatePersonExternalEducationCredentialsAsync()
            {
                var personExternalEducationCredential = _personExternalEducationCredentialsDtos.FirstOrDefault(x => x.Id == personExternalEducationCredentialsGuid);
                var result = await _personExternalEducationCredentialsService.UpdatePersonExternalEducationCredentialsAsync(personExternalEducationCredential);
                Assert.IsNotNull(result.Id);
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task PersonExternalEducationCredentialsService_UpdatePersonExternalEducationCredentialsAsync_BadInstType()
            {
                //  Error if high school and has any of disciplines, performance measures, class size/rank/percentile, thesis.
                _institution = new Domain.Base.Entities.Institution("0000043", Domain.Base.Entities.InstType.HighSchool);
                _institutionsRepoMock.Setup(repo => repo.GetInstitutionAsync(It.IsAny<string>())).ReturnsAsync(_institution);                
                _personExternalEducationCredentialsService = new PersonExternalEducationCredentialsService(_personExternalEducationCredentialsRepoMock.Object,
                    _institutionsRepoMock.Object, _referenceRepositoryMock.Object, _adapterRegistryMock.Object, currentUserFactory,
                    _roleRepositoryMock.Object, _configurationRepoMock.Object, _loggerMock.Object);

                var personExternalEducationCredential = _personExternalEducationCredentialsDtos.FirstOrDefault(x => x.Id == personExternalEducationCredentialsGuid);
                var result = await _personExternalEducationCredentialsService.UpdatePersonExternalEducationCredentialsAsync(personExternalEducationCredential);
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task PersonExternalEducationCredentialsService_UpdatePersonExternalEducationCredentialsAsync_InvalidCredential()
            {
                _personExternalEducationCredentialsDtos = new List<Dtos.PersonExternalEducationCredentials>()
                {
                    new Dtos.PersonExternalEducationCredentials()
                    {
                        ExternalEducation = new GuidObject2("1df164eb-8178-4321-a9f7-24f27f3991d8"),
                        Credential = new GuidObject2("abc"),
                        Id = "7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc"
                    }
                };
                _personExternalEducationCredentialsRepoMock.Setup(repo => repo.UpdateExternalEducationCredentialsAsync(It.IsAny<Domain.Base.Entities.ExternalEducation>()))
                    .ReturnsAsync(_personExternalEducationCredentialsCollection.FirstOrDefault(cs => cs.Id == personExternalEducationCredentialsCode));
                
                var personExternalEducationCredential = _personExternalEducationCredentialsDtos.FirstOrDefault(x => x.Id == personExternalEducationCredentialsGuid);
                var result = await _personExternalEducationCredentialsService.UpdatePersonExternalEducationCredentialsAsync(personExternalEducationCredential);  
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task PersonExternalEducationCredentialsService_UpdatePersonExternalEducationCredentialsAsync_InvalidDiscipline()
            {
                _personExternalEducationCredentialsDtos = new List<Dtos.PersonExternalEducationCredentials>()
                {
                    new Dtos.PersonExternalEducationCredentials()
                    {
                        Credential = new GuidObject2("3e4ac23d-79dc-4d43-a5d2-fb254cb13c62"),
                        ExternalEducation = new GuidObject2("1df164eb-8178-4321-a9f7-24f27f3991d8"),
                        Disciplines = new List<GuidObject2>()
                        {
                            new GuidObject2("123")
                        },
                        Id = "7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc"
                    }
                };
                _personExternalEducationCredentialsRepoMock.Setup(repo => repo.UpdateExternalEducationCredentialsAsync(It.IsAny<Domain.Base.Entities.ExternalEducation>()))
                    .ReturnsAsync(_personExternalEducationCredentialsCollection.FirstOrDefault(cs => cs.Id == personExternalEducationCredentialsCode));

                var personExternalEducationCredential = _personExternalEducationCredentialsDtos.FirstOrDefault(x => x.Id == personExternalEducationCredentialsGuid);
                var result = await _personExternalEducationCredentialsService.UpdatePersonExternalEducationCredentialsAsync(personExternalEducationCredential);
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task PersonExternalEducationCredentialsService_UpdatePersonExternalEducationCredentialsAsync_InvalidMultipleAttendancePeriods()
            {                
                _personExternalEducationCredentialsDtos = new List<Dtos.PersonExternalEducationCredentials>()
                {
                    new Dtos.PersonExternalEducationCredentials()
                    {
                        ExternalEducation = new GuidObject2("1df164eb-8178-4321-a9f7-24f27f3991d8"),
                        Credential = new GuidObject2("3e4ac23d-79dc-4d43-a5d2-fb254cb13c62"),
                        AttendancePeriods = new List<PersonExternalEducationCredentialsAttendanceperiodsDtoProperty>()
                        {
                            new PersonExternalEducationCredentialsAttendanceperiodsDtoProperty()
                            {
                                StartOn = new DateTime(2016, 09, 15),
                                EndOn = new DateTime(2017, 05, 17)
                            },
                            new PersonExternalEducationCredentialsAttendanceperiodsDtoProperty()
                            {
                                StartOn = new DateTime(2017, 09, 15),
                                EndOn = new DateTime(2018, 12, 17)
                            }
                        },
                        Id = "7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc"
                    }
                };
                _personExternalEducationCredentialsRepoMock.Setup(repo => repo.UpdateExternalEducationCredentialsAsync(It.IsAny<Domain.Base.Entities.ExternalEducation>()))
                    .ReturnsAsync(_personExternalEducationCredentialsCollection.FirstOrDefault(cs => cs.Id == personExternalEducationCredentialsCode));

                var personExternalEducationCredential = _personExternalEducationCredentialsDtos.FirstOrDefault(x => x.Id == personExternalEducationCredentialsGuid);
                var result = await _personExternalEducationCredentialsService.UpdatePersonExternalEducationCredentialsAsync(personExternalEducationCredential);
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task PersonExternalEducationCredentialsService_UpdatePersonExternalEducationCredentialsAsync_MissingExternalEducation()
            {
                _personExternalEducationCredentialsDtos = new List<Dtos.PersonExternalEducationCredentials>()
                {
                    new Dtos.PersonExternalEducationCredentials()
                    {
                        Credential = new GuidObject2("3e4ac23d-79dc-4d43-a5d2-fb254cb13c62"),  
                        Id = "7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc"
                    }
                };
                _personExternalEducationCredentialsRepoMock.Setup(repo => repo.UpdateExternalEducationCredentialsAsync(It.IsAny<Domain.Base.Entities.ExternalEducation>()))
                    .ReturnsAsync(_personExternalEducationCredentialsCollection.FirstOrDefault(cs => cs.Id == personExternalEducationCredentialsCode));

                var personExternalEducationCredential = _personExternalEducationCredentialsDtos.FirstOrDefault(x => x.Id == personExternalEducationCredentialsGuid);
                var result = await _personExternalEducationCredentialsService.UpdatePersonExternalEducationCredentialsAsync(personExternalEducationCredential);
            }
        }
    }
}

