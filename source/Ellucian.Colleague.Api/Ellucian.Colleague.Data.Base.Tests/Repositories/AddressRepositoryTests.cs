// Copyright 2012-2018 Ellucian Company L.P. and its affiliates.

using System.Collections.Generic;
using System.Linq;
using Moq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ellucian.Data.Colleague;
using System.Runtime.Caching;
using System.Collections.ObjectModel;
using Ellucian.Colleague.Data.Base.Repositories;
using Ellucian.Colleague.Data.Base.DataContracts;
using slf4net;
using Ellucian.Web.Cache;
using System;
using Ellucian.Data.Colleague.DataContracts;
using System.Threading.Tasks;
using System.Threading;
using Ellucian.Colleague.Domain.Exceptions;

namespace Ellucian.Colleague.Data.Base.Tests.Repositories
{
    [TestClass]
    public class AddressRepositoryTests
    {
        protected List<string> personIds;
        protected List<string> addressIds;
        protected Dictionary<string, Person> personRecords;
        protected Dictionary<string, Address> addressRecords;
        protected int PersonCount = 0;
        Collection<Person> personResponseData;
        Collection<Address> addressResponseData;
        AddressRepository addressRepo;
        Mock<IColleagueDataReader> dataAccessorMock;
        Mock<ICacheProvider> cacheProviderMock;
        


        #region Private data array setup

