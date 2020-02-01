// Copyright 2012-2019 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Data.Base.DataContracts;
using Ellucian.Colleague.Data.Base.Repositories;
using Ellucian.Data.Colleague;
using Ellucian.Data.Colleague.DataContracts;
using Ellucian.Web.Cache;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.Caching;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Data.Base.Tests.Repositories
{
    [TestClass]
    public class PhoneNumberRepositoryTests
    {
        protected List<string> personIds;
        protected List<string> addressIds;
        protected Dictionary<string, Person> personRecords;
        protected Dictionary<string, Address> addressRecords;

        protected int PersonCount = 0;
        protected Collection<Person> personResponseData;
        protected Collection<Address> addressResponseData;
        protected PhoneNumberRepository phoneNumberRepo;

        #region Private data array setup

        private string[,] _personData = {
                                       {"0000304", "Washington", "George", "", "Gen.", "101", "101;102;103", ""},
                                       {"0000404", "Adams", "John", "Peter", "Mr.", "104", "104;105", ""},
                                       {"0000504", "Jefferson", "Thomas", "", "Dr.", "106", "106", ""},
                                       
                                       {"9999999", "Test", null, null, null, null, null, null},
                                       {"9999998", "Test", "Blank", "", "", "107", "107", ""}
                                   };

        private string[,] _addressData = {
                                       {"0000304", "101 ", "65498 Ft. Belvoir Hwy;Mount Vernon;Alexandria, VA 21348", "65498 Ft. Belvoir Hwy;Mount Vernon", "Alexandria", "VA", "21348", "USA", "United States of America", "Father of our Country"},
                                       {"0000304", "102 ", "", "235 Beacon Hill Dr.", "Boston", "MA", "03549", "", "", ""},
                                       {"0000304", "103 ", "1 Champs d'Elyssie;U.S. Embassy;Paris;FRANCE", "1 Champs d'Elyssie", "Paris", "", "", "FR", "France", "Ambassador to France"},
                                       {"0000404", "104", "", "1812 Dolly Madison Dr.", "Arlington", "VA", "22146", "", "", ""},
                                       {"0000404", "105", "", "1787 Constitution Ave.", "Franklin", "TN", "34567", "", "", ""},
                                       {"0000504", "106", "", "1600 Pennsylvania Ave.;The White House", "Washington", "DC", "12345", "", "", "POTUS"},
                                       {"0000504", "107", "", "7413 Clifton Quarry Dr.", "Clifton", "VA", "20121", "", "", ""},
                                       {"9999999", "108 ", null, null, null, null, null, null, null, null},
                                       {"9999998", "109 ", "", "", "", "", "", "", "", ""}
                                   };

        private string[,] _phoneNumberData = {
                                       {"304-577-9924", "HO",""},
                                       {"703-968-9000", "BU", "4649"},
                                       {"703-803-3965", "FX", ""},
                                       {"", "", ""},
                                       {null, null, null}
                                   };

        #endregion

        [TestInitialize]
        public void Initialize()
        {
            personRecords = SetupPersons(out personIds);
            addressRecords = SetupAddresses(out addressIds);
            phoneNumberRepo = BuildValidRepository();
        }

        [TestCleanup]
        public void Cleanup()
        {
            phoneNumberRepo = null;
        }

        [TestMethod]
        public void CheckSingleFirstPhoneProperties_Valid()
        {
            var personId = personIds.ElementAt(0);
            Ellucian.Colleague.Domain.Base.Entities.PhoneNumber phoneNumber = phoneNumberRepo.GetPersonPhones(personId);
            Assert.AreEqual("304-577-9924", phoneNumber.PhoneNumbers.ElementAt(0).Number);
            Assert.AreEqual("HO", phoneNumber.PhoneNumbers.ElementAt(0).TypeCode);
            Assert.AreEqual("", phoneNumber.PhoneNumbers.ElementAt(0).Extension);
        }

        [TestMethod]
        public void CheckSingleSecondPhoneProperties_Valid()
        {
            var personId = personIds.ElementAt(0);
            Ellucian.Colleague.Domain.Base.Entities.PhoneNumber phoneNumber = phoneNumberRepo.GetPersonPhones(personId);
            Assert.AreEqual("703-968-9000", phoneNumber.PhoneNumbers.ElementAt(1).Number);
            Assert.AreEqual("BU", phoneNumber.PhoneNumbers.ElementAt(1).TypeCode);
            Assert.AreEqual("4649", phoneNumber.PhoneNumbers.ElementAt(1).Extension);
        }

        [TestMethod]
        public void SinglePersonPhoneNumberCount_Valid()
        {
            var personId = personIds.ElementAt(0);
            Ellucian.Colleague.Domain.Base.Entities.PhoneNumber phoneNumber = phoneNumberRepo.GetPersonPhones(personId);
            Assert.AreEqual(3, phoneNumber.PhoneNumbers.Count());
        }

        [TestMethod]
        public async Task MultiPersonPhoneCount_Valid()
        {
            var ids = new List<string>();
            ids.Add(personIds.ElementAt(0));
            ids.Add(personIds.ElementAt(1));
            ids.Add(personIds.ElementAt(2));
            IEnumerable<Ellucian.Colleague.Domain.Base.Entities.PhoneNumber> phoneNumbers = await phoneNumberRepo.GetPersonPhonesByIdsAsync(ids);
            Assert.AreEqual(5, phoneNumbers.Count());
            Assert.AreEqual(3, phoneNumbers.ElementAt(0).PhoneNumbers.Count());
            Assert.AreEqual(3, phoneNumbers.ElementAt(1).PhoneNumbers.Count());
            Assert.AreEqual(3, phoneNumbers.ElementAt(2).PhoneNumbers.Count());
            Assert.AreEqual(3, phoneNumbers.ElementAt(3).PhoneNumbers.Count());
            Assert.AreEqual(3, phoneNumbers.ElementAt(4).PhoneNumbers.Count());
        }

        [TestMethod]
        public async Task MultiPilotPersonPhoneCount_Valid()
        {
            var ids = new List<string>();
            ids.Add(personIds.ElementAt(0));
            ids.Add(personIds.ElementAt(1));
            ids.Add(personIds.ElementAt(2));

            Ellucian.Colleague.Domain.Base.Entities.PilotConfiguration pilotConfiguration = new Ellucian.Colleague.Domain.Base.Entities.PilotConfiguration();
            List<string> primaryPhoneTypes = new List<string>();
            primaryPhoneTypes.Add("HO");
            List<string> smsPhoneTypes = new List<string>();
            smsPhoneTypes.Add("FX");
            pilotConfiguration.PrimaryPhoneTypes = primaryPhoneTypes;

            pilotConfiguration.SmsPhoneTypes = smsPhoneTypes;
            var dataAccessorMock = new Mock<IColleagueDataReader>();
            IEnumerable<Ellucian.Colleague.Domain.Base.Entities.PilotPhoneNumber> phoneNumbers = await phoneNumberRepo.GetPilotPersonPhonesByIdsAsync(ids, pilotConfiguration);
            Assert.AreEqual(5, phoneNumbers.Count());
            Assert.AreEqual("304-577-9924", phoneNumbers.ElementAt(0).PrimaryPhoneNumber);
            Assert.AreEqual("703-803-3965", phoneNumbers.ElementAt(0).SmsPhoneNumber);
            Assert.AreEqual("304-577-9924", phoneNumbers.ElementAt(4).PrimaryPhoneNumber);
            Assert.AreEqual("703-803-3965", phoneNumbers.ElementAt(4).SmsPhoneNumber);
        }

        private PhoneNumberRepository BuildValidRepository()
        {
            var transFactoryMock = new Mock<IColleagueTransactionFactory>();

            var loggerMock = new Mock<ILogger>();

            // Cache mocking
            var cacheProviderMock = new Mock<ICacheProvider>();
            var localCacheMock = new Mock<ObjectCache>();
            //cacheProviderMock.Setup(provider => provider.GetCache(It.IsAny<string>())).Returns(localCacheMock.Object);

            // Set up data accessor for mocking 
            var dataAccessorMock = new Mock<IColleagueDataReader>();
            transFactoryMock.Setup(transFac => transFac.GetDataReader()).Returns(dataAccessorMock.Object);

            // Mock the call for getting multiple person records
            personResponseData = BuildPersonResponseData(personRecords);
            dataAccessorMock.Setup<ICollection<Person>>(acc => acc.BulkReadRecord<Person>("PERSON", It.IsAny<string[]>(), true)).Returns(personResponseData);

            // Mock for async bulk read of Person
            dataAccessorMock.Setup<Task<Collection<DataContracts.Person>>>(
               accessor => accessor.BulkReadRecordAsync<DataContracts.Person>("PERSON", It.IsAny<string[]>(), It.IsAny <bool>()))
               .Returns<string, string[], bool>((file, ids, b) =>
                   {
                       return Task.FromResult(personResponseData);
                   });
            
            // Mock the call for a single person record
            var personId = personIds.ElementAt(0);
            var personResponse = personResponseData.ElementAt(0);
            dataAccessorMock.Setup<Person>(acc => acc.ReadRecord<Person>("PERSON", personId, true)).Returns(personResponse);
            dataAccessorMock.Setup(acc => acc.ReadRecordAsync<Person>("PERSON", personId, true)).ReturnsAsync(personResponse);

            // Set up Address Response
            addressResponseData = BuildAddressResponse(addressRecords);
            ICollection<Address> addressResponse = new Collection<Address>();
            for (int i = 0; i < 7; i++)
            {
                addressResponse.Add(addressResponseData.ElementAt(i));
            }
            ICollection<Address> person1AddressResponse = new Collection<Address>();
            person1AddressResponse.Add(addressResponse.ElementAt(0));
            person1AddressResponse.Add(addressResponse.ElementAt(1));
            person1AddressResponse.Add(addressResponse.ElementAt(2));
            var personAddressIds = personResponseData.ElementAt(0).PersonAddresses.ToArray();
            var allPersonAddressIds = personResponseData.SelectMany(p => p.PersonAddresses).Distinct().ToArray();
            
            // Set up address response
            List<string> addressIds = new List<string>();
            for (int i = 0; i < 7; i++)
            {
                addressIds.Add(this.addressIds.ElementAt(i));
            }
            dataAccessorMock.Setup<ICollection<Address>>(acc => acc.BulkReadRecord<Address>("ADDRESS", addressIds.ToArray(), true)).Returns(addressResponse);
            dataAccessorMock.Setup<ICollection<Address>>(acc => acc.BulkReadRecord<Address>("ADDRESS", allPersonAddressIds, true)).Returns(addressResponse);
            dataAccessorMock.Setup<ICollection<Address>>(acc => acc.BulkReadRecord<Address>("ADDRESS", personAddressIds, true)).Returns(person1AddressResponse);

            // Mock for async bulk read of Address
            dataAccessorMock.Setup<Task<Collection<DataContracts.Address>>>(
                accessor => accessor.BulkReadRecordAsync<DataContracts.Address>("ADDRESS", It.IsAny<string[]>(), It.IsAny<bool>()))
                .Returns<string, string[], bool>((file, ids, b) =>
                    {
                        return Task.FromResult(addressResponseData);
                    });

            // Set up single address response
            var addressId = allPersonAddressIds.ElementAt(0);
            dataAccessorMock.Setup<Address>(acc => acc.ReadRecord<Address>("ADDRESS", addressId, true)).Returns(addressResponse.ElementAt(0));
            dataAccessorMock.Setup(acc => acc.ReadRecordAsync<Address>("ADDRESS", addressId, true)).ReturnsAsync(addressResponse.ElementAt(0));

            // Construct address repository
            phoneNumberRepo = new PhoneNumberRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object);
            
            return phoneNumberRepo;
        }
        private Dictionary<string, Person> SetupPersons(out List<string> ids)
        {
            ids = new List<string>();
            string[,] recordData = _personData;

            PersonCount = recordData.Length / 8;
            Dictionary<string, Person> records = new Dictionary<string, Person>();
            for (int i = 0; i < PersonCount; i++)
            {
                string id = recordData[i, 0].TrimEnd();
                string last = (recordData[i, 1] == null) ? String.Empty : recordData[i, 1].TrimEnd();
                string first = (recordData[i, 2] == null) ? null : recordData[i, 2].TrimEnd();
                string middle = (recordData[i, 3] == null) ? null : recordData[i, 3].TrimEnd();
                string preferredAddress = (recordData[i, 5] == null) ? null : recordData[i, 5].TrimEnd();
                List<string> addresses = (recordData[i, 6] == null) ? new List<string>() : recordData[i, 6].TrimEnd().Split(';').ToList<string>();
                
                DataContracts.Person record = new DataContracts.Person();
                record.Recordkey = id;
                record.LastName = last;
                record.FirstName = first;
                record.MiddleName = middle;
                record.PreferredAddress = preferredAddress;
                record.PersonAddresses = addresses;
                // Add Phone Numbers
                record.PersonalPhoneNumber = new List<string>();
                record.PersonalPhoneType = new List<string>();
                record.PersonalPhoneExtension = new List<string>();
                string[,] phoneData = _phoneNumberData;
                int phoneCount = phoneData.Length / 5;
                for (int x = 0; x < phoneCount; x++)
                {
                    string number = (phoneData[x, 0] == null) ? string.Empty : phoneData[x, 0].TrimEnd();
                    string type = (phoneData[x, 1] == null) ? string.Empty : phoneData[x, 1].TrimEnd();
                    string extension = (phoneData[x, 2] == null) ? string.Empty : phoneData[x, 2].TrimEnd();
                    record.PersonalPhoneNumber.Add(number);
                    record.PersonalPhoneType.Add(type);
                    record.PersonalPhoneExtension.Add(extension);
                }

                record.buildAssociations();

                ids.Add(id);
                records.Add(id, record);
            }
            return records;
        }

        private Collection<Person> BuildPersonResponseData(Dictionary<string, Person> personRecords)
        {
            Collection<Person> personContracts = new Collection<Person>();
            foreach (var personItem in personRecords)
            {
                personContracts.Add(personItem.Value);
            }
            return personContracts;
        }

        private Dictionary<string, Address> SetupAddresses(out List<string> ids)
        {
            ids = new List<string>();
            string[,] recordData = _addressData;

            int recordCount = recordData.Length / 10;
            Dictionary<string, Address> results = new Dictionary<string, Address>();
            for (int i = 0; i < recordCount; i++)
            {
                Address response = new Address();
                string key = recordData[i, 0].TrimEnd();
                string addressId = (recordData[i, 1] == null) ? String.Empty : recordData[i, 1].TrimEnd();
                List<string> label = (recordData[i, 2] == null) ? null : recordData[i, 2].TrimEnd().Split(';').ToList<string>();
                List<string> lines = (recordData[i, 3] == null) ? null : recordData[i, 3].TrimEnd().Split(';').ToList<string>();
                string city = (recordData[i, 4] == null) ? null : recordData[i, 4].TrimEnd();
                string state = (recordData[i, 5] == null) ? null : recordData[i, 5].TrimEnd();
                string zip = (recordData[i, 6] == null) ? null : recordData[i, 6].TrimEnd();
                string country = (recordData[i, 7] == null) ? null : recordData[i, 7].TrimEnd();
                string countryDesc = (recordData[i, 8] == null) ? null : recordData[i, 8].TrimEnd();
                string modifier = (recordData[i, 9] == null) ? null : recordData[i, 9].TrimEnd();

                response.Recordkey = addressId;
                response.AddressLines = lines;
                response.City = city;
                response.State = state;
                response.Zip = zip;
                response.Country = country;
                // Add Phone Numbers
                response.AddressPhones = new List<string>();
                response.AddressPhoneType = new List<string>();
                response.AddressPhoneExtension = new List<string>();
                string[,] phoneData = _phoneNumberData;
                int phoneCount = phoneData.Length / 5;
                for (int x = 0; x < phoneCount; x++)
                {
                    string number = (phoneData[x, 0] == null) ? string.Empty : phoneData[x, 0].TrimEnd();
                    string type = (phoneData[x, 1] == null) ? string.Empty : phoneData[x, 1].TrimEnd();
                    string extension = (phoneData[x, 2] == null) ? string.Empty : phoneData[x, 2].TrimEnd();
                    response.AddressPhones.Add(number);
                    response.AddressPhoneType.Add(type);
                    response.AddressPhoneExtension.Add(extension);
                }
                response.buildAssociations();

                ids.Add(addressId);
                results.Add(addressId, response);
            }
            return results;
        }

        private Collection<Address> BuildAddressResponse(Dictionary<string, Address> addressRecords)
        {
            Collection<Address> addressContracts = new Collection<Address>();
            foreach (var addressItem in addressRecords)
            {
                addressContracts.Add(addressItem.Value);
            }
            return addressContracts;
        }

        private ApplValcodes BuildValcodeResponse()
        {
            ApplValcodes phoneTypesResponse = new ApplValcodes();
            phoneTypesResponse.ValsEntityAssociation = new List<ApplValcodesVals>();
            phoneTypesResponse.ValsEntityAssociation.Add(new ApplValcodesVals("HO", "Home", "H", "HO", "", "", ""));
            phoneTypesResponse.ValsEntityAssociation.Add(new ApplValcodesVals("BU", "Business", "B", "BU", "", "", ""));
            phoneTypesResponse.ValsEntityAssociation.Add(new ApplValcodesVals("FX", "Fax", "", "FX", "", "", ""));

            return phoneTypesResponse;
        }
    }
}
