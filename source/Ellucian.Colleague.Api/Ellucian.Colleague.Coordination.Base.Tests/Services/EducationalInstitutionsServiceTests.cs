// Copyright 2019 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.Base.Entities;
using Ellucian.Colleague.Domain.Base.Tests;
using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Web.Adapters;
using Ellucian.Web.Security;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Colleague.Domain.Base;
using Ellucian.Colleague.Dtos.EnumProperties;

namespace Ellucian.Colleague.Coordination.Base.Tests.Services
{
    public class EducationalInstitutionsServiceTests
    {
        // Sets up a Current user that is an advisor
        public abstract class CurrentUserSetup
        {
            protected Domain.Entities.Role facultyRole = new Domain.Entities.Role(105, "Faculty");

            public class FacultyUserFactory : ICurrentUserFactory
            {
                public ICurrentUser CurrentUser
                {
                    get
                    {
                        return new CurrentUser(new Claims()
                        {
                            ControlId = "123",
                            Name = "George",
                            PersonId = "0000011",
                            SecurityToken = "321",
                            SessionTimeout = 30,
                            UserName = "Faculty",
                            Roles = new List<string>() { "Faculty" },
                            SessionFixationId = "abc123"
                        });
                    }
                }
            }
        }

        [TestClass]
        public class GetEducationalInstitutionsServiceTests : CurrentUserSetup
        {
            private Mock<IReferenceDataRepository> _referenceDataRepositoryMock;
            private IReferenceDataRepository _referenceDataRepository;

            private Mock<IInstitutionRepository> _institutionRepositoryMock;
            private IInstitutionRepository _institutionRepository;


            private Mock<IConfigurationRepository> _configurationRepoMock;
            private IConfigurationRepository _configurationRepo;
            private IPersonRepository _personRepository;
            private Mock<IPersonRepository> _personRepositoryMock;
            private ILogger _logger;
            private IAdapterRegistry _adapterRegistry;
            private Mock<IAdapterRegistry> _adapterRegistryMock;
            private ICurrentUserFactory _currentUserFactory;
            private Mock<ICurrentUserFactory> _currentUserFactoryMock;
            private IRoleRepository _roleRepository;
            private Mock<IRoleRepository> _roleRepositoryMock;


            private Domain.Entities.Permission permissionViewEducationalInstitution;

            private List<Domain.Base.Entities.Institution> _institutionsCollection;

            private EducationalInstitutionsService _educationalInstitutionsService;
            private List<Department> _allDepartments;
            private List<Division> _allDivisions;
            private List<School> _allSchools;

            private const string DepartmentGuid = "6d6040a5-1a98-4614-943d-ad20101ff057"; //BIOLOGY
            private const string DefaultHostGuid = "7y6040a5-2a98-4614-923d-ad20101ff088";
            private const string DivisionGuid = "50052c84-9f25-4f08-bd13-48e2a2ec4f49";
            private const string SchoolGuid = "62052c84-9f25-4f08-bd13-48e2a2ec4f49";

            private Domain.Base.Entities.SocialMediaType socialMediaType;
            private List<Domain.Base.Entities.EmailType> emailTypes;
            private Domain.Base.Entities.AddressType2 addressType;
            private Domain.Base.Entities.PhoneType phoneType;
            private IEnumerable<Domain.Base.Entities.SocialMediaType> allSocialMediaTypes;

            private Dictionary<string, string> idCollection;
            private List<string> ids;