        private string[,] _personData = { // id,    last name,   firstname, middle, prefix, preferred addr, person addresses, adrel type
                                       {"0000304", "Washington", "George", "", "Gen.", "101", "101;102;103", "HO;HO;BU"},
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
        private string[,] _phoneData = {
                                           {"703-332-9004","CP",""},
                                           {"304-899-4565","HO",""},
                                           {"0-1-9989-998-348","BU","4339"},
                                           {"414-335-9005","FX",""}
                                   };

        Collection<DataContracts.Places> _placeData = new Collection<Places>()
            {
                new Places(){PlacesCountry = "FRA", PlacesDesc= "France", PlacesRegion="Normandy", PlacesSubRegion="Calvados"},
                new Places(){PlacesCountry = "AUS", PlacesDesc= "Australia", PlacesRegion="Victoria", PlacesSubRegion="Barwon South West"},
                new Places(){PlacesCountry = "AUS", PlacesDesc= "Australia", PlacesRegion="Victoria", PlacesSubRegion="Gippsland"},
                new Places(){PlacesCountry = "AUS", PlacesDesc= "Australia", PlacesRegion="Victoria", PlacesSubRegion="Greater Melbourne"},
                new Places(){PlacesCountry = "AUS", PlacesDesc= "Australia", PlacesRegion="Victoria", PlacesSubRegion="Hume"},
                new Places(){PlacesCountry = "AUS", PlacesDesc= "Australia", PlacesRegion="Victoria", PlacesSubRegion="Loddon Mallee"}
            };


        #endregion

        [TestInitialize]
        public void Initialize()
        {
            
            personRecords = SetupPersons(out personIds);
            addressRecords = SetupAddresses(out addressIds);
            addressRepo = BuildValidRepository();
        }

        [TestCleanup]
        public void Cleanup()
        {
            addressRepo = null;
        }

          [TestMethod]
        public async Task GetAddressFromGuidAsync_Success()
        {
            string id, guid, id2, guid2, id3, guid3;
            GuidLookup guidLookup;
            GuidLookupResult guidLookupResult;
            Dictionary<string, GuidLookupResult> guidLookupDict;
            RecordKeyLookup recordLookup;
            RecordKeyLookupResult recordLookupResult;
            Dictionary<string, RecordKeyLookupResult> recordLookupDict;

            // Set up for GUID lookups
            id = "12345";
            id2 = "9876";
            id3 = "0012345";

            guid = "F5FC5310-17F1-49FC-926D-CC6E3DA6DAEA".ToLowerInvariant();
            guid2 = "5B35075D-14FB-45F7-858A-83F4174B76EA".ToLowerInvariant();
            guid3 = "246E16D9-8790-4D7E-ACA1-D5B1CB9D4A24".ToLowerInvariant();

            guidLookup = new GuidLookup(guid);
            guidLookupResult = new GuidLookupResult() { Entity = "ADDRESS", PrimaryKey = id };
            guidLookupDict = new Dictionary<string, GuidLookupResult>();
            recordLookup = new RecordKeyLookup("ADDRESS", id, false);
            recordLookupResult = new RecordKeyLookupResult() { Guid = guid };
            recordLookupDict = new Dictionary<string, RecordKeyLookupResult>();

            dataAccessorMock.Setup(dr => dr.SelectAsync(It.IsAny<GuidLookup[]>())).Returns<GuidLookup[]>(gla =>
            {
                if (gla.Any(gl => gl.Guid == guid))
                {
                    guidLookupDict.Add(guid, guidLookupResult);
                }
                if (gla.Any(gl => gl.Guid == guid2))
                {
                    guidLookupDict.Add(guid2, null);
                }
                if (gla.Any(gl => gl.Guid == guid3))
                {
                    guidLookupDict.Add(guid3, new GuidLookupResult() { Entity = "ADDRESS", PrimaryKey = id3 });
                }
                return Task.FromResult(guidLookupDict);
            });

            var result = await addressRepo.GetAddressFromGuidAsync(guid);
            Assert.AreEqual(id, result);

        }

        [TestMethod]
        [ExpectedException(typeof(RepositoryException))]
        public async Task AddressReposoitory_Get_RepositoryException()
        {
            dataAccessorMock.Setup(i => i.BulkReadRecordAsync<Address>("ADDRESS", It.IsAny<string[]>(), true)).ThrowsAsync(new RepositoryException());

            var actuals = await addressRepo.GetAddressesAsync(0, 200);
        }

        [TestMethod]
        public async Task AddressReposoitory_GetAddresses()
        {
            Collection<DataContracts.Address> Addressdb = new Collection<DataContracts.Address>();

            dataAccessorMock.Setup(a => a.SelectAsync("ADDRESS", It.IsAny<string>())).ReturnsAsync(addressIds.ToArray());

            dataAccessorMock.Setup(a => a.BulkReadRecordAsync<Address>("ADDRESS", It.IsAny<string[]>(), It.IsAny<bool>()))
               .ReturnsAsync(Addressdb);

            var addresses = await addressRepo.GetAddressesAsync(0, 100);
            Assert.IsNotNull(addresses);

            foreach (var address in addresses.Item1)
            {
                var expected = Addressdb.FirstOrDefault(i => i.RecordGuid.Equals(address.Guid));
                Assert.IsNotNull(expected);

                Assert.AreEqual(expected.RecordGuid, address.Guid);
                Assert.AreEqual(expected.AddressChapter, address.AddressChapter);
                Assert.AreEqual(expected.AddressLines, address.AddressLines);
                Assert.AreEqual(expected.IntlPostalCode, address.PostalCode);
                Assert.AreEqual(expected.Recordkey, address.AddressId);
                Assert.AreEqual(expected.CarrierRoute, address.CarrierRoute);
                Assert.AreEqual(expected.City, address.City);
                Assert.AreEqual(expected.CorrectionDigit, address.CorrectionDigit);
                Assert.AreEqual(expected.Country, address.Country);
                Assert.AreEqual(expected.County, address.County);
                Assert.AreEqual(expected.DeliveryPoint, address.DeliveryPoint);
                Assert.AreEqual(expected.IntlLocality, address.IntlLocality);
                Assert.AreEqual(expected.IntlPostalCode, address.IntlPostalCode);
                Assert.AreEqual(expected.IntlRegion, address.IntlRegion);
                Assert.AreEqual(expected.IntlSubRegion, address.IntlSubRegion);
                Assert.AreEqual(expected.Latitude, address.Latitude);
                Assert.AreEqual(expected.Longitude, address.Longitude);
                Assert.AreEqual(expected.AddressRouteCode, address.RouteCode);
                Assert.AreEqual(expected.State, address.State);

                foreach (var phone in address.PhoneNumbers)
                {
                    var phoneExt = expected.AddressPhoneExtension.Where(i => i.Contains(phone.Extension));
                    var phoneNo = expected.AddressPhones.Where(i => i.Contains(phone.Number));
                    var phoneType = expected.AddressPhoneType.Where(i => i.Contains(phone.TypeCode));
                    Assert.AreEqual(phoneNo, phone.Number);
                    Assert.AreEqual(phoneExt, phone.Extension);
                    Assert.AreEqual(phoneType, phone.TypeCode);
                }

            }
        }

        [TestMethod]
        public void CheckSingleAddressProperties_Valid()
        {
            IEnumerable<Ellucian.Colleague.Domain.Base.Entities.Address> addresses = addressRepo.GetPersonAddresses(personIds.ElementAt(0));
            Ellucian.Colleague.Domain.Base.Entities.Address address = addresses.ElementAt(0);
            Assert.AreEqual("101", address.AddressId);
            Assert.AreEqual("65498 Ft. Belvoir Hwy", address.AddressLines.ElementAt(0));
            Assert.AreEqual("Mount Vernon", address.AddressLines.ElementAt(1));
            Assert.AreEqual("Alexandria", address.City);
            Assert.AreEqual("VA", address.State);
            Assert.AreEqual("21348", address.PostalCode);
            Assert.AreEqual("USA", address.Country);
            Assert.AreEqual(2, address.PhoneNumbers.Count());
            Assert.AreEqual("304-899-4565", address.PhoneNumbers.ElementAt(0).Number);
            Assert.AreEqual("703-332-9004", address.PhoneNumbers.ElementAt(1).Number);
            Assert.AreEqual("HO", address.PhoneNumbers.ElementAt(0).TypeCode);
            Assert.AreEqual("CP", address.PhoneNumbers.ElementAt(1).TypeCode);
            Assert.AreEqual("HO", address.TypeCode);
            Assert.AreEqual("Home", address.Type);
        }

        [TestMethod]
        public void SinglePersonAddressCount_Valid()
        {
            var personId = personIds.ElementAt(0);
            IEnumerable<Ellucian.Colleague.Domain.Base.Entities.Address> addresses = addressRepo.GetPersonAddresses(personId);
            Assert.AreEqual(3, addresses.Count());
        }

        [TestMethod]
        public void MultiPersonAddressCount_Valid()
        {
            var ids = new List<string>();
            ids.Add(personIds.ElementAt(0));
            ids.Add(personIds.ElementAt(1));
            ids.Add(personIds.ElementAt(2));
            IEnumerable<Ellucian.Colleague.Domain.Base.Entities.Address> addresses = addressRepo.GetPersonAddressesByIds(ids);
            Assert.AreEqual(7, addresses.Count());
            foreach (var address in addresses)
            {
                Assert.AreEqual(2, address.PhoneNumbers.Count());
            }
        }

        [TestMethod]
        public async Task PlacesCount_Valid()
        {
          
            dataAccessorMock.Setup<Task<Collection<Places>>>(acc => acc.BulkReadRecordAsync<Places>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>())).Returns(() =>
           {
               return Task.FromResult(_placeData);
           });

            var places = await addressRepo.GetPlacesAsync();
            Assert.AreEqual(6, places.Count());
        }


