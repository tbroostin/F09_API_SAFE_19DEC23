// Copyright 2012-2016 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ellucian.Data.Colleague;
using Moq;
using System.Runtime.Caching;
using Ellucian.Colleague.Domain.Base.Entities;
using Ellucian.Colleague.Data.Base.Repositories;
using Ellucian.Colleague.Data.Base.Transactions;
using System.Threading.Tasks;
using System.Threading;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Web.Http.Configuration;

namespace Ellucian.Colleague.Data.Base.Tests.Repositories
{
    [TestClass]
    public class PersonRepositoryTests
    {

        #region Get Person

        [TestClass]
        public class GetPerson : BasePersonSetup
        {
            PersonRepository repository;

            [TestInitialize]
            public void Initialize()
            {
                // Initialize person setup and Mock framework
                PersonSetupInitialize();

                // Build the test repository
                repository = new PersonRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object, apiSettings);

            }

            [TestCleanup]
            public void Cleanup()
            {
                repository = null;
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task GetNullIdException()
            {
                await repository.GetAsync<Person>(string.Empty,
                    person => new Person(person.Recordkey, person.LastName));
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentOutOfRangeException))]
            public async Task GetSingleInvalidIdException()
            {
                await repository.GetAsync<Person>("123",
                    person => new Person(person.Recordkey, person.LastName));
            }

            [TestMethod]
            public async Task GetSingleIdTests()
            {
                foreach (var kvp in personRecords)
                {
                    string personId = kvp.Key;
                    DataContracts.Person record = kvp.Value;
                    Person result = await repository.GetAsync<Person>(personId,
                       person => new Person(person.Recordkey, person.LastName)); ;
                    CompareRecords(record, result);
                }
            }

            // GetPreferredName Tests - Given a name hierarchy list of "MA", "XYZ", "PF"

            [TestMethod]
            public async Task GetPreferredName_WhenPreferredNameOverrideSupplied()
            {
                var testPersonRecord = personRecords["0000001"];
                if (testPersonRecord != null)
                {
                    string personId = testPersonRecord.Recordkey;
                    Person result = await repository.GetAsync<Person>(personId,
                       person => new Person(person.Recordkey, person.LastName)); ;
                    Assert.AreEqual("Preferred Name Override", result.PreferredName);
                }
            }

            [TestMethod]
            public async Task GetPreferredName_ShortCut_II()
            {
                var testPersonRecord = personRecords["0000002"];
                if (testPersonRecord != null)
                {
                    string personId = testPersonRecord.Recordkey;
                    Person result = await repository.GetAsync<Person>(personId,
                       person => new Person(person.Recordkey, person.LastName)); ;
                    Assert.AreEqual("J. P. Adams", result.PreferredName);
                }
            }

            [TestMethod]
            public async Task GetPreferredName_ShortCut_IM_NoMiddleName()
            {
                var testPersonRecord = personRecords["0000003"];
                if (testPersonRecord != null)
                {
                    string personId = testPersonRecord.Recordkey;
                    Person result = await repository.GetAsync<Person>(personId,
                       person => new Person(person.Recordkey, person.LastName)); ;
                    Assert.AreEqual("T. Jefferson", result.PreferredName);
                }
            }

            [TestMethod]
            public async Task GetPreferredName_ShortCut_IM_WithMiddleName()
            {
                var testPersonRecord = personRecords["0000004"];
                if (testPersonRecord != null)
                {
                    string personId = testPersonRecord.Recordkey;
                    Person result = await repository.GetAsync<Person>(personId,
                       person => new Person(person.Recordkey, person.LastName)); ;
                    Assert.AreEqual("J. Adam Madison", result.PreferredName);
                }
            }

            [TestMethod]
            public async Task GetPreferredName_DefaultUsed()
            {
                var testPersonRecord = personRecords["0000005"];
                if (testPersonRecord != null)
                {
                    string personId = testPersonRecord.Recordkey;
                    Person result = await repository.GetAsync<Person>(personId,
                       person => new Person(person.Recordkey, person.LastName)); ;
                    // Preferred Name override is blank - default to First middle initial last - where person has first middle and last)
                    Assert.AreEqual("James W. Monroe", result.PreferredName);
                }
            }

            [TestMethod]
            public async Task GetPreferredName_FormattedNameUsed()
            {
                var testPersonRecord = personRecords["0000006"];
                if (testPersonRecord != null)
                {
                    string personId = testPersonRecord.Recordkey;
                    Person result = await repository.GetAsync<Person>(personId,
                       person => new Person(person.Recordkey, person.LastName)); ;
                    // Has a formatted name type of "XYZ" so the formatted name should be used.
                    Assert.AreEqual("Adams Formatted Name", result.PreferredName);
                }
            }

            [TestMethod]
            public async Task GetPreferredName_DefaultUsed_NoFirst_NoMiddle()
            {
                var testPersonRecord = personRecords["9999999"];
                if (testPersonRecord != null)
                {
                    string personId = testPersonRecord.Recordkey;
                    Person result = await repository.GetAsync<Person>(personId,
                       person => new Person(person.Recordkey, person.LastName)); ;
                    // Preferred Name override is blank - default to First middle initial last - where person only has a last name)
                    Assert.AreEqual("Test", result.PreferredName);
                }
            }

            [TestMethod]
            public async Task GetPreferredName_DefaultUsed_ProperTrims()
            {
                var testPersonRecord = personRecords["9999998"];
                if (testPersonRecord != null)
                {
                    string personId = testPersonRecord.Recordkey;
                    Person result = await repository.GetAsync<Person>(personId,
                       person => new Person(person.Recordkey, person.LastName)); ;
                    // Confirm trims are working correctly
                    Assert.AreEqual("Mary Beth A. Multi Part-Last", result.PreferredName);
                }
            }

            [TestMethod]
            public async Task GetPreferredName_MaidenName()
            {
                var testPersonRecord = personRecords["9999994"];
                if (testPersonRecord != null)
                {
                    string personId = testPersonRecord.Recordkey;
                    Person result = await repository.GetAsync<Person>(personId,
                       person => new Person(person.Recordkey, person.LastName)); ;
                    // Person has a maiden last and maiden middle but no maiden first so it should use their first name. 
                    Assert.AreEqual("Carla Smith Maiden Name", result.PreferredName);
                }
            }

            [TestMethod]
            public async Task GetPreferredName_PrefixSuffix()
            {
                var testPersonRecord = personRecords["9999995"];
                if (testPersonRecord != null)
                {
                    string personId = testPersonRecord.Recordkey;
                    Person result = await repository.GetAsync<Person>(personId,
                       person => new Person(person.Recordkey, person.LastName)); ;
                    // Person has a prefix and suffix. Make sure spacing is correct.
                    Assert.AreEqual("Mrs. Carla M. Test, Pharm.D., J.D., D.C.", result.PreferredName);
                }
            }

            [TestMethod]
            public async Task GetPreferredName_FMLS_SpecialCharactersStripped()
            {
                // Given a name that contains special characters - make sure all unwanted characters are stripped.
                // Resulting name should only have alphanumerica spaces dashes and ampersands.

                // mock data reader for getting the Preferred Name Addr Hierarchy
                dataReaderMock.Setup<Task<Ellucian.Colleague.Data.Base.DataContracts.NameAddrHierarchy>>(a =>
                    a.ReadRecordAsync<Ellucian.Colleague.Data.Base.DataContracts.NameAddrHierarchy>("NAME.ADDR.HIERARCHY", "PREFERRED", true))
                    .ReturnsAsync(new Ellucian.Colleague.Data.Base.DataContracts.NameAddrHierarchy()
                    {
                        Recordkey = "PREFERRED",
                        NahNameHierarchy = new List<string>() { "FMLS", "PF" }
                    });

                var testPersonRecord = personRecords["9999996"];
                if (testPersonRecord != null)
                {
                    string personId = testPersonRecord.Recordkey;
                    Person result = await repository.GetAsync<Person>(personId,
                       person => new Person(person.Recordkey, person.LastName)); ;
                    // Person has a prefix and suffix. Make sure spacing is correct.
                    Assert.AreEqual("Carla M Test Suf-fix123 & DC", result.PreferredName);
                }
            }

            [TestMethod]
            public async Task GetPreferredName_EmptyPreferredNameAddrHierarchy()
            {
                // mock data reader for getting the Preferred Name Addr Hierarchy
                dataReaderMock.Setup<Ellucian.Colleague.Data.Base.DataContracts.NameAddrHierarchy>(a =>
                    a.ReadRecord<Ellucian.Colleague.Data.Base.DataContracts.NameAddrHierarchy>("NAME.ADDR.HIERARCHY", "PREFERRED", true))
                    .Returns(new Ellucian.Colleague.Data.Base.DataContracts.NameAddrHierarchy());

                var testPersonRecord = personRecords["0000001"];
                if (testPersonRecord != null)
                {
                    string personId = testPersonRecord.Recordkey;
                    Person result = await repository.GetAsync<Person>(personId,
                       person => new Person(person.Recordkey, person.LastName)); ;
                    Assert.AreEqual("Preferred Name Override", result.PreferredName);
                }
            }