            [TestInitialize]
            public void Initialize()
            {
                permissionViewEducationalInstitution = new Ellucian.Colleague.Domain.Entities.Permission(BasePermissionCodes.ViewEducationalInstitution);

                _referenceDataRepositoryMock = new Mock<IReferenceDataRepository>();
                _referenceDataRepository = _referenceDataRepositoryMock.Object;
                _logger = new Mock<ILogger>().Object;
                _configurationRepoMock = new Mock<IConfigurationRepository>();
                _configurationRepo = _configurationRepoMock.Object;
                _personRepositoryMock = new Mock<IPersonRepository>();
                _personRepository = _personRepositoryMock.Object;
                _adapterRegistryMock = new Mock<IAdapterRegistry>();
                _adapterRegistry = _adapterRegistryMock.Object;
                _currentUserFactoryMock = new Mock<ICurrentUserFactory>();
                _currentUserFactory = _currentUserFactoryMock.Object;
                _roleRepositoryMock = new Mock<IRoleRepository>();
                _roleRepository = _roleRepositoryMock.Object;
                _institutionRepositoryMock = new Mock<IInstitutionRepository>();
                _institutionRepository = _institutionRepositoryMock.Object;

                _institutionsCollection = new List<Institution>();

                emailTypes = new List<Domain.Base.Entities.EmailType>() {
                        new Domain.Base.Entities.EmailType("899803da-48f8-4044-beb8-5913a04b995d", "COL", "College", EmailTypeCategory.School),
                        new Domain.Base.Entities.EmailType("301d485d-d37b-4d29-af00-465ced624a85", "PER", "Personal", EmailTypeCategory.Personal),
                        new Domain.Base.Entities.EmailType("53fb7dab-d348-4657-b071-45d0e5933e05", "BUS", "Business", EmailTypeCategory.Business)
                       };

                var socialMediaTypeAdapter =
                    new Coordination.Base.Adapters.PersonIntgSocialMediaEntityToDtoAdapter(_adapterRegistry, _logger);

                _adapterRegistryMock.Setup(reg => reg.GetAdapter<Tuple<IEnumerable<Domain.Base.Entities.SocialMedia>, IEnumerable<Domain.Base.Entities.SocialMediaType>>,
                       IEnumerable<Dtos.DtoProperties.PersonSocialMediaDtoProperty>>()).Returns(socialMediaTypeAdapter);

                var addressDtoAdapter = new Coordination.Base.Adapters.AddressDtoAdapter(_adapterRegistry, _logger);
                _adapterRegistryMock.Setup(reg =>
                    reg.GetAdapter<Ellucian.Colleague.Dtos.Base.Address, Ellucian.Colleague.Domain.Base.Entities.Address>())
                    .Returns(addressDtoAdapter);

                var allAddressTypes = new TestAddressTypeRepository().Get();
                _referenceDataRepositoryMock.Setup(repo => repo.GetAddressTypes2Async(It.IsAny<bool>())).ReturnsAsync(allAddressTypes);
                addressType = allAddressTypes.FirstOrDefault(x => x.Code == "H");

                var allPhoneTypes = new TestPhoneTypeRepository().Get();
                _referenceDataRepositoryMock.Setup(repo => repo.GetPhoneTypesAsync(It.IsAny<bool>())).ReturnsAsync(allPhoneTypes);
                phoneType = allPhoneTypes.FirstOrDefault(x => x.Code == "H");

                allSocialMediaTypes = new TestSocialMediaTypesRepository().GetSocialMediaTypes();
                socialMediaType = allSocialMediaTypes.FirstOrDefault(x => x.Code.Equals("FB", StringComparison.OrdinalIgnoreCase));

                var address = new Domain.Base.Entities.Address("1", false)
                {
                    AddressLines = new List<string>() { "line 1" },
                    City = "City 1",
                    State = "State 1",
                    PostalCode = "12345",
                    Type = addressType.Description,
                    TypeCode = addressType.Code
                };

                var institution1 = new Institution("1", InstType.HighSchool)
                {
                    Name = "Test 1",
                    Ceeb = "000001",
                    Addresses = new List<Domain.Base.Entities.Address>() { address },
                    EmailAddresses = new List<Domain.Base.Entities.EmailAddress>() { new Domain.Base.Entities.EmailAddress("test@test.com", "PER") },
                    SocialMedia = new List<SocialMedia>() { new SocialMedia(socialMediaType.Code, "handle") },
                    Phones = new List<Domain.Base.Entities.Phone>() { new Domain.Base.Entities.Phone("111-111-1111", "H") }
                    

                };
                _institutionsCollection.Add(institution1);


                _referenceDataRepositoryMock.Setup(repo => repo.GetEmailTypesAsync(It.IsAny<bool>()))
                    .ReturnsAsync(
                        new List<Domain.Base.Entities.EmailType>() {
                        new Domain.Base.Entities.EmailType( Guid.NewGuid().ToString(), "COL", "College", EmailTypeCategory.School),
                        new Domain.Base.Entities.EmailType( Guid.NewGuid().ToString(), "PER", "Personal", EmailTypeCategory.Personal),
                        new Domain.Base.Entities.EmailType( Guid.NewGuid().ToString(), "BUS", "Business", EmailTypeCategory.Business)
                       }
                    );
                _referenceDataRepositoryMock.Setup(repo => repo.GetEmailTypesAsync(It.IsAny<bool>()))
                    .ReturnsAsync(emailTypes);

                ids = _institutionsCollection.Where(x => (!string.IsNullOrEmpty(x.Id)))
                   .Select(x => x.Id).Distinct().ToList();

                idCollection = new Dictionary<string, string>();
                ids.ForEach(x => idCollection.Add(x, Guid.NewGuid().ToString()));

                var defaultsConfiguration = new DefaultsConfiguration()
                {
                    HostInstitutionCodeId = "0000043"
                };
                _configurationRepoMock.Setup(x => x.GetDefaultsConfiguration()).Returns(defaultsConfiguration);

                _personRepositoryMock.Setup(x => x.GetPersonGuidFromIdAsync(It.IsAny<string>()))
                    .ReturnsAsync(DefaultHostGuid);


                facultyRole.AddPermission(permissionViewEducationalInstitution);

                _roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { facultyRole });
                // Set up current user
                _currentUserFactory = new FacultyUserFactory();

