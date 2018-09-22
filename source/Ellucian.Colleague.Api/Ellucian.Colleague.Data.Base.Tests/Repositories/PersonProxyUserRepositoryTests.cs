// Copyright 2016 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ellucian.Colleague.Data.Base.DataContracts;
using Ellucian.Colleague.Data.Base.Repositories;
using Ellucian.Colleague.Data.Base.Transactions;
using Ellucian.Colleague.Domain.Base.Entities;
using Ellucian.Colleague.Domain.Exceptions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Ellucian.Colleague.Data.Base.Tests.Repositories
{
    [TestClass]
    public class PersonProxyUserRepositoryTests : BaseRepositorySetup
    {
        PersonProxyUserRepository repository;

        [TestInitialize]
        public void Initialize()
        {
            // Initialize Mock framework
            MockInitialize();

            // Build the test repository
            repository = new PersonProxyUserRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object, apiSettings);
        }

        [TestCleanup]
        public void Cleanup()
        {
            repository = null;
        }

        [TestClass]
        public class PersonProxyUserRepository_PostPersonProxyUserAsync : PersonProxyUserRepositoryTests
        {
            Domain.Base.Entities.PersonProxyUser userEnt;
            DataContracts.PersonProxyUser userData;
            CreatePersonProxyUserRequest createRequest;
            CreatePersonProxyUserResponse validResponse, errorResponse, nullResponse;
            protected string 
                mail1 = "mail1@mail.com",
                mailType1 = "PRI",
                mail2 = "mail2@mail.com",
                mailType2 = "BUS";

            protected string
                phone1 = "Phone1",
                phoneType1 = "HO",
                phoneExt1 = "Ext1",
                phone2 = "phone2",
                phoneType2 = "HO",
                phoneExt2 = "Ext2";

            protected string
                nameGiven1 = "Given1",
                nameMiddle1 = "Middle1",
                nameFamily1 = "Family1",
                nameGiven2 = "Given2",
                nameMiddle2 = "Middle2",
                nameFamily2 = "Family2";

            protected string
                given = "Given",
                middle = "Middle",
                family = "Family",
                gender = "M",
                ssn = "987654321",
                id = "0000001",
                prefix = "MR",
                suffix = "JR";

            List<EmailAddress> emails;
            List<Phone> phones;
            List<PersonName> names;


            protected DateTime birth = DateTime.Now.AddDays(-1);

            [TestInitialize]
            public void ProxyRepository_PostPersonProxyUserAsync_Initialize()
            {
                validResponse = new CreatePersonProxyUserResponse()
                {
                    PersonId = id,
                    ErrorMessages = new List<string>(),
                };
                errorResponse = new CreatePersonProxyUserResponse()
                {
                    PersonId = string.Empty,
                    ErrorMessages = new List<string>() { "This is an error message." }
                };
                emails = new List<EmailAddress>(){new EmailAddress(mail1, mailType1),
                                                      new EmailAddress(mail2, mailType2)};
                phones = new List<Phone>() { new Phone(phone1, phoneType1, phoneExt1),
                                                 new Phone(phone2, phoneType2, phoneExt2), };
                names = new List<PersonName>(){new PersonName(nameGiven1, nameMiddle1, nameFamily1),
                                                   new PersonName(nameGiven2, nameMiddle2, nameFamily2),};

                userEnt = new Domain.Base.Entities.PersonProxyUser(null, given, family, emails, phones, names)
                {
                    BirthDate = birth,
                    Gender = gender,
                    GovernmentId = ssn,
                    MiddleName = middle,
                    Prefix = prefix,
                    Suffix = suffix,
                };

                userData = new Data.Base.DataContracts.PersonProxyUser()
                {
                    BirthDate = birth,
                    FirstName = given,
                    Gender = gender,
                    LastName = family,
                    MiddleName = middle,
                    NamehistEntityAssociation = new List<PersonProxyUserNamehist>()
                    {
                        new PersonProxyUserNamehist(){NameHistoryFirstNameAssocMember = nameGiven1, NameHistoryMiddleNameAssocMember = nameMiddle1, NameHistoryLastNameAssocMember = nameFamily1,},
                        new PersonProxyUserNamehist(){NameHistoryFirstNameAssocMember = nameGiven2, NameHistoryMiddleNameAssocMember = nameMiddle2, NameHistoryLastNameAssocMember = nameFamily2,},
                    },
                    PeopleEmailEntityAssociation = new List<PersonProxyUserPeopleEmail>()
                    {
                        new PersonProxyUserPeopleEmail(){PersonEmailAddressesAssocMember = mail1, PersonEmailTypesAssocMember = mailType1,},
                        new PersonProxyUserPeopleEmail(){PersonEmailAddressesAssocMember = mail2, PersonEmailTypesAssocMember = mailType2,},
                    },
                    PerphoneEntityAssociation = new List<PersonProxyUserPerphone>()
                    {
                        new PersonProxyUserPerphone(){PersonalPhoneNumberAssocMember = phone1, PersonalPhoneExtensionAssocMember = phoneExt1, PersonalPhoneTypeAssocMember = phoneType1,},
                        new PersonProxyUserPerphone(){PersonalPhoneNumberAssocMember = phone2, PersonalPhoneExtensionAssocMember = phoneExt2, PersonalPhoneTypeAssocMember = phoneType2,},
                    },
                    Prefix = prefix,
                    Recordkey = id,
                    Ssn = ssn,
                    Suffix = suffix,
                };
            }

            [TestMethod]
            public async Task PersonProxyUserRepository_PostPersonProxyUserAsync_Valid()
            {

                transManagerMock.Setup(mgr =>
                    mgr.ExecuteAsync<CreatePersonProxyUserRequest, CreatePersonProxyUserResponse>(It.IsAny<CreatePersonProxyUserRequest>()))
                    .Returns(Task.FromResult<CreatePersonProxyUserResponse>(validResponse))
                    .Callback<CreatePersonProxyUserRequest>(req => createRequest = req);

                dataReaderMock.Setup<Task<DataContracts.PersonProxyUser>>(accessor =>
                    accessor.ReadRecordAsync<DataContracts.PersonProxyUser>
                    (It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(userData);

                var result = await repository.CreatePersonProxyUserAsync(userEnt);

                Assert.AreEqual(validResponse.PersonId, result.Id);
                CollectionAssert.AreEqual(userEnt.EmailAddresses, emails);
                CollectionAssert.AreEqual(userEnt.Phones, phones);
                CollectionAssert.AreEqual(userEnt.FormerNames, names);
                Assert.AreEqual(birth, result.BirthDate);
                Assert.AreEqual(gender,result.Gender);
                Assert.AreEqual(ssn,result.GovernmentId);
                Assert.AreEqual(prefix,result.Prefix);
                Assert.AreEqual(suffix, result.Suffix);
                Assert.AreEqual(given, result.FirstName);
                Assert.AreEqual(middle, result.MiddleName);
                Assert.AreEqual(family, result.LastName);
            }

            [TestMethod]
            [ExpectedException(typeof(RepositoryException))]
            public async Task PersonProxyUserRepository_PostPersonProxyUserAsync_ErrorDuringProcessing()
            {
                transManagerMock.Setup(mgr =>
                    mgr.ExecuteAsync<CreatePersonProxyUserRequest, CreatePersonProxyUserResponse>(It.IsAny<CreatePersonProxyUserRequest>()))
                    .Returns(Task.FromResult<CreatePersonProxyUserResponse>(errorResponse))
                    .Callback<CreatePersonProxyUserRequest>(req => createRequest = req);

                var result = await repository.CreatePersonProxyUserAsync(userEnt);
            }

            [TestMethod]
            [ExpectedException(typeof(RepositoryException))]
            public async Task PersonProxyUserRepository_PostPersonProxyUserAsync_NullResponse()
            {
                transManagerMock.Setup(mgr =>
                    mgr.ExecuteAsync<CreatePersonProxyUserRequest, CreatePersonProxyUserResponse>(It.IsAny<CreatePersonProxyUserRequest>()))
                    .Returns(Task.FromResult<CreatePersonProxyUserResponse>(nullResponse))
                    .Callback<CreatePersonProxyUserRequest>(req => createRequest = req);

                var result = await repository.CreatePersonProxyUserAsync(userEnt);
            }

            [TestMethod]
            [ExpectedException(typeof(RepositoryException))]
            public async Task PersonProxyUserRepository_PostPersonProxyUserAsync_NullDataRead()
            {
                transManagerMock.Setup(mgr =>
                    mgr.ExecuteAsync<CreatePersonProxyUserRequest, CreatePersonProxyUserResponse>(It.IsAny<CreatePersonProxyUserRequest>()))
                    .Returns(Task.FromResult<CreatePersonProxyUserResponse>(validResponse))
                    .Callback<CreatePersonProxyUserRequest>(req => createRequest = req);
                DataContracts.PersonProxyUser nullRead = null;

                dataReaderMock.Setup<Task<DataContracts.PersonProxyUser>>(accessor =>
                    accessor.ReadRecordAsync<DataContracts.PersonProxyUser>
                    (It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(nullRead);

                var result = await repository.CreatePersonProxyUserAsync(userEnt);
            }
        }
    }
}
