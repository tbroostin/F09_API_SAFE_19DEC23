// Copyright 2015-2018 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Ellucian.Colleague.Data.Base.Repositories;
using Ellucian.Colleague.Data.Base.Transactions;
using Ellucian.Colleague.Domain.Base.Entities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Ellucian.Data.Colleague;
using System.Collections.ObjectModel;
using Ellucian.Colleague.Domain.Exceptions;

namespace Ellucian.Colleague.Data.Base.Tests.Repositories
{
    [TestClass]
    public class PersonBaseRepositoryTests
    {
        [TestClass]
        public class GetPersonBaseAsync : BasePersonSetup
        {
            PersonBaseRepository repository;

            [TestInitialize]
            public void Initialize()
            {
                // Initialize person setup and Mock framework
                PersonSetupInitialize();

                // Build the test repository
                repository = new PersonBaseRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object, apiSettings);

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
                await repository.GetPersonBaseAsync(string.Empty);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentOutOfRangeException))]
            public async Task GetBasePersonAsync_InvalidIdException()
            {
                await repository.GetPersonBaseAsync("123");
            }

            [TestMethod]
            public async Task GetPersonBaseAsync_GetCachedIsTrue()
            {
                foreach (var personItem in personRecords)
                {
                    string personId = personItem.Key;
                    DataContracts.Person record = personItem.Value;
                    dataReaderMock.Setup(acc => acc.ReadRecordAsync<DataContracts.Person>("PERSON", record.Recordkey, true)).ReturnsAsync(record);
                    PersonBase result = await repository.GetPersonBaseAsync(personId);
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
                    PersonBase result = await repository.GetPersonBaseAsync(personId, false);
                    // Spot check a couple of attribues: All other attributes are tested by the Get<> method tests       
                    Assert.AreEqual(record.Recordkey, result.Id);
                    Assert.AreEqual(record.LastName, result.LastName);
                    // Verify that caching Add method was called for the person data contract
                    var cacheKey = repository.BuildFullCacheKey("PersonContract" + record.Recordkey);
                    cacheProviderMock.Verify(m => m.AddAndUnlockSemaphore(cacheKey, record, It.IsAny<SemaphoreSlim>(), It.IsAny<CacheItemPolicy>(), null));
                }
            }

        }

        [TestClass]
        public class GetPersonsBaseAsync : BasePersonSetup
        {
            PersonBaseRepository repository;

            [TestInitialize]
            public void Initialize()
            {
                // Initialize person setup and Mock framework
                PersonSetupInitialize();

                // Build the test repository
                repository = new PersonBaseRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object, apiSettings);

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
            [ExpectedException(typeof(ApplicationException))]
            public async Task GetPersonsBaseAsync_NullIdsException()
            {
                await repository.GetPersonsBaseAsync(null);
            }

            [TestMethod]
            [ExpectedException(typeof(ApplicationException))]
            public async Task GetPersonsBaseAsync_EmptyIdsException()
            {
                await repository.GetPersonsBaseAsync(new List<string>());
            }

            [TestMethod]
            public async Task GetPersonsBaseAsync_Valid()
            {
                var ids = personRecords.Values.Select(pr => pr.Recordkey).ToArray();
                var contractCollection = new System.Collections.ObjectModel.Collection<DataContracts.Person>();
                foreach (var record in personRecords.Values)
                {
                    contractCollection.Add(record);
                }
                dataReaderMock.Setup(acc => acc.BulkReadRecordAsync<DataContracts.Person>("PERSON", ids, true)).ReturnsAsync(contractCollection);
                IEnumerable<PersonBase> result = await repository.GetPersonsBaseAsync(ids);
                Assert.AreEqual(ids.Count(), result.Count());
            }

            [TestMethod]
            [ExpectedException(typeof(ApplicationException))]
            public async Task GetPersonsBaseAsync_CorruptRecords()
            {
                var ids = personRecords.Values.Select(pr => pr.Recordkey).ToArray();
                var contractCollection = new System.Collections.ObjectModel.Collection<DataContracts.Person>();
                foreach (var record in personRecords.Values)
                {
                    contractCollection.Add(record);
                }
                dataReaderMock.Setup(acc => acc.BulkReadRecordAsync<DataContracts.Person>("PERSON", ids, true)).ReturnsAsync(null);
                IEnumerable<PersonBase> result = await repository.GetPersonsBaseAsync(ids);
            }

            [TestMethod]
            [ExpectedException(typeof(ApplicationException))]
            public async Task GetPersonsBaseAsync_CorruptRecords2()
            {
                var ids = personRecords.Values.Select(pr => pr.Recordkey).ToArray();
                var contractCollection = new System.Collections.ObjectModel.Collection<DataContracts.Person>();
                foreach (var record in personRecords.Values)
                {
                    contractCollection.Add(record);
                }
                dataReaderMock.Setup(acc => acc.BulkReadRecordAsync<DataContracts.Person>("PERSON", ids, true)).ReturnsAsync(new System.Collections.ObjectModel.Collection<DataContracts.Person>());
                IEnumerable<PersonBase> result = await repository.GetPersonsBaseAsync(ids);
            }

            [TestMethod]
            [ExpectedException(typeof(ApplicationException))]
            public async Task GetPersonsBaseAsync_AtLeastOneNullId()
            {
                var ids = personRecords.Values.Select(pr => pr.Recordkey).ToArray();
                var idList = ids.ToList();
                idList.Add(null);
                var idListArray = idList.ToArray();
                var contractCollection = new System.Collections.ObjectModel.Collection<DataContracts.Person>();
                foreach (var record in personRecords.Values)
                {
                    contractCollection.Add(record);
                }
                dataReaderMock.Setup(acc => acc.BulkReadRecordAsync<DataContracts.Person>("PERSON", idListArray, true)).ThrowsAsync(new ArgumentNullException());
                IEnumerable<PersonBase> result = await repository.GetPersonsBaseAsync(ids);
            }
        }

        [TestClass]
        public class GetPersonBase : BasePersonSetup
        {
            PersonBaseRepository repository;

            [TestInitialize]
            public void Initialize()
            {
                // Initialize person setup and Mock framework
                PersonSetupInitialize();

                // Build the test repository
                repository = new PersonBaseRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object, apiSettings);

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
                await repository.GetBaseAsync<PersonBase>(string.Empty, person => new PersonBase(person.Recordkey, person.LastName));
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentOutOfRangeException))]
            public async Task GetBasePersonAsync_InvalidIdException()
            {
                await repository.GetBaseAsync<PersonBase>("123", person => new PersonBase(person.Recordkey, person.LastName));
            }

            [TestMethod]
            public async Task GetBasePersonAsync_GetCachedIsTrue()
            {
                foreach (var personItem in personRecords)
                {
                    string personId = personItem.Key;
                    DataContracts.Person record = personItem.Value;
                    dataReaderMock.Setup(acc => acc.ReadRecordAsync<DataContracts.Person>("PERSON", record.Recordkey, true)).ReturnsAsync(record);
                    PersonBase result = await repository.GetBaseAsync<PersonBase>(personId, person => new PersonBase(person.Recordkey, person.LastName));
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
                    PersonBase result = await repository.GetBaseAsync<PersonBase>(personId, person => new PersonBase(person.Recordkey, person.LastName), false);
                    // Spot check a couple of attribues: All other attributes are tested by the Get<> method tests       
                    Assert.AreEqual(record.Recordkey, result.Id);
                    Assert.AreEqual(record.LastName, result.LastName);
                    // Verify that caching Add method was called for the person data contract
                    var cacheKey = repository.BuildFullCacheKey("PersonContract" + record.Recordkey);
                    cacheProviderMock.Verify(m => m.AddAndUnlockSemaphore(cacheKey, record, It.IsAny<SemaphoreSlim>(), It.IsAny<CacheItemPolicy>(), null));
                }
            }
            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task GetNullIdException()
            {
                await repository.GetBaseAsync<PersonBase>(string.Empty,
                    person => new PersonBase(person.Recordkey, person.LastName));
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentOutOfRangeException))]
            public async Task GetSingleInvalidIdException()
            {
                await repository.GetBaseAsync<PersonBase>("123",
                    person => new PersonBase(person.Recordkey, person.LastName));
            }

            [TestMethod]
            public async Task GetSingleIdTests()
            {
                foreach (var kvp in personRecords)
                {
                    string personId = kvp.Key;
                    DataContracts.Person record = kvp.Value;
                    PersonBase result = await repository.GetBaseAsync<PersonBase>(personId,
                       person => new PersonBase(person.Recordkey, person.LastName)); ;
                    CompareBaseRecords(record, result);
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
                    PersonBase result = await repository.GetBaseAsync<PersonBase>(personId,
                       person => new PersonBase(person.Recordkey, person.LastName)); ;
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
                    PersonBase result = await repository.GetBaseAsync<PersonBase>(personId,
                       person => new PersonBase(person.Recordkey, person.LastName)); ;
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
                    PersonBase result = await repository.GetBaseAsync<PersonBase>(personId,
                       person => new PersonBase(person.Recordkey, person.LastName)); ;
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
                    PersonBase result = await repository.GetBaseAsync<PersonBase>(personId,
                       person => new PersonBase(person.Recordkey, person.LastName)); ;
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
                    PersonBase result = await repository.GetBaseAsync<PersonBase>(personId,
                       person => new PersonBase(person.Recordkey, person.LastName)); ;
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
                    PersonBase result = await repository.GetBaseAsync<PersonBase>(personId,
                       person => new PersonBase(person.Recordkey, person.LastName)); ;
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
                    PersonBase result = await repository.GetBaseAsync<PersonBase>(personId,
                       person => new PersonBase(person.Recordkey, person.LastName)); ;
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
                    PersonBase result = await repository.GetBaseAsync<PersonBase>(personId,
                       person => new PersonBase(person.Recordkey, person.LastName)); ;
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
                    PersonBase result = await repository.GetBaseAsync<PersonBase>(personId,
                       person => new PersonBase(person.Recordkey, person.LastName)); ;
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
                    PersonBase result = await repository.GetBaseAsync<PersonBase>(personId,
                       person => new PersonBase(person.Recordkey, person.LastName)); ;
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
                    PersonBase result = await repository.GetBaseAsync<PersonBase>(personId,
                       person => new PersonBase(person.Recordkey, person.LastName)); ;
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
                    PersonBase result = await repository.GetBaseAsync<PersonBase>(personId,
                       person => new PersonBase(person.Recordkey, person.LastName)); ;
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
                    PersonBase result = await repository.GetBaseAsync<PersonBase>(personId,
                       person => new PersonBase(person.Recordkey, person.LastName)); ;
                    // Confirm trims are working correctly and name is returned in Last, First MI format
                    Assert.AreEqual("Multi Part-Last, Mary Beth A.", result.PreferredName);
                }
            }

            [TestMethod]
            public async Task GetPreferredName_PREFERREDHierarchy_NotFound()
            {
                // Make sure a preferred name is built even if PREFERRED Hierarchy is not found.
                dataReaderMock.Setup<Task<Ellucian.Colleague.Data.Base.DataContracts.NameAddrHierarchy>>(a =>
                    a.ReadRecordAsync<Ellucian.Colleague.Data.Base.DataContracts.NameAddrHierarchy>("NAME.ADDR.HIERARCHY", "PREFERRED", true))
                .ReturnsAsync(null);
                var testPersonRecord = personRecords["9999998"];
                if (testPersonRecord != null)
                {
                    string personId = testPersonRecord.Recordkey;
                    PersonBase result = await repository.GetBaseAsync<PersonBase>(personId,
                       person => new PersonBase(person.Recordkey, person.LastName)); ;
                    // Confirm trims are working correctly and name is returned in Last, First MI format
                    Assert.AreEqual("Mary Beth A. Multi Part-Last", result.PreferredName);
                }
            }



            private void CompareBaseRecords(DataContracts.Person record, PersonBase result)
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
                Assert.AreEqual((record.PersonStatus == "D" || record.PersonStatus == "U"), result.IsDeceased);
            }
        }

        [TestClass]
        public class GetFilteredPerson2Guids : BasePersonSetup
        {
            PersonBaseRepository repository;

            [TestInitialize]
            public void Initialize()
            {
                PersonSetupInitialize();

                var personIds = new List<string>() { "1" };
                dataReaderMock.Setup(d => d.SelectAsync("PERSON", It.IsAny<string>())).ReturnsAsync(personIds.ToArray());

                repository = new PersonBaseRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object, apiSettings);
            }

            [TestCleanup]
            public void Cleanup()
            {
                repository = null;
            }

            [TestMethod]
            public async Task GetFilteredPerson2Guids_With_Invalid_PersonFilter1()
            {
                dataReaderMock.Setup(d => d.SelectAsync(It.IsAny<GuidLookup[]>())).ReturnsAsync(null);

                var result = await repository.GetFilteredPerson2GuidsAsync(0, 10, true, null, "1");

                Assert.IsTrue(result.Item2 == 0);
            }

            [TestMethod]
            public async Task GetFilteredPerson2Guids_With_Invalid_PersonFilter2()
            {
                var dicResult = new Dictionary<string, GuidLookupResult>() { };
                dataReaderMock.Setup(d => d.SelectAsync(It.IsAny<GuidLookup[]>())).ReturnsAsync(null);

                var result = await repository.GetFilteredPerson2GuidsAsync(0, 10, true, null, "1");

                Assert.IsTrue(result.Item2 == 0);
            }

            [TestMethod]
            public async Task GetFilteredPerson2Guids_With_Invalid_PersonFilter3()
            {
                var dictResult = new Dictionary<string, GuidLookupResult>() { { "1", null } };
                dataReaderMock.Setup(d => d.SelectAsync(It.IsAny<GuidLookup[]>())).ReturnsAsync(dictResult);

                var result = await repository.GetFilteredPerson2GuidsAsync(0, 10, true, null, "1");

                Assert.IsTrue(result.Item2 == 0);
            }

            [TestMethod]
            public async Task GetFilteredPerson2Guids_With_Invalid_EntityName()
            {
                var dictResult = new Dictionary<string, GuidLookupResult>() { { "1", new GuidLookupResult() { Entity = "SAVE.LIST.PARMS1" } } };
                dataReaderMock.Setup(d => d.SelectAsync(It.IsAny<GuidLookup[]>())).ReturnsAsync(dictResult);

                var result = await repository.GetFilteredPerson2GuidsAsync(0, 10, true, null, "1");

                Assert.IsTrue(result.Item2 == 0);
            }

            [TestMethod]
            [ExpectedException(typeof(RepositoryException))]
            public async Task GetFilteredPerson2Guids_Without_Filters()
            {
                var personGuids = new Dictionary<string, RecordKeyLookupResult>() { { "1+1", null } };
                dataReaderMock.Setup(d => d.SelectAsync(It.IsAny<RecordKeyLookup[]>())).ReturnsAsync(personGuids);

                var result = await repository.GetFilteredPerson2GuidsAsync(0, 10, true, null, "");
            }

            [TestMethod]
            public async Task GetFilteredPerson2Guids_With_Credential_Filters()
            {
                var personGuids = new Dictionary<string, RecordKeyLookupResult>() { { "1", new RecordKeyLookupResult() { Guid = Guid.NewGuid().ToString() } } };
                dataReaderMock.Setup(d => d.SelectAsync(It.IsAny<RecordKeyLookup[]>())).ReturnsAsync(personGuids);

                var ids = new string[] { "1" };
                dataReaderMock.Setup(d => d.SelectAsync("PERSON", It.IsAny<string[]>(), It.IsAny<string>())).ReturnsAsync(ids);
                dataReaderMock.Setup(d => d.SelectAsync("PERSON.PIN", It.IsAny<string[]>(), It.IsAny<string>())).ReturnsAsync(ids);

                var persons = new Collection<DataContracts.Person>()
                {
                    new DataContracts.Person()
                    {
                        RecordGuid = Guid.NewGuid().ToString(),
                        Recordkey = "1",
                        FirstName = "first",
                        LastName = "last",
                        MiddleName = "middle",
                        Prefix = "Mr.",
                        BirthNameLast = "P",
                        PersonAltEntityAssociation = new List<DataContracts.PersonPersonAlt> { new DataContracts.PersonPersonAlt { PersonAltIdsAssocMember = "0000818", PersonAltIdTypesAssocMember = "ELEV" } }

                    }
                };

                dataReaderMock.Setup(d => d.BulkReadRecordAsync<DataContracts.Person>("PERSON", It.IsAny<string[]>(), true)).ReturnsAsync(persons);

                var filter = new PersonFilterCriteria()
                {
                    Credentials = new List<Tuple<string, string>>()
                    {
                        new Tuple<string, string>("colleaguepersonid", "0000717"),
                        new Tuple<string, string>("elevateid", "0000818"),
                        new Tuple<string, string>("colleagueusername", "0000919"),
                    }
                };

                var result = await repository.GetFilteredPerson2GuidsAsync(0, 10, true, filter, "");

                Assert.IsTrue(result.Item2 == 1);
            }

            [TestMethod]
            public async Task GetFilteredPerson2Guids_With_Names_Filters()
            {
                var personGuids = new Dictionary<string, RecordKeyLookupResult>() { { "1", new RecordKeyLookupResult() { Guid = Guid.NewGuid().ToString() } } };
                dataReaderMock.Setup(d => d.SelectAsync(It.IsAny<RecordKeyLookup[]>())).ReturnsAsync(personGuids);

                var ids = new string[] { "1" };
                dataReaderMock.Setup(d => d.SelectAsync("PERSON", It.IsAny<string[]>(), It.IsAny<string>())).ReturnsAsync(ids);

                var persons = new Collection<DataContracts.Person>()
                {
                    new DataContracts.Person()
                    {
                        RecordGuid = Guid.NewGuid().ToString(),
                        Recordkey = "1",
                        FirstName = "first",
                        LastName = "last",
                        MiddleName = "middle",
                        Prefix = "Mr.",
                        BirthNameLast = "P"
                    }
                };

                var filter = new PersonFilterCriteria()
                {
                    Names = new List<PersonNamesCriteria>()
                    {
                        new PersonNamesCriteria()
                        {
                            FirstName = "first",
                            MiddleName = "middle",
                            LastName = "last",
                            Title = "Mr.",
                            LastNamePrefix = "P"
                        }
                    }
                };

                var response = new GetPersonFillterResultsV2Response()
                {
                    PersonIds = new List<string>() { "1" },
                    TotalRecords = 1
                };
                transManagerMock.Setup(t => t.ExecuteAsync<GetPersonFillterResultsV2Request, GetPersonFillterResultsV2Response>(It.IsAny<GetPersonFillterResultsV2Request>())).
                    ReturnsAsync(response);

                var result = await repository.GetFilteredPerson2GuidsAsync(0, 10, true, filter, "");

                Assert.IsTrue(result.Item2 == 1);
            }

            [TestMethod]
            public async Task GetFilteredPerson2Guids_With_Email_Filter()
            {
                var personGuids = new Dictionary<string, RecordKeyLookupResult>() { { "1", new RecordKeyLookupResult() { Guid = Guid.NewGuid().ToString() } } };
                dataReaderMock.Setup(d => d.SelectAsync(It.IsAny<RecordKeyLookup[]>())).ReturnsAsync(personGuids);

                var ids = new string[] { "1" };
                dataReaderMock.Setup(d => d.SelectAsync("PERSON", It.IsAny<string[]>(), It.IsAny<string>())).ReturnsAsync(ids);

                var emailIds = new string[] { "first.last@ellucian.com" };
                dataReaderMock.Setup(d => d.SelectAsync("PERSON", It.IsAny<string>(), It.IsAny<string[]>(), It.IsAny<string>(), true, 425)).ReturnsAsync(emailIds);

                var persons = new Collection<DataContracts.Person>()
                {
                    new DataContracts.Person()
                    {
                        RecordGuid = Guid.NewGuid().ToString(),
                        Recordkey = "1",
                    }
                };
                dataReaderMock.Setup(d => d.BulkReadRecordAsync<DataContracts.Person>("PERSON", It.IsAny<string[]>(), true)).ReturnsAsync(persons);

                var filter = new PersonFilterCriteria()
                {
                    Emails = new List<string>() { "first.last@ellucian.com" }
                };

                var result = await repository.GetFilteredPerson2GuidsAsync(0, 10, true, filter, "");

                Assert.IsTrue(result.Item2 == 1);
            }

            [TestMethod]
            public async Task GetFilteredPerson2Guids_With_Role_Filter()
            {
                var roleIds = new List<string>() { "1" };
                dataReaderMock.Setup(d => d.SelectAsync("STUDENTS", It.IsAny<string[]>(), It.IsAny<string>())).ReturnsAsync(roleIds.ToArray());
                dataReaderMock.Setup(d => d.SelectAsync("FACULTY", It.IsAny<string[]>(), It.IsAny<string>())).ReturnsAsync(roleIds.ToArray());
                dataReaderMock.Setup(d => d.SelectAsync("HRPER", It.IsAny<string[]>(), It.IsAny<string>())).ReturnsAsync(roleIds.ToArray());
                dataReaderMock.Setup(d => d.SelectAsync("EMPLOYES", It.IsAny<string[]>(), It.IsAny<string>())).ReturnsAsync(roleIds.ToArray());
                dataReaderMock.Setup(d => d.SelectAsync("APPLICANTS", It.IsAny<string[]>(), It.IsAny<string>())).ReturnsAsync(roleIds.ToArray());
                dataReaderMock.Setup(d => d.SelectAsync("VENDORS", It.IsAny<string[]>(), It.IsAny<string>())).ReturnsAsync(roleIds.ToArray());


                var personGuids = new Dictionary<string, RecordKeyLookupResult>() { { "1", new RecordKeyLookupResult() { Guid = Guid.NewGuid().ToString() } } };
                dataReaderMock.Setup(d => d.SelectAsync(It.IsAny<RecordKeyLookup[]>())).ReturnsAsync(personGuids);

                var ids = new string[] { "1" };
                dataReaderMock.Setup(d => d.SelectAsync("PERSON", It.IsAny<string[]>(), It.IsAny<string>())).ReturnsAsync(ids);
                dataReaderMock.Setup(d => d.SelectAsync("PERSON.INTG", It.IsAny<string[]>(), It.IsAny<string>())).ReturnsAsync(ids);

                var persons = new Collection<DataContracts.Person>()
                {
                    new DataContracts.Person()
                    {
                        RecordGuid = Guid.NewGuid().ToString(),
                        Recordkey = "1",
                    }
                };
                dataReaderMock.Setup(d => d.BulkReadRecordAsync<DataContracts.Person>("PERSON", It.IsAny<string[]>(), true)).ReturnsAsync(persons);

                var filter = new PersonFilterCriteria()
                {
                    Roles = new List<string>() { "student", "instructor", "employee", "prospectivestudent", "advisor", "alumni", "vendor", }
                };

                var result = await repository.GetFilteredPerson2GuidsAsync(0, 10, true, filter, "");

                Assert.IsTrue(result.Item2 == 1);
            }

            [TestMethod]
            public async Task GetFilteredPerson2Guids_With_Person_Filter()
            {
                var dictResult = new Dictionary<string, GuidLookupResult>() { { "1", new GuidLookupResult() { Entity = "SAVE.LIST.PARMS" } } };
                dataReaderMock.Setup(d => d.SelectAsync(It.IsAny<GuidLookup[]>())).ReturnsAsync(dictResult);

                var personGuids = new Dictionary<string, RecordKeyLookupResult>() { { "1", new RecordKeyLookupResult() { Guid = Guid.NewGuid().ToString() } } };
                dataReaderMock.Setup(d => d.SelectAsync(It.IsAny<RecordKeyLookup[]>())).ReturnsAsync(personGuids);

                var response = new GetPersonFillterResultsV2Response()
                {
                    PersonIds = new List<string>() { "1" },
                    TotalRecords = 1
                };
                transManagerMock.Setup(t => t.ExecuteAsync<GetPersonFillterResultsV2Request, GetPersonFillterResultsV2Response>(It.IsAny<GetPersonFillterResultsV2Request>())).
                    ReturnsAsync(response);

                var result = await repository.GetFilteredPerson2GuidsAsync(0, 10, true, null, "1");

                Assert.IsTrue(result.Item2 == 1);
            }

            [TestMethod]
            [ExpectedException(typeof(RepositoryException))]
            public async Task GetFilteredPerson2Guids_With_Names_Filters_ErrorResponse()
            {
                var personGuids = new Dictionary<string, RecordKeyLookupResult>() { { "1", new RecordKeyLookupResult() { Guid = Guid.NewGuid().ToString() } } };
                dataReaderMock.Setup(d => d.SelectAsync(It.IsAny<RecordKeyLookup[]>())).ReturnsAsync(personGuids);

                var ids = new string[] { "1" };
                dataReaderMock.Setup(d => d.SelectAsync("PERSON", It.IsAny<string[]>(), It.IsAny<string>())).ReturnsAsync(ids);

                var persons = new Collection<DataContracts.Person>()
                {
                    new DataContracts.Person()
                    {
                        RecordGuid = Guid.NewGuid().ToString(),
                        Recordkey = "1",
                        FirstName = "first",
                        LastName = "last",
                        MiddleName = "middle",
                        Prefix = "Mr.",
                        BirthNameLast = "P"
                    }
                };

                var filter = new PersonFilterCriteria()
                {
                    Names = new List<PersonNamesCriteria>()
                    {
                        new PersonNamesCriteria()
                        {
                            FirstName = "first",
                            MiddleName = "middle",
                            LastName = "last",
                            Title = "Mr.",
                            LastNamePrefix = "P"
                        }
                    }
                };

                var response = new GetPersonFillterResultsV2Response()
                {
                    ErrorMessages = new List<string>() { "error" },
                    LogStmt = new List<string>() { "log" }
                };
                transManagerMock.Setup(t => t.ExecuteAsync<GetPersonFillterResultsV2Request, GetPersonFillterResultsV2Response>(It.IsAny<GetPersonFillterResultsV2Request>())).
                    ReturnsAsync(response);

                await repository.GetFilteredPerson2GuidsAsync(0, 10, true, filter, "");
            }
        }

        #region SearchByName

        [TestClass]
        public class SearchByName : BaseRepositorySetup
        {
            PersonBaseRepository personBaseRepository;
            string quote;

            [TestInitialize]
            public void Initialize()
            {
                quote = '"'.ToString();

                // Build the test repository
                MockInitialize();

                personBaseRepository = new PersonBaseRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object, apiSettings);
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

                await personBaseRepository.SearchByNameAsync(lastName);
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
                await personBaseRepository.SearchByNameAsync(lastName, firstName, middleName);
                Assert.IsTrue(queryValue.Contains("WITH PARTIAL.NAME.INDEX EQ \"BROWN_JN\""));
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task SearchByName_ThrowsErrorIfLastNameNull()
            {
                string lastName = null;
                await personBaseRepository.SearchByNameAsync(lastName);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task SearchByName_ThrowsErrorIfLastNameBlank()
            {
                string lastName = "  ";
                await personBaseRepository.SearchByNameAsync(lastName);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task SearchByName_ThrowsErrorIfLastNameEmpty()
            {
                string lastName = "";
                await personBaseRepository.SearchByNameAsync(lastName);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task SearchByName_throwsErrorWhenLastNameLessThanTwoCharacters()
            {
                await personBaseRepository.SearchByNameAsync("x", "first", "middle");
            }
        }

        #endregion

        #region SearchByIdsOrNamesAsync
        [TestClass]
        public class SearchByIdsOrNamesAsync : BaseRepositorySetup
        {
            PersonBaseRepository personBaseRepository;

            // Used in record mocking to return different datasets based on criteria given to PERSON data reads
            private IList<Base.DataContracts.Person> PersonCriteriaFilter(IDictionary<string, Base.DataContracts.Person> keyedRecords, string[] requestedIds, string criteria)
            {
                if (string.IsNullOrEmpty(criteria))
                {
                    return keyedRecords.Where(kr => requestedIds.Contains(kr.Key)).Select(kr => kr.Value).ToList();
                }
                else
                {
                    if (criteria == "WITH PERSON.CORP.INDICATOR NE 'Y'")
                    {
                        return keyedRecords.Where(kr => requestedIds.Contains(kr.Key)).Select(kr => kr.Value).Where(v => v.PersonCorpIndicator != "Y").ToList();
                    }
                    else if (criteria == "WITH PARTIAL.NAME.INDEX EQ \"DOE_JN\"")
                    {
                        return keyedRecords.Where(kr => requestedIds.Contains(kr.Key)).Select(kr => kr.Value).Where(v => v.LastName.Equals("Doe", StringComparison.InvariantCultureIgnoreCase)).ToList();
                    }
                    else
                    {
                        return keyedRecords.Where(kr => requestedIds.Contains(kr.Key)).Select(kr => kr.Value).ToList();
                    }
                }
            }

            [TestInitialize]
            public void Initialize()
            {
                // Build the test repository
                MockInitialize();

                var personRecords = (new List<Base.DataContracts.Person>()
                    {
                        new Base.DataContracts.Person()
                        {
                            Recordkey = "0000001",
                            RecordGuid = GenerateGuid(),
                            LastName = "Smith",
                            FirstName = "John",
                            MiddleName = "Jacob Jingleheimer"
                        },
                        new Base.DataContracts.Person()
                        {
                            Recordkey = "0000002",
                            RecordGuid = GenerateGuid(),
                            LastName = "Doe",
                            FirstName = "John",
                            MiddleName = "None"
                        },
                        new Base.DataContracts.Person()
                        {
                            Recordkey = "0000003",
                            RecordGuid = GenerateGuid(),
                            LastName = "Corporation",
                            FirstName = "XYZ",
                            MiddleName = "None",
                            PersonCorpIndicator = "Y"
                        }
                    });
                MockRecordsAsync("PERSON", personRecords.ToDictionary(rr => rr.Recordkey), PersonCriteriaFilter);

                personBaseRepository = new PersonBaseRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object, apiSettings);

            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task SearchByIdsOrNamesAsync_WhenEmptyIdsAndNameGiven_ThrowsArgumentException()
            {
                var result = await personBaseRepository.SearchByIdsOrNamesAsync(new List<string>(), "");
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task SearchByIdsOrNamesAsync_WhenEmptyIdsAndNullNameGiven_ThrowsArgumentException()
            {
                var result = await personBaseRepository.SearchByIdsOrNamesAsync(new List<string>(), null);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task SearchByIdsOrNamesAsync_WhenNullIdAndEmptyNameGiven_ThrowsArgumentException()
            {
                var result = await personBaseRepository.SearchByIdsOrNamesAsync(null, "");
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task SearchByIdsOrNamesAsync_WhenNullIdsAndNameGiven_ThrowsArgumentException()
            {
                var result = await personBaseRepository.SearchByIdsOrNamesAsync(null, null);
            }

            [TestMethod]
            public async Task SearchByIdsOrNamesAsync_WhenIdsArePassed_ReturnsMatchingPersons()
            {
                var requestedIds = new List<string>() { "0000001", "0000002" };
                var result = await personBaseRepository.SearchByIdsOrNamesAsync(requestedIds, null);
                Assert.AreEqual(2, result.Count());
                Assert.AreEqual("Smith", result.ElementAt(0).LastName);
                Assert.AreEqual("Doe", result.ElementAt(1).LastName);
            }

            [TestMethod]
            public async Task SearchByIdsOrNamesAsync_WhenIdsArePassedForCorps_ReturnsMatchingPersonsFilteringCorps()
            {
                var requestedIds = new List<string>() { "0000001", "0000002", "0000003" };
                var result = await personBaseRepository.SearchByIdsOrNamesAsync(requestedIds, null);
                Assert.AreEqual(2, result.Count());
                Assert.AreEqual("Smith", result.ElementAt(0).LastName);
                Assert.AreEqual("Doe", result.ElementAt(1).LastName);
            }

            [TestMethod]
            public async Task SearchByIdsOrNamesAsync_WhenNumericKeywordIsGiven_ReturnsMatchingPersons()
            {
                var keyword = "0000001";
                var result = await personBaseRepository.SearchByIdsOrNamesAsync(null, keyword);
                Assert.AreEqual(1, result.Count());
                Assert.AreEqual("Smith", result.ElementAt(0).LastName);
            }

            [TestMethod]
            public async Task SearchByIdsOrNamesAsync_WhenIdsAndNumericKeywordIsGiven_ReturnsMatchingPersons()
            {
                var requestedIds = new List<string>() { "0000001" };
                var keyword = "0000002";
                var result = await personBaseRepository.SearchByIdsOrNamesAsync(requestedIds, keyword);
                Assert.AreEqual(2, result.Count());
                Assert.AreEqual("Smith", result.ElementAt(0).LastName);
                Assert.AreEqual("Doe", result.ElementAt(1).LastName);
            }

            [TestMethod]
            public async Task SearchByIdsOrNamesAsync_WhenNameKeywordIsGivenLFM_ReturnsMatchingPersons()
            {
                var lookupStringResponse = new GetPersonLookupStringResponse() { IndexString = ";PARTIAL.NAME.INDEX DOE_JN", ErrorMessage = "" };
                transManagerMock.Setup(manager => manager
                        .ExecuteAsync<GetPersonLookupStringRequest, GetPersonLookupStringResponse>(It.IsAny<GetPersonLookupStringRequest>()))
                        .ReturnsAsync(lookupStringResponse);

                var keyword = "Doe, Jane";
                var result = await personBaseRepository.SearchByIdsOrNamesAsync(null, keyword);
                Assert.AreEqual(1, result.Count());
                Assert.AreEqual("Doe", result.ElementAt(0).LastName);
            }

            [TestMethod]
            public async Task SearchByIdsOrNamesAsync_WhenNameKeywordIsGivenFML_ReturnsMatchingPersons()
            {
                var lookupStringResponse = new GetPersonLookupStringResponse() { IndexString = ";PARTIAL.NAME.INDEX DOE_JN", ErrorMessage = "" };
                transManagerMock.Setup(manager => manager
                        .ExecuteAsync<GetPersonLookupStringRequest, GetPersonLookupStringResponse>(It.IsAny<GetPersonLookupStringRequest>()))
                        .ReturnsAsync(lookupStringResponse);

                var keyword = "Jane Doe";
                var result = await personBaseRepository.SearchByIdsOrNamesAsync(null, keyword);
                Assert.AreEqual(1, result.Count());
                Assert.AreEqual("Doe", result.ElementAt(0).LastName);
            }

        }
        #endregion

        #region IsPerson

        [TestClass]
        public class IsPerson : BasePersonSetup
        {
            PersonBaseRepository repository;

            [TestInitialize]
            public void Initialize()
            {
                // Initialize person setup and Mock framework
                PersonSetupInitialize();

                // Build the test repository
                repository = new PersonBaseRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object, apiSettings);
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
            PersonBaseRepository repository;

            [TestInitialize]
            public void Initialize()
            {
                // Initialize person setup and Mock framework
                PersonSetupInitialize();

                // Build the test repository
                repository = new PersonBaseRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object, apiSettings);
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
            PersonBaseRepository repository;

            [TestInitialize]
            public void Initialize()
            {
                // Initialize person setup and Mock framework
                PersonSetupInitialize();

                // Build the test repository
                repository = new PersonBaseRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object, apiSettings);
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

        #region GetPersonIntegrationData

        [TestClass]
        public class GetPersonIntegrationData : BasePersonSetup
        {
            PersonBaseRepository repository;
            private Ellucian.Colleague.Domain.Base.Entities.PersonBase personBase;
            private List<Ellucian.Colleague.Domain.Base.Entities.Address> addresses;
            private List<Ellucian.Colleague.Domain.Base.Entities.Phone> phones;

            [TestInitialize]
            public void Initialize()
            {
                // Initialize person setup and Mock framework
                PersonSetupInitialize();

                // setup person object
                addresses = new List<Address>();
                phones = new List<Phone>();
                personBase = GetTestPersonDataEntities(out addresses, out phones);

                // Build the test repository
                repository = new PersonBaseRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object, apiSettings);
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
                for (int i = 0; i < personBase.EmailAddresses.Count(); i++)
                {
                    response.PersonEmailAddresses.Add(new PersonEmailAddresses()
                    {
                        EmailAddressValue = personBase.EmailAddresses[i].Value,
                        EmailAddressType = personBase.EmailAddresses[i].TypeCode
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
                for (int i = 0; i < personBase.EmailAddresses.Count(); i++)
                {
                    Assert.AreEqual(true, personBase.EmailAddresses[i].Value == personEmailAddresses[i].Value);
                    Assert.AreEqual(true, personBase.EmailAddresses[i].TypeCode == personEmailAddresses[i].TypeCode);
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
            public async Task GetPersonGuidsAsync()
            {
                IEnumerable<string> sublist = new List<string>() { "1", "2" };
                Dictionary<string, RecordKeyLookupResult> recordKeyLookupResults = new Dictionary<string, RecordKeyLookupResult>();
                recordKeyLookupResults.Add("PERSON+1", new RecordKeyLookupResult() { Guid = "854da721-4191-4875-bf58-7d6c00ffea8f", ModelName = "persons" });
                recordKeyLookupResults.Add("PERSON+2", new RecordKeyLookupResult() { Guid = "71e1a806-24a8-4d93-91a2-02d86056b63c", ModelName = "persons" });
                List<KeyValuePair<string, RecordKeyLookupResult>> list = recordKeyLookupResults.ToList();

                dataReaderMock.Setup(i => i.SelectAsync("PERSON", It.IsAny<string>())).ReturnsAsync(new[] { "1", "2", "3", "4" });
                dataReaderMock.Setup(i => i.SelectAsync(It.IsAny<RecordKeyLookup[]>())).ReturnsAsync(recordKeyLookupResults);

                var result = await repository.GetPersonGuidsAsync(0, 2, It.IsAny<bool>());
                Assert.IsNotNull(result);
                Assert.AreEqual(2, result.Item1.Count());
            }

            [TestMethod]
            public async Task GetPersonGuidsAsync_Null()
            {
                dataReaderMock.Setup(i => i.SelectAsync("PERSON", It.IsAny<string>())).ReturnsAsync(new string[] { });
                var result = await repository.GetPersonGuidsAsync(0, 2, It.IsAny<bool>());
                Assert.IsNotNull(result);
                Assert.IsNull(result.Item1);
            }

            [TestMethod]
            public async Task GetPersonGuidsFilteredAsync()
            {
                IEnumerable<string> sublist = new List<string>() { "1", "2" };
                Dictionary<string, RecordKeyLookupResult> recordKeyLookupResults = new Dictionary<string, RecordKeyLookupResult>();
                recordKeyLookupResults.Add("PERSON+1", new RecordKeyLookupResult() { Guid = "854da721-4191-4875-bf58-7d6c00ffea8f", ModelName = "persons" });
                recordKeyLookupResults.Add("PERSON+2", new RecordKeyLookupResult() { Guid = "71e1a806-24a8-4d93-91a2-02d86056b63c", ModelName = "persons" });
                List<KeyValuePair<string, RecordKeyLookupResult>> list = recordKeyLookupResults.ToList();

                dataReaderMock.Setup(i => i.SelectAsync("PERSON", It.IsAny<string>())).ReturnsAsync(new[] { "1", "2", "3", "4" });
                dataReaderMock.Setup(i => i.SelectAsync(It.IsAny<RecordKeyLookup[]>())).ReturnsAsync(recordKeyLookupResults);

                var result = await repository.GetPersonGuidsFilteredAsync(0, 2, null, It.IsAny<bool>());
                Assert.IsNotNull(result);
                Assert.AreEqual(2, result.Item1.Count());
            }

            [TestMethod]
            public async Task GetPersonGuidsFilteredAsync_Null()
            {
                dataReaderMock.Setup(i => i.SelectAsync("PERSON", It.IsAny<string>())).ReturnsAsync(new string[] { });
                dataReaderMock.Setup(i => i.SelectAsync("PERSON.PIN", It.IsAny<string>())).ReturnsAsync(new string[] { });
                var result = await repository.GetPersonGuidsFilteredAsync(0, 2, null, It.IsAny<bool>());
                Assert.IsNull(result);
            }

            [TestMethod]
            public async Task GetPersonGuidsFilteredAsync_SSN()
            {
                var testPersonRecord = personRecords["0000001"];

                Dictionary<string, RecordKeyLookupResult> recordKeyLookupResults = new Dictionary<string, RecordKeyLookupResult>();
                recordKeyLookupResults.Add("PERSON+1", new RecordKeyLookupResult() { Guid = "c90fa3b9-c3e7-4055-b112-946adb7bc104", ModelName = "persons" });

                List<KeyValuePair<string, RecordKeyLookupResult>> list = recordKeyLookupResults.ToList();

                dataReaderMock.Setup(i => i.SelectAsync("PERSON", It.IsAny<string>())).ReturnsAsync(new[] { "0000001" });
                dataReaderMock.Setup(i => i.SelectAsync(It.IsAny<RecordKeyLookup[]>())).ReturnsAsync(recordKeyLookupResults);

                var credentials = new Dictionary<string, string>();
                credentials.Add("ssn", "111-11-1111");

                var result = await repository.GetPersonGuidsFilteredAsync(0, 2, credentials, It.IsAny<bool>());
                Assert.IsNotNull(result);
                Assert.AreEqual(1, result.Item1.Count());
            }

            [TestMethod]
            public async Task GetPersonGuidsFilteredAsync_ColleaguePersonID()
            {
                var testPersonRecord = personRecords["0000001"];

                Dictionary<string, RecordKeyLookupResult> recordKeyLookupResults = new Dictionary<string, RecordKeyLookupResult>();
                recordKeyLookupResults.Add("PERSON+1", new RecordKeyLookupResult() { Guid = "c90fa3b9-c3e7-4055-b112-946adb7bc104", ModelName = "persons" });

                List<KeyValuePair<string, RecordKeyLookupResult>> list = recordKeyLookupResults.ToList();

                dataReaderMock.Setup(i => i.SelectAsync("PERSON", It.IsAny<string>())).ReturnsAsync(new[] { "0000001" });
                dataReaderMock.Setup(i => i.SelectAsync(It.IsAny<RecordKeyLookup[]>())).ReturnsAsync(recordKeyLookupResults);

                var credentials = new Dictionary<string, string>();
                credentials.Add("colleaguepersonid", "0000001");

                var result = await repository.GetPersonGuidsFilteredAsync(0, 2, credentials, It.IsAny<bool>());
                Assert.IsNotNull(result);
                Assert.AreEqual(1, result.Item1.Count());
            }

            [TestMethod]
            public async Task GetPersonGuidsFilteredAsync_ColleagueUsername()
            {
                var testPersonRecord = personRecords["0000001"];

                Dictionary<string, RecordKeyLookupResult> recordKeyLookupResults = new Dictionary<string, RecordKeyLookupResult>();
                recordKeyLookupResults.Add("PERSON+1", new RecordKeyLookupResult() { Guid = "c90fa3b9-c3e7-4055-b112-946adb7bc104", ModelName = "persons" });

                List<KeyValuePair<string, RecordKeyLookupResult>> list = recordKeyLookupResults.ToList();

                dataReaderMock.Setup(i => i.SelectAsync("PERSON", It.IsAny<string>())).ReturnsAsync(new[] { "0000001" });
                dataReaderMock.Setup(i => i.SelectAsync(It.IsAny<RecordKeyLookup[]>())).ReturnsAsync(recordKeyLookupResults);

                // await DataReader.SelectAsync("PERSON.PIN", " WITH PERSON.PIN.USER.ID = '?'", personPinIds.ToArray());

                dataReaderMock.Setup(i => i.SelectAsync("PERSON.PIN", It.IsAny<string>(), It.IsAny<string[]>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<int>()))
                    .ReturnsAsync(new[] { "0000001" });

                var credentials = new Dictionary<string, string>();
                credentials.Add("colleagueusername", "washington");

                var result = await repository.GetPersonGuidsFilteredAsync(0, 2, credentials, It.IsAny<bool>());
                Assert.IsNotNull(result);
                Assert.AreEqual(1, result.Item1.Count());
            }


            [TestMethod]
            public async Task GetPersonGuidsFilteredAsync_ElevateId()
            {
                var testPersonRecord = personRecords["0000001"];
                testPersonRecord.PersonAltEntityAssociation = new List<DataContracts.PersonPersonAlt>()
                {  new DataContracts.PersonPersonAlt("123", "ELEV")};

                var testPersonRecords = new Collection<DataContracts.Person>();
                testPersonRecords.Add(testPersonRecord);

                Dictionary<string, RecordKeyLookupResult> recordKeyLookupResults = new Dictionary<string, RecordKeyLookupResult>();
                recordKeyLookupResults.Add("PERSON+1", new RecordKeyLookupResult() { Guid = "c90fa3b9-c3e7-4055-b112-946adb7bc104", ModelName = "persons" });

                List<KeyValuePair<string, RecordKeyLookupResult>> list = recordKeyLookupResults.ToList();

                dataReaderMock.Setup(i => i.SelectAsync("PERSON", It.IsAny<string>())).ReturnsAsync(new[] { "0000001", "0000002" });
                dataReaderMock.Setup(i => i.SelectAsync(It.IsAny<RecordKeyLookup[]>())).ReturnsAsync(recordKeyLookupResults);

                dataReaderMock.Setup(i => i.SelectAsync("PERSON", It.IsAny<string>(), It.IsAny<string[]>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<int>()))
                    .ReturnsAsync(new[] { "0000001" });

                // var elevatePersons = await DataReader.BulkReadRecordAsync<DataContracts.Person>("PERSON", elevatePersonList.ToArray());
                dataReaderMock.Setup(i => i.BulkReadRecordAsync<DataContracts.Person>("PERSON", It.IsAny<string[]>(), It.IsAny<bool>()))
                   .ReturnsAsync(testPersonRecords);

                var credentials = new Dictionary<string, string>();
                credentials.Add("elevateid", "123");

                var result = await repository.GetPersonGuidsFilteredAsync(0, 2, credentials, It.IsAny<bool>());
                Assert.IsNotNull(result);
                Assert.AreEqual(1, result.Item1.Count());
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


            [TestMethod]
            public async Task GetPersonGuidsCollectionAsync()
            {
                IEnumerable<string> sublist = new List<string>() { "1", "2" };
                Dictionary<string, RecordKeyLookupResult> recordKeyLookupResults = new Dictionary<string, RecordKeyLookupResult>();
                recordKeyLookupResults.Add("PERSON+1", new RecordKeyLookupResult() { Guid = "854da721-4191-4875-bf58-7d6c00ffea8f", ModelName = "persons" });
                recordKeyLookupResults.Add("PERSON+2", new RecordKeyLookupResult() { Guid = "71e1a806-24a8-4d93-91a2-02d86056b63c", ModelName = "persons" });
                List<KeyValuePair<string, RecordKeyLookupResult>> list = recordKeyLookupResults.ToList();

                dataReaderMock.Setup(i => i.SelectAsync("PERSON", It.IsAny<string>())).ReturnsAsync(new[] { "1", "2", "3", "4" });
                dataReaderMock.Setup(i => i.SelectAsync(It.IsAny<RecordKeyLookup[]>())).ReturnsAsync(recordKeyLookupResults);

                var results = await repository.GetPersonGuidsCollectionAsync(sublist);
                Assert.IsNotNull(results);
                Assert.AreEqual(2, results.Count());
                foreach (var result in results)
                {
                    RecordKeyLookupResult recordKeyLookupResult = null;
                    recordKeyLookupResults.TryGetValue(string.Concat("PERSON+", result.Key), out recordKeyLookupResult);

                    Assert.AreEqual(result.Value, recordKeyLookupResult.Guid);
                }
            }
            #endregion
        }
    }

}

