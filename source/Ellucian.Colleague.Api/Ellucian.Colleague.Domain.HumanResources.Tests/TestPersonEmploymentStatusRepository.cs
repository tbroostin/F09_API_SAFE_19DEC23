/* Copyright 2016-2020 Ellucian Company L.P. and its affiliates. */
using Ellucian.Colleague.Domain.HumanResources.Entities;
using Ellucian.Colleague.Domain.HumanResources.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Domain.HumanResources.Tests
{
    public class TestPersonEmploymentStatusRepository : IPersonEmploymentStatusRepository
    {
        public class PersonEmploymentStatusRecord
        {
            public string id;
            public string personId;
            public string primaryPositionId;
            public string personPositionId;
            public DateTime? startDate;            
            public DateTime? endDate;
        }

        public List<PersonEmploymentStatusRecord> personEmploymentStatusRecords = new List<PersonEmploymentStatusRecord>()
        {
            new PersonEmploymentStatusRecord() 
            {
                id = "001",
                personId = "24601",
                primaryPositionId = "PRISONER321",
                personPositionId = "123",
                startDate = new DateTime(1805,1,2),           
                endDate = new DateTime(1825,1,2)
            },
            new PersonEmploymentStatusRecord() 
            {
                id = "002",
                personId = "24601",
                primaryPositionId = "MANUFACTURER123",
                personPositionId = "124",
                startDate = new DateTime(1825,1,3),           
                endDate = new DateTime(1830,1,2)
            },
            new PersonEmploymentStatusRecord() 
            {
                id = "003",
                personId = "24601",
                primaryPositionId = "MAYOR999",
                personPositionId = "125",
                startDate = new DateTime(1830,1,3),           
                endDate = null
            },
            new PersonEmploymentStatusRecord() 
            {
                id = "004",
                personId = "0012185",
                primaryPositionId = "INSPECTOR",
                personPositionId = "121",
                startDate = new DateTime(1805,1,2),           
                endDate = null
            },
            new PersonEmploymentStatusRecord()
            {
                id = "005",
                personId = "0012185",
                primaryPositionId = "INSPECTOR",
                personPositionId = "121",
                startDate = new DateTime(1805,1,2),
                endDate = null
            },
             new PersonEmploymentStatusRecord()
            {
                id = "006",
                personId = "0003916",
                primaryPositionId = "PRINT SHOP ASSISTANT",
                personPositionId = "420",
                startDate = new DateTime(2000,1,2),
                endDate = null
            },
             new PersonEmploymentStatusRecord()
            {
                id = "007",
                personId = "0003914",
                primaryPositionId = "PRINT SHOP ASSISTANT",
                personPositionId = "425",
                startDate = new DateTime(2000,1,2),
                endDate = null
            },
        };


        public IEnumerable<PersonEmploymentStatus> GetPersonEmploymentStatuses(IEnumerable<string> personIds)
        {
            var records = personEmploymentStatusRecords.Where(p => personIds.Contains(p.personId));

            var entities = new List<PersonEmploymentStatus>();
            foreach (var record in records)
            {
                if (record != null)
                {
                    try
                    {
                        entities.Add(BuildPersonEmploymentStatus(record));
                    }
                    catch (Exception) { }
                }
            }

            return entities;
        }

        public async Task<IEnumerable<PersonEmploymentStatus>> GetPersonEmploymentStatusesAsync(IEnumerable<string> personIds, DateTime? startDate = null)
        {
            return await Task.FromResult(GetPersonEmploymentStatuses(personIds));
        }

        public IEnumerable<string> personIdsUsedInTestData
        {
            get
            {
                return personEmploymentStatusRecords.Select(rec => rec.personId).Distinct();
            }
        }

        public PersonEmploymentStatus BuildPersonEmploymentStatus(PersonEmploymentStatusRecord record)
        {
            if (record == null)
            {
                throw new ArgumentNullException("record");
            }
            if (!record.startDate.HasValue)
            {
                throw new InvalidOperationException("startDate must have value");
            }
            return new PersonEmploymentStatus(record.id, record.personId, record.primaryPositionId, record.personPositionId, record.startDate, record.endDate);
        }

       
    }
}
