//Copyright 2019 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Colleague.Domain.Base.Entities;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Colleague.Dtos;
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
    public class PersonMatchRequestsServiceTests
    {
        public abstract class CurrentUserSetup
        {
            protected Ellucian.Colleague.Domain.Entities.Role viewPersonMatchRequestRole = new Ellucian.Colleague.Domain.Entities.Role(1, "VIEW.PERSON.MATCH.REQUEST");
            protected Ellucian.Colleague.Domain.Entities.Role createUpdatePersonMatchRequestRole = new Ellucian.Colleague.Domain.Entities.Role(1, "CREATE.PERSON.MATCH.REQUEST.PROSPECTS");

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

            // Represents a third party system like Elevate
            public class ThirdPartyUserFactory : ICurrentUserFactory
            {
                public ICurrentUser CurrentUser
                {
                    get
                    {
                        return new CurrentUser(new Claims()
                        {
                            ControlId = "123",
                            Name = "ELEVATE",
                            PersonId = "ELEVATE",
                            SecurityToken = "321",
                            SessionTimeout = 30,
                            UserName = "ERPADAPTER",
                            Roles = new List<string>() { "VIEW.PERSON.MATCH.REQUEST", "CREATE.PERSON.MATCH.REQUEST.PROSPECTS" },
                            SessionFixationId = "abc123"
                        });
                    }
                }
            }
        }
        /// <summary>
        /// Tests the prospect-opportunities service layer
        /// </summary>
        [TestClass]
        public class PersonMatchingRequestsServiceTests
        {
            private Mock<IPersonMatchingRequestsRepository> _personMatchingRequestsRepositoryMock;
            private Mock<IPersonRepository> _personRepositoryMock;
            private Mock<IReferenceDataRepository> _referenceDataRepositoryMock;

            private Mock<IAdapterRegistry> _adapterRegistryMock;
            private Mock<IRoleRepository> _roleRepositoryMock;
            private Mock<IConfigurationRepository> _configurationRepoMock;
            private Mock<ILogger> _loggerMock;

            private IPersonMatchingRequestsRepository _personMatchingRequestsRepository;
            private IPersonRepository _personRepository;
            private IReferenceDataRepository _referenceDataRepository;

            private IAdapterRegistry _adapterRegistry;
            private IRoleRepository _roleRepository;
            private IConfigurationRepository _configurationRepository;
            private ILogger logger;
            private ICurrentUserFactory userFactory;

            private IPersonMatchingRequestsService _personMatchingRequestsService;

            private Domain.Entities.Role _roleView = new Domain.Entities.Role(1, "VIEW.PERSON.MATCH.REQUEST");
            private Domain.Entities.Role _roleUpdate = new Domain.Entities.Role(2, "CREATE.PERSON.MATCH.REQUEST.PROSPECTS");

            private IEnumerable<PersonMatchRequest> personMatchingRequestEntities;
            private Dictionary<string, string> personDict;
            private PersonMatchRequest personMatch1;
            private PersonMatchRequest personMatch2;
            private PersonMatchRequest personMatch3;

            private PersonMatchingRequestsInitiationsProspects personMatchInitiationsDto1;
            private PersonMatchingRequests personMatchDto1;

            private int offset = 0, limit = 100;

            [TestInitialize]
            public void Initialize()
            {

                _personMatchingRequestsRepositoryMock = new Mock<IPersonMatchingRequestsRepository>();
                _personMatchingRequestsRepository = _personMatchingRequestsRepositoryMock.Object;
                _personRepositoryMock = new Mock<IPersonRepository>();
                _personRepository = _personRepositoryMock.Object;
                _referenceDataRepositoryMock = new Mock<IReferenceDataRepository>();
                _referenceDataRepository = _referenceDataRepositoryMock.Object;
                _adapterRegistryMock = new Mock<IAdapterRegistry>();
                _adapterRegistry = _adapterRegistryMock.Object;
                _roleRepositoryMock = new Mock<IRoleRepository>();
                _roleRepository = _roleRepositoryMock.Object;
                _configurationRepoMock = new Mock<IConfigurationRepository>();
                _configurationRepository = _configurationRepoMock.Object;
                _loggerMock = new Mock<ILogger>();
                logger = _loggerMock.Object;

                userFactory = new CurrentUserSetup.ThirdPartyUserFactory();

                InitializeData();
                InitializeMocks();


                _personMatchingRequestsService = new PersonMatchingRequestsService(_personMatchingRequestsRepository, _personRepository,
                                                                               _referenceDataRepository, _adapterRegistry, userFactory, _roleRepository,
                                                                               _configurationRepository, logger);

            }

            private void InitializeMocks()
            {
                // prospect opportunity repo
                // Get all
                Tuple<IEnumerable<PersonMatchRequest>, int> tuple = new Tuple<IEnumerable<PersonMatchRequest>, int>(personMatchingRequestEntities, personMatchingRequestEntities.Count());
                _personMatchingRequestsRepositoryMock.Setup(pmr => pmr.GetPersonMatchRequestsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<PersonMatchRequest>(), It.IsAny<string[]>(), It.IsAny<bool>())).ReturnsAsync(tuple);

                // Get
                _personMatchingRequestsRepositoryMock.Setup(pmr => pmr.GetPersonMatchRequestsByIdAsync(personMatch1.Guid, It.IsAny<bool>())).ReturnsAsync(personMatch1);
                _personMatchingRequestsRepositoryMock.Setup(pmr => pmr.GetPersonMatchRequestsByIdAsync(personMatch2.Guid, It.IsAny<bool>())).ReturnsAsync(personMatch2);
                _personMatchingRequestsRepositoryMock.Setup(pmr => pmr.GetPersonMatchRequestsByIdAsync(personMatch3.Guid, It.IsAny<bool>())).ReturnsAsync(personMatch3);
                _personMatchingRequestsRepositoryMock.Setup(pmr => pmr.CreatePersonMatchingRequestsInitiationsProspectsAsync(It.IsAny<Domain.Base.Entities.PersonMatchRequestInitiation>())).ReturnsAsync(personMatch1);

                personDict = new Dictionary<string, string>();
                personDict.Add(personMatch1.PersonId, Guid.NewGuid().ToString());
                personDict.Add(personMatch2.PersonId, Guid.NewGuid().ToString());
                personDict.Add(personMatch3.PersonId, Guid.NewGuid().ToString());
                _personRepositoryMock.Setup(pr => pr.GetPersonGuidsCollectionAsync(It.IsAny<List<string>>())).ReturnsAsync(personDict);

                // reference data repo
                _referenceDataRepositoryMock.Setup(rd => rd.GetPersonIdsByPersonFilterGuidAsync(It.IsAny<string>())).ReturnsAsync(new string[] { personMatch1.PersonId, personMatch2.PersonId, personMatch3.PersonId });

                // role repo
                _roleView.AddPermission(new Domain.Entities.Permission("VIEW.PERSON.MATCH.REQUEST"));
                _roleUpdate.AddPermission(new Domain.Entities.Permission("UPDATE.PERSON.MATCH.REQUEST.PROSPECTS"));
                _roleRepositoryMock.Setup(i => i.GetRolesAsync()).ReturnsAsync(new List<Domain.Entities.Role>() { _roleView });
            }

            private void InitializeData()
            {
                // For clarity:
                // Dtos.PersonMatchingRequests
                // Domain.Base.Entities.PersonMatchRequest 

                personMatch1 = new PersonMatchRequest()
                {
                    Guid = "6518d26e-ab8d-4aa0-95f0-f415fa7c0001",
                    RecordKey = "1",
                    PersonId = "1",
                    Originator = "ELEVATE"
                };
                personMatch1.AddPersonMatchRequestOutcomes(new PersonMatchRequestOutcomes(
                    PersonMatchRequestType.Initial,
                    PersonMatchRequestStatus.ExistingPerson,
                    new DateTimeOffset(new DateTime(2019, 11, 11)))
                );
                personMatch2 = new PersonMatchRequest()
                {
                    Guid = "6518d26e-ab8d-4aa0-95f0-f415fa7c0002",
                    RecordKey = "2",
                    PersonId = "2",
                    Originator = "ELEVATE"
                };
                personMatch2.AddPersonMatchRequestOutcomes(new PersonMatchRequestOutcomes(
                    PersonMatchRequestType.Initial,
                    PersonMatchRequestStatus.NewPerson,
                    new DateTimeOffset(new DateTime(2019, 10, 11)))
                );
                personMatch3 = new PersonMatchRequest()
                {
                    Guid = "6518d26e-ab8d-4aa0-95f0-f415fa7c0003",
                    RecordKey = "3",
                    PersonId = "3",
                    Originator = "ELEVATE"
                };
                personMatch3.AddPersonMatchRequestOutcomes(new PersonMatchRequestOutcomes(
                    PersonMatchRequestType.Initial,
                    PersonMatchRequestStatus.ReviewRequired,
                    new DateTimeOffset(new DateTime(2019, 9, 11)))
                );
                personMatch3.AddPersonMatchRequestOutcomes(new PersonMatchRequestOutcomes(
                    PersonMatchRequestType.Final,
                    PersonMatchRequestStatus.NewPerson,
                    new DateTimeOffset(new DateTime(2019, 9, 12)))
                );
                personMatchingRequestEntities = new List<PersonMatchRequest>() { personMatch1, personMatch2, personMatch3 };

                personMatchInitiationsDto1 = new PersonMatchingRequestsInitiationsProspects()
                {
                    Id = "00000000-0000-0000-0000-000000000000",
                    Names = new Dtos.DtoProperties.PersonMatchingRequestNamesDtoProperty()
                    {
                        Legal = new Dtos.DtoProperties.PersonMatchingRequestNamesNameDtoProperty()
                        {
                            LastName = "Carver",
                            FirstName = "John"
                        }
                    },
                    Gender = Dtos.EnumProperties.GenderType2.Male,
                    MatchingCriteria = new PersonMatchingRequestsInitiationsMatchingCriteria()
                    {
                        DateOfBirth = new DateTime(2001, 12, 1)
                    }
                };

                personMatchDto1 = new PersonMatchingRequests()
                {
                    Id = "6518d26e-ab8d-4aa0-95f0-f415fa7c0001",
                    Originator = "ELEVATE",
                    Person = new GuidObject2(Guid.NewGuid().ToString()),
                    Outcomes = new List<PersonMatchingRequestsOutcomesDtoProperty>()
                    {
                        new PersonMatchingRequestsOutcomesDtoProperty()
                        {
                            Type = Dtos.EnumProperties.PersonMatchingRequestsType.Initial,
                            Status = Dtos.EnumProperties.PersonMatchingRequestsStatus.ExistingPerson,
                            Date = new DateTime(2019, 11, 11)
                        }
                    }
                };
            }

            [TestCleanup]
            public void Cleanup()
            {
                _personMatchingRequestsRepositoryMock = null;
                _personMatchingRequestsRepository = null;
                _personRepositoryMock = null;
                _personRepository = null;
                _referenceDataRepositoryMock = null;
                _referenceDataRepository = null;
                _adapterRegistryMock = null;
                _adapterRegistry = null;
                _roleRepositoryMock = null;
                _roleRepository = null;
                _configurationRepoMock = null;
                _configurationRepository = null;
                _loggerMock = null;
                logger = null;
                userFactory = null;

            }

            [TestMethod]
            public async Task PersonMatchingRequestsService_GetPersonMatchingRequestsAsync()
            {
                var results = await _personMatchingRequestsService.GetPersonMatchingRequestsAsync(offset, limit, null, null, true);
                Assert.IsTrue(results is Tuple<IEnumerable<PersonMatchingRequests>, int>);
                Assert.IsNotNull(results);
            }

            [TestMethod]
            public async Task PersonMatchingRequestsService_GetPersonMatchingRequestsAsync_Count()
            {
                var results = await _personMatchingRequestsService.GetPersonMatchingRequestsAsync(offset, limit, null, null, true);
                Assert.AreEqual(3, results.Item2);
            }

            [TestMethod]
            public async Task PersonMatchingRequestsService_GetPersonMatchingRequestsAsync_Properties()
            {
                var result = await _personMatchingRequestsService.GetPersonMatchingRequestsAsync(offset, limit, null, null, true);

                foreach (var actual in result.Item1)
                {
                    var expected = personMatchingRequestEntities.FirstOrDefault(pmr => pmr.Guid == actual.Id);
                    Assert.IsNotNull(expected);
                    Assert.AreEqual(expected.Guid, actual.Id);
                    Assert.AreEqual(expected.Originator, actual.Originator);
                    if (actual.Outcomes != null && actual.Outcomes.Any())
                    {
                        Assert.AreEqual(expected.Outcomes.Count, actual.Outcomes.Count);
                        var expectedOutcome = expected.Outcomes.ElementAt(0);
                        var actualOutcome = actual.Outcomes.ElementAt(0);
                        Assert.AreEqual(expectedOutcome.Type.ToString().ToLower(), actualOutcome.Type.ToString().ToLower());
                        Assert.AreEqual(expectedOutcome.Status.ToString().ToLower(), actualOutcome.Status.ToString().ToLower());
                        Assert.AreEqual(expectedOutcome.Date, actualOutcome.Date);
                    }
                    if (actual.Person != null && !string.IsNullOrEmpty(actual.Person.Id))
                    {
                        string personGuid = "";
                        personDict.TryGetValue(expected.PersonId, out personGuid);
                        Assert.AreEqual(personGuid, actual.Person.Id);
                    }
                }

            }

            [TestMethod]
            public async Task PersonMatchingRequestsService_GetPersonMatchingRequestsAsync_PersonFilter()
            {
                var results = await _personMatchingRequestsService.GetPersonMatchingRequestsAsync(offset, limit, null, Guid.NewGuid().ToString(), true);
                Assert.AreEqual(3, results.Item2);
            }

            [TestMethod]
            public async Task PersonMatchingRequestsService_GetPersonMatchingRequestsAsync_Originator()
            {
                var criteria = new PersonMatchingRequests() { Originator = "ELEVATE" };
                var results = await _personMatchingRequestsService.GetPersonMatchingRequestsAsync(offset, limit, criteria, null, true);
                Assert.AreEqual(3, results.Item2);
            }

            [TestMethod]
            public async Task PersonMatchingRequestsService_GetPersonMatchingRequestsAsync_Outcomes()
            {
                var criteria = new PersonMatchingRequests()
                {
                    Outcomes = new List<PersonMatchingRequestsOutcomesDtoProperty>()
                    {
                        new PersonMatchingRequestsOutcomesDtoProperty()
                        {
                            Type = Dtos.EnumProperties.PersonMatchingRequestsType.Initial,
                            Status = Dtos.EnumProperties.PersonMatchingRequestsStatus.ExistingPerson
                        }
                    }
                };
                var results = await _personMatchingRequestsService.GetPersonMatchingRequestsAsync(offset, limit, criteria, null, true);
                Assert.AreEqual(3, results.Item2);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task PersonMatchingRequestsService_GetPersonMatchingRequestsByGuidAsync_Empty()
            {
                await _personMatchingRequestsService.GetPersonMatchingRequestsByGuidAsync("");
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task PersonMatchingRequestsService_GetPersonMatchingRequestsByGuidAsync_Null()
            {
                await _personMatchingRequestsService.GetPersonMatchingRequestsByGuidAsync(null);
            }

            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public async Task PersonMatcingRequestService_GetPersonMatchRequestsAsync_PermissionsException()
            {
                _roleRepositoryMock.Setup(i => i.GetRolesAsync()).ReturnsAsync(new List<Domain.Entities.Role>() { });
                await _personMatchingRequestsService.GetPersonMatchingRequestsAsync(0, 100, null, null);
            }

            #region person-matching-requests-initiations-prospects

            // POST
            [TestMethod]
            public async Task PersonMatchingRequestsService_CreatePersonMatchingRequestsInitiationsProspectsAsync()
            {
                _roleUpdate.AddPermission(new Domain.Entities.Permission("CREATE.PERSON.MATCH.REQUEST.PROSPECTS"));
                _roleRepositoryMock.Setup(i => i.GetRolesAsync()).ReturnsAsync(new List<Domain.Entities.Role>() { _roleUpdate });
                var actual = await _personMatchingRequestsService.CreatePersonMatchingRequestsInitiationsProspectsAsync(personMatchInitiationsDto1);
                var expected = personMatchDto1;

                Assert.IsNotNull(actual);
                Assert.AreEqual(expected.Id, actual.Id, "Guid");
            }

            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public async Task PersonMatchingRequestsService_CreatePersonMatchingRequestsInitiationsProspectsAsync_NoPermissions()
            {
                var actual = await _personMatchingRequestsService.CreatePersonMatchingRequestsInitiationsProspectsAsync(personMatchInitiationsDto1);
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task PersonMatchingRequestsService_CreatePersonMatchingRequestsInitiationsProspectsAsync_Validation_Null_Dto()
            {
                var actual = await _personMatchingRequestsService.CreatePersonMatchingRequestsInitiationsProspectsAsync(null);
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task PersonMatchingRequestsService_CreatePersonMatchingRequestsInitiationsProspectsAsync_Validation_Empty_DtoId()
            {
                personMatchInitiationsDto1.Id = " ";
                var actual = await _personMatchingRequestsService.CreatePersonMatchingRequestsInitiationsProspectsAsync(personMatchInitiationsDto1);
            }

            #endregion
        }
    }
}
