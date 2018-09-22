/*Copyright 2017 Ellucian Company L.P. and its affiliates.*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ellucian.Colleague.Domain.FinancialAid.Repositories;
using Ellucian.Colleague.Domain.FinancialAid.Entities;
using Ellucian.Colleague.Domain.Base.Entities;

namespace Ellucian.Colleague.Domain.FinancialAid.Tests
{
    public class TestFinancialAidPersonRepository : IFinancialAidPersonRepository
    {

        #region PersonData
        public class PersonRecord
        {
            public string id;
            public string preferredName;
            public string lastName;
            public string firstName;
            public string privacyCode;
        }

        public List<PersonRecord> personData = new List<PersonRecord>()
        {
            new PersonRecord()
            {
                id = "0006758",
                preferredName = "John Doe",
                lastName = "Doe",
                firstName = "John",
                privacyCode = "foo"
            },
            new PersonRecord()
            {
                id = "1234567",
                preferredName = "Marilyn Cane",
                lastName = "Cane",
                firstName = "Marilyn"
            },
            new PersonRecord()
            {
                id = "9876543",
                preferredName = "Grace Huntington",
                lastName = "Huntington",
                firstName = "Grace",
                privacyCode = "bar"
            }
        };
        #endregion

        #region ApplicantData
        public class Applicant
        {
            public string id;
        }

        public List<Applicant> applicantData = new List<Applicant>()
        {
            new Applicant()
            {
                id = "0006758"
            },
            new Applicant()
            {
                id = "0000000"
            }
        };

        #endregion

        #region StudentData
        public class Student
        {
            public string id;
        }

        public List<Student> studentData = new List<Student>()
        {
            new Student()
            {
                id = "1234567"
            },
            new Student()
            {
                id = "6574839"
            }
        };
        #endregion

        public Task<IEnumerable<PersonBase>> SearchFinancialAidPersonsByKeywordAsync(string criteria)
        {
            List<PersonBase> persons = new List<PersonBase>();
            if (personData.Any(p => p.lastName == criteria || p.id == criteria || p.preferredName == criteria))
            {
                var matches = personData.Where(p => p.lastName == criteria || p.id == criteria || p.preferredName == criteria);
                foreach (var match in matches)
                {
                    persons.Add(new PersonBase(match.id, match.lastName, match.privacyCode) { PreferredName = match.preferredName, FirstName = match.firstName });
                }
            }
            return Task.FromResult(persons.AsEnumerable());
        }

        public Task<IEnumerable<PersonBase>> SearchFinancialAidPersonsByIdsAsync(IEnumerable<string> criteria)
        {
            List<PersonBase> persons = new List<PersonBase>();
            foreach (var id in criteria)
            {
                if (personData.Any(p => p.id == id))
                {
                    var match = personData.Where(p =>p.id == id).First();
                    persons.Add(new PersonBase(match.id, match.lastName, match.privacyCode) { PreferredName = match.preferredName, FirstName = match.firstName });
                }
            }
            return Task.FromResult(persons.AsEnumerable());
        }
    }
}
