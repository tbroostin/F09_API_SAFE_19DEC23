/*Copyright 2017 Ellucian Company L.P. and its affiliates.*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ellucian.Colleague.Data.Base.Tests.Repositories;
using Ellucian.Colleague.Domain.FinancialAid.Tests;
using Ellucian.Colleague.Domain.FinancialAid.Entities;
using Ellucian.Colleague.Data.FinancialAid.Repositories;
using Moq;
using System.Collections.ObjectModel;
using Ellucian.Colleague.Data.Base.Transactions;
using Ellucian.Colleague.Domain.FinancialAid.Repositories;
using Ellucian.Colleague.Domain.Base.Entities;

namespace Ellucian.Colleague.Data.FinancialAid.Tests.Repositories
{
    [TestClass]
    public class FinancialAidPersonRepositoryTests : BaseRepositorySetup
    {
        private TestFinancialAidPersonRepository expectedRepository;
        private List<PersonBase> expectedPersons;

        private IFinancialAidPersonRepository actualRepository;
        private IEnumerable<PersonBase> actualPersons;

        private string lastName;
        private List<string> personIds;

        [TestInitialize]
        public void Initialize()
        {
            MockInitialize();
            expectedRepository = new TestFinancialAidPersonRepository();
            actualRepository = BuildFinancialAidPersonRepository();
        }

        private FinancialAidPersonRepository BuildFinancialAidPersonRepository()
        {
            dataReaderMock.Setup<Task<string[]>>(dr => dr.SelectAsync("STUDENTS", It.IsAny<string[]>(), It.IsAny<string>()))
                .Returns<string, string[], string>(
                (fileName, limitingKeys, criteria) => 
                {
                    var matches = expectedRepository.studentData
                         .Where(ap => ap.id == limitingKeys[0]);
                    return Task.FromResult(matches.Any() ? new string[] { matches.First().id } : new string[0]);
                });

            dataReaderMock.Setup<Task<string[]>>(dr => dr.SelectAsync("APPLICANTS", It.IsAny<string[]>(), It.IsAny<string>()))
                .Returns<string, string[], string>(
                (fileName, limitingKeys, criteria) =>
                {
                    var matches = expectedRepository.applicantData
                        .Where(ap => ap.id == limitingKeys[0]);
                    return Task.FromResult(matches.Any() ? new string[] { matches.First().id } : new string[0]);                        
                });

            transManagerMock.Setup(manager => manager
                        .ExecuteAsync<GetPersonLookupStringRequest, GetPersonLookupStringResponse>(It.IsAny<GetPersonLookupStringRequest>()))
                        .Returns<GetPersonLookupStringRequest>(
                        (request) =>
                        {
                            lastName = request.SearchString.Split(',')[0];
                            return Task.FromResult(new GetPersonLookupStringResponse() { IndexString = ";PARTIAL.NAME.INDEX " + lastName.ToUpper() + "_", ErrorMessage = "" });
                        });

            dataReaderMock.Setup(acc => acc.SelectAsync("PERSON", It.IsAny<string>()))
                .Returns<string, string>(
                (fileName, searchString) => {
                    int ndx = searchString.IndexOf("EQ \"");
                    lastName = searchString.Substring(ndx + 4).Replace("_", "").Replace("\"", "");
                    return Task.FromResult(expectedRepository.personData.Where(p => p.lastName.ToLower() == lastName.ToLower()).Select(p => p.id).ToArray());
                });

            dataReaderMock.Setup<Task<Collection<Data.Base.DataContracts.Person>>>(dr => dr.BulkReadRecordAsync<Data.Base.DataContracts.Person>("PERSON", It.IsAny<string[]>(), true))
                .Returns<string, string[], bool>((file, ids, b) =>
                {
                    var contracts = expectedRepository.personData.Where(p => ids.Contains(p.id));
                    var personContracts = new Collection<Data.Base.DataContracts.Person>();
                    foreach (var contract in contracts)
                    {
                        personContracts.Add(new Data.Base.DataContracts.Person()
                        {
                            Recordkey = contract.id,
                            LastName = contract.lastName,
                            FirstName = contract.firstName,
                            PrivacyFlag = contract.privacyCode
                        });
                    }
                    return Task.FromResult(personContracts);
                    
                });

            return new FinancialAidPersonRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object, apiSettings);                        
        }

        [TestMethod]
        public async Task LastName_SearchFinancialAidPersonsAsync_ReturnsExpectedResultTest()
        {
            actualPersons = await actualRepository.SearchFinancialAidPersonsByKeywordAsync(expectedRepository.personData.First().lastName);
            expectedPersons = new List<PersonBase>()
            {
                new PersonBase(expectedRepository.personData.First().id, expectedRepository.personData.First().lastName, expectedRepository.personData.First().privacyCode)
            };
            CollectionAssert.AreEqual(expectedPersons, actualPersons.ToList());
        }

        [TestMethod]
        public async Task LastFirstName_SearchFinancialAidPersonsAsync_ReturnsExpectedResultTest()
        {
            string criteria = expectedRepository.personData.First().lastName + "," + expectedRepository.personData.First().firstName;
            actualPersons = await actualRepository.SearchFinancialAidPersonsByKeywordAsync(criteria);
            expectedPersons = new List<PersonBase>()
            {
                new PersonBase(expectedRepository.personData.First().id, expectedRepository.personData.First().lastName, expectedRepository.personData.First().privacyCode)
            };
            CollectionAssert.AreEqual(expectedPersons, actualPersons.ToList());
        }

        [TestMethod]
        public async Task FirstLastName_SearchFinancialAidPersonsAsync_ReturnsExpectedResultTest()
        {
            string criteria = expectedRepository.personData.First().firstName + " " + expectedRepository.personData.First().lastName;
            actualPersons = await actualRepository.SearchFinancialAidPersonsByKeywordAsync(criteria);
            expectedPersons = new List<PersonBase>()
            {
                new PersonBase(expectedRepository.personData.First().id, expectedRepository.personData.First().lastName, expectedRepository.personData.First().privacyCode)
            };
            CollectionAssert.AreEqual(expectedPersons, actualPersons.ToList());
        }

        [TestMethod]
        [ExpectedException (typeof(ArgumentNullException))]
        public async Task NullKeyword_SearchFinancialAidPersonsByKeywordAsync_ThrowsArgumentNullExceptionTest()
        {
            await actualRepository.SearchFinancialAidPersonsByKeywordAsync(null);
        }

        [TestMethod]
        public async Task IdAsKeyword_SearchFinancialAidPersonsByKeywordAsync_ReturnsExpectedResultTest()
        {
            actualPersons = await actualRepository.SearchFinancialAidPersonsByKeywordAsync(expectedRepository.personData.First().id);
            expectedPersons = new List<PersonBase>()
            {
                new PersonBase(expectedRepository.personData.First().id, expectedRepository.personData.First().lastName, expectedRepository.personData.First().privacyCode)
            };
            CollectionAssert.AreEqual(expectedPersons, actualPersons.ToList());
        }

        [TestMethod]
        [ExpectedException(typeof(ApplicationException))]
        public async Task NotStudentOrApplicant_SearchFinancialAidPersonsByKeywordAsync_ThrowsApplicationExceptionTest()
        {
            await actualRepository.SearchFinancialAidPersonsByKeywordAsync("foo");
        }

        [TestMethod]
        public async Task Ids_SearchFinancialAidPersonsAsync_ReturnsExpectedResultTest()
        {
            actualPersons = await actualRepository.SearchFinancialAidPersonsByIdsAsync(new List<string>(){expectedRepository.personData.First().id});
            expectedPersons = new List<PersonBase>()
            {
                new PersonBase(expectedRepository.personData.First().id, expectedRepository.personData.First().lastName, expectedRepository.personData.First().privacyCode)
            };
            CollectionAssert.AreEqual(expectedPersons, actualPersons.ToList());
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task EmptyIdsList_SearchFinancialAidPersonsByIdsAsync_ThrowsArgumentNullExceptionTest()
        {
            await actualRepository.SearchFinancialAidPersonsByIdsAsync(new List<string>());
        }

        [TestMethod]
        [ExpectedException(typeof(ApplicationException))]
        public async Task NotStudentOrApplicant_SearchFinancialAidPersonsByIdsAsync_ThrowsApplicationExceptionTest()
        {
            personIds = new List<string>() { "foo", "bar" };
            await actualRepository.SearchFinancialAidPersonsByIdsAsync(personIds);
        }

    }
}