                _referenceDataRepositoryMock.Setup(repo => repo.GetSocialMediaTypesAsync(It.IsAny<bool>())).ReturnsAsync(allSocialMediaTypes);


                _educationalInstitutionsService = new EducationalInstitutionsService(_referenceDataRepository,
                   _personRepository, _institutionRepository, _configurationRepo, _adapterRegistry, _currentUserFactory, _roleRepository, _logger);
            }

            [TestCleanup]
            public void Cleanup()
            {
                _institutionRepository = null;
                _institutionRepositoryMock = null;

                _referenceDataRepository = null;
                _educationalInstitutionsService = null;

                _logger = null;
                _personRepository = null;
                _personRepositoryMock = null;
                _referenceDataRepository = null;
                _referenceDataRepositoryMock = null;
                _configurationRepoMock = null;
                _configurationRepo = null;
            }

            #region GetEducationalInstitutionsAsync

            [TestMethod]
            public async Task EducationalInstitutionsService_GetEducationalInstitutions()
            {
                
                _personRepositoryMock.Setup(x => x.GetPersonGuidFromIdAsync(It.IsAny<string>()))
                    .ReturnsAsync(DefaultHostGuid);

                var tuple = new Tuple<IEnumerable<Institution>, int>(
                  _institutionsCollection, _institutionsCollection.Count);
                _institutionRepositoryMock.Setup(x =>
                    x.GetInstitutionAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<InstType?>()))
                    .ReturnsAsync(tuple);
                _institutionRepositoryMock.Setup(x =>
                    x.GetInstitutionAsync(It.IsAny<int>(), It.IsAny<int>(), null))
                    .ReturnsAsync(tuple);

                _personRepositoryMock.Setup(x => x.GetPersonGuidsCollectionAsync(ids)).ReturnsAsync(idCollection);

                var response = await _educationalInstitutionsService.GetEducationalInstitutionsByTypeAsync(0, 1, null, false);

                var actual = response.Item1.FirstOrDefault();
                // the idCollection dictionary consisting of a Id (key) and guid (value)
                var idPair = idCollection.FirstOrDefault(x => x.Value.Equals(actual.Id, StringComparison.OrdinalIgnoreCase));
                // records in the institutionsCollection do not contain a guid, so we need the record key
                var expected = _institutionsCollection.FirstOrDefault(x => x.Id == idPair.Key);