            [TestMethod]
            public async Task GetPreferredName_LFM()
            {
                // Given a name that contains special characters - make sure all unwanted characters are stripped.
                // Resulting name should only have alphanumerica spaces dashes and ampersands.

                // mock data reader for getting the Preferred Name Addr Hierarchy
                dataReaderMock.Setup<Task<Ellucian.Colleague.Data.Base.DataContracts.NameAddrHierarchy>>(a =>
                    a.ReadRecordAsync<Ellucian.Colleague.Data.Base.DataContracts.NameAddrHierarchy>("NAME.ADDR.HIERARCHY", "PREFERRED", true))
                    .ReturnsAsync(new Ellucian.Colleague.Data.Base.DataContracts.NameAddrHierarchy()
                    {
                        Recordkey = "PREFERRED",
                        NahNameHierarchy = new List<string>() { "LFM" }
                    });

                var testPersonRecord = personRecords["9999998"];
                if (testPersonRecord != null)
                {
                    string personId = testPersonRecord.Recordkey;
                    Person result = await repository.GetAsync<Person>(personId,
                       person => new Person(person.Recordkey, person.LastName)); ;
                    // Confirm trims are working correctly
                    Assert.AreEqual("Multi Part-Last, Mary Beth A.", result.PreferredName);
                }
            }

