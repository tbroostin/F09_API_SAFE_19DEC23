// Copyright 2018-2020 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Domain.Finance.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ellucian.Colleague.Domain.Base.Entities;
using Ellucian.Colleague.Domain.Finance.Entities;
using Ellucian.Colleague.Domain.Student.Entities.InstantEnrollment;

namespace Ellucian.Colleague.Domain.Finance.Tests
{
    public class TestAccountsReceivableRepository : IAccountsReceivableRepository
    {
        public class PersonRecord
        {
            public string id;
            public string middleName;
            public string lastName;
            public string firstName;
            public string privacyCode;
        }

        public List<PersonRecord> personData = new List<PersonRecord>()
        {
            new PersonRecord()
            {
                id = "0000001",
                middleName = "",
                lastName = "Smith",
                firstName = "Joe",
                privacyCode = "foo"
            },
            new PersonRecord()
            {
                id = "0000002",
                middleName = "Jacob Jingleheimer",
                lastName = "Smith",
                firstName = "John",
                privacyCode = "foobar"
            },
            new PersonRecord()
            {
                id = "0000003",
                middleName = "Lisa",
                lastName = "Smithson",
                firstName = "Jane",
                privacyCode = "bar"
            },
            new PersonRecord()
            {
                id = "0002345",
                middleName = "Lisa",
                lastName = "O'Hruska",
                firstName = "Nadia",
                privacyCode = "ALL"
            },
            new PersonRecord()
            {
                id = "123456789",
                middleName = "Jane",
                lastName = "Malley",
                firstName = "Henrietta",
                privacyCode = ""
            },
            new PersonRecord()
            {
                id = "AB12345",
                middleName = "Ira",
                lastName = "Blodgett",
                firstName = "Lydia",
                privacyCode = ""
            }
        };

        public IEnumerable<ChargeCode> ChargeCodes
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public IEnumerable<DepositType> DepositTypes
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public Dictionary<string, string> EthosExtendedDataDictionary
        {
            get
            {
                throw new NotImplementedException();
            }

            set
            {
                throw new NotImplementedException();
            }
        }

        public IEnumerable<ExternalSystem> ExternalSystems
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public IEnumerable<InvoiceType> InvoiceTypes
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public IEnumerable<ReceivableType> ReceivableTypes
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public Task<PersonIntegration> Create2Async(PersonIntegration person, IEnumerable<Address> addresses, IEnumerable<Phone> phones, int version = 1)
        {
            throw new NotImplementedException();
        }

        public Task<PersonIntegration> CreateOrganizationAsync(PersonIntegration personOrg, IEnumerable<Address> addresses, IEnumerable<Phone> phones)
        {
            throw new NotImplementedException();
        }

        public Task<List<string>> Get1098HierarchyAddressAsync(string id)
        {
            throw new NotImplementedException();
        }

        public Task<Dictionary<string, string>> GetHierarchyAddressIdsAsync(List<string> ids, string hierarchy, DateTime? date)
        {
            throw new NotImplementedException();
        }
        
        public Task<Dictionary<string, string>> GetAddressGuidsCollectionAsync(IEnumerable<string> addressIds)
        {
            throw new NotImplementedException();
        }

        public AccountHolder GetAccountHolder(string personId)
        {
            throw new NotImplementedException();
        }

        public Task<string> GetAddressIdFromGuidAsync(string guid)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<ChargeCode>> GetChargeCodesAsync()
        {
            throw new NotImplementedException();
        }

        public Deposit GetDeposit(string id)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<Deposit> GetDeposits(IEnumerable<string> ids)
        {
            throw new NotImplementedException();
        }

        public List<DepositDue> GetDepositsDue(string personId)
        {
            throw new NotImplementedException();
        }

        public string GetDistribution(string studentId, string accountType, string callingProcess)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<string> GetDistributions(string studentId, IEnumerable<string> accountTypes, string paymentProcess)
        {
            throw new NotImplementedException();
        }

        public Task<EmailAddress> GetEmailAddressFromHierarchyAsync(string personId, string emailHierarchy)
        {
            throw new NotImplementedException();
        }