                Assert.AreEqual(idPair.Value, actual.Id, "Id");
                Assert.AreEqual(expected.Name, actual.Title, "Title");
                Assert.AreEqual(HomeInstitutionType.External, actual.HomeInstitution, "HomeInstitution");
                Assert.AreEqual(EducationalInstitutionType.SecondarySchool, actual.Type, "Type");

                Assert.IsNotNull(actual.Addresses);
                var address = actual.Addresses.FirstOrDefault();
                Assert.AreEqual(addressType.Guid, address.Type.Detail.Id, "Address guid");

                Assert.IsNotNull(actual.EmailAddresses);
                var emailAddress = actual.EmailAddresses.FirstOrDefault();
                var expectedEmailType = emailTypes.FirstOrDefault(x => x.Code == expected.EmailAddresses[0].TypeCode);
                Assert.AreEqual(expectedEmailType.Guid, emailAddress.Type.Detail.Id, "Email guid");

                Assert.IsNotNull(actual.Phones);
                var phone = actual.Phones.FirstOrDefault();
                Assert.AreEqual(phoneType.Guid, phone.Type.Detail.Id, "Phone guid");

                Assert.IsNotNull(actual.SocialMedia);
                var actualSocialMedia = actual.SocialMedia.FirstOrDefault();
                Assert.AreEqual(socialMediaType.Guid, actualSocialMedia.Type.Detail.Id, "Phone guid");

            }


            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task EducationalInstitutionsService_GetEducationalInstitutions_InvalidPerson()
            {

                _personRepositoryMock.Setup(x => x.GetPersonGuidFromIdAsync(It.IsAny<string>()))
                    .ThrowsAsync(new KeyNotFoundException());

                var tuple = new Tuple<IEnumerable<Institution>, int>(
                  null, _institutionsCollection.Count);
                _institutionRepositoryMock.Setup(x =>
                    x.GetInstitutionAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<InstType?>()))
                    .ReturnsAsync(tuple);
                _institutionRepositoryMock.Setup(x =>
                    x.GetInstitutionAsync(It.IsAny<int>(), It.IsAny<int>(), null))
                    .ReturnsAsync(tuple);

                _personRepositoryMock.Setup(x => x.GetPersonGuidsCollectionAsync(ids)).ReturnsAsync(idCollection);