            [TestMethod]
            public async Task GetSingleGuidTests()
            {
                foreach (var kvp in personRecords)
                {
                    string personGuid = kvp.Value.RecordGuid;
                    DataContracts.Person record = kvp.Value;
                    Person result = await repository.GetByGuidNonCachedAsync<Person>(personGuid,
                       person => new Person(person.Recordkey, person.LastName)); ;
                    CompareRecords(record, result);
                }
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task GetSingleGuidNoGuidException()
            {
                string guid = null;
                await repository.GetByGuidNonCachedAsync<Person>(guid,
                    person => new Person(person.Recordkey, person.LastName));
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentOutOfRangeException))]
            public async Task GetSingleGuidNoRecordException()
            {
                string guid = "Does not exist";
                await repository.GetByGuidNonCachedAsync<Person>(guid,
                    person => new Person(person.Recordkey, person.LastName));
            }

            [TestMethod]
            public async Task GetSinglePersonByGuidTests()
            {
                foreach (var kvp in personRecords)
                {
                    string personGuid = kvp.Value.RecordGuid;
                    DataContracts.Person record = kvp.Value;
                    Person result = await repository.GetPersonByGuidNonCachedAsync(personGuid);
                    CompareRecords(record, result);
                }
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task GetSinglePersonByGuidNoGuidException()
            {
                string guid = null;
                await repository.GetPersonByGuidNonCachedAsync(guid);
            }

            [TestMethod]
            public async Task GetMultipleByGuidTests()
            {
                var personGuids = personRecords.Select(pr => pr.Value.RecordGuid).ToList();

                IEnumerable<Person> result = await repository.GetByGuidNonCachedAsync<Person>(personGuids,
                    person => new Person(person.Recordkey, person.LastName));

                foreach (var personEntity in result)
                {
                    var personRecord = personRecords.Where(pr => pr.Value.RecordGuid == personEntity.Guid).FirstOrDefault();
                    CompareRecords(personRecord.Value, personEntity);
                }
            }

            [TestMethod]
            public async Task GetMultipleByGuidWithBadDataTests()
            {
                // mock person data with 2 good records and 1 bad
                string person1Id = "0000001";
                string person2Id = "0000002";
                string person3Id = "0000003";
                var mockedPersonData = new Collection<DataContracts.Person>();
                mockedPersonData.Add(new DataContracts.Person
                    {
                        Recordkey = person1Id,
                        RecordGuid = "b0665613-34fb-4fb6-bc8e-414339f13bb2",
                        FirstName = "First1",
                        LastName = "Last1"
                    });
                mockedPersonData.Add(new DataContracts.Person
                    {
                        Recordkey = person2Id,
                        RecordGuid = "7ee36f6b-cfbf-4d2c-8760-aa034781108d",
                        FirstName = "First2",
                        LastName = string.Empty  // bad data example
                    });
                mockedPersonData.Add(new DataContracts.Person
                    {
                        Recordkey = person3Id,
                        RecordGuid = "3cbbf8cc-8ee2-415d-98de-088d51d6ca9a",
                        FirstName = "First3",
                        LastName = "Last3"
                    });
                dataReaderMock.Setup<Task<Collection<DataContracts.Person>>>(acc => acc.BulkReadRecordAsync<DataContracts.Person>(It.IsAny<GuidLookup[]>(), true)).ReturnsAsync(mockedPersonData);

                var result = await repository.GetByGuidNonCachedAsync<Person>(mockedPersonData.Select(p => p.RecordGuid),
                    person => new Person(person.Recordkey, person.LastName));

                // the two good records will be returned as valid person objects and the third left out
                Assert.AreEqual(2, result.Count());
                Assert.AreEqual(person1Id, result.Where(p => p.Id == person1Id).FirstOrDefault().Id);
                Assert.AreEqual(person3Id, result.Where(p => p.Id == person3Id).FirstOrDefault().Id);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task GetMultipleNoGuidsException()
            {
                List<string> personGuids = null;

                await repository.GetByGuidNonCachedAsync<Person>(personGuids,
                    person => new Person(person.Recordkey, person.LastName));
            }

            [TestMethod]
            public async Task GetMultiplePersonByGuidTests()
            {
                var personGuids = personRecords.Select(pr => pr.Value.RecordGuid).ToList();
                IEnumerable<Person> result = await repository.GetPersonByGuidNonCachedAsync(personGuids);

                foreach (var personEntity in result)
                {
                    var personRecord = personRecords.Where(pr => pr.Value.RecordGuid == personEntity.Guid).FirstOrDefault();
                    CompareRecords(personRecord.Value, personEntity);
                }
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task GetMultiplePersonByGuidNoGuidException()
            {
                List<string> personGuids = null;
                await repository.GetPersonByGuidNonCachedAsync(personGuids);
            }

            private void CompareRecords(DataContracts.Person record, Person result)
            {
                string personId = record.Recordkey;
                Assert.AreEqual(personId, result.Id);
                Assert.AreEqual(record.RecordGuid, result.Guid);
                Assert.AreEqual(record.LastName, result.LastName);
                Assert.AreEqual(record.FirstName, result.FirstName);
                Assert.AreEqual(record.MiddleName, result.MiddleName);
                Assert.AreEqual(record.Prefix, result.Prefix);
                Assert.AreEqual(record.Suffix, result.Suffix);
                Assert.AreEqual(record.Nickname, result.Nickname);
                Assert.AreEqual(record.Ssn, result.GovernmentId);
                Assert.AreEqual(record.MaritalStatus, result.MaritalStatusCode);
                Assert.AreEqual(record.PerRaces, result.RaceCodes);
                Assert.AreEqual(record.PerEthnics, result.EthnicCodes);
                Assert.AreEqual(record.Gender, result.Gender);
                Assert.AreEqual(record.BirthDate, result.BirthDate);
                Assert.AreEqual(record.DeceasedDate, result.DeceasedDate);
                if (record.PeopleEmailEntityAssociation != null && record.PeopleEmailEntityAssociation.Count() > 0)
                {
                    foreach (var peopleEmailEntity in record.PeopleEmailEntityAssociation)
                    {
                        var type = peopleEmailEntity.PersonEmailTypesAssocMember;
                        var emailAddress = peopleEmailEntity.PersonEmailAddressesAssocMember;
                        var resultEmail = result.GetEmailAddresses(type);
                        if (resultEmail != null && resultEmail.Count() > 0)
                        {
                            var emailOfType = resultEmail.FirstOrDefault();
                            Assert.AreEqual(emailAddress, emailOfType);
                        }
                    }
                }

                // Handle cases with no preferred address (no address at all)
                if (String.IsNullOrEmpty(preferredAddressResponses[personId].OutAddressId))
                {
                    Assert.AreEqual(0, result.PreferredAddress.Count());
                }
                else
                {
                    Assert.AreEqual(preferredAddressResponses[personId].OutAddressLabel.Count, result.PreferredAddress.Count());
                    for (int i = 0; i < result.PreferredAddress.Count(); i++)
                    {
                        Assert.AreEqual(preferredAddressResponses[personId].OutAddressLabel[i], result.PreferredAddress.ElementAt(i));
                    }
                }
            }
        }

        [TestClass]
        public class GetBasePersonAsync : BasePersonSetup
        {
            PersonRepository repository;

            [TestInitialize]
            public void Initialize()
            {
                // Initialize person setup and Mock framework
                PersonSetupInitialize();

                // Build the test repository
                repository = new PersonRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object, apiSettings);

                cacheProviderMock.Setup<Task<Tuple<object, SemaphoreSlim>>>(x =>
                   x.GetAndLockSemaphoreAsync(It.IsAny<string>(), null))
                   .ReturnsAsync(new Tuple<object, SemaphoreSlim>(null, new SemaphoreSlim(1, 1)));

            }

            [TestCleanup]
            public void Cleanup()
            {
                repository = null;
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task GetBasePersonAsync_NullIdException()
            {
                await repository.GetBaseAsync<Profile>(string.Empty, person => new Profile(person.Recordkey, person.LastName));
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentOutOfRangeException))]
            public async Task GetBasePersonAsync_InvalidIdException()
            {
                await repository.GetBaseAsync<Profile>("123", person => new Profile(person.Recordkey, person.LastName));
            }

            [TestMethod]
            public async Task GetBasePersonAsync_GetCachedIsTrue()
            {
                foreach (var personItem in personRecords)
                {
                    string personId = personItem.Key;
                    DataContracts.Person record = personItem.Value;
                    dataReaderMock.Setup(acc => acc.ReadRecordAsync<DataContracts.Person>("PERSON", record.Recordkey, true)).ReturnsAsync(record);
                    Profile result = await repository.GetBaseAsync<Profile>(personId, person => new Profile(person.Recordkey, person.LastName));
                    // Spot check a couple of attributes: All other attributes are tested by the Get<> method tests
                    Assert.AreEqual(record.Recordkey, result.Id);
                    Assert.AreEqual(record.LastName, result.LastName);
                    // Verify that caching Contains method was called on the cached person contract
                    var cacheKey = repository.BuildFullCacheKey("PersonContract" + record.Recordkey);
                    cacheProviderMock.Verify(m => m.Contains(cacheKey, null));
                }
            }

            [TestMethod]
            public async Task GetBasePersonAsync_GetCachedIsFalse()
            {
                foreach (var personItem in personRecords)
                {
                    string personId = personItem.Key;
                    DataContracts.Person record = personItem.Value;
                    dataReaderMock.Setup(acc => acc.ReadRecordAsync<DataContracts.Person>("PERSON", record.Recordkey, true)).ReturnsAsync(record);
                    Profile result = await repository.GetBaseAsync<Profile>(personId, person => new Profile(person.Recordkey, person.LastName), false);
                    // Spot check a couple of attribues: All other attributes are tested by the Get<> method tests       
                    Assert.AreEqual(record.Recordkey, result.Id);
                    Assert.AreEqual(record.LastName, result.LastName);
                    // Verify that caching Add method was called for the person data contract
                    var cacheKey = repository.BuildFullCacheKey("PersonContract" + record.Recordkey);
                    cacheProviderMock.Verify(m => m.AddAndUnlockSemaphore(cacheKey, record, It.IsAny<SemaphoreSlim>(), It.IsAny<CacheItemPolicy>(), null));
                }
            }
    
        }
        #endregion

        #region Get Person Integration

        [TestClass]
        public class GetPersonIntegration : BasePersonSetup
        {
            PersonRepository repository;

            [TestInitialize]
            public void Initialize()
            {
                // Initialize person setup and Mock framework
                PersonSetupInitialize();

                // Build the test repository
                repository = new PersonRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object, apiSettings);

            }

            [TestCleanup]
            public void Cleanup()
            {
                repository = null;
            }      

            [TestMethod]
            public async Task GetPersonIntegrationByGuidTests()
            {
                foreach (var kvp in personRecords)
                {
               
                   string personGuid = kvp.Value.RecordGuid;
                    DataContracts.Person record = kvp.Value;

                    var result = await repository.GetPersonIntegrationByGuidNonCachedAsync(personGuid);

                   // Person result = await repository.GetIntegrationByGuidNonCachedAsync<PersonIntegration>(personGuid,
                    //   person => new PersonIntegration(person.Recordkey, person.LastName)); ;
                    CompareRecords(record, result);
                }
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task GetPersonIntegrationByGuid_ArgumentNullException()
            {
                string guid = null;
                await repository.GetPersonIntegrationByGuidNonCachedAsync(guid);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentOutOfRangeException))]
            public async Task GetPersonIntegrationByGuid_ArgumentOutOfRangeException()
            {
                string guid = "Does not exist";
                await repository.GetPersonIntegrationByGuidNonCachedAsync(guid);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task GetPersonIntegrationByGuidAsync_ArgumentNullException()
            {
                string guid = "";
                await repository.GetPersonIntegrationByGuidAsync(guid, It.IsAny<bool>());
            }

            [TestMethod]
            [ExpectedException(typeof(RepositoryException))]
            public async Task GetPersonIntegrationByGuidAsync_RepositoryException()
            {
                string guid = "1";
                dataReaderMock.Setup(repo => repo.ReadRecordAsync<DataContracts.Person>(It.IsAny<GuidLookup>(), It.IsAny<bool>())).ThrowsAsync(new RepositoryException());
                await repository.GetPersonIntegrationByGuidAsync(guid, It.IsAny<bool>());
            }

            [TestMethod]
            [ExpectedException(typeof(Exception))]
            public async Task GetPersonIntegrationByGuidAsync_Exception()
            {
                string guid = "1";
                dataReaderMock.Setup(repo => repo.ReadRecordAsync<DataContracts.Person>(It.IsAny<GuidLookup>(), It.IsAny<bool>())).ThrowsAsync(new Exception());
                await repository.GetPersonIntegrationByGuidAsync(guid, It.IsAny<bool>());
            }

            [TestMethod]
            public async Task GetMultiplePersonIntegrationByGuid()
            {
                var personGuids = personRecords.Select(pr => pr.Value.RecordGuid).ToList();



                var result = await repository.GetPersonIntegrationByGuidNonCachedAsync(personGuids);
                   
                foreach (var personEntity in result)
                {
                    var personRecord = personRecords.Where(pr => pr.Value.RecordGuid == personEntity.Guid).FirstOrDefault();
                    var personIntgRecord = personIntgRecords.Where(pr => pr.Value.Recordkey == personEntity.Id).FirstOrDefault();
                    CompareRecords(personRecord.Value,  personEntity, personIntgRecord.Value);
                }
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task GetMultipleNoGuidsException()
            {
                List<string> personGuids = null;

                await repository.GetPersonIntegrationByGuidNonCachedAsync(personGuids);
            }

            [TestMethod]
            public async Task GetMultiplePersonByGuidTests()
            {
                var personGuids = personRecords.Select(pr => pr.Value.RecordGuid).ToList();
               var result = await repository.GetPersonIntegrationByGuidNonCachedAsync(personGuids);

                foreach (var personEntity in result)
                {
                    var personRecord = personRecords.Where(pr => pr.Value.RecordGuid == personEntity.Guid).FirstOrDefault();
                    CompareRecords(personRecord.Value, personEntity);
                }
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task GetMultiplePersonByGuidNoGuidException()
            {
                List<string> personGuids = null;
                await repository.GetPersonIntegrationByGuidNonCachedAsync(personGuids);
            }

            private void CompareRecords(DataContracts.Person record, PersonIntegration result, DataContracts.PersonIntg recordPersonIntg = null)
            {
                string personId = record.Recordkey;
                Assert.AreEqual(personId, result.Id);
                Assert.AreEqual(record.RecordGuid, result.Guid);
                Assert.AreEqual(record.LastName, result.LastName);
                Assert.AreEqual(record.FirstName, result.FirstName);
                Assert.AreEqual(record.MiddleName, result.MiddleName);
                Assert.AreEqual(record.Prefix, result.Prefix);
                Assert.AreEqual(record.Suffix, result.Suffix);
                Assert.AreEqual(record.Nickname, result.Nickname);
                Assert.AreEqual(record.Ssn, result.GovernmentId);
                Assert.AreEqual(record.MaritalStatus, result.MaritalStatusCode);
                Assert.AreEqual(record.PerRaces, result.RaceCodes);
                //if (recordPersonIntg == null)
                //    Assert.AreEqual(record.PerEthnics, result.EthnicCodes);
                Assert.AreEqual(record.Gender, result.Gender);
                Assert.AreEqual(record.BirthDate, result.BirthDate);
                Assert.AreEqual(record.DeceasedDate, result.DeceasedDate);
                if (record.PeopleEmailEntityAssociation != null && record.PeopleEmailEntityAssociation.Count() > 0)
                {
                    foreach (var peopleEmailEntity in record.PeopleEmailEntityAssociation)
                    {
                        var type = peopleEmailEntity.PersonEmailTypesAssocMember;
                        var emailAddress = peopleEmailEntity.PersonEmailAddressesAssocMember;
                        var resultEmail = result.GetEmailAddresses(type);
                        if (resultEmail != null && resultEmail.Count() > 0)
                        {
                            var emailOfType = resultEmail.FirstOrDefault();
                            Assert.AreEqual(emailAddress, emailOfType);
                        }
                    }
                }

                if (recordPersonIntg != null)
                {
                    Assert.AreEqual(recordPersonIntg.PerIntgBirthCountry, result.BirthCountry);
                }
            }
        }

      
        #endregion

        #region Get Person Pin

        [TestClass]
        public class GetPersonPin : BasePersonSetup
        {
            //protected Mock<IColleagueDataReader> dataReaderMock;
            PersonRepository repository;

            [TestInitialize]
            public void Initialize()
            {
                // Initialize person setup and Mock framework
                MockInitialize();
                //dataReaderMock = new Mock<IColleagueDataReader>();
                //cacheProviderMock = new Mock<Web.Cache.ICacheProvider>();
                //transFactoryMock = new Mock<IColleagueTransactionFactory>();
                //loggerMock = new Mock<slf4net.ILogger>();
                //apiSettings = new ApiSettings("TEST");

                // Build the test repository
                repository = new PersonRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object, apiSettings);

            }

            [TestCleanup]
            public void Cleanup()
            {
                repository = null;
            }

            [TestMethod]
            public async Task GetPersonPinsTests()
            {
                string[] personGuids = new string[] { "a3c3c522-ddbf-46de-856b-3d72db515972", "b81b5dc0-428b-4c6e-a6fd-91e7c833b4ce" };

                Dictionary<string, GuidLookupResult> guidLookUpResults = new Dictionary<string,GuidLookupResult>();
                guidLookUpResults.Add("A", new GuidLookupResult() { Entity = "PERSON.PIN", PrimaryKey = "1" });
                guidLookUpResults.Add("B", new GuidLookupResult() { Entity = "PERSON.PIN", PrimaryKey = "2" });
                dataReaderMock.Setup(reader => reader.SelectAsync(It.IsAny < GuidLookup[]>())).ReturnsAsync(guidLookUpResults);
                
                Collection<DataContracts.PersonPin> personPinCollection = new Collection<DataContracts.PersonPin>();
                personPinCollection.Add(new DataContracts.PersonPin() { Recordkey = "1", PersonPinUserId = "personPinUserId1" });
                personPinCollection.Add(new DataContracts.PersonPin() { Recordkey = "2", PersonPinUserId = "personPinUserId2" });
                dataReaderMock.Setup(reader => reader.BulkReadRecordAsync<DataContracts.PersonPin>("PERSON.PIN", It.IsAny<string[]>(), true)).ReturnsAsync(personPinCollection);

                var actuals = await repository.GetPersonPinsAsync(personGuids);
                Assert.IsNotNull(actuals);
            }

            
        }


        #endregion

        #region SearchByName

        [TestClass]
        public class SearchByName : BaseRepositorySetup
        {
            PersonRepository personRepository;
            string quote;

            [TestInitialize]
            public void Initialize()
            {
                quote = '"'.ToString();

                // Build the test repository
                MockInitialize();

                personRepository = new PersonRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object, apiSettings);
            }

            [TestMethod]
            public async Task SearchByName_LastNameQuery()
            {
                string queryValue = null;
                var lastName = "brown";

                var lookupStringResponse = new GetPersonLookupStringResponse() { IndexString = ";PARTIAL.NAME.INDEX BROWN_", ErrorMessage = "" };
                transManagerMock.Setup(manager => manager
                        .ExecuteAsync<GetPersonLookupStringRequest, GetPersonLookupStringResponse>(It.IsAny<GetPersonLookupStringRequest>()))
                        .ReturnsAsync(lookupStringResponse);
                string[] people = new string[2] { "12345", "67890" };
                dataReaderMock.Setup(acc => acc.SelectAsync("PERSON", It.IsAny<string>())).Callback<string, string>((string s1, string s2) => queryValue = s2).ReturnsAsync(people);

                await personRepository.SearchByNameAsync(lastName);
                Assert.IsTrue(queryValue.Contains("WITH PARTIAL.NAME.INDEX EQ \"BROWN_\""));
            }

            [TestMethod]
            public async Task SearchByName_LastNameFirstNameMiddleNameQuery()
            {
                string queryValue = null;
                var lastName = "brown";
                var firstName = "jane";
                var middleName = "jubilee";
                var lookupStringResponse = new GetPersonLookupStringResponse() { IndexString = ";PARTIAL.NAME.INDEX BROWN_JN", ErrorMessage = "" };
                transManagerMock.Setup(manager => manager
                        .ExecuteAsync<GetPersonLookupStringRequest, GetPersonLookupStringResponse>(It.IsAny<GetPersonLookupStringRequest>()))
                        .ReturnsAsync(lookupStringResponse);
                string[] people = new string[2] { "12345", "67890" };
                dataReaderMock.Setup(acc => acc.SelectAsync("PERSON", It.IsAny<string>())).Callback<string, string>((string s1, string s2) => queryValue = s2).ReturnsAsync(people);
                await personRepository.SearchByNameAsync(lastName, firstName, middleName);
                Assert.IsTrue(queryValue.Contains("WITH PARTIAL.NAME.INDEX EQ \"BROWN_JN\""));
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task SearchByName_ThrowsErrorIfLastNameNull()
            {
                string lastName = null;
                await personRepository.SearchByNameAsync(lastName);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task SearchByName_ThrowsErrorIfLastNameBlank()
            {
                string lastName = "  ";
                await personRepository.SearchByNameAsync(lastName);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task SearchByName_ThrowsErrorIfLastNameEmpty()
            {
                string lastName = "";
                await personRepository.SearchByNameAsync(lastName);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task SearchByName_throwsErrorWhenLastNameLessThanTwoCharacters()
            {
                await personRepository.SearchByNameAsync("x", "first", "middle");
            }
        }

        #endregion

        #region CreatePerson

        [TestClass]
        public class CreatePerson2 : BasePersonSetup
        {
            PersonRepository _repository;
            private Ellucian.Colleague.Domain.Base.Entities.PersonIntegration _person;
            private List<Ellucian.Colleague.Domain.Base.Entities.Address> _addresses;
            private List<Ellucian.Colleague.Domain.Base.Entities.Phone> _phones;

            [TestInitialize]
            public void Initialize()
            {
                // Initialize person setup and Mock framework
                PersonSetupInitialize();

                // setup person object
                _addresses = new List<Address>();
                _phones = new List<Phone>();
                _person = BasePersonSetup.GetTestPersonIntegrationDataEntities(out _addresses, out _phones);

                // Build the test repository
                _repository = new PersonRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object, apiSettings);
            }

            [TestMethod]
            public async Task PersonRepository_CreatePerson2Test()
            {
                await _repository.Create2Async(_person, _addresses, _phones);
                // verify the person repository's create method was called with a specific update request
                transManagerMock.Verify(tm => tm.ExecuteAsync<UpdatePersonIntegrationRequest, UpdatePersonIntegrationResponse>(It.Is<UpdatePersonIntegrationRequest>(u => BasePersonSetup.ComparePersonIntgUpdateRequest(u, _person, _addresses, _phones))));
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task CreatePersonNullPersonException()
            {
                await _repository.Create2Async(null, null, null);
            }

            [TestMethod]
            [ExpectedException(typeof(RepositoryException))]
            public async Task CreatePerson2WithErrorsException()
            {
                var error = new PersonIntgErrors() {ErrorCodes = "XX", ErrorMessages = "Error"};

                // Mock the call for creating a person
                transManagerMock.Setup<Task<UpdatePersonIntegrationResponse>>(
                    manager => manager.ExecuteAsync<UpdatePersonIntegrationRequest, UpdatePersonIntegrationResponse>(
                        It.IsAny<UpdatePersonIntegrationRequest>())
                    ).Returns<UpdatePersonIntegrationRequest>(request => Task.FromResult(new UpdatePersonIntegrationResponse()
                    {
                        PersonGuid = null,                  
                        PersonIntgErrors = new List<PersonIntgErrors>() { error }
                    }));

                await _repository.Create2Async(_person, _addresses, _phones);
            }
        }
        #endregion

        #region UpdatePerson

        [TestClass]
        public class UpdatePerson2 : BasePersonSetup
        {
            PersonRepository _repository;
            private Ellucian.Colleague.Domain.Base.Entities.PersonIntegration _person;
            private List<Ellucian.Colleague.Domain.Base.Entities.Address> _addresses;
            private List<Ellucian.Colleague.Domain.Base.Entities.Phone> _phones;

            [TestInitialize]
            public void Initialize()
            {
                // Initialize person setup and Mock framework
                PersonSetupInitialize();

                // setup person object
                _addresses = new List<Address>();
                _phones = new List<Phone>();
                _person = GetTestPersonIntegrationDataEntities(out _addresses, out _phones);
                _person.AddSocialMedia(new SocialMedia("FB", "http://www.facebook.com/jDoe", true));
                _person.AddSocialMedia(new SocialMedia("TW", "http://www.twitter.com/jDoe", false));

                // Build the test repository
                _repository = new PersonRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object, apiSettings);
            }

            [TestMethod]
            public async Task PersonRepository_Update2PersonTest()
            {
                await _repository.Update2Async(_person, _addresses, _phones);
                // verify the person repository's update method was called with a specific update request
                transManagerMock.Verify(tm => tm.ExecuteAsync<UpdatePersonIntegrationRequest, UpdatePersonIntegrationResponse>(It.Is<UpdatePersonIntegrationRequest>(u => ComparePersonIntgUpdateRequest(u, _person, _addresses, _phones))));
            }

            [TestMethod]
            public async Task PersonRepository_Update2PersonToCreateTest()
            {
                await _repository.Update2Async(_person, _addresses, _phones);

                // verify the person repository's create method was called with a specific update request
                transManagerMock.Verify(tm => tm.ExecuteAsync<UpdatePersonIntegrationRequest, UpdatePersonIntegrationResponse>(It.Is<UpdatePersonIntegrationRequest>(u => ComparePersonIntgUpdateRequest(u, _person, _addresses, _phones))));
        
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task PersonRepository_Update2PersonNullPersonException()
            {
                await _repository.Update2Async(null, null, null);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task PersonRepository_Update2PersonNullPersonGuidException()
            {
                var person = new PersonIntegration(null, "Test") {Guid = null};
                await _repository.Update2Async(person, null, null);
            }

            [TestMethod]
            [ExpectedException(typeof(RepositoryException))]
            public async Task PersonRepository_Update2PersonWithErrorsException()
            {
                // Mock the call for creating a person
                transManagerMock.Setup<Task<UpdatePersonIntegrationResponse>>(
                    manager => manager.ExecuteAsync<UpdatePersonIntegrationRequest, UpdatePersonIntegrationResponse>(
                        It.IsAny<UpdatePersonIntegrationRequest>())
                    ).Returns<UpdatePersonIntegrationRequest>(request => Task.FromResult(new UpdatePersonIntegrationResponse()
                    {
                        PersonGuid =null,
                        PersonIntgErrors = new List<PersonIntgErrors>() { new PersonIntgErrors() {  ErrorCodes = "XX", ErrorMessages = "Error"} }

                    }));

                await _repository.Update2Async(_person, _addresses, _phones);
            }
        }
        #endregion

        #region IsPerson

        [TestClass]
        public class IsPerson : BasePersonSetup
        {
            PersonRepository repository;

            [TestInitialize]
            public void Initialize()
            {
                // Initialize person setup and Mock framework
                PersonSetupInitialize();

                // Build the test repository
                repository = new PersonRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object, apiSettings);
            }

            [TestMethod]
            public async Task IsPersonTrueTest()
            {
                dataReaderMock.Setup(accessor => accessor.SelectAsync("PERSON", It.IsAny<string[]>(), It.IsAny<string>())).ReturnsAsync(new string[] { "0004199" });
                var result = await repository.IsPersonAsync("0004199");
                Assert.AreEqual(true, result);
            }

            [TestMethod]
            public async Task IsPersonFalseEmptyArrayTest()
            {
                dataReaderMock.Setup(accessor => accessor.SelectAsync("PERSON", It.IsAny<string[]>(), It.IsAny<string>())).ReturnsAsync(new string[] { });
                var result = await repository.IsPersonAsync("0004199");
                Assert.AreEqual(false, result);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task IsPersonNullPersonException()
            {
                string personId = null;
                await repository.IsPersonAsync(personId);
            }
        }

        #endregion

        #region IsFacultyPerson

        [TestClass]
        public class IsFacultyPerson : BasePersonSetup
        {
            PersonRepository repository;

            [TestInitialize]
            public void Initialize()
            {
                // Initialize person setup and Mock framework
                PersonSetupInitialize();

                // Build the test repository
                repository = new PersonRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object, apiSettings);
            }

            [TestMethod]
            public async Task IsFacultyPersonTrueTest()
            {
                dataReaderMock.Setup(accessor => accessor.SelectAsync("FACULTY", It.IsAny<string[]>(), It.IsAny<string>())).ReturnsAsync(new string[] { "0004199" });
                var result = await repository.IsFacultyAsync("0004199");
                Assert.AreEqual(true, result);
            }

            [TestMethod]
            public async Task IsFacultyPersonFalseEmptyArrayTest()
            {
                dataReaderMock.Setup(accessor => accessor.Select("FACULTY", It.IsAny<string[]>(), It.IsAny<string>())).Returns(new string[] { });
                var result = await repository.IsFacultyAsync("0004199");
                Assert.AreEqual(false, result);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task IsFacultyPersonNullPersonException()
            {
                string personId = null;
                await repository.IsFacultyAsync(personId);
            }
        }

        #endregion

        #region IsStudentPerson

        [TestClass]
        public class IsStudentPerson : BasePersonSetup
        {
            PersonRepository repository;

            [TestInitialize]
            public void Initialize()
            {
                // Initialize person setup and Mock framework
                PersonSetupInitialize();

                // Build the test repository
                repository = new PersonRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object, apiSettings);
            }

            [TestMethod]
            public async Task IsStudentPersonTrueTest()
            {
                dataReaderMock.Setup(accessor => accessor.SelectAsync("STUDENTS", It.IsAny<string[]>(), It.IsAny<string>())).ReturnsAsync(new string[] { "0004199" });
                var result = await repository.IsStudentAsync("0004199");
                Assert.AreEqual(true, result);
            }

            [TestMethod]
            public async Task IsStudentPersonFalseEmptyArrayTest()
            {
                dataReaderMock.Setup(accessor => accessor.SelectAsync("STUDENTS", It.IsAny<string[]>(), It.IsAny<string>())).ReturnsAsync(new string[] { });
                var result = await repository.IsStudentAsync("0004199");
                Assert.AreEqual(false, result);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task IsStudentPersonNullPersonException()
            {
                string personId = null;
                await repository.IsStudentAsync(personId);
            }
        }

        #endregion

        #region GetFacultyPersonGuids

        [TestClass]
        public class GetFacultyPersonGuids : BasePersonSetup
        {
            PersonRepository repository;

            [TestInitialize]
            public void Initialize()
            {
                // Initialize person setup and Mock framework
                PersonSetupInitialize();

                // Build the test repository
                repository = new PersonRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object, apiSettings);
            }

            [TestMethod]
            public async Task GetFacultyPersonGuidsTest()
            {
                string facultyId1 = "0004199";
                string facultyGuid1 = "d9f37b0e-dcd6-45ba-bafe-ab5a27884f2a";
                string facultyId2 = "0004198";
                string facultyGuid2 = "457732d2-a5cb-4f6a-90a2-462a2e14adbb";
                string facultyId3 = "0004197";
                string facultyGuid3 = "9f035882-e576-479b-9368-e53a25574906";
                dataReaderMock.Setup(accessor => accessor.SelectAsync("FACULTY", "")).ReturnsAsync(new string[] { facultyId1, facultyId2, facultyId3 });
                dataReaderMock.Setup(acc => acc.SelectAsync(It.IsAny<RecordKeyLookup[]>())
                    ).Returns<RecordKeyLookup[]>(lookup =>
                    {
                        var results = new Dictionary<string, RecordKeyLookupResult>();
                        results.Add(facultyId1, new RecordKeyLookupResult() { Guid = facultyGuid1 });
                        results.Add(facultyId2, new RecordKeyLookupResult() { Guid = facultyGuid2 });
                        results.Add(facultyId3, new RecordKeyLookupResult() { Guid = facultyGuid3 });
                        return Task.FromResult(results);
                    });
                var facultyGuidsTuple = await repository.GetFacultyPersonGuidsAsync(0, 3);
                var facultyGuids = facultyGuidsTuple.Item1;
                Assert.AreEqual(3, facultyGuids.Count());
                Assert.AreEqual(true, facultyGuids.Contains(facultyGuid1));
                Assert.AreEqual(true, facultyGuids.Contains(facultyGuid2));
                Assert.AreEqual(true, facultyGuids.Contains(facultyGuid3));
            }

            [TestMethod]
            public async Task GetFacultyPersonGuidsNoFacultyTest()
            {
                dataReaderMock.Setup(accessor => accessor.Select("FACULTY", "")).Returns(new string[] { });
                var facultyGuidsTuple = await repository.GetFacultyPersonGuidsAsync(It.IsAny<int>(), It.IsAny<int>());
                Assert.AreEqual(0, facultyGuidsTuple.Item1.Count());
            }
        }

        #endregion

        #region GetMatchingPersons

        [TestClass]
        public class GetMatchingPersons : BasePersonSetup
        {
            PersonRepository repository;
            private Ellucian.Colleague.Domain.Base.Entities.Person person;

            [TestInitialize]
            public void Initialize()
            {
                // Initialize person setup and Mock framework
                PersonSetupInitialize();

                // setup person object
                person = BasePersonSetup.GetTestPersonEntity();

                // Build the test repository
                repository = new PersonRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object, apiSettings);
            }

            private static bool CompareMatchRequest(GetPersonMatchRequest request, Person person)
            {
                if (request.PersonId != person.Id) return false;
                if (request.FirstName != person.FirstName) return false;
                if (request.MiddleName != person.MiddleName) return false;
                if (request.LastName != person.LastName) return false;
                if (request.BirthDate != person.BirthDate) return false;
                if (request.Gender != person.Gender) return false;
                if (request.Ssn != person.GovernmentId) return false;
                if (request.EmailAddress != person.EmailAddresses.FirstOrDefault().Value) return false;
                return true;
            }

            [TestMethod]
            public async Task GetMatchingPersonsTest()
            {
                string personMatchGuid = "d9f37b0e-dcd6-45ba-bafe-ab5a27884f2a";

                // Mock the call for getting a person match
                transManagerMock.Setup<Task<GetPersonMatchResponse>>(
                    manager => manager.ExecuteAsync<GetPersonMatchRequest, GetPersonMatchResponse>(
                        It.IsAny<GetPersonMatchRequest>())
                    ).Returns<GetPersonMatchRequest>(request =>
                    {
                        return Task.FromResult(new GetPersonMatchResponse()
                        {
                            PersonMatchingGuids = new List<string>() { personMatchGuid }
                        });
                    });

                var results = await repository.GetMatchingPersonsAsync(person);
                Assert.AreEqual(personMatchGuid, results.FirstOrDefault());
                // verify the person repository's create method was called with a specific update request
                transManagerMock.Verify(tm => tm.ExecuteAsync<GetPersonMatchRequest, GetPersonMatchResponse>(It.Is<GetPersonMatchRequest>(r => CompareMatchRequest(r, person))));
            }

            [TestMethod]
            [ExpectedException(typeof(InvalidOperationException))]
            public async Task GetMatchingPersonsErrorsException()
            {
                // Mock the call for getting a person match
                transManagerMock.Setup<Task<GetPersonMatchResponse>>(
                    manager => manager.ExecuteAsync<GetPersonMatchRequest, GetPersonMatchResponse>(
                        It.IsAny<GetPersonMatchRequest>())
                    ).Returns<GetPersonMatchRequest>(request =>
                    {
                        return Task.FromResult(new GetPersonMatchResponse()
                        {
                            PersonMatchingGuids = new List<string>() { },
                            ErrorMessages = new List<string>() { "Error occurred" }
                        });
                    });

                await repository.GetMatchingPersonsAsync(person);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task GetMatchingNullPersonException()
            {
                await repository.GetMatchingPersonsAsync(null);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task GetMatchingNullPersonFirstNameException()
            {
                person.FirstName = null;
                await repository.GetMatchingPersonsAsync(null);
            }
        }

        #endregion

        #region GetPersonIntegrationData

        [TestClass]
        public class GetPersonIntegrationData : BasePersonSetup
        {
            PersonRepository repository;
            private Ellucian.Colleague.Domain.Base.Entities.Person person;
            private List<Ellucian.Colleague.Domain.Base.Entities.Address> addresses;
            private List<Ellucian.Colleague.Domain.Base.Entities.Phone> phones;
            private List<Ellucian.Colleague.Domain.Base.Entities.SocialMedia> socialMedias;

            [TestInitialize]
            public void Initialize()
            {
                // Initialize person setup and Mock framework
                PersonSetupInitialize();

                // setup person object
                addresses = new List<Address>();
                phones = new List<Phone>();
                person = GetTestPersonDataEntities(out addresses, out phones);

                // Build the test repository
                repository = new PersonRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object, apiSettings);
            }

            private GetPersonAddressesResponse ConvertEntitiesToPersonAddressesResponse()
            {
                var response = new GetPersonAddressesResponse();
                for (int i = 0; i < phones.Count(); i++)
                {
                    response.PersonPhones.Add(new PersonPhones()
                    {
                        PhoneType = phones[i].TypeCode,
                        PhoneNumber = phones[i].Number,
                        PhoneExtension = phones[i].Extension
                    });
                }
                for (int i = 0; i < person.EmailAddresses.Count(); i++)
                {
                    response.PersonEmailAddresses.Add(new PersonEmailAddresses()
                    {
                        EmailAddressValue = person.EmailAddresses[i].Value,
                        EmailAddressType = person.EmailAddresses[i].TypeCode
                    });
                }
                for (int i = 0; i < addresses.Count(); i++)
                {
                    var personAddress = new PersonAddresses();
                    personAddress.AddressType = addresses[i].Type;
                    personAddress.AddressCity = addresses[i].City;
                    personAddress.AddressCountry = addresses[i].Country;
                    personAddress.AddressCounty = addresses[i].County;
                    personAddress.AddressPostalCode = addresses[i].PostalCode;
                    personAddress.AddressRegion = addresses[i].State;
                    if (addresses[i].AddressLines != null && addresses[i].AddressLines.Count() > 0)
                    {
                        personAddress.AddressStreet1 = addresses[i].AddressLines[0];
                        if (addresses[i].AddressLines.Count() > 1)
                        {
                            personAddress.AddressStreet2 = addresses[i].AddressLines[1];
                        }
                        if (addresses[i].AddressLines.Count() > 2)
                        {
                            personAddress.AddressStreet3 = addresses[i].AddressLines[2];
                        }
                    }
                    response.PersonAddresses.Add(personAddress);
                }
                return response;
            }

            private bool CompareIntegrationResponseToPersonEntities(List<Ellucian.Colleague.Domain.Base.Entities.Address> personAddresses,
                List<Ellucian.Colleague.Domain.Base.Entities.EmailAddress> personEmailAddresses,
                List<Ellucian.Colleague.Domain.Base.Entities.Phone> personPhones)
            {
                for (int i = 0; i < phones.Count(); i++)
                {
                    Assert.AreEqual(true, phones[i].TypeCode == personPhones[i].TypeCode);
                    Assert.AreEqual(true, phones[i].Number == personPhones[i].Number);
                    Assert.AreEqual(true, phones[i].Extension == personPhones[i].Extension);
                }
                for (int i = 0; i < person.EmailAddresses.Count(); i++)
                {
                    Assert.AreEqual(true, person.EmailAddresses[i].Value == personEmailAddresses[i].Value);
                    Assert.AreEqual(true, person.EmailAddresses[i].TypeCode == personEmailAddresses[i].TypeCode);
                }
                for (int i = 0; i < addresses.Count(); i++)
                {
                    Assert.AreEqual(true, addresses[i].Type == personAddresses[i].Type);
                    Assert.AreEqual(true, addresses[i].City == personAddresses[i].City);
                    Assert.AreEqual(true, addresses[i].County == personAddresses[i].County);
                    Assert.AreEqual(true, addresses[i].Country == personAddresses[i].Country);
                    Assert.AreEqual(true, addresses[i].PostalCode == personAddresses[i].PostalCode);
                    Assert.AreEqual(true, addresses[i].State == personAddresses[i].State);
                    if (addresses[i].AddressLines != null && addresses[i].AddressLines.Count() > 0)
                    {
                        Assert.AreEqual(true, addresses[i].AddressLines[0] == personAddresses[i].AddressLines[0]);
                        if (addresses[i].AddressLines.Count() > 1)
                        {
                            Assert.AreEqual(true, addresses[i].AddressLines[1] == personAddresses[i].AddressLines[1]);
                        }
                        if (addresses[i].AddressLines.Count() > 2)
                        {
                            Assert.AreEqual(true, addresses[i].AddressLines[2] == personAddresses[i].AddressLines[2]);
                        }
                    }
                }
                return true;
            }

            [TestMethod]
            public async Task GetPersonIntegrationDataTest()
            {
                // Mock the call for getting person integration data
                transManagerMock.Setup<Task<GetPersonAddressesResponse>>(
                    manager => manager.ExecuteAsync<GetPersonAddressesRequest, GetPersonAddressesResponse>(
                        It.IsAny<GetPersonAddressesRequest>())
                    ).Returns<GetPersonAddressesRequest>(request =>
                    {
                        return Task.FromResult(ConvertEntitiesToPersonAddressesResponse());
                    });

                List<Ellucian.Colleague.Domain.Base.Entities.EmailAddress> personEmailAddresses;
                List<Ellucian.Colleague.Domain.Base.Entities.Phone> personPhones;
                List<Ellucian.Colleague.Domain.Base.Entities.Address> personAddresses;
                var result = await repository.GetPersonIntegrationDataAsync("0004199");
                personEmailAddresses = result.Item1;
                personPhones = result.Item2;
                personAddresses = result.Item3;
                bool success = result.Item4;
                Assert.AreEqual(true, success);
                Assert.AreEqual(true, CompareIntegrationResponseToPersonEntities(personAddresses, personEmailAddresses, personPhones));
            }

            [TestMethod]
            public async Task GetPersonIntegrationDataNullAddressesDataTest()
            {
                // Mock the call for getting person integration data
                transManagerMock.Setup<Task<GetPersonAddressesResponse>>(
                    manager => manager.ExecuteAsync<GetPersonAddressesRequest, GetPersonAddressesResponse>(
                        It.IsAny<GetPersonAddressesRequest>())
                    ).Returns<GetPersonAddressesRequest>(request =>
                    {
                        return Task.FromResult(new GetPersonAddressesResponse());
                    });

                List<Ellucian.Colleague.Domain.Base.Entities.EmailAddress> personEmailAddresses;
                List<Ellucian.Colleague.Domain.Base.Entities.Phone> personPhones;
                List<Ellucian.Colleague.Domain.Base.Entities.Address> personAddresses;
                var result = await repository.GetPersonIntegrationDataAsync("0004199");
                personEmailAddresses = result.Item1;
                personPhones = result.Item2;
                personAddresses = result.Item3;
                bool success = result.Item4;
                Assert.AreEqual(true, success);
                Assert.AreEqual(0, personAddresses.Count());
                Assert.AreEqual(0, personEmailAddresses.Count());
                Assert.AreEqual(0, personPhones.Count());
            }

            [TestMethod]
            [ExpectedException(typeof(InvalidOperationException))]
            public async Task GetPersonIntegrationDataErrorMessageException()
            {
                // Mock the call for getting person integration data
                transManagerMock.Setup<Task<GetPersonAddressesResponse>>(
                    manager => manager.ExecuteAsync<GetPersonAddressesRequest, GetPersonAddressesResponse>(
                        It.IsAny<GetPersonAddressesRequest>())
                    ).Returns<GetPersonAddressesRequest>(request =>
                    {
                        return Task.FromResult(new GetPersonAddressesResponse()
                        {
                            ErrorMessages = new List<string>() { "Error occurred" }
                        });
                    });

                List<Ellucian.Colleague.Domain.Base.Entities.EmailAddress> personEmailAddresses;
                List<Ellucian.Colleague.Domain.Base.Entities.Phone> personPhones;
                List<Ellucian.Colleague.Domain.Base.Entities.Address> personAddresses;
                var result = await repository.GetPersonIntegrationDataAsync("0004199");
                personEmailAddresses = result.Item1;
                personPhones = result.Item2;
                personAddresses = result.Item3;
                bool success = result.Item4;
            }
        }

        #endregion

        #region GetPersons
        [TestClass]
        public class GetPersons : BasePersonSetup
        {
            PersonRepository personRepository;

            [TestInitialize]
            public void Initialize()
            {
                PersonSetupInitialize();
                // Build the test repository
                personRepository = new PersonRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object, apiSettings);
            }

            [TestCleanup]
            public void Cleanup()
            {
                personRepository = null;
            }

            /// <summary>
            /// Tests if ArgumentNullException is thrown when the passed list of ids is empty
            /// </summary>
            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task EmptyIdsList_ExceptionThrownTest()
            {
                await personRepository.GetPersonsAsync(new List<string>(), person => new Staff(person.Recordkey, person.LastName));
            }

            /// <summary>
            /// Tests if ArgumentNullException is thrown when the passed list is null
            /// </summary>
            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task NullIdsList_ExceptionThrownTest()
            {
                await personRepository.GetPersonsAsync(null, person => new Staff(person.Recordkey, person.LastName));
            }

            /// <summary>
            /// Tests if ArgumentOutOfRangeException is thrown if no records were returned from BulkRead
            /// </summary>            
            [TestMethod]
            [ExpectedException(typeof(ArgumentOutOfRangeException))]
            public async Task NoRecordsReturned_ExceptionThrownTest()
            {
                dataReaderMock.Setup<Task<Collection<DataContracts.Person>>>(accessor => accessor.BulkReadRecordAsync<DataContracts.Person>(It.IsAny<string[]>(), true))
                .ReturnsAsync(new Collection<DataContracts.Person>());

                await personRepository.GetPersonsAsync(personRecords.Keys, person => new Staff(person.Recordkey, person.LastName));
            }

            /// <summary>
            /// Tests if ArgumentOutOfRangeException is thrown if null was returned from BulkRead
            /// </summary>            
            [TestMethod]
            [ExpectedException(typeof(ArgumentOutOfRangeException))]
            public async Task NullRecordsReturned_ExceptionThrownTest()
            {
                dataReaderMock.Setup<Task<Collection<DataContracts.Person>>>(accessor => accessor.BulkReadRecordAsync<DataContracts.Person>(It.IsAny<string[]>(), true))
                .ReturnsAsync(null);

                await personRepository.GetPersonsAsync(personRecords.Keys, person => new Staff(person.Recordkey, person.LastName));
            }

            /// <summary>
            /// Tests if the number of returned person entities equal the expected number of records
            /// </summary>            
            [TestMethod]
            public async Task NumberOfRecordsReturned_EqualsExpectedTest()
            {
                var expectedNumberOfRecords = personRecords.Count;
                IEnumerable<Person> actualRecords = await personRepository.GetPersonsAsync(personRecords.Keys, person => new Staff(person.Recordkey, person.LastName));
                Assert.AreEqual(expectedNumberOfRecords, actualRecords.Count());
            }

            /// <summary>
            /// Tests if the cache is used when appropriate flag value(true) is passed
            /// </summary>            
            [TestMethod]
            public async Task GetPersonsAsync_UseCacheTrueTest()
            {
                var records = new Collection<DataContracts.Person>();
                foreach (var personItem in personRecords)
                {
                    records.Add(personItem.Value);                    
                }
                dataReaderMock.Setup<Task<Collection<DataContracts.Person>>>(acc => acc.BulkReadRecordAsync<DataContracts.Person>(It.IsAny<string[]>(), true)).ReturnsAsync(records);
                IEnumerable<Person> persons = await personRepository.GetPersonsAsync(personRecords.Keys, person => new Staff(person.Recordkey, person.LastName));

                foreach (var person in persons)
                {
                    var expectedRecord = records.FirstOrDefault(r => r.Recordkey == person.Id);
                    Assert.AreEqual(expectedRecord.LastName, person.LastName);
                    Assert.AreEqual(expectedRecord.FirstName, person.FirstName);
                    var cacheKey = personRepository.BuildFullCacheKey(person.Id);
                    cacheProviderMock.Verify(m => m.Contains(cacheKey, null));    
                }
                            
            }

            /// <summary>
            /// Tests if AddOrUpdateCache is used when appropriate flag value(false) is passed
            /// </summary>            
            [TestMethod]
            public async Task GetPersonsAsync_UseCacheFalseTest()
            {
                var records = new Collection<DataContracts.Person>();
                foreach (var personItem in personRecords)
                {
                    records.Add(personItem.Value);
                }
                dataReaderMock.Setup<Task<Collection<DataContracts.Person>>>(acc => acc.BulkReadRecordAsync<DataContracts.Person>(It.IsAny<string[]>(), true)).ReturnsAsync(records);
                IEnumerable<Person> persons = await personRepository.GetPersonsAsync(personRecords.Keys, person => new Staff(person.Recordkey, person.LastName), false);

                foreach (var person in persons)
                {
                    var expectedRecord = records.FirstOrDefault(r => r.Recordkey == person.Id);
                    Assert.AreEqual(expectedRecord.LastName, person.LastName);
                    Assert.AreEqual(expectedRecord.FirstName, person.FirstName);
                    var cacheKey = personRepository.BuildFullCacheKey(person.Id);
                    cacheProviderMock.Verify(m => m.AddAndUnlockSemaphore(cacheKey, person, It.IsAny<SemaphoreSlim>(), It.IsAny<CacheItemPolicy>(), null));
                }
                
            }

            /// <summary>
            /// Tests if the person data is being retrieved from cache
            /// </summary>
            /// <returns></returns>
            [TestMethod]
            public async Task GetPersons_CacheGetMethodCalledTest()
            {
                // Set up local cache mock to respond to cache request:
                //  -to "Contains" request, return "true" to indicate item is in cache
                //  -to "Get" request, return the cache item 
                cacheProviderMock.Setup(x => x.Contains(It.IsAny<string>(), null)).Returns(true);

                cacheProviderMock.Setup(x => x.Get(It.IsAny<string>(), null)).Returns<string, string>((cacheKey, str) =>
                {
                    DataContracts.Person response = null;
                    //Get the id from the cacheKey - everything after "_"
                    var id = cacheKey.Substring(cacheKey.LastIndexOf("_") + 1);                                                                        
                    var personDataContract = personRecords.Values.Where(pr => pr.Recordkey == id).FirstOrDefault();
                    if (personDataContract != null)
                    {
                        response = (personRecords.TryGetValue(personDataContract.Recordkey, out response)) ? response : null;
                    }
                    return new Person(response.Recordkey, response.LastName);
                })
                .Verifiable();

                // return null for request, so that if we have a result, it wasn't the data accessor that returned it.                
                dataReaderMock.Setup<Task<Collection<DataContracts.Person>>>(acc => acc.BulkReadRecordAsync<DataContracts.Person>(It.IsAny<string>(), It.IsAny<string>(), true)).Returns(Task.FromResult<Collection<DataContracts.Person>>(new Collection<DataContracts.Person>()));

                var persons = await personRepository.GetPersonsAsync(personRecords.Keys, person => new Person(person.Recordkey, person.LastName));
                foreach (var person in persons)
                {
                    string cacheKey = personRepository.BuildFullCacheKey(person.Id);
                    // Verify that Get was called to get the persons from cache
                    cacheProviderMock.Verify(m => m.Get(cacheKey, null));

                    //Verify the returned person information matches expected
                    var expectedRecord = personRecords.Values.Where(pr => pr.Recordkey == person.Id).FirstOrDefault();
                    Assert.IsNotNull(expectedRecord);
                    Assert.AreEqual(expectedRecord.LastName, person.LastName);                    
                }
                
            }
        }

        #endregion

        #region GetMatchingPersonResultsAsync

        [TestClass]
        public class GetMatchingPersonResultsAsync : BasePersonSetup
        {
            PersonRepository repository;
            PersonMatchCriteria criteria;
            GetPersonMatchResultsResponse response;
            private Ellucian.Colleague.Domain.Base.Entities.Person person;

            [TestInitialize]
            public void Initialize()
            {
                // Initialize person setup and Mock framework
                PersonSetupInitialize();

                // setup person object
                person = BasePersonSetup.GetTestPersonEntity();
                criteria = new PersonMatchCriteria("PROXY.PERSON", new List<PersonName>() { new PersonName("First", null, "Last") });

                // Build the test repository
                repository = new PersonRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object, apiSettings);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task GetMatchingPersonResultsAsync_NullCriteria()
            {
                var results = await repository.GetMatchingPersonResultsAsync(null);
            }

            [TestMethod]
            [ExpectedException(typeof(InvalidOperationException))]
            public async Task GetMatchingPersonResultsAsync_Errors()
            {
                transManagerMock.Setup<Task<GetPersonMatchResultsResponse>>(
                    manager => manager.ExecuteAsync<GetPersonMatchResultsRequest, GetPersonMatchResultsResponse>(
                        It.IsAny<GetPersonMatchResultsRequest>())
                    ).Returns<GetPersonMatchResultsRequest>(request =>
                    {
                        return Task.FromResult(new GetPersonMatchResultsResponse()
                        {
                            ErrorMessages = new List<string>() { "An error occurred." },
                            MatchResults = new List<MatchResults>()
                        });
                    });

                repository = new PersonRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object, apiSettings);

                var results = await repository.GetMatchingPersonResultsAsync(criteria);
            }

            [TestMethod]
            public async Task GetMatchingPersonResultsAsync_Valid()
            {
                transManagerMock.Setup<Task<GetPersonMatchResultsResponse>>(
                    manager => manager.ExecuteAsync<GetPersonMatchResultsRequest, GetPersonMatchResultsResponse>(
                        It.IsAny<GetPersonMatchResultsRequest>())
                    ).Returns<GetPersonMatchResultsRequest>(request =>
                    {
                        return Task.FromResult(new GetPersonMatchResultsResponse()
                        {
                            MatchResults = new List<MatchResults>()
                            {
                                new MatchResults()
                                {
                                    MatchCategories = "D",
                                    MatchPersonIds = "0003315",
                                    MatchScores = 100
                                }
                            }
                        });
                    });

                repository = new PersonRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object, apiSettings);

                var results = await repository.GetMatchingPersonResultsAsync(criteria);
                Assert.AreEqual(1, results.Count());
            }

            [TestMethod]
            [ExpectedException(typeof(InvalidOperationException))]
            public async Task GetMatchingPersonResultsAsync_NullResponse()
            {
                transManagerMock.Setup<Task<GetPersonMatchResultsResponse>>(
                    manager => manager.ExecuteAsync<GetPersonMatchResultsRequest, GetPersonMatchResultsResponse>(
                        It.IsAny<GetPersonMatchResultsRequest>())
                    ).Returns<GetPersonMatchResultsRequest>(request =>
                    {
                        return Task.FromResult(response);
                    });

                repository = new PersonRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object, apiSettings);

                var results = await repository.GetMatchingPersonResultsAsync(criteria);
                Assert.AreEqual(0, results.Count());
            }

            [TestMethod]
            public async Task GetMatchingPersonResultsAsync_NullMatchResults()
            {
                transManagerMock.Setup<Task<GetPersonMatchResultsResponse>>(
                    manager => manager.ExecuteAsync<GetPersonMatchResultsRequest, GetPersonMatchResultsResponse>(
                        It.IsAny<GetPersonMatchResultsRequest>())
                    ).Returns<GetPersonMatchResultsRequest>(request =>
                    {
                        return Task.FromResult(new GetPersonMatchResultsResponse()
                        {
                            MatchResults = null
                        });
                    });

                repository = new PersonRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object, apiSettings);

                var results = await repository.GetMatchingPersonResultsAsync(criteria);
                Assert.AreEqual(0, results.Count());
            }

            [TestMethod]
            public async Task GetMatchingPersonResultsAsync_NoMatchResults()
            {
                transManagerMock.Setup<Task<GetPersonMatchResultsResponse>>(
                    manager => manager.ExecuteAsync<GetPersonMatchResultsRequest, GetPersonMatchResultsResponse>(
                        It.IsAny<GetPersonMatchResultsRequest>())
                    ).Returns<GetPersonMatchResultsRequest>(request =>
                    {
                        return Task.FromResult(new GetPersonMatchResultsResponse()
                        {
                            MatchResults = new List<MatchResults>()
                        });
                    });

                repository = new PersonRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object, apiSettings);

                var results = await repository.GetMatchingPersonResultsAsync(criteria);
                Assert.AreEqual(0, results.Count());
            }
        }

        #endregion  

        #region Create Organization

         
        [TestClass]
        public class CreateOrganization : BasePersonSetup
        {
            PersonRepository _repository;
            private Ellucian.Colleague.Domain.Base.Entities.PersonIntegration _person;
            private List<Ellucian.Colleague.Domain.Base.Entities.Address> _addresses;
            private List<Ellucian.Colleague.Domain.Base.Entities.Phone> _phones;

            [TestInitialize]
            public void Initialize()
            {
                // Initialize person setup and Mock framework
                PersonSetupInitialize();

                // setup person object
                _addresses = new List<Address>();
                _phones = new List<Phone>();
                _person = BasePersonSetup.GetTestPersonIntegrationDataEntities(out _addresses, out _phones);

                // Build the test repository
                _repository = new PersonRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object, apiSettings);
            }

            [TestMethod]
            public async Task PersonRepository_CreateOrganizationTest_withoutAddress()
            {
                _addresses = null;

                await _repository.CreateOrganizationAsync(_person, _addresses, _phones);
                // verify the person repository's create method was called with a specific update request
                transManagerMock.Verify(tm => tm.ExecuteAsync<UpdateOrganizationIntegrationRequest, 
                    UpdateOrganizationIntegrationResponse>(It.Is<UpdateOrganizationIntegrationRequest>(u => 
                        BasePersonSetup.CompareOrgIntgUpdateRequest(u, _person, _addresses, _phones))));
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task CreateOrganizationNullPersonException()
            {
                await _repository.CreateOrganizationAsync(null, null, null);
            }

            [TestMethod]
            [ExpectedException(typeof(RepositoryException))]
            public async Task CreateOrganizationWithErrorsException()
            {
                var error = new OrgIntgErrors() { ErrorCodes = "XX", ErrorMessages = "Error" };

                // Mock the call for creating a person
                transManagerMock.Setup(
                    manager => manager.ExecuteAsync<UpdateOrganizationIntegrationRequest, UpdateOrganizationIntegrationResponse>(
                        It.IsAny<UpdateOrganizationIntegrationRequest>())
                    ).Returns<UpdateOrganizationIntegrationRequest>(request => Task.FromResult(new UpdateOrganizationIntegrationResponse()
                    {
                        OrgGuid = string.Empty,
                        OrgIntgErrors = new List<OrgIntgErrors>() { error }
                    }));

                await _repository.CreateOrganizationAsync(_person, _addresses, _phones);
            }
        }
        #endregion

        #region Update Organization

        [TestClass]
        public class UpdateOrganization : BasePersonSetup
        {
            PersonRepository _repository;
            private Ellucian.Colleague.Domain.Base.Entities.PersonIntegration _person;
            private List<Ellucian.Colleague.Domain.Base.Entities.Address> _addresses;
            private List<Ellucian.Colleague.Domain.Base.Entities.Phone> _phones;

            [TestInitialize]
            public void Initialize()
            {
                // Initialize person setup and Mock framework
                PersonSetupInitialize();

                // setup person object
                _addresses = new List<Address>();
                _phones = new List<Phone>();
                _person = GetTestPersonIntegrationDataEntities(out _addresses, out _phones);

                // Build the test repository
                _repository = new PersonRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object, apiSettings);
            }

            [TestMethod]
            public async Task PersonRepository_UpdateOrganizationTest_withoutAddress()
            {
                _addresses = null;

                await _repository.UpdateOrganizationAsync(_person, _addresses, _phones);
                // verify the person repository's update method was called with a specific update request
                transManagerMock.Verify(tm => tm.ExecuteAsync<UpdateOrganizationIntegrationRequest,
                    UpdateOrganizationIntegrationResponse>(It.Is<UpdateOrganizationIntegrationRequest>(u =>
                        CompareOrgIntgUpdateRequest(u, _person, _addresses, _phones))));
            }

            [TestMethod]
            public async Task PersonRepository_UpdateOrganizationToCreateTest_withoutAddress()
            {
               _addresses = null;

                await _repository.UpdateOrganizationAsync(_person, _addresses, _phones);

                // verify the person repository's create method was called with a specific update request
                transManagerMock.Verify(tm => tm.ExecuteAsync<UpdateOrganizationIntegrationRequest,
                    UpdateOrganizationIntegrationResponse>(It.Is<UpdateOrganizationIntegrationRequest>(u => 
                        CompareOrgIntgUpdateRequest(u, _person, _addresses, _phones))));

            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task PersonRepository_UpdateOrganizationNullPersonException()
            {
                await _repository.UpdateOrganizationAsync(null, null, null);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task PersonRepository_UpdateOrganizationNullPersonGuidException()
            {
                var personIntg = new PersonIntegration(null, "Test") { Guid = null };
                await _repository.UpdateOrganizationAsync(personIntg, null, null);
            }

            [TestMethod]
            [ExpectedException(typeof(RepositoryException))]
            public async Task PersonRepository_UpdateOrganizationWithErrorsException()
            {
                // Mock the call for creating a person
                transManagerMock.Setup(
                    manager => manager.ExecuteAsync<UpdateOrganizationIntegrationRequest, UpdateOrganizationIntegrationResponse>(
                        It.IsAny<UpdateOrganizationIntegrationRequest>())
                    ).Returns<UpdateOrganizationIntegrationRequest>(request => Task.FromResult(new UpdateOrganizationIntegrationResponse()
                    {
                        OrgGuid = null,
                        OrgIntgErrors = new List<OrgIntgErrors>()
                        {
                            new OrgIntgErrors() { ErrorCodes = "XX", ErrorMessages = "Error" }
                        }

                    }));

                await _repository.UpdateOrganizationAsync(_person, _addresses, _phones);
            }
        }
        #endregion
    }
}