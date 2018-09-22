// Copyright 2016 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Coordination.Base.Adapters;
using Ellucian.Colleague.Dtos.Base;
using Ellucian.Web.Adapters;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;
using System;
using System.Collections.Generic;

namespace Ellucian.Colleague.Coordination.Base.Tests.Adapters
{
    [TestClass]
    public class PersonProxyUserDtoAdapterTests
    {
        public Mock<IAdapterRegistry> adapterRegistryMock;
        public Mock<ILogger> loggerMock;
        public PersonProxyUserDtoAdapter mapper;
        public PersonNameDtoAdapter nameMapper;
        public EmailAddressDtoAdapter emailMapper;
        public PhoneDtoAdapter phoneMapper;

        Dtos.Base.PersonProxyUser userDto;
        Dtos.Base.PersonProxyUser emptyUserDto;

        [TestInitialize]
        public void Initialize()
        {
            adapterRegistryMock = new Mock<IAdapterRegistry>();
            loggerMock = new Mock<ILogger>();
            mapper = new PersonProxyUserDtoAdapter(adapterRegistryMock.Object, loggerMock.Object);
            nameMapper = new PersonNameDtoAdapter(adapterRegistryMock.Object, loggerMock.Object);
            emailMapper = new EmailAddressDtoAdapter(adapterRegistryMock.Object, loggerMock.Object);
            phoneMapper = new PhoneDtoAdapter(adapterRegistryMock.Object, loggerMock.Object);


            userDto = new Dtos.Base.PersonProxyUser()
            {
                BirthDate = DateTime.Now.Date.AddDays(-1),
                EmailAddresses = new List<EmailAddress>()
                {
                    new EmailAddress(){Value = "mail1@mail.com", TypeCode = "PRI",},
                    new EmailAddress(){Value = "mail2@mail.com", TypeCode = "BUS",},
                },
                FirstName = "given",
                FormerNames = new List<PersonName>()
                {
                    new PersonName(){GivenName = "formerGiven1", MiddleName = "formerMiddle1", FamilyName = "formerFamily1",},
                    new PersonName(){GivenName = "formerGiven2", MiddleName = "formerMiddle2", FamilyName = "formerFamily2",},
                },
                Gender = "M",
                GovernmentId = "987654321",
                Id = "0000001",
                LastName = "Family",
                MiddleName = "Middle",
                Phones = new List<Phone>()
                {
                    new Phone(){Number = "phone1", Extension = "ext1", TypeCode = "HO",},
                    new Phone(){Number = "phone2", Extension = "ext2", TypeCode = "HO",},
                },
                Prefix = "MR",
                Suffix = "JR",
                PrivacyStatusCode = null
            };

            emptyUserDto = new Dtos.Base.PersonProxyUser()
            {
                BirthDate = DateTime.Now.Date.AddDays(-1),
                EmailAddresses = new List<EmailAddress>()
                {
                    new EmailAddress(){Value = "mail1@mail.com", TypeCode = "PRI",},
                    new EmailAddress(){Value = "mail2@mail.com", TypeCode = "BUS",},
                },
                FirstName = "given",
                FormerNames = new List<PersonName>(),
                Id = "0000001",
                LastName = "Family",
                Phones = new List<Phone>(),
                PrivacyStatusCode = null
            };
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void PersonProxyUserDtoAdapter_MapToType_Null()
        {
            mapper.MapToType(null);
        }

        [TestMethod]
        public void PersonProxyUserDtoAdapter_MapToType_Valid()
        {
            adapterRegistryMock.Setup(n =>
                n.GetAdapter<Dtos.Base.PersonName, Domain.Base.Entities.PersonName>())
                .Returns(nameMapper);

            adapterRegistryMock.Setup(e =>
                e.GetAdapter<Dtos.Base.EmailAddress, Domain.Base.Entities.EmailAddress>())
                .Returns(emailMapper);

            adapterRegistryMock.Setup(p =>
                p.GetAdapter<Dtos.Base.Phone, Domain.Base.Entities.Phone>())
                .Returns(phoneMapper);

            var userEnt = mapper.MapToType(userDto);
            Assert.AreEqual(userDto.BirthDate, userEnt.BirthDate);
            Assert.AreEqual(userDto.EmailAddresses.Count, userEnt.EmailAddresses.Count);
            Assert.AreEqual(userDto.EmailAddresses[0].Value, userEnt.EmailAddresses[0].Value);
            Assert.AreEqual(userDto.EmailAddresses[0].TypeCode, userEnt.EmailAddresses[0].TypeCode);
            Assert.AreEqual(userDto.EmailAddresses[1].Value, userEnt.EmailAddresses[1].Value);
            Assert.AreEqual(userDto.EmailAddresses[1].TypeCode, userEnt.EmailAddresses[1].TypeCode);
            Assert.AreEqual(userDto.FirstName, userEnt.FirstName);
            Assert.AreEqual(userDto.FormerNames.Count, userEnt.FormerNames.Count);
            Assert.AreEqual(userDto.FormerNames[0].GivenName, userEnt.FormerNames[0].GivenName);
            Assert.AreEqual(userDto.FormerNames[0].MiddleName, userEnt.FormerNames[0].MiddleName);
            Assert.AreEqual(userDto.FormerNames[0].FamilyName, userEnt.FormerNames[0].FamilyName);
            Assert.AreEqual(userDto.FormerNames[1].GivenName, userEnt.FormerNames[1].GivenName);
            Assert.AreEqual(userDto.FormerNames[1].MiddleName, userEnt.FormerNames[1].MiddleName);
            Assert.AreEqual(userDto.FormerNames[1].FamilyName, userEnt.FormerNames[1].FamilyName);
            Assert.AreEqual(userDto.Gender, userEnt.Gender);
            Assert.AreEqual(userDto.GovernmentId, userEnt.GovernmentId);
            Assert.AreEqual(userDto.Id, userEnt.Id);
            Assert.AreEqual(userDto.LastName, userEnt.LastName);
            Assert.AreEqual(userDto.MiddleName, userEnt.MiddleName);
            Assert.AreEqual(userDto.Prefix, userEnt.Prefix);
            Assert.AreEqual(userDto.Suffix, userEnt.Suffix);
            Assert.AreEqual(userDto.Phones.Count, userEnt.Phones.Count);
            Assert.AreEqual(userDto.Phones[0].Number, userEnt.Phones[0].Number);
            Assert.AreEqual(userDto.Phones[0].TypeCode, userEnt.Phones[0].TypeCode);
            Assert.AreEqual(userDto.Phones[0].Extension, userEnt.Phones[0].Extension);
            Assert.AreEqual(userDto.Phones[1].Number, userEnt.Phones[1].Number);
            Assert.AreEqual(userDto.Phones[1].TypeCode, userEnt.Phones[1].TypeCode);
            Assert.AreEqual(userDto.Phones[1].Extension, userEnt.Phones[1].Extension);
            Assert.AreEqual(userDto.PrivacyStatusCode, userEnt.PrivacyStatusCode);
        }

        [TestMethod]
        public void PersonProxyUserDtoAdapter_MapToType_Valid_NullGroups()
        {
            adapterRegistryMock.Setup(n =>
                n.GetAdapter<Dtos.Base.PersonName, Domain.Base.Entities.PersonName>())
                .Returns(nameMapper);

            adapterRegistryMock.Setup(e =>
                e.GetAdapter<Dtos.Base.EmailAddress, Domain.Base.Entities.EmailAddress>())
                .Returns(emailMapper);

            adapterRegistryMock.Setup(p =>
                p.GetAdapter<Dtos.Base.Phone, Domain.Base.Entities.Phone>())
                .Returns(phoneMapper);

            var userEnt = mapper.MapToType(emptyUserDto);
            Assert.AreEqual(emptyUserDto.BirthDate, userEnt.BirthDate);
            Assert.AreEqual(emptyUserDto.EmailAddresses.Count, userEnt.EmailAddresses.Count);
            Assert.AreEqual(emptyUserDto.EmailAddresses[0].Value, userEnt.EmailAddresses[0].Value);
            Assert.AreEqual(emptyUserDto.EmailAddresses[0].TypeCode, userEnt.EmailAddresses[0].TypeCode);
            Assert.AreEqual(emptyUserDto.EmailAddresses[1].Value, userEnt.EmailAddresses[1].Value);
            Assert.AreEqual(emptyUserDto.EmailAddresses[1].TypeCode, userEnt.EmailAddresses[1].TypeCode);
            Assert.AreEqual(emptyUserDto.FirstName, userEnt.FirstName);
            Assert.AreEqual(emptyUserDto.FormerNames.Count, userEnt.FormerNames.Count);
            Assert.AreEqual(string.Empty, userEnt.Gender);
            Assert.AreEqual(string.Empty, userEnt.GovernmentId);
            Assert.AreEqual(emptyUserDto.Id, userEnt.Id);
            Assert.AreEqual(emptyUserDto.LastName, userEnt.LastName);
            Assert.AreEqual(string.Empty, userEnt.MiddleName);
            Assert.AreEqual(string.Empty, userEnt.Prefix);
            Assert.AreEqual(string.Empty, userEnt.Suffix);
            Assert.AreEqual(emptyUserDto.Phones.Count, userEnt.Phones.Count);
            Assert.AreEqual(emptyUserDto.PrivacyStatusCode, userEnt.PrivacyStatusCode);
        }
    }
}
