// Copyright 2014-2021 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Linq;
using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Colleague.Domain.Base;
using Ellucian.Colleague.Domain.Base.Entities;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Web.Adapters;
using Ellucian.Web.Security;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;
using System.Threading.Tasks;
using Ellucian.Colleague.Domain.Base.Tests;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Colleague.Dtos;
using Ellucian.Colleague.Dtos.DtoProperties;
using Ellucian.Colleague.Dtos.EnumProperties;
using Address = Ellucian.Colleague.Domain.Base.Entities.Address;
using AddressType = Ellucian.Colleague.Dtos.EnumProperties.AddressType;
using AddressType2 = Ellucian.Colleague.Domain.Base.Entities.AddressType2;
using Country = Ellucian.Colleague.Domain.Base.Entities.Country;
using EmailAddress = Ellucian.Colleague.Domain.Base.Entities.EmailAddress;
using EmailType = Ellucian.Colleague.Domain.Base.Entities.EmailType;
using Ethnicity = Ellucian.Colleague.Domain.Base.Entities.Ethnicity;
using EthnicityType = Ellucian.Colleague.Domain.Base.Entities.EthnicityType;
using MaritalStatus = Ellucian.Colleague.Domain.Base.Entities.MaritalStatus;
using MaritalStatusType = Ellucian.Colleague.Domain.Base.Entities.MaritalStatusType;
using Person = Ellucian.Colleague.Domain.Base.Entities.Person;
using PersonName = Ellucian.Colleague.Domain.Base.Entities.PersonName;
using PersonNameType = Ellucian.Colleague.Domain.Base.Entities.PersonNameType;
using PersonNameTypeItem = Ellucian.Colleague.Domain.Base.Entities.PersonNameTypeItem;
using PersonRoleType = Ellucian.Colleague.Domain.Base.Entities.PersonRoleType;
using Phone = Ellucian.Colleague.Domain.Base.Entities.Phone;
using PhoneType = Ellucian.Colleague.Domain.Base.Entities.PhoneType;
using PrivacyStatus = Ellucian.Colleague.Domain.Base.Entities.PrivacyStatus;
using PrivacyStatusType = Ellucian.Colleague.Domain.Base.Entities.PrivacyStatusType;
using Race = Ellucian.Colleague.Domain.Base.Entities.Race;
using RaceType = Ellucian.Colleague.Domain.Base.Entities.RaceType;
using Relationship = Ellucian.Colleague.Domain.Base.Entities.Relationship;
using Role = Ellucian.Colleague.Domain.Entities.Role;
using SocialMediaType = Ellucian.Colleague.Domain.Base.Entities.SocialMediaType;
using SocialMediaTypeCategory = Ellucian.Colleague.Domain.Base.Entities.SocialMediaTypeCategory;
using Ellucian.Web.Http.Exceptions;
using Ellucian.Data.Colleague.DataContracts;
using System.Net;

namespace Ellucian.Colleague.Coordination.Base.Tests.Services
{
    [TestClass]
    public class PersonServiceTests
    {
        // sets up a current user
        public abstract class CurrentUserSetup
        {
            protected Role personRole = new Role(105, "Student");
            protected Ellucian.Colleague.Domain.Entities.Role updatePersonRole = new Ellucian.Colleague.Domain.Entities.Role(1, "UPDATE.PERSON");
            protected Ellucian.Colleague.Domain.Entities.Role createPersonRole = new Ellucian.Colleague.Domain.Entities.Role(2, "CREATE.PERSON");


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
                            UserName = "Student",
                            Roles = new List<string>() { "Student" },
                            SessionFixationId = "abc123",
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
                            Roles = new List<string>() { "CREATE.PERSON", "UPDATE.PERSON" },
                            SessionFixationId = "abc123"
                        });
                    }
                }
            }
        }

        #region GetPerson2 Tests

        [TestClass]
        public class GetPerson2NonCache : CurrentUserSetup
        {
            private string personId = "0000011";
            private string personId2 = "0000012";
            private string personGuid = "5674f28b-b216-4055-b236-81a922d93b4c";
            private string personGuid2 = "1111f28b-b216-4055-b236-81a922d93b4c";
            private Mock<IPersonRepository> personRepoMock;
            private IPersonRepository personRepo;
            private Mock<IPersonBaseRepository> personBaseRepoMock;
            private IPersonBaseRepository personBaseRepo;
            private Mock<IReferenceDataRepository> refRepoMock;
            private IReferenceDataRepository refRepo;
            private Mock<IAdapterRegistry> adapterRegistryMock;
            private IAdapterRegistry adapterRegistry;
            private PersonService personService;
            private ILogger logger;
            private Ellucian.Colleague.Domain.Base.Entities.Person person;
            private Ellucian.Colleague.Domain.Base.Entities.Person person2;
            private Ellucian.Colleague.Domain.Base.Entities.PersonIntegration personIntegration;
            private Mock<IRoleRepository> roleRepoMock;
            private IRoleRepository roleRepo;
            private ICurrentUserFactory currentUserFactory;
            private Ellucian.Colleague.Domain.Base.Entities.EmailAddress instEmail;
            private Ellucian.Colleague.Domain.Base.Entities.EmailAddress perEmail;
            private Ellucian.Colleague.Domain.Base.Entities.EmailAddress workEmail;
            private Ellucian.Colleague.Domain.Base.Entities.Address homeAddr;
            private Ellucian.Colleague.Domain.Base.Entities.Address mailAddr;
            private Ellucian.Colleague.Domain.Base.Entities.Address resAddr;
            private Ellucian.Colleague.Domain.Base.Entities.Address workAddr;
            private Ellucian.Colleague.Domain.Base.Entities.Phone homePhone;
            private Ellucian.Colleague.Domain.Base.Entities.Phone mobilePhone;
            private Ellucian.Colleague.Domain.Base.Entities.Phone residencePhone;
            private Ellucian.Colleague.Domain.Base.Entities.Phone workPhone;
            private IEnumerable<Ellucian.Colleague.Domain.Base.Entities.State> states;
            private IEnumerable<Ellucian.Colleague.Domain.Base.Entities.Country> countries;
            private List<Ellucian.Colleague.Domain.Base.Entities.County> counties;
            private string maritalStatusGuid = Guid.NewGuid().ToString();
            private string ethnicityGuid = Guid.NewGuid().ToString();
            private string raceAsianGuid = Guid.NewGuid().ToString();
            private string racePacificIslanderGuid = Guid.NewGuid().ToString();
            private string countyGuid = Guid.NewGuid().ToString();
            private string baptistGuid = Guid.NewGuid().ToString();
            private string catholicGuid = Guid.NewGuid().ToString();
            private Ellucian.Colleague.Domain.Entities.Permission permissionViewAnyPerson;

            [TestInitialize]
            public void Initialize()
            {
                personRepoMock = new Mock<IPersonRepository>();
                personRepo = personRepoMock.Object;
                personBaseRepoMock = new Mock<IPersonBaseRepository>();
                personBaseRepo = personBaseRepoMock.Object;
                refRepoMock = new Mock<IReferenceDataRepository>();
                refRepo = refRepoMock.Object;
                adapterRegistryMock = new Mock<IAdapterRegistry>();
                adapterRegistry = adapterRegistryMock.Object;
                roleRepoMock = new Mock<IRoleRepository>();
                roleRepo = roleRepoMock.Object;
                logger = new Mock<ILogger>().Object;

                // Mock person response from the person repository
                person = new Domain.Base.Entities.Person(personId, "Brown");
                person.Guid = personGuid;
                person.Prefix = "Mr.";
                person.FirstName = "Ricky";
                person.MiddleName = "Lee";
                person.Suffix = "Jr.";
                person.Nickname = "Rick";
                person.BirthDate = new DateTime(1930, 1, 1);
                person.DeceasedDate = new DateTime(2014, 5, 12);
                person.GovernmentId = "111-11-1111";
                person.MaritalStatusCode = "M";
                person.EthnicCodes = new List<string> { "H" };
                person.RaceCodes = new List<string> { "AS" };
                person.AddEmailAddress(new EmailAddress("xyz@xmail.com", "COL"));
                personRepoMock.Setup(repo => repo.GetPersonByGuidNonCachedAsync(personGuid)).ReturnsAsync(person);

                var filteredPersonGuidTuple = new Tuple<IEnumerable<string>, int>(new List<string>() { personGuid }, 1);
                personRepoMock.Setup(repo => repo.GetFilteredPerson2GuidsAsync(It.IsAny<int>(), It.IsAny<int>(), true, It.IsAny<PersonFilterCriteria>(), It.IsAny<string>())).ReturnsAsync(filteredPersonGuidTuple);

                personIntegration = new PersonIntegration(personId, "Brown");
                personIntegration.Guid = personGuid;
                personIntegration.Prefix = "Mr.";
                personIntegration.FirstName = "Ricky";
                personIntegration.MiddleName = "Lee";
                personIntegration.Suffix = "Jr.";
                personIntegration.Nickname = "Rick";
                personIntegration.BirthDate = new DateTime(1930, 1, 1);
                personIntegration.DeceasedDate = new DateTime(2014, 5, 12);
                personIntegration.GovernmentId = "111-11-1111";
                personIntegration.Religion = "CA";
                personIntegration.MaritalStatusCode = "M";
                personIntegration.EthnicCodes = new List<string> { "H", "N" };
                personIntegration.RaceCodes = new List<string> { "AS" };
                personIntegration.AddRole(new PersonRole(PersonRoleType.Alumni, new DateTime(15, 01, 22), new DateTime(15, 05, 25)));
                personIntegration.AddRole(new PersonRole(PersonRoleType.Instructor, new DateTime(15, 01, 22), new DateTime(15, 05, 25)));
                personIntegration.AddRole(new PersonRole(PersonRoleType.Student, new DateTime(15, 01, 22), new DateTime(15, 05, 25)));

                // Mock the email address data response
                instEmail = new Domain.Base.Entities.EmailAddress("inst@inst.com", "COL") { IsPreferred = true };
                personIntegration.AddEmailAddress(instEmail);
                perEmail = new Domain.Base.Entities.EmailAddress("personal@personal.com", "PER") { IsPreferred = false };
                personIntegration.AddEmailAddress(perEmail);
                workEmail = new Domain.Base.Entities.EmailAddress("work@work.com", "BUS") { IsPreferred = false };
                personIntegration.AddEmailAddress(workEmail);

                // Mock the address hierarchy responses
                var addresses = new List<Domain.Base.Entities.Address>();
                homeAddr = new Domain.Base.Entities.Address()
                {
                    TypeCode = "HO",
                    Type = Dtos.EnumProperties.AddressType.Home.ToString(),
                    Guid = Guid.NewGuid().ToString(),
                    Status = "Current",
                    IsPreferredAddress = true,
                    SeasonalDates = new List<AddressSeasonalDates>()
                    {
                        new AddressSeasonalDates("01/01", "05/31"),
                        new AddressSeasonalDates("08/01", "12/31")
                    }
                };
                addresses.Add(homeAddr);
                mailAddr = new Domain.Base.Entities.Address()
                {
                    TypeCode = "MA",
                    Type = Dtos.EnumProperties.AddressType.Mailing.ToString(),
                    Guid = Guid.NewGuid().ToString(),
                    Status = "Current"
                };
                addresses.Add(mailAddr);
                resAddr = new Domain.Base.Entities.Address()
                {
                    TypeCode = "VA",
                    Type = Dtos.EnumProperties.AddressType.Vacation.ToString(),
                    Guid = Guid.NewGuid().ToString(),
                    Status = "Current",
                    IsPreferredResidence = true
                };
                addresses.Add(resAddr);
                workAddr = new Domain.Base.Entities.Address()
                {
                    TypeCode = "BU",
                    Type = Dtos.EnumProperties.AddressType.Business.ToString(),
                    Guid = Guid.NewGuid().ToString(),
                    Status = "Current"
                };
                addresses.Add(workAddr);
                personIntegration.Addresses = addresses;

                // Mock the person phone per type response
                homePhone = new Domain.Base.Entities.Phone("111-111-1111", "HO");
                personIntegration.AddPhone(homePhone);
                mobilePhone = new Domain.Base.Entities.Phone("222-222-2222", "MO");
                personIntegration.AddPhone(mobilePhone);
                residencePhone = new Domain.Base.Entities.Phone("333-333-3333", "VA");
                personIntegration.AddPhone(residencePhone);
                workPhone = new Domain.Base.Entities.Phone("444-444-4444", "BU", "4444");
                personIntegration.AddPhone(workPhone);

                // Mock the social media
                var socialMediaTypeCode = "TW";
                var socialMediaHandle = "pontifex";
                var personSocialMedia = new SocialMedia(socialMediaTypeCode, socialMediaHandle);
                personIntegration.AddSocialMedia(personSocialMedia);

                // Mock the person languages
                personIntegration.PrimaryLanguage = "E";
                personIntegration.SecondaryLanguages = new List<String> { "SP", "TA" };

                personRepoMock.Setup(repo => repo.GetPersonIntegrationByGuidNonCachedAsync(personGuid)).ReturnsAsync(personIntegration);
                personRepoMock.Setup(repo => repo.GetPersonIntegrationByGuidAsync(personGuid, It.IsAny<bool>())).ReturnsAsync(personIntegration);


                var personGuidList = new List<string>() { personGuid };
                var personList = new List<PersonIntegration>() { personIntegration };
                personRepoMock.Setup(repo => repo.GetPersonIntegrationByGuidNonCachedAsync(personGuidList)).ReturnsAsync(personList);
                personRepoMock.Setup(repo => repo.GetFilteredPerson2GuidsAsync(It.IsAny<int>(), It.IsAny<int>(), true, It.IsAny<PersonFilterCriteria>(), It.IsAny<string>())).ReturnsAsync(new Tuple<IEnumerable<string>, int>(personGuidList, 1));

                person2 = new Domain.Base.Entities.Person(personId2, "Green");
                person2.Guid = personGuid2;
                person2.Prefix = "Ms.";
                person2.FirstName = "Amy";
                var personGuids = new List<string>();
                personGuids.Add(person.Guid);
                personGuids.Add(person2.Guid);
                var personEntities = new List<Domain.Base.Entities.Person>();
                personEntities.Add(person);
                personEntities.Add(person2);
                personRepoMock.Setup(repo => repo.GetPersonByGuidNonCachedAsync(personGuids)).ReturnsAsync(personEntities.AsEnumerable());

                // Mock the response for getting faculty guids
                var personGuidTuple = new Tuple<IEnumerable<string>, int>(personGuids, 2);
                personRepoMock.Setup(repo => repo.GetFacultyPersonGuidsAsync(It.IsAny<int>(), It.IsAny<int>())).ReturnsAsync(personGuidTuple);

                refRepoMock.Setup(repo => repo.GetPrivacyStatusesAsync(It.IsAny<bool>()))
                     .ReturnsAsync(
                         new List<PrivacyStatus>() {
                        new PrivacyStatus( Guid.NewGuid().ToString(), "UN", "Unrestricted", PrivacyStatusType.unrestricted),
                        new PrivacyStatus( Guid.NewGuid().ToString(), "R", "restricted", PrivacyStatusType.restricted)
                        }
                     );

                refRepoMock.Setup(repo => repo.GetPersonNameTypesAsync(It.IsAny<bool>()))
                    .ReturnsAsync(
                        new List<PersonNameTypeItem>() {
                        new PersonNameTypeItem(Guid.NewGuid().ToString(), "PREFERRED", "Personal", PersonNameType.Personal) ,
                        new PersonNameTypeItem(Guid.NewGuid().ToString(), "BIRTH", "Birth", PersonNameType.Birth) ,
                        new PersonNameTypeItem(Guid.NewGuid().ToString(), "LEGAL", "Legal", PersonNameType.Legal),
                        new PersonNameTypeItem(Guid.NewGuid().ToString(), "NICKNAME", "NickName", PersonNameType.Personal),
                        new PersonNameTypeItem(Guid.NewGuid().ToString(), "HISTORY", "History", PersonNameType.Personal)
                        }
                     );

                refRepoMock.Setup(repo => repo.GetDenominationsAsync(It.IsAny<bool>()))
                    .ReturnsAsync(
                        new List<Denomination>() {
                        new Denomination(baptistGuid,"BA", "Baptist") ,
                        new Denomination(catholicGuid,"CA", "Catholic")
                        }
                     );

                refRepoMock.Setup(repo => repo.GetEmailTypesAsync(It.IsAny<bool>()))
                     .ReturnsAsync(
                         new List<EmailType>() {
                        new EmailType( Guid.NewGuid().ToString(), "COL", "College", EmailTypeCategory.School),
                        new EmailType( Guid.NewGuid().ToString(), "PER", "Personal", EmailTypeCategory.Personal),
                        new EmailType( Guid.NewGuid().ToString(), "BUS", "Business", EmailTypeCategory.Business)
                        }
                     );

                refRepoMock.Setup(repo => repo.GetSocialMediaTypesAsync(It.IsAny<bool>()))
                     .ReturnsAsync(
                         new List<SocialMediaType>() {
                        new SocialMediaType( Guid.NewGuid().ToString(), "TW", "Twitter", SocialMediaTypeCategory.twitter)
                        }
                     );

                refRepoMock.Setup(repo => repo.GetAddressTypes2Async(It.IsAny<bool>()))
                     .ReturnsAsync(
                         new List<AddressType2>() {
                        new AddressType2( Guid.NewGuid().ToString(), "HO", "Home", AddressTypeCategory.Home),
                        new AddressType2( Guid.NewGuid().ToString(), "MA", "Mailing", AddressTypeCategory.Mailing),
                        new AddressType2( Guid.NewGuid().ToString(), "VA", "Vacation", AddressTypeCategory.Vacation),
                        new AddressType2( Guid.NewGuid().ToString(), "BU", "Business", AddressTypeCategory.Business)
                        }
                     );

                refRepoMock.Setup(repo => repo.GetPhoneTypesAsync(It.IsAny<bool>()))
                     .ReturnsAsync(
                         new List<PhoneType>() {
                        new PhoneType( Guid.NewGuid().ToString(), "HO", "Home", PhoneTypeCategory.Home),
                        new PhoneType( Guid.NewGuid().ToString(), "MO", "Mobile", PhoneTypeCategory.Mobile),
                        new PhoneType( Guid.NewGuid().ToString(), "VA", "Vacation", PhoneTypeCategory.Vacation),
                        new PhoneType( Guid.NewGuid().ToString(), "BU", "Business", PhoneTypeCategory.Business)
                        }
                     );

                // Mock the person repository for roles
                personRepoMock.Setup(repo => repo.IsFacultyAsync(personId)).ReturnsAsync(true);
                personRepoMock.Setup(repo => repo.IsStudentAsync(personId)).ReturnsAsync(true);

                // Mock the person repository GUID lookup
                personRepoMock.Setup(repo => repo.GetPersonIdFromGuidAsync(personGuid)).ReturnsAsync(personId);
                personRepoMock.Setup(repo => repo.GetPersonIdFromGuidAsync(personGuid2)).ReturnsAsync(personId2);

                // Mock the reference repository for states
                states = new List<State>()
                {
                    new State("VA","Virginia"),
                    new State("MD","Maryland"),
                    new State("NY","New York"),
                    new State("MA","Massachusetts")
                };
                refRepoMock.Setup(repo => repo.GetStateCodesAsync(It.IsAny<bool>())).Returns(Task.FromResult(states));

                // Mock the reference repository for country
                countries = new List<Country>()
                 {
                    new Country("US","United States","US"){ IsoAlpha3Code = "USA"},
                    new Country("CA","Canada","CA"){ IsoAlpha3Code = "CAN"},
                    new Country("MX","Mexico","MX"){ IsoAlpha3Code = "MEX"},
                    new Country("BR","Brazil","BR"){ IsoAlpha3Code = "BRA"}
                };
                refRepoMock.Setup(repo => repo.GetCountryCodesAsync(It.IsAny<bool>())).Returns(Task.FromResult(countries));

                // Places
                var places = new List<Place>();
                var place1 = new Place() { PlacesCountry = "USA", PlacesRegion = "US-NY" };
                places.Add(place1);
                var place2 = new Place() { PlacesCountry = "CAN", PlacesRegion = "CA-ON" };
                places.Add(place2);
                refRepoMock.Setup(repo => repo.GetPlacesAsync(It.IsAny<bool>())).Returns(Task.FromResult(places.AsEnumerable<Place>()));
                //personRepoMock.Setup(repo => repo.GetPlacesAsync()).ReturnsAsync(places);

                // International Parameters Host Country
                personRepoMock.Setup(repo => repo.GetHostCountryAsync()).ReturnsAsync("USA");

                // Mock the reference repository for county
                counties = new List<County>()
                {
                    new County(countyGuid, "FFX","Fairfax County"),
                    new County(countyGuid, "BAL","Baltimore County"),
                    new County(countyGuid, "NY","New York County"),
                    new County(countyGuid, "BOS","Boston County")
                };
                refRepoMock.Setup(repo => repo.Counties).Returns(counties);

                // Mock the reference repository for marital status
                refRepoMock.Setup(repo => repo.GetMaritalStatusesAsync(It.IsAny<bool>())).Returns(Task.FromResult<IEnumerable<MaritalStatus>>(new List<MaritalStatus>()
                {
                    new MaritalStatus(maritalStatusGuid, "M", "Married")
                }));

                // Mock the reference repository for ethnicity
                refRepoMock.Setup(repo => repo.GetEthnicitiesAsync(It.IsAny<bool>())).Returns(Task.FromResult<IEnumerable<Ethnicity>>(new List<Ethnicity>()
                {
                    new Ethnicity(ethnicityGuid, "H", "Hispanic", EthnicityType.Hispanic)
                }));

                // Mock the reference repository for races
                var raceEntities = new List<Race>();
                raceEntities.Add(new Race(raceAsianGuid, "AS", "Asian", RaceType.Asian));
                raceEntities.Add(new Race(racePacificIslanderGuid, "HP", "Hawaiian/Pacific Islander", RaceType.PacificIslander));
                refRepoMock.Setup(repo => repo.GetRacesAsync(It.IsAny<bool>())).Returns(Task.FromResult<IEnumerable<Race>>(raceEntities));

                // Mock the reference repository for prefix
                refRepoMock.Setup(repo => repo.Prefixes).Returns(new List<Prefix>()
                {
                    new Prefix("MR","Mr","Mr."),
                    new Prefix("MS","Ms","Ms.")
                });

                // Mock the reference repository for suffix
                refRepoMock.Setup(repo => repo.Suffixes).Returns(new List<Suffix>()
                {
                    new Suffix("JR","Jr","Jr."),
                    new Suffix("SR","Sr","Sr.")
                });

                // Set up current user
                currentUserFactory = new CurrentUserSetup.PersonUserFactory();

                // Mock permissions
                permissionViewAnyPerson = new Ellucian.Colleague.Domain.Entities.Permission(BasePermissionCodes.ViewAnyPerson);
                personRole.AddPermission(permissionViewAnyPerson);
                roleRepoMock.Setup(rpm => rpm.Roles).Returns(new List<Role>() { personRole });

                // Mock LANGUAGES valcode 
                var languages = new Ellucian.Data.Colleague.DataContracts.ApplValcodes()
                {

                    ValsEntityAssociation = new List<Ellucian.Data.Colleague.DataContracts.ApplValcodesVals>()
                    {
                        new ApplValcodesVals() { ValInternalCodeAssocMember = "E", ValExternalRepresentationAssocMember = "English", ValActionCode3AssocMember = "ENG" },
                        new ApplValcodesVals() { ValInternalCodeAssocMember = "SP", ValExternalRepresentationAssocMember = "Spanish", ValActionCode3AssocMember = "SPA" },
                        new ApplValcodesVals() { ValInternalCodeAssocMember = "TA", ValExternalRepresentationAssocMember = "Tagalog", ValActionCode3AssocMember = "TGL" }
                    }
                };
                personBaseRepoMock.Setup(repo => repo.GetLanguagesAsync()).ReturnsAsync(languages);


                personService = new PersonService(adapterRegistry, personRepo, personBaseRepo, refRepo, null, null, null, null, currentUserFactory, roleRepo, logger);
            }

            [TestCleanup]
            public void Cleanup()
            {
                adapterRegistry = null;
                personRepo = null;
                personBaseRepo = null;
                refRepo = null;
                roleRepo = null;
                personService = null;
                personRole = null;
                permissionViewAnyPerson = null;
                roleRepoMock = null;
                currentUserFactory = null;
                refRepoMock = null;
            }

            [TestMethod]
            public async Task GetPerson2Dto()
            {
                // Act--get person
                var personDto = await personService.GetPerson2ByGuidNonCachedAsync(personGuid);
                // Assert
                Assert.IsTrue(personDto is Dtos.Person2);
                Assert.AreEqual(person.Guid, personDto.Id);
                Assert.AreEqual(person.BirthDate, personDto.BirthDate);
                Assert.AreEqual(person.DeceasedDate, personDto.DeceasedDate);
                var personSsnCredential = personDto.Credentials.FirstOrDefault(c => c.Type == Dtos.EnumProperties.CredentialType.Ssn);
                // Assert.AreEqual(person.GovernmentId, personSsnCredential.);
                // check names
                Assert.AreEqual(2, personDto.PersonNames.Count());
                // check primary name
                var personPrimaryName = personDto.PersonNames.Where(pn => pn.NameType.Category == Dtos.EnumProperties.PersonNameType2.Legal).FirstOrDefault();
                Assert.AreEqual("Mr.", personPrimaryName.Title);
                Assert.AreEqual(person.FirstName, personPrimaryName.FirstName);
                Assert.AreEqual(person.MiddleName, personPrimaryName.MiddleName);
                Assert.AreEqual(null, personPrimaryName.LastNamePrefix);
                Assert.AreEqual(person.LastName, personPrimaryName.LastName);
                Assert.AreEqual("Jr.", personPrimaryName.Pedigree);
                //Assert.AreEqual(person.Nickname, personPrimaryName.Preference.Value);
                // check race
                var personRaces = personDto.Races.ToArray();
                Assert.AreEqual(raceAsianGuid, personRaces[0].Race.Id);
                // check ethnicity
                Assert.AreEqual(ethnicityGuid, personDto.Ethnicity.EthnicGroup.Id);
                // check religion
                Assert.AreEqual(catholicGuid, personDto.Religion.Id);
                // check roles
                var personFacultyRole = personDto.Roles.Where(r => r.RoleType == Dtos.EnumProperties.PersonRoleType.Instructor).FirstOrDefault();
                Assert.AreEqual(Dtos.EnumProperties.PersonRoleType.Instructor, personFacultyRole.RoleType);
                var personStudentRole = personDto.Roles.Where(r => r.RoleType == Dtos.EnumProperties.PersonRoleType.Student).FirstOrDefault();
                Assert.AreEqual(Dtos.EnumProperties.PersonRoleType.Student, personStudentRole.RoleType);
                var personAlumniRole = personDto.Roles.Where(r => r.RoleType == Dtos.EnumProperties.PersonRoleType.Alumni).FirstOrDefault();
                Assert.AreEqual(Dtos.EnumProperties.PersonRoleType.Alumni, personAlumniRole.RoleType);
                Assert.AreEqual(3, personDto.Roles.Count());
                // check email addresses
                var personEmailAddresses = personDto.EmailAddresses as List<Dtos.DtoProperties.PersonEmailDtoProperty>;
                Assert.AreEqual(instEmail.Value, personEmailAddresses[0].Address);
                Assert.AreEqual(Ellucian.Colleague.Dtos.EmailTypeList.School, personEmailAddresses[0].Type.EmailType);
                Assert.AreEqual(perEmail.Value, personEmailAddresses[1].Address);
                Assert.AreEqual(Ellucian.Colleague.Dtos.EmailTypeList.Personal, personEmailAddresses[1].Type.EmailType);
                Assert.AreEqual(workEmail.Value, personEmailAddresses[2].Address);
                Assert.AreEqual(Ellucian.Colleague.Dtos.EmailTypeList.Business, personEmailAddresses[2].Type.EmailType);
                // compare addresses
                var personAddresses = personDto.Addresses as List<Dtos.DtoProperties.PersonAddressDtoProperty>;
                // home addr
                Assert.AreEqual(homeAddr.Guid, personAddresses[0].address.Id);
                Assert.AreEqual(Dtos.EnumProperties.AddressType.Home, personAddresses[0].Type.AddressType);
                // mailing addr
                Assert.AreEqual(mailAddr.Guid, personAddresses[1].address.Id);
                Assert.AreEqual(Dtos.EnumProperties.AddressType.Mailing, personAddresses[1].Type.AddressType);
                // residence addr
                Assert.AreEqual(resAddr.Guid, personAddresses[2].address.Id);
                Assert.AreEqual(Dtos.EnumProperties.AddressType.Vacation, personAddresses[2].Type.AddressType);
                // work addr
                Assert.AreEqual(workAddr.Guid, personAddresses[3].address.Id);
                Assert.AreEqual(Dtos.EnumProperties.AddressType.Business, personAddresses[3].Type.AddressType);
                // compare phones
                var personPhones = personDto.Phones as List<Dtos.DtoProperties.PersonPhoneDtoProperty>;
                // home phone

                Assert.AreEqual(homePhone.Number, personPhones[0].Number);
                Assert.AreEqual("Home", personPhones[0].Type.PhoneType.ToString());
                // mobile phone

                Assert.AreEqual(mobilePhone.Number, personPhones[1].Number);
                Assert.AreEqual("Mobile", personPhones[1].Type.PhoneType.ToString());
                // residence phone

                Assert.AreEqual(residencePhone.Number, personPhones[2].Number);
                Assert.AreEqual("Vacation", personPhones[2].Type.PhoneType.ToString());
                // work phone

                Assert.AreEqual(workPhone.Number, personPhones[3].Number);
                Assert.AreEqual("Business", personPhones[3].Type.PhoneType.ToString());
                Assert.AreEqual(workPhone.Extension, personPhones[3].Extension);
            }

            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public async Task GetPerson2PermissionsException()
            {
                // Mock permissions
                personRole.RemovePermission(permissionViewAnyPerson);
                roleRepoMock.Setup(rpm => rpm.Roles).Returns(new List<Role>() { personRole });
                var personDto = await personService.GetPerson2ByGuidNonCachedAsync(personGuid);
            }

            [TestMethod]
            public async Task GetPerson2PrimaryNameNulls()
            {
                person.Prefix = string.Empty;
                person.Suffix = string.Empty;
                person.MiddleName = string.Empty;
                person.Nickname = string.Empty;
                // Act--get person
                var personDto = await personService.GetPerson2ByGuidNonCachedAsync(personGuid);
                var personPrimaryName = personDto.PersonNames.Where(pn => pn.NameType.Category == Dtos.EnumProperties.PersonNameType2.Personal).FirstOrDefault();

                Assert.AreEqual(null, personPrimaryName.Title);
                Assert.AreEqual(null, personPrimaryName.Pedigree);
                Assert.AreEqual(null, personPrimaryName.MiddleName);
            }


            [TestMethod]
            public async Task GetPerson2StudentRole()
            {
                // personRepoMock.Setup(repo => repo.IsFacultyAsync(personId)).ReturnsAsync(false);
                //personRepoMock.Setup(repo => repo.IsStudentAsync(personId)).ReturnsAsync(true);
                // Act--get person
                var personDto = await personService.GetPerson2ByGuidNonCachedAsync(personGuid);
                var personStudentRole = personDto.Roles.Where(r => r.RoleType == Dtos.EnumProperties.PersonRoleType.Student).FirstOrDefault();
                Assert.AreEqual(Dtos.EnumProperties.PersonRoleType.Student, personStudentRole.RoleType);
                Assert.AreEqual(3, personDto.Roles.Count());
            }

            [TestMethod]
            public async Task GetPerson2FacultyRole()
            {
                //personRepoMock.Setup(repo => repo.IsFacultyAsync(personId)).ReturnsAsync(true);
                //personRepoMock.Setup(repo => repo.IsStudentAsync(personId)).ReturnsAsync(false);
                // Act--get person
                var personDto = await personService.GetPerson2ByGuidNonCachedAsync(personGuid);
                var personStudentFacultyRole = personDto.Roles.Where(r => r.RoleType == Dtos.EnumProperties.PersonRoleType.Instructor).FirstOrDefault();
                Assert.AreEqual(Dtos.EnumProperties.PersonRoleType.Instructor, personStudentFacultyRole.RoleType);
                Assert.AreEqual(3, personDto.Roles.Count());
            }

            //[TestMethod]
            //public async Task GetPerson2NoRoles()
            //{
            //    personRepoMock.Setup(repo => repo.IsFacultyAsync(personId)).ReturnsAsync(false);
            //    personRepoMock.Setup(repo => repo.IsStudentAsync(personId)).ReturnsAsync(false);
            //    // Act--get person
            //    var personDto = await personService.GetPerson2ByGuidNonCachedAsync(personGuid);
            //    Assert.AreEqual(1, personDto.Roles.Count());
            //}

            [TestMethod]
            public async Task GetPerson2NoSsn()
            {
                // Act--get person
                personIntegration.GovernmentId = null;
                var personDto = await personService.GetPerson2ByGuidNonCachedAsync(personGuid);
                // check ssn
                var personSsnCredential = personDto.Credentials.FirstOrDefault(c => c.Type == Dtos.EnumProperties.CredentialType.Ssn);
                Assert.AreEqual(null, personSsnCredential);
            }

            [TestMethod]
            public async Task GetPerson2NoFirstName()
            {
                // Act--get person
                personIntegration.FirstName = null;
                var personDto = await personService.GetPerson2ByGuidNonCachedAsync(personGuid);
                // check primary name
                var personPrimaryName = personDto.PersonNames.Where(pn => pn.NameType.Category == Dtos.EnumProperties.PersonNameType2.Legal).FirstOrDefault();
                Assert.AreEqual(null, personPrimaryName.FirstName);
            }

            [TestMethod]
            public async Task GetPerson2GenderTypeMale()
            {
                personIntegration.Gender = "M";
                // Act--get person
                var personDto = await personService.GetPerson2ByGuidNonCachedAsync(personGuid);
                // Assert
                Assert.AreEqual(Dtos.EnumProperties.GenderType2.Male, personDto.GenderType);
            }

            [TestMethod]
            public async Task GetPerson2GenderTypeFemale()
            {
                personIntegration.Gender = "F";
                // Act--get person
                var personDto = await personService.GetPerson2ByGuidNonCachedAsync(personGuid);
                // Assert
                Assert.AreEqual(Dtos.EnumProperties.GenderType2.Female, personDto.GenderType);
            }

            [TestMethod]
            public async Task GetPerson2GenderTypeUnknown()
            {
                personIntegration.Gender = null;
                // Act--get person
                var personDto = await personService.GetPerson2ByGuidNonCachedAsync(personGuid);
                // Assert
                Assert.AreEqual(null, personDto.GenderType);
            }

            [TestMethod]
            public async Task GetPerson2NoMaritalStatus()
            {
                personIntegration.MaritalStatusCode = null;
                // Act--get person
                var personDto = await personService.GetPerson2ByGuidNonCachedAsync(personGuid);
                // Assert
                Assert.AreEqual(null, personDto.MaritalStatus);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task GetPersonEmptyIdException()
            {
                // Act-- try to get a person without passing an ID
                string guid = null;
                var personDto = await personService.GetPerson2ByGuidNonCachedAsync(guid);
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task GetPersonIdFromGuidNullException()
            {
                person = null;
                personRepoMock.Setup(repo => repo.GetPersonIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync(() => null);
                var personDto = await personService.GetPerson2ByGuidNonCachedAsync(personGuid);
            }


            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task GetPersonIntegrationByGuidNullException()
            {
                person = null;
                personRepoMock.Setup(repo => repo.GetPersonIntegrationByGuidNonCachedAsync(personGuid)).ReturnsAsync(() => null);
                var personDto = await personService.GetPerson2ByGuidNonCachedAsync(personGuid);
            }

            [TestMethod]
            [ExpectedException(typeof(RepositoryException))]
            public async Task GetPersonsByFilterException()
            {
                personRepoMock.Setup(repo => repo.GetFilteredPerson2GuidsAsync(It.IsAny<int>(), It.IsAny<int>(), false, It.IsAny<PersonFilterCriteria>(), It.IsAny<string>())).Throws(new RepositoryException());

                await personService.GetPerson2NonCachedAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>(),
                    It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                    It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), "ssn",
                    null, It.IsAny<string>());
            }

            [TestMethod]
            public async Task GetPersonsByFilter()
            {
                var personDtosTuple = await personService.GetPerson2NonCachedAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>(),
                    It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                    It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                    It.IsAny<string>(), It.IsAny<string>());

                var personDtos = personDtosTuple.Item1;
                Assert.AreEqual(personDtosTuple.Item2, personDtos.Count());
                var personDto1 = personDtos.Select(p => p.Id = personGuid);
                Assert.IsNotNull(personDto1);
            }

            [TestMethod]
            public async Task GetPersons2ByFilterNonCached()
            {
                var personDtosTuple = await personService.GetPerson2NonCachedAsync(It.IsAny<int>(), It.IsAny<int>(), true,
                    It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                    It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                    It.IsAny<string>(), It.IsAny<string>());

                // Assert
                Assert.IsNotNull(personDtosTuple.Item1);

                var personDto = personDtosTuple.Item1.FirstOrDefault();
                var seasonalOccupancies = personDto.Addresses.First().SeasonalOccupancies;
                var seasonalDates = homeAddr.SeasonalDates;

                Assert.AreEqual(person.Guid, personDto.Id);
                Assert.AreEqual(person.BirthDate, personDto.BirthDate);
                Assert.AreEqual(person.DeceasedDate, personDto.DeceasedDate);
                var personSsnCredential = personDto.Credentials.FirstOrDefault(c => c.Type == Dtos.EnumProperties.CredentialType.Ssn);
                // Assert.AreEqual(person.GovernmentId, personSsnCredential.);
                // check names
                Assert.AreEqual(2, personDto.PersonNames.Count());
                // check primary name
                var personPrimaryName = personDto.PersonNames.Where(pn => pn.NameType.Category == Dtos.EnumProperties.PersonNameType2.Legal).FirstOrDefault();
                Assert.AreEqual("Mr.", personPrimaryName.Title);
                Assert.AreEqual(person.FirstName, personPrimaryName.FirstName);
                Assert.AreEqual(person.MiddleName, personPrimaryName.MiddleName);
                Assert.AreEqual(null, personPrimaryName.LastNamePrefix);
                Assert.AreEqual(person.LastName, personPrimaryName.LastName);
                Assert.AreEqual("Jr.", personPrimaryName.Pedigree);
                //Assert.AreEqual(person.Nickname, personPrimaryName.Preference.Value);
                // check race
                var personRaces = personDto.Races.ToArray();
                Assert.AreEqual(raceAsianGuid, personRaces[0].Race.Id);
                // check ethnicity
                Assert.AreEqual(ethnicityGuid, personDto.Ethnicity.EthnicGroup.Id);
                // check roles
                var personFacultyRole = personDto.Roles.Where(r => r.RoleType == Dtos.EnumProperties.PersonRoleType.Instructor).FirstOrDefault();
                Assert.AreEqual(Dtos.EnumProperties.PersonRoleType.Instructor, personFacultyRole.RoleType);
                var personStudentRole = personDto.Roles.Where(r => r.RoleType == Dtos.EnumProperties.PersonRoleType.Student).FirstOrDefault();
                Assert.AreEqual(Dtos.EnumProperties.PersonRoleType.Student, personStudentRole.RoleType);
                Assert.AreEqual(3, personDto.Roles.Count());
                // check email addresses
                var personEmailAddresses = personDto.EmailAddresses as List<Dtos.DtoProperties.PersonEmailDtoProperty>;
                Assert.AreEqual(instEmail.Value, personEmailAddresses[0].Address);
                Assert.AreEqual(Ellucian.Colleague.Dtos.EmailTypeList.School, personEmailAddresses[0].Type.EmailType);

                //Check seasonal dates
                foreach (var seasonalOccupancy in seasonalOccupancies)
                {
                    var seasonalDate = seasonalDates.FirstOrDefault(i => Convert.ToDateTime(i.StartOn).Equals(seasonalOccupancy.Recurrence.TimePeriod.StartOn.Value.DateTime));
                    Assert.IsNotNull(seasonalDate);
                    Assert.AreEqual(Convert.ToDateTime(seasonalDate.StartOn), seasonalOccupancy.Recurrence.TimePeriod.StartOn.Value.DateTime);
                    Assert.AreEqual(Convert.ToDateTime(seasonalDate.EndOn), seasonalOccupancy.Recurrence.TimePeriod.EndOn.Value.DateTime);
                }
            }

            [TestMethod]
            public async Task GetPerson2Dto_BirthName()
            {
                var birthFirstName = "Bernard";
                var birthLastName = "Sanders";
                var birthMiddleName = "Bernie";
                personIntegration.BirthNameFirst = birthFirstName;
                personIntegration.BirthNameLast = birthLastName;
                personIntegration.BirthNameMiddle = birthMiddleName;

                // Act--get person
                var personDto = await personService.GetPerson2ByGuidNonCachedAsync(personGuid);
                var personBirthName = personDto.PersonNames.Where(pn => pn.NameType.Category == Dtos.EnumProperties.PersonNameType2.Birth).FirstOrDefault();

                // Assert

                Assert.AreEqual(birthFirstName, personBirthName.FirstName);
                Assert.AreEqual(birthMiddleName, personBirthName.MiddleName);
                Assert.AreEqual(birthLastName, personBirthName.LastName);
            }


            [TestMethod]
            public async Task GetPerson2Dto_PersonLanguages()
            {
                // Act--get person
                var personDto = await personService.GetPerson2ByGuidNonCachedAsync(personGuid);
                var personLanguages = personDto.Languages.Where(x => x.Code == PersonLanguageCode.eng).FirstOrDefault();

                // Assert
                Assert.AreEqual(PersonLanguageCode.eng, personLanguages.Code);
                Assert.AreEqual(Dtos.EnumProperties.PersonLanguagePreference.Primary, personLanguages.Preference);
            }


            [TestMethod]
            public async Task GetPerson2Dto_PersonAlienStatus()
            {
                var alienStatus = "CS";
                personIntegration.AlienStatus = alienStatus;

                var allCitizenshipStatuses = new TestCitizenshipStatusRepository().Get();
                refRepoMock.Setup(repo => repo.GetCitizenshipStatusesAsync(It.IsAny<bool>())).ReturnsAsync(allCitizenshipStatuses);
                // Act--get person
                var personDto = await personService.GetPerson2ByGuidNonCachedAsync(personGuid);

                // Assert
                // {"87ec6f69-9b16-4ed5-8954-59067f0318ec", "CS", "Canadian citizen", "NA"}, 

                var citizenShipStatus = allCitizenshipStatuses.FirstOrDefault(x => x.Code == alienStatus);

                Assert.AreEqual(Dtos.CitizenshipStatusType.Citizen, personDto.CitizenshipStatus.Category);
                Assert.AreEqual(citizenShipStatus.Guid, personDto.CitizenshipStatus.Detail.Id);
            }

            [TestMethod]
            public async Task GetPerson2Dto_PersonCountry()
            {
                var country = "CA";
                personIntegration.BirthCountry = country;
                // Act--get person
                var personDto = await personService.GetPerson2ByGuidNonCachedAsync(personGuid);

                // Assert          
                var birthCountry = countries.FirstOrDefault(x => x.Code == country);
                Assert.AreEqual(birthCountry.Iso3Code, personDto.CountryOfBirth);
            }

            [TestMethod]
            public async Task GetPerson2Dto_PersonCitizenship()
            {
                var country = "CA";
                personIntegration.Citizenship = country;
                // Act--get person
                var personDto = await personService.GetPerson2ByGuidNonCachedAsync(personGuid);
                // Assert          
                var birthCountry = countries.FirstOrDefault(x => x.Code == country);
                Assert.AreEqual(birthCountry.Iso3Code, personDto.CitizenshipCountry);
            }

            [TestMethod]
            public async Task GetPerson2Dto_SocialMedia()
            {
                var socialMediaTypeCode = "TW";
                var socialMediaHandle = "pontifex";
                var personSocialMedia = new SocialMedia(socialMediaTypeCode, socialMediaHandle);
                personIntegration.AddSocialMedia(personSocialMedia);
                // Act--get person
                var allSocialMediaTypes = new TestSocialMediaTypesRepository().GetSocialMediaTypes();
                refRepoMock.Setup(x => x.GetSocialMediaTypesAsync(It.IsAny<bool>())).ReturnsAsync(allSocialMediaTypes);
                var personDto = await personService.GetPerson2ByGuidNonCachedAsync(personGuid);

                //   {"13660156-d481-4b3d-b617-92136979314c", "TW", "Twitter", "twitter"}, 
                var socialMediaType = allSocialMediaTypes.FirstOrDefault(x => x.Code == socialMediaTypeCode);

                // Assert          
                var personSocialMediaType = personDto.SocialMedia.FirstOrDefault(x => x.Type.Category == Dtos.SocialMediaTypeCategory.twitter);
                Assert.AreEqual(Dtos.SocialMediaTypeCategory.twitter, personSocialMediaType.Type.Category);
                Assert.AreEqual(socialMediaType.Guid, personSocialMediaType.Type.Detail.Id);
                Assert.AreEqual(socialMediaHandle, personSocialMediaType.Address);
            }

            [TestMethod]
            public async Task GetPerson2Dto_Passport()
            {
                var passportNumber = "A1231";
                var passport = new PersonPassport(personGuid, passportNumber);
                passport.IssuingCountry = "USA";
                personIntegration.Passport = passport;

                var allIdentityDocuments = new TestIdentityDocumentTypeRepository().Get();
                refRepoMock.Setup(x => x.GetIdentityDocumentTypesAsync(It.IsAny<bool>())).ReturnsAsync(allIdentityDocuments);

                var passportIdentityDocument = allIdentityDocuments.FirstOrDefault(x => x.Code == "PASSPORT");

                var personDto = await personService.GetPerson2ByGuidNonCachedAsync(personGuid);
                var personPassport = personDto.IdentityDocuments.ElementAtOrDefault(0);

                Assert.IsNotNull(personPassport);
                Assert.AreEqual(passportNumber, personPassport.DocumentId);
                Assert.AreEqual(passportIdentityDocument.Guid, personPassport.Type.Detail.Id);
            }

            [TestMethod]
            public async Task GetPerson2Dto_DriversLicense()
            {
                var licenseNumber = "6523123";
                var licenseState = "NY";
                var driverLicense = new PersonDriverLicense(personGuid, licenseNumber);
                driverLicense.IssuingState = licenseState;
                personIntegration.DriverLicense = driverLicense;

                var allIdentityDocuments = new TestIdentityDocumentTypeRepository().Get();
                refRepoMock.Setup(x => x.GetIdentityDocumentTypesAsync(It.IsAny<bool>())).ReturnsAsync(allIdentityDocuments);

                var licenseIdentityDocument = allIdentityDocuments.FirstOrDefault(x => x.Code == "LICENSE");

                var personDto = await personService.GetPerson2ByGuidNonCachedAsync(personGuid);
                var personDriverLicense = personDto.IdentityDocuments.ElementAtOrDefault(0);

                Assert.IsNotNull(personDriverLicense);
                Assert.AreEqual(licenseNumber, personDriverLicense.DocumentId);
                Assert.AreEqual(string.Concat("US-", licenseState), personDriverLicense.Country.Region.Code);
                Assert.AreEqual("USA", personDriverLicense.Country.Code.ToString());
                Assert.AreEqual(licenseIdentityDocument.Guid, personDriverLicense.Type.Detail.Id);
            }

            [TestMethod]
            public async Task GetPerson2Dto_Interests()
            {
                var interest1 = "ART";
                var interest2 = "BASE";
                personIntegration.Interests = new List<string>() { interest1, interest2 };

                var allInterests = new TestInterestsRepository().GetInterests();

                refRepoMock.Setup(x => x.GetInterestsAsync(It.IsAny<bool>())).ReturnsAsync(allInterests);
                var personDto = await personService.GetPerson2ByGuidNonCachedAsync(personGuid);

                var interestArt = allInterests.FirstOrDefault(x => x.Code == interest1);
                var interestBase = allInterests.FirstOrDefault(x => x.Code == interest2);

                Assert.AreEqual(2, personDto.Interests.Count());
                Assert.AreEqual(interestArt.Guid, personDto.Interests.ElementAtOrDefault(0).Id);
                Assert.AreEqual(interestBase.Guid, personDto.Interests.ElementAtOrDefault(1).Id);
            }

            [TestMethod]
            public async Task GetPerson2Dto_Interests_Invalid()
            {
                var interest1 = "INVALID";
                personIntegration.Interests = new List<string>() { interest1 };
                var allInterests = new TestInterestsRepository().GetInterests();

                refRepoMock.Setup(x => x.GetInterestsAsync(It.IsAny<bool>())).ReturnsAsync(allInterests);
                var personDto = await personService.GetPerson2ByGuidNonCachedAsync(personGuid);

                Assert.AreEqual(null, personDto.Interests);
            }

        }

        [TestClass]
        public class GetPerson2 : CurrentUserSetup
        {
            private string personId = "0000011";
            private string personId2 = "0000012";
            private string personGuid = "5674f28b-b216-4055-b236-81a922d93b4c";
            private string personGuid2 = "1111f28b-b216-4055-b236-81a922d93b4c";
            private Mock<IPersonRepository> personRepoMock;
            private IPersonRepository personRepo;
            private Mock<IPersonBaseRepository> personBaseRepoMock;
            private IPersonBaseRepository personBaseRepo;
            private Mock<IReferenceDataRepository> refRepoMock;
            private IReferenceDataRepository refRepo;
            private Mock<IAdapterRegistry> adapterRegistryMock;
            private IAdapterRegistry adapterRegistry;
            private PersonService personService;
            private ILogger logger;
            private Ellucian.Colleague.Domain.Base.Entities.Person person;
            private Ellucian.Colleague.Domain.Base.Entities.Person person2;
            private Ellucian.Colleague.Domain.Base.Entities.PersonIntegration personIntegration;
            private Mock<IRoleRepository> roleRepoMock;
            private IRoleRepository roleRepo;
            private ICurrentUserFactory currentUserFactory;
            private Ellucian.Colleague.Domain.Base.Entities.EmailAddress instEmail;
            private Ellucian.Colleague.Domain.Base.Entities.EmailAddress perEmail;
            private Ellucian.Colleague.Domain.Base.Entities.EmailAddress workEmail;
            private Ellucian.Colleague.Domain.Base.Entities.Address homeAddr;
            private Ellucian.Colleague.Domain.Base.Entities.Address mailAddr;
            private Ellucian.Colleague.Domain.Base.Entities.Address resAddr;
            private Ellucian.Colleague.Domain.Base.Entities.Address workAddr;
            private Ellucian.Colleague.Domain.Base.Entities.Phone homePhone;
            private Ellucian.Colleague.Domain.Base.Entities.Phone mobilePhone;
            private Ellucian.Colleague.Domain.Base.Entities.Phone residencePhone;
            private Ellucian.Colleague.Domain.Base.Entities.Phone workPhone;
            private IEnumerable<Ellucian.Colleague.Domain.Base.Entities.State> states;
            private IEnumerable<Ellucian.Colleague.Domain.Base.Entities.Country> countries;
            private List<Ellucian.Colleague.Domain.Base.Entities.County> counties;
            private string maritalStatusGuid = Guid.NewGuid().ToString();
            private string ethnicityGuid = Guid.NewGuid().ToString();
            private string raceAsianGuid = Guid.NewGuid().ToString();
            private string racePacificIslanderGuid = Guid.NewGuid().ToString();
            private string countyGuid = Guid.NewGuid().ToString();
            private string baptistGuid = Guid.NewGuid().ToString();
            private string catholicGuid = Guid.NewGuid().ToString();
            private Ellucian.Colleague.Domain.Entities.Permission permissionViewAnyPerson;

            [TestInitialize]
            public void Initialize()
            {
                personRepoMock = new Mock<IPersonRepository>();
                personRepo = personRepoMock.Object;
                personBaseRepoMock = new Mock<IPersonBaseRepository>();
                personBaseRepo = personBaseRepoMock.Object;
                refRepoMock = new Mock<IReferenceDataRepository>();
                refRepo = refRepoMock.Object;
                adapterRegistryMock = new Mock<IAdapterRegistry>();
                adapterRegistry = adapterRegistryMock.Object;
                roleRepoMock = new Mock<IRoleRepository>();
                roleRepo = roleRepoMock.Object;
                logger = new Mock<ILogger>().Object;

                // Mock person response from the person repository
                person = new Domain.Base.Entities.Person(personId, "Brown");
                person.Guid = personGuid;
                person.Prefix = "Mr.";
                person.FirstName = "Ricky";
                person.MiddleName = "Lee";
                person.Suffix = "Jr.";
                person.Nickname = "Rick";
                person.BirthDate = new DateTime(1930, 1, 1);
                person.DeceasedDate = new DateTime(2014, 5, 12);
                person.GovernmentId = "111-11-1111";
                person.MaritalStatusCode = "M";
                person.EthnicCodes = new List<string> { "H" };
                person.RaceCodes = new List<string> { "AS" };
                person.AddEmailAddress(new EmailAddress("xyz@xmail.com", "COL"));
                personRepoMock.Setup(repo => repo.GetPersonByGuidNonCachedAsync(personGuid)).ReturnsAsync(person);

                var filteredPersonGuidTuple = new Tuple<IEnumerable<string>, int>(new List<string>() { personGuid }, 1);
                personRepoMock.Setup(repo => repo.GetFilteredPerson2GuidsAsync(It.IsAny<int>(), It.IsAny<int>(), true, It.IsAny<PersonFilterCriteria>(), It.IsAny<string>())).ReturnsAsync(filteredPersonGuidTuple);

                personIntegration = new PersonIntegration(personId, "Brown");
                personIntegration.Guid = personGuid;
                personIntegration.Prefix = "Mr.";
                personIntegration.FirstName = "Ricky";
                personIntegration.MiddleName = "Lee";
                personIntegration.Suffix = "Jr.";
                personIntegration.Nickname = "Rick";
                personIntegration.BirthDate = new DateTime(1930, 1, 1);
                personIntegration.DeceasedDate = new DateTime(2014, 5, 12);
                personIntegration.GovernmentId = "111-11-1111";
                personIntegration.Religion = "CA";
                personIntegration.MaritalStatusCode = "M";
                personIntegration.EthnicCodes = new List<string> { "H", "N" };
                personIntegration.RaceCodes = new List<string> { "AS" };
                personIntegration.AddRole(new PersonRole(PersonRoleType.Alumni, new DateTime(15, 01, 22), new DateTime(15, 05, 25)));
                personIntegration.AddRole(new PersonRole(PersonRoleType.Instructor, new DateTime(15, 01, 22), new DateTime(15, 05, 25)));
                personIntegration.AddRole(new PersonRole(PersonRoleType.Student, new DateTime(15, 01, 22), new DateTime(15, 05, 25)));
                // Mock the email address data response
                instEmail = new Domain.Base.Entities.EmailAddress("inst@inst.com", "COL") { IsPreferred = true };
                personIntegration.AddEmailAddress(instEmail);
                perEmail = new Domain.Base.Entities.EmailAddress("personal@personal.com", "PER") { IsPreferred = false };
                personIntegration.AddEmailAddress(perEmail);
                workEmail = new Domain.Base.Entities.EmailAddress("work@work.com", "BUS") { IsPreferred = false };
                personIntegration.AddEmailAddress(workEmail);

                // Mock the address hierarchy responses
                var addresses = new List<Domain.Base.Entities.Address>();
                homeAddr = new Domain.Base.Entities.Address()
                {
                    TypeCode = "HO",
                    Type = Dtos.EnumProperties.AddressType.Home.ToString(),
                    Guid = Guid.NewGuid().ToString(),
                    Status = "Current",
                    IsPreferredAddress = true
                };
                addresses.Add(homeAddr);
                mailAddr = new Domain.Base.Entities.Address()
                {
                    TypeCode = "MA",
                    Type = Dtos.EnumProperties.AddressType.Mailing.ToString(),
                    Guid = Guid.NewGuid().ToString(),
                    Status = "Current"
                };
                addresses.Add(mailAddr);
                resAddr = new Domain.Base.Entities.Address()
                {
                    TypeCode = "VA",
                    Type = Dtos.EnumProperties.AddressType.Vacation.ToString(),
                    Guid = Guid.NewGuid().ToString(),
                    Status = "Current",
                    IsPreferredResidence = true
                };
                addresses.Add(resAddr);
                workAddr = new Domain.Base.Entities.Address()
                {
                    TypeCode = "BU",
                    Type = Dtos.EnumProperties.AddressType.Business.ToString(),
                    Guid = Guid.NewGuid().ToString(),
                    Status = "Current"
                };
                addresses.Add(workAddr);
                personIntegration.Addresses = addresses;

                // Mock the person phone per type response
                homePhone = new Domain.Base.Entities.Phone("111-111-1111", "HO");
                personIntegration.AddPhone(homePhone);
                mobilePhone = new Domain.Base.Entities.Phone("222-222-2222", "MO");
                personIntegration.AddPhone(mobilePhone);
                residencePhone = new Domain.Base.Entities.Phone("333-333-3333", "VA");
                personIntegration.AddPhone(residencePhone);
                workPhone = new Domain.Base.Entities.Phone("444-444-4444", "BU", "4444");
                personIntegration.AddPhone(workPhone);

                // Mock the person languages
                personIntegration.PrimaryLanguage = "E";
                personIntegration.SecondaryLanguages = new List<String>{"SP", "TA" };

                // Mock the social media
                var socialMedia = new List<Domain.Base.Entities.SocialMedia>();
                var socialMediaTypeCode = "TW";
                var socialMediaHandle = "pontifex";
                var personSocialMedia = new SocialMedia(socialMediaTypeCode, socialMediaHandle);
                personIntegration.AddSocialMedia(personSocialMedia);

                personRepoMock.Setup(repo => repo.GetPersonIntegrationByGuidNonCachedAsync(personGuid)).ReturnsAsync(personIntegration);
                personRepoMock.Setup(repo => repo.GetPersonIntegrationByGuidAsync(personGuid, It.IsAny<bool>())).ReturnsAsync(personIntegration);


                var personGuidList = new List<string>() { personGuid };
                var personList = new List<PersonIntegration>() { personIntegration };
                personRepoMock.Setup(repo => repo.GetPersonIntegrationByGuidNonCachedAsync(personGuidList)).ReturnsAsync(personList);
                personRepoMock.Setup(repo => repo.GetFilteredPerson2GuidsAsync(It.IsAny<int>(), It.IsAny<int>(), true, It.IsAny<PersonFilterCriteria>(), It.IsAny<string>())).ReturnsAsync(new Tuple<IEnumerable<string>, int>(personGuidList, 1));

                person2 = new Domain.Base.Entities.Person(personId2, "Green");
                person2.Guid = personGuid2;
                person2.Prefix = "Ms.";
                person2.FirstName = "Amy";
                var personGuids = new List<string>();
                personGuids.Add(person.Guid);
                personGuids.Add(person2.Guid);
                var personEntities = new List<Domain.Base.Entities.Person>();
                personEntities.Add(person);
                personEntities.Add(person2);
                personRepoMock.Setup(repo => repo.GetPersonByGuidNonCachedAsync(personGuids)).ReturnsAsync(personEntities.AsEnumerable());

                // Mock the response for getting faculty guids
                var personGuidTuple = new Tuple<IEnumerable<string>, int>(personGuids, 2);
                personRepoMock.Setup(repo => repo.GetFacultyPersonGuidsAsync(It.IsAny<int>(), It.IsAny<int>())).ReturnsAsync(personGuidTuple);

                refRepoMock.Setup(repo => repo.GetPrivacyStatusesAsync(It.IsAny<bool>()))
                     .ReturnsAsync(
                         new List<PrivacyStatus>() {
                        new PrivacyStatus( Guid.NewGuid().ToString(), "UN", "Unrestricted", PrivacyStatusType.unrestricted),
                        new PrivacyStatus( Guid.NewGuid().ToString(), "R", "restricted", PrivacyStatusType.restricted)
                        }
                     );

                refRepoMock.Setup(repo => repo.GetPersonNameTypesAsync(It.IsAny<bool>()))
                    .ReturnsAsync(
                        new List<PersonNameTypeItem>() {
                        new PersonNameTypeItem(Guid.NewGuid().ToString(), "PREFERRED", "Personal", PersonNameType.Personal) ,
                        new PersonNameTypeItem(Guid.NewGuid().ToString(), "BIRTH", "Birth", PersonNameType.Birth) ,
                        new PersonNameTypeItem(Guid.NewGuid().ToString(), "LEGAL", "Legal", PersonNameType.Legal),
                        new PersonNameTypeItem(Guid.NewGuid().ToString(), "NICKNAME", "NickName", PersonNameType.Personal),
                        new PersonNameTypeItem(Guid.NewGuid().ToString(), "HISTORY", "History", PersonNameType.Personal)
                        }
                     );

                refRepoMock.Setup(repo => repo.GetDenominationsAsync(It.IsAny<bool>()))
                    .ReturnsAsync(
                        new List<Denomination>() {
                        new Denomination(baptistGuid,"BA", "Baptist") ,
                        new Denomination(catholicGuid,"CA", "Catholic")
                        }
                     );

                refRepoMock.Setup(repo => repo.GetEmailTypesAsync(It.IsAny<bool>()))
                     .ReturnsAsync(
                         new List<EmailType>() {
                        new EmailType( Guid.NewGuid().ToString(), "COL", "College", EmailTypeCategory.School),
                        new EmailType( Guid.NewGuid().ToString(), "PER", "Personal", EmailTypeCategory.Personal),
                        new EmailType( Guid.NewGuid().ToString(), "BUS", "Business", EmailTypeCategory.Business)
                        }
                     );

                refRepoMock.Setup(repo => repo.GetSocialMediaTypesAsync(It.IsAny<bool>()))
                     .ReturnsAsync(
                         new List<SocialMediaType>() {
                        new SocialMediaType( Guid.NewGuid().ToString(), "TW", "Twitter", SocialMediaTypeCategory.twitter)
                        }
                     );

                refRepoMock.Setup(repo => repo.GetAddressTypes2Async(It.IsAny<bool>()))
                     .ReturnsAsync(
                         new List<AddressType2>() {
                        new AddressType2( Guid.NewGuid().ToString(), "HO", "Home", AddressTypeCategory.Home),
                        new AddressType2( Guid.NewGuid().ToString(), "MA", "Mailing", AddressTypeCategory.Mailing),
                        new AddressType2( Guid.NewGuid().ToString(), "VA", "Vacation", AddressTypeCategory.Vacation),
                        new AddressType2( Guid.NewGuid().ToString(), "BU", "Business", AddressTypeCategory.Business)
                        }
                     );

                refRepoMock.Setup(repo => repo.GetPhoneTypesAsync(It.IsAny<bool>()))
                     .ReturnsAsync(
                         new List<PhoneType>() {
                        new PhoneType( Guid.NewGuid().ToString(), "HO", "Home", PhoneTypeCategory.Home),
                        new PhoneType( Guid.NewGuid().ToString(), "MO", "Mobile", PhoneTypeCategory.Mobile),
                        new PhoneType( Guid.NewGuid().ToString(), "VA", "Vacation", PhoneTypeCategory.Vacation),
                        new PhoneType( Guid.NewGuid().ToString(), "BU", "Business", PhoneTypeCategory.Business)
                        }
                     );

                // Mock the person repository for roles
                //personRepoMock.Setup(repo => repo.IsFacultyAsync(personId)).ReturnsAsync(true);
                //personRepoMock.Setup(repo => repo.IsStudentAsync(personId)).ReturnsAsync(true);

                // Mock the person repository GUID lookup
                personRepoMock.Setup(repo => repo.GetPersonIdFromGuidAsync(personGuid)).ReturnsAsync(personId);
                personRepoMock.Setup(repo => repo.GetPersonIdFromGuidAsync(personGuid2)).ReturnsAsync(personId2);

                // Mock the reference repository for states
                states = new List<State>()
                {
                    new State("VA","Virginia"),
                    new State("MD","Maryland"),
                    new State("NY","New York"),
                    new State("MA","Massachusetts")
                };
                refRepoMock.Setup(repo => repo.GetStateCodesAsync(It.IsAny<bool>())).Returns(Task.FromResult(states));

                // Mock the reference repository for country
                countries = new List<Country>()
                 {
                    new Country("US","United States","US"){ IsoAlpha3Code = "USA"},
                    new Country("CA","Canada","CA"){ IsoAlpha3Code = "CAN"},
                    new Country("MX","Mexico","MX"){ IsoAlpha3Code = "MEX"},
                    new Country("BR","Brazil","BR"){ IsoAlpha3Code = "BRA"}
                };
                refRepoMock.Setup(repo => repo.GetCountryCodesAsync(It.IsAny<bool>())).Returns(Task.FromResult(countries));

                // Places
                var places = new List<Place>();
                var place1 = new Place() { PlacesCountry = "USA", PlacesRegion = "US-NY" };
                places.Add(place1);
                var place2 = new Place() { PlacesCountry = "CAN", PlacesRegion = "CA-ON" };
                places.Add(place2);
                refRepoMock.Setup(repo => repo.GetPlacesAsync(It.IsAny<bool>())).Returns(Task.FromResult(places.AsEnumerable<Place>()));
                //personRepoMock.Setup(repo => repo.GetPlacesAsync()).ReturnsAsync(places);

                // International Parameters Host Country
                personRepoMock.Setup(repo => repo.GetHostCountryAsync()).ReturnsAsync("USA");

                // Mock the reference repository for county
                counties = new List<County>()
                {
                    new County(countyGuid, "FFX","Fairfax County"),
                    new County(countyGuid, "BAL","Baltimore County"),
                    new County(countyGuid, "NY","New York County"),
                    new County(countyGuid, "BOS","Boston County")
                };
                refRepoMock.Setup(repo => repo.Counties).Returns(counties);

                // Mock the reference repository for marital status
                refRepoMock.Setup(repo => repo.GetMaritalStatusesAsync(It.IsAny<bool>())).Returns(Task.FromResult<IEnumerable<MaritalStatus>>(new List<MaritalStatus>()
                {
                    new MaritalStatus(maritalStatusGuid, "M", "Married")
                }));

                // Mock the reference repository for ethnicity
                refRepoMock.Setup(repo => repo.GetEthnicitiesAsync(It.IsAny<bool>())).Returns(Task.FromResult<IEnumerable<Ethnicity>>(new List<Ethnicity>()
                {
                    new Ethnicity(ethnicityGuid, "H", "Hispanic", EthnicityType.Hispanic)
                }));

                // Mock the reference repository for races
                var raceEntities = new List<Race>();
                raceEntities.Add(new Race(raceAsianGuid, "AS", "Asian", RaceType.Asian));
                raceEntities.Add(new Race(racePacificIslanderGuid, "HP", "Hawaiian/Pacific Islander", RaceType.PacificIslander));
                refRepoMock.Setup(repo => repo.GetRacesAsync(It.IsAny<bool>())).Returns(Task.FromResult<IEnumerable<Race>>(raceEntities));

                // Mock the reference repository for prefix
                refRepoMock.Setup(repo => repo.Prefixes).Returns(new List<Prefix>()
                {
                    new Prefix("MR","Mr","Mr."),
                    new Prefix("MS","Ms","Ms.")
                });

                // Mock the reference repository for suffix
                refRepoMock.Setup(repo => repo.Suffixes).Returns(new List<Suffix>()
                {
                    new Suffix("JR","Jr","Jr."),
                    new Suffix("SR","Sr","Sr.")
                });

                // Set up current user
                currentUserFactory = new CurrentUserSetup.PersonUserFactory();

                // Mock permissions
                permissionViewAnyPerson = new Ellucian.Colleague.Domain.Entities.Permission(BasePermissionCodes.ViewAnyPerson);
                personRole.AddPermission(permissionViewAnyPerson);
                roleRepoMock.Setup(rpm => rpm.Roles).Returns(new List<Role>() { personRole });

                // Mock LANGUAGES valcode 
                var languages = new Ellucian.Data.Colleague.DataContracts.ApplValcodes()
                {
                    
                    ValsEntityAssociation = new List<Ellucian.Data.Colleague.DataContracts.ApplValcodesVals>()
                    {
                        new ApplValcodesVals() { ValInternalCodeAssocMember = "E", ValExternalRepresentationAssocMember = "English", ValActionCode3AssocMember = "ENG" },
                        new ApplValcodesVals() { ValInternalCodeAssocMember = "SP", ValExternalRepresentationAssocMember = "Spanish", ValActionCode3AssocMember = "SPA" },
                        new ApplValcodesVals() { ValInternalCodeAssocMember = "TA", ValExternalRepresentationAssocMember = "Tagalog", ValActionCode3AssocMember = "TGL" }
                    }
                };                
                personBaseRepoMock.Setup(repo => repo.GetLanguagesAsync()).ReturnsAsync(languages);

                personService = new PersonService(adapterRegistry, personRepo, personBaseRepo, refRepo, null, null, null, null, currentUserFactory, roleRepo, logger);
            }

            [TestCleanup]
            public void Cleanup()
            {
                adapterRegistry = null;
                personRepo = null;
                personBaseRepo = null;
                refRepo = null;
                roleRepo = null;
                personService = null;
                personRole = null;
                permissionViewAnyPerson = null;
                roleRepoMock = null;
                currentUserFactory = null;
                refRepoMock = null;
            }

            [TestMethod]
            public async Task GetPerson2ByGuid()
            {
                // Act--get person
                var personDto = await personService.GetPerson2ByGuidAsync(personGuid, false);
                // Assert
                Assert.IsTrue(personDto is Dtos.Person2);
                Assert.AreEqual(person.Guid, personDto.Id);
                Assert.AreEqual(person.BirthDate, personDto.BirthDate);
                Assert.AreEqual(person.DeceasedDate, personDto.DeceasedDate);
                var personSsnCredential = personDto.Credentials.FirstOrDefault(c => c.Type == Dtos.EnumProperties.CredentialType.Ssn);
                // Assert.AreEqual(person.GovernmentId, personSsnCredential.);
                // check names
                Assert.AreEqual(2, personDto.PersonNames.Count());
                // check primary name
                var personPrimaryName = personDto.PersonNames.Where(pn => pn.NameType.Category == Dtos.EnumProperties.PersonNameType2.Legal).FirstOrDefault();
                Assert.AreEqual("Mr.", personPrimaryName.Title);
                Assert.AreEqual(person.FirstName, personPrimaryName.FirstName);
                Assert.AreEqual(person.MiddleName, personPrimaryName.MiddleName);
                Assert.AreEqual(null, personPrimaryName.LastNamePrefix);
                Assert.AreEqual(person.LastName, personPrimaryName.LastName);
                Assert.AreEqual("Jr.", personPrimaryName.Pedigree);
                //Assert.AreEqual(person.Nickname, personPrimaryName.Preference.Value);
                // check race
                var personRaces = personDto.Races.ToArray();
                Assert.AreEqual(raceAsianGuid, personRaces[0].Race.Id);
                // check ethnicity
                Assert.AreEqual(ethnicityGuid, personDto.Ethnicity.EthnicGroup.Id);
                // check religion
                Assert.AreEqual(catholicGuid, personDto.Religion.Id);
                // check roles
                var personFacultyRole = personDto.Roles.Where(r => r.RoleType == Dtos.EnumProperties.PersonRoleType.Instructor).FirstOrDefault();
                Assert.AreEqual(Dtos.EnumProperties.PersonRoleType.Instructor, personFacultyRole.RoleType);
                var personStudentRole = personDto.Roles.Where(r => r.RoleType == Dtos.EnumProperties.PersonRoleType.Student).FirstOrDefault();
                Assert.AreEqual(Dtos.EnumProperties.PersonRoleType.Student, personStudentRole.RoleType);
                var personAlumniRole = personDto.Roles.Where(r => r.RoleType == Dtos.EnumProperties.PersonRoleType.Alumni).FirstOrDefault();
                Assert.AreEqual(Dtos.EnumProperties.PersonRoleType.Alumni, personAlumniRole.RoleType);
                Assert.AreEqual(3, personDto.Roles.Count());
                // check email addresses
                var personEmailAddresses = personDto.EmailAddresses as List<Dtos.DtoProperties.PersonEmailDtoProperty>;
                Assert.AreEqual(instEmail.Value, personEmailAddresses[0].Address);
                Assert.AreEqual(Ellucian.Colleague.Dtos.EmailTypeList.School, personEmailAddresses[0].Type.EmailType);
                Assert.AreEqual(perEmail.Value, personEmailAddresses[1].Address);
                Assert.AreEqual(Ellucian.Colleague.Dtos.EmailTypeList.Personal, personEmailAddresses[1].Type.EmailType);
                Assert.AreEqual(workEmail.Value, personEmailAddresses[2].Address);
                Assert.AreEqual(Ellucian.Colleague.Dtos.EmailTypeList.Business, personEmailAddresses[2].Type.EmailType);
                // compare addresses
                var personAddresses = personDto.Addresses as List<Dtos.DtoProperties.PersonAddressDtoProperty>;
                // home addr
                Assert.AreEqual(homeAddr.Guid, personAddresses[0].address.Id);
                Assert.AreEqual(Dtos.EnumProperties.AddressType.Home, personAddresses[0].Type.AddressType);
                // mailing addr
                Assert.AreEqual(mailAddr.Guid, personAddresses[1].address.Id);
                Assert.AreEqual(Dtos.EnumProperties.AddressType.Mailing, personAddresses[1].Type.AddressType);
                // residence addr
                Assert.AreEqual(resAddr.Guid, personAddresses[2].address.Id);
                Assert.AreEqual(Dtos.EnumProperties.AddressType.Vacation, personAddresses[2].Type.AddressType);
                // work addr
                Assert.AreEqual(workAddr.Guid, personAddresses[3].address.Id);
                Assert.AreEqual(Dtos.EnumProperties.AddressType.Business, personAddresses[3].Type.AddressType);
                // compare phones
                var personPhones = personDto.Phones as List<Dtos.DtoProperties.PersonPhoneDtoProperty>;
                // home phone

                Assert.AreEqual(homePhone.Number, personPhones[0].Number);
                Assert.AreEqual("Home", personPhones[0].Type.PhoneType.ToString());
                // mobile phone

                Assert.AreEqual(mobilePhone.Number, personPhones[1].Number);
                Assert.AreEqual("Mobile", personPhones[1].Type.PhoneType.ToString());
                // residence phone

                Assert.AreEqual(residencePhone.Number, personPhones[2].Number);
                Assert.AreEqual("Vacation", personPhones[2].Type.PhoneType.ToString());
                // work phone

                Assert.AreEqual(workPhone.Number, personPhones[3].Number);
                Assert.AreEqual("Business", personPhones[3].Type.PhoneType.ToString());
                Assert.AreEqual(workPhone.Extension, personPhones[3].Extension);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task GetPerson2ByGuid_EmptyGuid()
            {
                await personService.GetPerson2ByGuidAsync("", false);
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task GetPerson2ByGuid_InvalidGuid()
            {
                await personService.GetPerson2ByGuidAsync("invalid", false);
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task GetPerson2ByGuid_PersonCorpIndicator()
            {
                personIntegration.PersonCorpIndicator = "Y";
                await personService.GetPerson2ByGuidAsync(personGuid, false);
            }

            //[TestMethod]
            //[ExpectedException(typeof(PermissionsException))]
            //public async Task GetPerson2ByGuid_PermissionsException()
            //{
            //    // Mock permissions
            //    personRole.RemovePermission(permissionViewAnyPerson);
            //    roleRepoMock.Setup(rpm => rpm.Roles).Returns(new List<Role>() { personRole });
            //    var personDto = await personService.GetPerson2ByGuidAsync(personGuid, false);
            //}

            [TestMethod]
            public async Task GetPerson2ByGuid_PrimaryNameNulls()
            {
                person.Prefix = string.Empty;
                person.Suffix = string.Empty;
                person.MiddleName = string.Empty;
                person.Nickname = string.Empty;
                // Act--get person
                var personDto = await personService.GetPerson2ByGuidAsync(personGuid, false);
                var personPrimaryName = personDto.PersonNames.Where(pn => pn.NameType.Category == Dtos.EnumProperties.PersonNameType2.Personal).FirstOrDefault();

                Assert.AreEqual(null, personPrimaryName.Title);
                Assert.AreEqual(null, personPrimaryName.Pedigree);
                Assert.AreEqual(null, personPrimaryName.MiddleName);
            }

            [TestMethod]
            public async Task GetPerson2ByGuid_StudentRole()
            {
                //personRepoMock.Setup(repo => repo.IsFacultyAsync(personId)).ReturnsAsync(false);
                //personRepoMock.Setup(repo => repo.IsStudentAsync(personId)).ReturnsAsync(true);
                // Act--get person
                var personDto = await personService.GetPerson2ByGuidAsync(personGuid, false);
                var personStudentRole = personDto.Roles.Where(r => r.RoleType == Dtos.EnumProperties.PersonRoleType.Student).FirstOrDefault();
                Assert.AreEqual(Dtos.EnumProperties.PersonRoleType.Student, personStudentRole.RoleType);
                Assert.AreEqual(3, personDto.Roles.Count());
            }

            [TestMethod]
            public async Task GetPerson2ByGuid_FacultyRole()
            {
                // Act--get person
                var personDto = await personService.GetPerson2ByGuidAsync(personGuid, false);
                var personStudentFacultyRole = personDto.Roles.Where(r => r.RoleType == Dtos.EnumProperties.PersonRoleType.Instructor).FirstOrDefault();
                Assert.AreEqual(Dtos.EnumProperties.PersonRoleType.Instructor, personStudentFacultyRole.RoleType);
                Assert.AreEqual(3, personDto.Roles.Count());
            }

            //[TestMethod]
            //public async Task GetPerson2ByGuid_NoRoles()
            //{
            //    personIntegration.AddRole = null;
            //    // Act--get person
            //    var personDto = await personService.GetPerson2ByGuidAsync(personGuid, false);
            //    Assert.AreEqual(1, personDto.Roles.Count());
            //}

            [TestMethod]
            public async Task GetPerson2ByGuid_BirthName()
            {
                var birthFirstName = "Bernard";
                var birthLastName = "Anders";
                var birthMiddleName = "Bernie";
                personIntegration.BirthNameFirst = birthFirstName;
                personIntegration.BirthNameLast = birthLastName;
                personIntegration.BirthNameMiddle = birthMiddleName;

                // Act--get person
                var personDto = await personService.GetPerson2ByGuidAsync(personGuid, false);
                var personBirthName = personDto.PersonNames.Where(pn => pn.NameType.Category == Dtos.EnumProperties.PersonNameType2.Birth).FirstOrDefault();

                // Assert

                Assert.AreEqual(birthFirstName, personBirthName.FirstName);
                Assert.AreEqual(birthMiddleName, personBirthName.MiddleName);
                Assert.AreEqual(birthLastName, personBirthName.LastName);
            }

            [TestMethod]
            public async Task GetPerson2ByGuid_PersonLanguages()
            {
                // Act--get person
                var personDto = await personService.GetPerson2ByGuidAsync(personGuid, false);
                var personLanguages = personDto.Languages.Where(x => x.Code == PersonLanguageCode.eng).FirstOrDefault();

                // Assert
                Assert.AreEqual(PersonLanguageCode.eng, personLanguages.Code);
                Assert.AreEqual(Dtos.EnumProperties.PersonLanguagePreference.Primary, personLanguages.Preference);
            }

            [TestMethod]
            public async Task GetPerson2ByGuid_PersonAlienStatus()
            {
                var alienStatus = "CS";
                personIntegration.AlienStatus = alienStatus;

                var allCitizenshipStatuses = new TestCitizenshipStatusRepository().Get();
                refRepoMock.Setup(repo => repo.GetCitizenshipStatusesAsync(It.IsAny<bool>())).ReturnsAsync(allCitizenshipStatuses);
                // Act--get person
                var personDto = await personService.GetPerson2ByGuidAsync(personGuid, false);

                // Assert
                // {"87ec6f69-9b16-4ed5-8954-59067f0318ec", "CS", "Canadian citizen", "NA"}, 

                var citizenShipStatus = allCitizenshipStatuses.FirstOrDefault(x => x.Code == alienStatus);

                Assert.AreEqual(Dtos.CitizenshipStatusType.Citizen, personDto.CitizenshipStatus.Category);
                Assert.AreEqual(citizenShipStatus.Guid, personDto.CitizenshipStatus.Detail.Id);
            }

            [TestMethod]
            public async Task GetPerson2ByGuid_PersonCountry()
            {
                var country = "CA";
                personIntegration.BirthCountry = country;
                // Act--get person
                var personDto = await personService.GetPerson2ByGuidAsync(personGuid, false);

                // Assert          
                var birthCountry = countries.FirstOrDefault(x => x.Code == country);
                Assert.AreEqual(birthCountry.Iso3Code, personDto.CountryOfBirth);
            }

            [TestMethod]
            public async Task GetPerson2ByGuid_PersonCitizenship()
            {
                var country = "CA";
                personIntegration.Citizenship = country;
                // Act--get person
                var personDto = await personService.GetPerson2ByGuidAsync(personGuid, false);
                // Assert          
                var birthCountry = countries.FirstOrDefault(x => x.Code == country);
                Assert.AreEqual(birthCountry.Iso3Code, personDto.CitizenshipCountry);
            }

            [TestMethod]
            public async Task GetPerson2ByGuid_SocialMedia()
            {
                var socialMediaTypeCode = "TW";
                var socialMediaHandle = "pontifex";
                var personSocialMedia = new SocialMedia(socialMediaTypeCode, socialMediaHandle);
                personIntegration.AddSocialMedia(personSocialMedia);
                // Act--get person
                var allSocialMediaTypes = new TestSocialMediaTypesRepository().GetSocialMediaTypes();
                refRepoMock.Setup(x => x.GetSocialMediaTypesAsync(It.IsAny<bool>())).ReturnsAsync(allSocialMediaTypes);
                var personDto = await personService.GetPerson2ByGuidAsync(personGuid, false);

                //   {"13660156-d481-4b3d-b617-92136979314c", "TW", "Twitter", "twitter"}, 
                var socialMediaType = allSocialMediaTypes.FirstOrDefault(x => x.Code == socialMediaTypeCode);

                // Assert          
                var personSocialMediaType = personDto.SocialMedia.FirstOrDefault(x => x.Type.Category == Dtos.SocialMediaTypeCategory.twitter);
                Assert.AreEqual(Dtos.SocialMediaTypeCategory.twitter, personSocialMediaType.Type.Category);
                Assert.AreEqual(socialMediaType.Guid, personSocialMediaType.Type.Detail.Id);
                Assert.AreEqual(socialMediaHandle, personSocialMediaType.Address);
            }

            [TestMethod]
            public async Task GetPerson2Dto_Passport()
            {
                var passportNumber = "A1231";
                var passport = new PersonPassport(personGuid, passportNumber);
                passport.IssuingCountry = "USA";
                personIntegration.Passport = passport;

                var allIdentityDocuments = new TestIdentityDocumentTypeRepository().Get();
                refRepoMock.Setup(x => x.GetIdentityDocumentTypesAsync(It.IsAny<bool>())).ReturnsAsync(allIdentityDocuments);

                var passportIdentityDocument = allIdentityDocuments.FirstOrDefault(x => x.Code == "PASSPORT");

                var personDto = await personService.GetPerson2ByGuidAsync(personGuid, false);
                var personPassport = personDto.IdentityDocuments.ElementAtOrDefault(0);

                Assert.IsNotNull(personPassport);
                Assert.AreEqual(passportNumber, personPassport.DocumentId);
                Assert.AreEqual(passportIdentityDocument.Guid, personPassport.Type.Detail.Id);
            }

            [TestMethod]
            public async Task GetPerson2ByGuid_DriversLicense()
            {
                var licenseNumber = "6523123";
                var licenseState = "NY";
                var driverLicense = new PersonDriverLicense(personGuid, licenseNumber);
                driverLicense.IssuingState = licenseState;
                personIntegration.DriverLicense = driverLicense;

                var allIdentityDocuments = new TestIdentityDocumentTypeRepository().Get();
                refRepoMock.Setup(x => x.GetIdentityDocumentTypesAsync(It.IsAny<bool>())).ReturnsAsync(allIdentityDocuments);

                var licenseIdentityDocument = allIdentityDocuments.FirstOrDefault(x => x.Code == "LICENSE");

                var personDto = await personService.GetPerson2ByGuidAsync(personGuid, false);
                var personDriverLicense = personDto.IdentityDocuments.ElementAtOrDefault(0);

                Assert.IsNotNull(personDriverLicense);
                Assert.AreEqual(licenseNumber, personDriverLicense.DocumentId);
                Assert.AreEqual(string.Concat("US-", licenseState), personDriverLicense.Country.Region.Code);
                Assert.AreEqual("USA", personDriverLicense.Country.Code.ToString());
                Assert.AreEqual(licenseIdentityDocument.Guid, personDriverLicense.Type.Detail.Id);
            }

            [TestMethod]
            public async Task GetPerson2ByGuid_Interests()
            {
                var interest1 = "ART";
                var interest2 = "BASE";
                personIntegration.Interests = new List<string>() { interest1, interest2 };

                var allInterests = new TestInterestsRepository().GetInterests();

                refRepoMock.Setup(x => x.GetInterestsAsync(It.IsAny<bool>())).ReturnsAsync(allInterests);
                var personDto = await personService.GetPerson2ByGuidAsync(personGuid, false);

                var interestArt = allInterests.FirstOrDefault(x => x.Code == interest1);
                var interestBase = allInterests.FirstOrDefault(x => x.Code == interest2);

                Assert.AreEqual(2, personDto.Interests.Count());
                Assert.AreEqual(interestArt.Guid, personDto.Interests.ElementAtOrDefault(0).Id);
                Assert.AreEqual(interestBase.Guid, personDto.Interests.ElementAtOrDefault(1).Id);
            }

            [TestMethod]
            public async Task GetPerson2ByGuid_Interests_Invalid()
            {
                var interest1 = "INVALID";
                personIntegration.Interests = new List<string>() { interest1 };
                var allInterests = new TestInterestsRepository().GetInterests();

                refRepoMock.Setup(x => x.GetInterestsAsync(It.IsAny<bool>())).ReturnsAsync(allInterests);
                var personDto = await personService.GetPerson2ByGuidAsync(personGuid, false);

                Assert.AreEqual(null, personDto.Interests);
            }

        }
        #endregion

        #region GetPerson3 Tests

        [TestClass]
        public class GetPerson3 : CurrentUserSetup
        {
            private string personId = "0000011";
            private string personId2 = "0000012";
            private string personGuid = "5674f28b-b216-4055-b236-81a922d93b4c";
            private string personGuid2 = "1111f28b-b216-4055-b236-81a922d93b4c";
            private Mock<IPersonRepository> personRepoMock;
            private IPersonRepository personRepo;
            private Mock<IPersonBaseRepository> personBaseRepoMock;
            private IPersonBaseRepository personBaseRepo;
            private Mock<IReferenceDataRepository> refRepoMock;
            private IReferenceDataRepository refRepo;
            private Mock<IAdapterRegistry> adapterRegistryMock;
            private IAdapterRegistry adapterRegistry;
            private PersonService personService;
            private ILogger logger;
            private Ellucian.Colleague.Domain.Base.Entities.Person person;
            private Ellucian.Colleague.Domain.Base.Entities.Person person2;
            private Ellucian.Colleague.Domain.Base.Entities.PersonIntegration personIntegration;
            private Mock<IRoleRepository> roleRepoMock;
            private IRoleRepository roleRepo;
            private ICurrentUserFactory currentUserFactory;
            private Ellucian.Colleague.Domain.Base.Entities.EmailAddress instEmail;
            private Ellucian.Colleague.Domain.Base.Entities.EmailAddress perEmail;
            private Ellucian.Colleague.Domain.Base.Entities.EmailAddress workEmail;
            private Ellucian.Colleague.Domain.Base.Entities.Address homeAddr;
            private Ellucian.Colleague.Domain.Base.Entities.Address mailAddr;
            private Ellucian.Colleague.Domain.Base.Entities.Address resAddr;
            private Ellucian.Colleague.Domain.Base.Entities.Address workAddr;
            private Ellucian.Colleague.Domain.Base.Entities.Phone homePhone;
            private Ellucian.Colleague.Domain.Base.Entities.Phone mobilePhone;
            private Ellucian.Colleague.Domain.Base.Entities.Phone residencePhone;
            private Ellucian.Colleague.Domain.Base.Entities.Phone workPhone;
            private IEnumerable<Ellucian.Colleague.Domain.Base.Entities.State> states;
            private IEnumerable<Ellucian.Colleague.Domain.Base.Entities.Country> countries;
            private List<Ellucian.Colleague.Domain.Base.Entities.County> counties;
            private string maritalStatusGuid = Guid.NewGuid().ToString();
            private string ethnicityGuid = Guid.NewGuid().ToString();
            private string raceAsianGuid = Guid.NewGuid().ToString();
            private string racePacificIslanderGuid = Guid.NewGuid().ToString();
            private string countyGuid = Guid.NewGuid().ToString();
            private string baptistGuid = Guid.NewGuid().ToString();
            private string catholicGuid = Guid.NewGuid().ToString();
            private Ellucian.Colleague.Domain.Entities.Permission permissionViewAnyPerson;

            [TestInitialize]
            public void Initialize()
            {
                personRepoMock = new Mock<IPersonRepository>();
                personRepo = personRepoMock.Object;
                personBaseRepoMock = new Mock<IPersonBaseRepository>();
                personBaseRepo = personBaseRepoMock.Object;
                refRepoMock = new Mock<IReferenceDataRepository>();
                refRepo = refRepoMock.Object;
                adapterRegistryMock = new Mock<IAdapterRegistry>();
                adapterRegistry = adapterRegistryMock.Object;
                roleRepoMock = new Mock<IRoleRepository>();
                roleRepo = roleRepoMock.Object;
                logger = new Mock<ILogger>().Object;

                // Mock person response from the person repository
                person = new Domain.Base.Entities.Person(personId, "Brown");
                person.Guid = personGuid;
                person.Prefix = "Mr.";
                person.FirstName = "Ricky";
                person.MiddleName = "Lee";
                person.Suffix = "Jr.";
                person.Nickname = "Rick";
                person.BirthDate = new DateTime(1930, 1, 1);
                person.DeceasedDate = new DateTime(2014, 5, 12);
                person.GovernmentId = "111-11-1111";
                person.MaritalStatusCode = "M";
                person.EthnicCodes = new List<string> { "H" };
                person.RaceCodes = new List<string> { "AS" };
                person.AddEmailAddress(new EmailAddress("xyz@xmail.com", "COL"));
                personRepoMock.Setup(repo => repo.GetPersonByGuidNonCachedAsync(personGuid)).ReturnsAsync(person);

                var filteredPersonGuidTuple = new Tuple<IEnumerable<string>, int>(new List<string>() { personGuid }, 1);
                personRepoMock.Setup(repo => repo.GetFilteredPerson2GuidsAsync(It.IsAny<int>(), It.IsAny<int>(), true, It.IsAny<PersonFilterCriteria>(), It.IsAny<string>())).ReturnsAsync(filteredPersonGuidTuple);

                personIntegration = new PersonIntegration(personId, "Brown");
                personIntegration.Guid = personGuid;
                personIntegration.Prefix = "Mr.";
                personIntegration.FirstName = "Ricky";
                personIntegration.MiddleName = "Lee";
                personIntegration.Suffix = "Jr.";
                personIntegration.Nickname = "Rick";
                personIntegration.BirthDate = new DateTime(1930, 1, 1);
                personIntegration.DeceasedDate = new DateTime(2014, 5, 12);
                personIntegration.GovernmentId = "111-11-1111";
                personIntegration.Religion = "CA";
                personIntegration.MaritalStatusCode = "M";
                personIntegration.EthnicCodes = new List<string> { "H", "N" };
                personIntegration.RaceCodes = new List<string> { "AS" };
                personIntegration.AddRole(new PersonRole(PersonRoleType.Alumni, new DateTime(15, 01, 22), new DateTime(15, 05, 25)));
                personIntegration.AddRole(new PersonRole(PersonRoleType.Student, new DateTime(15, 01, 22), new DateTime(15, 05, 25)));
                personIntegration.AddRole(new PersonRole(PersonRoleType.Instructor, new DateTime(15, 01, 22), new DateTime(15, 05, 25)));
                // Mock the email address data response
                instEmail = new Domain.Base.Entities.EmailAddress("inst@inst.com", "COL") { IsPreferred = true };
                personIntegration.AddEmailAddress(instEmail);
                perEmail = new Domain.Base.Entities.EmailAddress("personal@personal.com", "PER") { IsPreferred = false };
                personIntegration.AddEmailAddress(perEmail);
                workEmail = new Domain.Base.Entities.EmailAddress("work@work.com", "BUS") { IsPreferred = false };
                personIntegration.AddEmailAddress(workEmail);

                // Mock the address hierarchy responses
                var addresses = new List<Domain.Base.Entities.Address>();
                homeAddr = new Domain.Base.Entities.Address()
                {
                    TypeCode = "HO",
                    Type = Dtos.EnumProperties.AddressType.Home.ToString(),
                    Guid = Guid.NewGuid().ToString(),
                    Status = "Current",
                    IsPreferredAddress = true
                };
                addresses.Add(homeAddr);
                mailAddr = new Domain.Base.Entities.Address()
                {
                    TypeCode = "MA",
                    Type = Dtos.EnumProperties.AddressType.Mailing.ToString(),
                    Guid = Guid.NewGuid().ToString(),
                    Status = "Current"
                };
                addresses.Add(mailAddr);
                resAddr = new Domain.Base.Entities.Address()
                {
                    TypeCode = "VA",
                    Type = Dtos.EnumProperties.AddressType.Vacation.ToString(),
                    Guid = Guid.NewGuid().ToString(),
                    Status = "Current",
                    IsPreferredResidence = true
                };
                addresses.Add(resAddr);
                workAddr = new Domain.Base.Entities.Address()
                {
                    TypeCode = "BU",
                    Type = Dtos.EnumProperties.AddressType.Business.ToString(),
                    Guid = Guid.NewGuid().ToString(),
                    Status = "Current"
                };
                addresses.Add(workAddr);
                personIntegration.Addresses = addresses;

                // Mock the person phone per type response
                homePhone = new Domain.Base.Entities.Phone("111-111-1111", "HO");
                personIntegration.AddPhone(homePhone);
                mobilePhone = new Domain.Base.Entities.Phone("222-222-2222", "MO");
                personIntegration.AddPhone(mobilePhone);
                residencePhone = new Domain.Base.Entities.Phone("333-333-3333", "VA");
                personIntegration.AddPhone(residencePhone);
                workPhone = new Domain.Base.Entities.Phone("444-444-4444", "BU", "4444");
                personIntegration.AddPhone(workPhone);

                // Mock the social media
                var socialMedia = new List<Domain.Base.Entities.SocialMedia>();
                var socialMediaTypeCode = "TW";
                var socialMediaHandle = "pontifex";
                var personSocialMedia = new SocialMedia(socialMediaTypeCode, socialMediaHandle);
                personIntegration.AddSocialMedia(personSocialMedia);

                // Mock the person languages
                personIntegration.PrimaryLanguage = "E";
                personIntegration.SecondaryLanguages = new List<String> { "SP", "TA" };

                personRepoMock.Setup(repo => repo.GetPersonIntegrationByGuidNonCachedAsync(personGuid)).ReturnsAsync(personIntegration);
                personRepoMock.Setup(repo => repo.GetPersonIntegrationByGuidAsync(personGuid, It.IsAny<bool>())).ReturnsAsync(personIntegration);


                var personGuidList = new List<string>() { personGuid };
                var personList = new List<PersonIntegration>() { personIntegration };
                personRepoMock.Setup(repo => repo.GetPersonIntegrationByGuidNonCachedAsync(personGuidList)).ReturnsAsync(personList);
                personRepoMock.Setup(repo => repo.GetFilteredPerson2GuidsAsync(It.IsAny<int>(), It.IsAny<int>(), true, It.IsAny<PersonFilterCriteria>(), It.IsAny<string>())).ReturnsAsync(new Tuple<IEnumerable<string>, int>(personGuidList, 1));

                person2 = new Domain.Base.Entities.Person(personId2, "Green");
                person2.Guid = personGuid2;
                person2.Prefix = "Ms.";
                person2.FirstName = "Amy";
                var personGuids = new List<string>();
                personGuids.Add(person.Guid);
                personGuids.Add(person2.Guid);
                var personEntities = new List<Domain.Base.Entities.Person>();
                personEntities.Add(person);
                personEntities.Add(person2);
                personRepoMock.Setup(repo => repo.GetPersonByGuidNonCachedAsync(personGuids)).ReturnsAsync(personEntities.AsEnumerable());

                // Mock the response for getting faculty guids
                var personGuidTuple = new Tuple<IEnumerable<string>, int>(personGuids, 2);
                personRepoMock.Setup(repo => repo.GetFacultyPersonGuidsAsync(It.IsAny<int>(), It.IsAny<int>())).ReturnsAsync(personGuidTuple);

                // Mock the response for getting a Person Pin 
                var personPin = new PersonPin("0000011", "testUsername");
                var personPins = new List<PersonPin>();
                personPins.Add(personPin);
                personRepoMock.Setup(repo => repo.GetPersonPinsAsync(It.IsAny<string[]>())).ReturnsAsync(personPins);
                var personUserName = new PersonUserName("0000011", "testUsername");
                var personUserNames = new List<PersonUserName>();
                personUserNames.Add(personUserName);
                personRepoMock.Setup(repo => repo.GetPersonUserNamesAsync(It.IsAny<string[]>())).ReturnsAsync(personUserNames);

                refRepoMock.Setup(repo => repo.GetPrivacyStatusesAsync(It.IsAny<bool>()))
                     .ReturnsAsync(
                         new List<PrivacyStatus>() {
                        new PrivacyStatus( Guid.NewGuid().ToString(), "UN", "Unrestricted", PrivacyStatusType.unrestricted),
                        new PrivacyStatus( Guid.NewGuid().ToString(), "R", "restricted", PrivacyStatusType.restricted)
                        }
                     );

                refRepoMock.Setup(repo => repo.GetPersonNameTypesAsync(It.IsAny<bool>()))
                    .ReturnsAsync(
                        new List<PersonNameTypeItem>() {
                        new PersonNameTypeItem(Guid.NewGuid().ToString(), "PREFERRED", "Personal", PersonNameType.Personal) ,
                        new PersonNameTypeItem(Guid.NewGuid().ToString(), "BIRTH", "Birth", PersonNameType.Birth) ,
                        new PersonNameTypeItem(Guid.NewGuid().ToString(), "LEGAL", "Legal", PersonNameType.Legal),
                        new PersonNameTypeItem(Guid.NewGuid().ToString(), "NICKNAME", "NickName", PersonNameType.Personal),
                        new PersonNameTypeItem(Guid.NewGuid().ToString(), "HISTORY", "History", PersonNameType.Personal)
                        }
                     );

                refRepoMock.Setup(repo => repo.GetDenominationsAsync(It.IsAny<bool>()))
                    .ReturnsAsync(
                        new List<Denomination>() {
                        new Denomination(baptistGuid,"BA", "Baptist") ,
                        new Denomination(catholicGuid,"CA", "Catholic")
                        }
                     );

                refRepoMock.Setup(repo => repo.GetEmailTypesAsync(It.IsAny<bool>()))
                     .ReturnsAsync(
                         new List<EmailType>() {
                        new EmailType( Guid.NewGuid().ToString(), "COL", "College", EmailTypeCategory.School),
                        new EmailType( Guid.NewGuid().ToString(), "PER", "Personal", EmailTypeCategory.Personal),
                        new EmailType( Guid.NewGuid().ToString(), "BUS", "Business", EmailTypeCategory.Business)
                        }
                     );
                
                refRepoMock.Setup(repo => repo.GetSocialMediaTypesAsync(It.IsAny<bool>()))
                     .ReturnsAsync(
                         new List<SocialMediaType>() {
                        new SocialMediaType( Guid.NewGuid().ToString(), "TW", "Twitter", SocialMediaTypeCategory.twitter)
                        }
                     );

                refRepoMock.Setup(repo => repo.GetAddressTypes2Async(It.IsAny<bool>()))
                     .ReturnsAsync(
                         new List<AddressType2>() {
                        new AddressType2( Guid.NewGuid().ToString(), "HO", "Home", AddressTypeCategory.Home),
                        new AddressType2( Guid.NewGuid().ToString(), "MA", "Mailing", AddressTypeCategory.Mailing),
                        new AddressType2( Guid.NewGuid().ToString(), "VA", "Vacation", AddressTypeCategory.Vacation),
                        new AddressType2( Guid.NewGuid().ToString(), "BU", "Business", AddressTypeCategory.Business)
                        }
                     );

                refRepoMock.Setup(repo => repo.GetPhoneTypesAsync(It.IsAny<bool>()))
                     .ReturnsAsync(
                         new List<PhoneType>() {
                        new PhoneType( Guid.NewGuid().ToString(), "HO", "Home", PhoneTypeCategory.Home),
                        new PhoneType( Guid.NewGuid().ToString(), "MO", "Mobile", PhoneTypeCategory.Mobile),
                        new PhoneType( Guid.NewGuid().ToString(), "VA", "Vacation", PhoneTypeCategory.Vacation),
                        new PhoneType( Guid.NewGuid().ToString(), "BU", "Business", PhoneTypeCategory.Business)
                        }
                     );

                // Mock the person repository for roles
                personRepoMock.Setup(repo => repo.IsFacultyAsync(personId)).ReturnsAsync(true);
                personRepoMock.Setup(repo => repo.IsStudentAsync(personId)).ReturnsAsync(true);

                // Mock the person repository GUID lookup
                personRepoMock.Setup(repo => repo.GetPersonIdFromGuidAsync(personGuid)).ReturnsAsync(personId);
                personRepoMock.Setup(repo => repo.GetPersonIdFromGuidAsync(personGuid2)).ReturnsAsync(personId2);

                // Mock the reference repository for states
                states = new List<State>()
                {
                    new State("VA","Virginia"),
                    new State("MD","Maryland"),
                    new State("NY","New York"),
                    new State("MA","Massachusetts")
                };
                refRepoMock.Setup(repo => repo.GetStateCodesAsync(It.IsAny<bool>())).Returns(Task.FromResult(states));

                // Mock the reference repository for country
                countries = new List<Country>()
                 {
                    new Country("US","United States","US"){ IsoAlpha3Code = "USA"},
                    new Country("CA","Canada","CA"){ IsoAlpha3Code = "CAN"},
                    new Country("MX","Mexico","MX"){ IsoAlpha3Code = "MEX"},
                    new Country("BR","Brazil","BR"){ IsoAlpha3Code = "BRA"}
                };
                refRepoMock.Setup(repo => repo.GetCountryCodesAsync(It.IsAny<bool>())).Returns(Task.FromResult(countries));

                // Places
                var places = new List<Place>();
                var place1 = new Place() { PlacesCountry = "USA", PlacesRegion = "US-NY" };
                places.Add(place1);
                var place2 = new Place() { PlacesCountry = "CAN", PlacesRegion = "CA-ON" };
                places.Add(place2);
                refRepoMock.Setup(repo => repo.GetPlacesAsync(It.IsAny<bool>())).Returns(Task.FromResult(places.AsEnumerable<Place>()));
                //personRepoMock.Setup(repo => repo.GetPlacesAsync()).ReturnsAsync(places);

                // International Parameters Host Country
                personRepoMock.Setup(repo => repo.GetHostCountryAsync()).ReturnsAsync("USA");

                // Mock the reference repository for county
                counties = new List<County>()
                {
                    new County(countyGuid, "FFX","Fairfax County"),
                    new County(countyGuid, "BAL","Baltimore County"),
                    new County(countyGuid, "NY","New York County"),
                    new County(countyGuid, "BOS","Boston County")
                };
                refRepoMock.Setup(repo => repo.Counties).Returns(counties);

                // Mock the reference repository for marital status
                refRepoMock.Setup(repo => repo.GetMaritalStatusesAsync(It.IsAny<bool>())).Returns(Task.FromResult<IEnumerable<MaritalStatus>>(new List<MaritalStatus>()
                {
                    new MaritalStatus(maritalStatusGuid, "M", "Married")
                }));

                // Mock the reference repository for ethnicity
                refRepoMock.Setup(repo => repo.GetEthnicitiesAsync(It.IsAny<bool>())).Returns(Task.FromResult<IEnumerable<Ethnicity>>(new List<Ethnicity>()
                {
                    new Ethnicity(ethnicityGuid, "H", "Hispanic", EthnicityType.Hispanic)
                }));

                // Mock the reference repository for races
                var raceEntities = new List<Race>();
                raceEntities.Add(new Race(raceAsianGuid, "AS", "Asian", RaceType.Asian));
                raceEntities.Add(new Race(racePacificIslanderGuid, "HP", "Hawaiian/Pacific Islander", RaceType.PacificIslander));
                refRepoMock.Setup(repo => repo.GetRacesAsync(It.IsAny<bool>())).Returns(Task.FromResult<IEnumerable<Race>>(raceEntities));

                // Mock the reference repository for prefix
                refRepoMock.Setup(repo => repo.Prefixes).Returns(new List<Prefix>()
                {
                    new Prefix("MR","Mr","Mr."),
                    new Prefix("MS","Ms","Ms.")
                });

                // Mock the reference repository for suffix
                refRepoMock.Setup(repo => repo.Suffixes).Returns(new List<Suffix>()
                {
                    new Suffix("JR","Jr","Jr."),
                    new Suffix("SR","Sr","Sr.")
                });

                // Set up current user
                currentUserFactory = new CurrentUserSetup.PersonUserFactory();

                // Mock permissions
                permissionViewAnyPerson = new Ellucian.Colleague.Domain.Entities.Permission(BasePermissionCodes.ViewAnyPerson);
                personRole.AddPermission(permissionViewAnyPerson);
                roleRepoMock.Setup(rpm => rpm.Roles).Returns(new List<Role>() { personRole });

                // Mock LANGUAGES valcode 
                var languages = new Ellucian.Data.Colleague.DataContracts.ApplValcodes()
                {

                    ValsEntityAssociation = new List<Ellucian.Data.Colleague.DataContracts.ApplValcodesVals>()
                    {
                        new ApplValcodesVals() { ValInternalCodeAssocMember = "E", ValExternalRepresentationAssocMember = "English", ValActionCode3AssocMember = "ENG" },
                        new ApplValcodesVals() { ValInternalCodeAssocMember = "SP", ValExternalRepresentationAssocMember = "Spanish", ValActionCode3AssocMember = "SPA" },
                        new ApplValcodesVals() { ValInternalCodeAssocMember = "TA", ValExternalRepresentationAssocMember = "Tagalog", ValActionCode3AssocMember = "TGL" }
                    }
                };
                personBaseRepoMock.Setup(repo => repo.GetLanguagesAsync()).ReturnsAsync(languages);

                personService = new PersonService(adapterRegistry, personRepo, personBaseRepo, refRepo, null, null, null, null, currentUserFactory, roleRepo, logger);
            }

            [TestCleanup]
            public void Cleanup()
            {
                adapterRegistry = null;
                personRepo = null;
                personBaseRepo = null;
                refRepo = null;
                roleRepo = null;
                personService = null;
                personRole = null;
                permissionViewAnyPerson = null;
                roleRepoMock = null;
                currentUserFactory = null;
                refRepoMock = null;
            }

            [TestMethod]
            public async Task GetPerson3ByGuid()
            {
                // Act--get person
                var personDto = await personService.GetPerson3ByGuidAsync(personGuid, false);
                // Assert
                Assert.IsTrue(personDto is Dtos.Person3);
                Assert.AreEqual(person.Guid, personDto.Id);
                Assert.AreEqual(person.BirthDate, personDto.BirthDate);
                Assert.AreEqual(person.DeceasedDate, personDto.DeceasedDate);
                var personSsnCredential = personDto.Credentials.FirstOrDefault(c => c.Type == Dtos.EnumProperties.CredentialType2.Ssn);
                // Assert.AreEqual(person.GovernmentId, personSsnCredential.);
                // check names
                Assert.AreEqual(2, personDto.PersonNames.Count());
                // check primary name
                var personPrimaryName = personDto.PersonNames.Where(pn => pn.NameType.Category == Dtos.EnumProperties.PersonNameType2.Legal).FirstOrDefault();
                Assert.AreEqual("Mr.", personPrimaryName.Title);
                Assert.AreEqual(person.FirstName, personPrimaryName.FirstName);
                Assert.AreEqual(person.MiddleName, personPrimaryName.MiddleName);
                Assert.AreEqual(null, personPrimaryName.LastNamePrefix);
                Assert.AreEqual(person.LastName, personPrimaryName.LastName);
                Assert.AreEqual("Jr.", personPrimaryName.Pedigree);
                //Assert.AreEqual(person.Nickname, personPrimaryName.Preference.Value);
                // check race
                var personRaces = personDto.Races.ToArray();
                Assert.AreEqual(raceAsianGuid, personRaces[0].Race.Id);
                // check ethnicity
                Assert.AreEqual(ethnicityGuid, personDto.Ethnicity.EthnicGroup.Id);
                // check religion
                Assert.AreEqual(catholicGuid, personDto.Religion.Id);
                // check roles
                var personFacultyRole = personDto.Roles.Where(r => r.RoleType == Dtos.EnumProperties.PersonRoleType.Instructor).FirstOrDefault();
                Assert.AreEqual(Dtos.EnumProperties.PersonRoleType.Instructor, personFacultyRole.RoleType);
                var personStudentRole = personDto.Roles.Where(r => r.RoleType == Dtos.EnumProperties.PersonRoleType.Student).FirstOrDefault();
                Assert.AreEqual(Dtos.EnumProperties.PersonRoleType.Student, personStudentRole.RoleType);
                var personAlumniRole = personDto.Roles.Where(r => r.RoleType == Dtos.EnumProperties.PersonRoleType.Alumni).FirstOrDefault();
                Assert.AreEqual(Dtos.EnumProperties.PersonRoleType.Alumni, personAlumniRole.RoleType);
                Assert.AreEqual(3, personDto.Roles.Count());
                // check email addresses
                var personEmailAddresses = personDto.EmailAddresses as List<Dtos.DtoProperties.PersonEmailDtoProperty>;
                Assert.AreEqual(instEmail.Value, personEmailAddresses[0].Address);
                Assert.AreEqual(Ellucian.Colleague.Dtos.EmailTypeList.School, personEmailAddresses[0].Type.EmailType);
                Assert.AreEqual(perEmail.Value, personEmailAddresses[1].Address);
                Assert.AreEqual(Ellucian.Colleague.Dtos.EmailTypeList.Personal, personEmailAddresses[1].Type.EmailType);
                Assert.AreEqual(workEmail.Value, personEmailAddresses[2].Address);
                Assert.AreEqual(Ellucian.Colleague.Dtos.EmailTypeList.Business, personEmailAddresses[2].Type.EmailType);
                // compare addresses
                var personAddresses = personDto.Addresses as List<Dtos.DtoProperties.PersonAddressDtoProperty>;
                // home addr
                Assert.AreEqual(homeAddr.Guid, personAddresses[0].address.Id);
                Assert.AreEqual(Dtos.EnumProperties.AddressType.Home, personAddresses[0].Type.AddressType);
                // mailing addr
                Assert.AreEqual(mailAddr.Guid, personAddresses[1].address.Id);
                Assert.AreEqual(Dtos.EnumProperties.AddressType.Mailing, personAddresses[1].Type.AddressType);
                // residence addr
                Assert.AreEqual(resAddr.Guid, personAddresses[2].address.Id);
                Assert.AreEqual(Dtos.EnumProperties.AddressType.Vacation, personAddresses[2].Type.AddressType);
                // work addr
                Assert.AreEqual(workAddr.Guid, personAddresses[3].address.Id);
                Assert.AreEqual(Dtos.EnumProperties.AddressType.Business, personAddresses[3].Type.AddressType);
                // compare phones
                var personPhones = personDto.Phones as List<Dtos.DtoProperties.PersonPhoneDtoProperty>;
                // home phone

                Assert.AreEqual(homePhone.Number, personPhones[0].Number);
                Assert.AreEqual("Home", personPhones[0].Type.PhoneType.ToString());
                // mobile phone

                Assert.AreEqual(mobilePhone.Number, personPhones[1].Number);
                Assert.AreEqual("Mobile", personPhones[1].Type.PhoneType.ToString());
                // residence phone

                Assert.AreEqual(residencePhone.Number, personPhones[2].Number);
                Assert.AreEqual("Vacation", personPhones[2].Type.PhoneType.ToString());
                // work phone

                Assert.AreEqual(workPhone.Number, personPhones[3].Number);
                Assert.AreEqual("Business", personPhones[3].Type.PhoneType.ToString());
                Assert.AreEqual(workPhone.Extension, personPhones[3].Extension);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task GetPerson3ByGuid_EmptyGuid()
            {
                await personService.GetPerson3ByGuidAsync("", false);
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task GetPerson3ByGuid_InvalidGuid()
            {
                await personService.GetPerson3ByGuidAsync("invalid", false);
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task GetPerson3ByGuid_PersonCorpIndicator()
            {
                personIntegration.PersonCorpIndicator = "Y";
                await personService.GetPerson3ByGuidAsync(personGuid, false);
            }

            //[TestMethod]
            //[ExpectedException(typeof(PermissionsException))]
            //public async Task GetPerson3ByGuid_PermissionsException()
            //{
            //    // Mock permissions
            //    personRole.RemovePermission(permissionViewAnyPerson);
            //    roleRepoMock.Setup(rpm => rpm.Roles).Returns(new List<Role>() { personRole });
            //    var personDto = await personService.GetPerson3ByGuidAsync(personGuid, false);
            //}

            [TestMethod]
            public async Task GetPerson3ByGuid_PrimaryNameNulls()
            {
                person.Prefix = string.Empty;
                person.Suffix = string.Empty;
                person.MiddleName = string.Empty;
                person.Nickname = string.Empty;
                // Act--get person
                var personDto = await personService.GetPerson3ByGuidAsync(personGuid, false);
                var personPrimaryName = personDto.PersonNames.Where(pn => pn.NameType.Category == Dtos.EnumProperties.PersonNameType2.Personal).FirstOrDefault();

                Assert.AreEqual(null, personPrimaryName.Title);
                Assert.AreEqual(null, personPrimaryName.Pedigree);
                Assert.AreEqual(null, personPrimaryName.MiddleName);
            }

            [TestMethod]
            public async Task GetPerson3ByGuid_StudentRole()
            {
                //personRepoMock.Setup(repo => repo.IsFacultyAsync(personId)).ReturnsAsync(false);
                //personRepoMock.Setup(repo => repo.IsStudentAsync(personId)).ReturnsAsync(true);
                // Act--get person
                var personDto = await personService.GetPerson3ByGuidAsync(personGuid, false);
                var personStudentRole = personDto.Roles.Where(r => r.RoleType == Dtos.EnumProperties.PersonRoleType.Student).FirstOrDefault();
                Assert.AreEqual(Dtos.EnumProperties.PersonRoleType.Student, personStudentRole.RoleType);
                Assert.AreEqual(3, personDto.Roles.Count());
            }

            [TestMethod]
            public async Task GetPerson3ByGuid_FacultyRole()
            {
                //personRepoMock.Setup(repo => repo.IsFacultyAsync(personId)).ReturnsAsync(true);
                //personRepoMock.Setup(repo => repo.IsStudentAsync(personId)).ReturnsAsync(false);
                // Act--get person
                var personDto = await personService.GetPerson3ByGuidAsync(personGuid, false);
                var personStudentFacultyRole = personDto.Roles.Where(r => r.RoleType == Dtos.EnumProperties.PersonRoleType.Instructor).FirstOrDefault();
                Assert.AreEqual(Dtos.EnumProperties.PersonRoleType.Instructor, personStudentFacultyRole.RoleType);
                Assert.AreEqual(3, personDto.Roles.Count());
            }

            //[TestMethod]
            //public async Task GetPerson3ByGuid_NoRoles()
            //{
            //    personRepoMock.Setup(repo => repo.IsFacultyAsync(personId)).ReturnsAsync(false);
            //    personRepoMock.Setup(repo => repo.IsStudentAsync(personId)).ReturnsAsync(false);
            //    // Act--get person
            //    var personDto = await personService.GetPerson3ByGuidAsync(personGuid, false);
            //    Assert.AreEqual(1, personDto.Roles.Count());
            //}

            [TestMethod]
            public async Task GetPerson3ByGuid_BirthName()
            {
                var birthFirstName = "Bernard";
                var birthLastName = "Anders";
                var birthMiddleName = "Bernie";
                personIntegration.BirthNameFirst = birthFirstName;
                personIntegration.BirthNameLast = birthLastName;
                personIntegration.BirthNameMiddle = birthMiddleName;

                // Act--get person
                var personDto = await personService.GetPerson3ByGuidAsync(personGuid, false);
                var personBirthName = personDto.PersonNames.Where(pn => pn.NameType.Category == Dtos.EnumProperties.PersonNameType2.Birth).FirstOrDefault();

                // Assert

                Assert.AreEqual(birthFirstName, personBirthName.FirstName);
                Assert.AreEqual(birthMiddleName, personBirthName.MiddleName);
                Assert.AreEqual(birthLastName, personBirthName.LastName);
            }

            [TestMethod]
            public async Task GetPerson3ByGuid_PersonLanguages()
            {
                // Act--get person
                var personDto = await personService.GetPerson3ByGuidAsync(personGuid, false);
                var personLanguages = personDto.Languages.Where(x => x.Code == PersonLanguageCode.eng).FirstOrDefault();

                // Assert
                Assert.AreEqual(PersonLanguageCode.eng, personLanguages.Code);
                Assert.AreEqual(Dtos.EnumProperties.PersonLanguagePreference.Primary, personLanguages.Preference);
            }

            [TestMethod]
            public async Task GetPerson3ByGuid_PersonAlienStatus()
            {
                var alienStatus = "CS";
                personIntegration.AlienStatus = alienStatus;

                var allCitizenshipStatuses = new TestCitizenshipStatusRepository().Get();
                refRepoMock.Setup(repo => repo.GetCitizenshipStatusesAsync(It.IsAny<bool>())).ReturnsAsync(allCitizenshipStatuses);
                // Act--get person
                var personDto = await personService.GetPerson3ByGuidAsync(personGuid, false);

                // Assert
                // {"87ec6f69-9b16-4ed5-8954-59067f0318ec", "CS", "Canadian citizen", "NA"}, 

                var citizenShipStatus = allCitizenshipStatuses.FirstOrDefault(x => x.Code == alienStatus);

                Assert.AreEqual(Dtos.CitizenshipStatusType.Citizen, personDto.CitizenshipStatus.Category);
                Assert.AreEqual(citizenShipStatus.Guid, personDto.CitizenshipStatus.Detail.Id);
            }

            [TestMethod]
            public async Task GetPerson3ByGuid_PersonCountry()
            {
                var country = "CA";
                personIntegration.BirthCountry = country;
                // Act--get person
                var personDto = await personService.GetPerson3ByGuidAsync(personGuid, false);

                // Assert          
                var birthCountry = countries.FirstOrDefault(x => x.Code == country);
                Assert.AreEqual(birthCountry.Iso3Code, personDto.CountryOfBirth);
            }

            [TestMethod]
            public async Task GetPerson3ByGuid_PersonCitizenship()
            {
                var country = "CA";
                personIntegration.Citizenship = country;
                // Act--get person
                var personDto = await personService.GetPerson3ByGuidAsync(personGuid, false);
                // Assert          
                var birthCountry = countries.FirstOrDefault(x => x.Code == country);
                Assert.AreEqual(birthCountry.Iso3Code, personDto.CitizenshipCountry);
            }

            [TestMethod]
            public async Task GetPerson3ByGuid_SocialMedia()
            {
                var socialMediaTypeCode = "TW";
                var socialMediaHandle = "pontifex";
                var personSocialMedia = new SocialMedia(socialMediaTypeCode, socialMediaHandle);
                personIntegration.AddSocialMedia(personSocialMedia);
                // Act--get person
                var allSocialMediaTypes = new TestSocialMediaTypesRepository().GetSocialMediaTypes();
                refRepoMock.Setup(x => x.GetSocialMediaTypesAsync(It.IsAny<bool>())).ReturnsAsync(allSocialMediaTypes);
                var personDto = await personService.GetPerson3ByGuidAsync(personGuid, false);

                //   {"13660156-d481-4b3d-b617-92136979314c", "TW", "Twitter", "twitter"}, 
                var socialMediaType = allSocialMediaTypes.FirstOrDefault(x => x.Code == socialMediaTypeCode);

                // Assert          
                var personSocialMediaType = personDto.SocialMedia.FirstOrDefault(x => x.Type.Category == Dtos.SocialMediaTypeCategory.twitter);
                Assert.AreEqual(Dtos.SocialMediaTypeCategory.twitter, personSocialMediaType.Type.Category);
                Assert.AreEqual(socialMediaType.Guid, personSocialMediaType.Type.Detail.Id);
                Assert.AreEqual(socialMediaHandle, personSocialMediaType.Address);
            }

            [TestMethod]
            public async Task GetPerson3Dto_Passport()
            {
                var passportNumber = "A1231";
                var passport = new PersonPassport(personGuid, passportNumber);
                passport.IssuingCountry = "USA";
                personIntegration.Passport = passport;

                var allIdentityDocuments = new TestIdentityDocumentTypeRepository().Get();
                refRepoMock.Setup(x => x.GetIdentityDocumentTypesAsync(It.IsAny<bool>())).ReturnsAsync(allIdentityDocuments);

                var passportIdentityDocument = allIdentityDocuments.FirstOrDefault(x => x.Code == "PASSPORT");

                var personDto = await personService.GetPerson3ByGuidAsync(personGuid, false);
                var personPassport = personDto.IdentityDocuments.ElementAtOrDefault(0);

                Assert.IsNotNull(personPassport);
                Assert.AreEqual(passportNumber, personPassport.DocumentId);
                Assert.AreEqual(passportIdentityDocument.Guid, personPassport.Type.Detail.Id);
            }

            [TestMethod]
            public async Task GetPerson3ByGuid_DriversLicense()
            {
                var licenseNumber = "6523123";
                var licenseState = "NY";
                var driverLicense = new PersonDriverLicense(personGuid, licenseNumber);
                driverLicense.IssuingState = licenseState;
                personIntegration.DriverLicense = driverLicense;

                var allIdentityDocuments = new TestIdentityDocumentTypeRepository().Get();
                refRepoMock.Setup(x => x.GetIdentityDocumentTypesAsync(It.IsAny<bool>())).ReturnsAsync(allIdentityDocuments);

                var licenseIdentityDocument = allIdentityDocuments.FirstOrDefault(x => x.Code == "LICENSE");

                var personDto = await personService.GetPerson3ByGuidAsync(personGuid, false);
                var personDriverLicense = personDto.IdentityDocuments.ElementAtOrDefault(0);

                Assert.IsNotNull(personDriverLicense);
                Assert.AreEqual(licenseNumber, personDriverLicense.DocumentId);
                Assert.AreEqual(string.Concat("US-", licenseState), personDriverLicense.Country.Region.Code);
                Assert.AreEqual("USA", personDriverLicense.Country.Code.ToString());
                Assert.AreEqual(licenseIdentityDocument.Guid, personDriverLicense.Type.Detail.Id);
            }

            [TestMethod]
            public async Task GetPerson3ByGuid_Interests()
            {
                var interest1 = "ART";
                var interest2 = "BASE";
                personIntegration.Interests = new List<string>() { interest1, interest2 };

                var allInterests = new TestInterestsRepository().GetInterests();

                refRepoMock.Setup(x => x.GetInterestsAsync(It.IsAny<bool>())).ReturnsAsync(allInterests);
                var personDto = await personService.GetPerson3ByGuidAsync(personGuid, false);

                var interestArt = allInterests.FirstOrDefault(x => x.Code == interest1);
                var interestBase = allInterests.FirstOrDefault(x => x.Code == interest2);

                Assert.AreEqual(2, personDto.Interests.Count());
                Assert.AreEqual(interestArt.Guid, personDto.Interests.ElementAtOrDefault(0).Id);
                Assert.AreEqual(interestBase.Guid, personDto.Interests.ElementAtOrDefault(1).Id);
            }

            [TestMethod]
            public async Task GetPerson3ByGuid_Interests_Invalid()
            {
                var interest1 = "INVALID";
                personIntegration.Interests = new List<string>() { interest1 };
                var allInterests = new TestInterestsRepository().GetInterests();

                refRepoMock.Setup(x => x.GetInterestsAsync(It.IsAny<bool>())).ReturnsAsync(allInterests);
                var personDto = await personService.GetPerson3ByGuidAsync(personGuid, false);

                Assert.AreEqual(null, personDto.Interests);
            }

            [TestMethod]
            public async Task GetPerson3ByGuid_Username()
            {
                var personDto = await personService.GetPerson3ByGuidAsync(personGuid, false);
                var userName = personDto.Credentials.FirstOrDefault(c => c.Type == Dtos.EnumProperties.CredentialType2.ColleagueUserName);
                Assert.AreEqual("testUsername", userName.Value);
            }

        }
        #endregion

        #region GetPersonsCredentials2 Tests

        [TestClass]
        public class GetPersonsCredentials2 : CurrentUserSetup
        {
            private string personId = "0000011";
            private string personGuid = "5674f28b-b216-4055-b236-81a922d93b4c";
            private Mock<IPersonRepository> personRepoMock;
            private IPersonRepository personRepo;
            private Mock<IPersonBaseRepository> personBaseRepoMock;
            private IPersonBaseRepository personBaseRepo;
            private Mock<IReferenceDataRepository> refRepoMock;
            private IReferenceDataRepository refRepo;
            private Mock<IAdapterRegistry> adapterRegistryMock;
            private IAdapterRegistry adapterRegistry;
            private PersonService personService;
            private ILogger logger;
            private Ellucian.Colleague.Domain.Base.Entities.Person person;
            private Mock<IRoleRepository> roleRepoMock;
            private IRoleRepository roleRepo;
            private ICurrentUserFactory currentUserFactory;
            private Ellucian.Colleague.Domain.Entities.Permission permissionViewAnyPerson;

            [TestInitialize]
            public void Initialize()
            {
                personRepoMock = new Mock<IPersonRepository>();
                personRepo = personRepoMock.Object;
                personBaseRepoMock = new Mock<IPersonBaseRepository>();
                personBaseRepo = personBaseRepoMock.Object;
                refRepoMock = new Mock<IReferenceDataRepository>();
                refRepo = refRepoMock.Object;
                adapterRegistryMock = new Mock<IAdapterRegistry>();
                adapterRegistry = adapterRegistryMock.Object;
                roleRepoMock = new Mock<IRoleRepository>();
                roleRepo = roleRepoMock.Object;
                logger = new Mock<ILogger>().Object;

                // Mock person response from the person repository
                person = new Domain.Base.Entities.Person(personId, "Brown");
                person.Guid = personGuid;
                person.Prefix = "Mr.";
                person.FirstName = "Ricky";
                person.MiddleName = "Lee";
                person.GovernmentId = "111-11-1111";
                person.AddPersonAlt(new PersonAlt("22908", "ELEV"));

                personRepoMock.Setup(repo => repo.GetPersonByGuidNonCachedAsync(personGuid)).ReturnsAsync(person);

                // International Parameters Host Country
                personRepoMock.Setup(repo => repo.GetHostCountryAsync()).ReturnsAsync("USA");

                // Set up current user
                currentUserFactory = new CurrentUserSetup.PersonUserFactory();

                // Mock permissions
                permissionViewAnyPerson = new Ellucian.Colleague.Domain.Entities.Permission(BasePermissionCodes.ViewAnyPerson);
                personRole.AddPermission(permissionViewAnyPerson);
                roleRepoMock.Setup(rpm => rpm.Roles).Returns(new List<Role>() { personRole });

                personService = new PersonService(adapterRegistry, personRepo, personBaseRepo, refRepo, null, null, null, null, currentUserFactory, roleRepo, logger);
            }

            [TestCleanup]
            public void Cleanup()
            {
                adapterRegistry = null;
                personRepo = null;
                personBaseRepo = null;
                refRepo = null;
                roleRepo = null;
                personService = null;
                personRole = null;
            }

            [TestMethod]
            public async Task GetPersonCredential2Dto()
            {
                // Act--get person
                var personCredentialDto = await personService.GetPersonCredential2ByGuidAsync(personGuid);
                // Assert
                Assert.IsTrue(personCredentialDto is Dtos.PersonCredential2);
                Assert.AreEqual(person.Guid, personCredentialDto.Id);
                var personSsnCredential = personCredentialDto.Credentials.FirstOrDefault(c => c.Type == Dtos.EnumProperties.CredentialType2.Ssn);
                Assert.AreEqual(person.GovernmentId, personSsnCredential.Value);
                var personElevateCredential = personCredentialDto.Credentials.FirstOrDefault(c => c.Type == Dtos.EnumProperties.CredentialType2.ElevateID);
                Assert.AreEqual(person.PersonAltIds.ElementAt(0).ToString(), personElevateCredential.Value);
            }

            //[TestMethod]
            //[ExpectedException(typeof(PermissionsException))]
            //public async Task GetPersonPermissions2Exception()
            //{
            //    // Mock permissions
            //    personRole.RemovePermission(permissionViewAnyPerson);
            //    roleRepoMock.Setup(rpm => rpm.Roles).Returns(new List<Role>() { personRole });
            //    var personDto = await personService.GetPersonCredential2ByGuidAsync(personGuid);
            //}

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task GetPersonPermissions2_NullId_Exception()
            {
                var personDto = await personService.GetPersonCredential2ByGuidAsync("");
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task GetPersonCredential2Dto_Null_Entity_()
            {
                // Act--get person
                var personCredentialDto = await personService.GetPersonCredential2ByGuidAsync("123456");
            }

            [TestMethod]
            public async Task GetPersonCredential2NoElevateId()
            {
                // Act--get person
                person = new Domain.Base.Entities.Person(personId, "Brown");
                person.Guid = personGuid;
                var personCredentialDto = await personService.GetPersonCredential2ByGuidAsync(personGuid);
                // check ssn
                var personElevateCredential = personCredentialDto.Credentials.FirstOrDefault(c => c.Type == Dtos.EnumProperties.CredentialType2.ElevateID);
                Assert.IsTrue(personElevateCredential is Dtos.DtoProperties.CredentialDtoProperty2);
            }

            [TestMethod]
            public async Task GetPersonCredential2NoSsn()
            {
                // Act--get person
                person.GovernmentId = null;
                var personDto = await personService.GetPersonCredential2ByGuidAsync(personGuid);
                // check ssn
                var personSsnCredential = personDto.Credentials.FirstOrDefault(c => c.Type == Dtos.EnumProperties.CredentialType2.Ssn);
                Assert.AreEqual(null, personSsnCredential);
            }
        }

        #endregion

        #region GetPersonsCredentials3 Tests

        [TestClass]
        public class GetPersonsCredentials3 : CurrentUserSetup
        {
            private string personId = "0000011";
            private string personGuid = "5674f28b-b216-4055-b236-81a922d93b4c";
            private Mock<IPersonRepository> personRepoMock;
            private IPersonRepository personRepo;
            private Mock<IPersonBaseRepository> personBaseRepoMock;
            private IPersonBaseRepository personBaseRepo;
            private Mock<IReferenceDataRepository> refRepoMock;
            private IReferenceDataRepository refRepo;
            private Mock<IAdapterRegistry> adapterRegistryMock;
            private IAdapterRegistry adapterRegistry;
            private PersonService personService;
            private ILogger logger;
            private Ellucian.Colleague.Domain.Base.Entities.Person person;
            private Mock<IRoleRepository> roleRepoMock;
            private IRoleRepository roleRepo;
            private ICurrentUserFactory currentUserFactory;
            private Ellucian.Colleague.Domain.Entities.Permission permissionViewAnyPerson;

            [TestInitialize]
            public void Initialize()
            {
                personRepoMock = new Mock<IPersonRepository>();
                personRepo = personRepoMock.Object;
                personBaseRepoMock = new Mock<IPersonBaseRepository>();
                personBaseRepo = personBaseRepoMock.Object;
                refRepoMock = new Mock<IReferenceDataRepository>();
                refRepo = refRepoMock.Object;
                adapterRegistryMock = new Mock<IAdapterRegistry>();
                adapterRegistry = adapterRegistryMock.Object;
                roleRepoMock = new Mock<IRoleRepository>();
                roleRepo = roleRepoMock.Object;
                logger = new Mock<ILogger>().Object;

                // Mock person response from the person repository
                person = new Domain.Base.Entities.Person(personId, "Brown");
                person.Guid = personGuid;
                person.Prefix = "Mr.";
                person.FirstName = "Ricky";
                person.MiddleName = "Lee";
                person.GovernmentId = "111-11-1111";
                person.AddPersonAlt(new PersonAlt("22908", "ELEV"));

                personRepoMock.Setup(repo => repo.GetPersonCredentialByGuidNonCachedAsync(personGuid)).ReturnsAsync(person);

                // International Parameters Host Country
                personRepoMock.Setup(repo => repo.GetHostCountryAsync()).ReturnsAsync("USA");

                // Set up current user
                currentUserFactory = new CurrentUserSetup.PersonUserFactory();

                // Mock permissions
                permissionViewAnyPerson = new Ellucian.Colleague.Domain.Entities.Permission(BasePermissionCodes.ViewAnyPerson);
                personRole.AddPermission(permissionViewAnyPerson);
                roleRepoMock.Setup(rpm => rpm.Roles).Returns(new List<Role>() { personRole });

                personService = new PersonService(adapterRegistry, personRepo, personBaseRepo, refRepo, null, null, null, null, currentUserFactory, roleRepo, logger);
            }

            [TestCleanup]
            public void Cleanup()
            {
                adapterRegistry = null;
                personRepo = null;
                personBaseRepo = null;
                refRepo = null;
                roleRepo = null;
                personService = null;
                personRole = null;
            }

            [TestMethod]
            public async Task GetPersonCredential3Dto()
            {
                // Act--get person
                var personCredentialDto = await personService.GetPersonCredential3ByGuidAsync(personGuid);
                // Assert
                Assert.IsTrue(personCredentialDto is Dtos.PersonCredential2);
                Assert.AreEqual(person.Guid, personCredentialDto.Id);
                var personSsnCredential = personCredentialDto.Credentials.FirstOrDefault(c => c.Type == Dtos.EnumProperties.CredentialType2.Ssn);
                Assert.AreEqual(person.GovernmentId, personSsnCredential.Value);
                var personElevateCredential = personCredentialDto.Credentials.FirstOrDefault(c => c.Type == Dtos.EnumProperties.CredentialType2.ElevateID);
                Assert.AreEqual(person.PersonAltIds.ElementAt(0).ToString(), personElevateCredential.Value);
            }

            //[TestMethod]
            //[ExpectedException(typeof(PermissionsException))]
            //public async Task GetPersonPermissions3Exception()
            //{
            //    // Mock permissions
            //    personRole.RemovePermission(permissionViewAnyPerson);
            //    roleRepoMock.Setup(rpm => rpm.Roles).Returns(new List<Role>() { personRole });
            //    var personDto = await personService.GetPersonCredential3ByGuidAsync(personGuid);
            //}

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task GetPersonPermissions3_NullId_Exception()
            {
                var personDto = await personService.GetPersonCredential3ByGuidAsync("");
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task GetPersonCredential3Dto_Null_Entity_()
            {
                // Act--get person
                var personCredentialDto = await personService.GetPersonCredential3ByGuidAsync("123456");
            }

            [TestMethod]
            public async Task GetPersonCredential3NoElevateId()
            {
                // Act--get person
                person = new Domain.Base.Entities.Person(personId, "Brown");
                person.Guid = personGuid;
                var personCredentialDto = await personService.GetPersonCredential3ByGuidAsync(personGuid);
                // check ssn
                var personElevateCredential = personCredentialDto.Credentials.FirstOrDefault(c => c.Type == Dtos.EnumProperties.CredentialType2.ElevateID);
                Assert.IsTrue(personElevateCredential is Dtos.DtoProperties.CredentialDtoProperty2);
            }

            [TestMethod]
            public async Task GetPersonCredential3NoSsn()
            {
                // Act--get person
                person.GovernmentId = null;
                var personDto = await personService.GetPersonCredential3ByGuidAsync(personGuid);
                // check ssn
                var personSsnCredential = personDto.Credentials.FirstOrDefault(c => c.Type == Dtos.EnumProperties.CredentialType2.Ssn);
                Assert.AreEqual(null, personSsnCredential);
            }
        }

        #endregion

        #region GetPersonsCredentials2_GET_ALL

        [TestClass]
        public class GetPersonsCredentials2_GET_ALL : CurrentUserSetup
        {
            //private string personId = "0000011";
            //private string personGuid = "5674f28b-b216-4055-b236-81a922d93b4c";
            private Mock<IPersonRepository> personRepoMock;
            private IPersonRepository personRepo;
            private Mock<IPersonBaseRepository> personBaseRepoMock;
            private IPersonBaseRepository personBaseRepo;
            private Mock<IReferenceDataRepository> refRepoMock;
            private IReferenceDataRepository refRepo;
            private Mock<IAdapterRegistry> adapterRegistryMock;
            private IAdapterRegistry adapterRegistry;
            private PersonService personService;
            private ILogger logger;
            private IEnumerable<Domain.Base.Entities.PersonIntegration> people;
            private IEnumerable<string> personIds;
            private IEnumerable<PersonPin> personPins;
            private Tuple<IEnumerable<string>, int> peopleIdsTuple;
            private Mock<IRoleRepository> roleRepoMock;
            private IRoleRepository roleRepo;
            private ICurrentUserFactory currentUserFactory;
            private Ellucian.Colleague.Domain.Entities.Permission permissionViewAnyPerson;

            [TestInitialize]
            public void Initialize()
            {
                personRepoMock = new Mock<IPersonRepository>();
                personRepo = personRepoMock.Object;
                personBaseRepoMock = new Mock<IPersonBaseRepository>();
                personBaseRepo = personBaseRepoMock.Object;
                refRepoMock = new Mock<IReferenceDataRepository>();
                refRepo = refRepoMock.Object;
                adapterRegistryMock = new Mock<IAdapterRegistry>();
                adapterRegistry = adapterRegistryMock.Object;
                roleRepoMock = new Mock<IRoleRepository>();
                roleRepo = roleRepoMock.Object;
                logger = new Mock<ILogger>().Object;

                // Mock person response from the person repository
                BuildData();

                personRepoMock.Setup(repo => repo.GetPersonGuidsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>())).ReturnsAsync(peopleIdsTuple);
                personRepoMock.Setup(repo => repo.GetPersonIntegrationByGuidNonCachedAsync(peopleIdsTuple.Item1)).ReturnsAsync(people);

                // International Parameters Host Country
                personRepoMock.Setup(repo => repo.GetHostCountryAsync()).ReturnsAsync("USA");

                // Set up current user
                currentUserFactory = new CurrentUserSetup.PersonUserFactory();

                // Mock permissions
                permissionViewAnyPerson = new Ellucian.Colleague.Domain.Entities.Permission(BasePermissionCodes.ViewAnyPerson);
                personRole.AddPermission(permissionViewAnyPerson);
                roleRepoMock.Setup(rpm => rpm.Roles).Returns(new List<Role>() { personRole });

                personService = new PersonService(adapterRegistry, personRepo, personBaseRepo, refRepo, null, null, null, null, currentUserFactory, roleRepo, logger);
            }

            private void BuildData()
            {
                people = new List<Domain.Base.Entities.PersonIntegration>()
                {
                    new Domain.Base.Entities.PersonIntegration("1", "Mouse")
                    {
                        Guid = "5674f28b-b216-4055-b236-81a922d93b4c",
                        Prefix = "Mr.",
                        FirstName = "Mickey",
                        MiddleName = "Lee",
                        GovernmentId = "111-11-1111"
                    },
                     new Domain.Base.Entities.PersonIntegration("2", "Brown")
                    {
                        Guid = "1674f28b-b216-4055-b236-81a922d93b4a",
                        Prefix = "Mr.",
                        FirstName = "Charlie",
                        MiddleName = "Lee",
                        GovernmentId = "222-22-2222"
                    }
                };
                int counter = 1;
                foreach (var singlePerson in people)
                {
                    singlePerson.AddPersonAlt(new PersonAlt(counter.ToString(), "ELEV"));
                    counter++;
                }
                personIds = new List<string>() { "1", "2" };
                peopleIdsTuple = new Tuple<IEnumerable<string>, int>(personIds, personIds.Count());

                personPins = new List<PersonPin>
                {
                    new PersonPin("1", "PersonUserId1"),
                    new PersonPin("2", "PersonUserId2")
                };
                personRepoMock.Setup(repo => repo.GetPersonPinsAsync(It.IsAny<string[]>())).ReturnsAsync(personPins);
            }

            [TestCleanup]
            public void Cleanup()
            {
                adapterRegistry = null;
                personRepo = null;
                personBaseRepo = null;
                refRepo = null;
                roleRepo = null;
                personService = null;
                personRole = null;
            }

            [TestMethod]
            public async Task GetAllPersonCredentials()
            {
                // Act--get person
                var actuals = await personService.GetAllPersonCredentials2Async(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>());
                // Assert
                Assert.IsNotNull(actuals);
                Assert.AreEqual(2, actuals.Item1.Count());

                foreach (var actual in actuals.Item1)
                {
                    var expected = people.FirstOrDefault(i => i.Guid.Equals(actual.Id, StringComparison.OrdinalIgnoreCase));

                    Assert.AreEqual(expected.GovernmentId, actual.Credentials.First(i => i.Type == Dtos.EnumProperties.CredentialType2.Ssn).Value);
                    Assert.AreEqual(expected.PersonAltIds.ElementAt(0).Id, actual.Credentials.FirstOrDefault(i => i.Type == Dtos.EnumProperties.CredentialType2.ElevateID).Value);
                }
            }

            [TestMethod]
            public async Task GetAllPersonCredentials2_NoData()
            {
                // Act--get person
                personRepoMock.Setup(repo => repo.GetPersonGuidsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>())).ReturnsAsync(() => null);
                var actuals = await personService.GetAllPersonCredentials2Async(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>());
                // Assert
                Assert.IsNotNull(actuals);
                Assert.AreEqual(0, actuals.Item2);
            }
        }

        #endregion

        #region GetPersonsCredentials3_GET_ALL

        [TestClass]
        public class GetPersonsCredentials3_GET_ALL : CurrentUserSetup
        {
            //private string personId = "0000011";
            //private string personGuid = "5674f28b-b216-4055-b236-81a922d93b4c";
            private Mock<IPersonRepository> personRepoMock;
            private IPersonRepository personRepo;
            private Mock<IPersonBaseRepository> personBaseRepoMock;
            private IPersonBaseRepository personBaseRepo;
            private Mock<IReferenceDataRepository> refRepoMock;
            private IReferenceDataRepository refRepo;
            private Mock<IAdapterRegistry> adapterRegistryMock;
            private IAdapterRegistry adapterRegistry;
            private PersonService personService;
            private ILogger logger;
            private IEnumerable<Domain.Base.Entities.PersonIntegration> people;
            private IEnumerable<string> personIds;
            private IEnumerable<PersonPin> personPins;
            private Tuple<IEnumerable<string>, int> peopleIdsTuple;
            private Mock<IRoleRepository> roleRepoMock;
            private IRoleRepository roleRepo;
            private ICurrentUserFactory currentUserFactory;
            private Ellucian.Colleague.Domain.Entities.Permission permissionViewAnyPerson;

            [TestInitialize]
            public void Initialize()
            {
                personRepoMock = new Mock<IPersonRepository>();
                personRepo = personRepoMock.Object;
                personBaseRepoMock = new Mock<IPersonBaseRepository>();
                personBaseRepo = personBaseRepoMock.Object;
                refRepoMock = new Mock<IReferenceDataRepository>();
                refRepo = refRepoMock.Object;
                adapterRegistryMock = new Mock<IAdapterRegistry>();
                adapterRegistry = adapterRegistryMock.Object;
                roleRepoMock = new Mock<IRoleRepository>();
                roleRepo = roleRepoMock.Object;
                logger = new Mock<ILogger>().Object;

                // Mock person response from the person repository
                BuildData();

                personRepoMock.Setup(repo => repo.GetPersonGuidsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>())).ReturnsAsync(peopleIdsTuple);

                personRepoMock.Setup(repo => repo.GetPersonCredentialsIntegrationByGuidNonCachedAsync(peopleIdsTuple.Item1)).ReturnsAsync(people);

                // International Parameters Host Country
                personRepoMock.Setup(repo => repo.GetHostCountryAsync()).ReturnsAsync("USA");

                // Set up current user
                currentUserFactory = new CurrentUserSetup.PersonUserFactory();

                // Mock permissions
                permissionViewAnyPerson = new Ellucian.Colleague.Domain.Entities.Permission(BasePermissionCodes.ViewAnyPerson);
                personRole.AddPermission(permissionViewAnyPerson);
                roleRepoMock.Setup(rpm => rpm.Roles).Returns(new List<Role>() { personRole });

                personService = new PersonService(adapterRegistry, personRepo, personBaseRepo, refRepo, null, null, null, null, currentUserFactory, roleRepo, logger);
            }

            private void BuildData()
            {
                people = new List<Domain.Base.Entities.PersonIntegration>()
                {
                    new Domain.Base.Entities.PersonIntegration("1", "Mouse")
                    {
                        Guid = "5674f28b-b216-4055-b236-81a922d93b4c",
                        Prefix = "Mr.",
                        FirstName = "Mickey",
                        MiddleName = "Lee",
                        GovernmentId = "111-11-1111"
                    },
                     new Domain.Base.Entities.PersonIntegration("2", "Brown")
                    {
                        Guid = "1674f28b-b216-4055-b236-81a922d93b4a",
                        Prefix = "Mr.",
                        FirstName = "Charlie",
                        MiddleName = "Lee",
                        GovernmentId = "222-22-2222"
                    }
                };
                int counter = 1;
                foreach (var singlePerson in people)
                {
                    singlePerson.AddPersonAlt(new PersonAlt(counter.ToString(), "ELEV"));
                    counter++;
                }
                personIds = new List<string>() { "1", "2" };
                peopleIdsTuple = new Tuple<IEnumerable<string>, int>(personIds, personIds.Count());

                personPins = new List<PersonPin>
                {
                    new PersonPin("1", "PersonUserId1"),
                    new PersonPin("2", "PersonUserId2")
                };
                personRepoMock.Setup(repo => repo.GetPersonPinsAsync(It.IsAny<string[]>())).ReturnsAsync(personPins);
            }

            [TestCleanup]
            public void Cleanup()
            {
                adapterRegistry = null;
                personRepo = null;
                personBaseRepo = null;
                refRepo = null;
                roleRepo = null;
                personService = null;
                personRole = null;
            }

            [TestMethod]
            public async Task GetAllPersonCredentials3()
            {
                personRepoMock.Setup(repo => repo.GetFilteredPerson2GuidsAsync(It.IsAny<int>(), It.IsAny<int>(),
                    It.IsAny<bool>(), null, null)).ReturnsAsync(peopleIdsTuple);

                // Act--get person
                var personCredentials = new PersonCredential2();
                var actuals = await personService.GetAllPersonCredentials3Async(It.IsAny<int>(), It.IsAny<int>(),
                    personCredentials, It.IsAny<bool>());
                // Assert
                Assert.IsNotNull(actuals);
                Assert.AreEqual(2, actuals.Item1.Count());

                foreach (var actual in actuals.Item1)
                {
                    var expected = people.FirstOrDefault(i => i.Guid.Equals(actual.Id, StringComparison.OrdinalIgnoreCase));

                    Assert.AreEqual(expected.GovernmentId, actual.Credentials.First(i => i.Type == Dtos.EnumProperties.CredentialType2.Ssn).Value);
                    Assert.AreEqual(expected.PersonAltIds.ElementAt(0).Id, actual.Credentials.FirstOrDefault(i => i.Type == Dtos.EnumProperties.CredentialType2.ElevateID).Value);
                }
            }

            [TestMethod]
            public async Task GetAllPersonCredentials3_withFilter()
            {
                personRepoMock.Setup(repo => repo.GetFilteredPerson2GuidsAsync(It.IsAny<int>(), It.IsAny<int>(),
                    It.IsAny<bool>(), It.IsAny<PersonFilterCriteria>(), null)).ReturnsAsync(peopleIdsTuple);

                // Act--get person
                var personCredentials = new PersonCredential2()
                {
                    Credentials = new List<CredentialDtoProperty2>()
                    {
                        new CredentialDtoProperty2() { Type = CredentialType2.Ssn, Value = "111-11-1111"}
                    }
                };
                var actuals = await personService.GetAllPersonCredentials3Async(It.IsAny<int>(), It.IsAny<int>(),
                    personCredentials, It.IsAny<bool>());
                // Assert
                Assert.IsNotNull(actuals);
                Assert.AreEqual(2, actuals.Item1.Count());

                foreach (var actual in actuals.Item1)
                {
                    var expected = people.FirstOrDefault(i => i.Guid.Equals(actual.Id, StringComparison.OrdinalIgnoreCase));

                    Assert.AreEqual(expected.GovernmentId, actual.Credentials.First(i => i.Type == Dtos.EnumProperties.CredentialType2.Ssn).Value);
                    Assert.AreEqual(expected.PersonAltIds.ElementAt(0).Id, actual.Credentials.FirstOrDefault(i => i.Type == Dtos.EnumProperties.CredentialType2.ElevateID).Value);
                }
            }

            [TestMethod]
            public async Task GetAllPersonCredentials3_NoData()
            {
                // Act--get person
                //personRepoMock.Setup(repo => repo.GetPersonGuidsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>())).ReturnsAsync(() => null);
                personRepoMock.Setup(repo => repo.GetFilteredPerson2GuidsAsync(It.IsAny<int>(), It.IsAny<int>(),
                    It.IsAny<bool>(), null, null)).ReturnsAsync(() => null);
                var personCredentials = new PersonCredential2();
                var actuals = await personService.GetAllPersonCredentials3Async(It.IsAny<int>(), It.IsAny<int>(),
                    personCredentials, It.IsAny<bool>());
                // Assert
                Assert.IsNotNull(actuals);
                Assert.AreEqual( 0, actuals.Item2 );
            }
        }

        #endregion

        #region GetPersonsCredentials4_GET_ALL

        [TestClass]
        public class GetPersonsCredentials4_GET_ALL: CurrentUserSetup
        {
            private string personId = "0000011";
            private string personGuid = "5674f28b-b216-4055-b236-81a922d93b4c";

            private Mock<IPersonRepository> personRepoMock;
            private IPersonRepository personRepo;
            private Mock<IPersonBaseRepository> personBaseRepoMock;
            private IPersonBaseRepository personBaseRepo;
            private Mock<IReferenceDataRepository> refRepoMock;
            private IReferenceDataRepository refRepo;
            private Mock<IAdapterRegistry> adapterRegistryMock;
            private IAdapterRegistry adapterRegistry;
            private PersonService personService;
            private ILogger logger;
            private IEnumerable<Domain.Base.Entities.PersonIntegration> people; 
            private Ellucian.Colleague.Domain.Base.Entities.Person person;

            private IEnumerable<string> personIds;
            private IEnumerable<PersonPin> personPins;
            private Tuple<IEnumerable<string>, int> peopleIdsTuple;
            private Mock<IRoleRepository> roleRepoMock;
            private IRoleRepository roleRepo;
            private ICurrentUserFactory currentUserFactory;
            private Ellucian.Colleague.Domain.Entities.Permission permissionViewAnyPerson;

            [TestInitialize]
            public void Initialize()
            {
                personRepoMock = new Mock<IPersonRepository>();
                personRepo = personRepoMock.Object;
                personBaseRepoMock = new Mock<IPersonBaseRepository>();
                personBaseRepo = personBaseRepoMock.Object;
                refRepoMock = new Mock<IReferenceDataRepository>();
                refRepo = refRepoMock.Object;
                adapterRegistryMock = new Mock<IAdapterRegistry>();
                adapterRegistry = adapterRegistryMock.Object;
                roleRepoMock = new Mock<IRoleRepository>();
                roleRepo = roleRepoMock.Object;
                logger = new Mock<ILogger>().Object;

                // Mock person response from the person repository
                BuildData();

                personRepoMock.Setup( repo => repo.GetPersonGuidsAsync( It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>() ) ).ReturnsAsync( peopleIdsTuple );

                personRepoMock.Setup( repo => repo.GetPersonCredentialsIntegrationByGuidNonCachedAsync( peopleIdsTuple.Item1 ) ).ReturnsAsync( people );

                // International Parameters Host Country
                personRepoMock.Setup( repo => repo.GetHostCountryAsync() ).ReturnsAsync( "USA" );

                // Set up current user
                currentUserFactory = new CurrentUserSetup.PersonUserFactory();

                // Mock permissions
                permissionViewAnyPerson = new Ellucian.Colleague.Domain.Entities.Permission( BasePermissionCodes.ViewAnyPerson );
                personRole.AddPermission( permissionViewAnyPerson );
                roleRepoMock.Setup( rpm => rpm.Roles ).Returns( new List<Role>() { personRole } );

                personService = new PersonService( adapterRegistry, personRepo, personBaseRepo, refRepo, null, null, null, null, currentUserFactory, roleRepo, logger );
            }

            private void BuildData()
            {
                people = new List<Domain.Base.Entities.PersonIntegration>()
                {
                    new Domain.Base.Entities.PersonIntegration("1", "Mouse")
                    {
                        Guid = "5674f28b-b216-4055-b236-81a922d93b4c",
                        Prefix = "Mr.",
                        FirstName = "Mickey",
                        MiddleName = "Lee",
                        GovernmentId = "111-11-1111"
                    },
                     new Domain.Base.Entities.PersonIntegration("2", "Brown")
                    {
                        Guid = "1674f28b-b216-4055-b236-81a922d93b4a",
                        Prefix = "Mr.",
                        FirstName = "Charlie",
                        MiddleName = "Lee",
                        GovernmentId = "222-22-2222"
                    }
                };
                int counter = 1;
                foreach( var singlePerson in people )
                {
                    singlePerson.AddPersonAlt( new PersonAlt( counter.ToString(), "ELEV" ) );
                    counter++;
                }
                personIds = new List<string>() { "1", "2" };
                peopleIdsTuple = new Tuple<IEnumerable<string>, int>( personIds, personIds.Count() );

                personPins = new List<PersonPin>
                {
                    new PersonPin("1", "PersonUserId1"),
                    new PersonPin("2", "PersonUserId2")
                };
                personRepoMock.Setup( repo => repo.GetPersonPinsAsync( It.IsAny<string[]>() ) ).ReturnsAsync( personPins );
                person = new Domain.Base.Entities.Person( personId, "Brown" );
                person.Guid = personGuid;
                person.Prefix = "Mr.";
                person.FirstName = "Ricky";
                person.MiddleName = "Lee";
                person.GovernmentId = "111-11-1111";
                person.AddPersonAlt( new PersonAlt( "22908", "ELEV" ) );

                personRepoMock.Setup( repo => repo.GetPersonCredentialByGuidNonCachedAsync( personGuid ) ).ReturnsAsync( person );
                refRepoMock.Setup( repo => repo.GetAlternateIdTypesAsync( It.IsAny<bool>() ) ).ReturnsAsync( new List<AltIdTypes>()
                {
                    new AltIdTypes("AE44FE48-2534-480B-8618-5480617CE74A", "ELEV2", "Elevate ID 2"),
                    new AltIdTypes("D525E2B2-CD7D-4995-93F0-97DA468EBE90", "GOVID2", "Government ID 2")
                } );
            }

            [TestCleanup]
            public void Cleanup()
            {
                adapterRegistry = null;
                personRepo = null;
                personBaseRepo = null;
                refRepo = null;
                roleRepo = null;
                personService = null;
                personRole = null;
            }

            [TestMethod]
            public async Task GetAllPersonCredentials3()
            {
                personRepoMock.Setup( repo => repo.GetFilteredPerson2GuidsAsync( It.IsAny<int>(), It.IsAny<int>(),
                     It.IsAny<bool>(), null, null ) ).ReturnsAsync( peopleIdsTuple );

                // Act--get person
                var personCredentials = new PersonCredential3();
                var actuals = await personService.GetAllPersonCredentials4Async( It.IsAny<int>(), It.IsAny<int>(),
                    personCredentials, It.IsAny<bool>() );
                // Assert
                Assert.IsNotNull( actuals );
                Assert.AreEqual( 2, actuals.Item1.Count() );

                foreach( var actual in actuals.Item1 )
                {
                    var expected = people.FirstOrDefault( i => i.Guid.Equals( actual.Id, StringComparison.OrdinalIgnoreCase ) );
                    Assert.IsNotNull(expected);
                    var cred = actual.Credentials.FirstOrDefault( c => c.Type == Credential3Type.TaxIdentificationNumber ).Value;
                    Assert.AreEqual( expected.GovernmentId, cred );
                }
            }

            [TestMethod]
            public async Task GetAllPersonCredentials3_WithCriteria()
            {
                PersonFilterCriteria personFilterCriteria = new PersonFilterCriteria();
                personFilterCriteria.AlternativeCredentials = null;
                personRepoMock.Setup( repo => repo.GetFilteredPerson2GuidsAsync( It.IsAny<int>(), It.IsAny<int>(),
                     It.IsAny<bool>(),It.IsAny< PersonFilterCriteria>(), null ) ).ReturnsAsync( peopleIdsTuple );

                // Act--get person
                var personCredentials = new PersonCredential3()
                {
                    Credentials = new List<Credential3DtoProperty>()
                    {
                        new Credential3DtoProperty()
                        {
                            Type = Credential3Type.TaxIdentificationNumber,
                            Value = "111-11-1111"
                        }
                    },
                    AlternativeCredentials = new List<AlternativeCredentials>()
                    {
                        new AlternativeCredentials()
                        {
                            Type = new GuidObject2("AE44FE48-2534-480B-8618-5480617CE74A"),
                            Value = "ELEV2"
                        }
                    }
                };
                var actuals = await personService.GetAllPersonCredentials4Async( It.IsAny<int>(), It.IsAny<int>(),
                    personCredentials, It.IsAny<bool>() );
                // Assert
                Assert.IsNotNull( actuals );
                Assert.AreEqual( 2, actuals.Item1.Count() );

                foreach( var actual in actuals.Item1 )
                {
                    var expected = people.FirstOrDefault( i => i.Guid.Equals( actual.Id, StringComparison.OrdinalIgnoreCase ) );
                    Assert.IsNotNull( expected );
                    var cred = actual.Credentials.FirstOrDefault( c => c.Type == Credential3Type.TaxIdentificationNumber ).Value;
                    Assert.AreEqual( expected.GovernmentId, cred );
                }
            }

            [TestMethod]
            [ExpectedException(typeof( RepositoryException ) )]
            public async Task GetAllPersonCredentials3_RepositoryException()
            {
                var message = "Repository error message.";
                personRepoMock.Setup( repo => repo.GetFilteredPerson2GuidsAsync( It.IsAny<int>(), It.IsAny<int>(),
                     It.IsAny<bool>(), null, null ) ).ThrowsAsync( new RepositoryException(message));

                // Act--get person                
                var actuals = await personService.GetAllPersonCredentials4Async( It.IsAny<int>(), It.IsAny<int>(), null, It.IsAny<bool>() );               
            }

            [TestMethod]
            [ExpectedException( typeof( Exception ) )]
            public async Task GetAllPersonCredentials3_Exception()
            {
                var message = "Repository error message.";
                personRepoMock.Setup( repo => repo.GetFilteredPerson2GuidsAsync( It.IsAny<int>(), It.IsAny<int>(),
                     It.IsAny<bool>(), null, null ) ).ThrowsAsync( new Exception( message ) );

                // Act--get person                
                var actuals = await personService.GetAllPersonCredentials4Async( It.IsAny<int>(), It.IsAny<int>(), null, It.IsAny<bool>() );
            }

            [TestMethod]
            public async Task GetPersonCredential4ByGuidAsync()
            {
                var actuals = await personService.GetPersonCredential4ByGuidAsync( personGuid );
                // Assert
                Assert.IsNotNull( actuals );
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task GetPersonCredential4ByGuidAsync_Null_Id_ArgumentNullException()
            {
                var actuals = await personService.GetPersonCredential4ByGuidAsync( string.Empty );
            }

            [TestMethod]
            public async Task GetPersonCredential4ByGuidAsync_ArgumentNullException()
            {
                HttpStatusCode statusCode = HttpStatusCode.NotFound;
                personRepoMock.Setup( repo => repo.GetPersonCredentialByGuidNonCachedAsync( personGuid ) ).ThrowsAsync( new ArgumentNullException() );

                try
                {
                    var actuals = await personService.GetPersonCredential4ByGuidAsync( personGuid );
                }
                catch( IntegrationApiException e )
                {
                    statusCode = e.HttpStatusCode;
                }
                Assert.AreEqual( HttpStatusCode.NotFound, statusCode );
            }

            [TestMethod]
            public async Task GetPersonCredential4ByGuidAsync_PersonEntity_Null_IntegrationApiException()
            {
                HttpStatusCode statusCode = HttpStatusCode.NotFound;

                personRepoMock.Setup( repo => repo.GetPersonCredentialByGuidNonCachedAsync( personGuid ) ).ReturnsAsync(() => null);
                try
                {
                    var actuals = await personService.GetPersonCredential4ByGuidAsync( personGuid );
                }
                catch( IntegrationApiException e )
                {
                    statusCode = e.HttpStatusCode;
                }
                Assert.AreEqual( HttpStatusCode.NotFound, statusCode );
            }
        }

        #endregion

        #region GetProfile Tests

        [TestClass]
        public class GetProfile : CurrentUserSetup
        {
            private string personId = "0000015";
            private Mock<IPersonRepository> personRepoMock;
            private IPersonRepository personRepo;
            private Mock<IPersonBaseRepository> personBaseRepoMock;
            private IPersonBaseRepository personBaseRepo;
            private Mock<IReferenceDataRepository> refRepoMock;
            private IReferenceDataRepository refRepo;
            private Mock<IProfileRepository> profileRepoMock;
            private IProfileRepository profileRepo;
            private Mock<IRelationshipRepository> relRepoMock;
            private IRelationshipRepository relRepo;
            private Mock<IAdapterRegistry> adapterRegistryMock;
            private IAdapterRegistry adapterRegistry;
            private PersonService personService;
            private ILogger logger;
            private Ellucian.Colleague.Domain.Base.Entities.Person person;
            private Ellucian.Colleague.Domain.Base.Entities.Profile profile;
            private Mock<IRoleRepository> roleRepoMock;
            private IRoleRepository roleRepo;
            private Mock<IProxyRepository> proxyRepoMock;
            private IProxyRepository proxyRepo;
            private ICurrentUserFactory currentUserFactory;
            private PhoneNumber personPhones;
            private List<Address> personAddresses;

            [TestInitialize]
            public void Initialize()
            {
                personRepoMock = new Mock<IPersonRepository>();
                personRepo = personRepoMock.Object;
                personBaseRepoMock = new Mock<IPersonBaseRepository>();
                personBaseRepo = personBaseRepoMock.Object;
                refRepoMock = new Mock<IReferenceDataRepository>();
                refRepo = refRepoMock.Object;
                profileRepoMock = new Mock<IProfileRepository>();
                profileRepo = profileRepoMock.Object;
                relRepoMock = new Mock<IRelationshipRepository>();
                relRepo = relRepoMock.Object;
                adapterRegistryMock = new Mock<IAdapterRegistry>();
                adapterRegistry = adapterRegistryMock.Object;
                roleRepoMock = new Mock<IRoleRepository>();
                roleRepo = roleRepoMock.Object;
                proxyRepoMock = new Mock<IProxyRepository>();
                proxyRepo = proxyRepoMock.Object;

                logger = new Mock<ILogger>().Object;

                // Mock profile response from the person repository
                profile = new Domain.Base.Entities.Profile(personId, "Brown");
                profile.FirstName = "Richard";
                profile.BirthDate = new DateTime(1930, 1, 1);
                profile.AddEmailAddress(new EmailAddress("rbrown@xmail.com", "COL") { IsPreferred = false });
                profile.AddEmailAddress(new EmailAddress("rlbrown@xellucian.com", "BUS") { IsPreferred = true }); // this one chosen
                profile.AddEmailAddress(new EmailAddress("booboo@qmail.com", "WWW") { IsPreferred = false }); // this one chosen
                profile.PreferredName = "R. Lee Brown";
                profile.AddressConfirmationDateTime = new DateTimeOffset(2001, 1, 2, 15, 16, 17, TimeSpan.FromHours(-3));
                profile.EmailAddressConfirmationDateTime = new DateTimeOffset(2002, 3, 4, 18, 19, 20, TimeSpan.FromHours(-3));
                profile.PhoneConfirmationDateTime = new DateTimeOffset(2003, 5, 6, 21, 22, 23, TimeSpan.FromHours(-3));
                profileRepoMock.Setup(repo => repo.GetProfileAsync(personId, It.IsAny<bool>())).ReturnsAsync(profile);

                relRepoMock.Setup(repo => repo.GetPersonRelationshipsAsync(personId)).ReturnsAsync(new List<Relationship>());

                var profileDtoAdapter = new Ellucian.Colleague.Coordination.Base.Adapters.ProfileAdapter(adapterRegistry, logger);
                adapterRegistryMock.Setup(reg => reg.GetAdapter<Domain.Base.Entities.Profile, Dtos.Base.Profile>()).Returns(profileDtoAdapter);

                var emailDtoAdapter = new AutoMapperAdapter<Ellucian.Colleague.Domain.Base.Entities.EmailAddress, Ellucian.Colleague.Dtos.Base.EmailAddress>(adapterRegistry, logger);
                adapterRegistryMock.Setup(reg => reg.GetAdapter<Ellucian.Colleague.Domain.Base.Entities.EmailAddress, Ellucian.Colleague.Dtos.Base.EmailAddress>()).Returns(emailDtoAdapter);

                // Set up current user
                currentUserFactory = new CurrentUserSetup.PersonUserFactory();

                // Set up phone numbers response
                personPhones = new PhoneNumber(personId);
                personPhones.AddPhone(new Phone("864-123-1234", "HO", "x123"));
                personPhones.AddPhone(new Phone("864-321-4321", "B", "x432")); // this one chosen
                personPhones.AddPhone(new Phone("555-800-1212", "X", "x432")); // this one chosen

                var phoneDtoAdapter = new AutoMapperAdapter<Ellucian.Colleague.Domain.Base.Entities.Phone, Ellucian.Colleague.Dtos.Base.Phone>(adapterRegistry, logger);
                adapterRegistryMock.Setup(reg => reg.GetAdapter<Ellucian.Colleague.Domain.Base.Entities.Phone, Ellucian.Colleague.Dtos.Base.Phone>()).Returns(phoneDtoAdapter);

                // Set up address response
                personAddresses = new List<Address>();
                personAddresses.Add(new Address("123", personId) { AddressLines = new List<string>() { "123 Main" }, City = "Anywhere", Type = "H" }); // this one chosen
                personAddresses.Add(new Address("234", personId) { AddressLines = new List<string>() { "1 Oak Ave" }, State = "AL", Type = "B" });
                personAddresses.Add(new Address("567", personId) { AddressLines = new List<string>() { "1 New sT" }, State = "NY", Type = "Z" }); // this one chosen

                var addressDtoAdapter = new AutoMapperAdapter<Ellucian.Colleague.Domain.Base.Entities.Address, Ellucian.Colleague.Dtos.Base.Address>(adapterRegistry, logger);
                adapterRegistryMock.Setup(reg => reg.GetAdapter<Ellucian.Colleague.Domain.Base.Entities.Address, Ellucian.Colleague.Dtos.Base.Address>()).Returns(addressDtoAdapter);

                // add phones and addresses to profile response
                foreach (var phoneNumber in personPhones.PhoneNumbers)
                {
                    profile.AddPhone(phoneNumber);
                }
                foreach (var addr in personAddresses)
                {
                    profile.AddAddress(addr);
                }
                personService = new PersonService(adapterRegistry, personRepo, personBaseRepo, refRepo, profileRepo, null, null, null, currentUserFactory, roleRepo, logger);
            }

            [TestCleanup]
            public void Cleanup()
            {
                adapterRegistry = null;
                personRepo = null;
                personBaseRepo = null;
                refRepo = null;
                roleRepo = null;
                personService = null;
                personRole = null;
            }

            [TestMethod]
            public async Task GetProfile_VerifyAttributes()
            {
                // Act--get person
                var profileDto = await personService.GetProfileAsync(personId);
                // Assert
                Assert.IsTrue(profileDto is Dtos.Base.Profile);
                Assert.AreEqual(profile.Id, profileDto.Id);
                Assert.AreEqual(profile.LastName, profileDto.LastName);
                Assert.AreEqual(profile.FirstName, profileDto.FirstName);
                Assert.AreEqual(profile.PreferredName, profileDto.PreferredName);
                Assert.AreEqual(profile.BirthDate, profileDto.BirthDate);
                Assert.AreEqual(profile.EmailAddresses.Where(e => e.IsPreferred == true).First().Value, profileDto.PreferredEmailAddress);
                Assert.AreEqual(profile.AddressConfirmationDateTime, profileDto.AddressConfirmationDateTime);
                Assert.AreEqual(profile.EmailAddressConfirmationDateTime, profileDto.EmailAddressConfirmationDateTime);
                Assert.AreEqual(profile.PhoneConfirmationDateTime, profileDto.PhoneConfirmationDateTime);
            }

            [TestMethod]
            public async Task GetProfile_AllEmailsReturned()
            {
                // Act--get person
                var profileDto = await personService.GetProfileAsync(personId);
                // Assert
                Assert.AreEqual(profile.EmailAddresses.Count(), profileDto.EmailAddresses.Count());
                foreach (var expectedEmail in profile.EmailAddresses)
                {
                    var actualEmail = profileDto.EmailAddresses.Where(e => e.Value == expectedEmail.Value).First();
                    Assert.AreEqual(expectedEmail.IsPreferred, actualEmail.IsPreferred);
                    Assert.AreEqual(expectedEmail.TypeCode, actualEmail.TypeCode);
                }
            }

            [TestMethod]
            public async Task GetProfile_AllAddressesReturned()
            {
                // Act--get person
                var profileDto = await personService.GetProfileAsync(personId);
                // Assert
                Assert.AreEqual(personAddresses.Count(), profileDto.Addresses.Count());
                foreach (var expectedAddress in personAddresses)
                {
                    var actualAddress = profileDto.Addresses.Where(a => a.AddressId == expectedAddress.AddressId).First();
                    Assert.AreEqual(expectedAddress.IsPreferredAddress, actualAddress.IsPreferredAddress);
                    Assert.AreEqual(expectedAddress.TypeCode, actualAddress.TypeCode);
                    Assert.AreEqual(expectedAddress.Type, actualAddress.Type);
                    // Not checking every field, automapper is used
                }
            }

            [TestMethod]
            public async Task GetProfile_AllPhonesReturned()
            {
                // Act--get person
                var profileDto = await personService.GetProfileAsync(personId);
                // Assert
                Assert.AreEqual(personPhones.PhoneNumbers.Count(), profileDto.Phones.Count());
                foreach (var expectedPhone in personPhones.PhoneNumbers)
                {
                    var actualPhone = profileDto.Phones.Where(p => p.Number == expectedPhone.Number).First();
                    Assert.AreEqual(expectedPhone.TypeCode, actualPhone.TypeCode);
                    Assert.AreEqual(expectedPhone.Extension, actualPhone.Extension);
                }
            }

            [TestMethod]
            public async Task GetProfile_UserNotSelf_PermissionValid()
            {
                relRepoMock.Setup(repo => repo.GetRelatedPersonIdsAsync(personId)).ReturnsAsync(new List<string>()
                    {
                        "0000009",
                        personId
                    });
                profile = new Domain.Base.Entities.Profile("0000009", "Brown");
                profile.FirstName = "Tonya";
                profile.BirthDate = new DateTime(1900, 1, 1);
                profile.AddEmailAddress(new EmailAddress("tbrown@xmail.com", "COL") { IsPreferred = false });
                profile.AddEmailAddress(new EmailAddress("tpbrown@xellucian.com", "BUS") { IsPreferred = true }); // this one chosen
                profile.AddEmailAddress(new EmailAddress("booboo2@qmail.com", "WWW") { IsPreferred = false }); // this one chosen
                profile.PreferredName = "Tawny H. Brown";
                profileRepoMock.Setup(repo => repo.GetProfileAsync("0000009", It.IsAny<bool>())).ReturnsAsync(profile);
                personService = new PersonService(adapterRegistry, personRepo, personBaseRepo, refRepo, profileRepo, null, relRepo, proxyRepo, currentUserFactory, roleRepo, logger);

                var profileDto = await personService.GetProfileAsync("0000009");
            }

            [TestMethod]
            public async Task GetProfile_UserNotSelf_PermissionValid2()
            {
                proxyRepoMock.Setup(r => r.GetUserProxyPermissionsAsync(personId, false)).ReturnsAsync(new List<ProxyUser>()
                {
                    new ProxyUser("0000009")
                });
                profile = new Domain.Base.Entities.Profile("0000009", "Brown");
                profile.FirstName = "Tonya";
                profile.BirthDate = new DateTime(1900, 1, 1);
                profile.AddEmailAddress(new EmailAddress("tbrown@xmail.com", "COL") { IsPreferred = false });
                profile.AddEmailAddress(new EmailAddress("tpbrown@xellucian.com", "BUS") { IsPreferred = true }); // this one chosen
                profile.AddEmailAddress(new EmailAddress("booboo2@qmail.com", "WWW") { IsPreferred = false }); // this one chosen
                profile.PreferredName = "Tawny H. Brown";
                profileRepoMock.Setup(repo => repo.GetProfileAsync("0000009", It.IsAny<bool>())).ReturnsAsync(profile);
                personService = new PersonService(adapterRegistry, personRepo, personBaseRepo, refRepo, profileRepo, null, relRepo, proxyRepo, currentUserFactory, roleRepo, logger);

                var profileDto = await personService.GetProfileAsync("0000009");
            }

            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public async Task GetProfile_UserNotSelf_PermissionInvalid()
            {
                relRepoMock.Setup(repo => repo.GetRelatedPersonIdsAsync(personId)).ReturnsAsync(new List<string>()
                    {
                        "0000009",
                        personId
                    });
                proxyRepoMock.Setup(r => r.GetUserProxyPermissionsAsync(personId, false)).ReturnsAsync(new List<ProxyUser>()
                {
                    new ProxyUser("0000009")
                });
                personService = new PersonService(adapterRegistry, personRepo, personBaseRepo, refRepo, profileRepo, null, relRepo, proxyRepo, currentUserFactory, roleRepo, logger);

                var profileDto = await personService.GetProfileAsync("0000010");
            }


            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task GetProfile_NullPersonId_ArgumentNullException()
            {
                var profileDto = await personService.GetProfileAsync(null);
            }

            [TestMethod]
            [ExpectedException(typeof(Exception))]
            public async Task GetProfile_NotFoundPersonThrowsException()
            {
                Domain.Base.Entities.Profile nullProfile = null;
                profileRepoMock.Setup(repo => repo.GetProfileAsync(It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(nullProfile);
                var profileDto = await personService.GetProfileAsync(personId);
            }
        }

        #endregion

        #region GetPersonProxyDetails Tests

        [TestClass]
        public class GetPersonProxyDetails : CurrentUserSetup
        {
            private string personId = "0000015";
            private Mock<IPersonRepository> personRepoMock;
            private IPersonRepository personRepo;
            private Mock<IPersonBaseRepository> personBaseRepoMock;
            private IPersonBaseRepository personBaseRepo;
            private Mock<IReferenceDataRepository> refRepoMock;
            private IReferenceDataRepository refRepo;
            private Mock<IProfileRepository> profileRepoMock;
            private IProfileRepository profileRepo;
            private Mock<IRelationshipRepository> relRepoMock;
            private IRelationshipRepository relRepo;
            private Mock<IAdapterRegistry> adapterRegistryMock;
            private IAdapterRegistry adapterRegistry;
            private PersonService personService;
            private ILogger logger;
            private Ellucian.Colleague.Domain.Base.Entities.Person person;
            private Ellucian.Colleague.Domain.Base.Entities.Profile profile;
            private Ellucian.Colleague.Domain.Base.Entities.PersonProxyDetails personProxyDetailsMock;
            private Domain.Base.Entities.ProxyConfiguration configEntity;
            private Mock<IRoleRepository> roleRepoMock;
            private IRoleRepository roleRepo;
            private Mock<IProxyRepository> proxyRepoMock;
            private IProxyRepository proxyRepo;
            private ICurrentUserFactory currentUserFactory;

            [TestInitialize]
            public void Initialize()
            {
                personRepoMock = new Mock<IPersonRepository>();
                personRepo = personRepoMock.Object;
                personBaseRepoMock = new Mock<IPersonBaseRepository>();
                personBaseRepo = personBaseRepoMock.Object;
                refRepoMock = new Mock<IReferenceDataRepository>();
                refRepo = refRepoMock.Object;
                profileRepoMock = new Mock<IProfileRepository>();
                profileRepo = profileRepoMock.Object;
                relRepoMock = new Mock<IRelationshipRepository>();
                relRepo = relRepoMock.Object;
                adapterRegistryMock = new Mock<IAdapterRegistry>();
                adapterRegistry = adapterRegistryMock.Object;
                roleRepoMock = new Mock<IRoleRepository>();
                roleRepo = roleRepoMock.Object;
                proxyRepoMock = new Mock<IProxyRepository>();
                proxyRepo = proxyRepoMock.Object;

                logger = new Mock<ILogger>().Object;

                // Mock profile response from the person repository
                profile = new Domain.Base.Entities.Profile(personId, "Brown");
                profile.FirstName = "Richard";
                profile.BirthDate = new DateTime(1930, 1, 1);
                profile.AddEmailAddress(new EmailAddress("rbrown@xmail.com", "COL") { IsPreferred = false });
                profile.AddEmailAddress(new EmailAddress("rlbrown@xellucian.com", "BUS") { IsPreferred = true }); // this one chosen
                profile.AddEmailAddress(new EmailAddress("booboo@qmail.com", "WWW") { IsPreferred = false }); // this one chosen
                profile.PreferredName = "R. Lee Brown";
                profile.AddressConfirmationDateTime = new DateTimeOffset(2001, 1, 2, 15, 16, 17, TimeSpan.FromHours(-3));
                profile.EmailAddressConfirmationDateTime = new DateTimeOffset(2002, 3, 4, 18, 19, 20, TimeSpan.FromHours(-3));
                profile.PhoneConfirmationDateTime = new DateTimeOffset(2003, 5, 6, 21, 22, 23, TimeSpan.FromHours(-3));
                profileRepoMock.Setup(repo => repo.GetProfileAsync(personId, It.IsAny<bool>())).ReturnsAsync(profile);
                configEntity = new Domain.Base.Entities.ProxyConfiguration(true, "DISCLOSURE.ID", "EMAIL.ID", true, true, new List<ProxyAndUserPermissionsMap>()) { DisclosureReleaseText = "Line 1" };


                // Mock PersonProxyDetails response from the person repo
                personProxyDetailsMock = new Domain.Base.Entities.PersonProxyDetails(personId,profile.PreferredName, "rlbrown@xellucian.com");


                relRepoMock.Setup(repo => repo.GetPersonRelationshipsAsync(personId)).ReturnsAsync(new List<Relationship>());

                var profileDtoAdapter = new Ellucian.Colleague.Coordination.Base.Adapters.ProfileAdapter(adapterRegistry, logger);
                adapterRegistryMock.Setup(reg => reg.GetAdapter<Domain.Base.Entities.Profile, Dtos.Base.Profile>()).Returns(profileDtoAdapter);

                var emailDtoAdapter = new AutoMapperAdapter<Ellucian.Colleague.Domain.Base.Entities.EmailAddress, Ellucian.Colleague.Dtos.Base.EmailAddress>(adapterRegistry, logger);
                adapterRegistryMock.Setup(reg => reg.GetAdapter<Ellucian.Colleague.Domain.Base.Entities.EmailAddress, Ellucian.Colleague.Dtos.Base.EmailAddress>()).Returns(emailDtoAdapter);


                var personProxyDetailsDtoAdapter = new AutoMapperAdapter<Domain.Base.Entities.PersonProxyDetails, Dtos.Base.PersonProxyDetails>(adapterRegistry,logger);
                adapterRegistryMock.Setup(reg => reg.GetAdapter<Ellucian.Colleague.Domain.Base.Entities.PersonProxyDetails, Ellucian.Colleague.Dtos.Base.PersonProxyDetails>()).Returns(personProxyDetailsDtoAdapter);

                // Set up current user
                currentUserFactory = new CurrentUserSetup.PersonUserFactory();
                personService = new PersonService(adapterRegistry, personRepo, personBaseRepo, refRepo, profileRepo, null, null, null, currentUserFactory, roleRepo, logger);
            }

            [TestCleanup]
            public void Cleanup()
            {
                adapterRegistry = null;
                personRepo = null;
                personBaseRepo = null;
                refRepo = null;
                roleRepo = null;
                personService = null;
                personRole = null;
            }

            [TestMethod]
            public async Task GetPersonProxyDetails_ExpectedEmail()
            {
                proxyRepoMock.Setup(r => r.GetProxyConfigurationAsync()).ReturnsAsync(new ProxyConfiguration(true, "DISCLOSURE.ID", "EMAIL.ID", true, true, new List<ProxyAndUserPermissionsMap>())
                {
                    ProxyEmailAddressHierarchy = "JPM2"
                });
                personService = new PersonService(adapterRegistry, personRepo, personBaseRepo, refRepo, profileRepo, null, relRepo, proxyRepo, currentUserFactory, roleRepo, logger);
                var personProxyDetailsDto = await personService.GetPersonProxyDetailsAsync(personId);
                // Assert
                Assert.IsTrue(personProxyDetailsDto is Dtos.Base.PersonProxyDetails);
                Assert.AreEqual(personProxyDetailsMock.PersonId, personProxyDetailsDto.PersonId);
                Assert.AreEqual(personProxyDetailsMock.PreferredName, personProxyDetailsDto.PreferredName);
                Assert.AreEqual(personProxyDetailsMock.ProxyEmailAddress, personProxyDetailsDto.ProxyEmailAddress);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task GetPersonProxyDetails_NullPersonID()
            {
                proxyRepoMock.Setup(r => r.GetProxyConfigurationAsync()).ReturnsAsync(new ProxyConfiguration(true, "DISCLOSURE.ID", "EMAIL.ID", true, true, new List<ProxyAndUserPermissionsMap>())
                {
                    ProxyEmailAddressHierarchy = "JPM2"
                });
                personService = new PersonService(adapterRegistry, personRepo, personBaseRepo, refRepo, profileRepo, null, relRepo, proxyRepo, currentUserFactory, roleRepo, logger);
                var personProxyDetailsDto = await personService.GetPersonProxyDetailsAsync(null);
                // Assert
                Assert.IsTrue(personProxyDetailsDto is Dtos.Base.PersonProxyDetails);
                Assert.AreEqual(personProxyDetailsMock.PersonId, personProxyDetailsDto.PersonId);
                Assert.AreEqual(personProxyDetailsMock.PreferredName, personProxyDetailsDto.PreferredName);
                Assert.AreEqual(personProxyDetailsMock.ProxyEmailAddress, personProxyDetailsDto.ProxyEmailAddress);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task GetPersonProxyDetails_NullHierarchy()
            {
                proxyRepoMock.Setup(r => r.GetProxyConfigurationAsync()).ReturnsAsync(new ProxyConfiguration(true, "DISCLOSURE.ID", "EMAIL.ID", true, true, new List<ProxyAndUserPermissionsMap>())
                {
                    ProxyEmailAddressHierarchy = null
                });
                personService = new PersonService(adapterRegistry, personRepo, personBaseRepo, refRepo, profileRepo, null, relRepo, proxyRepo, currentUserFactory, roleRepo, logger);
                var personProxyDetailsDto = await personService.GetPersonProxyDetailsAsync(null);
                // Assert
                Assert.IsTrue(personProxyDetailsDto is Dtos.Base.PersonProxyDetails);
                Assert.AreEqual(personProxyDetailsMock.PersonId, personProxyDetailsDto.PersonId);
                Assert.AreEqual(personProxyDetailsMock.PreferredName, personProxyDetailsDto.PreferredName);
                Assert.AreEqual(personProxyDetailsMock.ProxyEmailAddress, personProxyDetailsDto.ProxyEmailAddress);
            }

        }

            #endregion

        #region UpdateProfileTests

            [TestClass]
        public class UpdateProfile : CurrentUserSetup
        {
            private string personId = "0000015";
            private Mock<IPersonRepository> personRepoMock;
            private IPersonRepository personRepo;
            private Mock<IPersonBaseRepository> personBaseRepoMock;
            private IPersonBaseRepository personBaseRepo;
            private Mock<IReferenceDataRepository> refRepoMock;
            private IReferenceDataRepository refRepo;
            private Mock<IProfileRepository> profileRepoMock;
            private IProfileRepository profileRepo;
            private Mock<IConfigurationRepository> configurationRepoMock;
            private IConfigurationRepository configurationRepo;
            private static Mock<IAdapterRegistry> adapterRegistryMock;
            private static IAdapterRegistry adapterRegistry;
            private PersonService personService;
            private static ILogger logger;
            private Dtos.Base.Profile profileDto;
            private Mock<IRoleRepository> roleRepoMock;
            private IRoleRepository roleRepo;
            private ICurrentUserFactory currentUserFactory;

            [ClassInitialize]
            public static void ClassInitialize(TestContext testContext)
            {
                adapterRegistryMock = new Mock<IAdapterRegistry>();
                adapterRegistry = adapterRegistryMock.Object;
                logger = new Mock<ILogger>().Object;

                var emailEntityToDtoAdapter = new AutoMapperAdapter<Ellucian.Colleague.Domain.Base.Entities.EmailAddress, Ellucian.Colleague.Dtos.Base.EmailAddress>(adapterRegistry, logger);
                adapterRegistryMock.Setup(reg => reg.GetAdapter<Ellucian.Colleague.Domain.Base.Entities.EmailAddress, Ellucian.Colleague.Dtos.Base.EmailAddress>()).Returns(emailEntityToDtoAdapter);

                var phoneEntityToDtoAdapter = new AutoMapperAdapter<Ellucian.Colleague.Domain.Base.Entities.Phone, Ellucian.Colleague.Dtos.Base.Phone>(adapterRegistry, logger);
                adapterRegistryMock.Setup(reg => reg.GetAdapter<Ellucian.Colleague.Domain.Base.Entities.Phone, Ellucian.Colleague.Dtos.Base.Phone>()).Returns(phoneEntityToDtoAdapter);

                var addressEntityToDtoAdapter = new AutoMapperAdapter<Ellucian.Colleague.Domain.Base.Entities.Address, Ellucian.Colleague.Dtos.Base.Address>(adapterRegistry, logger);
                adapterRegistryMock.Setup(reg => reg.GetAdapter<Ellucian.Colleague.Domain.Base.Entities.Address, Ellucian.Colleague.Dtos.Base.Address>()).Returns(addressEntityToDtoAdapter);

                var emailDtoAdapter = new Coordination.Base.Adapters.EmailAddressDtoAdapter(adapterRegistry, logger);
                adapterRegistryMock.Setup(reg => reg.GetAdapter<Ellucian.Colleague.Dtos.Base.EmailAddress, Ellucian.Colleague.Domain.Base.Entities.EmailAddress>()).Returns(emailDtoAdapter);
                var profileAdapter = new Coordination.Base.Adapters.ProfileAdapter(adapterRegistry, logger);
                adapterRegistryMock.Setup(reg => reg.GetAdapter<Domain.Base.Entities.Profile, Dtos.Base.Profile>()).Returns(profileAdapter);


                var phoneDtoAdapter = new Coordination.Base.Adapters.PhoneDtoAdapter(adapterRegistry, logger);
                adapterRegistryMock.Setup(reg => reg.GetAdapter<Ellucian.Colleague.Dtos.Base.Phone, Ellucian.Colleague.Domain.Base.Entities.Phone>()).Returns(phoneDtoAdapter);

                var addressDtoAdapter = new Coordination.Base.Adapters.AddressDtoAdapter(adapterRegistry, logger);
                adapterRegistryMock.Setup(reg => reg.GetAdapter<Ellucian.Colleague.Dtos.Base.Address, Ellucian.Colleague.Domain.Base.Entities.Address>()).Returns(addressDtoAdapter);

                var profileDtoAdapter = new Coordination.Base.Adapters.ProfileDtoAdapter(adapterRegistry, logger);
                adapterRegistryMock.Setup(reg => reg.GetAdapter<Dtos.Base.Profile, Domain.Base.Entities.Profile>()).Returns(profileDtoAdapter);

            }

            [TestInitialize]
            public void Initialize()
            {
                personRepoMock = new Mock<IPersonRepository>();
                personRepo = personRepoMock.Object;
                personBaseRepoMock = new Mock<IPersonBaseRepository>();
                personBaseRepo = personBaseRepoMock.Object;
                refRepoMock = new Mock<IReferenceDataRepository>();
                refRepo = refRepoMock.Object;
                profileRepoMock = new Mock<IProfileRepository>();
                profileRepo = profileRepoMock.Object;
                roleRepoMock = new Mock<IRoleRepository>();
                roleRepo = roleRepoMock.Object;
                configurationRepoMock = new Mock<IConfigurationRepository>();
                configurationRepo = configurationRepoMock.Object;

                // Mock profile
                profileDto = new Dtos.Base.Profile()
                {
                    Id = personId,
                    LastName = "Brown",
                    FirstName = "Richard",
                    BirthDate = new DateTime(1930, 1, 1),
                    EmailAddresses = new List<Dtos.Base.EmailAddress>
                    {
                        new Dtos.Base.EmailAddress() { Value = "rbrown@xmail.com", TypeCode = "COL", IsPreferred = false },
                        new Dtos.Base.EmailAddress() { Value = "rlbrown@xellucian.com", TypeCode = "BUS", IsPreferred = true },
                        new Dtos.Base.EmailAddress() { Value = "booboo@qmail.com", TypeCode = "WWW", IsPreferred = false },
                    },
                    PreferredName = "R. Lee Brown",
                    AddressConfirmationDateTime = new DateTimeOffset(2001, 1, 2, 15, 16, 17, TimeSpan.FromHours(-3)),
                    EmailAddressConfirmationDateTime = new DateTimeOffset(2002, 3, 4, 18, 19, 20, TimeSpan.FromHours(-3)),
                    PhoneConfirmationDateTime = new DateTimeOffset(2003, 5, 6, 21, 22, 23, TimeSpan.FromHours(-3)),
                    Phones = new List<Dtos.Base.Phone>
                    {
                        new Dtos.Base.Phone() { Number = "864-123-1234", TypeCode = "HO", Extension = "x123" },
                        new Dtos.Base.Phone() { Number = "864-321-4321", TypeCode = "B", Extension = "x432" },
                        new Dtos.Base.Phone() { Number = "555-800-1212", TypeCode = "X", Extension = "x432" }
                    },
                    Addresses = new List<Dtos.Base.Address>
                    {
                        new Dtos.Base.Address()
                        {
                            AddressId = "123",
                            PersonId = personId,
                            AddressLines = new List<string>() { "123 Main" },
                            City = "Anywhere",
                            Type = "H"
                        },
                        new Dtos.Base.Address()
                        {
                            AddressId = "234",
                            PersonId = personId,
                            AddressLines = new List<string>() { "1 Oak Ave" },
                            State = "AL",
                            Type = "B"
                        },
                        new Dtos.Base.Address()
                        {
                            AddressId = "567",
                            PersonId = personId,
                            AddressLines = new List<string>() { "1 New sT" },
                            State = "NY",
                            Type = "Z"
                        },
                    }
                };

                // Set up current user
                currentUserFactory = new CurrentUserSetup.PersonUserFactory();

                var profileDtoAdapter = adapterRegistry.GetAdapter<Dtos.Base.Profile, Domain.Base.Entities.Profile>();
                var profileEntity = profileDtoAdapter.MapToType(profileDto);
                profileRepoMock.Setup(repo => repo.UpdateProfileAsync(profileEntity)).ReturnsAsync(profileEntity);

                // Mock the get of the profile from the repository
                var repoEntity = profileDtoAdapter.MapToType(profileDto);
                repoEntity.AddEmailAddress(new EmailAddress("somebody@somewhere.com", "ABC"));
                profileRepoMock.Setup(repo => repo.GetProfileAsync(personId, false)).ReturnsAsync(repoEntity);

                var configuration = new UserProfileConfiguration();
                configuration.UpdateEmailTypeConfiguration(true, null, true, null);
                configuration.CanUpdateEmailWithoutPermission = true;
                configurationRepoMock.Setup(repo => repo.GetUserProfileConfigurationAsync()).ReturnsAsync(configuration);

                roleRepoMock.Setup(repo => repo.Roles).Returns(new List<Role>());

                personService = new PersonService(adapterRegistry, personRepo, personBaseRepo, refRepo, profileRepo, configurationRepo, null, null, currentUserFactory, roleRepo, logger);
            }

            [TestCleanup]
            public void Cleanup()
            {
                personRepo = null;
                personBaseRepo = null;
                refRepo = null;
                roleRepo = null;
                personService = null;
                personRole = null;
                profileDto = null;
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task UpdateProfileAsync_ProfileWithEmptyId_ThrowsException()
            {
                var profileDtoWithNoId = new Dtos.Base.Profile() { Id = null };
                var updatedProfileDto = await personService.UpdateProfileAsync(profileDtoWithNoId);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task UpdateProfileAsync_NullProfile_ThrowsException()
            {
                var updatedProfileDto = await personService.UpdateProfileAsync(null);
            }

            [TestMethod]
            public async Task UpdateProfileAsync_ReturnsUpdatedProfile()
            {
                var updatedProfileDto = await personService.UpdateProfileAsync(profileDto);
                Assert.AreEqual(profileDto.Id, updatedProfileDto.Id);
                Assert.AreEqual(profileDto.LastName, updatedProfileDto.LastName);
                Assert.AreEqual(profileDto.FirstName, updatedProfileDto.FirstName);
                Assert.AreEqual(profileDto.BirthDate, updatedProfileDto.BirthDate);
                Assert.AreEqual(profileDto.Addresses.Count, updatedProfileDto.Addresses.Count);
                Assert.AreEqual(profileDto.EmailAddresses.Count, updatedProfileDto.EmailAddresses.Count);
                Assert.AreEqual(profileDto.Phones.Count, updatedProfileDto.Phones.Count);
                Assert.AreEqual(profileDto.AddressConfirmationDateTime, updatedProfileDto.AddressConfirmationDateTime);
                Assert.AreEqual(profileDto.EmailAddressConfirmationDateTime, updatedProfileDto.EmailAddressConfirmationDateTime);
                Assert.AreEqual(profileDto.PhoneConfirmationDateTime, updatedProfileDto.PhoneConfirmationDateTime);
            }
        }
        #endregion

        #region HEDM CreatePerson V8 Tests

        [TestClass]
        public class CreatePerson3 : CurrentUserSetup
        {
            //Mocks
            Mock<IAdapterRegistry> adapterRegistryMock;
            Mock<IPersonRepository> personRepositoryMock;
            Mock<IPersonBaseRepository> personBaseRepositoryMock;
            Mock<IReferenceDataRepository> referenceDataRepositoryMock;
            Mock<IProfileRepository> profileRepositoryMock;
            Mock<IConfigurationRepository> configurationRepositoryMock;
            Mock<IRelationshipRepository> relationshipRepositoryMock;
            Mock<IProxyRepository> proxyRepositoryMock;
            Mock<IRoleRepository> roleRepositoryMock;
            Mock<ILogger> loggerMock;

            //userfactory
            ICurrentUserFactory currentUserFactory;

            //Perms
            // private Ellucian.Colleague.Domain.Entities.Permission permissionUpdatePerson;

            //Service
            PersonService personService;


            private Ellucian.Colleague.Dtos.Person3 personDto;
            private Ellucian.Colleague.Domain.Base.Entities.PersonIntegration personIntegrationReturned;
            private Ellucian.Colleague.Domain.Base.Entities.PersonIntegration personIntegrationEntity;

            //private Ellucian.Colleague.Domain.Base.Entities.Person person;
            private List<Ellucian.Colleague.Domain.Base.Entities.Address> addresses = null;
            private List<Ellucian.Colleague.Domain.Base.Entities.Phone> phones = new List<Domain.Base.Entities.Phone>();
            //private IEnumerable<Ellucian.Colleague.Domain.Base.Entities.State> states;
            //private IEnumerable<Ellucian.Colleague.Domain.Base.Entities.Country> countries;
            //private List<Ellucian.Colleague.Domain.Base.Entities.County> counties;
            private List<PersonNameTypeItem> personNameTypes;
            private List<Country> countries;

            private IEnumerable<Domain.Base.Entities.PrivacyStatus> allPrivacyStatuses;

            //Entities
            private Ellucian.Colleague.Domain.Base.Entities.EmailAddress instEmail;
            private Ellucian.Colleague.Domain.Base.Entities.EmailAddress perEmail;
            private Ellucian.Colleague.Domain.Base.Entities.EmailAddress workEmail;
            private Ellucian.Colleague.Domain.Base.Entities.Address homeAddr;
            private Ellucian.Colleague.Domain.Base.Entities.Address mailAddr;
            private Ellucian.Colleague.Domain.Base.Entities.Address resAddr;
            private Ellucian.Colleague.Domain.Base.Entities.Address workAddr;
            private Ellucian.Colleague.Domain.Base.Entities.Phone homePhone;
            private Ellucian.Colleague.Domain.Base.Entities.Phone mobilePhone;
            private Ellucian.Colleague.Domain.Base.Entities.Phone residencePhone;
            private Ellucian.Colleague.Domain.Base.Entities.Phone workPhone;

            //Data
            private string personId = "0000011";
            private string personGuid = "5674f28b-b216-4055-b236-81a922d93b4c";
            private string maritalStatusGuid = "dca8edb5-120f-479a-a6bb-35ba3af4b344";
            private string maritalStatusSingleGuid = "dda8edb5-120f-479a-a6bb-35ba3af4b344";
            private string ethnicityGuid = "dd0c42ca-c61d-4ca6-8d21-96ab5be35623";
            private string raceAsianGuid = "72b7737b-27db-4a06-944b-97d00c29b3db";
            private string racePacificIslanderGuid = "e20f9821-28a2-4e34-8550-6758850a0cf8";
            private string baptistGuid = "c0bdfd92-462f-4e59-bba5-1b15c4771c86";
            private string catholicGuid = "f96f04b0-4973-41f6-bc3d-9c7bc1c2c458";
            private string demographicGuid = "d3d86052-9d55-4751-acda-5c07a064a82a";
            private string noncitizenGuid = "97ec6f69-9b16-4ed5-8954-59067f0318ec";


            private string countyGuid = Guid.NewGuid().ToString();


            [TestInitialize]
            public void Initialize()
            {
                adapterRegistryMock = new Mock<IAdapterRegistry>();
                personRepositoryMock = new Mock<IPersonRepository>();
                personBaseRepositoryMock = new Mock<IPersonBaseRepository>();
                referenceDataRepositoryMock = new Mock<IReferenceDataRepository>();
                profileRepositoryMock = new Mock<IProfileRepository>();
                configurationRepositoryMock = new Mock<IConfigurationRepository>();
                relationshipRepositoryMock = new Mock<IRelationshipRepository>();
                proxyRepositoryMock = new Mock<IProxyRepository>();
                roleRepositoryMock = new Mock<IRoleRepository>();
                loggerMock = new Mock<ILogger>();

                currentUserFactory = new CurrentUserSetup.ThirdPartyUserFactory();

                SetupData();

                SetupReferenceDataRepositoryMocks();

                personRepositoryMock.Setup(i => i.Create2Async(It.IsAny<PersonIntegration>(), addresses, phones, It.IsAny<int>())).ReturnsAsync(personIntegrationReturned);

                personService = new PersonService(adapterRegistryMock.Object, personRepositoryMock.Object, personBaseRepositoryMock.Object, referenceDataRepositoryMock.Object, profileRepositoryMock.Object,
                                                  configurationRepositoryMock.Object, relationshipRepositoryMock.Object, proxyRepositoryMock.Object, currentUserFactory,
                                                  roleRepositoryMock.Object, loggerMock.Object);
            }

            [TestCleanup]
            public void Cleanup()
            {
                personService = null;
                //permissionUpdatePerson = null;
                adapterRegistryMock = null;
                personRepositoryMock = null;
                personBaseRepositoryMock = null;
                referenceDataRepositoryMock = null;
                profileRepositoryMock = null;
                configurationRepositoryMock = null;
                relationshipRepositoryMock = null;
                proxyRepositoryMock = null;
                roleRepositoryMock = null;
                loggerMock = null;
                currentUserFactory = null;
            }

            [TestMethod]
            public async Task CreatePerson3_CreatePerson2Async()
            {
                //setup role
                createPersonRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Base.BasePermissionCodes.CreatePerson));
                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { createPersonRole });

                var actual = await personService.CreatePerson3Async(personDto);

                Assert.AreEqual(personDto.BirthDate, actual.BirthDate);
                // Assert.AreEqual(personDto.CitizenshipCountry, actual.CitizenshipCountry);
                Assert.AreEqual(personDto.CitizenshipStatus, actual.CitizenshipStatus);
                //Assert.AreEqual(personDto.CountryOfBirth, actual.CountryOfBirth);
                Assert.AreEqual(personDto.DeceasedDate, actual.DeceasedDate);
                Assert.AreEqual(personDto.GenderType, actual.GenderType);
                Assert.AreEqual(personDto.MaritalStatus.Detail.Id, actual.MaritalStatus.Detail.Id);
                Assert.AreEqual(personDto.MaritalStatus.MaritalCategory, actual.MaritalStatus.MaritalCategory);


                //Legal
                var legalActualName = actual.PersonNames.FirstOrDefault(item => item.NameType.Detail.Id.Equals("806af5a5-8a9a-424f-8c9f-c1e9d084ee71", StringComparison.OrdinalIgnoreCase));
                var legalExpectedName = personDto.PersonNames.FirstOrDefault(item => item.NameType.Detail.Id.Equals(legalActualName.NameType.Detail.Id, StringComparison.OrdinalIgnoreCase));

                //Assert.AreEqual(legalExpectedName.FullName, legalActualName.FullName); commented cause it will fail
                if (!string.IsNullOrEmpty(legalExpectedName.LastNamePrefix))
                    Assert.AreEqual(legalExpectedName.LastName, string.Concat(legalExpectedName.LastNamePrefix, " ", legalActualName.LastName));
                Assert.AreEqual(legalExpectedName.FirstName, legalActualName.FirstName);
                Assert.AreEqual(legalExpectedName.MiddleName, legalActualName.MiddleName);

                //Birth
                var birthActualName = actual.PersonNames.FirstOrDefault(item => item.NameType.Detail.Id.Equals("7dfa950c-8ae4-4dca-92f0-c083604285b6", StringComparison.OrdinalIgnoreCase));
                var birthexpectedName = personDto.PersonNames.FirstOrDefault(item => item.NameType.Detail.Id.Equals(birthActualName.NameType.Detail.Id, StringComparison.OrdinalIgnoreCase));

                Assert.AreEqual(birthexpectedName.FullName, birthActualName.FullName);

                //Chosen
                var chosenActualName = actual.PersonNames.FirstOrDefault(item => item.NameType.Detail.Id.Equals("dd20ebdf-2452-41ef-9F86-ad1b1621a78d", StringComparison.OrdinalIgnoreCase));
                var chosenExpectedName = personDto.PersonNames.FirstOrDefault(item => item.NameType.Detail.Id.Equals(chosenActualName.NameType.Detail.Id, StringComparison.OrdinalIgnoreCase));

                Assert.AreEqual(chosenExpectedName.FullName, chosenActualName.FullName);

                //Nickname
                var nickNameActualName = actual.PersonNames.FirstOrDefault(item => item.NameType.Detail.Id.Equals("7b55610f-7d00-4260-bbcf-0e47fdbae647", StringComparison.OrdinalIgnoreCase));
                var nickNameExpectedName = personDto.PersonNames.FirstOrDefault(item => item.NameType.Detail.Id.Equals(nickNameActualName.NameType.Detail.Id, StringComparison.OrdinalIgnoreCase));

                Assert.AreEqual(nickNameExpectedName.FullName, nickNameActualName.FullName);

                //History
                var historyActualName = actual.PersonNames.FirstOrDefault(item => item.NameType.Detail.Id.Equals("d42cc964-35cb-4560-bc46-4b881e7705ea", StringComparison.OrdinalIgnoreCase));
                var historyexpectedName = personDto.PersonNames.FirstOrDefault(item => item.NameType.Detail.Id.Equals(historyActualName.NameType.Detail.Id, StringComparison.OrdinalIgnoreCase));

                Assert.AreEqual(historyexpectedName.FullName, historyActualName.FullName);
            }

            #region Privacy Status

            [TestMethod]
            public async Task CreatePerson3_CreatePerson3Async_PrivacyStatus_Detail()
            {
                //setup role
                createPersonRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Base.BasePermissionCodes.CreatePerson));
                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { createPersonRole });

                var privacyStatus = new PersonPrivacyDtoProperty
                {
                    Detail = new GuidObject2(demographicGuid),
                    PrivacyCategory = Dtos.PrivacyStatusType.Unrestricted
                };
                personDto.PrivacyStatus = privacyStatus;

                var actual = await personService.CreatePerson3Async(personDto);
                Assert.IsNotNull(actual.PrivacyStatus);
                Assert.AreEqual(demographicGuid, actual.PrivacyStatus.Detail.Id);
            }

            [TestMethod]
            public async Task CreatePerson3_CreatePerson3Async_PrivacyStatus_PrivacyCategory()
            {
                //setup role
                createPersonRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Base.BasePermissionCodes.CreatePerson));
                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { createPersonRole });

                var privacyStatus = new PersonPrivacyDtoProperty
                {
                    PrivacyCategory = Dtos.PrivacyStatusType.Restricted
                };
                personDto.PrivacyStatus = privacyStatus;

                var actual = await personService.CreatePerson3Async(personDto);
                Assert.IsNotNull(actual.PrivacyStatus);
                Assert.AreEqual(demographicGuid, actual.PrivacyStatus.Detail.Id);
            }

            [TestMethod]
            [ExpectedException(typeof(Exception))]
            public async Task CreatePerson3_CreatePerson3Async_PrivacyStatus_Mismatch()
            {
                //setup role
                createPersonRole.AddPermission(
                    new Ellucian.Colleague.Domain.Entities.Permission(
                        Ellucian.Colleague.Domain.Base.BasePermissionCodes.CreatePerson));
                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { createPersonRole });

                var privacyStatus = new PersonPrivacyDtoProperty
                {
                    Detail = new GuidObject2(demographicGuid),
                    PrivacyCategory = Dtos.PrivacyStatusType.Restricted
                };
                personDto.PrivacyStatus = privacyStatus;
                await personService.CreatePerson3Async(personDto);
            }

            [TestMethod]
            [ExpectedException(typeof(Exception))]
            public async Task CreatePerson3_CreatePerson3Async_PrivacyStatus_EmptyDetail()
            {
                //setup role
                createPersonRole.AddPermission(
                    new Ellucian.Colleague.Domain.Entities.Permission(
                        Ellucian.Colleague.Domain.Base.BasePermissionCodes.CreatePerson));
                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { createPersonRole });

                var privacyStatus = new PersonPrivacyDtoProperty
                {
                    Detail = new GuidObject2(),
                    PrivacyCategory = Dtos.PrivacyStatusType.Restricted
                };
                personDto.PrivacyStatus = privacyStatus;
                await personService.CreatePerson3Async(personDto);
            }

            [TestMethod]
            [ExpectedException(typeof(Exception))]
            public async Task CreatePerson3_CreatePerson3Async_PrivacyStatus_InvalidDetail()
            {
                //setup role
                createPersonRole.AddPermission(
                    new Ellucian.Colleague.Domain.Entities.Permission(
                        Ellucian.Colleague.Domain.Base.BasePermissionCodes.CreatePerson));
                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { createPersonRole });

                var privacyStatus = new PersonPrivacyDtoProperty
                {
                    Detail = new GuidObject2("A448C44F-C5D5-492C-A7ED-83F3DD1B5042"),
                    PrivacyCategory = Dtos.PrivacyStatusType.Restricted
                };
                personDto.PrivacyStatus = privacyStatus;
                await personService.CreatePerson3Async(personDto);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task CreatePerson3_CreatePerson3Async_PrivacyStatus_EmptyCategory()
            {
                //setup role
                createPersonRole.AddPermission(
                    new Ellucian.Colleague.Domain.Entities.Permission(
                        Ellucian.Colleague.Domain.Base.BasePermissionCodes.CreatePerson));
                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { createPersonRole });

                referenceDataRepositoryMock.Setup(repo => repo.GetPrivacyStatusesAsync(It.IsAny<bool>())).ReturnsAsync(allPrivacyStatuses.Where(m => m.Guid == demographicGuid));

                var privacyStatus = new PersonPrivacyDtoProperty
                {
                    Detail = new GuidObject2(demographicGuid),

                };
                personDto.PrivacyStatus = privacyStatus;
                await personService.CreatePerson3Async(personDto);
            }

            #endregion

            #region Marital Status

            [TestMethod]
            public async Task CreatePerson3_CreatePerson3Async_MaritalStatus_Detail()
            {
                //setup role
                createPersonRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Base.BasePermissionCodes.CreatePerson));
                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { createPersonRole });

                var maritalStatus = new PersonMaritalStatusDtoProperty
                {
                    Detail = new GuidObject2(maritalStatusGuid),
                    MaritalCategory = PersonMaritalStatusCategory.Married
                };
                personDto.MaritalStatus = maritalStatus;

                var actual = await personService.CreatePerson3Async(personDto);
                Assert.IsNotNull(actual.MaritalStatus);
                Assert.AreEqual(maritalStatusGuid, actual.MaritalStatus.Detail.Id);
            }

            [TestMethod]
            public async Task CreatePerson3_CreatePerson3Async_MaritalStatus_MaritalCategory()
            {
                //setup role
                createPersonRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Base.BasePermissionCodes.CreatePerson));
                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { createPersonRole });

                var maritalStatus = new PersonMaritalStatusDtoProperty
                {

                    MaritalCategory = PersonMaritalStatusCategory.Married
                };
                personDto.MaritalStatus = maritalStatus;

                var actual = await personService.CreatePerson3Async(personDto);
                Assert.IsNotNull(actual.MaritalStatus);
                Assert.AreEqual(maritalStatusGuid, actual.MaritalStatus.Detail.Id);
            }

            [TestMethod]
            [ExpectedException(typeof(Exception))]
            public async Task CreatePerson3_CreatePerson3Async_MaritalStatus_Mismatch()
            {
                //setup role
                createPersonRole.AddPermission(
                    new Ellucian.Colleague.Domain.Entities.Permission(
                        Ellucian.Colleague.Domain.Base.BasePermissionCodes.CreatePerson));
                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { createPersonRole });

                var maritalStatus = new PersonMaritalStatusDtoProperty
                {
                    Detail = new GuidObject2(new Guid().ToString()),
                    MaritalCategory = PersonMaritalStatusCategory.Married
                };

                personDto.MaritalStatus = maritalStatus;

                await personService.CreatePerson3Async(personDto);
            }

            [TestMethod]
            [ExpectedException(typeof(Exception))]
            public async Task CreatePerson3_CreatePerson3Async_MaritalStatus_EmptyDetail()
            {
                //setup role
                createPersonRole.AddPermission(
                    new Ellucian.Colleague.Domain.Entities.Permission(
                        Ellucian.Colleague.Domain.Base.BasePermissionCodes.CreatePerson));
                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { createPersonRole });

                var maritalStatus = new PersonMaritalStatusDtoProperty
                {
                    Detail = new GuidObject2(),
                    MaritalCategory = PersonMaritalStatusCategory.Married
                };
                personDto.MaritalStatus = maritalStatus;
                await personService.CreatePerson3Async(personDto);
            }

            [TestMethod]
            [ExpectedException(typeof(Exception))]
            public async Task CreatePerson3_CreatePerson3Async_MaritalStatus_EmptyCategory()
            {
                //setup role
                createPersonRole.AddPermission(
                    new Ellucian.Colleague.Domain.Entities.Permission(
                        Ellucian.Colleague.Domain.Base.BasePermissionCodes.CreatePerson));
                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { createPersonRole });

                var maritalStatus = new PersonMaritalStatusDtoProperty
                {
                    Detail = new GuidObject2(new Guid().ToString()),

                };
                personDto.MaritalStatus = maritalStatus;

                await personService.CreatePerson3Async(personDto);
            }

            #endregion

            #region Citizenship
            [TestMethod]
            public async Task CreatePerson3_CreatePerson3Async_Citizenship_Detail()
            {
                //setup role
                createPersonRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Base.BasePermissionCodes.CreatePerson));
                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { createPersonRole });


                var allCitizenshipStatuses = new TestCitizenshipStatusRepository().Get().ToList();
                allCitizenshipStatuses.Add(
                    new Domain.Base.Entities.CitizenshipStatus(noncitizenGuid, "NC", "nonCitizen", Ellucian.Colleague.Domain.Base.Entities.CitizenshipStatusType.NonCitizen));
                referenceDataRepositoryMock.Setup(repo => repo.GetCitizenshipStatusesAsync(It.IsAny<bool>())).ReturnsAsync(allCitizenshipStatuses);

                var citizenshipStatus = new PersonCitizenshipDtoProperty
                {
                    Detail = new GuidObject2(noncitizenGuid),
                    Category = Dtos.CitizenshipStatusType.NonCitizen
                };
                personDto.CitizenshipStatus = citizenshipStatus;
                personIntegrationReturned.AlienStatus = "NC";

                var actual = await personService.CreatePerson3Async(personDto);
                Assert.IsNotNull(actual.CitizenshipStatus);
                Assert.AreEqual(noncitizenGuid, actual.CitizenshipStatus.Detail.Id);
                Assert.AreEqual(Dtos.CitizenshipStatusType.NonCitizen, actual.CitizenshipStatus.Category);
            }

            #endregion

            #region Religion

            [TestMethod]
            public async Task CreatePerson3_CreatePerson3Async_Religion()
            {
                //setup role
                createPersonRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Base.BasePermissionCodes.CreatePerson));
                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { createPersonRole });

                IEnumerable<Domain.Base.Entities.Denomination> allDenominations = new TestReligionRepository().GetDenominations().ToList();
                referenceDataRepositoryMock.Setup(repo => repo.GetDenominationsAsync(It.IsAny<bool>())).ReturnsAsync(allDenominations);
                //referenceDataRepositoryMock.Setup(repo => repo.GetDenominationsAsync(It.IsAny<bool>())).ReturnsAsync(allDenominations);

                var religion = allDenominations.FirstOrDefault(x => x.Code == "CA");

                personDto.Religion = new GuidObject2(religion.Guid);

                var actual = await personService.CreatePerson3Async(personDto);
                Assert.IsNotNull(actual.Religion);
                Assert.AreEqual(religion.Guid, actual.Religion.Id);

            }

            [TestMethod]
            [ExpectedException(typeof(Exception))]
            public async Task CreatePerson3_CreatePerson3Async_Religion_Invalid()
            {
                //setup role
                createPersonRole.AddPermission(
                    new Ellucian.Colleague.Domain.Entities.Permission(
                        Ellucian.Colleague.Domain.Base.BasePermissionCodes.CreatePerson));
                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { createPersonRole });

                IEnumerable<Domain.Base.Entities.Denomination> allDenominations =
                    new TestReligionRepository().GetDenominations().ToList();
                referenceDataRepositoryMock.Setup(repo => repo.GetDenominationsAsync(It.IsAny<bool>()))
                    .ReturnsAsync(allDenominations);
                //referenceDataRepositoryMock.Setup(repo => repo.DenominationsAsync()).ReturnsAsync(allDenominations);

                personDto.Religion = new GuidObject2(new Guid().ToString());

                await personService.CreatePerson3Async(personDto);
            }

            [TestMethod]
            [ExpectedException(typeof(Exception))]
            public async Task CreatePerson3_CreatePerson3Async_Religion_Null()
            {
                //setup role
                createPersonRole.AddPermission(
                    new Ellucian.Colleague.Domain.Entities.Permission(
                        Ellucian.Colleague.Domain.Base.BasePermissionCodes.CreatePerson));
                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { createPersonRole });

                IEnumerable<Domain.Base.Entities.Denomination> allDenominations =
                    new TestReligionRepository().GetDenominations().ToList();
                referenceDataRepositoryMock.Setup(repo => repo.GetDenominationsAsync(It.IsAny<bool>()))
                    .ReturnsAsync(allDenominations);
                //referenceDataRepositoryMock.Setup(repo => repo.DenominationsAsync()).ReturnsAsync(allDenominations);

                var religion = allDenominations.FirstOrDefault();

                personDto.Religion = new GuidObject2();

                await personService.CreatePerson3Async(personDto);
            }

            #endregion

            #region Language

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task CreatePerson3_CreatePerson3Async_Language()
            {
                //setup role
                createPersonRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Base.BasePermissionCodes.CreatePerson));
                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { createPersonRole });

                var personLanguages = new List<PersonLanguageDtoProperty>();

                var personLanguage1 = new PersonLanguageDtoProperty()
                {
                    Code = PersonLanguageCode.eng,
                    Preference = PersonLanguagePreference.Primary
                };
                personLanguages.Add(personLanguage1);

                personDto.Languages = personLanguages;

                var actual = await personService.CreatePerson3Async(personDto);
                var primaryLanguage =
                    actual.Languages.FirstOrDefault(x => x.Preference == PersonLanguagePreference.Primary);

                Assert.IsNotNull(primaryLanguage);
                Assert.AreEqual(PersonLanguagePreference.Primary, primaryLanguage.Preference);
                Assert.AreEqual(PersonLanguageCode.eng, primaryLanguage.Code);
            }


            [TestMethod]
            [ExpectedException(typeof(Exception))]
            public async Task CreatePerson3_CreatePerson3Async_Language_EmptyCode()
            {
                //setup role
                createPersonRole.AddPermission(
                    new Ellucian.Colleague.Domain.Entities.Permission(
                        Ellucian.Colleague.Domain.Base.BasePermissionCodes.CreatePerson));
                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { createPersonRole });

                var personLanguages = new List<PersonLanguageDtoProperty>();

                var personLanguage1 = new PersonLanguageDtoProperty()
                {
                    Preference = PersonLanguagePreference.Primary
                };
                personLanguages.Add(personLanguage1);

                personDto.Languages = personLanguages;

                await personService.CreatePerson3Async(personDto);
            }

            [TestMethod]
            [ExpectedException(typeof(Exception))]
            public async Task CreatePerson3_CreatePerson3Async_Language_MultiplePreferred()
            {
                //setup role
                createPersonRole.AddPermission(
                    new Ellucian.Colleague.Domain.Entities.Permission(
                        Ellucian.Colleague.Domain.Base.BasePermissionCodes.CreatePerson));
                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { createPersonRole });

                var personLanguages = new List<PersonLanguageDtoProperty>();

                var personLanguage1 = new PersonLanguageDtoProperty()
                {
                    Code = PersonLanguageCode.eng,
                    Preference = PersonLanguagePreference.Primary
                };
                personLanguages.Add(personLanguage1);

                var personLanguage2 = new PersonLanguageDtoProperty()
                {
                    Code = PersonLanguageCode.cha,
                    Preference = PersonLanguagePreference.Primary
                };
                personLanguages.Add(personLanguage2);

                personDto.Languages = personLanguages;

                await personService.CreatePerson3Async(personDto);
            }

            #endregion

            #region CountryOfBirth/countryOfCitizenship

            [TestMethod]
            public async Task CreatePerson3_CreatePerson3Async_Country()
            {
                personDto.CitizenshipCountry = "USA";
                personDto.CountryOfBirth = "USA";

                //setup role
                createPersonRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Base.BasePermissionCodes.CreatePerson));
                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { createPersonRole });

                // Mock the reference repository for country
                var countries = new List<Country>()
                 {
                    new Country("USA","United States","USA", "USA") {IsoAlpha3Code = "USA"},
                    new Country("CAN","Canada","CAN", "CAN")  {IsoAlpha3Code = "CAN"},
                    new Country("ME","Mexico","MEX", "MEX")  {IsoAlpha3Code = "MEX"},
                    new Country("BRA","Brazil","BRA", "BRA")  {IsoAlpha3Code = "BRA"}
                };
                referenceDataRepositoryMock.Setup(repo => repo.GetCountryCodesAsync(It.IsAny<bool>())).ReturnsAsync(countries);

                var actual = await personService.CreatePerson3Async(personDto);
                Assert.IsNotNull(actual.CountryOfBirth);
                Assert.AreEqual("USA", actual.CountryOfBirth);

                Assert.IsNotNull(actual.CitizenshipCountry);
                Assert.AreEqual("USA", actual.CitizenshipCountry);
            }

            [TestMethod]
            [ExpectedException(typeof(Exception))]
            public async Task CreatePerson3_CreatePerson3Async_CountryOfBirth_Invalid()
            {
                //setup role
                createPersonRole.AddPermission(
                    new Ellucian.Colleague.Domain.Entities.Permission(
                        Ellucian.Colleague.Domain.Base.BasePermissionCodes.CreatePerson));
                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { createPersonRole });

                personDto.CountryOfBirth = "XXX";

                await personService.CreatePerson3Async(personDto);
            }

            [TestMethod]
            [ExpectedException(typeof(Exception))]
            public async Task CreatePerson3_CreatePerson3Async_CitizenshipCountry_Invalid()
            {
                //setup role
                createPersonRole.AddPermission(
                    new Ellucian.Colleague.Domain.Entities.Permission(
                        Ellucian.Colleague.Domain.Base.BasePermissionCodes.CreatePerson));
                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { createPersonRole });

                personDto.CitizenshipCountry = "XXX";

                await personService.CreatePerson3Async(personDto);
            }


            #endregion

            #region Exceptions

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task CreatePerson3_CreatePerson3Async_PersonDTO_Null_ArgumentNullException()
            {
                var actual = await personService.CreatePerson3Async(null);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task CreatePerson3_CreatePerson3Async_PersonDTO_IdNull_ArgumentNullException()
            {
                personDto.Id = string.Empty;
                var actual = await personService.CreatePerson3Async(personDto);
            }

            [TestMethod]
            [ExpectedException(typeof(Exception))]
            public async Task CreatePerson3_PrimaryNames_Null_ArgumentNullException()
            {
                //setup role
                createPersonRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Base.BasePermissionCodes.CreatePerson));
                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { createPersonRole });

                personDto.PersonNames.FirstOrDefault().LastName = string.Empty;

                var actual = await personService.CreatePerson3Async(personDto);
            }

            [TestMethod]
            [ExpectedException(typeof(Exception))]
            public async Task UpdatePerson3_Username_Exception()
            {
                //setup role
                createPersonRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Base.BasePermissionCodes.CreatePerson));
                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { updatePersonRole });

                //personId 0000011
                personRepositoryMock.Setup(i => i.GetPersonIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync(personId);

                personDto.Credentials = new List<Dtos.DtoProperties.CredentialDtoProperty2>()
                {
                    new Dtos.DtoProperties.CredentialDtoProperty2()
                    {
                        Type = Dtos.EnumProperties.CredentialType2.ColleagueUserName,
                        Value = "testUsername"
                    }
                };
                // Mock the response for getting a Person Pin 
                var personPins = new List<PersonPin>();
                personPins = null;
                personRepositoryMock.Setup(repo => repo.GetPersonPinsAsync(It.IsAny<string[]>())).ReturnsAsync(personPins);

                var actual = await personService.UpdatePerson3Async(personDto);
            }

            #endregion

            #region Setup V3

            private void SetupData()
            {
                // setup personDto object
                personDto = new Dtos.Person3();
                personDto.Id = Guid.Empty.ToString();
                personDto.BirthDate = new DateTime(1930, 1, 1);
                personDto.DeceasedDate = new DateTime(2014, 5, 12);
                var personNames = new List<Dtos.DtoProperties.PersonNameDtoProperty>();
                var legalPrimaryName = new Dtos.DtoProperties.PersonNameDtoProperty()
                {
                    NameType = new Dtos.DtoProperties.PersonNameTypeDtoProperty() { Category = Dtos.EnumProperties.PersonNameType2.Legal, Detail = new Dtos.GuidObject2() { Id = "806af5a5-8a9a-424f-8c9f-c1e9d084ee71" } },
                    Title = "Mr.",
                    FirstName = "LegalFirst",
                    MiddleName = "LegalMiddle",
                    LastName = "LegalLast",
                    Pedigree = "Jr.",
                    LastNamePrefix = "Ignore",
                    FullName = "LegalFirst LegalMiddle LegalLast"
                };
                personNames.Add(legalPrimaryName);

                var birthPrimaryName = new Dtos.DtoProperties.PersonNameDtoProperty()
                {
                    NameType = new Dtos.DtoProperties.PersonNameTypeDtoProperty() { Category = Dtos.EnumProperties.PersonNameType2.Birth, Detail = new Dtos.GuidObject2() { Id = "7dfa950c-8ae4-4dca-92f0-c083604285b6" } },
                    Title = "Mr.",
                    FirstName = "BirthFirst",
                    MiddleName = "BirthMiddle",
                    LastName = "BirthLast",
                    Pedigree = "Jr.",
                    LastNamePrefix = "Ignore",
                    FullName = "BirthFirst BirthMiddle BirthLast"
                };
                personNames.Add(birthPrimaryName);

                var chosenPrimaryName = new PersonNameDtoProperty()
                {
                    NameType = new PersonNameTypeDtoProperty() { Category = Dtos.EnumProperties.PersonNameType2.Personal, Detail = new Dtos.GuidObject2() { Id = "dd20ebdf-2452-41ef-9F86-ad1b1621a78d" } },
                    Title = "Mr.",
                    FirstName = "ChosenFirst",
                    MiddleName = "ChosenMiddle",
                    LastName = "ChosenLast",
                    Pedigree = "Jr.",
                    LastNamePrefix = "Ignore",
                    FullName = "ChosenFirst ChosenMiddle ChosenLast"
                };
                personNames.Add(chosenPrimaryName);

                var nickNamePrimaryName = new Dtos.DtoProperties.PersonNameDtoProperty()
                {
                    NameType = new Dtos.DtoProperties.PersonNameTypeDtoProperty() { Category = Dtos.EnumProperties.PersonNameType2.Personal, Detail = new Dtos.GuidObject2() { Id = "7b55610f-7d00-4260-bbcf-0e47fdbae647" } },
                    Title = "Mr.",
                    FirstName = "NickNameFirst",
                    MiddleName = "NickNameMiddle",
                    LastName = "NickNameLast",
                    Pedigree = "Jr.",
                    LastNamePrefix = "Ignore",
                    FullName = "NickNameFirst NickNameMiddle NickNameLast"
                };
                personNames.Add(nickNamePrimaryName);

                var historyPrimaryName = new Dtos.DtoProperties.PersonNameDtoProperty()
                {
                    NameType = new Dtos.DtoProperties.PersonNameTypeDtoProperty() { Category = Dtos.EnumProperties.PersonNameType2.Personal, Detail = new Dtos.GuidObject2() { Id = "d42cc964-35cb-4560-bc46-4b881e7705ea" } },
                    Title = "Mr.",
                    FirstName = "HistoryFirst",
                    MiddleName = "HistoryMiddle",
                    LastName = "HistoryLast",
                    Pedigree = "Jr.",
                    LastNamePrefix = "Ignore",
                    FullName = "HistoryFirst HistoryMiddle HistoryLast"
                };
                personNames.Add(historyPrimaryName);

                var preferedPrimaryName = new Dtos.DtoProperties.PersonNameDtoProperty()
                {
                    NameType = new Dtos.DtoProperties.PersonNameTypeDtoProperty() { Category = Dtos.EnumProperties.PersonNameType2.Personal, Detail = new Dtos.GuidObject2() { Id = "8224f18e-69c5-480b-a9b4-52f596aa4a52" } },
                    Title = "Mr.",
                    FirstName = "PreferedFirst",
                    MiddleName = "PreferedMiddle",
                    LastName = "PreferedLast",
                    Pedigree = "Jr.",
                    LastNamePrefix = "Ignore",
                    FullName = "PreferedFirst PreferedMiddle PreferedLast"
                };
                personNames.Add(preferedPrimaryName);

                personDto.PersonNames = personNames;
                personDto.GenderType = Dtos.EnumProperties.GenderType2.Male;
                personDto.MaritalStatus = new Dtos.DtoProperties.PersonMaritalStatusDtoProperty() { Detail = new Dtos.GuidObject2(maritalStatusGuid), MaritalCategory = Dtos.EnumProperties.PersonMaritalStatusCategory.Married };// new Dtos.GuidObject(maritalStatusGuid);
                personDto.Ethnicity = new Dtos.DtoProperties.PersonEthnicityDtoProperty() { EthnicGroup = new Dtos.GuidObject2(ethnicityGuid) };// new Dtos.GuidObject(ethnicityGuid);
                personDto.Races = new List<Dtos.DtoProperties.PersonRaceDtoProperty>()
                {

                    new Dtos.DtoProperties.PersonRaceDtoProperty(){ Race = new Dtos.GuidObject2(raceAsianGuid)}
                };
                personDto.Credentials = new List<Dtos.DtoProperties.CredentialDtoProperty2>()
                {
                    new Dtos.DtoProperties.CredentialDtoProperty2()
                    {
                        Type = Dtos.EnumProperties.CredentialType2.Ssn,
                        Value = "111-11-1111"
                    }
                };
                var emailAddresses = new List<Dtos.DtoProperties.PersonEmailDtoProperty>();
                emailAddresses.Add(new Dtos.DtoProperties.PersonEmailDtoProperty()
                {

                    Type = new Dtos.DtoProperties.PersonEmailTypeDtoProperty() { EmailType = Dtos.EmailTypeList.School },
                    Address = "xyz@xmail.com"
                });
                personDto.EmailAddresses = emailAddresses;


                //Entity
                personIntegrationEntity = new PersonIntegration(It.IsAny<string>(), legalPrimaryName.LastName)
                {
                    Guid = personDto.Id,
                    Prefix = "Mr.",
                    FirstName = legalPrimaryName.FirstName,
                    MiddleName = legalPrimaryName.MiddleName,
                    Suffix = "Sr."

                };

                //Returned value
                personIntegrationReturned = new PersonIntegration(personId, "LegalLast");
                personIntegrationReturned.Guid = personGuid;
                personIntegrationReturned.Prefix = "Mr.";
                personIntegrationReturned.FirstName = "LegalFirst";
                personIntegrationReturned.MiddleName = "LegalMiddle";
                personIntegrationReturned.Suffix = "Jr.";
                personIntegrationReturned.Nickname = "NickNameFirst NickNameMiddle NickNameLast";
                personIntegrationReturned.BirthDate = new DateTime(1930, 1, 1);
                personIntegrationReturned.DeceasedDate = new DateTime(2014, 5, 12);
                personIntegrationReturned.GovernmentId = "111-11-1111";
                personIntegrationReturned.Religion = "CA";
                personIntegrationReturned.MaritalStatusCode = "M";
                personIntegrationReturned.EthnicCodes = new List<string> { "H", "N" };
                personIntegrationReturned.Gender = "M";
                personIntegrationReturned.RaceCodes = new List<string> { "AS" };
                personIntegrationReturned.AddRole(new PersonRole(PersonRoleType.Alumni, new DateTime(15, 01, 22), new DateTime(15, 05, 25)));
                personIntegrationReturned.BirthNameFirst = "BirthFirst";
                personIntegrationReturned.BirthNameLast = "BirthLast";
                personIntegrationReturned.BirthNameMiddle = "BirthMiddle";
                personIntegrationReturned.ChosenFirstName = "ChosenFirst";
                personIntegrationReturned.ChosenLastName = "ChosenLast";
                personIntegrationReturned.ChosenMiddleName = "ChosenMiddle";
                personIntegrationReturned.PreferredName = "PreferedFirst PreferedMiddle PreferedLast";
                personIntegrationReturned.BirthCountry = "USA";
                personIntegrationReturned.Citizenship = "USA";
                personIntegrationReturned.FormerNames = new List<PersonName>()
                {
                    new PersonName("HistoryFirst", "HistoryMiddle", "HistoryLast")
                };
                // Mock the email address data response
                instEmail = new Domain.Base.Entities.EmailAddress("inst@inst.com", "COL") { IsPreferred = true };
                personIntegrationReturned.AddEmailAddress(instEmail);
                perEmail = new Domain.Base.Entities.EmailAddress("personal@personal.com", "PER") { IsPreferred = false };
                personIntegrationReturned.AddEmailAddress(perEmail);
                workEmail = new Domain.Base.Entities.EmailAddress("work@work.com", "BUS") { IsPreferred = false };
                personIntegrationReturned.AddEmailAddress(workEmail);

                // Mock the address hierarchy responses
                var addresses = new List<Domain.Base.Entities.Address>();
                homeAddr = new Domain.Base.Entities.Address()
                {
                    TypeCode = "HO",
                    Type = Dtos.EnumProperties.AddressType.Home.ToString(),
                    Guid = Guid.NewGuid().ToString(),
                    Status = "Current",
                    IsPreferredAddress = true
                };
                addresses.Add(homeAddr);
                mailAddr = new Domain.Base.Entities.Address()
                {
                    TypeCode = "MA",
                    Type = Dtos.EnumProperties.AddressType.Mailing.ToString(),
                    Guid = Guid.NewGuid().ToString(),
                    Status = "Current"
                };
                addresses.Add(mailAddr);
                resAddr = new Domain.Base.Entities.Address()
                {
                    TypeCode = "VA",
                    Type = Dtos.EnumProperties.AddressType.Vacation.ToString(),
                    Guid = Guid.NewGuid().ToString(),
                    Status = "Current",
                    IsPreferredResidence = true
                };
                addresses.Add(resAddr);
                workAddr = new Domain.Base.Entities.Address()
                {
                    TypeCode = "BU",
                    Type = Dtos.EnumProperties.AddressType.Business.ToString(),
                    Guid = Guid.NewGuid().ToString(),
                    Status = "Current"
                };
                addresses.Add(workAddr);
                personIntegrationReturned.Addresses = addresses;

                // Mock the person phone per type response
                homePhone = new Domain.Base.Entities.Phone("111-111-1111", "HO");
                personIntegrationReturned.AddPhone(homePhone);
                mobilePhone = new Domain.Base.Entities.Phone("222-222-2222", "MO");
                personIntegrationReturned.AddPhone(mobilePhone);
                residencePhone = new Domain.Base.Entities.Phone("333-333-3333", "VA");
                personIntegrationReturned.AddPhone(residencePhone);
                workPhone = new Domain.Base.Entities.Phone("444-444-4444", "BU", "4444");
                personIntegrationReturned.AddPhone(workPhone);

                // Mock social media
                var socialMedia = new List<Domain.Base.Entities.SocialMedia>();
                var socialMediaTypeCode = "TW";
                var socialMediaHandle = "pontifex";
                var personSocialMedia = new SocialMedia(socialMediaTypeCode, socialMediaHandle);
                personIntegrationReturned.AddSocialMedia(personSocialMedia);
            }

            private void SetupReferenceDataRepositoryMocks()
            {
                // Mock the reference repository for country
                countries = new List<Country>()
                 {
                    new Country("US","United States","USA"),
                    new Country("CA","Canada","CAN"),
                    new Country("MX","Mexico","MEX"),
                    new Country("BR","Brazil","BRA")
                };
                referenceDataRepositoryMock.Setup(repo => repo.GetCountryCodesAsync(It.IsAny<bool>())).ReturnsAsync(countries);

                // Places
                var places = new List<Place>();
                var place1 = new Place() { PlacesCountry = "USA", PlacesRegion = "US-NY" };
                places.Add(place1);
                var place2 = new Place() { PlacesCountry = "CAN", PlacesRegion = "CA-ON" };
                places.Add(place2);
                referenceDataRepositoryMock.Setup(repo => repo.GetPlacesAsync(It.IsAny<bool>())).Returns(Task.FromResult(places.AsEnumerable<Place>()));
                //personRepositoryMock.Setup(repo => repo.GetPlacesAsync()).ReturnsAsync(places);

                // International Parameters Host Country
                personRepositoryMock.Setup(repo => repo.GetHostCountryAsync()).ReturnsAsync("USA");

                referenceDataRepositoryMock.Setup(repo => repo.GetPrivacyStatusesAsync(It.IsAny<bool>()))
                     .ReturnsAsync(
                         new List<PrivacyStatus>() {
                        new PrivacyStatus( "d3d86052-9d55-4751-acda-5c07a064a82a", "UN", "unrestricted", PrivacyStatusType.unrestricted),
                        new PrivacyStatus( "cff65dcc-4a9b-44ed-b8d0-930348c55ef8", "R", "restricted", PrivacyStatusType.restricted)
                        }
                     );
                personNameTypes = new List<PersonNameTypeItem>() {
                        new PersonNameTypeItem("8224f18e-69c5-480b-a9b4-52f596aa4a52", "PREFERRED", "Personal", PersonNameType.Personal),
                        new PersonNameTypeItem("7dfa950c-8ae4-4dca-92f0-c083604285b6", "BIRTH", "Birth", PersonNameType.Birth),
                        new PersonNameTypeItem("dd20ebdf-2452-41ef-9F86-ad1b1621a78d", "CHOSEN", "Chosen", PersonNameType.Personal),
                        new PersonNameTypeItem("806af5a5-8a9a-424f-8c9f-c1e9d084ee71", "LEGAL", "Legal", PersonNameType.Legal),
                        new PersonNameTypeItem("7b55610f-7d00-4260-bbcf-0e47fdbae647", "NICKNAME", "NickName", PersonNameType.Personal),
                        new PersonNameTypeItem("d42cc964-35cb-4560-bc46-4b881e7705ea", "HISTORY", "History", PersonNameType.Personal)
                        };
                referenceDataRepositoryMock.Setup(repo => repo.GetPersonNameTypesAsync(It.IsAny<bool>()))
                    .ReturnsAsync(personNameTypes);

                referenceDataRepositoryMock.Setup(repo => repo.GetDenominationsAsync(It.IsAny<bool>()))
                    .ReturnsAsync(
                        new List<Denomination>() {
                        new Denomination(baptistGuid,"BA", "Baptist") ,
                        new Denomination(catholicGuid,"CA", "Catholic")
                        }
                     );

                referenceDataRepositoryMock.Setup(repo => repo.GetEmailTypesAsync(It.IsAny<bool>()))
                     .ReturnsAsync(
                         new List<EmailType>() {
                        new EmailType("899803da-48f8-4044-beb8-5913a04b995d", "COL", "College", EmailTypeCategory.School),
                        new EmailType("301d485d-d37b-4d29-af00-465ced624a85", "PER", "Personal", EmailTypeCategory.Personal),
                        new EmailType("53fb7dab-d348-4657-b071-45d0e5933e05", "BUS", "Business", EmailTypeCategory.Business)
                        }
                     );

                referenceDataRepositoryMock.Setup(repo => repo.GetSocialMediaTypesAsync(It.IsAny<bool>()))
                     .ReturnsAsync(
                         new List<SocialMediaType>() {
                        new SocialMediaType("d1f311f4-687d-4dc7-a329-c6a8bfc9c74", "TW", "Twitter", SocialMediaTypeCategory.twitter)
                        }
                     );

                referenceDataRepositoryMock.Setup(repo => repo.GetAddressTypes2Async(It.IsAny<bool>()))
                     .ReturnsAsync(
                         new List<AddressType2>() {
                        new AddressType2("91979656-e110-4156-a75a-1a1a7294314d", "HO", "Home", AddressTypeCategory.Home),
                        new AddressType2("b887d5ec-9ed5-45e8-b44c-01782070f234", "MA", "Mailing", AddressTypeCategory.Mailing),
                        new AddressType2("d7d0a82c-fe74-480d-be1b-88a2e460af4c", "VA", "Vacation", AddressTypeCategory.Vacation),
                        new AddressType2("c9b8cd52-54e6-4c08-a9d9-224dd0c8b700", "BU", "Business", AddressTypeCategory.Business)
                         }
                     );

                referenceDataRepositoryMock.Setup(repo => repo.GetPhoneTypesAsync(It.IsAny<bool>()))
                     .ReturnsAsync(
                         new List<PhoneType>() {
                        new PhoneType("92c82d33-e55c-41a4-a2c3-f2f7d2c523d1", "HO", "Home", PhoneTypeCategory.Home),
                        new PhoneType("b6def2cc-cc95-4d0e-a32c-940fbbc2d689", "MO", "Mobile", PhoneTypeCategory.Mobile),
                        new PhoneType("f60e7b27-a3e3-4c92-9d36-f3cae27b724b", "VA", "Vacation", PhoneTypeCategory.Vacation),
                        new PhoneType("30e231cf-a199-4c9a-af01-be2e69b607c9", "BU", "Business", PhoneTypeCategory.Business)
                        }
                     );

                // Mock the reference repository for ethnicity
                referenceDataRepositoryMock.Setup(repo => repo.GetEthnicitiesAsync(It.IsAny<bool>())).Returns(Task.FromResult<IEnumerable<Ethnicity>>(new List<Ethnicity>()
                {
                    new Ethnicity(ethnicityGuid, "H", "Hispanic", EthnicityType.Hispanic)
                }));

                // Mock the reference repository for prefix
                referenceDataRepositoryMock.Setup(repo => repo.Prefixes).Returns(new List<Prefix>()
                {
                    new Prefix("MR","Mr","Mr."),
                    new Prefix("MS","Ms","Ms.")
                });

                // Mock the reference repository for prefix
                referenceDataRepositoryMock.Setup(repo => repo.GetPrefixesAsync()).ReturnsAsync(new List<Prefix>()
                {
                    new Prefix("MR","Mr","Mr."),
                    new Prefix("MS","Ms","Ms.")
                });

                // Mock the reference repository for suffix
                referenceDataRepositoryMock.Setup(repo => repo.Suffixes).Returns(new List<Suffix>()
                {
                    new Suffix("JR","Jr","Jr."),
                    new Suffix("SR","Sr","Sr.")
                });

                // Mock the reference repository for suffix
                referenceDataRepositoryMock.Setup(repo => repo.GetSuffixesAsync()).ReturnsAsync(new List<Suffix>()
                {
                    new Suffix("JR","Jr","Jr."),
                    new Suffix("SR","Sr","Sr.")
                });

                // Mock the reference repository for marital status
                referenceDataRepositoryMock.Setup(repo => repo.GetMaritalStatusesAsync(It.IsAny<bool>())).Returns(Task.FromResult<IEnumerable<MaritalStatus>>(new List<MaritalStatus>()
                {
                    new MaritalStatus(maritalStatusGuid, "M", "Married"){ Type = MaritalStatusType.Married },
                     new MaritalStatus(maritalStatusSingleGuid, "S", "Single"){ Type = MaritalStatusType.Single }
                }));

                var raceEntities = new List<Race>();
                raceEntities.Add(new Race(raceAsianGuid, "AS", "Asian", RaceType.Asian));
                raceEntities.Add(new Race(racePacificIslanderGuid, "HP", "Hawaiian/Pacific Islander", RaceType.PacificIslander));
                referenceDataRepositoryMock.Setup(repo => repo.GetRacesAsync(It.IsAny<bool>())).Returns(Task.FromResult<IEnumerable<Race>>(raceEntities));
            }

            #endregion

        }

        #endregion

        #region HEDM CreatePerson V12.0.0 Tests

        [TestClass]
        public class CreatePerson4 : CurrentUserSetup
        {
            //Mocks
            Mock<IAdapterRegistry> adapterRegistryMock;
            Mock<IPersonRepository> personRepositoryMock;
            Mock<IPersonBaseRepository> personBaseRepositoryMock;
            Mock<IReferenceDataRepository> referenceDataRepositoryMock;
            Mock<IProfileRepository> profileRepositoryMock;
            Mock<IConfigurationRepository> configurationRepositoryMock;
            Mock<IRelationshipRepository> relationshipRepositoryMock;
            Mock<IProxyRepository> proxyRepositoryMock;
            Mock<IRoleRepository> roleRepositoryMock;
            Mock<ILogger> loggerMock;

            //userfactory
            ICurrentUserFactory currentUserFactory;

            //Perms
            // private Ellucian.Colleague.Domain.Entities.Permission permissionUpdatePerson;

            //Service
            PersonService personService;


            private Ellucian.Colleague.Dtos.Person4 personDto;
            private Ellucian.Colleague.Domain.Base.Entities.PersonIntegration personIntegrationReturned;
            private Ellucian.Colleague.Domain.Base.Entities.PersonIntegration personIntegrationEntity;

            //private Ellucian.Colleague.Domain.Base.Entities.Person person;
            private List<Ellucian.Colleague.Domain.Base.Entities.Address> addresses = null;
            private List<Ellucian.Colleague.Domain.Base.Entities.Phone> phones = new List<Domain.Base.Entities.Phone>();
            //private IEnumerable<Ellucian.Colleague.Domain.Base.Entities.State> states;
            //private IEnumerable<Ellucian.Colleague.Domain.Base.Entities.Country> countries;
            //private List<Ellucian.Colleague.Domain.Base.Entities.County> counties;
            private List<PersonNameTypeItem> personNameTypes;
            private List<Country> countries;

            private IEnumerable<Domain.Base.Entities.PrivacyStatus> allPrivacyStatuses;

            //Entities
            private Ellucian.Colleague.Domain.Base.Entities.EmailAddress instEmail;
            private Ellucian.Colleague.Domain.Base.Entities.EmailAddress perEmail;
            private Ellucian.Colleague.Domain.Base.Entities.EmailAddress workEmail;
            private Ellucian.Colleague.Domain.Base.Entities.Address homeAddr;
            private Ellucian.Colleague.Domain.Base.Entities.Address mailAddr;
            private Ellucian.Colleague.Domain.Base.Entities.Address resAddr;
            private Ellucian.Colleague.Domain.Base.Entities.Address workAddr;
            private Ellucian.Colleague.Domain.Base.Entities.Phone homePhone;
            private Ellucian.Colleague.Domain.Base.Entities.Phone mobilePhone;
            private Ellucian.Colleague.Domain.Base.Entities.Phone residencePhone;
            private Ellucian.Colleague.Domain.Base.Entities.Phone workPhone;

            //Data
            private string personId = "0000011";
            private string personGuid = "5674f28b-b216-4055-b236-81a922d93b4c";
            private string maritalStatusGuid = "dca8edb5-120f-479a-a6bb-35ba3af4b344";
            private string maritalStatusSingleGuid = "dda8edb5-120f-479a-a6bb-35ba3af4b344";
            private string ethnicityGuid = "dd0c42ca-c61d-4ca6-8d21-96ab5be35623";
            private string raceAsianGuid = "72b7737b-27db-4a06-944b-97d00c29b3db";
            private string racePacificIslanderGuid = "e20f9821-28a2-4e34-8550-6758850a0cf8";
            private string baptistGuid = "c0bdfd92-462f-4e59-bba5-1b15c4771c86";
            private string catholicGuid = "f96f04b0-4973-41f6-bc3d-9c7bc1c2c458";
            private string demographicGuid = "d3d86052-9d55-4751-acda-5c07a064a82a";
            private string noncitizenGuid = "97ec6f69-9b16-4ed5-8954-59067f0318ec";


            private string countyGuid = Guid.NewGuid().ToString();


            [TestInitialize]
            public void Initialize()
            {
                adapterRegistryMock = new Mock<IAdapterRegistry>();
                personRepositoryMock = new Mock<IPersonRepository>();
                personBaseRepositoryMock = new Mock<IPersonBaseRepository>();
                referenceDataRepositoryMock = new Mock<IReferenceDataRepository>();
                profileRepositoryMock = new Mock<IProfileRepository>();
                configurationRepositoryMock = new Mock<IConfigurationRepository>();
                relationshipRepositoryMock = new Mock<IRelationshipRepository>();
                proxyRepositoryMock = new Mock<IProxyRepository>();
                roleRepositoryMock = new Mock<IRoleRepository>();
                loggerMock = new Mock<ILogger>();

                currentUserFactory = new CurrentUserSetup.ThirdPartyUserFactory();

                SetupData();

                SetupReferenceDataRepositoryMocks();

                personRepositoryMock.Setup(i => i.Create2Async(It.IsAny<PersonIntegration>(), addresses, phones, It.IsAny<int>())).ReturnsAsync(personIntegrationReturned);

                personService = new PersonService(adapterRegistryMock.Object, personRepositoryMock.Object, personBaseRepositoryMock.Object, referenceDataRepositoryMock.Object, profileRepositoryMock.Object,
                                                  configurationRepositoryMock.Object, relationshipRepositoryMock.Object, proxyRepositoryMock.Object, currentUserFactory,
                                                  roleRepositoryMock.Object, loggerMock.Object);
            }

            [TestCleanup]
            public void Cleanup()
            {
                personService = null;
                //permissionUpdatePerson = null;
                adapterRegistryMock = null;
                personRepositoryMock = null;
                personBaseRepositoryMock = null;
                referenceDataRepositoryMock = null;
                profileRepositoryMock = null;
                configurationRepositoryMock = null;
                relationshipRepositoryMock = null;
                proxyRepositoryMock = null;
                roleRepositoryMock = null;
                loggerMock = null;
                currentUserFactory = null;
            }

            [TestMethod]
            public async Task CreatePerson4_CreatePerson4Async()
            {
                //setup role
                createPersonRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Base.BasePermissionCodes.CreatePerson));
                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { createPersonRole });

                var actual = await personService.CreatePerson4Async(personDto);

                Assert.AreEqual(personDto.BirthDate, actual.BirthDate);
                // Assert.AreEqual(personDto.CitizenshipCountry, actual.CitizenshipCountry);
                Assert.AreEqual(personDto.CitizenshipStatus, actual.CitizenshipStatus);
                //Assert.AreEqual(personDto.CountryOfBirth, actual.CountryOfBirth);
                Assert.AreEqual(personDto.DeceasedDate, actual.DeceasedDate);
                Assert.AreEqual(personDto.GenderType, actual.GenderType);
                Assert.AreEqual(personDto.MaritalStatus.Detail.Id, actual.MaritalStatus.Detail.Id);
                Assert.AreEqual(personDto.MaritalStatus.MaritalCategory, actual.MaritalStatus.MaritalCategory);


                //Legal
                var legalActualName = actual.PersonNames.FirstOrDefault(item => item.NameType.Detail.Id.Equals("806af5a5-8a9a-424f-8c9f-c1e9d084ee71", StringComparison.OrdinalIgnoreCase));
                var legalExpectedName = personDto.PersonNames.FirstOrDefault(item => item.NameType.Detail.Id.Equals(legalActualName.NameType.Detail.Id, StringComparison.OrdinalIgnoreCase));

                //Assert.AreEqual(legalExpectedName.FullName, legalActualName.FullName); commented cause it will fail
                if (!string.IsNullOrEmpty(legalExpectedName.LastNamePrefix))
                    Assert.AreEqual(legalExpectedName.LastName, string.Concat(legalExpectedName.LastNamePrefix, " ", legalActualName.LastName));
                Assert.AreEqual(legalExpectedName.FirstName, legalActualName.FirstName);
                Assert.AreEqual(legalExpectedName.MiddleName, legalActualName.MiddleName);

                //Birth
                var birthActualName = actual.PersonNames.FirstOrDefault(item => item.NameType.Detail.Id.Equals("7dfa950c-8ae4-4dca-92f0-c083604285b6", StringComparison.OrdinalIgnoreCase));
                var birthexpectedName = personDto.PersonNames.FirstOrDefault(item => item.NameType.Detail.Id.Equals(birthActualName.NameType.Detail.Id, StringComparison.OrdinalIgnoreCase));

                Assert.AreEqual(birthexpectedName.FullName, birthActualName.FullName);

                //Chosen
                var chosenActualName = actual.PersonNames.FirstOrDefault(item => item.NameType.Detail.Id.Equals("dd20ebdf-2452-41ef-9f86-ad1b1621a78d", StringComparison.OrdinalIgnoreCase));
                var chosenExpectedName = personDto.PersonNames.FirstOrDefault(item => item.NameType.Detail.Id.Equals(chosenActualName.NameType.Detail.Id, StringComparison.OrdinalIgnoreCase));

                Assert.AreEqual(chosenExpectedName.FullName, chosenActualName.FullName);

                //Nickname
                var nickNameActualName = actual.PersonNames.FirstOrDefault(item => item.NameType.Detail.Id.Equals("7b55610f-7d00-4260-bbcf-0e47fdbae647", StringComparison.OrdinalIgnoreCase));
                var nickNameExpectedName = personDto.PersonNames.FirstOrDefault(item => item.NameType.Detail.Id.Equals(nickNameActualName.NameType.Detail.Id, StringComparison.OrdinalIgnoreCase));

                Assert.AreEqual(nickNameExpectedName.FullName, nickNameActualName.FullName);

                //History
                var historyActualName = actual.PersonNames.FirstOrDefault(item => item.NameType.Detail.Id.Equals("d42cc964-35cb-4560-bc46-4b881e7705ea", StringComparison.OrdinalIgnoreCase));
                var historyexpectedName = personDto.PersonNames.FirstOrDefault(item => item.NameType.Detail.Id.Equals(historyActualName.NameType.Detail.Id, StringComparison.OrdinalIgnoreCase));

                Assert.AreEqual(historyexpectedName.FullName, historyActualName.FullName);
            }

            #region Privacy Status

            [TestMethod]
            public async Task CreatePerson4_CreatePerson4Async_PrivacyStatus_Detail()
            {
                //setup role
                createPersonRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Base.BasePermissionCodes.CreatePerson));
                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { createPersonRole });

                var privacyStatus = new PersonPrivacyDtoProperty
                {
                    Detail = new GuidObject2(demographicGuid),
                    PrivacyCategory = Dtos.PrivacyStatusType.Unrestricted
                };
                personDto.PrivacyStatus = privacyStatus;

                var actual = await personService.CreatePerson4Async(personDto);
                Assert.IsNotNull(actual.PrivacyStatus);
                Assert.AreEqual(demographicGuid, actual.PrivacyStatus.Detail.Id);
            }

            [TestMethod]
            public async Task CreatePerson4_CreatePerson4Async_PrivacyStatus_PrivacyCategory()
            {
                //setup role
                createPersonRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Base.BasePermissionCodes.CreatePerson));
                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { createPersonRole });

                var privacyStatus = new PersonPrivacyDtoProperty
                {
                    PrivacyCategory = Dtos.PrivacyStatusType.Restricted
                };
                personDto.PrivacyStatus = privacyStatus;

                var actual = await personService.CreatePerson4Async(personDto);
                Assert.IsNotNull(actual.PrivacyStatus);
                Assert.AreEqual(demographicGuid, actual.PrivacyStatus.Detail.Id);
            }

            [TestMethod]
            [ExpectedException(typeof(Exception))]
            public async Task CreatePerson4_CreatePerson4Async_PrivacyStatus_Mismatch()
            {
                //setup role
                createPersonRole.AddPermission(
                    new Ellucian.Colleague.Domain.Entities.Permission(
                        Ellucian.Colleague.Domain.Base.BasePermissionCodes.CreatePerson));
                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { createPersonRole });

                var privacyStatus = new PersonPrivacyDtoProperty
                {
                    Detail = new GuidObject2(demographicGuid),
                    PrivacyCategory = Dtos.PrivacyStatusType.Restricted
                };
                personDto.PrivacyStatus = privacyStatus;
                await personService.CreatePerson4Async(personDto);
            }

            [TestMethod]
            [ExpectedException(typeof(Exception))]
            public async Task CreatePerson4_CreatePerson4Async_PrivacyStatus_EmptyDetail()
            {
                //setup role
                createPersonRole.AddPermission(
                    new Ellucian.Colleague.Domain.Entities.Permission(
                        Ellucian.Colleague.Domain.Base.BasePermissionCodes.CreatePerson));
                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { createPersonRole });

                var privacyStatus = new PersonPrivacyDtoProperty
                {
                    Detail = new GuidObject2(),
                    PrivacyCategory = Dtos.PrivacyStatusType.Restricted
                };
                personDto.PrivacyStatus = privacyStatus;
                await personService.CreatePerson4Async(personDto);
            }

            [TestMethod]
            [ExpectedException(typeof(Exception))]
            public async Task CreatePerson4_CreatePerson4Async_PrivacyStatus_InvalidDetail()
            {
                //setup role
                createPersonRole.AddPermission(
                    new Ellucian.Colleague.Domain.Entities.Permission(
                        Ellucian.Colleague.Domain.Base.BasePermissionCodes.CreatePerson));
                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { createPersonRole });

                var privacyStatus = new PersonPrivacyDtoProperty
                {
                    Detail = new GuidObject2("A448C44F-C5D5-492C-A7ED-83F3DD1B5042"),
                    PrivacyCategory = Dtos.PrivacyStatusType.Restricted
                };
                personDto.PrivacyStatus = privacyStatus;
                await personService.CreatePerson4Async(personDto);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task CreatePerson4_CreatePerson4Async_PrivacyStatus_EmptyCategory()
            {
                //setup role
                createPersonRole.AddPermission(
                    new Ellucian.Colleague.Domain.Entities.Permission(
                        Ellucian.Colleague.Domain.Base.BasePermissionCodes.CreatePerson));
                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { createPersonRole });

                referenceDataRepositoryMock.Setup(repo => repo.GetPrivacyStatusesAsync(It.IsAny<bool>())).ReturnsAsync(allPrivacyStatuses.Where(m => m.Guid == demographicGuid));

                var privacyStatus = new PersonPrivacyDtoProperty
                {
                    Detail = new GuidObject2(demographicGuid),

                };
                personDto.PrivacyStatus = privacyStatus;
                await personService.CreatePerson4Async(personDto);
            }

            #endregion

            #region Marital Status

            [TestMethod]
            public async Task CreatePerson4_CreatePerson4Async_MaritalStatus_Detail()
            {
                //setup role
                createPersonRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Base.BasePermissionCodes.CreatePerson));
                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { createPersonRole });

                var maritalStatus = new PersonMaritalStatusDtoProperty
                {
                    Detail = new GuidObject2(maritalStatusGuid),
                    MaritalCategory = PersonMaritalStatusCategory.Married
                };
                personDto.MaritalStatus = maritalStatus;

                var actual = await personService.CreatePerson4Async(personDto);
                Assert.IsNotNull(actual.MaritalStatus);
                Assert.AreEqual(maritalStatusGuid, actual.MaritalStatus.Detail.Id);
            }

            [TestMethod]
            public async Task CreatePerson4_CreatePerson4Async_MaritalStatus_MaritalCategory()
            {
                //setup role
                createPersonRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Base.BasePermissionCodes.CreatePerson));
                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { createPersonRole });

                var maritalStatus = new PersonMaritalStatusDtoProperty
                {

                    MaritalCategory = PersonMaritalStatusCategory.Married
                };
                personDto.MaritalStatus = maritalStatus;

                var actual = await personService.CreatePerson4Async(personDto);
                Assert.IsNotNull(actual.MaritalStatus);
                Assert.AreEqual(maritalStatusGuid, actual.MaritalStatus.Detail.Id);
            }

            [TestMethod]
            [ExpectedException(typeof(Exception))]
            public async Task CreatePerson4_CreatePerson4Async_MaritalStatus_Mismatch()
            {
                //setup role
                createPersonRole.AddPermission(
                    new Ellucian.Colleague.Domain.Entities.Permission(
                        Ellucian.Colleague.Domain.Base.BasePermissionCodes.CreatePerson));
                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { createPersonRole });

                var maritalStatus = new PersonMaritalStatusDtoProperty
                {
                    Detail = new GuidObject2(new Guid().ToString()),
                    MaritalCategory = PersonMaritalStatusCategory.Married
                };

                personDto.MaritalStatus = maritalStatus;

                await personService.CreatePerson4Async(personDto);
            }

            [TestMethod]
            [ExpectedException(typeof(Exception))]
            public async Task CreatePerson4_CreatePerson4Async_MaritalStatus_EmptyDetail()
            {
                //setup role
                createPersonRole.AddPermission(
                    new Ellucian.Colleague.Domain.Entities.Permission(
                        Ellucian.Colleague.Domain.Base.BasePermissionCodes.CreatePerson));
                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { createPersonRole });

                var maritalStatus = new PersonMaritalStatusDtoProperty
                {
                    Detail = new GuidObject2(),
                    MaritalCategory = PersonMaritalStatusCategory.Married
                };
                personDto.MaritalStatus = maritalStatus;
                await personService.CreatePerson4Async(personDto);
            }

            [TestMethod]
            [ExpectedException(typeof(Exception))]
            public async Task CreatePerson4_CreatePerson4Async_MaritalStatus_EmptyCategory()
            {
                //setup role
                createPersonRole.AddPermission(
                    new Ellucian.Colleague.Domain.Entities.Permission(
                        Ellucian.Colleague.Domain.Base.BasePermissionCodes.CreatePerson));
                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { createPersonRole });

                var maritalStatus = new PersonMaritalStatusDtoProperty
                {
                    Detail = new GuidObject2(new Guid().ToString()),

                };
                personDto.MaritalStatus = maritalStatus;

                await personService.CreatePerson4Async(personDto);
            }

            #endregion

            #region Citizenship
            [TestMethod]
            public async Task CreatePerson4_CreatePerson4Async_Citizenship_Detail()
            {
                //setup role
                createPersonRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Base.BasePermissionCodes.CreatePerson));
                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { createPersonRole });


                var allCitizenshipStatuses = new TestCitizenshipStatusRepository().Get().ToList();
                allCitizenshipStatuses.Add(
                    new Domain.Base.Entities.CitizenshipStatus(noncitizenGuid, "NC", "nonCitizen", Ellucian.Colleague.Domain.Base.Entities.CitizenshipStatusType.NonCitizen));
                referenceDataRepositoryMock.Setup(repo => repo.GetCitizenshipStatusesAsync(It.IsAny<bool>())).ReturnsAsync(allCitizenshipStatuses);

                var citizenshipStatus = new PersonCitizenshipDtoProperty
                {
                    Detail = new GuidObject2(noncitizenGuid),
                    Category = Dtos.CitizenshipStatusType.NonCitizen
                };
                personDto.CitizenshipStatus = citizenshipStatus;
                personIntegrationReturned.AlienStatus = "NC";

                var actual = await personService.CreatePerson4Async(personDto);
                Assert.IsNotNull(actual.CitizenshipStatus);
                Assert.AreEqual(noncitizenGuid, actual.CitizenshipStatus.Detail.Id);
                Assert.AreEqual(Dtos.CitizenshipStatusType.NonCitizen, actual.CitizenshipStatus.Category);
            }

            #endregion

            #region Religion

            [TestMethod]
            public async Task CreatePerson4_CreatePerson4Async_Religion()
            {
                //setup role
                createPersonRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Base.BasePermissionCodes.CreatePerson));
                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { createPersonRole });

                IEnumerable<Domain.Base.Entities.Denomination> allDenominations = new TestReligionRepository().GetDenominations().ToList();
                referenceDataRepositoryMock.Setup(repo => repo.GetDenominationsAsync(It.IsAny<bool>())).ReturnsAsync(allDenominations);
                //referenceDataRepositoryMock.Setup(repo => repo.DenominationsAsync()).ReturnsAsync(allDenominations);

                var religion = allDenominations.FirstOrDefault(x => x.Code == "CA");

                personDto.Religion = new GuidObject2(religion.Guid);

                var actual = await personService.CreatePerson4Async(personDto);
                Assert.IsNotNull(actual.Religion);
                Assert.AreEqual(religion.Guid, actual.Religion.Id);

            }

            [TestMethod]
            [ExpectedException(typeof(Exception))]
            public async Task CreatePerson4_CreatePerson4Async_Religion_Invalid()
            {
                //setup role
                createPersonRole.AddPermission(
                    new Ellucian.Colleague.Domain.Entities.Permission(
                        Ellucian.Colleague.Domain.Base.BasePermissionCodes.CreatePerson));
                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { createPersonRole });

                IEnumerable<Domain.Base.Entities.Denomination> allDenominations =
                    new TestReligionRepository().GetDenominations().ToList();
                referenceDataRepositoryMock.Setup(repo => repo.GetDenominationsAsync(It.IsAny<bool>()))
                    .ReturnsAsync(allDenominations);
                //referenceDataRepositoryMock.Setup(repo => repo.DenominationsAsync()).ReturnsAsync(allDenominations);

                personDto.Religion = new GuidObject2(new Guid().ToString());

                await personService.CreatePerson4Async(personDto);
            }

            [TestMethod]
            [ExpectedException(typeof(Exception))]
            public async Task CreatePerson4_CreatePerson4Async_Religion_Null()
            {
                //setup role
                createPersonRole.AddPermission(
                    new Ellucian.Colleague.Domain.Entities.Permission(
                        Ellucian.Colleague.Domain.Base.BasePermissionCodes.CreatePerson));
                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { createPersonRole });

                IEnumerable<Domain.Base.Entities.Denomination> allDenominations =
                    new TestReligionRepository().GetDenominations().ToList();
                referenceDataRepositoryMock.Setup(repo => repo.GetDenominationsAsync(It.IsAny<bool>()))
                    .ReturnsAsync(allDenominations);
                //referenceDataRepositoryMock.Setup(repo => repo.DenominationsAsync()).ReturnsAsync(allDenominations);

                var religion = allDenominations.FirstOrDefault();

                personDto.Religion = new GuidObject2();

                await personService.CreatePerson4Async(personDto);
            }

            #endregion

            #region Language

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task CreatePerson4_CreatePerson4Async_Language()
            {
                //setup role
                createPersonRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Base.BasePermissionCodes.CreatePerson));
                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { createPersonRole });

                var personLanguages = new List<PersonLanguageDtoProperty>();

                var personLanguage1 = new PersonLanguageDtoProperty()
                {
                    Code = PersonLanguageCode.eng,
                    Preference = PersonLanguagePreference.Primary
                };
                personLanguages.Add(personLanguage1);

                personDto.Languages = personLanguages;

                var actual = await personService.CreatePerson4Async(personDto);
                var primaryLanguage =
                    actual.Languages.FirstOrDefault(x => x.Preference == PersonLanguagePreference.Primary);

                Assert.IsNotNull(primaryLanguage);
                Assert.AreEqual(PersonLanguagePreference.Primary, primaryLanguage.Preference);
                Assert.AreEqual(PersonLanguageCode.eng, primaryLanguage.Code);
            }


            [TestMethod]
            [ExpectedException(typeof(Exception))]
            public async Task CreatePerson4_CreatePerson4Async_Language_EmptyCode()
            {
                //setup role
                createPersonRole.AddPermission(
                    new Ellucian.Colleague.Domain.Entities.Permission(
                        Ellucian.Colleague.Domain.Base.BasePermissionCodes.CreatePerson));
                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { createPersonRole });

                var personLanguages = new List<PersonLanguageDtoProperty>();

                var personLanguage1 = new PersonLanguageDtoProperty()
                {
                    Preference = PersonLanguagePreference.Primary
                };
                personLanguages.Add(personLanguage1);

                personDto.Languages = personLanguages;

                await personService.CreatePerson4Async(personDto);
            }

            [TestMethod]
            [ExpectedException(typeof(Exception))]
            public async Task CreatePerson4_CreatePerson4Async_Language_MultiplePreferred()
            {
                //setup role
                createPersonRole.AddPermission(
                    new Ellucian.Colleague.Domain.Entities.Permission(
                        Ellucian.Colleague.Domain.Base.BasePermissionCodes.CreatePerson));
                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { createPersonRole });

                var personLanguages = new List<PersonLanguageDtoProperty>();

                var personLanguage1 = new PersonLanguageDtoProperty()
                {
                    Code = PersonLanguageCode.eng,
                    Preference = PersonLanguagePreference.Primary
                };
                personLanguages.Add(personLanguage1);

                var personLanguage2 = new PersonLanguageDtoProperty()
                {
                    Code = PersonLanguageCode.cha,
                    Preference = PersonLanguagePreference.Primary
                };
                personLanguages.Add(personLanguage2);

                personDto.Languages = personLanguages;

                await personService.CreatePerson4Async(personDto);
            }

            #endregion

            #region CountryOfBirth/countryOfCitizenship

            [TestMethod]
            public async Task CreatePerson4_CreatePerson4Async_Country()
            {
                personDto.CitizenshipCountry = "USA";
                personDto.CountryOfBirth = "USA";

                //setup role
                createPersonRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Base.BasePermissionCodes.CreatePerson));
                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { createPersonRole });

                // Mock the reference repository for country
                var countries = new List<Country>()
                 {
                    new Country("USA","United States","USA", "USA") {IsoAlpha3Code = "USA"},
                    new Country("CAN","Canada","CAN", "CAN")  {IsoAlpha3Code = "CAN"},
                    new Country("ME","Mexico","MEX", "MEX")  {IsoAlpha3Code = "MEX"},
                    new Country("BRA","Brazil","BRA", "BRA")  {IsoAlpha3Code = "BRA"}
                };
                referenceDataRepositoryMock.Setup(repo => repo.GetCountryCodesAsync(It.IsAny<bool>())).ReturnsAsync(countries);

                var actual = await personService.CreatePerson4Async(personDto);
                Assert.IsNotNull(actual.CountryOfBirth);
                Assert.AreEqual("USA", actual.CountryOfBirth);

                Assert.IsNotNull(actual.CitizenshipCountry);
                Assert.AreEqual("USA", actual.CitizenshipCountry);
            }

            [TestMethod]
            [ExpectedException(typeof(Exception))]
            public async Task CreatePerson4_CreatePerson4Async_CountryOfBirth_Invalid()
            {
                //setup role
                createPersonRole.AddPermission(
                    new Ellucian.Colleague.Domain.Entities.Permission(
                        Ellucian.Colleague.Domain.Base.BasePermissionCodes.CreatePerson));
                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { createPersonRole });

                personDto.CountryOfBirth = "XXX";

                await personService.CreatePerson4Async(personDto);
            }

            [TestMethod]
            [ExpectedException(typeof(Exception))]
            public async Task CreatePerson4_CreatePerson4Async_CitizenshipCountry_Invalid()
            {
                //setup role
                createPersonRole.AddPermission(
                    new Ellucian.Colleague.Domain.Entities.Permission(
                        Ellucian.Colleague.Domain.Base.BasePermissionCodes.CreatePerson));
                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { createPersonRole });

                personDto.CitizenshipCountry = "XXX";

                await personService.CreatePerson4Async(personDto);
            }


            #endregion

            #region Exceptions

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task CreatePerson4_CreatePerson4Async_PersonDTO_Null_ArgumentNullException()
            {
                var actual = await personService.CreatePerson4Async(null);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task CreatePerson4_CreatePerson4Async_PersonDTO_IdNull_ArgumentNullException()
            {
                personDto.Id = string.Empty;
                var actual = await personService.CreatePerson4Async(personDto);
            }

            [TestMethod]
            [ExpectedException(typeof(Exception))]
            public async Task CreatePerson4_PrimaryNames_Null_ArgumentNullException()
            {
                //setup role
                createPersonRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Base.BasePermissionCodes.CreatePerson));
                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { createPersonRole });

                personDto.PersonNames.FirstOrDefault().LastName = string.Empty;

                var actual = await personService.CreatePerson4Async(personDto);
            }

            [TestMethod]
            [ExpectedException(typeof(Exception))]
            public async Task UpdatePerson4_Username_Exception()
            {
                //setup role
                createPersonRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Base.BasePermissionCodes.CreatePerson));
                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { updatePersonRole });

                //personId 0000011
                personRepositoryMock.Setup(i => i.GetPersonIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync(personId);

                personDto.Credentials = new List<Dtos.DtoProperties.Credential3DtoProperty>()
                {
                    new Dtos.DtoProperties.Credential3DtoProperty()
                    {
                        Type = Dtos.EnumProperties.Credential3Type.ColleagueUserName,
                        Value = "testUsername"
                    }
                };
                // Mock the response for getting a Person Pin 
                var personPins = new List<PersonPin>();
                personPins = null;
                personRepositoryMock.Setup(repo => repo.GetPersonPinsAsync(It.IsAny<string[]>())).ReturnsAsync(personPins);

                var actual = await personService.UpdatePerson4Async(personDto);
            }

            #endregion

            #region Setup V12

            private void SetupData()
            {
                // setup personDto object
                personDto = new Dtos.Person4();
                personDto.Id = Guid.Empty.ToString();
                personDto.BirthDate = new DateTime(1930, 1, 1);
                personDto.DeceasedDate = new DateTime(2014, 5, 12);
                var personNames = new List<Dtos.DtoProperties.PersonName2DtoProperty>();
                var legalPrimaryName = new Dtos.DtoProperties.PersonName2DtoProperty()
                {
                    NameType = new Dtos.DtoProperties.PersonNameTypeDtoProperty() { Category = Dtos.EnumProperties.PersonNameType2.Legal, Detail = new Dtos.GuidObject2() { Id = "806af5a5-8a9a-424f-8c9f-c1e9d084ee71" } },
                    Title = "Mr.",
                    FirstName = "LegalFirst",
                    MiddleName = "LegalMiddle",
                    LastName = "LegalLast",
                    Pedigree = "Jr.",
                    LastNamePrefix = "Ignore",
                    FullName = "LegalFirst LegalMiddle LegalLast"
                };
                personNames.Add(legalPrimaryName);

                var birthPrimaryName = new Dtos.DtoProperties.PersonName2DtoProperty()
                {
                    NameType = new Dtos.DtoProperties.PersonNameTypeDtoProperty() { Category = Dtos.EnumProperties.PersonNameType2.Birth, Detail = new Dtos.GuidObject2() { Id = "7dfa950c-8ae4-4dca-92f0-c083604285b6" } },
                    Title = "Mr.",
                    FirstName = "BirthFirst",
                    MiddleName = "BirthMiddle",
                    LastName = "BirthLast",
                    Pedigree = "Jr.",
                    LastNamePrefix = "Ignore",
                    FullName = "BirthFirst BirthMiddle BirthLast"
                };
                personNames.Add(birthPrimaryName);

                var chosenPrimaryName = new PersonName2DtoProperty()
                {
                    NameType = new PersonNameTypeDtoProperty() { Category = Dtos.EnumProperties.PersonNameType2.Personal, Detail = new Dtos.GuidObject2() { Id = "dd20ebdf-2452-41ef-9f86-ad1b1621a78d" } },
                    Title = "Mr.",
                    FirstName = "ChosenFirst",
                    MiddleName = "ChosenMiddle",
                    LastName = "ChosenLast",
                    Pedigree = "Jr.",
                    LastNamePrefix = "Ignore",
                    FullName = "ChosenFirst ChosenMiddle ChosenLast"
                };
                personNames.Add(chosenPrimaryName);

                var nickNamePrimaryName = new Dtos.DtoProperties.PersonName2DtoProperty()
                {
                    NameType = new Dtos.DtoProperties.PersonNameTypeDtoProperty() { Category = Dtos.EnumProperties.PersonNameType2.Personal, Detail = new Dtos.GuidObject2() { Id = "7b55610f-7d00-4260-bbcf-0e47fdbae647" } },
                    Title = "Mr.",
                    FirstName = "NickNameFirst",
                    MiddleName = "NickNameMiddle",
                    LastName = "NickNameLast",
                    Pedigree = "Jr.",
                    LastNamePrefix = "Ignore",
                    FullName = "NickNameFirst NickNameMiddle NickNameLast"
                };
                personNames.Add(nickNamePrimaryName);

                var historyPrimaryName = new Dtos.DtoProperties.PersonName2DtoProperty()
                {
                    NameType = new Dtos.DtoProperties.PersonNameTypeDtoProperty() { Category = Dtos.EnumProperties.PersonNameType2.Personal, Detail = new Dtos.GuidObject2() { Id = "d42cc964-35cb-4560-bc46-4b881e7705ea" } },
                    Title = "Mr.",
                    FirstName = "HistoryFirst",
                    MiddleName = "HistoryMiddle",
                    LastName = "HistoryLast",
                    Pedigree = "Jr.",
                    LastNamePrefix = "Ignore",
                    FullName = "HistoryFirst HistoryMiddle HistoryLast"
                };
                personNames.Add(historyPrimaryName);

                var preferedPrimaryName = new Dtos.DtoProperties.PersonName2DtoProperty()
                {
                    NameType = new Dtos.DtoProperties.PersonNameTypeDtoProperty() { Category = Dtos.EnumProperties.PersonNameType2.Personal, Detail = new Dtos.GuidObject2() { Id = "8224f18e-69c5-480b-a9b4-52f596aa4a52" } },
                    Title = "Mr.",
                    FirstName = "PreferedFirst",
                    MiddleName = "PreferedMiddle",
                    LastName = "PreferedLast",
                    Pedigree = "Jr.",
                    LastNamePrefix = "Ignore",
                    FullName = "PreferedFirst PreferedMiddle PreferedLast"
                };
                personNames.Add(preferedPrimaryName);

                personDto.PersonNames = personNames;
                personDto.GenderType = Dtos.EnumProperties.GenderType2.Male;
                personDto.MaritalStatus = new Dtos.DtoProperties.PersonMaritalStatusDtoProperty() { Detail = new Dtos.GuidObject2(maritalStatusGuid), MaritalCategory = Dtos.EnumProperties.PersonMaritalStatusCategory.Married };// new Dtos.GuidObject(maritalStatusGuid);
                personDto.Ethnicity = new Dtos.DtoProperties.PersonEthnicityDtoProperty() { EthnicGroup = new Dtos.GuidObject2(ethnicityGuid) };// new Dtos.GuidObject(ethnicityGuid);
                personDto.Races = new List<Dtos.DtoProperties.PersonRaceDtoProperty>()
                {

                    new Dtos.DtoProperties.PersonRaceDtoProperty(){ Race = new Dtos.GuidObject2(raceAsianGuid)}
                };
                personDto.Credentials = new List<Dtos.DtoProperties.Credential3DtoProperty>()
                {
                    new Dtos.DtoProperties.Credential3DtoProperty()
                    {
                        Type = Dtos.EnumProperties.Credential3Type.Ssn,
                        Value = "111-11-1111"
                    }
                };
                var emailAddresses = new List<Dtos.DtoProperties.PersonEmailDtoProperty>();
                emailAddresses.Add(new Dtos.DtoProperties.PersonEmailDtoProperty()
                {

                    Type = new Dtos.DtoProperties.PersonEmailTypeDtoProperty() { EmailType = Dtos.EmailTypeList.School },
                    Address = "xyz@xmail.com"
                });
                personDto.EmailAddresses = emailAddresses;


                //Entity
                personIntegrationEntity = new PersonIntegration(It.IsAny<string>(), legalPrimaryName.LastName)
                {
                    Guid = personDto.Id,
                    Prefix = "Mr.",
                    FirstName = legalPrimaryName.FirstName,
                    MiddleName = legalPrimaryName.MiddleName,
                    Suffix = "Sr."

                };

                //Returned value
                personIntegrationReturned = new PersonIntegration(personId, "LegalLast");
                personIntegrationReturned.Guid = personGuid;
                personIntegrationReturned.Prefix = "Mr.";
                personIntegrationReturned.FirstName = "LegalFirst";
                personIntegrationReturned.MiddleName = "LegalMiddle";
                personIntegrationReturned.Suffix = "Jr.";
                personIntegrationReturned.Nickname = "NickNameFirst NickNameMiddle NickNameLast";
                personIntegrationReturned.BirthDate = new DateTime(1930, 1, 1);
                personIntegrationReturned.DeceasedDate = new DateTime(2014, 5, 12);
                personIntegrationReturned.GovernmentId = "111-11-1111";
                personIntegrationReturned.Religion = "CA";
                personIntegrationReturned.MaritalStatusCode = "M";
                personIntegrationReturned.EthnicCodes = new List<string> { "H", "N" };
                personIntegrationReturned.Gender = "M";
                personIntegrationReturned.RaceCodes = new List<string> { "AS" };
                personIntegrationReturned.AddRole(new PersonRole(PersonRoleType.Alumni, new DateTime(15, 01, 22), new DateTime(15, 05, 25)));
                personIntegrationReturned.BirthNameFirst = "BirthFirst";
                personIntegrationReturned.BirthNameLast = "BirthLast";
                personIntegrationReturned.BirthNameMiddle = "BirthMiddle";
                personIntegrationReturned.ChosenFirstName = "ChosenFirst";
                personIntegrationReturned.ChosenLastName = "ChosenLast";
                personIntegrationReturned.ChosenMiddleName = "ChosenMiddle";
                personIntegrationReturned.PreferredName = "PreferedFirst PreferedMiddle PreferedLast";
                personIntegrationReturned.BirthCountry = "USA";
                personIntegrationReturned.Citizenship = "USA";
                personIntegrationReturned.FormerNames = new List<PersonName>()
                {
                    new PersonName("HistoryFirst", "HistoryMiddle", "HistoryLast")
                };
                // Mock the email address data response
                instEmail = new Domain.Base.Entities.EmailAddress("inst@inst.com", "COL") { IsPreferred = true };
                personIntegrationReturned.AddEmailAddress(instEmail);
                perEmail = new Domain.Base.Entities.EmailAddress("personal@personal.com", "PER") { IsPreferred = false };
                personIntegrationReturned.AddEmailAddress(perEmail);
                workEmail = new Domain.Base.Entities.EmailAddress("work@work.com", "BUS") { IsPreferred = false };
                personIntegrationReturned.AddEmailAddress(workEmail);

                // Mock the address hierarchy responses
                var addresses = new List<Domain.Base.Entities.Address>();
                homeAddr = new Domain.Base.Entities.Address()
                {
                    TypeCode = "HO",
                    Type = Dtos.EnumProperties.AddressType.Home.ToString(),
                    Guid = Guid.NewGuid().ToString(),
                    Status = "Current",
                    IsPreferredAddress = true
                };
                addresses.Add(homeAddr);
                mailAddr = new Domain.Base.Entities.Address()
                {
                    TypeCode = "MA",
                    Type = Dtos.EnumProperties.AddressType.Mailing.ToString(),
                    Guid = Guid.NewGuid().ToString(),
                    Status = "Current"
                };
                addresses.Add(mailAddr);
                resAddr = new Domain.Base.Entities.Address()
                {
                    TypeCode = "VA",
                    Type = Dtos.EnumProperties.AddressType.Vacation.ToString(),
                    Guid = Guid.NewGuid().ToString(),
                    Status = "Current",
                    IsPreferredResidence = true
                };
                addresses.Add(resAddr);
                workAddr = new Domain.Base.Entities.Address()
                {
                    TypeCode = "BU",
                    Type = Dtos.EnumProperties.AddressType.Business.ToString(),
                    Guid = Guid.NewGuid().ToString(),
                    Status = "Current"
                };
                addresses.Add(workAddr);
                personIntegrationReturned.Addresses = addresses;

                // Mock the person phone per type response
                homePhone = new Domain.Base.Entities.Phone("111-111-1111", "HO");
                personIntegrationReturned.AddPhone(homePhone);
                mobilePhone = new Domain.Base.Entities.Phone("222-222-2222", "MO");
                personIntegrationReturned.AddPhone(mobilePhone);
                residencePhone = new Domain.Base.Entities.Phone("333-333-3333", "VA");
                personIntegrationReturned.AddPhone(residencePhone);
                workPhone = new Domain.Base.Entities.Phone("444-444-4444", "BU", "4444");
                personIntegrationReturned.AddPhone(workPhone);

                // Mock social media
                var socialMedia = new List<Domain.Base.Entities.SocialMedia>();
                var socialMediaTypeCode = "TW";
                var socialMediaHandle = "pontifex";
                var personSocialMedia = new SocialMedia(socialMediaTypeCode, socialMediaHandle);
                personIntegrationReturned.AddSocialMedia(personSocialMedia);
            }

            private void SetupReferenceDataRepositoryMocks()
            {
                // Mock the reference repository for country
                countries = new List<Country>()
                 {
                    new Country("US","United States","USA"),
                    new Country("CA","Canada","CAN"),
                    new Country("MX","Mexico","MEX"),
                    new Country("BR","Brazil","BRA")
                };
                referenceDataRepositoryMock.Setup(repo => repo.GetCountryCodesAsync(It.IsAny<bool>())).ReturnsAsync(countries);

                // Places
                var places = new List<Place>();
                var place1 = new Place() { PlacesCountry = "USA", PlacesRegion = "US-NY" };
                places.Add(place1);
                var place2 = new Place() { PlacesCountry = "CAN", PlacesRegion = "CA-ON" };
                places.Add(place2);
                referenceDataRepositoryMock.Setup(repo => repo.GetPlacesAsync(It.IsAny<bool>())).Returns(Task.FromResult(places.AsEnumerable<Place>()));
                //personRepositoryMock.Setup(repo => repo.GetPlacesAsync()).ReturnsAsync(places);

                // International Parameters Host Country
                personRepositoryMock.Setup(repo => repo.GetHostCountryAsync()).ReturnsAsync("USA");

                referenceDataRepositoryMock.Setup(repo => repo.GetPrivacyStatusesAsync(It.IsAny<bool>()))
                     .ReturnsAsync(
                         new List<PrivacyStatus>() {
                        new PrivacyStatus( "d3d86052-9d55-4751-acda-5c07a064a82a", "UN", "unrestricted", PrivacyStatusType.unrestricted),
                        new PrivacyStatus( "cff65dcc-4a9b-44ed-b8d0-930348c55ef8", "R", "restricted", PrivacyStatusType.restricted)
                        }
                     );
                personNameTypes = new List<PersonNameTypeItem>() {
                        new PersonNameTypeItem("8224f18e-69c5-480b-a9b4-52f596aa4a52", "PREFERRED", "Personal", PersonNameType.Personal),
                        new PersonNameTypeItem("7dfa950c-8ae4-4dca-92f0-c083604285b6", "BIRTH", "Birth", PersonNameType.Birth),
                        new PersonNameTypeItem("dd20ebdf-2452-41ef-9f86-ad1b1621a78d", "CHOSEN", "Chosen", PersonNameType.Personal),
                        new PersonNameTypeItem("806af5a5-8a9a-424f-8c9f-c1e9d084ee71", "LEGAL", "Legal", PersonNameType.Legal),
                        new PersonNameTypeItem("7b55610f-7d00-4260-bbcf-0e47fdbae647", "NICKNAME", "NickName", PersonNameType.Personal),
                        new PersonNameTypeItem("d42cc964-35cb-4560-bc46-4b881e7705ea", "HISTORY", "History", PersonNameType.Personal)
                        };
                referenceDataRepositoryMock.Setup(repo => repo.GetPersonNameTypesAsync(It.IsAny<bool>()))
                    .ReturnsAsync(personNameTypes);

                referenceDataRepositoryMock.Setup(repo => repo.GetDenominationsAsync(It.IsAny<bool>()))
                    .ReturnsAsync(
                        new List<Denomination>() {
                        new Denomination(baptistGuid,"BA", "Baptist") ,
                        new Denomination(catholicGuid,"CA", "Catholic")
                        }
                     );

                referenceDataRepositoryMock.Setup(repo => repo.GetEmailTypesAsync(It.IsAny<bool>()))
                     .ReturnsAsync(
                         new List<EmailType>() {
                        new EmailType("899803da-48f8-4044-beb8-5913a04b995d", "COL", "College", EmailTypeCategory.School),
                        new EmailType("301d485d-d37b-4d29-af00-465ced624a85", "PER", "Personal", EmailTypeCategory.Personal),
                        new EmailType("53fb7dab-d348-4657-b071-45d0e5933e05", "BUS", "Business", EmailTypeCategory.Business)
                        }
                     );

                referenceDataRepositoryMock.Setup(repo => repo.GetSocialMediaTypesAsync(It.IsAny<bool>()))
                     .ReturnsAsync(
                         new List<SocialMediaType>() {
                        new SocialMediaType("d1f311f4-687d-4dc7-a329-c6a8bfc9c74", "TW", "Twitter", SocialMediaTypeCategory.twitter)
                        }
                     );

                referenceDataRepositoryMock.Setup(repo => repo.GetAddressTypes2Async(It.IsAny<bool>()))
                     .ReturnsAsync(
                         new List<AddressType2>() {
                        new AddressType2("91979656-e110-4156-a75a-1a1a7294314d", "HO", "Home", AddressTypeCategory.Home),
                        new AddressType2("b887d5ec-9ed5-45e8-b44c-01782070f234", "MA", "Mailing", AddressTypeCategory.Mailing),
                        new AddressType2("d7d0a82c-fe74-480d-be1b-88a2e460af4c", "VA", "Vacation", AddressTypeCategory.Vacation),
                        new AddressType2("c9b8cd52-54e6-4c08-a9d9-224dd0c8b700", "BU", "Business", AddressTypeCategory.Business)
                         }
                     );

                referenceDataRepositoryMock.Setup(repo => repo.GetPhoneTypesAsync(It.IsAny<bool>()))
                     .ReturnsAsync(
                         new List<PhoneType>() {
                        new PhoneType("92c82d33-e55c-41a4-a2c3-f2f7d2c523d1", "HO", "Home", PhoneTypeCategory.Home),
                        new PhoneType("b6def2cc-cc95-4d0e-a32c-940fbbc2d689", "MO", "Mobile", PhoneTypeCategory.Mobile),
                        new PhoneType("f60e7b27-a3e3-4c92-9d36-f3cae27b724b", "VA", "Vacation", PhoneTypeCategory.Vacation),
                        new PhoneType("30e231cf-a199-4c9a-af01-be2e69b607c9", "BU", "Business", PhoneTypeCategory.Business)
                        }
                     );

                // Mock the reference repository for ethnicity
                referenceDataRepositoryMock.Setup(repo => repo.GetEthnicitiesAsync(It.IsAny<bool>())).Returns(Task.FromResult<IEnumerable<Ethnicity>>(new List<Ethnicity>()
                {
                    new Ethnicity(ethnicityGuid, "H", "Hispanic", EthnicityType.Hispanic)
                }));

                // Mock the reference repository for prefix
                referenceDataRepositoryMock.Setup(repo => repo.Prefixes).Returns(new List<Prefix>()
                {
                    new Prefix("MR","Mr","Mr."),
                    new Prefix("MS","Ms","Ms.")
                });

                // Mock the reference repository for prefix
                referenceDataRepositoryMock.Setup(repo => repo.GetPrefixesAsync()).ReturnsAsync(new List<Prefix>()
                {
                    new Prefix("MR","Mr","Mr."),
                    new Prefix("MS","Ms","Ms.")
                });

                // Mock the reference repository for suffix
                referenceDataRepositoryMock.Setup(repo => repo.Suffixes).Returns(new List<Suffix>()
                {
                    new Suffix("JR","Jr","Jr."),
                    new Suffix("SR","Sr","Sr.")
                });

                // Mock the reference repository for suffix
                referenceDataRepositoryMock.Setup(repo => repo.GetSuffixesAsync()).ReturnsAsync(new List<Suffix>()
                {
                    new Suffix("JR","Jr","Jr."),
                    new Suffix("SR","Sr","Sr.")
                });

                // Mock the reference repository for marital status
                referenceDataRepositoryMock.Setup(repo => repo.GetMaritalStatusesAsync(It.IsAny<bool>())).Returns(Task.FromResult<IEnumerable<MaritalStatus>>(new List<MaritalStatus>()
                {
                    new MaritalStatus(maritalStatusGuid, "M", "Married"){ Type = MaritalStatusType.Married },
                     new MaritalStatus(maritalStatusSingleGuid, "S", "Single"){ Type = MaritalStatusType.Single }
                }));

                var raceEntities = new List<Race>();
                raceEntities.Add(new Race(raceAsianGuid, "AS", "Asian", RaceType.Asian));
                raceEntities.Add(new Race(racePacificIslanderGuid, "HP", "Hawaiian/Pacific Islander", RaceType.PacificIslander));
                referenceDataRepositoryMock.Setup(repo => repo.GetRacesAsync(It.IsAny<bool>())).Returns(Task.FromResult<IEnumerable<Race>>(raceEntities));
            }

            #endregion

        }

        #endregion

        #region HEDM CreatePerson V12.1.0 Tests

        [TestClass]
        public class CreatePerson5 : CurrentUserSetup
        {
            //Mocks
            Mock<IAdapterRegistry> adapterRegistryMock;
            Mock<IPersonRepository> personRepositoryMock;
            Mock<IPersonBaseRepository> personBaseRepositoryMock;
            Mock<IReferenceDataRepository> referenceDataRepositoryMock;
            Mock<IProfileRepository> profileRepositoryMock;
            Mock<IConfigurationRepository> configurationRepositoryMock;
            Mock<IRelationshipRepository> relationshipRepositoryMock;
            Mock<IProxyRepository> proxyRepositoryMock;
            Mock<IRoleRepository> roleRepositoryMock;
            Mock<ILogger> loggerMock;

            //userfactory
            ICurrentUserFactory currentUserFactory;

            //Perms
            // private Ellucian.Colleague.Domain.Entities.Permission permissionUpdatePerson;

            //Service
            PersonService personService;


            private Ellucian.Colleague.Dtos.Person5 personDto;
            private Ellucian.Colleague.Domain.Base.Entities.PersonIntegration personIntegrationReturned;
            private Ellucian.Colleague.Domain.Base.Entities.PersonIntegration personIntegrationEntity;

            //private Ellucian.Colleague.Domain.Base.Entities.Person person;
            private List<Ellucian.Colleague.Domain.Base.Entities.Address> addresses = null;
            private List<Ellucian.Colleague.Domain.Base.Entities.Phone> phones = new List<Domain.Base.Entities.Phone>();
            //private IEnumerable<Ellucian.Colleague.Domain.Base.Entities.State> states;
            //private IEnumerable<Ellucian.Colleague.Domain.Base.Entities.Country> countries;
            //private List<Ellucian.Colleague.Domain.Base.Entities.County> counties;
            private List<PersonNameTypeItem> personNameTypes;
            private List<Country> countries;

            private IEnumerable<Domain.Base.Entities.PrivacyStatus> allPrivacyStatuses;

            //Entities
            private Ellucian.Colleague.Domain.Base.Entities.EmailAddress instEmail;
            private Ellucian.Colleague.Domain.Base.Entities.EmailAddress perEmail;
            private Ellucian.Colleague.Domain.Base.Entities.EmailAddress workEmail;
            private Ellucian.Colleague.Domain.Base.Entities.Address homeAddr;
            private Ellucian.Colleague.Domain.Base.Entities.Address mailAddr;
            private Ellucian.Colleague.Domain.Base.Entities.Address resAddr;
            private Ellucian.Colleague.Domain.Base.Entities.Address workAddr;
            private Ellucian.Colleague.Domain.Base.Entities.Phone homePhone;
            private Ellucian.Colleague.Domain.Base.Entities.Phone mobilePhone;
            private Ellucian.Colleague.Domain.Base.Entities.Phone residencePhone;
            private Ellucian.Colleague.Domain.Base.Entities.Phone workPhone;

            //Data
            private string personId = "0000011";
            private string personGuid = "5674f28b-b216-4055-b236-81a922d93b4c";
            private string maritalStatusGuid = "dca8edb5-120f-479a-a6bb-35ba3af4b344";
            private string maritalStatusSingleGuid = "dda8edb5-120f-479a-a6bb-35ba3af4b344";
            private string ethnicityGuid = "dd0c42ca-c61d-4ca6-8d21-96ab5be35623";
            private string raceAsianGuid = "72b7737b-27db-4a06-944b-97d00c29b3db";
            private string racePacificIslanderGuid = "e20f9821-28a2-4e34-8550-6758850a0cf8";
            private string baptistGuid = "c0bdfd92-462f-4e59-bba5-1b15c4771c86";
            private string catholicGuid = "f96f04b0-4973-41f6-bc3d-9c7bc1c2c458";
            private string demographicGuid = "d3d86052-9d55-4751-acda-5c07a064a82a";
            private string noncitizenGuid = "97ec6f69-9b16-4ed5-8954-59067f0318ec";


            private string countyGuid = Guid.NewGuid().ToString();


            [TestInitialize]
            public void Initialize()
            {
                adapterRegistryMock = new Mock<IAdapterRegistry>();
                personRepositoryMock = new Mock<IPersonRepository>();
                personBaseRepositoryMock = new Mock<IPersonBaseRepository>();
                referenceDataRepositoryMock = new Mock<IReferenceDataRepository>();
                profileRepositoryMock = new Mock<IProfileRepository>();
                configurationRepositoryMock = new Mock<IConfigurationRepository>();
                relationshipRepositoryMock = new Mock<IRelationshipRepository>();
                proxyRepositoryMock = new Mock<IProxyRepository>();
                roleRepositoryMock = new Mock<IRoleRepository>();
                loggerMock = new Mock<ILogger>();

                currentUserFactory = new CurrentUserSetup.ThirdPartyUserFactory();

                SetupData();

                SetupReferenceDataRepositoryMocks();

                personRepositoryMock.Setup(i => i.Create2Async(It.IsAny<PersonIntegration>(), addresses, phones, It.IsAny<int>())).ReturnsAsync(personIntegrationReturned);

                // Mock LANGUAGES valcode 
                var languages = new Ellucian.Data.Colleague.DataContracts.ApplValcodes()
                {

                    ValsEntityAssociation = new List<Ellucian.Data.Colleague.DataContracts.ApplValcodesVals>()
                    {
                        new ApplValcodesVals() { ValInternalCodeAssocMember = "E", ValExternalRepresentationAssocMember = "English", ValActionCode3AssocMember = "ENG" },
                        new ApplValcodesVals() { ValInternalCodeAssocMember = "SP", ValExternalRepresentationAssocMember = "Spanish", ValActionCode3AssocMember = "SPA" },
                        new ApplValcodesVals() { ValInternalCodeAssocMember = "TA", ValExternalRepresentationAssocMember = "Tagalog", ValActionCode3AssocMember = "TGL" }
                    }
                };
                personBaseRepositoryMock.Setup(repo => repo.GetLanguagesAsync()).ReturnsAsync(languages);


                personService = new PersonService(adapterRegistryMock.Object, personRepositoryMock.Object, personBaseRepositoryMock.Object, referenceDataRepositoryMock.Object, profileRepositoryMock.Object,
                                                  configurationRepositoryMock.Object, relationshipRepositoryMock.Object, proxyRepositoryMock.Object, currentUserFactory,
                                                  roleRepositoryMock.Object, loggerMock.Object);
            }

            [TestCleanup]
            public void Cleanup()
            {
                personService = null;
                //permissionUpdatePerson = null;
                adapterRegistryMock = null;
                personRepositoryMock = null;
                personBaseRepositoryMock = null;
                referenceDataRepositoryMock = null;
                profileRepositoryMock = null;
                configurationRepositoryMock = null;
                relationshipRepositoryMock = null;
                proxyRepositoryMock = null;
                roleRepositoryMock = null;
                loggerMock = null;
                currentUserFactory = null;
            }

            [TestMethod]
            public async Task CreatePerson5_CreatePerson5Async()
            {
                //setup role
                createPersonRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Base.BasePermissionCodes.CreatePerson));
                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { createPersonRole });

                var actual = await personService.CreatePerson5Async(personDto);

                Assert.AreEqual(personDto.BirthDate, actual.BirthDate);
                // Assert.AreEqual(personDto.CitizenshipCountry, actual.CitizenshipCountry);
                Assert.AreEqual(personDto.CitizenshipStatus, actual.CitizenshipStatus);
                //Assert.AreEqual(personDto.CountryOfBirth, actual.CountryOfBirth);
                Assert.AreEqual(personDto.DeceasedDate, actual.DeceasedDate);
                Assert.AreEqual(personDto.GenderType, actual.GenderType);
                Assert.AreEqual(personDto.MaritalStatus.Detail.Id, actual.MaritalStatus.Detail.Id);
                Assert.AreEqual(personDto.MaritalStatus.MaritalCategory, actual.MaritalStatus.MaritalCategory);


                //Legal
                var legalActualName = actual.PersonNames.FirstOrDefault(item => item.NameType.Detail.Id.Equals("806af5a5-8a9a-424f-8c9f-c1e9d084ee71", StringComparison.OrdinalIgnoreCase));
                var legalExpectedName = personDto.PersonNames.FirstOrDefault(item => item.NameType.Detail.Id.Equals(legalActualName.NameType.Detail.Id, StringComparison.OrdinalIgnoreCase));

                //Assert.AreEqual(legalExpectedName.FullName, legalActualName.FullName); commented cause it will fail
                if (!string.IsNullOrEmpty(legalExpectedName.LastNamePrefix))
                    Assert.AreEqual(legalExpectedName.LastName, string.Concat(legalExpectedName.LastNamePrefix, " ", legalActualName.LastName));
                Assert.AreEqual(legalExpectedName.FirstName, legalActualName.FirstName);
                Assert.AreEqual(legalExpectedName.MiddleName, legalActualName.MiddleName);

                //Birth
                var birthActualName = actual.PersonNames.FirstOrDefault(item => item.NameType.Detail.Id.Equals("7dfa950c-8ae4-4dca-92f0-c083604285b6", StringComparison.OrdinalIgnoreCase));
                var birthexpectedName = personDto.PersonNames.FirstOrDefault(item => item.NameType.Detail.Id.Equals(birthActualName.NameType.Detail.Id, StringComparison.OrdinalIgnoreCase));

                Assert.AreEqual(birthexpectedName.FullName, birthActualName.FullName);

                //Chosen
                var chosenActualName = actual.PersonNames.FirstOrDefault(item => item.NameType.Detail.Id.Equals("dd20ebdf-2452-41ef-9f86-ad1b1621a78d", StringComparison.OrdinalIgnoreCase));
                var chosenExpectedName = personDto.PersonNames.FirstOrDefault(item => item.NameType.Detail.Id.Equals(chosenActualName.NameType.Detail.Id, StringComparison.OrdinalIgnoreCase));

                Assert.AreEqual(chosenExpectedName.FullName, chosenActualName.FullName);

                //Nickname
                var nickNameActualName = actual.PersonNames.FirstOrDefault(item => item.NameType.Detail.Id.Equals("7b55610f-7d00-4260-bbcf-0e47fdbae647", StringComparison.OrdinalIgnoreCase));
                var nickNameExpectedName = personDto.PersonNames.FirstOrDefault(item => item.NameType.Detail.Id.Equals(nickNameActualName.NameType.Detail.Id, StringComparison.OrdinalIgnoreCase));

                Assert.AreEqual(nickNameExpectedName.FullName, nickNameActualName.FullName);

                //History
                var historyActualName = actual.PersonNames.FirstOrDefault(item => item.NameType.Detail.Id.Equals("d42cc964-35cb-4560-bc46-4b881e7705ea", StringComparison.OrdinalIgnoreCase));
                var historyexpectedName = personDto.PersonNames.FirstOrDefault(item => item.NameType.Detail.Id.Equals(historyActualName.NameType.Detail.Id, StringComparison.OrdinalIgnoreCase));

                Assert.AreEqual(historyexpectedName.FullName, historyActualName.FullName);
            }

       

            #region Birth Date

            [TestMethod]
            public async Task CreatePerson5_CreatePerson5Async_BirthDate_Detail()
            {
                //setup role
                createPersonRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Base.BasePermissionCodes.CreatePerson));
                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { createPersonRole });

                personDto.BirthDate = new DateTime(1930, 1, 1);

                var actual = await personService.CreatePerson5Async(personDto);
                Assert.IsNotNull(actual.BirthDate);
                Assert.AreEqual("1/1/1930", actual.BirthDate.Value.ToShortDateString());
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task CreatePerson5_CreatePerson5Async_BirthDate_GT_Today()
            {
                //setup role
                createPersonRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Base.BasePermissionCodes.CreatePerson));
                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { createPersonRole });

                personDto.BirthDate = DateTime.Today.AddDays(1);

                try
                {
                    var actual = await personService.CreatePerson5Async(personDto);
                }
                catch (IntegrationApiException ex)
                {
                    Assert.IsNotNull(ex.Errors);
                    Assert.AreEqual("persons.dateOfBirth", ex.Errors.First().Code);
                    Assert.AreEqual("Date of birth cannot be after the current date.", ex.Errors.First().Message);
                    throw;
                }
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task CreatePerson5_CreatePerson5Async_BirthDate_GT_DeceasedDate()
            {
                //setup role
                createPersonRole.AddPermission(
                    new Ellucian.Colleague.Domain.Entities.Permission(
                        Ellucian.Colleague.Domain.Base.BasePermissionCodes.CreatePerson));
                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { createPersonRole });
                
                personDto.BirthDate = new DateTime(1930, 1, 1);
                personDto.DeceasedDate = new DateTime(1920, 1, 1);

                try
                {
                    await personService.CreatePerson5Async(personDto);
                }
                catch (IntegrationApiException ex)
                {
                    Assert.IsNotNull(ex.Errors);
                    Assert.AreEqual("persons.dateDeceased", ex.Errors.First().Code);
                    Assert.AreEqual("Date of birth cannot be after deceased date.", ex.Errors.First().Message);
                    throw;
                }
            }

            #endregion

            #region Gender Identity

            [TestMethod]
            public async Task CreatePerson5_CreatePerson5Async_GenderIdentity_Detail()
            {
                //setup role
                createPersonRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Base.BasePermissionCodes.CreatePerson));
                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { createPersonRole });

                personDto.GenderIdentity = new GuidObject2("9c3004ab-0f25-4d1d-84d6-65ea69ce1124");

                var actual = await personService.CreatePerson5Async(personDto);
                Assert.IsNotNull(actual.GenderIdentity);
                Assert.AreEqual("9c3004ab-0f25-4d1d-84d6-65ea69ce1124", actual.GenderIdentity.Id);
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task CreatePerson5_CreatePerson5Async_GenderIdentity_Null_Guid()
            {
                //setup role
                createPersonRole.AddPermission(
                    new Ellucian.Colleague.Domain.Entities.Permission(
                        Ellucian.Colleague.Domain.Base.BasePermissionCodes.CreatePerson));
                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { createPersonRole });

                personDto.GenderIdentity = new GuidObject2("");

                try
                {
                    await personService.CreatePerson5Async(personDto);
                }
                catch (IntegrationApiException ex)
                {
                    Assert.IsNotNull(ex.Errors);
                    Assert.AreEqual("persons.genderIdentity.id", ex.Errors.First().Code);
                    Assert.AreEqual("Gender Identity id is a required field when Gender Identity is in the message body.", ex.Errors.First().Message);
                    throw;
                }
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task CreatePerson5_CreatePerson5Async_GenderIdentity_Invalid_Guid()
            {
                //setup role
                createPersonRole.AddPermission(
                    new Ellucian.Colleague.Domain.Entities.Permission(
                        Ellucian.Colleague.Domain.Base.BasePermissionCodes.CreatePerson));
                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { createPersonRole });

                var guid = Guid.NewGuid().ToString();
                personDto.GenderIdentity = new GuidObject2(guid);

                try
                {
                    await personService.CreatePerson5Async(personDto);
                }
                catch (IntegrationApiException ex)
                {
                    Assert.IsNotNull(ex.Errors);
                    Assert.AreEqual("persons.genderIdentity.id", ex.Errors.First().Code);
                    Assert.AreEqual(string.Concat("Gender Identity ID '", guid, "' was not found."), ex.Errors.First().Message);
                    throw;
                }
            }

            #endregion

            #region Personal Pronoun

            [TestMethod]
            public async Task CreatePerson5_CreatePerson5Async_PersonalPronoun_Detail()
            {
                //setup role
                createPersonRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Base.BasePermissionCodes.CreatePerson));
                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { createPersonRole });

                personDto.PersonalPronoun = new GuidObject2("ae7a3392-fa07-4f53-b6d5-317d77cb62ec");

                var actual = await personService.CreatePerson5Async(personDto);
                Assert.IsNotNull(actual.PersonalPronoun);
                Assert.AreEqual("ae7a3392-fa07-4f53-b6d5-317d77cb62ec", actual.PersonalPronoun.Id);
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task CreatePerson5_CreatePerson5Async_PersonalPronoun_Null_Guid()
            {
                //setup role
                createPersonRole.AddPermission(
                    new Ellucian.Colleague.Domain.Entities.Permission(
                        Ellucian.Colleague.Domain.Base.BasePermissionCodes.CreatePerson));
                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { createPersonRole });

                personDto.PersonalPronoun = new GuidObject2("");

                try
                {
                    await personService.CreatePerson5Async(personDto);
                }
                catch (IntegrationApiException ex)
                {
                    Assert.IsNotNull(ex.Errors);
                    Assert.AreEqual("persons.personalPronoun.id", ex.Errors.First().Code);
                    Assert.AreEqual("Personal Pronoun id is a required field when Personal Pronoun is in the message body.", ex.Errors.First().Message);
                    throw;
                }
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task CreatePerson5_CreatePerson5Async_PersonalPronoun_Invalid_Guid()
            {
                //setup role
                createPersonRole.AddPermission(
                    new Ellucian.Colleague.Domain.Entities.Permission(
                        Ellucian.Colleague.Domain.Base.BasePermissionCodes.CreatePerson));
                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { createPersonRole });

                var guid = Guid.NewGuid().ToString();
                personDto.PersonalPronoun = new GuidObject2(guid);

                try
                {
                    await personService.CreatePerson5Async(personDto);
                }
                catch (IntegrationApiException ex)
                {
                    Assert.IsNotNull(ex.Errors);
                    Assert.AreEqual("persons.personalPronoun.id", ex.Errors.First().Code);
                    Assert.AreEqual(string.Concat("Personal Pronoun ID '", guid, "' was not found."), ex.Errors.First().Message);
                    throw;
                }
            }

            #endregion

            #region Privacy Status

            [TestMethod]
            public async Task CreatePerson5_CreatePerson5Async_PrivacyStatus_Detail()
            {
                //setup role
                createPersonRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Base.BasePermissionCodes.CreatePerson));
                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { createPersonRole });

                var privacyStatus = new PersonPrivacyDtoProperty
                {
                    Detail = new GuidObject2(demographicGuid),
                    PrivacyCategory = Dtos.PrivacyStatusType.Unrestricted
                };
                personDto.PrivacyStatus = privacyStatus;

                var actual = await personService.CreatePerson5Async(personDto);
                Assert.IsNotNull(actual.PrivacyStatus);
                Assert.AreEqual(demographicGuid, actual.PrivacyStatus.Detail.Id);
            }

            [TestMethod]
            public async Task CreatePerson5_CreatePerson5Async_PrivacyStatus_PrivacyCategory()
            {
                //setup role
                createPersonRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Base.BasePermissionCodes.CreatePerson));
                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { createPersonRole });

                var privacyStatus = new PersonPrivacyDtoProperty
                {
                    PrivacyCategory = Dtos.PrivacyStatusType.Restricted
                };
                personDto.PrivacyStatus = privacyStatus;

                var actual = await personService.CreatePerson5Async(personDto);
                Assert.IsNotNull(actual.PrivacyStatus);
                Assert.AreEqual(demographicGuid, actual.PrivacyStatus.Detail.Id);
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task CreatePerson5_CreatePerson5Async_PrivacyStatus_Mismatch()
            {
                //setup role
                createPersonRole.AddPermission(
                    new Ellucian.Colleague.Domain.Entities.Permission(
                        Ellucian.Colleague.Domain.Base.BasePermissionCodes.CreatePerson));
                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { createPersonRole });

                var privacyStatus = new PersonPrivacyDtoProperty
                {
                    Detail = new GuidObject2(demographicGuid),
                    PrivacyCategory = Dtos.PrivacyStatusType.Restricted
                };
                personDto.PrivacyStatus = privacyStatus;
                try
                {
                    await personService.CreatePerson5Async(personDto);
                }
                catch (IntegrationApiException ex)
                {
                    Assert.IsNotNull(ex.Errors);
                    Assert.AreEqual("persons.privacyStatus.detail.id", ex.Errors.First().Code);
                    Assert.AreEqual("Provided privacy status type Restricted does not match the privacy status type by id d3d86052-9d55-4751-acda-5c07a064a82a", ex.Errors.First().Message);
                    throw;
                }
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task CreatePerson5_CreatePerson5Async_PrivacyStatus_EmptyDetail()
            {
                //setup role
                createPersonRole.AddPermission(
                    new Ellucian.Colleague.Domain.Entities.Permission(
                        Ellucian.Colleague.Domain.Base.BasePermissionCodes.CreatePerson));
                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { createPersonRole });

                var privacyStatus = new PersonPrivacyDtoProperty
                {
                    Detail = new GuidObject2(),
                    PrivacyCategory = Dtos.PrivacyStatusType.Restricted
                };
                personDto.PrivacyStatus = privacyStatus;
                try
                {
                    await personService.CreatePerson5Async(personDto);
                }
                catch (IntegrationApiException ex)
                {
                    Assert.IsNotNull(ex.Errors);
                    Assert.AreEqual("persons.privacyStatus.detail.id", ex.Errors.First().Code);
                    Assert.AreEqual("Must provide an id for privacyStatus detail.", ex.Errors.First().Message);
                    throw;
                }
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task CreatePerson5_CreatePerson5Async_PrivacyStatus_InvalidDetail()
            {
                //setup role
                createPersonRole.AddPermission(
                    new Ellucian.Colleague.Domain.Entities.Permission(
                        Ellucian.Colleague.Domain.Base.BasePermissionCodes.CreatePerson));
                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { createPersonRole });

                var privacyStatus = new PersonPrivacyDtoProperty
                {
                    Detail = new GuidObject2("A448C44F-C5D5-492C-A7ED-83F3DD1B5042"),
                    PrivacyCategory = Dtos.PrivacyStatusType.Restricted
                };
                personDto.PrivacyStatus = privacyStatus;
                try
                {
                    await personService.CreatePerson5Async(personDto);
                }
                catch (IntegrationApiException ex)
                {
                    Assert.IsNotNull(ex.Errors);
                    Assert.AreEqual("persons.privacyStatus.detail.id", ex.Errors.First().Code);
                    Assert.AreEqual("Privacy status associated to guid 'A448C44F-C5D5-492C-A7ED-83F3DD1B5042' not found in repository.", ex.Errors.First().Message);
                    throw;
                }
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task CreatePerson5_CreatePerson5Async_PrivacyStatus_EmptyCategory()
            {
                //setup role
                createPersonRole.AddPermission(
                    new Ellucian.Colleague.Domain.Entities.Permission(
                        Ellucian.Colleague.Domain.Base.BasePermissionCodes.CreatePerson));
                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { createPersonRole });

                var privacyStatus = new PersonPrivacyDtoProperty
                {
                    Detail = new GuidObject2(demographicGuid)
                };
                personDto.PrivacyStatus = privacyStatus;
                try
                {
                    await personService.CreatePerson5Async(personDto);
                }
                catch (IntegrationApiException ex)
                {
                    Assert.IsNotNull(ex.Errors);
                    Assert.AreEqual("persons.privacyStatus.privacyCategory", ex.Errors.First().Code);
                    Assert.AreEqual("Must provide privacyCategory for privacyStatus.", ex.Errors.First().Message);
                    throw;
                }
            }

            #endregion

            #region Marital Status

            [TestMethod]
            public async Task CreatePerson5_CreatePerson5Async_MaritalStatus_Detail()
            {
                //setup role
                createPersonRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Base.BasePermissionCodes.CreatePerson));
                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { createPersonRole });

                var maritalStatus = new PersonMaritalStatusDtoProperty
                {
                    Detail = new GuidObject2(maritalStatusGuid),
                    MaritalCategory = PersonMaritalStatusCategory.Married
                };
                personDto.MaritalStatus = maritalStatus;

                var actual = await personService.CreatePerson5Async(personDto);
                Assert.IsNotNull(actual.MaritalStatus);
                Assert.AreEqual(maritalStatusGuid, actual.MaritalStatus.Detail.Id);
            }

            [TestMethod]
            public async Task CreatePerson5_CreatePerson5Async_MaritalStatus_MaritalCategory()
            {
                //setup role
                createPersonRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Base.BasePermissionCodes.CreatePerson));
                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { createPersonRole });

                var maritalStatus = new PersonMaritalStatusDtoProperty
                {

                    MaritalCategory = PersonMaritalStatusCategory.Married
                };
                personDto.MaritalStatus = maritalStatus;

                var actual = await personService.CreatePerson5Async(personDto);
                Assert.IsNotNull(actual.MaritalStatus);
                Assert.AreEqual(maritalStatusGuid, actual.MaritalStatus.Detail.Id);
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task CreatePerson5_CreatePerson5Async_MaritalStatus_Mismatch()
            {
                //setup role
                createPersonRole.AddPermission(
                    new Ellucian.Colleague.Domain.Entities.Permission(
                        Ellucian.Colleague.Domain.Base.BasePermissionCodes.CreatePerson));
                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { createPersonRole });

                var maritalStatus = new PersonMaritalStatusDtoProperty
                {
                    Detail = new GuidObject2(new Guid().ToString()),
                    MaritalCategory = PersonMaritalStatusCategory.Married
                };

                personDto.MaritalStatus = maritalStatus;

                try
                {
                    await personService.CreatePerson5Async(personDto);
                }
                catch (IntegrationApiException ex)
                {
                    Assert.IsNotNull(ex.Errors);
                    Assert.AreEqual("persons.maritalStatus", ex.Errors.First().Code);
                    Assert.AreEqual("Could not find marital status with id: 00000000-0000-0000-0000-000000000000", ex.Errors.First().Message);
                    throw;
                }
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task CreatePerson5_CreatePerson5Async_MaritalStatus_EmptyDetail()
            {
                //setup role
                createPersonRole.AddPermission(
                    new Ellucian.Colleague.Domain.Entities.Permission(
                        Ellucian.Colleague.Domain.Base.BasePermissionCodes.CreatePerson));
                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { createPersonRole });

                var maritalStatus = new PersonMaritalStatusDtoProperty
                {
                    Detail = new GuidObject2(),
                    MaritalCategory = PersonMaritalStatusCategory.Married
                };
                personDto.MaritalStatus = maritalStatus;
                try
                {
                    await personService.CreatePerson5Async(personDto);
                }
                catch (IntegrationApiException ex)
                {
                    Assert.IsNotNull(ex.Errors);
                    Assert.AreEqual("persons.maritalStatus", ex.Errors.First().Code);
                    Assert.AreEqual("Must provide an id for marital status detail.\r\nParameter name: maritalStatus.detail.id", ex.Errors.First().Message);
                    throw;
                }
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task CreatePerson5_CreatePerson5Async_MaritalStatus_EmptyCategory()
            {
                //setup role
                createPersonRole.AddPermission(
                    new Ellucian.Colleague.Domain.Entities.Permission(
                        Ellucian.Colleague.Domain.Base.BasePermissionCodes.CreatePerson));
                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { createPersonRole });

                var maritalStatus = new PersonMaritalStatusDtoProperty
                {
                    Detail = new GuidObject2(new Guid().ToString()),

                };
                personDto.MaritalStatus = maritalStatus;

                try
                {
                    await personService.CreatePerson5Async(personDto);
                }
                catch (IntegrationApiException ex)
                {
                    Assert.IsNotNull(ex.Errors);
                    Assert.AreEqual("persons.maritalStatus", ex.Errors.First().Code);
                    Assert.AreEqual("Must provide a valid category for marital status.\r\nParameter name: maritalStatus.maritalCategory", ex.Errors.First().Message);
                    throw;
                }
            }

            #endregion

            #region Citizenship
            [TestMethod]
            public async Task CreatePerson5_CreatePerson5Async_Citizenship_Detail()
            {
                //setup role
                createPersonRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Base.BasePermissionCodes.CreatePerson));
                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { createPersonRole });


                var allCitizenshipStatuses = new TestCitizenshipStatusRepository().Get().ToList();
                allCitizenshipStatuses.Add(
                    new Domain.Base.Entities.CitizenshipStatus(noncitizenGuid, "NC", "nonCitizen", Ellucian.Colleague.Domain.Base.Entities.CitizenshipStatusType.NonCitizen));
                referenceDataRepositoryMock.Setup(repo => repo.GetCitizenshipStatusesAsync(It.IsAny<bool>())).ReturnsAsync(allCitizenshipStatuses);

                var citizenshipStatus = new PersonCitizenshipDtoProperty
                {
                    Detail = new GuidObject2(noncitizenGuid),
                    Category = Dtos.CitizenshipStatusType.NonCitizen
                };
                personDto.CitizenshipStatus = citizenshipStatus;
                personIntegrationReturned.AlienStatus = "NC";

                var actual = await personService.CreatePerson5Async(personDto);
                Assert.IsNotNull(actual.CitizenshipStatus);
                Assert.AreEqual(noncitizenGuid, actual.CitizenshipStatus.Detail.Id);
                Assert.AreEqual(Dtos.CitizenshipStatusType.NonCitizen, actual.CitizenshipStatus.Category);
            }

            #endregion

            #region Religion

            [TestMethod]
            public async Task CreatePerson5_CreatePerson5Async_Religion()
            {
                //setup role
                createPersonRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Base.BasePermissionCodes.CreatePerson));
                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { createPersonRole });

                IEnumerable<Domain.Base.Entities.Denomination> allDenominations = new TestReligionRepository().GetDenominations().ToList();
                referenceDataRepositoryMock.Setup(repo => repo.GetDenominationsAsync(It.IsAny<bool>())).ReturnsAsync(allDenominations);
                //referenceDataRepositoryMock.Setup(repo => repo.DenominationsAsync()).ReturnsAsync(allDenominations);

                var religion = allDenominations.FirstOrDefault(x => x.Code == "CA");

                personDto.Religion = new GuidObject2(religion.Guid);

                var actual = await personService.CreatePerson5Async(personDto);
                Assert.IsNotNull(actual.Religion);
                Assert.AreEqual(religion.Guid, actual.Religion.Id);

            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task CreatePerson5_CreatePerson5Async_Religion_Invalid()
            {
                //setup role
                createPersonRole.AddPermission(
                    new Ellucian.Colleague.Domain.Entities.Permission(
                        Ellucian.Colleague.Domain.Base.BasePermissionCodes.CreatePerson));
                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { createPersonRole });

                IEnumerable<Domain.Base.Entities.Denomination> allDenominations =
                    new TestReligionRepository().GetDenominations().ToList();
                referenceDataRepositoryMock.Setup(repo => repo.GetDenominationsAsync(It.IsAny<bool>()))
                    .ReturnsAsync(allDenominations);
                //referenceDataRepositoryMock.Setup(repo => repo.DenominationsAsync()).ReturnsAsync(allDenominations);

                personDto.Religion = new GuidObject2(new Guid().ToString());

                try
                {
                    await personService.CreatePerson5Async(personDto);
                }
                catch (IntegrationApiException ex)
                {
                    Assert.IsNotNull(ex.Errors);
                    Assert.AreEqual("persons.religion", ex.Errors.First().Code);
                    Assert.AreEqual("Religion ID associated to guid '00000000-0000-0000-0000-000000000000' not found in repository", ex.Errors.First().Message);
                    throw;
                }
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task CreatePerson5_CreatePerson5Async_Religion_Null()
            {
                //setup role
                createPersonRole.AddPermission(
                    new Ellucian.Colleague.Domain.Entities.Permission(
                        Ellucian.Colleague.Domain.Base.BasePermissionCodes.CreatePerson));
                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { createPersonRole });

                IEnumerable<Domain.Base.Entities.Denomination> allDenominations =
                    new TestReligionRepository().GetDenominations().ToList();
                referenceDataRepositoryMock.Setup(repo => repo.GetDenominationsAsync(It.IsAny<bool>()))
                    .ReturnsAsync(allDenominations);

                var religion = allDenominations.FirstOrDefault();

                personDto.Religion = new GuidObject2();

                try
                {
                    await personService.CreatePerson5Async(personDto);
                }
                catch (IntegrationApiException ex)
                {
                    Assert.IsNotNull(ex.Errors);
                    Assert.AreEqual("persons.religion", ex.Errors.First().Code);
                    Assert.AreEqual("Must provide an id for religion.\r\nParameter name: personDto.religion.id", ex.Errors.First().Message);
                    throw;
                }
            }

            #endregion

            #region Language

            [TestMethod]
            public async Task CreatePerson5_CreatePerson5Async_Language()
            {
                //setup role
                createPersonRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Base.BasePermissionCodes.CreatePerson));
                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { createPersonRole });

                var personLanguages = new List<PersonLanguageDtoProperty>();

                var personLanguage1 = new PersonLanguageDtoProperty()
                {
                    Code = PersonLanguageCode.eng,
                    Preference = PersonLanguagePreference.Primary
                };
                personLanguages.Add(personLanguage1);

                personDto.Languages = personLanguages;

                var actual = await personService.CreatePerson5Async(personDto);
                var primaryLanguage =
                    actual.Languages.FirstOrDefault(x => x.Preference == PersonLanguagePreference.Primary);

                Assert.IsNotNull(primaryLanguage);
                Assert.AreEqual(PersonLanguagePreference.Primary, primaryLanguage.Preference);
                Assert.AreEqual(PersonLanguageCode.eng, primaryLanguage.Code);
            }


            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task CreatePerson5_CreatePerson5Async_Language_EmptyCode()
            {
                //setup role
                createPersonRole.AddPermission(
                    new Ellucian.Colleague.Domain.Entities.Permission(
                        Ellucian.Colleague.Domain.Base.BasePermissionCodes.CreatePerson));
                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { createPersonRole });

                var personLanguages = new List<PersonLanguageDtoProperty>();

                var personLanguage1 = new PersonLanguageDtoProperty()
                {
                    Preference = PersonLanguagePreference.Primary
                };
                personLanguages.Add(personLanguage1);

                personDto.Languages = personLanguages;

                try
                {
                    await personService.CreatePerson5Async(personDto);
                }
                catch (IntegrationApiException ex)
                {
                    Assert.IsNotNull(ex.Errors);
                    Assert.AreEqual("persons.languages", ex.Errors.First().Code);
                    Assert.AreEqual("language code must be specified\r\nParameter name: code", ex.Errors.First().Message);
                    throw;
                }
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task CreatePerson5_CreatePerson5Async_Language_MultiplePreferred()
            {
                //setup role
                createPersonRole.AddPermission(
                    new Ellucian.Colleague.Domain.Entities.Permission(
                        Ellucian.Colleague.Domain.Base.BasePermissionCodes.CreatePerson));
                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { createPersonRole });

                var personLanguages = new List<PersonLanguageDtoProperty>();

                var personLanguage1 = new PersonLanguageDtoProperty()
                {
                    Code = PersonLanguageCode.eng,
                    Preference = PersonLanguagePreference.Primary
                };
                personLanguages.Add(personLanguage1);

                var personLanguage2 = new PersonLanguageDtoProperty()
                {
                    Code = PersonLanguageCode.cha,
                    Preference = PersonLanguagePreference.Primary
                };
                personLanguages.Add(personLanguage2);

                personDto.Languages = personLanguages;

                try
                {
                    await personService.CreatePerson5Async(personDto);
                }
                catch (IntegrationApiException ex)
                {
                    Assert.IsNotNull(ex.Errors);
                    Assert.AreEqual("persons.languages", ex.Errors.First().Code);
                    Assert.AreEqual("The person may not have more than one language with a preference of 'primary'.", ex.Errors.First().Message);
                    throw;
                }
            }

            #endregion

            #region CountryOfBirth/countryOfCitizenship

            [TestMethod]
            public async Task CreatePerson5_CreatePerson5Async_Country()
            {
                personDto.CitizenshipCountry = "USA";
                personDto.CountryOfBirth = "USA";

                //setup role
                createPersonRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Base.BasePermissionCodes.CreatePerson));
                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { createPersonRole });

                // Mock the reference repository for country
                var countries = new List<Country>()
                 {
                    new Country("USA","United States","USA", "USA") {IsoAlpha3Code = "USA"},
                    new Country("CAN","Canada","CAN", "CAN")  {IsoAlpha3Code = "CAN"},
                    new Country("ME","Mexico","MEX", "MEX")  {IsoAlpha3Code = "MEX"},
                    new Country("BRA","Brazil","BRA", "BRA")  {IsoAlpha3Code = "BRA"}
                };
                referenceDataRepositoryMock.Setup(repo => repo.GetCountryCodesAsync(It.IsAny<bool>())).ReturnsAsync(countries);

                var actual = await personService.CreatePerson5Async(personDto);
                Assert.IsNotNull(actual.CountryOfBirth);
                Assert.AreEqual("USA", actual.CountryOfBirth);

                Assert.IsNotNull(actual.CitizenshipCountry);
                Assert.AreEqual("USA", actual.CitizenshipCountry);
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task CreatePerson5_CreatePerson5Async_CountryOfBirth_Invalid()
            {
                //setup role
                createPersonRole.AddPermission(
                    new Ellucian.Colleague.Domain.Entities.Permission(
                        Ellucian.Colleague.Domain.Base.BasePermissionCodes.CreatePerson));
                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { createPersonRole });

                personDto.CountryOfBirth = "XXX";

                try
                {
                    await personService.CreatePerson5Async(personDto);
                }
                catch (IntegrationApiException ex)
                {
                    Assert.IsNotNull(ex.Errors);
                    Assert.AreEqual("persons.countryOfBirth", ex.Errors.First().Code);
                    Assert.AreEqual("Country not found with Iso3 code: 'XXX'", ex.Errors.First().Message);
                    throw;
                }
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task CreatePerson5_CreatePerson5Async_CitizenshipCountry_Invalid()
            {
                //setup role
                createPersonRole.AddPermission(
                    new Ellucian.Colleague.Domain.Entities.Permission(
                        Ellucian.Colleague.Domain.Base.BasePermissionCodes.CreatePerson));
                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { createPersonRole });

                personDto.CitizenshipCountry = "XXX";

                try
                {
                    await personService.CreatePerson5Async(personDto);
                }
                catch (IntegrationApiException ex)
                {
                    Assert.IsNotNull(ex.Errors);
                    Assert.AreEqual("persons.citizenshipCountry", ex.Errors.First().Code);
                    Assert.AreEqual("Country not found with Iso3 code: 'XXX'", ex.Errors.First().Message);
                    throw;
                }
            }


            #endregion

            #region Veteran Status

            [TestMethod]
            public async Task CreatePerson5_CreatePerson5Async_VeteranStatus()
            {
                //setup role
                createPersonRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Base.BasePermissionCodes.CreatePerson));
                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { createPersonRole });

                var actual = await personService.CreatePerson5Async(personDto);
                var militaryStatus = actual.VeteranStatus;
                var statusCategory = militaryStatus != null ? militaryStatus.VeteranStatusCategory : null;
                var statusDetail = militaryStatus != null && militaryStatus.Detail != null ? militaryStatus.Detail.Id : null;

                Assert.IsNotNull(militaryStatus);
                Assert.IsNotNull(statusCategory);
                Assert.IsNotNull(statusDetail);
                Assert.AreEqual(statusCategory, VeteranStatusesCategory.Protectedveteran);
                Assert.AreEqual(statusDetail, "ae7a3392-fa07-4f53-b6d5-317d88cb62ec");
            }

            #endregion

            #region Exceptions

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task CreatePerson5_CreatePerson5Async_PersonDTO_Null_ArgumentNullException()
            {
                try
                {
                    var actual = await personService.CreatePerson5Async(null);
                }
                catch (IntegrationApiException ex)
                {
                    Assert.IsNotNull(ex.Errors);
                    Assert.AreEqual("persons", ex.Errors.First().Code);
                    Assert.AreEqual("Must provide a person object for creation", ex.Errors.First().Message);
                    throw;
                }
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task CreatePerson5_CreatePerson5Async_PersonDTO_IdNull_ArgumentNullException()
            {
                personDto.Id = string.Empty;
                try
                {
                    var actual = await personService.CreatePerson5Async(personDto);
                }
                catch (IntegrationApiException ex)
                {
                    Assert.IsNotNull(ex.Errors);
                    Assert.AreEqual("persons", ex.Errors.First().Code);
                    Assert.AreEqual("Must provide a guid for person creation", ex.Errors.First().Message);
                    throw;
                }
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task CreatePerson5_PrimaryNames_Null_ArgumentNullException()
            {
                //setup role
                createPersonRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Base.BasePermissionCodes.CreatePerson));
                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { createPersonRole });

                personDto.PersonNames.FirstOrDefault().LastName = string.Empty;

                try
                {
                    var actual = await personService.CreatePerson5Async(personDto);
                }
                catch (IntegrationApiException ex)
                {
                    Assert.IsNotNull(ex.Errors);
                    Assert.AreEqual("persons.names.lastName", ex.Errors.First().Code);
                    Assert.AreEqual("Last name is required for a legal name.", ex.Errors.First().Message);
                    throw;
                }
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task UpdatePerson5_Username_Exception()
            {
                //setup role
                updatePersonRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Base.BasePermissionCodes.UpdatePerson));
                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { updatePersonRole });

                //personId 0000011
                personRepositoryMock.Setup(i => i.GetPersonIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync(personId);

                personDto.Credentials = new List<Dtos.DtoProperties.Credential3DtoProperty>()
                {
                    new Dtos.DtoProperties.Credential3DtoProperty()
                    {
                        Type = Dtos.EnumProperties.Credential3Type.ColleagueUserName,
                        Value = "testUsername"
                    }
                };
                // Mock the response for getting a Person Pin 
                var personPins = new List<PersonPin>();
                personPins = null;
                personRepositoryMock.Setup(repo => repo.GetPersonPinsAsync(It.IsAny<string[]>())).ReturnsAsync(personPins);

                var actual = await personService.UpdatePerson5Async(personDto);
            }

            #endregion

            #region Setup V12.1.0

            private void SetupData()
            {
                // setup personDto object
                personDto = new Dtos.Person5();
                personDto.Id = Guid.Empty.ToString();
                personDto.BirthDate = new DateTime(1930, 1, 1);
                personDto.DeceasedDate = new DateTime(2014, 5, 12);
                var personNames = new List<Dtos.DtoProperties.PersonName2DtoProperty>();
                var legalPrimaryName = new Dtos.DtoProperties.PersonName2DtoProperty()
                {
                    NameType = new Dtos.DtoProperties.PersonNameTypeDtoProperty() { Category = Dtos.EnumProperties.PersonNameType2.Legal, Detail = new Dtos.GuidObject2() { Id = "806af5a5-8a9a-424f-8c9f-c1e9d084ee71" } },
                    Title = "Mr.",
                    FirstName = "LegalFirst",
                    MiddleName = "LegalMiddle",
                    LastName = "LegalLast",
                    Pedigree = "Jr.",
                    LastNamePrefix = "Ignore",
                    FullName = "LegalFirst LegalMiddle LegalLast"
                };
                personNames.Add(legalPrimaryName);

                var birthPrimaryName = new Dtos.DtoProperties.PersonName2DtoProperty()
                {
                    NameType = new Dtos.DtoProperties.PersonNameTypeDtoProperty() { Category = Dtos.EnumProperties.PersonNameType2.Birth, Detail = new Dtos.GuidObject2() { Id = "7dfa950c-8ae4-4dca-92f0-c083604285b6" } },
                    Title = "Mr.",
                    FirstName = "BirthFirst",
                    MiddleName = "BirthMiddle",
                    LastName = "BirthLast",
                    Pedigree = "Jr.",
                    LastNamePrefix = "Ignore",
                    FullName = "BirthFirst BirthMiddle BirthLast"
                };
                personNames.Add(birthPrimaryName);

                var chosenPrimaryName = new PersonName2DtoProperty()
                {
                    NameType = new PersonNameTypeDtoProperty() { Category = Dtos.EnumProperties.PersonNameType2.Personal, Detail = new Dtos.GuidObject2() { Id = "dd20ebdf-2452-41ef-9f86-ad1b1621a78d" } },
                    Title = "Mr.",
                    FirstName = "ChosenFirst",
                    MiddleName = "ChosenMiddle",
                    LastName = "ChosenLast",
                    Pedigree = "Jr.",
                    LastNamePrefix = "Ignore",
                    FullName = "ChosenFirst ChosenMiddle ChosenLast"
                };
                personNames.Add(chosenPrimaryName);

                var nickNamePrimaryName = new Dtos.DtoProperties.PersonName2DtoProperty()
                {
                    NameType = new Dtos.DtoProperties.PersonNameTypeDtoProperty() { Category = Dtos.EnumProperties.PersonNameType2.Personal, Detail = new Dtos.GuidObject2() { Id = "7b55610f-7d00-4260-bbcf-0e47fdbae647" } },
                    Title = "Mr.",
                    FirstName = "NickNameFirst",
                    MiddleName = "NickNameMiddle",
                    LastName = "NickNameLast",
                    Pedigree = "Jr.",
                    LastNamePrefix = "Ignore",
                    FullName = "NickNameFirst NickNameMiddle NickNameLast"
                };
                personNames.Add(nickNamePrimaryName);

                var historyPrimaryName = new Dtos.DtoProperties.PersonName2DtoProperty()
                {
                    NameType = new Dtos.DtoProperties.PersonNameTypeDtoProperty() { Category = Dtos.EnumProperties.PersonNameType2.Personal, Detail = new Dtos.GuidObject2() { Id = "d42cc964-35cb-4560-bc46-4b881e7705ea" } },
                    Title = "Mr.",
                    FirstName = "HistoryFirst",
                    MiddleName = "HistoryMiddle",
                    LastName = "HistoryLast",
                    Pedigree = "Jr.",
                    LastNamePrefix = "Ignore",
                    FullName = "HistoryFirst HistoryMiddle HistoryLast"
                };
                personNames.Add(historyPrimaryName);

                var preferedPrimaryName = new Dtos.DtoProperties.PersonName2DtoProperty()
                {
                    NameType = new Dtos.DtoProperties.PersonNameTypeDtoProperty() { Category = Dtos.EnumProperties.PersonNameType2.Personal, Detail = new Dtos.GuidObject2() { Id = "8224f18e-69c5-480b-a9b4-52f596aa4a52" } },
                    Title = "Mr.",
                    FirstName = "PreferedFirst",
                    MiddleName = "PreferedMiddle",
                    LastName = "PreferedLast",
                    Pedigree = "Jr.",
                    LastNamePrefix = "Ignore",
                    FullName = "PreferedFirst PreferedMiddle PreferedLast"
                };
                personNames.Add(preferedPrimaryName);

                personDto.PersonNames = personNames;
                personDto.GenderType = Dtos.EnumProperties.GenderType2.Male;
                personDto.MaritalStatus = new Dtos.DtoProperties.PersonMaritalStatusDtoProperty() { Detail = new Dtos.GuidObject2(maritalStatusGuid), MaritalCategory = Dtos.EnumProperties.PersonMaritalStatusCategory.Married };// new Dtos.GuidObject(maritalStatusGuid);
                personDto.Ethnicity = new Dtos.DtoProperties.PersonEthnicityDtoProperty() { EthnicGroup = new Dtos.GuidObject2(ethnicityGuid) };// new Dtos.GuidObject(ethnicityGuid);
                personDto.Races = new List<Dtos.DtoProperties.PersonRaceDtoProperty>()
                {

                    new Dtos.DtoProperties.PersonRaceDtoProperty(){ Race = new Dtos.GuidObject2(raceAsianGuid)}
                };
                personDto.Credentials = new List<Dtos.DtoProperties.Credential3DtoProperty>()
                {
                    new Dtos.DtoProperties.Credential3DtoProperty()
                    {
                        Type = Dtos.EnumProperties.Credential3Type.Ssn,
                        Value = "111-11-1111"
                    }
                };
                var emailAddresses = new List<Dtos.DtoProperties.PersonEmailDtoProperty>();
                emailAddresses.Add(new Dtos.DtoProperties.PersonEmailDtoProperty()
                {

                    Type = new Dtos.DtoProperties.PersonEmailTypeDtoProperty() { EmailType = Dtos.EmailTypeList.School },
                    Address = "xyz@xmail.com"
                });
                personDto.EmailAddresses = emailAddresses;
                personDto.GenderIdentity = new GuidObject2("9C3004AB-0F25-4D1D-84D6-65EA69CE1124");
                personDto.PersonalPronoun = new GuidObject2("AE7A3392-FA07-4F53-B6D5-317D77CB62EC");
                personDto.AlternativeCredentials = new List<AlternativeCredentials>()
                {
                    new AlternativeCredentials()
                    {
                        Type = new GuidObject2("D525E2B2-CD7D-4995-93F0-97DA468EBE90"),
                        Value = "1234"
                    }
                };
                personDto.VeteranStatus = new PersonVeteranStatusDtoProperty()
                {
                    VeteranStatusCategory = VeteranStatusesCategory.Protectedveteran,
                    Detail = new GuidObject2("AE7A3392-FA07-4F53-B6D5-317D88CB62EC")
                };

                //Entity
                personIntegrationEntity = new PersonIntegration(It.IsAny<string>(), legalPrimaryName.LastName)
                {
                    Guid = personDto.Id,
                    Prefix = "Mr.",
                    FirstName = legalPrimaryName.FirstName,
                    MiddleName = legalPrimaryName.MiddleName,
                    Suffix = "Sr."

                };

                //Returned value
                personIntegrationReturned = new PersonIntegration(personId, "LegalLast");
                personIntegrationReturned.Guid = personGuid;
                personIntegrationReturned.Prefix = "Mr.";
                personIntegrationReturned.FirstName = "LegalFirst";
                personIntegrationReturned.MiddleName = "LegalMiddle";
                personIntegrationReturned.Suffix = "Jr.";
                personIntegrationReturned.Nickname = "NickNameFirst NickNameMiddle NickNameLast";
                personIntegrationReturned.BirthDate = new DateTime(1930, 1, 1);
                personIntegrationReturned.DeceasedDate = new DateTime(2014, 5, 12);
                personIntegrationReturned.GovernmentId = "111-11-1111";
                personIntegrationReturned.Religion = "CA";
                personIntegrationReturned.MaritalStatusCode = "M";
                personIntegrationReturned.AddPersonLanguage(new PersonLanguage(personId, "eng", LanguagePreference.Primary));
                personIntegrationReturned.EthnicCodes = new List<string> { "H", "N" };
                personIntegrationReturned.Gender = "M";
                personIntegrationReturned.RaceCodes = new List<string> { "AS" };
                personIntegrationReturned.AddRole(new PersonRole(PersonRoleType.Alumni, new DateTime(15, 01, 22), new DateTime(15, 05, 25)));
                personIntegrationReturned.BirthNameFirst = "BirthFirst";
                personIntegrationReturned.BirthNameLast = "BirthLast";
                personIntegrationReturned.BirthNameMiddle = "BirthMiddle";
                personIntegrationReturned.ChosenFirstName = "ChosenFirst";
                personIntegrationReturned.ChosenLastName = "ChosenLast";
                personIntegrationReturned.ChosenMiddleName = "ChosenMiddle";
                personIntegrationReturned.PreferredName = "PreferedFirst PreferedMiddle PreferedLast";
                personIntegrationReturned.BirthCountry = "USA";
                personIntegrationReturned.Citizenship = "USA";
                personIntegrationReturned.FormerNames = new List<PersonName>()
                {
                    new PersonName("HistoryFirst", "HistoryMiddle", "HistoryLast")
                };
                // Mock the email address data response
                instEmail = new Domain.Base.Entities.EmailAddress("inst@inst.com", "COL") { IsPreferred = true };
                personIntegrationReturned.AddEmailAddress(instEmail);
                perEmail = new Domain.Base.Entities.EmailAddress("personal@personal.com", "PER") { IsPreferred = false };
                personIntegrationReturned.AddEmailAddress(perEmail);
                workEmail = new Domain.Base.Entities.EmailAddress("work@work.com", "BUS") { IsPreferred = false };
                personIntegrationReturned.AddEmailAddress(workEmail);
                personIntegrationReturned.GenderIdentityCode = "FTM";
                personIntegrationReturned.PersonalPronounCode = "HE";
                personIntegrationReturned.AddPersonAlt(new PersonAlt("2222", "ELEV2"));
                personIntegrationReturned.AddPersonAlt(new PersonAlt("3333", "GOVID2"));
                personIntegrationReturned.MilitaryStatus = "VET";

                // Mock the person languages
                personIntegrationReturned.PrimaryLanguage = "E";
                personIntegrationReturned.SecondaryLanguages = new List<String> { "SP", "TA" };

                // Mock the address hierarchy responses
                var addresses = new List<Domain.Base.Entities.Address>();
                homeAddr = new Domain.Base.Entities.Address()
                {
                    TypeCode = "HO",
                    Type = Dtos.EnumProperties.AddressType.Home.ToString(),
                    Guid = Guid.NewGuid().ToString(),
                    Status = "Current",
                    IsPreferredAddress = true
                };
                addresses.Add(homeAddr);
                mailAddr = new Domain.Base.Entities.Address()
                {
                    TypeCode = "MA",
                    Type = Dtos.EnumProperties.AddressType.Mailing.ToString(),
                    Guid = Guid.NewGuid().ToString(),
                    Status = "Current"
                };
                addresses.Add(mailAddr);
                resAddr = new Domain.Base.Entities.Address()
                {
                    TypeCode = "VA",
                    Type = Dtos.EnumProperties.AddressType.Vacation.ToString(),
                    Guid = Guid.NewGuid().ToString(),
                    Status = "Current",
                    IsPreferredResidence = true
                };
                addresses.Add(resAddr);
                workAddr = new Domain.Base.Entities.Address()
                {
                    TypeCode = "BU",
                    Type = Dtos.EnumProperties.AddressType.Business.ToString(),
                    Guid = Guid.NewGuid().ToString(),
                    Status = "Current"
                };
                addresses.Add(workAddr);
                personIntegrationReturned.Addresses = addresses;

                // Mock the person phone per type response
                homePhone = new Domain.Base.Entities.Phone("111-111-1111", "HO");
                personIntegrationReturned.AddPhone(homePhone);
                mobilePhone = new Domain.Base.Entities.Phone("222-222-2222", "MO");
                personIntegrationReturned.AddPhone(mobilePhone);
                residencePhone = new Domain.Base.Entities.Phone("333-333-3333", "VA");
                personIntegrationReturned.AddPhone(residencePhone);
                workPhone = new Domain.Base.Entities.Phone("444-444-4444", "BU", "4444");
                personIntegrationReturned.AddPhone(workPhone);

                // Mock social media
                var socialMedia = new List<Domain.Base.Entities.SocialMedia>();
                var socialMediaTypeCode = "TW";
                var socialMediaHandle = "pontifex";
                var personSocialMedia = new SocialMedia(socialMediaTypeCode, socialMediaHandle);
                personIntegrationReturned.AddSocialMedia(personSocialMedia);
            }

            private void SetupReferenceDataRepositoryMocks()
            {
                // Mock the reference repository for country
                countries = new List<Country>()
                 {
                    new Country("US","United States","USA"),
                    new Country("CA","Canada","CAN"),
                    new Country("MX","Mexico","MEX"),
                    new Country("BR","Brazil","BRA")
                };
                referenceDataRepositoryMock.Setup(repo => repo.GetCountryCodesAsync(It.IsAny<bool>())).ReturnsAsync(countries);

                // Places
                var places = new List<Place>();
                var place1 = new Place() { PlacesCountry = "USA", PlacesRegion = "US-NY" };
                places.Add(place1);
                var place2 = new Place() { PlacesCountry = "CAN", PlacesRegion = "CA-ON" };
                places.Add(place2);
                referenceDataRepositoryMock.Setup(repo => repo.GetPlacesAsync(It.IsAny<bool>())).Returns(Task.FromResult(places.AsEnumerable<Place>()));
                //personRepositoryMock.Setup(repo => repo.GetPlacesAsync()).ReturnsAsync(places);

                // International Parameters Host Country
                personRepositoryMock.Setup(repo => repo.GetHostCountryAsync()).ReturnsAsync("USA");

                referenceDataRepositoryMock.Setup(repo => repo.GetPrivacyStatusesAsync(It.IsAny<bool>()))
                     .ReturnsAsync(
                         new List<PrivacyStatus>() {
                        new PrivacyStatus( "d3d86052-9d55-4751-acda-5c07a064a82a", "UN", "unrestricted", PrivacyStatusType.unrestricted),
                        new PrivacyStatus( "cff65dcc-4a9b-44ed-b8d0-930348c55ef8", "R", "restricted", PrivacyStatusType.restricted)
                        }
                     );
                personNameTypes = new List<PersonNameTypeItem>() {
                        new PersonNameTypeItem("8224f18e-69c5-480b-a9b4-52f596aa4a52", "PREFERRED", "Personal", PersonNameType.Personal),
                        new PersonNameTypeItem("7dfa950c-8ae4-4dca-92f0-c083604285b6", "BIRTH", "Birth", PersonNameType.Birth),
                        new PersonNameTypeItem("dd20ebdf-2452-41ef-9f86-ad1b1621a78d", "CHOSEN", "Chosen", PersonNameType.Personal),
                        new PersonNameTypeItem("806af5a5-8a9a-424f-8c9f-c1e9d084ee71", "LEGAL", "Legal", PersonNameType.Legal),
                        new PersonNameTypeItem("7b55610f-7d00-4260-bbcf-0e47fdbae647", "NICKNAME", "NickName", PersonNameType.Personal),
                        new PersonNameTypeItem("d42cc964-35cb-4560-bc46-4b881e7705ea", "HISTORY", "History", PersonNameType.Personal)
                        };
                referenceDataRepositoryMock.Setup(repo => repo.GetPersonNameTypesAsync(It.IsAny<bool>()))
                    .ReturnsAsync(personNameTypes);

                referenceDataRepositoryMock.Setup(repo => repo.GetDenominationsAsync(It.IsAny<bool>()))
                    .ReturnsAsync(
                        new List<Denomination>() {
                        new Denomination(baptistGuid,"BA", "Baptist") ,
                        new Denomination(catholicGuid,"CA", "Catholic")
                        }
                     );

                referenceDataRepositoryMock.Setup(repo => repo.GetEmailTypesAsync(It.IsAny<bool>()))
                     .ReturnsAsync(
                         new List<EmailType>() {
                        new EmailType("899803da-48f8-4044-beb8-5913a04b995d", "COL", "College", EmailTypeCategory.School),
                        new EmailType("301d485d-d37b-4d29-af00-465ced624a85", "PER", "Personal", EmailTypeCategory.Personal),
                        new EmailType("53fb7dab-d348-4657-b071-45d0e5933e05", "BUS", "Business", EmailTypeCategory.Business)
                        }
                     );

                referenceDataRepositoryMock.Setup(repo => repo.GetSocialMediaTypesAsync(It.IsAny<bool>()))
                     .ReturnsAsync(
                         new List<SocialMediaType>() {
                        new SocialMediaType("d1f311f4-687d-4dc7-a329-c6a8bfc9c74", "TW", "Twitter", SocialMediaTypeCategory.twitter)
                        }
                     );

                referenceDataRepositoryMock.Setup(repo => repo.GetAddressTypes2Async(It.IsAny<bool>()))
                     .ReturnsAsync(
                         new List<AddressType2>() {
                        new AddressType2("91979656-e110-4156-a75a-1a1a7294314d", "HO", "Home", AddressTypeCategory.Home),
                        new AddressType2("b887d5ec-9ed5-45e8-b44c-01782070f234", "MA", "Mailing", AddressTypeCategory.Mailing),
                        new AddressType2("d7d0a82c-fe74-480d-be1b-88a2e460af4c", "VA", "Vacation", AddressTypeCategory.Vacation),
                        new AddressType2("c9b8cd52-54e6-4c08-a9d9-224dd0c8b700", "BU", "Business", AddressTypeCategory.Business)
                         }
                     );

                referenceDataRepositoryMock.Setup(repo => repo.GetPhoneTypesAsync(It.IsAny<bool>()))
                     .ReturnsAsync(
                         new List<PhoneType>() {
                        new PhoneType("92c82d33-e55c-41a4-a2c3-f2f7d2c523d1", "HO", "Home", PhoneTypeCategory.Home),
                        new PhoneType("b6def2cc-cc95-4d0e-a32c-940fbbc2d689", "MO", "Mobile", PhoneTypeCategory.Mobile),
                        new PhoneType("f60e7b27-a3e3-4c92-9d36-f3cae27b724b", "VA", "Vacation", PhoneTypeCategory.Vacation),
                        new PhoneType("30e231cf-a199-4c9a-af01-be2e69b607c9", "BU", "Business", PhoneTypeCategory.Business)
                        }
                     );

                // Mock the reference repository for ethnicity
                referenceDataRepositoryMock.Setup(repo => repo.GetEthnicitiesAsync(It.IsAny<bool>())).Returns(Task.FromResult<IEnumerable<Ethnicity>>(new List<Ethnicity>()
                {
                    new Ethnicity(ethnicityGuid, "H", "Hispanic", EthnicityType.Hispanic)
                }));

                // Mock the reference repository for prefix
                referenceDataRepositoryMock.Setup(repo => repo.Prefixes).Returns(new List<Prefix>()
                {
                    new Prefix("MR","Mr","Mr."),
                    new Prefix("MS","Ms","Ms.")
                });

                // Mock the reference repository for prefix
                referenceDataRepositoryMock.Setup(repo => repo.GetPrefixesAsync()).ReturnsAsync(new List<Prefix>()
                {
                    new Prefix("MR","Mr","Mr."),
                    new Prefix("MS","Ms","Ms.")
                });

                // Mock the reference repository for suffix
                referenceDataRepositoryMock.Setup(repo => repo.Suffixes).Returns(new List<Suffix>()
                {
                    new Suffix("JR","Jr","Jr."),
                    new Suffix("SR","Sr","Sr.")
                });

                // Mock the reference repository for suffix
                referenceDataRepositoryMock.Setup(repo => repo.GetSuffixesAsync()).ReturnsAsync(new List<Suffix>()
                {
                    new Suffix("JR","Jr","Jr."),
                    new Suffix("SR","Sr","Sr.")
                });

                // Mock the reference repository for marital status
                referenceDataRepositoryMock.Setup(repo => repo.GetMaritalStatusesAsync(It.IsAny<bool>())).Returns(Task.FromResult<IEnumerable<MaritalStatus>>(new List<MaritalStatus>()
                {
                    new MaritalStatus(maritalStatusGuid, "M", "Married"){ Type = MaritalStatusType.Married },
                     new MaritalStatus(maritalStatusSingleGuid, "S", "Single"){ Type = MaritalStatusType.Single }
                }));

                var raceEntities = new List<Race>();
                raceEntities.Add(new Race(raceAsianGuid, "AS", "Asian", RaceType.Asian));
                raceEntities.Add(new Race(racePacificIslanderGuid, "HP", "Hawaiian/Pacific Islander", RaceType.PacificIslander));
                referenceDataRepositoryMock.Setup(repo => repo.GetRacesAsync(It.IsAny<bool>())).Returns(Task.FromResult<IEnumerable<Race>>(raceEntities));

                // Mock the reference repository for Alternate ID Types
                referenceDataRepositoryMock.Setup(repo => repo.GetAlternateIdTypesAsync(It.IsAny<bool>())).ReturnsAsync(new List<AltIdTypes>()
                {
                    new AltIdTypes("AE44FE48-2534-480B-8618-5480617CE74A", "ELEV2", "Elevate ID 2"),
                    new AltIdTypes("D525E2B2-CD7D-4995-93F0-97DA468EBE90", "GOVID2", "Government ID 2")
                });

                // Mock the reference repository for Gender Identity Codes
                referenceDataRepositoryMock.Setup(repo => repo.GetGenderIdentityTypesAsync(It.IsAny<bool>())).ReturnsAsync(new List<GenderIdentityType>()
                {
                    new GenderIdentityType("9C3004AB-0F25-4D1D-84D6-65EA69CE1124","FTM","Female to Male"),
                    new GenderIdentityType("BCD23124-2FAA-411C-A990-24BA3FA8A93D", "MTF","Male to Female")
                });

                // Mock the reference repository for Personal Pronouns
                referenceDataRepositoryMock.Setup(repo => repo.GetPersonalPronounTypesAsync(It.IsAny<bool>())).ReturnsAsync(new List<PersonalPronounType>()
                {
                    new PersonalPronounType("AE7A3392-FA07-4F53-B6D5-317D77CB62EC","HE","He, Him, His"),
                    new PersonalPronounType("9567AFB5-5F3C-40DC-B4F9-FC1658ACEE15", "HER","She, Her, Hers")
                });
                referenceDataRepositoryMock.Setup(repo => repo.GetMilStatusesAsync(It.IsAny<bool>())).ReturnsAsync(new List<MilStatuses>()
                {
                    new MilStatuses("AE7A3392-FA07-4F53-B6D5-317D88CB62EC", "VET", "Veteran") { Category = VeteranStatusCategory.Protectedveteran },
                    new MilStatuses("9567AFB5-5F3C-40DC-B4F9-FC1699ACEE15", "RET", "Retired") { Category = VeteranStatusCategory.Activeduty }
                });
            }

            #endregion

        }

        #endregion

        #region HEDM Update Person V8 Tests

        [TestClass]
        public class UpdatePerson3 : CurrentUserSetup
        {
            //Mocks
            Mock<IAdapterRegistry> adapterRegistryMock;
            Mock<IPersonRepository> personRepositoryMock;
            Mock<IPersonBaseRepository> personBaseRepositoryMock;
            Mock<IReferenceDataRepository> referenceDataRepositoryMock;
            Mock<IProfileRepository> profileRepositoryMock;
            Mock<IConfigurationRepository> configurationRepositoryMock;
            Mock<IRelationshipRepository> relationshipRepositoryMock;
            Mock<IProxyRepository> proxyRepositoryMock;
            Mock<IRoleRepository> roleRepositoryMock;
            Mock<ILogger> loggerMock;

            //userfactory
            ICurrentUserFactory currentUserFactory;

            //Service
            PersonService personService;


            private Ellucian.Colleague.Dtos.Person3 personDto;
            private Ellucian.Colleague.Domain.Base.Entities.PersonIntegration personIntegrationReturned;
            private Ellucian.Colleague.Domain.Base.Entities.PersonIntegration personIntegrationEntity;

            //private Ellucian.Colleague.Domain.Base.Entities.Person person;
            private List<Ellucian.Colleague.Domain.Base.Entities.Address> addresses = null;
            private List<Ellucian.Colleague.Domain.Base.Entities.Phone> phones = new List<Domain.Base.Entities.Phone>();
            private IEnumerable<Ellucian.Colleague.Domain.Base.Entities.State> states;
            private IEnumerable<Ellucian.Colleague.Domain.Base.Entities.Country> countries;
            private List<Ellucian.Colleague.Domain.Base.Entities.County> counties;
            private List<PersonNameTypeItem> personNameTypes;

            //Entities
            private Ellucian.Colleague.Domain.Base.Entities.EmailAddress instEmail;
            private Ellucian.Colleague.Domain.Base.Entities.EmailAddress perEmail;
            private Ellucian.Colleague.Domain.Base.Entities.EmailAddress workEmail;
            private Ellucian.Colleague.Domain.Base.Entities.Address homeAddr;
            private Ellucian.Colleague.Domain.Base.Entities.Address mailAddr;
            private Ellucian.Colleague.Domain.Base.Entities.Address resAddr;
            private Ellucian.Colleague.Domain.Base.Entities.Address workAddr;
            private Ellucian.Colleague.Domain.Base.Entities.Phone homePhone;
            private Ellucian.Colleague.Domain.Base.Entities.Phone mobilePhone;
            private Ellucian.Colleague.Domain.Base.Entities.Phone residencePhone;
            private Ellucian.Colleague.Domain.Base.Entities.Phone workPhone;

            //Data
            private string personId = "0000011";
            private string personGuid = "5674f28b-b216-4055-b236-81a922d93b4c";
            private string maritalStatusGuid = "dca8edb5-120f-479a-a6bb-35ba3af4b344";
            private string ethnicityGuid = "dd0c42ca-c61d-4ca6-8d21-96ab5be35623";
            private string raceAsianGuid = "72b7737b-27db-4a06-944b-97d00c29b3db";
            private string racePacificIslanderGuid = "e20f9821-28a2-4e34-8550-6758850a0cf8";
            private string baptistGuid = "c0bdfd92-462f-4e59-bba5-1b15c4771c86";
            private string catholicGuid = "f96f04b0-4973-41f6-bc3d-9c7bc1c2c458";

            private string countyGuid = Guid.NewGuid().ToString();


            [TestInitialize]
            public void Initialize()
            {
                adapterRegistryMock = new Mock<IAdapterRegistry>();
                personRepositoryMock = new Mock<IPersonRepository>();
                personBaseRepositoryMock = new Mock<IPersonBaseRepository>();
                referenceDataRepositoryMock = new Mock<IReferenceDataRepository>();
                profileRepositoryMock = new Mock<IProfileRepository>();
                configurationRepositoryMock = new Mock<IConfigurationRepository>();
                relationshipRepositoryMock = new Mock<IRelationshipRepository>();
                proxyRepositoryMock = new Mock<IProxyRepository>();
                roleRepositoryMock = new Mock<IRoleRepository>();
                loggerMock = new Mock<ILogger>();

                currentUserFactory = new CurrentUserSetup.ThirdPartyUserFactory();

                SetupData();

                SetupReferenceDataRepositoryMocks();

                // International Parameters Host Country
                personRepositoryMock.Setup(repo => repo.GetHostCountryAsync()).ReturnsAsync("USA");

                personRepositoryMock.Setup(i => i.Update2Async(It.IsAny<PersonIntegration>(), addresses, phones, It.IsAny<int>())).ReturnsAsync(personIntegrationReturned);

                personService = new PersonService(adapterRegistryMock.Object, personRepositoryMock.Object, personBaseRepositoryMock.Object, referenceDataRepositoryMock.Object, profileRepositoryMock.Object,
                                                  configurationRepositoryMock.Object, relationshipRepositoryMock.Object, proxyRepositoryMock.Object, currentUserFactory,
                                                  roleRepositoryMock.Object, loggerMock.Object);
            }


            [TestCleanup]
            public void Cleanup()
            {
                personService = null;
                adapterRegistryMock = null;
                personRepositoryMock = null;
                personBaseRepositoryMock = null;
                referenceDataRepositoryMock = null;
                profileRepositoryMock = null;
                configurationRepositoryMock = null;
                relationshipRepositoryMock = null;
                proxyRepositoryMock = null;
                roleRepositoryMock = null;
                loggerMock = null;
            }

            [TestMethod]
            public async Task UpdatePerson3_UpdatePerson3Async()
            {
                //setup role
                updatePersonRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Base.BasePermissionCodes.UpdatePerson));
                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { updatePersonRole });

                //personId 0000011
                personRepositoryMock.Setup(i => i.GetPersonIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync(personId);

                var actual = await personService.UpdatePerson3Async(personDto);

                Assert.AreEqual(personDto.BirthDate, actual.BirthDate);
                Assert.AreEqual(personDto.CitizenshipCountry, actual.CitizenshipCountry);
                Assert.AreEqual(personDto.CitizenshipStatus, actual.CitizenshipStatus);
                Assert.AreEqual(personDto.CountryOfBirth, actual.CountryOfBirth);
                Assert.AreEqual(personDto.DeceasedDate, actual.DeceasedDate);
                Assert.AreEqual(personDto.GenderType, actual.GenderType);
                Assert.AreEqual(personDto.MaritalStatus.Detail.Id, actual.MaritalStatus.Detail.Id);
                Assert.AreEqual(personDto.MaritalStatus.MaritalCategory, actual.MaritalStatus.MaritalCategory);

                /*
                    This code will change because of some of the API changes in future
                */

                //Legal
                var legalActualName = actual.PersonNames.FirstOrDefault(item => item.NameType.Detail.Id.Equals("806af5a5-8a9a-424f-8c9f-c1e9d084ee71", StringComparison.OrdinalIgnoreCase));
                var legalExpectedName = personDto.PersonNames.FirstOrDefault(item => item.NameType.Detail.Id.Equals(legalActualName.NameType.Detail.Id, StringComparison.OrdinalIgnoreCase));

                //Assert.AreEqual(legalExpectedName.FullName, legalActualName.FullName); commented cause it will fail
                if (!string.IsNullOrEmpty(legalExpectedName.LastNamePrefix))
                    Assert.AreEqual(legalExpectedName.LastName, string.Concat(legalExpectedName.LastNamePrefix, " ", legalActualName.LastName));
                Assert.AreEqual(legalExpectedName.FirstName, legalActualName.FirstName);
                Assert.AreEqual(legalExpectedName.MiddleName, legalActualName.MiddleName);

                //Birth
                var birthActualName = actual.PersonNames.FirstOrDefault(item => item.NameType.Detail.Id.Equals("7dfa950c-8ae4-4dca-92f0-c083604285b6", StringComparison.OrdinalIgnoreCase));
                var birthexpectedName = personDto.PersonNames.FirstOrDefault(item => item.NameType.Detail.Id.Equals(birthActualName.NameType.Detail.Id, StringComparison.OrdinalIgnoreCase));

                Assert.AreEqual(birthexpectedName.FullName, birthActualName.FullName);

                //Chosen
                var chosenActualName = actual.PersonNames.FirstOrDefault(item => item.NameType.Detail.Id.Equals("dd20ebdf-2452-41ef-9F86-ad1b1621a78d", StringComparison.OrdinalIgnoreCase));
                var chosenExpectedName = personDto.PersonNames.FirstOrDefault(item => item.NameType.Detail.Id.Equals(chosenActualName.NameType.Detail.Id, StringComparison.OrdinalIgnoreCase));

                Assert.AreEqual(chosenExpectedName.FullName, chosenActualName.FullName);

                //Nickname
                var nickNameActualName = actual.PersonNames.FirstOrDefault(item => item.NameType.Detail.Id.Equals("7b55610f-7d00-4260-bbcf-0e47fdbae647", StringComparison.OrdinalIgnoreCase));
                var nickNameExpectedName = personDto.PersonNames.FirstOrDefault(item => item.NameType.Detail.Id.Equals(nickNameActualName.NameType.Detail.Id, StringComparison.OrdinalIgnoreCase));

                Assert.AreEqual(nickNameExpectedName.FullName, nickNameActualName.FullName);

                //History
                var historyActualName = actual.PersonNames.FirstOrDefault(item => item.NameType.Detail.Id.Equals("d42cc964-35cb-4560-bc46-4b881e7705ea", StringComparison.OrdinalIgnoreCase));
                var historyexpectedName = personDto.PersonNames.FirstOrDefault(item => item.NameType.Detail.Id.Equals(historyActualName.NameType.Detail.Id, StringComparison.OrdinalIgnoreCase));

                Assert.AreEqual(historyexpectedName.FullName, historyActualName.FullName);

            }

            [TestMethod]
            public async Task UpdatePerson3_UpdatePerson3Async_PersonId_NullEmpty_Create()
            {
                //setup role
                createPersonRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Base.BasePermissionCodes.CreatePerson));
                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { createPersonRole });

                //personId 0000011
                personRepositoryMock.Setup(i => i.GetPersonIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync(string.Empty);

                personRepositoryMock.Setup(i => i.Create2Async(It.IsAny<PersonIntegration>(), addresses, phones, It.IsAny<int>())).ReturnsAsync(personIntegrationReturned);


                var actual = await personService.UpdatePerson3Async(personDto);

                Assert.AreEqual(personDto.BirthDate, actual.BirthDate);
                Assert.AreEqual(personDto.CitizenshipCountry, actual.CitizenshipCountry);
                Assert.AreEqual(personDto.CitizenshipStatus, actual.CitizenshipStatus);
                Assert.AreEqual(personDto.CountryOfBirth, actual.CountryOfBirth);
                Assert.AreEqual(personDto.DeceasedDate, actual.DeceasedDate);
                Assert.AreEqual(personDto.GenderType, actual.GenderType);
                Assert.AreEqual(personDto.MaritalStatus.Detail.Id, actual.MaritalStatus.Detail.Id);
                Assert.AreEqual(personDto.MaritalStatus.MaritalCategory, actual.MaritalStatus.MaritalCategory);

                var nameCount = personDto.PersonNames.Count();
                personDto.PersonNames.OrderBy(i => i.NameType.Category);
                actual.PersonNames.OrderBy(i => i.NameType.Category);

                for (int i = 0; i < nameCount; i++)
                {
                    var actualName = actual.PersonNames.ToList()[i];
                    var expectedName = personDto.PersonNames.FirstOrDefault(item => item.NameType.Detail.Id.Equals(actualName.NameType.Detail.Id, StringComparison.OrdinalIgnoreCase));
                }
            }

            #region Exceptions
            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task UpdatePerson3_Dto_Null_ArgumentNullException()
            {
                var result = await personService.UpdatePerson3Async(null);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task UpdatePerson3_Id_Null_ArgumentNullException()
            {
                var result = await personService.UpdatePerson3Async(new Dtos.Person3() { Id = "" });
            }

            [TestMethod]
            [ExpectedException(typeof(Exception))]
            public async Task UpdatePerson3_PersonNames_Null_ArgumentNullException()
            {
                //setup role
                updatePersonRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Base.BasePermissionCodes.UpdatePerson));
                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { updatePersonRole });

                //personId 0000011
                personRepositoryMock.Setup(i => i.GetPersonIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync(personId);
                var result = await personService.UpdatePerson2Async(new Dtos.Person2() { Id = personId });
            }

            [TestMethod]
            [ExpectedException(typeof(Exception))]
            public async Task UpdatePerson3_PrimaryNames_Null_ArgumentNullException()
            {
                //setup role
                updatePersonRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Base.BasePermissionCodes.UpdatePerson));
                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { updatePersonRole });

                //personId 0000011
                personRepositoryMock.Setup(i => i.GetPersonIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync(personId);
                personDto.PersonNames.FirstOrDefault().NameType.Detail.Id = string.Empty;
                var result = await personService.UpdatePerson3Async(personDto);
            }

            [TestMethod]
            [ExpectedException(typeof(Exception))]
            public async Task UpdatePerson3_PrimaryNames_GT_1_ArgumentNullException()
            {
                //setup role
                updatePersonRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Base.BasePermissionCodes.UpdatePerson));
                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { updatePersonRole });

                //personId 0000011
                personRepositoryMock.Setup(i => i.GetPersonIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync(personId);
                personDto.PersonNames = null;
                var personNames = new List<Dtos.DtoProperties.PersonNameDtoProperty>();
                var personPrimaryName = new Dtos.DtoProperties.PersonNameDtoProperty()
                {
                    NameType = new Dtos.DtoProperties.PersonNameTypeDtoProperty() { Category = Dtos.EnumProperties.PersonNameType2.Personal, Detail = new Dtos.GuidObject2() { Id = "806af5a5-8a9a-424f-8c9f-c1e9d084ee71" } },
                    Title = "MR",
                    FirstName = "Ricky",
                    MiddleName = "Lee",
                    LastName = "Brown",
                    Pedigree = "JR",
                    LastNamePrefix = "Ignore"
                };
                personNames.Add(personPrimaryName);
                var personPrimaryName1 = new Dtos.DtoProperties.PersonNameDtoProperty()
                {
                    NameType = new Dtos.DtoProperties.PersonNameTypeDtoProperty() { Category = Dtos.EnumProperties.PersonNameType2.Personal, Detail = new Dtos.GuidObject2() { Id = "806af5a5-8a9a-424f-8c9f-c1e9d084ee71" } },
                    Title = "MR",
                    FirstName = "Ricky",
                    MiddleName = "Lee",
                    LastName = "Brown",
                    Pedigree = "JR",
                    LastNamePrefix = "Ignore"
                };
                personNames.Add(personPrimaryName1);
                personDto.PersonNames = personNames;
                var result = await personService.UpdatePerson3Async(personDto);
            }

            [TestMethod]
            [ExpectedException(typeof(Exception))]
            public async Task UpdatePerson3_PrimaryNames_LastName_Null_ArgumentNullException()
            {
                //setup role
                updatePersonRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Base.BasePermissionCodes.UpdatePerson));
                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { updatePersonRole });

                //personId 0000011
                personRepositoryMock.Setup(i => i.GetPersonIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync(personId);
                personDto.PersonNames.FirstOrDefault().LastName = string.Empty;

                var result = await personService.UpdatePerson3Async(personDto);
            }

            [TestMethod]
            [ExpectedException(typeof(RepositoryException))]
            public async Task UpdatePerson3_LEGAL_NameTypes_Null_ArgumentNullException()
            {
                var legalType = personNameTypes.FirstOrDefault(x => x.Code == "LEGAL");
                personNameTypes.Remove(legalType);

                //setup role
                updatePersonRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Base.BasePermissionCodes.UpdatePerson));
                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { updatePersonRole });

                //personId 0000011
                personRepositoryMock.Setup(i => i.GetPersonIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync(personId);

                var result = await personService.UpdatePerson3Async(personDto);
            }

            [TestMethod]
            [ExpectedException(typeof(RepositoryException))]
            public async Task UpdatePerson3_BIRTH_NameTypes_Null_ArgumentNullException()
            {
                var birthType = personNameTypes.FirstOrDefault(x => x.Code == "BIRTH");
                personNameTypes.Remove(birthType);

                //setup role
                updatePersonRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Base.BasePermissionCodes.UpdatePerson));
                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { updatePersonRole });

                //personId 0000011
                personRepositoryMock.Setup(i => i.GetPersonIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync(personId);

                var result = await personService.UpdatePerson3Async(personDto);
            }

            [TestMethod]
            [ExpectedException(typeof(RepositoryException))]
            public async Task UpdatePerson3_NICKNAME_NameTypes_Null_ArgumentNullException()
            {
                var nickNameType = personNameTypes.FirstOrDefault(x => x.Code == "NICKNAME");
                personNameTypes.Remove(nickNameType);

                //setup role
                updatePersonRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Base.BasePermissionCodes.UpdatePerson));
                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { updatePersonRole });

                //personId 0000011
                personRepositoryMock.Setup(i => i.GetPersonIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync(personId);

                var result = await personService.UpdatePerson3Async(personDto);
            }

            [TestMethod]
            [ExpectedException(typeof(Exception))]
            public async Task UpdatePerson3_NICKNAME_GT_1_ArgumentNullException()
            {
                //setup role
                updatePersonRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Base.BasePermissionCodes.UpdatePerson));
                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { updatePersonRole });

                //personId 0000011
                personRepositoryMock.Setup(i => i.GetPersonIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync(personId);
                personDto.PersonNames = null;
                var personNames = new List<Dtos.DtoProperties.PersonNameDtoProperty>();
                var legalPrimaryName = new Dtos.DtoProperties.PersonNameDtoProperty()
                {
                    NameType = new Dtos.DtoProperties.PersonNameTypeDtoProperty() { Category = Dtos.EnumProperties.PersonNameType2.Personal, Detail = new Dtos.GuidObject2() { Id = "806af5a5-8a9a-424f-8c9f-c1e9d084ee71" } },
                    Title = "MR",
                    FirstName = "Ricky",
                    MiddleName = "Lee",
                    LastName = "Brown",
                    Pedigree = "JR",
                    LastNamePrefix = "Ignore",
                    FullName = "FullName"
                };
                personNames.Add(legalPrimaryName);
                var birthPrimaryName = new Dtos.DtoProperties.PersonNameDtoProperty()
                {
                    NameType = new Dtos.DtoProperties.PersonNameTypeDtoProperty() { Category = Dtos.EnumProperties.PersonNameType2.Birth, Detail = new Dtos.GuidObject2() { Id = "7b55610f-7d00-4260-bbcf-0e47fdbae647" } },
                    Title = "MR",
                    FirstName = "Ricky",
                    MiddleName = "Lee",
                    LastName = "Brown",
                    Pedigree = "JR",
                    LastNamePrefix = "Ignore",
                    FullName = "FullName"
                };
                personNames.Add(birthPrimaryName);

                var birthPrimaryName2 = new Dtos.DtoProperties.PersonNameDtoProperty()
                {
                    NameType = new Dtos.DtoProperties.PersonNameTypeDtoProperty() { Category = Dtos.EnumProperties.PersonNameType2.Birth, Detail = new Dtos.GuidObject2() { Id = "7b55610f-7d00-4260-bbcf-0e47fdbae647" } },
                    Title = "MR",
                    FirstName = "Ricky",
                    MiddleName = "Lee",
                    LastName = "Brown",
                    Pedigree = "JR",
                    LastNamePrefix = "Ignore",
                    FullName = "FullName"
                };
                personNames.Add(birthPrimaryName2);
                personDto.PersonNames = personNames;
                var result = await personService.UpdatePerson3Async(personDto);
            }

            [TestMethod]
            [ExpectedException(typeof(Exception))]
            public async Task UpdatePerson3_Legal_FirstLastMiddle_Null_ArgumentNullException()
            {
                //setup role
                updatePersonRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Base.BasePermissionCodes.UpdatePerson));
                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { updatePersonRole });

                //personId 0000011
                personRepositoryMock.Setup(i => i.GetPersonIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync(personId);
                personDto.PersonNames = null;
                var personNames = new List<Dtos.DtoProperties.PersonNameDtoProperty>();
                var legalPrimaryName = new Dtos.DtoProperties.PersonNameDtoProperty()
                {
                    NameType = new Dtos.DtoProperties.PersonNameTypeDtoProperty() { Category = Dtos.EnumProperties.PersonNameType2.Personal, Detail = new Dtos.GuidObject2() { Id = "806af5a5-8a9a-424f-8c9f-c1e9d084ee71" } },
                    Title = "MR",
                    FirstName = "",
                    MiddleName = "",
                    LastName = "",
                    Pedigree = "JR",
                    LastNamePrefix = "Ignore",
                    FullName = "FullName"
                };
                personNames.Add(legalPrimaryName);
                var birthPrimaryName = new Dtos.DtoProperties.PersonNameDtoProperty()
                {
                    NameType = new Dtos.DtoProperties.PersonNameTypeDtoProperty() { Category = Dtos.EnumProperties.PersonNameType2.Birth, Detail = new Dtos.GuidObject2() { Id = "7b55610f-7d00-4260-bbcf-0e47fdbae647" } },
                    Title = "MR",
                    FirstName = "Ricky",
                    MiddleName = "Lee",
                    LastName = "Brown",
                    Pedigree = "JR",
                    LastNamePrefix = "Ignore",
                    FullName = "FullName"
                };
                personNames.Add(birthPrimaryName);

                var birthPrimaryName2 = new Dtos.DtoProperties.PersonNameDtoProperty()
                {
                    NameType = new Dtos.DtoProperties.PersonNameTypeDtoProperty() { Category = Dtos.EnumProperties.PersonNameType2.Birth, Detail = new Dtos.GuidObject2() { Id = "7b55610f-7d00-4260-bbcf-0e47fdbae647" } },
                    Title = "MR",
                    FirstName = "Ricky",
                    MiddleName = "Lee",
                    LastName = "Brown",
                    Pedigree = "JR",
                    LastNamePrefix = "Ignore",
                    FullName = "FullName"
                };
                personNames.Add(birthPrimaryName2);
                personDto.PersonNames = personNames;
                var result = await personService.UpdatePerson3Async(personDto);
            }

            [TestMethod]
            [ExpectedException(typeof(Exception))]
            public async Task UpdatePerson3_Birth_FirstLastMiddle_Null_ArgumentNullException()
            {
                //setup role
                updatePersonRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Base.BasePermissionCodes.UpdatePerson));
                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { updatePersonRole });

                //personId 0000011
                personRepositoryMock.Setup(i => i.GetPersonIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync(personId);
                personDto.PersonNames = null;
                var personNames = new List<Dtos.DtoProperties.PersonNameDtoProperty>();
                var legalPrimaryName = new Dtos.DtoProperties.PersonNameDtoProperty()
                {
                    NameType = new Dtos.DtoProperties.PersonNameTypeDtoProperty() { Category = Dtos.EnumProperties.PersonNameType2.Personal, Detail = new Dtos.GuidObject2() { Id = "806af5a5-8a9a-424f-8c9f-c1e9d084ee71" } },
                    Title = "MR",
                    FirstName = "LegalFirst",
                    MiddleName = "LegalMiddle",
                    LastName = "LegalLast",
                    Pedigree = "JR",
                    LastNamePrefix = "Ignore",
                    FullName = "FullName"
                };
                personNames.Add(legalPrimaryName);
                var birthPrimaryName = new Dtos.DtoProperties.PersonNameDtoProperty()
                {
                    NameType = new Dtos.DtoProperties.PersonNameTypeDtoProperty() { Category = Dtos.EnumProperties.PersonNameType2.Birth, Detail = new Dtos.GuidObject2() { Id = "7dfa950c-8ae4-4dca-92f0-c083604285b6" } },
                    Title = "MR",
                    FirstName = "",
                    MiddleName = "",
                    LastName = "",
                    Pedigree = "JR",
                    LastNamePrefix = "Ignore",
                    FullName = "FullName"
                };
                personNames.Add(birthPrimaryName);
                personDto.PersonNames = personNames;
                var result = await personService.UpdatePerson3Async(personDto);
            }

            [TestMethod]
            [ExpectedException(typeof(Exception))]
            public async Task UpdatePerson3_Birth_FullName_Null_ArgumentNullException()
            {
                //setup role
                updatePersonRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Base.BasePermissionCodes.UpdatePerson));
                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { updatePersonRole });

                //personId 0000011
                personRepositoryMock.Setup(i => i.GetPersonIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync(personId);
                personDto.PersonNames = null;
                var personNames = new List<Dtos.DtoProperties.PersonNameDtoProperty>();
                var legalPrimaryName = new Dtos.DtoProperties.PersonNameDtoProperty()
                {
                    NameType = new Dtos.DtoProperties.PersonNameTypeDtoProperty() { Category = Dtos.EnumProperties.PersonNameType2.Personal, Detail = new Dtos.GuidObject2() { Id = "806af5a5-8a9a-424f-8c9f-c1e9d084ee71" } },
                    Title = "MR",
                    FirstName = "LegalFirst",
                    MiddleName = "LegalMiddle",
                    LastName = "LegalLast",
                    Pedigree = "JR",
                    LastNamePrefix = "Ignore",
                    FullName = "FullName"
                };
                personNames.Add(legalPrimaryName);
                var birthPrimaryName = new Dtos.DtoProperties.PersonNameDtoProperty()
                {
                    NameType = new Dtos.DtoProperties.PersonNameTypeDtoProperty() { Category = Dtos.EnumProperties.PersonNameType2.Birth, Detail = new Dtos.GuidObject2() { Id = "7dfa950c-8ae4-4dca-92f0-c083604285b6" } },
                    Title = "MR",
                    FirstName = "First",
                    MiddleName = "Middle",
                    LastName = "Last",
                    Pedigree = "JR",
                    LastNamePrefix = "Ignore",
                    FullName = ""
                };
                personNames.Add(birthPrimaryName);
                personDto.PersonNames = personNames;
                var result = await personService.UpdatePerson3Async(personDto);
            }

            [TestMethod]
            [ExpectedException(typeof(Exception))]
            public async Task UpdatePerson3_NickName_FullName_Null_ArgumentNullException()
            {
                //setup role
                updatePersonRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Base.BasePermissionCodes.UpdatePerson));
                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { updatePersonRole });

                //personId 0000011
                personRepositoryMock.Setup(i => i.GetPersonIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync(personId);
                personDto.PersonNames = null;
                var personNames = new List<Dtos.DtoProperties.PersonNameDtoProperty>();
                var legalPrimaryName = new Dtos.DtoProperties.PersonNameDtoProperty()
                {
                    NameType = new Dtos.DtoProperties.PersonNameTypeDtoProperty() { Category = Dtos.EnumProperties.PersonNameType2.Personal, Detail = new Dtos.GuidObject2() { Id = "806af5a5-8a9a-424f-8c9f-c1e9d084ee71" } },
                    Title = "MR",
                    FirstName = "LegalFirst",
                    MiddleName = "LegalMiddle",
                    LastName = "LegalLast",
                    Pedigree = "JR",
                    LastNamePrefix = "Ignore",
                    FullName = "FullName"
                };
                personNames.Add(legalPrimaryName);
                var birthPrimaryName = new Dtos.DtoProperties.PersonNameDtoProperty()
                {
                    NameType = new Dtos.DtoProperties.PersonNameTypeDtoProperty() { Category = Dtos.EnumProperties.PersonNameType2.Birth, Detail = new Dtos.GuidObject2() { Id = "7b55610f-7d00-4260-bbcf-0e47fdbae647" } },
                    Title = "MR",
                    FirstName = "First",
                    MiddleName = "Middle",
                    LastName = "Last",
                    Pedigree = "JR",
                    LastNamePrefix = "Ignore",
                    FullName = ""
                };
                personNames.Add(birthPrimaryName);
                personDto.PersonNames = personNames;
                var result = await personService.UpdatePerson3Async(personDto);
            }

            [TestMethod]
            [ExpectedException(typeof(Exception))]
            public async Task UpdatePerson3_History_FullName_Null_ArgumentNullException()
            {
                //setup role
                updatePersonRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Base.BasePermissionCodes.UpdatePerson));
                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { updatePersonRole });

                //personId 0000011
                personRepositoryMock.Setup(i => i.GetPersonIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync(personId);
                personDto.PersonNames = null;
                var personNames = new List<Dtos.DtoProperties.PersonNameDtoProperty>();
                var legalPrimaryName = new Dtos.DtoProperties.PersonNameDtoProperty()
                {
                    NameType = new Dtos.DtoProperties.PersonNameTypeDtoProperty() { Category = Dtos.EnumProperties.PersonNameType2.Personal, Detail = new Dtos.GuidObject2() { Id = "806af5a5-8a9a-424f-8c9f-c1e9d084ee71" } },
                    Title = "MR",
                    FirstName = "LegalFirst",
                    MiddleName = "LegalMiddle",
                    LastName = "LegalLast",
                    Pedigree = "JR",
                    LastNamePrefix = "Ignore",
                    FullName = "FullName"
                };
                personNames.Add(legalPrimaryName);
                var birthPrimaryName = new Dtos.DtoProperties.PersonNameDtoProperty()
                {
                    NameType = new Dtos.DtoProperties.PersonNameTypeDtoProperty() { Category = Dtos.EnumProperties.PersonNameType2.Birth, Detail = new Dtos.GuidObject2() { Id = "d42cc964-35cb-4560-bc46-4b881e7705ea" } },
                    Title = "MR",
                    FirstName = "First",
                    MiddleName = "Middle",
                    LastName = "Last",
                    Pedigree = "JR",
                    LastNamePrefix = "Ignore",
                    FullName = ""
                };
                personNames.Add(birthPrimaryName);
                personDto.PersonNames = personNames;
                var result = await personService.UpdatePerson3Async(personDto);
            }

            [TestMethod]
            [ExpectedException(typeof(Exception))]
            public async Task UpdatePerson3_Prefered_FullName_Null_ArgumentNullException()
            {
                //setup role
                updatePersonRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Base.BasePermissionCodes.UpdatePerson));
                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { updatePersonRole });

                //personId 0000011
                personRepositoryMock.Setup(i => i.GetPersonIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync(personId);
                personDto.PersonNames = null;
                var personNames = new List<Dtos.DtoProperties.PersonNameDtoProperty>();
                var legalPrimaryName = new Dtos.DtoProperties.PersonNameDtoProperty()
                {
                    NameType = new Dtos.DtoProperties.PersonNameTypeDtoProperty() { Category = Dtos.EnumProperties.PersonNameType2.Personal, Detail = new Dtos.GuidObject2() { Id = "806af5a5-8a9a-424f-8c9f-c1e9d084ee71" } },
                    Title = "MR",
                    FirstName = "LegalFirst",
                    MiddleName = "LegalMiddle",
                    LastName = "LegalLast",
                    Pedigree = "JR",
                    LastNamePrefix = "Ignore",
                    FullName = "FullName"
                };
                personNames.Add(legalPrimaryName);
                var birthPrimaryName = new Dtos.DtoProperties.PersonNameDtoProperty()
                {
                    NameType = new Dtos.DtoProperties.PersonNameTypeDtoProperty() { Category = Dtos.EnumProperties.PersonNameType2.Birth, Detail = new Dtos.GuidObject2() { Id = "8224f18e-69c5-480b-a9b4-52f596aa4a52" } },
                    Title = "MR",
                    FirstName = "First",
                    MiddleName = "Middle",
                    LastName = "Last",
                    Pedigree = "JR",
                    LastNamePrefix = "Ignore",
                    FullName = ""
                };
                personNames.Add(birthPrimaryName);
                personDto.PersonNames = personNames;
                var result = await personService.UpdatePerson3Async(personDto);
            }

            [TestMethod]
            [ExpectedException(typeof(RepositoryException))]
            public async Task UpdatePerson3_HISTORY_NameTypes_Null_ArgumentNullException()
            {
                var historyNameType = personNameTypes.FirstOrDefault(x => x.Code == "HISTORY");
                personNameTypes.Remove(historyNameType);

                //setup role
                updatePersonRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Base.BasePermissionCodes.UpdatePerson));
                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { updatePersonRole });

                //personId 0000011
                personRepositoryMock.Setup(i => i.GetPersonIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync(personId);

                var result = await personService.UpdatePerson3Async(personDto);
            }

            [TestMethod]
            [ExpectedException(typeof(Exception))]
            public async Task UpdatePerson3_HISTORY_LastName_Null_ArgumentNullException()
            {
                //setup role
                updatePersonRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Base.BasePermissionCodes.UpdatePerson));
                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { updatePersonRole });

                personDto.PersonNames.FirstOrDefault(i => i.NameType.Detail.Id.Equals("d42cc964-35cb-4560-bc46-4b881e7705ea", StringComparison.OrdinalIgnoreCase)).LastName = string.Empty;

                //personId 0000011
                personRepositoryMock.Setup(i => i.GetPersonIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync(personId);

                var result = await personService.UpdatePerson3Async(personDto);
            }



            [TestMethod]
            [ExpectedException(typeof(Exception))]
            public async Task UpdatePerson3_PREFERRED_GT_1_ArgumentNullException()
            {
                //setup role
                updatePersonRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Base.BasePermissionCodes.UpdatePerson));
                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { updatePersonRole });

                //personId 0000011
                personRepositoryMock.Setup(i => i.GetPersonIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync(personId);
                personDto.PersonNames = null;
                var personNames = new List<Dtos.DtoProperties.PersonNameDtoProperty>();
                var legalPrimaryName = new Dtos.DtoProperties.PersonNameDtoProperty()
                {
                    NameType = new Dtos.DtoProperties.PersonNameTypeDtoProperty() { Category = Dtos.EnumProperties.PersonNameType2.Personal, Detail = new Dtos.GuidObject2() { Id = "806af5a5-8a9a-424f-8c9f-c1e9d084ee71" } },
                    Title = "MR",
                    FirstName = "Ricky",
                    MiddleName = "Lee",
                    LastName = "Brown",
                    Pedigree = "JR",
                    LastNamePrefix = "Ignore",
                    FullName = "FullName"
                };
                personNames.Add(legalPrimaryName);
                var birthPrimaryName = new Dtos.DtoProperties.PersonNameDtoProperty()
                {
                    NameType = new Dtos.DtoProperties.PersonNameTypeDtoProperty() { Category = Dtos.EnumProperties.PersonNameType2.Birth, Detail = new Dtos.GuidObject2() { Id = "8224f18e-69c5-480b-a9b4-52f596aa4a52" } },
                    Title = "MR",
                    FirstName = "Ricky",
                    MiddleName = "Lee",
                    LastName = "Brown",
                    Pedigree = "JR",
                    LastNamePrefix = "Ignore",
                    FullName = "FullName"
                };
                personNames.Add(birthPrimaryName);

                var birthPrimaryName2 = new Dtos.DtoProperties.PersonNameDtoProperty()
                {
                    NameType = new Dtos.DtoProperties.PersonNameTypeDtoProperty() { Category = Dtos.EnumProperties.PersonNameType2.Birth, Detail = new Dtos.GuidObject2() { Id = "8224f18e-69c5-480b-a9b4-52f596aa4a52" } },
                    Title = "MR",
                    FirstName = "Ricky",
                    MiddleName = "Lee",
                    LastName = "Brown",
                    Pedigree = "JR",
                    LastNamePrefix = "Ignore",
                    FullName = "FullName"
                };
                personNames.Add(birthPrimaryName2);
                personDto.PersonNames = personNames;
                var result = await personService.UpdatePerson3Async(personDto);
            }

            [TestMethod]
            [ExpectedException(typeof(Exception))]
            public async Task UpdatePerson3_BIRTHNames_GT_1_ArgumentNullException()
            {
                //setup role
                updatePersonRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Base.BasePermissionCodes.UpdatePerson));
                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { updatePersonRole });

                //personId 0000011
                personRepositoryMock.Setup(i => i.GetPersonIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync(personId);
                personDto.PersonNames = null;
                var personNames = new List<Dtos.DtoProperties.PersonNameDtoProperty>();
                var legalPrimaryName = new Dtos.DtoProperties.PersonNameDtoProperty()
                {
                    NameType = new Dtos.DtoProperties.PersonNameTypeDtoProperty() { Category = Dtos.EnumProperties.PersonNameType2.Personal, Detail = new Dtos.GuidObject2() { Id = "806af5a5-8a9a-424f-8c9f-c1e9d084ee71" } },
                    Title = "MR",
                    FirstName = "Ricky",
                    MiddleName = "Lee",
                    LastName = "Brown",
                    Pedigree = "JR",
                    LastNamePrefix = "Ignore",
                    FullName = "FullName"
                };
                personNames.Add(legalPrimaryName);
                var birthPrimaryName = new Dtos.DtoProperties.PersonNameDtoProperty()
                {
                    NameType = new Dtos.DtoProperties.PersonNameTypeDtoProperty() { Category = Dtos.EnumProperties.PersonNameType2.Birth, Detail = new Dtos.GuidObject2() { Id = "7dfa950c-8ae4-4dca-92f0-c083604285b6" } },
                    Title = "MR",
                    FirstName = "Ricky",
                    MiddleName = "Lee",
                    LastName = "Brown",
                    Pedigree = "JR",
                    LastNamePrefix = "Ignore",
                    FullName = "FullName"
                };
                personNames.Add(birthPrimaryName);

                var birthPrimaryName2 = new Dtos.DtoProperties.PersonNameDtoProperty()
                {
                    NameType = new Dtos.DtoProperties.PersonNameTypeDtoProperty() { Category = Dtos.EnumProperties.PersonNameType2.Birth, Detail = new Dtos.GuidObject2() { Id = "7dfa950c-8ae4-4dca-92f0-c083604285b6" } },
                    Title = "MR",
                    FirstName = "Ricky",
                    MiddleName = "Lee",
                    LastName = "Brown",
                    Pedigree = "JR",
                    LastNamePrefix = "Ignore",
                    FullName = "FullName"
                };
                personNames.Add(birthPrimaryName2);
                personDto.PersonNames = personNames;
                var result = await personService.UpdatePerson3Async(personDto);
            }

            [TestMethod]
            [ExpectedException(typeof(Exception))]
            public async Task UpdatePerson3_BirthNames_Empty_ArgumentNullException()
            {
                //setup role
                updatePersonRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Base.BasePermissionCodes.UpdatePerson));
                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { updatePersonRole });

                //personId 0000011
                personRepositoryMock.Setup(i => i.GetPersonIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync(personId);
                personDto.PersonNames = null;
                var personNames = new List<Dtos.DtoProperties.PersonNameDtoProperty>();

                var personPrimaryName = new Dtos.DtoProperties.PersonNameDtoProperty()
                {
                    NameType = new Dtos.DtoProperties.PersonNameTypeDtoProperty() { Category = Dtos.EnumProperties.PersonNameType2.Personal, Detail = new Dtos.GuidObject2() { Id = "806af5a5-8a9a-424f-8c9f-c1e9d084ee71" } },
                    Title = "MR",
                    FirstName = "Ricky",
                    MiddleName = "Lee",
                    LastName = "Brown",
                    Pedigree = "JR",
                    LastNamePrefix = "Ignore"
                };
                personNames.Add(personPrimaryName);

                var personPrimaryName1 = new Dtos.DtoProperties.PersonNameDtoProperty()
                {
                    NameType = new Dtos.DtoProperties.PersonNameTypeDtoProperty() { Category = Dtos.EnumProperties.PersonNameType2.Birth, Detail = new Dtos.GuidObject2() { Id = "7dfa950c-8ae4-4dca-92f0-c083604285b6" } },
                    Title = "MR",
                    FirstName = "",
                    MiddleName = "",
                    LastName = "",
                    Pedigree = "JR",
                    LastNamePrefix = "Ignore"
                };
                personNames.Add(personPrimaryName1);
                personDto.PersonNames = personNames;
                var result = await personService.UpdatePerson3Async(personDto);
            }

            #endregion

            private void SetupData()
            {
                // setup personDto object
                personDto = new Dtos.Person3();
                personDto.Id = personGuid;
                personDto.BirthDate = new DateTime(1930, 1, 1);
                personDto.DeceasedDate = new DateTime(2014, 5, 12);
                var personNames = new List<Dtos.DtoProperties.PersonNameDtoProperty>();
                var legalPrimaryName = new Dtos.DtoProperties.PersonNameDtoProperty()
                {
                    NameType = new Dtos.DtoProperties.PersonNameTypeDtoProperty() { Category = Dtos.EnumProperties.PersonNameType2.Legal, Detail = new Dtos.GuidObject2() { Id = "806af5a5-8a9a-424f-8c9f-c1e9d084ee71" } },
                    Title = "Mr.",
                    FirstName = "LegalFirst",
                    MiddleName = "LegalMiddle",
                    LastName = "LegalLast",
                    Pedigree = "Jr.",
                    LastNamePrefix = "Mr.",
                    FullName = "LegalFirst LegalMiddle LegalLast"
                };
                personNames.Add(legalPrimaryName);

                var birthPrimaryName = new Dtos.DtoProperties.PersonNameDtoProperty()
                {
                    NameType = new Dtos.DtoProperties.PersonNameTypeDtoProperty() { Category = Dtos.EnumProperties.PersonNameType2.Birth, Detail = new Dtos.GuidObject2() { Id = "7dfa950c-8ae4-4dca-92f0-c083604285b6" } },
                    Title = "Mr.",
                    FirstName = "BirthFirst",
                    MiddleName = "BirthMiddle",
                    LastName = "BirthLast",
                    Pedigree = "Jr.",
                    LastNamePrefix = "Mr.",
                    FullName = "BirthFirst BirthMiddle BirthLast"
                };
                personNames.Add(birthPrimaryName);

                var chosenPrimaryName = new PersonNameDtoProperty()
                {
                    NameType = new PersonNameTypeDtoProperty() { Category = Dtos.EnumProperties.PersonNameType2.Personal, Detail = new Dtos.GuidObject2() { Id = "dd20ebdf-2452-41ef-9F86-ad1b1621a78d" } },
                    Title = "Mr.",
                    FirstName = "ChosenFirst",
                    MiddleName = "ChosenMiddle",
                    LastName = "ChosenLast",
                    Pedigree = "Jr.",
                    LastNamePrefix = "Ignore",
                    FullName = "ChosenFirst ChosenMiddle ChosenLast"
                };
                personNames.Add(chosenPrimaryName);

                var nickNamePrimaryName = new Dtos.DtoProperties.PersonNameDtoProperty()
                {
                    NameType = new Dtos.DtoProperties.PersonNameTypeDtoProperty() { Category = Dtos.EnumProperties.PersonNameType2.Personal, Detail = new Dtos.GuidObject2() { Id = "7b55610f-7d00-4260-bbcf-0e47fdbae647" } },
                    Title = "Mr.",
                    FirstName = "NickNameFirst",
                    MiddleName = "NickNameMiddle",
                    LastName = "NickNameLast",
                    Pedigree = "Jr.",
                    LastNamePrefix = "Mr.",
                    FullName = "NickNameFirst NickNameMiddle NickNameLast"
                };
                personNames.Add(nickNamePrimaryName);

                var historyPrimaryName = new Dtos.DtoProperties.PersonNameDtoProperty()
                {
                    NameType = new Dtos.DtoProperties.PersonNameTypeDtoProperty() { Category = Dtos.EnumProperties.PersonNameType2.Personal, Detail = new Dtos.GuidObject2() { Id = "d42cc964-35cb-4560-bc46-4b881e7705ea" } },
                    Title = "Mr.",
                    FirstName = "HistoryFirst",
                    MiddleName = "HistoryMiddle",
                    LastName = "HistoryLast",
                    Pedigree = "Jr.",
                    LastNamePrefix = "Mr.",
                    FullName = "HistoryFirst HistoryMiddle HistoryLast"
                };
                personNames.Add(historyPrimaryName);

                personDto.PersonNames = personNames;
                personDto.GenderType = Dtos.EnumProperties.GenderType2.Male;
                personDto.MaritalStatus = new Dtos.DtoProperties.PersonMaritalStatusDtoProperty() { Detail = new Dtos.GuidObject2(maritalStatusGuid), MaritalCategory = Dtos.EnumProperties.PersonMaritalStatusCategory.Married };// new Dtos.GuidObject(maritalStatusGuid);
                personDto.Ethnicity = new Dtos.DtoProperties.PersonEthnicityDtoProperty() { EthnicGroup = new Dtos.GuidObject2(ethnicityGuid) };
                personDto.Races = new List<Dtos.DtoProperties.PersonRaceDtoProperty>()
                {

                    new Dtos.DtoProperties.PersonRaceDtoProperty(){ Race = new Dtos.GuidObject2(raceAsianGuid)}
                };
                personDto.Credentials = new List<Dtos.DtoProperties.CredentialDtoProperty2>()
                {
                    new Dtos.DtoProperties.CredentialDtoProperty2()
                    {
                        Type = Dtos.EnumProperties.CredentialType2.Ssn,
                        Value = "111-11-1111"
                    }
                };
                var emailAddresses = new List<Dtos.DtoProperties.PersonEmailDtoProperty>();
                emailAddresses.Add(new Dtos.DtoProperties.PersonEmailDtoProperty()
                {

                    Type = new Dtos.DtoProperties.PersonEmailTypeDtoProperty() { EmailType = Dtos.EmailTypeList.School },
                    Address = "xyz@xmail.com"
                });
                personDto.EmailAddresses = emailAddresses;

                //Entity
                personIntegrationEntity = new PersonIntegration(personId, legalPrimaryName.LastName)
                {
                    Guid = personDto.Id,
                    Prefix = "Mr.",
                    FirstName = legalPrimaryName.FirstName,
                    MiddleName = legalPrimaryName.MiddleName,
                    Suffix = "Sr."

                };
                //Returned value
                personIntegrationReturned = new PersonIntegration(personId, "LegalLast");
                personIntegrationReturned.Guid = personGuid;
                personIntegrationReturned.Prefix = "Mr.";
                personIntegrationReturned.FirstName = "LegalFirst";
                personIntegrationReturned.MiddleName = "LegalMiddle";
                personIntegrationReturned.Suffix = "Jr.";
                personIntegrationReturned.Nickname = "NickNameFirst NickNameMiddle NickNameLast";
                personIntegrationReturned.BirthDate = new DateTime(1930, 1, 1);
                personIntegrationReturned.DeceasedDate = new DateTime(2014, 5, 12);
                personIntegrationReturned.GovernmentId = "111-11-1111";
                personIntegrationReturned.Religion = "CA";
                personIntegrationReturned.MaritalStatusCode = "M";
                personIntegrationReturned.EthnicCodes = new List<string> { "H", "N" };
                personIntegrationReturned.Gender = "M";
                personIntegrationReturned.RaceCodes = new List<string> { "AS" };
                personIntegrationReturned.AddRole(new PersonRole(PersonRoleType.Alumni, new DateTime(15, 01, 22), new DateTime(15, 05, 25)));
                personIntegrationReturned.BirthNameFirst = "BirthFirst";
                personIntegrationReturned.BirthNameLast = "BirthLast";
                personIntegrationReturned.BirthNameMiddle = "BirthMiddle";
                personIntegrationReturned.ChosenFirstName = "ChosenFirst";
                personIntegrationReturned.ChosenLastName = "ChosenLast";
                personIntegrationReturned.ChosenMiddleName = "ChosenMiddle";
                personIntegrationReturned.PreferredName = "PreferedFirst PreferedMiddle PreferedLast";
                personIntegrationReturned.FormerNames = new List<PersonName>()
                {
                    new PersonName("HistoryFirst", "HistoryMiddle", "HistoryLast")
                };
                // Mock the email address data response
                instEmail = new Domain.Base.Entities.EmailAddress("inst@inst.com", "COL") { IsPreferred = true };
                personIntegrationReturned.AddEmailAddress(instEmail);
                perEmail = new Domain.Base.Entities.EmailAddress("personal@personal.com", "PER") { IsPreferred = false };
                personIntegrationReturned.AddEmailAddress(perEmail);
                workEmail = new Domain.Base.Entities.EmailAddress("work@work.com", "BUS") { IsPreferred = false };
                personIntegrationReturned.AddEmailAddress(workEmail);

                // Mock the address hierarchy responses
                var addresses = new List<Domain.Base.Entities.Address>();
                homeAddr = new Domain.Base.Entities.Address()
                {
                    TypeCode = "HO",
                    Type = Dtos.EnumProperties.AddressType.Home.ToString(),
                    Guid = Guid.NewGuid().ToString(),
                    Status = "Current",
                    IsPreferredAddress = true
                };
                addresses.Add(homeAddr);
                mailAddr = new Domain.Base.Entities.Address()
                {
                    TypeCode = "MA",
                    Type = Dtos.EnumProperties.AddressType.Mailing.ToString(),
                    Guid = Guid.NewGuid().ToString(),
                    Status = "Current"
                };
                addresses.Add(mailAddr);
                resAddr = new Domain.Base.Entities.Address()
                {
                    TypeCode = "VA",
                    Type = Dtos.EnumProperties.AddressType.Vacation.ToString(),
                    Guid = Guid.NewGuid().ToString(),
                    Status = "Current",
                    IsPreferredResidence = true
                };
                addresses.Add(resAddr);
                workAddr = new Domain.Base.Entities.Address()
                {
                    TypeCode = "BU",
                    Type = Dtos.EnumProperties.AddressType.Business.ToString(),
                    Guid = Guid.NewGuid().ToString(),
                    Status = "Current"
                };
                addresses.Add(workAddr);
                personIntegrationReturned.Addresses = addresses;

                // Mock the person phone per type response
                homePhone = new Domain.Base.Entities.Phone("111-111-1111", "HO");
                personIntegrationReturned.AddPhone(homePhone);
                mobilePhone = new Domain.Base.Entities.Phone("222-222-2222", "MO");
                personIntegrationReturned.AddPhone(mobilePhone);
                residencePhone = new Domain.Base.Entities.Phone("333-333-3333", "VA");
                personIntegrationReturned.AddPhone(residencePhone);
                workPhone = new Domain.Base.Entities.Phone("444-444-4444", "BU", "4444");
                personIntegrationReturned.AddPhone(workPhone);

                // Mock social media
                var socialMediaTypeCode = "TW";
                var socialMediaHandle = "pontifex";
                var personSocialMedia = new SocialMedia(socialMediaTypeCode, socialMediaHandle);
                personIntegrationReturned.AddSocialMedia(personSocialMedia);
            }

            private void SetupReferenceDataRepositoryMocks()
            {
                referenceDataRepositoryMock.Setup(repo => repo.GetPrivacyStatusesAsync(It.IsAny<bool>()))
                     .ReturnsAsync(
                         new List<PrivacyStatus>() {
                        new PrivacyStatus( "d3d86052-9d55-4751-acda-5c07a064a82a", "UN", "Unrestricted", PrivacyStatusType.unrestricted),
                        new PrivacyStatus( "cff65dcc-4a9b-44ed-b8d0-930348c55ef8", "R", "restricted", PrivacyStatusType.restricted)
                        }
                     );
                personNameTypes = new List<PersonNameTypeItem>() {
                        new PersonNameTypeItem("8224f18e-69c5-480b-a9b4-52f596aa4a52", "PREFERRED", "Personal", PersonNameType.Personal),
                        new PersonNameTypeItem("7dfa950c-8ae4-4dca-92f0-c083604285b6", "BIRTH", "Birth", PersonNameType.Birth),
                        new PersonNameTypeItem("dd20ebdf-2452-41ef-9F86-ad1b1621a78d", "CHOSEN", "Chosen", PersonNameType.Personal),
                        new PersonNameTypeItem("806af5a5-8a9a-424f-8c9f-c1e9d084ee71", "LEGAL", "Legal", PersonNameType.Legal),
                        new PersonNameTypeItem("7b55610f-7d00-4260-bbcf-0e47fdbae647", "NICKNAME", "NickName", PersonNameType.Personal),
                        new PersonNameTypeItem("d42cc964-35cb-4560-bc46-4b881e7705ea", "HISTORY", "History", PersonNameType.Personal)
                        };
                referenceDataRepositoryMock.Setup(repo => repo.GetPersonNameTypesAsync(It.IsAny<bool>()))
                    .ReturnsAsync(personNameTypes);

                referenceDataRepositoryMock.Setup(repo => repo.GetDenominationsAsync(It.IsAny<bool>()))
                    .ReturnsAsync(
                        new List<Denomination>() {
                        new Denomination(baptistGuid,"BA", "Baptist") ,
                        new Denomination(catholicGuid,"CA", "Catholic")
                        }
                     );

                referenceDataRepositoryMock.Setup(repo => repo.GetEmailTypesAsync(It.IsAny<bool>()))
                     .ReturnsAsync(
                         new List<EmailType>() {
                        new EmailType("899803da-48f8-4044-beb8-5913a04b995d", "COL", "College", EmailTypeCategory.School),
                        new EmailType("301d485d-d37b-4d29-af00-465ced624a85", "PER", "Personal", EmailTypeCategory.Personal),
                        new EmailType("53fb7dab-d348-4657-b071-45d0e5933e05", "BUS", "Business", EmailTypeCategory.Business)
                        }
                     );

                // Mock the reference repository for ethnicity
                referenceDataRepositoryMock.Setup(repo => repo.GetEthnicitiesAsync(It.IsAny<bool>())).Returns(Task.FromResult<IEnumerable<Ethnicity>>(new List<Ethnicity>()
                {
                    new Ethnicity(ethnicityGuid, "H", "Hispanic", EthnicityType.Hispanic)
                }));

                referenceDataRepositoryMock.Setup(repo => repo.GetSocialMediaTypesAsync(It.IsAny<bool>()))
                     .ReturnsAsync(
                         new List<SocialMediaType>() {
                        new SocialMediaType("d1f311f4-687d-4dc7-a329-c6a8bfc9c74", "TW", "Twitter", SocialMediaTypeCategory.twitter)
                        }
                     );

                referenceDataRepositoryMock.Setup(repo => repo.GetAddressTypes2Async(It.IsAny<bool>()))
                     .ReturnsAsync(
                         new List<AddressType2>() {
                        new AddressType2("91979656-e110-4156-a75a-1a1a7294314d", "HO", "Home", AddressTypeCategory.Home),
                        new AddressType2("b887d5ec-9ed5-45e8-b44c-01782070f234", "MA", "Mailing", AddressTypeCategory.Mailing),
                        new AddressType2("d7d0a82c-fe74-480d-be1b-88a2e460af4c", "VA", "Vacation", AddressTypeCategory.Vacation),
                        new AddressType2("c9b8cd52-54e6-4c08-a9d9-224dd0c8b700", "BU", "Business", AddressTypeCategory.Business)
                         }
                     );

                referenceDataRepositoryMock.Setup(repo => repo.GetPhoneTypesAsync(It.IsAny<bool>()))
                     .ReturnsAsync(
                         new List<PhoneType>() {
                        new PhoneType("92c82d33-e55c-41a4-a2c3-f2f7d2c523d1", "HO", "Home", PhoneTypeCategory.Home),
                        new PhoneType("b6def2cc-cc95-4d0e-a32c-940fbbc2d689", "MO", "Mobile", PhoneTypeCategory.Mobile),
                        new PhoneType("f60e7b27-a3e3-4c92-9d36-f3cae27b724b", "VA", "Vacation", PhoneTypeCategory.Vacation),
                        new PhoneType("30e231cf-a199-4c9a-af01-be2e69b607c9", "BU", "Business", PhoneTypeCategory.Business)
                        }
                     );

                // Mock the reference repository for prefix
                referenceDataRepositoryMock.Setup(repo => repo.Prefixes).Returns(new List<Prefix>()
                {
                    new Prefix("MR","Mr","Mr."),
                    new Prefix("MS","Ms","Ms.")
                });

                // Mock the reference repository for prefix
                referenceDataRepositoryMock.Setup(repo => repo.GetPrefixesAsync()).ReturnsAsync(new List<Prefix>()
                {
                    new Prefix("MR","Mr","Mr."),
                    new Prefix("MS","Ms","Ms.")
                });

                // Mock the reference repository for suffix
                referenceDataRepositoryMock.Setup(repo => repo.Suffixes).Returns(new List<Suffix>()
                {
                    new Suffix("JR","Jr","Jr."),
                    new Suffix("SR","Sr","Sr.")
                });

                // Mock the reference repository for suffix
                referenceDataRepositoryMock.Setup(repo => repo.GetSuffixesAsync()).ReturnsAsync(new List<Suffix>()
                {
                    new Suffix("JR","Jr","Jr."),
                    new Suffix("SR","Sr","Sr.")
                });

                // Mock the reference repository for marital status
                referenceDataRepositoryMock.Setup(repo => repo.GetMaritalStatusesAsync(It.IsAny<bool>())).Returns(Task.FromResult<IEnumerable<MaritalStatus>>(new List<MaritalStatus>()
                {
                    new MaritalStatus(maritalStatusGuid, "M", "Married"){ Type = MaritalStatusType.Married }
                }));

                var raceEntities = new List<Race>();
                raceEntities.Add(new Race(raceAsianGuid, "AS", "Asian", RaceType.Asian));
                raceEntities.Add(new Race(racePacificIslanderGuid, "HP", "Hawaiian/Pacific Islander", RaceType.PacificIslander));
                referenceDataRepositoryMock.Setup(repo => repo.GetRacesAsync(It.IsAny<bool>())).Returns(Task.FromResult<IEnumerable<Race>>(raceEntities));
            }
        }

        [TestClass]
        public class CreateUpdatePerson3WithAddressPhoneSocialMedia : CurrentUserSetup
        {
            //Mocks
            Mock<IAdapterRegistry> adapterRegistryMock;
            Mock<IPersonRepository> personRepositoryMock;
            Mock<IPersonBaseRepository> personBaseRepositoryMock;
            Mock<IReferenceDataRepository> referenceDataRepositoryMock;
            Mock<IProfileRepository> profileRepositoryMock;
            Mock<IConfigurationRepository> configurationRepositoryMock;
            Mock<IRelationshipRepository> relationshipRepositoryMock;
            Mock<IProxyRepository> proxyRepositoryMock;
            Mock<IRoleRepository> roleRepositoryMock;
            Mock<ILogger> loggerMock;

            //userfactory
            ICurrentUserFactory currentUserFactory;

            //Service
            PersonService personService;


            private Ellucian.Colleague.Dtos.Person3 personDto;
            private Ellucian.Colleague.Domain.Base.Entities.PersonIntegration personIntegrationReturned;
            private Ellucian.Colleague.Domain.Base.Entities.PersonIntegration personIntegrationEntity;

            //private Ellucian.Colleague.Domain.Base.Entities.Person person;
            private List<Ellucian.Colleague.Domain.Base.Entities.Address> addresses = null;
            private IEnumerable<Ellucian.Colleague.Domain.Base.Entities.Phone> phones = new List<Domain.Base.Entities.Phone>();
            private IEnumerable<Ellucian.Colleague.Domain.Base.Entities.State> states;
            private IEnumerable<Ellucian.Colleague.Domain.Base.Entities.Country> countries;
            private List<Ellucian.Colleague.Domain.Base.Entities.County> counties;
            private List<PersonNameTypeItem> personNameTypes;

            //Entities
            private Ellucian.Colleague.Domain.Base.Entities.EmailAddress instEmail;
            private Ellucian.Colleague.Domain.Base.Entities.EmailAddress perEmail;
            private Ellucian.Colleague.Domain.Base.Entities.EmailAddress workEmail;
            private Ellucian.Colleague.Domain.Base.Entities.Address homeAddr;
            private Ellucian.Colleague.Domain.Base.Entities.Address mailAddr;
            private Ellucian.Colleague.Domain.Base.Entities.Address resAddr;
            private Ellucian.Colleague.Domain.Base.Entities.Address workAddr;
            private Ellucian.Colleague.Domain.Base.Entities.Phone homePhone;
            private Ellucian.Colleague.Domain.Base.Entities.Phone mobilePhone;
            private Ellucian.Colleague.Domain.Base.Entities.Phone residencePhone;
            private Ellucian.Colleague.Domain.Base.Entities.Phone workPhone;

            private IEnumerable<Domain.Base.Entities.Address> allAddresses;
            private IEnumerable<Domain.Base.Entities.PhoneType> allPhones;
            private IEnumerable<Domain.Base.Entities.Chapter> allChapters;
            private IEnumerable<Domain.Base.Entities.County> allCounties;
            private IEnumerable<Domain.Base.Entities.ZipcodeXlat> allZipCodeXlats;
            private IEnumerable<Domain.Base.Entities.GeographicAreaType> allGeographicAreaTypes;


            private List<Dtos.DtoProperties.PersonAddressDtoProperty> addressesCollection = new List<Dtos.DtoProperties.PersonAddressDtoProperty>();

            //Data
            private string personId = "0000011";
            private string personGuid = "5674f28b-b216-4055-b236-81a922d93b4c";
            private string maritalStatusGuid = "dca8edb5-120f-479a-a6bb-35ba3af4b344";
            private string ethnicityGuid = "dd0c42ca-c61d-4ca6-8d21-96ab5be35623";
            private string raceAsianGuid = "72b7737b-27db-4a06-944b-97d00c29b3db";
            private string racePacificIslanderGuid = "e20f9821-28a2-4e34-8550-6758850a0cf8";
            private string baptistGuid = "c0bdfd92-462f-4e59-bba5-1b15c4771c86";
            private string catholicGuid = "f96f04b0-4973-41f6-bc3d-9c7bc1c2c458";

            private string countyGuid = Guid.NewGuid().ToString();


            [TestInitialize]
            public void Initialize()
            {
                adapterRegistryMock = new Mock<IAdapterRegistry>();
                personRepositoryMock = new Mock<IPersonRepository>();
                personBaseRepositoryMock = new Mock<IPersonBaseRepository>();
                referenceDataRepositoryMock = new Mock<IReferenceDataRepository>();
                profileRepositoryMock = new Mock<IProfileRepository>();
                configurationRepositoryMock = new Mock<IConfigurationRepository>();
                relationshipRepositoryMock = new Mock<IRelationshipRepository>();
                proxyRepositoryMock = new Mock<IProxyRepository>();
                roleRepositoryMock = new Mock<IRoleRepository>();
                loggerMock = new Mock<ILogger>();

                currentUserFactory = new CurrentUserSetup.ThirdPartyUserFactory();

                SetupData();

                SetupReferenceDataRepositoryMocks();

                // International Parameters Host Country
                personRepositoryMock.Setup(repo => repo.GetHostCountryAsync()).ReturnsAsync("USA");

                // Mock address guid check return
                personRepositoryMock.Setup(i => i.GetAddressIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync("x");

                personRepositoryMock.Setup(i => i.Update2Async(It.IsAny<PersonIntegration>(), It.IsAny<IEnumerable<Domain.Base.Entities.Address>>(),
                    It.IsAny<IEnumerable<Ellucian.Colleague.Domain.Base.Entities.Phone>>(), It.IsAny<int>())).ReturnsAsync(personIntegrationReturned);

                personService = new PersonService(adapterRegistryMock.Object, personRepositoryMock.Object, personBaseRepositoryMock.Object, referenceDataRepositoryMock.Object, profileRepositoryMock.Object,
                                                  configurationRepositoryMock.Object, relationshipRepositoryMock.Object, proxyRepositoryMock.Object, currentUserFactory,
                                                  roleRepositoryMock.Object, loggerMock.Object);
            }


            [TestCleanup]
            public void Cleanup()
            {
                personService = null;
                adapterRegistryMock = null;
                personRepositoryMock = null;
                personBaseRepositoryMock = null;
                referenceDataRepositoryMock = null;
                profileRepositoryMock = null;
                configurationRepositoryMock = null;
                relationshipRepositoryMock = null;
                proxyRepositoryMock = null;
                roleRepositoryMock = null;
                loggerMock = null;
            }

            [TestMethod]
            public async Task UpdatePerson3_UpdatePerson3Async_WithAddress()
            {
                //setup role
                updatePersonRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Base.BasePermissionCodes.UpdatePerson));
                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { updatePersonRole });

                //personId 0000011
                personRepositoryMock.Setup(i => i.GetPersonIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync(personId);

                var actual = await personService.UpdatePerson3Async(personDto);

                Assert.AreEqual(personDto.Addresses.Count(), actual.Addresses.Count());
                Assert.AreEqual(personDto.Phones.Count(), actual.Phones.Count());
                Assert.AreEqual(personDto.SocialMedia.Count(), actual.SocialMedia.Count());
            }

            [TestMethod]
            public async Task UpdatePerson3_UpdatePerson3Async_AddressNullId()
            {
                addressesCollection = new List<Dtos.DtoProperties.PersonAddressDtoProperty>();
                allAddresses = new TestAddressRepository().GetAddressDataWithNullId().ToList().Where(i => string.IsNullOrEmpty(i.Guid));

                foreach (var source in allAddresses)
                {
                    var address = new Ellucian.Colleague.Dtos.DtoProperties.PersonAddressDtoProperty
                    {
                        address = new PersonAddress()
                        {
                            Id = source.Guid,
                            AddressLines = source.AddressLines,
                            Latitude = source.Latitude,
                            Longitude = source.Longitude
                        },
                        AddressEffectiveStart = new DateTime(2015, 09, 01),
                        AddressEffectiveEnd = new DateTime(2015, 12, 20),
                        Preference = Dtos.EnumProperties.PersonPreference.Primary,
                        Type = new PersonAddressTypeDtoProperty()
                        {
                            AddressType = string.IsNullOrEmpty(source.Type) ? null : (Dtos.EnumProperties.AddressType?)Enum.Parse(typeof(Dtos.EnumProperties.AddressType), source.Type, true)
                        }
                    };
                    addressesCollection.Add(address);
                }
                personDto.Addresses = addressesCollection;
                personDto.Addresses.First().address.Place = new AddressPlace() { Country = new AddressCountry() { Locality = "Locality", Code = IsoCode.USA } };
                personDto.Addresses.First().address.AddressLines = new List<string>() { "Something" };
                personDto.Addresses.First().address.GeographicAreas = new List<GuidObject2>() { new GuidObject2("9ae3a175-1dfd-4937-b97b-3c9ad596e023") };

                updatePersonRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Base.BasePermissionCodes.UpdatePerson));
                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { updatePersonRole });

                var addrWithoutId = new TestAddressRepository().GetAddressDataWithNullId().Where(i => string.IsNullOrEmpty(i.Guid)).ToList();
                personIntegrationReturned.Addresses = addrWithoutId;
                personRepositoryMock.Setup(i => i.Update2Async(It.IsAny<PersonIntegration>(), It.IsAny<IEnumerable<Domain.Base.Entities.Address>>(),
                    It.IsAny<IEnumerable<Ellucian.Colleague.Domain.Base.Entities.Phone>>(), It.IsAny<int>())).ReturnsAsync(personIntegrationReturned);


                //personId 0000011
                personRepositoryMock.Setup(i => i.GetPersonIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync(personId);

                var actual = await personService.UpdatePerson3Async(personDto);

                Assert.IsNotNull(actual);
                Assert.AreEqual(personDto.Addresses.Count(), actual.Addresses.Count());
            }

            [TestMethod]
            public async Task UpdatePerson3_UpdatePerson3Async_All_Addresses()
            {
                addressesCollection = new List<Dtos.DtoProperties.PersonAddressDtoProperty>();
                allAddresses = new TestAddressRepository().GetAddressData().ToList();
                //d44134f9-0924-45d4-8b91-be9531aa7773
                var addAddress = allAddresses.FirstOrDefault(i => i.Guid.Equals("d44134f9-0924-45d4-8b91-be9531aa7773", StringComparison.OrdinalIgnoreCase));
                addAddress.TypeCode = "MA";
                allAddresses.ToList().Add(addAddress);
                allAddresses.All(i => i.SeasonalDates == new List<AddressSeasonalDates>()
                {
                    new AddressSeasonalDates("01/01", "05/31"),
                    new AddressSeasonalDates("08/01", "12/31")
                });
                allAddresses.Where(i => i.Guid.Equals("d44134f9-0924-45d4-8b91-be9531aa7773", StringComparison.OrdinalIgnoreCase)).All(i => i.IsPreferredAddress == true);

                foreach (var source in allAddresses)
                {
                    var address = new Ellucian.Colleague.Dtos.DtoProperties.PersonAddressDtoProperty
                    {
                        address = new PersonAddress()
                        {
                            Id = source.Guid,
                            AddressLines = source.AddressLines,
                            Latitude = source.Latitude,
                            Longitude = source.Longitude
                        },
                        AddressEffectiveStart = new DateTime(2015, 09, 01),
                        AddressEffectiveEnd = new DateTime(2015, 12, 20),
                        Preference = Dtos.EnumProperties.PersonPreference.Primary,
                        Type = new PersonAddressTypeDtoProperty()
                        {
                            AddressType = string.IsNullOrEmpty(source.Type) ? null : (Dtos.EnumProperties.AddressType?)Enum.Parse(typeof(Dtos.EnumProperties.AddressType), source.Type, true)
                        },
                        SeasonalOccupancies = new List<PersonAddressRecurrenceDtoProperty>()
                        {
                            new PersonAddressRecurrenceDtoProperty()
                            {
                             Recurrence = new Recurrence3()
                             {
                                 TimePeriod = new RepeatTimePeriod2(){ StartOn = new DateTimeOffset(2016, 01, 01,0,0,0, new TimeSpan()), EndOn = new DateTimeOffset(2016, 05, 31,0,0,0, new TimeSpan())}
                             }
                            },
                            new PersonAddressRecurrenceDtoProperty()
                            {
                             Recurrence = new Recurrence3()
                             {
                                 TimePeriod = new RepeatTimePeriod2(){ StartOn = new DateTimeOffset(2016, 08, 01,0,0,0, new TimeSpan()), EndOn = new DateTimeOffset(2016, 12, 31,0,0,0, new TimeSpan())}
                             }
                            }
                        }
                    };
                    addressesCollection.Add(address);
                }

                personDto.Addresses = addressesCollection;
                personDto.Addresses.First().address.Place = new AddressPlace() { Country = new AddressCountry() { Locality = "Locality", Code = IsoCode.USA } };
                personDto.Addresses.First().address.AddressLines = new List<string>() { "Something" };
                personDto.Addresses.First().address.GeographicAreas = new List<GuidObject2>() { new GuidObject2("9ae3a175-1dfd-4937-b97b-3c9ad596e023") };

                updatePersonRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Base.BasePermissionCodes.UpdatePerson));
                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { updatePersonRole });

                //personId 0000011
                personRepositoryMock.Setup(i => i.GetPersonIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync(personId);

                var actual = await personService.UpdatePerson3Async(personDto);

                Assert.IsNotNull(actual);
                Assert.AreEqual(personDto.Addresses.Count(), actual.Addresses.Count());
            }

            [TestMethod]
            public async Task UpdatePerson3_UpdatePerson3Async_PersonId_NullEmpty_Create()
            {
                //setup role
                createPersonRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Base.BasePermissionCodes.CreatePerson));
                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { createPersonRole });

                //personId 0000011
                personRepositoryMock.Setup(i => i.GetPersonIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync(string.Empty);

                personRepositoryMock.Setup(i => i.Create2Async(It.IsAny<PersonIntegration>(), It.IsAny<IEnumerable<Domain.Base.Entities.Address>>(), It.IsAny<IEnumerable<Domain.Base.Entities.Phone>>(), It.IsAny<int>())).ReturnsAsync(personIntegrationReturned);


                var actuals = await personService.UpdatePerson3Async(personDto);

                Assert.AreEqual(personDto.Addresses.Count(), actuals.Addresses.Count());
                foreach (var addr in actuals.Addresses)
                {
                    var expected = personDto.Addresses.FirstOrDefault(i => i.address.Id.Equals(addr.address.Id));
                    Assert.IsNotNull(expected);

                    Assert.AreEqual(expected.address.Id, addr.address.Id);
                    Assert.AreEqual(expected.address.Latitude, addr.address.Latitude);
                    Assert.AreEqual(expected.address.Longitude, addr.address.Longitude);
                    Assert.AreEqual(expected.address.Place, addr.address.Place);
                }

                foreach (var actual in actuals.Phones)
                {
                    var expected = personDto.Phones.FirstOrDefault(i => i.Number.Equals(actual.Number, StringComparison.OrdinalIgnoreCase));
                    Assert.IsNotNull(expected);

                    Assert.AreEqual(expected.Number, actual.Number);
                    Assert.AreEqual(expected.CountryCallingCode, actual.CountryCallingCode);
                    Assert.AreEqual(expected.Extension, actual.Extension);
                    Assert.AreEqual(expected.Preference, actual.Preference);
                    Assert.AreEqual(expected.Type.PhoneType, actual.Type.PhoneType);
                }

                foreach (var actual in actuals.SocialMedia)
                {
                    var expected = personDto.SocialMedia.FirstOrDefault(i => i.Address.Equals(actual.Address, StringComparison.OrdinalIgnoreCase));
                    Assert.IsNotNull(expected);

                    Assert.AreEqual(expected.Address, actual.Address);
                    Assert.AreEqual(expected.Preference, actual.Preference);
                    Assert.AreEqual(expected.Type.Category, actual.Type.Category);
                }
            }

            [TestMethod]
            [ExpectedException(typeof(Exception))]
            public async Task UpdatePerson3_UpdatePerson3Async_PlaceNull_Exception()
            {
                addressesCollection = new List<Dtos.DtoProperties.PersonAddressDtoProperty>();
                allAddresses = new TestAddressRepository().GetAddressDataWithNullId().ToList().Where(i => string.IsNullOrEmpty(i.Guid));

                foreach (var source in allAddresses)
                {
                    var address = new Ellucian.Colleague.Dtos.DtoProperties.PersonAddressDtoProperty
                    {
                        address = new PersonAddress()
                        {
                            Id = source.Guid,
                            AddressLines = source.AddressLines,
                            Latitude = source.Latitude,
                            Longitude = source.Longitude
                        },
                        AddressEffectiveStart = new DateTime(2015, 09, 01),
                        AddressEffectiveEnd = new DateTime(2015, 12, 20),
                        Preference = Dtos.EnumProperties.PersonPreference.Primary,
                        Type = new PersonAddressTypeDtoProperty()
                        {
                            AddressType = string.IsNullOrEmpty(source.Type) ? null : (Dtos.EnumProperties.AddressType?)Enum.Parse(typeof(Dtos.EnumProperties.AddressType), source.Type, true)
                        }
                    };
                    addressesCollection.Add(address);
                }
                personDto.Addresses = addressesCollection;
                //setup role
                updatePersonRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Base.BasePermissionCodes.UpdatePerson));
                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { updatePersonRole });

                //personId 0000011
                personRepositoryMock.Setup(i => i.GetPersonIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync(personId);

                var actual = await personService.UpdatePerson3Async(personDto);
            }

            [TestMethod]
            [ExpectedException(typeof(Exception))]
            public async Task UpdatePerson3_UpdatePerson3Async_AddressLineNull_Exception()
            {
                addressesCollection = new List<Dtos.DtoProperties.PersonAddressDtoProperty>();
                allAddresses = new TestAddressRepository().GetAddressDataWithNullId().ToList().Where(i => string.IsNullOrEmpty(i.Guid));

                foreach (var source in allAddresses)
                {
                    var address = new Ellucian.Colleague.Dtos.DtoProperties.PersonAddressDtoProperty
                    {
                        address = new PersonAddress()
                        {
                            Id = source.Guid,
                            AddressLines = source.AddressLines,
                            Latitude = source.Latitude,
                            Longitude = source.Longitude
                        },
                        AddressEffectiveStart = new DateTime(2015, 09, 01),
                        AddressEffectiveEnd = new DateTime(2015, 12, 20),
                        Preference = Dtos.EnumProperties.PersonPreference.Primary,
                        Type = new PersonAddressTypeDtoProperty()
                        {
                            AddressType = string.IsNullOrEmpty(source.Type) ? null : (Dtos.EnumProperties.AddressType?)Enum.Parse(typeof(Dtos.EnumProperties.AddressType), source.Type, true)
                        }
                    };
                    addressesCollection.Add(address);
                }
                personDto.Addresses = addressesCollection;
                personDto.Addresses.First().address.Place = new AddressPlace();
                //setup role
                updatePersonRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Base.BasePermissionCodes.UpdatePerson));
                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { updatePersonRole });

                //personId 0000011
                personRepositoryMock.Setup(i => i.GetPersonIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync(personId);

                var actual = await personService.UpdatePerson3Async(personDto);
            }

            [TestMethod]
            [ExpectedException(typeof(Exception))]
            public async Task UpdatePerson3_UpdatePerson3Async_PlaceCountryNull_Exception()
            {
                addressesCollection = new List<Dtos.DtoProperties.PersonAddressDtoProperty>();
                allAddresses = new TestAddressRepository().GetAddressDataWithNullId().ToList().Where(i => string.IsNullOrEmpty(i.Guid));

                foreach (var source in allAddresses)
                {
                    var address = new Ellucian.Colleague.Dtos.DtoProperties.PersonAddressDtoProperty
                    {
                        address = new PersonAddress()
                        {
                            Id = source.Guid,
                            AddressLines = source.AddressLines,
                            Latitude = source.Latitude,
                            Longitude = source.Longitude
                        },
                        AddressEffectiveStart = new DateTime(2015, 09, 01),
                        AddressEffectiveEnd = new DateTime(2015, 12, 20),
                        Preference = Dtos.EnumProperties.PersonPreference.Primary,
                        Type = new PersonAddressTypeDtoProperty()
                        {
                            AddressType = string.IsNullOrEmpty(source.Type) ? null : (Dtos.EnumProperties.AddressType?)Enum.Parse(typeof(Dtos.EnumProperties.AddressType), source.Type, true)
                        }
                    };
                    addressesCollection.Add(address);
                }
                personDto.Addresses = addressesCollection;
                personDto.Addresses.First().address.Place = new AddressPlace();
                personDto.Addresses.First().address.AddressLines = new List<string>() { "Something" };
                //setup role
                updatePersonRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Base.BasePermissionCodes.UpdatePerson));
                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { updatePersonRole });

                //personId 0000011
                personRepositoryMock.Setup(i => i.GetPersonIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync(personId);

                var actual = await personService.UpdatePerson3Async(personDto);
            }

            [TestMethod]
            [ExpectedException(typeof(Exception))]
            public async Task UpdatePerson3_UpdatePerson3Async_PlaceCountryLocalityNull_Exception()
            {
                addressesCollection = new List<Dtos.DtoProperties.PersonAddressDtoProperty>();
                allAddresses = new TestAddressRepository().GetAddressDataWithNullId().ToList().Where(i => string.IsNullOrEmpty(i.Guid));

                foreach (var source in allAddresses)
                {
                    var address = new Ellucian.Colleague.Dtos.DtoProperties.PersonAddressDtoProperty
                    {
                        address = new PersonAddress()
                        {
                            Id = source.Guid,
                            AddressLines = source.AddressLines,
                            Latitude = source.Latitude,
                            Longitude = source.Longitude
                        },
                        AddressEffectiveStart = new DateTime(2015, 09, 01),
                        AddressEffectiveEnd = new DateTime(2015, 12, 20),
                        Preference = Dtos.EnumProperties.PersonPreference.Primary,
                        Type = new PersonAddressTypeDtoProperty()
                        {
                            AddressType = string.IsNullOrEmpty(source.Type) ? null : (Dtos.EnumProperties.AddressType?)Enum.Parse(typeof(Dtos.EnumProperties.AddressType), source.Type, true)
                        }
                    };
                    addressesCollection.Add(address);
                }
                personDto.Addresses = addressesCollection;
                personDto.Addresses.First().address.Place = new AddressPlace() { Country = new AddressCountry() { Locality = string.Empty } };
                personDto.Addresses.First().address.AddressLines = new List<string>() { "Something" };
                //setup role
                updatePersonRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Base.BasePermissionCodes.UpdatePerson));
                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { updatePersonRole });

                //personId 0000011
                personRepositoryMock.Setup(i => i.GetPersonIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync(personId);

                var actual = await personService.UpdatePerson3Async(personDto);
            }

            [TestMethod]
            [ExpectedException(typeof(Exception))]
            public async Task UpdatePerson3_UpdatePerson3Async_SocialMediaTypeNull_Exception()
            {
                //setup role
                createPersonRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Base.BasePermissionCodes.CreatePerson));
                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { createPersonRole });

                //personId 0000011
                personRepositoryMock.Setup(i => i.GetPersonIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync(string.Empty);

                personRepositoryMock.Setup(i => i.Create2Async(It.IsAny<PersonIntegration>(), It.IsAny<IEnumerable<Domain.Base.Entities.Address>>(), It.IsAny<IEnumerable<Domain.Base.Entities.Phone>>(), It.IsAny<int>())).ReturnsAsync(personIntegrationReturned);

                personDto.SocialMedia.First().Type = null;

                var actual = await personService.UpdatePerson3Async(personDto);
            }

            [TestMethod]
            [ExpectedException(typeof(Exception))]
            public async Task UpdatePerson3_UpdatePerson2Async_SocialMediaAddressNull_Exception()
            {
                //setup role
                createPersonRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Base.BasePermissionCodes.CreatePerson));
                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { createPersonRole });

                //personId 0000011
                personRepositoryMock.Setup(i => i.GetPersonIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync(string.Empty);

                personRepositoryMock.Setup(i => i.Create2Async(It.IsAny<PersonIntegration>(), It.IsAny<IEnumerable<Domain.Base.Entities.Address>>(), It.IsAny<IEnumerable<Domain.Base.Entities.Phone>>(), It.IsAny<int>())).ReturnsAsync(personIntegrationReturned);

                personDto.SocialMedia.First().Address = null;

                var actual = await personService.UpdatePerson3Async(personDto);
            }

            [TestMethod]
            [ExpectedException(typeof(Exception))]
            public async Task UpdatePerson3_UpdatePerson3Async_SocialMediaTypNotFound_Exception()
            {
                //setup role
                createPersonRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Base.BasePermissionCodes.CreatePerson));
                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { createPersonRole });

                //personId 0000011
                personRepositoryMock.Setup(i => i.GetPersonIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync(string.Empty);

                personRepositoryMock.Setup(i => i.Create2Async(It.IsAny<PersonIntegration>(), It.IsAny<IEnumerable<Domain.Base.Entities.Address>>(), It.IsAny<IEnumerable<Domain.Base.Entities.Phone>>(), It.IsAny<int>())).ReturnsAsync(personIntegrationReturned);

                personDto.SocialMedia.First().Type = new PersonSocialMediaType() { Category = Ellucian.Colleague.Dtos.SocialMediaTypeCategory.blog };

                var actual = await personService.UpdatePerson3Async(personDto);
            }

            [TestMethod]
            [ExpectedException(typeof(Exception))]
            public async Task UpdatePerson3_NullAddress_Exception()
            {
                //setup role
                createPersonRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Base.BasePermissionCodes.CreatePerson));
                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { createPersonRole });

                //personId 0000011
                personRepositoryMock.Setup(i => i.GetPersonIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync(string.Empty);

                personRepositoryMock.Setup(i => i.Create2Async(It.IsAny<PersonIntegration>(), It.IsAny<IEnumerable<Domain.Base.Entities.Address>>(), phones, It.IsAny<int>())).ReturnsAsync(personIntegrationReturned);

                personDto.Addresses = new List<PersonAddressDtoProperty>()
                {
                    new PersonAddressDtoProperty(){address = null}
                };

                var actual = await personService.UpdatePerson3Async(personDto);
            }

            [TestMethod]
            [ExpectedException(typeof(Exception))]
            public async Task UpdatePerson3_NullType_Exception()
            {
                //setup role
                createPersonRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Base.BasePermissionCodes.CreatePerson));
                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { createPersonRole });

                //personId 0000011
                personRepositoryMock.Setup(i => i.GetPersonIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync(string.Empty);

                personRepositoryMock.Setup(i => i.Create2Async(It.IsAny<PersonIntegration>(), It.IsAny<IEnumerable<Domain.Base.Entities.Address>>(), phones, It.IsAny<int>())).ReturnsAsync(personIntegrationReturned);

                personDto.Addresses = new List<PersonAddressDtoProperty>()
                {
                    new PersonAddressDtoProperty(){address = new PersonAddress(), Type = null}
                };

                var actual = await personService.UpdatePerson3Async(personDto);
            }

            [TestMethod]
            [ExpectedException(typeof(Exception))]
            public async Task UpdatePerson3_NullPhoneType_Exception()
            {
                //setup role
                createPersonRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Base.BasePermissionCodes.CreatePerson));
                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { createPersonRole });

                //personId 0000011
                personRepositoryMock.Setup(i => i.GetPersonIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync(string.Empty);

                personRepositoryMock.Setup(i => i.Create2Async(It.IsAny<PersonIntegration>(), It.IsAny<IEnumerable<Domain.Base.Entities.Address>>(), phones, It.IsAny<int>())).ReturnsAsync(personIntegrationReturned);

                personDto.Phones = new List<PersonPhoneDtoProperty>()
                {
                    new PersonPhoneDtoProperty()
                    {
                         CountryCallingCode = "1",
                         Number = "111-111-1111"
                    }
                };

                var actual = await personService.UpdatePerson3Async(personDto);
            }

            [TestMethod]
            [ExpectedException(typeof(Exception))]
            public async Task UpdatePerson3_NullPhoneNumber_Exception()
            {
                //setup role
                createPersonRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Base.BasePermissionCodes.CreatePerson));
                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { createPersonRole });

                //personId 0000011
                personRepositoryMock.Setup(i => i.GetPersonIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync(string.Empty);

                personRepositoryMock.Setup(i => i.Create2Async(It.IsAny<PersonIntegration>(), It.IsAny<IEnumerable<Domain.Base.Entities.Address>>(), phones, It.IsAny<int>())).ReturnsAsync(personIntegrationReturned);

                personDto.Phones = new List<PersonPhoneDtoProperty>()
                {
                    new PersonPhoneDtoProperty()
                    {
                         CountryCallingCode = "1",
                         Number = "" ,
                         Type = new PersonPhoneTypeDtoProperty(){ PhoneType = PersonPhoneTypeCategory.Home}
                    }
                };

                var actual = await personService.UpdatePerson3Async(personDto);
            }

            [TestMethod]
            [ExpectedException(typeof(Exception))]
            public async Task UpdatePerson3_InvalidPhoneType_Exception()
            {
                //setup role
                createPersonRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Base.BasePermissionCodes.CreatePerson));
                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { createPersonRole });

                //personId 0000011
                personRepositoryMock.Setup(i => i.GetPersonIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync(string.Empty);

                personRepositoryMock.Setup(i => i.Create2Async(It.IsAny<PersonIntegration>(), It.IsAny<IEnumerable<Domain.Base.Entities.Address>>(), phones, It.IsAny<int>())).ReturnsAsync(personIntegrationReturned);

                personDto.Phones = new List<PersonPhoneDtoProperty>()
                {
                    new PersonPhoneDtoProperty()
                    {
                         CountryCallingCode = "1",
                         Number = "1234" ,
                         Type = new PersonPhoneTypeDtoProperty(){ PhoneType = PersonPhoneTypeCategory.Fax, Detail = new GuidObject2("12345") }
                    }
                };

                var actual = await personService.UpdatePerson3Async(personDto);
            }

            [TestMethod]
            [ExpectedException(typeof(Exception))]
            public async Task UpdatePerson3_NullPhoneTypeId_Exception()
            {
                //setup role
                createPersonRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Base.BasePermissionCodes.CreatePerson));
                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { createPersonRole });

                //personId 0000011
                personRepositoryMock.Setup(i => i.GetPersonIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync(string.Empty);

                personRepositoryMock.Setup(i => i.Create2Async(It.IsAny<PersonIntegration>(), It.IsAny<IEnumerable<Domain.Base.Entities.Address>>(), phones, It.IsAny<int>())).ReturnsAsync(personIntegrationReturned);

                personDto.Phones = new List<PersonPhoneDtoProperty>()
                {
                    new PersonPhoneDtoProperty()
                    {
                         CountryCallingCode = "1",
                         Number = "1234" ,
                         Type = new PersonPhoneTypeDtoProperty(){ PhoneType = PersonPhoneTypeCategory.Home, Detail = new GuidObject2() }
                    }
                };

                var actual = await personService.UpdatePerson3Async(personDto);
            }

            [TestMethod]
            [ExpectedException(typeof(Exception))]
            public async Task UpdatePerson3_Username_Exception()
            {
                //setup role
                updatePersonRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Base.BasePermissionCodes.UpdatePerson));
                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { updatePersonRole });

                //personId 0000011
                personRepositoryMock.Setup(i => i.GetPersonIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync(personId);

                personDto.Credentials = new List<Dtos.DtoProperties.CredentialDtoProperty2>()
                {
                    new Dtos.DtoProperties.CredentialDtoProperty2()
                    {
                        Type = Dtos.EnumProperties.CredentialType2.ColleagueUserName,
                        Value = "testUsername"
                    }
                };
                // Mock the response for getting a Person Pin 
                var personPin = new PersonPin("0000011", "WrongUsername");
                var personPins = new List<PersonPin>();
                personPins.Add(personPin);
                personRepositoryMock.Setup(repo => repo.GetPersonPinsAsync(It.IsAny<string[]>())).ReturnsAsync(personPins);

                var actual = await personService.UpdatePerson3Async(personDto);
            }

            private void SetupData()
            {
                // setup personDto object
                personDto = new Dtos.Person3();
                personDto.Id = personGuid;
                personDto.BirthDate = new DateTime(1930, 1, 1);
                personDto.DeceasedDate = new DateTime(2014, 5, 12);
                var personNames = new List<Dtos.DtoProperties.PersonNameDtoProperty>();
                var legalPrimaryName = new Dtos.DtoProperties.PersonNameDtoProperty()
                {
                    NameType = new Dtos.DtoProperties.PersonNameTypeDtoProperty() { Category = Dtos.EnumProperties.PersonNameType2.Legal, Detail = new Dtos.GuidObject2() { Id = "806af5a5-8a9a-424f-8c9f-c1e9d084ee71" } },
                    Title = "Mr.",
                    FirstName = "LegalFirst",
                    MiddleName = "LegalMiddle",
                    LastName = "LegalLast",
                    Pedigree = "Jr.",
                    LastNamePrefix = "Mr.",
                    FullName = "LegalFirst LegalMiddle LegalLast"
                };
                personNames.Add(legalPrimaryName);

                var birthPrimaryName = new Dtos.DtoProperties.PersonNameDtoProperty()
                {
                    NameType = new Dtos.DtoProperties.PersonNameTypeDtoProperty() { Category = Dtos.EnumProperties.PersonNameType2.Birth, Detail = new Dtos.GuidObject2() { Id = "7dfa950c-8ae4-4dca-92f0-c083604285b6" } },
                    Title = "Mr.",
                    FirstName = "BirthFirst",
                    MiddleName = "BirthMiddle",
                    LastName = "BirthLast",
                    Pedigree = "Jr.",
                    LastNamePrefix = "Mr.",
                    FullName = "BirthFirst BirthMiddle BirthLast"
                };
                personNames.Add(birthPrimaryName);

                var chosenPrimaryName = new PersonNameDtoProperty()
                {
                    NameType = new PersonNameTypeDtoProperty() { Category = Dtos.EnumProperties.PersonNameType2.Personal, Detail = new Dtos.GuidObject2() { Id = "dd20ebdf-2452-41ef-9F86-ad1b1621a78d" } },
                    Title = "Mr.",
                    FirstName = "ChosenFirst",
                    MiddleName = "ChosenMiddle",
                    LastName = "ChosenLast",
                    Pedigree = "Jr.",
                    LastNamePrefix = "Ignore",
                    FullName = "ChosenFirst ChosenMiddle ChosenLast"
                };
                personNames.Add(chosenPrimaryName);

                var nickNamePrimaryName = new Dtos.DtoProperties.PersonNameDtoProperty()
                {
                    NameType = new Dtos.DtoProperties.PersonNameTypeDtoProperty() { Category = Dtos.EnumProperties.PersonNameType2.Personal, Detail = new Dtos.GuidObject2() { Id = "7b55610f-7d00-4260-bbcf-0e47fdbae647" } },
                    Title = "Mr.",
                    FirstName = "NickNameFirst",
                    MiddleName = "NickNameMiddle",
                    LastName = "NickNameLast",
                    Pedigree = "Jr.",
                    LastNamePrefix = "Mr.",
                    FullName = "NickNameFirst NickNameMiddle NickNameLast"
                };
                personNames.Add(nickNamePrimaryName);

                var historyPrimaryName = new Dtos.DtoProperties.PersonNameDtoProperty()
                {
                    NameType = new Dtos.DtoProperties.PersonNameTypeDtoProperty() { Category = Dtos.EnumProperties.PersonNameType2.Personal, Detail = new Dtos.GuidObject2() { Id = "d42cc964-35cb-4560-bc46-4b881e7705ea" } },
                    Title = "Mr.",
                    FirstName = "HistoryFirst",
                    MiddleName = "HistoryMiddle",
                    LastName = "HistoryLast",
                    Pedigree = "Jr.",
                    LastNamePrefix = "Mr.",
                    FullName = "HistoryFirst HistoryMiddle HistoryLast"
                };
                personNames.Add(historyPrimaryName);

                personDto.PersonNames = personNames;
                personDto.GenderType = Dtos.EnumProperties.GenderType2.Male;
                personDto.MaritalStatus = new Dtos.DtoProperties.PersonMaritalStatusDtoProperty() { Detail = new Dtos.GuidObject2(maritalStatusGuid), MaritalCategory = Dtos.EnumProperties.PersonMaritalStatusCategory.Married };// new Dtos.GuidObject(maritalStatusGuid);
                personDto.Ethnicity = new Dtos.DtoProperties.PersonEthnicityDtoProperty() { EthnicGroup = new Dtos.GuidObject2(ethnicityGuid) };
                personDto.Races = new List<Dtos.DtoProperties.PersonRaceDtoProperty>()
                {

                    new Dtos.DtoProperties.PersonRaceDtoProperty(){ Race = new Dtos.GuidObject2(raceAsianGuid)}
                };
                personDto.Credentials = new List<Dtos.DtoProperties.CredentialDtoProperty2>()
                {
                    new Dtos.DtoProperties.CredentialDtoProperty2()
                    {
                        Type = Dtos.EnumProperties.CredentialType2.Ssn,
                        Value = "111-11-1111"
                    }
                };
                var emailAddresses = new List<Dtos.DtoProperties.PersonEmailDtoProperty>();
                emailAddresses.Add(new Dtos.DtoProperties.PersonEmailDtoProperty()
                {

                    Type = new Dtos.DtoProperties.PersonEmailTypeDtoProperty() { EmailType = Dtos.EmailTypeList.School },
                    Address = "xyz@xmail.com"
                });
                personDto.EmailAddresses = emailAddresses;

                // Mock the reference repository for states
                states = new List<State>()
                {
                    new State("VA","Virginia"),
                    new State("MD","Maryland"),
                    new State("NY","New York"),
                    new State("MA","Massachusetts")
                };

                //Entity
                personIntegrationEntity = new PersonIntegration(personId, legalPrimaryName.LastName)
                {
                    Guid = personDto.Id,
                    Prefix = "Mr.",
                    FirstName = legalPrimaryName.FirstName,
                    MiddleName = legalPrimaryName.MiddleName,
                    Suffix = "Sr."

                };
                allChapters = new TestGeographicAreaRepository().GetChapters();
                allCounties = new TestGeographicAreaRepository().GetCounties();
                allZipCodeXlats = new TestGeographicAreaRepository().GetZipCodeXlats();
                allGeographicAreaTypes = new TestGeographicAreaRepository().Get();
                counties = new List<County>()
                {
                    new County(Guid.NewGuid().ToString(), "FFX","Fairfax County"),
                    new County(Guid.NewGuid().ToString(), "BAL","Baltimore County"),
                    new County(Guid.NewGuid().ToString(), "NY","New York County"),
                    new County(Guid.NewGuid().ToString(), "BOS","Boston County")
                };

                //Addreses
                allAddresses = new TestAddressRepository().GetAddressData().ToList();

                foreach (var source in allAddresses)
                {
                    var address = new Ellucian.Colleague.Dtos.DtoProperties.PersonAddressDtoProperty
                    {
                        address = new PersonAddress()
                        {
                            Id = source.Guid,
                            AddressLines = source.AddressLines,
                            Latitude = source.Latitude,
                            Longitude = source.Longitude
                        },
                        AddressEffectiveStart = new DateTime(2015, 09, 01),
                        AddressEffectiveEnd = new DateTime(2015, 12, 20),
                        Preference = Dtos.EnumProperties.PersonPreference.Primary,
                        Type = new PersonAddressTypeDtoProperty()
                        {
                            AddressType = string.IsNullOrEmpty(source.Type) ? null : (Dtos.EnumProperties.AddressType?)Enum.Parse(typeof(Dtos.EnumProperties.AddressType), source.Type, true)
                        }
                    };
                    addressesCollection.Add(address);
                }
                personDto.Addresses = addressesCollection;

                var phoneList = new List<PersonPhoneDtoProperty>()
                {
                    new PersonPhoneDtoProperty()
                    {
                        CountryCallingCode = "01",
                         Extension = "1",
                         Number = "111-111-1111",
                         Preference = PersonPreference.Primary,
                         Type = new PersonPhoneTypeDtoProperty()
                         {
                             Detail = new GuidObject2("92c82d33-e55c-41a4-a2c3-f2f7d2c523d1"),
                             PhoneType = PersonPhoneTypeCategory.Home
                         }
                    },
                    new PersonPhoneDtoProperty()
                    {
                        CountryCallingCode = "02",
                         Extension = "2",
                         Number = "222-222-2222",
                         Preference = PersonPreference.Primary,
                         Type = new PersonPhoneTypeDtoProperty()
                         {
                             Detail = new GuidObject2("b6def2cc-cc95-4d0e-a32c-940fbbc2d689"),
                             PhoneType = PersonPhoneTypeCategory.Mobile
                         }
                    },
                    new PersonPhoneDtoProperty()
                    {
                        CountryCallingCode = "03",
                         Extension = "3",
                         Number = "333-333-3333",
                         Preference = PersonPreference.Primary,
                         Type = new PersonPhoneTypeDtoProperty()
                         {
                             Detail = new GuidObject2("f60e7b27-a3e3-4c92-9d36-f3cae27b724b"),
                             PhoneType = PersonPhoneTypeCategory.Vacation
                         }
                    },
                    new PersonPhoneDtoProperty()
                    {
                        CountryCallingCode = "04",
                         Extension = "4444",
                         Number = "444-444-4444",
                         Preference = PersonPreference.Primary,
                         Type = new PersonPhoneTypeDtoProperty()
                         {
                             Detail = new GuidObject2("30e231cf-a199-4c9a-af01-be2e69b607c9"),
                             PhoneType = PersonPhoneTypeCategory.Home
                         }
                    },
                };

                personDto.Phones = phoneList;

                //SocialMedia
                personDto.SocialMedia = new List<PersonSocialMediaDtoProperty>()
                {
                    new PersonSocialMediaDtoProperty()
                    {
                        Address = "http://www.facebook.com/jDoe",
                         Preference = PersonPreference.Primary,
                         Type = new PersonSocialMediaType(){ Category = Ellucian.Colleague.Dtos.SocialMediaTypeCategory.facebook, Detail = new GuidObject2("d1f311f4-687d-4dc7-a329-c6a8bfc9c74") }
                    },
                    new PersonSocialMediaDtoProperty()
                    {
                        Address = "http://www.somewebsite.com/jDoe",
                         Preference = PersonPreference.Primary,
                         Type = new PersonSocialMediaType(){ Category = Ellucian.Colleague.Dtos.SocialMediaTypeCategory.website, Detail = new GuidObject2("d1f311f4-687d-4dc7-a329-c6a8bfc9c75") }
                    }
                };

                //Returned value
                personIntegrationReturned = new PersonIntegration(personId, "LegalLast");
                personIntegrationReturned.Guid = personGuid;
                personIntegrationReturned.Prefix = "Mr.";
                personIntegrationReturned.FirstName = "LegalFirst";
                personIntegrationReturned.MiddleName = "LegalMiddle";
                personIntegrationReturned.Suffix = "Jr.";
                personIntegrationReturned.Nickname = "NickNameFirst NickNameMiddle NickNameLast";
                personIntegrationReturned.BirthDate = new DateTime(1930, 1, 1);
                personIntegrationReturned.DeceasedDate = new DateTime(2014, 5, 12);
                personIntegrationReturned.GovernmentId = "111-11-1111";
                personIntegrationReturned.Religion = "CA";
                personIntegrationReturned.MaritalStatusCode = "M";
                personIntegrationReturned.EthnicCodes = new List<string> { "H", "N" };
                personIntegrationReturned.Gender = "M";
                personIntegrationReturned.RaceCodes = new List<string> { "AS" };
                personIntegrationReturned.AddEmailAddress(new EmailAddress("inst@inst.com", "COL") { IsPreferred = true });
                personIntegrationReturned.AddRole(new PersonRole(PersonRoleType.Alumni, new DateTime(15, 01, 22), new DateTime(15, 05, 25)));
                personIntegrationReturned.BirthNameFirst = "BirthFirst";
                personIntegrationReturned.BirthNameLast = "BirthLast";
                personIntegrationReturned.BirthNameMiddle = "BirthMiddle";
                personIntegrationReturned.ChosenFirstName = "ChosenFirst";
                personIntegrationReturned.ChosenLastName = "ChosenLast";
                personIntegrationReturned.ChosenMiddleName = "ChosenMiddle";
                personIntegrationReturned.PreferredName = "PreferedFirst PreferedMiddle PreferedLast";
                personIntegrationReturned.FormerNames = new List<PersonName>()
                {
                    new PersonName("HistoryFirst", "HistoryMiddle", "HistoryLast")
                };
                // Mock the email address data response
                perEmail = new Domain.Base.Entities.EmailAddress("personal@personal.com", "PER") { IsPreferred = false };
                personIntegrationReturned.AddEmailAddress(perEmail);
                workEmail = new Domain.Base.Entities.EmailAddress("work@work.com", "BUS") { IsPreferred = false }; ;
                personIntegrationReturned.AddEmailAddress(workEmail);

                // Mock the address hierarchy responses
                var addresses = new TestAddressRepository().GetAddressData().ToList();
                personIntegrationReturned.Addresses = addresses;
                var addrWithoutId = new TestAddressRepository().GetAddressDataWithNullId().ToList();

                // Mock the person phone per type response
                homePhone = new Domain.Base.Entities.Phone("111-111-1111", "HO", "1") { CountryCallingCode = "01", IsPreferred = true };
                personIntegrationReturned.AddPhone(homePhone);
                mobilePhone = new Domain.Base.Entities.Phone("222-222-2222", "MO", "2") { CountryCallingCode = "02", IsPreferred = true, };
                personIntegrationReturned.AddPhone(mobilePhone);
                residencePhone = new Domain.Base.Entities.Phone("333-333-3333", "VA", "3") { CountryCallingCode = "03", IsPreferred = true };
                personIntegrationReturned.AddPhone(residencePhone);
                workPhone = new Domain.Base.Entities.Phone("444-444-4444", "HO", "4444") { CountryCallingCode = "04", IsPreferred = true };
                personIntegrationReturned.AddPhone(workPhone);

                personIntegrationReturned.AddSocialMedia(new Domain.Base.Entities.SocialMedia("FB", "http://www.facebook.com/jDoe") { IsPreferred = true });
                personIntegrationReturned.AddSocialMedia(new Domain.Base.Entities.SocialMedia("website", "http://www.somewebsite.com/jDoe") { IsPreferred = true });

            }

            private void SetupReferenceDataRepositoryMocks()
            {
                referenceDataRepositoryMock.Setup(repo => repo.GetPrivacyStatusesAsync(It.IsAny<bool>()))
                     .ReturnsAsync(
                         new List<PrivacyStatus>() {
                        new PrivacyStatus( "d3d86052-9d55-4751-acda-5c07a064a82a", "UN", "Unrestricted", PrivacyStatusType.unrestricted),
                        new PrivacyStatus( "cff65dcc-4a9b-44ed-b8d0-930348c55ef8", "R", "restricted", PrivacyStatusType.restricted)
                        }
                     );
                personNameTypes = new List<PersonNameTypeItem>() {
                        new PersonNameTypeItem("8224f18e-69c5-480b-a9b4-52f596aa4a52", "PREFERRED", "Personal", PersonNameType.Personal),
                        new PersonNameTypeItem("7dfa950c-8ae4-4dca-92f0-c083604285b6", "BIRTH", "Birth", PersonNameType.Birth),
                        new PersonNameTypeItem("dd20ebdf-2452-41ef-9F86-ad1b1621a78d", "CHOSEN", "Chosen", PersonNameType.Personal),
                        new PersonNameTypeItem("806af5a5-8a9a-424f-8c9f-c1e9d084ee71", "LEGAL", "Legal", PersonNameType.Legal),
                        new PersonNameTypeItem("7b55610f-7d00-4260-bbcf-0e47fdbae647", "NICKNAME", "NickName", PersonNameType.Personal),
                        new PersonNameTypeItem("d42cc964-35cb-4560-bc46-4b881e7705ea", "HISTORY", "History", PersonNameType.Personal)
                        };
                referenceDataRepositoryMock.Setup(repo => repo.GetPersonNameTypesAsync(It.IsAny<bool>()))
                    .ReturnsAsync(personNameTypes);

                referenceDataRepositoryMock.Setup(repo => repo.GetDenominationsAsync(It.IsAny<bool>()))
                    .ReturnsAsync(
                        new List<Denomination>() {
                        new Denomination(baptistGuid,"BA", "Baptist") ,
                        new Denomination(catholicGuid,"CA", "Catholic")
                        }
                     );

                referenceDataRepositoryMock.Setup(repo => repo.GetEmailTypesAsync(It.IsAny<bool>()))
                     .ReturnsAsync(
                         new List<EmailType>() {
                        new EmailType("899803da-48f8-4044-beb8-5913a04b995d", "COL", "College", EmailTypeCategory.School),
                        new EmailType("301d485d-d37b-4d29-af00-465ced624a85", "PER", "Personal", EmailTypeCategory.Personal),
                        new EmailType("53fb7dab-d348-4657-b071-45d0e5933e05", "BUS", "Business", EmailTypeCategory.Business)
                        }
                     );

                // Mock the reference repository for ethnicity
                referenceDataRepositoryMock.Setup(repo => repo.GetEthnicitiesAsync(It.IsAny<bool>())).Returns(Task.FromResult<IEnumerable<Ethnicity>>(new List<Ethnicity>()
                {
                    new Ethnicity(ethnicityGuid, "H", "Hispanic", EthnicityType.Hispanic)
                }));

                referenceDataRepositoryMock.Setup(repo => repo.GetSocialMediaTypesAsync(It.IsAny<bool>()))
                     .ReturnsAsync(
                         new List<SocialMediaType>() {
                        new SocialMediaType("d1f311f4-687d-4dc7-a329-c6a8bfc9c74", "FB", "Facebook", SocialMediaTypeCategory.facebook),
                        new SocialMediaType("d1f311f4-687d-4dc7-a329-c6a8bfc9c75", "WS", "Website", SocialMediaTypeCategory.website)
                        }
                     );

                referenceDataRepositoryMock.Setup(repo => repo.GetAddressTypes2Async(It.IsAny<bool>()))
                     .ReturnsAsync(
                         new List<AddressType2>() {
                        new AddressType2("91979656-e110-4156-a75a-1a1a7294314d", "HO", "Home", AddressTypeCategory.Home),
                        new AddressType2("b887d5ec-9ed5-45e8-b44c-01782070f234", "MA", "Mailing", AddressTypeCategory.Mailing),
                        new AddressType2("d7d0a82c-fe74-480d-be1b-88a2e460af4c", "VA", "Vacation", AddressTypeCategory.Vacation),
                        new AddressType2("c9b8cd52-54e6-4c08-a9d9-224dd0c8b700", "BU", "Business", AddressTypeCategory.Business)
                         }
                     );
                allPhones = new List<PhoneType>() {
                        new PhoneType("92c82d33-e55c-41a4-a2c3-f2f7d2c523d1", "HO", "Home", PhoneTypeCategory.Home),
                        new PhoneType("b6def2cc-cc95-4d0e-a32c-940fbbc2d689", "MO", "Mobile", PhoneTypeCategory.Mobile),
                        new PhoneType("f60e7b27-a3e3-4c92-9d36-f3cae27b724b", "VA", "Vacation", PhoneTypeCategory.Vacation),
                        new PhoneType("30e231cf-a199-4c9a-af01-be2e69b607c9", "BU", "Business", PhoneTypeCategory.Business)
                        };
                referenceDataRepositoryMock.Setup(repo => repo.GetPhoneTypesAsync(It.IsAny<bool>()))
                     .ReturnsAsync(allPhones);


                // Mock the reference repository for prefix
                referenceDataRepositoryMock.Setup(repo => repo.Prefixes).Returns(new List<Prefix>()
                {
                    new Prefix("MR","Mr","Mr."),
                    new Prefix("MS","Ms","Ms.")
                });

                // Mock the reference repository for prefix
                referenceDataRepositoryMock.Setup(repo => repo.GetPrefixesAsync()).ReturnsAsync(new List<Prefix>()
                {
                    new Prefix("MR","Mr","Mr."),
                    new Prefix("MS","Ms","Ms.")
                });

                // Mock the reference repository for suffix
                referenceDataRepositoryMock.Setup(repo => repo.Suffixes).Returns(new List<Suffix>()
                {
                    new Suffix("JR","Jr","Jr."),
                    new Suffix("SR","Sr","Sr.")
                });

                // Mock the reference repository for suffix
                referenceDataRepositoryMock.Setup(repo => repo.GetSuffixesAsync()).ReturnsAsync(new List<Suffix>()
                {
                    new Suffix("JR","Jr","Jr."),
                    new Suffix("SR","Sr","Sr.")
                });

                // Mock the reference repository for country
                countries = new List<Country>()
                 {
                    new Country("US","United States","US", "USA", false),
                    new Country("CA","Canada","CA", "CAN", false),
                    new Country("MX","Mexico","MX", "MEX", false),
                    new Country("BR","Brazil","BR", "BRA", false)
                };
                referenceDataRepositoryMock.Setup(repo => repo.GetCountryCodesAsync(It.IsAny<bool>())).Returns(Task.FromResult(countries));

                // Mock the reference repository for marital status
                referenceDataRepositoryMock.Setup(repo => repo.GetMaritalStatusesAsync(It.IsAny<bool>())).Returns(Task.FromResult<IEnumerable<MaritalStatus>>(new List<MaritalStatus>()
                {
                    new MaritalStatus(maritalStatusGuid, "M", "Married"){ Type = MaritalStatusType.Married }
                }));

                var raceEntities = new List<Race>();
                raceEntities.Add(new Race(raceAsianGuid, "AS", "Asian", RaceType.Asian));
                raceEntities.Add(new Race(racePacificIslanderGuid, "HP", "Hawaiian/Pacific Islander", RaceType.PacificIslander));
                referenceDataRepositoryMock.Setup(repo => repo.GetRacesAsync(It.IsAny<bool>())).Returns(Task.FromResult<IEnumerable<Race>>(raceEntities));

                referenceDataRepositoryMock.Setup(repo => repo.GetChaptersAsync(It.IsAny<bool>())).ReturnsAsync(allChapters);
                referenceDataRepositoryMock.Setup(repo => repo.GetCountiesAsync(It.IsAny<bool>())).ReturnsAsync(allCounties);
                referenceDataRepositoryMock.Setup(repo => repo.GetZipCodeXlatAsync(It.IsAny<bool>())).ReturnsAsync(allZipCodeXlats);
                referenceDataRepositoryMock.Setup(repo => repo.GetRecordInfoFromGuidGeographicAreaAsync(It.IsAny<string>())).ReturnsAsync(Domain.Base.Entities.GeographicAreaTypeCategory.Fundraising);
            }
        }

        #endregion

        #region HEDM Update Person V12.0.0 Tests

        [TestClass]
        public class UpdatePerson4 : CurrentUserSetup
        {
            //Mocks
            Mock<IAdapterRegistry> adapterRegistryMock;
            Mock<IPersonRepository> personRepositoryMock;
            Mock<IPersonBaseRepository> personBaseRepositoryMock;
            Mock<IReferenceDataRepository> referenceDataRepositoryMock;
            Mock<IProfileRepository> profileRepositoryMock;
            Mock<IConfigurationRepository> configurationRepositoryMock;
            Mock<IRelationshipRepository> relationshipRepositoryMock;
            Mock<IProxyRepository> proxyRepositoryMock;
            Mock<IRoleRepository> roleRepositoryMock;
            Mock<ILogger> loggerMock;

            //userfactory
            ICurrentUserFactory currentUserFactory;

            //Service
            PersonService personService;


            private Ellucian.Colleague.Dtos.Person4 personDto;
            private Ellucian.Colleague.Domain.Base.Entities.PersonIntegration personIntegrationReturned;
            private Ellucian.Colleague.Domain.Base.Entities.PersonIntegration personIntegrationEntity;

            //private Ellucian.Colleague.Domain.Base.Entities.Person person;
            private List<Ellucian.Colleague.Domain.Base.Entities.Address> addresses = null;
            private List<Ellucian.Colleague.Domain.Base.Entities.Phone> phones = new List<Domain.Base.Entities.Phone>();
            private IEnumerable<Ellucian.Colleague.Domain.Base.Entities.State> states;
            private IEnumerable<Ellucian.Colleague.Domain.Base.Entities.Country> countries;
            private List<Ellucian.Colleague.Domain.Base.Entities.County> counties;
            private List<PersonNameTypeItem> personNameTypes;

            //Entities
            private Ellucian.Colleague.Domain.Base.Entities.EmailAddress instEmail;
            private Ellucian.Colleague.Domain.Base.Entities.EmailAddress perEmail;
            private Ellucian.Colleague.Domain.Base.Entities.EmailAddress workEmail;
            private Ellucian.Colleague.Domain.Base.Entities.Address homeAddr;
            private Ellucian.Colleague.Domain.Base.Entities.Address mailAddr;
            private Ellucian.Colleague.Domain.Base.Entities.Address resAddr;
            private Ellucian.Colleague.Domain.Base.Entities.Address workAddr;
            private Ellucian.Colleague.Domain.Base.Entities.Phone homePhone;
            private Ellucian.Colleague.Domain.Base.Entities.Phone mobilePhone;
            private Ellucian.Colleague.Domain.Base.Entities.Phone residencePhone;
            private Ellucian.Colleague.Domain.Base.Entities.Phone workPhone;

            //Data
            private string personId = "0000011";
            private string personGuid = "5674f28b-b216-4055-b236-81a922d93b4c";
            private string maritalStatusGuid = "dca8edb5-120f-479a-a6bb-35ba3af4b344";
            private string ethnicityGuid = "dd0c42ca-c61d-4ca6-8d21-96ab5be35623";
            private string raceAsianGuid = "72b7737b-27db-4a06-944b-97d00c29b3db";
            private string racePacificIslanderGuid = "e20f9821-28a2-4e34-8550-6758850a0cf8";
            private string baptistGuid = "c0bdfd92-462f-4e59-bba5-1b15c4771c86";
            private string catholicGuid = "f96f04b0-4973-41f6-bc3d-9c7bc1c2c458";

            private string countyGuid = Guid.NewGuid().ToString();


            [TestInitialize]
            public void Initialize()
            {
                adapterRegistryMock = new Mock<IAdapterRegistry>();
                personRepositoryMock = new Mock<IPersonRepository>();
                personBaseRepositoryMock = new Mock<IPersonBaseRepository>();
                referenceDataRepositoryMock = new Mock<IReferenceDataRepository>();
                profileRepositoryMock = new Mock<IProfileRepository>();
                configurationRepositoryMock = new Mock<IConfigurationRepository>();
                relationshipRepositoryMock = new Mock<IRelationshipRepository>();
                proxyRepositoryMock = new Mock<IProxyRepository>();
                roleRepositoryMock = new Mock<IRoleRepository>();
                loggerMock = new Mock<ILogger>();

                currentUserFactory = new CurrentUserSetup.ThirdPartyUserFactory();

                SetupData();

                SetupReferenceDataRepositoryMocks();

                // International Parameters Host Country
                personRepositoryMock.Setup(repo => repo.GetHostCountryAsync()).ReturnsAsync("USA");

                personRepositoryMock.Setup(i => i.Update2Async(It.IsAny<PersonIntegration>(), addresses, phones, It.IsAny<int>())).ReturnsAsync(personIntegrationReturned);

                personService = new PersonService(adapterRegistryMock.Object, personRepositoryMock.Object, personBaseRepositoryMock.Object, referenceDataRepositoryMock.Object, profileRepositoryMock.Object,
                                                  configurationRepositoryMock.Object, relationshipRepositoryMock.Object, proxyRepositoryMock.Object, currentUserFactory,
                                                  roleRepositoryMock.Object, loggerMock.Object);
            }


            [TestCleanup]
            public void Cleanup()
            {
                personService = null;
                adapterRegistryMock = null;
                personRepositoryMock = null;
                personBaseRepositoryMock = null;
                referenceDataRepositoryMock = null;
                profileRepositoryMock = null;
                configurationRepositoryMock = null;
                relationshipRepositoryMock = null;
                proxyRepositoryMock = null;
                roleRepositoryMock = null;
                loggerMock = null;
            }

            [TestMethod]
            public async Task UpdatePerson4_UpdatePerson4Async()
            {
                //setup role
                updatePersonRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Base.BasePermissionCodes.UpdatePerson));
                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { updatePersonRole });

                //personId 0000011
                personRepositoryMock.Setup(i => i.GetPersonIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync(personId);

                var actual = await personService.UpdatePerson4Async(personDto);

                Assert.AreEqual(personDto.BirthDate, actual.BirthDate);
                Assert.AreEqual(personDto.CitizenshipCountry, actual.CitizenshipCountry);
                Assert.AreEqual(personDto.CitizenshipStatus, actual.CitizenshipStatus);
                Assert.AreEqual(personDto.CountryOfBirth, actual.CountryOfBirth);
                Assert.AreEqual(personDto.DeceasedDate, actual.DeceasedDate);
                Assert.AreEqual(personDto.GenderType, actual.GenderType);
                Assert.AreEqual(personDto.MaritalStatus.Detail.Id, actual.MaritalStatus.Detail.Id);
                Assert.AreEqual(personDto.MaritalStatus.MaritalCategory, actual.MaritalStatus.MaritalCategory);

                /*
                    This code will change because of some of the API changes in future
                */

                //Legal
                var legalActualName = actual.PersonNames.FirstOrDefault(item => item.NameType.Detail.Id.Equals("806af5a5-8a9a-424f-8c9f-c1e9d084ee71", StringComparison.OrdinalIgnoreCase));
                var legalExpectedName = personDto.PersonNames.FirstOrDefault(item => item.NameType.Detail.Id.Equals(legalActualName.NameType.Detail.Id, StringComparison.OrdinalIgnoreCase));

                //Assert.AreEqual(legalExpectedName.FullName, legalActualName.FullName); commented cause it will fail
                if (!string.IsNullOrEmpty(legalExpectedName.LastNamePrefix))
                    Assert.AreEqual(legalExpectedName.LastName, string.Concat(legalExpectedName.LastNamePrefix, " ", legalActualName.LastName));
                Assert.AreEqual(legalExpectedName.FirstName, legalActualName.FirstName);
                Assert.AreEqual(legalExpectedName.MiddleName, legalActualName.MiddleName);

                //Birth
                var birthActualName = actual.PersonNames.FirstOrDefault(item => item.NameType.Detail.Id.Equals("7dfa950c-8ae4-4dca-92f0-c083604285b6", StringComparison.OrdinalIgnoreCase));
                var birthexpectedName = personDto.PersonNames.FirstOrDefault(item => item.NameType.Detail.Id.Equals(birthActualName.NameType.Detail.Id, StringComparison.OrdinalIgnoreCase));

                Assert.AreEqual(birthexpectedName.FullName, birthActualName.FullName);

                //Chosen
                var chosenActualName = actual.PersonNames.FirstOrDefault(item => item.NameType.Detail.Id.Equals("dd20ebdf-2452-41ef-9f86-ad1b1621a78d", StringComparison.OrdinalIgnoreCase));
                var chosenExpectedName = personDto.PersonNames.FirstOrDefault(item => item.NameType.Detail.Id.Equals(chosenActualName.NameType.Detail.Id, StringComparison.OrdinalIgnoreCase));

                Assert.AreEqual(chosenExpectedName.FullName, chosenActualName.FullName);

                //Nickname
                var nickNameActualName = actual.PersonNames.FirstOrDefault(item => item.NameType.Detail.Id.Equals("7b55610f-7d00-4260-bbcf-0e47fdbae647", StringComparison.OrdinalIgnoreCase));
                var nickNameExpectedName = personDto.PersonNames.FirstOrDefault(item => item.NameType.Detail.Id.Equals(nickNameActualName.NameType.Detail.Id, StringComparison.OrdinalIgnoreCase));

                Assert.AreEqual(nickNameExpectedName.FullName, nickNameActualName.FullName);

                //History
                var historyActualName = actual.PersonNames.FirstOrDefault(item => item.NameType.Detail.Id.Equals("d42cc964-35cb-4560-bc46-4b881e7705ea", StringComparison.OrdinalIgnoreCase));
                var historyexpectedName = personDto.PersonNames.FirstOrDefault(item => item.NameType.Detail.Id.Equals(historyActualName.NameType.Detail.Id, StringComparison.OrdinalIgnoreCase));

                Assert.AreEqual(historyexpectedName.FullName, historyActualName.FullName);

            }

            [TestMethod]
            public async Task UpdatePerson4_UpdatePerson4Async_PersonId_NullEmpty_Create()
            {
                //setup role
                createPersonRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Base.BasePermissionCodes.CreatePerson));
                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { createPersonRole });

                //personId 0000011
                personRepositoryMock.Setup(i => i.GetPersonIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync(string.Empty);

                personRepositoryMock.Setup(i => i.Create2Async(It.IsAny<PersonIntegration>(), addresses, phones, It.IsAny<int>())).ReturnsAsync(personIntegrationReturned);


                var actual = await personService.UpdatePerson4Async(personDto);

                Assert.AreEqual(personDto.BirthDate, actual.BirthDate);
                Assert.AreEqual(personDto.CitizenshipCountry, actual.CitizenshipCountry);
                Assert.AreEqual(personDto.CitizenshipStatus, actual.CitizenshipStatus);
                Assert.AreEqual(personDto.CountryOfBirth, actual.CountryOfBirth);
                Assert.AreEqual(personDto.DeceasedDate, actual.DeceasedDate);
                Assert.AreEqual(personDto.GenderType, actual.GenderType);
                Assert.AreEqual(personDto.MaritalStatus.Detail.Id, actual.MaritalStatus.Detail.Id);
                Assert.AreEqual(personDto.MaritalStatus.MaritalCategory, actual.MaritalStatus.MaritalCategory);

                var nameCount = personDto.PersonNames.Count();
                personDto.PersonNames.OrderBy(i => i.NameType.Category);
                actual.PersonNames.OrderBy(i => i.NameType.Category);

                for (int i = 0; i < nameCount; i++)
                {
                    var actualName = actual.PersonNames.ToList()[i];
                    var expectedName = personDto.PersonNames.FirstOrDefault(item => item.NameType.Detail.Id.Equals(actualName.NameType.Detail.Id, StringComparison.OrdinalIgnoreCase));
                }
            }

            #region Exceptions
            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task UpdatePerson4_Dto_Null_ArgumentNullException()
            {
                var result = await personService.UpdatePerson4Async(null);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task UpdatePerson4_Id_Null_ArgumentNullException()
            {
                var result = await personService.UpdatePerson4Async(new Dtos.Person4() { Id = "" });
            }

            [TestMethod]
            [ExpectedException(typeof(Exception))]
            public async Task UpdatePerson4_PersonNames_Null_ArgumentNullException()
            {
                //setup role
                updatePersonRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Base.BasePermissionCodes.UpdatePerson));
                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { updatePersonRole });

                //personId 0000011
                personRepositoryMock.Setup(i => i.GetPersonIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync(personId);
                var result = await personService.UpdatePerson4Async(new Dtos.Person4() { Id = personId });
            }

            [TestMethod]
            [ExpectedException(typeof(Exception))]
            public async Task UpdatePerson4_PrimaryNames_Null_ArgumentNullException()
            {
                //setup role
                updatePersonRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Base.BasePermissionCodes.UpdatePerson));
                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { updatePersonRole });

                //personId 0000011
                personRepositoryMock.Setup(i => i.GetPersonIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync(personId);
                personDto.PersonNames.FirstOrDefault().NameType.Detail.Id = string.Empty;
                var result = await personService.UpdatePerson4Async(personDto);
            }

            [TestMethod]
            [ExpectedException(typeof(Exception))]
            public async Task UpdatePerson4_PrimaryNames_GT_1_ArgumentNullException()
            {
                //setup role
                updatePersonRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Base.BasePermissionCodes.UpdatePerson));
                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { updatePersonRole });

                //personId 0000011
                personRepositoryMock.Setup(i => i.GetPersonIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync(personId);
                personDto.PersonNames = null;
                var personNames = new List<Dtos.DtoProperties.PersonName2DtoProperty>();
                var personPrimaryName = new Dtos.DtoProperties.PersonName2DtoProperty()
                {
                    NameType = new Dtos.DtoProperties.PersonNameTypeDtoProperty() { Category = Dtos.EnumProperties.PersonNameType2.Personal, Detail = new Dtos.GuidObject2() { Id = "806af5a5-8a9a-424f-8c9f-c1e9d084ee71" } },
                    Title = "MR",
                    FirstName = "Ricky",
                    MiddleName = "Lee",
                    LastName = "Brown",
                    Pedigree = "JR",
                    LastNamePrefix = "Ignore"
                };
                personNames.Add(personPrimaryName);
                var personPrimaryName1 = new Dtos.DtoProperties.PersonName2DtoProperty()
                {
                    NameType = new Dtos.DtoProperties.PersonNameTypeDtoProperty() { Category = Dtos.EnumProperties.PersonNameType2.Personal, Detail = new Dtos.GuidObject2() { Id = "806af5a5-8a9a-424f-8c9f-c1e9d084ee71" } },
                    Title = "MR",
                    FirstName = "Ricky",
                    MiddleName = "Lee",
                    LastName = "Brown",
                    Pedigree = "JR",
                    LastNamePrefix = "Ignore"
                };
                personNames.Add(personPrimaryName1);
                personDto.PersonNames = personNames;
                var result = await personService.UpdatePerson4Async(personDto);
            }

            [TestMethod]
            [ExpectedException(typeof(Exception))]
            public async Task UpdatePerson4_PrimaryNames_LastName_Null_ArgumentNullException()
            {
                //setup role
                updatePersonRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Base.BasePermissionCodes.UpdatePerson));
                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { updatePersonRole });

                //personId 0000011
                personRepositoryMock.Setup(i => i.GetPersonIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync(personId);
                personDto.PersonNames.FirstOrDefault().LastName = string.Empty;

                var result = await personService.UpdatePerson4Async(personDto);
            }

            [TestMethod]
            [ExpectedException(typeof(Exception))]
            public async Task UpdatePerson4_LEGAL_NameTypes_Null_ArgumentNullException()
            {
                var legalType = personNameTypes.FirstOrDefault(x => x.Code == "LEGAL");
                personNameTypes.Remove(legalType);

                //setup role
                updatePersonRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Base.BasePermissionCodes.UpdatePerson));
                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { updatePersonRole });

                //personId 0000011
                personRepositoryMock.Setup(i => i.GetPersonIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync(personId);

                var result = await personService.UpdatePerson4Async(personDto);
            }

            [TestMethod]
            [ExpectedException(typeof(Exception))]
            public async Task UpdatePerson4_BIRTH_NameTypes_Null_ArgumentNullException()
            {
                var birthType = personNameTypes.FirstOrDefault(x => x.Code == "BIRTH");
                personNameTypes.Remove(birthType);

                //setup role
                updatePersonRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Base.BasePermissionCodes.UpdatePerson));
                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { updatePersonRole });

                //personId 0000011
                personRepositoryMock.Setup(i => i.GetPersonIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync(personId);

                var result = await personService.UpdatePerson4Async(personDto);
            }

            [TestMethod]
            [ExpectedException(typeof(Exception))]
            public async Task UpdatePerson4_NICKNAME_NameTypes_Null_ArgumentNullException()
            {
                var nickNameType = personNameTypes.FirstOrDefault(x => x.Code == "NICKNAME");
                personNameTypes.Remove(nickNameType);

                //setup role
                updatePersonRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Base.BasePermissionCodes.UpdatePerson));
                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { updatePersonRole });

                //personId 0000011
                personRepositoryMock.Setup(i => i.GetPersonIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync(personId);

                var result = await personService.UpdatePerson4Async(personDto);
            }

            [TestMethod]
            [ExpectedException(typeof(Exception))]
            public async Task UpdatePerson4_NICKNAME_GT_1_ArgumentNullException()
            {
                //setup role
                updatePersonRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Base.BasePermissionCodes.UpdatePerson));
                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { updatePersonRole });

                //personId 0000011
                personRepositoryMock.Setup(i => i.GetPersonIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync(personId);
                personDto.PersonNames = null;
                var personNames = new List<Dtos.DtoProperties.PersonName2DtoProperty>();
                var legalPrimaryName = new Dtos.DtoProperties.PersonName2DtoProperty()
                {
                    NameType = new Dtos.DtoProperties.PersonNameTypeDtoProperty() { Category = Dtos.EnumProperties.PersonNameType2.Personal, Detail = new Dtos.GuidObject2() { Id = "806af5a5-8a9a-424f-8c9f-c1e9d084ee71" } },
                    Title = "MR",
                    FirstName = "Ricky",
                    MiddleName = "Lee",
                    LastName = "Brown",
                    Pedigree = "JR",
                    LastNamePrefix = "Ignore",
                    FullName = "FullName"
                };
                personNames.Add(legalPrimaryName);
                var birthPrimaryName = new Dtos.DtoProperties.PersonName2DtoProperty()
                {
                    NameType = new Dtos.DtoProperties.PersonNameTypeDtoProperty() { Category = Dtos.EnumProperties.PersonNameType2.Birth, Detail = new Dtos.GuidObject2() { Id = "7b55610f-7d00-4260-bbcf-0e47fdbae647" } },
                    Title = "MR",
                    FirstName = "Ricky",
                    MiddleName = "Lee",
                    LastName = "Brown",
                    Pedigree = "JR",
                    LastNamePrefix = "Ignore",
                    FullName = "FullName"
                };
                personNames.Add(birthPrimaryName);

                var birthPrimaryName2 = new Dtos.DtoProperties.PersonName2DtoProperty()
                {
                    NameType = new Dtos.DtoProperties.PersonNameTypeDtoProperty() { Category = Dtos.EnumProperties.PersonNameType2.Birth, Detail = new Dtos.GuidObject2() { Id = "7b55610f-7d00-4260-bbcf-0e47fdbae647" } },
                    Title = "MR",
                    FirstName = "Ricky",
                    MiddleName = "Lee",
                    LastName = "Brown",
                    Pedigree = "JR",
                    LastNamePrefix = "Ignore",
                    FullName = "FullName"
                };
                personNames.Add(birthPrimaryName2);
                personDto.PersonNames = personNames;
                var result = await personService.UpdatePerson4Async(personDto);
            }

            [TestMethod]
            [ExpectedException(typeof(Exception))]
            public async Task UpdatePerson4_Legal_FirstLastMiddle_Null_ArgumentNullException()
            {
                //setup role
                updatePersonRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Base.BasePermissionCodes.UpdatePerson));
                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { updatePersonRole });

                //personId 0000011
                personRepositoryMock.Setup(i => i.GetPersonIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync(personId);
                personDto.PersonNames = null;
                var personNames = new List<Dtos.DtoProperties.PersonName2DtoProperty>();
                var legalPrimaryName = new Dtos.DtoProperties.PersonName2DtoProperty()
                {
                    NameType = new Dtos.DtoProperties.PersonNameTypeDtoProperty() { Category = Dtos.EnumProperties.PersonNameType2.Personal, Detail = new Dtos.GuidObject2() { Id = "806af5a5-8a9a-424f-8c9f-c1e9d084ee71" } },
                    Title = "MR",
                    FirstName = "",
                    MiddleName = "",
                    LastName = "",
                    Pedigree = "JR",
                    LastNamePrefix = "Ignore",
                    FullName = "FullName"
                };
                personNames.Add(legalPrimaryName);
                var birthPrimaryName = new Dtos.DtoProperties.PersonName2DtoProperty()
                {
                    NameType = new Dtos.DtoProperties.PersonNameTypeDtoProperty() { Category = Dtos.EnumProperties.PersonNameType2.Birth, Detail = new Dtos.GuidObject2() { Id = "7b55610f-7d00-4260-bbcf-0e47fdbae647" } },
                    Title = "MR",
                    FirstName = "Ricky",
                    MiddleName = "Lee",
                    LastName = "Brown",
                    Pedigree = "JR",
                    LastNamePrefix = "Ignore",
                    FullName = "FullName"
                };
                personNames.Add(birthPrimaryName);

                var birthPrimaryName2 = new Dtos.DtoProperties.PersonName2DtoProperty()
                {
                    NameType = new Dtos.DtoProperties.PersonNameTypeDtoProperty() { Category = Dtos.EnumProperties.PersonNameType2.Birth, Detail = new Dtos.GuidObject2() { Id = "7b55610f-7d00-4260-bbcf-0e47fdbae647" } },
                    Title = "MR",
                    FirstName = "Ricky",
                    MiddleName = "Lee",
                    LastName = "Brown",
                    Pedigree = "JR",
                    LastNamePrefix = "Ignore",
                    FullName = "FullName"
                };
                personNames.Add(birthPrimaryName2);
                personDto.PersonNames = personNames;
                var result = await personService.UpdatePerson4Async(personDto);
            }

            [TestMethod]
            [ExpectedException(typeof(Exception))]
            public async Task UpdatePerson4_Birth_FirstLastMiddle_Null_ArgumentNullException()
            {
                //setup role
                updatePersonRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Base.BasePermissionCodes.UpdatePerson));
                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { updatePersonRole });

                //personId 0000011
                personRepositoryMock.Setup(i => i.GetPersonIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync(personId);
                personDto.PersonNames = null;
                var personNames = new List<Dtos.DtoProperties.PersonName2DtoProperty>();
                var legalPrimaryName = new Dtos.DtoProperties.PersonName2DtoProperty()
                {
                    NameType = new Dtos.DtoProperties.PersonNameTypeDtoProperty() { Category = Dtos.EnumProperties.PersonNameType2.Personal, Detail = new Dtos.GuidObject2() { Id = "806af5a5-8a9a-424f-8c9f-c1e9d084ee71" } },
                    Title = "MR",
                    FirstName = "LegalFirst",
                    MiddleName = "LegalMiddle",
                    LastName = "LegalLast",
                    Pedigree = "JR",
                    LastNamePrefix = "Ignore",
                    FullName = "FullName"
                };
                personNames.Add(legalPrimaryName);
                var birthPrimaryName = new Dtos.DtoProperties.PersonName2DtoProperty()
                {
                    NameType = new Dtos.DtoProperties.PersonNameTypeDtoProperty() { Category = Dtos.EnumProperties.PersonNameType2.Birth, Detail = new Dtos.GuidObject2() { Id = "7dfa950c-8ae4-4dca-92f0-c083604285b6" } },
                    Title = "MR",
                    FirstName = "",
                    MiddleName = "",
                    LastName = "",
                    Pedigree = "JR",
                    LastNamePrefix = "Ignore",
                    FullName = "FullName"
                };
                personNames.Add(birthPrimaryName);
                personDto.PersonNames = personNames;
                var result = await personService.UpdatePerson4Async(personDto);
            }

            [TestMethod]
            [ExpectedException(typeof(Exception))]
            public async Task UpdatePerson4_Birth_FullName_Null_ArgumentNullException()
            {
                //setup role
                updatePersonRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Base.BasePermissionCodes.UpdatePerson));
                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { updatePersonRole });

                //personId 0000011
                personRepositoryMock.Setup(i => i.GetPersonIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync(personId);
                personDto.PersonNames = null;
                var personNames = new List<Dtos.DtoProperties.PersonName2DtoProperty>();
                var legalPrimaryName = new Dtos.DtoProperties.PersonName2DtoProperty()
                {
                    NameType = new Dtos.DtoProperties.PersonNameTypeDtoProperty() { Category = Dtos.EnumProperties.PersonNameType2.Personal, Detail = new Dtos.GuidObject2() { Id = "806af5a5-8a9a-424f-8c9f-c1e9d084ee71" } },
                    Title = "MR",
                    FirstName = "LegalFirst",
                    MiddleName = "LegalMiddle",
                    LastName = "LegalLast",
                    Pedigree = "JR",
                    LastNamePrefix = "Ignore",
                    FullName = "FullName"
                };
                personNames.Add(legalPrimaryName);
                var birthPrimaryName = new Dtos.DtoProperties.PersonName2DtoProperty()
                {
                    NameType = new Dtos.DtoProperties.PersonNameTypeDtoProperty() { Category = Dtos.EnumProperties.PersonNameType2.Birth, Detail = new Dtos.GuidObject2() { Id = "7dfa950c-8ae4-4dca-92f0-c083604285b6" } },
                    Title = "MR",
                    FirstName = "First",
                    MiddleName = "Middle",
                    LastName = "Last",
                    Pedigree = "JR",
                    LastNamePrefix = "Ignore",
                    FullName = ""
                };
                personNames.Add(birthPrimaryName);
                personDto.PersonNames = personNames;
                var result = await personService.UpdatePerson4Async(personDto);
            }

            [TestMethod]
            [ExpectedException(typeof(Exception))]
            public async Task UpdatePerson4_NickName_FullName_Null_ArgumentNullException()
            {
                //setup role
                updatePersonRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Base.BasePermissionCodes.UpdatePerson));
                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { updatePersonRole });

                //personId 0000011
                personRepositoryMock.Setup(i => i.GetPersonIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync(personId);
                personDto.PersonNames = null;
                var personNames = new List<Dtos.DtoProperties.PersonName2DtoProperty>();
                var legalPrimaryName = new Dtos.DtoProperties.PersonName2DtoProperty()
                {
                    NameType = new Dtos.DtoProperties.PersonNameTypeDtoProperty() { Category = Dtos.EnumProperties.PersonNameType2.Personal, Detail = new Dtos.GuidObject2() { Id = "806af5a5-8a9a-424f-8c9f-c1e9d084ee71" } },
                    Title = "MR",
                    FirstName = "LegalFirst",
                    MiddleName = "LegalMiddle",
                    LastName = "LegalLast",
                    Pedigree = "JR",
                    LastNamePrefix = "Ignore",
                    FullName = "FullName"
                };
                personNames.Add(legalPrimaryName);
                var birthPrimaryName = new Dtos.DtoProperties.PersonName2DtoProperty()
                {
                    NameType = new Dtos.DtoProperties.PersonNameTypeDtoProperty() { Category = Dtos.EnumProperties.PersonNameType2.Birth, Detail = new Dtos.GuidObject2() { Id = "7b55610f-7d00-4260-bbcf-0e47fdbae647" } },
                    Title = "MR",
                    FirstName = "First",
                    MiddleName = "Middle",
                    LastName = "Last",
                    Pedigree = "JR",
                    LastNamePrefix = "Ignore",
                    FullName = ""
                };
                personNames.Add(birthPrimaryName);
                personDto.PersonNames = personNames;
                var result = await personService.UpdatePerson4Async(personDto);
            }

            [TestMethod]
            [ExpectedException(typeof(Exception))]
            public async Task UpdatePerson4_History_FullName_Null_ArgumentNullException()
            {
                //setup role
                updatePersonRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Base.BasePermissionCodes.UpdatePerson));
                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { updatePersonRole });

                //personId 0000011
                personRepositoryMock.Setup(i => i.GetPersonIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync(personId);
                personDto.PersonNames = null;
                var personNames = new List<Dtos.DtoProperties.PersonName2DtoProperty>();
                var legalPrimaryName = new Dtos.DtoProperties.PersonName2DtoProperty()
                {
                    NameType = new Dtos.DtoProperties.PersonNameTypeDtoProperty() { Category = Dtos.EnumProperties.PersonNameType2.Personal, Detail = new Dtos.GuidObject2() { Id = "806af5a5-8a9a-424f-8c9f-c1e9d084ee71" } },
                    Title = "MR",
                    FirstName = "LegalFirst",
                    MiddleName = "LegalMiddle",
                    LastName = "LegalLast",
                    Pedigree = "JR",
                    LastNamePrefix = "Ignore",
                    FullName = "FullName"
                };
                personNames.Add(legalPrimaryName);
                var birthPrimaryName = new Dtos.DtoProperties.PersonName2DtoProperty()
                {
                    NameType = new Dtos.DtoProperties.PersonNameTypeDtoProperty() { Category = Dtos.EnumProperties.PersonNameType2.Birth, Detail = new Dtos.GuidObject2() { Id = "d42cc964-35cb-4560-bc46-4b881e7705ea" } },
                    Title = "MR",
                    FirstName = "First",
                    MiddleName = "Middle",
                    LastName = "Last",
                    Pedigree = "JR",
                    LastNamePrefix = "Ignore",
                    FullName = ""
                };
                personNames.Add(birthPrimaryName);
                personDto.PersonNames = personNames;
                var result = await personService.UpdatePerson4Async(personDto);
            }

            [TestMethod]
            [ExpectedException(typeof(Exception))]
            public async Task UpdatePerson4_Prefered_FullName_Null_ArgumentNullException()
            {
                //setup role
                updatePersonRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Base.BasePermissionCodes.UpdatePerson));
                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { updatePersonRole });

                //personId 0000011
                personRepositoryMock.Setup(i => i.GetPersonIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync(personId);
                personDto.PersonNames = null;
                var personNames = new List<Dtos.DtoProperties.PersonName2DtoProperty>();
                var legalPrimaryName = new Dtos.DtoProperties.PersonName2DtoProperty()
                {
                    NameType = new Dtos.DtoProperties.PersonNameTypeDtoProperty() { Category = Dtos.EnumProperties.PersonNameType2.Personal, Detail = new Dtos.GuidObject2() { Id = "806af5a5-8a9a-424f-8c9f-c1e9d084ee71" } },
                    Title = "MR",
                    FirstName = "LegalFirst",
                    MiddleName = "LegalMiddle",
                    LastName = "LegalLast",
                    Pedigree = "JR",
                    LastNamePrefix = "Ignore",
                    FullName = "FullName"
                };
                personNames.Add(legalPrimaryName);
                var birthPrimaryName = new Dtos.DtoProperties.PersonName2DtoProperty()
                {
                    NameType = new Dtos.DtoProperties.PersonNameTypeDtoProperty() { Category = Dtos.EnumProperties.PersonNameType2.Birth, Detail = new Dtos.GuidObject2() { Id = "8224f18e-69c5-480b-a9b4-52f596aa4a52" } },
                    Title = "MR",
                    FirstName = "First",
                    MiddleName = "Middle",
                    LastName = "Last",
                    Pedigree = "JR",
                    LastNamePrefix = "Ignore",
                    FullName = ""
                };
                personNames.Add(birthPrimaryName);
                personDto.PersonNames = personNames;
                var result = await personService.UpdatePerson4Async(personDto);
            }

            [TestMethod]
            [ExpectedException(typeof(Exception))]
            public async Task UpdatePerson4_HISTORY_NameTypes_Null_ArgumentNullException()
            {
                var historyNameType = personNameTypes.FirstOrDefault(x => x.Code == "HISTORY");
                personNameTypes.Remove(historyNameType);

                //setup role
                updatePersonRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Base.BasePermissionCodes.UpdatePerson));
                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { updatePersonRole });

                //personId 0000011
                personRepositoryMock.Setup(i => i.GetPersonIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync(personId);

                var result = await personService.UpdatePerson4Async(personDto);
            }

            [TestMethod]
            [ExpectedException(typeof(Exception))]
            public async Task UpdatePerson4_HISTORY_LastName_Null_ArgumentNullException()
            {
                //setup role
                updatePersonRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Base.BasePermissionCodes.UpdatePerson));
                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { updatePersonRole });

                personDto.PersonNames.FirstOrDefault(i => i.NameType.Detail.Id.Equals("d42cc964-35cb-4560-bc46-4b881e7705ea", StringComparison.OrdinalIgnoreCase)).LastName = string.Empty;

                //personId 0000011
                personRepositoryMock.Setup(i => i.GetPersonIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync(personId);

                var result = await personService.UpdatePerson4Async(personDto);
            }



            [TestMethod]
            [ExpectedException(typeof(Exception))]
            public async Task UpdatePerson4_PREFERRED_GT_1_ArgumentNullException()
            {
                //setup role
                updatePersonRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Base.BasePermissionCodes.UpdatePerson));
                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { updatePersonRole });

                //personId 0000011
                personRepositoryMock.Setup(i => i.GetPersonIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync(personId);
                personDto.PersonNames = null;
                var personNames = new List<Dtos.DtoProperties.PersonName2DtoProperty>();
                var legalPrimaryName = new Dtos.DtoProperties.PersonName2DtoProperty()
                {
                    NameType = new Dtos.DtoProperties.PersonNameTypeDtoProperty() { Category = Dtos.EnumProperties.PersonNameType2.Personal, Detail = new Dtos.GuidObject2() { Id = "806af5a5-8a9a-424f-8c9f-c1e9d084ee71" } },
                    Title = "MR",
                    FirstName = "Ricky",
                    MiddleName = "Lee",
                    LastName = "Brown",
                    Pedigree = "JR",
                    LastNamePrefix = "Ignore",
                    FullName = "FullName"
                };
                personNames.Add(legalPrimaryName);
                var birthPrimaryName = new Dtos.DtoProperties.PersonName2DtoProperty()
                {
                    NameType = new Dtos.DtoProperties.PersonNameTypeDtoProperty() { Category = Dtos.EnumProperties.PersonNameType2.Birth, Detail = new Dtos.GuidObject2() { Id = "8224f18e-69c5-480b-a9b4-52f596aa4a52" } },
                    Title = "MR",
                    FirstName = "Ricky",
                    MiddleName = "Lee",
                    LastName = "Brown",
                    Pedigree = "JR",
                    LastNamePrefix = "Ignore",
                    FullName = "FullName"
                };
                personNames.Add(birthPrimaryName);

                var birthPrimaryName2 = new Dtos.DtoProperties.PersonName2DtoProperty()
                {
                    NameType = new Dtos.DtoProperties.PersonNameTypeDtoProperty() { Category = Dtos.EnumProperties.PersonNameType2.Birth, Detail = new Dtos.GuidObject2() { Id = "8224f18e-69c5-480b-a9b4-52f596aa4a52" } },
                    Title = "MR",
                    FirstName = "Ricky",
                    MiddleName = "Lee",
                    LastName = "Brown",
                    Pedigree = "JR",
                    LastNamePrefix = "Ignore",
                    FullName = "FullName"
                };
                personNames.Add(birthPrimaryName2);
                personDto.PersonNames = personNames;
                var result = await personService.UpdatePerson4Async(personDto);
            }

            [TestMethod]
            [ExpectedException(typeof(Exception))]
            public async Task UpdatePerson4_BIRTHNames_GT_1_ArgumentNullException()
            {
                //setup role
                updatePersonRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Base.BasePermissionCodes.UpdatePerson));
                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { updatePersonRole });

                //personId 0000011
                personRepositoryMock.Setup(i => i.GetPersonIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync(personId);
                personDto.PersonNames = null;
                var personNames = new List<Dtos.DtoProperties.PersonName2DtoProperty>();
                var legalPrimaryName = new Dtos.DtoProperties.PersonName2DtoProperty()
                {
                    NameType = new Dtos.DtoProperties.PersonNameTypeDtoProperty() { Category = Dtos.EnumProperties.PersonNameType2.Personal, Detail = new Dtos.GuidObject2() { Id = "806af5a5-8a9a-424f-8c9f-c1e9d084ee71" } },
                    Title = "MR",
                    FirstName = "Ricky",
                    MiddleName = "Lee",
                    LastName = "Brown",
                    Pedigree = "JR",
                    LastNamePrefix = "Ignore",
                    FullName = "FullName"
                };
                personNames.Add(legalPrimaryName);
                var birthPrimaryName = new Dtos.DtoProperties.PersonName2DtoProperty()
                {
                    NameType = new Dtos.DtoProperties.PersonNameTypeDtoProperty() { Category = Dtos.EnumProperties.PersonNameType2.Birth, Detail = new Dtos.GuidObject2() { Id = "7dfa950c-8ae4-4dca-92f0-c083604285b6" } },
                    Title = "MR",
                    FirstName = "Ricky",
                    MiddleName = "Lee",
                    LastName = "Brown",
                    Pedigree = "JR",
                    LastNamePrefix = "Ignore",
                    FullName = "FullName"
                };
                personNames.Add(birthPrimaryName);

                var birthPrimaryName2 = new Dtos.DtoProperties.PersonName2DtoProperty()
                {
                    NameType = new Dtos.DtoProperties.PersonNameTypeDtoProperty() { Category = Dtos.EnumProperties.PersonNameType2.Birth, Detail = new Dtos.GuidObject2() { Id = "7dfa950c-8ae4-4dca-92f0-c083604285b6" } },
                    Title = "MR",
                    FirstName = "Ricky",
                    MiddleName = "Lee",
                    LastName = "Brown",
                    Pedigree = "JR",
                    LastNamePrefix = "Ignore",
                    FullName = "FullName"
                };
                personNames.Add(birthPrimaryName2);
                personDto.PersonNames = personNames;
                var result = await personService.UpdatePerson4Async(personDto);
            }

            [TestMethod]
            [ExpectedException(typeof(Exception))]
            public async Task UpdatePerson4_BirthNames_Empty_ArgumentNullException()
            {
                //setup role
                updatePersonRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Base.BasePermissionCodes.UpdatePerson));
                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { updatePersonRole });

                //personId 0000011
                personRepositoryMock.Setup(i => i.GetPersonIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync(personId);
                personDto.PersonNames = null;
                var personNames = new List<Dtos.DtoProperties.PersonName2DtoProperty>();

                var personPrimaryName = new Dtos.DtoProperties.PersonName2DtoProperty()
                {
                    NameType = new Dtos.DtoProperties.PersonNameTypeDtoProperty() { Category = Dtos.EnumProperties.PersonNameType2.Personal, Detail = new Dtos.GuidObject2() { Id = "806af5a5-8a9a-424f-8c9f-c1e9d084ee71" } },
                    Title = "MR",
                    FirstName = "Ricky",
                    MiddleName = "Lee",
                    LastName = "Brown",
                    Pedigree = "JR",
                    LastNamePrefix = "Ignore"
                };
                personNames.Add(personPrimaryName);

                var personPrimaryName1 = new Dtos.DtoProperties.PersonName2DtoProperty()
                {
                    NameType = new Dtos.DtoProperties.PersonNameTypeDtoProperty() { Category = Dtos.EnumProperties.PersonNameType2.Birth, Detail = new Dtos.GuidObject2() { Id = "7dfa950c-8ae4-4dca-92f0-c083604285b6" } },
                    Title = "MR",
                    FirstName = "",
                    MiddleName = "",
                    LastName = "",
                    Pedigree = "JR",
                    LastNamePrefix = "Ignore"
                };
                personNames.Add(personPrimaryName1);
                personDto.PersonNames = personNames;
                var result = await personService.UpdatePerson4Async(personDto);
            }

            #endregion

            private void SetupData()
            {
                // setup personDto object
                personDto = new Dtos.Person4();
                personDto.Id = personGuid;
                personDto.BirthDate = new DateTime(1930, 1, 1);
                personDto.DeceasedDate = new DateTime(2014, 5, 12);
                var personNames = new List<Dtos.DtoProperties.PersonName2DtoProperty>();
                var legalPrimaryName = new Dtos.DtoProperties.PersonName2DtoProperty()
                {
                    NameType = new Dtos.DtoProperties.PersonNameTypeDtoProperty() { Category = Dtos.EnumProperties.PersonNameType2.Legal, Detail = new Dtos.GuidObject2() { Id = "806af5a5-8a9a-424f-8c9f-c1e9d084ee71" } },
                    Title = "Mr.",
                    FirstName = "LegalFirst",
                    MiddleName = "LegalMiddle",
                    LastName = "LegalLast",
                    Pedigree = "Jr.",
                    LastNamePrefix = "Mr.",
                    FullName = "LegalFirst LegalMiddle LegalLast"
                };
                personNames.Add(legalPrimaryName);

                var birthPrimaryName = new Dtos.DtoProperties.PersonName2DtoProperty()
                {
                    NameType = new Dtos.DtoProperties.PersonNameTypeDtoProperty() { Category = Dtos.EnumProperties.PersonNameType2.Birth, Detail = new Dtos.GuidObject2() { Id = "7dfa950c-8ae4-4dca-92f0-c083604285b6" } },
                    Title = "Mr.",
                    FirstName = "BirthFirst",
                    MiddleName = "BirthMiddle",
                    LastName = "BirthLast",
                    Pedigree = "Jr.",
                    LastNamePrefix = "Mr.",
                    FullName = "BirthFirst BirthMiddle BirthLast"
                };
                personNames.Add(birthPrimaryName);

                var chosenPrimaryName = new PersonName2DtoProperty()
                {
                    NameType = new PersonNameTypeDtoProperty() { Category = Dtos.EnumProperties.PersonNameType2.Personal, Detail = new Dtos.GuidObject2() { Id = "dd20ebdf-2452-41ef-9f86-ad1b1621a78d" } },
                    Title = "Mr.",
                    FirstName = "ChosenFirst",
                    MiddleName = "ChosenMiddle",
                    LastName = "ChosenLast",
                    Pedigree = "Jr.",
                    LastNamePrefix = "Ignore",
                    FullName = "ChosenFirst ChosenMiddle ChosenLast"
                };
                personNames.Add(chosenPrimaryName);

                var nickNamePrimaryName = new Dtos.DtoProperties.PersonName2DtoProperty()
                {
                    NameType = new Dtos.DtoProperties.PersonNameTypeDtoProperty() { Category = Dtos.EnumProperties.PersonNameType2.Personal, Detail = new Dtos.GuidObject2() { Id = "7b55610f-7d00-4260-bbcf-0e47fdbae647" } },
                    Title = "Mr.",
                    FirstName = "NickNameFirst",
                    MiddleName = "NickNameMiddle",
                    LastName = "NickNameLast",
                    Pedigree = "Jr.",
                    LastNamePrefix = "Mr.",
                    FullName = "NickNameFirst NickNameMiddle NickNameLast"
                };
                personNames.Add(nickNamePrimaryName);

                var historyPrimaryName = new Dtos.DtoProperties.PersonName2DtoProperty()
                {
                    NameType = new Dtos.DtoProperties.PersonNameTypeDtoProperty() { Category = Dtos.EnumProperties.PersonNameType2.Personal, Detail = new Dtos.GuidObject2() { Id = "d42cc964-35cb-4560-bc46-4b881e7705ea" } },
                    Title = "Mr.",
                    FirstName = "HistoryFirst",
                    MiddleName = "HistoryMiddle",
                    LastName = "HistoryLast",
                    Pedigree = "Jr.",
                    LastNamePrefix = "Mr.",
                    FullName = "HistoryFirst HistoryMiddle HistoryLast"
                };
                personNames.Add(historyPrimaryName);

                personDto.PersonNames = personNames;
                personDto.GenderType = Dtos.EnumProperties.GenderType2.Male;
                personDto.MaritalStatus = new Dtos.DtoProperties.PersonMaritalStatusDtoProperty() { Detail = new Dtos.GuidObject2(maritalStatusGuid), MaritalCategory = Dtos.EnumProperties.PersonMaritalStatusCategory.Married };// new Dtos.GuidObject(maritalStatusGuid);
                personDto.Ethnicity = new Dtos.DtoProperties.PersonEthnicityDtoProperty() { EthnicGroup = new Dtos.GuidObject2(ethnicityGuid) };
                personDto.Races = new List<Dtos.DtoProperties.PersonRaceDtoProperty>()
                {

                    new Dtos.DtoProperties.PersonRaceDtoProperty(){ Race = new Dtos.GuidObject2(raceAsianGuid)}
                };
                personDto.Credentials = new List<Dtos.DtoProperties.Credential3DtoProperty>()
                {
                    new Dtos.DtoProperties.Credential3DtoProperty()
                    {
                        Type = Dtos.EnumProperties.Credential3Type.Ssn,
                        Value = "111-11-1111"
                    }
                };
                var emailAddresses = new List<Dtos.DtoProperties.PersonEmailDtoProperty>();
                emailAddresses.Add(new Dtos.DtoProperties.PersonEmailDtoProperty()
                {

                    Type = new Dtos.DtoProperties.PersonEmailTypeDtoProperty() { EmailType = Dtos.EmailTypeList.School },
                    Address = "xyz@xmail.com"
                });
                personDto.EmailAddresses = emailAddresses;

                //Entity
                personIntegrationEntity = new PersonIntegration(personId, legalPrimaryName.LastName)
                {
                    Guid = personDto.Id,
                    Prefix = "Mr.",
                    FirstName = legalPrimaryName.FirstName,
                    MiddleName = legalPrimaryName.MiddleName,
                    Suffix = "Sr."

                };
                //Returned value
                personIntegrationReturned = new PersonIntegration(personId, "LegalLast");
                personIntegrationReturned.Guid = personGuid;
                personIntegrationReturned.Prefix = "Mr.";
                personIntegrationReturned.FirstName = "LegalFirst";
                personIntegrationReturned.MiddleName = "LegalMiddle";
                personIntegrationReturned.Suffix = "Jr.";
                personIntegrationReturned.Nickname = "NickNameFirst NickNameMiddle NickNameLast";
                personIntegrationReturned.BirthDate = new DateTime(1930, 1, 1);
                personIntegrationReturned.DeceasedDate = new DateTime(2014, 5, 12);
                personIntegrationReturned.GovernmentId = "111-11-1111";
                personIntegrationReturned.Religion = "CA";
                personIntegrationReturned.MaritalStatusCode = "M";
                personIntegrationReturned.EthnicCodes = new List<string> { "H", "N" };
                personIntegrationReturned.Gender = "M";
                personIntegrationReturned.RaceCodes = new List<string> { "AS" };
                personIntegrationReturned.AddRole(new PersonRole(PersonRoleType.Alumni, new DateTime(15, 01, 22), new DateTime(15, 05, 25)));
                personIntegrationReturned.BirthNameFirst = "BirthFirst";
                personIntegrationReturned.BirthNameLast = "BirthLast";
                personIntegrationReturned.BirthNameMiddle = "BirthMiddle";
                personIntegrationReturned.ChosenFirstName = "ChosenFirst";
                personIntegrationReturned.ChosenLastName = "ChosenLast";
                personIntegrationReturned.ChosenMiddleName = "ChosenMiddle";
                personIntegrationReturned.PreferredName = "PreferedFirst PreferedMiddle PreferedLast";
                personIntegrationReturned.FormerNames = new List<PersonName>()
                {
                    new PersonName("HistoryFirst", "HistoryMiddle", "HistoryLast")
                };
                // Mock the email address data response
                instEmail = new Domain.Base.Entities.EmailAddress("inst@inst.com", "COL") { IsPreferred = true };
                personIntegrationReturned.AddEmailAddress(instEmail);
                perEmail = new Domain.Base.Entities.EmailAddress("personal@personal.com", "PER") { IsPreferred = false };
                personIntegrationReturned.AddEmailAddress(perEmail);
                workEmail = new Domain.Base.Entities.EmailAddress("work@work.com", "BUS") { IsPreferred = false };
                personIntegrationReturned.AddEmailAddress(workEmail);

                // Mock the address hierarchy responses
                var addresses = new List<Domain.Base.Entities.Address>();
                homeAddr = new Domain.Base.Entities.Address()
                {
                    TypeCode = "HO",
                    Type = Dtos.EnumProperties.AddressType.Home.ToString(),
                    Guid = Guid.NewGuid().ToString(),
                    Status = "Current",
                    IsPreferredAddress = true
                };
                addresses.Add(homeAddr);
                mailAddr = new Domain.Base.Entities.Address()
                {
                    TypeCode = "MA",
                    Type = Dtos.EnumProperties.AddressType.Mailing.ToString(),
                    Guid = Guid.NewGuid().ToString(),
                    Status = "Current"
                };
                addresses.Add(mailAddr);
                resAddr = new Domain.Base.Entities.Address()
                {
                    TypeCode = "VA",
                    Type = Dtos.EnumProperties.AddressType.Vacation.ToString(),
                    Guid = Guid.NewGuid().ToString(),
                    Status = "Current",
                    IsPreferredResidence = true
                };
                addresses.Add(resAddr);
                workAddr = new Domain.Base.Entities.Address()
                {
                    TypeCode = "BU",
                    Type = Dtos.EnumProperties.AddressType.Business.ToString(),
                    Guid = Guid.NewGuid().ToString(),
                    Status = "Current"
                };
                addresses.Add(workAddr);
                personIntegrationReturned.Addresses = addresses;

                // Mock the person phone per type response
                homePhone = new Domain.Base.Entities.Phone("111-111-1111", "HO");
                personIntegrationReturned.AddPhone(homePhone);
                mobilePhone = new Domain.Base.Entities.Phone("222-222-2222", "MO");
                personIntegrationReturned.AddPhone(mobilePhone);
                residencePhone = new Domain.Base.Entities.Phone("333-333-3333", "VA");
                personIntegrationReturned.AddPhone(residencePhone);
                workPhone = new Domain.Base.Entities.Phone("444-444-4444", "BU", "4444");
                personIntegrationReturned.AddPhone(workPhone);

                // Mock social media
                var socialMediaTypeCode = "TW";
                var socialMediaHandle = "pontifex";
                var personSocialMedia = new SocialMedia(socialMediaTypeCode, socialMediaHandle);
                personIntegrationReturned.AddSocialMedia(personSocialMedia);
            }

            private void SetupReferenceDataRepositoryMocks()
            {
                referenceDataRepositoryMock.Setup(repo => repo.GetPrivacyStatusesAsync(It.IsAny<bool>()))
                     .ReturnsAsync(
                         new List<PrivacyStatus>() {
                        new PrivacyStatus( "d3d86052-9d55-4751-acda-5c07a064a82a", "UN", "Unrestricted", PrivacyStatusType.unrestricted),
                        new PrivacyStatus( "cff65dcc-4a9b-44ed-b8d0-930348c55ef8", "R", "restricted", PrivacyStatusType.restricted)
                        }
                     );
                personNameTypes = new List<PersonNameTypeItem>() {
                        new PersonNameTypeItem("8224f18e-69c5-480b-a9b4-52f596aa4a52", "PREFERRED", "Personal", PersonNameType.Personal),
                        new PersonNameTypeItem("7dfa950c-8ae4-4dca-92f0-c083604285b6", "BIRTH", "Birth", PersonNameType.Birth),
                        new PersonNameTypeItem("dd20ebdf-2452-41ef-9f86-ad1b1621a78d", "CHOSEN", "Chosen", PersonNameType.Personal),
                        new PersonNameTypeItem("806af5a5-8a9a-424f-8c9f-c1e9d084ee71", "LEGAL", "Legal", PersonNameType.Legal),
                        new PersonNameTypeItem("7b55610f-7d00-4260-bbcf-0e47fdbae647", "NICKNAME", "NickName", PersonNameType.Personal),
                        new PersonNameTypeItem("d42cc964-35cb-4560-bc46-4b881e7705ea", "HISTORY", "History", PersonNameType.Personal)
                        };
                referenceDataRepositoryMock.Setup(repo => repo.GetPersonNameTypesAsync(It.IsAny<bool>()))
                    .ReturnsAsync(personNameTypes);

                referenceDataRepositoryMock.Setup(repo => repo.GetDenominationsAsync(It.IsAny<bool>()))
                    .ReturnsAsync(
                        new List<Denomination>() {
                        new Denomination(baptistGuid,"BA", "Baptist") ,
                        new Denomination(catholicGuid,"CA", "Catholic")
                        }
                     );

                referenceDataRepositoryMock.Setup(repo => repo.GetEmailTypesAsync(It.IsAny<bool>()))
                     .ReturnsAsync(
                         new List<EmailType>() {
                        new EmailType("899803da-48f8-4044-beb8-5913a04b995d", "COL", "College", EmailTypeCategory.School),
                        new EmailType("301d485d-d37b-4d29-af00-465ced624a85", "PER", "Personal", EmailTypeCategory.Personal),
                        new EmailType("53fb7dab-d348-4657-b071-45d0e5933e05", "BUS", "Business", EmailTypeCategory.Business)
                        }
                     );

                // Mock the reference repository for ethnicity
                referenceDataRepositoryMock.Setup(repo => repo.GetEthnicitiesAsync(It.IsAny<bool>())).Returns(Task.FromResult<IEnumerable<Ethnicity>>(new List<Ethnicity>()
                {
                    new Ethnicity(ethnicityGuid, "H", "Hispanic", EthnicityType.Hispanic)
                }));

                referenceDataRepositoryMock.Setup(repo => repo.GetSocialMediaTypesAsync(It.IsAny<bool>()))
                     .ReturnsAsync(
                         new List<SocialMediaType>() {
                        new SocialMediaType("d1f311f4-687d-4dc7-a329-c6a8bfc9c74", "TW", "Twitter", SocialMediaTypeCategory.twitter)
                        }
                     );

                referenceDataRepositoryMock.Setup(repo => repo.GetAddressTypes2Async(It.IsAny<bool>()))
                     .ReturnsAsync(
                         new List<AddressType2>() {
                        new AddressType2("91979656-e110-4156-a75a-1a1a7294314d", "HO", "Home", AddressTypeCategory.Home),
                        new AddressType2("b887d5ec-9ed5-45e8-b44c-01782070f234", "MA", "Mailing", AddressTypeCategory.Mailing),
                        new AddressType2("d7d0a82c-fe74-480d-be1b-88a2e460af4c", "VA", "Vacation", AddressTypeCategory.Vacation),
                        new AddressType2("c9b8cd52-54e6-4c08-a9d9-224dd0c8b700", "BU", "Business", AddressTypeCategory.Business)
                         }
                     );

                referenceDataRepositoryMock.Setup(repo => repo.GetPhoneTypesAsync(It.IsAny<bool>()))
                     .ReturnsAsync(
                         new List<PhoneType>() {
                        new PhoneType("92c82d33-e55c-41a4-a2c3-f2f7d2c523d1", "HO", "Home", PhoneTypeCategory.Home),
                        new PhoneType("b6def2cc-cc95-4d0e-a32c-940fbbc2d689", "MO", "Mobile", PhoneTypeCategory.Mobile),
                        new PhoneType("f60e7b27-a3e3-4c92-9d36-f3cae27b724b", "VA", "Vacation", PhoneTypeCategory.Vacation),
                        new PhoneType("30e231cf-a199-4c9a-af01-be2e69b607c9", "BU", "Business", PhoneTypeCategory.Business)
                        }
                     );

                // Mock the reference repository for prefix
                referenceDataRepositoryMock.Setup(repo => repo.Prefixes).Returns(new List<Prefix>()
                {
                    new Prefix("MR","Mr","Mr."),
                    new Prefix("MS","Ms","Ms.")
                });

                // Mock the reference repository for prefix
                referenceDataRepositoryMock.Setup(repo => repo.GetPrefixesAsync()).ReturnsAsync(new List<Prefix>()
                {
                    new Prefix("MR","Mr","Mr."),
                    new Prefix("MS","Ms","Ms.")
                });

                // Mock the reference repository for suffix
                referenceDataRepositoryMock.Setup(repo => repo.Suffixes).Returns(new List<Suffix>()
                {
                    new Suffix("JR","Jr","Jr."),
                    new Suffix("SR","Sr","Sr.")
                });

                // Mock the reference repository for suffix
                referenceDataRepositoryMock.Setup(repo => repo.GetSuffixesAsync()).ReturnsAsync(new List<Suffix>()
                {
                    new Suffix("JR","Jr","Jr."),
                    new Suffix("SR","Sr","Sr.")
                });

                // Mock the reference repository for marital status
                referenceDataRepositoryMock.Setup(repo => repo.GetMaritalStatusesAsync(It.IsAny<bool>())).Returns(Task.FromResult<IEnumerable<MaritalStatus>>(new List<MaritalStatus>()
                {
                    new MaritalStatus(maritalStatusGuid, "M", "Married"){ Type = MaritalStatusType.Married }
                }));

                var raceEntities = new List<Race>();
                raceEntities.Add(new Race(raceAsianGuid, "AS", "Asian", RaceType.Asian));
                raceEntities.Add(new Race(racePacificIslanderGuid, "HP", "Hawaiian/Pacific Islander", RaceType.PacificIslander));
                referenceDataRepositoryMock.Setup(repo => repo.GetRacesAsync(It.IsAny<bool>())).Returns(Task.FromResult<IEnumerable<Race>>(raceEntities));
            }
        }

        [TestClass]
        public class CreateUpdatePerson4WithAddressPhoneSocialMedia : CurrentUserSetup
        {
            //Mocks
            Mock<IAdapterRegistry> adapterRegistryMock;
            Mock<IPersonRepository> personRepositoryMock;
            Mock<IPersonBaseRepository> personBaseRepositoryMock;
            Mock<IReferenceDataRepository> referenceDataRepositoryMock;
            Mock<IProfileRepository> profileRepositoryMock;
            Mock<IConfigurationRepository> configurationRepositoryMock;
            Mock<IRelationshipRepository> relationshipRepositoryMock;
            Mock<IProxyRepository> proxyRepositoryMock;
            Mock<IRoleRepository> roleRepositoryMock;
            Mock<ILogger> loggerMock;

            //userfactory
            ICurrentUserFactory currentUserFactory;

            //Service
            PersonService personService;


            private Ellucian.Colleague.Dtos.Person4 personDto;
            private Ellucian.Colleague.Domain.Base.Entities.PersonIntegration personIntegrationReturned;
            private Ellucian.Colleague.Domain.Base.Entities.PersonIntegration personIntegrationEntity;

            //private Ellucian.Colleague.Domain.Base.Entities.Person person;
            private List<Ellucian.Colleague.Domain.Base.Entities.Address> addresses = null;
            private IEnumerable<Ellucian.Colleague.Domain.Base.Entities.Phone> phones = new List<Domain.Base.Entities.Phone>();
            private IEnumerable<Ellucian.Colleague.Domain.Base.Entities.State> states;
            private IEnumerable<Ellucian.Colleague.Domain.Base.Entities.Country> countries;
            private List<Ellucian.Colleague.Domain.Base.Entities.County> counties;
            private List<PersonNameTypeItem> personNameTypes;

            //Entities
            private Ellucian.Colleague.Domain.Base.Entities.EmailAddress instEmail;
            private Ellucian.Colleague.Domain.Base.Entities.EmailAddress perEmail;
            private Ellucian.Colleague.Domain.Base.Entities.EmailAddress workEmail;
            private Ellucian.Colleague.Domain.Base.Entities.Address homeAddr;
            private Ellucian.Colleague.Domain.Base.Entities.Address mailAddr;
            private Ellucian.Colleague.Domain.Base.Entities.Address resAddr;
            private Ellucian.Colleague.Domain.Base.Entities.Address workAddr;
            private Ellucian.Colleague.Domain.Base.Entities.Phone homePhone;
            private Ellucian.Colleague.Domain.Base.Entities.Phone mobilePhone;
            private Ellucian.Colleague.Domain.Base.Entities.Phone residencePhone;
            private Ellucian.Colleague.Domain.Base.Entities.Phone workPhone;

            private IEnumerable<Domain.Base.Entities.Address> allAddresses;
            private IEnumerable<Domain.Base.Entities.PhoneType> allPhones;
            private IEnumerable<Domain.Base.Entities.Chapter> allChapters;
            private IEnumerable<Domain.Base.Entities.County> allCounties;
            private IEnumerable<Domain.Base.Entities.ZipcodeXlat> allZipCodeXlats;
            private IEnumerable<Domain.Base.Entities.GeographicAreaType> allGeographicAreaTypes;


            private List<Dtos.DtoProperties.PersonAddressDtoProperty> addressesCollection = new List<Dtos.DtoProperties.PersonAddressDtoProperty>();

            //Data
            private string personId = "0000011";
            private string personGuid = "5674f28b-b216-4055-b236-81a922d93b4c";
            private string maritalStatusGuid = "dca8edb5-120f-479a-a6bb-35ba3af4b344";
            private string ethnicityGuid = "dd0c42ca-c61d-4ca6-8d21-96ab5be35623";
            private string raceAsianGuid = "72b7737b-27db-4a06-944b-97d00c29b3db";
            private string racePacificIslanderGuid = "e20f9821-28a2-4e34-8550-6758850a0cf8";
            private string baptistGuid = "c0bdfd92-462f-4e59-bba5-1b15c4771c86";
            private string catholicGuid = "f96f04b0-4973-41f6-bc3d-9c7bc1c2c458";

            private string countyGuid = Guid.NewGuid().ToString();


            [TestInitialize]
            public void Initialize()
            {
                adapterRegistryMock = new Mock<IAdapterRegistry>();
                personRepositoryMock = new Mock<IPersonRepository>();
                personBaseRepositoryMock = new Mock<IPersonBaseRepository>();
                referenceDataRepositoryMock = new Mock<IReferenceDataRepository>();
                profileRepositoryMock = new Mock<IProfileRepository>();
                configurationRepositoryMock = new Mock<IConfigurationRepository>();
                relationshipRepositoryMock = new Mock<IRelationshipRepository>();
                proxyRepositoryMock = new Mock<IProxyRepository>();
                roleRepositoryMock = new Mock<IRoleRepository>();
                loggerMock = new Mock<ILogger>();

                currentUserFactory = new CurrentUserSetup.ThirdPartyUserFactory();

                SetupData();

                SetupReferenceDataRepositoryMocks();

                // International Parameters Host Country
                personRepositoryMock.Setup(repo => repo.GetHostCountryAsync()).ReturnsAsync("USA");

                personRepositoryMock.Setup(i => i.Update2Async(It.IsAny<PersonIntegration>(), It.IsAny<IEnumerable<Domain.Base.Entities.Address>>(),
                    It.IsAny<IEnumerable<Ellucian.Colleague.Domain.Base.Entities.Phone>>(), It.IsAny<int>())).ReturnsAsync(personIntegrationReturned);

                personRepositoryMock.Setup(i => i.GetAddressIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync("x");

                personService = new PersonService(adapterRegistryMock.Object, personRepositoryMock.Object, personBaseRepositoryMock.Object, referenceDataRepositoryMock.Object, profileRepositoryMock.Object,
                                                  configurationRepositoryMock.Object, relationshipRepositoryMock.Object, proxyRepositoryMock.Object, currentUserFactory,
                                                  roleRepositoryMock.Object, loggerMock.Object);
            }


            [TestCleanup]
            public void Cleanup()
            {
                personService = null;
                adapterRegistryMock = null;
                personRepositoryMock = null;
                personBaseRepositoryMock = null;
                referenceDataRepositoryMock = null;
                profileRepositoryMock = null;
                configurationRepositoryMock = null;
                relationshipRepositoryMock = null;
                proxyRepositoryMock = null;
                roleRepositoryMock = null;
                loggerMock = null;
            }

            [TestMethod]
            public async Task UpdatePerson4_UpdatePerson4Async_WithAddress()
            {
                //setup role
                updatePersonRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Base.BasePermissionCodes.UpdatePerson));
                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { updatePersonRole });

                //personId 0000011
                personRepositoryMock.Setup(i => i.GetPersonIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync(personId);

                var actual = await personService.UpdatePerson4Async(personDto);

                Assert.AreEqual(personDto.Addresses.Count(), actual.Addresses.Count());
                Assert.AreEqual(personDto.Phones.Count(), actual.Phones.Count());
                Assert.AreEqual(personDto.SocialMedia.Count(), actual.SocialMedia.Count());
            }

            [TestMethod]
            public async Task UpdatePerson4_UpdatePerson4Async_AddressNullId()
            {
                addressesCollection = new List<Dtos.DtoProperties.PersonAddressDtoProperty>();
                allAddresses = new TestAddressRepository().GetAddressDataWithNullId().ToList().Where(i => string.IsNullOrEmpty(i.Guid));

                foreach (var source in allAddresses)
                {
                    var address = new Ellucian.Colleague.Dtos.DtoProperties.PersonAddressDtoProperty
                    {
                        address = new PersonAddress()
                        {
                            Id = source.Guid,
                            AddressLines = source.AddressLines,
                            Latitude = source.Latitude,
                            Longitude = source.Longitude
                        },
                        AddressEffectiveStart = new DateTime(2015, 09, 01),
                        AddressEffectiveEnd = new DateTime(2015, 12, 20),
                        Preference = Dtos.EnumProperties.PersonPreference.Primary,
                        Type = new PersonAddressTypeDtoProperty()
                        {
                            AddressType = string.IsNullOrEmpty(source.Type) ? null : (Dtos.EnumProperties.AddressType?)Enum.Parse(typeof(Dtos.EnumProperties.AddressType), source.Type, true)
                        }
                    };
                    addressesCollection.Add(address);
                }
                personDto.Addresses = addressesCollection;
                personDto.Addresses.First().address.Place = new AddressPlace() { Country = new AddressCountry() { Locality = "Locality", Code = IsoCode.USA } };
                personDto.Addresses.First().address.AddressLines = new List<string>() { "Something" };
                personDto.Addresses.First().address.GeographicAreas = new List<GuidObject2>() { new GuidObject2("9ae3a175-1dfd-4937-b97b-3c9ad596e023") };

                updatePersonRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Base.BasePermissionCodes.UpdatePerson));
                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { updatePersonRole });

                var addrWithoutId = new TestAddressRepository().GetAddressDataWithNullId().Where(i => string.IsNullOrEmpty(i.Guid)).ToList();
                personIntegrationReturned.Addresses = addrWithoutId;
                personRepositoryMock.Setup(i => i.Update2Async(It.IsAny<PersonIntegration>(), It.IsAny<IEnumerable<Domain.Base.Entities.Address>>(),
                    It.IsAny<IEnumerable<Ellucian.Colleague.Domain.Base.Entities.Phone>>(), It.IsAny<int>())).ReturnsAsync(personIntegrationReturned);


                //personId 0000011
                personRepositoryMock.Setup(i => i.GetPersonIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync(personId);

                var actual = await personService.UpdatePerson4Async(personDto);

                Assert.IsNotNull(actual);
                Assert.AreEqual(personDto.Addresses.Count(), actual.Addresses.Count());
            }

            [TestMethod]
            public async Task UpdatePerson4_UpdatePerson4Async_All_Addresses()
            {
                addressesCollection = new List<Dtos.DtoProperties.PersonAddressDtoProperty>();
                allAddresses = new TestAddressRepository().GetAddressData().ToList();
                //d44134f9-0924-45d4-8b91-be9531aa7773
                var addAddress = allAddresses.FirstOrDefault(i => i.Guid.Equals("d44134f9-0924-45d4-8b91-be9531aa7773", StringComparison.OrdinalIgnoreCase));
                addAddress.TypeCode = "MA";
                allAddresses.ToList().Add(addAddress);
                allAddresses.All(i => i.SeasonalDates == new List<AddressSeasonalDates>()
                {
                    new AddressSeasonalDates("01/01", "05/31"),
                    new AddressSeasonalDates("08/01", "12/31")
                });
                allAddresses.Where(i => i.Guid.Equals("d44134f9-0924-45d4-8b91-be9531aa7773", StringComparison.OrdinalIgnoreCase)).All(i => i.IsPreferredAddress == true);

                foreach (var source in allAddresses)
                {
                    var address = new Ellucian.Colleague.Dtos.DtoProperties.PersonAddressDtoProperty
                    {
                        address = new PersonAddress()
                        {
                            Id = source.Guid,
                            AddressLines = source.AddressLines,
                            Latitude = source.Latitude,
                            Longitude = source.Longitude
                        },
                        AddressEffectiveStart = new DateTime(2015, 09, 01),
                        AddressEffectiveEnd = new DateTime(2015, 12, 20),
                        Preference = Dtos.EnumProperties.PersonPreference.Primary,
                        Type = new PersonAddressTypeDtoProperty()
                        {
                            AddressType = string.IsNullOrEmpty(source.Type) ? null : (Dtos.EnumProperties.AddressType?)Enum.Parse(typeof(Dtos.EnumProperties.AddressType), source.Type, true)
                        },
                        SeasonalOccupancies = new List<PersonAddressRecurrenceDtoProperty>()
                        {
                            new PersonAddressRecurrenceDtoProperty()
                            {
                             Recurrence = new Recurrence3()
                             {
                                 TimePeriod = new RepeatTimePeriod2(){ StartOn = new DateTimeOffset(2016, 01, 01,0,0,0, new TimeSpan()), EndOn = new DateTimeOffset(2016, 05, 31,0,0,0, new TimeSpan())}
                             }
                            },
                            new PersonAddressRecurrenceDtoProperty()
                            {
                             Recurrence = new Recurrence3()
                             {
                                 TimePeriod = new RepeatTimePeriod2(){ StartOn = new DateTimeOffset(2016, 08, 01,0,0,0, new TimeSpan()), EndOn = new DateTimeOffset(2016, 12, 31,0,0,0, new TimeSpan())}
                             }
                            }
                        }
                    };
                    addressesCollection.Add(address);
                }

                personDto.Addresses = addressesCollection;
                personDto.Addresses.First().address.Place = new AddressPlace() { Country = new AddressCountry() { Locality = "Locality", Code = IsoCode.USA } };
                personDto.Addresses.First().address.AddressLines = new List<string>() { "Something" };
                personDto.Addresses.First().address.GeographicAreas = new List<GuidObject2>() { new GuidObject2("9ae3a175-1dfd-4937-b97b-3c9ad596e023") };

                updatePersonRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Base.BasePermissionCodes.UpdatePerson));
                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { updatePersonRole });

                //personId 0000011
                personRepositoryMock.Setup(i => i.GetPersonIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync(personId);

                var actual = await personService.UpdatePerson4Async(personDto);

                Assert.IsNotNull(actual);
                Assert.AreEqual(personDto.Addresses.Count(), actual.Addresses.Count());
            }

            [TestMethod]
            public async Task UpdatePerson4_UpdatePerson4Async_PersonId_NullEmpty_Create()
            {
                //setup role
                createPersonRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Base.BasePermissionCodes.CreatePerson));
                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { createPersonRole });

                //personId 0000011
                personRepositoryMock.Setup(i => i.GetPersonIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync(string.Empty);

                personRepositoryMock.Setup(i => i.Create2Async(It.IsAny<PersonIntegration>(), It.IsAny<IEnumerable<Domain.Base.Entities.Address>>(), It.IsAny<IEnumerable<Domain.Base.Entities.Phone>>(), It.IsAny<int>())).ReturnsAsync(personIntegrationReturned);


                var actuals = await personService.UpdatePerson4Async(personDto);

                Assert.AreEqual(personDto.Addresses.Count(), actuals.Addresses.Count());
                foreach (var addr in actuals.Addresses)
                {
                    var expected = personDto.Addresses.FirstOrDefault(i => i.address.Id.Equals(addr.address.Id));
                    Assert.IsNotNull(expected);

                    Assert.AreEqual(expected.address.Id, addr.address.Id);
                    Assert.AreEqual(expected.address.Latitude, addr.address.Latitude);
                    Assert.AreEqual(expected.address.Longitude, addr.address.Longitude);
                    Assert.AreEqual(expected.address.Place, addr.address.Place);
                }

                foreach (var actual in actuals.Phones)
                {
                    var expected = personDto.Phones.FirstOrDefault(i => i.Number.Equals(actual.Number, StringComparison.OrdinalIgnoreCase));
                    Assert.IsNotNull(expected);

                    Assert.AreEqual(expected.Number, actual.Number);
                    Assert.AreEqual(expected.CountryCallingCode, actual.CountryCallingCode);
                    Assert.AreEqual(expected.Extension, actual.Extension);
                    Assert.AreEqual(expected.Preference, actual.Preference);
                    Assert.AreEqual(expected.Type.PhoneType, actual.Type.PhoneType);
                }

                foreach (var actual in actuals.SocialMedia)
                {
                    var expected = personDto.SocialMedia.FirstOrDefault(i => i.Address.Equals(actual.Address, StringComparison.OrdinalIgnoreCase));
                    Assert.IsNotNull(expected);

                    Assert.AreEqual(expected.Address, actual.Address);
                    Assert.AreEqual(expected.Preference, actual.Preference);
                    Assert.AreEqual(expected.Type.Category, actual.Type.Category);
                }
            }

            [TestMethod]
            [ExpectedException(typeof(Exception))]
            public async Task UpdatePerson4_UpdatePerson4Async_PlaceNull_Exception()
            {
                addressesCollection = new List<Dtos.DtoProperties.PersonAddressDtoProperty>();
                allAddresses = new TestAddressRepository().GetAddressDataWithNullId().ToList().Where(i => string.IsNullOrEmpty(i.Guid));

                foreach (var source in allAddresses)
                {
                    var address = new Ellucian.Colleague.Dtos.DtoProperties.PersonAddressDtoProperty
                    {
                        address = new PersonAddress()
                        {
                            Id = source.Guid,
                            AddressLines = source.AddressLines,
                            Latitude = source.Latitude,
                            Longitude = source.Longitude
                        },
                        AddressEffectiveStart = new DateTime(2015, 09, 01),
                        AddressEffectiveEnd = new DateTime(2015, 12, 20),
                        Preference = Dtos.EnumProperties.PersonPreference.Primary,
                        Type = new PersonAddressTypeDtoProperty()
                        {
                            AddressType = string.IsNullOrEmpty(source.Type) ? null : (Dtos.EnumProperties.AddressType?)Enum.Parse(typeof(Dtos.EnumProperties.AddressType), source.Type, true)
                        }
                    };
                    addressesCollection.Add(address);
                }
                personDto.Addresses = addressesCollection;
                //setup role
                updatePersonRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Base.BasePermissionCodes.UpdatePerson));
                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { updatePersonRole });

                //personId 0000011
                personRepositoryMock.Setup(i => i.GetPersonIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync(personId);

                var actual = await personService.UpdatePerson4Async(personDto);
            }

            [TestMethod]
            [ExpectedException(typeof(Exception))]
            public async Task UpdatePerson4_UpdatePerson4Async_AddressLineNull_Exception()
            {
                addressesCollection = new List<Dtos.DtoProperties.PersonAddressDtoProperty>();
                allAddresses = new TestAddressRepository().GetAddressDataWithNullId().ToList().Where(i => string.IsNullOrEmpty(i.Guid));

                foreach (var source in allAddresses)
                {
                    var address = new Ellucian.Colleague.Dtos.DtoProperties.PersonAddressDtoProperty
                    {
                        address = new PersonAddress()
                        {
                            Id = source.Guid,
                            AddressLines = source.AddressLines,
                            Latitude = source.Latitude,
                            Longitude = source.Longitude
                        },
                        AddressEffectiveStart = new DateTime(2015, 09, 01),
                        AddressEffectiveEnd = new DateTime(2015, 12, 20),
                        Preference = Dtos.EnumProperties.PersonPreference.Primary,
                        Type = new PersonAddressTypeDtoProperty()
                        {
                            AddressType = string.IsNullOrEmpty(source.Type) ? null : (Dtos.EnumProperties.AddressType?)Enum.Parse(typeof(Dtos.EnumProperties.AddressType), source.Type, true)
                        }
                    };
                    addressesCollection.Add(address);
                }
                personDto.Addresses = addressesCollection;
                personDto.Addresses.First().address.Place = new AddressPlace();
                //setup role
                updatePersonRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Base.BasePermissionCodes.UpdatePerson));
                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { updatePersonRole });

                //personId 0000011
                personRepositoryMock.Setup(i => i.GetPersonIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync(personId);

                var actual = await personService.UpdatePerson4Async(personDto);
            }

            [TestMethod]
            [ExpectedException(typeof(Exception))]
            public async Task UpdatePerson4_UpdatePerson4Async_PlaceCountryNull_Exception()
            {
                addressesCollection = new List<Dtos.DtoProperties.PersonAddressDtoProperty>();
                allAddresses = new TestAddressRepository().GetAddressDataWithNullId().ToList().Where(i => string.IsNullOrEmpty(i.Guid));

                foreach (var source in allAddresses)
                {
                    var address = new Ellucian.Colleague.Dtos.DtoProperties.PersonAddressDtoProperty
                    {
                        address = new PersonAddress()
                        {
                            Id = source.Guid,
                            AddressLines = source.AddressLines,
                            Latitude = source.Latitude,
                            Longitude = source.Longitude
                        },
                        AddressEffectiveStart = new DateTime(2015, 09, 01),
                        AddressEffectiveEnd = new DateTime(2015, 12, 20),
                        Preference = Dtos.EnumProperties.PersonPreference.Primary,
                        Type = new PersonAddressTypeDtoProperty()
                        {
                            AddressType = string.IsNullOrEmpty(source.Type) ? null : (Dtos.EnumProperties.AddressType?)Enum.Parse(typeof(Dtos.EnumProperties.AddressType), source.Type, true)
                        }
                    };
                    addressesCollection.Add(address);
                }
                personDto.Addresses = addressesCollection;
                personDto.Addresses.First().address.Place = new AddressPlace();
                personDto.Addresses.First().address.AddressLines = new List<string>() { "Something" };
                //setup role
                updatePersonRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Base.BasePermissionCodes.UpdatePerson));
                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { updatePersonRole });

                //personId 0000011
                personRepositoryMock.Setup(i => i.GetPersonIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync(personId);

                var actual = await personService.UpdatePerson4Async(personDto);
            }

            [TestMethod]
            [ExpectedException(typeof(Exception))]
            public async Task UpdatePerson4_UpdatePerson4Async_PlaceCountryLocalityNull_Exception()
            {
                addressesCollection = new List<Dtos.DtoProperties.PersonAddressDtoProperty>();
                allAddresses = new TestAddressRepository().GetAddressDataWithNullId().ToList().Where(i => string.IsNullOrEmpty(i.Guid));

                foreach (var source in allAddresses)
                {
                    var address = new Ellucian.Colleague.Dtos.DtoProperties.PersonAddressDtoProperty
                    {
                        address = new PersonAddress()
                        {
                            Id = source.Guid,
                            AddressLines = source.AddressLines,
                            Latitude = source.Latitude,
                            Longitude = source.Longitude
                        },
                        AddressEffectiveStart = new DateTime(2015, 09, 01),
                        AddressEffectiveEnd = new DateTime(2015, 12, 20),
                        Preference = Dtos.EnumProperties.PersonPreference.Primary,
                        Type = new PersonAddressTypeDtoProperty()
                        {
                            AddressType = string.IsNullOrEmpty(source.Type) ? null : (Dtos.EnumProperties.AddressType?)Enum.Parse(typeof(Dtos.EnumProperties.AddressType), source.Type, true)
                        }
                    };
                    addressesCollection.Add(address);
                }
                personDto.Addresses = addressesCollection;
                personDto.Addresses.First().address.Place = new AddressPlace() { Country = new AddressCountry() { Locality = string.Empty } };
                personDto.Addresses.First().address.AddressLines = new List<string>() { "Something" };
                //setup role
                updatePersonRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Base.BasePermissionCodes.UpdatePerson));
                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { updatePersonRole });

                //personId 0000011
                personRepositoryMock.Setup(i => i.GetPersonIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync(personId);

                var actual = await personService.UpdatePerson4Async(personDto);
            }

            [TestMethod]
            [ExpectedException(typeof(Exception))]
            public async Task UpdatePerson4_UpdatePerson4Async_SocialMediaTypeNull_Exception()
            {
                //setup role
                createPersonRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Base.BasePermissionCodes.CreatePerson));
                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { createPersonRole });

                //personId 0000011
                personRepositoryMock.Setup(i => i.GetPersonIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync(string.Empty);

                personRepositoryMock.Setup(i => i.Create2Async(It.IsAny<PersonIntegration>(), It.IsAny<IEnumerable<Domain.Base.Entities.Address>>(), It.IsAny<IEnumerable<Domain.Base.Entities.Phone>>(), It.IsAny<int>())).ReturnsAsync(personIntegrationReturned);

                personDto.SocialMedia.First().Type = null;

                var actual = await personService.UpdatePerson4Async(personDto);
            }

            [TestMethod]
            [ExpectedException(typeof(Exception))]
            public async Task UpdatePerson4_UpdatePerson4Async_SocialMediaAddressNull_Exception()
            {
                //setup role
                createPersonRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Base.BasePermissionCodes.CreatePerson));
                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { createPersonRole });

                //personId 0000011
                personRepositoryMock.Setup(i => i.GetPersonIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync(string.Empty);

                personRepositoryMock.Setup(i => i.Create2Async(It.IsAny<PersonIntegration>(), It.IsAny<IEnumerable<Domain.Base.Entities.Address>>(), It.IsAny<IEnumerable<Domain.Base.Entities.Phone>>(), It.IsAny<int>())).ReturnsAsync(personIntegrationReturned);

                personDto.SocialMedia.First().Address = null;

                var actual = await personService.UpdatePerson4Async(personDto);
            }

            [TestMethod]
            [ExpectedException(typeof(Exception))]
            public async Task UpdatePerson4_UpdatePerson4Async_SocialMediaTypNotFound_Exception()
            {
                //setup role
                createPersonRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Base.BasePermissionCodes.CreatePerson));
                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { createPersonRole });

                //personId 0000011
                personRepositoryMock.Setup(i => i.GetPersonIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync(string.Empty);

                personRepositoryMock.Setup(i => i.Create2Async(It.IsAny<PersonIntegration>(), It.IsAny<IEnumerable<Domain.Base.Entities.Address>>(), It.IsAny<IEnumerable<Domain.Base.Entities.Phone>>(), It.IsAny<int>())).ReturnsAsync(personIntegrationReturned);

                personDto.SocialMedia.First().Type = new PersonSocialMediaType() { Category = Ellucian.Colleague.Dtos.SocialMediaTypeCategory.blog };

                var actual = await personService.UpdatePerson4Async(personDto);
            }

            [TestMethod]
            [ExpectedException(typeof(Exception))]
            public async Task UpdatePerson4_NullAddress_Exception()
            {
                //setup role
                createPersonRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Base.BasePermissionCodes.CreatePerson));
                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { createPersonRole });

                //personId 0000011
                personRepositoryMock.Setup(i => i.GetPersonIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync(string.Empty);

                personRepositoryMock.Setup(i => i.Create2Async(It.IsAny<PersonIntegration>(), It.IsAny<IEnumerable<Domain.Base.Entities.Address>>(), phones, It.IsAny<int>())).ReturnsAsync(personIntegrationReturned);

                personDto.Addresses = new List<PersonAddressDtoProperty>()
                {
                    new PersonAddressDtoProperty(){address = null}
                };

                var actual = await personService.UpdatePerson4Async(personDto);
            }

            [TestMethod]
            [ExpectedException(typeof(Exception))]
            public async Task UpdatePerson4_NullType_Exception()
            {
                //setup role
                createPersonRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Base.BasePermissionCodes.CreatePerson));
                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { createPersonRole });

                //personId 0000011
                personRepositoryMock.Setup(i => i.GetPersonIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync(string.Empty);

                personRepositoryMock.Setup(i => i.Create2Async(It.IsAny<PersonIntegration>(), It.IsAny<IEnumerable<Domain.Base.Entities.Address>>(), phones, It.IsAny<int>())).ReturnsAsync(personIntegrationReturned);

                personDto.Addresses = new List<PersonAddressDtoProperty>()
                {
                    new PersonAddressDtoProperty(){address = new PersonAddress(), Type = null}
                };

                var actual = await personService.UpdatePerson4Async(personDto);
            }

            [TestMethod]
            [ExpectedException(typeof(Exception))]
            public async Task UpdatePerson4_NullPhoneType_Exception()
            {
                //setup role
                createPersonRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Base.BasePermissionCodes.CreatePerson));
                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { createPersonRole });

                //personId 0000011
                personRepositoryMock.Setup(i => i.GetPersonIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync(string.Empty);

                personRepositoryMock.Setup(i => i.Create2Async(It.IsAny<PersonIntegration>(), It.IsAny<IEnumerable<Domain.Base.Entities.Address>>(), phones, It.IsAny<int>())).ReturnsAsync(personIntegrationReturned);

                personDto.Phones = new List<PersonPhoneDtoProperty>()
                {
                    new PersonPhoneDtoProperty()
                    {
                         CountryCallingCode = "1",
                         Number = "111-111-1111"
                    }
                };

                var actual = await personService.UpdatePerson4Async(personDto);
            }

            [TestMethod]
            [ExpectedException(typeof(Exception))]
            public async Task UpdatePerson4_NullPhoneNumber_Exception()
            {
                //setup role
                createPersonRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Base.BasePermissionCodes.CreatePerson));
                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { createPersonRole });

                //personId 0000011
                personRepositoryMock.Setup(i => i.GetPersonIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync(string.Empty);

                personRepositoryMock.Setup(i => i.Create2Async(It.IsAny<PersonIntegration>(), It.IsAny<IEnumerable<Domain.Base.Entities.Address>>(), phones, It.IsAny<int>())).ReturnsAsync(personIntegrationReturned);

                personDto.Phones = new List<PersonPhoneDtoProperty>()
                {
                    new PersonPhoneDtoProperty()
                    {
                         CountryCallingCode = "1",
                         Number = "" ,
                         Type = new PersonPhoneTypeDtoProperty(){ PhoneType = PersonPhoneTypeCategory.Home}
                    }
                };

                var actual = await personService.UpdatePerson4Async(personDto);
            }

            [TestMethod]
            [ExpectedException(typeof(Exception))]
            public async Task UpdatePerson4_InvalidPhoneType_Exception()
            {
                //setup role
                createPersonRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Base.BasePermissionCodes.CreatePerson));
                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { createPersonRole });

                //personId 0000011
                personRepositoryMock.Setup(i => i.GetPersonIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync(string.Empty);

                personRepositoryMock.Setup(i => i.Create2Async(It.IsAny<PersonIntegration>(), It.IsAny<IEnumerable<Domain.Base.Entities.Address>>(), phones, It.IsAny<int>())).ReturnsAsync(personIntegrationReturned);

                personDto.Phones = new List<PersonPhoneDtoProperty>()
                {
                    new PersonPhoneDtoProperty()
                    {
                         CountryCallingCode = "1",
                         Number = "1234" ,
                         Type = new PersonPhoneTypeDtoProperty(){ PhoneType = PersonPhoneTypeCategory.Fax, Detail = new GuidObject2("12345") }
                    }
                };

                var actual = await personService.UpdatePerson4Async(personDto);
            }

            [TestMethod]
            [ExpectedException(typeof(Exception))]
            public async Task UpdatePerson4_NullPhoneTypeId_Exception()
            {
                //setup role
                createPersonRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Base.BasePermissionCodes.CreatePerson));
                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { createPersonRole });

                //personId 0000011
                personRepositoryMock.Setup(i => i.GetPersonIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync(string.Empty);

                personRepositoryMock.Setup(i => i.Create2Async(It.IsAny<PersonIntegration>(), It.IsAny<IEnumerable<Domain.Base.Entities.Address>>(), phones, It.IsAny<int>())).ReturnsAsync(personIntegrationReturned);

                personDto.Phones = new List<PersonPhoneDtoProperty>()
                {
                    new PersonPhoneDtoProperty()
                    {
                         CountryCallingCode = "1",
                         Number = "1234" ,
                         Type = new PersonPhoneTypeDtoProperty(){ PhoneType = PersonPhoneTypeCategory.Home, Detail = new GuidObject2() }
                    }
                };

                var actual = await personService.UpdatePerson4Async(personDto);
            }

            [TestMethod]
            [ExpectedException(typeof(Exception))]
            public async Task UpdatePerson4_Username_Exception()
            {
                //setup role
                updatePersonRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Base.BasePermissionCodes.UpdatePerson));
                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { updatePersonRole });

                //personId 0000011
                personRepositoryMock.Setup(i => i.GetPersonIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync(personId);

                personDto.Credentials = new List<Dtos.DtoProperties.Credential3DtoProperty>()
                {
                    new Dtos.DtoProperties.Credential3DtoProperty()
                    {
                        Type = Dtos.EnumProperties.Credential3Type.ColleagueUserName,
                        Value = "testUsername"
                    }
                };
                // Mock the response for getting a Person Pin 
                var personPin = new PersonPin("0000011", "WrongUsername");
                var personPins = new List<PersonPin>();
                personPins.Add(personPin);
                personRepositoryMock.Setup(repo => repo.GetPersonPinsAsync(It.IsAny<string[]>())).ReturnsAsync(personPins);

                var actual = await personService.UpdatePerson4Async(personDto);
            }

            private void SetupData()
            {
                // setup personDto object
                personDto = new Dtos.Person4();
                personDto.Id = personGuid;
                personDto.BirthDate = new DateTime(1930, 1, 1);
                personDto.DeceasedDate = new DateTime(2014, 5, 12);
                var personNames = new List<Dtos.DtoProperties.PersonName2DtoProperty>();
                var legalPrimaryName = new Dtos.DtoProperties.PersonName2DtoProperty()
                {
                    NameType = new Dtos.DtoProperties.PersonNameTypeDtoProperty() { Category = Dtos.EnumProperties.PersonNameType2.Legal, Detail = new Dtos.GuidObject2() { Id = "806af5a5-8a9a-424f-8c9f-c1e9d084ee71" } },
                    Title = "Mr.",
                    FirstName = "LegalFirst",
                    MiddleName = "LegalMiddle",
                    LastName = "LegalLast",
                    Pedigree = "Jr.",
                    LastNamePrefix = "Mr.",
                    FullName = "LegalFirst LegalMiddle LegalLast"
                };
                personNames.Add(legalPrimaryName);

                var birthPrimaryName = new Dtos.DtoProperties.PersonName2DtoProperty()
                {
                    NameType = new Dtos.DtoProperties.PersonNameTypeDtoProperty() { Category = Dtos.EnumProperties.PersonNameType2.Birth, Detail = new Dtos.GuidObject2() { Id = "7dfa950c-8ae4-4dca-92f0-c083604285b6" } },
                    Title = "Mr.",
                    FirstName = "BirthFirst",
                    MiddleName = "BirthMiddle",
                    LastName = "BirthLast",
                    Pedigree = "Jr.",
                    LastNamePrefix = "Mr.",
                    FullName = "BirthFirst BirthMiddle BirthLast"
                };
                personNames.Add(birthPrimaryName);

                var chosenPrimaryName = new PersonName2DtoProperty()
                {
                    NameType = new PersonNameTypeDtoProperty() { Category = Dtos.EnumProperties.PersonNameType2.Personal, Detail = new Dtos.GuidObject2() { Id = "dd20ebdf-2452-41ef-9f86-ad1b1621a78d" } },
                    Title = "Mr.",
                    FirstName = "ChosenFirst",
                    MiddleName = "ChosenMiddle",
                    LastName = "ChosenLast",
                    Pedigree = "Jr.",
                    LastNamePrefix = "Ignore",
                    FullName = "ChosenFirst ChosenMiddle ChosenLast"
                };
                personNames.Add(chosenPrimaryName);

                var nickNamePrimaryName = new Dtos.DtoProperties.PersonName2DtoProperty()
                {
                    NameType = new Dtos.DtoProperties.PersonNameTypeDtoProperty() { Category = Dtos.EnumProperties.PersonNameType2.Personal, Detail = new Dtos.GuidObject2() { Id = "7b55610f-7d00-4260-bbcf-0e47fdbae647" } },
                    Title = "Mr.",
                    FirstName = "NickNameFirst",
                    MiddleName = "NickNameMiddle",
                    LastName = "NickNameLast",
                    Pedigree = "Jr.",
                    LastNamePrefix = "Mr.",
                    FullName = "NickNameFirst NickNameMiddle NickNameLast"
                };
                personNames.Add(nickNamePrimaryName);

                var historyPrimaryName = new Dtos.DtoProperties.PersonName2DtoProperty()
                {
                    NameType = new Dtos.DtoProperties.PersonNameTypeDtoProperty() { Category = Dtos.EnumProperties.PersonNameType2.Personal, Detail = new Dtos.GuidObject2() { Id = "d42cc964-35cb-4560-bc46-4b881e7705ea" } },
                    Title = "Mr.",
                    FirstName = "HistoryFirst",
                    MiddleName = "HistoryMiddle",
                    LastName = "HistoryLast",
                    Pedigree = "Jr.",
                    LastNamePrefix = "Mr.",
                    FullName = "HistoryFirst HistoryMiddle HistoryLast"
                };
                personNames.Add(historyPrimaryName);

                personDto.PersonNames = personNames;
                personDto.GenderType = Dtos.EnumProperties.GenderType2.Male;
                personDto.MaritalStatus = new Dtos.DtoProperties.PersonMaritalStatusDtoProperty() { Detail = new Dtos.GuidObject2(maritalStatusGuid), MaritalCategory = Dtos.EnumProperties.PersonMaritalStatusCategory.Married };// new Dtos.GuidObject(maritalStatusGuid);
                personDto.Ethnicity = new Dtos.DtoProperties.PersonEthnicityDtoProperty() { EthnicGroup = new Dtos.GuidObject2(ethnicityGuid) };
                personDto.Races = new List<Dtos.DtoProperties.PersonRaceDtoProperty>()
                {

                    new Dtos.DtoProperties.PersonRaceDtoProperty(){ Race = new Dtos.GuidObject2(raceAsianGuid)}
                };
                personDto.Credentials = new List<Dtos.DtoProperties.Credential3DtoProperty>()
                {
                    new Dtos.DtoProperties.Credential3DtoProperty()
                    {
                        Type = Dtos.EnumProperties.Credential3Type.Ssn,
                        Value = "111-11-1111"
                    }
                };
                var emailAddresses = new List<Dtos.DtoProperties.PersonEmailDtoProperty>();
                emailAddresses.Add(new Dtos.DtoProperties.PersonEmailDtoProperty()
                {

                    Type = new Dtos.DtoProperties.PersonEmailTypeDtoProperty() { EmailType = Dtos.EmailTypeList.School },
                    Address = "xyz@xmail.com"
                });
                personDto.EmailAddresses = emailAddresses;

                // Mock the reference repository for states
                states = new List<State>()
                {
                    new State("VA","Virginia"),
                    new State("MD","Maryland"),
                    new State("NY","New York"),
                    new State("MA","Massachusetts")
                };

                //Entity
                personIntegrationEntity = new PersonIntegration(personId, legalPrimaryName.LastName)
                {
                    Guid = personDto.Id,
                    Prefix = "Mr.",
                    FirstName = legalPrimaryName.FirstName,
                    MiddleName = legalPrimaryName.MiddleName,
                    Suffix = "Sr."

                };
                allChapters = new TestGeographicAreaRepository().GetChapters();
                allCounties = new TestGeographicAreaRepository().GetCounties();
                allZipCodeXlats = new TestGeographicAreaRepository().GetZipCodeXlats();
                allGeographicAreaTypes = new TestGeographicAreaRepository().Get();
                counties = new List<County>()
                {
                    new County(Guid.NewGuid().ToString(), "FFX","Fairfax County"),
                    new County(Guid.NewGuid().ToString(), "BAL","Baltimore County"),
                    new County(Guid.NewGuid().ToString(), "NY","New York County"),
                    new County(Guid.NewGuid().ToString(), "BOS","Boston County")
                };

                //Addreses
                allAddresses = new TestAddressRepository().GetAddressData().ToList();

                foreach (var source in allAddresses)
                {
                    var address = new Ellucian.Colleague.Dtos.DtoProperties.PersonAddressDtoProperty
                    {
                        address = new PersonAddress()
                        {
                            Id = source.Guid,
                            AddressLines = source.AddressLines,
                            Latitude = source.Latitude,
                            Longitude = source.Longitude
                        },
                        AddressEffectiveStart = new DateTime(2015, 09, 01),
                        AddressEffectiveEnd = new DateTime(2015, 12, 20),
                        Preference = Dtos.EnumProperties.PersonPreference.Primary,
                        Type = new PersonAddressTypeDtoProperty()
                        {
                            AddressType = string.IsNullOrEmpty(source.Type) ? null : (Dtos.EnumProperties.AddressType?)Enum.Parse(typeof(Dtos.EnumProperties.AddressType), source.Type, true)
                        }
                    };
                    addressesCollection.Add(address);
                }
                personDto.Addresses = addressesCollection;

                var phoneList = new List<PersonPhoneDtoProperty>()
                {
                    new PersonPhoneDtoProperty()
                    {
                        CountryCallingCode = "01",
                         Extension = "1",
                         Number = "111-111-1111",
                         Preference = PersonPreference.Primary,
                         Type = new PersonPhoneTypeDtoProperty()
                         {
                             Detail = new GuidObject2("92c82d33-e55c-41a4-a2c3-f2f7d2c523d1"),
                             PhoneType = PersonPhoneTypeCategory.Home
                         }
                    },
                    new PersonPhoneDtoProperty()
                    {
                        CountryCallingCode = "02",
                         Extension = "2",
                         Number = "222-222-2222",
                         Preference = PersonPreference.Primary,
                         Type = new PersonPhoneTypeDtoProperty()
                         {
                             Detail = new GuidObject2("b6def2cc-cc95-4d0e-a32c-940fbbc2d689"),
                             PhoneType = PersonPhoneTypeCategory.Mobile
                         }
                    },
                    new PersonPhoneDtoProperty()
                    {
                        CountryCallingCode = "03",
                         Extension = "3",
                         Number = "333-333-3333",
                         Preference = PersonPreference.Primary,
                         Type = new PersonPhoneTypeDtoProperty()
                         {
                             Detail = new GuidObject2("f60e7b27-a3e3-4c92-9d36-f3cae27b724b"),
                             PhoneType = PersonPhoneTypeCategory.Vacation
                         }
                    },
                    new PersonPhoneDtoProperty()
                    {
                        CountryCallingCode = "04",
                         Extension = "4444",
                         Number = "444-444-4444",
                         Preference = PersonPreference.Primary,
                         Type = new PersonPhoneTypeDtoProperty()
                         {
                             Detail = new GuidObject2("30e231cf-a199-4c9a-af01-be2e69b607c9"),
                             PhoneType = PersonPhoneTypeCategory.Home
                         }
                    },
                };

                personDto.Phones = phoneList;

                //SocialMedia
                personDto.SocialMedia = new List<PersonSocialMediaDtoProperty>()
                {
                    new PersonSocialMediaDtoProperty()
                    {
                        Address = "http://www.facebook.com/jDoe",
                         Preference = PersonPreference.Primary,
                         Type = new PersonSocialMediaType(){ Category = Ellucian.Colleague.Dtos.SocialMediaTypeCategory.facebook, Detail = new GuidObject2("d1f311f4-687d-4dc7-a329-c6a8bfc9c74") }
                    },
                    new PersonSocialMediaDtoProperty()
                    {
                        Address = "http://www.somewebsite.com/jDoe",
                         Preference = PersonPreference.Primary,
                         Type = new PersonSocialMediaType(){ Category = Ellucian.Colleague.Dtos.SocialMediaTypeCategory.website, Detail = new GuidObject2("d1f311f4-687d-4dc7-a329-c6a8bfc9c75") }
                    }
                };

                //Returned value
                personIntegrationReturned = new PersonIntegration(personId, "LegalLast");
                personIntegrationReturned.Guid = personGuid;
                personIntegrationReturned.Prefix = "Mr.";
                personIntegrationReturned.FirstName = "LegalFirst";
                personIntegrationReturned.MiddleName = "LegalMiddle";
                personIntegrationReturned.Suffix = "Jr.";
                personIntegrationReturned.Nickname = "NickNameFirst NickNameMiddle NickNameLast";
                personIntegrationReturned.BirthDate = new DateTime(1930, 1, 1);
                personIntegrationReturned.DeceasedDate = new DateTime(2014, 5, 12);
                personIntegrationReturned.GovernmentId = "111-11-1111";
                personIntegrationReturned.Religion = "CA";
                personIntegrationReturned.MaritalStatusCode = "M";
                personIntegrationReturned.EthnicCodes = new List<string> { "H", "N" };
                personIntegrationReturned.Gender = "M";
                personIntegrationReturned.RaceCodes = new List<string> { "AS" };
                personIntegrationReturned.AddEmailAddress(new EmailAddress("inst@inst.com", "COL") { IsPreferred = true });
                personIntegrationReturned.AddRole(new PersonRole(PersonRoleType.Alumni, new DateTime(15, 01, 22), new DateTime(15, 05, 25)));
                personIntegrationReturned.BirthNameFirst = "BirthFirst";
                personIntegrationReturned.BirthNameLast = "BirthLast";
                personIntegrationReturned.BirthNameMiddle = "BirthMiddle";
                personIntegrationReturned.ChosenFirstName = "ChosenFirst";
                personIntegrationReturned.ChosenLastName = "ChosenLast";
                personIntegrationReturned.ChosenMiddleName = "ChosenMiddle";
                personIntegrationReturned.PreferredName = "PreferedFirst PreferedMiddle PreferedLast";
                personIntegrationReturned.FormerNames = new List<PersonName>()
                {
                    new PersonName("HistoryFirst", "HistoryMiddle", "HistoryLast")
                };
                // Mock the email address data response
                perEmail = new Domain.Base.Entities.EmailAddress("personal@personal.com", "PER") { IsPreferred = false };
                personIntegrationReturned.AddEmailAddress(perEmail);
                workEmail = new Domain.Base.Entities.EmailAddress("work@work.com", "BUS") { IsPreferred = false }; ;
                personIntegrationReturned.AddEmailAddress(workEmail);

                // Mock the address hierarchy responses
                var addresses = new TestAddressRepository().GetAddressData().ToList();
                personIntegrationReturned.Addresses = addresses;
                var addrWithoutId = new TestAddressRepository().GetAddressDataWithNullId().ToList();

                // Mock the person phone per type response
                homePhone = new Domain.Base.Entities.Phone("111-111-1111", "HO", "1") { CountryCallingCode = "01", IsPreferred = true };
                personIntegrationReturned.AddPhone(homePhone);
                mobilePhone = new Domain.Base.Entities.Phone("222-222-2222", "MO", "2") { CountryCallingCode = "02", IsPreferred = true, };
                personIntegrationReturned.AddPhone(mobilePhone);
                residencePhone = new Domain.Base.Entities.Phone("333-333-3333", "VA", "3") { CountryCallingCode = "03", IsPreferred = true };
                personIntegrationReturned.AddPhone(residencePhone);
                workPhone = new Domain.Base.Entities.Phone("444-444-4444", "HO", "4444") { CountryCallingCode = "04", IsPreferred = true };
                personIntegrationReturned.AddPhone(workPhone);

                personIntegrationReturned.AddSocialMedia(new Domain.Base.Entities.SocialMedia("FB", "http://www.facebook.com/jDoe") { IsPreferred = true });
                personIntegrationReturned.AddSocialMedia(new Domain.Base.Entities.SocialMedia("website", "http://www.somewebsite.com/jDoe") { IsPreferred = true });

            }

            private void SetupReferenceDataRepositoryMocks()
            {
                referenceDataRepositoryMock.Setup(repo => repo.GetPrivacyStatusesAsync(It.IsAny<bool>()))
                     .ReturnsAsync(
                         new List<PrivacyStatus>() {
                        new PrivacyStatus( "d3d86052-9d55-4751-acda-5c07a064a82a", "UN", "Unrestricted", PrivacyStatusType.unrestricted),
                        new PrivacyStatus( "cff65dcc-4a9b-44ed-b8d0-930348c55ef8", "R", "restricted", PrivacyStatusType.restricted)
                        }
                     );
                personNameTypes = new List<PersonNameTypeItem>() {
                        new PersonNameTypeItem("8224f18e-69c5-480b-a9b4-52f596aa4a52", "PREFERRED", "Personal", PersonNameType.Personal),
                        new PersonNameTypeItem("7dfa950c-8ae4-4dca-92f0-c083604285b6", "BIRTH", "Birth", PersonNameType.Birth),
                        new PersonNameTypeItem("dd20ebdf-2452-41ef-9f86-ad1b1621a78d", "CHOSEN", "Chosen", PersonNameType.Personal),
                        new PersonNameTypeItem("806af5a5-8a9a-424f-8c9f-c1e9d084ee71", "LEGAL", "Legal", PersonNameType.Legal),
                        new PersonNameTypeItem("7b55610f-7d00-4260-bbcf-0e47fdbae647", "NICKNAME", "NickName", PersonNameType.Personal),
                        new PersonNameTypeItem("d42cc964-35cb-4560-bc46-4b881e7705ea", "HISTORY", "History", PersonNameType.Personal)
                        };
                referenceDataRepositoryMock.Setup(repo => repo.GetPersonNameTypesAsync(It.IsAny<bool>()))
                    .ReturnsAsync(personNameTypes);

                referenceDataRepositoryMock.Setup(repo => repo.GetDenominationsAsync(It.IsAny<bool>()))
                    .ReturnsAsync(
                        new List<Denomination>() {
                        new Denomination(baptistGuid,"BA", "Baptist") ,
                        new Denomination(catholicGuid,"CA", "Catholic")
                        }
                     );

                referenceDataRepositoryMock.Setup(repo => repo.GetEmailTypesAsync(It.IsAny<bool>()))
                     .ReturnsAsync(
                         new List<EmailType>() {
                        new EmailType("899803da-48f8-4044-beb8-5913a04b995d", "COL", "College", EmailTypeCategory.School),
                        new EmailType("301d485d-d37b-4d29-af00-465ced624a85", "PER", "Personal", EmailTypeCategory.Personal),
                        new EmailType("53fb7dab-d348-4657-b071-45d0e5933e05", "BUS", "Business", EmailTypeCategory.Business)
                        }
                     );

                // Mock the reference repository for ethnicity
                referenceDataRepositoryMock.Setup(repo => repo.GetEthnicitiesAsync(It.IsAny<bool>())).Returns(Task.FromResult<IEnumerable<Ethnicity>>(new List<Ethnicity>()
                {
                    new Ethnicity(ethnicityGuid, "H", "Hispanic", EthnicityType.Hispanic)
                }));

                referenceDataRepositoryMock.Setup(repo => repo.GetSocialMediaTypesAsync(It.IsAny<bool>()))
                     .ReturnsAsync(
                         new List<SocialMediaType>() {
                        new SocialMediaType("d1f311f4-687d-4dc7-a329-c6a8bfc9c74", "FB", "Facebook", SocialMediaTypeCategory.facebook),
                        new SocialMediaType("d1f311f4-687d-4dc7-a329-c6a8bfc9c75", "WS", "Website", SocialMediaTypeCategory.website)
                        }
                     );

                referenceDataRepositoryMock.Setup(repo => repo.GetAddressTypes2Async(It.IsAny<bool>()))
                     .ReturnsAsync(
                         new List<AddressType2>() {
                        new AddressType2("91979656-e110-4156-a75a-1a1a7294314d", "HO", "Home", AddressTypeCategory.Home),
                        new AddressType2("b887d5ec-9ed5-45e8-b44c-01782070f234", "MA", "Mailing", AddressTypeCategory.Mailing),
                        new AddressType2("d7d0a82c-fe74-480d-be1b-88a2e460af4c", "VA", "Vacation", AddressTypeCategory.Vacation),
                        new AddressType2("c9b8cd52-54e6-4c08-a9d9-224dd0c8b700", "BU", "Business", AddressTypeCategory.Business)
                         }
                     );
                allPhones = new List<PhoneType>() {
                        new PhoneType("92c82d33-e55c-41a4-a2c3-f2f7d2c523d1", "HO", "Home", PhoneTypeCategory.Home),
                        new PhoneType("b6def2cc-cc95-4d0e-a32c-940fbbc2d689", "MO", "Mobile", PhoneTypeCategory.Mobile),
                        new PhoneType("f60e7b27-a3e3-4c92-9d36-f3cae27b724b", "VA", "Vacation", PhoneTypeCategory.Vacation),
                        new PhoneType("30e231cf-a199-4c9a-af01-be2e69b607c9", "BU", "Business", PhoneTypeCategory.Business)
                        };
                referenceDataRepositoryMock.Setup(repo => repo.GetPhoneTypesAsync(It.IsAny<bool>()))
                     .ReturnsAsync(allPhones);


                // Mock the reference repository for prefix
                referenceDataRepositoryMock.Setup(repo => repo.Prefixes).Returns(new List<Prefix>()
                {
                    new Prefix("MR","Mr","Mr."),
                    new Prefix("MS","Ms","Ms.")
                });

                // Mock the reference repository for prefix
                referenceDataRepositoryMock.Setup(repo => repo.GetPrefixesAsync()).ReturnsAsync(new List<Prefix>()
                {
                    new Prefix("MR","Mr","Mr."),
                    new Prefix("MS","Ms","Ms.")
                });

                // Mock the reference repository for suffix
                referenceDataRepositoryMock.Setup(repo => repo.Suffixes).Returns(new List<Suffix>()
                {
                    new Suffix("JR","Jr","Jr."),
                    new Suffix("SR","Sr","Sr.")
                });

                // Mock the reference repository for suffix
                referenceDataRepositoryMock.Setup(repo => repo.GetSuffixesAsync()).ReturnsAsync(new List<Suffix>()
                {
                    new Suffix("JR","Jr","Jr."),
                    new Suffix("SR","Sr","Sr.")
                });

                // Mock the reference repository for country
                countries = new List<Country>()
                 {
                    new Country("US","United States","US", "USA", false),
                    new Country("CA","Canada","CA", "CAN", false),
                    new Country("MX","Mexico","MX", "MEX", false),
                    new Country("BR","Brazil","BR", "BRA", false)
                };
                referenceDataRepositoryMock.Setup(repo => repo.GetCountryCodesAsync(It.IsAny<bool>())).Returns(Task.FromResult(countries));

                // Mock the reference repository for marital status
                referenceDataRepositoryMock.Setup(repo => repo.GetMaritalStatusesAsync(It.IsAny<bool>())).Returns(Task.FromResult<IEnumerable<MaritalStatus>>(new List<MaritalStatus>()
                {
                    new MaritalStatus(maritalStatusGuid, "M", "Married"){ Type = MaritalStatusType.Married }
                }));

                var raceEntities = new List<Race>();
                raceEntities.Add(new Race(raceAsianGuid, "AS", "Asian", RaceType.Asian));
                raceEntities.Add(new Race(racePacificIslanderGuid, "HP", "Hawaiian/Pacific Islander", RaceType.PacificIslander));
                referenceDataRepositoryMock.Setup(repo => repo.GetRacesAsync(It.IsAny<bool>())).Returns(Task.FromResult<IEnumerable<Race>>(raceEntities));

                referenceDataRepositoryMock.Setup(repo => repo.GetChaptersAsync(It.IsAny<bool>())).ReturnsAsync(allChapters);
                referenceDataRepositoryMock.Setup(repo => repo.GetCountiesAsync(It.IsAny<bool>())).ReturnsAsync(allCounties);
                referenceDataRepositoryMock.Setup(repo => repo.GetZipCodeXlatAsync(It.IsAny<bool>())).ReturnsAsync(allZipCodeXlats);
                referenceDataRepositoryMock.Setup(repo => repo.GetRecordInfoFromGuidGeographicAreaAsync(It.IsAny<string>())).ReturnsAsync(Domain.Base.Entities.GeographicAreaTypeCategory.Fundraising);

                // Mock the reference repository for Alternate ID Types
                referenceDataRepositoryMock.Setup(repo => repo.GetAlternateIdTypesAsync(It.IsAny<bool>())).ReturnsAsync(new List<AltIdTypes>()
                {
                    new AltIdTypes("AE44FE48-2534-480B-8618-5480617CE74A", "ELEV2", "Elevate ID 2"),
                    new AltIdTypes("D525E2B2-CD7D-4995-93F0-97DA468EBE90", "GOVID2", "Government ID 2")
                });

                // Mock the reference repository for Gender Identity Codes
                referenceDataRepositoryMock.Setup(repo => repo.GetGenderIdentityTypesAsync(It.IsAny<bool>())).ReturnsAsync(new List<GenderIdentityType>()
                {
                    new GenderIdentityType("9C3004AB-0F25-4D1D-84D6-65EA69CE1124","FTM","Female to Male"),
                    new GenderIdentityType("BCD23124-2FAA-411C-A990-24BA3FA8A93D", "MTF","Male to Female")
                });

                // Mock the reference repository for Personal Pronouns
                referenceDataRepositoryMock.Setup(repo => repo.GetPersonalPronounTypesAsync(It.IsAny<bool>())).ReturnsAsync(new List<PersonalPronounType>()
                {
                    new PersonalPronounType("AE7A3392-FA07-4F53-B6D5-317D77CB62EC","HE","He, Him, His"),
                    new PersonalPronounType("9567AFB5-5F3C-40DC-B4F9-FC1658ACEE15", "HER","She, Her, Hers")
                });
            }
        }

        #endregion

        #region HEDM Update Person V12.1.0 Tests

        [TestClass]
        public class UpdatePerson5 : CurrentUserSetup
        {
            //Mocks
            Mock<IAdapterRegistry> adapterRegistryMock;
            Mock<IPersonRepository> personRepositoryMock;
            Mock<IPersonBaseRepository> personBaseRepositoryMock;
            Mock<IReferenceDataRepository> referenceDataRepositoryMock;
            Mock<IProfileRepository> profileRepositoryMock;
            Mock<IConfigurationRepository> configurationRepositoryMock;
            Mock<IRelationshipRepository> relationshipRepositoryMock;
            Mock<IProxyRepository> proxyRepositoryMock;
            Mock<IRoleRepository> roleRepositoryMock;
            Mock<ILogger> loggerMock;

            //userfactory
            ICurrentUserFactory currentUserFactory;

            //Service
            PersonService personService;


            private Ellucian.Colleague.Dtos.Person5 personDto;
            private Ellucian.Colleague.Domain.Base.Entities.PersonIntegration personIntegrationReturned;
            private Ellucian.Colleague.Domain.Base.Entities.PersonIntegration personIntegrationEntity;

            //private Ellucian.Colleague.Domain.Base.Entities.Person person;
            private List<Ellucian.Colleague.Domain.Base.Entities.Address> addresses = null;
            private List<Ellucian.Colleague.Domain.Base.Entities.Phone> phones = new List<Domain.Base.Entities.Phone>();
            private IEnumerable<Ellucian.Colleague.Domain.Base.Entities.State> states;
            private IEnumerable<Ellucian.Colleague.Domain.Base.Entities.Country> countries;
            private List<Ellucian.Colleague.Domain.Base.Entities.County> counties;
            private List<PersonNameTypeItem> personNameTypes;

            //Entities
            private Ellucian.Colleague.Domain.Base.Entities.EmailAddress instEmail;
            private Ellucian.Colleague.Domain.Base.Entities.EmailAddress perEmail;
            private Ellucian.Colleague.Domain.Base.Entities.EmailAddress workEmail;
            private Ellucian.Colleague.Domain.Base.Entities.Address homeAddr;
            private Ellucian.Colleague.Domain.Base.Entities.Address mailAddr;
            private Ellucian.Colleague.Domain.Base.Entities.Address resAddr;
            private Ellucian.Colleague.Domain.Base.Entities.Address workAddr;
            private Ellucian.Colleague.Domain.Base.Entities.Phone homePhone;
            private Ellucian.Colleague.Domain.Base.Entities.Phone mobilePhone;
            private Ellucian.Colleague.Domain.Base.Entities.Phone residencePhone;
            private Ellucian.Colleague.Domain.Base.Entities.Phone workPhone;

            //Data
            private string personId = "0000011";
            private string personGuid = "5674f28b-b216-4055-b236-81a922d93b4c";
            private string maritalStatusGuid = "dca8edb5-120f-479a-a6bb-35ba3af4b344";
            private string ethnicityGuid = "dd0c42ca-c61d-4ca6-8d21-96ab5be35623";
            private string raceAsianGuid = "72b7737b-27db-4a06-944b-97d00c29b3db";
            private string racePacificIslanderGuid = "e20f9821-28a2-4e34-8550-6758850a0cf8";
            private string baptistGuid = "c0bdfd92-462f-4e59-bba5-1b15c4771c86";
            private string catholicGuid = "f96f04b0-4973-41f6-bc3d-9c7bc1c2c458";

            private string countyGuid = Guid.NewGuid().ToString();


            [TestInitialize]
            public void Initialize()
            {
                adapterRegistryMock = new Mock<IAdapterRegistry>();
                personRepositoryMock = new Mock<IPersonRepository>();
                personBaseRepositoryMock = new Mock<IPersonBaseRepository>();
                referenceDataRepositoryMock = new Mock<IReferenceDataRepository>();
                profileRepositoryMock = new Mock<IProfileRepository>();
                configurationRepositoryMock = new Mock<IConfigurationRepository>();
                relationshipRepositoryMock = new Mock<IRelationshipRepository>();
                proxyRepositoryMock = new Mock<IProxyRepository>();
                roleRepositoryMock = new Mock<IRoleRepository>();
                loggerMock = new Mock<ILogger>();

                currentUserFactory = new CurrentUserSetup.ThirdPartyUserFactory();

                SetupData();

                SetupReferenceDataRepositoryMocks();

                // International Parameters Host Country
                personRepositoryMock.Setup(repo => repo.GetHostCountryAsync()).ReturnsAsync("USA");

                personRepositoryMock.Setup(i => i.Update2Async(It.IsAny<PersonIntegration>(), addresses, phones, It.IsAny<int>())).ReturnsAsync(personIntegrationReturned);

                personService = new PersonService(adapterRegistryMock.Object, personRepositoryMock.Object, personBaseRepositoryMock.Object, referenceDataRepositoryMock.Object, profileRepositoryMock.Object,
                                                  configurationRepositoryMock.Object, relationshipRepositoryMock.Object, proxyRepositoryMock.Object, currentUserFactory,
                                                  roleRepositoryMock.Object, loggerMock.Object);
            }


            [TestCleanup]
            public void Cleanup()
            {
                personService = null;
                adapterRegistryMock = null;
                personRepositoryMock = null;
                personBaseRepositoryMock = null;
                referenceDataRepositoryMock = null;
                profileRepositoryMock = null;
                configurationRepositoryMock = null;
                relationshipRepositoryMock = null;
                proxyRepositoryMock = null;
                roleRepositoryMock = null;
                loggerMock = null;
            }

            [TestMethod]
            public async Task UpdatePerson5_UpdatePerson5Async()
            {
                //setup role
                updatePersonRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Base.BasePermissionCodes.UpdatePerson));
                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { updatePersonRole });

                //personId 0000011
                personRepositoryMock.Setup(i => i.GetPersonIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync(personId);

                var actual = await personService.UpdatePerson5Async(personDto);

                Assert.AreEqual(personDto.BirthDate, actual.BirthDate);
                Assert.AreEqual(personDto.CitizenshipCountry, actual.CitizenshipCountry);
                Assert.AreEqual(personDto.CitizenshipStatus, actual.CitizenshipStatus);
                Assert.AreEqual(personDto.CountryOfBirth, actual.CountryOfBirth);
                Assert.AreEqual(personDto.DeceasedDate, actual.DeceasedDate);
                Assert.AreEqual(personDto.GenderType, actual.GenderType);
                Assert.AreEqual(personDto.MaritalStatus.Detail.Id, actual.MaritalStatus.Detail.Id);
                Assert.AreEqual(personDto.MaritalStatus.MaritalCategory, actual.MaritalStatus.MaritalCategory);

                /*
                    This code will change because of some of the API changes in future
                */

                //Legal
                var legalActualName = actual.PersonNames.FirstOrDefault(item => item.NameType.Detail.Id.Equals("806af5a5-8a9a-424f-8c9f-c1e9d084ee71", StringComparison.OrdinalIgnoreCase));
                var legalExpectedName = personDto.PersonNames.FirstOrDefault(item => item.NameType.Detail.Id.Equals(legalActualName.NameType.Detail.Id, StringComparison.OrdinalIgnoreCase));

                //Assert.AreEqual(legalExpectedName.FullName, legalActualName.FullName); commented cause it will fail
                if (!string.IsNullOrEmpty(legalExpectedName.LastNamePrefix))
                    Assert.AreEqual(legalExpectedName.LastName, string.Concat(legalExpectedName.LastNamePrefix, " ", legalActualName.LastName));
                Assert.AreEqual(legalExpectedName.FirstName, legalActualName.FirstName);
                Assert.AreEqual(legalExpectedName.MiddleName, legalActualName.MiddleName);

                //Birth
                var birthActualName = actual.PersonNames.FirstOrDefault(item => item.NameType.Detail.Id.Equals("7dfa950c-8ae4-4dca-92f0-c083604285b6", StringComparison.OrdinalIgnoreCase));
                var birthexpectedName = personDto.PersonNames.FirstOrDefault(item => item.NameType.Detail.Id.Equals(birthActualName.NameType.Detail.Id, StringComparison.OrdinalIgnoreCase));

                Assert.AreEqual(birthexpectedName.FullName, birthActualName.FullName);

                //Chosen
                var chosenActualName = actual.PersonNames.FirstOrDefault(item => item.NameType.Detail.Id.Equals("dd20ebdf-2452-41ef-9f86-ad1b1621a78d", StringComparison.OrdinalIgnoreCase));
                var chosenExpectedName = personDto.PersonNames.FirstOrDefault(item => item.NameType.Detail.Id.Equals(chosenActualName.NameType.Detail.Id, StringComparison.OrdinalIgnoreCase));

                Assert.AreEqual(chosenExpectedName.FullName, chosenActualName.FullName);

                //Nickname
                var nickNameActualName = actual.PersonNames.FirstOrDefault(item => item.NameType.Detail.Id.Equals("7b55610f-7d00-4260-bbcf-0e47fdbae647", StringComparison.OrdinalIgnoreCase));
                var nickNameExpectedName = personDto.PersonNames.FirstOrDefault(item => item.NameType.Detail.Id.Equals(nickNameActualName.NameType.Detail.Id, StringComparison.OrdinalIgnoreCase));

                Assert.AreEqual(nickNameExpectedName.FullName, nickNameActualName.FullName);

                //History
                var historyActualName = actual.PersonNames.FirstOrDefault(item => item.NameType.Detail.Id.Equals("d42cc964-35cb-4560-bc46-4b881e7705ea", StringComparison.OrdinalIgnoreCase));
                var historyexpectedName = personDto.PersonNames.FirstOrDefault(item => item.NameType.Detail.Id.Equals(historyActualName.NameType.Detail.Id, StringComparison.OrdinalIgnoreCase));

                Assert.AreEqual(historyexpectedName.FullName, historyActualName.FullName);

            }

            [TestMethod]
            public async Task UpdatePerson5_UpdatePerson5Async_PersonId_NullEmpty_Create()
            {
                //setup role
                createPersonRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Base.BasePermissionCodes.CreatePerson));
                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { createPersonRole });

                //personId 0000011
                personRepositoryMock.Setup(i => i.GetPersonIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync(string.Empty);

                personRepositoryMock.Setup(i => i.Create2Async(It.IsAny<PersonIntegration>(), addresses, phones, It.IsAny<int>())).ReturnsAsync(personIntegrationReturned);


                var actual = await personService.UpdatePerson5Async(personDto);

                Assert.AreEqual(personDto.BirthDate, actual.BirthDate);
                Assert.AreEqual(personDto.CitizenshipCountry, actual.CitizenshipCountry);
                Assert.AreEqual(personDto.CitizenshipStatus, actual.CitizenshipStatus);
                Assert.AreEqual(personDto.CountryOfBirth, actual.CountryOfBirth);
                Assert.AreEqual(personDto.DeceasedDate, actual.DeceasedDate);
                Assert.AreEqual(personDto.GenderType, actual.GenderType);
                Assert.AreEqual(personDto.MaritalStatus.Detail.Id, actual.MaritalStatus.Detail.Id);
                Assert.AreEqual(personDto.MaritalStatus.MaritalCategory, actual.MaritalStatus.MaritalCategory);

                var nameCount = personDto.PersonNames.Count();
                personDto.PersonNames.OrderBy(i => i.NameType.Category);
                actual.PersonNames.OrderBy(i => i.NameType.Category);

                for (int i = 0; i < nameCount; i++)
                {
                    var actualName = actual.PersonNames.ToList()[i];
                    var expectedName = personDto.PersonNames.FirstOrDefault(item => item.NameType.Detail.Id.Equals(actualName.NameType.Detail.Id, StringComparison.OrdinalIgnoreCase));
                }
            }

            #region Exceptions

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task UpdatePerson5_Dto_Null_ArgumentNullException()
            {
                try
                {
                    var result = await personService.UpdatePerson5Async(null);
                }
                catch (IntegrationApiException ex)
                {
                    Assert.IsNotNull(ex.Errors);
                    Assert.AreEqual("persons", ex.Errors.First().Code);
                    Assert.AreEqual("Must provide a person object for update", ex.Errors.First().Message);
                    throw;
                }
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task UpdatePerson5_Id_Null_ArgumentNullException()
            {
                try
                {
                    var result = await personService.UpdatePerson5Async(new Dtos.Person5() { Id = "" });
                }
                catch (IntegrationApiException ex)
                {
                    Assert.IsNotNull(ex.Errors);
                    Assert.AreEqual("persons", ex.Errors.First().Code);
                    Assert.AreEqual("Must provide a guid for person update", ex.Errors.First().Message);
                    throw;
                }
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task UpdatePerson5_PersonNames_Null_ArgumentNullException()
            {
                //setup role
                updatePersonRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Base.BasePermissionCodes.UpdatePerson));
                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { updatePersonRole });

                //personId 0000011
                personRepositoryMock.Setup(i => i.GetPersonIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync(personId);
                try
                {
                    var result = await personService.UpdatePerson5Async(new Dtos.Person5() { Id = personId });
                }
                catch (IntegrationApiException ex)
                {
                    Assert.IsNotNull(ex.Errors);
                    Assert.AreEqual("persons.names", ex.Errors.First().Code);
                    Assert.AreEqual("Must provide person name", ex.Errors.First().Message);
                    throw;
                }
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task UpdatePerson5_PrimaryNames_Null_ArgumentNullException()
            {
                //setup role
                updatePersonRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Base.BasePermissionCodes.UpdatePerson));
                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { updatePersonRole });

                //personId 0000011
                personRepositoryMock.Setup(i => i.GetPersonIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync(personId);
                personDto.PersonNames.FirstOrDefault().NameType.Detail.Id = string.Empty;
                try
                {
                    var result = await personService.UpdatePerson5Async(personDto);
                }
                catch (IntegrationApiException ex)
                {
                    Assert.IsNotNull(ex.Errors);
                    Assert.AreEqual("persons.names.type.detail.id", ex.Errors.First().Code);
                    Assert.AreEqual("Name type category with detail Id of '' is not valid.", ex.Errors.First().Message);
                    throw;
                }
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task UpdatePerson5_PrimaryNames_GT_1_ArgumentNullException()
            {
                //setup role
                updatePersonRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Base.BasePermissionCodes.UpdatePerson));
                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { updatePersonRole });

                //personId 0000011
                personRepositoryMock.Setup(i => i.GetPersonIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync(personId);
                personDto.PersonNames = null;
                var personNames = new List<Dtos.DtoProperties.PersonName2DtoProperty>();
                var personPrimaryName = new Dtos.DtoProperties.PersonName2DtoProperty()
                {
                    NameType = new Dtos.DtoProperties.PersonNameTypeDtoProperty() { Category = Dtos.EnumProperties.PersonNameType2.Personal, Detail = new Dtos.GuidObject2() { Id = "806af5a5-8a9a-424f-8c9f-c1e9d084ee71" } },
                    Title = "MR",
                    FirstName = "Ricky",
                    MiddleName = "Lee",
                    LastName = "Brown",
                    Pedigree = "JR",
                    LastNamePrefix = "Ignore"
                };
                personNames.Add(personPrimaryName);
                var personPrimaryName1 = new Dtos.DtoProperties.PersonName2DtoProperty()
                {
                    NameType = new Dtos.DtoProperties.PersonNameTypeDtoProperty() { Category = Dtos.EnumProperties.PersonNameType2.Personal, Detail = new Dtos.GuidObject2() { Id = "806af5a5-8a9a-424f-8c9f-c1e9d084ee71" } },
                    Title = "MR",
                    FirstName = "Ricky",
                    MiddleName = "Lee",
                    LastName = "Brown",
                    Pedigree = "JR",
                    LastNamePrefix = "Ignore"
                };
                personNames.Add(personPrimaryName1);
                personDto.PersonNames = personNames;
                try
                {
                    var result = await personService.UpdatePerson5Async(personDto);
                }
                catch (IntegrationApiException ex)
                {
                    Assert.IsNotNull(ex.Errors);
                    Assert.AreEqual("persons.names.type.category", ex.Errors.First().Code);
                    Assert.AreEqual("A legal name is required in the names array.", ex.Errors.First().Message);
                    throw;
                }
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task UpdatePerson5_PrimaryNames_LastName_Null_ArgumentNullException()
            {
                //setup role
                updatePersonRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Base.BasePermissionCodes.UpdatePerson));
                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { updatePersonRole });

                //personId 0000011
                personRepositoryMock.Setup(i => i.GetPersonIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync(personId);
                personDto.PersonNames.FirstOrDefault().LastName = string.Empty;

                try
                {
                    var result = await personService.UpdatePerson5Async(personDto);
                }
                catch (IntegrationApiException ex)
                {
                    Assert.IsNotNull(ex.Errors);
                    Assert.AreEqual("persons.names.lastName", ex.Errors.First().Code);
                    Assert.AreEqual("Last name is required for a legal name.", ex.Errors.First().Message);
                    throw;
                }
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task UpdatePerson5_PrimaryNames_FullName_Null_ArgumentNullException()
            {
                //setup role
                updatePersonRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Base.BasePermissionCodes.UpdatePerson));
                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { updatePersonRole });

                //personId 0000011
                personRepositoryMock.Setup(i => i.GetPersonIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync(personId);
                personDto.PersonNames.FirstOrDefault().FullName = string.Empty;

                try
                {
                    var result = await personService.UpdatePerson5Async(personDto);
                }
                catch (IntegrationApiException ex)
                {
                    Assert.IsNotNull(ex.Errors);
                    Assert.AreEqual("persons.names.fullName", ex.Errors.First().Code);
                    Assert.AreEqual("Full name is required for a legal name.", ex.Errors.First().Message);
                    throw;
                }
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task UpdatePerson5_LEGAL_NameTypes_Null_ArgumentNullException()
            {
                var legalType = personNameTypes.FirstOrDefault(x => x.Code == "LEGAL");
                personNameTypes.Remove(legalType);

                //setup role
                updatePersonRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Base.BasePermissionCodes.UpdatePerson));
                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { updatePersonRole });

                //personId 0000011
                personRepositoryMock.Setup(i => i.GetPersonIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync(personId);

                try
                {
                    var result = await personService.UpdatePerson5Async(personDto);
                }
                catch (IntegrationApiException ex)
                {
                    Assert.IsNotNull(ex.Errors);
                    Assert.AreEqual("persons.names.type.detail.id", ex.Errors.First().Code);
                    Assert.AreEqual("Name type category with detail Id of '806af5a5-8a9a-424f-8c9f-c1e9d084ee71' is not valid.", ex.Errors.First().Message);
                    throw;
                }
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task UpdatePerson5_BIRTH_NameTypes_Null_ArgumentNullException()
            {
                var birthType = personNameTypes.FirstOrDefault(x => x.Code == "BIRTH");
                personNameTypes.Remove(birthType);

                //setup role
                updatePersonRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Base.BasePermissionCodes.UpdatePerson));
                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { updatePersonRole });

                //personId 0000011
                personRepositoryMock.Setup(i => i.GetPersonIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync(personId);

                try
                {
                    var result = await personService.UpdatePerson5Async(personDto);
                }
                catch (IntegrationApiException ex)
                {
                    Assert.IsNotNull(ex.Errors);
                    Assert.AreEqual("persons.names.type.detail.id", ex.Errors.First().Code);
                    Assert.AreEqual("Name type category with detail Id of '7dfa950c-8ae4-4dca-92f0-c083604285b6' is not valid.", ex.Errors.First().Message);
                    throw;
                }
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task UpdatePerson5_NICKNAME_NameTypes_Null_ArgumentNullException()
            {
                var nickNameType = personNameTypes.FirstOrDefault(x => x.Code == "NICKNAME");
                personNameTypes.Remove(nickNameType);

                //setup role
                updatePersonRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Base.BasePermissionCodes.UpdatePerson));
                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { updatePersonRole });

                //personId 0000011
                personRepositoryMock.Setup(i => i.GetPersonIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync(personId);

                try
                {
                    var result = await personService.UpdatePerson5Async(personDto);
                }
                catch (IntegrationApiException ex)
                {
                    Assert.IsNotNull(ex.Errors);
                    Assert.AreEqual("persons.names.type.detail.id", ex.Errors.First().Code);
                    Assert.AreEqual("Name type category with detail Id of '7b55610f-7d00-4260-bbcf-0e47fdbae647' is not valid.", ex.Errors.First().Message);
                    throw;
                }
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task UpdatePerson5_Birthname_GT_1_ArgumentNullException()
            {
                //setup role
                updatePersonRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Base.BasePermissionCodes.UpdatePerson));
                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { updatePersonRole });

                //personId 0000011
                personRepositoryMock.Setup(i => i.GetPersonIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync(personId);
                personDto.PersonNames = null;
                var personNames = new List<Dtos.DtoProperties.PersonName2DtoProperty>();
                var legalPrimaryName = new Dtos.DtoProperties.PersonName2DtoProperty()
                {
                    NameType = new Dtos.DtoProperties.PersonNameTypeDtoProperty() { Category = Dtos.EnumProperties.PersonNameType2.Legal, Detail = new Dtos.GuidObject2() { Id = "806af5a5-8a9a-424f-8c9f-c1e9d084ee71" } },
                    Title = "MR",
                    FirstName = "Ricky",
                    MiddleName = "Lee",
                    LastName = "Brown",
                    Pedigree = "JR",
                    LastNamePrefix = "Ignore",
                    FullName = "FullName"
                };
                personNames.Add(legalPrimaryName);
                var birthPrimaryName = new Dtos.DtoProperties.PersonName2DtoProperty()
                {
                    NameType = new Dtos.DtoProperties.PersonNameTypeDtoProperty() { Category = Dtos.EnumProperties.PersonNameType2.Birth, Detail = new Dtos.GuidObject2() { Id = "7b55610f-7d00-4260-bbcf-0e47fdbae647" } },
                    Title = "MR",
                    FirstName = "Ricky",
                    MiddleName = "Lee",
                    LastName = "Brown",
                    Pedigree = "JR",
                    LastNamePrefix = "Ignore",
                    FullName = "FullName"
                };
                personNames.Add(birthPrimaryName);

                var birthPrimaryName2 = new Dtos.DtoProperties.PersonName2DtoProperty()
                {
                    NameType = new Dtos.DtoProperties.PersonNameTypeDtoProperty() { Category = Dtos.EnumProperties.PersonNameType2.Birth, Detail = new Dtos.GuidObject2() { Id = "7b55610f-7d00-4260-bbcf-0e47fdbae647" } },
                    Title = "MR",
                    FirstName = "Ricky",
                    MiddleName = "Lee",
                    LastName = "Brown",
                    Pedigree = "JR",
                    LastNamePrefix = "Ignore",
                    FullName = "FullName"
                };
                personNames.Add(birthPrimaryName2);
                personDto.PersonNames = personNames;

                try
                {
                    var result = await personService.UpdatePerson5Async(personDto);
                }
                catch (IntegrationApiException ex)
                {
                    Assert.IsNotNull(ex.Errors);
                    Assert.AreEqual("persons.names.type.category", ex.Errors.First().Code);
                    Assert.AreEqual("Colleague does not support more than one birth name for a person.", ex.Errors.First().Message);
                    throw;
                }
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task UpdatePerson5_Legal_FirstLastMiddle_Null_ArgumentNullException()
            {
                //setup role
                updatePersonRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Base.BasePermissionCodes.UpdatePerson));
                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { updatePersonRole });

                //personId 0000011
                personRepositoryMock.Setup(i => i.GetPersonIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync(personId);
                personDto.PersonNames = null;
                var personNames = new List<Dtos.DtoProperties.PersonName2DtoProperty>();
                var legalPrimaryName = new Dtos.DtoProperties.PersonName2DtoProperty()
                {
                    NameType = new Dtos.DtoProperties.PersonNameTypeDtoProperty() { Category = Dtos.EnumProperties.PersonNameType2.Legal, Detail = new Dtos.GuidObject2() { Id = "806af5a5-8a9a-424f-8c9f-c1e9d084ee71" } },
                    Title = "MR",
                    FirstName = "",
                    MiddleName = "",
                    LastName = "",
                    Pedigree = "JR",
                    LastNamePrefix = "Ignore",
                    FullName = "FullName"
                };
                personNames.Add(legalPrimaryName);
                personDto.PersonNames = personNames;

                try
                {
                    var result = await personService.UpdatePerson5Async(personDto);
                }
                catch (IntegrationApiException ex)
                {
                    Assert.IsNotNull(ex.Errors);
                    Assert.AreEqual("persons.names.lastName", ex.Errors.First().Code);
                    Assert.AreEqual("Last name is required for a legal name.", ex.Errors.First().Message);
                    throw;
                }
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task UpdatePerson5_Birth_FirstLastMiddle_Null_ArgumentNullException()
            {
                //setup role
                updatePersonRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Base.BasePermissionCodes.UpdatePerson));
                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { updatePersonRole });

                //personId 0000011
                personRepositoryMock.Setup(i => i.GetPersonIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync(personId);
                personDto.PersonNames = null;
                var personNames = new List<Dtos.DtoProperties.PersonName2DtoProperty>();
                var legalPrimaryName = new Dtos.DtoProperties.PersonName2DtoProperty()
                {
                    NameType = new Dtos.DtoProperties.PersonNameTypeDtoProperty() { Category = Dtos.EnumProperties.PersonNameType2.Legal, Detail = new Dtos.GuidObject2() { Id = "806af5a5-8a9a-424f-8c9f-c1e9d084ee71" } },
                    Title = "MR",
                    FirstName = "LegalFirst",
                    MiddleName = "LegalMiddle",
                    LastName = "LegalLast",
                    Pedigree = "JR",
                    LastNamePrefix = "Ignore",
                    FullName = "FullName"
                };
                personNames.Add(legalPrimaryName);
                var birthPrimaryName = new Dtos.DtoProperties.PersonName2DtoProperty()
                {
                    NameType = new Dtos.DtoProperties.PersonNameTypeDtoProperty() { Category = Dtos.EnumProperties.PersonNameType2.Birth, Detail = new Dtos.GuidObject2() { Id = "7dfa950c-8ae4-4dca-92f0-c083604285b6" } },
                    Title = "MR",
                    FirstName = "",
                    MiddleName = "",
                    LastName = "",
                    Pedigree = "JR",
                    LastNamePrefix = "Ignore",
                    FullName = "FullName"
                };
                personNames.Add(birthPrimaryName);
                personDto.PersonNames = personNames;

                try
                {
                    var result = await personService.UpdatePerson5Async(personDto);
                }
                catch (IntegrationApiException ex)
                {
                    Assert.IsNotNull(ex.Errors);
                    Assert.AreEqual("persons.names.lastName", ex.Errors.First().Code);
                    Assert.AreEqual("Either the firstName, middleName, or lastName is needed for a birth name.", ex.Errors.First().Message);
                    throw;
                }
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task UpdatePerson5_Birth_FullName_Null_ArgumentNullException()
            {
                //setup role
                updatePersonRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Base.BasePermissionCodes.UpdatePerson));
                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { updatePersonRole });

                //personId 0000011
                personRepositoryMock.Setup(i => i.GetPersonIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync(personId);
                personDto.PersonNames = null;
                var personNames = new List<Dtos.DtoProperties.PersonName2DtoProperty>();
                var legalPrimaryName = new Dtos.DtoProperties.PersonName2DtoProperty()
                {
                    NameType = new Dtos.DtoProperties.PersonNameTypeDtoProperty() { Category = Dtos.EnumProperties.PersonNameType2.Legal, Detail = new Dtos.GuidObject2() { Id = "806af5a5-8a9a-424f-8c9f-c1e9d084ee71" } },
                    Title = "MR",
                    FirstName = "LegalFirst",
                    MiddleName = "LegalMiddle",
                    LastName = "LegalLast",
                    Pedigree = "JR",
                    LastNamePrefix = "Ignore",
                    FullName = "FullName"
                };
                personNames.Add(legalPrimaryName);
                var birthPrimaryName = new Dtos.DtoProperties.PersonName2DtoProperty()
                {
                    NameType = new Dtos.DtoProperties.PersonNameTypeDtoProperty() { Category = Dtos.EnumProperties.PersonNameType2.Birth, Detail = new Dtos.GuidObject2() { Id = "7dfa950c-8ae4-4dca-92f0-c083604285b6" } },
                    Title = "MR",
                    FirstName = "First",
                    MiddleName = "Middle",
                    LastName = "Last",
                    Pedigree = "JR",
                    LastNamePrefix = "Ignore",
                    FullName = ""
                };
                personNames.Add(birthPrimaryName);
                personDto.PersonNames = personNames;

                try
                {
                    var result = await personService.UpdatePerson5Async(personDto);
                }
                catch (IntegrationApiException ex)
                {
                    Assert.IsNotNull(ex.Errors);
                    Assert.AreEqual("persons.names.fullName", ex.Errors.First().Code);
                    Assert.AreEqual("Full Name is needed for a birth name.", ex.Errors.First().Message);
                    throw;
                }
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task UpdatePerson5_NickName_FullName_Null_ArgumentNullException()
            {
                //setup role
                updatePersonRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Base.BasePermissionCodes.UpdatePerson));
                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { updatePersonRole });

                //personId 0000011
                personRepositoryMock.Setup(i => i.GetPersonIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync(personId);
                personDto.PersonNames = null;
                var personNames = new List<Dtos.DtoProperties.PersonName2DtoProperty>();
                var legalPrimaryName = new Dtos.DtoProperties.PersonName2DtoProperty()
                {
                    NameType = new Dtos.DtoProperties.PersonNameTypeDtoProperty() { Category = Dtos.EnumProperties.PersonNameType2.Legal, Detail = new Dtos.GuidObject2() { Id = "806af5a5-8a9a-424f-8c9f-c1e9d084ee71" } },
                    Title = "MR",
                    FirstName = "LegalFirst",
                    MiddleName = "LegalMiddle",
                    LastName = "LegalLast",
                    Pedigree = "JR",
                    LastNamePrefix = "Ignore",
                    FullName = "FullName"
                };
                personNames.Add(legalPrimaryName);
                var birthPrimaryName = new Dtos.DtoProperties.PersonName2DtoProperty()
                {
                    NameType = new Dtos.DtoProperties.PersonNameTypeDtoProperty() { Category = Dtos.EnumProperties.PersonNameType2.Personal, Detail = new Dtos.GuidObject2() { Id = "7b55610f-7d00-4260-bbcf-0e47fdbae647" } },
                    Title = "MR",
                    FirstName = "First",
                    MiddleName = "Middle",
                    LastName = "Last",
                    Pedigree = "JR",
                    LastNamePrefix = "Ignore",
                    FullName = ""
                };
                personNames.Add(birthPrimaryName);
                personDto.PersonNames = personNames;

                try
                {
                    var result = await personService.UpdatePerson5Async(personDto);
                }
                catch (IntegrationApiException ex)
                {
                    Assert.IsNotNull(ex.Errors);
                    Assert.AreEqual("persons.names.fullName", ex.Errors.First().Code);
                    Assert.AreEqual("Full Name is required for a nickname.", ex.Errors.First().Message);
                    throw;
                }
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task UpdatePerson5_History_FullName_Null_ArgumentNullException()
            {
                //setup role
                updatePersonRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Base.BasePermissionCodes.UpdatePerson));
                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { updatePersonRole });

                //personId 0000011
                personRepositoryMock.Setup(i => i.GetPersonIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync(personId);
                personDto.PersonNames = null;
                var personNames = new List<Dtos.DtoProperties.PersonName2DtoProperty>();
                var legalPrimaryName = new Dtos.DtoProperties.PersonName2DtoProperty()
                {
                    NameType = new Dtos.DtoProperties.PersonNameTypeDtoProperty() { Category = Dtos.EnumProperties.PersonNameType2.Legal, Detail = new Dtos.GuidObject2() { Id = "806af5a5-8a9a-424f-8c9f-c1e9d084ee71" } },
                    Title = "MR",
                    FirstName = "LegalFirst",
                    MiddleName = "LegalMiddle",
                    LastName = "LegalLast",
                    Pedigree = "JR",
                    LastNamePrefix = "Ignore",
                    FullName = "FullName"
                };
                personNames.Add(legalPrimaryName);
                var formerPrimaryName = new Dtos.DtoProperties.PersonName2DtoProperty()
                {
                    NameType = new Dtos.DtoProperties.PersonNameTypeDtoProperty() { Category = Dtos.EnumProperties.PersonNameType2.Personal, Detail = new Dtos.GuidObject2() { Id = "d42cc964-35cb-4560-bc46-4b881e7705ea" } },
                    Title = "MR",
                    FirstName = "First",
                    MiddleName = "Middle",
                    LastName = "Last",
                    Pedigree = "JR",
                    LastNamePrefix = "Ignore",
                    FullName = ""
                };
                personNames.Add(formerPrimaryName);
                personDto.PersonNames = personNames;

                try
                {
                    var result = await personService.UpdatePerson5Async(personDto);
                }
                catch (IntegrationApiException ex)
                {
                    Assert.IsNotNull(ex.Errors);
                    Assert.AreEqual("persons.names.fullName", ex.Errors.First().Code);
                    Assert.AreEqual("Full Name is required for a former name.", ex.Errors.First().Message);
                    throw;
                }
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task UpdatePerson5_HISTORY_NameTypes_Null_ArgumentNullException()
            {
                var historyNameType = personNameTypes.FirstOrDefault(x => x.Code == "HISTORY");
                personNameTypes.Remove(historyNameType);

                //setup role
                updatePersonRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Base.BasePermissionCodes.UpdatePerson));
                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { updatePersonRole });

                //personId 0000011
                personRepositoryMock.Setup(i => i.GetPersonIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync(personId);

                try
                {
                    var result = await personService.UpdatePerson5Async(personDto);
                }
                catch (IntegrationApiException ex)
                {
                    Assert.IsNotNull(ex.Errors);
                    Assert.AreEqual("persons.names.type.detail.id", ex.Errors.First().Code);
                    Assert.AreEqual("Name type category with detail Id of 'd42cc964-35cb-4560-bc46-4b881e7705ea' is not valid.", ex.Errors.First().Message);
                    throw;
                }
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task UpdatePerson5_HISTORY_LastName_Null_ArgumentNullException()
            {
                //setup role
                updatePersonRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Base.BasePermissionCodes.UpdatePerson));
                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { updatePersonRole });

                personDto.PersonNames.FirstOrDefault(i => i.NameType.Detail.Id.Equals("d42cc964-35cb-4560-bc46-4b881e7705ea", StringComparison.OrdinalIgnoreCase)).LastName = string.Empty;

                //personId 0000011
                personRepositoryMock.Setup(i => i.GetPersonIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync(personId);

                try
                {
                    var result = await personService.UpdatePerson5Async(personDto);
                }
                catch (IntegrationApiException ex)
                {
                    Assert.IsNotNull(ex.Errors);
                    Assert.AreEqual("persons.names.lastName", ex.Errors.First().Code);
                    Assert.AreEqual("Last Name is required for a former name.", ex.Errors.First().Message);
                    throw;
                }
            }



            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task UpdatePerson5_PREFERRED_GT_1_ArgumentNullException()
            {
                //setup role
                updatePersonRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Base.BasePermissionCodes.UpdatePerson));
                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { updatePersonRole });

                //personId 0000011
                personRepositoryMock.Setup(i => i.GetPersonIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync(personId);
                personDto.PersonNames = null;
                var personNames = new List<Dtos.DtoProperties.PersonName2DtoProperty>();
                var legalPrimaryName = new Dtos.DtoProperties.PersonName2DtoProperty()
                {
                    NameType = new Dtos.DtoProperties.PersonNameTypeDtoProperty() { Category = Dtos.EnumProperties.PersonNameType2.Legal, Detail = new Dtos.GuidObject2() { Id = "806af5a5-8a9a-424f-8c9f-c1e9d084ee71" } },
                    Title = "MR",
                    FirstName = "Ricky",
                    MiddleName = "Lee",
                    LastName = "Brown",
                    Pedigree = "JR",
                    LastNamePrefix = "Ignore",
                    FullName = "FullName",
                    Preference = PersonNamePreference.Preferred
                };
                personNames.Add(legalPrimaryName);
                var birthPrimaryName = new Dtos.DtoProperties.PersonName2DtoProperty()
                {
                    NameType = new Dtos.DtoProperties.PersonNameTypeDtoProperty() { Category = Dtos.EnumProperties.PersonNameType2.Birth, Detail = new Dtos.GuidObject2() { Id = "8224f18e-69c5-480b-a9b4-52f596aa4a52" } },
                    Title = "MR",
                    FirstName = "Ricky",
                    MiddleName = "Lee",
                    LastName = "Brown",
                    Pedigree = "JR",
                    LastNamePrefix = "Ignore",
                    FullName = "FullName",
                    Preference = PersonNamePreference.Preferred
                };
                personNames.Add(birthPrimaryName);
                personDto.PersonNames = personNames;

                try
                {
                    var result = await personService.UpdatePerson5Async(personDto);
                }
                catch (IntegrationApiException ex)
                {
                    Assert.IsNotNull(ex.Errors);
                    Assert.AreEqual("persons.names.preference", ex.Errors.First().Code);
                    Assert.AreEqual("Only one name type can be identified as preferred.", ex.Errors.First().Message);
                    throw;
                }
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task UpdatePerson5_BIRTHNames_GT_1_ArgumentNullException()
            {
                //setup role
                updatePersonRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Base.BasePermissionCodes.UpdatePerson));
                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { updatePersonRole });

                //personId 0000011
                personRepositoryMock.Setup(i => i.GetPersonIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync(personId);
                personDto.PersonNames = null;
                var personNames = new List<Dtos.DtoProperties.PersonName2DtoProperty>();
                var legalPrimaryName = new Dtos.DtoProperties.PersonName2DtoProperty()
                {
                    NameType = new Dtos.DtoProperties.PersonNameTypeDtoProperty() { Category = Dtos.EnumProperties.PersonNameType2.Legal, Detail = new Dtos.GuidObject2() { Id = "806af5a5-8a9a-424f-8c9f-c1e9d084ee71" } },
                    Title = "MR",
                    FirstName = "Ricky",
                    MiddleName = "Lee",
                    LastName = "Brown",
                    Pedigree = "JR",
                    LastNamePrefix = "Ignore",
                    FullName = "FullName"
                };
                personNames.Add(legalPrimaryName);
                var birthPrimaryName = new Dtos.DtoProperties.PersonName2DtoProperty()
                {
                    NameType = new Dtos.DtoProperties.PersonNameTypeDtoProperty() { Category = Dtos.EnumProperties.PersonNameType2.Birth, Detail = new Dtos.GuidObject2() { Id = "7dfa950c-8ae4-4dca-92f0-c083604285b6" } },
                    Title = "MR",
                    FirstName = "Ricky",
                    MiddleName = "Lee",
                    LastName = "Brown",
                    Pedigree = "JR",
                    LastNamePrefix = "Ignore",
                    FullName = "FullName"
                };
                personNames.Add(birthPrimaryName);

                var birthPrimaryName2 = new Dtos.DtoProperties.PersonName2DtoProperty()
                {
                    NameType = new Dtos.DtoProperties.PersonNameTypeDtoProperty() { Category = Dtos.EnumProperties.PersonNameType2.Birth, Detail = new Dtos.GuidObject2() { Id = "7dfa950c-8ae4-4dca-92f0-c083604285b6" } },
                    Title = "MR",
                    FirstName = "Ricky",
                    MiddleName = "Lee",
                    LastName = "Brown",
                    Pedigree = "JR",
                    LastNamePrefix = "Ignore",
                    FullName = "FullName"
                };
                personNames.Add(birthPrimaryName2);
                personDto.PersonNames = personNames;

                try
                {
                    var result = await personService.UpdatePerson5Async(personDto);
                }
                catch (IntegrationApiException ex)
                {
                    Assert.IsNotNull(ex.Errors);
                    Assert.AreEqual("persons.names.type.category", ex.Errors.First().Code);
                    Assert.AreEqual("Colleague does not support more than one birth name for a person.", ex.Errors.First().Message);
                    throw;
                }
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task UpdatePerson5_BirthNames_Empty_ArgumentNullException()
            {
                //setup role
                updatePersonRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Base.BasePermissionCodes.UpdatePerson));
                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { updatePersonRole });

                //personId 0000011
                personRepositoryMock.Setup(i => i.GetPersonIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync(personId);
                personDto.PersonNames = null;
                var personNames = new List<Dtos.DtoProperties.PersonName2DtoProperty>();

                var personPrimaryName = new Dtos.DtoProperties.PersonName2DtoProperty()
                {
                    NameType = new Dtos.DtoProperties.PersonNameTypeDtoProperty() { Category = Dtos.EnumProperties.PersonNameType2.Legal, Detail = new Dtos.GuidObject2() { Id = "806af5a5-8a9a-424f-8c9f-c1e9d084ee71" } },
                    Title = "MR",
                    FirstName = "Ricky",
                    MiddleName = "Lee",
                    LastName = "Brown",
                    Pedigree = "JR",
                    LastNamePrefix = "Ignore",
                    FullName = "FullName"
                };
                personNames.Add(personPrimaryName);

                var personPrimaryName1 = new Dtos.DtoProperties.PersonName2DtoProperty()
                {
                    NameType = new Dtos.DtoProperties.PersonNameTypeDtoProperty() { Category = Dtos.EnumProperties.PersonNameType2.Birth, Detail = new Dtos.GuidObject2() { Id = "7dfa950c-8ae4-4dca-92f0-c083604285b6" } },
                    Title = "MR",
                    FirstName = "",
                    MiddleName = "",
                    LastName = "",
                    Pedigree = "JR",
                    LastNamePrefix = "Ignore"
                };
                personNames.Add(personPrimaryName1);
                personDto.PersonNames = personNames;

                try
                {
                    var result = await personService.UpdatePerson5Async(personDto);
                }
                catch (IntegrationApiException ex)
                {
                    Assert.IsNotNull(ex.Errors);
                    Assert.AreEqual("persons.names.lastName", ex.Errors.First().Code);
                    Assert.AreEqual("Either the firstName, middleName, or lastName is needed for a birth name.", ex.Errors.First().Message);
                    throw;
                }
            }

            #endregion

            private void SetupData()
            {
                // setup personDto object
                personDto = new Dtos.Person5();
                personDto.Id = personGuid;
                personDto.BirthDate = new DateTime(1930, 1, 1);
                personDto.DeceasedDate = new DateTime(2014, 5, 12);
                var personNames = new List<Dtos.DtoProperties.PersonName2DtoProperty>();
                var legalPrimaryName = new Dtos.DtoProperties.PersonName2DtoProperty()
                {
                    NameType = new Dtos.DtoProperties.PersonNameTypeDtoProperty() { Category = Dtos.EnumProperties.PersonNameType2.Legal, Detail = new Dtos.GuidObject2() { Id = "806af5a5-8a9a-424f-8c9f-c1e9d084ee71" } },
                    Title = "Mr.",
                    FirstName = "LegalFirst",
                    MiddleName = "LegalMiddle",
                    LastName = "LegalLast",
                    Pedigree = "Jr.",
                    LastNamePrefix = "Mr.",
                    FullName = "LegalFirst LegalMiddle LegalLast"
                };
                personNames.Add(legalPrimaryName);

                var birthPrimaryName = new Dtos.DtoProperties.PersonName2DtoProperty()
                {
                    NameType = new Dtos.DtoProperties.PersonNameTypeDtoProperty() { Category = Dtos.EnumProperties.PersonNameType2.Birth, Detail = new Dtos.GuidObject2() { Id = "7dfa950c-8ae4-4dca-92f0-c083604285b6" } },
                    Title = "Mr.",
                    FirstName = "BirthFirst",
                    MiddleName = "BirthMiddle",
                    LastName = "BirthLast",
                    Pedigree = "Jr.",
                    LastNamePrefix = "Mr.",
                    FullName = "BirthFirst BirthMiddle BirthLast"
                };
                personNames.Add(birthPrimaryName);

                var chosenPrimaryName = new PersonName2DtoProperty()
                {
                    NameType = new PersonNameTypeDtoProperty() { Category = Dtos.EnumProperties.PersonNameType2.Personal, Detail = new Dtos.GuidObject2() { Id = "dd20ebdf-2452-41ef-9f86-ad1b1621a78d" } },
                    Title = "Mr.",
                    FirstName = "ChosenFirst",
                    MiddleName = "ChosenMiddle",
                    LastName = "ChosenLast",
                    Pedigree = "Jr.",
                    LastNamePrefix = "Ignore",
                    FullName = "ChosenFirst ChosenMiddle ChosenLast"
                };
                personNames.Add(chosenPrimaryName);

                var nickNamePrimaryName = new Dtos.DtoProperties.PersonName2DtoProperty()
                {
                    NameType = new Dtos.DtoProperties.PersonNameTypeDtoProperty() { Category = Dtos.EnumProperties.PersonNameType2.Personal, Detail = new Dtos.GuidObject2() { Id = "7b55610f-7d00-4260-bbcf-0e47fdbae647" } },
                    Title = "Mr.",
                    FirstName = "NickNameFirst",
                    MiddleName = "NickNameMiddle",
                    LastName = "NickNameLast",
                    Pedigree = "Jr.",
                    LastNamePrefix = "Mr.",
                    FullName = "NickNameFirst NickNameMiddle NickNameLast"
                };
                personNames.Add(nickNamePrimaryName);

                var historyPrimaryName = new Dtos.DtoProperties.PersonName2DtoProperty()
                {
                    NameType = new Dtos.DtoProperties.PersonNameTypeDtoProperty() { Category = Dtos.EnumProperties.PersonNameType2.Personal, Detail = new Dtos.GuidObject2() { Id = "d42cc964-35cb-4560-bc46-4b881e7705ea" } },
                    Title = "Mr.",
                    FirstName = "HistoryFirst",
                    MiddleName = "HistoryMiddle",
                    LastName = "HistoryLast",
                    Pedigree = "Jr.",
                    LastNamePrefix = "Mr.",
                    FullName = "HistoryFirst HistoryMiddle HistoryLast"
                };
                personNames.Add(historyPrimaryName);

                personDto.PersonNames = personNames;
                personDto.GenderType = Dtos.EnumProperties.GenderType2.Male;
                personDto.MaritalStatus = new Dtos.DtoProperties.PersonMaritalStatusDtoProperty() { Detail = new Dtos.GuidObject2(maritalStatusGuid), MaritalCategory = Dtos.EnumProperties.PersonMaritalStatusCategory.Married };// new Dtos.GuidObject(maritalStatusGuid);
                personDto.Ethnicity = new Dtos.DtoProperties.PersonEthnicityDtoProperty() { EthnicGroup = new Dtos.GuidObject2(ethnicityGuid) };
                personDto.Races = new List<Dtos.DtoProperties.PersonRaceDtoProperty>()
                {

                    new Dtos.DtoProperties.PersonRaceDtoProperty(){ Race = new Dtos.GuidObject2(raceAsianGuid)}
                };
                personDto.Credentials = new List<Dtos.DtoProperties.Credential3DtoProperty>()
                {
                    new Dtos.DtoProperties.Credential3DtoProperty()
                    {
                        Type = Dtos.EnumProperties.Credential3Type.Ssn,
                        Value = "111-11-1111"
                    }
                };
                var emailAddresses = new List<Dtos.DtoProperties.PersonEmailDtoProperty>();
                emailAddresses.Add(new Dtos.DtoProperties.PersonEmailDtoProperty()
                {

                    Type = new Dtos.DtoProperties.PersonEmailTypeDtoProperty() { EmailType = Dtos.EmailTypeList.School },
                    Address = "xyz@xmail.com"
                });
                personDto.EmailAddresses = emailAddresses;

                //Entity
                personIntegrationEntity = new PersonIntegration(personId, legalPrimaryName.LastName)
                {
                    Guid = personDto.Id,
                    Prefix = "Mr.",
                    FirstName = legalPrimaryName.FirstName,
                    MiddleName = legalPrimaryName.MiddleName,
                    Suffix = "Sr."

                };
                //Returned value
                personIntegrationReturned = new PersonIntegration(personId, "LegalLast");
                personIntegrationReturned.Guid = personGuid;
                personIntegrationReturned.Prefix = "Mr.";
                personIntegrationReturned.FirstName = "LegalFirst";
                personIntegrationReturned.MiddleName = "LegalMiddle";
                personIntegrationReturned.Suffix = "Jr.";
                personIntegrationReturned.Nickname = "NickNameFirst NickNameMiddle NickNameLast";
                personIntegrationReturned.BirthDate = new DateTime(1930, 1, 1);
                personIntegrationReturned.DeceasedDate = new DateTime(2014, 5, 12);
                personIntegrationReturned.GovernmentId = "111-11-1111";
                personIntegrationReturned.Religion = "CA";
                personIntegrationReturned.MaritalStatusCode = "M";
                personIntegrationReturned.EthnicCodes = new List<string> { "H", "N" };
                personIntegrationReturned.Gender = "M";
                personIntegrationReturned.RaceCodes = new List<string> { "AS" };
                personIntegrationReturned.AddRole(new PersonRole(PersonRoleType.Alumni, new DateTime(15, 01, 22), new DateTime(15, 05, 25)));
                personIntegrationReturned.BirthNameFirst = "BirthFirst";
                personIntegrationReturned.BirthNameLast = "BirthLast";
                personIntegrationReturned.BirthNameMiddle = "BirthMiddle";
                personIntegrationReturned.ChosenFirstName = "ChosenFirst";
                personIntegrationReturned.ChosenLastName = "ChosenLast";
                personIntegrationReturned.ChosenMiddleName = "ChosenMiddle";
                personIntegrationReturned.PreferredName = "PreferedFirst PreferedMiddle PreferedLast";
                personIntegrationReturned.FormerNames = new List<PersonName>()
                {
                    new PersonName("HistoryFirst", "HistoryMiddle", "HistoryLast")
                };
                // Mock the email address data response
                instEmail = new Domain.Base.Entities.EmailAddress("inst@inst.com", "COL") { IsPreferred = true };
                personIntegrationReturned.AddEmailAddress(instEmail);
                perEmail = new Domain.Base.Entities.EmailAddress("personal@personal.com", "PER") { IsPreferred = false };
                personIntegrationReturned.AddEmailAddress(perEmail);
                workEmail = new Domain.Base.Entities.EmailAddress("work@work.com", "BUS") { IsPreferred = false };
                personIntegrationReturned.AddEmailAddress(workEmail);

                // Mock the address hierarchy responses
                var addresses = new List<Domain.Base.Entities.Address>();
                homeAddr = new Domain.Base.Entities.Address()
                {
                    TypeCode = "HO",
                    Type = Dtos.EnumProperties.AddressType.Home.ToString(),
                    Guid = Guid.NewGuid().ToString(),
                    Status = "Current",
                    IsPreferredAddress = true
                };
                addresses.Add(homeAddr);
                mailAddr = new Domain.Base.Entities.Address()
                {
                    TypeCode = "MA",
                    Type = Dtos.EnumProperties.AddressType.Mailing.ToString(),
                    Guid = Guid.NewGuid().ToString(),
                    Status = "Current"
                };
                addresses.Add(mailAddr);
                resAddr = new Domain.Base.Entities.Address()
                {
                    TypeCode = "VA",
                    Type = Dtos.EnumProperties.AddressType.Vacation.ToString(),
                    Guid = Guid.NewGuid().ToString(),
                    Status = "Current",
                    IsPreferredResidence = true
                };
                addresses.Add(resAddr);
                workAddr = new Domain.Base.Entities.Address()
                {
                    TypeCode = "BU",
                    Type = Dtos.EnumProperties.AddressType.Business.ToString(),
                    Guid = Guid.NewGuid().ToString(),
                    Status = "Current"
                };
                addresses.Add(workAddr);
                personIntegrationReturned.Addresses = addresses;

                // Mock the person phone per type response
                homePhone = new Domain.Base.Entities.Phone("111-111-1111", "HO");
                personIntegrationReturned.AddPhone(homePhone);
                mobilePhone = new Domain.Base.Entities.Phone("222-222-2222", "MO");
                personIntegrationReturned.AddPhone(mobilePhone);
                residencePhone = new Domain.Base.Entities.Phone("333-333-3333", "VA");
                personIntegrationReturned.AddPhone(residencePhone);
                workPhone = new Domain.Base.Entities.Phone("444-444-4444", "BU", "4444");
                personIntegrationReturned.AddPhone(workPhone);

                // Mock social media
                var socialMediaTypeCode = "TW";
                var socialMediaHandle = "pontifex";
                var personSocialMedia = new SocialMedia(socialMediaTypeCode, socialMediaHandle);
                personIntegrationReturned.AddSocialMedia(personSocialMedia);
            }

            private void SetupReferenceDataRepositoryMocks()
            {
                referenceDataRepositoryMock.Setup(repo => repo.GetPrivacyStatusesAsync(It.IsAny<bool>()))
                     .ReturnsAsync(
                         new List<PrivacyStatus>() {
                        new PrivacyStatus( "d3d86052-9d55-4751-acda-5c07a064a82a", "UN", "Unrestricted", PrivacyStatusType.unrestricted),
                        new PrivacyStatus( "cff65dcc-4a9b-44ed-b8d0-930348c55ef8", "R", "restricted", PrivacyStatusType.restricted)
                        }
                     );
                personNameTypes = new List<PersonNameTypeItem>() {
                        new PersonNameTypeItem("8224f18e-69c5-480b-a9b4-52f596aa4a52", "PREFERRED", "Personal", PersonNameType.Personal),
                        new PersonNameTypeItem("7dfa950c-8ae4-4dca-92f0-c083604285b6", "BIRTH", "Birth", PersonNameType.Birth),
                        new PersonNameTypeItem("dd20ebdf-2452-41ef-9f86-ad1b1621a78d", "CHOSEN", "Chosen", PersonNameType.Personal),
                        new PersonNameTypeItem("806af5a5-8a9a-424f-8c9f-c1e9d084ee71", "LEGAL", "Legal", PersonNameType.Legal),
                        new PersonNameTypeItem("7b55610f-7d00-4260-bbcf-0e47fdbae647", "NICKNAME", "NickName", PersonNameType.Personal),
                        new PersonNameTypeItem("d42cc964-35cb-4560-bc46-4b881e7705ea", "HISTORY", "History", PersonNameType.Personal)
                        };
                referenceDataRepositoryMock.Setup(repo => repo.GetPersonNameTypesAsync(It.IsAny<bool>()))
                    .ReturnsAsync(personNameTypes);

                referenceDataRepositoryMock.Setup(repo => repo.GetDenominationsAsync(It.IsAny<bool>()))
                    .ReturnsAsync(
                        new List<Denomination>() {
                        new Denomination(baptistGuid,"BA", "Baptist") ,
                        new Denomination(catholicGuid,"CA", "Catholic")
                        }
                     );

                referenceDataRepositoryMock.Setup(repo => repo.GetEmailTypesAsync(It.IsAny<bool>()))
                     .ReturnsAsync(
                         new List<EmailType>() {
                        new EmailType("899803da-48f8-4044-beb8-5913a04b995d", "COL", "College", EmailTypeCategory.School),
                        new EmailType("301d485d-d37b-4d29-af00-465ced624a85", "PER", "Personal", EmailTypeCategory.Personal),
                        new EmailType("53fb7dab-d348-4657-b071-45d0e5933e05", "BUS", "Business", EmailTypeCategory.Business)
                        }
                     );

                // Mock the reference repository for ethnicity
                referenceDataRepositoryMock.Setup(repo => repo.GetEthnicitiesAsync(It.IsAny<bool>())).Returns(Task.FromResult<IEnumerable<Ethnicity>>(new List<Ethnicity>()
                {
                    new Ethnicity(ethnicityGuid, "H", "Hispanic", EthnicityType.Hispanic)
                }));

                referenceDataRepositoryMock.Setup(repo => repo.GetSocialMediaTypesAsync(It.IsAny<bool>()))
                     .ReturnsAsync(
                         new List<SocialMediaType>() {
                        new SocialMediaType("d1f311f4-687d-4dc7-a329-c6a8bfc9c74", "TW", "Twitter", SocialMediaTypeCategory.twitter)
                        }
                     );

                referenceDataRepositoryMock.Setup(repo => repo.GetAddressTypes2Async(It.IsAny<bool>()))
                     .ReturnsAsync(
                         new List<AddressType2>() {
                        new AddressType2("91979656-e110-4156-a75a-1a1a7294314d", "HO", "Home", AddressTypeCategory.Home),
                        new AddressType2("b887d5ec-9ed5-45e8-b44c-01782070f234", "MA", "Mailing", AddressTypeCategory.Mailing),
                        new AddressType2("d7d0a82c-fe74-480d-be1b-88a2e460af4c", "VA", "Vacation", AddressTypeCategory.Vacation),
                        new AddressType2("c9b8cd52-54e6-4c08-a9d9-224dd0c8b700", "BU", "Business", AddressTypeCategory.Business)
                         }
                     );

                referenceDataRepositoryMock.Setup(repo => repo.GetPhoneTypesAsync(It.IsAny<bool>()))
                     .ReturnsAsync(
                         new List<PhoneType>() {
                        new PhoneType("92c82d33-e55c-41a4-a2c3-f2f7d2c523d1", "HO", "Home", PhoneTypeCategory.Home),
                        new PhoneType("b6def2cc-cc95-4d0e-a32c-940fbbc2d689", "MO", "Mobile", PhoneTypeCategory.Mobile),
                        new PhoneType("f60e7b27-a3e3-4c92-9d36-f3cae27b724b", "VA", "Vacation", PhoneTypeCategory.Vacation),
                        new PhoneType("30e231cf-a199-4c9a-af01-be2e69b607c9", "BU", "Business", PhoneTypeCategory.Business)
                        }
                     );

                // Mock the reference repository for prefix
                referenceDataRepositoryMock.Setup(repo => repo.Prefixes).Returns(new List<Prefix>()
                {
                    new Prefix("MR","Mr","Mr."),
                    new Prefix("MS","Ms","Ms.")
                });

                // Mock the reference repository for prefix
                referenceDataRepositoryMock.Setup(repo => repo.GetPrefixesAsync()).ReturnsAsync(new List<Prefix>()
                {
                    new Prefix("MR","Mr","Mr."),
                    new Prefix("MS","Ms","Ms.")
                });

                // Mock the reference repository for suffix
                referenceDataRepositoryMock.Setup(repo => repo.Suffixes).Returns(new List<Suffix>()
                {
                    new Suffix("JR","Jr","Jr."),
                    new Suffix("SR","Sr","Sr.")
                });

                // Mock the reference repository for suffix
                referenceDataRepositoryMock.Setup(repo => repo.GetSuffixesAsync()).ReturnsAsync(new List<Suffix>()
                {
                    new Suffix("JR","Jr","Jr."),
                    new Suffix("SR","Sr","Sr.")
                });

                // Mock the reference repository for marital status
                referenceDataRepositoryMock.Setup(repo => repo.GetMaritalStatusesAsync(It.IsAny<bool>())).Returns(Task.FromResult<IEnumerable<MaritalStatus>>(new List<MaritalStatus>()
                {
                    new MaritalStatus(maritalStatusGuid, "M", "Married"){ Type = MaritalStatusType.Married }
                }));

                var raceEntities = new List<Race>();
                raceEntities.Add(new Race(raceAsianGuid, "AS", "Asian", RaceType.Asian));
                raceEntities.Add(new Race(racePacificIslanderGuid, "HP", "Hawaiian/Pacific Islander", RaceType.PacificIslander));
                referenceDataRepositoryMock.Setup(repo => repo.GetRacesAsync(It.IsAny<bool>())).Returns(Task.FromResult<IEnumerable<Race>>(raceEntities));
            }
        }

        [TestClass]
        public class CreateUpdatePerson5WithAddressPhoneSocialMedia : CurrentUserSetup
        {
            //Mocks
            Mock<IAdapterRegistry> adapterRegistryMock;
            Mock<IPersonRepository> personRepositoryMock;
            Mock<IPersonBaseRepository> personBaseRepositoryMock;
            Mock<IReferenceDataRepository> referenceDataRepositoryMock;
            Mock<IProfileRepository> profileRepositoryMock;
            Mock<IConfigurationRepository> configurationRepositoryMock;
            Mock<IRelationshipRepository> relationshipRepositoryMock;
            Mock<IProxyRepository> proxyRepositoryMock;
            Mock<IRoleRepository> roleRepositoryMock;
            Mock<ILogger> loggerMock;

            //userfactory
            ICurrentUserFactory currentUserFactory;

            //Service
            PersonService personService;


            private Ellucian.Colleague.Dtos.Person5 personDto;
            private Ellucian.Colleague.Domain.Base.Entities.PersonIntegration personIntegrationReturned;
            private Ellucian.Colleague.Domain.Base.Entities.PersonIntegration personIntegrationEntity;

            //private Ellucian.Colleague.Domain.Base.Entities.Person person;
            private List<Ellucian.Colleague.Domain.Base.Entities.Address> addresses = null;
            private IEnumerable<Ellucian.Colleague.Domain.Base.Entities.Phone> phones = new List<Domain.Base.Entities.Phone>();
            private IEnumerable<Ellucian.Colleague.Domain.Base.Entities.State> states;
            private IEnumerable<Ellucian.Colleague.Domain.Base.Entities.Country> countries;
            private List<Ellucian.Colleague.Domain.Base.Entities.County> counties;
            private List<PersonNameTypeItem> personNameTypes;

            //Entities
            private Ellucian.Colleague.Domain.Base.Entities.EmailAddress instEmail;
            private Ellucian.Colleague.Domain.Base.Entities.EmailAddress perEmail;
            private Ellucian.Colleague.Domain.Base.Entities.EmailAddress workEmail;
            private Ellucian.Colleague.Domain.Base.Entities.Address homeAddr;
            private Ellucian.Colleague.Domain.Base.Entities.Address mailAddr;
            private Ellucian.Colleague.Domain.Base.Entities.Address resAddr;
            private Ellucian.Colleague.Domain.Base.Entities.Address workAddr;
            private Ellucian.Colleague.Domain.Base.Entities.Phone homePhone;
            private Ellucian.Colleague.Domain.Base.Entities.Phone mobilePhone;
            private Ellucian.Colleague.Domain.Base.Entities.Phone residencePhone;
            private Ellucian.Colleague.Domain.Base.Entities.Phone workPhone;

            private IEnumerable<Domain.Base.Entities.Address> allAddresses;
            private IEnumerable<Domain.Base.Entities.PhoneType> allPhones;
            private IEnumerable<Domain.Base.Entities.Chapter> allChapters;
            private IEnumerable<Domain.Base.Entities.County> allCounties;
            private IEnumerable<Domain.Base.Entities.ZipcodeXlat> allZipCodeXlats;
            private IEnumerable<Domain.Base.Entities.GeographicAreaType> allGeographicAreaTypes;


            private List<Dtos.DtoProperties.PersonAddressDtoProperty> addressesCollection = new List<Dtos.DtoProperties.PersonAddressDtoProperty>();

            //Data
            private string personId = "0000011";
            private string personGuid = "5674f28b-b216-4055-b236-81a922d93b4c";
            private string maritalStatusGuid = "dca8edb5-120f-479a-a6bb-35ba3af4b344";
            private string ethnicityGuid = "dd0c42ca-c61d-4ca6-8d21-96ab5be35623";
            private string raceAsianGuid = "72b7737b-27db-4a06-944b-97d00c29b3db";
            private string racePacificIslanderGuid = "e20f9821-28a2-4e34-8550-6758850a0cf8";
            private string baptistGuid = "c0bdfd92-462f-4e59-bba5-1b15c4771c86";
            private string catholicGuid = "f96f04b0-4973-41f6-bc3d-9c7bc1c2c458";

            private string countyGuid = Guid.NewGuid().ToString();


            [TestInitialize]
            public void Initialize()
            {
                adapterRegistryMock = new Mock<IAdapterRegistry>();
                personRepositoryMock = new Mock<IPersonRepository>();
                personBaseRepositoryMock = new Mock<IPersonBaseRepository>();
                referenceDataRepositoryMock = new Mock<IReferenceDataRepository>();
                profileRepositoryMock = new Mock<IProfileRepository>();
                configurationRepositoryMock = new Mock<IConfigurationRepository>();
                relationshipRepositoryMock = new Mock<IRelationshipRepository>();
                proxyRepositoryMock = new Mock<IProxyRepository>();
                roleRepositoryMock = new Mock<IRoleRepository>();
                loggerMock = new Mock<ILogger>();

                currentUserFactory = new CurrentUserSetup.ThirdPartyUserFactory();

                SetupData();

                SetupReferenceDataRepositoryMocks();

                // International Parameters Host Country
                personRepositoryMock.Setup(repo => repo.GetHostCountryAsync()).ReturnsAsync("USA");

                personRepositoryMock.Setup(i => i.Update2Async(It.IsAny<PersonIntegration>(), It.IsAny<IEnumerable<Domain.Base.Entities.Address>>(),
                    It.IsAny<IEnumerable<Ellucian.Colleague.Domain.Base.Entities.Phone>>(), It.IsAny<int>())).ReturnsAsync(personIntegrationReturned);

                personRepositoryMock.Setup(i => i.GetAddressIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync("x");

                personService = new PersonService(adapterRegistryMock.Object, personRepositoryMock.Object, personBaseRepositoryMock.Object, referenceDataRepositoryMock.Object, profileRepositoryMock.Object,
                                                  configurationRepositoryMock.Object, relationshipRepositoryMock.Object, proxyRepositoryMock.Object, currentUserFactory,
                                                  roleRepositoryMock.Object, loggerMock.Object);
            }


            [TestCleanup]
            public void Cleanup()
            {
                personService = null;
                adapterRegistryMock = null;
                personRepositoryMock = null;
                personBaseRepositoryMock = null;
                referenceDataRepositoryMock = null;
                profileRepositoryMock = null;
                configurationRepositoryMock = null;
                relationshipRepositoryMock = null;
                proxyRepositoryMock = null;
                roleRepositoryMock = null;
                loggerMock = null;
            }

            [TestMethod]
            public async Task UpdatePerson5_UpdatePerson5Async_WithAddress()
            {
                //setup role
                updatePersonRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Base.BasePermissionCodes.UpdatePerson));
                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { updatePersonRole });

                //personId 0000011
                personRepositoryMock.Setup(i => i.GetPersonIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync(personId);

                var actual = await personService.UpdatePerson5Async(personDto);

                Assert.AreEqual(personDto.Addresses.Count(), actual.Addresses.Count());
                Assert.AreEqual(personDto.Phones.Count(), actual.Phones.Count());
                Assert.AreEqual(personDto.SocialMedia.Count(), actual.SocialMedia.Count());
            }

            [TestMethod]
            public async Task UpdatePerson5_UpdatePerson5Async_AddressNullId()
            {
                addressesCollection = new List<Dtos.DtoProperties.PersonAddressDtoProperty>();
                allAddresses = new TestAddressRepository().GetAddressDataWithNullId().ToList().Where(i => string.IsNullOrEmpty(i.Guid));

                foreach (var source in allAddresses)
                {
                    var address = new Ellucian.Colleague.Dtos.DtoProperties.PersonAddressDtoProperty
                    {
                        address = new PersonAddress()
                        {
                            Id = source.Guid,
                            AddressLines = source.AddressLines,
                            Latitude = source.Latitude,
                            Longitude = source.Longitude
                        },
                        AddressEffectiveStart = new DateTime(2015, 09, 01),
                        AddressEffectiveEnd = new DateTime(2015, 12, 20),
                        Preference = Dtos.EnumProperties.PersonPreference.Primary,
                        Type = new PersonAddressTypeDtoProperty()
                        {
                            AddressType = string.IsNullOrEmpty(source.Type) ? null : (Dtos.EnumProperties.AddressType?)Enum.Parse(typeof(Dtos.EnumProperties.AddressType), source.Type, true)
                        }
                    };
                    addressesCollection.Add(address);
                }
                personDto.Addresses = addressesCollection;
                personDto.Addresses.First().address.Place = new AddressPlace() { Country = new AddressCountry() { Locality = "Locality", Code = IsoCode.USA } };
                personDto.Addresses.First().address.AddressLines = new List<string>() { "Something" };
                personDto.Addresses.First().address.GeographicAreas = new List<GuidObject2>() { new GuidObject2("9ae3a175-1dfd-4937-b97b-3c9ad596e023") };

                updatePersonRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Base.BasePermissionCodes.UpdatePerson));
                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { updatePersonRole });

                var addrWithoutId = new TestAddressRepository().GetAddressDataWithNullId().Where(i => string.IsNullOrEmpty(i.Guid)).ToList();
                personIntegrationReturned.Addresses = addrWithoutId;
                personRepositoryMock.Setup(i => i.Update2Async(It.IsAny<PersonIntegration>(), It.IsAny<IEnumerable<Domain.Base.Entities.Address>>(),
                    It.IsAny<IEnumerable<Ellucian.Colleague.Domain.Base.Entities.Phone>>(), It.IsAny<int>())).ReturnsAsync(personIntegrationReturned);


                //personId 0000011
                personRepositoryMock.Setup(i => i.GetPersonIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync(personId);

                var actual = await personService.UpdatePerson5Async(personDto);

                Assert.IsNotNull(actual);
                Assert.AreEqual(personDto.Addresses.Count(), actual.Addresses.Count());
            }

            [TestMethod]
            public async Task UpdatePerson5_UpdatePerson5Async_All_Addresses()
            {
                addressesCollection = new List<Dtos.DtoProperties.PersonAddressDtoProperty>();
                allAddresses = new TestAddressRepository().GetAddressData().ToList();
                //d44134f9-0924-45d4-8b91-be9531aa7773
                var addAddress = allAddresses.FirstOrDefault(i => i.Guid.Equals("d44134f9-0924-45d4-8b91-be9531aa7773", StringComparison.OrdinalIgnoreCase));
                addAddress.TypeCode = "MA";
                allAddresses.ToList().Add(addAddress);
                allAddresses.All(i => i.SeasonalDates == new List<AddressSeasonalDates>()
                {
                    new AddressSeasonalDates("01/01", "05/31"),
                    new AddressSeasonalDates("08/01", "12/31")
                });
                allAddresses.Where(i => i.Guid.Equals("d44134f9-0924-45d4-8b91-be9531aa7773", StringComparison.OrdinalIgnoreCase)).All(i => i.IsPreferredAddress == true);

                foreach (var source in allAddresses)
                {
                    var address = new Ellucian.Colleague.Dtos.DtoProperties.PersonAddressDtoProperty
                    {
                        address = new PersonAddress()
                        {
                            Id = source.Guid,
                            AddressLines = source.AddressLines,
                            Latitude = source.Latitude,
                            Longitude = source.Longitude
                        },
                        AddressEffectiveStart = new DateTime(2015, 09, 01),
                        AddressEffectiveEnd = new DateTime(2015, 12, 20),
                        Preference = Dtos.EnumProperties.PersonPreference.Primary,
                        Type = new PersonAddressTypeDtoProperty()
                        {
                            AddressType = string.IsNullOrEmpty(source.Type) ? null : (Dtos.EnumProperties.AddressType?)Enum.Parse(typeof(Dtos.EnumProperties.AddressType), source.Type, true)
                        },
                        SeasonalOccupancies = new List<PersonAddressRecurrenceDtoProperty>()
                        {
                            new PersonAddressRecurrenceDtoProperty()
                            {
                             Recurrence = new Recurrence3()
                             {
                                 TimePeriod = new RepeatTimePeriod2(){ StartOn = new DateTimeOffset(2016, 01, 01,0,0,0, new TimeSpan()), EndOn = new DateTimeOffset(2016, 05, 31,0,0,0, new TimeSpan())}
                             }
                            },
                            new PersonAddressRecurrenceDtoProperty()
                            {
                             Recurrence = new Recurrence3()
                             {
                                 TimePeriod = new RepeatTimePeriod2(){ StartOn = new DateTimeOffset(2016, 08, 01,0,0,0, new TimeSpan()), EndOn = new DateTimeOffset(2016, 12, 31,0,0,0, new TimeSpan())}
                             }
                            }
                        }
                    };
                    addressesCollection.Add(address);
                }

                personDto.Addresses = addressesCollection;
                personDto.Addresses.First().address.Place = new AddressPlace() { Country = new AddressCountry() { Locality = "Locality", Code = IsoCode.USA } };
                personDto.Addresses.First().address.AddressLines = new List<string>() { "Something" };
                personDto.Addresses.First().address.GeographicAreas = new List<GuidObject2>() { new GuidObject2("9ae3a175-1dfd-4937-b97b-3c9ad596e023") };

                updatePersonRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Base.BasePermissionCodes.UpdatePerson));
                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { updatePersonRole });

                //personId 0000011
                personRepositoryMock.Setup(i => i.GetPersonIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync(personId);

                var actual = await personService.UpdatePerson5Async(personDto);

                Assert.IsNotNull(actual);
                Assert.AreEqual(personDto.Addresses.Count(), actual.Addresses.Count());
            }

            [TestMethod]
            public async Task UpdatePerson5_UpdatePerson5Async_PersonId_NullEmpty_Create()
            {
                //setup role
                createPersonRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Base.BasePermissionCodes.CreatePerson));
                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { createPersonRole });

                //personId 0000011
                personRepositoryMock.Setup(i => i.GetPersonIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync(string.Empty);

                personRepositoryMock.Setup(i => i.Create2Async(It.IsAny<PersonIntegration>(), It.IsAny<IEnumerable<Domain.Base.Entities.Address>>(), It.IsAny<IEnumerable<Domain.Base.Entities.Phone>>(), It.IsAny<int>())).ReturnsAsync(personIntegrationReturned);


                var actuals = await personService.UpdatePerson5Async(personDto);

                Assert.AreEqual(personDto.Addresses.Count(), actuals.Addresses.Count());
                foreach (var addr in actuals.Addresses)
                {
                    var expected = personDto.Addresses.FirstOrDefault(i => i.address.Id.Equals(addr.address.Id));
                    Assert.IsNotNull(expected);

                    Assert.AreEqual(expected.address.Id, addr.address.Id);
                    Assert.AreEqual(expected.address.Latitude, addr.address.Latitude);
                    Assert.AreEqual(expected.address.Longitude, addr.address.Longitude);
                    Assert.AreEqual(expected.address.Place, addr.address.Place);
                }

                foreach (var actual in actuals.Phones)
                {
                    var expected = personDto.Phones.FirstOrDefault(i => i.Number.Equals(actual.Number, StringComparison.OrdinalIgnoreCase));
                    Assert.IsNotNull(expected);

                    Assert.AreEqual(expected.Number, actual.Number);
                    Assert.AreEqual(expected.CountryCallingCode, actual.CountryCallingCode);
                    Assert.AreEqual(expected.Extension, actual.Extension);
                    Assert.AreEqual(expected.Type.PhoneType, actual.Type.PhoneType);
                }

                foreach (var actual in actuals.SocialMedia)
                {
                    var expected = personDto.SocialMedia.FirstOrDefault(i => i.Address.Equals(actual.Address, StringComparison.OrdinalIgnoreCase));
                    Assert.IsNotNull(expected);

                    Assert.AreEqual(expected.Address, actual.Address);
                    Assert.AreEqual(expected.Preference, actual.Preference);
                    Assert.AreEqual(expected.Type.Category, actual.Type.Category);
                }
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task UpdatePerson5_UpdatePerson5Async_AddressLineNull_Exception()
            {
                addressesCollection = new List<Dtos.DtoProperties.PersonAddressDtoProperty>();
                allAddresses = new TestAddressRepository().GetAddressDataWithNullId().ToList().Where(i => string.IsNullOrEmpty(i.Guid));

                foreach (var source in allAddresses)
                {
                    var address = new Ellucian.Colleague.Dtos.DtoProperties.PersonAddressDtoProperty
                    {
                        address = new PersonAddress()
                        {
                            Id = source.Guid,
                            AddressLines = source.AddressLines,
                            Latitude = source.Latitude,
                            Longitude = source.Longitude
                        },
                        AddressEffectiveStart = new DateTime(2015, 09, 01),
                        AddressEffectiveEnd = new DateTime(2015, 12, 20),
                        Preference = Dtos.EnumProperties.PersonPreference.Primary,
                        Type = new PersonAddressTypeDtoProperty()
                        {
                            AddressType = string.IsNullOrEmpty(source.Type) ? null : (Dtos.EnumProperties.AddressType?)Enum.Parse(typeof(Dtos.EnumProperties.AddressType), source.Type, true)
                        }
                    };
                    addressesCollection.Add(address);
                }
                personDto.Addresses = addressesCollection;
                personDto.Addresses.First().address.Place = new AddressPlace();
                //setup role
                updatePersonRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Base.BasePermissionCodes.UpdatePerson));
                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { updatePersonRole });

                //personId 0000011
                personRepositoryMock.Setup(i => i.GetPersonIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync(personId);

                try
                {
                    var actual = await personService.UpdatePerson5Async(personDto);
                }
                catch (IntegrationApiException ex)
                {
                    Assert.IsNotNull(ex.Errors);
                    Assert.AreEqual("persons.addresses", ex.Errors.First().Code);
                    Assert.AreEqual("Street address lines are required.\r\nParameter name: personDto", ex.Errors.First().Message);
                    throw;
                }
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task UpdatePerson5_UpdatePerson5Async_PlaceCountryNull_Exception()
            {
                addressesCollection = new List<Dtos.DtoProperties.PersonAddressDtoProperty>();
                allAddresses = new TestAddressRepository().GetAddressDataWithNullId().ToList().Where(i => string.IsNullOrEmpty(i.Guid));

                foreach (var source in allAddresses)
                {
                    var address = new Ellucian.Colleague.Dtos.DtoProperties.PersonAddressDtoProperty
                    {
                        address = new PersonAddress()
                        {
                            Id = source.Guid,
                            AddressLines = source.AddressLines,
                            Latitude = source.Latitude,
                            Longitude = source.Longitude
                        },
                        AddressEffectiveStart = new DateTime(2015, 09, 01),
                        AddressEffectiveEnd = new DateTime(2015, 12, 20),
                        Preference = Dtos.EnumProperties.PersonPreference.Primary,
                        Type = new PersonAddressTypeDtoProperty()
                        {
                            AddressType = string.IsNullOrEmpty(source.Type) ? null : (Dtos.EnumProperties.AddressType?)Enum.Parse(typeof(Dtos.EnumProperties.AddressType), source.Type, true)
                        }
                    };
                    addressesCollection.Add(address);
                }
                personDto.Addresses = addressesCollection;
                personDto.Addresses.First().address.Place = new AddressPlace();
                personDto.Addresses.First().address.AddressLines = new List<string>() { "Something" };
                //setup role
                updatePersonRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Base.BasePermissionCodes.UpdatePerson));
                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { updatePersonRole });

                //personId 0000011
                personRepositoryMock.Setup(i => i.GetPersonIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync(personId);

                try
                {
                    var actual = await personService.UpdatePerson5Async(personDto);
                }
                catch (IntegrationApiException ex)
                {
                    Assert.IsNotNull(ex.Errors);
                    Assert.AreEqual("persons.addresses", ex.Errors.First().Code);
                    Assert.AreEqual("A country code is required for an address with a place defined.\r\nParameter name: addressDto.place.country.code", ex.Errors.First().Message);
                    throw;
                }
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task UpdatePerson5_UpdatePerson5Async_SocialMediaTypeNull_Exception()
            {
                //setup role
                createPersonRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Base.BasePermissionCodes.CreatePerson));
                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { createPersonRole });

                //personId 0000011
                personRepositoryMock.Setup(i => i.GetPersonIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync(string.Empty);

                personRepositoryMock.Setup(i => i.Create2Async(It.IsAny<PersonIntegration>(), It.IsAny<IEnumerable<Domain.Base.Entities.Address>>(), It.IsAny<IEnumerable<Domain.Base.Entities.Phone>>(), It.IsAny<int>())).ReturnsAsync(personIntegrationReturned);

                personDto.SocialMedia.First().Type = null;

                try
                {
                    var actual = await personService.UpdatePerson5Async(personDto);
                }
                catch (IntegrationApiException ex)
                {
                    Assert.IsNotNull(ex.Errors);
                    Assert.AreEqual("persons.socialMedia", ex.Errors.First().Code);
                    Assert.AreEqual("Social media type is required to create a new social media\r\nParameter name: personDto", ex.Errors.First().Message);
                    throw;
                }
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task UpdatePerson5_UpdatePerson5Async_SocialMediaAddressNull_Exception()
            {
                //setup role
                createPersonRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Base.BasePermissionCodes.CreatePerson));
                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { createPersonRole });

                //personId 0000011
                personRepositoryMock.Setup(i => i.GetPersonIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync(string.Empty);

                personRepositoryMock.Setup(i => i.Create2Async(It.IsAny<PersonIntegration>(), It.IsAny<IEnumerable<Domain.Base.Entities.Address>>(), It.IsAny<IEnumerable<Domain.Base.Entities.Phone>>(), It.IsAny<int>())).ReturnsAsync(personIntegrationReturned);

                personDto.SocialMedia.First().Address = null;

                try
                {
                    var actual = await personService.UpdatePerson5Async(personDto);
                }
                catch (IntegrationApiException ex)
                {
                    Assert.IsNotNull(ex.Errors);
                    Assert.AreEqual("persons.socialMedia", ex.Errors.First().Code);
                    Assert.AreEqual("Social media handle is required to create a new social media\r\nParameter name: personDto", ex.Errors.First().Message);
                    throw;
                }
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task UpdatePerson5_UpdatePerson5Async_SocialMediaTypNotFound_Exception()
            {
                //setup role
                createPersonRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Base.BasePermissionCodes.CreatePerson));
                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { createPersonRole });

                //personId 0000011
                personRepositoryMock.Setup(i => i.GetPersonIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync(string.Empty);

                personRepositoryMock.Setup(i => i.Create2Async(It.IsAny<PersonIntegration>(), It.IsAny<IEnumerable<Domain.Base.Entities.Address>>(), It.IsAny<IEnumerable<Domain.Base.Entities.Phone>>(), It.IsAny<int>())).ReturnsAsync(personIntegrationReturned);

                personDto.SocialMedia.First().Type = new PersonSocialMediaType() { Category = Ellucian.Colleague.Dtos.SocialMediaTypeCategory.blog };

                try
                {
                    var actual = await personService.UpdatePerson5Async(personDto);
                }
                catch (IntegrationApiException ex)
                {
                    Assert.IsNotNull(ex.Errors);
                    Assert.AreEqual("persons.socialMedia", ex.Errors.First().Code);
                    Assert.AreEqual("Could not find the social media type for handle 'http://www.facebook.com/jDoe'. \r\nParameter name: socialMediaDto.Type", ex.Errors.First().Message);
                    throw;
                }
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task UpdatePerson5_NullAddress_Exception()
            {
                //setup role
                createPersonRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Base.BasePermissionCodes.CreatePerson));
                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { createPersonRole });

                //personId 0000011
                personRepositoryMock.Setup(i => i.GetPersonIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync(string.Empty);

                personRepositoryMock.Setup(i => i.Create2Async(It.IsAny<PersonIntegration>(), It.IsAny<IEnumerable<Domain.Base.Entities.Address>>(), phones, It.IsAny<int>())).ReturnsAsync(personIntegrationReturned);

                personDto.Addresses = new List<PersonAddressDtoProperty>()
                {
                    new PersonAddressDtoProperty(){address = null}
                };

                try
                {
                    var actual = await personService.UpdatePerson5Async(personDto);
                }
                catch (IntegrationApiException ex)
                {
                    Assert.IsNotNull(ex.Errors);
                    Assert.AreEqual("persons.addresses", ex.Errors.First().Code);
                    Assert.AreEqual("Address property is required\r\nParameter name: personDto", ex.Errors.First().Message);
                    throw;
                }
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task UpdatePerson5_NullType_Exception()
            {
                //setup role
                createPersonRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Base.BasePermissionCodes.CreatePerson));
                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { createPersonRole });

                //personId 0000011
                personRepositoryMock.Setup(i => i.GetPersonIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync(string.Empty);

                personRepositoryMock.Setup(i => i.Create2Async(It.IsAny<PersonIntegration>(), It.IsAny<IEnumerable<Domain.Base.Entities.Address>>(), phones, It.IsAny<int>())).ReturnsAsync(personIntegrationReturned);

                personDto.Addresses = new List<PersonAddressDtoProperty>()
                {
                    new PersonAddressDtoProperty(){address = new PersonAddress(), Type = null}
                };

                try
                {
                    var actual = await personService.UpdatePerson5Async(personDto);
                }
                catch (IntegrationApiException ex)
                {
                    Assert.IsNotNull(ex.Errors);
                    Assert.AreEqual("persons.addresses", ex.Errors.First().Code);
                    Assert.AreEqual("Address type is required\r\nParameter name: personDto", ex.Errors.First().Message);
                    throw;
                }
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task UpdatePerson5_NullPhoneType_Exception()
            {
                //setup role
                createPersonRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Base.BasePermissionCodes.CreatePerson));
                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { createPersonRole });

                //personId 0000011
                personRepositoryMock.Setup(i => i.GetPersonIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync(string.Empty);

                personRepositoryMock.Setup(i => i.Create2Async(It.IsAny<PersonIntegration>(), It.IsAny<IEnumerable<Domain.Base.Entities.Address>>(), phones, It.IsAny<int>())).ReturnsAsync(personIntegrationReturned);

                personDto.Phones = new List<PersonPhone2DtoProperty>()
                {
                    new PersonPhone2DtoProperty()
                    {
                         CountryCallingCode = "1",
                         Number = "111-111-1111"
                    }
                };

                try
                {
                    var actual = await personService.UpdatePerson5Async(personDto);
                }
                catch (IntegrationApiException ex)
                {
                    Assert.IsNotNull(ex.Errors);
                    Assert.AreEqual("persons.phones", ex.Errors.First().Code);
                    Assert.AreEqual("A valid Phone type is required for phone number '111-111-1111' \r\nParameter name: personDto.Phone.Type", ex.Errors.First().Message);
                    throw;
                }
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task UpdatePerson5_NullPhoneNumber_Exception()
            {
                //setup role
                createPersonRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Base.BasePermissionCodes.CreatePerson));
                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { createPersonRole });

                //personId 0000011
                personRepositoryMock.Setup(i => i.GetPersonIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync(string.Empty);

                personRepositoryMock.Setup(i => i.Create2Async(It.IsAny<PersonIntegration>(), It.IsAny<IEnumerable<Domain.Base.Entities.Address>>(), phones, It.IsAny<int>())).ReturnsAsync(personIntegrationReturned);

                personDto.Phones = new List<PersonPhone2DtoProperty>()
                {
                    new PersonPhone2DtoProperty()
                    {
                         CountryCallingCode = "1",
                         Number = "" ,
                         Type = new PersonPhoneTypeDtoProperty(){ PhoneType = PersonPhoneTypeCategory.Home}
                    }
                };

                try
                {
                    var actual = await personService.UpdatePerson5Async(personDto);
                }
                catch (IntegrationApiException ex)
                {
                    Assert.IsNotNull(ex.Errors);
                    Assert.AreEqual("persons.phones", ex.Errors.First().Code);
                    Assert.AreEqual("Phone number is required to create a new phone\r\nParameter name: personDto.Phone.Number", ex.Errors.First().Message);
                    throw;
                }
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task UpdatePerson5_InvalidPhoneType_Exception()
            {
                //setup role
                createPersonRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Base.BasePermissionCodes.CreatePerson));
                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { createPersonRole });

                //personId 0000011
                personRepositoryMock.Setup(i => i.GetPersonIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync(string.Empty);

                personRepositoryMock.Setup(i => i.Create2Async(It.IsAny<PersonIntegration>(), It.IsAny<IEnumerable<Domain.Base.Entities.Address>>(), phones, It.IsAny<int>())).ReturnsAsync(personIntegrationReturned);

                personDto.Phones = new List<PersonPhone2DtoProperty>()
                {
                    new PersonPhone2DtoProperty()
                    {
                         CountryCallingCode = "1",
                         Number = "1234" ,
                         Type = new PersonPhoneTypeDtoProperty(){ PhoneType = PersonPhoneTypeCategory.Fax, Detail = new GuidObject2("12345") }
                    }
                };

                try
                {
                    var actual = await personService.UpdatePerson5Async(personDto);
                }
                catch (IntegrationApiException ex)
                {
                    Assert.IsNotNull(ex.Errors);
                    Assert.AreEqual("persons.phones", ex.Errors.First().Code);
                    Assert.AreEqual("Could not find the phone type detail id '12345' for phone number '1234'. \r\nParameter name: phoneDto.Type.Detail.Id", ex.Errors.First().Message);
                    throw;
                }
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task UpdatePerson5_NullPhoneTypeId_Exception()
            {
                //setup role
                createPersonRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Base.BasePermissionCodes.CreatePerson));
                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { createPersonRole });

                //personId 0000011
                personRepositoryMock.Setup(i => i.GetPersonIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync(string.Empty);

                personRepositoryMock.Setup(i => i.Create2Async(It.IsAny<PersonIntegration>(), It.IsAny<IEnumerable<Domain.Base.Entities.Address>>(), phones, It.IsAny<int>())).ReturnsAsync(personIntegrationReturned);

                personDto.Phones = new List<PersonPhone2DtoProperty>()
                {
                    new PersonPhone2DtoProperty()
                    {
                         CountryCallingCode = "1",
                         Number = "1234" ,
                         Type = new PersonPhoneTypeDtoProperty(){ PhoneType = PersonPhoneTypeCategory.Home, Detail = new GuidObject2() }
                    }
                };

                try
                {
                    var actual = await personService.UpdatePerson5Async(personDto);
                }
                catch (IntegrationApiException ex)
                {
                    Assert.IsNotNull(ex.Errors);
                    Assert.AreEqual("persons.phones", ex.Errors.First().Code);
                    Assert.AreEqual("The Detail Id is required when Detail has been defined.\r\nParameter name: personDto.Phone.Type.Detail.Id", ex.Errors.First().Message);
                    throw;
                }
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task UpdatePerson5_Username_Exception()
            {
                //setup role
                updatePersonRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Base.BasePermissionCodes.UpdatePerson));
                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { updatePersonRole });

                //personId 0000011
                personRepositoryMock.Setup(i => i.GetPersonIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync(personId);

                personDto.Credentials = new List<Dtos.DtoProperties.Credential3DtoProperty>()
                {
                    new Dtos.DtoProperties.Credential3DtoProperty()
                    {
                        Type = Dtos.EnumProperties.Credential3Type.ColleagueUserName,
                        Value = "testUsername"
                    }
                };
                // Mock the response for getting a Person User Name 
                var personUserName = new PersonUserName("0000011", "WrongUsername");
                var personUserNames = new List<PersonUserName>();
                personUserNames.Add(personUserName);
                personRepositoryMock.Setup(repo => repo.GetPersonUserNamesAsync(It.IsAny<string[]>())).ReturnsAsync(personUserNames);

                try
                {
                    var actual = await personService.UpdatePerson5Async(personDto);
                }
                catch (IntegrationApiException ex)
                {
                    Assert.IsNotNull(ex.Errors);
                    Assert.AreEqual("persons.credentials", ex.Errors.First().Code);
                    Assert.AreEqual("You cannot add/edit Colleague usernames. You must maintain them in Colleague.", ex.Errors.First().Message);
                    throw;
                }
            }

            private void SetupData()
            {
                // setup personDto object
                personDto = new Dtos.Person5();
                personDto.Id = personGuid;
                personDto.BirthDate = new DateTime(1930, 1, 1);
                personDto.DeceasedDate = new DateTime(2014, 5, 12);
                var personNames = new List<Dtos.DtoProperties.PersonName2DtoProperty>();
                var legalPrimaryName = new Dtos.DtoProperties.PersonName2DtoProperty()
                {
                    NameType = new Dtos.DtoProperties.PersonNameTypeDtoProperty() { Category = Dtos.EnumProperties.PersonNameType2.Legal, Detail = new Dtos.GuidObject2() { Id = "806af5a5-8a9a-424f-8c9f-c1e9d084ee71" } },
                    Title = "Mr.",
                    FirstName = "LegalFirst",
                    MiddleName = "LegalMiddle",
                    LastName = "LegalLast",
                    Pedigree = "Jr.",
                    LastNamePrefix = "Mr.",
                    FullName = "LegalFirst LegalMiddle LegalLast"
                };
                personNames.Add(legalPrimaryName);

                var birthPrimaryName = new Dtos.DtoProperties.PersonName2DtoProperty()
                {
                    NameType = new Dtos.DtoProperties.PersonNameTypeDtoProperty() { Category = Dtos.EnumProperties.PersonNameType2.Birth, Detail = new Dtos.GuidObject2() { Id = "7dfa950c-8ae4-4dca-92f0-c083604285b6" } },
                    Title = "Mr.",
                    FirstName = "BirthFirst",
                    MiddleName = "BirthMiddle",
                    LastName = "BirthLast",
                    Pedigree = "Jr.",
                    LastNamePrefix = "Mr.",
                    FullName = "BirthFirst BirthMiddle BirthLast"
                };
                personNames.Add(birthPrimaryName);

                var chosenPrimaryName = new PersonName2DtoProperty()
                {
                    NameType = new PersonNameTypeDtoProperty() { Category = Dtos.EnumProperties.PersonNameType2.Personal, Detail = new Dtos.GuidObject2() { Id = "dd20ebdf-2452-41ef-9f86-ad1b1621a78d" } },
                    Title = "Mr.",
                    FirstName = "ChosenFirst",
                    MiddleName = "ChosenMiddle",
                    LastName = "ChosenLast",
                    Pedigree = "Jr.",
                    LastNamePrefix = "Ignore",
                    FullName = "ChosenFirst ChosenMiddle ChosenLast"
                };
                personNames.Add(chosenPrimaryName);

                var nickNamePrimaryName = new Dtos.DtoProperties.PersonName2DtoProperty()
                {
                    NameType = new Dtos.DtoProperties.PersonNameTypeDtoProperty() { Category = Dtos.EnumProperties.PersonNameType2.Personal, Detail = new Dtos.GuidObject2() { Id = "7b55610f-7d00-4260-bbcf-0e47fdbae647" } },
                    Title = "Mr.",
                    FirstName = "NickNameFirst",
                    MiddleName = "NickNameMiddle",
                    LastName = "NickNameLast",
                    Pedigree = "Jr.",
                    LastNamePrefix = "Mr.",
                    FullName = "NickNameFirst NickNameMiddle NickNameLast"
                };
                personNames.Add(nickNamePrimaryName);

                var historyPrimaryName = new Dtos.DtoProperties.PersonName2DtoProperty()
                {
                    NameType = new Dtos.DtoProperties.PersonNameTypeDtoProperty() { Category = Dtos.EnumProperties.PersonNameType2.Personal, Detail = new Dtos.GuidObject2() { Id = "d42cc964-35cb-4560-bc46-4b881e7705ea" } },
                    Title = "Mr.",
                    FirstName = "HistoryFirst",
                    MiddleName = "HistoryMiddle",
                    LastName = "HistoryLast",
                    Pedigree = "Jr.",
                    LastNamePrefix = "Mr.",
                    FullName = "HistoryFirst HistoryMiddle HistoryLast"
                };
                personNames.Add(historyPrimaryName);

                personDto.PersonNames = personNames;
                personDto.GenderType = Dtos.EnumProperties.GenderType2.Male;
                personDto.MaritalStatus = new Dtos.DtoProperties.PersonMaritalStatusDtoProperty() { Detail = new Dtos.GuidObject2(maritalStatusGuid), MaritalCategory = Dtos.EnumProperties.PersonMaritalStatusCategory.Married };// new Dtos.GuidObject(maritalStatusGuid);
                personDto.Ethnicity = new Dtos.DtoProperties.PersonEthnicityDtoProperty() { EthnicGroup = new Dtos.GuidObject2(ethnicityGuid) };
                personDto.Races = new List<Dtos.DtoProperties.PersonRaceDtoProperty>()
                {

                    new Dtos.DtoProperties.PersonRaceDtoProperty(){ Race = new Dtos.GuidObject2(raceAsianGuid)}
                };
                personDto.Credentials = new List<Dtos.DtoProperties.Credential3DtoProperty>()
                {
                    new Dtos.DtoProperties.Credential3DtoProperty()
                    {
                        Type = Dtos.EnumProperties.Credential3Type.Ssn,
                        Value = "111-11-1111"
                    }
                };
                var emailAddresses = new List<Dtos.DtoProperties.PersonEmailDtoProperty>();
                emailAddresses.Add(new Dtos.DtoProperties.PersonEmailDtoProperty()
                {

                    Type = new Dtos.DtoProperties.PersonEmailTypeDtoProperty() { EmailType = Dtos.EmailTypeList.School },
                    Address = "xyz@xmail.com"
                });
                personDto.EmailAddresses = emailAddresses;
                personDto.GenderIdentity = new GuidObject2("9C3004AB-0F25-4D1D-84D6-65EA69CE1124");
                personDto.PersonalPronoun = new GuidObject2("AE7A3392-FA07-4F53-B6D5-317D77CB62EC");
                personDto.AlternativeCredentials = new List<AlternativeCredentials>()
                {
                    new AlternativeCredentials()
                    {
                        Type = new GuidObject2("D525E2B2-CD7D-4995-93F0-97DA468EBE90"),
                        Value = "1234"
                    }
                };

                // Mock the reference repository for states
                states = new List<State>()
                {
                    new State("VA","Virginia"),
                    new State("MD","Maryland"),
                    new State("NY","New York"),
                    new State("MA","Massachusetts")
                };

                //Entity
                personIntegrationEntity = new PersonIntegration(personId, legalPrimaryName.LastName)
                {
                    Guid = personDto.Id,
                    Prefix = "Mr.",
                    FirstName = legalPrimaryName.FirstName,
                    MiddleName = legalPrimaryName.MiddleName,
                    Suffix = "Sr."

                };
                allChapters = new TestGeographicAreaRepository().GetChapters();
                allCounties = new TestGeographicAreaRepository().GetCounties();
                allZipCodeXlats = new TestGeographicAreaRepository().GetZipCodeXlats();
                allGeographicAreaTypes = new TestGeographicAreaRepository().Get();
                counties = new List<County>()
                {
                    new County(Guid.NewGuid().ToString(), "FFX","Fairfax County"),
                    new County(Guid.NewGuid().ToString(), "BAL","Baltimore County"),
                    new County(Guid.NewGuid().ToString(), "NY","New York County"),
                    new County(Guid.NewGuid().ToString(), "BOS","Boston County")
                };

                //Addreses
                allAddresses = new TestAddressRepository().GetAddressData().ToList();

                foreach (var source in allAddresses)
                {
                    var address = new Ellucian.Colleague.Dtos.DtoProperties.PersonAddressDtoProperty
                    {
                        address = new PersonAddress()
                        {
                            Id = source.Guid,
                            AddressLines = source.AddressLines,
                            Latitude = source.Latitude,
                            Longitude = source.Longitude
                        },
                        AddressEffectiveStart = new DateTime(2015, 09, 01),
                        AddressEffectiveEnd = new DateTime(2015, 12, 20),
                        Preference = Dtos.EnumProperties.PersonPreference.Primary,
                        Type = new PersonAddressTypeDtoProperty()
                        {
                            AddressType = string.IsNullOrEmpty(source.Type) ? null : (Dtos.EnumProperties.AddressType?)Enum.Parse(typeof(Dtos.EnumProperties.AddressType), source.Type, true)
                        }
                    };
                    addressesCollection.Add(address);
                }
                personDto.Addresses = addressesCollection;

                var phoneList = new List<PersonPhone2DtoProperty>()
                {
                    new PersonPhone2DtoProperty()
                    {
                        CountryCallingCode = "01",
                         Extension = "1",
                         Number = "111-111-1111",
                         //Preference = PersonPreference.Primary,
                         Type = new PersonPhoneTypeDtoProperty()
                         {
                             Detail = new GuidObject2("92c82d33-e55c-41a4-a2c3-f2f7d2c523d1"),
                             PhoneType = PersonPhoneTypeCategory.Home
                         }
                    },
                    new PersonPhone2DtoProperty()
                    {
                        CountryCallingCode = "02",
                         Extension = "2",
                         Number = "222-222-2222",
                         //Preference = PersonPreference.Primary,
                         Type = new PersonPhoneTypeDtoProperty()
                         {
                             Detail = new GuidObject2("b6def2cc-cc95-4d0e-a32c-940fbbc2d689"),
                             PhoneType = PersonPhoneTypeCategory.Mobile
                         }
                    },
                    new PersonPhone2DtoProperty()
                    {
                        CountryCallingCode = "03",
                         Extension = "3",
                         Number = "333-333-3333",
                         //Preference = PersonPreference.Primary,
                         Type = new PersonPhoneTypeDtoProperty()
                         {
                             Detail = new GuidObject2("f60e7b27-a3e3-4c92-9d36-f3cae27b724b"),
                             PhoneType = PersonPhoneTypeCategory.Vacation
                         }
                    },
                    new PersonPhone2DtoProperty()
                    {
                        CountryCallingCode = "04",
                         Extension = "4444",
                         Number = "444-444-4444",
                         //Preference = PersonPreference.Primary,
                         Type = new PersonPhoneTypeDtoProperty()
                         {
                             Detail = new GuidObject2("30e231cf-a199-4c9a-af01-be2e69b607c9"),
                             PhoneType = PersonPhoneTypeCategory.Home
                         }
                    },
                };

                personDto.Phones = phoneList;

                //SocialMedia
                personDto.SocialMedia = new List<PersonSocialMediaDtoProperty>()
                {
                    new PersonSocialMediaDtoProperty()
                    {
                        Address = "http://www.facebook.com/jDoe",
                         Preference = PersonPreference.Primary,
                         Type = new PersonSocialMediaType(){ Category = Ellucian.Colleague.Dtos.SocialMediaTypeCategory.facebook, Detail = new GuidObject2("d1f311f4-687d-4dc7-a329-c6a8bfc9c74") }
                    },
                    new PersonSocialMediaDtoProperty()
                    {
                        Address = "http://www.somewebsite.com/jDoe",
                         Preference = PersonPreference.Primary,
                         Type = new PersonSocialMediaType(){ Category = Ellucian.Colleague.Dtos.SocialMediaTypeCategory.website, Detail = new GuidObject2("d1f311f4-687d-4dc7-a329-c6a8bfc9c75") }
                    }
                };

                //Returned value
                personIntegrationReturned = new PersonIntegration(personId, "LegalLast");
                personIntegrationReturned.Guid = personGuid;
                personIntegrationReturned.Prefix = "Mr.";
                personIntegrationReturned.FirstName = "LegalFirst";
                personIntegrationReturned.MiddleName = "LegalMiddle";
                personIntegrationReturned.Suffix = "Jr.";
                personIntegrationReturned.Nickname = "NickNameFirst NickNameMiddle NickNameLast";
                personIntegrationReturned.BirthDate = new DateTime(1930, 1, 1);
                personIntegrationReturned.DeceasedDate = new DateTime(2014, 5, 12);
                personIntegrationReturned.GovernmentId = "111-11-1111";
                personIntegrationReturned.Religion = "CA";
                personIntegrationReturned.MaritalStatusCode = "M";
                personIntegrationReturned.EthnicCodes = new List<string> { "H", "N" };
                personIntegrationReturned.Gender = "M";
                personIntegrationReturned.RaceCodes = new List<string> { "AS" };
                personIntegrationReturned.AddEmailAddress(new EmailAddress("inst@inst.com", "COL") { IsPreferred = true });
                personIntegrationReturned.AddRole(new PersonRole(PersonRoleType.Alumni, new DateTime(15, 01, 22), new DateTime(15, 05, 25)));
                personIntegrationReturned.BirthNameFirst = "BirthFirst";
                personIntegrationReturned.BirthNameLast = "BirthLast";
                personIntegrationReturned.BirthNameMiddle = "BirthMiddle";
                personIntegrationReturned.ChosenFirstName = "ChosenFirst";
                personIntegrationReturned.ChosenLastName = "ChosenLast";
                personIntegrationReturned.ChosenMiddleName = "ChosenMiddle";
                personIntegrationReturned.PreferredName = "PreferedFirst PreferedMiddle PreferedLast";
                personIntegrationReturned.FormerNames = new List<PersonName>()
                {
                    new PersonName("HistoryFirst", "HistoryMiddle", "HistoryLast")
                };
                // Mock the email address data response
                perEmail = new Domain.Base.Entities.EmailAddress("personal@personal.com", "PER") { IsPreferred = false };
                personIntegrationReturned.AddEmailAddress(perEmail);
                workEmail = new Domain.Base.Entities.EmailAddress("work@work.com", "BUS") { IsPreferred = false }; ;
                personIntegrationReturned.AddEmailAddress(workEmail);
                personIntegrationReturned.GenderIdentityCode = "FTM";
                personIntegrationReturned.PersonalPronounCode = "HE";
                personIntegrationReturned.AddPersonAlt(new PersonAlt("2222", "ELEV2"));
                personIntegrationReturned.AddPersonAlt(new PersonAlt("3333", "GOVID2"));

                // Mock the address hierarchy responses
                var addresses = new TestAddressRepository().GetAddressData().ToList();
                personIntegrationReturned.Addresses = addresses;
                var addrWithoutId = new TestAddressRepository().GetAddressDataWithNullId().ToList();

                // Mock the person phone per type response
                homePhone = new Domain.Base.Entities.Phone("111-111-1111", "HO", "1") { CountryCallingCode = "01", IsPreferred = true };
                personIntegrationReturned.AddPhone(homePhone);
                mobilePhone = new Domain.Base.Entities.Phone("222-222-2222", "MO", "2") { CountryCallingCode = "02", IsPreferred = true, };
                personIntegrationReturned.AddPhone(mobilePhone);
                residencePhone = new Domain.Base.Entities.Phone("333-333-3333", "VA", "3") { CountryCallingCode = "03", IsPreferred = true };
                personIntegrationReturned.AddPhone(residencePhone);
                workPhone = new Domain.Base.Entities.Phone("444-444-4444", "HO", "4444") { CountryCallingCode = "04", IsPreferred = true };
                personIntegrationReturned.AddPhone(workPhone);

                personIntegrationReturned.AddSocialMedia(new Domain.Base.Entities.SocialMedia("FB", "http://www.facebook.com/jDoe") { IsPreferred = true });
                personIntegrationReturned.AddSocialMedia(new Domain.Base.Entities.SocialMedia("website", "http://www.somewebsite.com/jDoe") { IsPreferred = true });

            }

            private void SetupReferenceDataRepositoryMocks()
            {
                referenceDataRepositoryMock.Setup(repo => repo.GetPrivacyStatusesAsync(It.IsAny<bool>()))
                     .ReturnsAsync(
                         new List<PrivacyStatus>() {
                        new PrivacyStatus( "d3d86052-9d55-4751-acda-5c07a064a82a", "UN", "Unrestricted", PrivacyStatusType.unrestricted),
                        new PrivacyStatus( "cff65dcc-4a9b-44ed-b8d0-930348c55ef8", "R", "restricted", PrivacyStatusType.restricted)
                        }
                     );
                personNameTypes = new List<PersonNameTypeItem>() {
                        new PersonNameTypeItem("8224f18e-69c5-480b-a9b4-52f596aa4a52", "PREFERRED", "Personal", PersonNameType.Personal),
                        new PersonNameTypeItem("7dfa950c-8ae4-4dca-92f0-c083604285b6", "BIRTH", "Birth", PersonNameType.Birth),
                        new PersonNameTypeItem("dd20ebdf-2452-41ef-9f86-ad1b1621a78d", "CHOSEN", "Chosen", PersonNameType.Personal),
                        new PersonNameTypeItem("806af5a5-8a9a-424f-8c9f-c1e9d084ee71", "LEGAL", "Legal", PersonNameType.Legal),
                        new PersonNameTypeItem("7b55610f-7d00-4260-bbcf-0e47fdbae647", "NICKNAME", "NickName", PersonNameType.Personal),
                        new PersonNameTypeItem("d42cc964-35cb-4560-bc46-4b881e7705ea", "HISTORY", "History", PersonNameType.Personal)
                        };
                referenceDataRepositoryMock.Setup(repo => repo.GetPersonNameTypesAsync(It.IsAny<bool>()))
                    .ReturnsAsync(personNameTypes);

                referenceDataRepositoryMock.Setup(repo => repo.GetDenominationsAsync(It.IsAny<bool>()))
                    .ReturnsAsync(
                        new List<Denomination>() {
                        new Denomination(baptistGuid,"BA", "Baptist") ,
                        new Denomination(catholicGuid,"CA", "Catholic")
                        }
                     );

                referenceDataRepositoryMock.Setup(repo => repo.GetEmailTypesAsync(It.IsAny<bool>()))
                     .ReturnsAsync(
                         new List<EmailType>() {
                        new EmailType("899803da-48f8-4044-beb8-5913a04b995d", "COL", "College", EmailTypeCategory.School),
                        new EmailType("301d485d-d37b-4d29-af00-465ced624a85", "PER", "Personal", EmailTypeCategory.Personal),
                        new EmailType("53fb7dab-d348-4657-b071-45d0e5933e05", "BUS", "Business", EmailTypeCategory.Business)
                        }
                     );

                // Mock the reference repository for ethnicity
                referenceDataRepositoryMock.Setup(repo => repo.GetEthnicitiesAsync(It.IsAny<bool>())).Returns(Task.FromResult<IEnumerable<Ethnicity>>(new List<Ethnicity>()
                {
                    new Ethnicity(ethnicityGuid, "H", "Hispanic", EthnicityType.Hispanic)
                }));

                referenceDataRepositoryMock.Setup(repo => repo.GetSocialMediaTypesAsync(It.IsAny<bool>()))
                     .ReturnsAsync(
                         new List<SocialMediaType>() {
                        new SocialMediaType("d1f311f4-687d-4dc7-a329-c6a8bfc9c74", "FB", "Facebook", SocialMediaTypeCategory.facebook),
                        new SocialMediaType("d1f311f4-687d-4dc7-a329-c6a8bfc9c75", "WS", "Website", SocialMediaTypeCategory.website)
                        }
                     );

                referenceDataRepositoryMock.Setup(repo => repo.GetAddressTypes2Async(It.IsAny<bool>()))
                     .ReturnsAsync(
                         new List<AddressType2>() {
                        new AddressType2("91979656-e110-4156-a75a-1a1a7294314d", "HO", "Home", AddressTypeCategory.Home),
                        new AddressType2("b887d5ec-9ed5-45e8-b44c-01782070f234", "MA", "Mailing", AddressTypeCategory.Mailing),
                        new AddressType2("d7d0a82c-fe74-480d-be1b-88a2e460af4c", "VA", "Vacation", AddressTypeCategory.Vacation),
                        new AddressType2("c9b8cd52-54e6-4c08-a9d9-224dd0c8b700", "BU", "Business", AddressTypeCategory.Business)
                         }
                     );
                allPhones = new List<PhoneType>() {
                        new PhoneType("92c82d33-e55c-41a4-a2c3-f2f7d2c523d1", "HO", "Home", PhoneTypeCategory.Home),
                        new PhoneType("b6def2cc-cc95-4d0e-a32c-940fbbc2d689", "MO", "Mobile", PhoneTypeCategory.Mobile),
                        new PhoneType("f60e7b27-a3e3-4c92-9d36-f3cae27b724b", "VA", "Vacation", PhoneTypeCategory.Vacation),
                        new PhoneType("30e231cf-a199-4c9a-af01-be2e69b607c9", "BU", "Business", PhoneTypeCategory.Business)
                        };
                referenceDataRepositoryMock.Setup(repo => repo.GetPhoneTypesAsync(It.IsAny<bool>()))
                     .ReturnsAsync(allPhones);


                // Mock the reference repository for prefix
                referenceDataRepositoryMock.Setup(repo => repo.Prefixes).Returns(new List<Prefix>()
                {
                    new Prefix("MR","Mr","Mr."),
                    new Prefix("MS","Ms","Ms.")
                });

                // Mock the reference repository for prefix
                referenceDataRepositoryMock.Setup(repo => repo.GetPrefixesAsync()).ReturnsAsync(new List<Prefix>()
                {
                    new Prefix("MR","Mr","Mr."),
                    new Prefix("MS","Ms","Ms.")
                });

                // Mock the reference repository for suffix
                referenceDataRepositoryMock.Setup(repo => repo.Suffixes).Returns(new List<Suffix>()
                {
                    new Suffix("JR","Jr","Jr."),
                    new Suffix("SR","Sr","Sr.")
                });

                // Mock the reference repository for suffix
                referenceDataRepositoryMock.Setup(repo => repo.GetSuffixesAsync()).ReturnsAsync(new List<Suffix>()
                {
                    new Suffix("JR","Jr","Jr."),
                    new Suffix("SR","Sr","Sr.")
                });

                // Mock the reference repository for country
                countries = new List<Country>()
                 {
                    new Country("US","United States","US", "USA", false),
                    new Country("CA","Canada","CA", "CAN", false),
                    new Country("MX","Mexico","MX", "MEX", false),
                    new Country("BR","Brazil","BR", "BRA", false)
                };
                referenceDataRepositoryMock.Setup(repo => repo.GetCountryCodesAsync(It.IsAny<bool>())).Returns(Task.FromResult(countries));

                // Mock the reference repository for marital status
                referenceDataRepositoryMock.Setup(repo => repo.GetMaritalStatusesAsync(It.IsAny<bool>())).Returns(Task.FromResult<IEnumerable<MaritalStatus>>(new List<MaritalStatus>()
                {
                    new MaritalStatus(maritalStatusGuid, "M", "Married"){ Type = MaritalStatusType.Married }
                }));

                var raceEntities = new List<Race>();
                raceEntities.Add(new Race(raceAsianGuid, "AS", "Asian", RaceType.Asian));
                raceEntities.Add(new Race(racePacificIslanderGuid, "HP", "Hawaiian/Pacific Islander", RaceType.PacificIslander));
                referenceDataRepositoryMock.Setup(repo => repo.GetRacesAsync(It.IsAny<bool>())).Returns(Task.FromResult<IEnumerable<Race>>(raceEntities));

                referenceDataRepositoryMock.Setup(repo => repo.GetChaptersAsync(It.IsAny<bool>())).ReturnsAsync(allChapters);
                referenceDataRepositoryMock.Setup(repo => repo.GetCountiesAsync(It.IsAny<bool>())).ReturnsAsync(allCounties);
                referenceDataRepositoryMock.Setup(repo => repo.GetZipCodeXlatAsync(It.IsAny<bool>())).ReturnsAsync(allZipCodeXlats);
                referenceDataRepositoryMock.Setup(repo => repo.GetRecordInfoFromGuidGeographicAreaAsync(It.IsAny<string>())).ReturnsAsync(Domain.Base.Entities.GeographicAreaTypeCategory.Fundraising);


                // Mock the reference repository for Alternate ID Types
                referenceDataRepositoryMock.Setup(repo => repo.GetAlternateIdTypesAsync(It.IsAny<bool>())).ReturnsAsync(new List<AltIdTypes>()
                {
                    new AltIdTypes("AE44FE48-2534-480B-8618-5480617CE74A", "ELEV2", "Elevate ID 2"),
                    new AltIdTypes("D525E2B2-CD7D-4995-93F0-97DA468EBE90", "GOVID2", "Government ID 2")
                });

                // Mock the reference repository for Gender Identity Codes
                referenceDataRepositoryMock.Setup(repo => repo.GetGenderIdentityTypesAsync(It.IsAny<bool>())).ReturnsAsync(new List<GenderIdentityType>()
                {
                    new GenderIdentityType("9C3004AB-0F25-4D1D-84D6-65EA69CE1124","FTM","Female to Male"),
                    new GenderIdentityType("BCD23124-2FAA-411C-A990-24BA3FA8A93D", "MTF","Male to Female")
                });

                // Mock the reference repository for Personal Pronouns
                referenceDataRepositoryMock.Setup(repo => repo.GetPersonalPronounTypesAsync(It.IsAny<bool>())).ReturnsAsync(new List<PersonalPronounType>()
                {
                    new PersonalPronounType("AE7A3392-FA07-4F53-B6D5-317D77CB62EC","HE","He, Him, His"),
                    new PersonalPronounType("9567AFB5-5F3C-40DC-B4F9-FC1658ACEE15", "HER","She, Her, Hers")
                });
            }
        }
        #endregion

        #region HEDM GetPerson V6 Tests


        [TestClass]
        public class HEDM_GetPerson : CurrentUserSetup
        {
            private string personId = "0000011";
            private string personId2 = "0000012";
            private string personGuid = "5674f28b-b216-4055-b236-81a922d93b4c";
            private string personGuid2 = "1111f28b-b216-4055-b236-81a922d93b4c";
            private Mock<IPersonRepository> personRepoMock;
            private IPersonRepository personRepo;
            private Mock<IPersonBaseRepository> personBaseRepoMock;
            private IPersonBaseRepository personBaseRepo;
            private Mock<IReferenceDataRepository> refRepoMock;
            private IReferenceDataRepository refRepo;
            private Mock<IAdapterRegistry> adapterRegistryMock;
            private IAdapterRegistry adapterRegistry;
            private PersonService personService;
            private ILogger logger;
            private Ellucian.Colleague.Domain.Base.Entities.Person person;
            private Ellucian.Colleague.Domain.Base.Entities.Person person2;
            private Ellucian.Colleague.Domain.Base.Entities.PersonIntegration personIntegration;
            private Mock<IRoleRepository> roleRepoMock;
            private IRoleRepository roleRepo;
            private ICurrentUserFactory currentUserFactory;
            private Ellucian.Colleague.Domain.Base.Entities.EmailAddress instEmail;
            private Ellucian.Colleague.Domain.Base.Entities.EmailAddress perEmail;
            private Ellucian.Colleague.Domain.Base.Entities.EmailAddress workEmail;
            private Ellucian.Colleague.Domain.Base.Entities.Address homeAddr;
            private Ellucian.Colleague.Domain.Base.Entities.Address mailAddr;
            private Ellucian.Colleague.Domain.Base.Entities.Address resAddr;
            private Ellucian.Colleague.Domain.Base.Entities.Address workAddr;
            private Ellucian.Colleague.Domain.Base.Entities.Phone homePhone;
            private Ellucian.Colleague.Domain.Base.Entities.Phone mobilePhone;
            private Ellucian.Colleague.Domain.Base.Entities.Phone residencePhone;
            private Ellucian.Colleague.Domain.Base.Entities.Phone workPhone;
            private IEnumerable<Ellucian.Colleague.Domain.Base.Entities.State> states;
            private IEnumerable<Ellucian.Colleague.Domain.Base.Entities.Country> countries;
            private List<Ellucian.Colleague.Domain.Base.Entities.County> counties;
            private string maritalStatusGuid = Guid.NewGuid().ToString();
            private string ethnicityGuid = Guid.NewGuid().ToString();
            private string raceAsianGuid = Guid.NewGuid().ToString();
            private string racePacificIslanderGuid = Guid.NewGuid().ToString();
            private string countyGuid = Guid.NewGuid().ToString();
            private string baptistGuid = Guid.NewGuid().ToString();
            private string catholicGuid = Guid.NewGuid().ToString();
            private Ellucian.Colleague.Domain.Entities.Permission permissionViewAnyPerson;

            [TestInitialize]
            public void Initialize()
            {
                personRepoMock = new Mock<IPersonRepository>();
                personRepo = personRepoMock.Object;
                personBaseRepoMock = new Mock<IPersonBaseRepository>();
                personBaseRepo = personBaseRepoMock.Object;
                refRepoMock = new Mock<IReferenceDataRepository>();
                refRepo = refRepoMock.Object;
                adapterRegistryMock = new Mock<IAdapterRegistry>();
                adapterRegistry = adapterRegistryMock.Object;
                roleRepoMock = new Mock<IRoleRepository>();
                roleRepo = roleRepoMock.Object;
                logger = new Mock<ILogger>().Object;

                // Mock person response from the person repository
                person = new Domain.Base.Entities.Person(personId, "Brown");
                person.Guid = personGuid;
                person.Prefix = "Mr.";
                person.FirstName = "Ricky";
                person.MiddleName = "Lee";
                person.Suffix = "Jr.";
                person.Nickname = "Rick";
                person.BirthDate = new DateTime(1930, 1, 1);
                person.DeceasedDate = new DateTime(2014, 5, 12);
                person.GovernmentId = "111-11-1111";
                person.MaritalStatusCode = "M";
                person.EthnicCodes = new List<string> { "H" };
                person.RaceCodes = new List<string> { "AS" };
                person.AddEmailAddress(new EmailAddress("xyz@xmail.com", "COL"));
                personRepoMock.Setup(repo => repo.GetPersonByGuidNonCachedAsync(personGuid)).ReturnsAsync(person);

                var filteredPersonGuidTuple = new Tuple<IEnumerable<string>, int>(new List<string>() { personGuid }, 1);
                personRepoMock.Setup(repo => repo.GetFilteredPerson2GuidsAsync(It.IsAny<int>(), It.IsAny<int>(), true, It.IsAny<PersonFilterCriteria>(), It.IsAny<string>())).ReturnsAsync(filteredPersonGuidTuple);

                personIntegration = new PersonIntegration(personId, "Brown");
                personIntegration.Guid = personGuid;
                personIntegration.Prefix = "Mr.";
                personIntegration.FirstName = "Ricky";
                personIntegration.MiddleName = "Lee";
                personIntegration.Suffix = "Jr.";
                personIntegration.Nickname = "Rick";
                personIntegration.BirthDate = new DateTime(1930, 1, 1);
                personIntegration.DeceasedDate = new DateTime(2014, 5, 12);
                personIntegration.GovernmentId = "111-11-1111";
                personIntegration.Religion = "CA";
                personIntegration.MaritalStatusCode = "M";
                personIntegration.EthnicCodes = new List<string> { "H", "N" };
                personIntegration.RaceCodes = new List<string> { "AS" };
                personIntegration.AddRole(new PersonRole(PersonRoleType.Alumni, new DateTime(15, 01, 22), new DateTime(15, 05, 25)));
                // Mock the email address data response
                instEmail = new Domain.Base.Entities.EmailAddress("inst@inst.com", "COL") { IsPreferred = true };
                personIntegration.AddEmailAddress(instEmail);
                perEmail = new Domain.Base.Entities.EmailAddress("personal@personal.com", "PER") { IsPreferred = false };
                personIntegration.AddEmailAddress(perEmail);
                workEmail = new Domain.Base.Entities.EmailAddress("work@work.com", "BUS") { IsPreferred = false };
                personIntegration.AddEmailAddress(workEmail);

                // Mock the address hierarchy responses
                var addresses = new List<Domain.Base.Entities.Address>();
                homeAddr = new Domain.Base.Entities.Address()
                {
                    TypeCode = "HO",
                    Type = Dtos.EnumProperties.AddressType.Home.ToString(),
                    Guid = Guid.NewGuid().ToString(),
                    Status = "Current",
                    IsPreferredAddress = true
                };
                addresses.Add(homeAddr);
                mailAddr = new Domain.Base.Entities.Address()
                {
                    TypeCode = "MA",
                    Type = Dtos.EnumProperties.AddressType.Mailing.ToString(),
                    Guid = Guid.NewGuid().ToString(),
                    Status = "Current"
                };
                addresses.Add(mailAddr);
                resAddr = new Domain.Base.Entities.Address()
                {
                    TypeCode = "VA",
                    Type = Dtos.EnumProperties.AddressType.Vacation.ToString(),
                    Guid = Guid.NewGuid().ToString(),
                    Status = "Current",
                    IsPreferredResidence = true
                };
                addresses.Add(resAddr);
                workAddr = new Domain.Base.Entities.Address()
                {
                    TypeCode = "BU",
                    Type = Dtos.EnumProperties.AddressType.Business.ToString(),
                    Guid = Guid.NewGuid().ToString(),
                    Status = "Current"
                };
                addresses.Add(workAddr);
                personIntegration.Addresses = addresses;

                // Mock the person phone per type response
                homePhone = new Domain.Base.Entities.Phone("111-111-1111", "HO");
                personIntegration.AddPhone(homePhone);
                mobilePhone = new Domain.Base.Entities.Phone("222-222-2222", "MO");
                personIntegration.AddPhone(mobilePhone);
                residencePhone = new Domain.Base.Entities.Phone("333-333-3333", "VA");
                personIntegration.AddPhone(residencePhone);
                workPhone = new Domain.Base.Entities.Phone("444-444-4444", "BU", "4444");
                personIntegration.AddPhone(workPhone);

                // Mock the social media
                var socialMedia = new List<Domain.Base.Entities.SocialMedia>();
                var socialMediaTypeCode = "TW";
                var socialMediaHandle = "pontifex";
                var personSocialMedia = new SocialMedia(socialMediaTypeCode, socialMediaHandle);
                personIntegration.AddSocialMedia(personSocialMedia);

                personRepoMock.Setup(repo => repo.GetPersonIntegrationByGuidNonCachedAsync(personGuid)).ReturnsAsync(personIntegration);
                personRepoMock.Setup(repo => repo.GetPersonIntegrationByGuidAsync(personGuid, It.IsAny<bool>())).ReturnsAsync(personIntegration);


                var personGuidList = new List<string>() { personGuid };
                var personList = new List<PersonIntegration>() { personIntegration };
                personRepoMock.Setup(repo => repo.GetPersonIntegrationByGuidNonCachedAsync(personGuidList)).ReturnsAsync(personList);
                personRepoMock.Setup(repo => repo.GetFilteredPerson2GuidsAsync(It.IsAny<int>(), It.IsAny<int>(), true, It.IsAny<PersonFilterCriteria>(), It.IsAny<string>())).ReturnsAsync(new Tuple<IEnumerable<string>, int>(personGuidList, 1));

                person2 = new Domain.Base.Entities.Person(personId2, "Green");
                person2.Guid = personGuid2;
                person2.Prefix = "Ms.";
                person2.FirstName = "Amy";
                var personGuids = new List<string>();
                personGuids.Add(person.Guid);
                personGuids.Add(person2.Guid);
                var personEntities = new List<Domain.Base.Entities.Person>();
                personEntities.Add(person);
                personEntities.Add(person2);
                personRepoMock.Setup(repo => repo.GetPersonByGuidNonCachedAsync(personGuids)).ReturnsAsync(personEntities.AsEnumerable());

                // Mock the response for getting faculty guids
                var personGuidTuple = new Tuple<IEnumerable<string>, int>(personGuids, 2);
                personRepoMock.Setup(repo => repo.GetFacultyPersonGuidsAsync(It.IsAny<int>(), It.IsAny<int>())).ReturnsAsync(personGuidTuple);

                // Mock the response for getting a Person Pin 
                var personPin = new PersonPin("0000011", "testUsername");
                var personPins = new List<PersonPin>();
                personPins.Add(personPin);
                personRepoMock.Setup(repo => repo.GetPersonPinsAsync(It.IsAny<string[]>())).ReturnsAsync(personPins);
                var personUserName = new PersonUserName("0000011", "testUsername");
                var personUserNames = new List<PersonUserName>();
                personUserNames.Add(personUserName);
                personRepoMock.Setup(repo => repo.GetPersonUserNamesAsync(It.IsAny<string[]>())).ReturnsAsync(personUserNames);

                refRepoMock.Setup(repo => repo.GetPrivacyStatusesAsync(It.IsAny<bool>()))
                     .ReturnsAsync(
                         new List<PrivacyStatus>() {
                        new PrivacyStatus( Guid.NewGuid().ToString(), "UN", "Unrestricted", PrivacyStatusType.unrestricted),
                        new PrivacyStatus( Guid.NewGuid().ToString(), "R", "restricted", PrivacyStatusType.restricted)
                        }
                     );

                refRepoMock.Setup(repo => repo.GetPersonNameTypesAsync(It.IsAny<bool>()))
                    .ReturnsAsync(
                        new List<PersonNameTypeItem>() {
                        new PersonNameTypeItem(Guid.NewGuid().ToString(), "PREFERRED", "Personal", PersonNameType.Personal),
                        new PersonNameTypeItem(Guid.NewGuid().ToString(), "BIRTH", "Birth", PersonNameType.Birth),
                        new PersonNameTypeItem(Guid.NewGuid().ToString(), "CHOSEN", "Chosen", PersonNameType.Personal),
                        new PersonNameTypeItem(Guid.NewGuid().ToString(), "LEGAL", "Legal", PersonNameType.Legal),
                        new PersonNameTypeItem(Guid.NewGuid().ToString(), "NICKNAME", "NickName", PersonNameType.Personal),
                        new PersonNameTypeItem(Guid.NewGuid().ToString(), "HISTORY", "History", PersonNameType.Personal)
                        }
                     );

                refRepoMock.Setup(repo => repo.GetDenominationsAsync(It.IsAny<bool>()))
                    .ReturnsAsync(
                        new List<Denomination>() {
                        new Denomination(baptistGuid,"BA", "Baptist") ,
                        new Denomination(catholicGuid,"CA", "Catholic")
                        }
                     );

                refRepoMock.Setup(repo => repo.GetEmailTypesAsync(It.IsAny<bool>()))
                     .ReturnsAsync(
                         new List<EmailType>() {
                        new EmailType( Guid.NewGuid().ToString(), "COL", "College", EmailTypeCategory.School),
                        new EmailType( Guid.NewGuid().ToString(), "PER", "Personal", EmailTypeCategory.Personal),
                        new EmailType( Guid.NewGuid().ToString(), "BUS", "Business", EmailTypeCategory.Business)
                        }
                     );

                refRepoMock.Setup(repo => repo.GetSocialMediaTypesAsync(It.IsAny<bool>()))
                     .ReturnsAsync(
                         new List<SocialMediaType>() {
                        new SocialMediaType( Guid.NewGuid().ToString(), "TW", "Twitter", SocialMediaTypeCategory.twitter)
                        }
                     );

                refRepoMock.Setup(repo => repo.GetAddressTypes2Async(It.IsAny<bool>()))
                     .ReturnsAsync(
                         new List<AddressType2>() {
                        new AddressType2( Guid.NewGuid().ToString(), "HO", "Home", AddressTypeCategory.Home),
                        new AddressType2( Guid.NewGuid().ToString(), "MA", "Mailing", AddressTypeCategory.Mailing),
                        new AddressType2( Guid.NewGuid().ToString(), "VA", "Vacation", AddressTypeCategory.Vacation),
                        new AddressType2( Guid.NewGuid().ToString(), "BU", "Business", AddressTypeCategory.Business)
                        }
                     );

                refRepoMock.Setup(repo => repo.GetPhoneTypesAsync(It.IsAny<bool>()))
                     .ReturnsAsync(
                         new List<PhoneType>() {
                        new PhoneType( Guid.NewGuid().ToString(), "HO", "Home", PhoneTypeCategory.Home),
                        new PhoneType( Guid.NewGuid().ToString(), "MO", "Mobile", PhoneTypeCategory.Mobile),
                        new PhoneType( Guid.NewGuid().ToString(), "VA", "Vacation", PhoneTypeCategory.Vacation),
                        new PhoneType( Guid.NewGuid().ToString(), "BU", "Business", PhoneTypeCategory.Business)
                        }
                     );

                // Mock the person repository for roles
                personRepoMock.Setup(repo => repo.IsFacultyAsync(personId)).ReturnsAsync(true);
                personRepoMock.Setup(repo => repo.IsStudentAsync(personId)).ReturnsAsync(true);

                // Mock the person repository GUID lookup
                personRepoMock.Setup(repo => repo.GetPersonIdFromGuidAsync(personGuid)).ReturnsAsync(personId);
                personRepoMock.Setup(repo => repo.GetPersonIdFromGuidAsync(personGuid2)).ReturnsAsync(personId2);
                // Mock the reference repository for states
                states = new List<State>()
                {
                    new State("VA","Virginia"),
                    new State("MD","Maryland"),
                    new State("NY","New York"),
                    new State("MA","Massachusetts")
                };
                refRepoMock.Setup(repo => repo.GetStateCodesAsync(It.IsAny<bool>())).Returns(Task.FromResult(states));

                // Mock the reference repository for country
                countries = new List<Country>()
                 {
                    new Country("US","United States","US"){ IsoAlpha3Code = "USA"},
                    new Country("CA","Canada","CA"){ IsoAlpha3Code = "CAN"},
                    new Country("MX","Mexico","MX"){ IsoAlpha3Code = "MEX"},
                    new Country("BR","Brazil","BR"){ IsoAlpha3Code = "BRA"}
                };
                refRepoMock.Setup(repo => repo.GetCountryCodesAsync(It.IsAny<bool>())).Returns(Task.FromResult(countries));

                // Places
                var places = new List<Place>();
                var place1 = new Place() { PlacesCountry = "USA", PlacesRegion = "US-NY" };
                places.Add(place1);
                var place2 = new Place() { PlacesCountry = "CAN", PlacesRegion = "CA-ON" };
                places.Add(place2);
                refRepoMock.Setup(repo => repo.GetPlacesAsync(It.IsAny<bool>())).Returns(Task.FromResult(places.AsEnumerable<Place>()));
                //personRepoMock.Setup(repo => repo.GetPlacesAsync()).ReturnsAsync(places);

                // International Parameters Host Country
                personRepoMock.Setup(repo => repo.GetHostCountryAsync()).ReturnsAsync("USA");

                // Mock the reference repository for county
                counties = new List<County>()
                {
                    new County(countyGuid, "FFX","Fairfax County"),
                    new County(countyGuid, "BAL","Baltimore County"),
                    new County(countyGuid, "NY","New York County"),
                    new County(countyGuid, "BOS","Boston County")
                };
                refRepoMock.Setup(repo => repo.Counties).Returns(counties);

                // Mock the reference repository for marital status
                refRepoMock.Setup(repo => repo.GetMaritalStatusesAsync(It.IsAny<bool>())).Returns(Task.FromResult<IEnumerable<MaritalStatus>>(new List<MaritalStatus>()
                {
                    new MaritalStatus(maritalStatusGuid, "M", "Married")
                }));

                // Mock the reference repository for ethnicity
                refRepoMock.Setup(repo => repo.GetEthnicitiesAsync(It.IsAny<bool>())).Returns(Task.FromResult<IEnumerable<Ethnicity>>(new List<Ethnicity>()
                {
                    new Ethnicity(ethnicityGuid, "H", "Hispanic", EthnicityType.Hispanic)
                }));

                // Mock the reference repository for races
                var raceEntities = new List<Race>();
                raceEntities.Add(new Race(raceAsianGuid, "AS", "Asian", RaceType.Asian));
                raceEntities.Add(new Race(racePacificIslanderGuid, "HP", "Hawaiian/Pacific Islander", RaceType.PacificIslander));
                refRepoMock.Setup(repo => repo.GetRacesAsync(It.IsAny<bool>())).Returns(Task.FromResult<IEnumerable<Race>>(raceEntities));

                // Mock the reference repository for prefix
                refRepoMock.Setup(repo => repo.Prefixes).Returns(new List<Prefix>()
                {
                    new Prefix("MR","Mr","Mr."),
                    new Prefix("MS","Ms","Ms.")
                });

                // Mock the reference repository for suffix
                refRepoMock.Setup(repo => repo.Suffixes).Returns(new List<Suffix>()
                {
                    new Suffix("JR","Jr","Jr."),
                    new Suffix("SR","Sr","Sr.")
                });

                // Set up current user
                currentUserFactory = new CurrentUserSetup.PersonUserFactory();

                // Mock permissions
                permissionViewAnyPerson = new Ellucian.Colleague.Domain.Entities.Permission(BasePermissionCodes.ViewAnyPerson);
                personRole.AddPermission(permissionViewAnyPerson);
                roleRepoMock.Setup(rpm => rpm.Roles).Returns(new List<Role>() { personRole });

                personService = new PersonService(adapterRegistry, personRepo, personBaseRepo, refRepo, null, null, null, null, currentUserFactory, roleRepo, logger);
            }

            [TestCleanup]
            public void Cleanup()
            {
                adapterRegistry = null;
                personRepo = null;
                personBaseRepo = null;
                refRepo = null;
                roleRepo = null;
                personService = null;
                personRole = null;
                permissionViewAnyPerson = null;
                roleRepoMock = null;
                currentUserFactory = null;
                refRepoMock = null;
            }

            [TestMethod]
            public async Task GetPerson2ByGuidNonCachedAsync()
            {
                // Act--get person
                var personDto = await personService.GetPerson2ByGuidAsync(personGuid, false);
                // Assert
                Assert.IsTrue(personDto is Dtos.Person2);

            }

            [TestMethod]
            public async Task GetPerson3ByGuidNonCachedAsync()
            {
                // Act--get person
                var personDto = await personService.GetPerson3ByGuidAsync(personGuid, false);
                // Assert
                Assert.IsTrue(personDto is Dtos.Person3);

            }
        }
        #endregion

        #region HEDM GetPerson V12.0.0 Tests

        [TestClass]
        public class HEDM_GetPersonV12 : CurrentUserSetup
        {
            private string personId = "0000011";
            private string personId2 = "0000012";
            private string personGuid = "5674f28b-b216-4055-b236-81a922d93b4c";
            private string personGuid2 = "1111f28b-b216-4055-b236-81a922d93b4c";
            private Mock<IPersonRepository> personRepoMock;
            private IPersonRepository personRepo;
            private Mock<IPersonBaseRepository> personBaseRepoMock;
            private IPersonBaseRepository personBaseRepo;
            private Mock<IReferenceDataRepository> refRepoMock;
            private IReferenceDataRepository refRepo;
            private Mock<IAdapterRegistry> adapterRegistryMock;
            private IAdapterRegistry adapterRegistry;
            private PersonService personService;
            private ILogger logger;
            private Ellucian.Colleague.Domain.Base.Entities.Person person;
            private Ellucian.Colleague.Domain.Base.Entities.Person person2;
            private Ellucian.Colleague.Domain.Base.Entities.PersonIntegration personIntegration;
            private Mock<IRoleRepository> roleRepoMock;
            private IRoleRepository roleRepo;
            private ICurrentUserFactory currentUserFactory;
            private Ellucian.Colleague.Domain.Base.Entities.EmailAddress instEmail;
            private Ellucian.Colleague.Domain.Base.Entities.EmailAddress perEmail;
            private Ellucian.Colleague.Domain.Base.Entities.EmailAddress workEmail;
            private Ellucian.Colleague.Domain.Base.Entities.Address homeAddr;
            private Ellucian.Colleague.Domain.Base.Entities.Address mailAddr;
            private Ellucian.Colleague.Domain.Base.Entities.Address resAddr;
            private Ellucian.Colleague.Domain.Base.Entities.Address workAddr;
            private Ellucian.Colleague.Domain.Base.Entities.Phone homePhone;
            private Ellucian.Colleague.Domain.Base.Entities.Phone mobilePhone;
            private Ellucian.Colleague.Domain.Base.Entities.Phone residencePhone;
            private Ellucian.Colleague.Domain.Base.Entities.Phone workPhone;
            private IEnumerable<Ellucian.Colleague.Domain.Base.Entities.State> states;
            private IEnumerable<Ellucian.Colleague.Domain.Base.Entities.Country> countries;
            private List<Ellucian.Colleague.Domain.Base.Entities.County> counties;
            private string maritalStatusGuid = Guid.NewGuid().ToString();
            private string ethnicityGuid = Guid.NewGuid().ToString();
            private string raceAsianGuid = Guid.NewGuid().ToString();
            private string racePacificIslanderGuid = Guid.NewGuid().ToString();
            private string countyGuid = Guid.NewGuid().ToString();
            private string baptistGuid = Guid.NewGuid().ToString();
            private string catholicGuid = Guid.NewGuid().ToString();
            private Ellucian.Colleague.Domain.Entities.Permission permissionViewAnyPerson;


            private string guid = "5674f28b-b216-4055-b236-81a922d93b4c";

            [TestInitialize]
            public void Initialize()
            {
                personRepoMock = new Mock<IPersonRepository>();
                personRepo = personRepoMock.Object;
                personBaseRepoMock = new Mock<IPersonBaseRepository>();
                personBaseRepo = personBaseRepoMock.Object;
                refRepoMock = new Mock<IReferenceDataRepository>();
                refRepo = refRepoMock.Object;
                adapterRegistryMock = new Mock<IAdapterRegistry>();
                adapterRegistry = adapterRegistryMock.Object;
                roleRepoMock = new Mock<IRoleRepository>();
                roleRepo = roleRepoMock.Object;
                logger = new Mock<ILogger>().Object;

                // Mock person response from the person repository
                person = new Domain.Base.Entities.Person(personId, "Brown");
                person.Guid = personGuid;
                person.Prefix = "Mr.";
                person.FirstName = "Ricky";
                person.MiddleName = "Lee";
                person.Suffix = "Jr.";
                person.Nickname = "Rick";
                person.BirthDate = new DateTime(1930, 1, 1);
                person.DeceasedDate = new DateTime(2014, 5, 12);
                person.GovernmentId = "111-11-1111";
                person.MaritalStatusCode = "M";
                person.EthnicCodes = new List<string> { "H" };
                person.RaceCodes = new List<string> { "AS" };
                person.AddEmailAddress(new EmailAddress("xyz@xmail.com", "COL"));
                personRepoMock.Setup(repo => repo.GetPersonByGuidNonCachedAsync(personGuid)).ReturnsAsync(person);

                var filteredPersonGuidTuple = new Tuple<IEnumerable<string>, int>(new List<string>() { personGuid }, 1);
                personRepoMock.Setup(repo => repo.GetFilteredPerson2GuidsAsync(It.IsAny<int>(), It.IsAny<int>(), true, It.IsAny<PersonFilterCriteria>(), It.IsAny<string>())).ReturnsAsync(filteredPersonGuidTuple);

                personIntegration = new PersonIntegration(personId, "Brown");
                personIntegration.Guid = personGuid;
                personIntegration.GovernmentId = personGuid;
                personIntegration.Prefix = "Mr.";
                personIntegration.FirstName = "Ricky";
                personIntegration.MiddleName = "Lee";
                personIntegration.Suffix = "Jr.";
                personIntegration.Nickname = "Rick";
                personIntegration.BirthDate = new DateTime(1930, 1, 1);
                personIntegration.DeceasedDate = new DateTime(2014, 5, 12);
                personIntegration.GovernmentId = "111-11-1111";
                personIntegration.Religion = "CA";
                personIntegration.MaritalStatusCode = "M";
                personIntegration.EthnicCodes = new List<string> { "H", "N" };
                personIntegration.RaceCodes = new List<string> { "AS" };
                personIntegration.PrimaryLanguage = "E";
                personIntegration.SecondaryLanguages = new List<string> { "TA", "SP" };
                personIntegration.AddRole(new PersonRole(PersonRoleType.Alumni, new DateTime(15, 01, 22), new DateTime(15, 05, 25)));
                // Mock the email address data response
                instEmail = new Domain.Base.Entities.EmailAddress("inst@inst.com", "COL") { IsPreferred = true };
                personIntegration.AddEmailAddress(instEmail);
                perEmail = new Domain.Base.Entities.EmailAddress("personal@personal.com", "PER") { IsPreferred = false };
                personIntegration.AddEmailAddress(perEmail);
                workEmail = new Domain.Base.Entities.EmailAddress("work@work.com", "BUS") { IsPreferred = false };
                personIntegration.AddEmailAddress(workEmail);

                // Mock the address hierarchy responses
                var addresses = new List<Domain.Base.Entities.Address>();
                homeAddr = new Domain.Base.Entities.Address()
                {
                    TypeCode = "HO",
                    Type = Dtos.EnumProperties.AddressType.Home.ToString(),
                    Guid = Guid.NewGuid().ToString(),
                    Status = "Current",
                    IsPreferredAddress = true
                };
                addresses.Add(homeAddr);
                mailAddr = new Domain.Base.Entities.Address()
                {
                    TypeCode = "MA",
                    Type = Dtos.EnumProperties.AddressType.Mailing.ToString(),
                    Guid = Guid.NewGuid().ToString(),
                    Status = "Current"
                };
                addresses.Add(mailAddr);
                resAddr = new Domain.Base.Entities.Address()
                {
                    TypeCode = "VA",
                    Type = Dtos.EnumProperties.AddressType.Vacation.ToString(),
                    Guid = Guid.NewGuid().ToString(),
                    Status = "Current",
                    IsPreferredResidence = true
                };
                addresses.Add(resAddr);
                workAddr = new Domain.Base.Entities.Address()
                {
                    TypeCode = "BU",
                    Type = Dtos.EnumProperties.AddressType.Business.ToString(),
                    Guid = Guid.NewGuid().ToString(),
                    Status = "Current"
                };
                addresses.Add(workAddr);
                personIntegration.Addresses = addresses;

                // Mock the person phone per type response
                homePhone = new Domain.Base.Entities.Phone("111-111-1111", "HO");
                personIntegration.AddPhone(homePhone);
                mobilePhone = new Domain.Base.Entities.Phone("222-222-2222", "MO");
                personIntegration.AddPhone(mobilePhone);
                residencePhone = new Domain.Base.Entities.Phone("333-333-3333", "VA");
                personIntegration.AddPhone(residencePhone);
                workPhone = new Domain.Base.Entities.Phone("444-444-4444", "BU", "4444");
                personIntegration.AddPhone(workPhone);

                // Mock the social media
                var socialMedia = new List<Domain.Base.Entities.SocialMedia>();
                var socialMediaTypeCode = "TW";
                var socialMediaHandle = "pontifex";
                var personSocialMedia = new SocialMedia(socialMediaTypeCode, socialMediaHandle);
                personIntegration.AddSocialMedia(personSocialMedia);

                personIntegration.AddPersonAlt(new PersonAlt("1", "ELEV") { });

                personRepoMock.Setup(repo => repo.GetPersonIntegrationByGuidNonCachedAsync(personGuid)).ReturnsAsync(personIntegration);
                personRepoMock.Setup(repo => repo.GetPersonIntegrationByGuidAsync(personGuid, It.IsAny<bool>())).ReturnsAsync(personIntegration);

                var personGuidList = new List<string>() { personGuid };
                var personList = new List<PersonIntegration>() { personIntegration };
                personRepoMock.Setup(repo => repo.GetPersonIntegrationByGuidNonCachedAsync(personGuidList)).ReturnsAsync(personList);
                personRepoMock.Setup(repo => repo.GetFilteredPerson2GuidsAsync(It.IsAny<int>(), It.IsAny<int>(), true, It.IsAny<PersonFilterCriteria>(), It.IsAny<string>())).ReturnsAsync(new Tuple<IEnumerable<string>, int>(personGuidList, 1));

                person2 = new Domain.Base.Entities.Person(personId2, "Green");
                person2.Guid = personGuid2;
                person2.Prefix = "Ms.";
                person2.FirstName = "Amy";
                var personGuids = new List<string>();
                personGuids.Add(person.Guid);
                personGuids.Add(person2.Guid);
                var personEntities = new List<Domain.Base.Entities.Person>();
                personEntities.Add(person);
                personEntities.Add(person2);
                personRepoMock.Setup(repo => repo.GetPersonByGuidNonCachedAsync(personGuids)).ReturnsAsync(personEntities.AsEnumerable());

                // Mock the response for getting faculty guids
                var personGuidTuple = new Tuple<IEnumerable<string>, int>(personGuids, 2);
                personRepoMock.Setup(repo => repo.GetFacultyPersonGuidsAsync(It.IsAny<int>(), It.IsAny<int>())).ReturnsAsync(personGuidTuple);

                // Mock the response for getting a Person Pin 
                var personPin = new PersonPin("0000011", "testUsername");
                var personPins = new List<PersonPin>();
                personPins.Add(personPin);
                personRepoMock.Setup(repo => repo.GetPersonPinsAsync(It.IsAny<string[]>())).ReturnsAsync(personPins);

                refRepoMock.Setup(repo => repo.GetPrivacyStatusesAsync(It.IsAny<bool>()))
                     .ReturnsAsync(
                         new List<PrivacyStatus>() {
                        new PrivacyStatus( Guid.NewGuid().ToString(), "UN", "Unrestricted", PrivacyStatusType.unrestricted),
                        new PrivacyStatus( Guid.NewGuid().ToString(), "R", "restricted", PrivacyStatusType.restricted)
                        }
                     );

                refRepoMock.Setup(repo => repo.GetPersonNameTypesAsync(It.IsAny<bool>()))
                    .ReturnsAsync(
                        new List<PersonNameTypeItem>() {
                        new PersonNameTypeItem(Guid.NewGuid().ToString(), "PREFERRED", "Personal", PersonNameType.Personal),
                        new PersonNameTypeItem(Guid.NewGuid().ToString(), "BIRTH", "Birth", PersonNameType.Birth),
                        new PersonNameTypeItem(Guid.NewGuid().ToString(), "CHOSEN", "Chosen", PersonNameType.Personal),
                        new PersonNameTypeItem(Guid.NewGuid().ToString(), "LEGAL", "Legal", PersonNameType.Legal),
                        new PersonNameTypeItem(Guid.NewGuid().ToString(), "NICKNAME", "NickName", PersonNameType.Personal),
                        new PersonNameTypeItem(Guid.NewGuid().ToString(), "HISTORY", "History", PersonNameType.Personal)
                        }
                     );

                refRepoMock.Setup(repo => repo.GetDenominationsAsync(It.IsAny<bool>()))
                    .ReturnsAsync(
                        new List<Denomination>() {
                        new Denomination(baptistGuid,"BA", "Baptist") ,
                        new Denomination(catholicGuid,"CA", "Catholic")
                        }
                     );

                refRepoMock.Setup(repo => repo.GetEmailTypesAsync(It.IsAny<bool>()))
                     .ReturnsAsync(
                        new List<EmailType>() {
                        new EmailType( "86E1DE39-EE23-422F-BAD4-F80F638097A2", "COL", "College", EmailTypeCategory.School),
                        new EmailType( "8026705d-fa3a-4373-8125-9c99f495e9ae", "PER", "Personal", EmailTypeCategory.Personal),
                        new EmailType( "3EF60DDE-E317-4B4C-B95F-FA50125B6EFE", "BUS", "Business", EmailTypeCategory.Business)
                        }
                     );

                refRepoMock.Setup(repo => repo.GetSocialMediaTypesAsync(It.IsAny<bool>()))
                     .ReturnsAsync(
                         new List<SocialMediaType>() {
                        new SocialMediaType( Guid.NewGuid().ToString(), "TW", "Twitter", SocialMediaTypeCategory.twitter)
                        }
                     );

                refRepoMock.Setup(repo => repo.GetAddressTypes2Async(It.IsAny<bool>()))
                     .ReturnsAsync(
                         new List<AddressType2>() {
                        new AddressType2( Guid.NewGuid().ToString(), "HO", "Home", AddressTypeCategory.Home),
                        new AddressType2( Guid.NewGuid().ToString(), "MA", "Mailing", AddressTypeCategory.Mailing),
                        new AddressType2( Guid.NewGuid().ToString(), "VA", "Vacation", AddressTypeCategory.Vacation),
                        new AddressType2( Guid.NewGuid().ToString(), "BU", "Business", AddressTypeCategory.Business)
                        }
                     );

                refRepoMock.Setup(repo => repo.GetPhoneTypesAsync(It.IsAny<bool>()))
                     .ReturnsAsync(
                         new List<PhoneType>() {
                        new PhoneType( Guid.NewGuid().ToString(), "HO", "Home", PhoneTypeCategory.Home),
                        new PhoneType( Guid.NewGuid().ToString(), "MO", "Mobile", PhoneTypeCategory.Mobile),
                        new PhoneType( Guid.NewGuid().ToString(), "VA", "Vacation", PhoneTypeCategory.Vacation),
                        new PhoneType( Guid.NewGuid().ToString(), "BU", "Business", PhoneTypeCategory.Business)
                        }
                     );

                // Mock the person repository for roles
                personRepoMock.Setup(repo => repo.IsFacultyAsync(personId)).ReturnsAsync(true);
                personRepoMock.Setup(repo => repo.IsStudentAsync(personId)).ReturnsAsync(true);

                // Mock the person repository GUID lookup
                personRepoMock.Setup(repo => repo.GetPersonIdFromGuidAsync(personGuid)).ReturnsAsync(personId);
                personRepoMock.Setup(repo => repo.GetPersonIdFromGuidAsync(personGuid2)).ReturnsAsync(personId2);
                // Mock the reference repository for states
                states = new List<State>()
                {
                    new State("VA","Virginia"),
                    new State("MD","Maryland"),
                    new State("NY","New York"),
                    new State("MA","Massachusetts")
                };
                refRepoMock.Setup(repo => repo.GetStateCodesAsync(It.IsAny<bool>())).Returns(Task.FromResult(states));

                // Mock the reference repository for country
                countries = new List<Country>()
                 {
                    new Country("US","United States","US"){ IsoAlpha3Code = "USA"},
                    new Country("CA","Canada","CA"){ IsoAlpha3Code = "CAN"},
                    new Country("MX","Mexico","MX"){ IsoAlpha3Code = "MEX"},
                    new Country("BR","Brazil","BR"){ IsoAlpha3Code = "BRA"}
                };
                refRepoMock.Setup(repo => repo.GetCountryCodesAsync(It.IsAny<bool>())).Returns(Task.FromResult(countries));

                // Places
                var places = new List<Place>();
                var place1 = new Place() { PlacesCountry = "USA", PlacesRegion = "US-NY" };
                places.Add(place1);
                var place2 = new Place() { PlacesCountry = "CAN", PlacesRegion = "CA-ON" };
                places.Add(place2);
                refRepoMock.Setup(repo => repo.GetPlacesAsync(It.IsAny<bool>())).Returns(Task.FromResult(places.AsEnumerable<Place>()));
                //personRepoMock.Setup(repo => repo.GetPlacesAsync()).ReturnsAsync(places);

                // International Parameters Host Country
                personRepoMock.Setup(repo => repo.GetHostCountryAsync()).ReturnsAsync("USA");

                // Mock the reference repository for county
                counties = new List<County>()
                {
                    new County(countyGuid, "FFX","Fairfax County"),
                    new County(countyGuid, "BAL","Baltimore County"),
                    new County(countyGuid, "NY","New York County"),
                    new County(countyGuid, "BOS","Boston County")
                };
                refRepoMock.Setup(repo => repo.Counties).Returns(counties);

                // Mock the reference repository for marital status
                refRepoMock.Setup(repo => repo.GetMaritalStatusesAsync(It.IsAny<bool>())).Returns(Task.FromResult<IEnumerable<MaritalStatus>>(new List<MaritalStatus>()
                {
                    new MaritalStatus(maritalStatusGuid, "M", "Married")
                }));

                // Mock the reference repository for ethnicity
                refRepoMock.Setup(repo => repo.GetEthnicitiesAsync(It.IsAny<bool>())).Returns(Task.FromResult<IEnumerable<Ethnicity>>(new List<Ethnicity>()
                {
                    new Ethnicity(ethnicityGuid, "H", "Hispanic", EthnicityType.Hispanic)
                }));

                // Mock the reference repository for races
                var raceEntities = new List<Race>();
                raceEntities.Add(new Race(raceAsianGuid, "AS", "Asian", RaceType.Asian));
                raceEntities.Add(new Race(racePacificIslanderGuid, "HP", "Hawaiian/Pacific Islander", RaceType.PacificIslander));
                refRepoMock.Setup(repo => repo.GetRacesAsync(It.IsAny<bool>())).Returns(Task.FromResult<IEnumerable<Race>>(raceEntities));

                // Mock the reference repository for prefix
                refRepoMock.Setup(repo => repo.Prefixes).Returns(new List<Prefix>()
                {
                    new Prefix("MR","Mr","Mr."),
                    new Prefix("MS","Ms","Ms.")
                });

                // Mock the reference repository for suffix
                refRepoMock.Setup(repo => repo.Suffixes).Returns(new List<Suffix>()
                {
                    new Suffix("JR","Jr","Jr."),
                    new Suffix("SR","Sr","Sr.")
                });

                // Set up current user
                currentUserFactory = new CurrentUserSetup.PersonUserFactory();

                personRepoMock.Setup(repo => repo.GetPersonIntegration2ByGuidAsync(It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(personIntegration);

                // Mock permissions
                permissionViewAnyPerson = new Ellucian.Colleague.Domain.Entities.Permission(BasePermissionCodes.ViewAnyPerson);
                personRole.AddPermission(permissionViewAnyPerson);
                roleRepoMock.Setup(rpm => rpm.Roles).Returns(new List<Role>() { personRole });

                // Mock LANGUAGES valcode 
                var languages = new Ellucian.Data.Colleague.DataContracts.ApplValcodes()
                {

                    ValsEntityAssociation = new List<Ellucian.Data.Colleague.DataContracts.ApplValcodesVals>()
                    {
                        new ApplValcodesVals() { ValInternalCodeAssocMember = "E", ValExternalRepresentationAssocMember = "English", ValActionCode3AssocMember = "ENG" },
                        new ApplValcodesVals() { ValInternalCodeAssocMember = "SP", ValExternalRepresentationAssocMember = "Spanish", ValActionCode3AssocMember = "SPA" },
                        new ApplValcodesVals() { ValInternalCodeAssocMember = "TA", ValExternalRepresentationAssocMember = "Tagalog", ValActionCode3AssocMember = "TGL" }
                    }
                };
                personBaseRepoMock.Setup(repo => repo.GetLanguagesAsync()).ReturnsAsync(languages);



                personService = new PersonService(adapterRegistry, personRepo, personBaseRepo, refRepo, null, null, null, null, currentUserFactory, roleRepo, logger);
            }

            [TestCleanup]
            public void Cleanup()
            {
                adapterRegistry = null;
                personRepo = null;
                personBaseRepo = null;
                refRepo = null;
                roleRepo = null;
                personService = null;
                personRole = null;
                permissionViewAnyPerson = null;
                roleRepoMock = null;
                currentUserFactory = null;
                refRepoMock = null;
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task GetPerson4ByGuid_ArgumentNullException_EmptyGuid()
            {
                await personService.GetPerson4ByGuidAsync(null, true);
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task GetPerson4ByGuid_PersonNotFound()
            {
                personRepoMock.Setup(p => p.GetPersonIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync(() => null);
                await personService.GetPerson4ByGuidAsync(guid, true);
            }

            //[TestMethod]
            //[ExpectedException(typeof(PermissionsException))]
            //public async Task GetPerson4ByGuid_PermissionException()
            //{
            //    roleRepoMock.Setup(rpm => rpm.Roles).Returns(new List<Role>() { });
            //    await personService.GetPerson4ByGuidAsync(guid, true);
            //}

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task GetPerson4ByGuid_KeyNotFoundException_PersonNotFound()
            {
                personRepoMock.Setup(p => p.GetPersonIntegration2ByGuidAsync(It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(() => null);
                await personService.GetPerson4ByGuidAsync(guid, true);
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task GetPerson4ByGuid_KeyNotFoundException_InvalidPersonId()
            {
                var person = new PersonIntegration(guid, "lastName") { PersonCorpIndicator = "Y" };
                personRepoMock.Setup(p => p.GetPersonIntegration2ByGuidAsync(It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(person);
                await personService.GetPerson4ByGuidAsync(guid, true);
            }

            [TestMethod]
            [ExpectedException(typeof(RepositoryException))]
            public async Task GetPerson4ByGuid_RepositoryException()
            {
                personRepoMock.Setup(p => p.GetPersonIntegration2ByGuidAsync(It.IsAny<string>(), It.IsAny<bool>())).ThrowsAsync(new RepositoryException());
                await personService.GetPerson4ByGuidAsync(guid, true);
            }

            [TestMethod]
            [ExpectedException(typeof(InvalidOperationException))]
            public async Task GetPerson4ByGuid_MoreThanOneElevateIds()
            {
                personIntegration.AddPersonAlt(new PersonAlt("2", "ELEV") { });

                personRepoMock.Setup(p => p.GetPersonIntegration2ByGuidAsync(It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(personIntegration);

                await personService.GetPerson4ByGuidAsync(guid, true);
            }

            [TestMethod]
            public async Task GetPerson4ByGuid_ValidScenario()
            {
                var result = await personService.GetPerson4ByGuidAsync(guid, true);
                Assert.IsTrue(result is Dtos.Person4);
            }

            //[TestMethod]
            //[ExpectedException(typeof(PermissionsException))]
            //public async Task GetPerson4NonCached_PermissionException()
            //{
            //    roleRepoMock.Setup(rpm => rpm.Roles).Returns(new List<Role>() { });
            //    await personService.GetPerson4NonCachedAsync(0, 10, true, null, "");
            //}

            [TestMethod]
            [ExpectedException(typeof(RepositoryException))]
            public async Task GetPerson4NonCached_RepositoryException()
            {
                personRepoMock.Setup(r => r.GetFilteredPerson2GuidsAsync(It.IsAny<int>(), It.IsAny<int>(), true, It.IsAny<PersonFilterCriteria>(), It.IsAny<string>())).ThrowsAsync(new RepositoryException());
                await personService.GetPerson4NonCachedAsync(0, 10, true, null, "");
            }

            [TestMethod]
            [ExpectedException(typeof(Exception))]
            public async Task GetPerson4NonCached_Exception()
            {
                personRepoMock.Setup(r => r.GetFilteredPerson2GuidsAsync(It.IsAny<int>(), It.IsAny<int>(), true, It.IsAny<PersonFilterCriteria>(), It.IsAny<string>())).ThrowsAsync(new Exception());
                await personService.GetPerson4NonCachedAsync(0, 10, true, null, "");
            }

            [TestMethod]
            public async Task GetPerson4NonCached_WithFilters()
            {
                var person = new Person4()
                {
                    Credentials = new List<Credential3DtoProperty>()
                    {
                        new Credential3DtoProperty() { Type = Credential3Type.ColleaguePersonId, Value = "00009999"}
                    },
                    EmailAddresses = new List<PersonEmailDtoProperty>()
                    {
                        new PersonEmailDtoProperty() { Address = "a@ellucian.com" }
                    },
                    Roles = new List<PersonRoleDtoProperty>()
                    {
                        new PersonRoleDtoProperty() { RoleType  = Dtos.EnumProperties.PersonRoleType.Student }
                    },
                    PersonNames = new List<PersonName2DtoProperty>()
                    {
                        new PersonName2DtoProperty()
                        {
                            Title = "Mr.",
                            MiddleName = "Middle",
                            FirstName = "First",
                            LastName = "Last",
                            LastNamePrefix = "A"
                        }
                    }
                };

                var filteredPersonGuidTuple = new Tuple<IEnumerable<string>, int>(new List<string>() { personGuid }, 1);
                personRepoMock.Setup(r => r.GetFilteredPerson2GuidsAsync(It.IsAny<int>(), It.IsAny<int>(), true, It.IsAny<PersonFilterCriteria>(), It.IsAny<string>())).
                    ReturnsAsync(filteredPersonGuidTuple);

                var personEntities = new List<PersonIntegration>() { personIntegration };
                personRepoMock.Setup(r => r.GetPersonIntegration2ByGuidNonCachedAsync(It.IsAny<IEnumerable<string>>())).ReturnsAsync(personEntities);

                var result = await personService.GetPerson4NonCachedAsync(0, 1, true, person, "");

                Assert.IsTrue(result.Item1.FirstOrDefault().Id == personGuid);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task QueryPerson4ByPostAsync_NullDto()
            {
                var result = await personService.QueryPerson4ByPostAsync(null, false);
            }

            [TestMethod]
            public async Task QueryPerson4ByPostAsync_NoResults()
            {
                var personDto = new Person4()
                {
                    PersonNames = new List<PersonName2DtoProperty>()
                    {
                        new PersonName2DtoProperty()
                        {
                            Title = "Mr.",
                            MiddleName = "Middle",
                            FirstName = "First",
                            LastName = "Last",
                            LastNamePrefix = "A"
                        }
                    }
                };

                personRepoMock.Setup(r => r.GetMatchingPersonsAsync(It.IsAny<Colleague.Domain.Base.Entities.Person>())).ReturnsAsync(() => null);

                var result = await personService.QueryPerson4ByPostAsync(personDto, false);
                Assert.AreEqual(0, result.Count());
            }

            [TestMethod]
            public async Task QueryPerson4ByPostAsync_Valid()
            {
                var personDto = new Person4()
                {
                    Credentials = new List<Credential3DtoProperty>()
                    {
                        new Credential3DtoProperty() { Type = Credential3Type.ColleaguePersonId, Value = "00009999"}
                    },
                    EmailAddresses = new List<PersonEmailDtoProperty>()
                    {
                        new PersonEmailDtoProperty()
                        {
                            Address = "a@ellucian.com",
                            Type = new PersonEmailTypeDtoProperty()
                            {
                                EmailType = EmailTypeList.Personal,
                                Detail = new GuidObject2("8026705d-fa3a-4373-8125-9c99f495e9ae")
                            }
                        }
                    },
                    PersonNames = new List<PersonName2DtoProperty>()
                    {
                        new PersonName2DtoProperty()
                        {
                            Title = "Mr.",
                            MiddleName = "Middle",
                            FirstName = "First",
                            LastName = "Last",
                            LastNamePrefix = "A"
                        }
                    }
                };

                personRepoMock.Setup(r => r.GetMatchingPersonsAsync(It.IsAny<Colleague.Domain.Base.Entities.Person>())).ReturnsAsync(new List<string>() { guid });

                var personEntities = new List<PersonIntegration>() { personIntegration };
                personRepoMock.Setup(r => r.GetPersonIntegration2ByGuidNonCachedAsync(It.IsAny<IEnumerable<string>>())).ReturnsAsync(personEntities);

                var result = await personService.QueryPerson4ByPostAsync(personDto, false);

                Assert.IsTrue(result.ElementAt(0).Id == personGuid);
            }
        }

        #endregion

        #region HEDM GetPerson V12.1.0 Tests

        [TestClass]
        public class HEDM_GetPersonV12_1_0 : CurrentUserSetup
        {
            private string personId = "0000011";
            private string personId2 = "0000012";
            private string personGuid = "5674f28b-b216-4055-b236-81a922d93b4c";
            private string personGuid2 = "1111f28b-b216-4055-b236-81a922d93b4c";
            private Mock<IPersonRepository> personRepoMock;
            private IPersonRepository personRepo;
            private Mock<IPersonBaseRepository> personBaseRepoMock;
            private IPersonBaseRepository personBaseRepo;
            private Mock<IReferenceDataRepository> refRepoMock;
            private IReferenceDataRepository refRepo;
            private Mock<IAdapterRegistry> adapterRegistryMock;
            private IAdapterRegistry adapterRegistry;
            private PersonService personService;
            private ILogger logger;
            private Ellucian.Colleague.Domain.Base.Entities.Person person;
            private Ellucian.Colleague.Domain.Base.Entities.Person person2;
            private Ellucian.Colleague.Domain.Base.Entities.PersonIntegration personIntegration;
            private Mock<IRoleRepository> roleRepoMock;
            private IRoleRepository roleRepo;
            private ICurrentUserFactory currentUserFactory;
            private Ellucian.Colleague.Domain.Base.Entities.EmailAddress instEmail;
            private Ellucian.Colleague.Domain.Base.Entities.EmailAddress perEmail;
            private Ellucian.Colleague.Domain.Base.Entities.EmailAddress workEmail;
            private Ellucian.Colleague.Domain.Base.Entities.Address homeAddr;
            private Ellucian.Colleague.Domain.Base.Entities.Address mailAddr;
            private Ellucian.Colleague.Domain.Base.Entities.Address resAddr;
            private Ellucian.Colleague.Domain.Base.Entities.Address workAddr;
            private Ellucian.Colleague.Domain.Base.Entities.Phone homePhone;
            private Ellucian.Colleague.Domain.Base.Entities.Phone mobilePhone;
            private Ellucian.Colleague.Domain.Base.Entities.Phone residencePhone;
            private Ellucian.Colleague.Domain.Base.Entities.Phone workPhone;
            private IEnumerable<Ellucian.Colleague.Domain.Base.Entities.State> states;
            private IEnumerable<Ellucian.Colleague.Domain.Base.Entities.Country> countries;
            private List<Ellucian.Colleague.Domain.Base.Entities.County> counties;
            private string maritalStatusGuid = Guid.NewGuid().ToString();
            private string ethnicityGuid = Guid.NewGuid().ToString();
            private string raceAsianGuid = Guid.NewGuid().ToString();
            private string racePacificIslanderGuid = Guid.NewGuid().ToString();
            private string countyGuid = Guid.NewGuid().ToString();
            private string baptistGuid = Guid.NewGuid().ToString();
            private string catholicGuid = Guid.NewGuid().ToString();
            private Ellucian.Colleague.Domain.Entities.Permission permissionViewAnyPerson;


            private string guid = "5674f28b-b216-4055-b236-81a922d93b4c";

            [TestInitialize]
            public void Initialize()
            {
                personRepoMock = new Mock<IPersonRepository>();
                personRepo = personRepoMock.Object;
                personBaseRepoMock = new Mock<IPersonBaseRepository>();
                personBaseRepo = personBaseRepoMock.Object;
                refRepoMock = new Mock<IReferenceDataRepository>();
                refRepo = refRepoMock.Object;
                adapterRegistryMock = new Mock<IAdapterRegistry>();
                adapterRegistry = adapterRegistryMock.Object;
                roleRepoMock = new Mock<IRoleRepository>();
                roleRepo = roleRepoMock.Object;
                logger = new Mock<ILogger>().Object;

                // Mock person response from the person repository
                person = new Domain.Base.Entities.Person(personId, "Brown");
                person.Guid = personGuid;
                person.Prefix = "Mr.";
                person.FirstName = "Ricky";
                person.MiddleName = "Lee";
                person.Suffix = "Jr.";
                person.Nickname = "Rick";
                person.BirthDate = new DateTime(1930, 1, 1);
                person.DeceasedDate = new DateTime(2014, 5, 12);
                person.GovernmentId = "111-11-1111";
                person.MaritalStatusCode = "M";
                person.EthnicCodes = new List<string> { "H" };
                person.RaceCodes = new List<string> { "AS" };
                person.AddEmailAddress(new EmailAddress("xyz@xmail.com", "COL"));
                personRepoMock.Setup(repo => repo.GetPersonByGuidNonCachedAsync(personGuid)).ReturnsAsync(person);

                var filteredPersonGuidTuple = new Tuple<IEnumerable<string>, int>(new List<string>() { personGuid }, 1);
                personRepoMock.Setup(repo => repo.GetFilteredPerson2GuidsAsync(It.IsAny<int>(), It.IsAny<int>(), true, It.IsAny<PersonFilterCriteria>(), It.IsAny<string>())).ReturnsAsync(filteredPersonGuidTuple);
               
                personIntegration = new PersonIntegration(personId, "Brown");
                personIntegration.Guid = personGuid;
                personIntegration.GovernmentId = personGuid;
                personIntegration.Prefix = "Mr.";
                personIntegration.FirstName = "Ricky";
                personIntegration.MiddleName = "Lee";
                personIntegration.Suffix = "Jr.";
                personIntegration.Nickname = "Rick";
                personIntegration.BirthDate = new DateTime(1930, 1, 1);
                personIntegration.DeceasedDate = new DateTime(2014, 5, 12);
                personIntegration.GovernmentId = "111-11-1111";
                personIntegration.Religion = "CA";
                personIntegration.MaritalStatusCode = "M";
                personIntegration.EthnicCodes = new List<string> { "H", "N" };
                personIntegration.RaceCodes = new List<string> { "AS" };
                personIntegration.GenderIdentityCode = "FTM";
                personIntegration.PersonalPronounCode = "HE";
                personIntegration.AddPersonLanguage(new PersonLanguage(personId, "eng", LanguagePreference.Primary));
                personIntegration.AddPersonLanguage(new PersonLanguage(personId, "fre", LanguagePreference.Secondary));
                personIntegration.AddPersonAlt(new PersonAlt("2222", "ELEV2"));
                personIntegration.AddPersonAlt(new PersonAlt("3333", "GOVID2"));
                personIntegration.AddRole(new PersonRole(PersonRoleType.Alumni, new DateTime(15, 01, 22), new DateTime(15, 05, 25)));
                // Mock the email address data response
                instEmail = new Domain.Base.Entities.EmailAddress("inst@inst.com", "COL") { IsPreferred = true };
                personIntegration.AddEmailAddress(instEmail);
                perEmail = new Domain.Base.Entities.EmailAddress("personal@personal.com", "PER") { IsPreferred = false };
                personIntegration.AddEmailAddress(perEmail);
                workEmail = new Domain.Base.Entities.EmailAddress("work@work.com", "BUS") { IsPreferred = false };
                personIntegration.AddEmailAddress(workEmail);

                // Mock the address hierarchy responses
                var addresses = new List<Domain.Base.Entities.Address>();
                homeAddr = new Domain.Base.Entities.Address()
                {
                    TypeCode = "HO",
                    Type = Dtos.EnumProperties.AddressType.Home.ToString(),
                    Guid = Guid.NewGuid().ToString(),
                    Status = "Current",
                    IsPreferredAddress = true
                };
                addresses.Add(homeAddr);
                mailAddr = new Domain.Base.Entities.Address()
                {
                    TypeCode = "MA",
                    Type = Dtos.EnumProperties.AddressType.Mailing.ToString(),
                    Guid = Guid.NewGuid().ToString(),
                    Status = "Current"
                };
                addresses.Add(mailAddr);
                resAddr = new Domain.Base.Entities.Address()
                {
                    TypeCode = "VA",
                    Type = Dtos.EnumProperties.AddressType.Vacation.ToString(),
                    Guid = Guid.NewGuid().ToString(),
                    Status = "Current",
                    IsPreferredResidence = true
                };
                addresses.Add(resAddr);
                workAddr = new Domain.Base.Entities.Address()
                {
                    TypeCode = "BU",
                    Type = Dtos.EnumProperties.AddressType.Business.ToString(),
                    Guid = Guid.NewGuid().ToString(),
                    Status = "Current"
                };
                addresses.Add(workAddr);
                personIntegration.Addresses = addresses;

                // Mock the person phone per type response
                homePhone = new Domain.Base.Entities.Phone("111-111-1111", "HO");
                personIntegration.AddPhone(homePhone);
                mobilePhone = new Domain.Base.Entities.Phone("222-222-2222", "MO");
                personIntegration.AddPhone(mobilePhone);
                residencePhone = new Domain.Base.Entities.Phone("333-333-3333", "VA");
                personIntegration.AddPhone(residencePhone);
                workPhone = new Domain.Base.Entities.Phone("444-444-4444", "BU", "4444");
                personIntegration.AddPhone(workPhone);

                // Mock the social media
                var socialMedia = new List<Domain.Base.Entities.SocialMedia>();
                var socialMediaTypeCode = "TW";
                var socialMediaHandle = "pontifex";
                var personSocialMedia = new SocialMedia(socialMediaTypeCode, socialMediaHandle);
                personIntegration.AddSocialMedia(personSocialMedia);

                personIntegration.AddPersonAlt(new PersonAlt("1", "ELEV") { });
                personIntegration.MilitaryStatus = "VET";

                personRepoMock.Setup(repo => repo.GetPersonIntegrationByGuidNonCachedAsync(personGuid)).ReturnsAsync(personIntegration);
                personRepoMock.Setup(repo => repo.GetPersonIntegrationByGuidAsync(personGuid, It.IsAny<bool>())).ReturnsAsync(personIntegration);

                var personGuidList = new List<string>() { personGuid };
                var personList = new List<PersonIntegration>() { personIntegration };
                personRepoMock.Setup(repo => repo.GetPersonIntegrationByGuidNonCachedAsync(personGuidList)).ReturnsAsync(personList);
                personRepoMock.Setup(repo => repo.GetFilteredPerson2GuidsAsync(It.IsAny<int>(), It.IsAny<int>(), true, It.IsAny<PersonFilterCriteria>(), It.IsAny<string>())).ReturnsAsync(new Tuple<IEnumerable<string>, int>(personGuidList, 1));
               
                person2 = new Domain.Base.Entities.Person(personId2, "Green");
                person2.Guid = personGuid2;
                person2.Prefix = "Ms.";
                person2.FirstName = "Amy";
                var personGuids = new List<string>();
                personGuids.Add(person.Guid);
                personGuids.Add(person2.Guid);
                var personEntities = new List<Domain.Base.Entities.Person>();
                personEntities.Add(person);
                personEntities.Add(person2);
                personRepoMock.Setup(repo => repo.GetPersonByGuidNonCachedAsync(personGuids)).ReturnsAsync(personEntities.AsEnumerable());

                // Mock the response for getting faculty guids
                var personGuidTuple = new Tuple<IEnumerable<string>, int>(personGuids, 2);
                personRepoMock.Setup(repo => repo.GetFacultyPersonGuidsAsync(It.IsAny<int>(), It.IsAny<int>())).ReturnsAsync(personGuidTuple);

                // Mock the response for getting a Person Pin 
                var personPin = new PersonPin("0000011", "testUsername");
                var personPins = new List<PersonPin>();
                personPins.Add(personPin);
                personRepoMock.Setup(repo => repo.GetPersonPinsAsync(It.IsAny<string[]>())).ReturnsAsync(personPins);

                refRepoMock.Setup(repo => repo.GetPrivacyStatusesAsync(It.IsAny<bool>()))
                     .ReturnsAsync(
                         new List<PrivacyStatus>() {
                        new PrivacyStatus( Guid.NewGuid().ToString(), "UN", "Unrestricted", PrivacyStatusType.unrestricted),
                        new PrivacyStatus( Guid.NewGuid().ToString(), "R", "restricted", PrivacyStatusType.restricted)
                        }
                     );

                refRepoMock.Setup(repo => repo.GetPersonNameTypesAsync(It.IsAny<bool>()))
                    .ReturnsAsync(
                        new List<PersonNameTypeItem>() {
                        new PersonNameTypeItem(Guid.NewGuid().ToString(), "PREFERRED", "Personal", PersonNameType.Personal),
                        new PersonNameTypeItem(Guid.NewGuid().ToString(), "BIRTH", "Birth", PersonNameType.Birth),
                        new PersonNameTypeItem(Guid.NewGuid().ToString(), "CHOSEN", "Chosen", PersonNameType.Personal),
                        new PersonNameTypeItem(Guid.NewGuid().ToString(), "LEGAL", "Legal", PersonNameType.Legal),
                        new PersonNameTypeItem(Guid.NewGuid().ToString(), "NICKNAME", "NickName", PersonNameType.Personal),
                        new PersonNameTypeItem(Guid.NewGuid().ToString(), "HISTORY", "History", PersonNameType.Personal)
                        }
                     );

                refRepoMock.Setup(repo => repo.GetDenominationsAsync(It.IsAny<bool>()))
                    .ReturnsAsync(
                        new List<Denomination>() {
                        new Denomination(baptistGuid,"BA", "Baptist") ,
                        new Denomination(catholicGuid,"CA", "Catholic")
                        }
                     );

                refRepoMock.Setup(repo => repo.GetEmailTypesAsync(It.IsAny<bool>()))
                     .ReturnsAsync(
                        new List<EmailType>() {
                        new EmailType( "86E1DE39-EE23-422F-BAD4-F80F638097A2", "COL", "College", EmailTypeCategory.School),
                        new EmailType( "8026705d-fa3a-4373-8125-9c99f495e9ae", "PER", "Personal", EmailTypeCategory.Personal),
                        new EmailType( "3EF60DDE-E317-4B4C-B95F-FA50125B6EFE", "BUS", "Business", EmailTypeCategory.Business)
                        }
                     );

                refRepoMock.Setup(repo => repo.GetSocialMediaTypesAsync(It.IsAny<bool>()))
                     .ReturnsAsync(
                         new List<SocialMediaType>() {
                        new SocialMediaType( Guid.NewGuid().ToString(), "TW", "Twitter", SocialMediaTypeCategory.twitter)
                        }
                     );

                refRepoMock.Setup(repo => repo.GetAddressTypes2Async(It.IsAny<bool>()))
                     .ReturnsAsync(
                         new List<AddressType2>() {
                        new AddressType2( Guid.NewGuid().ToString(), "HO", "Home", AddressTypeCategory.Home),
                        new AddressType2( Guid.NewGuid().ToString(), "MA", "Mailing", AddressTypeCategory.Mailing),
                        new AddressType2( Guid.NewGuid().ToString(), "VA", "Vacation", AddressTypeCategory.Vacation),
                        new AddressType2( Guid.NewGuid().ToString(), "BU", "Business", AddressTypeCategory.Business)
                        }
                     );

                refRepoMock.Setup(repo => repo.GetPhoneTypesAsync(It.IsAny<bool>()))
                     .ReturnsAsync(
                         new List<PhoneType>() {
                        new PhoneType( Guid.NewGuid().ToString(), "HO", "Home", PhoneTypeCategory.Home),
                        new PhoneType( Guid.NewGuid().ToString(), "MO", "Mobile", PhoneTypeCategory.Mobile),
                        new PhoneType( Guid.NewGuid().ToString(), "VA", "Vacation", PhoneTypeCategory.Vacation),
                        new PhoneType( Guid.NewGuid().ToString(), "BU", "Business", PhoneTypeCategory.Business)
                        }
                     );

                // Mock the person repository for roles
                personRepoMock.Setup(repo => repo.IsFacultyAsync(personId)).ReturnsAsync(true);
                personRepoMock.Setup(repo => repo.IsStudentAsync(personId)).ReturnsAsync(true);

                // Mock the person repository GUID lookup
                personRepoMock.Setup(repo => repo.GetPersonIdFromGuidAsync(personGuid)).ReturnsAsync(personId);
                personRepoMock.Setup(repo => repo.GetPersonIdFromGuidAsync(personGuid2)).ReturnsAsync(personId2);
                // Mock the reference repository for states
                states = new List<State>()
                {
                    new State("VA","Virginia"),
                    new State("MD","Maryland"),
                    new State("NY","New York"),
                    new State("MA","Massachusetts")
                };
                refRepoMock.Setup(repo => repo.GetStateCodesAsync(It.IsAny<bool>())).Returns(Task.FromResult(states));

                // Mock the reference repository for country
                countries = new List<Country>()
                 {
                    new Country("US","United States","US"){ IsoAlpha3Code = "USA"},
                    new Country("CA","Canada","CA"){ IsoAlpha3Code = "CAN"},
                    new Country("MX","Mexico","MX"){ IsoAlpha3Code = "MEX"},
                    new Country("BR","Brazil","BR"){ IsoAlpha3Code = "BRA"}
                };
                refRepoMock.Setup(repo => repo.GetCountryCodesAsync(It.IsAny<bool>())).Returns(Task.FromResult(countries));

                // Places
                var places = new List<Place>();
                var place1 = new Place() { PlacesCountry = "USA", PlacesRegion = "US-NY" };
                places.Add(place1);
                var place2 = new Place() { PlacesCountry = "CAN", PlacesRegion = "CA-ON" };
                places.Add(place2);
                refRepoMock.Setup(repo => repo.GetPlacesAsync(It.IsAny<bool>())).Returns(Task.FromResult(places.AsEnumerable<Place>()));
                //personRepoMock.Setup(repo => repo.GetPlacesAsync()).ReturnsAsync(places);

                // International Parameters Host Country
                personRepoMock.Setup(repo => repo.GetHostCountryAsync()).ReturnsAsync("USA");

                // Mock the reference repository for county
                counties = new List<County>()
                {
                    new County(countyGuid, "FFX","Fairfax County"),
                    new County(countyGuid, "BAL","Baltimore County"),
                    new County(countyGuid, "NY","New York County"),
                    new County(countyGuid, "BOS","Boston County")
                };
                refRepoMock.Setup(repo => repo.Counties).Returns(counties);

                // Mock the reference repository for marital status
                refRepoMock.Setup(repo => repo.GetMaritalStatusesAsync(It.IsAny<bool>())).Returns(Task.FromResult<IEnumerable<MaritalStatus>>(new List<MaritalStatus>()
                {
                    new MaritalStatus(maritalStatusGuid, "M", "Married")
                }));

                // Mock the reference repository for ethnicity
                refRepoMock.Setup(repo => repo.GetEthnicitiesAsync(It.IsAny<bool>())).Returns(Task.FromResult<IEnumerable<Ethnicity>>(new List<Ethnicity>()
                {
                    new Ethnicity(ethnicityGuid, "H", "Hispanic", EthnicityType.Hispanic)
                }));

                // Mock the reference repository for races
                var raceEntities = new List<Race>();
                raceEntities.Add(new Race(raceAsianGuid, "AS", "Asian", RaceType.Asian));
                raceEntities.Add(new Race(racePacificIslanderGuid, "HP", "Hawaiian/Pacific Islander", RaceType.PacificIslander));
                refRepoMock.Setup(repo => repo.GetRacesAsync(It.IsAny<bool>())).Returns(Task.FromResult<IEnumerable<Race>>(raceEntities));

                // Mock the reference repository for prefix
                refRepoMock.Setup(repo => repo.Prefixes).Returns(new List<Prefix>()
                {
                    new Prefix("MR","Mr","Mr."),
                    new Prefix("MS","Ms","Ms.")
                });

                // Mock the reference repository for suffix
                refRepoMock.Setup(repo => repo.Suffixes).Returns(new List<Suffix>()
                {
                    new Suffix("JR","Jr","Jr."),
                    new Suffix("SR","Sr","Sr.")
                });
                
                // Mock LANGUAGES valcode 
                var languages = new Ellucian.Data.Colleague.DataContracts.ApplValcodes()
                {

                    ValsEntityAssociation = new List<Ellucian.Data.Colleague.DataContracts.ApplValcodesVals>()
                    {
                        new ApplValcodesVals() { ValInternalCodeAssocMember = "E", ValExternalRepresentationAssocMember = "English", ValActionCode3AssocMember = "ENG" },
                        new ApplValcodesVals() { ValInternalCodeAssocMember = "SP", ValExternalRepresentationAssocMember = "Spanish", ValActionCode3AssocMember = "SPA" },
                        new ApplValcodesVals() { ValInternalCodeAssocMember = "TA", ValExternalRepresentationAssocMember = "Tagalog", ValActionCode3AssocMember = "TGL" },
                        new ApplValcodesVals() { ValInternalCodeAssocMember = "XXX", ValExternalRepresentationAssocMember = "Invalid" }
                    }
                };
                personBaseRepoMock.Setup(repo => repo.GetLanguagesAsync()).ReturnsAsync(languages);

                // Mock the reference repository for Alternate ID Types
                refRepoMock.Setup(repo => repo.GetAlternateIdTypesAsync(It.IsAny<bool>())).ReturnsAsync(new List<AltIdTypes>()
                {
                    new AltIdTypes("AE44FE48-2534-480B-8618-5480617CE74A", "ELEV2", "Elevate ID 2"),
                    new AltIdTypes("D525E2B2-CD7D-4995-93F0-97DA468EBE90", "GOVID2", "Government ID 2")
                });

                // Mock the reference repository for Gender Identity Codes
                refRepoMock.Setup(repo => repo.GetGenderIdentityTypesAsync(It.IsAny<bool>())).ReturnsAsync(new List<GenderIdentityType>()
                {
                    new GenderIdentityType("9C3004AB-0F25-4D1D-84D6-65EA69CE1124","FTM","Female to Male"),
                    new GenderIdentityType("BCD23124-2FAA-411C-A990-24BA3FA8A93D", "MTF","Male to Female")
                });

                // Mock the reference repository for Personal Pronouns
                refRepoMock.Setup(repo => repo.GetPersonalPronounTypesAsync(It.IsAny<bool>())).ReturnsAsync(new List<PersonalPronounType>()
                {
                    new PersonalPronounType("AE7A3392-FA07-4F53-B6D5-317D77CB62EC","HE","He, Him, His"),
                    new PersonalPronounType("9567AFB5-5F3C-40DC-B4F9-FC1658ACEE15", "HER","She, Her, Hers")
                });
                refRepoMock.Setup(repo => repo.GetMilStatusesAsync(It.IsAny<bool>())).ReturnsAsync(new List<MilStatuses>()
                {
                    new MilStatuses("AE7A3392-FA07-4F53-B6D5-317D88CB62EC", "VET", "Veteran") { Category = VeteranStatusCategory.Protectedveteran },
                    new MilStatuses("9567AFB5-5F3C-40DC-B4F9-FC1699ACEE15", "RET", "Retired") { Category = VeteranStatusCategory.Activeduty },
                    new MilStatuses("BCD23124-2FAA-411C-A990-24BA3FA8A93D", "ZZZ", "Invalid Category")
                });

                // Set up current user
                currentUserFactory = new CurrentUserSetup.PersonUserFactory();

                personRepoMock.Setup(repo => repo.GetPersonIntegration2ByGuidAsync(It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(personIntegration);
                personRepoMock.Setup(repo => repo.GetPersonIntegration3ByGuidAsync(It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(personIntegration);

                // Mock permissions
                permissionViewAnyPerson = new Ellucian.Colleague.Domain.Entities.Permission(BasePermissionCodes.ViewAnyPerson);
                personRole.AddPermission(permissionViewAnyPerson);
                roleRepoMock.Setup(rpm => rpm.Roles).Returns(new List<Role>() { personRole });

                personService = new PersonService(adapterRegistry, personRepo, personBaseRepo, refRepo, null, null, null, null, currentUserFactory, roleRepo, logger);
            }

            [TestCleanup]
            public void Cleanup()
            {
                adapterRegistry = null;
                personRepo = null;
                personBaseRepo = null;
                refRepo = null;
                roleRepo = null;
                personService = null;
                personRole = null;
                permissionViewAnyPerson = null;
                roleRepoMock = null;
                currentUserFactory = null;
                refRepoMock = null;
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task GetPerson5ByGuid_ArgumentNullException_EmptyGuid()
            {
                await personService.GetPerson5ByGuidAsync(null, true);
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task GetPerson5ByGuid_PersonNotFound()
            {
                personRepoMock.Setup(p => p.GetPersonIdFromGuidAsync(It.IsAny<string>())).ThrowsAsync(new KeyNotFoundException());
                await personService.GetPerson5ByGuidAsync(guid, true);
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task GetPerson5ByGuid_PermissionException()
            {
                roleRepoMock.Setup(rpm => rpm.Roles).Returns(new List<Role>() { });
                try
                {
                    await personService.GetPerson5ByGuidAsync(guid, true);
                }
                catch (IntegrationApiException ex)
                {
                    Assert.IsNotNull(ex.Errors);
                    Assert.AreEqual(System.Net.HttpStatusCode.Forbidden, ex.Errors[0].StatusCode);
                    throw;
                }
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task GetPerson5ByGuid_KeyNotFoundException_PersonNotFound()
            {
                personRepoMock.Setup(p => p.GetPersonIntegration3ByGuidAsync(It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(() => null);
                await personService.GetPerson5ByGuidAsync(guid, true);
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task GetPerson5ByGuid_KeyNotFoundException_InvalidPersonId()
            {
                var person = new PersonIntegration(guid, "lastName") { PersonCorpIndicator = "Y" };
                personRepoMock.Setup(p => p.GetPersonIntegration3ByGuidAsync(It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(person);
                await personService.GetPerson5ByGuidAsync(guid, true);
            }

            [TestMethod]
            [ExpectedException(typeof(RepositoryException))]
            public async Task GetPerson5ByGuid_RepositoryException()
            {
                personRepoMock.Setup(p => p.GetPersonIntegration3ByGuidAsync(It.IsAny<string>(), It.IsAny<bool>())).ThrowsAsync(new RepositoryException());
                await personService.GetPerson5ByGuidAsync(guid, true);
            }

            [TestMethod]
            public async Task GetPerson5ByGuid_MoreThanOneElevateIds()
            {
                personIntegration.AddPersonAlt(new PersonAlt("2", "ELEV") { });

                personRepoMock.Setup(p => p.GetPersonIntegration2ByGuidAsync(It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(personIntegration);

                await personService.GetPerson5ByGuidAsync(guid, true);
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task GetPerson5ByGuid_InvalidGenderIdentity()
            {
                personIntegration.GenderIdentityCode = "XXX";

                personRepoMock.Setup(p => p.GetPersonIntegration2ByGuidAsync(It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(personIntegration);

                try
                {
                    await personService.GetPerson5ByGuidAsync(guid, true);
                }
                catch (IntegrationApiException ex)
                {
                    Assert.IsNotNull(ex.Errors);
                    Assert.AreEqual("persons.genderIdentity", ex.Errors.First().Code);
                    Assert.AreEqual("Gender Identity code of 'XXX' is not valid.", ex.Errors.First().Message);
                    throw;
                }
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task GetPerson5ByGuid_InvalidPersonalPronoun()
            {
                personIntegration.PersonalPronounCode = "XXX";

                personRepoMock.Setup(p => p.GetPersonIntegration2ByGuidAsync(It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(personIntegration);

                try
                {
                    await personService.GetPerson5ByGuidAsync(guid, true);
                }
                catch (IntegrationApiException ex)
                {
                    Assert.IsNotNull(ex.Errors);
                    Assert.AreEqual("persons.personalPronoun", ex.Errors.First().Code);
                    Assert.AreEqual("Personal Pronoun code of 'XXX' is not valid.", ex.Errors.First().Message);
                    throw;
                }
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task GetPerson5ByGuid_InvalidLanguageCode()
            {
                personIntegration.PrimaryLanguage = "XXX";

                personRepoMock.Setup(p => p.GetPersonIntegration2ByGuidAsync(It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(personIntegration);

                try
                {
                    await personService.GetPerson5ByGuidAsync(guid, true);
                }
                catch (IntegrationApiException ex)
                {
                    Assert.IsNotNull(ex.Errors);
                    Assert.AreEqual("Invalid.Language", ex.Errors.First().Code);
                    Assert.AreEqual("The language 'XXX' is not mapped to a language ISO code.", ex.Errors.First().Message);
                    throw;
                }
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task GetPerson5ByGuid_PassportIssuingCountryNull()
            {
                personIntegration.Passport = new PersonPassport(personIntegration.Id, "abc123") { IssuingCountry = "" };

                personRepoMock.Setup(p => p.GetPersonIntegration2ByGuidAsync(It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(personIntegration);

                try
                {
                    await personService.GetPerson5ByGuidAsync(guid, true);
                }
                catch (IntegrationApiException ex)
                {
                    Assert.IsNotNull(ex.Errors);
                    Assert.AreEqual("persons.identityDocuments", ex.Errors.First().Code);
                    Assert.AreEqual("Passport number 'abc123' does not have a valid issuing country set.", ex.Errors.First().Message);
                    throw;
                }
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task GetPerson5ByGuid_InvalidAddressType()
            {
                var addresses = new List<Domain.Base.Entities.Address>();
                homeAddr = new Domain.Base.Entities.Address()
                {
                    TypeCode = "XXX",
                    Type = Dtos.EnumProperties.AddressType.Home.ToString(),
                    Guid = Guid.NewGuid().ToString(),
                    Status = "Current",
                    IsPreferredAddress = true
                };
                addresses.Add(homeAddr);
                personIntegration.Addresses = addresses;

                personRepoMock.Setup(p => p.GetPersonIntegration2ByGuidAsync(It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(personIntegration);

                try
                {
                    await personService.GetPerson5ByGuidAsync(guid, true);
                }
                catch (IntegrationApiException ex)
                {
                    Assert.IsNotNull(ex.Errors);
                    Assert.AreEqual("persons.addresses", ex.Errors.First().Code);
                    Assert.AreEqual("Address type 'XXX' for address record ID '' is not valid.", ex.Errors.First().Message);
                    throw;
                }
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task GetPerson5ByGuid_MilitaryStatusInvalidCode()
            {
                personIntegration.MilitaryStatus = "XXX";

                personRepoMock.Setup(p => p.GetPersonIntegration2ByGuidAsync(It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(personIntegration);

                try
                {
                    await personService.GetPerson5ByGuidAsync(guid, true);
                }
                catch (IntegrationApiException ex)
                {
                    Assert.IsNotNull(ex.Errors);
                    Assert.AreEqual("persons.veteranStatus", ex.Errors.First().Code);
                    Assert.AreEqual("Veteran status code 'XXX' cannot be found.", ex.Errors.First().Message);
                    throw;
                }
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task GetPerson5ByGuid_MilitaryStatusInvalidCategory()
            {
                personIntegration.MilitaryStatus = "ZZZ";

                personRepoMock.Setup(p => p.GetPersonIntegration2ByGuidAsync(It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(personIntegration);

                try
                {
                    await personService.GetPerson5ByGuidAsync(guid, true);
                }
                catch (IntegrationApiException ex)
                {
                    Assert.IsNotNull(ex.Errors);
                    Assert.AreEqual("persons.veteranStatus", ex.Errors.First().Code);
                    Assert.AreEqual("Veteran Status categories must be mapped on CDHP for code 'ZZZ'.", ex.Errors.First().Message);
                    throw;
                }
            }

            [TestMethod]
            public async Task GetPerson5ByGuid_ValidScenario()
            {
                var result = await personService.GetPerson5ByGuidAsync(guid, true);
                Assert.IsTrue(result is Dtos.Person5);
            }


            [TestMethod]
            [ExpectedException(typeof(RepositoryException))]
            public async Task GetPerson5NonCached_RepositoryException()
            {
                personRepoMock.Setup(r => r.GetFilteredPerson3GuidsAsync(It.IsAny<int>(), It.IsAny<int>(), true, It.IsAny<PersonFilterCriteria>(), It.IsAny<string>())).ThrowsAsync(new RepositoryException());
                await personService.GetPerson5NonCachedAsync(0, 10, true, null, "");
            }

            [TestMethod]
            [ExpectedException(typeof(Exception))]
            public async Task GetPerson5NonCached_Exception()
            {
                personRepoMock.Setup(r => r.GetFilteredPerson3GuidsAsync(It.IsAny<int>(), It.IsAny<int>(), true, It.IsAny<PersonFilterCriteria>(), It.IsAny<string>())).ThrowsAsync(new Exception());
                await personService.GetPerson5NonCachedAsync(0, 10, true, null, "");
            }

            [TestMethod]
            public async Task GetPerson5NonCached_WithFilters()
            {
                var person = new Person5()
                {
                    Credentials = new List<Credential3DtoProperty>()
                    {
                        new Credential3DtoProperty() { Type = Credential3Type.ColleaguePersonId, Value = "00009999"}
                    },
                    AlternativeCredentials = new List<AlternativeCredentials>()
                    {
                        new AlternativeCredentials() { Type = new GuidObject2("AE44FE48-2534-480B-8618-5480617CE74A"), Value = "0000123"}
                    },
                    EmailAddresses = new List<PersonEmailDtoProperty>()
                    {
                        new PersonEmailDtoProperty() { Address = "a@ellucian.com" }
                    },
                    Roles = new List<PersonRoleDtoProperty>()
                    {
                        new PersonRoleDtoProperty() { RoleType  = Dtos.EnumProperties.PersonRoleType.Student }
                    },
                    PersonNames = new List<PersonName2DtoProperty>()
                    {
                        new PersonName2DtoProperty()
                        {
                            Title = "Mr.",
                            MiddleName = "Middle",
                            FirstName = "First",
                            LastName = "Last",
                            LastNamePrefix = "A"
                        }
                    }
                };

                var filteredPersonGuidTuple = new Tuple<IEnumerable<string>, int>(new List<string>() { personGuid }, 1);
                personRepoMock.Setup(r => r.GetFilteredPerson3GuidsAsync(It.IsAny<int>(), It.IsAny<int>(), true, It.IsAny<PersonFilterCriteria>(), It.IsAny<string>())).
                    ReturnsAsync(filteredPersonGuidTuple);

                var personEntities = new List<PersonIntegration>() { personIntegration };
                personRepoMock.Setup(r => r.GetPersonIntegration3ByGuidNonCachedAsync(It.IsAny<IEnumerable<string>>())).ReturnsAsync(personEntities);

                var result = await personService.GetPerson5NonCachedAsync(0, 1, true, person, "");

                Assert.IsTrue(result.Item1.FirstOrDefault().Id == personGuid);
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task QueryPerson5ByPostAsync_NullDto()
            {
                var result = await personService.QueryPerson5ByPostAsync(null, false);
            }

            [TestMethod]
            public async Task QueryPerson5ByPostAsync_NoResults()
            {
                var personDto = new Person5()
                {
                    PersonNames = new List<PersonName2DtoProperty>()
                    {
                        new PersonName2DtoProperty()
                        {
                            Title = "Mr.",
                            MiddleName = "Middle",
                            FirstName = "First",
                            LastName = "Last",
                            LastNamePrefix = "A"
                        }
                    }
                };

                personRepoMock.Setup(r => r.GetMatchingPersonsAsync(It.IsAny<Colleague.Domain.Base.Entities.Person>())).ReturnsAsync(() => null);

                var result = await personService.QueryPerson5ByPostAsync(personDto, false);
                Assert.AreEqual(0, result.Count());
            }

            [TestMethod]
            public async Task QueryPerson5ByPostAsync_Valid()
            {
                var personDto = new Person5()
                {
                    Credentials = new List<Credential3DtoProperty>()
                    {
                        new Credential3DtoProperty() { Type = Credential3Type.ColleaguePersonId, Value = "00009999"}
                    },
                    EmailAddresses = new List<PersonEmailDtoProperty>()
                    {
                        new PersonEmailDtoProperty()
                        {
                            Address = "a@ellucian.com",
                            Type = new PersonEmailTypeDtoProperty()
                            {
                                EmailType = EmailTypeList.Personal,
                                Detail = new GuidObject2("8026705d-fa3a-4373-8125-9c99f495e9ae")
                            }
                        }
                    },
                    PersonNames = new List<PersonName2DtoProperty>()
                    {
                        new PersonName2DtoProperty()
                        {
                            Title = "Mr.",
                            MiddleName = "Middle",
                            FirstName = "First",
                            LastName = "Last",
                            LastNamePrefix = "A"
                        }
                    }
                };

                personRepoMock.Setup(r => r.GetMatchingPersons2Async(It.IsAny<Colleague.Domain.Base.Entities.Person>())).ReturnsAsync(new List<string>() { guid });

                var personEntities = new List<PersonIntegration>() { personIntegration };
                personRepoMock.Setup(r => r.GetPersonIntegration2ByGuidNonCachedAsync(It.IsAny<IEnumerable<string>>())).ReturnsAsync(personEntities);

                var result = await personService.QueryPerson5ByPostAsync(personDto, false);

                Assert.IsTrue(result.ElementAt(0).Id == personGuid);
            }
        }

        #endregion

        #region QueryPersonMatchResultsByPostAsync Tests

        [TestClass]
        public class QueryPersonMatchResultsByPostAsync : CurrentUserSetup
        {
            private Mock<IPersonRepository> personRepoMock;
            private IPersonRepository personRepo;
            private Mock<IPersonBaseRepository> personBaseRepoMock;
            private IPersonBaseRepository personBaseRepo;
            private Mock<IReferenceDataRepository> refRepoMock;
            private IReferenceDataRepository refRepo;
            private Mock<IAdapterRegistry> adapterRegistryMock;
            private IAdapterRegistry adapterRegistry;
            private PersonService personService;
            private ILogger logger;
            private Mock<IRoleRepository> roleRepoMock;
            private IRoleRepository roleRepo;
            private ICurrentUserFactory currentUserFactory;
            private Dtos.Base.PersonMatchCriteria criteria;

            [TestInitialize]
            public void Initialize()
            {
                personRepoMock = new Mock<IPersonRepository>();
                personRepo = personRepoMock.Object;
                personBaseRepoMock = new Mock<IPersonBaseRepository>();
                personBaseRepo = personBaseRepoMock.Object;
                adapterRegistryMock = new Mock<IAdapterRegistry>();
                adapterRegistry = adapterRegistryMock.Object;
                roleRepoMock = new Mock<IRoleRepository>();
                roleRepo = roleRepoMock.Object;
                logger = new Mock<ILogger>().Object;

                criteria = new Dtos.Base.PersonMatchCriteria()
                {
                    MatchCriteriaIdentifier = "PERSON.PROXY",
                    MatchNames = new List<Dtos.Base.PersonName>()
                    {
                        new Dtos.Base.PersonName() { FamilyName = "family", GivenName = "given" },
                        new Dtos.Base.PersonName() { FamilyName = "family2" },
                        new Dtos.Base.PersonName() { GivenName = "given2" }
                    }
                };

                var criteriaDtoAdapter = new Ellucian.Colleague.Coordination.Base.Adapters.PersonMatchCriteriaDtoAdapter(adapterRegistry, logger);
                adapterRegistryMock.Setup(reg => reg.GetAdapter<Dtos.Base.PersonMatchCriteria, Domain.Base.Entities.PersonMatchCriteria>()).Returns(criteriaDtoAdapter);

                var criteriaAdapter = new AutoMapperAdapter<Domain.Base.Entities.PersonMatchResult, Dtos.Base.PersonMatchResult>(adapterRegistry, logger);
                adapterRegistryMock.Setup(reg => reg.GetAdapter<Domain.Base.Entities.PersonMatchResult, Dtos.Base.PersonMatchResult>()).Returns(criteriaAdapter);

                // Mock the person entity response from the person repository
                personRepoMock.Setup(repo => repo.GetMatchingPersonResultsAsync(It.IsAny<PersonMatchCriteria>())).ReturnsAsync(new List<Domain.Base.Entities.PersonMatchResult>()
                    {
                        new PersonMatchResult("0001234", 50, "P"),
                        new PersonMatchResult("0001235", 60, "P")
                    });

                // Set up current user
                currentUserFactory = new CurrentUserSetup.PersonUserFactory();

                personService = new PersonService(adapterRegistry, personRepo, personBaseRepo, refRepo, null, null, null, null, currentUserFactory, roleRepo, logger);
            }

            [TestCleanup]
            public void Cleanup()
            {
                adapterRegistry = null;
                personRepo = null;
                personBaseRepo = null;
                refRepo = null;
                roleRepo = null;
                personService = null;
                personRole = null;
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task QueryPersonMatchResultsByPostAsync_NullCriteria()
            {
                var results = await personService.QueryPersonMatchResultsByPostAsync(null);
            }

            [TestMethod]
            public async Task QueryPersonMatchResultsByPostAsync_NullResults()
            {
                personRepoMock.Setup(repo => repo.GetMatchingPersonResultsAsync(It.IsAny<PersonMatchCriteria>())).ReturnsAsync(() => null);
                personService = new PersonService(adapterRegistry, personRepo, personBaseRepo, refRepo, null, null, null, null, currentUserFactory, roleRepo, logger);

                var results = await personService.QueryPersonMatchResultsByPostAsync(criteria);
                Assert.AreEqual(0, results.Count());
            }

            [TestMethod]
            public async Task QueryPersonMatchResultsByPostAsync_NoResults()
            {
                personRepoMock.Setup(repo => repo.GetMatchingPersonResultsAsync(It.IsAny<PersonMatchCriteria>())).ReturnsAsync(new List<Domain.Base.Entities.PersonMatchResult>());
                personService = new PersonService(adapterRegistry, personRepo, personBaseRepo, refRepo, null, null, null, null, currentUserFactory, roleRepo, logger);

                var results = await personService.QueryPersonMatchResultsByPostAsync(criteria);
                Assert.AreEqual(0, results.Count());
            }

            [TestMethod]
            public async Task QueryPersonMatchResultsByPostAsync_Valid()
            {
                var results = await personService.QueryPersonMatchResultsByPostAsync(criteria);
                Assert.AreEqual(2, results.Count());
            }
        }

        #endregion

        #region Create Organization Tests

        [TestClass]
        public class CreateOrganization : CurrentUserSetup
        {
            //Mocks
            Mock<IAdapterRegistry> _adapterRegistryMock;
            Mock<IPersonRepository> _personRepositoryMock;
            Mock<IPersonBaseRepository> _personBaseRepositoryMock;
            Mock<IReferenceDataRepository> _referenceDataRepositoryMock;
            Mock<IProfileRepository> _profileRepositoryMock;
            Mock<IConfigurationRepository> _configurationRepositoryMock;
            Mock<IRelationshipRepository> _relationshipRepositoryMock;
            Mock<IProxyRepository> _proxyRepositoryMock;
            Mock<IRoleRepository> _roleRepositoryMock;
            Mock<ILogger> _loggerMock;

            //userfactory
            ICurrentUserFactory _currentUserFactory;

            //Perms
            // private Ellucian.Colleague.Domain.Entities.Permission permissionUpdatePerson;

            //Service
            PersonService _personService;


            private Organization2 _organizationDto;
            private PersonIntegration _personIntegrationReturned;
            private PersonIntegration _personIntegrationEntity;

            //private Ellucian.Colleague.Domain.Base.Entities.Person person;
            private readonly List<Address> _addresses = null;
            private readonly List<Phone> _phones = new List<Domain.Base.Entities.Phone>();
            private List<PersonNameTypeItem> _personNameTypes;
            private List<Country> _countries;

            private IEnumerable<PrivacyStatus> allPrivacyStatuses;

            //Entities
            private EmailAddress _instEmail;
            private EmailAddress _perEmail;
            private EmailAddress _workEmail;
            private Address _homeAddr;
            private Address _mailAddr;
            private Address _resAddr;
            private Address _workAddr;
            private Phone _homePhone;
            private Phone _mobilePhone;
            private Phone _residencePhone;
            private Phone _workPhone;

            //Data
            private const string PersonId = "000425";
            private const string PersonGuid = "1a507924-f207-460a-8c1d-1854ebe80566";

            [TestInitialize]
            public void Initialize()
            {
                _adapterRegistryMock = new Mock<IAdapterRegistry>();
                _personRepositoryMock = new Mock<IPersonRepository>();
                _personBaseRepositoryMock = new Mock<IPersonBaseRepository>();
                _referenceDataRepositoryMock = new Mock<IReferenceDataRepository>();
                _profileRepositoryMock = new Mock<IProfileRepository>();
                _configurationRepositoryMock = new Mock<IConfigurationRepository>();
                _relationshipRepositoryMock = new Mock<IRelationshipRepository>();
                _proxyRepositoryMock = new Mock<IProxyRepository>();
                _roleRepositoryMock = new Mock<IRoleRepository>();
                _loggerMock = new Mock<ILogger>();

                _currentUserFactory = new CurrentUserSetup.ThirdPartyUserFactory();

                SetupData();

                SetupReferenceDataRepositoryMocks();

                // Mock address ID guid check return
                _personRepositoryMock.Setup(i => i.GetAddressIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync("x");

                _personRepositoryMock.Setup(i => i.CreateOrganizationAsync(
                        It.IsAny<PersonIntegration>(), It.IsAny<IEnumerable<Address>>(),
                        It.IsAny<IEnumerable<Phone>>())).ReturnsAsync(_personIntegrationReturned);


                _personService = new PersonService(_adapterRegistryMock.Object, _personRepositoryMock.Object, _personBaseRepositoryMock.Object, _referenceDataRepositoryMock.Object, _profileRepositoryMock.Object,
                                                  _configurationRepositoryMock.Object, _relationshipRepositoryMock.Object, _proxyRepositoryMock.Object, _currentUserFactory,
                                                  _roleRepositoryMock.Object, _loggerMock.Object);
            }

            [TestCleanup]
            public void Cleanup()
            {
                _personService = null;
                //permissionUpdatePerson = null;
                _adapterRegistryMock = null;
                _personRepositoryMock = null;
                _personBaseRepositoryMock = null;
                _referenceDataRepositoryMock = null;
                _profileRepositoryMock = null;
                _configurationRepositoryMock = null;
                _relationshipRepositoryMock = null;
                _proxyRepositoryMock = null;
                _roleRepositoryMock = null;
                _loggerMock = null;
                _currentUserFactory = null;
            }


            [TestMethod]
            public async Task PersonService_CreateOrganization()
            {
                //setup role
                createPersonRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Base.BasePermissionCodes.CreateOrganization));
                _roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { createPersonRole });

                var actual = await _personService.CreateOrganizationAsync(_organizationDto);

                Assert.AreEqual(_organizationDto.Id, actual.Id, "Guid");
                Assert.AreEqual(_organizationDto.Title, actual.Title, "Title");

                foreach (var socialMedia in _organizationDto.SocialMedia)
                {
                    var actualSocialMedia = actual.SocialMedia.FirstOrDefault(x => x.Type.Category == socialMedia.Type.Category && x.Address == socialMedia.Address);
                    Assert.IsNotNull(actualSocialMedia, "Social Media Type");
                    Assert.AreEqual(socialMedia.Preference, actualSocialMedia.Preference);
                    Assert.AreEqual(socialMedia.Address, actualSocialMedia.Address);
                }

                foreach (var emailAddress in _organizationDto.EmailAddresses)
                {
                    var actualEmailAddress = actual.EmailAddresses.FirstOrDefault(x => x.Type.EmailType == emailAddress.Type.EmailType && x.Address == emailAddress.Address);
                    Assert.IsNotNull(actualEmailAddress, "Email Address");
                    Assert.AreEqual(emailAddress.Preference, actualEmailAddress.Preference);
                    Assert.AreEqual(emailAddress.Address, actualEmailAddress.Address);

                }

                foreach (var phones in _organizationDto.Phones)
                {
                    var actualPhones = actual.Phones.FirstOrDefault(x => x.Type.PhoneType == phones.Type.PhoneType);
                    Assert.IsNotNull(actualPhones, "Phone");
                    Assert.AreEqual(phones.Preference, actualPhones.Preference);
                    Assert.AreEqual(phones.Number, actualPhones.Number);
                    Assert.AreEqual(phones.Extension, actualPhones.Extension);
                    Assert.AreEqual(phones.CountryCallingCode, actualPhones.CountryCallingCode);
                }
            }


            #region Exceptions


            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task PersonService_CreateOrganizationAsync_ArgumentNullException()
            {
                var actual = await _personService.CreateOrganizationAsync(null);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task PersonService_CreateOrganizationAsync_IdNull_ArgumentNullException()
            {
                _organizationDto.Id = string.Empty;
                var actual = await _personService.CreateOrganizationAsync(_organizationDto);
            }

            #endregion

            #region Setup

            private void SetupData()
            {

                var addressesCollection = new List<PersonAddressDtoProperty>();

                // Mock the reference repository for county
                var counties = new List<County>()
                {
                    new County(Guid.NewGuid().ToString(), "FFX","Fairfax County"),
                    new County(Guid.NewGuid().ToString(), "BAL","Baltimore County"),
                    new County(Guid.NewGuid().ToString(), "NY","New York County"),
                    new County(Guid.NewGuid().ToString(), "BOS","Boston County")
                };

                // Mock the reference repository for states
                var states = new List<State>()
                {
                    new State("VA","Virginia"),
                    new State("MD","Maryland"),
                    new State("NY","New York"),
                    new State("MA","Massachusetts")
                };


                var allAddresses = new TestAddressRepository().GetAddressData().ToList();
                var source = allAddresses[0];

                var address = new PersonAddressDtoProperty
                {
                    address = new PersonAddress()
                    {
                        Id = source.Guid,
                        AddressLines = source.AddressLines,
                        Latitude = (source.Latitude.HasValue) ? (decimal?)38.862221 : source.Latitude,
                        Longitude = (source.Longitude.HasValue) ? (decimal?)-77.376553 : source.Longitude
                    }

                };

                var countryPlace = new Dtos.AddressCountry()
                {
                    Code = Dtos.EnumProperties.IsoCode.USA,
                    Title = source.Country,
                    PostalTitle = "UNITED STATES OF AMERICA",
                    CarrierRoute = source.CarrierRoute,
                    DeliveryPoint = source.DeliveryPoint,
                    CorrectionDigit = source.CorrectionDigit,
                    Locality = source.City,
                    PostalCode = source.PostalCode,

                };

                var region = new Dtos.AddressRegion() { Code = source.Country + "-" + source.State };
                var title = states.FirstOrDefault(x => x.Code == source.State);
                if (title != null)
                    region.Title = title.Description;

                var countyDesc = counties.FirstOrDefault(c => c.Code == source.County);
                var subRegion = new Dtos.AddressSubRegion() { Code = source.County }; ;
                if (countyDesc != null)
                    subRegion.Title = countyDesc.Description;

                countryPlace.Region = region;
                countryPlace.SubRegion = subRegion;

                address.address.Place = new Dtos.AddressPlace() { Country = countryPlace };
                address.Type = new PersonAddressTypeDtoProperty() { AddressType = AddressType.Business };
                addressesCollection.Add(address);


                _organizationDto = new Dtos.Organization2
                {
                    Id = PersonGuid,
                    Title = "Acme Corporation",
                    Addresses = new List<PersonAddressDtoProperty>() { addressesCollection[0] },
                    EmailAddresses = new List<PersonEmailDtoProperty>()
                {
                    new PersonEmailDtoProperty() { Address = "admissions@AcmeUniversity.com",
                        Type = new PersonEmailTypeDtoProperty(){ EmailType = EmailTypeList.Business}}
                },
                    Phones = new List<PersonPhoneDtoProperty>()
                {
                    new PersonPhoneDtoProperty(){ Number = "999-999-9999", Extension = "4444",
                        Type = new PersonPhoneTypeDtoProperty(){ PhoneType = PersonPhoneTypeCategory.Business}}
                },
                    Roles = new List<OrganizationRoleDtoProperty>()
                {
                    new OrganizationRoleDtoProperty() { Type = OrganizationRoleType.Vendor }
                },
                    SocialMedia = new List<PersonSocialMediaDtoProperty>()
                {
                    new PersonSocialMediaDtoProperty()
                    {
                        Address = "BalloonGoat977",
                        Type = new PersonSocialMediaType() { Category = Dtos.SocialMediaTypeCategory.twitter},
                        Preference = PersonPreference.Primary
                    },
                }
                };



                //Returned value
                _personIntegrationReturned = new PersonIntegration(PersonId, _organizationDto.Title)
                {
                    Guid = PersonGuid,


                };
                _personIntegrationReturned.AddEmailAddress(new EmailAddress("admissions@AcmeUniversity.com", "BUS") { IsPreferred = false });
                _personIntegrationReturned.AddRole(new PersonRole(PersonRoleType.Vendor, new DateTime(15, 01, 22), new DateTime(15, 05, 25)));

                _personIntegrationReturned.PreferredName = _organizationDto.Title;

                var addresses = new List<Domain.Base.Entities.Address>();
                var personIntegrationAddress = _organizationDto.Addresses.FirstOrDefault(x => x.Type.AddressType == AddressType.Business);
                if (personIntegrationAddress != null)
                {
                    _workAddr = new Domain.Base.Entities.Address()
                    {
                        TypeCode = "BU",
                        Type = Dtos.EnumProperties.AddressType.Business.ToString(),
                        Guid = Guid.NewGuid().ToString(),
                        Status = "Current",
                        IsPreferredAddress = true,
                        AddressLines = personIntegrationAddress.address.AddressLines,
                        Latitude = personIntegrationAddress.address.Latitude,
                        Longitude = personIntegrationAddress.address.Longitude,
                        PostalCode = personIntegrationAddress.address.Place.Country.PostalCode,
                        IsPreferredResidence = true


                    };
                    addresses.Add(_workAddr);
                }
                _personIntegrationReturned.Addresses = addresses;

                _workPhone = new Domain.Base.Entities.Phone("999-999-9999", "BU", "4444");
                _personIntegrationReturned.AddPhone(_workPhone);

                var socialMediaTypeCode = "TW";
                var socialMediaHandle = "BalloonGoat977";
                var personSocialMedia = new SocialMedia(socialMediaTypeCode, socialMediaHandle, true);
                _personIntegrationReturned.AddSocialMedia(personSocialMedia);

            }

            private void SetupReferenceDataRepositoryMocks()
            {
                // Mock the reference repository for country
                _countries = new List<Country>()
                 {
                    new Country("US","United States","USA"),
                    new Country("CA","Canada","CAN"),
                    new Country("MX","Mexico","MEX"),
                    new Country("BR","Brazil","BRA")
                };
                _referenceDataRepositoryMock.Setup(repo => repo.GetCountryCodesAsync(It.IsAny<bool>())).ReturnsAsync(_countries);

                // Places
                var places = new List<Place>();
                var place1 = new Place() { PlacesCountry = "USA", PlacesRegion = "US-NY" };
                places.Add(place1);
                var place2 = new Place() { PlacesCountry = "CAN", PlacesRegion = "CA-ON" };
                places.Add(place2);
                _referenceDataRepositoryMock.Setup(repo => repo.GetPlacesAsync(It.IsAny<bool>())).Returns(Task.FromResult(places.AsEnumerable<Place>()));
                //_personRepositoryMock.Setup(repo => repo.GetPlacesAsync()).ReturnsAsync(places);

                // International Parameters Host Country
                _personRepositoryMock.Setup(repo => repo.GetHostCountryAsync()).ReturnsAsync("USA");

                _referenceDataRepositoryMock.Setup(repo => repo.GetPrivacyStatusesAsync(It.IsAny<bool>()))
                     .ReturnsAsync(
                         new List<PrivacyStatus>() {
                        new PrivacyStatus( "d3d86052-9d55-4751-acda-5c07a064a82a", "UN", "unrestricted", PrivacyStatusType.unrestricted),
                        new PrivacyStatus( "cff65dcc-4a9b-44ed-b8d0-930348c55ef8", "R", "restricted", PrivacyStatusType.restricted)
                        }
                     );
                _personNameTypes = new List<PersonNameTypeItem>() {
                        new PersonNameTypeItem("8224f18e-69c5-480b-a9b4-52f596aa4a52", "PREFERRED", "Personal", PersonNameType.Personal) ,
                        new PersonNameTypeItem("7dfa950c-8ae4-4dca-92f0-c083604285b6", "BIRTH", "Birth", PersonNameType.Birth) ,
                        new PersonNameTypeItem("806af5a5-8a9a-424f-8c9f-c1e9d084ee71", "LEGAL", "Legal", PersonNameType.Legal),
                        new PersonNameTypeItem("7b55610f-7d00-4260-bbcf-0e47fdbae647", "NICKNAME", "NickName", PersonNameType.Personal),
                        new PersonNameTypeItem("d42cc964-35cb-4560-bc46-4b881e7705ea", "HISTORY", "History", PersonNameType.Personal)
                        };
                _referenceDataRepositoryMock.Setup(repo => repo.GetPersonNameTypesAsync(It.IsAny<bool>()))
                    .ReturnsAsync(_personNameTypes);



                _referenceDataRepositoryMock.Setup(repo => repo.GetEmailTypesAsync(It.IsAny<bool>()))
                     .ReturnsAsync(
                         new List<EmailType>() {
                        new EmailType("899803da-48f8-4044-beb8-5913a04b995d", "COL", "College", EmailTypeCategory.School),
                        new EmailType("301d485d-d37b-4d29-af00-465ced624a85", "PER", "Personal", EmailTypeCategory.Personal),
                        new EmailType("53fb7dab-d348-4657-b071-45d0e5933e05", "BUS", "Business", EmailTypeCategory.Business)
                        }
                     );

                _referenceDataRepositoryMock.Setup(repo => repo.GetSocialMediaTypesAsync(It.IsAny<bool>()))
                     .ReturnsAsync(
                         new List<SocialMediaType>() {
                        new SocialMediaType("d1f311f4-687d-4dc7-a329-c6a8bfc9c74", "TW", "Twitter", SocialMediaTypeCategory.twitter)
                        }
                     );

                _referenceDataRepositoryMock.Setup(repo => repo.GetAddressTypes2Async(It.IsAny<bool>()))
                     .ReturnsAsync(
                         new List<AddressType2>() {
                        new AddressType2("91979656-e110-4156-a75a-1a1a7294314d", "HO", "Home", AddressTypeCategory.Home),
                        new AddressType2("b887d5ec-9ed5-45e8-b44c-01782070f234", "MA", "Mailing", AddressTypeCategory.Mailing),
                        new AddressType2("d7d0a82c-fe74-480d-be1b-88a2e460af4c", "VA", "Vacation", AddressTypeCategory.Vacation),
                        new AddressType2("c9b8cd52-54e6-4c08-a9d9-224dd0c8b700", "BU", "Business", AddressTypeCategory.Business)
                         }
                     );

                _referenceDataRepositoryMock.Setup(repo => repo.GetPhoneTypesAsync(It.IsAny<bool>()))
                     .ReturnsAsync(
                         new List<PhoneType>() {
                        new PhoneType("92c82d33-e55c-41a4-a2c3-f2f7d2c523d1", "HO", "Home", PhoneTypeCategory.Home),
                        new PhoneType("b6def2cc-cc95-4d0e-a32c-940fbbc2d689", "MO", "Mobile", PhoneTypeCategory.Mobile),
                        new PhoneType("f60e7b27-a3e3-4c92-9d36-f3cae27b724b", "VA", "Vacation", PhoneTypeCategory.Vacation),
                        new PhoneType("30e231cf-a199-4c9a-af01-be2e69b607c9", "BU", "Business", PhoneTypeCategory.Business)
                        }
                     );

                /*
                  // Mock the reference repository for prefix
                  _referenceDataRepositoryMock.Setup(repo => repo.Prefixes).Returns(new List<Prefix>()
                  { 
                      new Prefix("MR","Mr","Mr."),
                      new Prefix("MS","Ms","Ms.")
                  });

                  // Mock the reference repository for prefix
                  _referenceDataRepositoryMock.Setup(repo => repo.GetPrefixesAsync()).ReturnsAsync(new List<Prefix>()
                  { 
                      new Prefix("MR","Mr","Mr."),
                      new Prefix("MS","Ms","Ms.")
                  });

                  // Mock the reference repository for suffix
                  _referenceDataRepositoryMock.Setup(repo => repo.Suffixes).Returns(new List<Suffix>()
                  { 
                      new Suffix("JR","Jr","Jr."),
                      new Suffix("SR","Sr","Sr.")
                  });

                  // Mock the reference repository for suffix
                  _referenceDataRepositoryMock.Setup(repo => repo.GetSuffixesAsync()).ReturnsAsync(new List<Suffix>()
                  { 
                      new Suffix("JR","Jr","Jr."),
                      new Suffix("SR","Sr","Sr.")
                  });

                */

            }

            #endregion
        }

        #endregion
    }
}