/*Copyright 2020 Ellucian Company L.P. and its affiliates.*/

using Ellucian.Colleague.Data.Base.DataContracts;
using Ellucian.Colleague.Data.Base.Tests.Repositories;
using Ellucian.Colleague.Data.ColleagueFinance.DataContracts;
using Ellucian.Colleague.Data.ColleagueFinance.Repositories;
using Ellucian.Colleague.Domain.Base.Transactions;
using Ellucian.Colleague.Domain.ColleagueFinance.Entities;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Data.Colleague;
using Ellucian.Web.Http.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Data.ColleagueFinance.Tests.Repositories
{
    [TestClass]
    public class VendorContactsRepositoryTests : BaseRepositorySetup
    {
        private Mock<ILogger> _iLoggerMock;

        private VendorContactsRepository vendorContactRepository;
        System.Collections.ObjectModel.Collection<Ellucian.Colleague.Data.ColleagueFinance.DataContracts.OrganizationContact> vendorContacts;
        System.Collections.ObjectModel.Collection<Ellucian.Colleague.Data.Base.DataContracts.Person> people;
        System.Collections.ObjectModel.Collection<Ellucian.Colleague.Data.Base.DataContracts.Relationship> relationships;
        System.Collections.ObjectModel.Collection<Ellucian.Colleague.Data.Base.DataContracts.Address> addresses;
        private Ellucian.Colleague.Domain.ColleagueFinance.Entities.OrganizationContactInitiationProcess orgCtInitProc;

        string[] ids = new string[] { "person1", "person2", "person3", "person4" };
        string[] relids = new string[] { "rel1", "rel2", "rel3", "rel4" };
        string expectedRecordKey = "1";
        string guid = "4f937f08-f6a0-4a1c-8d55-9f2a6dd6be46";
        int offset = 0;
        int limit = 4;

        [TestInitialize]
        public void Initialize()
        {
            MockInitialize();

            _iLoggerMock = new Mock<ILogger>();
            apiSettings = new ApiSettings("TEST");

            BuildData();

            vendorContactRepository = new VendorContactsRepository(cacheProviderMock.Object, transFactoryMock.Object, _iLoggerMock.Object, apiSettings);
        }

        [TestCleanup]
        public void Cleanup()
        {
            apiSettings = null;
            vendorContactRepository = null;
            transManager = null;
        }

        [TestMethod]
        public async Task VendorContactsRepository_GetVendorContacts2Async()
        {
            var actuals = await vendorContactRepository.GetVendorContactsAsync(offset, limit, It.IsAny<string>());
            Assert.IsNotNull(actuals);
            Assert.AreEqual(actuals.Item2, 4);
            Assert.AreEqual(actuals.Item1.Count(), 4);

        }

        [TestMethod]
        public async Task VendorContactsRepository_GetVendorContacts2Async_noRecord()
        {
            transManagerMock.Setup(mgr => mgr.ExecuteAsync<GetCacheApiKeysRequest, GetCacheApiKeysResponse>(It.IsAny<GetCacheApiKeysRequest>()))
                .ReturnsAsync(() => null);
            var actuals = await vendorContactRepository.GetVendorContactsAsync(offset, limit, It.IsAny<string>());
            Assert.IsNotNull(actuals);
            Assert.AreEqual(actuals.Item2, 0);

        }

        [TestMethod]
        public async Task VendorContactsRepository_GetVendorContacts2Async_noOrgContactRecord()
        {
            dataReaderMock.Setup(repo => repo.BulkReadRecordAsync<Ellucian.Colleague.Data.ColleagueFinance.DataContracts.OrganizationContact>("ORGANIZATION.CONTACT", ids, true))
                .ReturnsAsync(() => null);
            var actuals = await vendorContactRepository.GetVendorContactsAsync(offset, limit, It.IsAny<string>());
            Assert.IsNotNull(actuals);
            Assert.AreEqual(actuals.Item2, 0);

        }

        [TestMethod]
        public async Task VendorContactsRepository_GetVendorContacts2Async_Filter()
        {
            var actuals = await vendorContactRepository.GetVendorContactsAsync(offset, limit, "vendor1");
            Assert.IsNotNull(actuals);
        }

        [TestMethod]
        [ExpectedException(typeof(RepositoryException))]
        public async Task VendorContactsRepository_GetVendorContacts2Async_NoPersonRecord()
        {
            dataReaderMock.Setup(repo => repo.BulkReadRecordAsync<Ellucian.Colleague.Data.Base.DataContracts.Person>(ids, It.IsAny<bool>()))
                .ReturnsAsync(() => null);
            var actuals = await vendorContactRepository.GetVendorContactsAsync(offset, limit, It.IsAny<string>());
        }

        [TestMethod]
        [ExpectedException(typeof(RepositoryException))]
        public async Task VendorContactsRepository_GetVendorContacts2Async_InvalidPersonRecord()
        {
            var newpeople = new Collection<Base.DataContracts.Person>()
            {
                new Base.DataContracts.Person(){RecordGuid = "4f937f08-f6a0-4a1c-8d55-9f2a6dd6be46", Recordkey = "person1", PersonCorpIndicator = "Y", PreferredName = "John Adams",PerphoneEntityAssociation = new List<PersonPerphone>{ new PersonPerphone { PersonalPhoneExtensionAssocMember = "ext1", PersonalPhoneNumberAssocMember = "phone1", PersonalPhoneTypeAssocMember = "CP" } } },
                new Base.DataContracts.Person(){RecordGuid = "be0c904d-d3d5-4085-9f0a-a76a34c21bff", Recordkey = "person2", PersonCorpIndicator = "Y", LastName = "Adams", FirstName = "John", MiddleName = "M", Suffix = "Jr.", PseasonEntityAssociation = new List<PersonPseason>{ new PersonPseason { AddrLocalPhoneAssocMember = "phone2", AddrLocalPhoneTypeAssocMember = "HO", AddrLocalExtAssocMember = "123", PersonAddressesAssocMember = "addr2" } } },
                new Base.DataContracts.Person(){RecordGuid = "41a341d6-ebc0-4ac7-a77f-262e5e7dfd62", Recordkey = "person3", PersonCorpIndicator = "N", LastName = "Adams"}
            };
            dataReaderMock.Setup(repo => repo.BulkReadRecordAsync<Ellucian.Colleague.Data.Base.DataContracts.Person>(ids, It.IsAny<bool>()))
             .ReturnsAsync(newpeople);
            var actuals = await vendorContactRepository.GetVendorContactsAsync(offset, limit, It.IsAny<string>());
        }

        [TestMethod]
        [ExpectedException(typeof(RepositoryException))]
        public async Task VendorContactsRepository_GetVendorContacts2Async_InvalidRelationshipRecord()
        {
            var newrelationships = new Collection<Base.DataContracts.Relationship>()
            {
                new Base.DataContracts.Relationship(){RecordGuid = "4f937f08-f6a0-4a1c-8d55-9f2a6dd6be46", Recordkey = "rel1", RsRelationType = "contact"},
                new Base.DataContracts.Relationship(){RecordGuid = "4f937f08-f6a0-4a1c-8d55-9f2a6dd6be45", Recordkey = "rel2", RsRelationType = "contact"}
            };
            dataReaderMock.Setup(repo => repo.BulkReadRecordAsync<Ellucian.Colleague.Data.Base.DataContracts.Relationship>(relids, It.IsAny<bool>()))
                .ReturnsAsync(newrelationships);
            var actuals = await vendorContactRepository.GetVendorContactsAsync(offset, limit, It.IsAny<string>());
        }

        [TestMethod]
        [ExpectedException(typeof(RepositoryException))]
        public async Task VendorContactsRepository_GetVendorContacts2Async_InvalidAddressRecord()
        {
            var newaddresses = new Collection<Base.DataContracts.Address>()
            {
                new Base.DataContracts.Address(){RecordGuid = "4f937f08-f6a0-4a1c-8d55-9f2a6dd6be46", Recordkey = "addr2", AdrPhonesEntityAssociation = new List<AddressAdrPhones>{ new AddressAdrPhones { AddressPhoneExtensionAssocMember = "123", AddressPhonesAssocMember = "1111111111", AddressPhoneTypeAssocMember = "OR" } } }

            };
            string[] addrIds = new string[] { "addr1", "addr2" };
            dataReaderMock.Setup(repo => repo.BulkReadRecordAsync<Ellucian.Colleague.Data.Base.DataContracts.Address>(addrIds, It.IsAny<bool>()))
                .ReturnsAsync(newaddresses);
            var actuals = await vendorContactRepository.GetVendorContactsAsync(offset, limit, It.IsAny<string>());
        }

        [TestMethod]
        [ExpectedException(typeof(RepositoryException))]
        public async Task VendorContactsRepository_GetVendorContacts2Async_NoRelationshipRecord()
        {
            dataReaderMock.Setup(repo => repo.BulkReadRecordAsync<Ellucian.Colleague.Data.Base.DataContracts.Relationship>(relids, It.IsAny<bool>()))
                .ReturnsAsync(() => null);
            var actuals = await vendorContactRepository.GetVendorContactsAsync(offset, limit, It.IsAny<string>());
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task VendorContactsRepository_GetGetVendorContactsByGuidAsync_ArgumentNullException()
        {
            var actual = await vendorContactRepository.GetGetVendorContactsByGuidAsync(null);
        }


        [TestMethod]
        [ExpectedException(typeof( RepositoryException ) )]
        public async Task VendorContactsRepository_GetGetVendorContactsByGuidAsync_DictionaryNull_KeyNotFoundException()
        {
            GuidLookupResult result = new GuidLookupResult() { Entity = "ORGANIZATION.CONTACT", PrimaryKey = "1", SecondaryKey = "" };
            Dictionary<string, GuidLookupResult> resultDict = null;
            dataReaderMock.Setup(repo => repo.SelectAsync(It.IsAny<GuidLookup[]>())).ReturnsAsync(resultDict);
            var actual = await vendorContactRepository.GetGetVendorContactsByGuidAsync(guid);
        }

        [TestMethod]
        [ExpectedException(typeof( RepositoryException ) )]
        public async Task VendorContactsRepository_GetGetVendorContactsByGuidAsync_InvalidEntity_KeyNotFoundException()
        {
            GuidLookupResult result = new GuidLookupResult() { Entity = "PERSON", PrimaryKey = "1", SecondaryKey = "" };
            Dictionary<string, GuidLookupResult> resultDict = new Dictionary<string, GuidLookupResult>();
            resultDict.Add(guid, result);
            dataReaderMock.Setup(repo => repo.SelectAsync(It.IsAny<GuidLookup[]>())).ReturnsAsync(resultDict);
            var actual = await vendorContactRepository.GetGetVendorContactsByGuidAsync(guid);
        }

        [TestMethod]
        [ExpectedException(typeof( RepositoryException ) )]
        public async Task VendorContactsRepository_GetGetVendorContactsByGuidAsync_InvalidValue_KeyNotFoundException()
        {
            GuidLookupResult result = new GuidLookupResult() { Entity = "ORGANIZATION.CONTACT", PrimaryKey = "", SecondaryKey = "" };
            Dictionary<string, GuidLookupResult> resultDict = new Dictionary<string, GuidLookupResult>();
            resultDict.Add(guid, result);
            dataReaderMock.Setup(repo => repo.SelectAsync(It.IsAny<GuidLookup[]>())).ReturnsAsync(resultDict);
            var actual = await vendorContactRepository.GetGetVendorContactsByGuidAsync(guid);
        }

        [TestMethod]
        public async Task VendorContactsRepository_GetGetVendorContactsByGuidAsync()
        {
            var expectedContact = vendorContacts.FirstOrDefault(i => i.RecordGuid.Equals(guid));
            var expectedPerson = people.FirstOrDefault(i => i.Recordkey.Equals(expectedContact.OcnPersonId));
            var expectedAddr = addresses.FirstOrDefault(i => i.Recordkey.Equals(expectedContact.OcnAddress));
            var expectedRelationship = relationships.FirstOrDefault(i => i.Recordkey == expectedContact.OcnRelationship);
            dataReaderMock.Setup(repo => repo.ReadRecordAsync<Ellucian.Colleague.Data.ColleagueFinance.DataContracts.OrganizationContact>(expectedRecordKey, true)).ReturnsAsync(expectedContact);
            dataReaderMock.Setup(repo => repo.ReadRecordAsync<Ellucian.Colleague.Data.Base.DataContracts.Person>(expectedPerson.Recordkey, true)).ReturnsAsync(expectedPerson);
            dataReaderMock.Setup(repo => repo.ReadRecordAsync<Ellucian.Colleague.Data.Base.DataContracts.Relationship>(expectedRelationship.Recordkey, true)).ReturnsAsync(expectedRelationship);
            dataReaderMock.Setup(repo => repo.ReadRecordAsync<Ellucian.Colleague.Data.Base.DataContracts.Address>(expectedAddr.Recordkey, true)).ReturnsAsync(expectedAddr);
            dataReaderMock.Setup( repo => repo.ReadRecordAsync<DataContracts.Vendors>( It.IsAny<string>(), It.IsAny<bool>() ) ).ReturnsAsync( new DataContracts.Vendors() );
            var actual = await vendorContactRepository.GetGetVendorContactsByGuidAsync(guid);
            Assert.IsNotNull(actual);
        }

        [TestMethod]
        [ExpectedException( typeof( RepositoryException ) )]
        public async Task VendorContactsRepository_GetGetVendorContactsByGuidAsync_RepositoryException()
        {
            var expectedContact = vendorContacts.FirstOrDefault( i => i.RecordGuid.Equals( guid ) );
            var expectedPerson = people.FirstOrDefault( i => i.Recordkey.Equals( expectedContact.OcnPersonId ) );
            var expectedAddr = addresses.FirstOrDefault( i => i.Recordkey.Equals( expectedContact.OcnAddress ) );
            var expectedRelationship = relationships.FirstOrDefault( i => i.Recordkey == expectedContact.OcnRelationship );
            dataReaderMock.Setup( repo => repo.ReadRecordAsync<Ellucian.Colleague.Data.ColleagueFinance.DataContracts.OrganizationContact>( expectedRecordKey, true ) ).ReturnsAsync( expectedContact );
            dataReaderMock.Setup( repo => repo.ReadRecordAsync<Ellucian.Colleague.Data.Base.DataContracts.Person>( expectedPerson.Recordkey, true ) ).ReturnsAsync( expectedPerson );
            dataReaderMock.Setup( repo => repo.ReadRecordAsync<Ellucian.Colleague.Data.Base.DataContracts.Relationship>( expectedRelationship.Recordkey, true ) ).ReturnsAsync( expectedRelationship );
            dataReaderMock.Setup( repo => repo.ReadRecordAsync<Ellucian.Colleague.Data.Base.DataContracts.Address>( expectedAddr.Recordkey, true ) ).ReturnsAsync( expectedAddr );
            dataReaderMock.Setup( repo => repo.ReadRecordAsync<DataContracts.Vendors>( It.IsAny<string>(), It.IsAny<bool>() ) ).ReturnsAsync(() => null);
            var actual = await vendorContactRepository.GetGetVendorContactsByGuidAsync( guid );
            Assert.IsNotNull( actual );
        }

        [TestMethod]
        [ExpectedException(typeof(RepositoryException))]
        public async Task VendorContactsRepository_GetGetVendorContactsByGuidAsync_AddressNull()
        {
            var expectedContact = vendorContacts.FirstOrDefault(i => i.RecordGuid.Equals(guid));
            var expectedPerson = people.FirstOrDefault(i => i.Recordkey.Equals(expectedContact.OcnPersonId));
            var expectedAddr = addresses.FirstOrDefault(i => i.Recordkey.Equals(expectedContact.OcnAddress));
            var expectedRelationship = relationships.FirstOrDefault(i => i.Recordkey == expectedContact.OcnRelationship);
            dataReaderMock.Setup(repo => repo.ReadRecordAsync<Ellucian.Colleague.Data.ColleagueFinance.DataContracts.OrganizationContact>(expectedRecordKey, true)).ReturnsAsync(expectedContact);
            dataReaderMock.Setup(repo => repo.ReadRecordAsync<Ellucian.Colleague.Data.Base.DataContracts.Person>(expectedPerson.Recordkey, true)).ReturnsAsync(expectedPerson);
            dataReaderMock.Setup(repo => repo.ReadRecordAsync<Ellucian.Colleague.Data.Base.DataContracts.Relationship>(expectedRelationship.Recordkey, true)).ReturnsAsync(expectedRelationship);
            dataReaderMock.Setup(repo => repo.ReadRecordAsync<Ellucian.Colleague.Data.Base.DataContracts.Address>(expectedAddr.Recordkey, true)).ReturnsAsync(() => null);
            var actual = await vendorContactRepository.GetGetVendorContactsByGuidAsync(guid);
            Assert.IsNotNull(actual);
        }

        [TestMethod]
        [ExpectedException(typeof(RepositoryException))]
        public async Task VendorContactsRepository_GetGetVendorContactsByGuidAsync_InvalidPerson_NolastName()
        {
            var expectedContact = vendorContacts.FirstOrDefault(i => i.RecordGuid.Equals(guid));
            var expectedPerson = new Base.DataContracts.Person() { RecordGuid = "41a341d6-ebc0-4ac7-a77f-262e5e7dfd62", Recordkey = "person1", PersonCorpIndicator = "N" };
            var expectedRelationship = relationships.FirstOrDefault(i => i.Recordkey == expectedContact.OcnRelationship);
            var expectedAddr = addresses.FirstOrDefault(i => i.Recordkey.Equals(expectedContact.OcnAddress));
            dataReaderMock.Setup(repo => repo.ReadRecordAsync<Ellucian.Colleague.Data.ColleagueFinance.DataContracts.OrganizationContact>(expectedRecordKey, true)).ReturnsAsync(expectedContact);
            dataReaderMock.Setup(repo => repo.ReadRecordAsync<Ellucian.Colleague.Data.Base.DataContracts.Person>(expectedPerson.Recordkey, true)).ReturnsAsync(expectedPerson);
            dataReaderMock.Setup(repo => repo.ReadRecordAsync<Ellucian.Colleague.Data.Base.DataContracts.Relationship>(expectedRelationship.Recordkey, true)).ReturnsAsync(expectedRelationship);
            dataReaderMock.Setup(repo => repo.ReadRecordAsync<Ellucian.Colleague.Data.Base.DataContracts.Address>(expectedAddr.Recordkey, true)).ReturnsAsync(expectedAddr);
            var actual = await vendorContactRepository.GetGetVendorContactsByGuidAsync(guid);
        }

        [TestMethod]
        [ExpectedException(typeof(RepositoryException))]
        public async Task VendorContactsRepository_GetGetVendorContactsByGuidAsync_OrgContactNull()
        {
            var expectedContact = vendorContacts.FirstOrDefault(i => i.RecordGuid.Equals(guid));
            var expectedPerson = people.FirstOrDefault(i => i.Recordkey.Equals(expectedContact.OcnPersonId));
            var expectedRelationship = relationships.FirstOrDefault(i => i.Recordkey == expectedContact.OcnRelationship);
            dataReaderMock.Setup(repo => repo.ReadRecordAsync<Ellucian.Colleague.Data.ColleagueFinance.DataContracts.OrganizationContact>(expectedRecordKey, true)).ReturnsAsync(() => null);
            dataReaderMock.Setup(repo => repo.ReadRecordAsync<Ellucian.Colleague.Data.Base.DataContracts.Person>(expectedPerson.Recordkey, true)).ReturnsAsync(expectedPerson);
            dataReaderMock.Setup(repo => repo.ReadRecordAsync<Ellucian.Colleague.Data.Base.DataContracts.Relationship>(expectedRelationship.Recordkey, true)).ReturnsAsync(expectedRelationship);
            var actual = await vendorContactRepository.GetGetVendorContactsByGuidAsync(guid);
        }

        [TestMethod]
        [ExpectedException(typeof(RepositoryException))]
        public async Task VendorContactsRepository_GetGetVendorContactsByGuidAsync_PersonNull()
        {
            var expectedContact = vendorContacts.FirstOrDefault(i => i.RecordGuid.Equals(guid));
            var expectedPerson = people.FirstOrDefault(i => i.Recordkey.Equals(expectedContact.OcnPersonId));
            var expectedRelationship = relationships.FirstOrDefault(i => i.Recordkey == expectedContact.OcnRelationship);
            dataReaderMock.Setup(repo => repo.ReadRecordAsync<Ellucian.Colleague.Data.ColleagueFinance.DataContracts.OrganizationContact>(expectedRecordKey, true)).ReturnsAsync(expectedContact);
            dataReaderMock.Setup(repo => repo.ReadRecordAsync<Ellucian.Colleague.Data.Base.DataContracts.Person>(expectedPerson.Recordkey, true)).ReturnsAsync(() => null);
            dataReaderMock.Setup(repo => repo.ReadRecordAsync<Ellucian.Colleague.Data.Base.DataContracts.Relationship>(expectedRelationship.Recordkey, true)).ReturnsAsync(expectedRelationship);
            var actual = await vendorContactRepository.GetGetVendorContactsByGuidAsync(guid);
        }



        [TestMethod]
        [ExpectedException(typeof(RepositoryException))]
        public async Task VendorContactsRepository_GetGetVendorContactsByGuidAsync_RelationshipNull()
        {
            var expectedContact = vendorContacts.FirstOrDefault(i => i.RecordGuid.Equals(guid));
            var expectedPerson = people.FirstOrDefault(i => i.Recordkey.Equals(expectedContact.OcnPersonId));
            var expectedRelationship = relationships.FirstOrDefault(i => i.Recordkey == expectedContact.OcnRelationship);
            dataReaderMock.Setup(repo => repo.ReadRecordAsync<Ellucian.Colleague.Data.ColleagueFinance.DataContracts.OrganizationContact>(expectedRecordKey, true)).ReturnsAsync(expectedContact);
            dataReaderMock.Setup(repo => repo.ReadRecordAsync<Ellucian.Colleague.Data.Base.DataContracts.Person>(expectedPerson.Recordkey, true)).ReturnsAsync(expectedPerson);
            dataReaderMock.Setup(repo => repo.ReadRecordAsync<Ellucian.Colleague.Data.Base.DataContracts.Relationship>(expectedRelationship.Recordkey, true)).ReturnsAsync(() => null);
            var actual = await vendorContactRepository.GetGetVendorContactsByGuidAsync(guid);
        }

        [TestMethod]
        public async Task CreateVendorContactInitiationProcessAsync_Return_OrganizationContact()
        {
            Dflts dflts = new Dflts() { DfltsOrgContactRelType = "CZ" };
            dataReaderMock.Setup( r => r.ReadRecordAsync<Dflts>( It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>() ) ).ReturnsAsync( dflts );
            ProcessVendorContactResponse response = new ProcessVendorContactResponse()
            {
                OrgContactGuid = Guid.NewGuid().ToString()            
            };
            transManagerMock.Setup( r => r.ExecuteAsync<ProcessVendorContactRequest, ProcessVendorContactResponse>( It.IsAny<ProcessVendorContactRequest>() ) ).ReturnsAsync( response );

            var actual = await vendorContactRepository.CreateVendorContactInitiationProcessAsync( orgCtInitProc );
            Assert.IsNotNull( actual );
        }

        [TestMethod]
        public async Task CreateVendorContactInitiationProcessAsync_Return_PersonMatchRequestGuid()
        {
            Dflts dflts = new Dflts() { DfltsOrgContactRelType = "CZ" };
            dataReaderMock.Setup( r => r.ReadRecordAsync<Dflts>( It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>() ) ).ReturnsAsync( dflts );
            ProcessVendorContactResponse response = new ProcessVendorContactResponse()
            {
                OrgContactGuid = string.Empty,
                PersonMatchRequestGuid = Guid.NewGuid().ToString()
            };
            transManagerMock.Setup( r => r.ExecuteAsync<ProcessVendorContactRequest, ProcessVendorContactResponse>( It.IsAny<ProcessVendorContactRequest>() ) ).ReturnsAsync( response );

            var actual = await vendorContactRepository.CreateVendorContactInitiationProcessAsync( orgCtInitProc );
            Assert.IsNotNull( actual );
            Assert.IsNotNull( actual.Item2 );
        }

        [TestMethod]
        [ExpectedException( typeof( RepositoryException ) )]
        public async Task CreateVendorContactInitiationProcessAsync_RepositoryException()
        {
            Dflts dflts = new Dflts() { DfltsOrgContactRelType = "CZ" };
            dataReaderMock.Setup( r => r.ReadRecordAsync<Dflts>( It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>() ) ).ReturnsAsync( dflts );
            ProcessVendorContactResponse response = new ProcessVendorContactResponse()
            {
               ErrorOccurred = true,
               ProcessRelationshipRequestErrors = new List<ProcessRelationshipRequestErrors>()
               {
                  new ProcessRelationshipRequestErrors()
                  {
                      ErrorCodes = "1",
                      ErrorMessages = "Error Occured."
                  }
               }
            };
            transManagerMock.Setup( r => r.ExecuteAsync<ProcessVendorContactRequest, ProcessVendorContactResponse>( It.IsAny<ProcessVendorContactRequest>() ) ).ReturnsAsync( response );

            try
            {
                var actual = await vendorContactRepository.CreateVendorContactInitiationProcessAsync( orgCtInitProc );
            }
            catch( RepositoryException  e)
            {
                Assert.AreEqual( "1 - Error Occured.", e.Errors[ 0 ].Message );
                throw;
            }
        }

        [TestMethod]
        [ExpectedException( typeof( RepositoryException ) )]
        public async Task CreateVendorContactInitiationProcessAsync_Entity_Null()
        {
            try {
                var actual = await vendorContactRepository.CreateVendorContactInitiationProcessAsync( null );
            }
            catch( RepositoryException e )
            {
                Assert.AreEqual( "Must provide a organization contact initiation process body.", e.Message );
                throw;
            }
        }

        [TestMethod]
        [ExpectedException( typeof( RepositoryException ) )]
        public async Task CreateVendorContactInitiationProcessAsync_DefaultRelType_Null()
        {
            try
            {
                dataReaderMock.Setup( r => r.ReadRecordAsync<Dflts>( "CORE.PARMS", "DEFAULTS", It.IsAny<bool>() ) ).ReturnsAsync(() => null);
                var actual = await vendorContactRepository.CreateVendorContactInitiationProcessAsync( orgCtInitProc );
            }
            catch( RepositoryException e )
            {
                Assert.AreEqual( "The default Organization Contact Relation Type is required on RELP in order to create a vendor contact.", e.Errors[0].Message );
                throw;
            }
        }

        private void BuildData()
        {
            vendorContacts = new Collection<DataContracts.OrganizationContact>()
            {
                new DataContracts.OrganizationContact()
                {
                    RecordGuid = "4f937f08-f6a0-4a1c-8d55-9f2a6dd6be46",
                    Recordkey = "1",
                    OcnPersonId = "person1",
                    OcnCorpId = "vendor1",
                    OcnStartDate = new DateTime(2015, 10, 01),
                    OcnEndDate = new DateTime(2019, 10, 01),
                    OcnRelationship = "rel1",
                    OcnAddress = "addr1",
                    OcnVendorContact = "Y"
                },
                new DataContracts.OrganizationContact()
                {
                    RecordGuid = "be0c904d-d3d5-4085-9f0a-a76a34c21bff",
                    Recordkey = "2",
                    OcnPersonId = "person2",
                    OcnCorpId = "vendor2",
                    OcnStartDate = new DateTime(2015, 10, 01),
                    OcnEndDate = new DateTime(2019, 10, 01),
                    OcnRelationship = "rel2",
                    OcnAddress = "addr2",
                    OcnVendorContact = "Y"
                },
                new DataContracts.OrganizationContact()
                {
                    RecordGuid = "41a341d6-ebc0-4ac7-a77f-262e5e7dfd62",
                    Recordkey = "3",
                    OcnPersonId = "person3",
                    OcnCorpId = "vendor3",
                    OcnStartDate = new DateTime(2015, 10, 01),
                    OcnEndDate = new DateTime(2019, 10, 01),
                    OcnRelationship = "rel3",
                    OcnVendorContact = "Y"
                },
                new DataContracts.OrganizationContact()
                {
                    RecordGuid = "4d9962e8-195b-4442-93d7-197901cfb438",
                    Recordkey = "4",
                    OcnPersonId = "person4",
                    OcnCorpId = "vendor4",
                    OcnStartDate = new DateTime(2015, 10, 01),
                    OcnEndDate = new DateTime(2019, 10, 01),
                    OcnRelationship = "rel4",            
                    OcnVendorContact = "Y"
                }
            };
            dataReaderMock.Setup(repo => repo.BulkReadRecordAsync<Ellucian.Colleague.Data.ColleagueFinance.DataContracts.OrganizationContact>("ORGANIZATION.CONTACT", ids, true))
                .ReturnsAsync(vendorContacts);
            GetCacheApiKeysResponse resp = new GetCacheApiKeysResponse()
            {
                Offset = 0,
                Limit = 1,
                CacheName = "AllVendorsFilter:",
                Entity = "ORGANIZATION.CONTACT",
                Sublist = ids.ToList(),
                TotalCount = ids.ToList().Count,
                Criteria = "WITH OCN.VENDOR.CONTACT EQ 'Y'",
                KeyCacheInfo = new List<KeyCacheInfo>()
                {
                    new KeyCacheInfo()
                    {
                        KeyCacheMax = 5905,
                        KeyCacheMin = 1,
                        KeyCachePart = "000",
                        KeyCacheSize = 5905
                    },
                    new KeyCacheInfo()
                    {
                        KeyCacheMax = 7625,
                        KeyCacheMin = 5906,
                        KeyCachePart = "001",
                        KeyCacheSize = 1720
                    }
                }
            };
            transFactoryMock.Setup(transFac => transFac.GetTransactionInvoker()).Returns( transManagerMock.Object);
            transManagerMock.Setup(mgr => mgr.ExecuteAsync<GetCacheApiKeysRequest, GetCacheApiKeysResponse>(It.IsAny<GetCacheApiKeysRequest>()))
                   .ReturnsAsync(resp);
            people = new Collection<Base.DataContracts.Person>()
            {
                new Base.DataContracts.Person(){RecordGuid = "4f937f08-f6a0-4a1c-8d55-9f2a6dd6be46", Recordkey = "person1", PersonCorpIndicator = "Y", PreferredName = "John Adams", PersonAddresses = new List<string>{"addr1" },PerphoneEntityAssociation = new List<PersonPerphone>{ new PersonPerphone { PersonalPhoneExtensionAssocMember = "ext1", PersonalPhoneNumberAssocMember = "phone1", PersonalPhoneTypeAssocMember = "CP" } } },
                new Base.DataContracts.Person(){RecordGuid = "be0c904d-d3d5-4085-9f0a-a76a34c21bff", Recordkey = "person2", PersonCorpIndicator = "Y", LastName = "Adams", FirstName = "John", MiddleName = "M", Suffix = "Jr.", PseasonEntityAssociation = new List<PersonPseason>{ new PersonPseason { AddrLocalPhoneAssocMember = "phone2", AddrLocalPhoneTypeAssocMember = "HO", AddrLocalExtAssocMember = "123", PersonAddressesAssocMember = "addr2" } } },
                new Base.DataContracts.Person(){RecordGuid = "41a341d6-ebc0-4ac7-a77f-262e5e7dfd62", Recordkey = "person3", PersonCorpIndicator = "N", LastName = "Adams"},
                new Base.DataContracts.Person(){RecordGuid = "4d9962e8-195b-4442-93d7-197901cfb438", Recordkey = "person4", PersonCorpIndicator = "N", LastName = "Adams", FirstName = "John"}
            };
            dataReaderMock.Setup(repo => repo.BulkReadRecordAsync<Ellucian.Colleague.Data.Base.DataContracts.Person>(ids, It.IsAny<bool>()))
                .ReturnsAsync(people);

            relationships = new Collection<Base.DataContracts.Relationship>()
            {
                new Base.DataContracts.Relationship(){RecordGuid = "4f937f08-f6a0-4a1c-8d55-9f2a6dd6be46", Recordkey = "rel1", RsRelationType = "contact"},
                new Base.DataContracts.Relationship(){RecordGuid = "4f937f08-f6a0-4a1c-8d55-9f2a6dd6be45", Recordkey = "rel2", RsRelationType = "contact"},
                new Base.DataContracts.Relationship(){RecordGuid = "4f937f08-f6a0-4a1c-8d55-9f2a6dd6be47", Recordkey = "rel3", RsRelationType = "contact"},
                new Base.DataContracts.Relationship(){RecordGuid = "4f937f08-f6a0-4a1c-8d55-9f2a6dd6be48", Recordkey = "rel4", RsRelationType = "contact"}

            };
            dataReaderMock.Setup(repo => repo.BulkReadRecordAsync<Ellucian.Colleague.Data.Base.DataContracts.Relationship>(relids, It.IsAny<bool>()))
                .ReturnsAsync(relationships);

            addresses = new Collection<Base.DataContracts.Address>()
            {
                new Base.DataContracts.Address(){RecordGuid = "4f937f08-f6a0-4a1c-8d55-9f2a6dd6be46", Recordkey = "addr1", AdrPhonesEntityAssociation = new List<AddressAdrPhones>{ new AddressAdrPhones { AddressPhoneExtensionAssocMember = "123", AddressPhonesAssocMember = "1111111111", AddressPhoneTypeAssocMember = "OR" } } },
                new Base.DataContracts.Address(){RecordGuid = "4f937f08-f6a0-4a1c-8d55-9f2a6dd6be46", Recordkey = "addr2" }

            };
            string[] addrIds = new string[] { "addr1", "addr2" };
            dataReaderMock.Setup(repo => repo.BulkReadRecordAsync<Ellucian.Colleague.Data.Base.DataContracts.Address>(addrIds, It.IsAny<bool>()))
                .ReturnsAsync(addresses);
            GuidLookupResult result = new GuidLookupResult() { Entity = "ORGANIZATION.CONTACT", PrimaryKey = "1", SecondaryKey = "" };
            Dictionary<string, GuidLookupResult> resultDict = new Dictionary<string, GuidLookupResult>();
            resultDict.Add(guid, result);
            dataReaderMock.Setup(repo => repo.SelectAsync(It.IsAny<GuidLookup[]>())).ReturnsAsync(resultDict);

            orgCtInitProc = new OrganizationContactInitiationProcess()
            {
                EmailInfo = new ContactEmailInfo() { EmailAddress = "abc@abc.com", EmailType = "P" },
                FirstName = "FirstName",
                LastName = "LastName",
                MiddleName = "M",
                PersonId = "1",
                PhoneInfos = new List<ContactPhoneInfo>()
                {
                    new ContactPhoneInfo()
                    {
                        PhoneExtension = "123",
                        PhoneNumber = "8005551212",
                        PhoneType = "P"
                    }
                },
                RelationshipType = "CZ",                
                VendorId = "1"
            };

        }
    }
}
