//Copyright 2019 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Colleague.Coordination.ColleagueFinance.Services;
using Ellucian.Colleague.Data.ColleagueFinance.DataContracts;
using Ellucian.Colleague.Domain.Base.Entities;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.ColleagueFinance;
using Ellucian.Colleague.Domain.ColleagueFinance.Entities;
using Ellucian.Colleague.Domain.ColleagueFinance.Repositories;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Colleague.Dtos;
using Ellucian.Colleague.Dtos.DtoProperties;
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

namespace Ellucian.Colleague.Coordination.ColleagueFinance.Tests.Services
{
    public class VendorContactsServiceTests
    {
        public abstract class CurrentUserSetup
        {
            protected Ellucian.Colleague.Domain.Entities.Role processVendorContactRole = new Ellucian.Colleague.Domain.Entities.Role(1, "PROCESS.VENDOR.CONTACT");
            protected Ellucian.Colleague.Domain.Entities.Role viewVendorContactRole = new Ellucian.Colleague.Domain.Entities.Role(1, "VIEW.VENDOR.CONTACT");

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
                            Roles = new List<string>() { "VIEW.VENDOR.CONTACT", "PROCESS.VENDOR.CONTACT" },
                            SessionFixationId = "abc123"
                        });
                    }
                }
            }
        }
        /// <summary>
        /// Tests the vendor-contacts service layer
        /// </summary>
        [TestClass]
        public class VendorContactsServiceTest : CurrentUserSetup
        {
            private Mock<IPersonMatchingRequestsRepository> _personMatchingRequestsRepositoryMock;
            private Mock<IPersonRepository> _personRepositoryMock;
            private Mock<IReferenceDataRepository> _referenceDataRepositoryMock;
            private Mock<IVendorContactsRepository> _vendorContactsRepositoryMock;
            private Mock<IVendorsRepository> _vendorsRepositoryMock;


            private Mock<IAdapterRegistry> _adapterRegistryMock;
            private Mock<IRoleRepository> _roleRepositoryMock;
            private Mock<IConfigurationRepository> _configurationRepoMock;
            private Mock<ILogger> _loggerMock;

            private IPersonMatchingRequestsRepository _personMatchingRequestsRepository;
            private IPersonRepository _personRepository;
            private IReferenceDataRepository _referenceDataRepository;
            private IVendorContactsRepository _vendorContactsRepository;
            private IVendorsRepository _vendorsRepository;

            private IAdapterRegistry _adapterRegistry;
            private IRoleRepository _roleRepository;
            private IConfigurationRepository _configurationRepository;
            private ILogger logger;
            private ICurrentUserFactory userFactory;

            private IVendorContactsService _vendorContactsRequestsService;

            private Domain.Entities.Role _roleUPdate = new Domain.Entities.Role(2, "PROCESS.VENDOR.CONTACT");
            private Domain.Entities.Role _roleView = new Domain.Entities.Role(1, "VIEW.VENDOR.CONTACT");

            private IEnumerable<PersonMatchRequest> personMatchingRequestEntities;
            private Dictionary<string, string> personDict;
            private Dictionary<string, string> vendorDict;
            private PersonMatchRequest personMatch1;
            private PersonMatchRequest personMatch2;
            private PersonMatchRequest personMatch3;
            private Dtos.VendorContacts _vendorContactReturnInitDto;


            private VendorContactInitiationProcess vendorContactInitiationProcess;
            private PersonMatchingRequests personMatchDto1;
            private Domain.ColleagueFinance.Entities.OrganizationContact organizationContact;


            private Tuple<IEnumerable<Ellucian.Colleague.Domain.ColleagueFinance.Entities.OrganizationContact>, int> organizationContactTuple;
            private List<Ellucian.Colleague.Domain.ColleagueFinance.Entities.OrganizationContact> organizationContactCollection;
            private List<Domain.Base.Entities.RelationType> relationTypesCollection;
            private List<Domain.Base.Entities.PhoneType> phoneTypesCollection;
            private List<Domain.Base.Entities.EmailType> emailTypesCollection;

            private int offset = 0, limit = 100;

            [TestInitialize]
            public void Initialize()
            {
                _vendorsRepositoryMock = new Mock<IVendorsRepository>();
                _vendorsRepository = _vendorsRepositoryMock.Object;

                _vendorContactsRepositoryMock = new Mock<IVendorContactsRepository>();
                _vendorContactsRepository = _vendorContactsRepositoryMock.Object;


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


                _vendorContactsRequestsService = new VendorContactsService(_referenceDataRepository, _vendorContactsRepository, _vendorsRepository, _personRepository, _personMatchingRequestsRepository,
                                                                                _adapterRegistry, userFactory, _roleRepository,
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

                foreach (var type in relationTypesCollection)
                {
                    _referenceDataRepositoryMock.Setup(repo => repo.GetRelationTypes3GuidAsync(type.Code)).ReturnsAsync(new Tuple<string, string>(type.Guid, type.Guid));
                }
                _referenceDataRepositoryMock.Setup(repo => repo.GetRelationTypes3Async(It.IsAny<bool>())).ReturnsAsync(relationTypesCollection);

                foreach (var type in phoneTypesCollection)
                {
                    _referenceDataRepositoryMock.Setup(repo => repo.GetPhoneTypesGuidAsync(type.Code)).ReturnsAsync(type.Guid);
                }
                _referenceDataRepositoryMock.Setup(repo => repo.GetPhoneTypesAsync(It.IsAny<bool>())).ReturnsAsync(phoneTypesCollection);


                foreach (var type in emailTypesCollection)
                {
                    _referenceDataRepositoryMock.Setup(repo => repo.GetEmailTypesGuidAsync(type.Code)).ReturnsAsync(type.Guid);
                }
                _referenceDataRepositoryMock.Setup(repo => repo.GetEmailTypesAsync(It.IsAny<bool>())).ReturnsAsync(emailTypesCollection);


                var viewPermission = new Ellucian.Colleague.Domain.Entities.Permission(ColleagueFinancePermissionCodes.ViewVendorContacts);
                processVendorContactRole.AddPermission(viewPermission);
                var processPermission = new Ellucian.Colleague.Domain.Entities.Permission(ColleagueFinancePermissionCodes.ProcessVendorContact);
                processVendorContactRole.AddPermission(processPermission);

                _roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { processVendorContactRole });


                _vendorContactsRepositoryMock.Setup(pmr => pmr.GetVendorContactsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>())).ReturnsAsync(organizationContactTuple);

               
                _vendorsRepositoryMock.Setup(vrm => vrm.GetVendorGuidsCollectionAsync(It.IsAny<string[]>())).ReturnsAsync(vendorDict);

                _personRepositoryMock.Setup(p => p.IsCorpAsync(It.IsAny<string>())).ReturnsAsync(false);
            }

            private void InitializeData()
            {
                relationTypesCollection = new List<Domain.Base.Entities.RelationType>()
                {
                    new Domain.Base.Entities.RelationType("7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc", "PAR", "Parent", "1", "PA"),
                    new Domain.Base.Entities.RelationType("849e6a7c-6cd4-4f98-8a73-ab0aa3627f0d", "CHD", "Child", "1", "CD"),
                    new Domain.Base.Entities.RelationType("d2253ac7-9931-4560-b42f-1fccd43c952e", "SIB", "Sibling", "1", "SB")
                };

                phoneTypesCollection = new List<Domain.Base.Entities.PhoneType>()
                {
                    new Domain.Base.Entities.PhoneType(Guid.NewGuid().ToString(), "HOME", "Home", PhoneTypeCategory.Home),
                    new Domain.Base.Entities.PhoneType(Guid.NewGuid().ToString(), "BUS", "Business", PhoneTypeCategory.Business),
                };

                var emailTypeGuid = Guid.NewGuid().ToString();
                emailTypesCollection = new List<Domain.Base.Entities.EmailType>()
                {
                    new Domain.Base.Entities.EmailType(Guid.NewGuid().ToString(), "HOME", "Home", EmailTypeCategory.Personal),
                    new Domain.Base.Entities.EmailType(Guid.NewGuid().ToString(), "BUS", "Business", EmailTypeCategory.Business),
                };

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

                vendorDict = new Dictionary<string, string>();
                vendorDict.Add(personMatch1.PersonId, "6518d26e-ab8d-4aa0-95f0-f415fa7c0004");
                vendorDict.Add(personMatch2.PersonId, "6518d26e-ab8d-4aa0-95f0-f415fa7c0005");
                vendorDict.Add(personMatch3.PersonId, "6518d26e-ab8d-4aa0-95f0-f415fa7c0006");                

                personMatchDto1 = new PersonMatchingRequests()
                {
                    Id = Guid.NewGuid().ToString(),
                    Originator = "ELEVATE",
                    Person = new GuidObject2("6518d26e-ab8d-4aa0-95f0-f415fa7c0001"),
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


                organizationContactCollection = new List<Domain.ColleagueFinance.Entities.OrganizationContact>();

                var organizationContact1 = new Domain.ColleagueFinance.Entities.OrganizationContact(Guid.NewGuid().ToString(), "1")
                {
                    ContactAddress = "Address1",
                    ContactPersonGuid = "6518d26e-ab8d-4aa0-95f0-f415fa7c0001",
                    ContactPersonId = "1",
                    ContactPreferedName = "Pref1",
                    EndDate = new DateTime(2020, 1, 1),
                    PhoneInfos = new List<Domain.ColleagueFinance.Entities.ContactPhoneInfo>()
                    {
                        new Domain.ColleagueFinance.Entities.ContactPhoneInfo() { PhoneType = "HOME", PhoneExtension = "123", PhoneNumber = "1234567"}

                    },                
                    RelationshipType = "PAR",
                    StartDate = new DateTime(2019, 1, 1),
                    VendorId = "1"
                };

                organizationContactCollection.Add(organizationContact1);

                organizationContactTuple = new Tuple<IEnumerable<Domain.ColleagueFinance.Entities.OrganizationContact>, int>
                (organizationContactCollection, organizationContactCollection.Count());

                #region vendor-contact-initiation-process

                _vendorContactReturnInitDto = new VendorContacts()
                {
                    Id = "a830e686-7692-4012-8da5-b1b5d44389b4",
                    Vendor = new GuidObject2( "venguid3" ),
                    Contact = new Dtos.DtoProperties.VendorContactsContact()
                    {
                        EndOn = DateTime.Now.AddDays( 5 ),
                        Person = new Dtos.DtoProperties.VendorContactsPerson()
                        {
                            Detail = new GuidObject2( "b830e686-7692-4012-8da5-b1b5d44389b5" ),
                            Name = "Lastname M Firstname"
                        },
                        Phones = new List<Dtos.DtoProperties.VendorContactsPhones>()
                    {
                        new Dtos.DtoProperties.VendorContactsPhones()
                        {
                            Extension = "Ext1",
                            Number = "800 555 1212",
                            Type = new GuidObject2( "c830e686-7692-4012-8da5-b1b5d44389b6" )
                        }
                    },
                        RelationshipType = new GuidObject2( "d830e686-7692-4012-8da5-b1b5d44389b7" ),
                        StartOn = DateTime.Now
                    }
                };

                vendorContactInitiationProcess = new VendorContactInitiationProcess()
                {
                    Vendor = new GuidObject2( "6518d26e-ab8d-4aa0-95f0-f415fa7c0004" ),
                    VendorContact = new Dtos.DtoProperties.VendorContact()
                    {
                        Detail = new GuidObject2( "6518d26e-ab8d-4aa0-95f0-f415fa7c0001" ),
                        Person = new Dtos.DtoProperties.VendorPersonContact()
                        {
                            LastName = "Wick",
                            FirstName = "John"
                        }
                    },
                    VendorContactPhones = new List<VendorContactsPhones>()
                    {
                        new VendorContactsPhones()
                        {
                            Type = new GuidObject2("c830e686-7692-4012-8da5-b1b5d44389b6"),
                            Extension = "123",
                            Number = "8005551212"
                        }
                    },
                    VendorContactEmail = new EmailProperty()
                    {
                        EmailAddress = "abc@abc.com",
                        EmailType    = new GuidObject2( emailTypeGuid )
                    }
                };

                organizationContact = organizationContactCollection.First();
                #endregion
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
            public async Task VendorContactsService_GetVendorContactsAsync()
            {
                var results = await _vendorContactsRequestsService.GetVendorContactsAsync(offset, limit, null, true);
                Assert.IsTrue(results is Tuple<IEnumerable<VendorContacts>, int>);
                Assert.IsNotNull(results);
       
            }

            [TestMethod]
            public async Task VendorContactsService_GetVendorContactsAsync_Count()
            {
                var results = await _vendorContactsRequestsService.GetVendorContactsAsync(offset, limit, null, true);
                Assert.AreEqual(1, results.Item2);
            }

            [TestMethod]
            public async Task VendorContactsService_GetVendorContactsAsync_Properties()
            {
                var results = await _vendorContactsRequestsService.GetVendorContactsAsync(offset, limit, null, true);

                foreach (var result in results.Item1)
                {
                    var expected = organizationContactCollection.FirstOrDefault(x => x.Guid == result.Id);

                    Assert.IsTrue(result is VendorContacts);
                    Assert.IsNotNull(result);
                    Assert.AreEqual(expected.Guid, result.Id, "GUID");
                    Assert.AreEqual(expected.EndDate, result.Contact.EndOn, "End Date");
                    Assert.AreEqual(expected.StartDate, result.Contact.StartOn, "Start Date");
                    Assert.AreEqual(expected.ContactPreferedName, result.Contact.Person.Name, "Preferred Name");
                    Assert.AreEqual(expected.PhoneInfos.Count(), result.Contact.Phones.Count(), "Phone count");

                    Assert.AreEqual(expected.PhoneInfos[0].PhoneNumber, result.Contact.Phones[0].Number, "Phone Number");
                    Assert.AreEqual(expected.PhoneInfos[0].PhoneExtension, result.Contact.Phones[0].Extension, "Phone Ext");

                    var phoneTypeGuid = phoneTypesCollection.FirstOrDefault(ph => ph.Code == expected.PhoneInfos[0].PhoneType);

                    Assert.AreEqual(phoneTypeGuid.Guid, result.Contact.Phones[0].Type.Id);
                }
            }


            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task VendorContactsService_GetVendorContactsAsync_MissingRelationTypes()
            {

                _referenceDataRepositoryMock.Setup(repo => repo.GetRelationTypes3GuidAsync(It.IsAny<string>())).ThrowsAsync(new IntegrationApiException());
                var results = await _vendorContactsRequestsService.GetVendorContactsAsync(offset, limit, null, true);
            }


            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task VendorContactsService_GetVendorContactsAsync_MissingPhoneTypes()
            {

                _referenceDataRepositoryMock.Setup(repo => repo.GetPhoneTypesGuidAsync(It.IsAny<string>())).ThrowsAsync(new IntegrationApiException());
                var results = await _vendorContactsRequestsService.GetVendorContactsAsync(offset, limit, null, true);
            }

           
            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task VendorContactsService_GetVendorContactsAsync_MissingPhone()
            {
                foreach (var org in organizationContactCollection)
                {
                    foreach (var phone in org.PhoneInfos)
                    {
                        phone.PhoneNumber = string.Empty;
                    }
                }
                var results = await _vendorContactsRequestsService.GetVendorContactsAsync(offset, limit, null, true);
            }


            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task VendorContactsService_GetVendorContactsAsync_MissingVendor()
            {
                foreach (var org in organizationContactCollection)
                {
                    org.VendorId = string.Empty;
                }
                var results = await _vendorContactsRequestsService.GetVendorContactsAsync(offset, limit, null, true);
            }

            [TestMethod]
            public async Task VendorContactsService_GetPersonMatchingRequestsAsync_VendorFilter()
            {
                VendorContacts filter = null;
                 

                //vendorId = await _vendorsRepository.GetVendorIdFromGuidAsync(criteria.Vendor.Id);
                _vendorsRepositoryMock.Setup(vrm => vrm.GetVendorGuidsCollectionAsync(It.IsAny<string[]>())).ReturnsAsync(vendorDict);
                var vendor = vendorDict.FirstOrDefault();
                
                _vendorsRepositoryMock.Setup(vrm => vrm.GetVendorIdFromGuidAsync(vendor.Value)).ReturnsAsync(vendor.Key);
                filter = new VendorContacts() { Vendor = new GuidObject2(vendor.Value) };
                

                var results = await _vendorContactsRequestsService.GetVendorContactsAsync(offset, limit, filter, true);

                Assert.AreEqual(1, results.Item2);
                Assert.AreEqual(filter.Vendor.Id, results.Item1.FirstOrDefault().Vendor.Id, "Vendor ID");
            }

            [TestMethod]
            [ExpectedException(typeof(System.ArgumentNullException))]
            public async Task VendorContactsService_GetVendorContactsByGuidAsync_Empty()
            {
                await _vendorContactsRequestsService.GetVendorContactsByGuidAsync("");
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task VendorContactsService_GetVendorContactsByGuidAsync_Null()
            {
                await _vendorContactsRequestsService.GetVendorContactsByGuidAsync(null);
            }


            [TestMethod]
            public async Task VendorContactsService_GetVendorContactsByGuidAsync()
            {
                _vendorContactsRepositoryMock.Setup(pmr => pmr.GetVendorContactsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>()))
                    .ReturnsAsync(organizationContactTuple);
                var organizationContact = organizationContactTuple.Item1.FirstOrDefault(x => x.Id == "1");

                _vendorContactsRepositoryMock.Setup(pmr => pmr.GetGetVendorContactsByGuidAsync(It.IsAny<string>()))
                    .ReturnsAsync(organizationContact);

                _vendorsRepositoryMock.Setup(vrm => vrm.GetVendorGuidsCollectionAsync(It.IsAny<List<string>>())).ReturnsAsync(vendorDict);

                var result = await _vendorContactsRequestsService.GetVendorContactsByGuidAsync(organizationContact.Guid, true);
                Assert.IsTrue(result is VendorContacts);
                Assert.IsNotNull(result);

                Assert.AreEqual(new DateTime(2020, 1, 1), result.Contact.EndOn);
                Assert.AreEqual(new DateTime(2019, 1, 1), result.Contact.StartOn);
                Assert.AreEqual("Pref1", result.Contact.Person.Name);
                Assert.AreEqual(1, result.Contact.Phones.Count());
                Assert.AreEqual("6518d26e-ab8d-4aa0-95f0-f415fa7c0001", result.Contact.Person.Detail.Id);
                Assert.AreEqual("1234567", result.Contact.Phones[0].Number);
                Assert.AreEqual("123", result.Contact.Phones[0].Extension);

                var phoneTypeGuid = phoneTypesCollection.FirstOrDefault(ph => ph.Code == "HOME");

                Assert.AreEqual(phoneTypeGuid.Guid, result.Contact.Phones[0].Type.Id);

            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task VendorContactsService_GetVendorContactsByGuidAsync_EmptyVendorGuids()
            {

                _vendorsRepositoryMock.Setup(vrm => vrm.GetVendorGuidsCollectionAsync(It.IsAny<List<string>>())).ReturnsAsync(null);

                var organizationContact = organizationContactTuple.Item1.FirstOrDefault();

                _vendorContactsRepositoryMock.Setup(pmr => pmr.GetGetVendorContactsByGuidAsync(It.IsAny<string>()))
                    .ReturnsAsync(organizationContact);

                await _vendorContactsRequestsService.GetVendorContactsByGuidAsync(Guid.NewGuid().ToString(), true);

            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task VendorContactsService_GetVendorContactsByGuidAsync_KeyNotFound()
            {

                _vendorsRepositoryMock.Setup(vrm => vrm.GetVendorGuidsCollectionAsync(It.IsAny<List<string>>())).ReturnsAsync(vendorDict);

                var organizationContact = organizationContactTuple.Item1.FirstOrDefault();

                _vendorContactsRepositoryMock.Setup(pmr => pmr.GetGetVendorContactsByGuidAsync(It.IsAny<string>()))
                    .ThrowsAsync(new KeyNotFoundException());

                await _vendorContactsRequestsService.GetVendorContactsByGuidAsync(Guid.NewGuid().ToString(), true);

            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task VendorContactsService_GetVendorContactsByGuidAsync_InvalidOperationException()
            {

                _vendorsRepositoryMock.Setup(vrm => vrm.GetVendorGuidsCollectionAsync(It.IsAny<List<string>>())).ReturnsAsync(vendorDict);

                var organizationContact = organizationContactTuple.Item1.FirstOrDefault();

                _vendorContactsRepositoryMock.Setup(pmr => pmr.GetGetVendorContactsByGuidAsync(It.IsAny<string>()))
                    .ThrowsAsync(new InvalidOperationException());

                await _vendorContactsRequestsService.GetVendorContactsByGuidAsync(Guid.NewGuid().ToString(), true);

            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task VendorContactsService_GetVendorContactsAsync_PermissionsException()
            {
                _roleRepositoryMock.Setup(i => i.GetRolesAsync()).ReturnsAsync(new List<Domain.Entities.Role>() { });
                _roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { });

                await _vendorContactsRequestsService.GetVendorContactsAsync(offset, limit, null, true);
            }

            [TestMethod]
            public async Task CreateVendorContactInitiationProcessAsync_VendorContact()
            {
                var vendor = vendorDict.FirstOrDefault();
                vendorContactInitiationProcess.VendorContactEmail = null;
                vendorContactInitiationProcess.VendorContactPhones = null;
                _vendorContactsRepositoryMock.Setup( r => r.CreateVendorContactInitiationProcessAsync( It.IsAny<OrganizationContactInitiationProcess>() ) )
                    .ReturnsAsync( new Tuple<Domain.ColleagueFinance.Entities.OrganizationContact, string>( organizationContact, "" ) );
                _vendorsRepositoryMock.Setup( vrm => vrm.GetVendorIdFromGuidAsync( vendor.Value ) ).ReturnsAsync( vendor.Key );

                _personRepositoryMock.SetupSequence( p => p.IsCorpAsync( It.IsAny<string>() ) )
                    .Returns( Task.FromResult<bool>( true ) )
                    .Returns( Task.FromResult<bool>( false ) );
                _personRepositoryMock.Setup( r => r.GetPersonIdFromGuidAsync( It.IsAny<string>() ) ).ReturnsAsync( "1" );

                var results = await _vendorContactsRequestsService.CreateVendorContactInitiationProcessAsync( vendorContactInitiationProcess );
                Assert.IsTrue( results is VendorContacts );
                Assert.IsNotNull( results );
            }

            [TestMethod]
            public async Task CreateVendorContactInitiationProcessAsync_PMR()
            {
                var vendor = vendorDict.FirstOrDefault();
                vendorContactInitiationProcess.VendorContactEmail = null;
                vendorContactInitiationProcess.VendorContactPhones = null;
                _vendorContactsRepositoryMock.Setup( r => r.CreateVendorContactInitiationProcessAsync( It.IsAny<OrganizationContactInitiationProcess>() ) )
                    .ReturnsAsync( new Tuple<Domain.ColleagueFinance.Entities.OrganizationContact, string>( null, "1" ) );
                _vendorsRepositoryMock.Setup( vrm => vrm.GetVendorIdFromGuidAsync( vendor.Value ) ).ReturnsAsync( vendor.Key );

                _personRepositoryMock.SetupSequence( p => p.IsCorpAsync( It.IsAny<string>() ) )
                    .Returns( Task.FromResult<bool>( true ) )
                    .Returns( Task.FromResult<bool>( false ) );
                _personRepositoryMock.Setup( r => r.GetPersonIdFromGuidAsync( It.IsAny<string>() ) ).ReturnsAsync( "1" );
                _personMatchingRequestsRepositoryMock.Setup( r => r.GetPersonMatchRequestsByIdAsync( It.IsAny<string>(), It.IsAny<bool>() ) ).ReturnsAsync( personMatch1 );

                var results = await _vendorContactsRequestsService.CreateVendorContactInitiationProcessAsync( vendorContactInitiationProcess );
                Assert.IsTrue( results is PersonMatchingRequests );
                Assert.IsNotNull( results );
            }

            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public async Task CreateVendorContactInitiationProcessAsync_NoPermissions()
            {
                _roleRepositoryMock.Setup(i => i.GetRolesAsync()).ReturnsAsync(new List<Domain.Entities.Role>() { });
                _roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { });

                await _vendorContactsRequestsService.CreateVendorContactInitiationProcessAsync(vendorContactInitiationProcess);
            }

            [TestMethod]
            [ExpectedException( typeof( IntegrationApiException ) )]
            public async Task CreateVendorContactInitiationProcessAsync_Validation_Null_Dto()
            {
                var actual = await _vendorContactsRequestsService.CreateVendorContactInitiationProcessAsync( null );
            }

            [TestMethod]
            [ExpectedException( typeof( IntegrationApiException ) )]
            public async Task CreateVendorContactInitiationProcessAsync_Validation_Empty_DtoId()
            {
                vendorContactInitiationProcess.Vendor.Id = string.Empty;
                var actual = await _vendorContactsRequestsService.CreateVendorContactInitiationProcessAsync( vendorContactInitiationProcess );
            }

            [TestMethod]
            [ExpectedException( typeof( IntegrationApiException ) )]
            public async Task CreateVendorContactInitiationProcessAsync_Validation_IsCorp_False()
            {
                _personRepositoryMock.SetupSequence( p => p.IsCorpAsync( It.IsAny<string>() ) )
                   .Returns( Task.FromResult<bool>( false ) );
                try
                {
                    var actual = await _vendorContactsRequestsService.CreateVendorContactInitiationProcessAsync( vendorContactInitiationProcess );
                }
                catch( IntegrationApiException e )
                {
                    Assert.AreEqual( "The vendor specified is a person, only those vendors setup as organizations are eligible for a separate contact person.", e.Errors[ 0 ].Message );
                    throw;
                }
            }

            [TestMethod]
            [ExpectedException( typeof( IntegrationApiException ) )]
            public async Task CreateVendorContactInitiationProcessAsync_Validation_ArgumentNullException()
            {
                _personRepositoryMock.Setup( p => p.IsCorpAsync( It.IsAny<string>() ) ).ThrowsAsync( new ArgumentNullException() );
                try
                {
                    var actual = await _vendorContactsRequestsService.CreateVendorContactInitiationProcessAsync( vendorContactInitiationProcess );
                }
                catch( IntegrationApiException e )
                {
                    Assert.AreEqual( "vendor.id is not a valid GUID for vendor.", e.Errors[ 0 ].Message );
                    throw;
                }
            }

            [TestMethod]
            [ExpectedException( typeof( IntegrationApiException ) )]
            public async Task CreateVendorContactInitiationProcessAsync_Validation_KeyNotFoundException()
            {
                _personRepositoryMock.Setup( p => p.IsCorpAsync( It.IsAny<string>() ) ).ThrowsAsync( new KeyNotFoundException() );
                try
                {
                    var actual = await _vendorContactsRequestsService.CreateVendorContactInitiationProcessAsync( vendorContactInitiationProcess );
                }
                catch( IntegrationApiException e )
                {
                    Assert.AreEqual( "vendor.id is not a valid GUID for vendor.", e.Errors[ 0 ].Message );
                    throw;
                }
            }

            [TestMethod]
            [ExpectedException( typeof( IntegrationApiException ) )]
            public async Task CreateVendorContactInitiationProcessAsync_Validation_IsCorp_True()
            {
                var vendor = vendorDict.FirstOrDefault();
                _vendorsRepositoryMock.Setup( vrm => vrm.GetVendorIdFromGuidAsync( vendor.Value ) ).ReturnsAsync( vendor.Key );
                _personRepositoryMock.Setup( r => r.GetPersonIdFromGuidAsync( It.IsAny<string>() ) ).ReturnsAsync( "1" );
                _personRepositoryMock.Setup( p => p.IsCorpAsync( It.IsAny<string>() ) ).ReturnsAsync( true );
                try
                {
                    var actual = await _vendorContactsRequestsService.CreateVendorContactInitiationProcessAsync( vendorContactInitiationProcess );
                }
                catch( IntegrationApiException  e)
                {
                    Assert.AreEqual( "You cannot associate a corporation to a person as their other relations.", e.Errors[0].Message );
                    throw;
                }
            }

            [TestMethod]
            [ExpectedException( typeof( IntegrationApiException ) )]
            public async Task CreateVendorContactInitiationProcessAsync_Validation_ArgumentNullException1()
            {
                var vendor = vendorDict.FirstOrDefault();
                _vendorsRepositoryMock.Setup( vrm => vrm.GetVendorIdFromGuidAsync( vendor.Value ) ).ReturnsAsync( vendor.Key );
                _personRepositoryMock.Setup( r => r.GetPersonIdFromGuidAsync( It.IsAny<string>() ) ).ThrowsAsync( new ArgumentNullException() );
                _personRepositoryMock.Setup( p => p.IsCorpAsync( It.IsAny<string>() ) ).ReturnsAsync( true );

                try
                {
                    var actual = await _vendorContactsRequestsService.CreateVendorContactInitiationProcessAsync( vendorContactInitiationProcess );
                }
                catch( IntegrationApiException e )
                {
                    Assert.AreEqual( "vendorContact.detail.id is not a valid GUID for vendor contact.", e.Errors[ 0 ].Message );
                    throw;
                }
            }

            [TestMethod]
            [ExpectedException( typeof( IntegrationApiException ) )]
            public async Task CreateVendorContactInitiationProcessAsync_Validation_KeyNotFoundException1()
            {
                var vendor = vendorDict.FirstOrDefault();
                _vendorsRepositoryMock.Setup( vrm => vrm.GetVendorIdFromGuidAsync( vendor.Value ) ).ReturnsAsync( vendor.Key );
                _personRepositoryMock.Setup( r => r.GetPersonIdFromGuidAsync( It.IsAny<string>() ) ).ThrowsAsync( new KeyNotFoundException() );
                _personRepositoryMock.Setup( p => p.IsCorpAsync( It.IsAny<string>() ) ).ReturnsAsync( true );

                try
                {
                    var actual = await _vendorContactsRequestsService.CreateVendorContactInitiationProcessAsync( vendorContactInitiationProcess );
                }
                catch( IntegrationApiException e )
                {
                    Assert.AreEqual( "vendorContact.detail.id is not a valid GUID for vendor contact.", e.Errors[ 0 ].Message );
                    throw;
                }
            }

            [TestMethod]
            [ExpectedException( typeof( IntegrationApiException ) )]
            public async Task CreateVendorContactInitiationProcessAsync_Validation_RepositoryException()
            {
                var vendor = vendorDict.FirstOrDefault();
                _vendorsRepositoryMock.Setup( vrm => vrm.GetVendorIdFromGuidAsync( vendor.Value ) ).ReturnsAsync( vendor.Key );
                _personRepositoryMock.Setup( r => r.GetPersonIdFromGuidAsync( It.IsAny<string>() ) ).ThrowsAsync( new RepositoryException() );
                _personRepositoryMock.Setup( p => p.IsCorpAsync( It.IsAny<string>() ) ).ReturnsAsync( true );

                try
                {
                    var actual = await _vendorContactsRequestsService.CreateVendorContactInitiationProcessAsync( vendorContactInitiationProcess );
                }
                catch( IntegrationApiException e )
                {
                    Assert.AreEqual( "vendorContact.detail.id is not a valid GUID for vendor contact.", e.Errors[ 0 ].Message );
                    throw;
                }
            }

            [TestMethod]
            [ExpectedException( typeof( IntegrationApiException ) )]
            public async Task CreateVendorContactInitiationProcessAsync_IntegrationApiException_Demographic()
            {
                var vendor = vendorDict.FirstOrDefault();
                _vendorContactsRepositoryMock.Setup( r => r.CreateVendorContactInitiationProcessAsync( It.IsAny<OrganizationContactInitiationProcess>() ) )
                    .ReturnsAsync( new Tuple<Domain.ColleagueFinance.Entities.OrganizationContact, string>( organizationContact, "" ) );
                _vendorsRepositoryMock.Setup( vrm => vrm.GetVendorIdFromGuidAsync( vendor.Value ) ).ReturnsAsync( vendor.Key );
                _referenceDataRepositoryMock.Setup( r => r.GetPhoneTypesAsync( It.IsAny<bool>() ) ).ThrowsAsync(new Exception());

                vendorContactInitiationProcess.VendorContact.Detail = null;
                vendorContactInitiationProcess.VendorContact.Person.FirstName = string.Empty;
                vendorContactInitiationProcess.VendorContact.Person.LastName = string.Empty;

                _personRepositoryMock.SetupSequence( p => p.IsCorpAsync( It.IsAny<string>() ) )
                    .Returns( Task.FromResult<bool>( true ) )
                    .Returns( Task.FromResult<bool>( false ) );
                _personRepositoryMock.Setup( r => r.GetPersonIdFromGuidAsync( It.IsAny<string>() ) ).ReturnsAsync( "1" );

                var results = await _vendorContactsRequestsService.CreateVendorContactInitiationProcessAsync( vendorContactInitiationProcess );
            }

            [TestMethod]
            [ExpectedException( typeof( IntegrationApiException ) )]
            public async Task CreateVendorContactInitiationProcessAsync_IntegrationApiException_PhoneTypes()
            {
                var vendor = vendorDict.FirstOrDefault();
                _vendorContactsRepositoryMock.Setup( r => r.CreateVendorContactInitiationProcessAsync( It.IsAny<OrganizationContactInitiationProcess>() ) )
                    .ReturnsAsync( new Tuple<Domain.ColleagueFinance.Entities.OrganizationContact, string>( organizationContact, "" ) );
                _vendorsRepositoryMock.Setup( vrm => vrm.GetVendorIdFromGuidAsync( vendor.Value ) ).ReturnsAsync( vendor.Key );
                _referenceDataRepositoryMock.Setup( r => r.GetPhoneTypesAsync( It.IsAny<bool>() ) ).ThrowsAsync( new Exception() );

                vendorContactInitiationProcess.VendorContactPhones.FirstOrDefault().Type = null;
                vendorContactInitiationProcess.VendorContactPhones.FirstOrDefault().Number = string.Empty;

                _personRepositoryMock.SetupSequence( p => p.IsCorpAsync( It.IsAny<string>() ) )
                    .Returns( Task.FromResult<bool>( true ) )
                    .Returns( Task.FromResult<bool>( false ) );
                _personRepositoryMock.Setup( r => r.GetPersonIdFromGuidAsync( It.IsAny<string>() ) ).ReturnsAsync( "1" );

                var results = await _vendorContactsRequestsService.CreateVendorContactInitiationProcessAsync( vendorContactInitiationProcess );
            }

            [TestMethod]
            [ExpectedException( typeof( IntegrationApiException ) )]
            public async Task CreateVendorContactInitiationProcessAsync_IntegrationApiException_Phones_Validation()
            {
                var vendor = vendorDict.FirstOrDefault();
                _vendorContactsRepositoryMock.Setup( r => r.CreateVendorContactInitiationProcessAsync( It.IsAny<OrganizationContactInitiationProcess>() ) )
                    .ReturnsAsync( new Tuple<Domain.ColleagueFinance.Entities.OrganizationContact, string>( organizationContact, "" ) );
                _vendorsRepositoryMock.Setup( vrm => vrm.GetVendorIdFromGuidAsync( vendor.Value ) ).ReturnsAsync( vendor.Key );

                vendorContactInitiationProcess.VendorContactPhones.FirstOrDefault().Type = null;
                vendorContactInitiationProcess.VendorContactPhones.FirstOrDefault().Number = string.Empty;

                _personRepositoryMock.SetupSequence( p => p.IsCorpAsync( It.IsAny<string>() ) )
                    .Returns( Task.FromResult<bool>( true ) )
                    .Returns( Task.FromResult<bool>( false ) );
                _personRepositoryMock.Setup( r => r.GetPersonIdFromGuidAsync( It.IsAny<string>() ) ).ReturnsAsync( "1" );

                var results = await _vendorContactsRequestsService.CreateVendorContactInitiationProcessAsync( vendorContactInitiationProcess );
            }

            [TestMethod]
            [ExpectedException( typeof( IntegrationApiException ) )]
            public async Task CreateVendorContactInitiationProcessAsync_IntegrationApiException_Phones_Validation1()
            {
                var vendor = vendorDict.FirstOrDefault();
                _vendorContactsRepositoryMock.Setup( r => r.CreateVendorContactInitiationProcessAsync( It.IsAny<OrganizationContactInitiationProcess>() ) )
                    .ReturnsAsync( new Tuple<Domain.ColleagueFinance.Entities.OrganizationContact, string>( organizationContact, "" ) );
                _vendorsRepositoryMock.Setup( vrm => vrm.GetVendorIdFromGuidAsync( vendor.Value ) ).ReturnsAsync( vendor.Key );

                vendorContactInitiationProcess.VendorContactPhones.FirstOrDefault().Number = string.Empty;

                _personRepositoryMock.SetupSequence( p => p.IsCorpAsync( It.IsAny<string>() ) )
                    .Returns( Task.FromResult<bool>( true ) )
                    .Returns( Task.FromResult<bool>( false ) );
                _personRepositoryMock.Setup( r => r.GetPersonIdFromGuidAsync( It.IsAny<string>() ) ).ReturnsAsync( "1" );

                var results = await _vendorContactsRequestsService.CreateVendorContactInitiationProcessAsync( vendorContactInitiationProcess );
            }

            [TestMethod]
            [ExpectedException( typeof( IntegrationApiException ) )]
            public async Task CreateVendorContactInitiationProcessAsync_IntegrationApiException_Email_AddressNull()
            {
                var vendor = vendorDict.FirstOrDefault();
                vendorContactInitiationProcess.VendorContactPhones = null;
                vendorContactInitiationProcess.VendorContactEmail.EmailAddress = string.Empty;

                _vendorContactsRepositoryMock.Setup( r => r.CreateVendorContactInitiationProcessAsync( It.IsAny<OrganizationContactInitiationProcess>() ) )
                    .ReturnsAsync( new Tuple<Domain.ColleagueFinance.Entities.OrganizationContact, string>( organizationContact, "" ) );
                _vendorsRepositoryMock.Setup( vrm => vrm.GetVendorIdFromGuidAsync( vendor.Value ) ).ReturnsAsync( vendor.Key );

                _personRepositoryMock.SetupSequence( p => p.IsCorpAsync( It.IsAny<string>() ) )
                    .Returns( Task.FromResult<bool>( true ) )
                    .Returns( Task.FromResult<bool>( false ) );
                _personRepositoryMock.Setup( r => r.GetPersonIdFromGuidAsync( It.IsAny<string>() ) ).ReturnsAsync( "1" );

                var results = await _vendorContactsRequestsService.CreateVendorContactInitiationProcessAsync( vendorContactInitiationProcess );
            }

            [TestMethod]
            [ExpectedException( typeof( IntegrationApiException ) )]
            public async Task CreateVendorContactInitiationProcessAsync_IntegrationApiException_Email_Types_Null()
            {
                var vendor = vendorDict.FirstOrDefault();
                vendorContactInitiationProcess.VendorContactPhones = null;

                _vendorContactsRepositoryMock.Setup( r => r.CreateVendorContactInitiationProcessAsync( It.IsAny<OrganizationContactInitiationProcess>() ) )
                    .ReturnsAsync( new Tuple<Domain.ColleagueFinance.Entities.OrganizationContact, string>( organizationContact, "1" ) );
                _vendorsRepositoryMock.Setup( vrm => vrm.GetVendorIdFromGuidAsync( vendor.Value ) ).ReturnsAsync( vendor.Key );
                _referenceDataRepositoryMock.Setup( r => r.GetEmailTypesAsync( It.IsAny<bool>() ) ).ReturnsAsync( new List<Domain.Base.Entities.EmailType>() { } );

                _personRepositoryMock.SetupSequence( p => p.IsCorpAsync( It.IsAny<string>() ) )
                    .Returns( Task.FromResult<bool>( true ) )
                    .Returns( Task.FromResult<bool>( false ) );
                _personRepositoryMock.Setup( r => r.GetPersonIdFromGuidAsync( It.IsAny<string>() ) ).ReturnsAsync( "1" );

                var results = await _vendorContactsRequestsService.CreateVendorContactInitiationProcessAsync( vendorContactInitiationProcess );
            }

            [TestMethod]
            [ExpectedException( typeof( IntegrationApiException ) )]
            public async Task CreateVendorContactInitiationProcessAsync_IntegrationApiException_Email_Type_Null()
            {
                var vendor = vendorDict.FirstOrDefault();
                vendorContactInitiationProcess.VendorContactPhones = null;
                vendorContactInitiationProcess.VendorContactEmail.EmailType = null;

                _vendorContactsRepositoryMock.Setup( r => r.CreateVendorContactInitiationProcessAsync( It.IsAny<OrganizationContactInitiationProcess>() ) )
                    .ReturnsAsync( new Tuple<Domain.ColleagueFinance.Entities.OrganizationContact, string>( organizationContact, "1" ) );
                _vendorsRepositoryMock.Setup( vrm => vrm.GetVendorIdFromGuidAsync( vendor.Value ) ).ReturnsAsync( vendor.Key );

                _personRepositoryMock.SetupSequence( p => p.IsCorpAsync( It.IsAny<string>() ) )
                    .Returns( Task.FromResult<bool>( true ) )
                    .Returns( Task.FromResult<bool>( false ) );
                _personRepositoryMock.Setup( r => r.GetPersonIdFromGuidAsync( It.IsAny<string>() ) ).ReturnsAsync( "1" );

                var results = await _vendorContactsRequestsService.CreateVendorContactInitiationProcessAsync( vendorContactInitiationProcess );
            }
        }
    }
}