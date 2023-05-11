// Copyright 2019-2022 Ellucian Company L.P. and its affiliates.
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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.Base.Tests.Services
{
    [TestClass]
    public class PhoneNumberServiceTests
    {
        // sets up a current user
        public abstract class CurrentUserSetup
        {
            public class QueryPhoneNumbersUserFactory : ICurrentUserFactory
            {
                public ICurrentUser CurrentUser
                {
                    get
                    {
                        return new CurrentUser(new Claims
                        {
                            ControlId = "123",
                            Name = "George",
                            PersonId = "0000015",
                            SecurityToken = "321",
                            SessionTimeout = 30,
                            UserName = "Faculty",
                            Roles = new List<string> { "QueryPhoneNumbers" },
                            SessionFixationId = "abc123"
                        });
                    }
                }
            }
            public class NonQueryPhoneNumbersUserFactory : ICurrentUserFactory
            {
                public ICurrentUser CurrentUser
                {
                    get
                    {
                        return new CurrentUser(new Claims
                        {
                            ControlId = "123",
                            Name = "George",
                            PersonId = "0000015",
                            SecurityToken = "321",
                            SessionTimeout = 30,
                            UserName = "Faculty",
                            Roles = new List<string> { },
                            SessionFixationId = "abc123"
                        });
                    }
                }
            }

            public class PhoneNumberUserToViewPersonInformation : ICurrentUserFactory
            {
                public ICurrentUser CurrentUser
                {
                    get
                    {
                        return new CurrentUser(new Claims()
                        {
                            PersonId = "0000015",
                            ControlId = "123",
                            Name = "George",
                            SecurityToken = "321",
                            SessionTimeout = 30,
                            UserName = "PhoneNumberUserToViewPersonInformation",
                            Roles = new List<string>() { "VIEW.PERSON.INFORMATION" },
                            SessionFixationId = "abc123"
                        });
                    }
                }
            }

        }
        private Domain.Entities.Role queryPhoneNumbersRole;

        private Mock<IPhoneNumberRepository> phoneNumberRepositoryMock;
        private Mock<IConfigurationRepository> configurationRepositoryMock;
        private Mock<IRoleRepository> roleRepositoryMock;
        private Mock<IAdapterRegistry> adapterRegistryMock;
        private Mock<ILogger> loggerMock;

        private IPhoneNumberRepository phoneNumberRepository;
        private IConfigurationRepository configurationRepository;
        private ICurrentUserFactory currentUserFactory;
        private IRoleRepository roleRepository;
        private IAdapterRegistry adapterRegistry;
        private ILogger logger;

        private PhoneNumberService service;

        [TestInitialize]
        public void PhoneNumberServiceTests_Initialize()
        {
            phoneNumberRepositoryMock = new Mock<IPhoneNumberRepository>();
            phoneNumberRepository = phoneNumberRepositoryMock.Object;

            configurationRepositoryMock = new Mock<IConfigurationRepository>();
            configurationRepository = configurationRepositoryMock.Object;

            currentUserFactory = new CurrentUserSetup.QueryPhoneNumbersUserFactory();

            roleRepositoryMock = new Mock<IRoleRepository>();
            queryPhoneNumbersRole = new Domain.Entities.Role(105, "QueryPhoneNumbers");
            queryPhoneNumbersRole.AddPermission(new Domain.Entities.Permission(BasePermissionCodes.QueryPhoneNumbers));
            roleRepositoryMock.Setup(repo => repo.Roles).Returns(new List<Domain.Entities.Role>() { queryPhoneNumbersRole });
            roleRepository = roleRepositoryMock.Object;

            adapterRegistryMock = new Mock<IAdapterRegistry>();
            adapterRegistry = adapterRegistryMock.Object;

            loggerMock = new Mock<ILogger>();
            logger = loggerMock.Object;

            service = new PhoneNumberService(phoneNumberRepository, configurationRepository, currentUserFactory, roleRepository, adapterRegistry, logger);
        }

        [TestClass]
        public class PhoneNumberService_QueryPhoneNumbersAsync_Tests : PhoneNumberServiceTests
        {
            private IEnumerable<PhoneNumber> phoneNumberEntities;

            [TestInitialize]
            public void PhoneNumberService_QueryPhoneNumbersAsync_Tests_Initialize()
            {
                base.PhoneNumberServiceTests_Initialize();

                phoneNumberEntities = new List<PhoneNumber>()
                {
                    new PhoneNumber(currentUserFactory.CurrentUser.PersonId)
                };
                phoneNumberEntities.ElementAt(0).AddPhone(new Domain.Base.Entities.Phone("123-456-7890", "H", "123"));
                phoneNumberRepositoryMock.Setup(repo => repo.GetPersonPhonesByIdsAsync(It.IsAny<List<string>>())).ReturnsAsync(phoneNumberEntities);

                var phoneNumberAdapter = new AutoMapperAdapter<Domain.Base.Entities.PhoneNumber, Dtos.Base.PhoneNumber>(adapterRegistryMock.Object, loggerMock.Object);
                adapterRegistryMock.Setup<ITypeAdapter<Colleague.Domain.Base.Entities.PhoneNumber, Colleague.Dtos.Base.PhoneNumber>>(a => a.GetAdapter<Colleague.Domain.Base.Entities.PhoneNumber,
                    Colleague.Dtos.Base.PhoneNumber>()).Returns(phoneNumberAdapter);
                var phoneAdapter = new AutoMapperAdapter<Domain.Base.Entities.Phone, Dtos.Base.Phone>(adapterRegistryMock.Object, loggerMock.Object);
                adapterRegistryMock.Setup<ITypeAdapter<Colleague.Domain.Base.Entities.Phone, Colleague.Dtos.Base.Phone>>(a => a.GetAdapter<Colleague.Domain.Base.Entities.Phone,
                    Colleague.Dtos.Base.Phone>()).Returns(phoneAdapter);

            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task PhoneNumberService_QueryPhoneNumbersAsync_null_criteria_throws_exception()
            {
                var numbers = await service.QueryPhoneNumbersAsync(null);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task PhoneNumberService_QueryPhoneNumbersAsync_null_criteria_PersonIds_throws_exception()
            {
                var numbers = await service.QueryPhoneNumbersAsync(new Dtos.Base.PhoneNumberQueryCriteria() { PersonIds = null });
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task PhoneNumberService_QueryPhoneNumbersAsync_empty_criteria_PersonIds_throws_exception()
            {
                var numbers = await service.QueryPhoneNumbersAsync(new Dtos.Base.PhoneNumberQueryCriteria() { PersonIds = new List<string>() });
            }

            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public async Task PhoneNumberService_QueryPhoneNumbersAsync_nonmatching_person_ID_without_permission_throws_PermissionsException()
            {
                currentUserFactory = new CurrentUserSetup.NonQueryPhoneNumbersUserFactory();
                service = new PhoneNumberService(phoneNumberRepository, configurationRepository, currentUserFactory, roleRepository, adapterRegistry, logger);

                var numbers = await service.QueryPhoneNumbersAsync(new Dtos.Base.PhoneNumberQueryCriteria() { PersonIds = new List<string>() { currentUserFactory.CurrentUser.PersonId + "1" } });
            }

            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public async Task PhoneNumberService_QueryPhoneNumbersAsync_nonmatching_person_ID_and_matching_person_ID_without_permission_throws_PermissionsException()
            {
                currentUserFactory = new CurrentUserSetup.NonQueryPhoneNumbersUserFactory();
                service = new PhoneNumberService(phoneNumberRepository, configurationRepository, currentUserFactory, roleRepository, adapterRegistry, logger);

                var numbers = await service.QueryPhoneNumbersAsync(new Dtos.Base.PhoneNumberQueryCriteria() { PersonIds = new List<string>() { currentUserFactory.CurrentUser.PersonId + "1", currentUserFactory.CurrentUser.PersonId } });
            }

            [TestMethod]
            public async Task PhoneNumberService_QueryPhoneNumbersAsync_matching_person_ID_without_permission_allowed()
            {
                currentUserFactory = new CurrentUserSetup.NonQueryPhoneNumbersUserFactory();
                service = new PhoneNumberService(phoneNumberRepository, configurationRepository, currentUserFactory, roleRepository, adapterRegistry, logger);

                var numbers = await service.QueryPhoneNumbersAsync(new Dtos.Base.PhoneNumberQueryCriteria() { PersonIds = new List<string>() { currentUserFactory.CurrentUser.PersonId } });
                Assert.IsNotNull(numbers);
            }

            [TestMethod]
            public async Task PhoneNumberService_QueryPhoneNumbersAsync_nonmatching_person_ID_with_permission_allowed()
            {
                var numbers = await service.QueryPhoneNumbersAsync(new Dtos.Base.PhoneNumberQueryCriteria() { PersonIds = new List<string>() { currentUserFactory.CurrentUser.PersonId + "1" } });
                Assert.IsNotNull(numbers);
            }

            [TestMethod]
            public async Task PhoneNumberService_QueryPhoneNumbersAsync_matching_person_ID_with_permission_allowed()
            {
                var numbers = await service.QueryPhoneNumbersAsync(new Dtos.Base.PhoneNumberQueryCriteria() { PersonIds = new List<string>() { currentUserFactory.CurrentUser.PersonId } });
                Assert.IsNotNull(numbers);
            }


            [TestMethod]
            public async Task PhoneNumberService_QueryPhoneNumbersAsync_nonmatching_person_ID_and_matching_person_ID_with_permission_allowed()
            {
                var numbers = await service.QueryPhoneNumbersAsync(new Dtos.Base.PhoneNumberQueryCriteria() { PersonIds = new List<string>() { currentUserFactory.CurrentUser.PersonId + "1", currentUserFactory.CurrentUser.PersonId } });
                Assert.IsNotNull(numbers);
            }
        }

        [TestClass]
        public class PhoneNumberService_QueryPilotPhoneNumbersAsync_Tests : PhoneNumberServiceTests
        {
            private IEnumerable<PilotPhoneNumber> pilotPhoneNumberEntities;

            [TestInitialize]
            public void PhoneNumberService_QueryPilotPhoneNumbersAsync_Tests_Initialize()
            {
                base.PhoneNumberServiceTests_Initialize();

                pilotPhoneNumberEntities = new List<PilotPhoneNumber>()
                {
                    new PilotPhoneNumber(currentUserFactory.CurrentUser.PersonId)
                };
                pilotPhoneNumberEntities.ElementAt(0).PrimaryPhoneNumber = "123-456-7890";
                phoneNumberRepositoryMock.Setup(repo => repo.GetPilotPersonPhonesByIdsAsync(It.IsAny<List<string>>(), It.IsAny<PilotConfiguration>())).ReturnsAsync(pilotPhoneNumberEntities);

                var PilotPhoneNumberAdapter = new AutoMapperAdapter<Domain.Base.Entities.PilotPhoneNumber, Dtos.Base.PilotPhoneNumber>(adapterRegistryMock.Object, loggerMock.Object);
                adapterRegistryMock.Setup<ITypeAdapter<Colleague.Domain.Base.Entities.PilotPhoneNumber, Colleague.Dtos.Base.PilotPhoneNumber>>(a => a.GetAdapter<Colleague.Domain.Base.Entities.PilotPhoneNumber,
                    Colleague.Dtos.Base.PilotPhoneNumber>()).Returns(PilotPhoneNumberAdapter);
                var phoneAdapter = new AutoMapperAdapter<Domain.Base.Entities.Phone, Dtos.Base.Phone>(adapterRegistryMock.Object, loggerMock.Object);
                adapterRegistryMock.Setup<ITypeAdapter<Colleague.Domain.Base.Entities.Phone, Colleague.Dtos.Base.Phone>>(a => a.GetAdapter<Colleague.Domain.Base.Entities.Phone,
                    Colleague.Dtos.Base.Phone>()).Returns(phoneAdapter);

            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task PhoneNumberService_QueryPilotPhoneNumbersAsync_null_criteria_throws_exception()
            {
                var numbers = await service.QueryPilotPhoneNumbersAsync(null);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task PhoneNumberService_QueryPilotPhoneNumbersAsync_null_criteria_PersonIds_throws_exception()
            {
                var numbers = await service.QueryPilotPhoneNumbersAsync(new Dtos.Base.PhoneNumberQueryCriteria() { PersonIds = null });
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task PhoneNumberService_QueryPilotPhoneNumbersAsync_empty_criteria_PersonIds_throws_exception()
            {
                var numbers = await service.QueryPilotPhoneNumbersAsync(new Dtos.Base.PhoneNumberQueryCriteria() { PersonIds = new List<string>() });
            }

            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public async Task PhoneNumberService_QueryPilotPhoneNumbersAsync_nonmatching_person_ID_without_permission_throws_PermissionsException()
            {
                currentUserFactory = new CurrentUserSetup.NonQueryPhoneNumbersUserFactory();
                service = new PhoneNumberService(phoneNumberRepository, configurationRepository, currentUserFactory, roleRepository, adapterRegistry, logger);

                var numbers = await service.QueryPilotPhoneNumbersAsync(new Dtos.Base.PhoneNumberQueryCriteria() { PersonIds = new List<string>() { currentUserFactory.CurrentUser.PersonId + "1" } });
            }

            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public async Task PhoneNumberService_QueryPilotPhoneNumbersAsync_nonmatching_person_ID_and_matching_person_ID_without_permission_throws_PermissionsException()
            {
                currentUserFactory = new CurrentUserSetup.NonQueryPhoneNumbersUserFactory();
                service = new PhoneNumberService(phoneNumberRepository, configurationRepository, currentUserFactory, roleRepository, adapterRegistry, logger);

                var numbers = await service.QueryPilotPhoneNumbersAsync(new Dtos.Base.PhoneNumberQueryCriteria() { PersonIds = new List<string>() { currentUserFactory.CurrentUser.PersonId + "1", currentUserFactory.CurrentUser.PersonId } });
            }

            [TestMethod]
            public async Task PhoneNumberService_QueryPilotPhoneNumbersAsync_matching_person_ID_without_permission_allowed()
            {
                currentUserFactory = new CurrentUserSetup.NonQueryPhoneNumbersUserFactory();
                service = new PhoneNumberService(phoneNumberRepository, configurationRepository, currentUserFactory, roleRepository, adapterRegistry, logger);

                var numbers = await service.QueryPilotPhoneNumbersAsync(new Dtos.Base.PhoneNumberQueryCriteria() { PersonIds = new List<string>() { currentUserFactory.CurrentUser.PersonId } });
                Assert.IsNotNull(numbers);
            }

            [TestMethod]
            public async Task PhoneNumberService_QueryPilotPhoneNumbersAsync_nonmatching_person_ID_with_permission_allowed()
            {
                var numbers = await service.QueryPilotPhoneNumbersAsync(new Dtos.Base.PhoneNumberQueryCriteria() { PersonIds = new List<string>() { currentUserFactory.CurrentUser.PersonId + "1" } });
                Assert.IsNotNull(numbers);
            }

            [TestMethod]
            public async Task PhoneNumberService_QueryPilotPhoneNumbersAsync_matching_person_ID_with_permission_allowed()
            {
                var numbers = await service.QueryPilotPhoneNumbersAsync(new Dtos.Base.PhoneNumberQueryCriteria() { PersonIds = new List<string>() { currentUserFactory.CurrentUser.PersonId } });
                Assert.IsNotNull(numbers);
            }


            [TestMethod]
            public async Task PhoneNumberService_QueryPilotPhoneNumbersAsync_nonmatching_person_ID_and_matching_person_ID_with_permission_allowed()
            {
                var numbers = await service.QueryPilotPhoneNumbersAsync(new Dtos.Base.PhoneNumberQueryCriteria() { PersonIds = new List<string>() { currentUserFactory.CurrentUser.PersonId + "1", currentUserFactory.CurrentUser.PersonId } });
                Assert.IsNotNull(numbers);
            }
        }

        [TestClass]
        public class GetPersonPhones2Async_Tests : PhoneNumberServiceTests
        {          
            private PhoneNumber phoneNumber;
   
            [TestMethod]
            public async Task GetPersonPhones2Async_ViewPersonInformation_Permission()
            {
                string personId = "0000015";
                phoneNumber = new PhoneNumber(personId);
                phoneNumber.AddPhone(new Phone("703-815-4221", "HO", ""));
                phoneNumber.AddPhone(new Phone("304-577-9951", "BU", "123"));

                var viewPersonInformationRole = new Domain.Entities.Role(1, "VIEW.PERSON.INFORMATION");
                viewPersonInformationRole.AddPermission(new Domain.Entities.Permission("VIEW.PERSON.INFORMATION"));
                currentUserFactory = new CurrentUserSetup.PhoneNumberUserToViewPersonInformation();

                roleRepositoryMock.Setup(repo => repo.GetRolesAsync()).ReturnsAsync(new List<Domain.Entities.Role>()
                {
                    viewPersonInformationRole
                });
               
                phoneNumberRepositoryMock.Setup(i =>
                            i.GetPersonPhonesAsync(It.IsAny<string>()))
                            .ReturnsAsync(phoneNumber);
                var phoneNumberAdapter = new AutoMapperAdapter<Domain.Base.Entities.PhoneNumber, Dtos.Base.PhoneNumber>(adapterRegistryMock.Object, loggerMock.Object);
                adapterRegistryMock.Setup<ITypeAdapter<Colleague.Domain.Base.Entities.PhoneNumber, Colleague.Dtos.Base.PhoneNumber>>(a => a.GetAdapter<Colleague.Domain.Base.Entities.PhoneNumber,
                    Colleague.Dtos.Base.PhoneNumber>()).Returns(phoneNumberAdapter);
                var phoneAdapter = new AutoMapperAdapter<Domain.Base.Entities.Phone, Dtos.Base.Phone>(adapterRegistryMock.Object, loggerMock.Object);
                adapterRegistryMock.Setup<ITypeAdapter<Colleague.Domain.Base.Entities.Phone, Colleague.Dtos.Base.Phone>>(a => a.GetAdapter<Colleague.Domain.Base.Entities.Phone,
                    Colleague.Dtos.Base.Phone>()).Returns(phoneAdapter);
                service = new PhoneNumberService(phoneNumberRepository, configurationRepository, currentUserFactory, roleRepository, adapterRegistry, logger);
                var actuals = await service.GetPersonPhones2Async(personId);
                Assert.IsNotNull(actuals);
                var expected = phoneNumber;
                Assert.AreEqual(expected.PersonId, actuals.PersonId);
                Assert.AreEqual(expected.PhoneNumbers.Count(), actuals.PhoneNumbers.Count());
                foreach (var expectedPhone in expected.PhoneNumbers)
                {
                    var actualPhone = actuals.PhoneNumbers.Where(p => p.Number == expectedPhone.Number).First();
                    Assert.AreEqual(expectedPhone.TypeCode, actualPhone.TypeCode);
                    Assert.AreEqual(expectedPhone.Extension, actualPhone.Extension);
                    Assert.AreEqual(expectedPhone.Number, actualPhone.Number);
                }
            }

            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public async Task GetPersonPhones2Async_NotAppropriatePermission_NotSelf()
            {
                string personId = "9999998";
                phoneNumber = new PhoneNumber(personId);
                phoneNumber.AddPhone(new Phone("703-815-4221", "HO", ""));
                phoneNumber.AddPhone(new Phone("304-577-9951", "BU", "123"));

                var viewPersonInformationRole = new Domain.Entities.Role(1, "QUERY.PHONE.NUMBERS");
                viewPersonInformationRole.AddPermission(new Domain.Entities.Permission("QUERY.PHONE.NUMBERS"));
                currentUserFactory = new CurrentUserSetup.NonQueryPhoneNumbersUserFactory();

                roleRepositoryMock.Setup(repo => repo.GetRolesAsync()).ReturnsAsync(new List<Domain.Entities.Role>()
                {
                    viewPersonInformationRole
                });
                phoneNumberRepositoryMock.Setup(i =>
                            i.GetPersonPhonesAsync(It.IsAny<string>()))
                            .ReturnsAsync(phoneNumber);
                var phoneNumberAdapter = new AutoMapperAdapter<Domain.Base.Entities.PhoneNumber, Dtos.Base.PhoneNumber>(adapterRegistryMock.Object, loggerMock.Object);
                adapterRegistryMock.Setup<ITypeAdapter<Colleague.Domain.Base.Entities.PhoneNumber, Colleague.Dtos.Base.PhoneNumber>>(a => a.GetAdapter<Colleague.Domain.Base.Entities.PhoneNumber,
                    Colleague.Dtos.Base.PhoneNumber>()).Returns(phoneNumberAdapter);
                var phoneAdapter = new AutoMapperAdapter<Domain.Base.Entities.Phone, Dtos.Base.Phone>(adapterRegistryMock.Object, loggerMock.Object);
                adapterRegistryMock.Setup<ITypeAdapter<Colleague.Domain.Base.Entities.Phone, Colleague.Dtos.Base.Phone>>(a => a.GetAdapter<Colleague.Domain.Base.Entities.Phone,
                    Colleague.Dtos.Base.Phone>()).Returns(phoneAdapter);
                service = new PhoneNumberService(phoneNumberRepository, configurationRepository, currentUserFactory, roleRepository, adapterRegistry, logger);
                var actuals = await service.GetPersonPhones2Async(personId);
                Assert.IsNull(actuals);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task GetPersonAddresses2Async_ArgumentNullException()
            {

                string personId = "9999998";
                phoneNumber = new PhoneNumber(personId);
                phoneNumber.AddPhone(new Phone("703-815-4221", "HO", ""));
                phoneNumber.AddPhone(new Phone("304-577-9951", "BU", "123"));

                var viewPersonInformationRole = new Domain.Entities.Role(1, "VIEW.PERSON.INFORMATION");
                viewPersonInformationRole.AddPermission(new Domain.Entities.Permission("VIEW.PERSON.INFORMATION"));
                currentUserFactory = new CurrentUserSetup.NonQueryPhoneNumbersUserFactory();

                roleRepositoryMock.Setup(repo => repo.GetRolesAsync()).ReturnsAsync(new List<Domain.Entities.Role>()
                {
                    viewPersonInformationRole
                });
                phoneNumberRepositoryMock.Setup(i =>
                            i.GetPersonPhonesAsync(It.IsAny<string>()))
                            .ReturnsAsync(phoneNumber);
                var phoneNumberAdapter = new AutoMapperAdapter<Domain.Base.Entities.PhoneNumber, Dtos.Base.PhoneNumber>(adapterRegistryMock.Object, loggerMock.Object);
                adapterRegistryMock.Setup<ITypeAdapter<Colleague.Domain.Base.Entities.PhoneNumber, Colleague.Dtos.Base.PhoneNumber>>(a => a.GetAdapter<Colleague.Domain.Base.Entities.PhoneNumber,
                    Colleague.Dtos.Base.PhoneNumber>()).Returns(phoneNumberAdapter);
                var phoneAdapter = new AutoMapperAdapter<Domain.Base.Entities.Phone, Dtos.Base.Phone>(adapterRegistryMock.Object, loggerMock.Object);
                adapterRegistryMock.Setup<ITypeAdapter<Colleague.Domain.Base.Entities.Phone, Colleague.Dtos.Base.Phone>>(a => a.GetAdapter<Colleague.Domain.Base.Entities.Phone,
                    Colleague.Dtos.Base.Phone>()).Returns(phoneAdapter);
                service = new PhoneNumberService(phoneNumberRepository, configurationRepository, currentUserFactory, roleRepository, adapterRegistry, logger);
                var actuals = await service.GetPersonPhones2Async(null);
            }
        }
    }
}