        [TestMethod]
        public async Task PlacesCount_Properties()
        {
            dataAccessorMock.Setup<Task<Collection<Places>>>(acc => acc.BulkReadRecordAsync<Places>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>())).Returns(() =>
            {
                return Task.FromResult(_placeData);
            });
   
            var actual = (await addressRepo.GetPlacesAsync()).FirstOrDefault(x=> x.PlacesDesc =="France" && x.PlacesSubRegion == "Calvados");
            var expected = _placeData.FirstOrDefault(x => x.PlacesDesc == "France" && x.PlacesSubRegion == "Calvados");
            Assert.AreEqual(expected.PlacesCountry, actual.PlacesCountry);
            Assert.AreEqual(expected.PlacesDesc, actual.PlacesDesc);
            Assert.AreEqual(expected.PlacesRegion, actual.PlacesRegion);
            Assert.AreEqual(expected.PlacesSubRegion, actual.PlacesSubRegion);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task GetAddressbyIDAsync_ArgumentNullException()
        {
            var address = addressResponseData.ElementAtOrDefault(0);

            dataAccessorMock.Setup(acc => acc.ReadRecordAsync<DataContracts.Address>(It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(address);

            await addressRepo.GetAddressbyIDAsync(It.IsAny<string>());
        }

        [TestMethod]
        public async Task GetAddressbyIDAsync()
        {
            var address = addressResponseData.ElementAtOrDefault(0);

            dataAccessorMock.Setup(acc => acc.ReadRecordAsync<DataContracts.Address>(It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(address);

            var actual = await addressRepo.GetAddressbyIDAsync(address.Recordkey);

            Assert.AreEqual(address.RecordGuid, actual.Guid);
            Assert.AreEqual(address.AddressChapter, actual.AddressChapter);
            Assert.AreEqual(address.AddressLines, actual.AddressLines);
            Assert.AreEqual(address.Country, actual.Country);
            Assert.AreEqual(address.County, actual.County);
            Assert.AreEqual(address.Zip, actual.PostalCode);
            Assert.AreEqual(address.IntlLocality, actual.IntlLocality);
            Assert.AreEqual(address.IntlPostalCode, actual.IntlPostalCode);
            Assert.AreEqual(address.IntlRegion, actual.IntlRegion);
            Assert.AreEqual(address.IntlSubRegion, actual.IntlSubRegion);
            Assert.AreEqual(address.Latitude, actual.Longitude);
            Assert.AreEqual(address.Longitude, actual.Longitude);

        }

        [TestMethod]
        public async Task AddressReposoitory_GetZipCodeGuidsCollection()
        {
            IEnumerable<string> sublist = new List<string>() { "1", "2" };
            Dictionary<string, RecordKeyLookupResult> recordKeyLookupResults = new Dictionary<string, RecordKeyLookupResult>();
            recordKeyLookupResults.Add("ZIPCODE+12345", new RecordKeyLookupResult() { Guid = "854da721-4191-4875-bf58-7d6c00ffea8f" });
            recordKeyLookupResults.Add("ZIPCODE+23456", new RecordKeyLookupResult() { Guid = "71e1a806-24a8-4d93-91a2-02d86056b63c" });
            List<KeyValuePair<string, RecordKeyLookupResult>> list = recordKeyLookupResults.ToList();

            dataAccessorMock.Setup(i => i.SelectAsync("ZIP.CODE.XLAT", It.IsAny<string>())).ReturnsAsync(new[] { "12345", "23456", "35678", "44589" });
            dataAccessorMock.Setup(i => i.SelectAsync(It.IsAny<RecordKeyLookup[]>())).ReturnsAsync(recordKeyLookupResults);

            var results = await addressRepo.GetZipCodeGuidsCollectionAsync(sublist);
            Assert.IsNotNull(results);
            Assert.AreEqual(2, results.Count());
            foreach (var result in results)
            {
                RecordKeyLookupResult recordKeyLookupResult = null;
                recordKeyLookupResults.TryGetValue(string.Concat("ZIPCODE+", result.Key), out recordKeyLookupResult);

                Assert.AreEqual(result.Value, recordKeyLookupResult.Guid);
            }
        }

        [TestMethod]
        public async Task AddressReposoitory_GetZipCodeGuidsCollection_Empty()
        {
            IEnumerable<string> sublist = new List<string>() { };
            var results = await addressRepo.GetZipCodeGuidsCollectionAsync(sublist);
            Assert.IsNotNull(results);
            Assert.AreEqual(0, results.Count());
        }

        [TestMethod]
        [ExpectedException(typeof(RepositoryException))]
        public async Task AddressReposoitory_GetZipCodeGuidsCollection_Exception()
        {
            IEnumerable<string> sublist = new List<string>() { "1", "2" };

            dataAccessorMock.Setup(i => i.SelectAsync("ZIP.CODE.XLAT", It.IsAny<string>())).ThrowsAsync(new RepositoryException());
            dataAccessorMock.Setup(i => i.SelectAsync(It.IsAny<RecordKeyLookup[]>())).ThrowsAsync(new RepositoryException());

            await addressRepo.GetZipCodeGuidsCollectionAsync(sublist);
        }

        private AddressRepository BuildValidRepository()
        {
            var transFactoryMock = new Mock<IColleagueTransactionFactory>();

            var loggerMock = new Mock<ILogger>();

            // Cache mocking
            cacheProviderMock = new Mock<ICacheProvider>();
            cacheProviderMock.Setup<Task<Tuple<object, SemaphoreSlim>>>(x =>
               x.GetAndLockSemaphoreAsync(It.IsAny<string>(), null))
               .Returns(Task.FromResult(new Tuple<object, SemaphoreSlim>(
               null,
               new SemaphoreSlim(1, 1)
           )));


            var localCacheMock = new Mock<ObjectCache>();
            //cacheProviderMock.Setup(provider => provider.GetCache(It.IsAny<string>())).Returns(localCacheMock.Object);

            // Set up data accessor for mocking 
            dataAccessorMock = new Mock<IColleagueDataReader>();
            transFactoryMock.Setup(transFac => transFac.GetDataReader()).Returns(dataAccessorMock.Object);

            // Mock the call for getting multiple person records
            personResponseData = BuildPersonResponseData(personRecords);
            dataAccessorMock.Setup<ICollection<Person>>(acc => acc.BulkReadRecord<Person>("PERSON",It.IsAny<string[]>(), true)).Returns(personResponseData);
            
            // Mock the call for a single person record
            var personId = personIds.ElementAt(0);
            var personResponse = personResponseData.ElementAt(0);
            dataAccessorMock.Setup<Person>(acc => acc.ReadRecord<Person>("PERSON", personId, true)).Returns(personResponse);

            // mock data accessor PHONE.TYPES
            dataAccessorMock.Setup<ApplValcodes>(a =>
                a.ReadRecord<ApplValcodes>("CORE.VALCODES", "PHONE.TYPES", true))
                .Returns(new ApplValcodes()
                {
                    ValInternalCode = new List<string>() { "HO", "BU", "CP", "FX" },
                    ValExternalRepresentation = new List<string>() { "Home", "Business", "Cell Phone", "Fax" },
                    ValActionCode2 = new List<string>() { "H", "B", "", "" },
                    ValsEntityAssociation = new List<ApplValcodesVals>()
                    {
                        new ApplValcodesVals() 
                        {
                            ValInternalCodeAssocMember = "HO",
                            ValExternalRepresentationAssocMember = "Home",
                            ValActionCode2AssocMember = "H"
                        },
                         new ApplValcodesVals() 
                        {
                            ValInternalCodeAssocMember = "BU",
                            ValExternalRepresentationAssocMember = "Business",
                            ValActionCode2AssocMember = "B"
                        },
                        new ApplValcodesVals() 
                        {
                            ValInternalCodeAssocMember = "CP",
                            ValExternalRepresentationAssocMember = "Cell Phone",
                            ValActionCode2AssocMember = ""
                        },
                        new ApplValcodesVals() 
                        {
                            ValInternalCodeAssocMember = "FX",
                            ValExternalRepresentationAssocMember = "FAX",
                            ValActionCode2AssocMember = ""
                        }
                    }
                });

            // mock data accessor ADREL.TYPES
            dataAccessorMock.Setup<ApplValcodes>(a =>
                a.ReadRecord<ApplValcodes>("CORE.VALCODES", "ADREL.TYPES", true))
                .Returns(new ApplValcodes()
                {
                    ValInternalCode = new List<string>() { "HO", "BU" },
                    ValExternalRepresentation = new List<string>() { "Home", "Business" },
                    ValsEntityAssociation = new List<ApplValcodesVals>()
                    {
                        new ApplValcodesVals() 
                        {
                            ValInternalCodeAssocMember = "HO",
                            ValExternalRepresentationAssocMember = "Home"
                        },
                        new ApplValcodesVals() 
                        {
                            ValInternalCodeAssocMember = "BU",
                            ValExternalRepresentationAssocMember = "Business"
                        }
                    }
                });

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

            // Set up single address response
            var addressId = allPersonAddressIds.ElementAt(0);
            dataAccessorMock.Setup<Address>(acc => acc.ReadRecord<Address>("ADDRESS", addressId, true)).Returns(addressResponse.ElementAt(0));

            // Construct address repository
            addressRepo = new AddressRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object);

            return addressRepo;
        }

        private Dictionary<string, Person> SetupPersons(out List<string> ids)
        {
            ids = new List<string>();
            string[,] recordData = _personData;
            string[,] phoneData = _phoneData;

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
                List<string> adrelTypes = (recordData[i, 7] == null) ? new List<string>() : recordData[i, 7].TrimEnd().Split(';').ToList<string>();

                DataContracts.Person record = new DataContracts.Person();
                record.Recordkey = id;
                record.LastName = last;
                record.FirstName = first;
                record.MiddleName = middle;
                record.PreferredAddress = preferredAddress;
                record.PersonAddresses = addresses;
                record.AddrType = adrelTypes;

                // Add Personal Phone numbers to Person data
                record.PersonalPhoneNumber = new List<string>();
                record.PersonalPhoneType = new List<string>();
                record.PersonalPhoneExtension = new List<string>();
                int phoneCount = phoneData.Length / 3;
                for (int ii = 0; ii < phoneCount; ii++)
                {
                    string number = phoneData[ii, 0].TrimEnd();
                    string type = (phoneData[ii, 1] == null) ? String.Empty : phoneData[ii, 1].TrimEnd();
                    string ext = (phoneData[ii, 2] == null) ? String.Empty : phoneData[ii, 2].TrimEnd();
                    // Cell Phone is the only phone type stored in Personal Phone fields.
                    if (type == "CP")
                    {
                        record.PersonalPhoneNumber.Add(number);
                        record.PersonalPhoneType.Add(type);
                        record.PersonalPhoneExtension.Add(ext);
                    }
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
            string[,] phoneData = _phoneData;

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
                response.RecordGuid = "fakeguid" + i.ToString();

                // Add Home, Business and Fax Phone numbers to Address data
                // (Only Home Type should be returned from repository call.)
                response.AddressPhones = new List<string>();
                response.AddressPhoneType = new List<string>();
                response.AddressPhoneExtension = new List<string>();
                int phoneCount = phoneData.Length / 3;
                for (int ii = 0; ii < phoneCount; ii++)
                {
                    string number = phoneData[ii, 0].TrimEnd();
                    string type = (phoneData[ii, 1] == null) ? String.Empty : phoneData[ii, 1].TrimEnd();
                    string ext = (phoneData[ii, 2] == null) ? String.Empty : phoneData[ii, 2].TrimEnd();
                    // Cell phone is defined as a personal phone and stored with person.
                    if (type != "CP")
                    {
                        response.AddressPhones.Add(number);
                        response.AddressPhoneType.Add(type);
                        response.AddressPhoneExtension.Add(ext);
                    }
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
    }
}