        public Tuple<List<string>, List<string>> GetEthosExtendedDataLists()
        {
            throw new NotImplementedException();
        }

        public Task<Tuple<IEnumerable<string>, int>> GetFacultyPersonGuidsAsync(int offset, int limit)
        {
            throw new NotImplementedException();
        }

        public Task<Tuple<IEnumerable<string>, int>> GetFilteredOrganizationGuidsAsync(int offset, int limit, string role, string credentialType, string credentialValue)
        {
            throw new NotImplementedException();
        }

        public Task<Tuple<IEnumerable<string>, int>> GetFilteredPerson2GuidsAsync(int offset, int limit, bool bypassCache, PersonFilterCriteria personFilterCriteria, string personFilter)
        {
            throw new NotImplementedException();
        }

        public Task<string> GetHostCountryAsync()
        {
            throw new NotImplementedException();
        }

        public Invoice GetInvoice(string id)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<Invoice> GetInvoices(IEnumerable<string> ids)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<PersonMatchResult>> GetMatchingPersonResultsAsync(PersonMatchCriteria criteria)
        {
            throw new NotImplementedException();
        }
        public Task<IEnumerable<PersonMatchResult>> GetMatchingPersonResultsInstantEnrollmentAsync(PersonMatchCriteriaInstantEnrollment criteria)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<string>> GetMatchingPersonsAsync(Person person)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<string>> GetMatchingPersons2Async(Person person)
        {
            throw new NotImplementedException();
        }

        public ReceivablePayment GetPayment(string id)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<ReceivablePayment> GetPayments(IEnumerable<string> ids)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<Person>> GetPersonByGuidNonCachedAsync(IEnumerable<string> guids)
        {
            throw new NotImplementedException();
        }

        public Task<Person> GetPersonByGuidNonCachedAsync(string guid)
        {
            throw new NotImplementedException();
        }

        public Task<Person> GetPersonCredentialByGuidNonCachedAsync(string guid)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<PersonIntegration>> GetPersonCredentialsIntegrationByGuidNonCachedAsync(IEnumerable<string> guids)
        {
            throw new NotImplementedException();
        }

        public Task<string> GetPersonGuidFromIdAsync(string personId)
        {
            throw new NotImplementedException();
        }

        public Task<Tuple<IEnumerable<string>, int>> GetPersonGuidsAsync(int offset, int limit, bool bypassCache)
        {
            throw new NotImplementedException();
        }

        public Task<Dictionary<string, string>> GetPersonGuidsCollectionAsync(IEnumerable<string> personIds)
        {
            throw new NotImplementedException();
        }

        public Task<Tuple<IEnumerable<string>, int>> GetPersonGuidsFilteredAsync(int offset, int limit, Dictionary<string, string> credentials, bool bypassCache)
        {
            throw new NotImplementedException();
        }

        public Task<Dictionary<string, KeyValuePair<string, string>>> GetPersonGuidsFromOperKeysAsync(IEnumerable<string> operKeys)
        {
            throw new NotImplementedException();
        }

        public Task<Dictionary<string, string>> GetPersonGuidsWithNoCorpCollectionAsync(IEnumerable<string> personIds)
        {
            throw new NotImplementedException();
        }

        public Task<string> GetPersonIdFromGuidAsync(string guid)
        {
            throw new NotImplementedException();
        }

        public Task<PersonIntegration> GetPersonNamesAndCredsByGuidAsync(string guid)
        {
            throw new NotImplementedException();
        }

        public Task<PersonIntegration> GetPersonIntegration2ByGuidAsync(string guid, bool bypassCache)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<PersonIntegration>> GetPersonNamesAndCredsByGuidAsync(IEnumerable<string> guids)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<PersonIntegration>> GetPersonIntegration2ByGuidNonCachedAsync(IEnumerable<string> guids)
        {
            throw new NotImplementedException();
        }

        public Task<PersonIntegration> GetPersonIntegrationByGuidAsync(string guid, bool bypassCache)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<PersonIntegration>> GetPersonIntegrationByGuidNonCachedAsync(IEnumerable<string> guids)
        {
            throw new NotImplementedException();
        }