                await _educationalInstitutionsService.GetEducationalInstitutionsByTypeAsync(0, 1, null, false);
            }


            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task EducationalInstitutionsService_GetEducationalInstitutions_InvalidInstitutions()
            {

                _personRepositoryMock.Setup(x => x.GetPersonGuidFromIdAsync(It.IsAny<string>()))
                   .ReturnsAsync(DefaultHostGuid);

                
                _institutionRepositoryMock.Setup(x =>
                    x.GetInstitutionAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<InstType?>()))
                    .ThrowsAsync(new KeyNotFoundException());
                _institutionRepositoryMock.Setup(x =>
                    x.GetInstitutionAsync(It.IsAny<int>(), It.IsAny<int>(), null))
                    .ThrowsAsync(new KeyNotFoundException());

                _personRepositoryMock.Setup(x => x.GetPersonGuidsCollectionAsync(ids)).ReturnsAsync(idCollection);

                await _educationalInstitutionsService.GetEducationalInstitutionsByTypeAsync(0, 1, null, false);
            }

            [TestMethod]
            [ExpectedException(typeof(Exception))]
            public async Task EducationalInstitutionsService_GetEducationalInstitutions_EmptyGuidCollection()
            {

                _personRepositoryMock.Setup(x => x.GetPersonGuidFromIdAsync(It.IsAny<string>()))
                    .ReturnsAsync(DefaultHostGuid);

                var tuple = new Tuple<IEnumerable<Institution>, int>(
                  _institutionsCollection, _institutionsCollection.Count);
                _institutionRepositoryMock.Setup(x =>
                    x.GetInstitutionAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<InstType?>()))
                    .ReturnsAsync(tuple);
                _institutionRepositoryMock.Setup(x =>
                    x.GetInstitutionAsync(It.IsAny<int>(), It.IsAny<int>(), null))
                    .ReturnsAsync(tuple);

                _personRepositoryMock.Setup(x => x.GetPersonGuidsCollectionAsync(ids)).ReturnsAsync(null);

                await _educationalInstitutionsService.GetEducationalInstitutionsByTypeAsync(0, 1, null, false);
            }

            #endregion GetEducationalInstitutionsAsync

                #region GetEducationalInstitutionsByGuid

            [TestMethod]
            [ExpectedException(typeof(Exception))]
            public async Task EducationalInstitutionsService_GetEducationalInstitutionsByGuid_ArgumentNullException()
            {
                await _educationalInstitutionsService.GetEducationalInstitutionByGuidAsync("");
            }

            [TestMethod]
            [ExpectedException(typeof(Exception))]
            public async Task EducationalInstitutionsService_GetEducationalInstitutionsByGuid_InvalidID()
            {
                await _educationalInstitutionsService.GetEducationalInstitutionByGuidAsync("invalid");
            }

            [TestMethod]
            public async Task EducationalInstitutionsService_GetEducationalInstitutionsByGuid()
            {
                var defaultsConfiguration = new DefaultsConfiguration()
                {
                    HostInstitutionCodeId = "0000043"
                };
                _configurationRepoMock.Setup(x => x.GetDefaultsConfiguration()).Returns(defaultsConfiguration);

                _personRepositoryMock.Setup(x => x.GetPersonGuidFromIdAsync(It.IsAny<string>()))
                    .ReturnsAsync(DefaultHostGuid);

                var tuple = new Tuple<IEnumerable<Institution>, int>(
                  _institutionsCollection, _institutionsCollection.Count);
                _institutionRepositoryMock.Setup(x =>
                    x.GetInstitutionAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<InstType?>()))
                    .ReturnsAsync(tuple);
                _institutionRepositoryMock.Setup(x =>
                    x.GetInstitutionAsync(It.IsAny<int>(), It.IsAny<int>(), null))
                    .ReturnsAsync(tuple);

                _personRepositoryMock.Setup(x => x.GetPersonGuidsCollectionAsync(ids)).ReturnsAsync(idCollection);

                var expected = _institutionsCollection.FirstOrDefault();

                _institutionRepositoryMock.Setup(x => x.GetInstitutionByGuidAsync(It.IsAny<string>())).ReturnsAsync(expected); 

                var idPair = idCollection.FirstOrDefault(x => x.Key.Equals(expected.Id, StringComparison.OrdinalIgnoreCase));

                var actual = await _educationalInstitutionsService.GetEducationalInstitutionByGuidAsync(idPair.Value); 
               
                Assert.AreEqual(idPair.Value, actual.Id, "Id");
                Assert.AreEqual(expected.Name, actual.Title, "Title");
                Assert.AreEqual(HomeInstitutionType.External, actual.HomeInstitution, "HomeInstitution");
                Assert.AreEqual(EducationalInstitutionType.SecondarySchool, actual.Type, "Type");

                Assert.IsNotNull(actual.Addresses);
                var address = actual.Addresses.FirstOrDefault();
                Assert.AreEqual(addressType.Guid, address.Type.Detail.Id, "Address guid");

                Assert.IsNotNull(actual.EmailAddresses);
                var emailAddress = actual.EmailAddresses.FirstOrDefault();
                var expectedEmailType = emailTypes.FirstOrDefault(x => x.Code == expected.EmailAddresses[0].TypeCode);
                Assert.AreEqual(expectedEmailType.Guid, emailAddress.Type.Detail.Id, "Email guid");

                Assert.IsNotNull(actual.Phones);
                var phone = actual.Phones.FirstOrDefault();
                Assert.AreEqual(phoneType.Guid, phone.Type.Detail.Id, "Phone guid");

                Assert.IsNotNull(actual.SocialMedia);
                var actualSocialMedia = actual.SocialMedia.FirstOrDefault();
                Assert.AreEqual(socialMediaType.Guid, actualSocialMedia.Type.Detail.Id, "Phone guid");
            }

            #endregion GetEducationalInstitutionsById
        }
    }
}