// Copyright 2014-2021 Ellucian Company L.P. and its affiliates.
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
using Ellucian.Colleague.Dtos.Filters;
using Ellucian.Web.Http.Exceptions;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Colleague.Dtos;

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
        private Mock<IReferenceDataRepository> referenceDataRepositoryMock;
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
        private DateTime? effectiveDate1 = new DateTime(2012, 1, 1);
        private bool isMissingPersonContact1 = true;
        private bool isEmergencyContact1 = false;
        private string address1 = "12334 main str";

        // A second emergency contact
        private string name2 = "name 2";
        private string relationship2 = "rel 2";
        private string daytimePhone2 = "dp 2";
        private string eveningPhone2 = "ep 2";
        private string otherPhone2 = "ot 2";
        private DateTime? effectiveDate2 = new DateTime(2011, 2, 1);
        private bool isMissingPersonContact2 = false;
        private bool isEmergencyContact2 = true;
        private string address2 = "4444 elm str";

        // Other test values
        private string insuranceInformation = "My Ins info";
        private string hospitalPreference = "My hospital pref";
        private List<string> healthConditions = new List<string>() { "cond1", "cond2" };
        private DateTime? confirmedDate = new DateTime(2014, 2, 3);
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
            var emergencyContactAccessRole = new Domain.Entities.Role(3, "Emergency Contact Access");
            var emergencyHealthAccessRole = new Domain.Entities.Role(4, "Emergency Health Access");
            var emergencyOtherAccessRole = new Domain.Entities.Role(5, "Emergency Other Access");
            emergencyContactAccessRole.AddPermission(new Domain.Entities.Permission(Domain.Base.BasePermissionCodes.ViewPersonEmergencyContacts));
            emergencyHealthAccessRole.AddPermission(new Domain.Entities.Permission(Domain.Base.BasePermissionCodes.ViewPersonHealthConditions));
            emergencyOtherAccessRole.AddPermission(new Domain.Entities.Permission(Domain.Base.BasePermissionCodes.ViewPersonOtherEmergencyInformation));
            roleRepoMock.Setup(rr => rr.Roles).Returns(new List<Domain.Entities.Role> { emergencyAccessRole, studentRole, emergencyContactAccessRole, emergencyHealthAccessRole, emergencyOtherAccessRole });
            roleRepo = roleRepoMock.Object;
            currentUserFactoryFake = new Person001UserFactory();
            configurationRepositoryMock = new Mock<IConfigurationRepository>();
            referenceDataRepositoryMock = new Mock<IReferenceDataRepository>();
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
                    emerInfoRepo, configurationRepositoryMock.Object, personBaseRepositoryMock.Object, staffRepository, personRepositoryMock.Object, referenceDataRepositoryMock.Object);

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

        [TestMethod]
        public async Task EmergencyInformation_GetEmergencyInformation_WithContactPermission_ReturnsEmergencyContactInformation()
        {
            // User should have only contact access
            currentUserFactoryFake.HasEmergencyContactAccessRole = true;

            var otherUserId = "S002";
            personBaseRepositoryMock.Setup(pr => pr.GetPersonBaseAsync(otherUserId, It.IsAny<bool>())).ReturnsAsync(new PersonBase(otherUserId, "Ipsum") { PreferredName = "Laurel Ipsum" });
            var emerInfoEntity = BuildEmerInfoEntity(otherUserId);
            emerInfoRepoMock.Setup(r => r.Get(otherUserId)).Returns(emerInfoEntity);

            PrivacyWrapper<Dtos.Base.EmergencyInformation> privacyWrappedEmergencyInformationDto = await emergencyInformationService.GetEmergencyInformation2Async(otherUserId);
            Dtos.Base.EmergencyInformation emergencyInformationDto = privacyWrappedEmergencyInformationDto.Dto;

            Assert.IsTrue(privacyWrappedEmergencyInformationDto.HasPrivacyRestrictions);
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
            Assert.IsNull(emergencyInformationDto.HealthConditions);
            Assert.IsNull(emergencyInformationDto.HospitalPreference);
            Assert.IsNull(emergencyInformationDto.InsuranceInformation);
            Assert.IsNull(emergencyInformationDto.AdditionalInformation);
            Assert.AreEqual(emerInfoEntity.PersonId, emergencyInformationDto.PersonId);
        }

        [TestMethod]
        public async Task EmergencyInformation_GetEmergencyInformation_WithHealthPermission_ReturnsEmergencyHealthInformation()
        {
            // User should have only health information access
            currentUserFactoryFake.HasEmergencyHealthAccessRole = true;

            var otherUserId = "S002";
            personBaseRepositoryMock.Setup(pr => pr.GetPersonBaseAsync(otherUserId, It.IsAny<bool>())).ReturnsAsync(new PersonBase(otherUserId, "Weasley") { PreferredName = "George Weasley" });
            var emerInfoEntity = BuildEmerInfoEntity(otherUserId);
            emerInfoRepoMock.Setup(r => r.Get(otherUserId)).Returns(emerInfoEntity);

            PrivacyWrapper<Dtos.Base.EmergencyInformation> privacyWrappedEmergencyInformationDto = await emergencyInformationService.GetEmergencyInformation2Async(otherUserId);
            Dtos.Base.EmergencyInformation emergencyInformationDto = privacyWrappedEmergencyInformationDto.Dto;

            Assert.AreEqual(emerInfoEntity.ConfirmedDate, emergencyInformationDto.ConfirmedDate);
            CollectionAssert.AreEqual(emerInfoEntity.HealthConditions, emergencyInformationDto.HealthConditions);
            Assert.IsNull(emergencyInformationDto.EmergencyContacts);
            Assert.IsNull(emergencyInformationDto.HospitalPreference);
            Assert.IsNull(emergencyInformationDto.InsuranceInformation);
            Assert.IsNull(emergencyInformationDto.AdditionalInformation);
            Assert.AreEqual(emerInfoEntity.PersonId, emergencyInformationDto.PersonId);
        }

        [TestMethod]
        public async Task EmergencyInformation_GetEmergencyInformation_WithOtherPermission_ReturnsEmergencyOtherInformation()
        {
            // User should have only other information access
            currentUserFactoryFake.HasEmergencyOtherAccessRole = true;

            var otherUserId = "S002";
            personBaseRepositoryMock.Setup(pr => pr.GetPersonBaseAsync(otherUserId, It.IsAny<bool>())).ReturnsAsync(new PersonBase(otherUserId, "Weasley") { PreferredName = "George Weasley" });
            var emerInfoEntity = BuildEmerInfoEntity(otherUserId);
            emerInfoRepoMock.Setup(r => r.Get(otherUserId)).Returns(emerInfoEntity);

            PrivacyWrapper<Dtos.Base.EmergencyInformation> privacyWrappedEmergencyInformationDto = await emergencyInformationService.GetEmergencyInformation2Async(otherUserId);
            Dtos.Base.EmergencyInformation emergencyInformationDto = privacyWrappedEmergencyInformationDto.Dto;

            Assert.AreEqual(emerInfoEntity.AdditionalInformation, emergencyInformationDto.AdditionalInformation);
            Assert.AreEqual(emerInfoEntity.ConfirmedDate, emergencyInformationDto.ConfirmedDate);
            Assert.IsNull(emergencyInformationDto.EmergencyContacts);
            Assert.IsNull(emergencyInformationDto.HealthConditions);
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
            public bool HasEmergencyContactAccessRole { get; set; }
            public bool HasEmergencyHealthAccessRole { get; set; }
            public bool HasEmergencyOtherAccessRole { get; set; }

            public ICurrentUser CurrentUser
            {
                get
                {
                    var roles = new List<string>() { "Student" };
                    if (HasEmergencyAccessRole)
                    {
                        roles.Add("Emergency Access");
                    }
                    if (HasEmergencyContactAccessRole)
                    {
                        roles.Add("Emergency Contact Access");
                    }
                    if (HasEmergencyHealthAccessRole)
                    {
                        roles.Add("Emergency Health Access");
                    }
                    if (HasEmergencyOtherAccessRole)
                    {
                        roles.Add("Emergency Other Access");
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

    #region person-emergency-contacts Unit tests

    [TestClass]
    public class EmergencyInformationServiceHEDMTests
    {
        private EmergencyInformationService emergencyInformationService;

        // Mock/fake objects to construct EmergencyInformationService
        private Mock<IEmergencyInformationRepository> emerInfoRepoMock;
        private IEmergencyInformationRepository emerInfoRepo;
        private Mock<IPersonBaseRepository> personBaseRepositoryMock;
        private Mock<IAdapterRegistry> adapterRegistryMock;
        private IAdapterRegistry adapterRegistry;
        private ILogger logger;
        private Person001UserFactory currentUserFactoryFake;
        private Mock<IRoleRepository> roleRepoMock;
        private Mock<IConfigurationRepository> configurationRepositoryMock;
        private Mock<IReferenceDataRepository> referenceDataRepositoryMock;
        private Mock<IStaffRepository> staffRepositoryMock;
        private IStaffRepository staffRepository;
        private Mock<IPersonRepository> personRepositoryMock;

        List<Domain.Base.Entities.PersonContact> entities = new List<Domain.Base.Entities.PersonContact>();
        List<Domain.Base.Entities.PersonContact> entitiesWithNoData = new List<Domain.Base.Entities.PersonContact>();
        Tuple<IEnumerable<Domain.Base.Entities.PersonContact>, int> tupleWithNodata;
        List<Dtos.PersonEmergencyContacts> personEmerContactDtos;
        PersonFilterFilter2 personFilterFilter2;
        Dtos.PersonEmergencyContacts personFilter;
        private Domain.Entities.Permission permissionViewAnyPersonContact;
        protected Domain.Entities.Role _viewContactRole = new Domain.Entities.Role(1, "VIEW.PERSON.CONTACT");
        protected Domain.Entities.Role _updateContactRole = new Domain.Entities.Role(1, "UPDATE.PERSON.CONTACT");

        private const string guid = "7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc";
        private const string personGuid = "da8549d0-7271-46cf-8159-cb0ac5cd74b6";

        // Emergency information data for one person for tests
        private static string personId = "S001";

      

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
            currentUserFactoryFake = new Person001UserFactory();
            // Mock permissions
            permissionViewAnyPersonContact = new Ellucian.Colleague.Domain.Entities.Permission(Domain.Base.BasePermissionCodes.ViewAnyPersonContact);
            _viewContactRole.AddPermission(permissionViewAnyPersonContact);
            configurationRepositoryMock = new Mock<IConfigurationRepository>();
            referenceDataRepositoryMock = new Mock<IReferenceDataRepository>();
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


            BuildAndMockData(); 

            // Instantiate the service
            emergencyInformationService = new EmergencyInformationService(adapterRegistry, currentUserFactoryFake, roleRepoMock.Object, logger,
                    emerInfoRepo, configurationRepositoryMock.Object, personBaseRepositoryMock.Object, staffRepository, personRepositoryMock.Object, referenceDataRepositoryMock.Object);

        }

        // Fake an ICurrentUserFactory implementation to construct EmergencyInformationService
        public class Person001UserFactory : ICurrentUserFactory
        {
            public ICurrentUser CurrentUser
            {
                get
                {
                                      
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
                        Roles = new List<string>() { "VIEW.PERSON.CONTACT", "UPDATE.PERSON.CONTACT" },
                        SessionFixationId = "abc123"
                    });
                }
            }
        }
        private void BuildAndMockData()
        {
            // role repo
            //var emergencyAccessRole = new Domain.Entities.Role(1, "Emergency Access");
            _viewContactRole.AddPermission(new Domain.Entities.Permission(Domain.Base.BasePermissionCodes.ViewAnyPersonContact));
            roleRepoMock.Setup(rr => rr.Roles).Returns(new List<Domain.Entities.Role> { _viewContactRole });  

            // Create an personContact domain entity with our test data

            entities = new List<Domain.Base.Entities.PersonContact>()
            {
                new Domain.Base.Entities.PersonContact(guid,personId, personId)
                {
                     PersonContactDetails = new List<PersonContactDetails>()
                     {
                         new PersonContactDetails()
                         {
                             ContactName = "Mike M Myers",
                             ContactFlag = "Y",
                             MissingContactFlag = "Y",
                            DaytimePhone = "+1 111-111-1111 x 1",
                            EveningPhone = "+2 222-222-2222 x 2",
                            OtherPhone = "+3 333-333-3333 x 3",
                            Relationship = "Uncle",
                            Guid = guid
                         }
                     }
                }
             };

            Tuple<IEnumerable<Domain.Base.Entities.PersonContact>, int> tuple = new Tuple<IEnumerable<Domain.Base.Entities.PersonContact>, int>(entities, entities.Count());
            tupleWithNodata = new Tuple<IEnumerable<Domain.Base.Entities.PersonContact>, int>(entitiesWithNoData, entitiesWithNoData.Count());
            personEmerContactDtos = new List<Dtos.PersonEmergencyContacts>() {
                new Dtos.PersonEmergencyContacts()
                {
                    Person = new Dtos.GuidObject2(personGuid),
                    Contact = new Dtos.DtoProperties.PersonEmergencyContactsContact()
                    {
                        Name = new Dtos.PersonContactName()
                        {
                            FirstName = "Mike",
                            MiddleName = "M",
                            LastName = "Myers",
                            FullName = "Mike M Myers"
                        },
                        Types = new List<Dtos.GuidObject2>
                        {
                            new Dtos.GuidObject2("emerGuid"),
                            new Dtos.GuidObject2("MissGuid")
                        },
                        Phones = new List<Dtos.DtoProperties.PersonEmergencyContactsPhones>
                        {
                            new Dtos.DtoProperties.PersonEmergencyContactsPhones
                            {
                                ContactAvailability = new Dtos.GuidObject2("dayGuid"),
                                CountryCallingCode = "1",
                                Number = "111-111-1111",
                                Extension = "1"
                            },
                            new Dtos.DtoProperties.PersonEmergencyContactsPhones
                            {
                                ContactAvailability = new Dtos.GuidObject2("eveGuid"),
                                CountryCallingCode = "2",
                                Number = "222-222-2222",
                                Extension = "2"
                            },
                            new Dtos.DtoProperties.PersonEmergencyContactsPhones
                            {
                                ContactAvailability = new Dtos.GuidObject2("othGuid"),
                                CountryCallingCode = "3",
                                Number = "333-333-3333",
                                Extension = "4"
                            }
                        },
                        Relationship = new Dtos.DtoProperties.PersonEmergencyContactsRelationship()
                        {
                            Type = "Uncle",
                            Detail = new Dtos.GuidObject2("RelaGuid")
                        }
                    }
                }
            };
            personFilterFilter2 = new PersonFilterFilter2()
            {
                personFilter = new Dtos.GuidObject2("b07ab144-cb26-4b4a-a098-82d034b6e41b")
            };
            personFilter = new Dtos.PersonEmergencyContacts()
            {
                Person = new Dtos.GuidObject2(personGuid)
            };
            //repo mock
            emerInfoRepoMock.Setup(repo => repo.GetPersonContacts2Async(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>(),
              It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string[]>())).ReturnsAsync(tuple);
            emerInfoRepoMock.Setup(repo => repo.GetPersonContactById2Async(It.IsAny<string>())).ReturnsAsync(entities.FirstOrDefault());
            //emerInfoRepoMock.Setup(repo => repo.DeletePersonEmergencyContactsAsync(entities.FirstOrDefault()));

            //person guid collection mock
            Dictionary<string, string> dict = new Dictionary<string, string>();
            dict.Add(personId, personGuid);
            personRepositoryMock.Setup(repo => repo.GetPersonGuidsCollectionAsync(It.IsAny<IEnumerable<string>>())).ReturnsAsync(dict);
            personRepositoryMock.Setup(repo => repo.GetPersonIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync("1");

            var _emergencyContactTypesCollection = new List<IntgPersonEmerTypes>()
                {
                    new IntgPersonEmerTypes("emerGuid", "EMER", "Emergency Contact"),
                    new IntgPersonEmerTypes("missGuid", "MISS", "Missing Person Contact"),
                };

            referenceDataRepositoryMock.Setup(repo => repo.GetIntgPersonEmerTypesAsync(It.IsAny<bool>()))
                .ReturnsAsync(_emergencyContactTypesCollection);

            var _emergencyContactPhoneAvailabilitiesCollection = new List<IntgPersonEmerPhoneTypes>()
                {
                    new IntgPersonEmerPhoneTypes("dayGuid", "DAY", "Day Phone"),
                    new IntgPersonEmerPhoneTypes("eveGuid", "EVE", "Evening Phone"),
                    new IntgPersonEmerPhoneTypes("othGuid", "OTH", "Other Phone")
                };


            referenceDataRepositoryMock.Setup(repo => repo.GetIntgPersonEmerPhoneTypesAsync(It.IsAny<bool>()))
                .ReturnsAsync(_emergencyContactPhoneAvailabilitiesCollection);

            var allRelationTypes = new List<Domain.Base.Entities.RelationType>()
                {
                    new Domain.Base.Entities.RelationType("7989a936-f41d-4c08-9fda-dd41314a9e34", "Parent", "P", "", Domain.Base.Entities.PersonalRelationshipType.Parent, Domain.Base.Entities.PersonalRelationshipType.Father, Domain.Base.Entities.PersonalRelationshipType.Mother, "Child"),
                    new Domain.Base.Entities.RelationType("2c27b01e-fb4e-4884-aece-77dbfce45250", "Child", "C", "", Domain.Base.Entities.PersonalRelationshipType.Child, Domain.Base.Entities.PersonalRelationshipType.Son, Domain.Base.Entities.PersonalRelationshipType.Daughter, "Parent"),
                    new Domain.Base.Entities.RelationType("8c27b01e-fb4e-4884-aece-77dbfce45259", "Affiliated", "A", "", Domain.Base.Entities.PersonalRelationshipType.Other, Domain.Base.Entities.PersonalRelationshipType.Other, Domain.Base.Entities.PersonalRelationshipType.Other, "Other")
                };

            referenceDataRepositoryMock.Setup(i => i.GetRelationTypesAsync(It.IsAny<bool>())).ReturnsAsync(allRelationTypes);
            referenceDataRepositoryMock.Setup(i => i.GetPersonIdsByPersonFilterGuidAsync(It.IsAny<string>())).ReturnsAsync(new string[] { personId });

        }

        [TestCleanup]
        public void Cleanup()
        {
            emerInfoRepo = null;
            adapterRegistry = null;
            logger = null;
            roleRepoMock = null;
            currentUserFactoryFake = null;
            emergencyInformationService = null;
        }

        #region Get All Testing
        [TestMethod]
        public async Task EmergencyInformationService_GetPersonEmergencyContacts2Async()
        {
            var results = await emergencyInformationService.GetPersonEmergencyContacts2Async(It.IsAny<int>(), It.IsAny<int>(), personEmerContactDtos.FirstOrDefault(),
                "personFilter", It.IsAny<bool>());
            Assert.IsNotNull(results);
        }

        //[TestMethod]
        //[ExpectedException(typeof(PermissionsException))]
        //public async Task EmergencyInformationService_GetPersonEmergencyContacts2Asyn_PermissionException()
        //{
        //    roleRepoMock.Setup(rr => rr.Roles).Returns(new List<Domain.Entities.Role>() { new Domain.Entities.Role(1, "VIEW.PERSON") });
        //    var results = await emergencyInformationService.GetPersonEmergencyContacts2Async(It.IsAny<int>(), It.IsAny<int>(), personEmerContactDtos.FirstOrDefault(),
        //        "personFilter", It.IsAny<bool>());
           
        //}

        [TestMethod]
        public async Task EmergencyInformationService_GetPersonEmergencyContacts2Async_Person_Filter_NamedQuery_EmptyResult()
        {
            referenceDataRepositoryMock.Setup(repo => repo.GetPersonIdsByPersonFilterGuidAsync(It.IsAny<string>())).ReturnsAsync(() => null);
            var results = await emergencyInformationService.GetPersonEmergencyContacts2Async(It.IsAny<int>(), It.IsAny<int>(), personEmerContactDtos.FirstOrDefault(),
            "personFilter", It.IsAny<bool>());
            Assert.IsNotNull(results);
            Assert.AreEqual(0, results.Item2);
        }

        [TestMethod]
        public async Task EmergencyInformationService_GetPersonEmergencyContacts2Async_Person_Filter_NamedQuery_Exception_EmptyResult()
        {
            referenceDataRepositoryMock.Setup(repo => repo.GetPersonIdsByPersonFilterGuidAsync(It.IsAny<string>())).ThrowsAsync(new Exception());
            var results = await emergencyInformationService.GetPersonEmergencyContacts2Async(It.IsAny<int>(), It.IsAny<int>(), personEmerContactDtos.FirstOrDefault(),
           "personFilter", It.IsAny<bool>());
            Assert.IsNotNull(results);
            Assert.AreEqual(0, results.Item2);
        }

        [TestMethod]
        public async Task EmergencyInformationService_GetPersonEmergencyContacts2Async_Person_Exception_EmptyResult()
        {
            personRepositoryMock.Setup(repo => repo.GetPersonIdFromGuidAsync(It.IsAny<string>())).ThrowsAsync(new Exception());
            var results = await emergencyInformationService.GetPersonEmergencyContacts2Async(It.IsAny<int>(), It.IsAny<int>(), personEmerContactDtos.FirstOrDefault(),
           "personFilter", It.IsAny<bool>());
            Assert.IsNotNull(results);
            Assert.AreEqual(0, results.Item2);
        }

        [TestMethod]
        public async Task EmergencyInformationService_GetPersonEmergencyContacts2Async_Repo_Empty()
        {
            emerInfoRepoMock.Setup(repo => repo.GetPersonContacts2Async(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>(),
             It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string[]>())).ReturnsAsync(() => null);
            var results = await emergencyInformationService.GetPersonEmergencyContacts2Async(It.IsAny<int>(), It.IsAny<int>(), personEmerContactDtos.FirstOrDefault(),
           "personFilter", It.IsAny<bool>());
            Assert.IsNotNull(results);
            Assert.AreEqual(0, results.Item2);
        }

        [TestMethod]
        [ExpectedException(typeof(IntegrationApiException))]
        public async Task EmergencyInformationService_GetPersonEmergencyContacts2Async_Repo_Exception()
        {
            emerInfoRepoMock.Setup(repo => repo.GetPersonContacts2Async(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>(),
             It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string[]>())).ThrowsAsync(new RepositoryException());
            var results = await emergencyInformationService.GetPersonEmergencyContacts2Async(It.IsAny<int>(), It.IsAny<int>(), personEmerContactDtos.FirstOrDefault(),
           "personFilter", It.IsAny<bool>());
        }


        [TestMethod]
        [ExpectedException(typeof(Exception))]
        public async Task EmergencyInformationService_GetPersonEmergencyContacts2Async_Exception()
        {
            emerInfoRepoMock.Setup(repo => repo.GetPersonContacts2Async(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>(),
             It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string[]>())).ThrowsAsync(new Exception());
            var results = await emergencyInformationService.GetPersonEmergencyContacts2Async(It.IsAny<int>(), It.IsAny<int>(), personEmerContactDtos.FirstOrDefault(),
           "personFilter", It.IsAny<bool>());
        }

        [TestMethod]
        public async Task EmergencyInformationService_GetPersonEmergencyContacts2Async_Repo_Empty_Tuple()
        {
            emerInfoRepoMock.Setup(repo => repo.GetPersonContacts2Async(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>(),
             It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string[]>())).ReturnsAsync(new Tuple<IEnumerable<Domain.Base.Entities.PersonContact>, int>(null, 0));
            var results = await emergencyInformationService.GetPersonEmergencyContacts2Async(It.IsAny<int>(), It.IsAny<int>(), personEmerContactDtos.FirstOrDefault(),
           "personFilter", It.IsAny<bool>());
            Assert.IsNotNull(results);
            Assert.AreEqual(0, results.Item2);
        }



        [TestMethod]
        [ExpectedException(typeof(IntegrationApiException))]
        public async Task EmergencyInformationService_GetPersonEmergencyContacts2Async_Entity_No_Guid()
        {
            try
            {
                entities.FirstOrDefault().PersonContactDetails.FirstOrDefault().Guid = String.Empty;
                var results = await emergencyInformationService.GetPersonEmergencyContacts2Async(It.IsAny<int>(), It.IsAny<int>(), personEmerContactDtos.FirstOrDefault(),
               "personFilter", It.IsAny<bool>());
            }
            catch (IntegrationApiException ex)
            {
                Assert.IsNotNull(ex);
                Assert.IsTrue(ex.Errors.Count == 1);
                Assert.IsTrue(ex.Errors[0].Message == "Unable to locate GUID");
                throw ex;
            }
        }

        [TestMethod]
        [ExpectedException(typeof(IntegrationApiException))]
        public async Task EmergencyInformationService_GetPersonEmergencyContacts2Async_Entity_No_Person_Guid_Collection()
        {
            try
            {
                Dictionary<string, string> dict = new Dictionary<string, string>();
                dict.Add(personId, personGuid);
                personRepositoryMock.Setup(repo => repo.GetPersonGuidsCollectionAsync(It.IsAny<IEnumerable<string>>())).ReturnsAsync(() => null);
                var results = await emergencyInformationService.GetPersonEmergencyContacts2Async(It.IsAny<int>(), It.IsAny<int>(), personEmerContactDtos.FirstOrDefault(),
               "personFilter", It.IsAny<bool>());
            }
            catch (IntegrationApiException ex)
            {
                Assert.IsNotNull(ex);
                Assert.IsTrue(ex.Errors.Count == 1);
                Assert.IsTrue(ex.Errors[0].Message.Contains("Unable to locate GUID for person Id")) ;
                throw ex;
            }
        }

        [TestMethod]
        [ExpectedException(typeof(IntegrationApiException))]
        public async Task EmergencyInformationService_GetPersonEmergencyContacts2Async_Entity_No_Person_Guid()
        {
            try
            {
                Dictionary<string, string> dict = new Dictionary<string, string>();
                dict.Add("1", personGuid);
                personRepositoryMock.Setup(repo => repo.GetPersonGuidsCollectionAsync(It.IsAny<IEnumerable<string>>())).ReturnsAsync(dict);
                var results = await emergencyInformationService.GetPersonEmergencyContacts2Async(It.IsAny<int>(), It.IsAny<int>(), personEmerContactDtos.FirstOrDefault(),
               "personFilter", It.IsAny<bool>());
            }
            catch (IntegrationApiException ex)
            {
                Assert.IsNotNull(ex);
                Assert.IsTrue(ex.Errors.Count == 1);
                Assert.IsTrue(ex.Errors[0].Message.Contains("Unable to locate GUID for person Id"));
                throw ex;
            }
        }

        [TestMethod]
        [ExpectedException(typeof(IntegrationApiException))]
        public async Task EmergencyInformationService_GetPersonEmergencyContacts2Async_Entity_No_Contact_Name()
        {
            try
            {
                entities.FirstOrDefault().PersonContactDetails.FirstOrDefault().ContactName = String.Empty;
                var results = await emergencyInformationService.GetPersonEmergencyContacts2Async(It.IsAny<int>(), It.IsAny<int>(), personEmerContactDtos.FirstOrDefault(),
               "personFilter", It.IsAny<bool>());
            }
            catch (IntegrationApiException ex)
            {
                Assert.IsNotNull(ex);
                Assert.IsTrue(ex.Errors.Count == 1);
                Assert.IsTrue(ex.Errors[0].Message.Contains("Contact Name is required."));
                throw ex;
            }
        }


        [TestMethod]
        [ExpectedException(typeof(IntegrationApiException))]
        public async Task EmergencyInformationService_GetPersonEmergencyContacts2Async_Entity_No_EmerTypes()
        {
            try
            {
                referenceDataRepositoryMock.Setup(repo => repo.GetIntgPersonEmerTypesAsync(It.IsAny<bool>())).ReturnsAsync(() => null);
                var results = await emergencyInformationService.GetPersonEmergencyContacts2Async(It.IsAny<int>(), It.IsAny<int>(), personEmerContactDtos.FirstOrDefault(),
               "personFilter", It.IsAny<bool>());
            }
            catch (IntegrationApiException ex)
            {
                Assert.IsNotNull(ex);
                Assert.IsTrue(ex.Errors.Count == 1);
                Assert.IsTrue(ex.Errors[0].Message.Contains("Unable to locate emergency-contact-types"));
                throw ex;
            }
        }

        [TestMethod]
        [ExpectedException(typeof(IntegrationApiException))]
        public async Task EmergencyInformationService_GetPersonEmergencyContacts2Async_Entity_Invalid_EmerTypes()
        {
            try
            {
                var _emergencyContactTypesCollection = new List<IntgPersonEmerTypes>()
                {
                    new IntgPersonEmerTypes("missGuid", "MISS", "Missing Person Contact")
                };

                referenceDataRepositoryMock.Setup(repo => repo.GetIntgPersonEmerTypesAsync(It.IsAny<bool>()))
                    .ReturnsAsync(_emergencyContactTypesCollection);
                var results = await emergencyInformationService.GetPersonEmergencyContacts2Async(It.IsAny<int>(), It.IsAny<int>(), personEmerContactDtos.FirstOrDefault(),
               "personFilter", It.IsAny<bool>());
            }
            catch (IntegrationApiException ex)
            {
                Assert.IsNotNull(ex);
                Assert.IsTrue(ex.Errors.Count == 1);
                Assert.IsTrue(ex.Errors[0].Message.Contains("Unable to locate GUid for emergency-contact-types of EMER"));
                throw ex;
            }
        }

        [TestMethod]
        [ExpectedException(typeof(IntegrationApiException))]
        public async Task EmergencyInformationService_GetPersonEmergencyContacts2Async_Entity_Invalid_Miss_EmerTypes()
        {
            try
            {
                var _emergencyContactTypesCollection = new List<IntgPersonEmerTypes>()
                {
                      new IntgPersonEmerTypes("emerGuid", "EMER", "Emergency Contact")
                };

                referenceDataRepositoryMock.Setup(repo => repo.GetIntgPersonEmerTypesAsync(It.IsAny<bool>()))
                    .ReturnsAsync(_emergencyContactTypesCollection);
                var results = await emergencyInformationService.GetPersonEmergencyContacts2Async(It.IsAny<int>(), It.IsAny<int>(), personEmerContactDtos.FirstOrDefault(),
               "personFilter", It.IsAny<bool>());
            }
            catch (IntegrationApiException ex)
            {
                Assert.IsNotNull(ex);
                Assert.IsTrue(ex.Errors.Count == 1);
                Assert.IsTrue(ex.Errors[0].Message.Contains("Unable to locate GUid for emergency-contact-types of MISS"));
                throw ex;
            }
        }

        [TestMethod]
        [ExpectedException(typeof(IntegrationApiException))]
        public async Task EmergencyInformationService_GetPersonEmergencyContacts2Async_Entity_Invalid_EmerPhones()
        {
            try
            {
                referenceDataRepositoryMock.Setup(repo => repo.GetIntgPersonEmerPhoneTypesAsync(It.IsAny<bool>()))
                .ReturnsAsync(() => null);
                var results = await emergencyInformationService.GetPersonEmergencyContacts2Async(It.IsAny<int>(), It.IsAny<int>(), personEmerContactDtos.FirstOrDefault(),
               "personFilter", It.IsAny<bool>());
            }
            catch (IntegrationApiException ex)
            {
                Assert.IsNotNull(ex);
                Assert.IsTrue(ex.Errors.Count == 1);
                Assert.IsTrue(ex.Errors[0].Message.Contains("Unable to locate emergency-contact-phone-availabilities"));
                throw ex;
            }
        }

        [TestMethod]
        [ExpectedException(typeof(IntegrationApiException))]
        public async Task EmergencyInformationService_GetPersonEmergencyContacts2Async_Entity_Invalid_Day_EmerPhones()
        {
            try
            {
                var _emergencyContactPhoneAvailabilitiesCollection = new List<IntgPersonEmerPhoneTypes>()
                {
                    new IntgPersonEmerPhoneTypes("eveGuid", "EVE", "Evening Phone"),
                    new IntgPersonEmerPhoneTypes("othGuid", "OTH", "Other Phone")
                };


                referenceDataRepositoryMock.Setup(repo => repo.GetIntgPersonEmerPhoneTypesAsync(It.IsAny<bool>()))
                    .ReturnsAsync(_emergencyContactPhoneAvailabilitiesCollection);
                var results = await emergencyInformationService.GetPersonEmergencyContacts2Async(It.IsAny<int>(), It.IsAny<int>(), personEmerContactDtos.FirstOrDefault(),
               "personFilter", It.IsAny<bool>());
            }
            catch (IntegrationApiException ex)
            {
                Assert.IsNotNull(ex);
                Assert.IsTrue(ex.Errors.Count == 1);
                Assert.IsTrue(ex.Errors[0].Message.Contains("Unable to locate GUid for emergency-contact-phone-availabilities of DAY"));
                throw ex;
            }
        }


        [TestMethod]
        [ExpectedException(typeof(IntegrationApiException))]
        public async Task EmergencyInformationService_GetPersonEmergencyContacts2Async_Entity_Invalid_Oth_EmerPhones()
        {
            try
            {
                var _emergencyContactPhoneAvailabilitiesCollection = new List<IntgPersonEmerPhoneTypes>()
                {
                     new IntgPersonEmerPhoneTypes("dayGuid", "DAY", "Day Phone"),
                    new IntgPersonEmerPhoneTypes("eveGuid", "EVE", "Evening Phone")
                };


                referenceDataRepositoryMock.Setup(repo => repo.GetIntgPersonEmerPhoneTypesAsync(It.IsAny<bool>()))
                    .ReturnsAsync(_emergencyContactPhoneAvailabilitiesCollection);
                var results = await emergencyInformationService.GetPersonEmergencyContacts2Async(It.IsAny<int>(), It.IsAny<int>(), personEmerContactDtos.FirstOrDefault(),
               "personFilter", It.IsAny<bool>());
            }
            catch (IntegrationApiException ex)
            {
                Assert.IsNotNull(ex);
                Assert.IsTrue(ex.Errors.Count == 1);
                Assert.IsTrue(ex.Errors[0].Message.Contains("Unable to locate GUid for emergency-contact-phone-availabilities of OTH"));
                throw ex;
            }
        }



        [TestMethod]
        [ExpectedException(typeof(IntegrationApiException))]
        public async Task EmergencyInformationService_GetPersonEmergencyContacts2Async_Entity_Invalid_Eve_EmerPhones()
        {
            try
            {
                var _emergencyContactPhoneAvailabilitiesCollection = new List<IntgPersonEmerPhoneTypes>()
                {
                    new IntgPersonEmerPhoneTypes("dayGuid", "DAY", "Day Phone"),
                    new IntgPersonEmerPhoneTypes("othGuid", "OTH", "Other Phone")
                };


                referenceDataRepositoryMock.Setup(repo => repo.GetIntgPersonEmerPhoneTypesAsync(It.IsAny<bool>()))
                    .ReturnsAsync(_emergencyContactPhoneAvailabilitiesCollection);
                var results = await emergencyInformationService.GetPersonEmergencyContacts2Async(It.IsAny<int>(), It.IsAny<int>(), personEmerContactDtos.FirstOrDefault(),
               "personFilter", It.IsAny<bool>());
            }
            catch (IntegrationApiException ex)
            {
                Assert.IsNotNull(ex);
                Assert.IsTrue(ex.Errors.Count == 1);
                Assert.IsTrue(ex.Errors[0].Message.Contains("Unable to locate GUid for emergency-contact-phone-availabilities of EVE"));
                throw ex;
            }
        }

        #endregion

        #region Person Emergency Contacts GetbyId testing

        [TestMethod]
        public async Task EmergencyInformationService_GetPersonEmergencyContactsByGuid2Async()
        {
            var results = await emergencyInformationService.GetPersonEmergencyContactsByGuid2Async(guid, It.IsAny<bool>());
            Assert.IsNotNull(results);
        }

        [TestMethod]
        [ExpectedException(typeof(IntegrationApiException))]
        public async Task EmergencyInformationService_GetPersonEmergencyContactsByGuid2Async_No_Guid()
        {
            try
            {
                var results = await emergencyInformationService.GetPersonEmergencyContactsByGuid2Async(It.IsAny<string>(), It.IsAny<bool>());
            }
            catch (IntegrationApiException ex)
            {
                Assert.IsNotNull(ex);
                Assert.IsTrue(ex.Errors.Count == 1);
                Assert.IsTrue(ex.Errors[0].Message.Contains("GUID is required to get a person emergency contact"));
                throw ex;
            }
        }

        //[TestMethod]
        //[ExpectedException(typeof(PermissionsException))]
        //public async Task EmergencyInformationService_GetPersonEmergencyContactsByGuid2Async_PermissionException()
        //{
        //    roleRepoMock.Setup(rr => rr.Roles).Returns(new List<Domain.Entities.Role>() { new Domain.Entities.Role(1, "VIEW.PERSON")});
        //    //roleRepoMock.Setup(i => i.GetRolesAsync()).ReturnsAsync(new List<Domain.Entities.Role>() { });
        //    var results = await emergencyInformationService.GetPersonEmergencyContactsByGuid2Async(guid, It.IsAny<bool>());
        //}


        [TestMethod]
        [ExpectedException(typeof(IntegrationApiException))]
        public async Task EmergencyInformationService_GetPersonEmergencyContactsByGuid2Async_Repo_Exception()
        {
            emerInfoRepoMock.Setup(repo => repo.GetPersonContactById2Async(It.IsAny<string>())).ThrowsAsync(new RepositoryException());
            var results = await emergencyInformationService.GetPersonEmergencyContactsByGuid2Async(guid, It.IsAny<bool>());
        }


        [TestMethod]
        [ExpectedException(typeof(Exception))]
        public async Task EmergencyInformationService_GetPersonEmergencyContactsByGuid2Async_Exception()
        {
            emerInfoRepoMock.Setup(repo => repo.GetPersonContactById2Async(It.IsAny<string>())).ThrowsAsync(new Exception());
            var results = await emergencyInformationService.GetPersonEmergencyContactsByGuid2Async(guid, It.IsAny<bool>());
        }

        [TestMethod]
        [ExpectedException(typeof(KeyNotFoundException))]
        public async Task EmergencyInformationService_GetPersonEmergencyContactsByGuid2Async_KeyNotFoundException()
        {
            emerInfoRepoMock.Setup(repo => repo.GetPersonContactById2Async(It.IsAny<string>())).ThrowsAsync(new KeyNotFoundException());
            var results = await emergencyInformationService.GetPersonEmergencyContactsByGuid2Async(guid, It.IsAny<bool>());
        }

        [TestMethod]
        [ExpectedException(typeof(KeyNotFoundException))]
        public async Task EmergencyInformationService_GetPersonEmergencyContactsByGuid2Async_Null()
        {
            emerInfoRepoMock.Setup(repo => repo.GetPersonContactById2Async(It.IsAny<string>())).ReturnsAsync(() => null);
            var results = await emergencyInformationService.GetPersonEmergencyContactsByGuid2Async(guid, It.IsAny<bool>());
        }

        #endregion

        #region Delete Unit Tests

        [TestMethod]
        public async Task EmergencyInformationService_DeletePersonEmergencyContactsAsync()
        {
            _viewContactRole.AddPermission(new Domain.Entities.Permission(Domain.Base.BasePermissionCodes.DeletePersonContact));
            roleRepoMock.Setup(rr => rr.Roles).Returns(new List<Domain.Entities.Role> { _viewContactRole });
            await emergencyInformationService.DeletePersonEmergencyContactsAsync(guid);
        }


        [TestMethod]
        [ExpectedException(typeof(IntegrationApiException))]
        public async Task EmergencyInformationService_DeletePersonEmergencyContactsAsync_No_Guid()
        {
            try
            {
                _viewContactRole.AddPermission(new Domain.Entities.Permission(Domain.Base.BasePermissionCodes.DeletePersonContact));
                roleRepoMock.Setup(rr => rr.Roles).Returns(new List<Domain.Entities.Role> { _viewContactRole });
                await emergencyInformationService.DeletePersonEmergencyContactsAsync(It.IsAny<string>());
            }
            catch (IntegrationApiException ex)
            {
                Assert.IsNotNull(ex);
                Assert.IsTrue(ex.Errors.Count == 1);
                Assert.IsTrue(ex.Errors[0].Message.Contains("GUID is required to delete a person emergency contact"));
                throw ex;
            }
        }

        [TestMethod]
        [ExpectedException(typeof(KeyNotFoundException))]
        public async Task EmergencyInformationService_DeletePersonEmergencyContactsAsync_Invalid_Guid()
        {
            _viewContactRole.AddPermission(new Domain.Entities.Permission(Domain.Base.BasePermissionCodes.DeletePersonContact));
            roleRepoMock.Setup(rr => rr.Roles).Returns(new List<Domain.Entities.Role> { _viewContactRole });
            emerInfoRepoMock.Setup(repo => repo.GetPersonContactById2Async(It.IsAny<string>())).ReturnsAsync(() => null);
            await emergencyInformationService.DeletePersonEmergencyContactsAsync(guid);
        }

        //[TestMethod]
        //[ExpectedException(typeof(PermissionsException))]
        //public async Task EmergencyInformationService_DeletePersonEmergencyContactsAsync_Permission_Exception()
        //{
        //    roleRepoMock.Setup(rr => rr.Roles).Returns(new List<Domain.Entities.Role>() { new Domain.Entities.Role(1, "VIEW.PERSON") });
        //    await emergencyInformationService.DeletePersonEmergencyContactsAsync(guid);
        //}

        [TestMethod]
        [ExpectedException(typeof(KeyNotFoundException))]
        public async Task EmergencyInformationService_DeletePersonEmergencyContactsAsync_Invalid_Guid2()
        {
            _viewContactRole.AddPermission(new Domain.Entities.Permission(Domain.Base.BasePermissionCodes.DeletePersonContact));
            roleRepoMock.Setup(rr => rr.Roles).Returns(new List<Domain.Entities.Role> { _viewContactRole });
            emerInfoRepoMock.Setup(repo => repo.GetPersonContactById2Async(It.IsAny<string>())).ThrowsAsync(new KeyNotFoundException());
            await emergencyInformationService.DeletePersonEmergencyContactsAsync(guid);
        }

        [TestMethod]
        [ExpectedException(typeof(IntegrationApiException))]
        public async Task EmergencyInformationService_DeletePersonEmergencyContactsAsync_Repo_Exceptipn()
        {
            _viewContactRole.AddPermission(new Domain.Entities.Permission(Domain.Base.BasePermissionCodes.DeletePersonContact));
            roleRepoMock.Setup(rr => rr.Roles).Returns(new List<Domain.Entities.Role> { _viewContactRole });
            emerInfoRepoMock.Setup(repo => repo.GetPersonContactById2Async(It.IsAny<string>())).ThrowsAsync(new RepositoryException());
            await emergencyInformationService.DeletePersonEmergencyContactsAsync(guid);
        }

        [TestMethod]
        [ExpectedException(typeof(IntegrationApiException))]
        public async Task EmergencyInformationService_DeletePersonEmergencyContactsAsync_IntegrationApiException_Exceptipn()
        {
            _viewContactRole.AddPermission(new Domain.Entities.Permission(Domain.Base.BasePermissionCodes.DeletePersonContact));
            roleRepoMock.Setup(rr => rr.Roles).Returns(new List<Domain.Entities.Role> { _viewContactRole });
            emerInfoRepoMock.Setup(repo => repo.GetPersonContactById2Async(It.IsAny<string>())).ThrowsAsync(new IntegrationApiException());
            await emergencyInformationService.DeletePersonEmergencyContactsAsync(guid);
        }


        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task EmergencyInformationService_DeletePersonEmergencyContactsAsync_ArgumentException_Exceptipn()
        {
            _viewContactRole.AddPermission(new Domain.Entities.Permission(Domain.Base.BasePermissionCodes.DeletePersonContact));
            roleRepoMock.Setup(rr => rr.Roles).Returns(new List<Domain.Entities.Role> { _viewContactRole });
            emerInfoRepoMock.Setup(repo => repo.GetPersonContactById2Async(It.IsAny<string>())).ThrowsAsync(new ArgumentException());
            await emergencyInformationService.DeletePersonEmergencyContactsAsync(guid);
        }

        [TestMethod]
        [ExpectedException(typeof(Exception))]
        public async Task EmergencyInformationService_DeletePersonEmergencyContactsAsync_Exception_Exceptipn()
        {
            _viewContactRole.AddPermission(new Domain.Entities.Permission(Domain.Base.BasePermissionCodes.DeletePersonContact));
            roleRepoMock.Setup(rr => rr.Roles).Returns(new List<Domain.Entities.Role> { _viewContactRole });
            emerInfoRepoMock.Setup(repo => repo.GetPersonContactById2Async(It.IsAny<string>())).ThrowsAsync(new Exception());
            await emergencyInformationService.DeletePersonEmergencyContactsAsync(guid);
        }


        [TestMethod]
        [ExpectedException(typeof(KeyNotFoundException))]
        public async Task EmergencyInformationService_DeletePersonEmergencyContactsAsync_KeyNotFoundException_Exceptipn()
        {
            _viewContactRole.AddPermission(new Domain.Entities.Permission(Domain.Base.BasePermissionCodes.DeletePersonContact));
            roleRepoMock.Setup(rr => rr.Roles).Returns(new List<Domain.Entities.Role> { _viewContactRole });
            entities.FirstOrDefault().PersonContactDetails = null;
            emerInfoRepoMock.Setup(repo => repo.GetPersonContactById2Async(It.IsAny<string>())).ReturnsAsync(entities.FirstOrDefault());
            await emergencyInformationService.DeletePersonEmergencyContactsAsync(guid);
        }
        #endregion

        #region Create/Update
        
        [TestMethod]
        [ExpectedException(typeof(IntegrationApiException))]
        public async Task CreatePersonEmergencyContactsAsync_PersonEmergencyContacts_Null()
        {
            try
            {
                //_viewContactRole.AddPermission(new Domain.Entities.Permission(Domain.Base.BasePermissionCodes.DeletePersonContact));
                //roleRepoMock.Setup(rr => rr.Roles).Returns(new List<Domain.Entities.Role> { _viewContactRole });
                await emergencyInformationService.CreatePersonEmergencyContactsAsync(null);
            }
            catch (IntegrationApiException ex)
            {
                Assert.IsNotNull(ex);
                Assert.IsTrue(ex.Errors.Count == 1);
                Assert.IsTrue(ex.Errors[0].Message.Contains("Must provide a person emergency contact representation for create."));
                throw ex;
            }
        }

        [TestMethod]
        [ExpectedException(typeof(IntegrationApiException))]
        public async Task CreatePersonEmergencyContactsAsync_Id_Null()
        {
            try
            {
                PersonEmergencyContacts pec = new PersonEmergencyContacts();
                await emergencyInformationService.CreatePersonEmergencyContactsAsync(pec);
            }
            catch (IntegrationApiException ex)
            {
                Assert.IsNotNull(ex);
                Assert.IsTrue(ex.Errors.Count == 1);
                Assert.IsTrue(ex.Errors[0].Message.Contains("Must provide a person emergency contact id for create."));
                throw ex;
            }
        }

        [TestMethod]
        [ExpectedException(typeof(IntegrationApiException))]
        public async Task CreatePersonEmergencyContactsAsync_Guid_Not_Nil()
        {
            try
            {
                PersonEmergencyContacts pec = new PersonEmergencyContacts() { Id = Guid.NewGuid().ToString()};
                await emergencyInformationService.CreatePersonEmergencyContactsAsync(pec);
            }
            catch (IntegrationApiException ex)
            {
                Assert.IsNotNull(ex);
                Assert.IsTrue(ex.Errors.Count == 1);
                Assert.IsTrue(ex.Errors[0].Message.Contains("Must provide a nil person emergency contact id for create."));
                throw ex;
            }
        }

        //[TestMethod]
        //[ExpectedException(typeof(PermissionsException))]
        //public async Task CreatePersonEmergencyContactsAsync_PermissionsException()
        //{
        //    try
        //    {
        //        //_viewContactRole.AddPermission(new Domain.Entities.Permission(Domain.Base.BasePermissionCodes.DeletePersonContact));
        //        //roleRepoMock.Setup(rr => rr.Roles).Returns(new List<Domain.Entities.Role> { _viewContactRole });
        //        PersonEmergencyContacts pec = new PersonEmergencyContacts() { Id = Guid.Empty.ToString() };
        //        await emergencyInformationService.CreatePersonEmergencyContactsAsync(pec);
        //    }
        //    catch (PermissionsException ex)
        //    {
        //        Assert.IsNotNull(ex);
        //        //Assert.IsTrue(ex.Errors.Count == 1);
        //        Assert.IsTrue(ex.Message.Contains("User is not authorized to create or update person-emergency-contacts."));
        //        throw ex;
        //    }
        //}

        [TestMethod]
        [ExpectedException(typeof(IntegrationApiException))]
        public async Task CreatePersonEmergencyContactsAsync_IntegrationApiExceptionAddError()
        {
            _updateContactRole.AddPermission(new Domain.Entities.Permission(Domain.Base.BasePermissionCodes.UpdatePersonContact));
            roleRepoMock.Setup(rr => rr.Roles).Returns(new List<Domain.Entities.Role> { _updateContactRole });
            PersonEmergencyContacts pec = new PersonEmergencyContacts()
            {
                Id = Guid.Empty.ToString(),
                Person = null,
                Contact = new Dtos.DtoProperties.PersonEmergencyContactsContact()
                {

                }
            };
            await emergencyInformationService.CreatePersonEmergencyContactsAsync(pec);
        }

        [TestMethod]
        [ExpectedException(typeof(IntegrationApiException))]
        public async Task CreatePersonEmergencyContactsAsync_Person_Repository_Exception()
        {
            _updateContactRole.AddPermission(new Domain.Entities.Permission(Domain.Base.BasePermissionCodes.UpdatePersonContact));
            roleRepoMock.Setup(rr => rr.Roles).Returns(new List<Domain.Entities.Role> { _updateContactRole });

            personRepositoryMock.Setup(repo => repo.GetPersonIdFromGuidAsync(It.IsAny<string>())).ThrowsAsync(new Exception());

            PersonEmergencyContacts pec = new PersonEmergencyContacts()
            {
                Id = Guid.Empty.ToString(),
                Person = new GuidObject2("1"),
                Contact = new Dtos.DtoProperties.PersonEmergencyContactsContact()
                {

                }
            };
            await emergencyInformationService.CreatePersonEmergencyContactsAsync(pec);
        }

        [TestMethod]
        [ExpectedException(typeof(IntegrationApiException))]
        public async Task CreatePersonEmergencyContactsAsync_Person_Id_Not_Match_PersonEmerId()
        {
            _updateContactRole.AddPermission(new Domain.Entities.Permission(Domain.Base.BasePermissionCodes.UpdatePersonContact));
            roleRepoMock.Setup(rr => rr.Roles).Returns(new List<Domain.Entities.Role> { _updateContactRole });

            personRepositoryMock.Setup(repo => repo.GetPersonIdFromGuidAsync(It.IsAny<string>())).ThrowsAsync(new Exception());

            PersonEmergencyContacts pec = new PersonEmergencyContacts()
            {
                Id = Guid.Empty.ToString(),
                Person = new GuidObject2("1"),
                Contact = new Dtos.DtoProperties.PersonEmergencyContactsContact()
                {

                }
            };
            await emergencyInformationService.CreatePersonEmergencyContactsAsync(pec);
        }

        [TestMethod]
        [ExpectedException(typeof(IntegrationApiException))]
        public async Task CreatePersonEmergencyContactsAsync_Empty_PersonId()
        {
            _updateContactRole.AddPermission(new Domain.Entities.Permission(Domain.Base.BasePermissionCodes.UpdatePersonContact));
            roleRepoMock.Setup(rr => rr.Roles).Returns(new List<Domain.Entities.Role> { _updateContactRole });

            personRepositoryMock.Setup(repo => repo.GetPersonIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync("");

            PersonEmergencyContacts pec = new PersonEmergencyContacts()
            {
                Id = Guid.Empty.ToString(),
                Person = new GuidObject2("1"),
                Contact = new Dtos.DtoProperties.PersonEmergencyContactsContact()
                {

                }
            };
            await emergencyInformationService.CreatePersonEmergencyContactsAsync(pec);
        }

        [TestMethod]
        [ExpectedException(typeof(IntegrationApiException))]
        public async Task CreatePersonEmergencyContactsAsync_ContactNameEmpty()
        {
            _updateContactRole.AddPermission(new Domain.Entities.Permission(Domain.Base.BasePermissionCodes.UpdatePersonContact));
            roleRepoMock.Setup(rr => rr.Roles).Returns(new List<Domain.Entities.Role> { _updateContactRole });

            personRepositoryMock.Setup(repo => repo.GetPersonIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync("ABC");

            PersonEmergencyContacts pec = new PersonEmergencyContacts()
            {
                Id = Guid.Empty.ToString(),
                Person = new GuidObject2("1"),
                Contact = new Dtos.DtoProperties.PersonEmergencyContactsContact()
                {

                }
            };
            await emergencyInformationService.CreatePersonEmergencyContactsAsync(pec);
        }

        [TestMethod]
        [ExpectedException(typeof(IntegrationApiException))]
        public async Task CreatePersonEmergencyContactsAsync_Contact_Null()
        {
            _updateContactRole.AddPermission(new Domain.Entities.Permission(Domain.Base.BasePermissionCodes.UpdatePersonContact));
            roleRepoMock.Setup(rr => rr.Roles).Returns(new List<Domain.Entities.Role> { _updateContactRole });

            personRepositoryMock.Setup(repo => repo.GetPersonIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync("ABC");

            PersonEmergencyContacts pec = new PersonEmergencyContacts()
            {
                Id = Guid.Empty.ToString(),
                Person = new GuidObject2("1"),
                Contact = null
            };
            await emergencyInformationService.CreatePersonEmergencyContactsAsync(pec);
        }

        [TestMethod]
        [ExpectedException(typeof(IntegrationApiException))]
        public async Task CreatePersonEmergencyContactsAsync_ContactTypes_Null()
        {
            _updateContactRole.AddPermission(new Domain.Entities.Permission(Domain.Base.BasePermissionCodes.UpdatePersonContact));
            roleRepoMock.Setup(rr => rr.Roles).Returns(new List<Domain.Entities.Role> { _updateContactRole });

            personRepositoryMock.Setup(repo => repo.GetPersonIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync("ABC");

            PersonEmergencyContacts pec = new PersonEmergencyContacts()
            {
                Id = Guid.Empty.ToString(),
                Person = new GuidObject2("1"),
                Contact = new Dtos.DtoProperties.PersonEmergencyContactsContact()
                {
                    Name = new PersonContactName()
                    {
                        FirstName = "First Name",
                        LastName = "Last Name",
                        MiddleName = "Middle Name"
                    }
                }
            };
            await emergencyInformationService.CreatePersonEmergencyContactsAsync(pec);
        }

        [TestMethod]
        [ExpectedException(typeof(IntegrationApiException))]
        public async Task CreatePersonEmergencyContactsAsync_With_FirstName_ContactTypes_Null()
        {
            _updateContactRole.AddPermission(new Domain.Entities.Permission(Domain.Base.BasePermissionCodes.UpdatePersonContact));
            roleRepoMock.Setup(rr => rr.Roles).Returns(new List<Domain.Entities.Role> { _updateContactRole });

            personRepositoryMock.Setup(repo => repo.GetPersonIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync("ABC");

            PersonEmergencyContacts pec = new PersonEmergencyContacts()
            {
                Id = Guid.Empty.ToString(),
                Person = new GuidObject2("1"),
                Contact = new Dtos.DtoProperties.PersonEmergencyContactsContact()
                {
                    Name = new PersonContactName()
                    {
                        FirstName = "First Name"
                    }
                }
            };
            await emergencyInformationService.CreatePersonEmergencyContactsAsync(pec);
        }

        [TestMethod]
        [ExpectedException(typeof(IntegrationApiException))]
        public async Task CreatePersonEmergencyContactsAsync_With_MiddleName_ContactTypes_Null()
        {
            _updateContactRole.AddPermission(new Domain.Entities.Permission(Domain.Base.BasePermissionCodes.UpdatePersonContact));
            roleRepoMock.Setup(rr => rr.Roles).Returns(new List<Domain.Entities.Role> { _updateContactRole });

            personRepositoryMock.Setup(repo => repo.GetPersonIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync("ABC");

            PersonEmergencyContacts pec = new PersonEmergencyContacts()
            {
                Id = Guid.Empty.ToString(),
                Person = new GuidObject2("1"),
                Contact = new Dtos.DtoProperties.PersonEmergencyContactsContact()
                {
                    Name = new PersonContactName()
                    {
                        MiddleName = "Middle Name"
                    }
                }
            };
            await emergencyInformationService.CreatePersonEmergencyContactsAsync(pec);
        }

        [TestMethod]
        [ExpectedException(typeof(IntegrationApiException))]
        public async Task CreatePersonEmergencyContactsAsync_With_LastName_ContactTypes_Null()
        {
            _updateContactRole.AddPermission(new Domain.Entities.Permission(Domain.Base.BasePermissionCodes.UpdatePersonContact));
            roleRepoMock.Setup(rr => rr.Roles).Returns(new List<Domain.Entities.Role> { _updateContactRole });

            personRepositoryMock.Setup(repo => repo.GetPersonIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync("ABC");

            PersonEmergencyContacts pec = new PersonEmergencyContacts()
            {
                Id = Guid.Empty.ToString(),
                Person = new GuidObject2("1"),
                Contact = new Dtos.DtoProperties.PersonEmergencyContactsContact()
                {
                    Name = new PersonContactName()
                    {
                        LastName = "Last Name"
                    }
                }
            };
            await emergencyInformationService.CreatePersonEmergencyContactsAsync(pec);
        }

        [TestMethod]
        [ExpectedException(typeof(IntegrationApiException))]
        public async Task CreatePersonEmergencyContactsAsync_With_FullName_ContactTypes_Null()
        {
            _updateContactRole.AddPermission(new Domain.Entities.Permission(Domain.Base.BasePermissionCodes.UpdatePersonContact));
            roleRepoMock.Setup(rr => rr.Roles).Returns(new List<Domain.Entities.Role> { _updateContactRole });

            personRepositoryMock.Setup(repo => repo.GetPersonIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync("ABC");

            PersonEmergencyContacts pec = new PersonEmergencyContacts()
            {
                Id = Guid.Empty.ToString(),
                Person = new GuidObject2("1"),
                Contact = new Dtos.DtoProperties.PersonEmergencyContactsContact()
                {
                    Name = new PersonContactName()
                    {
                        FullName = "Full Name"
                    }
                }
            };
            await emergencyInformationService.CreatePersonEmergencyContactsAsync(pec);
        }

        [TestMethod]
        [ExpectedException(typeof(IntegrationApiException))]
        public async Task CreatePersonEmergencyContactsAsync_With_Name_Null()
        {
            _updateContactRole.AddPermission(new Domain.Entities.Permission(Domain.Base.BasePermissionCodes.UpdatePersonContact));
            roleRepoMock.Setup(rr => rr.Roles).Returns(new List<Domain.Entities.Role> { _updateContactRole });

            personRepositoryMock.Setup(repo => repo.GetPersonIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync("ABC");

            PersonEmergencyContacts pec = new PersonEmergencyContacts()
            {
                Id = Guid.Empty.ToString(),
                Person = new GuidObject2("1"),
                Contact = new Dtos.DtoProperties.PersonEmergencyContactsContact()
                {
                    Name = new PersonContactName()
                    {
                        FirstName = "",
                        MiddleName = "",
                        LastName = ""
                    }
                }
            };
            await emergencyInformationService.CreatePersonEmergencyContactsAsync(pec);
        }

        [TestMethod]
        [ExpectedException(typeof(IntegrationApiException))]
        public async Task CreatePersonEmergencyContactsAsync_ContactReference_Null()
        {
            _updateContactRole.AddPermission(new Domain.Entities.Permission(Domain.Base.BasePermissionCodes.UpdatePersonContact));
            roleRepoMock.Setup(rr => rr.Roles).Returns(new List<Domain.Entities.Role> { _updateContactRole });

            personRepositoryMock.Setup(repo => repo.GetPersonIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync("ABC");
            referenceDataRepositoryMock.Setup(repo => repo.GetIntgPersonEmerTypesAsync(It.IsAny<bool>())).ReturnsAsync(() => null);

            PersonEmergencyContacts pec = new PersonEmergencyContacts()
            {
                Id = Guid.Empty.ToString(),
                Person = new GuidObject2("1"),
                Contact = new Dtos.DtoProperties.PersonEmergencyContactsContact()
                {
                    Name = new PersonContactName()
                    {
                        FirstName = "FirstName",
                        MiddleName = "MiddleName",
                        LastName = "LastName"
                    },
                    Types = new List<GuidObject2>()
                    {
                        new GuidObject2("Guid 1")
                    }
                }
            };
            await emergencyInformationService.CreatePersonEmergencyContactsAsync(pec);
        }

        [TestMethod]
        [ExpectedException(typeof(IntegrationApiException))]
        public async Task CreatePersonEmergencyContactsAsync_ContactTypeId_Null()
        {
            _updateContactRole.AddPermission(new Domain.Entities.Permission(Domain.Base.BasePermissionCodes.UpdatePersonContact));
            roleRepoMock.Setup(rr => rr.Roles).Returns(new List<Domain.Entities.Role> { _updateContactRole });

            personRepositoryMock.Setup(repo => repo.GetPersonIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync("ABC");

            PersonEmergencyContacts pec = new PersonEmergencyContacts()
            {
                Id = Guid.Empty.ToString(),
                Person = new GuidObject2("1"),
                Contact = new Dtos.DtoProperties.PersonEmergencyContactsContact()
                {
                    Name = new PersonContactName()
                    {
                        FirstName = "FirstName",
                        MiddleName = "MiddleName",
                        LastName = "LastName"
                    },
                    Types = new List<GuidObject2>()
                    {
                        new GuidObject2("")
                    }
                }
            };
            await emergencyInformationService.CreatePersonEmergencyContactsAsync(pec);
        }

        [TestMethod]
        [ExpectedException(typeof(IntegrationApiException))]
        public async Task CreatePersonEmergencyContactsAsync_Wrong_ContactType()
        {
            _updateContactRole.AddPermission(new Domain.Entities.Permission(Domain.Base.BasePermissionCodes.UpdatePersonContact));
            roleRepoMock.Setup(rr => rr.Roles).Returns(new List<Domain.Entities.Role> { _updateContactRole });

            personRepositoryMock.Setup(repo => repo.GetPersonIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync("ABC");

            PersonEmergencyContacts pec = new PersonEmergencyContacts()
            {
                Id = Guid.Empty.ToString(),
                Person = new GuidObject2("1"),
                Contact = new Dtos.DtoProperties.PersonEmergencyContactsContact()
                {
                    Name = new PersonContactName()
                    {
                        FirstName = "FirstName",
                        MiddleName = "MiddleName",
                        LastName = "LastName"
                    },
                    Types = new List<GuidObject2>()
                    {
                        new GuidObject2(Guid.NewGuid().ToString())
                    }
                }
            };
            await emergencyInformationService.CreatePersonEmergencyContactsAsync(pec);
        }

        [TestMethod]
        [ExpectedException(typeof(IntegrationApiException))]
        public async Task CreatePersonEmergencyContactsAsync_Empty_Phone_Number()
        {
            _updateContactRole.AddPermission(new Domain.Entities.Permission(Domain.Base.BasePermissionCodes.UpdatePersonContact));
            roleRepoMock.Setup(rr => rr.Roles).Returns(new List<Domain.Entities.Role> { _updateContactRole });

            personRepositoryMock.Setup(repo => repo.GetPersonIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync("ABC");

            PersonEmergencyContacts pec = new PersonEmergencyContacts()
            {
                Id = Guid.Empty.ToString(),
                Person = new GuidObject2("1"),
                Contact = new Dtos.DtoProperties.PersonEmergencyContactsContact()
                {
                    Name = new PersonContactName()
                    {
                        FirstName = "FirstName",
                        MiddleName = "MiddleName",
                        LastName = "LastName"
                    },
                    Types = new List<GuidObject2>()
                    {
                        new GuidObject2("emerguid"),
                        new GuidObject2("missguid")
                    },
                    Phones = new List<Dtos.DtoProperties.PersonEmergencyContactsPhones>()
                    {
                        new Dtos.DtoProperties.PersonEmergencyContactsPhones()
                        {
                            Number = ""
                        }
                    }
                }                
            };
            await emergencyInformationService.CreatePersonEmergencyContactsAsync(pec);
        }

        [TestMethod]
        [ExpectedException(typeof(IntegrationApiException))]
        public async Task CreatePersonEmergencyContactsAsync_ReferencePhone_Type_Null()
        {
            _updateContactRole.AddPermission(new Domain.Entities.Permission(Domain.Base.BasePermissionCodes.UpdatePersonContact));
            roleRepoMock.Setup(rr => rr.Roles).Returns(new List<Domain.Entities.Role> { _updateContactRole });

            personRepositoryMock.Setup(repo => repo.GetPersonIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync("ABC");
            referenceDataRepositoryMock.Setup(repo => repo.GetIntgPersonEmerPhoneTypesAsync(It.IsAny<bool>())).ReturnsAsync(() => null);

            PersonEmergencyContacts pec = new PersonEmergencyContacts()
            {
                Id = Guid.Empty.ToString(),
                Person = new GuidObject2("1"),
                Contact = new Dtos.DtoProperties.PersonEmergencyContactsContact()
                {
                    Name = new PersonContactName()
                    {
                        FirstName = "FirstName",
                        MiddleName = "MiddleName",
                        LastName = "LastName"
                    },
                    Types = new List<GuidObject2>()
                    {
                        new GuidObject2("emerguid"),
                        new GuidObject2("missguid")
                    },
                    Phones = new List<Dtos.DtoProperties.PersonEmergencyContactsPhones>()
                    {
                        new Dtos.DtoProperties.PersonEmergencyContactsPhones()
                        {
                            Number = "8005551212",
                            ContactAvailability = new GuidObject2("123")
                        }
                    }
                }
            };
            await emergencyInformationService.CreatePersonEmergencyContactsAsync(pec);
        }

        [TestMethod]
        [ExpectedException(typeof(IntegrationApiException))]
        public async Task CreatePersonEmergencyContactsAsync_Wrong_Phone_Type_Null()
        {
            _updateContactRole.AddPermission(new Domain.Entities.Permission(Domain.Base.BasePermissionCodes.UpdatePersonContact));
            roleRepoMock.Setup(rr => rr.Roles).Returns(new List<Domain.Entities.Role> { _updateContactRole });

            personRepositoryMock.Setup(repo => repo.GetPersonIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync("ABC");

            PersonEmergencyContacts pec = new PersonEmergencyContacts()
            {
                Id = Guid.Empty.ToString(),
                Person = new GuidObject2("1"),
                Contact = new Dtos.DtoProperties.PersonEmergencyContactsContact()
                {
                    Name = new PersonContactName()
                    {
                        FirstName = "FirstName",
                        MiddleName = "MiddleName",
                        LastName = "LastName"
                    },
                    Types = new List<GuidObject2>()
                    {
                        new GuidObject2("emerguid"),
                        new GuidObject2("missguid")
                    },
                    Phones = new List<Dtos.DtoProperties.PersonEmergencyContactsPhones>()
                    {
                        new Dtos.DtoProperties.PersonEmergencyContactsPhones()
                        {
                            Number = "8005551212",
                            ContactAvailability = new GuidObject2("123")
                        }
                    }
                }
            };
            await emergencyInformationService.CreatePersonEmergencyContactsAsync(pec);
        }

        [TestMethod]
        [ExpectedException(typeof(IntegrationApiException))]
        public async Task CreatePersonEmergencyContactsAsync_Duplicate_Phone_Type()
        {
            _updateContactRole.AddPermission(new Domain.Entities.Permission(Domain.Base.BasePermissionCodes.UpdatePersonContact));
            roleRepoMock.Setup(rr => rr.Roles).Returns(new List<Domain.Entities.Role> { _updateContactRole });

            personRepositoryMock.Setup(repo => repo.GetPersonIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync("ABC");

            PersonEmergencyContacts pec = new PersonEmergencyContacts()
            {
                Id = Guid.Empty.ToString(),
                Person = new GuidObject2("1"),
                Contact = new Dtos.DtoProperties.PersonEmergencyContactsContact()
                {
                    Name = new PersonContactName()
                    {
                        FirstName = "FirstName",
                        MiddleName = "MiddleName",
                        LastName = "LastName"
                    },
                    Types = new List<GuidObject2>()
                    {
                        new GuidObject2("emerguid"),
                        new GuidObject2("missguid")
                    },
                    Phones = new List<Dtos.DtoProperties.PersonEmergencyContactsPhones>()
                    {
                        new Dtos.DtoProperties.PersonEmergencyContactsPhones()
                        {
                            Number = "8005551212",
                            ContactAvailability = new GuidObject2("dayguid")
                        },
                        new Dtos.DtoProperties.PersonEmergencyContactsPhones()
                        {
                            Number = "8005551212",
                            ContactAvailability = new GuidObject2("dayguid")
                        }
                    }
                }
            };
            await emergencyInformationService.CreatePersonEmergencyContactsAsync(pec);
        }

        [TestMethod]
        [ExpectedException(typeof(IntegrationApiException))]
        public async Task CreatePersonEmergencyContactsAsync_Reference_Relation_Type_Null()
        {
            _updateContactRole.AddPermission(new Domain.Entities.Permission(Domain.Base.BasePermissionCodes.UpdatePersonContact));
            roleRepoMock.Setup(rr => rr.Roles).Returns(new List<Domain.Entities.Role> { _updateContactRole });

            personRepositoryMock.Setup(repo => repo.GetPersonIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync("ABC");
            referenceDataRepositoryMock.Setup(repo => repo.GetRelationTypesAsync(It.IsAny<bool>())).ReturnsAsync(() => null);

            PersonEmergencyContacts pec = new PersonEmergencyContacts()
            {
                Id = Guid.Empty.ToString(),
                Person = new GuidObject2("1"),
                Contact = new Dtos.DtoProperties.PersonEmergencyContactsContact()
                {
                    Name = new PersonContactName()
                    {
                        FirstName = "FirstName",
                        MiddleName = "MiddleName",
                        LastName = "LastName"
                    },
                    Types = new List<GuidObject2>()
                    {
                        new GuidObject2("emerguid"),
                        new GuidObject2("missguid")
                    },
                    Phones = new List<Dtos.DtoProperties.PersonEmergencyContactsPhones>()
                    {
                        new Dtos.DtoProperties.PersonEmergencyContactsPhones()
                        {
                            Number = "8005551212",
                            ContactAvailability = new GuidObject2("dayguid")
                        },
                        new Dtos.DtoProperties.PersonEmergencyContactsPhones()
                        {
                            Number = "8005551212",
                            ContactAvailability = new GuidObject2("eveguid")
                        },
                        new Dtos.DtoProperties.PersonEmergencyContactsPhones()
                        {
                            Number = "8005551212",
                            ContactAvailability = new GuidObject2("othguid")
                        }
                    },
                    Relationship = new Dtos.DtoProperties.PersonEmergencyContactsRelationship()
                    {
                        Detail = new GuidObject2("guid"),
                        Type = ""
                    }
                }
            };
            await emergencyInformationService.CreatePersonEmergencyContactsAsync(pec);
        }

        [TestMethod]
        [ExpectedException(typeof(IntegrationApiException))]
        public async Task CreatePersonEmergencyContactsAsync_Relation_Type()
        {
            _updateContactRole.AddPermission(new Domain.Entities.Permission(Domain.Base.BasePermissionCodes.UpdatePersonContact));
            roleRepoMock.Setup(rr => rr.Roles).Returns(new List<Domain.Entities.Role> { _updateContactRole });

            personRepositoryMock.Setup(repo => repo.GetPersonIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync("ABC");

            PersonEmergencyContacts pec = new PersonEmergencyContacts()
            {
                Id = Guid.Empty.ToString(),
                Person = new GuidObject2("1"),
                Contact = new Dtos.DtoProperties.PersonEmergencyContactsContact()
                {
                    Name = new PersonContactName()
                    {
                        FirstName = "FirstName",
                        MiddleName = "MiddleName",
                        LastName = "LastName"
                    },
                    Types = new List<GuidObject2>()
                    {
                        new GuidObject2("emerguid"),
                        new GuidObject2("missguid")
                    },
                    Phones = new List<Dtos.DtoProperties.PersonEmergencyContactsPhones>()
                    {
                        new Dtos.DtoProperties.PersonEmergencyContactsPhones()
                        {
                            Number = "8005551212",
                            ContactAvailability = new GuidObject2("dayguid")
                        },
                        new Dtos.DtoProperties.PersonEmergencyContactsPhones()
                        {
                            Number = "8005551212",
                            ContactAvailability = new GuidObject2("eveguid")
                        },
                        new Dtos.DtoProperties.PersonEmergencyContactsPhones()
                        {
                            Number = "8005551212",
                            ContactAvailability = new GuidObject2("othguid")
                        }
                    },
                    Relationship = new Dtos.DtoProperties.PersonEmergencyContactsRelationship()
                    {
                        Detail = new GuidObject2("guid"),
                        Type = ""
                    }
                }
            };
            await emergencyInformationService.CreatePersonEmergencyContactsAsync(pec);
        }

        [TestMethod]
        [ExpectedException(typeof(IntegrationApiException))]
        public async Task CreatePersonEmergencyContactsAsync_Relation_Type_Null()
        {
            _updateContactRole.AddPermission(new Domain.Entities.Permission(Domain.Base.BasePermissionCodes.UpdatePersonContact));
            roleRepoMock.Setup(rr => rr.Roles).Returns(new List<Domain.Entities.Role> { _updateContactRole });

            personRepositoryMock.Setup(repo => repo.GetPersonIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync("ABC");

            PersonEmergencyContacts pec = new PersonEmergencyContacts()
            {
                Id = Guid.Empty.ToString(),
                Person = new GuidObject2("1"),
                Contact = new Dtos.DtoProperties.PersonEmergencyContactsContact()
                {
                    Name = new PersonContactName()
                    {
                        FirstName = "FirstName",
                        MiddleName = "MiddleName",
                        LastName = "LastName"
                    },
                    Types = new List<GuidObject2>()
                    {
                        new GuidObject2("emerguid"),
                        new GuidObject2("missguid")
                    },
                    Phones = new List<Dtos.DtoProperties.PersonEmergencyContactsPhones>()
                    {
                        new Dtos.DtoProperties.PersonEmergencyContactsPhones()
                        {
                            Number = "8005551212",
                            ContactAvailability = new GuidObject2("dayguid")
                        },
                        new Dtos.DtoProperties.PersonEmergencyContactsPhones()
                        {
                            Number = "8005551212",
                            ContactAvailability = new GuidObject2("eveguid")
                        },
                        new Dtos.DtoProperties.PersonEmergencyContactsPhones()
                        {
                            Number = "8005551212",
                            ContactAvailability = new GuidObject2("othguid")
                        }
                    },
                    Relationship = new Dtos.DtoProperties.PersonEmergencyContactsRelationship()
                    {
                        Detail = new GuidObject2("guid"),
                        Type = ""
                    }
                }
            };
            await emergencyInformationService.CreatePersonEmergencyContactsAsync(pec);
        }

        [TestMethod]
        public async Task CreatePersonEmergencyContactsAsync()
        {
            _updateContactRole.AddPermission(new Domain.Entities.Permission(Domain.Base.BasePermissionCodes.UpdatePersonContact));
            roleRepoMock.Setup(rr => rr.Roles).Returns(new List<Domain.Entities.Role> { _updateContactRole });

            personRepositoryMock.Setup(repo => repo.GetPersonIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync("ABC");
            emerInfoRepoMock.Setup(repo => repo.UpdatePersonEmergencyContactsAsync(It.IsAny<Domain.Base.Entities.PersonContact>()))
                .ReturnsAsync(entities.FirstOrDefault());

            PersonEmergencyContacts pec = new PersonEmergencyContacts()
            {
                Id = Guid.Empty.ToString(),
                Person = new GuidObject2("1"),
                Contact = new Dtos.DtoProperties.PersonEmergencyContactsContact()
                {
                    Name = new PersonContactName()
                    {
                        FirstName = "FirstName",
                        MiddleName = "MiddleName",
                        LastName = "LastName"
                    },
                    Types = new List<GuidObject2>()
                    {
                        new GuidObject2("emerguid"),
                        new GuidObject2("missguid")
                    },
                    Phones = new List<Dtos.DtoProperties.PersonEmergencyContactsPhones>()
                    {
                        new Dtos.DtoProperties.PersonEmergencyContactsPhones()
                        {
                            Number = "8005551212",
                            ContactAvailability = new GuidObject2("dayguid")
                        },
                        new Dtos.DtoProperties.PersonEmergencyContactsPhones()
                        {
                            Number = "8005551212",
                            ContactAvailability = new GuidObject2("eveguid")
                        },
                        new Dtos.DtoProperties.PersonEmergencyContactsPhones()
                        {
                            Number = "8005551212",
                            ContactAvailability = new GuidObject2("othguid")
                        }
                    },
                    Relationship = new Dtos.DtoProperties.PersonEmergencyContactsRelationship()
                    {
                        Detail = new GuidObject2("7989a936-f41d-4c08-9fda-dd41314a9e34")
                    }
                }
            };
            await emergencyInformationService.CreatePersonEmergencyContactsAsync(pec);
        }

        [TestMethod]
        public async Task CreatePersonEmergencyContactsAsync_WithRelType()
        {
            _updateContactRole.AddPermission(new Domain.Entities.Permission(Domain.Base.BasePermissionCodes.UpdatePersonContact));
            roleRepoMock.Setup(rr => rr.Roles).Returns(new List<Domain.Entities.Role> { _updateContactRole });

            personRepositoryMock.Setup(repo => repo.GetPersonIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync("ABC");
            emerInfoRepoMock.Setup(repo => repo.UpdatePersonEmergencyContactsAsync(It.IsAny<Domain.Base.Entities.PersonContact>()))
                .ReturnsAsync(entities.FirstOrDefault());

            PersonEmergencyContacts pec = new PersonEmergencyContacts()
            {
                Id = Guid.Empty.ToString(),
                Person = new GuidObject2("1"),
                Contact = new Dtos.DtoProperties.PersonEmergencyContactsContact()
                {
                    Name = new PersonContactName()
                    {
                        FirstName = "FirstName",
                        MiddleName = "MiddleName",
                        LastName = "LastName"
                    },
                    Types = new List<GuidObject2>()
                    {
                        new GuidObject2("emerguid"),
                        new GuidObject2("missguid")
                    },
                    Phones = new List<Dtos.DtoProperties.PersonEmergencyContactsPhones>()
                    {
                        new Dtos.DtoProperties.PersonEmergencyContactsPhones()
                        {
                            Number = "8005551212",
                            ContactAvailability = new GuidObject2("dayguid")
                        },
                        new Dtos.DtoProperties.PersonEmergencyContactsPhones()
                        {
                            Number = "8005551212",
                            ContactAvailability = new GuidObject2("eveguid")
                        },
                        new Dtos.DtoProperties.PersonEmergencyContactsPhones()
                        {
                            Number = "8005551212",
                            ContactAvailability = new GuidObject2("othguid")
                        }
                    },
                    Relationship = new Dtos.DtoProperties.PersonEmergencyContactsRelationship()
                    {
                        Type = "Type"
                    }
                }
            };
            await emergencyInformationService.CreatePersonEmergencyContactsAsync(pec);
        }

        [TestMethod]
        [ExpectedException(typeof(RepositoryException))]
        public async Task CreatePersonEmergencyContactsAsync_RepositoryException()
        {
            _updateContactRole.AddPermission(new Domain.Entities.Permission(Domain.Base.BasePermissionCodes.UpdatePersonContact));
            roleRepoMock.Setup(rr => rr.Roles).Returns(new List<Domain.Entities.Role> { _updateContactRole });

            personRepositoryMock.Setup(repo => repo.GetPersonIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync("ABC");
            emerInfoRepoMock.Setup(repo => repo.UpdatePersonEmergencyContactsAsync(It.IsAny<Domain.Base.Entities.PersonContact>()))
                .ThrowsAsync(new RepositoryException());

            PersonEmergencyContacts pec = new PersonEmergencyContacts()
            {
                Id = Guid.Empty.ToString(),
                Person = new GuidObject2("1"),
                Contact = new Dtos.DtoProperties.PersonEmergencyContactsContact()
                {
                    Name = new PersonContactName()
                    {
                        FirstName = "FirstName",
                        MiddleName = "MiddleName",
                        LastName = "LastName"
                    },
                    Types = new List<GuidObject2>()
                    {
                        new GuidObject2("emerguid"),
                        new GuidObject2("missguid")
                    },
                    Phones = new List<Dtos.DtoProperties.PersonEmergencyContactsPhones>()
                    {
                        new Dtos.DtoProperties.PersonEmergencyContactsPhones()
                        {
                            Number = "8005551212",
                            ContactAvailability = new GuidObject2("dayguid")
                        },
                        new Dtos.DtoProperties.PersonEmergencyContactsPhones()
                        {
                            Number = "8005551212",
                            ContactAvailability = new GuidObject2("eveguid")
                        },
                        new Dtos.DtoProperties.PersonEmergencyContactsPhones()
                        {
                            Number = "8005551212",
                            ContactAvailability = new GuidObject2("othguid")
                        }
                    },
                    Relationship = new Dtos.DtoProperties.PersonEmergencyContactsRelationship()
                    {
                        Type = "Type"
                    }
                }
            };
            await emergencyInformationService.CreatePersonEmergencyContactsAsync(pec);
        }

        [TestMethod]
        [ExpectedException(typeof(KeyNotFoundException))]
        public async Task CreatePersonEmergencyContactsAsync_KeyNotFoundException()
        {
            _updateContactRole.AddPermission(new Domain.Entities.Permission(Domain.Base.BasePermissionCodes.UpdatePersonContact));
            roleRepoMock.Setup(rr => rr.Roles).Returns(new List<Domain.Entities.Role> { _updateContactRole });

            personRepositoryMock.Setup(repo => repo.GetPersonIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync("ABC");
            emerInfoRepoMock.Setup(repo => repo.UpdatePersonEmergencyContactsAsync(It.IsAny<Domain.Base.Entities.PersonContact>()))
                .ThrowsAsync(new KeyNotFoundException());

            PersonEmergencyContacts pec = new PersonEmergencyContacts()
            {
                Id = Guid.Empty.ToString(),
                Person = new GuidObject2("1"),
                Contact = new Dtos.DtoProperties.PersonEmergencyContactsContact()
                {
                    Name = new PersonContactName()
                    {
                        FirstName = "FirstName",
                        MiddleName = "MiddleName",
                        LastName = "LastName"
                    },
                    Types = new List<GuidObject2>()
                    {
                        new GuidObject2("emerguid"),
                        new GuidObject2("missguid")
                    },
                    Phones = new List<Dtos.DtoProperties.PersonEmergencyContactsPhones>()
                    {
                        new Dtos.DtoProperties.PersonEmergencyContactsPhones()
                        {
                            Number = "8005551212",
                            ContactAvailability = new GuidObject2("dayguid")
                        },
                        new Dtos.DtoProperties.PersonEmergencyContactsPhones()
                        {
                            Number = "8005551212",
                            ContactAvailability = new GuidObject2("eveguid")
                        },
                        new Dtos.DtoProperties.PersonEmergencyContactsPhones()
                        {
                            Number = "8005551212",
                            ContactAvailability = new GuidObject2("othguid")
                        }
                    },
                    Relationship = new Dtos.DtoProperties.PersonEmergencyContactsRelationship()
                    {
                        Type = "Type"
                    }
                }
            };
            await emergencyInformationService.CreatePersonEmergencyContactsAsync(pec);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task CreatePersonEmergencyContactsAsync_ArgumentException()
        {
            _updateContactRole.AddPermission(new Domain.Entities.Permission(Domain.Base.BasePermissionCodes.UpdatePersonContact));
            roleRepoMock.Setup(rr => rr.Roles).Returns(new List<Domain.Entities.Role> { _updateContactRole });

            personRepositoryMock.Setup(repo => repo.GetPersonIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync("ABC");
            emerInfoRepoMock.Setup(repo => repo.UpdatePersonEmergencyContactsAsync(It.IsAny<Domain.Base.Entities.PersonContact>()))
                .ThrowsAsync(new ArgumentException());

            PersonEmergencyContacts pec = new PersonEmergencyContacts()
            {
                Id = Guid.Empty.ToString(),
                Person = new GuidObject2("1"),
                Contact = new Dtos.DtoProperties.PersonEmergencyContactsContact()
                {
                    Name = new PersonContactName()
                    {
                        FirstName = "FirstName",
                        MiddleName = "MiddleName",
                        LastName = "LastName"
                    },
                    Types = new List<GuidObject2>()
                    {
                        new GuidObject2("emerguid"),
                        new GuidObject2("missguid")
                    },
                    Phones = new List<Dtos.DtoProperties.PersonEmergencyContactsPhones>()
                    {
                        new Dtos.DtoProperties.PersonEmergencyContactsPhones()
                        {
                            Number = "8005551212",
                            ContactAvailability = new GuidObject2("dayguid")
                        },
                        new Dtos.DtoProperties.PersonEmergencyContactsPhones()
                        {
                            Number = "8005551212",
                            ContactAvailability = new GuidObject2("eveguid")
                        },
                        new Dtos.DtoProperties.PersonEmergencyContactsPhones()
                        {
                            Number = "8005551212",
                            ContactAvailability = new GuidObject2("othguid")
                        }
                    },
                    Relationship = new Dtos.DtoProperties.PersonEmergencyContactsRelationship()
                    {
                        Type = "Type"
                    }
                }
            };
            await emergencyInformationService.CreatePersonEmergencyContactsAsync(pec);
        }

        [TestMethod]
        [ExpectedException(typeof(Exception))]
        public async Task CreatePersonEmergencyContactsAsync_Exception()
        {
            _updateContactRole.AddPermission(new Domain.Entities.Permission(Domain.Base.BasePermissionCodes.UpdatePersonContact));
            roleRepoMock.Setup(rr => rr.Roles).Returns(new List<Domain.Entities.Role> { _updateContactRole });

            personRepositoryMock.Setup(repo => repo.GetPersonIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync("ABC");
            emerInfoRepoMock.Setup(repo => repo.UpdatePersonEmergencyContactsAsync(It.IsAny<Domain.Base.Entities.PersonContact>()))
                .ThrowsAsync(new Exception());

            PersonEmergencyContacts pec = new PersonEmergencyContacts()
            {
                Id = Guid.Empty.ToString(),
                Person = new GuidObject2("1"),
                Contact = new Dtos.DtoProperties.PersonEmergencyContactsContact()
                {
                    Name = new PersonContactName()
                    {
                        FirstName = "FirstName",
                        MiddleName = "MiddleName",
                        LastName = "LastName"
                    },
                    Types = new List<GuidObject2>()
                    {
                        new GuidObject2("emerguid"),
                        new GuidObject2("missguid")
                    },
                    Phones = new List<Dtos.DtoProperties.PersonEmergencyContactsPhones>()
                    {
                        new Dtos.DtoProperties.PersonEmergencyContactsPhones()
                        {
                            Number = "8005551212",
                            ContactAvailability = new GuidObject2("dayguid")
                        },
                        new Dtos.DtoProperties.PersonEmergencyContactsPhones()
                        {
                            Number = "8005551212",
                            ContactAvailability = new GuidObject2("eveguid")
                        },
                        new Dtos.DtoProperties.PersonEmergencyContactsPhones()
                        {
                            Number = "8005551212",
                            ContactAvailability = new GuidObject2("othguid")
                        }
                    },
                    Relationship = new Dtos.DtoProperties.PersonEmergencyContactsRelationship()
                    {
                        Type = "Type"
                    }
                }
            };
            await emergencyInformationService.CreatePersonEmergencyContactsAsync(pec);
        }

        [TestMethod]
        public async Task UpdatePersonEmergencyContactsAsync()
        {
            _updateContactRole.AddPermission(new Domain.Entities.Permission(Domain.Base.BasePermissionCodes.UpdatePersonContact));
            roleRepoMock.Setup(rr => rr.Roles).Returns(new List<Domain.Entities.Role> { _updateContactRole });

            personRepositoryMock.Setup(repo => repo.GetPersonIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync("ABC");
            emerInfoRepoMock.Setup(repo => repo.UpdatePersonEmergencyContactsAsync(It.IsAny<Domain.Base.Entities.PersonContact>()))
                .ReturnsAsync(entities.FirstOrDefault());

            PersonEmergencyContacts pec = new PersonEmergencyContacts()
            {
                Id = guid,
                Person = new GuidObject2("1"),
                Contact = new Dtos.DtoProperties.PersonEmergencyContactsContact()
                {
                    Name = new PersonContactName()
                    {
                        FirstName = "FirstName",
                        MiddleName = "MiddleName",
                        LastName = "LastName"
                    },
                    Types = new List<GuidObject2>()
                    {
                        new GuidObject2("emerguid"),
                        new GuidObject2("missguid")
                    },
                    Phones = new List<Dtos.DtoProperties.PersonEmergencyContactsPhones>()
                    {
                        new Dtos.DtoProperties.PersonEmergencyContactsPhones()
                        {
                            Number = "8005551212",
                            ContactAvailability = new GuidObject2("dayguid")
                        },
                        new Dtos.DtoProperties.PersonEmergencyContactsPhones()
                        {
                            Number = "8005551212",
                            ContactAvailability = new GuidObject2("eveguid")
                        },
                        new Dtos.DtoProperties.PersonEmergencyContactsPhones()
                        {
                            Number = "8005551212",
                            ContactAvailability = new GuidObject2("othguid")
                        }
                    },
                    Relationship = new Dtos.DtoProperties.PersonEmergencyContactsRelationship()
                    {
                        Type = "Type"
                    }
                }
            };
            await emergencyInformationService.UpdatePersonEmergencyContactsAsync(pec);
        }

        [TestMethod]
        [ExpectedException(typeof(PermissionsException))]
        public async Task UpdatePersonEmergencyContactsAsync_PermissionsException()
        {
            _updateContactRole.AddPermission(new Domain.Entities.Permission(Domain.Base.BasePermissionCodes.UpdatePersonContact));
            roleRepoMock.Setup(rr => rr.Roles).Returns(new List<Domain.Entities.Role> { _updateContactRole });

            personRepositoryMock.Setup(repo => repo.GetPersonIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync("ABC");
            emerInfoRepoMock.Setup(repo => repo.UpdatePersonEmergencyContactsAsync(It.IsAny<Domain.Base.Entities.PersonContact>()))
                .ThrowsAsync(new PermissionsException());

            PersonEmergencyContacts pec = new PersonEmergencyContacts()
            {
                Id = guid,
                Person = new GuidObject2("1"),
                Contact = new Dtos.DtoProperties.PersonEmergencyContactsContact()
                {
                    Name = new PersonContactName()
                    {
                        FirstName = "FirstName",
                        MiddleName = "MiddleName",
                        LastName = "LastName"
                    },
                    Types = new List<GuidObject2>()
                    {
                        new GuidObject2("emerguid"),
                        new GuidObject2("missguid")
                    },
                    Phones = new List<Dtos.DtoProperties.PersonEmergencyContactsPhones>()
                    {
                        new Dtos.DtoProperties.PersonEmergencyContactsPhones()
                        {
                            Number = "8005551212",
                            ContactAvailability = new GuidObject2("dayguid")
                        },
                        new Dtos.DtoProperties.PersonEmergencyContactsPhones()
                        {
                            Number = "8005551212",
                            ContactAvailability = new GuidObject2("eveguid")
                        },
                        new Dtos.DtoProperties.PersonEmergencyContactsPhones()
                        {
                            Number = "8005551212",
                            ContactAvailability = new GuidObject2("othguid")
                        }
                    },
                    Relationship = new Dtos.DtoProperties.PersonEmergencyContactsRelationship()
                    {
                        Type = "Type"
                    }
                }
            };
            await emergencyInformationService.UpdatePersonEmergencyContactsAsync(pec);
        }

        [TestMethod]
        [ExpectedException(typeof(IntegrationApiException))]
        public async Task UpdatePersonEmergencyContactsAsync_IntegrationApiException()
        {
            _updateContactRole.AddPermission(new Domain.Entities.Permission(Domain.Base.BasePermissionCodes.UpdatePersonContact));
            roleRepoMock.Setup(rr => rr.Roles).Returns(new List<Domain.Entities.Role> { _updateContactRole });

            personRepositoryMock.Setup(repo => repo.GetPersonIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync("ABC");
            emerInfoRepoMock.Setup(repo => repo.UpdatePersonEmergencyContactsAsync(It.IsAny<Domain.Base.Entities.PersonContact>()))
                .ThrowsAsync(new IntegrationApiException());

            PersonEmergencyContacts pec = new PersonEmergencyContacts()
            {
                Id = guid,
                Person = new GuidObject2("1"),
                Contact = new Dtos.DtoProperties.PersonEmergencyContactsContact()
                {
                    Name = new PersonContactName()
                    {
                        FirstName = "FirstName",
                        MiddleName = "MiddleName",
                        LastName = "LastName"
                    },
                    Types = new List<GuidObject2>()
                    {
                        new GuidObject2("emerguid"),
                        new GuidObject2("missguid")
                    },
                    Phones = new List<Dtos.DtoProperties.PersonEmergencyContactsPhones>()
                    {
                        new Dtos.DtoProperties.PersonEmergencyContactsPhones()
                        {
                            Number = "8005551212",
                            ContactAvailability = new GuidObject2("dayguid")
                        },
                        new Dtos.DtoProperties.PersonEmergencyContactsPhones()
                        {
                            Number = "8005551212",
                            ContactAvailability = new GuidObject2("eveguid")
                        },
                        new Dtos.DtoProperties.PersonEmergencyContactsPhones()
                        {
                            Number = "8005551212",
                            ContactAvailability = new GuidObject2("othguid")
                        }
                    },
                    Relationship = new Dtos.DtoProperties.PersonEmergencyContactsRelationship()
                    {
                        Type = "Type"
                    }
                }
            };
            await emergencyInformationService.UpdatePersonEmergencyContactsAsync(pec);
        }

        [TestMethod]
        [ExpectedException(typeof(RepositoryException))]
        public async Task UpdatePersonEmergencyContactsAsync_RepositoryException()
        {
            _updateContactRole.AddPermission(new Domain.Entities.Permission(Domain.Base.BasePermissionCodes.UpdatePersonContact));
            roleRepoMock.Setup(rr => rr.Roles).Returns(new List<Domain.Entities.Role> { _updateContactRole });

            personRepositoryMock.Setup(repo => repo.GetPersonIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync("ABC");
            emerInfoRepoMock.Setup(repo => repo.UpdatePersonEmergencyContactsAsync(It.IsAny<Domain.Base.Entities.PersonContact>()))
                .ThrowsAsync(new RepositoryException());

            PersonEmergencyContacts pec = new PersonEmergencyContacts()
            {
                Id = guid,
                Person = new GuidObject2("1"),
                Contact = new Dtos.DtoProperties.PersonEmergencyContactsContact()
                {
                    Name = new PersonContactName()
                    {
                        FirstName = "FirstName",
                        MiddleName = "MiddleName",
                        LastName = "LastName"
                    },
                    Types = new List<GuidObject2>()
                    {
                        new GuidObject2("emerguid"),
                        new GuidObject2("missguid")
                    },
                    Phones = new List<Dtos.DtoProperties.PersonEmergencyContactsPhones>()
                    {
                        new Dtos.DtoProperties.PersonEmergencyContactsPhones()
                        {
                            Number = "8005551212",
                            ContactAvailability = new GuidObject2("dayguid")
                        },
                        new Dtos.DtoProperties.PersonEmergencyContactsPhones()
                        {
                            Number = "8005551212",
                            ContactAvailability = new GuidObject2("eveguid")
                        },
                        new Dtos.DtoProperties.PersonEmergencyContactsPhones()
                        {
                            Number = "8005551212",
                            ContactAvailability = new GuidObject2("othguid")
                        }
                    },
                    Relationship = new Dtos.DtoProperties.PersonEmergencyContactsRelationship()
                    {
                        Type = "Type"
                    }
                }
            };
            await emergencyInformationService.UpdatePersonEmergencyContactsAsync(pec);
        }

        [TestMethod]
        [ExpectedException(typeof(KeyNotFoundException))]
        public async Task UpdatePersonEmergencyContactsAsync_KeyNotFoundException()
        {
            _updateContactRole.AddPermission(new Domain.Entities.Permission(Domain.Base.BasePermissionCodes.UpdatePersonContact));
            roleRepoMock.Setup(rr => rr.Roles).Returns(new List<Domain.Entities.Role> { _updateContactRole });

            personRepositoryMock.Setup(repo => repo.GetPersonIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync("ABC");
            emerInfoRepoMock.Setup(repo => repo.UpdatePersonEmergencyContactsAsync(It.IsAny<Domain.Base.Entities.PersonContact>()))
                .ThrowsAsync(new KeyNotFoundException());

            PersonEmergencyContacts pec = new PersonEmergencyContacts()
            {
                Id = guid,
                Person = new GuidObject2("1"),
                Contact = new Dtos.DtoProperties.PersonEmergencyContactsContact()
                {
                    Name = new PersonContactName()
                    {
                        FirstName = "FirstName",
                        MiddleName = "MiddleName",
                        LastName = "LastName"
                    },
                    Types = new List<GuidObject2>()
                    {
                        new GuidObject2("emerguid"),
                        new GuidObject2("missguid")
                    },
                    Phones = new List<Dtos.DtoProperties.PersonEmergencyContactsPhones>()
                    {
                        new Dtos.DtoProperties.PersonEmergencyContactsPhones()
                        {
                            Number = "8005551212",
                            ContactAvailability = new GuidObject2("dayguid")
                        },
                        new Dtos.DtoProperties.PersonEmergencyContactsPhones()
                        {
                            Number = "8005551212",
                            ContactAvailability = new GuidObject2("eveguid")
                        },
                        new Dtos.DtoProperties.PersonEmergencyContactsPhones()
                        {
                            Number = "8005551212",
                            ContactAvailability = new GuidObject2("othguid")
                        }
                    },
                    Relationship = new Dtos.DtoProperties.PersonEmergencyContactsRelationship()
                    {
                        Type = "Type"
                    }
                }
            };
            await emergencyInformationService.UpdatePersonEmergencyContactsAsync(pec);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task UpdatePersonEmergencyContactsAsync_ArgumentException()
        {
            _updateContactRole.AddPermission(new Domain.Entities.Permission(Domain.Base.BasePermissionCodes.UpdatePersonContact));
            roleRepoMock.Setup(rr => rr.Roles).Returns(new List<Domain.Entities.Role> { _updateContactRole });

            personRepositoryMock.Setup(repo => repo.GetPersonIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync("ABC");
            emerInfoRepoMock.Setup(repo => repo.UpdatePersonEmergencyContactsAsync(It.IsAny<Domain.Base.Entities.PersonContact>()))
                .ThrowsAsync(new ArgumentException());

            PersonEmergencyContacts pec = new PersonEmergencyContacts()
            {
                Id = guid,
                Person = new GuidObject2("1"),
                Contact = new Dtos.DtoProperties.PersonEmergencyContactsContact()
                {
                    Name = new PersonContactName()
                    {
                        FirstName = "FirstName",
                        MiddleName = "MiddleName",
                        LastName = "LastName"
                    },
                    Types = new List<GuidObject2>()
                    {
                        new GuidObject2("emerguid"),
                        new GuidObject2("missguid")
                    },
                    Phones = new List<Dtos.DtoProperties.PersonEmergencyContactsPhones>()
                    {
                        new Dtos.DtoProperties.PersonEmergencyContactsPhones()
                        {
                            Number = "8005551212",
                            ContactAvailability = new GuidObject2("dayguid")
                        },
                        new Dtos.DtoProperties.PersonEmergencyContactsPhones()
                        {
                            Number = "8005551212",
                            ContactAvailability = new GuidObject2("eveguid")
                        },
                        new Dtos.DtoProperties.PersonEmergencyContactsPhones()
                        {
                            Number = "8005551212",
                            ContactAvailability = new GuidObject2("othguid")
                        }
                    },
                    Relationship = new Dtos.DtoProperties.PersonEmergencyContactsRelationship()
                    {
                        Type = "Type"
                    }
                }
            };
            await emergencyInformationService.UpdatePersonEmergencyContactsAsync(pec);
        }

        [TestMethod]
        [ExpectedException(typeof(Exception))]
        public async Task UpdatePersonEmergencyContactsAsync_Exception()
        {
            _updateContactRole.AddPermission(new Domain.Entities.Permission(Domain.Base.BasePermissionCodes.UpdatePersonContact));
            roleRepoMock.Setup(rr => rr.Roles).Returns(new List<Domain.Entities.Role> { _updateContactRole });

            personRepositoryMock.Setup(repo => repo.GetPersonIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync("ABC");
            emerInfoRepoMock.Setup(repo => repo.UpdatePersonEmergencyContactsAsync(It.IsAny<Domain.Base.Entities.PersonContact>()))
                .ThrowsAsync(new Exception());

            PersonEmergencyContacts pec = new PersonEmergencyContacts()
            {
                Id = guid,
                Person = new GuidObject2("1"),
                Contact = new Dtos.DtoProperties.PersonEmergencyContactsContact()
                {
                    Name = new PersonContactName()
                    {
                        FirstName = "FirstName",
                        MiddleName = "MiddleName",
                        LastName = "LastName"
                    },
                    Types = new List<GuidObject2>()
                    {
                        new GuidObject2("emerguid"),
                        new GuidObject2("missguid")
                    },
                    Phones = new List<Dtos.DtoProperties.PersonEmergencyContactsPhones>()
                    {
                        new Dtos.DtoProperties.PersonEmergencyContactsPhones()
                        {
                            Number = "8005551212",
                            ContactAvailability = new GuidObject2("dayguid")
                        },
                        new Dtos.DtoProperties.PersonEmergencyContactsPhones()
                        {
                            Number = "8005551212",
                            ContactAvailability = new GuidObject2("eveguid")
                        },
                        new Dtos.DtoProperties.PersonEmergencyContactsPhones()
                        {
                            Number = "8005551212",
                            ContactAvailability = new GuidObject2("othguid")
                        }
                    },
                    Relationship = new Dtos.DtoProperties.PersonEmergencyContactsRelationship()
                    {
                        Type = "Type"
                    }
                }
            };
            await emergencyInformationService.UpdatePersonEmergencyContactsAsync(pec);
        }

        [TestMethod]
        [ExpectedException(typeof(IntegrationApiException))]
        public async Task UpdatePersonEmergencyContactsAsync_personEmergencyContacts_Null()
        {
            PersonEmergencyContacts pec = null;
            await emergencyInformationService.UpdatePersonEmergencyContactsAsync(pec);
        }

        [TestMethod]
        [ExpectedException(typeof(IntegrationApiException))]
        public async Task UpdatePersonEmergencyContactsAsync_personEmergencyContacts_Id_Null()
        {
            PersonEmergencyContacts pec = new PersonEmergencyContacts() { Id = string.Empty};
            await emergencyInformationService.UpdatePersonEmergencyContactsAsync(pec);
        }

        [TestMethod]
        [ExpectedException(typeof(IntegrationApiException))]
        public async Task UpdatePersonEmergencyContactsAsync_personEmergencyContacts_Guid_Nill()
        {
            PersonEmergencyContacts pec = new PersonEmergencyContacts()
            {
                Id = Guid.Empty.ToString()
            };
            await emergencyInformationService.UpdatePersonEmergencyContactsAsync(pec);
        }
        #endregion
    }
    #endregion
}