        public Task<PersonIntegration> GetPersonIntegrationByGuidNonCachedAsync(string guid)
        {
            throw new NotImplementedException();
        }

        public Task<Tuple<List<EmailAddress>, List<Phone>, List<Address>, List<SocialMedia>, bool>> GetPersonIntegrationData2Async(string personId)
        {
            throw new NotImplementedException();
        }

        public Task<Tuple<List<EmailAddress>, List<Phone>, List<Address>, bool>> GetPersonIntegrationDataAsync(string personId)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<PersonPin>> GetPersonPinsAsync(string[] personGuids)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<PersonUserName>> GetPersonUserNamesAsync(string[] personIds)
        {
            throw new NotImplementedException();
        }

        public ReceivableInvoice GetReceivableInvoice(string id)
        {
            throw new NotImplementedException();
        }

        public Task<bool> IsAdvisorAsync(string personId)
        {
            throw new NotImplementedException();
        }

        public Task<bool> IsCorpAsync(string personId)
        {
            throw new NotImplementedException();
        }

        public Task<bool> IsFacultyAsync(string personId)
        {
            throw new NotImplementedException();
        }

        public bool IsPerson(string personId)
        {
            throw new NotImplementedException();
        }

        public Task<bool> IsPersonAsync(string personId)
        {
            throw new NotImplementedException();
        }

        public Task<bool> IsStudentAsync(string personId)
        {
            throw new NotImplementedException();
        }

        public ReceivableInvoice PostReceivableInvoice(ReceivableInvoice source)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<InvoicePayment>> QueryInvoicePaymentsAsync(IEnumerable<string> invoiceIds, InvoiceDataSubset invoiceDataSubsetType)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<AccountHolder>> SearchAccountHoldersByIdsAsync(IEnumerable<string> ids)
        {
            List<AccountHolder> persons = new List<AccountHolder>();
            foreach (var id in ids)
            {
                if (personData.Any(p => p.id == id))
                {
                    var match = personData.Where(p => p.id == id).First();
                    persons.Add(new AccountHolder(match.id, match.lastName, match.privacyCode) { MiddleName = match.middleName, FirstName = match.firstName });
                }
            }
            return Task.FromResult(persons.AsEnumerable());
        }

        public Task<IEnumerable<AccountHolder>> SearchAccountHoldersByKeywordAsync(string criteria)
        {
            List<AccountHolder> persons = new List<AccountHolder>();
            if (personData.Any(p => p.lastName == criteria || p.id == criteria))
            {
                var matches = personData.Where(p => p.lastName == criteria || p.id == criteria);
                foreach (var match in matches)
                {
                    persons.Add(new AccountHolder(match.id, match.lastName, match.privacyCode) { MiddleName = match.middleName, FirstName = match.firstName });
                }
            }
            return Task.FromResult(persons.AsEnumerable());
        }

        public Task<PersonIntegration> Update2Async(PersonIntegration person, IEnumerable<Address> addresses, IEnumerable<Phone> phones, int version = 1)
        {
            throw new NotImplementedException();
        }

        public Task<PersonIntegration> UpdateOrganizationAsync(PersonIntegration person, IEnumerable<Address> addresses, IEnumerable<Phone> phones)
        {
            throw new NotImplementedException();
        }

        public Task<PersonIntegration> GetPersonIntegration3ByGuidAsync(string guid, bool bypassCache)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<PersonIntegration>> GetPersonIntegration3ByGuidNonCachedAsync(IEnumerable<string> guids)
        {
            throw new NotImplementedException();
        }

        public Task<Tuple<IEnumerable<string>, int>> GetFilteredPerson3GuidsAsync(int offset, int limit, bool bypassCache, PersonFilterCriteria personFilterCriteria, string personFilter)
        {
            throw new NotImplementedException();
        }

        public Task<string> GetPersonIdForNonCorpOnly(string personGuid)
        {
            throw new NotImplementedException();
        }
    }
}
