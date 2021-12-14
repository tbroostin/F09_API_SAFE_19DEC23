//Copyright 2019-2020 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Colleague.Domain.Base;
using Ellucian.Colleague.Domain.Base.Entities;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Colleague.Dtos;
using Ellucian.Colleague.Dtos.Filters;
using Ellucian.Web.Adapters;
using Ellucian.Web.Http.Exceptions;
using Ellucian.Web.Security;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.Base.Tests.Services
{
    [TestClass]
    public class PersonExternalEducationServiceTests
    {
        public abstract class CurrentUserSetup
        {
            protected Ellucian.Colleague.Domain.Entities.Role viewExternalEducationsRole = new Ellucian.Colleague.Domain.Entities.Role(1, BasePermissionCodes.ViewExternalEducation);
            protected Ellucian.Colleague.Domain.Entities.Role updateExternalEducationRole = new Ellucian.Colleague.Domain.Entities.Role(1, BasePermissionCodes.CreateExternalEducation);

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
                            Roles = new List<string>() { BasePermissionCodes.ViewExternalEducation, BasePermissionCodes.CreateExternalEducation },
                            SessionFixationId = "abc123"
                        });
                    }
                }
            }
        }

        [TestClass]
        public class PersonExternalEducation_Tests : CurrentUserSetup
        {
            private const string personExternalEducationGuid = "3f67b180-ce1d-4552-8d81-feb96b9fea5b";
            private const string personExternalEducationCode = "1";
            private ICollection<Domain.Base.Entities.InstitutionsAttend> _personExternalEducationEntityCollection;
            private IEnumerable<Dtos.PersonExternalEducation> _personExternalEducationDtoCollection;
            private Dictionary<string, string> _personGuidCollection;
            private PersonExternalEducationService _personExternalEducationService;
            private readonly int offset = 0;
            private readonly int limit = 100;

            private Mock<IReferenceDataRepository> _referenceRepositoryMock;
            private Mock<ILogger> _loggerMock;
            private Mock<IAdapterRegistry> _adapterRegistryMock;
            private ICurrentUserFactory currentUserFactory;
            private Mock<IRoleRepository> _roleRepositoryMock;
            private Mock<IConfigurationRepository> _configurationRepoMock;
            private Mock<IInstitutionsAttendRepository> _InstitutionsAttendRepoMock;
            private Mock<IInstitutionRepository> _institutionsRepoMock;
            private Mock<IPersonRepository> _personRepoMock;

            [TestInitialize]
            public void Initialize()
            {
                _personRepoMock = new Mock<IPersonRepository>();
                _InstitutionsAttendRepoMock = new Mock<IInstitutionsAttendRepository>();
                _institutionsRepoMock = new Mock<IInstitutionRepository>();
                _referenceRepositoryMock = new Mock<IReferenceDataRepository>();
                _adapterRegistryMock = new Mock<IAdapterRegistry>();
                _loggerMock = new Mock<ILogger>();
                currentUserFactory = new CurrentUserSetup.ThirdPartyUserFactory();
                _roleRepositoryMock = new Mock<IRoleRepository>();
                _configurationRepoMock = new Mock<IConfigurationRepository>();


                viewExternalEducationsRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(BasePermissionCodes.ViewExternalEducation));
                updateExternalEducationRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(BasePermissionCodes.CreateExternalEducation));

                _roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { viewExternalEducationsRole, updateExternalEducationRole });

                BuildData();

                _InstitutionsAttendRepoMock.Setup(repo => repo.GetInstitutionsAttendAsync(offset, limit, It.IsAny<string>(), It.IsAny<string[]>(), It.IsAny<string>(), It.IsAny<string>(), 
                    It.IsAny<InstType?>(), It.IsAny<bool>()))
                    .ReturnsAsync(new Tuple<IEnumerable<Domain.Base.Entities.InstitutionsAttend>, int>(_personExternalEducationEntityCollection, 4));
                _InstitutionsAttendRepoMock.Setup(repo => repo.GetInstitutionsAttendAsync(offset, limit, It.IsAny<string>(), It.IsAny<string[]>(), It.IsAny<string>(), string.Empty,
                   null, It.IsAny<bool>()))
                   .ReturnsAsync(new Tuple<IEnumerable<Domain.Base.Entities.InstitutionsAttend>, int>(_personExternalEducationEntityCollection, 4));

                _InstitutionsAttendRepoMock.Setup(repo => repo.GetInstitutionsAttendIdFromGuidAsync(It.IsAny<string>()))
                    .ReturnsAsync(_personExternalEducationEntityCollection.FirstOrDefault(cs => cs.Guid == personExternalEducationGuid).Id);

                _InstitutionsAttendRepoMock.Setup(repo => repo.GetInstitutionAttendByIdAsync(It.IsAny<string>()))
                    .ReturnsAsync(_personExternalEducationEntityCollection.FirstOrDefault(cs => cs.Guid == personExternalEducationGuid));

                _personRepoMock.Setup(repo => repo.GetPersonGuidsCollectionAsync(It.IsAny<IEnumerable<string>>())).ReturnsAsync(_personGuidCollection);

           
                _personRepoMock.Setup(repo => repo.GetPersonIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync("2");
                _personRepoMock.Setup(repo => repo.GetPersonIdForNonCorpOnly(It.IsAny<string>())).ReturnsAsync("2");

                _institutionsRepoMock.Setup(repo => repo.GetInstitutionFromGuidAsync(It.IsAny<string>())).ReturnsAsync("11");
                _institutionsRepoMock.Setup(repo => repo.GetInstitutionAsync(It.IsAny<string>())).ReturnsAsync(new Domain.Base.Entities.Institution("11", Domain.Base.Entities.InstType.HighSchool));
                _InstitutionsAttendRepoMock.Setup(repo => repo.UpdateExternalEducationAsync(It.IsAny<Domain.Base.Entities.InstitutionsAttend>()))
                    .ReturnsAsync(_personExternalEducationEntityCollection.FirstOrDefault(cs => cs.Guid == personExternalEducationGuid));
                
                _personExternalEducationService = new PersonExternalEducationService(_adapterRegistryMock.Object, currentUserFactory,
                    _roleRepositoryMock.Object, _InstitutionsAttendRepoMock.Object, _referenceRepositoryMock.Object,
                    _personRepoMock.Object, _institutionsRepoMock.Object,
                    _configurationRepoMock.Object, _loggerMock.Object);
            }

            private void BuildData()
            {
                _personExternalEducationDtoCollection = new List<PersonExternalEducation>()
                {
                    new Dtos.PersonExternalEducation()
                    {
                        Id = "bbd216fb-0fc5-4f44-ae45-42d3cdd1e89a",
                        Person = new GuidObject2("e214b7b0-aced-451b-aece-f97b1f5dfb10"),
                        Institution = new GuidObject2("ab1f9756-f5b4-4355-903e-513ff179a338"),
                        PerformanceMeasure = "3.900",

                    },
                    new Dtos.PersonExternalEducation()
                    {
                        Id = "3f67b180-ce1d-4552-8d81-feb96b9fea5b",
                        Person = new GuidObject2("f214b7b0-aced-451b-aece-f97b1f5dfb10"),
                        Institution = new GuidObject2("bb1f9756-f5b4-4355-903e-513ff179a338"),
                        PerformanceMeasure = "2.900",
                        ClassSize = 100,
                        ClassRank = 5,
                        TotalCredits = 10,
                        AttendancePeriods = new List<ExternalEducationAttendancePeriods>()
                        {
                            new ExternalEducationAttendancePeriods()
                                {
                                StartDate = new Dtos.DtoProperties.DateDtoProperty () { Day = 10, Month = 10, Year = 1970},
                                EndDate = new Dtos.DtoProperties.DateDtoProperty () { Day = 30, Month = 5, Year = 1976}
                                }
                        }
                    },
                    new Dtos.PersonExternalEducation()
                    {
                        Id = "bf67e156-8f5d-402b-8101-81b0a2796873",
                        Person = new GuidObject2("b214b7b0-aced-451b-aece-f97b1f5dfb10"),
                        Institution = new GuidObject2("cb1f9756-f5b4-4355-903e-513ff179a338"),
                        PerformanceMeasure = "1.900",
                        ClassSize = 320,
                        ClassRank = 52,
                        TotalCredits = 1,
                        AttendancePeriods = new List<ExternalEducationAttendancePeriods>()
                        {
                            new ExternalEducationAttendancePeriods()
                                {
                                StartDate = new Dtos.DtoProperties.DateDtoProperty () { Day = 10, Month = 10, Year = 2010},
                                EndDate = new Dtos.DtoProperties.DateDtoProperty () { Day = 30, Month = 5, Year = 2014}
                                }
                        }

                    },
                    new Dtos.PersonExternalEducation()
                    {
                        Id = "0111d6ef-5a86-465f-ac58-4265a997c136",
                        Person = new GuidObject2("1214b7b0-aced-451b-aece-f97b1f5dfb10"),
                        Institution = new GuidObject2("db1f9756-f5b4-4355-903e-513ff179a338"),
                        PerformanceMeasure = "2.900",

                    },
                };

                _personExternalEducationEntityCollection = new List<Domain.Base.Entities.InstitutionsAttend>();

                _personGuidCollection = new Dictionary<string, string>();
                int personId = 1;
                int institutionsId = 10;
                foreach (var dto in _personExternalEducationDtoCollection)
                {
                    _personGuidCollection.Add(personId.ToString(), dto.Person.Id);
                    _personGuidCollection.Add(institutionsId.ToString(), dto.Institution.Id);
                    var recordKey = string.Concat(personId.ToString(), "*", institutionsId.ToString());
                    personId++;
                    institutionsId++;

                    var institutionsAttendEntity = new Domain.Base.Entities.InstitutionsAttend(dto.Id, recordKey)
                    {
                        ExtGpa = !string.IsNullOrEmpty(dto.PerformanceMeasure) ? decimal.Parse(dto.PerformanceMeasure) : new decimal?(),
                        RankNumerator = dto.ClassRank,
                        RankDenominator = dto.ClassSize,
                        RankPercent = dto.ClassPercentile
                    };
                    if (dto.AttendancePeriods != null && dto.AttendancePeriods.Any())
                    {
                        institutionsAttendEntity.DatesAttended = new List<Tuple<DateTime?, DateTime?>>();
                        foreach (var attend in dto.AttendancePeriods)
                        {
                            if (attend.StartDate != null && attend.StartDate.Month != null && attend.StartDate.Day != null &&
                                attend.EndDate != null && attend.EndDate.Month != null && attend.EndDate.Day != null)
                            {
                                var start = new DateTime(attend.StartDate.Year, attend.StartDate.Month.Value, attend.StartDate.Day.Value);
                                var end = new DateTime(attend.EndDate.Year, attend.EndDate.Month.Value, attend.EndDate.Day.Value);
                                institutionsAttendEntity.DatesAttended.Add(new Tuple<DateTime?, DateTime?>(start, end));
                            }
                        }
                    }

                    _personExternalEducationEntityCollection.Add(institutionsAttendEntity);
                }
            }

            [TestCleanup]
            public void Cleanup()
            {
                _personExternalEducationService = null;
                _personExternalEducationEntityCollection = null;
                _referenceRepositoryMock = null;
                _loggerMock = null;
                currentUserFactory = null;
                _roleRepositoryMock = null;
                _configurationRepoMock = null;
            }

            [TestMethod]
            public async Task PersonExternalEducationService_GetPersonExternalEducationAsync()
            {
                var results = await _personExternalEducationService.GetPersonExternalEducationAsync(offset, limit, null, string.Empty, null, true);
                Assert.IsTrue(results.Item1 is IEnumerable<Dtos.PersonExternalEducation>);
                Assert.AreEqual(results.Item2, 4);
                Assert.IsNotNull(results);
                
                foreach (var actualResult in results.Item1)
                {
                    var expectedResults = _personExternalEducationDtoCollection.FirstOrDefault(c => c.Id == actualResult.Id);
                    Assert.AreEqual(expectedResults.Person.Id, actualResult.Person.Id);
                    Assert.AreEqual(expectedResults.Institution.Id, actualResult.Institution.Id);
                    Assert.AreEqual(expectedResults.ClassSize, actualResult.ClassSize);
                    Assert.AreEqual(expectedResults.ClassRank, actualResult.ClassRank);
                    Assert.AreEqual(expectedResults.ClassPercentile, actualResult.ClassPercentile);
                    if (expectedResults.AttendancePeriods != null && expectedResults.AttendancePeriods.Any())
                    {
                        Assert.AreEqual(expectedResults.AttendancePeriods.Count(), actualResult.AttendancePeriods.Count());
                    }
                }
            }

            [TestMethod]
            public async Task PersonExternalEducationService_GetPersonExternalEducationAsync_Count()
            {
                var results = await _personExternalEducationService.GetPersonExternalEducationAsync(offset, limit, null, string.Empty, null, true);
                Assert.AreEqual(results.Item2, 4);
            }

            [TestMethod]
            public async Task PersonExternalEducationService_GetPersonExternalEducationAsync_Expected()
            {
                var expectedResults = _personExternalEducationDtoCollection.FirstOrDefault(c => c.Id == personExternalEducationGuid);
                var actualResult =
                    (await _personExternalEducationService.GetPersonExternalEducationAsync(offset, limit, null, string.Empty, null, true)).Item1.FirstOrDefault(x => x.Id == personExternalEducationGuid);

                Assert.AreEqual(expectedResults.Id, actualResult.Id);
                Assert.AreEqual(expectedResults.Person.Id, actualResult.Person.Id);
                Assert.AreEqual(expectedResults.Institution.Id, actualResult.Institution.Id);
                Assert.AreEqual(expectedResults.ClassSize, actualResult.ClassSize);
                Assert.AreEqual(expectedResults.ClassRank, actualResult.ClassRank);
                Assert.AreEqual(expectedResults.ClassPercentile, actualResult.ClassPercentile);
                Assert.AreEqual(expectedResults.AttendancePeriods.Count(), actualResult.AttendancePeriods.Count());
            }

           
            [TestMethod]
            public async Task PersonExternalEducationService_GetPersonExternalEducationAsync_Criteria_InvalidPersonIdFilter()
            {
                _personRepoMock.Setup(repo => repo.GetPersonIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync("");

                PersonExternalEducation filter = new PersonExternalEducation()
                {
                    Person = new GuidObject2("invalid")
                };
                var results = await _personExternalEducationService.GetPersonExternalEducationAsync(offset, limit, filter, string.Empty, null, true);
                Assert.AreEqual(results.Item2, 0);
            }

            [TestMethod]
            public async Task PersonExternalEducationService_GetPersonExternalEducationAsync_PersonByInstitutionType_InvalidPersonIdFilter()
            {
               
                _personRepoMock.Setup(repo => repo.GetPersonIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync(string.Empty);

                PersonByInstitutionType filter = new PersonByInstitutionType()
                {
                    Person = new GuidObject2("invalid"),
                    Type = Dtos.EnumProperties.EducationalInstitutionType.PostSecondarySchool
                };
                var results = await _personExternalEducationService.GetPersonExternalEducationAsync(offset, limit, null, string.Empty,  filter, true);
                Assert.AreEqual(results.Item2, 0);
            }

            [TestMethod]
            public async Task PersonExternalEducationService_GetPersonExternalEducationAsync_PersonByInstitutionType_Exception()
            {
                _personRepoMock.Setup(repo => repo.GetPersonIdFromGuidAsync(It.IsAny<string>())).ThrowsAsync(new RepositoryException());

                PersonByInstitutionType filter = new PersonByInstitutionType()
                {
                    Person = new GuidObject2(Guid.NewGuid().ToString()),
                    Type = Dtos.EnumProperties.EducationalInstitutionType.PostSecondarySchool
                };
                var results = await _personExternalEducationService.GetPersonExternalEducationAsync(offset, limit, null, string.Empty, filter, true);
                Assert.AreEqual(results.Item2, 0);
            }

            [TestMethod]
            public async Task PersonExternalEducationService_GetPersonExternalEducationAsync_PersonByInstitutionType_PostSecondarySchool()
            {
                _InstitutionsAttendRepoMock.Setup(repo => repo.GetInstitutionsAttendAsync(offset, limit, It.IsAny<string>(), It.IsAny<string[]>(), It.IsAny<string>(), It.IsAny<string>(),
                   It.IsAny<InstType?>(), It.IsAny<bool>()))
                   .ReturnsAsync(new Tuple<IEnumerable<Domain.Base.Entities.InstitutionsAttend>, int>(_personExternalEducationEntityCollection, 4));

                PersonByInstitutionType filter = new PersonByInstitutionType()
                {
                    Person = new GuidObject2(Guid.NewGuid().ToString()),
                    Type = Dtos.EnumProperties.EducationalInstitutionType.PostSecondarySchool
                };
                var results = await _personExternalEducationService.GetPersonExternalEducationAsync(offset, limit, null, string.Empty, filter, true);
                Assert.AreEqual(results.Item2, 4);
            }

            [TestMethod]
            public async Task PersonExternalEducationService_GetPersonExternalEducationAsync_PersonByInstitutionType_SecondarySchool()
            {
                _InstitutionsAttendRepoMock.Setup(repo => repo.GetInstitutionsAttendAsync(offset, limit, It.IsAny<string>(), It.IsAny<string[]>(), It.IsAny<string>(), It.IsAny<string>(),
                   It.IsAny<InstType?>(), It.IsAny<bool>()))
                   .ReturnsAsync(new Tuple<IEnumerable<Domain.Base.Entities.InstitutionsAttend>, int>(_personExternalEducationEntityCollection, 4));


                PersonByInstitutionType filter = new PersonByInstitutionType()
                {
                    Person = new GuidObject2(Guid.NewGuid().ToString()),
                    Type = Dtos.EnumProperties.EducationalInstitutionType.SecondarySchool
                };
                var results = await _personExternalEducationService.GetPersonExternalEducationAsync(offset, limit, null, string.Empty, filter, true);
                Assert.AreEqual(results.Item2, 4);
            }

            [TestMethod]
            public async Task PersonExternalEducationService_GetPersonExternalEducationAsync_PersonByInstitutionType_PersonIdOnly()
            {
              
                PersonByInstitutionType filter = new PersonByInstitutionType()
                {
                    Person = new GuidObject2(Guid.NewGuid().ToString())
                };
                var results = await _personExternalEducationService.GetPersonExternalEducationAsync(offset, limit, null, string.Empty, filter, true);
                Assert.AreEqual(results.Item2, 0);
            }

            [TestMethod]
            public async Task PersonExternalEducationService_GetPersonExternalEducationAsync_PersonByInstitutionType_TypeOnly()
            {

                PersonByInstitutionType filter = new PersonByInstitutionType()
                {
                    Type = Dtos.EnumProperties.EducationalInstitutionType.PostSecondarySchool
                };
                var results = await _personExternalEducationService.GetPersonExternalEducationAsync(offset, limit, null, string.Empty, filter, true);
                Assert.AreEqual(results.Item2, 0);
            }

            [TestMethod]
            public async Task PersonExternalEducationService_GetPersonExternalEducationAsync_PersonByInstitutionType_PersonIsCorp()
            {
                _InstitutionsAttendRepoMock.Setup(repo => repo.GetInstitutionsAttendAsync(offset, limit, It.IsAny<string>(), It.IsAny<string[]>(), It.IsAny<string>(), It.IsAny<string>(),
                   It.IsAny<InstType?>(), It.IsAny<bool>()))
                   .ReturnsAsync(new Tuple<IEnumerable<Domain.Base.Entities.InstitutionsAttend>, int>(_personExternalEducationEntityCollection, 4));
                
                _personRepoMock.Setup(repo => repo.GetPersonIdForNonCorpOnly(It.IsAny<string>())).ReturnsAsync("");


                PersonByInstitutionType filter = new PersonByInstitutionType()
                {
                    Person = new GuidObject2(Guid.NewGuid().ToString()),
                    Type = Dtos.EnumProperties.EducationalInstitutionType.PostSecondarySchool
                };
                var results = await _personExternalEducationService.GetPersonExternalEducationAsync(offset, limit, null, string.Empty, filter, true);
                Assert.AreEqual(results.Item2, 4);
            }


            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task PersonExternalEducationService_GetPersonExternalEducationByGuidAsync_Empty()
            {
                _InstitutionsAttendRepoMock.Setup(repo => repo.GetInstitutionAttendByIdAsync(It.IsAny<string>()))
                    .Throws<KeyNotFoundException>();

                await _personExternalEducationService.GetPersonExternalEducationByGuidAsync("");
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task PersonExternalEducationService_GetPersonExternalEducationByGuidAsync_Null()
            {
                _InstitutionsAttendRepoMock.Setup(repo => repo.GetInstitutionAttendByIdAsync(It.IsAny<string>()))
                    .Throws<KeyNotFoundException>();

                await _personExternalEducationService.GetPersonExternalEducationByGuidAsync(null);
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task PersonExternalEducationService_GetPersonExternalEducationByGuidAsync_InvalidId()
            {
                _InstitutionsAttendRepoMock.Setup(repo => repo.GetInstitutionAttendByIdAsync(It.IsAny<string>()))
                    .Throws<KeyNotFoundException>();

                await _personExternalEducationService.GetPersonExternalEducationByGuidAsync("99");
            }

            [TestMethod]
            public async Task PersonExternalEducationService_GetPersonExternalEducationByGuidAsync_Expected()
            {
                var expectedResults =
                    _personExternalEducationDtoCollection.First(c => c.Id == personExternalEducationGuid);
                var actualResult =
                    await _personExternalEducationService.GetPersonExternalEducationByGuidAsync(personExternalEducationGuid);
                Assert.AreEqual(expectedResults.Id, actualResult.Id);
                Assert.AreEqual(expectedResults.Person.Id, actualResult.Person.Id);
                Assert.AreEqual(expectedResults.Institution.Id, actualResult.Institution.Id);
                Assert.AreEqual(expectedResults.ClassSize, actualResult.ClassSize);
                Assert.AreEqual(expectedResults.ClassRank, actualResult.ClassRank);
                Assert.AreEqual(expectedResults.ClassPercentile, actualResult.ClassPercentile);
                Assert.AreEqual(expectedResults.AttendancePeriods.Count(), actualResult.AttendancePeriods.Count());
            }

            [TestMethod]
            public async Task PersonExternalEducationService_PostPersonExternalEducationAsync_Expected()
            {
                var expectedResults = _personExternalEducationDtoCollection.FirstOrDefault(c => c.Id == personExternalEducationGuid);
                expectedResults.Id = Guid.Empty.ToString();
                var actualResult =
                    await _personExternalEducationService.CreateUpdatePersonExternalEducationAsync(expectedResults, false);

                Assert.AreEqual(expectedResults.Person.Id, actualResult.Person.Id);
                Assert.AreEqual(expectedResults.Institution.Id, actualResult.Institution.Id);
                Assert.AreEqual(expectedResults.ClassSize, actualResult.ClassSize);
                Assert.AreEqual(expectedResults.ClassRank, actualResult.ClassRank);
                Assert.AreEqual(expectedResults.ClassPercentile, actualResult.ClassPercentile);
                Assert.AreEqual(expectedResults.AttendancePeriods.Count(), actualResult.AttendancePeriods.Count());
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task PersonExternalEducationService_PostPersonExternalEducationAsync_InvalidPersonId()
            {
                _personRepoMock.Setup(repo => repo.GetPersonIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync(string.Empty);
                var expectedResults = _personExternalEducationDtoCollection.FirstOrDefault(c => c.Id == personExternalEducationGuid);
                expectedResults.Id = Guid.Empty.ToString();
                try
                {
                    var actualResult =
                        await _personExternalEducationService.CreateUpdatePersonExternalEducationAsync(expectedResults, false);
                }
                catch (IntegrationApiException ex)
                {
                    Assert.AreEqual(string.Format("No person was found for guid '{0}'", expectedResults.Person.Id), ex.Errors[0].Message);
                    throw ex;
                }
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task PersonExternalEducationService_PostPersonExternalEducationAsync_InvalidInstitutionId()
            {
                _institutionsRepoMock.Setup(repo => repo.GetInstitutionFromGuidAsync(It.IsAny<string>())).ReturnsAsync(string.Empty);
                var expectedResults = _personExternalEducationDtoCollection.FirstOrDefault(c => c.Id == personExternalEducationGuid);
                expectedResults.Id = Guid.Empty.ToString();
                try
                {
                    var actualResult =
                        await _personExternalEducationService.CreateUpdatePersonExternalEducationAsync(expectedResults, false);
                }
                catch (IntegrationApiException ex)
                {
                    Assert.AreEqual(string.Format("No external institution was found for guid '{0}'", expectedResults.Institution.Id), ex.Errors[0].Message);
                    throw ex;
                }
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task PersonExternalEducationService_PostPersonExternalEducationAsync_RepositoryError()
            {
                _InstitutionsAttendRepoMock.Setup(repo => repo.UpdateExternalEducationAsync(It.IsAny<Domain.Base.Entities.InstitutionsAttend>()))
                    .ThrowsAsync(new RepositoryException());

                var expectedResults = _personExternalEducationDtoCollection.FirstOrDefault(c => c.Id == personExternalEducationGuid);
                expectedResults.Id = Guid.Empty.ToString();
                try
                {
                    var actualResult =
                        await _personExternalEducationService.CreateUpdatePersonExternalEducationAsync(expectedResults, false);
                }
                catch (IntegrationApiException ex)
                {
                    Assert.AreEqual("Repository exception", ex.Errors[0].Message);
                    throw ex;
                }
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task PersonExternalEducationService_PostPersonExternalEducationAsync_InvalidClassData()
            {
                _institutionsRepoMock.Setup(repo => repo.GetInstitutionAsync(It.IsAny<string>())).ReturnsAsync(new Domain.Base.Entities.Institution("11", Domain.Base.Entities.InstType.College));
                var expectedResults = _personExternalEducationDtoCollection.FirstOrDefault(c => c.Id == personExternalEducationGuid);
                expectedResults.Id = Guid.Empty.ToString();
                try
                {
                    var actualResult =
                        await _personExternalEducationService.CreateUpdatePersonExternalEducationAsync(expectedResults, false);
                }
                catch (IntegrationApiException ex)
                {
                    Assert.AreEqual("Class Size, Class Rank, and Class Percentile are maintained at the person-external-education-credentials level for Colleges.", ex.Errors[0].Message);
                    throw ex;
                }
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task PersonExternalEducationService_PostPersonExternalEducationAsync_NullClassRank()
            {
                var expectedResults = _personExternalEducationDtoCollection.FirstOrDefault(c => c.Id == personExternalEducationGuid);
                expectedResults.Id = Guid.Empty.ToString();
                expectedResults.ClassRank = null;
                try
                {
                    var actualResult =
                        await _personExternalEducationService.CreateUpdatePersonExternalEducationAsync(expectedResults, false);
                }
                catch (IntegrationApiException ex)
                {
                    Assert.AreEqual("Class Rank is required when Class Size is provided.", ex.Errors[0].Message);
                    throw ex;
                }
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task PersonExternalEducationService_PostPersonExternalEducationAsync_NullClassSize()
            {
                var expectedResults = _personExternalEducationDtoCollection.FirstOrDefault(c => c.Id == personExternalEducationGuid);
                expectedResults.Id = Guid.Empty.ToString();
                expectedResults.ClassSize = null;
                try
                {
                    var actualResult =
                        await _personExternalEducationService.CreateUpdatePersonExternalEducationAsync(expectedResults, false);
                }
                catch (IntegrationApiException ex)
                {
                    Assert.AreEqual("Class Size is required when Class Rank is provided.", ex.Errors[0].Message);
                    throw ex;
                }
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task PersonExternalEducationService_PostPersonExternalEducationAsync_InvalidClassRank()
            {
                var expectedResults = _personExternalEducationDtoCollection.FirstOrDefault(c => c.Id == personExternalEducationGuid);
                expectedResults.Id = Guid.Empty.ToString();
                expectedResults.ClassRank = 10000000;
                try
                {
                    var actualResult =
                        await _personExternalEducationService.CreateUpdatePersonExternalEducationAsync(expectedResults, false);
                }
                catch (IntegrationApiException ex)
                {
                    Assert.AreEqual("The Class Rank is not a valid value.", ex.Errors.LastOrDefault().Message);
                    throw ex;
                }
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task PersonExternalEducationService_PostPersonExternalEducationAsync_InvalidClassSize()
            {
                var expectedResults = _personExternalEducationDtoCollection.FirstOrDefault(c => c.Id == personExternalEducationGuid);
                expectedResults.Id = Guid.Empty.ToString();
                expectedResults.ClassSize = 10000000;
                try
                {
                    var actualResult =
                        await _personExternalEducationService.CreateUpdatePersonExternalEducationAsync(expectedResults, false);
                }
                catch (IntegrationApiException ex)
                {
                    Assert.AreEqual("The Class Size is not a valid value.", ex.Errors.LastOrDefault().Message);
                    throw ex;
                }
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task PersonExternalEducationService_PostPersonExternalEducationAsync_InvalidClassPercentile()
            {
                var expectedResults = _personExternalEducationDtoCollection.FirstOrDefault(c => c.Id == personExternalEducationGuid);
                expectedResults.Id = Guid.Empty.ToString();
                expectedResults.ClassPercentile = 10000000;
                try
                {
                    var actualResult =
                        await _personExternalEducationService.CreateUpdatePersonExternalEducationAsync(expectedResults, false);
                }
                catch (IntegrationApiException ex)
                {
                    Assert.AreEqual("The Class Percentile is not a valid value.", ex.Errors.LastOrDefault().Message);
                    throw ex;
                }
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task PersonExternalEducationService_PostPersonExternalEducationAsync_InvalidTotalCredits()
            {
                var expectedResults = _personExternalEducationDtoCollection.FirstOrDefault(c => c.Id == personExternalEducationGuid);
                expectedResults.Id = Guid.Empty.ToString();
                expectedResults.TotalCredits = 10000.00m;
                try
                {
                    var actualResult =
                        await _personExternalEducationService.CreateUpdatePersonExternalEducationAsync(expectedResults, false);
                }
                catch (IntegrationApiException ex)
                {
                    Assert.AreEqual("The Total Credits is not a valid value.", ex.Errors.LastOrDefault().Message);
                    throw ex;
                }
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task PersonExternalEducationService_PostPersonExternalEducationAsync_InvalidPerformanceMeasure()
            {
                var expectedResults = _personExternalEducationDtoCollection.FirstOrDefault(c => c.Id == personExternalEducationGuid);
                expectedResults.Id = Guid.Empty.ToString();
                expectedResults.PerformanceMeasure = "abc";
                try
                {
                    var actualResult =
                        await _personExternalEducationService.CreateUpdatePersonExternalEducationAsync(expectedResults, false);
                }
                catch (IntegrationApiException ex)
                {
                    Assert.AreEqual(string.Format("Unable to convert performance measure to a decimal. Value: '{0}'", "abc"), ex.Errors[0].Message);
                    throw ex;
                }
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task PersonExternalEducationService_PostPersonExternalEducationAsync_InvalidGraduationSource()
            {
                var expectedResults = _personExternalEducationDtoCollection.FirstOrDefault(c => c.Id == personExternalEducationGuid);
                expectedResults.Id = Guid.Empty.ToString();
                expectedResults.GraduationDetails = new ExternalEducationGraduationDetails()
                {
                    Source = Dtos.EnumProperties.ExternalEducationGraduationDetailsSource.NotSet,
                    GraduatedOn = new DateTime(2016,6,24)
                };

                try
                {
                    var actualResult =
                        await _personExternalEducationService.CreateUpdatePersonExternalEducationAsync(expectedResults, false);
                }
                catch (IntegrationApiException ex)
                {
                    Assert.AreEqual("If providing a GraduationDetails object, the GraduationDetails.Source is a required property on the schema.", ex.Errors[0].Message);
                    throw ex;
                }
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task PersonExternalEducationService_PostPersonExternalEducationAsync_InvalidGraduatedOnDate()
            {
                var expectedResults = _personExternalEducationDtoCollection.FirstOrDefault(c => c.Id == personExternalEducationGuid);
                expectedResults.Id = Guid.Empty.ToString();
                expectedResults.GraduationDetails = new ExternalEducationGraduationDetails()
                {
                    Source = Dtos.EnumProperties.ExternalEducationGraduationDetailsSource.SecondarySchool
                };

                try
                {
                    var actualResult =
                        await _personExternalEducationService.CreateUpdatePersonExternalEducationAsync(expectedResults, false);
                }
                catch (IntegrationApiException ex)
                {
                    Assert.AreEqual("If providing a GraduationDetails object, the GraduationDetails.GraduatedOn is a required property on the schema.", ex.Errors[0].Message);
                    throw ex;
                }
            }
            
            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task PersonExternalEducationService_PostPersonExternalEducationAsync_InvalidStartOnDate()
            {
                var expectedResults = _personExternalEducationDtoCollection.FirstOrDefault(c => c.Id == personExternalEducationGuid);
                expectedResults.Id = Guid.Empty.ToString();
                expectedResults.AttendancePeriods = new List<ExternalEducationAttendancePeriods>()
                {
                    new ExternalEducationAttendancePeriods()
                    {
                        StartDate = new Dtos.DtoProperties.DateDtoProperty () { Year = 2010},
                        EndDate = new Dtos.DtoProperties.DateDtoProperty () { Day = 30, Month = 5, Year = 2014}
                    }
                };

                try
                {
                    var actualResult =
                        await _personExternalEducationService.CreateUpdatePersonExternalEducationAsync(expectedResults, false);
                }
                catch (IntegrationApiException ex)
                {
                    Assert.AreEqual("Cannot provide an end date when only a start year was provided.", ex.Errors[0].Message);
                    throw ex;
                }
            }
            
            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task PersonExternalEducationService_PostPersonExternalEducationAsync_InvalidEndOnDate()
            {
                var expectedResults = _personExternalEducationDtoCollection.FirstOrDefault(c => c.Id == personExternalEducationGuid);
                expectedResults.Id = Guid.Empty.ToString();
                expectedResults.AttendancePeriods = new List<ExternalEducationAttendancePeriods>()
                {
                    new ExternalEducationAttendancePeriods()
                    {
                        StartDate = new Dtos.DtoProperties.DateDtoProperty () { Day = 10, Month = 10, Year = 2010},
                        EndDate = new Dtos.DtoProperties.DateDtoProperty () { Year = 2014}
                    }
                };

                try
                {
                    var actualResult =
                        await _personExternalEducationService.CreateUpdatePersonExternalEducationAsync(expectedResults, false);
                }
                catch (IntegrationApiException ex)
                {
                    Assert.AreEqual("Cannot provide only an end year when a start date was provided.", ex.Errors[0].Message);
                    throw ex;
                }
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task PersonExternalEducationService_PostPersonExternalEducationAsync_StartYearGTEndYear()
            {
                var expectedResults = _personExternalEducationDtoCollection.FirstOrDefault(c => c.Id == personExternalEducationGuid);
                expectedResults.Id = Guid.Empty.ToString();
                expectedResults.AttendancePeriods = new List<ExternalEducationAttendancePeriods>()
                {
                    new ExternalEducationAttendancePeriods()
                    {
                        StartDate = new Dtos.DtoProperties.DateDtoProperty () { Year = 2016},
                        EndDate = new Dtos.DtoProperties.DateDtoProperty () { Year = 2014}
                    }
                };

                try
                {
                    var actualResult =
                        await _personExternalEducationService.CreateUpdatePersonExternalEducationAsync(expectedResults, false);
                }
                catch (IntegrationApiException ex)
                {
                    Assert.AreEqual("The Start Year cannot be greater than the End Year.", ex.Errors[0].Message);
                    throw ex;
                }
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task PersonExternalEducationService_PostPersonExternalEducationAsync_StartDateGTEndDate()
            {
                var expectedResults = _personExternalEducationDtoCollection.FirstOrDefault(c => c.Id == personExternalEducationGuid);
                expectedResults.Id = Guid.Empty.ToString();
                expectedResults.AttendancePeriods = new List<ExternalEducationAttendancePeriods>()
                {
                    new ExternalEducationAttendancePeriods()
                    {
                        StartDate = new Dtos.DtoProperties.DateDtoProperty () { Day = 10, Month= 10, Year = 2016},
                        EndDate = new Dtos.DtoProperties.DateDtoProperty () { Day = 30, Month = 5, Year = 2014}
                    }
                };

                try
                {
                    var actualResult =
                        await _personExternalEducationService.CreateUpdatePersonExternalEducationAsync(expectedResults, false);
                }
                catch (IntegrationApiException ex)
                {
                    Assert.AreEqual("The Start Date cannot be greater than the End Date.", ex.Errors.LastOrDefault().Message);
                    throw ex;
                }
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task PersonExternalEducationService_PostPersonExternalEducationAsync_MissingStartOnDate()
            {
                var expectedResults = _personExternalEducationDtoCollection.FirstOrDefault(c => c.Id == personExternalEducationGuid);
                expectedResults.Id = Guid.Empty.ToString();
                expectedResults.AttendancePeriods = new List<ExternalEducationAttendancePeriods>()
                {
                    new ExternalEducationAttendancePeriods()
                    {
                        EndDate = new Dtos.DtoProperties.DateDtoProperty () { Day = 30, Month = 5, Year = 2014}
                    }
                };

                try
                {
                    var actualResult =
                        await _personExternalEducationService.CreateUpdatePersonExternalEducationAsync(expectedResults, false);
                }
                catch (IntegrationApiException ex)
                {
                    Assert.AreEqual("Start On is required when End On is included in the payload.", ex.Errors.LastOrDefault().Message);
                    throw ex;
                }
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task PersonExternalEducationService_PostPersonExternalEducationAsync_InvalidStartOnMonth()
            {
                var expectedResults = _personExternalEducationDtoCollection.FirstOrDefault(c => c.Id == personExternalEducationGuid);
                expectedResults.Id = Guid.Empty.ToString();
                expectedResults.AttendancePeriods = new List<ExternalEducationAttendancePeriods>()
                {
                    new ExternalEducationAttendancePeriods()
                    {
                        StartDate = new Dtos.DtoProperties.DateDtoProperty () { Day = 10, Month= 15, Year = 2010 },
                        EndDate = new Dtos.DtoProperties.DateDtoProperty () { Day = 30, Month = 5, Year = 2014 }
                    }
                };

                try
                {
                    var actualResult =
                        await _personExternalEducationService.CreateUpdatePersonExternalEducationAsync(expectedResults, false);
                }
                catch (IntegrationApiException ex)
                {
                    Assert.AreEqual("The specified Start Date is not a valid value.", ex.Errors.LastOrDefault().Message);
                    throw ex;
                }
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task PersonExternalEducationService_PostPersonExternalEducationAsync_InvalidEndOnMonth()
            {
                var expectedResults = _personExternalEducationDtoCollection.FirstOrDefault(c => c.Id == personExternalEducationGuid);
                expectedResults.Id = Guid.Empty.ToString();
                expectedResults.AttendancePeriods = new List<ExternalEducationAttendancePeriods>()
                {
                    new ExternalEducationAttendancePeriods()
                    {
                        StartDate = new Dtos.DtoProperties.DateDtoProperty () { Day = 10, Month= 10, Year = 2010 },
                        EndDate = new Dtos.DtoProperties.DateDtoProperty () { Day = 30, Month = 15, Year = 2014 }
                    }
                };

                try
                {
                    var actualResult =
                        await _personExternalEducationService.CreateUpdatePersonExternalEducationAsync(expectedResults, false);
                }
                catch (IntegrationApiException ex)
                {
                    Assert.AreEqual("The specified End Date is not a valid value.", ex.Errors.LastOrDefault().Message);
                    throw ex;
                }
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task PersonExternalEducationService_PostPersonExternalEducationAsync_DuplicateStartOnDate()
            {
                var expectedResults = _personExternalEducationDtoCollection.FirstOrDefault(c => c.Id == personExternalEducationGuid);
                expectedResults.Id = Guid.Empty.ToString();
                expectedResults.AttendancePeriods = new List<ExternalEducationAttendancePeriods>()
                {
                    new ExternalEducationAttendancePeriods()
                    {
                        StartDate = new Dtos.DtoProperties.DateDtoProperty () { Day = 10, Month= 10, Year = 2010 },
                        EndDate = new Dtos.DtoProperties.DateDtoProperty () { Day = 30, Month = 5, Year = 2014 }
                    },
                    new ExternalEducationAttendancePeriods()
                    {
                        StartDate = new Dtos.DtoProperties.DateDtoProperty () { Day = 10, Month= 10, Year = 2010 },
                        EndDate = new Dtos.DtoProperties.DateDtoProperty () { Day = 30, Month = 5, Year = 2014 }
                    }
                };

                try
                {
                    var actualResult =
                        await _personExternalEducationService.CreateUpdatePersonExternalEducationAsync(expectedResults, false);
                }
                catch (IntegrationApiException ex)
                {
                    Assert.AreEqual("A duplicate Start On is not allowed.", ex.Errors.LastOrDefault().Message);
                    throw ex;
                }
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task PersonExternalEducationService_PostPersonExternalEducationAsync_DuplicateStartOnYear()
            {
                var expectedResults = _personExternalEducationDtoCollection.FirstOrDefault(c => c.Id == personExternalEducationGuid);
                expectedResults.Id = Guid.Empty.ToString();
                expectedResults.AttendancePeriods = new List<ExternalEducationAttendancePeriods>()
                {
                    new ExternalEducationAttendancePeriods()
                    {
                        StartDate = new Dtos.DtoProperties.DateDtoProperty () { Day = 08, Month= 08, Year = 2010 },
                        EndDate = new Dtos.DtoProperties.DateDtoProperty () { Day = 30, Month = 5, Year = 2016 }
                    },
                    new ExternalEducationAttendancePeriods()
                    {
                        StartDate = new Dtos.DtoProperties.DateDtoProperty () { Day = 10, Month= 10, Year = 2010 },
                        EndDate = new Dtos.DtoProperties.DateDtoProperty () { Day = 30, Month = 5, Year = 2014 }
                    }
                };

                try
                {
                    var actualResult =
                        await _personExternalEducationService.CreateUpdatePersonExternalEducationAsync(expectedResults, false);
                }
                catch (IntegrationApiException ex)
                {
                    Assert.AreEqual("A duplicate Start On is not allowed.", ex.Errors.LastOrDefault().Message);
                    throw ex;
                }
            }

            [TestMethod]
            public async Task PersonExternalEducationService_PutPersonExternalEducationAsync_Expected()
            {
                var expectedResults = _personExternalEducationDtoCollection.FirstOrDefault(c => c.Id == personExternalEducationGuid);
                var actualResult =
                    await _personExternalEducationService.CreateUpdatePersonExternalEducationAsync(expectedResults, true);

                Assert.AreEqual(expectedResults.Person.Id, actualResult.Person.Id);
                Assert.AreEqual(expectedResults.Institution.Id, actualResult.Institution.Id);
                Assert.AreEqual(expectedResults.ClassSize, actualResult.ClassSize);
                Assert.AreEqual(expectedResults.ClassRank, actualResult.ClassRank);
                Assert.AreEqual(expectedResults.ClassPercentile, actualResult.ClassPercentile);
                Assert.AreEqual(expectedResults.AttendancePeriods.Count(), actualResult.AttendancePeriods.Count());
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task PersonExternalEducationService_PutPersonExternalEducationAsync_InvalidInstitutionsAttendId()
            {
                _InstitutionsAttendRepoMock.Setup(repo => repo.GetInstitutionsAttendIdFromGuidAsync(It.IsAny<string>()))
                    .ReturnsAsync("abc");

                var expectedResults = _personExternalEducationDtoCollection.FirstOrDefault(c => c.Id == personExternalEducationGuid);
                expectedResults.Person = new GuidObject2(Guid.NewGuid().ToString());
                try
                {
                    var actualResult =
                        await _personExternalEducationService.CreateUpdatePersonExternalEducationAsync(expectedResults, true);
                }
                catch (IntegrationApiException ex)
                {
                    Assert.AreEqual("InstitutionAttend Record Key is not valid: abc", ex.Errors[0].Message);
                    throw ex;
                }
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task PersonExternalEducationService_PutPersonExternalEducationAsync_WrongInstitutionsAttendPersonId()
            {
                _InstitutionsAttendRepoMock.Setup(repo => repo.GetInstitutionsAttendIdFromGuidAsync(It.IsAny<string>()))
                    .ReturnsAsync("abc*123");

                var expectedResults = _personExternalEducationDtoCollection.FirstOrDefault(c => c.Id == personExternalEducationGuid);
                expectedResults.Person = new GuidObject2(Guid.NewGuid().ToString());
                try
                {
                    var actualResult =
                        await _personExternalEducationService.CreateUpdatePersonExternalEducationAsync(expectedResults, true);
                }
                catch (IntegrationApiException ex)
                {
                    Assert.AreEqual("Person Id from person guid is not associated with the record key: abc*123", ex.Errors[0].Message);
                    throw ex;
                }
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task PersonExternalEducationService_PutPersonExternalEducationAsync_WrongInstitutionsAttendInstitutionId()
            {
                _InstitutionsAttendRepoMock.Setup(repo => repo.GetInstitutionsAttendIdFromGuidAsync(It.IsAny<string>()))
                    .ReturnsAsync("2*123");

                var expectedResults = _personExternalEducationDtoCollection.FirstOrDefault(c => c.Id == personExternalEducationGuid);
                expectedResults.Person = new GuidObject2(Guid.NewGuid().ToString());
                try
                {
                    var actualResult =
                        await _personExternalEducationService.CreateUpdatePersonExternalEducationAsync(expectedResults, true);
                }
                catch (IntegrationApiException ex)
                {
                    Assert.AreEqual("Institution Id from person guid is not associated with the record key: 2*123", ex.Errors[0].Message);
                    throw ex;
                }
            }
        }
    }
}