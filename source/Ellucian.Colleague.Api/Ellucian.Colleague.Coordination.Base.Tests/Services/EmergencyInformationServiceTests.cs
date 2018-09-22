// Copyright 2014-2018 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Web.Adapters;
using Ellucian.Web.Security;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;
using Ellucian.Colleague.Domain.Base.Entities;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.Base.Tests.Services
{
    [TestClass]
    public class EmergencyInformationServiceTests
    {
        // The service to be tested
        private EmergencyInformationService emergencyInformationService;

        // Mock/fake objects to construct EmergencyInformationService
        private Mock<IEmergencyInformationRepository> emerInfoRepoMock;
        private IEmergencyInformationRepository emerInfoRepo;
        private Mock<IPersonBaseRepository> personBaseRepositoryMock;
        private Mock<IAdapterRegistry> adapterRegistryMock;
        private IAdapterRegistry adapterRegistry;
        private ILogger logger;
        private Mock<IRoleRepository> roleRepoMock;
        private IRoleRepository roleRepo;
        private Person001UserFactory currentUserFactoryFake;
        private Mock<IConfigurationRepository> configurationRepositoryMock;
        private Mock<IStaffRepository> staffRepositoryMock;
        private IStaffRepository staffRepository;
        private Mock<IPersonRepository> personRepositoryMock;

        // Emergency information data for one person for tests
        private static string personId = "S001";

        // One emergency contact
        private string name1 = "name 1";
        private string relationship1 = "rel 1";
        private string daytimePhone1 = "dp 1";
        private string eveningPhone1 = "ep 1";
        private string otherPhone1 = "ot 1";
        private DateTime? effectiveDate1 = new DateTime(2012,1,1);
        private bool isMissingPersonContact1 = true;
        private bool isEmergencyContact1 = false;
        private string address1 = "12334 main str";

        // A second emergency contact
        private string name2 = "name 2";
        private string relationship2 = "rel 2";
        private string daytimePhone2 = "dp 2";
        private string eveningPhone2 = "ep 2";
        private string otherPhone2 = "ot 2";
        private DateTime? effectiveDate2 = new DateTime(2011,2,1);
        private bool isMissingPersonContact2 = false;
        private bool isEmergencyContact2 = true;
        private string address2 = "4444 elm str";

        // Other test values
        private string insuranceInformation = "My Ins info";
        private string hospitalPreference = "My hospital pref";
        private List<string> healthConditions = new List<string>() { "cond1", "cond2" };
        private DateTime? confirmedDate = new DateTime(2014,2,3);
        private string additionalInformation = "Addl info";

        // A domain entity populated with the emergency information data for person S001
        private Domain.Base.Entities.EmergencyInformation emergencyInformationEntityPerson1;

        // A dto populated with the emergency information data for person S001
        private Dtos.Base.EmergencyInformation emergencyInformationDtoPerson1;


        [TestInitialize]
        public void Initialize()
        {
            // Instantiate mock and fake objects used to construct the service
            emerInfoRepoMock = new Mock<IEmergencyInformationRepository>();
            emerInfoRepo = emerInfoRepoMock.Object;
            adapterRegistryMock = new Mock<IAdapterRegistry>();
            personBaseRepositoryMock = new Mock<IPersonBaseRepository>();
            adapterRegistry = adapterRegistryMock.Object;
            logger = new Mock<ILogger>().Object;
            roleRepoMock = new Mock<IRoleRepository>();
            var emergencyAccessRole = new Domain.Entities.Role(1, "Emergency Access");
            var studentRole = new Domain.Entities.Role(2, "Student");
            emergencyAccessRole.AddPermission(new Domain.Entities.Permission(Domain.Base.BasePermissionCodes.ViewPersonEmergencyContacts));
            emergencyAccessRole.AddPermission(new Domain.Entities.Permission(Domain.Base.BasePermissionCodes.ViewPersonHealthConditions));
            emergencyAccessRole.AddPermission(new Domain.Entities.Permission(Domain.Base.BasePermissionCodes.ViewPersonOtherEmergencyInformation));
            roleRepoMock.Setup(rr => rr.Roles).Returns(new List<Domain.Entities.Role> { emergencyAccessRole, studentRole });
            roleRepo = roleRepoMock.Object;
            currentUserFactoryFake = new Person001UserFactory();
            configurationRepositoryMock = new Mock<IConfigurationRepository>();
            staffRepositoryMock = new Mock<IStaffRepository>();
            staffRepository = staffRepositoryMock.Object;
            personRepositoryMock = new Mock<IPersonRepository>();

            configurationRepositoryMock.Setup(x => x.GetEmergencyInformationConfigurationAsync()).ReturnsAsync(new EmergencyInformationConfiguration(false, false, false, true));

            personBaseRepositoryMock.Setup(pr => pr.GetPersonBaseAsync(personId, It.IsAny<bool>())).ReturnsAsync(new PersonBase(personId, "Weasley") { PreferredName = "Fred Weasley" });

            // Mock the adapter registry to use the automappers between the EmergencyInformation domain entity and dto. 
            var emptyAdapterRegistryMock = new Mock<IAdapterRegistry>(); // An empty mock adapter registry to instantiate AutoMapperAdapter
            adapterRegistryMock.Setup(x => x.GetAdapter<Domain.Base.Entities.EmergencyInformation,
                Dtos.Base.EmergencyInformation>()).Returns(new Coordination.Base.Adapters.EmergencyInformationEntityAdapter(emptyAdapterRegistryMock.Object, logger));
            adapterRegistryMock.Setup(x => x.GetAdapter<Dtos.Base.EmergencyInformation,
                Domain.Base.Entities.EmergencyInformation>()).Returns(new Coordination.Base.Adapters.EmergencyInformationDtoAdapter(emptyAdapterRegistryMock.Object, logger));

            // Instantiate the service
            emergencyInformationService = new EmergencyInformationService(adapterRegistry, currentUserFactoryFake, roleRepo, logger,
                    emerInfoRepo, configurationRepositoryMock.Object, personBaseRepositoryMock.Object, staffRepository, personRepositoryMock.Object);

            // Create an EmergencyInformation domain entity with our test data
            emergencyInformationEntityPerson1 = new Domain.Base.Entities.EmergencyInformation(personId);
            emergencyInformationEntityPerson1.AddEmergencyContact(new Domain.Base.Entities.EmergencyContact(name1)
                {
                    Address = address1,
                    DaytimePhone = daytimePhone1,
                    EffectiveDate = effectiveDate1,
                    EveningPhone = eveningPhone1,
                    IsEmergencyContact = isEmergencyContact1,
                    IsMissingPersonContact = isMissingPersonContact1,
                    OtherPhone = otherPhone1,
                    Relationship = relationship1
                });

            emergencyInformationEntityPerson1.AddEmergencyContact(new Domain.Base.Entities.EmergencyContact(name2)
            {
                Address = address2,
                DaytimePhone = daytimePhone2,
                EffectiveDate = effectiveDate2,
                EveningPhone = eveningPhone2,
                IsEmergencyContact = isEmergencyContact2,
                IsMissingPersonContact = isMissingPersonContact2,
                OtherPhone = otherPhone2,
                Relationship = relationship2
            });
            foreach (string s in healthConditions)
            {
                emergencyInformationEntityPerson1.AddHealthCondition(s);
            }
            emergencyInformationEntityPerson1.AdditionalInformation = additionalInformation;
            emergencyInformationEntityPerson1.ConfirmedDate = confirmedDate;
            emergencyInformationEntityPerson1.HospitalPreference = hospitalPreference;
            emergencyInformationEntityPerson1.InsuranceInformation = insuranceInformation;

            // Create a dto with our test data
            emergencyInformationDtoPerson1 = new Dtos.Base.EmergencyInformation();
            emergencyInformationDtoPerson1.PersonId = personId;
            emergencyInformationDtoPerson1.EmergencyContacts = new List<Dtos.Base.EmergencyContact>();
            emergencyInformationDtoPerson1.EmergencyContacts.Add(new Dtos.Base.EmergencyContact()
            {
                Address = address1,
                DaytimePhone = daytimePhone1,
                EffectiveDate = effectiveDate1,
                EveningPhone = eveningPhone1,
                IsEmergencyContact = isEmergencyContact1,
                IsMissingPersonContact = isMissingPersonContact1,
                Name = name1,
                OtherPhone = otherPhone1,
                Relationship = relationship1
            });

            emergencyInformationDtoPerson1.EmergencyContacts.Add(new Dtos.Base.EmergencyContact()
            {
                Address = address2,
                DaytimePhone = daytimePhone2,
                EffectiveDate = effectiveDate2,
                EveningPhone = eveningPhone2,
                IsEmergencyContact = isEmergencyContact2,
                IsMissingPersonContact = isMissingPersonContact2,
                Name = name2,
                OtherPhone = otherPhone2,
                Relationship = relationship2
            });
            emergencyInformationDtoPerson1.HealthConditions = new List<string>();
            foreach (string s in healthConditions)
                emergencyInformationDtoPerson1.HealthConditions.Add(s);
            emergencyInformationDtoPerson1.AdditionalInformation = additionalInformation;
            emergencyInformationDtoPerson1.ConfirmedDate = confirmedDate;
            emergencyInformationDtoPerson1.HospitalPreference = hospitalPreference;
            emergencyInformationDtoPerson1.InsuranceInformation = insuranceInformation;
        }

        [TestCleanup]
        public void Cleanup()
        {
            emerInfoRepo = null;
            adapterRegistry = null;
            logger = null;
            roleRepo = null;
            currentUserFactoryFake = null;
            emergencyInformationService = null;
        }


        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task EmergencyInformation_GetEmergencyInformationThrowsExceptionIfNullPersonId()
        {
            await emergencyInformationService.GetEmergencyInformationAsync(null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task EmergencyInformation_GetEmergencyInformationThrowsExceptionIfEmptyPersonId()
        {
            await emergencyInformationService.GetEmergencyInformationAsync(String.Empty);
        }


        [TestMethod]
        [ExpectedException(typeof(PermissionsException))]
        public async Task EmergencyInformation_GetEmergencyInformationThrowsExceptionIfNotCurrentPerson()
        {
            // The service was constructed with an ICurrentUserFactory that returns personId as the person ID
            // of the current user. Pass a different person ID
            await emergencyInformationService.GetEmergencyInformationAsync(personId + "ZZZ");
        }

        [TestMethod]
        public async Task EmergencyInformation_GetEmergencyInformationReturnsEmergencyInformation()
        {
            // Mock the repository Get to return the test EmergencyInformation domain entity 
            // constructed in the Initialize method from our test data.
            emerInfoRepoMock.Setup(r => r.Get(personId)).Returns(emergencyInformationEntityPerson1);
            
            // Invoke the service method
            Dtos.Base.EmergencyInformation emergencyInformationDto = await emergencyInformationService.GetEmergencyInformationAsync(personId);

            // Verify that the dto returned by the service matches the entity returned by the mocked repository Get.
            // This establishes that the repository Get was called, and that the entity-to-dto mapper worked correctly.
            Assert.AreEqual(emergencyInformationEntityPerson1.AdditionalInformation, emergencyInformationDto.AdditionalInformation);
            Assert.AreEqual(emergencyInformationEntityPerson1.ConfirmedDate, emergencyInformationDto.ConfirmedDate);
            Assert.AreEqual(emergencyInformationEntityPerson1.EmergencyContacts.Count, emergencyInformationDto.EmergencyContacts.Count);
            for (int i = 0; i < emergencyInformationDto.EmergencyContacts.Count; i++)
            {
                var entityEmergencyContact = emergencyInformationEntityPerson1.EmergencyContacts[i];
                var dtoEmergencyContact = emergencyInformationDto.EmergencyContacts[i];
                Assert.AreEqual(entityEmergencyContact.Address, dtoEmergencyContact.Address);
                Assert.AreEqual(entityEmergencyContact.DaytimePhone, dtoEmergencyContact.DaytimePhone);
                Assert.AreEqual(entityEmergencyContact.EffectiveDate, dtoEmergencyContact.EffectiveDate);
                Assert.AreEqual(entityEmergencyContact.EveningPhone, dtoEmergencyContact.EveningPhone);
                Assert.AreEqual(entityEmergencyContact.IsEmergencyContact, dtoEmergencyContact.IsEmergencyContact);
                Assert.AreEqual(entityEmergencyContact.IsMissingPersonContact, dtoEmergencyContact.IsMissingPersonContact);
                Assert.AreEqual(entityEmergencyContact.Name, dtoEmergencyContact.Name);
                Assert.AreEqual(entityEmergencyContact.OtherPhone, dtoEmergencyContact.OtherPhone);
                Assert.AreEqual(entityEmergencyContact.Relationship, dtoEmergencyContact.Relationship);
            }
            CollectionAssert.AreEqual(emergencyInformationEntityPerson1.HealthConditions, emergencyInformationDto.HealthConditions);
            Assert.AreEqual(emergencyInformationEntityPerson1.HospitalPreference, emergencyInformationDto.HospitalPreference);
            Assert.AreEqual(emergencyInformationEntityPerson1.InsuranceInformation, emergencyInformationDto.InsuranceInformation);
            Assert.AreEqual(emergencyInformationEntityPerson1.PersonId, emergencyInformationDto.PersonId);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void EmergencyInformation_UpdateEmergencyInformationThrowsExceptionIfNullDto()
        {
            emergencyInformationService.UpdateEmergencyInformation(null);
        }

        [TestMethod]
        [ExpectedException(typeof(PermissionsException))]
        public void EmergencyInformation_UpdateEmergencyInformationThrowsExceptionIfNotCurrentPerson()
        {
            // The service was constructed with an ICurrentUserFactory that returns personId from our test data
            // of the current user. Pass a dto for a different person.
            Dtos.Base.EmergencyInformation emergencyInformationDto = new Dtos.Base.EmergencyInformation();
            emergencyInformationDto.PersonId = personId + "ZZZ";
            emergencyInformationService.UpdateEmergencyInformation(emergencyInformationDto);
        }

        [TestMethod]
        public void EmergencyInformation_UpdateEmergencyInformationSuccesfullyCallsRepoUpdateMethod()
        {
            // Mock the repository UpdateEmergencyInformation method to simply return the same
            // domain entity pass into it.
            emerInfoRepoMock.Setup(r => r.UpdateEmergencyInformation(It.IsAny<Domain.Base.Entities.EmergencyInformation>()))
                .Returns((Domain.Base.Entities.EmergencyInformation ei) => ei);

            // Call the UpdateEmergencyInformation service, passing in the DTO with our test data built in the Initialize method
            Dtos.Base.EmergencyInformation returnEmergencyInformationDto = emergencyInformationService.UpdateEmergencyInformation(emergencyInformationDtoPerson1);

            // Verify that the dto returned by the service matches the dto it was supplied.
            // This verifies that
            // 1. The passed-in dto was converted to a domain entity successfully
            // 2. The UpdateEmergencyInformation repository method was called
            // 3. The entity returned by the repository was succesfully mapped to a dto.

            Assert.AreEqual(emergencyInformationDtoPerson1.AdditionalInformation, returnEmergencyInformationDto.AdditionalInformation);
            Assert.AreEqual(emergencyInformationDtoPerson1.ConfirmedDate, returnEmergencyInformationDto.ConfirmedDate);
            Assert.AreEqual(emergencyInformationDtoPerson1.EmergencyContacts.Count, emergencyInformationDtoPerson1.EmergencyContacts.Count);
            for (int i = 0; i < emergencyInformationDtoPerson1.EmergencyContacts.Count; i++)
            {
                var dtoPerson1EmergencyContact = emergencyInformationDtoPerson1.EmergencyContacts[i];
                var returnDtoEmergencyContact = returnEmergencyInformationDto.EmergencyContacts[i];
                Assert.AreEqual(dtoPerson1EmergencyContact.Address, returnDtoEmergencyContact.Address);
                Assert.AreEqual(dtoPerson1EmergencyContact.DaytimePhone, returnDtoEmergencyContact.DaytimePhone);
                Assert.AreEqual(dtoPerson1EmergencyContact.EffectiveDate, returnDtoEmergencyContact.EffectiveDate);
                Assert.AreEqual(dtoPerson1EmergencyContact.EveningPhone, returnDtoEmergencyContact.EveningPhone);
                Assert.AreEqual(dtoPerson1EmergencyContact.IsEmergencyContact, returnDtoEmergencyContact.IsEmergencyContact);
                Assert.AreEqual(dtoPerson1EmergencyContact.IsMissingPersonContact, returnDtoEmergencyContact.IsMissingPersonContact);
                Assert.AreEqual(dtoPerson1EmergencyContact.Name, returnDtoEmergencyContact.Name);
                Assert.AreEqual(dtoPerson1EmergencyContact.OtherPhone, returnDtoEmergencyContact.OtherPhone);
                Assert.AreEqual(dtoPerson1EmergencyContact.Relationship, returnDtoEmergencyContact.Relationship);
            }
            CollectionAssert.AreEqual(emergencyInformationDtoPerson1.HealthConditions, returnEmergencyInformationDto.HealthConditions);
            Assert.AreEqual(emergencyInformationDtoPerson1.HospitalPreference, returnEmergencyInformationDto.HospitalPreference);
            Assert.AreEqual(emergencyInformationDtoPerson1.InsuranceInformation, returnEmergencyInformationDto.InsuranceInformation);
            Assert.AreEqual(emergencyInformationDtoPerson1.PersonId, returnEmergencyInformationDto.PersonId);
        }

        [TestMethod]
        [ExpectedException(typeof(PermissionsException))]
        public async Task EmergencyInformation_GetEmergencyInformation_OtherUserWithoutPermission_ThrowsPermissionException()
        {
            // Mock repo call to other user
            var otherUserId = "S002";
            var emerInfoEntity = BuildEmerInfoEntity(otherUserId);
            emerInfoRepoMock.Setup(r => r.Get(otherUserId)).Returns(emerInfoEntity);

            // Invoke the service method
            PrivacyWrapper<Dtos.Base.EmergencyInformation> privacyWrappedEmergencyInformationDto = await emergencyInformationService.GetEmergencyInformation2Async(otherUserId);
        }

        [TestMethod]
        public async Task EmergencyInformation_GetEmergencyInformation_ForOtherUser_ReturnsEmergencyInformation()
        {
            // User should have access
            currentUserFactoryFake.HasEmergencyAccessRole = true;
            // Mock repo call to other user
            var otherUserId = "S002";
            personBaseRepositoryMock.Setup(pr => pr.GetPersonBaseAsync(otherUserId, It.IsAny<bool>())).ReturnsAsync(new PersonBase(otherUserId, "Weasley") { PreferredName = "George Weasley" });
            var emerInfoEntity = BuildEmerInfoEntity(otherUserId);
            emerInfoRepoMock.Setup(r => r.Get(otherUserId)).Returns(emerInfoEntity);
            
            // Invoke the service method
            PrivacyWrapper<Dtos.Base.EmergencyInformation> privacyWrappedEmergencyInformationDto = await emergencyInformationService.GetEmergencyInformation2Async(otherUserId);
            Dtos.Base.EmergencyInformation emergencyInformationDto = privacyWrappedEmergencyInformationDto.Dto;

            // Verify that the dto returned by the service matches the entity returned by the mocked repository Get.
            // This establishes that the repository Get was called, and that the entity-to-dto mapper worked correctly.
            Assert.AreEqual(emerInfoEntity.AdditionalInformation, emergencyInformationDto.AdditionalInformation);
            Assert.AreEqual(emerInfoEntity.ConfirmedDate, emergencyInformationDto.ConfirmedDate);
            Assert.AreEqual(emerInfoEntity.EmergencyContacts.Count, emergencyInformationDto.EmergencyContacts.Count);
            for (int i = 0; i < emergencyInformationDto.EmergencyContacts.Count; i++)
            {
                var entityEmergencyContact = emergencyInformationEntityPerson1.EmergencyContacts[i];
                var dtoEmergencyContact = emergencyInformationDto.EmergencyContacts[i];
                Assert.AreEqual(entityEmergencyContact.Address, dtoEmergencyContact.Address);
                Assert.AreEqual(entityEmergencyContact.DaytimePhone, dtoEmergencyContact.DaytimePhone);
                Assert.AreEqual(entityEmergencyContact.EffectiveDate, dtoEmergencyContact.EffectiveDate);
                Assert.AreEqual(entityEmergencyContact.EveningPhone, dtoEmergencyContact.EveningPhone);
                Assert.AreEqual(entityEmergencyContact.IsEmergencyContact, dtoEmergencyContact.IsEmergencyContact);
                Assert.AreEqual(entityEmergencyContact.IsMissingPersonContact, dtoEmergencyContact.IsMissingPersonContact);
                Assert.AreEqual(entityEmergencyContact.Name, dtoEmergencyContact.Name);
                Assert.AreEqual(entityEmergencyContact.OtherPhone, dtoEmergencyContact.OtherPhone);
                Assert.AreEqual(entityEmergencyContact.Relationship, dtoEmergencyContact.Relationship);
            }
            CollectionAssert.AreEqual(emerInfoEntity.HealthConditions, emergencyInformationDto.HealthConditions);
            Assert.AreEqual(emerInfoEntity.HospitalPreference, emergencyInformationDto.HospitalPreference);
            Assert.AreEqual(emerInfoEntity.InsuranceInformation, emergencyInformationDto.InsuranceInformation);
            Assert.AreEqual(emerInfoEntity.PersonId, emergencyInformationDto.PersonId);
        }

        private EmergencyInformation BuildEmerInfoEntity(string id)
        {

            // Create an EmergencyInformation domain entity with our test data
            var emergencyInformationEntity = new Domain.Base.Entities.EmergencyInformation(id);
            emergencyInformationEntity.AddEmergencyContact(new Domain.Base.Entities.EmergencyContact(name1)
                {
                    Address = address1,
                    DaytimePhone = daytimePhone1,
                    EffectiveDate = effectiveDate1,
                    EveningPhone = eveningPhone1,
                    IsEmergencyContact = isEmergencyContact1,
                    IsMissingPersonContact = isMissingPersonContact1,
                    OtherPhone = otherPhone1,
                    Relationship = relationship1
                });

            emergencyInformationEntity.AddEmergencyContact(new Domain.Base.Entities.EmergencyContact(name2)
            {
                Address = address2,
                DaytimePhone = daytimePhone2,
                EffectiveDate = effectiveDate2,
                EveningPhone = eveningPhone2,
                IsEmergencyContact = isEmergencyContact2,
                IsMissingPersonContact = isMissingPersonContact2,
                OtherPhone = otherPhone2,
                Relationship = relationship2
            });
            foreach (string s in healthConditions)
            {
                emergencyInformationEntity.AddHealthCondition(s);
            }
            emergencyInformationEntity.AdditionalInformation = additionalInformation;
            emergencyInformationEntity.ConfirmedDate = confirmedDate;
            emergencyInformationEntity.HospitalPreference = hospitalPreference;
            emergencyInformationEntity.InsuranceInformation = insuranceInformation;


            return emergencyInformationEntity;
        }

        // Fake an ICurrentUserFactory implementation to construct EmergencyInformationService
        public class Person001UserFactory : ICurrentUserFactory
        {
            public bool HasEmergencyAccessRole { get; set; }
            public ICurrentUser CurrentUser
            {
                get
                {
                    var roles = new List<string>() { "Student" };
                    if (HasEmergencyAccessRole)
                    {
                        roles.Add("Emergency Access");
                    }

                    return new CurrentUser(new Claims()
                    {
                        // Only the PersonId is part of the test, whether it matches the ID of the person whose 
                        // emergency information is requested. The remaining fields are arbitrary.
                        ControlId = "123",
                        Name = "Fred",
                        PersonId = personId,   /* From the test data of the test class */
                        SecurityToken = "321",
                        SessionTimeout = 30,
                        UserName = "Student",
                        Roles = roles,
                        SessionFixationId = "abc123"
                    });
                }
            }
        }   
    
    }

    
}
