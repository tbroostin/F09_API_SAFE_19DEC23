//Copyright 2015-2019 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.Base.Entities;
using System.Collections.ObjectModel;

namespace Ellucian.Colleague.Domain.Base.Tests
{
    public class TestStaffRepository : IStaffRepository
    {
        public class StaffRecord
        {
            public string recordKey;
            public string type;
            public string status;
            public string initials;
        }

        public class PersonRecord
        {
            public string recordKey;
            public string firstName;
            public string lastName;
        }

        public List<StaffRecord> staffData = new List<StaffRecord>()
        {
            new StaffRecord(){
                recordKey = "0000001",
                type = "S",
                status = "C",
                initials = "STAFFA"
                
            },
            new StaffRecord(){
                recordKey = "0000002",
                type = "S",
                status = "F",
                initials = "STAFFB"
            },
            new StaffRecord(){
                recordKey = "0000003",
                type = "V",
                status = "C",
                initials = "STAFFC"
            }
        };

        public List<PersonRecord> personData = new List<PersonRecord>()
        {
            new PersonRecord(){
                recordKey = "0000001",
                firstName = "Sam",
                lastName = "Smith"
            },
            new PersonRecord(){
                recordKey = "0000002",
                firstName = "Jane",
                lastName = "Kawalski"
            },
            new PersonRecord(){
                recordKey = "0000003",
                firstName = "Nick",
                lastName = "Doe"
            }
        };

        public Staff Get(string id)
        {
            return Get(new List<string>() { id }).First();
        }

        public async Task<Staff> GetAsync(string id)
        {
            var staff = await Task.Run(() => Get(new List<string>() { id }).First());
            return staff;
        }


        public async Task<IEnumerable<Staff>> GetAsync(IEnumerable<string> ids)
        {
            var staff = await Task.Run(() => Get(ids));
            return staff;
        }

        public IEnumerable<Staff> Get(IEnumerable<string> ids)
        {
            List<Staff> staffEntities = new List<Staff>();

            var staffDataContracts = staffData.Where(s => ids.Any(id => id == s.recordKey));
            if (staffData != null && staffData.Any())
            {
                foreach (var id in ids)
                {
                    var staffDataContact = staffData.FirstOrDefault(sd => sd.recordKey == id);
                    var person = personData.First(p => p.recordKey == id);

                    Staff staff = new Staff(person.recordKey, person.lastName);

                    staffEntities.Add(staff);
                }
            }

            return staffEntities;
        }

        public async Task<string> GetStaffLoginIdForPersonAsync(string personId)
        {
            return "AJK";
        }

    }
}
